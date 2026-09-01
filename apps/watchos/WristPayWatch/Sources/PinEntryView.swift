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
            CompanionPresenceChecker.shared.checkSilently { level in
                session.companionTrustLevel = level
            }
        }
    }
}
