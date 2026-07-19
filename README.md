# ExpenseTracker Pro

ExpenseTracker Pro is an ASP.NET Core 8 personal-finance API built with a
layered Domain, Application, Infrastructure, and API architecture.

## Current capabilities

- Email/password, Google, and passwordless mobile authentication
- Forgot-password email, rotating refresh tokens, and logout revocation
- Accounts, income and expense transactions, and account transfers
- Budgets, three-to-twelve-month forecasting, recurring transactions, financial goals, and notifications
- Detailed daily expense calendar with transaction drill-down data
- Dashboard analytics and financial reports
- MoMo smart finance buddy with live, user-scoped insights and conversational follow-ups
- API versioning, Swagger, pagination, health checks, and background processing
- Rate limiting, mutation audit logs, correlation IDs, and optimistic concurrency
- Per-module and consolidated, authenticated audit-trail views

## Local setup

Requirements:

- .NET 8 SDK
- SQL Server or SQL Server Express

Configure the connection string for your machine, then store a random JWT
signing secret outside tracked configuration:

```bash
cd ExpenseTracker.Api
dotnet user-secrets set "Jwt:Key" "<random-secret-containing-at-least-32-bytes>"
```

Never commit the real signing value. Deployed environments should provide it
through the `Jwt__Key` environment variable or a managed secret store.

MoMo runs entirely on the application's own finance rules and user-scoped data.
It does not require an external AI model, API key, or financial-data egress.

Apply migrations and start the API. From PowerShell at the solution root:

```powershell
dotnet ef database update --project ".\ExpenseTracker.Infrastructure\ExpenseTracker.Infrastructure.csproj" --startup-project ".\ExpenseTracker.Api\ExpenseTracker.Api.csproj"
dotnet run --project ".\ExpenseTracker.Api\ExpenseTracker.Api.csproj"
```

Swagger is available in Development at `/swagger`.

## Tests

```bash
dotnet test ExpenseTracker.Pro.sln
```

Development sample-data seeding is disabled by default. See
`docs/TEST-DATA.md` for the complete existing-user test dataset and
`docs/PHASE-1-RELIABILITY-SECURITY.md` for the security baseline. Provider
setup and endpoint examples are in `docs/AUTHENTICATION-PROVIDERS.md`.
Expense calendar and forecast behavior is documented in
`docs/BUDGET-FORECAST-AND-EXPENSE-CALENDAR.md`.

For deployment and million-user capacity planning, see
`docs/PRODUCTION-SCALE.md`. The document separates application hardening already
implemented here from the distributed infrastructure and load-testing gates that
are still required before a large production launch.
