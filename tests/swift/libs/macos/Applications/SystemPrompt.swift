import CustomDump
import Testing

@testable import Applications

// --- [CONSTANTS] -----------------------------------------------------------------------

private let held: [(text: String, name: String, holds: Bool)] = [
    ("Rasm Café quit unexpectedly.", "Rasm Café", true),
    ("Rasm Café quit unexpectedly.", "Café", true),
    ("Rasm Café quit unexpectedly.", "quit unexpectedly", true),
    ("\"Rasm\" would like to control your computer", "Rasm", true),
    ("Rasm", "Rasm", true),
    ("Rasm-Café", "Rasm", true),
    ("Rasmussen quit unexpectedly.", "Rasm", false),
    ("PreRasm quit unexpectedly.", "Rasm", false),
    ("Rasm7 quit unexpectedly.", "Rasm", false),
    ("7Rasm quit unexpectedly.", "Rasm", false),
    ("Cafés quit unexpectedly.", "Café", false),
    ("ßCafé quit unexpectedly.", "Café", false),
    ("Ωmega quit unexpectedly.", "mega", false),
    ("rasm quit unexpectedly.", "Rasm", false),
    ("Rasm quit unexpectedly.", "Other", false),
]

// --- [COMPOSITION] ---------------------------------------------------------------------

struct SystemPrompts {
    @Test(arguments: held)
    func textHoldsNameOnlyBetweenWordBoundaries(text: String, name: String, holds: Bool) {
        expectNoDifference(text.holds(wholeWords: name), holds)
    }
}
