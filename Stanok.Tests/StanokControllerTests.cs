using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Stanok.Application.Services;
using Stanok.Core.Abstractions;
using Stanok_DeliveryClub.Contracts;
using Stanok_DeliveryClub.Controllers;

namespace Stanok.Tests;

public class StanokControllerTests
{
    private readonly Mock<IStanokService> mockStanokService;
    private readonly Mock<IDeliveryService> mockDeliveryService;
    private readonly Mock<IDeliveryTimeoutService> _mockDeliveryTimeoutService;

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

        mockStanokService.Setup(service => service.Create(It.IsAny<string>(),
            It.IsAny<string>(), It.IsAny<double>())).Returns(newStanokId);

        mockDeliveryService.Setup(service => service.Create(It.IsAny<Guid>())).Returns(newDeliveryId);

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
            stanokRequest.name,
            stanokRequest.manufacturer,
            stanokRequest.price), Times.Once());

        mockDeliveryService.Verify(service => service.Create(
            newStanokId), Times.Once());

        _mockDeliveryTimeoutService.Verify(service => service.StartTimerForNewDelivery(newDeliveryId));
    }

    [Fact]
    public void CreateStanok_LoadTest_ReturnsGuid()
    {
        //StanokRequest stanokRequest = new StanokRequest("Test stanok", "Test stanok manufacturer", 20000000);

        const int stanokCount = 100;
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
            mockStanokService.Setup(service => service.Create(It.IsAny<string>(),
            It.IsAny<string>(), It.IsAny<double>())).Returns(stanokIds[i]);

            mockDeliveryService.Setup(service => service.Create(It.IsAny<Guid>())).Returns(deliveryIds[i]);

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
}
