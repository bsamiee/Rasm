import AppKit
import Encoding
import UniformTypeIdentifiers
import os

// --- [SERVICES] ------------------------------------------------------------------------

final class Application: NSApplication {
    private var dispatching = false

    override func nextEvent(matching mask: NSEvent.EventTypeMask, until expiration: Date?, inMode mode: RunLoop.Mode, dequeue: Bool) -> NSEvent? {
        let event = super.nextEvent(matching: mask, until: expiration, inMode: mode, dequeue: dequeue)
        guard dequeue, let event else { return event }
        let modifiers = Lenient.known(event.modifierFlags.intersection(.deviceIndependentFlagsMask))
        switch event.type {
            case .leftMouseDown, .leftMouseUp, .leftMouseDragged, .rightMouseDown, .rightMouseUp, .rightMouseDragged, .otherMouseDown, .otherMouseUp, .otherMouseDragged:
                emit(Line.pointer(type: .known(event.type), window: event.windowNumber, at: event.locationInWindow, clicks: event.clickCount, modifiers: modifiers))
            case .keyDown, .keyUp: emit(Line.key(type: .known(event.type), window: event.windowNumber, characters: event.characters, code: event.keyCode, modifiers: modifiers))
            case .scrollWheel: emit(Line.scroll(window: event.windowNumber, at: event.locationInWindow, delta: CGVector(dx: event.scrollingDeltaX, dy: event.scrollingDeltaY), modifiers: modifiers))
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
        emit(
            Line.action(selector: NSStringFromSelector(action), control: (delegate as? Delegate)?.fact(of: sender), cause: dispatching ? currentEvent.map { event in Lenient.known(event.type) } : nil)
        )
        return super.sendAction(action, to: target, from: sender)
    }
}

@MainActor
final class Delegate: NSObject, NSApplicationDelegate, NSMenuDelegate, NSToolbarDelegate, NSWindowDelegate {
    private let rows = Rows()
    private let keyboard = Keyboard.fieldEditor()
    private let layout = Layout()
    private let popover = NSPopover()
    private let windows: [Surface: NSWindow]
    private var built: [Inventory: (surface: Surface?, caption: String?, members: [any NSObject & NSAccessibilityProtocol])] = [:]
    private var identities: [ObjectIdentifier: (entry: Inventory, member: Int, object: any NSObject & NSAccessibilityProtocol)] = [:]
    private var captions: [Inventory: NSTextField] = [:]
    private var menus: [Inventory: NSMenu] = [:]
    private var status: NSStatusItem?

    override init() {
        let document: NSWindow.StyleMask = [.titled, .closable, .miniaturizable, .resizable]
        let kinds: [(Surface, NSWindow.Type, NSWindow.StyleMask)] = [
            (.controls, NSWindow.self, document), (.lists, NSWindow.self, document), (.covered, NSWindow.self, document), (.unlisted, NSWindow.self, document),
            (.dialog, NSPanel.self, [.titled]), (.progress, NSPanel.self, [.titled]), (.panel, NSPanel.self, [.titled, .closable, .utilityWindow]), (.sheet, NSWindow.self, [.titled]),
        ]
        windows = Dictionary(
            uniqueKeysWithValues: kinds.map { surface, kind, style in
                let window = kind.init(contentRect: .zero, styleMask: style, backing: .buffered, defer: false)
                window.title = surface.rawValue.capitalized
                window.isReleasedWhenClosed = false
                window.level = NSWindow.Level(NSWindow.Level.normal.rawValue - 1)
                (window as? NSPanel)?.hidesOnDeactivate = false
                return (surface, window)
            }
        )
        super.init()
    }

    // --- [LIFECYCLE]

    func applicationDidFinishLaunching(_: Notification) {
        built = Dictionary(uniqueKeysWithValues: Inventory.allCases.map { entry in (entry, entry.row(rows: rows)) })
        for surface in Surface.allCases.map(Optional.some) + [nil] { place(surface) }
        if let window = windows[.unlisted] {
            let region = Region()
            region.text = window.title
            window.contentView = region
            window.setContentSize(region.intrinsicContentSize)
            window.setAccessibilityElement(false)
            window.setAccessibilityChildren([])
        }
        if let panel = windows[.panel], let palette = panel.contentView {
            palette.setAccessibilityElement(true)
            palette.setAccessibilityRole(.layoutArea)
            let flattened = unsafe NSAccessibility.unignoredChildren(from: panel.accessibilityChildren() ?? []).flatMap { child in
                unsafe (child as AnyObject) === palette ? NSAccessibility.unignoredChildren(from: palette.accessibilityChildren() ?? []) : [child]
            }
            panel.setAccessibilityChildren(flattened)
        }
        windows[.controls]?.delegate = self
        let toolbar = NSToolbar(identifier: Surface.toolbar.rawValue)
        toolbar.delegate = self
        windows[.controls]?.toolbar = toolbar
        popover.behavior = .applicationDefined
        layout.fileURL = Layout.folder.appending(component: "Layout").appendingPathExtension(for: .plainText)
        layout.fileType = UTType.plainText.identifier
        layout.addWindowController(NSWindowController(window: windows[.lists]))
        for object in built.values.flatMap(\.members) { (unsafe (object as? NSMenuItem)?.menu ?? (object as? NSView)?.menu)?.delegate = self }
        for (name, selector) in [
            (NSWindow.didBecomeKeyNotification, #selector(keyed(_:))), (NSWindow.didResignKeyNotification, #selector(keyed(_:))),
            (NSMenu.didBeginTrackingNotification, #selector(tracked(_:))), (NSMenu.didEndTrackingNotification, #selector(tracked(_:))),
        ] {
            NotificationCenter.default.addObserver(self, selector: selector, name: name, object: nil)
        }
        if let screen = NSScreen.main?.visibleFrame {
            var left = screen.minX
            for case let window? in [Surface.controls, .lists, .covered, .unlisted, .panel].map({ surface in windows[surface] }) {
                window.setFrameTopLeftPoint(NSPoint(x: left, y: screen.maxY))
                left += window.frame.width
            }
        }
        for surface in [Surface.unlisted, .covered, .lists, .controls] { windows[surface]?.orderBack(nil) }
        atexit {
            do {
                try FileManager.default.removeItem(at: Layout.folder)
            } catch {
                emit(String(describing: error), to: .standardError)
            }
        }
        Task.detached { [self] in
            do {
                try FileManager.default.createDirectory(at: Layout.folder, withIntermediateDirectories: false)
                for try await line in FileHandle.standardInput.bytes.jsonLines {
                    let command = try JSONDecoder().decode(Command.self, from: line)
                    CFRunLoopPerformBlock(CFRunLoopGetMain(), CFRunLoopMode.commonModes.rawValue) { MainActor.assumeIsolated { self.handle(command) } }
                    CFRunLoopWakeUp(CFRunLoopGetMain())
                }
                exit(EXIT_SUCCESS)
            } catch {
                emit(String(describing: error), to: .standardError)
                exit(EXIT_FAILURE)
            }
        }
        emit(Line.ready)
    }

    func applicationDidBecomeActive(_: Notification) { emit(Line.active(active: true)) }

    func applicationDidResignActive(_: Notification) { emit(Line.active(active: false)) }

    func windowWillReturnFieldEditor(_: NSWindow, to client: Any?) -> Any? {
        (client as? NSObject).flatMap { client in identities[ObjectIdentifier(client)] }?.entry == .measure ? keyboard : nil
    }

    func menuNeedsUpdate(_ menu: NSMenu) {
        guard let popup = built[.deferred]?.members.first as? NSPopUpButton, menu === popup.menu, popup.numberOfItems == 1 else { return }
        popup.insertItem(withTitle: "50%", at: 0)
        popup.addItem(withTitle: "200%")
    }

    func menuWillOpen(_ menu: NSMenu) { emit(Line.menu(title: menu.title, opened: true)) }

    func menuDidClose(_ menu: NSMenu) { emit(Line.menu(title: menu.title, opened: false)) }

    func toolbar(_: NSToolbar, itemForItemIdentifier itemIdentifier: NSToolbarItem.Identifier, willBeInsertedIntoToolbar _: Bool) -> NSToolbarItem? {
        let item = NSToolbarItem(itemIdentifier: itemIdentifier)
        item.label = itemIdentifier.rawValue.capitalized
        item.view = Inventory(rawValue: itemIdentifier.rawValue).flatMap { entry in built[entry]?.members.first as? NSView }
        return item
    }

    func toolbarDefaultItemIdentifiers(_: NSToolbar) -> [NSToolbarItem.Identifier] {
        Inventory.allCases.filter { entry in built[entry]?.surface == .toolbar }.map { entry in NSToolbarItem.Identifier(entry.rawValue) }
    }

    func toolbarAllowedItemIdentifiers(_ toolbar: NSToolbar) -> [NSToolbarItem.Identifier] { toolbarDefaultItemIdentifiers(toolbar) }

    // --- [EVENTS]

    @objc func acted(_: Any?) {}

    @objc func answered(_ sender: NSButton) { NSApp.stopModal(withCode: NSApplication.ModalResponse(sender.tag)) }

    @objc func exported(_: Any?) { present(.dialog, shown: true) }

    @objc func document(_: NSDocument, shouldClose: Bool, contextInfo _: UnsafeMutableRawPointer?) { emit(Line.answered(surface: .save, accepted: shouldClose)) }

    @objc func keyed(_ notification: Notification) {
        if let window = notification.object as? NSWindow { emit(Line.keyed(window: window.windowNumber, key: notification.name == NSWindow.didBecomeKeyNotification)) }
    }

    @objc func tracked(_ notification: Notification) {
        if let menu = notification.object as? NSMenu { emit(Line.tracking(title: menu.title, began: notification.name == NSMenu.didBeginTrackingNotification)) }
    }

    func fact(of object: Any?) -> Report.Control? {
        (object as AnyObject?).flatMap { object in identities[ObjectIdentifier(object)] }.map { identity in identity.entry.fact(of: identity.object, member: identity.member) }
    }

    // --- [COMMANDS]

    func handle(_ command: Command) {
        switch command {
            case .report: emit(Line.report(report: report()))
            case .show(let surface): present(surface, shown: true)
            case .dismiss(let surface): present(surface, shown: false)
            case .retitle(let entry, let member, let title):
                switch built[entry]?.members.dropFirst(member).first {
                    case let button as NSButton where !(button is NSPopUpButton): button.title = title
                    case let item as NSMenuItem: item.title = title
                    case let region as Region: region.text = title
                    default: captions[entry]?.stringValue = title
                }
            case .reorder(let from, let to): (windows[.controls]?.contentView as? NSStackView).map { form in form.insertArrangedSubview(form.arrangedSubviews[from], at: to) }
            case .fault(let nonce): Logger().fault("\(nonce, privacy: .public)")
            case .hang(.sleeping(let duration)): Thread.sleep(forTimeInterval: duration / .seconds(1))
            case .hang(.spinning(let duration)):
                let end = ContinuousClock.now + duration
                while ContinuousClock.now < end {}
            case .abort: abort()
        }
    }

    private func present(_ surface: Surface, shown: Bool) {
        switch (surface, shown) {
            case (.controls, _), (.lists, _), (.covered, _), (.unlisted, _), (.panel, _): shown ? windows[surface]?.orderBack(nil) : windows[surface]?.orderOut(nil)
            case (.toolbar, _): windows[.controls]?.toolbar?.isVisible = shown
            case (.dialog, true), (.progress, true):
                for entry in Inventory.allCases where built[entry]?.surface == surface { built[entry] = entry.row(rows: rows) }
                place(surface)
                guard let panel = windows[surface] else { return }
                panel.center()
                let response = NSApp.runModal(for: panel)
                panel.orderOut(nil)
                emit(Line.answered(surface: surface, accepted: response == .OK))
            case (.dialog, false), (.progress, false): NSApp.stopModal()
            case (.sheet, true): windows[.sheet].map { sheet in windows[.controls]?.beginSheet(sheet) }
            case (.sheet, false): windows[.sheet].map { sheet in windows[.controls]?.endSheet(sheet) }
            case (.save, true):
                layout.updateChangeCount(.changeDone)
                unsafe layout.canClose(withDelegate: self, shouldClose: #selector(document(_:shouldClose:contextInfo:)), contextInfo: nil)
            case (.save, false): windows[.lists]?.attachedSheet.map { sheet in windows[.lists]?.endSheet(sheet) }
            case (.popover, true): if let anchor = built[.action]?.members.first as? NSView { popover.show(relativeTo: anchor.bounds, of: anchor, preferredEdge: .maxY) }
            case (.popover, false): popover.close()
            case (.status, true):
                status = status ?? NSStatusBar.system.statusItem(withLength: NSStatusItem.variableLength)
                status?.menu = menus[.status]
                status?.button?.title = surface.rawValue.capitalized
            case (.status, false):
                status.map(NSStatusBar.system.removeStatusItem)
                status = nil
        }
    }

    private func place(_ surface: Surface?) {
        var views: [NSView] = []
        var placed: [NSMenu] = []
        for entry in Inventory.allCases {
            guard let row = built[entry], row.surface == surface else { continue }
            for (member, object) in row.members.enumerated() {
                identities[ObjectIdentifier(object)] = (entry, member, object)
                unsafe NSAccessibility.unignoredDescendant(of: object).map { element in identities[ObjectIdentifier(element as AnyObject)] = (entry, member, object) }
            }
            let items = row.members.compactMap { object in object as? NSMenuItem }.filter { item in unsafe item.menu == nil }
            if !items.isEmpty {
                let menu = NSMenu(title: row.caption ?? "")
                menu.items = items
                menus[entry] = menu
                placed.append(menu)
            }
            let members: [NSView] = row.members.compactMap { object in
                guard let list = object as? NSTableView else { return object as? NSView }
                let scroll = NSScrollView()
                scroll.documentView = list
                scroll.hasVerticalScroller = true
                scroll.widthAnchor.constraint(equalToConstant: list.tableColumns.map(\.width).reduce(0, +)).isActive = true
                scroll.heightAnchor.constraint(equalToConstant: list.rect(ofRow: list.numberOfRows - 1).maxY + (list.headerView?.frame.height ?? 0)).isActive = true
                return scroll
            }
            guard let text = row.caption, !members.isEmpty else {
                views += members
                continue
            }
            let caption = NSTextField(labelWithString: text)
            captions[entry] = caption
            for object in row.members {
                unsafe (NSAccessibility.unignoredDescendant(of: object) as? any NSAccessibilityProtocol)?.setAccessibilityTitleUIElement(NSAccessibility.unignoredDescendant(of: caption))
            }
            let line = NSStackView(views: [caption] + members)
            guard let tabs = built[.tabs]?.members.first as? NSTabView, let tab = tabs.tabViewItems.first(where: { item in item.identifier as? Inventory == entry }) else {
                views.append(line)
                continue
            }
            tab.label = text
            tab.view = line
            tabs.widthAnchor.constraint(greaterThanOrEqualToConstant: tabs.minimumSize.width + line.fittingSize.width).isActive = true
            tabs.heightAnchor.constraint(greaterThanOrEqualToConstant: tabs.minimumSize.height + line.fittingSize.height).isActive = true
        }
        let content = NSStackView(views: surface == .toolbar ? [] : views)
        content.orientation = .vertical
        content.alignment = .leading
        content.edgeInsets = NSEdgeInsets(top: 16, left: 16, bottom: 16, right: 16)
        switch surface {
            case nil:
                let bar = NSMenu()
                for menu in placed { bar.addItem(withTitle: menu.title, action: nil, keyEquivalent: "").submenu = menu }
                bar.delegate = self
                NSApp.mainMenu = bar
            case .toolbar, .save, .status: break
            case .popover:
                popover.contentViewController = NSViewController()
                popover.contentViewController?.view = content
                popover.contentSize = content.fittingSize
            case let kind?:
                for view in views { view.menu = placed.first }
                windows[kind]?.contentView = content
                windows[kind]?.setContentSize(content.fittingSize)
        }
    }

    // --- [REPORT]

    func report() -> Report {
        var surfaces = Dictionary(uniqueKeysWithValues: windows.map { surface, window in (ObjectIdentifier(window), surface) })
        let transient: [(Surface, NSWindow?)] = [(.save, windows[.lists]?.attachedSheet), (.popover, unsafe popover.contentViewController?.view.window), (.status, unsafe status?.button?.window)]
        for case (let surface, let window?) in transient { surfaces[ObjectIdentifier(window)] = surface }
        return Report(
            controls: Inventory.allCases.flatMap { entry in (built[entry]?.members ?? []).enumerated().map { member, object in entry.fact(of: object, member: member) } },
            windows: NSApp.windows.map { window in
                Report.Window(
                    surface: surfaces[ObjectIdentifier(window)],
                    number: window.windowNumber,
                    className: String(describing: type(of: window)),
                    title: window.title,
                    role: window.isAccessibilityElement() ? window.accessibilityRole()?.rawValue : nil,
                    subrole: window.accessibilitySubrole()?.rawValue,
                    modal: window.isAccessibilityModal(),
                    frame: window.accessibilityFrame(),
                    level: window.level.rawValue,
                    visible: window.isVisible,
                    key: window.isAccessibilityFocused(),
                    main: window.isAccessibilityMain(),
                    edited: window.isDocumentEdited,
                    defaultButton: fact(of: window.accessibilityDefaultButton()),
                    cancelButton: fact(of: window.accessibilityCancelButton()),
                )
            },
        )
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

func emit(_ value: some Encodable, to output: FileHandle = .standardOutput) {
    do {
        try output.write(contentsOf: JSONEncoder.jsonLines.encodeLine(value))
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
