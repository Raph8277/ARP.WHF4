using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Wfrp4.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSortsReference : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SortsReference",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    Nom = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Categorie = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Domaine = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    Cn = table.Column<int>(type: "integer", nullable: true),
                    Portee = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    Cible = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    Duree = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    Resume = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SortsReference", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SortsReference_Code",
                table: "SortsReference",
                column: "Code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SortsReference");
        }
    }
}
