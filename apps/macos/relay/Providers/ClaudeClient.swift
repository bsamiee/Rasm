import Files
import Foundation
import JSON
import Processes
import Results
import Subprocess
import System

// --- [MODELS] --------------------------------------------------------------------------

nonisolated struct ClaudePendingSwitch: Codable, Equatable, Sendable {
    let incoming: UUID
    let outgoing: UUID?

    var copies: [UUID] { [incoming, outgoing].compactMap(\.self) }
}

// --- [SERVICES] ------------------------------------------------------------------------

actor ClaudeClient: ProviderClient {
    // --- [STATE]
    nonisolated let shared: ClaudeCredentialStore
    private let paths: FileLocations
    private let environment: [Environment.Key: String]
    private let username: String
    private let session: URLSession
    private var verifiedIdentities: [String: AccountIdentity] = [:]
    private var lastCredential: ClaudeCredential?
    private var installedVersion: Task<Result<Substring, ClaudeFailure>, Never>?

    init(paths: FileLocations, environment: [Environment.Key: String]) {
        self.paths = paths
        self.environment = environment
        let candidate: String = environment["USER"].flatMap { value in value.isEmpty ? nil : value } ?? NSUserName()
        username = candidate.contains(#/^[a-zA-Z0-9._-]+$/#) ? candidate : "claude-code-user"
        let configuration: URLSessionConfiguration = .ephemeral
        configuration.timeoutIntervalForResource = 30
        configuration.requestCachePolicy = .reloadIgnoringLocalCacheData
        session = URLSession(configuration: configuration)
        shared = ClaudeCredentialStore(home: paths.home, environment: environment, username: username)
    }

    private func store(for account: Account, isSelected: Bool) -> ClaudeCredentialStore {
        isSelected ? shared : privateStore(account.id)
    }

    private nonisolated func privateStore(_ id: UUID) -> ClaudeCredentialStore {
        ClaudeCredentialStore(home: paths.home, environment: ["CLAUDE_CONFIG_DIR": FilePath(paths.claudeDirectory(id).path(percentEncoded: false)).string], username: username)
    }

    // --- [SELECTION]
    func currentSelection(selected: Account?) async -> Result<AccountIdentity?, ProviderError> {
        await Result { () async throws(ClaudeFailure) in
            let content: ClaudeStoreContent = try await identified(shared.read(), in: shared)
            let identity: AccountIdentity? = if let held: ClaudeCredential = content.credential { held.identity } else { try shared.identity() }
            if let selected, let parked: ClaudeCredential = lastCredential, parked.identity.isSameAccount(as: selected.identity),
                !(identity.map(parked.identity.isSameAccount(as:)) ?? false)
            {
                let store: ClaudeCredentialStore = privateStore(selected.id)
                try await ClaudeLock.withLock(directories: [store.directory]) { () async throws(ClaudeFailure) in try await store.write(parked, over: store.readItem()) }
            }
            lastCredential = content.credential
            return identity
        }
        .mapError(ProviderError.init(failure:))
    }

    func holdsCredential(for account: Account) async -> Result<Bool, ProviderError> {
        await Result(catching: privateStore(account.id).read)
            .map { content in
                switch content {
                    case .signedOut: false
                    case .unidentified: true
                    case .credential(let credential): credential.identity.isSameAccount(as: account.identity)
                }
            }
            .mapError(ProviderError.init(failure:))
    }

    nonisolated func pendingSwitch() -> Result<ClaudePendingSwitch?, ProviderError> {
        ifPresent { try Data(contentsOf: paths.claudeSelectionFile) }
            .flatMap { data in Result { try data.flatMap { data in try JSONDecoder().decode(ClaudePendingSwitch?.self, from: data) } } }
            .mapError { error in ProviderError(failure: ClaudeFailure.filesystem(error)) }
    }

    func completePendingSwitch(_ pending: ClaudePendingSwitch) async -> Result<Void, ProviderError> {
        await Result { () async throws(ClaudeFailure) in
            try await ClaudeLock.withLock(directories: ([shared] + pending.copies.map(privateStore)).map(\.directory)) { () async throws(ClaudeFailure) in try await self.settle(pending) }
        }
        .mapError(ProviderError.init(failure:))
    }

    private nonisolated func settle(_ pending: ClaudePendingSwitch) async throws(ClaudeFailure) {
        let current: String? = try await shared.read().token?.accessToken
        for copy: ClaudeCredentialStore in pending.copies.map(privateStore) {
            if let duplicate: ClaudeCredential = try await copy.read().credential, duplicate.token.accessToken == current {
                try await shared.writeAccount(duplicate.account)
                try await copy.deleteItem()
            }
        }
        try writePendingSwitch(nil)
    }

    // --- [SWITCH]
    func select(
        _ account: Account,
        outgoing: Account?,
    ) async -> Result<AccountIdentity, ProviderError> {
        let incoming: ClaudeCredentialStore = privateStore(account.id)
        let parking: ClaudeCredentialStore? = outgoing.map { outgoing in privateStore(outgoing.id) }
        let pending: ClaudePendingSwitch = ClaudePendingSwitch(incoming: account.id, outgoing: outgoing?.id)
        return await Result { () async throws(ClaudeFailure) in
            for store: ClaudeCredentialStore in [shared, incoming] where try store.identity() == nil {
                _ = try await identified(store.read(), in: store)
            }
            let last: ClaudeCredential? = lastCredential
            let credential: ClaudeCredential = try await ClaudeLock.withLock(directories: [shared, incoming, parking].compactMap(\.self).map(\.directory)) {
                () async throws(ClaudeFailure) in
                let current: ClaudeStoreContent = try await self.shared.read()
                let stored: ClaudeCredential? = try await incoming.read().credential
                let selected: ClaudeCredential? = current.credential.flatMap { held in held.identity.isSameAccount(as: account.identity) ? held : nil }
                let parked: ClaudeCredential? = outgoing.flatMap { outgoing in [current.credential, last].lazy.compactMap(\.self).first { held in held.identity.isSameAccount(as: outgoing.identity) } }
                guard let entering: ClaudeCredential = selected ?? stored, entering.identity.isSameAccount(as: account.identity) else { throw .signInRequired }
                guard current.token.map({ token in [selected, parked].contains { held in held?.token.accessToken == token.accessToken } }) ?? true else { throw .accountChanged }
                let sharedItem: JSONDocument<ClaudeCredentialItem>? = try await self.shared.readItem()
                let parkingItem: JSONDocument<ClaudeCredentialItem>? = try await parking?.readItem()
                try self.writePendingSwitch(pending)
                let moved: Result<Void, ClaudeFailure> = await Result { () async throws(ClaudeFailure) in
                    if let parked { try await parking?.write(parked, over: parkingItem) }
                    if selected == nil { try await self.shared.write(entering, over: sharedItem) }
                    try await incoming.deleteItem()
                    try self.writePendingSwitch(nil)
                }
                if case .failure(let error) = moved { throw error.releasing(await Result { () async throws(ClaudeFailure) in try await self.settle(pending) }) }
                return entering
            }
            lastCredential = credential
            return try await verify(credential).identity
        }
        .mapError(ProviderError.init(failure:))
    }

    private nonisolated func writePendingSwitch(_ pending: ClaudePendingSwitch?) throws(ClaudeFailure) {
        try Result {
            try FileManager.default.createDirectory(at: paths.applicationSupportDirectory, withIntermediateDirectories: true)
            try JSONEncoder().encode(pending).write(to: paths.claudeSelectionFile, options: .atomic)
        }
        .mapError(ClaudeFailure.filesystem)
        .get()
    }

    // --- [SIGN_IN]
    func signIn(_ target: SignInTarget, codes: AsyncStream<String>) async -> Result<AccountIdentity, ProviderError> {
        let (store, expected): (ClaudeCredentialStore, AccountIdentity?) =
            switch target {
                case .newAccount(let id): (privateStore(id), nil)
                case .account(let account, let isSelected): (store(for: account, isSelected: isSelected), account.identity)
            }
        return await Result { () async throws(ClaudeFailure) in
            try await run(
                Arguments(["auth", "login", "--claudeai"] + (expected.map { identity in ["--email", identity.email] } ?? [])),
                store: store,
                within: .seconds(300),
                output: .discarded,
                input: .sequence(codes.prefix(1).map { code in Data((code + "\n").utf8) }),
            )
            return switch try await store.read() {
                case .credential(let credential) where expected.map(credential.identity.isSameAccount(as:)) ?? true: credential.identity
                case .credential:
                    throw .accountChanged.releasing(await Result { () async throws(ClaudeFailure) in try await run(["auth", "logout"], store: store, within: .seconds(60), output: .discarded) })
                case .unidentified, .signedOut: throw .signInRequired
            }
        }
        .mapError(ProviderError.init(failure:))
    }

    func signOut(_ account: Account, isSelected: Bool) async -> Result<Void, ProviderError> {
        let store: ClaudeCredentialStore = store(for: account, isSelected: isSelected)
        if isSelected { lastCredential = nil }
        return await Result { () async throws(ClaudeFailure) in try await run(["auth", "logout"], store: store, within: .seconds(60), output: .discarded) }.mapError(ProviderError.init(failure:))
    }

    func deletePrivateStore(id: UUID) async -> Result<Void, ProviderError> {
        await Result { () async throws(ClaudeFailure) in try await privateStore(id).deleteItem() }.mapError(ProviderError.init(failure:))
    }

    // --- [USAGE]
    func usage(for account: Account, isSelected: Bool) async -> Result<AccountUsage, ProviderError> {
        func send(_ credential: ClaudeCredential) async throws(ClaudeFailure) -> AccountUsage {
            let headers: [String: String] = ["Authorization": "Bearer \(credential.token.accessToken)", "anthropic-beta": "oauth-2025-04-20", "Accept": Self.jsonMediaType]
            return try await fetch(ClaudeUsageResponse.self, from: .usage, headers: headers).usage(observedAt: Date(), signInExpiresAt: credential.token.refreshTokenExpiresAt)
        }
        return await Result { () async throws(ClaudeFailure) in
            let stored: ClaudeCredential = try await credential(for: account, isSelected: isSelected, replacing: nil)
            let sent: Result<AccountUsage, ClaudeFailure> = await Result { () async throws(ClaudeFailure) in try await send(verify(stored)) }
            guard case .failure(let error) = sent, error.isUnauthorized else { return try sent.get() }
            verifiedIdentities[stored.token.accessToken] = nil
            return try await send(verify(credential(for: account, isSelected: isSelected, replacing: stored)))
        }
        .mapError(ProviderError.init(failure:))
    }

    func greet(_ account: Account, isSelected: Bool) async -> Result<Date?, ProviderError> {
        await Result { () async throws(ClaudeFailure) in
            try await ClaudeSession.run(configuration(store: store(for: account, isSelected: isSelected)), token: credential(for: account, isSelected: isSelected, replacing: nil).token)
        }
        .mapError(ProviderError.init(failure:))
    }

    private func credential(
        for account: Account,
        isSelected: Bool,
        replacing stale: ClaudeCredential?,
    ) async throws(ClaudeFailure) -> ClaudeCredential {
        let store: ClaudeCredentialStore = store(for: account, isSelected: isSelected)
        let stored: ClaudeCredential = try await Self.matched(identified(store.read(), in: store), to: account.identity)
        let current: ClaudeCredential =
            if stored.token.needsRefresh(at: Date()) || stored.token.accessToken == stale?.token.accessToken {
                try await withTaskCancellationShield { () async throws(ClaudeFailure) in
                    try await ClaudeLock.withLock(directories: [store.directory]) { () async throws(ClaudeFailure) in
                        let locked: ClaudeCredential = try await Self.matched(store.read(), to: stored.identity)
                        return locked.token.accessToken == stored.token.accessToken ? try await self.renew(locked, in: store) : locked
                    }
                }
            } else {
                stored
            }
        if isSelected { lastCredential = current }
        return current
    }

    // --- [REFRESH]
    private func renew(
        _ current: ClaudeCredential,
        in store: ClaudeCredentialStore,
    ) async throws(ClaudeFailure) -> ClaudeCredential {
        guard let posted: String = current.token.document.known.refreshToken else { throw .signInRequired }
        let grant: ClaudeTokenRequest = ClaudeTokenRequest(
            refreshToken: posted,
            clientId: current.token.document.known.clientId ?? "9d1c250a-e61b-44d9-88ed-5944d1962f5e",
            scope: current.token.scopes.joined(separator: " "),
        )
        let exchanged: Result<ClaudeTokenResponse, ClaudeFailure> = await Result { () async throws(ClaudeFailure) in
            let body: Data = try Result { try ClaudeEndpoint.encoder.encode(grant) }.mapError(ClaudeFailure.invalidCredentials).get()
            return try await fetch(ClaudeTokenResponse.self, from: .token, headers: ["Content-Type": Self.jsonMediaType], body: body)
        }
        switch exchanged {
            case .success(let response):
                let now: Date = Date()
                let item: JSONDocument<ClaudeCredentialItem>? = try await store.readItem()
                guard let held: String = item?.known.claudeAiOauth?.known.refreshToken, ["", posted].contains(held) else {
                    return try await Self.matched(store.read(), to: current.identity)
                }
                let token: ClaudeOAuthToken = try response.oauth(replacing: current.token, posted: posted, at: now)
                try await store.writeOAuth(token.document, over: item)
                if let identity: AccountIdentity = try response.identity(plan: token.document.known.subscriptionType) { verifiedIdentities[token.accessToken] = identity }
                return ClaudeCredential(token: token, account: current.account, identity: current.identity)
            case .failure(.signInRequired):
                throw .signInRequired.releasing(
                    await Result { () async throws(ClaudeFailure) in
                        let item: JSONDocument<ClaudeCredentialItem>? = try await store.readItem()
                        if let oauth: JSONDocument<ClaudeOAuthToken.Attributes> = item?.known.claudeAiOauth, oauth.known.refreshToken == posted {
                            try await store.writeOAuth(JSONDocument(fields: oauth.fields, known: oauth.known.signedOut), over: item)
                        }
                    }
                )
            case .failure(let error):
                throw error
        }
    }

    // --- [IDENTITY]
    private static func matched(
        _ content: ClaudeStoreContent,
        to identity: AccountIdentity,
    ) throws(ClaudeFailure) -> ClaudeCredential {
        switch content {
            case .credential(let credential) where credential.identity.isSameAccount(as: identity): credential
            case .credential, .unidentified: throw .accountChanged
            case .signedOut: throw .signInRequired
        }
    }

    private func identified(
        _ content: ClaudeStoreContent,
        in store: ClaudeCredentialStore,
    ) async throws(ClaudeFailure) -> ClaudeStoreContent {
        guard case .unidentified(let token) = content else { return content }
        let identity: AccountIdentity = try await profile(of: token)
        let account: JSONDocument<ClaudeOAuthAccount> = JSONDocument(
            fields: [:],
            known: ClaudeOAuthAccount(
                accountUuid: identity.accountID,
                emailAddress: identity.email,
                organizationUuid: identity.organizationID,
                organizationType: token.document.known.subscriptionType,
            ),
        )
        try await store.writeAccount(account)
        verifiedIdentities[token.accessToken] = identity
        return .credential(ClaudeCredential(token: token, account: account, identity: identity))
    }

    private func verify(_ credential: ClaudeCredential) async throws(ClaudeFailure) -> ClaudeCredential {
        let identity: AccountIdentity = if let verified: AccountIdentity = verifiedIdentities[credential.token.accessToken] { verified } else { try await profile(of: credential.token) }
        guard identity.isSameAccount(as: credential.identity) else { throw .accountChanged }
        verifiedIdentities[credential.token.accessToken] = identity
        return credential
    }

    private func profile(of token: ClaudeOAuthToken) async throws(ClaudeFailure) -> AccountIdentity {
        let headers: [String: String] = ["Authorization": "Bearer \(token.accessToken)", "Content-Type": Self.jsonMediaType, "Cache-Control": "no-cache"]
        return try await fetch(ClaudeProfile.self, from: .profile, headers: headers).identity(plan: token.document.known.subscriptionType)
    }

    // --- [HTTP]
    private static let jsonMediaType: String = "application/json"

    private func fetch<Value: Decodable>(
        _: Value.Type,
        from endpoint: ClaudeEndpoint,
        headers: [String: String],
        body: Data? = nil,
    ) async throws(ClaudeFailure) -> Value {
        guard let url: URL = URL(string: endpoint.rawValue) else { throw .invalidResponse(endpoint, nil) }
        let reading: Task<Result<Substring, ClaudeFailure>, Never> =
            installedVersion
            ?? Task {
                await Result { () async throws(ClaudeFailure) in
                    guard let version: Substring = try await run(["--version"], store: shared, within: .seconds(15), output: .string(limit: 1 << 10)).split(whereSeparator: \.isWhitespace).first else {
                        throw .protocolFailure
                    }
                    return version
                }
            }
        installedVersion = reading
        let read: Result<Substring, ClaudeFailure> = await reading.value
        if case .failure = read, installedVersion == reading { installedVersion = nil }
        let installedVersion: Substring = try read.get()
        var request: URLRequest = URLRequest(url: url, timeoutInterval: endpoint.timeout)
        request.httpMethod = body == nil ? "GET" : "POST"
        request.httpBody = body
        request.allHTTPHeaderFields = headers
        request.setValue("claude-cli/\(installedVersion) (external, cli)", forHTTPHeaderField: "User-Agent")
        let (data, response): (Data, URLResponse) = try await Result { try await session.data(for: request) }
            .mapError { error -> ClaudeFailure in
                switch error {
                    case is CancellationError, URLError.cancelled: .deadline(.cancelled)
                    default: .transport(error)
                }
            }
            .get()
        return switch response {
            case let http as HTTPURLResponse where http.statusCode == 200 && Task.isCancelled: throw .deadline(.cancelled)
            case let http as HTTPURLResponse where http.statusCode == 200:
                try Result { try ClaudeEndpoint.decoder.decode(Value.self, from: data) }.mapError { error in ClaudeFailure.invalidResponse(endpoint, error) }.get()
            case let http as HTTPURLResponse:
                throw .rejection(by: endpoint, status: http.statusCode, body: data, retryAfter: http.value(forHTTPHeaderField: "Retry-After"), at: Date())
            default: throw .invalidResponse(endpoint, nil)
        }
    }

    // --- [CHILDREN]
    private func configuration(store: ClaudeCredentialStore, arguments: Arguments = []) throws(ClaudeFailure) -> Configuration {
        try Result {
            try FileManager.default.createDirectory(at: paths.workingDirectory, withIntermediateDirectories: true)
            try FileManager.default.createDirectory(at: store.directory, withIntermediateDirectories: true)
        }
        .mapError(ClaudeFailure.filesystem)
        .get()
        return Configuration(
            executable: .name("claude"),
            arguments: arguments,
            environment: .custom(environment.filter { key, _ in !["ANTHROPIC_", "CLAUDE_", "CCR_"].contains(where: key.rawValue.hasPrefix) }).updating(store.variables),
            workingDirectory: FilePath(paths.workingDirectory.path(percentEncoded: false)),
            platformOptions: .session,
        )
    }

    private func run<Output: OutputProtocol>(
        _ arguments: Arguments,
        store: ClaudeCredentialStore,
        within deadline: Duration,
        output: Output,
        input: some InputProtocol = .none,
    ) async throws(ClaudeFailure) -> Output.OutputType {
        let invocation: Configuration = try configuration(store: store, arguments: arguments)
        let result: ExecutionResult<Void, Output, DiscardedOutput> = try await withTimeout(deadline) { [invocation] in
            await Result { try await Subprocess.run(invocation, input: input, output: output) }.mapError(ClaudeFailure.init(child:))
        }
        .get()
        guard result.terminationStatus.isSuccess else { throw .process(.exit(result.terminationStatus)) }
        return result.standardOutput
    }
}
