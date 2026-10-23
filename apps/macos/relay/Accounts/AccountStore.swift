import AppKit
import AsyncAlgorithms
import CasePathsCore
import CurrentProcess
import Files
import Foundation
import Network
import Observation
import Results
import ServiceManagement
import Subprocess
import SwiftUI
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
    private(set) var codexPrecondition: CodexFailure?
    private var storageIssue: AccountStorageFailure?
    private var providerIssues: [Provider: ProviderError] = [:]
    private var pendingSwitchIssue: ProviderError?
    private var isStopping: Bool = false
    private var occupiedStores: Set<CredentialStore> = []

    private let locations: FileLocations
    private let storage: AccountStorage
    private let claude: ClaudeClient
    private let codex: CodexClient
    private let logger: Logger = Logger(for: AccountStore.self)
    private let saves: (stream: AsyncStream<Void>, continuation: AsyncStream<Void>.Continuation) = AsyncStream.makeStream(bufferingPolicy: .bufferingNewest(1))
    @ObservationIgnored private var root: Task<Void, Never>?
    @ObservationIgnored private var authenticationTask: Task<Void, Never>?
    @ObservationIgnored private var refreshPause: Task<Void, any Error>?
    @ObservationIgnored private var lastPanelOpen: ContinuousClock.Instant?

    init(environment: [Subprocess.Environment.Key: String]) {
        locations = FileLocations(environment: environment)
        storage = AccountStorage(fileURL: locations.accountsFile)
        claude = ClaudeClient(paths: locations, environment: environment)
        codex = CodexClient(paths: locations, environment: environment)
    }

    func issue(at now: Date) -> String? {
        let messages: [String] =
            [storageIssue?.localizedDescription].compactMap(\.self)
            + ([pendingSwitchIssue] + Provider.allCases.map { provider in providerIssues[provider] }).compactMap { error in error?.description(at: now) }
        return messages.isEmpty ? nil : messages.joined(separator: "\n")
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

    private var signposter: OSSignposter { OSSignposter(logger: logger) }

    // --- [CREDENTIAL_OWNERSHIP]
    private func withStores(
        _ requested: @autoclosure () -> Set<CredentialStore>,
        owning: Set<CredentialStore> = [],
        _ work: (Set<CredentialStore>) async -> Void,
    ) async {
        for await _ in Observations({ self.occupiedStores }) {
            let needed: Set<CredentialStore> = requested().subtracting(owning)
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
            let account: Account = model.account
            let interval: OSSignpostIntervalState = signposter.beginInterval(operation.name, id: signposter.makeSignpostID(), "\(account.id, privacy: .public)")
            logger.log(
                level: operation.level,
                "\(operation.name, privacy: .public) started for \(account.provider.name, privacy: .public) account \(account.id, privacy: .public) \(account.identity.email, privacy: .private)",
            )
            await work()
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
                group.addTask(name: "Claude refresh lock") { await observeRefreshLock() }
                group.addTask(name: "Codex selection") { await observe(file: self.codex.liveAuthFile, provider: .openAI) }
                group.addTask(name: "Codex configuration") { await observe(file: self.codex.liveHome.appending(path: "config.toml"), provider: .openAI) }
                group.addTask(name: "Codex rate limits") { await observeCodexUpdates() }
                group.addTask(name: "Refresh schedule") { await schedule() }
            }
        }
        func readCodexPrecondition() async {
            guard accounts.contains(where: { model in model.account.provider == .openAI }) else { return }
            await withStores([.live(.openAI)]) { _ in
                do throws(CodexFailure) {
                    try await codex.preconditions()
                    codexPrecondition = nil
                    logger.debug("Codex switch preconditions hold")
                } catch {
                    codexPrecondition = error
                    logger.error("Codex switch check failed with \(error, privacy: .public)")
                }
            }
        }
        func reconcile() async {
            await withStores([.live(.claude)]) { live in
                switch claude.pendingSwitch() {
                    case .success(.some(let pending)):
                        logger.notice("Completing pending Claude switch over private stores \(pending.copies, privacy: .public)")
                        await withStores(
                            Set(pending.copies.map(CredentialStore.privateAccount)),
                            owning: live,
                        ) { _ in
                            switch await claude.completePendingSwitch(pending) {
                                case .success: logger.notice("Completed Claude switch to \(pending.incoming, privacy: .public)")
                                case .failure(let error):
                                    pendingSwitchIssue = error
                                    logger.error("Claude switch to \(pending.incoming, privacy: .public) failed with \(error.failure, privacy: .public)")
                            }
                        }
                    case .success(.none): break
                    case .failure(let error):
                        pendingSwitchIssue = error
                        logger.error("Pending Claude switch read failed with \(error.failure, privacy: .public)")
                }
            }
            guard !accounts.isEmpty else { return }
            await readCodexPrecondition()
            let orphans: [UUID]
            switch locations.orphanAccountDirectories(excluding: Set(accounts.map(\.account.id) + [authentication?.id].compactMap(\.self))) {
                case .success(let found): orphans = found
                case .failure(let error):
                    logger.error("Accounts directory listing failed with \(error, privacy: .public)")
                    orphans = []
            }
            guard !orphans.isEmpty else { return }
            logger.notice("Deleting orphan account directories \(orphans, privacy: .public)")
            await withDiscardingTaskGroup { group in
                for (id, provider): (UUID, Provider) in orphans.flatMap({ id in Provider.allCases.map { provider in (id, provider) } }) {
                    group.addTask(name: "\(provider.name) orphan deletion") { [client = client(for: provider), logger] in
                        if case .failure(let error) = await client.deletePrivateStore(id: id) {
                            logger.error("\(provider.name, privacy: .public) orphan \(id, privacy: .public) delete failed with \(error.failure, privacy: .public)")
                        }
                    }
                }
            }
            for id: UUID in orphans { removeAccountDirectory(id) }
        }
        func observe(file: URL, provider: Provider) async {
            do {
                for try await _ in FileWatch.events(of: file).map({ next in
                    _ = try next.get()
                    return try ifPresent { try Data(contentsOf: file) }.get()
                }).removeDuplicates()
                    .dropFirst().debounce(for: .milliseconds(300))
                {
                    logger.debug("\(provider.name, privacy: .public) live store \(file.lastPathComponent, privacy: .public) changed")
                    let before: [UUID: Bool] = Dictionary(
                        uniqueKeysWithValues: accounts.map { model in (model.account.id, model.isSelected) }
                    )
                    await readSelection(provider)
                    if provider == .openAI { await readCodexPrecondition() }
                    let changed: [AccountModel] = accounts.filter { model in
                        before[model.account.id] != model.isSelected
                    }
                    refresh(changed, trigger: .background)
                }
            } catch {
                logger.error("\(provider.name, privacy: .public) live store \(file.lastPathComponent, privacy: .public) watch failed with \(error, privacy: .public)")
            }
        }
        func observeActivation() async {
            for await _ in NotificationCenter.default.notifications(named: NSApplication.didBecomeActiveNotification) {
                loginItem = .success(SMAppService.mainApp.status)
            }
        }
        func observeNetwork() async {
            for await status: NWPath.Status in NWPathMonitor().map(\.status).removeDuplicates() where status == .satisfied && !isStopping {
                logger.debug("Network path satisfied")
                await readSelection()
                refresh(accounts, trigger: .background)
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
        func observeRefreshLock() async {
            let lock: URL = ClaudeLock.refreshLockfile(in: claude.shared.directory)
            do {
                for try await present: Bool in FileWatch.events(of: lock).map({ next in
                    _ = try next.get()
                    return FileManager.default.fileExists(atPath: lock.path(percentEncoded: false))
                }).removeDuplicates().dropFirst()
                where !present {
                    logger.debug("Claude refresh lock released")
                    await readSelection(.claude)
                }
            } catch {
                logger.error("Claude refresh lock watch failed with \(error, privacy: .public)")
            }
        }
        func observeCodexUpdates() async {
            for await home: URL in codex.updates {
                logger.debug("Codex rate limits updated for \(home.path(percentEncoded: false), privacy: .private)")
                let owner: () -> AccountModel? = { [self] in
                    accounts.first { model in model.account.provider == .openAI && codex.home(for: model.account, isSelected: model.isSelected) == home }
                }
                while let operation: RunningOperation = owner()?.running { await operation.task.value }
                if let model: AccountModel = owner() { refresh([model], trigger: .immediate) }
            }
        }
        func schedule() async {
            while !Task.isCancelled {
                let now: Date = Date()
                let base: Duration =
                    switch (isMenuBarExtraVisible, lastPanelOpen?.duration(to: .now)) {
                        case (true, _): .seconds(AccountModel.usageReadSpacing)
                        case (false, .some(..<Duration.seconds(15 * 60))): .seconds(5 * 60)
                        case (false, .some(..<Duration.seconds(60 * 60))): .seconds(15 * 60)
                        case (false, _): .seconds(30 * 60)
                    }
                let boundary: Duration? = accounts.flatMap { model -> [Date] in
                    let resets: [Date] =
                        model.usage.usage.map { usage in
                            usage.windows.compactMap(\.resetsAt).filter { date in date > usage.observedAt }
                        } ?? []
                    let earliest: Date? = model.usageReadAfter
                    return (resets + [model.retryAfter].compactMap(\.self))
                        .map { date in max(date, earliest ?? date) }
                }
                .filter { date in date > now }
                .min()
                .map { date in .seconds(date.timeIntervalSince(now) + 1) }
                let delay: Duration = min(base, boundary ?? base)
                logger.debug("Next refresh in \(delay, privacy: .public)")
                let pause: Task<Void, any Error> = Task(name: "Refresh pause") { try await Task.sleep(for: delay) }
                refreshPause = pause
                let slept: Result<Void, any Error> = await withTaskCancellationHandler {
                    await pause.result
                } onCancel: {
                    pause.cancel()
                }
                guard !Task.isCancelled else { return }
                if case .success = slept { refresh(accounts, trigger: .background) }
            }
        }
    }

    func stop() async {
        logger.notice("Stopping account work")
        isStopping = true
        root?.cancel()
        authenticationTask?.cancel()
        for model: AccountModel in accounts { model.running?.task.cancel() }
        let stopped: Result<Void, Deadline> = await withTimeout(.seconds(5)) { @MainActor [self] in
            await root?.value
            await authenticationTask?.value
            for model: AccountModel in accounts { await model.running?.task.value }
            await codex.shutdown()
            return .success(())
        }
        switch stopped {
            case .success: logger.notice("Stopped account work")
            case .failure(let error): logger.error("Account work stop failed with \(error, privacy: .public)")
        }
        await save()
    }

    private func load() async {
        switch await storage.load(at: Date()) {
            case .success(let loaded):
                accounts = loaded.map { saved in
                    AccountModel(
                        account: saved.account,
                        authentication: saved.authentication,
                        usage: saved.usage.map(UsageState.stale) ?? .unavailable,
                        retryAfter: saved.retryAfter,
                    )
                }
                storageState = .available
                logger.notice("Loaded \(loaded.count, privacy: .public) accounts from accounts.json")
            case .failure(let error):
                storageState = .unavailable
                storageIssue = error
                logger.error("accounts.json load failed with \(error, privacy: .public)")
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
            refresh(accounts, trigger: .background)
        }
        refreshPause?.cancel()
    }

    func registerLoginItem() {
        loginItem = Result { try SMAppService.mainApp.register() }.map { SMAppService.mainApp.status }
        switch loginItem {
            case .success: logger.notice("Registered login item")
            case .failure(let error): logger.error("Login item register failed with \(error, privacy: .public)")
        }
    }

    func unregisterLoginItem() async {
        loginItem = await Result { try await SMAppService.mainApp.unregister() }.map { SMAppService.mainApp.status }
        switch loginItem {
            case .success: logger.notice("Unregistered login item")
            case .failure(let error): logger.error("Login item unregister failed with \(error, privacy: .public)")
        }
    }

    // --- [ACCOUNT_ACTIONS]
    func cancelOperation(_ id: UUID) {
        guard let running: RunningOperation = model(id)?.running else { return }
        logger.notice("\(running.kind.name, privacy: .public) cancel requested for account \(id, privacy: .public)")
        running.task.cancel()
    }

    func setSessionPolicy(_ policy: SessionPolicy, for id: UUID) {
        guard let model: AccountModel = model(id) else { return }
        logger.notice("Account \(id, privacy: .public) session policy set to \(policy.rawValue, privacy: .public)")
        model.account.sessionPolicy = policy
        model.automaticStartAttempted = policy == .automatic && model.canStartSession(at: Date())
        saves.continuation.yield()
        if model.automaticStartAttempted { startSession(id) }
    }

    func moveAccounts(from offsets: IndexSet, to destination: Int) {
        accounts.move(fromOffsets: offsets, toOffset: destination)
        logger.info("Moved accounts at \(Array(offsets), privacy: .public) to position \(destination, privacy: .public)")
        saves.continuation.yield()
    }

    // --- [SELECTION]
    private func readSelection() async {
        guard storageState == .available, !isStopping else { return }
        await withDiscardingTaskGroup { group in
            for provider: Provider in Provider.allCases {
                group.addTask(name: "\(provider.name) selection") { await self.readSelection(provider) }
            }
        }
    }

    private func readSelection(_ provider: Provider, owning: Set<CredentialStore> = []) async {
        await withStores([.live(provider)], owning: owning) { live in
            let selected: Account? = accounts.first { model in model.account.provider == provider && model.isSelected }?.account
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
                            if let identity { register(identity) } else { apply(selected: nil, provider: provider) }
                        }
                    case .failure(let error) where error.failure.isCancellation: break
                    case .failure(let error):
                        providerIssues[provider] = error
                        logger.error("\(provider.name, privacy: .public) selection read failed with \(error.failure, privacy: .public)")
                }
            }
        }
        func register(_ identity: AccountIdentity) {
            if let existing: AccountModel = accounts.first(where: { model in
                model.account.provider == provider && model.account.identity.isSameAccount(as: identity)
            }) {
                apply(selected: existing.account.id, provider: provider)
                return
            }
            let model: AccountModel = AccountModel(
                account: Account(id: UUID(), provider: provider, identity: identity, sessionPolicy: .manual),
                authentication: .connected,
                usage: .unavailable,
                retryAfter: nil,
            )
            accounts.append(model)
            logger.notice(
                "Registered \(provider.name, privacy: .public) account \(model.account.id, privacy: .public) \(identity.email, privacy: .private) from live store"
            )
            apply(selected: model.account.id, provider: provider)
            saves.continuation.yield()
            refresh([model], trigger: .background)
        }
    }

    private func apply(selected id: UUID?, provider: Provider) {
        providerIssues[provider] = nil
        let previous: AccountModel? = accounts.first { model in model.account.provider == provider && model.isSelected }
        switch accounts.first(where: { model in model.account.provider == provider && model.account.id == id }) {
            case .some(let selected) where selected !== previous:
                logger.notice("\(provider.name, privacy: .public) selection moved to account \(selected.account.id, privacy: .public) \(selected.account.identity.email, privacy: .private)")
            case .none where previous != nil: logger.notice("\(provider.name, privacy: .public) selection cleared, live store holds no connected account")
            case .some, .none: logger.debug("\(provider.name, privacy: .public) selection unchanged")
        }
        for model: AccountModel in accounts where model.account.provider == provider {
            let selected: Bool = model.account.id == id
            model.isSelected = selected
            if selected, !model.isConnected { refresh([model], trigger: .revalidation) }
        }
    }

    // --- [SWITCH]
    func select(_ id: UUID) {
        guard let model: AccountModel = model(id), model.canSelect else { return }
        let account: Account = model.account
        perform(.selecting, on: model) { [self] in
            await withStores([.live(account.provider)]) { live in
                await readSelection(account.provider, owning: live)
                guard !model.isSelected else {
                    logger.notice("Account \(id, privacy: .public) already selected in live store")
                    return
                }
                let outgoing: AccountModel? = accounts.first { other in
                    other.account.provider == account.provider && other.isSelected && other.account.id != id
                }
                await withStores(Set([id, outgoing?.account.id].compactMap(\.self).map(CredentialStore.privateAccount)), owning: live) { stores in
                    switch await client(for: account.provider).select(account, outgoing: outgoing?.account) {
                        case .success(let identity):
                            model.account.identity = identity
                            model.issue = nil
                            saves.continuation.yield()
                            logger.notice(
                                "Switched \(account.provider.name, privacy: .public) to account \(id, privacy: .public) \(identity.email, privacy: .private) from \([outgoing?.account.id].compactMap(\.self), privacy: .public)"
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
        guard let model: AccountModel = model(id), !model.isBusy, authentication == nil, !isStopping
        else { return }
        authenticate(id, provider: model.account.provider)
    }

    func submitAuthenticationCode(_ code: String) {
        let trimmed: String = code.trimmingCharacters(in: .whitespacesAndNewlines)
        guard let pending: AuthenticationPresentation = authentication, pending.phase.is(\.pending), !trimmed.isEmpty else { return }
        logger.notice("Submitted sign-in code for \(pending.provider.name, privacy: .public) account \(pending.id, privacy: .public)")
        authentication?.phase = .completing
        authentication?.codes.yield(trimmed)
        authentication?.codes.finish()
    }

    func cancelAuthentication() {
        guard let pending: AuthenticationPresentation = authentication else { return }
        switch pending.phase {
            case .pending, .completing:
                logger.notice("\(AccountOperation.signingIn.name, privacy: .public) cancel requested for \(pending.provider.name, privacy: .public) account \(pending.id, privacy: .public)")
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
        guard let model: AccountModel = model(id) else { return }
        let account: Account = model.account
        run(model, .signingOut) { [self] stores in
            switch await client(for: account.provider).signOut(account, isSelected: model.isSelected) {
                case .success:
                    model.authentication = .signInRequired
                    model.usage = .unavailable
                    model.issue = nil
                    saves.continuation.yield()
                    logger.notice("Signed out \(account.provider.name, privacy: .public) account \(id, privacy: .public) \(account.identity.email, privacy: .private)")
                case .failure(let error): await record(error, of: .signingOut, for: model, owning: stores)
            }
            if stores.contains(.live(account.provider)) {
                await withTaskCancellationShield { await readSelection(account.provider, owning: stores) }
            }
        }
    }

    func remove(_ id: UUID) {
        guard let model: AccountModel = model(id) else { return }
        let account: Account = model.account
        run(model, .removing) { [self, client = client(for: account.provider)] stores in
            let removed: Result<Void, ProviderError> = await Result { () async throws(ProviderError) in
                try await client.signOut(account, isSelected: model.isSelected).flatMapError { error in error.failure.requiresSignIn ? .success(()) : .failure(error) }.get()
                try await client.deletePrivateStore(id: id).get()
            }
            switch removed {
                case .success:
                    accounts.removeAll { other in other.account.id == id }
                    removeAccountDirectory(id)
                    saves.continuation.yield()
                    logger.notice("Removed \(account.provider.name, privacy: .public) account \(id, privacy: .public) \(account.identity.email, privacy: .private)")
                case .failure(let error): await record(error, of: .removing, for: model, owning: stores)
            }
            if stores.contains(.live(account.provider)) {
                await withTaskCancellationShield { await readSelection(account.provider, owning: stores) }
            }
        }
    }

    private func authenticate(_ id: UUID, provider: Provider) {
        let existing: AccountModel? = model(id)
        let codes: (stream: AsyncStream<String>, continuation: AsyncStream<String>.Continuation) = AsyncStream<String>.makeStream()
        authentication = AuthenticationPresentation(id: id, provider: provider, phase: .pending, startedAt: Date(), codes: codes.continuation)
        let task: Task<Void, Never> = Task(name: "Sign in \(provider.name)") { [self] in
            defer { codes.continuation.finish() }
            let interval: OSSignpostIntervalState = signposter.beginInterval(AccountOperation.signingIn.name, id: signposter.makeSignpostID(), "\(id, privacy: .public)")
            defer { signposter.endInterval(AccountOperation.signingIn.name, interval) }
            logger.notice(
                "\(AccountOperation.signingIn.name, privacy: .public) started for \(provider.name, privacy: .public) account \(id, privacy: .public), new \(existing == nil, privacy: .public)"
            )
            await withStores(existing.map(stores(of:)) ?? [.privateAccount(id)]) { stores in
                let result: Result<AccountIdentity, ProviderError> = await client(for: provider).signIn(
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
                            logger.error(
                                "\(AccountOperation.signingIn.name, privacy: .public) failed for new \(provider.name, privacy: .public) account \(id, privacy: .public) with \(error.failure, privacy: .public)"
                            )
                        }
                        authentication?.phase = .refused(refusal)
                    case .success, .failure:
                        logger.notice("\(AccountOperation.signingIn.name, privacy: .public) canceled for \(provider.name, privacy: .public) account \(id, privacy: .public)")
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
                let duplicate: AccountModel? = matchingAccount()
                if let duplicate, duplicate.isConnected || duplicate.isBusy {
                    logger.notice(
                        "\(AccountOperation.signingIn.name, privacy: .public) for account \(id, privacy: .public) refused, account \(duplicate.account.id, privacy: .public) holds \(identity.email, privacy: .private)"
                    )
                    duplicate.issue = nil
                    if existing == nil { await deletePrivateStore(id, provider: provider, owning: stores) }
                    authentication?.phase = .refused(.alreadyConnected(email: identity.email))
                    return
                }
                let replaced: AccountModel? = existing == nil ? duplicate : nil
                let model: AccountModel =
                    existing
                    ?? AccountModel(
                        account: Account(
                            id: id,
                            provider: provider,
                            identity: identity,
                            sessionPolicy: replaced?.account.sessionPolicy ?? .manual,
                        ),
                        authentication: .connected,
                        usage: replaced?.usage ?? .unavailable,
                        retryAfter: nil,
                    )
                if let replaced, let index: Int = accounts.firstIndex(where: { stored in stored.account.id == replaced.account.id }) {
                    accounts[index] = model
                    logger.notice("Account \(id, privacy: .public) replaces signed-out account \(replaced.account.id, privacy: .public) with same identity")
                    await deletePrivateStore(replaced.account.id, provider: provider, owning: stores)
                } else if existing == nil {
                    accounts.append(model)
                }
                model.account.identity = identity
                model.authentication = .connected
                model.retryAfter = nil
                model.issue = nil
                logger.notice("Signed in \(provider.name, privacy: .public) account \(id, privacy: .public) \(identity.email, privacy: .private)")
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
                case .success: logger.info("Deleted \(provider.name, privacy: .public) private store of account \(id, privacy: .public)")
                case .failure(let error): logger.error("\(provider.name, privacy: .public) private store delete of account \(id, privacy: .public) failed with \(error.failure, privacy: .public)")
            }
            removeAccountDirectory(id)
        }
    }

    // --- [USAGE]
    func startSession(_ id: UUID) {
        guard let model: AccountModel = model(id), model.isConnected else { return }
        run(model, .starting) { [self] stores in
            guard model.isConnected else { return }
            await apply(await client(for: model.account.provider).startSession(for: model.account, isSelected: model.isSelected), of: .starting, to: model, owning: stores)
        }
    }

    private func apply(_ result: Result<AccountUsage, ProviderError>, of operation: AccountOperation, to model: AccountModel, owning: Set<CredentialStore>) async {
        switch result {
            case .success(let usage):
                let now: Date = Date()
                let carried: AccountUsage = usage.fillingResets { kind in
                    model.usage.usage?.windows.first { earlier in earlier.kind == kind }?.resetsAt.flatMap { reset in reset > now ? reset : nil }
                }
                model.usage = .current(carried)
                model.authentication = .connected
                model.retryAfter = nil
                model.issue = nil
                let running: Bool = carried.availability(at: now).is(\.running)
                if running { model.automaticStartAttempted = false }
                logger.log(
                    level: operation.level,
                    "\(operation.name, privacy: .public) succeeded for account \(model.account.id, privacy: .public), session running \(running, privacy: .public)",
                )
                saves.continuation.yield()
                refreshPause?.cancel()
                if !isStopping, model.account.sessionPolicy == .automatic, !model.automaticStartAttempted, model.canStartSession(at: Date()) {
                    model.automaticStartAttempted = true
                    logger.notice("Starting automatic session for account \(model.account.id, privacy: .public)")
                    startSession(model.account.id)
                }
            case .failure(let error) where error.failure.isCancellation: return
            case .failure(let error):
                model.usage = model.usage.usage.map(UsageState.stale) ?? .unavailable
                await record(error, of: operation, for: model, owning: owning)
        }
    }

    private func refresh(_ candidates: [AccountModel], trigger: RefreshTrigger) {
        guard storageState == .available, !isStopping else { return }
        let now: Date = Date()
        for model: AccountModel in candidates where !model.isBusy {
            let due: Bool = (trigger == .immediate ? model.retryAfter : model.usageReadAfter).map { date in date <= now } ?? true
            let refreshes: Bool =
                switch (model.isConnected, model.isSelected) {
                    case (true, _): due
                    case (false, true): due && trigger == .revalidation
                    case (false, false): true
                }
            guard refreshes else { continue }
            run(model, .refreshing) { [self] stores in
                let credential: Result<Bool, ProviderError>? =
                    if !model.isConnected, !model.isSelected {
                        await client(for: model.account.provider).holdsCredential(for: model.account)
                    } else {
                        nil
                    }
                switch credential {
                    case .some(.success(true)):
                        logger.notice("Signed-out account \(model.account.id, privacy: .public) holds credential, marked connected")
                        model.authentication = .connected
                        model.issue = nil
                        saves.continuation.yield()
                    case .some(.success(false)):
                        logger.debug("Signed-out account \(model.account.id, privacy: .public) holds no credential")
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
        await apply(await client(for: model.account.provider).usage(for: model.account, isSelected: model.isSelected), of: operation, to: model, owning: owning)
    }

    // --- [FAILURES]
    private func record(_ error: ProviderError, of operation: AccountOperation, for model: AccountModel, owning: Set<CredentialStore>) async {
        guard !error.failure.isCancellation else { return }
        logger.error(
            "\(operation.name, privacy: .public) failed for \(model.account.provider.name, privacy: .public) account \(model.account.id, privacy: .public) \(model.account.identity.email, privacy: .private) with \(error.failure, privacy: .public), requires sign-in \(error.failure.requiresSignIn, privacy: .public)"
        )
        model.issue = error
        if let retryAfter: Date = error.failure.retryAfter {
            let held: Date = max(model.retryAfter ?? retryAfter, retryAfter)
            model.retryAfter = held
            logger.notice("Account \(model.account.id, privacy: .public) usage reads wait until \(held, privacy: .public)")
            saves.continuation.yield()
            refreshPause?.cancel()
        }
        guard error.failure.requiresSignIn else { return }
        model.authentication = .signInRequired
        saves.continuation.yield()
        if owning.contains(.live(model.account.provider)) { await readSelection(model.account.provider, owning: owning) }
    }

    // --- [PERSISTENCE]
    private func save() async {
        guard storageState == .available else { return }
        let snapshot: [SavedAccount] = accounts.map { model in
            SavedAccount(
                account: model.account,
                authentication: model.authentication,
                usage: model.usage.usage,
                retryAfter: model.retryAfter,
            )
        }
        switch await storage.save(snapshot) {
            case .success:
                storageIssue = nil
                logger.debug("Saved \(snapshot.count, privacy: .public) accounts to accounts.json")
            case .failure(let error):
                storageIssue = error
                logger.error("accounts.json save failed with \(error, privacy: .public)")
        }
    }

    private func removeAccountDirectory(_ id: UUID) {
        if case .failure(let error) = ifPresent({ try FileManager.default.removeItem(at: locations.accountDirectory(id)) }) {
            logger.error("Account \(id, privacy: .public) directory remove failed with \(error, privacy: .public)")
        }
    }
}
