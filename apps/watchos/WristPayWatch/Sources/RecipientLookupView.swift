import SwiftUI

struct RecipientLookupView: View {
    let query: String
    @State private var recipient: Recipient?
    @State private var isLoading = true
    @State private var notFound = false

    var body: some View {
        Group {
            if isLoading {
                ProgressView("Looking up recipient…")
            } else if let recipient {
                VStack(spacing: 12) {
                    RecipientCard(recipient: recipient)
                    NavigationLink("Continue") {
                        AmountView(recipient: recipient)
                    }
                }
            } else {
                Text("Couldn't find a UPI-linked account for \(query)")
                    .font(.caption)
                    .multilineTextAlignment(.center)
                    .padding()
            }
        }
        .task {
            recipient = try? await WristPayAPIClient.shared.lookupRecipient(query: query)
            isLoading = false
        }
    }
}

private struct RecipientCard: View {
    let recipient: Recipient
    var body: some View {
        VStack(spacing: 4) {
            Text(recipient.displayName).font(.headline)
            Text(recipient.mobileNumber).font(.caption2).foregroundStyle(.secondary)
        }
    }
}
