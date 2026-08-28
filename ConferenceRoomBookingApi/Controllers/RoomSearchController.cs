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
        List<Room> rooms = _roomSearchService.SearchAvailableRooms(
            request.StartTime,
            request.EndTime,
            request.Capacity);

        return Ok(rooms);
    }
}