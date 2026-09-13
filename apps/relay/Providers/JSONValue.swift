import Foundation

nonisolated enum JSONValue: Codable, Equatable, Sendable {
  case null
  case bool(Bool)
  case number(Double)
  case string(String)
  case array([JSONValue])
  case object([String: JSONValue])

  init(from decoder: any Decoder) throws {
    let container: any SingleValueDecodingContainer = try decoder.singleValueContainer()
    if container.decodeNil() {
      self = .null
    } else if let value: Bool = try? container.decode(Bool.self) {
      self = .bool(value)
    } else if let value: Double = try? container.decode(Double.self) {
      self = .number(value)
    } else if let value: String = try? container.decode(String.self) {
      self = .string(value)
    } else if let value: [JSONValue] = try? container.decode([JSONValue].self) {
      self = .array(value)
    } else {
      self = .object(try container.decode([String: JSONValue].self))
    }
  }

  func encode(to encoder: any Encoder) throws {
    var container: any SingleValueEncodingContainer = encoder.singleValueContainer()
    switch self {
    case .null: try container.encodeNil()
    case .bool(let value): try container.encode(value)
    case .number(let value): try container.encode(value)
    case .string(let value): try container.encode(value)
    case .array(let value): try container.encode(value)
    case .object(let value): try container.encode(value)
    }
  }

  subscript(_ key: String) -> JSONValue? {
    objectValue?[key]
  }

  var stringValue: String? {
    switch self {
    case .string(let value): value
    default: nil
    }
  }

  var boolValue: Bool? {
    switch self {
    case .bool(let value): value
    default: nil
    }
  }

  var intValue: Int? {
    switch self {
    case .number(let value): Int(exactly: value)
    default: nil
    }
  }

  var doubleValue: Double? {
    switch self {
    case .number(let value): value
    default: nil
    }
  }

  var arrayValue: [JSONValue]? {
    switch self {
    case .array(let value): value
    default: nil
    }
  }

  var objectValue: [String: JSONValue]? {
    switch self {
    case .object(let value): value
    default: nil
    }
  }
}
