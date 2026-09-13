import Foundation

enum JSONValue: Codable, Equatable, Sendable {
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
      return
    }
    do {
      self = .bool(try container.decode(Bool.self))
      return
    } catch DecodingError.typeMismatch(_, _) {}
    do {
      self = .number(try container.decode(Double.self))
      return
    } catch DecodingError.typeMismatch(_, _) {}
    do {
      self = .string(try container.decode(String.self))
      return
    } catch DecodingError.typeMismatch(_, _) {}
    do {
      self = .array(try container.decode([JSONValue].self))
      return
    } catch DecodingError.typeMismatch(_, _) {}
    self = .object(try container.decode([String: JSONValue].self))
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

  subscript(_ index: Int) -> JSONValue? {
    guard case .array(let values) = self, values.indices.contains(index) else { return nil }
    return values[index]
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
