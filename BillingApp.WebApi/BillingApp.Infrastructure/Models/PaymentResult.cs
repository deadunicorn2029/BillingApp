namespace BillingApp.Infrastructure.Models;

public sealed class PaymentResult
{
    public required bool Success { get; init; }
    public string? ConfirmationCode { get; init; }
    public string? ErrorMessage { get; init; }
}
