package import AppKit
@unsafe @preconcurrency import ApplicationServices
import AsyncAlgorithms
import CasePaths
import CasePathsCore
import Files
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

    private var held: [ProcessInstance: Watch] = [:]
    private let followers: Mutex<[UUID: AsyncStream<Watch>.Continuation]> = Mutex([:])
    private let windowServer: Result<WindowServerConnection, ApplicationFailure>
    private let swept: ApplicationFailure?

    package init() {
        let timed: AXError = AXUIElementSetMessagingTimeout(AXUIElementCreateSystemWide(), Float(messagingTimeout / .seconds(1)))
        windowServer = Result { () throws(ApplicationFailure) in
            guard timed == .success else { throw ApplicationFailure(subject: "Messaging timeout of the system-wide element", reason: .accessibility(timed)) }
            return try WindowServerConnection()
        }
        do {
            let tables: [URL] = try ifPresent { () throws -> [URL] in try FileManager.default.contentsOfDirectory(at: Watch.tables, includingPropertiesForKeys: nil) } ?? []
            for table in tables where ProcessInstance(rawValue: table.lastPathComponent)?.hasExited != false { try FileManager.default.removeItem(at: table) }
            swept = nil
        } catch {
            swept = ApplicationFailure(subject: Watch.tables.path(percentEncoded: false), cause: error)
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
        var found: Instance? = held[process]?.instance
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

    package func watch(_ instance: Instance) async -> Watch {
        if let watching = held[instance.process] { return watching }
        let started = Watch(instance)
        held[instance.process] = started
        let windowed: Set<WindowNotification> = Set(WindowNotification.allCases).subtracting([.secureInputEnabled, .secureInputDisabled])
        await started.start(windows: notices(of: windowed, from: instance.process.pid)) { await self.remove(instance.process, started) }
        await started.drive()
        followers.withLock { following in
            for follower in following.values { follower.yield(started) }
        }
        return started
    }

    package func finish() async {
        let watching: [Watch] = Array(held.values)
        await withDiscardingTaskGroup { group in
            for watch in watching { group.addTask { await watch.finish() } }
        }
    }

    package func release(_ process: ProcessInstance) async {
        guard let released = held.removeValue(forKey: process) else { return }
        await released.finish()
    }

    private func remove(_ process: ProcessInstance, _ watch: Watch) { if held[process] === watch { held[process] = nil } }

    private func watched() -> AsyncStream<Watch> {
        let (stream, continuation) = AsyncStream<Watch>.makeStream()
        let id = UUID()
        continuation.onTermination = { _ in self.followers.withLock { following in following[id] = nil } }
        followers.withLock { following in following[id] = continuation }
        for watched in held.values { continuation.yield(watched) }
        return stream
    }

    private nonisolated func follow(_ matching: @escaping @Sendable (NSRunningApplication) -> Bool) -> AsyncStream<(instance: Instance, message: Watch.Message?)> {
        let (output, delivery) = AsyncStream<(instance: Instance, message: Watch.Message?)>.makeStream()
        let following = Task {
            let requested = await self.watched().filter { watch in matching(watch.instance.application) }
            await withDiscardingTaskGroup { group in
                var followed: [ProcessInstance: Watch] = [:]
                for await watch in requested where followed.updateValue(watch, forKey: watch.instance.process) !== watch {
                    group.addTask {
                        for await message in await watch.messages() { delivery.yield((watch.instance, message)) }
                        delivery.yield((watch.instance, nil))
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
        let (relists, relist) = AsyncStream<Void>.makeStream(bufferingPolicy: .bufferingNewest(1))
        let observations: [NSKeyValueObservation] = [
            NSWorkspace.shared.observe(\.runningApplications, options: [.initial]) { _, _ in recount.yield() },
            NSWorkspace.shared.observe(\.frontmostApplication) { _, _ in relist.yield() },
        ]
        let windowed = notices(of: Set(WindowNotification.allCases).subtracting([.reordered, .appUnresponsive, .appResponsive, .secureInputEnabled, .secureInputDisabled]))
        let relisting = Task {
            for await notice in windowed where notice.is(\.success) { relist.yield() }
        }
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
            relisting.cancel()
            for observation in observations { observation.invalidate() }
            CFRunLoopPerformBlock(CFRunLoopGetMain(), CFRunLoopMode.commonModes.rawValue) {
                for observer in hosts.compactMap({ host in host[case: \.success] }) { CFRunLoopSourceInvalidate(AXObserverGetRunLoopSource(observer)) }
                unsafe refcon.release()
            }
            CFRunLoopWakeUp(CFRunLoopGetMain())
        }
        let refused: [ApplicationFailure] = hosts.compactMap { (host: Result<AXObserver, ApplicationFailure>) -> ApplicationFailure? in host[case: \.failure] } + [swept].compactMap(\.self)
        let refusals = refused.map(Result<Bool, ApplicationFailure>.failure).async
        let recounted = chain(refusals, recounts.map { () -> Result<Bool, ApplicationFailure> in .success(true) })
        let triggers = merge(recounted, relists.map { () -> Result<Bool, ApplicationFailure> in .success(false) })
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
        let driven = follow(matching).compactMap { (followed: (instance: Instance, message: Watch.Message?)) -> Change? in
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
        follow(matching).compactMap { (followed: (instance: Instance, message: Watch.Message?)) -> Result<InstanceEvent, ApplicationFailure>? in
            switch followed.message {
                case .event(let event): .success(event)
                case .failure(let failure): .failure(failure)
                case .state, .observation, nil: nil
            }
        }
    }

    package nonisolated func observations(of matching: @escaping @Sendable (NSRunningApplication) -> Bool) -> some AsyncSequence<Watch.Report, Never> & Sendable {
        follow(matching).compactMap { (followed: (instance: Instance, message: Watch.Message?)) -> Watch.Report? in followed.message?[case: \.observation] }
    }
}
