using BillingApp.Application.Models;
using BillingApp.Infrastructure.PaymentGateways;
using Xunit;

namespace BillingApp.Tests.PaymentGateways;

public class MockGatewayATests
{
    [Fact]
    public async Task ChargeAsync_AlwaysSucceedsWithConfirmationCode()
    {
        var sut = new MockGatewayA();
        var order = new Order { OrderNumber = "ORD-1", UserId = "user-1", PayableAmount = 10m, PaymentGatewayId = sut.GatewayId };

        var result = await sut.ChargeAsync(order);

        Assert.True(result.Success);
        Assert.False(string.IsNullOrWhiteSpace(result.ConfirmationCode));
    }
}

public class MockGatewayBTests
{
    [Fact]
    public async Task ChargeAsync_ResultShapeIsAlwaysConsistent()
    {
        var sut = new MockGatewayB();
        var order = new Order { OrderNumber = "ORD-1", UserId = "user-1", PayableAmount = 10m, PaymentGatewayId = sut.GatewayId };

        for (var i = 0; i < 50; i++)
        {
            var result = await sut.ChargeAsync(order);

            if (result.Success)
            {
                Assert.False(string.IsNullOrWhiteSpace(result.ConfirmationCode));
            }
            else
            {
                Assert.False(string.IsNullOrWhiteSpace(result.ErrorMessage));
            }
        }
    }
}
