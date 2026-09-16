import CoreGraphics
import Foundation

nonisolated struct Window: Codable {
  struct Bounds: Encodable {
    let x: Double
    let y: Double
    let w: Double
    let h: Double
  }

  let windowId: CGWindowID
  let ownerPid: pid_t
  let name: String?
  let layer: CGWindowLevel
  let bounds: Bounds

  private enum WindowServerKey: String, CodingKey {
    case windowId = "kCGWindowNumber"
    case ownerPid = "kCGWindowOwnerPID"
    case name = "kCGWindowName"
    case layer = "kCGWindowLayer"
    case bounds = "kCGWindowBounds"
  }

  init(from decoder: any Decoder) throws {
    let entry: KeyedDecodingContainer<WindowServerKey> = try decoder.container(
      keyedBy: WindowServerKey.self)
    let representation: [String: Double] = try entry.decode([String: Double].self, forKey: .bounds)
    guard let rect: CGRect = CGRect(dictionaryRepresentation: representation as CFDictionary) else {
      throw DecodingError.dataCorruptedError(
        forKey: .bounds, in: entry, debugDescription: "not a CGRect dictionary representation")
    }
    windowId = try entry.decode(CGWindowID.self, forKey: .windowId)
    ownerPid = try entry.decode(pid_t.self, forKey: .ownerPid)
    name = try entry.decodeIfPresent(String.self, forKey: .name)
    layer = try entry.decode(CGWindowLevel.self, forKey: .layer)
    bounds = Bounds(x: rect.minX, y: rect.minY, w: rect.width, h: rect.height)
  }
}

nonisolated enum WindowListFailure: Error {
  case usage
  case noWindowServer
}

@main
enum WindowList {
  nonisolated static func windows(of pid: pid_t) throws -> Result<[Window], WindowListFailure> {
    try CGWindowListCopyWindowInfo([.optionOnScreenOnly, .excludeDesktopElements], kCGNullWindowID)
      .map { list in
        .success(
          try PropertyListDecoder().decode(
            [Window].self,
            from: PropertyListSerialization.data(
              fromPropertyList: list, format: .binary, options: 0)
          )
          .filter { window in window.ownerPid == pid })
      } ?? .failure(.noWindowServer)
  }

  static func main() throws {
    let reading: Result<[Window], WindowListFailure> =
      try CommandLine.arguments.dropFirst().first.flatMap(pid_t.init).map(windows(of:))
      ?? .failure(.usage)
    switch reading {
    case .success(let windows):
      try FileHandle.standardOutput.write(
        contentsOf: JSONEncoder().encode(windows) + Data("\n".utf8))
    case .failure(.usage):
      try FileHandle.standardError.write(contentsOf: Data("usage: WindowList <pid>\n".utf8))
      exit(2)
    case .failure(.noWindowServer):
      try FileHandle.standardError.write(
        contentsOf: Data("WindowList: no window server in this session\n".utf8))
      exit(1)
    }
  }
}
