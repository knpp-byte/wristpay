# WristPay — Apple Watch app

Requires Xcode + a Mac (see `/docs/architecture.md`). Nothing here compiles standalone yet — there's
no `.xcodeproj` because that has to be created in Xcode itself. The files under `WristPayWatch/Sources/`
are the planned SwiftUI views, written ahead of time so they can be dropped straight into a new
watchOS App target instead of starting from a blank screen.

## Flow these views implement

```
Open app → 6-digit PIN → silent companion-iPhone trust check (if reachable)
  → Send Money (contact / mobile number / UPI ID) → recipient resolved
  → amount → confirm → POST to WristPay.Api → payment status
```

## When you have a Mac

1. Create a new watchOS App project in Xcode named `WristPayWatch`.
2. Copy the files from `Sources/` into the new project's watch app target.
3. Add a `WristPayAPIClient.swift` that calls the backend's `/api/payments` and
   `/api/recipients/lookup` endpoints (see `backend/src/WristPay.Api/Controllers`).
4. Wire `WatchConnectivity` between this target and the iPhone app in `apps/ios/`.

Do not add Face ID/Touch ID prompts here — this app intentionally uses PIN + silent companion-device
presence instead, per the agreed security model in `/docs/security.md`.
