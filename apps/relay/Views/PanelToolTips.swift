import AppKit
import SwiftUI

struct PanelToolTips: NSViewRepresentable {
  func makeNSView(context: Context) -> PanelToolTipsView {
    PanelToolTipsView()
  }

  func updateNSView(_ view: PanelToolTipsView, context: Context) {}
}

final class PanelToolTipsView: NSView {
  override func viewDidMoveToWindow() {
    super.viewDidMoveToWindow()
    window?.allowsToolTipsWhenApplicationIsInactive = true
  }
}
