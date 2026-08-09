using Microsoft.EntityFrameworkCore;
using Wfrp4.Infrastructure.Entities;

namespace Wfrp4.Infrastructure.Data;

public class Wfrp4DbContext : DbContext
{
    public Wfrp4DbContext(DbContextOptions<Wfrp4DbContext> options) : base(options) { }

    public DbSet<Personnage> Personnages => Set<Personnage>();
    public DbSet<PersonnageCaracteristique> PersonnageCaracteristiques => Set<PersonnageCaracteristique>();
    public DbSet<PersonnageCompetence> PersonnageCompetences => Set<PersonnageCompetence>();
    public DbSet<PersonnageTalent> PersonnageTalents => Set<PersonnageTalent>();
    public DbSet<PersonnageCarriere> PersonnageCarrieres => Set<PersonnageCarriere>();
    public DbSet<PersonnagePartage> PersonnagePartages => Set<PersonnagePartage>();
    public DbSet<PersonnagePossession> PersonnagePossessions => Set<PersonnagePossession>();
    public DbSet<HistoriqueXP> HistoriqueXPs => Set<HistoriqueXP>();

    public DbSet<Espece> Especes => Set<Espece>();
    public DbSet<Classe> Classes => Set<Classe>();
    public DbSet<Carriere> Carrieres => Set<Carriere>();
    public DbSet<NiveauCarriere> NiveauCarrieres => Set<NiveauCarriere>();
    public DbSet<Competence> Competences => Set<Competence>();
    public DbSet<Talent> Talents => Set<Talent>();
    public DbSet<ArmeReference> ArmesReference => Set<ArmeReference>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(Wfrp4DbContext).Assembly);
    }
}
