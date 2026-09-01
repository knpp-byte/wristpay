import SwiftUI

/// Step in account setup: create the 6-digit app PIN. This PIN gates the Watch payment app — it is
/// NOT the user's UPI PIN, and must never be compared against or stored alongside it.
struct PinSetupView: View {
    @State private var pin = ""
    @State private var confirmPin = ""
    @State private var errorMessage: String?
    var onPinCreated: (String) -> Void

    var body: some View {
        VStack(spacing: 16) {
            Text("Create your WristPay PIN")
                .font(.headline)
            Text("6 digits. This is separate from your UPI PIN — never reuse it.")
                .font(.caption)
                .foregroundStyle(.secondary)

            SecureField("6-digit PIN", text: $pin)
                .keyboardType(.numberPad)
            SecureField("Confirm PIN", text: $confirmPin)
                .keyboardType(.numberPad)

            if let errorMessage {
                Text(errorMessage).font(.caption2).foregroundStyle(.red)
            }

            Button("Continue") {
                submit()
            }
            .disabled(pin.count != 6)
        }
        .padding()
    }

    private func submit() {
        guard pin.count == 6, pin.allSatisfy(\.isNumber) else {
            errorMessage = "PIN must be exactly 6 digits."
            return
        }
        guard pin == confirmPin else {
            errorMessage = "PINs don't match."
            return
        }
        // TODO: send to PinCredentialProvisioning, never keep `pin` around after this call.
        onPinCreated(pin)
    }
}

/// TODO: real implementation provisions a device-bound credential (not the raw PIN) to the backend
/// and to the paired Watch via WatchConnectivity, using Keychain for local material.
enum PinCredentialProvisioning {
    static func createCredential(fromPin pin: String) async throws {
        fatalError("Not implemented — wire up to account backend + Keychain before use.")
    }
}
