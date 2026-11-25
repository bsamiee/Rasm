import CasePaths
import CryptoKit
import Encoding
import Files
import Foundation
import Processes
import Results
import Security
import Subprocess
import System

// --- [MODELS] --------------------------------------------------------------------------

nonisolated struct ClaudeOAuthToken: Sendable {
    struct Attributes: Codable, Sendable {
        let accessToken: String?
        let refreshToken: String?
        let subscriptionType: String?
    }

    let document: JSONDocument<Attributes>
    let accessToken: String

    init?(_ document: JSONDocument<Attributes>) throws(AccountFailure) {
        guard let token = document.known.accessToken else { throw AccountFailure(subject: "Claude access token", reason: .missing) }
        guard !token.isEmpty, document.known.refreshToken?.isEmpty != true else { return nil }
        (self.document, accessToken) = (document, token)
    }
}

nonisolated struct ClaudeOAuthAccount: Codable, Sendable {
    let accountUuid: String?
    let emailAddress: String?
    let organizationUuid: String?
    let organizationType: String?

    func identity(plan: String?) throws(AccountFailure) -> AccountIdentity {
        try AccountIdentity(accountID: accountUuid, organizationID: organizationUuid, email: emailAddress, plan: plan ?? organizationType)
    }
}

nonisolated struct ClaudeCredentialItem: Codable, Sendable {
    let claudeAiOauth: JSONDocument<ClaudeOAuthToken.Attributes>?
}

nonisolated struct ClaudeAccountFile: Codable, Sendable {
    let oauthAccount: JSONDocument<ClaudeOAuthAccount>?
    let hasCompletedOnboarding: Bool?
}

nonisolated struct ClaudeCredential: Sendable {
    let token: ClaudeOAuthToken
    let account: JSONDocument<ClaudeOAuthAccount>
    let identity: AccountIdentity
}

@CasePathable
nonisolated enum ClaudeStoreContent: Sendable {
    case signedOut
    case unidentified(ClaudeOAuthToken)
    case credential(ClaudeCredential)

    var token: ClaudeOAuthToken? {
        switch self {
            case .signedOut: nil
            case .unidentified(let token): token
            case .credential(let credential): credential.token
        }
    }
}

// --- [SERVICES] ------------------------------------------------------------------------

nonisolated struct ClaudeCredentialStore: Sendable {
    // --- [STATE]

    let directory: URL
    let configFile: URL
    let directoryConfigFile: URL
    let service: String
    let username: String
    let variables: [Environment.Key: String]

    init(home: URL, environment: [Environment.Key: String], username: String) {
        let defaultDirectory = home.appending(path: ".claude", directoryHint: .isDirectory)
        let configured = environment["CLAUDE_CONFIG_DIR"].flatMap { value in value.isEmpty ? nil : URL(filePath: value, directoryHint: .isDirectory) }
        let secure = (environment["CLAUDE_SECURESTORAGE_CONFIG_DIR"] ?? environment["CLAUDE_CONFIG_DIR"]).flatMap { value in value.isEmpty ? nil : value.precomposedStringWithCanonicalMapping }
        directory = secure.map { value in URL(filePath: value, directoryHint: .isDirectory) } ?? defaultDirectory
        configFile = (configured ?? home).appending(path: ".claude.json")
        directoryConfigFile = (configured ?? defaultDirectory).appending(path: ".config.json")
        service = secure.map { value in "Claude Code-credentials-" + SHA256.hash(data: Data(value.utf8)).prefix(4).hexEncoded() } ?? "Claude Code-credentials"
        self.username = username
        variables = environment.filter { key, _ in ["CLAUDE_CONFIG_DIR", "CLAUDE_SECURESTORAGE_CONFIG_DIR"].contains(key) }
    }

    private var credentialFile: URL { directory.appending(path: ".credentials.json") }

    // --- [STORE_CONTENT]

    func read() async throws(AccountFailure) -> ClaudeStoreContent {
        let token = try await readItem()?.known.claudeAiOauth.flatMap(ClaudeOAuthToken.init)
        return switch (token, try token.flatMap { _ throws(AccountFailure) in try readAccount() }) {
            case (.none, _): .signedOut
            case (.some(let token), .none): .unidentified(token)
            case (.some(let token), .some(let account)):
                .credential(ClaudeCredential(token: token, account: account, identity: try account.known.identity(plan: token.document.known.subscriptionType)))
        }
    }

    func write(_ credential: ClaudeCredential, over item: JSONDocument<ClaudeCredentialItem>?) async throws(AccountFailure) {
        try await writeOAuth(credential.token.document, over: item)
        try await writeAccount(credential.account)
    }

    // --- [ACCOUNT_FILE]

    func identity() throws(AccountFailure) -> AccountIdentity? {
        try readAccount()?.known.identity(plan: nil)
    }

    func readAccount() throws(AccountFailure) -> JSONDocument<ClaudeOAuthAccount>? {
        try Self.read(existingConfigFile)?.known.oauthAccount
    }

    func writeAccount(_ account: JSONDocument<ClaudeOAuthAccount>) async throws(AccountFailure) {
        let file = existingConfigFile
        try file.deletingLastPathComponent().createDirectory()
        let retry = Backoff(count: 6, base: .milliseconds(200), cap: .seconds(4), jitter: 1)
        try await Lockfile.withLocks([Lockfile(file.appendingPathExtension("lock"), retry: retry)]) { () async throws(AccountFailure) in
            let fields = try Self.read(file)?.fields ?? [:]
            let mode: FilePermissions?
            do { mode = try ifPresent { () throws(Errno) in try FilePath(file.path(percentEncoded: false)).stat().permissions } } catch {
                throw AccountFailure(subject: file.lastPathComponent, reason: .filesystem(error))
            }
            try await JSONEncoder()
                .encode(JSONDocument(fields: fields, known: ClaudeAccountFile(oauthAccount: account, hasCompletedOnboarding: true)), subject: file.lastPathComponent)
                .write(to: file, permissions: mode ?? .ownerReadWrite)
        }
    }

    private var existingConfigFile: URL {
        if case .success(let stat) = Result(catching: { () throws(Errno) in try FilePath(directoryConfigFile.path(percentEncoded: false)).stat() }), stat.type == .regular,
            stat.size > 0 || (!stat.permissions.isDisjoint(with: [.ownerWrite, .groupWrite, .otherWrite]) && !FileManager.default.fileExists(atPath: configFile.path(percentEncoded: false)))
        {
            directoryConfigFile
        } else {
            configFile
        }
    }

    private static func read(_ file: URL) throws(AccountFailure) -> JSONDocument<ClaudeAccountFile>? {
        try file.contents().map { data throws(AccountFailure) in try JSONDecoder().decode(JSONDocument<ClaudeAccountFile>.self, from: data, subject: file.lastPathComponent) }
    }

    // --- [KEYCHAIN_ITEM]

    func readItem() async throws(AccountFailure) -> JSONDocument<ClaudeCredentialItem>? {
        let password = if let printed = try await security(["find-generic-password", "-a", username, "-s", service, "-w"]) { Data(printed.utf8) } else { try credentialFile.contents() }
        return try password.map { data throws(AccountFailure) in try JSONDecoder().decode(JSONDocument<ClaudeCredentialItem>.self, from: data, subject: "Claude credential") }
    }

    func writeOAuth(_ oauth: JSONDocument<ClaudeOAuthToken.Attributes>, over item: JSONDocument<ClaudeCredentialItem>?) async throws(AccountFailure) {
        let password = try JSONEncoder().encode(JSONDocument(fields: item?.fields ?? [:], known: ClaudeCredentialItem(claudeAiOauth: oauth)), subject: "Claude credential").hexEncoded()
        let arguments = ["add-generic-password", "-U", "-a", username, "-s", service, "-X", password]
        let line = arguments.map { argument in "\"" + argument.replacing("\\", with: "\\\\").replacing("\"", with: "\\\"") + "\"" }.joined(separator: " ") + "\n"
        _ = if line.utf8.count <= 4032 { try await security(["-i"], input: .string(line)) } else { try await security(Arguments(arguments)) }
        try credentialFile.remove()
    }

    func deleteItem() async throws(AccountFailure) {
        _ = try await security(["delete-generic-password", "-a", username, "-s", service])
        try credentialFile.remove()
    }

    private func security(_ arguments: Arguments, input: some InputProtocol = .none) async throws(AccountFailure) -> String? {
        func code(_ status: OSStatus) -> Int32 { status & 0x00FF_FFFF }
        do throws(ProcessFailure) {
            return try await Configuration(
                executable: .path("/usr/bin/security"),
                arguments: arguments,
                environment: .custom([:]),
                workingDirectory: FilePath(URL.homeDirectory.path(percentEncoded: false)),
                platformOptions: .session(),
            )
            .run(output: .string(limit: 1 << 20), within: .seconds(15), input: input)
            .standardOutput
        } catch {
            return switch error.reason {
                case .exit(.exited(code(errSecItemNotFound)), _): nil
                case .exit(.exited(code(errSecInteractionNotAllowed)), _): throw AccountFailure(subject: "Keychain", reason: .interactionNotAllowed)
                default: throw AccountFailure(subject: "Keychain", reason: error.reason.is(\.cancelled) ? .cancelled : .process(error))
            }
        }
    }

    // --- [LOCKS]

    static func storageWriteLockfile(in directory: URL) -> URL {
        directory.appending(path: ".storage-write.lock")
    }

    static func withLock<Value: Sendable>(directories: [URL], _ body: @escaping @Sendable () async throws(AccountFailure) -> Value) async throws(AccountFailure) -> Value {
        let refresh = Backoff(count: 5, base: .seconds(1), growth: 1, jitter: 1)
        let storageWrite = Backoff(count: 10, base: .milliseconds(100), cap: .seconds(1))
        let lockfiles = try Set(directories).sorted { first, second in first.path(percentEncoded: false) < second.path(percentEncoded: false) }
            .map { directory throws(AccountFailure) in
                try directory.createDirectory()
                let canonical: URL
                do { canonical = try directory.canonical() } catch { throw AccountFailure(subject: directory.lastPathComponent, reason: .filesystem(error)) }
                return [directory.appending(path: ".oauth_refresh.lock"), canonical.appendingPathExtension("lock")].map { url in
                    Lockfile(url, stale: .seconds(60), update: .seconds(5), retry: refresh)
                }
                    + [Lockfile(storageWriteLockfile(in: directory), stale: .seconds(15), retry: storageWrite)]
            }
        return try await Lockfile.withLocks(lockfiles.flatMap(\.self), body)
    }
}
