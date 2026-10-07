import Foundation

// --- [MODELS] --------------------------------------------------------------------------

nonisolated enum CodexProtocol {
    // --- [JSON_RPC]
    enum Method: String, Codable, Sendable {
        case initialize
        case initialized
        case modelList = "model/list"
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

    struct Request<Params: Encodable & Sendable>: Encodable, Sendable {
        let id: String
        let method: Method
        let params: Params?
    }

    struct Notification: Encodable, Sendable {
        let method: Method
    }

    struct ErrorObject: Codable, Sendable {
        let code: Int
        let message: String
    }

    struct ErrorReply: Encodable, Sendable {
        let id: JSONValue
        let error: ErrorObject
    }

    struct ResultField<Value: Decodable & Sendable>: Decodable, Sendable {
        let result: Value
    }

    struct ParamsField<Value: Decodable & Sendable>: Decodable, Sendable {
        let params: Value
    }

    enum ServerMessage: Decodable, Sendable {
        case request(id: JSONValue)
        case response(id: String, error: ErrorObject?, holdsResult: Bool)
        case notification(method: Method)
        case ignored

        enum CodingKeys: CodingKey {
            case id, method, params, result, error
        }

        init(from decoder: any Decoder) throws {
            let container: KeyedDecodingContainer<CodingKeys>? = try? decoder.container(keyedBy: CodingKeys.self)
            self = try container.map(Self.init(container:)) ?? .ignored
        }

        private init(container: KeyedDecodingContainer<CodingKeys>) throws {
            let responseID: String? = try? container.decode(String.self, forKey: .id)
            let method: Method? = try? container.decode(Method.self, forKey: .method)
            self =
                if container.contains(.id), container.contains(.method) {
                    try .request(id: container.decode(JSONValue.self, forKey: .id))
                } else if let responseID {
                    try .response(
                        id: responseID,
                        error: container.contains(.error) ? container.decode(ErrorObject.self, forKey: .error) : nil,
                        holdsResult: container.contains(.result),
                    )
                } else if let method, container.contains(.params) {
                    .notification(method: method)
                } else {
                    .ignored
                }
        }
    }

    // --- [REQUESTS]
    enum InputType: String, Encodable, Sendable {
        case text
    }

    enum AuthMode: String, Encodable, Sendable {
        case chatgpt
    }

    struct InitializeParams: Encodable, Sendable {
        struct ClientInfo: Encodable, Sendable {
            let name: String
            let title: String
            let version: String
        }

        struct Capabilities: Encodable, Sendable {
            let experimentalApi: Bool
        }

        let clientInfo: ClientInfo
        let capabilities: Capabilities
    }

    struct ModelListParams: Encodable, Sendable {
        let includeHidden: Bool
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
        let type: InputType
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
        let type: AuthMode
    }

    // --- [MESSAGES]
    struct InitializeResult: Decodable, Sendable {
        let userAgent: String?
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

        var codexLimits: RateLimitSnapshot? {
            switch (rateLimitsByLimitId, rateLimits.isCodex) {
                case (.some(let byID), _): byID[RateLimitSnapshot.codexLimitID]
                case (.none, true): rateLimits
                case (.none, false): nil
            }
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

    struct ModelList: Decodable, Sendable {
        let data: [Model]
        let nextCursor: String?
    }

    struct ThreadReference: Decodable, Sendable {
        let id: String
    }

    struct ThreadStarted: Decodable, Sendable {
        let thread: ThreadReference
    }

    enum TurnErrorInfo: String, Decodable, Sendable {
        case unauthorized
    }

    struct TurnError: Decodable, Sendable {
        let message: String
        let codexErrorInfo: Lenient<TurnErrorInfo>
    }

    enum TurnStatus: String, Decodable, Sendable {
        case completed, interrupted, failed
    }

    struct Turn: Decodable, Sendable {
        let id: String
        let status: Lenient<TurnStatus>
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
        let loginId: String?
        let authUrl: String?
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
            let authCredentialsStore: AuthCredentialsStore?
            let forcedWorkspace: JSONValue?

            enum CodingKeys: String, CodingKey {
                case mcpServers = "mcp_servers"
                case authCredentialsStore = "cli_auth_credentials_store"
                case forcedWorkspace = "forced_chatgpt_workspace_id"
            }
        }
        let config: Config
    }

    // --- [USAGE]
    static func usage(
        _ response: AccountRateLimits,
        identity: AccountIdentity,
        observedAt: Date,
    ) -> Result<AccountUsage, CodexFailure> {
        limits(of: response, for: identity).flatMap { limits in
            windows(limits).map { windows in
                AccountUsage(
                    windows: windows,
                    includedUsageAllowed: response.ordinaryUsageAllowed,
                    observedAt: observedAt,
                    signInExpiresAt: nil,
                )
            }
        }
    }

    private static func limits(
        of response: AccountRateLimits,
        for identity: AccountIdentity,
    ) -> Result<RateLimitSnapshot, CodexFailure> {
        let sameWorkspace: Bool =
            response.accountId == nil || response.accountId == identity.organizationID
        return sameWorkspace
            ? response.codexLimits.map(Result.success)
                ?? .failure(.invalidResponse(field: "usage limits"))
            : .failure(.identityChanged)
    }

    private static func windows(_ limits: RateLimitSnapshot) -> Result<[QuotaWindow], CodexFailure> {
        let reached: Bool = limits.rateLimitReachedType != nil || limits.spendControlReached == true
        return traverse([limits.primary, limits.secondary].compactMap(\.self)) { window in
            quotaWindow(window, reached: reached)
        }
        .map { windows in windows.compactMap(\.self) }
        .mapError { failure in .invalidResponse(field: failure.errors.joined(separator: ", ")) }
    }

    private static func quotaWindow(
        _ window: RateLimitWindow,
        reached: Bool,
    ) -> Result<QuotaWindow?, AggregateError<String>> {
        let kind: QuotaKind? =
            switch window.windowDurationMins {
                case .some(let minutes) where minutes <= 0: nil
                case .some(let minutes) where minutes <= 12 * 60: .session
                case .some: .weekly
                case .none: nil
            }
        return kind.map { kind in
            UsageAmount.make(percent: window.usedPercent)
                .mapError { _ in AggregateError(first: "\(kind.name) usage percentage", remaining: []) }
                .map { amount in
                    QuotaWindow(kind: kind, used: amount, resetsAt: window.resetsAt, rejected: reached && amount.isExhausted)
                }
        } ?? .success(nil)
    }

    // --- [GREETING]
    static let greetingModelName: String = "gpt-5.6-luna"

    static func greetingEffort(
        _ connection: CodexConnection,
        cursor: String? = nil,
    ) async -> Result<String, CodexFailure> {
        let reasoningEfforts: [String] = ["none", "minimal", "low", "medium", "high", "xhigh", "max", "ultra"]
        return await connection.request(
            ModelList.self,
            .modelList,
            params: ModelListParams(includeHidden: false, cursor: cursor),
        )
        .bind { list in
            if let model: Model = list.data.first(where: { value in value.model == greetingModelName }) {
                let supported: Set<String> = Set(model.supportedReasoningEfforts.map(\.reasoningEffort))
                return reasoningEfforts.first(where: supported.contains).map(Result.success)
                    ?? .failure(.invalidResponse(field: "model reasoning effort"))
            }
            return await
                (list.nextCursor.map(Result<String, CodexFailure>.success)
                ?? .failure(.modelUnavailable))
                .bind { next in await greetingEffort(connection, cursor: next) }
        }
    }

    static func sendGreeting(
        _ connection: CodexConnection,
        effort: String,
        workingDirectory: URL,
    ) async -> Result<Void, CodexFailure> {
        await connection.request(
            EffectiveConfig.self,
            .configRead,
            params: ConfigReadParams(includeLayers: false, cwd: workingDirectory.path),
        )
        .map { effective in greetingOverrides(effective.config) }
        .bind { overrides in
            await connection.request(
                ThreadStarted.self,
                .threadStart,
                params: ThreadStartParams(
                    model: greetingModelName,
                    modelProvider: "openai",
                    cwd: workingDirectory.path,
                    approvalPolicy: "never",
                    sandbox: "read-only",
                    runtimeWorkspaceRoots: [],
                    ephemeral: true,
                    environments: [],
                    dynamicTools: [],
                    selectedCapabilityRoots: [],
                    baseInstructions: "Reply with hi.",
                    developerInstructions: "",
                    config: overrides,
                ),
            )
        }
        .bind { started -> Result<Void, CodexFailure> in
            let outcome: Result<Void, CodexFailure> = await connection.request(
                TurnStarted.self,
                .turnStart,
                params: TurnStartParams(
                    threadId: started.thread.id,
                    input: [TextInput(type: .text, text: "hi")],
                    effort: effort,
                    serviceTierForTurn: "default",
                ),
            )
            .bind { turn in
                await connection.notification(TurnCompleted.self, .turnCompleted) { completed in
                    completed.threadId == started.thread.id && completed.turn.id == turn.turn.id
                }
            }
            .flatMap { completed -> Result<Void, CodexFailure> in
                switch (completed.turn.status.value, completed.turn.error) {
                    case (.completed, _): .success(())
                    case (.interrupted, _): .failure(.cancelled)
                    case (.failed, .some(let error)) where error.codexErrorInfo.value == .unauthorized:
                        .failure(.turnUnauthorized(message: error.message))
                    case (.failed, .some(let error)): .failure(.turnFailed(message: error.message))
                    case (.failed, .none): .failure(.invalidResponse(field: "turn error"))
                    case (.none, _): .failure(.invalidResponse(field: "turn completion"))
                }
            }
            let detached: Result<Void, CodexFailure> = await connection.request(
                .threadUnsubscribe,
                params: ThreadParams(threadId: started.thread.id),
            )
            return outcome.flatMap { _ in detached }
        }
    }

    private static func greetingOverrides(_ config: EffectiveConfig.Config) -> JSONDocument<GreetingConfig> {
        let disabledFeatures: [String] = [
            "features.apps", "features.code_mode", "features.code_mode_only", "features.context_management",
            "features.current_time_reminder", "features.deferred_executor", "features.enable_fanout",
            "features.goals", "features.hooks", "features.image_generation", "features.memories",
            "features.multi_agent", "features.multi_agent_v2", "features.plugins",
            "features.request_permissions_tool", "features.shell_snapshot", "features.shell_tool",
            "features.standalone_web_search", "features.token_budget", "features.tool_suggest",
            "features.unified_exec", "features.view_image", "orchestrator.skills.enabled",
            "skills.include_instructions", "token_budget.use_history_notes_extension",
            "tools.experimental_request_user_input.enabled", "tools.update_plan.enabled",
        ]
        return JSONDocument(
            fields: Dictionary(uniqueKeysWithValues: disabledFeatures.map { key in (key, JSONValue.bool(false)) }),
            known: GreetingConfig(
                webSearch: "disabled",
                projectDocMaxBytes: 0,
                mcpServers: (config.mcpServers ?? [:]).mapValues { _ in GreetingConfig.Server(enabled: false) },
            ),
        )
    }
}

nonisolated enum CodexAuthFile {
    private struct Document: Decodable, Sendable {
        struct Tokens: Decodable, Sendable {
            let idToken: String?
            let accountID: String?

            enum CodingKeys: String, CodingKey {
                case idToken = "id_token"
                case accountID = "account_id"
            }
        }

        let authMode: String?
        let tokens: Tokens?

        enum CodingKeys: String, CodingKey {
            case tokens
            case authMode = "auth_mode"
        }
    }

    private struct Claims: Decodable, Sendable {
        struct Auth: Decodable, Sendable {
            let chatgptAccountID: String?
            let chatgptUserID: String?
            let userID: String?
            let chatgptPlanType: String?

            enum CodingKeys: String, CodingKey {
                case chatgptAccountID = "chatgpt_account_id"
                case chatgptUserID = "chatgpt_user_id"
                case userID = "user_id"
                case chatgptPlanType = "chatgpt_plan_type"
            }
        }

        let email: String?
        let auth: Auth?

        enum CodingKeys: String, CodingKey {
            case email
            case auth = "https://api.openai.com/auth"
        }
    }

    static let name: String = "auth.json"

    static func read(at url: URL) -> Result<AccountIdentity?, CodexFailure> {
        ifPresent { try Data(contentsOf: url) }
            .mapError(CodexFailure.storage)
            .flatMap { data in
                data.map { data in
                    Result { try JSONDecoder().decode(Document.self, from: data) }
                        .mapError { _ in .invalidResponse(field: name) }
                        .flatMap(parse)
                } ?? .success(nil)
            }
    }

    private static func parse(_ document: Document) -> Result<AccountIdentity?, CodexFailure> {
        if let mode: String = document.authMode, CodexProtocol.AuthMode(rawValue: mode) != .chatgpt {
            return .failure(.subscriptionRequired)
        }
        return
            if let tokens: Document.Tokens = document.tokens, let idToken: String = tokens.idToken
        {
            claims(idToken)
                .flatMap { claims -> Result<(String, String, String, String?), CodexFailure> in
                    let workspace: Result<String, AggregateError<String>> =
                        (claims.auth?.chatgptAccountID ?? tokens.accountID)
                        .map(Result.success) ?? .failure(AggregateError(first: "workspace identifier", remaining: []))
                    let user: Result<String, AggregateError<String>> =
                        (claims.auth?.chatgptUserID ?? claims.auth?.userID)
                        .map(Result.success) ?? .failure(AggregateError(first: "user identifier", remaining: []))
                    let email: Result<String, AggregateError<String>> =
                        claims.email.map(Result.success) ?? .failure(AggregateError(first: "account email", remaining: []))
                    return combine(user, workspace, email)
                        .mapError { failure in .invalidResponse(field: failure.errors.joined(separator: ", ")) }
                        .map { user, workspace, email in (user, workspace, email, claims.auth?.chatgptPlanType) }
                }
                .flatMap { user, workspace, email, plan in
                    AccountIdentity.make(accountID: user, organizationID: workspace, email: email, plan: plan)
                        .mapError { _ in .invalidResponse(field: "account identity") }
                }
                .map(Optional.some)
        } else {
            .success(nil)
        }
    }

    private static func claims(_ token: String) -> Result<Claims, CodexFailure> {
        let components: [Substring] = token.split(separator: ".")
        let unreadable: CodexFailure = .invalidResponse(field: "account identity token")
        guard components.count == 3 else { return .failure(unreadable) }
        let encoded: String = String(components[1]).replacing("-", with: "+").replacing("_", with: "/")
        let padded: String = encoded + String(repeating: "=", count: (4 - encoded.count % 4) % 4)
        return Data(base64Encoded: padded).map { data in
            Result { try JSONDecoder().decode(Claims.self, from: data) }.mapError { _ in unreadable }
        } ?? .failure(unreadable)
    }
}
