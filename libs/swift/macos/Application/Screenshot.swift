public import AppKit
public import ScreenCaptureKit

// --- [ERRORS] --------------------------------------------------------------------------

public enum ScreenshotError: LocalizedError {
    case inactive
    case unavailable(CGWindowID?)

    public var errorDescription: String? {
        switch self {
            case .inactive: "The application could not be activated for capture"
            case .unavailable(let id): id.map { id in "Window \(id) is unavailable for capture" } ?? "The application has no visible window to capture"
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension NSRunningApplication {
    public func screenshot(windowID: CGWindowID? = nil) async -> Result<(window: SCWindow, windows: [SCWindow], image: SCScreenshotOutput), any Error> {
        await Result {
            let (states, sink): (AsyncStream<Bool>, AsyncStream<Bool>.Continuation) = AsyncStream.makeStream()
            let observations: [NSKeyValueObservation] = [\Self.isActive, \.isTerminated].map { keyPath in
                observe(keyPath, options: [.initial, .new]) { application, _ in
                    if application.isActive || application.isTerminated { sink.yield(application.isActive) }
                }
            }
            defer {
                for observation: NSKeyValueObservation in observations { observation.invalidate() }
                sink.finish()
            }
            guard activate(), await states.first(where: { _ in true }) == true else { throw ScreenshotError.inactive }
            let windows: [SCWindow] = try await SCShareableContent.excludingDesktopWindows(true, onScreenWindowsOnly: true).windows.filter { window in
                window.owningApplication?.processID == processIdentifier
            }
            let selected: SCWindow?
            if let windowID {
                selected = windows.first { window in window.windowID == windowID }
            } else {
                guard let ordered: [[String: Any]] = CGWindowListCopyWindowInfo([.optionOnScreenOnly, .excludeDesktopElements], kCGNullWindowID) as? [[String: Any]] else {
                    throw CocoaError(.coderReadCorrupt)
                }
                selected =
                    ordered.lazy.compactMap { row in row[kCGWindowNumber as String] as? CGWindowID }
                    .compactMap { id in windows.first { window in window.windowID == id } }
                    .first
            }
            guard let selected else { throw ScreenshotError.unavailable(windowID) }
            let configuration: SCScreenshotConfiguration = SCScreenshotConfiguration()
            configuration.ignoreShadows = true
            configuration.showsCursor = false
            let image: SCScreenshotOutput = try await SCScreenshotManager.captureScreenshot(contentFilter: SCContentFilter(desktopIndependentWindow: selected), configuration: configuration)
            return (window: selected, windows: windows, image: image)
        }
    }
}
