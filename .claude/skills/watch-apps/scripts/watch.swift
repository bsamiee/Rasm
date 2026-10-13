import AppKit
import ApplicationServices

// --- [MODELS] --------------------------------------------------------------------------
enum Kind: String, Encodable { case watch, launch, exit, window, dialog, menu, closed, fail }
struct Event: Encodable {
    let app: String?
    var pid: pid_t?
    let event: Kind
    var role: String?
    var text: [String]?
    var buttons: [String]?
    var pressed: String?
    var status: Int?
}

// --- [SERVICES] ------------------------------------------------------------------------
@MainActor final class Watch {
    let (app, log, follow): (String, Int32, any DispatchSourceFileSystemObject)
    var (held, observers, shown): (Bool, [pid_t: AXObserver], [AXUIElement: Event]) = (false, [:], [:])
    init(app: String) {
        let path: String = FileManager.default.temporaryDirectory.appending(path: "\(app).events").path(percentEncoded: false)
        let reader: Int32 = unsafe open(path, O_RDONLY | O_CREAT, 0o600)
        (self.app, log) = (app, unsafe open(path, O_WRONLY | O_APPEND))
        follow = DispatchSource.makeFileSystemObjectSource(fileDescriptor: reader, eventMask: .extend)
        lseek(reader, 0, SEEK_END)
        follow.setEventHandler { output.continuation.yield((FileHandle(fileDescriptor: reader).availableData, STDOUT_FILENO)) }
        if !once { follow.activate() }
        contend()
    }
    func emit(_ event: Event) { send(event, held ? log : STDOUT_FILENO) }
    func contend() {
        var lock: flock = flock(l_start: 0, l_len: 0, l_pid: 0, l_type: Int16(F_WRLCK), l_whence: Int16(SEEK_SET))
        held = unsafe fcntl(log, once ? F_GETLK : F_SETLK, &lock) == 0 && !once
        if !once { emit(Event(app: app, event: .watch, text: held ? flags : nil)) }
        for instance: NSRunningApplication in NSRunningApplication.runningApplications(withBundleIdentifier: app) { change(instance, .watch) }
        guard !held, !once, unsafe fcntl(log, F_GETLK, &lock) == 0 else { return }
        if lock.l_type == F_WRLCK { exited(lock.l_pid, contend) } else { contend() }
    }
    func change(_ instance: NSRunningApplication, _ kind: Kind, _ status: Int? = nil) {
        let (pid, application): (pid_t, AXUIElement) = (instance.processIdentifier, AXUIElementCreateApplication(instance.processIdentifier))
        var created: AXObserver?
        unsafe AXObserverCreate(pid, observed, &created)
        observers[pid] = held && kind != .exit ? created : nil
        let refcon: UnsafeMutableRawPointer = unsafe Unmanaged.passUnretained(self).toOpaque()
        let failed: [String] = (held && kind != .exit ? notifications.keys.sorted() : []).filter { name in
            observers[pid].map { observer in
                unsafe ApplicationServices.AXObserverAddNotification(observer, application, name as CFString, refcon)
            } != .success
        }
        if held, kind != .watch { emit(Event(app: app, pid: pid, event: kind, status: status)) }
        if !failed.isEmpty { emit(Event(app: app, pid: pid, event: .fail, text: failed)) }
        observers[pid].map { observer in CFRunLoopAddSource(CFRunLoopGetMain(), AXObserverGetRunLoopSource(observer), .defaultMode) }
        for element: AXUIElement in kind == .exit ? [] : application.elements { report(element, pid, acting: held || once) }
    }
    func report(_ element: AXUIElement, _ pid: pid_t, acting: Bool) {
        let role: String? = element[kAXSubroleAttribute] ?? element.role
        let kind: Kind = role.flatMap { role in kinds[role] } ?? element.role.flatMap { role in kinds[role] } ?? .window
        let nodes: [AXUIElement] = kind == .dialog ? element.descendants : []
        let lines: [String?] =
            [(kind == .menu ? element[kAXParentAttribute] : element)?.label]
            + nodes.map { node in node.role == kAXStaticTextRole ? node[kAXValueAttribute] : nil }
        let text: [String] = lines.compactMap(\.self).filter { line in !line.isEmpty }
        let close: [(AXUIElement, String)] =
            kind == .menu ? [(element, kAXCancelAction)] : element[kAXCloseButtonAttribute].map { [($0, kAXCloseButtonAttribute)] } ?? []
        let buttons: [(AXUIElement, String)] = nodes.compactMap { node in node.role == kAXButtonRole ? node.label.map { (node, $0) } : nil } + close
        let choice: (AXUIElement, String)? =
            (dismiss && kind == .dialog && buttons.count == 1 ? buttons.first : nil)
            ?? buttons.first { _, label in answers.contains { pair in pair.last == label[...] && text.contains { line in line.contains(pair[0]) } } }
        let action: CFString = (kind == .menu ? kAXCancelAction : kAXPressAction) as CFString
        let pressed: Bool = acting && choice.map { button, _ in AXUIElementPerformAction(button, action) == .success } == true
        shown[element] = Event(app: app, pid: pid, event: kind, role: role, text: text, buttons: buttons.map(\.1), pressed: pressed ? choice?.1 : nil)
        shown[element].map(emit)
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
extension AXUIElement {
    subscript<Value>(_ attribute: String) -> Value? {
        var value: CFTypeRef?
        return unsafe AXUIElementCopyAttributeValue(self, attribute as CFString, &value) == .success ? value as? Value : nil
    }
    var role: String? { self[kAXRoleAttribute] }
    var children: [AXUIElement] { self[kAXChildrenAttribute] ?? [] }
    var descendants: [AXUIElement] { [self] + children.flatMap(\.descendants) }
    var elements: [AXUIElement] {
        children.filter { top in top.role != kAXMenuBarRole }.flatMap { top in [top] + top.children.filter { child in child.role == kAXSheetRole } }
    }
    var label: String? { [kAXTitleAttribute, kAXDescriptionAttribute].lazy.compactMap { self[$0] as String? }.first { label in !label.isEmpty } }
}
func send(_ event: Event, _ sink: Int32) { output.continuation.yield(with: Result { (try encoder.encode(event) + [10], sink) }) }
func exited(_ pid: pid_t, _ then: @escaping @MainActor () -> Void) {
    let source: any DispatchSourceProcess = DispatchSource.makeProcessSource(identifier: pid, eventMask: .exit, queue: .main)
    source.setEventHandler { source.cancel() }
    source.setCancelHandler { MainActor.assumeIsolated(then) }
    source.activate()
}
let observed: AXObserverCallback = { _, element, notification, info in
    MainActor.assumeIsolated {
        let watch: Watch? = unsafe info.map { info in unsafe Unmanaged<Watch>.fromOpaque(info).takeUnretainedValue() }
        let (closing, ended): (Bool, Event?) = (notifications[notification as String] == true, watch?.shown[element])
        var pid: pid_t = 0
        if closing, let ended { watch?.emit(Event(app: ended.app, pid: ended.pid, event: .closed, role: ended.role, text: ended.text)) }
        if closing { watch?.shown[element] = nil }
        if !closing, ended == nil, unsafe AXUIElementGetPid(element, &pid) == .success { watch?.report(element, pid, acting: true) }
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------
let notifications: [String: Bool] = [
    kAXWindowCreatedNotification: false, kAXSheetCreatedNotification: false, kAXMenuOpenedNotification: false,
    kAXUIElementDestroyedNotification: true, kAXMenuClosedNotification: true,
]
let kinds: [String: Kind] = [
    kAXMenuRole: .menu, kAXSheetRole: .dialog, kAXLayoutAreaRole: .dialog, kAXDialogSubrole: .dialog, kAXSystemDialogSubrole: .dialog,
]
let flags: [String] = Array(CommandLine.arguments.dropFirst())
let (dismiss, once): (Bool, Bool) = (flags.contains("--dismiss"), flags.contains("--once"))
let answers: [[Substring]] = zip(flags, flags.dropFirst()).filter { flag, _ in flag == "--answer" }.map { _, answer in answer.split(separator: "=") }
let encoder: JSONEncoder = JSONEncoder()
let output: (stream: AsyncThrowingStream<(Data, Int32), any Error>, continuation: AsyncThrowingStream<(Data, Int32), any Error>.Continuation) =
    AsyncThrowingStream.makeStream()
encoder.outputFormatting = [.sortedKeys, .withoutEscapingSlashes]
let apps: [(name: String, ids: [String])] = zip([""] + flags, flags).filter { previous, flag in !flag.hasPrefix("--") && previous != "--answer" }
    .map { _, name in
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
    + apps.filter(\.ids.isEmpty).map { app in Event(app: app.name, event: .fail, text: ["No installed application has this bundle id or name"]) }
for event: Event in failures { send(event, STDOUT_FILENO) }
let watches: [Watch] = failures.isEmpty ? Set(apps.flatMap(\.ids)).sorted().map(Watch.init) : []
let lifecycle: [any NSObjectProtocol] = [
    NSWorkspace.didLaunchApplicationNotification: Kind.launch, NSWorkspace.didTerminateApplicationNotification: .exit,
]
.map { name, kind in
    NSWorkspace.shared.notificationCenter.addObserver(forName: name, object: nil, queue: .main) { note in
        let instance: NSRunningApplication? = note.userInfo?[NSWorkspace.applicationUserInfoKey] as? NSRunningApplication
        let status: Int? = note.userInfo?["NSWorkspaceExitStatusKey"] as? Int
        MainActor.assumeIsolated {
            let watch: Watch? = watches.first { watch in watch.held && watch.app == instance?.bundleIdentifier }
            instance.map { instance in watch?.change(instance, kind, status) }
        }
    }
}
exited(getppid()) { exit(0) }
if once || !failures.isEmpty { output.continuation.finish() }
for try await (data, sink): (Data, Int32) in output.stream { try FileHandle(fileDescriptor: sink).write(contentsOf: data) }
exit(watches.contains { watch in watch.shown.values.contains { event in event.pressed != nil } } ? 0 : 1)
