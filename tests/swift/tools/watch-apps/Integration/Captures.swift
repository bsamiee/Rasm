import AppKit
import CasePaths
import CustomDump
import ImageIO
import Subprocess
import TestSupport
import Testing
import UniformTypeIdentifiers
import WatchAppsWire

@testable import AppKitFixture

// --- [MODELS] --------------------------------------------------------------------------

private struct Cell: Equatable {
    let row: Int
    let column: Int
    let mismatches: Int
}

// --- [OPERATIONS] ----------------------------------------------------------------------

private func capture(_ fixture: Fixture, pid: Int32, _ flags: [String] = []) async throws -> (capture: WatchAppsLine.Capture, image: CGImage, file: URL) {
    let run: (lines: [WatchAppsLine], status: TerminationStatus, standardError: String) = try await jsonLines(
        of: .product(named: "watch-apps"),
        arguments: ["capture", fixture.identifier, "--pid", String(pid)] + flags,
        as: WatchAppsLine.self,
    )
    let instances: [Int32: WatchAppsLine.Instance] = try #require(run.lines.first?.entry.instances)
    let capture: WatchAppsLine.Capture = try #require(instances[pid]?.captured)
    let temporary: String = try #require(ProcessInfo.processInfo.environment["TMPDIR"])
    let parts: [String] = [fixture.identifier, String(pid)] + capture.windows.map { window in String(window) }
    let named: String = parts.joined(separator: "-") + ".png"
    let expected: String = URL(filePath: temporary).appending(components: "watch-apps", named).resolvingSymlinksInPath().path(percentEncoded: false)
    let file: URL = URL(filePath: capture.file)
    let written: String = file.resolvingSymlinksInPath().path(percentEncoded: false)
    let source: CGImageSource = try #require(CGImageSourceCreateWithURL(file as CFURL, nil))
    let type: String? = CGImageSourceGetType(source) as String?
    let image: CGImage = try #require(CGImageSourceCreateImageAtIndex(source, 0, nil))
    expectNoDifference(run.status, .exited(0))
    expectNoDifference(run.lines.map(\.key), [fixture.identifier])
    expectNoDifference(pids(in: run.lines), [[pid]])
    expectNoDifference(written, expected)
    expectNoDifference(type, UTType.png.identifier)
    return (capture, image, file)
}

@MainActor
private func scale(of frame: CGRect) throws -> CGFloat {
    let center: CGPoint = CGPoint(x: frame.midX, y: frame.midY)
    let number: NSDeviceDescriptionKey = NSDeviceDescriptionKey("NSScreenNumber")
    let screen: NSScreen = try #require(
        NSScreen.screens.first { screen in
            if let display: CGDirectDisplayID = screen.deviceDescription[number] as? CGDirectDisplayID { CGDisplayBounds(display).contains(center) } else { false }
        }
    )
    return screen.backingScaleFactor
}

@MainActor
private func mismatches(in bitmap: NSBitmapImageRep, cell: NSRect, below bar: CGFloat, scale: CGFloat, expected: NSColor, tolerance: CGFloat = 3 / 255) -> Int {
    let xs: StrideTo<Int> = stride(from: Int(cell.minX * scale), to: Int(cell.maxX * scale), by: 3)
    let ys: StrideTo<Int> = stride(from: Int((bar + cell.minY) * scale), to: Int((bar + cell.maxY) * scale), by: 3)
    return ys.reduce(0) { count, y in
        count
            + xs.count { x in
                guard let found: NSColor = bitmap.colorAt(x: x, y: y) else { return true }
                let red: Bool = abs(found.redComponent - expected.redComponent) > tolerance
                let green: Bool = abs(found.greenComponent - expected.greenComponent) > tolerance
                let blue: Bool = abs(found.blueComponent - expected.blueComponent) > tolerance
                return red || green || blue
            }
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

extension Integration {
    struct Captures {
        @MainActor
        @Test(.application(FixtureApplication.self, launches: false))
        func captureWritesSwatchPixelsAtDisplayScale() async throws {
            let fixture: Fixture = try Fixture()
            let watched: (outcome: Int32, lines: [TimedLine<WatchAppsLine>]) = try await watching([fixture.identifier]) { lines in
                let pid: Int32 = try await launch(fixture).pid
                _ = try await lines.next { line in line.entry.instances?[pid]?.phase == .responding }
                try await press(.swatch, in: fixture, pid: pid)
                _ = try await lines.next { line in line.elements(of: pid)?.titled("Swatch") != nil }
                return pid
            }
            let pid: Int32 = watched.outcome
            let elements: [WatchAppsLine.Element] = try await read(fixture, pid: pid)
            let swatch: WatchAppsLine.Element = try #require(elements.titled("Swatch"))
            let number: Int = try #require(swatch.window)
            let frame: CGRect = try #require(swatch.detail?.frame)
            let factor: CGFloat = try scale(of: frame)
            let full: (capture: WatchAppsLine.Capture, image: CGImage, file: URL) = try await capture(fixture, pid: pid, ["--window", String(number)])
            let pixels: [Int] = [full.image.width, full.image.height]
            let expectedPixels: [Int] = [Int((frame.width * factor).rounded()), Int((frame.height * factor).rounded())]
            let content: CGSize = NSWindow.contentRect(forFrameRect: frame, styleMask: [.titled]).size
            let bar: CGFloat = frame.height - content.height
            let converted: NSBitmapImageRep? = NSBitmapImageRep(cgImage: full.image).converting(to: .sRGB, renderingIntent: .relativeColorimetric)
            let bitmap: NSBitmapImageRep = try #require(converted)
            let bounds: NSRect = NSRect(origin: .zero, size: content)
            let cells: [Cell] = Swatch.colors.enumerated().flatMap { row, colors in
                colors.enumerated().map { column, color in
                    let cell: NSRect = Swatch.cell(row: row, column: column, in: bounds).insetBy(dx: 6, dy: 6)
                    let expected: NSColor = color.usingColorSpace(.sRGB) ?? color
                    return Cell(row: row, column: column, mismatches: mismatches(in: bitmap, cell: cell, below: bar, scale: factor, expected: expected))
                }
            }
            let matching: [Cell] = cells.map { cell in Cell(row: cell.row, column: cell.column, mismatches: 0) }
            if cells != matching || pixels != expectedPixels { Attachment.record(full.image, named: "swatch", as: .png) }
            expectNoDifference(full.capture.windows, [number])
            expectNoDifference(pixels, expectedPixels)
            expectNoDifference(cells, matching)
            let longest: Int = 120
            let scaled: (capture: WatchAppsLine.Capture, image: CGImage, file: URL) = try await capture(fixture, pid: pid, ["--window", String(number), "--longest-side", String(longest)])
            let major: Int = max(full.image.width, full.image.height)
            let minor: Int = min(full.image.width, full.image.height)
            let scaledMajor: Int = max(scaled.image.width, scaled.image.height)
            let scaledMinor: Int = min(scaled.image.width, scaled.image.height)
            let proportional: Bool = abs(scaledMinor - Int((Double(longest * minor) / Double(major)).rounded())) <= 1
            expectNoDifference(scaledMajor, longest)
            #expect(proportional, "\(scaled.image.width)x\(scaled.image.height) from \(full.image.width)x\(full.image.height)")
            let all: (capture: WatchAppsLine.Capture, image: CGImage, file: URL) = try await capture(fixture, pid: pid)
            let windows: Set<Int> = Set(elements.filter { element in element.role == "AXWindow" }.compactMap(\.window))
            let quit: WatchAppsLine.Control = try #require(elements.menuBar?.detail?.controls?.compactMap(\.items).joined().first { item in item.label == "Quit" })
            expectNoDifference(Set(all.capture.windows), windows)
            try FileManager.default.removeItem(at: all.file)
            _ = try await watching([fixture.identifier], within: .seconds(15)) { lines in
                _ = try await lines.next { line in line.key == fixture.identifier }
                try await perform(quit)
                _ = try await lines.next { line in line.entry.instances?[pid]?.phase == .exited }
            }
        }

        @Test(.application(FixtureApplication.self, launches: false, activationPolicy: .regular))
        func hiddenInstancePrintsFailInPlaceOfEntry() async throws {
            let fixture: Fixture = try Fixture()
            let pid: Int32 = try await launch(fixture, flags: ["--hides"]).pid
            let elements: [WatchAppsLine.Element] = try await read(fixture, pid: pid)
            expectNoDifference(elements.titled("Fixture 0")?.minimized, false)
            try await expectRefusal(["capture", fixture.identifier], keyedBy: fixture.identifier)
        }

        @Test(.application(FixtureApplication.self, launches: false))
        func minimizedInstancePrintsFailInPlaceOfEntry() async throws {
            let fixture: Fixture = try Fixture()
            let pid: Int32 = try await launch(fixture).pid
            _ = try await watching([fixture.identifier], within: .seconds(10)) { lines in
                _ = try await lines.next { line in line.entry.instances?[pid]?.phase == .responding }
                try await press(.minimize, in: fixture, pid: pid)
                _ = try await lines.next { line in line.elements(of: pid)?.titled("Fixture 0")?.subrole != "AXStandardWindow" }
            }
            let elements: [WatchAppsLine.Element] = try await read(fixture, pid: pid)
            expectNoDifference(elements.titled("Fixture 0")?.minimized, true)
            try await expectRefusal(["capture", fixture.identifier], keyedBy: fixture.identifier)
        }
    }
}
