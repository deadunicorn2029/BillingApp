using BillingApp.Application.Dtos;
using BillingApp.Application.Interfaces;
using BillingApp.Infrastructure.Interfaces;
using BillingApp.Infrastructure.Models;

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

    public async Task<OrderResult> ProcessAsync(OrderRequest request, CancellationToken ct = default)
    {
        var order = new Order
        {
            OrderNumber = request.OrderNumber,
            UserId = request.UserId,
            PayableAmount = request.PayableAmount,
            PaymentGatewayId = request.PaymentGatewayId,
            Description = request.Description
        };

        var result = await _idempotencyCache.GetOrProcessAsync(order.OrderNumber, () => ChargeAsync(order, ct), ct);

        return ToOrderResult(result);
    }

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

    private static OrderResult ToOrderResult(OrderProcessingResult result) =>
        new()
        {
            Success = result.Success,
            ErrorMessage = result.ErrorMessage,
            Receipt = result.Receipt is null
                ? null
                : new OrderReceipt
                {
                    OrderNumber = result.Receipt.OrderNumber,
                    Amount = result.Receipt.Amount,
                    Timestamp = result.Receipt.Timestamp,
                    ConfirmationCode = result.Receipt.ConfirmationCode
                }
        };
}
