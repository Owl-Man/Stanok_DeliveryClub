using Stanok.Core.Abstractions;

namespace Stanok.Application.Services;

public class StanokService(IStanoksRepository stanoksRepository) : IStanokService
{
    public Guid Create(Guid id, string name, string manufacturer, double price)
    {
        return stanoksRepository.Create(id, name, manufacturer, price);
    }
}
