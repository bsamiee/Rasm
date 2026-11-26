public import AppKit
@unsafe @preconcurrency import ApplicationServices
import AsyncAlgorithms
import Carbon.HIToolbox
public import CasePaths
import CasePathsCore
public import OrderedCollections
public import Processes
import Synchronization
import System
import os

// --- [CONSTANTS] -----------------------------------------------------------------------

private let notifications: [String] = [
    kAXMainWindowChangedNotification, kAXFocusedWindowChangedNotification, kAXFocusedUIElementChangedNotification, kAXApplicationActivatedNotification,
    kAXApplicationDeactivatedNotification, kAXApplicationHiddenNotification, kAXApplicationShownNotification, kAXWindowCreatedNotification, kAXWindowMovedNotification,
    kAXWindowResizedNotification, kAXWindowMiniaturizedNotification, kAXWindowDeminiaturizedNotification, kAXDrawerCreatedNotification, kAXSheetCreatedNotification,
    kAXHelpTagCreatedNotification, kAXValueChangedNotification, kAXUIElementDestroyedNotification, kAXElementBusyChangedNotification, kAXMenuOpenedNotification,
    kAXMenuClosedNotification, kAXMenuItemSelectedNotification, kAXRowCountChangedNotification, kAXRowExpandedNotification, kAXRowCollapsedNotification,
    kAXSelectedCellsChangedNotification, kAXUnitsChangedNotification, kAXSelectedChildrenMovedNotification, kAXSelectedChildrenChangedNotification, kAXResizedNotification,
    kAXMovedNotification, kAXCreatedNotification, kAXSelectedRowsChangedNotification, kAXSelectedColumnsChangedNotification, kAXSelectedTextChangedNotification,
    kAXTitleChangedNotification, kAXLayoutChangedNotification, kAXAnnouncementRequestedNotification,
]
private let invalidating: Set<String> = [
    kAXWindowCreatedNotification, kAXDrawerCreatedNotification, kAXSheetCreatedNotification, kAXCreatedNotification, kAXUIElementDestroyedNotification, kAXLayoutChangedNotification,
    kAXRowCountChangedNotification, kAXRowExpandedNotification, kAXRowCollapsedNotification, kAXTitleChangedNotification, kAXWindowMiniaturizedNotification,
    kAXWindowDeminiaturizedNotification, kAXUnitsChangedNotification, kAXValueChangedNotification, kAXSelectedChildrenChangedNotification, kAXSelectedRowsChangedNotification,
    kAXSelectedCellsChangedNotification,
]
private let moving: Set<String> = [kAXWindowMovedNotification, kAXWindowResizedNotification, kAXMovedNotification, kAXResizedNotification]
private let applicationAttributes: [String] = [kAXWindowsAttribute, kAXFocusedUIElementAttribute, kAXFrontmostAttribute, kAXMenuBarAttribute, kAXExtrasMenuBarAttribute]
private let budget: (nodes: Int, time: Duration) = (4000, .seconds(3))
private let menuGap: Duration = .milliseconds(600)
private let quiet: Duration = .milliseconds(100)
private let modalBand: Range<Int> = Int(CGWindowLevelForKey(.modalPanelWindow))..<Int(CGWindowLevelForKey(.utilityWindow))
private let popUpMenuLevel = Int(CGWindowLevelForKey(.popUpMenuWindow))
private let pressOpened: Set<String> = [kAXMenuBarItemRole, kAXPopUpButtonRole, kAXMenuButtonRole]
private let theftNotice: (type: UInt32, subtype: CGEventField?, stolen: Int64, resigning: Set<Int64>, thief: CGEventField?) =
    (21, CGEventField(rawValue: 64), 0x4000, [0x4000, 0x1000], CGEventField(rawValue: 73))
private let fenced: CGEventMask = [CGEventType.keyDown, .keyUp, .flagsChanged, .leftMouseDown, .leftMouseUp, .rightMouseDown, .rightMouseUp, .otherMouseDown, .otherMouseUp]
    .reduce(CGEventMask(0)) { mask, type in mask | CGEventMask(1) << type.rawValue }

// --- [SERVICES] ------------------------------------------------------------------------

struct RunLoopSource: ~Copyable {
    private let source: CFRunLoopSource

    init(_ source: CFRunLoopSource) {
        CFRunLoopAddSource(CFRunLoopGetMain(), source, .commonModes)
        self.source = source
    }

    deinit { CFRunLoopSourceInvalidate(source) }
}

public actor Watch {
    // --- [STATE]

    public nonisolated let instance: Instance
    public nonisolated let language: Locale.Language
    private let build: String
    private let applicationElement: AXUIElement
    private let observer: Result<AXObserver, ApplicationFailure>
    private let recognize: @Sendable (Watch, CGWindowID) async throws(ApplicationFailure) -> [(text: String, frame: CGRect)]
    private let ticks: (stream: AsyncStream<Void>, continuation: AsyncStream<Void>.Continuation) = AsyncStream.makeStream(bufferingPolicy: .bufferingNewest(1))
    private let dirty: Mutex<[AXUIElement: Set<String>]> = Mutex([:])
    private let subscribers: Mutex<[UUID: AsyncStream<Message>.Continuation]?> = Mutex([:])
    private let away: Mutex<Duration> = Mutex(.zero)
    private var observing: RunLoopSource?
    private var unregistered: [String] = notifications
    private var table: [AXUIElement: Int] = [:]
    private var handles: [AXUIElement?] = []
    private var locators: OrderedSet<Locator> = []
    private var located: [Int: Int] = [:]
    private var drawn: [Int: (window: CGWindowID, frame: CGRect)] = [:]
    private var owners: [Int: Int] = [:]
    private var controls: [Int: (control: Control, container: Int)] = [:]
    private var open: OrderedDictionary<Int, Container> = [:]
    private var sent: [Int: Observation] = [:]
    private var bars: [Container] = []
    private var levels: OrderedDictionary<Int, Container> = [:]
    private var menuRead: ContinuousClock.Instant?
    private var focused: Int?
    private var active: Bool?
    private var sequence: Int = 0
    private var state: InstanceState
    private var faults: [(at: Date, line: Result<LogEntry, ProcessFailure>)] = []
    private var recording: Int = 0
    private var following: Task<Void, Never>?
    private var openCommandDocuments: OrderedSet<URL> = []
    private var guarding: [(port: CFMachPort, source: CFRunLoopSource)] = [] {
        didSet {
            for tap in oldValue where !guarding.contains(where: { kept in kept.port === tap.port }) {
                CFRunLoopRemoveSource(CFRunLoopGetMain(), tap.source, .commonModes)
                CFMachPortInvalidate(tap.port)
            }
            for tap in guarding where !oldValue.contains(where: { held in held.port === tap.port }) {
                CFRunLoopAddSource(CFRunLoopGetMain(), tap.source, .commonModes)
                CGEvent.tapEnable(tap: tap.port, enable: true)
            }
        }
    }

    // --- [LIFECYCLE]

    init(_ instance: Instance, recognize: @escaping @Sendable (Watch, CGWindowID) async throws(ApplicationFailure) -> [(text: String, frame: CGRect)]) {
        let bundle: Bundle? = instance.application.bundleURL.flatMap(Bundle.init(url:))
        let preferred: [String]? = bundle?.bundleIdentifier.flatMap { identifier in CFPreferencesCopyAppValue("AppleLanguages" as CFString, identifier as CFString) as? [String] }
        let callback: AXObserverCallbackWithInfo = { _, element, notification, _, refcon in
            guard let refcon = unsafe refcon else { return }
            let watch: Watch = unsafe Unmanaged<Watch>.fromOpaque(refcon).takeUnretainedValue()
            watch.dirty.withLock { held in _ = held[element, default: []].insert(notification as String) }
            watch.ticks.continuation.yield()
        }
        var created: AXObserver?
        let answer: AXError = unsafe AXObserverCreateWithInfoCallback(instance.process.pid, callback, &created)
        let subject = String(instance.process.pid)
        self.instance = instance
        language =
            bundle.flatMap { bundle in Bundle.preferredLocalizations(from: bundle.localizations, forPreferences: preferred).first ?? bundle.developmentLocalization }
            .map { localization in Locale.Language(identifier: Locale.canonicalLanguageIdentifier(from: localization)) } ?? Locale.current.language
        build = bundle?.object(forInfoDictionaryKey: kCFBundleVersionKey as String) as? String ?? ""
        applicationElement = AXUIElementCreateApplication(instance.process.pid)
        observer =
            if let created { .success(created) } else {
                .failure(ApplicationFailure(answer, subject: subject, process: instance.process.pid) { ApplicationFailure(subject: subject, reason: .refused(answer)) })
            }
        self.recognize = recognize
        state = .running(InstanceState.Activity(status: .launching, active: instance.application.isActive, windows: [], usage: nil, listening: .success([])))
    }

    func start(ending: @escaping @Sendable () async -> Void) {
        observing = observer[case: \.success].map { observer in RunLoopSource(AXObserverGetRunLoopSource(observer)) }
        register()
        publish(observer[case: \.failure].map(InstanceState.failed) ?? beat())
        let clock = ContinuousClock()
        let beats = AsyncTimerSequence(interval: messagingTimeout, clock: clock).map { _ in await self.beat() }
        Task { [instance] in
            for await next in instance.lifetime(beats, clock: clock) { self.publish(next) }
            self.finish()
            await ending()
        }
        Task { [ticks] in
            for await _ in ticks.stream.debounce(for: quiet) { self.settle() }
        }
        following = Task { [instance] in
            for await line in LogEntry.stream(of: .process(instance.process.pid), levels: [.error, .fault]) where self.recording > 0 {
                self.faults.append((line[case: \.success]?.timestamp ?? .now, line))
            }
        }
    }

    private func register() {
        guard case .success(let observer) = observer else { return }
        let refcon = unsafe Unmanaged.passUnretained(self).toOpaque()
        unregistered = unregistered.filter { notification in unsafe AXObserverAddNotification(observer, applicationElement, notification as CFString, refcon) == .cannotComplete }
    }

    private func finish() {
        following?.cancel()
        observing = nil
        ticks.continuation.finish()
        let ending: [UUID: AsyncStream<Message>.Continuation] = subscribers.withLock { held in held.take() } ?? [:]
        for subscriber in ending.values { subscriber.finish() }
    }

    // --- [MESSAGES]

    @CasePathable
    public enum Message: Sendable {
        case state(InstanceState)
        case event(InstanceEvent)
        case observation(Report)
    }

    public struct Report: Encodable, Sendable {
        public let instance: ProcessInstance
        public let identifier: String?
        public let language: String
        public let observation: Observation
    }

    public func messages() -> AsyncStream<Message> {
        let (stream, continuation) = AsyncStream<Message>.makeStream()
        let id = UUID()
        continuation.yield(.state(state))
        continuation.yield(
            .event(
                InstanceEvent(instance: instance.process, sequence: sequence, at: .now, change: .started(identifier: instance.application.bundleIdentifier, name: instance.application.localizedName))
            )
        )
        continuation.onTermination = { _ in self.subscribers.withLock { held in held?[id] = nil } }
        let listening: Bool = subscribers.withLock { held in
            held?[id] = continuation
            return held != nil
        }
        if !listening { continuation.finish() }
        return stream
    }

    public func states() -> some AsyncSequence<InstanceState, Never> & Sendable { messages().compactMap { message in message[case: \.state] } }

    public var current: InstanceState { state }

    private func send(_ message: Message) {
        subscribers.withLock { held in
            for subscriber in (held ?? [:]).values { subscriber.yield(message) }
        }
    }

    private func emit(_ change: InstanceEvent.Change) {
        sequence += 1
        send(.event(InstanceEvent(instance: instance.process, sequence: sequence, at: .now, change: change)))
    }

    private func publish(_ next: InstanceState) {
        guard next[case: \.failed]?.reason.is(\.noProcess) != true else { return }
        let (before, after) = (state[case: \.running], next[case: \.running])
        if let after, after.status != before?.status { emit(.status(status: after.status, thread: after.status[case: \.unresponsive])) }
        if let ending = next[case: \.exited], state[case: \.exited] != ending { emit(.exited(termination: ending)) }
        state = next
        send(.state(next))
    }

    // --- [TABLES]

    private func intern(_ element: AXUIElement) -> Int {
        if let id = table[element] { return id }
        table[element] = handles.count
        handles.append(element)
        return handles.count - 1
    }

    private func intern(_ locator: Locator) -> Int { locators.append(locator).index + 1 }

    public func intern(_ locators: [Locator]) -> [Int] { locators.map(intern) }

    private func root(of locator: Locator) -> Int? { locators.firstIndex(of: Locator(steps: Array(locator.steps.prefix(1)))).flatMap { index in located[index + 1] } }

    private func locate(_ model: Model) {
        located.merge(model.located) { _, read in read }
        drawn.merge(model.drawn) { _, read in read }
        owners.merge(model.located.map { ref, element in (element, ref) }) { _, read in read }
        controls.merge(model.containers.flatMap { container in container.controls.map { control in (control.ref, (control: control, container: container.ref)) } }) { _, read in read }
    }

    // --- [READS]

    private enum Reach {
        case window
        case element
        case menu

        var attributes: [String] {
            switch self {
                case .window: windowAttributes
                case .element: elementAttributes
                case .menu: menuAttributes
            }
        }
    }

    private func read(_ element: AXUIElement, _ attributes: [String]) -> (answer: AXError, element: Element) {
        let id = intern(element)
        let (answer, read) = element.element(id, reading: attributes, interning: intern)
        var range: CFTypeRef?
        var text: CFTypeRef?
        guard read.role == kAXTextAreaRole, unsafe AXUIElementCopyAttributeValue(element, kAXVisibleCharacterRangeAttribute as CFString, &range) == .success, let range,
            unsafe AXUIElementCopyParameterizedAttributeValue(element, kAXStringForRangeParameterizedAttribute as CFString, range, &text) == .success, let shown = text as? String
        else { return (answer, read) }
        return (answer, Element(id: id, facts: read.facts.merging([kAXValueAttribute: .text(value: shown)]) { _, shown in shown }, actions: read.actions, window: read.window))
    }

    private func application() throws(ApplicationFailure) -> Element {
        let (answer, app) = read(applicationElement, applicationAttributes)
        guard answer == .success else { throw applicationElement.failure(answer, reading: kAXWindowsAttribute) }
        return app
    }

    private func walk(_ starts: [(id: Int, reach: Reach, depth: Int)], rows: (multi: Int, ids: [Int])?) throws(ApplicationFailure) -> (elements: [Int: Element], truncated: [Int]) {
        let (deadline, top) = (ContinuousClock.now + budget.time, intern(applicationElement))
        var elements: [Int: Element] = [:]
        var pending: [(id: Int, parent: Int?, reach: Reach, depth: Int)] = starts.reversed().map { start in (start.id, nil, start.reach, start.depth) }
        while let next = pending.popLast() {
            guard elements.count < budget.nodes, ContinuousClock.now < deadline else { return (elements, ([next] + pending).compactMap(\.parent)) }
            guard elements[next.id] == nil, let element = handles[next.id] else { continue }
            let (answer, node) = read(element, next.reach.attributes)
            switch answer {
                case .success: elements[next.id] = node
                case .cannotComplete, .apiDisabled: throw applicationElement.failure(answer, reading: kAXChildrenAttribute)
                default: continue
            }
            let children: [Int] =
                switch (rows, next.depth > 0) {
                    case (_, false): []
                    case (.some(let rows), true) where rows.multi == next.id: rows.ids
                    case (_, true): node.children
                }
            let inner: Reach = next.reach == .menu ? .menu : .element
            pending += children.reversed().map { child -> (id: Int, parent: Int?, reach: Reach, depth: Int) in (child, next.id, inner, next.depth - 1) }
            if let parent = node.facts[kAXParentAttribute]?.element, next.reach != .menu, parent != top, elements[parent] == nil { pending.append((parent, nil, .element, 0)) }
        }
        return (elements, [])
    }

    private func hierarchy(of app: Element, full: Set<Int>, menus: Bool = false, rows: (multi: Int, ids: [Int])? = nil) throws(ApplicationFailure) -> Hierarchy {
        let windows: [Int] = app.facts[kAXWindowsAttribute]?.elements ?? []
        let bars: [Int] = menus ? [kAXMenuBarAttribute, kAXExtrasMenuBarAttribute].compactMap { attribute in app.facts[attribute]?.element } : []
        let starts: [(id: Int, reach: Reach, depth: Int)] =
            bars.map { bar in (id: bar, reach: Reach.menu, depth: 2) } + windows.map { window in (id: window, reach: Reach.window, depth: full.contains(window) ? Int.max : 0) }
        let (elements, truncated) = try walk(starts, rows: rows)
        return Hierarchy(
            elements: Array(elements.values),
            roots: windows + bars,
            windows: InstanceState.Window.all(.optionAll)[instance.process.pid, default: []],
            regular: instance.application.activationPolicy == .regular,
            focused: app.facts[kAXFocusedUIElementAttribute]?.element,
            truncated: truncated + windows.filter { window in !full.contains(window) },
            extras: app.facts[kAXExtrasMenuBarAttribute]?.element,
        )
    }

    private func spaced<Value>(clock: some Clock<Duration>, _ read: () throws(ApplicationFailure) -> Value) async throws(ApplicationFailure) -> Value {
        while let last = menuRead, case let remaining = menuGap - last.duration(to: .now), remaining > .zero {
            do { try await clock.sleep(for: remaining) } catch { throw ApplicationFailure(subject: String(instance.process.pid), reason: .cancelled) }
        }
        defer { menuRead = .now }
        return try read()
    }

    // --- [CHANGES]

    private func merged(_ hierarchy: Hierarchy, full: Set<Int>) throws(ApplicationFailure) -> Model {
        let read = Model(hierarchy, sequence: sequence, ref: intern)
        for control in read.containers.flatMap(\.controls) {
            guard let held = controls[control.ref]?.control else { continue }
            if let value = control.value, held.value != value { emit(.value(control: control.ref, kind: control.kind, fact: value)) }
            if let evidence = control.selected, held.selected != evidence || held.value != control.value { emit(.selection(control: control.ref, kind: control.kind, evidence: evidence)) }
        }
        locate(read)
        let windows: Set<Int> = Set(hierarchy.roots)
        let menuKinds: Set<Container.Kind> = [.menuBar, .statusItem]
        let menuBars: [Container] = read.containers.filter { (container: Container) -> Bool in menuKinds.contains(container.kind) }
        let within: [Container] = read.containers.filter { (container: Container) -> Bool in
            let root: Int? = root(of: container.locator)
            return !menuKinds.contains(container.kind) && root.map(full.contains) == true
        }
        let fresh: OrderedDictionary<Int, Container> = OrderedDictionary(within.map { (container: Container) -> (Int, Container) in (container.ref, container) }) { first, _ in first }
        for (ref, container) in open where container.kind != .menu && fresh[ref] == nil && root(of: container.locator).map({ root in full.contains(root) || !windows.contains(root) }) != false {
            emit(.closed(container: ref, kind: container.kind, title: container.title))
            (sent[ref], open[ref]) = (nil, nil)
        }
        for (ref, container) in fresh where open[ref] == nil {
            guard container.asking else {
                emit(.opened(container: ref, kind: container.kind, title: container.title))
                continue
            }
            let labels: [Int: String] = Dictionary(container.controls.compactMap { control in control.label.map { label in (control.ref, label.text) } }) { first, _ in first }
            let marked: OrderedSet<Int> = OrderedSet(
                [container.default, container.cancel, container.close].compactMap(\.self) + container.controls.filter { control in control.kind == .action }.map(\.ref)
            )
            let answers: [InstanceEvent.Answer] = marked.map { answer in InstanceEvent.Answer(ref: answer, label: labels[answer]) }
            emit(.asking(container: ref, kind: container.kind, title: container.title, text: container.text, answers: answers))
        }
        open.merge(fresh) { _, read in read }
        bars = menuBars.isEmpty ? bars : menuBars
        for container in fresh.values + menuBars { try observe(container, cause: nil) }
        return Model(sequence: sequence, containers: read.containers + open.values.filter { container in container.kind == .menu }, located: read.located)
    }

    private func observe(_ container: Container, cause: Locator?) throws(ApplicationFailure) {
        let (now, last): (Date, Observation?) = (.now, sent[container.ref])
        let observation = try Observation(container, in: open, build: build, cause: cause, opened: last?.opened ?? now, at: now) { ref in locators[ref - 1] }
        guard last.map({ last in (last.kind, last.title, last.controls) != (observation.kind, observation.title, observation.controls) }) ?? true else { return }
        sent[container.ref] = observation
        send(.observation(Report(instance: instance.process, identifier: instance.application.bundleIdentifier, language: language.minimalIdentifier, observation: observation)))
    }

    private func reconcile(_ touched: Set<Int>) throws(ApplicationFailure) {
        let app = try application()
        register()
        if let frontmost = app.facts[kAXFrontmostAttribute]?.bool, frontmost != active {
            if active != nil { emit(.frontmost(active: frontmost)) }
            active = frontmost
        }
        if let element = app.facts[kAXFocusedUIElementAttribute]?.element, element != focused {
            focused = element
            if let control = owners[element], let held = controls[control] { emit(.focused(control: control, kind: held.control.kind)) }
        }
        let windows: Set<Int> = Set(app.facts[kAXWindowsAttribute]?.elements ?? [])
        let known: Set<Int> = Set(open.values.filter { container in container.parent == nil && container.kind != .menu }.compactMap { container in located[container.ref] })
        guard windows != known || !touched.isEmpty else { return }
        let full: Set<Int> = touched.union(windows.subtracting(known))
        _ = try merged(try hierarchy(of: app, full: full), full: full)
    }

    private func settle() {
        let marked: [AXUIElement: Set<String>] = dirty.withLock { held in exchange(&held, with: [:]) }
        for (element, names) in marked where !names.isDisjoint(with: moving) {
            guard let ref = table[element].flatMap({ id in owners[id] }), let container = open[ref], let frame = read(element, frameAttributes).element.frame else { continue }
            emit(.moved(container: ref, kind: container.kind, frame: frame))
        }
        for (element, names) in marked where names.contains(kAXMenuClosedNotification) {
            guard let ref = table[element].flatMap({ id in owners[id] }), let menu = open[ref], menu.kind == .menu else { continue }
            emit(.closed(container: ref, kind: .menu, title: menu.title))
            (sent[ref], open[ref]) = (nil, nil)
        }
        for (element, names) in marked where names.contains(kAXMenuOpenedNotification) && !names.contains(kAXMenuClosedNotification) {
            do throws(ApplicationFailure) { try track(element) } catch { publish(.failed(error)) }
        }
        let touched: Set<Int> = Set(
            marked.filter { _, names in !names.isDisjoint(with: invalidating) }.keys.compactMap { (element: AXUIElement) -> Int? in
                table[element].flatMap { id in owners[id] }.flatMap { ref in root(of: locators[ref - 1]) }
                    ?? read(element, [kAXTopLevelUIElementAttribute]).element.facts[kAXTopLevelUIElementAttribute]?.element
            }
        )
        do throws(ApplicationFailure) { try reconcile(touched) } catch { publish(.failed(error)) }
        for (element, names) in marked where names.contains(kAXUIElementDestroyedNotification) {
            if let id = table.removeValue(forKey: element) { handles[id] = nil }
        }
    }

    private func beat() -> InstanceState {
        let subject = String(instance.process.pid)
        let thread: MainThread
        do throws(Errno) { thread = try MainThread(of: instance.process) } catch { return .failed(ApplicationFailure(error, process: subject)) }
        let after: MainThread?
        do throws(ApplicationFailure) {
            if !thread.stopped, instance.application.activationPolicy != .prohibited { try reconcile([]) }
            after = nil
        } catch  where error.reason.is(\.unanswered) {
            do throws(Errno) { after = try MainThread(of: instance.process) } catch { return .failed(ApplicationFailure(error, process: subject)) }
        } catch {
            return .failed(error)
        }
        let status: InstanceState.Activity.Status =
            switch (thread.stopped, after, instance.application.isFinishedLaunching || open.values.contains(where: \.asking)) {
                case (true, _, _): .unresponsive(.stopped)
                case (false, .some(let after), true): .unresponsive(after.status(since: thread))
                case (false, _, false): .launching
                case (false, .none, true): .responding
            }
        let blocked = status.is(\.unresponsive)
        let byWindow: [CGWindowID: Container] = Dictionary(
            open.values.compactMap { container in container.parent == nil ? container.window.map { window in (window, container) } : nil }
        ) { first, _ in first }
        let usage: Result<ResourceUsage, Errno>? = blocked ? state[case: \.running]?.usage ?? Result { () throws(Errno) in try ResourceUsage(of: instance.process) } : nil
        let windows: [InstanceState.Window] = InstanceState.Window.all(.optionAll)[instance.process.pid, default: []].map { row in
            row.described(by: byWindow[row.number], blocking: blocked && row.onScreen && modalBand.contains(row.layer))
        }
        let listening = Result { () throws(Errno) in try instance.process.listeningPorts() }
        return .running(InstanceState.Activity(status: status, active: active ?? instance.application.isActive, windows: windows, usage: usage, listening: listening))
    }

    // --- [MODEL]

    public func model(_ scope: Model.Scope, clock: some Clock<Duration> = ContinuousClock()) async throws(ApplicationFailure) -> Model {
        switch scope {
            case .application:
                var read: Hierarchy = try await spaced(clock: clock) { () throws(ApplicationFailure) -> Hierarchy in
                    let app = try application()
                    return try hierarchy(of: app, full: Set(app.facts[kAXWindowsAttribute]?.elements ?? []), menus: true)
                }
                for row in read.flyouts { read.recognized[row.number] = try await recognize(self, row.number) }
                let model = try merged(read, full: Set(read.roots))
                return Model(sequence: model.sequence, containers: model.containers, located: model.located, pointer: Pointer(of: instance.application, windows: read.windows))
            case .container(let ref):
                guard let within = open[ref]?.locator, let root = root(of: within) else { throw stale(ref) }
                let model = try merged(try hierarchy(of: try application(), full: [root]), full: [root])
                return Model(
                    sequence: model.sequence,
                    containers: model.containers.filter { container in container.locator.steps.starts(with: within.steps) },
                    located: model.located,
                )
            case .menu(let ref):
                return try await level(of: ref, clock: clock)
            case .rows(let ref, let from, let count):
                guard let multi = located[ref], let element = handles[multi], let held = controls[ref], let root = root(of: held.control.locator) else { throw stale(ref) }
                var listed: CFArray?
                let answer = unsafe AXUIElementCopyAttributeValues(element, kAXRowsAttribute as CFString, from, count, &listed)
                guard answer == .success else { throw element.failure(answer, reading: kAXRowsAttribute) }
                let rows: [Int] = listed.flatMap { list in Fact(list, interning: intern) }?.elements ?? []
                let model = Model(try hierarchy(of: try application(), full: [root], rows: (multi, rows)), sequence: sequence, ref: intern)
                return Model(sequence: sequence, containers: model.containers.filter { read in read.ref == held.container }, located: model.located)
            case .menus:
                return Model(sequence: sequence, containers: bars + levels.values, located: [:])
        }
    }

    private func level(of ref: Int, clock: some Clock<Duration>) async throws(ApplicationFailure) -> Model {
        guard let item = located[ref], handles[item] != nil, let opener = controls[ref] else { throw stale(ref) }
        var level = Model(sequence: sequence, containers: [], located: [:])
        for _ in 0..<2 where level.containers.first?.controls.isEmpty != false {
            level = try await spaced(clock: clock) { () throws(ApplicationFailure) -> Model in
                let (elements, truncated) = try walk([(id: item, reach: .menu, depth: 3)], rows: nil)
                let menus: [Int] = elements[item]?.facts[kAXChildrenAttribute]?.elements ?? []
                let hierarchy = Hierarchy(elements: Array(elements.values), roots: menus, windows: [], regular: true, focused: nil, truncated: truncated, extras: nil)
                return Model(hierarchy, sequence: sequence, under: Dictionary(menus.map { menu in (menu, opener.control.locator) }) { first, _ in first }, ref: intern)
            }
        }
        if let held = level.containers.first, !held.controls.isEmpty {
            levels[ref] = held
            locate(level)
            try observe(held, cause: opener.control.locator)
        }
        return level
    }

    // --- [MENUS]

    public enum Presentation: Encodable, Sendable {
        case behind(held: Duration)
        case forward

        private enum CodingKeys: CodingKey {
            case behind, forward
        }

        private enum HeldKeys: CodingKey {
            case held
        }

        public func encode(to encoder: any Encoder) throws {
            var cases = encoder.container(keyedBy: CodingKeys.self)
            switch self {
                case .behind(let held):
                    var fields = cases.nestedContainer(keyedBy: HeldKeys.self, forKey: .behind)
                    try fields.encode(held / .seconds(1), forKey: .held)
                case .forward: _ = cases.nestedContainer(keyedBy: HeldKeys.self, forKey: .forward)
            }
        }
    }

    private func track(_ menu: AXUIElement) throws(ApplicationFailure) {
        let head: Element = read(menu, frameAttributes + [kAXParentAttribute, kAXTopLevelUIElementAttribute]).element
        guard let frame = head.frame, !frame.isEmpty else { return }
        let row: InstanceState.Window? = InstanceState.Window.all(.optionOnScreenOnly)[instance.process.pid, default: []]
            .filter { row in row.layer == popUpMenuLevel && row.frame.contains(frame) }
            .min { first, second in first.frame.width * first.frame.height < second.frame.width * second.frame.height }
        let opener: Int? = [kAXParentAttribute, kAXTopLevelUIElementAttribute].compactMap { attribute in head.facts[attribute]?.element.flatMap { id in owners[id] } }.first
        let (elements, truncated) = try walk([(id: head.id, reach: .menu, depth: 2)], rows: nil)
        let shown: [Element] = elements.values.map { element in element.id == head.id ? Element(id: element.id, facts: element.facts, actions: element.actions, window: row?.number) : element }
        let hierarchy = Hierarchy(elements: shown, roots: [head.id], windows: row.map { row in [row] } ?? [], regular: true, focused: nil, truncated: truncated, extras: nil)
        let model = Model(hierarchy, sequence: sequence, under: opener.map { ref in [head.id: locators[ref - 1]] } ?? [:], ref: intern)
        guard let container = model.containers.first(where: { container in container.kind == .menu }), open[container.ref] == nil else { return }
        locate(model)
        open[container.ref] = container
        emit(.opened(container: container.ref, kind: .menu, title: container.title))
        try observe(container, cause: opener.flatMap { ref in controls[ref]?.control.locator })
    }

    public nonisolated func presenting<Value>(
        _ ref: Int?,
        clock: some Clock<Duration> = ContinuousClock(),
        _ body: ([Container]) async throws(ApplicationFailure) -> Value,
    ) async throws(ApplicationFailure) -> (value: Value, presentation: Presentation?) {
        var levels: [(menu: Container, presentation: Presentation?)] = []
        let value: Result<Value, ApplicationFailure>
        do throws(ApplicationFailure) {
            while let level = try await popUp(toward: ref, past: levels.count, clock: clock) { levels.append(level) }
            value = .success(try await body(levels.map(\.menu)))
        } catch {
            value = .failure(error)
        }
        try await cancel(levels.first(where: { level in level.presentation != nil })?.menu.ref, clock: clock)
        let presentation: Presentation? = levels.compactMap(\.presentation).reduce(nil) { (folded: Presentation?, next: Presentation) -> Presentation? in
            switch (folded, next) {
                case (.none, _): next
                case (.forward, _), (_, .forward): .forward
                case (.behind(let held), .behind(let more)): .behind(held: held + more)
            }
        }
        return (try value.get(), presentation)
    }

    private func popUp(toward ref: Int?, past count: Int, clock: some Clock<Duration>) async throws(ApplicationFailure) -> (menu: Container, presentation: Presentation?)? {
        guard let ref else { return nil }
        guard locators.indices.contains(ref - 1) else { throw stale(ref) }
        let steps: [Locator.Step] = locators[ref - 1].steps
        let prefixes: [(ref: Int?, inner: Bool)] = steps.indices.map { index in
            (locators.firstIndex(of: Locator(steps: Array(steps[...index]))).map { position in position + 1 }, index < steps.count - 1)
        }
        let openers: [Control] = prefixes.compactMap { (prefix: Int?, inner: Bool) -> Control? in
            prefix.flatMap { (prefix: Int) -> Control? in controls[prefix]?.control }.flatMap { (control: Control) -> Control? in
                inner || control.opens != nil || pressOpened.contains(control.role) ? control : nil
            }
        }
        guard openers.indices.contains(count) else { return nil }
        let opener: Control = openers[count]
        if let level = open.values.first(where: { menu in menu.kind == .menu && menu.locator.steps.dropLast().elementsEqual(opener.locator.steps) }) { return (level, nil) }
        let element: AXUIElement = try await resolve(opener.ref)
        let shown: Element = read(element, frameAttributes).element
        let action: String? = opener.opens != nil || pressOpened.contains(opener.role) ? kAXPressAction : shown.actions.first { action in action == kAXShowMenuAction }
        guard let action else {
            throw ApplicationFailure(subject: "\(opener.ref), which lists neither \(kAXPressAction) on a menu nor \(kAXShowMenuAction)", reason: .refused(nil))
        }
        guard let owner: pid_t = keyFocusProcess() else { throw ApplicationFailure(subject: "process holding key focus before \(action) on \(opener.ref)", reason: .refused(nil)) }
        let behind: Bool = owner != instance.process.pid
        guard !behind || !IsSecureEventInputEnabled() else {
            throw ApplicationFailure(subject: "\(action) on \(opener.ref) while secure input hides the keys of process \(owner) from every fence", reason: .refused(nil))
        }
        for forward in [false, true] where forward ? behind : shown.frame?.isEmpty == false {
            if forward {
                guarding = []
                let activations = messages()
                let raised = AXUIElementSetAttributeValue(applicationElement, kAXFrontmostAttribute as CFString, kCFBooleanTrue)
                let activated: Void? = await Self.next(
                    { (change: InstanceEvent.Change) -> Void? in if case .frontmost(true) = change { () } else { nil } },
                    in: activations,
                    clock: clock,
                )
                guard raised == .success, activated != nil else {
                    throw ApplicationFailure(subject: "\(kAXFrontmostAttribute) of process \(instance.process.pid)", reason: .refused(raised))
                }
            }
            let fencing: Bool = !forward && behind
            let notices: [CFMachPort?] =
                fencing && guarding.isEmpty
                ? [
                    unsafe CGEvent.tapCreateForPid(
                        pid: owner,
                        place: .headInsertEventTap,
                        options: .defaultTap,
                        eventsOfInterest: CGEventMask(1) << theftNotice.type,
                        callback: { _, type, event, refcon in
                            guard let refcon = unsafe refcon, type.rawValue == theftNotice.type, theftNotice.subtype.map(event.getIntegerValueField) == theftNotice.stolen else {
                                return unsafe Unmanaged.passUnretained(event)
                            }
                            let watch: Watch = unsafe Unmanaged<Watch>.fromOpaque(refcon).takeUnretainedValue()
                            guard theftNotice.thief.map(event.getIntegerValueField) == Int64(watch.instance.process.pid) else { return unsafe Unmanaged.passUnretained(event) }
                            let noticed: ContinuousClock.Instant = .now
                            var own: Int32 = 0
                            if unsafe stealKeyFocus(mainConnection(), 0xC80, &own) == .success { _ = releaseKeyFocus(own) }
                            watch.away.withLock { held in held += noticed.duration(to: .now) }
                            return nil
                        },
                        userInfo: unsafe Unmanaged.passUnretained(self).toOpaque(),
                    ),
                    unsafe CGEvent.tapCreateForPid(
                        pid: instance.process.pid,
                        place: .headInsertEventTap,
                        options: .defaultTap,
                        eventsOfInterest: CGEventMask(1) << theftNotice.type,
                        callback: { _, _, event, _ in
                            theftNotice.subtype.map(event.getIntegerValueField).map { subtype in theftNotice.resigning.contains(subtype) } == true
                                ? nil : unsafe Unmanaged.passUnretained(event)
                        },
                        userInfo: nil,
                    ),
                ]
                : []
            let fence: CFMachPort? =
                fencing
                ? unsafe CGEvent.tapCreateForPid(
                    pid: instance.process.pid,
                    place: .headInsertEventTap,
                    options: .defaultTap,
                    eventsOfInterest: fenced,
                    callback: { _, type, event, owner in
                        switch (type, event.getIntegerValueField(.eventSourceUnixProcessID)) {
                            case (_, Int64(getpid())), (.tapDisabledByTimeout, _), (.tapDisabledByUserInput, _): return unsafe Unmanaged.passUnretained(event)
                            default:
                                event.postToPid(pid_t(Int(bitPattern: owner)))
                                return nil
                        }
                    },
                    userInfo: UnsafeMutableRawPointer(bitPattern: Int(owner)),
                ) : nil
            let arming: [CFMachPort?] = notices + (fencing ? [fence] : [])
            guarding += arming.compactMap(\.self).map { tap in (tap, CFMachPortCreateRunLoopSource(nil, tap, 0)) }
            guard arming.allSatisfy({ tap in tap != nil }) else {
                throw ApplicationFailure(subject: "key focus fence between processes \(owner) and \(instance.process.pid)", reason: .refused(nil))
            }
            let events = messages()
            away.withLock { held in held = .zero }
            AXUIElementSetMessagingTimeout(element, Float(messagingTimeout / 2 / .seconds(1)))
            let answer = AXUIElementPerformAction(element, action as CFString)
            AXUIElementSetMessagingTimeout(element, Float(messagingTimeout / .seconds(1)))
            guard [.success, .cannotComplete].contains(answer) else { throw ApplicationFailure(subject: "\(action) on \(opener.ref)", reason: .refused(answer)) }
            let opened: Int? = await Self.next(
                { (change: InstanceEvent.Change) -> Int? in if case .opened(let menu, .menu, _) = change { menu } else { nil } },
                in: events,
                clock: clock,
            )
            let returned: Bool = !fencing || keyFocusProcess() == owner
            guarding.removeAll { tap in tap.port === fence }
            guard let level = opened.flatMap({ menu in open[menu] }) else {
                _ = AXUIElementPerformAction(element, kAXCancelAction as CFString)
                continue
            }
            guard returned else {
                try await cancel(level.ref, clock: clock)
                throw ApplicationFailure(subject: "key focus of process \(owner), which \(action) on \(opener.ref) kept", reason: .refused(nil))
            }
            return (level, forward ? .forward : .behind(held: away.withLock { held in held }))
        }
        throw ApplicationFailure(subject: "\(action) on \(opener.ref)", reason: .noWindow)
    }

    private func cancel(_ level: Int?, clock: some Clock<Duration>) async throws(ApplicationFailure) {
        defer { guarding = [] }
        guard let level, open[level] != nil, let element = located[level].flatMap({ id in handles[id] }) else { return }
        let events = messages()
        let answer = AXUIElementPerformAction(element, kAXCancelAction as CFString)
        guard await Self.next({ (change: InstanceEvent.Change) -> Int? in if case .closed(level, .menu, _) = change { level } else { nil } }, in: events, clock: clock) != nil else {
            throw ApplicationFailure(subject: "\(kAXCancelAction) on \(level)", reason: answer == .success ? .unanswered(nil) : .refused(answer))
        }
    }

    private static func next<Matched: Sendable>(
        _ change: @escaping @Sendable (InstanceEvent.Change) -> Matched?,
        in events: AsyncStream<Message>,
        clock: some Clock<Duration>,
    ) async -> Matched? {
        let matched = events.compactMap { (message: Message) -> Matched? in message[case: \.event].flatMap { event in change(event.change) } }.map(Optional.some)
        let expired = AsyncTimerSequence(interval: messagingTimeout, clock: clock).map { _ in Matched?.none }
        return await merge(matched, expired).first { _ in true }.flatMap(\.self)
    }

    // --- [ACTIONS]

    public func perform<Value: Sendable>(on ref: Int?, _ body: @Sendable (AXUIElement?) throws(ApplicationFailure) -> Value) async throws(ApplicationFailure) -> Value {
        let element: AXUIElement? = if let ref { try await resolve(ref) } else { nil }
        return try body(element)
    }

    private func resolve(_ ref: Int) async throws(ApplicationFailure) -> AXUIElement {
        if located[ref].flatMap({ id in handles[id] }) == nil { _ = try await model(.container(ref: controls[ref]?.container ?? ref)) }
        guard let element = located[ref].flatMap({ id in handles[id] }) else { throw stale(ref) }
        return element
    }

    public func frame(of ref: Int) async throws(ApplicationFailure) -> (window: CGWindowID, frame: CGRect) {
        if let shown = drawn[ref] {
            guard InstanceState.Window.all(.optionOnScreenOnly)[instance.process.pid, default: []].contains(where: { row in row.number == shown.window && row.onScreen }) else { throw stale(ref) }
            return shown
        }
        let element = try await resolve(ref)
        guard let frame = read(element, frameAttributes).element.frame else { throw ApplicationFailure(subject: String(ref), reason: .unreadable(attribute: kAXPositionAttribute, error: .noValue)) }
        guard let window = open[controls[ref]?.container ?? ref]?.window else { throw ApplicationFailure(subject: String(ref), reason: .noWindow) }
        return (window, frame)
    }

    public func documents() -> [CGWindowID: URL] {
        Dictionary(open.values.compactMap { container in container.window.flatMap { window in container.document.map { document in (window, document) } } }) { first, _ in first }
    }

    public func addOpenCommandDocuments(_ documents: [URL]) { openCommandDocuments.append(contentsOf: documents) }

    public var documentFiles: [URL] { openCommandDocuments.union(open.values.compactMap(\.document)).elements.filter(\.isFileURL) }

    public func recordFaults() -> Date {
        recording += 1
        return .now
    }

    public func recordedFaults(since start: Date) -> (entries: [LogEntry], failures: [ProcessFailure]) {
        let kept = faults.filter { fault in fault.at >= start }.map(\.line)
        recording -= 1
        if recording == 0 { faults = [] }
        return (kept.compactMap { line in line[case: \.success] }, kept.compactMap { line in line[case: \.failure] })
    }

    private func stale(_ ref: Int) -> ApplicationFailure {
        let steps: [Locator.Step] = locators.indices.contains(ref - 1) ? locators[ref - 1].steps : []
        let reached: Container? = open.values.lazy.filter { container in steps.starts(with: container.locator.steps) }.max { first, second in first.locator.steps.count < second.locator.steps.count }
        return ApplicationFailure(subject: String(ref), reason: .stale(reached: reached?.locator.steps.count ?? 0, candidates: reached?.controls.compactMap(\.label?.text) ?? []))
    }
}

public actor Watches {
    private enum Change: Sendable {
        case listed(OrderedDictionary<Instance, InstanceState>)
        case watched(instance: Instance, state: InstanceState)
    }

    private var held: [ProcessInstance: (watch: Watch, driven: Bool)] = [:]
    private let recognize: @Sendable (Watch, CGWindowID) async throws(ApplicationFailure) -> [(text: String, frame: CGRect)]
    private let followers: Mutex<[UUID: AsyncStream<(watch: Watch, driven: Bool)>.Continuation]> = Mutex([:])

    public init(recognize: @escaping @Sendable (Watch, CGWindowID) async throws(ApplicationFailure) -> [(text: String, frame: CGRect)]) {
        AXUIElementSetMessagingTimeout(AXUIElementCreateSystemWide(), Float(messagingTimeout / .seconds(1)))
        self.recognize = recognize
    }

    public func watch(_ instance: Instance, driven: Bool = true) async -> Watch {
        let before: (watch: Watch, driven: Bool)? = held[instance.process]
        let after: (watch: Watch, driven: Bool) = (before?.watch ?? Watch(instance, recognize: recognize), driven || before?.driven == true)
        held[instance.process] = after
        if before == nil { await after.watch.start { await self.remove(instance.process) } }
        if before?.driven != after.driven {
            followers.withLock { following in
                for follower in following.values { follower.yield(after) }
            }
        }
        return after.watch
    }

    private func remove(_ process: ProcessInstance) { held[process] = nil }

    private func watched() -> AsyncStream<(watch: Watch, driven: Bool)> {
        let (stream, continuation) = AsyncStream<(watch: Watch, driven: Bool)>.makeStream()
        let id = UUID()
        continuation.onTermination = { _ in self.followers.withLock { following in following[id] = nil } }
        followers.withLock { following in following[id] = continuation }
        for watched in held.values { continuation.yield(watched) }
        return stream
    }

    private nonisolated func follow(_ matching: @escaping @Sendable (NSRunningApplication) -> Bool, prompts: Bool) -> AsyncStream<(instance: Instance, message: Watch.Message)> {
        let raised = AsyncTimerSequence(interval: messagingTimeout, clock: ContinuousClock()).prefix(prompts ? .max : 0).map { _ in
            let owners: Set<pid_t> = Set(
                InstanceState.Window.all([.optionOnScreenOnly, .excludeDesktopElements]).filter { _, windows in windows.contains { window in modalBand.contains(window.layer) } }.keys
            )
            return NSWorkspace.shared.instances { application in owners.contains(application.processIdentifier) && application.activationPolicy != .regular }
        }
        let (output, delivery) = AsyncStream<(instance: Instance, message: Watch.Message)>.makeStream()
        let following = Task {
            let requested = await self.watched().filter { held in (prompts || held.driven) && matching(held.watch.instance.application) }.map(\.watch)
            let prompting = raised.flatMap(\.async).map { instance in await self.watch(instance, driven: false) }
            await withDiscardingTaskGroup { group in
                var followed: Set<ProcessInstance> = []
                for await watch in merge(requested, prompting) where followed.insert(watch.instance.process).inserted {
                    group.addTask { for await message in await watch.messages() { delivery.yield((watch.instance, message)) } }
                }
            }
        }
        delivery.onTermination = { _ in following.cancel() }
        return output
    }

    public nonisolated func states(of matching: @escaping @Sendable (NSRunningApplication) -> Bool) -> some AsyncSequence<OrderedDictionary<Instance, InstanceState>, Never> & Sendable {
        let (changes, changed) = AsyncStream<Void>.makeStream(bufferingPolicy: .bufferingNewest(1))
        let observations: [NSKeyValueObservation] = [
            NSWorkspace.shared.observe(\.runningApplications, options: [.initial]) { _, _ in changed.yield() },
            NSWorkspace.shared.observe(\.frontmostApplication) { _, _ in changed.yield() },
        ]
        changed.onTermination = { _ in
            for observation in observations { observation.invalidate() }
        }
        let listed = merge(changes, AsyncTimerSequence(interval: messagingTimeout, clock: ContinuousClock()).map { _ in }).map { _ -> Change in
            let windows: [pid_t: [InstanceState.Window]] = InstanceState.Window.all(.optionAll)
            let running: [Instance] = NSWorkspace.shared.instances { application in application.activationPolicy != .prohibited && matching(application) }
            return .listed(
                OrderedDictionary(
                    uniqueKeysWithValues: running.map { instance -> (Instance, InstanceState) in
                        let activity = InstanceState.Activity(
                            status: instance.application.isFinishedLaunching ? .responding : .launching,
                            active: instance.application.isActive,
                            windows: windows[instance.process.pid, default: []],
                            usage: nil,
                            listening: Result { () throws(Errno) in try instance.process.listeningPorts() },
                        )
                        return (instance, .running(activity))
                    }
                )
            )
        }
        let driven = follow(matching, prompts: false).compactMap { (instance: Instance, message: Watch.Message) -> Change? in
            message[case: \.state].map { state in .watched(instance: instance, state: state) }
        }
        let held = merge(listed, driven).reductions(into: (listed: OrderedDictionary<Instance, InstanceState>(), watched: OrderedDictionary<Instance, InstanceState>())) {
            (held: inout (listed: OrderedDictionary<Instance, InstanceState>, watched: OrderedDictionary<Instance, InstanceState>), change: Change) in
            held.watched.removeAll { _, state in state.is(\.exited) }
            switch change {
                case .listed(let listing): held.listed = listing
                case .watched(let instance, let state): held.watched[instance] = state
            }
        }
        return held.map { held in held.listed.merging(held.watched) { _, watched in watched } }.removeDuplicates()
    }

    public nonisolated func events(of matching: @escaping @Sendable (NSRunningApplication) -> Bool) -> some AsyncSequence<InstanceEvent, Never> & Sendable {
        follow(matching, prompts: true).compactMap { _, message in message[case: \.event] }
    }

    public nonisolated func observations(of matching: @escaping @Sendable (NSRunningApplication) -> Bool) -> some AsyncSequence<Watch.Report, Never> & Sendable {
        follow(matching, prompts: false).compactMap { _, message in message[case: \.observation] }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

@_silgen_name("CPSStealKeyFocusReturningID")
private func stealKeyFocus(_: Int32, _: UInt32, _: UnsafeMutablePointer<Int32>) -> CGError

@_silgen_name("CPSReleaseKeyFocusWithID")
private func releaseKeyFocus(_: Int32) -> CGError
