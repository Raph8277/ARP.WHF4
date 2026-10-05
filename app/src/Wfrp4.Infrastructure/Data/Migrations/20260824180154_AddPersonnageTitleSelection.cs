using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wfrp4.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPersonnageTitleSelection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TitreBaseReferenceId",
                table: "Personnages",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TitreQualificatifReferenceId",
                table: "Personnages",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Personnages_TitreBaseReferenceId",
                table: "Personnages",
                column: "TitreBaseReferenceId");

            migrationBuilder.CreateIndex(
                name: "IX_Personnages_TitreQualificatifReferenceId",
                table: "Personnages",
                column: "TitreQualificatifReferenceId");

            migrationBuilder.AddForeignKey(
                name: "FK_Personnages_TitresBaseReference_TitreBaseReferenceId",
                table: "Personnages",
                column: "TitreBaseReferenceId",
                principalTable: "TitresBaseReference",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Personnages_TitresQualificatifReference_TitreQualificatifRe~",
                table: "Personnages",
                column: "TitreQualificatifReferenceId",
                principalTable: "TitresQualificatifReference",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Personnages_TitresBaseReference_TitreBaseReferenceId",
                table: "Personnages");

            migrationBuilder.DropForeignKey(
                name: "FK_Personnages_TitresQualificatifReference_TitreQualificatifRe~",
                table: "Personnages");

            migrationBuilder.DropIndex(
                name: "IX_Personnages_TitreBaseReferenceId",
                table: "Personnages");

            migrationBuilder.DropIndex(
                name: "IX_Personnages_TitreQualificatifReferenceId",
                table: "Personnages");

            migrationBuilder.DropColumn(
                name: "TitreBaseReferenceId",
                table: "Personnages");

            migrationBuilder.DropColumn(
                name: "TitreQualificatifReferenceId",
                table: "Personnages");
        }
    }
}
