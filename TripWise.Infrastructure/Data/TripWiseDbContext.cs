using Microsoft.EntityFrameworkCore;
using TripWise.Application.Interfaces;
using TripWise.Domain.Entities;

namespace TripWise.Infrastructure.Data;

public class TripWiseDbContext(DbContextOptions<TripWiseDbContext> options) : DbContext(options), IAppDbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Trip> Trips => Set<Trip>();
    public DbSet<TripMember> TripMembers => Set<TripMember>();
    public DbSet<BudgetPlan> BudgetPlans => Set<BudgetPlan>();
    public DbSet<BudgetCategory> BudgetCategories => Set<BudgetCategory>();
    public DbSet<Expense> Expenses => Set<Expense>();
    public DbSet<ExpenseSplit> ExpenseSplits => Set<ExpenseSplit>();
    public DbSet<Settlement> Settlements => Set<Settlement>();
    public DbSet<WalletContribution> WalletContributions => Set<WalletContribution>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<InviteToken> InviteTokens => Set<InviteToken>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(e => e.HasIndex(u => u.Email).IsUnique());

        modelBuilder.Entity<Trip>(e =>
        {
            e.HasOne(t => t.CreatedBy).WithMany(u => u.CreatedTrips).HasForeignKey(t => t.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Settlement>(e =>
        {
            e.HasOne(s => s.Payer).WithMany().HasForeignKey(s => s.PayerUserId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(s => s.Receiver).WithMany().HasForeignKey(s => s.ReceiverUserId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Expense>(e =>
        {
            e.HasOne(ex => ex.PaidBy).WithMany().HasForeignKey(ex => ex.PaidByUserId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<InviteToken>(e => e.HasIndex(i => i.Token).IsUnique());

        modelBuilder.Entity<PasswordResetToken>(e =>
        {
            e.HasIndex(p => p.Token).IsUnique();
            e.HasOne(p => p.User).WithMany().HasForeignKey(p => p.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AuditLog>(e =>
        {
            e.HasOne(a => a.User).WithMany().HasForeignKey(a => a.UserId).OnDelete(DeleteBehavior.Restrict);
        });
    }
}
