import Applications
import CustomDump
import PropertyBased
import Testing

// --- [OPERATIONS] ----------------------------------------------------------------------

@Suite
struct VersionTests {
    @Test(arguments: [("2.0b2", "2.0b10"), ("3.0.0-beta.1", "3.0.0"), ("1.9", "1.10"), ("27.1", "27.1.1"), ("0.9.9", "1")] as [(String, String)])
    func ordersReleases(earlier: String, later: String) {
        expectNoDifference([later, earlier].map(Installation.Version.init(rawValue:)).sorted().map(\.rawValue), [earlier, later])
    }

    @Test(arguments: [("2.0", "2.0.0"), ("1", "1.0.0.0"), ("3.0-rc1", "3.0.0rc1")] as [(String, String)])
    func equatesTrailingZeros(left: String, right: String) {
        expectNoDifference(Set([left, right].map(Installation.Version.init(rawValue:))).count, 1)
    }

    @Test
    func ordersNumericReleasesLexicographically() async {
        await propertyCheck(input: Gen.int(in: 0...30).array(of: 1...4), Gen.int(in: 0...30).array(of: 1...4)) { left, right in
            let (lower, upper) = (
                Installation.Version(rawValue: left.map(String.init).joined(separator: ".")), Installation.Version(rawValue: right.map(String.init).joined(separator: ".")),
            )
            let trimmed: ([Int]) -> [Int] = { parts in Array(parts.reversed().drop { part in part == 0 }.reversed()) }
            expectNoDifference(lower < upper, trimmed(left).lexicographicallyPrecedes(trimmed(right)))
            expectNoDifference(lower == upper, trimmed(left) == trimmed(right))
        }
    }
}
