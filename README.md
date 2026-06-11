# 📦 SupplyChainCore - Sistema de Gestión de Cadena de Suministro (Full-Stack)

¡Bienvenido a **SupplyChainCore**! Este es un ecosistema transaccional empresarial diseñado bajo los estándares más exigentes de la industria de desarrollo de software. El sistema implementa un **Ledger Logístico inmutable** para registrar ingresos, salidas y mermas de inventario en tiempo real, garantizando la integridad de los datos mediante reglas de negocio estrictas.

Desarrollado como una solución de pila completa (**Full-Stack**), este repositorio integra un backend desacoplado de alta eficiencia y un frontend interactivo moderno.

---

## 🏛️ Arquitectura del Sistema (Clean Architecture)

El backend está estructurado siguiendo los principios de **Arquitectura Limpia (Clean Architecture)** y el **Principio de Responsabilidad Única (SRP)**, dividiéndose en capas totalmente desacopladas independientes del motor de base de datos o de la interfaz de usuario:

* **`SupplyChainCore.Domain` (Núcleo):** Contiene las entidades puras del negocio (`Producto`, `MovimientoInventario`, `Almacen`, `Usuario`, `Rol`) libres de cualquier dependencia de frameworks externos.
* **`SupplyChainCore.Application` (Cerebro):** Define los contratos (interfaces) de persistencia y centraliza los casos de uso y servicios lógicos donde se ejecutan las reglas de negocio.
* **`SupplyChainCore.Infrastructure` (Músculo):** Implementa el acceso a datos mediante **Entity Framework Core**, patrones de diseño (Repository Pattern), Fluent API para tipado estricto y el mecanismo de *Data Seeding* automatizado.
* **`SupplyChainCore.WebApi` (Aduana Perimetral):** Expone los endpoints mediante controladores RESTful e interactúa con el cliente utilizando objetos de transferencia de datos modernos (**C# Records como DTOs**).
* **`SupplyChainCore.UnitTests` (Calidad):** Capa de pruebas automatizadas que aísla la lógica de aplicación utilizando dobles de prueba (**Mocks**).
* **`supply-chain-ui` (Frontend):** Interfaz gráfica de usuario reactiva que consume de forma segura los servicios del backend.

---

## 🛠️ Stack Tecnológico Utilizado

| Componente | Tecnología / Librería | Propósito |
| :--- | :--- | :--- |
| **Backend** | .NET Core 9.0 / C# | Framework principal del servidor de alta velocidad. |
| **Persistencia** | Entity Framework Core | ORM para el mapeo relacional de objetos. |
| **Base de Datos** | Microsoft SQL Server | Motor de base de datos relacional para almacenamiento físico. |
| **Pruebas** | xUnit / Moq | Framework de testing y clonación de dependencias en memoria. |
| **Frontend** | React / Vite | Librería de interfaz de usuario y empaquetador de alto rendimiento. |
| **Seguridad** | CORS / SSL | Políticas perimetrales para encriptación e intercambio seguro. |

---

## 🧠 Reglas de Negocio Clave Implementadas

> 🛡️ **Garantía de Stock Positivo:** El sistema opera bajo un Ledger transaccional estricto. Cuando se intenta registrar una `SALIDA` o una `MERMA`, la capa de `Application` calcula dinámicamente el saldo histórico en ese almacén. Si la cantidad solicitada supera el stock disponible, **la transacción se aborta inmediatamente en memoria y lanza una excepción controlada**, impidiendo saldos negativos o corrupción de datos.

---

## 🚀 Guía de Ejecución Local

### Prerrequisitos
* [.NET SDK 9.0+](https://dotnet.microsoft.com/)
* [Node.js (v18+)](https://nodejs.org/)
* SQL Server Local / LocalDB

### 1. Configuración del Backend (.NET)
Asegúrate de configurar tu cadena de conexión en el archivo `appsettings.json` dentro de `SupplyChainCore.WebApi`. Luego, abre una terminal en la raíz del proyecto y ejecuta:

```powershell
# Restaurar dependencias de la solución
dotnet restore

# Aplicar migraciones e inyectar datos semilla (Data Seeding)
dotnet ef database update --project SupplyChainCore.Infrastructure --startup-project SupplyChainCore.WebApi

# Encender el servidor backend (Puerto seguro asignado: 7047)
dotnet run --project SupplyChainCore.WebApi