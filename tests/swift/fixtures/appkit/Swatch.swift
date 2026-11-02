public import AppKit

// --- [SERVICES] ------------------------------------------------------------------------

public final class Swatch: NSView {
    public nonisolated static let colors: [[NSColor]] = [
        [
            NSColor(srgbRed: 1, green: 0, blue: 0, alpha: 1), NSColor(srgbRed: 0, green: 1, blue: 0, alpha: 1), NSColor(srgbRed: 0, green: 0, blue: 1, alpha: 1),
            NSColor(srgbRed: 1, green: 1, blue: 1, alpha: 1),
        ],
        [
            NSColor(srgbRed: 0, green: 0, blue: 0, alpha: 1), NSColor(srgbRed: 0.25, green: 0.5, blue: 0.75, alpha: 1), NSColor(srgbRed: 1, green: 0.5, blue: 0, alpha: 1),
            NSColor(srgbRed: 0.5, green: 0.5, blue: 0.5, alpha: 1),
        ],
    ]

    public nonisolated static let size: NSSize = NSSize(width: 160, height: 80)

    public override var isOpaque: Bool { true }

    public override var isFlipped: Bool { true }

    public nonisolated static func cell(row: Int, column: Int, in bounds: NSRect) -> NSRect {
        let size: NSSize = NSSize(width: bounds.width / CGFloat(colors[row].count), height: bounds.height / CGFloat(colors.count))
        return NSRect(origin: NSPoint(x: CGFloat(column) * size.width, y: CGFloat(row) * size.height), size: size)
    }

    public override func draw(_ dirtyRect: NSRect) {
        for (row, colors): (Int, [NSColor]) in Self.colors.enumerated() {
            for (column, color): (Int, NSColor) in colors.enumerated() {
                color.setFill()
                Self.cell(row: row, column: column, in: bounds).intersection(dirtyRect).fill()
            }
        }
    }
}
