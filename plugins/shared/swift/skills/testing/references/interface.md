# [INTERFACE]

Tests of code reading or driving another process launch `Fixture`, act on it from outside, and compare what code under test reads with what `Fixture` reports about itself.

## [01]-[FIXTURE]

`Fixture` is an executable target in `tests/support/swift/Fixture`, an AppKit app holding the controls of each `Inventory` case on its `Surface` windows. Test targets depend on it, and `FixtureSession.launched` runs its build from beside the test bundle through `Subprocess`:

```swift
let (report, before): (Report, [Line]) = try await FixtureSession.launched { fixture in
    try await fixture.send(.show(surface: .panel))
    try await fixture.send(.report)
    return try await fixture.next(\.report)
}
```

- Wire values are JSON lines: `Command` on standard input, `Line` on standard output, and errors on standard error
- Bodies start after the `.ready` line, and `launched` finishes standard input once the body returns, which ends the fixture's process
- Acts, outside reads, and captures run inside the body while the fixture lives, and the body returns every value its expectations read
- Expectations follow the body, and `#require` inside it holds a fact a later step needs
- Windows order back one level below `.normal`, and `Fixture` never activates, so the owner's work keeps focus while a suite runs
- Report fields read `NSAccessibility` members of each control's `NSAccessibility.unignoredDescendant(of:)`, the element a client reaches, never AppKit state
- Facts a new test needs join `Report` or `Line` as the AppKit type stating them, a host enum as a `Lenient` field (`Lenient<NSEvent.EventType>`)

## [02]-[ORACLE]

Reports and outside reads of one launch come from separate mechanisms, AppKit's accessibility protocol inside `Fixture` and Accessibility messaging from the test process, and their relation is the oracle (`code:test` relation row). Comparing a report with values `Fixture` wrote itself checks the wire alone:

| [INDEX] | [SUITE]        | [EXPECTED]                                                       | [GOT]                                                |
| :-----: | :------------- | :--------------------------------------------------------------- | :--------------------------------------------------- |
|  [01]   | `FixtureTests` | Reported members with a frame, roles from `NSAccessibility.Role` | Outside rows at those frames                         |
|  [02]   | `ModelTests`   | Kind and label per `Inventory` case from a literal table         | `Watch.model(.application)` control at that frame    |
|  [03]   | `ModelTests`   | Refs of a first pass                                             | Refs after a window shows again or its lines reorder |
|  [04]   | `ModelTests`   | Menu item shortcuts as `NSEvent` flags                           | Key steps the library reads from each menu level     |

- Expected roles and kinds are literals and AppKit constants (`NSAccessibility.Role.button`), never fixture output, as a role pinned from output confirms itself
- Unordered members compare as sets (`expectNoDifference(seen, stated)`), and coverage prints its gap (`expectNoDifference(documented.subtracting(shown), [])`) in place of a bound `Bool`
- Members sharing a role and label on one line are positional, and a reorder relation leaves them out, as no client tells them apart

## [03]-[OUTSIDE_READS]

Hand-written Accessibility reads exist where `Fixture` itself is under test (`FixtureTests`), and library tests read through `Watch`. Each read keeps every answer:
- Windows read first, `kAXWindowsAttribute` of `AXUIElementCreateApplication(fixture.process)` under `#require` on `.success` with the raw `AXError` in its comment
- Discarded answers read an untrusted test process, answering `kAXErrorAPIDisabled`, as an app with no windows
- Elements take one `AXUIElementCopyMultipleAttributeValues` per element for every attribute a pass reads, children included, walked through a worklist
- Calls with options `[]` return their own `AXError`, an `AXValue` of type `.axError` in each refused slot, and `CFNull` for an attribute with no value, each kept in its row
- Positions put 0,0 at the top-left of the menu-bar screen and AppKit frames at its bottom-left, so rows flip as `height - origin.y - size.height`
- `height` is `CGDisplayBounds(CGMainDisplayID()).height`, read once per test

## [04]-[SHARED_STATE]

Suites of one bundle using key focus, the frontmost application, posted events, or the general pasteboard nest under one `.serialized` parent, as separate suites run beside each other:

```swift
@Suite(.serialized, .timeLimit(.minutes(1)))
struct WindowServerTests {}

extension WindowServerTests {
    struct KeyboardTests {}
}
```

- `WindowServerTests` in `ApplicationsTests` holds `KeyboardTests`, `LaunchTests`, `MenuTests`, `ModelTests`, and `RouteTests`, each in its own file as an extension
- Fixture suites of `FixtureTests` and `ScreenCaptureTests` sit in their own bundles, so they never overlap `ApplicationsTests`
- Suites reading no session state stay outside it (`PasteboardTests` on unique pasteboards, `InputTests` and `PointerTests` on values)

## [05]-[EVENT_ORDER]

`Fixture` writes one line per event it dispatches, in dispatch order, through one pipe. `next(_:)` reads lines until one matches its case path and returns that value with every line read before it, and a later call starts after the match, so a test keeps each `before` it asserts over:

| [INDEX] | [SUITE]             | [AWAITED]                         | [ASSERTED]                                                            |
| :-----: | :------------------ | :-------------------------------- | :-------------------------------------------------------------------- |
|  [01]   | `MenuTests`         | `.report` after the last act      | `menu` balance, `tracking` order, no `.active(true)`, key sources     |
|  [02]   | `RouteTests` clicks | Down and up `.pointer` per press  | `clickCount` literals per schedule, arrival gaps at least posted gaps |
|  [03]   | `RouteTests` pen    | Entering and leaving `.proximity` | Event types, `tabletPoint` subtype, pressure and tilt between them    |
|  [04]   | `RouteTests` focus  | Activation and deactivation       | Mouse down and up alone between them                                  |

- Absence asserts over lines between awaited lines, which is exact, and `.timeLimit` ends a wait for a line that never comes, in place of a sleep or poll
- Event timing compares `timestamp` gaps of fixture lines with gaps the test posted, never wall-clock reads
- Posted keys carry the test's process in `sourceProcess`, read from `eventSourceUnixProcessID`, so `getpid()` proves no other process typed into `Fixture`
- Posted points derive from a reported frame flipped against `CGDisplayBounds(CGMainDisplayID()).height` and reach a window behind the owner's windows
- Typed text holds characters beyond ASCII (`"Façade Noël, À É"`), as each key resolves through the current layout's map (`Input/Keyboard.swift`)

## [06]-[DRAWN_IMAGES]

Text recognition and screenshot comparison read images a test draws into a `CGContext` with labels, frames, and marks it chose, needing no grant and no window:
- Contexts take a backing scale of 2 and a flip (`translateBy`, `scaleBy(x: 2, y: -2)`), and AppKit text draws through `NSGraphicsContext(cgContext:flipped: true)`
- Drawing sits between `NSGraphicsContext.saveGraphicsState()` and `restoreGraphicsState()`, as a current context belongs to the pool thread running the test
- Recognized text compares as a set with drawn labels, and each label's recognized center falls inside its drawn rectangle offset by the capture frame
- Languages are argument rows (`en`, `ja`) holding labels at the top and bottom edges and a word no dictionary holds
- `Screenshot.changed(from:)` rows: equal images give `nil`, and a mark one color step off names its rectangle within half a point, a corner pixel and an edge band included

## [07]-[FAILURE_CAPTURES]

Red runs keep what the screen held: tests hand a window capture taken inside the session to a task-local holder, and a trait on the serialized parent records it as a PNG attachment once the test fails:

```swift
final class Captures: Sendable {
    @TaskLocal static var current: Captures?

    let shots = Mutex<[Screenshot]>([])
    let failed = Atomic<Bool>(false)

    func take(in watch: Watch) async {
        let numbers: [CGWindowID] = await watch.current[case: \.running]?.windows.filter(\.onScreen).map(\.number) ?? []
        let taken: [Result<Screenshot, ApplicationFailure>]
        do throws(ApplicationFailure) {
            taken = try await watch.instance.capture(.windows(numbers: numbers), in: watch)
        } catch {
            taken = [.failure(error)]
        }
        shots.withLock { shots in shots += taken.compactMap { shot in shot[case: \.success] } }
        for refusal in taken.compactMap({ shot in shot[case: \.failure] }) { Issue.record("Capturing windows \(numbers) answered \(refusal.localizedDescription)", severity: .warning) }
    }
}

struct CapturesOnFailure: SuiteTrait, TestScoping {
    var isRecursive: Bool { true }

    func provideScope(for _: Test, testCase _: Test.Case?, performing function: @concurrent @Sendable () async throws -> Void) async throws {
        let captures = Captures()
        let outcome: Result<Void, any Error>
        do {
            outcome = .success(try await Captures.$current.withValue(captures) { try await function() })
        } catch {
            captures.failed.store(true, ordering: .relaxed)
            outcome = .failure(error)
        }
        if captures.failed.load(ordering: .relaxed) {
            for shot in captures.shots.withLock({ shots in shots }) { Attachment.record(shot.image, named: "window \(shot.frame)", as: .png) }
        }
        try outcome.get()
    }
}

@Suite(
    .serialized,
    .timeLimit(.minutes(1)),
    CapturesOnFailure(),
    .compactMapIssues { issue in
        if issue.isFailure { Captures.current?.failed.store(true, ordering: .relaxed) }
        return issue
    },
)
struct WindowServerTests {}
```

`Mutex` and `Atomic` come from `Synchronization`, `Screenshot` from `ScreenCapture`:
- Traits of one suite provide scopes in listed order, outermost first, so `CapturesOnFailure` binds `Captures.current` around the issue handler
- `.compactMapIssues` runs synchronously for each issue the test records, inside every scope listed before it, and returns each issue unchanged
- Errors a test throws pass through `provideScope`, and its `catch` marks them failed whether or not the handler saw them
- Tests call `Captures.current?.take(in: watch)` inside the session, as fixture windows close with it, and a refused capture records a warning issue that leaves the verdict alone
- `NonisolatedNonsendingByDefault` makes `provideScope` take its function as `@concurrent @Sendable`, and `withValue` takes a closure calling it, as the function passed whole selects a deprecated overload

## [08]-[INTERFACE_AUDITS]

`XCUIAutomation` drives and audits an app's own interface from a UI testing bundle, and library code that is itself an Accessibility client tests from Swift Testing bundles against `Fixture`:
- Xcode ships `XCUIAutomation.framework` beside `XCTest.framework`, whose module map excludes the moved headers and whose `XCTest.h` imports it
- `XCUIApplication` refuses a unit test bundle at run time ("use of XCUIApplication is not supported ... used in a unit test bundle instead of a UI test bundle")
- SwiftPM target types are `regular`, `executable`, `test`, `system`, `binary`, `plugin`, and `macro`, so no root `Package.swift` target hosts a UI testing bundle
- UI testing bundles sit in an app's Xcode project (`products/macos/rasad/Rasad.xcodeproj`), run inside `XCTRunner.app`, and build through `Rasm.xcworkspace`
- `XCUIApplication()` targets the bundle's Target Application setting, and `XCUIApplication(url:)` or `XCUIApplication(bundleIdentifier:)` names another app

`performAccessibilityAudit(for:_:)` is a throwing `@MainActor` member that records each issue it finds as a failure of the running test, unless its handler returns `true` for that issue:
- Audits on macOS are `.contrast`, `.elementDetection`, `.hitRegion`, `.sufficientElementDescription`, `.action`, and `.parentChild`, each an option of `.all`
- Audit types `.dynamicType`, `.textClipped`, and `.trait` exist on iOS, tvOS, and watchOS alone

`resetAuthorizationStatusForResource(_:)` resets contacts, calendars, reminders, photos, microphone, camera, media library, HomeKit, Focus, protected folders, volumes, and Apple Events for its app. No `XCUIProtectedResource` names Accessibility, Screen Recording, or posting events, so suites reading `Fixture` take those grants through TCC.
