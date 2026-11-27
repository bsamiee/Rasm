package import AppKit
@unsafe @preconcurrency import ApplicationServices
import AsyncAlgorithms
import CasePaths
import CasePathsCore
import Encoding
import OSLog
package import OrderedCollections
package import Processes
import Results
import SPI
import Synchronization
import System

// --- [CONSTANTS] -----------------------------------------------------------------------

private let notifications: [String] =
    [
        kAXMainWindowChangedNotification, kAXFocusedWindowChangedNotification, kAXFocusedUIElementChangedNotification, kAXWindowCreatedNotification, kAXWindowMovedNotification,
        kAXWindowResizedNotification, kAXWindowMiniaturizedNotification, kAXWindowDeminiaturizedNotification, kAXDrawerCreatedNotification, kAXSheetCreatedNotification,
        kAXHelpTagCreatedNotification, kAXValueChangedNotification, kAXUIElementDestroyedNotification, kAXElementBusyChangedNotification, kAXMenuOpenedNotification,
        kAXMenuClosedNotification, kAXMenuItemSelectedNotification, kAXRowCountChangedNotification, kAXRowExpandedNotification, kAXRowCollapsedNotification,
        kAXSelectedCellsChangedNotification, kAXUnitsChangedNotification, kAXSelectedChildrenMovedNotification, kAXSelectedChildrenChangedNotification, kAXResizedNotification,
        kAXMovedNotification, kAXCreatedNotification, kAXSelectedRowsChangedNotification, kAXSelectedColumnsChangedNotification, kAXSelectedTextChangedNotification,
        kAXTitleChangedNotification, kAXLayoutChangedNotification, kAXAnnouncementRequestedNotification, kAXLayoutCompleteNotification, kAXLoadCompleteNotification,
    ] + dragging.elements
private let dragging: OrderedSet<String> = OrderedSet(
    [
        "AXDraggingSourceDragBegan", "AXDraggingSourceDragEnded", "AXDraggingDestinationDropAllowed", "AXDraggingDestinationDropNotAllowed", "AXDraggingDestinationDragAccepted",
        "AXDraggingDestinationDragNotAccepted",
    ]
)
private let invalidating: Set<String> = [
    kAXWindowCreatedNotification, kAXDrawerCreatedNotification, kAXSheetCreatedNotification, kAXCreatedNotification, kAXUIElementDestroyedNotification, kAXLayoutChangedNotification,
    kAXRowCountChangedNotification, kAXRowExpandedNotification, kAXRowCollapsedNotification, kAXTitleChangedNotification, kAXWindowMiniaturizedNotification,
    kAXWindowDeminiaturizedNotification, kAXUnitsChangedNotification, kAXSelectedChildrenChangedNotification, kAXSelectedRowsChangedNotification,
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

package actor Watch {
    // --- [STATE]

    package nonisolated let instance: Instance
    package nonisolated let language: Locale.Language
    private let queue: DispatchSerialQueue
    private let serial: ProcessSerialNumber?
    private let build: String?
    private let applicationElement: AXUIElement
    private let observer: Result<AXObserver, ApplicationFailure>
    private let ticks: (stream: AsyncStream<Void>, continuation: AsyncStream<Void>.Continuation) = AsyncStream.makeStream(bufferingPolicy: .bufferingNewest(1))
    private let marked: Mutex<[AXUIElement: Set<String>]> = Mutex([:])
    private let dragged: Mutex<[(element: AXUIElement, notification: String)]> = Mutex([])
    private let noticed: Mutex<(announced: [(text: String, priority: Lenient<AXPriority>?)], openings: [UUID: AsyncStream<Void>.Continuation])> = Mutex(([], [:]))
    private let subscribers: Mutex<[UUID: AsyncStream<Message>.Continuation]?> = Mutex([:])
    private let windowTicks: (stream: AsyncStream<Void>, continuation: AsyncStream<Void>.Continuation) = AsyncStream.makeStream(bufferingPolicy: .bufferingNewest(1))
    private var observing: RunLoopSource?
    private var postponed: [AXUIElement: Set<String>] = [:]
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
    private var living: Task<Void, Never>?
    private var tasks: [Task<Void, Never>] = []
    private var openCommandDocuments: OrderedSet<URL> = []

    // --- [LIFECYCLE]

    init(_ instance: Instance) {
        let bundle: Bundle? = instance.application.bundleURL.flatMap(Bundle.init(url:))
        let preferred: [String]? = bundle?.bundleIdentifier.flatMap { identifier in CFPreferencesCopyAppValue("AppleLanguages" as CFString, identifier as CFString) as? [String] }
        let application: AXUIElement = AXUIElementCreateApplication(instance.process.pid)
        var stated: CFTypeRef?
        let spoken: String? = unsafe AXUIElementCopyAttributeValue(application, preferredLanguageAttribute as CFString, &stated) == .success ? stated as? String : nil
        let callback: AXObserverCallbackWithInfo = { _, element, notification, info, refcon in
            guard let watch: Watch = unsafe refcon.map({ refcon in unsafe Unmanaged<Watch>.fromOpaque(refcon).takeUnretainedValue() }) else { return }
            let name = notification as String
            let fields: [String: Any] = name == kAXAnnouncementRequestedNotification ? info as? [String: Any] ?? [:] : [:]
            let announcement: String? = (fields[kAXAnnouncementKey] as? NSAttributedString)?.string ?? (fields[kAXAnnouncementKey] as? String)
            let priority: Lenient<AXPriority>? = (fields[kAXPriorityKey] as? Int).map(Lenient<AXPriority>.init(rawValue:))
            watch.marked.withLock { marked in _ = marked[element, default: []].insert(name) }
            if dragging.contains(name) { watch.dragged.withLock { dragged in dragged.append((element, name)) } }
            watch.noticed.withLock { noticed in
                noticed.announced += announcement.map { text in [(text, priority)] } ?? []
                for opening in noticed.openings.values where name == kAXMenuOpenedNotification { opening.yield() }
            }
            watch.ticks.continuation.yield()
        }
        var created: AXObserver?
        let answer: AXError = unsafe AXObserverCreateWithInfoCallback(instance.process.pid, callback, &created)
        let subject = String(instance.process.pid)
        self.instance = instance
        let localization: String? =
            spoken ?? bundle.flatMap { bundle in Bundle.preferredLocalizations(from: bundle.localizations, forPreferences: preferred).first ?? bundle.developmentLocalization }
        language = localization.map { localization in Locale.Language(identifier: Locale.canonicalLanguageIdentifier(from: localization)) } ?? Locale.current.language
        build = bundle?.object(forInfoDictionaryKey: kCFBundleVersionKey as String) as? String
        applicationElement = application
        observer =
            if let created { .success(created) } else {
                .failure(ApplicationFailure(answer, subject: subject, process: instance.process.pid) { ApplicationFailure(subject: subject, reason: .accessibility(answer)) })
            }
        activation = (instance.application.isActive, instance.application.isHidden)
        state = .running(
            InstanceState.Activity(status: .launching, pendingEventAge: nil, active: instance.application.isActive, windows: [], usage: nil, listening: .success([]), statusItems: nil)
        )
        var serial = ProcessSerialNumber()
        self.serial = unsafe findProcess(instance.process.pid, &serial) ? serial : nil
        queue = DispatchSerialQueue(label: "\(String(reflecting: Watch.self)) \(instance.process)")
    }

    package nonisolated var unownedExecutor: UnownedSerialExecutor { unsafe queue.asUnownedSerialExecutor() }

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
        let (process, began): (ProcessInstance, Date) = (instance.process, .now)
        tasks.append(Task { for await line in LogEntry.stream(of: .process(process.pid), levels: [.fault]) { self.emit(line.map(InstanceEvent.Change.logged(entry:))) } })
        tasks += DiagnosticReport.directories.map { directory in
            let reports = DiagnosticReport.written(since: began, in: directory).filter { @Sendable found in
                found[case: \.success].map { report in report.kind != .crash && report.isWritten(by: process) } ?? true
            }
            return Task { for await found in reports { self.emit(found.map(InstanceEvent.Change.reported(report:))) } }
        }
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
            case .success(let notice) where [.appUnresponsive, .appResponsive].contains(notice.notification): beat()
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
                case (.created, false), (.destroyed, false), (.moved, false), (.resized, false), (.reordered, _), (.activeSpaceChanged, _), (.appUnresponsive, _), (.appResponsive, _): false
            }
        }
        for event in reported { emit(.window(change: event.change, window: event.window)) }
        if case .running(let activity) = state {
            publish(
                .running(
                    InstanceState.Activity(
                        status: activity.status,
                        pendingEventAge: activity.pendingEventAge,
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
    enum Message: Sendable {
        case state(InstanceState)
        case event(InstanceEvent)
        case observation(Report)
        case failure(ApplicationFailure)
    }

    package struct Report: Encodable, Sendable {
        let instance: ProcessInstance
        let identifier: String?
        let language: String
        let observation: Observation
    }

    func messages() -> AsyncStream<Message> {
        let (stream, continuation) = AsyncStream<Message>.makeStream()
        let id = UUID()
        continuation.yield(.state(state))
        continuation.yield(
            .event(
                InstanceEvent(instance: instance.process, sequence: sequence, at: .now, change: .started(identifier: instance.application.bundleIdentifier, name: instance.application.localizedName))
            )
        )
        for container in open.values where container.asking {
            continuation.yield(.event(InstanceEvent(instance: instance.process, sequence: sequence, at: .now, change: InstanceEvent.Change(asking: container))))
        }
        continuation.onTermination = { _ in self.subscribers.withLock { held in held?[id] = nil } }
        let listening: Bool = subscribers.withLock { held in
            held?[id] = continuation
            return held != nil
        }
        if !listening { continuation.finish() }
        return stream
    }

    func states() -> some AsyncSequence<InstanceState, Never> & Sendable { messages().compactMap { message in message[case: \.state] } }

    package var current: InstanceState { state }

    private func send(_ message: Message) {
        subscribers.withLock { held in
            for subscriber in (held ?? [:]).values { subscriber.yield(message) }
        }
    }

    private func emit(_ change: InstanceEvent.Change) {
        sequence += 1
        send(.event(InstanceEvent(instance: instance.process, sequence: sequence, at: .now, change: change)))
    }

    private func emit(_ read: Result<InstanceEvent.Change, ProcessFailure>) {
        switch read {
            case .success(let change): emit(change)
            case .failure(let failure): send(.failure(ApplicationFailure(subject: "diagnostics of process \(instance.process.pid)", reason: .foundation(failure as NSError))))
        }
    }

    private func publish(_ next: InstanceState) {
        guard !state.is(\.exited) || next.is(\.exited), next[case: \.failed]?.reason != .system(.noSuchProcess) else { return }
        let (before, after) = (state[case: \.running], next[case: \.running])
        if let after, after.status != before?.status { emit(.status(status: after.status)) }
        if driven, after?.status.is(\.unresponsive) == true, before?.status.is(\.unresponsive) != true {
            let process: ProcessInstance = instance.process
            tasks.append(Task { self.emit(await Result { () async throws(ProcessFailure) -> URL in try await process.sample(for: messagingTimeout) }.map(InstanceEvent.Change.sampled(report:))) })
        }
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

    package func intern(_ locators: [Locator]) -> [Int] {
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
        guard read.role == kAXTextAreaRole, let range = read.facts[kAXVisibleCharacterRangeAttribute]?.range, let value = read.facts[kAXValueAttribute]?.text,
            let visible = Range(NSRange(location: range.location, length: range.length), in: value)
        else { return (answer, read) }
        let facts: [String: Fact] = read.facts.merging([kAXValueAttribute: .text(value: String(value[visible]))]) { _, shown in shown }
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
            documents: open.values.compactMap(\.document),
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
            emit(container.asking ? InstanceEvent.Change(asking: container) : .opened(container: ref, kind: container.kind, title: container.title))
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
        var restated: Set<AXUIElement> = []
        for (element, names) in marked where names.contains(kAXValueChangedNotification) && names.isDisjoint(with: invalidating) {
            guard let ref = table[element].flatMap({ id in owners[id] }), let held = controls[ref], held.control.kind.holdsValueAttribute, held.control.subrole != kAXSecureTextFieldSubrole,
                let index = open[held.container]?.controls.firstIndex(where: { (control: Control) -> Bool in control.ref == ref })
            else { continue }
            restated.insert(element)
            let fact: Fact? = read(element, [kAXValueAttribute]).element.facts[kAXValueAttribute].flatMap { (fact: Fact) -> Fact? in fact.elements.isEmpty ? fact : nil }
            guard fact != held.control.value else { continue }
            controls[ref]?.control.value = fact
            open[held.container]?.controls[index].value = fact
            if let fact { emit(.value(control: ref, kind: held.control.kind, fact: fact)) }
            if let container = open[held.container] { try observe(container, opener: nil) }
        }
        let rewalked: [AXUIElement: Set<String>] = marked.filter { element, names in
            !names.isDisjoint(with: invalidating) || (names.contains(kAXValueChangedNotification) && !restated.contains(element))
        }
        let touched: Set<Int> = Set(
            rewalked.keys.compactMap { (element: AXUIElement) -> Int? in
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
        let noticedNow: [AXUIElement: Set<String>] = self.marked.withLock { held in exchange(&held, with: [:]) }
        let marked: [AXUIElement: Set<String>] = exchange(&postponed, with: [:]).merging(noticedNow) { kept, read in kept.union(read) }
        let announced: [(text: String, priority: Lenient<AXPriority>?)] = noticed.withLock { noticed in exchange(&noticed.announced, with: []) }
        let dragged: [(element: AXUIElement, notification: String)] = self.dragged.withLock { held in exchange(&held, with: []) }
        for (text, priority) in announced { emit(.announced(text: text, priority: priority)) }
        for (element, notification) in dragged {
            let held: (ref: Int, kind: Control.Kind)? = table[element].flatMap { id in owners[id] }.flatMap { ref in controls[ref].map { held in (ref, held.control.kind) } }
            emit(.dragging(notification: notification, control: held?.ref, kind: held?.kind))
        }
        let now: (active: Bool, hidden: Bool) = (instance.application.isActive, instance.application.isHidden)
        if now.active != activation.active { emit(.frontmost(active: now.active)) }
        if now.hidden != activation.hidden { emit(.hidden(hidden: now.hidden)) }
        activation = now
        if marked.values.contains(where: { names in names.contains(kAXTitleChangedNotification) }) { windowTicks.continuation.yield() }
        let read = Result { () throws(ApplicationFailure) -> InstanceState.Activity in
            let thread: MainThread
            do throws(Errno) { thread = try MainThread(of: instance.process) } catch { throw ApplicationFailure(subject: subject, reason: .system(error)) }
            let pending: Duration? = try serial.flatMap { serial throws(ApplicationFailure) -> Duration? in try pendingEventAge(of: serial) }
            let held: Bool = [.stopped, .traced].contains(thread.state) || pending != nil
            if pending != nil { postponed = marked }
            let after: MainThread?
            do throws(ApplicationFailure) {
                if !held, instance.application.activationPolicy != .prohibited || !marked.isEmpty { try reconcile(marked) }
                after = nil
            } catch  where error.reason.is(\.unanswered) {
                do throws(Errno) { after = try MainThread(of: instance.process) } catch { throw ApplicationFailure(subject: subject, reason: .system(error)) }
            }
            let ready: Bool = instance.application.isFinishedLaunching || open.values.contains(where: \.asking)
            let status: InstanceState.Activity.Status =
                switch (held, after, ready) {
                    case (true, _, _): .unresponsive(thread: thread.state)
                    case (false, .some(let after), true): .unresponsive(thread: after.status(since: thread))
                    case (false, _, false): .launching
                    case (false, .none, true): .responding
                }
            let blocked: Bool = status.is(\.unresponsive)
            let began: ResourceUsage? = state[case: \.running]?.usage?[case: \.success]?.start
            let usage: Result<ResourceUsage.Interval, Errno>? =
                blocked
                ? Result { () throws(Errno) -> ResourceUsage.Interval in
                    let now = try ResourceUsage(of: instance.process)
                    return now.since(began ?? now)
                } : nil
            let listening = Result { () throws(Errno) in try instance.process.listeningPorts() }
            return InstanceState.Activity(
                status: status,
                pendingEventAge: pending,
                active: activation.active,
                windows: windows(blocking: blocked),
                usage: usage,
                listening: listening,
                statusItems: nil,
            )
        }
        switch read {
            case .success(let activity): publish(.running(activity))
            case .failure(let failure): publish(.failed(failure))
        }
        return state
    }

    private func windows(blocking: Bool) -> [InstanceState.Window] {
        let placed: [CGWindowID: [Container]] = Dictionary(grouping: open.values.filter { container in container.window != nil }) { container in container.window ?? kCGNullWindowID }
        return rows.map { row in
            let held: [Container] = placed[row.number] ?? []
            var described: InstanceState.Window = row.described(by: held.first { container in container.parent == nil } ?? held.first, blocking: blocking && row.onScreen && row.band == .modalPanel)
            described.asking = described.asking || held.contains(where: \.asking)
            return described
        }
    }

    // --- [MODEL]

    package func model(_ scope: Model.Scope, known: Model.Known? = nil, clock: some Clock<Duration> = ContinuousClock()) async throws(ApplicationFailure) -> Model {
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
                return Model(sequence: model.sequence, containers: containers, located: model.located, pointer: Pointer(of: instance.application, windows: read.windows))
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
                let hierarchy = Hierarchy(
                    elements: Array(elements.values),
                    roots: menus,
                    windows: [],
                    documents: open.values.compactMap(\.document),
                    regular: true,
                    focused: nil,
                    truncated: truncated,
                    extras: nil,
                )
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

    enum Presentation: CaseTagged, Comparable, Sendable {
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
        let hierarchy = Hierarchy(
            elements: shown,
            roots: [head.id],
            windows: row.map { row in [row] } ?? [],
            documents: open.values.compactMap(\.document),
            regular: true,
            focused: nil,
            truncated: truncated,
            extras: nil,
        )
        let model = Model(hierarchy, sequence: sequence, under: opener.map { ref in [head.id: locators[ref - 1]] } ?? [:], ref: intern)
        guard let container = model.containers.first(where: { container in container.kind == .menu }), open[container.ref] == nil else { return }
        locate(model)
        open[container.ref] = container
        emit(.opened(container: container.ref, kind: .menu, title: container.title))
        try observe(container, opener: opener.flatMap { ref in controls[ref] })
    }

    nonisolated func presenting<Value>(
        _ ref: Int?,
        by opening: String? = nil,
        clock: some Clock<Duration> = ContinuousClock(),
        _ body: ([Container]) async throws(ApplicationFailure) -> Value,
    ) async throws(ApplicationFailure) -> (value: Value, presentation: Presentation?) {
        guard let ref else { return (try await body([]), nil) }
        var levels: [(menu: Container, presentation: Presentation?)] = []
        let value: Result<Value, ApplicationFailure> = await Result { () async throws(ApplicationFailure) -> Value in
            while let level = try await self.popUp(toward: ref, by: opening, past: levels.count, clock: clock) { levels.append(level) }
            return try await body(levels.map(\.menu))
        }
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
        let openers: [(control: Control, action: String)] = prefixes.compactMap { (prefix: Int?, inner: Bool) -> (control: Control, action: String)? in
            switch (prefix.flatMap { prefix in controls[prefix]?.control }, inner, opening) {
                case (.some(let control), false, .some(let action)): (control, action)
                case (.some(let control), _, _) where control.opens != nil || pressOpened.contains(control.role): (control, kAXPressAction)
                case (.some(let control), true, _) where control.actions?.contains(kAXShowMenuAction) == true: (control, kAXShowMenuAction)
                case (.some, _, _), (.none, _, _): nil
            }
        }
        guard openers.indices.contains(count) else { return nil }
        let opener: Control = openers[count].control
        if let level = open.values.first(where: { menu in menu.kind == .menu && menu.locator.steps.dropLast().elementsEqual(opener.locator.steps) }) { return (level, nil) }
        let element: AXUIElement = try await resolve(opener.ref)
        let shown: Element = read(element, frameAttributes).element
        let action: String = openers[count].action
        let process: pid_t = instance.process.pid
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
            let (menusOpened, menuOpened) = AsyncStream<Void>.makeStream(bufferingPolicy: .bufferingNewest(1))
            let id = UUID()
            noticed.withLock { noticed in noticed.openings[id] = menuOpened }
            defer { noticed.withLock { noticed in noticed.openings[id] = nil } }
            let shortened: AXError = AXUIElementSetMessagingTimeout(element, Float(messagingTimeout / 2 / .seconds(1)))
            guard shortened == .success else { throw ApplicationFailure(subject: "messaging timeout of \(opener.ref)", reason: .accessibility(shortened)) }
            let hold = try await PermittedFrontList.shared.holding(
                excluding: { application in application.processIdentifier == process },
                for: messagingTimeout / 2,
                clock: clock,
                { () async -> Task<AXError, Never> in
                    let pressing: Task<AXError, Never> = Task.detached { AXUIElementPerformAction(element, action as CFString) }
                    _ = await race(
                        { () async -> Void? in await menusOpened.first { _ in true } },
                        against: { () async -> Void? in
                            try? await clock.sleep(for: [AXError.success, .cannotComplete].contains(await pressing.value) ? messagingTimeout / 2 : .zero)
                            return nil
                        },
                    )
                    return pressing
                },
            )
            let answer: AXError = await hold.value.get().value
            let restored: AXError = AXUIElementSetMessagingTimeout(element, Float(messagingTimeout / .seconds(1)))
            let opened: Result<Int?, ApplicationFailure> =
                switch (hold.released, restored, [AXError.success, .cannotComplete].contains(answer)) {
                    case (.failure(let failure), _, _): .failure(failure)
                    case (.success, _, _) where restored != .success: .failure(ApplicationFailure(subject: "messaging timeout of \(opener.ref)", reason: .accessibility(restored)))
                    case (.success, _, true):
                        await Result { () async throws(ApplicationFailure) -> Int? in
                            try await next(
                                { (change: InstanceEvent.Change) -> Int? in change[case: \.opened].flatMap { opened in opened.kind == .menu ? opened.container : nil } },
                                in: events,
                                clock: clock,
                            )
                        }
                    case (.success, _, false): .failure(ApplicationFailure(subject: "\(action) on \(opener.ref)", reason: .accessibility(answer)))
                }
            guard let level = try opened.get().flatMap({ menu in open[menu] }) else {
                let cancelled: AXError = AXUIElementPerformAction(element, kAXCancelAction as CFString)
                unopened.append("\(forward ? Presentation.forward : .behind) with \(kAXCancelAction) answering AXError \(cancelled.rawValue)")
                continue
            }
            return (level, forward ? .forward : .behind)
        }
        throw unopened.isEmpty
            ? ApplicationFailure(subject: "\(frameAttribute) of \(opener.ref) in the active application", reason: .accessibility(.noValue))
            : ApplicationFailure(subject: "\(action) on \(opener.ref), \(unopened.formatted())", reason: .unseen)
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

    func perform<Value: Sendable>(on ref: Int?, _ body: @Sendable (AXUIElement?) throws(ApplicationFailure) -> Value) async throws(ApplicationFailure) -> Value {
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

    func frame(of ref: Int) async throws(ApplicationFailure) -> (window: CGWindowID, frame: CGRect) {
        if let shown = drawn[ref] {
            guard rows.contains(where: { row in row.number == shown.window && row.onScreen }) else { throw stale(ref) }
            return shown
        }
        let element = try await resolve(ref)
        guard let frame = read(element, frameAttributes).element.frame else { throw ApplicationFailure(subject: "\(frameAttribute) of \(ref)", reason: .accessibility(.noValue)) }
        guard let window = open[controls[ref]?.container ?? ref]?.window else { throw ApplicationFailure(subject: String(ref), reason: .noWindow) }
        return (window, frame)
    }

    func documents() -> [CGWindowID: URL] {
        Dictionary(open.values.compactMap { container in container.window.flatMap { window in container.document.map { document in (window, document) } } }) { first, _ in first }
    }

    func addOpenCommandDocuments(_ documents: [URL]) { openCommandDocuments.append(contentsOf: documents) }

    package var documentFiles: [URL] { openCommandDocuments.union(open.values.compactMap(\.document)).elements.filter(\.isFileURL) }

    private func stale(_ ref: Int) -> ApplicationFailure {
        let steps: [Locator.Step] = locators.indices.contains(ref - 1) ? locators[ref - 1].steps : []
        let reached: Container? = open.values.lazy.filter { container in steps.starts(with: container.locator.steps) }.max { first, second in first.locator.steps.count < second.locator.steps.count }
        return ApplicationFailure(subject: String(ref), reason: .stale(reached: reached?.locator.steps.count ?? 0, candidates: reached?.controls.compactMap(\.label?.text) ?? []))
    }
}

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
            guard timed == .success else { throw ApplicationFailure(subject: "messaging timeout of the system-wide element", reason: .accessibility(timed)) }
            return try WindowServerConnection()
        }
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

    package func watch(_ instance: Instance, driven: Bool = true) async -> Watch {
        let before: (watch: Watch, driven: Bool)? = held[instance.process]
        let after: (watch: Watch, driven: Bool) = (before?.watch ?? Watch(instance), driven || before?.driven == true)
        held[instance.process] = after
        let started: Watch = after.watch
        if before == nil { await started.start(windows: notices(of: Set(WindowNotification.allCases), from: instance.process.pid)) { await self.remove(instance.process, started) } }
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
                InstanceState.Window.all([.optionOnScreenOnly, .excludeDesktopElements]).filter { owner, windows in
                    windows.contains { window in window.layer >= InstanceState.Window.Band.modalPanel.level && window.band != .mainMenu && window.band != .status }
                        && NSRunningApplication(processIdentifier: owner).map { application in application.activationPolicy != .regular } == true
                        && Result { () throws(ApplicationFailure) in try AXUIElementCreateApplication(owner).elements(kAXWindowsAttribute).isEmpty } != .success(true)
                }.keys
            )
            let listed: Result<[Instance], ApplicationFailure> = Result { () throws(ApplicationFailure) in
                try NSWorkspace.shared.instances { application in owners.contains(application.processIdentifier) }
            }
            if case .failure(let failure) = listed { delivery.yield(.failure(failure)) }
            return listed[case: \.success]
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
            let answer: AXError = unsafe AXObserverCreate(
                host,
                { _, _, _, refcon in unsafe (refcon.map { refcon in unsafe Unmanaged<AnyObject>.fromOpaque(refcon).takeUnretainedValue() } as? AsyncStream<Void>.Continuation)?.yield() },
                &created,
            )
            guard answer == .success, let observer = created else { return .failure(ApplicationFailure(subject: "status item observer of process \(host)", reason: .accessibility(answer))) }
            let application: AXUIElement = AXUIElementCreateApplication(host)
            let refused: AXError? = [kAXLayoutChangedNotification, kAXUIElementDestroyedNotification].map { notification in
                unsafe AXObserverAddNotification(observer, application, notification as CFString, refcon.toOpaque())
            }
            .first { registered in registered != .success }
            guard let refused else {
                CFRunLoopAddSource(CFRunLoopGetMain(), AXObserverGetRunLoopSource(observer), .commonModes)
                return .success(observer)
            }
            return .failure(ApplicationFailure(subject: "status item notifications of process \(host)", reason: .accessibility(refused)))
        }
        recount.onTermination = { _ in
            for observation in observations { observation.invalidate() }
            CFRunLoopPerformBlock(CFRunLoopGetMain(), CFRunLoopMode.commonModes.rawValue) {
                for observer in hosts.compactMap({ host in host[case: \.success] }) { CFRunLoopSourceInvalidate(AXObserverGetRunLoopSource(observer)) }
                unsafe refcon.release()
            }
            CFRunLoopWakeUp(CFRunLoopGetMain())
        }
        let windowed = notices(of: Set(WindowNotification.allCases).subtracting([.reordered, .appUnresponsive, .appResponsive])).debounce(for: quiet).map { notice in notice.map { _ in false } }
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
                        listening: activity.listening,
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
