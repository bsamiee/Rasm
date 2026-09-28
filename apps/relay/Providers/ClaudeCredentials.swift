import CryptoKit
import Foundation

// --- [MODELS] --------------------------------------------------------------------------

nonisolated struct ClaudeOAuthToken: Sendable {
    struct Attributes: Decodable, Sendable {
        let accessToken: String?
        let refreshToken: String?
        let subscriptionType: String?
        let rateLimitTier: String?

        enum CodingKeys: CodingKey {
            case accessToken, refreshToken, subscriptionType, rateLimitTier
        }

        init(from decoder: any Decoder) {
            let container: KeyedDecodingContainer<CodingKeys>? = try? decoder.container(keyedBy: CodingKeys.self)
            accessToken = try? container?.decodeIfPresent(String.self, forKey: .accessToken)
            refreshToken = try? container?.decodeIfPresent(String.self, forKey: .refreshToken)
            subscriptionType = try? container?.decodeIfPresent(String.self, forKey: .subscriptionType)
            rateLimitTier = try? container?.decodeIfPresent(String.self, forKey: .rateLimitTier)
        }
    }

    struct Grant: Decodable, Sendable {
        let scopes: [String]
        let expiresAt: Double?
        let refreshTokenExpiresAt: Double?
    }

    let fields: [String: JSONValue]
    let accessToken: String
    let refreshToken: String?
    let scopes: [String]
    let expiresAt: Date?
    let refreshTokenExpiresAt: Date?
    let plan: String?
    let rateLimitTier: String?

    var fingerprint: SHA256Digest { SHA256.hash(data: Data(accessToken.utf8)) }

    static let refreshMargin: TimeInterval = 300

    func needsRefresh(at now: Date) -> Bool {
        expiresAt.map { expiry in expiry <= now.addingTimeInterval(Self.refreshMargin) } ?? false
    }

    static func make(_ value: JSONValue) -> Result<Self?, ClaudeFailure> {
        let grant: Result<Grant, ClaudeFailure> = Result { try value.decode(as: Grant.self) }
            .mapError { _ in ClaudeFailure.invalidCredentials }
            .flatMap { grant in
                grant.scopes.contains(where: \.isEmpty) ? .failure(.invalidCredentials) : .success(grant)
            }
        return Result { try value.decode(as: JSONDocument<Attributes>.self) }
            .mapError { _ in ClaudeFailure.invalidCredentials }
            .flatMap { document -> Result<Self?, ClaudeFailure> in
                switch document.known.accessToken {
                    case .none: .failure(.invalidCredentials)
                    case .some(let token) where token.isEmpty || document.known.refreshToken?.isEmpty == true:
                        .success(nil)
                    case .some(let token):
                        grant.map { grant -> Self? in
                            Self(
                                fields: document.fields,
                                accessToken: token,
                                refreshToken: document.known.refreshToken,
                                scopes: grant.scopes,
                                expiresAt: date(grant.expiresAt),
                                refreshTokenExpiresAt: date(grant.refreshTokenExpiresAt),
                                plan: document.known.subscriptionType,
                                rateLimitTier: document.known.rateLimitTier,
                            )
                        }
                }
            }
    }

    private static func date(_ milliseconds: Double?) -> Date? {
        milliseconds.flatMap { value in value > 0 ? Date(timeIntervalSince1970: value / 1000) : nil }
    }
}

nonisolated struct ClaudeOAuthUpdate: Encodable, Sendable {
    static let signedOut: Self = Self(
        accessToken: "",
        refreshToken: "",
        expiresAt: 0,
        refreshTokenExpiresAt: nil,
        scopes: nil,
    )

    let accessToken: String
    let refreshToken: String
    let expiresAt: Double
    let refreshTokenExpiresAt: Double?
    let scopes: [String]?
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

    func oauth(replacing token: ClaudeOAuthToken, posted: String, at now: Date) -> JSONDocument<ClaudeOAuthUpdate> {
        JSONDocument(
            fields: token.fields,
            known: ClaudeOAuthUpdate(
                accessToken: accessToken,
                refreshToken: refreshToken ?? posted,
                expiresAt: Self.milliseconds(now.addingTimeInterval(expiresIn)),
                refreshTokenExpiresAt: refreshTokenExpiresIn.map { interval in
                    Self.milliseconds(now.addingTimeInterval(interval))
                },
                scopes: scope.map { scope in scope.split(separator: " ").map(String.init) },
            ),
        )
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

nonisolated extension ClaudeOAuthAccount {
    init(from decoder: any Decoder) throws {
        let container: KeyedDecodingContainer<CodingKeys> = try decoder.container(keyedBy: CodingKeys.self)
        accountUuid = try? container.decodeIfPresent(String.self, forKey: .accountUuid)
        emailAddress = try? container.decodeIfPresent(String.self, forKey: .emailAddress)
        organizationUuid = try? container.decodeIfPresent(String.self, forKey: .organizationUuid)
        organizationType = try? container.decodeIfPresent(String.self, forKey: .organizationType)
    }
}

nonisolated struct ClaudeCredentialItem<OAuth: Sendable>: Sendable {
    let claudeAiOauth: OAuth?
}

nonisolated extension ClaudeCredentialItem: Decodable where OAuth: Decodable {}

nonisolated extension ClaudeCredentialItem: Encodable where OAuth: Encodable {}

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
    let service: String
    let username: String

    init(
        directory: URL,
        configFile: URL,
        directoryConfigFile: URL,
        configDirectoryPath: String?,
        username: String,
    ) {
        self.directory = directory
        self.configFile = configFile
        self.directoryConfigFile = directoryConfigFile
        self.configDirectoryPath = configDirectoryPath
        service =
            configDirectoryPath.map { value in
                "Claude Code-credentials-" + SHA256.hash(data: Data(value.utf8)).prefix(4).hexEncoded
            } ?? "Claude Code-credentials"
        self.username = username
    }

    // --- [KEYCHAIN_ITEM]
    func read() async -> Result<ClaudeStoreContent, ClaudeFailure> {
        await readItem().flatMap { item -> Result<ClaudeStoreContent, ClaudeFailure> in
            switch item?.known.claudeAiOauth {
                case .none: .success(.signedOut)
                case .some(let oauth):
                    ClaudeOAuthToken.make(oauth).flatMap { token in token.map(content(of:)) ?? .success(.signedOut) }
            }
        }
    }

    func readItem() async -> Result<JSONDocument<ClaudeCredentialItem<JSONValue>>?, ClaudeFailure> {
        await Keychain.readGenericPassword(service: service, account: username)
            .claude()
            .flatMap { data in
                data.map { data in
                    Self.decode(JSONDocument<ClaudeCredentialItem<JSONValue>>.self, from: data).map(Optional.some)
                } ?? .success(nil)
            }
    }

    func writeOAuth(
        _ oauth: some Encodable & Sendable,
        over item: JSONDocument<ClaudeCredentialItem<JSONValue>>?,
    ) async -> Result<Void, ClaudeFailure> {
        await Result {
            try JSONEncoder().encode(
                JSONDocument(fields: item?.fields ?? [:], known: ClaudeCredentialItem(claudeAiOauth: oauth))
            )
        }
        .mapError(ClaudeFailure.filesystem)
        .bind { data in
            await Keychain.writeGenericPassword(service: service, account: username, data: data)
                .claude()
        }
    }

    func deleteItem() async -> Result<Void, ClaudeFailure> {
        await Keychain.deleteGenericPassword(service: service, account: username).claude()
    }

    private func content(of token: ClaudeOAuthToken) -> Result<ClaudeStoreContent, ClaudeFailure> {
        readAccount().flatMap { account in
            account.map { account in
                account.known.identity(plan: token.plan).map { identity in
                    .credential(ClaudeCredential(token: token, account: account, identity: identity))
                }
            } ?? .success(.unidentified(token))
        }
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
