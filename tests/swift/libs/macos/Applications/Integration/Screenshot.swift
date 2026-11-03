import AppKit
import Applications
import CasePaths
import CustomDump
import ScreenCaptureKit
import TestSupport
import Testing

@testable import AppKitFixture

// --- [MODELS] --------------------------------------------------------------------------

private struct Sample: Equatable, CustomStringConvertible {
    let red: Int
    let green: Int
    let blue: Int

    init(red: Int, green: Int, blue: Int) {
        (self.red, self.green, self.blue) = (red, green, blue)
    }

    init(_ color: NSColor) {
        let srgb: NSColor = color.usingColorSpace(.sRGB) ?? color
        self.init(red: Int((srgb.redComponent * 255).rounded()), green: Int((srgb.greenComponent * 255).rounded()), blue: Int((srgb.blueComponent * 255).rounded()))
    }

    var description: String { "(\(red), \(green), \(blue))" }

    func near(_ other: Self, within tolerance: Int = 3) -> Bool { max(abs(red - other.red), abs(green - other.green), abs(blue - other.blue)) <= tolerance }
}

private struct Pixels {
    let bytes: Data
    let width: Int
    let height: Int
    let bytesPerRow: Int

    init(matching image: CGImage) throws {
        let space: CGColorSpace = try #require(CGColorSpace(name: CGColorSpace.sRGB))
        let context: CGContext = try unsafe #require(
            CGContext(
                data: nil,
                width: image.width,
                height: image.height,
                bitsPerComponent: 8,
                bytesPerRow: 0,
                space: space as CGColorSpace?,
                bitmapInfo: CGBitmapInfo(rawValue: CGImageAlphaInfo.premultipliedLast.rawValue),
            )
        )
        context.draw(image, in: CGRect(x: 0, y: 0, width: image.width, height: image.height))
        let drawn: CGImage = try #require(context.makeImage())
        let data: CFData = try #require(drawn.dataProvider?.data)
        (bytes, width, height, bytesPerRow) = (data as Data, image.width, image.height, drawn.bytesPerRow)
    }

    func sample(x: Int, y: Int) -> Sample? {
        guard (0..<width).contains(x), (0..<height).contains(y) else { return nil }
        let offset: Int = y * bytesPerRow + x * 4
        return Sample(red: Int(bytes[offset]), green: Int(bytes[offset + 1]), blue: Int(bytes[offset + 2]))
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

private func swatch(of instance: NSRunningApplication) async throws -> Element {
    let seen: [InstanceState] = try await observe(
        instance,
        until: { seen in seen.last?.elements.contains { element in element.title == "Swatch" && element.detail?.frame != nil } == true },
        walking: { element in element.title == "Swatch" },
        acting: { try await run(.swatch, in: instance) },
    )
    return try #require(seen.last?.elements.first { element in element.title == "Swatch" })
}

private func dismiss(_ swatch: Element) async throws {
    let button: Control = try #require(swatch.detail?.controls.first { control in control.attribute == kAXCloseButtonAttribute })
    try await ElementAction.perform(element: button.id, action: kAXPressAction).perform()
}

// --- [COMPOSITION] ---------------------------------------------------------------------

extension Integration {
    @Suite
    struct Screenshots {
        @Test(.application(FixtureApplication.self, arguments: Settings().arguments))
        func windowCaptureHoldsSwatchGridAtItsFrameAndColors() async throws {
            let (_, instance): (Installation, NSRunningApplication) = try await launched()
            try #require(CGPreflightScreenCaptureAccess(), "capture needs Screen Recording grant")
            let element: Element = try await swatch(of: instance)
            let window: CGWindowID = try #require(element.window)
            let frame: CGRect = try #require(element.detail?.frame)
            let captured: (output: SCScreenshotOutput, windows: [CGWindowID]) = try await instance.screenshot(of: window)
            let image: CGImage = try #require(captured.output.sdrImage)
            Attachment.record(image, named: "swatch", as: .png)
            expectNoDifference(captured.windows, [window])
            expectNoDifference(frame.width, Swatch.size.width)
            let scale: CGFloat = CGFloat(image.width) / frame.width
            expectNoDifference(image.height, Int((frame.height * scale).rounded()))
            let pixels: Pixels = try Pixels(matching: image)
            let content: CGRect = CGRect(x: 0, y: (frame.height - Swatch.size.height) * scale, width: Swatch.size.width * scale, height: Swatch.size.height * scale)
            let bounds: NSRect = NSRect(origin: .zero, size: content.size)
            let probes: [(cell: String, x: Int, y: Int, expected: Sample)] = Swatch.colors.enumerated().flatMap { row, colors in
                colors.enumerated().flatMap { column, color in
                    let cell: NSRect = Swatch.cell(row: row, column: column, in: bounds)
                    let points: [(CGFloat, CGFloat)] =
                        [(cell.midX, cell.midY), (cell.minX + 4 * scale, cell.minY + 4 * scale)] + (column > 0 ? [(cell.minX, cell.midY)] : [])
                        + (column < colors.count - 1 ? [(cell.maxX - 1, cell.midY)] : []) + (row > 0 ? [(cell.midX, cell.minY)] : [])
                        + (row < Swatch.colors.count - 1 ? [(cell.midX, cell.maxY - 1)] : [])
                    return points.map { x, y in (cell: "\(row),\(column)", x: Int(x), y: Int(content.minY + y), expected: Sample(color)) }
                }
            }
            let misses: [String] = probes.compactMap { probe in
                let found: Sample? = pixels.sample(x: probe.x, y: probe.y)
                return found?.near(probe.expected) == true ? nil : "cell \(probe.cell) at \(probe.x),\(probe.y): \(found?.description ?? "outside") for \(probe.expected)"
            }
            expectNoDifference(misses, [])
            try await dismiss(element)
        }

        @Test(.application(FixtureApplication.self, arguments: Settings().arguments), arguments: [100, 213, 4096])
        func longestSideScalesDownExactlyAndNeverUp(longestSide: Int) async throws {
            let (_, instance): (Installation, NSRunningApplication) = try await launched()
            try #require(CGPreflightScreenCaptureAccess(), "capture needs Screen Recording grant")
            let element: Element = try await swatch(of: instance)
            let window: CGWindowID = try #require(element.window)
            let native: CGImage = try #require(try await instance.screenshot(of: window).output.sdrImage)
            let scaled: CGImage = try #require(try await instance.screenshot(of: window, longestSide: longestSide).output.sdrImage)
            let longest: Int = max(native.width, native.height)
            let factor: Double = min(1, Double(longestSide) / Double(longest))
            expectNoDifference(max(scaled.width, scaled.height), min(longestSide, longest))
            let proportional: Bool = abs(Double(min(scaled.width, scaled.height)) - Double(min(native.width, native.height)) * factor) <= 1
            #expect(proportional, "scaled \(scaled.width)x\(scaled.height) from \(native.width)x\(native.height)")
            Attachment.record(scaled, named: "swatch-\(longestSide)", as: .png)
            let ratio: CGFloat = CGFloat(scaled.width) / CGFloat(native.width)
            let bar: CGFloat = CGFloat(native.height) - Swatch.size.height * CGFloat(native.width) / Swatch.size.width
            let content: CGRect = CGRect(x: 0, y: bar * ratio, width: CGFloat(scaled.width), height: CGFloat(scaled.height) - bar * ratio)
            let pixels: Pixels = try Pixels(matching: scaled)
            let misses: [String] = Swatch.colors.enumerated().flatMap { row, colors in
                colors.enumerated().compactMap { column, color in
                    let cell: NSRect = Swatch.cell(row: row, column: column, in: NSRect(origin: .zero, size: content.size))
                    let found: Sample? = pixels.sample(x: Int(cell.midX), y: Int(content.minY + cell.midY))
                    return found?.near(Sample(color)) == true ? nil : "cell \(row),\(column): \(found?.description ?? "outside") for \(Sample(color))"
                }
            }
            expectNoDifference(misses, [])
            try await dismiss(element)
        }

        @Test(.application(FixtureApplication.self, launches: false))
        func zeroSizeWindowCaptureFailsNamingItsNumberAndFrame() async throws {
            let (_, instance): (Installation, NSRunningApplication) = try await launched(Settings())
            try #require(CGPreflightScreenCaptureAccess(), "capture needs Screen Recording grant")
            let seen: [InstanceState] = try await observe(
                instance,
                until: { seen in seen.last?.elements.contains { element in element.title == "Collapsed" } == true },
                acting: { try await run(.collapsed, in: instance) },
            )
            let numbered: CGWindowID? = seen.last?.elements.first { element in element.title == "Collapsed" }?.window
            let collapsed: CGWindowID = try #require(numbered)
            let failure: ApplicationFailure = try await #require(throws: ApplicationFailure.self) { try await instance.screenshot(of: collapsed) }
            let absent: String = try #require(failure.reason[case: \.absent])
            expectNoDifference(absent.contains(String(collapsed)), true, absent)
        }

        @Test(.application(FixtureApplication.self, arguments: Settings().arguments))
        func applicationCaptureHoldsOnScreenWindowsAlone() async throws {
            let (_, instance): (Installation, NSRunningApplication) = try await launched()
            try #require(CGPreflightScreenCaptureAccess(), "capture needs Screen Recording grant")
            let numbered: CGWindowID? = try await read(instance).first { element in element.title == "Fixture 0" }?.window
            let ordered: CGWindowID = try #require(numbered)
            let element: Element = try await swatch(of: instance)
            let window: CGWindowID = try #require(element.window)
            let frame: CGRect = try #require(element.detail?.frame)
            let captured: (output: SCScreenshotOutput, windows: [CGWindowID]) = try await instance.screenshot()
            let image: CGImage = try #require(captured.output.sdrImage)
            Attachment.record(image, named: "application", as: .png)
            expectNoDifference(captured.windows, [window])
            let scale: CGFloat = CGFloat(image.width) / frame.width
            expectNoDifference(image.height, Int((frame.height * scale).rounded()))
            let failure: ApplicationFailure = try await #require(throws: ApplicationFailure.self) { try await instance.screenshot(of: ordered) }
            let absent: Bool = failure.reason.is(\.absent)
            #expect(absent, "ordered out capture failed \(failure.reason)")
            try await dismiss(element)
        }
    }
}
