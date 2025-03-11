//using Xunit;
//using Microsoft.AspNetCore.Mvc.Testing;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.VisualStudio.TestPlatform.TestHost;
//using Stanok_DeliveryClub.Contracts;
//using System.Net.Http.Json;
//using System.Text.Json;
//using Testcontainers.PostgreSql;
//using Stanok.DataAccess;

//namespace Stanok.Tests;

//public class DeliveryTimeoutServiceTests : IAsyncLifetime
//{
//    private readonly WebApplicationFactory<Program> _factory;
//    private readonly HttpClient _client;
//    private readonly PostgreSqlContainer _postgresContainer;

//    public DeliveryTimeoutServiceTests()
//    {
//        _factory = new WebApplicationFactory<Program>();
//        _client = _factory.CreateClient();

//        _postgresContainer = new PostgreSqlBuilder()
//            .WithImage("postgres:latest")
//            .WithDatabase("testdb")
//            .WithUsername("testuser")
//            .WithPassword("testpass")
//            .Build();
//    }

//    public async Task InitializeAsync()
//    {
//        await _postgresContainer.StartAsync();
//        var dbOptions = new DbContextOptionsBuilder<StanokDbContext>()
//            .UseNpgsql(_postgresContainer.GetConnectionString())
//            .Options;

//        using var dbContext = new StanokDbContext(dbOptions);
//        await dbContext.Database.MigrateAsync();
//    }

//    public async Task DisposeAsync()
//    {
//        await _postgresContainer.StopAsync();
//        await _postgresContainer.DisposeAsync();
//    }

//    [Fact]
//    public async Task CreateStanokWithTimers_ChangesStatusAfterTimeout()
//    {
//        // Arrange
//        var dbOptions = new DbContextOptionsBuilder<StanokDbContext>()
//            .UseNpgsql(_postgresContainer.GetConnectionString())
//            .Options;

//        using var dbContext = new StanokDbContext(dbOptions);
//        var stanoks = new List<(Guid StanokId, string Name, Guid DeliveryId)>();
//        var random = new Random();

//        for (int i = 0; i < 5; i++)
//        {
//            var requestData = new { Name = $"Stanok_{i}", Description = "Test" };
//            var response = await _client.PostAsync("/stanok.create", JsonContent.Create(requestData));
//            response.EnsureSuccessStatusCode();

//            var responseContent = await response.Content.ReadAsStringAsync();
//            var createdStanok = JsonSerializer.Deserialize<StanokResponse>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

//            var deliveryFromDb = await dbContext.Deliveries
//                .FirstOrDefaultAsync(d => d.StanokId == createdStanok.id);
//            Assert.NotNull(deliveryFromDb);
//            Assert.Equal("CREATE", deliveryFromDb.Status.ToString());

//            stanoks.Add((createdStanok.id, createdStanok.name, deliveryFromDb.Id));
//        }

//        // Act: Ждем 10 секунд
//        await Task.Delay(TimeSpan.FromSeconds(10));

//        // Assert
//        foreach (var (stanokId, name, deliveryId) in stanoks)
//        {
//            var deliveryFromDb = await dbContext.Deliveries.FindAsync(deliveryId);
//            Assert.Equal("CANCELLED", deliveryFromDb.Status.ToString());
//            Assert.Equal(stanokId, deliveryFromDb.StanokId);
//        }

//        // After
//        await dbContext.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"Deliveries\" RESTART IDENTITY CASCADE;");
//        await dbContext.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"Stanoks\" RESTART IDENTITY;");
//    }
//}