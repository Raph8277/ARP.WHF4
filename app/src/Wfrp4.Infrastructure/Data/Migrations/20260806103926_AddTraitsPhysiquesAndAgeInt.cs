using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wfrp4.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTraitsPhysiquesAndAgeInt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                ALTER TABLE "Personnages"
                ALTER COLUMN "Age" TYPE integer
                USING CASE WHEN "Age" ~ '^\d+$' THEN "Age"::integer ELSE NULL END;
                """);

            migrationBuilder.AddColumn<string>(
                name: "TraitsPhysiques",
                table: "Especes",
                type: "jsonb",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TraitsPhysiques",
                table: "Especes");

            migrationBuilder.AlterColumn<string>(
                name: "Age",
                table: "Personnages",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);
        }
    }
}
