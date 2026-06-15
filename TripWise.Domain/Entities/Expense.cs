namespace TripWise.Domain.Entities;

public class Expense
{
    public Guid ExpenseId { get; set; } = Guid.NewGuid();
    public Guid TripId { get; set; }
    public Guid PaidByUserId { get; set; }
    public decimal Amount { get; set; }
    public string Category { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateOnly ExpenseDate { get; set; }
    public string? AttachmentUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Trip Trip { get; set; } = null!;
    public User PaidBy { get; set; } = null!;
    public ICollection<ExpenseSplit> Splits { get; set; } = new List<ExpenseSplit>();
}

public class ExpenseSplit
{
    public Guid SplitId { get; set; } = Guid.NewGuid();
    public Guid ExpenseId { get; set; }
    public Guid UserId { get; set; }
    public string SplitType { get; set; } = "Equal";
    public decimal ShareAmount { get; set; }
    public decimal? SharePercentage { get; set; }

    public Expense Expense { get; set; } = null!;
    public User User { get; set; } = null!;
}
