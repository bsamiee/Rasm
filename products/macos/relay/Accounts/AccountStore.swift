import AppKit
import AsyncAlgorithms
import CasePathsCore
import CurrentProcess
import Files
import Foundation
import Logging
import Network
import Observation
import Results
import ServiceManagement
import Subprocess
import SwiftUI
import System
import Telemetry
import Tracing
import os

// --- [TYPES] ---------------------------------------------------------------------------

private nonisolated enum CredentialStore: Hashable, Sendable {
    case live(Provider)
    case privateAccount(UUID)
}

enum StorageState: Sendable {
    case loading
    case available
    case unavailable
}

// --- [SERVICES] ------------------------------------------------------------------------

@Observable
final class AccountStore {
    // --- [STATE]

    private(set) var accounts: [AccountModel] = []
    private(set) var authentication: AuthenticationPresentation?
    private(set) var loginItem: Result<SMAppService.Status, any Error> = .success(SMAppService.mainApp.status)
    private(set) var storageState: StorageState = .loading
    private(set) var isMenuBarExtraVisible: Bool = false
    private var isStopping: Bool = false
    private var occupiedStores: Set<CredentialStore> = []
    @ObservationIgnored private var failures: [UUID: (operation: AccountOperation, cause: String, repeats: Int)] = [:]

    private let locations: FileLocations
    private let storage: AccountStorage
    private let claude: ClaudeClient
    private let codex: CodexClient
    private let logger: Logging.Logger = .init(label: String(reflecting: AccountStore.self))
    private let signposter: OSSignposter = .init(subsystem: Program.current.identifier, category: String(reflecting: AccountStore.self))
    private let saves: (stream: AsyncStream<Void>, continuation: AsyncStream<Void>.Continuation) = AsyncStream.makeStream(bufferingPolicy: .bufferingNewest(1))
    @ObservationIgnored private var root: Task<Void, Never>?
    @ObservationIgnored private var authenticationTask: Task<Void, Never>?
    @ObservationIgnored private var refreshWake: AsyncStream<Void>.Continuation?
    @ObservationIgnored private var lastPanelOpen: ContinuousClock.Instant?

    init(environment: [Subprocess.Environment.Key: String]) {
        locations = FileLocations(environment: environment)
        storage = AccountStorage(fileURL: locations.accountsFile)
        claude = ClaudeClient(paths: locations, environment: environment)
        codex = CodexClient(paths: locations, environment: environment)
    }

    var canAddAccount: Bool { storageState == .available && !isStopping && authentication == nil }

    private func model(_ id: UUID) -> AccountModel? {
        accounts.first { model in model.account.id == id }
    }

    private func client(for provider: Provider) -> any ProviderClient {
        switch provider {
            case .claude: claude
            case .openAI: codex
        }
    }

    // --- [CREDENTIAL_OWNERSHIP]

    private func withStores(
        _ requested: @autoclosure () -> Set<CredentialStore>,
        owning: Set<CredentialStore> = [],
        _ work: (Set<CredentialStore>) async -> Void,
    ) async {
        for await _ in Observations({ self.occupiedStores }) {
            let needed = requested().subtracting(owning)
            guard needed.isDisjoint(with: occupiedStores) else { continue }
            guard !Task.isCancelled else { return }
            occupiedStores.formUnion(needed)
            defer { occupiedStores.subtract(needed) }
            await work(owning.union(needed))
            return
        }
    }

    private func stores(of model: AccountModel) -> Set<CredentialStore> {
        Set([.privateAccount(model.account.id)] + (model.isSelected ? [.live(model.account.provider)] : []))
    }

    private func perform(_ operation: AccountOperation, on model: AccountModel, _ work: @escaping @MainActor () async -> Void) {
        guard !isStopping else { return }
        model.run(operation) { [self] in
            let account = model.account
            let interval = signposter.beginInterval(operation.name, id: signposter.makeSignpostID(), "\(account.id, privacy: .public)")
            logger.log(level: operation.level, "Account operation started", metadata: account.metadata.merging(["operation": "\(operation.name, privacy: .public)"]) { _, stated in stated })
            await withSpan(operation.name.description) { span in
                span.attributes["account.id"] = account.id.uuidString
                await work()
            }
            signposter.endInterval(operation.name, interval)
        }
    }

    private func run(
        _ model: AccountModel,
        _ operation: AccountOperation,
        _ work: @escaping (Set<CredentialStore>) async -> Void,
    ) {
        perform(operation, on: model) { [self] in await withStores(stores(of: model), work) }
    }

    // --- [LIFECYCLE]

    func start() {
        guard root == nil else { return }
        root = Task(name: "Accounts") { [self] in
            await withDiscardingTaskGroup { group in
                group.addTask(name: "Login item") { await observeActivation() }
                await load()
                guard storageState == .available, !isStopping else { return }
                await reconcile()
                await readSelection()
                group.addTask(name: "Save") { for await _ in self.saves.stream { await self.save() } }
                group.addTask(name: "Network") { await observeNetwork() }
                group.addTask(name: "Wake") { await observeWake() }
                group.addTask(name: "Claude selection") {
                    await observe(file: self.claude.shared.configFile, provider: .claude)
                }
                group.addTask(name: "Claude credential writes") { await observeCredentialWrites() }
                group.addTask(name: "Codex selection") { await observe(file: self.codex.liveAuthFile, provider: .openAI) }
                group.addTask(name: "Codex configuration") { await observe(file: self.codex.liveHome.appending(path: "config.toml"), provider: .openAI) }
                group.addTask(name: "Refresh schedule") { await schedule() }
            }
        }
        func readCodexPrecondition() async {
            guard accounts.contains(where: { model in model.account.provider == .openAI }) else { return }
            await withStores([.live(.openAI)]) { _ in
                do throws(CodexFailure) {
                    try await codex.preconditions()
                    logger.debug("Codex switch preconditions hold")
                } catch {
                    logger.error("Codex switch check failed", error: error)
                }
            }
        }
        func reconcile() async {
            await withStores([.live(.claude)]) { live in
                switch claude.pendingSwitch() {
                    case .success(.some(let pending)):
                        logger.notice("Completing pending Claude switch", metadata: ["copies": "\(pending.copies, privacy: .public)"])
                        await withStores(
                            Set(pending.copies.map(CredentialStore.privateAccount)),
                            owning: live,
                        ) { _ in
                            switch await claude.completePendingSwitch(pending) {
                                case .success: logger.notice("Completed Claude switch", metadata: ["account": "\(pending.incoming, privacy: .public)"])
                                case .failure(let error):
                                    logger.error("Claude switch failed", error: error.failure, metadata: ["account": "\(pending.incoming, privacy: .public)"])
                            }
                        }
                    case .success(.none): break
                    case .failure(let error):
                        logger.error("Pending Claude switch read failed", error: error.failure)
                }
            }
            guard !accounts.isEmpty else { return }
            await readCodexPrecondition()
            let orphans: [UUID]
            switch locations.orphanAccountDirectories(excluding: Set(accounts.map(\.account.id) + [authentication?.id].compactMap(\.self))) {
                case .success(let found): orphans = found
                case .failure(let error):
                    logger.error("Accounts directory listing failed", error: error)
                    orphans = []
            }
            guard !orphans.isEmpty else { return }
            logger.notice("Deleting orphan account directories", metadata: ["accounts": "\(orphans, privacy: .public)"])
            await withDiscardingTaskGroup { group in
                for (id, provider) in orphans.flatMap({ id in Provider.allCases.map { provider in (id, provider) } }) {
                    group.addTask(name: "\(provider.name) orphan deletion") { [client = client(for: provider), logger] in
                        if case .failure(let error) = await client.deletePrivateStore(id: id) {
                            logger.error(
                                "Orphan private store delete failed",
                                error: error.failure,
                                metadata: ["provider": "\(provider.name, privacy: .public)", "account": "\(id, privacy: .public)"],
                            )
                        }
                    }
                }
            }
            for id in orphans { removeAccountDirectory(id) }
        }
        func observe(file: URL, provider: Provider) async {
            do {
                for try await _ in FileEvent.events(of: file).map({ batch in
                    _ = try batch.get()
                    return try ifPresent { try Data(contentsOf: file) }
                }).removeDuplicates()
                    .dropFirst().debounce(for: .milliseconds(300))
                {
                    logger.debug("Live store changed", metadata: ["provider": "\(provider.name, privacy: .public)", "file": "\(file.lastPathComponent, privacy: .public)"])
                    let before = Dictionary(
                        uniqueKeysWithValues: accounts.map { model in (model.account.id, model.isSelected) }
                    )
                    await readSelection(provider)
                    if provider == .openAI { await readCodexPrecondition() }
                    let changed = accounts.filter { model in
                        before[model.account.id] != model.isSelected
                    }
                    refresh(changed)
                }
            } catch {
                logger.error("Live store watch failed", error: error, metadata: ["provider": "\(provider.name, privacy: .public)", "file": "\(file.lastPathComponent, privacy: .public)"])
            }
        }
        func observeActivation() async {
            for await _ in NotificationCenter.default.notifications(named: NSApplication.didBecomeActiveNotification) {
                loginItem = .success(SMAppService.mainApp.status)
            }
        }
        func observeNetwork() async {
            for await status in NWPathMonitor().map(\.status).removeDuplicates() where status == .satisfied && !isStopping {
                logger.debug("Network path satisfied")
                await readSelection()
                refresh(accounts)
            }
        }
        func observeWake() async {
            for await _ in NSWorkspace.shared.notificationCenter.notifications(
                named: NSWorkspace.didWakeNotification
            ) {
                logger.debug("Woke from sleep")
                await readSelection()
            }
        }
        func observeCredentialWrites() async {
            let lock = ClaudeCredentialStore.storageWriteLockfile(in: claude.shared.directory)
            do {
                for try await present in FileEvent.presence(of: lock).map({ present in try present.get() }).removeDuplicates().dropFirst()
                where !present {
                    logger.debug("Claude storage write lock released")
                    await readSelection(.claude)
                }
            } catch {
                logger.error("Claude storage write lock watch failed", error: error)
            }
        }
        func schedule() async {
            while !Task.isCancelled {
                let now = Date()
                let base: Duration =
                    switch (isMenuBarExtraVisible, lastPanelOpen?.duration(to: .now)) {
                        case (true, _): .seconds(AccountModel.usageReadSpacing.selected)
                        case (false, .some(..<Duration.seconds(15 * 60))): .seconds(5 * 60)
                        case (false, .some(..<Duration.seconds(60 * 60))): .seconds(15 * 60)
                        case (false, _): .seconds(30 * 60)
                    }
                let boundary = accounts.compactMap(\.usage.usage?.nextReset)
                    .filter { date in date > now }
                    .min()
                    .map { date in Duration.seconds(date.timeIntervalSince(now) + 1) }
                let delay = min(base, boundary ?? base)
                logger.debug("Scheduled next refresh", metadata: ["delay": "\(delay, privacy: .public)"])
                let (wakes, wake) = AsyncStream<Void>.makeStream(bufferingPolicy: .bufferingNewest(1))
                refreshWake = wake
                let slept = await Result {
                    try await race {
                        try await Task.sleep(for: delay)
                        return true
                    } against: {
                        await wakes.first { _ in true }.map { false } ?? false
                    }
                }
                guard !Task.isCancelled else { return }
                if case .success(true) = slept { refresh(accounts) }
            }
        }
    }

    func stop() async {
        logger.notice("Stopping account work")
        isStopping = true
        root?.cancel()
        authenticationTask?.cancel()
        for model in accounts { model.running?.task.cancel() }
        do throws(Errno) {
            try await withTimeout(.seconds(5), expired: .timedOut) { @MainActor [self] () async throws(Errno) in
                await root?.value
                await authenticationTask?.value
                for model in accounts { await model.running?.task.value }
                await codex.shutdown()
            }
            logger.notice("Stopped account work")
        } catch {
            logger.error("Account work stop failed", error: error)
        }
        await save()
    }

    private func load() async {
        switch await storage.load() {
            case .success(let loaded):
                accounts = loaded.map { saved in
                    AccountModel(
                        account: saved.account,
                        authentication: saved.authentication,
                        usage: saved.usage.map(UsageState.stale) ?? .unavailable,
                    )
                }
                storageState = .available
                logger.notice(
                    "Loaded accounts",
                    metadata: ["count": .stringConvertible(loaded.count), "file": "\(self.locations.accountsFile.lastPathComponent, privacy: .public)"],
                )
            case .failure(let error):
                storageState = .unavailable
                logger.error("Accounts load failed", error: error, metadata: ["file": "\(self.locations.accountsFile.lastPathComponent, privacy: .public)"])
        }
    }

    // --- [INTERFACE]

    func setMenuBarExtraVisible(_ visible: Bool) {
        isMenuBarExtraVisible = visible
        guard visible else { return }
        lastPanelOpen = .now
        logger.debug("Panel opened")
        Task(name: "Panel opened") { [self] in
            await readSelection()
            refresh(accounts)
        }
        refreshWake?.yield()
    }

    func registerLoginItem() {
        loginItem = Result { try SMAppService.mainApp.register() }.map { SMAppService.mainApp.status }
        switch loginItem {
            case .success: logger.notice("Registered login item")
            case .failure(let error): logger.error("Login item register failed", error: error)
        }
    }

    func unregisterLoginItem() async {
        loginItem = await Result { try await SMAppService.mainApp.unregister() }.map { SMAppService.mainApp.status }
        switch loginItem {
            case .success: logger.notice("Unregistered login item")
            case .failure(let error): logger.error("Login item unregister failed", error: error)
        }
    }

    // --- [ACCOUNT_ACTIONS]

    func moveAccounts(from offsets: IndexSet, to destination: Int) {
        accounts.move(fromOffsets: offsets, toOffset: destination)
        logger.info("Moved accounts", metadata: ["offsets": "\(Array(offsets), privacy: .public)", "destination": .stringConvertible(destination)])
        saves.continuation.yield()
    }

    // --- [SELECTION]

    private func readSelection() async {
        guard storageState == .available, !isStopping else { return }
        await withDiscardingTaskGroup { group in
            for provider in Provider.allCases {
                group.addTask(name: "\(provider.name) selection") { await self.readSelection(provider) }
            }
        }
    }

    private func readSelection(_ provider: Provider, owning: Set<CredentialStore> = []) async {
        await withStores([.live(provider)], owning: owning) { live in
            let selected = accounts.first { model in model.account.provider == provider && model.isSelected }?.account
            await withStores(Set([selected.map { account in CredentialStore.privateAccount(account.id) }].compactMap(\.self)), owning: live) { stores in
                switch await client(for: provider).currentSelection(selected: selected) {
                    case .success(let identity):
                        await withStores(
                            Set(
                                accounts.filter { model in
                                    model.account.provider == provider
                                        && (model.isSelected || identity.map(model.account.identity.isSameAccount(as:)) == true)
                                }.map { model in .privateAccount(model.account.id) }
                            ),
                            owning: stores,
                        ) { _ in
                            if let identity { register(identity) } else { apply(selected: nil) }
                        }
                    case .failure(let error) where error.failure.isCancellation: break
                    case .failure(let error):
                        logger.error("Selection read failed", error: error.failure, metadata: ["provider": "\(provider.name, privacy: .public)"])
                }
            }
        }
        func register(_ identity: AccountIdentity) {
            if let existing = accounts.first(where: { model in
                model.account.provider == provider && model.account.identity.isSameAccount(as: identity)
            }) {
                apply(selected: existing.account.id)
                return
            }
            let model = AccountModel(
                account: Account(id: UUID(), provider: provider, identity: identity),
                authentication: .connected,
                usage: .unavailable,
            )
            accounts.append(model)
            logger.notice("Registered account from live store", metadata: model.account.metadata)
            apply(selected: model.account.id)
            saves.continuation.yield()
            refresh([model])
        }
        func apply(selected id: UUID?) {
            let previous = accounts.first { model in model.account.provider == provider && model.isSelected }
            switch accounts.first(where: { model in model.account.provider == provider && model.account.id == id }) {
                case .some(let selected) where selected !== previous:
                    logger.notice("Selection moved", metadata: selected.account.metadata)
                case .none where previous != nil: logger.notice("Selection cleared, live store holds no connected account", metadata: ["provider": "\(provider.name, privacy: .public)"])
                case .some, .none: logger.debug("Selection unchanged", metadata: ["provider": "\(provider.name, privacy: .public)"])
            }
            for model in accounts where model.account.provider == provider {
                let selected = model.account.id == id
                model.isSelected = selected
                if selected, !model.isConnected { refresh([model]) }
            }
        }
    }

    // --- [SWITCHES]

    func select(_ id: UUID) {
        guard let model = model(id), model.canSelect else { return }
        let account = model.account
        perform(.selecting, on: model) { [self] in
            await withStores([.live(account.provider)]) { live in
                await readSelection(account.provider, owning: live)
                guard !model.isSelected else {
                    logger.notice("Account already selected in live store", metadata: ["account": "\(id, privacy: .public)"])
                    return
                }
                let outgoing = accounts.first { other in
                    other.account.provider == account.provider && other.isSelected && other.account.id != id
                }
                await withStores(Set([id, outgoing?.account.id].compactMap(\.self).map(CredentialStore.privateAccount)), owning: live) { stores in
                    switch await client(for: account.provider).select(account, outgoing: outgoing?.account) {
                        case .success(let identity):
                            model.account.identity = identity
                            saves.continuation.yield()
                            logger.notice(
                                "Switched account",
                                metadata: model.account.metadata.merging(["outgoing": "\([outgoing?.account.id].compactMap(\.self), privacy: .public)"]) { _, stated in stated },
                            )
                        case .failure(let error): await record(error, of: .selecting, for: model, owning: stores)
                    }
                    await withTaskCancellationShield { await readSelection(account.provider, owning: stores) }
                }
            }
        }
    }

    // --- [SIGN_IN]

    func addAccount(_ provider: Provider) {
        guard canAddAccount else { return }
        authenticate(UUID(), provider: provider)
    }

    func signIn(_ id: UUID) {
        guard let model = model(id), !model.isBusy, authentication == nil, !isStopping
        else { return }
        authenticate(id, provider: model.account.provider)
    }

    func submitAuthenticationCode(_ code: String) {
        let trimmed = code.trimmingCharacters(in: .whitespacesAndNewlines)
        guard let pending = authentication, pending.phase.is(\.pending), !trimmed.isEmpty else { return }
        logger.notice("Submitted sign-in code", metadata: ["provider": "\(pending.provider.name, privacy: .public)", "account": "\(pending.id, privacy: .public)"])
        authentication?.phase = .completing
        authentication?.codes.yield(trimmed)
        authentication?.codes.finish()
    }

    func cancelAuthentication() {
        guard let pending = authentication else { return }
        switch pending.phase {
            case .pending, .completing:
                logger.notice("Sign-in cancel requested", metadata: ["provider": "\(pending.provider.name, privacy: .public)", "account": "\(pending.id, privacy: .public)"])
                authentication?.phase = .cancelling
                authenticationTask?.cancel()
            case .refused, .cancelling: return
        }
    }

    func dismissAuthentication() {
        guard (authentication?.phase).is(\.refused) else { return }
        authentication = nil
    }

    func signOut(_ id: UUID) {
        guard let model = model(id) else { return }
        let account = model.account
        run(model, .signingOut) { [self] stores in
            switch await client(for: account.provider).signOut(account, isSelected: model.isSelected) {
                case .success:
                    model.authentication = .signInRequired
                    model.usage = .unavailable
                    saves.continuation.yield()
                    logger.notice("Signed out account", metadata: account.metadata)
                case .failure(let error): await record(error, of: .signingOut, for: model, owning: stores)
            }
            if stores.contains(.live(account.provider)) {
                await withTaskCancellationShield { await readSelection(account.provider, owning: stores) }
            }
        }
    }

    func remove(_ id: UUID) {
        guard let model = model(id) else { return }
        let account = model.account
        run(model, .removing) { [self, client = client(for: account.provider)] stores in
            let removed = await Result { () async throws(ProviderError) in
                try await client.signOut(account, isSelected: model.isSelected).flatMapError { error in error.failure.requiresSignIn ? .success(()) : .failure(error) }.get()
                try await client.deletePrivateStore(id: id).get()
            }
            switch removed {
                case .success:
                    accounts.removeAll { other in other.account.id == id }
                    removeAccountDirectory(id)
                    saves.continuation.yield()
                    failures[account.id] = nil
                    logger.notice("Removed account", metadata: account.metadata)
                case .failure(let error): await record(error, of: .removing, for: model, owning: stores)
            }
            if stores.contains(.live(account.provider)) {
                await withTaskCancellationShield { await readSelection(account.provider, owning: stores) }
            }
        }
    }

    private func authenticate(_ id: UUID, provider: Provider) {
        let existing = model(id)
        let codes = AsyncStream<String>.makeStream()
        authentication = AuthenticationPresentation(id: id, provider: provider, phase: .pending, startedAt: Date(), codes: codes.continuation)
        let task = Task(name: "Sign in \(provider.name)") { [self] in
            defer { codes.continuation.finish() }
            let interval = signposter.beginInterval(AccountOperation.signingIn.name, id: signposter.makeSignpostID(), "\(id, privacy: .public)")
            defer { signposter.endInterval(AccountOperation.signingIn.name, interval) }
            logger.notice(
                "Sign-in started",
                metadata: ["provider": "\(provider.name, privacy: .public)", "account": "\(id, privacy: .public)", "new": .stringConvertible(existing == nil)],
            )
            await withStores(existing.map(stores(of:)) ?? [.privateAccount(id)]) { stores in
                let result = await client(for: provider).signIn(
                    existing.map { model in .account(model.account, isSelected: model.isSelected) } ?? .newAccount(id),
                    codes: codes.stream,
                )
                switch result {
                    case .success(let identity) where !Task.isCancelled: await connect(identity, owning: stores)
                    case .failure(let error) where !error.failure.isCancellation && !Task.isCancelled:
                        let refusal: AuthenticationRefusal =
                            if let existing, error.failure.isAccountMismatch {
                                .differentAccount(email: existing.account.identity.email)
                            } else {
                                .failure(error)
                            }
                        if let existing {
                            await record(error, of: .signingIn, for: existing, owning: stores)
                        } else {
                            logger.error("Sign-in failed for new account", error: error.failure, metadata: ["provider": "\(provider.name, privacy: .public)", "account": "\(id, privacy: .public)"])
                        }
                        authentication?.phase = .refused(refusal)
                    case .success, .failure:
                        logger.notice("Sign-in canceled", metadata: ["provider": "\(provider.name, privacy: .public)", "account": "\(id, privacy: .public)"])
                        if existing == nil { await withTaskCancellationShield { await deletePrivateStore(id, provider: provider, owning: stores) } }
                        authentication = nil
                }
                if stores.contains(.live(provider)) {
                    await withTaskCancellationShield { await readSelection(provider, owning: stores) }
                }
            }
            if Task.isCancelled, authentication?.id == id { authentication = nil }
            await readSelection(provider)
        }
        authenticationTask = task
        existing?.run(.signingIn) { await task.value }
        func connect(_ identity: AccountIdentity, owning stores: Set<CredentialStore>) async {
            let matchingAccount: () -> AccountModel? = { [self] in
                accounts.first { model in
                    model.account.id != id && model.account.provider == provider
                        && model.account.identity.isSameAccount(as: identity)
                }
            }
            await withStores(matchingAccount().map(stores(of:)) ?? [], owning: stores) { stores in
                let duplicate = matchingAccount()
                if let duplicate, duplicate.isConnected || duplicate.isBusy {
                    logger.notice(
                        "Sign-in refused, another account holds its identity",
                        metadata: ["account": "\(id, privacy: .public)", "holder": "\(duplicate.account.id, privacy: .public)", "email": "\(identity.email, privacy: .private)"],
                    )
                    if existing == nil { await deletePrivateStore(id, provider: provider, owning: stores) }
                    authentication?.phase = .refused(.alreadyConnected(email: identity.email))
                    return
                }
                let replaced = existing == nil ? duplicate : nil
                let model =
                    existing
                    ?? AccountModel(
                        account: Account(id: id, provider: provider, identity: identity),
                        authentication: .connected,
                        usage: replaced?.usage ?? .unavailable,
                    )
                if let replaced, let index = accounts.firstIndex(where: { stored in stored.account.id == replaced.account.id }) {
                    accounts[index] = model
                    logger.notice("Account replaces signed-out account with same identity", metadata: ["account": "\(id, privacy: .public)", "replaced": "\(replaced.account.id, privacy: .public)"])
                    await deletePrivateStore(replaced.account.id, provider: provider, owning: stores)
                } else if existing == nil {
                    accounts.append(model)
                }
                model.account.identity = identity
                model.authentication = .connected
                logger.notice(
                    "Signed in account",
                    metadata: ["provider": "\(provider.name, privacy: .public)", "account": "\(id, privacy: .public)", "email": "\(identity.email, privacy: .private)"],
                )
                await save()
                if stores.contains(.live(provider)) { await readSelection(provider, owning: stores) }
                authentication = nil
                switch existing {
                    case .some: await readUsage(model, for: .signingIn, owning: stores)
                    case .none: run(model, .refreshing) { [self] stores in await readUsage(model, for: .refreshing, owning: stores) }
                }
            }
        }
    }

    private func deletePrivateStore(_ id: UUID, provider: Provider, owning: Set<CredentialStore>) async {
        await withStores([.privateAccount(id)], owning: owning) { _ in
            switch await client(for: provider).deletePrivateStore(id: id) {
                case .success: logger.info("Deleted private store", metadata: ["provider": "\(provider.name, privacy: .public)", "account": "\(id, privacy: .public)"])
                case .failure(let error):
                    logger.error("Private store delete failed", error: error.failure, metadata: ["provider": "\(provider.name, privacy: .public)", "account": "\(id, privacy: .public)"])
            }
            removeAccountDirectory(id)
        }
    }

    // --- [USAGE]

    private func refresh(_ candidates: [AccountModel]) {
        guard storageState == .available, !isStopping else { return }
        let now = Date()
        for model in candidates where !model.isBusy {
            let due = model.usageReadAfter.map { date in date <= now } ?? true
            guard due || !model.isConnected else { continue }
            run(model, .refreshing) { [self] stores in
                let held = await (model.isConnected ? nil : client(for: model.account.provider).holdsCredential(for: model.account, isSelected: model.isSelected))
                switch held {
                    case .some(.success(true)):
                        logger.notice("Signed-out account holds credential, marked connected", metadata: ["account": "\(model.account.id, privacy: .public)"])
                        model.authentication = .connected
                        saves.continuation.yield()
                    case .some(.success(false)):
                        logger.debug("Signed-out account holds no credential", metadata: ["account": "\(model.account.id, privacy: .public)"])
                        return
                    case .some(.failure(let error)):
                        await record(error, of: .refreshing, for: model, owning: stores)
                        return
                    case .none: break
                }
                if due { await readUsage(model, for: .refreshing, owning: stores) }
            }
        }
    }

    private func readUsage(_ model: AccountModel, for operation: AccountOperation, owning: Set<CredentialStore>) async {
        switch await client(for: model.account.provider).usage(for: model.account, isSelected: model.isSelected) {
            case .success(let usage):
                model.usage = .current(usage)
                model.authentication = .connected
                if let cleared = failures.removeValue(forKey: model.account.id) {
                    logger.notice("Account failure cleared", metadata: ["account": "\(model.account.id, privacy: .public)", "repeats": .stringConvertible(cleared.repeats)])
                }
                logger.log(level: operation.level, "Account operation succeeded", metadata: ["operation": "\(operation.name, privacy: .public)", "account": "\(model.account.id, privacy: .public)"])
                saves.continuation.yield()
                refreshWake?.yield()
            case .failure(let error) where error.failure.isCancellation: return
            case .failure(let error):
                model.usage = model.usage.usage.map(UsageState.stale) ?? .unavailable
                await record(error, of: operation, for: model, owning: owning)
        }
    }

    // --- [FAILURES]

    private func record(_ error: ProviderError, of operation: AccountOperation, for model: AccountModel, owning: Set<CredentialStore>) async {
        guard !error.failure.isCancellation else { return }
        let cause = String(describing: error.failure)
        let last = failures[model.account.id]
        let repeated: Bool = last?.operation == operation && last?.cause == cause
        let repeats: Int = repeated ? (last?.repeats ?? 0) + 1 : 0
        failures[model.account.id] = (operation, cause, repeats)
        if repeats == 0 {
            logger.error(
                "Account operation failed",
                error: error.failure,
                metadata: model.account.metadata.merging([
                    "operation": "\(operation.name, privacy: .public)", "requiresSignIn": .stringConvertible(error.failure.requiresSignIn),
                ]) { _, stated in stated
                },
            )
        }
        guard error.failure.requiresSignIn else { return }
        model.authentication = .signInRequired
        model.usage = model.usage.usage.map(UsageState.stale) ?? .unavailable
        saves.continuation.yield()
        if owning.contains(.live(model.account.provider)) { await readSelection(model.account.provider, owning: owning) }
    }

    // --- [PERSISTENCE]

    private func save() async {
        guard storageState == .available else { return }
        let snapshot = accounts.map { model in
            SavedAccount(
                account: model.account,
                authentication: model.authentication,
                usage: model.usage.usage,
            )
        }
        switch await storage.save(snapshot) {
            case .success:
                logger.debug(
                    "Saved accounts",
                    metadata: ["count": .stringConvertible(snapshot.count), "file": "\(self.locations.accountsFile.lastPathComponent, privacy: .public)"],
                )
            case .failure(let error):
                logger.error("Accounts save failed", error: error, metadata: ["file": "\(self.locations.accountsFile.lastPathComponent, privacy: .public)"])
        }
    }

    private func removeAccountDirectory(_ id: UUID) {
        do {
            _ = try ifPresent { try FileManager.default.removeItem(at: locations.accountDirectory(id)) }
        } catch {
            logger.error("Account directory remove failed", error: error, metadata: ["account": "\(id, privacy: .public)"])
        }
    }
}
