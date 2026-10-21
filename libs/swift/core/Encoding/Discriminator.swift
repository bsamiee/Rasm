public import Foundation

// --- [MODELS] --------------------------------------------------------------------------

public struct Discriminator: CodingKey {
    private static let key: CodingUserInfoKey? = CodingUserInfoKey(rawValue: "Discriminator")

    public let stringValue: String
    public var intValue: Int? { nil }
    public var userInfo: [CodingUserInfoKey: any Sendable] { Self.key.map { key in [key: self] } ?? [:] }

    public init(stringValue: String) { self.stringValue = stringValue }
    public init?(intValue _: Int) { nil }

    public init(_ userInfo: [CodingUserInfoKey: Any]) throws(CocoaError) {
        guard let carried: Self = Self.key.flatMap({ key in userInfo[key] }) as? Self else { throw CocoaError(.coderValueNotFound) }
        self = carried
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension Encoder {
    public func encode<Key: CodingKey>(case name: some Encodable, keyedBy _: Key.Type, _ fill: (inout KeyedEncodingContainer<Key>) throws -> Void = { _ in }) throws {
        try [Discriminator(userInfo).stringValue: name].encode(to: self)
        var fields: KeyedEncodingContainer<Key> = container(keyedBy: Key.self)
        try fill(&fields)
    }
}
