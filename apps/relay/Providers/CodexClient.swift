import Foundation
import Subprocess
import System

// --- [MODELS] --------------------------------------------------------------------------

nonisolated struct CodexRateLimitsUpdate: Sendable {
    let home: URL
    let windows: [QuotaWindow]
    let observedAt: Date
}

// --- [SERVICES] ------------------------------------------------------------------------

private struct CodexServer {
    let connection: CodexConnection
    let task: Task<Void, Never>
}

actor CodexClient: ProviderClient {
    // --- [STATE]
    nonisolated let liveHome: URL
    nonisolated let updates: AsyncStream<CodexRateLimitsUpdate>
    private static let requestDeadline: Duration = .seconds(60)
    private let paths: FileLocations
    private let environment: [String: String]
    private let updatesContinuation: AsyncStream<CodexRateLimitsUpdate>.Continuation
    private var servers: [URL: CodexServer] = [:]

    init(paths: FileLocations, environment: [String: String]) {
        self.paths = paths
        self.environment = environment
        liveHome =
            environment["CODEX_HOME"].flatMap { value in
                value.isEmpty ? nil : URL(filePath: value, directoryHint: .isDirectory)
            } ?? paths.home.appending(path: ".codex", directoryHint: .isDirectory)
        (updates, updatesContinuation) = AsyncStream<CodexRateLimitsUpdate>.makeStream()
    }

    nonisolated var liveAuthFile: URL { liveHome.appending(path: CodexAuthFile.name) }

    nonisolated func home(for account: Account, isSelected: Bool) -> URL {
        isSelected ? liveHome : paths.codexHome(account.id)
    }

    // --- [SELECTION]
    func currentSelection(known _: [Account]) -> Result<AccountIdentity?, ProviderError> {
        CodexAuthFile.read(at: liveAuthFile).mapError(ProviderError.init(failure:))
    }

    func holdsCredential(for account: Account) -> Result<Bool, ProviderError> {
        CodexAuthFile.read(at: paths.codexHome(account.id).appending(path: CodexAuthFile.name))
            .map { identity in identity?.isSameAccount(as: account.identity) ?? false }
            .mapError(ProviderError.init(failure:))
    }

    private func identity(at home: URL) -> Result<AccountIdentity, CodexFailure> {
        CodexAuthFile.read(at: home.appending(path: CodexAuthFile.name)).flatMap { identity in
            identity.map(Result.success) ?? .failure(.signInRequired)
        }
    }

    // --- [SWITCH]
    func preconditions() async -> Result<Void, CodexFailure> {
        await connection(for: liveHome).bind { connection in
            await ProcessRun.withDeadline(Self.requestDeadline) {
                await connection.request(
                    CodexProtocol.EffectiveConfig.self,
                    .configRead,
                    params: CodexProtocol.ConfigReadParams(includeLayers: false, cwd: nil),
                )
            }
        }
        .flatMap { response in
            switch (response.config.authCredentialsStore, response.config.forcedWorkspace) {
                case (.keyring, _), (.auto, _), (.ephemeral, _): .failure(.keyringStorage)
                case (_, .some): .failure(.forcedWorkspace)
                case (_, .none): .success(())
            }
        }
    }

    func select(
        _ account: Account,
        candidates: [Account],
    ) async -> Result<AccountIdentity, ProviderError> {
        let incoming: URL = paths.codexHome(account.id).appending(path: CodexAuthFile.name)
        return await preconditions().bind { _ in
            await CodexAuthFile.read(at: incoming).bind {
                saved -> Result<AccountIdentity, CodexFailure> in
                guard let saved, saved.isSameAccount(as: account.identity) else {
                    return .failure(.signInRequired)
                }
                await stopServer(liveHome)
                await stopServer(paths.codexHome(account.id))
                return await swapAuthFiles(account, candidates: candidates, incoming: incoming)
                    .bind { _ in await CodexDesktop.relaunchIfRunning() }
                    .map { _ in saved }
            }
        }
        .mapError(ProviderError.init(failure:))
    }

    private func swapAuthFiles(
        _ account: Account,
        candidates: [Account],
        incoming: URL,
    ) async -> Result<Void, CodexFailure> {
        await CodexAuthFile.read(at: liveAuthFile)
            .bind { live -> Result<Void, CodexFailure> in
                if let live, live.isSameAccount(as: account.identity) {
                    return remove(incoming)
                }
                let outgoing: URL? = live.flatMap { live in
                    candidates.first { candidate in
                        candidate.provider == .openAI && candidate.identity.isSameAccount(as: live)
                    }
                }
                .map { candidate in paths.codexHome(candidate.id) }
                if let outgoing { await stopServer(outgoing) }
                return
                    (outgoing.map { home in
                        installFile(from: liveAuthFile, to: home.appending(path: CodexAuthFile.name))
                    } ?? .success(()))
                    .flatMap { _ in installFile(from: incoming, to: liveAuthFile) }
                    .flatMap { _ in remove(incoming) }
            }
    }

    private func installFile(from source: URL, to destination: URL) -> Result<Void, CodexFailure> {
        Result {
            let data: Data = try Data(contentsOf: source)
            let directory: URL = destination.deletingLastPathComponent()
            try FileManager.default.createDirectory(at: directory, withIntermediateDirectories: true)
            let temporary: URL = directory.appending(path: ".\(destination.lastPathComponent).\(UUID().uuidString)")
            guard
                FileManager.default.createFile(
                    atPath: temporary.path,
                    contents: data,
                    attributes: [.posixPermissions: 0o600],
                )
            else { throw CocoaError(.fileWriteUnknown) }
            _ = try FileManager.default.replaceItemAt(
                destination,
                withItemAt: temporary,
                options: .usingNewMetadataOnly,
            )
        }
        .mapError(CodexFailure.storage)
    }

    private func remove(_ url: URL) -> Result<Void, CodexFailure> {
        ifPresent { try FileManager.default.removeItem(at: url) }.map { _ in () }
            .mapError(CodexFailure.storage)
    }

    // --- [SIGN_IN]
    func connect(id: UUID, codes _: AsyncStream<String>) async -> Result<AccountIdentity, ProviderError> {
        let home: URL = paths.codexHome(id)
        return await signIn(home: home).bind { _ in identity(at: home) }.mapError(ProviderError.init(failure:))
    }

    func reconnect(account: Account, isSelected: Bool, codes _: AsyncStream<String>) async -> Result<AccountIdentity, ProviderError> {
        let home: URL = home(for: account, isSelected: isSelected)
        return await signIn(home: home).bind { _ in identity(at: home) }
            .bind { identity in
                if identity.isSameAccount(as: account.identity) { .success(identity) } else { await logOut(home: home).flatMap { _ in .failure(.identityChanged) } }
            }
            .mapError(ProviderError.init(failure:))
    }

    func signOut(_ account: Account, isSelected: Bool) async -> Result<Void, ProviderError> {
        await logOut(home: home(for: account, isSelected: isSelected)).mapError(ProviderError.init(failure:))
    }

    func remove(_ account: Account, isSelected: Bool) async -> Result<Void, ProviderError> {
        await signOut(account, isSelected: isSelected)
            .flatMapError { error in error.failure.requiresSignIn ? .success(()) : .failure(error) }
            .bind { _ in await deletePrivateStore(id: account.id) }
    }

    func deletePrivateStore(id: UUID) async -> Result<Void, ProviderError> {
        let home: URL = paths.codexHome(id)
        await stopServer(home)
        return remove(home).mapError(ProviderError.init(failure:))
    }

    private func signIn(home: URL) async -> Result<Void, CodexFailure> {
        await connection(for: home).bind { connection in
            await ProcessRun.withDeadline(.seconds(300)) {
                await connection.request(
                    CodexProtocol.LoginStarted.self,
                    .accountLoginStart,
                    params: CodexProtocol.LoginStartParams(type: .chatgpt),
                )
                .bind { started in await Self.awaitLogin(started, on: connection) }
                .flatMap { completed in
                    completed.success ? .success(()) : .failure(.signInRefused(reason: completed.error))
                }
            }
        }
    }

    private static func awaitLogin(
        _ started: CodexProtocol.LoginStarted,
        on connection: CodexConnection,
    ) async -> Result<CodexProtocol.LoginCompleted, CodexFailure> {
        if let loginID: String = started.loginId, let address: String = started.authUrl,
            let url: URL = URL(string: address), url.scheme == "https", url.host(percentEncoded: false) != nil
        {
            await CodexDesktop.openSignIn(url).bind { _ in
                await connection.notification(CodexProtocol.LoginCompleted.self, .accountLoginCompleted) {
                    completed in completed.loginId == loginID
                }
            }
        } else {
            .failure(.invalidResponse(field: "sign-in response"))
        }
    }

    private func logOut(home: URL) async -> Result<Void, CodexFailure> {
        defer { await stopServer(home) }
        return await connection(for: home).bind { connection in
            await ProcessRun.withDeadline(Self.requestDeadline) {
                await connection.request(.accountLogout).map { _ in () }
            }
        }
    }

    // --- [USAGE]
    func usage(for account: Account, isSelected: Bool) async -> Result<AccountUsage, ProviderError> {
        let home: URL = home(for: account, isSelected: isSelected)
        return await identity(at: home).flatMap { identity in
            identity.isSameAccount(as: account.identity) ? .success(identity) : .failure(.identityChanged)
        }
        .bind { identity in
            await connection(for: home).bind { connection in
                await ProcessRun.withDeadline(Self.requestDeadline) {
                    await connection.request(
                        CodexProtocol.AccountRateLimits.self,
                        .accountRateLimitsRead,
                        params: CodexProtocol.RateLimitsReadParams(excludeResetCreditDetails: true),
                    )
                }
            }
            .flatMap { response in CodexProtocol.usage(response, identity: identity, observedAt: Date()) }
        }
        .mapError(ProviderError.init(failure:))
    }

    func startSession(
        for account: Account,
        isSelected: Bool,
    ) async -> Result<AccountUsage, ProviderError> {
        let home: URL = home(for: account, isSelected: isSelected)
        return await usage(for: account, isSelected: isSelected).bind { current in
            if case .ready = current.availability(at: Date()) {
                await greet(home: home).mapError(ProviderError.init(failure:)).bind { _ in await usage(for: account, isSelected: isSelected) }
            } else {
                .success(current)
            }
        }
    }

    private func greet(home: URL) async -> Result<Void, CodexFailure> {
        let workingDirectory: URL = paths.workingDirectory
        return await connection(for: home).bind { connection in
            await ProcessRun.withDeadline(.seconds(120)) {
                await CodexProtocol.greetingEffort(connection).bind { effort in
                    await CodexProtocol.sendGreeting(connection, effort: effort, workingDirectory: workingDirectory)
                }
            }
        }
    }

    // --- [SERVERS]
    func shutdown() async {
        for home: URL in Array(servers.keys) { await stopServer(home) }
        updatesContinuation.finish()
    }

    private func connection(for home: URL) async -> Result<CodexConnection, CodexFailure> {
        if let server: CodexServer = servers[home], await !server.connection.isFinished {
            return .success(server.connection)
        }
        servers.removeValue(forKey: home)
        let workingDirectory: URL = paths.workingDirectory
        let excludedEnvironmentVariables: Set<String> = ["OPENAI_API_KEY", "OPENAI_BASE_URL", "OPENAI_ORG_ID", "OPENAI_PROJECT_ID"]
        let variables: [String: String] =
            environment
            .filter { key, _ in
                !key.hasPrefix("CODEX_") && !excludedEnvironmentVariables.contains(key)
            }
            .merging(["CODEX_HOME": home.path]) { _, override in override }
        return await CodexDesktop.serverExecutable()
            .flatMap { executable -> Result<URL, CodexFailure> in
                FileManager.default.isExecutableFile(atPath: executable.path)
                    ? .success(executable) : .failure(.applicationUnavailable)
            }
            .flatMap { executable in
                Result {
                    try FileManager.default.createDirectory(at: home, withIntermediateDirectories: true)
                    try FileManager.default.createDirectory(
                        at: workingDirectory,
                        withIntermediateDirectories: true,
                    )
                }
                .mapError(CodexFailure.storage).map { _ in executable }
            }
            .bind { executable in
                await startServer(
                    home: home,
                    invocation: ProcessRun.configuration(
                        executable: .path(FilePath(executable.path)),
                        arguments: ["app-server"],
                        environment: variables,
                        workingDirectory: workingDirectory,
                    ),
                )
            }
    }

    private func startServer(
        home: URL,
        invocation: Configuration,
    ) async -> Result<CodexConnection, CodexFailure> {
        let ready:
            (
                stream: AsyncStream<Result<CodexConnection, CodexFailure>>,
                continuation: AsyncStream<Result<CodexConnection, CodexFailure>>.Continuation
            ) = AsyncStream.makeStream()
        let continuation: AsyncStream<CodexRateLimitsUpdate>.Continuation = updatesContinuation
        let task: Task<Void, Never> = Task(name: "codex app-server \(home.lastPathComponent)") {
            let outcome: Result<(Void, TerminationStatus), ProcessFailure> = await ProcessRun.stream(
                invocation,
                deadline: nil,
            ) { execution in
                .success(
                    await Self.serve(
                        CodexConnection(execution: execution),
                        home: home,
                        updates: continuation,
                        ready: ready.continuation,
                    )
                )
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
        _ connection: CodexConnection,
        home: URL,
        updates: AsyncStream<CodexRateLimitsUpdate>.Continuation,
        ready: AsyncStream<Result<CodexConnection, CodexFailure>>.Continuation,
    ) async {
        await withDiscardingTaskGroup { group in
            group.addTask(name: "codex read") { await connection.read() }
            group.addTask(name: "codex rate limits") {
                await forward(connection.updates, from: home, into: updates)
            }
            let initialized: Result<Void, CodexFailure> = await ProcessRun.withDeadline(requestDeadline) {
                await connection.initialize()
            }
            ready.yield(initialized.map { _ in connection })
            ready.finish()
        }
    }

    private nonisolated static func forward(
        _ updates: AsyncStream<CodexProtocol.RateLimitsUpdated>,
        from home: URL,
        into continuation: AsyncStream<CodexRateLimitsUpdate>.Continuation,
    ) async {
        for await update: CodexProtocol.RateLimitsUpdated in updates {
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
