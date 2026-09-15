import Foundation
import Subprocess

nonisolated enum ClaudeFailure: ProviderFailure {
  case executableMissing
  case signInRequired
  case authenticationFailed(TerminationStatus)
  case process(ProcessFailure)
  case keychainDenied
  case keychainLocked
  case filesystem(any Error)
  case invalidCredentials
  case invalidIdentity(IdentityFailure)
  case invalidResponse
  case invalidQuota(ClaudeQuotaErrors)
  case http(Int)
  case rateLimited(until: Date?)
  case transport(any Error)
  case accountChanged
  case modelUnavailable
  case requestFailed
  case protocolFailure
  case cancelled
  case timedOut
  case lockHeld(URL)
  case lockCompromised(URL)
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
    if case .http(401) = cause { true } else { false }
  }

  var requiresSignIn: Bool {
    if case .signInRequired = cause { true } else { false }
  }

  var isCancellation: Bool {
    switch cause {
    case .cancelled, .process(.cancelled): true
    default: false
    }
  }

  var errorDescription: String? {
    switch self {
    case .executableMissing: "Install Claude Code to connect an account"
    case .signInRequired: "Sign in to this Claude account again"
    case .http(401): "Claude refused the saved sign-in token"
    case .authenticationFailed: "Claude could not complete sign-in"
    case .process(let failure): failure.localizedDescription
    case .keychainDenied: "macOS refused Relay’s Keychain request for this Claude sign-in"
    case .keychainLocked: "Unlock the login keychain to read this Claude sign-in"
    case .filesystem: "Relay could not read or save the Claude account"
    case .invalidCredentials: "Claude’s saved sign-in is incomplete"
    case .invalidIdentity: "Claude did not return a complete account identity"
    case .invalidResponse, .invalidQuota: "Claude returned usage Relay could not interpret"
    case .rateLimited(.some(let until)):
      "Claude is limiting usage requests until \(UsagePresentation.weekday(until))"
    case .rateLimited(.none): "Claude is limiting usage requests"
    case .http: "Claude could not provide current usage"
    case .transport: "Could not reach Claude"
    case .accountChanged: "Claude sign-in changed during this operation"
    case .modelUnavailable: "Account has no enabled Haiku model"
    case .requestFailed: "Claude did not complete the session request"
    case .protocolFailure: "Claude Code returned an unsupported response"
    case .cancelled: "Canceled"
    case .timedOut: "Claude Code did not answer in time"
    case .lockHeld: "Claude is updating this sign-in, try again when it finishes"
    case .lockCompromised: "Claude’s sign-in changed while Relay was updating it"
    case .cleanup(let operation, _): operation.localizedDescription
    }
  }
}

nonisolated extension ClaudeFailure {
  init(process failure: ProcessFailure) {
    self =
      switch failure {
      case .cancelled: .cancelled
      case .timedOut: .timedOut
      default: .process(failure)
      }
  }
}

nonisolated extension Result where Failure == KeychainFailure {
  func claude() -> Result<Success, ClaudeFailure> {
    mapError { failure in
      switch failure {
      case .denied: .keychainDenied
      case .interactionNotAllowed: .keychainLocked
      case .process(let error): ClaudeFailure(process: error)
      }
    }
  }
}

nonisolated extension Result where Failure == ProcessFailure {
  func claude() -> Result<Success, ClaudeFailure> {
    mapError(ClaudeFailure.init(process:))
  }
}

nonisolated struct ClaudeQuotaErrors: AggregateError {
  let first: QuotaFailure
  let remaining: [QuotaFailure]
}
