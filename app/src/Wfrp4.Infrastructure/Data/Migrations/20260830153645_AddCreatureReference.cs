using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Wfrp4.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCreatureReference : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CreaturesReference",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Nom = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Categorie = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    M = table.Column<int>(type: "integer", nullable: false),
                    CC = table.Column<int>(type: "integer", nullable: false),
                    CT = table.Column<int>(type: "integer", nullable: false),
                    F = table.Column<int>(type: "integer", nullable: false),
                    E = table.Column<int>(type: "integer", nullable: false),
                    I = table.Column<int>(type: "integer", nullable: false),
                    Ag = table.Column<int>(type: "integer", nullable: false),
                    Dex = table.Column<int>(type: "integer", nullable: false),
                    Int = table.Column<int>(type: "integer", nullable: false),
                    FM = table.Column<int>(type: "integer", nullable: false),
                    Soc = table.Column<int>(type: "integer", nullable: false),
                    B = table.Column<int>(type: "integer", nullable: false),
                    Traits = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    TraitsOptionnels = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Page = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CreaturesReference", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CreaturesReference_Code",
                table: "CreaturesReference",
                column: "Code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CreaturesReference");
        }
    }
}
