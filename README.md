# WristPay — Apple Watch UPI App

A native Apple Watch + iPhone companion app for sending UPI payments from the wrist,
using a contact, mobile number, or UPI ID — no pre-added beneficiary required.

## Status

Early-stage prototype. See `docs/` for the full project plan (product, security model,
regulatory path, and architecture decisions).

## Planned structure

```
apps/
  ios/          iPhone companion app (Swift + SwiftUI)
  watchos/      Apple Watch app (Swift + SwiftUI)
backend/        Core payment backend (C# / ASP.NET Core)
ai/             AI/ML services — fraud, risk, voice/NLP (Python + FastAPI)
infrastructure/ IaC, deployment config
docs/           Architecture, security, compliance, bank-integration docs
```
