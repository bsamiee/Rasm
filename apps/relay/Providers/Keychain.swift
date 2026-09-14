import Foundation
import Subprocess

nonisolated enum KeychainFailure: Error, Sendable {
  case denied
  case interactionNotAllowed
  case process(ProcessFailure)
}

nonisolated enum Keychain {
  private static let tool: URL = URL(filePath: "/usr/bin/security")
  private static let deadline: Duration = .seconds(15)
  private static let errSecItemNotFound: OSStatus = -25300
  private static let errSecInteractionNotAllowed: OSStatus = -25308
  private static let notFoundStatus: Int32 = exitStatus(of: errSecItemNotFound)
  private static let interactionNotAllowedStatus: Int32 = exitStatus(
    of: errSecInteractionNotAllowed)
  private static let workingDirectory: URL = URL.homeDirectory

  static func readGenericPassword(
    service: String, account: String
  ) async -> Result<Data?, KeychainFailure> {
    await ProcessRun.collect(
      ProcessInvocation(
        executable: tool,
        arguments: ["find-generic-password", "-a", account, "-s", service, "-w"],
        environment: [:], workingDirectory: Self.workingDirectory
      ), deadline: deadline
    )
    .mapError(KeychainFailure.process)
    .flatMap { output in
      switch output.status {
      case .exited(0): .success(decode(output.standardOutput))
      case .exited(let code) where code == notFoundStatus: .success(nil)
      case .exited(let code) where code == interactionNotAllowedStatus:
        .failure(.interactionNotAllowed)
      case .exited, .signaled: .failure(.denied)
      }
    }
  }

  static func writeGenericPassword(
    service: String, account: String, data: Data
  ) async -> Result<Void, KeychainFailure> {
    let command: String =
      "add-generic-password -U -a \(quoted(account)) -s \(quoted(service)) -X \(data.hexEncoded)\n"
    return await ProcessRun.collect(
      ProcessInvocation(
        executable: tool, arguments: ["-i"], environment: [:],
        workingDirectory: Self.workingDirectory),
      input: .string(command), deadline: deadline
    )
    .mapError(KeychainFailure.process)
    .flatMap { output in
      switch output.status {
      case .exited(0): .success(())
      case .exited(let code) where code == interactionNotAllowedStatus:
        .failure(.interactionNotAllowed)
      case .exited, .signaled: .failure(.denied)
      }
    }
  }

  static func deleteGenericPassword(
    service: String, account: String
  ) async -> Result<Void, KeychainFailure> {
    await ProcessRun.collect(
      ProcessInvocation(
        executable: tool,
        arguments: ["delete-generic-password", "-a", account, "-s", service],
        environment: [:], workingDirectory: Self.workingDirectory
      ), deadline: deadline
    )
    .mapError(KeychainFailure.process)
    .flatMap { output in
      switch output.status {
      case .exited(let code) where code == 0 || code == notFoundStatus: .success(())
      case .exited(let code) where code == interactionNotAllowedStatus:
        .failure(.interactionNotAllowed)
      case .exited, .signaled: .failure(.denied)
      }
    }
  }

  private static func exitStatus(of status: OSStatus) -> Int32 {
    Int32(bitPattern: UInt32(bitPattern: status) & 0x00FF_FFFF)
  }

  private static func decode(_ printed: String) -> Data {
    let text: String = printed.hasSuffix("\n") ? String(printed.dropLast()) : printed
    if text.contains(/^[0-9a-fA-F]+$/), text.count.isMultiple(of: 2),
      let bytes: Data = hexDecoded(text)
    {
      return bytes
    }
    return Data(text.utf8)
  }

  private static func hexDecoded(_ text: String) -> Data? {
    var bytes: Data = Data(capacity: text.count / 2)
    var index: String.Index = text.startIndex
    while index < text.endIndex {
      let next: String.Index = text.index(index, offsetBy: 2)
      guard let byte: UInt8 = UInt8(text[index..<next], radix: 16) else { return nil }
      bytes.append(byte)
      index = next
    }
    return bytes
  }

  private static func quoted(_ value: String) -> String {
    "\"" + value.replacing("\\", with: "\\\\").replacing("\"", with: "\\\"") + "\""
  }
}

nonisolated extension Sequence<UInt8> {
  var hexEncoded: String {
    map { byte in String(format: "%02x", byte) }.joined()
  }
}
