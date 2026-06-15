using Microsoft.EntityFrameworkCore;
using TripWise.Application.DTOs;
using TripWise.Application.Interfaces;
using TripWise.Domain.Entities;
namespace TripWise.Application.Services;

public class WalletService(IAppDbContext db) : IWalletService
{
    public async Task ContributeAsync(Guid tripId, ContributeRequest request)
    {
        db.WalletContributions.Add(new WalletContribution { TripId = tripId, UserId = request.UserId, Amount = request.Amount, Note = request.Note });
        await db.SaveChangesAsync();
    }

    public async Task<WalletResponse> GetWalletAsync(Guid tripId)
    {
        var contributions = await db.WalletContributions.Where(w => w.TripId == tripId)
            .Select(w => new ContributionItem(w.UserId, w.User.FullName, w.Amount, w.ContributedAt, w.Note))
            .ToListAsync();
        var totalBalance = contributions.Sum(c => c.Amount);
        var totalExpenses = await db.Expenses.Where(e => e.TripId == tripId).SumAsync(e => e.Amount);
        return new WalletResponse(totalBalance, totalExpenses, totalBalance - totalExpenses, contributions);
    }

    public async Task<IEnumerable<ContributionItem>> GetTransactionsAsync(Guid tripId)
    {
        return await db.WalletContributions.Where(w => w.TripId == tripId)
            .Select(w => new ContributionItem(w.UserId, w.User.FullName, w.Amount, w.ContributedAt, w.Note))
            .ToListAsync();
    }
}

public class AnalyticsService(IAppDbContext db) : IAnalyticsService
{
    public async Task<AnalyticsSummaryResponse> GetSummaryAsync(Guid tripId)
    {
        var budget = await db.BudgetPlans.FirstOrDefaultAsync(b => b.TripId == tripId);
        var totalBudget = budget?.TotalBudget ?? 0;
        var totalExpense = await db.Expenses.Where(e => e.TripId == tripId).SumAsync(e => e.Amount);
        var members = await db.TripMembers.CountAsync(m => m.TripId == tripId);
        var topCategory = await db.Expenses.Where(e => e.TripId == tripId)
            .GroupBy(e => e.Category)
            .OrderByDescending(g => g.Sum(e => e.Amount))
            .Select(g => g.Key)
            .FirstOrDefaultAsync();
        return new AnalyticsSummaryResponse(totalBudget, totalExpense, totalBudget - totalExpense, members, topCategory);
    }

    public async Task<IEnumerable<CategoryBreakdownItem>> GetCategoryBreakdownAsync(Guid tripId)
    {
        var total = await db.Expenses.Where(e => e.TripId == tripId).SumAsync(e => e.Amount);
        return await db.Expenses.Where(e => e.TripId == tripId)
            .GroupBy(e => e.Category)
            .Select(g => new CategoryBreakdownItem(g.Key, g.Sum(e => e.Amount), total == 0 ? 0 : Math.Round(g.Sum(e => e.Amount) / total * 100, 2)))
            .ToListAsync();
    }

    public async Task<IEnumerable<BudgetVsActualItem>> GetBudgetVsActualAsync(Guid tripId)
    {
        var plan = await db.BudgetPlans.Include(b => b.Categories).FirstOrDefaultAsync(b => b.TripId == tripId)
            ?? throw new KeyNotFoundException("Budget not found.");
        var actuals = await db.Expenses.Where(e => e.TripId == tripId)
            .GroupBy(e => e.Category)
            .Select(g => new { Category = g.Key, Total = g.Sum(e => e.Amount) })
            .ToDictionaryAsync(x => x.Category, x => x.Total);
        return plan.Categories.Select(c =>
        {
            var actual = actuals.GetValueOrDefault(c.Category, 0);
            return new BudgetVsActualItem(c.Category, c.PlannedAmount, actual, c.PlannedAmount - actual);
        });
    }

    public async Task<IEnumerable<MemberContributionItem>> GetMemberContributionsAsync(Guid tripId)
    {
        var total = await db.Expenses.Where(e => e.TripId == tripId).SumAsync(e => e.Amount);
        return await db.Expenses.Where(e => e.TripId == tripId)
            .GroupBy(e => new { e.PaidByUserId, e.PaidBy.FullName })
            .Select(g => new MemberContributionItem(g.Key.PaidByUserId, g.Key.FullName, g.Sum(e => e.Amount), total == 0 ? 0 : Math.Round(g.Sum(e => e.Amount) / total * 100, 2)))
            .ToListAsync();
    }

    public async Task<IEnumerable<SpendingTrendItem>> GetSpendingTrendAsync(Guid tripId)
    {
        return await db.Expenses.Where(e => e.TripId == tripId)
            .GroupBy(e => e.ExpenseDate)
            .Select(g => new SpendingTrendItem(g.Key, g.Sum(e => e.Amount)))
            .OrderBy(x => x.Date)
            .ToListAsync();
    }
}

public class NotificationService(IAppDbContext db) : INotificationService
{
    public async Task<IEnumerable<NotificationResponse>> GetNotificationsAsync(Guid userId, bool? isRead)
    {
        var query = db.Notifications.Where(n => n.UserId == userId);
        if (isRead.HasValue) query = query.Where(n => n.IsRead == isRead.Value);
        return await query.Select(n => new NotificationResponse(n.NotificationId, n.Type, n.Message, n.IsRead, n.CreatedAt)).ToListAsync();
    }

    public async Task MarkReadAsync(Guid notificationId)
    {
        var n = await db.Notifications.FirstOrDefaultAsync(n => n.NotificationId == notificationId) ?? throw new KeyNotFoundException("Notification not found.");
        n.IsRead = true;
        await db.SaveChangesAsync();
    }

    public async Task MarkAllReadAsync(Guid userId)
    {
        var notifications = await db.Notifications.Where(n => n.UserId == userId && !n.IsRead).ToListAsync();
        foreach (var n in notifications) n.IsRead = true;
        await db.SaveChangesAsync();
    }
}

public class AdminService(IAppDbContext db) : IAdminService
{
    public async Task<AdminDashboardResponse> GetDashboardAsync()
    {
        var users = await db.Users.CountAsync();
        var activeTrips = await db.Trips.CountAsync(t => t.Status == "Active");
        var totalExpenses = await db.Expenses.SumAsync(e => e.Amount);
        var settlements = await db.Settlements.CountAsync();
        return new AdminDashboardResponse(users, activeTrips, totalExpenses, settlements);
    }

    public async Task<IEnumerable<UserProfileResponse>> GetUsersAsync(string? search, bool? isActive, int page, int pageSize)
    {
        var query = db.Users.AsQueryable();
        if (search != null) query = query.Where(u => u.FullName.Contains(search) || u.Email.Contains(search));
        if (isActive.HasValue) query = query.Where(u => u.IsActive == isActive.Value);
        return await query.Skip((page - 1) * pageSize).Take(pageSize)
            .Select(u => new UserProfileResponse(u.UserId, u.FullName, u.Email, u.PhoneNumber, u.ProfilePicture))
            .ToListAsync();
    }

    public async Task DeactivateUserAsync(Guid userId)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.UserId == userId) ?? throw new KeyNotFoundException("User not found.");
        user.IsActive = false;
        await db.SaveChangesAsync();
    }

    public async Task<IEnumerable<TripResponse>> GetAllTripsAsync(string? status, string? search, int page, int pageSize)
    {
        var query = db.Trips.AsQueryable();
        if (status != null) query = query.Where(t => t.Status == status);
        if (search != null) query = query.Where(t => t.TripName.Contains(search) || t.Destination.Contains(search));
        return await query.Skip((page - 1) * pageSize).Take(pageSize)
            .Select(t => new TripResponse(t.TripId, t.TripName, t.Destination, t.StartDate, t.EndDate, t.Description, t.TripType, t.Status, t.CreatedAt))
            .ToListAsync();
    }

    public async Task<IEnumerable<AuditLogResponse>> GetAuditLogsAsync(Guid? userId, string? action, DateTime? startDate, DateTime? endDate)
    {
        var query = db.AuditLogs.AsQueryable();
        if (userId.HasValue) query = query.Where(a => a.UserId == userId.Value);
        if (action != null) query = query.Where(a => a.Action == action);
        if (startDate.HasValue) query = query.Where(a => a.Timestamp >= startDate.Value);
        if (endDate.HasValue) query = query.Where(a => a.Timestamp <= endDate.Value);
        return await query.Select(a => new AuditLogResponse(a.AuditLogId, a.UserId, a.Action, a.Entity, a.EntityId, a.Timestamp, a.IPAddress)).ToListAsync();
    }
}
