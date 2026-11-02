public import Foundation
public import Subprocess
import System

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
    of name: String,
    arguments: [String] = [],
    as _: Line.Type = Line.self,
    limit: Int = 1 << 20,
) async throws(TestFailure) -> (lines: [Line], status: TerminationStatus, standardError: String) {
    let executable: URL = try builtProduct(named: name)
    let subject: String = ([name] + arguments).joined(separator: " ")
    let result: ExecutionResult<Void, StringOutput<UTF8>, StringOutput<UTF8>>
    do {
        result = try await Subprocess.run(
            .path(FilePath(executable.path(percentEncoded: false))),
            arguments: Arguments(arguments),
            platformOptions: session(),
            output: .string(limit: limit),
            error: .string(limit: limit),
        )
    } catch {
        throw TestFailure(subject: subject, reason: .host(error))
    }
    let decoder: JSONDecoder = JSONDecoder()
    let lines: [Line]
    do {
        lines = try result.standardOutput.split(whereSeparator: \.isNewline).map { line in try decoder.decode(Line.self, from: Data(line.utf8)) }
    } catch let failure as DecodingError {
        throw TestFailure(subject: subject, reason: .undecodable(failure))
    } catch {
        throw TestFailure(subject: subject, reason: .host(error))
    }
    return (lines, result.terminationStatus, result.standardError)
}

func session() -> PlatformOptions {
    var options: PlatformOptions = PlatformOptions()
    options.createSession = true
    options.teardownSequence = [.gracefulShutDown(toProcessGroup: true, allowedDurationToNextStep: .seconds(2))]
    return options
}
