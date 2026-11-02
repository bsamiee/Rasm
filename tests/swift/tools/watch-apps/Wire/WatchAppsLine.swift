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
        case fail(reason: String)
        case system(memory: MemoryPressure, thermal: ThermalState)
        case instances([Int32: Instance])
    }

    public let key: String
    public let entry: Entry

    public init(from decoder: any Decoder) throws {
        let container: KeyedDecodingContainer<FieldKey> = try decoder.container(keyedBy: FieldKey.self)
        guard let key: FieldKey = container.allKeys.first, container.allKeys.count == 1 else {
            throw DecodingError.dataCorrupted(DecodingError.Context(codingPath: decoder.codingPath, debugDescription: "\(container.allKeys) hold no single key"))
        }
        let fields: KeyedDecodingContainer<FieldKey> = try container.nestedContainer(keyedBy: FieldKey.self, forKey: key)
        let names: Set<String> = Set(fields.allKeys.map(\.stringValue))
        self.key = key.stringValue
        entry =
            switch names {
                case ["fail"]:
                    .fail(reason: try fields.nestedContainer(keyedBy: FieldKey.self, forKey: FieldKey(stringValue: "fail")).decode(String.self, forKey: FieldKey(stringValue: "reason")))
                case ["memory", "thermal"]:
                    .system(
                        memory: try fields.decode(MemoryPressure.self, forKey: FieldKey(stringValue: "memory")),
                        thermal: try fields.decode(ThermalState.self, forKey: FieldKey(stringValue: "thermal")),
                    )
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

    @CasePathable
    @dynamicMemberLookup
    public enum Status: Decodable, Equatable, Sendable {
        case launching
        case responding(elements: [Element])
        case unresponsive(process: ProcessStatus)

        private enum CodingKeys: String, CodingKey {
            case launching
            case responding
            case unresponsive
            case elements
            case process
        }

        public init(from decoder: any Decoder) throws {
            let container: KeyedDecodingContainer<CodingKeys> = try decoder.container(keyedBy: CodingKeys.self)
            self =
                switch container.allKeys {
                    case [.launching]: .launching
                    case [.responding]:
                        .responding(elements: try container.nestedContainer(keyedBy: CodingKeys.self, forKey: .responding).decode([Element].self, forKey: .elements))
                    case [.unresponsive]:
                        .unresponsive(process: try container.nestedContainer(keyedBy: CodingKeys.self, forKey: .unresponsive).decode(ProcessStatus.self, forKey: .process))
                    default: throw DecodingError.dataCorrupted(DecodingError.Context(codingPath: decoder.codingPath, debugDescription: "\(container.allKeys) name no status"))
                }
        }
    }

    public struct Running: Decodable, Equatable, Sendable {
        public let status: Status
        public let usage: Usage?
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
        public let report: Report?
        public let reportFailure: String?
        public let statusFailure: String?
    }

    public struct Capture: Decodable, Equatable, Sendable {
        public let file: String
        public let windows: [Int]
    }

    @CasePathable
    @dynamicMemberLookup
    public enum Instance: Decodable, Equatable, Sendable {
        case running(Running)
        case failed(reason: String)
        case exited(Exit)
        case captured(Capture)

        private enum CodingKeys: String, CodingKey {
            case running
            case failed
            case exited
            case reason
            case file
            case windows
        }

        public init(from decoder: any Decoder) throws {
            let container: KeyedDecodingContainer<CodingKeys> = try decoder.container(keyedBy: CodingKeys.self)
            self =
                switch Set(container.allKeys) {
                    case [.running]: .running(try container.decode(Running.self, forKey: .running))
                    case [.failed]: .failed(reason: try container.nestedContainer(keyedBy: CodingKeys.self, forKey: .failed).decode(String.self, forKey: .reason))
                    case [.exited]: .exited(try container.decode(Exit.self, forKey: .exited))
                    case [.file, .windows]: .captured(try Capture(from: decoder))
                    default: throw DecodingError.dataCorrupted(DecodingError.Context(codingPath: decoder.codingPath, debugDescription: "\(container.allKeys) name no instance state"))
                }
        }
    }
}
