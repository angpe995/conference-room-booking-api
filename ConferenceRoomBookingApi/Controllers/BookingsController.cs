using Microsoft.AspNetCore.Mvc;
using ConferenceRoomBookingApi.Services;
using ConferenceRoomBookingApi.Models;
using ConferenceRoomBookingApi.DTOs;
namespace ConferenceRoomBookingApi.Controllers;

[ApiController]
// Creates a new booking for the requested room.
[Route("api/[controller]")]
public class BookingController : ControllerBase
{
    private readonly BookingService _bookingService;
    private readonly RoomService _roomService;

    public BookingController(BookingService bookingService, RoomService roomService)
    {
        _bookingService = bookingService;
        _roomService = roomService;
    }
    [HttpPost]
    public ActionResult<Booking> CreateBooking([FromBody] CreateBookingRequest request)
    {
        // Find the requested room by ID
        Room? room = _roomService.GetRoomById(request.RoomId);
        if (room is null)
        {
            return NotFound(
                $"Room with ID {request.RoomId} was not found.");
        }
        // Get the services selected for this room
        List<Service> services = room.Services
            .Where(s => request.ServiceIds.Contains(s.Id))
            .ToList();
        // Calculate the total booking price
        decimal totalPrice = _bookingService.CalculatePrice(
            services,
            room,
            request.StartTime,
            request.EndTime);
        // Create and store the booking
        Booking booking = _bookingService.CreateBooking(
            room,
            request.StartTime,
            request.EndTime,
            totalPrice);
        return Ok(booking);
    }
}