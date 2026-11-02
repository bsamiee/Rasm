public import AppKit
import AsyncAlgorithms
public import OrderedCollections

// --- [OPERATIONS] ----------------------------------------------------------------------

extension StringProtocol {
    func holds(wholeWords name: String) -> Bool {
        ranges(of: name).contains { range in
            [self[..<range.lowerBound].last, self[range.upperBound...].first].allSatisfy { next in next.map { next in !next.isLetter && !next.isNumber } ?? true }
        }
    }
}

extension NSWorkspace {
    @MainActor
    public func systemPrompts(
        naming names: [String],
        notifications: Set<String>,
        polling interval: Duration,
        clock: some Clock<Duration> & Sendable = ContinuousClock(),
    ) -> some AsyncSequence<(host: String, prompts: OrderedDictionary<NSRunningApplication, InstanceState>), Never> & Sendable {
        let prompting: @Sendable (InstanceState) -> InstanceState? = { state in
            guard case .running(.responding(let elements), let usage) = state else { return nil }
            let asking: [Element] = elements.filter { element in
                element.requiresAnswer && ([element.title].compactMap(\.self) + (element.detail?.text ?? [])).contains { text in names.contains(where: text.holds(wholeWords:)) }
            }
            return asking.isEmpty ? nil : .running(.responding(elements: asking), usage: usage)
        }
        func prompts(of host: String) -> some AsyncSequence<(host: String, prompts: OrderedDictionary<NSRunningApplication, InstanceState>), Never> & Sendable {
            instanceStates(of: { instance in instance.bundleIdentifier == host }, walking: \.requiresAnswer, notifications: notifications, polling: interval, clock: clock)
                .map { held in held.compactMapValues(prompting) }.drop(while: \.isEmpty).map { held in (host, held) }
        }
        return merge(
            merge(prompts(of: "com.apple.UserNotificationCenter"), prompts(of: "com.apple.coreservices.uiagent")),
            merge(prompts(of: "com.apple.accessibility.universalAccessAuthWarn"), prompts(of: "com.apple.SecurityAgent")),
        )
    }
}
