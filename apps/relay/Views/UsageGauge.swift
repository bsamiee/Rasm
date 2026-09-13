import SwiftUI

enum UsagePeriod {
  case session
  case weekly
  case fable

  var name: String {
    switch self {
    case .session: "Session"
    case .weekly: "Weekly"
    case .fable: "Fable"
    }
  }
}

struct UsageGauge: View {
  let period: UsagePeriod
  let quota: QuotaWindow?
  let isCurrent: Bool

  private static let barHeight: CGFloat = 6

  var body: some View {
    TimelineView(.periodic(from: .now, by: 60)) { context in
      VStack(alignment: .leading, spacing: 4) {
        HStack(alignment: .firstTextBaseline, spacing: 8) {
          Text(period.name)
            .foregroundStyle(isCurrent ? .primary : .secondary)
          Spacer(minLength: 8)
          Text(reading(at: context.date))
            .monospacedDigit()
            .foregroundStyle(isCurrent ? .secondary : .tertiary)
            .contentTransition(.numericText())
        }
        .font(.subheadline)

        Capsule()
          .fill(.quaternary)
          .overlay(alignment: .leading) {
            GeometryReader { geometry in
              Capsule()
                .fill(isCurrent ? Color.blue : Color.gray)
                .frame(width: geometry.size.width * (quota?.amount.fraction ?? 0))
            }
          }
          .frame(height: Self.barHeight)
      }
      .animation(.default, value: quota)
      .accessibilityElement(children: .ignore)
      .accessibilityLabel("\(period.name) usage")
      .accessibilityValue(accessibilityValue(at: context.date))
      .accessibilityHint(resetDetails(at: context.date))
      .help(resetDetails(at: context.date))
      .focusable()
    }
  }

  private func reading(at now: Date) -> String {
    [quota.map { usagePercentage($0.amount.percent) } ?? "", resetLabel(at: now)]
      .filter { !$0.isEmpty }
      .joined(separator: " · ")
  }

  private func resetLabel(at now: Date) -> String {
    guard let quota else { return "Unavailable" }
    switch (period, quota.resetsAt) {
    case (.session, .some(let reset)) where reset > now:
      return sessionCountdown(until: reset, at: now)
    case (.session, .some):
      return isCurrent ? "Ready" : "Awaiting update"
    case (.session, .none) where quota.amount.fraction == 0:
      return isCurrent ? "Ready" : "Awaiting update"
    case (.session, .none), (.weekly, .none), (.fable, .none):
      return "Unavailable"
    case (.weekly, .some(let reset)), (.fable, .some(let reset)):
      return reset > now
        ? reset.formatted(.dateTime.weekday(.abbreviated).hour().minute())
        : "Awaiting update"
    }
  }

  private func resetDetails(at now: Date) -> String {
    guard let reset: Date = quota?.resetsAt else {
      return quota == nil ? "Usage unavailable" : "No reset time reported"
    }
    let exact: String = reset.formatted(date: .complete, time: .complete)
    let remaining: TimeInterval = reset.timeIntervalSince(now)
    if remaining <= 0 { return "Reset time: \(exact). No time remaining." }
    if remaining < 60 { return "Resets \(exact). Less than a minute remaining." }
    let duration: String = Duration.seconds(remaining).formatted(
      Duration.UnitsFormatStyle(
        allowedUnits: [.days, .hours, .minutes], width: .wide, maximumUnitCount: 2
      ))
    return "Resets \(exact). \(duration) remaining."
  }

  private func accessibilityValue(at now: Date) -> String {
    guard let quota else { return "Unavailable" }
    let amount: String = quota.amount.fraction.formatted(.percent.precision(.fractionLength(0...2)))
    return "\(amount) used, \(resetLabel(at: now))\(isCurrent ? "" : ", last reported usage")"
  }
}

private func usagePercentage(_ percent: Double) -> String {
  switch percent {
  case 0: ""
  case ..<1: "<1%"
  case 100: "100%"
  default: "\(Int(percent.rounded(.down)))%"
  }
}

private func sessionCountdown(until reset: Date, at now: Date) -> String {
  let minutes: Int = Int(reset.timeIntervalSince(now) / 60)
  switch minutes {
  case ..<1: return "<1m"
  case ..<60: return "\(minutes.formatted())m"
  default:
    let hours: Int = minutes / 60
    let remainder: Int = minutes % 60
    return remainder == 0
      ? "\(hours.formatted())h" : "\(hours.formatted())h \(remainder.formatted())m"
  }
}
