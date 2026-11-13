---
name: cpp-coding-testing
description: "Use when writing or reviewing C++ tests, covering sources of truth, compile-time and rejection tests, cases, properties, fuzz targets, regression proof, and seams"
---

# [CPP_CODING_TESTING]

Tests state what code must do, from a source that code did not write. Snippets use Catch2 members, and each rule holds under any framework.

[PRINCIPLES]:
- Expected values come from a contract, an analytic result, a second implementation, or an exemplar project, never from code under test or its output
- Tests count once seen failing against code lacking their behavior, for the reason they state
- Failing tests are right until evidence shows otherwise, and no assertion, tolerance, case, or input class changes to pass
- Facts a compiler decides are asserted at compile time, behavior at run time, and `constexpr` behavior at both
- Surfaces get an accept direction and a reject direction, and one assertion pins what a result means beyond its being well formed
- Tests give one result on every run: time, randomness, and I/O enter through parameters, seeds are literals, and waits end on an event
- Failure output names its input, expected value, and actual value, locating a defect without a rerun
- Tests reach code through its public declarations, a test needing a private member shows a missing parameter or type

[REFERENCES]:
- [01]-[COMPILE_TIME](references/compile-time.md): Static twins, compiler facts, negative probes, and rejection sources
- [02]-[CASES](references/cases.md): Test units, reject tables, bounds, type classes, and whole-value matchers
- [03]-[PROPERTIES](references/properties.md): Seeded round trips, reference paths, and fuzz targets
- [04]-[SEAMS](references/seams.md): Counted allocations, concurrent waits, and benchmarks

## [01]-[SOURCES]

Each behavior takes its expected value from the source stating it:

| [INDEX] | [BEHAVIOR]                         | [SOURCE]                                                      |
| :-----: | :--------------------------------- | :------------------------------------------------------------ |
|  [01]   | Value with a closed form           | Result worked from its formula, written as a literal          |
|  [02]   | Named value (zero, empty, a bound) | Documented sentence about it, as an equation                  |
|  [03]   | Encode and decode pair             | Round trip to its starting value                              |
|  [04]   | Optimized path                     | Plain reference written in the test, over the same inputs     |
|  [05]   | Paths documented as equivalent     | Agreement between them                                        |
|  [06]   | Reported defect                    | Reported input and reported symptom                           |
|  [07]   | Behavior a refactor keeps          | Case recorded before the change, named as recorded            |
|  [08]   | Recorded output                    | Earlier output, each difference reviewed as a behavior change |

- Recorded outputs detect change and prove no correctness, one regenerated to pass is a weakened test
- Tolerances are values a contract states, and an exact contract (equality, ordering, a round trip) takes none
- Checks leave with the property they verify: a deleted `static_assert`, rejection, or case has a replacement or holds by construction
- Tests of a removed feature go with it, a case asserting absence pins nothing

## [02]-[KINDS]

Each kind of test proves one class of fact:

| [INDEX] | [KIND]          | [PROVES]                                                                |
| :-----: | :-------------- | :---------------------------------------------------------------------- |
|  [01]   | `static_assert` | Types, constraints, `noexcept`, and values of constant expressions      |
|  [02]   | Rejection       | Misuse fails to compile with its diagnostic                             |
|  [03]   | Case            | Behavior for chosen inputs, memory and race errors on paths it executes |
|  [04]   | Property        | Equation over a generated domain                                        |
|  [05]   | Fuzz target     | Oracle holds and no crash or hang over mutated bytes                    |
|  [06]   | Benchmark       | Cost of one operation on representative input                           |

- Cases prove memory and race facts under a sanitizer alone, and a defect one sanitizer reports is seen failing under that sanitizer
- Properties and fuzz targets asserting validity alone prove survival, each surface takes one assertion pinning a value

## [03]-[COMPILE_TIME]

Constant evaluation refuses core-language undefined behavior, and a `static_assert` over a call checks its value and its evaluation:
- Behavior of a `constexpr` function is asserted by `static_assert` and by the same call on a run-time value
- Facts with no run-time form (`noexcept`, a result type, a constraint, triviality) are `static_assert`s alone
- Probes pass arguments of a parameter's own type, a converted argument adds its conversion to the fact
- Negative probes sit in a concept over a dependent type, a `requires` expression over concrete types is ill-formed in place of `false`
- Negative probes stand beside a positive probe differing in the refused property alone
- Refusals by a `consteval` call or a `static_assert` in a body read as valid to `requires`
- Rejection sources hold one refused statement per guard, after the accepted statement differing in its refused part
- Rejections expect their diagnostic at the guarded statement, a failure of another kind is no pass
- Templates run per class of type argument their contract separates (trivially copyable, move-only, padded, floating)
- Use `references/compile-time.md` for twins, probes, and rejection sources

## [04]-[CASES]

One case states one behavior of one public entity, named as a sentence a failure report stands on:

| [INDEX] | [INPUT]  | [CLASSES]                                                                       |
| :-----: | :------- | :------------------------------------------------------------------------------ |
|  [01]   | Text     | Empty, missing or repeated separator, sign, trailing bytes, digits past a range |
|  [02]   | Integer  | Zero, one, each bound, one past each bound                                      |
|  [03]   | Float    | Zero of each sign, subnormal, infinity, NaN, neighbors of a threshold           |
|  [04]   | Sequence | Empty, one element, duplicates, sorted, reversed, longer than a fixed buffer    |
|  [05]   | State    | Each state, each refused transition                                             |

- Cases differing in a value are rows of one table, sections share setup of one behavior, and independent behaviors are separate cases
- Fatal assertions guard values later lines read, non-fatal assertions state independent facts
- Refusal cases assert the documented reason and that their subject kept its value
- Collections and records compare whole through a matcher, enumerations and package types print by name
- Generated and tabled inputs are captured, so a failure prints the row that failed
- Runs count when they list a case as executed, a filter matching none passes nothing
- Skips state their reason
- Use `references/cases.md` for test units, tables, and matchers

## [05]-[PROPERTIES]

Properties state an equation every input of a domain satisfies:

| [INDEX] | [PROPERTY]  | [EQUATION]                                                 |
| :-----: | :---------- | :--------------------------------------------------------- |
|  [01]   | Round trip  | Decode of an encoded value equals that value               |
|  [02]   | Reference   | Optimized path equals a plain path over the same input     |
|  [03]   | Invariant   | Stated relation holds after every operation                |
|  [04]   | Metamorphic | Reordered, rescaled, or split input gives a related output |
|  [05]   | Rejection   | Input outside a grammar is refused with its reason         |

- Generators draw a parameter's whole documented domain, bounds and hostile values included, from an engine seeded by a literal
- Cases hold one property each
- Fuzz targets enter at the function taking untrusted bytes, call it within its contract, and end the process when an oracle fails
- Inputs that fail a fuzz target join its corpus and fail there before their fix
- Use `references/properties.md` for seeded properties and fuzz targets

## [06]-[ORDER]

New behavior:
1. Write one case from its contract
2. Run it, its failure names the missing behavior
3. Make the smallest change that passes it
4. Run every case of its package

Reported defect:
1. Write a case from the reported input and symptom
2. Run it against unfixed code, its failure matches the reported symptom
3. Fix the cause
4. Run it again, then search sibling entry points for the same pattern and add a case for each

- Cases written after their fix run once with that fix reverted
- Cases failing for another reason than their own are corrected before any code changes
- Refactors start from cases covering the kept behavior and edit no expectation

## [07]-[SEAMS]

Tests supply each dependency through the parameter code under test takes:

| [INDEX] | [DEPENDENCY] | [TEST_SUPPLIES]                                     |
| :-----: | :----------- | :-------------------------------------------------- |
|  [01]   | Time         | Fixed time point, or a clock type the test advances |
|  [02]   | Randomness   | Engine seeded by a literal                          |
|  [03]   | Memory       | Counting or bounded `std::pmr::memory_resource`     |
|  [04]   | Output       | `std::ostream&` or `std::span` over test storage    |
|  [05]   | Collaborator | Lambda or value of the concept a function accepts   |

- Doubles are values passed through existing parameters, a declaration added to reach private state goes
- Concurrent cases end on a stop request, a latch, or a join, and assert state their own thread owns
- Cases share no state and pass in any order
- Use `references/seams.md` for counted allocations and concurrent waits

## [08]-[COST]

Counts a machine does not change (allocations, passes, rebuilds) are asserted in cases, and durations are reported by benchmarks:
- Benchmarks assert their result before timing it, on input shaped as production input is
- Optimized paths keep a plain reference, compared at small, boundary, and large sizes
