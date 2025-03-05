namespace Stanok.Core.Abstractions
{
    public interface IStanokService
    {
        Guid Create(Guid id, string name, string manufacturer, double price);
    }
}