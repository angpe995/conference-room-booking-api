using ConferenceRoomBookingApi.Models;
using ConferenceRoomBookingApi.Data;
using ConferenceRoomBookingApi.DTOs;
using ConferenceRoomBookingApi.Exceptions;
using Microsoft.EntityFrameworkCore;
namespace ConferenceRoomBookingApi.Services;

public class RoomService
{
    // Minimum allowed price per hour
    private const decimal MinPrice = 0m;
    // Minimum allowed room capacity
    private const int MinCapacity = 0;
    private readonly AppDbContext _context;
    public RoomService(AppDbContext context)
    {
        _context = context;
    }
    //searchs room by id
    public Room? GetRoomById(int roomId)
    {
        return _context.Rooms
            .Include(r => r.Services)
            .FirstOrDefault(r => r.Id == roomId);
    }
    // Creates a new room and adds it to the room list
    public Room CreateRoom(
        string name,
        decimal pricePerHour,
        int capacity, List<ServiceRequest>? services)
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
            Name = name,
            Capacity = capacity,
        };
        room.UpdatePrice(pricePerHour);
        _context.Rooms.Add(room);
        _context.SaveChanges();
        if (services is not null)
        {
            foreach (var serviceRequest in services)
            {
                CreateServiceForRoom(
                    room.Id,
                    serviceRequest.Name,
                    serviceRequest.Price);
            }
        }
        _context.SaveChanges();
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
        var room = GetRoomById(roomId);
        if (room is null)
        {
            throw new NotFoundException("Room was not found.");
        }
        // creates a new service
        var service = new Service(name, price);
        // updates list
        room.Services.Add(service);
        _context.SaveChanges();
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
        var room = GetRoomById(roomId);
        if (room is null)
        {
            throw new NotFoundException("Room was not found.");
        }
        // finds the service by id
        var service = room.Services.FirstOrDefault(s => s.Id == serviceId);
        if (service is null)
        {
            throw new NotFoundException("Service was not found for this room.");
        }
        // updates the service price
        service.UpdatePrice(newPrice);
        _context.SaveChanges();
        return service;
    }
    // optionally updates the room price or adds a new service.
    public Room UpdateRoom(
        int roomId,
        decimal? newPrice,
        ServiceRequest? serviceToAdd)
    {
        // checks that at least one update argument was provided.
        if (newPrice is null && serviceToAdd is null)
        {
            throw new ArgumentException(
                "Either newPrice or serviceToAdd must be provided.");
        }
        // finds the room by id
        var room = GetRoomById(roomId);
        if (room == null)
        {
            throw new NotFoundException("Room was not found.");
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
            _context.SaveChanges();
        }
        // adds the new service if one was provided.
        if (serviceToAdd is not null)
        {
            CreateServiceForRoom(
            roomId,
            serviceToAdd.Name,
            serviceToAdd.Price);
        }
        _context.SaveChanges();
        return room;
    }
    // Deletes a room by its ID.
    public void DeleteRoom(int roomId)
    {
        var room = GetRoomById(roomId);
        if (room is null)
        {
            throw new NotFoundException("Room was not found.");
        }
        _context.Rooms.Remove(room);
        _context.SaveChanges();
    }
}