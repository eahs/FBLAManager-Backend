using ADSBackend.Models;
using ADSBackend.Models.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ADSBackend.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, int>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<ConfigurationItem> ConfigurationItem { get; set; }

        public DbSet<Member> Member { get; set; }

        public DbSet<Club> Club { get; set; }

        public DbSet<ClubMember> ClubMember { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);

            // intermediate ClubMembers table
            builder.Entity<ClubMember>()
                .HasKey(t => new { t.ClubId, t.MemberId });

            builder.Entity<ClubMember>()
                .HasOne(cm => cm.Club)
                .WithMany(c => c.ClubMembers)
                .HasForeignKey(cm => cm.ClubId);
                // .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ClubMember>()
                .HasOne(cm => cm.Member)
                .WithMany(m => m.ClubMembers)
                .HasForeignKey(cm => cm.MemberId);
        }

        public DbSet<ADSBackend.Models.Meeting> Meeting { get; set; }
    }
}
