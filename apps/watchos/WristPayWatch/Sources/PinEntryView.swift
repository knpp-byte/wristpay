import SwiftUI

struct PinEntryView: View {
    @EnvironmentObject private var session: AppSession
    @State private var pin = ""
    @State private var showError = false

    var body: some View {
        VStack(spacing: 8) {
            Text("Enter PIN")
                .font(.headline)

            SecureField("", text: $pin)
                .frame(maxWidth: 100)
                .multilineTextAlignment(.center)
                .onChange(of: pin) { _, newValue in
                    if newValue.count == 6 { attemptUnlock() }
                }

            if showError {
                Text(session.isLockedOut
                     ? "Too many attempts. Reactivate from iPhone."
                     : "Incorrect PIN")
                    .font(.caption2)
                    .foregroundStyle(.red)
            }
        }
        .padding()
        .disabled(session.isLockedOut)
    }

    private func attemptUnlock() {
        let ok = session.verifyPin(pin)
        showError = !ok
        pin = ""
        if ok {
            // checkSilently's completion can run off the main actor (it's driven by
            // WCSession's reply/error handlers) — session is @MainActor, so hop back before touching it.
            CompanionPresenceChecker.shared.checkSilently { level in
                Task { @MainActor in
                    session.companionTrustLevel = level
                }
            }
        }
    }
}
