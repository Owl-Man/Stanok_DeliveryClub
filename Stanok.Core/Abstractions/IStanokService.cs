
namespace Stanok.Core.Abstractions
{
    public interface IStanokService
    {
        Guid Create(string name, string manufacturer, double price);
    }
}