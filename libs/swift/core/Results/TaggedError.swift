// --- [TYPES] ---------------------------------------------------------------------------

public protocol TaggedError: Error, Encodable {
    static var tag: String { get }
}
