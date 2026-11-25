import AppKit
import Encoding

// --- [MODELS] --------------------------------------------------------------------------

@MainActor
final class Region: NSView {
    private let attributes: [NSAttributedString.Key: Any] = [.font: NSFont.systemFont(ofSize: 28), .foregroundColor: NSColor.labelColor]

    var text = "" {
        didSet {
            needsDisplay = true
            invalidateIntrinsicContentSize()
        }
    }

    override var acceptsFirstResponder: Bool { true }

    override var intrinsicContentSize: NSSize { NSRect(origin: .zero, size: text.size(withAttributes: attributes)).insetBy(dx: -24, dy: -24).size }

    override func draw(_: NSRect) {
        NSColor.textBackgroundColor.setFill()
        bounds.fill()
        text.draw(in: bounds.insetBy(dx: 24, dy: 24), withAttributes: attributes)
    }

    override func keyDown(with _: NSEvent) {}

    override func keyUp(with _: NSEvent) {}
}

@MainActor
final class Rows: NSObject, NSTableViewDataSource, NSTableViewDelegate, NSOutlineViewDataSource, NSOutlineViewDelegate {
    let layers: [[String]] = [["Background", "12 KB"], ["Text", "4 KB"], ["Guides", "1 KB"], ["Images", "2.1 MB"]]
    let tree: [NSString?: [NSString]] = [nil: ["Pages", "Masters"], "Pages": ["1", "2", "3"], "Masters": ["A-Master"]]

    func numberOfRows(in _: NSTableView) -> Int { layers.count }

    func tableView(_ tableView: NSTableView, viewFor tableColumn: NSTableColumn?, row: Int) -> NSView? {
        tableColumn.flatMap(tableView.tableColumns.firstIndex(of:)).map { column in NSTextField(labelWithString: layers[row][column]) }
    }

    func outlineView(_: NSOutlineView, numberOfChildrenOfItem item: Any?) -> Int { tree[item as? NSString, default: []].count }

    func outlineView(_: NSOutlineView, child index: Int, ofItem item: Any?) -> Any { tree[item as? NSString, default: []][index] }

    func outlineView(_: NSOutlineView, isItemExpandable item: Any) -> Bool { tree[item as? NSString] != nil }

    func outlineView(_: NSOutlineView, viewFor _: NSTableColumn?, item: Any) -> NSView? { (item as? String).map(NSTextField.init(labelWithString:)) }
}

final class MeasureCell: NSTextFieldCell {
    override func setAccessibilityValue(_: Any?) {}
}

final class Keyboard: NSTextView {
    override func keyDown(with event: NSEvent) {
        super.keyDown(
            with: NSEvent.keyEvent(
                with: event.type,
                location: event.locationInWindow,
                modifierFlags: event.modifierFlags,
                timestamp: event.timestamp,
                windowNumber: event.windowNumber,
                context: nil,
                characters: event.characters(byApplyingModifiers: event.modifierFlags) ?? "",
                charactersIgnoringModifiers: event.characters(byApplyingModifiers: []) ?? "",
                isARepeat: event.isARepeat,
                keyCode: event.keyCode,
            ) ?? event
        )
    }
}

final class Layout: NSDocument {
    nonisolated static let folder = URL.temporaryDirectory.appending(component: ProcessInfo.processInfo.globallyUniqueString, directoryHint: .isDirectory)

    override static var autosavesInPlace: Bool { false }

    override func data(ofType _: String) throws -> Data { Data() }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension Inventory {
    @MainActor
    func row(rows: Rows) -> (surface: Surface?, caption: String?, members: [any NSObject & NSAccessibilityProtocol]) {
        let action = #selector(Delegate.acted(_:))
        switch self {
            case .action: return (.controls, "Document", [NSButton(title: "Apply", target: nil, action: action)])
            case .toggle: return (.controls, "Grid", [NSButton(checkboxWithTitle: "Snap to Grid", target: nil, action: action)])
            case .popup, .deferred:
                let (titles, selected, caption) = self == .popup ? (["Letter", "A4", "Tabloid"], 1, "Page Size") : (["100%"], 0, "Zoom")
                let popup = NSPopUpButton(frame: .zero, pullsDown: false)
                popup.addItems(withTitles: titles)
                popup.selectItem(at: selected)
                popup.action = action
                popup.menu?.title = caption
                return (.controls, caption, [popup])
            case .combo:
                let combo = NSComboBox()
                combo.addItems(withObjectValues: ["Helvetica", "Minion Pro", "Myriad Pro"])
                combo.selectItem(at: 0)
                combo.action = action
                return (.controls, "Font", [combo])
            case .radio:
                let radios = ["Portrait", "Landscape"].map { title in NSButton(radioButtonWithTitle: title, target: nil, action: action) }
                radios[0].state = .on
                return (.controls, "Orientation", radios)
            case .tabs:
                let tabs = NSTabView()
                for entry: Self in [.text, .search] { tabs.addTabViewItem(NSTabViewItem(identifier: entry)) }
                return (.controls, "Panel", [tabs])
            case .segments:
                let segmented = NSSegmentedControl(labels: ["Left", "Center", "Right"], trackingMode: .selectOne, target: nil, action: action)
                segmented.selectedSegment = 0
                return (.controls, "Align", [segmented])
            case .tools:
                let tools = NSSegmentedControl(labels: ["Select", "Type", "Frame"], trackingMode: .selectOne, target: nil, action: action)
                let grouped = NSMenu(title: "Frame Tools")
                grouped.items = ["Rectangle Frame", "Ellipse Frame", "Polygon Frame"].map { title in NSMenuItem(title: title, action: action, keyEquivalent: "") }
                tools.selectedSegment = 0
                tools.setMenu(grouped, forSegment: 2)
                tools.setShowsMenuIndicator(true, forSegment: 2)
                return (.toolbar, nil, [tools] + (grouped.items as [any NSObject & NSAccessibilityProtocol]))
            case .table:
                let table = NSTableView()
                for title in ["Name", "Size"] {
                    let column = NSTableColumn()
                    column.title = title
                    table.addTableColumn(column)
                }
                table.dataSource = rows
                table.delegate = rows
                table.allowsMultipleSelection = true
                table.reloadData()
                table.selectRowIndexes([0, 2], byExtendingSelection: false)
                return (.lists, nil, [table])
            case .outline:
                let outline = NSOutlineView()
                let column = NSTableColumn()
                column.title = "Pages"
                outline.addTableColumn(column)
                unsafe outline.outlineTableColumn = column
                outline.dataSource = rows
                outline.delegate = rows
                outline.reloadData()
                outline.expandItem(nil, expandChildren: true)
                outline.selectRowIndexes([1], byExtendingSelection: false)
                return (.lists, nil, [outline])
            case .slider:
                let slider = NSSlider(value: 40, minValue: 0, maxValue: 100, target: nil, action: action)
                slider.numberOfTickMarks = 11
                slider.allowsTickMarkValuesOnly = true
                return (.controls, "Opacity", [slider])
            case .stepper:
                let stepper = NSStepper()
                stepper.minValue = 1
                stepper.maxValue = 99
                stepper.increment = 1
                stepper.doubleValue = 3
                stepper.action = action
                return (.controls, "Copies", [stepper])
            case .measure:
                let fields = ["8.5 in", "51p0"].map { text in
                    let field = NSTextField()
                    let cell = MeasureCell(textCell: text)
                    cell.isEditable = true
                    cell.isBezeled = true
                    cell.isScrollable = true
                    field.cell = cell
                    field.action = action
                    field.widthAnchor.constraint(equalToConstant: 96).isActive = true
                    return field
                }
                return (.controls, "Size", fields)
            case .text: return (.controls, "Caption", [NSTextField(string: "Untitled")])
            case .search:
                let search = NSSearchField()
                search.placeholderString = "Find"
                return (.controls, "Search", [search])
            case .secure: return (.controls, "Password", [NSSecureTextField(string: "Rasm")])
            case .color:
                let well = NSColorWell(style: .default)
                well.color = NSColor(srgbRed: 0.8, green: 0.2, blue: 0.1, alpha: 1)
                return (.controls, "Fill", [well])
            case .date:
                let picker = NSDatePicker()
                picker.datePickerStyle = .textFieldAndStepper
                picker.datePickerElements = .yearMonthDay
                picker.dateValue = Date(timeIntervalSince1970: 1_795_564_800)
                return (.controls, "Date", [picker])
            case .quit:
                let name = ProcessInfo.processInfo.processName
                return (nil, name, [NSMenuItem(title: "Quit \(name)", action: #selector(NSApplication.terminate(_:)), keyEquivalent: "q")])
            case .export: return (nil, "File", [NSMenuItem(title: "Export…", action: #selector(Delegate.exported(_:)), keyEquivalent: "")])
            case .select: return (nil, "Edit", [NSMenuItem(title: "Select All", action: #selector(NSText.selectAll(_:)), keyEquivalent: "a")])
            case .item:
                let item = NSMenuItem(title: "Show Grid", action: action, keyEquivalent: "'")
                item.state = .on
                return (nil, "View", [item])
            case .region:
                let region = Region()
                region.text = "Canvas"
                region.setAccessibilityElement(true)
                return (.covered, nil, [region])
            case .context: return (.covered, "Context", ["Cut", "Copy", "Paste"].map { title in NSMenuItem(title: title, action: action, keyEquivalent: "") })
            case .siblings:
                let siblings = (1...8).map { number in
                    let sibling = NSButton(title: "Swatch", target: nil, action: action)
                    sibling.toolTip = "Swatch \(number)"
                    return sibling
                }
                return (.panel, nil, siblings)
            case .answer:
                let answers = [(NSApplication.ModalResponse.cancel, "Cancel", "\u{1b}"), (NSApplication.ModalResponse.OK, "Export", "\r")].map { response, title, key in
                    let button = NSButton(title: title, target: nil, action: #selector(Delegate.answered(_:)))
                    button.tag = response.rawValue
                    button.keyEquivalent = key
                    return button
                }
                return (.dialog, nil, answers)
            case .progress:
                let indicator = NSProgressIndicator()
                indicator.style = .bar
                indicator.isIndeterminate = true
                let cancel = NSButton(title: "Cancel", target: nil, action: action)
                cancel.isEnabled = false
                return (.progress, nil, [indicator, cancel])
            case .bleed: return (.sheet, nil, [NSButton(checkboxWithTitle: "Include Bleed", target: nil, action: action)])
            case .done: return (.popover, "Details", [NSButton(title: "Done", target: nil, action: action)])
            case .status: return (.status, "Status", ["Pause", "Resume"].map { title in NSMenuItem(title: title, action: action, keyEquivalent: "") })
        }
    }

    @MainActor
    func fact(of object: any NSObject & NSAccessibilityProtocol, member: Int) -> Report.Control {
        let element = unsafe NSAccessibility.unignoredDescendant(of: object) as? any NSAccessibilityProtocol ?? object
        let item = object as? NSMenuItem
        let value: JSONValue? =
            switch element.accessibilityValue() {
                case let text as String: .string(text)
                case let date as Date: .string(date.formatted(.iso8601))
                case let number as NSNumber: .number(number.doubleValue)
                default: nil
            }
        let range: ClosedRange<Double>? =
            switch (element.accessibilityMinValue(), element.accessibilityMaxValue()) {
                case (let low as Double, let high as Double): low...high
                default: nil
            }
        return Report.Control(
            entry: self,
            member: member,
            role: element.isAccessibilityElement() ? element.accessibilityRole()?.rawValue : nil,
            subrole: element.accessibilitySubrole()?.rawValue,
            parent: unsafe (element.accessibilityParent().flatMap(NSAccessibility.unignoredAncestor(of:)) as? any NSAccessibilityProtocol)?.accessibilityRole()?.rawValue,
            title: element.accessibilityTitle(),
            caption: (element.accessibilityTitleUIElement() as? any NSAccessibilityProtocol)?.accessibilityValue() as? String,
            help: element.accessibilityHelp(),
            value: value,
            enabled: element.isAccessibilityEnabled(),
            range: range,
            step: (object as? NSStepper)?.increment,
            mark: item.map { item in Lenient.known(item.state) },
            shortcut: item.flatMap { item in item.keyEquivalent.isEmpty ? nil : Report.Shortcut(key: item.keyEquivalent, modifiers: .known(item.keyEquivalentModifierMask)) },
            window: unsafe (object as? NSView)?.window?.windowNumber,
            frame: unsafe (object as? NSView)?.window.map { _ in element.accessibilityFrame() },
        )
    }
}
