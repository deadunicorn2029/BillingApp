using BillingApp.Application.Dtos;
using BillingApp.Application.Services;
using BillingApp.Infrastructure.Caching;
using BillingApp.Infrastructure.Interfaces;
using BillingApp.Infrastructure.Models;
using BillingApp.Tests.TestDoubles;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Xunit;

namespace BillingApp.Tests.Services;

public class OrderProcessingServiceTests
{
    private static OrderRequest CreateRequest(string orderNumber = "ORD-1", string gatewayId = "mock-gateway-a") =>
        new()
        {
            OrderNumber = orderNumber,
            UserId = "user-1",
            PayableAmount = 49.99m,
            PaymentGatewayId = gatewayId
        };

    [Fact]
    public async Task ProcessAsync_SuccessfulPayment_ReturnsReceiptMatchingOrder()
    {
        var gateway = new Mock<IPaymentGateway>();
        gateway.Setup(g => g.ChargeAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaymentResult { Success = true, ConfirmationCode = "CONF-1" });

        var resolver = new Mock<IPaymentGatewayResolver>();
        resolver.Setup(r => r.Resolve("mock-gateway-a")).Returns(gateway.Object);

        var sut = new OrderProcessingService(resolver.Object, new PassthroughIdempotencyCacheService());
        var request = CreateRequest();

        var result = await sut.ProcessAsync(request);

        Assert.True(result.Success);
        Assert.NotNull(result.Receipt);
        Assert.Equal(request.OrderNumber, result.Receipt!.OrderNumber);
        Assert.Equal(request.PayableAmount, result.Receipt.Amount);
        Assert.Equal("CONF-1", result.Receipt.ConfirmationCode);
    }

    [Fact]
    public async Task ProcessAsync_DeclinedPayment_ReturnsErrorWithoutReceipt()
    {
        var gateway = new Mock<IPaymentGateway>();
        gateway.Setup(g => g.ChargeAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaymentResult { Success = false, ErrorMessage = "Card declined." });

        var resolver = new Mock<IPaymentGatewayResolver>();
        resolver.Setup(r => r.Resolve(It.IsAny<string>())).Returns(gateway.Object);

        var sut = new OrderProcessingService(resolver.Object, new PassthroughIdempotencyCacheService());

        var result = await sut.ProcessAsync(CreateRequest());

        Assert.False(result.Success);
        Assert.Null(result.Receipt);
        Assert.Equal("Card declined.", result.ErrorMessage);
    }

    [Fact]
    public async Task ProcessAsync_UnknownGateway_ReturnsErrorMentioningGatewayId()
    {
        var resolver = new Mock<IPaymentGatewayResolver>();
        resolver.Setup(r => r.Resolve("nonexistent-gateway")).Returns((IPaymentGateway?)null);

        var sut = new OrderProcessingService(resolver.Object, new PassthroughIdempotencyCacheService());

        var result = await sut.ProcessAsync(CreateRequest(gatewayId: "nonexistent-gateway"));

        Assert.False(result.Success);
        Assert.Contains("nonexistent-gateway", result.ErrorMessage);
    }

    [Fact]
    public async Task ProcessAsync_SameOrderNumberTwice_OnlyChargesGatewayOnce()
    {
        var callCount = 0;
        var gateway = new Mock<IPaymentGateway>();
        gateway.Setup(g => g.ChargeAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() =>
            {
                callCount++;
                return new PaymentResult { Success = true, ConfirmationCode = $"CONF-{callCount}" };
            });

        var resolver = new Mock<IPaymentGatewayResolver>();
        resolver.Setup(r => r.Resolve("mock-gateway-a")).Returns(gateway.Object);

        var cache = new MemoryIdempotencyCacheService(new MemoryCache(new MemoryCacheOptions()));
        var sut = new OrderProcessingService(resolver.Object, cache);
        var request = CreateRequest();

        var first = await sut.ProcessAsync(request);
        var second = await sut.ProcessAsync(request);

        Assert.Equal(1, callCount);
        Assert.Equal(first.Receipt!.ConfirmationCode, second.Receipt!.ConfirmationCode);
    }
}
