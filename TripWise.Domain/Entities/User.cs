namespace TripWise.Domain.Entities;

public class User
{
    public Guid UserId { get; set; } = Guid.NewGuid();
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = "User";
    public string? ProfilePicture { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Trip> CreatedTrips { get; set; } = new List<Trip>();
    public ICollection<TripMember> TripMemberships { get; set; } = new List<TripMember>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}
