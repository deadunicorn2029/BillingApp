using BillingApp.Application.Interfaces;
using BillingApp.Application.Services;
using BillingApp.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace BillingApp.Application;

public static class ServiceCollectionExtensions
{
    /// <summary>Wires up Application and, transitively, Infrastructure — WebApi never needs to know Infrastructure exists.</summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddInfrastructure();
        services.AddSingleton<IOrderProcessingService, OrderProcessingService>();

        return services;
    }
}
