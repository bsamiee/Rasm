import Foundation

struct ClaudeUsageResponse: Decodable, Sendable {
  struct Window: Decodable, Sendable {
    let utilization: Double?
    let resetsAt: String?

    enum CodingKeys: String, CodingKey {
      case utilization
      case resetsAt = "resets_at"
    }

    func window() -> Result<QuotaWindow?, ClaudeQuotaIssues> {
      guard let utilization else { return .success(nil) }
      let amount: Result<UsageAmount, ClaudeQuotaIssues> =
        UsageAmount.make(percent: utilization).mapError { failure in ClaudeQuotaIssues(failure) }
      let reset: Result<Date?, ClaudeQuotaIssues> =
        resetsAt.map { value in
          ClaudeUsageResponse.date(value).map(Optional.some).mapError { failure in
            ClaudeQuotaIssues(failure)
          }
        } ?? .success(nil)
      return combine(amount, reset).map { amount, reset in
        QuotaWindow(amount: amount, resetsAt: reset)
      }
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

    func window() -> Result<QuotaWindow?, ClaudeQuotaIssues> {
      Window(utilization: percent, resetsAt: resetsAt).window()
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
    let session: Result<QuotaWindow?, ClaudeQuotaIssues> = fiveHour?.window() ?? .success(nil)
    let weekly: Result<QuotaWindow?, ClaudeQuotaIssues> = sevenDay?.window() ?? .success(nil)
    let scoped: Limit? = limits?.first { limit in
      limit.kind == "weekly_scoped"
        && limit.scope?.model?.displayName?.hasPrefix("Fable") == true
    }
    let fable: Result<QuotaWindow?, ClaudeQuotaIssues> =
      switch scoped {
      case .some(let limit): limit.window()
      case .none: sevenDayOverageIncluded?.window() ?? .success(nil)
      }
    return combine(session, weekly, fable).mapError(ClaudeFailure.invalidQuota).flatMap {
      session, weekly, fable in
      guard session != nil || weekly != nil || fable != nil else {
        return .failure(.invalidResponse)
      }
      return .success(
        UsageSnapshot(session: session, weekly: weekly, fable: fable, observedAt: observedAt))
    }
  }

  private static func date(_ value: String) -> Result<Date, QuotaFailure> {
    do {
      let strategy: Date.ISO8601FormatStyle = .init(includingFractionalSeconds: value.contains("."))
      return .success(try strategy.parse(value))
    } catch {
      return .failure(.invalidResetDate)
    }
  }
}
