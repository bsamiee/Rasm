import Foundation
import Subprocess

nonisolated enum LoginShellFailure: Error {
  case shellUnset
  case shellNotOnPath(String)
  case run(ProcessFailure)
  case terminationStatus(ProcessOutput)
  case markerNotFound
}

nonisolated enum LoginShell {
  static let variables: [String] = [
    "CLAUDE_CONFIG_DIR", "CLAUDE_SECURESTORAGE_CONFIG_DIR", "CODEX_HOME",
  ]
  private static let deadline: Duration = .seconds(5)
  private static let workingDirectory: URL = URL.homeDirectory

  static func exports(over process: [String: String]) async -> Result<
    [String: String], LoginShellFailure
  > {
    let shell: Result<URL, LoginShellFailure> =
      process["SHELL"].flatMap { name in name.isEmpty ? nil : name }
      .map { name in executable(named: name, searching: process["PATH"]) } ?? .failure(.shellUnset)
    let marker: String = UUID().uuidString
    let fields: String = variables.map { name in "\"${\(name)+1}\" \"$\(name)\"" }.joined(
      separator: " ")
    let script: String = "printf %s \(marker); printf '%s\\0' \(fields); printf %s \(marker)"
    return await shell.bind { executable in
      await ProcessRun.collect(
        ProcessInvocation(
          executable: executable, arguments: ["-lc", script],
          environment: process, workingDirectory: workingDirectory
        ), deadline: deadline
      )
      .mapError(LoginShellFailure.run)
    }
    .flatMap { output in
      output.status.isSuccess ? .success(output) : .failure(.terminationStatus(output))
    }
    .flatMap { output -> Result<String, LoginShellFailure> in
      let printed: String = output.standardOutput
      guard let opening: Range<String.Index> = printed.range(of: marker),
        let closing: Range<String.Index> = printed.range(
          of: marker, range: opening.upperBound..<printed.endIndex)
      else { return .failure(.markerNotFound) }
      return .success(String(printed[opening.upperBound..<closing.lowerBound]))
    }
    .map { dump in
      let fields: [Substring] = dump.split(separator: "\0", omittingEmptySubsequences: false)
      let exported: [(String, String)] = variables.enumerated().compactMap { index, name in
        let set: Int = index * 2
        guard fields.indices.contains(set + 1), fields[set] == "1" else { return nil }
        return (name, String(fields[set + 1]))
      }
      return process.merging(exported) { _, shell in shell }
    }
  }

  private static func executable(
    named name: String, searching path: String?
  ) -> Result<URL, LoginShellFailure> {
    name.contains("/")
      ? .success(URL(filePath: name, directoryHint: .notDirectory, relativeTo: workingDirectory))
      : (path?.split(separator: ":") ?? [])
        .map { directory in
          URL(filePath: String(directory), directoryHint: .isDirectory).appending(path: name)
        }
        .first { url in FileManager.default.isExecutableFile(atPath: url.path) }
        .map(Result<URL, LoginShellFailure>.success) ?? .failure(.shellNotOnPath(name))
  }
}
