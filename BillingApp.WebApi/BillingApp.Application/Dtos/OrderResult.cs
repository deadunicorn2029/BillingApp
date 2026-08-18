namespace BillingApp.Application.Dtos;

public sealed class OrderResult
{
    public required bool Success { get; init; }
    public OrderReceipt? Receipt { get; init; }
    public string? ErrorMessage { get; init; }
}
