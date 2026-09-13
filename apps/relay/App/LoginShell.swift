import Foundation

enum LoginShellFailure: Error {
  case shellUnset
  case run(any Error)
  case terminationStatus(Int32)
  case markerNotFound
}

enum LoginShell {
  static func environment(over process: [String: String]) -> Result<
    [String: String], LoginShellFailure
  > {
    process["SHELL"].map { environment(shell: $0, over: process) } ?? .failure(.shellUnset)
  }

  private static func environment(shell path: String, over process: [String: String]) -> Result<
    [String: String], LoginShellFailure
  > {
    let marker: String = UUID().uuidString
    let shell: Process = Process()
    shell.executableURL = URL(filePath: path)
    // -ilc: zsh reads .zprofile only in a login shell and .zshrc only in an interactive shell
    shell.arguments = ["-ilc", "printf %s \(marker); /usr/bin/env -0; printf %s \(marker)"]
    let output: Pipe = Pipe()
    shell.standardInput = FileHandle.nullDevice
    shell.standardOutput = output
    shell.standardError = FileHandle.nullDevice
    return Result(catching: shell.run)
      .mapError(LoginShellFailure.run)
      .flatMap { _ -> Result<Data, LoginShellFailure> in
        let printed: Data = output.fileHandleForReading.readDataToEndOfFile()
        shell.waitUntilExit()
        return shell.terminationStatus == 0
          ? .success(printed) : .failure(.terminationStatus(shell.terminationStatus))
      }
      .flatMap { printed -> Result<Data, LoginShellFailure> in
        let frame: Data = Data(marker.utf8)
        guard let opening: Range<Data.Index> = printed.range(of: frame),
          let closing: Range<Data.Index> = printed.range(
            of: frame, in: opening.upperBound..<printed.endIndex)
        else { return .failure(.markerNotFound) }
        return .success(printed[opening.upperBound..<closing.lowerBound])
      }
      .map { dump in
        let exported: [(String, String)] = dump.split(separator: 0).compactMap { entry in
          guard let pair: String = String(data: entry, encoding: .utf8),
            let separator: String.Index = pair.firstIndex(of: "=")
          else { return nil }
          return (String(pair[..<separator]), String(pair[pair.index(after: separator)...]))
        }
        return process.merging(exported) { _, shell in shell }
      }
  }
}
