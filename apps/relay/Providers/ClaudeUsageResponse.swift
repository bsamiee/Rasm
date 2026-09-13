import Foundation

nonisolated struct ClaudeUsageResponse: Decodable, Sendable {
  struct Window: Decodable, Sendable {
    let utilization: Double?
    let resetsAt: String?

    enum CodingKeys: String, CodingKey {
      case utilization
      case resetsAt = "resets_at"
    }

    func quotaWindow() -> Result<QuotaWindow?, ClaudeQuotaErrors> {
      utilization.map { utilization in
        combine(
          UsageAmount.make(percent: utilization).mapError { failure in ClaudeQuotaErrors(failure) },
          resetDate()
        ).map { amount, reset in QuotaWindow(amount: amount, resetsAt: reset) }
      } ?? .success(nil)
    }

    private func resetDate() -> Result<Date?, ClaudeQuotaErrors> {
      resetsAt.map { value in
        ClaudeUsageResponse.date(value).map(Optional.some).mapError { failure in
          ClaudeQuotaErrors(failure)
        }
      } ?? .success(nil)
    }
  }

  struct Limit: Decodable, Sendable {
    struct Scope: Decodable, Sendable {
      struct Model: Decodable, Sendable {
        let displayName: String?
        enum CodingKeys: String, CodingKey { case displayName = "display_name" }
      }
      let model: Model?
    }

    let kind: String
    let percent: Double?
    let resetsAt: String?
    let scope: Scope?

    enum CodingKeys: String, CodingKey {
      case kind, percent, scope
      case resetsAt = "resets_at"
    }
  }

  let fiveHour: Window?
  let sevenDay: Window?
  let sevenDayOverageIncluded: Window?
  let limits: [Limit]?

  enum CodingKeys: String, CodingKey {
    case fiveHour = "five_hour"
    case sevenDay = "seven_day"
    case sevenDayOverageIncluded = "seven_day_overage_included"
    case limits
  }

  func snapshot(observedAt: Date) -> Result<UsageSnapshot, ClaudeFailure> {
    let session: Result<QuotaWindow?, ClaudeQuotaErrors> =
      fiveHour?.quotaWindow() ?? .success(nil)
    let weekly: Result<QuotaWindow?, ClaudeQuotaErrors> =
      sevenDay?.quotaWindow() ?? .success(nil)
    let scoped: Limit? = limits?.first { limit in
      limit.kind == "weekly_scoped"
        && limit.scope?.model?.displayName?.hasPrefix("Fable") == true
    }
    let fable: Result<QuotaWindow?, ClaudeQuotaErrors> =
      switch scoped {
      case .some(let limit):
        Window(utilization: limit.percent, resetsAt: limit.resetsAt).quotaWindow()
      case .none: sevenDayOverageIncluded?.quotaWindow() ?? .success(nil)
      }
    return combine(session, weekly, fable).mapError(ClaudeFailure.invalidQuota).flatMap {
      session, weekly, fable in
      session == nil && weekly == nil && fable == nil
        ? .failure(.invalidResponse)
        : .success(
          UsageSnapshot(session: session, weekly: weekly, fable: fable, observedAt: observedAt))
    }
  }

  private static func date(_ value: String) -> Result<Date, QuotaFailure> {
    Result {
      try Date.ISO8601FormatStyle(includingFractionalSeconds: value.contains(".")).parse(value)
    }
    .mapError { _ in .invalidResetDate }
  }
}
