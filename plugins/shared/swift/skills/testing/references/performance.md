# [PERFORMANCE]

Performance cases measure on the owner's Mac through XCTest metrics, and hosted runs check cost through counts that hold on every machine.

## [01]-[CASES]

Swift Testing declares no measuring member, and `.timeLimit` ends a hung test without judging its cost. Measurement runs in an `XCTestCase` subclass through `measure(metrics:options:block:)`:
- Root `Package.swift` test targets link XCTest with no dependency row, so `import XCTest` sits beside `import Testing` in any test target
- Performance classes sit alone in target `<Module>PerformanceTests`, which a test plan selects whole, and no Swift Testing test shares their process
- Blocks are synchronous and non-escaping, so they hold a parse, fold, walk, or encode, and setup runs before `measure`

```swift
final class LogLinePerformanceTests: XCTestCase {
    func testNewestReadsTheLastLinesOfALargeLog() throws {
        let folder = FileManager.default.temporaryDirectory.appending(component: UUID().uuidString)
        try FileManager.default.createDirectory(at: folder, withIntermediateDirectories: true)
        addTeardownBlock { try FileManager.default.removeItem(at: folder) }
        try Data((0..<1_000_000).map { index in "line \(index)\n" }.joined().utf8).write(to: folder.appending(component: "server.log"))
        let options = XCTMeasureOptions.default
        options.iterationCount = 10
        measure(metrics: [XCTClockMetric(), XCTCPUMetric(limitingToCurrentThread: true), XCTMemoryMetric()], options: options) {
            _ = LogLine.newest(in: [LogFolder(url: folder)], since: nil, limit: 10_000, where: { _ in true })
        }
    }
}
```

`LogLine.newest` from `Files` reads a log backward from its end, and the file is written once, outside every iteration.

## [02]-[METRICS]

Each metric reports the values a baseline compares, named as the test report shows them:

| [INDEX] | [METRIC]                                        | [REPORTS]                                      |
| :-----: | :---------------------------------------------- | :--------------------------------------------- |
|  [01]   | `XCTClockMetric()`                              | Clock Monotonic Time                           |
|  [02]   | `XCTCPUMetric(limitingToCurrentThread: true)`   | CPU Instructions Retired, CPU Cycles, CPU Time |
|  [03]   | `XCTCPUMetric()`                                | Same values over the whole process             |
|  [04]   | `XCTMemoryMetric()`                             | Memory Physical, Memory Peak Physical          |
|  [05]   | `XCTStorageMetric()`                            | Disk Logical Writes                            |
|  [06]   | `XCTOSSignpostMetric(subsystem:category:name:)` | Duration of one signpost interval              |

- Metric arrays are non-empty, and XCTest copies each metric for every iteration through `NSCopying`
- `limitingToCurrentThread: true` counts the thread running the block alone, with lower variance than the process form for single-threaded work
- Instructions retired vary least between runs of one Mac and differ between CPU designs, so they gate against that Mac's baseline alone
- `XCTOSSignpostMetric` strings come from the emitter's `OSSignposter` (Relay's `AccountStore` takes `Program.current.identifier` and `String(reflecting: AccountStore.self)`, with `AccountOperation.name` per interval)
- `XCTApplicationLaunchMetric`, `XCTHitchMetric`, and each `init(application:)` form read an `XCUIApplication`, which only a UI testing bundle drives, and root `Package.swift` declares none

## [03]-[OPTIONS]

`XCTMeasureOptions` sets how each block runs:
- `iterationCount` defaults to 5, and XCTest invokes the block once more and discards that first iteration as an outlier
- `invocationOptions` defaults to `[]`, measuring from the block's start to its end
- `[.manuallyStart, .manuallyStop]` measures from `startMeasuring()` to `stopMeasuring()`, so per-iteration setup and teardown run unmeasured
- Under manual start, each iteration calls `startMeasuring()` once and `stopMeasuring()` at most once, as a second call fails the test
- `XCTMeasureOptions.default` builds a new set of recommended options on each read, so a test sets fields on its own value

## [04]-[BASELINES]

Baselines record one Mac's accepted averages, and Xcode writes them from a test report's set-baseline action, as `xcodebuild` has no option that records one:
- Package test targets keep them under `.swiftpm/xcode/xcshareddata/xcbaselines/<TestTarget>.xcbaseline/`, a tracked folder, as `.gitignore` excludes `xcuserdata/` alone
- `Info.plist` maps each run destination UUID under `runDestinationsByUUID` to a `localComputer` record (model code, CPU kind, core counts, clock speeds, platform) and a `targetArchitecture`
- `<UUID>.plist` keys `classNames` by class, test method, and metric identifier to a `baselineAverage` with optional `maxPercentRegression`, `maxPercentRelativeStandardDeviation`, `maxRegression`, and `maxStandardDeviation`
- Builds copy a target's baselines into its test bundle at `Contents/Resources/xcbaselines`, and the test harness takes the record matching the running Mac and architecture
- Runs matching no record measure, report, and pass, as build setting `TREAT_MISSING_BASELINES_AS_TEST_FAILURES` defaults to `NO`
- `TREAT_MISSING_BASELINES_AS_TEST_FAILURES = YES` in `.config/xcode.xcconfig`, the file `XCODE_XCCONFIG_FILE` applies to package targets, fails such a case with "No baseline average for" its metric, so no performance case passes on a Mac lacking its baseline
- Macs of another model or CPU match no record and fail until a baseline of their own is set

## [05]-[HOSTED_RUNS]

Hosted runners match no baseline, so performance targets stay out of every hosted run and its cached result:
- Scheme `Rasm-Package` references `.config/Performance.xctestplan`, holding each `<Module>PerformanceTests` target, beside default plan `.config/Rasm-Package.xctestplan`
- `test--xcodebuild` runs the default plan and caches its result, and `test--xcodebuild-plans` runs each other plan under the Nx configuration of its name with no cache, so `nx test Rasm-Package -c performance` measures on the owner's Mac on every run and CI's `nx affected -t test` runs no other plan
- Test targets join a plan by name, so a target absent from both plans never runs

Other forms of leaving cases out fail one of those conditions:

| [INDEX] | [FORM]                                        | [OUTCOME]                                                                        |
| :-----: | :-------------------------------------------- | :------------------------------------------------------------------------------- |
|  [01]   | `-skip-testing:<target>` in the CI step       | Flag reaching every affected project's test task                                 |
|  [02]   | Testable marked `skipped="YES"` in scheme     | Target unreachable by `-only-testing` on every machine                           |
|  [03]   | `XCTSkip` under a CI variable                 | Skip reported, Nx caching the run as green                                       |
|  [04]   | Performance class in a functional test target | Swift Testing tests in the measured process, plan split by a hand-kept test list |
|  [05]   | Performance target in the default plan        | Hosted run passing with no baseline, or red under the setting                    |

## [06]-[COUNTS]

Hosted runs check cost through a count that is the same on every machine, with a relation oracle over input size:
- Operation counts: a member taking a closure (`where:`, a comparator, a pricing function) has its calls counted by the closure the test passes
- Cost relations: the count at `n` and at `k·n` states the bound (equal for work bounded by a limit, `k` times for one pass, more for a repeated walk)
- Retained blocks: `blocks_in_use` of `malloc_zone_statistics(nil, &statistics)` before and after a warmed call counts blocks it leaves live
- Heap reads run in an `#expect(processExitsWith:)` body, as parallel tests share one heap, and return through `\.standardOutputContent`

```swift
@Test
func newestFiltersOnlyTheLinesItKeepsWhateverTheFileLength() throws {
    let calls: [Int] = try [100, 10_000].map { length in
        let folder = FileManager.default.temporaryDirectory.appending(component: UUID().uuidString)
        defer {
            do { try FileManager.default.removeItem(at: folder) } catch { Issue.record(error) }
        }
        try FileManager.default.createDirectory(at: folder, withIntermediateDirectories: true)
        try Data((0..<length).map { index in "line \(index)\n" }.joined().utf8).write(to: folder.appending(component: "server.log"))
        var count = 0
        _ = LogLine.newest(in: [LogFolder(url: folder)], since: nil, limit: 10) { _ in
            count += 1
            return true
        }
        return count
    }
    #expect(calls == [10, 10])
}
```

`newest` stops at its limit while reading backward, so a file a hundred times longer calls `where:` as often, and a rewrite reading every line first turns the second count red.
