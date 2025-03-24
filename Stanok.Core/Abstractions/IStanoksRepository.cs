namespace Stanok.Core.Abstractions
{
    public interface IStanoksRepository
    {
        Guid Create(string name, string manufacturer, double price);
    }
}