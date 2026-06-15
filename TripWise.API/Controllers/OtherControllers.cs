using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TripWise.Application.DTOs;
using TripWise.Application.Interfaces;

namespace TripWise.API.Controllers;

[Route("api/expenses")]
[Authorize]
public class ExpenseSplitController(ISplitService splitService) : BaseController
{
    [HttpPost("{expenseId:guid}/split")]
    public async Task<IActionResult> CreateSplit(Guid expenseId, CreateSplitRequest request)
    {
        await splitService.CreateSplitAsync(expenseId, request);
        return Ok<object>(null!, "Split created.");
    }

    [HttpGet("{expenseId:guid}/split")]
    public async Task<IActionResult> GetSplit(Guid expenseId) =>
        Ok(await splitService.GetSplitAsync(expenseId));

    [HttpPut("{expenseId:guid}/split")]
    public async Task<IActionResult> UpdateSplit(Guid expenseId, CreateSplitRequest request)
    {
        await splitService.UpdateSplitAsync(expenseId, request);
        return Ok<object>(null!, "Split updated.");
    }
}

[Route("api/notifications")]
[Authorize]
public class NotificationsController(INotificationService notificationService) : BaseController
{
    [HttpGet]
    public async Task<IActionResult> GetNotifications([FromQuery] bool? isRead) =>
        Ok(await notificationService.GetNotificationsAsync(CurrentUserId, isRead));

    [HttpPut("{notificationId:guid}/read")]
    public async Task<IActionResult> MarkRead(Guid notificationId)
    {
        await notificationService.MarkReadAsync(notificationId);
        return Ok<object>(null!, "Marked as read.");
    }

    [HttpPut("read-all")]
    public async Task<IActionResult> MarkAllRead()
    {
        await notificationService.MarkAllReadAsync(CurrentUserId);
        return Ok<object>(null!, "All marked as read.");
    }
}

[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminController(IAdminService adminService) : BaseController
{
    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard() =>
        Ok(await adminService.GetDashboardAsync());

    [HttpGet("users")]
    public async Task<IActionResult> GetUsers([FromQuery] string? search, [FromQuery] bool? isActive, [FromQuery] int page = 1, [FromQuery] int pageSize = 20) =>
        Ok(await adminService.GetUsersAsync(search, isActive, page, pageSize));

    [HttpPut("users/{userId:guid}/deactivate")]
    public async Task<IActionResult> DeactivateUser(Guid userId)
    {
        await adminService.DeactivateUserAsync(userId);
        return Ok<object>(null!, "User deactivated.");
    }

    [HttpGet("trips")]
    public async Task<IActionResult> GetTrips([FromQuery] string? status, [FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 20) =>
        Ok(await adminService.GetAllTripsAsync(status, search, page, pageSize));

    [HttpGet("audit-logs")]
    public async Task<IActionResult> GetAuditLogs([FromQuery] Guid? userId, [FromQuery] string? action, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate) =>
        Ok(await adminService.GetAuditLogsAsync(userId, action, startDate, endDate));
}
