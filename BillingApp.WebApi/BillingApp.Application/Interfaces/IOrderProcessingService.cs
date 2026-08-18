using BillingApp.Application.Dtos;

namespace BillingApp.Application.Interfaces;

public interface IOrderProcessingService
{
    Task<OrderResult> ProcessAsync(OrderRequest request, CancellationToken ct = default);
}
