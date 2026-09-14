import Foundation
import Subprocess

nonisolated enum CodexSelection: Equatable, Sendable {
  case none
  case known(UUID)
  case unknown(AccountIdentity)
}

nonisolated struct CodexRateLimitsUpdate: Sendable {
  let home: URL
  let windows: [QuotaWindow]
  let observedAt: Date
}

private struct CodexServer {
  let connection: CodexConnection
  let task: Task<Void, Never>
}

actor CodexClient {
  private static let excludedEnvironmentVariables: Set<String> = [
    "OPENAI_API_KEY", "OPENAI_BASE_URL", "OPENAI_ORG_ID", "OPENAI_PROJECT_ID",
  ]
  private static let requestDeadline: Duration = .seconds(60)
  private static let greetingDeadline: Duration = .seconds(120)
  private static let loginDeadline: Duration = .seconds(300)

  private let paths: FileLocations
  private let environment: [String: String]
  nonisolated let liveHome: URL
  nonisolated let liveAuthFile: URL
  nonisolated let updates: AsyncStream<CodexRateLimitsUpdate>
  private let updatesContinuation: AsyncStream<CodexRateLimitsUpdate>.Continuation
  private var servers: [URL: CodexServer] = [:]

  init(paths: FileLocations, environment: [String: String]) {
    self.paths = paths
    self.environment = environment
    liveHome = paths.defaultCodexHome
    liveAuthFile = paths.defaultCodexHome.appending(path: "auth.json")
    let stream = AsyncStream<CodexRateLimitsUpdate>.makeStream()
    updates = stream.stream
    updatesContinuation = stream.continuation
  }

  nonisolated func home(for account: Account, isSelected: Bool) -> URL {
    isSelected ? liveHome : paths.codexHome(account.id)
  }

  func preconditions() -> Result<Void, CodexFailure> {
    CodexConfigFile.read(at: liveHome.appending(path: "config.toml")).flatMap { config in
      config.switchable
    }
  }

  func currentSelection(known: [Account]) -> Result<CodexSelection, CodexFailure> {
    CodexAuthFile.read(at: liveAuthFile).map { auth in
      guard let auth else { return .none }
      let match: Account? = known.first { account in
        account.provider == .openAI && account.identity.isSameAccount(as: auth.identity)
      }
      return match.map { account in .known(account.id) } ?? .unknown(auth.identity)
    }
    .flatMapError { error in
      if case .signInRequired = error { .success(.none) } else { .failure(error) }
    }
  }

  func connect(id: UUID) async -> Result<AccountIdentity, CodexFailure> {
    let home: URL = paths.codexHome(id)
    return await signIn(home: home).bind { _ in identity(at: home) }
  }

  func reconnect(account: Account) async -> Result<AccountIdentity, CodexFailure> {
    let home: URL = paths.codexHome(account.id)
    return await signIn(home: home).bind { _ in identity(at: home) }.bind { identity in
      guard identity.isSameAccount(as: account.identity) else {
        return await logOut(home: home).flatMap { _ in .failure(.identityChanged) }
      }
      return .success(identity)
    }
  }

  func discardConnection(id: UUID) async -> Result<Void, CodexFailure> {
    let home: URL = paths.codexHome(id)
    await stopServer(home)
    return remove(home)
  }

  func usage(for account: Account, isSelected: Bool) async -> Result<AccountUsage, CodexFailure> {
    let home: URL = home(for: account, isSelected: isSelected)
    return await identity(at: home, matching: account).bind { identity in
      await connection(for: home).bind { connection in
        await withDeadline(Self.requestDeadline) {
          await connection.request(
            CodexProtocol.AccountRateLimits.self, "account/rateLimits/read",
            params: .object(["excludeResetCreditDetails": .bool(true)]))
        }
      }
      .flatMap { response in CodexProtocol.usage(response, identity: identity, observedAt: Date()) }
    }
  }

  func startSession(
    for account: Account, isSelected: Bool
  ) async -> Result<AccountUsage, CodexFailure> {
    let home: URL = home(for: account, isSelected: isSelected)
    return await usage(for: account, isSelected: isSelected).bind { current in
      guard case .ready = current.availability(at: Date()) else { return .success(current) }
      return await greet(home: home).bind { _ in await usage(for: account, isSelected: isSelected) }
    }
  }

  private func greet(home: URL) async -> Result<Void, CodexFailure> {
    let workingDirectory: URL = paths.workingDirectory
    return await connection(for: home).bind { connection in
      await self.withDeadline(Self.greetingDeadline) {
        await CodexProtocol.greetingModel(connection).bind { model in
          await CodexProtocol.sendGreeting(
            connection, model: model, workingDirectory: workingDirectory)
        }
      }
    }
  }

  func select(_ account: Account, outgoing: Account?) async -> Result<
    AccountIdentity, CodexFailure
  > {
    let incoming: URL = paths.codexHome(account.id).appending(path: "auth.json")
    return await preconditions().bind { _ in
      await CodexAuthFile.read(at: incoming).bind {
        parked -> Result<AccountIdentity, CodexFailure> in
        guard let parked, parked.identity.isSameAccount(as: account.identity) else {
          return .failure(.signInRequired)
        }
        await stopServer(liveHome)
        await stopServer(paths.codexHome(account.id))
        if let outgoing { await stopServer(paths.codexHome(outgoing.id)) }
        return await swapAuthFiles(account, outgoing: outgoing, incoming: incoming)
          .bind { _ in await CodexDesktop.relaunchIfRunning() }
          .map { _ in parked.identity }
      }
    }
  }

  func signOut(_ account: Account, isSelected: Bool) async -> Result<Void, CodexFailure> {
    await logOut(home: home(for: account, isSelected: isSelected))
  }

  func remove(_ account: Account, isSelected: Bool) async -> Result<Void, CodexFailure> {
    await signOut(account, isSelected: isSelected)
      .flatMapError { error in error.requiresSignIn ? .success(()) : .failure(error) }
      .bind { _ in await discardConnection(id: account.id) }
  }

  func shutdown() async {
    for home: URL in Array(servers.keys) { await stopServer(home) }
    updatesContinuation.finish()
  }

  private func swapAuthFiles(
    _ account: Account, outgoing: Account?, incoming: URL
  ) async -> Result<Void, CodexFailure> {
    await CodexAuthFile.read(at: liveAuthFile)
      .flatMapError { error in
        if case .signInRequired = error { .success(nil) } else { .failure(error) }
      }
      .bind { live -> Result<Void, CodexFailure> in
        if let live, live.identity.isSameAccount(as: account.identity) {
          return remove(incoming)
        }
        let parked: Result<Void, CodexFailure> =
          if let live, let outgoing, live.identity.isSameAccount(as: outgoing.identity) {
            installFile(
              from: liveAuthFile, to: paths.codexHome(outgoing.id).appending(path: "auth.json"))
          } else {
            .success(())
          }
        return
          parked
          .flatMap { _ in installFile(from: incoming, to: liveAuthFile) }
          .flatMap { _ in remove(incoming) }
      }
  }

  private func installFile(from source: URL, to destination: URL) -> Result<Void, CodexFailure> {
    Result {
      let data: Data = try Data(contentsOf: source)
      let directory: URL = destination.deletingLastPathComponent()
      try FileManager.default.createDirectory(at: directory, withIntermediateDirectories: true)
      let staged: URL = directory.appending(path: ".auth.json.relay-\(UUID().uuidString)")
      guard
        FileManager.default.createFile(
          atPath: staged.path, contents: data, attributes: [.posixPermissions: 0o600])
      else { throw CocoaError(.fileWriteUnknown) }
      _ = try FileManager.default.replaceItemAt(
        destination, withItemAt: staged, options: .usingNewMetadataOnly)
    }
    .mapError(CodexFailure.storage)
  }

  private func remove(_ url: URL) -> Result<Void, CodexFailure> {
    Result { try FileManager.default.removeItem(at: url) }.flatMapError { error in
      (error as? CocoaError)?.code == .fileNoSuchFile ? .success(()) : .failure(.storage(error))
    }
  }

  private func identity(at home: URL) -> Result<AccountIdentity, CodexFailure> {
    CodexAuthFile.read(at: home.appending(path: "auth.json")).flatMap { auth in
      auth.map { auth in .success(auth.identity) } ?? .failure(.signInRequired)
    }
  }

  private func identity(
    at home: URL, matching account: Account
  ) -> Result<AccountIdentity, CodexFailure> {
    identity(at: home).flatMap { identity in
      identity.isSameAccount(as: account.identity) ? .success(identity) : .failure(.identityChanged)
    }
  }

  private func signIn(home: URL) async -> Result<Void, CodexFailure> {
    await connection(for: home).bind { connection in
      await self.withDeadline(Self.loginDeadline) { await Self.login(on: connection) }
    }
  }

  private static func login(on connection: CodexConnection) async -> Result<Void, CodexFailure> {
    await connection.request(
      CodexProtocol.LoginStarted.self, "account/login/start",
      params: .object(["type": .string("chatgpt")])
    )
    .bind { started in await awaitLogin(started, on: connection) }
    .flatMap { completed in
      completed.success ? .success(()) : .failure(.signInRefused(reason: completed.error))
    }
  }

  private static func awaitLogin(
    _ started: CodexProtocol.LoginStarted, on connection: CodexConnection
  ) async -> Result<CodexProtocol.LoginCompleted, CodexFailure> {
    guard let loginID: String = started.loginId, let address: String = started.authUrl,
      let url: URL = URL(string: address), url.scheme == "https", url.host != nil
    else {
      return .failure(.invalidResponse(field: "sign-in response"))
    }
    return await CodexDesktop.openSignIn(url).bind { _ in
      await connection.notification(CodexProtocol.LoginCompleted.self, "account/login/completed") {
        completed in completed.loginId == loginID
      }
    }
  }

  private func logOut(home: URL) async -> Result<Void, CodexFailure> {
    let outcome: Result<Void, CodexFailure> = await connection(for: home).bind { connection in
      await withDeadline(Self.requestDeadline) {
        await connection.request("account/logout").map { _ in () }
      }
    }
    await stopServer(home)
    return outcome
  }

  private func withDeadline<Value: Sendable>(
    _ deadline: Duration, _ work: @escaping @Sendable () async -> Result<Value, CodexFailure>
  ) async -> Result<Value, CodexFailure> {
    await ProcessRun.withDeadline(deadline, timedOut: .timedOut, cancelled: .cancelled, work)
  }

  private func connection(for home: URL) async -> Result<CodexConnection, CodexFailure> {
    if let server: CodexServer = servers[home], await !server.connection.isFinished {
      return .success(server.connection)
    }
    servers.removeValue(forKey: home)
    let workingDirectory: URL = paths.workingDirectory
    let variables: [String: String] =
      environment
      .filter { key, _ in
        !key.hasPrefix("CODEX_") && !Self.excludedEnvironmentVariables.contains(key)
      }
      .merging(["CODEX_HOME": home.path]) { _, override in override }
    return await CodexDesktop.applicationURL()
      .map { application in application.appending(path: "Contents/Resources/codex") }
      .flatMap { executable -> Result<URL, CodexFailure> in
        FileManager.default.isExecutableFile(atPath: executable.path)
          ? .success(executable) : .failure(.applicationUnavailable)
      }
      .flatMap { executable in
        Result {
          try FileManager.default.createDirectory(at: home, withIntermediateDirectories: true)
          try FileManager.default.createDirectory(
            at: workingDirectory, withIntermediateDirectories: true)
        }
        .mapError(CodexFailure.storage).map { _ in executable }
      }
      .bind { executable in
        await startServer(
          home: home,
          invocation: ProcessInvocation(
            executable: executable, arguments: ["app-server"], environment: variables,
            workingDirectory: workingDirectory))
      }
  }

  private func startServer(
    home: URL, invocation: ProcessInvocation
  ) async -> Result<CodexConnection, CodexFailure> {
    let ready = AsyncStream<Result<CodexConnection, CodexFailure>>.makeStream()
    let continuation: AsyncStream<CodexRateLimitsUpdate>.Continuation = updatesContinuation
    let task: Task<Void, Never> = Task(name: "codex app-server \(home.lastPathComponent)") {
      let outcome: Result<(Void, TerminationStatus), ProcessFailure> = await ProcessRun.stream(
        invocation, deadline: nil
      ) { execution in
        .success(
          await Self.serve(
            CodexConnection(execution: execution), home: home, updates: continuation,
            ready: ready.continuation))
      }
      if case .failure(let error) = outcome {
        ready.continuation.yield(.failure(CodexFailure(process: error)))
        ready.continuation.finish()
      }
    }
    var iterator: AsyncStream<Result<CodexConnection, CodexFailure>>.Iterator =
      ready.stream.makeAsyncIterator()
    let first: Result<CodexConnection, CodexFailure> =
      await iterator.next() ?? .failure(.connectionClosed)
    switch first {
    case .success(let connection):
      servers[home] = CodexServer(connection: connection, task: task)
    case .failure:
      task.cancel()
      await task.value
    }
    return first
  }

  private nonisolated static func serve(
    _ connection: CodexConnection, home: URL,
    updates: AsyncStream<CodexRateLimitsUpdate>.Continuation,
    ready: AsyncStream<Result<CodexConnection, CodexFailure>>.Continuation
  ) async {
    await withDiscardingTaskGroup { group in
      group.addTask(name: "codex read") { await connection.read() }
      group.addTask(name: "codex rate limits") {
        await forward(connection.updates, from: home, into: updates)
      }
      let initialized: Result<Void, CodexFailure> = await ProcessRun.withDeadline(
        requestDeadline, timedOut: .timedOut, cancelled: .cancelled
      ) { await connection.initialize() }
      ready.yield(initialized.map { _ in connection })
      ready.finish()
    }
  }

  private nonisolated static func forward(
    _ updates: AsyncStream<CodexProtocol.RateLimitsUpdated>, from home: URL,
    into continuation: AsyncStream<CodexRateLimitsUpdate>.Continuation
  ) async {
    for await update in updates {
      if case .success(let windows) = CodexProtocol.windows(update.rateLimits) {
        continuation.yield(CodexRateLimitsUpdate(home: home, windows: windows, observedAt: Date()))
      }
    }
  }

  private func stopServer(_ home: URL) async {
    guard let server: CodexServer = servers.removeValue(forKey: home) else { return }
    await server.connection.finish(.connectionClosed)
    server.task.cancel()
    await server.task.value
  }
}
