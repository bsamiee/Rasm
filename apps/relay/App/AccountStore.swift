import AppKit
import Foundation
import Network
import OSLog
import Observation
import SwiftUI

@Observable
final class AccountStore {
  private(set) var accounts: [AccountModel] = []
  private(set) var authentication: AuthenticationPresentation?
  private(set) var loginItem: LoginItem = .current
  private(set) var isLoading: Bool = true
  private(set) var isMenuBarExtraVisible: Bool = false
  private var storageIssue: String?
  private var providerIssues: [Provider: String] = [:]
  private(set) var codexPrecondition: String?

  @ObservationIgnored private let process: [String: String]
  @ObservationIgnored private let locations: FileLocations
  @ObservationIgnored private let storage: AccountStorage
  @ObservationIgnored private var claude: ClaudeClient
  @ObservationIgnored private var codex: CodexClient
  @ObservationIgnored private let logger: Logger = Logger(
    subsystem: "app.rasm.relay", category: "Accounts")
  @ObservationIgnored private var root: Task<Void, Never>?
  @ObservationIgnored private var authenticationTask: Task<Void, Never>?
  @ObservationIgnored private var authenticationCodes: AsyncStream<String>.Continuation?
  @ObservationIgnored private var saveTask: Task<Void, Never>?
  @ObservationIgnored private var refreshPause: Task<Void, any Error>?
  @ObservationIgnored private var lastPanelOpen: Date = .distantPast
  private var isStorageAvailable: Bool = false
  private var isStopping: Bool = false

  private static let panelRefreshInterval: Duration = .seconds(60)
  private static let stopDeadline: Duration = .seconds(5)

  init(process: [String: String]) {
    self.process = process
    locations = Self.locations(home: URL.homeDirectory, environment: process)
    storage = AccountStorage(fileURL: locations.accountsFile)
    claude = ClaudeClient(paths: locations, environment: process)
    codex = CodexClient(paths: locations, environment: process)
  }

  private static func locations(home: URL, environment: [String: String]) -> FileLocations {
    let claudeDirectory: URL =
      environment["CLAUDE_CONFIG_DIR"].flatMap { value in
        value.isEmpty ? nil : URL(filePath: value, directoryHint: .isDirectory)
      } ?? home.appending(path: ".claude", directoryHint: .isDirectory)
    let codexHome: URL =
      environment["CODEX_HOME"].flatMap { value in
        value.isEmpty ? nil : URL(filePath: value, directoryHint: .isDirectory)
      } ?? home.appending(path: ".codex", directoryHint: .isDirectory)
    return FileLocations(
      applicationSupportDirectory: URL.applicationSupportDirectory.appending(
        path: "Relay", directoryHint: .isDirectory),
      defaultClaudeDirectory: claudeDirectory, defaultCodexHome: codexHome)
  }

  private func configure(environment: [String: String]) {
    let locations: FileLocations = Self.locations(home: URL.homeDirectory, environment: environment)
    claude = ClaudeClient(paths: locations, environment: environment)
    codex = CodexClient(paths: locations, environment: environment)
  }

  var issue: String? {
    let messages: [String] =
      [storageIssue].compactMap { $0 }
      + Provider.allCases.compactMap { providerIssues[$0] }
    return messages.isEmpty ? nil : messages.joined(separator: "\n")
  }

  var canAddAccount: Bool { isStorageAvailable && !isStopping && authentication == nil }

  var isSwitching: Bool { accounts.contains { model in model.operation == .selecting } }

  private var records: [Account] { accounts.map(\.account) }

  func start() {
    guard root == nil else { return }
    root = Task(name: "Accounts") { [self] in
      switch await LoginShell.exports(over: process) {
      case .success(let environment): configure(environment: environment)
      case .failure(let error):
        logger.error(
          "Login shell exports unavailable: \(String(describing: error), privacy: .public)")
      }
      await load()
      guard isStorageAvailable, !isStopping else { return }
      await settle()
      await readSelection()
      await withDiscardingTaskGroup { group in
        group.addTask(name: "Network") { await self.observeNetwork() }
        group.addTask(name: "Wake") { await self.observeWake() }
        group.addTask(name: "Claude selection") {
          await self.observe(file: self.claude.sharedConfigFile)
        }
        group.addTask(name: "Claude refresh lock") {
          await self.observeRefreshLock(self.claude.sharedRefreshLock)
        }
        group.addTask(name: "Codex selection") { await self.observe(file: self.codex.liveAuthFile) }
        group.addTask(name: "Codex rate limits") { await self.observeCodexUpdates() }
        group.addTask(name: "Refresh schedule") { await self.schedule() }
      }
    }
  }

  func stop() async {
    isStopping = true
    root?.cancel()
    authenticationTask?.cancel()
    refreshPause?.cancel()
    for model: AccountModel in accounts { model.cancel() }
    _ = await ProcessRun.withDeadline(Self.stopDeadline) { .success(await self.awaitOutstanding()) }
    if isStorageAvailable { await save() }
  }

  private func awaitOutstanding() async {
    await root?.value
    await authenticationTask?.value
    for model: AccountModel in accounts { await model.finish() }
    await codex.shutdown()
  }

  private func reschedule() {
    refreshPause?.cancel()
  }

  func setMenuBarExtraVisible(_ visible: Bool) {
    isMenuBarExtraVisible = visible
    guard visible else { return }
    lastPanelOpen = Date()
    loginItem = .current
    Task(name: "Panel opened") { [self] in
      await readSelection()
      refresh(revalidatingSelected: false)
    }
    reschedule()
  }

  func refreshLoginItem() {
    loginItem = .current
  }

  func startSession(_ id: UUID) {
    guard let model: AccountModel = model(id), model.isConnected else { return }
    guard model.operation != .refreshing else {
      Task(name: "Session start after refresh") { [self] in
        await model.finish()
        startSession(id)
      }
      return
    }
    let account: Account = model.account
    let isSelected: Bool = model.isSelected
    model.run(.starting) { [self] in
      let result: Result<AccountUsage, ProviderError> =
        switch account.provider {
        case .claude:
          await claude.startSession(for: account, isSelected: isSelected).erased()
        case .openAI:
          await codex.startSession(for: account, isSelected: isSelected).erased()
        }
      apply(result, to: model)
    }
  }

  func cancelOperation(_ id: UUID) {
    model(id)?.cancel()
  }

  func select(_ id: UUID) {
    guard let model: AccountModel = model(id), model.canSelect else { return }
    let account: Account = model.account
    let outgoing: AccountModel? = accounts.first { other in
      other.account.provider == account.provider && other.isSelected && other.id != id
    }
    model.run(.selecting) { [self] in
      await outgoing?.finish()
      guard !Task.isCancelled else { return }
      let result: Result<AccountIdentity, ProviderError> =
        switch account.provider {
        case .claude: await claude.select(account, outgoing: outgoing?.account).erased()
        case .openAI: await codex.select(account, outgoing: outgoing?.account).erased()
        }
      switch result {
      case .success(let identity):
        model.account.identity = identity
        model.issue = nil
        for other: AccountModel in accounts where other.account.provider == account.provider {
          other.isSelected = other.id == id
        }
        providerIssues[account.provider] = nil
        scheduleSave()
      case .failure(let error):
        record(error, for: model)
        await readSelection()
      }
    }
    Task(name: "Refresh after switch") { [self] in
      await model.finish()
      guard !isStopping else { return }
      refresh([model] + (outgoing.map { [$0] } ?? []), revalidatingSelected: true)
    }
  }

  func addAccount(_ provider: Provider) {
    guard canAddAccount else { return }
    authenticate(id: UUID(), provider: provider, existing: nil)
  }

  func signIn(_ id: UUID) {
    guard let model: AccountModel = model(id), !model.isBusy, authentication == nil, !isStopping
    else { return }
    authenticate(id: id, provider: model.account.provider, existing: model)
  }

  func submitAuthenticationCode(_ code: String) {
    let trimmed: String = code.trimmingCharacters(in: .whitespacesAndNewlines)
    guard let pending: AuthenticationPresentation = authentication, pending.phase == .pending,
      let codes: AsyncStream<String>.Continuation = authenticationCodes, !trimmed.isEmpty
    else { return }
    authentication = pending.entering(.completing)
    codes.yield(trimmed)
    codes.finish()
  }

  func cancelAuthentication() {
    guard let pending: AuthenticationPresentation = authentication else { return }
    switch pending.phase {
    case .pending, .completing:
      authentication = pending.entering(.cancelling)
      authenticationTask?.cancel()
    case .refused, .cancelling: return
    }
  }

  func dismissAuthentication() {
    guard let pending: AuthenticationPresentation = authentication,
      case .refused = pending.phase
    else { return }
    authentication = nil
  }

  func signOut(_ id: UUID) {
    guard let model: AccountModel = model(id) else { return }
    let account: Account = model.account
    let isSelected: Bool = model.isSelected
    model.run(.signingOut) { [self] in
      let result: Result<Void, ProviderError> =
        switch account.provider {
        case .claude: await claude.signOut(account, isSelected: isSelected).erased()
        case .openAI: await codex.signOut(account, isSelected: isSelected).erased()
        }
      switch result {
      case .success:
        model.authentication = .signInRequired
        model.usage = .unavailable
        model.isSelected = false
        scheduleSave()
        await readSelection()
      case .failure(let error): record(error, for: model)
      }
    }
  }

  func remove(_ id: UUID) {
    guard let model: AccountModel = model(id) else { return }
    let account: Account = model.account
    let isSelected: Bool = model.isSelected
    model.run(.removing) { [self] in
      let result: Result<Void, ProviderError> =
        switch account.provider {
        case .claude: await claude.remove(account, isSelected: isSelected).erased()
        case .openAI: await codex.remove(account, isSelected: isSelected).erased()
        }
      switch result {
      case .success:
        accounts.removeAll { other in other.id == id }
        removeAccountDirectory(id)
        scheduleSave()
        await readSelection()
      case .failure(let error): record(error, for: model)
      }
    }
  }

  func setSessionPolicy(_ policy: SessionPolicy, for id: UUID) {
    guard let model: AccountModel = model(id) else { return }
    model.account.sessionPolicy = policy
    model.automaticStartAttempted = false
    scheduleSave()
    if policy == .automatic { refresh([model], revalidatingSelected: false) }
  }

  func moveAccount(_ id: UUID, by offset: Int) {
    guard let index: Int = accounts.firstIndex(where: { model in model.id == id }),
      accounts.indices.contains(index + offset)
    else { return }
    accounts.swapAt(index, index + offset)
    scheduleSave()
  }

  func moveAccounts(from offsets: IndexSet, to destination: Int) {
    accounts.move(fromOffsets: offsets, toOffset: destination)
    scheduleSave()
  }

  func setLoginItemEnabled(_ enabled: Bool) {
    Task(name: "Login item") { [self] in loginItem = await LoginItem.setEnabled(enabled) }
  }

  private func model(_ id: UUID) -> AccountModel? {
    accounts.first { model in model.id == id }
  }

  private func load() async {
    defer { isLoading = false }
    switch await storage.load() {
    case .success(let saved):
      accounts = saved.accounts.map { saved in
        AccountModel(
          account: saved.account, authentication: saved.authentication,
          usage: saved.usage.map(UsageState.stale) ?? .unavailable)
      }
      isStorageAvailable = true
    case .failure(let error):
      storageIssue = error.localizedDescription
      logger.error("\(String(describing: error), privacy: .public)")
    }
  }

  private func settle() async {
    if case .failure(let error) = await claude.settle(known: records) {
      providerIssues[.claude] = error.localizedDescription
    }
    codexPrecondition = (await codex.preconditions()).failure?.localizedDescription
    let orphans: [UUID]
    switch locations.orphanAccountDirectories(excluding: Set(accounts.map(\.id))) {
    case .success(let found): orphans = found
    case .failure(let error):
      logger.error("Orphan listing: \(String(describing: error), privacy: .public)")
      orphans = []
    }
    for id: UUID in orphans {
      if case .failure(let error) = await claude.discardConnection(id: id) {
        logger.error(
          "Orphan \(id.uuidString, privacy: .public): \(String(describing: error), privacy: .public)"
        )
      }
      if case .failure(let error) = await codex.discardConnection(id: id) {
        logger.error(
          "Orphan \(id.uuidString, privacy: .public): \(String(describing: error), privacy: .public)"
        )
      }
      removeAccountDirectory(id)
    }
  }

  private func removeAccountDirectory(_ id: UUID) {
    let directory: URL = locations.accountDirectory(id)
    do {
      try FileManager.default.removeItem(at: directory)
    } catch let error as CocoaError where error.code == .fileNoSuchFile {
      return
    } catch {
      logger.error("\(String(describing: error), privacy: .public)")
    }
  }

  private func readSelection() async {
    guard !isStopping else { return }
    async let claudeRead: Result<ClaudeSelection, ClaudeFailure> = claude.currentSelection(
      known: records)
    async let codexRead: Result<CodexSelection, CodexFailure> = codex.currentSelection(
      known: records)
    let claudeSelection: Result<ClaudeSelection, ClaudeFailure> = await claudeRead
    let codexSelection: Result<CodexSelection, CodexFailure> = await codexRead
    switch claudeSelection {
    case .success(.none): apply(selected: nil, provider: .claude)
    case .success(.known(let id)): apply(selected: id, provider: .claude)
    case .success(.unknown(let identity)): register(identity, provider: .claude)
    case .failure(let error): report(ProviderError(failure: error), provider: .claude)
    }
    switch codexSelection {
    case .success(.none): apply(selected: nil, provider: .openAI)
    case .success(.known(let id)): apply(selected: id, provider: .openAI)
    case .success(.unknown(let identity)): register(identity, provider: .openAI)
    case .failure(let error): report(ProviderError(failure: error), provider: .openAI)
    }
  }

  private func apply(selected id: UUID?, provider: Provider) {
    providerIssues[provider] = nil
    for model: AccountModel in accounts where model.account.provider == provider {
      let selected: Bool = model.id == id
      model.isSelected = selected
      if selected, model.authentication != .connected, !model.isBusy {
        refresh([model], revalidatingSelected: true)
      }
    }
  }

  private func register(_ identity: AccountIdentity, provider: Provider) {
    if let existing: AccountModel = accounts.first(where: { model in
      model.account.provider == provider && model.account.identity.isSameAccount(as: identity)
    }) {
      apply(selected: existing.id, provider: provider)
      return
    }
    let model: AccountModel = AccountModel(
      account: Account(id: UUID(), provider: provider, identity: identity, sessionPolicy: .manual),
      authentication: .connected, usage: .unavailable)
    accounts.append(model)
    apply(selected: model.id, provider: provider)
    scheduleSave()
    refresh([model], revalidatingSelected: false)
  }

  private func report(_ error: ProviderError, provider: Provider) {
    guard !error.isCancellation else { return }
    providerIssues[provider] = error.localizedDescription
    logger.error("\(String(describing: error), privacy: .public)")
  }

  private func authenticate(id: UUID, provider: Provider, existing: AccountModel?) {
    authentication = AuthenticationPresentation(
      id: id, provider: provider, phase: .pending, startedAt: Date())
    let codes: (stream: AsyncStream<String>, continuation: AsyncStream<String>.Continuation) =
      AsyncStream<String>.makeStream()
    authenticationCodes = codes.continuation
    let task: Task<Void, Never> = Task(name: "Sign in \(provider.name)") { [self] in
      let result: Result<AccountIdentity, ProviderError> =
        switch (provider, existing) {
        case (.claude, .some(let model)):
          await claude.reconnect(account: model.account, codes: codes.stream).erased()
        case (.claude, .none): await claude.connect(id: id, codes: codes.stream).erased()
        case (.openAI, .some(let model)): await codex.reconnect(account: model.account).erased()
        case (.openAI, .none): await codex.connect(id: id).erased()
        }
      codes.continuation.finish()
      authenticationCodes = nil
      switch result {
      case .success(let identity) where !Task.isCancelled:
        await connect(id: id, provider: provider, identity: identity, existing: existing)
      case .success:
        if existing == nil { await discard(id, provider: provider) }
        authentication = nil
      case .failure(let error) where error.isCancellation || Task.isCancelled:
        if existing == nil { await discard(id, provider: provider) }
        authentication = nil
      case .failure(let error):
        let message: String =
          existing != nil && error.requiresSignIn
          ? "Sign in to the account you selected" : error.localizedDescription
        authentication = AuthenticationPresentation(
          id: id, provider: provider, phase: .refused(message), startedAt: Date())
        if let existing { record(error, for: existing) }
      }
      authenticationTask = nil
    }
    authenticationTask = task
    existing?.run(.signingIn) { await task.value }
  }

  private func connect(
    id: UUID, provider: Provider, identity: AccountIdentity, existing: AccountModel?
  ) async {
    let duplicate: AccountModel? = accounts.first { model in
      model.id != id && model.account.provider == provider
        && model.account.identity.isSameAccount(as: identity)
    }
    let replaced: AccountModel?
    switch (duplicate, existing) {
    case (.some(let connected), _) where connected.isConnected || connected.isBusy:
      authentication = AuthenticationPresentation(
        id: id, provider: provider, phase: .refused("\(identity.email) is already connected"),
        startedAt: Date())
      connected.issue = nil
      if existing == nil { await discard(id, provider: provider) }
      return
    case (.some(let signedOut), .none): replaced = signedOut
    case (.some, .some), (.none, _): replaced = nil
    }
    let model: AccountModel =
      existing
      ?? AccountModel(
        account: Account(
          id: id, provider: provider, identity: identity,
          sessionPolicy: replaced?.account.sessionPolicy ?? .manual),
        authentication: .connected, usage: replaced?.usage ?? .unavailable)
    if let replaced, let index: Int = accounts.firstIndex(where: { $0.id == replaced.id }) {
      accounts[index] = model
      await discard(replaced.id, provider: provider)
    } else if existing == nil {
      accounts.append(model)
    }
    model.account.identity = identity
    model.authentication = .connected
    model.issue = nil
    authentication = nil
    await save()
    await readSelection()
    refresh([model], revalidatingSelected: true)
  }

  private func discard(_ id: UUID, provider: Provider) async {
    let result: Result<Void, ProviderError> =
      switch provider {
      case .claude: await claude.discardConnection(id: id).erased()
      case .openAI: await codex.discardConnection(id: id).erased()
      }
    removeAccountDirectory(id)
    switch result {
    case .success: providerIssues[provider] = nil
    case .failure(let error): report(error, provider: provider)
    }
  }

  private func apply(_ result: Result<AccountUsage, ProviderError>, to model: AccountModel) {
    switch result {
    case .success(let usage):
      let carried: AccountUsage = usage.keepingResets(from: model.usage.usage, at: Date())
      model.usage = .current(carried)
      model.authentication = .connected
      model.issue = nil
      if case .running = carried.availability(at: Date()) { model.automaticStartAttempted = false }
      scheduleSave()
      reschedule()
      startAutomaticSessionIfNeeded(model)
    case .failure(let error) where error.isCancellation: return
    case .failure(let error):
      model.usage = model.usage.usage.map(UsageState.stale) ?? .unavailable
      record(error, for: model)
    }
  }

  private func refresh(revalidatingSelected: Bool) {
    refresh(accounts, revalidatingSelected: revalidatingSelected)
  }

  private func refresh(_ candidates: [AccountModel], revalidatingSelected: Bool) {
    guard isStorageAvailable, !isStopping, !isSwitching else { return }
    for model: AccountModel in candidates
    where (model.isConnected || (revalidatingSelected && model.isSelected)) && !model.isBusy {
      let account: Account = model.account
      let isSelected: Bool = model.isSelected
      model.run(.refreshing) { [self] in
        let result: Result<AccountUsage, ProviderError> =
          switch account.provider {
          case .claude: await claude.usage(for: account, isSelected: isSelected).erased()
          case .openAI: await codex.usage(for: account, isSelected: isSelected).erased()
          }
        apply(result, to: model)
      }
    }
  }

  private func startAutomaticSessionIfNeeded(_ model: AccountModel) {
    guard !isStopping, model.account.sessionPolicy == .automatic, model.isConnected,
      !model.automaticStartAttempted, model.availability(at: Date()) == .ready
    else { return }
    model.automaticStartAttempted = true
    Task(name: "Automatic session start") { [self] in
      await model.finish()
      startSession(model.id)
    }
  }

  private func observeNetwork() async {
    var previous: NWPath.Status? = nil
    for await path in NWPathMonitor() {
      defer { previous = path.status }
      guard path.status == .satisfied, previous != .satisfied, !isStopping else { continue }
      await readSelection()
      refresh(revalidatingSelected: true)
    }
  }

  private func observeWake() async {
    for await _ in NSWorkspace.shared.notificationCenter.notifications(
      named: NSWorkspace.didWakeNotification)
    {
      await readSelection()
    }
  }

  private func observe(file: URL) async {
    do {
      for try await _ in FileWatch.changes(of: file) {
        guard !isSwitching else { continue }
        let before: [UUID: Bool] = Dictionary(
          uniqueKeysWithValues: accounts.map { model in (model.id, model.isSelected) })
        await readSelection()
        codexPrecondition = (await codex.preconditions()).failure?.localizedDescription
        let changed: [AccountModel] = accounts.filter { model in
          before[model.id] != model.isSelected
        }
        refresh(changed, revalidatingSelected: true)
      }
    } catch {
      logger.error(
        "Watch on \(file.path, privacy: .private): \(String(describing: error), privacy: .public)")
    }
  }

  private func observeRefreshLock(_ lock: URL) async {
    do {
      for try await present in FileWatch.presence(of: lock) {
        guard !isSwitching, !present else { continue }
        await readSelection()
      }
    } catch {
      logger.error(
        "Watch on \(lock.path, privacy: .private): \(String(describing: error), privacy: .public)")
    }
  }

  private func observeCodexUpdates() async {
    for await update in codex.updates {
      guard
        let model: AccountModel = accounts.first(where: { model in
          model.account.provider == .openAI
            && codex.home(for: model.account, isSelected: model.isSelected) == update.home
        })
      else { continue }
      let previous: AccountUsage? = model.usage.usage
      apply(
        .success(
          AccountUsage(
            windows: update.windows, includedUsageAllowed: previous?.includedUsageAllowed,
            observedAt: update.observedAt)),
        to: model)
    }
  }

  private func schedule() async {
    while !Task.isCancelled, !isStopping {
      let delay: Duration = nextRefreshDelay(at: Date())
      let pause: Task<Void, any Error> = Task(name: "Refresh pause") {
        try await Task.sleep(for: delay)
      }
      refreshPause = pause
      let slept: Result<Void, any Error> = await pause.result
      guard !Task.isCancelled, !isStopping else { return }
      if case .success = slept { refresh(revalidatingSelected: false) }
    }
  }

  private func nextRefreshDelay(at now: Date) -> Duration {
    let recency: TimeInterval = now.timeIntervalSince(lastPanelOpen)
    let base: Duration =
      isMenuBarExtraVisible
      ? Self.panelRefreshInterval
      : recency < 15 * 60
        ? .seconds(5 * 60) : recency < 60 * 60 ? .seconds(15 * 60) : .seconds(30 * 60)
    let boundary: Duration? = accounts.compactMap { model in model.usage.usage?.nextReset }
      .filter { reset in reset > now }
      .min()
      .map { reset in .seconds(reset.timeIntervalSince(now) + 1) }
    return min(base, boundary ?? base)
  }

  private func record(_ error: ProviderError, for model: AccountModel) {
    guard !error.isCancellation else { return }
    model.issue = error.localizedDescription
    if error.requiresSignIn {
      model.authentication = .signInRequired
      model.isSelected = false
    }
    logger.error("\(String(describing: error), privacy: .public)")
  }

  private func scheduleSave() {
    let previous: Task<Void, Never>? = saveTask
    saveTask = Task(name: "Save accounts") { [self] in
      await previous?.value
      await save()
    }
  }

  @discardableResult
  private func save() async -> Result<Void, AccountStorageFailure> {
    guard isStorageAvailable else { return .failure(.unavailable) }
    let snapshot: SavedAccounts = SavedAccounts(
      accounts: accounts.map { model in
        SavedAccount(
          account: model.account, authentication: model.authentication, usage: model.usage.usage)
      })
    let result: Result<Void, AccountStorageFailure> = await storage.save(snapshot)
    switch result {
    case .success: storageIssue = nil
    case .failure(let error):
      storageIssue = error.localizedDescription
      logger.error("\(String(describing: error), privacy: .public)")
    }
    return result
  }
}

nonisolated extension Result where Failure: ProviderFailure {
  func erased() -> Result<Success, ProviderError> {
    mapError(ProviderError.init(failure:))
  }
}
