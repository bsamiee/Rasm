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
        let expiresAt: Double?
        let refreshTokenExpiresAt: Double?
    }

    let document: JSONDocument<Attributes>
    let accessToken: String

    init?(_ document: JSONDocument<Attributes>) throws(AccountFailure) {
        guard let token = document.known.accessToken else { throw AccountFailure(subject: "Claude access token", reason: .missing) }
        guard !token.isEmpty, document.known.refreshToken?.isEmpty != true else { return nil }
        (self.document, accessToken) = (document, token)
    }

    var expires: Date? { document.known.expiresAt.map { milliseconds in Date(timeIntervalSince1970: milliseconds / 1000) } }

    var refreshesSoon: Bool { expires.map { expiry in expiry < Date(timeIntervalSinceNow: 600) } ?? true }

    var loginEnds: Date? {
        document.known.refreshTokenExpiresAt.map { milliseconds in max(Date(timeIntervalSince1970: milliseconds / 1000), expires ?? .distantPast) }
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
    let keychainOverflow: JSONValue?
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
    case signedOut(SignInCause)
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
        let item = try await readItem()?.known
        guard let oauth = item?.claudeAiOauth else {
            guard item?.keychainOverflow == nil else { throw AccountFailure(subject: service, reason: .sealed) }
            return .signedOut(.missing)
        }
        guard let token = try ClaudeOAuthToken(oauth) else { return .signedOut(.ended) }
        if let end = token.loginEnds, end <= .now { return .signedOut(.expired(end)) }
        guard let account = try readAccount() else { return .unidentified(token) }
        return .credential(ClaudeCredential(token: token, account: account, identity: try account.known.identity(plan: token.document.known.subscriptionType)))
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
        try await Lockfile.withLocks([Lockfile(file.appendingPathExtension("lock"), retries: retry)]) { () async throws(AccountFailure) in
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

    private static let itemSuffixes: [String] = ["", "-overflow-a", "-overflow-b"]

    func readItem() async throws(AccountFailure) -> JSONDocument<ClaudeCredentialItem>? {
        let password = if let held = try await password(of: service) { held } else { try credentialFile.contents() }
        return try password.map { data throws(AccountFailure) in try JSONDecoder().decode(JSONDocument<ClaudeCredentialItem>.self, from: data, subject: "Claude credential") }
    }

    func writeOAuth(_ oauth: JSONDocument<ClaudeOAuthToken.Attributes>, over item: JSONDocument<ClaudeCredentialItem>?) async throws(AccountFailure) {
        let known = ClaudeCredentialItem(claudeAiOauth: oauth, keychainOverflow: nil)
        try await setPassword(JSONEncoder().encode(JSONDocument(fields: item?.fields ?? [:], known: known), subject: "Claude credential"), of: service)
        try credentialFile.remove()
    }

    func deleteItem() async throws(AccountFailure) {
        for suffix in Self.itemSuffixes { _ = try await security(["delete-generic-password", "-a", username, "-s", service + suffix]) }
        try credentialFile.remove()
    }

    func moveItems(to destination: Self) async throws(AccountFailure) -> Bool {
        var moved = false
        for suffix in Self.itemSuffixes {
            let (source, target) = (service + suffix, destination.service + suffix)
            guard let held = try await password(of: source) else { continue }
            if try await destination.password(of: target) == nil {
                try await destination.setPassword(held, of: target)
                guard try await destination.password(of: target) == held else { throw AccountFailure(subject: target, reason: .missing) }
                moved = true
            }
            _ = try await security(["delete-generic-password", "-a", username, "-s", source])
        }
        return moved
    }

    private func password(of service: String) async throws(AccountFailure) -> Data? {
        try await security(["find-generic-password", "-a", username, "-s", service, "-w"]).map { (printed: String) -> Data in
            let text = printed.hasSuffix("\n") ? printed.dropLast() : printed[...]
            return Data(hexEncoded: text) ?? Data(text.utf8)
        }
    }

    private func setPassword(_ password: Data, of service: String) async throws(AccountFailure) {
        let arguments = ["add-generic-password", "-U", "-a", username, "-s", service, "-X", password.hexEncoded()]
        let line = arguments.map { argument in "\"" + argument.replacing("\\", with: "\\\\").replacing("\"", with: "\\\"") + "\"" }.joined(separator: " ") + "\n"
        _ = if line.utf8.count <= 4032 { try await security(["-i"], input: .string(line)) } else { try await security(Arguments(arguments)) }
    }

    private func security(_ arguments: Arguments, input: some InputProtocol = .none) async throws(AccountFailure) -> String? {
        func code(_ status: OSStatus) -> Int32 { status & 0x00FF_FFFF }
        let invocation = Configuration(
            executable: .path("/usr/bin/security"),
            arguments: arguments,
            environment: .custom([:]),
            workingDirectory: FilePath(URL.homeDirectory.path(percentEncoded: false)),
            platformOptions: .session,
        )
        do throws(ProcessFailure) {
            return try await invocation.run(output: .string(limit: 1 << 20), within: .seconds(15), input: input)
        } catch {
            return switch error.reason {
                case .exit(.exited(code(errSecItemNotFound)), _): nil
                case .exit(.exited(code(errSecInteractionNotAllowed)), _): throw AccountFailure(subject: "Keychain", reason: .interactionNotAllowed)
                case .cancelled: throw AccountFailure(subject: "Keychain", reason: .cancelled)
                default: throw AccountFailure(subject: "Keychain", reason: .process(ProcessFailure(subject: invocation.executable.description, reason: error.reason)))
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
                    Lockfile(url, stale: .seconds(60), update: .seconds(5), retries: refresh)
                }
                    + [Lockfile(storageWriteLockfile(in: directory), stale: .seconds(15), retries: storageWrite)]
            }
        return try await Lockfile.withLocks(lockfiles.flatMap(\.self), body)
    }
}
