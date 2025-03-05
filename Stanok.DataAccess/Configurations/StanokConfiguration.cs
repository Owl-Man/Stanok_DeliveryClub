using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Stanok.DataAccess.Entities;

namespace Stanok.DataAccess.Configurations;

public class StanokConfiguration : IEntityTypeConfiguration<StanokEntity>
{
    public void Configure(EntityTypeBuilder<StanokEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(s => s.Name)
            .IsRequired();

        builder.Property(s => s.Price)
            .IsRequired();
    }
}
