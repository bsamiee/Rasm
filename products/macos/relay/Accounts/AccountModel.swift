import Foundation
import Observation

// --- [MODELS] --------------------------------------------------------------------------

@Observable
final class AccountModel: Codable {
    private enum CodingKeys: CodingKey {
        case authentication, usage
    }

    static let usageReadSpacing: (active: Duration, inactive: Duration) = (.seconds(180), .seconds(1800))

    var account: Account
    var authentication: AuthenticationState
    var usage: UsageState
    var isActive: Bool = false
    private(set) var running: (token: UUID, kind: AccountOperation, task: Task<Void, Never>)?

    init(account: Account, authentication: AuthenticationState, usage: UsageState) {
        (self.account, self.authentication, self.usage) = (account, authentication, usage)
    }

    convenience init(from decoder: any Decoder) throws {
        let fields = try decoder.container(keyedBy: CodingKeys.self)
        try self.init(
            account: Account(from: decoder),
            authentication: fields.decode(AuthenticationState.self, forKey: .authentication),
            usage: fields.decodeIfPresent(AccountUsage.self, forKey: .usage).map(UsageState.current) ?? .unavailable,
        )
    }

    func encode(to encoder: any Encoder) throws {
        var fields = encoder.container(keyedBy: CodingKeys.self)
        try account.encode(to: encoder)
        try fields.encode(authentication, forKey: .authentication)
        try fields.encodeIfPresent(usage.usage, forKey: .usage)
    }

    var isBusy: Bool { running != nil }
    var isConnected: Bool { authentication == .connected }
    var acceptsOperation: Bool { running.map { running in running.kind == .refresh } ?? true }
    var canActivate: Bool { isConnected && acceptsOperation && !isActive }

    var usageReadAfter: Date? {
        usage.usage.map { usage in
            let spaced = usage.observedAt.addingTimeInterval((isActive ? Self.usageReadSpacing.active : Self.usageReadSpacing.inactive) / .seconds(1))
            return usage.nextReset.map { reset in min(reset, spaced) } ?? spaced
        }
    }

    func run(_ operation: AccountOperation, _ work: @escaping @MainActor () async -> Void) {
        guard acceptsOperation else { return }
        let previous = running?.task
        let token = UUID()
        running = (
            token,
            operation,
            Task(name: operation.name.description) { [self] in
                await withTaskCancellationHandler {
                    await previous?.value
                } onCancel: {
                    previous?.cancel()
                }
                await work()
                guard running?.token == token else { return }
                running = nil
            },
        )
    }
}
