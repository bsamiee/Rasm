import CryptoKit
import Foundation

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

        static let signedOut: Self = Self(
            accessToken: "",
            refreshToken: "",
            scopes: nil,
            expiresAt: 0,
            refreshTokenExpiresAt: nil,
            subscriptionType: nil,
            rateLimitTier: nil,
        )
    }

    let document: JSONDocument<Attributes>
    let accessToken: String
    let scopes: [String]

    var expiresAt: Date? { Self.date(document.known.expiresAt) }
    var refreshTokenExpiresAt: Date? { Self.date(document.known.refreshTokenExpiresAt) }
    var fingerprint: SHA256Digest { SHA256.hash(data: Data(accessToken.utf8)) }

    static let refreshMargin: TimeInterval = 300

    func needsRefresh(at now: Date) -> Bool {
        expiresAt.map { expiry in expiry <= now.addingTimeInterval(Self.refreshMargin) } ?? false
    }

    static func make(_ document: JSONDocument<Attributes>) -> Result<Self?, ClaudeFailure> {
        switch (document.known.accessToken, document.known.scopes) {
            case (.none, _): .failure(.invalidCredentials)
            case (.some(let token), _) where token.isEmpty || document.known.refreshToken?.isEmpty == true:
                .success(nil)
            case (.some(let token), .some(let scopes)) where !scopes.contains(where: \.isEmpty):
                .success(Self(document: document, accessToken: token, scopes: scopes))
            case (.some, _): .failure(.invalidCredentials)
        }
    }

    private static func date(_ milliseconds: Double?) -> Date? {
        milliseconds.flatMap { value in value > 0 ? Date(timeIntervalSince1970: value / 1000) : nil }
    }
}

nonisolated struct ClaudeTokenRequest: Encodable, Sendable {
    let grantType: String = "refresh_token"
    let refreshToken: String
    let clientID: String = "9d1c250a-e61b-44d9-88ed-5944d1962f5e"
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

    func oauth(replacing token: ClaudeOAuthToken, posted: String, at now: Date) -> Result<ClaudeOAuthToken, ClaudeFailure> {
        ClaudeOAuthToken.make(
            JSONDocument(
                fields: token.document.fields,
                known: ClaudeOAuthToken.Attributes(
                    accessToken: accessToken,
                    refreshToken: refreshToken ?? posted,
                    scopes: scope.map { scope in scope.split(separator: " ").map(String.init) } ?? token.scopes,
                    expiresAt: Self.milliseconds(now.addingTimeInterval(expiresIn)),
                    refreshTokenExpiresAt: refreshTokenExpiresIn.map { interval in
                        Self.milliseconds(now.addingTimeInterval(interval))
                    } ?? token.document.known.refreshTokenExpiresAt,
                    subscriptionType: token.document.known.subscriptionType,
                    rateLimitTier: token.document.known.rateLimitTier,
                ),
            )
        )
        .flatMap { token in token.map(Result.success) ?? .failure(.invalidResponse) }
    }

    func identity(plan: String?) -> Result<AccountIdentity?, ClaudeFailure> {
        if let accountID: String = account?.uuid, let email: String = account?.emailAddress,
            let organizationID: String = organization?.uuid
        {
            AccountIdentity.make(
                accountID: accountID,
                organizationID: organizationID,
                email: email,
                plan: plan,
            )
            .mapError(ClaudeFailure.invalidIdentity)
            .map(Optional.some)
        } else {
            .success(nil)
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

    func identity(plan: String?) -> Result<AccountIdentity, ClaudeFailure> {
        AccountIdentity.make(
            accountID: account.uuid,
            organizationID: organization.uuid,
            email: account.email,
            plan: plan,
        )
        .mapError(ClaudeFailure.invalidIdentity)
    }
}

nonisolated struct ClaudeOAuthAccount: Codable, Sendable {
    let accountUuid: String?
    let emailAddress: String?
    let organizationUuid: String?
    let organizationType: String?

    func identity(plan: String?) -> Result<AccountIdentity, ClaudeFailure> {
        if let accountUuid, let emailAddress {
            AccountIdentity.make(
                accountID: accountUuid,
                organizationID: organizationUuid,
                email: emailAddress,
                plan: plan ?? organizationType,
            )
            .mapError(ClaudeFailure.invalidIdentity)
        } else {
            .failure(.invalidCredentials)
        }
    }
}

nonisolated struct ClaudeCredentialItem: Codable, Sendable {
    let claudeAiOauth: JSONDocument<ClaudeOAuthToken.Attributes>?
}

nonisolated struct ClaudeAccountFile: Decodable, Sendable {
    let oauthAccount: JSONDocument<ClaudeOAuthAccount>?
}

nonisolated struct ClaudeAccountUpdate: Encodable, Sendable {
    let oauthAccount: JSONDocument<ClaudeOAuthAccount>
    let hasCompletedOnboarding: Bool = true
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
    let directory: URL
    let configFile: URL
    let directoryConfigFile: URL
    let configDirectoryPath: String?
    let username: String

    var service: String {
        configDirectoryPath.map { value in
            "Claude Code-credentials-" + SHA256.hash(data: Data(value.utf8)).prefix(4).hexEncoded
        } ?? "Claude Code-credentials"
    }

    // --- [KEYCHAIN_ITEM]
    func read() async -> Result<ClaudeStoreContent, ClaudeFailure> {
        await readItem()
            .flatMap { item -> Result<ClaudeOAuthToken?, ClaudeFailure> in
                item?.known.claudeAiOauth.map(ClaudeOAuthToken.make) ?? .success(nil)
            }
            .flatMap { token -> Result<(token: ClaudeOAuthToken, account: JSONDocument<ClaudeOAuthAccount>?)?, ClaudeFailure> in
                token.map { token in readAccount().map { account in (token, account) } } ?? .success(nil)
            }
            .flatMap { stored -> Result<ClaudeStoreContent, ClaudeFailure> in
                switch stored {
                    case .none: .success(.signedOut)
                    case .some((let token, .none)): .success(.unidentified(token))
                    case .some((let token, .some(let account))):
                        account.known.identity(plan: token.document.known.subscriptionType).map { identity in
                            .credential(ClaudeCredential(token: token, account: account, identity: identity))
                        }
                }
            }
    }

    func readItem() async -> Result<JSONDocument<ClaudeCredentialItem>?, ClaudeFailure> {
        await Keychain.readGenericPassword(service: service, account: username)
            .claude()
            .flatMap { data in
                data.map { data in
                    Self.decode(JSONDocument<ClaudeCredentialItem>.self, from: data)
                        .map(Optional.some)
                } ?? .success(nil)
            }
    }

    func writeOAuth(
        _ oauth: JSONDocument<ClaudeOAuthToken.Attributes>,
        over item: JSONDocument<ClaudeCredentialItem>?,
    ) async -> Result<Void, ClaudeFailure> {
        await Result {
            try JSONEncoder().encode(
                JSONDocument(fields: item?.fields ?? [:], known: ClaudeCredentialItem(claudeAiOauth: oauth))
            )
        }
        .mapError(ClaudeFailure.filesystem)
        .bind { data in await Keychain.writeGenericPassword(service: service, account: username, data: data).claude() }
    }

    func deleteItem() async -> Result<Void, ClaudeFailure> {
        await Keychain.deleteGenericPassword(service: service, account: username).claude()
    }

    // --- [ACCOUNT_FILE]
    func identity() -> Result<AccountIdentity?, ClaudeFailure> {
        readAccount().flatMap { account in
            account.map { account in account.known.identity(plan: nil).map(Optional.some) }
                ?? .success(nil)
        }
    }

    func readAccount() -> Result<JSONDocument<ClaudeOAuthAccount>?, ClaudeFailure> {
        Self.read(ClaudeAccountFile.self, at: existingConfigFile).map { file in file?.oauthAccount }
    }

    func writeAccount(_ account: JSONDocument<ClaudeOAuthAccount>) -> Result<Void, ClaudeFailure> {
        let file: URL = existingConfigFile
        return Self.read([String: JSONValue].self, at: file).flatMap { configuration in
            Self.write(
                JSONDocument(fields: configuration ?? [:], known: ClaudeAccountUpdate(oauthAccount: account)),
                to: file,
            )
        }
    }

    private var existingConfigFile: URL {
        FileManager.default.fileExists(atPath: directoryConfigFile.path)
            ? directoryConfigFile : configFile
    }

    private static func read<Document: Decodable>(
        _ type: Document.Type,
        at url: URL,
    ) -> Result<Document?, ClaudeFailure> {
        ifPresent { try Data(contentsOf: url) }
            .mapError(ClaudeFailure.filesystem)
            .flatMap { data in data.map { data in decode(type, from: data).map(Optional.some) } ?? .success(nil) }
    }

    private static func decode<Document: Decodable>(
        _ type: Document.Type,
        from data: Data,
    ) -> Result<Document, ClaudeFailure> {
        Result { try JSONDecoder().decode(type, from: data) }.mapError { _ in ClaudeFailure.invalidCredentials }
    }

    private static func write(_ document: some Encodable, to url: URL) -> Result<Void, ClaudeFailure> {
        Result {
            try FileManager.default.createDirectory(
                at: url.deletingLastPathComponent(),
                withIntermediateDirectories: true,
            )
            try JSONEncoder().encode(document).write(to: url, options: .atomic)
        }
        .mapError(ClaudeFailure.filesystem)
    }
}
