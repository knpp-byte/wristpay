import SwiftUI

struct HomeView: View {
    @EnvironmentObject private var session: AppSession
    @State private var recipientQuery = ""
    @State private var navigateToAmount = false

    var body: some View {
        NavigationStack {
            List {
                Section("Send Money") {
                    // TODO: replace with real favourites/contacts once account setup exists.
                    ForEach(["Mom", "Ravi", "Arun"], id: \.self) { name in
                        Button(name) {
                            recipientQuery = name
                            navigateToAmount = true
                        }
                    }
                }

                Section {
                    TextField("Mobile number or UPI ID", text: $recipientQuery)
                    Button("Continue") { navigateToAmount = true }
                        .disabled(recipientQuery.isEmpty)
                }
            }
            .navigationTitle("WristPay")
            .navigationDestination(isPresented: $navigateToAmount) {
                RecipientLookupView(query: recipientQuery)
            }
        }
    }
}
