public import AppKit
import ImageIO
public import ScreenCaptureKit
public import UniformTypeIdentifiers

// --- [OPERATIONS] ----------------------------------------------------------------------

extension NSRunningApplication {
    public func screenshot(
        of window: CGWindowID? = nil,
        longestSide: Int? = nil,
        configuration: SCScreenshotConfiguration = SCScreenshotConfiguration(),
    ) async throws(ApplicationFailure) -> (output: SCScreenshotOutput, windows: [CGWindowID]) {
        let process: String = "process \(processIdentifier)"
        let content: SCShareableContent = try await Result { try await SCShareableContent.excludingDesktopWindows(true, onScreenWindowsOnly: true) }
            .mapError { error in ApplicationFailure(subject: "ScreenCaptureKit shareable content of \(process)", reason: .host(error)) }.get()
        let owned: [SCWindow] = content.windows.filter { window in window.owningApplication?.processID == processIdentifier }
        guard !owned.isEmpty else { throw ApplicationFailure(subject: process, reason: .absent("on-screen window")) }
        let covered: (SCDisplay) -> CGFloat = { display in
            owned.map(\.frame).map(display.frame.intersection).reduce(0) { (area: CGFloat, overlap: CGRect) in area + overlap.width * overlap.height }
        }
        let chosen: Result<(SCContentFilter, [SCWindow]), ApplicationFailure> =
            switch window {
                case .some(let id):
                    owned.first { window in window.windowID == id }.map { window in .success((SCContentFilter(desktopIndependentWindow: window), [window])) }
                        ?? .failure(ApplicationFailure(subject: process, reason: .absent("on-screen window \(id)")))
                case .none:
                    content.displays.filter { display in covered(display) > 0 }.max { left, right in covered(left) < covered(right) }.map { display in
                        let shown: [SCWindow] = owned.filter { window in display.frame.intersects(window.frame) }
                        return .success((SCContentFilter(display: display, including: shown), shown))
                    } ?? .failure(ApplicationFailure(subject: process, reason: .absent("display showing its windows")))
            }
        let (filter, captured): (SCContentFilter, [SCWindow]) = try chosen.get()
        configuration.sourceRect = captured.map(\.frame).reduce(CGRect.null) { union, frame in union.union(frame) }.intersection(filter.contentRect).offsetBy(
            dx: -filter.contentRect.minX,
            dy: -filter.contentRect.minY,
        )
        return try await (filter.screenshot(configuration: configuration, longestSide: longestSide), captured.map(\.windowID))
    }
}

extension SCContentFilter {
    public func screenshot(configuration: SCScreenshotConfiguration, longestSide: Int? = nil) async throws(ApplicationFailure) -> SCScreenshotOutput {
        let native: CGSize = configuration.sourceRect == .null ? contentRect.size : configuration.sourceRect.size
        let scale: CGFloat = CGFloat(pointPixelScale) * (longestSide.map { limit in min(1, CGFloat(limit) / (max(native.width, native.height) * CGFloat(pointPixelScale))) } ?? 1)
        configuration.width = Int((native.width * scale).rounded())
        configuration.height = Int((native.height * scale).rounded())
        return try await Result { try await SCScreenshotManager.captureScreenshot(contentFilter: self, configuration: configuration) }
            .mapError { error in ApplicationFailure(subject: "ScreenCaptureKit capture of \(contentRect)", reason: .host(error)) }.get()
    }
}

extension CGImage {
    public func data(as type: UTType) throws(ApplicationFailure) -> Data {
        let data: CFMutableData = CFDataCreateMutable(nil, 0)
        let unencodable: ApplicationFailure = ApplicationFailure(subject: "ImageIO", reason: .absent("\(type.identifier) encoder"))
        guard let destination: CGImageDestination = CGImageDestinationCreateWithData(data, type.identifier as CFString, 1, nil) else { throw unencodable }
        CGImageDestinationAddImage(destination, self, nil)
        guard CGImageDestinationFinalize(destination) else { throw unencodable }
        return data as Data
    }
}
