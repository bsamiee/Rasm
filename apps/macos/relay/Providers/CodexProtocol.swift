import Encoding
import Files
import Foundation
import Results

// --- [MODELS] --------------------------------------------------------------------------

nonisolated enum CodexProtocol {
    // --- [JSON_RPC]

    enum Method: String, Sendable {
        case initialize
        case initialized
        case configRead = "config/read"
        case accountRateLimitsRead = "account/rateLimits/read"
        case accountLoginStart = "account/login/start"
        case accountLoginCompleted = "account/login/completed"
        case accountLogout = "account/logout"
    }

    // --- [REQUESTS]

    static let authMode: String = "chatgpt"

    struct InitializeParams: Encodable, Sendable {
        struct ClientInfo: Encodable, Sendable {
            let name: String? = Bundle.main.bundleIdentifier
            let title: String? = Bundle.main.object(forInfoDictionaryKey: kCFBundleNameKey as String) as? String
            let version: String? = Bundle.main.object(forInfoDictionaryKey: "CFBundleShortVersionString") as? String
        }

        struct Capabilities: Encodable, Sendable {
            let experimentalApi: Bool = true
        }

        let clientInfo: ClientInfo = .init()
        let capabilities: Capabilities = .init()
    }

    struct ConfigReadParams: Encodable, Sendable {
        let includeLayers: Bool
    }

    struct RateLimitsReadParams: Encodable, Sendable {
        let excludeResetCreditDetails: Bool
    }

    struct LoginStartParams: Encodable, Sendable {
        let type: String = authMode
    }

    // --- [MESSAGES]

    struct RateLimitWindow: Decodable, Sendable {
        let usedPercent: Double
        let windowDurationMins: Int?
        let resetsAt: Int?
    }

    struct RateLimitSnapshot: Decodable, Sendable {
        static let codexLimitID: String = "codex"

        let limitId: String?
        let primary: RateLimitWindow?
        let secondary: RateLimitWindow?

        var isCodex: Bool { limitId == nil || limitId == Self.codexLimitID }
    }

    struct AccountRateLimits: Decodable, Sendable {
        let accountId: String?
        let ordinaryUsageAllowed: Bool?
        let rateLimits: RateLimitSnapshot
        let rateLimitsByLimitId: [String: RateLimitSnapshot]?

        func usage(identity: AccountIdentity, observedAt: Date) throws(CodexFailure) -> AccountUsage {
            guard accountId == nil || accountId == identity.organizationID else { throw .identityChanged }
            guard let limits = rateLimitsByLimitId?[RateLimitSnapshot.codexLimitID] ?? (rateLimits.isCodex ? rateLimits : nil) else {
                throw .invalidResponse(.accountRateLimitsRead)
            }
            let weekly = [limits.primary, limits.secondary].compactMap(\.self).filter { window in (window.windowDurationMins ?? 0) >= 7 * 24 * 60 }
            let windows = try traverse(weekly) { window in
                UsageAmount.make(percent: window.usedPercent)
                    .aggregated(\.self)
                    .map { amount in
                        QuotaWindow(
                            kind: .weekly,
                            used: amount,
                            resetsAt: window.resetsAt.map { seconds in Date(timeIntervalSince1970: TimeInterval(seconds)) },
                        )
                    }
            }
            .mapError(CodexFailure.invalidQuota)
            .get()
            return AccountUsage(windows: windows, includedUsageAllowed: ordinaryUsageAllowed, observedAt: observedAt, signInExpiresAt: nil)
        }
    }

    struct LoginStarted: Decodable, Sendable {
        let loginId: String
        let authUrl: URL
    }

    struct LoginCompleted: Decodable, Sendable {
        let loginId: String?
        let success: Bool
        let error: String?
    }

    enum AuthCredentialsStore: String, Sendable {
        case file, keyring, auto, ephemeral
    }

    struct EffectiveConfig: Decodable, Sendable {
        struct Config: Decodable, Sendable {
            let cliAuthCredentialsStore: Lenient<AuthCredentialsStore>?
            let forcedChatgptWorkspaceId: JSONValue?

            enum CodingKeys: String, CodingKey {
                case cliAuthCredentialsStore = "cli_auth_credentials_store"
                case forcedChatgptWorkspaceId = "forced_chatgpt_workspace_id"
            }
        }
        let config: Config
    }
}

nonisolated enum CodexAuthFile {
    private static let decoder: JSONDecoder = {
        let decoder = JSONDecoder()
        decoder.keyDecodingStrategy = .convertFromSnakeCase
        return decoder
    }()

    private struct Document: Decodable, Sendable {
        struct Tokens: Decodable, Sendable {
            let idToken: String?
            let accountId: String?
        }

        let authMode: String?
        let tokens: Tokens?
    }

    private struct Claims: Decodable, Sendable {
        struct Auth: Decodable, Sendable {
            let chatgptAccountId: String?
            let chatgptUserId: String?
            let userId: String?
            let chatgptPlanType: String?
        }

        let email: String?
        let auth: Auth?

        enum CodingKeys: String, CodingKey {
            case email
            case auth = "https://api.openai.com/auth"
        }
    }

    static func file(in home: URL) -> URL { home.appending(path: "auth.json") }

    static func read(in home: URL) throws(CodexFailure) -> (data: Data, identity: AccountIdentity?)? {
        let url = file(in: home)
        guard let data = try Result(catching: { try ifPresent { try Data(contentsOf: url) } }).mapError(CodexFailure.storage).get() else { return nil }
        let document = try Result { try decoder.decode(Document.self, from: data) }.mapError { error in CodexFailure.invalidAuthFile(url, error) }.get()
        if let mode = document.authMode, mode != CodexProtocol.authMode { throw .subscriptionRequired }
        guard let tokens = document.tokens, let idToken = tokens.idToken else { return (data, nil) }
        let components = idToken.split(separator: ".")
        guard components.count == 3 else { throw .invalidAuthFile(url, nil) }
        let encoded = String(components[1]).replacing("-", with: "+").replacing("_", with: "/")
        guard let payload = Data(base64Encoded: encoded + String(repeating: "=", count: (4 - encoded.count % 4) % 4)) else { throw .invalidAuthFile(url, nil) }
        let claims = try Result { try decoder.decode(Claims.self, from: payload) }.mapError { error in CodexFailure.invalidAuthFile(url, error) }.get()
        let workspace = claims.auth?.chatgptAccountId ?? tokens.accountId
        let identity = try combine(
            AccountIdentity.make(accountID: claims.auth?.chatgptUserId ?? claims.auth?.userId, organizationID: workspace, email: claims.email, plan: claims.auth?.chatgptPlanType),
            workspace.map(Result.success) ?? .failure(AggregateError(first: IdentityError(field: \AccountIdentity.organizationID))),
        )
        .map { identity, _ in identity }
        .mapError(CodexFailure.invalidIdentity)
        .get()
        return (data, identity)
    }
}
