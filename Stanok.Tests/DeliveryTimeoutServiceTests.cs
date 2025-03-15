using Microsoft.EntityFrameworkCore;
using Stanok.Core.Models;
using Stanok.DataAccess.Entities;
using Stanok_DeliveryClub.Contracts;
using System.Net.Http.Json;
using System.Text.Json;

namespace Stanok.Tests;

public class DeliveryTimeoutServiceTests : BaseIntegrationTest
{
    public DeliveryTimeoutServiceTests(TestWebAppFactory factory) : base(factory) { }

    [Fact]
    public async Task CreateStanokWithTimers_ChangesStatusAfterTimeout()
    {
        // Arrange

        var stanoks = new List<(Guid StanokId, string Name, Guid DeliveryId)>();

        //Act

        for (int i = 0; i < 10; i++)
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

            //await Task.Delay(TimeSpan.FromSeconds(new Random().Next(5, 11)));
        }

        await Task.Delay(TimeSpan.FromSeconds(15));

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

    [Fact]
    public async Task CreateStanokWithTimers_ChangesStatusAfterTimeoutWithReload()
    {
        var stanokResponses = new List<StanokResponse>();
        for (int i = 0; i < 3; i++)
        {
            var requestData = new { name = $"Stanok_{i}", manufacturer = "Test", price = i };
            var response = await _client.PostAsync("/Stanoks/stanok.create", JsonContent.Create(requestData));
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var createdStanok = JsonSerializer.Deserialize<StanokResponse>(responseContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            stanokResponses.Add(createdStanok);
        }


        var delivery1 = dbContext.Deliveries.FirstOrDefault(d => d.StanokId == stanokResponses[0].id);
        var delivery2 = dbContext.Deliveries.FirstOrDefault(d => d.StanokId == stanokResponses[1].id);
        var delivery3 = dbContext.Deliveries.FirstOrDefault(d => d.StanokId == stanokResponses[2].id);

        Assert.NotNull(delivery1);
        Assert.NotNull(delivery2);
        Assert.NotNull(delivery3);

        delivery1.CreatedAt = DateTime.UtcNow - TimeSpan.FromSeconds(20); // Просрочена на 20 секунд
        delivery2.CreatedAt = DateTime.UtcNow - TimeSpan.FromSeconds(10); // Только что просрочена
        delivery3.CreatedAt = DateTime.UtcNow - TimeSpan.FromSeconds(2);  // Еще 8 секунд

        dbContext.SaveChanges();

        var delivery1FromDb = dbContext.Deliveries.FirstOrDefault(d => d.Id == delivery1.Id);
        var delivery2FromDb = dbContext.Deliveries.FirstOrDefault(d => d.Id == delivery2.Id);
        Assert.Equal(delivery1.CreatedAt, delivery1FromDb.CreatedAt);
        Assert.Equal(delivery2.CreatedAt, delivery2FromDb.CreatedAt);
        Assert.Equal("CANCELLED", delivery1FromDb.Status.ToString());
        Assert.Equal("CANCELLED", delivery2FromDb.Status.ToString());

        await Task.Delay(TimeSpan.FromSeconds(8) + TimeSpan.FromSeconds(1));

        var delivery3FromDb = dbContext.Deliveries.FirstOrDefault(d => d.Id == delivery3.Id);
        Assert.Equal("CANCELLED", delivery3FromDb.Status.ToString());

        // Drop db
        await dbContext.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"Deliveries\" RESTART IDENTITY CASCADE;");
        await dbContext.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"Stanoks\" RESTART IDENTITY;");
    }
}