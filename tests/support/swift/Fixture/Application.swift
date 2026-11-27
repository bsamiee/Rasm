import AppKit
import Encoding
import UniformTypeIdentifiers

// --- [SERVICES] ------------------------------------------------------------------------

final class Application: NSApplication {
    override func nextEvent(matching mask: NSEvent.EventTypeMask, until expiration: Date?, inMode mode: RunLoop.Mode, dequeue: Bool) -> NSEvent? {
        let event = super.nextEvent(matching: mask, until: expiration, inMode: mode, dequeue: dequeue)
        switch (dequeue, event?.type) {
            case (true, .keyDown), (true, .keyUp): emit(Line.key(sourceProcess: event?.cgEvent.map { posted in pid_t(posted.getIntegerValueField(.eventSourceUnixProcessID)) }))
            default: break
        }
        return event
    }
}

@MainActor
final class Delegate: NSObject, NSApplicationDelegate, NSMenuDelegate, NSToolbarDelegate {
    private let layout = Layout()
    private let popover = NSPopover()
    private let windows: [Surface: NSWindow]
    private let built: [Inventory: (surface: Surface?, caption: String?, members: [any NSObject & NSAccessibilityProtocol])]
    private var status: NSStatusItem?
    private var termination: Duration?
    private var follower: (any DispatchSourceProcess)?

    override init() {
        let document: NSWindow.StyleMask = [.titled, .closable, .miniaturizable, .resizable]
        let kinds: [(Surface, NSWindow.Type, NSWindow.StyleMask)] = [
            (.controls, NSWindow.self, document), (.lists, NSWindow.self, document), (.covered, NSWindow.self, document), (.unlisted, NSWindow.self, document),
            (.progress, NSWindow.self, [.titled]), (.panel, NSPanel.self, [.titled, .closable, .utilityWindow]), (.sheet, NSWindow.self, [.titled]),
            (.drawer, Drawer.self, [.borderless, .nonactivatingPanel]),
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
        built = Dictionary(uniqueKeysWithValues: Inventory.allCases.map { entry in (entry, entry.row()) })
        super.init()
    }

    // --- [LIFECYCLE]

    func applicationDidFinishLaunching(_: Notification) {
        for surface in Surface.allCases.map(Optional.some) + [nil] { place(surface) }
        for object in built.values.flatMap(\.members) { (unsafe (object as? NSMenuItem)?.menu ?? (object as? NSView)?.menu)?.delegate = self }
        for name in [NSMenu.didBeginTrackingNotification, NSMenu.didEndTrackingNotification] {
            NotificationCenter.default.addObserver(self, selector: #selector(tracked(_:)), name: name, object: nil)
        }
        for (surface, region) in [(Surface.unlisted, Region(text: Surface.unlisted.rawValue.capitalized)), (.drawer, Region(text: "Rectangle"))] {
            windows[surface]?.contentView = region
            windows[surface]?.setContentSize(region.intrinsicContentSize)
        }
        windows[.unlisted]?.setAccessibilityElement(false)
        windows[.unlisted]?.setAccessibilityChildren([])
        windows[.drawer]?.level = .popUpMenu
        let toolbar = NSToolbar(identifier: Surface.toolbar.rawValue)
        toolbar.delegate = self
        windows[.controls]?.toolbar = toolbar
        popover.behavior = .applicationDefined
        layout.fileURL = Layout.folder.appending(component: "Layout").appendingPathExtension(for: .plainText)
        layout.fileType = UTType.plainText.identifier
        layout.addWindowController(NSWindowController(window: windows[.lists]))
        windows[.covered]?.representedURL = Layout.folder.appending(component: "Covered").appendingPathExtension(for: .plainText)
        if let screen = NSScreen.main?.visibleFrame {
            var left = screen.minX
            for case let window? in [Surface.controls, .lists, .covered, .unlisted, .panel].map({ surface in windows[surface] }) {
                window.setFrameTopLeftPoint(NSPoint(x: left, y: screen.maxY))
                left += window.frame.width
            }
        }
        windows[.progress]?.center()
        for surface in [Surface.unlisted, .covered, .lists, .controls] { windows[surface]?.orderBack(nil) }
        if UserDefaults.standard.bool(forKey: FixtureBundle.ordersFrontAtLaunch), let window = windows[.controls] {
            window.level = .normal
            window.orderFrontRegardless()
        }
        let followed = pid_t(UserDefaults.standard.integer(forKey: FixtureBundle.followsProcess))
        if followed != 0 {
            let source = DispatchSource.makeProcessSource(identifier: followed, eventMask: .exit, queue: .main)
            source.setEventHandler { exit(EXIT_SUCCESS) }
            source.activate()
            follower = source
        }
        var input = stat()
        guard unsafe fstat(FileHandle.standardInput.fileDescriptor, &input) == 0, input.st_mode & S_IFMT == S_IFIFO else { return }
        Task.detached { [self] in
            do {
                for try await line in FileHandle.standardInput.bytes.jsonLines {
                    let command = try JSONDecoder().decode(Command.self, from: line)
                    DispatchQueue.main.async { MainActor.assumeIsolated { self.handle(command) } }
                }
                exit(EXIT_SUCCESS)
            } catch {
                emit(String(describing: error), to: .standardError)
                exit(EXIT_FAILURE)
            }
        }
        emit(Line.ready)
    }

    func applicationWillTerminate(_: Notification) {
        guard let termination else { return }
        for window in windows.values { window.close() }
        let end = ContinuousClock.now + termination
        while ContinuousClock.now < end {}
    }

    func applicationDidBecomeActive(_: Notification) { emit(Line.active(active: true)) }

    func applicationDidResignActive(_: Notification) { emit(Line.active(active: false)) }

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

    @objc func drew(_: Any?) { present(.drawer, shown: true) }

    @objc func tracked(_ notification: Notification) {
        if let menu = notification.object as? NSMenu { emit(Line.tracking(title: menu.title, began: notification.name == NSMenu.didBeginTrackingNotification)) }
    }

    // --- [COMMANDS]

    func handle(_ command: Command) {
        switch command {
            case .report: emit(Line.report(report: report()))
            case .show(let surface): present(surface, shown: true)
            case .raise(let surface): windows[surface]?.orderFrontRegardless()
            case .dismiss(let surface): present(surface, shown: false)
            case .retitle(let title): (built[.action]?.members.first as? NSButton)?.title = title
            case .select(let segment): (built[.tools]?.members.first as? NSSegmentedControl)?.selectedSegment = segment
            case .reorder(let from, let to):
                if let form = windows[.controls]?.contentView as? NSStackView {
                    form.insertArrangedSubview(form.arrangedSubviews[from], at: to)
                    form.layoutSubtreeIfNeeded()
                }
            case .sleep(let duration):
                emit(Line.sleeping)
                Thread.sleep(forTimeInterval: duration / .seconds(1))
            case .spinOnTermination(let duration): termination = duration
            case .edit:
                NSDocumentController.shared.addDocument(layout)
                layout.updateChangeCount(.changeDone)
        }
    }

    private func present(_ surface: Surface, shown: Bool) {
        switch (surface, shown) {
            case (.controls, true), (.lists, true), (.covered, true), (.unlisted, true), (.progress, true), (.panel, true): windows[surface]?.orderBack(nil)
            case (.controls, false), (.lists, false), (.covered, false), (.unlisted, false), (.progress, false), (.panel, false), (.drawer, false): windows[surface]?.orderOut(nil)
            case (.drawer, true): windows[.drawer]?.orderFrontRegardless()
            case (.toolbar, _): windows[.controls]?.toolbar?.isVisible = shown
            case (.sheet, true): if let sheet = windows[.sheet] { windows[.controls]?.beginSheet(sheet) }
            case (.sheet, false): if let sheet = windows[.sheet] { windows[.controls]?.endSheet(sheet) }
            case (.save, true):
                layout.updateChangeCount(.changeDone)
                unsafe layout.canClose(withDelegate: self, shouldClose: nil, contextInfo: nil)
            case (.save, false): if let sheet = windows[.lists]?.attachedSheet { windows[.lists]?.endSheet(sheet) }
            case (.popover, true): if let anchor = built[.action]?.members.first as? NSView { popover.show(relativeTo: anchor.bounds, of: anchor, preferredEdge: .maxY) }
            case (.popover, false): popover.close()
            case (.status, true):
                status = status ?? NSStatusBar.system.statusItem(withLength: NSStatusItem.variableLength)
                status?.menu = unsafe built[.status]?.members.compactMap { object in object as? NSMenuItem }.first?.menu
                status?.button?.image = NSImage(size: NSSize(width: 16, height: 16), flipped: false) { frame in
                    NSBezierPath(ovalIn: frame).fill()
                    return true
                }
            case (.status, false):
                status.map(NSStatusBar.system.removeStatusItem)
                status = nil
        }
    }

    private func place(_ surface: Surface?) {
        var (views, placed): ([NSView], [NSMenu]) = ([], [])
        for entry in Inventory.allCases {
            guard let row = built[entry], row.surface == surface else { continue }
            let items = row.members.compactMap { object in object as? NSMenuItem }.filter { item in unsafe item.menu == nil }
            if !items.isEmpty {
                let menu = NSMenu(title: row.caption ?? "")
                menu.items = items
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
        let content = NSStackView(views: views)
        content.orientation = .vertical
        content.alignment = .leading
        content.edgeInsets = NSEdgeInsets(top: 16, left: 16, bottom: 16, right: 16)
        switch surface {
            case nil:
                let bar = NSMenu()
                for menu in placed { bar.addItem(withTitle: menu.title, action: nil, keyEquivalent: "").submenu = menu }
                NSApp.mainMenu = bar
            case .toolbar, .save, .status: break
            case .popover:
                let controller = NSViewController()
                controller.view = content
                popover.contentViewController = controller
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
        if let shown = unsafe popover.contentViewController?.view.window { surfaces[ObjectIdentifier(shown)] = .popover }
        return Report(
            controls: Inventory.allCases.flatMap { entry in (built[entry]?.members ?? []).enumerated().map { member, object in entry.fact(of: object, member: member) } },
            windows: NSApp.windows.map { window in
                Report.Window(
                    surface: surfaces[ObjectIdentifier(window)],
                    number: window.windowNumber,
                    parent: (window.parent ?? window.sheetParent)?.windowNumber,
                    frame: window.accessibilityFrame(),
                    visible: window.isVisible,
                    edited: window.isDocumentEdited,
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
        application.setActivationPolicy(.accessory)
        withExtendedLifetime(delegate) { application.run() }
    }
}
