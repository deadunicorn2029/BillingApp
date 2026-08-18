using BillingApp.Infrastructure.Models;

namespace BillingApp.Infrastructure.Interfaces;

public interface IPaymentGateway
{
    string GatewayId { get; }

    Task<PaymentResult> ChargeAsync(Order order, CancellationToken ct = default);
}
