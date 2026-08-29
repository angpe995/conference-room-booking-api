using Microsoft.AspNetCore.Mvc;
using ConferenceRoomBookingApi.Services;
using ConferenceRoomBookingApi.Models;
using ConferenceRoomBookingApi.DTOs;
namespace ConferenceRoomBookingApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RoomsController : ControllerBase
{
    private readonly RoomService _roomService;
    public RoomsController(
        RoomService roomService)
    {
        _roomService = roomService;
    }
    // Creates a new room with its available services.
    [HttpPost]
    public async Task<ActionResult<Room>> CreateRoom(
    [FromBody] CreateRoomRequest request)
    {
        var room = await _roomService.CreateRoomAsync(
            request.Name,
            request.PricePerHour,
            request.Capacity,
            request.Services);
        return Ok(room);
    }
    // Deletes a room by its ID.
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRoom(int id)
    {
        await _roomService.DeleteRoomAsync(id);
        return NoContent();
    }
     // Deletes a service by its ID.
    [HttpDelete("services/{id}")]
    public async Task<IActionResult> DeleteService(int id)
    {
        await _roomService.DeleteServiceAsync(id);
        return NoContent();
    }
    // Updates the room price and optionally adds a new service.
    [HttpPut("{id}")]
    public async Task<ActionResult<Room>> UpdateRoom(
    int id,
    [FromBody] UpdateRoomRequest request)
    {
        var room =await _roomService.UpdateRoomAsync(
            id,
            request.PricePerHour,
            request.ServiceToAdd);

        return Ok(room);
    }
}
