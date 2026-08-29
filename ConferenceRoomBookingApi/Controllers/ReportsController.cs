using Microsoft.AspNetCore.Mvc;
using ConferenceRoomBookingApi.Data;
using ConferenceRoomBookingApi.DTOs;
using ConferenceRoomBookingApi.Services;
namespace ConferenceRoomBookingApi.Controllers;

// Provides endpoints for retrieving booking and revenue reports.
[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly ReportsService _reportsService;
    public ReportsController(ReportsService reportsService)
    {
        _reportsService = reportsService;
    }
    // Returns revenue and booking statistics for each room.
    [HttpGet("room-revenue")]
    public async Task<ActionResult<List<RoomRevenueReportDto>>> GetRoomRevenueReport(DateTime? startDate,
    DateTime? endDate)
    {
        return await _reportsService.GetRoomRevenueReport(startDate,endDate);
    }
     // Returns the number of bookings for each operating hour.
    [HttpGet("hourly-bookings")]
    public async Task<List<HourlyBookingReportDto>> GetHourlyBookingReport()
    {
        return await _reportsService.GetHourlyBookingReport();
    }

}