import CasePathsCore
import Encoding
import Files
import Foundation
import Logging
import Processes
import Results
import Subprocess
import System
import Telemetry

// --- [MODELS] --------------------------------------------------------------------------

nonisolated struct ClaudePendingSwitch: Codable, Sendable {
    let incoming: UUID
    let outgoing: UUID?

    var copies: [UUID] { [incoming, outgoing].compactMap(\.self) }
}

private nonisolated struct ClaudeUsageReport: Decodable, Sendable {
    enum Meter: String, Sendable {
        case weeklyAll = "weekly_all"
        case weeklyScoped = "weekly_scoped"
    }

    struct Limit: Decodable, Sendable {
        struct Scope: Decodable, Sendable {
            struct Model: Decodable, Sendable {
                let displayName: String
            }

            let model: Model?
        }

        let kind: Lenient<Meter>
        let percent: Double
        let resetsAt: Date?
        let scope: Scope?

        var rateLimitKind: RateLimitKind? {
            switch (kind, scope?.model) {
                case (.known(.weeklyAll), _): .weekly
                case (.known(.weeklyScoped), .some(let model)): .model(model.displayName)
                case (.known(.weeklyScoped), .none), (.unknown, _): nil
            }
        }
    }

    struct RateLimits: Decodable, Sendable {
        let limits: [Limit]?
    }

    let rateLimits: RateLimits?

    func usage(observedAt: Date, signInExpiresAt: Date?) throws(AccountFailure) -> AccountUsage {
        guard let limits = rateLimits?.limits else { throw AccountFailure(subject: Provider.claude.tool, reason: .usageUnavailable) }
        let windows = try traverse(limits.compactMap { limit in limit.rateLimitKind.map { kind in (kind, limit) } }) { kind, limit in
            Result { () throws(AccountFailure) in try RateLimitWindow(kind: kind, percent: limit.percent, resetsAt: limit.resetsAt) }.aggregated(\.self)
        }
        .mapError { causes in AccountFailure(subject: "Claude usage", reason: .invalid(causes)) }
        .get()
        guard !windows.isEmpty else { throw AccountFailure(subject: "Claude weekly usage", reason: .missing) }
        return AccountUsage(windows: windows, ordinaryUsageAllowed: nil, observedAt: observedAt, signInExpiresAt: signInExpiresAt)
    }
}

private nonisolated struct ClaudeStreamMessage: Decodable {
    let usageReport: ClaudeUsageReport?
}

// --- [SERVICES] ------------------------------------------------------------------------

actor ClaudeClient: ProviderClient {
    // --- [STATE]

    nonisolated let active: ClaudeCredentialStore
    private let locations: FileLocations
    private let environment: [Environment.Key: String]
    private let username: String
    private let logger = Logger(label: String(reflecting: ClaudeClient.self))
    private var lastCredential: ClaudeCredential?
    private var reading: Task<Result<ClaudeUsageReport?, AccountFailure>, Never>?

    private static let output: JSONDecoder = {
        let decoder = JSONDecoder()
        decoder.keyDecodingStrategy = .convertFromSnakeCase
        decoder.dateDecodingStrategy = .iso8601
        return decoder
    }()

    init(locations: FileLocations, environment: [Environment.Key: String]) {
        self.locations = locations
        self.environment = environment
        let candidate = environment["USER"].flatMap { value in value.isEmpty ? nil : value } ?? NSUserName()
        username = candidate.contains(#/^[a-zA-Z0-9._-]+$/#) ? candidate : "claude-code-user"
        active = ClaudeCredentialStore(home: locations.home, environment: environment, username: username)
    }

    private func store(for account: Account, isActive: Bool) -> ClaudeCredentialStore {
        isActive ? active : privateStore(account.id)
    }

    private nonisolated func privateStore(_ id: UUID, under accounts: URL? = nil) -> ClaudeCredentialStore {
        let directory = FilePath(locations.claudeDirectory(id, under: accounts).path(percentEncoded: false)).string
        return ClaudeCredentialStore(home: locations.home, environment: ["CLAUDE_CONFIG_DIR": directory], username: username)
    }

    func settle(_ accounts: [Account], movedFrom previous: URL?) async -> Bool {
        var settled = true
        for (id, source) in accounts.compactMap({ (account: Account) in previous.map { (previous: URL) in (account.id, privateStore(account.id, under: previous)) } }) {
            let store = privateStore(id)
            do throws(AccountFailure) {
                let moved = try await ClaudeCredentialStore.withLock(directories: [store.directory]) { () async throws(AccountFailure) in try await source.moveItems(to: store) }
                if moved { logger.notice("Moved Claude login", metadata: Provider.claude.metadata(id, ["from": "\(source.service, privacy: .public)", "to": "\(store.service, privacy: .public)"])) }
            } catch {
                settled = false
                logger.error("Claude login move failed", error: error, metadata: Provider.claude.metadata(id, ["from": "\(source.service, privacy: .public)"]))
            }
        }
        let held = await Result { () async throws(AccountFailure) in try await active.read() }[case: \.success]?[case: \.credential]?.identity
        for account in accounts {
            let store = held.map(account.identity.isSameAccount(as:)) == true ? active : privateStore(account.id)
            let fields = Provider.claude.metadata(account.id, ["service": "\(store.service, privacy: .public)"])
            do throws(AccountFailure) {
                let content = try await store.read()
                let facts: [String: Logger.MetadataValue?] = [
                    "login": Mirror(reflecting: content).children.first?.label.map { (label: String) -> Logger.MetadataValue in "\(label, privacy: .public)" },
                    "cause": content[case: \.signedOut].map { (cause: SignInCause) -> Logger.MetadataValue in "\(cause, privacy: .public)" },
                    "tokenExpires": content.token?.expires.map { (expiry: Date) -> Logger.MetadataValue in "\(expiry.formatted(.iso8601), privacy: .public)" },
                    "loginEnds": content.token?.loginEnds.map { (end: Date) -> Logger.MetadataValue in "\(end.formatted(.iso8601), privacy: .public)" },
                ]
                logger.info("Read Claude store", metadata: fields.merging(facts.compactMapValues(\.self)) { stated, _ in stated })
            } catch {
                logger.error("Claude store read failed", error: error, metadata: fields)
            }
        }
        return settled
    }

    // --- [ACTIVE_ACCOUNT]

    func activeIdentity(active account: Account?) async throws(AccountFailure) -> AccountIdentity? {
        let content = try await active.read()
        let identity = if let held = content[case: \.credential] { held.identity } else { try active.identity() }
        if let account, let saved = lastCredential, saved.identity.isSameAccount(as: account.identity), !(identity.map(saved.identity.isSameAccount(as:)) ?? false) {
            let store = privateStore(account.id)
            let written = try await ClaudeCredentialStore.withLock(directories: [store.directory]) { () async throws(AccountFailure) -> Bool in
                let item = try await store.readItem()
                let stored = try item?.known.claudeAiOauth.flatMap(ClaudeOAuthToken.init)?.expires
                guard stored.map({ stored in saved.token.expires.map { expiry in expiry > stored } ?? false }) ?? true else { return false }
                try await store.write(saved, over: item)
                return true
            }
            logger.notice(
                written ? "Saved credential to private store after outside sign-in" : "Kept newer private store credential after outside sign-in",
                metadata: Provider.claude.metadata(account.id, ["service": "\(store.service, privacy: .public)"]),
            )
        }
        lastCredential = content[case: \.credential]
        return identity
    }

    nonisolated func pendingSwitch() throws(AccountFailure) -> ClaudePendingSwitch? {
        try locations.claudePendingSwitchFile.contents().flatMap { data throws(AccountFailure) in
            try JSONDecoder().decode(ClaudePendingSwitch?.self, from: data, subject: locations.claudePendingSwitchFile.lastPathComponent)
        }
    }

    func completePendingSwitch(_ pending: ClaudePendingSwitch) async throws(AccountFailure) {
        try await ClaudeCredentialStore.withLock(directories: ([active] + pending.copies.map { id in privateStore(id) }).map(\.directory)) { () async throws(AccountFailure) in
            try await self.finish(pending)
        }
    }

    private nonisolated func finish(_ pending: ClaudePendingSwitch) async throws(AccountFailure) {
        let current = try await active.read().token?.accessToken
        for id in pending.copies {
            let copy = privateStore(id)
            if let duplicate = try await copy.read()[case: \.credential], duplicate.token.accessToken == current {
                try await active.writeAccount(duplicate.account)
                try await copy.deleteItem()
                logger.notice("Restored active account file from duplicate and deleted it", metadata: Provider.claude.metadata(id))
            }
        }
        try await writePendingSwitch(nil)
    }

    private nonisolated func writePendingSwitch(_ pending: ClaudePendingSwitch?) async throws(AccountFailure) {
        try await JSONEncoder().encode(pending, subject: locations.claudePendingSwitchFile.lastPathComponent).write(to: locations.claudePendingSwitchFile)
    }

    // --- [SWITCHES]

    func activate(_ account: Account, outgoing: Account?) async throws(AccountFailure) -> AccountIdentity {
        let incoming = privateStore(account.id)
        let outgoingStore = outgoing.map { outgoing in privateStore(outgoing.id) }
        let pending = ClaudePendingSwitch(incoming: account.id, outgoing: outgoing?.id)
        let last = lastCredential
        for store in [active, incoming] {
            if let token = try await store.read()[case: \.unidentified] { _ = try await report(of: store, holding: token) }
        }
        let credential = try await ClaudeCredentialStore.withLock(directories: [active, incoming, outgoingStore].compactMap(\.self).map(\.directory)) {
            () async throws(AccountFailure) in
            let current = try await self.active.read()
            let held = try await incoming.read()
            let stored = held[case: \.credential]
            let activeCredential = current[case: \.credential].flatMap { held in held.identity.isSameAccount(as: account.identity) ? held : nil }
            let outgoingCredential = outgoing.flatMap { outgoing in
                [current[case: \.credential], last].lazy.compactMap(\.self).first { held in held.identity.isSameAccount(as: outgoing.identity) }
            }
            guard let entering = activeCredential ?? stored, entering.identity.isSameAccount(as: account.identity) else {
                throw switch held {
                    case .signedOut(let cause): AccountFailure(subject: incoming.service, reason: .signInRequired(cause))
                    case .unidentified: AccountFailure(subject: "Claude account entry", reason: .missing)
                    case .credential: AccountFailure(subject: incoming.service, reason: .signInRequired(.otherAccount))
                }
            }
            guard current.token.map({ token in [activeCredential, outgoingCredential].contains { held in held?.token.accessToken == token.accessToken } }) ?? true else {
                throw AccountFailure(.claude, .activeAccountChanged)
            }
            let activeItem = try await self.active.readItem()
            let outgoingItem = try await outgoingStore?.readItem()
            try await self.writePendingSwitch(pending)
            do throws(AccountFailure) {
                if let outgoingCredential { try await outgoingStore?.write(outgoingCredential, over: outgoingItem) }
                if activeCredential == nil { try await self.active.write(entering, over: activeItem) }
                try await incoming.deleteItem()
                try await self.writePendingSwitch(nil)
            } catch {
                throw error.releasing(await Result { () async throws(AccountFailure) in try await self.finish(pending) })
            }
            self.logger.notice(
                "Switched active store to account",
                metadata: Provider.claude.metadata(account.id, ["outgoingSaved": .stringConvertible(outgoingCredential != nil), "activeItemReplaced": .stringConvertible(activeCredential == nil)]),
            )
            return entering
        }
        lastCredential = credential
        return credential.identity
    }

    // --- [SIGN_IN]

    func signIn(_ target: SignInTarget, codes: AsyncStream<String>) async throws(AccountFailure) -> AccountIdentity {
        let (store, expected): (ClaudeCredentialStore, AccountIdentity?) =
            switch target {
                case .newAccount(let id): (privateStore(id), nil)
                case .account(let account, let isActive): (store(for: account, isActive: isActive), account.identity)
            }
        let email: [String] = expected.map { identity in ["--email", identity.email] } ?? []
        try await run(
            Arguments(["auth", "login", "--claudeai"] + email),
            store: store,
            within: .seconds(300),
            output: .discarded,
            input: .sequence(codes.prefix(1).map { code in Data((code + "\n").utf8) }),
        )
        let held = try await credential(in: store)
        guard expected.map(held.identity.isSameAccount(as:)) ?? true else {
            throw AccountFailure(subject: store.service, reason: .signInRequired(.otherAccount)).releasing(
                await Result { () async throws(AccountFailure) in try await run(["auth", "logout"], store: store, within: .seconds(60), output: .discarded) }
            )
        }
        return held.identity
    }

    func signOut(_ account: Account, isActive: Bool) async throws(AccountFailure) {
        if isActive { lastCredential = nil }
        try await run(["auth", "logout"], store: store(for: account, isActive: isActive), within: .seconds(60), output: .discarded)
    }

    func deletePrivateStore(id: UUID) async throws(AccountFailure) {
        try await privateStore(id).deleteItem()
    }

    // --- [USAGE]

    func usage(for account: Account, isActive: Bool) async throws(AccountFailure) -> AccountUsage {
        let store = store(for: account, isActive: isActive)
        let held = try await credential(in: store)
        guard held.identity.isSameAccount(as: account.identity) else {
            throw isActive ? AccountFailure(.claude, .activeAccountChanged) : AccountFailure(subject: store.service, reason: .signInRequired(.otherAccount))
        }
        let outcome = await Result { () async throws(AccountFailure) in try await report(of: store, holding: held.token) }
        guard case .success(.some(let report)) = outcome else {
            if case .success(.signedOut(let cause)) = await Result(catching: { () async throws(AccountFailure) in try await store.read() }) {
                throw AccountFailure(subject: store.service, reason: .signInRequired(cause))
            }
            _ = try outcome.get()
            throw AccountFailure(subject: "Claude Code usage report", reason: .missing)
        }
        return try report.usage(observedAt: Date(), signInExpiresAt: held.token.loginEnds)
    }

    // --- [CHILDREN]

    private func credential(in store: ClaudeCredentialStore) async throws(AccountFailure) -> ClaudeCredential {
        let first = try await store.read()
        if let token = first[case: \.unidentified] { _ = try await report(of: store, holding: token) }
        switch first.is(\.unidentified) ? try await store.read() : first {
            case .credential(let held): return held
            case .unidentified: throw AccountFailure(subject: "Claude account entry", reason: .missing)
            case .signedOut(let cause): throw AccountFailure(subject: store.service, reason: .signInRequired(cause))
        }
    }

    private func report(of store: ClaudeCredentialStore, holding token: ClaudeOAuthToken) async throws(AccountFailure) -> ClaudeUsageReport? {
        let previous = reading
        let read = Task {
            _ = await previous?.value
            return await Result { () async throws(AccountFailure) -> ClaudeUsageReport? in
                guard !Task.isCancelled else { throw AccountFailure(.claude, .cancelled) }
                let printed: Data = try await run(
                    ["-p", "/usage", "--output-format", "stream-json", "--verbose", "--no-session-persistence", "--strict-mcp-config", "--settings", #"{"disableClaudeAiConnectors":true}"#],
                    store: store,
                    within: token.refreshesSoon ? .seconds(180) : .seconds(60),
                    output: .data(limit: 1 << 20),
                )
                let reports = try printed.split(separator: UInt8(ascii: "\n")).map { line throws(AccountFailure) -> ClaudeUsageReport? in
                    try Self.output.decode(ClaudeStreamMessage.self, from: line, subject: "Claude Code output").usageReport
                }
                return reports.lazy.compactMap(\.self).first
            }
        }
        reading = read
        return try await withTaskCancellationHandler {
            await read.value
        } onCancel: { [logger] in
            if token.refreshesSoon { logger.debug("Usage read near token expiry runs past cancel", metadata: ["service": "\(store.service, privacy: .public)"]) } else { read.cancel() }
        }
        .get()
    }

    @discardableResult
    private func run<Output: OutputProtocol>(
        _ arguments: Arguments,
        store: ClaudeCredentialStore,
        within timeout: Duration,
        output: Output,
        input: some InputProtocol = .none,
    ) async throws(AccountFailure) -> Output.OutputType {
        try store.directory.createDirectory()
        let invocation = Configuration(
            executable: .name("claude"),
            arguments: arguments,
            environment: .custom(environment.filter { key, _ in !["ANTHROPIC_", "CLAUDE_", "CCR_"].contains(where: key.rawValue.hasPrefix) }).updating(store.variables),
            platformOptions: .session,
        )
        do throws(ProcessFailure) {
            return try await invocation.run(output: output, within: timeout, input: input)
        } catch {
            throw AccountFailure(.claude, child: error)
        }
    }
}
