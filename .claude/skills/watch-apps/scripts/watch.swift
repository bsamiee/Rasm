import AppKit
import ApplicationServices
import System

// --- [CONSTANTS] -----------------------------------------------------------------------
let newline: UInt8 = UInt8(ascii: "\n")
let opening: [String] = [kAXWindowCreatedNotification, kAXSheetCreatedNotification, kAXMenuOpenedNotification]
let closing: [String] = [kAXUIElementDestroyedNotification, kAXMenuClosedNotification]

// --- [MODELS] --------------------------------------------------------------------------
enum Kind: String, Codable { case watch, launch, exit, window, dialog, menu, closed, answered, fail }
enum Sink {
    case output
    case log(FileHandle)
}
struct Key: Hashable {
    let pid: pid_t
    let role: String?
    let title: String?
    let text: [String]?
}
struct Event: Codable {
    let app: String?
    var pid: pid_t?
    let event: Kind
    var role: String?
    var title: String?
    var text: [String]?
    var buttons: [String]?
    var pressed: String?
    var status: Int?
    var key: Key? { pid.map { pid in Key(pid: pid, role: role, title: title, text: text) } }
    var closed: Self { Self(app: app, pid: pid, event: .closed, role: role, title: title, text: text) }
}
struct Answer {
    let text: String
    let button: String
}

// --- [SERVICES] ------------------------------------------------------------------------
@MainActor final class Watch {
    let (app, log, reader): (String, FileHandle, FileHandle)
    let follow: any DispatchSourceFileSystemObject
    var (held, buffer): (Bool, Data) = (false, Data())
    var (observers, shown, visible): ([pid_t: AXObserver], [AXUIElement: Event], [Key: Event]) = ([:], [:], [:])
    init(app: String) {
        (self.app, log, reader) = (app, logHandle(app, O_WRONLY | O_APPEND), logHandle(app, O_RDONLY))
        follow = DispatchSource.makeFileSystemObjectSource(fileDescriptor: reader.fileDescriptor, eventMask: .extend, queue: .main)
        _ = try? FileDescriptor(rawValue: reader.fileDescriptor).seek(offset: 0, from: .end)
        let opened: [Event] = NSRunningApplication.runningApplications(withBundleIdentifier: app).flatMap { instance in
            AXUIElementCreateApplication(instance.processIdentifier).elements.map { element in element.described(app, instance.processIdentifier).event }
        }
        for event: Event in [Event(app: app, event: .watch)] + opened { show(event) }
        follow.setEventHandler { MainActor.assumeIsolated { self.drain() } }
        follow.activate()
        Task {
            if let failure: Errno = await locked(log) { show(Event(app: app, event: .fail, text: ["Waiting on the log lock returned \(failure)"])) } else { take() }
        }
    }
    func take() {
        held = true
        for instance: NSRunningApplication in NSRunningApplication.runningApplications(withBundleIdentifier: app) { attach(instance, visible) }
        let seen: Set<Key> = Set(shown.values.compactMap(\.key))
        for (key, event): (Key, Event) in visible where !seen.contains(key) { output.yield((event.closed, .log(log))) }
    }
    func drain() {
        buffer.append(reader.availableData)
        while let end: Data.Index = buffer.firstIndex(of: newline) {
            let line: Data = buffer[..<end]
            buffer.removeSubrange(...end)
            let event: Event =
                switch Result(catching: { try JSONDecoder().decode(Event.self, from: line) }) {
                    case .success(let event): event
                    case .failure(let error): Event(app: app, event: .fail, text: [String(describing: error)])
                }
            show(event)
        }
    }
    func show(_ event: Event) {
        switch (event.event, event.key) {
            case (.window, let key?), (.dialog, let key?), (.menu, let key?): visible[key] = event
            case (.closed, let key?): visible[key] = nil
            case (.exit, _): visible = visible.filter { key, _ in key.pid != event.pid }
            default: break
        }
        output.yield((event, .output))
    }
    func changed(_ instance: NSRunningApplication, _ kind: Kind, _ status: Int?) {
        let pid: pid_t = instance.processIdentifier
        output.yield((Event(app: app, pid: pid, event: kind, status: status), .log(log)))
        if kind == .launch {
            attach(instance, [:])
        } else {
            (observers[pid], shown) = (nil, shown.filter { _, event in event.pid != pid })
        }
    }
    func attach(_ instance: NSRunningApplication, _ known: [Key: Event]) {
        let pid: pid_t = instance.processIdentifier
        observers[pid] = observe(instance, opening + closing, self, .log(log))
        for element: AXUIElement in AXUIElementCreateApplication(pid).elements { report(element, pid, known) }
    }
    func report(_ element: AXUIElement, _ pid: pid_t, _ known: [Key: Event]) {
        guard shown[element] == nil else { return }
        let event: Event = element.described(app, pid).event
        shown[element] = event
        if event.key.flatMap({ key in known[key] }) == nil { output.yield((event, .log(log))) }
    }
    func ended(_ element: AXUIElement) {
        guard let event: Event = shown.removeValue(forKey: element) else { return }
        output.yield((event.closed, .log(log)))
    }
}
@MainActor final class Answering {
    var pending: Answer?
    var observers: [pid_t: AXObserver] = [:]
    init(answer: Answer, ids: [String]) {
        pending = answer
        for instance: NSRunningApplication in ids.flatMap(NSRunningApplication.runningApplications(withBundleIdentifier:)) { attach(instance) }
    }
    func attach(_ instance: NSRunningApplication) {
        let pid: pid_t = instance.processIdentifier
        observers[pid] = observe(instance, opening, self, .output)
        for element: AXUIElement in AXUIElementCreateApplication(pid).elements { consider(element, pid) }
    }
    func consider(_ element: AXUIElement, _ pid: pid_t) {
        guard let answer: Answer = pending, let app: String = NSRunningApplication(processIdentifier: pid)?.bundleIdentifier else { return }
        let (event, buttons): (Event, [(AXUIElement, String)]) = element.described(app, pid)
        guard ([event.title] + (event.text ?? [])).contains(where: { line in line?.contains(answer.text) == true }) else { return }
        let action: String = event.event == .menu ? kAXCancelAction : kAXPressAction
        let result: AXError? = buttons.first { _, label in label == answer.button }.map { button, _ in AXUIElementPerformAction(button, action as CFString) }
        let outcome: Event =
            switch result {
                case .success:
                    Event(app: app, pid: pid, event: .answered, role: event.role, title: event.title, text: event.text, buttons: event.buttons, pressed: answer.button)
                case let result?:
                    Event(app: app, pid: pid, event: .fail, title: event.title, text: ["Pressing \(answer.button) returned AXError \(result.rawValue)"], buttons: event.buttons)
                case nil: Event(app: app, pid: pid, event: .fail, title: event.title, text: ["No button \(answer.button) in this element"], buttons: event.buttons)
            }
        pending = nil
        output.yield((outcome, .log(logHandle(app, O_WRONLY | O_APPEND))))
        output.yield((outcome, .output))
        output.finish()
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
extension AXUIElement {
    subscript<Value>(_ attribute: String) -> Value? {
        var value: CFTypeRef?
        return unsafe AXUIElementCopyAttributeValue(self, attribute as CFString, &value) == .success ? value as? Value : nil
    }
    var pid: pid_t? {
        var pid: pid_t = 0
        return unsafe AXUIElementGetPid(self, &pid) == .success ? pid : nil
    }
    var role: String? { self[kAXRoleAttribute] }
    var children: [AXUIElement] { self[kAXChildrenAttribute] ?? [] }
    var descendants: [AXUIElement] {
        let collections: Set<String?> = [kAXScrollAreaRole, kAXOutlineRole, kAXTableRole, kAXListRole, kAXBrowserRole]
        return [self] + children.filter { child in !collections.contains(child.role) }.flatMap(\.descendants)
    }
    var elements: [AXUIElement] {
        children.filter { top in top.role != kAXMenuBarRole }.flatMap { top in [top] + top.children.filter { child in child.role == kAXSheetRole } }
    }
    var label: String? { [kAXTitleAttribute, kAXDescriptionAttribute].lazy.compactMap { self[$0] as String? }.first { label in !label.isEmpty } }
    func described(_ app: String, _ pid: pid_t) -> (event: Event, buttons: [(AXUIElement, String)]) {
        let kinds: [String: Kind] = [
            kAXMenuRole: .menu, kAXSheetRole: .dialog, kAXLayoutAreaRole: .dialog, kAXDialogSubrole: .dialog, kAXSystemDialogSubrole: .dialog,
        ]
        let subrole: String? = self[kAXSubroleAttribute]
        let kind: Kind = subrole.flatMap { subrole in kinds[subrole] } ?? role.flatMap { role in kinds[role] } ?? .window
        let nodes: [AXUIElement] = kind == .dialog ? descendants : []
        let text: [String] = nodes.compactMap { node in node.role == kAXStaticTextRole ? node[kAXValueAttribute] as String? : nil }.filter { line in !line.isEmpty }
        let close: [(AXUIElement, String)] =
            kind == .menu ? [(self, kAXCancelAction)] : (self[kAXCloseButtonAttribute] as AXUIElement?).map { button in [(button, kAXCloseButtonAttribute)] } ?? []
        let buttons: [(AXUIElement, String)] = nodes.compactMap { node in node.role == kAXButtonRole ? node.label.map { label in (node, label) } : nil } + close
        let title: String? = kind == .menu ? (self[kAXParentAttribute] as AXUIElement?)?.label : label
        return (Event(app: app, pid: pid, event: kind, role: subrole ?? role, title: title, text: text.isEmpty ? nil : text, buttons: buttons.isEmpty ? nil : buttons.map(\.1)), buttons)
    }
}
func logHandle(_ app: String, _ mode: Int32) -> FileHandle {
    FileHandle(fileDescriptor: unsafe open(URL.temporaryDirectory.appending(path: "\(app).events").path(percentEncoded: false), mode | O_CREAT, S_IRUSR | S_IWUSR), closeOnDealloc: true)
}
@concurrent func locked(_ log: FileHandle) async -> Errno? {
    var lock: flock = flock(l_start: 0, l_len: 0, l_pid: 0, l_type: Int16(F_WRLCK), l_whence: Int16(SEEK_SET))
    return unsafe fcntl(log.fileDescriptor, F_SETLKW, &lock) == 0 ? nil : Errno(rawValue: errno)
}
@MainActor func observe(_ instance: NSRunningApplication, _ names: [String], _ owner: AnyObject, _ sink: Sink) -> AXObserver? {
    let (pid, refcon): (pid_t, UnsafeMutableRawPointer) = (instance.processIdentifier, unsafe Unmanaged.passUnretained(owner).toOpaque())
    let application: AXUIElement = AXUIElementCreateApplication(pid)
    var created: AXObserver?
    unsafe AXObserverCreate(pid, observed, &created)
    let failed: [String] = created.map { observer in names.filter { name in unsafe AXObserverAddNotification(observer, application, name as CFString, refcon) != .success } } ?? names
    if let created { CFRunLoopAddSource(CFRunLoopGetMain(), AXObserverGetRunLoopSource(created), .defaultMode) }
    if !failed.isEmpty { output.yield((Event(app: instance.bundleIdentifier, pid: pid, event: .fail, text: failed), sink)) }
    return created
}
let observed: AXObserverCallback = { _, element, notification, info in
    MainActor.assumeIsolated {
        let owner: AnyObject? = unsafe info.map { pointer in unsafe Unmanaged<AnyObject>.fromOpaque(pointer).takeUnretainedValue() }
        switch (owner, closing.contains(notification as String), element.pid) {
            case (let watch as Watch, true, _): watch.ended(element)
            case (let watch as Watch, false, let pid?): watch.report(element, pid, [:])
            case (let answering as Answering, false, let pid?): answering.consider(element, pid)
            default: break
        }
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------
let (lines, output): (AsyncStream<(Event, Sink)>, AsyncStream<(Event, Sink)>.Continuation) = AsyncStream.makeStream(of: (Event, Sink).self)
let encoder: JSONEncoder = JSONEncoder()
encoder.outputFormatting = [.sortedKeys, .withoutEscapingSlashes]
let arguments: ArraySlice<String> = CommandLine.arguments.dropFirst()
let flag: Int? = arguments.firstIndex(of: "--answer")
let answer: Answer? = flag.flatMap { at in arguments[(at + 1)...].first }
    .flatMap { pair in pair.lastIndex(of: "=").map { at in Answer(text: String(pair[..<at]), button: String(pair[pair.index(after: at)...])) } }
let names: ArraySlice<String> = flag.map { at in arguments[..<at] + arguments[(at + 1)...].dropFirst() } ?? arguments
let apps: [(name: String, ids: [String])] = names.filter { name in !name.hasPrefix("--") }
    .map { name in
        let match: String =
            "kMDItemContentType == 'com.apple.application-bundle'"
            + " && (kMDItemCFBundleIdentifier == '\(name)' || kMDItemDisplayName == '\(name)')"
        let query: MDQuery? = MDQueryCreate(nil, match as CFString, [kMDItemCFBundleIdentifier] as CFArray, nil)
        let found: Bool = MDQueryExecute(query, CFOptionFlags(kMDQuerySynchronous.rawValue))
        return (name, found ? MDQueryCopyValuesOfAttribute(query, kMDItemCFBundleIdentifier) as? [String] ?? [] : [])
    }
let failures: [Event] =
    (AXIsProcessTrusted() ? [] : [Event(app: nil, event: .fail, text: ["Accessibility access is off for this process"])])
    + (apps.isEmpty ? [Event(app: nil, event: .fail, text: ["No application named"])] : [])
    + names.filter { name in name.hasPrefix("--") }.map { flag in Event(app: nil, event: .fail, text: ["Unknown flag \(flag)"]) }
    + (flag != nil && answer == nil ? [Event(app: nil, event: .fail, text: ["--answer takes '<text>=<button>'"])] : [])
    + apps.filter(\.ids.isEmpty).map { app in Event(app: app.name, event: .fail, text: ["No installed application has this bundle id or name"]) }
let ids: [String] = failures.isEmpty ? Set(apps.flatMap(\.ids)).sorted() : []
let answering: Answering? = answer.map { answer in Answering(answer: answer, ids: ids) }
let watches: [Watch] = answering == nil ? ids.map(Watch.init) : []
let lifecycle: [any NSObjectProtocol] = [
    NSWorkspace.didLaunchApplicationNotification: Kind.launch, NSWorkspace.didTerminateApplicationNotification: .exit,
]
.map { name, kind in
    NSWorkspace.shared.notificationCenter.addObserver(forName: name, object: nil, queue: .main) { note in
        let instance: NSRunningApplication? = note.userInfo?[NSWorkspace.applicationUserInfoKey] as? NSRunningApplication
        let status: Int? = note.userInfo?["NSWorkspaceExitStatusKey"] as? Int
        MainActor.assumeIsolated {
            guard let instance, let app: String = instance.bundleIdentifier, ids.contains(app) else { return }
            if kind == .launch { answering?.attach(instance) }
            watches.first { watch in watch.held && watch.app == app }?.changed(instance, kind, status)
        }
    }
}
let parent: any DispatchSourceProcess = DispatchSource.makeProcessSource(identifier: getppid(), eventMask: .exit, queue: .main)
parent.setEventHandler { MainActor.assumeIsolated { output.finish() } }
parent.activate()
for event: Event in failures { output.yield((event, .output)) }
if !failures.isEmpty { output.finish() }
var last: Kind?
for await (event, sink): (Event, Sink) in lines {
    let line: Data = try encoder.encode(event) + [newline]
    switch sink {
        case .output: try FileHandle.standardOutput.write(contentsOf: line)
        case .log(let log): try log.write(contentsOf: line)
    }
    last = event.event
}
exit(failures.isEmpty && (answer == nil || last == .answered) ? EXIT_SUCCESS : EXIT_FAILURE)
