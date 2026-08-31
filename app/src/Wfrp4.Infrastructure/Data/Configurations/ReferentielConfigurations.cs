using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wfrp4.Infrastructure.Entities;

namespace Wfrp4.Infrastructure.Data.Configurations;

public class EspeceConfiguration : IEntityTypeConfiguration<Espece>
{
    public void Configure(EntityTypeBuilder<Espece> builder)
    {
        builder.HasIndex(e => e.Code).IsUnique();
        builder.Property(e => e.Code).HasMaxLength(20).IsRequired();
        builder.Property(e => e.Nom).HasMaxLength(100).IsRequired();
        builder.Property(e => e.CaracInitiales).HasColumnType("jsonb");
        builder.Property(e => e.TraitsPhysiques).HasColumnType("jsonb");
    }
}

public class ClasseConfiguration : IEntityTypeConfiguration<Classe>
{
    public void Configure(EntityTypeBuilder<Classe> builder)
    {
        builder.HasIndex(c => c.Code).IsUnique();
        builder.Property(c => c.Code).HasMaxLength(20).IsRequired();
        builder.Property(c => c.Nom).HasMaxLength(100).IsRequired();
    }
}

public class CarriereConfiguration : IEntityTypeConfiguration<Carriere>
{
    public void Configure(EntityTypeBuilder<Carriere> builder)
    {
        builder.HasIndex(c => c.Code).IsUnique();
        builder.Property(c => c.Code).HasMaxLength(50).IsRequired();
        builder.Property(c => c.Nom).HasMaxLength(100).IsRequired();

        builder.HasOne(c => c.Classe)
               .WithMany(cl => cl.Carrieres)
               .HasForeignKey(c => c.ClasseId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.Property(c => c.EspecesAutorisees).HasMaxLength(200);

        builder.HasMany(c => c.Niveaux)
               .WithOne(n => n.Carriere)
               .HasForeignKey(n => n.CarriereId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

public class NiveauCarriereConfiguration : IEntityTypeConfiguration<NiveauCarriere>
{
    public void Configure(EntityTypeBuilder<NiveauCarriere> builder)
    {
        builder.Property(n => n.Intitule).HasMaxLength(100).IsRequired();
        builder.Property(n => n.AvancesCarac).HasColumnType("jsonb");
        builder.Property(n => n.CompetenceRevenu).HasMaxLength(500);
        builder.Property(n => n.TalentsRevenu).HasMaxLength(500);
        builder.Property(n => n.Dotations).HasMaxLength(1000);
    }
}

public class CompetenceConfiguration : IEntityTypeConfiguration<Competence>
{
    public void Configure(EntityTypeBuilder<Competence> builder)
    {
        builder.HasIndex(c => c.Code).IsUnique();
        builder.Property(c => c.Code).HasMaxLength(50).IsRequired();
        builder.Property(c => c.Nom).HasMaxLength(100).IsRequired();
        builder.Property(c => c.Caracteristique).HasMaxLength(5).IsRequired();
    }
}

public class TalentConfiguration : IEntityTypeConfiguration<Talent>
{
    public void Configure(EntityTypeBuilder<Talent> builder)
    {
        builder.HasIndex(t => t.Code).IsUnique();
        builder.Property(t => t.Code).HasMaxLength(50).IsRequired();
        builder.Property(t => t.Nom).HasMaxLength(100).IsRequired();
    }
}

public class ArmeReferenceConfiguration : IEntityTypeConfiguration<ArmeReference>
{
    public void Configure(EntityTypeBuilder<ArmeReference> builder)
    {
        builder.HasIndex(a => a.Code).IsUnique();
        builder.Property(a => a.Code).HasMaxLength(50).IsRequired();
        builder.Property(a => a.Nom).HasMaxLength(100).IsRequired();
        builder.Property(a => a.Groupe).HasMaxLength(50).IsRequired();
        builder.Property(a => a.TypeArme).HasMaxLength(20).IsRequired();
        builder.Property(a => a.Prix).HasMaxLength(50);
        builder.Property(a => a.Dommage).HasMaxLength(50).IsRequired();
        builder.Property(a => a.Disponibilite).HasMaxLength(50).IsRequired();
        builder.Property(a => a.Longueur).HasMaxLength(50);
        builder.Property(a => a.Portee).HasMaxLength(50);
        builder.Property(a => a.Qualites).HasMaxLength(300);
        builder.Property(a => a.Defauts).HasMaxLength(300);
    }
}

public class SortReferenceConfiguration : IEntityTypeConfiguration<SortReference>
{
    public void Configure(EntityTypeBuilder<SortReference> builder)
    {
        builder.HasIndex(s => s.Code).IsUnique();
        builder.Property(s => s.Code).HasMaxLength(160).IsRequired();
        builder.Property(s => s.Nom).HasMaxLength(120).IsRequired();
        builder.Property(s => s.Categorie).HasMaxLength(50).IsRequired();
        builder.Property(s => s.Domaine).HasMaxLength(80);
        builder.Property(s => s.Portee).HasMaxLength(80);
        builder.Property(s => s.Cible).HasMaxLength(80);
        builder.Property(s => s.Duree).HasMaxLength(80);
        builder.Property(s => s.Resume).HasMaxLength(1000);
    }
}

public class CreatureReferenceConfiguration : IEntityTypeConfiguration<CreatureReference>
{
    public void Configure(EntityTypeBuilder<CreatureReference> builder)
    {
        builder.HasIndex(c => c.Code).IsUnique();
        builder.Property(c => c.Code).HasMaxLength(80).IsRequired();
        builder.Property(c => c.Nom).HasMaxLength(120).IsRequired();
        builder.Property(c => c.Categorie).HasMaxLength(60).IsRequired();
        builder.Property(c => c.Traits).HasMaxLength(1000).IsRequired();
        builder.Property(c => c.TraitsOptionnels).HasMaxLength(1000);
    }
}

public class TitreBaseReferenceConfiguration : IEntityTypeConfiguration<TitreBaseReference>
{
    public void Configure(EntityTypeBuilder<TitreBaseReference> builder)
    {
        builder.ToTable(t => t.HasCheckConstraint("CK_TitresBaseReference_NiveauMaitrise", "\"NiveauMaitrise\" BETWEEN 1 AND 4"));
        builder.HasIndex(t => t.Code).IsUnique();
        builder.Property(t => t.Code).HasMaxLength(80).IsRequired();
        builder.Property(t => t.Libelle).HasMaxLength(120).IsRequired();
        builder.Property(t => t.NiveauMaitrise).IsRequired();
    }
}

public class TitreQualificatifReferenceConfiguration : IEntityTypeConfiguration<TitreQualificatifReference>
{
    public void Configure(EntityTypeBuilder<TitreQualificatifReference> builder)
    {
        builder.ToTable(t => t.HasCheckConstraint("CK_TitresQualificatifReference_NiveauMaitrise", "\"NiveauMaitrise\" BETWEEN 1 AND 4"));
        builder.HasIndex(t => t.Code).IsUnique();
        builder.Property(t => t.Code).HasMaxLength(80).IsRequired();
        builder.Property(t => t.Libelle).HasMaxLength(120).IsRequired();
        builder.Property(t => t.NiveauMaitrise).IsRequired();
    }
}

public class HistoriqueXPConfiguration : IEntityTypeConfiguration<HistoriqueXP>
{
    public void Configure(EntityTypeBuilder<HistoriqueXP> builder)
    {
        builder.Property(h => h.AuteurKeycloakId).HasMaxLength(100).IsRequired();
        builder.Property(h => h.Cible).HasMaxLength(100);
        builder.Property(h => h.Notes).HasMaxLength(500);
        builder.HasIndex(h => h.PersonnageId);
    }
}

public class PersonnagePartageConfiguration : IEntityTypeConfiguration<PersonnagePartage>
{
    public void Configure(EntityTypeBuilder<PersonnagePartage> builder)
    {
        builder.Property(p => p.MjKeycloakId).HasMaxLength(100).IsRequired();
        builder.HasIndex(p => new { p.PersonnageId, p.MjKeycloakId }).IsUnique();
    }
}

public class PersonnagePossessionConfiguration : IEntityTypeConfiguration<PersonnagePossession>
{
    public void Configure(EntityTypeBuilder<PersonnagePossession> builder)
    {
        builder.Property(p => p.Nom).HasMaxLength(200).IsRequired();
        builder.Property(p => p.Type).HasConversion<string>().HasMaxLength(10);
        builder.HasIndex(p => p.PersonnageId);
    }
}

public class PersonnageSortConfiguration : IEntityTypeConfiguration<PersonnageSort>
{
    public void Configure(EntityTypeBuilder<PersonnageSort> builder)
    {
        builder.HasIndex(s => new { s.PersonnageId, s.SortReferenceId }).IsUnique();

        builder.HasOne(s => s.Personnage)
               .WithMany(p => p.Sorts)
               .HasForeignKey(s => s.PersonnageId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(s => s.SortReference)
               .WithMany()
               .HasForeignKey(s => s.SortReferenceId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}

public class PersonnageParcheminConfiguration : IEntityTypeConfiguration<PersonnageParchemin>
{
    public void Configure(EntityTypeBuilder<PersonnageParchemin> builder)
    {
        builder.HasIndex(p => new { p.PersonnageId, p.SortReferenceId }).IsUnique();

        builder.HasOne(p => p.Personnage)
               .WithMany(p => p.Parchemins)
               .HasForeignKey(p => p.PersonnageId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.SortReference)
               .WithMany()
               .HasForeignKey(p => p.SortReferenceId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}

public class PersonnageCarriereConfiguration : IEntityTypeConfiguration<PersonnageCarriere>
{
    public void Configure(EntityTypeBuilder<PersonnageCarriere> builder)
    {
        builder.HasOne(pc => pc.NiveauCarriere)
               .WithMany()
               .HasForeignKey(pc => pc.NiveauCarriereId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
