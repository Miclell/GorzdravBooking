using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class ManualSearchRequestConfiguration : IEntityTypeConfiguration<ManualSearchRequest>
{
    public void Configure(EntityTypeBuilder<ManualSearchRequest> builder)
    {
        // empty
    }
}