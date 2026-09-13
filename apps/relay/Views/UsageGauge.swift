import SwiftUI

struct UsageGauge: View {
  let quota: AccountQuota
  let window: QuotaWindow?
  let isCurrent: Bool

  private static let trackHeight: CGFloat = 6

  var body: some View {
    TimelineView(.periodic(from: .now, by: 60)) { context in
      VStack(alignment: .leading, spacing: 4) {
        HStack(alignment: .firstTextBaseline, spacing: 8) {
          Text(quota.name)
            .foregroundStyle(isCurrent ? .primary : .secondary)
          Spacer(minLength: 8)
          Text(currentValueLabel(at: context.date))
            .monospacedDigit()
            .foregroundStyle(isCurrent ? .secondary : .tertiary)
            .contentTransition(.numericText())
        }
        .font(.subheadline)

        Capsule()
          .fill(.quaternary)
          .overlay(alignment: .leading) {
            if let window {
              GeometryReader { geometry in
                Capsule()
                  .fill(isCurrent ? Color.accentColor : Color.secondary)
                  .frame(width: geometry.size.width * window.amount.fraction)
              }
            }
          }
          .frame(height: Self.trackHeight)
      }
      .animation(.default, value: window)
      .accessibilityElement(children: .ignore)
      .accessibilityLabel("\(quota.name) usage")
      .accessibilityValue(accessibilityValue(at: context.date))
      .accessibilityHint(resetDescription(at: context.date))
      .help(resetDescription(at: context.date))
      .focusable()
    }
  }

  private func currentValueLabel(at now: Date) -> String {
    [window.flatMap { usagePercentage($0.amount.percent) }, resetLabel(at: now)]
      .compactMap { $0 }
      .joined(separator: " · ")
  }

  private func resetLabel(at now: Date) -> String {
    window.map { window in
      switch quota {
      case .session:
        switch window.sessionState(at: now) {
        case .running(let reset): sessionCountdown(until: reset, at: now)
        case .idle: isCurrent ? "Ready" : "Awaiting update"
        case .unknown: "Unavailable"
        }
      case .weekly, .fable:
        window.resetsAt.map { reset in
          reset > now
            ? reset.formatted(.dateTime.weekday(.abbreviated).hour().minute())
            : "Awaiting update"
        } ?? "Unavailable"
      }
    } ?? "Unavailable"
  }

  private func resetDescription(at now: Date) -> String {
    switch window {
    case .none: "Usage unavailable"
    case .some(let window):
      window.resetsAt.map { remainingDescription(until: $0, at: now) } ?? "No reset time reported"
    }
  }

  private func accessibilityValue(at now: Date) -> String {
    window.map { window in
      let amount: String = window.amount.fraction.formatted(
        .percent.precision(.fractionLength(0...2)))
      return "\(amount) used, \(resetLabel(at: now))\(isCurrent ? "" : ", last reported usage")"
    } ?? "Unavailable"
  }
}

private func remainingDescription(until reset: Date, at now: Date) -> String {
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

private func usagePercentage(_ percent: Double) -> String? {
  switch percent {
  case 0: nil
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
