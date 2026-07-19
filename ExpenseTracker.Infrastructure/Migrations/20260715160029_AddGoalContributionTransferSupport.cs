using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExpenseTracker.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGoalContributionTransferSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_GoalContributions_FinancialGoalId",
                table: "GoalContributions");

            migrationBuilder.AddColumn<int>(
                name: "ContributionType",
                table: "GoalContributions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TransferId",
                table: "GoalContributions",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_GoalContributions_FinancialGoalId_TransferId_ContributionType",
                table: "GoalContributions",
                columns: new[] { "FinancialGoalId", "TransferId", "ContributionType" },
                unique: true,
                filter: "[TransferId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_GoalContributions_TransferId",
                table: "GoalContributions",
                column: "TransferId");

            migrationBuilder.AddForeignKey(
                name: "FK_GoalContributions_Transfers_TransferId",
                table: "GoalContributions",
                column: "TransferId",
                principalTable: "Transfers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GoalContributions_Transfers_TransferId",
                table: "GoalContributions");

            migrationBuilder.DropIndex(
                name: "IX_GoalContributions_FinancialGoalId_TransferId_ContributionType",
                table: "GoalContributions");

            migrationBuilder.DropIndex(
                name: "IX_GoalContributions_TransferId",
                table: "GoalContributions");

            migrationBuilder.DropColumn(
                name: "ContributionType",
                table: "GoalContributions");

            migrationBuilder.DropColumn(
                name: "TransferId",
                table: "GoalContributions");

            migrationBuilder.CreateIndex(
                name: "IX_GoalContributions_FinancialGoalId",
                table: "GoalContributions",
                column: "FinancialGoalId");
        }
    }
}
