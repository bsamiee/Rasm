import CoreGraphics
import CoreText
import CryptoKit
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

nonisolated struct RegisteredFont: Encodable {
  struct Axis: Encodable {
    let id: UInt32
    let value: Double
  }

  let postScriptName: String
  let file: String
  let axes: [Axis]
  let nameTableDigest: String

  static func read(_ descriptor: CTFontDescriptor, at index: Int) -> Result<Self, NativeFailure> {
    guard let expectedName = CTFontDescriptorCopyAttribute(descriptor, kCTFontNameAttribute) as? String else {
      return .failure(.fontAttribute(index: index, attribute: .postScriptName))
    }
    guard let expectedFile = CTFontDescriptorCopyAttribute(descriptor, kCTFontURLAttribute) as? URL, expectedFile.isFileURL else {
      return .failure(.fontAttribute(index: index, attribute: .file))
    }
    let font = CTFontCreateWithFontDescriptorAndOptions(descriptor, 0, nil, [.preventAutoActivation, .preventAutoDownload])
    let name = CTFontCopyPostScriptName(font) as String
    guard let file = CTFontCopyAttribute(font, kCTFontURLAttribute) as? URL, file.isFileURL else {
      return .failure(.fontAttribute(index: index, attribute: .file))
    }
    guard name == expectedName, file == expectedFile else {
      return .failure(.fontSubstitution(index: index, expectedName: expectedName, actualName: name, expectedFile: expectedFile.path, actualFile: file.path))
    }
    guard let table = CTFontCopyTable(font, CTFontTableTag(kCTFontTableName), []) else {
      return .failure(.fontAttribute(index: index, attribute: .nameTable))
    }
    let values: [NSNumber: NSNumber]
    if let variation = CTFontCopyVariation(font) {
      guard let coordinates = variation as? [NSNumber: NSNumber] else {
        return .failure(.fontAttribute(index: index, attribute: .axes))
      }
      values = coordinates
    } else {
      values = [:]
    }
    let axes = values.map { Axis(id: $0.key.uint32Value, value: $0.value.doubleValue) }.sorted { $0.id < $1.id }
    let digest = SHA256.hash(data: table as Data).map { String(format: "%02x", $0) }.joined()
    return .success(Self(postScriptName: name, file: file.path, axes: axes, nameTableDigest: digest))
  }
}

nonisolated struct FontRegistry: Encodable {
  var accepted: [RegisteredFont]
  var rejected: [NativeFailure]
}

nonisolated enum NativeFailure: Error, Encodable, CustomStringConvertible {
  enum FontAttribute: String, Encodable {
    case postScriptName
    case file
    case axes
    case nameTable
  }

  case usage
  case noWindowServer
  case noFontRegistry
  case fontAttribute(index: Int, attribute: FontAttribute)
  case fontSubstitution(index: Int, expectedName: String, actualName: String, expectedFile: String, actualFile: String)
  case propertyList(String)
  case decode(String)
  case encode(String)
  case write(String)

  var description: String {
    switch self {
    case .usage: "Usage: NativeHost fonts | windows <pid>"
    case .noWindowServer: "Window server is unavailable in session"
    case .noFontRegistry: "Font registry is unavailable in session"
    case .fontAttribute(let index, let attribute): "Font registration \(index) has no usable \(attribute.rawValue)"
    case .fontSubstitution(let index, let expectedName, let actualName, let expectedFile, let actualFile):
      "Font registration \(index) substituted \(expectedName) at \(expectedFile) with \(actualName) at \(actualFile)"
    case .propertyList(let error): "Window list serialization failed: \(error)"
    case .decode(let error): "Window list decoding failed: \(error)"
    case .encode(let error): "JSON encoding failed: \(error)"
    case .write(let error): "Standard output write failed: \(error)"
    }
  }

  var exitCode: Int32 {
    switch self {
    case .usage: 2
    case .noWindowServer, .noFontRegistry, .fontAttribute, .fontSubstitution, .propertyList, .decode, .encode, .write: 1
    }
  }
}

@main
enum NativeHost {
  nonisolated static func windows(of pid: pid_t) -> Result<[Window], NativeFailure> {
    CGWindowListCopyWindowInfo([.optionOnScreenOnly, .excludeDesktopElements], kCGNullWindowID)
      .map { list in
        Result {
          try PropertyListSerialization.data(fromPropertyList: list, format: .binary, options: 0)
        }
        .mapError { NativeFailure.propertyList($0.localizedDescription) }
        .flatMap { data in
          Result { try PropertyListDecoder().decode([Window].self, from: data) }
            .mapError { NativeFailure.decode($0.localizedDescription) }
        }
        .map { windows in windows.filter { window in window.ownerPid == pid } }
      } ?? .failure(.noWindowServer)
  }

  nonisolated static func fonts() -> Result<FontRegistry, NativeFailure> {
    let collection = CTFontCollectionCreateFromAvailableFonts([kCTFontCollectionDisallowAutoActivationOption: true] as CFDictionary)
    guard let descriptors = CTFontCollectionCreateMatchingFontDescriptors(collection) as? [CTFontDescriptor] else {
      return .failure(.noFontRegistry)
    }
    let readings = descriptors.enumerated().map { RegisteredFont.read($0.element, at: $0.offset) }
    let registry = readings.reduce(into: FontRegistry(accepted: [], rejected: [])) { registry, reading in
      switch reading {
      case .success(let font): registry.accepted.append(font)
      case .failure(let failure): registry.rejected.append(failure)
      }
    }
    return .success(registry)
  }

  nonisolated static func emit<Value: Encodable>(_ value: Value) -> Result<Void, NativeFailure> {
    let encoder = JSONEncoder()
    encoder.outputFormatting = .sortedKeys
    return Result { try encoder.encode(value) }
      .mapError { NativeFailure.encode($0.localizedDescription) }
      .flatMap { json in
        Result { try FileHandle.standardOutput.write(contentsOf: json + Data("\n".utf8)) }
          .mapError { NativeFailure.write($0.localizedDescription) }
      }
  }

  static func main() {
    let arguments = CommandLine.arguments.dropFirst()
    let result: Result<Void, NativeFailure> =
      switch (arguments.first, arguments.count) {
      case ("fonts", 1): fonts().flatMap(emit)
      case ("windows", 2): arguments.last.flatMap(pid_t.init).map(windows(of:))?.flatMap(emit) ?? .failure(.usage)
      default: .failure(.usage)
      }
    switch result {
    case .success:
      return
    case .failure(let failure):
      fputs("\(failure)\n", stderr)
      exit(failure.exitCode)
    }
  }
}
