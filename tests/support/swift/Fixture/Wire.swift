public import AppKit
public import CasePaths
public import Encoding

// --- [TYPES] ---------------------------------------------------------------------------

public enum Control: String, CaseIterable, Codable, Sendable {
    case action
    case toggle
    case popup
    case deferred
    case combo
    case radio
    case tabs
    case segments
    case tools
    case table
    case outline
    case row
    case slider
    case stepper
    case measure
    case text
    case search
    case secure
    case color
    case date
    case item
    case region
    case siblings
}

public enum Surface: String, CaseIterable, Codable, Sendable {
    case dialog
    case panel
    case sheet
    case popover
    case context
    case deferred
    case grouped
    case status
}

public enum Window: String, CaseIterable, Codable, Sendable {
    case controls
    case lists
    case covered
}

// --- [MODELS] --------------------------------------------------------------------------

public struct Fact: Codable, Equatable, Sendable {
    public struct Shortcut: Codable, Equatable, Sendable {
        public let key: String
        public let modifiers: UInt
    }

    public let control: Control
    public let member: Int
    public let className: String
    public let elementClass: String
    public let element: Bool
    public let role: String?
    public let subrole: String?
    public let title: String?
    public let label: String?
    public let help: String?
    public let value: JSONValue?
    public let enabled: Bool
    public let range: ClosedRange<Double>?
    public let step: Double?
    public let options: [String]?
    public let selection: [Int]?
    public let shortcut: Shortcut?
    public let window: Int?
    public let frame: CGRect?
}

public struct WindowFact: Codable, Equatable, Sendable {
    public let window: Window?
    public let number: Int
    public let className: String
    public let title: String
    public let role: String?
    public let subrole: String?
    public let frame: CGRect
    public let level: Int
    public let visible: Bool
    public let key: Bool
    public let main: Bool
}

public struct Report: Codable, Equatable, Sendable {
    public let controls: [Fact]
    public let windows: [WindowFact]
}

public enum Command: Codable, Sendable {
    case report
    case show(Surface)
    case dismiss(Surface)
    case close(Window)
    case open(Window)
    case retitle(Control, member: Int, to: String)
    case reorder(Control, to: Int)
    case fault(String)
    case hang(Duration)
    case activate
    case abort
}

@CasePathable
public enum Line: Codable, Sendable {
    case ready
    case report(Report)
    case action(selector: String, control: Control?, member: Int?, value: JSONValue?)
    case pointer(type: UInt, window: Int, at: CGPoint, clicks: Int, modifiers: UInt)
    case key(type: UInt, window: Int, characters: String?, code: UInt16, modifiers: UInt)
    case scroll(window: Int, at: CGPoint, delta: CGVector, modifiers: UInt)
    case active(Bool)
    case keyed(window: Int, Bool)
    case menu(title: String, open: Bool)
}
