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

private nonisolated struct ClaudeAuthStatus: Decodable {
    let loggedIn: Bool
    let email: String?
    let orgId: String?
    let subscriptionType: String?
    let loginExpiresAt: Date?
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

        var quota: QuotaKind? {
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
        let windows = try traverse(limits.compactMap { limit in limit.quota.map { kind in (kind, limit) } }) { kind, limit in
            Result { () throws(AccountFailure) in try QuotaWindow(kind: kind, percent: limit.percent, resetsAt: limit.resetsAt) }.aggregated(\.self)
        }
        .mapError { causes in AccountFailure(subject: "Claude usage", reason: .invalid(causes)) }
        .get()
        guard !windows.isEmpty else { throw AccountFailure(subject: "Claude weekly usage", reason: .missing) }
        return AccountUsage(windows: windows, includedUsageAllowed: nil, observedAt: observedAt, signInExpiresAt: signInExpiresAt)
    }
}

private nonisolated struct ClaudeStreamMessage: Decodable {
    let usageReport: ClaudeUsageReport?
}

// --- [SERVICES] ------------------------------------------------------------------------

actor ClaudeClient: ProviderClient {
    // --- [STATE]

    nonisolated let shared: ClaudeCredentialStore
    private let paths: FileLocations
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

    init(paths: FileLocations, environment: [Environment.Key: String]) {
        self.paths = paths
        self.environment = environment
        let candidate = environment["USER"].flatMap { value in value.isEmpty ? nil : value } ?? NSUserName()
        username = candidate.contains(#/^[a-zA-Z0-9._-]+$/#) ? candidate : "claude-code-user"
        shared = ClaudeCredentialStore(home: paths.home, environment: environment, username: username)
    }

    private func store(for account: Account, isSelected: Bool) -> ClaudeCredentialStore {
        isSelected ? shared : privateStore(account.id)
    }

    private nonisolated func privateStore(_ id: UUID) -> ClaudeCredentialStore {
        ClaudeCredentialStore(home: paths.home, environment: ["CLAUDE_CONFIG_DIR": FilePath(paths.claudeDirectory(id).path(percentEncoded: false)).string], username: username)
    }

    // --- [SELECTION]

    func currentSelection(selected: Account?) async throws(AccountFailure) -> AccountIdentity? {
        let content = try await shared.read()
        let identity = if let held = content[case: \.credential] { held.identity } else { try shared.identity() }
        if let selected, let saved = lastCredential, saved.identity.isSameAccount(as: selected.identity), !(identity.map(saved.identity.isSameAccount(as:)) ?? false) {
            let store = privateStore(selected.id)
            try await ClaudeCredentialStore.withLock(directories: [store.directory]) { () async throws(AccountFailure) in try await store.write(saved, over: store.readItem()) }
            logger.notice("Saved credential to private store after outside sign-in", metadata: Provider.claude.metadata(selected.id))
        }
        lastCredential = content[case: \.credential]
        return identity
    }

    func holdsCredential(for account: Account, isSelected: Bool) async throws(AccountFailure) -> Bool {
        let now = Date()
        return try await signedIn(store(for: account, isSelected: isSelected)).map { signedIn in
            signedIn.identity.isSameAccount(as: account.identity) && signedIn.expiresAt.map { expiry in expiry > now } ?? true
        } ?? false
    }

    nonisolated func pendingSwitch() throws(AccountFailure) -> ClaudePendingSwitch? {
        try paths.claudeSelectionFile.contents().flatMap { data throws(AccountFailure) in
            try JSONDecoder().decode(ClaudePendingSwitch?.self, from: data, subject: paths.claudeSelectionFile.lastPathComponent)
        }
    }

    func completePendingSwitch(_ pending: ClaudePendingSwitch) async throws(AccountFailure) {
        try await ClaudeCredentialStore.withLock(directories: ([shared] + pending.copies.map(privateStore)).map(\.directory)) { () async throws(AccountFailure) in
            try await self.finish(pending)
        }
    }

    private nonisolated func finish(_ pending: ClaudePendingSwitch) async throws(AccountFailure) {
        let current = try await shared.read().token?.accessToken
        for id in pending.copies {
            let copy = privateStore(id)
            if let duplicate = try await copy.read()[case: \.credential], duplicate.token.accessToken == current {
                try await shared.writeAccount(duplicate.account)
                try await copy.deleteItem()
                logger.notice("Restored live account file from duplicate and deleted it", metadata: Provider.claude.metadata(id))
            }
        }
        try await writePendingSwitch(nil)
    }

    private nonisolated func writePendingSwitch(_ pending: ClaudePendingSwitch?) async throws(AccountFailure) {
        try await JSONEncoder().encode(pending, subject: paths.claudeSelectionFile.lastPathComponent).write(to: paths.claudeSelectionFile)
    }

    // --- [SWITCHES]

    func select(_ account: Account, outgoing: Account?) async throws(AccountFailure) -> AccountIdentity {
        let incoming = privateStore(account.id)
        let outgoingStore = outgoing.map { outgoing in privateStore(outgoing.id) }
        let pending = ClaudePendingSwitch(incoming: account.id, outgoing: outgoing?.id)
        let last = lastCredential
        for store in [shared, incoming] where try await store.read().is(\.unidentified) {
            _ = try await report(of: store)
        }
        let credential = try await ClaudeCredentialStore.withLock(directories: [shared, incoming, outgoingStore].compactMap(\.self).map(\.directory)) {
            () async throws(AccountFailure) in
            let current = try await self.shared.read()
            let stored = try await incoming.read()[case: \.credential]
            let selected = current[case: \.credential].flatMap { held in held.identity.isSameAccount(as: account.identity) ? held : nil }
            let outgoingCredential = outgoing.flatMap { outgoing in
                [current[case: \.credential], last].lazy.compactMap(\.self).first { held in held.identity.isSameAccount(as: outgoing.identity) }
            }
            guard let entering = selected ?? stored, entering.identity.isSameAccount(as: account.identity) else { throw AccountFailure(.claude, .signInRequired) }
            guard current.token.map({ token in [selected, outgoingCredential].contains { held in held?.token.accessToken == token.accessToken } }) ?? true else {
                throw AccountFailure(.claude, .selectionChanged)
            }
            let sharedItem = try await self.shared.readItem()
            let outgoingItem = try await outgoingStore?.readItem()
            try await self.writePendingSwitch(pending)
            do throws(AccountFailure) {
                if let outgoingCredential { try await outgoingStore?.write(outgoingCredential, over: outgoingItem) }
                if selected == nil { try await self.shared.write(entering, over: sharedItem) }
                try await incoming.deleteItem()
                try await self.writePendingSwitch(nil)
            } catch {
                throw error.releasing(await Result { () async throws(AccountFailure) in try await self.finish(pending) })
            }
            self.logger.notice(
                "Switched live store to account",
                metadata: Provider.claude.metadata(account.id, ["outgoingSaved": .stringConvertible(outgoingCredential != nil), "liveItemReplaced": .stringConvertible(selected == nil)]),
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
                case .account(let account, let isSelected): (store(for: account, isSelected: isSelected), account.identity)
            }
        let email: [String] = expected.map { identity in ["--email", identity.email] } ?? []
        try await run(
            Arguments(["auth", "login", "--claudeai"] + email),
            store: store,
            within: .seconds(300),
            output: .discarded,
            input: .sequence(codes.prefix(1).map { code in Data((code + "\n").utf8) }),
        )
        return switch try await signedIn(store) {
            case .some(let signedIn) where expected.map(signedIn.identity.isSameAccount(as:)) ?? true: signedIn.identity
            case .some:
                throw AccountFailure(.claude, .accountChanged).releasing(
                    await Result { () async throws(AccountFailure) in try await run(["auth", "logout"], store: store, within: .seconds(60), output: .discarded) }
                )
            case .none: throw AccountFailure(.claude, .signInRequired)
        }
    }

    func signOut(_ account: Account, isSelected: Bool) async throws(AccountFailure) {
        if isSelected { lastCredential = nil }
        try await run(["auth", "logout"], store: store(for: account, isSelected: isSelected), within: .seconds(60), output: .discarded)
    }

    func deletePrivateStore(id: UUID) async throws(AccountFailure) {
        try await privateStore(id).deleteItem()
    }

    // --- [USAGE]

    func usage(for account: Account, isSelected: Bool) async throws(AccountFailure) -> AccountUsage {
        let store = store(for: account, isSelected: isSelected)
        let report = try await report(of: store)
        guard let signedIn = try await signedIn(store) else { throw AccountFailure(.claude, .signInRequired) }
        guard signedIn.identity.isSameAccount(as: account.identity) else { throw AccountFailure(.claude, .accountChanged) }
        guard let report else { throw AccountFailure(subject: "Claude Code usage report", reason: .missing) }
        return try report.usage(observedAt: Date(), signInExpiresAt: signedIn.expiresAt)
    }

    // --- [CHILDREN]

    private func report(of store: ClaudeCredentialStore) async throws(AccountFailure) -> ClaudeUsageReport? {
        let previous = reading
        let read = Task {
            _ = await previous?.value
            return await Result { () async throws(AccountFailure) -> ClaudeUsageReport? in
                let printed: Data = try await run(
                    ["-p", "/usage", "--output-format", "stream-json", "--verbose", "--no-session-persistence", "--strict-mcp-config", "--settings", #"{"disableClaudeAiConnectors":true}"#],
                    store: store,
                    within: .seconds(60),
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
        } onCancel: {
            read.cancel()
        }
        .get()
    }

    private func signedIn(_ store: ClaudeCredentialStore) async throws(AccountFailure) -> (identity: AccountIdentity, expiresAt: Date?)? {
        let printed: Data
        do throws(AccountFailure) {
            printed = try await run(["auth", "status", "--json"], store: store, within: .seconds(15), output: .data(limit: 1 << 16))
        } catch let error where error.reason[case: \.process]?.reason[case: \.exit]?.0 == .exited(1) {
            return nil
        }
        let status = try Self.output.decode(ClaudeAuthStatus.self, from: printed, subject: "claude auth status")
        guard status.loggedIn else { return nil }
        let identity = try AccountIdentity(accountID: store.readAccount()?.known.accountUuid, organizationID: status.orgId, email: status.email, plan: status.subscriptionType)
        return (identity, status.loginExpiresAt)
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
            platformOptions: .session(),
        )
        do throws(ProcessFailure) {
            return try await invocation.run(output: output, within: timeout, input: input).standardOutput
        } catch {
            throw AccountFailure(.claude, child: error)
        }
    }
}
