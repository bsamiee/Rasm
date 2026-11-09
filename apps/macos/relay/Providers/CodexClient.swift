import AppKit
import CurrentProcess
import Encoding
import Files
import Foundation
import JSONRPC
import Processes
import Results
import Subprocess
import System
import os

// --- [CONSTANTS] -----------------------------------------------------------------------

nonisolated extension Environment.Key {
    static let codexHome: Self = "CODEX_HOME"
}

// --- [MODELS] --------------------------------------------------------------------------

private nonisolated struct CodexPackage: Decodable {
    let entrypoint: String
}

// --- [SERVICES] ------------------------------------------------------------------------

private nonisolated struct CodexServer {
    let connection: JSONRPCConnection<CodexProtocol.Method, CodexFailure>
    let task: Task<Void, Never>
    let identity: AccountIdentity?
}

actor CodexClient: ProviderClient {
    // --- [STATE]

    private static let desktop: String = "com.openai.codex"

    nonisolated let liveHome: URL
    private let paths: FileLocations
    private let environment: [Environment.Key: String]
    private let logger: Logger = .init(for: CodexClient.self)
    private var servers: [URL: CodexServer] = [:]

    init(paths: FileLocations, environment: [Environment.Key: String]) {
        self.paths = paths
        self.environment = environment
        liveHome =
            environment[.codexHome].flatMap { value in
                value.isEmpty ? nil : URL(filePath: value, directoryHint: .isDirectory)
            } ?? paths.home.appending(path: ".codex", directoryHint: .isDirectory)
    }

    nonisolated var liveAuthFile: URL { CodexAuthFile.file(in: liveHome) }

    nonisolated func home(for account: Account, isSelected: Bool) -> URL {
        isSelected ? liveHome : paths.codexHome(account.id)
    }

    // --- [SELECTION]

    func currentSelection(selected _: Account?) -> Result<AccountIdentity?, ProviderError> {
        Result { () throws(CodexFailure) in try CodexAuthFile.read(in: liveHome)?.identity }.mapError(ProviderError.init(failure:))
    }

    func holdsCredential(for account: Account, isSelected: Bool) -> Result<Bool, ProviderError> {
        Result { () throws(CodexFailure) in
            try CodexAuthFile.read(in: home(for: account, isSelected: isSelected))?.identity?.isSameAccount(as: account.identity) ?? false
        }
        .mapError(ProviderError.init(failure:))
    }

    private func identity(at home: URL) throws(CodexFailure) -> AccountIdentity {
        guard let identity: AccountIdentity = try CodexAuthFile.read(in: home)?.identity else { throw .signInRequired }
        return identity
    }

    // --- [SWITCHES]

    func preconditions() async throws(CodexFailure) {
        let response: CodexProtocol.EffectiveConfig = try await connection(for: liveHome).request(
            .configRead,
            params: CodexProtocol.ConfigReadParams(includeLayers: false),
            returning: CodexProtocol.EffectiveConfig.self,
        )
        guard response.config.cliAuthCredentialsStore ?? .known(.file) == .known(.file) else { throw .keyringStorage }
        guard response.config.forcedChatgptWorkspaceId == nil else { throw .forcedWorkspace }
    }

    func select(
        _ account: Account,
        outgoing: Account?,
    ) async -> Result<AccountIdentity, ProviderError> {
        func installFile(_ data: Data, to destination: URL) throws(CodexFailure) {
            try Result {
                let file = FileWrapper(regularFileWithContents: data)
                file.fileAttributes[FileAttributeKey.posixPermissions.rawValue] = 0o600
                try FileManager.default.createDirectory(at: destination.deletingLastPathComponent(), withIntermediateDirectories: true)
                try file.write(to: destination, options: .atomic, originalContentsURL: nil)
            }
            .mapError(CodexFailure.storage)
            .get()
        }
        let incoming: URL = paths.codexHome(account.id)
        return await Result { () async throws(CodexFailure) in
            guard NSRunningApplication.runningApplications(withBundleIdentifier: Self.desktop).allSatisfy(\.isTerminated) else { throw .desktopRunning }
            try await preconditions()
            await stopServer(liveHome)
            await stopServer(incoming)
            if let outgoing { await stopServer(paths.codexHome(outgoing.id)) }
            guard let saved = try CodexAuthFile.read(in: incoming), let identity = saved.identity,
                identity.isSameAccount(as: account.identity)
            else { throw .signInRequired }
            let live = try CodexAuthFile.read(in: liveHome)
            if live?.identity?.isSameAccount(as: account.identity) != true {
                guard live?.identity.map({ identity in outgoing?.identity.isSameAccount(as: identity) == true }) ?? (outgoing == nil) else { throw .identityChanged }
                if let outgoing, let live { try installFile(live.data, to: CodexAuthFile.file(in: paths.codexHome(outgoing.id))) }
                try installFile(saved.data, to: liveAuthFile)
                logger.notice("Installed account \(account.id, privacy: .public) auth file in live CODEX_HOME, outgoing saved \(outgoing != nil && live != nil, privacy: .public)")
            }
            try remove(CodexAuthFile.file(in: incoming))
            return identity
        }
        .mapError(ProviderError.init(failure:))
    }

    private func remove(_ url: URL) throws(CodexFailure) {
        do { _ = try ifPresent { try FileManager.default.removeItem(at: url) } } catch { throw .storage(error) }
    }

    // --- [SIGN_IN]

    func signIn(_ target: SignInTarget, codes _: AsyncStream<String>) async -> Result<AccountIdentity, ProviderError> {
        let (home, expected): (URL, AccountIdentity?) =
            switch target {
                case .newAccount(let id): (paths.codexHome(id), nil)
                case .account(let account, let isSelected): (home(for: account, isSelected: isSelected), account.identity)
            }
        return await Result { () async throws(CodexFailure) in
            let connection: JSONRPCConnection<CodexProtocol.Method, CodexFailure> = try await connection(for: home)
            try await withTimeout(.seconds(300), expired: .timedOut(.seconds(300))) { () async throws(CodexFailure) in
                let completions: AsyncStream<Result<CodexProtocol.LoginCompleted, CodexFailure>> = connection.subscribe(.accountLoginCompleted, to: CodexProtocol.LoginCompleted.self)
                let started: CodexProtocol.LoginStarted = try await connection.request(
                    .accountLoginStart,
                    params: CodexProtocol.LoginStartParams(),
                    returning: CodexProtocol.LoginStarted.self,
                )
                guard started.authUrl.scheme == "https", started.authUrl.host(percentEncoded: false) != nil else { throw .invalidResponse(.accountLoginStart) }
                guard NSWorkspace.shared.open(started.authUrl) else { throw .signInPageUnopened }
                self.logger.info("Opened OpenAI sign-in page for \(home.path(percentEncoded: false), privacy: .private)")
                let completion: Result<CodexProtocol.LoginCompleted, CodexFailure>? = await completions.first { received in
                    switch received {
                        case .success(let completed): completed.loginId == started.loginId
                        case .failure: true
                    }
                }
                guard let completed: CodexProtocol.LoginCompleted = try completion?.get() else { throw .cancelled(CancellationError()) }
                guard completed.success else { throw .signInRefused(reason: completed.error) }
            }
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

    func deletePrivateStore(id: UUID) async -> Result<Void, ProviderError> {
        let home: URL = paths.codexHome(id)
        await stopServer(home)
        return Result { () throws(CodexFailure) in try remove(home) }.mapError(ProviderError.init(failure:))
    }

    private func logOut(home: URL) async throws(CodexFailure) {
        defer { await stopServer(home) }
        try await connection(for: home).request(.accountLogout)
    }

    // --- [USAGE]

    func usage(for account: Account, isSelected: Bool) async -> Result<AccountUsage, ProviderError> {
        let home: URL = home(for: account, isSelected: isSelected)
        return await Result { () async throws(CodexFailure) in
            let identity: AccountIdentity = try identity(at: home)
            guard identity.isSameAccount(as: account.identity) else { throw .identityChanged }
            return try await connection(for: home)
                .request(
                    .accountRateLimitsRead,
                    params: CodexProtocol.RateLimitsReadParams(excludeResetCreditDetails: true),
                    returning: CodexProtocol.AccountRateLimits.self,
                )
                .usage(identity: identity, observedAt: Date())
        }
        .mapError(ProviderError.init(failure:))
    }

    // --- [SERVERS]

    func shutdown() async {
        for home: URL in Array(servers.keys) { await stopServer(home) }
    }

    private func connection(for home: URL) async throws(CodexFailure) -> JSONRPCConnection<CodexProtocol.Method, CodexFailure> {
        let identity: AccountIdentity? = try CodexAuthFile.read(in: home)?.identity
        if let server: CodexServer = servers[home], server.identity == identity, !server.connection.isFinished {
            return server.connection
        }
        await stopServer(home)
        let excludedEnvironmentVariables: Set<Environment.Key> = ["OPENAI_API_KEY", "OPENAI_BASE_URL", "OPENAI_ORG_ID", "OPENAI_PROJECT_ID"]
        let executable: FilePath
        switch await Result(catching: { () async throws(SubprocessError) in try await Executable.name("codex").resolveExecutablePath(in: .custom(environment)) }) {
            case .success(let found): executable = found
            case .failure(let error) where error.code != .executableNotFound: throw .process(ProcessFailure(subject: "codex", error))
            case .failure:
                guard
                    let package: URL = NSWorkspace.shared.urlForApplication(withBundleIdentifier: Self.desktop).flatMap(Bundle.init(url:))?
                        .url(forResource: "codex-cli", withExtension: nil)
                else { throw .executableMissing }
                let manifest: CodexPackage = try Result { try JSONDecoder().decode(CodexPackage.self, from: Data(contentsOf: package.appending(path: "codex-package.json"))) }
                    .mapError(CodexFailure.storage)
                    .get()
                executable = FilePath(package.appending(path: manifest.entrypoint).path(percentEncoded: false))
        }
        try Result { try FileManager.default.createDirectory(at: home, withIntermediateDirectories: true) }.mapError(CodexFailure.storage).get()
        let invocation = Configuration(
            executable: .path(executable),
            arguments: ["app-server"],
            environment: .custom(environment.filter { key, _ in !key.rawValue.hasPrefix("CODEX_") && !excludedEnvironmentVariables.contains(key) })
                .updating([.codexHome: FilePath(home.path(percentEncoded: false)).string]),
            platformOptions: .session(),
        )
        let ready = AsyncStream<Result<JSONRPCConnection<CodexProtocol.Method, CodexFailure>, CodexFailure>>.makeStream()
        let task: Task<Void, Never> = Task(name: "codex app-server \(home.lastPathComponent)") { [logger] in
            do {
                let status: TerminationStatus = try await Subprocess.run(invocation, input: .inputWriter, output: .sequence, error: .discarded) { execution in
                    logger.info(
                        "Started codex app-server \(executable.string, privacy: .public) as PID \(execution.processIdentifier, privacy: .public) for \(home.path(percentEncoded: false), privacy: .private)"
                    )
                    await JSONRPCConnection<CodexProtocol.Method, CodexFailure>.run(
                        reading: execution.standardOutput.strings(separatedBy: .unicodeScalarSequence(["\n"]), bufferingPolicy: .unbounded).map { line in Data(line.utf8) },
                        writing: { line in _ = try await execution.standardInputWriter.write(line) },
                        timeout: .seconds(60),
                        serving: { _, _ throws(JSONRPCError) in throw JSONRPCError(code: -32601, message: "Relay provides no agent tools") },
                        failure: { failure in
                            switch failure.reason {
                                case .timedOut(let timeout): .timedOut(timeout)
                                case .cancelled(let error): .cancelled(error)
                                case .transport, .closed, .unreadable, .unencodable, .refused: .rpc(failure)
                            }
                        },
                    ) { connection in
                        ready.continuation.yield(
                            await Result { () async throws(CodexFailure) in
                                try await connection.request(.initialize, params: CodexProtocol.InitializeParams())
                                try await connection.notify(.initialized)
                                return connection
                            }
                        )
                        ready.continuation.finish()
                        _ = await connection.finished()
                    }
                }
                .terminationStatus
                logger.log(
                    level: Task.isCancelled ? .info : .default,
                    "codex app-server for \(home.path(percentEncoded: false), privacy: .private) ended with \(status, privacy: .public), stopped by Relay \(Task.isCancelled, privacy: .public)",
                )
            } catch {
                ready.continuation.yield(.failure(CodexFailure(process: ProcessFailure(subject: invocation.executable.description, error))))
                ready.continuation.finish()
            }
        }
        switch await ready.stream.first(where: { _ in true }) ?? .failure(.cancelled(CancellationError())) {
            case .success(let connection):
                servers[home] = CodexServer(connection: connection, task: task, identity: identity)
                return connection
            case .failure(let error):
                task.cancel()
                await task.value
                throw error
        }
    }

    private func stopServer(_ home: URL) async {
        guard let server: CodexServer = servers.removeValue(forKey: home) else { return }
        logger.info("Stopping codex app-server for \(home.path(percentEncoded: false), privacy: .private)")
        server.task.cancel()
        await server.task.value
    }
}
