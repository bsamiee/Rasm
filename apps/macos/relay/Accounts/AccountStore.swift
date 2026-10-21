import AppKit
import AsyncAlgorithms
import CryptoKit
import Files
import Foundation
import Network
import Observation
import Processes
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
    private var isStopping: Bool = false
    private var occupiedStores: Set<CredentialStore> = []

    private let locations: FileLocations
    private let storage: AccountStorage
    private let claude: ClaudeClient
    private let codex: CodexClient
    private let logger: Logger = Logger(category: "Accounts")
    private let saves: (stream: AsyncStream<Void>, continuation: AsyncStream<Void>.Continuation) = AsyncStream.makeStream(bufferingPolicy: .bufferingNewest(1))
    @ObservationIgnored private var root: Task<Void, Never>?
    @ObservationIgnored private var authenticationTask: Task<Void, Never>?
    @ObservationIgnored private var authenticationCodes: AsyncStream<String>.Continuation?
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
            + Provider.allCases.compactMap { provider in providerIssues[provider].map { error in UsagePresentation.issue(error, at: now) } }
        return messages.isEmpty ? nil : messages.joined(separator: "\n")
    }

    var canAddAccount: Bool { storageState == .available && !isStopping && authentication == nil }

    private var records: [Account] { accounts.map(\.account) }

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
            let needed: Set<CredentialStore> = requested().subtracting(owning)
            guard needed.isDisjoint(with: occupiedStores) else { continue }
            guard !Task.isCancelled else { return }
            occupiedStores.formUnion(needed)
            defer { occupiedStores.subtract(needed) }
            await work(owning.union(needed))
            return
        }
    }

    private func run(
        _ model: AccountModel,
        _ operation: AccountOperation,
        _ work: @escaping (Set<CredentialStore>) async -> Void,
    ) {
        guard !isStopping else { return }
        model.run(operation) { [self] in
            await withStores(
                Set([.privateAccount(model.account.id)] + (model.isSelected ? [.live(model.account.provider)] : [])),
                work,
            )
        }
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
                } catch {
                    codexPrecondition = error
                }
            }
        }
        func reconcile() async {
            await withStores([.live(.claude)]) { live in
                switch claude.pendingSwitch() {
                    case .success(.some(let pending)):
                        await withStores(
                            Set([pending.incoming, pending.outgoing].compactMap(\.self).map(CredentialStore.privateAccount)),
                            owning: live,
                        ) { _ in
                            if case .failure(let error) = await claude.completePendingSwitch(pending) { providerIssues[.claude] = error }
                        }
                    case .success(.none): break
                    case .failure(let error): providerIssues[.claude] = error
                }
            }
            guard !accounts.isEmpty else { return }
            await readCodexPrecondition()
            let orphans: [UUID]
            switch locations.orphanAccountDirectories(excluding: Set(accounts.map(\.account.id) + [authentication?.id].compactMap(\.self))) {
                case .success(let found): orphans = found
                case .failure(let error):
                    logger.error("Accounts directory listing: \(String(describing: error), privacy: .public)")
                    orphans = []
            }
            await withDiscardingTaskGroup { group in
                for (id, provider): (UUID, Provider) in orphans.flatMap({ id in Provider.allCases.map { provider in (id, provider) } }) {
                    group.addTask(name: "\(provider.name) orphan deletion") { [client = client(for: provider), logger] in
                        if case .failure(let error) = await client.deletePrivateStore(id: id) {
                            logger.error(
                                "\(provider.name, privacy: .public) orphan \(id.uuidString, privacy: .public) deletion: \(String(describing: error.failure), privacy: .public)"
                            )
                        }
                    }
                }
            }
            for id: UUID in orphans { removeAccountDirectory(id) }
        }
        func observe(file: URL, provider: Provider) async {
            do {
                for try await _ in FileWatch.events(of: file).map({ next in try next.get() }).map({ try ifPresent { try Data(contentsOf: file) }.get().map(SHA256.hash(data:)) }).removeDuplicates()
                    .dropFirst().debounce(for: .milliseconds(300))
                {
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
                logger.error(
                    "Watch on \(file.path(percentEncoded: false), privacy: .private): \(String(describing: error), privacy: .public)"
                )
            }
        }
        func observeActivation() async {
            for await _ in NotificationCenter.default.notifications(named: NSApplication.didBecomeActiveNotification) {
                loginItem = .success(SMAppService.mainApp.status)
            }
        }
        func observeNetwork() async {
            for await status: NWPath.Status in NWPathMonitor().map(\.status).removeDuplicates() where status == .satisfied && !isStopping {
                await readSelection()
                refresh(accounts, trigger: .background)
            }
        }
        func observeWake() async {
            for await _ in NSWorkspace.shared.notificationCenter.notifications(
                named: NSWorkspace.didWakeNotification
            ) {
                await readSelection()
            }
        }
        func observeRefreshLock() async {
            let lock: URL = ClaudeLock.refreshLockfile(in: claude.shared.directory)
            do {
                for try await present: Bool in FileWatch.events(of: lock).map({ next in try next.get() }).map({ FileManager.default.fileExists(atPath: lock.path(percentEncoded: false)) })
                    .removeDuplicates().dropFirst()
                where !present {
                    await readSelection(.claude)
                }
            } catch {
                logger.error(
                    "Watch on \(lock.path(percentEncoded: false), privacy: .private): \(String(describing: error), privacy: .public)"
                )
            }
        }
        func observeCodexUpdates() async {
            for await home: URL in codex.updates {
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
                let boundary: Duration? = accounts.flatMap { model in
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
                let pause: Task<Void, any Error> = Task(name: "Refresh pause") {
                    try await Task.sleep(for: delay)
                }
                refreshPause = pause
                let slept: Result<Void, any Error> = await pause.result
                guard !Task.isCancelled else { return }
                if case .success = slept { refresh(accounts, trigger: .background) }
            }
        }
    }

    func stop() async {
        isStopping = true
        root?.cancel()
        authenticationTask?.cancel()
        refreshPause?.cancel()
        for model: AccountModel in accounts { model.running?.task.cancel() }
        _ = await withTimeout(.seconds(5)) { @MainActor [self] in
            await root?.value
            await authenticationTask?.value
            for model: AccountModel in accounts { await model.running?.task.value }
            await codex.shutdown()
            return Result<Void, ProcessFailure>.success(())
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
            case .failure(let error):
                storageState = .unavailable
                storageIssue = error
                logger.error("accounts.json load: \(String(describing: error), privacy: .public)")
        }
    }

    // --- [INTERFACE]
    func setMenuBarExtraVisible(_ visible: Bool) {
        isMenuBarExtraVisible = visible
        guard visible else { return }
        lastPanelOpen = .now
        Task(name: "Panel opened") { [self] in
            await readSelection()
            refresh(accounts, trigger: .background)
        }
        refreshPause?.cancel()
    }

    func registerLoginItem() {
        loginItem = Result { try SMAppService.mainApp.register() }.map { SMAppService.mainApp.status }
    }

    func unregisterLoginItem() async {
        loginItem = await Result { try await SMAppService.mainApp.unregister() }.map { SMAppService.mainApp.status }
    }

    // --- [ACCOUNT_ACTIONS]
    func cancelOperation(_ id: UUID) {
        model(id)?.running?.task.cancel()
    }

    func setSessionPolicy(_ policy: SessionPolicy, for id: UUID) {
        guard let model: AccountModel = model(id) else { return }
        model.account.sessionPolicy = policy
        model.automaticStartAttempted = policy == .automatic && model.canStartSession(at: Date())
        saves.continuation.yield()
        if model.automaticStartAttempted { startSession(id) }
    }

    func moveAccounts(from offsets: IndexSet, to destination: Int) {
        accounts.move(fromOffsets: offsets, toOffset: destination)
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
            let outgoing: UUID? = provider == .claude ? await claude.selectionDestination(known: records) : nil
            await withStores(Set([outgoing.map(CredentialStore.privateAccount)].compactMap(\.self)), owning: live) { stores in
                switch await client(for: provider).currentSelection(known: records) {
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
                    case .failure(let error): report(error, provider: provider)
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
            apply(selected: model.account.id, provider: provider)
            saves.continuation.yield()
            refresh([model], trigger: .background)
        }
    }

    private func apply(selected id: UUID?, provider: Provider) {
        providerIssues[provider] = nil
        for model: AccountModel in accounts where model.account.provider == provider {
            let selected: Bool = model.account.id == id
            model.isSelected = selected
            if selected, !model.isConnected { refresh([model], trigger: .revalidation) }
        }
    }

    // --- [SWITCH]
    func select(_ id: UUID) {
        guard let model: AccountModel = model(id), model.canSelect, !isStopping else { return }
        let account: Account = model.account
        model.run(.selecting) { [self] in
            await withStores([.live(account.provider)]) { live in
                await readSelection(account.provider, owning: live)
                guard !model.isSelected else { return }
                let outgoing: AccountModel? = accounts.first { other in
                    other.account.provider == account.provider && other.isSelected && other.account.id != id
                }
                await withStores(Set([id, outgoing?.account.id].compactMap(\.self).map(CredentialStore.privateAccount)), owning: live) { stores in
                    switch await client(for: account.provider).select(account, outgoing: outgoing?.account) {
                        case .success(let identity):
                            model.account.identity = identity
                            model.issue = nil
                            saves.continuation.yield()
                        case .failure(let error): await record(error, for: model, owning: stores)
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
        guard case .pending? = authentication?.phase,
            let codes: AsyncStream<String>.Continuation = authenticationCodes, !trimmed.isEmpty
        else { return }
        authentication?.phase = .completing
        codes.yield(trimmed)
        codes.finish()
    }

    func cancelAuthentication() {
        switch authentication?.phase {
            case .pending, .completing:
                authentication?.phase = .cancelling
                authenticationTask?.cancel()
            case .refused, .cancelling, .none: return
        }
    }

    func dismissAuthentication() {
        guard case .refused? = authentication?.phase else { return }
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
                case .failure(let error): await record(error, for: model, owning: stores)
            }
            if stores.contains(.live(account.provider)) {
                await withTaskCancellationShield { await readSelection(account.provider, owning: stores) }
            }
        }
    }

    func remove(_ id: UUID) {
        guard let model: AccountModel = model(id) else { return }
        let account: Account = model.account
        run(model, .removing) { [self] stores in
            switch await client(for: account.provider).remove(account, isSelected: model.isSelected) {
                case .success:
                    accounts.removeAll { other in other.account.id == id }
                    removeAccountDirectory(id)
                    saves.continuation.yield()
                case .failure(let error): await record(error, for: model, owning: stores)
            }
            if stores.contains(.live(account.provider)) {
                await withTaskCancellationShield { await readSelection(account.provider, owning: stores) }
            }
        }
    }

    private func authenticate(_ id: UUID, provider: Provider) {
        let existing: AccountModel? = model(id)
        authentication = AuthenticationPresentation(
            id: id,
            provider: provider,
            phase: .pending,
            startedAt: Date(),
        )
        let codes: (stream: AsyncStream<String>, continuation: AsyncStream<String>.Continuation) =
            AsyncStream<String>.makeStream()
        authenticationCodes = codes.continuation
        let task: Task<Void, Never> = Task(name: "Sign in \(provider.name)") { [self] in
            defer {
                codes.continuation.finish()
                if authentication?.id == id || authentication == nil {
                    authenticationCodes = nil
                    authenticationTask = nil
                }
            }
            await withStores(
                Set([.privateAccount(id)] + (existing?.isSelected == true ? [.live(provider)] : []))
            ) { stores in
                let result: Result<AccountIdentity, ProviderError> = await client(for: provider).signIn(
                    existing.map { model in .account(model.account, isSelected: model.isSelected) } ?? .newAccount(id),
                    codes: codes.stream,
                )
                codes.continuation.finish()
                authenticationCodes = nil
                switch result {
                    case .success(let identity) where !Task.isCancelled:
                        await connect(identity, owning: stores)
                    case .success:
                        if existing == nil {
                            await withTaskCancellationShield { await deletePrivateStore(id, provider: provider, owning: stores) }
                        }
                        authentication = nil
                    case .failure(let error) where error.failure.isCancellation || Task.isCancelled:
                        if existing == nil {
                            await withTaskCancellationShield { await deletePrivateStore(id, provider: provider, owning: stores) }
                        }
                        authentication = nil
                    case .failure(let error):
                        let refusal: AuthenticationRefusal =
                            if let existing, error.failure.isAccountMismatch {
                                .differentAccount(email: existing.account.identity.email)
                            } else {
                                .failure(error)
                            }
                        if let existing { await record(error, for: existing, owning: stores) }
                        authentication?.phase = .refused(refusal)
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
            await withStores(
                Set(
                    [matchingAccount()].compactMap(\.self).flatMap { model in
                        [.privateAccount(model.account.id)] + (model.isSelected ? [.live(provider)] : [])
                    }
                ),
                owning: stores,
            ) { stores in
                let duplicate: AccountModel? = matchingAccount()
                let replaced: AccountModel?
                switch (duplicate, existing) {
                    case (.some(let connected), _) where connected.isConnected || connected.isBusy:
                        connected.issue = nil
                        if existing == nil { await deletePrivateStore(id, provider: provider, owning: stores) }
                        authentication?.phase = .refused(.alreadyConnected(email: identity.email))
                        return
                    case (.some(let signedOut), .none): replaced = signedOut
                    case (.some, .some), (.none, _): replaced = nil
                }
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
                    await deletePrivateStore(replaced.account.id, provider: provider, owning: stores)
                } else if existing == nil {
                    accounts.append(model)
                }
                model.account.identity = identity
                model.authentication = .connected
                model.retryAfter = nil
                model.issue = nil
                await save()
                if stores.contains(.live(provider)) { await readSelection(provider, owning: stores) }
                authentication = nil
                switch existing {
                    case .some: await readUsage(model, owning: stores)
                    case .none: run(model, .refreshing) { [self] stores in await readUsage(model, owning: stores) }
                }
            }
        }
    }

    private func deletePrivateStore(_ id: UUID, provider: Provider, owning: Set<CredentialStore>) async {
        await withStores([.privateAccount(id)], owning: owning) { _ in
            let result: Result<Void, ProviderError> = await client(for: provider).deletePrivateStore(id: id)
            removeAccountDirectory(id)
            switch result {
                case .success: providerIssues[provider] = nil
                case .failure(let error): report(error, provider: provider)
            }
        }
    }

    // --- [USAGE]
    func startSession(_ id: UUID) {
        guard let model: AccountModel = model(id), model.isConnected else { return }
        run(model, .starting) { [self] stores in
            guard model.isConnected else { return }
            await apply(await client(for: model.account.provider).startSession(for: model.account, isSelected: model.isSelected), to: model, owning: stores)
        }
    }

    private func apply(_ result: Result<AccountUsage, ProviderError>, to model: AccountModel, owning: Set<CredentialStore>) async {
        switch result {
            case .success(let usage):
                let now: Date = Date()
                let carried: AccountUsage = usage.keepingResets(from: model.usage.usage, at: now)
                model.usage = .current(carried)
                model.authentication = .connected
                model.retryAfter = nil
                model.issue = nil
                if case .running = carried.availability(at: now) { model.automaticStartAttempted = false }
                saves.continuation.yield()
                refreshPause?.cancel()
                if !isStopping, model.account.sessionPolicy == .automatic, !model.automaticStartAttempted, model.canStartSession(at: Date()) {
                    model.automaticStartAttempted = true
                    startSession(model.account.id)
                }
            case .failure(let error) where error.failure.isCancellation: return
            case .failure(let error):
                model.usage = model.usage.usage.map(UsageState.stale) ?? .unavailable
                await record(error, for: model, owning: owning)
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
                        model.authentication = .connected
                        model.issue = nil
                        saves.continuation.yield()
                    case .some(.success(false)): return
                    case .some(.failure(let error)):
                        await record(error, for: model, owning: stores)
                        return
                    case .none: break
                }
                if due { await readUsage(model, owning: stores) }
            }
        }
    }

    private func readUsage(_ model: AccountModel, owning: Set<CredentialStore>) async {
        await apply(await client(for: model.account.provider).usage(for: model.account, isSelected: model.isSelected), to: model, owning: owning)
    }

    // --- [FAILURES]
    private func record(_ error: ProviderError, for model: AccountModel, owning: Set<CredentialStore>) async {
        guard !error.failure.isCancellation else { return }
        logger.error(
            "\(model.running?.kind.description ?? "Account operation", privacy: .public) failed for \(model.account.id.uuidString, privacy: .public) \(model.account.identity.email, privacy: .private): \(String(describing: error), privacy: .public)"
        )
        model.issue = error
        if let retryAfter: Date = error.failure.retryAfter {
            model.retryAfter = max(model.retryAfter ?? retryAfter, retryAfter)
            saves.continuation.yield()
            refreshPause?.cancel()
        }
        guard error.failure.requiresSignIn else { return }
        model.authentication = .signInRequired
        saves.continuation.yield()
        if owning.contains(.live(model.account.provider)) { await readSelection(model.account.provider, owning: owning) }
    }

    private func report(_ error: ProviderError, provider: Provider) {
        guard !error.failure.isCancellation else { return }
        providerIssues[provider] = error
        logger.error("\(provider.name, privacy: .public) provider operation failed: \(String(describing: error), privacy: .public)")
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
            case .success: storageIssue = nil
            case .failure(let error):
                storageIssue = error
                logger.error("accounts.json save: \(String(describing: error), privacy: .public)")
        }
    }

    private func removeAccountDirectory(_ id: UUID) {
        if case .failure(let error) = ifPresent({ try FileManager.default.removeItem(at: locations.accountDirectory(id)) }) {
            logger.error("Account directory \(id.uuidString, privacy: .public) removal: \(String(describing: error), privacy: .public)")
        }
    }
}
