namespace TripWise.Domain.Entities;

public class Trip
{
    public Guid TripId { get; set; } = Guid.NewGuid();
    public Guid CreatedByUserId { get; set; }
    public string TripName { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string? Description { get; set; }
    public string TripType { get; set; } = "Group";
    public string Status { get; set; } = "Active";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User CreatedBy { get; set; } = null!;
    public ICollection<TripMember> Members { get; set; } = new List<TripMember>();
    public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
    public BudgetPlan? BudgetPlan { get; set; }
    public ICollection<Settlement> Settlements { get; set; } = new List<Settlement>();
    public ICollection<WalletContribution> WalletContributions { get; set; } = new List<WalletContribution>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    public ICollection<InviteToken> InviteTokens { get; set; } = new List<InviteToken>();
}
