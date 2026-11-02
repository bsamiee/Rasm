import AppKit
import Applications
import CasePaths
import OrderedCollections
import TestSupport
import Testing
@testable import AppKitFixture

// --- [OPERATIONS] ----------------------------------------------------------------------

@MainActor
private func ignoreAlerts(naming name: String) async {
    let prompts: some AsyncSequence<(host: String, prompts: OrderedDictionary<NSRunningApplication, InstanceState>), Never> = NSWorkspace.shared.systemPrompts(
        naming: [name],
        notifications: [kAXWindowCreatedNotification],
        polling: .seconds(1),
    )
    for await prompt: (host: String, prompts: OrderedDictionary<NSRunningApplication, InstanceState>) in prompts {
        let elements: [Element] = prompt.prompts.values.compactMap { state in state[case: \.running]?.0[case: \.responding] }.joined().map(\.self)
        let buttons: [Control] = elements.compactMap(\.detail).flatMap(\.controls).filter { control in control.identifier == "action-button-2" }
        for button: Control in buttons {
            do { try await ElementAction.perform(element: button.id, action: kAXPressAction).perform() } catch { Issue.record(error) }
        }
        guard buttons.isEmpty else { return }
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

struct InstanceStates {
    @Test(.application(FixtureApplication.self, arguments: Settings().arguments, answering: ignoreAlerts))
    func launchedFixtureResponds() async throws {
        try checkAccessibilityTrust()
        let installation: Installation = try Installation(at: try #require(TestApplication.current))
        let instance: NSRunningApplication = try #require(NSWorkspace.shared.instances(of: installation.runs).first)
        try await instance.finishedLaunching()
        let elements: [Element] = try #require(await instance.state { _ in false }[case: \.running]?.0[case: \.responding])
        #expect(elements.contains { element in element.title == "Fixture 0" })
    }
}
