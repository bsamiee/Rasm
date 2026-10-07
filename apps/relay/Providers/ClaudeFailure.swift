import Foundation
import Subprocess

// --- [TYPES] ---------------------------------------------------------------------------

nonisolated enum ClaudeEndpoint: String, Sendable {
    case profile = "https://api.anthropic.com/api/oauth/profile"
    case usage = "https://api.anthropic.com/api/oauth/usage"
    case token = "https://platform.claude.com/v1/oauth/token"

    var timeout: TimeInterval { self == .token ? 30 : 15 }

    static var decoder: JSONDecoder {
        let decoder: JSONDecoder = JSONDecoder()
        decoder.keyDecodingStrategy = .convertFromSnakeCase
        return decoder
    }
}

// --- [MODELS] --------------------------------------------------------------------------

nonisolated struct ClaudeAPIError: Decodable, Sendable {
    let type: String
    let message: String
}

nonisolated struct ClaudeErrorBody: Decodable, Sendable {
    enum Field: Decodable, Sendable {
        case code(String)
        case detail(ClaudeAPIError)

        init(from decoder: any Decoder) throws {
            let container: any SingleValueDecodingContainer = try decoder.singleValueContainer()
            self = try Result { try .code(container.decode(String.self)) }
                .flatMapError { _ in Result { try .detail(container.decode(ClaudeAPIError.self)) } }
                .get()
        }

        var code: String {
            switch self {
                case .code(let code): code
                case .detail(let detail): detail.type
            }
        }
    }

    let error: Field?
    let errorDescription: String?
    let errorUri: String?
}

// --- [ERRORS] --------------------------------------------------------------------------

nonisolated enum ClaudeFailure: DeadlineFailure, ProviderFailure {
    case executableMissing
    case signInRequired
    case accountOnHold(URL)
    case authenticationFailed(TerminationStatus)
    case process(ProcessFailure)
    case keychainLocked
    case keychainFailed(TerminationStatus)
    case credentialTooLarge
    case filesystem(any Error)
    case invalidCredentials
    case invalidIdentity(AggregateError<IdentityError>)
    case invalidResponse
    case invalidQuota(AggregateError<QuotaFailure>)
    case http(ClaudeEndpoint, status: Int, error: ClaudeAPIError?)
    case rateLimited(ClaudeEndpoint, until: Date)
    case transport(any Error)
    case accountChanged
    case modelUnavailable
    case greetingFailed
    case protocolFailure
    case cancelled
    case timedOut
    case lockHeld(URL)
    case lockCompromised(URL)
    indirect case cleanup(operation: Self, release: Self)

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

    var isUnauthorized: Bool {
        if case .http(_, 401, _) = cause { true } else { false }
    }

    var requiresSignIn: Bool {
        if case .signInRequired = cause { true } else { false }
    }

    var isAccountMismatch: Bool {
        if case .accountChanged = cause { true } else { false }
    }

    var isCancellation: Bool {
        if case .cancelled = cause { true } else { false }
    }

    var retryAfter: Date? {
        if case .rateLimited(_, let until) = cause { until } else { nil }
    }

    var errorDescription: String? {
        switch self {
            case .executableMissing: "Claude Code not installed"
            case .signInRequired: "Sign in required"
            case .accountOnHold(let url): "Account on hold: \(url.absoluteString)"
            case .authenticationFailed(let status): "Claude Code sign-in failed: \(status)"
            case .process(let failure): failure.localizedDescription
            case .keychainLocked: "Login keychain locked"
            case .keychainFailed(let status): "Keychain access failed: \(status)"
            case .credentialTooLarge: "Claude credential too large to save"
            case .filesystem(let error): "Could not access Claude files: \(error.localizedDescription)"
            case .invalidCredentials: "Unreadable Claude credential"
            case .invalidIdentity: "Incomplete Claude account identity"
            case .invalidResponse: "Unreadable Claude response"
            case .invalidQuota: "Unreadable Claude usage"
            case .http(let endpoint, let status, .some(let error)): "Claude \(endpoint) request returned \(status): \(error.message)"
            case .http(let endpoint, let status, .none): "Claude \(endpoint) request returned \(status)"
            case .rateLimited(let endpoint, _): "Claude \(endpoint) requests limited"
            case .transport(let error): "Could not reach Claude: \(error.localizedDescription)"
            case .accountChanged: "Signed in to a different Claude account"
            case .modelUnavailable: "Haiku model unavailable"
            case .greetingFailed: "Claude greeting failed"
            case .protocolFailure: "Unexpected Claude Code response"
            case .cancelled: "Canceled"
            case .timedOut: "Claude Code timed out"
            case .lockHeld: "Claude credential update in progress, try again when it finishes"
            case .lockCompromised: "Claude credential lock lost"
            case .cleanup(let operation, _): operation.localizedDescription
        }
    }
}

nonisolated extension ClaudeFailure {
    static func rejection(
        by endpoint: ClaudeEndpoint,
        status: Int,
        body: Data,
        retryAfter header: String?,
        at now: Date,
    ) -> ClaudeFailure {
        let decoded: ClaudeErrorBody? = try? ClaudeEndpoint.decoder.decode(ClaudeErrorBody.self, from: body)
        let code: String? = decoded?.error?.code
        let onHold: Bool = [decoded?.errorDescription, code].contains("account_on_hold")
        let detail: ClaudeAPIError? = if case .detail(let detail)? = decoded?.error { detail } else { nil }
        let rejected: ClaudeFailure = .http(endpoint, status: status, error: detail)
        return switch endpoint {
            case _ where status == 429: .rateLimited(endpoint, until: retryDate(header, at: now))
            case .token where onHold && [400, 401, 403].contains(status):
                URL(string: decoded?.errorUri ?? "https://claude.ai/restricted")
                    .map(Self.accountOnHold) ?? rejected
            case .token where code == "invalid_grant" && [400, 401].contains(status): .signInRequired
            case .profile, .usage, .token: rejected
        }
    }

    private static func retryDate(_ header: String?, at now: Date) -> Date {
        let defaultRetryDelay: TimeInterval = 300
        let maximumRetryDelay: TimeInterval = 86_400
        let delay: TimeInterval? = header.flatMap { value in
            UInt(value).map(TimeInterval.init)
                ?? (try? Date(value, strategy: .http)).map { date in date.timeIntervalSince(now) }
        }
        let wait: TimeInterval =
            switch delay {
                case .some(let seconds) where seconds > 0: min(seconds, maximumRetryDelay)
                case .some, .none: defaultRetryDelay
            }
        return now.addingTimeInterval(wait)
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

nonisolated extension Result where Failure == KeychainFailure {
    func claude() -> Result<Success, ClaudeFailure> {
        mapError { failure in
            switch failure {
                case .interactionNotAllowed: .keychainLocked
                case .itemTooLarge: .credentialTooLarge
                case .failed(let status): .keychainFailed(status)
                case .process(let error): ClaudeFailure(process: error)
            }
        }
    }
}
