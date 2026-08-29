namespace ConferenceRoomBookingApi.DTOs;
// Represents booking and revenue statistics for a room.
public class RoomRevenueReportDto
{
    public int RoomId { get; set; }
    public string RoomName { get; set; } = string.Empty;
    public int BookingCount { get; set; }
    public decimal TotalRevenue { get; set; }
}