# [TESTING]

Suites are Swift Testing types, and the fact a test checks selects the assertion:

| [INDEX] | [FACT]                        | [FORM]                                                                   |
| :-----: | :---------------------------- | :----------------------------------------------------------------------- |
|  [01]   | Value equals another          | `expectNoDifference(<actual>, <expected>)`                               |
|  [02]   | Value a later step reads      | `try #require(<optional>)`                                               |
|  [03]   | Boolean member holds          | `#expect(<value>.contains(<element>))`, `#expect(!<value>.isEmpty)`      |
|  [04]   | Step fails with a typed error | `#expect(throws: E.self)` returning `E?`, `try #require(throws: E.self)` |
|  [05]   | Rule holds over every input   | `propertyCheck(input:)` over `Gen` values                                |
|  [06]   | Process exits or crashes      | `#expect(processExitsWith: <condition>)`                                 |
|  [07]   | Expected events arrive        | `confirmation(expectedCount:)` around the awaited producer               |

`#expect(<a> == <b>)` expands to a closure type-checked against every `==` overload, past the build's scope limit.

## [01]-[SUITES]

Suites hold `@Test` functions named for the behavior they check, and each test runs on a fresh suite instance:
- `init() async throws` sets up per-test state, and `deinit` of a `final class` or `~Copyable` suite tears it down
- Tests and parameterized cases run in parallel under `swift test`, and `.serialized` orders one suite's subtree while sibling suites keep running
- Tests sharing Accessibility focus, the frontmost application, or screen capture sit under one top-level suite holding `.serialized` and `.timeLimit`
- `@Test(arguments:)` takes a collection, a pair of collections for their Cartesian product, or a `zip` for element pairs
- `.enabled { await <check> }` gates a test on a host capability, and `try Test.cancel(<reason>)` ends one whose fact appears at run time with no failure
- `.timeLimit(.minutes(<n>))` cancels cooperatively, and a body blocked in a wait that ignores cancellation (kqueue, semaphore, run loop) outlasts it
- Trait arguments take no `try`, and a throwing value enters inside the test body
- `@testable import` reaches an executable target's module (`watch_apps`)

## [02]-[ASSERTIONS]

`expectNoDifference` from swift-custom-dump compares `Equatable` values and prints both as a line diff, with an optional message naming the case:
- `#expect(throws:)` returns the caught error typed, and the matcher form `#expect { } throws: { }` fails as deprecated under warnings as errors
- `#expect(throws: <error>)` compares an `Equatable` error value
- `#require` stops the test at an absent value, `#expect` records an issue and continues
- Strict memory safety marks a call taking unsafe-typed arguments as `try unsafe #require(<call>)`
- `#expect(processExitsWith: .signal(SIGABRT))` runs its body in a child copy of the test process, captures values through an explicit `Sendable & Codable` capture list, and reads output named in `observing:`
- `Attachment.record(<image>, named: <name>, as: .png)` records a `CGImage` or `NSImage`, and targets write each into `.artifacts/swift/{test,test-integration}/<project>`

## [03]-[GENERATED_INPUTS]

`propertyCheck` from swift-property-based runs its closure over values `Gen` combinators build, shrinks a failing input, and prints a `.fixedSeed(<seed>)` trait replaying it:

```swift
@Test
func acceptsQuantitiesInRange() async {
    await propertyCheck(input: Gen.int(in: -2_000...2_000)) { quantity in
        expectNoDifference(Bounded<1, 999>(rawValue: quantity)?.rawValue, (1...999).contains(quantity) ? quantity : nil)
    }
}
```

- `input:` takes a pack of generators, and the closure takes one value per generator
- `Xoshiro` is a seeded `RandomNumberGenerator` for a function taking `inout some RandomNumberGenerator`

## [04]-[SCOPED_RESOURCES]

Resources a test reads come from a trait: a `SuiteTrait & TestTrait & TestScoping` type binds the resource to a `@TaskLocal` in `provideScope`, runs the test inside, and tears the resource down after the test passes, throws, or is cancelled:
- `provideScope` takes `performing function: @concurrent @Sendable () async throws -> Void`, the requirement's spelling under `NonisolatedNonsendingByDefault`
- `isRecursive` false runs a suite trait once around its suite, and a test trait runs once per case
- Teardown runs under `withTaskCancellationShield` after the body's `Result` is held, records each failure through `Issue.record`, then rethrows the body's failure

`TestSupport` traits and functions:

| [INDEX] | [MEMBER]                          | [PROVIDES]                                                                                        |
| :-----: | :-------------------------------- | :------------------------------------------------------------------------------------------------ |
|  [01]   | `.application(<Entry>.self, ...)` | Fixture bundle per case in `TestApplication.current`, removed with its defaults and crash reports |
|  [02]   | `.testDirectory`                  | Canonical temporary folder per case in `TestDirectory.current`, removed after the test            |
|  [03]   | `builtProduct(named:)`            | URL of a built executable, `TestFailure` when absent                                              |
|  [04]   | `jsonLines(of:arguments:as:)`     | Decoded output lines, termination status, and standard error of a built executable                |

- Suites pass a library step to a trait as a function value
- Fixture bundles sit in the user caches folder LaunchServices resolves by bundle id, `indexed: true` moves one to Spotlight-indexed `/Users/Shared`
- Fixture executables under `tests/swift/fixtures/<name>` read their scenario from launch arguments a `Settings` value writes and exit with their owner process
- Tests that crash a fixture instance await its exited state, and teardown finds its crash report
