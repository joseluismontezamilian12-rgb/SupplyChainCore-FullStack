# 📦 SupplyChainCore — Full-Stack Supply Chain Management System

[![CI](https://github.com/joseluismontezamilian12-rgb/SupplyChainCore-FullStack/actions/workflows/ci.yml/badge.svg)](https://github.com/joseluismontezamilian12-rgb/SupplyChainCore-FullStack/actions/workflows/ci.yml)

Transactional inventory system built around an **immutable logistics ledger**: every stock movement (inbound, outbound, shrinkage) is validated in real time against the historical balance, and any operation that would produce a negative stock is rejected **before it persists**.

Full-stack solution: a decoupled .NET 10 backend and a React frontend.

---

## 🏛️ Architecture (Clean Architecture)

The backend follows **Clean Architecture** and the Single Responsibility Principle, split into fully decoupled layers that are independent of the database engine and the UI:

- **`SupplyChainCore.Domain`** — pure business entities (`Product`, `InventoryMovement`, `Warehouse`, `User`, `Role`) with zero framework dependencies.
- **`SupplyChainCore.Application`** — persistence contracts (interfaces), use cases, and the services where business rules execute.
- **`SupplyChainCore.Infrastructure`** — data access with **Entity Framework Core**: Repository Pattern, Fluent API mapping, and automated data seeding.
- **`SupplyChainCore.WebApi`** — RESTful endpoints exchanging **C# records as DTOs**.
- **`SupplyChainCore.UnitTests`** — xUnit test suite isolating application logic with **Moq** test doubles.
- **`supply-chain-ui`** — React + Vite frontend consuming the API.

---

## 🛠️ Tech Stack

| Component | Technology | Purpose |
| :-- | :-- | :-- |
| Backend | .NET 10 / C# | Core server framework |
| Persistence | Entity Framework Core | Object-relational mapping |
| Database | Microsoft SQL Server | Relational storage |
| Testing | xUnit / Moq | Unit testing & dependency mocking |
| Frontend | React / Vite | UI library & build tooling |
| Security | JWT · RBAC · CORS | Authentication, authorization & perimeter policies |

---

## 🧠 Key Business Rule

> 🛡️ **Positive-stock guarantee:** the system runs on a strict transactional ledger. When an `OUTBOUND` or `SHRINKAGE` movement is requested, the Application layer computes the historical balance for that warehouse on the fly. If the requested quantity exceeds the available stock, **the transaction aborts in memory with a controlled exception** — no negative balances, no corrupted data.

---

## 🔐 Authentication & Authorization

Every endpoint except `POST /api/auth/login` requires a **JWT bearer token**.

| Endpoint | Method | Access |
| :-- | :-- | :-- |
| `/api/auth/login` | POST | Anonymous |
| `/api/auth/me` | GET | Any authenticated user |
| `/api/movimientos/stock/{producto}/{almacen}` | GET | Any authenticated user |
| `/api/movimientos/historial/{producto}/{almacen}` | GET | Any authenticated user |
| `/api/movimientos` | POST | **`Admin` role only** |
| `/api/analytics/*` | GET | Any authenticated user |

Design decisions worth calling out:

- **Passwords are stored as PBKDF2-HMAC-SHA256** (100,000 iterations, a random 16-byte salt per user), compared in constant time with `CryptographicOperations.FixedTimeEquals`. The iteration count is embedded in the stored value (`{iterations}.{salt}.{hash}`) so the work factor can be raised later without invalidating existing passwords.
- **The ledger's authorship comes from the token, never from the request body.** `POST /api/movimientos` reads the user id from the `NameIdentifier` claim; if the client could choose it, anyone could sign a movement in someone else's name and the ledger would stop being a trustworthy audit trail.
- **A failed login is indistinguishable from an unknown email** — same response, and the same cryptographic work is performed either way, so response timing does not reveal which accounts exist.
- `ClockSkew` is set to `TimeSpan.Zero`; the .NET default silently accepts tokens for 5 minutes past expiry.

### Demo accounts (seeded)

| Email | Password | Role |
| :-- | :-- | :-- |
| `jose@supplychain.com` | `Admin123!` | `Admin` — can write to the ledger |
| `operador@supplychain.com` | `Operador123!` | `Operador` — read-only |

### Configuring the signing key

`appsettings.json` ships with an empty `Jwt:Key` and **the API refuses to start without one**. For local development the key lives in `appsettings.Development.json`; anywhere else, supply it as an environment variable:

```bash
Jwt__Key="a-key-of-at-least-32-bytes"   # HS256 rejects anything shorter
```

---

## 🚢 Deployment

See **[DEPLOY.md](DEPLOY.md)** for required environment variables, the hosting trade-off (Azure SQL vs. swapping to PostgreSQL) and a post-deploy smoke test.

---

## 🐳 Running with Docker

Brings up the API and its SQL Server instance together, applying migrations on startup:

```bash
docker compose up --build
# API on http://localhost:8080 — OpenAPI document at /openapi/v1.json
```

---

## 🚀 Getting Started

### Prerequisites

- [.NET SDK 10.0+](https://dotnet.microsoft.com/)
- [Node.js 18+](https://nodejs.org/)
- SQL Server / LocalDB

### Backend

Configure your connection string in `SupplyChainCore.WebApi/appsettings.json`, then:

```powershell
dotnet restore
dotnet ef database update --project SupplyChainCore.Infrastructure --startup-project SupplyChainCore.WebApi
dotnet run --project SupplyChainCore.WebApi   # serves on https://localhost:7047
```

### Frontend

```bash
cd supply-chain-ui
npm install
npm run dev
```

The API base URL defaults to `https://localhost:7047`. Point it elsewhere with a `.env` file:

```bash
VITE_API_BASE_URL=http://localhost:8080
```

### Tests

```bash
dotnet test    # 57 unit tests: ledger rules, password hashing, JWT issuance, login flow
```

---

## 📄 License

MIT — see [LICENSE](LICENSE).
