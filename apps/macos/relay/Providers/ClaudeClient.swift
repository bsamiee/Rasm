import CasePathsCore
import CurrentProcess
import Encoding
import Files
import Foundation
import Processes
import Results
import Subprocess
import System
import os

// --- [MODELS] --------------------------------------------------------------------------

nonisolated struct ClaudePendingSwitch: Codable, Equatable, Sendable {
    let incoming: UUID
    let outgoing: UUID?

    var copies: [UUID] { [incoming, outgoing].compactMap(\.self) }
}

nonisolated struct ClaudeAuthStatus: Decodable, Sendable {
    let loggedIn: Bool
    let email: String?
    let orgId: String?
    let subscriptionType: String?
    let loginExpiresAt: Date?
}

nonisolated struct ClaudeUsageReport: Decodable, Sendable {
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

    func usage(observedAt: Date, signInExpiresAt: Date?) throws(ClaudeFailure) -> AccountUsage {
        guard let limits = rateLimits?.limits else { throw .usageUnavailable }
        let windows = try traverse(limits.compactMap { limit in limit.quota.map { kind in (kind, limit) } }) { kind, limit in
            UsageAmount.make(percent: limit.percent).aggregated(\.self).map { amount in QuotaWindow(kind: kind, used: amount, resetsAt: limit.resetsAt) }
        }
        .mapError(ClaudeFailure.invalidQuota)
        .get()
        guard !windows.isEmpty else { throw .protocolFailure(nil) }
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
    private let logger: Logger = .init(for: ClaudeClient.self)
    private var lastCredential: ClaudeCredential?
    private var reading: Task<Result<ClaudeUsageReport?, ClaudeFailure>, Never>?

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
        ClaudeCredentialStore(home: paths.home, environment: [.claudeConfigDirectory: FilePath(paths.claudeDirectory(id).path(percentEncoded: false)).string], username: username)
    }

    // --- [SELECTION]

    func currentSelection(selected: Account?) async -> Result<AccountIdentity?, ProviderError> {
        await Result { () async throws(ClaudeFailure) in
            let content = try await shared.read()
            let identity = if let held = content[case: \.credential] { held.identity } else { try shared.identity() }
            if let selected, let saved = lastCredential, saved.identity.isSameAccount(as: selected.identity),
                !(identity.map(saved.identity.isSameAccount(as:)) ?? false)
            {
                let store = privateStore(selected.id)
                try await ClaudeCredentialStore.withLock(directories: [store.directory]) { () async throws(ClaudeFailure) in try await store.write(saved, over: store.readItem()) }
                logger.notice("Saved account \(selected.id, privacy: .public) credential to private store after outside sign-in")
            }
            lastCredential = content[case: \.credential]
            return identity
        }
        .mapError(ProviderError.init(failure:))
    }

    func holdsCredential(for account: Account, isSelected: Bool) async -> Result<Bool, ProviderError> {
        let now = Date()
        return await Result { () async throws(ClaudeFailure) in
            try await signedIn(store(for: account, isSelected: isSelected)).map { signedIn in
                signedIn.identity.isSameAccount(as: account.identity) && signedIn.expiresAt.map { expiry in expiry > now } ?? true
            } ?? false
        }
        .mapError(ProviderError.init(failure:))
    }

    nonisolated func pendingSwitch() -> Result<ClaudePendingSwitch?, ProviderError> {
        Result { try ifPresent { try Data(contentsOf: paths.claudeSelectionFile) }.flatMap { data in try JSONDecoder().decode(ClaudePendingSwitch?.self, from: data) } }
            .mapError { error in ProviderError(failure: ClaudeFailure.filesystem(error)) }
    }

    func completePendingSwitch(_ pending: ClaudePendingSwitch) async -> Result<Void, ProviderError> {
        await Result { () async throws(ClaudeFailure) in
            try await ClaudeCredentialStore.withLock(directories: ([shared] + pending.copies.map(privateStore)).map(\.directory)) { () async throws(ClaudeFailure) in
                try await self.finish(pending)
            }
        }
        .mapError(ProviderError.init(failure:))
    }

    private nonisolated func finish(_ pending: ClaudePendingSwitch) async throws(ClaudeFailure) {
        let current = try await shared.read().token?.accessToken
        for id in pending.copies {
            let copy = privateStore(id)
            if let duplicate = try await copy.read()[case: \.credential], duplicate.token.accessToken == current {
                try await shared.writeAccount(duplicate.account)
                try await copy.deleteItem()
                logger.notice("Restored live account file from account \(id, privacy: .public) duplicate and deleted it")
            }
        }
        try writePendingSwitch(nil)
    }

    // --- [SWITCHES]

    func select(
        _ account: Account,
        outgoing: Account?,
    ) async -> Result<AccountIdentity, ProviderError> {
        let incoming = privateStore(account.id)
        let outgoingStore = outgoing.map { outgoing in privateStore(outgoing.id) }
        let pending = ClaudePendingSwitch(incoming: account.id, outgoing: outgoing?.id)
        let last = lastCredential
        return await Result { () async throws(ClaudeFailure) in
            for store in [shared, incoming] where try await store.read().is(\.unidentified) {
                _ = try await report(of: store)
            }
            let credential = try await ClaudeCredentialStore.withLock(directories: [shared, incoming, outgoingStore].compactMap(\.self).map(\.directory)) {
                () async throws(ClaudeFailure) in
                let current = try await self.shared.read()
                let stored = try await incoming.read()[case: \.credential]
                let selected = current[case: \.credential].flatMap { held in held.identity.isSameAccount(as: account.identity) ? held : nil }
                let outgoingCredential = outgoing.flatMap { outgoing in
                    [current[case: \.credential], last].lazy.compactMap(\.self).first { held in held.identity.isSameAccount(as: outgoing.identity) }
                }
                guard let entering = selected ?? stored, entering.identity.isSameAccount(as: account.identity) else { throw .signInRequired }
                guard current.token.map({ token in [selected, outgoingCredential].contains { held in held?.token.accessToken == token.accessToken } }) ?? true else { throw .accountChanged }
                let sharedItem = try await self.shared.readItem()
                let outgoingItem = try await outgoingStore?.readItem()
                try self.writePendingSwitch(pending)
                let moved = await Result { () async throws(ClaudeFailure) in
                    if let outgoingCredential { try await outgoingStore?.write(outgoingCredential, over: outgoingItem) }
                    if selected == nil { try await self.shared.write(entering, over: sharedItem) }
                    try await incoming.deleteItem()
                    try self.writePendingSwitch(nil)
                }
                if case .failure(let error) = moved { throw error.releasing(await Result { () async throws(ClaudeFailure) in try await self.finish(pending) }) }
                self.logger.notice(
                    "Switched to account \(account.id, privacy: .public), outgoing saved \(outgoingCredential != nil, privacy: .public), live item replaced \(selected == nil, privacy: .public)"
                )
                return entering
            }
            lastCredential = credential
            return credential.identity
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
            return switch try await signedIn(store) {
                case .some(let signedIn) where expected.map(signedIn.identity.isSameAccount(as:)) ?? true: signedIn.identity
                case .some:
                    throw .accountChanged.releasing(await Result { () async throws(ClaudeFailure) in try await run(["auth", "logout"], store: store, within: .seconds(60), output: .discarded) })
                case .none: throw .signInRequired
            }
        }
        .mapError(ProviderError.init(failure:))
    }

    func signOut(_ account: Account, isSelected: Bool) async -> Result<Void, ProviderError> {
        let store = store(for: account, isSelected: isSelected)
        if isSelected { lastCredential = nil }
        return await Result { () async throws(ClaudeFailure) in try await run(["auth", "logout"], store: store, within: .seconds(60), output: .discarded) }.mapError(ProviderError.init(failure:))
    }

    func deletePrivateStore(id: UUID) async -> Result<Void, ProviderError> {
        await Result { () async throws(ClaudeFailure) in try await privateStore(id).deleteItem() }.mapError(ProviderError.init(failure:))
    }

    // --- [USAGE]

    func usage(for account: Account, isSelected: Bool) async -> Result<AccountUsage, ProviderError> {
        let store = store(for: account, isSelected: isSelected)
        return await Result { () async throws(ClaudeFailure) in
            let report = try await report(of: store)
            guard let signedIn = try await signedIn(store) else { throw .signInRequired }
            guard signedIn.identity.isSameAccount(as: account.identity) else { throw .accountChanged }
            guard let report else { throw .protocolFailure(nil) }
            return try report.usage(observedAt: Date(), signInExpiresAt: signedIn.expiresAt)
        }
        .mapError(ProviderError.init(failure:))
    }

    // --- [CHILDREN]

    private func report(of store: ClaudeCredentialStore) async throws(ClaudeFailure) -> ClaudeUsageReport? {
        let previous = reading
        let read = Task {
            _ = await previous?.value
            return await Result { () async throws(ClaudeFailure) -> ClaudeUsageReport? in
                let output = try await run(
                    ["-p", "/usage", "--output-format", "stream-json", "--verbose", "--no-session-persistence", "--strict-mcp-config", "--settings", #"{"disableClaudeAiConnectors":true}"#],
                    store: store,
                    within: .seconds(60),
                    output: .data(limit: 1 << 20),
                )
                let reports = try output.split(separator: UInt8(ascii: "\n")).map { line throws(ClaudeFailure) in
                    try Self.decode(ClaudeStreamMessage.self, from: line).usageReport
                }
                return reports.lazy.compactMap(\.self).first
            }
        }
        reading = read
        return try await withTaskCancellationHandler {
            await read.value
        } onCancel: {
            read.cancel()
        }.get()
    }

    private func signedIn(_ store: ClaudeCredentialStore) async throws(ClaudeFailure) -> (identity: AccountIdentity, expiresAt: Date?)? {
        let printed = await Result { () async throws(ClaudeFailure) in
            try await run(["auth", "status", "--json"], store: store, within: .seconds(15), output: .data(limit: 1 << 16))
        }
        guard printed[case: \.failure]?.cause[case: \.process]?.reason[case: \.exit]?.0 != .exited(1) else { return nil }
        let status = try Self.decode(ClaudeAuthStatus.self, from: printed.get())
        guard status.loggedIn else { return nil }
        return try AccountIdentity.make(accountID: store.readAccount()?.known.accountUuid, organizationID: status.orgId, email: status.email, plan: status.subscriptionType)
            .map { identity in (identity, status.loginExpiresAt) }
            .mapError(ClaudeFailure.invalidIdentity)
            .get()
    }

    private static func decode<Value: Decodable>(_ type: Value.Type, from data: Data) throws(ClaudeFailure) -> Value {
        try Result { try output.decode(type, from: data) }.mapError { error in ClaudeFailure.protocolFailure(error) }.get()
    }

    private func run<Output: OutputProtocol>(
        _ arguments: Arguments,
        store: ClaudeCredentialStore,
        within timeout: Duration,
        output: Output,
        input: some InputProtocol = .none,
    ) async throws(ClaudeFailure) -> Output.OutputType {
        try Result { try FileManager.default.createDirectory(at: store.directory, withIntermediateDirectories: true) }.mapError(ClaudeFailure.filesystem).get()
        let invocation = Configuration(
            executable: .name("claude"),
            arguments: arguments,
            environment: .custom(environment.filter { key, _ in !["ANTHROPIC_", "CLAUDE_", "CCR_"].contains(where: key.rawValue.hasPrefix) }).updating(store.variables),
            platformOptions: .session(),
        )
        return try await Result { () async throws(ProcessFailure) in try await invocation.run(output: output, within: timeout, input: input) }
            .mapError(ClaudeFailure.init(child:))
            .get()
            .standardOutput
    }
}
