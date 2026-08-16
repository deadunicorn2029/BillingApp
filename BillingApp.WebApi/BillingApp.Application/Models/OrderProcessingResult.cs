namespace BillingApp.Application.Models;

public sealed class OrderProcessingResult
{
    public required bool Success { get; init; }
    public Receipt? Receipt { get; init; }
    public string? ErrorMessage { get; init; }
}
