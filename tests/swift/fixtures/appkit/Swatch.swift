public import AppKit

// --- [SERVICES] ------------------------------------------------------------------------

public final class Swatch: NSView {
    public static let color: NSColor = NSColor(srgbRed: 1, green: 0, blue: 0, alpha: 1)

    public override var isOpaque: Bool { true }

    public override func draw(_ dirtyRect: NSRect) {
        Self.color.setFill()
        dirtyRect.fill()
    }
}
