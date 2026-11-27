package import AppKit
@unsafe @preconcurrency import ApplicationServices
import AsyncAlgorithms
import CasePaths
import CasePathsCore
package import OrderedCollections
package import Processes
import Synchronization
import System

// --- [SERVICES] ------------------------------------------------------------------------

package actor Watches {
    private enum Change: Sendable {
        case listed(Result<OrderedDictionary<Instance, InstanceState>, ApplicationFailure>)
        case watched(instance: Instance, state: InstanceState)
        case released(Instance)
    }

    private var held: [ProcessInstance: (watch: Watch, driven: Bool)] = [:]
    private let followers: Mutex<[UUID: AsyncStream<(watch: Watch, driven: Bool)>.Continuation]> = Mutex([:])
    private let windowServer: Result<WindowServerConnection, ApplicationFailure>

    package init() {
        let timed: AXError = AXUIElementSetMessagingTimeout(AXUIElementCreateSystemWide(), Float(messagingTimeout / .seconds(1)))
        windowServer = Result { () throws(ApplicationFailure) in
            guard timed == .success else { throw ApplicationFailure(subject: "Messaging timeout of the system-wide element", reason: .accessibility(timed)) }
            return try WindowServerConnection()
        }
    }

    nonisolated func notices(of notifications: Set<WindowNotification>, from owner: pid_t? = nil) -> AsyncStream<Result<WindowServerConnection.Notice, ApplicationFailure>> {
        switch windowServer {
            case .success(let connection): connection.notices(of: notifications, from: owner)
            case .failure(let failure):
                AsyncStream { continuation in
                    continuation.yield(.failure(failure))
                    continuation.finish()
                }
        }
    }

    package func instance(_ process: ProcessInstance, matching: @Sendable () -> NSPredicate) async throws(ApplicationFailure) -> Instance {
        let listed = { () throws(ApplicationFailure) -> Instance? in
            try NSWorkspace.shared.instances { (application: NSRunningApplication) -> Bool in application.processIdentifier == process.pid }
                .first { (listed: Instance) -> Bool in listed.process == process }
        }
        var found: Instance? = held[process]?.watch.instance
        if found == nil { found = try listed() }
        if found == nil, !process.hasExited {
            let (listings, listing) = AsyncStream<Void>.makeStream(bufferingPolicy: .bufferingNewest(1))
            let observation: NSKeyValueObservation = NSWorkspace.shared.observe(\.runningApplications, options: [.initial]) { _, _ in listing.yield() }
            defer { observation.invalidate() }
            for await _ in merge(listings, process.exits().map { (_: Result<ProcessExit, Errno>) -> Void in () }) {
                found = try listed()
                if found != nil || process.hasExited { break }
            }
        }
        guard let found else { throw ApplicationFailure(subject: "The instance", reason: process.hasExited ? .absent : .cancelled) }
        let application: NSRunningApplication = found.application
        guard matching().evaluate(bundleIdentifier: application.bundleIdentifier, displayName: application.localizedName, bundle: application.bundleURL) else {
            throw ApplicationFailure(subject: application.name, reason: .outsideClaim)
        }
        return found
    }

    package func watch(_ instance: Instance, driven: Bool = true, locators: [Locator] = []) async -> Watch {
        let before: (watch: Watch, driven: Bool)? = held[instance.process]
        let after: (watch: Watch, driven: Bool) = (before?.watch ?? Watch(instance, locators: locators), driven || before?.driven == true)
        held[instance.process] = after
        let started: Watch = after.watch
        if before == nil {
            let windowed: Set<WindowNotification> = Set(WindowNotification.allCases).subtracting([.secureInputEnabled, .secureInputDisabled])
            await started.start(windows: notices(of: windowed, from: instance.process.pid)) { await self.remove(instance.process, started) }
        }
        if after.driven, before?.driven != true { await after.watch.drive() }
        if before?.driven != after.driven {
            followers.withLock { following in
                for follower in following.values { follower.yield(after) }
            }
        }
        return after.watch
    }

    package func finish() async {
        let watching: [Watch] = held.values.map(\.watch)
        await withDiscardingTaskGroup { group in
            for watch in watching { group.addTask { await watch.finish() } }
        }
    }

    package func release(_ process: ProcessInstance) async {
        guard let released = held.removeValue(forKey: process) else { return }
        await released.watch.finish()
    }

    package func locators(of process: ProcessInstance) async -> [Locator] { await held[process]?.watch.locators ?? [] }

    private func remove(_ process: ProcessInstance, _ watch: Watch) { if held[process]?.watch === watch { held[process] = nil } }

    private func watched() -> AsyncStream<(watch: Watch, driven: Bool)> {
        let (stream, continuation) = AsyncStream<(watch: Watch, driven: Bool)>.makeStream()
        let id = UUID()
        continuation.onTermination = { _ in self.followers.withLock { following in following[id] = nil } }
        followers.withLock { following in following[id] = continuation }
        for watched in held.values { continuation.yield(watched) }
        return stream
    }

    private nonisolated func follow(
        _ matching: @escaping @Sendable (NSRunningApplication) -> Bool,
        prompts: Bool,
    ) -> AsyncStream<Result<(instance: Instance, message: Watch.Message?), ApplicationFailure>> {
        let (output, delivery) = AsyncStream<Result<(instance: Instance, message: Watch.Message?), ApplicationFailure>>.makeStream()
        let raising: AsyncStream<Result<WindowServerConnection.Notice, ApplicationFailure>> =
            prompts ? notices(of: [.created, .shown]) : AsyncStream { continuation in continuation.finish() }
        let raisedByAgents = raising.filter { (notice: Result<WindowServerConnection.Notice, ApplicationFailure>) -> Bool in
            guard case .success(let notice) = notice else { return true }
            return notice.owner.flatMap(NSRunningApplication.init(processIdentifier:)).map { owner in owner.activationPolicy != .regular } == true
        }
        let scans = chain(
            [Result<CGWindowID?, ApplicationFailure>.success(nil)].async.prefix(prompts ? 1 : 0),
            raisedByAgents.map { (notice: Result<WindowServerConnection.Notice, ApplicationFailure>) -> Result<CGWindowID?, ApplicationFailure> in notice.map(\.window) },
        )
        let raised = scans.compactMap { (scan: Result<CGWindowID?, ApplicationFailure>) -> [Instance]? in
            guard case .success(let window) = scan else {
                if case .failure(let failure) = scan { delivery.yield(.failure(failure)) }
                return nil
            }
            let listed: [pid_t: [InstanceState.Window]] =
                window.map { (window: CGWindowID) -> [pid_t: [InstanceState.Window]] in InstanceState.Window.all(.optionIncludingWindow, relativeTo: window) }
                ?? InstanceState.Window.all([.optionOnScreenOnly, .excludeDesktopElements])
            let owners: Set<pid_t> = Set(
                listed.filter { (entry: (key: pid_t, value: [InstanceState.Window])) -> Bool in
                    entry.value.contains { window in window.onScreen && window.layer >= InstanceState.Window.Band.modalPanel.level && window.band != .mainMenu && window.band != .status }
                        && NSRunningApplication(processIdentifier: entry.key).map { application in application.activationPolicy != .regular } == true
                        && Result { () throws(ApplicationFailure) in try AXUIElementCreateApplication(entry.key).elements(kAXWindowsAttribute).isEmpty } != .success(true)
                }.keys
            )
            let instances: Result<[Instance], ApplicationFailure> = Result { () throws(ApplicationFailure) in
                try NSWorkspace.shared.instances { application in owners.contains(application.processIdentifier) }
            }
            if case .failure(let failure) = instances { delivery.yield(.failure(failure)) }
            return instances[case: \.success]
        }
        let following = Task {
            let requested = await self.watched().filter { held in (prompts || held.driven) && matching(held.watch.instance.application) }.map(\.watch)
            let prompting = raised.flatMap(\.async).map { instance in await self.watch(instance, driven: false) }
            await withDiscardingTaskGroup { group in
                var followed: [ProcessInstance: Watch] = [:]
                for await watch in merge(requested, prompting) where followed.updateValue(watch, forKey: watch.instance.process) !== watch {
                    group.addTask {
                        for await message in await watch.messages() { delivery.yield(.success((watch.instance, message))) }
                        delivery.yield(.success((watch.instance, nil)))
                    }
                }
            }
        }
        delivery.onTermination = { _ in following.cancel() }
        return output
    }

    package nonisolated func states(
        of matching: @escaping @Sendable (NSRunningApplication) -> Bool
    ) -> some AsyncSequence<Result<OrderedDictionary<Instance, InstanceState>, ApplicationFailure>, Never> & Sendable {
        let (recounts, recount) = AsyncStream<Void>.makeStream(bufferingPolicy: .bufferingNewest(1))
        let (fronts, fronted) = AsyncStream<Void>.makeStream(bufferingPolicy: .bufferingNewest(1))
        let observations: [NSKeyValueObservation] = [
            NSWorkspace.shared.observe(\.runningApplications, options: [.initial]) { _, _ in recount.yield() },
            NSWorkspace.shared.observe(\.frontmostApplication) { _, _ in fronted.yield() },
        ]
        nonisolated(unsafe) let refcon: Unmanaged<AnyObject> = unsafe Unmanaged.passRetained(recount as AnyObject)
        let hosts: [Result<AXObserver, ApplicationFailure>] = Set(InstanceState.Activity.statusBars(in: InstanceState.Window.all(.optionAll)).map(\.host)).map { host in
            var created: AXObserver?
            let subject = "Status item notifications of \(NSRunningApplication(processIdentifier: host)?.localizedName ?? "the menu bar")"
            let answer: AXError = unsafe AXObserverCreate(
                host,
                { _, _, _, refcon in unsafe (refcon.map { refcon in unsafe Unmanaged<AnyObject>.fromOpaque(refcon).takeUnretainedValue() } as? AsyncStream<Void>.Continuation)?.yield() },
                &created,
            )
            guard answer == .success, let observer = created else { return .failure(ApplicationFailure(subject: subject, reason: .accessibility(answer))) }
            let application: AXUIElement = AXUIElementCreateApplication(host)
            let refused: AXError? = [kAXLayoutChangedNotification, kAXUIElementDestroyedNotification].map { notification in
                unsafe AXObserverAddNotification(observer, application, notification as CFString, refcon.toOpaque())
            }
            .first { registered in registered != .success }
            guard let refused else {
                CFRunLoopAddSource(CFRunLoopGetMain(), AXObserverGetRunLoopSource(observer), .commonModes)
                return .success(observer)
            }
            return .failure(ApplicationFailure(subject: subject, reason: .accessibility(refused)))
        }
        recount.onTermination = { _ in
            for observation in observations { observation.invalidate() }
            CFRunLoopPerformBlock(CFRunLoopGetMain(), CFRunLoopMode.commonModes.rawValue) {
                for observer in hosts.compactMap({ host in host[case: \.success] }) { CFRunLoopSourceInvalidate(AXObserverGetRunLoopSource(observer)) }
                unsafe refcon.release()
            }
            CFRunLoopWakeUp(CFRunLoopGetMain())
        }
        let windowed = notices(of: Set(WindowNotification.allCases).subtracting([.reordered, .appUnresponsive, .appResponsive, .secureInputEnabled, .secureInputDisabled]))
            .compactMap { notice in notice[case: \.success] }
            .debounce(for: quiet).map { _ in Result<Bool, ApplicationFailure>.success(false) }
        let refusals = hosts.compactMap { host in host[case: \.failure].map { failure in Result<Bool, ApplicationFailure>.failure(failure) } }.async
        let recounted = chain(refusals, recounts.map { () -> Result<Bool, ApplicationFailure> in .success(true) })
        let triggers = merge(recounted, fronts.map { () -> Result<Bool, ApplicationFailure> in .success(false) }, windowed)
        let counted: (counts: Result<[pid_t: Int], ApplicationFailure>, change: Change) = (.success([:]), .listed(.success([:])))
        let listed = triggers.reductions(into: counted) { (held: inout (counts: Result<[pid_t: Int], ApplicationFailure>, change: Change), trigger: Result<Bool, ApplicationFailure>) in
            guard case .success(let recount) = trigger else {
                held.change = .listed(trigger.map { _ in [:] })
                return
            }
            let windows: [pid_t: [InstanceState.Window]] = InstanceState.Window.all(.optionAll)
            if recount { held.counts = Result { () throws(ApplicationFailure) in try InstanceState.Activity.statusItems(hostedIn: windows) } }
            let counts: Result<[pid_t: Int], ApplicationFailure> = held.counts
            let running: Result<[Instance], ApplicationFailure> = Result { () throws(ApplicationFailure) in
                try NSWorkspace.shared.instances { application in application.activationPolicy != .prohibited && matching(application) }
            }
            held.change = .listed(
                running.map { (instances: [Instance]) -> OrderedDictionary<Instance, InstanceState> in
                    OrderedDictionary(
                        uniqueKeysWithValues: instances.map { (instance: Instance) -> (Instance, InstanceState) in
                            let activity = InstanceState.Activity(
                                status: instance.application.isFinishedLaunching ? .responding : .launching,
                                pendingEventAge: nil,
                                active: instance.application.isActive,
                                windows: windows[instance.process.pid, default: []],
                                usage: nil,
                                statusItems: counts.map { counts in counts[instance.process.pid, default: 0] },
                            )
                            return (instance, .running(activity))
                        }
                    )
                }
            )
        }
        .map(\.change)
        let driven = follow(matching, prompts: false).compactMap { (followed: Result<(instance: Instance, message: Watch.Message?), ApplicationFailure>) -> Change? in
            guard case .success(let followed) = followed else { return nil }
            guard let message = followed.message else { return .released(followed.instance) }
            return message[case: \.state].map { state in .watched(instance: followed.instance, state: state) }
        }
        let seed: (listed: Result<OrderedDictionary<Instance, InstanceState>, ApplicationFailure>, watched: OrderedDictionary<Instance, InstanceState>) = (.success([:]), [:])
        let held = merge(listed, driven).reductions(into: seed) {
            (held: inout (listed: Result<OrderedDictionary<Instance, InstanceState>, ApplicationFailure>, watched: OrderedDictionary<Instance, InstanceState>), change: Change) in
            held.watched.removeAll { _, state in state.is(\.exited) }
            switch change {
                case .listed(let listing): held.listed = listing
                case .watched(let instance, let state): held.watched[instance] = state
                case .released(let instance): held.watched[instance] = nil
            }
        }
        return held.map { held in
            held.listed.map { listing in
                listing.merging(held.watched) { listed, watched in
                    guard let activity = watched[case: \.running] else { return watched }
                    let counted = InstanceState.Activity(
                        status: activity.status,
                        pendingEventAge: activity.pendingEventAge,
                        active: activity.active,
                        windows: activity.windows,
                        usage: activity.usage,
                        statusItems: listed[case: \.running]?.statusItems,
                    )
                    return .running(counted)
                }
            }
        }
        .removeDuplicates()
    }

    package nonisolated func events(of matching: @escaping @Sendable (NSRunningApplication) -> Bool) -> some AsyncSequence<Result<InstanceEvent, ApplicationFailure>, Never> & Sendable {
        follow(matching, prompts: true).compactMap { (followed: Result<(instance: Instance, message: Watch.Message?), ApplicationFailure>) -> Result<InstanceEvent, ApplicationFailure>? in
            switch followed {
                case .success((_, .event(let event)?)): .success(event)
                case .success((_, .failure(let failure)?)), .failure(let failure): .failure(failure)
                case .success((_, .state?)), .success((_, .observation?)), .success((_, nil)): nil
            }
        }
    }

    package nonisolated func observations(of matching: @escaping @Sendable (NSRunningApplication) -> Bool) -> some AsyncSequence<Watch.Report, Never> & Sendable {
        follow(matching, prompts: false).compactMap { (followed: Result<(instance: Instance, message: Watch.Message?), ApplicationFailure>) -> Watch.Report? in
            guard case .success(let followed) = followed else { return nil }
            return followed.message?[case: \.observation]
        }
    }
}
