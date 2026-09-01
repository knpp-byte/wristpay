import SwiftUI

struct ConfirmView: View {
    let recipient: Recipient
    let amountInr: Decimal
    @State private var isSubmitting = false
    @State private var navigateToStatus = false
    @State private var result: PaymentResult?

    var body: some View {
        VStack(spacing: 12) {
            Text("Send ₹\(amountInr.description) to")
            Text(recipient.displayName).font(.headline)

            Button(isSubmitting ? "Sending…" : "Confirm") {
                submit()
            }
            .disabled(isSubmitting)
        }
        .padding()
        .navigationDestination(isPresented: $navigateToStatus) {
            if let result {
                PaymentStatusView(result: result)
            }
        }
    }

    private func submit() {
        isSubmitting = true
        Task {
            // Every confirm generates its own idempotency key exactly once, here — never regenerate
            // it on retry, or a network retry could look like a brand-new payment to the backend.
            let idempotencyKey = UUID().uuidString
            do {
                let response = try await WristPayAPIClient.shared.sendPayment(
                    idempotencyKey: idempotencyKey,
                    recipientQuery: recipient.mobileNumber,
                    amountInr: amountInr)
                result = PaymentResult(response: response)
            } catch {
                result = PaymentResult(errorDescription: error.localizedDescription)
            }
            isSubmitting = false
            navigateToStatus = true
        }
    }
}
