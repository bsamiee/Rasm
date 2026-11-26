public import AppKit
@unsafe @preconcurrency import ApplicationServices
import AsyncAlgorithms
public import CasePaths
import CasePathsCore
public import Encoding
import OSLog
public import OrderedCollections
public import Processes
import Results
import Synchronization
import System

// --- [CONSTANTS] -----------------------------------------------------------------------

private let notifications: [String] = [
    kAXMainWindowChangedNotification, kAXFocusedWindowChangedNotification, kAXFocusedUIElementChangedNotification, kAXWindowCreatedNotification, kAXWindowMovedNotification,
    kAXWindowResizedNotification, kAXWindowMiniaturizedNotification, kAXWindowDeminiaturizedNotification, kAXDrawerCreatedNotification, kAXSheetCreatedNotification,
    kAXHelpTagCreatedNotification, kAXValueChangedNotification, kAXUIElementDestroyedNotification, kAXElementBusyChangedNotification, kAXMenuOpenedNotification,
    kAXMenuClosedNotification, kAXMenuItemSelectedNotification, kAXRowCountChangedNotification, kAXRowExpandedNotification, kAXRowCollapsedNotification,
    kAXSelectedCellsChangedNotification, kAXUnitsChangedNotification, kAXSelectedChildrenMovedNotification, kAXSelectedChildrenChangedNotification, kAXResizedNotification,
    kAXMovedNotification, kAXCreatedNotification, kAXSelectedRowsChangedNotification, kAXSelectedColumnsChangedNotification, kAXSelectedTextChangedNotification,
    kAXTitleChangedNotification, kAXLayoutChangedNotification, kAXAnnouncementRequestedNotification, kAXLayoutCompleteNotification, kAXLoadCompleteNotification,
]
private let invalidating: Set<String> = [
    kAXWindowCreatedNotification, kAXDrawerCreatedNotification, kAXSheetCreatedNotification, kAXCreatedNotification, kAXUIElementDestroyedNotification, kAXLayoutChangedNotification,
    kAXRowCountChangedNotification, kAXRowExpandedNotification, kAXRowCollapsedNotification, kAXTitleChangedNotification, kAXWindowMiniaturizedNotification,
    kAXWindowDeminiaturizedNotification, kAXUnitsChangedNotification, kAXValueChangedNotification, kAXSelectedChildrenChangedNotification, kAXSelectedRowsChangedNotification,
    kAXSelectedCellsChangedNotification, kAXLayoutCompleteNotification, kAXLoadCompleteNotification,
]
private let moving: Set<String> = [kAXWindowMovedNotification, kAXWindowResizedNotification, kAXMovedNotification, kAXResizedNotification]
private let applicationAttributes: [String] = [kAXWindowsAttribute, kAXFocusedUIElementAttribute, kAXMenuBarAttribute, kAXExtrasMenuBarAttribute]
private let budget: (nodes: Int, time: Duration) = (4000, .seconds(3))
private let menuGap: Duration = .milliseconds(600)
let quiet: Duration = .milliseconds(100)
let pressOpened: Set<String> = [kAXMenuBarItemRole, kAXPopUpButtonRole, kAXMenuButtonRole]

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
    private let ticks: (stream: AsyncStream<Void>, continuation: AsyncStream<Void>.Continuation) = AsyncStream.makeStream(bufferingPolicy: .bufferingNewest(1))
    private let dirty: Mutex<[AXUIElement: Set<String>]> = Mutex([:])
    private let subscribers: Mutex<[UUID: AsyncStream<Message>.Continuation]?> = Mutex([:])
    private let windowTicks: (stream: AsyncStream<Void>, continuation: AsyncStream<Void>.Continuation) = AsyncStream.makeStream(bufferingPolicy: .bufferingNewest(1))
    private var observing: RunLoopSource?
    private var windowChanges: OrderedDictionary<CGWindowID, OrderedSet<WindowNotification>> = [:]
    private var rows: [InstanceState.Window] = []
    private var driven: Bool = false
    private var observations: [NSKeyValueObservation] = []
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
    private var activation: (active: Bool, hidden: Bool)
    private var sequence: Int = 0
    private var state: InstanceState
    private var faults: [(at: Date, line: Result<LogEntry, ProcessFailure>)] = []
    private var recording: Int = 0
    private var living: Task<Void, Never>?
    private var tasks: [Task<Void, Never>] = []
    private var openCommandDocuments: OrderedSet<URL> = []

    // --- [LIFECYCLE]

    init(_ instance: Instance) {
        let bundle: Bundle? = instance.application.bundleURL.flatMap(Bundle.init(url:))
        let preferred: [String]? = bundle?.bundleIdentifier.flatMap { identifier in CFPreferencesCopyAppValue("AppleLanguages" as CFString, identifier as CFString) as? [String] }
        let application: AXUIElement = AXUIElementCreateApplication(instance.process.pid)
        var stated: CFTypeRef?
        let spoken: String? = unsafe AXUIElementCopyAttributeValue(application, "AXPreferredLanguage" as CFString, &stated) == .success ? stated as? String : nil
        let callback: AXObserverCallbackWithInfo = { _, element, notification, _, refcon in
            guard let watch: Watch = unsafe refcon.map({ refcon in unsafe Unmanaged<Watch>.fromOpaque(refcon).takeUnretainedValue() }) else { return }
            watch.dirty.withLock { held in _ = held[element, default: []].insert(notification as String) }
            watch.ticks.continuation.yield()
        }
        var created: AXObserver?
        let answer: AXError = unsafe AXObserverCreateWithInfoCallback(instance.process.pid, callback, &created)
        let subject = String(instance.process.pid)
        self.instance = instance
        let localization: String? =
            spoken ?? bundle.flatMap { bundle in Bundle.preferredLocalizations(from: bundle.localizations, forPreferences: preferred).first ?? bundle.developmentLocalization }
        language = localization.map { localization in Locale.Language(identifier: Locale.canonicalLanguageIdentifier(from: localization)) } ?? Locale.current.language
        build = bundle?.object(forInfoDictionaryKey: kCFBundleVersionKey as String) as? String ?? ""
        applicationElement = application
        observer =
            if let created { .success(created) } else {
                .failure(ApplicationFailure(answer, subject: subject, process: instance.process.pid) { ApplicationFailure(subject: subject, reason: .accessibility(answer)) })
            }
        activation = (instance.application.isActive, instance.application.isHidden)
        state = .running(InstanceState.Activity(status: .launching, active: instance.application.isActive, windows: [], usage: nil, listening: .success([]), statusItems: nil))
    }

    func start(windows notices: AsyncStream<Result<WindowServerConnection.Notice, ApplicationFailure>>, ending: @escaping @Sendable () async -> Void) {
        let process: pid_t = instance.process.pid
        observing = observer[case: \.success].map { observer in RunLoopSource(AXObserverGetRunLoopSource(observer)) }
        observations = [\NSRunningApplication.isActive, \.isHidden].map { [ticks] property in instance.application.observe(property) { _, _ in ticks.continuation.yield() } }
        rows = InstanceState.Window.all(.optionAll, of: process)[process, default: []]
        register()
        switch observer {
            case .success: beat()
            case .failure(let failure): publish(.failed(failure))
        }
        let clock = ContinuousClock()
        let beats = AsyncTimerSequence(interval: messagingTimeout, clock: clock).map { _ in await self.beat() }
        tasks = [
            Task { [ticks] in
                for await _ in ticks.stream.debounce(for: quiet) { self.beat() }
            },
            Task {
                for await notice in notices { self.note(notice) }
            },
            Task { [windowTicks] in
                for await _ in windowTicks.stream { self.refresh() }
            },
        ]
        living = Task { [instance] in
            for await next in instance.lifetime(beats, clock: clock) where next.is(\.exited) { self.publish(next) }
            await self.close()
            await ending()
        }
    }

    private func register() {
        guard case .success(let observer) = observer else { return }
        let refcon = unsafe Unmanaged.passUnretained(self).toOpaque()
        unregistered = unregistered.filter { notification in unsafe AXObserverAddNotification(observer, applicationElement, notification as CFString, refcon) == .cannotComplete }
    }

    func drive() {
        driven = true
        tasks.append(
            Task { [instance] in
                for await line in LogEntry.stream(of: .process(instance.process.pid), levels: [.error, .fault]) where self.recording > 0 {
                    self.faults.append((line[case: \.success]?.date ?? .now, line))
                }
            }
        )
        do throws(ApplicationFailure) {
            let answer: AXError =
                try applicationElement.settable(manualAccessibilityAttribute)
                ? AXUIElementSetAttributeValue(applicationElement, manualAccessibilityAttribute as CFString, kCFBooleanTrue) : .success
            guard answer == .success else { throw applicationElement.failure(answer, reading: manualAccessibilityAttribute) }
        } catch {
            publish(.failed(error))
        }
    }

    private func note(_ notice: Result<WindowServerConnection.Notice, ApplicationFailure>) {
        switch notice {
            case .success(let notice):
                if let window = notice.window { windowChanges[window, default: []].append(notice.notification) }
                windowTicks.continuation.yield()
            case .failure(let failure): publish(.failed(failure))
        }
    }

    private func refresh() {
        let process: pid_t = instance.process.pid
        let changes: OrderedDictionary<CGWindowID, OrderedSet<WindowNotification>> = exchange(&windowChanges, with: [:])
        let blocking: Bool = state[case: \.running]?.status.is(\.unresponsive) == true
        let before: [CGWindowID: InstanceState.Window] = Dictionary(windows(blocking: blocking).map { row in (row.number, row) }) { first, _ in first }
        rows = InstanceState.Window.all(.optionAll, of: process)[process, default: []]
        let shown: [InstanceState.Window] = windows(blocking: blocking)
        let after: [CGWindowID: InstanceState.Window] = Dictionary(shown.map { row in (row.number, row) }) { first, _ in first }
        let events: [(change: WindowNotification, window: InstanceState.Window)] = changes.flatMap { number, notifications -> [(change: WindowNotification, window: InstanceState.Window)] in
            let row: InstanceState.Window? = after[number] ?? before[number]
            let leveled: Bool = row.flatMap { row in before[number].map { held in held.layer != row.layer } } == true
            return row.map { row in notifications.union(leveled ? [.levelChanged] : []).map { change in (change, row) } } ?? []
        }
        let reported = events.filter { event in
            switch (event.change, event.window.container == nil) {
                case (.created, true), (.destroyed, true), (.moved, true), (.resized, true), (.levelChanged, _), (.shown, _), (.hidden, _): true
                case (.created, false), (.destroyed, false), (.moved, false), (.resized, false), (.reordered, _), (.closed, _), (.activeSpaceChanged, _): false
            }
        }
        for event in reported { emit(.window(change: event.change, window: event.window)) }
        if case .running(let activity) = state {
            publish(
                .running(
                    InstanceState.Activity(
                        status: activity.status,
                        active: activity.active,
                        windows: shown,
                        usage: activity.usage,
                        listening: activity.listening,
                        statusItems: activity.statusItems,
                    )
                )
            )
        }
        if driven, changes.values.contains(where: { notifications in !notifications.isDisjoint(with: [.reordered, .shown, .created]) }) { repair() }
    }

    private func repair() {
        let process: pid_t = instance.process.pid
        guard !instance.application.isActive, let front = focusedProcesses().front, front != process else { return }
        let listed: [[String: Any]] = CGWindowListCopyWindowInfo([.optionOnScreenOnly, .excludeDesktopElements], kCGNullWindowID) as? [[String: Any]] ?? []
        let owners: [pid_t] = listed.compactMap { (row: [String: Any]) -> pid_t? in
            row[kCGWindowLayer as String] as? Int == InstanceState.Window.Band.normal.level ? row[kCGWindowOwnerPID as String] as? pid_t : nil
        }
        guard let fronted = owners.firstIndex(of: front), owners[..<fronted].contains(process) else { return }
        _ = instance.application.unhide()
    }

    func finish() async {
        guard let living else { return }
        living.cancel()
        await living.value
    }

    private func close() async {
        observing = nil
        observations = []
        ticks.continuation.finish()
        windowTicks.continuation.finish()
        let ending: [UUID: AsyncStream<Message>.Continuation] = subscribers.withLock { held in held.take() } ?? [:]
        for subscriber in ending.values { subscriber.finish() }
        let running: [Task<Void, Never>] = exchange(&tasks, with: [])
        for task in running { task.cancel() }
        for task in running { await task.value }
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
        guard !state.is(\.exited) || next.is(\.exited), next[case: \.failed]?.reason != .system(.noSuchProcess) else { return }
        let (before, after) = (state[case: \.running], next[case: \.running])
        if let after, after.status != before?.status { emit(.status(status: after.status)) }
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

    public func intern(_ locators: [Locator]) -> [Int] {
        let opened: [Locator] = open.values.map(\.locator)
        let documented: [Locator] = opened.filter { (held: Locator) -> Bool in held != held.stored }
        let documents: [Locator: [Locator]] = Dictionary(grouping: documented) { (held: Locator) -> Locator in held.stored }
        return locators.map { (locator: Locator) -> Int in
            let index: Int? = locator.steps.firstIndex { step in step.key.is(\.document) }
            let live: [Locator] = index.flatMap { index in documents[Locator(steps: Array(locator.steps[...index]))] } ?? []
            return intern(live.count == 1 ? Locator(steps: live[0].steps + locator.steps.dropFirst((index ?? 0) + 1)) : locator)
        }
    }

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
        case web

        var attributes: [String] {
            switch self {
                case .window: windowAttributes
                case .element: elementAttributes
                case .menu: menuAttributes
                case .web: elementAttributes + webAttributes
            }
        }
    }

    private func read(_ element: AXUIElement, _ attributes: [String]) -> (answer: AXError, element: Element) {
        let id = intern(element)
        let (answer, read) = element.element(id, reading: attributes, interning: intern)
        let visible: AXValue? = read.facts[kAXVisibleCharacterRangeAttribute]?.range.flatMap { range in
            var bounds = CFRange(location: range.location, length: range.length)
            return unsafe AXValueCreate(.cfRange, &bounds)
        }
        var text: CFTypeRef?
        guard read.role == kAXTextAreaRole, let visible,
            unsafe AXUIElementCopyParameterizedAttributeValue(element, kAXStringForRangeParameterizedAttribute as CFString, visible, &text) == .success, let shown = text as? String
        else { return (answer, read) }
        let facts: [String: Fact] = read.facts.merging([kAXValueAttribute: .text(value: shown)]) { _, shown in shown }
        return (answer, Element(id: id, facts: facts, actions: read.actions, window: read.window, descriptions: read.descriptions, counts: read.counts))
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
                    case (.some(let rows), true) where rows.multi == next.id: rows.ids + node.columns
                    case (_, true): node.children
                }
            let inner: Reach = next.reach == .menu ? .menu : next.reach == .web || node.role == NSAccessibility.Role.webAreaRole.rawValue ? .web : .element
            pending += children.reversed().map { child -> (id: Int, parent: Int?, reach: Reach, depth: Int) in (child, next.id, inner, next.depth - 1) }
            if let parent = node.facts[kAXParentAttribute]?.element, next.reach != .menu, parent != top, elements[parent] == nil { pending.append((parent, nil, .element, 0)) }
        }
        return (elements, [])
    }

    private func hierarchy(of app: Element, full: Set<Int>, menus: Bool = false, rows ranged: (multi: Int, ids: [Int])? = nil) throws(ApplicationFailure) -> Hierarchy {
        let windows: [Int] = app.facts[kAXWindowsAttribute]?.elements ?? []
        let bars: [Int] = menus ? [kAXMenuBarAttribute, kAXExtrasMenuBarAttribute].compactMap { attribute in app.facts[attribute]?.element } : []
        let starts: [(id: Int, reach: Reach, depth: Int)] =
            bars.map { bar in (id: bar, reach: Reach.menu, depth: 2) } + windows.map { window in (id: window, reach: Reach.window, depth: full.contains(window) ? Int.max : 0) }
        let (elements, truncated) = try walk(starts, rows: ranged)
        return Hierarchy(
            elements: Array(elements.values),
            roots: windows + bars,
            windows: self.windows(blocking: false),
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
            emit(
                .asking(
                    container: ref,
                    kind: container.kind,
                    title: container.title,
                    text: container.text,
                    answers: answers,
                    default: container.default,
                    cancel: container.cancel,
                    close: container.close,
                )
            )
        }
        open.merge(fresh) { _, read in read }
        bars = menuBars.isEmpty ? bars : menuBars
        for container in fresh.values + menuBars { try observe(container, opener: nil) }
        return Model(sequence: sequence, containers: read.containers + open.values.filter { container in container.kind == .menu }, located: read.located)
    }

    private func observe(_ container: Container, opener: (control: Control, container: Int)?) throws(ApplicationFailure) {
        guard let observation = try Observation(container, in: open, build: build, opener: opener, after: sent[container.ref], at: .now, locator: { ref in locators[ref - 1] }) else { return }
        sent[container.ref] = observation
        send(.observation(Report(instance: instance.process, identifier: instance.application.bundleIdentifier, language: language.minimalIdentifier, observation: observation)))
    }

    private func reconcile(_ marked: [AXUIElement: Set<String>]) throws(ApplicationFailure) {
        let app = try application()
        register()
        for (element, names) in marked where !names.isDisjoint(with: moving) {
            guard let ref = table[element].flatMap({ id in owners[id] }), let container = open[ref], let frame = read(element, frameAttributes).element.frame else { continue }
            emit(.moved(container: ref, kind: container.kind, frame: frame))
        }
        for (element, names) in marked where names.contains(kAXMenuClosedNotification) {
            guard let ref = table[element].flatMap({ id in owners[id] }), let menu = open[ref], menu.kind == .menu else { continue }
            emit(.closed(container: ref, kind: .menu, title: menu.title))
            (sent[ref], open[ref]) = (nil, nil)
        }
        for (element, names) in marked where names.contains(kAXMenuOpenedNotification) && !names.contains(kAXMenuClosedNotification) { try track(element) }
        if let element = app.facts[kAXFocusedUIElementAttribute]?.element, element != focused {
            focused = element
            if let control = owners[element], let held = controls[control] { emit(.focused(control: control, kind: held.control.kind)) }
        }
        let touched: Set<Int> = Set(
            marked.filter { _, names in !names.isDisjoint(with: invalidating) }.keys.compactMap { (element: AXUIElement) -> Int? in
                table[element].flatMap { id in owners[id] }.flatMap { ref in root(of: locators[ref - 1]) }
                    ?? read(element, [kAXTopLevelUIElementAttribute]).element.facts[kAXTopLevelUIElementAttribute]?.element
            }
        )
        let windows: Set<Int> = Set(app.facts[kAXWindowsAttribute]?.elements ?? [])
        let known: Set<Int> = Set(open.values.filter { container in container.parent == nil && container.kind != .menu }.compactMap { container in located[container.ref] })
        if windows != known || !touched.isEmpty {
            let full: Set<Int> = touched.union(windows.subtracting(known))
            _ = try merged(try hierarchy(of: app, full: full), full: full)
        }
        for (element, names) in marked where names.contains(kAXUIElementDestroyedNotification) {
            if let id = table.removeValue(forKey: element) { handles[id] = nil }
        }
    }

    @discardableResult
    func beat() -> InstanceState {
        let subject = String(instance.process.pid)
        let marked: [AXUIElement: Set<String>] = dirty.withLock { held in exchange(&held, with: [:]) }
        let now: (active: Bool, hidden: Bool) = (instance.application.isActive, instance.application.isHidden)
        if now.active != activation.active { emit(.frontmost(active: now.active)) }
        if now.hidden != activation.hidden { emit(.hidden(hidden: now.hidden)) }
        activation = now
        if marked.values.contains(where: { names in names.contains(kAXTitleChangedNotification) }) { windowTicks.continuation.yield() }
        let read = Result { () throws(ApplicationFailure) -> InstanceState.Activity in
            let thread: MainThread
            do throws(Errno) { thread = try MainThread(of: instance.process) } catch { throw ApplicationFailure(subject: subject, reason: .system(error)) }
            let stopped: Bool = [.stopped, .traced].contains(thread.state)
            let after: MainThread?
            do throws(ApplicationFailure) {
                if !stopped, instance.application.activationPolicy != .prohibited || !marked.isEmpty { try reconcile(marked) }
                after = nil
            } catch  where error.reason.is(\.unanswered) {
                do throws(Errno) { after = try MainThread(of: instance.process) } catch { throw ApplicationFailure(subject: subject, reason: .system(error)) }
            }
            let ready: Bool = instance.application.isFinishedLaunching || open.values.contains(where: \.asking)
            let status: InstanceState.Activity.Status =
                switch (stopped, after, ready) {
                    case (true, _, _): .unresponsive(thread: thread.state)
                    case (false, .some(let after), true): .unresponsive(thread: after.status(since: thread))
                    case (false, _, false): .launching
                    case (false, .none, true): .responding
                }
            let blocked: Bool = status.is(\.unresponsive)
            let usage: Result<ResourceUsage, Errno>? = blocked ? state[case: \.running]?.usage ?? Result { () throws(Errno) in try ResourceUsage(of: instance.process) } : nil
            let listening = Result { () throws(Errno) in try instance.process.listeningPorts() }
            return InstanceState.Activity(status: status, active: activation.active, windows: windows(blocking: blocked), usage: usage, listening: listening, statusItems: nil)
        }
        switch read {
            case .success(let activity): publish(.running(activity))
            case .failure(let failure): publish(.failed(failure))
        }
        return state
    }

    private func windows(blocking: Bool) -> [InstanceState.Window] {
        let byWindow: [CGWindowID: Container] = Dictionary(
            open.values.compactMap { container in container.parent == nil ? container.window.map { window in (window, container) } : nil }
        ) { first, _ in first }
        return rows.map { row in row.described(by: byWindow[row.number], blocking: blocking && row.onScreen && row.band == .modalPanel) }
    }

    // --- [MODEL]

    public func model(_ scope: Model.Scope, known: Model.Known? = nil, clock: some Clock<Duration> = ContinuousClock()) async throws(ApplicationFailure) -> Model {
        switch scope {
            case .application:
                let verified: Set<String> = known?.build == build ? known?.containers ?? [] : []
                var menus: Bool = bars.isEmpty
                for bar in bars where !verified.contains(try bar.locator.text()) { menus = true }
                let walk = { () throws(ApplicationFailure) -> (Hierarchy, Set<Int>) in
                    let app: Element = try self.application()
                    var kept: Set<Int> = []
                    for container in self.open.values where container.parent == nil {
                        if verified.contains(try container.locator.text()), let element = self.located[container.ref] { kept.insert(element) }
                    }
                    let windows: Set<Int> = Set(app.facts[kAXWindowsAttribute]?.elements ?? []).subtracting(kept)
                    return (try self.hierarchy(of: app, full: windows, menus: menus), kept)
                }
                let (walked, kept): (Hierarchy, Set<Int>) = if menus { try await spaced(clock: clock, walk) } else { try walk() }
                var read: Hierarchy = walked
                read.recognized = try await recognizeText(in: read.flyouts.map(\.number), labels: known?.labels ?? [])
                let model: Model = try merged(read, full: Set(read.roots).subtracting(kept))
                let fresh: [Container] = model.containers.filter { (container: Container) -> Bool in root(of: container.locator).map(kept.contains) != true }
                let held: [Container] = open.values.filter { (container: Container) -> Bool in container.kind != .menu && root(of: container.locator).map(kept.contains) == true }
                let containers: [Container] = fresh + held
                return Model(sequence: model.sequence, containers: containers, located: model.located, pointer: Pointer(of: instance.application, windows: read.windows, containers: containers))
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
            case .text(let ref):
                guard let id = located[ref], let element = handles[id], let held = controls[ref], let root = root(of: held.control.locator) else { throw stale(ref) }
                var read = try hierarchy(of: try application(), full: [root])
                read.runs[id] = try element.runs(interning: intern)
                let model = Model(read, sequence: sequence, ref: intern)
                return Model(sequence: sequence, containers: model.containers.filter { container in container.ref == held.container }, located: model.located)
            case .search(let text, let key, let limit):
                let found: Set<Int> = Set(try applicationElement.search(text, key: key, limit: limit).map(intern))
                let roots: Set<Int> = Set(
                    found.compactMap { (id: Int) -> Int? in
                        owners[id].flatMap { ref in root(of: locators[ref - 1]) }
                            ?? handles[id].flatMap { hit in read(hit, [kAXTopLevelUIElementAttribute]).element.facts[kAXTopLevelUIElementAttribute]?.element }
                    }
                )
                let model = try merged(try hierarchy(of: try application(), full: roots), full: roots)
                let hits: Set<Int> = Set(model.located.compactMap { ref, element in found.contains(element) ? ref : nil })
                return Model(
                    sequence: model.sequence,
                    containers: model.containers.filter { container in hits.contains(container.ref) || container.controls.contains { control in hits.contains(control.ref) } },
                    located: model.located,
                )
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
            try observe(held, opener: opener)
        }
        return level
    }

    // --- [MENUS]

    public enum Presentation: CaseTagged, Comparable, Sendable {
        case behind
        case forward
    }

    private func track(_ menu: AXUIElement) throws(ApplicationFailure) {
        let head: Element = read(menu, frameAttributes + [kAXParentAttribute, kAXTopLevelUIElementAttribute]).element
        guard let frame = head.frame, !frame.isEmpty else { return }
        let row: InstanceState.Window? = rows.filter { row in row.onScreen && row.band == .popUpMenu && row.frame.contains(frame) }
            .min { first, second in first.frame.width * first.frame.height < second.frame.width * second.frame.height }
        let opener: Int? = [kAXParentAttribute, kAXTopLevelUIElementAttribute].compactMap { attribute in head.facts[attribute]?.element.flatMap { id in owners[id] } }.first
        let (elements, truncated) = try walk([(id: head.id, reach: .menu, depth: 2)], rows: nil)
        let shown: [Element] = elements.values.map { element in
            element.id == head.id
                ? Element(id: element.id, facts: element.facts, actions: element.actions, window: row?.number, descriptions: element.descriptions, counts: element.counts) : element
        }
        let hierarchy = Hierarchy(elements: shown, roots: [head.id], windows: row.map { row in [row] } ?? [], regular: true, focused: nil, truncated: truncated, extras: nil)
        let model = Model(hierarchy, sequence: sequence, under: opener.map { ref in [head.id: locators[ref - 1]] } ?? [:], ref: intern)
        guard let container = model.containers.first(where: { container in container.kind == .menu }), open[container.ref] == nil else { return }
        locate(model)
        open[container.ref] = container
        emit(.opened(container: container.ref, kind: .menu, title: container.title))
        try observe(container, opener: opener.flatMap { ref in controls[ref] })
    }

    public nonisolated func presenting<Value>(
        _ ref: Int?,
        by opening: String? = nil,
        clock: some Clock<Duration> = ContinuousClock(),
        _ body: ([Container]) async throws(ApplicationFailure) -> Value,
    ) async throws(ApplicationFailure) -> (value: Value, presentation: Presentation?) {
        guard let ref else { return (try await body([]), nil) }
        let process: pid_t = instance.process.pid
        let held: OrderedSet<Locator> = await locators
        let openers: Int = held.indices.contains(ref - 1) ? held[ref - 1].steps.count : 0
        let steps: Int = 3 * openers + 2
        let lifetime: Duration = messagingTimeout * steps
        var levels: [(menu: Container, presentation: Presentation?)] = []
        let value: Result<Value, ApplicationFailure> = try await PermittedFrontList.shared.holding(
            excluding: { application in application.processIdentifier == process },
            for: lifetime,
            clock: clock,
            { () async throws(ApplicationFailure) -> Value in
                while let level = try await self.popUp(toward: ref, by: opening, past: levels.count, clock: clock) { levels.append(level) }
                let menus: [Container] = levels.map(\.menu)
                return try await body(menus)
            },
        )
        let opened: Int? = levels.first { level in level.presentation != nil }?.menu.ref
        let closed: Result<Void, ApplicationFailure> = await withTaskCancellationShield { await Result { () async throws(ApplicationFailure) in try await self.cancel(opened, clock: clock) } }
        let presented: Value = try value.get()
        try closed.get()
        return (presented, levels.compactMap(\.presentation).max())
    }

    private func popUp(toward ref: Int, by opening: String?, past count: Int, clock: some Clock<Duration>) async throws(ApplicationFailure) -> (menu: Container, presentation: Presentation?)? {
        guard locators.indices.contains(ref - 1) else { throw stale(ref) }
        let steps: [Locator.Step] = locators[ref - 1].steps
        let prefixes: [(ref: Int?, inner: Bool)] = steps.indices.map { index in
            (locators.firstIndex(of: Locator(steps: Array(steps[...index]))).map { position in position + 1 }, index < steps.count - 1)
        }
        let openers: [(control: Control, action: String?)] = prefixes.compactMap { (prefix: Int?, inner: Bool) -> (control: Control, action: String?)? in
            switch (prefix.flatMap { prefix in controls[prefix]?.control }, inner, opening) {
                case (.some(let control), false, .some(let action)): (control, action)
                case (.some(let control), _, _) where control.opens != nil || pressOpened.contains(control.role): (control, kAXPressAction)
                case (.some(let control), true, _): (control, nil)
                case (.some, false, .none), (.none, _, _): nil
            }
        }
        guard openers.indices.contains(count) else { return nil }
        let opener: Control = openers[count].control
        if let level = open.values.first(where: { menu in menu.kind == .menu && menu.locator.steps.dropLast().elementsEqual(opener.locator.steps) }) { return (level, nil) }
        let element: AXUIElement = try await resolve(opener.ref)
        let shown: Element = read(element, frameAttributes).element
        guard let action = openers[count].action ?? shown.actions.first(where: { action in action == kAXShowMenuAction }) else {
            throw ApplicationFailure(subject: "\(opener.ref), which lists neither \(kAXPressAction) on a menu nor \(kAXShowMenuAction)", reason: .refused)
        }
        var unopened: [String] = []
        for forward in [false, true] where forward ? !instance.application.isActive : shown.frame?.isEmpty == false {
            if forward {
                try PermittedFrontList.shared.admit(instance.process.pid).get()
                let activations = messages()
                let raised = AXUIElementSetAttributeValue(applicationElement, kAXFrontmostAttribute as CFString, kCFBooleanTrue)
                let activated: Void? = try await next({ (change: InstanceEvent.Change) -> Void? in change[case: \.frontmost] == true ? () : nil }, in: activations, clock: clock)
                guard raised == .success, activated != nil else {
                    let subject = "\(kAXFrontmostAttribute) of process \(instance.process.pid)"
                    throw raised == .success
                        ? ApplicationFailure(subject: subject, reason: .unseen)
                        : ApplicationFailure(raised, subject: subject, process: instance.process.pid) { ApplicationFailure(subject: subject, reason: .accessibility(raised)) }
                }
            }
            let events = messages()
            AXUIElementSetMessagingTimeout(element, Float(messagingTimeout / 2 / .seconds(1)))
            let answer = AXUIElementPerformAction(element, action as CFString)
            AXUIElementSetMessagingTimeout(element, Float(messagingTimeout / .seconds(1)))
            let opened: Result<Int?, ApplicationFailure> =
                if [.success, .cannotComplete].contains(answer) {
                    await Result { () async throws(ApplicationFailure) -> Int? in
                        try await next(
                            { (change: InstanceEvent.Change) -> Int? in change[case: \.opened].flatMap { opened in opened.kind == .menu ? opened.container : nil } },
                            in: events,
                            clock: clock,
                        )
                    }
                } else {
                    .failure(ApplicationFailure(subject: "\(action) on \(opener.ref)", reason: .accessibility(answer)))
                }
            guard let level = try opened.get().flatMap({ menu in open[menu] }) else {
                let cancelled: AXError = AXUIElementPerformAction(element, kAXCancelAction as CFString)
                unopened.append("\(forward ? Presentation.forward : .behind) with \(kAXCancelAction) answering AXError \(cancelled.rawValue)")
                continue
            }
            return (level, forward ? .forward : .behind)
        }
        let subject: String =
            unopened.isEmpty ? "\(action) on \(opener.ref), which shows no frame in the active application" : "\(action) on \(opener.ref), which opened no menu \(unopened.formatted())"
        throw ApplicationFailure(subject: subject, reason: .noWindow)
    }

    private func cancel(_ level: Int?, clock: some Clock<Duration>) async throws(ApplicationFailure) {
        guard let level, open[level] != nil, let element = located[level].flatMap({ id in handles[id] }) else { return }
        let events = messages()
        let answer = AXUIElementPerformAction(element, kAXCancelAction as CFString)
        guard try await next({ (change: InstanceEvent.Change) -> Void? in change[case: \.closed]?.container == level ? () : nil }, in: events, clock: clock) != nil else {
            let subject = "\(kAXCancelAction) on \(level)"
            throw answer == .success
                ? ApplicationFailure(subject: subject, reason: .unseen)
                : ApplicationFailure(answer, subject: subject, process: instance.process.pid) { ApplicationFailure(subject: subject, reason: .accessibility(answer)) }
        }
    }

    nonisolated func next<Matched: Sendable>(
        _ change: @escaping @Sendable (InstanceEvent.Change) -> Matched?,
        in events: AsyncStream<Message>,
        clock: some Clock<Duration>,
    ) async throws(ApplicationFailure) -> Matched? {
        let subject = String(instance.process.pid)
        return try await race(
            { () async throws(ApplicationFailure) -> Matched? in
                let matched: Matched? = await events.compactMap { (message: Message) -> Matched? in message[case: \.event].flatMap { event in change(event.change) } }.first { _ in true }
                guard let matched else { throw ApplicationFailure(subject: subject, reason: Task.isCancelled ? .cancelled : .system(.noSuchProcess)) }
                return matched
            },
            against: { () async throws(ApplicationFailure) -> Matched? in
                do { try await clock.sleep(for: messagingTimeout) } catch { throw ApplicationFailure(subject: subject, reason: .cancelled) }
                return nil
            },
        )
    }

    // --- [ACTIONS]

    public func perform<Value: Sendable>(on ref: Int?, _ body: @Sendable (AXUIElement?) throws(ApplicationFailure) -> Value) async throws(ApplicationFailure) -> Value {
        let element: AXUIElement? = if let ref { try await resolve(ref) } else { nil }
        return try body(element)
    }

    private func resolve(_ ref: Int) async throws(ApplicationFailure) -> AXUIElement {
        if located[ref].flatMap({ id in handles[id] }) == nil {
            let within: Int? = controls[ref]?.container ?? open.values.last { container in locators.indices.contains(ref - 1) && locators[ref - 1].steps.starts(with: container.locator.steps) }?.ref
            _ = try await model(.container(ref: within ?? ref))
        }
        guard let element = located[ref].flatMap({ id in handles[id] }) else { throw stale(ref) }
        return element
    }

    public func frame(of ref: Int) async throws(ApplicationFailure) -> (window: CGWindowID, frame: CGRect) {
        if let shown = drawn[ref] {
            guard rows.contains(where: { row in row.number == shown.window && row.onScreen }) else { throw stale(ref) }
            return shown
        }
        let element = try await resolve(ref)
        guard let frame = read(element, frameAttributes).element.frame else { throw ApplicationFailure(subject: "\(frameAttribute) of \(ref)", reason: .accessibility(.noValue)) }
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
        case listed(Result<OrderedDictionary<Instance, InstanceState>, ApplicationFailure>)
        case watched(instance: Instance, state: InstanceState)
    }

    private var held: [ProcessInstance: (watch: Watch, driven: Bool)] = [:]
    private let followers: Mutex<[UUID: AsyncStream<(watch: Watch, driven: Bool)>.Continuation]> = Mutex([:])
    private let windowServer: Result<WindowServerConnection, ApplicationFailure>

    public init() {
        AXUIElementSetMessagingTimeout(AXUIElementCreateSystemWide(), Float(messagingTimeout / .seconds(1)))
        windowServer = Result { () throws(ApplicationFailure) in try WindowServerConnection() }
    }

    private nonisolated func notices(of notifications: Set<WindowNotification>, from owner: pid_t? = nil) -> AsyncStream<Result<WindowServerConnection.Notice, ApplicationFailure>> {
        switch windowServer {
            case .success(let connection): connection.notices(of: notifications, from: owner)
            case .failure(let failure):
                AsyncStream { continuation in
                    continuation.yield(.failure(failure))
                    continuation.finish()
                }
        }
    }

    public func watch(_ instance: Instance, driven: Bool = true) async -> Watch {
        let before: (watch: Watch, driven: Bool)? = held[instance.process]
        let after: (watch: Watch, driven: Bool) = (before?.watch ?? Watch(instance), driven || before?.driven == true)
        held[instance.process] = after
        if before == nil { await after.watch.start(windows: notices(of: Set(WindowNotification.allCases), from: instance.process.pid)) { await self.remove(instance.process) } }
        if after.driven, before?.driven != true { await after.watch.drive() }
        if before?.driven != after.driven {
            followers.withLock { following in
                for follower in following.values { follower.yield(after) }
            }
        }
        return after.watch
    }

    public func finish() async {
        let watching: [Watch] = held.values.map(\.watch)
        await withDiscardingTaskGroup { group in
            for watch in watching { group.addTask { await watch.finish() } }
        }
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

    private nonisolated func follow(
        _ matching: @escaping @Sendable (NSRunningApplication) -> Bool,
        prompts: Bool,
    ) -> AsyncStream<Result<(instance: Instance, message: Watch.Message), ApplicationFailure>> {
        let (output, delivery) = AsyncStream<Result<(instance: Instance, message: Watch.Message), ApplicationFailure>>.makeStream()
        let raising: AsyncStream<Result<WindowServerConnection.Notice, ApplicationFailure>> =
            prompts ? notices(of: [.created, .levelChanged, .shown]) : AsyncStream { continuation in continuation.finish() }
        let raisedByAgents = raising.filter { notice in
            guard case .success(let notice) = notice else { return true }
            return notice.owner.flatMap(NSRunningApplication.init(processIdentifier:)).map { owner in owner.activationPolicy != .regular } == true
        }
        let scans = chain([Result<Void, ApplicationFailure>.success(())].async.prefix(prompts ? 1 : 0), raisedByAgents.map { notice in notice.map { _ in } })
        let raised = scans.compactMap { (scan: Result<Void, ApplicationFailure>) -> [Instance]? in
            if case .failure(let failure) = scan { delivery.yield(.failure(failure)) }
            guard case .success = scan else { return nil }
            let owners: Set<pid_t> = Set(
                InstanceState.Window.all([.optionOnScreenOnly, .excludeDesktopElements]).filter { _, windows in windows.contains { window in window.band == .modalPanel } }.keys
            )
            let listed: Result<[Instance], ApplicationFailure> = Result { () throws(ApplicationFailure) in
                try NSWorkspace.shared.instances { application in owners.contains(application.processIdentifier) && application.activationPolicy != .regular }
            }
            if case .failure(let failure) = listed { delivery.yield(.failure(failure)) }
            return listed[case: \.success]
        }
        let following = Task {
            let requested = await self.watched().filter { held in (prompts || held.driven) && matching(held.watch.instance.application) }.map(\.watch)
            let prompting = raised.flatMap(\.async).map { instance in await self.watch(instance, driven: false) }
            await withDiscardingTaskGroup { group in
                var followed: Set<ProcessInstance> = []
                for await watch in merge(requested, prompting) where followed.insert(watch.instance.process).inserted {
                    group.addTask { for await message in await watch.messages() { delivery.yield(.success((watch.instance, message))) } }
                }
            }
        }
        delivery.onTermination = { _ in following.cancel() }
        return output
    }

    public nonisolated func states(
        of matching: @escaping @Sendable (NSRunningApplication) -> Bool
    ) -> some AsyncSequence<Result<OrderedDictionary<Instance, InstanceState>, ApplicationFailure>, Never> & Sendable {
        let (changes, changed) = AsyncStream<Void>.makeStream(bufferingPolicy: .bufferingNewest(1))
        let observations: [NSKeyValueObservation] = [
            NSWorkspace.shared.observe(\.runningApplications, options: [.initial]) { _, _ in changed.yield() },
            NSWorkspace.shared.observe(\.frontmostApplication) { _, _ in changed.yield() },
        ]
        nonisolated(unsafe) let refcon: Unmanaged<AnyObject> = unsafe Unmanaged.passRetained(changed as AnyObject)
        let hosts: [AXObserver] = Set(InstanceState.Activity.statusBars(in: InstanceState.Window.all(.optionAll)).map(\.host)).compactMap { host in
            var created: AXObserver?
            let answer: AXError = unsafe AXObserverCreate(
                host,
                { _, _, _, refcon in unsafe (refcon.map { refcon in unsafe Unmanaged<AnyObject>.fromOpaque(refcon).takeUnretainedValue() } as? AsyncStream<Void>.Continuation)?.yield() },
                &created,
            )
            guard answer == .success, let observer = created else { return nil }
            let application: AXUIElement = AXUIElementCreateApplication(host)
            for notification in [kAXLayoutChangedNotification, kAXUIElementDestroyedNotification] {
                _ = unsafe AXObserverAddNotification(observer, application, notification as CFString, refcon.toOpaque())
            }
            CFRunLoopAddSource(CFRunLoopGetMain(), AXObserverGetRunLoopSource(observer), .commonModes)
            return observer
        }
        changed.onTermination = { _ in
            for observation in observations { observation.invalidate() }
            for observer in hosts { CFRunLoopSourceInvalidate(AXObserverGetRunLoopSource(observer)) }
            unsafe refcon.release()
        }
        let windowed = notices(of: Set(WindowNotification.allCases).subtracting([.reordered, .closed])).debounce(for: quiet).map { notice in notice.map { _ in false } }
        let triggers = merge(changes.map { () -> Result<Bool, ApplicationFailure> in .success(true) }, windowed)
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
                                active: instance.application.isActive,
                                windows: windows[instance.process.pid, default: []],
                                usage: nil,
                                listening: Result { () throws(Errno) in try instance.process.listeningPorts() },
                                statusItems: counts.map { counts in counts[instance.process.pid, default: 0] },
                            )
                            return (instance, .running(activity))
                        }
                    )
                }
            )
        }
        .map(\.change)
        let driven = follow(matching, prompts: false).compactMap { (followed: Result<(instance: Instance, message: Watch.Message), ApplicationFailure>) -> Change? in
            guard case .success(let followed) = followed else { return nil }
            return followed.message[case: \.state].map { state in .watched(instance: followed.instance, state: state) }
        }
        let seed: (listed: Result<OrderedDictionary<Instance, InstanceState>, ApplicationFailure>, watched: OrderedDictionary<Instance, InstanceState>) = (.success([:]), [:])
        let held = merge(listed, driven).reductions(into: seed) {
            (held: inout (listed: Result<OrderedDictionary<Instance, InstanceState>, ApplicationFailure>, watched: OrderedDictionary<Instance, InstanceState>), change: Change) in
            held.watched.removeAll { _, state in state.is(\.exited) }
            switch change {
                case .listed(let listing): held.listed = listing
                case .watched(let instance, let state): held.watched[instance] = state
            }
        }
        return held.map { held in
            held.listed.map { listing in
                listing.merging(held.watched) { listed, watched in
                    guard let activity = watched[case: \.running] else { return watched }
                    let counted = InstanceState.Activity(
                        status: activity.status,
                        active: activity.active,
                        windows: activity.windows,
                        usage: activity.usage,
                        listening: activity.listening,
                        statusItems: listed[case: \.running]?.statusItems,
                    )
                    return .running(counted)
                }
            }
        }
        .removeDuplicates()
    }

    public nonisolated func events(of matching: @escaping @Sendable (NSRunningApplication) -> Bool) -> some AsyncSequence<Result<InstanceEvent, ApplicationFailure>, Never> & Sendable {
        follow(matching, prompts: true).compactMap { (followed: Result<(instance: Instance, message: Watch.Message), ApplicationFailure>) -> Result<InstanceEvent, ApplicationFailure>? in
            switch followed {
                case .success(let followed): followed.message[case: \.event].map(Result.success)
                case .failure(let failure): .failure(failure)
            }
        }
    }

    public nonisolated func observations(of matching: @escaping @Sendable (NSRunningApplication) -> Bool) -> some AsyncSequence<Watch.Report, Never> & Sendable {
        follow(matching, prompts: false).compactMap { (followed: Result<(instance: Instance, message: Watch.Message), ApplicationFailure>) -> Watch.Report? in
            guard case .success(let followed) = followed else { return nil }
            return followed.message[case: \.observation]
        }
    }
}
