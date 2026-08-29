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
    public async Task<Room?> GetRoomByIdAsync(int roomId)
    {
        return await _context.Rooms
            .Include(r => r.Services)
            .FirstOrDefaultAsync(r => r.Id == roomId);
    }
    public async Task<Service?> GetServiceByIdAsync(int serviceId)
    {
        return await _context.Services
            .FirstOrDefaultAsync(s => s.Id == serviceId);
    }
    // Creates a new room and adds it to the room list
    public async Task<Room> CreateRoomAsync(
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
        await _context.SaveChangesAsync();
        if (services is not null)
        {
            foreach (var serviceRequest in services)
            {
                await CreateServiceForRoomAsync(
                    room.Id,
                    serviceRequest.Name,
                    serviceRequest.Price);
            }
        }
        return room;
    }
    // Creates a new service and adds it to the specified room.
    public async Task<Service> CreateServiceForRoomAsync(
        int roomId,
        string name,
        decimal price)
    {
        if (price <= MinPrice)
        {
            throw new ArgumentException("Service price must be greater than zero");
        }
        // finds the room by id
        var room = await GetRoomByIdAsync(roomId);
        if (room is null)
        {
            throw new NotFoundException("Room was not found.");
        }
        // creates a new service
        var service = new Service(name, price);
        // updates list
        room.Services.Add(service);
        await _context.SaveChangesAsync();
        return service;
    }
    // Updates the price of an existing service for the specified room.
    // optionally updates the room price or adds a new service.
    public async Task<Room> UpdateRoomAsync(
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
        var room = await GetRoomByIdAsync(roomId);
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
            await _context.SaveChangesAsync();
        }
        // adds the new service if one was provided.
        if (serviceToAdd is not null)
        {
            await CreateServiceForRoomAsync(
            roomId,
            serviceToAdd.Name,
            serviceToAdd.Price);
        }
        await _context.SaveChangesAsync();
        return room;
    }
    // Deletes a room by its ID.
    public async Task DeleteRoomAsync(int roomId)
    {
        var room = await GetRoomByIdAsync(roomId);
        if (room is null)
        {
            throw new NotFoundException("Room was not found.");
        }
        _context.Rooms.Remove(room);
        await _context.SaveChangesAsync();
    }
    // Deletes a room by its ID.
    public async Task DeleteServiceAsync(int serviceId)
    {
        var service = await GetServiceByIdAsync(serviceId);
        if (service is null)
        {
            throw new NotFoundException("Service was not found.");
        }
        _context.Services.Remove(service);
        await _context.SaveChangesAsync();
    }
}