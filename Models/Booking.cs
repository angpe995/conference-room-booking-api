namespace ConferenceRoomBookingApi.Models;

public class Booking
{
    public int Id { get; set; }
    public int RoomId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public decimal TotalPrice {get;set;}
}