using BillingApp.Application.Dtos;
using BillingApp.Application.Interfaces;
using BillingApp.WebApi.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace BillingApp.WebApi.Controllers;

[ApiController]
[Route("api/orders")]
public sealed class OrdersController : ControllerBase
{
    private readonly IOrderProcessingService _orderProcessingService;

    public OrdersController(IOrderProcessingService orderProcessingService)
    {
        _orderProcessingService = orderProcessingService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(OrderReceipt), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status402PaymentRequired)]
    public async Task<IActionResult> SubmitOrder([FromBody] SubmitOrderRequest request, CancellationToken ct)
    {
        var orderRequest = new OrderRequest
        {
            OrderNumber = request.OrderNumber,
            UserId = request.UserId,
            PayableAmount = request.PayableAmount,
            PaymentGatewayId = request.PaymentGatewayId,
            Description = request.Description
        };

        var result = await _orderProcessingService.ProcessAsync(orderRequest, ct);

        return result.Success
            ? Ok(result.Receipt)
            : StatusCode(StatusCodes.Status402PaymentRequired, new ErrorResponse { Message = result.ErrorMessage! });
    }
}
