package import AppKit
import AppleEvents
@unsafe @preconcurrency import ApplicationServices
import CasePaths
import CasePathsCore
import Encoding
import Files
import OSLog
import OrderedCollections
import Processes
import Results
import SPI
import Synchronization
import System

// --- [CONSTANTS] -----------------------------------------------------------------------

private let dragging: [NSAccessibility.Notification] = [
    .NSAccessibilityDraggingSourceDragBegan, .NSAccessibilityDraggingSourceDragEnded, .NSAccessibilityDraggingDestinationDropAllowed,
    .NSAccessibilityDraggingDestinationDropNotAllowed, .NSAccessibilityDraggingDestinationDragAccepted, .NSAccessibilityDraggingDestinationDragNotAccepted,
]
private let remote: [NSAccessibility.Notification] = [.remoteApplicationElementsAdded, .remoteApplicationElementsRemoved]
private let notifications: [String] =
    [
        kAXMainWindowChangedNotification, kAXFocusedWindowChangedNotification, kAXFocusedUIElementChangedNotification, kAXWindowCreatedNotification, kAXWindowMovedNotification,
        kAXWindowResizedNotification, kAXWindowMiniaturizedNotification, kAXWindowDeminiaturizedNotification, kAXDrawerCreatedNotification, kAXSheetCreatedNotification,
        kAXHelpTagCreatedNotification, kAXValueChangedNotification, kAXUIElementDestroyedNotification, kAXElementBusyChangedNotification, kAXMenuOpenedNotification,
        kAXMenuClosedNotification, kAXMenuItemSelectedNotification, kAXRowCountChangedNotification, kAXRowExpandedNotification, kAXRowCollapsedNotification,
        kAXSelectedCellsChangedNotification, kAXUnitsChangedNotification, kAXSelectedChildrenMovedNotification, kAXSelectedChildrenChangedNotification, kAXResizedNotification,
        kAXMovedNotification, kAXCreatedNotification, kAXSelectedRowsChangedNotification, kAXSelectedColumnsChangedNotification, kAXSelectedTextChangedNotification,
        kAXTitleChangedNotification, kAXLayoutChangedNotification, kAXAnnouncementRequestedNotification, kAXLayoutCompleteNotification, kAXLoadCompleteNotification,
    ] + (dragging + remote).map(\.rawValue)
private let invalidating: Set<String> = Set(
    [
        kAXWindowCreatedNotification, kAXDrawerCreatedNotification, kAXSheetCreatedNotification, kAXCreatedNotification, kAXUIElementDestroyedNotification, kAXLayoutChangedNotification,
        kAXRowCountChangedNotification, kAXRowExpandedNotification, kAXRowCollapsedNotification, kAXTitleChangedNotification, kAXWindowMiniaturizedNotification,
        kAXWindowDeminiaturizedNotification, kAXUnitsChangedNotification, kAXSelectedChildrenChangedNotification, kAXSelectedRowsChangedNotification,
        kAXSelectedCellsChangedNotification, kAXLayoutCompleteNotification, kAXLoadCompleteNotification,
    ] + remote.map(\.rawValue)
)
private let moving: Set<String> = [kAXWindowMovedNotification, kAXWindowResizedNotification, kAXMovedNotification, kAXResizedNotification]
private let applicationAttributes: [String] = [kAXWindowsAttribute, kAXFocusedUIElementAttribute, kAXMenuBarAttribute, kAXExtrasMenuBarAttribute]
private let menuRoles: Set<String> = [kAXMenuBarRole, kAXMenuBarItemRole, kAXMenuRole, kAXMenuItemRole]
private let budget: (nodes: Int, time: Duration) = (4000, .seconds(3))
let pressOpened: Set<String> = [kAXMenuBarItemRole, kAXPopUpButtonRole, kAXMenuButtonRole]

// --- [SERVICES] ------------------------------------------------------------------------

struct RunLoopSource: ~Copyable {
    private let source: CFRunLoopSource

    init(_ source: CFRunLoopSource) {
        CFRunLoopAddSource(CFRunLoopGetMain(), source, .commonModes)
        self.source = source
    }

    deinit {
        let invalidated: CFRunLoopSource = source
        CFRunLoopPerformBlock(CFRunLoopGetMain(), CFRunLoopMode.commonModes.rawValue) { CFRunLoopSourceInvalidate(invalidated) }
        CFRunLoopWakeUp(CFRunLoopGetMain())
    }
}

package actor Watch {
    // --- [STATE]

    private enum Driving: Equatable {
        case watched
        case owed
        case answered
    }

    static let tables: URL = .temporaryDirectory.appending(
        components: Bundle.main.bundleIdentifier ?? ProcessInfo.processInfo.processName,
        String(describing: Watch.self),
        directoryHint: .isDirectory,
    )

    package nonisolated let instance: Instance
    package private(set) var language: Locale.Language
    private nonisolated let applicationName: String
    private let queue: DispatchSerialQueue
    private let serial: ProcessSerialNumber?
    private let build: String?
    private let applicationElement: AXUIElement
    private let observer: Result<AXObserver, ApplicationFailure>
    private let ticks: (stream: AsyncStream<Void>, continuation: AsyncStream<Void>.Continuation) = AsyncStream.makeStream(bufferingPolicy: .bufferingNewest(1))
    private let marked: Mutex<[AXUIElement: Set<String>]> = Mutex([:])
    private let dragged: Mutex<[(element: AXUIElement, notification: String)]> = Mutex([])
    private let noticed: Mutex<(announced: [(text: String, priority: InstanceEvent.Change.Priority?)], waiting: [String: [UUID: AsyncStream<Void>.Continuation]])> = Mutex(([], [:]))
    private let subscribers: Mutex<[UUID: AsyncStream<Message>.Continuation]?> = Mutex([:])
    private let windowTicks: (stream: AsyncStream<Void>, continuation: AsyncStream<Void>.Continuation) = AsyncStream.makeStream(bufferingPolicy: .bufferingNewest(1))
    private var observing: RunLoopSource?
    private var postponed: [AXUIElement: Set<String>] = [:]
    private var reordered: Bool = false
    private var lowered: Set<CGWindowID> = []
    private var stepWindows: Set<CGWindowID> = []
    private var windowsChanged: Bool = false
    private var rows: [InstanceState.Window] = []
    private var driving: Driving = .watched
    private var observations: [NSKeyValueObservation] = []
    private var unregistered: [String] = notifications
    private var table: [AXUIElement: Int] = [:]
    private var handles: [AXUIElement?] = []
    private var locators: [Locator]
    private var numbers: [Locator: Int]
    private let tableFile: (url: URL, handle: Result<FileHandle, ApplicationFailure>)
    private var located: [Int: Int] = [:]
    private var scrolled: Set<Int> = []
    private var steps: (running: Int, waiting: [CheckedContinuation<Void, Never>]) = (0, [])
    @TaskLocal private static var stepping: Bool = false
    private var drawn: [Int: (window: CGWindowID, frame: CGRect)] = [:]
    private let regions: Mutex<[CGWindowID: [CGRect]]> = Mutex([:])
    private var latency: Duration?
    private var followed: Int = 0
    private var owners: [Int: Int] = [:]
    private var controls: [Int: (control: Control, container: Int)] = [:]
    private var lists: [Int: Int] = [:]
    private var open: OrderedDictionary<Int, Container> = [:]
    private var secured: [Int: InstanceEvent.Answer] = [:]
    private var sent: [Int: Observation] = [:]
    private var pressed: (control: Control, container: Int)?
    private var openedBy: [Int: (control: Control, container: Int)] = [:]
    private var bars: [Container] = []
    private var levels: OrderedDictionary<Int, Container> = [:]
    private var focused: Int?
    private var activation: (active: Bool, hidden: Bool)
    private var unanswered: ContinuousClock.Instant?
    private var sampling: ContinuousClock.Instant?
    private var sequence: Int = 0
    private var state: InstanceState
    private var living: Task<Void, Never>?
    private var tasks: [Task<Void, Never>] = []
    private var openCommandDocuments: OrderedSet<URL> = []

    // --- [LIFECYCLE]

    init(_ instance: Instance) {
        let bundle: Bundle? = instance.application.bundleURL.flatMap(Bundle.init(url:))
        let preferred: [String]? = bundle?.bundleIdentifier.flatMap { identifier in CFPreferencesCopyAppValue("AppleLanguages" as CFString, identifier as CFString) as? [String] }
        let callback: AXObserverCallbackWithInfo = { _, element, notification, info, refcon in
            guard let watch: Watch = unsafe refcon.map({ refcon in unsafe Unmanaged<Watch>.fromOpaque(refcon).takeUnretainedValue() }) else { return }
            let name = notification as String
            let fields: [String: Any] = info as? [String: Any] ?? [:]
            let named: [AXUIElement] = [kAXUIElementsKey, remoteApplicationElementsKey].flatMap { (key: String) -> [AXUIElement] in fields[key] as? [AXUIElement] ?? [] }
            let announcement: String? = name == kAXAnnouncementRequestedNotification ? (fields[kAXAnnouncementKey] as? NSAttributedString)?.string ?? (fields[kAXAnnouncementKey] as? String) : nil
            let priority: InstanceEvent.Change.Priority? = (fields[kAXPriorityKey] as? Int).flatMap(AXPriority.init(rawValue:)).flatMap(InstanceEvent.Change.Priority.init)
            watch.marked.withLock { marked in
                for changed in named.isEmpty ? [element] : named { marked[changed, default: []].insert(name) }
            }
            if dragging.contains(NSAccessibility.Notification(rawValue: name)) { watch.dragged.withLock { dragged in dragged.append((element, name)) } }
            watch.noticed.withLock { noticed in
                noticed.announced += announcement.map { text in [(text, priority)] } ?? []
                for waiter in noticed.waiting[name, default: [:]].values { waiter.yield() }
            }
            watch.ticks.continuation.yield()
        }
        var created: AXObserver?
        let answer: AXError = unsafe AXObserverCreateWithInfoCallback(instance.process.pid, callback, &created)
        let name: String = instance.application.localizedName ?? instance.application.bundleIdentifier ?? "the application"
        self.instance = instance
        applicationName = name
        let localization: String? = bundle.flatMap { (bundle: Bundle) -> String? in
            Bundle.preferredLocalizations(from: bundle.localizations, forPreferences: preferred).first ?? bundle.developmentLocalization
        }
        language = localization.map { localization in Locale.Language(identifier: Locale.canonicalLanguageIdentifier(from: localization)) } ?? Locale.current.language
        build = bundle?.object(forInfoDictionaryKey: kCFBundleVersionKey as String) as? String
        applicationElement = AXUIElementCreateApplication(instance.process.pid)
        observer =
            if let created { .success(created) } else {
                .failure(ApplicationFailure(answer, subject: "Accessibility notifications of \(name)", process: instance.process.pid))
            }
        activation = (instance.application.isActive, instance.application.isHidden)
        state = .running(
            InstanceState.Activity(status: .launching, pendingEventAge: nil, active: instance.application.isActive, windows: [], usage: nil, statusItems: nil)
        )
        var serial = ProcessSerialNumber()
        self.serial = unsafe findProcess(instance.process.pid, &serial) ? serial : nil
        queue = DispatchSerialQueue(label: "\(String(reflecting: Watch.self)) \(instance.process)")
        let file: URL = Watch.tables.appending(component: instance.process.rawValue)
        var seed: [Locator] = []
        let handle: Result<FileHandle, ApplicationFailure>
        do {
            let decoder = JSONDecoder()
            let lines: [Data] = try ifPresent { () throws -> Data in try Data(contentsOf: file) }?.split(separator: UInt8(ascii: "\n")) ?? []
            seed = try lines.map { (line: Data) throws -> Locator in Locator(steps: try decoder.decode([Locator.Step].self, from: line)) }
            try FileManager.default.createDirectory(at: Watch.tables, withIntermediateDirectories: true)
            let descriptor = try FileDescriptor.open(FilePath(file.path(percentEncoded: false)), .writeOnly, options: [.append, .create, .closeOnExec], permissions: .ownerReadWrite)
            handle = .success(FileHandle(fileDescriptor: descriptor.rawValue, closeOnDealloc: true))
        } catch {
            handle = .failure(ApplicationFailure(subject: file.path(percentEncoded: false), cause: error))
        }
        (locators, numbers, tableFile) = (seed, Dictionary(zip(seed, 1...)) { first, _ in first }, (file, handle))
    }

    package nonisolated var unownedExecutor: UnownedSerialExecutor { unsafe queue.asUnownedSerialExecutor() }

    func start(windows notices: AsyncStream<Result<WindowServerConnection.Notice, ApplicationFailure>>, ending: @escaping @Sendable () async -> Void) {
        let process: pid_t = instance.process.pid
        var stated: CFTypeRef?
        let spoken: String? = unsafe AXUIElementCopyAttributeValue(applicationElement, preferredLanguageAttribute as CFString, &stated) == .success ? stated as? String : nil
        language = spoken.map { (spoken: String) -> Locale.Language in Locale.Language(identifier: Locale.canonicalLanguageIdentifier(from: spoken)) } ?? language
        observing = observer[case: \.success].map { observer in RunLoopSource(AXObserverGetRunLoopSource(observer)) }
        observations = [\NSRunningApplication.isActive, \.isHidden, \.isFinishedLaunching].map { [ticks] property in
            instance.application.observe(property) { _, _ in ticks.continuation.yield() }
        }
        rows = InstanceState.Window.all(.optionAll, of: process)[process, default: []]
        register()
        switch observer {
            case .success: beat()
            case .failure(let failure): publish(.failed(failure))
        }
        tasks = [
            Task { [ticks] in
                for await _ in ticks.stream { self.beat() }
            },
            Task {
                for await notice in notices { self.note(notice) }
            },
            Task { [windowTicks] in
                for await _ in windowTicks.stream { self.refresh() }
            },
        ]
        living = Task { [instance] in
            for await next in instance.lifetime(clock: SuspendingClock()) { self.publish(next) }
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
        (driving, reordered) = (.owed, true)
        windowTicks.continuation.yield()
        ticks.continuation.yield()
        let (process, began): (ProcessInstance, Date) = (instance.process, .now)
        tasks += DiagnosticReport.directories.map { directory in
            let reports = DiagnosticReport.written(since: began, in: directory).filter { @Sendable found in
                found[case: \.success].map { report in report.kind != .crash && report.isWritten(by: process) } ?? true
            }
            return Task { for await found in reports { self.emit(found.map(InstanceEvent.Change.reported(report:))) } }
        }
    }

    private func note(_ notice: Result<WindowServerConnection.Notice, ApplicationFailure>) {
        switch notice[case: \.success]?.notification {
            case .appUnresponsive, .appResponsive: beat()
            case .created, .destroyed, .shown, .hidden:
                windowsChanged = true
                windowTicks.continuation.yield()
                ticks.continuation.yield()
            case let notification:
                reordered = reordered || notification == .reordered
                windowTicks.continuation.yield()
        }
    }

    private func refresh() {
        let process: pid_t = instance.process.pid
        let ordered: Bool = exchange(&reordered, with: false)
        let blocking: Bool = state[case: \.running]?.status.is(\.unresponsive) == true
        let before: [CGWindowID: InstanceState.Window] = Dictionary(windows(blocking: blocking).map { row in (row.number, row) }) { first, _ in first }
        rows = InstanceState.Window.all(.optionAll, of: process)[process, default: []]
        let shown: [InstanceState.Window] = windows(blocking: blocking)
        let after: [CGWindowID: InstanceState.Window] = Dictionary(shown.map { row in (row.number, row) }) { first, _ in first }
        let events: [(change: WindowNotification, window: InstanceState.Window)] = OrderedSet(shown.map(\.number) + before.keys.sorted()).flatMap {
            (number: CGWindowID) -> [(change: WindowNotification, window: InstanceState.Window)] in
            let changes: [WindowNotification?] =
                switch (before[number], after[number]) {
                    case (.none, .some): [.created]
                    case (.some, .none): [.destroyed]
                    case (.some(let held), .some(let read)):
                        [
                            held.onScreen != read.onScreen && (read.spaces.isEmpty || held.hidden != read.hidden) ? (read.onScreen ? .shown : .hidden) : nil,
                            held.layer != read.layer ? .levelChanged : nil, held.frame.origin != read.frame.origin ? .moved : nil, held.frame.size != read.frame.size ? .resized : nil,
                            Set(read.spaces).isSubset(of: held.spaces) ? nil : .addedToSpace, Set(held.spaces).isSubset(of: read.spaces) ? nil : .removedFromSpace,
                        ]
                    case (.none, .none): []
                }
            return (after[number] ?? before[number]).map { row in changes.compactMap(\.self).map { change in (change: change, window: row) } } ?? []
        }
        for event in events where event.window.container == nil || ![WindowNotification.created, .destroyed, .moved, .resized].contains(event.change) {
            emit(.window(change: event.change, window: event.window))
        }
        if case .running(let activity) = state {
            publish(
                .running(
                    InstanceState.Activity(
                        status: activity.status,
                        pendingEventAge: activity.pendingEventAge,
                        active: activity.active,
                        windows: shown,
                        usage: activity.usage,
                        statusItems: activity.statusItems,
                    )
                )
            )
        }
        let appeared: [CGWindowID] = events.filter { (event: (change: WindowNotification, window: InstanceState.Window)) -> Bool in [WindowNotification.shown, .created].contains(event.change) }
            .map(\.window.number)
        if steps.running > 0 { stepWindows.formUnion(appeared) }
        if driving != .watched, ordered || !appeared.isEmpty { repair() }
    }

    private func repair() {
        let (process, front): (pid_t, pid_t?) = (instance.process.pid, focusedProcesses().front)
        let listed: [[String: Any]] = CGWindowListCopyWindowInfo([.optionOnScreenOnly, .excludeDesktopElements], kCGNullWindowID) as? [[String: Any]] ?? []
        let normal: [(owner: pid_t, window: CGWindowID)] = listed.compactMap { (row: [String: Any]) -> (owner: pid_t, window: CGWindowID)? in
            guard row[kCGWindowLayer as String] as? Int == InstanceState.Window.Band.normal.level, let owner = row[kCGWindowOwnerPID as String] as? pid_t,
                let window = row[kCGWindowNumber as String] as? CGWindowID
            else { return nil }
            return (owner, window)
        }
        let fronted: Int = front == process ? 0 : normal.firstIndex { (row: (owner: pid_t, window: CGWindowID)) -> Bool in row.owner == front } ?? 0
        let covering: Set<CGWindowID> = Set(normal[..<fronted].compactMap { (row: (owner: pid_t, window: CGWindowID)) -> CGWindowID? in row.owner == process ? row.window : nil })
        let raised: Bool = !covering.isSubset(of: lowered) && covering.isDisjoint(with: stepWindows)
        lowered = raised ? covering : lowered.intersection(covering)
        if raised { _ = instance.application.unhide() }
    }

    func finish() async {
        guard let living else { return }
        living.cancel()
        await living.value
    }

    private func close() async {
        observing = nil
        CFRunLoopPerformBlock(CFRunLoopGetMain(), CFRunLoopMode.commonModes.rawValue) { @Sendable [self] in withExtendedLifetime(self) {} }
        CFRunLoopWakeUp(CFRunLoopGetMain())
        observations = []
        ticks.continuation.finish()
        windowTicks.continuation.finish()
        do {
            if instance.process.hasExited { _ = try ifPresent { () throws in try FileManager.default.removeItem(at: tableFile.url) } }
        } catch {
            send(.failure(ApplicationFailure(subject: tableFile.url.path(percentEncoded: false), cause: error)))
        }
        let ending: [UUID: AsyncStream<Message>.Continuation] = subscribers.withLock { held in held.take() } ?? [:]
        for subscriber in ending.values { subscriber.finish() }
        let waiting: [String: [UUID: AsyncStream<Void>.Continuation]] = noticed.withLock { noticed in exchange(&noticed.waiting, with: [:]) }
        for waiter in waiting.values.flatMap(\.values) { waiter.finish() }
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
            let presentation: Presentation? = secured[container.ref].map { _ in instance.application.isActive ? .forward : .behind }
            let asking = InstanceEvent.Change(asking: container, secure: secured[container.ref], presentation: presentation)
            continuation.yield(.event(InstanceEvent(instance: instance.process, sequence: sequence, at: .now, change: asking)))
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

    nonisolated func notices(of name: String) -> AsyncStream<Void> {
        let (stream, continuation) = AsyncStream<Void>.makeStream(bufferingPolicy: .bufferingNewest(1))
        let id = UUID()
        continuation.onTermination = { _ in self.noticed.withLock { noticed in noticed.waiting[name]?[id] = nil } }
        noticed.withLock { noticed in noticed.waiting[name, default: [:]][id] = continuation }
        return stream
    }

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
            case .failure(let failure): send(.failure(ApplicationFailure(subject: "Diagnostics of \(applicationName)", reason: .foundation(failure as NSError))))
        }
    }

    private func publish(_ next: InstanceState) {
        guard !state.is(\.exited) || next.is(\.exited), next[case: \.failed]?.reason != .system(.noSuchProcess) else { return }
        let (before, after) = (state[case: \.running], next[case: \.running])
        if let after, after.status != before?.status { emit(.status(status: after.status)) }
        if driving != .watched, sampling == nil, after?.status.is(\.unresponsive) == true, before?.status.is(\.unresponsive) != true {
            let process: ProcessInstance = instance.process
            sampling = ContinuousClock.now
            tasks.append(
                Task {
                    let sampled: Result<URL, ProcessFailure> = await Result { () async throws(ProcessFailure) -> URL in try await process.sample(for: messagingTimeout) }
                    if self.state[case: \.running]?.status.is(\.unresponsive) == true { self.emit(sampled.map(InstanceEvent.Change.sampled(report:))) }
                    self.sampling = nil
                    self.beat()
                }
            )
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

    func reference(of element: AXUIElement) -> Int? { table[element].flatMap { (id: Int) -> Int? in owners[id] } }

    private func intern(_ locator: Locator) -> Int {
        if let number = numbers[locator] { return number }
        locators.append(locator)
        numbers[locator] = locators.count
        do {
            try tableFile.handle.get().write(contentsOf: JSONEncoder.jsonLines.encodeLine(locator.steps))
        } catch {
            send(.failure(error as? ApplicationFailure ?? ApplicationFailure(subject: tableFile.url.path(percentEncoded: false), cause: error)))
        }
        return locators.count
    }

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

    private func root(of locator: Locator) -> Int? { numbers[Locator(steps: Array(locator.steps.prefix(1)))].flatMap { ref in located[ref] } }

    private func locate(_ model: Model) {
        located.merge(model.located) { _, read in read }
        drawn.merge(model.drawn) { _, read in read }
        owners.merge(model.located.map { ref, element in (element, ref) }) { _, read in read }
        controls.merge(model.containers.flatMap { container in container.controls.map { control in (control.ref, (control: control, container: container.ref)) } }) { _, read in read }
        for container in model.containers {
            var list: Int?
            for control in container.controls where control.parent == nil || control.parent == control.ref {
                list = control.kind == .row ? list : control.kind == .multi ? control.ref : nil
                if control.kind == .row, let list { lists[control.ref] = list }
            }
        }
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
        let asked: ContinuousClock.Instant = .now
        let (answer, read) = element.element(id, reading: attributes, interning: intern)
        unanswered = answer == .cannotComplete ? unanswered ?? asked : nil
        guard read.role == kAXTextAreaRole, let range = read.facts[kAXVisibleCharacterRangeAttribute]?.range, let value = read.facts[kAXValueAttribute]?.text,
            let visible = Range(NSRange(location: range.location, length: range.length), in: value)
        else { return (answer, read) }
        let facts: [String: Fact] = read.facts.merging([kAXValueAttribute: .text(value: String(value[visible]))]) { _, shown in shown }
        return (answer, Element(id: id, facts: facts, actions: read.actions, window: read.window, descriptions: read.descriptions, counts: read.counts, settable: read.settable))
    }

    private func application() throws(ApplicationFailure) -> Element {
        let (answer, app) = read(applicationElement, applicationAttributes)
        guard answer == .success else { throw applicationElement.failure(answer, reading: kAXWindowsAttribute) }
        return app
    }

    private func walk(_ starts: [(id: Int, reach: Reach, depth: Int)], rows: (multi: Int, ids: [Int])?) throws(ApplicationFailure) -> (elements: [Int: Element], truncated: [Int]) {
        let (deadline, top) = (SuspendingClock.now + budget.time, intern(applicationElement))
        var elements: [Int: Element] = [:]
        var pending: [(id: Int, parent: Int?, reach: Reach, depth: Int)] = starts.reversed().map { start in (start.id, nil, start.reach, start.depth) }
        while let next = pending.popLast() {
            guard elements.count < budget.nodes, SuspendingClock.now < deadline else { return (elements, ([next] + pending).compactMap(\.parent)) }
            guard elements[next.id] == nil, let element = handles[next.id] else { continue }
            let first: (answer: AXError, element: Element) = read(element, next.reach.attributes)
            let reach: Reach = next.reach == .menu && first.element.role.map(menuRoles.contains) == false ? .window : next.reach
            let (answer, node) = reach == next.reach ? first : read(element, reach.attributes)
            switch answer {
                case .success: elements[next.id] = node
                case .cannotComplete, .apiDisabled: throw applicationElement.failure(answer, reading: kAXChildrenAttribute)
                default: continue
            }
            let depth: Int = reach == next.reach ? next.depth : Int.max
            let children: [Int] =
                switch (rows, depth > 0) {
                    case (_, false): []
                    case (.some(let rows), true) where rows.multi == next.id: rows.ids + node.columns
                    case (_, true): node.children
                }
            let inner: Reach = reach == .menu ? .menu : reach == .web || node.role == NSAccessibility.Role.webAreaRole.rawValue ? .web : .element
            pending += children.reversed().map { child -> (id: Int, parent: Int?, reach: Reach, depth: Int) in (child, next.id, inner, depth - 1) }
            if let parent = node.facts[kAXParentAttribute]?.element, reach != .menu, parent != top, elements[parent] == nil { pending.append((parent, nil, .element, 0)) }
        }
        return (elements, [])
    }

    private func hierarchy(of app: Element, full: Set<Int>, menus: Bool = false, rows ranged: (multi: Int, ids: [Int])? = nil) throws(ApplicationFailure) -> Hierarchy {
        let windows: [Int] = app.facts[kAXWindowsAttribute]?.elements ?? []
        let bars: [Int] = [kAXMenuBarAttribute, kAXExtrasMenuBarAttribute].compactMap { attribute in app.facts[attribute]?.element }.filter { bar in menus || full.contains(bar) }
        let starts: [(id: Int, reach: Reach, depth: Int)] =
            bars.map { bar in (id: bar, reach: Reach.menu, depth: 2) } + windows.map { window in (id: window, reach: Reach.window, depth: full.contains(window) ? Int.max : 0) }
        let (elements, truncated) = try walk(starts, rows: ranged)
        var read = Hierarchy(
            elements: Array(elements.values),
            roots: windows + bars,
            windows: self.windows(blocking: false),
            documents: open.values.compactMap(\.document),
            regular: instance.application.activationPolicy == .regular,
            focused: app.facts[kAXFocusedUIElementAttribute]?.element,
            truncated: truncated + windows.filter { window in !full.contains(window) },
            extras: app.facts[kAXExtrasMenuBarAttribute]?.element,
        )
        let (described, hollow): (Set<CGWindowID>, [CGWindowID: Int]) = (read.described, read.unreadable)
        let served: [(id: Int, host: Int?)] = try read.windows.filter { row in row.remote != nil && !described.contains(row.number) }.map {
            (row: InstanceState.Window) throws(ApplicationFailure) -> (id: Int, host: Int?)? in
            let server: pid_t? = row.remote.flatMap(windowDescription(of:))?.owner
            let listed: [AXUIElement] = try server.map { (owner: pid_t) throws(ApplicationFailure) -> [AXUIElement] in try AXUIElementCreateApplication(owner).elements(kAXWindowsAttribute) } ?? []
            let shown: AXUIElement? = listed.first { (element: AXUIElement) -> Bool in element.window == row.number }
            return shown.map { (element: AXUIElement) -> (id: Int, host: Int?) in (id: intern(element), host: hollow[row.number]) }
        }
        .compactMap(\.self)
        let (reached, cut) = try walk(served.map { entry in (id: entry.id, reach: Reach.window, depth: Int.max) }, rows: nil)
        let links: [Int: [String: Fact]] = Dictionary(
            served.flatMap { (entry: (id: Int, host: Int?)) -> [(Int, [String: Fact])] in
                entry.host.map { (host: Int) -> [(Int, [String: Fact])] in
                    [(host, [kAXChildrenAttribute: Fact.list(values: [.element(id: entry.id)])]), (entry.id, [kAXParentAttribute: Fact.element(id: host)])]
                } ?? []
            }
        ) { first, _ in first }
        read.elements = elements.merging(reached) { held, _ in held }.values.map { (element: Element) -> Element in
            links[element.id].map { (link: [String: Fact]) -> Element in
                let facts: [String: Fact] = element.facts.merging(link) { (_: Fact, linked: Fact) -> Fact in linked }
                return Element(id: element.id, facts: facts, actions: element.actions, window: element.window, descriptions: element.descriptions, counts: element.counts, settable: element.settable)
            } ?? element
        }
        let unhosted: [Int] = served.filter { entry in entry.host == nil }.map(\.id)
        (read.roots, read.truncated, read.full) = (windows + unhosted + bars, read.truncated + cut, full.union(unhosted))
        return read
    }

    // --- [CHANGES]

    private func merged(_ hierarchy: Hierarchy) throws(ApplicationFailure) -> Model {
        scrolled.subtract(hierarchy.full)
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
            return !menuKinds.contains(container.kind) && root.map(hierarchy.full.contains) == true
        }
        let fresh: OrderedDictionary<Int, Container> = OrderedDictionary(within.map { (container: Container) -> (Int, Container) in (container.ref, container) }) { first, _ in first }
        let closed: OrderedDictionary<Int, Container> = open.filter { ref, container in
            container.kind != .menu && fresh[ref] == nil && root(of: container.locator).map { root in hierarchy.full.contains(root) || !windows.contains(root) } != false
        }
        let opened: OrderedDictionary<Int, Container> = fresh.filter { ref, _ in open[ref] == nil }
        let front: String? = instance.application.isActive ? nil : NSWorkspace.shared.frontmostApplication?.localizedName
        for (ref, container) in closed {
            emit(.closed(container: ref, kind: container.kind, title: container.title, front: front))
            (sent[ref], open[ref], secured[ref], openedBy[ref]) = (nil, nil, nil, nil)
        }
        for ref in opened.keys { openedBy[ref] = pressed }
        var entered: [Int: InstanceEvent.Answer] = [:]
        for container in fresh.values where container.asking && secured[container.ref] == nil {
            let field: Control? = container.controls.first { (control: Control) -> Bool in control.subrole == kAXSecureTextFieldSubrole }
            let element: AXUIElement? = field.flatMap { (field: Control) -> AXUIElement? in located[field.ref].flatMap { (id: Int) -> AXUIElement? in handles[id] } }
            guard let field, let element, try !element.settable(kAXValueAttribute) else { continue }
            entered[container.ref] = InstanceEvent.Answer(ref: field.ref, label: field.label?.text)
        }
        for (ref, container) in fresh where opened[ref] != nil || entered[ref] != nil {
            let presentation: Presentation? = entered[ref].map { _ in instance.application.isActive ? .forward : .behind }
            let asking = InstanceEvent.Change(asking: container, secure: entered[ref], presentation: presentation)
            emit(container.asking ? asking : .opened(container: ref, kind: container.kind, title: container.title))
        }
        secured.merge(entered) { _, read in read }
        if !closed.isEmpty || !opened.isEmpty { windowTicks.continuation.yield() }
        open.merge(fresh) { _, read in read }
        bars = menuBars + bars.filter { held in !menuBars.contains { read in read.kind == held.kind } }
        for container in fresh.values + menuBars {
            let popover: AXUIElement? = container.kind == .popover ? located[container.ref].flatMap { (id: Int) -> AXUIElement? in handles[id] } : nil
            let linked: Int? = popover.flatMap { (element: AXUIElement) -> Int? in self.read(element, [kAXLinkedUIElementsAttribute]).element.facts[kAXLinkedUIElementsAttribute]?.elements.first }
            let anchor: (control: Control, container: Int)? = linked.flatMap { (id: Int) -> Int? in owners[id] }.flatMap { (ref: Int) -> (control: Control, container: Int)? in controls[ref] }
            try observe(container, opener: anchor ?? openedBy[container.ref])
        }
        return Model(sequence: sequence, containers: read.containers + open.values.filter { container in container.kind == .menu }, located: read.located)
    }

    private func observe(_ container: Container, opener: (control: Control, container: Int)?) throws(ApplicationFailure) {
        guard let observation = try Observation(container, in: open, build: build, opener: opener, after: sent[container.ref], at: .now, locator: { ref in locators[ref - 1] }) else { return }
        sent[container.ref] = observation
        send(.observation(Report(instance: instance.process, identifier: instance.application.bundleIdentifier, language: language.minimalIdentifier, observation: observation)))
    }

    private func reconcile(_ marked: [AXUIElement: Set<String>]) throws(ApplicationFailure) {
        register()
        for (element, names) in marked where !names.isDisjoint(with: moving) {
            guard let ref = table[element].flatMap({ id in owners[id] }), let container = open[ref], let frame = read(element, frameAttributes).element.frame else { continue }
            emit(.moved(container: ref, kind: container.kind, frame: frame))
        }
        for (element, names) in marked where names.contains(kAXMenuClosedNotification) {
            guard let ref = table[element].flatMap({ id in owners[id] }), let menu = open[ref], menu.kind == .menu else { continue }
            emit(.closed(container: ref, kind: .menu, title: menu.title, front: instance.application.isActive ? nil : NSWorkspace.shared.frontmostApplication?.localizedName))
            (sent[ref], open[ref]) = (nil, nil)
        }
        for (element, names) in marked where names.contains(kAXMenuOpenedNotification) && !names.contains(kAXMenuClosedNotification) { try track(element) }
        let changed: [AXUIElement: Element] = Dictionary(
            uniqueKeysWithValues: marked.compactMap { (entry: (key: AXUIElement, value: Set<String>)) -> (AXUIElement, Element)? in
                guard entry.value.contains(kAXValueChangedNotification), entry.value.isDisjoint(with: invalidating) else { return nil }
                return (entry.key, read(entry.key, [kAXRoleAttribute, kAXValueAttribute, kAXNumberOfCharactersAttribute, kAXTopLevelUIElementAttribute]).element)
            }
        )
        let scrollBars: [AXUIElement: Element] = changed.filter { (entry: (key: AXUIElement, value: Element)) -> Bool in entry.value.role == kAXScrollBarRole }
        scrolled.formUnion(scrollBars.values.compactMap { (bar: Element) -> Int? in bar.facts[kAXTopLevelUIElementAttribute]?.element })
        var restated: Set<AXUIElement> = Set(scrollBars.keys)
        for (element, shown) in changed where scrollBars[element] == nil {
            guard let ref = table[element].flatMap({ id in owners[id] }), let held = controls[ref], held.control.kind.holdsValueAttribute, held.control.subrole != kAXSecureTextFieldSubrole,
                let index = open[held.container]?.controls.firstIndex(where: { (control: Control) -> Bool in control.ref == ref })
            else { continue }
            restated.insert(element)
            let fact: Fact? = shown.facts[kAXValueAttribute].flatMap { (fact: Fact) -> Fact? in fact.elements.isEmpty ? fact : nil }
            guard fact != held.control.value else { continue }
            let characters: Int? = held.control.numberOfCharacters.flatMap { _ in shown.facts[kAXNumberOfCharactersAttribute]?.integer }
            controls[ref]?.control.value = fact
            controls[ref]?.control.numberOfCharacters = characters
            open[held.container]?.controls[index].value = fact
            open[held.container]?.controls[index].numberOfCharacters = characters
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
        let refocused: Bool = marked.values.contains { (names: Set<String>) -> Bool in names.contains(kAXFocusedUIElementChangedNotification) }
        if windowsChanged || marked.isEmpty || refocused || !rewalked.isEmpty {
            let app = try application()
            windowsChanged = false
            if let element = app.facts[kAXFocusedUIElementAttribute]?.element, element != focused {
                focused = element
                if let control = owners[element], let held = controls[control] { emit(.focused(control: control, kind: held.control.kind)) }
            }
            let windows: Set<Int> = Set(app.facts[kAXWindowsAttribute]?.elements ?? [])
            let known: Set<Int> = Set(open.values.filter { container in container.parent == nil && container.kind != .menu }.compactMap { container in located[container.ref] })
            let full: Set<Int> = touched.union(windows.subtracting(known))
            if windows != known || !touched.isEmpty { _ = try merged(try hierarchy(of: app, full: full)) }
        }
        for (element, names) in marked where names.contains(kAXUIElementDestroyedNotification) {
            if let id = table.removeValue(forKey: element) { handles[id] = nil }
        }
    }

    @discardableResult
    func beat() -> InstanceState {
        let subject: String = applicationName
        let noticedNow: [AXUIElement: Set<String>] = self.marked.withLock { held in exchange(&held, with: [:]) }
        let marked: [AXUIElement: Set<String>] = exchange(&postponed, with: [:]).merging(noticedNow) { kept, read in kept.union(read) }
        let announced: [(text: String, priority: InstanceEvent.Change.Priority?)] = noticed.withLock { noticed in exchange(&noticed.announced, with: []) }
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
            let standing: Bool = state[case: \.running]?.status.is(\.unresponsive) == true
            let inFlight: Duration? = [unanswered, sampling].compactMap(\.self).min().map { (began: ContinuousClock.Instant) -> Duration in began.duration(to: .now) }
            let stalled: Bool =
                switch (pending, inFlight) {
                    case (.none, _): false
                    case (.some, .none): true
                    case (.some(let age), .some(let asked)): standing || asked < age
                }
            if held { postponed = marked }
            do throws(ApplicationFailure) {
                if !held, instance.application.activationPolicy != .prohibited || !marked.isEmpty { try reconcile(marked) }
                let written: AXError? =
                    switch (held, driving) {
                        case (false, .owed):
                            try applicationElement.settable(manualAccessibilityAttribute)
                                ? AXUIElementSetAttributeValue(applicationElement, manualAccessibilityAttribute as CFString, kCFBooleanTrue) : .success
                        case (true, _), (_, .watched), (_, .answered): nil
                    }
                let refused: ApplicationFailure? = written.flatMap { answer in answer == .success ? nil : applicationElement.failure(answer, reading: manualAccessibilityAttribute) }
                if let refused, refused.reason.is(\.unanswered) || refused.reason.is(\.unansweredMayHaveRun) || refused.reason == .system(.noSuchProcess) { throw refused }
                if written != nil { driving = .answered }
                if let refused { send(.failure(refused)) }
            } catch  where error.reason.is(\.unanswered) || error.reason.is(\.unansweredMayHaveRun) {
                postponed = marked
            }
            let ready: Bool = instance.application.isFinishedLaunching || open.values.contains(where: \.asking)
            let status: InstanceState.Activity.Status =
                switch (stalled, ready) {
                    case (true, _): .unresponsive(thread: thread.state)
                    case (false, false): .launching
                    case (false, true): .responding
                }
            let began: ResourceUsage? = state[case: \.running]?.usage?[case: \.success]?.start
            let usage: Result<ResourceUsage.Interval, Errno>? =
                stalled
                ? Result { () throws(Errno) -> ResourceUsage.Interval in
                    let now = try ResourceUsage(of: instance.process)
                    return now.since(began ?? now)
                } : nil
            return InstanceState.Activity(
                status: status,
                pendingEventAge: pending,
                active: activation.active,
                windows: windows(blocking: stalled),
                usage: usage,
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

    package func model(_ scope: Model.Scope, known: Model.Known? = nil) async throws(ApplicationFailure) -> Model {
        while !Watch.stepping, steps.running > 0 { await withCheckedContinuation { (waiter: CheckedContinuation<Void, Never>) in steps.waiting.append(waiter) } }
        beat()
        switch scope {
            case .application:
                let verified: Set<String> = known?.build == build ? known?.containers ?? [] : []
                var menus: Bool = bars.isEmpty
                for bar in bars where !verified.contains(try bar.locator.text()) { menus = true }
                let app: Element = try application()
                var kept: Set<Int> = []
                for container in open.values where container.parent == nil {
                    if verified.contains(try container.locator.text()), let element = located[container.ref], !scrolled.contains(element) { kept.insert(element) }
                }
                let windows: Set<Int> = Set(app.facts[kAXWindowsAttribute]?.elements ?? []).subtracting(kept)
                var read: Hierarchy = try hierarchy(of: app, full: windows, menus: menus)
                read.recognized = try await recognizeText(in: read.flyouts.map(\.number), labels: known?.labels ?? [])
                let model: Model = try merged(read)
                let fresh: [Container] = model.containers.filter { (container: Container) -> Bool in root(of: container.locator).map(kept.contains) != true }
                let held: [Container] = open.values.filter { (container: Container) -> Bool in container.kind != .menu && root(of: container.locator).map(kept.contains) == true }
                let containers: [Container] = fresh + held
                return Model(sequence: model.sequence, containers: containers, located: model.located, pointer: Pointer(of: instance.application, windows: read.windows))
            case .container(let ref):
                guard let within = open[ref]?.locator, let root = root(of: within) else { throw stale(ref) }
                let model = try merged(try hierarchy(of: try application(), full: [root]))
                return Model(
                    sequence: model.sequence,
                    containers: model.containers.filter { container in container.locator.steps.starts(with: within.steps) },
                    located: model.located,
                )
            case .menu(let ref):
                return try level(of: ref)
            case .rows(let ref, let from, let count):
                guard let multi = located[ref], let element = handles[multi], let held = controls[ref], let root = root(of: held.control.locator) else { throw stale(ref) }
                let scrolling: [String] = [kAXParentAttribute, kAXVerticalScrollBarAttribute, kAXHorizontalScrollBarAttribute]
                let ancestors: [Element] = Array(
                    Swift.sequence(first: read(element, scrolling).element) { (node: Element) -> Element? in
                        let parent: AXUIElement? = node.facts[kAXParentAttribute]?.element.flatMap { (id: Int) -> AXUIElement? in self.handles[id] }
                        return parent.map { (parent: AXUIElement) -> Element in self.read(parent, scrolling).element }
                    }
                    .prefix(while: { (node: Element) -> Bool in node.id != intern(applicationElement) })
                )
                let positions: [(bar: AXUIElement, value: CFTypeRef?)] = ancestors.flatMap { (node: Element) -> [Int] in
                    [kAXVerticalScrollBarAttribute, kAXHorizontalScrollBarAttribute].compactMap { (attribute: String) -> Int? in node.facts[attribute]?.element }
                }
                .compactMap { (id: Int) -> AXUIElement? in handles[id] }
                .map { (bar: AXUIElement) -> (bar: AXUIElement, value: CFTypeRef?) in
                    var value: CFTypeRef?
                    _ = unsafe AXUIElementCopyAttributeValue(bar, kAXValueAttribute as CFString, &value)
                    return (bar, value)
                }
                var listed: CFArray?
                let answer = unsafe AXUIElementCopyAttributeValues(element, kAXRowsAttribute as CFString, from, count, &listed)
                guard answer == .success else { throw element.failure(answer, reading: kAXRowsAttribute) }
                let rows: [Int] = listed.flatMap { list in Fact(list, interning: intern) }?.elements ?? []
                let model = Model(try hierarchy(of: try application(), full: [root], rows: (multi, rows)), sequence: sequence, ref: intern)
                for (bar, value) in positions {
                    var now: CFTypeRef?
                    _ = unsafe AXUIElementCopyAttributeValue(bar, kAXValueAttribute as CFString, &now)
                    guard let value, let now, !CFEqual(value, now) else { continue }
                    let restored: AXError = AXUIElementSetAttributeValue(bar, kAXValueAttribute as CFString, value)
                    let subject = "Restoring the scroll position after reading rows of \(applicationName)"
                    guard restored == .success else { throw ApplicationFailure(restored, subject: subject, process: instance.process.pid) }
                }
                return Model(sequence: sequence, containers: model.containers.filter { read in read.ref == held.container }, located: model.located)
            case .text(let ref):
                guard let id = located[ref], let element = handles[id], let held = controls[ref], let root = root(of: held.control.locator) else { throw stale(ref) }
                var read = try hierarchy(of: try application(), full: [root])
                read.runs[id] = try element.runs(interning: intern)
                let model = Model(read, sequence: sequence, ref: intern)
                return Model(sequence: sequence, containers: model.containers.filter { container in container.ref == held.container }, located: model.located)
            case .menus:
                return Model(sequence: sequence, containers: bars + levels.values, located: [:])
        }
    }

    private func level(of ref: Int) throws(ApplicationFailure) -> Model {
        guard let item = located[ref], handles[item] != nil, let opener = controls[ref] else { throw stale(ref) }
        var level = Model(sequence: sequence, containers: [], located: [:])
        for _ in 0..<2 where level.containers.first?.controls.isEmpty != false {
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
            level = Model(hierarchy, sequence: sequence, under: Dictionary(menus.map { menu in (menu, opener.control.locator) }) { first, _ in first }, ref: intern)
        }
        if let held = level.containers.first, !held.controls.isEmpty {
            levels[ref] = held
            locate(level)
            try observe(held, opener: opener)
        }
        return level
    }

    // --- [ACTIVATION]

    package enum Presentation: CaseTagged, Equatable, Sendable {
        case behind
        case forward
        case restored(back: Bool)
    }

    package func forward(clock: some Clock<Duration> = SuspendingClock()) async throws(ApplicationFailure) -> Presentation {
        let subject = "Bringing \(applicationName) forward"
        let (ahead, activations): (Bool, AsyncStream<Message>) = (instance.application.isActive, messages())
        try PermittedFrontList.shared.admit(instance.process.pid).get()
        let front: NSRunningApplication = focusedProcesses().front.flatMap(NSRunningApplication.init(processIdentifier:)) ?? .current
        guard ahead || instance.application.activate(from: front, options: []) else { throw ApplicationFailure(subject: subject, reason: .refused) }
        let seen: Void? = if ahead { () } else { try await next({ (change: InstanceEvent.Change) -> Void? in change[case: \.frontmost] == true ? () : nil }, in: activations, clock: clock) }
        guard seen != nil else { throw ApplicationFailure(subject: subject, reason: .noChangeSeen) }
        return .forward
    }

    package func consent() async {
        let process: pid_t = instance.process.pid
        let read: OSStatus = await NSAppleEventDescriptor(processIdentifier: process).consent(to: AEEventClass(kAECoreSuite), AEEventID(kAEGetData), asking: false)
        guard read == OSStatus(errAEEventWouldRequireUserConsent) else { return }
        emit(.consenting(granted: nil))
        let answer: OSStatus = await NSAppleEventDescriptor(processIdentifier: process).consent(to: AEEventClass(kAECoreSuite), AEEventID(kAEGetData), asking: true)
        emit(.consenting(granted: answer == OSStatus(noErr)))
    }

    // --- [MENUS]

    private func track(_ menu: AXUIElement) throws(ApplicationFailure) {
        let head: Element = read(menu, frameAttributes + [kAXParentAttribute, kAXTopLevelUIElementAttribute]).element
        guard let frame = head.frame, !frame.isEmpty else { return }
        let row: InstanceState.Window? = rows.filter { row in row.onScreen && row.band == .popUpMenu && row.frame.contains(frame) }
            .min { first, second in first.frame.width * first.frame.height < second.frame.width * second.frame.height }
        let opener: Int? = [kAXParentAttribute, kAXTopLevelUIElementAttribute].compactMap { attribute in head.facts[attribute]?.element.flatMap { id in owners[id] } }.first
        let (elements, truncated) = try walk([(id: head.id, reach: .menu, depth: 2)], rows: nil)
        let shown: [Element] = elements.values.map { (element: Element) -> Element in
            guard element.id == head.id else { return element }
            var placed: Element = element
            placed.window = row?.number
            return placed
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
        clock: some Clock<Duration> = SuspendingClock(),
        _ body: ([Container]) async throws(ApplicationFailure) -> Value,
    ) async throws(ApplicationFailure) -> (value: Value, presentation: Presentation?) {
        guard let ref else { return (try await body([]), nil) }
        let process: pid_t = instance.process.pid
        let front: NSRunningApplication? = focusedProcesses().front.flatMap(NSRunningApplication.init(processIdentifier:))
        var levels: [(surface: Container?, presentation: Presentation?)] = []
        let value: Result<Value, ApplicationFailure> = await Result { () async throws(ApplicationFailure) -> Value in
            while let level = try await self.popUp(toward: ref, by: opening, past: levels.count, clock: clock) { levels.append(level) }
            return try await body(levels.compactMap(\.surface))
        }
        let opened: Int? = levels.first { level in level.surface?.kind == .menu && level.presentation != nil }?.surface?.ref
        let closed: Result<Void, ApplicationFailure> = await withTaskCancellationShield { await Result { () async throws(ApplicationFailure) in try await self.cancel(opened, clock: clock) } }
        let standing: Bool = levels.contains { level in level.surface?.kind != .menu }
        let restoring: Result<Bool?, ApplicationFailure> = await withTaskCancellationShield {
            await Result { () async throws(ApplicationFailure) -> Bool? in
                guard !standing, let front, front.processIdentifier != process, focusedProcesses().front == process else { return nil }
                let activations = await self.messages()
                guard front.activate(from: self.instance.application, options: []) else { return false }
                let receded: Void? = try await self.next({ (change: InstanceEvent.Change) -> Void? in change[case: \.frontmost] == false ? () : nil }, in: activations, clock: clock)
                return receded != nil
            }
        }
        let back: Bool? =
            switch restoring {
                case .success(let back): back
                case .failure: false
            }
        let presented: Value = try value.get()
        try closed.get()
        let shown: [Presentation] = levels.compactMap(\.presentation)
        let ahead: Bool = standing && !shown.isEmpty && front?.processIdentifier != process && focusedProcesses().front == process
        let stated: Presentation? = ahead ? Presentation.forward : shown.first { (held: Presentation) -> Bool in held == .forward } ?? shown.first
        let presentation: Presentation? = back.map(Presentation.restored(back:)) ?? stated
        return (presented, presentation)
    }

    private func popUp(toward ref: Int, by opening: String?, past count: Int, clock: some Clock<Duration>) async throws(ApplicationFailure) -> (surface: Container?, presentation: Presentation?)? {
        guard locators.indices.contains(ref - 1) else { throw stale(ref) }
        let openers: [(control: Control, action: String)] = Array(openers(along: locators[ref - 1].steps, by: opening))
        guard openers.indices.contains(count) else { return nil }
        let (opener, action): (Control, String) = openers[count]
        let extra: Bool = bars.contains { bar in bar.kind == .statusItem && opener.locator.steps.starts(with: bar.locator.steps) }
        guard !extra || opening != nil || opener.ref != ref else { return nil }
        let standing: Container? = open.values.first { (held: Container) -> Bool in
            held.kind == .menu ? held.locator.steps.dropLast().elementsEqual(opener.locator.steps) : openedBy[held.ref]?.control.ref == opener.ref
        }
        if let standing { return (standing, nil) }
        let element: AXUIElement = try await resolve(opener.ref)
        let shown: Element = read(element, frameAttributes).element
        guard action != kAXShowMenuAction || shown.actions.contains(kAXShowMenuAction) else { throw ApplicationFailure(subject: "\(kAXShowMenuAction) on \(element.name)", reason: .refused) }
        let process: pid_t = instance.process.pid
        let presentation: Presentation = shown.frame?.isEmpty == false ? .behind : .forward
        guard presentation == .behind || !extra && !instance.application.isActive else {
            throw ApplicationFailure(subject: "\(frameAttribute) of \(element.name)", reason: .accessibility(.noValue))
        }
        if presentation == .forward { _ = try await forward(clock: clock) }
        let events = messages()
        let hosted = { () -> Set<CGWindowID> in
            let listed: [pid_t: [InstanceState.Window]] = InstanceState.Window.all([.optionOnScreenOnly, .excludeDesktopElements])
            return Set(listed.filter { (entry: (key: pid_t, value: [InstanceState.Window])) -> Bool in entry.key == process || responsiblePID(entry.key) == process }.values.joined().map(\.number))
        }
        let before: Set<CGWindowID> = hosted()
        let menus: AsyncStream<Void> = notices(of: kAXMenuOpenedNotification)
        pressing(opener.ref)
        defer { pressing(nil) }
        let hold = try await PermittedFrontList.shared.holding(
            excluding: { application in application.processIdentifier == process },
            { () async -> (pressing: Task<Result<Void, ApplicationFailure>, Never>, menu: Bool) in
                let pressing: Task<Result<Void, ApplicationFailure>, Never> = Task(executorPreference: self.queue) {
                    Result { () throws(ApplicationFailure) in try Watch.press(element, action: action) }
                }
                let menu: Void? = await race(
                    { () async -> Void? in await menus.first { _ in true } },
                    against: { () async -> Void? in
                        _ = await pressing.value
                        return nil
                    },
                )
                return (pressing, menu != nil)
            },
        )
        let raced: (pressing: Task<Result<Void, ApplicationFailure>, Never>, menu: Bool) = hold.value.get()
        let answer: Result<Void, ApplicationFailure> = await raced.pressing.value
        try hold.released.get()
        try answer.get()
        let tied: @Sendable (InstanceEvent.Change) -> Void? = Watch.evidence(window: shown.window, control: opener.ref, listed: before)
        let change: InstanceEvent.Change? = try await next(
            { (change: InstanceEvent.Change) -> InstanceEvent.Change? in
                let answered: Bool = change.is(\.opened) || tied(change) != nil
                return answered && change[case: \.window]?.window.band != .popUpMenu ? change : nil
            },
            in: events,
            clock: clock,
        )
        let window: CGWindowID? = change?[case: \.window]?.window.number
        if window != nil { beat() }
        let container: Int? = change?[case: \.opened]?.container ?? change?[case: \.asking]?.container
        let surface: Container? =
            container.flatMap { container in open[container] }
            ?? window.flatMap { number in open.values.first { (held: Container) -> Bool in held.parent == nil && held.window == number } }
        if change != nil || raced.menu || !hosted().isSubset(of: before) { return (surface, presentation) }
        let cancelled: AXError? = shown.actions.contains(kAXCancelAction) ? AXUIElementPerformAction(element, kAXCancelAction as CFString) : nil
        let subject: String =
            switch cancelled {
                case .some(let answer) where answer != .success: "\(action) on \(element.name) \(presentation), with \(kAXCancelAction) answering AXError \(answer.rawValue),"
                case .some, .none: "\(action) on \(element.name) \(presentation)"
            }
        throw ApplicationFailure(subject: subject, reason: .noChangeSeen)
    }

    private func openers(along steps: [Locator.Step], by opening: String?) -> some Sequence<(control: Control, action: String)> {
        steps.indices.lazy.compactMap { (index: Int) -> (control: Control, action: String)? in
            let control: Control? = self.numbers[Locator(steps: Array(steps[...index]))].flatMap { ref in self.controls[ref]?.control }
            return switch (control, index < steps.count - 1, opening) {
                case (.some(let control), false, .some(let action)): (control, action)
                case (.some(let control), _, _) where control.opens != nil || pressOpened.contains(control.role): (control, kAXPressAction)
                case (.some(let control), true, _) where steps[index + 1].role == kAXMenuRole: (control, kAXShowMenuAction)
                case (.some, _, _), (.none, _, _): nil
            }
        }
    }

    private func cancel(_ level: Int?, clock: some Clock<Duration>) async throws(ApplicationFailure) {
        guard let level, open[level] != nil, let element = located[level].flatMap({ id in handles[id] }) else { return }
        let events = messages()
        let answer = AXUIElementPerformAction(element, kAXCancelAction as CFString)
        guard try await next({ (change: InstanceEvent.Change) -> Void? in change[case: \.closed]?.container == level ? () : nil }, in: events, clock: clock) != nil else {
            let subject = "\(kAXCancelAction) on \(element.name)"
            throw answer == .success
                ? ApplicationFailure(subject: subject, reason: .noChangeSeen)
                : ApplicationFailure(answer, subject: subject, process: instance.process.pid)
        }
    }

    nonisolated func next<Matched: Sendable>(
        _ change: @escaping @Sendable (InstanceEvent.Change) -> Matched?,
        in events: AsyncStream<Message>,
        clock: some Clock<Duration>,
        within limit: Duration = messagingTimeout,
    ) async throws(ApplicationFailure) -> Matched? {
        let subject = "waiting on \(applicationName)"
        return try await race(
            { () async throws(ApplicationFailure) -> Matched? in
                var replayed: Int?
                for await event in events.compactMap({ (message: Message) -> InstanceEvent? in message[case: \.event] }) {
                    let first: Int = replayed ?? event.sequence
                    replayed = first
                    guard event.sequence > first, let matched = change(event.change) else { continue }
                    return matched
                }
                throw ApplicationFailure(subject: subject, reason: Task.isCancelled ? .cancelled : .system(.noSuchProcess))
            },
            against: { () async throws(ApplicationFailure) -> Matched? in
                do { try await clock.sleep(for: limit) } catch { throw ApplicationFailure(subject: subject, reason: .cancelled) }
                return nil
            },
        )
    }

    // --- [ACTIONS]

    nonisolated static func evidence(window aimed: CGWindowID?, control ref: Int?, listed windows: Set<CGWindowID>) -> @Sendable (InstanceEvent.Change) -> Void? {
        { (change: InstanceEvent.Change) -> Void? in
            switch change {
                case .opened(_, let kind, _): [Container.Kind.menu, .popover, .sheet, .dialog, .prompt, .drawer].contains(kind) ? () : nil
                case .closed, .asking: ()
                case .window(let change, let window):
                    window.number == aimed || (windows.contains(window.number) ? change == .destroyed || !window.onScreen : window.onScreen) ? () : nil
                case .focused(let control, _), .value(let control, _, _), .selection(let control, _, _): control == ref ? () : nil
                case .started, .consenting, .moved, .frontmost, .hidden, .status, .announced, .dragging, .proven, .reported, .sampled, .exited: nil
            }
        }
    }

    nonisolated static func press(_ element: AXUIElement, action: String) throws(ApplicationFailure) {
        let answers: [AXError] = [
            AXUIElementSetMessagingTimeout(element, Float(messagingTimeout / 2 / .seconds(1))), AXUIElementPerformAction(element, action as CFString), AXUIElementSetMessagingTimeout(element, 0),
        ]
        guard let refused = answers.first(where: { (answer: AXError) -> Bool in ![.success, .cannotComplete].contains(answer) }) else { return }
        throw ApplicationFailure(subject: "\(action) on \(element.name)", reason: .accessibility(refused))
    }

    func pressing(_ ref: Int?) { pressed = ref.flatMap { (ref: Int) -> (control: Control, container: Int)? in controls[ref] } }

    nonisolated func step<Value: Sendable, Failure: Error>(_ body: @escaping @Sendable () async throws(Failure) -> Value) async throws(Failure) -> Value {
        await hold(1)
        let session: AsyncStream<Message> = await messages()
        let outcome: Result<Value, Failure> = await withTaskGroup(of: Result<Value, Failure>?.self) { (group: inout TaskGroup<Result<Value, Failure>?>) -> Result<Value, Failure> in
            group.addTask { await Watch.$stepping.withValue(true) { await Result(catching: body) } }
            group.addTask {
                _ = await session.first { (message: Message) -> Bool in message[case: \.event]?.change.is(\.exited) == true }
                return nil
            }
            for await finished in group {
                group.cancelAll()
                if let finished { return finished }
            }
            preconditionFailure("Step group ended without the step's outcome")
        }
        await hold(-1)
        return try outcome.get()
    }

    private func hold(_ change: Int) {
        steps.running += change
        guard steps.running == 0 else { return }
        for waiter in exchange(&steps.waiting, with: []) { waiter.resume() }
        guard !exchange(&stepWindows, with: []).isEmpty else { return }
        reordered = true
        windowTicks.continuation.yield()
    }

    func invalidate(window number: CGWindowID) {
        scrolled.formUnion(open.values.filter { (container: Container) -> Bool in container.parent == nil && container.window == number }.compactMap { container in located[container.ref] })
    }

    func perform<Value: Sendable>(on ref: Int?, _ body: @Sendable (AXUIElement?) throws(ApplicationFailure) -> Value) async throws(ApplicationFailure) -> Value {
        let element: AXUIElement? = if let ref { try await resolve(ref) } else { nil }
        return try body(element)
    }

    private func resolve(_ ref: Int) async throws(ApplicationFailure) -> AXUIElement {
        guard locators.indices.contains(ref - 1) else { throw stale(ref) }
        let live = { () -> AXUIElement? in self.located[ref].flatMap { id in self.handles[id] } }
        let steps: [Locator.Step] = locators[ref - 1].steps
        let within: Model.Scope? = open.values.last { (container: Container) -> Bool in steps.starts(with: container.locator.steps) }
            .map { (container: Container) -> Model.Scope in .container(ref: container.ref) }
        let scope: Model.Scope? = within ?? (open.isEmpty && bars.isEmpty ? .application : nil)
        if live() == nil, let scope { _ = try await model(scope) }
        for (opener, _) in openers(along: locators[ref - 1].steps, by: nil) where live() == nil && opener.ref != ref && opener.opens != nil && located[opener.ref].flatMap({ id in handles[id] }) != nil
        {
            _ = try level(of: opener.ref)
        }
        let row: (control: Control, container: Int)? = controls[ref].flatMap { (held: (control: Control, container: Int)) -> (control: Control, container: Int)? in
            held.control.kind == .row ? held : held.control.parent.flatMap { (parent: Int) -> (control: Control, container: Int)? in self.controls[parent] }
        }
        let list: AXUIElement? = row.flatMap { (row: (control: Control, container: Int)) -> Int? in self.lists[row.control.ref] }.flatMap { (list: Int) -> Int? in self.located[list] }
            .flatMap { (id: Int) -> AXUIElement? in self.handles[id] }
        if live() == nil, let row, let index = row.control.index, let list {
            var rows: CFArray?
            let read: AXError = unsafe AXUIElementCopyAttributeValues(list, kAXRowsAttribute as CFString, index, 1, &rows)
            let fetched: Int? = rows.flatMap { (rows: CFArray) -> Fact? in Fact(rows, interning: intern) }?.elements.first
            let shown: AXError? = fetched.flatMap { (id: Int) -> AXUIElement? in self.handles[id] }.map { (row: AXUIElement) -> AXError in
                AXUIElementPerformAction(row, NSAccessibility.Action.scrollToVisibleAction.rawValue as CFString)
            }
            if read == .success, shown == .success { _ = try await model(.container(ref: row.container)) }
        }
        guard let element = live() else { throw stale(ref) }
        return element
    }

    func frame(of ref: Int) async throws(ApplicationFailure) -> (window: CGWindowID?, frame: CGRect) {
        if let shown = drawn[ref] {
            guard rows.contains(where: { row in row.number == shown.window }) else { throw stale(ref) }
            return (shown.window, shown.frame)
        }
        let element = try await resolve(ref)
        guard let frame = read(element, frameAttributes).element.frame else { throw ApplicationFailure(subject: "\(frameAttribute) of \(element.name)", reason: .accessibility(.noValue)) }
        return (open[controls[ref]?.container ?? ref]?.window, frame)
    }

    nonisolated func restless(of number: CGWindowID) -> [CGRect] { regions.withLock { held in held[number] ?? [] } }

    nonisolated func rest(_ dirty: [CGRect], in number: CGWindowID) {
        regions.withLock { held in
            for region in dirty {
                let kept: [CGRect] = held[number] ?? []
                let meeting: [CGRect] = kept.filter { (other: CGRect) -> Bool in other.intersects(region) }
                let apart: [CGRect] = kept.filter { (other: CGRect) -> Bool in !other.intersects(region) }
                held[number] = apart + [meeting.reduce(region) { (joint: CGRect, other: CGRect) -> CGRect in joint.union(other) }]
            }
        }
    }

    func frames(of number: CGWindowID, during call: AsyncStream<Void>) async -> Result<StreamOutput, ApplicationFailure> {
        let shown: Set<CGWindowID> = Set(rows.map(\.number))
        let (opened, opening) = AsyncStream<Result<StreamOutput, ApplicationFailure>>.makeStream(bufferingPolicy: .bufferingOldest(1))
        regions.withLock { held in held = held.filter { (entry: (key: CGWindowID, value: [CGRect])) -> Bool in shown.contains(entry.key) } }
        tasks.append(Task.detached { await self.proving(number, during: call, into: opening) })
        return await opened.first { _ in true } ?? .failure(ApplicationFailure(subject: applicationName, reason: .cancelled))
    }

    func faults(since began: Date) async -> [LogEntry] {
        do throws(ProcessFailure) {
            return try await LogEntry.history(of: .process(instance.process.pid), since: began, limit: .max, levels: [.fault])
        } catch {
            send(.failure(ApplicationFailure(subject: "Diagnostics of \(applicationName)", reason: .foundation(error as NSError))))
            return []
        }
    }

    var answerTime: Duration { min(latency ?? messagingTimeout, messagingTimeout) }

    func prove(after elapsed: Duration) { latency = max(latency ?? .zero, elapsed) }

    func follow(_ late: @escaping @Sendable () async -> (drawn: Bool, after: Duration)?) -> Int {
        followed += 1
        let id: Int = followed
        tasks.append(
            Task {
                guard let seen = await late() else { return }
                self.prove(after: seen.after)
                self.emit(.proven(followed: id, drawn: seen.drawn))
            }
        )
        return id
    }

    func documents() -> [CGWindowID: URL] {
        Dictionary(open.values.compactMap { container in container.window.flatMap { window in container.document.map { document in (window, document) } } }) { first, _ in first }
    }

    func addOpenCommandDocuments(_ documents: [URL]) { openCommandDocuments.append(contentsOf: documents) }

    package var documentFiles: [URL] { openCommandDocuments.union(open.values.compactMap(\.document)).elements.filter(\.isFileURL) }

    private func stale(_ ref: Int) -> ApplicationFailure {
        let steps: [Locator.Step] = locators.indices.contains(ref - 1) ? locators[ref - 1].steps : []
        let reached: Container? = open.values.lazy.filter { container in steps.starts(with: container.locator.steps) }.max { first, second in first.locator.steps.count < second.locator.steps.count }
        let control: Control? = controls[ref]?.control
        let role: String =
            control.flatMap { control in NSAccessibility.Role(rawValue: control.role).description(with: control.subrole.map(NSAccessibility.Subrole.init(rawValue:))) } ?? "reference"
        let subject: String = (control?.label?.text ?? open[ref]?.title).map { label in "\(role) \(String(reflecting: label))" } ?? role
        return ApplicationFailure(subject: subject, reason: .notOpen(nearest: reached?.controls.compactMap(\.label?.text) ?? []))
    }
}
