import Foundation

enum QuotaFailure: Error, Equatable {
  case invalidPercentage(Double)
  case invalidResetDate
}

struct UsageAmount: Equatable, Sendable {
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

struct QuotaWindow: Equatable, Sendable {
  let amount: UsageAmount
  let resetsAt: Date?
}

enum SessionState: Equatable, Sendable {
  case unknown
  case idle
  case running(resetsAt: Date)
}

struct UsageSnapshot: Equatable, Sendable {
  let session: QuotaWindow?
  let weekly: QuotaWindow?
  let fable: QuotaWindow?
  let observedAt: Date

  func sessionState(at now: Date) -> SessionState {
    guard let session else { return .unknown }
    switch session.resetsAt {
    case .some(let deadline) where deadline > now:
      return .running(resetsAt: deadline)
    case .some:
      return .idle
    case .none where session.amount.fraction == 0:
      return .idle
    case .none:
      return .unknown
    }
  }
}

enum UsageState: Equatable, Sendable {
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
