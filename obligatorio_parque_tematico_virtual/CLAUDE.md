# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**Parque Temático Virtual** - A virtual theme park management system combining physical attractions with gamified digital experiences. Built with ASP.NET Core 8.0, Entity Framework Core with SQL Server, and JWT authentication.

## Essential Commands

### Building and Running

```bash
# Build entire solution
dotnet build obligatorio_parque_tematico_virtual.sln

# Apply database migrations (required before first run)
dotnet ef database update --project DataAccess --startup-project Api

# Run the API (launches Swagger at http://localhost:5020)
dotnet run --project Api/Api.csproj

# Build specific project
dotnet build <ProjectFolder>/<ProjectName>.csproj
```

### Testing

```bash
# Run all tests
dotnet test obligatorio_parque_tematico_virtual.sln

# Run tests for specific project
dotnet test TestApi/TestApi.csproj
dotnet test TestBusinessLogic/TestBusinessLogic.csproj
dotnet test TestDataAccess/TestDataAccess.csproj
dotnet test TestDomain/TestDomain.csproj

# Run single test class
dotnet test --filter FullyQualifiedName~AttractionControllerTest

# Run single test method
dotnet test --filter FullyQualifiedName~AttractionControllerTest.TestMethodName
```

### Adding Projects to Solution

```bash
# Add new project to solution
dotnet sln add <path-to-project>/<ProjectName>.csproj

# Add project reference
dotnet add <ProjectA>/<ProjectA>.csproj reference <ProjectB>/<ProjectB>.csproj
```

## Architecture

### Clean Architecture Layers

The solution follows a **clean architecture** pattern with clear separation of concerns:

```
Domain (Core) ← IBusinessLogic ← BusinessLogic ← Api
                      ↓               ↓
                 IDataAccess ← DataAccess
```

**Dependency Flow:** Api → BusinessLogic → DataAccess → Domain (outer layers depend on inner)

#### 1. **Domain** (Core Layer)

- Pure business entities with no dependencies
- Inheritance hierarchy: `User` (abstract) → `Administrator`, `Operator`, `Visitor`
- Entity relationships: `Event` ↔ `Attraction` (many-to-many via `EventAttraction`), `Visitor` → `Ticket` (one-to-many)

#### 2. **IBusinessLogic & BusinessLogic**

- Business rules and application logic
- Services follow interface segregation (e.g., `IAttractionService`, `IEventService`, `IAuthLogic`)
- **Key Pattern:** `DateTimeLogic` is a **singleton** for test-controllable time (allows setting custom date/time via API)
- **AutoMapper Integration:** Uses AutoMapper 12.0.1 for DTO mapping (configured via `MappingProfile.cs`)
- **Extracted Services:**
  - `IUserValidationService`: Handles all user validation logic (email format, uniqueness, birthdate, required fields, membership levels)
  - `IParkEntryLogic`: Manages attraction entry/exit registration and visitor reports
  - `IUserManagementLogic`: User CRUD operations (formerly `IUserLogic`)

#### 3. **IDataAccess & DataAccess**

- Repository pattern for data access
- Uses Entity Framework Core with **SQL Server database**
- Context: `AppDbContext` with TPH (Table-Per-Hierarchy) discriminator for `User` inheritance
- Migrations managed through EF Core CLI tools

#### 4. **Models**

- DTOs for API contracts (`Models.In` for requests, `Models.Out` for responses)
- Separates API layer concerns from domain entities

#### 5. **Api**

- ASP.NET Core Web API with controllers
- JWT Bearer authentication configured in `Program.cs`
- Global exception filter (`ExceptionFilter`) maps exceptions to HTTP status codes

#### 6. **ApiServiceFactory**

- Centralized dependency injection configuration
- Extension method `AddServices()` registers all services with appropriate lifetimes

### Critical Patterns & Conventions

#### AutoMapper Configuration

**AutoMapper 12.0.1** is used for mapping between domain entities and DTOs:

- Configuration: `BusinessLogic/MappingProfile.cs` defines all mappings
- Usage: Inject `IMapper` into services and use `_mapper.Map<TDestination>(source)`
- All business logic services use AutoMapper instead of manual DTO construction
- Observer interfaces (`IDateChangeObserver`, `IScoreChangeObserver`) are in `IBusinessLogic` layer

#### Password Validation

Password complexity requirements enforced by `PasswordService`:

- Minimum 8 characters
- At least one uppercase letter
- At least one lowercase letter
- At least one digit
- Validates on user registration and password changes

#### User Type Discriminator Pattern

EF Core uses TPH (Table-Per-Hierarchy) with discriminator column:

```csharp
modelBuilder.Entity<User>()
    .HasDiscriminator<string>("UserType")
    .HasValue<Administrator>("Administrator")
    .HasValue<Operator>("Operator")
    .HasValue<Visitor>("Visitor");
```

#### JWT Role Extraction

**IMPORTANT:** Roles are derived from **C# class names**, not from a property:

```csharp
string role = user.GetType().Name; // "Administrator", "Operator", or "Visitor"
```

This is used in token generation and `[Authorize(Roles = "...")]` attributes.

**Role Constants** (defined in `Domain/Role.cs`):
```csharp
public const string Administrator = "Administrator";
public const string Operator = "Operator";
public const string Visitor = "Visitor";
```

Use these constants instead of hardcoded strings: `Role.Administrator`, `Role.Operator`, `Role.Visitor`

#### DateTime Singleton for Testing

`DateTimeLogic` is a thread-safe singleton allowing system-wide time override:

- Use `DateTimeLogic.Instance.GetCurrentDateTime()` instead of `DateTime.Now`
- Public endpoint `/api/datetime` allows setting custom date/time for testing
- **Must call `DateTimeLogic.ResetInstance()` in test cleanup**

#### Service Lifetimes

- **Singleton:** `DateTimeLogic`, `PasswordService`, `TokenService` (stateless or shared state)
- **Scoped:** All other services and repositories (per-request lifecycle)

### Testing Architecture

Uses **MSTest** framework with **Moq** for mocking:

- **TestApi:** Integration tests using `WebApplicationFactory<Program>` to test full HTTP pipeline (uses in-memory database for isolation)
- **TestBusinessLogic:** Unit tests for business logic with mocked repositories
- **TestDataAccess:** Repository tests with in-memory database (for test isolation)
- **TestDomain:** Domain entity validation tests

**Key Testing Pattern (TestApi):**

```csharp
[TestInitialize]
public void Setup()
{
    var mockService = new Mock<IService>();
    _factory = new WebApplicationFactory<Program>()
        .WithWebHostBuilder(builder => {
            builder.ConfigureServices(services => {
                services.AddSingleton(mockService.Object);
            });
        });

    // Create authenticated client with JWT token
    var tokenService = new TokenService();
    var adminUser = new Administrator { /* ... */ };
    string token = tokenService.GenerateToken(adminUser);
    _adminClient = _factory.CreateClient();
    _adminClient.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue("Bearer", token);
}

[TestCleanup]
public void Cleanup()
{
    DateTimeLogic.ResetInstance(); // Always reset singleton
    _client?.Dispose();
    _factory?.Dispose();
}
```

### Authentication & Authorization

#### Current Implementation

- **JWT Bearer tokens** with 1-hour expiration
- Secret key: `"MySecretKeyForJWTTokenGeneration1234567890"` (hardcoded in both `TokenService.cs` and `Program.cs`)
- Public endpoints: `/api/auth/login`, `/api/auth/register`, `/api/datetime/*`
- Protected endpoints use `[Authorize(Roles = "Administrator")]` or similar

#### Known Issues & Limitations

1. **AuthController.Login() returns hardcoded role/name** (lines 30-31 in AuthController.cs) - should extract from user entity
2. **No error handling for invalid login** - returns 200 OK with null token instead of 401
3. **TicketController has no authorization** - all ticket endpoints are public
4. **No admin/operator registration endpoints** - only visitor self-registration exists

### Database Schema Notes

**Unique Constraints:**

- `User.Email` (unique index)
- `Attraction.Name` (unique index)
- `Event.Name` (unique index)
- `Ticket.QRCode` (unique index)

**Relationships:**

- `EventAttraction`: Composite key (`EventId`, `AttractionId`) for many-to-many
- `Ticket → Visitor`: FK with `CASCADE DELETE`

## Project-Specific Conventions

### Exception Handling Strategy

The `ExceptionFilter` maps exceptions to HTTP status codes:

- `KeyNotFoundException` → 404 Not Found
- `ArgumentException` → 400 Bad Request
- `NotImplementedException` → 501 Not Implemented
- All others → 500 Internal Server Error

**Convention:** Throw domain-appropriate exceptions in business logic; filter handles HTTP mapping.

### Naming Conventions

- Services end with `Service` or `Logic` (e.g., `AttractionService`, `AuthLogic`)
- Repositories end with `Repository` (e.g., `UserRepository`)
- Controllers end with `Controller` (e.g., `AttractionController`)
- Test classes end with `Test` and mirror source file name (e.g., `AttractionServiceTest`)
- **Constants use PascalCase** (e.g., `Role.Administrator`, not `Role.ADMINISTRATOR`)
- **File-scoped namespaces** are preferred (C# 10+ feature) for cleaner code structure

### API Route Patterns

- Base route: `/api/[controller]` or `/api/{resource}` (e.g., `/api/attractions`)
- RESTful conventions: GET, POST, PUT, DELETE
- ID parameters use `Guid` for most entities, `int` for tickets

## Development Workflow

### Adding New Features

1. **Domain Layer:** Add entity classes to `Domain/` project
2. **Data Access:** Create repository interface in `IDataAccess/`, implement in `DataAccess/Repositories/`
3. **Business Logic:** Create service interface in `IBusinessLogic/`, implement in `BusinessLogic/`
4. **Models:** Add request/response DTOs to `Models/In` and `Models/Out`
5. **API:** Create controller in `Api/Controllers/` with appropriate `[Authorize]` attributes
6. **DI Registration:** Add service registration to `ApiServiceFactory/ServiceFactory.cs`
7. **Tests:** Add tests in corresponding `Test*` projects
8. **TDD:** Add tests for new functionality before implementing it following the GREEN-RED-REFACTOR commit cycle (see previous commits to follow the exact same pattern, never include claude mentions in commits nor add this file to staged changes). FOR EVERY PLAN you will have to first plan the commits to be made with the specific fixes or features implemented (all RED-GREEN commits and optionally REFACTOR commits planned and shown on the plan beforehand)
   Follow this pattern for RED commits: "test: [RED] ..." and for GREEN commits: "feat: [GREEN] ..." and for REFACTOR commits: "refactor: [REFACTOR] ..."
9. NEVER include disclaimers indicating that the code was generated with claude

### Working with Authentication

**To test authenticated endpoints:**

```csharp
// Create a token for specific role
var tokenService = new TokenService();
var adminUser = new Administrator {
    Id = Guid.NewGuid(),
    Name = "Admin",
    LastName = "User",
    Email = "admin@test.com"
};
string token = tokenService.GenerateToken(adminUser);

// Use in HTTP client
client.DefaultRequestHeaders.Authorization =
    new AuthenticationHeaderValue("Bearer", token);
```

### Database Modifications

When adding/modifying entities:

1. Update domain entity in `Domain/`
2. Add/modify EF Core configuration in `AppDbContext.OnModelCreating()`
3. Update repository interfaces/implementations
4. Create and apply migration:

   ```bash
   # Create migration
   dotnet ef migrations add <MigrationName> --project DataAccess --startup-project Api

   # Apply migration to database
   dotnet ef database update --project DataAccess --startup-project Api

   # Remove last migration (if needed before applying)
   dotnet ef migrations remove --project DataAccess --startup-project Api
   ```

## Recent Refactorings

The following refactorings have been applied to improve code quality and maintainability:

### AutoMapper Integration (Commits 1-2)
- Added AutoMapper 12.0.1 to BusinessLogic project
- Created `MappingProfile.cs` with all domain entity → DTO mappings
- Refactored all business logic services to use AutoMapper instead of manual DTO construction
- Updated all tests to use AutoMapper

### Architecture Improvements (Commit 3)
- Moved observer interfaces (`IDateChangeObserver`, `IScoreChangeObserver`) from Domain to IBusinessLogic layer
- Maintains proper dependency flow in clean architecture

### Error Message Standardization (Commit 4)
- Standardized all error messages to English across business logic and domain layers
- Ensures consistent error messaging for API consumers

### Password Complexity Validation (Commits 5-6)
- Added comprehensive password validation rules (8+ chars, uppercase, lowercase, digit)
- Implemented in `PasswordService.ValidatePasswordComplexity()`
- Applied to user registration and password changes
- Full test coverage (19 tests)

### Service Extraction for Single Responsibility (Commits 7-11)
- **UserValidationService** (commits 7-8): Extracted all user validation logic from UserLogic
  - Email format validation
  - Email uniqueness validation
  - Birthdate validation
  - Required fields validation
  - Membership level validation
  - 19 comprehensive tests
- **ParkEntryLogic** (commits 9-10): Extracted attraction entry/exit logic from UserLogic
  - Entry registration
  - Exit registration
  - Visitor reports
  - 10 comprehensive tests
- **UserLogic refactoring** (commit 11): Updated to use extracted services, reducing constructor dependencies from 9 to 6

### Naming Improvements (Commits 12-13)
- Renamed `UserLogic` → `UserManagementLogic` for clarity (commit 12)
- Renamed `IUserLogic` → `IUserManagementLogic` (commit 12)
- Standardized Role constants from SCREAMING_SNAKE_CASE to PascalCase (commit 13):
  - `Role.ADMINISTRATOR` → `Role.Administrator`
  - `Role.OPERATOR` → `Role.Operator`
  - `Role.VISITOR` → `Role.Visitor`

### Code Modernization (Commit 19)
- Converted `UserManagementLogic` to use file-scoped namespace (C# 10+ feature)
- Reduces indentation and improves code readability

## API Endpoints Reference

### Public Endpoints (No Auth)

- `POST /api/auth/login` - Authenticate user
- `POST /api/auth/register` - Self-register as visitor
- `GET /api/datetime` - Get current system date/time
- `POST /api/datetime` - Set custom date/time (for testing)

### Administrator Only

- `/api/attractions/*` - Full CRUD for attractions
- `/api/events/*` - Full CRUD for events

### Unprotected (Should Be Fixed)

- `/api/tickets/*` - Currently public, should require authentication
