using ConferenceRoomBookingApi.Models;
namespace ConferenceRoomBookingApi.Data;
// In-memory storage for application data.
public class DataStore
{
    public List<Room> Rooms { get; } = new();
    public List<Booking> Bookings { get; } = new();
    private int _nextServiceId = 1;
    private int _nextRoomId = 1;
    public int GetNextServiceId()
    {
        return _nextServiceId++;
    }
    public int GetNextRoomId()
    {
        return _nextRoomId++;
    }
}
