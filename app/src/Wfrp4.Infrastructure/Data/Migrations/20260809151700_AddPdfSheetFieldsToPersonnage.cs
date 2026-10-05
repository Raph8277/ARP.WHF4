using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wfrp4.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPdfSheetFieldsToPersonnage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AmbitionCourtTerme",
                table: "Personnages",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AmbitionLongTerme",
                table: "Personnages",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CorruptionMutations",
                table: "Personnages",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GroupeMembres",
                table: "Personnages",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GroupeNom",
                table: "Personnages",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Psychologie",
                table: "Personnages",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AmbitionCourtTerme",
                table: "Personnages");

            migrationBuilder.DropColumn(
                name: "AmbitionLongTerme",
                table: "Personnages");

            migrationBuilder.DropColumn(
                name: "CorruptionMutations",
                table: "Personnages");

            migrationBuilder.DropColumn(
                name: "GroupeMembres",
                table: "Personnages");

            migrationBuilder.DropColumn(
                name: "GroupeNom",
                table: "Personnages");

            migrationBuilder.DropColumn(
                name: "Psychologie",
                table: "Personnages");
        }
    }
}
