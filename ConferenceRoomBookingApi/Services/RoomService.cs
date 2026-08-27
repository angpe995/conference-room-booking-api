using ConferenceRoomBookingApi.Models;
namespace ConferenceRoomBookingApi.Services;

public class RoomService
{
    private const decimal MinPrice = 0m;
    private const int MinCapacity = 0;
    private readonly List<Room> _rooms = new();
    private int _nextRoomId = 1;
    // Generates a unique ID for a new room.
    private int NextRoomId()
    {
        return _nextRoomId++;
    }
    // Generates a unique ID for a new service.
    private int _nextServiceId = 1;
    private int NextServiceId()
    {
        return _nextServiceId++;
    }
    public Room CreateRoom(
        string name,
        decimal pricePerHour,
        int capacity, List<Service>? services)
    {
        if (pricePerHour <= MinPrice)
        {
            throw new ArgumentException("Price per hour must be greater than zero");
        }
        if (capacity <= MinCapacity)
        {
            throw new ArgumentException("Capacity must be greater than zero");
        }
        Room room = new Room
        {
            Id = NextRoomId(),
            Name = name,
            Capacity = capacity,
            Services = new List<Service>(services ?? new List<Service>())
        };
        room.UpdatePrice(pricePerHour);
        _rooms.Add(room);
        return room;
    }
    public Service CreateServiceForRoom(
        int roomId,
        string name,
        decimal price)
    {
        if (price <= MinPrice)
        {
            throw new ArgumentException("Service price must be greater than zero");
        }
        var room = _rooms.FirstOrDefault(r => r.Id == roomId);
        if (room is null)
        {
            throw new ArgumentException("Room was not found.");
        }
        var service = new Service(name, price)
        {
            Id = NextServiceId()
        };
        room.Services.Add(service);
        return service;
    }
    public Service UpdateServiceForRoom(
        int roomId,
        int serviceId,
        decimal newPrice)
    {
        if (newPrice <= MinPrice)
        {
            throw new ArgumentException("Service price must be greater than zero");
        }
        var room = _rooms.FirstOrDefault(r=>r.Id==roomId);
        if(room is null)
        {
             throw new ArgumentException("Room was not found.");
        }
        var service = room.Services.FirstOrDefault(s => s.Id == serviceId);
        if (service is null)
        {
            throw new ArgumentException("Service was not found for this room.");
        }
        service.UpdatePrice(newPrice);
        return service;
    }
    public Room UpdateRoom(
        int roomId,
        decimal? newPrice,
        Service? newService)
    {
        if (newPrice == null && newService == null)
        {
            throw new ArgumentException("either newPrice or newService must not equal null");
        }
        var room = _rooms.FirstOrDefault(r => r.Id == roomId);
        if (room == null)
        {
            throw new ArgumentException("Room was not found.");
        }
        if (newPrice is decimal price)
        {
            if (price <= MinPrice)
            {
                throw new ArgumentException(
                    "Price per hour must be greater than zero.");
            }
            room.UpdatePrice(price);
        }
        if (newService is not null) room.Services.Add(newService);
        return room;
    }
}