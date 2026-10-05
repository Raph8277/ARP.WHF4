using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wfrp4.Infrastructure.Entities;

namespace Wfrp4.Infrastructure.Data.Configurations;

public class PartieConfiguration : IEntityTypeConfiguration<Partie>
{
    public void Configure(EntityTypeBuilder<Partie> builder)
    {
        builder.HasIndex(p => p.MjKeycloakId);
        builder.Property(p => p.Nom).HasMaxLength(120).IsRequired();
        builder.Property(p => p.Description).HasMaxLength(2000);
        builder.Property(p => p.Type).HasConversion<string>().HasMaxLength(20);
        builder.Property(p => p.Statut).HasConversion<string>().HasMaxLength(20);
        builder.Property(p => p.MjKeycloakId).HasMaxLength(100).IsRequired();
        builder.Property(p => p.MjNom).HasMaxLength(100).IsRequired();

        builder.HasMany(p => p.Membres)
               .WithOne(m => m.Partie)
               .HasForeignKey(m => m.PartieId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

public class PartieMembreConfiguration : IEntityTypeConfiguration<PartieMembre>
{
    public void Configure(EntityTypeBuilder<PartieMembre> builder)
    {
        builder.HasIndex(m => new { m.PartieId, m.JoueurKeycloakId }).IsUnique();
        builder.HasIndex(m => m.JoueurKeycloakId);
        builder.Property(m => m.JoueurKeycloakId).HasMaxLength(100).IsRequired();
        builder.Property(m => m.JoueurNom).HasMaxLength(100).IsRequired();

        // Un personnage supprimé libère la place sans retirer le joueur de la partie.
        builder.HasOne(m => m.Personnage)
               .WithMany()
               .HasForeignKey(m => m.PersonnageId)
               .OnDelete(DeleteBehavior.SetNull);
    }
}
