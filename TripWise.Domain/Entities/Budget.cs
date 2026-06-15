namespace TripWise.Domain.Entities;

public class BudgetPlan
{
    public Guid BudgetPlanId { get; set; } = Guid.NewGuid();
    public Guid TripId { get; set; }
    public decimal TotalBudget { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Trip Trip { get; set; } = null!;
    public ICollection<BudgetCategory> Categories { get; set; } = new List<BudgetCategory>();
}

public class BudgetCategory
{
    public Guid BudgetCategoryId { get; set; } = Guid.NewGuid();
    public Guid BudgetPlanId { get; set; }
    public string Category { get; set; } = string.Empty;
    public decimal PlannedAmount { get; set; }

    public BudgetPlan BudgetPlan { get; set; } = null!;
}
