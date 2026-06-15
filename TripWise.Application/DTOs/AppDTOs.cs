namespace TripWise.Application.DTOs;

// Budget
public record BudgetCategoryRequest(string Category, decimal PlannedAmount);
public record CreateBudgetRequest(decimal TotalBudget, IEnumerable<BudgetCategoryRequest> Categories);
public record BudgetCategoryResponse(string Category, decimal PlannedAmount);
public record BudgetResponse(decimal TotalBudget, IEnumerable<BudgetCategoryResponse> Categories, decimal TotalPlanned);
public record BudgetVsActualItem(string Category, decimal PlannedAmount, decimal ActualAmount, decimal Difference);

// Expense
public record CreateExpenseRequest(decimal Amount, string Category, string? Description, DateOnly ExpenseDate, Guid PaidByUserId, string? AttachmentUrl);
public record UpdateExpenseRequest(decimal Amount, string Category, string? Description, DateOnly ExpenseDate, string? AttachmentUrl);
public record ExpenseResponse(Guid ExpenseId, decimal Amount, string Category, string? Description, DateOnly ExpenseDate, Guid PaidByUserId, string? AttachmentUrl, DateTime CreatedAt);

// Split
public record SplitMemberRequest(Guid UserId, decimal? ShareAmount, decimal? SharePercentage);
public record CreateSplitRequest(string SplitType, IEnumerable<SplitMemberRequest> Members);
public record SplitMemberResponse(Guid UserId, string FullName, decimal ShareAmount);
public record SplitResponse(string SplitType, IEnumerable<SplitMemberResponse> Members);

// Settlement
public record SettlementResponse(Guid SettlementId, Guid PayerUserId, string PayerName, Guid ReceiverUserId, string ReceiverName, decimal Amount, string Status);
public record MySettlementsResponse(IEnumerable<SettlementResponse> Payables, IEnumerable<SettlementResponse> Receivables);
public record MarkPaidRequest(DateTime PaidAt);
public record MemberBalanceResponse(Guid UserId, string FullName, decimal TotalPaid, decimal FairShare, decimal NetBalance);

// Wallet
public record ContributeRequest(Guid UserId, decimal Amount, string? Note);
public record ContributionItem(Guid UserId, string FullName, decimal Amount, DateTime ContributedAt, string? Note);
public record WalletResponse(decimal TotalBalance, decimal TotalExpenses, decimal RemainingBalance, IEnumerable<ContributionItem> Contributions);

// Group
public record InviteMemberRequest(string? Email, string? PhoneNumber);
public record JoinTripRequest(string InviteToken);

// Notification
public record NotificationResponse(Guid NotificationId, string Type, string Message, bool IsRead, DateTime CreatedAt);

// Analytics
public record AnalyticsSummaryResponse(decimal TotalBudget, decimal TotalExpense, decimal RemainingBudget, int TotalMembers, string? TopCategory);
public record CategoryBreakdownItem(string Category, decimal Amount, decimal Percentage);
public record MemberContributionItem(Guid UserId, string FullName, decimal TotalPaid, decimal SharePercentage);
public record SpendingTrendItem(DateOnly Date, decimal TotalAmount);

// Admin
public record AdminDashboardResponse(int TotalUsers, int ActiveTrips, decimal TotalExpenses, int TotalSettlements);
public record AuditLogResponse(Guid AuditLogId, Guid UserId, string Action, string Entity, string EntityId, DateTime Timestamp, string? IPAddress);
