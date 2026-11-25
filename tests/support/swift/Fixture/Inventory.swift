import AppKit
import Encoding

// --- [MODELS] --------------------------------------------------------------------------

@MainActor
final class Region: NSView {
    var text = "" {
        didSet { needsDisplay = true }
    }

    override var acceptsFirstResponder: Bool { true }

    override func draw(_: NSRect) {
        NSColor.textBackgroundColor.setFill()
        bounds.fill()
        text.draw(in: bounds.insetBy(dx: 24, dy: 24), withAttributes: [.font: NSFont.systemFont(ofSize: 28), .foregroundColor: NSColor.labelColor])
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
    override static var autosavesInPlace: Bool { false }

    override func data(ofType _: String) throws -> Data { Data() }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension Inventory {
    @MainActor
    func row(sending action: Selector, to target: AnyObject, rows: Rows) -> (surface: Surface?, caption: String?, members: [any NSObject & NSAccessibilityProtocol]) {
        switch self {
            case .action: return (.controls, "Document", [NSButton(title: "Apply", target: target, action: action)])
            case .toggle: return (.controls, "Grid", [NSButton(checkboxWithTitle: "Snap to Grid", target: target, action: action)])
            case .popup:
                let popup = NSPopUpButton(frame: .zero, pullsDown: false)
                popup.addItems(withTitles: ["Letter", "A4", "Tabloid"])
                popup.selectItem(at: 1)
                (popup.target, popup.action) = (target, action)
                return (.controls, "Page Size", [popup])
            case .deferred:
                let popup = NSPopUpButton(frame: .zero, pullsDown: false)
                popup.addItem(withTitle: "100%")
                (popup.target, popup.action) = (target, action)
                popup.menu?.title = "Zoom"
                return (.controls, "Zoom", [popup])
            case .combo:
                let combo = NSComboBox()
                combo.addItems(withObjectValues: ["Helvetica", "Minion Pro", "Myriad Pro"])
                combo.selectItem(at: 0)
                (combo.target, combo.action) = (target, action)
                return (.controls, "Font", [combo])
            case .radio:
                let radios = ["Portrait", "Landscape"].map { title in NSButton(radioButtonWithTitle: title, target: target, action: action) }
                radios[0].state = .on
                return (.controls, "Orientation", radios)
            case .tabs:
                let tabs = NSTabView()
                for label in ["Text", "Frame"] {
                    tabs.addTabViewItem(NSTabViewItem())
                    tabs.tabViewItems.last?.label = label
                }
                return (.controls, "Panel", [tabs])
            case .segments:
                let segmented = NSSegmentedControl(labels: ["Left", "Center", "Right"], trackingMode: .selectOne, target: target, action: action)
                segmented.selectedSegment = 0
                return (.controls, "Align", [segmented])
            case .tools:
                let tools = NSSegmentedControl(labels: ["Select", "Type", "Frame"], trackingMode: .selectOne, target: target, action: action)
                let grouped = NSMenu(title: "Frame Tools")
                for title in ["Rectangle Frame", "Ellipse Frame", "Polygon Frame"] { grouped.addItem(withTitle: title, action: action, keyEquivalent: "").target = target }
                tools.selectedSegment = 0
                tools.setMenu(grouped, forSegment: 2)
                tools.setShowsMenuIndicator(true, forSegment: 2)
                return (.toolbar, "Tools", [tools])
            case .table:
                let table = NSTableView()
                for title in ["Name", "Size"] {
                    table.addTableColumn(NSTableColumn())
                    table.tableColumns.last?.title = title
                }
                (table.dataSource, table.delegate, table.allowsMultipleSelection) = (rows, rows, true)
                table.reloadData()
                table.selectRowIndexes([0, 2], byExtendingSelection: false)
                return (.lists, nil, [table])
            case .outline:
                let outline = NSOutlineView()
                outline.addTableColumn(NSTableColumn())
                (outline.tableColumns[0].title, outline.dataSource, outline.delegate) = ("Pages", rows, rows)
                unsafe outline.outlineTableColumn = outline.tableColumns[0]
                outline.reloadData()
                outline.expandItem(nil, expandChildren: true)
                outline.selectRowIndexes([1], byExtendingSelection: false)
                return (.lists, nil, [outline])
            case .slider:
                let slider = NSSlider(value: 40, minValue: 0, maxValue: 100, target: target, action: action)
                (slider.numberOfTickMarks, slider.allowsTickMarkValuesOnly) = (11, true)
                return (.controls, "Opacity", [slider])
            case .stepper:
                let stepper = NSStepper()
                (stepper.minValue, stepper.maxValue, stepper.increment, stepper.doubleValue, stepper.target, stepper.action) = (1, 99, 1, 3, target, action)
                return (.controls, "Copies", [stepper])
            case .measure:
                return (
                    .controls, "Size",
                    ["8.5 in", "51p0"].map { text in
                        let (field, cell) = (NSTextField(), MeasureCell(textCell: text))
                        (cell.isEditable, cell.isBezeled, cell.isScrollable) = (true, true, true)
                        (field.cell, field.target, field.action) = (cell, target, action)
                        field.widthAnchor.constraint(equalToConstant: 96).isActive = true
                        return field
                    },
                )
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
                (picker.datePickerStyle, picker.datePickerElements, picker.dateValue) = (.textFieldAndStepper, .yearMonthDay, Date(timeIntervalSince1970: 1_795_564_800))
                return (.controls, "Date", [picker])
            case .item:
                let item = NSMenuItem(title: "Show Grid", action: action, keyEquivalent: "'")
                (item.target, item.state) = (target, .on)
                return (nil, "View", [item])
            case .region:
                let region = Region()
                region.text = "Canvas"
                region.setAccessibilityElement(true)
                return (.covered, nil, [region])
            case .siblings:
                return (
                    .panel, nil,
                    (1...8).map { number in
                        let sibling = NSButton(title: "Swatch", target: target, action: action)
                        sibling.toolTip = "Swatch \(number)"
                        return sibling
                    },
                )
        }
    }

    @MainActor
    func fact(of object: any NSObject & NSAccessibilityProtocol, member: Int) -> Report.Control {
        let (value, range, step, items): (JSONValue?, ClosedRange<Double>?, Double?, [Report.Item]?) =
            switch object {
                case let popup as NSPopUpButton:
                    (popup.titleOfSelectedItem.map(JSONValue.string), nil, nil, popup.itemArray.map { item in Report.Item(title: item.title, selected: item === popup.selectedItem, level: nil) })
                case let button as NSButton: (.number(Double(button.state.rawValue)), nil, nil, nil)
                case let segmented as NSSegmentedControl:
                    (
                        nil, nil, nil,
                        (0..<segmented.segmentCount).map { segment in
                            Report.Item(title: segmented.label(forSegment: segment) ?? "", selected: segmented.isSelected(forSegment: segment), level: nil)
                        },
                    )
                case let slider as NSSlider:
                    (.number(slider.doubleValue), slider.minValue...slider.maxValue, slider.allowsTickMarkValuesOnly ? slider.tickMarkValue(at: 1) - slider.tickMarkValue(at: 0) : nil, nil)
                case let stepper as NSStepper: (.number(stepper.doubleValue), stepper.minValue...stepper.maxValue, stepper.increment, nil)
                case let combo as NSComboBox:
                    (
                        .string(combo.stringValue), nil, nil,
                        combo.objectValues.enumerated().map { index, value in Report.Item(title: String(describing: value), selected: index == combo.indexOfSelectedItem, level: nil) },
                    )
                case let field as NSTextField: (.string(field.stringValue), nil, nil, nil)
                case let picker as NSDatePicker: (.string(picker.dateValue.formatted(.iso8601)), nil, nil, nil)
                case let well as NSColorWell:
                    (
                        well.color.usingColorSpace(.sRGB).map { color in .array([color.redComponent, color.greenComponent, color.blueComponent, color.alphaComponent].map(JSONValue.number)) }, nil,
                        nil, nil,
                    )
                case let tabs as NSTabView: (nil, nil, nil, tabs.tabViewItems.map { item in Report.Item(title: item.label, selected: item === tabs.selectedTabViewItem, level: nil) })
                case let list as NSTableView:
                    (
                        nil, nil, nil,
                        (0..<list.numberOfRows).map { row in
                            Report.Item(
                                title: (list.view(atColumn: 0, row: row, makeIfNecessary: true) as? NSTextField)?.stringValue ?? "",
                                selected: list.isRowSelected(row),
                                level: (list as? NSOutlineView)?.level(forRow: row),
                            )
                        },
                    )
                case let item as NSMenuItem: (.number(Double(item.state.rawValue)), nil, nil, nil)
                case let region as Region: (.string(region.text), nil, nil, nil)
                default: (nil, nil, nil, nil)
            }
        let item = object as? NSMenuItem
        let element: any NSAccessibilityProtocol = (object as? NSControl).flatMap { control in control.isAccessibilityElement() ? nil : control.cell } ?? object
        let frame = (object as? NSView).map { _ in element.accessibilityFrame() } ?? .zero
        return Report.Control(
            entry: self,
            member: member,
            className: String(describing: type(of: object)),
            role: element.isAccessibilityElement() ? element.accessibilityRole()?.rawValue : nil,
            subrole: element.accessibilitySubrole()?.rawValue,
            parent: (element.accessibilityParent() as? any NSAccessibilityProtocol)?.accessibilityRole()?.rawValue,
            title: item?.title ?? element.accessibilityTitle().flatMap { title in title.isEmpty ? nil : title },
            caption: (object.accessibilityTitleUIElement() as? any NSAccessibilityProtocol)?.accessibilityValue() as? String,
            help: element.accessibilityHelp().flatMap { help in help.isEmpty ? nil : help },
            value: value,
            enabled: (object as? NSControl)?.isEnabled ?? item?.isEnabled ?? true,
            range: range,
            step: step,
            items: items,
            shortcut: item.flatMap { item in item.keyEquivalent.isEmpty ? nil : Report.Shortcut(key: item.keyEquivalent, modifiers: item.keyEquivalentModifierMask.rawValue) },
            window: unsafe (object as? NSView)?.window?.windowNumber,
            frame: frame.isEmpty ? nil : CGRect(x: frame.minX, y: (NSScreen.screens.first?.frame.maxY ?? 0) - frame.maxY, width: frame.width, height: frame.height),
        )
    }
}
