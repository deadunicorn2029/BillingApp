using System.Collections.Concurrent;
using BillingApp.Infrastructure.Interfaces;
using BillingApp.Infrastructure.Models;
using Microsoft.Extensions.Caching.Memory;

namespace BillingApp.Infrastructure.Caching;

/// <summary>
/// In-memory implementation of <see cref="IIdempotencyCacheService"/>. Registered as a
/// singleton so the cache and the per-key locks are shared across all requests.
/// </summary>
public sealed class MemoryIdempotencyCacheService : IIdempotencyCacheService
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(24);

    private readonly IMemoryCache _cache;
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();

    public MemoryIdempotencyCacheService(IMemoryCache cache)
    {
        _cache = cache;
    }

    public async Task<OrderProcessingResult> GetOrProcessAsync(
        string key,
        Func<Task<OrderProcessingResult>> processAsync,
        CancellationToken ct = default)
    {
        if (_cache.TryGetValue(key, out OrderProcessingResult? cached))
        {
            return cached!;
        }

        var keyLock = _locks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
        await keyLock.WaitAsync(ct);
        try
        {
            // Another request for the same key may have finished while we were waiting.
            if (_cache.TryGetValue(key, out cached))
            {
                return cached!;
            }

            var result = await processAsync();
            _cache.Set(key, result, CacheDuration);
            return result;
        }
        finally
        {
            keyLock.Release();
            _locks.TryRemove(key, out _);
        }
    }
}
