using ExpenseTracker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExpenseTracker.Infrastructure.Migrations;

[DbContext(typeof(ExpenseTrackerDbContext))]
[Migration("20260718143000_AddExternalAuthenticationSupport")]
public partial class AddExternalAuthenticationSupport : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "EmailIndex",
            table: "AspNetUsers");

        migrationBuilder.AlterColumn<string>(
            name: "PhoneNumber",
            table: "AspNetUsers",
            type: "nvarchar(32)",
            maxLength: 32,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(max)",
            oldNullable: true);

        migrationBuilder.CreateIndex(
            name: "EmailIndex",
            table: "AspNetUsers",
            column: "NormalizedEmail",
            unique: true,
            filter: "[NormalizedEmail] IS NOT NULL");

        migrationBuilder.CreateIndex(
            name: "IX_AspNetUsers_PhoneNumber",
            table: "AspNetUsers",
            column: "PhoneNumber",
            unique: true,
            filter: "[PhoneNumber] IS NOT NULL");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "EmailIndex",
            table: "AspNetUsers");

        migrationBuilder.DropIndex(
            name: "IX_AspNetUsers_PhoneNumber",
            table: "AspNetUsers");

        migrationBuilder.AlterColumn<string>(
            name: "PhoneNumber",
            table: "AspNetUsers",
            type: "nvarchar(max)",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(32)",
            oldMaxLength: 32,
            oldNullable: true);

        migrationBuilder.CreateIndex(
            name: "EmailIndex",
            table: "AspNetUsers",
            column: "NormalizedEmail");
    }
}
