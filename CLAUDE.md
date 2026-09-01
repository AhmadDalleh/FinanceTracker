# Personal Finance Tracker

## Commands
- Build: `dotnet build`
- Test: `dotnet test`
- Run API: `dotnet run --project src/Api`
- Angular dev server: `ng serve` (from `client/`)
- Add migration: `dotnet ef migrations add <Name> --project src/Infrastructure --startup-project src/Api`
- Update database: `dotnet ef database update --project src/Infrastructure --startup-project src/Api`

## Context
- ASP.NET Core Web API using **Controllers** (not Minimal API) + **MediatR** for command/query handling
- **Clean Architecture** — strict dependency rule, see below
- **SOLID principles** enforced throughout, see below
- **AutoMapper** for all Entity ↔ DTO mapping — no manual object mapping in handlers
- Angular frontend, separate app under `client/`
- EF Core + PostgreSQL
- FluentValidation for request validation, wired in as a MediatR pipeline behaviour
- Follow-up project after the Link Shortener (Minimal API + EF Core + PostgreSQL) — this one deliberately uses Controllers instead, to practice that pattern

## Architecture & Principles

**Dependency rule (Clean Architecture):**
```
Domain          <- no project dependencies, pure C#
  ^
Application     <- depends only on Domain
  ^
Infrastructure  <- depends on Application (implements its interfaces)
Api             <- depends on Application (+ references Infrastructure only for DI wiring in Program.cs)
```
Nothing inside `Domain` or `Application` may reference EF Core, ASP.NET Core, or any other framework package. Abstractions live in `Application`; implementations live in `Infrastructure`.

**SOLID, applied concretely in this codebase:**
- **S — Single Responsibility**: one Handler per command/query; one Validator per command/query; a Controller action only translates HTTP → `Send()` → HTTP, nothing else.
- **O — Open/Closed**: cross-cutting behaviour (validation, logging, performance) is added via MediatR `IPipelineBehaviour`, not by editing existing handlers. New features get a new feature folder, not edits to unrelated ones.
- **L — Liskov Substitution**: any implementation of an `Application`-defined interface (e.g. a repository or `IDateTimeProvider`) must be fully substitutable — no `throw new NotImplementedException()` on interface members, no implementation-specific exceptions leaking through the abstraction.
- **I — Interface Segregation**: prefer small, purpose-built interfaces (`IAccountRepository`, `ICurrentUserService`) over one large generic repository. A handler depends only on the interface members it actually uses.
- **D — Dependency Inversion**: `Application` defines interfaces (`IApplicationDbContext`, `IAccountRepository`, `ICurrentUserService`); `Infrastructure` implements them. Handlers and controllers depend on the interface, never on the concrete `Infrastructure` type.

## Structure
```
FinanceTracker.sln
src/
  Domain/
    Entities/                     Account.cs, Transaction.cs, Category.cs, Budget.cs, Goal.cs
    Enums/                        AccountType.cs, TransactionType.cs
    ValueObjects/                 Money.cs
    Common/                       BaseEntity.cs, IAuditableEntity.cs
    Exceptions/                   DomainException.cs
    Domain.csproj

  Application/
    Common/
      Interfaces/                 IApplicationDbContext.cs, ICurrentUserService.cs, IDateTimeProvider.cs
      Behaviours/                 ValidationBehaviour.cs, LoggingBehaviour.cs, UnhandledExceptionBehaviour.cs
      Mappings/                   IMapFrom.cs (marker interface used by AutoMapper's assembly scan)
      Exceptions/                 ValidationException.cs, NotFoundException.cs, ForbiddenAccessException.cs
    Features/
      Accounts/
        Commands/
          CreateAccount/          CreateAccountCommand.cs, CreateAccountCommandHandler.cs, CreateAccountCommandValidator.cs
          UpdateAccount/
          ArchiveAccount/
        Queries/
          GetAccounts/            GetAccountsQuery.cs, GetAccountsQueryHandler.cs
          GetAccountById/
        AccountDto.cs              implements IMapFrom<Account>
      Transactions/                same shape as Accounts
      Categories/
      Budgets/
      Goals/
    DependencyInjection.cs         <- ServiceExtensions file for this project (see below)
    Application.csproj

  Infrastructure/
    Persistence/
      ApplicationDbContext.cs
      Configurations/              AccountConfiguration.cs, TransactionConfiguration.cs, ... (IEntityTypeConfiguration<T>)
      Migrations/
      Interceptors/                AuditableEntitySaveChangesInterceptor.cs
    Repositories/                  AccountRepository.cs, TransactionRepository.cs
    Services/                      CurrentUserService.cs, DateTimeProvider.cs
    DependencyInjection.cs         <- ServiceExtensions file for this project
    Infrastructure.csproj

  Api/
    Controllers/                   AccountsController.cs, TransactionsController.cs, CategoriesController.cs
    Extensions/
      ServiceCollectionExtensions.cs   <- ServiceExtensions file for this project (Swagger, CORS, Auth, versioning)
    Middleware/                    ExceptionHandlingMiddleware.cs
    Program.cs
    appsettings.json
    Api.csproj

client/                            Angular app

tests/
  Domain.UnitTests/
  Application.UnitTests/
  Application.IntegrationTests/    Testcontainers + Postgres
  Api.FunctionalTests/
```

## Dependency Injection / Service Extensions
Every project that registers services has **exactly one** static class exposing a single `IServiceCollection` extension method — no service registration scattered elsewhere, and nothing registered directly in `Program.cs` beyond calling these three methods.

- `Application/DependencyInjection.cs` → `AddApplicationServices(this IServiceCollection services)`
  Registers MediatR (scanning the Application assembly), AutoMapper (scanning the Application assembly), FluentValidation validators, and the pipeline behaviours.
- `Infrastructure/DependencyInjection.cs` → `AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)`
  Registers `ApplicationDbContext` (Postgres connection string), repository implementations, `ICurrentUserService`, `IDateTimeProvider`.
- `Api/Extensions/ServiceCollectionExtensions.cs` → `AddApiServices(this IServiceCollection services, IConfiguration configuration)`
  Registers Controllers, Swagger/OpenAPI, CORS policy, JWT authentication, API versioning, rate limiting.

`Program.cs` reads as three lines plus middleware wiring:
```csharp
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddApiServices(builder.Configuration);
```

## Mapping (AutoMapper)
- Every DTO implements a marker interface `IMapFrom<T>` with a default `Mapping(Profile profile)` method; a single `AddAutoMapper(Assembly)` call in `Application/DependencyInjection.cs` scans the assembly and picks up every profile automatically — no per-DTO registration.
- Handlers never hand-map objects. Always `_mapper.Map<AccountDto>(account)` or `_mapper.ProjectTo<AccountDto>(queryable)` for query handlers, so filtering/paging happens in SQL, not in memory.
- If a mapping needs custom logic beyond a straight property match, override `Mapping()` in that DTO with `.ForMember(...)` — do not put mapping logic in the handler.

## CQRS / MediatR Conventions
- One feature folder per business capability: `Application/Features/<FeatureName>/`
- Each command/query is a self-contained set: `Request` (`IRequest<TResponse>`), `Handler` (`IRequestHandler<TRequest, TResponse>`), `Validator` (`AbstractValidator<TRequest>`), and a `Response` DTO
- Commands mutate and return only what the caller needs (usually just an Id or nothing — not the full entity)
- Queries use `IApplicationDbContext` directly with `.ProjectTo<Dto>()`, no repository needed for read paths
- Controllers are pure: `[HttpPost] public async Task<IActionResult> Create(CreateAccountCommand cmd) => Ok(await _mediator.Send(cmd));` — no branching, no mapping, no validation calls in the controller itself

## Conventions
- Controllers stay thin — they only map the request to a MediatR `Send()` call, no logic
- Validation happens automatically via `ValidationBehaviour` in the MediatR pipeline — controllers and handlers never call a validator directly
- Repositories are interfaces in `Application`, implementations in `Infrastructure`
- All entities inherit `BaseEntity` (Id) and, where relevant, implement `IAuditableEntity` (CreatedAt/By, UpdatedAt/By) — set automatically via the `SaveChangesInterceptor`, never manually in a handler

## Do Not
- Do not put business logic in Controllers
- Do not put EF Core–specific code (DbContext, LINQ-to-SQL) outside `Infrastructure`
- Do not manually map entities to DTOs — use AutoMapper
- Do not register services outside each project's single `DependencyInjection.cs` / `ServiceCollectionExtensions.cs` file
- Do not edit existing EF Core migration files — always add a new migration
- Do not add new NuGet or npm packages without checking with me first

## Testing
- Run `dotnet test` before considering any backend task done
- New commands/queries need at least one happy-path and one validation-failure test
- Mapping profiles get a test asserting `AssertConfigurationIsValid()` so a broken mapping fails fast

## Current milestones
1. Solution scaffold — all four projects wired via their `DependencyInjection.cs` files, empty `Program.cs` composition root
2. Accounts — full CRUD vertical slice (Command/Query/Validator/DTO/Controller), AutoMapper profile
3. Transactions — linked to Accounts, with validation rules
4. Angular frontend for Accounts + Transactions
5. Budgets / Reports
6. Full review pass (security, N+1 queries, SOLID/architecture consistency)
