import CasePaths
import Foundation

// --- [MODELS] --------------------------------------------------------------------------

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

nonisolated struct QuotaWindow: Codable, Equatable, Sendable {
    let kind: QuotaKind
    let percent: Double
    let resetsAt: Date?

    init(kind: QuotaKind, percent: Double, resetsAt: Date?) throws(AccountFailure) {
        guard (0...100).contains(percent) else { throw AccountFailure(subject: "\(kind.name) usage \(percent)%", reason: .outOfRange) }
        (self.kind, self.percent, self.resetsAt) = (kind, percent, resetsAt)
    }

    init(from decoder: any Decoder) throws {
        let fields = try decoder.container(keyedBy: CodingKeys.self)
        try self.init(
            kind: fields.decode(QuotaKind.self, forKey: .kind),
            percent: fields.decode(Double.self, forKey: .percent),
            resetsAt: fields.decodeIfPresent(Date.self, forKey: .resetsAt),
        )
    }

    var fraction: Double { percent / 100 }
    func hasReset(at now: Date) -> Bool { resetsAt.map { reset in reset <= now } ?? false }
    func blocks(at now: Date) -> Bool { !hasReset(at: now) && percent >= 100 }
}

nonisolated struct AccountUsage: Codable, Equatable, Sendable {
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
