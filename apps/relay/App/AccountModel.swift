import Foundation
import Observation

struct RunningOperation {
  let kind: AccountOperation
  let startedAt: Date
  let task: Task<Void, Never>
}

@Observable
final class AccountModel: Identifiable {
  var account: Account
  var authentication: AuthenticationState
  var usage: UsageState
  var issue: String?
  var isSelected: Bool = false
  var automaticStartAttempted: Bool = false
  private(set) var running: RunningOperation?

  init(account: Account, authentication: AuthenticationState, usage: UsageState) {
    self.account = account
    self.authentication = authentication
    self.usage = usage
  }

  var id: UUID { account.id }
  var operation: AccountOperation? { running?.kind }
  var operationStartedAt: Date? { running?.startedAt }
  var isBusy: Bool { running != nil }
  var isConnected: Bool { authentication == .connected }
  var canSelect: Bool { isConnected && !isBusy && !isSelected }

  func availability(at now: Date) -> Availability? {
    usage.usage.map { usage in usage.availability(at: now) }
  }

  func canStartSession(at now: Date) -> Bool {
    guard isConnected, !isBusy else { return false }
    return availability(at: now).map { availability in availability == .ready } ?? true
  }

  func run(_ operation: AccountOperation, _ work: @escaping @MainActor () async -> Void) {
    if let running {
      issue = "Wait for \(running.kind.description.lowercased()) to finish"
      return
    }
    issue = nil
    running = RunningOperation(
      kind: operation, startedAt: Date(),
      task: Task(name: operation.description) { [self] in
        await work()
        running = nil
      })
  }

  func cancel() {
    running?.task.cancel()
  }

  func finish() async {
    await running?.task.value
  }
}
