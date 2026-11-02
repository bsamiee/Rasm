import CustomDump
import Darwin
import Files
import Foundation
import System
import TestSupport
import Testing

// --- [OPERATIONS] ----------------------------------------------------------------------

private func kernelPath(of url: URL) throws -> String {
    let descriptor: FileDescriptor = try FileDescriptor.open(FilePath(url.path(percentEncoded: false)), .readOnly)
    return try descriptor.closeAfter {
        var buffer: [CChar] = Array(repeating: 0, count: Int(MAXPATHLEN))
        let status: Int32 = unsafe fcntl(descriptor.rawValue, F_GETPATH, &buffer)
        let resolved: Bool = status == 0
        try #require(resolved)
        let path: String? = String(bytes: buffer.prefix { character in character != 0 }.map(UInt8.init(bitPattern:)), encoding: .utf8)
        return try #require(path)
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

@Suite(.testDirectory)
struct CanonicalPaths {
    @Test
    func resolvesTemporaryDirectoryToKernelPath() throws {
        let canonical: String = try URL.temporaryDirectory.canonical().path(percentEncoded: false)
        expectNoDifference(canonical, try kernelPath(of: URL.temporaryDirectory))
        #expect(!URL.temporaryDirectory.path(percentEncoded: false).hasPrefix(canonical))
    }

    @Test
    func resolvesEveryLinkInChainAndEachDotDotComponent() throws {
        let directory: URL = try #require(TestDirectory.current)
        let target: URL = directory.appending(path: "Café Menu.txt")
        try Data("menu".utf8).write(to: target)
        try FileManager.default.createDirectory(at: directory.appending(path: "nested"), withIntermediateDirectories: false)
        try FileManager.default.createSymbolicLink(at: directory.appending(path: "first"), withDestinationURL: target)
        try FileManager.default.createSymbolicLink(atPath: directory.appending(path: "second").path(percentEncoded: false), withDestinationPath: "nested/../first")
        let resolved: [String] = try ["second", "first", "nested/../Café Menu.txt", "./nested/./../second"].map { path in
            try directory.appending(path: path).canonical().path(percentEncoded: false)
        }
        expectNoDifference(resolved, Array(repeating: try kernelPath(of: target), count: 4))
    }

    @Test
    func canonicalPathIsFixedPoint() throws {
        let directory: URL = try #require(TestDirectory.current).canonical()
        expectNoDifference(try directory.canonical().path(percentEncoded: false), directory.path(percentEncoded: false))
        let temporary: URL = try URL.temporaryDirectory.canonical()
        expectNoDifference(try temporary.canonical().path(percentEncoded: false), temporary.path(percentEncoded: false))
    }

    @Test
    func missingPathThrowsNoSuchFile() throws {
        let missing: URL = try #require(TestDirectory.current).appending(path: "absent/child")
        #expect(throws: Errno.noSuchFileOrDirectory) { try missing.canonical() }
    }

    @Test
    func danglingLinkThrowsNoSuchFile() throws {
        let link: URL = try #require(TestDirectory.current).appending(path: "dangling")
        try FileManager.default.createSymbolicLink(atPath: link.path(percentEncoded: false), withDestinationPath: "nowhere")
        #expect(throws: Errno.noSuchFileOrDirectory) { try link.canonical() }
    }
}

@Suite(.testDirectory)
struct PresentReads {
    @Test
    func missingFileReadsAsNil() throws {
        let missing: URL = try #require(TestDirectory.current).appending(path: "absent.json")
        expectNoDifference(try ifPresent { try Data(contentsOf: missing) }, nil)
        expectNoDifference(try ifPresent { try FileManager.default.attributesOfItem(atPath: missing.path(percentEncoded: false)).count }, nil)
        expectNoDifference(try ifPresent { try FileManager.default.contentsOfDirectory(at: missing, includingPropertiesForKeys: nil) }, nil)
        expectNoDifference(try ifPresent { try FileManager.default.removeItem(at: missing) } == nil, true)
        expectNoDifference(try ifPresent { () throws(Errno) -> Int in throw Errno.noSuchFileOrDirectory }, nil)
    }

    @Test
    func presentFileReadsItsValue() throws {
        let file: URL = try #require(TestDirectory.current).appending(path: "present.json")
        try Data("{}".utf8).write(to: file)
        expectNoDifference(try ifPresent { try Data(contentsOf: file) }, Data("{}".utf8))
    }

    @Test
    func otherFailuresPropagateUnchanged() throws {
        let file: URL = try #require(TestDirectory.current).appending(path: "locked.json")
        try Data("{}".utf8).write(to: file)
        try FileManager.default.setAttributes([.posixPermissions: 0o000], ofItemAtPath: file.path(percentEncoded: false))
        let denied: CocoaError? = #expect(throws: CocoaError.self) { try ifPresent { try Data(contentsOf: file) } }
        expectNoDifference(denied?.code, .fileReadNoPermission)
        #expect(throws: Errno.permissionDenied) { try ifPresent { () throws(Errno) -> Int in throw Errno.permissionDenied } }
    }
}
