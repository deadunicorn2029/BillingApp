using System.Net;
using System.Net.Http.Json;
using BillingApp.Application.Dtos;
using BillingApp.WebApi.Contracts;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace BillingApp.Tests.Integration;

/// <summary>
/// Exercises the real HTTP pipeline (routing, model binding, validation, DI-wired mock gateways
/// and idempotency cache, exception handling) through an in-memory TestServer — no external
/// dependencies, matches how the app actually runs via `dotnet run`.
/// </summary>
public class OrdersApiTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public OrdersApiTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private static SubmitOrderRequest CreateRequest(string orderNumber, string gatewayId = "mock-gateway-a") => new()
    {
        OrderNumber = orderNumber,
        UserId = "user-1",
        PayableAmount = 19.99m,
        PaymentGatewayId = gatewayId,
        Description = "integration test order"
    };

    [Fact]
    public async Task PostOrder_ValidOrderOnMockGatewayA_ReturnsOkWithReceipt()
    {
        var response = await _client.PostAsJsonAsync("/api/orders", CreateRequest($"IT-{Guid.NewGuid():N}"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var receipt = await response.Content.ReadFromJsonAsync<OrderReceipt>();
        Assert.NotNull(receipt);
        Assert.False(string.IsNullOrWhiteSpace(receipt!.ConfirmationCode));
    }

    [Fact]
    public async Task PostOrder_UnknownGateway_Returns402WithErrorMessage()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/orders", CreateRequest($"IT-{Guid.NewGuid():N}", gatewayId: "does-not-exist"));

        Assert.Equal((HttpStatusCode)StatusCodes.Status402PaymentRequired, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.Contains("does-not-exist", error!.Message);
    }

    [Fact]
    public async Task PostOrder_MissingRequiredField_ReturnsBadRequest()
    {
        var payload = new { userId = "user-1", payableAmount = 10m, paymentGatewayId = "mock-gateway-a" };

        var response = await _client.PostAsJsonAsync("/api/orders", payload);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostOrder_SameOrderNumberTwice_ReturnsIdenticalReceipt()
    {
        var request = CreateRequest($"IT-{Guid.NewGuid():N}");

        var firstResponse = await _client.PostAsJsonAsync("/api/orders", request);
        var secondResponse = await _client.PostAsJsonAsync("/api/orders", request);

        var first = await firstResponse.Content.ReadFromJsonAsync<OrderReceipt>();
        var second = await secondResponse.Content.ReadFromJsonAsync<OrderReceipt>();

        Assert.Equal(first!.ConfirmationCode, second!.ConfirmationCode);
        Assert.Equal(first.Timestamp, second.Timestamp);
    }

    [Fact]
    public async Task GetSwaggerDocument_ReturnsOk()
    {
        var response = await _client.GetAsync("/swagger/v1/swagger.json");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
