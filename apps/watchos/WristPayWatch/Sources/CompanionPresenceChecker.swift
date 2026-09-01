import Foundation
#if canImport(WatchConnectivity)
import WatchConnectivity
#endif

/// "Companion Device Presence Verification" — a silent, non-interactive challenge/response with the
/// registered iPhone over WatchConnectivity. No Face ID, no notification, no button press on the
/// phone. If the iPhone isn't reachable, the app falls back to Watch-only trust rather than blocking
/// the flow. See /docs/security.md for why this must never be replaced by "Bluetooth connected == trusted".
final class CompanionPresenceChecker: NSObject {
    static let shared = CompanionPresenceChecker()

    private override init() {
        super.init()
        #if canImport(WatchConnectivity)
        // isReachable/sendMessage are meaningless until the session is activated — without this,
        // checkSilently would always fall back to .watchOnly even with the iPhone right there.
        if WCSession.isSupported() {
            let session = WCSession.default
            session.delegate = self
            session.activate()
        }
        #endif
    }

    func checkSilently(completion: @escaping (CompanionTrustLevel) -> Void) {
        #if canImport(WatchConnectivity)
        guard WCSession.isSupported() else {
            completion(.watchOnly)
            return
        }

        Task {
            // Activation (kicked off in init) is asynchronous. Without waiting for it, the very
            // first check right after app launch would see .notActivated and report .watchOnly
            // even when the paired iPhone is reachable. If activation never completes (e.g. no
            // paired iPhone), this times out and performChallenge's own guard falls back safely.
            await waitForActivationIfNeeded()
            performChallenge(completion: completion)
        }
        #else
        completion(.watchOnly)
        #endif
    }

    #if canImport(WatchConnectivity)
    private func waitForActivationIfNeeded() async {
        let deadline = Date().addingTimeInterval(2.0)
        while WCSession.default.activationState != .activated, Date() < deadline {
            try? await Task.sleep(nanoseconds: 100_000_000)
        }
    }

    private func performChallenge(completion: @escaping (CompanionTrustLevel) -> Void) {
        let session = WCSession.default
        guard session.activationState == .activated, session.isReachable else {
            completion(.watchOnly)
            return
        }

        let nonce = UUID().uuidString
        session.sendMessage(["challenge": nonce], replyHandler: { reply in
            // TODO: verify the reply is a valid signed response to `nonce` using the
            // registered device credential, not just "a reply arrived".
            if reply["signedResponse"] != nil {
                completion(.phoneReachablePresenceVerified)
            } else {
                completion(.watchOnly)
            }
        }, errorHandler: { _ in
            completion(.watchOnly)
        })
    }
    #endif
}

#if canImport(WatchConnectivity)
extension CompanionPresenceChecker: WCSessionDelegate {
    // The only delegate method required on watchOS (iOS additionally requires
    // sessionDidBecomeInactive/sessionDidDeactivate, which don't apply to this target).
    func session(
        _ session: WCSession,
        activationDidCompleteWith activationState: WCSessionActivationState,
        error: Error?
    ) {
        // No-op: waitForActivationIfNeeded polls session.activationState directly rather than
        // caching state here.
    }
}
#endif
