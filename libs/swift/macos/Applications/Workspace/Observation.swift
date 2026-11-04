public import AppKit
import AppleEvents
import AsyncAlgorithms
public import CasePaths
public import OrderedCollections
import Processes
import Synchronization
import System

// --- [MODELS] --------------------------------------------------------------------------

public enum Target: Sendable {
    case watch([Installation])
    case launch([(installation: Installation, documents: [URL])], arguments: [String], environment: [String: String], newInstance: Bool, waitForExit: Bool = false)
    case act(ElementAction)
    case quit([NSRunningApplication])
    case kill([NSRunningApplication])
}

@CasePathable
public enum Observation: Sendable {
    case instance(NSRunningApplication, InstanceState)
    case states(String, OrderedDictionary<NSRunningApplication, InstanceState>)
    case failure(String, ApplicationFailure)
}

private enum Course {
    case launched(created: Bool, waitForExit: Bool)
    case acted(window: CGWindowID?)
    case quitting
    case killed
}

private enum Event {
    case state(InstanceState)
    case reply(Result<Void, ApplicationFailure>)
    case held(ApplicationFailure)
}

private enum Ending {
    case finished
    case prompted
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension InstanceState {
    fileprivate func noting(created: Bool?, quitting: Bool, hidden: Bool) -> Self {
        switch self {
            case .running(let running): .running(Running(status: running.status, usage: running.usage, hidden: hidden, created: created, quitting: quitting))
            case .failed, .exited: self
        }
    }
}

extension NSRunningApplication {
    fileprivate var key: String { bundleIdentifier ?? String(processIdentifier) }

    fileprivate func follow(
        _ course: Course,
        restoring hidden: Bool,
        ending exits: AsyncStream<Result<ProcessExit, Errno>>,
        into output: AsyncStream<Observation>.Continuation,
    ) async {
        let (events, event): (AsyncStream<Event>, AsyncStream<Event>.Continuation) = AsyncStream.makeStream()
        let reading: Task<Void, Never> = Task {
            for await state: InstanceState in states(ending: exits) { event.yield(.state(state)) }
            event.finish()
        }
        var sending: Task<Void, Never>?
        var holding: Task<Void, Never>?
        defer {
            reading.cancel()
            sending?.cancel()
            holding?.cancel()
        }
        let created: Bool? = if case .launched(let created, _) = course { created } else { nil }
        for await next: Event in events {
            switch next {
                case .reply(.success): continue
                case .reply(.failure(let failure)) where failure.reason.is(\.refused):
                    holding = Task {
                        _ = await AsyncTimerSequence(interval: messagingTimeout, clock: ContinuousClock()).first { _ in true }
                        event.yield(.held(failure))
                    }
                case .held(let failure):
                    switch await state(walking: \.requiresAnswer) {
                        case let read where read.is(\.failed): continue
                        case let read where read.requiresAnswer || read.isUnresponsive: event.yield(.state(read))
                        case _:
                            output.yield(.failure(key, failure))
                            return
                    }
                case .reply(.failure(let failure)):
                    output.yield(.failure(key, failure))
                    return
                case .state(let read):
                    if hidden, !isHidden, read.is(\.running), isFinishedLaunching {
                        do throws(ApplicationFailure) { try await setHidden(true) } catch { output.yield(.failure(key, error)) }
                    }
                    let state: InstanceState = read.noting(created: created, quitting: sending != nil, hidden: isHidden)
                    output.yield(.instance(self, state))
                    guard !state.is(\.exited) else { continue }
                    switch course {
                        case .launched(_, false) where state.next?.is(\.wait) != true: return
                        case .acted(let window) where window.map({ window in state.next?[case: \.capture]?.contains(window) != true }) ?? true: return
                        case .quitting where state.requiresAnswer || state.isUnresponsive || state.is(\.failed), .killed where state.is(\.failed): return
                        case .quitting where sending == nil: sending = Task { event.yield(.reply(await quit())) }
                        case .launched, .acted, .quitting, .killed: continue
                    }
            }
        }
    }

    private func quit() async -> Result<Void, ApplicationFailure> {
        let request: NSAppleEventDescriptor = NSAppleEventDescriptor(
            eventClass: AEEventClass(kCoreEventClass),
            eventID: AEEventID(kAEQuitApplication),
            targetDescriptor: NSAppleEventDescriptor(processIdentifier: processIdentifier),
            returnID: AEReturnID(kAutoGenerateReturnID),
            transactionID: AETransactionID(kAnyTransactionID),
        )
        let subject: String = String(processIdentifier)
        do {
            _ = try await request.sent()
            return .success(())
        } catch  where error.status == OSStatus(userCanceledErr) {
            return .failure(ApplicationFailure(subject: subject, reason: .refused(nil)))
        } catch {
            return .failure(ApplicationFailure(subject: subject, reason: .event(error)))
        }
    }
}

extension NSWorkspace {
    @MainActor
    public func observed(_ target: Target) -> AsyncStream<Observation> {
        let (observations, output): (AsyncStream<Observation>, AsyncStream<Observation>.Continuation) = AsyncStream.makeStream()
        let launched: Mutex<[(instance: NSRunningApplication, created: Bool)]> = Mutex([])
        let (names, work): ([String], @Sendable () async -> Void) =
            switch target {
                case .watch(let installations): (installations.map(\.name), { await Self.watch(installations, into: output) })
                case .launch(let launches, let arguments, let environment, let newInstance, let waitForExit):
                    (
                        launches.map(\.installation.name),
                        {
                            await withDiscardingTaskGroup { group in
                                for (installation, documents): (Installation, [URL]) in launches {
                                    group.addTask {
                                        await Self.launch(
                                            installation,
                                            documents,
                                            arguments: arguments,
                                            environment: environment,
                                            newInstance: newInstance,
                                            waitForExit: waitForExit,
                                            into: output,
                                        ) {
                                            opened in
                                            launched.withLock { held in held.append(opened) }
                                        }
                                    }
                                }
                            }
                        },
                    )
                case .act(let action):
                    (
                        [Result { () throws(ApplicationFailure) in try action.process }[case: \.success].flatMap(NSRunningApplication.init(processIdentifier:))?.localizedName]
                            .compactMap(\.self),
                        { await Self.act(action, into: output) },
                    )
                case .quit(let instances): (instances.compactMap(\.localizedName), { await Self.end(instances, killing: false, into: output) })
                case .kill(let instances): (instances.compactMap(\.localizedName), { await Self.end(instances, killing: true, into: output) })
            }
        let stopsAtPrompt: Bool = if case .launch(_, _, _, _, false) = target { true } else { false }
        let prompts: AsyncStream<(host: String, prompts: OrderedDictionary<NSRunningApplication, InstanceState>)> = systemPrompts(naming: names)
        let observing: Task<Void, Never> = Task {
            let prompted: Mutex<[String: OrderedDictionary<NSRunningApplication, InstanceState>]> = Mutex([:])
            let ending: Ending? = await withTaskGroup(of: Ending?.self) { group in
                group.addTask {
                    for await asking: (host: String, prompts: OrderedDictionary<NSRunningApplication, InstanceState>) in prompts {
                        prompted.withLock { held in held[asking.host] = asking.prompts }
                        output.yield(.states(asking.host, asking.prompts))
                        if stopsAtPrompt { return .prompted }
                    }
                    return nil
                }
                group.addTask {
                    await work()
                    return .finished
                }
                defer { group.cancelAll() }
                for await case let ending? in group { return ending }
                return nil
            }
            guard !Task.isCancelled else { return output.finish() }
            if case .prompted = ending {
                for (instance, created): (NSRunningApplication, Bool) in launched.withLock({ held in held }) {
                    output.yield(.instance(instance, await instance.state(walking: \.requiresAnswer).noting(created: created, quitting: false, hidden: instance.isHidden)))
                }
            }
            for (host, held): (String, OrderedDictionary<NSRunningApplication, InstanceState>) in await Self.shared.currentPrompts(naming: names)
            where prompted.withLock({ shown in shown[host] }) != held {
                output.yield(.states(host, held))
            }
            output.finish()
        }
        output.onTermination = { _ in observing.cancel() }
        return observations
    }

    private static func watch(_ installations: [Installation], into output: AsyncStream<Observation>.Continuation) async {
        await withDiscardingTaskGroup { group in
            for identifier: String in OrderedSet(installations.map(\.identifier)) {
                let states: some AsyncSequence<OrderedDictionary<NSRunningApplication, InstanceState>, Never> & Sendable = await Self.shared.instanceStates { instance in
                    installations.contains { installation in installation.identifier == identifier && installation.runs(instance) }
                }
                group.addTask { for await held: OrderedDictionary<NSRunningApplication, InstanceState> in states { output.yield(.states(identifier, held)) } }
            }
        }
    }

    private static func launch(
        _ installation: Installation,
        _ documents: [URL],
        arguments: [String],
        environment: [String: String],
        newInstance: Bool,
        waitForExit: Bool,
        into output: AsyncStream<Observation>.Continuation,
        opened: @Sendable ((instance: NSRunningApplication, created: Bool)) -> Void,
    ) async {
        do throws(ApplicationFailure) {
            let reused: Bool? = Self.shared.runningApplications.first(where: installation.runs)?.isHidden
            let launched: (instance: NSRunningApplication, created: Bool) = try await Self.shared.launch(
                installation,
                opening: documents,
                arguments: arguments,
                environment: environment,
                newInstance: newInstance,
            )
            opened(launched)
            await launched.instance.follow(
                .launched(created: launched.created, waitForExit: waitForExit),
                restoring: launched.created ? !installation.agent : reused == true,
                ending: termination(of: launched.instance.processIdentifier),
                into: output,
            )
        } catch {
            output.yield(.failure(installation.identifier, error))
        }
    }

    private static func act(_ action: ElementAction, into output: AsyncStream<Observation>.Continuation) async {
        do throws(ApplicationFailure) {
            let owner: pid_t = try action.process
            guard let instance: NSRunningApplication = NSRunningApplication(processIdentifier: owner) else { throw ApplicationFailure(subject: String(owner), reason: .noProcess) }
            let exits: AsyncStream<Result<ProcessExit, Errno>> = termination(of: owner)
            let hidden: Bool = instance.isHidden
            let window: CGWindowID?
            do throws(ApplicationFailure) { window = try await action.perform(in: owner) } catch  where error.reason.is(\.noProcess) { window = nil }
            await instance.follow(.acted(window: window), restoring: hidden, ending: exits, into: output)
        } catch {
            output.yield(.failure(action.element.base64EncodedString(), error))
        }
    }

    private static func end(_ instances: [NSRunningApplication], killing: Bool, into output: AsyncStream<Observation>.Continuation) async {
        await withDiscardingTaskGroup { group in
            for instance: NSRunningApplication in instances {
                let exits: AsyncStream<Result<ProcessExit, Errno>> = termination(of: instance.processIdentifier)
                guard !killing || instance.forceTerminate() else {
                    let reason: ApplicationFailure.Reason = instance.isTerminated ? .noProcess : .refused(nil)
                    output.yield(.failure(instance.key, ApplicationFailure(subject: String(instance.processIdentifier), reason: reason)))
                    continue
                }
                group.addTask {
                    await instance.follow(killing ? .killed : .quitting, restoring: instance.isHidden, ending: exits, into: output)
                }
            }
        }
    }
}
