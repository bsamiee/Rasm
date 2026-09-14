import Foundation

nonisolated enum UsagePresentation {
  private static let countdownStyle: Duration.UnitsFormatStyle = Duration.UnitsFormatStyle(
    allowedUnits: [.hours, .minutes], width: .narrow)

  static func percentage(_ amount: UsageAmount) -> String? {
    switch amount.percent {
    case 0: nil
    case ..<1: "<1%"
    default: amount.fraction.formatted(.percent.rounded(rule: .down).precision(.fractionLength(0)))
    }
  }

  static func countdown(until reset: Date, at now: Date) -> String {
    let remaining: Duration = .seconds(reset.timeIntervalSince(now))
    return remaining < .seconds(60) ? "<1m" : remaining.formatted(countdownStyle)
  }

  static func weekday(_ reset: Date) -> String {
    resetTime(reset, weekday: .abbreviated)
  }

  static func resetTooltip(_ reset: Date?) -> String {
    reset.map { reset in resetTime(reset, weekday: .wide) } ?? "No reset"
  }

  private static func resetTime(_ reset: Date, weekday: Date.FormatStyle.Symbol.Weekday) -> String {
    reset.formatted(.dateTime.weekday(weekday).hour().minute())
  }

  static func sessionReading(
    _ window: QuotaWindow?, availability: Availability?, isCurrent: Bool, at now: Date
  ) -> String? {
    let used: String? = window.flatMap { window in percentage(window.used) }
    let state: String? =
      switch availability {
      case .running(let until): countdown(until: until, at: now)
      case .blocked: "Blocked"
      case .ready, .none: "Awaiting update"
      case .noSessionWindow: nil
      }
    guard !isCurrent || availability != .ready else { return nil }
    let parts: [String] = [used, state].compactMap { $0 }
    return parts.isEmpty ? nil : parts.joined(separator: " · ")
  }

  static func reading(_ window: QuotaWindow, at now: Date) -> String {
    let used: String? = percentage(window.used)
    let reset: String? = window.resetsAt.map { reset in
      switch reset.timeIntervalSince(now) {
      case ...0: "Awaiting update"
      case ..<86_400: countdown(until: reset, at: now)
      default: weekday(reset)
      }
    }
    return [used, reset].compactMap { $0 }.joined(separator: " · ")
  }

  static func blockedLine(_ availability: Availability?) -> String? {
    guard case .blocked(let until) = availability else { return nil }
    return until.map { reset in "Blocked until \(weekday(reset))" } ?? "Blocked"
  }

  static func signInLine(expiring expiry: Date?, at now: Date) -> String? {
    guard let expiry, expiry <= now else { return nil }
    return "Login expired"
  }

  static func loginExpiry(_ expiry: Date) -> String {
    expiry.formatted(.dateTime.month(.abbreviated).day())
  }

  static func loginExpiryTooltip(_ expiry: Date) -> String {
    "Login expires \(expiry.formatted(.dateTime.weekday(.wide).month().day().hour().minute()))"
  }
}
