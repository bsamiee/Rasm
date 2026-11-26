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

        static func distinguish(_ siblings: [(role: String, keys: [Key])]) -> [Self] {
            let candidates: [(Self, Int)] = siblings.flatMap { sibling in sibling.keys.map { (key: Key) -> (Self, Int) in (Self(role: sibling.role, key: key), 1) } }
            let counts: [Self: Int] = Dictionary(candidates, uniquingKeysWith: +)
            return siblings.reduce(into: (steps: [Self](), anchor: Self?.none, offsets: [String: Int]())) { walk, sibling in
                switch sibling.keys.lazy.map({ key in Self(role: sibling.role, key: key) }).first(where: { step in counts[step] == 1 }) {
                    case .some(let step):
                        walk.steps.append(step)
                        (walk.anchor, walk.offsets) = (step, [:])
                    case .none:
                        walk.offsets[sibling.role, default: 0] += 1
                        walk.steps.append(Self(role: sibling.role, key: .offset(from: walk.anchor, by: walk.offsets[sibling.role, default: 0])))
                }
            }.steps
        }
    }

    private static let encoder: JSONEncoder = .jsonLines

    let steps: [Step]

    var positional: Bool { if case .offset = steps.last?.key { true } else { false } }

    init(steps: [Step]) { self.steps = steps }

    public init(from decoder: any Decoder) throws { steps = try JSONDecoder().decode([Step].self, from: Data(decoder.singleValueContainer().decode(String.self).utf8)) }

    func appending(_ step: Step) -> Self { Self(steps: steps + [step]) }

    func text() throws(ApplicationFailure) -> String {
        do { return String(decoding: try Self.encoder.encode(steps), as: UTF8.self) } catch { throw ApplicationFailure(subject: "\(steps)", reason: .foundation(error as NSError)) }
    }

    public func encode(to encoder: any Encoder) throws {
        var text = encoder.singleValueContainer()
        try text.encode(self.text())
    }
}
