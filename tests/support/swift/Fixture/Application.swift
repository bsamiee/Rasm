import AppKit
import Encoding
import UniformTypeIdentifiers
import os

// --- [SERVICES] ------------------------------------------------------------------------

final class Application: NSApplication {
    private var dispatching = false

    override func nextEvent(matching mask: NSEvent.EventTypeMask, until expiration: Date?, inMode mode: RunLoop.Mode, dequeue: Bool) -> NSEvent? {
        let event = super.nextEvent(matching: mask, until: expiration, inMode: mode, dequeue: dequeue)
        let pointer: NSEvent.EventTypeMask = [.leftMouseDown, .leftMouseUp, .leftMouseDragged, .rightMouseDown, .rightMouseUp, .rightMouseDragged, .otherMouseDown, .otherMouseUp, .otherMouseDragged]
        let (type, modifiers) = (event?.type.rawValue ?? 0, event?.modifierFlags.intersection(.deviceIndependentFlagsMask).rawValue ?? 0)
        switch dequeue ? event : nil {
            case let event? where pointer.contains(NSEvent.EventTypeMask(type: event.type)):
                emit(.pointer(type: type, window: event.windowNumber, at: event.locationInWindow, clicks: event.clickCount, modifiers: modifiers))
            case let event? where event.type == .keyDown || event.type == .keyUp:
                emit(.key(type: type, window: event.windowNumber, characters: event.characters, code: event.keyCode, modifiers: modifiers))
            case let event? where event.type == .scrollWheel:
                emit(.scroll(window: event.windowNumber, at: event.locationInWindow, delta: CGVector(dx: event.scrollingDeltaX, dy: event.scrollingDeltaY), modifiers: modifiers))
            default: break
        }
        return event
    }

    override func sendEvent(_ event: NSEvent) {
        let outer = dispatching
        dispatching = true
        super.sendEvent(event)
        dispatching = outer
    }

    override func sendAction(_ action: Selector, to target: Any?, from sender: Any?) -> Bool {
        (delegate as? Delegate)?.sent(action, from: sender, cause: dispatching ? currentEvent?.type.rawValue : nil)
        return super.sendAction(action, to: target, from: sender)
    }
}

@MainActor
final class Delegate: NSObject, NSApplicationDelegate, NSMenuDelegate, NSToolbarDelegate, NSWindowDelegate {
    private let rows = Rows()
    private let keyboard = Keyboard()
    private let layout = Layout()
    private let form = NSStackView()
    private let popover = NSPopover()
    private let windows: [Surface: NSWindow]
    private var members: [Inventory: [any NSObject & NSAccessibilityProtocol]] = [:]
    private var identities: [ObjectIdentifier: (entry: Inventory, member: Int)] = [:]
    private var captions: [Inventory: NSTextField] = [:]
    private var toolbarItems: [NSToolbarItem.Identifier: NSToolbarItem] = [:]
    private var status: NSStatusItem?
    private var tracking = false

    override init() {
        let screen = NSScreen.main?.visibleFrame ?? .zero
        let document: NSWindow.StyleMask = [.titled, .closable, .miniaturizable, .resizable]
        let rows: [(Surface, NSWindow.Type, NSWindow.StyleMask, NSRect, String)] = [
            (.controls, NSWindow.self, document, NSRect(x: 40, y: 760, width: 520, height: 720), "Controls"),
            (.lists, NSWindow.self, document, NSRect(x: 580, y: 480, width: 420, height: 440), "Lists"),
            (.covered, NSWindow.self, document, NSRect(x: 60, y: 380, width: 360, height: 260), "Covered"),
            (.unlisted, NSWindow.self, document, NSRect(x: 440, y: 300, width: 300, height: 160), "Unlisted"),
            (.dialog, NSPanel.self, [.titled], NSRect(x: 100, y: 300, width: 320, height: 120), "Export"),
            (.progress, NSPanel.self, [.titled], NSRect(x: 100, y: 300, width: 320, height: 100), "Exporting"),
            (.panel, NSPanel.self, [.titled, .closable, .utilityWindow], NSRect(x: 640, y: 760, width: 240, height: 160), "Swatches"),
            (.sheet, NSWindow.self, [.titled], NSRect(x: 0, y: 0, width: 300, height: 120), "Options"),
        ]
        windows = Dictionary(
            uniqueKeysWithValues: rows.map { surface, kind, style, frame, title in
                let window = kind.init(
                    contentRect: NSRect(x: screen.minX + frame.minX, y: screen.maxY - frame.minY, width: frame.width, height: frame.height),
                    styleMask: style,
                    backing: .buffered,
                    defer: false,
                )
                (window.title, window.isReleasedWhenClosed, window.level) = (title, false, NSWindow.Level(NSWindow.Level.normal.rawValue - 1))
                (window as? NSPanel)?.hidesOnDeactivate = false
                return (surface, window)
            }
        )
        super.init()
    }

    // --- [LIFECYCLE]

    func applicationDidFinishLaunching(_: Notification) {
        let acted = #selector(acted(_:))
        let built = Inventory.allCases.map { entry in (entry: entry, row: entry.row(sending: acted, to: self, rows: rows)) }
        let placed = Dictionary(grouping: built, by: \.row.surface)
        for (entry, row) in built {
            members[entry] = row.members
            for (member, object) in row.members.enumerated() { identities[ObjectIdentifier(object)] = (entry, member) }
        }
        for (entry, row) in placed[.controls] ?? [] {
            let caption = NSTextField(labelWithString: row.caption ?? "")
            captions[entry] = caption
            for control in row.members.compactMap({ object in object as? NSControl }) { control.setAccessibilityTitleUIElement(caption) }
            form.addArrangedSubview(NSStackView(views: [caption] + row.members.compactMap { object in object as? NSView }))
        }
        (form.orientation, form.alignment, form.edgeInsets) = (.vertical, .leading, NSEdgeInsets(top: 16, left: 16, bottom: 16, right: 16))
        let lists = NSStackView(
            views: (placed[.lists] ?? []).flatMap(\.row.members).compactMap { list in
                (list as? NSView).map { list in
                    let scroll = NSScrollView()
                    (scroll.documentView, scroll.hasVerticalScroller) = (list, true)
                    return scroll
                }
            }
        )
        (lists.orientation, lists.distribution) = (.vertical, .fillEqually)
        let palette = NSStackView(views: (placed[.panel] ?? []).flatMap(\.row.members).compactMap { object in object as? NSView })
        (palette.orientation, palette.alignment) = (.vertical, .leading)
        palette.setAccessibilityElement(true)
        palette.setAccessibilityRole(.layoutArea)
        let unlisted = Region()
        unlisted.text = windows[.unlisted]?.title ?? ""
        let cancel = NSButton(title: "Cancel", target: self, action: #selector(answered(_:)))
        let export = NSButton(title: "Export", target: self, action: #selector(answered(_:)))
        (cancel.tag, cancel.keyEquivalent, export.tag, export.keyEquivalent) = (NSApplication.ModalResponse.cancel.rawValue, "\u{1b}", NSApplication.ModalResponse.OK.rawValue, "\r")
        let indicator = NSProgressIndicator()
        (indicator.style, indicator.isIndeterminate) = (.bar, true)
        let waiting = NSButton(title: "Cancel", target: self, action: #selector(answered(_:)))
        waiting.isEnabled = false
        let contents: [Surface: NSView?] = [
            .controls: form, .lists: lists, .covered: (placed[.covered] ?? []).flatMap(\.row.members).first as? NSView, .unlisted: unlisted,
            .dialog: NSStackView(views: [cancel, export]), .progress: NSStackView(views: [indicator, waiting]), .panel: palette,
            .sheet: NSStackView(views: [NSButton(checkboxWithTitle: "Include Bleed", target: self, action: acted)]),
        ]
        for case (let surface, let content?) in contents { windows[surface]?.contentView = content }
        let flattened: [Any] = (windows[.panel]?.accessibilityChildren() ?? []).flatMap { child -> [Any] in (child as AnyObject) === palette ? palette.accessibilityChildren() ?? [] : [child] }
        windows[.panel]?.setAccessibilityChildren(flattened)
        windows[.unlisted]?.setAccessibilityElement(false)
        windows[.unlisted]?.setAccessibilityChildren([])
        windows[.controls]?.delegate = self
        for (entry, row) in placed[.toolbar] ?? [] {
            let item = NSToolbarItem(itemIdentifier: NSToolbarItem.Identifier(entry.rawValue))
            (item.label, item.view) = (row.caption ?? "", row.members.first as? NSView)
            toolbarItems[item.itemIdentifier] = item
        }
        let toolbar = NSToolbar(identifier: Surface.toolbar.rawValue)
        toolbar.delegate = self
        windows[.controls]?.toolbar = toolbar
        popover.contentViewController = NSViewController()
        popover.contentViewController?.view = NSStackView(views: [NSTextField(labelWithString: "Details"), NSButton(title: "Done", target: self, action: acted)])
        popover.behavior = .applicationDefined
        layout.fileURL = FileManager.default.temporaryDirectory.appending(component: "Layout").appendingPathExtension(for: .plainText)
        layout.fileType = UTType.plainText.identifier
        layout.addWindowController(NSWindowController(window: windows[.lists]))
        let context = NSMenu(title: "Context")
        for title in ["Cut", "Copy", "Paste"] { context.addItem(withTitle: title, action: acted, keyEquivalent: "").target = self }
        (members[.region]?.first as? NSView)?.menu = context
        let bar = NSMenu()
        let application = NSMenu(title: ProcessInfo.processInfo.processName)
        application.addItem(withTitle: "Quit \(ProcessInfo.processInfo.processName)", action: #selector(NSApplication.terminate(_:)), keyEquivalent: "q")
        let edit = NSMenu(title: "Edit")
        edit.addItem(withTitle: "Select All", action: #selector(NSText.selectAll(_:)), keyEquivalent: "a")
        let views = (placed[nil] ?? []).map { _, row in
            let menu = NSMenu(title: row.caption ?? "")
            menu.items = row.members.compactMap { object in object as? NSMenuItem }
            return menu
        }
        for submenu in [application, edit] + views { bar.addItem(withTitle: submenu.title, action: nil, keyEquivalent: "").submenu = submenu }
        NSApp.mainMenu = bar
        let menus: [NSMenu?] = [context, (members[.deferred]?.first as? NSPopUpButton)?.menu, (members[.tools]?.first as? NSSegmentedControl)?.menu(forSegment: 2)] + views
        for case let menu? in menus { menu.delegate = self }
        for (name, selector) in [
            (NSWindow.didBecomeKeyNotification, #selector(keyed(_:))), (NSWindow.didResignKeyNotification, #selector(keyed(_:))),
            (NSMenu.didBeginTrackingNotification, #selector(tracked(_:))), (NSMenu.didEndTrackingNotification, #selector(tracked(_:))),
        ] {
            NotificationCenter.default.addObserver(self, selector: selector, name: name, object: nil)
        }
        for surface in [Surface.unlisted, .covered, .lists, .controls] { windows[surface]?.orderBack(nil) }
        Task.detached { [self] in
            do {
                for try await line in FileHandle.standardInput.bytes.jsonLines {
                    let command = try JSONDecoder().decode(Command.self, from: line)
                    CFRunLoopPerformBlock(CFRunLoopGetMain(), CFRunLoopMode.commonModes.rawValue) { MainActor.assumeIsolated { self.handle(command) } }
                    CFRunLoopWakeUp(CFRunLoopGetMain())
                }
                exit(EXIT_SUCCESS)
            } catch {
                Logger().error("\(String(describing: error), privacy: .public)")
                exit(EXIT_FAILURE)
            }
        }
        emit(.ready)
    }

    func applicationDidBecomeActive(_: Notification) { emit(.active(active: true)) }

    func applicationDidResignActive(_: Notification) { emit(.active(active: false)) }

    func windowWillReturnFieldEditor(_: NSWindow, to client: Any?) -> Any? {
        (client as? NSObject).flatMap { client in identities[ObjectIdentifier(client)] }?.entry == .measure ? keyboard : nil
    }

    func menuNeedsUpdate(_ menu: NSMenu) {
        guard let popup = members[.deferred]?.first as? NSPopUpButton, menu === popup.menu, popup.numberOfItems == 1 else { return }
        popup.insertItem(withTitle: "50%", at: 0)
        popup.addItem(withTitle: "200%")
    }

    func menuWillOpen(_ menu: NSMenu) { emit(.menu(title: menu.title, opened: true, tracking: tracking)) }

    func menuDidClose(_ menu: NSMenu) { emit(.menu(title: menu.title, opened: false, tracking: tracking)) }

    func toolbar(_: NSToolbar, itemForItemIdentifier itemIdentifier: NSToolbarItem.Identifier, willBeInsertedIntoToolbar _: Bool) -> NSToolbarItem? { toolbarItems[itemIdentifier] }

    func toolbarDefaultItemIdentifiers(_: NSToolbar) -> [NSToolbarItem.Identifier] {
        Inventory.allCases.map { entry in NSToolbarItem.Identifier(entry.rawValue) }.filter { identifier in toolbarItems[identifier] != nil }
    }

    func toolbarAllowedItemIdentifiers(_ toolbar: NSToolbar) -> [NSToolbarItem.Identifier] { toolbarDefaultItemIdentifiers(toolbar) }

    // --- [EVENTS]

    @objc func acted(_: Any?) {}

    @objc func answered(_ sender: NSButton) { NSApp.stopModal(withCode: NSApplication.ModalResponse(sender.tag)) }

    @objc func document(_: NSDocument, shouldClose: Bool, contextInfo _: UnsafeMutableRawPointer?) { emit(.answered(surface: .save, accepted: shouldClose)) }

    @objc func keyed(_ notification: Notification) {
        if let window = notification.object as? NSWindow { emit(.keyed(window: window.windowNumber, key: notification.name == NSWindow.didBecomeKeyNotification)) }
    }

    @objc func tracked(_ notification: Notification) { tracking = notification.name == NSMenu.didBeginTrackingNotification }

    func sent(_ action: Selector, from sender: Any?, cause: UInt?) {
        let identity = (sender as? NSObject).flatMap { object in identities[ObjectIdentifier(object)] }
        let value = identity.flatMap { identity in members[identity.entry]?.dropFirst(identity.member).first.flatMap { object in identity.entry.fact(of: object, member: identity.member).value } }
        emit(.action(selector: NSStringFromSelector(action), entry: identity?.entry, member: identity?.member, value: value, cause: cause))
    }

    // --- [COMMANDS]

    func handle(_ command: Command) {
        switch command {
            case .report: emit(.report(report: report()))
            case .show(let surface): present(surface, shown: true)
            case .dismiss(let surface): present(surface, shown: false)
            case .retitle(let entry, let member, let title):
                switch members[entry]?.dropFirst(member).first {
                    case let button as NSButton: button.title = title
                    case let item as NSMenuItem: item.title = title
                    case let region as Region: region.text = title
                    default: captions[entry]?.stringValue = title
                }
            case .reorder(let entry, let index): if let row = unsafe captions[entry]?.superview { form.insertArrangedSubview(row, at: index) }
            case .fault(let nonce): Logger().fault("\(nonce, privacy: .public)")
            case .hang(.sleeping(let duration)): Thread.sleep(forTimeInterval: duration / .seconds(1))
            case .hang(.spinning(let duration)):
                let end = ContinuousClock.now + duration
                while ContinuousClock.now < end {}
            case .activate: NSApp.activate()
            case .abort: abort()
        }
    }

    private func present(_ surface: Surface, shown: Bool) {
        switch (surface, shown) {
            case (.controls, true), (.lists, true), (.covered, true), (.unlisted, true), (.panel, true): windows[surface]?.orderBack(nil)
            case (.controls, false), (.lists, false), (.covered, false), (.unlisted, false), (.panel, false): windows[surface]?.orderOut(nil)
            case (.toolbar, _): windows[.controls]?.toolbar?.isVisible = shown
            case (.dialog, true), (.progress, true):
                guard let panel = windows[surface] else { return }
                let response = NSApp.runModal(for: panel)
                panel.orderOut(nil)
                if response != .abort { emit(.answered(surface: surface, accepted: response == .OK)) }
            case (.dialog, false), (.progress, false): NSApp.stopModal(withCode: .abort)
            case (.sheet, true): windows[.sheet].map { sheet in windows[.controls]?.beginSheet(sheet) }
            case (.sheet, false): windows[.sheet].map { sheet in windows[.controls]?.endSheet(sheet) }
            case (.save, true):
                layout.updateChangeCount(.changeDone)
                unsafe layout.canClose(withDelegate: self, shouldClose: #selector(document(_:shouldClose:contextInfo:)), contextInfo: nil)
            case (.save, false): windows[.lists]?.attachedSheet.map { sheet in windows[.lists]?.endSheet(sheet) }
            case (.popover, true): if let anchor = members[.action]?.first as? NSView { popover.show(relativeTo: anchor.bounds, of: anchor, preferredEdge: .maxY) }
            case (.popover, false): popover.close()
            case (.status, true):
                let menu = NSMenu(title: "Status")
                for title in ["Pause", "Resume"] { menu.addItem(withTitle: title, action: #selector(acted(_:)), keyEquivalent: "").target = self }
                status = status ?? NSStatusBar.system.statusItem(withLength: NSStatusItem.variableLength)
                menu.delegate = self
                status?.menu = menu
                status?.button?.title = ProcessInfo.processInfo.processName
            case (.status, false):
                status.map(NSStatusBar.system.removeStatusItem)
                status = nil
        }
    }

    // --- [REPORT]

    func report() -> Report {
        let surfaces = Dictionary(Surface.allCases.compactMap { surface in window(of: surface).map { window in (ObjectIdentifier(window), surface) } }, uniquingKeysWith: { first, _ in first })
        let height = NSScreen.screens.first?.frame.maxY ?? 0
        return Report(
            controls: Inventory.allCases.flatMap { entry in (members[entry] ?? []).enumerated().map { member, object in entry.fact(of: object, member: member) } },
            windows: NSApp.windows.map { window in
                Report.Window(
                    surface: surfaces[ObjectIdentifier(window)],
                    number: window.windowNumber,
                    className: String(describing: type(of: window)),
                    title: window.title,
                    role: window.isAccessibilityElement() ? window.accessibilityRole()?.rawValue : nil,
                    subrole: window.accessibilitySubrole()?.rawValue,
                    modal: NSApp.modalWindow === window,
                    frame: CGRect(x: window.frame.minX, y: height - window.frame.maxY, width: window.frame.width, height: window.frame.height),
                    level: window.level.rawValue,
                    visible: window.isVisible,
                    key: window.isKeyWindow,
                    main: window.isMainWindow,
                    edited: window.isDocumentEdited,
                )
            },
        )
    }

    private func window(of surface: Surface) -> NSWindow? {
        switch surface {
            case .controls, .lists, .covered, .unlisted, .dialog, .progress, .panel, .sheet: windows[surface]
            case .toolbar: nil
            case .save: windows[.lists]?.attachedSheet
            case .popover: unsafe popover.contentViewController?.view.window
            case .status: unsafe status?.button?.window
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

@MainActor
func emit(_ line: Line) {
    do {
        try FileHandle.standardOutput.write(contentsOf: JSONEncoder.lines.encodeLine(line))
    } catch {
        fatalError(String(describing: error))
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

@main
enum FixtureApp {
    @MainActor
    static func main() {
        let application = Application.shared
        let delegate = Delegate()
        application.delegate = delegate
        application.setActivationPolicy(.regular)
        withExtendedLifetime(delegate) { application.run() }
    }
}
