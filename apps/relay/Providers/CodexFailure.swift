import Foundation

nonisolated enum CodexAccountAction: Sendable {
  case signOut
  case remove
  case reconnect
}

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

nonisolated enum CodexFailure: LocalizedError, Sendable {
  case applicationUnavailable
  case process(ProcessFailure)
  case cancelled
  case connectionClosed
  case invalidResponse(field: String)
  case requestRejected(code: Int, message: String)
  case signInRequired
  case subscriptionRequired
  case signInPageUnopened
  case signInRefused(reason: String?)
  case identityChanged
  case modelUnavailable
  case includedUsageBlocked
  case turnFailed(code: CodexTurnErrorCode?, message: String)
  indirect case sessionConfirmationPending(CodexFailure)
  case storage(any Error)
  case desktopLaunch(any Error)
  case desktopUnavailable
  case desktopMustClose(CodexAccountAction)

  var requiresSignIn: Bool {
    switch self {
    case .signInRequired, .subscriptionRequired, .identityChanged: true
    case .turnFailed(.unauthorized, _): true
    case .sessionConfirmationPending(let failure): failure.requiresSignIn
    case .applicationUnavailable, .process, .cancelled, .connectionClosed, .invalidResponse,
      .requestRejected, .signInPageUnopened, .signInRefused, .modelUnavailable,
      .includedUsageBlocked, .turnFailed, .storage, .desktopLaunch, .desktopUnavailable,
      .desktopMustClose:
      false
    }
  }

  var awaitingSessionConfirmation: Bool {
    if case .sessionConfirmationPending = self { true } else { false }
  }

  var errorDescription: String? {
    switch self {
    case .applicationUnavailable:
      "Install the OpenAI desktop app to connect an account."
    case .process(let failure): failure.localizedDescription
    case .cancelled: "Canceled."
    case .connectionClosed: "The OpenAI connection closed."
    case .invalidResponse(let field): "OpenAI returned an unreadable \(field)."
    case .requestRejected(_, let message): message
    case .signInRequired: "Sign in to this OpenAI account."
    case .subscriptionRequired: "Connect this account with a ChatGPT subscription."
    case .signInPageUnopened: "The OpenAI sign-in page could not open."
    case .signInRefused(let reason): reason ?? "OpenAI sign-in did not finish."
    case .identityChanged: "OpenAI is signed in to a different user or workspace."
    case .modelUnavailable:
      "The lowest-cost supported OpenAI model is unavailable for this account."
    case .includedUsageBlocked: "OpenAI reports that this account’s included usage is unavailable."
    case .turnFailed(_, let message): message
    case .sessionConfirmationPending(let failure):
      "Session start is awaiting confirmation. \(failure.localizedDescription)"
    case .storage(let error): "Could not save the OpenAI account: \(error.localizedDescription)"
    case .desktopLaunch(let error): "Could not open the OpenAI app: \(error.localizedDescription)"
    case .desktopUnavailable: "The OpenAI account window is no longer open."
    case .desktopMustClose(.signOut): "Quit this account’s Codex app before signing out."
    case .desktopMustClose(.remove): "Quit this account’s Codex app before removing it."
    case .desktopMustClose(.reconnect): "Quit this account’s Codex app before signing in again."
    }
  }
}

nonisolated struct CodexFieldFailure: AggregateError {
  let first: String
  let remaining: [String]
}
