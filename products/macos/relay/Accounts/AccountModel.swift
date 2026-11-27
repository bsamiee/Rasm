import Foundation
import Observation

// --- [MODELS] --------------------------------------------------------------------------

@Observable
final class AccountModel: Codable {
    private enum CodingKeys: CodingKey {
        case signIn, usage
    }

    static let usageReadSpacing: (active: Duration, inactive: Duration) = (.seconds(180), .seconds(1800))

    var account: Account
    var signIn: SignInCause?
    var usage: UsageState
    var isActive: Bool = false
    private(set) var running: (token: UUID, kind: AccountOperation, task: Task<Void, Never>)?

    init(account: Account, usage: UsageState, signIn: SignInCause? = nil) {
        (self.account, self.usage, self.signIn) = (account, usage, signIn)
    }

    convenience init(from decoder: any Decoder) throws {
        let fields = try decoder.container(keyedBy: CodingKeys.self)
        try self.init(
            account: Account(from: decoder),
            usage: fields.decodeIfPresent(AccountUsage.self, forKey: .usage).map(UsageState.current) ?? .unavailable,
            signIn: fields.decodeIfPresent(SignInCause.self, forKey: .signIn),
        )
    }

    func encode(to encoder: any Encoder) throws {
        var fields = encoder.container(keyedBy: CodingKeys.self)
        try account.encode(to: encoder)
        try fields.encodeIfPresent(signIn, forKey: .signIn)
        try fields.encodeIfPresent(usage.usage, forKey: .usage)
    }

    var isBusy: Bool { running != nil }
    var isConnected: Bool { signIn == nil }
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
