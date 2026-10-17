public import ServiceManagement

// --- [MODELS] --------------------------------------------------------------------------

public struct LoginItem {
    public let status: SMAppService.Status
    public let failure: any Error?

    public static var current: Self {
        Self(status: SMAppService.mainApp.status, failure: nil)
    }

    public var isEnabled: Bool {
        switch status {
            case .enabled, .requiresApproval: true
            case .notRegistered, .notFound: false
            @unknown default: false
        }
    }

    public var issue: String? {
        failure?.localizedDescription ?? (status == .requiresApproval ? "Approval pending in Login Items" : nil)
    }

    public static func setEnabled(_ enabled: Bool) async -> Self {
        let update: Result<Void, any Error> = await Result {
            if enabled {
                try SMAppService.mainApp.register()
            } else {
                try await SMAppService.mainApp.unregister()
            }
        }
        return Self(
            status: SMAppService.mainApp.status,
            failure: update.failure,
        )
    }
}
