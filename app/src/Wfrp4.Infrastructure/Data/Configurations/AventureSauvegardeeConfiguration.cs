using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wfrp4.Infrastructure.Entities;

namespace Wfrp4.Infrastructure.Data.Configurations;

public class AventureSauvegardeeConfiguration : IEntityTypeConfiguration<AventureSauvegardee>
{
    public void Configure(EntityTypeBuilder<AventureSauvegardee> builder)
    {
        builder.HasIndex(a => a.KeycloakId);
        builder.Property(a => a.KeycloakId).HasMaxLength(100).IsRequired();
        builder.Property(a => a.Type).HasMaxLength(20).IsRequired();
        builder.Property(a => a.Titre).HasMaxLength(300).IsRequired();
        builder.Property(a => a.DataJson).HasColumnType("jsonb").IsRequired();
        builder.Property(a => a.CreatedAt).HasDefaultValueSql("now()");
        builder.Property(a => a.UpdatedAt).HasDefaultValueSql("now()");
    }
}
