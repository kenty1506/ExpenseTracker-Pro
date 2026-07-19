# Budget Forecast and Expense Calendar

These features are read-only analytics built from the authenticated user's existing
transactions, budgets, categories, accounts, and recurring transactions. They do not
add tables or columns, so no new Entity Framework migration is required.

## Detailed expense calendar

```http
GET /api/v1/reports/expense-calendar?year=2026&month=7
Authorization: Bearer <access-token>
```

The endpoint returns every day in the selected month, including days without spending.
The root response contains monthly totals, averages, the largest spending day, and the
previous/next month values needed by calendar navigation.

Each item in `days` includes:

- `date`, `day`, `dayOfWeek`, and `isToday`
- `totalExpense`, `transactionCount`, and `largestExpense`
- `topCategory`
- `transactions`, ordered by amount and then date

Each calendar transaction includes `id`, `date`, `amount`, `notes`, category metadata,
account metadata, and `isRecurring`. The frontend can use the returned `id` as the route
parameter when a user clicks an expense. For example, transaction `42` can open the
frontend route for transaction `42`; the API does not impose a frontend route format.

The existing endpoint below remains available for clients that only need daily totals:

```http
GET /api/v1/reports/calendar?year=2026&month=7
```

## Budget forecast

```http
GET /api/v1/budgets/forecast?months=3&historyMonths=3&safetyBufferPercent=10
Authorization: Bearer <access-token>
```

Query options:

| Option | Allowed | Default | Purpose |
| --- | ---: | ---: | --- |
| `months` | 3–12 | 3 | Number of future full months returned |
| `historyMonths` | 1–12 | 3 | Completed months used for historical averages |
| `safetyBufferPercent` | 0–50 | 10 | Extra amount added to recommended category budgets |

The forecast starts on the next full calendar month. The current partial month is excluded
from the historical average so an unfinished month does not artificially reduce the result.

For each expense category, the service calculates:

1. Average expense across the selected completed history months. Months with no activity
   count as zero.
2. Active recurring expense scheduled for the forecast month.
3. Forecast expense as the larger of those two values. Using the larger value instead of
   adding both avoids counting recurring transactions twice when they already appear in
   transaction history.
4. Planned budget from an exact budget for that future month. If none exists, the latest
   earlier category budget is marked `Carried Forward`.
5. Recommended budget as forecast expense plus the selected safety buffer.

Forecast income follows the same conservative rule: it uses the larger of completed-month
average income and active recurring income for that forecast month.

### Risk levels

| Risk | Meaning |
| --- | --- |
| `On Track` | Forecast uses less than 80% of the planned category budget |
| `Warning` | Forecast uses 80% through 100% of the planned budget |
| `Over Budget` | Forecast is greater than the planned budget |
| `Unbudgeted` | Forecast spending exists but no saved category budget applies |
| `No Activity` | No spending is forecast for the category or month |

The response also returns `methodology` and `warnings`. Displaying these near the forecast
helps users understand that it is a planning estimate rather than a guaranteed future result.

## Test in Swagger

1. Start the API and open `/swagger`.
2. Sign in through `POST /api/v1/auth/login` and copy `accessToken`.
3. Select **Authorize** and enter `Bearer <accessToken>`.
4. Run **GET `/api/v1/reports/expense-calendar`** with a year and month containing expenses.
5. Confirm that a spending day contains transaction IDs in its `transactions` array.
6. Run **GET `/api/v1/budgets/forecast`** with the default values.
7. Confirm that three future months are returned and inspect each category's budget source,
   recommended budget, projected remaining amount, and risk level.

## Build and test

Run these commands one at a time from the solution root in PowerShell:

```powershell
dotnet restore ".\ExpenseTracker.Pro.sln"
dotnet build ".\ExpenseTracker.Pro.sln"
dotnet test ".\ExpenseTracker.Pro.sln" --no-build
dotnet run --project ".\ExpenseTracker.Api\ExpenseTracker.Api.csproj"
```
