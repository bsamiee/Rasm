public import AppKit
import ImageIO
public import ScreenCaptureKit
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
    public let sourceRect: CGRect?
    public let destinationRect: CGRect?
    public let maxDimension: Int?
    public let showsCursor: Bool
    public let ignoreShadows: Bool
    public let ignoreClipping: Bool
    public let includeChildWindows: Bool
    public let displayIntent: SCScreenshotConfiguration.DisplayIntent
    public let dynamicRange: SCScreenshotConfiguration.DynamicRange
    public let contentType: UTType

    public init(
        target: Target = .window(nil),
        sourceRect: CGRect? = nil,
        destinationRect: CGRect? = nil,
        maxDimension: Int? = nil,
        showsCursor: Bool = false,
        ignoreShadows: Bool = true,
        ignoreClipping: Bool = false,
        includeChildWindows: Bool = true,
        displayIntent: SCScreenshotConfiguration.DisplayIntent = .canonical,
        dynamicRange: SCScreenshotConfiguration.DynamicRange = .sdr,
        contentType: UTType = .png,
    ) {
        self.target = target
        self.sourceRect = sourceRect
        self.destinationRect = destinationRect
        self.maxDimension = maxDimension
        self.showsCursor = showsCursor
        self.ignoreShadows = ignoreShadows
        self.ignoreClipping = ignoreClipping
        self.includeChildWindows = includeChildWindows
        self.displayIntent = displayIntent
        self.dynamicRange = dynamicRange
        self.contentType = contentType
    }
}

extension ScreenshotOptions: Decodable {
    private enum Key: CodingKey { case target, sourceRect, destinationRect, maxDimension, dynamicRange, contentType }
    private enum Range: String, Decodable { case sdr, hdr, bothSDRAndHDR }

    public init(from decoder: any Decoder) throws {
        let fields: KeyedDecodingContainer<Key> = try decoder.container(keyedBy: Key.self)
        let (range, type): (Range, String) = try (fields.decode(Range.self, forKey: .dynamicRange), fields.decode(String.self, forKey: .contentType))
        let dynamicRange: SCScreenshotConfiguration.DynamicRange =
            switch range {
                case .sdr: .sdr
                case .hdr: .hdr
                case .bothSDRAndHDR: .bothSDRAndHDR
            }
        guard let contentType: UTType = UTType(type) else { throw DecodingError.dataCorruptedError(forKey: .contentType, in: fields, debugDescription: "\(type) names no uniform type") }
        try self.init(
            target: fields.decodeIfPresent(Target.self, forKey: .target) ?? .window(nil),
            sourceRect: fields.decodeIfPresent(CGRect.self, forKey: .sourceRect),
            destinationRect: fields.decodeIfPresent(CGRect.self, forKey: .destinationRect),
            maxDimension: fields.decodeIfPresent(Int.self, forKey: .maxDimension),
            dynamicRange: dynamicRange,
            contentType: contentType,
        )
    }
}

extension ScreenshotOptions.Target: Decodable {
    private enum Scope: String, Decodable { case application }

    public init(from decoder: any Decoder) throws {
        let value: any SingleValueDecodingContainer = try decoder.singleValueContainer()
        self = try Result { try .window(value.decode(CGWindowID.self)) }.flatMapError { _ in Result { try value.decode(Scope.self) }.map { _ in .application } }.get()
    }
}

public struct Screenshot: Encodable, Sendable {
    public struct Image: Encodable, Sendable {
        public let data: Data
        public let mimeType: String
        public let width: Int
        public let height: Int
    }

    public let window: ShareableWindow
    public let windows: [ShareableWindow]
    public let sdrImage: Image?
    public let hdrImage: Image?
}

// --- [ERRORS] --------------------------------------------------------------------------

public enum ScreenshotFailure: CustomNSError, LocalizedError {
    case content(any Error)
    case windowList
    case noWindow
    case absent(CGWindowID)
    case noDisplay
    case capture(any Error)
    case unlabeled(UTType)
    case encode(UTType)

    public var errorDescription: String? {
        switch self {
            case .content(let error): "ScreenCaptureKit could not list shareable content: \(error.localizedDescription)"
            case .windowList: "The window server returned no on-screen window list"
            case .noWindow: "Application has no on-screen window to capture"
            case .absent(let id): "Window \(id) is not an on-screen window of the application"
            case .noDisplay: "No display holds the application's front window"
            case .capture(let error): "ScreenCaptureKit could not capture: \(error.localizedDescription)"
            case .unlabeled(let type): "\(type.identifier) declares no MIME type to label the image"
            case .encode(let type): "ImageIO could not encode the capture as \(type.identifier)"
        }
    }

    public var errorUserInfo: [String: Any] {
        switch self {
            case .content(let error), .capture(let error): [NSUnderlyingErrorKey: error]
            case .windowList, .noWindow, .absent, .noDisplay, .unlabeled, .encode: [:]
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension NSRunningApplication {
    public func screenshot(_ options: ScreenshotOptions = ScreenshotOptions()) async -> Result<Screenshot, ScreenshotFailure> {
        await Result { () async throws(ScreenshotFailure) in
            guard let mimeType: String = options.contentType.preferredMIMEType else { throw .unlabeled(options.contentType) }
            let content: SCShareableContent = try await Result { try await SCShareableContent.excludingDesktopWindows(true, onScreenWindowsOnly: true) }.mapError(ScreenshotFailure.content).get()
            let owned: [SCWindow] = content.windows.filter { window in window.owningApplication?.processID == processIdentifier }
            guard let listed: [[String: Any]] = CGWindowListCopyWindowInfo([.optionOnScreenOnly, .excludeDesktopElements], kCGNullWindowID) as? [[String: Any]] else { throw .windowList }
            let windows: [SCWindow] = listed.compactMap { row in (row[kCGWindowNumber as String] as? CGWindowID).flatMap { id in owned.first { window in window.windowID == id } } }
            guard let front: SCWindow = windows.first else { throw .noWindow }
            func shown(_ id: CGWindowID) throws(ScreenshotFailure) -> SCWindow {
                guard let window: SCWindow = windows.first(where: { window in window.windowID == id }) else { throw .absent(id) }
                return window
            }
            func encoded(_ image: CGImage) throws(ScreenshotFailure) -> Screenshot.Image {
                let data: CFMutableData = CFDataCreateMutable(nil, 0)
                guard let destination: CGImageDestination = CGImageDestinationCreateWithData(data, options.contentType.identifier as CFString, 1, nil) else { throw .encode(options.contentType) }
                CGImageDestinationAddImage(destination, image, nil)
                guard CGImageDestinationFinalize(destination) else { throw .encode(options.contentType) }
                return Screenshot.Image(data: data as Data, mimeType: mimeType, width: image.width, height: image.height)
            }
            let window: SCWindow
            let filter: SCContentFilter
            switch options.target {
                case .window(let id):
                    window = try id.map(shown) ?? front
                    filter = SCContentFilter(desktopIndependentWindow: window)
                case .application:
                    guard let display: SCDisplay = content.displays.first(where: { display in display.frame.contains(CGPoint(x: front.frame.midX, y: front.frame.midY)) }) else {
                        throw .noDisplay
                    }
                    window = front
                    filter = SCContentFilter(display: display, including: windows)
            }
            let configuration: SCScreenshotConfiguration = SCScreenshotConfiguration()
            let native: CGSize = options.sourceRect?.size ?? filter.contentRect.size
            let scale: CGFloat =
                CGFloat(filter.pointPixelScale) * (options.maxDimension.map { limit in min(1, CGFloat(limit) / (max(native.width, native.height) * CGFloat(filter.pointPixelScale))) } ?? 1)
            configuration.width = Int((native.width * scale).rounded())
            configuration.height = Int((native.height * scale).rounded())
            if let sourceRect: CGRect = options.sourceRect { configuration.sourceRect = sourceRect }
            if let destinationRect: CGRect = options.destinationRect { configuration.destinationRect = destinationRect }
            configuration.showsCursor = options.showsCursor
            configuration.ignoreShadows = options.ignoreShadows
            configuration.ignoreClipping = options.ignoreClipping
            configuration.includeChildWindows = options.includeChildWindows
            configuration.displayIntent = options.displayIntent
            configuration.dynamicRange = options.dynamicRange
            let output: SCScreenshotOutput = try await Result { try await SCScreenshotManager.captureScreenshot(contentFilter: filter, configuration: configuration) }
                .mapError(ScreenshotFailure.capture).get()
            return try Screenshot(window: ShareableWindow(window), windows: windows.map(ShareableWindow.init), sdrImage: output.sdrImage.map(encoded), hdrImage: output.hdrImage.map(encoded))
        }
    }
}
