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
    static let usageReadSpacing: TimeInterval = 180

    var account: Account
    var authentication: AuthenticationState
    var usage: UsageState
    var retryAfter: Date?
    var issue: ProviderError?
    var isSelected: Bool = false
    var automaticStartAttempted: Bool = false
    private(set) var running: RunningOperation?
    private(set) var blockingOperation: AccountOperation?

    init(account: Account, authentication: AuthenticationState, usage: UsageState, retryAfter: Date?) {
        self.account = account
        self.authentication = authentication
        self.usage = usage
        self.retryAfter = retryAfter
    }

    var isBusy: Bool { running != nil }
    var isConnected: Bool { authentication == .connected }
    var acceptsOperation: Bool { running.map { running in running.kind == .refreshing } ?? true }
    var canSelect: Bool { isConnected && acceptsOperation && !isSelected }

    func canStartSession(at now: Date) -> Bool {
        isConnected && usage.isCurrent && acceptsOperation && usage.usage?.availability(at: now) == .ready
    }

    var usageReadAfter: Date? {
        let nextRead: Date? =
            switch usage {
                case .current(let usage): usage.observedAt.addingTimeInterval(Self.usageReadSpacing)
                case .stale, .unavailable: nil
            }
        return [retryAfter, nextRead].compactMap(\.self).max()
    }

    func run(_ operation: AccountOperation, _ work: @escaping @MainActor () async -> Void) {
        guard acceptsOperation else {
            blockingOperation = running?.kind
            return
        }
        let previous: Task<Void, Never>? = running?.task
        let token: UUID = UUID()
        running = RunningOperation(
            token: token,
            kind: operation,
            task: Task(name: operation.description) { [self] in
                await withTaskCancellationHandler {
                    await previous?.value
                } onCancel: {
                    previous?.cancel()
                }
                await work()
                guard running?.token == token else { return }
                running = nil
                blockingOperation = nil
            },
        )
    }
}
