using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _4_InfraData.Migrations
{
    /// <inheritdoc />
    public partial class ModificacaoModel4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_CompanyUsers",
                table: "CompanyUsers");

            migrationBuilder.DropColumn(
                name: "Role",
                table: "CompanyUsers");

            migrationBuilder.AddColumn<int>(
                name: "PermissionId",
                table: "CompanyUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_CompanyUsers",
                table: "CompanyUsers",
                columns: new[] { "UserId", "CompanyId" });

            migrationBuilder.CreateTable(
                name: "Permissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CompanyUsers_PermissionId",
                table: "CompanyUsers",
                column: "PermissionId");

            migrationBuilder.AddForeignKey(
                name: "FK_CompanyUsers_Permissions_PermissionId",
                table: "CompanyUsers",
                column: "PermissionId",
                principalTable: "Permissions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CompanyUsers_Permissions_PermissionId",
                table: "CompanyUsers");

            migrationBuilder.DropTable(
                name: "Permissions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CompanyUsers",
                table: "CompanyUsers");

            migrationBuilder.DropIndex(
                name: "IX_CompanyUsers_PermissionId",
                table: "CompanyUsers");

            migrationBuilder.DropColumn(
                name: "PermissionId",
                table: "CompanyUsers");

            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "CompanyUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CompanyUsers",
                table: "CompanyUsers",
                columns: new[] { "UserId", "CompanyId", "SubCompanyId" });
        }
    }
}
