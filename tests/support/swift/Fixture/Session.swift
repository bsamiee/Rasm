import CasePaths
public import CasePathsCore
import Encoding
public import Foundation
public import Subprocess
import System

// --- [ERRORS] --------------------------------------------------------------------------

public struct FixtureFailure: LocalizedError, Sendable {
    @CasePathable
    public enum Reason: Sendable {
        case ended
    }

    public let subject: String
    public let reason: Reason

    public var errorDescription: String? { "\(reason): \(subject)" }
}

// --- [SERVICES] ------------------------------------------------------------------------

public struct FixtureSession: Sendable {
    public let process: pid_t
    private let writer: StandardInputWriter
    private let lines: AsyncThrowingStream<Line, any Error>

    public func send(_ command: Command) async throws {
        _ = try await writer.write(Array(JSONEncoder.lines.encodeLine(command)))
    }

    public func next<Value>(_ line: CaseKeyPath<Line, Value>) async throws -> Value {
        for try await read in lines {
            if let value = read[case: line] { return value }
        }
        throw FixtureFailure(subject: String(describing: line), reason: .ended)
    }

    public static func launched<Value: Sendable>(_ body: (Self) async throws -> Value) async throws -> Value {
        let executable = Bundle(for: Delegate.self).bundleURL.deletingLastPathComponent().appending(component: "Fixture")
        return try await run(.path(FilePath(executable.path(percentEncoded: false))), input: .inputWriter, output: .sequence, error: .currentStandardError) { execution in
            let (lines, continuation) = AsyncThrowingStream.makeStream(of: Line.self)
            async let drained: Void = drain(execution.standardOutput, into: continuation)
            let session = Self(process: execution.processIdentifier.value, writer: execution.standardInputWriter, lines: lines)
            let outcome: Result<Value, any Error>
            do {
                _ = try await session.next(\.ready)
                outcome = .success(try await body(session))
            } catch {
                outcome = .failure(error)
            }
            try await execution.standardInputWriter.finish()
            await drained
            return try outcome.get()
        }.closureResult
    }

    private static func drain(_ output: SubprocessOutputSequence, into continuation: AsyncThrowingStream<Line, any Error>.Continuation) async {
        do {
            for try await text in output.strings() { continuation.yield(try JSONDecoder().decode(Line.self, from: Data(text.utf8))) }
            continuation.finish()
        } catch {
            continuation.finish(throwing: error)
        }
    }
}
