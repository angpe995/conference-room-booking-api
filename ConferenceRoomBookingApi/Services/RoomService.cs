using ConferenceRoomBookingApi.Models;
namespace ConferenceRoomBookingApi.Services;

public class RoomService
{
    // Minimum allowed price per hour
    private const decimal MinPrice = 0m;
    // Minimum allowed room capacity
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
    // Creates a new room and adds it to the room list
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
    // Creates a new service and adds it to the specified room.
    public Service CreateServiceForRoom(
        int roomId,
        string name,
        decimal price)
    {
        if (price <= MinPrice)
        {
            throw new ArgumentException("Service price must be greater than zero");
        }
        // finds the room by id
        var room = _rooms.FirstOrDefault(r => r.Id == roomId);
        if (room is null)
        {
            throw new ArgumentException("Room was not found.");
        }
        // creates a new service
        var service = new Service(name, price)
        {
            Id = NextServiceId()
        };
        // updates list
        room.Services.Add(service);
        return service;
    }
    // Updates the price of an existing service for the specified room.
    public Service UpdateServiceForRoom(
        int roomId,
        int serviceId,
        decimal newPrice)
    {
        //checks the price argument
        if (newPrice <= MinPrice)
        {
            throw new ArgumentException("Service price must be greater than zero");
        }
        // finds the room by id
        var room = _rooms.FirstOrDefault(r => r.Id == roomId);
        if (room is null)
        {
            throw new ArgumentException("Room was not found.");
        }
        // finds the service by id
        var service = room.Services.FirstOrDefault(s => s.Id == serviceId);
        if (service is null)
        {
            throw new ArgumentException("Service was not found for this room.");
        }
        // updates the service price
        service.UpdatePrice(newPrice);
        return service;
    }
    // optionally updates the room price or adds a new service.
    public Room UpdateRoom(
        int roomId,
        decimal? newPrice,
        Service? newService)
    {
        // checks that at least one update argument was provided.
        if (newPrice == null && newService == null)
        {
            throw new ArgumentException("either newPrice or newService must not equal null");
        }
        // finds the room by id
        var room = _rooms.FirstOrDefault(r => r.Id == roomId);
        if (room == null)
        {
            throw new ArgumentException("Room was not found.");
        }
        // updates the room price if a new price was provided.
        if (newPrice is decimal price)
        {
            if (price <= MinPrice)
            {
                throw new ArgumentException(
                    "Price per hour must be greater than zero.");
            }
            room.UpdatePrice(price);
        }
        // adds the new service if one was provided.
        if (newService is not null) room.Services.Add(newService);
        return room;
    }
    // Deletes a room by its ID.
    public void DeleteRoom(int roomId)
    {
        var room = _rooms.FirstOrDefault(r => r.Id == roomId);
        if (room is null)
        {
            throw new ArgumentException("Room was not found.");
        }
        _rooms.Remove(room);
    }
}