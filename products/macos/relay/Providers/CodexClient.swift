import AppKit
import CasePathsCore
import Encoding
import Files
import Foundation
import JSONRPC
import Logging
import Processes
import Results
import Subprocess
import System
import Telemetry

// --- [MODELS] --------------------------------------------------------------------------

private nonisolated struct CodexPackage: Decodable {
    let entrypoint: String
}

// --- [SERVICES] ------------------------------------------------------------------------

actor CodexClient: ProviderClient {
    typealias Connection = JSONRPCConnection<CodexProtocol.Method, AccountFailure>

    // --- [STATE]

    private static let desktop: String = "com.openai.codex"

    nonisolated let liveHome: URL
    private let paths: FileLocations
    private let environment: [Environment.Key: String]
    private let logger = Logger(label: String(reflecting: CodexClient.self))
    private var servers: [URL: (connection: Connection, task: Task<Void, Never>, identity: AccountIdentity?)] = [:]

    init(paths: FileLocations, environment: [Environment.Key: String]) {
        self.paths = paths
        self.environment = environment
        liveHome =
            environment["CODEX_HOME"].flatMap { value in value.isEmpty ? nil : URL(filePath: value, directoryHint: .isDirectory) } ?? paths.home.appending(path: ".codex", directoryHint: .isDirectory)
    }

    nonisolated var liveAuthFile: URL { CodexAuthFile.file(in: liveHome) }

    private nonisolated func home(for account: Account, isSelected: Bool) -> URL {
        isSelected ? liveHome : paths.codexHome(account.id)
    }

    private func identity(at home: URL) throws(AccountFailure) -> AccountIdentity {
        guard let identity = try CodexAuthFile.read(in: home)?.identity else { throw AccountFailure(.openAI, .signInRequired) }
        return identity
    }

    // --- [SELECTION]

    func currentSelection(selected _: Account?) throws(AccountFailure) -> AccountIdentity? {
        try CodexAuthFile.read(in: liveHome)?.identity
    }

    func holdsCredential(for account: Account, isSelected: Bool) throws(AccountFailure) -> Bool {
        try CodexAuthFile.read(in: home(for: account, isSelected: isSelected))?.identity?.isSameAccount(as: account.identity) ?? false
    }

    // --- [SWITCHES]

    func preconditions() async throws(AccountFailure) {
        let response = try await connection(for: liveHome).request(.configRead, params: CodexProtocol.ConfigReadParams(includeLayers: false), returning: CodexProtocol.EffectiveConfig.self)
        guard response.config.cliAuthCredentialsStore ?? .known(.file) == .known(.file) else { throw AccountFailure(.openAI, .keyringStorage) }
        guard response.config.forcedChatgptWorkspaceId == nil else { throw AccountFailure(.openAI, .forcedWorkspace) }
    }

    func select(_ account: Account, outgoing: Account?) async throws(AccountFailure) -> AccountIdentity {
        let incoming = paths.codexHome(account.id)
        guard NSRunningApplication.runningApplications(withBundleIdentifier: Self.desktop).allSatisfy(\.isTerminated) else {
            throw AccountFailure(subject: Provider.openAI.tool, reason: .desktopRunning)
        }
        try await preconditions()
        for home in [liveHome, incoming] + [outgoing.map { outgoing in paths.codexHome(outgoing.id) }].compactMap(\.self) { await stopServer(home) }
        guard let saved = try CodexAuthFile.read(in: incoming), let identity = saved.identity, identity.isSameAccount(as: account.identity) else {
            throw AccountFailure(.openAI, .signInRequired)
        }
        let live = try CodexAuthFile.read(in: liveHome)
        if live?.identity?.isSameAccount(as: account.identity) != true {
            guard live?.identity.map({ identity in outgoing?.identity.isSameAccount(as: identity) == true }) ?? (outgoing == nil) else { throw AccountFailure(.openAI, .selectionChanged) }
            if let outgoing, let live { try await live.data.write(to: CodexAuthFile.file(in: paths.codexHome(outgoing.id))) }
            try await saved.data.write(to: liveAuthFile)
            logger.notice("Installed account auth file in live CODEX_HOME", metadata: Provider.openAI.metadata(account.id, ["outgoingSaved": .stringConvertible(outgoing != nil && live != nil)]))
        }
        try CodexAuthFile.file(in: incoming).remove()
        return identity
    }

    // --- [SIGN_IN]

    func signIn(_ target: SignInTarget, codes _: AsyncStream<String>) async throws(AccountFailure) -> AccountIdentity {
        let (home, expected): (URL, AccountIdentity?) =
            switch target {
                case .newAccount(let id): (paths.codexHome(id), nil)
                case .account(let account, let isSelected): (home(for: account, isSelected: isSelected), account.identity)
            }
        let connection = try await connection(for: home)
        try await withTimeout(.seconds(300), expired: AccountFailure(subject: "OpenAI sign-in", reason: .timedOut(.seconds(300)))) { () async throws(AccountFailure) in
            let completions = connection.subscribe(.accountLoginCompleted, to: CodexProtocol.LoginCompleted.self)
            let started = try await connection.request(.accountLoginStart, params: CodexProtocol.LoginStartParams(), returning: CodexProtocol.LoginStarted.self)
            guard started.authUrl.scheme == "https", started.authUrl.host(percentEncoded: false) != nil else { throw AccountFailure(subject: "OpenAI sign-in page", reason: .missing) }
            guard NSWorkspace.shared.open(started.authUrl) else { throw AccountFailure(.openAI, .signInPageUnopened) }
            self.logger.info("Opened OpenAI sign-in page", metadata: ["home": "\(home.path(percentEncoded: false), privacy: .private)"])
            let completion = await completions.first { received in received[case: \.success].map { completed in completed.loginId == started.loginId } ?? true }
            guard let completed = try completion?.get() else { throw AccountFailure(subject: "OpenAI sign-in", reason: .cancelled) }
            guard completed.success else { throw AccountFailure(.openAI, .signInRefused(reason: completed.error)) }
        }
        let identity = try identity(at: home)
        guard expected.map(identity.isSameAccount(as:)) ?? true else {
            try await logOut(home: home)
            throw AccountFailure(.openAI, .accountChanged)
        }
        return identity
    }

    func signOut(_ account: Account, isSelected: Bool) async throws(AccountFailure) {
        try await logOut(home: home(for: account, isSelected: isSelected))
    }

    func deletePrivateStore(id: UUID) async throws(AccountFailure) {
        let home = paths.codexHome(id)
        await stopServer(home)
        try home.remove()
    }

    private func logOut(home: URL) async throws(AccountFailure) {
        defer { await stopServer(home) }
        try await connection(for: home).request(.accountLogout)
    }

    // --- [USAGE]

    func usage(for account: Account, isSelected: Bool) async throws(AccountFailure) -> AccountUsage {
        let home = home(for: account, isSelected: isSelected)
        let identity = try identity(at: home)
        guard identity.isSameAccount(as: account.identity) else { throw AccountFailure(.openAI, .accountChanged) }
        return try await connection(for: home)
            .request(.accountRateLimitsRead, params: CodexProtocol.RateLimitsReadParams(excludeResetCreditDetails: true), returning: CodexProtocol.AccountRateLimits.self)
            .usage(identity: identity, observedAt: Date())
    }

    // --- [SERVERS]

    func shutdown() async {
        for home in Array(servers.keys) { await stopServer(home) }
    }

    private func connection(for home: URL) async throws(AccountFailure) -> Connection {
        let identity = try CodexAuthFile.read(in: home)?.identity
        if let server = servers[home], server.identity == identity, !server.connection.isClosed { return server.connection }
        await stopServer(home)
        let executable: FilePath
        do throws(SubprocessError) {
            executable = try await Executable.name("codex").resolveExecutablePath(in: .custom(environment))
        } catch  where error.code == .executableNotFound {
            guard let package = NSWorkspace.shared.urlForApplication(withBundleIdentifier: Self.desktop).flatMap(Bundle.init(url:))?.url(forResource: "codex-cli", withExtension: nil),
                let manifest = try package.appending(path: "codex-package.json").contents()
            else { throw AccountFailure(subject: Provider.openAI.tool, reason: .notInstalled) }
            executable = FilePath(package.appending(path: try JSONDecoder().decode(CodexPackage.self, from: manifest, subject: "codex-package.json").entrypoint).path(percentEncoded: false))
        } catch {
            throw AccountFailure(subject: Provider.openAI.tool, reason: .process(ProcessFailure(subject: "codex", error)))
        }
        try home.createDirectory()
        let excluded: Set<Environment.Key> = ["OPENAI_API_KEY", "OPENAI_BASE_URL", "OPENAI_ORG_ID", "OPENAI_PROJECT_ID"]
        let invocation = Configuration(
            executable: .path(executable),
            arguments: ["app-server"],
            environment: .custom(environment.filter { key, _ in !key.rawValue.hasPrefix("CODEX_") && !excluded.contains(key) }).updating([
                "CODEX_HOME": FilePath(home.path(percentEncoded: false)).string
            ]
            ),
            platformOptions: .session(),
        )
        let subject = "Codex app server"
        let ready = AsyncStream<Result<Connection, AccountFailure>>.makeStream()
        let task = Task(name: "codex app-server \(home.lastPathComponent)") { [logger] in
            let fields: Logger.Metadata = ["home": "\(home.path(percentEncoded: false), privacy: .private)", "executable": "\(executable.string, privacy: .private)"]
            do {
                let status = try await Subprocess.run(invocation, input: .inputWriter, output: .sequence, error: .sequence) { execution in
                    logger.info("Started codex app-server", metadata: fields.merging(["pid": "\(execution.processIdentifier, privacy: .public)"]) { _, stated in stated })
                    async let standardError: Void = {
                        do {
                            for try await line in execution.standardError.strings(separatedBy: .unicodeScalarSequence(["\n"]), bufferingPolicy: .unbounded) {
                                logger.debug("codex app-server wrote standard error", metadata: ["line": "\(line, privacy: .private)"])
                            }
                        } catch {
                            logger.error("codex app-server standard error read failed", error: error)
                        }
                    }()
                    await Connection.run(
                        reading: execution.standardOutput.strings(separatedBy: .unicodeScalarSequence(["\n"]), bufferingPolicy: .unbounded).map { line in Data(line.utf8) },
                        writing: { line in _ = try await execution.standardInputWriter.write(line) },
                        timeout: .seconds(60),
                        handler: { _, _ throws(JSONRPCError) in throw JSONRPCError(code: -32601, message: "Relay provides no agent tools") },
                        failure: { failure in
                            if case .cancelled = failure.reason { AccountFailure(subject: subject, reason: .cancelled) } else { AccountFailure(subject: subject, reason: .rpc(failure)) }
                        },
                    ) { connection in
                        ready.continuation.yield(
                            await Result { () async throws(AccountFailure) in
                                try await connection.request(.initialize, params: CodexProtocol.InitializeParams())
                                try await connection.notify(.initialized)
                                return connection
                            }
                        )
                        ready.continuation.finish()
                        _ = await connection.waitUntilClosed()
                    }
                    try await execution.standardInputWriter.finish()
                    await standardError
                }
                .terminationStatus
                logger.log(
                    level: Task.isCancelled ? .info : .notice,
                    "codex app-server ended",
                    metadata: fields.merging(["status": "\(status, privacy: .public)", "stoppedByRelay": .stringConvertible(Task.isCancelled)]) { _, stated in stated },
                )
            } catch {
                ready.continuation.yield(.failure(AccountFailure(.openAI, child: ProcessFailure(subject: invocation.executable.description, error))))
                ready.continuation.finish()
            }
        }
        switch await ready.stream.first(where: { _ in true }) ?? .failure(AccountFailure(subject: subject, reason: .cancelled)) {
            case .success(let connection):
                servers[home] = (connection, task, identity)
                return connection
            case .failure(let error):
                task.cancel()
                await task.value
                throw error
        }
    }

    private func stopServer(_ home: URL) async {
        guard let server = servers.removeValue(forKey: home) else { return }
        logger.info("Stopping codex app-server", metadata: ["home": "\(home.path(percentEncoded: false), privacy: .private)"])
        server.task.cancel()
        await server.task.value
    }
}
