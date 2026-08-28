using Microsoft.AspNetCore.Mvc;
using ConferenceRoomBookingApi.Services;
using ConferenceRoomBookingApi.Models;
using ConferenceRoomBookingApi.DTOs;
using ConferenceRoomBookingApi.Exceptions;
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
            throw new NotFoundException(
                $"Room with ID {request.RoomId} was not found.");
        }
        // Create and store the booking
        Booking booking = _bookingService.CreateBooking(
            room,
            request.StartTime,
            request.EndTime,request.ServiceIds);
        return Ok(booking);
    }
}