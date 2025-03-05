using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Stanok.DataAccess.Entities;

namespace Stanok.DataAccess.Configurations;

public class DeliveryConfiguration : IEntityTypeConfiguration<DeliveryEntity>
{
    public void Configure(EntityTypeBuilder<DeliveryEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(d => d.StanokId)
            .IsRequired();
    }
}
