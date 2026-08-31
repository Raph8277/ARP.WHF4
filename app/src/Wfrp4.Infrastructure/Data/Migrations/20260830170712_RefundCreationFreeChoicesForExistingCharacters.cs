using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wfrp4.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RefundCreationFreeChoicesForExistingCharacters : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                WITH refunds AS (
                    SELECT "PersonnageId", SUM(-"Montant") AS "MontantRembourse"
                    FROM "HistoriqueXPs"
                    WHERE "Montant" < 0
                      AND (
                          ("Type" = 2 AND "Notes" LIKE 'Création%avance(s) initiale(s)%')
                          OR ("Type" = 3 AND "Notes" LIKE 'Création%talent initial%')
                      )
                    GROUP BY "PersonnageId"
                )
                UPDATE "Personnages" p
                SET "XpDepense" = GREATEST(0, p."XpDepense" - r."MontantRembourse"),
                    "UpdatedAt" = NOW()
                FROM refunds r
                WHERE p."Id" = r."PersonnageId"
                  AND NOT EXISTS (
                      SELECT 1
                      FROM "HistoriqueXPs" h
                      WHERE h."PersonnageId" = p."Id"
                        AND h."Notes" = 'Correction création - remboursement des choix gratuits.'
                  );
                """);

            migrationBuilder.Sql("""
                WITH refunds AS (
                    SELECT "PersonnageId", SUM(-"Montant") AS "MontantRembourse"
                    FROM "HistoriqueXPs"
                    WHERE "Montant" < 0
                      AND (
                          ("Type" = 2 AND "Notes" LIKE 'Création%avance(s) initiale(s)%')
                          OR ("Type" = 3 AND "Notes" LIKE 'Création%talent initial%')
                      )
                    GROUP BY "PersonnageId"
                )
                INSERT INTO "HistoriqueXPs" ("PersonnageId", "AuteurKeycloakId", "Montant", "Type", "Cible", "Notes", "CreatedAt")
                SELECT p."Id",
                       p."KeycloakId",
                       r."MontantRembourse",
                       0,
                       'creation-gratuite',
                       'Correction création - remboursement des choix gratuits.',
                       NOW()
                FROM "Personnages" p
                INNER JOIN refunds r ON p."Id" = r."PersonnageId"
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM "HistoriqueXPs" h
                    WHERE h."PersonnageId" = p."Id"
                      AND h."Notes" = 'Correction création - remboursement des choix gratuits.'
                  );
                """);

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
            migrationBuilder.Sql("""
                DELETE FROM "HistoriqueXPs"
                WHERE "Notes" = 'Correction création - remboursement des choix gratuits.';
                """);
        }
    }
}
