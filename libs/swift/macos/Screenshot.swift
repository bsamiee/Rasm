public import AppKit
import ImageIO
import ScreenCaptureKit
public import UniformTypeIdentifiers

// --- [MODELS] --------------------------------------------------------------------------

public struct ShareableWindow: Codable, Hashable, Sendable {
    public let id: CGWindowID
    public let title: String?
    public let layer: Int
    public let frame: CGRect
    public let onScreen: Bool

    init(_ window: SCWindow) {
        id = window.windowID
        title = window.title
        layer = window.windowLayer
        frame = window.frame
        onScreen = window.isOnScreen
    }
}

public struct ScreenshotOptions: Sendable {
    public enum Target: Sendable {
        case window(CGWindowID?)
        case application
    }

    public let target: Target
    public let region: CGRect?
    public let maxDimension: Int?
    public let showsCursor: Bool
    public let ignoresShadows: Bool
    public let includesChildWindows: Bool
    public let hdr: Bool
    public let format: UTType

    public init(
        target: Target = .window(nil),
        region: CGRect? = nil,
        maxDimension: Int? = nil,
        showsCursor: Bool = false,
        ignoresShadows: Bool = true,
        includesChildWindows: Bool = true,
        hdr: Bool = false,
        format: UTType = .png,
    ) {
        self.target = target
        self.region = region
        self.maxDimension = maxDimension
        self.showsCursor = showsCursor
        self.ignoresShadows = ignoresShadows
        self.includesChildWindows = includesChildWindows
        self.hdr = hdr
        self.format = format
    }
}

public struct Screenshot: Encodable, Sendable {
    public let window: ShareableWindow
    public let windows: [ShareableWindow]
    public let image: Data
    public let width: Int
    public let height: Int
}

// --- [ERRORS] --------------------------------------------------------------------------

public enum ScreenshotFailure: LocalizedError {
    case content(any Error)
    case noWindow
    case absent(CGWindowID)
    case noDisplay
    case capture(any Error)
    case encode(UTType)

    public var errorDescription: String? {
        switch self {
            case .content(let error): "ScreenCaptureKit could not list shareable content: \(error.localizedDescription)"
            case .noWindow: "Application has no on-screen window to capture"
            case .absent(let id): "Window \(id) is not an on-screen window of the application"
            case .noDisplay: "No display holds the application's front window"
            case .capture(let error): "ScreenCaptureKit could not capture: \(error.localizedDescription)"
            case .encode(let type): "ImageIO could not encode the capture as \(type.identifier)"
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension NSRunningApplication {
    public func screenshot(_ options: ScreenshotOptions = ScreenshotOptions()) async -> Result<Screenshot, ScreenshotFailure> {
        await Result { try await SCShareableContent.excludingDesktopWindows(true, onScreenWindowsOnly: true) }
            .mapError(ScreenshotFailure.content)
            .flatMap { content -> Result<(filter: SCContentFilter, window: SCWindow, windows: [SCWindow]), ScreenshotFailure> in
                let owned: [SCWindow] = content.windows.filter { window in window.owningApplication?.processID == processIdentifier }
                let order: [CGWindowID]? = (CGWindowListCopyWindowInfo([.optionOnScreenOnly, .excludeDesktopElements], kCGNullWindowID) as? [[String: Any]])?
                    .compactMap { row in row[kCGWindowNumber as String] as? CGWindowID }
                let windows: [SCWindow] = order?.compactMap { id in owned.first { window in window.windowID == id } } ?? []
                return switch (options.target, windows.first) {
                    case (_, .none): .failure(.noWindow)
                    case (.window(.some(let id)), .some):
                        windows.first { window in window.windowID == id }.map { window in .success((SCContentFilter(desktopIndependentWindow: window), window, windows)) } ?? .failure(.absent(id))
                    case (.window(.none), .some(let front)): .success((SCContentFilter(desktopIndependentWindow: front), front, windows))
                    case (.application, .some(let front)):
                        content.displays.first { display in display.frame.contains(CGPoint(x: front.frame.midX, y: front.frame.midY)) }
                            .map { display in .success((SCContentFilter(display: display, including: windows), front, windows)) } ?? .failure(.noDisplay)
                }
            }
            .bind { filter, window, windows in
                let configuration: SCScreenshotConfiguration = SCScreenshotConfiguration()
                let native: CGSize = options.region?.size ?? filter.contentRect.size
                let scale: CGFloat =
                    CGFloat(filter.pointPixelScale) * (options.maxDimension.map { limit in min(1, CGFloat(limit) / (max(native.width, native.height) * CGFloat(filter.pointPixelScale))) } ?? 1)
                configuration.width = Int((native.width * scale).rounded())
                configuration.height = Int((native.height * scale).rounded())
                if let region: CGRect = options.region { configuration.sourceRect = region }
                configuration.showsCursor = options.showsCursor
                configuration.ignoreShadows = options.ignoresShadows
                configuration.includeChildWindows = options.includesChildWindows
                configuration.dynamicRange = options.hdr ? .hdr : .sdr
                return await Result { try await SCScreenshotManager.captureScreenshot(contentFilter: filter, configuration: configuration) }
                    .mapError(ScreenshotFailure.capture)
                    .flatMap { output in
                        let data: CFMutableData = CFDataCreateMutable(nil, 0)
                        let encoded: CGImage? = (options.hdr ? output.hdrImage : output.sdrImage).flatMap { image in
                            CGImageDestinationCreateWithData(data, options.format.identifier as CFString, 1, nil).flatMap { destination in
                                CGImageDestinationAddImage(destination, image, [kCGImageDestinationOptimizeColorForSharing: true] as CFDictionary)
                                return CGImageDestinationFinalize(destination) ? image : nil
                            }
                        }
                        return encoded.map { image in
                            .success(Screenshot(window: ShareableWindow(window), windows: windows.map(ShareableWindow.init), image: data as Data, width: image.width, height: image.height))
                        } ?? .failure(.encode(options.format))
                    }
            }
    }
}
