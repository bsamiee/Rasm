import Foundation

nonisolated enum CodexTurnErrorCode: String, Sendable {
  case contextWindowExceeded
  case sessionBudgetExceeded
  case usageLimitExceeded
  case rateLimitExceeded
  case serverOverloaded
  case cyberPolicy
  case misalignmentPolicyViolation
  case internalServerError
  case unauthorized
  case badRequest
  case threadRollbackFailed
  case sandboxError
  case other
}

nonisolated enum CodexFailure: ProviderFailure {
  case applicationUnavailable
  case process(ProcessFailure)
  case cancelled
  case timedOut
  case connectionClosed
  case invalidResponse(field: String)
  case requestRejected(code: Int, message: String)
  case signInRequired
  case subscriptionRequired
  case signInPageUnopened
  case signInRefused(reason: String?)
  case identityChanged
  case modelUnavailable
  case turnFailed(code: CodexTurnErrorCode?, message: String)
  case storage(any Error)
  case keyringStorage
  case forcedWorkspace
  case desktopLaunch(any Error)
  case desktopQuitRefused

  var requiresSignIn: Bool {
    switch self {
    case .signInRequired, .subscriptionRequired, .identityChanged: true
    case .turnFailed(.unauthorized, _): true
    case .applicationUnavailable, .process, .cancelled, .timedOut, .connectionClosed,
      .invalidResponse, .requestRejected, .signInPageUnopened, .signInRefused, .modelUnavailable,
      .turnFailed, .storage, .keyringStorage, .forcedWorkspace,
      .desktopLaunch, .desktopQuitRefused:
      false
    }
  }

  var isCancellation: Bool {
    switch self {
    case .cancelled, .process(.cancelled): true
    default: false
    }
  }

  var errorDescription: String? {
    switch self {
    case .applicationUnavailable:
      "Install the OpenAI desktop app to connect an account"
    case .process(let failure): failure.localizedDescription
    case .cancelled: "Canceled"
    case .timedOut: "OpenAI app server did not answer in time"
    case .connectionClosed: "OpenAI connection closed"
    case .invalidResponse(let field): "OpenAI returned an unreadable \(field)"
    case .requestRejected(_, let message): message
    case .signInRequired: "Sign in to this OpenAI account"
    case .subscriptionRequired: "Connect this account with a ChatGPT subscription"
    case .signInPageUnopened: "OpenAI sign-in page could not open"
    case .signInRefused(let reason): reason ?? "OpenAI sign-in did not finish"
    case .identityChanged: "OpenAI is signed in to a different user or workspace"
    case .modelUnavailable:
      "OpenAI model \(CodexProtocol.greetingModelName) is unavailable for this account"
    case .turnFailed(_, let message): message
    case .storage(let error): "Could not save the OpenAI account, \(error.localizedDescription)"
    case .keyringStorage:
      "Codex keeps its sign-in in the Keychain, set cli_auth_credentials_store = \"file\" in ~/.codex/config.toml to switch accounts"
    case .forcedWorkspace:
      "Codex pins a workspace through forced_chatgpt_workspace_id in ~/.codex/config.toml, unset it to switch accounts"
    case .desktopLaunch(let error): "Could not open the OpenAI app, \(error.localizedDescription)"
    case .desktopQuitRefused: "OpenAI app did not accept the quit request"
    }
  }
}

nonisolated extension CodexFailure {
  init(process failure: ProcessFailure) {
    self =
      switch failure {
      case .cancelled: .cancelled
      case .timedOut: .timedOut
      default: .process(failure)
      }
  }
}

nonisolated extension Result where Failure == ProcessFailure {
  func codex() -> Result<Success, CodexFailure> {
    mapError(CodexFailure.init(process:))
  }
}

nonisolated struct CodexFieldFailure: AggregateError {
  let first: String
  let remaining: [String]
}
