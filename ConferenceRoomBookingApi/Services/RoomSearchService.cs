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
         // Verify that the booking starts and ends exactly at the top of the hour.
        if (startTime.Minute != 0 || endTime.Minute != 0)
        {
            throw new ArgumentException(
                "Bookings must start and end exactly at the top of the hour (e.g., 10:00, 11:00).");
        }
        if (startTime.Date != endTime.Date)
        {
            throw new ArgumentException(
                "Booking must start and end on the same day.");
        }
        if (endTime <= startTime)
        {
            throw new ArgumentException(
                "End time must be later than start time.");
        }
        if (capacity <= 0)
        {
            throw new ArgumentException(
                "Capacity must be greater than zero.");
        }
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