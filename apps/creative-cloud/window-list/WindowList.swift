import CoreGraphics
import Foundation

nonisolated struct Window: Codable {
  struct Bounds: Codable {
    let x: Double
    let y: Double
    let width: Double
    let height: Double

    enum CodingKeys: String, CodingKey {
      case x = "X"
      case y = "Y"
      case width = "Width"
      case height = "Height"
    }
  }

  let windowId: CGWindowID
  let ownerPid: pid_t
  let name: String?
  let layer: CGWindowLevel
  let bounds: Bounds

  enum CodingKeys: String, CodingKey {
    case windowId = "kCGWindowNumber"
    case ownerPid = "kCGWindowOwnerPID"
    case name = "kCGWindowName"
    case layer = "kCGWindowLayer"
    case bounds = "kCGWindowBounds"
  }
}

nonisolated enum WindowListFailure: Error, CustomStringConvertible {
  case usage
  case noWindowServer
  case propertyList(any Error)
  case decode(any Error)
  case encode(any Error)
  case write(any Error)

  var description: String {
    switch self {
    case .usage: "usage: WindowList <pid>"
    case .noWindowServer: "WindowList: no window server in this session"
    case .propertyList(let error): "WindowList: window list is not a property list: \(error)"
    case .decode(let error): "WindowList: window dictionary rejected: \(error)"
    case .encode(let error): "WindowList: JSON encoding failed: \(error)"
    case .write(let error): "WindowList: standard output write failed: \(error)"
    }
  }

  var exitCode: Int32 {
    switch self {
    case .usage: 2
    case .noWindowServer, .propertyList, .decode, .encode, .write: 1
    }
  }
}

@main
enum WindowList {
  nonisolated static func windows(of pid: pid_t) -> Result<[Window], WindowListFailure> {
    CGWindowListCopyWindowInfo([.optionOnScreenOnly, .excludeDesktopElements], kCGNullWindowID)
      .map { list in
        Result {
          try PropertyListSerialization.data(fromPropertyList: list, format: .binary, options: 0)
        }
        .mapError(WindowListFailure.propertyList)
        .flatMap { data in
          Result { try PropertyListDecoder().decode([Window].self, from: data) }
            .mapError(WindowListFailure.decode)
        }
        .map { windows in windows.filter { window in window.ownerPid == pid } }
      } ?? .failure(.noWindowServer)
  }

  nonisolated static func emit(_ windows: [Window]) -> Result<Void, WindowListFailure> {
    let encoder = JSONEncoder()
    encoder.outputFormatting = .sortedKeys
    return Result { try encoder.encode(windows) }
      .mapError(WindowListFailure.encode)
      .flatMap { json in
        Result { try FileHandle.standardOutput.write(contentsOf: json + Data("\n".utf8)) }
          .mapError(WindowListFailure.write)
      }
  }

  static func main() {
    let reading: Result<[Window], WindowListFailure> =
      CommandLine.arguments.dropFirst().first.flatMap(pid_t.init).map(windows(of:))
      ?? .failure(.usage)
    switch reading.flatMap(emit) {
    case .success:
      return
    case .failure(let failure):
      fputs("\(failure)\n", stderr)
      exit(failure.exitCode)
    }
  }
}
