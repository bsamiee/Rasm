import CoreGraphics
import CustomDump
import RasmTesting
import Testing

// --- [OPERATIONS] ----------------------------------------------------------------------

@Suite
struct ForegroundTests {
    @Test(
        arguments: [
            ([], true),
            ([11, 22, 33, 44], true),
            ([11, 33], true),
            ([44, 11, 22, 33], false),
            ([11, 44, 22, 33], false),
            ([22, 11, 33], false),
        ] as [([CGWindowID], Bool)]
    )
    func keepsTheOrderOfSurvivingWindowsAndRefusesANewWindowAheadOfThem(later order: [CGWindowID], unchanged: Bool) {
        let pointer = CGPoint(x: 640, y: 412)
        let before = Foreground(application: 501, window: 22, order: [11, 22, 33], pointer: pointer)
        let later = Foreground(application: 501, window: 22, order: order, pointer: pointer)
        expectNoDifference(before.unchanged(in: later), unchanged)
    }
}
