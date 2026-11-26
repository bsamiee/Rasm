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
    case quit
    case export
    case select
    case item
    case region
    case context
    case siblings
    case answer
    case progress
    case bleed
    case done
    case status
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
    public struct Shortcut: Codable, Equatable, Sendable {
        public let key: String
        public let modifiers: Lenient<NSEvent.ModifierFlags>
    }

    public struct Control: Codable, Equatable, Sendable {
        public let entry: Inventory
        public let member: Int
        public let role: String?
        public let subrole: String?
        public let parent: String?
        public let title: String?
        public let description: String?
        public let caption: String?
        public let help: String?
        public let value: JSONValue?
        public let enabled: Bool
        public let range: ClosedRange<Double>?
        public let step: Double?
        public let mark: Lenient<NSControl.StateValue>?
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
        public let defaultButton: Control?
        public let cancelButton: Control?
    }

    public let controls: [Control]
    public let windows: [Window]
}

public enum Command: Codable, Sendable {
    case report
    case show(surface: Surface)
    case dismiss(surface: Surface)
    case retitle(entry: Inventory, member: Int, to: String)
    case reorder(from: Int, to: Int)
    case fault(nonce: String)
    case hang(kind: Hang)
    case abort
}

@CasePathable
public enum Line: Codable, Sendable {
    case ready
    case report(report: Report)
    case action(selector: String, control: Report.Control?, cause: Lenient<NSEvent.EventType>?)
    case pointer(
        type: Lenient<NSEvent.EventType>,
        windowNumber: Int,
        locationInWindow: CGPoint,
        eventNumber: Int,
        clickCount: Int,
        pressure: Float,
        subtype: Lenient<NSEvent.EventSubtype>,
        tilt: CGPoint?,
        timestamp: TimeInterval,
        modifierFlags: Lenient<NSEvent.ModifierFlags>,
    )
    case moved(windowNumber: Int, locationInWindow: CGPoint, timestamp: TimeInterval, modifierFlags: Lenient<NSEvent.ModifierFlags>)
    case proximity(windowNumber: Int, isEnteringProximity: Bool, pointingDeviceType: Lenient<NSEvent.PointingDeviceType>, timestamp: TimeInterval)
    case scroll(
        windowNumber: Int,
        locationInWindow: CGPoint,
        scrollingDelta: CGVector,
        hasPreciseScrollingDeltas: Bool,
        phase: Lenient<NSEvent.Phase>,
        momentumPhase: Lenient<NSEvent.Phase>,
        timestamp: TimeInterval,
        modifierFlags: Lenient<NSEvent.ModifierFlags>,
    )
    case key(
        type: Lenient<NSEvent.EventType>,
        windowNumber: Int,
        characters: String?,
        keyCode: UInt16,
        isARepeat: Bool,
        timestamp: TimeInterval,
        modifierFlags: Lenient<NSEvent.ModifierFlags>,
        sourceProcess: pid_t?,
    )
    case menu(title: String, opened: Bool)
    case tracking(title: String, began: Bool)
    case answered(surface: Surface, accepted: Bool)
    case active(active: Bool)
    case keyed(window: Int, key: Bool)
}
