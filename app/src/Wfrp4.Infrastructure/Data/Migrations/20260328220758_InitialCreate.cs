using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Wfrp4.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Classes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Nom = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Classes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Competences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Nom = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Caracteristique = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    EstAvancee = table.Column<bool>(type: "boolean", nullable: false),
                    EstGroupee = table.Column<bool>(type: "boolean", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Competences", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Especes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Nom = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    MouvementBase = table.Column<int>(type: "integer", nullable: false),
                    CaracInitiales = table.Column<string>(type: "jsonb", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Especes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Talents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Nom = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    MaxFois = table.Column<int>(type: "integer", nullable: true),
                    Empilable = table.Column<bool>(type: "boolean", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Effet = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Talents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Carrieres",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Nom = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ClasseId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Carrieres", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Carrieres_Classes_ClasseId",
                        column: x => x.ClasseId,
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "NiveauCarrieres",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CarriereId = table.Column<int>(type: "integer", nullable: false),
                    Niveau = table.Column<int>(type: "integer", nullable: false),
                    Intitule = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Statut = table.Column<int>(type: "integer", nullable: false),
                    StatutNumerique = table.Column<int>(type: "integer", nullable: false),
                    AvancesCarac = table.Column<string>(type: "jsonb", nullable: true),
                    CompetenceRevenu = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NiveauCarrieres", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NiveauCarrieres_Carrieres_CarriereId",
                        column: x => x.CarriereId,
                        principalTable: "Carrieres",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Personnages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    KeycloakId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Nom = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    EspeceId = table.Column<int>(type: "integer", nullable: false),
                    CarriereCouranteId = table.Column<int>(type: "integer", nullable: true),
                    XpTotal = table.Column<int>(type: "integer", nullable: false),
                    XpDepense = table.Column<int>(type: "integer", nullable: false),
                    BlessuresMax = table.Column<int>(type: "integer", nullable: false),
                    Destin = table.Column<int>(type: "integer", nullable: false),
                    Fortune = table.Column<int>(type: "integer", nullable: false),
                    Resilience = table.Column<int>(type: "integer", nullable: false),
                    Resolution = table.Column<int>(type: "integer", nullable: false),
                    Mouvement = table.Column<int>(type: "integer", nullable: false),
                    Motivation = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    StatutSocial = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Age = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    CouleurYeux = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CouleurCheveux = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    TailleCm = table.Column<int>(type: "integer", nullable: true),
                    EstActif = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Personnages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Personnages_Especes_EspeceId",
                        column: x => x.EspeceId,
                        principalTable: "Especes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Personnages_NiveauCarrieres_CarriereCouranteId",
                        column: x => x.CarriereCouranteId,
                        principalTable: "NiveauCarrieres",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "HistoriqueXPs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PersonnageId = table.Column<int>(type: "integer", nullable: false),
                    AuteurKeycloakId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Montant = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Cible = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoriqueXPs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HistoriqueXPs_Personnages_PersonnageId",
                        column: x => x.PersonnageId,
                        principalTable: "Personnages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PersonnageCaracteristiques",
                columns: table => new
                {
                    PersonnageId = table.Column<int>(type: "integer", nullable: false),
                    Code = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    ValeurInitiale = table.Column<int>(type: "integer", nullable: false),
                    Avances = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonnageCaracteristiques", x => new { x.PersonnageId, x.Code });
                    table.ForeignKey(
                        name: "FK_PersonnageCaracteristiques_Personnages_PersonnageId",
                        column: x => x.PersonnageId,
                        principalTable: "Personnages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PersonnageCarrieres",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PersonnageId = table.Column<int>(type: "integer", nullable: false),
                    NiveauCarriereId = table.Column<int>(type: "integer", nullable: false),
                    EstCourante = table.Column<bool>(type: "boolean", nullable: false),
                    DateEntree = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateSortie = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonnageCarrieres", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PersonnageCarrieres_NiveauCarrieres_NiveauCarriereId",
                        column: x => x.NiveauCarriereId,
                        principalTable: "NiveauCarrieres",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PersonnageCarrieres_Personnages_PersonnageId",
                        column: x => x.PersonnageId,
                        principalTable: "Personnages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PersonnageCompetences",
                columns: table => new
                {
                    PersonnageId = table.Column<int>(type: "integer", nullable: false),
                    CompetenceId = table.Column<int>(type: "integer", nullable: false),
                    Avances = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonnageCompetences", x => new { x.PersonnageId, x.CompetenceId });
                    table.ForeignKey(
                        name: "FK_PersonnageCompetences_Competences_CompetenceId",
                        column: x => x.CompetenceId,
                        principalTable: "Competences",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PersonnageCompetences_Personnages_PersonnageId",
                        column: x => x.PersonnageId,
                        principalTable: "Personnages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PersonnagePartages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PersonnageId = table.Column<int>(type: "integer", nullable: false),
                    MjKeycloakId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Permission = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonnagePartages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PersonnagePartages_Personnages_PersonnageId",
                        column: x => x.PersonnageId,
                        principalTable: "Personnages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PersonnageTalents",
                columns: table => new
                {
                    PersonnageId = table.Column<int>(type: "integer", nullable: false),
                    TalentId = table.Column<int>(type: "integer", nullable: false),
                    Fois = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonnageTalents", x => new { x.PersonnageId, x.TalentId });
                    table.ForeignKey(
                        name: "FK_PersonnageTalents_Personnages_PersonnageId",
                        column: x => x.PersonnageId,
                        principalTable: "Personnages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PersonnageTalents_Talents_TalentId",
                        column: x => x.TalentId,
                        principalTable: "Talents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Carrieres_ClasseId",
                table: "Carrieres",
                column: "ClasseId");

            migrationBuilder.CreateIndex(
                name: "IX_Carrieres_Code",
                table: "Carrieres",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Classes_Code",
                table: "Classes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Competences_Code",
                table: "Competences",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Especes_Code",
                table: "Especes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HistoriqueXPs_PersonnageId",
                table: "HistoriqueXPs",
                column: "PersonnageId");

            migrationBuilder.CreateIndex(
                name: "IX_NiveauCarrieres_CarriereId",
                table: "NiveauCarrieres",
                column: "CarriereId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonnageCarrieres_NiveauCarriereId",
                table: "PersonnageCarrieres",
                column: "NiveauCarriereId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonnageCarrieres_PersonnageId",
                table: "PersonnageCarrieres",
                column: "PersonnageId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonnageCompetences_CompetenceId",
                table: "PersonnageCompetences",
                column: "CompetenceId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonnagePartages_PersonnageId_MjKeycloakId",
                table: "PersonnagePartages",
                columns: new[] { "PersonnageId", "MjKeycloakId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Personnages_CarriereCouranteId",
                table: "Personnages",
                column: "CarriereCouranteId");

            migrationBuilder.CreateIndex(
                name: "IX_Personnages_EspeceId",
                table: "Personnages",
                column: "EspeceId");

            migrationBuilder.CreateIndex(
                name: "IX_Personnages_KeycloakId",
                table: "Personnages",
                column: "KeycloakId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonnageTalents_TalentId",
                table: "PersonnageTalents",
                column: "TalentId");

            migrationBuilder.CreateIndex(
                name: "IX_Talents_Code",
                table: "Talents",
                column: "Code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HistoriqueXPs");

            migrationBuilder.DropTable(
                name: "PersonnageCaracteristiques");

            migrationBuilder.DropTable(
                name: "PersonnageCarrieres");

            migrationBuilder.DropTable(
                name: "PersonnageCompetences");

            migrationBuilder.DropTable(
                name: "PersonnagePartages");

            migrationBuilder.DropTable(
                name: "PersonnageTalents");

            migrationBuilder.DropTable(
                name: "Competences");

            migrationBuilder.DropTable(
                name: "Personnages");

            migrationBuilder.DropTable(
                name: "Talents");

            migrationBuilder.DropTable(
                name: "Especes");

            migrationBuilder.DropTable(
                name: "NiveauCarrieres");

            migrationBuilder.DropTable(
                name: "Carrieres");

            migrationBuilder.DropTable(
                name: "Classes");
        }
    }
}
