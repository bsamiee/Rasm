import Applications
import ArgumentParser
import Foundation

// --- [COMPOSITION] ---------------------------------------------------------------------

struct Target: ParsableArguments {
    @Argument(
        help: "Element ID from read or watch line",
        transform: { id in
            guard let element: Data = Data(base64Encoded: id) else { throw ValidationError("\(id) is no element ID") }
            return element
        },
    )
    var element: Data

    func run(_ action: ElementAction) async throws {
        try await WatchApps.run { output in
            do {
                try await action.perform()
            } catch {
                output.yield((action.element.base64EncodedString(), .fail(reason: error.localizedDescription)))
            }
        }
    }
}

struct Perform: AsyncParsableCommand {
    static let configuration: CommandConfiguration = CommandConfiguration(
        abstract: "Performs one element Accessibility action, silent unless it fails"
    )

    @OptionGroup var target: Target

    @Argument(help: "Accessibility action from element actions")
    var action: String

    func run() async throws {
        try await target.run(.perform(element: target.element, action: action))
    }
}

struct Set: AsyncParsableCommand {
    static let configuration: CommandConfiguration = CommandConfiguration(
        abstract: "Sets element AXValue to text, silent unless write fails"
    )

    @OptionGroup var target: Target

    @Argument(help: "Text for element AXValue")
    var value: String

    func run() async throws {
        try await target.run(.set(element: target.element, value: value))
    }
}
