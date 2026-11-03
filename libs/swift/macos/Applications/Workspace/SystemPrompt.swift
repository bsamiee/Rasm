import AppKit
import AsyncAlgorithms
import OrderedCollections

// --- [CONSTANTS] -----------------------------------------------------------------------

private let hosts: [String] = [
    "com.apple.UserNotificationCenter", "com.apple.coreservices.uiagent", "com.apple.accessibility.universalAccessAuthWarn", "com.apple.SecurityAgent",
]

// --- [OPERATIONS] ----------------------------------------------------------------------

extension StringProtocol {
    func holds(wholeWords name: String) -> Bool {
        ranges(of: name).contains { range in
            [self[..<range.lowerBound].last, self[range.upperBound...].first].allSatisfy { next in next.map { next in !next.isLetter && !next.isNumber } ?? true }
        }
    }
}

extension InstanceState {
    fileprivate func prompting(_ names: [String]) -> Self? {
        guard case .running(let running) = self, case .responding(let elements, let windows) = running.status else { return nil }
        let asking: [Element] = elements.filter { element in
            element.requiresAnswer && ([element.title].compactMap(\.self) + (element.detail?.text ?? [])).contains { text in names.contains(where: text.holds(wholeWords:)) }
        }
        return asking.isEmpty ? nil : .running(Running(status: .responding(elements: asking, windows: windows), usage: running.usage))
    }
}

extension NSWorkspace {
    @MainActor
    func systemPrompts(naming names: [String]) -> AsyncStream<(host: String, prompts: OrderedDictionary<NSRunningApplication, InstanceState>)> {
        AsyncStream { continuation in
            let following: Task<Void, Never> = Task { @MainActor in
                await withDiscardingTaskGroup { group in
                    for host: String in hosts {
                        let prompts: some AsyncSequence<OrderedDictionary<NSRunningApplication, InstanceState>, Never> & Sendable = instanceStates { instance in
                            instance.bundleIdentifier == host
                        }
                        .map { held in held.compactMapValues { state in state.prompting(names) } }.drop(while: \.isEmpty).removeDuplicates()
                        group.addTask { for await held: OrderedDictionary<NSRunningApplication, InstanceState> in prompts { continuation.yield((host, held)) } }
                    }
                }
                continuation.finish()
            }
            continuation.onTermination = { _ in following.cancel() }
        }
    }

    @MainActor
    func currentPrompts(naming names: [String]) async -> [(host: String, prompts: OrderedDictionary<NSRunningApplication, InstanceState>)] {
        var found: [(host: String, prompts: OrderedDictionary<NSRunningApplication, InstanceState>)] = []
        for host: String in hosts {
            var prompts: OrderedDictionary<NSRunningApplication, InstanceState> = [:]
            for instance: NSRunningApplication in instances(of: { instance in instance.bundleIdentifier == host }) {
                prompts[instance] = await instance.state(walking: \.requiresAnswer).prompting(names)
            }
            if !prompts.isEmpty { found.append((host, prompts)) }
        }
        return found
    }
}
