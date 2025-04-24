using Microsoft.EntityFrameworkCore;
using YourProjectName.Domain.Entities;

namespace YourProjectName.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // DbSet ها
        public DbSet<Player> Players { get; set; }
        public DbSet<Village> Villages { get; set; }
        public DbSet<Building> Buildings { get; set; }
        public DbSet<Resources> Resources { get; set; }
        public DbSet<Troop> Troops { get; set; }
        public DbSet<Alliance> Alliances { get; set; }
        public DbSet<AllianceMember> AllianceMembers { get; set; }
        public DbSet<Attack> Attacks { get; set; }
        public DbSet<AttackTroop> AttackTroops { get; set; }
        public DbSet<BattleReport> BattleReports { get; set; }
        public DbSet<BuildingProductionRate> BuildingProductionRates { get; set; }
        public DbSet<LoginAttempt> LoginAttempts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // تنظیمات کلیدهای ترکیبی
            modelBuilder.Entity<AllianceMember>()
                .HasKey(am => new { am.AllianceId, am.PlayerId });
                
            modelBuilder.Entity<AttackTroop>()
                .HasKey(at => new { at.AttackId, at.TroopType });

            // ایندکس‌ها
            modelBuilder.Entity<Player>()
                .HasIndex(p => p.Email)
                .IsUnique();

            modelBuilder.Entity<Village>()
                .HasIndex(v => new { v.X, v.Y })
                .IsUnique();

            modelBuilder.Entity<Alliance>()
                .HasIndex(a => a.Tag)
                .IsUnique();

            // روابط
            modelBuilder.Entity<Village>()
                .HasOne(v => v.Resources)
                .WithOne(r => r.Village)
                .HasForeignKey<Resources>(r => r.VillageId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Attack>()
                .HasMany(a => a.Troops)
                .WithOne(t => t.Attack)
                .OnDelete(DeleteBehavior.Cascade);

            // Seed داده‌های اولیه
            SeedInitialData(modelBuilder);
        }

        private void SeedInitialData(ModelBuilder modelBuilder)
        {
            // مقادیر اولیه برای نرخ تولید ساختمان‌ها
            modelBuilder.Entity<BuildingProductionRate>().HasData(
                new BuildingProductionRate {
                    BuildingType = BuildingType.Woodcutter,
                    BaseProduction = 5,
                    ProductionFactor = 1.5f,
                    BaseStorage = 500,
                    StorageFactor = 1.2f
                },
                new BuildingProductionRate {
                    BuildingType = BuildingType.ClayPit,
                    BaseProduction = 4,
                    ProductionFactor = 1.4f,
                    BaseStorage = 500,
                    StorageFactor = 1.2f
                }
                // سایر ساختمان‌ها...
            );
        }
    }
}