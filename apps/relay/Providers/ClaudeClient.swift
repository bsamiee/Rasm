import CryptoKit
import Foundation
import Subprocess
import System

nonisolated enum ClaudeSelection: Equatable, Sendable {
  case none
  case known(UUID)
  case unknown(AccountIdentity)
}

private nonisolated struct ClaudeCachedUsage: Sendable {
  let usage: AccountUsage
  let until: Date
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
  private static let executableDirectories: [String] = [
    ".local/bin", ".bun/bin", ".npm-global/bin", ".volta/bin",
  ]
  private static let systemExecutableDirectories: [String] = [
    "/opt/homebrew/bin", "/usr/local/bin",
  ]
  private static let usageCacheDuration: TimeInterval = 15 * 60
  private static let loginDeadline: Duration = .seconds(300)
  private static let logoutDeadline: Duration = .seconds(60)

  private let paths: FileLocations
  private let environment: [String: String]
  private let username: String
  private let session: URLSession
  nonisolated let sharedConfigFile: URL
  private var verifiedIdentities: [SHA256Digest: AccountIdentity] = [:]
  private var usageCache: [UUID: ClaudeCachedUsage] = [:]
  private var retryAfter: [UUID: Date] = [:]
  private var lastCredential: ClaudeCredential?
  private var lastSelection: AccountIdentity?

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
    sharedConfigFile =
      Self.defaultStore(
        paths: paths, environment: environment, username: username
      ).configFile
  }

  func settle(known: [Account]) async -> Result<Void, ClaudeFailure> {
    await selectionState().bind { state -> Result<Void, ClaudeFailure> in
      guard let pending: ClaudePendingSwitch = state.pending else { return .success(()) }
      return await settle(pending, known: known)
    }
  }

  private func settle(
    _ pending: ClaudePendingSwitch, known: [Account]
  ) async -> Result<Void, ClaudeFailure> {
    let shared: ClaudeCredentialStore = defaultStore()
    let incoming: ClaudeCredentialStore = privateStore(pending.incoming)
    let outgoing: ClaudeCredentialStore? = pending.outgoing.map(privateStore)
    let stores: [ClaudeCredentialStore] = [shared, incoming] + (outgoing.map { [$0] } ?? [])
    let copy: (id: UUID?, store: ClaudeCredentialStore?) =
      switch pending.phase {
      case .saving: (pending.outgoing, outgoing)
      case .installing: (pending.incoming, incoming)
      }
    return await ClaudeLock.oauthRefresh(directories: stores.map(\.directory)) {
      await shared.identity()
        .bind { identity in await self.deleteDuplicate(copy, of: identity, known: known) }
        .bind { _ in await self.record(nil) }
    }
  }

  private func deleteDuplicate(
    _ copy: (id: UUID?, store: ClaudeCredentialStore?), of identity: AccountIdentity?,
    known: [Account]
  ) async -> Result<Void, ClaudeFailure> {
    guard let identity, let store: ClaudeCredentialStore = copy.store,
      let account: Account = known.first(where: { account in account.id == copy.id }),
      account.identity.isSameAccount(as: identity)
    else { return .success(()) }
    return await store.deleteItem()
  }

  func currentSelection(known: [Account]) async -> Result<ClaudeSelection, ClaudeFailure> {
    await defaultStore().identity().bind { identity -> Result<ClaudeSelection, ClaudeFailure> in
      guard let identity else {
        lastSelection = nil
        lastCredential = nil
        return .success(.none)
      }
      let saved: Result<Void, ClaudeFailure> = await saveLastCredential(
        incoming: identity, known: known)
      lastSelection = identity
      let match: Account? = known.first { account in
        account.provider == .claude && account.identity.isSameAccount(as: identity)
      }
      return saved.map { _ in match.map { account in .known(account.id) } ?? .unknown(identity) }
    }
  }

  func connect(id: UUID) async -> Result<AccountIdentity, ClaudeFailure> {
    let store: ClaudeCredentialStore = privateStore(id)
    return await authenticate(store: store, email: nil)
      .bind { _ in await store.read() }
      .flatMap { content in
        content.credential.map { credential in .success(credential.identity) }
          ?? .failure(.signInRequired)
      }
  }

  func reconnect(account: Account) async -> Result<AccountIdentity, ClaudeFailure> {
    let store: ClaudeCredentialStore = privateStore(account.id)
    return await authenticate(store: store, email: account.identity.email)
      .bind { _ in await store.read() }
      .bind { content -> Result<AccountIdentity, ClaudeFailure> in
        guard let credential: ClaudeCredential = content.credential else {
          return .failure(.signInRequired)
        }
        guard credential.identity.isSameAccount(as: account.identity) else {
          return .failure(.accountChanged.releasing(await logOut(store: store)))
        }
        return .success(credential.identity)
      }
  }

  func discardConnection(id: UUID) async -> Result<Void, ClaudeFailure> {
    let store: ClaudeCredentialStore = privateStore(id)
    usageCache.removeValue(forKey: id)
    return await store.deleteItem().flatMap { _ in
      Result { try FileManager.default.removeItem(at: store.directory) }.flatMapError { error in
        (error as? CocoaError)?.code == .fileNoSuchFile
          ? .success(()) : .failure(.filesystem(error))
      }
    }
  }

  func usage(
    for account: Account, isSelected: Bool, fresh: Bool
  ) async -> Result<AccountUsage, ClaudeFailure> {
    let now: Date = Date()
    if !fresh, let cached: ClaudeCachedUsage = usageCache[account.id], cached.until > now {
      return .success(cached.usage)
    }
    if let limit: Date = retryAfter[account.id], limit > now {
      return .failure(.rateLimited(until: limit))
    }
    return await credential(for: account, isSelected: isSelected).bind { stored in
      await request(for: account, isSelected: isSelected, credential: stored) { credential in
        await fetchUsage(credential.token)
      }
    }
    .map { usage in
      usageCache[account.id] = ClaudeCachedUsage(
        usage: usage, until: Date().addingTimeInterval(Self.usageCacheDuration))
      return usage
    }
    .mapError { error in
      if case .rateLimited(let until) = error.cause { retryAfter[account.id] = until }
      return error
    }
  }

  func startSession(
    for account: Account, isSelected: Bool
  ) async -> Result<AccountUsage, ClaudeFailure> {
    await usage(for: account, isSelected: isSelected, fresh: false).bind { current in
      guard case .ready = current.availability(at: Date()) else { return .success(current) }
      return await credential(for: account, isSelected: isSelected).bind { stored in
        await greet(store: store(for: account, isSelected: isSelected), token: stored.token)
      }
      .bind { _ in await usage(for: account, isSelected: isSelected, fresh: true) }
    }
  }

  func select(_ account: Account, outgoing: Account?) async -> Result<
    AccountIdentity, ClaudeFailure
  > {
    let shared: ClaudeCredentialStore = defaultStore()
    let incoming: ClaudeCredentialStore = privateStore(account.id)
    let outgoingStore: ClaudeCredentialStore? = outgoing.map { value in privateStore(value.id) }
    let directories: [URL] =
      [shared.directory, incoming.directory] + (outgoingStore.map { [$0.directory] } ?? [])
    return await ClaudeLock.oauthRefresh(directories: directories) {
      await self.switchCredential(
        account, outgoing: outgoing, shared: shared, incoming: incoming,
        outgoingStore: outgoingStore)
    }
    .bind { credential in await verify(credential).map { verified in verified.identity } }
  }

  func signOut(_ account: Account, isSelected: Bool) async -> Result<Void, ClaudeFailure> {
    let store: ClaudeCredentialStore = store(for: account, isSelected: isSelected)
    usageCache.removeValue(forKey: account.id)
    if isSelected { lastCredential = nil }
    return await logOut(store: store)
  }

  func remove(_ account: Account, isSelected: Bool) async -> Result<Void, ClaudeFailure> {
    await signOut(account, isSelected: isSelected).bind { _ in
      await discardConnection(id: account.id)
    }
  }

  private func saveLastCredential(
    incoming identity: AccountIdentity, known: [Account]
  ) async -> Result<Void, ClaudeFailure> {
    guard let previous: AccountIdentity = lastSelection, !previous.isSameAccount(as: identity),
      let outgoing: Account = known.first(where: { account in
        account.provider == .claude && account.identity.isSameAccount(as: previous)
      }),
      let credential: ClaudeCredential = lastCredential,
      credential.identity.isSameAccount(as: previous)
    else {
      if !(lastSelection?.isSameAccount(as: identity) ?? false) { lastCredential = nil }
      return .success(())
    }
    lastCredential = nil
    let store: ClaudeCredentialStore = privateStore(outgoing.id)
    return await ClaudeLock.oauthRefresh(directories: [store.directory]) {
      await store.readItem().bind { existing -> Result<Void, ClaudeFailure> in
        let holdsCredential: Bool =
          existing?["claudeAiOauth"].map { value in value != .null } == true
        return await holdsCredential ? .success(()) : self.write(credential, into: store)
      }
    }
  }

  private func write(
    _ credential: ClaudeCredential, into store: ClaudeCredentialStore
  ) async -> Result<Void, ClaudeFailure> {
    await ClaudeLock.storageWrite(directory: store.directory) {
      await store.writeItem(credential.item)
    }
    .flatMap { _ in store.writeAccount(credential.account) }
  }

  private func record(_ pending: ClaudePendingSwitch?) -> Result<Void, ClaudeFailure> {
    updateSelectionState { state in state.pending = pending }
  }

  private func switchCredential(
    _ account: Account, outgoing: Account?, shared: ClaudeCredentialStore,
    incoming: ClaudeCredentialStore, outgoingStore: ClaudeCredentialStore?
  ) async -> Result<ClaudeCredential, ClaudeFailure> {
    await shared.read().bind { before -> Result<ClaudeCredential, ClaudeFailure> in
      await incoming.read().bind { content -> Result<ClaudeCredential, ClaudeFailure> in
        await self.install(
          content.credential, replacing: before.credential, for: account, outgoing: outgoing,
          shared: shared, incoming: incoming, outgoingStore: outgoingStore)
      }
    }
  }

  private func install(
    _ credential: ClaudeCredential?, replacing current: ClaudeCredential?, for account: Account,
    outgoing: Account?, shared: ClaudeCredentialStore, incoming: ClaudeCredentialStore,
    outgoingStore: ClaudeCredentialStore?
  ) async -> Result<ClaudeCredential, ClaudeFailure> {
    guard let credential, credential.identity.isSameAccount(as: account.identity) else {
      return .failure(.signInRequired)
    }
    if let current, current.identity.isSameAccount(as: account.identity) {
      return await incoming.deleteItem().map { _ in current }
    }
    let saving: ClaudePendingSwitch = ClaudePendingSwitch(
      incoming: account.id, outgoing: outgoing?.id, phase: .saving)
    let installing: ClaudePendingSwitch = ClaudePendingSwitch(
      incoming: account.id, outgoing: outgoing?.id, phase: .installing)
    return await record(saving)
      .bind { _ in await self.save(current, for: outgoing, into: outgoingStore) }
      .flatMap { _ in self.record(installing) }
      .bind { _ in await self.write(credential, into: shared) }
      .bind { _ in await incoming.deleteItem() }
      .flatMap { _ in self.record(nil) }
      .map { _ in
        lastCredential = credential
        lastSelection = credential.identity
        usageCache.removeValue(forKey: account.id)
        return credential
      }
  }

  private func save(
    _ current: ClaudeCredential?, for outgoing: Account?, into store: ClaudeCredentialStore?
  ) async -> Result<Void, ClaudeFailure> {
    guard let current, let outgoing, let store,
      current.identity.isSameAccount(as: outgoing.identity)
    else { return .success(()) }
    return await write(current, into: store)
  }

  private func store(for account: Account, isSelected: Bool) -> ClaudeCredentialStore {
    isSelected ? defaultStore() : privateStore(account.id)
  }

  private func credential(
    for account: Account, isSelected: Bool
  ) async -> Result<ClaudeCredential, ClaudeFailure> {
    let store: ClaudeCredentialStore = store(for: account, isSelected: isSelected)
    return await store.read()
      .flatMap { content in matched(content, to: account.identity) }
      .bind { credential -> Result<ClaudeCredential, ClaudeFailure> in
        await credential.token.needsRefresh(at: Date())
          ? refreshCredential(in: store, for: account.identity) : .success(credential)
      }
      .map { credential in
        if isSelected { lastCredential = credential }
        return credential
      }
  }

  private func request<Value: Sendable>(
    for account: Account, isSelected: Bool, credential: ClaudeCredential,
    _ send: (ClaudeCredential) async -> Result<Value, ClaudeFailure>
  ) async -> Result<Value, ClaudeFailure> {
    let sent: Result<Value, ClaudeFailure> = await verify(credential).bind(send)
    guard case .failure(let error) = sent, error.unauthorized else { return sent }
    let store: ClaudeCredentialStore = store(for: account, isSelected: isSelected)
    return await refreshCredential(in: store, for: account.identity)
      .bind { renewed in await verify(renewed).bind(send) }
      .mapError { retried in retried.unauthorized ? .signInRequired : retried }
  }

  private func refreshCredential(
    in store: ClaudeCredentialStore, for identity: AccountIdentity
  ) async -> Result<ClaudeCredential, ClaudeFailure> {
    await runSession(store: store, action: .refreshCredentials, token: nil)
      .bind { _ in await store.read() }
      .flatMap { content in matched(content, to: identity) }
  }

  private func matched(
    _ content: ClaudeStoreContent, to identity: AccountIdentity
  ) -> Result<ClaudeCredential, ClaudeFailure> {
    switch content {
    case .credential(let credential) where credential.identity.isSameAccount(as: identity):
      .success(credential)
    case .credential: .failure(.accountChanged)
    case .empty, .signedOut: .failure(.signInRequired)
    }
  }

  private func verify(_ credential: ClaudeCredential) async -> Result<
    ClaudeCredential, ClaudeFailure
  > {
    let fingerprint: SHA256Digest = credential.token.fingerprint
    if let identity: AccountIdentity = verifiedIdentities[fingerprint] {
      return identity.isSameAccount(as: credential.identity)
        ? .success(credential) : .failure(.accountChanged)
    }
    return await fetch(
      "https://api.anthropic.com/api/oauth/profile", bearer: credential.token.accessToken,
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
        plan: credential.token.plan
      ).mapError(ClaudeFailure.invalidIdentity)
    }
    .flatMap { identity -> Result<ClaudeCredential, ClaudeFailure> in
      guard identity.isSameAccount(as: credential.identity) else {
        return .failure(.accountChanged)
      }
      verifiedIdentities[fingerprint] = identity
      return .success(credential)
    }
  }

  private func fetchUsage(_ token: ClaudeOAuthToken) async -> Result<AccountUsage, ClaudeFailure> {
    await fetch(
      "https://api.anthropic.com/api/oauth/usage", bearer: token.accessToken,
      headers: ["anthropic-beta": "oauth-2025-04-20", "Accept": "application/json"]
    )
    .flatMap { data in decode(ClaudeUsageResponse.self, from: data) }
    .flatMap { usage in usage.usage(observedAt: Date()) }
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
      guard let http: HTTPURLResponse = response as? HTTPURLResponse else {
        return .failure(.invalidResponse)
      }
      switch http.statusCode {
      case 200: return Task.isCancelled ? .failure(.cancelled) : .success(data)
      case 429:
        let seconds: TimeInterval =
          http.value(forHTTPHeaderField: "Retry-After").flatMap(TimeInterval.init) ?? 0
        return .failure(
          .rateLimited(
            until: Date().addingTimeInterval(seconds > 0 ? seconds : Self.usageCacheDuration)))
      case let status: return .failure(.http(status))
      }
    }
  }

  private func decode<Value: Decodable>(
    _ type: Value.Type, from data: Data
  ) -> Result<Value, ClaudeFailure> {
    Result { try JSONDecoder().decode(type, from: data) }.mapError { _ in .invalidResponse }
  }

  private func greet(
    store: ClaudeCredentialStore, token: ClaudeOAuthToken
  ) async -> Result<Void, ClaudeFailure> {
    await runSession(store: store, action: .greeting, token: token)
  }

  private func runSession(
    store: ClaudeCredentialStore, action: ClaudeSessionAction, token: ClaudeOAuthToken?
  ) async -> Result<Void, ClaudeFailure> {
    await createDirectories(store: store).flatMap { _ in executable() }.bind {
      binary -> Result<Void, ClaudeFailure> in
      let sessionID: UUID = UUID()
      var childEnvironment: [String: String] = processEnvironment(store: store)
      let tokenPipe: Result<FileDescriptor?, ClaudeFailure> =
        token.map { token in
          Self.tokenDescriptor(token.accessToken).map { descriptor in
            childEnvironment["CLAUDE_CODE_OAUTH_TOKEN_FILE_DESCRIPTOR"] = String(
              descriptor.rawValue)
            if let plan: String = token.plan {
              childEnvironment["CLAUDE_CODE_SUBSCRIPTION_TYPE"] = plan
            }
            if let tier: String = token.value["rateLimitTier"]?.stringValue {
              childEnvironment["CLAUDE_CODE_RATE_LIMIT_TIER"] = tier
            }
            return descriptor
          }
        } ?? .success(nil)
      return await tokenPipe.bind { descriptor -> Result<Void, ClaudeFailure> in
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
          workingDirectory: paths.workingDirectory,
          inheritedInput: descriptor
        )
        let outcome: Result<Void, ClaudeFailure> = await ClaudeSession.run(
          invocation: invocation, action: action, sessionID: sessionID)
        if let descriptor { try? descriptor.close() }
        return outcome
      }
    }
  }

  private static func tokenDescriptor(_ token: String) -> Result<FileDescriptor, ClaudeFailure> {
    Result { try FileDescriptor.pipe() }
      .mapError(ClaudeFailure.filesystem)
      .flatMap { pipe in
        Result { try pipe.writeEnd.closeAfter { try pipe.writeEnd.writeAll(token.utf8) } }
          .map { _ in pipe.readEnd }
          .mapError { error in
            try? pipe.readEnd.close()
            return .filesystem(error)
          }
      }
  }

  private func authenticate(
    store: ClaudeCredentialStore, email: String?
  ) async -> Result<Void, ClaudeFailure> {
    await createDirectories(store: store)
      .flatMap { _ in executable() }
      .bind { binary -> Result<Void, ClaudeFailure> in
        var arguments: [String] = ["auth", "login", "--claudeai"]
        if let email { arguments.append(contentsOf: ["--email", email]) }
        return await ProcessRun.collect(
          ProcessInvocation(
            executable: binary, arguments: arguments, environment: processEnvironment(store: store),
            workingDirectory: paths.workingDirectory
          ), deadline: Self.loginDeadline
        )
        .claude()
        .flatMap { output in
          output.status.isSuccess ? .success(()) : .failure(.authenticationFailed(output.status))
        }
      }
  }

  private func logOut(store: ClaudeCredentialStore) async -> Result<Void, ClaudeFailure> {
    await createDirectories(store: store)
      .flatMap { _ in executable() }
      .bind { binary in
        await ProcessRun.collect(
          ProcessInvocation(
            executable: binary, arguments: ["auth", "logout"],
            environment: processEnvironment(store: store),
            workingDirectory: paths.workingDirectory
          ), deadline: Self.logoutDeadline
        ).claude()
      }
      .bind { _ in await store.deleteItem() }
  }

  private func executable() -> Result<URL, ClaudeFailure> {
    let home: URL = homeDirectory()
    let directories: [URL] =
      Self.executableDirectories.map { path in
        home.appending(path: path, directoryHint: .isDirectory)
      }
      + Self.systemExecutableDirectories.map { path in
        URL(filePath: path, directoryHint: .isDirectory)
      }
      + (environment["PATH"]?.split(separator: ":").map { path in
        URL(filePath: String(path), directoryHint: .isDirectory)
      } ?? [])
    return directories.map { directory in directory.appending(path: "claude") }
      .first { url in FileManager.default.isExecutableFile(atPath: url.path) }
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
      directoryConfigFile: directory.appending(path: ".config.json"),
      configDirectoryPath: directory.path.precomposedStringWithCanonicalMapping,
      username: username
    )
  }

  private func defaultStore() -> ClaudeCredentialStore {
    Self.defaultStore(paths: paths, environment: environment, username: username)
  }

  private static func defaultStore(
    paths: FileLocations, environment: [String: String], username: String
  ) -> ClaudeCredentialStore {
    let home: URL = homeDirectory(environment)
    let configured: String? = environment["CLAUDE_CONFIG_DIR"].flatMap { value in
      value.isEmpty ? nil : value
    }
    let secure: String? = environment["CLAUDE_SECURESTORAGE_CONFIG_DIR"]
    let directory: URL =
      if let secure, !secure.isEmpty {
        URL(filePath: secure.precomposedStringWithCanonicalMapping, directoryHint: .isDirectory)
      } else if secure != nil {
        home.appending(path: ".claude", directoryHint: .isDirectory)
      } else {
        paths.defaultClaudeDirectory
      }
    let configFile: URL =
      configured == nil
      ? home.appending(path: ".claude.json")
      : paths.defaultClaudeDirectory.appending(path: ".claude.json")
    let configDirectoryPath: String? =
      if let secure {
        secure.isEmpty ? nil : secure.precomposedStringWithCanonicalMapping
      } else {
        configured?.precomposedStringWithCanonicalMapping
      }
    return ClaudeCredentialStore(
      directory: directory, configFile: configFile,
      directoryConfigFile: paths.defaultClaudeDirectory.appending(path: ".config.json"),
      configDirectoryPath: configDirectoryPath, username: username
    )
  }

  private func homeDirectory() -> URL {
    Self.homeDirectory(environment)
  }

  private static func homeDirectory(_ environment: [String: String]) -> URL {
    environment["HOME"].flatMap { home in
      home.isEmpty ? nil : URL(filePath: home, directoryHint: .isDirectory)
    } ?? URL.homeDirectory
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
