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

`#expect(<a> == <b>)` expands to a closure type-checked against every `==` overload, past the build's `-warn-long-expression-type-checking-scopes` limit.

## [01]-[SUITES]

Suites hold `@Test` functions named for the behavior they check, and each test runs on a fresh suite instance:
- `init() async throws` sets up per-test state, and `deinit` of a `final class` or `~Copyable` suite tears it down
- Tests and parameterized cases run in parallel under `swift test`, and `.serialized` orders one suite's subtree while sibling suites keep running
- Tests sharing Accessibility focus, the frontmost application, or screen capture sit under one top-level suite holding `.serialized` and `.timeLimit`
- Tests differing only in input and expected value are one `@Test(arguments:)` over a collection, a pair of collections, or a `zip`
- Host grants a test needs enter as `try #require(<check>, "<grant>")`, and a missing grant fails the test
- Tests waiting on an event open a gate the child awaits (a FIFO from `mkfifo`) in place of a sleep or elapsed-time budget
- `.timeLimit(.minutes(<n>))` cancels cooperatively, and a body blocked in a wait that ignores cancellation (kqueue, semaphore, run loop) outlasts it
- Trait arguments take no `try`, and a throwing value enters inside the test body
- `@testable import` reaches an executable target's module

## [02]-[ASSERTIONS]

`expectNoDifference` from swift-custom-dump compares `Equatable` values and prints both as a line diff, with an optional message naming the case:
- Matcher form `#expect { } throws: { }` fails as deprecated under warnings as errors
- `#expect(throws: <error>)` compares an `Equatable` error value
- `#expect` records an issue and continues past a failed check
- Strict memory safety marks a call taking unsafe-typed arguments as `try unsafe #require(<call>)`
- `#expect(processExitsWith:observing:)` runs its body in a child copy of the test process with an explicit `Sendable & Codable` capture list
- `Attachment.record(<image>, named: <name>, as: .png)` records a `CGImage` or `NSImage` beside the test's results

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
- `provideScope` takes `performing function: @concurrent @Sendable () async throws -> Void` under `NonisolatedNonsendingByDefault`
- `isRecursive` false runs a suite trait once around its suite, and a test trait runs once per case
- Teardown runs in `withTaskCancellationShield` after taking the body's `Result`, records failures in `Issue.record`, then rethrows the body's failure
- Suites pass a library step to a trait as a function value
- Fixture bundles sit in the user caches folder LaunchServices resolves by bundle id
- Fixture executables read their scenario from launch arguments and exit with their owner process
- Tests that crash a fixture instance await its exited state, and teardown finds its crash report
