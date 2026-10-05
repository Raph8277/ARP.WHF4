using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wfrp4.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTalentsRevenuToNiveauCarriere : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TalentsRevenu",
                table: "NiveauCarrieres",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TalentsRevenu",
                table: "NiveauCarrieres");
        }
    }
}
