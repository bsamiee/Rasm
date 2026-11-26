import AppKit
import AsyncAlgorithms
public import CoreGraphics
import CoreImage.CIFilterBuiltins
public import Foundation
import ImageIO
import Processes
import Results
import SPI
@unsafe @preconcurrency import ScreenCaptureKit
import UniformTypeIdentifiers

// --- [CONSTANTS] -----------------------------------------------------------------------

private let listing = Backoff(count: 6, base: .milliseconds(15))
let rendering = CIContext(options: [.workingColorSpace: NSNull()])

// --- [MODELS] --------------------------------------------------------------------------

public enum Source: Decodable, Hashable, Sendable {
    case windows(numbers: [CGWindowID])
    case element(ref: Int)
    case region(frame: CGRect)

    private enum CodingKeys: CodingKey {
        case windows
        case element
        case region
    }

    private enum Field: String, CodingKey {
        case tag = "_tag"
        case numbers, ref, frame
    }

    var ref: Int? { if case .element(let ref) = self { ref } else { nil } }

    public init(from decoder: any Decoder) throws {
        let fields = try decoder.container(keyedBy: Field.self)
        let tag: String = try fields.decode(String.self, forKey: .tag)
        self =
            switch CodingKeys(stringValue: tag) {
                case .windows: try .windows(numbers: fields.decode([CGWindowID].self, forKey: .numbers))
                case .element: try .element(ref: fields.decode(Int.self, forKey: .ref))
                case .region: try .region(frame: fields.decode(CGRect.self, forKey: .frame))
                case nil: throw DecodingError.dataCorruptedError(forKey: .tag, in: fields, debugDescription: "\(tag) names no \(Self.self)")
            }
    }
}

public struct LongestSide: RawRepresentable, Decodable, Sendable {
    public let rawValue: Int

    public init?(rawValue: Int) {
        guard rawValue > 0 else { return nil }
        self.rawValue = rawValue
    }
}

public struct Mark: Decodable, Sendable {
    public let frame: CGRect
    public let label: String
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
    public let presentation: Watch.Presentation?

    // --- [ENCODING]

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

    // --- [DIFFERENCE]

    public func changed(from earlier: Self) -> CGRect? {
        guard frame == earlier.frame, (image.width, image.height) == (earlier.image.width, earlier.image.height) else { return frame.union(earlier.frame) }
        let extent = CGRect(x: 0, y: 0, width: image.width, height: image.height)
        let difference = CIFilter.differenceBlendMode()
        (difference.inputImage, difference.backgroundImage) = (CIImage(cgImage: image).settingAlphaOne(in: extent), CIImage(cgImage: earlier.image).settingAlphaOne(in: extent))
        let summed = CIFilter.colorMatrix()
        (summed.inputImage, summed.rVector) = (difference.outputImage, CIVector(x: 1, y: 1, z: 1, w: 0))
        let bounds = CIFilter.areaBoundsRed()
        (bounds.inputImage, bounds.extent) = (summed.outputImage, extent)
        var box: [Float] = [0, 0, 0, 0]
        if let output = bounds.outputImage {
            unsafe rendering.render(output, toBitmap: &box, rowBytes: MemoryLayout<Float>.stride * box.count, bounds: CGRect(x: 0, y: 0, width: 1, height: 1), format: .RGBAf, colorSpace: nil)
        }
        let area = CGRect(x: CGFloat(box[0]), y: 1 - CGFloat(box[1] + box[3]), width: CGFloat(box[2]), height: CGFloat(box[3]))
        return area.isEmpty ? nil : area.applying(CGAffineTransform(a: frame.width, b: 0, c: 0, d: frame.height, tx: frame.minX, ty: frame.minY))
    }
}

struct Framing {
    let filter: SCContentFilter
    let width: Int
    let height: Int
    let frame: CGRect
    let windows: [SCWindow]
    let display: SCDisplay?
    let subject: String

    var backingStore: Bool { windows.count == 1 && windows.allSatisfy { window in !window.isOnScreen } }

    // --- [CAPTURE]

    func capture() async throws(ApplicationFailure) -> CGImage {
        let configuration = SCScreenshotConfiguration()
        (configuration.showsCursor, configuration.width, configuration.height) = (false, width, height)
        configuration.sourceRect = frame.offsetBy(dx: -filter.contentRect.minX, dy: -filter.contentRect.minY)
        let (filter, stored, size, subject): (SCContentFilter, SCWindow?, CGSize, String) = (self.filter, backingStore ? windows.first : nil, CGSize(width: width, height: height), self.subject)
        let blank = ApplicationFailure(subject: subject, reason: .noImage)
        return try await withTimeout(
            messagingTimeout * 2,
            expired: ApplicationFailure(subject: subject, reason: .unanswered(nil)),
            cancelled: ApplicationFailure(subject: subject, reason: .cancelled),
        ) { (answer: @escaping @Sendable (Result<CGImage, ApplicationFailure>) -> Void) in
            guard let stored else {
                return SCScreenshotManager.captureScreenshot(contentFilter: filter, configuration: configuration) { (output: SCScreenshotOutput?, error: (any Error)?) in
                    answer(output?.sdrImage.map(Result.success) ?? .failure(error.map { error in ApplicationFailure(subject: subject, reason: .foundation(error as NSError)) } ?? blank))
                }
            }
            let backing: CGImage? = (unsafe captureWindowList(mainConnection(), [stored.windowID], 1, [.ignoreGlobalClipShape, .bestResolution]) as? [CGImage])?.first
            let cropped: CGImage? = backing.flatMap { backing in
                backing.cropping(to: configuration.sourceRect.applying(CGAffineTransform(scaleX: CGFloat(backing.width) / stored.frame.width, y: CGFloat(backing.height) / stored.frame.height)))
            }
            let scaled: CGImage? = cropped.flatMap { cropped in
                rendering.createCGImage(
                    CIImage(cgImage: cropped).transformed(by: CGAffineTransform(scaleX: size.width / CGFloat(cropped.width), y: size.height / CGFloat(cropped.height)), highQualityDownsample: true),
                    from: CGRect(origin: .zero, size: size),
                    format: .RGBA8,
                    colorSpace: cropped.colorSpace,
                )
            }
            answer(scaled.map(Result.success) ?? .failure(blank))
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension CGImage {
    public func png() throws(ApplicationFailure) -> Data {
        let encoded = NSMutableData()
        guard let destination = CGImageDestinationCreateWithData(encoded, UTType.png.identifier as CFString, 1, nil) else {
            throw ApplicationFailure(subject: UTType.png.identifier, reason: .noEncoder)
        }
        CGImageDestinationAddImage(destination, self, [kCGImageDestinationOptimizeColorForSharing: true] as CFDictionary)
        guard CGImageDestinationFinalize(destination) else { throw ApplicationFailure(subject: "\(UTType.png.identifier) encoding of a \(width)×\(height) image", reason: .noImage) }
        return encoded as Data
    }

    public func marking(_ marks: [Mark], in area: CGRect) throws(ApplicationFailure) -> CGImage {
        let meeting: [Mark] = marks.filter { mark in mark.frame.intersects(area) }
        guard !meeting.isEmpty else { return self }
        let subject = "\(meeting.count) marks on a \(width)×\(height) image"
        guard let space = colorSpace,
            let context = unsafe CGContext(
                data: nil,
                width: width,
                height: height,
                bitsPerComponent: 8,
                bytesPerRow: 0,
                space: space,
                bitmapInfo: CGBitmapInfo(rawValue: CGImageAlphaInfo.premultipliedLast.rawValue),
            )
        else {
            throw ApplicationFailure(subject: subject, reason: .noImage)
        }
        let scale: (x: CGFloat, y: CGFloat) = (CGFloat(width) / area.width, CGFloat(height) / area.height)
        let pixel: CGFloat = min(scale.x, scale.y)
        let font = NSFont.monospacedDigitSystemFont(ofSize: NSFont.smallSystemFontSize * max(1, pixel) / pixel, weight: .semibold)
        let frames: [CGRect] = meeting.map(\.frame)
        context.draw(self, in: CGRect(x: 0, y: 0, width: width, height: height))
        context.translateBy(x: 0, y: CGFloat(height))
        context.scaleBy(x: scale.x, y: -scale.y)
        context.translateBy(x: -area.minX, y: -area.minY)
        context.textMatrix = CGAffineTransform(scaleX: 1, y: -1)
        context.setLineWidth(max(1, pixel) / pixel)
        context.setStrokeColor(NSColor.systemRed.cgColor)
        context.addRects(frames)
        context.strokePath()
        var placed: [CGRect] = []
        for mark in meeting {
            let line = CTLineCreateWithAttributedString(
                NSAttributedString(string: mark.label, attributes: [.font: font, NSAttributedString.Key(kCTForegroundColorFromContextAttributeName as String): true])
            )
            var (ascent, descent): (CGFloat, CGFloat) = (0, 0)
            let advance = CGFloat(unsafe CTLineGetTypographicBounds(line, &ascent, &descent, nil))
            let size = CGSize(width: advance + ascent / 2, height: ascent + descent)
            let beside: [CGRect] = [
                CGRect(origin: CGPoint(x: mark.frame.minX, y: mark.frame.minY - size.height), size: size),
                CGRect(origin: CGPoint(x: mark.frame.minX, y: mark.frame.maxY), size: size),
                CGRect(origin: CGPoint(x: mark.frame.maxX, y: mark.frame.minY), size: size),
                CGRect(origin: CGPoint(x: mark.frame.minX - size.width, y: mark.frame.minY), size: size),
            ]
            let corner = CGPoint(x: min(max(mark.frame.minX, area.minX), area.maxX - size.width), y: min(max(mark.frame.minY, area.minY), area.maxY - size.height))
            let label: CGRect =
                beside.first { (spot: CGRect) -> Bool in
                    area.contains(spot) && placed.allSatisfy { (held: CGRect) -> Bool in !spot.intersects(held) }
                        && frames.allSatisfy { (other: CGRect) -> Bool in other.contains(mark.frame) || !spot.intersects(other) }
                } ?? CGRect(origin: corner, size: size)
            placed.append(label)
            context.setFillColor(NSColor.systemRed.cgColor)
            context.fill(label)
            context.setFillColor(NSColor.white.cgColor)
            context.textPosition = CGPoint(x: label.minX + ascent / 4, y: label.minY + ascent)
            CTLineDraw(line, context)
        }
        guard let marked = context.makeImage() else { throw ApplicationFailure(subject: subject, reason: .noImage) }
        return marked
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

extension Watch {
    private nonisolated func shareable(listing named: Set<CGWindowID>, after delays: ArraySlice<Duration>) async throws(ApplicationFailure) -> (windows: [SCWindow], displays: [SCDisplay]) {
        let process: pid_t = instance.process.pid
        let subject = "shareable content for process \(process)"
        let content: SCShareableContent = try await withTimeout(
            messagingTimeout * 2,
            expired: ApplicationFailure(subject: subject, reason: .unanswered(nil)),
            cancelled: ApplicationFailure(subject: subject, reason: .cancelled),
        ) { (answer: @escaping @Sendable (Result<SCShareableContent, ApplicationFailure>) -> Void) in
            SCShareableContent.getExcludingDesktopWindows(true, onScreenWindowsOnly: false) { (content: SCShareableContent?, error: (any Error)?) in
                let reason: ApplicationFailure.Reason = if let error { .foundation(error as NSError) } else { .unanswered(nil) }
                answer(content.map(Result.success) ?? .failure(ApplicationFailure(subject: subject, reason: reason)))
            }
        }
        let owned = content.windows.filter { window in window.owningApplication?.processID == process }
        guard let delay = delays.first, !named.isSubset(of: owned.map(\.windowID)) else { return (owned, content.displays) }
        do { try await Task.sleep(for: delay) } catch { throw ApplicationFailure(subject: subject, reason: .cancelled) }
        return try await shareable(listing: named, after: delays.dropFirst())
    }

    nonisolated func framings(_ source: Source, showing menus: [Container], longestSide: LongestSide?) async throws(ApplicationFailure) -> [Result<Framing, ApplicationFailure>] {
        let process: pid_t = instance.process.pid
        let targets: [(named: [CGWindowID], crop: CGRect?, subject: String)] =
            switch (source, menus.isEmpty) {
                case (.windows(let numbers), _): numbers.map { number in ([number], nil, "capture of window \(number)") }
                case (.element(let ref), true): [try await frame(of: ref)].map { element in ([element.window], element.frame, "capture of element \(ref)") }
                case (.element, false): menus.compactMap { menu in menu.window.flatMap { window in menu.frame.map { frame in ([window], frame, "capture of menu \(menu.ref)") } } }
                case (.region(let frame), _): [([], frame, "capture of region \(frame)")]
            }
        var generator = SystemRandomNumberGenerator()
        await NSApplication.loadApplication()
        let (owned, displays) = try await shareable(listing: Set(targets.flatMap(\.named)), after: listing.delays(using: &generator)[...])
        let rows: [InstanceState.Window] = InstanceState.Window.all(.optionAll, of: process)[process, default: []]
        let parents: [CGWindowID: CGWindowID] = Dictionary(rows.compactMap { row in row.parent.map { parent in (row.number, parent) } }) { first, _ in first }
        let withheld: Set<CGWindowID> = Set(rows.filter { row in !row.shared }.map(\.number))
        return targets.map { target in
            Result { () throws(ApplicationFailure) -> Framing in
                guard withheld.isDisjoint(with: target.named) else { throw ApplicationFailure(subject: target.subject, reason: .unshared) }
                let named: [SCWindow] = owned.filter { window in target.named.contains(window.windowID) }
                let floor: Int = named.map(\.windowLayer).min() ?? .min
                let joined: [SCWindow] = owned.filter { window in
                    let descends: Bool = sequence(first: window.windowID) { number in parents[number] }.dropFirst().prefix(parents.count).contains(where: target.named.contains)
                    let above: Bool = window.windowLayer > floor && target.crop?.intersects(window.frame) == true
                    return window.isOnScreen && (descends || above)
                }
                let windows: [SCWindow] = named + joined
                guard named.count == target.named.count, let first = windows.first else { throw ApplicationFailure(subject: target.subject, reason: .noWindow) }
                let frame: CGRect = target.crop ?? windows.map(\.frame).reduce(first.frame) { joint, next in joint.union(next) }
                let display: SCDisplay? = displays.first { display in display.frame.contains(CGPoint(x: frame.midX, y: frame.midY)) }
                let filter: SCContentFilter =
                    switch (windows.count, display) {
                        case (1, _): SCContentFilter(desktopIndependentWindow: first)
                        case (_, .some(let display)): SCContentFilter(display: display, including: windows)
                        case (_, .none): throw ApplicationFailure(subject: target.subject, reason: .noWindow)
                    }
                let shown: CGRect = frame.intersection(filter.contentRect)
                guard !shown.isEmpty else { throw ApplicationFailure(subject: target.subject, reason: .noWindow) }
                let scale = CGFloat(filter.pointPixelScale)
                let factor: CGFloat = scale * min(1, longestSide.map { limit in CGFloat(limit.rawValue) / (max(shown.width, shown.height) * scale) } ?? 1)
                return Framing(
                    filter: filter,
                    width: Int((shown.width * factor).rounded()),
                    height: Int((shown.height * factor).rounded()),
                    frame: shown,
                    windows: windows,
                    display: display,
                    subject: target.subject,
                )
            }
        }
    }

    public nonisolated func capture(_ source: Source, longestSide: LongestSide? = nil, marks: [Mark] = []) async throws(ApplicationFailure) -> [Result<Screenshot, ApplicationFailure>] {
        try Grant.screenCapture.check()
        async let documents: [CGWindowID: URL] = self.documents()
        let (taken, presentation) = try await presenting(source.ref) { (menus: [Container]) async throws(ApplicationFailure) -> [Result<(image: CGImage, framing: Framing), ApplicationFailure>] in
            var captured: [Result<(image: CGImage, framing: Framing), ApplicationFailure>] = []
            for framing in try await framings(source, showing: menus, longestSide: longestSide) {
                captured.append(
                    await Result { () async throws(ApplicationFailure) -> (image: CGImage, framing: Framing) in
                        let framed: Framing = try framing.get()
                        return (try await framed.capture().marking(marks, in: framed.frame), framed)
                    }
                )
            }
            return captured
        }
        let shown: [CGWindowID: URL] = await documents
        return taken.map { result in
            result.map { shot in
                let numbers: [CGWindowID] = shot.framing.windows.map(\.windowID)
                return Screenshot(
                    image: shot.image,
                    frame: shot.framing.frame,
                    windows: numbers,
                    backingStore: shot.framing.backingStore,
                    document: numbers.lazy.compactMap { number in shown[number] }.first,
                    presentation: presentation,
                )
            }
        }
    }
}
