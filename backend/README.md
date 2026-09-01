# WristPay backend

ASP.NET Core modular monolith. Requires the .NET 10 SDK.

```
src/
  WristPay.Domain          Entities: PaymentIntent, Recipient, TransactionLimits, PaymentStatus
  WristPay.Application     PaymentService orchestration + provider/resolver/store interfaces
  WristPay.Infrastructure  Mock implementations (payment provider, recipient resolver, in-memory store)
  WristPay.Api             Controllers, Program.cs
tests/
  WristPay.Tests           xUnit tests for PaymentService
```

## Run

```
dotnet run --project src/WristPay.Api
```

Defaults to whatever port `launchSettings.json` picks (or set `ASPNETCORE_URLS`). Health check at
`GET /health`.

## Test

```
dotnet test
```

## Smoke-test the mock end-to-end payment flow

```bash
curl -X POST http://localhost:5199/api/payments \
  -H "Content-Type: application/json" \
  -d '{"idempotencyKey":"demo-1","senderMobileNumber":"9000000001","recipientQuery":"9876543210","amountInr":500}'
```

Known mock recipients (`WristPay.Infrastructure/MockRecipientResolver.cs`): `9876543210` /
`rahul@okhdfcbank`, `9123456780` / `kishore@okhdfcbank`. Mobile number `0000000000` resolves but the
mock PSP always declines it, for testing the failure path.

## Next steps (not done yet)

- Swap `InMemoryPaymentIntentStore` for a PostgreSQL-backed store via EF Core + Npgsql
  (`infrastructure/docker-compose.yml` has a local Postgres/Redis stack ready for this).
- Swap `MockPaymentProvider` / `MockRecipientResolver` for real PSP-bank/NPCI-approved integrations
  once that access exists — nothing else in the codebase should need to change.
- Add auth (device-bound tokens) once `apps/ios` account creation exists.
