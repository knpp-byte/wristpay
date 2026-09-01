import Foundation

struct Recipient: Codable {
    let displayName: String
    let mobileNumber: String
    let upiId: String
}

struct SendPaymentResponse: Codable {
    let id: String
    let status: String
    let amountInr: Decimal
    let recipientDisplayName: String?
    let providerReference: String?
    let failureReason: String?
}

/// Talks to WristPay.Api. Base URL points at the Mock-PSP-backed prototype backend today —
/// see backend/src/WristPay.Api. Swap this for a properly authenticated client (device-bound
/// tokens, TLS pinning) before this ever touches a real PSP integration.
final class WristPayAPIClient {
    static let shared = WristPayAPIClient()

    // TODO: load from build configuration instead of hardcoding.
    private let baseURL = URL(string: "http://localhost:5199")!

    func lookupRecipient(query: String) async throws -> Recipient {
        var components = URLComponents(url: baseURL.appendingPathComponent("api/recipients/lookup"),
                                        resolvingAgainstBaseURL: false)!
        components.queryItems = [URLQueryItem(name: "query", value: query)]

        let (data, _) = try await URLSession.shared.data(from: components.url!)
        return try JSONDecoder().decode(Recipient.self, from: data)
    }

    func sendPayment(idempotencyKey: String, recipientQuery: String, amountInr: Decimal) async throws -> SendPaymentResponse {
        var request = URLRequest(url: baseURL.appendingPathComponent("api/payments"))
        request.httpMethod = "POST"
        request.setValue("application/json", forHTTPHeaderField: "Content-Type")

        // TODO: senderMobileNumber should come from the signed-in account, not be hardcoded.
        let body: [String: Any] = [
            "idempotencyKey": idempotencyKey,
            "senderMobileNumber": "unknown",
            "recipientQuery": recipientQuery,
            "amountInr": NSDecimalNumber(decimal: amountInr).doubleValue,
        ]
        request.httpBody = try JSONSerialization.data(withJSONObject: body)

        let (data, _) = try await URLSession.shared.data(for: request)
        return try JSONDecoder().decode(SendPaymentResponse.self, from: data)
    }
}
