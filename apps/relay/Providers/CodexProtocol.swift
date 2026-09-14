import Foundation

nonisolated enum CodexProtocol {
  struct RateLimitWindow: Decodable, Sendable {
    let usedPercent: Double
    let windowDurationMins: Int?
    let resetsAt: Double?
  }

  struct RateLimitSnapshot: Decodable, Sendable {
    let limitId: String?
    let primary: RateLimitWindow?
    let secondary: RateLimitWindow?
    let rateLimitReachedType: String?
    let spendControlReached: Bool?
  }

  struct AccountRateLimits: Decodable, Sendable {
    let accountId: String?
    let ordinaryUsageAllowed: Bool?
    let rateLimits: RateLimitSnapshot
    let rateLimitsByLimitId: [String: RateLimitSnapshot]?

    var codexLimits: RateLimitSnapshot? {
      if let byID: [String: RateLimitSnapshot] = rateLimitsByLimitId { return byID["codex"] }
      return rateLimits.limitId == nil || rateLimits.limitId == "codex" ? rateLimits : nil
    }
  }

  struct RateLimitsUpdated: Decodable, Sendable {
    let rateLimits: RateLimitSnapshot
  }

  struct ReasoningEffortOption: Decodable, Sendable {
    let reasoningEffort: String
  }

  struct Model: Decodable, Sendable {
    let id: String
    let model: String
    let hidden: Bool
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

  struct TurnError: Decodable, Sendable {
    let message: String
    let codexErrorInfo: JSONValue?
  }

  struct Turn: Decodable, Sendable {
    let id: String
    let status: String
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
    let type: String
    let loginId: String?
    let authUrl: String?
  }

  struct LoginCompleted: Decodable, Sendable {
    let loginId: String?
    let success: Bool
    let error: String?
  }

  struct EffectiveConfig: Decodable, Sendable {
    struct Config: Decodable, Sendable {
      let mcpServers: [String: JSONValue]?
      let forcedChatgptWorkspaceId: JSONValue?

      enum CodingKeys: String, CodingKey {
        case mcpServers = "mcp_servers"
        case forcedChatgptWorkspaceId = "forced_chatgpt_workspace_id"
      }
    }
    let config: Config
  }

  struct GreetingModel: Sendable {
    let name: String
    let effort: String
  }

  static let greetingModelName: String = "gpt-5.6-luna"

  private static let reasoningEfforts: [String] = [
    "none", "minimal", "low", "medium", "high", "xhigh", "max", "ultra",
  ]

  static func usage(
    _ response: AccountRateLimits, identity: AccountIdentity, observedAt: Date
  ) -> Result<AccountUsage, CodexFailure> {
    limits(of: response, for: identity).flatMap { limits in
      windows(limits).map { windows in
        AccountUsage(
          windows: windows, includedUsageAllowed: response.ordinaryUsageAllowed,
          observedAt: observedAt)
      }
    }
  }

  private static func limits(
    of response: AccountRateLimits, for identity: AccountIdentity
  ) -> Result<RateLimitSnapshot, CodexFailure> {
    let sameWorkspace: Bool =
      response.accountId == nil || response.accountId == identity.organizationID
    return sameWorkspace
      ? response.codexLimits.map(Result.success)
        ?? .failure(.invalidResponse(field: "usage limits"))
      : .failure(.identityChanged)
  }

  static func windows(_ limits: RateLimitSnapshot) -> Result<[QuotaWindow], CodexFailure> {
    let reached: Bool = limits.rateLimitReachedType != nil || limits.spendControlReached == true
    return traverse([limits.primary, limits.secondary].compactMap { $0 }) { window in
      quotaWindow(window, reached: reached)
    }
    .map { windows in windows.compactMap { $0 } }
    .mapError { failure in .invalidResponse(field: failure.errors.joined(separator: ", ")) }
  }

  private static func quotaWindow(
    _ window: RateLimitWindow, reached: Bool
  ) -> Result<QuotaWindow?, CodexFieldFailure> {
    let kind: QuotaKind? =
      switch window.windowDurationMins {
      case .some(let minutes) where minutes <= 0: nil
      case .some(let minutes) where minutes <= 12 * 60: .session
      case .some: .weekly
      case .none: nil
      }
    guard let kind else { return .success(nil) }
    let amount: Result<UsageAmount, CodexFieldFailure> = UsageAmount.make(
      percent: window.usedPercent
    )
    .mapError { _ in CodexFieldFailure("\(kind.name) usage percentage") }
    let resetsAt: Result<Date?, CodexFieldFailure> =
      switch window.resetsAt {
      case .none: .success(nil)
      case .some(let seconds) where seconds.isFinite: .success(Date(timeIntervalSince1970: seconds))
      case .some: .failure(CodexFieldFailure("\(kind.name) reset time"))
      }
    return combine(amount, resetsAt).map { amount, resetsAt in
      QuotaWindow(
        kind: kind, used: amount, resetsAt: resetsAt, rejected: reached && amount.isExhausted)
    }
  }

  static func greetingModel(
    _ connection: CodexConnection, cursor: String? = nil
  ) async -> Result<GreetingModel, CodexFailure> {
    var parameters: [String: JSONValue] = ["includeHidden": .bool(false)]
    if let cursor { parameters["cursor"] = .string(cursor) }
    return await connection.request(ModelList.self, "model/list", params: .object(parameters))
      .bind { list in
        if let model: Model = list.data.first(where: { value in
          value.model == greetingModelName && !value.hidden
        }) {
          return greetingEffort(model).map { effort in
            GreetingModel(name: greetingModelName, effort: effort)
          }
        }
        return await
          (list.nextCursor.map(Result<String, CodexFailure>.success)
          ?? .failure(.modelUnavailable))
          .bind { next in await greetingModel(connection, cursor: next) }
      }
  }

  static func sendGreeting(
    _ connection: CodexConnection, model: GreetingModel, workingDirectory: URL
  ) async -> Result<Void, CodexFailure> {
    await connection.request(
      EffectiveConfig.self, "config/read",
      params: .object(["includeLayers": .bool(false), "cwd": .string(workingDirectory.path)])
    )
    .map { effective in greetingOverrides(effective.config) }
    .bind { overrides in
      await connection.request(
        ThreadStarted.self, "thread/start",
        params: .object([
          "model": .string(model.name),
          "modelProvider": .string("openai"),
          "cwd": .string(workingDirectory.path),
          "approvalPolicy": .string("never"),
          "sandbox": .string("read-only"),
          "runtimeWorkspaceRoots": .array([]),
          "ephemeral": .bool(true),
          "environments": .array([]),
          "dynamicTools": .array([]),
          "selectedCapabilityRoots": .array([]),
          "baseInstructions": .string("Reply with hi."),
          "developerInstructions": .string(""),
          "config": .object(overrides),
        ]))
    }
    .bind { started -> Result<Void, CodexFailure> in
      let outcome: Result<Void, CodexFailure> = await completeGreeting(
        connection, threadID: started.thread.id, model: model)
      let detached: Result<JSONValue, CodexFailure> = await connection.request(
        "thread/unsubscribe", params: .object(["threadId": .string(started.thread.id)]))
      return outcome.flatMap { _ in detached.map { _ in () } }
    }
  }

  private static func greetingEffort(_ model: Model) -> Result<String, CodexFailure> {
    let supported: Set<String> = Set(model.supportedReasoningEfforts.map(\.reasoningEffort))
    return reasoningEfforts.first(where: supported.contains).map { effort in .success(effort) }
      ?? .failure(.invalidResponse(field: "model reasoning effort"))
  }

  private static func greetingOverrides(_ config: EffectiveConfig.Config) -> [String: JSONValue] {
    var overrides: [String: JSONValue] = Dictionary(
      uniqueKeysWithValues: disabledFeatures.map { key in (key, .bool(false)) })
    overrides["web_search"] = .string("disabled")
    overrides["project_doc_max_bytes"] = .number(0)
    overrides["mcp_servers"] = .object(
      (config.mcpServers ?? [:]).mapValues { _ in .object(["enabled": .bool(false)]) })
    return overrides
  }

  private static func completeGreeting(
    _ connection: CodexConnection, threadID: String, model: GreetingModel
  ) async -> Result<Void, CodexFailure> {
    await connection.request(
      TurnStarted.self, "turn/start",
      params: .object([
        "threadId": .string(threadID),
        "input": .array([.object(["type": .string("text"), "text": .string("hi")])]),
        "effort": .string(model.effort),
        "serviceTierForTurn": .string("default"),
      ])
    )
    .bind { started in
      await connection.notification(TurnCompleted.self, "turn/completed") { completed in
        completed.threadId == threadID && completed.turn.id == started.turn.id
      }
    }
    .flatMap { completed -> Result<Void, CodexFailure> in
      switch completed.turn.status {
      case "completed": .success(())
      case "interrupted": .failure(.cancelled)
      case "failed":
        completed.turn.error.map { error in
          .failure(
            .turnFailed(
              code: error.codexErrorInfo?.stringValue.flatMap(CodexTurnErrorCode.init),
              message: error.message))
        } ?? .failure(.invalidResponse(field: "turn error"))
      default: .failure(.invalidResponse(field: "turn completion"))
      }
    }
  }

  private static let disabledFeatures: [String] = [
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
}

nonisolated struct CodexAuthFile: Sendable {
  let identity: AccountIdentity

  static func read(at url: URL) -> Result<CodexAuthFile?, CodexFailure> {
    Result { try Data(contentsOf: url) }.map(Optional.some)
      .flatMapError { error -> Result<Data?, CodexFailure> in
        (error as? CocoaError)?.code == .fileReadNoSuchFile
          ? .success(nil) : .failure(.storage(error))
      }
      .flatMap { data in
        data.map { data in parse(data).map(Optional.some) } ?? .success(nil)
      }
  }

  private static func parse(_ data: Data) -> Result<CodexAuthFile, CodexFailure> {
    guard let document: JSONValue = try? JSONDecoder().decode(JSONValue.self, from: data) else {
      return .failure(.invalidResponse(field: "auth.json"))
    }
    if let mode: String = document["auth_mode"]?.stringValue, mode != "chatgpt" {
      return .failure(.subscriptionRequired)
    }
    guard let tokens: JSONValue = document["tokens"], tokens != .null,
      let idToken: String = tokens["id_token"]?.stringValue
    else {
      return .failure(.signInRequired)
    }
    return claims(idToken).flatMap { claims in
      let auth: JSONValue? = claims["https://api.openai.com/auth"]
      let workspace: Result<String, CodexFieldFailure> =
        (auth?["chatgpt_account_id"]?.stringValue ?? tokens["account_id"]?.stringValue)
        .map { value in .success(value) } ?? .failure(CodexFieldFailure("workspace identifier"))
      let user: Result<String, CodexFieldFailure> =
        (auth?["chatgpt_user_id"]?.stringValue ?? auth?["user_id"]?.stringValue)
        .map { value in .success(value) } ?? .failure(CodexFieldFailure("user identifier"))
      let email: Result<String, CodexFieldFailure> =
        claims["email"]?.stringValue.map { value in .success(value) }
        ?? .failure(CodexFieldFailure("account email"))
      return combine(user, workspace, email)
        .mapError { failure in .invalidResponse(field: failure.errors.joined(separator: ", ")) }
        .flatMap { user, workspace, email in
          AccountIdentity.make(
            accountID: user, organizationID: workspace, email: email,
            plan: auth?["chatgpt_plan_type"]?.stringValue
          )
          .mapError { _ in .invalidResponse(field: "account identity") }
          .map(CodexAuthFile.init(identity:))
        }
    }
  }

  private static func claims(_ token: String) -> Result<JSONValue, CodexFailure> {
    let components: [Substring] = token.split(separator: ".")
    guard components.count == 3 else {
      return .failure(.invalidResponse(field: "account identity token"))
    }
    let encoded: String = String(components[1]).replacing("-", with: "+").replacing("_", with: "/")
    let padded: String = encoded + String(repeating: "=", count: (4 - encoded.count % 4) % 4)
    guard let data: Data = Data(base64Encoded: padded),
      let claims: JSONValue = try? JSONDecoder().decode(JSONValue.self, from: data)
    else {
      return .failure(.invalidResponse(field: "account identity token"))
    }
    return .success(claims)
  }
}

nonisolated struct CodexConfigFile: Sendable {
  let credentialStore: String?
  let forcesWorkspace: Bool

  static func read(at url: URL) -> Result<CodexConfigFile, CodexFailure> {
    Result { try String(contentsOf: url, encoding: .utf8) }.map(Optional.some)
      .flatMapError { error -> Result<String?, CodexFailure> in
        (error as? CocoaError)?.code == .fileReadNoSuchFile
          ? .success(nil) : .failure(.storage(error))
      }
      .map { text in
        text.map(parse) ?? CodexConfigFile(credentialStore: nil, forcesWorkspace: false)
      }
  }

  private static func parse(_ text: String) -> CodexConfigFile {
    let top: [Substring] = text.split(separator: "\n").prefix { line in
      !line.trimmingCharacters(in: .whitespaces).hasPrefix("[")
    }
    let store: String? = top.lazy.compactMap { line in
      line.firstMatch(of: /^\s*cli_auth_credentials_store\s*=\s*"([^"]*)"/).map { match in
        String(match.1)
      }
    }.first
    let forced: Bool = top.contains { line in
      line.contains(/^\s*forced_chatgpt_workspace_id\s*=/)
    }
    return CodexConfigFile(credentialStore: store, forcesWorkspace: forced)
  }

  var switchable: Result<Void, CodexFailure> {
    guard credentialStore == nil || credentialStore == "file" else {
      return .failure(.keyringStorage)
    }
    return forcesWorkspace ? .failure(.forcedWorkspace) : .success(())
  }
}
