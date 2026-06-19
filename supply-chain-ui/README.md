# 📦 SupplyChainCore — Ledger Logístico & Enterprise Analytics Dashboard

¡Bienvenido a **SupplyChainCore**! Una plataforma de software empresarial full-stack diseñada bajo el paradigma de **Clean Architecture** (Arquitectura Limpia). El sistema fusiona un motor transaccional de alta consistencia para el registro de inventarios (**OLTP**) con un potente panel de inteligencia de negocios y analítica gráfica en tiempo real (**OLAP**).

---

## 🚀 Arquitectura del Ecosistema

El proyecto está dividido estrictamente siguiendo los principios de separación de responsabilidades y diseño guiado por el dominio (DDD):

```text
SupplyChainCore (Solución Backend .NET 8)
│
├── 🏛️ SupplyChainCore.Domain          # Entidades puras, Enums y Reglas críticas de Negocio.
├── ⚙️ SupplyChainCore.Application     # Casos de uso, Interfaces de Servicios y DTOs Analíticos.
├── 💾 SupplyChainCore.Infrastructure  # Persistencia (EF Core, SQL Server), Repositorios y AnalyticsService.
└── 🌐 SupplyChainCore.WebApi          # Controladores REST API, Inyección de Dependencias y Políticas CORS.

supply-chain-ui (Frontend React)
│
└── 🎨 Componentes React + Recharts para renderizado gráfico y gestión transaccional.