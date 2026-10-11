import Foundation

// --- [OPERATIONS] ----------------------------------------------------------------------

nonisolated enum UsagePresentation {
    private static let dayStyle: Date.FormatStyle = .dateTime.month(.abbreviated).day()
    private static let detailStyle: Date.FormatStyle = .dateTime.weekday(.wide).month().day().hour().minute()
    private static let separator: String = " · "

    static func percentage(_ amount: UsageAmount) -> String? {
        switch amount.percent {
            case 0: nil
            case ..<1: "<1%"
            default: amount.fraction.formatted(.percent.rounded(rule: .down).precision(.fractionLength(0)))
        }
    }

    static func reset(_ reset: Date, at now: Date) -> String {
        switch reset.timeIntervalSince(now) {
            case ..<60: "<1m"
            case let remaining where remaining < 86_400:
                Duration.seconds(remaining).formatted(Duration.UnitsFormatStyle(allowedUnits: [.hours, .minutes], width: .narrow))
            case ..<(6 * 86_400): reset.formatted(.dateTime.weekday(.abbreviated).hour().minute())
            default: reset.formatted(dayStyle)
        }
    }

    static func resetTooltip(_ reset: Date?) -> String {
        reset.map { reset in reset.formatted(detailStyle) } ?? "No reset"
    }

    static func sessionReading(
        _ window: QuotaWindow?,
        availability: Availability?,
        isCurrent: Bool,
        at now: Date,
    ) -> String? {
        let used: String? = window.flatMap { window in percentage(window.used) }
        let state: String? =
            switch availability {
                case .running(let until): reset(until, at: now)
                case .blocked: "Blocked"
                case .ready, .none: "Awaiting update"
                case .noSessionWindow: nil
            }
        guard !isCurrent || availability != .ready else { return nil }
        let parts: [String] = [used, state].compactMap(\.self)
        return parts.isEmpty ? nil : parts.joined(separator: separator)
    }

    static func reading(_ window: QuotaWindow, at now: Date) -> String {
        let used: String? = percentage(window.used)
        let resetText: String? = window.resetsAt.map { date in
            date <= now ? "Awaiting update" : reset(date, at: now)
        }
        return [used, resetText].compactMap(\.self).joined(separator: separator)
    }

    static func blockedLine(_ availability: Availability?, at now: Date) -> String? {
        if case .blocked(let until) = availability { until.map { date in "Blocked\(separator)\(reset(date, at: now))" } ?? "Blocked" } else { nil }
    }

    static func issue(_ error: ProviderError, at now: Date) -> String {
        [error.failure.localizedDescription, error.failure.retryAfter.map { until in reset(until, at: now) }]
            .compactMap(\.self)
            .joined(separator: separator)
    }

    static func refusal(_ refusal: AuthenticationRefusal, at now: Date) -> String {
        if case .failure(let error) = refusal { issue(error, at: now) } else { refusal.description }
    }

    static func signInLine(expiring expiry: Date?, at now: Date) -> String? {
        if let expiry, expiry <= now { "Sign-in expired" } else { nil }
    }

    static func signInExpiry(_ expiry: Date) -> String {
        expiry.formatted(dayStyle)
    }

    static func signInExpiryTooltip(_ expiry: Date) -> String {
        "Sign-in expires \(expiry.formatted(detailStyle))"
    }
}
