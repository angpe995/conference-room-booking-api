using Microsoft.AspNetCore.Mvc;
using ConferenceRoomBookingApi.Services;
using ConferenceRoomBookingApi.Models;
using ConferenceRoomBookingApi.DTOs;
namespace ConferenceRoomBookingApi.Controllers;

[ApiController]
[Route("api/rooms")]
public class RoomSearchController : ControllerBase
{
    private readonly RoomSearchService _roomSearchService;
    public RoomSearchController(RoomSearchService roomSearchService)
    {
        _roomSearchService = roomSearchService;
    }
    // Searches for available rooms by time and required capacity.
    [HttpGet("available")]
    public ActionResult<List<Room>> SearchAvailableRooms(
        [FromQuery] SearchAvailableRoomsRequest request)
    {
        // Get rooms matching the requested criteria.
        var rooms = _roomSearchService.SearchAvailableRooms(
            request.StartTime,
            request.EndTime,
            request.Capacity);
        // Returns an error if no available rooms are found.
        if (!rooms.Any())
        {
            return NotFound("No available rooms found for the specified criteria.");
        }
        return Ok(rooms);
    }
}