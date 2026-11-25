public import AppKit
public import CasePaths
public import Encoding

// --- [TYPES] ---------------------------------------------------------------------------

public enum Inventory: String, CaseIterable, Codable, Sendable {
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
    case controls
    case lists
    case covered
    case unlisted
    case toolbar
    case dialog
    case progress
    case panel
    case sheet
    case save
    case popover
    case status
}

public enum Hang: Codable, Sendable {
    case sleeping(Duration)
    case spinning(Duration)
}

// --- [MODELS] --------------------------------------------------------------------------

public struct Report: Codable, Equatable, Sendable {
    public struct Item: Codable, Equatable, Sendable {
        public let title: String
        public let selected: Bool
        public let level: Int?
    }

    public struct Shortcut: Codable, Equatable, Sendable {
        public let key: String
        public let modifiers: UInt
    }

    public struct Control: Codable, Equatable, Sendable {
        public let entry: Inventory
        public let member: Int
        public let className: String
        public let role: String?
        public let subrole: String?
        public let parent: String?
        public let title: String?
        public let caption: String?
        public let help: String?
        public let value: JSONValue?
        public let enabled: Bool
        public let range: ClosedRange<Double>?
        public let step: Double?
        public let items: [Item]?
        public let shortcut: Shortcut?
        public let window: Int?
        public let frame: CGRect?
    }

    public struct Window: Codable, Equatable, Sendable {
        public let surface: Surface?
        public let number: Int
        public let className: String
        public let title: String
        public let role: String?
        public let subrole: String?
        public let modal: Bool
        public let frame: CGRect
        public let level: Int
        public let visible: Bool
        public let key: Bool
        public let main: Bool
        public let edited: Bool
    }

    public let controls: [Control]
    public let windows: [Window]
}

public enum Command: Codable, Sendable {
    case report
    case show(Surface)
    case dismiss(Surface)
    case retitle(Inventory, member: Int, to: String)
    case reorder(Inventory, to: Int)
    case fault(String)
    case hang(Hang)
    case activate
    case abort
}

@CasePathable
public enum Line: Codable, Sendable {
    case ready
    case report(report: Report)
    case action(selector: String, entry: Inventory?, member: Int?, value: JSONValue?, cause: UInt?)
    case pointer(type: UInt, window: Int, at: CGPoint, clicks: Int, modifiers: UInt)
    case key(type: UInt, window: Int, characters: String?, code: UInt16, modifiers: UInt)
    case scroll(window: Int, at: CGPoint, delta: CGVector, modifiers: UInt)
    case menu(title: String, opened: Bool, tracking: Bool)
    case answered(surface: Surface, accepted: Bool)
    case active(active: Bool)
    case keyed(window: Int, key: Bool)
}
