namespace ConferenceRoomBookingApi.Models;

public class Room
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal PricePerHour { get; private set; }
    public int Capacity { get; set; }
    public List<Service> Services { get; set; } = new();
    public void UpdatePrice(decimal newPrice)
    {
        if (newPrice <= 0)
        {
            return;
        }

        PricePerHour = newPrice;
    }
}