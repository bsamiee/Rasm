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

    func keepingReset(from previous: Self?, at now: Date) -> Self {
        if resetsAt == nil, let previous, previous.kind == kind,
            let reset: Date = previous.resetsAt, reset > now
        {
            withReset(reset)
        } else {
            self
        }
    }

    func withReset(_ reset: Date) -> Self {
        Self(kind: kind, used: used, resetsAt: reset, rejected: rejected)
    }
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
    var models: [QuotaWindow] {
        windows.filter { window in
            if case .model = window.kind { true } else { false }
        }
    }
    var nextReset: Date? {
        windows.compactMap(\.resetsAt).min()
    }

    func availability(at now: Date) -> Availability {
        if let weekly, weekly.blocks { return .blocked(until: weekly.resetsAt) }
        if includedUsageAllowed == false { return .blocked(until: weekly?.resetsAt) }
        guard let session else { return .noSessionWindow }
        if let reset: Date = session.resetsAt, reset > now { return .running(until: reset) }
        return .ready
    }

    func keepingResets(from previous: Self?, at now: Date) -> Self {
        withWindows(
            windows.map { window in
                window.keepingReset(
                    from: previous?.windows.first { earlier in earlier.kind == window.kind },
                    at: now,
                )
            }
        )
    }

    func settingSessionReset(_ reset: Date?) -> Self {
        if let reset, let session, session.resetsAt == nil { withWindows(windows.map { window in window.kind == .session ? window.withReset(reset) : window }) } else { self }
    }

    func withWindows(_ windows: [QuotaWindow]) -> Self {
        Self(
            windows: windows,
            includedUsageAllowed: includedUsageAllowed,
            observedAt: observedAt,
            signInExpiresAt: signInExpiresAt,
        )
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

// --- [ERRORS] --------------------------------------------------------------------------

nonisolated enum QuotaFailure: Error {
    case invalidPercentage(Double)
    case invalidResetDate
}
