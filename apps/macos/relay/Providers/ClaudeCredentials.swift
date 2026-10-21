import CryptoKit
import Files
import Foundation
import JSON
import Processes
import Subprocess
import System

// --- [MODELS] --------------------------------------------------------------------------

nonisolated struct ClaudeOAuthToken: Sendable {
    struct Attributes: Codable, Sendable {
        let accessToken: String?
        let refreshToken: String?
        let scopes: [String]?
        let expiresAt: Double?
        let refreshTokenExpiresAt: Double?
        let subscriptionType: String?
        let rateLimitTier: String?
        let clientId: String?

        var signedOut: Self {
            Self(
                accessToken: "",
                refreshToken: "",
                scopes: scopes,
                expiresAt: 0,
                refreshTokenExpiresAt: refreshTokenExpiresAt,
                subscriptionType: subscriptionType,
                rateLimitTier: rateLimitTier,
                clientId: clientId,
            )
        }
    }

    let document: JSONDocument<Attributes>
    let accessToken: String
    let scopes: [String]

    var expiresAt: Date? { Self.date(document.known.expiresAt) }
    var refreshTokenExpiresAt: Date? { Self.date(document.known.refreshTokenExpiresAt) }

    func needsRefresh(at now: Date) -> Bool {
        expiresAt.map { expiry in expiry <= now.addingTimeInterval(300) } ?? false
    }

    static func make(_ document: JSONDocument<Attributes>) throws(ClaudeFailure) -> Self? {
        guard let token: String = document.known.accessToken else { throw .invalidCredentials(nil) }
        guard !token.isEmpty, document.known.refreshToken?.isEmpty != true else { return nil }
        guard let scopes: [String] = document.known.scopes, !scopes.contains(where: \.isEmpty) else { throw .invalidCredentials(nil) }
        return Self(document: document, accessToken: token, scopes: scopes)
    }

    private static func date(_ milliseconds: Double?) -> Date? {
        milliseconds.flatMap { value in value > 0 ? Date(timeIntervalSince1970: value / 1000) : nil }
    }
}

nonisolated struct ClaudeTokenRequest: Encodable, Sendable {
    let grantType: String = "refresh_token"
    let refreshToken: String
    let clientId: String
    let scope: String
}

nonisolated struct ClaudeTokenResponse: Decodable, Sendable {
    struct Account: Decodable, Sendable {
        let uuid: String?
        let emailAddress: String?
    }

    struct Organization: Decodable, Sendable {
        let uuid: String?
    }

    let accessToken: String
    let refreshToken: String?
    let expiresIn: TimeInterval
    let refreshTokenExpiresIn: TimeInterval?
    let scope: String?
    let account: Account?
    let organization: Organization?

    func oauth(replacing token: ClaudeOAuthToken, posted: String, at now: Date) throws(ClaudeFailure) -> ClaudeOAuthToken {
        let attributes: ClaudeOAuthToken.Attributes = ClaudeOAuthToken.Attributes(
            accessToken: accessToken,
            refreshToken: refreshToken ?? posted,
            scopes: scope?.split(separator: " ").map(String.init) ?? token.scopes,
            expiresAt: Self.milliseconds(now.addingTimeInterval(expiresIn)),
            refreshTokenExpiresAt: refreshTokenExpiresIn.map { interval in Self.milliseconds(now.addingTimeInterval(interval)) } ?? token.document.known.refreshTokenExpiresAt,
            subscriptionType: token.document.known.subscriptionType,
            rateLimitTier: token.document.known.rateLimitTier,
            clientId: token.document.known.clientId,
        )
        guard let renewed: ClaudeOAuthToken = try ClaudeOAuthToken.make(JSONDocument(fields: token.document.fields, known: attributes)) else { throw .invalidResponse(.token, nil) }
        return renewed
    }

    func identity(plan: String?) throws(ClaudeFailure) -> AccountIdentity? {
        if let accountID: String = account?.uuid, let email: String = account?.emailAddress, let organizationID: String = organization?.uuid {
            try AccountIdentity.make(accountID: accountID, organizationID: organizationID, email: email, plan: plan).mapError(ClaudeFailure.invalidIdentity).get()
        } else {
            nil
        }
    }

    private static func milliseconds(_ date: Date) -> Double {
        (date.timeIntervalSince1970 * 1000).rounded()
    }
}

nonisolated struct ClaudeProfile: Decodable, Sendable {
    struct Account: Decodable, Sendable {
        let uuid: String
        let email: String
    }

    struct Organization: Decodable, Sendable {
        let uuid: String
    }

    let account: Account
    let organization: Organization

    func identity(plan: String?) throws(ClaudeFailure) -> AccountIdentity {
        try AccountIdentity.make(accountID: account.uuid, organizationID: organization.uuid, email: account.email, plan: plan).mapError(ClaudeFailure.invalidIdentity).get()
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

nonisolated enum ClaudeStoreContent: Sendable {
    case signedOut
    case unidentified(ClaudeOAuthToken)
    case credential(ClaudeCredential)

    var credential: ClaudeCredential? {
        if case .credential(let credential) = self { credential } else { nil }
    }

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
    static let keychainDeadline: Duration = .seconds(15)

    let directory: URL
    let configFile: URL
    let directoryConfigFile: URL
    let configDirectoryPath: String?
    let username: String
    let variables: [Environment.Key: String]

    init(home: URL, environment: [Environment.Key: String], username: String) {
        let defaultDirectory: URL = home.appending(path: ".claude", directoryHint: .isDirectory)
        let configured: URL? = environment["CLAUDE_CONFIG_DIR"].flatMap { value in value.isEmpty ? nil : URL(filePath: value, directoryHint: .isDirectory) }
        configDirectoryPath = (environment["CLAUDE_SECURESTORAGE_CONFIG_DIR"] ?? environment["CLAUDE_CONFIG_DIR"]).flatMap { value in
            value.isEmpty ? nil : value.precomposedStringWithCanonicalMapping
        }
        directory = configDirectoryPath.map { value in URL(filePath: value, directoryHint: .isDirectory) } ?? defaultDirectory
        configFile = (configured ?? home).appending(path: ".claude.json")
        directoryConfigFile = (configured ?? defaultDirectory).appending(path: ".config.json")
        self.username = username
        variables = environment.filter { key, _ in ["CLAUDE_CONFIG_DIR", "CLAUDE_SECURESTORAGE_CONFIG_DIR"].contains(key) }
    }

    var service: String {
        configDirectoryPath.map { value in
            "Claude Code-credentials-" + SHA256.hash(data: Data(value.utf8)).prefix(4).hexEncoded
        } ?? "Claude Code-credentials"
    }

    // --- [KEYCHAIN_ITEM]
    func read() async throws(ClaudeFailure) -> ClaudeStoreContent {
        let token: ClaudeOAuthToken? = try await readItem()?.known.claudeAiOauth.flatMap(ClaudeOAuthToken.make)
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

    func readItem() async throws(ClaudeFailure) -> JSONDocument<ClaudeCredentialItem>? {
        try await Keychain.genericPassword(service: service, account: username, within: Self.keychainDeadline)
            .mapError(ClaudeFailure.keychain)
            .get()
            .map { data throws(ClaudeFailure) in try Self.decode(JSONDocument<ClaudeCredentialItem>.self, from: data) }
    }

    func writeOAuth(
        _ oauth: JSONDocument<ClaudeOAuthToken.Attributes>,
        over item: JSONDocument<ClaudeCredentialItem>?,
    ) async throws(ClaudeFailure) {
        let data: Data = try Result { try JSONEncoder().encode(JSONDocument(fields: item?.fields ?? [:], known: ClaudeCredentialItem(claudeAiOauth: oauth))) }
            .mapError(ClaudeFailure.invalidCredentials)
            .get()
        try await Keychain.setGenericPassword(data, service: service, account: username, within: Self.keychainDeadline).mapError(ClaudeFailure.keychain).get()
    }

    func deleteItem() async throws(ClaudeFailure) {
        try await Keychain.deleteGenericPassword(service: service, account: username, within: Self.keychainDeadline).mapError(ClaudeFailure.keychain).get()
    }

    // --- [ACCOUNT_FILE]
    func identity() throws(ClaudeFailure) -> AccountIdentity? {
        try readAccount()?.known.identity(plan: nil)
    }

    func readAccount() throws(ClaudeFailure) -> JSONDocument<ClaudeOAuthAccount>? {
        try Self.read(at: existingConfigFile)?.known.oauthAccount
    }

    func writeAccount(_ account: JSONDocument<ClaudeOAuthAccount>) async throws(ClaudeFailure) {
        let file: URL = existingConfigFile
        try Result { try FileManager.default.createDirectory(at: file.deletingLastPathComponent(), withIntermediateDirectories: true) }.mapError(ClaudeFailure.filesystem).get()
        let retries: [Duration] = [200, 400, 800, 1600, 3200, 4000].map { delay in .milliseconds(delay) * Double.random(in: 1..<2) }
        try await Lockfile.withLocks([Lockfile(file.appendingPathExtension("lock"), retries: retries)]) { () throws(ClaudeFailure) in
            let fields: [String: JSONValue] = try Self.read(at: file)?.fields ?? [:]
            let mode: FilePermissions? = try ifPresent { try FilePath(file.path(percentEncoded: false)).stat().permissions }.mapError(ClaudeFailure.filesystem).get()
            try Result {
                let document: FileWrapper = FileWrapper(
                    regularFileWithContents: try JSONEncoder().encode(JSONDocument(fields: fields, known: ClaudeAccountFile(oauthAccount: account, hasCompletedOnboarding: true)))
                )
                document.fileAttributes = [FileAttributeKey.posixPermissions.rawValue: Int((mode ?? .ownerReadWrite).rawValue)]
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
        try ifPresent { try Data(contentsOf: url) }
            .mapError(ClaudeFailure.filesystem)
            .get()
            .map { data throws(ClaudeFailure) in try decode(JSONDocument<ClaudeAccountFile>.self, from: data) }
    }

    private static func decode<Document: Decodable>(_ type: Document.Type, from data: Data) throws(ClaudeFailure) -> Document {
        try Result { try JSONDecoder().decode(type, from: data) }.mapError(ClaudeFailure.invalidCredentials).get()
    }
}
