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
    reset.formatted(.dateTime.weekday(.abbreviated).hour().minute())
  }

  static func sessionReading(
    _ window: QuotaWindow?, availability: Availability?, isCurrent: Bool, at now: Date
  ) -> String {
    guard let window else { return isCurrent ? "Ready" : "Awaiting update" }
    let used: String? = percentage(window.used)
    let state: String =
      switch availability {
      case .running(let until): countdown(until: until, at: now)
      case .blocked: "Blocked"
      case .ready, .none: isCurrent ? "Ready" : "Awaiting update"
      }
    return [used, state].compactMap { $0 }.joined(separator: " · ")
  }

  static func reading(_ window: QuotaWindow, at now: Date) -> String {
    let used: String? = percentage(window.used)
    let reset: String? = window.resetsAt.map { reset in
      reset > now ? weekday(reset) : "Awaiting update"
    }
    return [used, reset].compactMap { $0 }.joined(separator: " · ")
  }

  static func sharedResetLine(_ windows: [QuotaWindow], at now: Date) -> String? {
    let resets: [Date] = windows.compactMap(\.resetsAt).filter { reset in reset > now }
    guard let first: Date = resets.min(), let last: Date = resets.max() else { return nil }
    let exact: String = first.formatted(
      .dateTime.weekday(.abbreviated).month(.abbreviated).day().hour().minute())
    return last.timeIntervalSince(first) < 60
      ? "Resets \(exact)" : "Resets \(exact), \(weekday(last)) for the last window"
  }

  static func blockedLine(_ availability: Availability?) -> String? {
    guard case .blocked(let until) = availability else { return nil }
    return until.map { reset in "Blocked until \(weekday(reset))" } ?? "Blocked"
  }

  static func remaining(until reset: Date, at now: Date) -> String {
    let exact: String = reset.formatted(date: .complete, time: .complete)
    let remaining: TimeInterval = reset.timeIntervalSince(now)
    let duration: String = Duration.seconds(remaining).formatted(
      Duration.UnitsFormatStyle(
        allowedUnits: [.days, .hours, .minutes], width: .wide, maximumUnitCount: 2
      ))
    return switch remaining {
    case ...0: "Reset time: \(exact). No time remaining."
    case ..<60: "Resets \(exact). Less than a minute remaining."
    default: "Resets \(exact). \(duration) remaining."
    }
  }
}
