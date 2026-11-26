import CasePaths
import Encoding
import Foundation
import UniformTypeIdentifiers

// --- [MODELS] --------------------------------------------------------------------------

public struct Locator: Codable, Hashable, Sendable {
    struct Step: Codable, Hashable, Sendable {
        @CasePathable
        enum Key: Codable, Hashable, Sendable {
            case identifier(value: String)
            case document(type: String, url: URL?)
            case label(value: String)
            case sole
            case members
            indirect case offset(from: Step?, by: Int)

            static func document(at url: URL) -> Self { .document(type: (UTType(filenameExtension: url.pathExtension, conformingTo: .item) ?? .item).identifier, url: url) }

            static func label(folding text: String) -> Self? {
                let folded: String = text.folding(options: [.caseInsensitive, .diacriticInsensitive, .widthInsensitive], locale: nil).filter { character in
                    !character.isPunctuation && !character.isWhitespace
                }
                return folded.isEmpty ? nil : .label(value: folded)
            }
        }

        private static var levels: [PartialCaseKeyPath<Key>] { [\.identifier, \.document, \.label, \.sole, \.members] }

        let role: String
        let key: Key

        var stored: Self {
            switch key {
                case .document(let type, _): Self(role: role, key: .document(type: type, url: nil))
                case .offset(let from, let by): Self(role: role, key: .offset(from: from?.stored, by: by))
                case .identifier, .label, .sole, .members: self
            }
        }

        static func distinguish(_ siblings: [(role: String, keys: [Key])]) -> [Self] {
            let keyed: [Int: Self] = levels.reduce(into: [:]) { keyed, level in
                let open: [(index: Int, step: Self)] = siblings.indices.compactMap { (index: Int) -> (index: Int, step: Self)? in
                    guard keyed[index] == nil, let key = siblings[index].keys.first(where: { (key: Key) -> Bool in key.is(level) }) else { return nil }
                    return (index: index, step: Self(role: siblings[index].role, key: key))
                }
                let counts: [Self: Int] = Dictionary(open.map { entry in (entry.step, 1) }, uniquingKeysWith: +)
                for entry in open where counts[entry.step] == 1 { keyed[entry.index] = entry.step }
            }
            return siblings.indices.reduce(into: (steps: [Self](), anchor: Self?.none, offsets: [String: Int]())) { walk, index in
                let role: String = siblings[index].role
                switch keyed[index] {
                    case .some(let step):
                        walk.steps.append(step)
                        (walk.anchor, walk.offsets) = (step, [:])
                    case .none:
                        walk.offsets[role, default: 0] += 1
                        walk.steps.append(Self(role: role, key: .offset(from: walk.anchor, by: walk.offsets[role, default: 0])))
                }
            }.steps
        }
    }

    private static let encoder: JSONEncoder = .jsonLines
    private static let decoder = JSONDecoder()

    let steps: [Step]

    var positional: Bool { if case .offset = steps.last?.key { true } else { false } }

    var stored: Self { Self(steps: steps.map(\.stored)) }

    func appending(_ step: Step) -> Self { Self(steps: steps + CollectionOfOne(step)) }

    func text() throws(ApplicationFailure) -> String {
        do { return String(decoding: try Self.encoder.encode(stored.steps), as: UTF8.self) } catch { throw ApplicationFailure(subject: "\(steps)", reason: .foundation(error as NSError)) }
    }

    public func encode(to encoder: any Encoder) throws {
        var text = encoder.singleValueContainer()
        try text.encode(self.text())
    }
}

extension Locator {
    public init(from decoder: any Decoder) throws { steps = try Self.decoder.decode([Step].self, from: Data(decoder.singleValueContainer().decode(String.self).utf8)) }
}
