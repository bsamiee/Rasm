import CoreGraphics
import CustomDump
import Encoding
import Foundation
import ImageIO
import Subprocess
import TestSupport
import Testing
import UniformTypeIdentifiers

@testable import AppKitFixture

// --- [OPERATIONS] ----------------------------------------------------------------------

private func image(of shot: JSONValue?, in fixture: Fixture, pid: String) throws -> CGImage {
    let file: URL = URL(filePath: try #require(shot?["file"]?.argument))
    let number: String = try #require(shot?["number"]?.argument)
    let expected: URL = URL.temporaryDirectory.appending(components: product, number).appendingPathExtension(for: .png)
    let data: Data = try Data(contentsOf: file)
    try FileManager.default.removeItem(at: file)
    let source: CGImageSource = try #require(CGImageSourceCreateWithData(data as CFData, nil))
    expectNoDifference(file.resolvingSymlinksInPath().path(percentEncoded: false), expected.resolvingSymlinksInPath().path(percentEncoded: false))
    expectNoDifference(CGImageSourceGetType(source) as String?, UTType.png.identifier)
    return try #require(CGImageSourceCreateImageAtIndex(source, 0, nil))
}

// --- [COMPOSITION] ---------------------------------------------------------------------

extension Integration {
    struct Captures {
        @Test(.application(FixtureApplication.self, launches: false))
        func capturePrintsOneNamedPNGPerWindowHonorsItsFlagsAndFailsOnAWindowItLacks() async throws {
            let fixture: Fixture = try Fixture()
            try #require(CGPreflightScreenCaptureAccess(), "capture needs Screen Recording grant")
            let pid: String = try await launch(fixture, Settings(windows: 2))
            let all: (lines: [JSONValue], status: TerminationStatus) = try await tool(["capture", pid])
            let shots: [JSONValue?] = all.lines.map { line in line[fixture.identifier, pid, "captured"] }
            for shot: JSONValue? in shots { _ = try image(of: shot, in: fixture, pid: pid) }
            expectNoDifference(shots.count, 2)
            expectNoDifference(all.status, .exited(0))
            let number: String = try #require(shots.first??["number"]?.argument)
            let longest: Int = 120
            let one: (lines: [JSONValue], status: TerminationStatus) = try await tool(["capture", pid, "--window", number, "--longest-side", String(longest)])
            let scaled: CGImage = try image(of: one.lines.first?[fixture.identifier, pid, "captured"], in: fixture, pid: pid)
            expectNoDifference(max(scaled.width, scaled.height), longest)
            let lacking: (lines: [JSONValue], status: TerminationStatus) = try await tool(["capture", pid, "--window", "0"])
            expectNoDifference(lacking.lines.map { line in line[fixture.identifier, "fail", "reason"] }, [.string("noWindow")])
            expectNoDifference(lacking.status, .exited(1))
        }
    }
}
