import Foundation

nonisolated enum QuotaFailure: Error {
  case invalidPercentage(Double)
  case invalidResetDate
}

nonisolated struct UsageAmount: Equatable, Sendable {
  let percent: Double

  var fraction: Double { percent / 100 }

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

nonisolated enum SessionState: Sendable {
  case unknown
  case idle
  case running(resetsAt: Date)
}

nonisolated struct QuotaWindow: Equatable, Sendable {
  let amount: UsageAmount
  let resetsAt: Date?

  func sessionState(at now: Date) -> SessionState {
    switch resetsAt {
    case .some(let deadline) where deadline > now: .running(resetsAt: deadline)
    case .some: .idle
    case .none where amount.fraction == 0: .idle
    case .none: .unknown
    }
  }
}

nonisolated struct UsageSnapshot: Sendable {
  let session: QuotaWindow?
  let weekly: QuotaWindow?
  let fable: QuotaWindow?
  let observedAt: Date

  func sessionState(at now: Date) -> SessionState {
    session?.sessionState(at: now) ?? .unknown
  }
}

enum UsageState: Sendable {
  case unavailable
  case current(UsageSnapshot)
  case stale(UsageSnapshot)

  var snapshot: UsageSnapshot? {
    switch self {
    case .unavailable: nil
    case .current(let snapshot), .stale(let snapshot): snapshot
    }
  }
}
