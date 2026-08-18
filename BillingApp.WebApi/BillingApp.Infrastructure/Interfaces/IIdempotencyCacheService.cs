using BillingApp.Infrastructure.Models;

namespace BillingApp.Infrastructure.Interfaces;

/// <summary>
/// Guarantees that a keyed operation (order processing) runs at most once,
/// returning the cached result for any repeated call with the same key.
/// </summary>
public interface IIdempotencyCacheService
{
    Task<OrderProcessingResult> GetOrProcessAsync(
        string key,
        Func<Task<OrderProcessingResult>> processAsync,
        CancellationToken ct = default);
}
