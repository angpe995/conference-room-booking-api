using ConferenceRoomBookingApi.Models;
using ConferenceRoomBookingApi.Data;
using ConferenceRoomBookingApi.Exceptions;
using Microsoft.EntityFrameworkCore;
namespace ConferenceRoomBookingApi.Services;

public class BookingService
{
    // Morning booking period.
    private const int MorningStartHour = 6;
    private const int MorningEndHour = 9;
    // Standard booking period ends at 18:00.
    private const int StandardEndHour = 18;
    // Peak booking period.
    private const int PeakStartHour = 12;
    private const int PeakEndHour = 14;
    // Evening booking period ends at 23:00.
    private const int EveningEndHour = 23;
    // Price multipliers for different booking periods.
    private const decimal MorningDiscount = 0.90m;
    private const decimal PeakMarkup = 1.15m;
    private const decimal EveningDiscount = 0.80m;
    private const decimal StandardMultiplier = 1.00m;
    // Defines the structure for a pricing rule.
    private record PriceRule(int StartHour, int EndHour, decimal Multiplier);
    // List of predefined pricing rules evaluated during price calculation.
    private readonly List<PriceRule> _priceRules = new()
    {
        new PriceRule(MorningStartHour, MorningEndHour, MorningDiscount),
        new PriceRule(PeakStartHour, PeakEndHour, PeakMarkup),
        // The evening period starts immediately after the standard period ends
        new PriceRule(StandardEndHour, EveningEndHour, EveningDiscount)
    };
    private readonly AppDbContext _context;
    public BookingService(AppDbContext context)
    {
        _context = context;
    }
    // Allowed booking hours.
    private const int BookingStartHour = 6;
    private const int BookingEndHour = 23;
    // Checks whether the booking falls within the allowed hours.
    private bool IsWithinBookingHours(DateTime startTime, DateTime endTime)
    {
        return startTime.TimeOfDay >= TimeSpan.FromHours(BookingStartHour)
            && endTime.TimeOfDay <= TimeSpan.FromHours(BookingEndHour);
    }
    // Creates and stores a new booking.
    public async Task<Booking> CreateBookingAsync(
        Room room,
        DateTime startTime,
        DateTime endTime,
        List<int> selectedServicesIds)
    {
        // Throws an error when attempting to book outside the allowed hours (23:00–06:00). 
        if (!IsWithinBookingHours(startTime, endTime))
        {
            throw new ArgumentException("Bookings are allowed only between 06:00 and 23:00.");
        }
        // Ensures that the booking ends on the same day as it starts
        if (startTime.Date != endTime.Date)
        {
            throw new ArgumentException(
                "Booking must start and end on the same day.");
        }
        // Ensures that the booking end time is later than the start time.
        if (endTime <= startTime)
        {
            throw new ArgumentException(
                "End time must be later than start time.");
        }
        //throws error when the room is alredy booked for the requested time 
        if (!await CheckAvailability(room.Id, startTime, endTime))
        {
            throw new ConflictException("Room is already booked.");
        }
        List<Service> services = room.Services
            .Where(s => selectedServicesIds.Contains(s.Id))
            .ToList();
            
        if (services.Count != selectedServicesIds.Count)
        {
            throw new ArgumentException(
                "One or more selected services were not found.");
        }
        decimal totalPrice = CalculatePrice(services, room, startTime, endTime);
        var booking = new Booking
        {
            RoomId = room.Id,
            StartTime = startTime,
            EndTime = endTime,
            TotalPrice = totalPrice
        };

        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync();
        return booking;
    }
    // Checks whether the room has no conflicting bookings.
    public async Task<bool> CheckAvailability(int roomId, DateTime StartTime,
     DateTime EndTime)
    {
        return !await _context.Bookings.AnyAsync(booking =>
            booking.RoomId == roomId &&
            booking.StartTime < EndTime &&
            StartTime < booking.EndTime);
    }
    // Returns the price multiplier for the given hour.
    private decimal GetPriceMultiplier(DateTime startTime)
    {
        var rule = _priceRules.FirstOrDefault(r =>
            startTime.Hour >= r.StartHour && startTime.Hour < r.EndHour);
        return rule?.Multiplier ?? StandardMultiplier;
    }
    // Calculates the room price for the entire booking duration.
    // Currently supports full-hour bookings only.
    private decimal CalculateRoomPrice(DateTime startTime, DateTime endTime, decimal price)
    {
        // Verify that the booking starts and ends exactly at the top of the hour.
        if (startTime.Minute != 0 || endTime.Minute != 0)
        {
            throw new ArgumentException(
                "Bookings must start and end exactly at the top of the hour (e.g., 10:00, 11:00).");
        }
        decimal totalPrice = 0;
        TimeSpan difference = endTime - startTime;
        if (difference.TotalHours <= 0)
        {
            throw new ArgumentException(
                "Booking duration must be a positive whole number of hours.");
        }
        for (DateTime currentHour = startTime; currentHour < endTime; currentHour = currentHour.AddHours(1))
        {
            totalPrice += GetPriceMultiplier(currentHour) * price;
        }
        return totalPrice;
    }
    // Calculates the total booking price, including room rental and services.
    public decimal CalculatePrice(List<Service> services, Room room, DateTime startTime,
    DateTime endTime)
    {
        decimal totalPrice = CalculateRoomPrice(startTime, endTime, room.PricePerHour);
        foreach (var service in services)
        {
            totalPrice += service.Price;
        }
        return totalPrice;
    }
}