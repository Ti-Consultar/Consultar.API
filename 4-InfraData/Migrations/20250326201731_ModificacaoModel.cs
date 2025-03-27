using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _4_InfraData.Migrations
{
    /// <inheritdoc />
    public partial class ModificacaoModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_CompanyUsers",
                table: "CompanyUsers");

            migrationBuilder.AddColumn<int>(
                name: "SubCompanyId",
                table: "CompanyUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_CompanyUsers",
                table: "CompanyUsers",
                columns: new[] { "UserId", "CompanyId", "SubCompanyId" });

            migrationBuilder.CreateIndex(
                name: "IX_CompanyUsers_SubCompanyId",
                table: "CompanyUsers",
                column: "SubCompanyId");

            migrationBuilder.AddForeignKey(
                name: "FK_CompanyUsers_SubCompanies_SubCompanyId",
                table: "CompanyUsers",
                column: "SubCompanyId",
                principalTable: "SubCompanies",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CompanyUsers_SubCompanies_SubCompanyId",
                table: "CompanyUsers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CompanyUsers",
                table: "CompanyUsers");

            migrationBuilder.DropIndex(
                name: "IX_CompanyUsers_SubCompanyId",
                table: "CompanyUsers");

            migrationBuilder.DropColumn(
                name: "SubCompanyId",
                table: "CompanyUsers");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CompanyUsers",
                table: "CompanyUsers",
                columns: new[] { "UserId", "CompanyId" });
        }
    }
}
