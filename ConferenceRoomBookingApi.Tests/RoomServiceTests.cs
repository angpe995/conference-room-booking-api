using ConferenceRoomBookingApi.Models;
using ConferenceRoomBookingApi.Services;

namespace ConferenceRoomBookingApi.Tests;

public class RoomServiceTests
{
    private readonly RoomService _roomService;
    private const string RoomName = "A";
    private const int RoomCapacity = 50;
    private const decimal RoomPricePerHour = 2000m;
    private const string ServiceName = "Projector";
    private const decimal ServicePricePerHour = 2000m;
    public RoomServiceTests()
    {
        _roomService = new RoomService();
    }
    [Fact]
    public void CreateRoom_ReturnsRoom_WhenPriceAndCapacityAreGreaterThanZero()
    {
        Room room = _roomService.CreateRoom(
            RoomName,
            RoomPricePerHour,
            RoomCapacity,
            null);

        Assert.NotNull(room);
        Assert.Equal(RoomName, room.Name);
        Assert.Equal(RoomPricePerHour, room.PricePerHour);
        Assert.Equal(RoomCapacity, room.Capacity);
    }
    [Fact]
    public void CreateRoom_Throws_WhenPriceIsLessOrEqualToZero()
    {
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
        Room room = _roomService.CreateRoom(
            RoomName,
            RoomPricePerHour,
            RoomCapacity,
            null);

        Service newService = _roomService.CreateServiceForRoom(
            room.Id,
            ServiceName,
            ServicePricePerHour);

        Assert.NotNull(newService);
        Assert.Contains(room.Services, s => s.Id == newService.Id);
        Assert.Equal(ServiceName, newService.Name);
        Assert.Equal(ServicePricePerHour, newService.Price);
    }
    [Fact]
    public void CreateServiceForRoom_Throws_WhenPriceIsLessThanOrEqualToZero()
    {
        Room room = _roomService.CreateRoom(
            RoomName,
            RoomPricePerHour,
            RoomCapacity,
            null);

        Assert.NotNull(room);

        Assert.Throws<ArgumentException>(() =>
            _roomService.CreateServiceForRoom(
                room.Id,
                ServiceName,
                -200));
    }

    [Fact]
    public void UpdateServiceForRoom_ReturnsService_WhenPriceIsGreaterThanZero()
    {
        Room room = _roomService.CreateRoom(
            RoomName,
            RoomPricePerHour,
            RoomCapacity,
            null);

        Service service = _roomService.CreateServiceForRoom(
            room.Id,
            ServiceName,
            ServicePricePerHour);

        Service updatedService = _roomService.UpdateServiceForRoom(
            room.Id,
            service.Id,
            300);

        Assert.NotNull(updatedService);
        Assert.Contains(room.Services, s => s.Id == updatedService.Id);
        Assert.Equal(300, updatedService.Price);
    }
    [Fact]
    public void UpdateServiceForRoom_Throws_WhenPriceIsLessThanOrEqualToZero()
    {
        Room room = _roomService.CreateRoom(
            RoomName,
            RoomPricePerHour,
            RoomCapacity,
            null);
        Service service = _roomService.CreateServiceForRoom(
            room.Id,
            ServiceName,
            ServicePricePerHour);
        Assert.Throws<ArgumentException>(() =>
            _roomService.UpdateServiceForRoom(
                room.Id,
                service.Id,
                -200));
    }
    [Fact]
    public void UpdateRoom_ReturnsRoom_WhenPriceIsGreaterThanZero()
    {
        Room room = _roomService.CreateRoom(
            RoomName,
            RoomPricePerHour,
            RoomCapacity,
            null);

        Room updatedRoom = _roomService.UpdateRoom(
            room.Id,
            3000,
            null);
        Assert.NotNull(updatedRoom);
        Assert.Equal(3000, updatedRoom.PricePerHour);
    }
    [Fact]
    public void UpdateRoom_ReturnsRoom_WhenServiceProvided()
    {
        Room room = _roomService.CreateRoom(
            RoomName,
            RoomPricePerHour,
            RoomCapacity,
            null);
        Service service = new Service(
            ServiceName,
            ServicePricePerHour);
        Room updatedRoom = _roomService.UpdateRoom(
            room.Id,
            null,
            service);

        Assert.NotNull(updatedRoom);
        Assert.Contains(updatedRoom.Services, s => s.Name == ServiceName);
    }
    [Fact]
    public void UpdateRoom_Throws_WhenNoPriceOrServiceProvided()
    {
        Room room = _roomService.CreateRoom(
            RoomName,
            RoomPricePerHour,
            RoomCapacity,
            null);
        Assert.Throws<ArgumentException>(() =>
            _roomService.UpdateRoom(
                room.Id,
                null,
                null));
    }
}