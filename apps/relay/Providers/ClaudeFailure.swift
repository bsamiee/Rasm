import Foundation
import Security

enum ClaudeFailure: Error {
  case executableMissing
  case signInRequired
  case authenticationFailed(Int32)
  case native(ProcessFailure)
  case keychain(OSStatus)
  case filesystem(any Error)
  case systemCall(operation: String, code: Int32)
  case invalidCredentials
  case invalidIdentity(IdentityFailure)
  case invalidSavedAccounts(ClaudeAccountIssues)
  case invalidResponse
  case invalidQuota(ClaudeQuotaIssues)
  case http(Int)
  case transport(any Error)
  case accountChanged
  case accountUnavailable
  case modelUnavailable
  case sessionUnknown
  case requestFailed
  case protocolFailure
  case operationInProgress
  case cancelled
  case leaseBusy(URL)
  case leaseCompromised(URL)
  case unfinishedSelection
  indirect case sessionConfirmationPending(ClaudeFailure)
  indirect case cleanup(operation: ClaudeFailure, release: ClaudeFailure)

  /// The operation failure beneath any release failures recorded after it.
  var cause: ClaudeFailure {
    switch self {
    case .cleanup(let operation, _): operation.cause
    default: self
    }
  }

  /// This failure, carrying the release failure that followed it.
  func releasing(_ release: Result<Void, ClaudeFailure>) -> ClaudeFailure {
    switch release {
    case .success: self
    case .failure(let error): .cleanup(operation: self, release: error)
    }
  }

  var requiresSignIn: Bool {
    switch self {
    case .signInRequired, .accountUnavailable:
      true
    case .http(401):
      true
    case .cleanup(let operation, _):
      operation.requiresSignIn
    case .sessionConfirmationPending(let cause):
      cause.requiresSignIn
    case .executableMissing, .authenticationFailed, .native, .keychain, .filesystem, .systemCall,
      .invalidCredentials, .invalidIdentity, .invalidSavedAccounts, .invalidResponse, .invalidQuota,
      .http, .transport, .accountChanged, .modelUnavailable, .sessionUnknown,
      .requestFailed, .protocolFailure, .operationInProgress, .cancelled,
      .leaseBusy, .leaseCompromised, .unfinishedSelection:
      false
    }
  }

  var userMessage: String {
    switch self {
    case .executableMissing: "Install Claude Code to connect an account."
    case .signInRequired, .accountUnavailable: "Sign in to this Claude account again."
    case .authenticationFailed: "Claude could not complete sign-in."
    case .native(let failure): failure.userMessage
    case .keychain: "Allow Relay to access the Claude sign-in in Keychain."
    case .filesystem: "Relay could not read or save the Claude account."
    case .systemCall: "Relay could not access Claude’s account storage."
    case .invalidCredentials: "Claude’s saved sign-in is incomplete."
    case .invalidIdentity: "Claude did not return a complete account identity."
    case .invalidSavedAccounts: "Relay could not recover the saved Claude account identities."
    case .invalidResponse, .invalidQuota: "Claude returned usage Relay could not interpret."
    case .http(401): "Sign in to this Claude account again."
    case .http(429): "Claude is limiting usage requests."
    case .http: "Claude could not provide current usage."
    case .transport: "Could not reach Claude."
    case .accountChanged: "The Claude sign-in changed during this operation."
    case .modelUnavailable: "No eligible Haiku model is available for this account."
    case .sessionUnknown: "Refresh Claude usage before starting a session."
    case .requestFailed: "Claude did not complete the session request."
    case .protocolFailure: "This Claude Code response is not supported."
    case .operationInProgress: "Wait for the current Claude account operation."
    case .cancelled: "Cancelled."
    case .leaseBusy: "Claude is updating this sign-in. Try again when it finishes."
    case .leaseCompromised: "Claude’s sign-in changed while Relay was updating it."
    case .unfinishedSelection: "Finish recovering the previous Claude account switch."
    case .sessionConfirmationPending: "Waiting for Claude to confirm the session window."
    case .cleanup(let operation, _): operation.userMessage
    }
  }

  var awaitingSessionConfirmation: Bool {
    switch self {
    case .sessionConfirmationPending: true
    case .cleanup(let operation, let release):
      operation.awaitingSessionConfirmation || release.awaitingSessionConfirmation
    case .executableMissing, .signInRequired, .authenticationFailed, .native, .keychain,
      .filesystem,
      .systemCall, .invalidCredentials, .invalidIdentity, .invalidSavedAccounts, .invalidResponse,
      .invalidQuota, .http, .transport, .accountChanged, .accountUnavailable, .modelUnavailable,
      .sessionUnknown, .requestFailed, .protocolFailure, .operationInProgress, .cancelled,
      .leaseBusy, .leaseCompromised, .unfinishedSelection:
      false
    }
  }
}

extension Result where Failure == ClaudeFailure {
  /// Binds an asynchronous operation that consumes this result's value; a failure skips it.
  /// Named apart from `flatMap` because an async overload of that name would capture every
  /// synchronous `flatMap` call made from an async context.
  func bind<Next>(
    _ next: (Success) async -> Result<Next, ClaudeFailure>
  ) async -> Result<Next, ClaudeFailure> {
    switch self {
    case .success(let value): await next(value)
    case .failure(let error): .failure(error)
    }
  }
}

struct ClaudeAccountIssues: IssueAggregate {
  let first: IdentityFailure
  let remaining: [IdentityFailure]
}

struct ClaudeQuotaIssues: IssueAggregate {
  let first: QuotaFailure
  let remaining: [QuotaFailure]
}
