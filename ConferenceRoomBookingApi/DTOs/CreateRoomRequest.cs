namespace ConferenceRoomBookingApi.DTOs;

public class CreateRoomRequest
{
    public string Name { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public decimal PricePerHour { get; set; }
    public List<ServiceRequest> Services { get; set; } = new();
}