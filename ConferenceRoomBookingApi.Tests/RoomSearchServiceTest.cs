using ConferenceRoomBookingApi.Data;
using ConferenceRoomBookingApi.DTOs;
using ConferenceRoomBookingApi.Models;
using ConferenceRoomBookingApi.Services;
using Microsoft.EntityFrameworkCore;

namespace ConferenceRoomBookingApi.Tests;

public class RoomSearchServiceTests
{
    private readonly AppDbContext _context;
    private readonly RoomSearchService _roomSearchService;
    private readonly RoomService _roomService;
    private readonly BookingService _bookingService;

    public RoomSearchServiceTests()
    {
        // Configure In-Memory database for testing with a unique name per test instance
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);
        // Initialize services with the EF Core context instead of DataStore
        _roomService = new RoomService(_context);
        _bookingService = new BookingService(_context);
        _roomSearchService = new RoomSearchService(
            _bookingService,
            _context);
    }
    [Fact]
    public void SearchAvailableRooms_ReturnsRoomsWithEnoughCapacity()
    {
        // Arrange
        var roomA = _roomService.CreateRoom(
            "Room A",
            2000m,
            50,
            new List<ServiceRequest>());
        var roomB = _roomService.CreateRoom(
            "Room B",
            3500m,
            100,
            new List<ServiceRequest>());
        // Act
        var result = _roomSearchService.SearchAvailableRooms(
            new DateTime(2026, 9, 1, 10, 0, 0),
            new DateTime(2026, 9, 1, 12, 0, 0),
            50);
        // Assert
        Assert.Contains(roomA, result);
        Assert.Contains(roomB, result);
    }
    [Fact]
    public void SearchAvailableRooms_ExcludesRoomsWithInsufficientCapacity()
    {
        // Arrange
        var room = _roomService.CreateRoom(
            "Room A",
            2000m,
            30,
            new List<ServiceRequest>());
        // Act
        var result = _roomSearchService.SearchAvailableRooms(
            new DateTime(2026, 9, 1, 10, 0, 0),
            new DateTime(2026, 9, 1, 12, 0, 0),
            50);
        // Assert
        Assert.DoesNotContain(room, result);
    }

    [Fact]
    public void SearchAvailableRooms_ExcludesBookedRooms()
    {
        // Arrange
        var room = _roomService.CreateRoom(
            "Room A",
            2000m,
            50,
            new List<ServiceRequest>());
        _bookingService.CreateBooking(
            room,
            new DateTime(2026, 9, 1, 10, 0, 0),
            new DateTime(2026, 9, 1, 12, 0, 0),
            new List<int>());
        // Act
        var result = _roomSearchService.SearchAvailableRooms(
            new DateTime(2026, 9, 1, 11, 0, 0),
            new DateTime(2026, 9, 1, 13, 0, 0),
            50);
        // Assert
        Assert.DoesNotContain(room, result);
    }
    [Fact]
    public void SearchAvailableRooms_ReturnsRoom_WhenBookingDoesNotOverlap()
    {
        // Arrange
        var room = _roomService.CreateRoom(
            "Room A",
            2000m,
            50,
            new List<ServiceRequest>());

        _bookingService.CreateBooking(
            room,
            new DateTime(2026, 9, 1, 10, 0, 0),
            new DateTime(2026, 9, 1, 12, 0, 0),
            new List<int>());
        // Act
        var result = _roomSearchService.SearchAvailableRooms(
            new DateTime(2026, 9, 1, 12, 0, 0),
            new DateTime(2026, 9, 1, 14, 0, 0),
            50);

        // Assert
        Assert.Contains(room, result);
    }
    [Fact]
    public void SearchAvailableRooms_ReturnsEmptyList_WhenNoRoomsAreAvailable()
    {
        // Arrange
        _roomService.CreateRoom(
            "Room A",
            2000m,
            30,
            new List<ServiceRequest>());
        // Act
        var result = _roomSearchService.SearchAvailableRooms(
            new DateTime(2026, 9, 1, 10, 0, 0),
            new DateTime(2026, 9, 1, 12, 0, 0),
            50);
        // Assert
        Assert.Empty(result);
    }
    [Fact]
    public void SearchAvailableRooms_Throws_WhenEndTimeIsNotLaterThanStartTime()
    {
        // Arrange
        var startTime = new DateTime(2026, 9, 1, 12, 0, 0);
        var endTime = new DateTime(2026, 9, 1, 10, 0, 0);
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            _roomSearchService.SearchAvailableRooms(
                startTime,
                endTime,
                50));
    }
    [Fact]
    public void SearchAvailableRooms_Throws_WhenBookingSpansMultipleDays()
    {
        // Arrange
        var startTime = new DateTime(2026, 9, 1, 10, 0, 0);
        var endTime = new DateTime(2026, 9, 2, 12, 0, 0);
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            _roomSearchService.SearchAvailableRooms(
                startTime,
                endTime,
                50));
    }
    [Fact]
    public void SearchAvailableRooms_Throws_WhenStartTimeIsNotExactHour()
    {
        // Arrange
        var startTime = new DateTime(2026, 9, 1, 10, 30, 0);
        var endTime = new DateTime(2026, 9, 1, 12, 0, 0);
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            _roomSearchService.SearchAvailableRooms(
                startTime,
                endTime,
                50));
    }
    [Fact]
    public void SearchAvailableRooms_Throws_WhenEndTimeIsNotExactHour()
    {
        // Arrange
        var startTime = new DateTime(2026, 9, 1, 10, 0, 0);
        var endTime = new DateTime(2026, 9, 1, 12, 30, 0);
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            _roomSearchService.SearchAvailableRooms(
                startTime,
                endTime,
                50));
    }
    [Fact]
    public void SearchAvailableRooms_Throws_WhenCapacityIsZero()
    {
        // Arrange
        var startTime = new DateTime(2026, 9, 1, 10, 0, 0);
        var endTime = new DateTime(2026, 9, 1, 12, 0, 0);
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            _roomSearchService.SearchAvailableRooms(
                startTime,
                endTime,
                0));
    }
    [Fact]
    public void SearchAvailableRooms_Throws_WhenCapacityIsNegative()
    {
        // Arrange
        var startTime = new DateTime(2026, 9, 1, 10, 0, 0);
        var endTime = new DateTime(2026, 9, 1, 12, 0, 0);
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            _roomSearchService.SearchAvailableRooms(
                startTime,
                endTime,
                -10));
    }
}