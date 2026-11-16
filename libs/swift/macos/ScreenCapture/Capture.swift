public import AppKit
public import Applications
import ImageIO
import Processes
import ScreenCaptureKit
import UniformTypeIdentifiers

// --- [MODELS] --------------------------------------------------------------------------

public enum Source: Codable, Hashable, Sendable {
    case windows(numbers: [CGWindowID])
    case element(id: Element.ID)
}

// --- [OPERATIONS] ----------------------------------------------------------------------

public func checkCaptureAccess(prompting: Bool = false) throws(ApplicationFailure) {
    guard prompting ? CGRequestScreenCaptureAccess() : CGPreflightScreenCaptureAccess() else {
        throw ApplicationFailure(subject: ProcessInfo.processInfo.processName, reason: .noCaptureAccess)
    }
}

private func transparent(_ image: CGImage) -> Bool {
    guard
        let context = unsafe CGContext(
            data: nil,
            width: image.width,
            height: image.height,
            bitsPerComponent: 8,
            bytesPerRow: 0,
            space: nil as CGColorSpace?,
            bitmapInfo: CGBitmapInfo(rawValue: CGImageAlphaInfo.alphaOnly.rawValue),
        )
    else { return false }
    context.draw(image, in: CGRect(x: 0, y: 0, width: image.width, height: image.height))
    return context.makeImage()?.dataProvider?.data.map { alpha in (alpha as Data).allSatisfy { byte in byte == 0 } } ?? false
}

extension Instance {
    func targets(
        _ source: Source?,
        longestSide: Int?,
    ) async throws(ApplicationFailure) -> [CGWindowID: Result<(filter: SCContentFilter, region: CGRect?, factor: CGFloat), ApplicationFailure>] {
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
                        let size = region?.size ?? filter.contentRect.size
                        let scale = CGFloat(filter.pointPixelScale)
                        return (filter, region, scale * (longestSide.map { limit in min(1, CGFloat(limit) / (max(size.width, size.height) * scale)) } ?? 1))
                    },
                )
            }
        ) { first, _ in first }
    }
    public func capture(
        _ source: Source? = nil,
        to file: (CGWindowID) -> URL,
        longestSide: Int? = nil,
    ) async throws(ApplicationFailure) -> [CGWindowID: Result<(file: URL, width: Int, height: Int), ApplicationFailure>] {
        var stills: [CGWindowID: Result<(file: URL, width: Int, height: Int), ApplicationFailure>] = [:]
        for (number, target) in try await targets(source, longestSide: longestSide) {
            let path = file(number)
            let still = await Result { () async throws(ApplicationFailure) -> (file: URL, width: Int, height: Int)? in
                let (filter, region, factor) = try target.get()
                let subject = path.path(percentEncoded: false)
                guard let type = UTType(filenameExtension: path.pathExtension) else { throw ApplicationFailure(subject: subject, reason: .noExtension) }
                let configuration = SCScreenshotConfiguration()
                (configuration.width, configuration.height) = (Int((filter.contentRect.width * factor).rounded()), Int((filter.contentRect.height * factor).rounded()))
                (configuration.showsCursor, configuration.includeChildWindows) = (false, false)
                let output: SCScreenshotOutput
                do { output = try await SCScreenshotManager.captureScreenshot(contentFilter: filter, configuration: configuration) } catch {
                    throw ApplicationFailure(subject: String(number), reason: .host(error as NSError))
                }
                let pixels = region.map { region in
                    CGRect(x: (region.minX * factor).rounded(), y: (region.minY * factor).rounded(), width: (region.width * factor).rounded(), height: (region.height * factor).rounded())
                }
                guard let image = pixels.map({ pixels in output.sdrImage?.cropping(to: pixels) }) ?? output.sdrImage else { throw ApplicationFailure(subject: String(number), reason: .noImage) }
                guard source != nil || !transparent(image) else { return nil }
                let data: CFMutableData = CFDataCreateMutable(nil, 0)
                guard let destination = CGImageDestinationCreateWithData(data, type.identifier as CFString, 1, nil) else { throw ApplicationFailure(subject: type.identifier, reason: .noEncoder) }
                CGImageDestinationAddImage(destination, image, nil)
                guard CGImageDestinationFinalize(destination) else { throw ApplicationFailure(subject: type.identifier, reason: .noEncoder) }
                do { try (data as Data).write(to: path) } catch { throw ApplicationFailure(subject: subject, reason: .host(error as NSError)) }
                return (path, image.width, image.height)
            }
            switch still {
                case .success(.some(let written)): stills[number] = .success(written)
                case .success(.none): break
                case .failure(let failure): stills[number] = .failure(failure)
            }
        }
        return stills
    }
}
