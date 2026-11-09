import CasePaths
import Foundation

// --- [MODELS] --------------------------------------------------------------------------

nonisolated struct UsageAmount: Equatable, Sendable {
    let percent: Double

    var fraction: Double { percent / 100 }
    var isExhausted: Bool { percent >= 100 }

    private init(percent: Double) {
        self.percent = percent
    }

    static func make(percent: Double) -> Result<Self, QuotaFailure> {
        if (0...100).contains(percent) { .success(Self(percent: percent)) } else { .failure(.invalidPercentage(percent)) }
    }
}

nonisolated enum QuotaKind: Hashable, Codable, Sendable {
    case weekly
    case model(String)

    var name: String {
        switch self {
            case .weekly: "Weekly"
            case .model(let name): name
        }
    }
}

nonisolated struct QuotaWindow: Equatable, Sendable {
    let kind: QuotaKind
    let used: UsageAmount
    let resetsAt: Date?

    func hasReset(at now: Date) -> Bool { resetsAt.map { reset in reset <= now } ?? false }
    func blocks(at now: Date) -> Bool { !hasReset(at: now) && used.isExhausted }
}

nonisolated struct AccountUsage: Equatable, Sendable {
    let windows: [QuotaWindow]
    let includedUsageAllowed: Bool?
    let observedAt: Date
    let signInExpiresAt: Date?

    var weekly: QuotaWindow? { windows.first { window in window.kind == .weekly } }
    var nextReset: Date? { windows.compactMap(\.resetsAt).filter { reset in reset > observedAt }.min() }
    func isBlocked(at now: Date) -> Bool { weekly?.blocks(at: now) == true || includedUsageAllowed == false }
}

@CasePathable
enum UsageState: Sendable {
    case unavailable
    case current(AccountUsage)
    case stale(AccountUsage)

    var usage: AccountUsage? {
        switch self {
            case .unavailable: nil
            case .current(let usage), .stale(let usage): usage
        }
    }
}

// --- [ERRORS] --------------------------------------------------------------------------

nonisolated enum QuotaFailure: Error {
    case invalidPercentage(Double)
}
