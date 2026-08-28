using ConferenceRoomBookingApi.Models;
namespace ConferenceRoomBookingApi.Data;
// In-memory storage for application data.
public class DataStore
{
    public List<Room>Rooms { get; } = new();
    public List<Booking> Bookings { get; } = new();
}
