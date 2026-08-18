namespace BillingApp.Infrastructure.Interfaces;

public interface IPaymentGatewayResolver
{
    IPaymentGateway? Resolve(string gatewayId);
}
