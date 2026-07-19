# Module and consolidated audit trails

ExpenseTracker Pro records privacy-safe metadata for every `POST`, `PUT`,
`PATCH`, and `DELETE` request. It does not store request bodies, passwords,
tokens, account balances, transaction amounts, or financial-goal values.

## Recorded fields

- Authenticated user ID
- Module and controller action
- Operation and HTTP method
- Entity or parent-record ID when available
- Route and HTTP status code
- Success or failure result
- Request duration in milliseconds
- Correlation/trace ID and UTC timestamp

## Authorization boundary

All audit API endpoints require a valid access token. Results are always
filtered by the authenticated user's ID. There is intentionally no cross-user
endpoint because the project does not yet have an Administrator authorization
policy.

## API endpoints

### Consolidated history

```http
GET /api/v1/audit-trails?page=1&pageSize=20&sortBy=date&sortDirection=desc
```

Optional filters:

- `module`
- `operation`
- `entityId`
- `statusCode`
- `succeeded`
- `fromUtc` and `toUtc`
- `traceId`
- `search`

Example showing failed writes across all modules:

```http
GET /api/v1/audit-trails?succeeded=false
```

### One module

```http
GET /api/v1/audit-trails/modules/Transfers
GET /api/v1/audit-trails/modules/FinancialGoals
GET /api/v1/audit-trails/modules/Accounts?entityId=28
```

### Module summary

```http
GET /api/v1/audit-trails/modules
GET /api/v1/audit-trails/modules?fromUtc=2026-07-01T00:00:00Z
```

### One audit event

```http
GET /api/v1/audit-trails/123
```

## Database migration

Apply migration `20260718120000_AddModuleAuditTrails`. In Development, normal
API startup applies pending migrations automatically. It can also be applied
manually from the solution directory:

```powershell
dotnet ef database update --project ".\ExpenseTracker.Infrastructure\ExpenseTracker.Infrastructure.csproj" --startup-project ".\ExpenseTracker.Api\ExpenseTracker.Api.csproj"
```

Existing audit records are retained and backfilled with module, operation, and
success metadata. Entity IDs and durations are available for newly captured
events.

## Direct SQL verification

```sql
DECLARE @Email NVARCHAR(256) = N'testuser1@gmail.com';

SELECT TOP (100)
    U.UserName,
    A.Module,
    A.Operation,
    A.EntityId,
    A.Method,
    A.Route,
    A.StatusCode,
    A.Succeeded,
    A.ElapsedMilliseconds,
    A.TraceId,
    A.CreatedAtUtc
FROM dbo.AuditLogs AS A
INNER JOIN dbo.AspNetUsers AS U
    ON U.Id = A.UserId
WHERE U.NormalizedEmail = UPPER(@Email)
ORDER BY A.CreatedAtUtc DESC;
```

## Development sample audit logs

The development data seeder includes 18 sample events for its configured user.
They cover the main modules, entity filters, multiple HTTP methods, successful
responses, a not-found response, and a conflict response. No request bodies,
credentials, tokens, or financial values are stored in these samples.

To insert only these audit examples for `testuser1@gmail.com`, run:

```text
docs/SQL/SEED-TESTUSER1-AUDIT-LOGS.sql
```

The seed operation is safe to rerun because each record has a stable trace ID.

## Retention

The current version preserves audit events indefinitely. Before production,
define a retention and archival policy based on operational and legal needs.
