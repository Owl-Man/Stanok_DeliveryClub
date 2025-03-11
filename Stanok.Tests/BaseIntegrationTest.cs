using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Stanok.DataAccess;

namespace Stanok.Tests;

public abstract class BaseIntegrationTest : IClassFixture<TestWebAppFactory>
{
    private readonly IServiceScope _scope;
    protected readonly StanokDbContext dbContext;
    protected readonly HttpClient _client;

    public BaseIntegrationTest(TestWebAppFactory factory)
    {
        _scope = factory.Services.CreateScope();

        dbContext = _scope.ServiceProvider.GetRequiredService<StanokDbContext>();

        dbContext.Database.Migrate();

        _client = factory.CreateClient();
    }
}
