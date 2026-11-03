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
        let hidden: Bool = isHidden
        try await setHidden(false)
        let shot: Result<(output: SCScreenshotOutput, windows: [CGWindowID]), ApplicationFailure> = await Result { () async throws(ApplicationFailure) in
            let content: SCShareableContent
            do { content = try await SCShareableContent.excludingDesktopWindows(true, onScreenWindowsOnly: true) } catch {
                throw ApplicationFailure(subject: String(processIdentifier), reason: .host(error))
            }
            let owned: [SCWindow] = content.windows.filter { candidate in
                candidate.owningApplication?.processID == processIdentifier && window.map { id in id == candidate.windowID } ?? true
            }
            guard !owned.isEmpty else { throw ApplicationFailure(subject: window.map(String.init) ?? String(processIdentifier), reason: .noWindow) }
            let displayed: [SCWindow] = owned.filter { candidate in !candidate.frame.isEmpty && content.displays.contains { display in display.frame.intersects(candidate.frame) } }
            let covered: (SCDisplay) -> CGFloat = { display in
                displayed.map(\.frame).map(display.frame.intersection).reduce(0) { (area: CGFloat, overlap: CGRect) in area + overlap.width * overlap.height }
            }
            guard let display: SCDisplay = content.displays.max(by: { left, right in covered(left) < covered(right) }), !displayed.isEmpty else {
                throw ApplicationFailure(subject: owned.map { candidate in "\(candidate.windowID) at \(candidate.frame)" }.joined(separator: ", "), reason: .noDisplay)
            }
            let captured: [SCWindow] = displayed.filter { candidate in display.frame.intersects(candidate.frame) }
            let filter: SCContentFilter = SCContentFilter(display: display, including: captured)
            configuration.sourceRect = captured.map(\.frame).reduce(CGRect.null) { union, frame in union.union(frame) }.intersection(filter.contentRect).offsetBy(
                dx: -filter.contentRect.minX,
                dy: -filter.contentRect.minY,
            )
            return try await (filter.screenshot(configuration: configuration, longestSide: longestSide), captured.map(\.windowID))
        }
        try await setHidden(hidden)
        return try shot.get()
    }
}

extension SCContentFilter {
    public func screenshot(configuration: SCScreenshotConfiguration, longestSide: Int? = nil) async throws(ApplicationFailure) -> SCScreenshotOutput {
        let native: CGSize = configuration.sourceRect == .null ? contentRect.size : configuration.sourceRect.size
        let scale: CGFloat = CGFloat(pointPixelScale) * (longestSide.map { limit in min(1, CGFloat(limit) / (max(native.width, native.height) * CGFloat(pointPixelScale))) } ?? 1)
        configuration.width = Int((native.width * scale).rounded())
        configuration.height = Int((native.height * scale).rounded())
        do { return try await SCScreenshotManager.captureScreenshot(contentFilter: self, configuration: configuration) } catch {
            throw ApplicationFailure(subject: "\(contentRect)", reason: .host(error))
        }
    }
}

extension SCScreenshotOutput {
    public func write(to file: URL) throws(ApplicationFailure) {
        guard let image: CGImage = sdrImage else { throw ApplicationFailure(subject: file.path(percentEncoded: false), reason: .noImage) }
        guard let type: UTType = UTType(filenameExtension: file.pathExtension) else { throw ApplicationFailure(subject: file.path(percentEncoded: false), reason: .noExtension) }
        let data: Data = try image.data(as: type)
        do { try data.write(to: file) } catch { throw ApplicationFailure(subject: file.path(percentEncoded: false), reason: .host(error)) }
    }
}

extension CGImage {
    public func data(as type: UTType) throws(ApplicationFailure) -> Data {
        let data: CFMutableData = CFDataCreateMutable(nil, 0)
        let unencodable: ApplicationFailure = ApplicationFailure(subject: type.identifier, reason: .noEncoder)
        guard let destination: CGImageDestination = CGImageDestinationCreateWithData(data, type.identifier as CFString, 1, nil) else { throw unencodable }
        CGImageDestinationAddImage(destination, self, nil)
        guard CGImageDestinationFinalize(destination) else { throw unencodable }
        return data as Data
    }
}
