import CasePaths
import CryptoKit
import Encoding
import Files
import Foundation
import Processes
import Security
import Subprocess
import System

// --- [CONSTANTS] -----------------------------------------------------------------------

nonisolated extension Environment.Key {
    static let claudeConfigDirectory: Self = "CLAUDE_CONFIG_DIR"
    static let claudeSecureStorageDirectory: Self = "CLAUDE_SECURESTORAGE_CONFIG_DIR"
}

// --- [MODELS] --------------------------------------------------------------------------

nonisolated struct ClaudeOAuthToken: Sendable {
    struct Attributes: Codable, Sendable {
        let accessToken: String?
        let refreshToken: String?
        let subscriptionType: String?
    }

    let document: JSONDocument<Attributes>
    let accessToken: String

    static func make(_ document: JSONDocument<Attributes>) throws(ClaudeFailure) -> Self? {
        guard let token = document.known.accessToken else { throw .invalidCredentials(nil) }
        return token.isEmpty || document.known.refreshToken?.isEmpty == true ? nil : Self(document: document, accessToken: token)
    }
}

nonisolated struct ClaudeOAuthAccount: Codable, Sendable {
    let accountUuid: String?
    let emailAddress: String?
    let organizationUuid: String?
    let organizationType: String?

    func identity(plan: String?) throws(ClaudeFailure) -> AccountIdentity {
        try AccountIdentity.make(accountID: accountUuid, organizationID: organizationUuid, email: emailAddress, plan: plan ?? organizationType).mapError(ClaudeFailure.invalidIdentity).get()
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
    let configDirectoryPath: String?
    let username: String
    let variables: [Environment.Key: String]

    init(home: URL, environment: [Environment.Key: String], username: String) {
        let defaultDirectory = home.appending(path: ".claude", directoryHint: .isDirectory)
        let configured = environment[.claudeConfigDirectory].flatMap { value in value.isEmpty ? nil : URL(filePath: value, directoryHint: .isDirectory) }
        configDirectoryPath = (environment[.claudeSecureStorageDirectory] ?? environment[.claudeConfigDirectory]).flatMap { value in
            value.isEmpty ? nil : value.precomposedStringWithCanonicalMapping
        }
        directory = configDirectoryPath.map { value in URL(filePath: value, directoryHint: .isDirectory) } ?? defaultDirectory
        configFile = (configured ?? home).appending(path: ".claude.json")
        directoryConfigFile = (configured ?? defaultDirectory).appending(path: ".config.json")
        self.username = username
        variables = environment.filter { key, _ in [.claudeConfigDirectory, .claudeSecureStorageDirectory].contains(key) }
    }

    var service: String {
        configDirectoryPath.map { value in
            "Claude Code-credentials-" + SHA256.hash(data: Data(value.utf8)).prefix(4).hexEncoded()
        } ?? "Claude Code-credentials"
    }

    private var credentialFile: URL { directory.appending(path: ".credentials.json") }

    // --- [STORE_CONTENT]

    func read() async throws(ClaudeFailure) -> ClaudeStoreContent {
        let token = try await readItem()?.known.claudeAiOauth.flatMap(ClaudeOAuthToken.make)
        return switch (token, try token.flatMap { _ throws(ClaudeFailure) in try readAccount() }) {
            case (.none, _): .signedOut
            case (.some(let token), .none): .unidentified(token)
            case (.some(let token), .some(let account)):
                .credential(ClaudeCredential(token: token, account: account, identity: try account.known.identity(plan: token.document.known.subscriptionType)))
        }
    }

    func write(_ credential: ClaudeCredential, over item: JSONDocument<ClaudeCredentialItem>?) async throws(ClaudeFailure) {
        try await writeOAuth(credential.token.document, over: item)
        try await writeAccount(credential.account)
    }

    // --- [ACCOUNT_FILE]

    func identity() throws(ClaudeFailure) -> AccountIdentity? {
        try readAccount()?.known.identity(plan: nil)
    }

    func readAccount() throws(ClaudeFailure) -> JSONDocument<ClaudeOAuthAccount>? {
        try Self.read(at: existingConfigFile)?.known.oauthAccount
    }

    func writeAccount(_ account: JSONDocument<ClaudeOAuthAccount>) async throws(ClaudeFailure) {
        let file = existingConfigFile
        try Result { try FileManager.default.createDirectory(at: file.deletingLastPathComponent(), withIntermediateDirectories: true) }.mapError(ClaudeFailure.filesystem).get()
        let retry = Backoff(count: 6, base: .milliseconds(200), cap: .seconds(4), jitter: 1)
        try await Lockfile.withLocks([Lockfile(file.appendingPathExtension("lock"), retry: retry)]) { () throws(ClaudeFailure) in
            let fields = try Self.read(at: file)?.fields ?? [:]
            let mode = try Result { () throws(Errno) in try ifPresent { () throws(Errno) in try FilePath(file.path(percentEncoded: false)).stat().permissions } }
                .mapError(ClaudeFailure.filesystem)
                .get()
            try Result {
                let document = FileWrapper(
                    regularFileWithContents: try JSONEncoder().encode(JSONDocument(fields: fields, known: ClaudeAccountFile(oauthAccount: account, hasCompletedOnboarding: true)))
                )
                document.fileAttributes[FileAttributeKey.posixPermissions.rawValue] = Int((mode ?? .ownerReadWrite).rawValue)
                try document.write(to: file, options: .atomic, originalContentsURL: nil)
            }
            .mapError(ClaudeFailure.filesystem)
            .get()
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

    private static func read(at url: URL) throws(ClaudeFailure) -> JSONDocument<ClaudeAccountFile>? {
        let data: Data?
        do { data = try ifPresent { try Data(contentsOf: url) } } catch { throw .filesystem(error) }
        return try data.map { data throws(ClaudeFailure) in try decode(JSONDocument<ClaudeAccountFile>.self, from: data) }
    }

    private static func decode<Document: Decodable>(_ type: Document.Type, from data: Data) throws(ClaudeFailure) -> Document {
        try Result { try JSONDecoder().decode(type, from: data) }.mapError(ClaudeFailure.invalidCredentials).get()
    }

    // --- [KEYCHAIN_ITEM]

    func readItem() async throws(ClaudeFailure) -> JSONDocument<ClaudeCredentialItem>? {
        let password: Data?
        if let printed = try await security(["find-generic-password", "-a", username, "-s", service, "-w"]) {
            password = Data(printed.utf8)
        } else {
            do { password = try ifPresent { try Data(contentsOf: credentialFile) } } catch { throw .filesystem(error) }
        }
        return try password.map { data throws(ClaudeFailure) in try Self.decode(JSONDocument<ClaudeCredentialItem>.self, from: data) }
    }

    func writeOAuth(
        _ oauth: JSONDocument<ClaudeOAuthToken.Attributes>,
        over item: JSONDocument<ClaudeCredentialItem>?,
    ) async throws(ClaudeFailure) {
        let password = try Result { try JSONEncoder().encode(JSONDocument(fields: item?.fields ?? [:], known: ClaudeCredentialItem(claudeAiOauth: oauth))) }
            .mapError(ClaudeFailure.invalidCredentials)
            .get()
            .hexEncoded()
        let arguments = ["add-generic-password", "-U", "-a", username, "-s", service, "-X", password]
        let line = arguments.map { argument in "\"" + argument.replacing("\\", with: "\\\\").replacing("\"", with: "\\\"") + "\"" }.joined(separator: " ") + "\n"
        _ = if line.utf8.count <= 4032 { try await security(["-i"], input: .string(line)) } else { try await security(Arguments(arguments)) }
        do { _ = try ifPresent { try FileManager.default.removeItem(at: credentialFile) } } catch { throw .filesystem(error) }
    }

    func deleteItem() async throws(ClaudeFailure) {
        _ = try await security(["delete-generic-password", "-a", username, "-s", service])
        do { _ = try ifPresent { try FileManager.default.removeItem(at: credentialFile) } } catch { throw .filesystem(error) }
    }

    private func security(_ arguments: Arguments, input: some InputProtocol = .none) async throws(ClaudeFailure) -> String? {
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
                case .exit(.exited(code(errSecInteractionNotAllowed)), _): throw .interactionNotAllowed
                default: throw ClaudeFailure(process: error)
            }
        }
    }

    // --- [LOCKS]

    static func storageWriteLockfile(in directory: URL) -> URL {
        directory.appending(path: ".storage-write.lock")
    }

    static func withLock<Value: Sendable>(
        directories: [URL],
        _ body: @escaping @Sendable () async throws(ClaudeFailure) -> Value,
    ) async throws(ClaudeFailure) -> Value {
        let refresh = Backoff(count: 5, base: .seconds(1), growth: 1, jitter: 1)
        let storageWrite = Backoff(count: 10, base: .milliseconds(100), cap: .seconds(1))
        let lockfiles = try Set(directories).sorted { first, second in first.path(percentEncoded: false) < second.path(percentEncoded: false) }
            .map { directory throws(ClaudeFailure) in
                try Result { try FileManager.default.createDirectory(at: directory, withIntermediateDirectories: true) }.mapError(ClaudeFailure.filesystem).get()
                let canonical = try Result(catching: directory.canonical).mapError(ClaudeFailure.filesystem).get()
                return [directory.appending(path: ".oauth_refresh.lock"), canonical.appendingPathExtension("lock")].map { url in
                    Lockfile(url, stale: .seconds(60), update: .seconds(5), retry: refresh)
                }
                    + [Lockfile(storageWriteLockfile(in: directory), stale: .seconds(15), retry: storageWrite)]
            }
        return try await Lockfile.withLocks(lockfiles.flatMap(\.self), body)
    }
}
