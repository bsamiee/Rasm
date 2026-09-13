import ServiceManagement

struct LoginItem {
  let status: SMAppService.Status
  let updateFailed: Bool

  static var current: LoginItem {
    LoginItem(status: SMAppService.mainApp.status, updateFailed: false)
  }

  var isEnabled: Bool {
    switch status {
    case .enabled, .requiresApproval: true
    case .notRegistered, .notFound: false
    @unknown default: false
    }
  }

  var issue: String? {
    switch (status, updateFailed) {
    case (_, true): "Launch at login could not be updated"
    case (.notFound, false): "Launch at login is unavailable"
    case (.enabled, false), (.notRegistered, false), (.requiresApproval, false): nil
    @unknown default: nil
    }
  }

  static func setEnabled(_ enabled: Bool) async -> LoginItem {
    let update: Result<Void, any Error> = await Result {
      if enabled {
        try SMAppService.mainApp.register()
      } else {
        try await SMAppService.mainApp.unregister()
      }
    }
    return LoginItem(status: SMAppService.mainApp.status, updateFailed: (try? update.get()) == nil)
  }
}
