using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class ReferralSearchRequestConfiguration : IEntityTypeConfiguration<ReferralSearchRequest>
{
    public void Configure(EntityTypeBuilder<ReferralSearchRequest> builder)
    {
        builder.Property(rsr => rsr.ReferralNumber)
            .IsRequired();

        builder.HasIndex(rsr => rsr.ReferralNumber);
    }
}