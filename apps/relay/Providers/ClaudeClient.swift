import CryptoKit
import Foundation
import Subprocess

// --- [SERVICES] ------------------------------------------------------------------------

actor ClaudeClient: ProviderClient {
    // --- [STATE]
    nonisolated let shared: ClaudeCredentialStore
    private let paths: FileLocations
    private let environment: [String: String]
    private let username: String
    private let session: URLSession
    private var verifiedIdentities: [SHA256Digest: AccountIdentity] = [:]
    private var lastCredential: ClaudeCredential?
    private var lastSelection: AccountIdentity?
    private var userAgentLookup: Task<String?, Never>?

    init(paths: FileLocations, environment: [String: String]) {
        self.paths = paths
        self.environment = environment
        let candidate: String = environment["USER"] ?? NSUserName()
        username = candidate.contains(/^[a-zA-Z0-9._-]+$/) ? candidate : "claude-code-user"
        let configuration: URLSessionConfiguration = .ephemeral
        configuration.timeoutIntervalForResource = 30
        configuration.requestCachePolicy = .reloadIgnoringLocalCacheData
        session = URLSession(configuration: configuration)
        let configured: String? = environment["CLAUDE_CONFIG_DIR"].flatMap { value in
            value.isEmpty ? nil : value
        }
        let secure: String? = environment["CLAUDE_SECURESTORAGE_CONFIG_DIR"]
        let defaultDirectory: URL = paths.home.appending(path: ".claude", directoryHint: .isDirectory)
        let configDirectory: URL =
            configured.map { value in URL(filePath: value, directoryHint: .isDirectory) } ?? defaultDirectory
        shared = ClaudeCredentialStore(
            directory: secure.map { value in
                value.isEmpty
                    ? defaultDirectory
                    : URL(filePath: value.precomposedStringWithCanonicalMapping, directoryHint: .isDirectory)
            } ?? configDirectory,
            configFile: (configured == nil ? paths.home : configDirectory).appending(path: ".claude.json"),
            directoryConfigFile: configDirectory.appending(path: ".config.json"),
            configDirectoryPath: (secure.map { value in value.isEmpty ? nil : value } ?? configured)?
                .precomposedStringWithCanonicalMapping,
            username: username,
        )
    }

    private func store(for account: Account, isSelected: Bool) -> ClaudeCredentialStore {
        isSelected ? shared : privateStore(account.id)
    }

    private func privateStore(_ id: UUID) -> ClaudeCredentialStore {
        let directory: URL = paths.claudeDirectory(id)
        return ClaudeCredentialStore(
            directory: directory,
            configFile: directory.appending(path: ".claude.json"),
            directoryConfigFile: directory.appending(path: ".config.json"),
            configDirectoryPath: directory.path.precomposedStringWithCanonicalMapping,
            username: username,
        )
    }

    // --- [SELECTION]
    func completePendingSwitch() async -> Result<Void, ClaudeFailure> {
        await ClaudeSelectionState.read(at: paths.claudeSelectionFile).bind {
            state -> Result<Void, ClaudeFailure> in
            if let pending: ClaudePendingSwitch = state.pending { await completePendingSwitch(pending) } else { .success(()) }
        }
    }

    func currentSelection(known: [Account]) async -> Result<AccountIdentity?, ProviderError> {
        await identity(of: shared).bind { identity -> Result<AccountIdentity?, ClaudeFailure> in
            guard let identity else { return .success(nil) }
            return await saveLastCredential(incoming: identity, known: known).bind { _ in
                lastSelection = identity
                if case .success(.credential(let held)) = await shared.read(),
                    held.identity.isSameAccount(as: identity)
                {
                    lastCredential = held
                }
                return .success(identity)
            }
        }
        .mapError(ProviderError.init(failure:))
    }

    func holdsCredential(for account: Account) async -> Result<Bool, ProviderError> {
        await privateStore(account.id).read().map { content in
            switch content {
                case .signedOut: false
                case .unidentified: true
                case .credential(let credential): credential.identity.isSameAccount(as: account.identity)
            }
        }
        .mapError(ProviderError.init(failure:))
    }

    private func completePendingSwitch(_ pending: ClaudePendingSwitch) async -> Result<Void, ClaudeFailure> {
        let copies: [ClaudeCredentialStore] = [pending.incoming, pending.outgoing].compactMap(\.self).map(privateStore)
        return await ClaudeLock.withLock(directories: ([shared] + copies).map(\.directory)) {
            await self.shared.read()
                .bind { current in await self.deleteDuplicates(of: current.token, among: copies) }
                .bind { _ in await self.writePendingSwitch(nil) }
        }
    }

    private func deleteDuplicates(
        of token: ClaudeOAuthToken?,
        among copies: [ClaudeCredentialStore],
    ) async -> Result<Void, ClaudeFailure> {
        guard let token, let copy: ClaudeCredentialStore = copies.first else { return .success(()) }
        return await copy.read()
            .bind { content -> Result<Void, ClaudeFailure> in
                if let duplicate: ClaudeCredential = content.credential,
                    duplicate.token.accessToken == token.accessToken
                {
                    await shared.writeAccount(duplicate.account).bind { _ in await copy.deleteItem() }
                } else {
                    .success(())
                }
            }
            .bind { _ in await deleteDuplicates(of: token, among: Array(copies.dropFirst())) }
    }

    private func saveLastCredential(
        incoming identity: AccountIdentity,
        known: [Account],
    ) async -> Result<Void, ClaudeFailure> {
        guard let previous: AccountIdentity = lastSelection, !previous.isSameAccount(as: identity),
            let outgoing: Account = known.first(where: { account in
                account.provider == .claude && account.identity.isSameAccount(as: previous)
            }),
            let credential: ClaudeCredential = lastCredential,
            credential.identity.isSameAccount(as: previous)
        else {
            if !(lastSelection?.isSameAccount(as: identity) ?? false) { lastCredential = nil }
            return .success(())
        }
        let store: ClaudeCredentialStore = privateStore(outgoing.id)
        return await ClaudeLock.withLock(directories: [store.directory]) {
            await store.read().bind { existing -> Result<Void, ClaudeFailure> in
                let keepsExisting: Bool =
                    existing.token.map { held in
                        (held.expiresAt ?? .distantPast) >= (credential.token.expiresAt ?? .distantPast)
                    } ?? false
                return await keepsExisting ? .success(()) : self.write(credential, into: store)
            }
        }
        .map { _ in lastCredential = nil }
    }

    // --- [SWITCH]
    func select(
        _ account: Account,
        candidates: [Account],
    ) async -> Result<AccountIdentity, ProviderError> {
        let incoming: ClaudeCredentialStore = privateStore(account.id)
        return await identity(of: shared)
            .bind { _ in await identity(of: incoming) }
            .bind { _ in
                await ClaudeLock.withLock(directories: [shared.directory, incoming.directory]) {
                    await self.switchCredential(account, candidates: candidates, incoming: incoming)
                }
            }
            .bind { credential in await verify(credential).map(\.identity) }
            .mapError(ProviderError.init(failure:))
    }

    private func switchCredential(
        _ account: Account,
        candidates: [Account],
        incoming: ClaudeCredentialStore,
    ) async -> Result<ClaudeCredential, ClaudeFailure> {
        await shared.read().bind { before -> Result<ClaudeCredential, ClaudeFailure> in
            if case .unidentified = before { return .failure(.accountChanged) }
            let outgoing: (id: UUID, credential: ClaudeCredential)? = before.credential.flatMap { current in
                candidates.first { candidate in
                    candidate.provider == .claude && candidate.id != account.id
                        && candidate.identity.isSameAccount(as: current.identity)
                }
                .map { candidate in (candidate.id, current) }
            }
            return await ClaudeLock.withLock(
                directories: [outgoing.map { copy in privateStore(copy.id).directory }].compactMap(\.self)
            ) {
                await incoming.read().bind { content -> Result<ClaudeCredential, ClaudeFailure> in
                    await self.install(
                        content.credential,
                        replacing: before.credential,
                        for: account,
                        saving: outgoing,
                        incoming: incoming,
                    )
                }
            }
        }
    }

    private func install(
        _ credential: ClaudeCredential?,
        replacing current: ClaudeCredential?,
        for account: Account,
        saving outgoing: (id: UUID, credential: ClaudeCredential)?,
        incoming: ClaudeCredentialStore,
    ) async -> Result<ClaudeCredential, ClaudeFailure> {
        guard let credential, credential.identity.isSameAccount(as: account.identity) else {
            return .failure(.signInRequired)
        }
        if let current, current.identity.isSameAccount(as: account.identity) {
            return await incoming.deleteItem().map { _ in current }
        }
        return await writePendingSwitch(ClaudePendingSwitch(incoming: account.id, outgoing: outgoing?.id))
            .bind { _ in
                if let outgoing { await self.write(outgoing.credential, into: privateStore(outgoing.id)) } else { .success(()) }
            }
            .bind { _ in await self.write(credential, into: shared) }
            .bind { _ in await incoming.deleteItem() }
            .flatMap { _ in self.writePendingSwitch(nil) }
            .map { _ in
                lastCredential = credential
                lastSelection = credential.identity
                return credential
            }
    }

    private func write(
        _ credential: ClaudeCredential,
        into store: ClaudeCredentialStore,
    ) async -> Result<Void, ClaudeFailure> {
        await store.readItem()
            .bind { item in await store.writeOAuth(credential.token.document, over: item) }
            .flatMap { _ in store.writeAccount(credential.account) }
    }

    private func writePendingSwitch(_ pending: ClaudePendingSwitch?) -> Result<Void, ClaudeFailure> {
        ClaudeSelectionState(pending: pending).write(to: paths.claudeSelectionFile)
    }

    // --- [SIGN_IN]
    func connect(
        id: UUID,
        codes: AsyncStream<String>,
    ) async -> Result<AccountIdentity, ProviderError> {
        let store: ClaudeCredentialStore = privateStore(id)
        return await authenticate(store: store, email: nil, codes: codes)
            .bind { _ in await store.read() }
            .flatMap { content in
                content.credential.map { credential in .success(credential.identity) }
                    ?? .failure(.signInRequired)
            }
            .mapError(ProviderError.init(failure:))
    }

    func reconnect(
        account: Account,
        isSelected: Bool,
        codes: AsyncStream<String>,
    ) async -> Result<AccountIdentity, ProviderError> {
        let store: ClaudeCredentialStore = store(for: account, isSelected: isSelected)
        return await authenticate(store: store, email: account.identity.email, codes: codes)
            .bind { _ in await store.read() }
            .bind { content -> Result<AccountIdentity, ClaudeFailure> in
                switch content {
                    case .credential(let credential) where credential.identity.isSameAccount(as: account.identity):
                        .success(credential.identity)
                    case .credential: .failure(.accountChanged.releasing(await logOut(store: store)))
                    case .unidentified, .signedOut: .failure(.signInRequired)
                }
            }
            .mapError(ProviderError.init(failure:))
    }

    func signOut(_ account: Account, isSelected: Bool) async -> Result<Void, ProviderError> {
        let store: ClaudeCredentialStore = store(for: account, isSelected: isSelected)
        if isSelected { lastCredential = nil }
        return await logOut(store: store).mapError(ProviderError.init(failure:))
    }

    func remove(_ account: Account, isSelected: Bool) async -> Result<Void, ProviderError> {
        await signOut(account, isSelected: isSelected).bind { _ in
            await deletePrivateStore(id: account.id)
        }
    }

    func deletePrivateStore(id: UUID) async -> Result<Void, ProviderError> {
        let store: ClaudeCredentialStore = privateStore(id)
        return await store.deleteItem()
            .flatMap { _ in
                ifPresent { try FileManager.default.removeItem(at: store.directory) }.map { _ in () }
                    .mapError(ClaudeFailure.filesystem)
            }
            .mapError(ProviderError.init(failure:))
    }

    private func authenticate(
        store: ClaudeCredentialStore,
        email: String?,
        codes: AsyncStream<String>,
    ) async -> Result<Void, ClaudeFailure> {
        await createDirectories(store: store)
            .bind { _ in await executable() }
            .bind { binary -> Result<Void, ClaudeFailure> in
                await ProcessRun.stream(
                    ProcessRun.configuration(
                        executable: binary,
                        arguments: ["auth", "login", "--claudeai"] + (email.map { email in ["--email", email] } ?? []),
                        environment: processEnvironment(store: store),
                        workingDirectory: paths.workingDirectory,
                    ),
                    deadline: .seconds(300),
                ) { execution in
                    await withTaskGroup { group in
                        group.addTask(name: "Login code") {
                            for await code: String in codes {
                                return await Result {
                                    _ = try await execution.standardInputWriter.write(code + "\n", using: UTF8.self)
                                    try await execution.standardInputWriter.finish()
                                }
                                .mapError(ProcessRun.failure)
                            }
                            return .success(())
                        }
                        _ = await Result { for try await _ in execution.standardOutput.strings() {} }
                        group.cancelAll()
                        let delivered: Result<Void, ProcessFailure> = await group.next() ?? .success(())
                        return Task.isCancelled ? .failure(.cancelled) : delivered
                    }
                }
                .mapError(ClaudeFailure.init(process:))
                .flatMap { _, status in
                    status.isSuccess ? .success(()) : .failure(.authenticationFailed(status))
                }
            }
    }

    private func logOut(store: ClaudeCredentialStore) async -> Result<Void, ClaudeFailure> {
        await createDirectories(store: store)
            .bind { _ in await executable() }
            .bind { binary in
                await ProcessRun.collect(
                    ProcessRun.configuration(
                        executable: binary,
                        arguments: ["auth", "logout"],
                        environment: processEnvironment(store: store),
                        workingDirectory: paths.workingDirectory,
                    ),
                    deadline: .seconds(60),
                ).mapError(ClaudeFailure.init(process:))
            }
            .bind { _ in await store.deleteItem() }
    }

    // --- [USAGE]
    func usage(for account: Account, isSelected: Bool) async -> Result<AccountUsage, ProviderError> {
        await request(for: account, isSelected: isSelected) { credential in
            await fetch(
                .usage,
                headers: [
                    "Authorization": "Bearer \(credential.token.accessToken)", "anthropic-beta": "oauth-2025-04-20",
                    "Accept": Self.jsonMediaType,
                ],
            )
            .flatMap { data in decode(ClaudeUsageResponse.self, from: data) }
            .flatMap { usage in
                usage.usage(observedAt: Date(), signInExpiresAt: credential.token.refreshTokenExpiresAt)
            }
        }
        .mapError(ProviderError.init(failure:))
    }

    func startSession(
        for account: Account,
        isSelected: Bool,
    ) async -> Result<AccountUsage, ProviderError> {
        let store: ClaudeCredentialStore = store(for: account, isSelected: isSelected)
        return await usage(for: account, isSelected: isSelected).bind { current in
            guard case .ready = current.availability(at: Date()) else { return .success(current) }
            return await credential(for: account, isSelected: isSelected, replacing: nil)
                .bind { stored in
                    await createDirectories(store: store).bind { _ in await executable() }.bind { binary in
                        await ClaudeSession.run(
                            executable: binary,
                            token: stored.token,
                            environment: processEnvironment(store: store),
                            workingDirectory: paths.workingDirectory,
                        )
                    }
                }
                .mapError(ProviderError.init(failure:))
                .bind { reset in
                    await usage(for: account, isSelected: isSelected).map { started in
                        started.settingSessionReset(reset)
                    }
                }
        }
    }

    private func credential(
        for account: Account,
        isSelected: Bool,
        replacing stale: ClaudeCredential?,
    ) async -> Result<ClaudeCredential, ClaudeFailure> {
        let store: ClaudeCredentialStore = store(for: account, isSelected: isSelected)
        return await store.read()
            .bind { content in await identified(content, in: store) }
            .flatMap { content in Self.matched(content, to: account.identity) }
            .bind { credential -> Result<ClaudeCredential, ClaudeFailure> in
                await credential.token.needsRefresh(at: Date())
                    || credential.token.accessToken == stale?.token.accessToken
                    ? refreshCredential(in: store, for: credential) : .success(credential)
            }
            .map { credential in
                if isSelected { lastCredential = credential }
                return credential
            }
    }

    private func request<Value: Sendable>(
        for account: Account,
        isSelected: Bool,
        _ send: (ClaudeCredential) async -> Result<Value, ClaudeFailure>,
    ) async -> Result<Value, ClaudeFailure> {
        await credential(for: account, isSelected: isSelected, replacing: nil).bind { stored in
            let sent: Result<Value, ClaudeFailure> = await verify(stored).bind(send)
            guard case .failure(let error) = sent, error.isUnauthorized else { return sent }
            verifiedIdentities[stored.token.fingerprint] = nil
            return await credential(for: account, isSelected: isSelected, replacing: stored).bind { renewed in
                await verify(renewed).bind(send)
            }
        }
    }

    // --- [REFRESH]
    private nonisolated func refreshCredential(
        in store: ClaudeCredentialStore,
        for stale: ClaudeCredential,
    ) async -> Result<ClaudeCredential, ClaudeFailure> {
        await withTaskCancellationShield {
            await ClaudeLock.withLock(directories: [store.directory]) {
                await store.read()
                    .flatMap { content in Self.matched(content, to: stale.identity) }
                    .bind { current -> Result<ClaudeCredential, ClaudeFailure> in
                        await current.token.accessToken == stale.token.accessToken
                            ? self.renew(current, in: store) : .success(current)
                    }
            }
        }
    }

    private func renew(
        _ current: ClaudeCredential,
        in store: ClaudeCredentialStore,
    ) async -> Result<ClaudeCredential, ClaudeFailure> {
        guard let posted: String = current.token.document.known.refreshToken else { return .failure(.signInRequired) }
        let grant: ClaudeTokenRequest = ClaudeTokenRequest(
            refreshToken: posted,
            scope: current.token.scopes.joined(separator: " "),
        )
        let exchanged: Result<ClaudeTokenResponse, ClaudeFailure> = await Result {
            let encoder: JSONEncoder = JSONEncoder()
            encoder.keyEncodingStrategy = .convertToSnakeCase
            return try encoder.encode(grant)
        }
        .mapError { _ in ClaudeFailure.invalidCredentials }
        .bind { body in await fetch(.token, headers: ["Content-Type": Self.jsonMediaType], body: body) }
        .flatMap { data in decode(ClaudeTokenResponse.self, from: data) }
        switch exchanged {
            case .success(let response):
                return await save(response, replacing: current, posted: posted, in: store)
            case .failure(.signInRequired):
                let signedOut: Result<Void, ClaudeFailure> = await store.readItem().bind {
                    item -> Result<Void, ClaudeFailure> in
                    if let oauth: JSONDocument<ClaudeOAuthToken.Attributes> = item?.known.claudeAiOauth,
                        oauth.known.refreshToken == posted
                    {
                        await store.writeOAuth(JSONDocument(fields: oauth.fields, known: ClaudeOAuthToken.Attributes.signedOut), over: item)
                    } else {
                        .success(())
                    }
                }
                return .failure(.signInRequired.releasing(signedOut))
            case .failure(let error):
                return .failure(error)
        }
    }

    private func save(
        _ response: ClaudeTokenResponse,
        replacing current: ClaudeCredential,
        posted: String,
        in store: ClaudeCredentialStore,
    ) async -> Result<ClaudeCredential, ClaudeFailure> {
        let now: Date = Date()
        return await store.readItem().bind { item -> Result<ClaudeCredential, ClaudeFailure> in
            if let stored: String = item?.known.claudeAiOauth?.known.refreshToken, !stored.isEmpty, stored != posted {
                return await store.read().flatMap { content in Self.matched(content, to: current.identity) }
            }
            return await response.oauth(replacing: current.token, posted: posted, at: now)
                .bind { token in
                    await store.writeOAuth(token.document, over: item).map { _ in
                        ClaudeCredential(token: token, account: current.account, identity: current.identity)
                    }
                }
                .flatMap { renewed in response.identity(plan: renewed.token.document.known.subscriptionType).map { identity in (renewed, identity) } }
                .map { renewed, identity in
                    if let identity { verifiedIdentities[renewed.token.fingerprint] = identity }
                    return renewed
                }
        }
    }

    // --- [IDENTITY]
    private static func matched(
        _ content: ClaudeStoreContent,
        to identity: AccountIdentity,
    ) -> Result<ClaudeCredential, ClaudeFailure> {
        switch content {
            case .credential(let credential) where credential.identity.isSameAccount(as: identity):
                .success(credential)
            case .credential, .unidentified: .failure(.accountChanged)
            case .signedOut: .failure(.signInRequired)
        }
    }

    private func identity(of store: ClaudeCredentialStore) async -> Result<AccountIdentity?, ClaudeFailure> {
        await store.identity().bind { identity -> Result<AccountIdentity?, ClaudeFailure> in
            identity == nil
                ? await store.read()
                    .bind { content in await identified(content, in: store) }
                    .map { content in content.credential?.identity }
                : .success(identity)
        }
    }

    private func identified(
        _ content: ClaudeStoreContent,
        in store: ClaudeCredentialStore,
    ) async -> Result<ClaudeStoreContent, ClaudeFailure> {
        if case .unidentified(let token) = content {
            await profile(of: token).flatMap { identity -> Result<ClaudeStoreContent, ClaudeFailure> in
                let account: JSONDocument<ClaudeOAuthAccount> = JSONDocument(
                    fields: [:],
                    known: ClaudeOAuthAccount(
                        accountUuid: identity.accountID,
                        emailAddress: identity.email,
                        organizationUuid: identity.organizationID,
                        organizationType: token.document.known.subscriptionType,
                    ),
                )
                return store.writeAccount(account).map { _ in
                    verifiedIdentities[token.fingerprint] = identity
                    return .credential(ClaudeCredential(token: token, account: account, identity: identity))
                }
            }
        } else {
            .success(content)
        }
    }

    private func verify(_ credential: ClaudeCredential) async -> Result<ClaudeCredential, ClaudeFailure> {
        let fingerprint: SHA256Digest = credential.token.fingerprint
        if let identity: AccountIdentity = verifiedIdentities[fingerprint] {
            return identity.isSameAccount(as: credential.identity)
                ? .success(credential) : .failure(.accountChanged)
        }
        return await profile(of: credential.token).flatMap { identity -> Result<ClaudeCredential, ClaudeFailure> in
            guard identity.isSameAccount(as: credential.identity) else {
                return .failure(.accountChanged)
            }
            verifiedIdentities[fingerprint] = identity
            return .success(credential)
        }
    }

    private func profile(of token: ClaudeOAuthToken) async -> Result<AccountIdentity, ClaudeFailure> {
        await fetch(
            .profile,
            headers: [
                "Authorization": "Bearer \(token.accessToken)",
                "Content-Type": Self.jsonMediaType, "Cache-Control": "no-cache",
            ],
        )
        .flatMap { data in decode(ClaudeProfile.self, from: data) }
        .flatMap { profile in profile.identity(plan: token.document.known.subscriptionType) }
    }

    // --- [HTTP]
    private static let jsonMediaType: String = "application/json"

    private func fetch(
        _ endpoint: ClaudeEndpoint,
        headers: [String: String],
        body: Data? = nil,
    ) async -> Result<Data, ClaudeFailure> {
        guard let url: URL = URL(string: endpoint.rawValue) else { return .failure(.invalidResponse) }
        var request: URLRequest = URLRequest(url: url, timeoutInterval: endpoint.timeout)
        request.httpMethod = body == nil ? "GET" : "POST"
        request.httpBody = body
        request.allHTTPHeaderFields = headers
        request.setValue(await userAgent(), forHTTPHeaderField: "User-Agent")
        let response: Result<(Data, URLResponse), any Error> = await Result {
            try await session.data(for: request)
        }
        return response.mapError { error -> ClaudeFailure in
            switch error {
                case is CancellationError: .cancelled
                case let error as URLError where error.code == .cancelled: .cancelled
                default: .transport(error)
            }
        }
        .flatMap { data, response -> Result<Data, ClaudeFailure> in
            switch response {
                case let http as HTTPURLResponse where http.statusCode == 200 && Task.isCancelled: .failure(.cancelled)
                case let http as HTTPURLResponse where http.statusCode == 200: .success(data)
                case let http as HTTPURLResponse:
                    .failure(
                        .rejection(
                            by: endpoint,
                            status: http.statusCode,
                            body: data,
                            retryAfter: http.value(forHTTPHeaderField: "Retry-After"),
                            at: Date(),
                        )
                    )
                default: .failure(.invalidResponse)
            }
        }
    }

    private func decode<Value: Decodable>(
        _ type: Value.Type,
        from data: Data,
    ) -> Result<Value, ClaudeFailure> {
        Result { try ClaudeEndpoint.decoder.decode(type, from: data) }.mapError { _ in .invalidResponse }
    }

    private func userAgent() async -> String? {
        if let userAgentLookup { return await userAgentLookup.value }
        let lookup: Task<String?, Never> = Task(name: "Claude version") {
            [environment = processEnvironment(store: shared), directory = paths.workingDirectory] in
            await Self.userAgent(
                running: await createDirectories(store: shared).bind { _ in await executable() },
                environment: environment,
                in: directory,
            )
        }
        userAgentLookup = lookup
        return await lookup.value
    }

    private static func userAgent(
        running executable: Result<Executable, ClaudeFailure>,
        environment: [String: String],
        in directory: URL,
    ) async -> String? {
        let printed: Result<ExecutionResult<Void, StringOutput<UTF8>, StringOutput<UTF8>>, ClaudeFailure> = await executable.bind { binary in
            await ProcessRun.collect(
                ProcessRun.configuration(
                    executable: binary,
                    arguments: ["--version"],
                    environment: environment,
                    workingDirectory: directory,
                ),
                deadline: .seconds(15),
            ).mapError(ClaudeFailure.init(process:))
        }
        return
            if case .success(let output) = printed, output.terminationStatus.isSuccess,
            let version: Substring = output.standardOutput.split(whereSeparator: \.isWhitespace).first
        {
            "claude-cli/\(version) (external, cli)"
        } else {
            nil
        }
    }

    // --- [CHILDREN]
    private func executable() async -> Result<Executable, ClaudeFailure> {
        let directories: [String] =
            [".local/bin", ".bun/bin", ".npm-global/bin", ".volta/bin"].map { path in paths.home.appending(path: path).path }
            + ["/opt/homebrew/bin", "/usr/local/bin"] + [environment["PATH"]].compactMap(\.self)
        return await Result {
            try await Executable.name("claude").resolveExecutablePath(in: .custom(["PATH": directories.joined(separator: ":")]))
        }
        .map(Executable.path)
        .mapError { _ in .executableMissing }
    }

    private func processEnvironment(store: ClaudeCredentialStore) -> [String: String] {
        let configDirectory: String? = store.configDirectoryPath.flatMap { path in
            store.service != shared.service || store.configFile != shared.configFile ? path : nil
        }
        let variables: [String: String?] = ["CLAUDE_CONFIG_DIR": configDirectory, "CLAUDE_CODE_DISABLE_AUTO_MEMORY": "1"]
        let excludedEnvironmentVariables: Set<String> = [
            "ANTHROPIC_API_KEY", "ANTHROPIC_AUTH_TOKEN", "ANTHROPIC_BASE_URL", "ANTHROPIC_MODEL",
            "CLAUDE_CODE_OAUTH_TOKEN", "CLAUDE_CODE_OAUTH_TOKEN_FILE_DESCRIPTOR", "CCR_OAUTH_TOKEN_FILE",
            "CLAUDE_CODE_OAUTH_REFRESH_TOKEN", "CLAUDE_CODE_OAUTH_SCOPES", "CLAUDE_CODE_OAUTH_CLIENT_ID",
            "CLAUDE_CODE_USE_BEDROCK", "CLAUDE_CODE_USE_VERTEX", "CLAUDE_CODE_USE_FOUNDRY",
            "CLAUDE_CODE_SIMPLE", "CLAUDE_CODE_REMOTE_SESSION_ID", "CLAUDE_CODE_REMOTE",
            "ANTHROPIC_UNIX_SOCKET", "CLAUDE_CODE_SUBSCRIPTION_TYPE", "CLAUDE_CODE_RATE_LIMIT_TIER",
            "CLAUDE_CODE_API_KEY_FILE_DESCRIPTOR",
        ]
        return environment.filter { entry in
            !excludedEnvironmentVariables.contains(entry.key)
                && (configDirectory == nil || entry.key != "CLAUDE_SECURESTORAGE_CONFIG_DIR")
        }
        .merging(variables.compactMapValues(\.self)) { _, new in new }
    }

    private func createDirectories(store: ClaudeCredentialStore) -> Result<Void, ClaudeFailure> {
        Result {
            try FileManager.default.createDirectory(
                at: paths.workingDirectory,
                withIntermediateDirectories: true,
            )
            try FileManager.default.createDirectory(
                at: store.directory,
                withIntermediateDirectories: true,
            )
        }
        .mapError(ClaudeFailure.filesystem)
    }
}
