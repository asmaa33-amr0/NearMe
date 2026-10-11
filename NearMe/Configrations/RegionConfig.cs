using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NearMe.Models;

namespace NearMe.Configrations
{
    public class RegionConfig:IEntityTypeConfiguration<Region>
    {
        public void Configure (EntityTypeBuilder<Region>builder)
        {
            builder.HasKey(r => r.id);
            builder.Property(r => r.name).IsRequired().HasMaxLength(500);

            builder.HasMany(r=>r.providers)
                .WithOne(p => p.Region).HasForeignKey(p => p.RegionId);

        }
    }
}
