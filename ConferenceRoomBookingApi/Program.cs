using ConferenceRoomBookingApi.Data;
using ConferenceRoomBookingApi.Models;
using ConferenceRoomBookingApi.DTOs;
using ConferenceRoomBookingApi.Services;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<DataStore>();
builder.Services.AddSingleton<RoomService>();
builder.Services.AddSingleton<BookingService>();
builder.Services.AddSingleton<RoomSearchService>();
var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();
app.MapControllers();
var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild",
    "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};
app.Run();