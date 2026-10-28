using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NearMe.Models;

namespace NearMe.Configrations
{
    public class ProviderImageConfig:IEntityTypeConfiguration<ProviderImage>
    {
        public void Configure(EntityTypeBuilder <ProviderImage> builder)
        {
            builder.HasKey(img=>img.Id);
            builder.Property(img=>img.ImageUrl).IsRequired().HasMaxLength(500);
            builder.HasOne(img=>img.ProviderProfile)
                .WithMany(p=>p.Gallery)
                .HasForeignKey(img=>img.ProviderProfileId)
                .OnDelete(DeleteBehavior.Cascade);

        }
    }
}
