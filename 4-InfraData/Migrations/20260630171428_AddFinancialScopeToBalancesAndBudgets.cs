using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _4_InfraData.Migrations
{
    /// <inheritdoc />
    public partial class AddFinancialScopeToBalancesAndBudgets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GroupId",
                table: "Balancete",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                table: "Balancete",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SubCompanyId",
                table: "Balancete",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GroupId",
                table: "Budget",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                table: "Budget",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SubCompanyId",
                table: "Budget",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "UX_AccountPlans_CanonicalGroup",
                table: "AccountPlans",
                column: "GroupId",
                unique: true,
                filter: "[CompanyId] IS NULL AND [SubCompanyId] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Balancete_CompanyId",
                table: "Balancete",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Balancete_FinancialScope_Period",
                table: "Balancete",
                columns: new[] { "GroupId", "CompanyId", "SubCompanyId", "DateYear", "DateMonth" });

            migrationBuilder.CreateIndex(
                name: "IX_Balancete_SubCompanyId",
                table: "Balancete",
                column: "SubCompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Budget_CompanyId",
                table: "Budget",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Budget_FinancialScope_Period",
                table: "Budget",
                columns: new[] { "GroupId", "CompanyId", "SubCompanyId", "DateYear", "DateMonth" });

            migrationBuilder.CreateIndex(
                name: "IX_Budget_SubCompanyId",
                table: "Budget",
                column: "SubCompanyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Balancete_Companies_CompanyId",
                table: "Balancete",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Balancete_Groups_GroupId",
                table: "Balancete",
                column: "GroupId",
                principalTable: "Groups",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Balancete_SubCompanies_SubCompanyId",
                table: "Balancete",
                column: "SubCompanyId",
                principalTable: "SubCompanies",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Budget_Companies_CompanyId",
                table: "Budget",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Budget_Groups_GroupId",
                table: "Budget",
                column: "GroupId",
                principalTable: "Groups",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Budget_SubCompanies_SubCompanyId",
                table: "Budget",
                column: "SubCompanyId",
                principalTable: "SubCompanies",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Balancete_Companies_CompanyId",
                table: "Balancete");

            migrationBuilder.DropForeignKey(
                name: "FK_Balancete_Groups_GroupId",
                table: "Balancete");

            migrationBuilder.DropForeignKey(
                name: "FK_Balancete_SubCompanies_SubCompanyId",
                table: "Balancete");

            migrationBuilder.DropForeignKey(
                name: "FK_Budget_Companies_CompanyId",
                table: "Budget");

            migrationBuilder.DropForeignKey(
                name: "FK_Budget_Groups_GroupId",
                table: "Budget");

            migrationBuilder.DropForeignKey(
                name: "FK_Budget_SubCompanies_SubCompanyId",
                table: "Budget");

            migrationBuilder.DropIndex(
                name: "UX_AccountPlans_CanonicalGroup",
                table: "AccountPlans");

            migrationBuilder.DropIndex(
                name: "IX_Balancete_CompanyId",
                table: "Balancete");

            migrationBuilder.DropIndex(
                name: "IX_Balancete_FinancialScope_Period",
                table: "Balancete");

            migrationBuilder.DropIndex(
                name: "IX_Balancete_SubCompanyId",
                table: "Balancete");

            migrationBuilder.DropIndex(
                name: "IX_Budget_CompanyId",
                table: "Budget");

            migrationBuilder.DropIndex(
                name: "IX_Budget_FinancialScope_Period",
                table: "Budget");

            migrationBuilder.DropIndex(
                name: "IX_Budget_SubCompanyId",
                table: "Budget");

            migrationBuilder.DropColumn(
                name: "GroupId",
                table: "Balancete");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "Balancete");

            migrationBuilder.DropColumn(
                name: "SubCompanyId",
                table: "Balancete");

            migrationBuilder.DropColumn(
                name: "GroupId",
                table: "Budget");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "Budget");

            migrationBuilder.DropColumn(
                name: "SubCompanyId",
                table: "Budget");
        }
    }
}
