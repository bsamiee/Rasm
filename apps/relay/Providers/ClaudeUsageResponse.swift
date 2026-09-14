import Foundation

nonisolated struct ClaudeUsageResponse: Decodable, Sendable {
  struct Window: Decodable, Sendable {
    let utilization: Double?
    let resetsAt: String?
    let status: String?

    enum CodingKeys: String, CodingKey {
      case utilization, status
      case resetsAt = "resets_at"
    }

    func quotaWindow(kind: QuotaKind) -> Result<QuotaWindow?, ClaudeQuotaErrors> {
      utilization.map { utilization in
        combine(
          UsageAmount.make(percent: utilization).mapError(ClaudeQuotaErrors.init),
          ClaudeUsageResponse.resetDate(resetsAt)
        ).map { amount, reset in
          QuotaWindow(kind: kind, used: amount, resetsAt: reset, rejected: status == "rejected")
        }
      } ?? .success(nil)
    }
  }

  struct Limit: Decodable, Sendable {
    struct Scope: Decodable, Sendable {
      struct Model: Decodable, Sendable {
        let displayName: String?
        let id: String?
        enum CodingKeys: String, CodingKey {
          case id
          case displayName = "display_name"
        }
      }
      let model: Model?
    }

    let kind: String
    let group: String?
    let percent: Double?
    let utilization: Double?
    let resetsAt: String?
    let status: String?
    let scope: Scope?

    enum CodingKeys: String, CodingKey {
      case kind, group, percent, utilization, scope, status
      case resetsAt = "resets_at"
    }

    var modelName: String? {
      scope?.model.flatMap { model in model.displayName ?? model.id }
    }

    func quotaWindow() -> Result<QuotaWindow?, ClaudeQuotaErrors> {
      guard kind == "weekly_scoped", let name: String = modelName else { return .success(nil) }
      return Window(utilization: percent ?? utilization, resetsAt: resetsAt, status: status)
        .quotaWindow(kind: .model(name))
    }
  }

  let fiveHour: Window?
  let sevenDay: Window?
  let limits: [Limit]?

  enum CodingKeys: String, CodingKey {
    case fiveHour = "five_hour"
    case sevenDay = "seven_day"
    case limits
  }

  func usage(observedAt: Date) -> Result<AccountUsage, ClaudeFailure> {
    let session: Result<QuotaWindow?, ClaudeQuotaErrors> =
      fiveHour?.quotaWindow(kind: .session) ?? .success(nil)
    let weekly: Result<QuotaWindow?, ClaudeQuotaErrors> =
      sevenDay?.quotaWindow(kind: .weekly) ?? .success(nil)
    let models: Result<[QuotaWindow?], ClaudeQuotaErrors> = traverse(limits ?? []) { limit in
      limit.quotaWindow()
    }
    return combine(session, weekly, models).mapError(ClaudeFailure.invalidQuota).flatMap {
      session, weekly, models in
      let windows: [QuotaWindow] = [session, weekly].compactMap { $0 } + models.compactMap { $0 }
      return windows.isEmpty
        ? .failure(.invalidResponse)
        : .success(
          AccountUsage(windows: windows, includedUsageAllowed: nil, observedAt: observedAt))
    }
  }

  static func resetDate(_ value: String?) -> Result<Date?, ClaudeQuotaErrors> {
    value.map { value in
      Result {
        try Date.ISO8601FormatStyle(includingFractionalSeconds: value.contains(".")).parse(value)
      }
      .map(Optional.some)
      .mapError { _ in ClaudeQuotaErrors(.invalidResetDate) }
    } ?? .success(nil)
  }
}
