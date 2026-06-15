namespace TripWise.Application.DTOs;

public record RegisterRequest(string FullName, string Email, string PhoneNumber, string Password);
public record LoginRequest(string Email, string Password);
public record ForgotPasswordRequest(string Email);
public record ResetPasswordRequest(string ResetToken, string NewPassword);
public record AuthResponse(Guid UserId, string Token, string Role);

public record UserProfileResponse(Guid UserId, string FullName, string Email, string PhoneNumber, string? ProfilePicture);
public record UpdateProfileRequest(string FullName, string PhoneNumber, string? ProfilePicture);
