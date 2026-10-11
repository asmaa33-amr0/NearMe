using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NearMe.Models;


namespace NearMe.Configrations
{
    public class ReviewConfig:IEntityTypeConfiguration<Review>
    {
        public void Configure(EntityTypeBuilder<Review> builder)
        {
            builder.HasKey(r=>r.Id);
            builder.Property(r=>r.Rating).IsRequired();
            // Review -> User
            builder.HasOne(r => r.User)
                   .WithMany()
                   .HasForeignKey(r => r.UserId)
                   .OnDelete(DeleteBehavior.Restrict);

            // Review -> ProviderProfile
            builder.HasOne(r => r.providerProfile)
                   .WithMany(p => p.Reviews)
                   .HasForeignKey(r => r.ProviderProfileId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
