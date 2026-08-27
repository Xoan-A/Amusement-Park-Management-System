# Virtual Theme Park Management System

Full-stack management system for a virtual amusement park, enabling administrators, operators, and visitors to manage attractions, events, tickets, rewards, maintenance, incidents, and a configurable scoring system with plugin-based extensibility.

## CI Status

### Main
![Build - Test](https://github.com/IngSoft-DA2/Alvarez-Atrio-Viera/actions/workflows/build-test.yml/badge.svg?branch=main&event=push)
![Code Analysis](https://github.com/IngSoft-DA2/Alvarez-Atrio-Viera/actions/workflows/code-analysis.yml/badge.svg?branch=main&event=push)

### Develop
![Build - Test](https://github.com/IngSoft-DA2/Alvarez-Atrio-Viera/actions/workflows/build-test.yml/badge.svg?branch=develop&event=push)
![Code Analysis](https://github.com/IngSoft-DA2/Alvarez-Atrio-Viera/actions/workflows/code-analysis.yml/badge.svg?branch=develop&event=push)

## Tech Stack

| Layer | Technology |
|---|---|
| **Backend** | C# / .NET 8.0, ASP.NET Core Web API |
| **Frontend** | Angular 19, TypeScript, Bootstrap 5, Chart.js |
| **Database** | SQL Server 2017 (Docker) |
| **ORM** | Entity Framework Core 8 |
| **Auth** | JWT + BCrypt |
| **Containerization** | Docker & Docker Compose |
| **CI/CD** | GitHub Actions |

## Features

- **Authentication & Authorization** -- JWT-based login with three roles: Administrator, Operator, Visitor
- **Attraction Management** -- CRUD operations with capacity tracking and incident reporting
- **Event Management** -- Create events linked to attractions with special ticket pricing
- **Ticket System** -- Purchase tickets with QR code generation
- **Park Entry/Exit Tracking** -- Operators register visitor entries and exits per attraction
- **Incident Management** -- Report incidents on attractions, automatically disabling them
- **Maintenance Scheduling** -- Schedule, track, and complete maintenance tasks with overdue detection
- **Scoring System** -- Configurable scoring via Strategy Pattern (PerAttraction, Combo, PerEvent, plugins)
- **Plugin System** -- Runtime DLL loading for custom scoring strategies
- **Rewards & Redemption** -- Browse and redeem rewards using accumulated points
- **Score History** -- Full audit trail of point changes with filtering
- **Simulated Clock** -- Independent date/time for testing time-dependent features
- **Dashboard** -- Role-specific dashboards with charts and analytics

## Screenshots

![Administrator Dashboard](docs/screenshots/Admin%20dashboard.jpg)

![Operator Entry/Exit Management](docs/screenshots/Operator%20entry%20exit.jpg)

![Scoring Strategy Plugins](docs/screenshots/Admin%20strategy.jpg)

## Architecture

Clean Architecture with strict layer separation:

```
Domain/              -- Entities and enums (no dependencies)
Models/              -- DTOs (request/response)
IBusinessLogic/      -- Business logic contracts
IDataAccess/         -- Data access contracts
BusinessLogic/       -- Service implementations (Strategy, Observer, Plugin patterns)
DataAccess/          -- EF Core repositories and migrations
Api/                 -- Controllers, middleware, DI composition
frontend/            -- Angular 19 SPA (Nginx in production)
```

### Design Patterns

- **Strategy Pattern** -- Pluggable scoring algorithms (PerAttraction, Combo, PerEvent, custom plugins)
- **Observer Pattern** -- Date/time change notifications for daily score resets and maintenance checks
- **Plugin System** -- Dynamic DLL loading via reflection for runtime extensibility
- **Repository Pattern** -- Interface-based data access with dependency injection

## Project Structure

```
obligatorio_parque_tematico_virtual/
├── Api/                    # ASP.NET Core Web API
│   ├── Controllers/        # 13 REST controllers
│   └── Filters/            # Global exception handling
├── BusinessLogic/          # Business logic implementations
│   ├── Strategy/           # Scoring strategies
│   └── Plugins/            # Dynamic plugin loader
├── DataAccess/             # EF Core repositories and migrations
│   └── Context/            # AppDbContext (14 tables)
├── Domain/                 # Domain entities
├── Models/                 # DTOs (In/Out)
├── IBusinessLogic/         # Business logic interfaces
├── IDataAccess/            # Data access interfaces
├── ApiServiceFactory/      # DI composition root
├── ExamplePlugin/          # Sample scoring plugin
├── frontend/               # Angular 19 SPA
│   └── src/app/
│       ├── core/           # Services, guards, interceptors
│       ├── features/       # Role-based feature modules
│       └── shared/         # Shared components
├── docker-compose.yml
└── Dockerfile
```

## Prerequisites

- .NET 8.0 SDK
- Node.js (for Angular frontend)
- Docker & Docker Compose (recommended)

## Quick Start

### Docker Compose (Recommended)

```bash
docker-compose up --build
```

Services:
- **SQL Server**: `localhost:1433`
- **Backend API**: `localhost:8080`
- **Frontend**: `localhost:4200`

### Local Development

1. Start SQL Server (Docker or local instance)
2. Copy `.env.example` to `.env` and configure:
   ```
   DB_SERVER=localhost,1433
   DB_NAME=ParqueTematicoDB
   DB_USER=SA
   DB_PASSWORD=Your_password123
   JWT_SECRET_KEY=MySecretKeyForJWTTokenGeneration1234567890
   JWT_ISSUER=ParqueTematico
   JWT_AUDIENCE=ParqueTematico
   JWT_EXPIRATION_HOURS=1
   ```
3. Run the backend:
   ```bash
   dotnet run --project obligatorio_parque_tematico_virtual/Api/Api.csproj
   ```
4. Run the frontend:
   ```bash
   cd obligatorio_parque_tematico_virtual/frontend
   npm install
   ng serve
   ```

### Default Users

| Role | Email | Password |
|---|---|---|
| Administrator | admin@test.com | admin123 |
| Operator | operator@test.com | operator123 |

## API Documentation

Swagger UI is available at `/swagger` when running in development mode.

## Testing

```bash
dotnet test obligatorio_parque_tematico_virtual/obligatorio_parque_tematico_virtual.sln
```

Four test projects covering all layers:
- `TestApi` -- Controller and integration tests
- `TestBusinessLogic` -- Business logic unit tests
- `TestDataAccess` -- Repository tests
- `TestDomain` -- Domain entity tests

## Database

- **Production**: SQL Server (auto-migrates on startup)
- **Testing**: SQLite (separate `ParqueTematicoDB_Test` database)
- Seed data scripts in `Datos/BaseDataNoEntries.sql`
- Postman collection in `Datos/parque_tematico_virtual_api.postman_collection.json`
