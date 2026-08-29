using ConferenceRoomBookingApi.Models;
using ConferenceRoomBookingApi.Services;
using ConferenceRoomBookingApi.Data;
using ConferenceRoomBookingApi.Exceptions;
using Microsoft.EntityFrameworkCore;
namespace ConferenceRoomBookingApi.Tests;

public class BookingServiceTests
{
    private readonly BookingService _bookingService;
    private readonly AppDbContext _context;
    private readonly Room _room;
    private readonly List<Service> _services;
    public BookingServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
           .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
           .Options;
        _context = new AppDbContext(options);
        _bookingService = new BookingService(_context);
        _room = new Room
        {
            Id = 1,
            Name = "Room A",
            Capacity = 50
        };
        _room.UpdatePrice(2000);
        _services = new List<Service>();
        _services.Add(new Service("projector", 500));

    }
    [Fact]
    public async Task CheckAvailability_ReturnsTrue_WhenRoomHasNoBookings()
    {
        // Arrange
        DateTime startTime = new DateTime(2026, 9, 1, 10, 0, 0);
        DateTime endTime = new DateTime(2026, 9, 1, 12, 0, 0);
        // Act
        bool result = await _bookingService.CheckAvailability(
            _room.Id,
            startTime,
            endTime);
        // Assert
        Assert.True(result);
    }
    [Fact]
    public async Task CheckAvailability_ReturnsFalse_WhenBookingOverlaps()
    {
        // Arrange
        DateTime existingStart = new DateTime(2026, 9, 1, 10, 0, 0);
        DateTime existingEnd = new DateTime(2026, 9, 1, 12, 0, 0);
        await _bookingService.CreateBookingAsync(
            _room,
            existingStart,
            existingEnd,
            new List<int>());
        // Act
        bool result = await _bookingService.CheckAvailability(
            1,
            new DateTime(2026, 9, 1, 11, 0, 0),
            new DateTime(2026, 9, 1, 13, 0, 0));
        // Assert
        Assert.False(result);
    }
    [Fact]
    public async Task CheckAvailability_ReturnsTrue_WhenNewBookingStartsWhenPreviousEnds()
    {
        // Arrange
        await _bookingService.CreateBookingAsync(
            _room,
            new DateTime(2026, 9, 1, 10, 0, 0),
            new DateTime(2026, 9, 1, 12, 0, 0),
            new List<int>());
        // Act
        bool result = await _bookingService.CheckAvailability(
            1,
            new DateTime(2026, 9, 1, 12, 0, 0),
            new DateTime(2026, 9, 1, 14, 0, 0));
        // Assert
        Assert.True(result);
    }
    [Fact]
    public void CalculatePrice_AppliesPeakMarkup()
    {
        // Arrange
        var startTime = new DateTime(2026, 9, 1, 12, 0, 0);
        var endTime = new DateTime(2026, 9, 1, 14, 0, 0);
        // Act
        decimal result = _bookingService.CalculatePrice(
            _services,
            _room,
            startTime,
            endTime);
        // Assert
        Assert.Equal(5100, result);
    }
    [Fact]
    public void CalculatePrice_ReturnsBasePrice_DuringStandardHours()
    {
        // Arrange
        var startTime = new DateTime(2026, 9, 1, 10, 0, 0);
        var endTime = new DateTime(2026, 9, 1, 12, 0, 0);
        // Act
        decimal result = _bookingService.CalculatePrice(
            _services,
            _room,
            startTime,
            endTime);
        // Assert
        Assert.Equal(4500, result);
    }
    [Fact]
    public void CalculatePrice_AppliesMorningDiscount()
    {
        // Arrange
        var startTime = new DateTime(2026, 9, 1, 6, 0, 0);
        var endTime = new DateTime(2026, 9, 1, 8, 0, 0);
        // Act
        decimal result = _bookingService.CalculatePrice(
            _services,
            _room,
            startTime,
            endTime);
        //Assert
        Assert.Equal(4100, result);
    }
    [Fact]
    public void CalculatePrice_AppliesEveningDiscount()
    {
        // Arrange
        var startTime = new DateTime(2026, 9, 1, 18, 0, 0);
        var endTime = new DateTime(2026, 9, 1, 20, 0, 0);
        // Act
        decimal result = _bookingService.CalculatePrice(
            _services,
            _room,
            startTime,
            endTime);
        // Assert
        Assert.Equal(3700, result);
    }
    [Fact]
    public async Task CreateBooking_CreatesBookingWithCorrectData()
    {
        // Arrange
        var startTime = new DateTime(2026, 9, 1, 10, 0, 0);
        var endTime = new DateTime(2026, 9, 1, 12, 0, 0);
        // Act
        var booking = await _bookingService.CreateBookingAsync(
            _room,
            startTime,
            endTime,
            new List<int>());
        // Assert
        Assert.Equal(1, booking.Id);
        Assert.Equal(1, booking.RoomId);
        Assert.Equal(startTime, booking.StartTime);
        Assert.Equal(endTime, booking.EndTime);
        Assert.Equal(4000, booking.TotalPrice);
    }
    [Fact]
    public async Task CreateBooking_Throws_WhenRoomIsNotAvailable()
    {
        // Arrange
        var startTime = new DateTime(2026, 9, 1, 10, 0, 0);
        var endTime = new DateTime(2026, 9, 1, 12, 0, 0);
        // Act & Assert
        await _bookingService.CreateBookingAsync(
            _room,
            startTime,
            endTime,
            new List<int>());
        await Assert.ThrowsAsync<ConflictException>(async () =>
        {
            await _bookingService.CreateBookingAsync(
                _room,
                startTime,
                endTime,
                new List<int>());
        });
    }
    [Fact]
    public async Task CheckAvailability_ReturnsTrue_WhenDifferentRoomIsBooked()
    {
        // Arrange
        var anotherRoom = new Room
        {
            Id = 2,
            Name = "Room B",
            Capacity = 100,
        };
        anotherRoom.UpdatePrice(3500);
        await _bookingService.CreateBookingAsync(
            anotherRoom,
            new DateTime(2026, 9, 1, 10, 0, 0),
            new DateTime(2026, 9, 1, 12, 0, 0),
            new List<int>());
        // Act
        bool result = await _bookingService.CheckAvailability(
            _room.Id,
            new DateTime(2026, 9, 1, 10, 0, 0),
            new DateTime(2026, 9, 1, 12, 0, 0));

        // Assert
        Assert.True(result);
    }
    [Fact]
    public async Task CreateBooking_GeneratesUniqueIds()
    {
        var firstBooking = await _bookingService.CreateBookingAsync(
            _room,
            new DateTime(2026, 9, 1, 10, 0, 0),
            new DateTime(2026, 9, 1, 12, 0, 0),
            new List<int>());
        var secondBooking = await _bookingService.CreateBookingAsync(
            _room,
            new DateTime(2026, 9, 1, 12, 0, 0),
            new DateTime(2026, 9, 1, 14, 0, 0),
            new List<int>());
        Assert.NotEqual(firstBooking.Id, secondBooking.Id);
    }
    [Fact]
    public async Task CreateBooking_Throws_WhenEndTimeIsNotLaterThanStartTime()
    {
        // Arrange
        var startTime = new DateTime(2026, 9, 1, 12, 0, 0);
        var endTime = new DateTime(2026, 9, 1, 10, 0, 0);
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(async() =>
            await _bookingService.CreateBookingAsync(
                _room,
                startTime,
                endTime,
                new List<int>()));
    }
    [Fact]
    public async Task CreateBooking_Throws_WhenBookingSpansMultipleDays()
    {
        // Arrange
        var startTime = new DateTime(2026, 9, 1, 10, 0, 0);
        var endTime = new DateTime(2026, 9, 2, 12, 0, 0);
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(async() =>
            await _bookingService.CreateBookingAsync(
                _room,
                startTime,
                endTime,
                new List<int>()));
    }
    [Fact]
    public async Task CreateBooking_Throws_WhenStartTimeIsNotExactHour()
    {
        // Arrange
        var startTime = new DateTime(2026, 9, 1, 10, 30, 0);
        var endTime = new DateTime(2026, 9, 1, 12, 0, 0);
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(async() =>
            await _bookingService.CreateBookingAsync(
                _room,
                startTime,
                endTime,
                new List<int>()));
    }
    [Fact]
    public async Task CreateBooking_Throws_WhenEndTimeIsNotExactHour()
    {
        // Arrange
        var startTime = new DateTime(2026, 9, 1, 10, 0, 0);
        var endTime = new DateTime(2026, 9, 1, 12, 30, 0);
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(async() =>
            await _bookingService.CreateBookingAsync(
                _room,
                startTime,
                endTime,
                new List<int>()));
    }
    [Fact]
    public async Task CreateBooking_Throws_WhenServicesNotFounded()
    {
        // Arrange
        var startTime = new DateTime(2026, 9, 1, 10, 0, 0);
        var endTime = new DateTime(2026, 9, 1, 12, 0, 0);
        // Act & Assert
        var invalidServiceIds = new List<int> { 999 };
        await Assert.ThrowsAsync<ArgumentException>(async() =>
            await _bookingService.CreateBookingAsync(
                _room,
                startTime,
                endTime,
                invalidServiceIds));
    }

}
