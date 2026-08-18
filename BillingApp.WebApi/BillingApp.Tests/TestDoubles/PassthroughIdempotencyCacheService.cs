using BillingApp.Infrastructure.Interfaces;
using BillingApp.Infrastructure.Models;

namespace BillingApp.Tests.TestDoubles;

/// <summary> Executes the factory immediately, without caching — isolates the caller's own logic from caching behavior.</summary>
internal sealed class PassthroughIdempotencyCacheService : IIdempotencyCacheService
{
    public Task<OrderProcessingResult> GetOrProcessAsync(
        string key,
        Func<Task<OrderProcessingResult>> processAsync,
        CancellationToken ct = default) => processAsync();
}
