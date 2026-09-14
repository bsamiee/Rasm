import AppKit
import OSLog

enum Activation {
  private static let logger: Logger = Logger(subsystem: "app.rasm.relay", category: "Activation")

  static func requestFront() async {
    let opened: Result<NSRunningApplication, any Error> = await Result {
      try await NSWorkspace.shared.openApplication(
        at: Bundle.main.bundleURL, configuration: NSWorkspace.OpenConfiguration())
    }
    if case .failure(let error) = opened {
      logger.error("Activation refused: \(String(describing: error), privacy: .public)")
    }
  }
}
