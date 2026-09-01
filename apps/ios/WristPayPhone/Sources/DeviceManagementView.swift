import SwiftUI

/// "Lost Watch" kill switch: Devices → Apple Watch → Disable Payments. Must revoke the Watch's
/// credential server-side, not just flip a local flag — a stolen Watch must be denied even if the
/// thief already knows the 6-digit PIN.
struct DeviceManagementView: View {
    @State private var devices: [PairedDevice] = []

    var body: some View {
        List(devices) { device in
            HStack {
                VStack(alignment: .leading) {
                    Text(device.name)
                    Text(device.isPaymentsEnabled ? "Payments enabled" : "Payments disabled")
                        .font(.caption)
                        .foregroundStyle(device.isPaymentsEnabled ? .green : .red)
                }
                Spacer()
                if device.isPaymentsEnabled {
                    Button("Disable Payments", role: .destructive) {
                        disablePayments(for: device)
                    }
                }
            }
        }
        .task { devices = await loadPairedDevices() }
    }

    private func disablePayments(for device: PairedDevice) {
        // TODO: call backend to revoke this device's credential, then refresh the list.
    }

    private func loadPairedDevices() async -> [PairedDevice] {
        // TODO: fetch from account backend.
        []
    }
}

struct PairedDevice: Identifiable {
    let id: String
    let name: String
    let isPaymentsEnabled: Bool
}
