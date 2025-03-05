namespace Stanok.Core.Models;

public class Stanok
{
    public Guid Id { get; }
    public string Name { get; }
    public string Manufacturer { get; }
    public double Price { get; }

    public Stanok(Guid id, string name, string manufacturer, double price)
    {
        Id = id;
        Name = name;
        Manufacturer = manufacturer;
        Price = price;
    }
}
