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
}

nonisolated enum Availability: Equatable, Sendable {
    case blocked(until: Date?)
    case running(until: Date)
    case ready
    case noSessionWindow
}

nonisolated struct AccountUsage: Equatable, Sendable {
    let windows: [QuotaWindow]
    let includedUsageAllowed: Bool?
    let observedAt: Date
    let signInExpiresAt: Date?

    var session: QuotaWindow? { windows.first { window in window.kind == .session } }
    var weekly: QuotaWindow? { windows.first { window in window.kind == .weekly } }

    func availability(at now: Date) -> Availability {
        switch (weekly, includedUsageAllowed, session, session?.resetsAt) {
            case (.some(let weekly), _, _, _) where weekly.blocks: .blocked(until: weekly.resetsAt)
            case (_, false, _, _): .blocked(until: weekly?.resetsAt)
            case (_, _, .none, _): .noSessionWindow
            case (_, _, .some, .some(let reset)) where reset > now: .running(until: reset)
            case (_, _, .some, _): .ready
        }
    }

    func fillingResets(_ reset: (QuotaKind) -> Date?) -> Self {
        Self(
            windows: windows.map { window in QuotaWindow(kind: window.kind, used: window.used, resetsAt: window.resetsAt ?? reset(window.kind), rejected: window.rejected) },
            includedUsageAllowed: includedUsageAllowed,
            observedAt: observedAt,
            signInExpiresAt: signInExpiresAt,
        )
    }
}

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

    var isCurrent: Bool {
        if case .current = self { true } else { false }
    }
}

// --- [ERRORS] --------------------------------------------------------------------------

nonisolated enum QuotaFailure: Error {
    case invalidPercentage(Double)
    case invalidResetDate(String, any Error)
}
