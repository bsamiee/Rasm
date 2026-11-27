import AppKit
package import CasePathsCore
import Encoding
package import Foundation
import Results
package import Subprocess
import System

// --- [ERRORS] --------------------------------------------------------------------------

package struct FixtureFailure: LocalizedError, Sendable {
    package let subject: String

    package var errorDescription: String? { "Fixture output ended before \(subject)" }
}

// --- [SERVICES] ------------------------------------------------------------------------

package struct FixtureSession: Sendable {
    static let executable = Bundle(for: Delegate.self).bundleURL.deletingLastPathComponent().appending(component: "Fixture")

    package let process: pid_t
    private let writer: StandardInputWriter
    private let lines: AsyncThrowingStream<Line, any Error>

    package func send(_ command: Command) async throws {
        _ = try await writer.write(Array(JSONEncoder.jsonLines.encodeLine(command)))
    }

    package func next<Value>(_ line: CaseKeyPath<Line, Value>) async throws -> (value: Value, before: [Line]) {
        var before: [Line] = []
        for try await read in lines {
            if let value = read[case: line] { return (value, before) }
            before.append(read)
        }
        try Task.checkCancellation()
        throw FixtureFailure(subject: String(describing: line))
    }

    package static func launched<Value: Sendable>(_ body: (Self) async throws -> Value) async throws -> Value {
        try await run(.path(FilePath(executable.path(percentEncoded: false))), input: .inputWriter, output: .sequence, error: .currentStandardError) { execution in
            let (lines, continuation) = AsyncThrowingStream.makeStream(of: Line.self)
            async let drained: Void = { () async -> Void in
                let read = await Result { () async throws -> Void in
                    for try await text in execution.standardOutput.strings() { continuation.yield(try JSONDecoder().decode(Line.self, from: Data(text.utf8))) }
                }
                guard case .failure(let error) = read else { return continuation.finish() }
                continuation.finish(throwing: error)
            }()
            let session = Self(process: execution.processIdentifier.value, writer: execution.standardInputWriter, lines: lines)
            let outcome = await Result { () async throws -> Value in
                _ = try await session.next(\.ready)
                return try await body(session)
            }
            try await execution.standardInputWriter.finish()
            await drained
            return try outcome.get()
        }.closureResult
    }
}

package struct FixtureBundle: Sendable {
    static let ordersFrontAtLaunch = "OrdersFrontAtLaunch"
    static let followsProcess = "FollowsProcess"

    package let url: URL

    package func arguments(orderingFront: Bool) -> [String] {
        ["-\(Self.followsProcess)", String(getpid())] + (orderingFront ? ["-\(Self.ordersFrontAtLaunch)", "YES"] : [])
    }

    package static func installed<Value: Sendable>(_ body: (Self) async throws -> Value) async throws -> Value {
        let name = FixtureSession.executable.lastPathComponent
        let identifier = "\(name).\(UUID().uuidString)"
        let folder = URL.temporaryDirectory.appending(component: identifier)
        let bundle = Self(url: folder.appending(component: "\(name).app"))
        let contents = bundle.url.appending(component: "Contents")
        try FileManager.default.createDirectory(at: contents.appending(component: "MacOS"), withIntermediateDirectories: true)
        let outcome = await Result { () async throws -> Value in
            try FileManager.default.copyItem(at: FixtureSession.executable, to: contents.appending(components: "MacOS", name))
            let info: [String: Any] = [kCFBundleIdentifierKey as String: identifier, kCFBundleExecutableKey as String: name, "LSUIElement": true]
            try PropertyListSerialization.data(fromPropertyList: info, format: .xml, options: 0).write(to: contents.appending(component: "Info.plist"))
            return try await body(bundle)
        }
        for running in NSRunningApplication.runningApplications(withBundleIdentifier: identifier) { _ = running.forceTerminate() }
        let removed = Result { () throws -> Void in try FileManager.default.removeItem(at: folder) }
        return try combine(outcome.aggregated { (error: any Error) -> any Error in error }, removed.aggregated { (error: any Error) -> any Error in error }).get().0
    }
}
