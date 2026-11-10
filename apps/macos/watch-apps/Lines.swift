import AppKit
import Applications
import Processes

// --- [CONSTANTS] -----------------------------------------------------------------------

let sessionVariable: String = "WATCH_APPS_SESSION"
let dateFormat: Date.ISO8601FormatStyle = .init(includingFractionalSeconds: true)
let encoder: JSONEncoder = {
    let encoder = JSONEncoder()
    encoder.outputFormatting = [.sortedKeys, .withoutEscapingSlashes]
    encoder.dateEncodingStrategy = .custom { date, target in
        var value = target.singleValueContainer()
        try value.encode(dateFormat.format(date))
    }
    return encoder
}()

// --- [MODELS] --------------------------------------------------------------------------

struct Record<Line> {
    let at: Date
    let pid: pid_t
    var session: String?
    var argv: [String]?
    var line: Line?
    var exit: Exit?

    init(argv: [String]? = nil, line: Line? = nil, exit: Exit? = nil) {
        (at, pid, session, self.argv, self.line, self.exit) = (.now, getpid(), argv.flatMap { _ in ProcessInfo.processInfo.environment[sessionVariable] }, argv, line, exit)
    }
}

extension Record: Encodable where Line: Encodable {}
extension Record: Decodable where Line: Decodable {}

struct Exit: Codable {
    let status: Int32
    let signal: Int32?
}

enum Event: Encodable {
    case fail(ApplicationFailure)
    case launched(created: Bool)
    case running(Outline)
    case captured(Screenshot)

    private enum Key: CodingKey {
        case fail, launched, running, captured, created
    }

    func encode(to encoder: any Encoder) throws {
        var fields = encoder.container(keyedBy: Key.self)
        switch self {
            case .fail(let failure): try fields.encode(failure, forKey: .fail)
            case .launched(let created):
                var launched = fields.nestedContainer(keyedBy: Key.self, forKey: .launched)
                try launched.encode(created, forKey: .created)
            case .running(let outline): try fields.encode(outline, forKey: .running)
            case .captured(let screenshot): try fields.encode(screenshot, forKey: .captured)
        }
    }
}

struct Screenshot: Encodable {
    let file: String
    let number: CGWindowID
}

struct Outline: Encodable {
    struct Row: Encodable {
        let kind: InstanceState.Window.Description
        let number: CGWindowID
        var options: [String]
        var text: [String]
        let title: String?
    }

    let active: Bool
    var asking: [Row]
    let menus: [String]
    let process: ProcessStatus?
    let status: InstanceState.Running.Status
    var windows: [CGWindowID]

    func fitted<Failure: Error>(within limit: Int, measuring length: (Self) throws(Failure) -> Int) throws(Failure) -> Self {
        var cut = self
        var excess = try length(cut) - limit
        while excess > 0 {
            switch (cut.asking.lastIndex { row in !row.text.isEmpty }, cut.asking.lastIndex { row in !row.options.isEmpty }, cut.windows.isEmpty) {
                case (nil, nil, true): return cut
                case (let row?, _, _):
                    let last = cut.asking[row].text.removeLast()
                    cut.asking[row].text += last.count > excess ? [String(last.dropLast(excess))] : []
                case (nil, let row?, _): cut.asking[row].options.removeLast()
                case (nil, nil, false): cut.windows.removeLast()
            }
            excess = try length(cut) - limit
        }
        return cut
    }
}
