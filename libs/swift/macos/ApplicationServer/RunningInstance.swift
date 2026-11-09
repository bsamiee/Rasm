public import AppKit
public import Applications

// --- [MODELS] --------------------------------------------------------------------------

public struct RunningInstance: Encodable, Sendable {
    private enum Key: CodingKey {
        case pid, identifier, url, activationPolicy, state
    }

    public let instance: NSRunningApplication
    public let identifier: String
    public let url: URL
    public let state: InstanceState

    public init?(_ instance: NSRunningApplication, state: InstanceState) {
        guard let url: URL = instance.bundleURL, let identifier: String = instance.bundleIdentifier else { return nil }
        (self.instance, self.identifier, self.url, self.state) = (instance, identifier, url, state)
    }

    public func encode(to encoder: any Encoder) throws {
        var fields: KeyedEncodingContainer<Key> = encoder.container(keyedBy: Key.self)
        try fields.encode(instance.processIdentifier, forKey: .pid)
        try fields.encode(identifier, forKey: .identifier)
        try fields.encode(url, forKey: .url)
        try fields.encode(instance.activationPolicy.name, forKey: .activationPolicy)
        try fields.encode(state, forKey: .state)
    }
}
