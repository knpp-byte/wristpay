import SwiftUI

struct AmountView: View {
    let recipient: Recipient
    @State private var amountText = ""

    var body: some View {
        VStack(spacing: 12) {
            Text("Send to \(recipient.displayName)")
                .font(.headline)

            TextField("₹ Amount", text: $amountText)

            NavigationLink("Continue") {
                ConfirmView(recipient: recipient, amountInr: Decimal(string: amountText) ?? 0)
            }
            .disabled(Decimal(string: amountText) == nil || amountText.isEmpty)
        }
        .padding()
    }
}
