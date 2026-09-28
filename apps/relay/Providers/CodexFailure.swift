import Foundation

// --- [ERRORS] --------------------------------------------------------------------------

nonisolated enum CodexFailure: DeadlineFailure, ProviderFailure {
    case applicationUnavailable
    case process(ProcessFailure)
    case cancelled
    case timedOut
    case connectionClosed
    case invalidResponse(field: String)
    case requestRejected(method: CodexProtocol.Method, code: Int, message: String, serverVersion: String?)
    case signInRequired
    case subscriptionRequired
    case signInPageUnopened
    case signInRefused(reason: String?)
    case identityChanged
    case modelUnavailable
    case turnUnauthorized(message: String)
    case turnFailed(message: String)
    case storage(any Error)
    case keyringStorage
    case forcedWorkspace
    case desktopLaunch(any Error)
    case desktopQuitRefused

    var requiresSignIn: Bool {
        switch self {
            case .signInRequired, .subscriptionRequired, .identityChanged, .turnUnauthorized: true
            case .applicationUnavailable, .process, .cancelled, .timedOut, .connectionClosed,
                .invalidResponse, .requestRejected, .signInPageUnopened, .signInRefused, .modelUnavailable,
                .turnFailed, .storage, .keyringStorage, .forcedWorkspace,
                .desktopLaunch, .desktopQuitRefused:
                false
        }
    }

    var isAccountMismatch: Bool {
        if case .identityChanged = self { true } else { false }
    }

    var isCancellation: Bool {
        switch self {
            case .cancelled, .process(.cancelled): true
            default: false
        }
    }

    var retryAfter: Date? { nil }

    var errorDescription: String? {
        switch self {
            case .applicationUnavailable: "Codex app not installed"
            case .process(let failure): failure.localizedDescription
            case .cancelled: "Canceled"
            case .timedOut: "Codex app server timed out"
            case .connectionClosed: "Codex app server connection closed"
            case .invalidResponse(let field): "Unreadable Codex \(field)"
            case .requestRejected(let method, let code, let message, .some(let serverVersion)):
                "Codex app server \(serverVersion) rejected \(method.rawValue) with \(code): \(message)"
            case .requestRejected(let method, let code, let message, .none):
                "Codex app server rejected \(method.rawValue) with \(code): \(message)"
            case .signInRequired: "Sign in required"
            case .subscriptionRequired: "ChatGPT sign-in required"
            case .signInPageUnopened: "Could not open the OpenAI sign-in page"
            case .signInRefused(.some(let reason)): "OpenAI sign-in failed: \(reason)"
            case .signInRefused(.none): "OpenAI sign-in failed"
            case .identityChanged: "Signed in to a different OpenAI user or workspace"
            case .modelUnavailable: "\(CodexProtocol.greetingModelName) model unavailable"
            case .turnUnauthorized(let message): "OpenAI greeting unauthorized: \(message)"
            case .turnFailed(let message): "OpenAI greeting failed: \(message)"
            case .storage(let error): "Could not access Codex files: \(error.localizedDescription)"
            case .keyringStorage: "Set cli_auth_credentials_store = \"file\" in config.toml to switch accounts"
            case .forcedWorkspace: "Unset forced_chatgpt_workspace_id in config.toml to switch accounts"
            case .desktopLaunch(let error): "Could not open the Codex app: \(error.localizedDescription)"
            case .desktopQuitRefused: "Codex app refused to quit"
        }
    }
}
