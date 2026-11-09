public import AppKit
import ImageIO
public import ScreenCaptureKit
import UniformTypeIdentifiers

// --- [OPERATIONS] ----------------------------------------------------------------------

private func transparent(_ image: CGImage) -> Bool {
    guard
        let context: CGContext = unsafe CGContext(
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

extension SCScreenshotConfiguration {
    public static var background: SCScreenshotConfiguration {
        let configuration = SCScreenshotConfiguration()
        configuration.showsCursor = false
        configuration.includeChildWindows = false
        return configuration
    }
}

extension NSRunningApplication {
    public func screenshots(
        of windows: [CGWindowID],
        longestSide: Int? = nil,
        configuration: () -> SCScreenshotConfiguration = { .background },
    ) async throws(ApplicationFailure) -> [CGWindowID: Result<SCScreenshotOutput, ApplicationFailure>] {
        guard CGPreflightScreenCaptureAccess() else { throw ApplicationFailure(subject: ProcessInfo.processInfo.processName, reason: .noCaptureAccess) }
        await NSApplication.loadApplication()
        let content: SCShareableContent
        do { content = try await SCShareableContent.excludingDesktopWindows(true, onScreenWindowsOnly: false) } catch {
            throw ApplicationFailure(subject: String(processIdentifier), reason: .host(error as NSError))
        }
        let listed: [CGWindowID: SCWindow] = Dictionary(
            content.windows.filter { window in window.owningApplication?.processID == processIdentifier }.map { window in (window.windowID, window) }
        ) { first, _ in first }
        var shots: [CGWindowID: Result<SCScreenshotOutput, ApplicationFailure>] = [:]
        for number: CGWindowID in windows {
            shots[number] = await Result { () async throws(ApplicationFailure) in
                guard let window: SCWindow = listed[number], !window.frame.isEmpty else { throw ApplicationFailure(subject: String(number), reason: .noWindow) }
                let filter = SCContentFilter(desktopIndependentWindow: window)
                let scale = CGFloat(filter.pointPixelScale)
                let fitted: CGFloat = longestSide.map { limit in min(1, CGFloat(limit) / (max(filter.contentRect.width, filter.contentRect.height) * scale)) } ?? 1
                let capture: SCScreenshotConfiguration = configuration()
                capture.width = Int((filter.contentRect.width * scale * fitted).rounded())
                capture.height = Int((filter.contentRect.height * scale * fitted).rounded())
                do { return try await SCScreenshotManager.captureScreenshot(contentFilter: filter, configuration: capture) } catch {
                    throw ApplicationFailure(subject: String(number), reason: .host(error as NSError))
                }
            }
        }
        return shots
    }

    public func capturing(into directory: URL, window: CGWindowID? = nil, longestSide: Int? = nil) async throws(ApplicationFailure) -> [CGWindowID: Result<URL, ApplicationFailure>] {
        let windows: [CGWindowID]
        if let window {
            windows = [window]
        } else {
            switch await state() {
                case .running(let running): windows = running.windows.filter(\.presented).map(\.number)
                case .failed(let failure): throw failure
                case .exited: throw ApplicationFailure(subject: String(processIdentifier), reason: .noProcess)
            }
        }
        return try await screenshots(of: windows, longestSide: longestSide).reduce(into: [:]) { files, shot in
            let (number, output): (CGWindowID, Result<SCScreenshotOutput, ApplicationFailure>) = shot
            if window == nil, case .success(let image) = output, image.sdrImage.map(transparent) == true { return }
            files[number] = output.flatMap { image in
                Result { () throws(ApplicationFailure) in
                    let path: URL = directory.appending(component: String(number)).appendingPathExtension(for: .png)
                    try image.write(to: path)
                    return path
                }
            }
        }
    }
}

extension SCScreenshotOutput {
    public func write(to file: URL) throws(ApplicationFailure) {
        let path: String = file.path(percentEncoded: false)
        guard let image: CGImage = sdrImage else { throw ApplicationFailure(subject: path, reason: .noImage) }
        guard let type = UTType(filenameExtension: file.pathExtension) else { throw ApplicationFailure(subject: path, reason: .noExtension) }
        let data: CFMutableData = CFDataCreateMutable(nil, 0)
        let unencodable = ApplicationFailure(subject: type.identifier, reason: .noEncoder)
        guard let destination = CGImageDestinationCreateWithData(data, type.identifier as CFString, 1, nil) else { throw unencodable }
        CGImageDestinationAddImage(destination, image, nil)
        guard CGImageDestinationFinalize(destination) else { throw unencodable }
        do { try (data as Data).write(to: file) } catch { throw ApplicationFailure(subject: path, reason: .host(error as NSError)) }
    }
}
