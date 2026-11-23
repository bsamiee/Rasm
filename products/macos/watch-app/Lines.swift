import AppKit
import Applications
import Configuration
import CurrentProcess
import Processes

// --- [CONSTANTS] -----------------------------------------------------------------------

let journal: URL = {
    let agents: [String?] = ["CLAUDE_CODE_SESSION_ID", "CODEX_THREAD_ID", "ANTIGRAVITY_CONVERSATION_ID"].map { name in ProcessInfo.processInfo.environment[name] }
    let named: [String] = ([Program.current.settings.string(forKey: "session")] + agents).compactMap(\.self)
    return Program.current.directory(.logs).appending(component: named.first { session in !session.isEmpty } ?? Program.current.name).appendingPathExtension("jsonl")
}()

// --- [MODELS] --------------------------------------------------------------------------

struct Record<Line> {
    let at: Date
    let pid: pid_t
    var argv: [String]?
    var line: Line?
    var exit: Exit?

    init(argv: [String]? = nil, line: Line? = nil, exit: Exit? = nil) {
        (at, pid, self.argv, self.line, self.exit) = (.now, getpid(), argv, line, exit)
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
    case launched(Launched)
    case running(Outline)
    case captured(Footage)
    case node(Node)

    private enum Key: CodingKey {
        case fail, launched, running, captured, node, created, replaced, url
    }

    func encode(to encoder: any Encoder) throws {
        var fields = encoder.container(keyedBy: Key.self)
        switch self {
            case .fail(let failure): try fields.encode(failure, forKey: .fail)
            case .launched(let launched):
                var row = fields.nestedContainer(keyedBy: Key.self, forKey: .launched)
                try row.encode(launched.created, forKey: .created)
                if launched.replaced { try row.encode(true, forKey: .replaced) }
                try row.encodeIfPresent(launched.instance.application.bundleURL, forKey: .url)
            case .running(let outline): try fields.encode(outline, forKey: .running)
            case .captured(let footage): try fields.encode(footage, forKey: .captured)
            case .node(let node): try fields.encode(node, forKey: .node)
        }
    }
}

struct Tagged<Value: Encodable>: Encodable {
    private enum Key: CodingKey {
        case instance
    }

    let instance: ProcessInstance
    let value: Value

    func encode(to encoder: any Encoder) throws {
        try value.encode(to: encoder)
        var fields = encoder.container(keyedBy: Key.self)
        try fields.encode(instance, forKey: .instance)
    }
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

    func fitted(within limit: Int, measuring length: (Self) -> Int) -> Self {
        var cut = self
        var excess = length(cut) - limit
        while excess > 0 {
            switch (cut.asking.lastIndex { row in !row.text.isEmpty }, cut.asking.lastIndex { row in !row.options.isEmpty }, cut.windows.isEmpty) {
                case (nil, nil, true): return cut
                case (let row?, _, _):
                    let last = cut.asking[row].text.removeLast()
                    cut.asking[row].text += last.count > excess ? [String(last.dropLast(excess))] : []
                case (nil, let row?, _): cut.asking[row].options.removeLast()
                case (nil, nil, false): cut.windows.removeLast()
            }
            excess = length(cut) - limit
        }
        return cut
    }
}
