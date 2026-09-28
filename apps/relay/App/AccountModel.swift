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
    var canSelect: Bool { isConnected && !isBusy && !isSelected }

    func availability(at now: Date) -> Availability? {
        usage.usage.map { usage in usage.availability(at: now) }
    }

    func canStartSession(at now: Date) -> Bool {
        isConnected && usage.isCurrent && (running?.kind == nil || running?.kind == .refreshing)
            && availability(at: now) == .ready
    }

    func isUsageReadDue(for trigger: RefreshTrigger, at now: Date) -> Bool {
        let usageReadInterval: TimeInterval = 180
        let limited: Bool = retryAfter.map { date in date > now } ?? false
        let recent: Bool =
            switch usage {
                case .current(let usage): now.timeIntervalSince(usage.observedAt) < usageReadInterval
                case .stale, .unavailable: false
            }
        return trigger == .userAction || !limited && !recent
    }

    func run(_ operation: AccountOperation, _ work: @escaping @MainActor () async -> Void) {
        let previous: Task<Void, Never>?
        switch (running?.kind, operation) {
            case (.none, _): previous = nil
            case (.refreshing, .starting): previous = running?.task
            case (.some(let blocking), _):
                blockingOperation = blocking
                return
        }
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
