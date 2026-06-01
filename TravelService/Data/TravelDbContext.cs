using Microsoft.EntityFrameworkCore;
using TravelService.Models;

namespace TravelService.Data
{
    public class TravelDbContext : DbContext
    {
        public TravelDbContext(DbContextOptions<TravelDbContext> options) : base(options) { }

        public DbSet<TravelPlan> TravelPlans { get; set; }
        public DbSet<Destination> Destinations { get; set; }
        public DbSet<Activity> Activities { get; set; }
        public DbSet<ChecklistItem> ChecklistItems { get; set; }
        public DbSet<ShareToken> ShareTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure TravelPlan
            modelBuilder.Entity<TravelPlan>()
                .Property(t => t.Budget)
                .HasPrecision(18, 2);

            modelBuilder.Entity<TravelPlan>()
                .Property(t => t.IsDeleted)
                .HasDefaultValue(false);

            modelBuilder.Entity<TravelPlan>()
                .Property(t => t.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            modelBuilder.Entity<TravelPlan>()
                .Property(t => t.UpdatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            // Configure Activity
            modelBuilder.Entity<Activity>()
                .Property(a => a.EstimatedCost)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Activity>()
                .Property(a => a.IsDeleted)
                .HasDefaultValue(false);

            modelBuilder.Entity<Activity>()
                .Property(a => a.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            modelBuilder.Entity<Activity>()
                .Property(a => a.UpdatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            // Configure Destination
            modelBuilder.Entity<Destination>()
                .Property(d => d.IsDeleted)
                .HasDefaultValue(false);

            modelBuilder.Entity<Destination>()
                .Property(d => d.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            modelBuilder.Entity<Destination>()
                .Property(d => d.UpdatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            // Configure ChecklistItem
            modelBuilder.Entity<ChecklistItem>()
                .Property(c => c.IsDeleted)
                .HasDefaultValue(false);

            modelBuilder.Entity<ChecklistItem>()
                .Property(c => c.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            modelBuilder.Entity<ChecklistItem>()
                .Property(c => c.UpdatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            // Configure ShareToken
            modelBuilder.Entity<ShareToken>()
                .Property(s => s.IsDeleted)
                .HasDefaultValue(false);

            modelBuilder.Entity<ShareToken>()
                .Property(s => s.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            // Configure relationships with cascade delete
            modelBuilder.Entity<TravelPlan>()
                .HasMany(t => t.Activities)
                .WithOne(a => a.TravelPlan)
                .HasForeignKey(a => a.TravelPlanId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TravelPlan>()
                .HasMany(t => t.Destinations)
                .WithOne(d => d.TravelPlan)
                .HasForeignKey(d => d.TravelPlanId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TravelPlan>()
                .HasMany(t => t.ChecklistItems)
                .WithOne(c => c.TravelPlan)
                .HasForeignKey(c => c.TravelPlanId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TravelPlan>()
                .HasMany(t => t.ShareTokens)
                .WithOne(s => s.TravelPlan)
                .HasForeignKey(s => s.TravelPlanId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
