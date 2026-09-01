import SwiftUI

@main
struct WristPayApp: App {
    @StateObject private var session = AppSession()

    var body: some Scene {
        WindowGroup {
            if session.isUnlocked {
                HomeView()
                    .environmentObject(session)
            } else {
                PinEntryView()
                    .environmentObject(session)
            }
        }
    }
}
