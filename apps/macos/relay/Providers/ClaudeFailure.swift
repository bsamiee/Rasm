import CasePaths
import Files
import Foundation
import HTTPTypes
import Processes
import Results
import Security
import Subprocess

// --- [TYPES] ---------------------------------------------------------------------------

nonisolated enum ClaudeEndpoint: String, Sendable {
    case profile = "https://api.anthropic.com/api/oauth/profile"
    case usage = "https://api.anthropic.com/api/oauth/usage"
    case token = "https://platform.claude.com/v1/oauth/token"

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
    @CasePathable
    enum Field: Decodable, Sendable {
        case code(String)
        case detail(ClaudeAPIError)

        init(from decoder: any Decoder) throws {
            let container: any SingleValueDecodingContainer = try decoder.singleValueContainer()
            self = try Result { try .code(container.decode(String.self)) }
                .flatMapError { error in if case DecodingError.typeMismatch = error { Result { try .detail(container.decode(ClaudeAPIError.self)) } } else { .failure(error) } }
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

@CasePathable
nonisolated enum ClaudeFailure: ProviderFailure, LockFailure {
    case executableMissing
    case signInRequired
    case accountOnHold(URL)
    case process(ProcessFailure)
    case filesystem(any Error)
    case interactionNotAllowed
    case invalidCredentials(any Error?)
    case invalidIdentity(AggregateError<IdentityError>)
    case invalidResponse(ClaudeEndpoint, any Error?)
    case invalidQuota(AggregateError<QuotaFailure>)
    case http(ClaudeEndpoint, status: HTTPResponse.Status, error: ClaudeAPIError?)
    case rateLimited(ClaudeEndpoint, until: Date)
    case transport(any Error)
    case accountChanged
    case protocolFailure
    case cancelled(CancellationError)
    case lock(FileFailure)
    indirect case cleanup(operation: Self, release: Self)

    init(child failure: ProcessFailure) {
        self =
            switch failure.reason {
                case .subprocess(let error) where error.code == .executableNotFound: .executableMissing
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

    var retryAfter: Date? { cause[case: \.rateLimited]?.until }

    var errorDescription: String? {
        switch self {
            case .executableMissing: "Claude Code not installed"
            case .signInRequired: "Sign in required"
            case .accountOnHold(let url): "Account on hold, visit \(url.absoluteString)"
            case .process(let failure): failure.localizedDescription
            case .filesystem(let error): "Claude file access failed. \(error.localizedDescription)"
            case .interactionNotAllowed: SecCopyErrorMessageString(errSecInteractionNotAllowed, nil) as String?
            case .invalidCredentials: "Unreadable Claude credential"
            case .invalidIdentity: "Incomplete Claude account identity"
            case .invalidResponse(let endpoint, _): "Unreadable Claude \(endpoint) response"
            case .invalidQuota: "Unreadable Claude usage"
            case .http(let endpoint, let status, .some(let error)): "Claude \(endpoint) request returned \(status). \(error.message)"
            case .http(let endpoint, let status, .none): "Claude \(endpoint) request returned \(status)"
            case .rateLimited(let endpoint, _): "Claude \(endpoint) requests rate limited"
            case .transport(let error): "Claude unreachable. \(error.localizedDescription)"
            case .accountChanged: "Signed in to different Claude account"
            case .protocolFailure: "Unexpected Claude Code response"
            case .cancelled: "Claude Code was canceled"
            case .lock(let failure) where failure.reason.is(\.held): "Claude credential update in progress, retry after it finishes"
            case .lock(let failure): failure.localizedDescription
            case .cleanup(let operation, _): operation.localizedDescription
        }
    }
}

nonisolated extension ClaudeFailure {
    static func rejection(
        by endpoint: ClaudeEndpoint,
        status: HTTPResponse.Status,
        body: Data,
        retryAfter header: String?,
        at now: Date,
    ) -> ClaudeFailure {
        let decoded: ClaudeErrorBody? = try? ClaudeEndpoint.decoder.decode(ClaudeErrorBody.self, from: body)
        let code: String? = decoded?.error?.code
        let onHold: Bool = [decoded?.errorDescription, code].contains("account_on_hold")
        let rejected: ClaudeFailure = .http(endpoint, status: status, error: decoded?.error?[case: \.detail])
        let requested: TimeInterval? = header.flatMap { value -> TimeInterval? in UInt(value).map(TimeInterval.init) ?? (try? Date(value, strategy: .http))?.timeIntervalSince(now) }
        let retryDelay: TimeInterval = requested.flatMap { seconds in seconds > 0 ? min(seconds, 86_400) : nil } ?? 300
        return switch endpoint {
            case _ where status == .tooManyRequests: .rateLimited(endpoint, until: now.addingTimeInterval(retryDelay))
            case .token where onHold && [.badRequest, .unauthorized, .forbidden].contains(status):
                URL(string: decoded?.errorUri ?? "https://claude.ai/restricted")
                    .map(Self.accountOnHold) ?? rejected
            case .token where code == "invalid_grant" && [.badRequest, .unauthorized].contains(status): .signInRequired
            case .profile, .usage, .token: rejected
        }
    }
}
