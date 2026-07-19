using ExpenseTracker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExpenseTracker.Infrastructure.Migrations;

[DbContext(typeof(ExpenseTrackerDbContext))]
[Migration("20260717130000_ReliabilitySecurityBaseline")]
public partial class ReliabilitySecurityBaseline : Migration
{
    private static readonly string[] ConcurrencyProtectedTables =
    [
        "Accounts",
        "Budgets",
        "Categories",
        "FinancialGoals",
        "GoalContributions",
        "Notifications",
        "RecurringTransactions",
        "Transactions",
        "Transfers"
    ];

    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "RefreshTokenHash",
            table: "AspNetUsers",
            type: "nvarchar(64)",
            maxLength: 64,
            nullable: true);

        migrationBuilder.AddColumn<DateTime>(
            name: "RefreshTokenCreatedAtUtc",
            table: "AspNetUsers",
            type: "datetime2",
            nullable: true);

        migrationBuilder.AddColumn<DateTime>(
            name: "RefreshTokenExpiresAtUtc",
            table: "AspNetUsers",
            type: "datetime2",
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_AspNetUsers_RefreshTokenHash",
            table: "AspNetUsers",
            column: "RefreshTokenHash",
            unique: true,
            filter: "[RefreshTokenHash] IS NOT NULL");

        foreach (var table in ConcurrencyProtectedTables)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: table,
                type: "rowversion",
                rowVersion: true,
                nullable: false);
        }

        migrationBuilder.CreateTable(
            name: "AuditLogs",
            columns: table => new
            {
                Id = table.Column<long>(
                        type: "bigint",
                        nullable: false)
                    .Annotation(
                        "SqlServer:Identity",
                        "1, 1"),
                UserId = table.Column<string>(
                    type: "nvarchar(450)",
                    maxLength: 450,
                    nullable: true),
                Method = table.Column<string>(
                    type: "nvarchar(10)",
                    maxLength: 10,
                    nullable: false),
                Route = table.Column<string>(
                    type: "nvarchar(500)",
                    maxLength: 500,
                    nullable: false),
                Action = table.Column<string>(
                    type: "nvarchar(100)",
                    maxLength: 100,
                    nullable: false),
                StatusCode = table.Column<int>(
                    type: "int",
                    nullable: false),
                TraceId = table.Column<string>(
                    type: "nvarchar(100)",
                    maxLength: 100,
                    nullable: false),
                CreatedAtUtc = table.Column<DateTime>(
                    type: "datetime2",
                    nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AuditLogs", audit => audit.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_AuditLogs_CreatedAtUtc",
            table: "AuditLogs",
            column: "CreatedAtUtc");

        migrationBuilder.CreateIndex(
            name: "IX_AuditLogs_UserId_CreatedAtUtc",
            table: "AuditLogs",
            columns: ["UserId", "CreatedAtUtc"]);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "AuditLogs");

        foreach (var table in ConcurrencyProtectedTables)
        {
            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: table);
        }

        migrationBuilder.DropIndex(
            name: "IX_AspNetUsers_RefreshTokenHash",
            table: "AspNetUsers");

        migrationBuilder.DropColumn(
            name: "RefreshTokenHash",
            table: "AspNetUsers");

        migrationBuilder.DropColumn(
            name: "RefreshTokenCreatedAtUtc",
            table: "AspNetUsers");

        migrationBuilder.DropColumn(
            name: "RefreshTokenExpiresAtUtc",
            table: "AspNetUsers");

    }
}
