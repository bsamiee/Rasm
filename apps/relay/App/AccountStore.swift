import AppKit
import Foundation
import Network
import OSLog
import Observation
import SwiftUI

// --- [SERVICES] ------------------------------------------------------------------------

@Observable
final class AccountStore {
    // --- [STATE]
    private(set) var accounts: [AccountModel] = []
    private(set) var authentication: AuthenticationPresentation?
    private(set) var loginItem: LoginItem = .current
    private(set) var isLoading: Bool = true
    private(set) var isMenuBarExtraVisible: Bool = false
    private(set) var codexPrecondition: CodexFailure?
    private var storageIssue: AccountStorageFailure?
    private var providerIssues: [Provider: ProviderError] = [:]
    private var isStorageAvailable: Bool = false
    private var isStopping: Bool = false

    @ObservationIgnored private let locations: FileLocations
    @ObservationIgnored private let storage: AccountStorage
    @ObservationIgnored private let claude: ClaudeClient
    @ObservationIgnored private let codex: CodexClient
    @ObservationIgnored private let logger: Logger = Logger(
        subsystem: "app.rasm.relay",
        category: "Accounts",
    )
    @ObservationIgnored private var root: Task<Void, Never>?
    @ObservationIgnored private var authenticationTask: Task<Void, Never>?
    @ObservationIgnored private var authenticationCodes: AsyncStream<String>.Continuation?
    @ObservationIgnored private var saveTask: Task<Void, Never>?
    @ObservationIgnored private var refreshPause: Task<Void, any Error>?
    @ObservationIgnored private var lastPanelOpen: ContinuousClock.Instant?

    init(environment: [String: String]) {
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

    var canAddAccount: Bool { isStorageAvailable && !isStopping && authentication == nil }

    var isSwitching: Bool { accounts.contains { model in model.running?.kind == .selecting } }

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

    // --- [LIFECYCLE]
    func start() {
        guard root == nil else { return }
        root = Task(name: "Accounts") { [self] in
            await load()
            guard isStorageAvailable, !isStopping else { return }
            await reconcile()
            await readSelection()
            await withDiscardingTaskGroup { group in
                group.addTask(name: "Network") { await self.observeNetwork() }
                group.addTask(name: "Wake") { await self.observeWake() }
                group.addTask(name: "Claude selection") {
                    await self.observe(file: self.claude.shared.configFile)
                }
                group.addTask(name: "Claude refresh lock") {
                    await self.observeRefreshLock(ClaudeLock.refreshLockfile(in: self.claude.shared.directory))
                }
                group.addTask(name: "Codex selection") { await self.observe(file: self.codex.liveAuthFile) }
                group.addTask(name: "Codex rate limits") { await self.observeCodexUpdates() }
                group.addTask(name: "Refresh schedule") { await self.schedule() }
            }
        }
    }

    func stop() async {
        isStopping = true
        root?.cancel()
        authenticationTask?.cancel()
        refreshPause?.cancel()
        for model: AccountModel in accounts { model.running?.task.cancel() }
        _ = await ProcessRun.withDeadline(.seconds(5)) {
            Result<Void, ProcessFailure>.success(await self.awaitOutstanding())
        }
        await save()
    }

    private func awaitOutstanding() async {
        await root?.value
        await authenticationTask?.value
        for model: AccountModel in accounts { await model.running?.task.value }
        await codex.shutdown()
    }

    private func load() async {
        defer { isLoading = false }
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
                isStorageAvailable = true
            case .failure(let error):
                storageIssue = error
                logger.error("accounts.json load: \(String(describing: error), privacy: .public)")
        }
    }

    private func reconcile() async {
        if case .failure(let error) = await claude.completePendingSwitch().mapError(ProviderError.init(failure:)) {
            providerIssues[.claude] = error
        }
        codexPrecondition = (await codex.preconditions()).failure
        guard !accounts.isEmpty else { return }
        let orphans: [UUID]
        switch locations.orphanAccountDirectories(excluding: Set(accounts.map(\.account.id))) {
            case .success(let found): orphans = found
            case .failure(let error):
                logger.error("Accounts directory listing: \(String(describing: error), privacy: .public)")
                orphans = []
        }
        await withDiscardingTaskGroup { group in
            for id: UUID in orphans {
                for provider: Provider in Provider.allCases {
                    group.addTask(name: "\(provider.name) orphan deletion") { [client = client(for: provider), logger] in
                        if case .failure(let error) = await client.deletePrivateStore(id: id) {
                            logger.error(
                                "\(provider.name, privacy: .public) orphan \(id.uuidString, privacy: .public) deletion: \(String(describing: error.failure), privacy: .public)"
                            )
                        }
                    }
                }
            }
        }
        for id: UUID in orphans { removeAccountDirectory(id) }
    }

    // --- [INTERFACE]
    func setMenuBarExtraVisible(_ visible: Bool) {
        isMenuBarExtraVisible = visible
        guard visible else { return }
        lastPanelOpen = .now
        loginItem = .current
        Task(name: "Panel opened") { [self] in
            await readSelection()
            refresh(accounts, trigger: .background)
        }
        refreshPause?.cancel()
    }

    func refreshLoginItem() {
        loginItem = .current
    }

    func setLoginItemEnabled(_ enabled: Bool) {
        Task(name: "Login item") { [self] in loginItem = await LoginItem.setEnabled(enabled) }
    }

    // --- [ACCOUNT_ACTIONS]
    func cancelOperation(_ id: UUID) {
        model(id)?.running?.task.cancel()
    }

    func setSessionPolicy(_ policy: SessionPolicy, for id: UUID) {
        guard let model: AccountModel = model(id) else { return }
        model.account.sessionPolicy = policy
        model.automaticStartAttempted = false
        scheduleSave()
        if policy == .automatic { refresh([model], trigger: .userAction) }
    }

    func moveAccount(_ id: UUID, by offset: Int) {
        guard let index: Int = accounts.firstIndex(where: { model in model.account.id == id }),
            accounts.indices.contains(index + offset)
        else { return }
        accounts.swapAt(index, index + offset)
        scheduleSave()
    }

    func moveAccounts(from offsets: IndexSet, to destination: Int) {
        accounts.move(fromOffsets: offsets, toOffset: destination)
        scheduleSave()
    }

    // --- [SELECTION]
    private func readSelection() async {
        guard !isStopping else { return }
        let selections: [Provider: Result<AccountIdentity?, ProviderError>] = await withTaskGroup { group in
            for provider: Provider in Provider.allCases {
                group.addTask(name: "\(provider.name) selection") { [client = client(for: provider), known = records] in
                    (provider, await client.currentSelection(known: known))
                }
            }
            var reads: [Provider: Result<AccountIdentity?, ProviderError>] = [:]
            for await (provider, selection): (Provider, Result<AccountIdentity?, ProviderError>) in group {
                reads[provider] = selection
            }
            return reads
        }
        for (provider, selection): (Provider, Result<AccountIdentity?, ProviderError>) in Provider.allCases.compactMap({ provider in
            selections[provider].map { selection in (provider, selection) }
        }) {
            switch selection {
                case .success(.none): apply(selected: nil, provider: provider)
                case .success(.some(let identity)): register(identity, provider: provider)
                case .failure(let error): report(error, provider: provider)
            }
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

    private func register(_ identity: AccountIdentity, provider: Provider) {
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
        scheduleSave()
        refresh([model], trigger: .background)
    }

    // --- [SWITCH]
    func select(_ id: UUID) {
        guard let model: AccountModel = model(id), model.canSelect else { return }
        let account: Account = model.account
        let outgoing: AccountModel? = accounts.first { other in
            other.account.provider == account.provider && other.isSelected && other.account.id != id
        }
        model.run(.selecting) { [self] in
            await outgoing?.running?.task.value
            guard !Task.isCancelled else { return }
            switch await client(for: account.provider).select(account, candidates: records) {
                case .success(let identity):
                    model.account.identity = identity
                    model.issue = nil
                    scheduleSave()
                case .failure(let error): await record(error, for: model)
            }
            await readSelection()
        }
        Task(name: "Refresh after switch") { [self] in
            await model.running?.task.value
            guard !isStopping else { return }
            refresh([model, outgoing].compactMap(\.self), trigger: .userAction)
        }
    }

    // --- [SIGN_IN]
    func addAccount(_ provider: Provider) {
        guard canAddAccount else { return }
        authenticate(id: UUID(), provider: provider, existing: nil)
    }

    func signIn(_ id: UUID) {
        guard let model: AccountModel = model(id), !model.isBusy, authentication == nil, !isStopping
        else { return }
        authenticate(id: id, provider: model.account.provider, existing: model)
    }

    func submitAuthenticationCode(_ code: String) {
        let trimmed: String = code.trimmingCharacters(in: .whitespacesAndNewlines)
        guard let pending: AuthenticationPresentation = authentication, case .pending = pending.phase,
            let codes: AsyncStream<String>.Continuation = authenticationCodes, !trimmed.isEmpty
        else { return }
        authentication = pending.entering(.completing)
        codes.yield(trimmed)
        codes.finish()
    }

    func cancelAuthentication() {
        guard let pending: AuthenticationPresentation = authentication else { return }
        switch pending.phase {
            case .pending, .completing:
                authentication = pending.entering(.cancelling)
                authenticationTask?.cancel()
            case .refused, .cancelling: return
        }
    }

    func dismissAuthentication() {
        guard let pending: AuthenticationPresentation = authentication,
            case .refused = pending.phase
        else { return }
        authentication = nil
    }

    func signOut(_ id: UUID) {
        guard let model: AccountModel = model(id) else { return }
        let account: Account = model.account
        let isSelected: Bool = model.isSelected
        model.run(.signingOut) { [self] in
            switch await client(for: account.provider).signOut(account, isSelected: isSelected) {
                case .success:
                    model.authentication = .signInRequired
                    model.usage = .unavailable
                    model.issue = nil
                    scheduleSave()
                    await readSelection()
                case .failure(let error): await record(error, for: model)
            }
        }
    }

    func remove(_ id: UUID) {
        guard let model: AccountModel = model(id) else { return }
        let account: Account = model.account
        let isSelected: Bool = model.isSelected
        model.run(.removing) { [self] in
            switch await client(for: account.provider).remove(account, isSelected: isSelected) {
                case .success:
                    accounts.removeAll { other in other.account.id == id }
                    removeAccountDirectory(id)
                    scheduleSave()
                    await readSelection()
                case .failure(let error): await record(error, for: model)
            }
        }
    }

    private func authenticate(id: UUID, provider: Provider, existing: AccountModel?) {
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
            let client: any ProviderClient = client(for: provider)
            let result: Result<AccountIdentity, ProviderError> =
                if let existing {
                    await client.reconnect(account: existing.account, isSelected: existing.isSelected, codes: codes.stream)
                } else {
                    await client.connect(id: id, codes: codes.stream)
                }
            codes.continuation.finish()
            authenticationCodes = nil
            switch result {
                case .success(let identity) where !Task.isCancelled:
                    await connect(id: id, provider: provider, identity: identity, existing: existing)
                case .success:
                    if existing == nil { await deletePrivateStore(id, provider: provider) }
                    authentication = nil
                case .failure(let error) where error.failure.isCancellation || Task.isCancelled:
                    if existing == nil { await deletePrivateStore(id, provider: provider) }
                    authentication = nil
                case .failure(let error):
                    let refusal: AuthenticationRefusal =
                        if let existing, error.failure.isAccountMismatch {
                            .differentAccount(email: existing.account.identity.email)
                        } else {
                            .failure(error)
                        }
                    authentication = AuthenticationPresentation(
                        id: id,
                        provider: provider,
                        phase: .refused(refusal),
                        startedAt: Date(),
                    )
                    if let existing { await record(error, for: existing) }
            }
            authenticationTask = nil
        }
        authenticationTask = task
        existing?.run(.signingIn) { await task.value }
    }

    private func connect(
        id: UUID,
        provider: Provider,
        identity: AccountIdentity,
        existing: AccountModel?,
    ) async {
        let duplicate: AccountModel? = accounts.first { model in
            model.account.id != id && model.account.provider == provider
                && model.account.identity.isSameAccount(as: identity)
        }
        let replaced: AccountModel?
        switch (duplicate, existing) {
            case (.some(let connected), _) where connected.isConnected || connected.isBusy:
                authentication = AuthenticationPresentation(
                    id: id,
                    provider: provider,
                    phase: .refused(.alreadyConnected(email: identity.email)),
                    startedAt: Date(),
                )
                connected.issue = nil
                if existing == nil { await deletePrivateStore(id, provider: provider) }
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
            await deletePrivateStore(replaced.account.id, provider: provider)
        } else if existing == nil {
            accounts.append(model)
        }
        model.account.identity = identity
        model.authentication = .connected
        model.retryAfter = nil
        model.issue = nil
        authentication = nil
        await save()
        await readSelection()
        switch existing {
            case .some: await readUsage(model)
            case .none: model.run(.refreshing) { [self] in await readUsage(model) }
        }
    }

    private func deletePrivateStore(_ id: UUID, provider: Provider) async {
        let result: Result<Void, ProviderError> = await client(for: provider).deletePrivateStore(id: id)
        removeAccountDirectory(id)
        switch result {
            case .success: providerIssues[provider] = nil
            case .failure(let error): report(error, provider: provider)
        }
    }

    // --- [USAGE]
    func startSession(_ id: UUID) {
        guard let model: AccountModel = model(id), model.isConnected else { return }
        model.run(.starting) { [self] in
            guard model.isConnected else { return }
            await apply(await client(for: model.account.provider).startSession(for: model.account, isSelected: model.isSelected), to: model)
        }
    }

    private func apply(_ result: Result<AccountUsage, ProviderError>, to model: AccountModel) async {
        switch result {
            case .success(let usage):
                let now: Date = Date()
                let carried: AccountUsage = usage.keepingResets(from: model.usage.usage, at: now)
                model.usage = .current(carried)
                model.authentication = .connected
                model.retryAfter = nil
                model.issue = nil
                if case .running = carried.availability(at: now) { model.automaticStartAttempted = false }
                scheduleSave()
                refreshPause?.cancel()
                startAutomaticSessionIfNeeded(model)
            case .failure(let error) where error.failure.isCancellation: return
            case .failure(let error):
                model.usage = model.usage.usage.map(UsageState.stale) ?? .unavailable
                await record(error, for: model)
        }
    }

    private func refresh(_ candidates: [AccountModel], trigger: RefreshTrigger) {
        guard isStorageAvailable, !isStopping, !isSwitching else { return }
        let now: Date = Date()
        for model: AccountModel in candidates where !model.isBusy {
            let due: Bool = model.isUsageReadDue(for: trigger, at: now)
            switch (model.isConnected, model.isSelected) {
                case (true, _) where due, (false, true) where due && trigger == .revalidation:
                    model.run(.refreshing) { [self] in await readUsage(model) }
                case (false, false):
                    model.run(.refreshing) { [self] in
                        if await recheckSignIn(model), due { await readUsage(model) }
                    }
                case (true, _), (false, true): continue
            }
        }
    }

    private func recheckSignIn(_ model: AccountModel) async -> Bool {
        switch await client(for: model.account.provider).holdsCredential(for: model.account) {
            case .success(true):
                model.authentication = .connected
                model.issue = nil
                scheduleSave()
                return true
            case .success(false): return false
            case .failure(let error):
                await record(error, for: model)
                return false
        }
    }

    private func readUsage(_ model: AccountModel) async {
        await apply(await client(for: model.account.provider).usage(for: model.account, isSelected: model.isSelected), to: model)
    }

    private func startAutomaticSessionIfNeeded(_ model: AccountModel) {
        guard !isStopping, model.account.sessionPolicy == .automatic, !model.automaticStartAttempted,
            model.canStartSession(at: Date())
        else { return }
        model.automaticStartAttempted = true
        startSession(model.account.id)
    }

    // --- [TRIGGERS]
    private func observeNetwork() async {
        var previous: NWPath.Status?
        for await path: NWPath in NWPathMonitor() {
            defer { previous = path.status }
            guard path.status == .satisfied, previous != .satisfied, !isStopping else { continue }
            await readSelection()
            refresh(accounts, trigger: .background)
        }
    }

    private func observeWake() async {
        for await _ in NSWorkspace.shared.notificationCenter.notifications(
            named: NSWorkspace.didWakeNotification
        ) {
            await readSelection()
        }
    }

    private func observe(file: URL) async {
        do {
            for try await _ in FileWatch.values(
                of: file,
                probe: FileWatch.contentDigest,
                debounce: .milliseconds(300),
            ) where !isSwitching {
                let before: [UUID: Bool] = Dictionary(
                    uniqueKeysWithValues: accounts.map { model in (model.account.id, model.isSelected) }
                )
                await readSelection()
                codexPrecondition = (await codex.preconditions()).failure
                let changed: [AccountModel] = accounts.filter { model in
                    before[model.account.id] != model.isSelected
                }
                refresh(changed, trigger: .background)
            }
        } catch {
            logger.error(
                "Watch on \(file.path, privacy: .private): \(String(describing: error), privacy: .public)"
            )
        }
    }

    private func observeRefreshLock(_ lock: URL) async {
        do {
            for try await present: Bool in FileWatch.values(of: lock, probe: FileWatch.exists, debounce: nil)
            where !isSwitching && !present {
                await readSelection()
            }
        } catch {
            logger.error(
                "Watch on \(lock.path, privacy: .private): \(String(describing: error), privacy: .public)"
            )
        }
    }

    private func observeCodexUpdates() async {
        for await update: CodexRateLimitsUpdate in codex.updates {
            guard
                let model: AccountModel = accounts.first(where: { model in
                    model.account.provider == .openAI
                        && codex.home(for: model.account, isSelected: model.isSelected) == update.home
                })
            else { continue }
            let previous: AccountUsage? = model.usage.usage
            await apply(
                .success(
                    AccountUsage(
                        windows: update.windows,
                        includedUsageAllowed: previous?.includedUsageAllowed,
                        observedAt: update.observedAt,
                        signInExpiresAt: nil,
                    )
                ),
                to: model,
            )
        }
    }

    private func schedule() async {
        while !Task.isCancelled, !isStopping {
            let delay: Duration = nextRefreshDelay(at: Date(), sincePanelOpen: lastPanelOpen?.duration(to: .now))
            let pause: Task<Void, any Error> = Task(name: "Refresh pause") {
                try await Task.sleep(for: delay)
            }
            refreshPause = pause
            let slept: Result<Void, any Error> = await pause.result
            guard !Task.isCancelled, !isStopping else { return }
            if case .success = slept { refresh(accounts, trigger: .background) }
        }
    }

    private func nextRefreshDelay(at now: Date, sincePanelOpen recency: Duration?) -> Duration {
        let base: Duration =
            if isMenuBarExtraVisible {
                .seconds(180)
            } else {
                switch recency {
                    case .some(..<Duration.seconds(15 * 60)): .seconds(5 * 60)
                    case .some(..<Duration.seconds(60 * 60)): .seconds(15 * 60)
                    case .some, .none: .seconds(30 * 60)
                }
            }
        let boundary: Duration? = accounts.flatMap { model in
            [model.usage.usage?.nextReset, model.retryAfter].compactMap(\.self)
        }
        .filter { date in date > now }
        .min()
        .map { date in .seconds(date.timeIntervalSince(now) + 1) }
        return min(base, boundary ?? base)
    }

    // --- [FAILURES]
    private func record(_ error: ProviderError, for model: AccountModel) async {
        guard !error.failure.isCancellation else { return }
        logger.error(
            "\(model.account.id.uuidString, privacy: .public) \(model.account.identity.email, privacy: .private): \(String(describing: error), privacy: .public)"
        )
        model.issue = error
        if let retryAfter: Date = error.failure.retryAfter {
            model.retryAfter = max(model.retryAfter ?? retryAfter, retryAfter)
            scheduleSave()
            refreshPause?.cancel()
        }
        guard error.failure.requiresSignIn else { return }
        model.authentication = .signInRequired
        scheduleSave()
        await readSelection()
    }

    private func report(_ error: ProviderError, provider: Provider) {
        guard !error.failure.isCancellation else { return }
        providerIssues[provider] = error
        logger.error("\(provider.name, privacy: .public): \(String(describing: error), privacy: .public)")
    }

    // --- [PERSISTENCE]
    private func scheduleSave() {
        let previous: Task<Void, Never>? = saveTask
        saveTask = Task(name: "Save accounts") { [self] in
            await previous?.value
            await save()
        }
    }

    private func save() async {
        guard isStorageAvailable else { return }
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
