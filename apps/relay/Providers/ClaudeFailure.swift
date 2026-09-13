import Foundation

nonisolated enum ClaudeSystemCall: Sendable {
  case mkdir
  case rmdir
  case lstat
  case procPIDInfo
}

nonisolated enum ClaudeFailure: LocalizedError {
  case executableMissing
  case signInRequired
  case authenticationFailed(Int32)
  case process(ProcessFailure)
  case keychain(OSStatus)
  case filesystem(any Error)
  case systemCall(ClaudeSystemCall, Int32)
  case invalidCredentials
  case invalidIdentity(IdentityFailure)
  case invalidSavedAccounts(ClaudeAccountErrors)
  case invalidResponse
  case invalidQuota(ClaudeQuotaErrors)
  case http(Int)
  case transport(any Error)
  case accountChanged
  case modelUnavailable
  case sessionUnknown
  case requestFailed
  case protocolFailure
  case operationInProgress
  case cancelled
  case lockHeld(URL)
  case lockCompromised(URL)
  case unfinishedSelection
  indirect case sessionConfirmationPending(ClaudeFailure)
  indirect case cleanup(operation: ClaudeFailure, release: ClaudeFailure)

  var cause: ClaudeFailure {
    switch self {
    case .cleanup(let operation, _): operation.cause
    default: self
    }
  }

  func releasing(_ release: Result<Void, ClaudeFailure>) -> ClaudeFailure {
    switch release {
    case .success: self
    case .failure(let error): .cleanup(operation: self, release: error)
    }
  }

  var unauthorized: Bool {
    if case .http(401) = self { true } else { false }
  }

  var requiresSignIn: Bool {
    switch self {
    case .signInRequired: true
    case .cleanup(let operation, _): operation.requiresSignIn
    case .sessionConfirmationPending(let cause): cause.requiresSignIn
    case .executableMissing, .authenticationFailed, .process, .keychain, .filesystem, .systemCall,
      .invalidCredentials, .invalidIdentity, .invalidSavedAccounts, .invalidResponse, .invalidQuota,
      .http, .transport, .accountChanged, .modelUnavailable, .sessionUnknown, .requestFailed,
      .protocolFailure, .operationInProgress, .cancelled, .lockHeld, .lockCompromised,
      .unfinishedSelection:
      false
    }
  }

  var errorDescription: String? {
    switch self {
    case .executableMissing: "Install Claude Code to connect an account."
    case .signInRequired: "Sign in to this Claude account again."
    case .http(401): "Claude refused the saved sign-in token."
    case .authenticationFailed: "Claude could not complete sign-in."
    case .process(let failure): failure.localizedDescription
    case .keychain: "Allow Relay to access the Claude sign-in in Keychain."
    case .filesystem: "Relay could not read or save the Claude account."
    case .systemCall: "Relay could not access Claude’s account storage."
    case .invalidCredentials: "Claude’s saved sign-in is incomplete."
    case .invalidIdentity: "Claude did not return a complete account identity."
    case .invalidSavedAccounts: "Relay could not recover the saved Claude account identities."
    case .invalidResponse, .invalidQuota: "Claude returned usage Relay could not interpret."
    case .http(429): "Claude is limiting usage requests."
    case .http: "Claude could not provide current usage."
    case .transport: "Could not reach Claude."
    case .accountChanged: "The Claude sign-in changed during this operation."
    case .modelUnavailable: "No eligible Haiku model is available for this account."
    case .sessionUnknown: "Refresh Claude usage before starting a session."
    case .requestFailed: "Claude did not complete the session request."
    case .protocolFailure: "This Claude Code response is not supported."
    case .operationInProgress: "Wait for the current Claude account operation."
    case .cancelled: "Canceled."
    case .lockHeld: "Claude is updating this sign-in. Try again when it finishes."
    case .lockCompromised: "Claude’s sign-in changed while Relay was updating it."
    case .unfinishedSelection: "Finish recovering the previous Claude account switch."
    case .sessionConfirmationPending: "Waiting for Claude to confirm the session window."
    case .cleanup(let operation, _): operation.localizedDescription
    }
  }

  var awaitingSessionConfirmation: Bool {
    switch self {
    case .sessionConfirmationPending: true
    case .cleanup(let operation, let release):
      operation.awaitingSessionConfirmation || release.awaitingSessionConfirmation
    case .executableMissing, .signInRequired, .authenticationFailed, .process, .keychain,
      .filesystem, .systemCall, .invalidCredentials, .invalidIdentity, .invalidSavedAccounts,
      .invalidResponse, .invalidQuota, .http, .transport, .accountChanged, .modelUnavailable,
      .sessionUnknown, .requestFailed, .protocolFailure, .operationInProgress, .cancelled,
      .lockHeld, .lockCompromised, .unfinishedSelection:
      false
    }
  }
}

nonisolated extension Result<ProcessTermination, ProcessFailure> {
  func exited(
    _ failure: (Int32) -> ClaudeFailure = { status in .process(.exit(status)) }
  ) -> Result<Void, ClaudeFailure> {
    mapError(ClaudeFailure.process).flatMap { termination in
      termination.status == 0 ? .success(()) : .failure(failure(termination.status))
    }
  }
}

nonisolated struct ClaudeAccountErrors: AggregateError {
  let first: IdentityFailure
  let remaining: [IdentityFailure]
}

nonisolated struct ClaudeQuotaErrors: AggregateError {
  let first: QuotaFailure
  let remaining: [QuotaFailure]
}
