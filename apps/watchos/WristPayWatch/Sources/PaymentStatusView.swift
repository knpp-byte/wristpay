import SwiftUI

struct PaymentStatusView: View {
    let result: PaymentResult

    var body: some View {
        VStack(spacing: 8) {
            Image(systemName: result.succeeded ? "checkmark.circle.fill" : "xmark.circle.fill")
                .foregroundStyle(result.succeeded ? .green : .red)
                .font(.largeTitle)

            Text(result.headline)
                .font(.headline)
                .multilineTextAlignment(.center)

            if let detail = result.detail {
                Text(detail)
                    .font(.caption2)
                    .foregroundStyle(.secondary)
                    .multilineTextAlignment(.center)
            }
        }
        .padding()
    }
}

struct PaymentResult {
    let succeeded: Bool
    let headline: String
    let detail: String?

    init(response: SendPaymentResponse) {
        succeeded = response.status == "Succeeded"
        headline = succeeded ? "₹\(response.amountInr) sent" : "Payment \(response.status)"
        detail = response.failureReason
    }

    init(errorDescription: String) {
        succeeded = false
        headline = "Couldn't reach WristPay"
        detail = errorDescription
    }
}
