import AppKit
import Files
import Foundation
import JSONRPC
import Processes
import Results
import Subprocess
import System

// --- [MODELS] --------------------------------------------------------------------------

private nonisolated struct CodexPackage: Decodable {
    let entrypoint: String
    let version: String
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

    nonisolated var liveAuthFile: URL { CodexAuthFile.file(in: liveHome) }

    nonisolated func home(for account: Account, isSelected: Bool) -> URL {
        isSelected ? liveHome : paths.codexHome(account.id)
    }

    // --- [SELECTION]
    func currentSelection(selected _: Account?) -> Result<AccountIdentity?, ProviderError> {
        Result { () throws(CodexFailure) in try CodexAuthFile.read(in: liveHome)?.identity }.mapError(ProviderError.init(failure:))
    }

    func holdsCredential(for account: Account) -> Result<Bool, ProviderError> {
        Result { () throws(CodexFailure) in
            try CodexAuthFile.read(in: paths.codexHome(account.id))?.identity?.isSameAccount(as: account.identity) ?? false
        }
        .mapError(ProviderError.init(failure:))
    }

    private func identity(at home: URL) throws(CodexFailure) -> AccountIdentity {
        guard let identity: AccountIdentity = try CodexAuthFile.read(in: home)?.identity else { throw .signInRequired }
        return identity
    }

    // --- [SWITCH]
    func preconditions() async throws(CodexFailure) {
        let response: CodexProtocol.EffectiveConfig = try await connection(for: liveHome).request(
            .configRead,
            params: CodexProtocol.ConfigReadParams(includeLayers: false, cwd: nil),
            returning: CodexProtocol.EffectiveConfig.self,
        )
        guard response.config.cliAuthCredentialsStore ?? .file == .file else { throw .keyringStorage }
        guard response.config.forcedChatgptWorkspaceId == nil else { throw .forcedWorkspace }
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
        let incoming: URL = paths.codexHome(account.id)
        return await Result { () async throws(CodexFailure) in
            guard NSRunningApplication.runningApplications(withBundleIdentifier: Self.desktop).allSatisfy(\.isTerminated) else { throw .desktopRunning }
            try await preconditions()
            await stopServer(liveHome)
            await stopServer(incoming)
            if let outgoing { await stopServer(paths.codexHome(outgoing.id)) }
            guard let saved: (data: Data, identity: AccountIdentity?) = try CodexAuthFile.read(in: incoming), let identity: AccountIdentity = saved.identity,
                identity.isSameAccount(as: account.identity)
            else { throw .signInRequired }
            let live: (data: Data, identity: AccountIdentity?)? = try CodexAuthFile.read(in: liveHome)
            if live?.identity?.isSameAccount(as: account.identity) != true {
                guard live?.identity.map({ identity in outgoing?.identity.isSameAccount(as: identity) == true }) ?? (outgoing == nil) else { throw .identityChanged }
                if let outgoing, let live { try installFile(live.data, to: CodexAuthFile.file(in: paths.codexHome(outgoing.id))) }
                try installFile(saved.data, to: liveAuthFile)
            }
            try remove(CodexAuthFile.file(in: incoming))
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
            let connection: JSONRPCConnection<CodexProtocol.Method, CodexFailure> = try await connection(for: home)
            try await withTimeout(.seconds(300)) {
                await Result { () async throws(CodexFailure) in
                    let started: CodexProtocol.LoginStarted = try await connection.request(
                        .accountLoginStart,
                        params: CodexProtocol.LoginStartParams(),
                        returning: CodexProtocol.LoginStarted.self,
                    )
                    guard started.authUrl.scheme == "https", started.authUrl.host(percentEncoded: false) != nil else { throw .invalidResponse(.accountLoginStart) }
                    guard NSWorkspace.shared.open(started.authUrl) else { throw .signInPageUnopened }
                    let completed: CodexProtocol.LoginCompleted = try await connection.notification(.accountLoginCompleted, returning: CodexProtocol.LoginCompleted.self) {
                        completed in completed.loginId == started.loginId
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

    func greet(_ account: Account, isSelected: Bool) async -> Result<Date?, ProviderError> {
        let workingDirectory: URL = paths.workingDirectory
        return await Result { () async throws(CodexFailure) in
            let connection: JSONRPCConnection<CodexProtocol.Method, CodexFailure> = try await connection(for: home(for: account, isSelected: isSelected))
            try await withTimeout(.seconds(120)) { await Result { () async throws(CodexFailure) in try await CodexProtocol.greet(connection, workingDirectory: workingDirectory) } }.get()
            return nil
        }
        .mapError(ProviderError.init(failure:))
    }

    // --- [SERVERS]
    func shutdown() async {
        for home: URL in Array(servers.keys) { await stopServer(home) }
        updatesContinuation.finish()
    }

    private func connection(for home: URL) async throws(CodexFailure) -> JSONRPCConnection<CodexProtocol.Method, CodexFailure> {
        let identity: AccountIdentity? = try CodexAuthFile.read(in: home)?.identity
        if let server: CodexServer = servers[home], server.identity == identity, !(await server.connection.isFinished) {
            return server.connection
        }
        await stopServer(home)
        let workingDirectory: URL = paths.workingDirectory
        let excludedEnvironmentVariables: Set<Environment.Key> = ["OPENAI_API_KEY", "OPENAI_BASE_URL", "OPENAI_ORG_ID", "OPENAI_PROJECT_ID"]
        guard
            let package: URL = NSWorkspace.shared.urlForApplication(withBundleIdentifier: Self.desktop).flatMap(Bundle.init(url:))?
                .url(forResource: "codex-cli", withExtension: nil)
        else { throw .applicationUnavailable }
        let manifest: CodexPackage = try Result { try JSONDecoder().decode(CodexPackage.self, from: Data(contentsOf: package.appending(path: "codex-package.json"))) }
            .mapError(CodexFailure.storage)
            .get()
        try Result {
            try FileManager.default.createDirectory(at: home, withIntermediateDirectories: true)
            try FileManager.default.createDirectory(at: workingDirectory, withIntermediateDirectories: true)
        }
        .mapError(CodexFailure.storage)
        .get()
        let invocation: Configuration = Configuration(
            executable: .path(FilePath(package.appending(path: manifest.entrypoint).path(percentEncoded: false))),
            arguments: ["app-server"],
            environment: .custom(environment.filter { key, _ in !key.rawValue.hasPrefix("CODEX_") && !excludedEnvironmentVariables.contains(key) })
                .updating(["CODEX_HOME": FilePath(home.path(percentEncoded: false)).string]),
            workingDirectory: FilePath(workingDirectory.path(percentEncoded: false)),
            platformOptions: .session,
        )
        let ready:
            (
                stream: AsyncStream<Result<JSONRPCConnection<CodexProtocol.Method, CodexFailure>, CodexFailure>>,
                continuation: AsyncStream<Result<JSONRPCConnection<CodexProtocol.Method, CodexFailure>, CodexFailure>>.Continuation
            ) = AsyncStream.makeStream()
        let updates: AsyncStream<URL>.Continuation = updatesContinuation
        let task: Task<Void, Never> = Task(name: "codex app-server \(home.lastPathComponent)") {
            do {
                _ = try await Subprocess.run(invocation, input: .inputWriter, output: .sequence, error: .discarded) { execution in
                    await JSONRPCConnection<CodexProtocol.Method, CodexFailure>.run(
                        over: execution,
                        decoder: CodexProtocol.decoder,
                        timeout: .seconds(60),
                        retaining: [.accountLoginCompleted, .turnCompleted],
                        serve: { _ in .failure(JSONRPCRejection(code: -32601, message: "Relay does not provide agent tools")) },
                        failure: { failure in .rpc(failure, serverVersion: manifest.version) },
                    ) { connection in
                        let limits: AsyncStream<CodexProtocol.RateLimitsUpdated> = await connection.subscribe(.accountRateLimitsUpdated, to: CodexProtocol.RateLimitsUpdated.self)
                        ready.continuation.yield(
                            await Result { () async throws(CodexFailure) in
                                try await connection.request(.initialize, params: CodexProtocol.InitializeParams())
                                try await connection.notify(.initialized)
                                return connection
                            }
                        )
                        ready.continuation.finish()
                        for await update: CodexProtocol.RateLimitsUpdated in limits where update.rateLimits.isCodex {
                            updates.yield(home)
                        }
                    }
                }
            } catch {
                ready.continuation.yield(.failure(CodexFailure(process: error)))
                ready.continuation.finish()
            }
        }
        switch await ready.stream.first(where: { _ in true }) ?? .failure(.deadline(.cancelled)) {
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
        server.task.cancel()
        await server.task.value
    }
}
