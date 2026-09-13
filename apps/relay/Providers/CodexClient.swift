import Foundation

actor CodexClient {
  // One app-server run per account operation, its task cancellable and awaitable by account.
  private struct Operation {
    let accountID: UUID
    let cancel: @Sendable () -> Void
    let completion: @Sendable () async -> Void
    var connection: CodexConnection?
  }

  private let paths: RelayPaths
  private let environment: [String: String]
  private var operations: [UUID: Operation] = [:]

  init(paths: RelayPaths, environment: [String: String]) {
    self.paths = paths
    self.environment = environment
  }

  func connect(id: UUID) async -> Result<AccountIdentity, CodexFailure> {
    return await withConnection(id: id) { connection in
      await Self.signIn(connection)
    }
  }

  func reconnect(account: RelayAccount) async -> Result<AccountIdentity, CodexFailure> {
    return await requireClosed(account.id, action: .reconnect).bind { _ in
      await withConnection(id: account.id) { connection in
        await Self.signIn(connection).flatMap { identity in
          identity.identifies(account.identity)
            ? .success(identity) : .failure(.identityChanged)
        }
      }
    }
  }

  func usage(for account: RelayAccount) async -> Result<UsageSnapshot, CodexFailure> {
    return await withConnection(id: account.id) { connection in
      await CodexProtocol.identity(connection, matching: account).bind { identity in
        await CodexProtocol.usage(connection, identity: identity).map { usage in usage.snapshot }
      }
    }
  }

  func startSession(for account: RelayAccount) async -> Result<UsageSnapshot, CodexFailure> {
    let workingDirectory: URL = paths.workingDirectory
    return await withConnection(id: account.id) { connection in
      await CodexProtocol.identity(connection, matching: account).bind { identity in
        await CodexProtocol.usage(connection, identity: identity).bind { current in
          if case .running = current.snapshot.sessionState(at: current.snapshot.observedAt) {
            return .success(current.snapshot)
          }
          if current.permitsIncludedUsage == false { return .failure(.includedUsageBlocked) }
          return await CodexProtocol.starterModel(connection).bind { model in
            await CodexProtocol.sendGreeting(
              connection, model: model, workingDirectory: workingDirectory)
          }
          .bind { _ in
            await CodexProtocol.usage(connection, identity: identity)
              .mapError(CodexFailure.sessionConfirmationPending)
              .map { usage in usage.snapshot }
          }
        }
      }
    }
  }

  func select(_ account: RelayAccount) async -> Result<AccountSelection, CodexFailure> {
    return await withConnection(id: account.id) { connection in
      await CodexProtocol.identity(connection, matching: account)
    }
    .bind { _ in await openDesktop(accountID: account.id) }
    .bind { instance in
      // A fresh native reader observes the identity the launched desktop persisted.
      await withConnection(id: account.id) { connection in
        await CodexProtocol.identity(connection, matching: account)
      }
      .bind { identity in
        guard await CodexDesktop.isRunning(instance) else { return .failure(.desktopUnavailable) }
        guard !Task.isCancelled else { return .failure(.cancelled) }
        return markSelected(account.id).map { _ in
          AccountSelection(identity: identity, preservedAccount: nil)
        }
      }
    }
  }

  func selectedAccount(in accounts: [RelayAccount]) async -> Result<UUID?, CodexFailure> {
    return await desktopState().bind { state in
      let foreground: UUID? = await CodexDesktop.foregroundAccount(in: state.instances)
      guard let selected: UUID = foreground ?? state.selectedAccountID,
        let account: RelayAccount = accounts.first(where: { value in
          value.id == selected && value.provider == .openAI
        }),
        let instance: CodexDesktopInstance = state.instances.first(where: { value in
          value.accountID == selected
        })
      else {
        return .success(nil)
      }
      guard await CodexDesktop.isRunning(instance) else {
        return markSelected(nil).map { _ in nil }
      }
      return await withConnection(id: selected) { connection in
        await CodexProtocol.identity(connection, matching: account)
      }
      .bind { _ in
        guard await CodexDesktop.isRunning(instance) else { return .success(nil) }
        return markSelected(selected).map { _ in selected }
      }
    }
  }

  func signOut(_ account: RelayAccount) async -> Result<Void, CodexFailure> {
    return await requireClosed(account.id, action: .signOut).bind { _ in
      await logOut(account)
    }
  }

  func remove(_ account: RelayAccount) async -> Result<Void, CodexFailure> {
    return await requireClosed(account.id, action: .remove)
      .bind { _ in await logOut(account) }
      .flatMap { _ in deleteProfile(account.id) }
  }

  func discardConnection(id: UUID) async -> Result<Void, CodexFailure> {
    await cancel(accountID: id)
    return await requireClosed(id, action: .remove).bind { _ in
      guard FileManager.default.fileExists(atPath: paths.codexHome(id).path) else {
        return deleteProfile(id)
      }
      return await withConnection(id: id) { connection in
        await connection.request("account/logout").map { _ in () }
      }
      .flatMap { _ in deleteProfile(id) }
    }
  }

  func cancel(accountID: UUID) async {
    await cancel(operations.filter { _, operation in operation.accountID == accountID })
  }

  func cancelOperations() async {
    await cancel(operations)
  }

  private func cancel(_ selected: [UUID: Operation]) async {
    for operation: Operation in selected.values { operation.cancel() }
    for operation: Operation in selected.values {
      if let connection: CodexConnection = operation.connection { await connection.cancel() }
    }
    for operation: Operation in selected.values { await operation.completion() }
  }

  private func withConnection<Value: Sendable>(
    id: UUID,
    body: @escaping @Sendable (CodexConnection) async -> Result<Value, CodexFailure>
  ) async -> Result<Value, CodexFailure> {
    let operationID: UUID = UUID()
    let run: Task<Result<Value, CodexFailure>, Never> = Task {
      await self.performConnection(accountID: id, operationID: operationID, body: body)
    }
    // The task body starts after this method suspends, so the record precedes its first read.
    operations[operationID] = Operation(
      accountID: id,
      cancel: { run.cancel() },
      completion: { _ = await run.value },
      connection: nil
    )
    let result: Result<Value, CodexFailure> = await withTaskCancellationHandler {
      await run.value
    } onCancel: {
      run.cancel()
      Task { await self.cancelOperation(operationID) }
    }
    operations.removeValue(forKey: operationID)
    return result
  }

  private func performConnection<Value: Sendable>(
    accountID: UUID,
    operationID: UUID,
    body: @escaping @Sendable (CodexConnection) async -> Result<Value, CodexFailure>
  ) async -> Result<Value, CodexFailure> {
    guard !Task.isCancelled else { return .failure(.cancelled) }
    let executable: URL
    switch await CodexDesktop.applicationURL() {
    case .success(let application):
      executable = application.appending(path: "Contents/Resources/codex")
    case .failure(let error): return .failure(error)
    }
    guard FileManager.default.isExecutableFile(atPath: executable.path) else {
      return .failure(.applicationUnavailable)
    }
    let home: URL = paths.codexHome(accountID)
    do {
      try FileManager.default.createDirectory(at: home, withIntermediateDirectories: true)
      try FileManager.default.createDirectory(
        at: paths.workingDirectory, withIntermediateDirectories: true)
    } catch {
      return .failure(.storage(error))
    }
    guard !Task.isCancelled else { return .failure(.cancelled) }
    var variables: [String: String] = Self.providerEnvironment(environment)
    variables["CODEX_HOME"] = home.path
    let process: NativeProcess
    switch await NativeProcess.launch(
      ProcessInvocation(
        executable: executable,
        arguments: ["app-server"],
        environment: variables,
        workingDirectory: paths.workingDirectory
      ))
    {
    case .success(let value): process = value
    case .failure(let error): return .failure(.process(error))
    }
    let connection: CodexConnection = CodexConnection(process: process)
    operations[operationID]?.connection = connection
    guard !Task.isCancelled else {
      await connection.cancel()
      return .failure(.cancelled)
    }
    let result: Result<Value, CodexFailure> = await connection.initialize().bind { _ in
      await body(connection)
    }
    await connection.close()
    return result
  }

  private func cancelOperation(_ id: UUID) async {
    if let connection: CodexConnection = operations[id]?.connection { await connection.cancel() }
  }

  private static func signIn(_ connection: CodexConnection) async -> Result<
    AccountIdentity, CodexFailure
  > {
    return await connection.request(
      "account/login/start", params: .object(["type": .string("chatgpt")])
    )
    .bind { (response: JSONValue) async -> Result<JSONValue, CodexFailure> in
      guard let loginID: String = response["loginId"]?.stringValue,
        let address: String = response["authUrl"]?.stringValue,
        let url: URL = URL(string: address), url.scheme == "https", url.host != nil
      else {
        return .failure(.invalidResponse(field: "sign-in response"))
      }
      return await CodexDesktop.openSignIn(url).bind { _ in
        await connection.notification("account/login/completed") { value in
          value["loginId"]?.stringValue == loginID
        }
      }
    }
    .bind { (completed: JSONValue) async -> Result<AccountIdentity, CodexFailure> in
      switch completed["success"] {
      case .some(.bool(true)): return await CodexProtocol.identity(connection)
      case .some(.bool(false)):
        return .failure(.signInRefused(reason: completed["error"]?.stringValue))
      case .none, .some: return .failure(.invalidResponse(field: "sign-in completion"))
      }
    }
  }

  private func logOut(_ account: RelayAccount) async -> Result<Void, CodexFailure> {
    return await withConnection(id: account.id) {
      (connection: CodexConnection) async -> Result<Void, CodexFailure> in
      let identity: Result<AccountIdentity, CodexFailure> = await CodexProtocol.identity(
        connection, matching: account)
      switch identity {
      case .success, .failure(.signInRequired): break
      case .failure(let error): return .failure(error)
      }
      return await connection.request("account/logout").map { _ in () }
    }
    .flatMap { _ in clearSelection(account.id) }
  }

  // The app-server and the desktop read the account's home from the environment Relay sets alone.
  private static func providerEnvironment(_ source: [String: String]) -> [String: String] {
    let excluded: Set<String> = [
      "OPENAI_API_KEY", "OPENAI_BASE_URL", "OPENAI_ORG_ID", "OPENAI_PROJECT_ID",
    ]
    return source.filter { key, _ in !key.hasPrefix("CODEX_") && !excluded.contains(key) }
  }

  private var desktopStateFile: URL {
    paths.applicationSupport.appending(path: "codex-desktop.json")
  }

  private func desktopState() -> Result<CodexDesktopState, CodexFailure> {
    do {
      let data: Data = try Data(contentsOf: desktopStateFile)
      return .success(try JSONDecoder().decode(CodexDesktopState.self, from: data))
    } catch let error as CocoaError where error.code == .fileReadNoSuchFile {
      return .success(CodexDesktopState(instances: [], selectedAccountID: nil))
    } catch {
      return .failure(.storage(error))
    }
  }

  private func saveDesktopState(_ state: CodexDesktopState) -> Result<Void, CodexFailure> {
    do {
      try FileManager.default.createDirectory(
        at: paths.applicationSupport, withIntermediateDirectories: true)
      try JSONEncoder().encode(state).write(to: desktopStateFile, options: .atomic)
      return .success(())
    } catch {
      return .failure(.storage(error))
    }
  }

  private func markSelected(_ id: UUID?) -> Result<Void, CodexFailure> {
    return desktopState().flatMap { state in
      saveDesktopState(CodexDesktopState(instances: state.instances, selectedAccountID: id))
    }
  }

  private func clearSelection(_ id: UUID) -> Result<Void, CodexFailure> {
    return desktopState().flatMap { state in
      guard state.selectedAccountID == id else { return .success(()) }
      return saveDesktopState(CodexDesktopState(instances: state.instances, selectedAccountID: nil))
    }
  }

  private func requireClosed(_ id: UUID, action: CodexAccountAction) async -> Result<
    Void, CodexFailure
  > {
    return await desktopState().bind { state in
      if let instance: CodexDesktopInstance = state.instances.first(where: { value in
        value.accountID == id
      }),
        await CodexDesktop.isRunning(instance)
      {
        return .failure(.desktopMustClose(action))
      }
      return .success(())
    }
  }

  private func openDesktop(accountID: UUID) async -> Result<CodexDesktopInstance, CodexFailure> {
    return await desktopState().bind {
      (state: CodexDesktopState) async -> Result<CodexDesktopInstance, CodexFailure> in
      do {
        try FileManager.default.createDirectory(
          at: paths.codexDesktopDirectory(accountID), withIntermediateDirectories: true)
      } catch {
        return .failure(.storage(error))
      }
      return await CodexDesktop.open(
        accountID: accountID,
        home: paths.codexHome(accountID),
        desktopDirectory: paths.codexDesktopDirectory(accountID),
        environment: Self.providerEnvironment(environment),
        existing: state.instances.first(where: { value in value.accountID == accountID })
      )
    }
    .flatMap { instance in
      desktopState().flatMap { latest in
        let instances: [CodexDesktopInstance] =
          latest.instances.filter { value in value.accountID != accountID } + [instance]
        return saveDesktopState(
          CodexDesktopState(instances: instances, selectedAccountID: latest.selectedAccountID)
        ).map { _ in instance }
      }
    }
  }

  private func deleteProfile(_ id: UUID) -> Result<Void, CodexFailure> {
    do {
      for directory: URL in [paths.codexHome(id), paths.codexDesktopDirectory(id)]
      where FileManager.default.fileExists(atPath: directory.path) {
        try FileManager.default.removeItem(at: directory)
      }
    } catch {
      return .failure(.storage(error))
    }
    return desktopState().flatMap { state in
      saveDesktopState(
        CodexDesktopState(
          instances: state.instances.filter { value in value.accountID != id },
          selectedAccountID: state.selectedAccountID == id ? nil : state.selectedAccountID
        ))
    }
  }
}
