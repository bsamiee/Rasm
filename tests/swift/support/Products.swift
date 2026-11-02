public import Foundation
public import Subprocess
import System

// --- [MODELS] --------------------------------------------------------------------------

public struct TimedLine<Line: Sendable>: Sendable {
    public let line: Line
    public let instant: ContinuousClock.Instant
}

// --- [SERVICES] ------------------------------------------------------------------------

private final class SupportBundle {}

// --- [OPERATIONS] ----------------------------------------------------------------------

public var productsDirectory: URL { Bundle(for: SupportBundle.self).bundleURL.deletingLastPathComponent() }

public func builtProduct(named name: String) throws(TestFailure) -> URL {
    let url: URL = productsDirectory.appending(component: name)
    guard FileManager.default.isExecutableFile(atPath: url.path(percentEncoded: false)) else { throw TestFailure(subject: url.path(percentEncoded: false), reason: .missing) }
    return url
}

public func jsonLines<Line: Decodable>(
    of executable: Executable,
    arguments: [String] = [],
    as _: Line.Type = Line.self,
    limit: Int = 1 << 20,
) async throws(TestFailure) -> (lines: [Line], status: TerminationStatus, standardError: String) {
    let result: (standardOutput: String, status: TerminationStatus, standardError: String) = try await output(of: executable, arguments: arguments, limit: limit)
    let decoder: JSONDecoder = JSONDecoder()
    do {
        let lines: [Line] = try result.standardOutput.split(whereSeparator: \.isNewline).map { line in try decoder.decode(Line.self, from: Data(line.utf8)) }
        return (lines, result.status, result.standardError)
    } catch let failure as DecodingError {
        throw TestFailure(subject: ([executable.description] + arguments).joined(separator: " "), reason: .undecodable(failure))
    } catch {
        throw TestFailure(subject: ([executable.description] + arguments).joined(separator: " "), reason: .host(error))
    }
}

public func output(
    of executable: Executable,
    arguments: [String] = [],
    limit: Int = 1 << 20,
) async throws(TestFailure) -> (standardOutput: String, status: TerminationStatus, standardError: String) {
    do {
        let result: ExecutionResult<Void, StringOutput<UTF8>, StringOutput<UTF8>> = try await Subprocess.run(
            executable,
            arguments: Arguments(arguments),
            platformOptions: session(),
            output: .string(limit: limit),
            error: .string(limit: limit),
        )
        return (result.standardOutput, result.terminationStatus, result.standardError)
    } catch {
        throw TestFailure(subject: ([executable.description] + arguments).joined(separator: " "), reason: .host(error))
    }
}

public func streamingJSONLines<Line: Decodable & Sendable, Outcome: Sendable>(
    of executable: Executable,
    within budget: Duration,
    arguments: [String] = [],
    as _: Line.Type = Line.self,
    _ body: (_ lines: AsyncStream<TimedLine<Line>>) async throws -> Outcome,
) async throws -> (outcome: Outcome, lines: [TimedLine<Line>], status: TerminationStatus) {
    let subject: String = ([executable.description] + arguments).joined(separator: " ")
    let (lines, delivery): (AsyncStream<TimedLine<Line>>, AsyncStream<TimedLine<Line>>.Continuation) =
        AsyncStream.makeStream()
    let (requests, stops): (AsyncStream<Void>, AsyncStream<Void>.Continuation) = AsyncStream.makeStream()
    @concurrent
    func decode(_ output: SubprocessOutputSequence) async throws(TestFailure) -> [TimedLine<Line>] {
        defer {
            delivery.finish()
            stops.finish()
        }
        let decoder: JSONDecoder = JSONDecoder()
        var transcript: [TimedLine<Line>] = []
        do {
            for try await text: String in output.strings(separatedBy: .unicodeScalarSequence(["\n"]), bufferingPolicy: .unbounded) where !text.isEmpty {
                let timed: TimedLine<Line> = TimedLine(line: try decoder.decode(Line.self, from: Data(text.utf8)), instant: .now)
                transcript.append(timed)
                delivery.yield(timed)
            }
        } catch let failure as DecodingError {
            throw TestFailure(subject: subject, reason: .undecodable(failure))
        } catch {
            throw TestFailure(subject: subject, reason: .host(error))
        }
        return transcript
    }
    @concurrent
    func expire() async {
        guard case .success = await Result(catching: { try await Task.sleep(for: budget) }) else { return }
        stops.yield()
    }
    let result: ExecutionResult<(Outcome, [TimedLine<Line>]), SequenceOutput, DiscardedOutput> = try await Subprocess.run(
        Configuration(executable: executable, arguments: Arguments(arguments), platformOptions: session()),
        input: .none,
        output: .sequence,
        error: .discarded,
    ) { execution in
        async let transcript: [TimedLine<Line>] = decode(execution.standardOutput)
        async let termination: Void = { () async throws(SubprocessError) in
            for await _: Void in requests.prefix(1) { try execution.send(signal: .terminate, toProcessGroup: true) }
        }()
        async let _: Void = expire()
        let outcome: Outcome = try await body(lines)
        stops.yield()
        try await termination
        return (outcome, try await transcript)
    }
    return (result.closureResult.0, result.closureResult.1, result.terminationStatus)
}

extension Executable {
    public static func product(named name: String) throws(TestFailure) -> Self { .path(FilePath(try builtProduct(named: name).path(percentEncoded: false))) }
}

func session() -> PlatformOptions {
    var options: PlatformOptions = PlatformOptions()
    options.createSession = true
    options.teardownSequence = [.gracefulShutDown(toProcessGroup: true, allowedDurationToNextStep: .seconds(2))]
    return options
}
