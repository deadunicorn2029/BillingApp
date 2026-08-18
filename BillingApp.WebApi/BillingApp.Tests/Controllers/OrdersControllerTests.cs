using BillingApp.Application.Dtos;
using BillingApp.Application.Interfaces;
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
        var receipt = new OrderReceipt
        {
            OrderNumber = "ORD-1",
            Amount = 25.5m,
            Timestamp = DateTimeOffset.UtcNow,
            ConfirmationCode = "CONF-1"
        };
        var service = new Mock<IOrderProcessingService>();
        service.Setup(s => s.ProcessAsync(It.IsAny<OrderRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OrderResult { Success = true, Receipt = receipt });

        var sut = new OrdersController(service.Object);

        var actionResult = await sut.SubmitOrder(CreateRequest(), CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        Assert.Same(receipt, okResult.Value);
    }

    [Fact]
    public async Task SubmitOrder_DeclinedPayment_Returns402WithErrorResponse()
    {
        var service = new Mock<IOrderProcessingService>();
        service.Setup(s => s.ProcessAsync(It.IsAny<OrderRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OrderResult { Success = false, ErrorMessage = "Payment declined." });

        var sut = new OrdersController(service.Object);

        var actionResult = await sut.SubmitOrder(CreateRequest(), CancellationToken.None);

        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal(StatusCodes.Status402PaymentRequired, objectResult.StatusCode);
        var error = Assert.IsType<ErrorResponse>(objectResult.Value);
        Assert.Equal("Payment declined.", error.Message);
    }

    [Fact]
    public async Task SubmitOrder_MapsRequestFieldsOntoOrderRequest()
    {
        OrderRequest? captured = null;
        var service = new Mock<IOrderProcessingService>();
        service.Setup(s => s.ProcessAsync(It.IsAny<OrderRequest>(), It.IsAny<CancellationToken>()))
            .Callback<OrderRequest, CancellationToken>((request, _) => captured = request)
            .ReturnsAsync(new OrderResult { Success = false, ErrorMessage = "n/a" });

        var sut = new OrdersController(service.Object);
        var request = CreateRequest();

        await sut.SubmitOrder(request, CancellationToken.None);

        Assert.NotNull(captured);
        Assert.Equal(request.OrderNumber, captured!.OrderNumber);
        Assert.Equal(request.UserId, captured.UserId);
        Assert.Equal(request.PayableAmount, captured.PayableAmount);
        Assert.Equal(request.PaymentGatewayId, captured.PaymentGatewayId);
        Assert.Equal(request.Description, captured.Description);
    }
}
