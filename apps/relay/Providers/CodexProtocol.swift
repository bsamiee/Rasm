import Foundation

enum CodexProtocol {
  struct AccountUsage: Sendable {
    let snapshot: UsageSnapshot
    let permitsIncludedUsage: Bool?
  }

  struct StarterModel: Sendable {
    let name: String
    let effort: String
  }

  // Published subscription pricing gives Luna the most messages per window of every current model.
  static let starterModelName: String = "gpt-5.6-luna"

  // Least costly first, the model's advertised efforts select the first member it supports.
  private static let effortLadder: [String] = [
    "none", "minimal", "low", "medium", "high", "xhigh", "max", "ultra",
  ]

  static func identity(_ connection: CodexConnection) async -> Result<AccountIdentity, CodexFailure>
  {
    return await connection.request("account/read", params: .object(["refreshToken": .bool(false)]))
      .flatMap(chatgptAccount)
      .bind { account in
        await connection.request(
          "getAuthStatus",
          params: .object(["includeToken": .bool(true), "refreshToken": .bool(false)])
        )
        .flatMap(chatgptToken)
        .flatMap(authenticationClaims)
        .flatMap { claims in
          AccountIdentity.make(
            accountID: claims.userID,
            organizationID: claims.workspaceID,
            email: account.email,
            plan: account.plan
          ).mapError { _ in .invalidResponse(field: "account identity") }
        }
      }
  }

  static func identity(
    _ connection: CodexConnection,
    matching account: RelayAccount
  ) async -> Result<AccountIdentity, CodexFailure> {
    return await identity(connection).flatMap { identity in
      identity.identifies(account.identity) ? .success(identity) : .failure(.identityChanged)
    }
  }

  static func usage(
    _ connection: CodexConnection,
    identity: AccountIdentity
  ) async -> Result<AccountUsage, CodexFailure> {
    return await connection.request(
      "account/rateLimits/read",
      params: .object(["excludeResetCreditDetails": .bool(true)])
    ).flatMap { response in
      usage(response, identity: identity, observedAt: Date())
    }
  }

  static func usage(
    _ response: JSONValue,
    identity: AccountIdentity,
    observedAt: Date
  ) -> Result<AccountUsage, CodexFailure> {
    switch response["accountId"] {
    case .none, .some(.null): break
    case .some(.string(let workspaceID)) where workspaceID == identity.organizationID: break
    case .some: return .failure(.invalidResponse(field: "usage workspace identifier"))
    }
    let permitsIncludedUsage: Bool?
    switch response["ordinaryUsageAllowed"] {
    case .none, .some(.null): permitsIncludedUsage = nil
    case .some(.bool(let value)): permitsIncludedUsage = value
    case .some: return .failure(.invalidResponse(field: "included usage availability"))
    }
    let limits: JSONValue?
    switch response["rateLimitsByLimitId"] {
    case .some(.object(let values)): limits = values["codex"]
    case .none, .some(.null):
      guard let primary: JSONValue = response["rateLimits"], primary.objectValue != nil else {
        return .failure(.invalidResponse(field: "usage limits"))
      }
      switch primary["limitId"] {
      case .none, .some(.null), .some(.string("codex")): limits = primary
      case .some(.string): limits = nil
      case .some: return .failure(.invalidResponse(field: "usage limit identifier"))
      }
    case .some: return .failure(.invalidResponse(field: "usage limits"))
    }
    var session: QuotaWindow?
    var weekly: QuotaWindow?
    var failures: [String] = []
    for field: String in ["primary", "secondary"] {
      guard let value: JSONValue = limits?[field], value != .null else { continue }
      switch window(value, field: field) {
      case .failure(let error): failures.append(error.field)
      case .success((let duration, let window)):
        switch duration {
        case 300: session = window
        case 10_080: weekly = window
        default: break
        }
      }
    }
    if !failures.isEmpty {
      return .failure(.invalidResponse(field: failures.joined(separator: ", ")))
    }
    return .success(
      AccountUsage(
        snapshot: UsageSnapshot(
          session: session, weekly: weekly, fable: nil, observedAt: observedAt),
        permitsIncludedUsage: permitsIncludedUsage
      ))
  }

  static func starterModel(_ connection: CodexConnection) async -> Result<
    StarterModel, CodexFailure
  > {
    var cursor: String?
    repeat {
      var parameters: [String: JSONValue] = ["includeHidden": .bool(false)]
      if let cursor { parameters["cursor"] = .string(cursor) }
      let response: JSONValue
      switch await connection.request("model/list", params: .object(parameters)) {
      case .success(let value): response = value
      case .failure(let error): return .failure(error)
      }
      guard let models: [JSONValue] = response["data"]?.arrayValue else {
        return .failure(.invalidResponse(field: "model list"))
      }
      if let model: JSONValue = models.first(where: { value in
        value["model"]?.stringValue == starterModelName && value["hidden"]?.boolValue != true
      }) {
        return starterEffort(model).map { effort in
          StarterModel(name: starterModelName, effort: effort)
        }
      }
      cursor = response["nextCursor"]?.stringValue
    } while cursor != nil
    return .failure(.modelUnavailable)
  }

  static func sendGreeting(
    _ connection: CodexConnection,
    model: StarterModel,
    workingDirectory: URL
  ) async -> Result<Void, CodexFailure> {
    return await connection.request(
      "config/read",
      params: .object(["includeLayers": .bool(false), "cwd": .string(workingDirectory.path)])
    )
    .flatMap(greetingOverrides)
    .bind { overrides in
      await connection.request(
        "thread/start",
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
    .bind { (started: JSONValue) async -> Result<Void, CodexFailure> in
      guard let threadID: String = started["thread"]?["id"]?.stringValue else {
        return .failure(.invalidResponse(field: "temporary thread identifier"))
      }
      let outcome: Result<Void, CodexFailure> = await completeGreeting(
        connection, threadID: threadID, model: model
      )
      .mapError(CodexFailure.sessionConfirmationPending)
      let detached: Result<JSONValue, CodexFailure> = await connection.request(
        "thread/unsubscribe", params: .object(["threadId": .string(threadID)])
      )
      return outcome.flatMap { _ in
        detached.map { _ in () }.mapError(CodexFailure.sessionConfirmationPending)
      }
    }
  }

  private struct ChatgptAccount {
    let email: String
    let plan: String?
  }

  private struct AuthenticationClaims {
    let userID: String
    let workspaceID: String
  }

  private static func chatgptAccount(_ response: JSONValue) -> Result<ChatgptAccount, CodexFailure>
  {
    guard let account: JSONValue = response["account"], account != .null else {
      return .failure(.signInRequired)
    }
    guard account["type"]?.stringValue == "chatgpt" else { return .failure(.subscriptionRequired) }
    guard let email: String = account["email"]?.stringValue else {
      return .failure(.invalidResponse(field: "account email"))
    }
    return .success(ChatgptAccount(email: email, plan: account["planType"]?.stringValue))
  }

  private static func chatgptToken(_ response: JSONValue) -> Result<String, CodexFailure> {
    guard let method: String = response["authMethod"]?.stringValue else {
      return .failure(.signInRequired)
    }
    guard method == "chatgpt" else { return .failure(.subscriptionRequired) }
    guard let token: String = response["authToken"]?.stringValue else {
      return .failure(.signInRequired)
    }
    return .success(token)
  }

  // Codex's own JWT reader takes `chatgpt_user_id`, then `user_id`, under the auth claim.
  private static func authenticationClaims(_ token: String) -> Result<
    AuthenticationClaims, CodexFailure
  > {
    let components: [Substring] = token.split(separator: ".")
    guard components.count == 3 else {
      return .failure(.invalidResponse(field: "account identity"))
    }
    let encoded: String = String(components[1]).replacingOccurrences(of: "-", with: "+")
      .replacingOccurrences(of: "_", with: "/")
    let padded: String = encoded + String(repeating: "=", count: (4 - encoded.count % 4) % 4)
    guard let data: Data = Data(base64Encoded: padded),
      let claims: JSONValue = try? JSONDecoder().decode(JSONValue.self, from: data)
    else {
      return .failure(.invalidResponse(field: "account identity"))
    }
    guard let auth: [String: JSONValue] = claims["https://api.openai.com/auth"]?.objectValue,
      let workspaceID: String = auth["chatgpt_account_id"]?.stringValue
    else {
      return .failure(.invalidResponse(field: "workspace identifier"))
    }
    switch (auth["chatgpt_user_id"], auth["user_id"]) {
    case (.some(.string(let userID)), _), (.none, .some(.string(let userID))),
      (.some(.null), .some(.string(let userID))):
      return .success(AuthenticationClaims(userID: userID, workspaceID: workspaceID))
    default:
      return .failure(.invalidResponse(field: "user identifier"))
    }
  }

  private static func starterEffort(_ model: JSONValue) -> Result<String, CodexFailure> {
    guard let efforts: [JSONValue] = model["supportedReasoningEfforts"]?.arrayValue else {
      return .failure(.invalidResponse(field: "model reasoning effort"))
    }
    let supported: Set<String> = Set(
      efforts.compactMap { value in value["reasoningEffort"]?.stringValue })
    guard let effort: String = effortLadder.first(where: supported.contains) else {
      return .failure(.invalidResponse(field: "model reasoning effort"))
    }
    return .success(effort)
  }

  private static func greetingOverrides(_ effective: JSONValue) -> Result<
    [String: JSONValue], CodexFailure
  > {
    guard let config: [String: JSONValue] = effective["config"]?.objectValue else {
      return .failure(.invalidResponse(field: "effective configuration"))
    }
    let servers: [String: JSONValue]
    switch config["mcp_servers"] {
    case .none, .some(.null): servers = [:]
    case .some(.object(let values)):
      servers = values.mapValues { _ in .object(["enabled": .bool(false)]) }
    case .some: return .failure(.invalidResponse(field: "MCP configuration"))
    }
    var overrides: [String: JSONValue] = Dictionary(
      uniqueKeysWithValues: disabledFeatures.map { key in (key, .bool(false)) })
    overrides["web_search"] = .string("disabled")
    overrides["project_doc_max_bytes"] = .number(0)
    overrides["mcp_servers"] = .object(servers)
    return .success(overrides)
  }

  private static func completeGreeting(
    _ connection: CodexConnection,
    threadID: String,
    model: StarterModel
  ) async -> Result<Void, CodexFailure> {
    return await connection.request(
      "turn/start",
      params: .object([
        "threadId": .string(threadID),
        "input": .array([.object(["type": .string("text"), "text": .string("hi")])]),
        "effort": .string(model.effort),
        "serviceTierForTurn": .string("default"),
      ])
    )
    .bind { (response: JSONValue) async -> Result<JSONValue, CodexFailure> in
      guard let turnID: String = response["turn"]?["id"]?.stringValue else {
        return .failure(.invalidResponse(field: "temporary turn identifier"))
      }
      return await connection.notification("turn/completed") { value in
        value["threadId"]?.stringValue == threadID && value["turn"]?["id"]?.stringValue == turnID
      }
    }
    .flatMap { (completed: JSONValue) -> Result<Void, CodexFailure> in
      let turn: JSONValue? = completed["turn"]
      switch turn?["status"]?.stringValue {
      case "completed": return .success(())
      case "interrupted": return .failure(.cancelled)
      case "failed":
        let error: JSONValue? = turn?["error"]
        return .failure(
          .turnFailed(
            code: error?["codexErrorInfo"]?.stringValue.flatMap(CodexTurnErrorCode.init),
            message: error?["message"]?.stringValue ?? "OpenAI could not start the session."))
      default: return .failure(.invalidResponse(field: "turn completion"))
      }
    }
  }

  private static func window(
    _ value: JSONValue,
    field: String
  ) -> Result<(Int?, QuotaWindow), CodexWindowFailure> {
    let duration: Int?
    switch value["windowDurationMins"] {
    case .none, .some(.null): duration = nil
    case .some(let number):
      guard let minutes: Int = number.intValue, minutes > 0 else {
        return .failure(CodexWindowFailure(field: "\(field) window duration"))
      }
      duration = minutes
    }
    guard let percent: Double = value["usedPercent"]?.doubleValue,
      case .success(let amount) = UsageAmount.make(percent: percent)
    else {
      return .failure(CodexWindowFailure(field: "\(field) usage percentage"))
    }
    let resetsAt: Date?
    switch value["resetsAt"] {
    case .none, .some(.null): resetsAt = nil
    case .some(.number(let seconds)) where seconds.isFinite:
      resetsAt = Date(timeIntervalSince1970: seconds)
    case .some: return .failure(CodexWindowFailure(field: "\(field) reset time"))
    }
    return .success((duration, QuotaWindow(amount: amount, resetsAt: resetsAt)))
  }

  // Codex's temporary_structured_request disables the owning tool capabilities at thread creation.
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

private struct CodexWindowFailure: Error {
  let field: String
}
