using Microsoft.AspNetCore.Mvc;
using ConferenceRoomBookingApi.Services;
using ConferenceRoomBookingApi.Models;
using ConferenceRoomBookingApi.DTOs;
namespace ConferenceRoomBookingApi.Controllers;

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
    public ActionResult<Room> CreateRoom(
       [FromBody] CreateRoomRequest request)
    {
        var services = request.Services
        .Select(serviceRequest => new Service(
            serviceRequest.Name,
            serviceRequest.Price))
        .ToList();
        var room = _roomService.CreateRoom(
        request.Name,
        request.PricePerHour,
        request.Capacity,
        services);
        return Ok(room);
    }
    // Deletes a room by its ID.
    [HttpDelete("{id}")]
    public IActionResult DeleteRoom(int id)
    {
        _roomService.DeleteRoom(id);
        return NoContent();
    }
    // Updates the room price and optionally adds a new service.
    [HttpPut("{id}")]
    public ActionResult<Room> UpdateRoom(
    int id,
    [FromBody] UpdateRoomRequest request)
    {
        Service? newService = null;
        if (request.ServiceToAdd is not null)
        {
            newService = new Service(
                request.ServiceToAdd.Name,
                request.ServiceToAdd.Price);
        }
        var room = _roomService.UpdateRoom(
            id,
            request.PricePerHour,
            newService);
        return Ok(room);
    }
}
