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
        let content: SCShareableContent
        do { content = try await SCShareableContent.excludingDesktopWindows(true, onScreenWindowsOnly: true) } catch {
            throw ApplicationFailure(subject: "ScreenCaptureKit shareable content of \(process)", reason: .host(error))
        }
        let owned: [SCWindow] = content.windows.filter { candidate in
            candidate.owningApplication?.processID == processIdentifier && window.map { id in id == candidate.windowID } ?? true
        }
        guard !owned.isEmpty else { throw ApplicationFailure(subject: process, reason: .absent(window.map { id in "on-screen window \(id)" } ?? "on-screen window")) }
        let displayed: [SCWindow] = owned.filter { candidate in !candidate.frame.isEmpty && content.displays.contains { display in display.frame.intersects(candidate.frame) } }
        guard !displayed.isEmpty else {
            let placed: String = owned.map { candidate in "\(candidate.windowID) at \(candidate.frame)" }.joined(separator: ", ")
            throw ApplicationFailure(subject: process, reason: .absent("display showing window \(placed)"))
        }
        let covered: (SCDisplay) -> CGFloat = { display in
            displayed.map(\.frame).map(display.frame.intersection).reduce(0) { (area: CGFloat, overlap: CGRect) in area + overlap.width * overlap.height }
        }
        guard let display: SCDisplay = content.displays.max(by: { left, right in covered(left) < covered(right) }) else {
            throw ApplicationFailure(subject: process, reason: .absent("display"))
        }
        let captured: [SCWindow] = displayed.filter { candidate in display.frame.intersects(candidate.frame) }
        let filter: SCContentFilter = SCContentFilter(display: display, including: captured)
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
        do { return try await SCScreenshotManager.captureScreenshot(contentFilter: self, configuration: configuration) } catch {
            throw ApplicationFailure(subject: "ScreenCaptureKit capture of \(contentRect)", reason: .host(error))
        }
    }
}

extension SCScreenshotConfiguration {
    public convenience init(file: URL, as type: UTType = .png) {
        self.init()
        let reference: UTTypeReference = type as UTTypeReference
        defer { extendLifetime(reference) }
        unsafe contentType = reference
        fileURL = file
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
