import Encoding
import Files
import Foundation
import JSONRPC
import Results
import System

// --- [MODELS] --------------------------------------------------------------------------

nonisolated enum CodexProtocol {
    // --- [JSON_RPC]
    static let decoder: JSONDecoder = {
        let decoder: JSONDecoder = JSONDecoder()
        decoder.dateDecodingStrategy = .secondsSince1970
        decoder.keyDecodingStrategy = .convertFromSnakeCase
        return decoder
    }()

    enum Method: String, Sendable {
        case initialize
        case initialized
        case modelList = "model/list"
        case experimentalFeatureList = "experimentalFeature/list"
        case configRead = "config/read"
        case threadStart = "thread/start"
        case threadUnsubscribe = "thread/unsubscribe"
        case turnStart = "turn/start"
        case turnCompleted = "turn/completed"
        case accountRateLimitsRead = "account/rateLimits/read"
        case accountRateLimitsUpdated = "account/rateLimits/updated"
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

        let clientInfo: ClientInfo = ClientInfo()
        let capabilities: Capabilities = Capabilities()
    }

    struct PageParams: Encodable, Sendable {
        let cursor: String?
    }

    struct ConfigReadParams: Encodable, Sendable {
        let includeLayers: Bool
        let cwd: String?
    }

    struct GreetingConfig: Encodable, Sendable {
        struct Server: Encodable, Sendable {
            let enabled: Bool
        }

        let webSearch: String
        let projectDocMaxBytes: Int
        let mcpServers: [String: Server]

        enum CodingKeys: String, CodingKey {
            case webSearch = "web_search"
            case projectDocMaxBytes = "project_doc_max_bytes"
            case mcpServers = "mcp_servers"
        }
    }

    struct ThreadStartParams: Encodable, Sendable {
        let model: String
        let modelProvider: String
        let cwd: String
        let approvalPolicy: String
        let sandbox: String
        let runtimeWorkspaceRoots: [String]
        let ephemeral: Bool
        let environments: [JSONValue]
        let dynamicTools: [JSONValue]
        let selectedCapabilityRoots: [String]
        let baseInstructions: String
        let developerInstructions: String
        let config: JSONDocument<GreetingConfig>
    }

    struct ThreadParams: Encodable, Sendable {
        let threadId: String
    }

    struct TextInput: Encodable, Sendable {
        let type: String = "text"
        let text: String
    }

    struct TurnStartParams: Encodable, Sendable {
        let threadId: String
        let input: [TextInput]
        let effort: String
        let serviceTierForTurn: String
    }

    struct RateLimitsReadParams: Encodable, Sendable {
        let excludeResetCreditDetails: Bool
    }

    struct LoginStartParams: Encodable, Sendable {
        let type: String = authMode
    }

    // --- [MESSAGES]
    struct Page<Item: Decodable & Sendable>: Decodable, Sendable {
        let data: [Item]
        let nextCursor: String?
    }

    struct RateLimitWindow: Decodable, Sendable {
        let usedPercent: Double
        let windowDurationMins: Int?
        let resetsAt: Date?
    }

    struct RateLimitSnapshot: Decodable, Sendable {
        static let codexLimitID: String = "codex"

        let limitId: String?
        let primary: RateLimitWindow?
        let secondary: RateLimitWindow?
        let rateLimitReachedType: String?
        let spendControlReached: Bool?

        var isCodex: Bool { limitId == nil || limitId == Self.codexLimitID }
    }

    struct AccountRateLimits: Decodable, Sendable {
        let accountId: String?
        let ordinaryUsageAllowed: Bool?
        let rateLimits: RateLimitSnapshot
        let rateLimitsByLimitId: [String: RateLimitSnapshot]?

        func usage(identity: AccountIdentity, observedAt: Date) throws(CodexFailure) -> AccountUsage {
            guard accountId == nil || accountId == identity.organizationID else { throw .identityChanged }
            guard let limits: RateLimitSnapshot = rateLimitsByLimitId.map({ byID in byID[RateLimitSnapshot.codexLimitID] }) ?? (rateLimits.isCodex ? rateLimits : nil) else {
                throw .invalidResponse(.accountRateLimitsRead)
            }
            let reached: Bool = limits.rateLimitReachedType != nil || limits.spendControlReached == true
            let windows: [QuotaWindow?] = try traverse([limits.primary, limits.secondary].compactMap(\.self)) { window -> Result<QuotaWindow?, AggregateError<QuotaFailure>> in
                let kind: QuotaKind? =
                    switch window.windowDurationMins {
                        case .some(let minutes) where minutes <= 0: nil
                        case .some(let minutes) where minutes <= 12 * 60: .session
                        case .some: .weekly
                        case .none: nil
                    }
                return kind.map { kind in
                    UsageAmount.make(percent: window.usedPercent)
                        .aggregated(\.self)
                        .map { amount in QuotaWindow(kind: kind, used: amount, resetsAt: window.resetsAt, rejected: reached && amount.isExhausted) }
                } ?? .success(nil)
            }
            .mapError(CodexFailure.invalidQuota)
            .get()
            return AccountUsage(windows: windows.compactMap(\.self), includedUsageAllowed: ordinaryUsageAllowed, observedAt: observedAt, signInExpiresAt: nil)
        }
    }

    struct RateLimitsUpdated: Decodable, Sendable {
        let rateLimits: RateLimitSnapshot
    }

    struct ReasoningEffortOption: Decodable, Sendable {
        let reasoningEffort: String
    }

    struct Model: Decodable, Sendable {
        let model: String
        let supportedReasoningEfforts: [ReasoningEffortOption]
    }

    struct ExperimentalFeature: Decodable, Sendable {
        let name: String
        let enabled: Bool
    }

    struct ThreadReference: Decodable, Sendable {
        let id: String
    }

    struct ThreadStarted: Decodable, Sendable {
        let thread: ThreadReference
    }

    struct TurnError: Decodable, Sendable {
        let message: String
        @Lenient var codexErrorInfo: String?
    }

    enum TurnStatus: String, Decodable, Sendable {
        case completed, interrupted, failed
    }

    struct Turn: Decodable, Sendable {
        let id: String
        @Lenient var status: TurnStatus?
        let error: TurnError?
    }

    struct TurnStarted: Decodable, Sendable {
        let turn: Turn
    }

    struct TurnCompleted: Decodable, Sendable {
        let threadId: String
        let turn: Turn
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

    enum AuthCredentialsStore: String, Decodable, Sendable {
        case file, keyring, auto, ephemeral
    }

    struct EffectiveConfig: Decodable, Sendable {
        struct Config: Decodable, Sendable {
            let mcpServers: [String: JSONValue]?
            let cliAuthCredentialsStore: AuthCredentialsStore?
            let forcedChatgptWorkspaceId: JSONValue?
        }
        let config: Config
    }

    // --- [GREETING]
    static let greetingModelName: String = "gpt-5.6-luna"

    static func greet(_ connection: JSONRPCConnection<Method, CodexFailure>, workingDirectory: URL) async throws(CodexFailure) {
        func pages<Item: Decodable & Sendable>(_ method: Method, of _: Item.Type, after cursor: String? = nil) async throws(CodexFailure) -> [Item] {
            let page: Page<Item> = try await connection.request(method, params: PageParams(cursor: cursor), returning: Page<Item>.self)
            let rest: [Item] = if let next: String = page.nextCursor { try await pages(method, of: Item.self, after: next) } else { [] }
            return page.data + rest
        }
        let directory: String = FilePath(workingDirectory.path(percentEncoded: false)).string
        guard let model: Model = try await pages(.modelList, of: Model.self).first(where: { value in value.model == greetingModelName }) else { throw .modelUnavailable }
        let supported: Set<String> = Set(model.supportedReasoningEfforts.map(\.reasoningEffort))
        guard let effort: String = ["none", "minimal", "low", "medium", "high", "xhigh", "max", "ultra"].first(where: supported.contains) else {
            throw .invalidResponse(.modelList)
        }
        let enabled: [String] = try await pages(.experimentalFeatureList, of: ExperimentalFeature.self).filter(\.enabled).map { feature in "features.\(feature.name)" }
        let disabled: [String] =
            enabled + [
                "orchestrator.skills.enabled", "skills.include_instructions", "token_budget.use_history_notes_extension",
                "tools.experimental_request_user_input.enabled", "tools.update_plan.enabled",
            ]
        let effective: EffectiveConfig = try await connection.request(.configRead, params: ConfigReadParams(includeLayers: false, cwd: directory), returning: EffectiveConfig.self)
        let started: ThreadStarted = try await connection.request(
            .threadStart,
            params: ThreadStartParams(
                model: greetingModelName,
                modelProvider: "openai",
                cwd: directory,
                approvalPolicy: "never",
                sandbox: "read-only",
                runtimeWorkspaceRoots: [],
                ephemeral: true,
                environments: [],
                dynamicTools: [],
                selectedCapabilityRoots: [],
                baseInstructions: "Reply with hi.",
                developerInstructions: "",
                config: JSONDocument(
                    fields: Dictionary(uniqueKeysWithValues: disabled.map { key in (key, JSONValue.bool(false)) }),
                    known: GreetingConfig(
                        webSearch: "disabled",
                        projectDocMaxBytes: 0,
                        mcpServers: (effective.config.mcpServers ?? [:]).mapValues { _ in GreetingConfig.Server(enabled: false) },
                    ),
                ),
            ),
            returning: ThreadStarted.self,
        )
        let outcome: Result<Void, CodexFailure> = await Result { () async throws(CodexFailure) in
            let turn: TurnStarted = try await connection.request(
                .turnStart,
                params: TurnStartParams(threadId: started.thread.id, input: [TextInput(text: "hi")], effort: effort, serviceTierForTurn: "default"),
                returning: TurnStarted.self,
            )
            let completed: TurnCompleted = try await connection.notification(.turnCompleted, returning: TurnCompleted.self) { completed in
                completed.threadId == started.thread.id && completed.turn.id == turn.turn.id
            }
            switch (completed.turn.status, completed.turn.error) {
                case (.completed, _): return
                case (.interrupted, _): throw .deadline(.cancelled)
                case (.failed, .some(let error)) where error.codexErrorInfo == "unauthorized": throw .turnUnauthorized(message: error.message)
                case (.failed, .some(let error)): throw .turnFailed(message: error.message)
                case (.failed, .none), (.none, _): throw .invalidResponse(.turnCompleted)
            }
        }
        let detached: Result<Void, CodexFailure> = await Result { () async throws(CodexFailure) in
            try await connection.request(.threadUnsubscribe, params: ThreadParams(threadId: started.thread.id))
        }
        try outcome.get()
        try detached.get()
    }
}

nonisolated enum CodexAuthFile {
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
        let url: URL = file(in: home)
        guard let data: Data = try ifPresent({ try Data(contentsOf: url) }).mapError(CodexFailure.storage).get() else { return nil }
        let document: Document = try Result { try CodexProtocol.decoder.decode(Document.self, from: data) }.mapError { error in CodexFailure.invalidAuthFile(url, error) }.get()
        if let mode: String = document.authMode, mode != CodexProtocol.authMode { throw .subscriptionRequired }
        guard let tokens: Document.Tokens = document.tokens, let idToken: String = tokens.idToken else { return (data, nil) }
        let components: [Substring] = idToken.split(separator: ".")
        guard components.count == 3 else { throw .invalidAuthFile(url, nil) }
        let encoded: String = String(components[1]).replacing("-", with: "+").replacing("_", with: "/")
        guard let payload: Data = Data(base64Encoded: encoded + String(repeating: "=", count: (4 - encoded.count % 4) % 4)) else { throw .invalidAuthFile(url, nil) }
        let claims: Claims = try Result { try CodexProtocol.decoder.decode(Claims.self, from: payload) }.mapError { error in CodexFailure.invalidAuthFile(url, error) }.get()
        let workspace: String? = claims.auth?.chatgptAccountId ?? tokens.accountId
        let identity: AccountIdentity = try combine(
            AccountIdentity.make(accountID: claims.auth?.chatgptUserId ?? claims.auth?.userId, organizationID: workspace, email: claims.email, plan: claims.auth?.chatgptPlanType),
            workspace.map(Result.success) ?? .failure(AggregateError(first: IdentityError(field: \AccountIdentity.organizationID))),
        )
        .map { identity, _ in identity }
        .mapError(CodexFailure.invalidIdentity)
        .get()
        return (data, identity)
    }
}
