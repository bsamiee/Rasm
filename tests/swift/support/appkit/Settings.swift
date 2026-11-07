public import Foundation

// --- [MODELS] --------------------------------------------------------------------------

public enum Scenario: String, CaseIterable, Sendable {
    case slowLaunch
    case launchModal
    case singleInstance
    case window
    case undescribed
    case collapsed
    case swatch
    case table
    case sheet
    case modal
    case saveSheet
    case systemDialog
    case floatingPanel
    case hidingPanel
    case edited
    case minimize
    case hangBlocked
    case hangBusy
    case hangQuiet
    case hangWindow
    case modalBlocked
    case stop
    case fault
    case abort
    case exit
    case helper
    case terminate

    public var atLaunch: Bool {
        switch self {
            case .slowLaunch, .launchModal, .singleInstance: true
            case .window, .undescribed, .collapsed, .swatch, .table, .sheet, .modal, .saveSheet, .systemDialog, .floatingPanel, .hidingPanel, .edited, .minimize, .hangBlocked, .hangBusy, .hangQuiet,
                .hangWindow, .modalBlocked, .stop, .fault, .abort, .exit, .helper, .terminate:
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
        case role = "FIXTURE_ROLE"
        case scenario = "FIXTURE_SCENARIO"
        case windows = "FIXTURE_WINDOWS"
        case document = "FIXTURE_DOCUMENT"
        case edited = "FIXTURE_EDITED"
        case status = "FIXTURE_STATUS"
        case owner = "FIXTURE_OWNER"
    }

    public let role: Role
    public let scenario: Scenario?
    public let windows: Int
    public let document: URL?
    public let edited: Bool
    public let status: Int32
    public let owner: pid_t?

    public init(
        role: Role = .application,
        scenario: Scenario? = nil,
        windows: Int = 1,
        document: URL? = nil,
        edited: Bool = false,
        status: Int32 = 0,
        owner: pid_t? = getpid(),
    ) {
        self.role = role
        self.scenario = scenario
        self.windows = windows
        self.document = document
        self.edited = edited
        self.status = status
        self.owner = owner
    }

    init(reading environment: [String: String]) {
        func text(_ key: Key) -> String? { environment[key.rawValue] }
        let fallback: Self = Self(owner: nil)
        self.init(
            role: text(.role).flatMap(Role.init(rawValue:)) ?? fallback.role,
            scenario: text(.scenario).flatMap(Scenario.init(rawValue:)),
            windows: text(.windows).flatMap { windows in Int(windows) } ?? fallback.windows,
            document: text(.document).map { path in URL(filePath: path) },
            edited: text(.edited).flatMap { edited in Bool(edited) } ?? fallback.edited,
            status: text(.status).flatMap { status in Int32(status) } ?? fallback.status,
            owner: text(.owner).flatMap { owner in pid_t(owner) },
        )
    }

    public var environment: [String: String] {
        let values: [(key: Key, value: String?)] = [
            (.role, role.rawValue), (.scenario, scenario?.rawValue), (.windows, String(windows)), (.document, document?.path(percentEncoded: false)), (.edited, String(edited)),
            (.status, String(status)), (.owner, owner.map { owner in String(owner) }),
        ]
        return Dictionary(uniqueKeysWithValues: values.compactMap { key, value in value.map { text in (key.rawValue, text) } })
    }
}
