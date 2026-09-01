# WristPay — iPhone companion app

Same status as `apps/watchos`: no `.xcodeproj` yet, needs Xcode. Files under `WristPayPhone/Sources/`
are the planned SwiftUI setup flow.

## Role

The iPhone is the **security-management device** — the Watch is the **payment-initiation device**.
Per `/docs/security.md`:

- The 6-digit app PIN is created and reset only here, never on the Watch.
- Watch registration/removal and "Disable Payments" (lost-Watch kill switch) happen here.
- No Face ID / interactive approval on payment attempts — the iPhone answers a silent
  WatchConnectivity challenge automatically when reachable (see `apps/watchos/.../CompanionPresenceChecker.swift`).

## When you have a Mac

Create a new iOS App project named `WristPayPhone`, copy `Sources/` in, and implement the
`PinCredentialProvisioning` TODOs against Keychain + your account backend.
