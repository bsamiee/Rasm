import AppKit
import Encoding
import os

// --- [SERVICES] ------------------------------------------------------------------------

final class Application: NSApplication {
    override func sendEvent(_ event: NSEvent) {
        let (type, modifiers) = (event.type.rawValue, event.modifierFlags.intersection(.deviceIndependentFlagsMask).rawValue)
        switch event.type {
            case .leftMouseDown, .leftMouseUp, .leftMouseDragged, .rightMouseDown, .rightMouseUp, .rightMouseDragged, .otherMouseDown, .otherMouseUp, .otherMouseDragged:
                emit(.pointer(type: type, window: event.windowNumber, at: event.locationInWindow, clicks: event.clickCount, modifiers: modifiers))
            case .keyDown, .keyUp: emit(.key(type: type, window: event.windowNumber, characters: event.characters, code: event.keyCode, modifiers: modifiers))
            case .scrollWheel: emit(.scroll(window: event.windowNumber, at: event.locationInWindow, delta: CGVector(dx: event.scrollingDeltaX, dy: event.scrollingDeltaY), modifiers: modifiers))
            default: break
        }
        super.sendEvent(event)
    }

    override func sendAction(_ action: Selector, to target: Any?, from sender: Any?) -> Bool {
        (delegate as? Delegate)?.sent(action, from: sender)
        return super.sendAction(action, to: target, from: sender)
    }
}

@MainActor
final class Delegate: NSObject, NSApplicationDelegate, NSMenuDelegate, NSToolbarDelegate {
    private let rows = Rows()
    private var members: [Control: [any NSObject & NSAccessibilityProtocol]] = [:]
    private var identities: [ObjectIdentifier: (control: Control, member: Int)] = [:]
    private var captions: [Control: NSTextField] = [:]
    private var windows: [Window: NSWindow] = [:]
    private var named: [ObjectIdentifier: Window] = [:]
    private let form = NSStackView()
    private let dialog = NSPanel(contentRect: NSRect(x: 0, y: 0, width: 320, height: 120), styleMask: [.titled], backing: .buffered, defer: false)
    private let panel = NSPanel(contentRect: NSRect(x: 0, y: 0, width: 240, height: 160), styleMask: [.titled, .closable, .utilityWindow], backing: .buffered, defer: false)
    private let sheet = NSWindow(contentRect: NSRect(x: 0, y: 0, width: 300, height: 120), styleMask: [.titled], backing: .buffered, defer: false)
    private let popover = NSPopover()
    private let context = NSMenu(title: "Context")
    private var status: NSStatusItem?

    // --- [LIFECYCLE]

    func applicationDidFinishLaunching(_: Notification) {
        rows.added = { [weak self] row, index in
            guard index == 0 else { return }
            self?.members[.row] = [row]
        }
        for control in Control.allCases {
            let built = control.members(sending: #selector(acted(_:)), to: self, rows: rows)
            members[control] = built
            for (member, object) in built.enumerated() { identities[ObjectIdentifier(object)] = (control, member) }
        }
        NSApp.mainMenu = mainMenu()
        for name in [NSWindow.didBecomeKeyNotification, NSWindow.didResignKeyNotification] {
            NotificationCenter.default.addObserver(self, selector: #selector(keyed(_:)), name: name, object: nil)
        }
        let frame = NSScreen.main?.visibleFrame ?? .zero
        windows = [
            .controls: window("Controls", NSRect(x: frame.minX + 40, y: frame.maxY - 760, width: 520, height: 720), content: controls()),
            .lists: window("Lists", NSRect(x: frame.minX + 580, y: frame.maxY - 480, width: 420, height: 440), content: lists()),
            .covered: window("Covered", NSRect(x: frame.minX + 60, y: frame.maxY - 380, width: 360, height: 260), content: members[.region]?.first as? NSView),
        ]
        named = Dictionary(uniqueKeysWithValues: windows.map { name, window in (ObjectIdentifier(window), name) })
        windows[.controls]?.toolbar = toolbar()
        (panel.title, panel.isFloatingPanel, panel.hidesOnDeactivate) = ("Swatches", true, false)
        (dialog.title, sheet.title) = ("Export", "Options")
        let acted = #selector(acted(_:))
        dialog.contentView = NSStackView(views: [NSButton(title: "Cancel", target: self, action: acted), NSButton(title: "Export", target: self, action: acted)])
        sheet.contentView = NSStackView(views: [NSButton(checkboxWithTitle: "Include Bleed", target: self, action: acted)])
        popover.contentViewController = NSViewController()
        popover.contentViewController?.view = NSStackView(views: [NSTextField(labelWithString: "Details"), NSButton(title: "Done", target: self, action: acted)])
        popover.behavior = .applicationDefined
        for title in ["Cut", "Copy", "Paste"] { context.addItem(withTitle: title, action: acted, keyEquivalent: "").target = self }
        (members[.region]?.first as? NSView)?.menu = context
        for menu in [context, (members[.deferred]?.first as? NSPopUpButton)?.menu, (members[.tools]?.first as? NSSegmentedControl)?.menu(forSegment: 2)] { menu?.delegate = self }
        for window in [Window.covered, .lists, .controls].compactMap({ name in windows[name] }) { window.orderBack(nil) }
        Task.detached { [self] in
            do {
                for try await line in FileHandle.standardInput.bytes.jsonLines {
                    let command = try JSONDecoder().decode(Command.self, from: line)
                    DispatchQueue.main.async { MainActor.assumeIsolated { self.handle(command) } }
                }
            } catch {
                Logger().error("\(String(describing: error), privacy: .public)")
            }
            DispatchQueue.main.async { MainActor.assumeIsolated { NSApp.terminate(nil) } }
        }
        emit(.ready)
    }

    func applicationDidBecomeActive(_: Notification) { emit(.active(true)) }

    func applicationDidResignActive(_: Notification) { emit(.active(false)) }

    func menuWillOpen(_ menu: NSMenu) {
        if let popup = members[.deferred]?.first as? NSPopUpButton, menu === popup.menu {
            let chosen = popup.titleOfSelectedItem
            popup.removeAllItems()
            popup.addItems(withTitles: ["50%", "100%", "200%"])
            popup.selectItem(withTitle: chosen ?? "100%")
        }
        emit(.menu(title: menu.title, open: true))
    }

    func menuDidClose(_ menu: NSMenu) {
        if let popup = members[.deferred]?.first as? NSPopUpButton, menu === popup.menu {
            for item in popup.itemArray where item !== popup.selectedItem { menu.removeItem(item) }
        }
        emit(.menu(title: menu.title, open: false))
    }

    func toolbar(_: NSToolbar, itemForItemIdentifier itemIdentifier: NSToolbarItem.Identifier, willBeInsertedIntoToolbar _: Bool) -> NSToolbarItem? {
        let item = NSToolbarItem(itemIdentifier: itemIdentifier)
        (item.label, item.view) = ("Tools", members[.tools]?.first as? NSView)
        return item
    }

    func toolbarDefaultItemIdentifiers(_: NSToolbar) -> [NSToolbarItem.Identifier] { [NSToolbarItem.Identifier(Control.tools.rawValue)] }

    func toolbarAllowedItemIdentifiers(_ toolbar: NSToolbar) -> [NSToolbarItem.Identifier] { toolbarDefaultItemIdentifiers(toolbar) }

    // --- [EVENTS]

    @objc func acted(_: Any?) {}

    @objc func keyed(_ notification: Notification) {
        if let window = notification.object as? NSWindow { emit(.keyed(window: window.windowNumber, notification.name == NSWindow.didBecomeKeyNotification)) }
    }

    func sent(_ action: Selector, from sender: Any?) {
        let identity = (sender as? NSObject).flatMap { object in identities[ObjectIdentifier(object)] }
        let value = identity.flatMap { identity in members[identity.control]?.dropFirst(identity.member).first.flatMap { object in identity.control.fact(of: object, member: identity.member).value } }
        emit(.action(selector: NSStringFromSelector(action), control: identity?.control, member: identity?.member, value: value))
    }

    // --- [COMMANDS]

    func handle(_ command: Command) {
        switch command {
            case .report: emit(.report(report()))
            case .show(.dialog): _ = NSApp.runModal(for: dialog)
            case .dismiss(.dialog):
                NSApp.stopModal()
                dialog.orderOut(nil)
            case .show(.panel): panel.orderBack(nil)
            case .dismiss(.panel): panel.orderOut(nil)
            case .show(.sheet): windows[.controls]?.beginSheet(sheet)
            case .dismiss(.sheet): windows[.controls]?.endSheet(sheet)
            case .show(.popover): if let anchor = members[.action]?.first as? NSView { popover.show(relativeTo: anchor.bounds, of: anchor, preferredEdge: .maxY) }
            case .dismiss(.popover): popover.close()
            case .show(.context):
                if let region = members[.region]?.first as? NSView { _ = context.popUp(positioning: nil, at: NSPoint(x: 24, y: 24), in: region) }
            case .dismiss(.context): context.cancelTracking()
            case .show(.deferred): (members[.deferred]?.first as? NSPopUpButton)?.performClick(nil)
            case .dismiss(.deferred): (members[.deferred]?.first as? NSPopUpButton)?.menu?.cancelTracking()
            case .show(.grouped):
                if let tools = members[.tools]?.first as? NSSegmentedControl {
                    _ = tools.menu(forSegment: 2)?.popUp(positioning: nil, at: NSPoint(x: tools.width(forSegment: 0) + tools.width(forSegment: 1), y: 0), in: tools)
                }
            case .dismiss(.grouped): (members[.tools]?.first as? NSSegmentedControl)?.menu(forSegment: 2)?.cancelTracking()
            case .show(.status): status = statusItem()
            case .dismiss(.status):
                if let status { NSStatusBar.system.removeStatusItem(status) }
                status = nil
            case .close(let name): windows[name]?.close()
            case .open(let name): windows[name]?.orderBack(nil)
            case .retitle(let control, let member, let title):
                switch members[control]?.dropFirst(member).first {
                    case let button as NSButton: button.title = title
                    case let item as NSMenuItem: item.title = title
                    default: captions[control]?.stringValue = title
                }
            case .reorder(let control, let index): if let row = unsafe captions[control]?.superview { form.insertArrangedSubview(row, at: index) }
            case .fault(let nonce): Logger().fault("\(nonce, privacy: .public)")
            case .hang(let duration): Thread.sleep(forTimeInterval: duration / .seconds(1))
            case .activate: NSApp.activate()
            case .abort: abort()
        }
    }

    // --- [REPORT]

    func report() -> Report {
        Report(
            controls: Control.allCases.flatMap { control in (members[control] ?? []).enumerated().map { member, object in control.fact(of: object, member: member) } },
            windows: NSApp.windows.map { window in
                WindowFact(
                    window: named[ObjectIdentifier(window)],
                    number: window.windowNumber,
                    className: String(describing: type(of: window)),
                    title: window.title,
                    role: window.accessibilityRole()?.rawValue,
                    subrole: window.accessibilitySubrole()?.rawValue,
                    frame: window.frame,
                    level: window.level.rawValue,
                    visible: window.isVisible,
                    key: window.isKeyWindow,
                    main: window.isMainWindow,
                )
            },
        )
    }

    // --- [CONTAINERS]

    private func window(_ title: String, _ frame: NSRect, content: NSView?) -> NSWindow {
        let window = NSWindow(contentRect: frame, styleMask: [.titled, .closable, .miniaturizable, .resizable], backing: .buffered, defer: false)
        (window.title, window.isReleasedWhenClosed, window.contentView) = (title, false, content)
        return window
    }

    private func controls() -> NSView {
        for (control, caption) in Control.allCases.compactMap({ control in control.caption.map { caption in (control, caption) } }) {
            let label = NSTextField(labelWithString: caption)
            captions[control] = label
            form.addArrangedSubview(NSStackView(views: [label] + (members[control] ?? []).compactMap { object in object as? NSView }))
        }
        (form.orientation, form.alignment, form.edgeInsets) = (.vertical, .leading, NSEdgeInsets(top: 16, left: 16, bottom: 16, right: 16))
        return form
    }

    private func lists() -> NSView {
        let lists = NSStackView(
            views: [Control.table, .outline].compactMap { control in
                (members[control]?.first as? NSView).map { list in
                    let scroll = NSScrollView()
                    (scroll.documentView, scroll.hasVerticalScroller) = (list, true)
                    return scroll
                }
            }
        )
        (lists.orientation, lists.distribution) = (.vertical, .fillEqually)
        return lists
    }

    private func toolbar() -> NSToolbar {
        let toolbar = NSToolbar(identifier: Window.controls.rawValue)
        toolbar.delegate = self
        return toolbar
    }

    private func mainMenu() -> NSMenu {
        let menu = NSMenu()
        let application = NSMenu(title: ProcessInfo.processInfo.processName)
        application.addItem(withTitle: "Quit \(ProcessInfo.processInfo.processName)", action: #selector(NSApplication.terminate(_:)), keyEquivalent: "q")
        let view = NSMenu(title: "View")
        for item in (members[.item] ?? []).compactMap({ object in object as? NSMenuItem }) { view.addItem(item) }
        for submenu in [application, view] {
            submenu.delegate = self
            menu.addItem(withTitle: submenu.title, action: nil, keyEquivalent: "").submenu = submenu
        }
        return menu
    }

    private func statusItem() -> NSStatusItem {
        let item = NSStatusBar.system.statusItem(withLength: NSStatusItem.variableLength)
        let menu = NSMenu(title: "Status")
        for title in ["Pause", "Resume"] { menu.addItem(withTitle: title, action: #selector(acted(_:)), keyEquivalent: "").target = self }
        menu.delegate = self
        item.button?.title = ProcessInfo.processInfo.processName
        item.menu = menu
        return item
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
