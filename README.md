# Billing Application

Full-stack billing app: an ASP.NET Core 9 Web API backend and an Angular 17 frontend. Users submit orders through the UI, orders are processed through mocked payment gateways, and resubmitting the same order never double-charges.

## Overview

- **Backend:** ASP.NET Core 9 Web API, layered architecture (`WebApi → Application → Infrastructure`)
- **Frontend:** Angular 17 (standalone components, Reactive Forms)
- Payment gateways are **mocked** - no external API keys, works out of the box
- **Idempotent** order processing - resubmitting the same `orderNumber` never re-charges the gateway

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- Node.js 20.11+ and npm

> The frontend is pinned to Angular CLI 17 (see `BillingApp.UI/package.json`). Newer Angular majors (18+) require a Node patch version this environment didn't have available; if your Node is newer, Angular 18+ will also work fine.

## Running the backend

```bash
cd BillingApp.WebApi/BillingApp.WebApi
dotnet run
```

Listens on `http://localhost:5112` (Development environment by default - set via `launchSettings.json`). Swagger UI: `http://localhost:5112/swagger`.

## Running the frontend

```bash
cd BillingApp.UI
npm install
npm start
```

Opens on `http://localhost:4200`. Requests to `/api/*` are proxied to the backend automatically (`BillingApp.UI/proxy.conf.json`) - no CORS configuration needed on the backend.

**Both need to be running at the same time** for the UI to work end-to-end.

## Testing the API without the UI

Open Swagger UI (`http://localhost:5112/swagger`) and try `POST /api/orders` directly.

### Order payload

```json
{
  "orderNumber": "string, required",
  "userId": "string, required",
  "payableAmount": "decimal, required, > 0",
  "paymentGatewayId": "string, required - mock-gateway-a | mock-gateway-b",
  "description": "string, optional"
}
```

### Available gateways

- `mock-gateway-a` - always approves
- `mock-gateway-b` - declines ~20% of the time at random, to exercise the error path

### Idempotency note

Resubmitting the **exact same** `orderNumber` returns the cached result of the first attempt instead of charging the gateway again. Use a fresh `orderNumber` each time you want to test a new outcome (this trips people up when clicking "Execute" repeatedly in Swagger without changing the request body).

## Running the automated tests

```bash
cd BillingApp.WebApi
dotnet test
```

20 tests (unit + integration) covering backend logic - success/decline/unknown-gateway paths, idempotency (including concurrent requests), and the real HTTP pipeline via `WebApplicationFactory`.

## Configuration

No external configuration, API keys, or secrets are required.

| Setting | Where | Default |
|---|---|---|
| Backend URL | `BillingApp.WebApi/BillingApp.WebApi/Properties/launchSettings.json` | `http://localhost:5112` |
| Frontend API base URL | `BillingApp.UI/src/environments/environment*.ts` (`apiUrl`) | `/api` (proxied) |
| Frontend dev proxy target | `BillingApp.UI/proxy.conf.json` | `http://localhost:5112` |

If you change the backend port, update `proxy.conf.json` to match.

## Architecture

```
BillingApp.WebApi/          - HTTP layer: controllers, DI composition (Program.cs), exception handling, Swagger
BillingApp.Application/     - business orchestration + public DTOs (OrderRequest, OrderReceipt, OrderResult)
BillingApp.Infrastructure/  - mocked payment gateways, idempotency cache, internal domain models
BillingApp.Tests/           - unit + integration tests (xUnit, Moq, WebApplicationFactory)
BillingApp.UI/               - Angular frontend
```

**Dependency direction:** `WebApi → Application → Infrastructure`. `WebApi` references only `Application` - it never touches `Infrastructure` directly, not even for DI: `Program.cs` calls a single `builder.Services.AddApplication()`, which internally wires up Infrastructure's mocked gateways and cache (`AddInfrastructure()`). The controller only ever sees `Application`'s DTOs (`OrderRequest` / `OrderReceipt` / `OrderResult`) - never Infrastructure's internal models.

### Idempotency

`IIdempotencyCacheService` (in-memory implementation: `MemoryIdempotencyCacheService`) wraps every order-processing call: the same `OrderNumber` submitted twice - even concurrently - only reaches the payment gateway once; every other caller gets the first call's cached result.

### Adding a new payment gateway

Implement `IPaymentGateway` (`BillingApp.Infrastructure/Interfaces`) and register it in `BillingApp.Infrastructure/ServiceCollectionExtensions.cs` (`AddInfrastructure`) - the resolver picks it up automatically by its `GatewayId`.

### Swapping the idempotency cache for Redis

Implement `IIdempotencyCacheService` against `IDistributedCache` and swap the registration in `AddInfrastructure` - nothing else in the app changes.

## Bonus features implemented

- Swagger/OpenAPI documentation (`/swagger`)
- Global exception handling with structured error responses
- Integration tests via `WebApplicationFactory`
- Frontend request validation (required fields, minimum amount)

Not implemented (out of scope for this submission): retry logic, structured logging, Docker/Docker Compose - `dotnet run` / `npm start` already satisfy the "runs out of the box" requirement without them.
