# FinanceTracker

A personal finance tracker: accounts, transactions, categories, budgets, and goals, with a .NET Web API backend and an Angular frontend.

Follow-up project after a prior Link Shortener (Minimal API + EF Core + PostgreSQL) app — this one deliberately uses Controllers + MediatR + Clean Architecture to practice that pattern.

## Tech Stack

- **Backend:** ASP.NET Core Web API (Controllers, not Minimal API), MediatR (CQRS), FluentValidation, AutoMapper, EF Core, PostgreSQL
- **Frontend:** Angular (`client/`)
- **Testing:** xUnit, Testcontainers + PostgreSQL for integration tests
- **CI:** GitHub Actions (build/test backend, build/lint frontend, Docker image builds, on push/PR to `master`)
- **Hosting:** Railway (API + Angular/nginx + Postgres, deployed from Dockerfiles) — see [Deployment](#deployment-railway)

## Architecture

Clean Architecture with a strict dependency rule — nothing in `Domain` or `Application` may reference EF Core, ASP.NET Core, or any other framework package:

```
Domain          <- no project dependencies, pure C#
  ^
Application     <- depends only on Domain
  ^
Infrastructure  <- depends on Application (implements its interfaces)
Api             <- depends on Application (+ Infrastructure only for DI wiring in Program.cs)
```

SOLID is enforced throughout: one handler per command/query, cross-cutting concerns via MediatR pipeline behaviours, small purpose-built interfaces (`IAccountRepository`, `ICurrentUserService`), and dependency inversion between `Application` (interfaces) and `Infrastructure` (implementations).

## Project Structure

```
FinanceTracker.slnx
src/
  Domain/          Entities, Enums, ValueObjects, Common, Exceptions
  Application/      Common (Interfaces/Behaviours/Mappings/Exceptions), Features/<FeatureName>/{Commands,Queries}
  Infrastructure/  Persistence (DbContext, Configurations, Migrations, Interceptors), Repositories, Services
  Api/             Controllers, Extensions, Middleware, Program.cs
client/            Angular app
tests/
  Domain.UnitTests/
  Application.UnitTests/
  Application.IntegrationTests/   Testcontainers + Postgres
  Api.FunctionalTests/
```

Each project registers its own services through exactly one `DependencyInjection.cs` / `ServiceCollectionExtensions.cs` file — nothing is registered directly in `Program.cs` beyond three calls: `AddApplicationServices()`, `AddInfrastructureServices()`, `AddApiServices()`.

Each command/query is a self-contained feature slice: `Request`, `Handler`, `Validator`, and a `Response` DTO (implementing `IMapFrom<T>` for AutoMapper). Controllers stay thin — they only translate HTTP into a `Send()` call.

## Getting Started

```bash
# Build
dotnet build

# Run tests
dotnet test

# Run the API
dotnet run --project src/Api

# Angular dev server (from client/)
ng serve
```

### EF Core Migrations

```bash
# Add a migration
dotnet ef migrations add <Name> --project src/Infrastructure --startup-project src/Api

# Apply migrations
dotnet ef database update --project src/Infrastructure --startup-project src/Api
```

### Local database setup

The API needs a Postgres database and a JWT signing key to run. Neither is committed (`appsettings.json` intentionally ships empty placeholders for both — they're secrets), so each developer configures their own via `src/Api/appsettings.Development.json`, which is gitignored.

1. Create a dedicated database and role (using any local Postgres install, e.g. `psql -U postgres`):
   ```sql
   CREATE ROLE financetracker WITH LOGIN PASSWORD 'choose-a-local-password';
   CREATE DATABASE financetracker OWNER financetracker;
   ```
2. Create `src/Api/appsettings.Development.json` (if it doesn't already exist) with:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=localhost;Port=5432;Database=financetracker;Username=financetracker;Password=choose-a-local-password"
     },
     "Jwt": {
       "Key": "any-sufficiently-long-random-string-for-local-dev"
     }
   }
   ```
3. Apply migrations (see above), then `dotnet run --project src/Api`.

This is for local development only — real environments should get both values from proper secrets management (Key Vault, environment variables, etc.), never a checked-in file.

### CORS & observability

`Cors:AllowedOrigins` (in `appsettings.json`) defaults to `http://localhost:4200` for local dev. **Every non-Development deployment must override it** with the real frontend origin — otherwise the browser will reject every API call. Override via an environment variable rather than a committed file:

```bash
Cors__AllowedOrigins__0=https://your-frontend-domain.example.com
```

If this isn't set, the API logs a warning on startup (visible wherever your host collects logs) rather than failing silently.

Two health check endpoints are available for load balancers / orchestrators:

- `GET /health/live` — process is up, no dependency checks (liveness probe)
- `GET /health/ready` — also checks the database connection (readiness probe)

Every request is logged (method, path, status code, duration) via ASP.NET Core's built-in HTTP logging — no request/response bodies or headers are logged, so this is safe to leave on in production.

## Deployment (Railway)

The app deploys as **three Railway resources** in one project: a Postgres database, the API, and the Angular app. The Angular container's nginx serves the static files *and* reverse-proxies `/api/*` to the API over Railway's private network — the browser only ever talks to one public origin, so there's no CORS to configure for the deployed app itself (the `Cors:AllowedOrigins` setting from the section above still exists as a fallback for anyone hitting the API directly).

### 1. Database

In the Railway project: **New → Database → PostgreSQL**. No configuration needed — Railway exposes `PGHOST`/`PGPORT`/`PGDATABASE`/`PGUSER`/`PGPASSWORD` variables that the API service references below.

### 2. API service

**New → GitHub Repo** → select this repo.

- **Root Directory:** `.` (repo root — the Dockerfile needs the whole solution as build context)
- **Dockerfile Path:** `src/Api/Dockerfile`
- **Healthcheck Path:** `/health/ready`
- **Variables** (Railway's `${{ServiceName.VAR}}` syntax reaches into the Postgres service; replace `Postgres` with whatever you named that service):
  ```
  ASPNETCORE_ENVIRONMENT=Production
  ConnectionStrings__DefaultConnection=Host=${{Postgres.PGHOST}};Port=${{Postgres.PGPORT}};Database=${{Postgres.PGDATABASE}};Username=${{Postgres.PGUSER}};Password=${{Postgres.PGPASSWORD}}
  Jwt__Key=<generate a long random secret - e.g. `openssl rand -base64 48`>
  Jwt__Issuer=FinanceTracker
  Jwt__Audience=FinanceTracker
  Jwt__AccessTokenMinutes=15
  App__ClientBaseUrl=https://<your-web-service>.up.railway.app
  Cors__AllowedOrigins__0=https://<your-web-service>.up.railway.app
  ```
- In **Settings → Networking**, note the service's private domain (defaults to the service name, e.g. `api.railway.internal`) — the web service needs it next.

### 3. Web service (Angular)

**New → GitHub Repo** → same repo again, as a second service.

- **Root Directory:** `client`
- **Dockerfile Path:** `Dockerfile` (the default)
- **Variables:**
  ```
  API_INTERNAL_URL=http://<api-service-private-domain>:8080
  ```
- Generate a public domain for this service (**Settings → Networking → Generate Domain**). This is the URL your users hit, and the one you feed back into `App__ClientBaseUrl` / `Cors__AllowedOrigins__0` on the API service above.

### 4. Apply migrations

Migrations are **not** applied automatically on startup (deliberately — see `dotnet ef database update` below). After the first deploy, and after any deploy that adds a migration, run it against the Postgres instance:

```bash
dotnet ef database update --project src/Infrastructure --startup-project src/Api --connection "<the same connection string you set on the API service, using Postgres's public host/port instead of the private one>"
```

Railway's Postgres service has a "Connect" tab with the public connection details for exactly this.

### 5. Gate deploys on CI

Both services: **Settings → Deploy Triggers → Wait for CI**. This makes Railway wait for the GitHub Actions checks (see [CI](#ci) below — backend tests, frontend build/lint, and the Docker builds) to pass before deploying, so a broken `master` push never reaches production.

From here, every push to `master` that passes CI redeploys both services automatically — no separate CD workflow needed, Railway's GitHub integration handles it.

## Features

MVP (v1) scope covers:

- **Auth & Multi-user** — JWT register/login, per-user data isolation, password reset
- **Accounts** — multiple accounts per user, balance auto-calculated from transactions
- **Transactions** — CRUD, search/filter by date range, category, amount, account
- **Categories** — user-defined, with a default set seeded on signup
- **Budgeting** — monthly budget per category, budget vs. actual comparison
- **Reporting** — monthly income/expense summary, spend-by-category breakdown
- **Security & Reliability** — FluentValidation on all commands, global exception handling middleware
- **Testing & DevOps** — xUnit handler tests, CI pipeline (build/test/lint on PR)

Should-have (v2) and stretch scope include multi-currency accounts, recurring transactions, receipt attachments, household/shared accounts, net worth over time, savings goals, and email notifications. See `FEATURES.md` for the full prioritized list and `USER_STORIES.md` for detailed user stories with acceptance criteria per role (Guest, Registered User, Household Member, Admin).

## Milestones

1. Solution scaffold — all four projects wired via `DependencyInjection.cs`, empty `Program.cs` composition root
2. Accounts — full CRUD vertical slice (Command/Query/Validator/DTO/Controller), AutoMapper profile
3. Transactions — linked to Accounts, with validation rules
4. Angular frontend for Accounts + Transactions
5. Budgets / Reports
6. Full review pass (security, N+1 queries, SOLID/architecture consistency)

## Conventions

- Do not put business logic in Controllers
- Do not put EF Core–specific code outside `Infrastructure`
- Do not manually map entities to DTOs — use AutoMapper
- Do not register services outside each project's single DI extension file
- Do not edit existing EF Core migration files — always add a new migration
- New commands/queries need at least one happy-path and one validation-failure test
- Run `dotnet test` before considering any backend task done

## CI

GitHub Actions runs on every push/PR to `main`:

- **Backend:** `dotnet restore` → `dotnet build` → `dotnet test` (.NET 9)
- **Frontend:** `npm ci` → `npm run lint` → `npm run build` (Node 20, from `client/`)
- **Docker:** builds both `src/Api/Dockerfile` and `client/Dockerfile` (catches deployment-image breakage before it reaches `master`)
