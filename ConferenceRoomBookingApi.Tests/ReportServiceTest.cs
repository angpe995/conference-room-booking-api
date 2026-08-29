using ConferenceRoomBookingApi.Models;
using ConferenceRoomBookingApi.Services;
using ConferenceRoomBookingApi.Data;
using ConferenceRoomBookingApi.DTOs;
using Microsoft.EntityFrameworkCore;

namespace ConferenceRoomBookingApi.Tests;

public class ReportsServiceTests
{
    private readonly ReportsService _reportsService;
    private readonly AppDbContext _context;
    private const string RoomName = "A";
    private const int RoomCapacity = 50;
    private const decimal RoomPricePerHour = 2000M;
    private const decimal FirstBookingPrice = 4000m;
    private const decimal SecondBookingPrice = 6000m;
    private async Task<Room> CreateRoom(
        string name = RoomName,
        int capacity = RoomCapacity,
        decimal pricePerHour = RoomPricePerHour)
    {
        Room room = new Room
        {
            Name = name,
            Capacity = capacity,
        };
        room.UpdatePrice(pricePerHour);
        _context.Rooms.Add(room);
        await _context.SaveChangesAsync();
        return room;
    }
    public ReportsServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _reportsService = new ReportsService(_context);
    }

    [Fact]
    public async Task GetHourlyBookingReport_ReturnsCorrectBookingCountForEachHour()
    {
        // Arrange
        Room room = await CreateRoom(RoomName,RoomCapacity,RoomPricePerHour);
        await _context.SaveChangesAsync();
        _context.Bookings.AddRange(
            new Booking
            {
                RoomId = room.Id,
                StartTime = new DateTime(2026, 9, 1, 10, 0, 0),
                EndTime = new DateTime(2026, 9, 1, 12, 0, 0),
                TotalPrice = FirstBookingPrice
            },
            new Booking
            {
                RoomId = room.Id,
                StartTime = new DateTime(2026, 9, 1, 11, 0, 0),
                EndTime = new DateTime(2026, 9, 1, 13, 0, 0),
                TotalPrice = SecondBookingPrice
            });

        await _context.SaveChangesAsync();

        // Act
        List<HourlyBookingReportDto> result =
            await _reportsService.GetHourlyBookingReport();
        // Assert
        Assert.Equal(17, result.Count);
        Assert.Equal(0, result.First(r => r.Hour == 9).BookingCount);
        Assert.Equal(1, result.First(r => r.Hour == 10).BookingCount);
        Assert.Equal(2, result.First(r => r.Hour == 11).BookingCount);
        Assert.Equal(1, result.First(r => r.Hour == 12).BookingCount);
        Assert.Equal(0, result.First(r => r.Hour == 13).BookingCount);
    }

    [Fact]
    public async Task GetHourlyBookingReport_ReturnsZero_WhenThereAreNoBookings()
    {
        // Arrange
        Room room = await CreateRoom(RoomName,RoomCapacity,RoomPricePerHour);
        // Act
        List<HourlyBookingReportDto> result =
            await _reportsService.GetHourlyBookingReport();
        // Assert
        Assert.All(
            result,
            report => Assert.Equal(0, report.BookingCount));
    }

    [Fact]
    public async Task GetRoomRevenueReport_ReturnsCorrectBookingCountAndRevenue()
    {
        // Arrange
        Room room = await CreateRoom(RoomName,RoomCapacity,RoomPricePerHour);

        _context.Bookings.AddRange(
            new Booking
            {
                RoomId = room.Id,
                StartTime = new DateTime(2026, 9, 1, 10, 0, 0),
                EndTime = new DateTime(2026, 9, 1, 12, 0, 0),
                TotalPrice = FirstBookingPrice
            },
            new Booking
            {
                RoomId = room.Id,
                StartTime = new DateTime(2026, 9, 2, 10, 0, 0),
                EndTime = new DateTime(2026, 9, 2, 12, 0, 0),
                TotalPrice = SecondBookingPrice
            });
        await _context.SaveChangesAsync();
        // Act
        List<RoomRevenueReportDto> result =
            await _reportsService.GetRoomRevenueReport();
        // Assert
        RoomRevenueReportDto roomReport = Assert.Single(result);

        Assert.Equal(room.Id, roomReport.RoomId);
        Assert.Equal(RoomName, roomReport.RoomName);
        Assert.Equal(2, roomReport.BookingCount);
        Assert.Equal(10000m, roomReport.TotalRevenue);
    }

    [Fact]
    public async Task GetRoomRevenueReport_ReturnsZero_WhenRoomHasNoBookings()
    {
        // Arrange
        Room room = await CreateRoom(RoomName,RoomCapacity,RoomPricePerHour);
        // Act
        List<RoomRevenueReportDto> result =
            await _reportsService.GetRoomRevenueReport();

        // Assert
        RoomRevenueReportDto roomReport = Assert.Single(result);
        Assert.Equal(room.Id, roomReport.RoomId);
        Assert.Equal(RoomName, roomReport.RoomName);
        Assert.Equal(0, roomReport.BookingCount);
        Assert.Equal(0m, roomReport.TotalRevenue);
    }

    [Fact]
    public async Task GetRoomRevenueReport_ReturnsOnlyBookingsWithinDateRange()
    {
        // Arrange
        Room room = await CreateRoom(RoomName,RoomCapacity,RoomPricePerHour);
        _context.Bookings.AddRange(
            new Booking
            {
                RoomId = room.Id,
                StartTime = new DateTime(2026, 9, 5, 10, 0, 0),
                EndTime = new DateTime(2026, 9, 5, 12, 0, 0),
                TotalPrice = FirstBookingPrice
            },
            new Booking
            {
                RoomId = room.Id,
                StartTime = new DateTime(2026, 9, 15, 10, 0, 0),
                EndTime = new DateTime(2026, 9, 15, 12, 0, 0),
                TotalPrice = SecondBookingPrice
            });
        await _context.SaveChangesAsync();
        // Act
        List<RoomRevenueReportDto> result =
            await _reportsService.GetRoomRevenueReport(
                new DateTime(2026, 9, 10),
                new DateTime(2026, 9, 20));
        // Assert
        RoomRevenueReportDto roomReport = Assert.Single(result);

        Assert.Equal(1, roomReport.BookingCount);
        Assert.Equal(SecondBookingPrice, roomReport.TotalRevenue);
    }

    [Fact]
    public async Task GetRoomRevenueReport_ReturnsAllBookings_WhenDatesAreNotProvided()
    {
        // Arrange
        Room room = await CreateRoom(RoomName,RoomCapacity,RoomPricePerHour);
        _context.Bookings.AddRange(
            new Booking
            {
                RoomId = room.Id,
                StartTime = new DateTime(2026, 8, 1, 10, 0, 0),
                EndTime = new DateTime(2026, 8, 1, 12, 0, 0),
                TotalPrice = FirstBookingPrice
            },
            new Booking
            {
                RoomId = room.Id,
                StartTime = new DateTime(2026, 9, 1, 10, 0, 0),
                EndTime = new DateTime(2026, 9, 1, 12, 0, 0),
                TotalPrice = SecondBookingPrice
            });
        await _context.SaveChangesAsync();
        // Act
        List<RoomRevenueReportDto> result =
            await _reportsService.GetRoomRevenueReport();
        // Assert
        RoomRevenueReportDto roomReport = Assert.Single(result);
        Assert.Equal(2, roomReport.BookingCount);
        Assert.Equal(10000m, roomReport.TotalRevenue);
    }
    [Fact]
    public async Task GetRoomRevenueReport_Throws_WhenEndDateIsBeforeStartDate()
    {
        // Arrange
        DateTime startDate = new DateTime(2026, 9, 20);
        DateTime endDate = new DateTime(2026, 9, 10);
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _reportsService.GetRoomRevenueReport(
                startDate,
                endDate));
    }
    [Fact]
    public async Task GetRoomRevenueReport_Throws_WhenDatesAreEqual()
    {
        // Arrange
        DateTime startDate = new DateTime(2026, 9, 10);
        DateTime endDate = new DateTime(2026, 9, 10);
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _reportsService.GetRoomRevenueReport(
                startDate,
                endDate));
    }
}