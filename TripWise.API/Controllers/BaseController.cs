using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using TripWise.Application.DTOs;

namespace TripWise.API.Controllers;

[ApiController]
public abstract class BaseController : ControllerBase
{
    protected Guid CurrentUserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    protected IActionResult Ok<T>(T data, string message = "Success") =>
        base.Ok(ApiResponse<T>.Ok(data, message));

    protected IActionResult Fail(string message) =>
        BadRequest(ApiResponse<object>.Fail(message));
}
