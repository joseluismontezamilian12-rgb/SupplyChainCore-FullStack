# Deployment

The API is packaged as a container (see [`Dockerfile`](Dockerfile)). What follows is what the running instance needs, and the trade-off between the two realistic hosting paths.

## Required configuration

The API **refuses to start without a signing key** — that is deliberate, so a deployment can never silently run with a default secret. Every setting below is read from environment variables (the `__` separator maps to nested JSON keys):

| Variable | Required | Notes |
| :-- | :--: | :-- |
| `Jwt__Key` | ✅ | **Minimum 32 bytes** — HS256 rejects anything shorter and the app throws at startup. |
| `ConnectionStrings__DefaultConnection` | ✅ | SQL Server connection string. |
| `Jwt__Issuer` | | Defaults to `SupplyChainCore.WebApi`. |
| `Jwt__Audience` | | Defaults to `SupplyChainCore.Client`. |
| `Jwt__ExpiraEnMinutos` | | Defaults to `60`. |
| `Database__AutoMigrate` | | `true` applies pending migrations at startup. Convenient for a demo; for anything serious run migrations as an explicit deployment step instead. |
| `Cors__OrigenesPermitidos__0` | | Origin of the deployed frontend. Add `__1`, `__2`… for more. |
| `ASPNETCORE_URLS` | | Already set to `http://+:8080` in the image. |

Generate a key that actually has entropy — do not type one by hand:

```bash
openssl rand -base64 48
```

> ⚠️ `appsettings.Development.json` ships a **development-only** key on purpose, so the repo can be cloned and run without setup. It must never be the key of a deployed instance.

## Choosing a host

The deciding factor is that this project persists to **SQL Server**, which is heavy: the official container wants ~2 GB of RAM, more than most free compute tiers allow.

**Azure App Service + Azure SQL Database — recommended.** Azure SQL has a free serverless tier, and it is the only path that needs **no code change**, because the EF Core provider stays `UseSqlServer`. The container runs on App Service; the database is managed separately, so nothing has to fit SQL Server into the app's memory budget.

**Fly.io / Render + PostgreSQL.** Cheaper compute, but neither offers managed SQL Server, so this path means swapping the provider to `Npgsql.EntityFrameworkCore.PostgreSQL` and regenerating the migrations. Worth it only if the target is Postgres anyway.

Running SQL Server in a container *next to* the API on a free tier is not a real option — it will be evicted for exceeding memory.

## Publishing the OpenAPI document

`MapOpenApi()` is currently gated to the Development environment. To expose a public document at `/openapi/v1.json` on the deployed instance, either set `ASPNETCORE_ENVIRONMENT=Development` (acceptable for a portfolio demo, but it also loosens error-page behaviour) or move the `MapOpenApi()` call outside the `IsDevelopment()` check in [`Program.cs`](SupplyChainCore.WebApi/Program.cs) — the deliberate choice being that a public API document is a feature, not a debugging aid.

## Frontend

The React app reads its API base URL at build time:

```bash
VITE_API_BASE_URL=https://<your-api-host> npm run build
```

Remember to add that frontend's origin to `Cors__OrigenesPermitidos__0` on the API, or the browser will block every request.

## Smoke test after deploying

```bash
# 1. Log in — should return a token
curl -s -X POST https://<host>/api/auth/login \
  -H 'Content-Type: application/json' \
  -d '{"email":"jose@supplychain.com","password":"Admin123!"}'

# 2. Without a token — must be 401
curl -s -o /dev/null -w '%{http_code}\n' https://<host>/api/analytics/kpis

# 3. As Operador, writing to the ledger — must be 403
TOKEN=$(curl -s -X POST https://<host>/api/auth/login \
  -H 'Content-Type: application/json' \
  -d '{"email":"operador@supplychain.com","password":"Operador123!"}' | jq -r .token)
curl -s -o /dev/null -w '%{http_code}\n' -X POST https://<host>/api/movimientos \
  -H "Authorization: Bearer $TOKEN" -H 'Content-Type: application/json' \
  -d '{"productoId":1,"almacenId":1,"cantidad":1,"tipoMovimiento":"INGRESO","motivo":"smoke test"}'
```

Getting `401` and `403` on steps 2 and 3 is the point: it proves authentication and role enforcement survived the deployment.

> **Change the seeded demo passwords** before pointing anyone at a public instance — they are published in this repository.
