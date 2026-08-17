using BillingApp.Application.Interfaces;
using BillingApp.Application.Models;

namespace BillingApp.Infrastructure.PaymentGateways;

/// <summary> Mock gateway that randomly declines ~20% of payments, to exercise the error path.</summary>
public sealed class MockGatewayB : IPaymentGateway
{
    private const int DeclineChancePercent = 20;

    public string GatewayId => "mock-gateway-b";

    public Task<PaymentResult> ChargeAsync(Order order, CancellationToken ct = default)
    {
        var declined = Random.Shared.Next(100) < DeclineChancePercent;

        var result = declined
            ? new PaymentResult { Success = false, ErrorMessage = "Payment declined by mock-gateway-b." }
            : new PaymentResult { Success = true, ConfirmationCode = $"B-{Guid.NewGuid():N}" };

        return Task.FromResult(result);
    }
}
