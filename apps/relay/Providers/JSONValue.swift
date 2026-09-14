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
    self = try container.decodeNil() ? .null : Self.value(in: container)
  }

  private static func value(in container: any SingleValueDecodingContainer) throws -> JSONValue {
    let first: Result<JSONValue, any Error> = Result { try .bool(container.decode(Bool.self)) }
    let candidates: [() throws -> JSONValue] = [
      { try .number(container.decode(Double.self)) },
      { try .string(container.decode(String.self)) },
      { try .array(container.decode([JSONValue].self)) },
      { try .object(container.decode([String: JSONValue].self)) },
    ]
    return try candidates.reduce(first) { outcome, candidate in
      outcome.flatMapError { _ in Result(catching: candidate) }
    }
    .get()
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
