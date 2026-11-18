import Applications
import CoreImage
import CustomDump
import Foundation
import JSONRPC
import Testing

@testable import ApplicationServer

// --- [MODELS] --------------------------------------------------------------------------

private struct Previewed: Decodable {
    let width: Int
    let height: Int
    let preview: Data
}

// --- [OPERATIONS] ----------------------------------------------------------------------

private func sizes(previewing size: CGSize, longestSide: Int) async throws -> [CGSize] {
    let file = FileManager.default.temporaryDirectory.appending(component: "\(UUID().uuidString).png")
    let space = try #require(CGColorSpace(name: CGColorSpace.sRGB))
    try CIContext().writePNGRepresentation(of: CIImage(color: CIColor(red: 0.8, green: 0.3, blue: 0.1)).cropped(to: CGRect(origin: .zero, size: size)), to: file, format: .RGBA8, colorSpace: space)
    defer {
        do { try FileManager.default.removeItem(at: file) } catch { Issue.record(error) }
    }
    let request = try JSONDecoder().decode(Request.self, from: Data(#"{"preview":{"file":"\#(file.absoluteString)","longestSide":\#(longestSide)}}"#.utf8))
    let encoded: Data? =
        switch try await request.perform(admitting: { NSPredicate(value: false) }, refusing: { _ in [] }, definitions: ScriptingDefinitions(cached: { _ in nil })) {
            case .value(let value?): try JSONEncoder().encode(value)
            case .value(.none), .stream: nil
        }
    let answer = try JSONDecoder().decode(Previewed.self, from: try #require(encoded))
    return [CGSize(width: answer.width, height: answer.height), try #require(CIImage(data: answer.preview)).extent.size]
}

@Suite
struct PreviewTests {
    @Test
    func scalesLongestSideDownToLimitAndReportsFileSize() async throws {
        let measured = try await sizes(previewing: CGSize(width: 96, height: 240), longestSide: 60)
        expectNoDifference(measured, [CGSize(width: 96, height: 240), CGSize(width: 24, height: 60)])
    }

    @Test
    func keepsImageUnscaledWhenLimitExceedsLongestSide() async throws {
        let measured = try await sizes(previewing: CGSize(width: 96, height: 240), longestSide: 500)
        expectNoDifference(measured, [CGSize(width: 96, height: 240), CGSize(width: 96, height: 240)])
    }
}
