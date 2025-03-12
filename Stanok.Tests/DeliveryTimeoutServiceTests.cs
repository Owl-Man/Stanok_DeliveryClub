using Microsoft.EntityFrameworkCore;
using Stanok_DeliveryClub.Contracts;
using System.Net.Http.Json;
using System.Text.Json;

namespace Stanok.Tests;

public class DeliveryTimeoutServiceTests : BaseIntegrationTest
{
    public DeliveryTimeoutServiceTests(TestWebAppFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task CreateStanokWithTimers_ChangesStatusAfterTimeout()
    {
        // Arrange

        var stanoks = new List<(Guid StanokId, string Name, Guid DeliveryId)>();
        var random = new Random();

        //Act

        for (int i = 0; i < 100; i++)
        {
            var requestData = new { name = $"Stanok_{i}", manufacturer = "Test", price = i };
            var response = await _client.PostAsync("/Stanoks/stanok.create", JsonContent.Create(requestData));
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var createdStanok = JsonSerializer.Deserialize<StanokResponse>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            var deliveryFromDb = await dbContext.Deliveries
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.StanokId == createdStanok.id);
            Assert.NotNull(deliveryFromDb);
            Assert.Equal("CREATE", deliveryFromDb.Status.ToString());

            stanoks.Add((createdStanok.id, createdStanok.name, deliveryFromDb.Id));

            await Task.Delay(TimeSpan.FromSeconds(new Random().Next(10, 21)));
        }

        await Task.Delay(TimeSpan.FromSeconds(22));

        // Assert
        foreach (var (stanokId, name, deliveryId) in stanoks)
        {
            var deliveryFromDb = dbContext.Deliveries
                .AsNoTracking()
                .FirstOrDefault(d => d.Id == deliveryId);
            Assert.Equal(stanokId, deliveryFromDb.StanokId);
            Assert.Equal("CANCELLED", deliveryFromDb.Status.ToString());
        }

        // Drop db
        await dbContext.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"Deliveries\" RESTART IDENTITY CASCADE;");
        await dbContext.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"Stanoks\" RESTART IDENTITY;");
    }
}