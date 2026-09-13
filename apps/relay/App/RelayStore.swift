import AppKit
import Foundation
import OSLog
import Observation
import ServiceManagement

@MainActor
@Observable
final class RelayStore {
  private var states: [AccountState] = []
  private var selections: [RelayProvider: UUID] = [:]
  private(set) var expandedSessions: Set<UUID> = []
  private(set) var authentication: AuthenticationPresentation?
  private(set) var launchAtLogin: LoginItemState = LoginItemState(
    registration: .disabled, issue: nil)
  private(set) var isLoading: Bool = true
  private var storageIssue: String?
  private var providerIssues: [RelayProvider: String] = [:]

  @ObservationIgnored private let repository: AccountRepository
  @ObservationIgnored private let claude: ClaudeClient
  @ObservationIgnored private let codex: CodexClient
  @ObservationIgnored private let logger: Logger = Logger(
    subsystem: "app.rasm.relay", category: "Accounts")
  @ObservationIgnored private var launchTask: Task<Void, Never>?
  @ObservationIgnored private var monitorTask: Task<Void, Never>?
  @ObservationIgnored private var authenticationTask: Task<Void, Never>?
  @ObservationIgnored private var loginItemTask: Task<Void, Never>?
  @ObservationIgnored private var persistenceTask: Task<Void, Never>?
  @ObservationIgnored private var wakeObserver: NSObjectProtocol?
  @ObservationIgnored private var panelVisible: Bool = false
  @ObservationIgnored private var stopping: Bool = false
  @ObservationIgnored private var storageAvailable: Bool = false

  private static let refreshInterval: Duration = .seconds(60)

  init() {
    let environment: [String: String] = ProcessInfo.processInfo.environment
    let home: URL = FileManager.default.homeDirectoryForCurrentUser
    let support: URL = home.appending(
      path: "Library/Application Support/Relay", directoryHint: .isDirectory)
    let claudeDirectory: URL =
      environment["CLAUDE_CONFIG_DIR"].map {
        URL(filePath: $0, directoryHint: .isDirectory)
      } ?? home.appending(path: ".claude", directoryHint: .isDirectory)
    let paths: RelayPaths = RelayPaths(
      applicationSupport: support, defaultClaudeDirectory: claudeDirectory)
    repository = AccountRepository(fileURL: paths.accountsFile)
    claude = ClaudeClient(paths: paths, environment: environment)
    codex = CodexClient(paths: paths, environment: environment)
  }

  var accounts: [AccountPresentation] {
    states.map { state in
      state.presentation(isSelected: selections[state.account.provider] == state.id)
    }
  }

  var issue: String? {
    let messages: [String] =
      [storageIssue].compactMap { $0 }
      + RelayProvider.allCases.compactMap { providerIssues[$0] }
    return messages.isEmpty ? nil : messages.joined(separator: "\n")
  }

  var canAddAccount: Bool { storageAvailable && !stopping && authentication == nil }

  private var records: [RelayAccount] { states.map(\.account) }

  func start() {
    guard launchTask == nil else { return }
    launchAtLogin = LoginItem.state()
    launchTask = Task {
      await load()
      guard storageAvailable, !stopping else { return }
      await reconcileSelections()
      refresh()
    }
    wakeObserver = NSWorkspace.shared.notificationCenter.addObserver(
      forName: NSWorkspace.didWakeNotification, object: nil, queue: .main
    ) { _ in
      Task { @MainActor in self.refresh() }
    }
  }

  func stop() async {
    stopping = true
    monitorTask?.cancel()
    authenticationTask?.cancel()
    if let wakeObserver {
      NSWorkspace.shared.notificationCenter.removeObserver(wakeObserver)
      self.wakeObserver = nil
    }
    if let authentication {
      switch authentication.provider {
      case .claude: await claude.cancel(accountID: authentication.id)
      case .openAI: await codex.cancel(accountID: authentication.id)
      }
    }
    // Credential handoffs finish before application termination
    for case .job(_, let task) in states.map(\.activity) { await task.value }
    let pending: [Task<Void, Never>] = [
      authenticationTask, loginItemTask, persistenceTask, monitorTask, launchTask,
    ].compactMap { $0 }
    for task: Task<Void, Never> in pending { await task.value }
    await claude.cancelOperations()
    await codex.cancelOperations()
    if storageAvailable { await persist() }
  }

  func setPanelVisible(_ visible: Bool) {
    panelVisible = visible
    guard visible else { return }
    launchAtLogin = LoginItem.state()
    refresh()
  }

  func activateProvider(_ id: UUID) {
    guard let index: Int = index(of: id), states[index].authentication == .connected else {
      return
    }
    if case .current(let snapshot) = states[index].usage,
      case .running = snapshot.sessionState(at: Date())
    {
      expandedSessions.formSymmetricDifference([id])
      return
    }
    beginSession(id)
  }

  func select(_ id: UUID) {
    guard let index: Int = index(of: id), states[index].authentication == .connected else {
      return
    }
    let account: RelayAccount = states[index].account
    begin(id, .selecting) { [self] in
      let result: Result<AccountSelection, ClientFailure> =
        switch account.provider {
        case .claude: await claude.select(account, accounts: records).mapError(ClientFailure.claude)
        case .openAI: await codex.select(account).mapError(ClientFailure.codex)
        }
      switch result {
      case .success(let selection):
        update(id) { $0.account.identity = selection.identity }
        if let preserved: RelayAccount = selection.preservedAccount,
          !states.contains(where: { $0.id == preserved.id })
        {
          states.append(.connected(preserved))
        }
        selections[account.provider] = id
        providerIssues[account.provider] = nil
      case .failure(let error):
        report(error, for: id)
        await recoverClaudeAccounts()
        await reconcileSelections()
      }
    }
  }

  func addAccount(_ provider: RelayProvider) {
    guard storageAvailable, authentication == nil, !stopping else { return }
    authenticate(id: UUID(), provider: provider, existing: nil)
  }

  func signIn(_ id: UUID) {
    guard let index: Int = index(of: id), case .idle = states[index].activity,
      authentication == nil, !stopping
    else { return }
    let account: RelayAccount = states[index].account
    authenticate(id: id, provider: account.provider, existing: account)
  }

  func cancelAuthentication() {
    guard let pending: AuthenticationPresentation = authentication else { return }
    let previous: Task<Void, Never>? = authenticationTask
    previous?.cancel()
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
    let account: RelayAccount = states[index].account
    begin(id, .signingOut) { [self] in
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
        forget(id, provider: account.provider)
      case .failure(let error): report(error, for: id)
      }
    }
  }

  func remove(_ id: UUID) {
    guard let index: Int = index(of: id) else { return }
    let account: RelayAccount = states[index].account
    begin(id, .removing) { [self] in
      let result: Result<Void, ClientFailure> =
        switch account.provider {
        case .claude: await claude.remove(account, accounts: records).mapError(ClientFailure.claude)
        case .openAI: await codex.remove(account).mapError(ClientFailure.codex)
        }
      switch result {
      case .success:
        states.removeAll { $0.id == id }
        forget(id, provider: account.provider)
      case .failure(let error): report(error, for: id)
      }
    }
  }

  func setSessionPolicy(_ policy: SessionPolicy, for id: UUID) {
    update(id) { state in
      state.account.sessionPolicy = policy
      state.automaticStartAttempted = false
    }
    saveConfiguration()
    if policy == .automatic { refresh(states.filter { $0.id == id }) }
  }

  func moveAccount(_ id: UUID, by offset: Int) {
    guard let index: Int = index(of: id), states.indices.contains(index + offset) else { return }
    states.swapAt(index, index + offset)
    saveConfiguration()
  }

  func moveAccounts(from offsets: IndexSet, to destination: Int) {
    states.move(fromOffsets: offsets, toOffset: destination)
    saveConfiguration()
  }

  func setLaunchAtLogin(_ enabled: Bool) {
    guard loginItemTask == nil else { return }
    loginItemTask = Task {
      launchAtLogin = await LoginItem.setEnabled(enabled)
      loginItemTask = nil
    }
  }

  func openLoginItems() {
    SMAppService.openSystemSettingsLoginItems()
  }

  private func load() async {
    defer { isLoading = false }
    switch await repository.load() {
    case .success(let saved):
      states = saved.accounts.map(AccountState.restored)
      storageAvailable = true
      await recoverClaudeAccounts()
    case .failure(let error):
      storageIssue = error.userMessage
      logger.error("\(String(describing: error), privacy: .private)")
    }
  }

  private func recoverClaudeAccounts() async {
    switch await claude.recoverAccounts() {
    case .success(let recovered):
      providerIssues[.claude] = nil
      states += recovered.filter { index(of: $0.id) == nil }.map(AccountState.connected)
    case .failure(let error):
      providerIssues[.claude] = error.userMessage
    }
  }

  private func reconcileSelections() async {
    async let claudeSelection: Result<UUID?, ClaudeFailure> = claude.selectedAccount(in: records)
    async let codexSelection: Result<UUID?, CodexFailure> = codex.selectedAccount(in: records)
    let outcomes: [(RelayProvider, Result<UUID?, ClientFailure>)] = [
      (.claude, await claudeSelection.mapError(ClientFailure.claude)),
      (.openAI, await codexSelection.mapError(ClientFailure.codex)),
    ]
    for (provider, outcome): (RelayProvider, Result<UUID?, ClientFailure>) in outcomes {
      switch outcome {
      case .success(let id):
        selections[provider] = id
        providerIssues[provider] = nil
      case .failure(let error):
        selections[provider] = nil
        providerIssues[provider] = error.userMessage
      }
    }
  }

  private func authenticate(id: UUID, provider: RelayProvider, existing: RelayAccount?) {
    authentication = AuthenticationPresentation(id: id, provider: provider, issue: nil)
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
          id: id, provider: provider, issue: error.userMessage)
        if existing != nil { report(error, for: id) }
        update(id) { $0.activity = .idle }
      }
    }
  }

  // Sign-in succeeded: the identity joins its account or is refused as a duplicate.
  // A refused new connection is discarded when the sheet is cancelled.
  private func connect(
    id: UUID, provider: RelayProvider, identity: AccountIdentity, existing: RelayAccount?
  ) async {
    if let duplicate: AccountState = states.first(where: {
      $0.id != id && $0.account.provider == provider && $0.account.identity.identifies(identity)
    }) {
      authentication = AuthenticationPresentation(
        id: id, provider: provider, issue: "This account is already connected")
      update(duplicate.id) { $0.issue = nil }
      update(id) { $0.activity = .idle }
      return
    }
    if let existing, !existing.identity.identifies(identity) {
      authentication = AuthenticationPresentation(
        id: id, provider: provider, issue: "Sign in to the account you selected")
      update(id) { state in
        state.authentication = .signInRequired
        state.activity = .idle
      }
      return
    }
    if existing == nil {
      states.append(
        .connected(
          RelayAccount(id: id, provider: provider, identity: identity, sessionPolicy: .manual)))
    }
    update(id) { state in
      state.account.identity = identity
      state.authentication = .connected
      state.issue = nil
    }
    authentication = nil
    await reconcileSelections()
    await persist()
    update(id) { $0.activity = .idle }
    refresh(states.filter { $0.id == id })
  }

  private func discard(_ id: UUID, provider: RelayProvider) async {
    let result: Result<Void, ClientFailure> =
      switch provider {
      case .claude: await claude.discardConnection(id: id).mapError(ClientFailure.claude)
      case .openAI: await codex.discardConnection(id: id).mapError(ClientFailure.codex)
      }
    switch result {
    case .success: providerIssues[provider] = nil
    case .failure(let error): providerIssues[provider] = error.userMessage
    }
  }

  private func beginSession(_ id: UUID) {
    guard let index: Int = index(of: id), states[index].sessionStart == .idle else { return }
    let account: RelayAccount = states[index].account
    begin(id, .starting) { [self] in
      // The pending mark is durable before the greeting so an interrupted start is not repeated
      update(id) { $0.sessionStart = .awaitingConfirmation }
      guard case .success = await persist() else {
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

  private func readUsage(_ account: RelayAccount) async -> Result<UsageSnapshot, ClientFailure> {
    switch account.provider {
    case .claude: await claude.usage(for: account, accounts: records).mapError(ClientFailure.claude)
    case .openAI: await codex.usage(for: account).mapError(ClientFailure.codex)
    }
  }

  private func applyUsage(_ result: Result<UsageSnapshot, ClientFailure>, to id: UUID) {
    guard let index: Int = index(of: id) else { return }
    switch result {
    case .success(let snapshot):
      let previous: UsageSnapshot? = states[index].usage.snapshot
      states[index].usage = .current(snapshot)
      states[index].issue = nil
      if case .running = snapshot.sessionState(at: Date()) {
        states[index].automaticStartAttempted = false
        states[index].sessionStart = .idle
      } else if let previous, case .running = previous.sessionState(at: previous.observedAt) {
        states[index].automaticStartAttempted = false
      }
    case .failure(let error):
      states[index].usage = states[index].usage.snapshot.map(UsageState.stale) ?? .unavailable
      report(error, for: id)
    }
  }

  // Every connected account now, and the monitor cadence restarts from now
  private func refresh() {
    refresh(states)
    startMonitoring()
  }

  private func refresh(_ candidates: [AccountState]) {
    for state: AccountState in candidates where state.authentication == .connected {
      begin(state.id, .refreshing) { [self] in
        applyUsage(await readUsage(state.account), to: state.id)
      }
    }
  }

  private func startMonitoring() {
    monitorTask?.cancel()
    guard storageAvailable, !stopping else { return }
    monitorTask = Task {
      do {
        while true {
          try await Task.sleep(for: Self.refreshInterval)
          guard !stopping else { return }
          refresh(
            states.filter {
              panelVisible || $0.account.sessionPolicy == .automatic
                || $0.sessionStart == .awaitingConfirmation
            })
        }
      } catch {
        return
      }
    }
  }

  // One job per account: the job owns its slot, persists after its work, and clears the slot
  private func begin(
    _ id: UUID, _ operation: AccountOperation, work: @escaping @MainActor () async -> Void
  ) {
    guard storageAvailable, !stopping, let index: Int = index(of: id),
      case .idle = states[index].activity
    else { return }
    states[index].issue = nil
    let task: Task<Void, Never> = Task {
      await work()
      await persist()
      update(id) { $0.activity = .idle }
      considerAutomaticSession(id)
    }
    states[index].activity = .job(operation, task)
  }

  private func considerAutomaticSession(_ id: UUID) {
    guard !stopping, let index: Int = index(of: id),
      states[index].account.sessionPolicy == .automatic,
      states[index].authentication == .connected, !states[index].automaticStartAttempted,
      states[index].sessionStart == .idle, case .idle = states[index].activity,
      case .current(let snapshot) = states[index].usage,
      case .idle = snapshot.sessionState(at: Date())
    else { return }
    states[index].automaticStartAttempted = true
    beginSession(id)
  }

  private func index(of id: UUID) -> Int? {
    states.firstIndex { $0.id == id }
  }

  private func update(_ id: UUID, _ change: (inout AccountState) -> Void) {
    guard let index: Int = index(of: id) else { return }
    change(&states[index])
  }

  private func forget(_ id: UUID, provider: RelayProvider) {
    expandedSessions.remove(id)
    if selections[provider] == id { selections[provider] = nil }
  }

  private func report(_ error: ClientFailure, for id: UUID) {
    guard let index: Int = index(of: id) else { return }
    states[index].issue = error.userMessage
    if error.requiresSignIn {
      states[index].authentication = .signInRequired
      let provider: RelayProvider = states[index].account.provider
      if selections[provider] == id { selections[provider] = nil }
    }
    logger.error("\(error.userMessage, privacy: .private)")
  }

  private func saveConfiguration() {
    persistenceTask = Task { await persist() }
  }

  @discardableResult
  private func persist() async -> Result<Void, AccountStorageFailure> {
    guard storageAvailable else { return .failure(.unavailable) }
    let snapshot: SavedAccounts = SavedAccounts(
      accounts: states.map(\.saved), selected: selections)
    let result: Result<Void, AccountStorageFailure> = await repository.save(snapshot)
    switch result {
    case .success: storageIssue = nil
    case .failure(let error):
      storageIssue = error.userMessage
      logger.error("\(String(describing: error), privacy: .private)")
    }
    return result
  }

  private struct AccountState: Identifiable {
    var account: RelayAccount
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

    static func connected(_ account: RelayAccount) -> AccountState {
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
      case .job(let operation, _): operation
      }
    }

    func presentation(isSelected: Bool) -> AccountPresentation {
      AccountPresentation(
        account: account, authentication: authentication, usageState: usage,
        operation: operation, issue: issue, isSelected: isSelected
      )
    }
  }

  private enum Activity {
    case idle
    case signingIn
    case job(AccountOperation, Task<Void, Never>)
  }

  // Swift's Result needs a concrete Error, so the two provider failures meet in one closed sum
  private enum ClientFailure: Error {
    case claude(ClaudeFailure)
    case codex(CodexFailure)

    var userMessage: String {
      switch self {
      case .claude(let error): error.userMessage
      case .codex(let error): error.userMessage
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
