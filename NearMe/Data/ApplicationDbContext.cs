
using NearMe.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NearMe.Configrations;


namespace NearMe.Data
    {
        public class ApplicationDbContext
            : IdentityDbContext<ApplicationUser>
        {
            public ApplicationDbContext(
                DbContextOptions<ApplicationDbContext> options)
                : base(options)
            {
            }

            public DbSet<ProviderProfile> ProviderProfiles { get; set; }
            public DbSet<Category> Categories { get; set; }
            public DbSet<Region> Regions { get; set; }
            public DbSet<Review> Reviews { get; set; }
            public DbSet<ProviderImage> ProviderImages { get; set; }

            protected override void OnModelCreating(ModelBuilder builder)
            {
                // Important: configure Identity tables
                base.OnModelCreating(builder);

                // Apply all IEntityTypeConfiguration classes
                builder.ApplyConfigurationsFromAssembly(
                    typeof(ApplicationDbContext).Assembly
                );
            }
        }
    }

