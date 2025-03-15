using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Stanok.DataAccess;

namespace Stanok.Tests;

public abstract class BaseIntegrationTest : IClassFixture<TestWebAppFactory>
{
    protected IServiceScope _scope;
    protected StanokDbContext dbContext;
    protected HttpClient _client;

    public BaseIntegrationTest(TestWebAppFactory factory)
    {
        _scope = factory.Services.CreateScope();

        dbContext = _scope.ServiceProvider.GetRequiredService<StanokDbContext>();

        dbContext.Database.Migrate();

        _client = factory.CreateClient();
    }
}
