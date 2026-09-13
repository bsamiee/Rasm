import ServiceManagement

enum LoginItemRegistration: Equatable {
  case enabled
  case disabled
  case requiresApproval
  case unavailable
}

struct LoginItemState: Equatable {
  let registration: LoginItemRegistration
  let issue: String?

  var isEnabled: Bool {
    switch registration {
    case .enabled, .requiresApproval: true
    case .disabled, .unavailable: false
    }
  }
}

@MainActor
enum LoginItem {
  static func state(issue: String? = nil) -> LoginItemState {
    let registration: LoginItemRegistration =
      switch SMAppService.mainApp.status {
      case .enabled: .enabled
      case .notRegistered: .disabled
      case .requiresApproval: .requiresApproval
      case .notFound: .unavailable
      @unknown default: .unavailable
      }
    return LoginItemState(registration: registration, issue: issue)
  }

  static func setEnabled(_ enabled: Bool) async -> LoginItemState {
    do {
      if enabled {
        try SMAppService.mainApp.register()
      } else {
        try await SMAppService.mainApp.unregister()
      }
      return state()
    } catch {
      return state(issue: "Launch at login could not be updated")
    }
  }
}
