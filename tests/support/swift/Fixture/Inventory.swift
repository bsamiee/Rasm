import AppKit
import Encoding

// --- [MODELS] --------------------------------------------------------------------------

@MainActor
final class Region: NSView {
    override var acceptsFirstResponder: Bool { true }

    override func draw(_: NSRect) {
        NSColor.textBackgroundColor.setFill()
        bounds.fill()
        NSColor.separatorColor.setStroke()
        NSBezierPath(rect: bounds.insetBy(dx: 12, dy: 12)).stroke()
    }

    override func keyDown(with _: NSEvent) {}

    override func keyUp(with _: NSEvent) {}
}

@MainActor
final class Rows: NSObject, NSTableViewDataSource, NSTableViewDelegate, NSOutlineViewDataSource, NSOutlineViewDelegate {
    let layers: [[String]] = [["Background", "12 KB"], ["Text", "4 KB"], ["Guides", "1 KB"], ["Images", "2.1 MB"]]
    let roots: [NSString] = ["Pages", "Masters"]
    let tree: [NSString: [NSString]] = ["Pages": ["1", "2", "3"], "Masters": ["A-Master"]]
    var added: (NSTableRowView, Int) -> Void = { _, _ in }

    func numberOfRows(in _: NSTableView) -> Int { layers.count }

    func tableView(_ tableView: NSTableView, viewFor tableColumn: NSTableColumn?, row: Int) -> NSView? {
        tableColumn.flatMap(tableView.tableColumns.firstIndex(of:)).map { column in NSTextField(labelWithString: layers[row][column]) }
    }

    func tableView(_: NSTableView, didAdd rowView: NSTableRowView, forRow row: Int) { added(rowView, row) }

    func outlineView(_: NSOutlineView, numberOfChildrenOfItem item: Any?) -> Int { children(of: item).count }

    func outlineView(_: NSOutlineView, child index: Int, ofItem item: Any?) -> Any { children(of: item)[index] }

    func outlineView(_: NSOutlineView, isItemExpandable item: Any) -> Bool { !children(of: item).isEmpty }

    func outlineView(_: NSOutlineView, viewFor _: NSTableColumn?, item: Any) -> NSView? {
        (item as? String).map(NSTextField.init(labelWithString:))
    }

    private func children(of item: Any?) -> [NSString] {
        switch item {
            case nil: roots
            case let parent as NSString: tree[parent] ?? []
            default: []
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension Control {
    var caption: String? {
        switch self {
            case .action: "Document"
            case .toggle: "Grid"
            case .popup: "Page Size"
            case .deferred: "Zoom"
            case .combo: "Font"
            case .radio: "Orientation"
            case .tabs: "Panel"
            case .segments: "Align"
            case .slider: "Opacity"
            case .stepper: "Copies"
            case .measure: "Size"
            case .text: "Caption"
            case .search: "Search"
            case .secure: "Password"
            case .color: "Fill"
            case .date: "Date"
            case .siblings: "Swatches"
            case .tools, .table, .outline, .row, .item, .region: nil
        }
    }

    @MainActor
    func members(sending action: Selector, to target: AnyObject, rows: Rows) -> [any NSObject & NSAccessibilityProtocol] {
        switch self {
            case .action: [NSButton(title: "Apply", target: target, action: action)]
            case .toggle: [NSButton(checkboxWithTitle: "Snap to Grid", target: target, action: action)]
            case .popup:
                [
                    {
                        let popup = NSPopUpButton(frame: .zero, pullsDown: false)
                        popup.addItems(withTitles: ["Letter", "A4", "Tabloid"])
                        popup.selectItem(at: 1)
                        (popup.target, popup.action) = (target, action)
                        return popup
                    }()
                ]
            case .deferred:
                [
                    {
                        let popup = NSPopUpButton(frame: .zero, pullsDown: false)
                        popup.addItem(withTitle: "100%")
                        (popup.target, popup.action) = (target, action)
                        return popup
                    }()
                ]
            case .combo:
                [
                    {
                        let combo = NSComboBox()
                        combo.addItems(withObjectValues: ["Helvetica", "Minion Pro", "Myriad Pro"])
                        combo.selectItem(at: 0)
                        (combo.target, combo.action) = (target, action)
                        return combo
                    }()
                ]
            case .radio:
                ["Portrait", "Landscape"].enumerated().map { index, title in
                    let radio = NSButton(radioButtonWithTitle: title, target: target, action: action)
                    radio.state = index == 0 ? .on : .off
                    return radio
                }
            case .tabs:
                [
                    {
                        let tabs = NSTabView()
                        ["Text", "Frame"].map { label in
                            let item = NSTabViewItem()
                            item.label = label
                            return item
                        }.forEach(tabs.addTabViewItem)
                        return tabs
                    }()
                ]
            case .segments: [segments(["Left", "Center", "Right"], sending: action, to: target)]
            case .tools:
                [
                    {
                        let tools = segments(["Select", "Type", "Frame"], sending: action, to: target)
                        let grouped = NSMenu(title: "Frame Tools")
                        for title in ["Rectangle Frame", "Ellipse Frame", "Polygon Frame"] { grouped.addItem(withTitle: title, action: action, keyEquivalent: "").target = target }
                        tools.setMenu(grouped, forSegment: 2)
                        tools.setShowsMenuIndicator(true, forSegment: 2)
                        return tools
                    }()
                ]
            case .table:
                [
                    {
                        let table = NSTableView()
                        ["Name", "Size"].map { title in
                            let column = NSTableColumn()
                            column.title = title
                            return column
                        }.forEach(table.addTableColumn)
                        (table.dataSource, table.delegate, table.allowsMultipleSelection) = (rows, rows, true)
                        table.reloadData()
                        table.selectRowIndexes([0, 2], byExtendingSelection: false)
                        return table
                    }()
                ]
            case .outline:
                [
                    {
                        let outline = NSOutlineView()
                        let column = NSTableColumn()
                        column.title = "Pages"
                        outline.addTableColumn(column)
                        unsafe outline.outlineTableColumn = column
                        (outline.dataSource, outline.delegate) = (rows, rows)
                        outline.reloadData()
                        outline.expandItem(nil, expandChildren: true)
                        outline.selectRowIndexes([1], byExtendingSelection: false)
                        return outline
                    }()
                ]
            case .row: []
            case .slider:
                [
                    {
                        let slider = NSSlider(value: 40, minValue: 0, maxValue: 100, target: target, action: action)
                        (slider.numberOfTickMarks, slider.allowsTickMarkValuesOnly) = (11, true)
                        return slider
                    }()
                ]
            case .stepper:
                [
                    {
                        let stepper = NSStepper()
                        (stepper.minValue, stepper.maxValue, stepper.increment, stepper.doubleValue) = (1, 99, 1, 3)
                        (stepper.target, stepper.action) = (target, action)
                        return stepper
                    }()
                ]
            case .measure: ["8.5 in", "51p0"].map(NSTextField.init(string:))
            case .text: [NSTextField(string: "Untitled")]
            case .search:
                [
                    {
                        let search = NSSearchField()
                        search.placeholderString = "Find"
                        return search
                    }()
                ]
            case .secure: [NSSecureTextField(string: "Rasm")]
            case .color:
                [
                    {
                        let well = NSColorWell(style: .default)
                        well.color = NSColor(srgbRed: 0.8, green: 0.2, blue: 0.1, alpha: 1)
                        return well
                    }()
                ]
            case .date:
                [
                    {
                        let picker = NSDatePicker()
                        (picker.datePickerStyle, picker.datePickerElements) = (.textFieldAndStepper, .yearMonthDay)
                        picker.dateValue = Date(timeIntervalSince1970: 1_795_564_800)
                        return picker
                    }()
                ]
            case .item:
                [
                    {
                        let item = NSMenuItem(title: "Show Grid", action: action, keyEquivalent: "'")
                        (item.target, item.state) = (target, .on)
                        return item
                    }()
                ]
            case .region: [Region()]
            case .siblings:
                (1...8).map { number in
                    let sibling = NSButton(title: "Swatch", target: target, action: action)
                    sibling.toolTip = "Swatch \(number)"
                    return sibling
                }
        }
    }

    @MainActor
    func fact(of object: any NSObject & NSAccessibilityProtocol, member: Int) -> Fact {
        let (value, range, step, options, selection): (JSONValue?, ClosedRange<Double>?, Double?, [String]?, [Int]?) =
            switch object {
                case let popup as NSPopUpButton: (popup.titleOfSelectedItem.map(JSONValue.string), nil, nil, popup.itemTitles, [popup.indexOfSelectedItem].filter { index in index >= 0 })
                case let button as NSButton: (.number(Double(button.state.rawValue)), nil, nil, nil, nil)
                case let segmented as NSSegmentedControl:
                    (nil, nil, nil, (0..<segmented.segmentCount).compactMap(segmented.label(forSegment:)), (0..<segmented.segmentCount).filter(segmented.isSelected(forSegment:)))
                case let slider as NSSlider:
                    (.number(slider.doubleValue), slider.minValue...slider.maxValue, slider.allowsTickMarkValuesOnly ? slider.tickMarkValue(at: 1) - slider.tickMarkValue(at: 0) : nil, nil, nil)
                case let stepper as NSStepper: (.number(stepper.doubleValue), stepper.minValue...stepper.maxValue, stepper.increment, nil, nil)
                case let combo as NSComboBox:
                    (.string(combo.stringValue), nil, nil, combo.objectValues.map(String.init(describing:)), [combo.indexOfSelectedItem].filter { index in index >= 0 })
                case let field as NSTextField: (.string(field.stringValue), nil, nil, nil, nil)
                case let picker as NSDatePicker: (.string(picker.dateValue.formatted(.iso8601)), nil, nil, nil, nil)
                case let well as NSColorWell:
                    (
                        well.color.usingColorSpace(.sRGB).map { color in .array([color.redComponent, color.greenComponent, color.blueComponent, color.alphaComponent].map(JSONValue.number)) }, nil,
                        nil, nil, nil,
                    )
                case let tabs as NSTabView: (nil, nil, nil, tabs.tabViewItems.map(\.label), tabs.selectedTabViewItem.map { item in [tabs.indexOfTabViewItem(item)] })
                case let table as NSTableView: (nil, nil, nil, nil, Array(table.selectedRowIndexes))
                case let row as NSTableRowView: (.bool(row.isSelected), nil, nil, nil, nil)
                case let item as NSMenuItem: (.number(Double(item.state.rawValue)), nil, nil, nil, nil)
                default: (nil, nil, nil, nil, nil)
            }
        let view = object as? NSView
        let item = object as? NSMenuItem
        let element: any NSAccessibilityProtocol = (object as? NSControl).flatMap { control in control.isAccessibilityElement() ? nil : control.cell } ?? object
        let (title, label): (String?, String?) = item.map { item in (item.title, nil) } ?? (element.accessibilityTitle(), element.accessibilityLabel())
        return Fact(
            control: self,
            member: member,
            className: String(describing: type(of: object)),
            elementClass: String(describing: type(of: element)),
            element: element.isAccessibilityElement(),
            role: element.accessibilityRole()?.rawValue,
            subrole: element.accessibilitySubrole()?.rawValue,
            title: title.flatMap { title in title.isEmpty ? nil : title },
            label: label.flatMap { label in label.isEmpty ? nil : label },
            help: element.accessibilityHelp().flatMap { help in help.isEmpty ? nil : help },
            value: value,
            enabled: (object as? NSControl)?.isEnabled ?? item?.isEnabled ?? true,
            range: range,
            step: step,
            options: options,
            selection: selection,
            shortcut: item.flatMap { item in item.keyEquivalent.isEmpty ? nil : Fact.Shortcut(key: item.keyEquivalent, modifiers: item.keyEquivalentModifierMask.rawValue) },
            window: unsafe view?.window?.windowNumber,
            frame: view.map { view in view.convert(view.bounds, to: nil) },
        )
    }

    @MainActor
    private func segments(_ labels: [String], sending action: Selector, to target: AnyObject) -> NSSegmentedControl {
        let segmented = NSSegmentedControl(labels: labels, trackingMode: .selectOne, target: target, action: action)
        segmented.selectedSegment = 0
        return segmented
    }
}
