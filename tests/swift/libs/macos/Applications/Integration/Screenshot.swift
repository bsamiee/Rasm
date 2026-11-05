import AppKit
import CasePaths
import CustomDump
import ScreenCaptureKit
import TestSupport
import Testing

@testable import AppKitFixture
@testable import Applications

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

private func swatch(of instance: NSRunningApplication) async throws -> InstanceState.Window {
    _ = try await observe(
        instance,
        until: { seen in seen.last?.elements.contains { element in element.title == "Swatch" } == true },
        acting: { try await run(.swatch, in: instance) },
    )
    return try #require(try await read(instance).windows.first { row in row.description[case: \.element]?.title == "Swatch" })
}

private func image(of window: CGWindowID, in instance: NSRunningApplication, longestSide: Int? = nil) async throws -> CGImage {
    let shots: [CGWindowID: Result<SCScreenshotOutput, ApplicationFailure>] = try await instance.screenshots(of: [window], longestSide: longestSide)
    let shot: Result<SCScreenshotOutput, ApplicationFailure> = try #require(shots[window])
    return try #require(try shot.get().sdrImage)
}

private func dismiss(_ swatch: InstanceState.Window) async throws {
    let button: Control = try #require(swatch.description[case: \.element]?.detail?.controls.first { control in control.attribute == kAXCloseButtonAttribute })
    _ = try await ElementAction.perform(element: button.id, action: kAXPressAction).perform()
}

// --- [COMPOSITION] ---------------------------------------------------------------------

extension Integration {
    @Suite
    struct Screenshots {
        @Test(.application(FixtureApplication.self, launches: false))
        func backgroundWindowCaptureHoldsSwatchGridAtItsFrameAndColors() async throws {
            let (_, instance): (Installation, NSRunningApplication) = try await launched()
            try #require(CGPreflightScreenCaptureAccess(), "capture needs Screen Recording grant")
            let row: InstanceState.Window = try await swatch(of: instance)
            let captured: CGImage = try await image(of: row.number, in: instance)
            Attachment.record(captured, named: "swatch", as: .png)
            expectNoDifference(row.frame.width, Swatch.size.width)
            let scale: CGFloat = CGFloat(captured.width) / row.frame.width
            expectNoDifference(captured.height, Int((row.frame.height * scale).rounded()))
            let pixels: Pixels = try Pixels(matching: captured)
            let content: CGRect = CGRect(x: 0, y: (row.frame.height - Swatch.size.height) * scale, width: Swatch.size.width * scale, height: Swatch.size.height * scale)
            let bounds: NSRect = NSRect(origin: .zero, size: content.size)
            let probes: [(cell: String, x: Int, y: Int, expected: Sample)] = Swatch.colors.enumerated().flatMap { index, colors in
                colors.enumerated().flatMap { column, color in
                    let cell: NSRect = Swatch.cell(row: index, column: column, in: bounds)
                    let points: [(CGFloat, CGFloat)] =
                        [(cell.midX, cell.midY), (cell.minX + 4 * scale, cell.minY + 4 * scale)] + (column > 0 ? [(cell.minX, cell.midY)] : [])
                        + (column < colors.count - 1 ? [(cell.maxX - 1, cell.midY)] : []) + (index > 0 ? [(cell.midX, cell.minY)] : [])
                        + (index < Swatch.colors.count - 1 ? [(cell.midX, cell.maxY - 1)] : [])
                    return points.map { x, y in (cell: "\(index),\(column)", x: Int(x), y: Int(content.minY + y), expected: Sample(color)) }
                }
            }
            let misses: [String] = probes.compactMap { probe in
                let found: Sample? = pixels.sample(x: probe.x, y: probe.y)
                return found?.near(probe.expected) == true ? nil : "cell \(probe.cell) at \(probe.x),\(probe.y): \(found?.description ?? "outside") for \(probe.expected)"
            }
            expectNoDifference(misses, [])
            try await dismiss(row)
        }

        @Test(.application(FixtureApplication.self, launches: false), arguments: [100, 213, 4096])
        func longestSideScalesDownExactlyAndNeverUp(longestSide: Int) async throws {
            let (_, instance): (Installation, NSRunningApplication) = try await launched()
            try #require(CGPreflightScreenCaptureAccess(), "capture needs Screen Recording grant")
            let row: InstanceState.Window = try await swatch(of: instance)
            let native: CGImage = try await image(of: row.number, in: instance)
            let scaled: CGImage = try await image(of: row.number, in: instance, longestSide: longestSide)
            let longest: Int = max(native.width, native.height)
            let factor: Double = min(1, Double(longestSide) / Double(longest))
            expectNoDifference(max(scaled.width, scaled.height), min(longestSide, longest))
            let proportional: Bool = abs(Double(min(scaled.width, scaled.height)) - Double(min(native.width, native.height)) * factor) <= 1
            #expect(proportional, "scaled \(scaled.width)x\(scaled.height) from \(native.width)x\(native.height)")
            try await dismiss(row)
        }

        @Test(.application(FixtureApplication.self, launches: false))
        func closingOneWindowCostsThatWindowAlone() async throws {
            let (_, instance): (Installation, NSRunningApplication) = try await launched()
            try #require(CGPreflightScreenCaptureAccess(), "capture needs Screen Recording grant")
            let ordered: CGWindowID = try await numbered("Fixture 0", in: instance)
            let absent: CGWindowID = CGWindowID.max
            let shots: [CGWindowID: Result<SCScreenshotOutput, ApplicationFailure>] = try await instance.screenshots(of: [ordered, absent])
            let held: Result<SCScreenshotOutput, ApplicationFailure> = try #require(shots[ordered])
            _ = try #require(try held.get().sdrImage)
            let missing: Result<SCScreenshotOutput, ApplicationFailure> = try #require(shots[absent])
            let failure: ApplicationFailure = try #require(throws: ApplicationFailure.self) { try missing.get() }
            let windowless: Bool = failure.reason.is(\.noWindow)
            #expect(windowless, "absent window failed \(failure.reason)")
        }

        @Test(.application(FixtureApplication.self, launches: false), arguments: [Scenario.hidingPanel, .floatingPanel, .undescribed])
        func everyBackgroundWindowKindReadsRealPixels(scenario: Scenario) async throws {
            let (_, instance): (Installation, NSRunningApplication) = try await launched()
            try #require(CGPreflightScreenCaptureAccess(), "capture needs Screen Recording grant")
            let before: Set<CGWindowID> = Set(try await read(instance).windows.map(\.number))
            try await run(scenario, in: instance)
            let added: [CGWindowID] = try await read(instance).windows.map(\.number).filter { number in !before.contains(number) }
            for number: CGWindowID in added {
                let shots: [CGWindowID: Result<SCScreenshotOutput, ApplicationFailure>] = try await instance.screenshots(of: [number])
                let shot: Result<SCScreenshotOutput, ApplicationFailure> = try #require(shots[number])
                _ = try #require(try shot.get().sdrImage)
            }
            expectNoDifference(added.isEmpty, false)
            let after: InstanceState.Running = try await read(instance)
            expectNoDifference(after.active, false)
        }
    }
}
