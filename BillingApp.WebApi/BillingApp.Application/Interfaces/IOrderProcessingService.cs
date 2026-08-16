using BillingApp.Application.Models;

namespace BillingApp.Application.Interfaces;

public interface IOrderProcessingService
{
    Task<OrderProcessingResult> ProcessAsync(Order order, CancellationToken ct = default);
}
