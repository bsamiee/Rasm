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
    case active(Provider)
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
    private struct Records: Codable {
        let records: [AccountModel]
    }

    // --- [STATE]

    private(set) var accounts: [AccountModel] = []
    private(set) var authentication: AuthenticationPresentation?
    private(set) var loginItem: Result<SMAppService.Status, any Error> = .success(SMAppService.mainApp.status)
    private(set) var storageState: StorageState = .loading
    private var isStopping: Bool = false
    private var reservedStores: Set<CredentialStore> = []
    @ObservationIgnored private var isMenuBarExtraVisible: Bool = false
    @ObservationIgnored private var lastMenuBarExtraOpen: ContinuousClock.Instant?
    @ObservationIgnored private var failures: [UUID: (operation: AccountOperation, cause: String, repeats: Int)] = [:]
    @ObservationIgnored private var root: Task<Void, Never>?
    @ObservationIgnored private var authenticationTask: Task<Void, Never>?
    @ObservationIgnored private var refreshWake: AsyncStream<Void>.Continuation?
    @ObservationIgnored private var written: Task<Void, Never>?

    private let locations: FileLocations
    private let claude: ClaudeClient
    private let codex: CodexClient
    private let logger = Logging.Logger(label: String(reflecting: AccountStore.self))
    private let signposter = OSSignposter(subsystem: Program.current.identifier, category: String(reflecting: AccountStore.self))
    private let saves: (stream: AsyncStream<Void>, continuation: AsyncStream<Void>.Continuation) = AsyncStream.makeStream(bufferingPolicy: .bufferingNewest(1))
    private let coding: (decoder: JSONDecoder, encoder: JSONEncoder) = {
        let (decoder, encoder) = (JSONDecoder(), JSONEncoder())
        decoder.dateDecodingStrategy = .iso8601
        encoder.dateEncodingStrategy = .iso8601
        encoder.outputFormatting = [.prettyPrinted, .sortedKeys]
        return (decoder, encoder)
    }()

    init(environment: [Subprocess.Environment.Key: String]) {
        locations = FileLocations(environment: environment)
        claude = ClaudeClient(locations: locations, environment: environment)
        codex = CodexClient(locations: locations, environment: environment)
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

    private func withStores(_ requested: @autoclosure () -> Set<CredentialStore>, owning: Set<CredentialStore> = [], _ work: (Set<CredentialStore>) async -> Void) async {
        for await _ in Observations({ self.reservedStores }) {
            let needed = requested().subtracting(owning)
            guard needed.isDisjoint(with: reservedStores) else { continue }
            guard !Task.isCancelled else { return }
            reservedStores.formUnion(needed)
            defer { reservedStores.subtract(needed) }
            await work(owning.union(needed))
            return
        }
    }

    private func stores(of model: AccountModel) -> Set<CredentialStore> {
        Set([.privateAccount(model.account.id)] + (model.isActive ? [.active(model.account.provider)] : []))
    }

    private func perform(_ operation: AccountOperation, on model: AccountModel, _ work: @escaping @MainActor () async -> Void) {
        guard !isStopping else { return }
        model.run(operation) { [self] in
            let account = model.account
            let interval = signposter.beginInterval(operation.name, id: signposter.makeSignpostID(), "\(account.id, privacy: .public)")
            logger.log(level: operation.level, "Account operation started", metadata: account.metadata(["operation": "\(operation.name, privacy: .public)"]))
            await withSpan(operation.name.description) { span in
                span.attributes["account.id"] = account.id.uuidString
                await work()
            }
            signposter.endInterval(operation.name, interval)
        }
    }

    private func run(_ model: AccountModel, _ operation: AccountOperation, _ work: @escaping (Set<CredentialStore>) async -> Void) {
        perform(operation, on: model) { [self] in await withStores(stores(of: model), work) }
    }

    // --- [LIFECYCLE]

    func start() {
        guard root == nil else { return }
        root = Task(name: "Accounts") { [self] in
            await withDiscardingTaskGroup { group in
                group.addTask(name: "Login item") { await observeActivation() }
                load()
                guard storageState == .available, !isStopping else { return }
                await reconcile()
                await readActiveAccounts()
                group.addTask(name: "Save") { for await _ in self.saves.stream { await self.save() } }
                group.addTask(name: "Network") { await observeNetwork() }
                group.addTask(name: "Wake") { await observeWake() }
                group.addTask(name: "Claude active store") { await observe(file: self.claude.active.configFile, provider: .claude) }
                group.addTask(name: "Claude credential writes") { await observeCredentialWrites() }
                group.addTask(name: "Codex active store") { await observe(file: self.codex.activeAuthFile, provider: .openAI) }
                group.addTask(name: "Codex configuration") { await observe(file: self.codex.activeHome.appending(path: "config.toml"), provider: .openAI) }
                group.addTask(name: "Refresh schedule") { await schedule() }
            }
        }
        func readCodexPrecondition() async {
            guard accounts.contains(where: { model in model.account.provider == .openAI }) else { return }
            await withStores([.active(.openAI)]) { _ in
                do throws(AccountFailure) {
                    try await codex.preconditions()
                    logger.debug("Codex switch preconditions hold")
                } catch {
                    logger.error("Codex switch check failed", error: error)
                }
            }
        }
        func reconcile() async {
            await withStores([.active(.claude)]) { active in
                do throws(AccountFailure) {
                    guard let pending = try claude.pendingSwitch() else { return }
                    logger.notice("Completing pending Claude switch", metadata: ["copies": "\(pending.copies, privacy: .public)"])
                    await withStores(Set(pending.copies.map(CredentialStore.privateAccount)), owning: active) { _ in
                        do throws(AccountFailure) {
                            try await claude.completePendingSwitch(pending)
                            logger.notice("Completed Claude switch", metadata: Provider.claude.metadata(pending.incoming))
                        } catch {
                            logger.error("Claude switch failed", error: error, metadata: Provider.claude.metadata(pending.incoming))
                        }
                    }
                } catch {
                    logger.error("Pending Claude switch read failed", error: error)
                }
            }
            guard !accounts.isEmpty else { return }
            await readCodexPrecondition()
            let known = Set(accounts.map(\.account.id) + [authentication?.id].compactMap(\.self))
            let entries: [URL]
            do {
                entries = try ifPresent { try FileManager.default.contentsOfDirectory(at: locations.accountsDirectory, includingPropertiesForKeys: []) } ?? []
            } catch {
                logger.error("Accounts directory listing failed", error: error)
                return
            }
            let orphans = entries.compactMap { entry in UUID(uuidString: entry.lastPathComponent) }.filter { id in !known.contains(id) }
            guard !orphans.isEmpty else { return }
            logger.notice("Deleting orphan account directories", metadata: ["accounts": "\(orphans, privacy: .public)"])
            await withDiscardingTaskGroup { group in
                for (id, provider) in orphans.flatMap({ id in Provider.allCases.map { provider in (id, provider) } }) {
                    group.addTask(name: "\(provider.name) orphan deletion") { [client = client(for: provider), logger] in
                        do throws(AccountFailure) {
                            try await client.deletePrivateStore(id: id)
                        } catch {
                            logger.error("Orphan private store delete failed", error: error, metadata: provider.metadata(id))
                        }
                    }
                }
            }
            for id in orphans { removeAccountDirectory(id) }
        }
        func observe(file: URL, provider: Provider) async {
            let fields: Logging.Logger.Metadata = ["provider": "\(provider.name, privacy: .public)", "file": "\(file.lastPathComponent, privacy: .public)"]
            do {
                for try await _ in FileEvent.events(of: file).map({ batch in
                    _ = try batch.get()
                    return try ifPresent { try Data(contentsOf: file) }
                }).removeDuplicates().dropFirst().debounce(for: .milliseconds(300)) {
                    logger.debug("Active store changed", metadata: fields)
                    let before = Dictionary(uniqueKeysWithValues: accounts.map { model in (model.account.id, model.isActive) })
                    await readActiveAccount(provider)
                    if provider == .openAI { await readCodexPrecondition() }
                    refresh(accounts.filter { model in before[model.account.id] != model.isActive })
                }
            } catch {
                logger.error("Active store watch failed", error: error, metadata: fields)
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
                await readActiveAccounts()
                refresh(accounts)
            }
        }
        func observeWake() async {
            for await _ in NSWorkspace.shared.notificationCenter.notifications(named: NSWorkspace.didWakeNotification) {
                logger.debug("Woke from sleep")
                await readActiveAccounts()
            }
        }
        func observeCredentialWrites() async {
            do {
                let lock = ClaudeCredentialStore.storageWriteLockfile(in: claude.active.directory)
                for try await present in FileEvent.presence(of: lock).map({ present in try present.get() }).removeDuplicates().dropFirst() where !present {
                    logger.debug("Claude storage write lock released")
                    await readActiveAccount(.claude)
                }
            } catch {
                logger.error("Claude storage write lock watch failed", error: error)
            }
        }
        func schedule() async {
            while !Task.isCancelled {
                let now = Date()
                let base: Duration =
                    switch (isMenuBarExtraVisible, lastMenuBarExtraOpen?.duration(to: .now)) {
                        case (true, _): AccountModel.usageReadSpacing.active
                        case (false, .some(..<Duration.seconds(15 * 60))): .seconds(5 * 60)
                        case (false, .some(..<Duration.seconds(60 * 60))): .seconds(15 * 60)
                        case (false, _): AccountModel.usageReadSpacing.inactive
                    }
                let boundary = accounts.compactMap(\.usage.usage?.nextReset).filter { date in date > now }.min().map { date in Duration.seconds(date.timeIntervalSince(now) + 1) }
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

    // --- [INTERFACE]

    func setMenuBarExtraVisible(_ visible: Bool) {
        isMenuBarExtraVisible = visible
        guard visible else { return }
        lastMenuBarExtraOpen = .now
        logger.debug("Menu bar extra opened")
        Task(name: "Menu bar extra opened") { [self] in
            await readActiveAccounts()
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

    func moveAccounts(from offsets: IndexSet, to destination: Int) {
        accounts.move(fromOffsets: offsets, toOffset: destination)
        logger.info("Moved accounts", metadata: ["offsets": "\(Array(offsets), privacy: .public)", "destination": .stringConvertible(destination)])
        saves.continuation.yield()
    }

    // --- [ACTIVE_ACCOUNT]

    private func readActiveAccounts() async {
        guard storageState == .available, !isStopping else { return }
        await withDiscardingTaskGroup { group in
            for provider in Provider.allCases {
                group.addTask(name: "\(provider.name) active account") { await self.readActiveAccount(provider) }
            }
        }
    }

    private func readActiveAccount(_ provider: Provider, owning: Set<CredentialStore> = []) async {
        await withStores([.active(provider)], owning: owning) { active in
            let activeAccount = accounts.first { model in model.account.provider == provider && model.isActive }?.account
            await withStores(Set([activeAccount.map { account in CredentialStore.privateAccount(account.id) }].compactMap(\.self)), owning: active) { stores in
                do throws(AccountFailure) {
                    let identity = try await client(for: provider).activeIdentity(active: activeAccount)
                    let matching = accounts.filter { model in
                        model.account.provider == provider && (model.isActive || identity.map(model.account.identity.isSameAccount(as:)) == true)
                    }
                    await withStores(Set(matching.map { model in .privateAccount(model.account.id) }), owning: stores) { _ in
                        if let identity { register(identity) } else { apply(active: nil) }
                    }
                } catch let error where error.isCancellation {
                } catch {
                    logger.error("Active account read failed", error: error, metadata: ["provider": "\(provider.name, privacy: .public)"])
                }
            }
        }
        func register(_ identity: AccountIdentity) {
            if let existing = accounts.first(where: { model in model.account.provider == provider && model.account.identity.isSameAccount(as: identity) }) {
                apply(active: existing.account.id)
                return
            }
            let model = AccountModel(account: Account(id: UUID(), provider: provider, identity: identity), authentication: .connected, usage: .unavailable)
            accounts.append(model)
            logger.notice("Registered account from active store", metadata: model.account.metadata())
            apply(active: model.account.id)
            saves.continuation.yield()
            refresh([model])
        }
        func apply(active id: UUID?) {
            let previous = accounts.first { model in model.account.provider == provider && model.isActive }
            switch accounts.first(where: { model in model.account.provider == provider && model.account.id == id }) {
                case .some(let active) where active !== previous: logger.notice("Active account moved", metadata: active.account.metadata())
                case .none where previous != nil: logger.notice("Active account cleared, active store holds no connected account", metadata: ["provider": "\(provider.name, privacy: .public)"])
                case .some, .none: logger.debug("Active account unchanged", metadata: ["provider": "\(provider.name, privacy: .public)"])
            }
            for model in accounts where model.account.provider == provider {
                model.isActive = model.account.id == id
                if model.isActive, !model.isConnected { refresh([model]) }
            }
        }
    }

    // --- [SWITCHES]

    func activate(_ id: UUID) {
        guard let model = model(id), model.canActivate else { return }
        let account = model.account
        perform(.activate, on: model) { [self] in
            await withStores([.active(account.provider)]) { active in
                await readActiveAccount(account.provider, owning: active)
                guard !model.isActive else {
                    logger.notice("Account already active in its store", metadata: account.metadata())
                    return
                }
                let outgoing = accounts.first { other in other.account.provider == account.provider && other.isActive && other.account.id != id }
                await withStores(Set([id, outgoing?.account.id].compactMap(\.self).map(CredentialStore.privateAccount)), owning: active) { stores in
                    do throws(AccountFailure) {
                        model.account.identity = try await client(for: account.provider).activate(account, outgoing: outgoing?.account)
                        saves.continuation.yield()
                        logger.notice("Switched account", metadata: model.account.metadata(["outgoing": "\([outgoing?.account.id].compactMap(\.self), privacy: .public)"]))
                    } catch {
                        await record(error, of: .activate, for: model, owning: stores)
                    }
                    await withTaskCancellationShield { await readActiveAccount(account.provider, owning: stores) }
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
        guard let model = model(id), !model.isBusy, authentication == nil, !isStopping else { return }
        authenticate(id, provider: model.account.provider)
    }

    func submitAuthenticationCode(_ code: String) {
        let trimmed = code.trimmingCharacters(in: .whitespacesAndNewlines)
        guard let pending = authentication, pending.phase.is(\.pending), !trimmed.isEmpty else { return }
        logger.notice("Submitted sign-in code", metadata: pending.provider.metadata(pending.id))
        authentication?.phase = .completing
        authentication?.codes.yield(trimmed)
        authentication?.codes.finish()
    }

    func cancelAuthentication() {
        guard let pending = authentication else { return }
        switch pending.phase {
            case .pending, .completing:
                logger.notice("Sign-in cancel requested", metadata: pending.provider.metadata(pending.id))
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
        run(model, .signOut) { [self] stores in
            do throws(AccountFailure) {
                try await client(for: account.provider).signOut(account, isActive: model.isActive)
                model.authentication = .signInRequired
                model.usage = .unavailable
                saves.continuation.yield()
                logger.notice("Signed out account", metadata: account.metadata())
            } catch {
                await record(error, of: .signOut, for: model, owning: stores)
            }
            if stores.contains(.active(account.provider)) { await withTaskCancellationShield { await readActiveAccount(account.provider, owning: stores) } }
        }
    }

    func remove(_ id: UUID) {
        guard let model = model(id) else { return }
        let account = model.account
        run(model, .remove) { [self, client = client(for: account.provider)] stores in
            do throws(AccountFailure) {
                do throws(AccountFailure) { try await client.signOut(account, isActive: model.isActive) } catch let error where error.requiresSignIn {}
                try await client.deletePrivateStore(id: id)
                accounts.removeAll { other in other.account.id == id }
                removeAccountDirectory(id)
                saves.continuation.yield()
                failures[account.id] = nil
                logger.notice("Removed account", metadata: account.metadata())
            } catch {
                await record(error, of: .remove, for: model, owning: stores)
            }
            if stores.contains(.active(account.provider)) { await withTaskCancellationShield { await readActiveAccount(account.provider, owning: stores) } }
        }
    }

    private func authenticate(_ id: UUID, provider: Provider) {
        let existing = model(id)
        let codes = AsyncStream<String>.makeStream()
        let fields = provider.metadata(id)
        authentication = AuthenticationPresentation(id: id, provider: provider, phase: .pending, startedAt: Date(), codes: codes.continuation)
        let task = Task(name: "Sign in \(provider.name)") { [self] in
            defer { codes.continuation.finish() }
            let interval = signposter.beginInterval(AccountOperation.signIn.name, id: signposter.makeSignpostID(), "\(id, privacy: .public)")
            defer { signposter.endInterval(AccountOperation.signIn.name, interval) }
            logger.notice("Sign-in started", metadata: provider.metadata(id, ["new": .stringConvertible(existing == nil)]))
            await withStores(existing.map(stores(of:)) ?? [.privateAccount(id)]) { stores in
                let target: SignInTarget = existing.map { model in .account(model.account, isActive: model.isActive) } ?? .newAccount(id)
                switch await Result(catching: { () async throws(AccountFailure) in try await client(for: provider).signIn(target, codes: codes.stream) }) {
                    case .success(let identity) where !Task.isCancelled: await connect(identity, owning: stores)
                    case .failure(let error) where !error.isCancellation && !Task.isCancelled:
                        if let existing {
                            await record(error, of: .signIn, for: existing, owning: stores)
                        } else {
                            logger.error("Sign-in failed for new account", error: error, metadata: fields)
                        }
                        authentication?.phase = .refused(existing.flatMap { model in error.isAccountMismatch ? .differentAccount(email: model.account.identity.email) : nil } ?? .failure(error))
                    case .success, .failure:
                        logger.notice("Sign-in canceled", metadata: fields)
                        if existing == nil { await withTaskCancellationShield { await deletePrivateStore(id, provider: provider, owning: stores) } }
                        authentication = nil
                }
                if stores.contains(.active(provider)) { await withTaskCancellationShield { await readActiveAccount(provider, owning: stores) } }
            }
            if Task.isCancelled, authentication?.id == id { authentication = nil }
            await readActiveAccount(provider)
        }
        authenticationTask = task
        existing?.run(.signIn) { await task.value }
        func connect(_ identity: AccountIdentity, owning stores: Set<CredentialStore>) async {
            let matchingAccount: () -> AccountModel? = { [self] in
                accounts.first { model in model.account.id != id && model.account.provider == provider && model.account.identity.isSameAccount(as: identity) }
            }
            await withStores(matchingAccount().map(stores(of:)) ?? [], owning: stores) { stores in
                let duplicate = matchingAccount()
                if let duplicate, duplicate.isConnected || duplicate.isBusy {
                    logger.notice(
                        "Sign-in refused, another account holds its identity",
                        metadata: provider.metadata(id, ["holder": "\(duplicate.account.id, privacy: .public)", "email": "\(identity.email, privacy: .private)"]),
                    )
                    if existing == nil { await deletePrivateStore(id, provider: provider, owning: stores) }
                    authentication?.phase = .refused(.alreadyConnected(email: identity.email))
                    return
                }
                let replaced = existing == nil ? duplicate : nil
                let model = existing ?? AccountModel(account: Account(id: id, provider: provider, identity: identity), authentication: .connected, usage: replaced?.usage ?? .unavailable)
                if let replaced, let index = accounts.firstIndex(where: { stored in stored === replaced }) {
                    accounts[index] = model
                    logger.notice("Account replaces signed-out account with same identity", metadata: provider.metadata(id, ["replaced": "\(replaced.account.id, privacy: .public)"]))
                    await deletePrivateStore(replaced.account.id, provider: provider, owning: stores)
                } else if existing == nil {
                    accounts.append(model)
                }
                model.account.identity = identity
                model.authentication = .connected
                logger.notice("Signed in account", metadata: model.account.metadata())
                await save()
                if stores.contains(.active(provider)) { await readActiveAccount(provider, owning: stores) }
                authentication = nil
                switch existing {
                    case .some: await readUsage(model, for: .signIn, owning: stores)
                    case .none: run(model, .refresh) { [self] stores in await readUsage(model, for: .refresh, owning: stores) }
                }
            }
        }
    }

    private func deletePrivateStore(_ id: UUID, provider: Provider, owning: Set<CredentialStore>) async {
        await withStores([.privateAccount(id)], owning: owning) { _ in
            do throws(AccountFailure) {
                try await client(for: provider).deletePrivateStore(id: id)
                logger.info("Deleted private store", metadata: provider.metadata(id))
            } catch {
                logger.error("Private store delete failed", error: error, metadata: provider.metadata(id))
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
            run(model, .refresh) { [self] stores in
                let held =
                    await
                    (model.isConnected
                    ? nil
                    : Result { () async throws(AccountFailure) in
                        try await client(for: model.account.provider).holdsCredential(for: model.account, isActive: model.isActive)
                    })
                switch held {
                    case .some(.success(true)):
                        logger.notice("Signed-out account holds credential, marked connected", metadata: model.account.metadata())
                        model.authentication = .connected
                        saves.continuation.yield()
                    case .some(.success(false)):
                        logger.debug("Signed-out account holds no credential", metadata: model.account.metadata())
                        return
                    case .some(.failure(let error)):
                        await record(error, of: .refresh, for: model, owning: stores)
                        return
                    case .none: break
                }
                if due { await readUsage(model, for: .refresh, owning: stores) }
            }
        }
    }

    private func readUsage(_ model: AccountModel, for operation: AccountOperation, owning: Set<CredentialStore>) async {
        do throws(AccountFailure) {
            model.usage = .current(try await client(for: model.account.provider).usage(for: model.account, isActive: model.isActive))
            model.authentication = .connected
            if let cleared = failures.removeValue(forKey: model.account.id) {
                logger.notice("Account failure cleared", metadata: model.account.metadata(["repeats": .stringConvertible(cleared.repeats)]))
            }
            logger.log(level: operation.level, "Account operation succeeded", metadata: model.account.metadata(["operation": "\(operation.name, privacy: .public)"]))
            saves.continuation.yield()
            refreshWake?.yield()
        } catch let error where error.isCancellation {
        } catch {
            model.usage = model.usage.usage.map(UsageState.stale) ?? .unavailable
            await record(error, of: operation, for: model, owning: owning)
        }
    }

    // --- [FAILURES]

    private func record(_ error: AccountFailure, of operation: AccountOperation, for model: AccountModel, owning: Set<CredentialStore>) async {
        guard !error.isCancellation else { return }
        let cause = String(describing: error)
        let last = failures[model.account.id]
        let repeats = last.map { last in last.operation == operation && last.cause == cause ? last.repeats + 1 : 0 } ?? 0
        failures[model.account.id] = (operation, cause, repeats)
        if repeats == 0 {
            logger.error(
                "Account operation failed",
                error: error,
                metadata: model.account.metadata(["operation": "\(operation.name, privacy: .public)", "requiresSignIn": .stringConvertible(error.requiresSignIn)]),
            )
        }
        guard error.requiresSignIn else { return }
        model.authentication = .signInRequired
        model.usage = model.usage.usage.map(UsageState.stale) ?? .unavailable
        saves.continuation.yield()
        if owning.contains(.active(model.account.provider)) { await readActiveAccount(model.account.provider, owning: owning) }
    }

    // --- [PERSISTENCE]

    private func load() {
        let file = locations.accountsFile
        do throws(AccountFailure) {
            accounts = try file.contents().map { data throws(AccountFailure) in try coding.decoder.decode(Records.self, from: data, subject: file.lastPathComponent).records } ?? []
            storageState = .available
            logger.notice("Loaded accounts", metadata: ["file": "\(file.lastPathComponent, privacy: .public)", "count": .stringConvertible(accounts.count)])
        } catch {
            storageState = .unavailable
            logger.error("Accounts load failed", error: error, metadata: ["file": "\(file.lastPathComponent, privacy: .public)"])
        }
    }

    private func save() async {
        guard storageState == .available else { return }
        let file = locations.accountsFile
        let previous = written
        let write = Task { [self] in
            await previous?.value
            let fields: Logging.Logger.Metadata = ["file": "\(file.lastPathComponent, privacy: .public)", "count": .stringConvertible(accounts.count)]
            do throws(AccountFailure) {
                try await coding.encoder.encode(Records(records: accounts), subject: file.lastPathComponent).write(to: file)
                logger.debug("Saved accounts", metadata: fields)
            } catch {
                logger.error("Accounts save failed", error: error, metadata: fields)
            }
        }
        written = write
        await write.value
    }

    private func removeAccountDirectory(_ id: UUID) {
        do throws(AccountFailure) {
            try locations.accountDirectory(id).remove()
        } catch {
            logger.error("Account directory remove failed", error: error, metadata: ["account": "\(id, privacy: .public)"])
        }
    }
}
