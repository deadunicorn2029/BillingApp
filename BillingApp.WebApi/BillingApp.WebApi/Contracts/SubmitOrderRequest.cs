using System.ComponentModel.DataAnnotations;

namespace BillingApp.WebApi.Contracts;

public sealed class SubmitOrderRequest
{
    [Required]
    public string OrderNumber { get; init; } = string.Empty;

    [Required]
    public string UserId { get; init; } = string.Empty;

    [Range(0.01, double.MaxValue, ErrorMessage = "PayableAmount must be greater than zero.")]
    public decimal PayableAmount { get; init; }

    [Required]
    public string PaymentGatewayId { get; init; } = string.Empty;

    public string? Description { get; init; }
}
