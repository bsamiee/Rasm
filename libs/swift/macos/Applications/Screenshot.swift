public import AppKit
import ImageIO
public import ScreenCaptureKit
import UniformTypeIdentifiers

// --- [OPERATIONS] ----------------------------------------------------------------------

extension SCScreenshotConfiguration {
    public static var background: SCScreenshotConfiguration {
        let configuration: SCScreenshotConfiguration = SCScreenshotConfiguration()
        configuration.showsCursor = false
        configuration.includeChildWindows = false
        return configuration
    }
}

extension NSRunningApplication {
    public func screenshots(
        of windows: [CGWindowID],
        longestSide: Int? = nil,
        configuration: SCScreenshotConfiguration = .background,
    ) async throws(ApplicationFailure) -> [CGWindowID: Result<SCScreenshotOutput, ApplicationFailure>] {
        guard CGPreflightScreenCaptureAccess() else { throw ApplicationFailure(subject: ProcessInfo.processInfo.processName, reason: .noCaptureAccess) }
        let content: SCShareableContent
        do { content = try await SCShareableContent.excludingDesktopWindows(true, onScreenWindowsOnly: false) } catch {
            throw ApplicationFailure(subject: String(processIdentifier), reason: .host(error as NSError))
        }
        let listed: [CGWindowID: SCWindow] = Dictionary(content.windows.map { window in (window.windowID, window) }) { first, _ in first }
        var shots: [CGWindowID: Result<SCScreenshotOutput, ApplicationFailure>] = [:]
        for number: CGWindowID in windows {
            shots[number] = await Result { () async throws(ApplicationFailure) in
                guard let window: SCWindow = listed[number], !window.frame.isEmpty else { throw ApplicationFailure(subject: String(number), reason: .noWindow) }
                let filter: SCContentFilter = SCContentFilter(desktopIndependentWindow: window)
                let scale: CGFloat = CGFloat(filter.pointPixelScale)
                let fitted: CGFloat = longestSide.map { limit in min(1, CGFloat(limit) / (max(filter.contentRect.width, filter.contentRect.height) * scale)) } ?? 1
                configuration.width = Int((filter.contentRect.width * scale * fitted).rounded())
                configuration.height = Int((filter.contentRect.height * scale * fitted).rounded())
                do { return try await SCScreenshotManager.captureScreenshot(contentFilter: filter, configuration: configuration) } catch {
                    throw ApplicationFailure(subject: String(number), reason: .host(error as NSError))
                }
            }
        }
        return shots
    }
}

extension SCScreenshotOutput {
    public func write(to file: URL) throws(ApplicationFailure) {
        let path: String = file.path(percentEncoded: false)
        guard let image: CGImage = sdrImage else { throw ApplicationFailure(subject: path, reason: .noImage) }
        guard let type: UTType = UTType(filenameExtension: file.pathExtension) else { throw ApplicationFailure(subject: path, reason: .noExtension) }
        let data: CFMutableData = CFDataCreateMutable(nil, 0)
        let unencodable: ApplicationFailure = ApplicationFailure(subject: type.identifier, reason: .noEncoder)
        guard let destination: CGImageDestination = CGImageDestinationCreateWithData(data, type.identifier as CFString, 1, nil) else { throw unencodable }
        CGImageDestinationAddImage(destination, image, nil)
        guard CGImageDestinationFinalize(destination) else { throw unencodable }
        do { try (data as Data).write(to: file) } catch { throw ApplicationFailure(subject: path, reason: .host(error as NSError)) }
    }
}
