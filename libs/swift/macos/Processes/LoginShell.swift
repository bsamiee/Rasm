public import Foundation
import Results
public import Subprocess
import System

// --- [ERRORS] --------------------------------------------------------------------------

public enum LoginShellFailure: CustomNSError, LocalizedError {
    case run(ProcessFailure)
    case markerNotFound

    public var errorDescription: String? {
        switch self {
            case .run(let error): "Login shell environment read failed: \(error.localizedDescription)"
            case .markerNotFound: "Login shell printed no environment"
        }
    }

    public var errorUserInfo: [String: Any] {
        if case .run(let error) = self { [NSUnderlyingErrorKey: error] } else { [:] }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

public enum LoginShell {
    private static var shell: FilePath {
        FilePath(
            unsafe getpwuid(getuid()).flatMap { entry in unsafe entry.pointee.pw_shell.map { path in unsafe String(cString: path) } }
                .flatMap { path in path.isEmpty ? nil : path } ?? _PATH_BSHELL
        )
    }

    public static func environment(within timeout: Duration) async -> Result<[Environment.Key: String], LoginShellFailure> {
        let marker: String = UUID().uuidString
        let user: String = ProcessInfo.processInfo.userName
        return await Result { () async throws(LoginShellFailure) in
            let output: ExecutionResult<Void, StringOutput<UTF8>, DiscardedOutput> = try await withTimeout(timeout) {
                await Result {
                    try await Subprocess.run(
                        .path(shell),
                        arguments: ["-lc", "printf %s \(marker); exec /usr/bin/env -0"],
                        environment: .custom(["HOME": URL.homeDirectory.path(percentEncoded: false), "LOGNAME": user, "PATH": _PATH_DEFPATH, "SHELL": shell.string, "USER": user]),
                        workingDirectory: FilePath(URL.homeDirectory.path(percentEncoded: false)),
                        platformOptions: .session,
                        output: .string(limit: 1 << 20),
                    )
                }
                .mapError(ProcessFailure.init)
            }
            .mapError(LoginShellFailure.run).get()
            guard output.terminationStatus.isSuccess else { throw .run(.exit(output.terminationStatus)) }
            let parts: [Substring] = output.standardOutput.split(separator: marker, maxSplits: 1, omittingEmptySubsequences: false)
            guard parts.count == 2 else { throw .markerNotFound }
            return Dictionary(
                parts[1].split(separator: "\0").compactMap { entry in
                    entry.firstIndex(of: "=").map { index in (Environment.Key(stringLiteral: String(entry[..<index])), String(entry[entry.index(after: index)...])) }
                }
            ) { first, _ in first }
        }
    }
}
