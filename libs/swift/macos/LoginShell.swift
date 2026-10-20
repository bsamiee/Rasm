import Foundation
public import Subprocess
import System

// --- [ERRORS] --------------------------------------------------------------------------

public enum LoginShellFailure: Error {
    case run(ProcessFailure)
    case terminationStatus(ExecutionResult<Void, StringOutput<UTF8>, StringOutput<UTF8>>)
    case markerNotFound
}

// --- [OPERATIONS] ----------------------------------------------------------------------

public enum LoginShell {
    public static var initialEnvironment: [String: String] {
        let user: String = ProcessInfo.processInfo.userName
        return [
            "HOME": FilePath(URL.homeDirectory.path(percentEncoded: false)).string, "LOGNAME": user, "PATH": _PATH_DEFPATH, "SHELL": shell.string, "USER": user,
        ]
    }

    private static var shell: FilePath {
        FilePath(
            unsafe getpwuid(getuid()).flatMap { entry in unsafe entry.pointee.pw_shell.map { path in unsafe String(cString: path) } }
                .flatMap { path in path.isEmpty ? nil : path } ?? _PATH_BSHELL
        )
    }

    public static func environment(within deadline: Duration) async -> Result<[String: String], LoginShellFailure> {
        let marker: String = UUID().uuidString
        let configuration: Configuration = ProcessRun.configuration(
            executable: .path(shell),
            arguments: ["-lc", "printf %s \(marker); exec /usr/bin/env -0"],
            environment: initialEnvironment,
            workingDirectory: .homeDirectory,
        )
        return await Result { () async throws(LoginShellFailure) in
            let output: ExecutionResult<Void, StringOutput<UTF8>, StringOutput<UTF8>> = try await ProcessRun.run(deadline: deadline) {
                try await Subprocess.run(configuration, output: .string(limit: ProcessRun.outputLimit), error: .string(limit: ProcessRun.outputLimit))
            }
            .mapError(LoginShellFailure.run).get()
            let parts: [Substring] = output.standardOutput.split(separator: marker, maxSplits: 1, omittingEmptySubsequences: false)
            guard output.terminationStatus.isSuccess else { throw .terminationStatus(output) }
            guard parts.count == 2 else { throw .markerNotFound }
            return Dictionary(
                parts[1].split(separator: "\0").compactMap { entry in
                    entry.firstIndex(of: "=").map { index in (String(entry[..<index]), String(entry[entry.index(after: index)...])) }
                }
            ) { first, _ in first }
        }
    }
}
