import Files
import Foundation
import Processes
import Results
import Subprocess

// --- [TYPES] ---------------------------------------------------------------------------

nonisolated enum ClaudeEndpoint: String, Sendable {
    case profile = "https://api.anthropic.com/api/oauth/profile"
    case usage = "https://api.anthropic.com/api/oauth/usage"
    case token = "https://platform.claude.com/v1/oauth/token"

    var timeout: TimeInterval { self == .token ? 30 : 15 }

    static let decoder: JSONDecoder = {
        let decoder: JSONDecoder = JSONDecoder()
        decoder.keyDecodingStrategy = .convertFromSnakeCase
        return decoder
    }()

    static let encoder: JSONEncoder = {
        let encoder: JSONEncoder = JSONEncoder()
        encoder.keyEncodingStrategy = .convertToSnakeCase
        return encoder
    }()
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

nonisolated enum ClaudeFailure: ProviderFailure, LockFailure {
    case executableMissing
    case signInRequired
    case accountOnHold(URL)
    case process(ProcessFailure)
    case keychain(KeychainFailure)
    case filesystem(any Error)
    case invalidCredentials(any Error?)
    case invalidIdentity(AggregateError<IdentityError>)
    case invalidResponse(ClaudeEndpoint, any Error?)
    case invalidQuota(AggregateError<QuotaFailure>)
    case http(ClaudeEndpoint, status: Int, error: ClaudeAPIError?)
    case rateLimited(ClaudeEndpoint, until: Date)
    case transport(any Error)
    case accountChanged
    case modelUnavailable
    case greetingFailed
    case protocolFailure
    case deadline(Deadline)
    case lock(LockfileFailure)
    indirect case cleanup(operation: Self, release: Self)

    init(child error: any Error) {
        self =
            switch ProcessFailure(error) {
                case .subprocess(let error) where error.code == .executableNotFound: .executableMissing
                case let failure: Self(process: failure)
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
        switch cause {
            case .deadline(.cancelled), .keychain(.process(.deadline(.cancelled))): true
            default: false
        }
    }

    var retryAfter: Date? {
        if case .rateLimited(_, let until) = cause { until } else { nil }
    }

    var errorDescription: String? {
        switch self {
            case .executableMissing: "Claude Code not installed"
            case .signInRequired: "Sign in required"
            case .accountOnHold(let url): "Account on hold: \(url.absoluteString)"
            case .process(let failure): failure.localizedDescription
            case .keychain(let failure): failure.localizedDescription
            case .filesystem(let error): "Could not access Claude files: \(error.localizedDescription)"
            case .invalidCredentials: "Unreadable Claude credential"
            case .invalidIdentity: "Incomplete Claude account identity"
            case .invalidResponse(let endpoint, _): "Unreadable Claude \(endpoint) response"
            case .invalidQuota: "Unreadable Claude usage"
            case .http(let endpoint, let status, .some(let error)): "Claude \(endpoint) request returned \(status): \(error.message)"
            case .http(let endpoint, let status, .none): "Claude \(endpoint) request returned \(status)"
            case .rateLimited(let endpoint, _): "Claude \(endpoint) requests limited"
            case .transport(let error): "Could not reach Claude: \(error.localizedDescription)"
            case .accountChanged: "Signed in to a different Claude account"
            case .modelUnavailable: "Haiku model unavailable"
            case .greetingFailed: "Claude greeting failed"
            case .protocolFailure: "Unexpected Claude Code response"
            case .deadline(.cancelled): "Canceled"
            case .deadline(.timedOut(let deadline)): "Claude Code did not answer within \(deadline.formatted(.units(width: .wide)))"
            case .lock(.held): "Claude credential update in progress, try again when it finishes"
            case .lock(let failure): failure.localizedDescription
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
        let retryDelay: TimeInterval =
            header.flatMap { value in UInt(value).map(TimeInterval.init) ?? (try? Date(value, strategy: .http))?.timeIntervalSince(now) }
            .flatMap { seconds in seconds > 0 ? min(seconds, 86_400) : nil } ?? 300
        return switch endpoint {
            case _ where status == 429: .rateLimited(endpoint, until: now.addingTimeInterval(retryDelay))
            case .token where onHold && [400, 401, 403].contains(status):
                URL(string: decoded?.errorUri ?? "https://claude.ai/restricted")
                    .map(Self.accountOnHold) ?? rejected
            case .token where code == "invalid_grant" && [400, 401].contains(status): .signInRequired
            case .profile, .usage, .token: rejected
        }
    }
}
