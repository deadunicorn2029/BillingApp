using BillingApp.Infrastructure.Interfaces;
using BillingApp.Infrastructure.PaymentGateways;
using Moq;
using Xunit;

namespace BillingApp.Tests.PaymentGateways;

public class PaymentGatewayResolverTests
{
    [Fact]
    public void Resolve_KnownGatewayId_ReturnsMatchingGateway()
    {
        var gatewayA = CreateGateway("mock-gateway-a");
        var gatewayB = CreateGateway("mock-gateway-b");
        var sut = new PaymentGatewayResolver(new[] { gatewayA.Object, gatewayB.Object });

        var resolved = sut.Resolve("mock-gateway-a");

        Assert.Same(gatewayA.Object, resolved);
    }

    [Fact]
    public void Resolve_IsCaseInsensitive()
    {
        var gatewayA = CreateGateway("mock-gateway-a");
        var sut = new PaymentGatewayResolver(new[] { gatewayA.Object });

        var resolved = sut.Resolve("MOCK-GATEWAY-A");

        Assert.Same(gatewayA.Object, resolved);
    }

    [Fact]
    public void Resolve_UnknownGatewayId_ReturnsNull()
    {
        var sut = new PaymentGatewayResolver(Array.Empty<IPaymentGateway>());

        var resolved = sut.Resolve("does-not-exist");

        Assert.Null(resolved);
    }

    private static Mock<IPaymentGateway> CreateGateway(string gatewayId)
    {
        var gateway = new Mock<IPaymentGateway>();
        gateway.Setup(g => g.GatewayId).Returns(gatewayId);
        return gateway;
    }
}
