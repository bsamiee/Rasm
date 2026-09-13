import CryptoKit
import Foundation

private nonisolated struct ClaudeRunningOperation: Sendable {
  let id: UUID
  let cancel: @Sendable () -> Void
  let wait: @Sendable () async -> Void
}

private nonisolated struct ClaudeCurrentSelection: Sendable {
  let selected: UUID?
  let shared: ClaudeCredentials?
  let state: ClaudeSelectionState
}

private nonisolated struct ClaudeStoredCredentials: Sendable {
  let store: ClaudeCredentialStore
  let credentials: ClaudeCredentials
}

actor ClaudeClient {
  private static let excludedEnvironmentVariables: Set<String> = [
    "ANTHROPIC_API_KEY", "ANTHROPIC_AUTH_TOKEN", "ANTHROPIC_BASE_URL", "ANTHROPIC_MODEL",
    "CLAUDE_CODE_OAUTH_TOKEN", "CLAUDE_CODE_OAUTH_TOKEN_FILE_DESCRIPTOR", "CCR_OAUTH_TOKEN_FILE",
    "CLAUDE_CODE_OAUTH_REFRESH_TOKEN", "CLAUDE_CODE_OAUTH_SCOPES", "CLAUDE_CODE_OAUTH_CLIENT_ID",
    "CLAUDE_CODE_USE_BEDROCK", "CLAUDE_CODE_USE_VERTEX", "CLAUDE_CODE_USE_FOUNDRY",
    "CLAUDE_CODE_SIMPLE", "CLAUDE_CODE_REMOTE_SESSION_ID", "CLAUDE_CODE_REMOTE",
    "ANTHROPIC_UNIX_SOCKET", "CLAUDE_CODE_SUBSCRIPTION_TYPE", "CLAUDE_CODE_RATE_LIMIT_TIER",
    "CLAUDE_CODE_API_KEY_FILE_DESCRIPTOR",
  ]

  private let paths: FileLocations
  private let environment: [String: String]
  private let username: String
  private let allAccountsID: UUID = UUID()
  private let session: URLSession
  private var operations: [UUID: ClaudeRunningOperation] = [:]
  private var exclusiveOperation: UUID?
  private var verifiedIdentities: [SHA256Digest: AccountIdentity] = [:]

  init(paths: FileLocations, environment: [String: String]) {
    self.paths = paths
    self.environment = environment
    let candidate: String = environment["USER"] ?? NSUserName()
    username = candidate.contains(/^[a-zA-Z0-9._-]+$/) ? candidate : "claude-code-user"
    let configuration: URLSessionConfiguration = .ephemeral
    configuration.timeoutIntervalForRequest = 15
    configuration.timeoutIntervalForResource = 30
    configuration.requestCachePolicy = .reloadIgnoringLocalCacheData
    session = URLSession(configuration: configuration)
  }

  func connect(id: UUID) async -> Result<AccountIdentity, ClaudeFailure> {
    await run(forAccount: id) {
      await self.connectAccount(id: id, expected: nil, policy: .manual)
    }
  }

  func reconnect(account: Account) async -> Result<AccountIdentity, ClaudeFailure> {
    await run(forAccount: account.id, exclusive: true) {
      await self.reconnectAccount(account).bind { identity in
        let refreshed: Account = Account(
          id: account.id, provider: .claude, identity: identity,
          sessionPolicy: account.sessionPolicy
        )
        return await self.selectAccount(refreshed, accounts: [refreshed], onlyIfSelected: true)
          .map { selection in selection.identity }
      }
    }
  }

  func usage(for account: Account, accounts: [Account]) async -> Result<
    UsageSnapshot, ClaudeFailure
  > {
    await run(forAccount: account.id) {
      await self.withCredentials(for: account, accounts: accounts) { stored in
        await self.fetchUsage(stored.credentials.token)
      }
      .map { _, snapshot in snapshot }
    }
  }

  func startSession(
    for account: Account, accounts: [Account]
  ) async -> Result<UsageSnapshot, ClaudeFailure> {
    await run(forAccount: account.id) {
      let usage: Result<(ClaudeStoredCredentials, UsageSnapshot), ClaudeFailure> =
        await self.withCredentials(for: account, accounts: accounts) { stored in
          await self.fetchUsage(stored.credentials.token)
        }
      return await usage.bind { stored, snapshot -> Result<UsageSnapshot, ClaudeFailure> in
        switch snapshot.sessionState(at: Date()) {
        case .running: return .success(snapshot)
        case .unknown: return .failure(.sessionUnknown)
        case .idle:
          return await self.runSession(
            store: stored.store, action: .greeting, token: stored.credentials.token
          ).bind { _ in
            await self.fetchUsage(stored.credentials.token)
              .mapError(ClaudeFailure.sessionConfirmationPending)
          }
        }
      }
    }
  }

  func select(_ account: Account, accounts: [Account]) async -> Result<
    AccountSelection, ClaudeFailure
  > {
    await run(forAccount: account.id, exclusive: true) {
      await self.selectAccount(account, accounts: accounts, onlyIfSelected: false)
    }
  }

  func signOut(_ account: Account, accounts: [Account]) async -> Result<
    Void, ClaudeFailure
  > {
    await run(forAccount: account.id, exclusive: true) {
      await self.signOutAccount(account, accounts: accounts)
    }
  }

  func remove(_ account: Account, accounts: [Account]) async -> Result<
    Void, ClaudeFailure
  > {
    await run(forAccount: account.id, exclusive: true) {
      await self.signOutAccount(account, accounts: accounts).bind { _ in
        await self.discardPrivateStore(id: account.id)
      }
    }
  }

  func discardConnection(id: UUID) async -> Result<Void, ClaudeFailure> {
    await cancel(accountID: id)
    return await run(forAccount: id, exclusive: true) {
      await self.discardPrivateStore(id: id)
    }
  }

  func selectedAccount(in accounts: [Account]) async -> Result<UUID?, ClaudeFailure> {
    await accounts.contains(where: { account in account.provider == .claude })
      ? run(
        forAccount: allAccountsID,
        operation: {
          await self.currentSelection(accounts: accounts).map { current in current.selected }
        })
      : .success(nil)
  }

  func recoverAccounts() async -> Result<[Account], ClaudeFailure> {
    await run(forAccount: allAccountsID, exclusive: true) {
      await self.recoverSelection(operationID: self.allAccountsID).bind { _ in
        await self.selectionState()
      }.flatMap { state in
        traverse(state.preservedAccounts) { record in
          record.account().mapError(ClaudeAccountErrors.init)
        }
        .mapError(ClaudeFailure.invalidSavedAccounts)
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

  private func run<Value: Sendable>(
    forAccount accountID: UUID,
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
      return await Task.isCancelled ? .failure(.cancelled) : operation()
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

  private func cancelHandler(for accountID: UUID) -> @Sendable () async -> Void {
    { [weak self] in await self?.operations[accountID]?.cancel() }
  }

  private func withRefreshLock<Value: Sendable>(
    _ directories: [URL], operationID: UUID,
    _ body: (ClaudeLock) async -> Result<Value, ClaudeFailure>
  ) async -> Result<Value, ClaudeFailure> {
    await ClaudeLock.oauthRefresh(
      directories: directories, onCompromised: cancelHandler(for: operationID)
    )
    .bind { lock in await lock.release(returning: await body(lock)) }
  }

  private func withStorageWriteLock<Value: Sendable>(
    _ store: ClaudeCredentialStore, operationID: UUID,
    _ body: () async -> Result<Value, ClaudeFailure>
  ) async -> Result<Value, ClaudeFailure> {
    await ClaudeLock.storageWrite(
      directory: store.directory, onCompromised: cancelHandler(for: operationID)
    )
    .bind { lock in await lock.release(returning: await body()) }
  }

  private func connectAccount(
    id: UUID, expected: AccountIdentity?, policy: SessionPolicy
  ) async -> Result<AccountIdentity, ClaudeFailure> {
    let store: ClaudeCredentialStore = privateStore(id)
    return await authenticate(store: store, email: expected?.email, token: nil, selection: nil)
      .bind { _ in await store.read() }
      .flatMap { credentials in matched(credentials, to: expected) }
      .flatMap { credentials in
        updateSelectionState { state in
          state.preserve(
            ClaudeStoredAccount(
              Account(
                id: id, provider: .claude, identity: credentials.identity, sessionPolicy: policy)))
          state.unavailableAccountIDs.remove(id)
        }.map { _ in credentials.identity }
      }
  }

  private func reconnectAccount(_ account: Account) async -> Result<
    AccountIdentity, ClaudeFailure
  > {
    let store: ClaudeCredentialStore = privateStore(account.id)
    return await withRefreshLock([store.directory], operationID: account.id) { lock in
      await store.read().bind { previous -> Result<AccountIdentity, ClaudeFailure> in
        let signedIn: Result<AccountIdentity, ClaudeFailure> = await connectAccount(
          id: account.id, expected: account.identity, policy: account.sessionPolicy
        )
        guard case .failure(let error) = signedIn else { return signedIn }
        return .failure(
          error.releasing(
            await lock.status().bind { _ in
              await restore(previous, to: store, operationID: account.id)
            }))
      }
    }
  }

  private func restore(
    _ previous: ClaudeCredentials?, to store: ClaudeCredentialStore, operationID: UUID
  ) async -> Result<Void, ClaudeFailure> {
    await Task {
      if let previous {
        return await self.write(previous, to: store, operationID: operationID)
      }
      return await self.withStorageWriteLock(store, operationID: operationID) {
        await store.deleteCredentials()
      }
    }.value
  }

  private func withCredentials<Value: Sendable>(
    for account: Account, accounts: [Account],
    _ request: (ClaudeStoredCredentials) async -> Result<Value, ClaudeFailure>
  ) async -> Result<(ClaudeStoredCredentials, Value), ClaudeFailure> {
    func send(_ stored: ClaudeStoredCredentials) async -> Result<
      (ClaudeStoredCredentials, Value), ClaudeFailure
    > {
      await verify(stored.credentials).bind { _ in
        await request(stored).map { value in (stored, value) }
      }
    }
    return await storedCredentials(for: account, accounts: accounts).bind { stored in
      let sent: Result<(ClaudeStoredCredentials, Value), ClaudeFailure> = await send(stored)
      guard case .failure(let error) = sent, error.unauthorized else { return sent }
      return await renew(stored, for: account).bind(send).mapError { retried in
        retried.unauthorized ? .signInRequired : retried
      }
    }
  }

  private func renew(
    _ stored: ClaudeStoredCredentials, for account: Account
  ) async -> Result<ClaudeStoredCredentials, ClaudeFailure> {
    await withRefreshLock([stored.store.directory], operationID: account.id) { _ in
      await stored.store.read()
    }
    .flatMap { credentials in self.matched(credentials, to: account.identity) }
    .bind { current in
      await current.token.fingerprint == stored.credentials.token.fingerprint
        ? refreshOAuthToken(current, in: stored.store, for: account.identity) : .success(current)
    }
    .map { credentials in ClaudeStoredCredentials(store: stored.store, credentials: credentials) }
  }

  private func refreshOAuthToken(
    _ credentials: ClaudeCredentials, in store: ClaudeCredentialStore, for identity: AccountIdentity
  ) async -> Result<ClaudeCredentials, ClaudeFailure> {
    await
      (credentials.token.refreshToken == nil
      ? .failure(.signInRequired) : runSession(store: store, action: .refreshCredentials))
      .bind { _ in await store.read() }
      .flatMap { credentials in matched(credentials, to: identity) }
  }

  private func storedCredentials(
    for account: Account, accounts: [Account]
  ) async -> Result<ClaudeStoredCredentials, ClaudeFailure> {
    await
      (account.provider == .claude
      ? currentSelection(accounts: accounts) : .failure(.invalidCredentials))
      .bind { current -> Result<ClaudeStoredCredentials, ClaudeFailure> in
        let owner: ClaudeCredentialStore
        let stored: Result<ClaudeCredentials?, ClaudeFailure>
        if current.selected == account.id {
          owner = defaultStore()
          stored = .success(current.shared)
        } else if current.state.unavailableAccountIDs.contains(account.id) {
          return .failure(.signInRequired)
        } else {
          owner = privateStore(account.id)
          stored = await owner.read()
        }
        return await stored.flatMap { credentials in matched(credentials, to: account.identity) }
          .bind { credentials in
            await credentials.token.needsRefresh(at: Date())
              ? refreshOAuthToken(credentials, in: owner, for: account.identity)
              : .success(credentials)
          }
          .flatMap { current in
            Task.isCancelled
              ? .failure(.cancelled)
              : .success(ClaudeStoredCredentials(store: owner, credentials: current))
          }
      }
  }

  private func matched(
    _ credentials: ClaudeCredentials?, to identity: AccountIdentity?
  ) -> Result<ClaudeCredentials, ClaudeFailure> {
    switch credentials {
    case .some(let value)
    where identity.map({ expected in value.identity.isSameAccount(as: expected) }) != false:
      .success(value)
    case .some: .failure(.accountChanged)
    case .none: .failure(.signInRequired)
    }
  }

  private func verify(_ credentials: ClaudeCredentials) async -> Result<
    ClaudeCredentials, ClaudeFailure
  > {
    let fingerprint: SHA256Digest = credentials.token.fingerprint
    if let identity: AccountIdentity = verifiedIdentities[fingerprint] {
      return identity.isSameAccount(as: credentials.identity)
        ? .success(credentials) : .failure(.accountChanged)
    }
    return await fetch(
      "https://api.anthropic.com/api/oauth/profile", bearer: credentials.token.accessToken,
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
        plan: credentials.token.plan
      ).mapError(ClaudeFailure.invalidIdentity)
    }
    .flatMap { identity -> Result<ClaudeCredentials, ClaudeFailure> in
      guard identity.isSameAccount(as: credentials.identity) else {
        return .failure(.accountChanged)
      }
      verifiedIdentities[fingerprint] = identity
      return .success(credentials)
    }
  }

  private func fetchUsage(_ token: ClaudeOAuthToken) async -> Result<
    UsageSnapshot, ClaudeFailure
  > {
    await fetch(
      "https://api.anthropic.com/api/oauth/usage", bearer: token.accessToken,
      headers: ["anthropic-beta": "oauth-2025-04-20", "Accept": "application/json"]
    )
    .flatMap { data in decode(ClaudeUsageResponse.self, from: data) }
    .flatMap { usage in usage.snapshot(observedAt: Date()) }
  }

  private func fetch(
    _ address: String, bearer: String, headers: [String: String]
  ) async -> Result<Data, ClaudeFailure> {
    guard let url: URL = URL(string: address) else { return .failure(.invalidResponse) }
    var request: URLRequest = URLRequest(url: url)
    request.setValue("Bearer \(bearer)", forHTTPHeaderField: "Authorization")
    for (name, value) in headers { request.setValue(value, forHTTPHeaderField: name) }
    let response: Result<(Data, URLResponse), any Error> = await Result {
      try await session.data(for: request)
    }
    return response.mapError { error -> ClaudeFailure in
      switch error {
      case is CancellationError: .cancelled
      case let error as URLError where error.code == .cancelled: .cancelled
      default: .transport(error)
      }
    }
    .flatMap { data, response -> Result<Data, ClaudeFailure> in
      switch (response as? HTTPURLResponse)?.statusCode {
      case 200: Task.isCancelled ? .failure(.cancelled) : .success(data)
      case .some(let status): .failure(.http(status))
      case .none: .failure(.invalidResponse)
      }
    }
  }

  private func decode<Value: Decodable>(
    _ type: Value.Type, from data: Data
  ) -> Result<Value, ClaudeFailure> {
    Result { try JSONDecoder().decode(type, from: data) }.mapError { _ in .invalidResponse }
  }

  private func currentSelection(accounts: [Account]) async -> Result<
    ClaudeCurrentSelection, ClaudeFailure
  > {
    await defaultStore().read().flatMap { current in
      let selected: UUID? = current.flatMap { credentials in
        accounts.first { account in
          account.provider == .claude && account.identity.isSameAccount(as: credentials.identity)
        }?.id
      }
      return selectionState().flatMap { state -> Result<ClaudeCurrentSelection, ClaudeFailure> in
        guard state.pending == nil else { return .failure(.unfinishedSelection) }
        guard state.selectedAccountID != selected else {
          return .success(
            ClaudeCurrentSelection(selected: selected, shared: current, state: state))
        }
        var updated: ClaudeSelectionState = state
        if let previous: UUID = state.selectedAccountID {
          updated.unavailableAccountIDs.insert(previous)
        }
        updated.selectedAccountID = selected
        if let selected { updated.unavailableAccountIDs.remove(selected) }
        return updated.write(to: paths.claudeSelectionFile).map { _ in
          ClaudeCurrentSelection(selected: selected, shared: current, state: updated)
        }
      }
    }
  }

  private func selectAccount(
    _ account: Account, accounts: [Account], onlyIfSelected: Bool
  ) async -> Result<AccountSelection, ClaudeFailure> {
    await recoverSelection(operationID: account.id).flatMap { _ in selectionState() }.bind {
      state -> Result<AccountSelection, ClaudeFailure> in
      let shared: ClaudeCredentialStore = defaultStore()
      return await withRefreshLock(
        [shared.directory, privateStore(account.id).directory], operationID: account.id
      ) { lock -> Result<AccountSelection, ClaudeFailure> in
        await shared.read().bind { before in
          await select(
            account, accounts: accounts, onlyIfSelected: onlyIfSelected, state: state,
            before: before, lock: lock)
        }
      }
    }
  }

  private func select(
    _ account: Account, accounts: [Account], onlyIfSelected: Bool,
    state: ClaudeSelectionState, before: ClaudeCredentials?, lock: ClaudeLock
  ) async -> Result<AccountSelection, ClaudeFailure> {
    switch before {
    case .some(let current)
    where !onlyIfSelected && current.identity.isSameAccount(as: account.identity):
      return updateSelectionState { state in
        state.selectedAccountID = account.id
        state.unavailableAccountIDs.remove(account.id)
      }.map { _ in AccountSelection(identity: current.identity, preservedAccount: nil) }
    case _ where onlyIfSelected && before?.identity.isSameAccount(as: account.identity) != true:
      return .success(AccountSelection(identity: account.identity, preservedAccount: nil))
    default: break
    }
    return await
      (state.unavailableAccountIDs.contains(account.id)
      ? .failure(.signInRequired) : outgoingAccount(before, accounts: accounts, state: state))
      .bind { outgoing in
        guard let outgoing, outgoing.id != account.id else {
          return await switchSelection(
            account, accounts: accounts, outgoing: outgoing, before: before, lock: lock)
        }
        return await withRefreshLock([privateStore(outgoing.id).directory], operationID: account.id)
        {
          _ in
          await switchSelection(
            account, accounts: accounts, outgoing: outgoing, before: before, lock: lock)
        }
      }
  }

  private func outgoingAccount(
    _ before: ClaudeCredentials?, accounts: [Account], state: ClaudeSelectionState
  ) -> Result<Account?, ClaudeFailure> {
    before.map { before in
      knownAccount(for: before.identity, accounts: accounts, state: state).map(Optional.some)
    } ?? .success(nil)
  }

  private func knownAccount(
    for identity: AccountIdentity, accounts: [Account], state: ClaudeSelectionState
  ) -> Result<Account, ClaudeFailure> {
    accounts.first { candidate in
      candidate.provider == .claude && candidate.identity.isSameAccount(as: identity)
    }
    .map { known -> Result<Account, ClaudeFailure> in .success(known) }
      ?? state.preservedAccount(for: identity).map { record in
        record.account().mapError(ClaudeFailure.invalidIdentity)
      }
      ?? .success(
        Account(id: UUID(), provider: .claude, identity: identity, sessionPolicy: .manual))
  }

  private func switchSelection(
    _ account: Account, accounts: [Account], outgoing: Account?,
    before: ClaudeCredentials?, lock: ClaudeLock
  ) async -> Result<AccountSelection, ClaudeFailure> {
    await privateStore(account.id).read()
      .flatMap { credentials in matched(credentials, to: account.identity) }
      .bind { incoming -> Result<AccountSelection, ClaudeFailure> in
        guard incoming.token.refreshToken != nil, !incoming.token.scopes.isEmpty else {
          return .failure(.signInRequired)
        }
        var pending: ClaudePendingSelection = ClaudePendingSelection(
          incoming: ClaudeStoredAccount(account), outgoing: outgoing.map(ClaudeStoredAccount.init),
          phase: .prepared, child: nil
        )
        let prepared: Result<Void, ClaudeFailure> = prepareSelection(
          pending, account: account, outgoing: outgoing)
        pending.phase = .importing
        return await prepared.bind { _ in
          await importSelection(
            pending, incoming: incoming, before: before, operationID: account.id, lock: lock)
        }
        .flatMap { credentials in
          selection(credentials, account: account, accounts: accounts, outgoing: outgoing)
        }
      }
  }

  private func prepareSelection(
    _ pending: ClaudePendingSelection, account: Account, outgoing: Account?
  ) -> Result<Void, ClaudeFailure> {
    executable()
      .flatMap { _ in createDirectories(store: defaultStore()) }
      .flatMap { _ in Task.isCancelled ? .failure(.cancelled) : .success(()) }
      .flatMap { _ in
        updateSelectionState { state in
          state.pending = pending
          if let outgoing { state.preserve(ClaudeStoredAccount(outgoing)) }
          state.preserve(ClaudeStoredAccount(account))
        }
      }
  }

  private func importSelection(
    _ pending: ClaudePendingSelection, incoming: ClaudeCredentials, before: ClaudeCredentials?,
    operationID: UUID, lock: ClaudeLock
  ) async -> Result<ClaudeCredentials?, ClaudeFailure> {
    let saved: Result<Void, ClaudeFailure> =
      if let outgoing = pending.outgoing, let before, outgoing.id != pending.incoming.id {
        await write(before, to: privateStore(outgoing.id), operationID: operationID)
      } else {
        .success(())
      }
    let imported: Result<Void, ClaudeFailure> = await saved.bind { _ in
      await importCredentials(pending, token: incoming.token, lock: lock)
    }
    let reconciled: Result<ClaudeCredentials?, ClaudeFailure> = await selectionState()
      .flatMap { recorded in
        recorded.pending.map(Result<ClaudePendingSelection, ClaudeFailure>.success)
          ?? .failure(.unfinishedSelection)
      }
      .bind { pending in await reconcileSelection(pending) }
    return imported.mapError { error in error.releasing(reconciled.map { _ in () }) }
      .flatMap { _ in reconciled }
  }

  private func selection(
    _ credentials: ClaudeCredentials?, account: Account, accounts: [Account], outgoing: Account?
  ) -> Result<AccountSelection, ClaudeFailure> {
    guard let credentials, credentials.identity.isSameAccount(as: account.identity) else {
      return .failure(.accountChanged)
    }
    let preserved: Account? = outgoing.flatMap { value in
      accounts.contains(where: { known in known.id == value.id }) ? nil : value
    }
    return .success(AccountSelection(identity: credentials.identity, preservedAccount: preserved))
  }

  private func importCredentials(
    _ pending: ClaudePendingSelection, token: ClaudeOAuthToken, lock: ClaudeLock
  ) async -> Result<Void, ClaudeFailure> {
    await lock.status()
      .flatMap { _ in Task.isCancelled ? .failure(.cancelled) : .success(()) }
      .flatMap { _ in updateSelectionState { state in state.pending = pending } }
      .bind { _ in
        await authenticate(store: defaultStore(), email: nil, token: token, selection: pending)
      }
  }

  private func write(
    _ credentials: ClaudeCredentials, to store: ClaudeCredentialStore, operationID: UUID
  ) async -> Result<Void, ClaudeFailure> {
    await withStorageWriteLock(store, operationID: operationID) { await store.write(credentials) }
  }

  private func reconcileSelection(
    _ pending: ClaudePendingSelection
  ) async -> Result<ClaudeCredentials?, ClaudeFailure> {
    await defaultStore().read().bind { current in
      await selectionState().bind { recorded in
        await reconciled(recorded, pending: pending, current: current)
          .flatMap { state in state.write(to: paths.claudeSelectionFile) }
          .map { _ in current }
      }
    }
  }

  private func reconciled(
    _ recorded: ClaudeSelectionState, pending: ClaudePendingSelection,
    current: ClaudeCredentials?
  ) async -> Result<ClaudeSelectionState, ClaudeFailure> {
    var state: ClaudeSelectionState = recorded
    let selected: UUID? = current.flatMap { credentials in
      recorded.preservedAccount(for: credentials.identity)?.id
    }
    state.selectedAccountID = selected
    state.pending = nil
    if let selected { state.unavailableAccountIDs.remove(selected) }
    if pending.phase == .importing, selected != pending.incoming.id {
      state.unavailableAccountIDs.insert(pending.incoming.id)
    }
    guard let outgoing: ClaudeStoredAccount = pending.outgoing, outgoing.id != selected,
      outgoing.id != pending.incoming.id
    else {
      return .success(state)
    }
    let reconciled: ClaudeSelectionState = state
    return await privateStore(outgoing.id).read().map { saved in
      var updated: ClaudeSelectionState = reconciled
      if saved.map({ value in outgoing.isSameAccount(as: value.identity) }) == true {
        updated.unavailableAccountIDs.remove(outgoing.id)
      } else {
        updated.unavailableAccountIDs.insert(outgoing.id)
      }
      return updated
    }
  }

  private func recoverSelection(operationID: UUID) async -> Result<Void, ClaudeFailure> {
    await selectionState().bind { state -> Result<Void, ClaudeFailure> in
      guard let pending: ClaudePendingSelection = state.pending else { return .success(()) }
      let running: Result<Bool, ClaudeFailure> =
        pending.child.map { child in child.isRunning() } ?? .success(false)
      return await running.bind { running -> Result<Void, ClaudeFailure> in
        await running ? .failure(.unfinishedSelection) : recover(pending, operationID: operationID)
      }
    }
  }

  private func recover(
    _ pending: ClaudePendingSelection, operationID: UUID
  ) async -> Result<Void, ClaudeFailure> {
    let stores: [ClaudeCredentialStore?] = [
      defaultStore(), privateStore(pending.incoming.id),
      pending.outgoing.map { value in privateStore(value.id) },
    ]
    return await withRefreshLock(
      stores.compactMap { store in store?.directory }, operationID: operationID
    ) { _ in
      await reconcileSelection(pending).map { _ in () }
    }
  }

  private func signOutAccount(_ account: Account, accounts: [Account]) async -> Result<
    Void, ClaudeFailure
  > {
    await recoverSelection(operationID: account.id).bind { _ in
      let shared: ClaudeCredentialStore = defaultStore()
      let saved: ClaudeCredentialStore = privateStore(account.id)
      return await withRefreshLock([shared.directory, saved.directory], operationID: account.id) {
        _ in
        await signOut(account, accounts: accounts, shared: shared, saved: saved)
      }
    }
  }

  private func signOut(
    _ account: Account, accounts: [Account], shared: ClaudeCredentialStore,
    saved: ClaudeCredentialStore
  ) async -> Result<Void, ClaudeFailure> {
    await shared.read().bind { current -> Result<Void, ClaudeFailure> in
      guard let current, current.identity.isSameAccount(as: account.identity) else {
        return await saved.read().bind { credentials -> Result<Void, ClaudeFailure> in
          switch credentials {
          case .some(let value) where value.identity.isSameAccount(as: account.identity):
            return await logOut(store: saved)
          case .some: return .failure(.accountChanged)
          case .none: return .success(())
          }
        }
      }
      let erased: Result<Void, ClaudeFailure> = await write(
        current, to: saved, operationID: account.id
      ).bind { _ in
        await withStorageWriteLock(shared, operationID: account.id) {
          await shared.removeOAuthToken(matching: current.token.value)
        }
      }
      guard case .failure(let error) = erased else { return await logOut(store: saved) }
      guard case .accountChanged = error.cause else { return .failure(error) }
      return .failure(
        error.releasing(
          await currentSelection(accounts: accounts).flatMap { _ in
            updateSelectionState { state in state.unavailableAccountIDs.remove(account.id) }
          }))
    }
    .flatMap { _ in
      updateSelectionState { state in
        if state.selectedAccountID == account.id { state.selectedAccountID = nil }
        state.unavailableAccountIDs.insert(account.id)
      }
    }
  }

  private func discardPrivateStore(id: UUID) async -> Result<Void, ClaudeFailure> {
    let store: ClaudeCredentialStore = privateStore(id)
    return await withRefreshLock([store.directory], operationID: id) { _ in
      await withStorageWriteLock(store, operationID: id) { await store.deleteCredentials() }
    }
    .flatMap { _ in
      Result { try FileManager.default.removeItem(at: store.directory) }
        .mapError(ClaudeFailure.filesystem)
    }
    .flatMap { _ in
      updateSelectionState { state in
        state.preservedAccounts.removeAll { record in record.id == id }
        state.unavailableAccountIDs.remove(id)
      }
    }
  }

  private func runSession(
    store: ClaudeCredentialStore, action: ClaudeSessionAction, token: ClaudeOAuthToken? = nil
  ) async -> Result<Void, ClaudeFailure> {
    await createDirectories(store: store).flatMap { _ in executable() }.bind {
      binary -> Result<Void, ClaudeFailure> in
      let sessionID: UUID = UUID()
      var childEnvironment: [String: String] = processEnvironment(store: store)
      if let token {
        childEnvironment["CLAUDE_CODE_OAUTH_TOKEN"] = token.accessToken
        childEnvironment["CLAUDE_CODE_SUBSCRIPTION_TYPE"] = token.plan
        childEnvironment["CLAUDE_CODE_RATE_LIMIT_TIER"] = token.value["rateLimitTier"]?.stringValue
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
      return await ClaudeSession.run(invocation: invocation, action: action, sessionID: sessionID)
    }
  }

  private func authenticate(
    store: ClaudeCredentialStore, email: String?, token: ClaudeOAuthToken?,
    selection: ClaudePendingSelection?
  ) async -> Result<Void, ClaudeFailure> {
    await createDirectories(store: store)
      .flatMap { _ in executable() }
      .bind { binary -> Result<ChildProcess, ClaudeFailure> in
        var arguments: [String] = ["auth", "login", "--claudeai"]
        if let email { arguments.append(contentsOf: ["--email", email]) }
        var childEnvironment: [String: String] = processEnvironment(store: store)
        if let token {
          guard let refresh: String = token.refreshToken, !refresh.isEmpty,
            !token.scopes.isEmpty
          else { return .failure(.signInRequired) }
          childEnvironment["CLAUDE_CODE_OAUTH_REFRESH_TOKEN"] = refresh
          childEnvironment["CLAUDE_CODE_OAUTH_SCOPES"] = token.scopes.joined(separator: " ")
          childEnvironment["CLAUDE_CODE_OAUTH_CLIENT_ID"] = token.clientID
        }
        guard !Task.isCancelled else { return .failure(.cancelled) }
        let invocation: ProcessInvocation = ProcessInvocation(
          executable: binary, arguments: arguments, environment: childEnvironment,
          workingDirectory: paths.workingDirectory
        )
        return await ChildProcess.launch(invocation).mapError(ClaudeFailure.process)
      }
      .mapError { error in importNotStarted(error, selection: selection) }
      .bind { process -> Result<Void, ClaudeFailure> in
        if case .failure(let error) = recordChild(of: process, selection: selection) {
          await process.cancel()
          return .failure(error)
        }
        return await withTaskCancellationHandler {
          let termination: Result<ProcessTermination, ProcessFailure> =
            await process.waitUntilExit()
          return Task.isCancelled
            ? .failure(.cancelled) : termination.exited(ClaudeFailure.authenticationFailed)
        } onCancel: {
          Task { await process.cancel() }
        }
      }
  }

  private func recordChild(
    of process: ChildProcess, selection: ClaudePendingSelection?
  ) -> Result<Void, ClaudeFailure> {
    selection.map { selection in
      ClaudeChildProcess.read(processID: process.processIdentifier).flatMap { child in
        var importing: ClaudePendingSelection = selection
        importing.child = child
        importing.phase = .importing
        return updateSelectionState { state in state.pending = importing }
      }
    } ?? .success(())
  }

  private func importNotStarted(
    _ failure: ClaudeFailure, selection: ClaudePendingSelection?
  ) -> ClaudeFailure {
    guard var selection else { return failure }
    selection.phase = .prepared
    selection.child = nil
    return failure.releasing(updateSelectionState { state in state.pending = selection })
  }

  private func logOut(store: ClaudeCredentialStore) async -> Result<Void, ClaudeFailure> {
    await (Task.isCancelled ? .failure(.cancelled) : createDirectories(store: store))
      .flatMap { _ in executable() }
      .bind { binary in
        await ChildProcess.launch(
          ProcessInvocation(
            executable: binary, arguments: ["auth", "logout"],
            environment: processEnvironment(store: store),
            workingDirectory: paths.workingDirectory
          )
        ).mapError(ClaudeFailure.process)
      }
      .bind { process in
        await withTaskCancellationHandler {
          await process.closeInput()
          let termination: Result<ProcessTermination, ProcessFailure> =
            await process.waitUntilExit()
          return Task.isCancelled ? .failure(.cancelled) : termination.exited()
        } onCancel: {
          Task { await process.cancel() }
        }
      }
  }

  private func executable() -> Result<URL, ClaudeFailure> {
    let candidates: [URL] =
      (environment["PATH"]?.split(separator: ":").map { path in
        URL(fileURLWithPath: String(path), isDirectory: true).appending(path: "claude")
      } ?? []) + [homeDirectory().appending(path: ".local/bin/claude")]
    return candidates.first { url in FileManager.default.isExecutableFile(atPath: url.path) }
      .map(Result<URL, ClaudeFailure>.success) ?? .failure(.executableMissing)
  }

  private func processEnvironment(store: ClaudeCredentialStore) -> [String: String] {
    var result: [String: String] = environment.filter { entry in
      !Self.excludedEnvironmentVariables.contains(entry.key)
    }
    let shared: ClaudeCredentialStore = defaultStore()
    if let path: String = store.configDirectoryPath,
      store.service != shared.service || store.configFile != shared.configFile
    {
      result["CLAUDE_CONFIG_DIR"] = path
      result.removeValue(forKey: "CLAUDE_SECURESTORAGE_CONFIG_DIR")
    }
    result["CLAUDE_CODE_DISABLE_AUTO_MEMORY"] = "1"
    return result
  }

  private func privateStore(_ id: UUID) -> ClaudeCredentialStore {
    let directory: URL = paths.claudeDirectory(id)
    return ClaudeCredentialStore(
      directory: directory, configFile: directory.appending(path: ".claude.json"),
      legacyConfigFile: directory.appending(path: ".config.json"),
      configDirectoryPath: directory.path.precomposedStringWithCanonicalMapping,
      username: username
    )
  }

  private func defaultStore() -> ClaudeCredentialStore {
    let configured: String? = environment["CLAUDE_CONFIG_DIR"].flatMap { value in
      value.isEmpty ? nil : value
    }
    let secure: String? = environment["CLAUDE_SECURESTORAGE_CONFIG_DIR"]
    let directory: URL =
      if let secure, !secure.isEmpty {
        URL(fileURLWithPath: secure.precomposedStringWithCanonicalMapping, isDirectory: true)
      } else if secure != nil {
        homeDirectory().appending(path: ".claude", directoryHint: .isDirectory)
      } else {
        paths.defaultClaudeDirectory
      }
    let configFile: URL =
      configured == nil
      ? homeDirectory().appending(path: ".claude.json")
      : paths.defaultClaudeDirectory.appending(path: ".claude.json")
    let configDirectoryPath: String? =
      if let secure {
        secure.isEmpty ? nil : secure.precomposedStringWithCanonicalMapping
      } else {
        configured?.precomposedStringWithCanonicalMapping
      }
    return ClaudeCredentialStore(
      directory: directory, configFile: configFile,
      legacyConfigFile: paths.defaultClaudeDirectory.appending(path: ".config.json"),
      configDirectoryPath: configDirectoryPath, username: username
    )
  }

  private func homeDirectory() -> URL {
    environment["HOME"].flatMap { home in
      home.isEmpty ? nil : URL(fileURLWithPath: home, isDirectory: true)
    } ?? FileManager.default.homeDirectoryForCurrentUser
  }

  private func createDirectories(store: ClaudeCredentialStore) -> Result<Void, ClaudeFailure> {
    Result {
      try FileManager.default.createDirectory(
        at: paths.workingDirectory, withIntermediateDirectories: true)
      try FileManager.default.createDirectory(
        at: store.directory, withIntermediateDirectories: true)
    }
    .mapError(ClaudeFailure.filesystem)
  }

  private func selectionState() -> Result<ClaudeSelectionState, ClaudeFailure> {
    ClaudeSelectionState.read(at: paths.claudeSelectionFile)
  }

  private func updateSelectionState(
    _ change: (inout ClaudeSelectionState) -> Void
  ) -> Result<Void, ClaudeFailure> {
    selectionState().flatMap { state in
      var updated: ClaudeSelectionState = state
      change(&updated)
      return updated.write(to: paths.claudeSelectionFile)
    }
  }
}
