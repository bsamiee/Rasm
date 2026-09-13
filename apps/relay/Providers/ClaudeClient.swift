import Foundation

private struct ClaudeRunningOperation: Sendable {
  let id: UUID
  let cancel: @Sendable () -> Void
  let wait: @Sendable () async -> Void
}

/// One read of the shared default store and the journal it reconciles against.
private struct ClaudeObservedSelection: Sendable {
  let selected: UUID?
  let shared: ClaudeCredentialSnapshot?
  let journal: ClaudeSelectionJournal
}

/// An account's current grant and the store that owns it.
private struct ClaudeOwnedCredentials: Sendable {
  let store: ClaudeNativeStore
  let snapshot: ClaudeCredentialSnapshot
}

private enum ClaudeSelectionMode: Sendable {
  /// Make the account the shared login, importing its grant unless it is already there.
  case select
  /// Re-import the account's grant only when it is the shared login.
  case refreshSelected
}

actor ClaudeClient {
  private let paths: RelayPaths
  private let environment: [String: String]
  private let username: String
  private let maintenanceID: UUID = UUID()
  private let session: URLSession
  private var operations: [UUID: ClaudeRunningOperation] = [:]
  private var exclusiveOperation: UUID?
  private var verifiedIdentities: [String: AccountIdentity] = [:]

  init(paths: RelayPaths, environment: [String: String]) {
    self.paths = paths
    self.environment = environment
    let candidate: String = environment["USER"] ?? NSUserName()
    username =
      candidate.range(of: "^[a-zA-Z0-9._-]+$", options: .regularExpression) != nil
      ? candidate : "claude-code-user"
    let configuration: URLSessionConfiguration = .ephemeral
    configuration.timeoutIntervalForRequest = 15
    configuration.timeoutIntervalForResource = 30
    configuration.requestCachePolicy = .reloadIgnoringLocalCacheData
    session = URLSession(configuration: configuration)
  }

  // MARK: - Store API

  func connect(id: UUID) async -> Result<AccountIdentity, ClaudeFailure> {
    await perform(accountID: id) {
      await self.connectAccount(id: id, expected: nil, policy: .manual)
    }
  }

  func reconnect(account: RelayAccount) async -> Result<AccountIdentity, ClaudeFailure> {
    await perform(accountID: account.id, exclusive: true) {
      await self.reconnectPrivately(account).bind { identity in
        let refreshed: RelayAccount = RelayAccount(
          id: account.id, provider: .claude, identity: identity,
          sessionPolicy: account.sessionPolicy
        )
        return await self.selectAccount(refreshed, accounts: [refreshed], mode: .refreshSelected)
          .map { selection in selection.identity }
      }
    }
  }

  func usage(for account: RelayAccount, accounts: [RelayAccount]) async -> Result<
    UsageSnapshot, ClaudeFailure
  > {
    await perform(accountID: account.id) {
      await self.ownedCredentials(for: account, accounts: accounts).bind { owned in
        await self.fetchUsage(owned.snapshot.grant)
      }
    }
  }

  func startSession(
    for account: RelayAccount, accounts: [RelayAccount]
  ) async -> Result<UsageSnapshot, ClaudeFailure> {
    await perform(accountID: account.id) {
      await self.ownedCredentials(for: account, accounts: accounts).bind { owned in
        await self.fetchUsage(owned.snapshot.grant).bind {
          snapshot -> Result<UsageSnapshot, ClaudeFailure> in
          switch snapshot.sessionState(at: Date()) {
          case .running: return .success(snapshot)
          case .unknown: return .failure(.sessionUnknown)
          case .idle:
            // The greeting runs on the access token alone, so the grant it used stays current.
            return await self.headless(
              store: owned.store, action: .greeting, grant: owned.snapshot.grant
            ).bind { _ in
              await self.fetchUsage(owned.snapshot.grant)
                .mapError(ClaudeFailure.sessionConfirmationPending)
            }
          }
        }
      }
    }
  }

  func select(_ account: RelayAccount, accounts: [RelayAccount]) async -> Result<
    AccountSelection, ClaudeFailure
  > {
    await perform(accountID: account.id, exclusive: true) {
      await self.selectAccount(account, accounts: accounts, mode: .select)
    }
  }

  func signOut(_ account: RelayAccount, accounts: [RelayAccount]) async -> Result<
    Void, ClaudeFailure
  > {
    await perform(accountID: account.id, exclusive: true) {
      await self.signOutAccount(account, accounts: accounts)
    }
  }

  func remove(_ account: RelayAccount, accounts: [RelayAccount]) async -> Result<
    Void, ClaudeFailure
  > {
    await perform(accountID: account.id, exclusive: true) {
      await self.signOutAccount(account, accounts: accounts).bind { _ in
        await self.discardPrivateStore(id: account.id)
      }
    }
  }

  func discardConnection(id: UUID) async -> Result<Void, ClaudeFailure> {
    await cancel(accountID: id)
    return await perform(accountID: id, exclusive: true) {
      await self.discardPrivateStore(id: id)
    }
  }

  func selectedAccount(in accounts: [RelayAccount]) async -> Result<UUID?, ClaudeFailure> {
    guard accounts.contains(where: { account in account.provider == .claude }) else {
      return .success(nil)
    }
    return await perform(accountID: maintenanceID) {
      await self.observeSelection(accounts: accounts).map { observed in observed.selected }
    }
  }

  func recoverAccounts() async -> Result<[RelayAccount], ClaudeFailure> {
    await perform(accountID: maintenanceID, exclusive: true) {
      await self.recoverSelection().bind { _ in
        await self.readJournal().flatMap { journal in
          traverse(journal.preservedAccounts) { record in
            record.account().mapError { failure in ClaudeAccountIssues(failure) }
          }
          .mapError(ClaudeFailure.invalidSavedAccounts)
        }
      }
    }
  }

  func cancel(accountID: UUID) async {
    guard let operation: ClaudeRunningOperation = operations[accountID] else { return }
    operation.cancel()
    await operation.wait()
    if operations[accountID]?.id == operation.id { operations.removeValue(forKey: accountID) }
    if exclusiveOperation == operation.id { exclusiveOperation = nil }
  }

  func cancelOperations() async {
    for operation in operations.values { operation.cancel() }
    for accountID in Array(operations.keys) { await cancel(accountID: accountID) }
  }

  // MARK: - Operation registry

  /// One operation per account at a time; an exclusive operation waits for every running one
  /// and admits no other until it finishes.
  private func perform<Value: Sendable>(
    accountID: UUID,
    exclusive: Bool = false,
    operation: @escaping @Sendable () async -> Result<Value, ClaudeFailure>
  ) async -> Result<Value, ClaudeFailure> {
    guard exclusiveOperation == nil, operations[accountID] == nil else {
      return .failure(.operationInProgress)
    }
    let preceding: [ClaudeRunningOperation] = exclusive ? Array(operations.values) : []
    let id: UUID = UUID()
    let task: Task<Result<Value, ClaudeFailure>, Never> = Task {
      for previous in preceding { await previous.wait() }
      guard !Task.isCancelled else { return .failure(.cancelled) }
      return await operation()
    }
    operations[accountID] = ClaudeRunningOperation(
      id: id, cancel: { task.cancel() }, wait: { _ = await task.value }
    )
    if exclusive { exclusiveOperation = id }
    let result: Result<Value, ClaudeFailure> = await withTaskCancellationHandler {
      await task.value
    } onCancel: {
      task.cancel()
    }
    if operations[accountID]?.id == id { operations.removeValue(forKey: accountID) }
    if exclusiveOperation == id { exclusiveOperation = nil }
    return result
  }

  private func requestCancellation(accountID: UUID) {
    operations[accountID]?.cancel()
  }

  private func compromise(_ accountID: UUID) -> @Sendable () async -> Void {
    { [weak self] in await self?.requestCancellation(accountID: accountID) }
  }

  /// Holds Claude Code's refresh locks on the directories while the body runs.
  private func refreshing<Value: Sendable>(
    _ directories: [URL], operationID: UUID,
    _ body: (ClaudeRefreshLease) async -> Result<Value, ClaudeFailure>
  ) async -> Result<Value, ClaudeFailure> {
    switch await ClaudeRefreshLease.acquire(
      directories: directories, onCompromise: compromise(operationID))
    {
    case .success(let lease): return await lease.finish(await body(lease))
    case .failure(let error): return .failure(error)
    }
  }

  /// Holds Claude Code's storage-write lock on the store while the body writes it.
  private func writing<Value: Sendable>(
    _ store: ClaudeNativeStore, operationID: UUID,
    _ body: () async -> Result<Value, ClaudeFailure>
  ) async -> Result<Value, ClaudeFailure> {
    switch await ClaudeRefreshLease.storageWrite(
      directory: store.directory, onCompromise: compromise(operationID))
    {
    case .success(let lease): return await lease.finish(await body())
    case .failure(let error): return .failure(error)
    }
  }

  // MARK: - Connection

  private func connectAccount(
    id: UUID, expected: AccountIdentity?, policy: SessionPolicy
  ) async -> Result<AccountIdentity, ClaudeFailure> {
    let store: ClaudeNativeStore = privateStore(id)
    return await authenticate(store: store, email: expected?.email, grant: nil, selection: nil)
      .bind { _ in await store.read() }
      .flatMap { snapshot -> Result<AccountIdentity, ClaudeFailure> in
        guard let snapshot else { return .failure(.signInRequired) }
        if let expected, !snapshot.identity.identifies(expected) {
          return .failure(.accountChanged)
        }
        let account: RelayAccount = RelayAccount(
          id: id, provider: .claude, identity: snapshot.identity, sessionPolicy: policy)
        return updateJournal { journal in
          journal.preservedAccounts.removeAll { record in record.id == id }
          journal.preservedAccounts.append(ClaudeStoredAccount(account))
          journal.unavailableAccountIDs.remove(id)
        }.map { _ in snapshot.identity }
      }
  }

  private func reconnectPrivately(_ account: RelayAccount) async -> Result<
    AccountIdentity, ClaudeFailure
  > {
    let store: ClaudeNativeStore = privateStore(account.id)
    return await refreshing([store.directory], operationID: account.id) {
      lease -> Result<AccountIdentity, ClaudeFailure> in
      let previous: ClaudeCredentialSnapshot?
      switch await store.read() {
      case .success(let value): previous = value
      case .failure(let error): return .failure(error)
      }
      let signedIn: Result<AccountIdentity, ClaudeFailure> = await connectAccount(
        id: account.id, expected: account.identity, policy: account.sessionPolicy
      )
      guard case .failure(let error) = signedIn else { return signedIn }
      if case .failure(let compromised) = await lease.status() {
        return .failure(.cleanup(operation: error, release: compromised))
      }
      // Browser login obtains a new grant and native re-login preserves the previous grant.
      // Restoration owns its lifetime so a cancelled login still restores.
      let restored: Result<Void, ClaudeFailure> = await Task {
        await self.restorePrivateConnection(previous, store: store, operationID: account.id)
      }.value
      return .failure(error.releasing(restored))
    }
  }

  private func restorePrivateConnection(
    _ previous: ClaudeCredentialSnapshot?, store: ClaudeNativeStore, operationID: UUID
  ) async -> Result<Void, ClaudeFailure> {
    if let previous { return await park(previous, in: store, operationID: operationID) }
    return await writing(store, operationID: operationID) {
      await store.deletePrivateCredentials()
    }
  }

  // MARK: - Credentials

  /// Reads the account's grant once from the store that owns it, refreshes it through Claude
  /// Code when it is about to expire, and confirms the identity it belongs to.
  private func ownedCredentials(
    for account: RelayAccount, accounts: [RelayAccount]
  ) async -> Result<ClaudeOwnedCredentials, ClaudeFailure> {
    guard account.provider == .claude else { return .failure(.invalidCredentials) }
    let observed: ClaudeObservedSelection
    switch await observeSelection(accounts: accounts) {
    case .success(let value): observed = value
    case .failure(let error): return .failure(error)
    }
    let owner: ClaudeNativeStore
    let stored: ClaudeCredentialSnapshot?
    if observed.selected == account.id {
      owner = defaultStore()
      stored = observed.shared
    } else {
      guard !observed.journal.unavailableAccountIDs.contains(account.id) else {
        return .failure(.accountUnavailable)
      }
      owner = privateStore(account.id)
      switch await owner.read() {
      case .success(let value): stored = value
      case .failure(let error): return .failure(error)
      }
    }
    guard let stored else { return .failure(.signInRequired) }
    guard stored.identity.identifies(account.identity) else { return .failure(.accountChanged) }
    var current: ClaudeCredentialSnapshot = stored
    if stored.grant.needsRefresh(at: Date()) {
      guard stored.grant.refreshToken != nil else { return .failure(.signInRequired) }
      if case .failure(let error) = await headless(store: owner, action: .refreshCredentials) {
        return .failure(error)
      }
      switch await owner.read() {
      case .success(.some(let refreshed)) where refreshed.identity.identifies(account.identity):
        current = refreshed
      case .success(.some): return .failure(.accountChanged)
      case .success(.none): return .failure(.signInRequired)
      case .failure(let error): return .failure(error)
      }
    }
    guard !Task.isCancelled else { return .failure(.cancelled) }
    return await verify(current).map { snapshot in
      ClaudeOwnedCredentials(store: owner, snapshot: snapshot)
    }
  }

  /// Confirms through the profile endpoint, once per access token, that the grant belongs to
  /// the account the configuration names.
  private func verify(_ snapshot: ClaudeCredentialSnapshot) async -> Result<
    ClaudeCredentialSnapshot, ClaudeFailure
  > {
    let fingerprint: String = snapshot.grant.fingerprint
    if let identity: AccountIdentity = verifiedIdentities[fingerprint] {
      return identity.identifies(snapshot.identity) ? .success(snapshot) : .failure(.accountChanged)
    }
    return await fetch(
      "https://api.anthropic.com/api/oauth/profile", bearer: snapshot.grant.accessToken,
      headers: ["Content-Type": "application/json", "Cache-Control": "no-cache"]
    )
    .flatMap { data in decode(JSONValue.self, from: data) }
    .flatMap { profile -> Result<AccountIdentity, ClaudeFailure> in
      guard let accountID: String = profile["account"]?["uuid"]?.stringValue,
        let email: String = profile["account"]?["email"]?.stringValue,
        let organizationID: String = profile["organization"]?["uuid"]?.stringValue
      else {
        return .failure(.invalidResponse)
      }
      return AccountIdentity.make(
        accountID: accountID, organizationID: organizationID, email: email,
        plan: snapshot.grant.plan
      ).mapError(ClaudeFailure.invalidIdentity)
    }
    .flatMap { identity -> Result<ClaudeCredentialSnapshot, ClaudeFailure> in
      guard identity.identifies(snapshot.identity) else { return .failure(.accountChanged) }
      verifiedIdentities[fingerprint] = identity
      return .success(snapshot)
    }
  }

  private func fetchUsage(_ grant: ClaudeGrant) async -> Result<UsageSnapshot, ClaudeFailure> {
    await fetch(
      "https://api.anthropic.com/api/oauth/usage", bearer: grant.accessToken,
      headers: ["anthropic-beta": "oauth-2025-04-20", "Accept": "application/json"]
    )
    .flatMap { data in decode(ClaudeUsageResponse.self, from: data) }
    .flatMap { payload in payload.snapshot(observedAt: Date()) }
  }

  private func fetch(
    _ address: String, bearer: String, headers: [String: String]
  ) async -> Result<Data, ClaudeFailure> {
    guard let url: URL = URL(string: address) else { return .failure(.invalidResponse) }
    var request: URLRequest = URLRequest(url: url)
    request.cachePolicy = .reloadIgnoringLocalCacheData
    request.setValue("Bearer \(bearer)", forHTTPHeaderField: "Authorization")
    for (name, value) in headers { request.setValue(value, forHTTPHeaderField: name) }
    do {
      let (data, response): (Data, URLResponse) = try await session.data(for: request)
      guard let response: HTTPURLResponse = response as? HTTPURLResponse else {
        return .failure(.invalidResponse)
      }
      guard response.statusCode == 200 else { return .failure(.http(response.statusCode)) }
      return Task.isCancelled ? .failure(.cancelled) : .success(data)
    } catch is CancellationError {
      return .failure(.cancelled)
    } catch let error as URLError where error.code == .cancelled {
      return .failure(.cancelled)
    } catch {
      return .failure(.transport(error))
    }
  }

  private func decode<Value: Decodable>(
    _ type: Value.Type, from data: Data
  ) -> Result<Value, ClaudeFailure> {
    do {
      return .success(try JSONDecoder().decode(type, from: data))
    } catch {
      return .failure(.invalidResponse)
    }
  }

  // MARK: - Selection

  /// Derives the selected account from the shared login and records it in the journal.
  private func observeSelection(accounts: [RelayAccount]) async -> Result<
    ClaudeObservedSelection, ClaudeFailure
  > {
    await defaultStore().read().flatMap { current in
      readJournal().flatMap { journal -> Result<ClaudeObservedSelection, ClaudeFailure> in
        guard journal.pending == nil else { return .failure(.unfinishedSelection) }
        let selected: UUID? = current.flatMap { snapshot in
          accounts.first { account in
            account.provider == .claude && account.identity.identifies(snapshot.identity)
          }?.id
        }
        guard journal.selectedAccountID != selected else {
          return .success(
            ClaudeObservedSelection(selected: selected, shared: current, journal: journal))
        }
        var updated: ClaudeSelectionJournal = journal
        if let previous: UUID = journal.selectedAccountID {
          updated.unavailableAccountIDs.insert(previous)
        }
        updated.selectedAccountID = selected
        if let selected { updated.unavailableAccountIDs.remove(selected) }
        return updated.write(to: paths.claudeSelectionFile).map { _ in
          ClaudeObservedSelection(selected: selected, shared: current, journal: updated)
        }
      }
    }
  }

  private func selectAccount(
    _ account: RelayAccount, accounts: [RelayAccount], mode: ClaudeSelectionMode
  ) async -> Result<AccountSelection, ClaudeFailure> {
    if case .failure(let error) = await recoverSelection(operationID: account.id) {
      return .failure(error)
    }
    let journal: ClaudeSelectionJournal
    switch readJournal() {
    case .success(let value): journal = value
    case .failure(let error): return .failure(error)
    }
    let shared: ClaudeNativeStore = defaultStore()
    // The lock precedes the read, so no native refresh rotates the shared grant after it.
    return await refreshing(
      [shared.directory, privateStore(account.id).directory], operationID: account.id
    ) { lease -> Result<AccountSelection, ClaudeFailure> in
      let before: ClaudeCredentialSnapshot?
      switch await shared.read() {
      case .success(let value): before = value
      case .failure(let error): return .failure(error)
      }
      switch (mode, before) {
      case (.select, .some(let current)) where current.identity.identifies(account.identity):
        return updateJournal { journal in
          journal.selectedAccountID = account.id
          journal.unavailableAccountIDs.remove(account.id)
        }.map { _ in AccountSelection(identity: current.identity, preservedAccount: nil) }
      case (.refreshSelected, let current)
      where current?.identity.identifies(account.identity) != true:
        return .success(AccountSelection(identity: account.identity, preservedAccount: nil))
      default: break
      }
      guard !journal.unavailableAccountIDs.contains(account.id) else {
        return .failure(.accountUnavailable)
      }
      let outgoing: RelayAccount?
      switch outgoingAccount(before, accounts: accounts, journal: journal) {
      case .success(let value): outgoing = value
      case .failure(let error): return .failure(error)
      }
      guard let outgoing, outgoing.id != account.id else {
        return await selectLeased(
          account, accounts: accounts, outgoing: outgoing, before: before, lease: lease)
      }
      return await refreshing([privateStore(outgoing.id).directory], operationID: account.id) {
        _ in
        await selectLeased(
          account, accounts: accounts, outgoing: outgoing, before: before, lease: lease)
      }
    }
  }

  /// The account whose grant the shared store holds before the switch.
  private func outgoingAccount(
    _ before: ClaudeCredentialSnapshot?, accounts: [RelayAccount],
    journal: ClaudeSelectionJournal
  ) -> Result<RelayAccount?, ClaudeFailure> {
    guard let before else { return .success(nil) }
    if let known: RelayAccount = accounts.first(where: { candidate in
      candidate.provider == .claude && candidate.identity.identifies(before.identity)
    }) {
      return .success(known)
    }
    if let record: ClaudeStoredAccount = journal.preservedAccounts.first(where: { record in
      record.identifies(before.identity)
    }) {
      return record.account().map(Optional.some).mapError(ClaudeFailure.invalidIdentity)
    }
    return .success(
      RelayAccount(id: UUID(), provider: .claude, identity: before.identity, sessionPolicy: .manual)
    )
  }

  private func selectLeased(
    _ account: RelayAccount, accounts: [RelayAccount], outgoing: RelayAccount?,
    before: ClaudeCredentialSnapshot?, lease: ClaudeRefreshLease
  ) async -> Result<AccountSelection, ClaudeFailure> {
    let shared: ClaudeNativeStore = defaultStore()
    let incoming: ClaudeCredentialSnapshot
    switch await privateStore(account.id).read() {
    case .success(.some(let value)): incoming = value
    case .success(.none): return .failure(.signInRequired)
    case .failure(let error): return .failure(error)
    }
    guard incoming.identity.identifies(account.identity) else { return .failure(.accountChanged) }
    guard incoming.grant.refreshToken != nil, !incoming.grant.scopes.isEmpty else {
      return .failure(.signInRequired)
    }
    if case .failure(let error) = executable() { return .failure(error) }
    if case .failure(let error) = prepareDirectories(store: shared) { return .failure(error) }
    guard !Task.isCancelled else { return .failure(.cancelled) }
    var pending: ClaudePendingSelection = ClaudePendingSelection(
      incoming: ClaudeStoredAccount(account), outgoing: outgoing.map(ClaudeStoredAccount.init),
      phase: .prepared, child: nil
    )
    let prepared: Result<Void, ClaudeFailure> = updateJournal { journal in
      journal.pending = pending
      if let outgoing {
        journal.preservedAccounts.removeAll { record in record.id == outgoing.id }
        journal.preservedAccounts.append(ClaudeStoredAccount(outgoing))
      }
      journal.preservedAccounts.removeAll { record in record.id == account.id }
      journal.preservedAccounts.append(ClaudeStoredAccount(account))
    }
    if case .failure(let error) = prepared { return .failure(error) }
    let parked: Result<Void, ClaudeFailure> =
      if let outgoing, let before, outgoing.id != account.id {
        await park(before, in: privateStore(outgoing.id), operationID: account.id)
      } else {
        .success(())
      }
    pending.phase = .importing
    let imported: Result<Void, ClaudeFailure> = await parked.bind { _ in
      await importSelection(pending, grant: incoming.grant, lease: lease)
    }
    let reconciled: Result<ClaudeCredentialSnapshot?, ClaudeFailure> = await readJournal().bind {
      recorded -> Result<ClaudeCredentialSnapshot?, ClaudeFailure> in
      guard let pending: ClaudePendingSelection = recorded.pending else {
        return .failure(.unfinishedSelection)
      }
      return await reconcileSelection(pending)
    }
    switch (imported, reconciled) {
    case (.failure(let operation), .failure(let recovery)):
      return .failure(.cleanup(operation: operation, release: recovery))
    case (.failure(let error), .success): return .failure(error)
    case (.success, .failure(let error)): return .failure(error)
    case (.success, .success(let snapshot)):
      guard let snapshot, snapshot.identity.identifies(account.identity) else {
        return .failure(.accountChanged)
      }
      let preserved: RelayAccount? = outgoing.flatMap { value in
        accounts.contains(where: { known in known.id == value.id }) ? nil : value
      }
      return .success(AccountSelection(identity: snapshot.identity, preservedAccount: preserved))
    }
  }

  private func importSelection(
    _ pending: ClaudePendingSelection, grant: ClaudeGrant, lease: ClaudeRefreshLease
  ) async -> Result<Void, ClaudeFailure> {
    if case .failure(let error) = await lease.status() { return .failure(error) }
    guard !Task.isCancelled else { return .failure(.cancelled) }
    if case .failure(let error) = updateJournal({ journal in journal.pending = pending }) {
      return .failure(error)
    }
    // The native login rotates the refresh token and writes the account under Claude Code's
    // own storage-write lock, which stays free while the child runs.
    return await authenticate(store: defaultStore(), email: nil, grant: grant, selection: pending)
  }

  private func park(
    _ snapshot: ClaudeCredentialSnapshot, in store: ClaudeNativeStore, operationID: UUID
  ) async -> Result<Void, ClaudeFailure> {
    await writing(store, operationID: operationID) { await store.park(snapshot) }
  }

  /// Settles a switch against the shared login as it stands and clears the pending record.
  private func reconcileSelection(
    _ pending: ClaudePendingSelection
  ) async -> Result<ClaudeCredentialSnapshot?, ClaudeFailure> {
    let current: ClaudeCredentialSnapshot?
    switch await defaultStore().read() {
    case .success(let value): current = value
    case .failure(let error): return .failure(error)
    }
    var journal: ClaudeSelectionJournal
    switch readJournal() {
    case .success(let value): journal = value
    case .failure(let error): return .failure(error)
    }
    let selected: UUID? = current.flatMap { snapshot in
      journal.preservedAccounts.first { record in record.identifies(snapshot.identity) }?.id
    }
    journal.selectedAccountID = selected
    journal.pending = nil
    if let selected { journal.unavailableAccountIDs.remove(selected) }
    if pending.phase == .importing, selected != pending.incoming.id {
      // A failed finalizer may have rotated the dormant refresh token before
      // persisting its replacement. That old grant is no longer a credential owner.
      journal.unavailableAccountIDs.insert(pending.incoming.id)
    }
    if let outgoing: ClaudeStoredAccount = pending.outgoing,
      outgoing.id != selected, outgoing.id != pending.incoming.id
    {
      switch await privateStore(outgoing.id).read() {
      case .success(.some(let saved)) where outgoing.identifies(saved.identity):
        journal.unavailableAccountIDs.remove(outgoing.id)
      case .success:
        journal.unavailableAccountIDs.insert(outgoing.id)
      case .failure(let error): return .failure(error)
      }
    }
    return journal.write(to: paths.claudeSelectionFile).map { _ in current }
  }

  private func recoverSelection(operationID: UUID? = nil) async -> Result<Void, ClaudeFailure> {
    let pending: ClaudePendingSelection
    switch readJournal() {
    case .success(let journal):
      guard let value: ClaudePendingSelection = journal.pending else { return .success(()) }
      pending = value
    case .failure(let error): return .failure(error)
    }
    if let child: ClaudeChildIdentity = pending.child {
      switch child.isRunning() {
      case .success(true): return .failure(.unfinishedSelection)
      case .success(false): break
      case .failure(let error): return .failure(error)
      }
    }
    let directories: [URL] =
      [defaultStore().directory, privateStore(pending.incoming.id).directory]
      + (pending.outgoing.map { value in [privateStore(value.id).directory] } ?? [])
    return await refreshing(directories, operationID: operationID ?? maintenanceID) { _ in
      await reconcileSelection(pending).map { _ in () }
    }
  }

  // MARK: - Sign-out

  private func signOutAccount(_ account: RelayAccount, accounts: [RelayAccount]) async -> Result<
    Void, ClaudeFailure
  > {
    if case .failure(let error) = await recoverSelection(operationID: account.id) {
      return .failure(error)
    }
    let shared: ClaudeNativeStore = defaultStore()
    let saved: ClaudeNativeStore = privateStore(account.id)
    return await refreshing([shared.directory, saved.directory], operationID: account.id) { _ in
      await signOutLeased(account, accounts: accounts, shared: shared, saved: saved)
    }
  }

  private func signOutLeased(
    _ account: RelayAccount, accounts: [RelayAccount], shared: ClaudeNativeStore,
    saved: ClaudeNativeStore
  ) async -> Result<Void, ClaudeFailure> {
    let current: ClaudeCredentialSnapshot?
    switch await shared.read() {
    case .success(let value): current = value
    case .failure(let error): return .failure(error)
    }
    if let current, current.identity.identifies(account.identity) {
      // The shared grant moves to the private store, where the native logout revokes it.
      if case .failure(let error) = await park(current, in: saved, operationID: account.id) {
        return .failure(error)
      }
      let erased: Result<Void, ClaudeFailure> = await writing(shared, operationID: account.id) {
        await shared.eraseSavedSignIn(matching: current.grant.value)
      }
      if case .failure(let error) = erased {
        guard case .accountChanged = error.cause else { return .failure(error) }
        // The parked copy stays valid; the shared login now names another account.
        if case .failure(let recovery) = await observeSelection(accounts: accounts) {
          return .failure(.cleanup(operation: error, release: recovery))
        }
        return .failure(
          error.releasing(
            updateJournal { journal in journal.unavailableAccountIDs.remove(account.id) }))
      }
      if case .failure(let error) = await logout(store: saved) { return .failure(error) }
    } else {
      switch await saved.read() {
      case .success(.some(let snapshot)):
        guard snapshot.identity.identifies(account.identity) else {
          return .failure(.accountChanged)
        }
        if case .failure(let error) = await logout(store: saved) { return .failure(error) }
      case .success(.none): break
      case .failure(let error): return .failure(error)
      }
    }
    return updateJournal { journal in
      if journal.selectedAccountID == account.id { journal.selectedAccountID = nil }
      journal.unavailableAccountIDs.insert(account.id)
    }
  }

  private func discardPrivateStore(id: UUID) async -> Result<Void, ClaudeFailure> {
    let store: ClaudeNativeStore = privateStore(id)
    return await refreshing([store.directory], operationID: id) { _ in
      await writing(store, operationID: id) { await store.deletePrivateCredentials() }
    }
    .flatMap { _ -> Result<Void, ClaudeFailure> in
      do {
        try FileManager.default.removeItem(at: store.directory)
        return .success(())
      } catch {
        return .failure(.filesystem(error))
      }
    }
    .flatMap { _ in
      updateJournal { journal in
        journal.preservedAccounts.removeAll { record in record.id == id }
        journal.unavailableAccountIDs.remove(id)
      }
    }
  }

  // MARK: - Native client

  private func headless(
    store: ClaudeNativeStore, action: ClaudeHeadlessAction, grant: ClaudeGrant? = nil
  ) async -> Result<Void, ClaudeFailure> {
    await prepareDirectories(store: store).flatMap { _ in executable() }.bind {
      binary -> Result<Void, ClaudeFailure> in
      let sessionID: UUID = UUID()
      var childEnvironment: [String: String] = processEnvironment(store: store)
      if let grant {
        childEnvironment["CLAUDE_CODE_OAUTH_TOKEN"] = grant.accessToken
        childEnvironment["CLAUDE_CODE_SUBSCRIPTION_TYPE"] = grant.plan
        childEnvironment["CLAUDE_CODE_RATE_LIMIT_TIER"] = grant.value["rateLimitTier"]?.stringValue
      }
      let invocation: ProcessInvocation = ProcessInvocation(
        executable: binary,
        arguments: [
          "-p", "--input-format", "stream-json", "--output-format", "stream-json", "--verbose",
          "--tools", "", "--setting-sources=", "--strict-mcp-config", "--disable-slash-commands",
          "--no-session-persistence", "--max-turns", "1", "--session-id", sessionID.uuidString,
          "--system-prompt", "Reply with one word.",
          "--settings", "{\"disableAllHooks\":true,\"autoMemoryEnabled\":false}",
        ],
        environment: childEnvironment,
        workingDirectory: paths.workingDirectory
      )
      return await ClaudeHeadless.run(invocation: invocation, action: action, sessionID: sessionID)
    }
  }

  /// Runs the native login: a browser flow into the store, or a refresh-token exchange when a
  /// grant is given, which Claude Code finalizes with its own rotation and account metadata.
  private func authenticate(
    store: ClaudeNativeStore, email: String?, grant: ClaudeGrant?,
    selection: ClaudePendingSelection?
  ) async -> Result<Void, ClaudeFailure> {
    let launched: Result<NativeProcess, ClaudeFailure> = await prepareDirectories(store: store)
      .flatMap { _ in executable() }
      .bind { binary -> Result<NativeProcess, ClaudeFailure> in
        var arguments: [String] = ["auth", "login", "--claudeai"]
        if let email { arguments.append(contentsOf: ["--email", email]) }
        var childEnvironment: [String: String] = processEnvironment(store: store)
        if let grant {
          guard let refresh: String = grant.refreshToken, !refresh.isEmpty,
            !grant.scopes.isEmpty
          else { return .failure(.signInRequired) }
          childEnvironment["CLAUDE_CODE_OAUTH_REFRESH_TOKEN"] = refresh
          childEnvironment["CLAUDE_CODE_OAUTH_SCOPES"] = grant.scopes.joined(separator: " ")
          childEnvironment["CLAUDE_CODE_OAUTH_CLIENT_ID"] = grant.clientID
        }
        guard !Task.isCancelled else { return .failure(.cancelled) }
        let invocation: ProcessInvocation = ProcessInvocation(
          executable: binary, arguments: arguments, environment: childEnvironment,
          workingDirectory: paths.workingDirectory
        )
        return await NativeProcess.launch(invocation).mapError(ClaudeFailure.native)
      }
    let process: NativeProcess
    switch launched {
    case .success(let value): process = value
    case .failure(let error): return authenticationNotStarted(error, selection: selection)
    }
    if var selection {
      let recorded: Result<Void, ClaudeFailure> = ClaudeChildIdentity.read(
        processID: process.processIdentifier
      ).flatMap { child -> Result<Void, ClaudeFailure> in
        selection.child = child
        selection.phase = .importing
        return updateJournal { journal in journal.pending = selection }
      }
      if case .failure(let error) = recorded {
        await process.cancel()
        return .failure(error)
      }
    }
    return await withTaskCancellationHandler {
      let output: Result<ProcessOutput, ProcessFailure> = await process.waitForExit()
      if Task.isCancelled { return .failure(.cancelled) }
      return output.mapError(ClaudeFailure.native).flatMap { output in
        output.exitCode == 0 ? .success(()) : .failure(.authenticationFailed(output.exitCode))
      }
    } onCancel: {
      Task { await process.cancel() }
    }
  }

  /// Records that the import never started, so recovery keeps the incoming grant available.
  private func authenticationNotStarted(
    _ failure: ClaudeFailure, selection: ClaudePendingSelection?
  ) -> Result<Void, ClaudeFailure> {
    guard var selection else { return .failure(failure) }
    selection.phase = .prepared
    selection.child = nil
    return .failure(failure.releasing(updateJournal { journal in journal.pending = selection }))
  }

  private func logout(store: ClaudeNativeStore) async -> Result<Void, ClaudeFailure> {
    guard !Task.isCancelled else { return .failure(.cancelled) }
    return await prepareDirectories(store: store).flatMap { _ in executable() }.bind {
      binary -> Result<Void, ClaudeFailure> in
      let invocation: ProcessInvocation = ProcessInvocation(
        executable: binary, arguments: ["auth", "logout"],
        environment: processEnvironment(store: store),
        workingDirectory: paths.workingDirectory
      )
      let process: NativeProcess
      switch await NativeProcess.launch(invocation) {
      case .success(let value): process = value
      case .failure(let error): return .failure(.native(error))
      }
      return await withTaskCancellationHandler {
        await process.closeInput()
        let result: Result<ProcessOutput, ProcessFailure> = await process.waitForExit()
        if Task.isCancelled { return .failure(.cancelled) }
        return result.mapError(ClaudeFailure.native).flatMap { output in
          output.exitCode == 0 ? .success(()) : .failure(.native(.exit(output.exitCode)))
        }
      } onCancel: {
        Task { await process.cancel() }
      }
    }
  }

  private func executable() -> Result<URL, ClaudeFailure> {
    let pathEntries: [URL] = (environment["PATH"] ?? "").split(separator: ":").map { path in
      URL(fileURLWithPath: String(path), isDirectory: true).appending(path: "claude")
    }
    let native: URL = userHome().appending(path: ".local/bin/claude")
    guard
      let executable: URL = (pathEntries + [native]).first(where: { url in
        FileManager.default.isExecutableFile(atPath: url.path)
      })
    else { return .failure(.executableMissing) }
    return .success(executable)
  }

  /// The child's environment: no credential or provider override, the private store's
  /// directory when the store is not the shared one, and auto memory off.
  private func processEnvironment(store: ClaudeNativeStore) -> [String: String] {
    var result: [String: String] = environment
    let overrides: [String] = [
      "ANTHROPIC_API_KEY", "ANTHROPIC_AUTH_TOKEN", "ANTHROPIC_BASE_URL", "ANTHROPIC_MODEL",
      "CLAUDE_CODE_OAUTH_TOKEN", "CLAUDE_CODE_OAUTH_TOKEN_FILE_DESCRIPTOR", "CCR_OAUTH_TOKEN_FILE",
      "CLAUDE_CODE_OAUTH_REFRESH_TOKEN", "CLAUDE_CODE_OAUTH_SCOPES", "CLAUDE_CODE_OAUTH_CLIENT_ID",
      "CLAUDE_CODE_USE_BEDROCK", "CLAUDE_CODE_USE_VERTEX", "CLAUDE_CODE_USE_FOUNDRY",
      "CLAUDE_CODE_SIMPLE",
      "CLAUDE_CODE_REMOTE_SESSION_ID", "CLAUDE_CODE_REMOTE", "ANTHROPIC_UNIX_SOCKET",
      "CLAUDE_CODE_SUBSCRIPTION_TYPE", "CLAUDE_CODE_RATE_LIMIT_TIER",
      "CLAUDE_CODE_API_KEY_FILE_DESCRIPTOR",
    ]
    for name in overrides { result.removeValue(forKey: name) }
    let shared: ClaudeNativeStore = defaultStore()
    if store.service != shared.service || store.configuration != shared.configuration {
      result["CLAUDE_CONFIG_DIR"] = store.directory.path.precomposedStringWithCanonicalMapping
      result.removeValue(forKey: "CLAUDE_SECURESTORAGE_CONFIG_DIR")
    }
    result["CLAUDE_CODE_DISABLE_AUTO_MEMORY"] = "1"
    return result
  }

  // MARK: - Stores and files

  private func privateStore(_ id: UUID) -> ClaudeNativeStore {
    let directory: URL = paths.claudeDirectory(id)
    return ClaudeNativeStore(
      directory: directory, configuration: directory.appending(path: ".claude.json"),
      defaultNamespace: false, username: username
    )
  }

  /// The store Claude Code reads with the app's environment: `CLAUDE_SECURESTORAGE_CONFIG_DIR`
  /// names the credential directory when set (empty meaning `~/.claude`), otherwise
  /// `CLAUDE_CONFIG_DIR`, which also moves `.claude.json` inside it; the Keychain namespace
  /// is the default one only when the credential directory is the default.
  private func defaultStore() -> ClaudeNativeStore {
    let configured: String? = environment["CLAUDE_CONFIG_DIR"].flatMap { value in
      value.isEmpty ? nil : value
    }
    let secure: String? = environment["CLAUDE_SECURESTORAGE_CONFIG_DIR"]
    let directory: URL =
      if let secure, !secure.isEmpty {
        URL(fileURLWithPath: secure.precomposedStringWithCanonicalMapping, isDirectory: true)
      } else if secure != nil {
        userHome().appending(path: ".claude", directoryHint: .isDirectory)
      } else {
        paths.defaultClaudeDirectory
      }
    let configuration: URL =
      configured == nil
      ? userHome().appending(path: ".claude.json")
      : paths.defaultClaudeDirectory.appending(path: ".claude.json")
    let defaultNamespace: Bool = if let secure { secure.isEmpty } else { configured == nil }
    return ClaudeNativeStore(
      directory: directory, configuration: configuration,
      defaultNamespace: defaultNamespace, username: username
    )
  }

  private func userHome() -> URL {
    if let home: String = environment["HOME"], !home.isEmpty {
      return URL(fileURLWithPath: home, isDirectory: true)
    }
    return FileManager.default.homeDirectoryForCurrentUser
  }

  private func prepareDirectories(store: ClaudeNativeStore) -> Result<Void, ClaudeFailure> {
    do {
      try FileManager.default.createDirectory(
        at: paths.workingDirectory, withIntermediateDirectories: true)
      try FileManager.default.createDirectory(
        at: store.directory, withIntermediateDirectories: true)
      return .success(())
    } catch {
      return .failure(.filesystem(error))
    }
  }

  private func readJournal() -> Result<ClaudeSelectionJournal, ClaudeFailure> {
    ClaudeSelectionJournal.read(at: paths.claudeSelectionFile)
  }

  private func updateJournal(
    _ change: (inout ClaudeSelectionJournal) -> Void
  ) -> Result<Void, ClaudeFailure> {
    readJournal().flatMap { journal in
      var updated: ClaudeSelectionJournal = journal
      change(&updated)
      return updated.write(to: paths.claudeSelectionFile)
    }
  }
}
