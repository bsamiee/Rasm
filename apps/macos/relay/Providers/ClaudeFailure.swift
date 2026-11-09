import CasePaths
import Files
import Foundation
import Processes
import Results
import Security
import Subprocess

// --- [CONSTANTS] -----------------------------------------------------------------------

nonisolated private let onHoldLead: String = "Your account is on hold"

// --- [ERRORS] --------------------------------------------------------------------------

@CasePathable
nonisolated enum ClaudeFailure: ProviderFailure, LockFailure {
    case executableMissing
    case signInRequired
    case accountOnHold(appeal: URL)
    case process(ProcessFailure)
    case filesystem(any Error)
    case interactionNotAllowed
    case invalidCredentials(any Error?)
    case invalidIdentity(AggregateError<IdentityError>)
    case invalidQuota(AggregateError<QuotaFailure>)
    case usageUnavailable
    case accountChanged
    case protocolFailure((any Error)?)
    case cancelled(CancellationError)
    case lock(FileFailure)
    indirect case cleanup(operation: Self, release: Self)

    init(child failure: ProcessFailure) {
        self =
            switch failure.reason {
                case .subprocess(let error) where error.code == .executableNotFound: .executableMissing
                case .exit(_, let standardError) where standardError.contains(onHoldLead):
                    standardError.firstMatch(of: URL.ParseStrategy()).map { link in .accountOnHold(appeal: link.output) } ?? Self(process: failure)
                default: Self(process: failure)
            }
    }

    var cause: Self {
        switch self {
            case .cleanup(let operation, _): operation.cause
            default: self
        }
    }

    func releasing(_ release: Result<Void, Self>) -> Self {
        switch release {
            case .success: self
            case .failure(let error): .cleanup(operation: self, release: error)
        }
    }

    var requiresSignIn: Bool { cause.is(\.signInRequired) }

    var isAccountMismatch: Bool { cause.is(\.accountChanged) }

    var isCancellation: Bool { cause.is(\.cancelled) }

    var errorDescription: String? {
        switch self {
            case .executableMissing: "Claude Code not installed"
            case .signInRequired: "Sign in required"
            case .accountOnHold(let appeal): "Account on hold, view details or appeal at \(appeal.absoluteString)"
            case .process(let failure): failure.localizedDescription
            case .filesystem(let error): "Claude file access failed. \(error.localizedDescription)"
            case .interactionNotAllowed: SecCopyErrorMessageString(errSecInteractionNotAllowed, nil) as String?
            case .invalidCredentials: "Unreadable Claude credential"
            case .invalidIdentity: "Incomplete Claude account identity"
            case .invalidQuota: "Unreadable Claude usage"
            case .usageUnavailable: "Claude Code could not fetch usage"
            case .accountChanged: "Signed in to different Claude account"
            case .protocolFailure: "Unexpected Claude Code response"
            case .cancelled: "Claude Code was canceled"
            case .lock(let failure) where failure.reason.is(\.held): "Claude credential update in progress, retry after it finishes"
            case .lock(let failure): failure.localizedDescription
            case .cleanup(let operation, _): operation.localizedDescription
        }
    }
}
