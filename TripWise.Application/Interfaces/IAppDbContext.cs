using Microsoft.EntityFrameworkCore;
using TripWise.Domain.Entities;

namespace TripWise.Application.Interfaces;

public interface IAppDbContext
{
    DbSet<User> Users { get; }
    DbSet<Trip> Trips { get; }
    DbSet<TripMember> TripMembers { get; }
    DbSet<BudgetPlan> BudgetPlans { get; }
    DbSet<BudgetCategory> BudgetCategories { get; }
    DbSet<Expense> Expenses { get; }
    DbSet<ExpenseSplit> ExpenseSplits { get; }
    DbSet<Settlement> Settlements { get; }
    DbSet<WalletContribution> WalletContributions { get; }
    DbSet<Notification> Notifications { get; }
    DbSet<InviteToken> InviteTokens { get; }
    DbSet<AuditLog> AuditLogs { get; }
    DbSet<PasswordResetToken> PasswordResetTokens { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
