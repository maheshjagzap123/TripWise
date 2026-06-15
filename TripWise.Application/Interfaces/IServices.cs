using TripWise.Application.DTOs;

namespace TripWise.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task ForgotPasswordAsync(ForgotPasswordRequest request);
    Task ResetPasswordAsync(ResetPasswordRequest request);
    Task<UserProfileResponse> GetProfileAsync(Guid userId);
    Task UpdateProfileAsync(Guid userId, UpdateProfileRequest request);
}

public interface ITripService
{
    Task<TripResponse> CreateTripAsync(Guid userId, CreateTripRequest request);
    Task<IEnumerable<TripResponse>> GetMyTripsAsync(Guid userId);
    Task<TripResponse> GetTripAsync(Guid tripId);
    Task UpdateTripAsync(Guid tripId, UpdateTripRequest request);
    Task DeleteTripAsync(Guid tripId);
    Task<TripDashboardResponse> GetDashboardAsync(Guid tripId, Guid userId);
}

public interface IBudgetService
{
    Task<BudgetResponse> CreateOrUpdateBudgetAsync(Guid tripId, CreateBudgetRequest request);
    Task<BudgetResponse> GetBudgetAsync(Guid tripId);
    Task<IEnumerable<BudgetVsActualItem>> GetBudgetSummaryAsync(Guid tripId);
}

public interface IExpenseService
{
    Task<ExpenseResponse> CreateExpenseAsync(Guid tripId, CreateExpenseRequest request);
    Task<IEnumerable<ExpenseResponse>> GetExpensesAsync(Guid tripId, string? category, DateOnly? startDate, DateOnly? endDate);
    Task<ExpenseResponse> GetExpenseAsync(Guid tripId, Guid expenseId);
    Task UpdateExpenseAsync(Guid tripId, Guid expenseId, UpdateExpenseRequest request);
    Task DeleteExpenseAsync(Guid tripId, Guid expenseId);
    Task<string> UploadAttachmentAsync(Guid tripId, Guid expenseId, Stream file, string fileName);
}

public interface IGroupService
{
    Task InviteMemberAsync(Guid tripId, InviteMemberRequest request);
    Task JoinTripAsync(Guid tripId, Guid userId, JoinTripRequest request);
    Task<IEnumerable<TripMemberResponse>> GetMembersAsync(Guid tripId);
    Task RemoveMemberAsync(Guid tripId, Guid userId);
    Task<string> GenerateInviteLinkAsync(Guid tripId);
}

public interface ISplitService
{
    Task CreateSplitAsync(Guid expenseId, CreateSplitRequest request);
    Task<SplitResponse> GetSplitAsync(Guid expenseId);
    Task UpdateSplitAsync(Guid expenseId, CreateSplitRequest request);
}

public interface ISettlementService
{
    Task<IEnumerable<SettlementResponse>> GetSettlementsAsync(Guid tripId);
    Task<MySettlementsResponse> GetMySettlementsAsync(Guid tripId, Guid userId);
    Task MarkPaidAsync(Guid tripId, Guid settlementId, MarkPaidRequest request);
    Task<IEnumerable<SettlementResponse>> GetHistoryAsync(Guid tripId);
    Task<IEnumerable<MemberBalanceResponse>> GetMemberBalancesAsync(Guid tripId);
}

public interface IWalletService
{
    Task ContributeAsync(Guid tripId, ContributeRequest request);
    Task<WalletResponse> GetWalletAsync(Guid tripId);
    Task<IEnumerable<ContributionItem>> GetTransactionsAsync(Guid tripId);
}

public interface IAnalyticsService
{
    Task<AnalyticsSummaryResponse> GetSummaryAsync(Guid tripId);
    Task<IEnumerable<CategoryBreakdownItem>> GetCategoryBreakdownAsync(Guid tripId);
    Task<IEnumerable<BudgetVsActualItem>> GetBudgetVsActualAsync(Guid tripId);
    Task<IEnumerable<MemberContributionItem>> GetMemberContributionsAsync(Guid tripId);
    Task<IEnumerable<SpendingTrendItem>> GetSpendingTrendAsync(Guid tripId);
}

public interface INotificationService
{
    Task<IEnumerable<NotificationResponse>> GetNotificationsAsync(Guid userId, bool? isRead);
    Task MarkReadAsync(Guid notificationId);
    Task MarkAllReadAsync(Guid userId);
}

public interface IAdminService
{
    Task<AdminDashboardResponse> GetDashboardAsync();
    Task<IEnumerable<UserProfileResponse>> GetUsersAsync(string? search, bool? isActive, int page, int pageSize);
    Task DeactivateUserAsync(Guid userId);
    Task<IEnumerable<TripResponse>> GetAllTripsAsync(string? status, string? search, int page, int pageSize);
    Task<IEnumerable<AuditLogResponse>> GetAuditLogsAsync(Guid? userId, string? action, DateTime? startDate, DateTime? endDate);
}
