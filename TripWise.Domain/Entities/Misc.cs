namespace TripWise.Domain.Entities;

public class WalletContribution
{
    public Guid ContributionId { get; set; } = Guid.NewGuid();
    public Guid TripId { get; set; }
    public Guid UserId { get; set; }
    public decimal Amount { get; set; }
    public DateTime ContributedAt { get; set; } = DateTime.UtcNow;
    public string? Note { get; set; }

    public Trip Trip { get; set; } = null!;
    public User User { get; set; } = null!;
}

public class Notification
{
    public Guid NotificationId { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public Guid? TripId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
    public Trip? Trip { get; set; }
}

public class InviteToken
{
    public Guid InviteTokenId { get; set; } = Guid.NewGuid();
    public Guid TripId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; } = false;

    public Trip Trip { get; set; } = null!;
}

public class AuditLog
{
    public Guid AuditLogId { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string Entity { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? IPAddress { get; set; }

    public User User { get; set; } = null!;
}
