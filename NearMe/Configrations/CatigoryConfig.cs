using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NearMe.Models;

namespace NearMe.Configrations
{
    public class CatigoryConfig : IEntityTypeConfiguration<Category>

    {

        public void Configure(EntityTypeBuilder<Category> builder)
        {
            builder.HasKey(c => c.Id);

            builder.Property(c => c.Name)
                  .IsRequired()
                  .HasMaxLength(100);

            builder.Property(c => c.IconPath)
                   .HasMaxLength(500);

            builder.HasMany(c=>c.profiles).WithOne (p=>p.Category)
                .HasForeignKey(p => p.CategoryId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
