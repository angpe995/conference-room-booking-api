using ConferenceRoomBookingApi.Models;
using ConferenceRoomBookingApi.Data;
using ConferenceRoomBookingApi.Exceptions;
using ConferenceRoomBookingApi.DTOs;
using Microsoft.EntityFrameworkCore;
namespace ConferenceRoomBookingApi.Services;

// Service for generating booking and revenue reports.
public class ReportsService
{
    // Operating hours for the booking system.
    private const int FirstHour = 6;
    private const int LastHour = 22;
    private const int MinBookingCount = 0;
    private readonly AppDbContext _context;
    public ReportsService(AppDbContext context)
    {
        _context = context;
    }
     // Generates a report showing the number of bookings for each operating hour.
    public async Task<List<HourlyBookingReportDto>> GetHourlyBookingReport()
    {
        // Initialize result with zero bookings for each operating hour.
        var result = Enumerable.Range(FirstHour, LastHour - FirstHour + 1)
            .Select(hour => new HourlyBookingReportDto
            {
                Hour = hour,
                BookingCount = MinBookingCount
            })
            .ToList();
        // Retrieve all active bookings from the database.
        var bookings = await _context.Bookings.ToListAsync();
        // Iterate through each booking and increment the count for each hour it occupies.
        foreach (var booking in bookings)
        {
            for (int hour = booking.StartTime.Hour;
                 hour < booking.EndTime.Hour;
                 hour++)
            {
                result[hour - FirstHour].BookingCount++;
            }
        }
        return result;
    }
    // Generates a revenue report for each room.
    public async Task<List<RoomRevenueReportDto>> GetRoomRevenueReport(
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        // Validate that the date range is correct if both dates are provided.
        if (startDate.HasValue &&
            endDate.HasValue &&
            endDate <= startDate)
        {
            throw new ArgumentException(
                "End date must be later than start date.");
        }
        // Build the bookings query with optional date filters.
        var bookingsQuery = _context.Bookings.AsQueryable();
        if (startDate.HasValue)
        {
            bookingsQuery = bookingsQuery.Where(b => b.StartTime >= startDate.Value);
        }
        if (endDate.HasValue)
        {
            bookingsQuery = bookingsQuery.Where(b => b.StartTime < endDate.Value);
        }
        // Execute the query and retrieve all rooms.
        List<Booking> bookings = await bookingsQuery.ToListAsync();
        List<Room> rooms = await _context.Rooms.ToListAsync();
        // Build the revenue report by aggregating booking data per room.
        List<RoomRevenueReportDto> result = new();
        foreach (Room room in rooms)
        {
            // Calculate total bookings and revenue for the current room.
            int totalBookingCount = bookings
                .Count(b => b.RoomId == room.Id);
            decimal totalRevenue = bookings
                .Where(b => b.RoomId == room.Id)
                .Sum(b => b.TotalPrice);
            result.Add(new RoomRevenueReportDto
            {
                RoomId = room.Id,
                RoomName = room.Name,
                BookingCount = totalBookingCount,
                TotalRevenue = totalRevenue
            });
        }
        return result;
    }
}