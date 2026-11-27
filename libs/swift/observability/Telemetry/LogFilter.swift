package import Logging
import Synchronization

// --- [SERVICES] ------------------------------------------------------------------------

package final class LogFilter: Sendable {
    package struct Rules: Sendable {
        package var level: Logger.Level
        var others: Logger.Level
        var scopes: [String: Logger.Level]

        package init(level: Logger.Level = .info, others: Logger.Level = .warning, scopes: [String: Logger.Level] = [:]) { (self.level, self.others, self.scopes) = (level, others, scopes) }
    }

    private let roots: Set<Substring>
    private let state: Mutex<(rules: Rules, resolved: [String: Logger.Level])>

    package init(roots: [Any.Type], rules: Rules = .init()) {
        self.roots = Set(roots.map { root in String(reflecting: root).prefix { character in character != "." } })
        state = Mutex((rules, [:]))
    }

    package var rules: Rules {
        get { state.withLock { state in state.rules } }
        set { state.withLock { state in state = (newValue, [:]) } }
    }

    package func level(for scope: String) -> Logger.Level {
        state.withLock { state in
            if let resolved = state.resolved[scope] { return resolved }
            let level =
                sequence(first: scope[...]) { name in name.lastIndex(of: ".").map { end in name[..<end] } }.lazy
                .compactMap { (name: Substring) -> Logger.Level? in state.rules.scopes[String(name)] ?? (roots.contains(name) ? state.rules.level : nil) }.first ?? state.rules.others
            state.resolved[scope] = level
            return level
        }
    }
}
