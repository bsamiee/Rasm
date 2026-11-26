import Encoding
import Foundation

// --- [MODELS] --------------------------------------------------------------------------

public struct Locator: Codable, Hashable, Sendable {
    struct Step: Codable, Hashable, Sendable {
        indirect enum Key: Codable, Hashable, Sendable {
            case identifier(value: String)
            case document(value: URL)
            case label(value: String)
            case sole
            case members
            case offset(from: Step?, by: Int)

            static func label(folding text: String) -> Self? {
                let folded: String = text.folding(options: [.caseInsensitive, .diacriticInsensitive, .widthInsensitive], locale: nil).filter { character in
                    !character.isPunctuation && !character.isWhitespace
                }
                return folded.isEmpty ? nil : .label(value: folded)
            }
        }

        let role: String
        let key: Key

        static func keyed(_ members: [(role: String, keys: [Key])]) -> [Self] {
            let counts: [Self: Int] = members.flatMap { (member: (role: String, keys: [Key])) -> [Self] in member.keys.map { (key: Key) -> Self in Self(role: member.role, key: key) } }
                .reduce(into: [:]) { (counts: inout [Self: Int], step: Self) in counts[step, default: 0] += 1 }
            return members.reduce(into: (steps: [Self](), anchor: Self?.none, offsets: [String: Int]())) { walk, member in
                switch member.keys.lazy.map({ key in Self(role: member.role, key: key) }).first(where: { step in counts[step] == 1 }) {
                    case .some(let step):
                        walk.steps.append(step)
                        (walk.anchor, walk.offsets) = (step, [:])
                    case .none:
                        walk.offsets[member.role, default: 0] += 1
                        walk.steps.append(Self(role: member.role, key: .offset(from: walk.anchor, by: walk.offsets[member.role, default: 0])))
                }
            }.steps
        }
    }

    private static let encoder: JSONEncoder = .jsonLines
    private static let decoder = JSONDecoder()

    let steps: [Step]

    var positional: Bool { if case .offset = steps.last?.key { true } else { false } }

    init(steps: [Step]) { self.steps = steps }

    public init(from decoder: any Decoder) throws { steps = try Self.decoder.decode([Step].self, from: Data(decoder.singleValueContainer().decode(String.self).utf8)) }

    func appending(_ step: Step) -> Self { Self(steps: steps + [step]) }

    func text() throws(ApplicationFailure) -> String {
        do { return String(decoding: try Self.encoder.encode(steps), as: UTF8.self) } catch { throw ApplicationFailure(subject: "\(steps)", reason: .foundation(error as NSError)) }
    }

    public func encode(to encoder: any Encoder) throws {
        var text = encoder.singleValueContainer()
        try text.encode(self.text())
    }
}
