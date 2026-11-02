import AppKit
import Applications
import CasePaths
import CustomDump
import Encoding
import Foundation
import OrderedCollections
import PropertyBased
import Results
import TestSupport
import Testing

@testable import AppKitFixture

// --- [CONSTANTS] -----------------------------------------------------------------------

private let promptName: String = "Rasm Café \(UUID().uuidString.prefix(8))"
private let crashes: [(scenario: Scenario, signal: Int32, exception: String)] = [
    (.fault, 11, "EXC_BAD_ACCESS"), (.signal, 11, "EXC_CRASH"), (.trap, 5, "EXC_BREAKPOINT"), (.abort, 6, "EXC_CRASH"),
]

// --- [OPERATIONS] ----------------------------------------------------------------------

private func crashed(_ instance: NSRunningApplication, by scenario: Scenario) async throws -> [Observed] {
    try await observe(instance, within: .seconds(90), until: { _ in false }, acting: { try await run(scenario, in: instance) })
}

// --- [COMPOSITION] ---------------------------------------------------------------------

extension Integration {
    @Suite
    struct Crashes {
        @Test(
            .application(FixtureApplication.self, arguments: Settings().arguments),
            arguments: crashes,
        )
        func crashReadsTheSignalThenItsReportAndEnds(scenario: Scenario, signal: Int32, exception: String) async throws {
            let (installation, instance): (Installation, NSRunningApplication) = try await launched()
            let seen: [Observed] = try await crashed(instance, by: scenario)
            expectNoDifference(summaries(seen), ["responding", "exited signal \(signal)", "exited signal \(signal) report \(exception)"])
            let exited: Observed = try #require(seen.first { observed in summary(observed.state) == "exited signal \(signal)" })
            let reported: Observed = try #require(seen.last)
            let outcome: Result<CrashReport, ApplicationFailure>? = reported.state[case: \.exited]?.report
            let report: CrashReport = try #require(outcome).get()
            expectNoDifference(report.pid, instance.processIdentifier)
            expectNoDifference(report.bundleIdentifier, installation.identifier)
            expectNoDifference(report.url.deletingLastPathComponent().resolvingSymlinksInPath().pathComponents, CrashReport.directory.resolvingSymlinksInPath().pathComponents)
            let contents: Data = try Data(contentsOf: report.url)
            let header: [String: JSONValue] = try JSONDecoder().decode(JSONValue.self, from: contents.prefix { byte in byte != UInt8(ascii: "\n") }).decode()
            let body: [String: JSONValue] = try JSONDecoder().decode(JSONValue.self, from: contents.drop { byte in byte != UInt8(ascii: "\n") }.dropFirst()).decode()
            expectNoDifference([header["bundleID"], header["bug_type"], body["pid"]], [.string(installation.identifier), .string("309"), .number(Double(instance.processIdentifier))])
            expectElapsed(exited.instant - seen[0].instant, in: .zero...(.seconds(5)), "exit line")
            expectElapsed(reported.instant - exited.instant, in: .zero...(.seconds(60)), "report line")
        }

        @Test(.application(FixtureApplication.self, named: promptName, launches: false, activationPolicy: .regular, answering: answerCrashAlert))
        func crashAlertIsAttributedByWholeWordsOfTheDisplayName() async throws {
            func candidates(in name: String, using generator: inout Xoshiro) -> [String: Bool] {
                let named: [String] = name.split(separator: " ").map { word in String(word) }
                let words: [String] = named + ["quit", "unexpectedly"]
                let spans: [String] = words.indices.flatMap { (start: Int) -> [String] in
                    words.indices[start...].map { (end: Int) -> String in words[start...end].joined(separator: " ") }
                }
                let attached: [Character] = ["x", "7", "é", "ß", "Ω"]
                let infixes: [String] = words.filter { word in word.count >= 4 }.flatMap { word in
                    let characters: [Character] = Array(word)
                    let start: Int = Int.random(in: 0...(characters.count - 3), using: &generator)
                    let end: Int = Int.random(in: (start + 3)...(start == 0 ? characters.count - 1 : characters.count), using: &generator)
                    return [String(characters[start..<end]), String(characters.dropLast()), String(characters.dropFirst())]
                }
                let extended: [String] = words.flatMap { (word: String) -> [String] in
                    let (suffix, prefix): (Character, Character) = (attached.randomElement(using: &generator) ?? "x", attached.randomElement(using: &generator) ?? "x")
                    return [word + String(suffix), String(prefix) + word]
                }
                let matching: Set<String> = Set(spans.shuffled(using: &generator).prefix(5) + ["Café", "unexpectedly"])
                let missing: Set<String> = Set(infixes + extended).subtracting(words + spans)
                return Dictionary(uniqueKeysWithValues: matching.map { candidate in (candidate, true) } + missing.map { candidate in (candidate, false) })
            }
            func prompt(naming name: String) async -> Result<(host: String, element: Element)?, TestFailure> {
                await Result { () async throws(TestFailure) in
                    try await withTimeout(.seconds(20), expired: TestFailure(subject: "prompt naming \(name)", reason: .missing)) { () async throws(TestFailure) in
                        let shown: (host: String, prompts: OrderedDictionary<NSRunningApplication, InstanceState>)? = await NSWorkspace.shared.systemPrompts(
                            naming: [name],
                            notifications: [kAXWindowCreatedNotification],
                            polling: heartbeat,
                        )
                        .first { prompt in prompt.prompts.values.contains { state in state.elements.contains(where: \.requiresAnswer) } }
                        return shown.flatMap { prompt in prompt.prompts.values.lazy.flatMap(\.elements).first(where: \.requiresAnswer).map { element in (prompt.host, element) } }
                    }
                }
            }
            try checkAccessibilityTrust()
            let installation: Installation = try installation()
            let instance: NSRunningApplication = try await NSWorkspace.shared.launch(installation, configuration: configuration(Settings(), hides: true))
            try await instance.finishedLaunching()
            expectNoDifference(instance.activationPolicy, .regular)
            expectNoDifference(installation.name, promptName)
            let seen: [Observed] = try await crashed(instance, by: .fault)
            expectNoDifference(summaries(seen).last, "exited signal 11 report EXC_BAD_ACCESS")
            var generator: Xoshiro = Xoshiro()
            let seed: String = generator.currentSeed
            let expected: [String: Bool] = candidates(in: promptName, using: &generator).merging([promptName: true]) { kept, _ in kept }
            let found: [String: Result<(host: String, element: Element)?, TestFailure>] = await withTaskGroup { group in
                for name: String in expected.keys { group.addTask { (name, await prompt(naming: name)) } }
                return await group.reduce(into: [:]) { found, next in found[next.0] = next.1 }
            }
            let outcomes: [String: String] = found.mapValues { result in
                switch result {
                    case .success(.some(let prompt)): "attributed by \(prompt.host)"
                    case .success(.none): "stream ended"
                    case .failure: "unattributed"
                }
            }
            expectNoDifference(outcomes, expected.mapValues { attributed in attributed ? "attributed by com.apple.UserNotificationCenter" : "unattributed" }, "seed \(seed)")
            let full: (host: String, element: Element) = try #require(try found[promptName]?.get())
            expectNoDifference(full.host, "com.apple.UserNotificationCenter")
            expectNoDifference(
                try shape(full.element, dropping: ["id", "window", "detail", "title", "modal"]),
                try json(#"{"role":"AXWindow","subrole":"AXSystemDialog","identifier":"_NS:87","description":"alert","minimized":false,"actions":[],"requiresAnswer":true}"#),
            )
            let named: Bool = full.element.detail?.text.contains { text in text.hasPrefix("\(promptName) quit unexpectedly") } == true
            #expect(named, "text \(full.element.detail?.text ?? [])")
            let answerable: Bool = full.element.detail?.controls.contains { control in control.identifier == "action-button-2" } == true
            #expect(answerable, "alert offers its second button")
        }
    }
}
