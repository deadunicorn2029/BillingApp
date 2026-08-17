using BillingApp.Application.Interfaces;
using BillingApp.Infrastructure.PaymentGateways;
using BillingApp.WebApi.ExceptionHandling;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();

// Payment gateways: register each mock gateway, then the resolver that picks one by GatewayId.
builder.Services.AddSingleton<IPaymentGateway, MockGatewayA>();
builder.Services.AddSingleton<IPaymentGateway, MockGatewayB>();
builder.Services.AddSingleton<IPaymentGatewayResolver, PaymentGatewayResolver>();

var app = builder.Build();

app.UseExceptionHandler();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
