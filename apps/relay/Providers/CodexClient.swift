import Foundation

actor CodexClient {
  private struct Operation {
    let accountID: UUID
    let cancel: @Sendable () -> Void
    let completion: @Sendable () async -> Void
    var connection: CodexConnection?
  }

  private let paths: FileLocations
  private let environment: [String: String]
  private var operations: [UUID: Operation] = [:]

  init(paths: FileLocations, environment: [String: String]) {
    self.paths = paths
    self.environment = environment
  }

  func connect(id: UUID) async -> Result<AccountIdentity, CodexFailure> {
    return await withConnection(id: id) { connection in
      await Self.signIn(connection)
    }
  }

  func reconnect(account: Account) async -> Result<AccountIdentity, CodexFailure> {
    return await requireDesktopClosed(account.id, action: .reconnect).bind { _ in
      await withConnection(id: account.id) { connection in
        await Self.signIn(connection).flatMap { identity in
          identity.isSameAccount(as: account.identity)
            ? .success(identity) : .failure(.identityChanged)
        }
      }
    }
  }

  func usage(for account: Account) async -> Result<UsageSnapshot, CodexFailure> {
    return await withConnection(id: account.id) { connection in
      await CodexProtocol.identity(connection, matching: account).bind { identity in
        await CodexProtocol.usage(connection, identity: identity).map { usage in usage.snapshot }
      }
    }
  }

  func startSession(for account: Account) async -> Result<UsageSnapshot, CodexFailure> {
    let workingDirectory: URL = paths.workingDirectory
    return await withConnection(id: account.id) { connection in
      await CodexProtocol.identity(connection, matching: account).bind { identity in
        await Self.startSession(connection, identity: identity, workingDirectory: workingDirectory)
      }
    }
  }

  func select(_ account: Account) async -> Result<AccountSelection, CodexFailure> {
    return await withConnection(id: account.id) { connection in
      await CodexProtocol.identity(connection, matching: account)
    }
    .bind { _ in await openDesktop(accountID: account.id) }
    .bind { instance in
      await withConnection(id: account.id) { connection in
        await CodexProtocol.identity(connection, matching: account)
      }
      .bind { identity in
        guard await CodexDesktop.isRunning(instance) else { return .failure(.desktopUnavailable) }
        return Task.isCancelled
          ? .failure(.cancelled)
          : saveSelectedAccount(account.id).map { _ in
            AccountSelection(identity: identity, preservedAccount: nil)
          }
      }
    }
  }

  func selectedAccount(in accounts: [Account]) async -> Result<UUID?, CodexFailure> {
    return await desktopState().bind { state in
      let frontmost: UUID? = await CodexDesktop.frontmostAccount(in: state.instances)
      guard let selected: UUID = frontmost ?? state.selectedAccountID,
        let account: Account = accounts.first(where: { value in
          value.id == selected && value.provider == .openAI
        }),
        let instance: CodexDesktopInstance = state.instances.first(where: { value in
          value.accountID == selected
        })
      else {
        return .success(nil)
      }
      guard await CodexDesktop.isRunning(instance) else {
        return saveSelectedAccount(nil).map { _ in nil }
      }
      return await withConnection(id: selected) { connection in
        await CodexProtocol.identity(connection, matching: account)
      }
      .bind { _ in
        guard await CodexDesktop.isRunning(instance) else { return .success(nil) }
        return saveSelectedAccount(selected).map { _ in selected }
      }
    }
  }

  func signOut(_ account: Account) async -> Result<Void, CodexFailure> {
    return await requireDesktopClosed(account.id, action: .signOut).bind { _ in
      await logOut(account)
    }
  }

  func remove(_ account: Account) async -> Result<Void, CodexFailure> {
    return await requireDesktopClosed(account.id, action: .remove)
      .bind { _ in await logOut(account) }
      .flatMap { _ in deleteAccountDirectories(account.id) }
  }

  func discardConnection(id: UUID) async -> Result<Void, CodexFailure> {
    await cancel(accountID: id)
    return await requireDesktopClosed(id, action: .remove).bind { _ in
      return await
        (FileManager.default.fileExists(atPath: paths.codexHome(id).path)
        ? withConnection(id: id) { connection in
          await connection.request("account/logout").map { _ in () }
        }
        : .success(()))
        .flatMap { _ in deleteAccountDirectories(id) }
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
    for connection: CodexConnection in selected.values.compactMap({ operation in
      operation.connection
    }) {
      await connection.cancel()
    }
    for operation: Operation in selected.values { await operation.completion() }
  }

  private func withConnection<Value: Sendable>(
    id: UUID,
    body: @escaping @Sendable (CodexConnection) async -> Result<Value, CodexFailure>
  ) async -> Result<Value, CodexFailure> {
    let operationID: UUID = UUID()
    let run: Task<Result<Value, CodexFailure>, Never> = Task {
      await self.openConnection(accountID: id, operationID: operationID, body: body)
    }
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
      Task { await self.operations[operationID]?.connection?.cancel() }
    }
    operations.removeValue(forKey: operationID)
    return result
  }

  private func openConnection<Value: Sendable>(
    accountID: UUID,
    operationID: UUID,
    body: @escaping @Sendable (CodexConnection) async -> Result<Value, CodexFailure>
  ) async -> Result<Value, CodexFailure> {
    guard !Task.isCancelled else { return .failure(.cancelled) }
    let home: URL = paths.codexHome(accountID)
    let workingDirectory: URL = paths.workingDirectory
    let variables: [String: String] = Self.providerEnvironment(environment)
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
        await ChildProcess.launch(
          ProcessInvocation(
            executable: executable,
            arguments: ["app-server"],
            environment: variables,
            workingDirectory: workingDirectory
          )
        ).mapError(CodexFailure.process)
      }
      .bind { process in
        let connection: CodexConnection = CodexConnection(process: process)
        operations[operationID]?.connection = connection
        let result: Result<Value, CodexFailure> = await connection.initialize().bind { _ in
          await body(connection)
        }
        await connection.close()
        return result
      }
  }

  private static func startSession(
    _ connection: CodexConnection, identity: AccountIdentity, workingDirectory: URL
  ) async -> Result<UsageSnapshot, CodexFailure> {
    await CodexProtocol.usage(connection, identity: identity).bind { current in
      if case .running = current.snapshot.sessionState(at: current.snapshot.observedAt) {
        return .success(current.snapshot)
      }
      if current.permitsIncludedUsage == false { return .failure(.includedUsageBlocked) }
      return await CodexProtocol.greetingModel(connection).bind { model in
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

  private func logOut(_ account: Account) async -> Result<Void, CodexFailure> {
    return await withConnection(id: account.id) { connection in
      await CodexProtocol.identity(connection, matching: account)
        .map { _ in () }
        .flatMapError { error -> Result<Void, CodexFailure> in
          if case .signInRequired = error { .success(()) } else { .failure(error) }
        }
        .bind { _ in await connection.request("account/logout").map { _ in () } }
    }
    .flatMap { _ in clearSelectedAccount(account.id) }
  }

  private static func providerEnvironment(_ source: [String: String]) -> [String: String] {
    let excluded: Set<String> = [
      "OPENAI_API_KEY", "OPENAI_BASE_URL", "OPENAI_ORG_ID", "OPENAI_PROJECT_ID",
    ]
    return source.filter { key, _ in !key.hasPrefix("CODEX_") && !excluded.contains(key) }
  }

  private var desktopStateFile: URL {
    paths.applicationSupportDirectory.appending(path: "codex-desktop.json")
  }

  private func desktopState() -> Result<CodexDesktopState, CodexFailure> {
    return Result {
      try JSONDecoder().decode(CodexDesktopState.self, from: Data(contentsOf: desktopStateFile))
    }
    .flatMapError { error in
      (error as? CocoaError)?.code == .fileReadNoSuchFile
        ? .success(CodexDesktopState(instances: [], selectedAccountID: nil))
        : .failure(.storage(error))
    }
  }

  private func saveDesktopState(_ state: CodexDesktopState) -> Result<Void, CodexFailure> {
    return Result {
      try FileManager.default.createDirectory(
        at: paths.applicationSupportDirectory, withIntermediateDirectories: true)
      try JSONEncoder().encode(state).write(to: desktopStateFile, options: .atomic)
    }
    .mapError(CodexFailure.storage)
  }

  private func saveSelectedAccount(_ id: UUID?) -> Result<Void, CodexFailure> {
    return desktopState().flatMap { state in
      saveDesktopState(CodexDesktopState(instances: state.instances, selectedAccountID: id))
    }
  }

  private func clearSelectedAccount(_ id: UUID) -> Result<Void, CodexFailure> {
    return desktopState().flatMap { state in
      state.selectedAccountID == id
        ? saveDesktopState(
          CodexDesktopState(instances: state.instances, selectedAccountID: nil))
        : .success(())
    }
  }

  private func requireDesktopClosed(_ id: UUID, action: CodexAccountAction) async -> Result<
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
    return await desktopState().bind { state in
      await Result {
        try FileManager.default.createDirectory(
          at: paths.codexDesktopDirectory(accountID), withIntermediateDirectories: true)
      }
      .mapError(CodexFailure.storage)
      .bind { _ in
        await CodexDesktop.open(
          accountID: accountID,
          home: paths.codexHome(accountID),
          desktopDirectory: paths.codexDesktopDirectory(accountID),
          environment: Self.providerEnvironment(environment),
          existing: state.instances.first(where: { value in value.accountID == accountID })
        )
      }
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

  private func deleteAccountDirectories(_ id: UUID) -> Result<Void, CodexFailure> {
    return Result {
      for directory: URL in [paths.codexHome(id), paths.codexDesktopDirectory(id)]
      where FileManager.default.fileExists(atPath: directory.path) {
        try FileManager.default.removeItem(at: directory)
      }
    }
    .mapError(CodexFailure.storage)
    .flatMap { _ in desktopState() }
    .flatMap { state in
      saveDesktopState(
        CodexDesktopState(
          instances: state.instances.filter { value in value.accountID != id },
          selectedAccountID: state.selectedAccountID == id ? nil : state.selectedAccountID
        ))
    }
  }
}
