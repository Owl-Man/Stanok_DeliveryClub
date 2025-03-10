using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Stanok.Application.Services;
using Stanok.Core.Abstractions;
using Stanok_DeliveryClub.Contracts;
using Stanok_DeliveryClub.Controllers;
using Xunit;

namespace Stanok.Tests;

public class StanokControllerTests
{
    private readonly Mock<IStanokService> mockStanokService;
    private readonly Mock<IDeliveryService> mockDeliveryService;
    private readonly Mock<IDeliveryTimeoutService> _mockDeliveryTimeoutService;

    private readonly Mock<IDeliveriesRepository> _mockDeliveriesRepository;

    private readonly StanoksController stanokController;
    private readonly ILogger<StanokControllerTests> logger;

    public StanokControllerTests()
    {
        mockStanokService = new Mock<IStanokService>();
        mockDeliveryService = new Mock<IDeliveryService>();
        _mockDeliveryTimeoutService = new Mock<IDeliveryTimeoutService>();

        stanokController = new StanoksController(mockStanokService.Object, mockDeliveryService.Object, _mockDeliveryTimeoutService.Object);

        logger = LoggerFactory.Create(builder =>
        {
            builder.SetMinimumLevel(LogLevel.Debug);
            builder.AddConsole();
        }).CreateLogger<StanokControllerTests>();
    }

    [Fact]
    public void CreateStanok_ReturnsGuid() 
    {
        StanokRequest stanokRequest = new StanokRequest("Test stanok", "Test stanok manufacturer", 20000000);

        var newStanokId = Guid.NewGuid();
        var newDeliveryId = Guid.NewGuid();

        mockStanokService.Setup(service => service.Create(It.IsAny<Guid>(), It.IsAny<string>(),
            It.IsAny<string>(), It.IsAny<double>())).Returns(newStanokId);

        mockDeliveryService.Setup(service => service.Create(It.IsAny<Guid>(),It.IsAny<Guid>())).Returns(newDeliveryId);

        _mockDeliveryTimeoutService.Setup(service => service.StartTimerForNewDelivery(It.IsAny<Guid>()));

        var result = stanokController.CreateStanok(stanokRequest);

        var actionResult = Assert.IsType<ActionResult<StanokResponse>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);

        var stanokResponse = Assert.IsType<StanokResponse>(okResult.Value);

        logger.LogDebug("stanok objects: {}", stanokResponse);
        Assert.Equal(newStanokId, stanokResponse.id);
        Assert.Equal("Test stanok", stanokResponse.name);
        Assert.Equal("Test stanok manufacturer", stanokResponse.manufacturer);
        Assert.Equal(20000000, stanokResponse.price);

        mockStanokService.Verify(service => service.Create(
            It.IsAny<Guid>(),
            stanokRequest.name,
            stanokRequest.manufacturer,
            stanokRequest.price), Times.Once());

        mockDeliveryService.Verify(service => service.Create(
            It.IsAny<Guid>(),
            newStanokId), Times.Once());

        _mockDeliveryTimeoutService.Verify(service => service.StartTimerForNewDelivery(newDeliveryId));
    }

    [Fact]
    public void CreateStanok_LoadTest_ReturnsGuid()
    {
        //StanokRequest stanokRequest = new StanokRequest("Test stanok", "Test stanok manufacturer", 20000000);

        const int stanokCount = 10000;
        var stanokRequests = new List<StanokRequest>();

        var stanokIds = new List<Guid>();
        var deliveryIds = new List<Guid>();

        for (int i = 0; i < stanokCount; i++)
        {
            stanokRequests.Add(new StanokRequest($"Stanok_{i}", $"Manufacturer_{i}", 1000 + i));
            stanokIds.Add(Guid.NewGuid());
            deliveryIds.Add(Guid.NewGuid());
        }

        for (int i = 0; i < stanokCount; i++)
        {
            mockStanokService.Setup(service => service.Create(It.IsAny<Guid>(), It.IsAny<string>(),
            It.IsAny<string>(), It.IsAny<double>())).Returns(stanokIds[i]);

            mockDeliveryService.Setup(service => service.Create(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(deliveryIds[i]);

            _mockDeliveryTimeoutService.Setup(service => service.StartTimerForNewDelivery(It.IsAny<Guid>()));

            var result = stanokController.CreateStanok(stanokRequests[i]);

            var actionResult = Assert.IsType<ActionResult<StanokResponse>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);

            var stanokResponse = Assert.IsType<StanokResponse>(okResult.Value);

            logger.LogDebug("stanok objects: {}", stanokResponse);
            Assert.Equal(stanokIds[i], stanokResponse.id);
            Assert.Equal(stanokRequests[i].name, stanokResponse.name);
            Assert.Equal(stanokRequests[i].manufacturer, stanokResponse.manufacturer);
            Assert.Equal(stanokRequests[i].price, stanokResponse.price);
        }
    }

    /*[Fact]
    public async Task SimulateServerRestart_10Timers_DifferentConditions()
    {
        // Arrange
        const int deliveryCount = 10;
        var deliveries = new List<Delivery>();
        var currentTime = DateTime.UtcNow;
        var maxStatusIgnoreTime = TimeSpan.FromMinutes(10); // 10 минут максимального времени ожидания

        // Создаем 10 доставок с разным временем создания и статусами
        for (int i = 0; i < deliveryCount; i++)
        {
            var stanokId = Guid.NewGuid();
            var deliveryId = Guid.NewGuid();
            var createdAt = currentTime.AddMinutes(-i * 2); // Разница в 2 минуты между каждой доставкой
            var status = i < 5 ? Status.CREATE : Status.IN_DELIVERY; // Первые 5 - CREATE, остальные - IN_DELIVERY

            deliveries.Add(new Delivery(deliveryId, stanokId, status, createdAt));
        }

        // Настройка репозитория для возврата списка доставок
        _mockDeliveriesRepository.Setup(repo => repo.GetAll())
            .Returns(deliveries.Where(d => d.Status == Status.CREATE).ToList());

        // Настройка Update для отслеживания изменений статуса
        _mockDeliveriesRepository.Setup(repo => repo.Update(It.IsAny<Guid>(), Status.CANCELED))
            .Returns((Guid id, Status _) => id);

        // Создаем экземпляр DeliveryTimeoutService с моками
        var deliveryTimeoutService = new DeliveryTimeoutService(
            new Mock<IServiceScopeFactory>(MockBehavior.Strict).Object);

        // Настраиваем IServiceScopeFactory для возврата наших моков
        var mockScope = new Mock<IServiceScope>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        mockServiceProvider.Setup(sp => sp.GetService(typeof(IDeliveriesRepository)))
            .Returns(_mockDeliveriesRepository.Object);
        mockScope.Setup(s => s.ServiceProvider).Returns(mockServiceProvider.Object);
        var mockScopeFactory = new Mock<IServiceScopeFactory>();
        mockScopeFactory.Setup(f => f.CreateScope()).Returns(mockScope.Object);

        var testableService = new DeliveryTimeoutService(mockScopeFactory.Object);

        // Act: Эмулируем перезапуск сервера
        await testableService.RestoreTimers();

        // Даем таймерам время отработать (в реальном тесте это зависит от реализации)
        await Task.Delay(TimeSpan.FromSeconds(15)); // Ждем 15 секунд, чтобы все таймеры сработали

        // Assert
        var pendingDeliveries = deliveries.Where(d => d.Status == Status.CREATE).ToList();
        foreach (var delivery in pendingDeliveries)
        {
            var elapsedTime = currentTime - delivery.CreatedAt;
            if (elapsedTime > maxStatusIgnoreTime)
            {
                // Если прошло больше 10 минут, статус должен быть изменен на CANCELED
                _mockDeliveriesRepository.Verify(repo => repo.Update(delivery.Id, Status.CANCELED), Times.Once());
            }
            else
            {
                // Если меньше 10 минут, статус не должен меняться
                _mockDeliveriesRepository.Verify(repo => repo.Update(delivery.Id, Status.CANCELED), Times.Never());
            }
        }

        // Проверяем, что для доставок со статусом IN_DELIVERY не вызывался Update
        var nonPendingDeliveries = deliveries.Where(d => d.Status != Status.CREATE).ToList();
        foreach (var delivery in nonPendingDeliveries)
        {
            _mockDeliveriesRepository.Verify(repo => repo.Update(delivery.Id, Status.CANCELED), Times.Never());
        }

        _logger.LogDebug("Successfully verified 10 timers after server restart simulation");
    }*/
}
