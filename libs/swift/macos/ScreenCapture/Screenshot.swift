public import AppKit
public import Applications
import CoreImage.CIFilterBuiltins
import Processes
import ScreenCaptureKit
public import UniformTypeIdentifiers

// --- [MODELS] --------------------------------------------------------------------------

public enum Source: Codable, Hashable, Sendable {
    case windows(numbers: [CGWindowID])
    case element(id: Element.ID)
}

public struct LongestSide: RawRepresentable, Decodable, Sendable {
    public let rawValue: Int

    public init?(rawValue: Int) {
        guard rawValue > 0 else { return nil }
        self.rawValue = rawValue
    }
}

public struct Screenshot: Encodable, Sendable {
    public let file: URL
    public let width: Int
    public let height: Int
    public let document: URL?
    public let preview: Data?
}

// --- [OPERATIONS] ----------------------------------------------------------------------

public func screenshotFile(in folder: URL, window: CGWindowID, _ type: UTType, at time: Date = .now) -> URL {
    folder.appending(component: "\(window)-\(time.formatted(Date.ISO8601FormatStyle(includingFractionalSeconds: true).dateSeparator(.omitted).timeSeparator(.omitted)))")
        .appendingPathExtension(for: type)
}

private func isTransparent(_ image: CGImage) -> Bool {
    let maximum = CIFilter.areaMaximumAlpha()
    (maximum.inputImage, maximum.extent) = (CIImage(cgImage: image), CGRect(x: 0, y: 0, width: image.width, height: image.height))
    var alpha: UInt8 = 0
    guard let output = maximum.outputImage else { return false }
    unsafe CIContext().render(output, toBitmap: &alpha, rowBytes: 1, bounds: CGRect(x: 0, y: 0, width: 1, height: 1), format: .A8, colorSpace: nil)
    return alpha == 0
}

extension CGImage {
    public func preview(longestSide: LongestSide, in context: CIContext) throws(ApplicationFailure) -> Data {
        let factor = min(1, CGFloat(longestSide.rawValue) / CGFloat(max(width, height)))
        guard let space = colorSpace,
            let encoded = context.pngRepresentation(
                of: CIImage(cgImage: self).transformed(by: CGAffineTransform(scaleX: factor, y: factor), highQualityDownsample: true),
                format: .RGBA8,
                colorSpace: space,
            )
        else { throw ApplicationFailure(subject: UTType.png.identifier, reason: .noEncoder) }
        return encoded
    }
}

extension Instance {
    func targets(
        _ source: Source?,
        longestSide: LongestSide?,
    ) async throws(ApplicationFailure) -> [CGWindowID: Result<(filter: SCContentFilter, region: CGRect?, width: Int, height: Int), ApplicationFailure>] {
        try Grant.screenCapture.check()
        let element: (window: CGWindowID, frame: CGRect)? = if case .element(let id) = source { try await id.frame() } else { nil }
        let windows: [CGWindowID] =
            switch (source, element) {
                case (.windows(let numbers), _): numbers
                case (_, .some(let element)): [element.window]
                case (_, .none):
                    switch await state() {
                        case .running(let running): running.windows.filter(\.presented).map(\.number)
                        case .failed(let failure): throw failure
                        case .exited: throw ApplicationFailure(subject: String(process.pid), reason: .noProcess)
                    }
            }
        await NSApplication.loadApplication()
        let content: SCShareableContent
        do { content = try await SCShareableContent.excludingDesktopWindows(true, onScreenWindowsOnly: false) } catch {
            throw ApplicationFailure(subject: String(process.pid), reason: .foundation(error as NSError))
        }
        let listed = Dictionary(content.windows.filter { window in window.owningApplication?.processID == process.pid }.map { window in (window.windowID, window) }) { first, _ in first }
        return Dictionary(
            windows.map { number in
                (
                    number,
                    Result { () throws(ApplicationFailure) in
                        guard let window = listed[number], !window.frame.isEmpty else { throw ApplicationFailure(subject: String(number), reason: .noWindow) }
                        let filter = SCContentFilter(desktopIndependentWindow: window)
                        let region = element.map { element in element.frame.offsetBy(dx: -window.frame.minX, dy: -window.frame.minY).intersection(CGRect(origin: .zero, size: window.frame.size)) }
                        guard region?.isEmpty != true else { throw ApplicationFailure(subject: String(number), reason: .noWindow) }
                        let (size, scale) = (region?.size ?? filter.contentRect.size, CGFloat(filter.pointPixelScale))
                        let factor = scale * (longestSide.map { limit in min(1, CGFloat(limit.rawValue) / (max(size.width, size.height) * scale)) } ?? 1)
                        return (filter, region, Int((size.width * factor).rounded()), Int((size.height * factor).rounded()))
                    },
                )
            }
        ) { first, _ in first }
    }

    public func capture(
        _ source: Source?,
        to folder: URL,
        longestSide: LongestSide? = nil,
        preview: LongestSide? = nil,
    ) async throws(ApplicationFailure) -> [CGWindowID: Result<Screenshot, ApplicationFailure>] {
        do { try FileManager.default.createDirectory(at: folder, withIntermediateDirectories: true) } catch {
            throw ApplicationFailure(subject: folder.path(percentEncoded: false), reason: .foundation(error as NSError))
        }
        let targets = try await targets(source, longestSide: longestSide)
        let documents: [CGWindowID: URL]
        do throws(ApplicationFailure) { documents = try await self.documents() } catch {
            guard case .unanswered = error.reason else { throw error }
            documents = [:]
        }
        var screenshots: [CGWindowID: Result<Screenshot, ApplicationFailure>] = [:]
        let context = CIContext()
        for (number, target) in targets {
            let path = screenshotFile(in: folder, window: number, .png)
            let screenshot = await Result { () async throws(ApplicationFailure) -> Screenshot? in
                let (filter, region, width, height) = try target.get()
                let configuration = SCScreenshotConfiguration()
                (configuration.showsCursor, configuration.includeChildWindows) = (false, false)
                if region != nil || longestSide != nil { (configuration.width, configuration.height) = (width, height) }
                if let region { configuration.sourceRect = region }
                let output: SCScreenshotOutput
                do { output = try await SCScreenshotManager.captureScreenshot(contentFilter: filter, configuration: configuration) } catch {
                    throw ApplicationFailure(subject: String(number), reason: .foundation(error as NSError))
                }
                guard let image = output.sdrImage else { throw ApplicationFailure(subject: String(number), reason: .noImage) }
                guard source != nil || !isTransparent(image) else { return nil }
                guard let encoded = NSBitmapImageRep(cgImage: image).representation(using: .png, properties: [:]) else { throw ApplicationFailure(subject: UTType.png.identifier, reason: .noEncoder) }
                do { try encoded.write(to: path) } catch { throw ApplicationFailure(subject: path.path(percentEncoded: false), reason: .foundation(error as NSError)) }
                return Screenshot(
                    file: path,
                    width: image.width,
                    height: image.height,
                    document: documents[number],
                    preview: try preview.map { limit throws(ApplicationFailure) in try image.preview(longestSide: limit, in: context) },
                )
            }
            switch screenshot {
                case .success(.some(let written)): screenshots[number] = .success(written)
                case .success(.none): break
                case .failure(let failure): screenshots[number] = .failure(failure)
            }
        }
        return screenshots
    }
}
