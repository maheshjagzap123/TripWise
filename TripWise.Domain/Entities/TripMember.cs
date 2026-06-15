namespace TripWise.Domain.Entities;

public class TripMember
{
    public Guid TripMemberId { get; set; } = Guid.NewGuid();
    public Guid TripId { get; set; }
    public Guid UserId { get; set; }
    public string Role { get; set; } = "Member";
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    public Trip Trip { get; set; } = null!;
    public User User { get; set; } = null!;
}
