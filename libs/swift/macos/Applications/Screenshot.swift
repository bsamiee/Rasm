public import AppKit
import ImageIO
public import ScreenCaptureKit
public import UniformTypeIdentifiers

// --- [ERRORS] --------------------------------------------------------------------------

public enum ScreenshotFailure: CustomNSError, LocalizedError {
    case content(any Error)
    case noWindow
    case absent(CGWindowID)
    case noDisplay
    case capture(any Error)
    case unrendered
    case encode(UTType)

    public var errorDescription: String? {
        switch self {
            case .content(let error): "ScreenCaptureKit could not list shareable content: \(error.localizedDescription)"
            case .noWindow: "Application has no on-screen window to capture"
            case .absent(let id): "Window \(id) is not an on-screen window of the application"
            case .noDisplay: "No display holds the application's windows"
            case .capture(let error): "ScreenCaptureKit could not capture: \(error.localizedDescription)"
            case .unrendered: "ScreenCaptureKit returned the capture without an SDR image"
            case .encode(let type): "ImageIO could not encode the capture as \(type.identifier)"
        }
    }

    public var errorUserInfo: [String: Any] {
        switch self {
            case .content(let error), .capture(let error): [NSUnderlyingErrorKey: error]
            case .noWindow, .absent, .noDisplay, .unrendered, .encode: [:]
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension NSRunningApplication {
    public func screenshot(of window: CGWindowID?, longestSide: Int?) async -> Result<(image: CGImage, windows: [CGWindowID]), ScreenshotFailure> {
        await Result { () async throws(ScreenshotFailure) in
            let content: SCShareableContent = try await Result { try await SCShareableContent.excludingDesktopWindows(true, onScreenWindowsOnly: true) }.mapError(ScreenshotFailure.content).get()
            let owned: [SCWindow] = content.windows.filter { window in window.owningApplication?.processID == processIdentifier }
            guard !owned.isEmpty else { throw .noWindow }
            let covered: (SCDisplay) -> CGFloat = { display in
                owned.map { window in display.frame.intersection(window.frame) }.reduce(0) { area, overlap in area + overlap.width * overlap.height }
            }
            let chosen: Result<(SCContentFilter, [SCWindow]), ScreenshotFailure> =
                switch window {
                    case .some(let id):
                        owned.first { window in window.windowID == id }.map { window in
                            _ = CGWindowListCopyWindowInfo(.optionIncludingWindow, window.windowID)
                            return .success((SCContentFilter(desktopIndependentWindow: window), [window]))
                        } ?? .failure(.absent(id))
                    case .none:
                        content.displays.filter { display in covered(display) > 0 }.max { left, right in covered(left) < covered(right) }.map { display in
                            let shown: [SCWindow] = owned.filter { window in display.frame.intersects(window.frame) }
                            return .success((SCContentFilter(display: display, including: shown), shown))
                        } ?? .failure(.noDisplay)
                }
            let (filter, captured): (SCContentFilter, [SCWindow]) = try chosen.get()
            let configuration: SCScreenshotConfiguration = SCScreenshotConfiguration()
            configuration.showsCursor = false
            configuration.sourceRect = captured.map(\.frame).reduce(CGRect.null) { union, frame in union.union(frame) }.intersection(filter.contentRect).offsetBy(
                dx: -filter.contentRect.minX,
                dy: -filter.contentRect.minY,
            )
            return try await (filter.screenshot(configuration, longestSide: longestSide).get(), captured.map(\.windowID))
        }
    }
}

extension SCContentFilter {
    public func screenshot(_ configuration: SCScreenshotConfiguration, longestSide: Int?) async -> Result<CGImage, ScreenshotFailure> {
        let native: CGSize = configuration.sourceRect == .null ? contentRect.size : configuration.sourceRect.size
        let scale: CGFloat = CGFloat(pointPixelScale) * (longestSide.map { limit in min(1, CGFloat(limit) / (max(native.width, native.height) * CGFloat(pointPixelScale))) } ?? 1)
        configuration.width = Int((native.width * scale).rounded())
        configuration.height = Int((native.height * scale).rounded())
        return await Result { try await SCScreenshotManager.captureScreenshot(contentFilter: self, configuration: configuration) }
            .mapError(ScreenshotFailure.capture)
            .flatMap { output in output.sdrImage.map(Result.success) ?? .failure(.unrendered) }
    }
}

extension CGImage {
    public func data(as type: UTType) -> Result<Data, ScreenshotFailure> {
        let data: CFMutableData = CFDataCreateMutable(nil, 0)
        guard let destination: CGImageDestination = CGImageDestinationCreateWithData(data, type.identifier as CFString, 1, nil) else { return .failure(.encode(type)) }
        CGImageDestinationAddImage(destination, self, nil)
        return CGImageDestinationFinalize(destination) ? .success(data as Data) : .failure(.encode(type))
    }
}
