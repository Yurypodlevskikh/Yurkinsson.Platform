using Microsoft.EntityFrameworkCore;
using DataAccess.Models;

namespace DataAccess
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        
        public DbSet<MetronomeCollectionName> MetronomeCollectionNames { get; set; } = null!;
        public DbSet<UserInfo> UserInfos { get; set; } = null!;
        public DbSet<MetronomeSettings> MetronomeSettings { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // MetronomeSettings -> UserInfo (required)
            modelBuilder.Entity<MetronomeSettings>()
            .HasOne(m => m.User)
            .WithMany(u => u.MetronomeSettings)
            .HasForeignKey(m => m.UserInfoId)
            .OnDelete(DeleteBehavior.Cascade);

            // MetronomeSettings -> MetronomeCollectionName (optional)
            modelBuilder.Entity<MetronomeSettings>()
            .HasOne(m => m.MetronomeCollection)
            .WithMany(mc => mc.MetronomeSettings)
            .HasForeignKey(m => m.MetronomeCollectionId)
            .OnDelete(DeleteBehavior.SetNull);

            // So that two identical Users can't exist in the same database
            modelBuilder.Entity<UserInfo>().HasIndex(u => u.GuidId).IsUnique();
        }
    }
}