// --- [TYPES] ---------------------------------------------------------------------------

public enum Interference: Sendable, CustomStringConvertible {
    case activation
    case interface
    case menuAction
    case script

    public var description: String {
        switch self {
            case .activation: "activates or launches an application"
            case .interface: "changes windows with these arguments"
            case .menuAction: "runs a menu action"
            case .script: "runs a script"
        }
    }
}
