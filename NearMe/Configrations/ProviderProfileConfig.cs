using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NearMe.Models;
namespace NearMe.Configrations
{
    public class ProviderProfileConfig : IEntityTypeConfiguration<ProviderProfile>
    {
        public void Configure(EntityTypeBuilder <ProviderProfile> builder)
        {
            builder.HasKey(p => p.Id);
            builder.Property(p=>p.CategoryId).IsRequired();
            builder.Property(p => p.BusinessName).IsRequired();
            builder.Property(p => p.Address).IsRequired();
            builder.Property(p => p.RegionId).IsRequired();
            builder.HasOne(p=>p.User).WithOne(u=>u.ProviderProfile)
                .HasForeignKey<ProviderProfile>(p=>p.UserId).OnDelete(DeleteBehavior.Cascade);
        }
    }
}
