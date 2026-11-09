import CasePaths
import Foundation
import JSONRPC
import Processes
import Results

// --- [ERRORS] --------------------------------------------------------------------------

@CasePathable
nonisolated enum CodexFailure: ProviderFailure {
    case executableMissing
    case process(ProcessFailure)
    case timedOut(Duration)
    case cancelled(CancellationError)
    case rpc(JSONRPCFailure)
    case invalidResponse(CodexProtocol.Method)
    case invalidAuthFile(URL, any Error?)
    case invalidIdentity(AggregateError<IdentityError>)
    case invalidQuota(AggregateError<QuotaFailure>)
    case signInRequired
    case subscriptionRequired
    case signInPageUnopened
    case signInRefused(reason: String?)
    case identityChanged
    case storage(any Error)
    case keyringStorage
    case forcedWorkspace
    case desktopRunning

    var requiresSignIn: Bool {
        switch self {
            case .signInRequired, .subscriptionRequired, .identityChanged: true
            default: false
        }
    }

    var isAccountMismatch: Bool { self[case: \.identityChanged] != nil }

    var isCancellation: Bool { self[case: \.cancelled] != nil }

    var errorDescription: String? {
        switch self {
            case .executableMissing: "Codex not installed"
            case .process(let failure): failure.localizedDescription
            case .timedOut(let timeout): "Codex app server timed out after \(timeout.formatted(.units(width: .wide)))"
            case .cancelled: "Codex app server was canceled"
            case .rpc(let failure): "Codex app server failed: \(failure.localizedDescription)"
            case .invalidResponse(let method): "Unreadable Codex \(method.rawValue) response"
            case .invalidAuthFile(let url, _): "Unreadable Codex \(url.lastPathComponent)"
            case .invalidIdentity: "Incomplete OpenAI account identity"
            case .invalidQuota: "Unreadable Codex usage"
            case .signInRequired: "Sign in required"
            case .subscriptionRequired: "ChatGPT sign-in required"
            case .signInPageUnopened: "Could not open OpenAI sign-in page"
            case .signInRefused(.some(let reason)): "OpenAI sign-in failed. \(reason)"
            case .signInRefused(.none): "OpenAI sign-in failed"
            case .identityChanged: "Signed in to different OpenAI user or workspace"
            case .storage(let error): "Codex file access failed: \(error.localizedDescription)"
            case .keyringStorage: "Set cli_auth_credentials_store = \"file\" to switch accounts"
            case .forcedWorkspace: "Unset forced_chatgpt_workspace_id to switch accounts"
            case .desktopRunning: "Quit Codex to switch accounts"
        }
    }
}
