import CoreGraphics
import CustomDump
import Testing

@testable import watch_apps

// --- [CONSTANTS] -----------------------------------------------------------------------

private let long: String = String(repeating: "a", count: lineLimit)
private let numbers: [CGWindowID] = Array(1...CGWindowID(lineLimit + 2))
private let kept: [String] = numbers.dropLast(2).map { number in String(number) }
private let fits: [(text: [String], options: [String], windows: [CGWindowID], kept: [[String]])] = [
    ([long], ["OK", "Cancel"], [1, 2, 3], [[String(long.dropLast(11))], ["OK", "Cancel"], ["1", "2", "3"]]),
    (["a"], [long], [1], [[], [], ["1"]]),
    ([], [], numbers, [[], [], kept]),
]

// --- [COMPOSITION] ---------------------------------------------------------------------

struct Outlines {
    @Test(arguments: fits)
    func fittedCutsTextThenOptionsThenWindowsDownToTheLimit(text: [String], options: [String], windows: [CGWindowID], kept: [[String]]) throws {
        let row: Outline.Row = Outline.Row(kind: "alert", number: 1, options: options, text: text, title: nil)
        let fitted: Outline = Outline(active: false, asking: [row], menus: [], process: nil, status: "responding", windows: windows).fitted { outline in
            outline.asking.flatMap(\.text).joined().count + outline.asking.flatMap(\.options).joined().count + outline.windows.count
        }
        let cut: Outline.Row = try #require(fitted.asking.first)
        expectNoDifference([cut.text, cut.options, fitted.windows.map { number in String(number) }], kept)
    }
}
