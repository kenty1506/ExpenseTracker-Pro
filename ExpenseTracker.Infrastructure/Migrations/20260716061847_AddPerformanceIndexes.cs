using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExpenseTracker.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPerformanceIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_GoalContributions_TransferId",
                table: "GoalContributions");

            migrationBuilder.CreateIndex(
                name: "IX_Transfers_UserId_FromAccountId_TransferDate",
                table: "Transfers",
                columns: new[] { "UserId", "FromAccountId", "TransferDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Transfers_UserId_ToAccountId_TransferDate",
                table: "Transfers",
                columns: new[] { "UserId", "ToAccountId", "TransferDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_UserId_CategoryId_Date",
                table: "Transactions",
                columns: new[] { "UserId", "CategoryId", "Date" });

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_UserId_Date",
                table: "Transactions",
                columns: new[] { "UserId", "Date" });

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_UserId_Type_Date",
                table: "Transactions",
                columns: new[] { "UserId", "Type", "Date" });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId_CreatedAt",
                table: "Notifications",
                columns: new[] { "UserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId_IsRead_CreatedAt",
                table: "Notifications",
                columns: new[] { "UserId", "IsRead", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_GoalContributions_TransferId_ContributionType",
                table: "GoalContributions",
                columns: new[] { "TransferId", "ContributionType" },
                filter: "[TransferId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialGoals_UserId_AccountId",
                table: "FinancialGoals",
                columns: new[] { "UserId", "AccountId" });

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_UserId_Type",
                table: "Accounts",
                columns: new[] { "UserId", "Type" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Transfers_UserId_FromAccountId_TransferDate",
                table: "Transfers");

            migrationBuilder.DropIndex(
                name: "IX_Transfers_UserId_ToAccountId_TransferDate",
                table: "Transfers");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_UserId_CategoryId_Date",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_UserId_Date",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_UserId_Type_Date",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_UserId_CreatedAt",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_UserId_IsRead_CreatedAt",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_GoalContributions_TransferId_ContributionType",
                table: "GoalContributions");

            migrationBuilder.DropIndex(
                name: "IX_FinancialGoals_UserId_AccountId",
                table: "FinancialGoals");

            migrationBuilder.DropIndex(
                name: "IX_Accounts_UserId_Type",
                table: "Accounts");

            migrationBuilder.CreateIndex(
                name: "IX_GoalContributions_TransferId",
                table: "GoalContributions",
                column: "TransferId");
        }
    }
}
