import AppKit
import Carbon.HIToolbox
import CustomDump
import Testing

@testable import Applications

// --- [OPERATIONS] ----------------------------------------------------------------------

@_silgen_name("SLSCopySpacesForWindows")
private func copySpacesForWindows(_: Int32, _: SpaceMask, _: CFArray) -> Unmanaged<CFArray>?

@Suite(.timeLimit(.minutes(1)))
struct SessionTests {
    @Test(.enabled(if: !IsSecureEventInputEnabled(), "Another process holds secure event input"))
    func readsEachSecureInputChangeFromTheWindowServerWithNoOtherSessionNotice() async throws {
        var readings = Session.changes().makeAsyncIterator()
        let before = try #require(await readings.next(isolation: #isolation)).get()
        _ = EnableSecureEventInput()
        let held = try #require(await readings.next(isolation: #isolation)).get()
        _ = DisableSecureEventInput()
        let after = try #require(await readings.next(isolation: #isolation)).get()
        expectNoDifference([before.secureInput, held.secureInput, after.secureInput], [false, true, false])
    }

    @Test
    func readsTheCurrentSpaceOfEveryDisplayHoldingAnOnScreenWindow() {
        let session = Session.current()
        let shown: [[UInt64]] = InstanceState.Window.all(.optionOnScreenOnly).values.flatMap { rows in rows.filter { row in row.band == .normal && !row.spaces.isEmpty }.map(\.spaces) }
        expectNoDifference(session.spaces.count, NSScreen.screensHaveSeparateSpaces ? NSScreen.screens.count : 1)
        expectNoDifference(shown.filter { spaces in Set(spaces).isDisjoint(with: session.spaces) }, [])
    }

    @Test
    func placesEachWindowOrderedInOrOutOnEverySpaceTheWindowServerAssignsIt() {
        let connection: Int32 = mainConnection()
        let rows: [InstanceState.Window] = InstanceState.Window.all(.optionAll).values.flatMap(\.self)
        let assigned: [CGWindowID: Set<UInt64>] = Dictionary(
            rows.map { row in (row.number, Set((unsafe copySpacesForWindows(connection, [.current, .others, .user], [row.number] as CFArray)?.takeRetainedValue() as? [UInt64]) ?? [])) }
        ) { first, _ in first }
        expectNoDifference(Dictionary(rows.map { row in (row.number, Set(row.spaces)) }) { first, _ in first }, assigned)
    }
}
