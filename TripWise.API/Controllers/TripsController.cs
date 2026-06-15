using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TripWise.Application.DTOs;
using TripWise.Application.Interfaces;

namespace TripWise.API.Controllers;

[Route("api/trips")]
[Authorize]
public class TripsController(ITripService tripService, IBudgetService budgetService,
    IExpenseService expenseService, IGroupService groupService,
    ISettlementService settlementService,
    IWalletService walletService, IAnalyticsService analyticsService) : BaseController
{
    // --- Trip CRUD ---
    [HttpPost]
    public async Task<IActionResult> Create(CreateTripRequest request) =>
        Ok(await tripService.CreateTripAsync(CurrentUserId, request));

    [HttpGet]
    public async Task<IActionResult> GetMyTrips() =>
        Ok(await tripService.GetMyTripsAsync(CurrentUserId));

    [HttpGet("{tripId:guid}")]
    public async Task<IActionResult> GetTrip(Guid tripId) =>
        Ok(await tripService.GetTripAsync(tripId));

    [HttpPut("{tripId:guid}")]
    public async Task<IActionResult> UpdateTrip(Guid tripId, UpdateTripRequest request)
    {
        await tripService.UpdateTripAsync(tripId, request);
        return Ok<object>(null!, "Trip updated.");
    }

    [HttpDelete("{tripId:guid}")]
    public async Task<IActionResult> DeleteTrip(Guid tripId)
    {
        await tripService.DeleteTripAsync(tripId);
        return Ok<object>(null!, "Trip cancelled.");
    }

    [HttpGet("{tripId:guid}/dashboard")]
    public async Task<IActionResult> Dashboard(Guid tripId) =>
        Ok(await tripService.GetDashboardAsync(tripId, CurrentUserId));

    // --- Budget ---
    [HttpPost("{tripId:guid}/budget")]
    public async Task<IActionResult> CreateBudget(Guid tripId, CreateBudgetRequest request) =>
        Ok(await budgetService.CreateOrUpdateBudgetAsync(tripId, request));

    [HttpGet("{tripId:guid}/budget")]
    public async Task<IActionResult> GetBudget(Guid tripId) =>
        Ok(await budgetService.GetBudgetAsync(tripId));

    [HttpGet("{tripId:guid}/budget/summary")]
    public async Task<IActionResult> BudgetSummary(Guid tripId) =>
        Ok(await budgetService.GetBudgetSummaryAsync(tripId));

    // --- Expenses ---
    [HttpPost("{tripId:guid}/expenses")]
    public async Task<IActionResult> CreateExpense(Guid tripId, CreateExpenseRequest request) =>
        Ok(await expenseService.CreateExpenseAsync(tripId, request));

    [HttpGet("{tripId:guid}/expenses")]
    public async Task<IActionResult> GetExpenses(Guid tripId, [FromQuery] string? category, [FromQuery] DateOnly? startDate, [FromQuery] DateOnly? endDate) =>
        Ok(await expenseService.GetExpensesAsync(tripId, category, startDate, endDate));

    [HttpGet("{tripId:guid}/expenses/{expenseId:guid}")]
    public async Task<IActionResult> GetExpense(Guid tripId, Guid expenseId) =>
        Ok(await expenseService.GetExpenseAsync(tripId, expenseId));

    [HttpPut("{tripId:guid}/expenses/{expenseId:guid}")]
    public async Task<IActionResult> UpdateExpense(Guid tripId, Guid expenseId, UpdateExpenseRequest request)
    {
        await expenseService.UpdateExpenseAsync(tripId, expenseId, request);
        return Ok<object>(null!, "Expense updated.");
    }

    [HttpDelete("{tripId:guid}/expenses/{expenseId:guid}")]
    public async Task<IActionResult> DeleteExpense(Guid tripId, Guid expenseId)
    {
        await expenseService.DeleteExpenseAsync(tripId, expenseId);
        return Ok<object>(null!, "Expense deleted.");
    }

    [HttpPost("{tripId:guid}/expenses/{expenseId:guid}/attachment")]
    public async Task<IActionResult> UploadAttachment(Guid tripId, Guid expenseId, IFormFile file)
    {
        var url = await expenseService.UploadAttachmentAsync(tripId, expenseId, file.OpenReadStream(), file.FileName);
        return Ok(url);
    }

    // --- Members ---
    [HttpGet("{tripId:guid}/members")]
    public async Task<IActionResult> GetMembers(Guid tripId) =>
        Ok(await groupService.GetMembersAsync(tripId));

    [HttpPost("{tripId:guid}/members/invite")]
    public async Task<IActionResult> InviteMember(Guid tripId, InviteMemberRequest request)
    {
        await groupService.InviteMemberAsync(tripId, request);
        return Ok<object>(null!, "Invite sent.");
    }

    [HttpPost("{tripId:guid}/members/join")]
    public async Task<IActionResult> JoinTrip(Guid tripId, JoinTripRequest request)
    {
        await groupService.JoinTripAsync(tripId, CurrentUserId, request);
        return Ok<object>(null!, "Joined trip.");
    }

    [HttpDelete("{tripId:guid}/members/{userId:guid}")]
    public async Task<IActionResult> RemoveMember(Guid tripId, Guid userId)
    {
        await groupService.RemoveMemberAsync(tripId, userId);
        return Ok<object>(null!, "Member removed.");
    }

    [HttpGet("{tripId:guid}/invite-link")]
    public async Task<IActionResult> InviteLink(Guid tripId) =>
        Ok(await groupService.GenerateInviteLinkAsync(tripId));

    // --- Settlements ---
    [HttpGet("{tripId:guid}/settlements")]
    public async Task<IActionResult> GetSettlements(Guid tripId) =>
        Ok(await settlementService.GetSettlementsAsync(tripId));

    [HttpGet("{tripId:guid}/settlements/my")]
    public async Task<IActionResult> MySettlements(Guid tripId) =>
        Ok(await settlementService.GetMySettlementsAsync(tripId, CurrentUserId));

    [HttpPost("{tripId:guid}/settlements/{settlementId:guid}/pay")]
    public async Task<IActionResult> MarkPaid(Guid tripId, Guid settlementId, MarkPaidRequest request)
    {
        await settlementService.MarkPaidAsync(tripId, settlementId, request);
        return Ok<object>(null!, "Marked as paid.");
    }

    [HttpGet("{tripId:guid}/settlements/history")]
    public async Task<IActionResult> SettlementHistory(Guid tripId) =>
        Ok(await settlementService.GetHistoryAsync(tripId));

    [HttpGet("{tripId:guid}/settlements/member-balance")]
    public async Task<IActionResult> MemberBalances(Guid tripId) =>
        Ok(await settlementService.GetMemberBalancesAsync(tripId));

    // --- Wallet ---
    [HttpPost("{tripId:guid}/wallet/contribute")]
    public async Task<IActionResult> Contribute(Guid tripId, ContributeRequest request)
    {
        await walletService.ContributeAsync(tripId, request);
        return Ok<object>(null!, "Contribution added.");
    }

    [HttpGet("{tripId:guid}/wallet")]
    public async Task<IActionResult> GetWallet(Guid tripId) =>
        Ok(await walletService.GetWalletAsync(tripId));

    [HttpGet("{tripId:guid}/wallet/transactions")]
    public async Task<IActionResult> WalletTransactions(Guid tripId) =>
        Ok(await walletService.GetTransactionsAsync(tripId));

    // --- Analytics ---
    [HttpGet("{tripId:guid}/analytics/summary")]
    public async Task<IActionResult> AnalyticsSummary(Guid tripId) =>
        Ok(await analyticsService.GetSummaryAsync(tripId));

    [HttpGet("{tripId:guid}/analytics/category-breakdown")]
    public async Task<IActionResult> CategoryBreakdown(Guid tripId) =>
        Ok(await analyticsService.GetCategoryBreakdownAsync(tripId));

    [HttpGet("{tripId:guid}/analytics/budget-vs-actual")]
    public async Task<IActionResult> BudgetVsActual(Guid tripId) =>
        Ok(await analyticsService.GetBudgetVsActualAsync(tripId));

    [HttpGet("{tripId:guid}/analytics/member-contributions")]
    public async Task<IActionResult> MemberContributions(Guid tripId) =>
        Ok(await analyticsService.GetMemberContributionsAsync(tripId));

    [HttpGet("{tripId:guid}/analytics/spending-trend")]
    public async Task<IActionResult> SpendingTrend(Guid tripId) =>
        Ok(await analyticsService.GetSpendingTrendAsync(tripId));
}
