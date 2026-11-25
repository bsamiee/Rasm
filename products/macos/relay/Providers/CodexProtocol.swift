import CurrentProcess
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
            let name: String = Program.current.identifier
            let title: String? = Bundle.main.object(forInfoDictionaryKey: kCFBundleNameKey as String) as? String
            let version: String? = Program.current.version
        }

        struct Capabilities: Encodable, Sendable {
            let experimentalApi: Bool = true
        }

        let clientInfo = ClientInfo()
        let capabilities = Capabilities()
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
    }

    struct AccountRateLimits: Decodable, Sendable {
        let accountId: String?
        let ordinaryUsageAllowed: Bool?
        let rateLimits: RateLimitSnapshot
        let rateLimitsByLimitId: [String: RateLimitSnapshot]?

        func usage(identity: AccountIdentity, observedAt: Date) throws(AccountFailure) -> AccountUsage {
            guard accountId == nil || accountId == identity.organizationID else { throw AccountFailure(.openAI, .accountChanged) }
            let fallback = [nil, RateLimitSnapshot.codexLimitID].contains(rateLimits.limitId) ? rateLimits : nil
            guard let limits = rateLimitsByLimitId?[RateLimitSnapshot.codexLimitID] ?? fallback else {
                throw AccountFailure(subject: "Codex \(RateLimitSnapshot.codexLimitID) rate limits", reason: .missing)
            }
            let weekly: [RateLimitWindow] = [limits.primary, limits.secondary].compactMap(\.self).filter { window in (window.windowDurationMins ?? 0) >= 7 * 24 * 60 }
            let windows = try traverse(weekly) { window in
                Result { () throws(AccountFailure) in
                    try Relay.RateLimitWindow(kind: .weekly, percent: window.usedPercent, resetsAt: window.resetsAt.map { seconds in Date(timeIntervalSince1970: TimeInterval(seconds)) })
                }
                .aggregated(\.self)
            }
            .mapError { causes in AccountFailure(subject: "Codex usage", reason: .invalid(causes)) }
            .get()
            return AccountUsage(windows: windows, ordinaryUsageAllowed: ordinaryUsageAllowed, observedAt: observedAt, signInExpiresAt: nil)
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
            enum CodingKeys: String, CodingKey {
                case cliAuthCredentialsStore = "cli_auth_credentials_store"
                case forcedChatgptWorkspaceId = "forced_chatgpt_workspace_id"
            }

            let cliAuthCredentialsStore: Lenient<AuthCredentialsStore>?
            let forcedChatgptWorkspaceId: JSONValue?
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
        enum CodingKeys: String, CodingKey {
            case email
            case auth = "https://api.openai.com/auth"
        }

        struct Auth: Decodable, Sendable {
            let chatgptAccountId: String?
            let chatgptUserId: String?
            let userId: String?
            let chatgptPlanType: String?
        }

        let email: String?
        let auth: Auth?
    }

    static func file(in home: URL) -> URL { home.appending(path: "auth.json") }

    static func read(in home: URL) throws(AccountFailure) -> (data: Data, identity: AccountIdentity?)? {
        let url = file(in: home)
        guard let data = try url.contents() else { return nil }
        let subject = "Codex \(url.lastPathComponent)"
        let document = try decoder.decode(Document.self, from: data, subject: subject)
        if let mode = document.authMode, mode != CodexProtocol.authMode { throw AccountFailure(subject: "ChatGPT", reason: .signInRequired) }
        guard let tokens = document.tokens, let idToken = tokens.idToken else { return (data, nil) }
        let segments = idToken.split(separator: ".")
        let encoded: String = segments.count == 3 ? String(segments[1]).replacing("-", with: "+").replacing("_", with: "/") : ""
        let padded: String = encoded.padding(toLength: (encoded.count + 3) / 4 * 4, withPad: "=", startingAt: 0)
        guard let payload = Data(base64Encoded: padded), !payload.isEmpty else {
            throw AccountFailure(subject: "\(subject) id token payload", reason: .missing)
        }
        let claims = try decoder.decode(Claims.self, from: payload, subject: "\(subject) id token")
        guard let workspace = claims.auth?.chatgptAccountId ?? tokens.accountId else { throw AccountFailure(subject: "OpenAI workspace", reason: .missing) }
        return (data, try AccountIdentity(accountID: claims.auth?.chatgptUserId ?? claims.auth?.userId, organizationID: workspace, email: claims.email, plan: claims.auth?.chatgptPlanType))
    }
}
