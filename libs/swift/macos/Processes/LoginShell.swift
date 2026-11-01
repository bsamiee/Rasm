import Foundation
public import Subprocess
import System

// --- [OPERATIONS] ----------------------------------------------------------------------

public func loginShellEnvironment(within timeout: Duration) async throws(ProcessFailure) -> [Environment.Key: String] {
    guard let entry: passwd = unsafe getpwuid(getuid())?.pointee else { throw ProcessFailure(subject: "uid \(getuid())", reason: .unlisted) }
    let (user, home, login): (String, String, String) = unsafe (String(cString: entry.pw_name), String(cString: entry.pw_dir), String(cString: entry.pw_shell))
    let shell: FilePath = FilePath(login.isEmpty ? _PATH_BSHELL : login)
    let marker: String = UUID().uuidString
    let printed: String = try await Configuration(
        executable: .path(shell),
        arguments: ["-lc", "printf %s \(marker); exec /usr/bin/env -0"],
        environment: .custom(["HOME": home, "LOGNAME": user, "PATH": _PATH_DEFPATH, "SHELL": shell.string, "USER": user]),
        workingDirectory: FilePath(home),
        platformOptions: .session(),
    )
    .run(output: .string(limit: sysconf(_SC_ARG_MAX)), within: timeout)
    .standardOutput
    let parts: [Substring] = printed.split(separator: marker, maxSplits: 1, omittingEmptySubsequences: false)
    guard parts.count == 2 else { throw ProcessFailure(subject: shell.string, reason: .markerNotFound) }
    return Dictionary(
        parts[1].split(separator: "\0").compactMap { variable in
            variable.firstIndex(of: "=").map { index in (Environment.Key(stringLiteral: String(variable[..<index])), String(variable[variable.index(after: index)...])) }
        }
    ) { first, _ in first }
}
