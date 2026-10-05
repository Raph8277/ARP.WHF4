using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wfrp4.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class SetBaseCompetencesMinimumFive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                INSERT INTO "PersonnageCompetences" ("PersonnageId", "CompetenceId", "Avances")
                SELECT p."Id", c."Id", 5
                FROM "Personnages" p
                CROSS JOIN "Competences" c
                WHERE c."EstAvancee" = FALSE
                  AND NOT EXISTS (
                      SELECT 1
                      FROM "PersonnageCompetences" pc
                      WHERE pc."PersonnageId" = p."Id"
                        AND pc."CompetenceId" = c."Id"
                  );
                """);

            migrationBuilder.Sql("""
                UPDATE "PersonnageCompetences" pc
                SET "Avances" = 5
                FROM "Competences" c
                WHERE pc."CompetenceId" = c."Id"
                  AND c."EstAvancee" = FALSE
                  AND pc."Avances" < 5;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Non reversible without risking loss of legitimate player progress.
        }
    }
}
