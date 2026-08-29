using ConferenceRoomBookingApi.Models;
using ConferenceRoomBookingApi.Services;
using ConferenceRoomBookingApi.Data;
using ConferenceRoomBookingApi.Exceptions;
namespace ConferenceRoomBookingApi.Tests;

public class BookingServiceTests
{
    private readonly BookingService _bookingService;
    private readonly DataStore _dataStore= new();
    private readonly Room _room;
    private readonly List<Service> _services;
    public BookingServiceTests()
    {
        _bookingService = new BookingService(_dataStore);
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
    public void CheckAvailability_ReturnsTrue_WhenRoomHasNoBookings()
    {
        // Arrange
        DateTime startTime = new DateTime(2026, 9, 1, 10, 0, 0);
        DateTime endTime = new DateTime(2026, 9, 1, 12, 0, 0);
        // Act
        bool result = _bookingService.CheckAvailability(
            _room.Id,
            startTime,
            endTime);
        // Assert
        Assert.True(result);
    }
    [Fact]
    public void CheckAvailability_ReturnsFalse_WhenBookingOverlaps()
    {
        // Arrange
        DateTime existingStart = new DateTime(2026, 9, 1, 10, 0, 0);
        DateTime existingEnd = new DateTime(2026, 9, 1, 12, 0, 0);
        _bookingService.CreateBooking(
            _room,
            existingStart,
            existingEnd,
            new List<int>());
        // Act
        bool result = _bookingService.CheckAvailability(
            1,
            new DateTime(2026, 9, 1, 11, 0, 0),
            new DateTime(2026, 9, 1, 13, 0, 0));
        // Assert
        Assert.False(result);
    }
    [Fact]
    public void CheckAvailability_ReturnsTrue_WhenNewBookingStartsWhenPreviousEnds()
    {
        // Arrange
        _bookingService.CreateBooking(
            _room,
            new DateTime(2026, 9, 1, 10, 0, 0),
            new DateTime(2026, 9, 1, 12, 0, 0),
            new List<int>());
        // Act
        bool result = _bookingService.CheckAvailability(
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
    public void CreateBooking_CreatesBookingWithCorrectData()
    {
        // Arrange
        var startTime = new DateTime(2026, 9, 1, 10, 0, 0);
        var endTime = new DateTime(2026, 9, 1, 12, 0, 0);
        // Act
        var booking = _bookingService.CreateBooking(
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
    public void CreateBooking_Throws_WhenRoomIsNotAvailable()
    {
        // Arrange
        var startTime = new DateTime(2026, 9, 1, 10, 0, 0);
        var endTime = new DateTime(2026, 9, 1, 12, 0, 0);
        // Act & Assert
        _bookingService.CreateBooking(
            _room,
            startTime,
            endTime,
            new List<int>());
        Assert.Throws<ConflictException>(() =>
        {
            _bookingService.CreateBooking(
                _room,
                startTime,
                endTime,
                new List<int>());
        });
    }
    [Fact]
    public void CheckAvailability_ReturnsTrue_WhenDifferentRoomIsBooked()
    {
        // Arrange
        var anotherRoom = new Room
        {
            Id = 2,
            Name = "Room B",
            Capacity = 100,
        };
            anotherRoom.UpdatePrice(3500);
        _bookingService.CreateBooking(
            anotherRoom,
            new DateTime(2026, 9, 1, 10, 0, 0),
            new DateTime(2026, 9, 1, 12, 0, 0),
            new List<int>());
        // Act
        bool result = _bookingService.CheckAvailability(
            _room.Id,
            new DateTime(2026, 9, 1, 10, 0, 0),
            new DateTime(2026, 9, 1, 12, 0, 0));

        // Assert
        Assert.True(result);
    }
    [Fact]
    public void CreateBooking_GeneratesUniqueIds()
    {
        var firstBooking = _bookingService.CreateBooking(
            _room,
            new DateTime(2026, 9, 1, 10, 0, 0),
            new DateTime(2026, 9, 1, 12, 0, 0),
            new List<int>());
        var secondBooking = _bookingService.CreateBooking(
            _room,
            new DateTime(2026, 9, 1, 12, 0, 0),
            new DateTime(2026, 9, 1, 14, 0, 0),
            new List<int>());
        Assert.NotEqual(firstBooking.Id, secondBooking.Id);
    }
    [Fact]
    public void CreateBooking_Throws_WhenEndTimeIsNotLaterThanStartTime()
    {
        // Arrange
        var startTime = new DateTime(2026, 9, 1, 12, 0, 0);
        var endTime = new DateTime(2026, 9, 1, 10, 0, 0);
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            _bookingService.CreateBooking(
                _room,
                startTime,
                endTime,
                new List<int>()));
    }
    [Fact]
    public void CreateBooking_Throws_WhenBookingSpansMultipleDays()
    {
        // Arrange
        var startTime = new DateTime(2026, 9, 1, 10, 0, 0);
        var endTime = new DateTime(2026, 9, 2, 12, 0, 0);
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            _bookingService.CreateBooking(
                _room,
                startTime,
                endTime,
                new List<int>()));
    }
    [Fact]
    public void CreateBooking_Throws_WhenStartTimeIsNotExactHour()
    {
        // Arrange
        var startTime = new DateTime(2026, 9, 1, 10, 30, 0);
        var endTime = new DateTime(2026, 9, 1, 12, 0, 0);
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            _bookingService.CreateBooking(
                _room,
                startTime,
                endTime,
                new List<int>()));
    }
    [Fact]
    public void CreateBooking_Throws_WhenEndTimeIsNotExactHour()
    {
        // Arrange
        var startTime = new DateTime(2026, 9, 1, 10, 0, 0);
        var endTime = new DateTime(2026, 9, 1, 12, 30, 0);
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            _bookingService.CreateBooking(
                _room,
                startTime,
                endTime,
                new List<int>()));
    }

}
