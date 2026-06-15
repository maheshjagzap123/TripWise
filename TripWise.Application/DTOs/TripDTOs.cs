namespace TripWise.Application.DTOs;

public record CreateTripRequest(string TripName, string Destination, DateOnly StartDate, DateOnly EndDate, string? Description, string TripType);
public record UpdateTripRequest(string TripName, string Destination, DateOnly StartDate, DateOnly EndDate, string? Description);

public record TripResponse(Guid TripId, string TripName, string Destination, DateOnly StartDate, DateOnly EndDate, string? Description, string TripType, string Status, DateTime CreatedAt);

public record TripDashboardResponse(
    decimal TotalBudget, decimal ActualExpense, decimal RemainingBudget,
    decimal PayableAmount, decimal ReceivableAmount,
    IEnumerable<ExpenseResponse> RecentExpenses);

public record TripMemberResponse(Guid UserId, string FullName, string Role, DateTime JoinedAt);
