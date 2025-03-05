using Stanok.Core.Abstractions;

namespace Stanok.Application.Services;

public class StanokService : IStanokService
{
    private readonly IStanoksRepository _stanoksRepository;

    public StanokService(IStanoksRepository stanoksRepository)
    {
        _stanoksRepository = stanoksRepository;
    }

    public Guid Create(Guid id, string name, string manufacturer, double price)
    {
        return _stanoksRepository.Create(id, name, manufacturer, price);
    }
}
