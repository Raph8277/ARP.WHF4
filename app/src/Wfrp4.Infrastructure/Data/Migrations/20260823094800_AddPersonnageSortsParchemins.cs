using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Wfrp4.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPersonnageSortsParchemins : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PersonnageParchemins",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PersonnageId = table.Column<int>(type: "integer", nullable: false),
                    SortReferenceId = table.Column<int>(type: "integer", nullable: false),
                    Quantite = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonnageParchemins", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PersonnageParchemins_Personnages_PersonnageId",
                        column: x => x.PersonnageId,
                        principalTable: "Personnages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PersonnageParchemins_SortsReference_SortReferenceId",
                        column: x => x.SortReferenceId,
                        principalTable: "SortsReference",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PersonnageSorts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PersonnageId = table.Column<int>(type: "integer", nullable: false),
                    SortReferenceId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonnageSorts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PersonnageSorts_Personnages_PersonnageId",
                        column: x => x.PersonnageId,
                        principalTable: "Personnages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PersonnageSorts_SortsReference_SortReferenceId",
                        column: x => x.SortReferenceId,
                        principalTable: "SortsReference",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PersonnageParchemins_PersonnageId_SortReferenceId",
                table: "PersonnageParchemins",
                columns: new[] { "PersonnageId", "SortReferenceId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PersonnageParchemins_SortReferenceId",
                table: "PersonnageParchemins",
                column: "SortReferenceId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonnageSorts_PersonnageId_SortReferenceId",
                table: "PersonnageSorts",
                columns: new[] { "PersonnageId", "SortReferenceId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PersonnageSorts_SortReferenceId",
                table: "PersonnageSorts",
                column: "SortReferenceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PersonnageParchemins");

            migrationBuilder.DropTable(
                name: "PersonnageSorts");
        }
    }
}
