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

### Tests

```bash
dotnet test
```

---

## 📄 License

MIT — see [LICENSE](LICENSE).
