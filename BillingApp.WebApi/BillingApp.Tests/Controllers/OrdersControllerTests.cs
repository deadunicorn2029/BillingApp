using BillingApp.Application.Interfaces;
using BillingApp.Application.Models;
using BillingApp.WebApi.Contracts;
using BillingApp.WebApi.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace BillingApp.Tests.Controllers;

public class OrdersControllerTests
{
    private static SubmitOrderRequest CreateRequest() => new()
    {
        OrderNumber = "ORD-1",
        UserId = "user-1",
        PayableAmount = 25.5m,
        PaymentGatewayId = "mock-gateway-a",
        Description = "test"
    };

    [Fact]
    public async Task SubmitOrder_SuccessfulPayment_ReturnsOkWithReceipt()
    {
        var receipt = new Receipt
        {
            OrderNumber = "ORD-1",
            Amount = 25.5m,
            Timestamp = DateTimeOffset.UtcNow,
            ConfirmationCode = "CONF-1"
        };
        var service = new Mock<IOrderProcessingService>();
        service.Setup(s => s.ProcessAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OrderProcessingResult { Success = true, Receipt = receipt });

        var sut = new OrdersController(service.Object);

        var actionResult = await sut.SubmitOrder(CreateRequest(), CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        Assert.Same(receipt, okResult.Value);
    }

    [Fact]
    public async Task SubmitOrder_DeclinedPayment_Returns402WithErrorResponse()
    {
        var service = new Mock<IOrderProcessingService>();
        service.Setup(s => s.ProcessAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OrderProcessingResult { Success = false, ErrorMessage = "Payment declined." });

        var sut = new OrdersController(service.Object);

        var actionResult = await sut.SubmitOrder(CreateRequest(), CancellationToken.None);

        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal(StatusCodes.Status402PaymentRequired, objectResult.StatusCode);
        var error = Assert.IsType<ErrorResponse>(objectResult.Value);
        Assert.Equal("Payment declined.", error.Message);
    }

    [Fact]
    public async Task SubmitOrder_MapsRequestFieldsOntoOrder()
    {
        Order? capturedOrder = null;
        var service = new Mock<IOrderProcessingService>();
        service.Setup(s => s.ProcessAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .Callback<Order, CancellationToken>((order, _) => capturedOrder = order)
            .ReturnsAsync(new OrderProcessingResult { Success = false, ErrorMessage = "n/a" });

        var sut = new OrdersController(service.Object);
        var request = CreateRequest();

        await sut.SubmitOrder(request, CancellationToken.None);

        Assert.NotNull(capturedOrder);
        Assert.Equal(request.OrderNumber, capturedOrder!.OrderNumber);
        Assert.Equal(request.UserId, capturedOrder.UserId);
        Assert.Equal(request.PayableAmount, capturedOrder.PayableAmount);
        Assert.Equal(request.PaymentGatewayId, capturedOrder.PaymentGatewayId);
        Assert.Equal(request.Description, capturedOrder.Description);
    }
}
