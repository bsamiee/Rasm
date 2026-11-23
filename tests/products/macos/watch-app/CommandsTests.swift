import ArgumentParser
import CustomDump
import ScreenCapture
import Testing

@testable import watch_app

// --- [CONSTANTS] -----------------------------------------------------------------------

private let commands: [[String]] = [["capture", "Finder"], ["watch", "Finder", "--record", "/private/tmp/watch-app-record"]]
private let sides: [(given: String, accepted: Int?)] = [("0", nil), ("-1", nil), ("1", 1)]

// --- [OPERATIONS] ----------------------------------------------------------------------

@Suite
struct LongestSideTests {
    @Test(arguments: commands, sides)
    func refusesAtParseUnlessPositive(command: [String], side: (given: String, accepted: Int?)) throws {
        let arguments = command + ["--longest-side=\(side.given)"]
        let read: Result<Int?, any Error> = Result { try WatchApp.parseAsRoot(arguments) }.map { command in ((command as? Capture)?.longestSide ?? (command as? Watch)?.longestSide)?.rawValue }
        switch (read, side.accepted) {
            case (.success(let pixels?), let accepted?): expectNoDifference(pixels, accepted)
            case (.failure(let error), nil):
                let message: String = WatchApp.message(for: error)
                let named: Bool = message.contains("\(side.given) is not a positive integer")
                #expect(named, Comment(rawValue: message))
            case (.success(let pixels), _): Issue.record("\(arguments) parsed with longest side \(String(describing: pixels))")
            case (.failure(let error), _?): Issue.record("\(arguments) refused: \(WatchApp.message(for: error))")
        }
    }
}
