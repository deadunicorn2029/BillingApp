using BillingApp.Application.Models;

namespace BillingApp.Application.Interfaces;

public interface IPaymentGateway
{
    string GatewayId { get; }

    Task<PaymentResult> ChargeAsync(Order order, CancellationToken ct = default);
}
