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

func isDialogHelper(_ process: pid_t) -> Bool {
    NSRunningApplication(processIdentifier: process)?.isDialogHelper == true
}

extension StringProtocol {
    func holds(wholeWords name: String) -> Bool {
        let word: Regex<Substring> = Regex {
            Anchor.wordBoundary
            name
            Anchor.wordBoundary
        }
        return String(self).firstMatch(of: word) != nil
    }
}

extension InstanceState {
    func prompting(_ names: [String]) -> [Window] {
        self[case: \.running]?.windows.filter { window in
            if let element: Element = window.description[case: \.element], element.asking {
                ([element.title].compactMap(\.self) + (element.detail?.text ?? [])).contains { text in names.contains(where: text.holds(wholeWords:)) }
            } else {
                false
            }
        } ?? []
    }

    fileprivate func prompts(naming names: [String]) -> Self? {
        guard case .running(let running) = self else { return nil }
        let rows: [Window] = prompting(names)
        return rows.isEmpty ? nil : .running(Running(status: running.status, active: running.active, windows: rows, prompts: [], menus: [], usage: running.usage))
    }
}

extension NSWorkspace {
    public func systemPrompts(
        naming names: [String],
        clock: some Clock<Duration> & Sendable = ContinuousClock(),
    ) -> some AsyncSequence<OrderedDictionary<NSRunningApplication, InstanceState>, Never> & Sendable {
        tracked(of: \.isDialogHelper, walking: \.asking, clock: clock).map { held in held.compactMapValues { state in state.prompts(naming: names) } }.drop(while: \.isEmpty).removeDuplicates()
    }
}
