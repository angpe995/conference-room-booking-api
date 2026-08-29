using ConferenceRoomBookingApi.Models;
using ConferenceRoomBookingApi.Services;
using ConferenceRoomBookingApi.Data;
using ConferenceRoomBookingApi.DTOs;
using ConferenceRoomBookingApi.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace ConferenceRoomBookingApi.Tests;

public class RoomServiceTests
{
    private readonly RoomService _roomService;
    private const string RoomName = "A";
    private const int RoomCapacity = 50;
    private const decimal RoomPricePerHour = 2000m;
    private const string ServiceName = "Projector";
    private const decimal ServicePricePerHour = 500m;
    private readonly AppDbContext _context;
    public RoomServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);
        _roomService = new RoomService(_context);
    }
    [Fact]
    public async Task CreateRoom_ReturnsRoom_WhenPriceAndCapacityAreGreaterThanZero()
    {
        // Arrange & Act
        Room room = await _roomService.CreateRoomAsync(
            RoomName,
            RoomPricePerHour,
            RoomCapacity,
            null);

        // Assert
        Assert.NotNull(room);
        Assert.Equal(RoomName, room.Name);
        Assert.Equal(RoomPricePerHour, room.PricePerHour);
        Assert.Equal(RoomCapacity, room.Capacity);
    }
    [Fact]
    public async Task CreateRoom_Throws_WhenPriceIsLessOrEqualToZero()
    {
        // Arrange

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await _roomService.CreateRoomAsync(
                RoomName,
                -1,
                RoomCapacity,
                null));
    }
    [Fact]
    public async Task CreateRoom_Throws_WhenCapacityIsLessOrEqualToZero()
    {
        // Arrange
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(async () =>
           await _roomService.CreateRoomAsync(
                RoomName,
                RoomPricePerHour,
                -10,
                null));
    }
    [Fact]
    public async Task CreateServiceForRoom_ReturnsService_WhenPriceIsGreaterThanZero()
    {
        // Arrange
        Room room = await _roomService.CreateRoomAsync(
            RoomName,
            RoomPricePerHour,
            RoomCapacity,
            null);
        // Act
        Service newService = await _roomService.CreateServiceForRoomAsync(
            room.Id,
            ServiceName,
            ServicePricePerHour);
        // Assert
        Assert.NotNull(newService);
        Assert.Contains(room.Services, s => s.Id == newService.Id);
        Assert.Equal(ServiceName, newService.Name);
        Assert.Equal(ServicePricePerHour, newService.Price);
    }
    [Fact]
    public async Task CreateServiceForRoom_Throws_WhenPriceIsLessThanOrEqualToZero()
    {
        // Arrange
        Room room =await _roomService.CreateRoomAsync(
            RoomName,
            RoomPricePerHour,
            RoomCapacity,
            null);

        Assert.NotNull(room);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await _roomService.CreateServiceForRoomAsync(
                room.Id,
                ServiceName,
                -200));
    }
    [Fact]
    public async Task UpdateRoom_ReturnsRoom_WhenPriceIsGreaterThanZero()
    {
        // Arrange
        Room room = await _roomService.CreateRoomAsync(
            RoomName,
            RoomPricePerHour,
            RoomCapacity,
            null);
        // Act
        Room updatedRoom = await _roomService.UpdateRoomAsync(
            room.Id,
            3000,
            null);

        // Assert
        Assert.NotNull(updatedRoom);
        Assert.Equal(3000, updatedRoom.PricePerHour);
    }
    [Fact]
    public async Task UpdateRoom_ReturnsRoom_WhenServiceProvided()
    {
        // Arrange
        Room room = await _roomService.CreateRoomAsync(
            RoomName,
            RoomPricePerHour,
            RoomCapacity,
            null);
        ServiceRequest service = new ServiceRequest{Name=ServiceName,Price=RoomPricePerHour};
        // Act
        Room updatedRoom = await _roomService.UpdateRoomAsync(
            room.Id,
            null,
            service);

        // Assert
        Assert.NotNull(updatedRoom);
        Assert.Contains(updatedRoom.Services, s => s.Name == ServiceName);
    }
    [Fact]
    public async Task UpdateRoom_Throws_WhenNoPriceOrServiceProvided()
    {
        // Arrange
        Room room = await _roomService.CreateRoomAsync(
            RoomName,
            RoomPricePerHour,
            RoomCapacity,
            null);
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(async () =>
           await _roomService.UpdateRoomAsync(
                room.Id,
                null,
                null));
    }
    [Fact]
    public async Task DeleteRoom_Throws_WhenRoomDoesNotExist()
    {
        // Arrange
        const int missingRoomId = 999;
        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(async () =>
            await _roomService.DeleteRoomAsync(missingRoomId));
    }
    [Fact]
    public async Task DeleteRoom_RemovesExistingRoom()
    {
        // Arrange
        Room room = await _roomService.CreateRoomAsync(
            RoomName,
            RoomPricePerHour,
            RoomCapacity,
            new List<ServiceRequest>());
        // Act
        await _roomService.DeleteRoomAsync(room.Id);
        // Assert
        await Assert.ThrowsAsync<NotFoundException>(async () =>
           await _roomService.UpdateRoomAsync(room.Id, 2500, null));
    }
    [Fact]
    public async Task CreateRoom_GeneratesUniqueIds()
    {
        // Arrange
        var firstRoom =await _roomService.CreateRoomAsync(
            RoomName,
            RoomPricePerHour,
            RoomCapacity,
            new List<ServiceRequest>());
        var secondRoom =await _roomService.CreateRoomAsync(
            "Room B",
            3000m,
            100,
            new List<ServiceRequest>());
        // Assert
        Assert.NotEqual(firstRoom.Id, secondRoom.Id);
    }
}