using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Stanok.Core.Models;
using Stanok.DataAccess;
using Stanok.DataAccess.Entities;
using Stanok_DeliveryClub.Contracts;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;

namespace Stanok.Tests;

public class DeliveryTimeoutServiceTests : BaseIntegrationTest
{
    private readonly List<DeliveryEntity> _deliveries = new();

    public DeliveryTimeoutServiceTests(TestWebAppFactory factory) : base(factory) 
    {
        var lifeTime = factory.Services.GetRequiredService<IHostApplicationLifetime>();
        lifeTime.ApplicationStarted.Register(() => PrepareTestData());
    }

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
        var delivery1FromDb = dbContext.Deliveries.FirstOrDefault(d => d.Id == _deliveries[0].Id);
        var delivery2FromDb = dbContext.Deliveries.FirstOrDefault(d => d.Id == _deliveries[1].Id);
        Assert.Equal(delivery1FromDb.CreatedAt, delivery1FromDb.CreatedAt);
        Assert.Equal(delivery2FromDb.CreatedAt, delivery2FromDb.CreatedAt);
        Assert.Equal("CANCELLED", delivery1FromDb.Status.ToString());
        Assert.Equal("CANCELLED", delivery2FromDb.Status.ToString());

        await Task.Delay(TimeSpan.FromSeconds(8));

        var delivery3FromDb = dbContext.Deliveries.FirstOrDefault(d => d.Id == _deliveries[2].Id);
        Assert.Equal("CANCELLED", delivery3FromDb.Status.ToString());

        // Drop db
        await dbContext.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"Deliveries\" RESTART IDENTITY CASCADE;");
        await dbContext.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"Stanoks\" RESTART IDENTITY;");
    }

    private void PrepareTestData()
    {
        var stanokId1 = Guid.NewGuid();
        var stanokId2 = Guid.NewGuid();
        var stanokId3 = Guid.NewGuid();

        var delivery1 = new DeliveryEntity
        {
            StanokId = stanokId1,
            Status = Status.CREATE,
            CreatedAt = DateTime.UtcNow - TimeSpan.FromSeconds(20) // Просрочена на 10 секунд
        };
        var delivery2 = new DeliveryEntity
        {
            StanokId = stanokId2,
            Status = Status.CREATE,
            CreatedAt = DateTime.UtcNow - TimeSpan.FromSeconds(10) // Только что просрочена
        };
        var delivery3 = new DeliveryEntity
        {
            StanokId = stanokId3,
            Status = Status.CREATE,
            CreatedAt = DateTime.UtcNow - TimeSpan.FromSeconds(1) // Останется активной 9 секунд
        };

        dbContext.Deliveries.AddRange(delivery1, delivery2, delivery3);
        _deliveries.AddRange(delivery1, delivery2, delivery3);

        Debug.WriteLine("DATA PREPARED------------------------------------->");

        dbContext.SaveChanges();
    }
}