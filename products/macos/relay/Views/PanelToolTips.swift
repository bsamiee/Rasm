import AppKit
import SwiftUI

// --- [VIEWS] ---------------------------------------------------------------------------

struct PanelToolTips: NSViewRepresentable {
    func makeNSView(context _: Context) -> PanelToolTipsView {
        PanelToolTipsView()
    }

    func updateNSView(_: PanelToolTipsView, context _: Context) {}
}

final class PanelToolTipsView: NSView {
    override func viewWillMove(toWindow newWindow: NSWindow?) {
        super.viewWillMove(toWindow: newWindow)
        newWindow?.allowsToolTipsWhenApplicationIsInactive = true
    }
}
