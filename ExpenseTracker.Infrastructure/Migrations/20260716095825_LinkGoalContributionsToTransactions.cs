using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExpenseTracker.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class LinkGoalContributionsToTransactions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TransactionId",
                table: "GoalContributions",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_GoalContributions_TransactionId",
                table: "GoalContributions",
                column: "TransactionId",
                filter: "[TransactionId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_GoalContributions_Transactions_TransactionId",
                table: "GoalContributions",
                column: "TransactionId",
                principalTable: "Transactions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GoalContributions_Transactions_TransactionId",
                table: "GoalContributions");

            migrationBuilder.DropIndex(
                name: "IX_GoalContributions_TransactionId",
                table: "GoalContributions");

            migrationBuilder.DropColumn(
                name: "TransactionId",
                table: "GoalContributions");
        }
    }
}
