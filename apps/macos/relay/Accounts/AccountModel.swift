import Foundation
import Observation

// --- [MODELS] --------------------------------------------------------------------------

struct RunningOperation {
    let token: UUID
    let kind: AccountOperation
    let task: Task<Void, Never>
}

@Observable
final class AccountModel {
    static let usageReadSpacing: (selected: TimeInterval, parked: TimeInterval) = (180, 1800)

    var account: Account
    var authentication: AuthenticationState
    var usage: UsageState
    var isSelected: Bool = false
    private(set) var running: RunningOperation?

    init(account: Account, authentication: AuthenticationState, usage: UsageState) {
        self.account = account
        self.authentication = authentication
        self.usage = usage
    }

    var isBusy: Bool { running != nil }
    var isConnected: Bool { authentication == .connected }
    var acceptsOperation: Bool { running.map { running in running.kind == .refreshing } ?? true }
    var canSelect: Bool { isConnected && acceptsOperation && !isSelected }

    var usageReadAfter: Date? {
        usage.usage.map { usage in
            let spaced: Date = usage.observedAt.addingTimeInterval(isSelected ? Self.usageReadSpacing.selected : Self.usageReadSpacing.parked)
            return usage.nextReset.map { reset in min(reset, spaced) } ?? spaced
        }
    }

    func run(_ operation: AccountOperation, _ work: @escaping @MainActor () async -> Void) {
        guard acceptsOperation else { return }
        let previous: Task<Void, Never>? = running?.task
        let token: UUID = UUID()
        running = RunningOperation(
            token: token,
            kind: operation,
            task: Task(name: operation.name.description) { [self] in
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
