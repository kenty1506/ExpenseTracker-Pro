# Complete development test data

The development seeder can populate an existing registered account without
storing or knowing its password.

## Included dataset

- 10 categories covering expenses, income, savings, and goal funding
- 5 accounts: Cash, BPI Savings, GCash, Maya Wallet, and BDO Credit Card
- A full month of income and expense transactions
- 3 account transfers
- 6 monthly budgets
- 8 recurring transactions
- 4 financial goals in active, paused, and completed states
- 3 goal contributions, including interest
- 5 read and unread notifications with different priorities
- 18 audit-trail events across the main modules, including successful and failed operations

Seed operations are module-aware and can be run again without duplicating a
module that already contains data. Missing standard categories and accounts are
added individually. Seed audit records use stable `dev-seed-audit-01` through
`dev-seed-audit-18` trace IDs, so rerunning the seeder only adds missing records.

## Populate an existing account

From the directory containing `ExpenseTracker.Pro.sln`, set the development
seeder options using User Secrets:

```powershell
dotnet user-secrets set "DevelopmentSeeder:Enabled" "true" --project ".\ExpenseTracker.Api\ExpenseTracker.Api.csproj"
dotnet user-secrets set "DevelopmentSeeder:Email" "testuser1@gmail.com" --project ".\ExpenseTracker.Api\ExpenseTracker.Api.csproj"
```

Start the API in Development:

```powershell
dotnet run --project ".\ExpenseTracker.Api\ExpenseTracker.Api.csproj"
```

The data is created during startup. Stop the API after startup completes, then
disable automatic seeding:

```powershell
dotnet user-secrets set "DevelopmentSeeder:Enabled" "false" --project ".\ExpenseTracker.Api\ExpenseTracker.Api.csproj"
```

Restart the API, log in normally, and inspect the dashboard and each feature.

Audit logs can be checked through:

```http
GET /api/v1/audit-trails?page=1&pageSize=50&sortBy=date&sortDirection=desc
```

If the other test data already exists and only the audit records are needed,
run `docs/SQL/SEED-TESTUSER1-AUDIT-LOGS.sql` in SQL Server Management Studio.
The script is idempotent and reports how many records it inserted.

## Important behavior

- If the configured email already exists, no password is needed.
- If the email does not exist, the seeder requires
  `DevelopmentSeeder:Password` or the user must be registered first.
- The seeder must never be enabled in production.
- The seeder does not delete existing user data.
