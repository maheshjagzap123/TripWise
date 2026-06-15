using Microsoft.EntityFrameworkCore;
using TripWise.Application.DTOs;
using TripWise.Application.Interfaces;
using TripWise.Domain.Entities;
namespace TripWise.Application.Services;

public class BudgetService(IAppDbContext db) : IBudgetService
{
    public async Task<BudgetResponse> CreateOrUpdateBudgetAsync(Guid tripId, CreateBudgetRequest request)
    {
        var plan = await db.BudgetPlans.Include(b => b.Categories).FirstOrDefaultAsync(b => b.TripId == tripId);
        if (plan == null)
        {
            plan = new BudgetPlan { TripId = tripId };
            db.BudgetPlans.Add(plan);
        }
        else
        {
            db.BudgetCategories.RemoveRange(plan.Categories);
        }
        plan.TotalBudget = request.TotalBudget;
        plan.Categories = request.Categories.Select(c => new BudgetCategory { BudgetPlanId = plan.BudgetPlanId, Category = c.Category, PlannedAmount = c.PlannedAmount }).ToList();
        await db.SaveChangesAsync();
        return Map(plan);
    }

    public async Task<BudgetResponse> GetBudgetAsync(Guid tripId)
    {
        var plan = await db.BudgetPlans.Include(b => b.Categories).FirstOrDefaultAsync(b => b.TripId == tripId)
            ?? throw new KeyNotFoundException("Budget not found.");
        return Map(plan);
    }

    public async Task<IEnumerable<BudgetVsActualItem>> GetBudgetSummaryAsync(Guid tripId)
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

    private static BudgetResponse Map(BudgetPlan plan) =>
        new(plan.TotalBudget, plan.Categories.Select(c => new BudgetCategoryResponse(c.Category, c.PlannedAmount)), plan.Categories.Sum(c => c.PlannedAmount));
}

public class ExpenseService(IAppDbContext db) : IExpenseService
{
    public async Task<ExpenseResponse> CreateExpenseAsync(Guid tripId, CreateExpenseRequest request)
    {
        var expense = new Expense
        {
            TripId = tripId,
            PaidByUserId = request.PaidByUserId,
            Amount = request.Amount,
            Category = request.Category,
            Description = request.Description,
            ExpenseDate = request.ExpenseDate,
            AttachmentUrl = request.AttachmentUrl
        };
        db.Expenses.Add(expense);
        await db.SaveChangesAsync();
        return Map(expense);
    }

    public async Task<IEnumerable<ExpenseResponse>> GetExpensesAsync(Guid tripId, string? category, DateOnly? startDate, DateOnly? endDate)
    {
        var query = db.Expenses.Where(e => e.TripId == tripId);
        if (category != null) query = query.Where(e => e.Category == category);
        if (startDate != null) query = query.Where(e => e.ExpenseDate >= startDate);
        if (endDate != null) query = query.Where(e => e.ExpenseDate <= endDate);
        return (await query.ToListAsync()).Select(Map);
    }

    public async Task<ExpenseResponse> GetExpenseAsync(Guid tripId, Guid expenseId)
    {
        var e = await db.Expenses.FirstOrDefaultAsync(e => e.TripId == tripId && e.ExpenseId == expenseId)
            ?? throw new KeyNotFoundException("Expense not found.");
        return Map(e);
    }

    public async Task UpdateExpenseAsync(Guid tripId, Guid expenseId, UpdateExpenseRequest request)
    {
        var e = await db.Expenses.FirstOrDefaultAsync(e => e.TripId == tripId && e.ExpenseId == expenseId)
            ?? throw new KeyNotFoundException("Expense not found.");
        e.Amount = request.Amount;
        e.Category = request.Category;
        e.Description = request.Description;
        e.ExpenseDate = request.ExpenseDate;
        e.AttachmentUrl = request.AttachmentUrl;
        await db.SaveChangesAsync();
    }

    public async Task DeleteExpenseAsync(Guid tripId, Guid expenseId)
    {
        var e = await db.Expenses.FirstOrDefaultAsync(e => e.TripId == tripId && e.ExpenseId == expenseId)
            ?? throw new KeyNotFoundException("Expense not found.");
        db.Expenses.Remove(e);
        await db.SaveChangesAsync();
    }

    public Task<string> UploadAttachmentAsync(Guid tripId, Guid expenseId, Stream file, string fileName)
    {
        // TODO: integrate with S3 or local storage
        return Task.FromResult($"/attachments/{expenseId}/{fileName}");
    }

    private static ExpenseResponse Map(Expense e) =>
        new(e.ExpenseId, e.Amount, e.Category, e.Description, e.ExpenseDate, e.PaidByUserId, e.AttachmentUrl, e.CreatedAt);
}

public class GroupService(IAppDbContext db) : IGroupService
{
    public async Task InviteMemberAsync(Guid tripId, InviteMemberRequest request)
    {
        var token = new InviteToken
        {
            TripId = tripId,
            Token = Guid.NewGuid().ToString("N"),
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };
        db.InviteTokens.Add(token);
        await db.SaveChangesAsync();
        // TODO: send invite email/SMS with token
    }

    public async Task JoinTripAsync(Guid tripId, Guid userId, JoinTripRequest request)
    {
        var token = await db.InviteTokens.FirstOrDefaultAsync(t => t.Token == request.InviteToken && t.TripId == tripId && !t.IsUsed && t.ExpiresAt > DateTime.UtcNow)
            ?? throw new InvalidOperationException("Invalid or expired invite token.");
        db.TripMembers.Add(new TripMember { TripId = tripId, UserId = userId });
        token.IsUsed = true;
        await db.SaveChangesAsync();
    }

    public async Task<IEnumerable<TripMemberResponse>> GetMembersAsync(Guid tripId)
    {
        return await db.TripMembers.Where(m => m.TripId == tripId)
            .Select(m => new TripMemberResponse(m.UserId, m.User.FullName, m.Role, m.JoinedAt))
            .ToListAsync();
    }

    public async Task RemoveMemberAsync(Guid tripId, Guid userId)
    {
        var member = await db.TripMembers.FirstOrDefaultAsync(m => m.TripId == tripId && m.UserId == userId)
            ?? throw new KeyNotFoundException("Member not found.");
        db.TripMembers.Remove(member);
        await db.SaveChangesAsync();
    }

    public async Task<string> GenerateInviteLinkAsync(Guid tripId)
    {
        var token = new InviteToken { TripId = tripId, Token = Guid.NewGuid().ToString("N"), ExpiresAt = DateTime.UtcNow.AddDays(7) };
        db.InviteTokens.Add(token);
        await db.SaveChangesAsync();
        return $"/api/trips/{tripId}/members/join?token={token.Token}";
    }
}

public class SplitService(IAppDbContext db) : ISplitService
{
    public async Task CreateSplitAsync(Guid expenseId, CreateSplitRequest request)
    {
        var existing = await db.ExpenseSplits.Where(s => s.ExpenseId == expenseId).ToListAsync();
        db.ExpenseSplits.RemoveRange(existing);
        db.ExpenseSplits.AddRange(request.Members.Select(m => new ExpenseSplit
        {
            ExpenseId = expenseId,
            UserId = m.UserId,
            SplitType = request.SplitType,
            ShareAmount = m.ShareAmount ?? 0,
            SharePercentage = m.SharePercentage
        }));
        await db.SaveChangesAsync();
    }

    public async Task<SplitResponse> GetSplitAsync(Guid expenseId)
    {
        var splits = await db.ExpenseSplits.Where(s => s.ExpenseId == expenseId)
            .Select(s => new SplitMemberResponse(s.UserId, s.User.FullName, s.ShareAmount))
            .ToListAsync();
        var type = await db.ExpenseSplits.Where(s => s.ExpenseId == expenseId).Select(s => s.SplitType).FirstOrDefaultAsync() ?? "Equal";
        return new SplitResponse(type, splits);
    }

    public Task UpdateSplitAsync(Guid expenseId, CreateSplitRequest request) => CreateSplitAsync(expenseId, request);
}

public class SettlementService(IAppDbContext db) : ISettlementService
{
    public async Task<IEnumerable<SettlementResponse>> GetSettlementsAsync(Guid tripId)
    {
        return await db.Settlements.Where(s => s.TripId == tripId)
            .Select(s => new SettlementResponse(s.SettlementId, s.PayerUserId, s.Payer.FullName, s.ReceiverUserId, s.Receiver.FullName, s.Amount, s.Status))
            .ToListAsync();
    }

    public async Task<MySettlementsResponse> GetMySettlementsAsync(Guid tripId, Guid userId)
    {
        var all = await db.Settlements.Where(s => s.TripId == tripId && s.Status == "Pending")
            .Select(s => new SettlementResponse(s.SettlementId, s.PayerUserId, s.Payer.FullName, s.ReceiverUserId, s.Receiver.FullName, s.Amount, s.Status))
            .ToListAsync();
        return new MySettlementsResponse(all.Where(s => s.PayerUserId == userId), all.Where(s => s.ReceiverUserId == userId));
    }

    public async Task MarkPaidAsync(Guid tripId, Guid settlementId, MarkPaidRequest request)
    {
        var s = await db.Settlements.FirstOrDefaultAsync(s => s.TripId == tripId && s.SettlementId == settlementId)
            ?? throw new KeyNotFoundException("Settlement not found.");
        s.Status = "Paid";
        s.PaidAt = request.PaidAt;
        await db.SaveChangesAsync();
    }

    public async Task<IEnumerable<SettlementResponse>> GetHistoryAsync(Guid tripId)
    {
        return await db.Settlements.Where(s => s.TripId == tripId && s.Status == "Paid")
            .Select(s => new SettlementResponse(s.SettlementId, s.PayerUserId, s.Payer.FullName, s.ReceiverUserId, s.Receiver.FullName, s.Amount, s.Status))
            .ToListAsync();
    }

    public async Task<IEnumerable<MemberBalanceResponse>> GetMemberBalancesAsync(Guid tripId)
    {
        var members = await db.TripMembers.Where(m => m.TripId == tripId).Select(m => new { m.UserId, m.User.FullName }).ToListAsync();
        var totalExpense = await db.Expenses.Where(e => e.TripId == tripId).SumAsync(e => e.Amount);
        var fairShare = members.Count > 0 ? totalExpense / members.Count : 0;
        var paid = await db.Expenses.Where(e => e.TripId == tripId)
            .GroupBy(e => e.PaidByUserId)
            .Select(g => new { UserId = g.Key, Total = g.Sum(e => e.Amount) })
            .ToDictionaryAsync(x => x.UserId, x => x.Total);
        return members.Select(m =>
        {
            var totalPaid = paid.GetValueOrDefault(m.UserId, 0);
            return new MemberBalanceResponse(m.UserId, m.FullName, totalPaid, fairShare, totalPaid - fairShare);
        });
    }
}
