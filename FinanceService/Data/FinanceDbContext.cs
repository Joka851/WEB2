using Microsoft.EntityFrameworkCore;
using FinanceService.Models;

namespace FinanceService.Data
{
    public class FinanceDbContext : DbContext
    {
        public FinanceDbContext(DbContextOptions<FinanceDbContext> options) : base(options) { }

        public DbSet<Expense> Expenses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Expense precision
            modelBuilder.Entity<Expense>()
                .Property(e => e.Amount)
                .HasPrecision(18, 2);

            // Configure Expense table
            modelBuilder.Entity<Expense>()
                .ToTable("Expenses");

            // Configure indexes
            modelBuilder.Entity<Expense>()
                .HasIndex(e => e.TravelPlanId);

            modelBuilder.Entity<Expense>()
                .HasIndex(e => e.Category);

            // Configure constraints
            modelBuilder.Entity<Expense>()
                .Property(e => e.IsDeleted)
                .HasDefaultValue(false);

            modelBuilder.Entity<Expense>()
                .Property(e => e.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            modelBuilder.Entity<Expense>()
                .Property(e => e.UpdatedAt)
                .HasDefaultValueSql("GETUTCDATE()");
        }
    }
}
