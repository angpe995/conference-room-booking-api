using ConferenceRoomBookingApi.Models;
using ConferenceRoomBookingApi.Data;
namespace ConferenceRoomBookingApi.Services;
public class RoomSearchService
{
    private readonly BookingService _bookingService;
    private readonly DataStore _dataStore;
    public RoomSearchService(
        BookingService bookingService,
        DataStore dataStore)
    {
        _bookingService = bookingService;
        _dataStore = dataStore;
    }
    public List<Room> SearchAvailableRooms(
        DateTime startTime,
        DateTime endTime,
        int capacity)
    {
        return _dataStore.Rooms
            .Where(room =>
                room.Capacity >= capacity &&
                _bookingService.CheckAvailability(
                    room.Id,
                    startTime,
                    endTime))
            .ToList();
    }
}