# Security model

## Trust layers (Watch)

1. Apple Watch itself unlocked.
2. Watch registered/bound to the account.
3. 6-digit app PIN (created/reset only on the iPhone — never on the Watch).
4. If the registered iPhone is reachable: silent WatchConnectivity challenge/response
   (`CompanionPresenceChecker`). No Face ID, no notification, no button press — this raises trust,
   its absence does not by itself imply fraud.
5. Risk engine (amount, new recipient, recent failures, device state).
6. Explicit "Send ₹X to Y?" confirmation — separate action from just having the app open.
7. PSP/UPI-required authentication (not ours to implement or replace).

## Hard rules

- **Never store or read the user's UPI PIN.** Our PIN is an app-level access gate only.
- **Never store the app PIN as plaintext** — device-bound credential + secure storage only.
- **Bluetooth/WatchConnectivity reachability alone is never "authenticated."** Always perform the
  signed challenge/response; treat absence as a fallback to stricter rules, not a hard failure.
- **PIN setup and reset happen only on the iPhone.** Changing the PIN invalidates existing Watch
  sessions/credentials.
- **Lost Watch** → iPhone "Disable Payments" revokes the Watch's credential server-side — must work
  even if the PIN is known.
- **Idempotency is mandatory** on every payment-initiating request.

## Claims to avoid making publicly

Don't say our app PIN replaces the UPI PIN, that WatchConnectivity presence proves identity, or that
NPCI/RBI have approved P2P Watch payments, unless that has actually happened.
