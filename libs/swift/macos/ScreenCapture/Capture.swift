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

public struct Shot: Encodable, Sendable {
    public let file: URL
    public let width: Int
    public let height: Int
    public let document: URL?
    public let preview: Data?
}

// --- [OPERATIONS] ----------------------------------------------------------------------

public func footageFile(in folder: URL, window: CGWindowID, _ type: UTType, at time: Date = .now) -> URL {
    folder.appending(component: "\(window)-\(time.formatted(Date.ISO8601FormatStyle(includingFractionalSeconds: true).dateSeparator(.omitted).timeSeparator(.omitted)))")
        .appendingPathExtension(for: type)
}

public func checkCaptureAccess(prompting: Bool = false) throws(ApplicationFailure) {
    guard prompting ? CGRequestScreenCaptureAccess() : CGPreflightScreenCaptureAccess() else {
        throw ApplicationFailure(subject: ProcessInfo.processInfo.processName, reason: .noCaptureAccess)
    }
}

private func transparent(_ image: CGImage) -> Bool {
    let maximum = CIFilter.areaMaximumAlpha()
    (maximum.inputImage, maximum.extent) = (CIImage(cgImage: image), CGRect(x: 0, y: 0, width: image.width, height: image.height))
    var alpha: UInt8 = 0
    guard let output = maximum.outputImage else { return false }
    unsafe CIContext().render(output, toBitmap: &alpha, rowBytes: 1, bounds: CGRect(x: 0, y: 0, width: 1, height: 1), format: .A8, colorSpace: nil)
    return alpha == 0
}

extension Instance {
    func targets(
        _ source: Source?,
        longestSide: Int?,
    ) async throws(ApplicationFailure) -> [CGWindowID: Result<(filter: SCContentFilter, region: CGRect?, width: Int, height: Int), ApplicationFailure>] {
        try checkCaptureAccess()
        let element: (window: CGWindowID, frame: CGRect)? = if case .element(let id) = source { try await id.framed() } else { nil }
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
            throw ApplicationFailure(subject: String(process.pid), reason: .host(error as NSError))
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
                        let factor = scale * (longestSide.map { limit in min(1, CGFloat(limit) / (max(size.width, size.height) * scale)) } ?? 1)
                        return (filter, region, Int((size.width * factor).rounded()), Int((size.height * factor).rounded()))
                    },
                )
            }
        ) { first, _ in first }
    }

    public func capture(
        _ source: Source?,
        to folder: URL,
        longestSide: Int? = nil,
        preview: Int? = nil,
    ) async throws(ApplicationFailure) -> [CGWindowID: Result<Shot, ApplicationFailure>] {
        do { try FileManager.default.createDirectory(at: folder, withIntermediateDirectories: true) } catch {
            throw ApplicationFailure(subject: folder.path(percentEncoded: false), reason: .host(error as NSError))
        }
        let (targets, documents) = (try await targets(source, longestSide: longestSide), try await documents())
        var shots: [CGWindowID: Result<Shot, ApplicationFailure>] = [:]
        for (number, target) in targets {
            let path = footageFile(in: folder, window: number, .png)
            let shot = await Result { () async throws(ApplicationFailure) -> Shot? in
                let (filter, region, width, height) = try target.get()
                let configuration = SCScreenshotConfiguration()
                (configuration.showsCursor, configuration.includeChildWindows) = (false, region == nil)
                if region != nil || longestSide != nil { (configuration.width, configuration.height) = (width, height) }
                if let region { configuration.sourceRect = region }
                let output: SCScreenshotOutput
                do { output = try await SCScreenshotManager.captureScreenshot(contentFilter: filter, configuration: configuration) } catch {
                    throw ApplicationFailure(subject: String(number), reason: .host(error as NSError))
                }
                guard let image = output.sdrImage else { throw ApplicationFailure(subject: String(number), reason: .noImage) }
                guard source != nil || !transparent(image) else { return nil }
                guard let encoded = NSBitmapImageRep(cgImage: image).representation(using: .png, properties: [:]) else { throw ApplicationFailure(subject: UTType.png.identifier, reason: .noEncoder) }
                do { try encoded.write(to: path) } catch { throw ApplicationFailure(subject: path.path(percentEncoded: false), reason: .host(error as NSError)) }
                let factor = preview.map { limit in min(1, CGFloat(limit) / CGFloat(max(image.width, image.height))) }
                let scaled: Data? =
                    switch (factor, image.colorSpace) {
                        case (.some(let factor), .some(let space)):
                            CIContext().pngRepresentation(
                                of: CIImage(cgImage: image).transformed(by: CGAffineTransform(scaleX: factor, y: factor), highQualityDownsample: true),
                                format: .RGBA8,
                                colorSpace: space,
                            )
                        case (.none, _), (_, .none): nil
                    }
                guard preview == nil || scaled != nil else { throw ApplicationFailure(subject: path.path(percentEncoded: false), reason: .noImage) }
                return Shot(file: path, width: image.width, height: image.height, document: documents[number], preview: scaled)
            }
            switch shot {
                case .success(.some(let written)): shots[number] = .success(written)
                case .success(.none): break
                case .failure(let failure): shots[number] = .failure(failure)
            }
        }
        return shots
    }
}
