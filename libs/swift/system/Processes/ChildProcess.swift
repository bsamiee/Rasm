import Foundation
import Results
public import Subprocess
import System

// --- [OPERATIONS] ----------------------------------------------------------------------

extension PlatformOptions {
    public static var session: Self {
        var options = Self()
        options.createSession = true
        options.teardownSequence = [.gracefulShutDown(toProcessGroup: true, allowedDurationToNextStep: .seconds(2))]
        return options
    }
}

extension Configuration {
    public func run<Output: OutputProtocol>(
        output: Output,
        within timeout: Duration,
        input: some InputProtocol = .none,
    ) async throws(ProcessFailure) -> Output.OutputType {
        let subject = executable.description
        return try await withTimeout(timeout, expired: ProcessFailure(subject: subject, reason: .timedOut(timeout))) { () async throws(ProcessFailure) in
            let result: ExecutionResult<Void, Output, StringOutput<UTF8>>
            do {
                result = try await Subprocess.run(self, input: input, output: output, error: .string(limit: 1 << 20))
                try Task.checkCancellation()
            } catch {
                throw ProcessFailure(subject: subject, error)
            }
            guard result.terminationStatus.isSuccess else { throw ProcessFailure(subject: subject, reason: .exit(result.terminationStatus, standardError: result.standardError)) }
            return result.standardOutput
        }
    }
}

public func readLoginShellEnvironment(within timeout: Duration) async throws(ProcessFailure) -> [Environment.Key: String] {
    guard let entry = unsafe getpwuid(getuid())?.pointee else { throw ProcessFailure(subject: "uid \(getuid())", reason: .unlisted) }
    let (user, home, login) = unsafe (String(cString: entry.pw_name), String(cString: entry.pw_dir), String(cString: entry.pw_shell))
    let shell = FilePath(login.isEmpty ? _PATH_BSHELL : login)
    let marker = UUID().uuidString
    let printed = try await Configuration(
        executable: .path(shell),
        arguments: ["-lc", "printf %s \(marker); exec /usr/bin/env -0"],
        environment: .custom(["HOME": home, "LOGNAME": user, "PATH": _PATH_DEFPATH, "SHELL": shell.string, "USER": user]),
        workingDirectory: FilePath(home),
        platformOptions: .session,
    )
    .run(output: .string(limit: sysconf(_SC_ARG_MAX)), within: timeout)
    guard let start = printed.firstRange(of: marker)?.upperBound else { throw ProcessFailure(subject: shell.string, reason: .noMarker) }
    return Dictionary(
        printed[start...].split(separator: "\0").compactMap { variable in
            guard let equals = variable.firstIndex(of: "="), let key = Environment.Key(rawValue: String(variable[..<equals])) else { return nil }
            return (key, String(variable[variable.index(after: equals)...]))
        }
    ) { first, _ in first }
}
