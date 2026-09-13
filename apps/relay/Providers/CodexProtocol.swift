import Foundation

nonisolated enum CodexProtocol {
  struct AccountUsage: Sendable {
    let snapshot: UsageSnapshot
    let permitsIncludedUsage: Bool?
  }

  struct GreetingModel: Sendable {
    let name: String
    let effort: String
  }

  static let greetingModelName: String = "gpt-5.6-luna"

  private static let reasoningEfforts: [String] = [
    "none", "minimal", "low", "medium", "high", "xhigh", "max", "ultra",
  ]

  static func identity(_ connection: CodexConnection) async -> Result<AccountIdentity, CodexFailure>
  {
    return await connection.request("account/read", params: .object(["refreshToken": .bool(false)]))
      .flatMap(chatGPTAccount)
      .bind { account in
        await connection.request(
          "getAuthStatus",
          params: .object(["includeToken": .bool(true), "refreshToken": .bool(false)])
        )
        .flatMap(chatGPTToken)
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
    matching account: Account
  ) async -> Result<AccountIdentity, CodexFailure> {
    return await identity(connection).flatMap { identity in
      identity.isSameAccount(as: account.identity) ? .success(identity) : .failure(.identityChanged)
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
    let workspace: Result<Void, CodexFieldFailure> =
      switch response["accountId"] {
      case .none, .some(.null): .success(())
      case .some(.string(let workspaceID)) where workspaceID == identity.organizationID:
        .success(())
      case .some: .failure(CodexFieldFailure("usage workspace identifier"))
      }
    let permitsIncludedUsage: Result<Bool?, CodexFieldFailure> =
      switch response["ordinaryUsageAllowed"] {
      case .none, .some(.null): .success(nil)
      case .some(.bool(let value)): .success(value)
      case .some: .failure(CodexFieldFailure("included usage availability"))
      }
    let limits: Result<JSONValue?, CodexFieldFailure> =
      switch response["rateLimitsByLimitId"] {
      case .some(.object(let values)): .success(values["codex"])
      case .none, .some(.null):
        switch response["rateLimits"] {
        case .some(.object(let primary)):
          switch primary["limitId"] {
          case .none, .some(.null), .some(.string("codex")): .success(.object(primary))
          case .some(.string): .success(nil)
          case .some: .failure(CodexFieldFailure("usage limit identifier"))
          }
        case .none, .some: .failure(CodexFieldFailure("usage limits"))
        }
      case .some: .failure(CodexFieldFailure("usage limits"))
      }
    return combine(workspace, permitsIncludedUsage, limits)
      .flatMap { _, permitsIncludedUsage, limits in
        traverse(
          ["primary", "secondary"].compactMap { field in
            limits?[field].flatMap { value in value == .null ? nil : (field, value) }
          }
        ) { field, value in quotaWindow(value, field: field) }
        .map { windows in
          AccountUsage(
            snapshot: UsageSnapshot(
              session: windows.last { duration, _ in duration == 300 }?.1,
              weekly: windows.last { duration, _ in duration == 10_080 }?.1,
              fable: nil,
              observedAt: observedAt
            ),
            permitsIncludedUsage: permitsIncludedUsage
          )
        }
      }
      .mapError { failure in .invalidResponse(field: failure.errors.joined(separator: ", ")) }
  }

  static func greetingModel(
    _ connection: CodexConnection,
    cursor: String? = nil
  ) async -> Result<GreetingModel, CodexFailure> {
    var parameters: [String: JSONValue] = ["includeHidden": .bool(false)]
    if let cursor { parameters["cursor"] = .string(cursor) }
    return await connection.request("model/list", params: .object(parameters)).bind { response in
      guard let models: [JSONValue] = response["data"]?.arrayValue else {
        return .failure(.invalidResponse(field: "model list"))
      }
      if let model: JSONValue = models.first(where: { value in
        value["model"]?.stringValue == greetingModelName && value["hidden"]?.boolValue != true
      }) {
        return greetingEffort(model).map { effort in
          GreetingModel(name: greetingModelName, effort: effort)
        }
      }
      return await
        (response["nextCursor"]?.stringValue.map(Result<String, CodexFailure>.success)
        ?? .failure(.modelUnavailable))
        .bind { next in await greetingModel(connection, cursor: next) }
    }
  }

  static func sendGreeting(
    _ connection: CodexConnection,
    model: GreetingModel,
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

  private struct ChatGPTAccount {
    let email: String
    let plan: String?
  }

  private struct AuthenticationClaims {
    let userID: String
    let workspaceID: String
  }

  private static func chatGPTAccount(_ response: JSONValue) -> Result<ChatGPTAccount, CodexFailure>
  {
    guard let account: JSONValue = response["account"], account != .null else {
      return .failure(.signInRequired)
    }
    return account["type"]?.stringValue == "chatgpt"
      ? account["email"]?.stringValue.map { email in
        .success(ChatGPTAccount(email: email, plan: account["planType"]?.stringValue))
      } ?? .failure(.invalidResponse(field: "account email"))
      : .failure(.subscriptionRequired)
  }

  private static func chatGPTToken(_ response: JSONValue) -> Result<String, CodexFailure> {
    response["authMethod"]?.stringValue.map { method in
      method == "chatgpt"
        ? response["authToken"]?.stringValue.map { token in .success(token) }
          ?? .failure(.signInRequired)
        : .failure(.subscriptionRequired)
    } ?? .failure(.signInRequired)
  }

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
    return claims["https://api.openai.com/auth"]?.objectValue.map(authenticationClaims(auth:))
      ?? .failure(.invalidResponse(field: "workspace identifier"))
  }

  private static func authenticationClaims(
    auth: [String: JSONValue]
  ) -> Result<AuthenticationClaims, CodexFailure> {
    let workspaceID: Result<String, CodexFieldFailure> =
      auth["chatgpt_account_id"]?.stringValue.map { workspaceID in .success(workspaceID) }
      ?? .failure(CodexFieldFailure("workspace identifier"))
    let userID: Result<String, CodexFieldFailure> =
      switch (auth["chatgpt_user_id"], auth["user_id"]) {
      case (.some(.string(let userID)), _), (.none, .some(.string(let userID))),
        (.some(.null), .some(.string(let userID))):
        .success(userID)
      default: .failure(CodexFieldFailure("user identifier"))
      }
    return combine(userID, workspaceID)
      .map { userID, workspaceID in AuthenticationClaims(userID: userID, workspaceID: workspaceID) }
      .mapError { failure in .invalidResponse(field: failure.errors.joined(separator: ", ")) }
  }

  private static func greetingEffort(_ model: JSONValue) -> Result<String, CodexFailure> {
    let supported: Set<String> = Set(
      (model["supportedReasoningEfforts"]?.arrayValue ?? []).compactMap { value in
        value["reasoningEffort"]?.stringValue
      })
    return reasoningEfforts.first(where: supported.contains).map { effort in .success(effort) }
      ?? .failure(.invalidResponse(field: "model reasoning effort"))
  }

  private static func greetingOverrides(_ effective: JSONValue) -> Result<
    [String: JSONValue], CodexFailure
  > {
    effective["config"]?.objectValue.map(greetingOverrides(config:))
      ?? .failure(.invalidResponse(field: "effective configuration"))
  }

  private static func greetingOverrides(
    config: [String: JSONValue]
  ) -> Result<[String: JSONValue], CodexFailure> {
    let servers: Result<[String: JSONValue], CodexFailure> =
      switch config["mcp_servers"] {
      case .none, .some(.null): .success([:])
      case .some(.object(let values)):
        .success(values.mapValues { _ in .object(["enabled": .bool(false)]) })
      case .some: .failure(.invalidResponse(field: "MCP configuration"))
      }
    return servers.map { servers in
      var overrides: [String: JSONValue] = Dictionary(
        uniqueKeysWithValues: disabledFeatures.map { key in (key, .bool(false)) })
      overrides["web_search"] = .string("disabled")
      overrides["project_doc_max_bytes"] = .number(0)
      overrides["mcp_servers"] = .object(servers)
      return overrides
    }
  }

  private static func completeGreeting(
    _ connection: CodexConnection,
    threadID: String,
    model: GreetingModel
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
      return await
        (response["turn"]?["id"]?.stringValue.map(Result<String, CodexFailure>.success)
        ?? .failure(.invalidResponse(field: "temporary turn identifier")))
        .bind { turnID in
          await connection.notification("turn/completed") { value in
            value["threadId"]?.stringValue == threadID
              && value["turn"]?["id"]?.stringValue == turnID
          }
        }
    }
    .flatMap { (completed: JSONValue) -> Result<Void, CodexFailure> in
      let turn: JSONValue? = completed["turn"]
      switch turn?["status"]?.stringValue {
      case "completed": return .success(())
      case "interrupted": return .failure(.cancelled)
      case "failed":
        let error: JSONValue? = turn?["error"]
        return error?["message"]?.stringValue.map { message in
          .failure(
            .turnFailed(
              code: error?["codexErrorInfo"]?.stringValue.flatMap(CodexTurnErrorCode.init),
              message: message))
        } ?? .failure(.invalidResponse(field: "turn error"))
      default: return .failure(.invalidResponse(field: "turn completion"))
      }
    }
  }

  private static func quotaWindow(
    _ value: JSONValue,
    field: String
  ) -> Result<(Int?, QuotaWindow), CodexFieldFailure> {
    let duration: Result<Int?, CodexFieldFailure> =
      switch value["windowDurationMins"] {
      case .none, .some(.null): .success(nil)
      case .some(let number):
        number.intValue.flatMap { minutes in minutes > 0 ? minutes : nil }
          .map { minutes in .success(minutes) }
          ?? .failure(CodexFieldFailure("\(field) window duration"))
      }
    let amount: Result<UsageAmount, CodexFieldFailure> =
      (value["usedPercent"]?.doubleValue).map { percent in
        UsageAmount.make(percent: percent).mapError { _ in
          CodexFieldFailure("\(field) usage percentage")
        }
      } ?? .failure(CodexFieldFailure("\(field) usage percentage"))
    let resetsAt: Result<Date?, CodexFieldFailure> =
      switch value["resetsAt"] {
      case .none, .some(.null): .success(nil)
      case .some(.number(let seconds)) where seconds.isFinite:
        .success(Date(timeIntervalSince1970: seconds))
      case .some: .failure(CodexFieldFailure("\(field) reset time"))
      }
    return combine(duration, amount, resetsAt).map { duration, amount, resetsAt in
      (duration, QuotaWindow(amount: amount, resetsAt: resetsAt))
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
