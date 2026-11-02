import AppKit
import Subprocess
import Synchronization
import System
import os

// --- [CONSTANTS] -----------------------------------------------------------------------

let windowSize: NSSize = NSSize(width: 400, height: 300)
let undescribedSize: NSSize = NSSize(width: 200, height: 100)
let rowHeight: CGFloat = 20

// --- [SERVICES] ------------------------------------------------------------------------

private final class Hang: Sendable {
    let released: Atomic<Bool> = Atomic(false)
    let waiting: DispatchSemaphore = DispatchSemaphore(value: 0)
    let release: any DispatchSourceSignal = DispatchSource.makeSignalSource(signal: SIGUSR1, queue: .global())

    init() {
        signal(SIGUSR1, SIG_IGN)
        release.setEventHandler { @Sendable [self] in
            released.store(true, ordering: .relaxed)
            waiting.signal()
        }
        release.activate()
    }
}

@MainActor
final class ApplicationDelegate: NSObject, NSApplicationDelegate, NSTableViewDataSource {
    private let settings: Settings
    private let logger: Logger = Logger(for: ApplicationDelegate.self)
    private let status: NSTextField = NSTextField(labelWithString: "idle")
    private var windows: [NSWindow] = []
    private var helpers: [Task<Void, Never>] = []

    init(settings: Settings) {
        self.settings = settings
    }

    // --- [LIFECYCLE]

    func applicationWillFinishLaunching(_ notification: Notification) {
        let bar: NSMenu = NSMenu()
        let application: NSMenu = NSMenu()
        application.addItem(withTitle: "Quit", action: #selector(NSApplication.terminate(_:)), keyEquivalent: "q")
        let scenarios: NSMenu = NSMenu(title: "Scenario")
        for scenario: Scenario in Scenario.allCases where !scenario.atLaunch {
            scenarios.addItem(withTitle: scenario.rawValue, action: #selector(chosen(_:)), keyEquivalent: "").target = self
        }
        scenarios.addItem(.separator())
        scenarios.addItem(withTitle: "", action: #selector(chosen(_:)), keyEquivalent: "").target = self
        scenarios.addItem(withTitle: "disabled", action: nil, keyEquivalent: "")
        for submenu: NSMenu in [application, scenarios] { bar.addItem(withTitle: submenu.title, action: nil, keyEquivalent: "").submenu = submenu }
        NSApp.mainMenu = bar
        if settings.hidden { NSApp.hide(nil) }
        windows = (0..<max(1, settings.windows)).map(window)
        if let scenario: Scenario = settings.scenario, scenario.atLaunch { run(scenario) }
    }

    func applicationDidFinishLaunching(_ notification: Notification) {
        logger.notice("Launch finished with \(self.windows.count, privacy: .public) windows")
        if let scenario: Scenario = settings.scenario, !scenario.atLaunch { run(scenario) }
    }

    func applicationWillTerminate(_ notification: Notification) {
        for helper: Task<Void, Never> in helpers { helper.cancel() }
    }

    // --- [ACTIONS]

    @objc
    private func pressed(_ sender: NSButton) {
        status.stringValue = "pressed \(sender.title)"
        logger.notice("Control \(sender.title, privacy: .public) pressed")
    }

    @objc
    private func entered(_ sender: NSTextField) {
        status.stringValue = "entered \(sender.stringValue)"
    }

    @objc
    private func chosen(_ sender: NSMenuItem) {
        if let scenario: Scenario = Scenario(rawValue: sender.title) { run(scenario) }
    }

    // --- [SCENARIOS]

    private func run(_ scenario: Scenario) {
        func alert(_ message: String) -> NSAlert {
            let alert: NSAlert = NSAlert()
            alert.messageText = message
            alert.informativeText = scenario.rawValue
            alert.addButton(withTitle: "OK")
            alert.addButton(withTitle: "Cancel")
            return alert
        }
        func panel(_ title: String, style: NSWindow.StyleMask, subrole: NSAccessibility.Subrole? = nil, level: NSWindow.Level = .normal) -> NSPanel {
            let panel: NSPanel = NSPanel(contentRect: NSRect(x: 300, y: 300, width: 300, height: 120), styleMask: style, backing: .buffered, defer: false)
            panel.title = title
            panel.level = level
            panel.hidesOnDeactivate = false
            panel.isReleasedWhenClosed = false
            subrole.map(panel.setAccessibilitySubrole)
            let dismiss: NSButton = NSButton(title: "Dismiss", target: panel, action: #selector(NSWindow.orderOut(_:)))
            dismiss.setAccessibilityIdentifier("dismiss")
            panel.contentView = dismiss
            panel.orderFront(nil)
            return panel
        }
        func undescribed() -> NSWindow {
            let window: NSWindow = NSWindow(contentRect: NSRect(origin: NSPoint(x: 500, y: 500), size: undescribedSize), styleMask: [.titled], backing: .buffered, defer: false)
            window.title = "Undescribed"
            window.isRestorable = false
            window.isReleasedWhenClosed = false
            window.setAccessibilityElement(false)
            window.orderFront(nil)
            return window
        }
        func collapsed() -> NSWindow {
            let window: NSWindow = NSWindow(contentRect: NSRect(origin: NSPoint(x: 600, y: 600), size: .zero), styleMask: [.borderless], backing: .buffered, defer: false)
            window.title = "Collapsed"
            window.isRestorable = false
            window.isReleasedWhenClosed = false
            window.orderFront(nil)
            return window
        }
        func swatch() -> NSWindow {
            let window: NSWindow = NSWindow(contentRect: NSRect(origin: .zero, size: Swatch.size), styleMask: [.titled, .closable], backing: .buffered, defer: false)
            window.title = "Swatch"
            window.isRestorable = false
            window.isReleasedWhenClosed = false
            window.colorSpace = .sRGB
            window.hasShadow = false
            window.contentView = Swatch(frame: NSRect(origin: .zero, size: Swatch.size))
            window.center()
            for shown: NSWindow in windows { shown.orderOut(nil) }
            NSApp.unhideWithoutActivation()
            window.orderFront(nil)
            return window
        }
        func table() -> NSWindow {
            let window: NSWindow = NSWindow(contentRect: NSRect(origin: .zero, size: windowSize), styleMask: [.titled], backing: .buffered, defer: false)
            window.title = "Table"
            window.isRestorable = false
            window.isReleasedWhenClosed = false
            let table: NSTableView = NSTableView()
            let column: NSTableColumn = NSTableColumn(identifier: NSUserInterfaceItemIdentifier("row"))
            column.isEditable = false
            table.addTableColumn(column)
            table.headerView = nil
            table.style = .plain
            table.rowHeight = rowHeight
            table.intercellSpacing = .zero
            table.dataSource = self
            let scroll: NSScrollView = NSScrollView(frame: NSRect(origin: .zero, size: windowSize))
            scroll.documentView = table
            scroll.hasVerticalScroller = true
            window.contentView = scroll
            window.center()
            window.orderFront(nil)
            return window
        }
        func block(for duration: Duration) {
            let released: DispatchSemaphore = DispatchSemaphore(value: 0)
            Task { @concurrent in
                _ = await Result { try await Task.sleep(for: duration) }
                released.signal()
            }
            released.wait()
        }
        func deferred(titled title: String, _ work: @escaping @MainActor () -> Void) {
            windows.first?.title = title
            Task { @MainActor in work() }
        }
        let duration: Duration = settings.duration
        logger.notice("Scenario \(scenario.rawValue, privacy: .public) started")
        switch scenario {
            case .slowLaunch: block(for: duration)
            case .launchModal: alert("Launch modal").runModal()
            case .singleInstance:
                let others: [NSRunningApplication] = (Bundle.main.bundleIdentifier.map(NSRunningApplication.runningApplications(withBundleIdentifier:)) ?? [])
                    .filter { instance in instance.processIdentifier != getpid() }
                guard !others.isEmpty else { return }
                logger.notice("Instance refused with response \(alert("Another instance is running").runModal().rawValue, privacy: .public)")
                exit(settings.status)
            case .window: windows.append(window(windows.count))
            case .undescribed: windows.append(undescribed())
            case .collapsed: windows.append(collapsed())
            case .swatch: windows.append(swatch())
            case .table: windows.append(table())
            case .sheet: windows.first.map { window in alert("Sheet alert").beginSheetModal(for: window, completionHandler: nil) }
            case .modal: alert("Modal alert").runModal()
            case .saveSheet: windows.first.map { window in NSSavePanel().beginSheetModal(for: window) { _ in } }
            case .systemDialog: windows.append(panel("System dialog", style: [.titled], subrole: .systemDialog))
            case .floatingPanel: windows.append(panel("Floating panel", style: [.titled, .utilityWindow], level: .modalPanel))
            case .edited: windows.first?.isDocumentEdited = true
            case .minimize: windows.first?.miniaturize(nil)
            case .hangBlocked:
                let held: Hang = Hang()
                deferred(titled: "Blocked") { held.waiting.wait() }
            case .hangBusy:
                let held: Hang = Hang()
                deferred(titled: "Busy") {
                    while !held.released.load(ordering: .relaxed) {}
                }
            case .hangQuiet: Hang().waiting.wait()
            case .stop: raise(SIGSTOP)
            case .fault: unsafe UnsafeMutablePointer<Int>(bitPattern: 8).unsafelyUnwrapped.pointee = 1
            case .abort: abort()
            case .exit: exit(settings.status)
            case .helper: helpers.append(helper())
            case .terminate: NSApp.terminate(nil)
        }
    }

    private func helper() -> Task<Void, Never> {
        Task { [logger] in
            do {
                let arguments: [String] = Settings(role: .helper).arguments
                let executable: FilePath = FilePath(ProcessInfo.processInfo.arguments[0])
                var session: PlatformOptions = PlatformOptions()
                session.createSession = true
                session.teardownSequence = [.gracefulShutDown(toProcessGroup: true, allowedDurationToNextStep: .seconds(2))]
                _ = try await Subprocess.run(.path(executable), arguments: Arguments(arguments), platformOptions: session, input: .none, output: .discarded, error: .discarded) {
                    execution in logger.notice("Helper \(execution.processIdentifier.value, privacy: .public) started")
                }
            } catch {
                logger.error("Helper failed: \(error, privacy: .public)")
            }
        }
    }

    // --- [ROWS]

    func numberOfRows(in tableView: NSTableView) -> Int { 200 }

    func tableView(_ tableView: NSTableView, objectValueFor tableColumn: NSTableColumn?, row: Int) -> Any? { "Row \(row)" }

    // --- [WINDOWS]

    private func window(_ index: Int) -> NSWindow {
        let offset: CGFloat = CGFloat(200 + index * 40)
        let window: NSWindow = NSWindow(
            contentRect: NSRect(origin: NSPoint(x: offset, y: offset), size: windowSize),
            styleMask: [.titled, .closable, .miniaturizable, .resizable],
            backing: .buffered,
            defer: false,
        )
        window.title = "Fixture \(index)"
        window.isRestorable = false
        window.isReleasedWhenClosed = false
        let press: NSButton = NSButton(title: "Press", target: self, action: #selector(pressed(_:)))
        press.setAccessibilityIdentifier("press")
        let field: NSTextField = NSTextField(string: "")
        field.setAccessibilityIdentifier("field")
        field.target = self
        field.action = #selector(entered(_:))
        let check: NSButton = NSButton(checkboxWithTitle: "Check", target: self, action: #selector(pressed(_:)))
        let popUp: NSPopUpButton = NSPopUpButton(frame: .zero, pullsDown: false)
        popUp.addItems(withTitles: ["One", "Two", "Three"])
        status.setAccessibilityIdentifier("status")
        window.contentView = NSStackView(views: [press, field, check, popUp, status])
        window.representedURL = settings.document
        window.isDocumentEdited = settings.edited
        window.orderFront(nil)
        return window
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension Logger {
    init(for owner: any Any.Type) {
        self.init(subsystem: Bundle.main.bundleIdentifier ?? ProcessInfo.processInfo.processName, category: String(reflecting: owner))
    }
}
