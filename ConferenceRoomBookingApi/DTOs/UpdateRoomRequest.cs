namespace ConferenceRoomBookingApi.DTOs;

public class UpdateRoomRequest
{
    public decimal? PricePerHour { get; set; }
    public ServiceRequest? ServiceToAdd { get; set; }
}