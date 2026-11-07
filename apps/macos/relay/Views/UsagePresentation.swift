import CasePathsCore
import DeveloperToolsSupport
import Foundation

// --- [CONSTANTS] -----------------------------------------------------------------------

private nonisolated let separator: String = " · "

// --- [OPERATIONS] ----------------------------------------------------------------------

extension Provider {
    var symbol: ImageResource {
        switch self {
            case .claude: .claude
            case .openAI: .openAI
        }
    }
}

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
            case ..<TimeInterval(6 * 86_400): formatted(.dateTime.weekday(.abbreviated).hour().minute())
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
    func resetText(at now: Date) -> String? {
        hasReset(at: now) ? nil : resetsAt?.resetText(at: now)
    }

    func reading(at now: Date) -> String {
        hasReset(at: now) ? "Reset" : [used.percentage, resetText(at: now)].compactMap(\.self).joined(separator: separator)
    }
}

extension AccountModel {
    func note(at now: Date) -> String? {
        let parts: [String?] =
            switch (authentication, usage.usage, usage.usage?.signInExpiresAt.flatMap { expiry in expiry <= now ? expiry : nil }) {
                case (_, _, .some(let expiry)): ["Sign-in expired \(expiry.formatted(.monthDay))"]
                case (.signInRequired, _, .none): ["Sign in required"]
                case (.connected, .none, .none): []
                case (.connected, .some(let reading), .none):
                    (reading.isBlocked(at: now) ? ["Blocked", reading.weekly?.resetText(at: now)] as [String?] : [])
                        + [usage.is(\.stale) ? "Updated \(reading.observedAt.formatted(.relative(presentation: .named)))" : nil] as [String?]
            }
        let text: [String] = parts.compactMap(\.self)
        return text.isEmpty ? nil : text.joined(separator: separator)
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
            case .refused(.differentAccount(let email)): "Signed-in account is not \(email)"
            case .refused(.alreadyConnected(let email)): "\(email) is already connected"
            case .refused(.failure(let error)): error.description(at: now)
            case .cancelling: "Canceling sign-in"
        }
    }
}
