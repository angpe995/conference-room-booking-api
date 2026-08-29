namespace ConferenceRoomBookingApi.DTOs;
// Represents booking statistics for a specific hour
public class HourlyBookingReportDto
{
    public int Hour { get; set; }
    public int BookingCount { get; set; }
}