import Files
import Foundation
import Processes
import Results
import Subprocess
import System

// --- [SERVICES] ------------------------------------------------------------------------

private struct CodexServer {
    let connection: CodexConnection
    let task: Task<Void, Never>
    let identity: AccountIdentity?
}

actor CodexClient: ProviderClient {
    // --- [STATE]
    nonisolated let liveHome: URL
    nonisolated let updates: AsyncStream<URL>
    private let paths: FileLocations
    private let environment: [Environment.Key: String]
    private let updatesContinuation: AsyncStream<URL>.Continuation
    private var servers: [URL: CodexServer] = [:]

    init(paths: FileLocations, environment: [Environment.Key: String]) {
        self.paths = paths
        self.environment = environment
        liveHome =
            environment["CODEX_HOME"].flatMap { value in
                value.isEmpty ? nil : URL(filePath: value, directoryHint: .isDirectory)
            } ?? paths.home.appending(path: ".codex", directoryHint: .isDirectory)
        (updates, updatesContinuation) = AsyncStream<URL>.makeStream()
    }

    nonisolated var liveAuthFile: URL { liveHome.appending(path: CodexAuthFile.name) }

    nonisolated func home(for account: Account, isSelected: Bool) -> URL {
        isSelected ? liveHome : paths.codexHome(account.id)
    }

    // --- [SELECTION]
    func currentSelection(selected _: Account?) -> Result<AccountIdentity?, ProviderError> {
        Result { () throws(CodexFailure) in try CodexAuthFile.read(at: liveAuthFile)?.identity }.mapError(ProviderError.init(failure:))
    }

    func holdsCredential(for account: Account) -> Result<Bool, ProviderError> {
        Result { () throws(CodexFailure) in
            try CodexAuthFile.read(at: paths.codexHome(account.id).appending(path: CodexAuthFile.name))?.identity?.isSameAccount(as: account.identity) ?? false
        }
        .mapError(ProviderError.init(failure:))
    }

    private func identity(at home: URL) throws(CodexFailure) -> AccountIdentity {
        guard let identity: AccountIdentity = try CodexAuthFile.read(at: home.appending(path: CodexAuthFile.name))?.identity else { throw .signInRequired }
        return identity
    }

    // --- [SWITCH]
    func preconditions() async throws(CodexFailure) {
        let connection: CodexConnection = try await connection(for: liveHome)
        let response: CodexProtocol.EffectiveConfig = try await connection.request(
            CodexProtocol.EffectiveConfig.self,
            .configRead,
            params: CodexProtocol.ConfigReadParams(includeLayers: false, cwd: nil),
        )
        switch (response.config.cliAuthCredentialsStore, response.config.forcedChatgptWorkspaceId) {
            case (.keyring, _), (.auto, _), (.ephemeral, _): throw .keyringStorage
            case (_, .some): throw .forcedWorkspace
            case (_, .none): return
        }
    }

    func select(
        _ account: Account,
        outgoing: Account?,
    ) async -> Result<AccountIdentity, ProviderError> {
        func installFile(_ data: Data, to destination: URL) throws(CodexFailure) {
            try Result {
                let file: FileWrapper = FileWrapper(regularFileWithContents: data)
                file.fileAttributes = [FileAttributeKey.posixPermissions.rawValue: 0o600]
                try FileManager.default.createDirectory(at: destination.deletingLastPathComponent(), withIntermediateDirectories: true)
                try file.write(to: destination, options: .atomic, originalContentsURL: nil)
            }
            .mapError(CodexFailure.storage)
            .get()
        }
        let incoming: URL = paths.codexHome(account.id).appending(path: CodexAuthFile.name)
        return await Result { () async throws(CodexFailure) in
            try await CodexDesktop.refuseWhileRunning()
            try await preconditions()
            await stopServer(liveHome)
            await stopServer(paths.codexHome(account.id))
            if let outgoing { await stopServer(paths.codexHome(outgoing.id)) }
            guard let saved: (data: Data, identity: AccountIdentity?) = try CodexAuthFile.read(at: incoming), let identity: AccountIdentity = saved.identity,
                identity.isSameAccount(as: account.identity)
            else { throw .signInRequired }
            let live: (data: Data, identity: AccountIdentity?)? = try CodexAuthFile.read(at: liveAuthFile)
            if live?.identity?.isSameAccount(as: account.identity) != true {
                let matchesOutgoing: Bool =
                    switch (live?.identity, outgoing) {
                        case (.none, .none): true
                        case (.some(let identity), .some(let outgoing)): identity.isSameAccount(as: outgoing.identity)
                        case (.none, .some), (.some, .none): false
                    }
                guard matchesOutgoing else { throw .identityChanged }
                if let outgoing, let live { try installFile(live.data, to: paths.codexHome(outgoing.id).appending(path: CodexAuthFile.name)) }
                try installFile(saved.data, to: liveAuthFile)
            }
            try remove(incoming)
            return identity
        }
        .mapError(ProviderError.init(failure:))
    }

    private func remove(_ url: URL) throws(CodexFailure) {
        _ = try ifPresent { try FileManager.default.removeItem(at: url) }.mapError(CodexFailure.storage).get()
    }

    // --- [SIGN_IN]
    func signIn(_ target: SignInTarget, codes _: AsyncStream<String>) async -> Result<AccountIdentity, ProviderError> {
        let (home, expected): (URL, AccountIdentity?) =
            switch target {
                case .newAccount(let id): (paths.codexHome(id), nil)
                case .account(let account, let isSelected): (home(for: account, isSelected: isSelected), account.identity)
            }
        return await Result { () async throws(CodexFailure) in
            let connection: CodexConnection = try await connection(for: home)
            try await withTimeout(.seconds(300)) {
                await Result { () async throws(CodexFailure) in
                    let started: CodexProtocol.LoginStarted = try await connection.request(CodexProtocol.LoginStarted.self, .accountLoginStart, params: CodexProtocol.LoginStartParams())
                    guard let loginID: String = started.loginId, let address: String = started.authUrl, let url: URL = URL(string: address), url.scheme == "https",
                        url.host(percentEncoded: false) != nil
                    else { throw .invalidResponse(.accountLoginStart, nil) }
                    try await CodexDesktop.openSignIn(url)
                    let completed: CodexProtocol.LoginCompleted = try await connection.notification(CodexProtocol.LoginCompleted.self, .accountLoginCompleted) { completed in
                        completed.loginId == loginID
                    }
                    guard completed.success else { throw .signInRefused(reason: completed.error) }
                }
            }
            .get()
            let identity: AccountIdentity = try identity(at: home)
            guard expected.map(identity.isSameAccount(as:)) ?? true else {
                try await logOut(home: home)
                throw .identityChanged
            }
            return identity
        }
        .mapError(ProviderError.init(failure:))
    }

    func signOut(_ account: Account, isSelected: Bool) async -> Result<Void, ProviderError> {
        await Result { () async throws(CodexFailure) in try await logOut(home: home(for: account, isSelected: isSelected)) }.mapError(ProviderError.init(failure:))
    }

    func remove(_ account: Account, isSelected: Bool) async -> Result<Void, ProviderError> {
        await Result { () async throws(ProviderError) in
            try await signOut(account, isSelected: isSelected).flatMapError { error in error.failure.requiresSignIn ? .success(()) : .failure(error) }.get()
            try await deletePrivateStore(id: account.id).get()
        }
    }

    func deletePrivateStore(id: UUID) async -> Result<Void, ProviderError> {
        let home: URL = paths.codexHome(id)
        await stopServer(home)
        return Result { () throws(CodexFailure) in try remove(home) }.mapError(ProviderError.init(failure:))
    }

    private func logOut(home: URL) async throws(CodexFailure) {
        defer { await stopServer(home) }
        let connection: CodexConnection = try await connection(for: home)
        try await connection.request(.accountLogout)
    }

    // --- [USAGE]
    func usage(for account: Account, isSelected: Bool) async -> Result<AccountUsage, ProviderError> {
        await Result { () async throws(CodexFailure) in
            let home: URL = home(for: account, isSelected: isSelected)
            let identity: AccountIdentity = try identity(at: home)
            guard identity.isSameAccount(as: account.identity) else { throw .identityChanged }
            let connection: CodexConnection = try await connection(for: home)
            return try await connection.request(CodexProtocol.AccountRateLimits.self, .accountRateLimitsRead, params: CodexProtocol.RateLimitsReadParams(excludeResetCreditDetails: true))
                .usage(identity: identity, observedAt: Date())
        }
        .mapError(ProviderError.init(failure:))
    }

    func startSession(
        for account: Account,
        isSelected: Bool,
    ) async -> Result<AccountUsage, ProviderError> {
        let home: URL = home(for: account, isSelected: isSelected)
        let workingDirectory: URL = paths.workingDirectory
        return await Result { () async throws(ProviderError) in
            let current: AccountUsage = try await usage(for: account, isSelected: isSelected).get()
            guard case .ready = current.availability(at: Date()) else { return current }
            try await Result { () async throws(CodexFailure) in
                let connection: CodexConnection = try await connection(for: home)
                try await withTimeout(.seconds(120)) {
                    await Result { () async throws(CodexFailure) in
                        try await CodexProtocol.greet(connection, workingDirectory: workingDirectory)
                    }
                }
                .get()
            }
            .mapError(ProviderError.init(failure:))
            .get()
            return try await usage(for: account, isSelected: isSelected).get()
        }
    }

    // --- [SERVERS]
    func shutdown() async {
        for home: URL in Array(servers.keys) { await stopServer(home) }
        updatesContinuation.finish()
    }

    private func connection(for home: URL) async throws(CodexFailure) -> CodexConnection {
        let identity: AccountIdentity? = try CodexAuthFile.read(at: home.appending(path: CodexAuthFile.name))?.identity
        if let server: CodexServer = servers[home], server.identity == identity, !(await server.connection.isFinished) {
            return server.connection
        }
        await stopServer(home)
        let workingDirectory: URL = paths.workingDirectory
        let excludedEnvironmentVariables: Set<Environment.Key> = ["OPENAI_API_KEY", "OPENAI_BASE_URL", "OPENAI_ORG_ID", "OPENAI_PROJECT_ID"]
        let executable: URL = try await CodexDesktop.serverExecutable()
        try Result {
            try FileManager.default.createDirectory(at: home, withIntermediateDirectories: true)
            try FileManager.default.createDirectory(at: workingDirectory, withIntermediateDirectories: true)
        }
        .mapError(CodexFailure.storage)
        .get()
        let invocation: Configuration = Configuration(
            executable: .path(FilePath(executable.path(percentEncoded: false))),
            arguments: ["app-server"],
            environment: .custom(environment.filter { key, _ in !key.rawValue.hasPrefix("CODEX_") && !excludedEnvironmentVariables.contains(key) })
                .updating(["CODEX_HOME": FilePath(home.path(percentEncoded: false)).string]),
            workingDirectory: FilePath(workingDirectory.path(percentEncoded: false)),
            platformOptions: .session,
        )
        let ready:
            (
                stream: AsyncStream<Result<CodexConnection, CodexFailure>>,
                continuation: AsyncStream<Result<CodexConnection, CodexFailure>>.Continuation
            ) = AsyncStream.makeStream()
        let continuation: AsyncStream<URL>.Continuation = updatesContinuation
        let task: Task<Void, Never> = Task(name: "codex app-server \(home.lastPathComponent)") {
            let outcome: Result<ExecutionResult<Void, SequenceOutput, DiscardedOutput>, ProcessFailure> = await Result {
                try await Subprocess.run(invocation, input: .inputWriter, output: .sequence, error: .discarded) { execution in
                    await Self.serve(CodexConnection(execution: execution), home: home, updates: continuation, ready: ready.continuation)
                }
            }
            .mapError(ProcessFailure.init)
            if case .failure(let error) = outcome {
                let failure: CodexFailure =
                    switch error {
                        case .launch(let launch) where launch.code == .executableNotFound: .applicationUnavailable
                        default: CodexFailure(process: error)
                    }
                ready.continuation.yield(.failure(failure))
                ready.continuation.finish()
            }
        }
        var iterator: AsyncStream<Result<CodexConnection, CodexFailure>>.Iterator =
            ready.stream.makeAsyncIterator()
        switch await iterator.next() ?? .failure(.connectionClosed) {
            case .success(let connection):
                servers[home] = CodexServer(connection: connection, task: task, identity: identity)
                return connection
            case .failure(let error):
                task.cancel()
                await task.value
                throw error
        }
    }

    private nonisolated static func serve(
        _ connection: CodexConnection,
        home: URL,
        updates: AsyncStream<URL>.Continuation,
        ready: AsyncStream<Result<CodexConnection, CodexFailure>>.Continuation,
    ) async {
        await withDiscardingTaskGroup { group in
            group.addTask(name: "codex read") { await connection.read() }
            group.addTask(name: "codex rate limits") {
                for await update: CodexProtocol.RateLimitsUpdated in connection.updates where update.rateLimits.isCodex {
                    updates.yield(home)
                }
            }
            ready.yield(await Result { () async throws(CodexFailure) in try await connection.initialize() }.map { _ in connection })
            ready.finish()
        }
    }

    private func stopServer(_ home: URL) async {
        guard let server: CodexServer = servers.removeValue(forKey: home) else { return }
        await server.connection.finish(.connectionClosed)
        server.task.cancel()
        await server.task.value
    }
}
