import ServiceManagement

struct LoginItem {
  let status: SMAppService.Status
  let failure: String?

  static var current: LoginItem {
    LoginItem(status: SMAppService.mainApp.status, failure: nil)
  }

  var isEnabled: Bool {
    switch status {
    case .enabled, .requiresApproval: true
    case .notRegistered, .notFound: false
    @unknown default: false
    }
  }

  var requiresApproval: Bool { status == .requiresApproval }

  var issue: String? {
    failure ?? (requiresApproval ? "Approval pending in Login Items" : nil)
  }

  static func setEnabled(_ enabled: Bool) async -> LoginItem {
    let update: Result<Void, any Error> = await Result {
      if enabled {
        try SMAppService.mainApp.register()
      } else {
        try await SMAppService.mainApp.unregister()
      }
    }
    return LoginItem(
      status: SMAppService.mainApp.status, failure: update.failure?.localizedDescription)
  }
}
