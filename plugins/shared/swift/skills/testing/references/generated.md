# [GENERATED]

`PropertyBased` runs property checks inside Swift Testing tests over composable generators, and prints a replayable seed with each failure.

## [01]-[CHECKS]

`await propertyCheck(count:input:perform:)` calls its body with new inputs `count` times, 100 by default, and stops at the first iteration that records an error:

```swift
@Test(arguments: [Shape.clicks, .stroke, .drag])
func pairsEveryPressWithOneReleaseOfItsEventNumber(shape: Shape) async {
    await propertyCheck(input: draws, hosts) { (draw: Draw, host: Host) in
        let pressing = input(shape, draw)
        let presses: [CGEvent] = events(try timeline(of: pressing, host), among: downs.union(ups)).map(\.event)
        let order: [Bool] = presses.map { (event: CGEvent) -> Bool in downs.contains(event.type) }
        expectNoDifference(order, Array(repeating: [true, false], count: pressing.schedule.count).flatMap(\.self))
    }
}
```

`InputTests` declares `draws` and `hosts` at file scope, each a `zip` of built-in generators mapped onto a record, and each argument case runs its own check:
- Inputs take 1 to 10 generators, each spread to one body parameter, and `zip` combines 2 to 10 into one tuple generator
- Failures print the mapped value through `String(describingForTest:)`, so a record names each drawn field
- Iterations each seed a new `Xoshiro` from `SystemRandomNumberGenerator`, so every run draws new inputs and prints the seed it failed on
- Errors thrown out of a body fail their iteration from 2.1.0, recorded at the check's call, and earlier releases discard them as a pass
- Calls outside a running test stop on a precondition, and `count: 0` returns before any draw, a check that asserts nothing

`propertyCheck` reads a task-local value of each trait below at entry, bound by its `provideScope` around the test function, so a trait reaches every check its test awaits:

| [INDEX] | [TRAIT]                                                        | [ON]        | [BINDS]                                          |
| :-----: | :------------------------------------------------------------- | :---------- | :----------------------------------------------- |
|  [01]   | `.fixedSeed("<base64>")`, `.fixedSeed(<s1>, <s2>, <s3>, <s4>)` | Test        | Generator state of one replayed iteration        |
|  [02]   | `.shrinking`, `.shrinking(false)`                              | Test, suite | Shrinking on or off                              |
|  [03]   | `.maximumAttempts(<n>)`                                        | Test, suite | Draws a filtering generator makes for each value |

## [02]-[GENERATORS]

Built-in generators each pair a draw with a shrinker:

| [INDEX] | [GENERATOR]                                      | [DRAWS]                                 | [SHRINKS]                                 |
| :-----: | :----------------------------------------------- | :-------------------------------------- | :---------------------------------------- |
|  [01]   | `Gen.int(in:)`, `Gen.int64(in:)`, `Gen.uint64()` | Integer in range, full width by default | Toward zero within range                  |
|  [02]   | `Gen.double(in:)`                                | Float in a finite range                 | Toward zero within range                  |
|  [03]   | `Gen.bool`, `Gen.bool(<rate>)`                   | `true` at a rate, half by default       | To `false`                                |
|  [04]   | `.array(of:)`, `.string(of:)`                    | Count from a range, then each element   | Shorter to lower bound, then each element |
|  [05]   | `.set(ofAtMost:)`, `.dictionary(ofAtMost:)`      | Array with duplicates dropped           | As an array                               |
|  [06]   | `.optional`, `.optional(valueRate:)`             | `nil` in a quarter of draws by default  | Inner value, then `nil`                   |
|  [07]   | `zip(...)`, `.pair`, `.simd2`                    | Tuple or vector of independent draws    | One position at a time                    |
|  [08]   | `Gen.oneOf(...)`, `Gen.frequency(...)`           | One generator, equal or weighted        | Within the chosen generator               |
|  [09]   | `.map`, `.compactMap`, `.filter`                 | Transformed or accepted value           | Underlying input, rejects skipped         |
|  [10]   | `Gen.always`, `Gen<T>.case`, `Gen.element(of:)`  | Constant, case, or element              | None                                      |

- Built-ins combined through `zip` and `map` combine their shrinkers, and `Generator(run:)` declares a value with none, so a record maps from a `zip`
- Strings shrink by length alone, as character generators hold no shrinker
- Uniform draws reach a limit rarely, and `Gen.frequency` rows of `Gen.always(<limit>)` give limits and hostile values a stated share of draws
- `rows` in `ResultsTests` weights an empty refusal list `2` against `1`, so 2 draws in 3 take the success path
- `Gen.letterOrNumber` draws ASCII letters and digits alone, `Gen.character(in:)` one scalar per character, so combining marks enter as `Gen.always` rows
- Sets and dictionaries hold at most their count, fewer as draws collide (`Gen<Modifier>.case.set(ofAtMost: 0...3)` over five cases)
- No member counts which partition a check reached; a test drawing values through `Generator.run(using:)` from a seeded `Xoshiro` counts them

## [03]-[ATTEMPTS]

`filter` and `compactMap` draw again for each rejected value, 10,000 times per value unless `.maximumAttempts(<n>)` binds another limit. Exhausted draws record `GeneratorError.runLimitExceeded` with their seed and fail the check:
- Valid inputs come from construction, a `map` over ranges holding only valid values, and a filter stays for rejections a draw rarely meets
- Limits raised after a run limit failure keep a generator that draws mostly invalid values

## [04]-[SHRINKING]

Shrinking is on in this workspace: package trait `EnableShrinking` is a default trait under Swift 6.2 and later, and root `Package.swift` names no traits for its row. `.shrinking(false)` turns it off for a test or suite:
- Failing iterations record no issue while shrinking runs, and each candidate runs the body with its issues dropped, a failing one replacing the input
- Searches end when no candidate fails, and a final run over shrunk input records its issues, with both inputs and step count in its message
- Bodies run once per candidate, so effects outside the process (files) repeat, and checks sharing such state take `.shrinking(false)`
- Candidates pass generator filters and an array's lower count, so each shrunk input is one the generator can draw
- Bodies recording only warnings report "Warning occured with input" at warning severity and pass

## [05]-[REPLAY]

Failures print generator state from before the failing iteration as a base64 seed:

```text
Failure occured with input <value>.
(shrunk down from <value> after <n> iterations)

Add `.fixedSeed("<base64>")` to the Test to reproduce this issue.
```

`.fixedSeed("<base64>")` replays one iteration from that state in every check of its test and ignores `count`:
- Replays record every issue of the test with warnings raised to errors, or "A fixed seed was used, but no expectation failure occured" when none
- Tests carrying the trait never pass, and package documentation says never to commit it
- Seeds reproduce an input only while generator expressions, package version, and dependencies stay unchanged
- Malformed seeds and a second `.fixedSeed` record an issue and run no body
- `PropertyBased` keeps no failure store, and a seed exists only in one run's issue text

Counterexamples become named example tests:
1. Add the printed `.fixedSeed` to its failing test, run it alone, and read its shrunk input
2. Write a named `@Test` holding the shrunk input as literal arguments, and run it red before any fix
3. Remove `.fixedSeed`, and add the counterexample's class to its check's generator as a `Gen.frequency` row
4. Fix code until both named test and property check pass

## [06]-[SEEDED_SOURCES]

Production members take their random source as `inout some RandomNumberGenerator`, and tests pass a `Xoshiro`, which yields one sequence per seed:

```swift
private let hosts = zip(Gen.int(in: 100...900), Gen.int(in: 100...600), Gen.int(in: 15...100), Gen.int(in: 4...34), Gen.int64(in: 0...10_000), Gen.uint64())
    .map { (host: (Int, Int, Int, Int, Int64, UInt64)) -> Host in
        Host(interval: .milliseconds(host.0), delay: .milliseconds(host.1), repeatInterval: .milliseconds(host.2), frame: .milliseconds(host.3), first: host.4, seed: host.5)
    }

private func timeline(of input: Input, _ host: Host) throws(ApplicationFailure) -> [(offset: Duration, event: CGEvent)] {
    var generator = Xoshiro(seed: (host.seed, 1, 2, 3))
    return try input.timeline(doubleClick: host.interval, keyRepeat: (host.delay, host.repeatInterval), frame: host.frame, layout: layout, numbering: (host.first, 1), using: &generator)
}
```

`Input.timeline` draws click jitter from the generator it takes:
- `Gen.uint64()` draws the seed as a field of `Host`, so failures print the random source and `.fixedSeed` replays it
- `Xoshiro(seed:)` takes four `UInt64` words, and its base64 `String` form returns `nil` when malformed
- Zero in all four words yields zero forever, so words fixed beside a drawn one stay nonzero
- Example tests seed a literal (`Xoshiro(seed: (1, 2, 3, 4))` in `pacesPressesOfAKeyAtTheKeyRepeatIntervalWhereNoEveryIsGiven`)
