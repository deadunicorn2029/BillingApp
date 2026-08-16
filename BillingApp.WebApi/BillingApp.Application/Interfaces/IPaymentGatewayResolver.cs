namespace BillingApp.Application.Interfaces;

public interface IPaymentGatewayResolver
{
    IPaymentGateway? Resolve(string gatewayId);
}
