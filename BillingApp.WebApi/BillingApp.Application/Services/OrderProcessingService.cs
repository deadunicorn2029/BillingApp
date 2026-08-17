using BillingApp.Application.Interfaces;
using BillingApp.Application.Models;

namespace BillingApp.Application.Services;

public sealed class OrderProcessingService : IOrderProcessingService
{
    private readonly IPaymentGatewayResolver _gatewayResolver;
    private readonly IIdempotencyCacheService _idempotencyCache;

    public OrderProcessingService(
        IPaymentGatewayResolver gatewayResolver,
        IIdempotencyCacheService idempotencyCache)
    {
        _gatewayResolver = gatewayResolver;
        _idempotencyCache = idempotencyCache;
    }

    public Task<OrderProcessingResult> ProcessAsync(Order order, CancellationToken ct = default) =>
        _idempotencyCache.GetOrProcessAsync(order.OrderNumber, () => ChargeAsync(order, ct), ct);

    private async Task<OrderProcessingResult> ChargeAsync(Order order, CancellationToken ct)
    {
        var gateway = _gatewayResolver.Resolve(order.PaymentGatewayId);
        if (gateway is null)
        {
            return new OrderProcessingResult
            {
                Success = false,
                ErrorMessage = $"Unknown payment gateway: '{order.PaymentGatewayId}'."
            };
        }

        var paymentResult = await gateway.ChargeAsync(order, ct);

        if (!paymentResult.Success)
        {
            return new OrderProcessingResult
            {
                Success = false,
                ErrorMessage = paymentResult.ErrorMessage ?? "Payment was declined."
            };
        }

        var receipt = new Receipt
        {
            OrderNumber = order.OrderNumber,
            Amount = order.PayableAmount,
            Timestamp = DateTimeOffset.UtcNow,
            ConfirmationCode = paymentResult.ConfirmationCode!
        };

        return new OrderProcessingResult { Success = true, Receipt = receipt };
    }
}
