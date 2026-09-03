using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wfrp4.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCreationFreeCounters : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AvancesCompetenceGratuitesRestantes",
                table: "Personnages",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TalentsGratuitsRestants",
                table: "Personnages",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql("""
                UPDATE "Personnages" p
                SET "AvancesCompetenceGratuitesRestantes" = GREATEST(0, 40 - COALESCE(c."TotalAvances", 0))
                FROM (
                    SELECT "PersonnageId", SUM("Avances") AS "TotalAvances"
                    FROM "PersonnageCompetences"
                    GROUP BY "PersonnageId"
                ) c
                WHERE p."Id" = c."PersonnageId";
                """);

            migrationBuilder.Sql("""
                UPDATE "Personnages" p
                SET "AvancesCompetenceGratuitesRestantes" = 40
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM "PersonnageCompetences" c
                    WHERE c."PersonnageId" = p."Id"
                );
                """);

            migrationBuilder.Sql("""
                UPDATE "Personnages" p
                SET "TalentsGratuitsRestants" = GREATEST(0, 1 - COALESCE(t."TotalTalents", 0))
                FROM (
                    SELECT "PersonnageId", SUM("Fois") AS "TotalTalents"
                    FROM "PersonnageTalents"
                    GROUP BY "PersonnageId"
                ) t
                WHERE p."Id" = t."PersonnageId";
                """);

            migrationBuilder.Sql("""
                UPDATE "Personnages" p
                SET "TalentsGratuitsRestants" = 1
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM "PersonnageTalents" t
                    WHERE t."PersonnageId" = p."Id"
                );
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvancesCompetenceGratuitesRestantes",
                table: "Personnages");

            migrationBuilder.DropColumn(
                name: "TalentsGratuitsRestants",
                table: "Personnages");
        }
    }
}
