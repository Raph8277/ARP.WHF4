using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Wfrp4.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddArmesReference : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ArmesReference",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Nom = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Groupe = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TypeArme = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Prix = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Encombrement = table.Column<int>(type: "integer", nullable: false),
                    Dommage = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Disponibilite = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Longueur = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Portee = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Qualites = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    Defauts = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    DeuxMains = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArmesReference", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ArmesReference_Code",
                table: "ArmesReference",
                column: "Code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ArmesReference");
        }
    }
}
