using Microsoft.EntityFrameworkCore;
using TripWise.Application.DTOs;
using TripWise.Application.Interfaces;
using TripWise.Domain.Entities;

namespace TripWise.Application.Services;

public class TripService(IAppDbContext db) : ITripService
{
    public async Task<TripResponse> CreateTripAsync(Guid userId, CreateTripRequest request)
    {
        var trip = new Trip
        {
            CreatedByUserId = userId,
            TripName = request.TripName,
            Destination = request.Destination,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Description = request.Description,
            TripType = request.TripType
        };
        db.Trips.Add(trip);
        db.TripMembers.Add(new TripMember { TripId = trip.TripId, UserId = userId, Role = "Admin" });
        await db.SaveChangesAsync();
        return Map(trip);
    }

    public async Task<IEnumerable<TripResponse>> GetMyTripsAsync(Guid userId)
    {
        var trips = await db.TripMembers
            .Where(m => m.UserId == userId)
            .Select(m => m.Trip)
            .ToListAsync();
        return trips.Select(Map);
    }

    public async Task<TripResponse> GetTripAsync(Guid tripId)
    {
        var trip = await db.Trips.FirstOrDefaultAsync(t => t.TripId == tripId) ?? throw new KeyNotFoundException("Trip not found.");
        return Map(trip);
    }

    public async Task UpdateTripAsync(Guid tripId, UpdateTripRequest request)
    {
        var trip = await db.Trips.FirstOrDefaultAsync(t => t.TripId == tripId) ?? throw new KeyNotFoundException("Trip not found.");
        trip.TripName = request.TripName;
        trip.Destination = request.Destination;
        trip.StartDate = request.StartDate;
        trip.EndDate = request.EndDate;
        trip.Description = request.Description;
        await db.SaveChangesAsync();
    }

    public async Task DeleteTripAsync(Guid tripId)
    {
        var trip = await db.Trips.FirstOrDefaultAsync(t => t.TripId == tripId) ?? throw new KeyNotFoundException("Trip not found.");
        trip.Status = "Cancelled";
        await db.SaveChangesAsync();
    }

    public async Task<TripDashboardResponse> GetDashboardAsync(Guid tripId, Guid userId)
    {
        var budget = await db.BudgetPlans.FirstOrDefaultAsync(b => b.TripId == tripId);
        var totalBudget = budget?.TotalBudget ?? 0;
        var expenses = await db.Expenses.Where(e => e.TripId == tripId).ToListAsync();
        var actualExpense = expenses.Sum(e => e.Amount);
        var settlements = await db.Settlements.Where(s => s.TripId == tripId && s.Status == "Pending").ToListAsync();
        var payable = settlements.Where(s => s.PayerUserId == userId).Sum(s => s.Amount);
        var receivable = settlements.Where(s => s.ReceiverUserId == userId).Sum(s => s.Amount);
        var recent = expenses.OrderByDescending(e => e.CreatedAt).Take(5)
            .Select(e => new ExpenseResponse(e.ExpenseId, e.Amount, e.Category, e.Description, e.ExpenseDate, e.PaidByUserId, e.AttachmentUrl, e.CreatedAt));
        return new TripDashboardResponse(totalBudget, actualExpense, totalBudget - actualExpense, payable, receivable, recent);
    }

    private static TripResponse Map(Trip t) =>
        new(t.TripId, t.TripName, t.Destination, t.StartDate, t.EndDate, t.Description, t.TripType, t.Status, t.CreatedAt);
}
