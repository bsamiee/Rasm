import Foundation

nonisolated enum QuotaFailure: Error {
  case invalidPercentage(Double)
  case invalidResetDate
}

nonisolated struct UsageAmount: Equatable, Sendable {
  let percent: Double

  var fraction: Double { percent / 100 }
  var isExhausted: Bool { percent >= 100 }

  private init(percent: Double) {
    self.percent = percent
  }

  static func make(percent: Double) -> Result<UsageAmount, QuotaFailure> {
    guard percent.isFinite, (0...100).contains(percent) else {
      return .failure(.invalidPercentage(percent))
    }
    return .success(UsageAmount(percent: percent))
  }
}

nonisolated enum QuotaKind: Hashable, Sendable {
  case session
  case weekly
  case model(String)

  var name: String {
    switch self {
    case .session: "Session"
    case .weekly: "Weekly"
    case .model(let name): name
    }
  }
}

nonisolated struct QuotaWindow: Equatable, Sendable {
  let kind: QuotaKind
  let used: UsageAmount
  let resetsAt: Date?
  let rejected: Bool

  var blocks: Bool { used.isExhausted || rejected }

  func carryingReset(from previous: QuotaWindow?, at now: Date) -> QuotaWindow {
    guard resetsAt == nil, let previous, previous.kind == kind,
      let reset: Date = previous.resetsAt, reset > now
    else { return self }
    return QuotaWindow(kind: kind, used: used, resetsAt: reset, rejected: rejected)
  }
}

nonisolated enum Availability: Equatable, Sendable {
  case blocked(until: Date?)
  case running(until: Date)
  case ready
}

nonisolated struct AccountUsage: Equatable, Sendable {
  let windows: [QuotaWindow]
  let includedUsageAllowed: Bool?
  let observedAt: Date

  var session: QuotaWindow? { windows.first { window in window.kind == .session } }
  var weekly: QuotaWindow? { windows.first { window in window.kind == .weekly } }
  var models: [QuotaWindow] {
    windows.filter { window in
      if case .model = window.kind { true } else { false }
    }
  }

  func availability(at now: Date) -> Availability {
    if let weekly, weekly.blocks { return .blocked(until: weekly.resetsAt) }
    if includedUsageAllowed == false { return .blocked(until: weekly?.resetsAt) }
    if let reset: Date = session?.resetsAt, reset > now { return .running(until: reset) }
    return .ready
  }

  func carryingResets(from previous: AccountUsage?, at now: Date) -> AccountUsage {
    AccountUsage(
      windows: windows.map { window in
        window.carryingReset(
          from: previous?.windows.first { earlier in earlier.kind == window.kind }, at: now)
      },
      includedUsageAllowed: includedUsageAllowed, observedAt: observedAt)
  }

  var nextReset: Date? {
    windows.compactMap(\.resetsAt).min()
  }
}

nonisolated enum UsageState: Sendable {
  case unavailable
  case current(AccountUsage)
  case stale(AccountUsage)

  var usage: AccountUsage? {
    switch self {
    case .unavailable: nil
    case .current(let usage), .stale(let usage): usage
    }
  }

  var isCurrent: Bool {
    if case .current = self { true } else { false }
  }
}
