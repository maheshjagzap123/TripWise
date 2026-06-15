namespace TripWise.Domain.Entities;

public class Settlement
{
    public Guid SettlementId { get; set; } = Guid.NewGuid();
    public Guid TripId { get; set; }
    public Guid PayerUserId { get; set; }
    public Guid ReceiverUserId { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = "Pending";
    public DateTime? PaidAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Trip Trip { get; set; } = null!;
    public User Payer { get; set; } = null!;
    public User Receiver { get; set; } = null!;
}
