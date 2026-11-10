public import AppKit
import AsyncAlgorithms
import CasePaths
import CasePathsCore
public import OrderedCollections
import RegexBuilder

// --- [CONSTANTS] -----------------------------------------------------------------------

let dialogHelpers: Set<String> = [
    "com.apple.UserNotificationCenter", "com.apple.coreservices.uiagent", "com.apple.accessibility.universalAccessAuthWarn", "com.apple.SecurityAgent", "com.apple.tcc.AuthorizationPromptService",
    "com.apple.CoreLocationAgent", "com.apple.ProblemReporter",
]

// --- [OPERATIONS] ----------------------------------------------------------------------

extension InstanceState {
    func prompting(_ names: [String]) -> [Window] {
        let words = names.map { name in
            Regex {
                Anchor.wordBoundary
                name
                Anchor.wordBoundary
            }
        }
        return self[case: \.running]?.windows.filter { window in
            if let element = window.description[case: \.element], element.asking {
                ([element.title].compactMap(\.self) + (element.detail?.text ?? [])).contains { text in words.contains { word in text.contains(word) } }
            } else {
                false
            }
        } ?? []
    }
}

extension NSWorkspace {
    public func systemPrompts(
        naming names: [String],
        clock: some Clock<Duration> & Sendable = ContinuousClock(),
    ) -> some AsyncSequence<OrderedDictionary<NSRunningApplication, InstanceState>, Never> & Sendable {
        tracked(of: \.isDialogHelper, walking: \.asking, clock: clock)
            .map { held in
                held.compactMapValues { state in
                    guard case .running(let running) = state else { return nil }
                    let rows = state.prompting(names)
                    return rows.isEmpty ? nil : .running(InstanceState.Running(status: running.status, active: running.active, windows: rows, prompts: [], menus: [], usage: running.usage))
                }
            }
            .drop(while: \.isEmpty)
            .removeDuplicates()
    }
}
