using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wfrp4.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTitleMasteryLevels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NiveauMaitrise",
                table: "TitresQualificatifReference",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "NiveauMaitrise",
                table: "TitresBaseReference",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddCheckConstraint(
                name: "CK_TitresQualificatifReference_NiveauMaitrise",
                table: "TitresQualificatifReference",
                sql: "\"NiveauMaitrise\" BETWEEN 1 AND 4");

            migrationBuilder.AddCheckConstraint(
                name: "CK_TitresBaseReference_NiveauMaitrise",
                table: "TitresBaseReference",
                sql: "\"NiveauMaitrise\" BETWEEN 1 AND 4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_TitresQualificatifReference_NiveauMaitrise",
                table: "TitresQualificatifReference");

            migrationBuilder.DropCheckConstraint(
                name: "CK_TitresBaseReference_NiveauMaitrise",
                table: "TitresBaseReference");

            migrationBuilder.DropColumn(
                name: "NiveauMaitrise",
                table: "TitresQualificatifReference");

            migrationBuilder.DropColumn(
                name: "NiveauMaitrise",
                table: "TitresBaseReference");
        }
    }
}
