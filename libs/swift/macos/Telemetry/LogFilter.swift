public import Logging
import Synchronization

// --- [SERVICES] ------------------------------------------------------------------------

public final class LogFilter: Sendable {
    public struct Rules: Sendable {
        public var level: Logger.Level
        public var others: Logger.Level
        public var scopes: [String: Logger.Level]

        public init(level: Logger.Level = .info, others: Logger.Level = .warning, scopes: [String: Logger.Level] = [:]) { (self.level, self.others, self.scopes) = (level, others, scopes) }
    }

    private let roots: Set<Substring>
    private let state: Mutex<(rules: Rules, resolved: [String: Logger.Level])>

    public init(roots: [Any.Type], rules: Rules = .init()) {
        self.roots = Set(roots.map { root in String(reflecting: root).prefix { character in character != "." } })
        state = Mutex((rules, [:]))
    }

    public var rules: Rules {
        get { state.withLock { state in state.rules } }
        set { state.withLock { state in state = (newValue, [:]) } }
    }

    public func level(for scope: String) -> Logger.Level {
        state.withLock { state in
            if let resolved = state.resolved[scope] { return resolved }
            let stated = sequence(first: scope[...]) { name in name.lastIndex(of: ".").map { end in name[..<end] } }.lazy.compactMap { name in state.rules.scopes[String(name)] }.first
            let level = stated ?? (roots.contains(scope.prefix { character in character != "." }) ? state.rules.level : state.rules.others)
            state.resolved[scope] = level
            return level
        }
    }
}

public struct OnceLogHandler: LogHandler {
    private final class Causes: Sendable {
        let seen: Mutex<Set<String>> = .init([])
    }

    public var metadata: Logger.Metadata = [:]
    public var metadataProvider: Logger.MetadataProvider?
    public var logLevel: Logger.Level = .trace
    private let handler: any LogHandler
    private let causes: Causes = .init()

    public init(_ handler: any LogHandler) { self.handler = handler }

    public subscript(metadataKey key: String) -> Logger.Metadata.Value? {
        get { metadata[key] }
        set { metadata[key] = newValue }
    }

    public func log(event: LogEvent) {
        let cause = event.error.map { error in String(reflecting: type(of: error)) } ?? event.message.description
        guard handler.logLevel <= event.level, causes.seen.withLock({ seen in seen.insert(cause).inserted }) else { return }
        var first = event
        first.metadata = attributes(of: event)
        handler.log(event: first)
    }
}
