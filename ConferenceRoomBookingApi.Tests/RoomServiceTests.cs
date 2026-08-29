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
    public void CreateRoom_ReturnsRoom_WhenPriceAndCapacityAreGreaterThanZero()
    {
        // Arrange & Act
        Room room = _roomService.CreateRoom(
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
    public void CreateRoom_Throws_WhenPriceIsLessOrEqualToZero()
    {
        // Arrange

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            _roomService.CreateRoom(
                RoomName,
                -1,
                RoomCapacity,
                null));
    }
    [Fact]
    public void CreateRoom_Throws_WhenCapacityIsLessOrEqualToZero()
    {
        // Arrange
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            _roomService.CreateRoom(
                RoomName,
                RoomPricePerHour,
                -10,
                null));
    }
    [Fact]
    public void CreateServiceForRoom_ReturnsService_WhenPriceIsGreaterThanZero()
    {
        // Arrange
        Room room = _roomService.CreateRoom(
            RoomName,
            RoomPricePerHour,
            RoomCapacity,
            null);
        // Act
        Service newService = _roomService.CreateServiceForRoom(
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
    public void CreateServiceForRoom_Throws_WhenPriceIsLessThanOrEqualToZero()
    {
        // Arrange
        Room room = _roomService.CreateRoom(
            RoomName,
            RoomPricePerHour,
            RoomCapacity,
            null);

        Assert.NotNull(room);

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            _roomService.CreateServiceForRoom(
                room.Id,
                ServiceName,
                -200));
    }

    [Fact]
    public void UpdateServiceForRoom_ReturnsService_WhenPriceIsGreaterThanZero()
    {
        // Arrange
        Room room = _roomService.CreateRoom(
            RoomName,
            RoomPricePerHour,
            RoomCapacity,
            null);

        Service service = _roomService.CreateServiceForRoom(
            room.Id,
            ServiceName,
            ServicePricePerHour);
        // Act
        Service updatedService = _roomService.UpdateServiceForRoom(
            room.Id,
            service.Id,
            300);
        // Assert
        Assert.NotNull(updatedService);
        Assert.Contains(room.Services, s => s.Id == updatedService.Id);
        Assert.Equal(300, updatedService.Price);
    }
    [Fact]
    public void UpdateServiceForRoom_Throws_WhenPriceIsLessThanOrEqualToZero()
    {
        // Arrange
        Room room = _roomService.CreateRoom(
            RoomName,
            RoomPricePerHour,
            RoomCapacity,
            null);
        Service service = _roomService.CreateServiceForRoom(
            room.Id,
            ServiceName,
            ServicePricePerHour);
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            _roomService.UpdateServiceForRoom(
                room.Id,
                service.Id,
                -200));
    }
    [Fact]
    public void UpdateRoom_ReturnsRoom_WhenPriceIsGreaterThanZero()
    {
        // Arrange
        Room room = _roomService.CreateRoom(
            RoomName,
            RoomPricePerHour,
            RoomCapacity,
            null);
        // Act
        Room updatedRoom = _roomService.UpdateRoom(
            room.Id,
            3000,
            null);

        // Assert
        Assert.NotNull(updatedRoom);
        Assert.Equal(3000, updatedRoom.PricePerHour);
    }
    [Fact]
    public void UpdateRoom_ReturnsRoom_WhenServiceProvided()
    {
        // Arrange
        Room room = _roomService.CreateRoom(
            RoomName,
            RoomPricePerHour,
            RoomCapacity,
            null);
        ServiceRequest service = new ServiceRequest{Name=ServiceName,Price=RoomPricePerHour};
        // Act
        Room updatedRoom = _roomService.UpdateRoom(
            room.Id,
            null,
            service);

        // Assert
        Assert.NotNull(updatedRoom);
        Assert.Contains(updatedRoom.Services, s => s.Name == ServiceName);
    }
    [Fact]
    public void UpdateRoom_Throws_WhenNoPriceOrServiceProvided()
    {
        // Arrange
        Room room = _roomService.CreateRoom(
            RoomName,
            RoomPricePerHour,
            RoomCapacity,
            null);
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            _roomService.UpdateRoom(
                room.Id,
                null,
                null));
    }
    [Fact]
    public void DeleteRoom_Throws_WhenRoomDoesNotExist()
    {
        // Arrange
        const int missingRoomId = 999;
        // Act & Assert
        Assert.Throws<NotFoundException>(() =>
            _roomService.DeleteRoom(missingRoomId));
    }
    [Fact]
    public void DeleteRoom_RemovesExistingRoom()
    {
        // Arrange
        Room room = _roomService.CreateRoom(
            RoomName,
            RoomPricePerHour,
            RoomCapacity,
            new List<ServiceRequest>());
        // Act
        _roomService.DeleteRoom(room.Id);
        // Assert
        Assert.Throws<NotFoundException>(() =>
            _roomService.UpdateRoom(room.Id, 2500, null));
    }
    [Fact]
    public void CreateRoom_GeneratesUniqueIds()
    {
        // Arrange
        var firstRoom = _roomService.CreateRoom(
            RoomName,
            RoomPricePerHour,
            RoomCapacity,
            new List<ServiceRequest>());
        var secondRoom = _roomService.CreateRoom(
            "Room B",
            3000m,
            100,
            new List<ServiceRequest>());
        // Assert
        Assert.NotEqual(firstRoom.Id, secondRoom.Id);
    }
}