using BillingApp.Application.Interfaces;
using BillingApp.Application.Models;

namespace BillingApp.Infrastructure.PaymentGateways;

/// <summary> Mock gateway that always approves the payment.</summary>
public sealed class MockGatewayA : IPaymentGateway
{
    public string GatewayId => "mock-gateway-a";

    public Task<PaymentResult> ChargeAsync(Order order, CancellationToken ct = default)
    {
        var result = new PaymentResult
        {
            Success = true,
            ConfirmationCode = $"A-{Guid.NewGuid():N}"
        };

        return Task.FromResult(result);
    }
}
