package import AppKit
package import CasePaths
package import Encoding

// --- [TYPES] ---------------------------------------------------------------------------

package enum Inventory: String, CaseIterable, Codable, Sendable {
    case action
    case toggle
    case popup
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
    case item
    case region
    case context
    case siblings
    case progress
    case bleed
    case done
    case status
    case drawer
}

package enum Surface: String, CaseIterable, Codable, Sendable {
    case controls
    case lists
    case covered
    case unlisted
    case toolbar
    case progress
    case panel
    case sheet
    case save
    case popover
    case status
    case drawer
}

// --- [MODELS] --------------------------------------------------------------------------

package struct Report: Codable, Sendable {
    package struct Shortcut: Codable, Sendable {
        package let key: String
        package let modifiers: Lenient<NSEvent.ModifierFlags>
    }

    package struct Control: Codable, Sendable {
        package let entry: Inventory
        package let member: Int
        package let role: String?
        package let subrole: String?
        package let parent: String?
        package let title: String?
        package let description: String?
        package let caption: String?
        package let inputs: [String]?
        package let help: String?
        package let value: JSONValue?
        package let shortcut: Shortcut?
        package let window: Int?
        package let frame: CGRect?
        package let customActions: [String]?
        package let columns: [String]?
        package let rows: Int?
    }

    package struct Window: Codable, Sendable {
        package let surface: Surface?
        package let number: Int
        package let parent: Int?
        package let frame: CGRect
        package let visible: Bool
        package let edited: Bool
    }

    package let controls: [Control]
    package let windows: [Window]
}

package enum Command: Codable, Sendable {
    case report
    case show(surface: Surface)
    case raise(surface: Surface)
    case dismiss(surface: Surface)
    case retitle(to: String)
    case select(segment: Int)
    case reorder(from: Int, to: Int)
    case sleep(for: Duration)
    case spinOnTermination(for: Duration)
    case edit
}

@CasePathable
package enum Line: Codable, Sendable {
    case ready
    case sleeping
    case report(report: Report)
    case key(sourceProcess: pid_t?)
    case menu(title: String, opened: Bool)
    case tracking(title: String, began: Bool)
    case active(active: Bool)
}
