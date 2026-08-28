namespace ConferenceRoomBookingApi.DTOs;

public class CreateBookingRequest
{
    public int RoomId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public List<int> ServiceIds { get; set; } = new();
}