using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wfrp4.Infrastructure.Entities;

namespace Wfrp4.Infrastructure.Data.Configurations;

public class PersonnageCaracteristiqueConfiguration : IEntityTypeConfiguration<PersonnageCaracteristique>
{
    public void Configure(EntityTypeBuilder<PersonnageCaracteristique> builder)
    {
        builder.HasKey(pc => new { pc.PersonnageId, pc.Code });
        builder.Property(pc => pc.Code).HasMaxLength(5).IsRequired();
    }
}

public class PersonnageCompetenceConfiguration : IEntityTypeConfiguration<PersonnageCompetence>
{
    public void Configure(EntityTypeBuilder<PersonnageCompetence> builder)
    {
        builder.HasKey(pc => new { pc.PersonnageId, pc.CompetenceId });

        builder.HasOne(pc => pc.Competence)
               .WithMany()
               .HasForeignKey(pc => pc.CompetenceId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}

public class PersonnageTalentConfiguration : IEntityTypeConfiguration<PersonnageTalent>
{
    public void Configure(EntityTypeBuilder<PersonnageTalent> builder)
    {
        builder.HasKey(pt => new { pt.PersonnageId, pt.TalentId });

        builder.HasOne(pt => pt.Talent)
               .WithMany()
               .HasForeignKey(pt => pt.TalentId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
