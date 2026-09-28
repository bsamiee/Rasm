import Foundation
import Subprocess
import System

// --- [ERRORS] --------------------------------------------------------------------------

nonisolated enum LoginShellFailure: Error {
    case shellUnset
    case run(ProcessFailure)
    case terminationStatus(ProcessOutput)
    case markerNotFound
}

// --- [OPERATIONS] ----------------------------------------------------------------------

nonisolated enum LoginShell {
    static let variables: [String] = [
        "CLAUDE_CONFIG_DIR", "CLAUDE_SECURESTORAGE_CONFIG_DIR", "CODEX_HOME",
    ]

    static func exports(
        over process: [String: String]
    ) async -> Result<
        [String: String], LoginShellFailure
    > {
        let shell: Result<Executable, LoginShellFailure> =
            process["SHELL"].flatMap { name in name.isEmpty ? nil : FilePath(name) }
            .map { path in path.isAbsolute ? Executable.path(path) : .name(path.string) }
            .map(Result.success) ?? .failure(.shellUnset)
        let marker: String = UUID().uuidString
        let fields: String = variables.map { name in "\"${\(name)+1}\" \"$\(name)\"" }.joined(
            separator: " "
        )
        let script: String = "printf %s \(marker); printf '%s\\0' \(fields); printf %s \(marker)"
        return await shell.bind { executable in
            await ProcessRun.collect(
                ProcessInvocation(
                    executable: executable,
                    arguments: ["-lc", script],
                    environment: process,
                    workingDirectory: .homeDirectory,
                    inheritedInput: nil,
                ),
                deadline: .seconds(5),
            )
            .mapError(LoginShellFailure.run)
        }
        .flatMap { output in
            output.status.isSuccess ? .success(output) : .failure(.terminationStatus(output))
        }
        .flatMap { output -> Result<String, LoginShellFailure> in
            let parts: [Substring] = output.standardOutput.split(
                separator: marker,
                maxSplits: 2,
                omittingEmptySubsequences: false,
            )
            return parts.count == 3 ? .success(String(parts[1])) : .failure(.markerNotFound)
        }
        .map { dump in
            let fields: [Substring] = dump.split(separator: "\0", omittingEmptySubsequences: false)
            let exported: [(String, String)] = variables.enumerated().compactMap { index, name in
                let set: Int = index * 2
                return if fields.indices.contains(set + 1), fields[set] == "1" { (name, String(fields[set + 1])) } else { nil }
            }
            return process.merging(exported) { _, shell in shell }
        }
    }
}
