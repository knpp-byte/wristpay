import Foundation

/// Watch-side session state. The app PIN itself is created/reset only on the iPhone
/// (see apps/ios) — this only verifies a PIN attempt against the credential synced from there.
@MainActor
final class AppSession: ObservableObject {
    @Published var isUnlocked = false
    @Published var failedPinAttempts = 0
    @Published var companionTrustLevel: CompanionTrustLevel = .unknown

    private let maxAttemptsBeforeLockout = 5

    /// TODO: replace with a real check against the device-bound credential synced from the iPhone
    /// during account setup. Never compare against a plaintext PIN stored on the Watch.
    func verifyPin(_ pin: String) -> Bool {
        let isCorrect = PinCredentialStore.verify(pin)
        if isCorrect {
            failedPinAttempts = 0
            isUnlocked = true
        } else {
            failedPinAttempts += 1
        }
        return isCorrect
    }

    var isLockedOut: Bool { failedPinAttempts >= maxAttemptsBeforeLockout }

    func lock() {
        isUnlocked = false
    }
}

enum CompanionTrustLevel {
    case unknown
    case phoneReachablePresenceVerified
    case watchOnly
}

/// Placeholder for the real Secure Enclave / Keychain-backed verification against the
/// credential the iPhone provisioned on this Watch during pairing.
enum PinCredentialStore {
    static func verify(_ pin: String) -> Bool {
        // TODO: real implementation — never true unconditionally.
        pin.count == 6
    }
}
