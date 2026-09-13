import AppKit
import Foundation
import OSLog
import Observation
import SwiftUI

@Observable
final class AccountStore {
  private var states: [AccountState] = []
  private var selections: [Provider: UUID] = [:]
  private(set) var authentication: AuthenticationPresentation?
  private(set) var loginItem: LoginItem = .current
  private(set) var isLoading: Bool = true
  private var storageIssue: String?
  private var providerIssues: [Provider: String] = [:]

  @ObservationIgnored private let storage: AccountStorage
  @ObservationIgnored private let claude: ClaudeClient
  @ObservationIgnored private let codex: CodexClient
  @ObservationIgnored private let logger: Logger = Logger(
    subsystem: "app.rasm.relay", category: "Accounts")
  @ObservationIgnored private var launchTask: Task<Void, Never>?
  @ObservationIgnored private var scheduledRefresh: Task<Void, Never>?
  @ObservationIgnored private var authenticationTask: Task<Void, Never>?
  @ObservationIgnored private var loginItemTask: Task<Void, Never>?
  @ObservationIgnored private var saveTask: Task<Void, Never>?
  @ObservationIgnored private var wakeTask: Task<Void, Never>?
  @ObservationIgnored private var isMenuBarExtraVisible: Bool = false
  @ObservationIgnored private var isStopping: Bool = false
  @ObservationIgnored private var isStorageAvailable: Bool = false

  private static let refreshInterval: Duration = .seconds(60)

  init(environment: [String: String]) {
    let home: URL = FileManager.default.homeDirectoryForCurrentUser
    let support: URL = home.appending(
      path: "Library/Application Support/Relay", directoryHint: .isDirectory)
    let claudeDirectory: URL =
      environment["CLAUDE_CONFIG_DIR"].map {
        URL(filePath: $0, directoryHint: .isDirectory)
      } ?? home.appending(path: ".claude", directoryHint: .isDirectory)
    let locations: FileLocations = FileLocations(
      applicationSupportDirectory: support, defaultClaudeDirectory: claudeDirectory)
    storage = AccountStorage(fileURL: locations.accountsFile)
    claude = ClaudeClient(paths: locations, environment: environment)
    codex = CodexClient(paths: locations, environment: environment)
  }

  var accounts: [AccountViewModel] {
    states.map { state in
      AccountViewModel(
        account: state.account, authentication: state.authentication, usageState: state.usage,
        operation: state.operation, issue: state.issue,
        isSelected: selections[state.account.provider] == state.id
      )
    }
  }

  var issue: String? {
    let messages: [String] =
      [storageIssue].compactMap { $0 }
      + Provider.allCases.compactMap { providerIssues[$0] }
    return messages.isEmpty ? nil : messages.joined(separator: "\n")
  }

  var canAddAccount: Bool { isStorageAvailable && !isStopping && authentication == nil }

  private var records: [Account] { states.map(\.account) }

  func start() {
    guard launchTask == nil else { return }
    launchTask = Task {
      await load()
      guard isStorageAvailable, !isStopping else { return }
      await updateSelections()
      refresh()
    }
    wakeTask = Task {
      for await _ in NSWorkspace.shared.notificationCenter.notifications(
        named: NSWorkspace.didWakeNotification)
      {
        refresh()
      }
    }
  }

  func stop() async {
    isStopping = true
    wakeTask?.cancel()
    scheduledRefresh?.cancel()
    authenticationTask?.cancel()
    if let authentication {
      switch authentication.provider {
      case .claude: await claude.cancel(accountID: authentication.id)
      case .openAI: await codex.cancel(accountID: authentication.id)
      }
    }
    for case .running(_, let task) in states.map(\.activity) { await task.value }
    let pending: [Task<Void, Never>] = [
      authenticationTask, loginItemTask, saveTask, scheduledRefresh, wakeTask, launchTask,
    ].compactMap { $0 }
    for task: Task<Void, Never> in pending { await task.value }
    await claude.cancelOperations()
    await codex.cancelOperations()
    if isStorageAvailable { await save() }
  }

  func setMenuBarExtraVisible(_ visible: Bool) {
    isMenuBarExtraVisible = visible
    guard visible else { return }
    loginItem = .current
    refresh()
  }

  func startSession(_ id: UUID) {
    guard let index: Int = index(of: id), states[index].authentication == .connected,
      states[index].sessionStart == .idle
    else { return }
    let account: Account = states[index].account
    perform(.starting, for: id) { [self] in
      update(id) { $0.sessionStart = .awaitingConfirmation }
      guard case .success = await save() else {
        update(id) { $0.sessionStart = .idle }
        return
      }
      let result: Result<UsageSnapshot, ClientFailure> =
        switch account.provider {
        case .claude:
          await claude.startSession(for: account, accounts: records).mapError(ClientFailure.claude)
        case .openAI: await codex.startSession(for: account).mapError(ClientFailure.codex)
        }
      applyUsage(result, to: id)
      if case .failure(let error) = result, !error.awaitingSessionConfirmation {
        update(id) { $0.sessionStart = .idle }
      }
    }
  }

  func select(_ id: UUID) {
    guard let index: Int = index(of: id), states[index].authentication == .connected else {
      return
    }
    let account: Account = states[index].account
    perform(.selecting, for: id) { [self] in
      let result: Result<AccountSelection, ClientFailure> =
        switch account.provider {
        case .claude: await claude.select(account, accounts: records).mapError(ClientFailure.claude)
        case .openAI: await codex.select(account).mapError(ClientFailure.codex)
        }
      switch result {
      case .success(let selection):
        update(id) { $0.account.identity = selection.identity }
        if let preserved: Account = selection.preservedAccount,
          !states.contains(where: { $0.id == preserved.id })
        {
          states.append(.connected(preserved))
        }
        selections[account.provider] = id
        providerIssues[account.provider] = nil
      case .failure(let error):
        record(error, for: id)
        await loadClaudeAccounts()
        await updateSelections()
      }
    }
  }

  func addAccount(_ provider: Provider) {
    guard canAddAccount else { return }
    authenticate(id: UUID(), provider: provider, existing: nil)
  }

  func signIn(_ id: UUID) {
    guard let index: Int = index(of: id), case .idle = states[index].activity,
      authentication == nil, !isStopping
    else { return }
    let account: Account = states[index].account
    authenticate(id: id, provider: account.provider, existing: account)
  }

  func cancelAuthentication() {
    guard let pending: AuthenticationPresentation = authentication else { return }
    let previous: Task<Void, Never>? = authenticationTask
    previous?.cancel()
    authentication = AuthenticationPresentation(
      id: pending.id, provider: pending.provider, phase: .cancelling)
    authenticationTask = Task {
      switch pending.provider {
      case .claude: await claude.cancel(accountID: pending.id)
      case .openAI: await codex.cancel(accountID: pending.id)
      }
      if let previous { await previous.value }
      if index(of: pending.id) == nil { await discard(pending.id, provider: pending.provider) }
      authentication = nil
      update(pending.id) { $0.activity = .idle }
    }
  }

  func signOut(_ id: UUID) {
    guard let index: Int = index(of: id) else { return }
    let account: Account = states[index].account
    perform(.signingOut, for: id) { [self] in
      let result: Result<Void, ClientFailure> =
        switch account.provider {
        case .claude:
          await claude.signOut(account, accounts: records).mapError(ClientFailure.claude)
        case .openAI: await codex.signOut(account).mapError(ClientFailure.codex)
        }
      switch result {
      case .success:
        update(id) { state in
          state.authentication = .signInRequired
          state.usage = .unavailable
          state.sessionStart = .idle
        }
        if selections[account.provider] == id { selections[account.provider] = nil }
      case .failure(let error): record(error, for: id)
      }
    }
  }

  func remove(_ id: UUID) {
    guard let index: Int = index(of: id) else { return }
    let account: Account = states[index].account
    perform(.removing, for: id) { [self] in
      let result: Result<Void, ClientFailure> =
        switch account.provider {
        case .claude: await claude.remove(account, accounts: records).mapError(ClientFailure.claude)
        case .openAI: await codex.remove(account).mapError(ClientFailure.codex)
        }
      switch result {
      case .success:
        states.removeAll { $0.id == id }
        if selections[account.provider] == id { selections[account.provider] = nil }
      case .failure(let error): record(error, for: id)
      }
    }
  }

  func setSessionPolicy(_ policy: SessionPolicy, for id: UUID) {
    update(id) { state in
      state.account.sessionPolicy = policy
      state.automaticStartAttempted = false
    }
    scheduleSave()
    if policy == .automatic { refresh(states.filter { $0.id == id }) }
  }

  func moveAccount(_ id: UUID, by offset: Int) {
    guard let index: Int = index(of: id), states.indices.contains(index + offset) else { return }
    states.swapAt(index, index + offset)
    scheduleSave()
  }

  func moveAccounts(from offsets: IndexSet, to destination: Int) {
    states.move(fromOffsets: offsets, toOffset: destination)
    scheduleSave()
  }

  func setLoginItemEnabled(_ enabled: Bool) {
    guard loginItemTask == nil else { return }
    loginItemTask = Task {
      loginItem = await LoginItem.setEnabled(enabled)
      loginItemTask = nil
    }
  }

  private func load() async {
    defer { isLoading = false }
    switch await storage.load() {
    case .success(let saved):
      states = saved.accounts.map(AccountState.restored)
      isStorageAvailable = true
      await loadClaudeAccounts()
    case .failure(let error):
      storageIssue = error.localizedDescription
      logger.error("\(String(describing: error), privacy: .private)")
    }
  }

  private func loadClaudeAccounts() async {
    switch await claude.recoverAccounts() {
    case .success(let recovered):
      providerIssues[.claude] = nil
      states += recovered.filter { index(of: $0.id) == nil }.map(AccountState.connected)
    case .failure(let error):
      providerIssues[.claude] = error.localizedDescription
    }
  }

  private func updateSelections() async {
    async let claudeSelection: Result<UUID?, ClaudeFailure> = claude.selectedAccount(in: records)
    async let codexSelection: Result<UUID?, CodexFailure> = codex.selectedAccount(in: records)
    let outcomes: [(Provider, Result<UUID?, ClientFailure>)] = [
      (.claude, await claudeSelection.mapError(ClientFailure.claude)),
      (.openAI, await codexSelection.mapError(ClientFailure.codex)),
    ]
    for (provider, outcome): (Provider, Result<UUID?, ClientFailure>) in outcomes {
      switch outcome {
      case .success(let id):
        selections[provider] = id
        providerIssues[provider] = nil
        if let id {
          update(id) { state in
            state.authentication = .connected
            state.issue = nil
          }
        }
      case .failure(let error):
        selections[provider] = nil
        providerIssues[provider] = error.localizedDescription
      }
    }
  }

  private func authenticate(id: UUID, provider: Provider, existing: Account?) {
    authentication = AuthenticationPresentation(id: id, provider: provider, phase: .pending)
    update(id) { $0.activity = .signingIn }
    authenticationTask = Task {
      let result: Result<AccountIdentity, ClientFailure> =
        switch (provider, existing) {
        case (.claude, .some(let account)):
          await claude.reconnect(account: account).mapError(ClientFailure.claude)
        case (.claude, .none): await claude.connect(id: id).mapError(ClientFailure.claude)
        case (.openAI, .some(let account)):
          await codex.reconnect(account: account).mapError(ClientFailure.codex)
        case (.openAI, .none): await codex.connect(id: id).mapError(ClientFailure.codex)
        }
      guard !Task.isCancelled else { return }
      switch result {
      case .success(let identity):
        await connect(id: id, provider: provider, identity: identity, existing: existing)
      case .failure(let error):
        authentication = AuthenticationPresentation(
          id: id, provider: provider, phase: .refused(error.localizedDescription))
        if existing != nil { record(error, for: id) }
        update(id) { $0.activity = .idle }
      }
    }
  }

  private func connect(
    id: UUID, provider: Provider, identity: AccountIdentity, existing: Account?
  ) async {
    if let duplicate: AccountState = states.first(where: {
      $0.id != id && $0.account.provider == provider
        && $0.account.identity.isSameAccount(as: identity)
    }) {
      authentication = AuthenticationPresentation(
        id: id, provider: provider, phase: .refused("This account is already connected"))
      update(duplicate.id) { $0.issue = nil }
      update(id) { $0.activity = .idle }
      return
    }
    if let existing, !existing.identity.isSameAccount(as: identity) {
      authentication = AuthenticationPresentation(
        id: id, provider: provider, phase: .refused("Sign in to the account you selected"))
      update(id) { state in
        state.authentication = .signInRequired
        state.activity = .idle
      }
      return
    }
    if existing == nil {
      states.append(
        .connected(
          Account(id: id, provider: provider, identity: identity, sessionPolicy: .manual)))
    }
    update(id) { state in
      state.account.identity = identity
      state.authentication = .connected
      state.issue = nil
    }
    authentication = nil
    await updateSelections()
    await save()
    update(id) { $0.activity = .idle }
    refresh(states.filter { $0.id == id })
  }

  private func discard(_ id: UUID, provider: Provider) async {
    let result: Result<Void, ClientFailure> =
      switch provider {
      case .claude: await claude.discardConnection(id: id).mapError(ClientFailure.claude)
      case .openAI: await codex.discardConnection(id: id).mapError(ClientFailure.codex)
      }
    switch result {
    case .success: providerIssues[provider] = nil
    case .failure(let error): providerIssues[provider] = error.localizedDescription
    }
  }

  private func applyUsage(_ result: Result<UsageSnapshot, ClientFailure>, to id: UUID) {
    guard let index: Int = index(of: id) else { return }
    switch result {
    case .success(let snapshot):
      states[index].usage = .current(snapshot)
      states[index].issue = nil
      states[index].sessionStart = .idle
      if case .running = snapshot.sessionState(at: Date()) {
        states[index].automaticStartAttempted = false
      }
    case .failure(let error):
      states[index].usage = states[index].usage.snapshot.map(UsageState.stale) ?? .unavailable
      record(error, for: id)
    }
  }

  private func refresh() {
    refresh(states)
    scheduleRefresh()
  }

  private func refresh(_ candidates: [AccountState]) {
    for state: AccountState in candidates where state.authentication == .connected {
      let account: Account = state.account
      perform(.refreshing, for: state.id) { [self] in
        let result: Result<UsageSnapshot, ClientFailure> =
          switch account.provider {
          case .claude:
            await claude.usage(for: account, accounts: records).mapError(ClientFailure.claude)
          case .openAI: await codex.usage(for: account).mapError(ClientFailure.codex)
          }
        applyUsage(result, to: state.id)
      }
    }
  }

  private func scheduleRefresh() {
    scheduledRefresh?.cancel()
    guard isStorageAvailable, !isStopping else { return }
    scheduledRefresh = Task {
      while (try? await Task.sleep(for: Self.refreshInterval)) != nil {
        refresh(
          states.filter {
            isMenuBarExtraVisible || $0.account.sessionPolicy == .automatic
              || $0.sessionStart == .awaitingConfirmation
          })
      }
    }
  }

  private func perform(
    _ operation: AccountOperation, for id: UUID, _ work: @escaping @MainActor () async -> Void
  ) {
    guard isStorageAvailable, !isStopping, let index: Int = index(of: id),
      case .idle = states[index].activity
    else { return }
    states[index].issue = nil
    let task: Task<Void, Never> = Task {
      await work()
      await save()
      update(id) { $0.activity = .idle }
      startAutomaticSessionIfNeeded(id)
    }
    states[index].activity = .running(operation, task)
  }

  private func startAutomaticSessionIfNeeded(_ id: UUID) {
    guard !isStopping, let index: Int = index(of: id),
      states[index].account.sessionPolicy == .automatic,
      states[index].authentication == .connected, !states[index].automaticStartAttempted,
      states[index].sessionStart == .idle, case .idle = states[index].activity,
      case .current(let snapshot) = states[index].usage,
      case .idle = snapshot.sessionState(at: Date())
    else { return }
    states[index].automaticStartAttempted = true
    startSession(id)
  }

  private func index(of id: UUID) -> Int? {
    states.firstIndex { $0.id == id }
  }

  private func update(_ id: UUID, _ change: (inout AccountState) -> Void) {
    guard let index: Int = index(of: id) else { return }
    change(&states[index])
  }

  private func record(_ error: ClientFailure, for id: UUID) {
    guard let index: Int = index(of: id) else { return }
    states[index].issue = error.localizedDescription
    if error.requiresSignIn {
      states[index].authentication = .signInRequired
      let provider: Provider = states[index].account.provider
      if selections[provider] == id { selections[provider] = nil }
    }
    logger.error("\(String(describing: error), privacy: .private)")
  }

  private func scheduleSave() {
    saveTask = Task { await save() }
  }

  @discardableResult
  private func save() async -> Result<Void, AccountStorageFailure> {
    guard isStorageAvailable else { return .failure(.unavailable) }
    let snapshot: SavedAccounts = SavedAccounts(
      accounts: states.map(\.saved), selected: selections)
    let result: Result<Void, AccountStorageFailure> = await storage.save(snapshot)
    switch result {
    case .success: storageIssue = nil
    case .failure(let error):
      storageIssue = error.localizedDescription
      logger.error("\(String(describing: error), privacy: .private)")
    }
    return result
  }

  private struct AccountState: Identifiable {
    var account: Account
    var authentication: AuthenticationState
    var usage: UsageState
    var sessionStart: SessionStartState
    var activity: Activity
    var issue: String?
    var automaticStartAttempted: Bool

    var id: UUID { account.id }

    static func restored(_ saved: SavedAccount) -> AccountState {
      AccountState(
        account: saved.account, authentication: saved.authentication,
        usage: saved.usage.map(UsageState.stale) ?? .unavailable,
        sessionStart: saved.sessionStart, activity: .idle, issue: nil,
        automaticStartAttempted: false
      )
    }

    static func connected(_ account: Account) -> AccountState {
      AccountState(
        account: account, authentication: .connected, usage: .unavailable, sessionStart: .idle,
        activity: .idle, issue: nil, automaticStartAttempted: false
      )
    }

    var saved: SavedAccount {
      SavedAccount(
        account: account, authentication: authentication, sessionStart: sessionStart,
        usage: usage.snapshot)
    }

    var operation: AccountOperation {
      switch activity {
      case .idle: sessionStart == .awaitingConfirmation ? .awaitingWindow : .idle
      case .signingIn: .signingIn
      case .running(let operation, _): operation
      }
    }
  }

  private enum Activity {
    case idle
    case signingIn
    case running(AccountOperation, Task<Void, Never>)
  }

  nonisolated private enum ClientFailure: LocalizedError {
    case claude(ClaudeFailure)
    case codex(CodexFailure)

    var errorDescription: String? {
      switch self {
      case .claude(let error): error.localizedDescription
      case .codex(let error): error.localizedDescription
      }
    }

    var requiresSignIn: Bool {
      switch self {
      case .claude(let error): error.requiresSignIn
      case .codex(let error): error.requiresSignIn
      }
    }

    var awaitingSessionConfirmation: Bool {
      switch self {
      case .claude(let error): error.awaitingSessionConfirmation
      case .codex(let error): error.awaitingSessionConfirmation
      }
    }
  }
}
