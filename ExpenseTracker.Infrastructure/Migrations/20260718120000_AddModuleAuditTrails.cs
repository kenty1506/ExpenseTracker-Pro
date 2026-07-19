using ExpenseTracker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExpenseTracker.Infrastructure.Migrations;

[DbContext(typeof(ExpenseTrackerDbContext))]
[Migration("20260718120000_AddModuleAuditTrails")]
public partial class AddModuleAuditTrails : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<long>(
            name: "ElapsedMilliseconds",
            table: "AuditLogs",
            type: "bigint",
            nullable: false,
            defaultValue: 0L);

        migrationBuilder.AddColumn<string>(
            name: "EntityId",
            table: "AuditLogs",
            type: "nvarchar(100)",
            maxLength: 100,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "Module",
            table: "AuditLogs",
            type: "nvarchar(100)",
            maxLength: 100,
            nullable: false,
            defaultValue: "System");

        migrationBuilder.AddColumn<string>(
            name: "Operation",
            table: "AuditLogs",
            type: "nvarchar(50)",
            maxLength: 50,
            nullable: false,
            defaultValue: "Execute");

        migrationBuilder.AddColumn<bool>(
            name: "Succeeded",
            table: "AuditLogs",
            type: "bit",
            nullable: false,
            defaultValue: false);

        migrationBuilder.Sql(
            """
            UPDATE dbo.AuditLogs
            SET
                Module = CASE
                    WHEN CHARINDEX('.', Action) > 1
                        THEN LEFT(Action, CHARINDEX('.', Action) - 1)
                    ELSE 'System'
                END,
                Operation = CASE UPPER(Method)
                    WHEN 'POST' THEN 'Create'
                    WHEN 'PUT' THEN 'Update'
                    WHEN 'PATCH' THEN 'Update'
                    WHEN 'DELETE' THEN 'Delete'
                    ELSE 'Execute'
                END,
                Succeeded = CASE
                    WHEN StatusCode >= 200 AND StatusCode < 400 THEN 1
                    ELSE 0
                END;
            """);

        migrationBuilder.CreateIndex(
            name: "IX_AuditLogs_UserId_Module_CreatedAtUtc",
            table: "AuditLogs",
            columns: new[]
            {
                "UserId",
                "Module",
                "CreatedAtUtc"
            });

        migrationBuilder.CreateIndex(
            name: "IX_AuditLogs_UserId_Module_EntityId",
            table: "AuditLogs",
            columns: new[]
            {
                "UserId",
                "Module",
                "EntityId"
            });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_AuditLogs_UserId_Module_CreatedAtUtc",
            table: "AuditLogs");

        migrationBuilder.DropIndex(
            name: "IX_AuditLogs_UserId_Module_EntityId",
            table: "AuditLogs");

        migrationBuilder.DropColumn(
            name: "ElapsedMilliseconds",
            table: "AuditLogs");

        migrationBuilder.DropColumn(
            name: "EntityId",
            table: "AuditLogs");

        migrationBuilder.DropColumn(
            name: "Module",
            table: "AuditLogs");

        migrationBuilder.DropColumn(
            name: "Operation",
            table: "AuditLogs");

        migrationBuilder.DropColumn(
            name: "Succeeded",
            table: "AuditLogs");
    }
}
