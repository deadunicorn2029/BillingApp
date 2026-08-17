using BillingApp.Application.Models;
using BillingApp.Infrastructure.Caching;
using Microsoft.Extensions.Caching.Memory;
using Xunit;

namespace BillingApp.Tests.Caching;

public class MemoryIdempotencyCacheServiceTests
{
    private static MemoryIdempotencyCacheService CreateSut() =>
        new(new MemoryCache(new MemoryCacheOptions()));

    [Fact]
    public async Task GetOrProcessAsync_SameKeyTwice_OnlyInvokesFactoryOnce()
    {
        var sut = CreateSut();
        var callCount = 0;

        Task<OrderProcessingResult> Process()
        {
            callCount++;
            return Task.FromResult(new OrderProcessingResult { Success = true });
        }

        var first = await sut.GetOrProcessAsync("key-1", Process);
        var second = await sut.GetOrProcessAsync("key-1", Process);

        Assert.Equal(1, callCount);
        Assert.Same(first, second);
    }

    [Fact]
    public async Task GetOrProcessAsync_DifferentKeys_InvokesFactoryForEachKey()
    {
        var sut = CreateSut();
        var callCount = 0;

        Task<OrderProcessingResult> Process()
        {
            callCount++;
            return Task.FromResult(new OrderProcessingResult { Success = true });
        }

        await sut.GetOrProcessAsync("key-1", Process);
        await sut.GetOrProcessAsync("key-2", Process);

        Assert.Equal(2, callCount);
    }

    [Fact]
    public async Task GetOrProcessAsync_ConcurrentCallsWithSameKey_OnlyInvokesFactoryOnce()
    {
        var sut = CreateSut();
        var callCount = 0;
        var gate = new TaskCompletionSource();

        async Task<OrderProcessingResult> Process()
        {
            Interlocked.Increment(ref callCount);
            await gate.Task; // held open until every concurrent caller has queued on the lock
            return new OrderProcessingResult { Success = true };
        }

        var tasks = Enumerable.Range(0, 10)
            .Select(_ => sut.GetOrProcessAsync("same-key", Process))
            .ToArray();

        await Task.Delay(50);
        gate.SetResult();

        var results = await Task.WhenAll(tasks);

        Assert.Equal(1, callCount);
        Assert.All(results, r => Assert.True(r.Success));
    }
}
