import AppKit
import CasePathsCore
package import CoreGraphics
import CoreImage.CIFilterBuiltins
package import Foundation
import ImageIO
import Processes
import Results
import SPI
@unsafe @preconcurrency import ScreenCaptureKit
import UniformTypeIdentifiers

// --- [CONSTANTS] -----------------------------------------------------------------------

let rendering = CIContext(options: [.workingColorSpace: NSNull()])

// --- [MODELS] --------------------------------------------------------------------------

package enum Source: Decodable, Hashable, Sendable {
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

    package init(from decoder: any Decoder) throws {
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

package struct LongestSide: RawRepresentable, Decodable, Sendable {
    package let rawValue: Int

    package init?(rawValue: Int) {
        guard rawValue > 0 else { return nil }
        self.rawValue = rawValue
    }
}

package struct Mark: Decodable, Sendable {
    let frame: CGRect
    let label: String
}

package struct Screenshot: Encodable, Sendable {
    // --- [ENCODING]

    private enum CodingKeys: CodingKey {
        case png, width, height, frame, windows, backingStore, document, presentation
    }

    let image: CGImage
    let frame: CGRect
    let windows: [CGWindowID]
    let backingStore: Bool
    let document: URL?
    var presentation: Watch.Presentation?

    package func encode(to encoder: any Encoder) throws {
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
}

struct Framing {
    // --- [CONTENT]

    enum Content {
        case composite(SCContentFilter, windows: [SCWindow], displays: [SCDisplay])
        case displays([(filter: SCContentFilter, frame: CGRect)], windows: [SCWindow])
        case backingStore(InstanceState.Window)
    }

    let content: Content
    let frame: CGRect
    let subject: String

    var windows: [CGWindowID] {
        switch content {
            case .composite(_, let windows, _), .displays(_, let windows): windows.map(\.windowID)
            case .backingStore(let row): [row.number]
        }
    }

    var backingStore: Bool { if case .backingStore = content { true } else { false } }

    func fit(at scale: CGFloat, longestSide: LongestSide?) -> (width: Int, height: Int) {
        let factor: CGFloat = scale * min(1, longestSide.map { limit in CGFloat(limit.rawValue) / (max(frame.width, frame.height) * scale) } ?? 1)
        return (Int((frame.width * factor).rounded()), Int((frame.height * factor).rounded()))
    }

    // --- [CAPTURE]

    func capture(longestSide: LongestSide?) async throws(ApplicationFailure) -> CGImage {
        let subject = "capture of \(self.subject)"
        let parts: [(filter: SCContentFilter, frame: CGRect)]
        switch content {
            case .composite(let filter, let windows, let displays) where filter.style == .window && !displays.isEmpty:
                let shown: [SCDisplay] = displays.filter { (display: SCDisplay) -> Bool in display.frame.intersects(frame) }
                let regions: [(filter: SCContentFilter, frame: CGRect)] = shown.map { (display: SCDisplay) -> (filter: SCContentFilter, frame: CGRect) in
                    (SCContentFilter(display: display, including: windows), frame.intersection(display.frame))
                }
                do throws(ApplicationFailure) {
                    return try await Self(content: .composite(filter, windows: windows, displays: []), frame: frame, subject: self.subject).capture(longestSide: longestSide)
                } catch let refusal where refusal.reason.is(\.foundation) && !regions.isEmpty {
                    do throws(ApplicationFailure) {
                        return try await Self(content: .displays(regions, windows: windows), frame: frame, subject: self.subject).capture(longestSide: longestSide)
                    } catch let second where second.reason.is(\.foundation) {
                        throw ApplicationFailure(subject: subject, reason: .foundation(AggregateError(first: refusal, remaining: [second]) as NSError))
                    }
                }
            case .composite(let filter, _, _): parts = [(filter, frame)]
            case .displays(let held, _): parts = held
            case .backingStore(let row):
                let stored: CGImage? = (captureWindowList(mainConnection(), [row.number].span, [.ignoreGlobalClipShape, .bestResolution]) as? [CGImage])?.first
                let source: CIImage = stored.map(CIImage.init(cgImage:)) ?? CIImage.empty()
                let peak = CIFilter.areaMaximumAlpha()
                (peak.inputImage, peak.extent) = (source, source.extent)
                var pixel: [UInt8] = [0, 0, 0, 0]
                if let output = peak.outputImage {
                    unsafe rendering.render(output, toBitmap: &pixel, rowBytes: pixel.count, bounds: CGRect(x: 0, y: 0, width: 1, height: 1), format: .RGBA8, colorSpace: nil)
                }
                guard pixel[3] > 0 else { throw ApplicationFailure(subject: "\(row)", reason: .noBackingStore) }
                let scale: (x: CGFloat, y: CGFloat) = (source.extent.width / row.frame.width, source.extent.height / row.frame.height)
                let size: (width: Int, height: Int) = fit(at: scale.x, longestSide: longestSide)
                let area = CGRect(x: (frame.minX - row.frame.minX) * scale.x, y: (row.frame.maxY - frame.maxY) * scale.y, width: frame.width * scale.x, height: frame.height * scale.y)
                let placed: CIImage = source.transformed(
                    by: CGAffineTransform(scaleX: CGFloat(size.width) / area.width, y: CGFloat(size.height) / area.height).translatedBy(x: -area.minX, y: -area.minY),
                    highQualityDownsample: true,
                )
                guard let scaled = rendering.createCGImage(placed, from: CGRect(x: 0, y: 0, width: size.width, height: size.height), format: .RGBA8, colorSpace: stored?.colorSpace) else {
                    throw ApplicationFailure(subject: subject, reason: .noImage)
                }
                return scaled
        }
        let density: Float = parts.map(\.filter.pointPixelScale).max() ?? 1
        let size: (width: Int, height: Int) = fit(at: CGFloat(density), longestSide: longestSide)
        let (across, down): (CGFloat, CGFloat) = (CGFloat(size.width) / frame.width, CGFloat(size.height) / frame.height)
        var shots: [(image: CGImage, frame: CGRect)] = []
        for (filter, part) in parts {
            let configuration = SCScreenshotConfiguration()
            (configuration.showsCursor, configuration.width, configuration.height) = (false, Int((part.width * across).rounded()), Int((part.height * down).rounded()))
            configuration.sourceRect = part.offsetBy(dx: -filter.contentRect.minX, dy: -filter.contentRect.minY)
            let image: CGImage = try await withTimeout(
                messagingTimeout * 2,
                expired: ApplicationFailure(subject: subject, reason: .unanswered),
                cancelled: ApplicationFailure(subject: subject, reason: .cancelled),
            ) { (answer: @escaping @Sendable (Result<CGImage, ApplicationFailure>) -> Void) in
                SCScreenshotManager.captureScreenshot(contentFilter: filter, configuration: configuration) { (output: SCScreenshotOutput?, error: (any Error)?) in
                    let reason: ApplicationFailure.Reason = error.map { error in .foundation(error as NSError) } ?? .noImage
                    answer(output?.sdrImage.map(Result.success) ?? .failure(ApplicationFailure(subject: subject, reason: reason)))
                }
            }
            shots.append((image, part))
        }
        if let only = shots.first, shots.count == 1, only.frame == frame { return only.image }
        let placed: CIImage = shots.reduce(CIImage.empty()) { (joint: CIImage, shot: (image: CGImage, frame: CGRect)) -> CIImage in
            let offset = CGAffineTransform(translationX: (shot.frame.minX - frame.minX) * across, y: (frame.maxY - shot.frame.maxY) * down)
            return CIImage(cgImage: shot.image).transformed(by: offset).composited(over: joint)
        }
        guard let joined = rendering.createCGImage(placed, from: CGRect(x: 0, y: 0, width: size.width, height: size.height), format: .RGBA8, colorSpace: shots.first?.image.colorSpace) else {
            throw ApplicationFailure(subject: subject, reason: .noImage)
        }
        return joined
    }
}

extension InstanceState.Window: CustomStringConvertible {
    var description: String { title.map { title in "window \(Fact.Scalar.text(title))" } ?? "untitled window at \(SIMD2(frame.minX, frame.minY).coordinates)" }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension CGImage {
    package func png() throws(ApplicationFailure) -> Data {
        let encoded = NSMutableData()
        guard let destination = CGImageDestinationCreateWithData(encoded, UTType.png.identifier as CFString, 1, nil) else {
            throw ApplicationFailure(subject: UTType.png.identifier, reason: .noEncoder(extensions: []))
        }
        CGImageDestinationAddImage(destination, self, [kCGImageDestinationOptimizeColorForSharing: true] as CFDictionary)
        guard CGImageDestinationFinalize(destination) else { throw ApplicationFailure(subject: "\(UTType.png.identifier) encoding of a \(width)×\(height) image", reason: .noImage) }
        return encoded as Data
    }

    func draw(_ marks: [Mark], in area: CGRect) throws(ApplicationFailure) -> CGImage {
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
    package func scale(longestSide: LongestSide) throws(ApplicationFailure) -> CGImage {
        let options = [kCGImageSourceCreateThumbnailFromImageAlways: true, kCGImageSourceCreateThumbnailWithTransform: true, kCGImageSourceThumbnailMaxPixelSize: longestSide.rawValue] as CFDictionary
        guard let image = CGImageSourceCreateWithData(self as CFData, nil).flatMap({ source in CGImageSourceCreateThumbnailAtIndex(source, 0, options) }) else {
            throw ApplicationFailure(subject: "\(count) bytes", reason: .noImage)
        }
        return image
    }
}

extension Watch {
    nonisolated func frame(_ source: Source, showing menus: [Container]) async throws(ApplicationFailure) -> [Result<Framing, ApplicationFailure>] {
        let process: pid_t = instance.process.pid
        let rows: [CGWindowID: InstanceState.Window] = Dictionary(InstanceState.Window.all(.optionAll, of: process)[process, default: []].map { row in (row.number, row) }) { first, _ in first }
        let closed = ApplicationFailure(subject: "the window to capture", reason: .absent)
        let targets: [Result<(row: InstanceState.Window?, crop: CGRect?, subject: String), ApplicationFailure>] =
            switch (source, menus.isEmpty) {
                case (.windows(let numbers), _): numbers.map { number in rows[number].map { row in .success((row: row, crop: nil, subject: "\(row)")) } ?? .failure(closed) }
                case (.element(let ref), true):
                    [try await frame(of: ref)].map { (element: (window: CGWindowID?, frame: CGRect)) -> Result<(row: InstanceState.Window?, crop: CGRect?, subject: String), ApplicationFailure> in
                        let row: InstanceState.Window? = element.window.flatMap { (number: CGWindowID) -> InstanceState.Window? in rows[number] }
                        return switch (element.window, row) {
                            case (_, .some(let row)): .success((row: row, crop: element.frame, subject: "the element of \(row)"))
                            case (.none, _): .success((row: nil, crop: element.frame, subject: "the element at \(SIMD2(element.frame.minX, element.frame.minY).coordinates)"))
                            case (.some, .none): .failure(closed)
                        }
                    }
                case (.element, false):
                    menus.map { menu in
                        let named: String = menu.title.map { title in "menu \(Fact.Scalar.text(title))" } ?? "the open menu"
                        return menu.window.flatMap { window in rows[window] }.map { row in .success((row: row, crop: menu.frame, subject: named)) }
                            ?? .failure(ApplicationFailure(subject: named, reason: menu.window == nil ? .noWindow : .absent))
                    }
                case (.region(let frame), _): [.success((row: nil, crop: frame, subject: "the region at \(SIMD2(frame.minX, frame.minY).coordinates)"))]
            }
        let parents: [CGWindowID: CGWindowID] = rows.compactMapValues(\.parent)
        let listed: Result<(windows: [SCWindow], displays: [SCDisplay]), ApplicationFailure> = await Result { () async throws(ApplicationFailure) -> (windows: [SCWindow], displays: [SCDisplay]) in
            guard targets.contains(where: { target in target.map { held in held.row?.onScreen ?? true } == .success(true) }) else { return ([], []) }
            let subject = "listing of windows by ScreenCaptureKit"
            await NSApplication.loadApplication()
            let content: SCShareableContent = try await withTimeout(
                messagingTimeout * 2,
                expired: ApplicationFailure(subject: subject, reason: .unanswered),
                cancelled: ApplicationFailure(subject: subject, reason: .cancelled),
            ) { (answer: @escaping @Sendable (Result<SCShareableContent, ApplicationFailure>) -> Void) in
                SCShareableContent.getExcludingDesktopWindows(true, onScreenWindowsOnly: true) { (content: SCShareableContent?, error: (any Error)?) in
                    let reason: ApplicationFailure.Reason = error.map { error in .foundation(error as NSError) } ?? .unanswered
                    answer(content.map(Result.success) ?? .failure(ApplicationFailure(subject: subject, reason: reason)))
                }
            }
            return (content.windows.filter { window in window.owningApplication?.processID == process }, content.displays)
        }
        return targets.map { target in
            Result { () throws(ApplicationFailure) -> Framing in
                let (row, crop, subject) = try target.get()
                let bounds: CGRect = [row?.frame, crop].compactMap(\.self).reduce(CGRect.infinite) { area, bound in area.intersection(bound) }
                guard !bounds.isEmpty else { throw ApplicationFailure(subject: "capture of \(subject)", reason: .outsideWindow) }
                let shown: (windows: [SCWindow], displays: [SCDisplay]) = try row?.onScreen == false ? ([], []) : listed.get()
                let named: SCWindow? = row.flatMap { row in shown.windows.first { window in window.windowID == row.number } }
                let floor: Int = named?.windowLayer ?? .min
                let joined: [SCWindow] = shown.windows.filter { (window: SCWindow) -> Bool in
                    let descends: Bool = sequence(first: window.windowID) { (number: CGWindowID) -> CGWindowID? in parents[number] }.dropFirst().prefix(parents.count).contains { number in
                        number == row?.number
                    }
                    return descends || (window.windowLayer > floor && crop?.intersects(window.frame) == true)
                }
                let windows: [SCWindow] = row == nil ? joined : named.map { named in [named] + joined } ?? []
                let area: CGRect = crop == nil ? joined.map(\.frame).reduce(bounds) { area, next in area.union(next) } : bounds
                let displays: [SCDisplay] = shown.displays.filter { display in display.frame.intersects(area) }
                let filters: [SCContentFilter] =
                    switch (windows.first, row) {
                        case (.some(let only), _) where windows.count == 1: [SCContentFilter(desktopIndependentWindow: only)]
                        case (.some, _): displays.map { display in SCContentFilter(display: display, including: windows) }
                        case (.none, .none): displays.map { display in SCContentFilter(display: display, excludingWindows: []) }
                        case (.none, .some): []
                    }
                let framed: [(filter: SCContentFilter, frame: CGRect)] = filters.map { (filter: SCContentFilter) -> (filter: SCContentFilter, frame: CGRect) in
                    (filter, area.intersection(filter.contentRect))
                }
                let parts: [(filter: SCContentFilter, frame: CGRect)] = framed.filter { (part: (filter: SCContentFilter, frame: CGRect)) -> Bool in !part.frame.isEmpty }
                return switch (parts.first, row) {
                    case (.some(let part), _) where parts.count == 1:
                        Framing(content: .composite(part.filter, windows: windows, displays: displays), frame: part.frame, subject: subject)
                    case (.some, _):
                        Framing(
                            content: .displays(parts, windows: windows),
                            frame: parts.reduce(CGRect.null) { (joint: CGRect, part: (filter: SCContentFilter, frame: CGRect)) -> CGRect in joint.union(part.frame) },
                            subject: subject,
                        )
                    case (.none, .some(let row)) where row.shared: Framing(content: .backingStore(row), frame: bounds, subject: subject)
                    case (.none, .some(let row)): throw ApplicationFailure(subject: "\(row)", reason: .closedToCapture)
                    case (.none, .none): throw ApplicationFailure(subject: "capture of \(subject)", reason: .noDisplay)
                }
            }
        }
    }

    package nonisolated func capture(_ source: Source, longestSide: LongestSide? = nil, marks: [Mark] = []) async throws(ApplicationFailure) -> [Result<Screenshot, ApplicationFailure>] {
        try Grant.screenCapture.check()
        async let documents: [CGWindowID: URL] = self.documents()
        let (taken, presentation) = try await presenting(source.ref) { (menus: [Container]) async throws(ApplicationFailure) -> [Result<(image: CGImage, framing: Framing), ApplicationFailure>] in
            var captured: [Result<(image: CGImage, framing: Framing), ApplicationFailure>] = []
            for framing in try await frame(source, showing: menus) {
                captured.append(
                    await Result { () async throws(ApplicationFailure) -> (image: CGImage, framing: Framing) in
                        let framed: Framing = try framing.get()
                        return (try await framed.capture(longestSide: longestSide).draw(marks, in: framed.frame), framed)
                    }
                )
            }
            return captured
        }
        let shown: [CGWindowID: URL] = await documents
        return taken.map { result in
            result.map { shot in
                Screenshot(
                    image: shot.image,
                    frame: shot.framing.frame,
                    windows: shot.framing.windows,
                    backingStore: shot.framing.backingStore,
                    document: shot.framing.windows.lazy.compactMap { number in shown[number] }.first,
                    presentation: presentation,
                )
            }
        }
    }
}
