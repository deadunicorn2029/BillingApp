using BillingApp.Infrastructure.Caching;
using BillingApp.Infrastructure.Interfaces;
using BillingApp.Infrastructure.PaymentGateways;
using Microsoft.Extensions.DependencyInjection;

namespace BillingApp.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddMemoryCache();

        services.AddSingleton<IPaymentGateway, MockGatewayA>();
        services.AddSingleton<IPaymentGateway, MockGatewayB>();
        services.AddSingleton<IPaymentGatewayResolver, PaymentGatewayResolver>();
        services.AddSingleton<IIdempotencyCacheService, MemoryIdempotencyCacheService>();

        return services;
    }
}
