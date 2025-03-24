using Stanok.Core.Abstractions;

namespace Stanok.Application.Services;

public class StanokService(IStanoksRepository stanoksRepository) : IStanokService
{
    public Guid Create(string name, string manufacturer, double price)
    {
        return stanoksRepository.Create(name, manufacturer, price);
    }
}
