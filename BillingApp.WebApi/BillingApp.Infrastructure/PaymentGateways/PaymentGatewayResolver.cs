using BillingApp.Application.Interfaces;

namespace BillingApp.Infrastructure.PaymentGateways;

public sealed class PaymentGatewayResolver : IPaymentGatewayResolver
{
    private readonly IReadOnlyDictionary<string, IPaymentGateway> _gateways;

    public PaymentGatewayResolver(IEnumerable<IPaymentGateway> gateways)
    {
        _gateways = gateways.ToDictionary(g => g.GatewayId, StringComparer.OrdinalIgnoreCase);
    }

    public IPaymentGateway? Resolve(string gatewayId) =>
        _gateways.GetValueOrDefault(gatewayId);
}
