namespace Stanok.Core.Abstractions
{
    public interface IStanoksRepository
    {
        Guid Create(Guid id, string name, string manufacturer, double price);
    }
}