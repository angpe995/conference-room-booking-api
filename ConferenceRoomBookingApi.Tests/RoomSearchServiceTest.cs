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
    public async Task SearchAvailableRoomsAsync_ReturnsRoomsWithEnoughCapacity()
    {
        // Arrange
        var roomA = await _roomService.CreateRoomAsync(
            "Room A",
            2000m,
            50,
            new List<ServiceRequest>());
        var roomB = await _roomService.CreateRoomAsync(
            "Room B",
            3500m,
            100,
            new List<ServiceRequest>());
        // Act
        var result = await _roomSearchService.SearchAvailableRoomsAsync(
            new DateTime(2026, 9, 1, 10, 0, 0),
            new DateTime(2026, 9, 1, 12, 0, 0),
            50);
        // Assert
        Assert.Contains(roomA, result);
        Assert.Contains(roomB, result);
    }
    [Fact]
    public async Task SearchAvailableRoomsAsync_ExcludesRoomsWithInsufficientCapacity()
    {
        // Arrange
        var room =await _roomService.CreateRoomAsync(
            "Room A",
            2000m,
            30,
            new List<ServiceRequest>());
        // Act
        var result =await _roomSearchService.SearchAvailableRoomsAsync(
            new DateTime(2026, 9, 1, 10, 0, 0),
            new DateTime(2026, 9, 1, 12, 0, 0),
            50);
        // Assert
        Assert.DoesNotContain(room, result);
    }

    [Fact]
    public async Task SearchAvailableRoomsAsync_ExcludesBookedRooms()
    {
        // Arrange
        var room =await _roomService.CreateRoomAsync(
            "Room A",
            2000m,
            50,
            new List<ServiceRequest>());
        await _bookingService.CreateBookingAsync(
            room,
            new DateTime(2026, 9, 1, 10, 0, 0),
            new DateTime(2026, 9, 1, 12, 0, 0),
            new List<int>());
        // Act
        var result = await _roomSearchService.SearchAvailableRoomsAsync(
            new DateTime(2026, 9, 1, 11, 0, 0),
            new DateTime(2026, 9, 1, 13, 0, 0),
            50);
        // Assert
        Assert.DoesNotContain(room, result);
    }
    [Fact]
    public async Task SearchAvailableRoomsAsync_ReturnsRoom_WhenBookingDoesNotOverlap()
    {
        // Arrange
        var room = await _roomService.CreateRoomAsync(
            "Room A",
            2000m,
            50,
            new List<ServiceRequest>());

        await _bookingService.CreateBookingAsync(
            room,
            new DateTime(2026, 9, 1, 10, 0, 0),
            new DateTime(2026, 9, 1, 12, 0, 0),
            new List<int>());
        // Act
        var result =await _roomSearchService.SearchAvailableRoomsAsync(
            new DateTime(2026, 9, 1, 12, 0, 0),
            new DateTime(2026, 9, 1, 14, 0, 0),
            50);

        // Assert
        Assert.Contains(room, result);
    }
    [Fact]
    public async Task SearchAvailableRoomsAsync_ReturnsEmptyList_WhenNoRoomsAreAvailable()
    {
        // Arrange
        await _roomService.CreateRoomAsync(
            "Room A",
            2000m,
            30,
            new List<ServiceRequest>());
        // Act
        var result =await _roomSearchService.SearchAvailableRoomsAsync(
            new DateTime(2026, 9, 1, 10, 0, 0),
            new DateTime(2026, 9, 1, 12, 0, 0),
            50);
        // Assert
        Assert.Empty(result);
    }
    [Fact]
    public async Task SearchAvailableRoomsAsync_Throws_WhenEndTimeIsNotLaterThanStartTime()
    {
        // Arrange
        var startTime = new DateTime(2026, 9, 1, 12, 0, 0);
        var endTime = new DateTime(2026, 9, 1, 10, 0, 0);
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await _roomSearchService.SearchAvailableRoomsAsync(
                startTime,
                endTime,
                50));
    }
    [Fact]
    public async Task SearchAvailableRoomsAsync_Throws_WhenBookingSpansMultipleDays()
    {
        // Arrange
        var startTime = new DateTime(2026, 9, 1, 10, 0, 0);
        var endTime = new DateTime(2026, 9, 2, 12, 0, 0);
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(async () =>
           await _roomSearchService.SearchAvailableRoomsAsync(
                startTime,
                endTime,
                50));
    }
    [Fact]
    public async Task SearchAvailableRoomsAsync_Throws_WhenStartTimeIsNotExactHour()
    {
        // Arrange
        var startTime = new DateTime(2026, 9, 1, 10, 30, 0);
        var endTime = new DateTime(2026, 9, 1, 12, 0, 0);
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(async() =>
           await _roomSearchService.SearchAvailableRoomsAsync(
                startTime,
                endTime,
                50));
    }
    [Fact]
    public async Task SearchAvailableRoomsAsync_Throws_WhenEndTimeIsNotExactHour()
    {
        // Arrange
        var startTime = new DateTime(2026, 9, 1, 10, 0, 0);
        var endTime = new DateTime(2026, 9, 1, 12, 30, 0);
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _roomSearchService.SearchAvailableRoomsAsync(
                startTime,
                endTime,
                50));
    }
    [Fact]
    public async Task SearchAvailableRoomsAsync_Throws_WhenCapacityIsZero()
    {
        // Arrange
        var startTime = new DateTime(2026, 9, 1, 10, 0, 0);
        var endTime = new DateTime(2026, 9, 1, 12, 0, 0);
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await _roomSearchService.SearchAvailableRoomsAsync(
                startTime,
                endTime,
                0));
    }
    [Fact]
    public async Task SearchAvailableRoomsAsync_Throws_WhenCapacityIsNegative()
    {
        // Arrange
        var startTime = new DateTime(2026, 9, 1, 10, 0, 0);
        var endTime = new DateTime(2026, 9, 1, 12, 0, 0);
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await _roomSearchService.SearchAvailableRoomsAsync(
                startTime,
                endTime,
                -10));
    }
}