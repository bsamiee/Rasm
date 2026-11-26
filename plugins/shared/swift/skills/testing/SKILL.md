---
name: testing
description: "Use when writing, running, or debugging Swift tests, covering Swift Testing traits, expectations, arguments, exit tests, attachments, generated inputs, tests that read or drive another process, privacy grants, XCTest performance metrics, and CI"
---

# [SWIFT_TESTING]

Covers tests of root `Package.swift` test targets, which `xcodebuild` runs through `Rasm.xcworkspace` with Swift Testing as runner and XCTest for measurement alone. Use `code:test` for test design: oracles, cases, structure, independence, doubles, generated-input kinds, and grading.

Tests use each module below before a hand form:

| [INDEX] | [FIELD]                           | [MODULES]                                                                    |
| :-----: | :-------------------------------- | :--------------------------------------------------------------------------- |
|  [01]   | Runner, expectations, attachments | `Testing`, with `AppKit`, `CoreGraphics`, `CoreImage`, `Foundation` overlays |
|  [02]   | Structured comparison             | `CustomDump` (`expectNoDifference`)                                          |
|  [03]   | Generated inputs                  | `PropertyBased`                                                              |
|  [04]   | Controlled time                   | `Clocks` (`ImmediateClock`, `TestClock`)                                     |
|  [05]   | Issues library code reports       | `IssueReportingTestSupport`                                                  |
|  [06]   | Processes under test              | `Fixture` executable, `FixtureSession` over `Subprocess`                     |
|  [07]   | Measurement                       | `XCTest` metrics                                                             |

[REFERENCES]:
- [01]-[GENERATED](references/generated.md): Generators, property checks, seeds, shrinking, and replayed counterexamples with `PropertyBased`
- [02]-[INTERFACE](references/interface.md): `Fixture`, outside reads, shared session state, event order, drawn images, failure captures, and interface audits
- [03]-[PRIVACY](references/privacy.md): Privacy grants (TCC): services, databases, clients, requirements, Automation consent, and grants on a Mac
- [04]-[PERFORMANCE](references/performance.md): XCTest metrics, baselines, the performance test plan, and operation counts hosted runs check
- [05]-[CI](references/ci.md): Test selection and plans, hosted image, grant step, cached skips, result bundles, and flaky-test hunts

## [01]-[RUNNING]

`nx test Rasm-Package` runs `xcodebuild test` of shared scheme `Rasm-Package`, whose default plan `.config/Rasm-Package.xctestplan` lists every functional test target:
- Bundles load one after another, each into its own `xctest` agent of the selected Xcode, and Swift Testing runs a bundle's tests in parallel
- Builds before testing compile every scheme target, so one target that fails to compile fails every test
- Words after `--` join each `xcodebuild` run, `-only-testing:<id>` and `-skip-testing:<id>` running or excluding a target, suite, or test (`nx test Rasm-Package -- -only-testing:<target>/<suite>/<test>()`)
- `Package.swift` adds `IssueReportingTestSupport` to every test target, so `expectNoDifference` and `reportIssue` record Swift Testing issues
- Tell the owner before a run that launches `Fixture`, as its windows open on the owner's screen

## [02]-[TRAITS]

Traits state a test's conditions on its declaration:

| [INDEX] | [TRAIT]                              | [USE]                                                                    |
| :-----: | :----------------------------------- | :----------------------------------------------------------------------- |
|  [01]   | `.timeLimit(.minutes(<n>))`          | Hang guard on a suite waiting for a process or stream                    |
|  [02]   | `.serialized`                        | Suite whose tests share process-wide or window server state              |
|  [03]   | `.enabled(if:)`, `.disabled(if:)`    | Fact a correct run on a supported Mac lacks, never a grant CI can supply |
|  [04]   | `.bug(<url>, id: <id>)`              | Issue tracking behavior a `withKnownIssue` body asserts                  |
|  [05]   | `TestScoping` conformance            | State each test scopes through a task local                              |
|  [06]   | `.compactMapIssues`, `.filterIssues` | Context added to an issue, never a failure dropped                       |

- `TimeLimitTrait.Duration` marks `.seconds` and finer units unavailable, and a test past its limit records `timeLimitExceeded`
- `.serialized` orders its suite's tests and nested suites, while separate suites run beside it

## [03]-[EXPECTATIONS]

Each fact takes the expectation that states it:

| [INDEX] | [FACT]                              | [FORM]                                                    |
| :-----: | :---------------------------------- | :-------------------------------------------------------- |
|  [01]   | Fact later steps need               | `try #require(<condition>)`, `try #require(<optional>)`   |
|  [02]   | Fact checked, test continuing       | `#expect(<condition>, <comment>)`                         |
|  [03]   | Structured value                    | `expectNoDifference(<got>, <want>)`                       |
|  [04]   | Failure of a type                   | `#expect(throws: <Error>.self) { ... }` returning `E?`    |
|  [05]   | Failure equal to a value            | `#expect(throws: <error>) { ... }` over `Equatable` error |
|  [06]   | No failure                          | `#expect(throws: Never.self) { ... }`                     |
|  [07]   | Process exit                        | `#expect(processExitsWith: <condition>) { ... }`          |
|  [08]   | Cause caught outside an expectation | `Issue.record(<error>)`                                   |
|  [09]   | Behavior known to be wrong          | `withKnownIssue { ... }` under `.bug(...)`                |

- Typed reasons assert on the error `#expect(throws:)` returns, as its `performing:throws:` matcher overloads are deprecated in favor of that result
- `#expect` prints the operands of a comparison it holds, and a `Bool` bound beforehand prints its name alone, so its `Comment` interpolates every value a reader needs
- Comments interpolated inside `#require` exceed the 200 type-check scopes root `Package.swift` allows, so tests pass a bound `Bool` and `Comment`
- `withKnownIssue` passes while its body records an issue and records `knownIssueNotRecorded` once the behavior is fixed, with `isIntermittent` left `false`
- `Test.cancel(_:)` and `Issue.record(_:severity: .warning)` end or mark a test with no failing verdict

## [04]-[ARGUMENTS]

`@Test(arguments:)` runs one case per element and reports each case alone:
- One collection runs each element, a pair of collections runs their Cartesian product, and `zip(<first>, <second>)` pairs elements
- Tuple elements spread across parameters of their arity
- Arguments are `Sendable`, held in file-scope `private let` tables with each partition written out (`refusals` in `InputTests`)
- Xcode reruns one case alone where its element is `Encodable`, `CustomTestArgumentEncodable`, or `Identifiable` or `RawRepresentable` over `Encodable`
- Elements whose values name nothing hold a `label` field, and failures print it (`unnamed`, `drags` in `PointerTests`)

## [05]-[ASYNC]

Async tests await each event they assert and observe absence by ending its source:
- `confirmation(_:expectedCount:)` takes an `Int`, a range, or `0`, and counts confirmations before its body returns, so the body awaits a later callback
- Spies are `AsyncStream.makeStream()` continuations a double yields to, finished after the act and reduced, so a count of zero is exact (`MatchingTests`)
- Work that must not finish on its own sleeps an hour and yields to a stream once cancelled (`pending` in `ResultsTests`), making cancellation an asserted event
- Members that wait take `some Clock<Duration>`: `ImmediateClock` returns each sleep at once and advances `now`, `TestClock` moves through `advance(by:)`
- Asserted instants prove which path ran: `expiresAtTheTimeoutAndCancelsTheWork` expects `ImmediateClock<Duration>.Instant(offset: .seconds(5))`

## [06]-[FIXTURES]

Tests own what they create, under a name no other test uses, and release it when their scope ends:

```swift
private struct TemporaryBundle: ~Copyable {
    let url: URL

    init(identifier: String) throws {
        url = FileManager.default.temporaryDirectory.appending(components: UUID().uuidString, "Fixture.app")
        let contents = url.appending(component: "Contents")
        try FileManager.default.createDirectory(at: contents, withIntermediateDirectories: true)
        try PropertyListSerialization.data(fromPropertyList: ["CFBundleIdentifier": identifier], format: .xml, options: 0).write(to: contents.appending(component: "Info.plist"))
    }

    deinit {
        do { try FileManager.default.removeItem(at: url.deletingLastPathComponent()) } catch { Issue.record(error) }
    }
}
```

`ScriptingDictionariesTests` and `RequestTests` hold this shape over bundles with scripting definitions:
- `~Copyable` gives the resource one owner, and its `deinit` runs at the end of the binding's scope, after any await past its last use
- Cleanup failures reach `Issue.record` in place of `try?`
- Shared namespaces take a unique instance: a `UUID` folder under `temporaryDirectory`, or `.itemReplacementDirectory`
- Pasteboard tests write `NSPasteboard.withUniqueName()` and release it with `releaseGlobally()`
- `@testable import` constructs internal inputs (`Input`, `InstanceState.Window` decoded from JSON rows), and assertions read members callers use

## [07]-[EXIT_TESTS]

`#expect(processExitsWith:observing:)` runs its body in a child process of the test runner, for paths that end a process (`precondition`, `fatalError`, `exit`):
- Conditions are `.success`, `.failure`, `.exitCode(<code>)`, and `.signal(<signal>)`
- `observing: [\.standardOutputContent, \.standardErrorContent]` keeps output in the `ExitTest.Result` the macro returns, empty otherwise
- Bodies take outside values through a capture list, each `Codable` and `Sendable`

## [08]-[ATTACHMENTS]

`Attachment.record(<value>, named:)` adds a value to a test's results, which `xcodebuild` writes into the result bundle:
- `Data`, `String`, and `[UInt8]` record as their bytes, an `Encodable` type declaring `Attachable` as JSON through the `Foundation` overlay
- `Attachment(contentsOf: <url>)` reads a file the test wrote (a recorded movie)
- `CGImage`, `NSImage`, and `CIImage` record with `as: .png`, `.jpeg(withEncodingQuality:)`, or `AttachableImageFormat(contentType:)`
