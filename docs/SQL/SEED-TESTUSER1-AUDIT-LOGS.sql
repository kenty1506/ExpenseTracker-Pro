SET NOCOUNT ON;

DECLARE @Email NVARCHAR(256) = N'testuser1@gmail.com';
DECLARE @UserId NVARCHAR(450);
DECLARE @Now DATETIME2(7) = SYSUTCDATETIME();

SELECT @UserId = U.Id
FROM dbo.AspNetUsers AS U
WHERE U.NormalizedEmail = UPPER(@Email);

IF @UserId IS NULL
BEGIN
    THROW 50001, 'The user testuser1@gmail.com does not exist.', 1;
END;

DECLARE @AccountId NVARCHAR(100) =
(
    SELECT TOP (1) CONVERT(NVARCHAR(100), A.Id)
    FROM dbo.Accounts AS A
    WHERE A.UserId = @UserId
    ORDER BY A.Id
);

DECLARE @CategoryId NVARCHAR(100) =
(
    SELECT TOP (1) CONVERT(NVARCHAR(100), C.Id)
    FROM dbo.Categories AS C
    WHERE C.UserId = @UserId
    ORDER BY C.Id
);

DECLARE @TransactionId NVARCHAR(100) =
(
    SELECT TOP (1) CONVERT(NVARCHAR(100), T.Id)
    FROM dbo.Transactions AS T
    WHERE T.UserId = @UserId
    ORDER BY T.Id
);

DECLARE @TransferId NVARCHAR(100) =
(
    SELECT TOP (1) CONVERT(NVARCHAR(100), T.Id)
    FROM dbo.Transfers AS T
    WHERE T.UserId = @UserId
    ORDER BY T.Id
);

DECLARE @BudgetId NVARCHAR(100) =
(
    SELECT TOP (1) CONVERT(NVARCHAR(100), B.Id)
    FROM dbo.Budgets AS B
    WHERE B.UserId = @UserId
    ORDER BY B.Id
);

DECLARE @RecurringId NVARCHAR(100) =
(
    SELECT TOP (1) CONVERT(NVARCHAR(100), R.Id)
    FROM dbo.RecurringTransactions AS R
    WHERE R.UserId = @UserId
    ORDER BY R.Id
);

DECLARE @GoalId NVARCHAR(100) =
(
    SELECT TOP (1) CONVERT(NVARCHAR(100), G.Id)
    FROM dbo.FinancialGoals AS G
    WHERE G.UserId = @UserId
    ORDER BY G.Id
);

DECLARE @NotificationId NVARCHAR(100) =
(
    SELECT TOP (1) CONVERT(NVARCHAR(100), N.Id)
    FROM dbo.Notifications AS N
    WHERE N.UserId = @UserId
    ORDER BY N.Id
);

DECLARE @SampleLogs TABLE
(
    TraceId NVARCHAR(100) NOT NULL PRIMARY KEY,
    Method NVARCHAR(10) NOT NULL,
    Module NVARCHAR(100) NOT NULL,
    Operation NVARCHAR(50) NOT NULL,
    EntityId NVARCHAR(100) NULL,
    Route NVARCHAR(500) NOT NULL,
    Action NVARCHAR(100) NOT NULL,
    StatusCode INT NOT NULL,
    Succeeded BIT NOT NULL,
    ElapsedMilliseconds BIGINT NOT NULL,
    CreatedAtUtc DATETIME2(7) NOT NULL
);

INSERT INTO @SampleLogs
(
    TraceId,
    Method,
    Module,
    Operation,
    EntityId,
    Route,
    Action,
    StatusCode,
    Succeeded,
    ElapsedMilliseconds,
    CreatedAtUtc
)
VALUES
    (N'dev-seed-audit-01', N'POST', N'Auth', N'Login', NULL,
     N'/api/v1/auth/login', N'Auth.Login', 200, 1, 142, DATEADD(DAY, -14, @Now)),

    (N'dev-seed-audit-02', N'POST', N'Accounts', N'Create', @AccountId,
     N'/api/v1/accounts', N'Accounts.Create', 201, 1, 96, DATEADD(DAY, -13, @Now)),

    (N'dev-seed-audit-03', N'POST', N'Categories', N'Create', @CategoryId,
     N'/api/v1/categories', N'Categories.Create', 201, 1, 54, DATEADD(DAY, -12, @Now)),

    (N'dev-seed-audit-04', N'POST', N'Transactions', N'Create', @TransactionId,
     N'/api/v1/transactions', N'Transactions.Create', 201, 1, 88, DATEADD(DAY, -11, @Now)),

    (N'dev-seed-audit-05', N'PUT', N'Transactions', N'Update', @TransactionId,
     CONCAT(N'/api/v1/transactions/', @TransactionId), N'Transactions.Update',
     200, 1, 73, DATEADD(DAY, -10, @Now)),

    (N'dev-seed-audit-06', N'POST', N'Transfers', N'Create', @TransferId,
     N'/api/v1/transfers', N'Transfers.Create', 201, 1, 105, DATEADD(DAY, -9, @Now)),

    (N'dev-seed-audit-07', N'PUT', N'Transfers', N'Update', @TransferId,
     CONCAT(N'/api/v1/transfers/', @TransferId), N'Transfers.Update',
     200, 1, 91, DATEADD(HOUR, -210, @Now)),

    (N'dev-seed-audit-08', N'POST', N'Budgets', N'Create', @BudgetId,
     N'/api/v1/budgets', N'Budgets.Create', 201, 1, 67, DATEADD(DAY, -8, @Now)),

    (N'dev-seed-audit-09', N'PUT', N'Budgets', N'Update', @BudgetId,
     CONCAT(N'/api/v1/budgets/', @BudgetId), N'Budgets.Update',
     200, 1, 61, DATEADD(DAY, -7, @Now)),

    (N'dev-seed-audit-10', N'POST', N'RecurringTransactions', N'Create', @RecurringId,
     N'/api/v1/recurring-transactions', N'RecurringTransactions.Create',
     201, 1, 84, DATEADD(DAY, -6, @Now)),

    (N'dev-seed-audit-11', N'POST', N'RecurringTransactions', N'GenerateDue', NULL,
     N'/api/v1/recurring-transactions/generate-due', N'RecurringTransactions.GenerateDue',
     200, 1, 126, DATEADD(DAY, -5, @Now)),

    (N'dev-seed-audit-12', N'POST', N'FinancialGoals', N'Create', @GoalId,
     N'/api/v1/financial-goals', N'FinancialGoals.Create',
     201, 1, 113, DATEADD(HOUR, -98, @Now)),

    (N'dev-seed-audit-13', N'POST', N'FinancialGoals', N'AddContribution', @GoalId,
     CONCAT(N'/api/v1/financial-goals/', @GoalId, N'/contributions'),
     N'FinancialGoals.AddContribution', 200, 1, 119, DATEADD(DAY, -4, @Now)),

    (N'dev-seed-audit-14', N'PATCH', N'Notifications', N'MarkAsRead', @NotificationId,
     CONCAT(N'/api/v1/notifications/', @NotificationId, N'/read'),
     N'Notifications.MarkAsRead', 204, 1, 42, DATEADD(DAY, -3, @Now)),

    (N'dev-seed-audit-15', N'POST', N'Notifications', N'Generate', NULL,
     N'/api/v1/notifications/generate', N'Notifications.Generate',
     200, 1, 157, DATEADD(DAY, -2, @Now)),

    (N'dev-seed-audit-16', N'DELETE', N'Transactions', N'Delete', N'999999',
     N'/api/v1/transactions/999999', N'Transactions.Delete',
     404, 0, 39, DATEADD(HOUR, -6, @Now)),

    (N'dev-seed-audit-17', N'POST', N'Transfers', N'Create', NULL,
     N'/api/v1/transfers', N'Transfers.Create',
     409, 0, 64, DATEADD(HOUR, -2, @Now)),

    (N'dev-seed-audit-18', N'POST', N'Auth', N'Logout', NULL,
     N'/api/v1/auth/logout', N'Auth.Logout',
     204, 1, 31, DATEADD(MINUTE, -30, @Now));

INSERT INTO dbo.AuditLogs
(
    UserId,
    Method,
    Module,
    Operation,
    EntityId,
    Route,
    Action,
    StatusCode,
    Succeeded,
    ElapsedMilliseconds,
    TraceId,
    CreatedAtUtc
)
SELECT
    @UserId,
    S.Method,
    S.Module,
    S.Operation,
    S.EntityId,
    S.Route,
    S.Action,
    S.StatusCode,
    S.Succeeded,
    S.ElapsedMilliseconds,
    S.TraceId,
    S.CreatedAtUtc
FROM @SampleLogs AS S
WHERE NOT EXISTS
(
    SELECT 1
    FROM dbo.AuditLogs AS A
    WHERE A.UserId = @UserId
      AND A.TraceId = S.TraceId
);

DECLARE @InsertedRows INT = @@ROWCOUNT;

SELECT
    @Email AS UserEmail,
    @InsertedRows AS InsertedAuditLogs,
    COUNT(*) AS TotalSampleAuditLogs
FROM dbo.AuditLogs AS A
WHERE A.UserId = @UserId
  AND A.TraceId LIKE N'dev-seed-audit-%';
