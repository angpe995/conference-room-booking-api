namespace ConferenceRoomBookingApi.Models;

public class Service
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; private set; }
    public Service(string name, decimal price)
    {
        Name = name;
        Price = price;
    }
    public void UpdatePrice(decimal newPrice)
    {
        if (newPrice <= 0)
        {
            return;
        }
        Price = newPrice;
    }
}
