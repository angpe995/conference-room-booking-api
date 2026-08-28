namespace ConferenceRoomBookingApi.DTOs;

public class SearchAvailableRoomsRequest{
    public DateTime StartTime{get;set;}
    public DateTime EndTime{get;set;}
     public int Capacity { get; set; }
}