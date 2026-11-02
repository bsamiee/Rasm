public import Foundation

// --- [MODELS] --------------------------------------------------------------------------

public enum Scenario: String, CaseIterable, Sendable {
    case slowLaunch
    case launchModal
    case singleInstance
    case window
    case swatch
    case table
    case sheet
    case modal
    case saveSheet
    case systemDialog
    case floatingPanel
    case edited
    case minimize
    case hangBlocked
    case hangBusy
    case hangQuiet
    case stop
    case signal
    case fault
    case trap
    case abort
    case exit
    case helper
    case terminate

    public var atLaunch: Bool {
        switch self {
            case .slowLaunch, .launchModal, .singleInstance: true
            case .window, .swatch, .table, .sheet, .modal, .saveSheet, .systemDialog, .floatingPanel, .edited, .minimize, .hangBlocked, .hangBusy, .hangQuiet, .stop, .signal, .fault, .trap,
                .abort, .exit, .helper, .terminate:
                false
        }
    }
}

public enum Role: String, Sendable {
    case application
    case helper
}

public struct Settings: Sendable {
    private enum Key: String {
        case role
        case scenario
        case windows
        case document
        case edited
        case hidden
        case duration
        case status
        case owner
    }

    public let role: Role
    public let scenario: Scenario?
    public let windows: Int
    public let document: URL?
    public let edited: Bool
    public let hidden: Bool
    public let duration: Duration
    public let status: Int32
    public let owner: pid_t?

    public init(
        role: Role = .application,
        scenario: Scenario? = nil,
        windows: Int = 1,
        document: URL? = nil,
        edited: Bool = false,
        hidden: Bool = false,
        duration: Duration = .seconds(2),
        status: Int32 = 0,
        owner: pid_t? = getpid(),
    ) {
        self.role = role
        self.scenario = scenario
        self.windows = windows
        self.document = document
        self.edited = edited
        self.hidden = hidden
        self.duration = duration
        self.status = status
        self.owner = owner
    }

    init(defaults: UserDefaults) {
        func text(_ key: Key) -> String? { defaults.string(forKey: key.rawValue) }
        let fallback: Self = Self(owner: nil)
        self.init(
            role: text(.role).flatMap(Role.init(rawValue:)) ?? fallback.role,
            scenario: text(.scenario).flatMap(Scenario.init(rawValue:)),
            windows: text(.windows).flatMap { windows in Int(windows) } ?? fallback.windows,
            document: text(.document).map { path in URL(filePath: path) },
            edited: text(.edited).flatMap { edited in Bool(edited) } ?? fallback.edited,
            hidden: text(.hidden).flatMap { hidden in Bool(hidden) } ?? fallback.hidden,
            duration: text(.duration).flatMap { seconds in Double(seconds) }.map(Duration.seconds) ?? fallback.duration,
            status: text(.status).flatMap { status in Int32(status) } ?? fallback.status,
            owner: text(.owner).flatMap { owner in pid_t(owner) },
        )
    }

    public var arguments: [String] {
        let values: [(key: Key, value: String?)] = [
            (.role, role.rawValue), (.scenario, scenario?.rawValue), (.windows, String(windows)), (.document, document?.path(percentEncoded: false)), (.edited, String(edited)),
            (.hidden, String(hidden)), (.duration, String(duration / .seconds(1))), (.status, String(status)), (.owner, owner.map { owner in String(owner) }),
        ]
        return values.flatMap { key, value in value.map { text in ["-\(key.rawValue)", text] } ?? [] }
    }
}
