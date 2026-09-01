# Architecture

## Product

Apple Watch → contact / mobile number / UPI ID → resolve recipient → amount → confirm → UPI payment.
No pre-added beneficiary required. See root `README.md` for the pitch.

## Two regulatory tracks

| Product | Route |
|---|---|
| Watch → merchant (P2M) | NPCI's Oct 2025 UPI Circle / IoT-device circular already permits this (₹5,000/txn, ₹15,000/month per delegation). |
| Watch → person via mobile number/contact (P2P) | Target differentiator. Not covered by the current IoT framework — needs an explicit PSP-bank/NPCI-approved path. |

The backend's `TransactionLimits` defaults mirror the P2M figures above as a placeholder, not because
they're assumed to apply to the eventual P2P product.

## Stack

- **Watch + iPhone**: Swift + SwiftUI, native. `apps/watchos`, `apps/ios`.
- **Backend**: C# / ASP.NET Core, modular monolith (`backend/src`): `WristPay.Domain` →
  `WristPay.Application` → `WristPay.Infrastructure` → `WristPay.Api`.
- **Payment provider**: behind `IPaymentProvider`. `MockPaymentProvider` today; a real PSP-bank/NPCI
  integration later — the rest of the app never assumes which.
- **Recipient resolution**: behind `IRecipientResolver`. Mocked directory today; a real UPI Number/UPI
  ID lookup later.
- **Storage**: `IPaymentIntentStore`, currently `InMemoryPaymentIntentStore`. PostgreSQL is the
  intended production store (see `infrastructure/docker-compose.yml`); swapping it in only touches
  the Infrastructure layer.
- **AI/ML** (`ai/risk-service`): deferred. Fraud scoring, anomaly detection, voice/NLP — never
  authorizes a payment on its own.

## Idempotency

Every payment request carries an `idempotencyKey`. A retry with the same key returns the existing
`PaymentIntent` rather than reprocessing — enforced in `PaymentService.SendAsync`.

## What's proven end-to-end right now

`POST /api/payments` → idempotency check → limit check → recipient lookup → mock PSP → status. See
`backend/tests/WristPay.Tests/PaymentServiceTests.cs` and the smoke-test commands in
`backend/README.md`.
