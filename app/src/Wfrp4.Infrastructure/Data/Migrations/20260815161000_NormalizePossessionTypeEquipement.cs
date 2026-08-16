using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Wfrp4.Infrastructure.Data;

#nullable disable

namespace Wfrp4.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(Wfrp4DbContext))]
    [Migration("20260815161000_NormalizePossessionTypeEquipement")]
    public partial class NormalizePossessionTypeEquipement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """UPDATE "PersonnagePossessions" SET "Type" = 'Objet' WHERE "Type" = 'Equipement';""");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
