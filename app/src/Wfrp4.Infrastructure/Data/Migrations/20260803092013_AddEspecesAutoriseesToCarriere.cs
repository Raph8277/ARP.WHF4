using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wfrp4.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEspecesAutoriseesToCarriere : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EspecesAutorisees",
                table: "Carrieres",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EspecesAutorisees",
                table: "Carrieres");
        }
    }
}
