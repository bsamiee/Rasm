import AppKit
import Encoding

// --- [TYPES] ---------------------------------------------------------------------------

extension Inventory {
    // --- [CONTROLS]

    package static let toolSegments: [(label: String, help: String)] = [("Select", "Selection Tool (V)"), ("Type", "Type Tool (T)"), ("Frame", "Frame Tool (F)")]
    @MainActor private static let rows = Rows()

    @MainActor
    func row() -> (surface: Surface?, caption: String?, members: [any NSObject & NSAccessibilityProtocol]) {
        let action = #selector(Delegate.acted(_:))
        switch self {
            case .action:
                let apply = NSButton(title: "Apply", target: nil, action: action)
                apply.setAccessibilityUserInputLabels(["Apply", "Commit Changes"])
                return (.controls, "Document", [apply])
            case .toggle: return (.controls, "Grid", [NSButton(checkboxWithTitle: "Snap to Grid", target: nil, action: action)])
            case .popup:
                let (caption, popup) = ("Page Size", NSPopUpButton(frame: .zero, pullsDown: false))
                popup.addItems(withTitles: ["Letter", "A4", "Tabloid"])
                popup.action = action
                popup.menu?.title = caption
                return (.controls, caption, [popup])
            case .combo:
                let combo = NSComboBox()
                combo.action = action
                return (.controls, "Font", [combo])
            case .radio: return (.controls, "Orientation", ["Portrait", "Landscape"].map { title in NSButton(radioButtonWithTitle: title, target: nil, action: action) })
            case .tabs:
                let tabs = NSTabView()
                for entry: Self in [.text, .search] { tabs.addTabViewItem(NSTabViewItem(identifier: entry)) }
                return (.controls, "Panel", [tabs])
            case .segments: return (.controls, "Align", [NSSegmentedControl(labels: ["Left", "Center", "Right"], trackingMode: .selectOne, target: nil, action: action)])
            case .tools:
                let tools = NSSegmentedControl(labels: Self.toolSegments.map(\.label), trackingMode: .selectOne, target: nil, action: action)
                for (segment, tool) in Self.toolSegments.enumerated() { tools.setToolTip(tool.help, forSegment: segment) }
                return (.toolbar, nil, [tools])
            case .table:
                let table = NSTableView()
                for title in ["Name", "Size"] {
                    let column = NSTableColumn()
                    column.title = title
                    table.addTableColumn(column)
                }
                table.dataSource = Self.rows
                table.delegate = Self.rows
                table.reloadData()
                return (.lists, nil, [table])
            case .outline:
                let (outline, column) = (NSOutlineView(), NSTableColumn())
                column.title = "Pages"
                outline.addTableColumn(column)
                unsafe outline.outlineTableColumn = column
                outline.dataSource = Self.rows
                outline.delegate = Self.rows
                outline.reloadData()
                outline.expandItem(nil, expandChildren: true)
                return (.lists, nil, [outline])
            case .slider: return (.controls, "Opacity", [NSSlider(value: 40, minValue: 0, maxValue: 100, target: nil, action: action)])
            case .stepper:
                let stepper = NSStepper()
                stepper.action = action
                return (.controls, "Copies", [stepper])
            case .measure: return (.controls, "Size", [NSTextField(string: "8.5 in")])
            case .text: return (.controls, "Caption", [NSTextField(string: "Untitled")])
            case .search: return (.controls, "Search", [NSSearchField()])
            case .secure: return (.controls, "Password", [NSSecureTextField(string: "Rasm")])
            case .color: return (.controls, "Fill", [NSColorWell()])
            case .date: return (.controls, "Date", [NSDatePicker()])
            case .quit:
                let name = ProcessInfo.processInfo.processName
                return (nil, name, [NSMenuItem(title: "Quit \(name)", action: #selector(NSApplication.terminate(_:)), keyEquivalent: "q")])
            case .export:
                let keyed: [(title: String, key: String, mask: NSEvent.ModifierFlags)] = [
                    ("Export…", "e", [.command, .shift, .option]), ("Export Page", String(Character(NSEvent.SpecialKey.f5.unicodeScalar)), [.control]), ("Export Spread", "s", [.command, .function]),
                ]
                return (
                    nil,
                    "File",
                    keyed.map { title, key, mask in
                        let item = NSMenuItem(title: title, action: action, keyEquivalent: key)
                        item.keyEquivalentModifierMask = mask
                        return item
                    },
                )
            case .item: return (nil, "View", [NSMenuItem(title: "Show Grid", action: action, keyEquivalent: "'")])
            case .region:
                let region = Region(text: "Canvas")
                region.setAccessibilityElement(true)
                return (.covered, nil, [region])
            case .context:
                let (align, arrange) = (NSMenu(title: "Align"), NSMenu(title: "Arrange"))
                align.items = ["Left", "Right"].map { title in NSMenuItem(title: title, action: action, keyEquivalent: "") }
                let aligning = NSMenuItem(title: "Align", action: nil, keyEquivalent: "")
                aligning.submenu = align
                arrange.items = [aligning]
                let arranging = NSMenuItem(title: "Arrange", action: nil, keyEquivalent: "")
                arranging.submenu = arrange
                let edits: [NSMenuItem] = ["Cut", "Copy", "Paste"].map { title in NSMenuItem(title: title, action: action, keyEquivalent: "") }
                return (.covered, "Context", edits + [arranging, aligning] + align.items)
            case .siblings:
                let siblings = (1...3).map { number in
                    let sibling = NSButton(title: "Swatch", target: nil, action: action)
                    sibling.toolTip = "Swatch \(number)"
                    return sibling
                }
                return (.panel, nil, siblings)
            case .progress:
                let indicator = NSProgressIndicator()
                indicator.isIndeterminate = false
                return (.progress, nil, [indicator])
            case .bleed: return (.sheet, nil, [NSButton(checkboxWithTitle: "Include Bleed", target: nil, action: action)])
            case .done: return (.popover, "Details", [NSButton(title: "Done", target: nil, action: action)])
            case .status: return (.status, "Status", ["Pause", "Resume"].map { title in NSMenuItem(title: title, action: action, keyEquivalent: "") })
            case .drawer: return (.controls, "Tools", [NSButton(title: "All Tools", target: nil, action: #selector(Delegate.drew(_:)))])
        }
    }

    // --- [REPORT]

    @MainActor
    func fact(of object: any NSObject & NSAccessibilityProtocol, member: Int) -> Report.Control {
        let element = unsafe NSAccessibility.unignoredDescendant(of: object) as? any NSAccessibilityProtocol ?? object
        let value: JSONValue? =
            switch element.accessibilityValue() {
                case let text as String: .string(text)
                case let date as Date: .string(date.formatted(Date.ISO8601FormatStyle(includingFractionalSeconds: true)))
                case let number as NSNumber: .number(number.doubleValue)
                default: nil
            }
        return Report.Control(
            entry: self,
            member: member,
            role: element.isAccessibilityElement() ? element.accessibilityRole()?.rawValue : nil,
            subrole: element.accessibilitySubrole()?.rawValue,
            parent: unsafe (element.accessibilityParent().flatMap(NSAccessibility.unignoredAncestor(of:)) as? any NSAccessibilityProtocol)?.accessibilityRole()?.rawValue,
            title: element.accessibilityTitle(),
            description: element.accessibilityLabel(),
            caption: (element.accessibilityTitleUIElement() as? any NSAccessibilityProtocol)?.accessibilityValue() as? String,
            inputs: element.accessibilityUserInputLabels(),
            help: element.accessibilityHelp(),
            value: value,
            shortcut: (object as? NSMenuItem).flatMap { item in item.keyEquivalent.isEmpty ? nil : Report.Shortcut(key: item.keyEquivalent, modifiers: .known(item.keyEquivalentModifierMask)) },
            window: unsafe (object as? NSView)?.window?.windowNumber,
            frame: unsafe (object as? NSView)?.window.map { _ in element.accessibilityFrame() },
            customActions: element.accessibilityCustomActions()?.map(\.name),
            columns: (object as? NSTableView)?.tableColumns.map(\.title),
            rows: (object as? NSTableView)?.numberOfRows,
        )
    }
}

// --- [SERVICES] ------------------------------------------------------------------------

final class Region: NSView {
    private let attributes: [NSAttributedString.Key: Any] = [.font: NSFont.systemFont(ofSize: 28), .foregroundColor: NSColor.labelColor]

    var text = "" {
        didSet {
            needsDisplay = true
            invalidateIntrinsicContentSize()
        }
    }

    convenience init(text: String) {
        self.init(frame: .zero)
        self.text = text
    }

    override var intrinsicContentSize: NSSize { NSRect(origin: .zero, size: text.size(withAttributes: attributes)).insetBy(dx: -24, dy: -24).size }

    override func draw(_: NSRect) {
        NSColor.textBackgroundColor.setFill()
        bounds.fill()
        text.draw(in: bounds.insetBy(dx: 24, dy: 24), withAttributes: attributes)
    }
}

@MainActor
final class Rows: NSObject, NSTableViewDataSource, NSTableViewDelegate, NSOutlineViewDataSource, NSOutlineViewDelegate {
    // --- [TABLE]

    private let layers: [[String]] = [["Background", "12 KB"], ["Text", "4 KB"], ["Guides", "1 KB"], ["Images", "2.1 MB"]]

    func numberOfRows(in _: NSTableView) -> Int { layers.count }

    func tableView(_ tableView: NSTableView, viewFor tableColumn: NSTableColumn?, row: Int) -> NSView? {
        tableColumn.flatMap(tableView.tableColumns.firstIndex(of:)).map { column in NSTextField(labelWithString: layers[row][column]) }
    }

    // --- [OUTLINE]

    private let tree: [NSString?: [NSString]] = [nil: ["Pages", "Masters"], "Pages": ["1", "2", "3"], "Masters": ["A-Master"]]

    func outlineView(_: NSOutlineView, numberOfChildrenOfItem item: Any?) -> Int { tree[item as? NSString, default: []].count }

    func outlineView(_: NSOutlineView, child index: Int, ofItem item: Any?) -> Any { tree[item as? NSString, default: []][index] }

    func outlineView(_: NSOutlineView, isItemExpandable item: Any) -> Bool { tree[item as? NSString] != nil }

    func outlineView(_: NSOutlineView, viewFor _: NSTableColumn?, item: Any) -> NSView? { (item as? String).map(NSTextField.init(labelWithString:)) }
}

final class Drawer: NSPanel {
    override var canBecomeKey: Bool { true }

    override func cancelOperation(_: Any?) { orderOut(nil) }
}

final class Layout: NSDocument {
    nonisolated static let folder = URL.temporaryDirectory.appending(component: ProcessInfo.processInfo.globallyUniqueString, directoryHint: .isDirectory)

    override static var autosavesInPlace: Bool { false }

    override func data(ofType _: String) throws -> Data { Data() }
}
