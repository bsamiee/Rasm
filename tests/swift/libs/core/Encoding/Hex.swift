import CustomDump
import Encoding
import PropertyBased
import Testing

// --- [COMPOSITION] ---------------------------------------------------------------------

struct Hex {
    @Test
    func roundTrip() async {
        await propertyCheck(input: Gen.uint8().array(of: 0...64)) { bytes in
            expectNoDifference([UInt8](hexEncoded: bytes.hexEncoded()), bytes)
        }
    }
}
