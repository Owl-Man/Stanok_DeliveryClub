using Microsoft.Extensions.Logging;
using Stanok.Core.Abstractions;
using Stanok.Core.Models;
using Stanok.DataAccess.Entities;

namespace Stanok.DataAccess.Repositories;

public class StanoksRepository(StanokDbContext context, ILogger<StanoksRepository> logger) : IStanoksRepository
{
    public Guid Create(string name, string manufacturer, double price)
    {
        try
        {
            var stanok = new StanokEntity() { Name = name, Manufacturer = manufacturer, Price = price };

            context.Stanoks.Add(stanok);
            context.SaveChanges();

            return stanok.Id;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при создании станок {StanokName}.", name);
            throw;
        }
    }
}
