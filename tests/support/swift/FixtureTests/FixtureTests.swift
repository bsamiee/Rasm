import AppKit
@unsafe @preconcurrency import ApplicationServices
import CasePathsCore
import CustomDump
import Fixture
import Testing

// --- [MODELS] --------------------------------------------------------------------------

struct Read: Hashable {
    let role: String?
    let subrole: String?
    let frame: CGRect?
    let parent: String?
    let help: String?
    let caption: String?
    let refused: [String: AXError]
}

// --- [ERRORS] --------------------------------------------------------------------------

struct BodyFailure: Error {
    let process: pid_t
}

// --- [OPERATIONS] ----------------------------------------------------------------------

@Suite(.serialized, .timeLimit(.minutes(1)))
struct FixtureTests {
    // --- [CONTRACT]

    @Test
    func reportsEveryMemberOnScreenAsAnOutsideAccessibilityReadOfTheProcessFindsIt() async throws {
        let roles: [NSAccessibility.Role] = [
            .button, .checkBox, .radioButton, .popUpButton, .comboBox, .tabGroup, .radioGroup, .table, .outline, .slider, .incrementor, .textField, .colorWell, .dateTimeAreaRole,
        ]
        let documented: Set<String> = Set(roles.map(\.rawValue))
        let (report, outside): (Report, [Read]) = try await FixtureSession.launched { fixture in
            try await fixture.send(.show(surface: .panel))
            try await fixture.send(.report)
            let report = try await fixture.next(\.report).value
            func read(_ element: AXUIElement, _ attribute: String) throws -> CFTypeRef? {
                var value: CFTypeRef?
                let answer: AXError = unsafe AXUIElementCopyAttributeValue(element, attribute as CFString, &value)
                let answered: Bool = answer == .success
                let refusal: Comment = "Reading \(attribute) of the fixture answered AXError \(answer.rawValue)"
                try #require(answered, refusal)
                return value
            }
            func decoded<Value: BitwiseCopyable>(_ slot: CFTypeRef, as type: AXValueType, from start: Value) throws -> Value {
                var value: Value = start
                let typed: Bool = unsafe CFGetTypeID(slot) == AXValueGetTypeID() && AXValueGetValue(unsafeDowncast(slot, to: AXValue.self), type, &value)
                let mistyped: Comment = "The fixture answered \(slot) where an AXValue of type \(type.rawValue) belongs"
                try #require(typed, mistyped)
                return value
            }
            func elements(_ slot: CFTypeRef?) -> [AXUIElement] {
                let held: [CFTypeRef] = (slot as? [CFTypeRef]) ?? slot.map { single in [single] } ?? []
                return held.compactMap { one in CFGetTypeID(one) == AXUIElementGetTypeID() ? unsafe unsafeDowncast(one, to: AXUIElement.self) : nil }
            }
            let attributes = [kAXRoleAttribute, kAXSubroleAttribute, kAXPositionAttribute, kAXSizeAttribute, kAXParentAttribute, kAXHelpAttribute, kAXTitleUIElementAttribute, kAXChildrenAttribute]
            let height = CGDisplayBounds(CGMainDisplayID()).height
            var pending = elements(try read(AXUIElementCreateApplication(fixture.process), kAXWindowsAttribute))
            var outside: [Read] = []
            while let element = pending.popLast() {
                var copied: CFArray?
                let answer: AXError = unsafe AXUIElementCopyMultipleAttributeValues(element, attributes as CFArray, [], &copied)
                let answered: Bool = answer == .success
                let refusal: Comment = "Reading \(attributes) of a fixture element answered AXError \(answer.rawValue)"
                try #require(answered, refusal)
                let slots: [String: CFTypeRef] = Dictionary(uniqueKeysWithValues: zip(attributes, try #require(copied as? [CFTypeRef])))
                let errors: [String: AXError] = try slots.filter { _, slot in CFGetTypeID(slot) == AXValueGetTypeID() && AXValueGetType(unsafe unsafeDowncast(slot, to: AXValue.self)) == .axError }
                    .mapValues { slot in try decoded(slot, as: .axError, from: AXError.success) }
                let values: [String: CFTypeRef] = slots.filter { attribute, slot in errors[attribute] == nil && CFGetTypeID(slot) != CFNullGetTypeID() }
                pending += elements(values[kAXChildrenAttribute])
                let origin: CGPoint? = try values[kAXPositionAttribute].map { slot in try decoded(slot, as: .cgPoint, from: CGPoint.zero) }
                let size: CGSize? = try values[kAXSizeAttribute].map { slot in try decoded(slot, as: .cgSize, from: CGSize.zero) }
                outside.append(
                    Read(
                        role: values[kAXRoleAttribute] as? String,
                        subrole: values[kAXSubroleAttribute] as? String,
                        frame: origin.flatMap { origin in size.map { size in CGRect(x: origin.x, y: height - origin.y - size.height, width: size.width, height: size.height) } },
                        parent: try elements(values[kAXParentAttribute]).first.flatMap { parent in try read(parent, kAXRoleAttribute) as? String },
                        help: values[kAXHelpAttribute] as? String,
                        caption: try elements(values[kAXTitleUIElementAttribute]).first.flatMap { title in try read(title, kAXValueAttribute) as? String },
                        refused: errors.filter { _, error in ![AXError.noValue, .attributeUnsupported].contains(error) },
                    )
                )
            }
            return (report, outside)
        }
        let visible: Set<Int> = Set(report.windows.filter(\.visible).map(\.number))
        let reported: [Read] = report.controls.filter { control in control.window.map { number throws(Never) in visible.contains(number) } ?? false }.compactMap { control in
            control.frame.map { frame in Read(role: control.role, subrole: control.subrole, frame: frame, parent: control.parent, help: control.help, caption: control.caption, refused: [:]) }
        }
        let frames: Set<CGRect> = Set(reported.compactMap(\.frame))
        let seen: Set<Read> = Set(
            outside.filter { read in
                !read.refused.isEmpty || (read.frame.map { frame throws(Never) in frames.contains(frame) } ?? false && read.role.map { role throws(Never) in documented.contains(role) } ?? false)
            }
        )
        let stated: Set<Read> = Set(reported.filter { read in read.role.map { role throws(Never) in documented.contains(role) } ?? false })
        expectNoDifference(seen, stated)
        expectNoDifference(documented.subtracting(reported.compactMap(\.role)), [])
    }

    @Test
    func runsOutOfTheDockAndNeverActivatesWhileItsWindowsOrderFront() async throws {
        let (policy, lines): (NSApplication.ActivationPolicy, [Line]) = try await FixtureSession.launched { fixture in
            let running = try #require(NSRunningApplication(processIdentifier: fixture.process))
            try await fixture.send(.show(surface: .panel))
            try await fixture.send(.raise(surface: .controls))
            try await fixture.send(.report)
            return (running.activationPolicy, try await fixture.next(\.report).before)
        }
        #expect(policy != .regular, "The fixture runs under activation policy \(policy.rawValue), which puts it in the Dock")
        #expect(!lines.contains { line in line[case: \.active] == true }, "The fixture became active: \(lines)")
    }

    // --- [TEARDOWN]

    @Test
    func endsByTheTimeASessionWhoseBodyReturnedFinishes() async throws {
        let process: pid_t = try await FixtureSession.launched { fixture in fixture.process }
        let ended: Bool = kill(process, 0) == -1 && errno == ESRCH
        #expect(ended, "Fixture \(process) outlived a session whose body returned")
    }

    @Test
    func endsByTheTimeASessionWhoseBodyThrewFinishes() async throws {
        let failure = await #expect(throws: BodyFailure.self) { try await FixtureSession.launched { fixture in throw BodyFailure(process: fixture.process) } }
        let process = try #require(failure?.process)
        let ended: Bool = kill(process, 0) == -1 && errno == ESRCH
        #expect(ended, "Fixture \(process) outlived a session whose body threw")
    }

    @Test
    func endsByTheTimeACancelledSessionFinishes() async throws {
        let (processes, started) = AsyncStream.makeStream(of: pid_t.self)
        let session = Task {
            try await FixtureSession.launched { fixture in
                started.yield(fixture.process)
                return try await fixture.next(\.report).value
            }
        }
        let process = try #require(await processes.first { _ in true })
        session.cancel()
        _ = await session.result
        let ended: Bool = kill(process, 0) == -1 && errno == ESRCH
        #expect(ended, "Fixture \(process) outlived a session cancelled while its body awaited a line")
    }

    @Test
    func endsOnceTheProcessHoldingItsSessionIsKilled() async throws {
        let result = try await #require(processExitsWith: .signal(SIGKILL), observing: [\.standardOutputContent]) {
            try await FixtureSession.launched { fixture in
                try FileHandle.standardOutput.write(contentsOf: Data(String(fixture.process).utf8))
                kill(getpid(), SIGKILL)
            }
        }
        let printed = String(decoding: result.standardOutputContent, as: UTF8.self)
        let parsed: pid_t? = pid_t(printed)
        let process: pid_t = try #require(parsed)
        let (exits, exited) = AsyncStream.makeStream(of: Never.self)
        let source = DispatchSource.makeProcessSource(identifier: process, eventMask: .exit)
        source.setEventHandler { exited.finish() }
        source.activate()
        for await _ in exits {}
        source.cancel()
    }
}
