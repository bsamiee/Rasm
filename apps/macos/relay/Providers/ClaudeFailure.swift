import CasePaths
import Foundation
import HTTPTypes
import Keychain
import Lockfiles
import Processes
import Results
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

@CasePathable
nonisolated enum ClaudeFailure: DeadlineFailure, ProviderFailure, LockFailure {
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
    case http(ClaudeEndpoint, status: HTTPResponse.Status, error: ClaudeAPIError?)
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

    var requiresSignIn: Bool { cause.is(\.signInRequired) }

    var isAccountMismatch: Bool { cause.is(\.accountChanged) }

    var isCancellation: Bool { [cause[case: \.deadline], cause[case: \.keychain.process.deadline]].contains(.cancelled) }

    var retryAfter: Date? { cause[case: \.rateLimited]?.until }

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
            case .deadline(let deadline): "Claude Code \(deadline.localizedDescription)"
            case .lock(.held): "Claude credential update in progress, try again when it finishes"
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
