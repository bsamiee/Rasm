public import CasePaths
public import CoreGraphics
public import Foundation

// --- [MODELS] --------------------------------------------------------------------------

private struct FieldKey: CodingKey {
    let stringValue: String
    let intValue: Int? = nil

    init(stringValue: String) {
        self.stringValue = stringValue
    }

    init?(intValue: Int) {
        nil
    }
}

public struct WatchAppsLine: Decodable, Equatable, Sendable {
    @CasePathable
    @dynamicMemberLookup
    public enum Entry: Equatable, Sendable {
        case fail(Failure)
        case system(memory: MemoryPressure, thermal: ThermalState)
        case instances([Int32: Instance])
    }

    private enum EntryKey: String, CodingKey {
        case fail, memory, thermal
    }

    public let key: String
    public let entry: Entry

    public init(from decoder: any Decoder) throws {
        let container: KeyedDecodingContainer<FieldKey> = try decoder.container(keyedBy: FieldKey.self)
        guard let key: FieldKey = container.allKeys.first, container.allKeys.count == 1 else {
            throw DecodingError.dataCorrupted(DecodingError.Context(codingPath: decoder.codingPath, debugDescription: "\(container.allKeys) hold no single key"))
        }
        let fields: KeyedDecodingContainer<FieldKey> = try container.nestedContainer(keyedBy: FieldKey.self, forKey: key)
        let entries: KeyedDecodingContainer<EntryKey> = try container.nestedContainer(keyedBy: EntryKey.self, forKey: key)
        self.key = key.stringValue
        entry =
            switch Set(fields.allKeys.map { field in EntryKey(stringValue: field.stringValue) }) {
                case [.fail]: .fail(try entries.decode(Failure.self, forKey: .fail))
                case [.memory, .thermal]: .system(memory: try entries.decode(MemoryPressure.self, forKey: .memory), thermal: try entries.decode(ThermalState.self, forKey: .thermal))
                default:
                    .instances(
                        Dictionary(
                            uniqueKeysWithValues: try fields.allKeys.map { field in
                                guard let pid: Int32 = Int32(field.stringValue) else {
                                    throw DecodingError.dataCorruptedError(forKey: field, in: fields, debugDescription: "\(field.stringValue) is no pid")
                                }
                                return (pid, try fields.decode(Instance.self, forKey: field))
                            }
                        )
                    )
            }
    }
}

extension WatchAppsLine {
    public enum MemoryPressure: String, Decodable, Sendable {
        case normal
        case warning
        case critical
    }

    public enum ThermalState: String, Decodable, Sendable {
        case nominal
        case fair
        case serious
        case critical
    }

    public enum ProcessStatus: String, Decodable, Sendable {
        case running
        case sleeping
        case stopped
    }

    public enum Scalar: Decodable, Equatable, Sendable {
        case number(Double)
        case flag(Bool)
        case text(String)

        public init(from decoder: any Decoder) throws {
            let container: any SingleValueDecodingContainer = try decoder.singleValueContainer()
            do {
                self = .number(try container.decode(Double.self))
            } catch DecodingError.typeMismatch {
                do { self = .flag(try container.decode(Bool.self)) } catch DecodingError.typeMismatch { self = .text(try container.decode(String.self)) }
            }
        }
    }

    public struct Control: Decodable, Equatable, Sendable {
        public let id: String
        public let role: String?
        public let subrole: String?
        public let label: String?
        public let identifier: String?
        public let value: Scalar?
        public let enabled: Bool?
        public let actions: [String]?
        public let attribute: String?
        public let items: [Self]?
    }

    public struct Unreadable: Decodable, Equatable, Sendable {
        public let reads: [String]
        public let role: String?
        public let label: String?
        public let error: Int32
    }

    public struct Detail: Decodable, Equatable, Sendable {
        public let frame: CGRect?
        public let main: Bool?
        public let focused: Bool?
        public let text: [String]?
        public let controls: [Control]?
        public let unreadable: [Unreadable]?
    }

    public struct Element: Decodable, Equatable, Sendable {
        public let id: String
        public let window: Int?
        public let role: String?
        public let subrole: String?
        public let modal: Bool?
        public let title: String?
        public let description: String?
        public let identifier: String?
        public let document: URL?
        public let edited: Bool?
        public let minimized: Bool?
        public let requiresAnswer: Bool?
        public let actions: [String]?
        public let detail: Detail?
    }

    public struct Usage: Decodable, Equatable, Sendable {
        public let cpu: Double
        public let footprint: Int
        public let threads: Int
    }

    public struct Window: Decodable, Equatable, Sendable {
        public let number: Int
        public let title: String?
        public let frame: CGRect
        public let layer: Int
        public let onScreen: Bool
    }

    @CasePathable
    @dynamicMemberLookup
    public enum Status: Decodable, Equatable, Sendable {
        case launching(windows: [Window])
        case responding(elements: [Element], windows: [Window])
        case unresponsive(process: ProcessStatus, windows: [Window])

        public var windows: [Window] {
            switch self {
                case .launching(let windows), .responding(_, let windows), .unresponsive(_, let windows): windows
            }
        }
    }

    @CasePathable
    @dynamicMemberLookup
    public enum Next: Decodable, Equatable, Sendable {
        case wait
        case drive
        case answer(id: String, accept: String?, discard: String?, cancel: String?)
        case capture(windows: [Int])
        case resume
        case force
        case grant
        case report(url: URL)
    }

    public struct Failure: Decodable, Equatable, Sendable {
        public let reason: String
        public let subject: String
        public let message: String?
        public let pid: Int32?
        public let error: Int32?
        public let attribute: String?
        public let reads: [String]?
        public let role: String?
        public let label: String?
        public let candidates: [String]?
        public let limit: Double?
        public let status: Int32?
        public let next: Next?
    }

    public struct Running: Decodable, Equatable, Sendable {
        public let status: Status
        public let next: Next?
        public let created: Bool?
        public let usage: Usage?
        public let usageFailure: String?
    }

    public struct Report: Decodable, Equatable, Sendable {
        public let url: URL
        public let pid: Int32
        public let bundleIdentifier: String
        public let exception: String
    }

    public struct Exit: Decodable, Equatable, Sendable {
        public let status: Int32?
        public let signal: Int32?
        public let detail: [String]?
        public let report: Report?
        public let reportFailure: String?
        public let statusFailure: String?
        public let next: Next?
    }

    public struct Capture: Decodable, Equatable, Sendable {
        public let file: String
        public let windows: [Int]
    }

    @CasePathable
    @dynamicMemberLookup
    public enum Instance: Decodable, Equatable, Sendable {
        case running(Running)
        case failed(Failure)
        case exited(Exit)
        case captured(Capture)

        private enum CodingKeys: String, CodingKey {
            case running
            case failed
            case exited
            case file
            case windows
        }

        public init(from decoder: any Decoder) throws {
            let container: KeyedDecodingContainer<CodingKeys> = try decoder.container(keyedBy: CodingKeys.self)
            self =
                switch Set(container.allKeys) {
                    case [.running]: .running(try container.decode(Running.self, forKey: .running))
                    case [.failed]: .failed(try container.decode(Failure.self, forKey: .failed))
                    case [.exited]: .exited(try container.decode(Exit.self, forKey: .exited))
                    case [.file, .windows]: .captured(try Capture(from: decoder))
                    default: throw DecodingError.dataCorrupted(DecodingError.Context(codingPath: decoder.codingPath, debugDescription: "\(container.allKeys) name no instance state"))
                }
        }
    }
}
