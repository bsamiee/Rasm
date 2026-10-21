import Foundation

// --- [CONSTANTS] -----------------------------------------------------------------------

nonisolated private let separator: String = " · "
nonisolated private let awaitingUpdate: String = "Awaiting update"
nonisolated private let blockedText: String = "Blocked"

// --- [OPERATIONS] ----------------------------------------------------------------------

nonisolated extension FormatStyle where Self == Date.FormatStyle {
    static var monthDay: Self { .dateTime.month(.abbreviated).day() }
    static var detail: Self { .dateTime.weekday(.wide).month().day().hour().minute() }
}

nonisolated extension Date {
    func resetText(at now: Date) -> String {
        switch timeIntervalSince(now) {
            case ..<60: "<1m"
            case let remaining where remaining < 86_400:
                Duration.seconds(remaining).formatted(Duration.UnitsFormatStyle(allowedUnits: [.hours, .minutes], width: .narrow))
            case ..<(6 * 86_400): formatted(.dateTime.weekday(.abbreviated).hour().minute())
            default: formatted(.monthDay)
        }
    }
}

extension Date? {
    var resetTooltip: String { map { reset in reset.formatted(.detail) } ?? "No reset" }
}

extension UsageAmount {
    var percentage: String? {
        switch percent {
            case 0: nil
            case ..<1: "<1%"
            default: fraction.formatted(.percent.rounded(rule: .down).precision(.fractionLength(0)))
        }
    }
}

extension QuotaWindow {
    func reading(at now: Date) -> String {
        [used.percentage, resetsAt.map { date in date <= now ? awaitingUpdate : date.resetText(at: now) }].compactMap(\.self).joined(separator: separator)
    }
}

extension Availability {
    func blockedLine(at now: Date) -> String? {
        if case .blocked(let until) = self { [blockedText, until?.resetText(at: now)].compactMap(\.self).joined(separator: separator) } else { nil }
    }
}

extension AccountUsage {
    func signInLine(at now: Date) -> String? {
        signInExpiresAt.flatMap { expiry in expiry <= now ? "Sign-in expired" : nil }
    }
}

extension UsageState {
    func sessionReading(at now: Date) -> String? {
        let availability: Availability? = usage?.availability(at: now)
        let state: String? =
            switch availability {
                case .running(let until): until.resetText(at: now)
                case .blocked: blockedText
                case .ready, .none: awaitingUpdate
                case .noSessionWindow: nil
            }
        guard !isCurrent || availability != .ready else { return nil }
        let parts: [String] = [usage?.session?.used.percentage, state].compactMap(\.self)
        return parts.isEmpty ? nil : parts.joined(separator: separator)
    }
}

extension ProviderError {
    func description(at now: Date) -> String {
        [failure.localizedDescription, failure.retryAfter.map { until in until.resetText(at: now) }].compactMap(\.self).joined(separator: separator)
    }
}

extension AuthenticationPhase {
    func description(at now: Date) -> String {
        switch self {
            case .pending: "Waiting for browser sign-in"
            case .completing: "Completing sign-in"
            case .refused(.differentAccount(let email)): "Signed in to a different account than \(email)"
            case .refused(.alreadyConnected(let email)): "\(email) is already connected"
            case .refused(.failure(let error)): error.description(at: now)
            case .cancelling: "Canceling sign-in"
        }
    }
}
