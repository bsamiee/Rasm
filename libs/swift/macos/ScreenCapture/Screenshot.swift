public import AppKit
public import Applications
import CoreImage.CIFilterBuiltins
import ImageIO
import Processes
import Results
import ScreenCaptureKit
import UniformTypeIdentifiers

// --- [TYPES] ---------------------------------------------------------------------------

typealias Framing = (filter: SCContentFilter, area: CGRect, width: Int, height: Int, frame: CGRect, windows: [SCWindow], display: SCDisplay?)

// --- [CONSTANTS] -----------------------------------------------------------------------

private let listing = Backoff(count: 6, base: .milliseconds(15))
private let rendering = CIContext(options: [.workingColorSpace: NSNull()])

// --- [MODELS] --------------------------------------------------------------------------

public enum Source: Codable, Hashable, Sendable {
    case windows(numbers: [CGWindowID])
    case element(ref: Int)
    case region(frame: CGRect)

    var ref: Int? { if case .element(let ref) = self { ref } else { nil } }
}

public struct LongestSide: RawRepresentable, Decodable, Sendable {
    public let rawValue: Int

    public init?(rawValue: Int) {
        guard rawValue > 0 else { return nil }
        self.rawValue = rawValue
    }
}

public struct Screenshot: Encodable, Sendable {
    private enum CodingKeys: CodingKey {
        case png, width, height, frame, windows, backingStore, document, presentation
    }

    public let image: CGImage
    public let frame: CGRect
    public let windows: [CGWindowID]
    public let backingStore: Bool
    public let document: URL?
    public var presentation: Watch.Presentation?

    public func encode(to encoder: any Encoder) throws {
        var fields = encoder.container(keyedBy: CodingKeys.self)
        try fields.encode(image.png(), forKey: .png)
        try fields.encode(image.width, forKey: .width)
        try fields.encode(image.height, forKey: .height)
        try fields.encode(frame, forKey: .frame)
        try fields.encode(windows, forKey: .windows)
        try fields.encode(backingStore, forKey: .backingStore)
        try fields.encodeIfPresent(document, forKey: .document)
        try fields.encodeIfPresent(presentation, forKey: .presentation)
    }

    public func changed(from earlier: Self) -> CGRect? {
        guard frame == earlier.frame, (image.width, image.height) == (earlier.image.width, earlier.image.height) else { return frame.union(earlier.frame) }
        let difference = CIFilter.differenceBlendMode()
        (difference.inputImage, difference.backgroundImage) = (CIImage(cgImage: image), CIImage(cgImage: earlier.image))
        let summed = CIFilter.colorMatrix()
        (summed.inputImage, summed.rVector) = (difference.outputImage, CIVector(x: 1, y: 1, z: 1, w: 0))
        let bounds = CIFilter.areaBoundsRed()
        (bounds.inputImage, bounds.extent) = (summed.outputImage, CGRect(x: 0, y: 0, width: image.width, height: image.height))
        var box: [Float] = [0, 0, 0, 0]
        if let output = bounds.outputImage {
            unsafe rendering.render(output, toBitmap: &box, rowBytes: MemoryLayout<Float>.stride * box.count, bounds: CGRect(x: 0, y: 0, width: 1, height: 1), format: .RGBAf, colorSpace: nil)
        }
        let area = CGRect(x: CGFloat(box[0]), y: 1 - CGFloat(box[1] + box[3]), width: CGFloat(box[2]), height: CGFloat(box[3]))
        return area.isEmpty ? nil : area.applying(CGAffineTransform(a: frame.width, b: 0, c: 0, d: frame.height, tx: frame.minX, ty: frame.minY))
    }
}

private struct WindowCaptureOptions: OptionSet {
    let rawValue: UInt32

    static let bestResolution = Self(rawValue: 1 << 8)
    static let ignoreGlobalClipShape = Self(rawValue: 1 << 11)
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension CGImage {
    public func png() throws(ApplicationFailure) -> Data {
        guard let encoded = NSBitmapImageRep(cgImage: self).representation(using: .png, properties: [:]) else { throw ApplicationFailure(subject: UTType.png.identifier, reason: .noEncoder) }
        return encoded
    }
}

extension Data {
    public func image(longestSide: LongestSide) throws(ApplicationFailure) -> CGImage {
        let options = [kCGImageSourceCreateThumbnailFromImageAlways: true, kCGImageSourceCreateThumbnailWithTransform: true, kCGImageSourceThumbnailMaxPixelSize: longestSide.rawValue] as CFDictionary
        guard let image = CGImageSourceCreateWithData(self as CFData, nil).flatMap({ source in CGImageSourceCreateThumbnailAtIndex(source, 0, options) }) else {
            throw ApplicationFailure(subject: "\(count) bytes", reason: .noImage)
        }
        return image
    }
}

extension Instance {
    private func shareable(listing named: Set<CGWindowID>, after delays: ArraySlice<Duration>) async throws(ApplicationFailure) -> (windows: [SCWindow], displays: [SCDisplay]) {
        let content: SCShareableContent
        do { content = try await SCShareableContent.excludingDesktopWindows(true, onScreenWindowsOnly: false) } catch {
            throw ApplicationFailure(subject: String(process.pid), reason: .foundation(error as NSError))
        }
        let owned = content.windows.filter { window in window.owningApplication?.processID == process.pid }
        guard let delay = delays.first, !named.isSubset(of: owned.map(\.windowID)) else { return (owned, content.displays) }
        do { try await Task.sleep(for: delay) } catch { throw ApplicationFailure(subject: String(process.pid), reason: .cancelled) }
        return try await shareable(listing: named, after: delays.dropFirst())
    }

    func framings(_ source: Source, showing menus: [Container], in watch: Watch, longestSide: LongestSide?) async throws(ApplicationFailure) -> [Result<Framing, ApplicationFailure>] {
        let levels: [(named: [CGWindowID], crop: CGRect?)] = menus.compactMap { (menu: Container) -> (named: [CGWindowID], crop: CGRect?)? in
            guard let window = menu.window, let frame = menu.frame else { return nil }
            return ([window], frame)
        }
        let targets: [(named: [CGWindowID], crop: CGRect?)] =
            switch (source, levels.isEmpty) {
                case (.windows(let numbers), _): numbers.map { (number: CGWindowID) -> (named: [CGWindowID], crop: CGRect?) in ([number], nil) }
                case (.element(let ref), true):
                    try await [watch.frame(of: ref)].map { (element: (window: CGWindowID, frame: CGRect)) -> (named: [CGWindowID], crop: CGRect?) in ([element.window], element.frame) }
                case (.element, false): levels
                case (.region(let frame), _): [([], frame)]
            }
        var generator = SystemRandomNumberGenerator()
        await NSApplication.loadApplication()
        let (owned, displays) = try await shareable(listing: Set(targets.flatMap(\.named)), after: listing.delays(using: &generator)[...])
        return targets.map { (target: (named: [CGWindowID], crop: CGRect?)) -> Result<Framing, ApplicationFailure> in
            Result { () throws(ApplicationFailure) -> Framing in
                let subject: String =
                    target.crop.map { (crop: CGRect) -> String in String(describing: crop) } ?? target.named.map { (number: CGWindowID) -> String in String(number) }.joined(separator: ",")
                let named: [SCWindow] = owned.filter { (window: SCWindow) -> Bool in target.named.contains(window.windowID) }
                let floor: Int = named.map(\.windowLayer).min() ?? .min
                let above: [SCWindow] = owned.filter { (window: SCWindow) -> Bool in window.windowLayer > floor && target.crop.map { (crop: CGRect) -> Bool in window.frame.intersects(crop) } == true }
                let windows: [SCWindow] = named + above
                guard named.count == target.named.count, let first = windows.first else { throw ApplicationFailure(subject: subject, reason: .noWindow) }
                let frame: CGRect = target.crop ?? first.frame
                let display: SCDisplay? = displays.first { (display: SCDisplay) -> Bool in display.frame.contains(CGPoint(x: frame.midX, y: frame.midY)) }
                let (filter, bounds): (SCContentFilter, CGRect) =
                    switch (windows.count, display) {
                        case (1, _): (SCContentFilter(desktopIndependentWindow: first), first.frame)
                        case (_, .some(let display)): (SCContentFilter(display: display, including: windows), display.frame)
                        case (_, .none): throw ApplicationFailure(subject: subject, reason: .noWindow)
                    }
                let area: CGRect = frame.offsetBy(dx: -bounds.minX, dy: -bounds.minY).intersection(CGRect(origin: .zero, size: bounds.size))
                guard !area.isEmpty else { throw ApplicationFailure(subject: subject, reason: .noWindow) }
                let scale = CGFloat(filter.pointPixelScale)
                let factor: CGFloat = scale * (longestSide.map { (limit: LongestSide) -> CGFloat in min(1, CGFloat(limit.rawValue) / (max(area.width, area.height) * scale)) } ?? 1)
                let (width, height): (Int, Int) = (Int((area.width * factor).rounded()), Int((area.height * factor).rounded()))
                return (filter, area, width, height, area.offsetBy(dx: bounds.minX, dy: bounds.minY), windows, display)
            }
        }
    }

    public func capture(_ source: Source, in watch: Watch, longestSide: LongestSide? = nil) async throws(ApplicationFailure) -> [Result<Screenshot, ApplicationFailure>] {
        try Grant.screenCapture.check()
        let documents: [CGWindowID: URL] = await watch.documents()
        let (shots, presentation) = try await watch.presenting(source.ref) { (menus: [Container]) async throws(ApplicationFailure) -> [Result<Screenshot, ApplicationFailure>] in
            var shots: [Result<Screenshot, ApplicationFailure>] = []
            for framing in try await framings(source, showing: menus, in: watch, longestSide: longestSide) {
                shots.append(
                    await Result { () async throws(ApplicationFailure) -> Screenshot in
                        let (filter, area, width, height, frame, windows, _) = try framing.get()
                        let numbers: [CGWindowID] = windows.map(\.windowID)
                        let subject: String = numbers.map { (number: CGWindowID) -> String in String(number) }.joined(separator: ",")
                        let stored: SCWindow? = windows.count == 1 ? windows.first { (window: SCWindow) -> Bool in !window.isOnScreen } : nil
                        let image: CGImage?
                        if let window = stored {
                            image = (unsafe captureWindowList(mainConnection(), [window.windowID], 1, [.ignoreGlobalClipShape, .bestResolution]) as? [CGImage])?.first
                                .flatMap { (backing: CGImage) -> CGImage? in
                                    backing.cropping(to: area.applying(CGAffineTransform(scaleX: CGFloat(backing.width) / window.frame.width, y: CGFloat(backing.height) / window.frame.height)))
                                }
                                .flatMap { (cropped: CGImage) -> CGImage? in
                                    rendering.createCGImage(
                                        CIImage(cgImage: cropped).transformed(by: CGAffineTransform(scaleX: CGFloat(width) / CGFloat(cropped.width), y: CGFloat(height) / CGFloat(cropped.height)), highQualityDownsample: true),
                                        from: CGRect(x: 0, y: 0, width: width, height: height),
                                        format: .RGBA8,
                                        colorSpace: cropped.colorSpace,
                                    )
                                }
                        } else {
                            let configuration = SCScreenshotConfiguration()
                            (configuration.showsCursor, configuration.ignoreShadows, configuration.sourceRect, configuration.width, configuration.height) = (false, true, area, width, height)
                            do { image = try await SCScreenshotManager.captureScreenshot(contentFilter: filter, configuration: configuration).sdrImage } catch {
                                throw ApplicationFailure(subject: subject, reason: .foundation(error as NSError))
                            }
                        }
                        guard let image else { throw ApplicationFailure(subject: subject, reason: .noImage) }
                        let document: URL? = numbers.lazy.compactMap { (number: CGWindowID) -> URL? in documents[number] }.first
                        return Screenshot(image: image, frame: frame, windows: numbers, backingStore: stored != nil, document: document, presentation: nil)
                    }
                )
            }
            return shots
        }
        return shots.map { (shot: Result<Screenshot, ApplicationFailure>) -> Result<Screenshot, ApplicationFailure> in
            shot.map { (taken: Screenshot) -> Screenshot in
                var presented: Screenshot = taken
                presented.presentation = presentation
                return presented
            }
        }
    }
}

@_silgen_name("CGSHWCaptureWindowList")
private func captureWindowList(_: Int32, _: UnsafePointer<UInt32>, _: Int32, _: WindowCaptureOptions) -> CFArray?
