import CasePathsCore
import CustomDump
import Fixture
import RasmTesting
import Testing

// --- [OPERATIONS] ----------------------------------------------------------------------

@Suite(.timeLimit(.minutes(1)))
struct FixtureTests {
    @Test(.foregroundUnchanged)
    func reportsTheRoleAndSubroleAppKitDocumentsForEachControlClassWithoutTakingTheForeground() async throws {
        let report = try await FixtureSession.launched { fixture in
            try await fixture.send(.report)
            return try await fixture.next(\.report)
        }
        let roles: [Control: Set<String?>] = [
            .action: ["AXButton"], .toggle: ["AXCheckBox"], .radio: ["AXRadioButton"], .siblings: ["AXButton"], .popup: ["AXPopUpButton"], .deferred: ["AXPopUpButton"],
            .combo: ["AXComboBox"], .tabs: ["AXTabGroup"], .table: ["AXTable"], .outline: ["AXOutline"], .slider: ["AXSlider"],
            .stepper: ["AXIncrementor"], .measure: ["AXTextField"], .text: ["AXTextField"], .search: ["AXTextField"], .secure: ["AXTextField"],
            .color: ["AXColorWell"], .item: ["AXMenuItem"],
        ]
        let subroles: [Control: Set<String?>] = [.search: ["AXSearchField"], .secure: ["AXSecureTextField"]]
        let reported = Dictionary(grouping: report.controls, by: \.control)
        expectNoDifference(Dictionary(uniqueKeysWithValues: roles.keys.map { control in (control, Set(reported[control, default: []].map(\.role))) }), roles)
        expectNoDifference(Dictionary(uniqueKeysWithValues: subroles.keys.map { control in (control, Set(reported[control, default: []].map(\.subrole))) }), subroles)
    }
}
