import AppKit
import Applications
import CasePaths
import Configuration
import CurrentProcess
import Encoding
import Processes

// --- [CONSTANTS] -----------------------------------------------------------------------

let journal: URL = Program.current.directory(.log).appending(path: Program.current.name, directoryHint: .isDirectory)
    .appending(component: Program.current.settings.string(forKey: "session").flatMap { session in session.isEmpty ? nil : session } ?? Program.current.name).appendingPathExtension("jsonl")

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

extension Record<[String: JSONValue]> {
    static func journaled(_ text: Data) throws -> [(text: Data, record: Self)] {
        let decoder = JSONDecoder()
        decoder.dateDecodingStrategy = .custom { decoder in try Date.ISO8601FormatStyle(includingFractionalSeconds: true).parse(decoder.singleValueContainer().decode(String.self)) }
        return try text.split(separator: UInt8(ascii: "\n")).map { line in (Data(line) + [UInt8(ascii: "\n")], try decoder.decode(Self.self, from: Data(line))) }
    }

    var instances: [Seen] { (line ?? [:]).values.compactMap { keyed in Result { try keyed.decode([String: Seen].self) }[case: \.success] }.flatMap(\.values) }
}

struct Seen: Decodable {
    struct Launch: Decodable {
        let created: Bool
    }

    let instance: ProcessInstance
    let launched: Launch?
}

struct Exit: Codable {
    let status: Int32
    let signal: Int32?
}

struct Probe: Encodable {
    let argv: [String]
    let runs: Int
    let seconds: Double
}

enum Event: Encodable {
    case fail(ApplicationFailure)
    case launched(Launched)
    case running(Outline)
    case captured(Footage)
    case change(InstanceChange)
    case busy(ResourceUsage, load: Double)
    case idle(load: Double)
    case logged(LogEntry)
    case polled(Probe)

    private enum Key: CodingKey {
        case fail, launched, running, captured, created, replaced, status, process, closed, number, title, opened, failed, busy, idle, load, footprint, threads, logged, type, subsystem, category,
            message, polled
    }

    func encode(to encoder: any Encoder) throws {
        var fields = encoder.container(keyedBy: Key.self)
        switch self {
            case .fail(let failure): try fields.encode(failure, forKey: .fail)
            case .launched(let launched):
                var row = fields.nestedContainer(keyedBy: Key.self, forKey: .launched)
                try row.encode(launched.created, forKey: .created)
                if launched.replaced { try row.encode(true, forKey: .replaced) }
            case .running(let outline): try fields.encode(outline, forKey: .running)
            case .captured(let footage): try fields.encode(footage, forKey: .captured)
            case .change(.status(let status)):
                try fields.encode(status, forKey: .status)
                try fields.encodeIfPresent(status[case: \.unresponsive], forKey: .process)
            case .change(.closed(let window)):
                var row = fields.nestedContainer(keyedBy: Key.self, forKey: .closed)
                try row.encode(window.number, forKey: .number)
                try row.encodeIfPresent(window.title, forKey: .title)
            case .change(.opened(let window)): try fields.encode(Outline.Row(window), forKey: .opened)
            case .change(.failed(let failure)): try fields.encode(failure, forKey: .failed)
            case .change(.exited(let exit, let report)): try InstanceState.exited(exit, report: report).encode(to: encoder)
            case .busy(let usage, let load):
                var row = fields.nestedContainer(keyedBy: Key.self, forKey: .busy)
                try row.encode(load, forKey: .load)
                try row.encode(usage.footprint, forKey: .footprint)
                try row.encode(usage.threads, forKey: .threads)
            case .idle(let load):
                var row = fields.nestedContainer(keyedBy: Key.self, forKey: .idle)
                try row.encode(load, forKey: .load)
            case .logged(let entry):
                var row = fields.nestedContainer(keyedBy: Key.self, forKey: .logged)
                try row.encodeIfPresent(entry.messageType, forKey: .type)
                try row.encodeIfPresent(entry.subsystem, forKey: .subsystem)
                try row.encodeIfPresent(entry.category, forKey: .category)
                try row.encodeIfPresent(entry.eventMessage, forKey: .message)
            case .polled(let probe): try fields.encode(probe, forKey: .polled)
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
        private enum CodingKeys: CodingKey {
            case kind, number, options, text, title
        }

        let kind: InstanceState.Window.Description
        let number: CGWindowID
        var options: [String]
        var text: [String]
        let title: String?

        init(_ window: InstanceState.Window) {
            let element = window.description[case: \.element]
            (kind, number, options, text, title) = (
                window.description, window.number, element?.answers?.buttons.compactMap(\.label) ?? [], element?.detail?.text ?? [], element?.title ?? window.title,
            )
        }

        func encode(to encoder: any Encoder) throws {
            var fields = encoder.container(keyedBy: CodingKeys.self)
            try fields.encode(kind, forKey: .kind)
            try fields.encode(number, forKey: .number)
            if !options.isEmpty { try fields.encode(options, forKey: .options) }
            if !text.isEmpty { try fields.encode(text, forKey: .text) }
            try fields.encodeIfPresent(title, forKey: .title)
        }
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
