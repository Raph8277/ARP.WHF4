using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Wfrp4.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCharacterTitleReferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TitresBaseReference",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Libelle = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Ordre = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TitresBaseReference", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TitresQualificatifReference",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Libelle = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Ordre = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TitresQualificatifReference", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TitresBaseReference_Code",
                table: "TitresBaseReference",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TitresQualificatifReference_Code",
                table: "TitresQualificatifReference",
                column: "Code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TitresBaseReference");

            migrationBuilder.DropTable(
                name: "TitresQualificatifReference");
        }
    }
}
