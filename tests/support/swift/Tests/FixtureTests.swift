import CasePathsCore
import CustomDump
import Fixture
import Testing

// --- [OPERATIONS] ----------------------------------------------------------------------

@Suite(.timeLimit(.minutes(1)))
struct FixtureTests {
    @Test
    func reportsTheRoleAndSubroleAppKitDocumentsForEveryInventoryEntry() async throws {
        let report = try await FixtureSession.launched { fixture in
            try await fixture.send(.report)
            return try await fixture.next(\.report).value
        }
        let roles: [Inventory: String] = [
            .action: "AXButton", .toggle: "AXCheckBox", .popup: "AXPopUpButton", .deferred: "AXPopUpButton", .combo: "AXComboBox", .radio: "AXRadioButton",
            .tabs: "AXTabGroup", .segments: "AXRadioGroup", .tools: "AXRadioGroup", .table: "AXTable", .outline: "AXOutline", .slider: "AXSlider",
            .stepper: "AXIncrementor", .measure: "AXTextField", .text: "AXTextField", .search: "AXTextField", .secure: "AXTextField", .color: "AXColorWell",
            .date: "AXDateTimeArea", .item: "AXMenuItem", .region: "AXUnknown", .siblings: "AXButton",
        ]
        let subroles: [Inventory: String] = [.search: "AXSearchField", .secure: "AXSecureTextField"]
        let reported = Dictionary(grouping: report.controls, by: \.entry)
        expectNoDifference(reported.mapValues { controls in Set(controls.map(\.role)) }, roles.mapValues { role in [role] })
        expectNoDifference(reported.mapValues { controls in Set(controls.map(\.subrole)) }, Dictionary(uniqueKeysWithValues: Inventory.allCases.map { entry in (entry, [subroles[entry]]) }))
    }
}
