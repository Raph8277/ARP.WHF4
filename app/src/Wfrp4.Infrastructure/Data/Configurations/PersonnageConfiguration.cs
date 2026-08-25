using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wfrp4.Infrastructure.Entities;
using Wfrp4.Shared.Models;

namespace Wfrp4.Infrastructure.Data.Configurations;

public class PersonnageConfiguration : IEntityTypeConfiguration<Personnage>
{
    public void Configure(EntityTypeBuilder<Personnage> builder)
    {
        builder.HasIndex(p => p.KeycloakId);
        builder.Property(p => p.Nom).HasMaxLength(200).IsRequired();
        builder.Property(p => p.Genre)
               .HasConversion<string>()
               .HasMaxLength(20)
               .HasDefaultValue(GenrePersonnage.Masculin);
        builder.Property(p => p.KeycloakId).HasMaxLength(100).IsRequired();
        builder.Property(p => p.StatutSocial).HasMaxLength(50);
        builder.Property(p => p.Age);
        builder.Property(p => p.CouleurYeux).HasMaxLength(50);
        builder.Property(p => p.CouleurCheveux).HasMaxLength(50);
        builder.Property(p => p.Motivation).HasMaxLength(500);
        builder.Property(p => p.AmbitionCourtTerme).HasMaxLength(500);
        builder.Property(p => p.AmbitionLongTerme).HasMaxLength(500);
        builder.Property(p => p.GroupeNom).HasMaxLength(200);
        builder.Property(p => p.GroupeMembres).HasMaxLength(500);
        builder.Property(p => p.Psychologie).HasMaxLength(1000);
        builder.Property(p => p.CorruptionMutations).HasMaxLength(1000);

        builder.HasOne(p => p.TitreBaseReference)
               .WithMany()
               .HasForeignKey(p => p.TitreBaseReferenceId)
               .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(p => p.TitreQualificatifReference)
               .WithMany()
               .HasForeignKey(p => p.TitreQualificatifReferenceId)
               .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(p => p.Espece)
               .WithMany()
               .HasForeignKey(p => p.EspeceId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.CarriereCourante)
               .WithMany()
               .HasForeignKey(p => p.CarriereCouranteId)
               .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(p => p.Caracteristiques)
               .WithOne()
               .HasForeignKey(pc => pc.PersonnageId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Competences)
               .WithOne()
               .HasForeignKey(pc => pc.PersonnageId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Talents)
               .WithOne()
               .HasForeignKey(pt => pt.PersonnageId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Carrieres)
               .WithOne()
               .HasForeignKey(pc => pc.PersonnageId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.HistoriqueXP)
               .WithOne(h => h.Personnage)
               .HasForeignKey(h => h.PersonnageId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Partages)
               .WithOne(pp => pp.Personnage)
               .HasForeignKey(pp => pp.PersonnageId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
