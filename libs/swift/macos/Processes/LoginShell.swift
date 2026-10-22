public import CasePaths
public import Foundation
import Results
public import Subprocess
import System

// --- [ERRORS] --------------------------------------------------------------------------

@CasePathable
public enum LoginShellFailure: CustomNSError, LocalizedError {
    case unlisted(uid_t)
    case run(ProcessFailure)
    case markerNotFound

    public var errorDescription: String? {
        switch self {
            case .unlisted(let uid): "getpwuid found no user database entry for uid \(uid)"
            case .run(let error): "Login shell environment read failed: \(error.localizedDescription)"
            case .markerNotFound: "Login shell printed no environment"
        }
    }

    public var errorUserInfo: [String: Any] {
        self[case: \.run].map { error in [NSUnderlyingErrorKey: error] } ?? [:]
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

public enum LoginShell: Sendable {
    public static func environment(within timeout: Duration) async -> Result<[Environment.Key: String], LoginShellFailure> {
        let marker: String = UUID().uuidString
        return await Result { () async throws(LoginShellFailure) in
            guard let entry: passwd = unsafe getpwuid(getuid())?.pointee else { throw .unlisted(getuid()) }
            let (user, home, login): (String, String, String) = unsafe (String(cString: entry.pw_name), String(cString: entry.pw_dir), String(cString: entry.pw_shell))
            let shell: FilePath = FilePath(login.isEmpty ? _PATH_BSHELL : login)
            let output: ExecutionResult<Void, StringOutput<UTF8>, DiscardedOutput> = try await withTimeout(timeout) {
                await Result {
                    try await Subprocess.run(
                        .path(shell),
                        arguments: ["-lc", "printf %s \(marker); exec /usr/bin/env -0"],
                        environment: .custom(["HOME": home, "LOGNAME": user, "PATH": _PATH_DEFPATH, "SHELL": shell.string, "USER": user]),
                        workingDirectory: FilePath(home),
                        platformOptions: .session,
                        output: .string(limit: sysconf(_SC_ARG_MAX)),
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
