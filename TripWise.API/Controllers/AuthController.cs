using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TripWise.Application.DTOs;
using TripWise.Application.Interfaces;

namespace TripWise.API.Controllers;

[Route("api/auth")]
public class AuthController(IAuthService authService) : BaseController
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request) =>
        Ok(await authService.RegisterAsync(request));

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request) =>
        Ok(await authService.LoginAsync(request));

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest request)
    {
        await authService.ForgotPasswordAsync(request);
        return Ok<object>(null!, "Reset link sent.");
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(ResetPasswordRequest request)
    {
        await authService.ResetPasswordAsync(request);
        return Ok<object>(null!, "Password reset successful.");
    }
}

[Route("api/users")]
[Authorize]
public class UsersController(IAuthService authService) : BaseController
{
    [HttpGet("{userId:guid}")]
    public async Task<IActionResult> GetProfile(Guid userId) =>
        Ok(await authService.GetProfileAsync(userId));

    [HttpPut("{userId:guid}")]
    public async Task<IActionResult> UpdateProfile(Guid userId, UpdateProfileRequest request)
    {
        await authService.UpdateProfileAsync(userId, request);
        return Ok<object>(null!, "Profile updated.");
    }
}
