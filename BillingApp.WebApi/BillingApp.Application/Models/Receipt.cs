namespace BillingApp.Application.Models;

public sealed class Receipt
{
    public required string OrderNumber { get; init; }
    public required decimal Amount { get; init; }
    public required DateTimeOffset Timestamp { get; init; }
    public required string ConfirmationCode { get; init; }
}
