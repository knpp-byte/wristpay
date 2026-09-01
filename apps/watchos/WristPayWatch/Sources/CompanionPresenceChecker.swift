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

    func checkSilently(completion: @escaping (CompanionTrustLevel) -> Void) {
        #if canImport(WatchConnectivity)
        guard WCSession.isSupported() else {
            completion(.watchOnly)
            return
        }
        let session = WCSession.default
        guard session.isReachable else {
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
        #else
        completion(.watchOnly)
        #endif
    }
}
