---
name: cpp-coding-testing
description: "Use when writing or reviewing C++ tests, covering sources of truth, compile-time and rejection tests, cases, properties, fuzz targets, regression proof, and seams"
---

# [CPP_CODING_TESTING]

Tests state what code must do, from a source that code did not write. Snippets use Catch2 members.

[PRINCIPLES]:
- Expected values come from a contract, an analytic result, a second implementation, or an exemplar suite, never from code under test or its output
- Tests count once seen failing against code lacking their behavior, for the reason they state
- Failing tests are findings: no assertion, tolerance, case, or input class changes to pass, and no case is skipped
- Facts a compiler decides are asserted at compile time, behavior at run time
- Inputs get an accept direction and a reject direction
- Tests give one result on every run and every standard library
- Failure output names its input, expected value, and actual value
- Tests reach code through its public declarations, a test needing a private member shows a missing parameter or type
- Cases stand directly over a package's own types: no shared helper module, fixture base, or mock layer sits between a case and its subject

[REFERENCES]:
- [01]-[COMPILE_TIME](references/compile-time.md): Assertions in both phases, compiler facts, and paired probes
- [02]-[CASES](references/cases.md): Test units, reject tables, bounds, type classes, and whole-value matchers
- [03]-[PROPERTIES](references/properties.md): Round trips over bounds and seeded draws, reference paths, and fuzz targets
- [04]-[SEAMS](references/seams.md): Counted storage, order on one thread, and blocking waits

## [01]-[SOURCES]

Each behavior takes its expected value from the source stating it:

| [INDEX] | [BEHAVIOR]                         | [SOURCE]                                                    |
| :-----: | :--------------------------------- | :---------------------------------------------------------- |
|  [01]   | Value with a closed form           | Result worked from its formula, written as a literal        |
|  [02]   | Named value (zero, empty, a bound) | Documented sentence about it, as an equation                |
|  [03]   | Behavior a specification states    | Its section, each stated case as a row                      |
|  [04]   | Behavior an exemplar suite checks  | That suite's case, its expectation unchanged                |
|  [05]   | Optimized path                     | Plain reference written in its test, over one set of inputs |
|  [06]   | Reported defect                    | Reported input and reported symptom                         |

- Expectations name their source where a failure shows it: a contract sentence as the case name, a specification section or exemplar file as a tag
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
- Properties and fuzz targets asserting validity alone prove survival, each takes one oracle comparing a value

## [03]-[COMPILE_TIME]

Constant evaluation refuses core-language undefined behavior, and a `static_assert` over a call checks its value and its evaluation:
- Behavior of a `constexpr` function is asserted by `static_assert` and by the same call on a run-time value
- Facts with no run-time form (`noexcept`, a result type, a constraint, triviality) are `static_assert`s alone
- Probes pass arguments of a parameter's own type, a converted argument adds its conversion to the fact
- Negative probes sit in a concept over a dependent type, a `requires` expression over concrete types is ill-formed in place of `false`
- Negative probes stand beside a positive probe differing in the refused property alone
- `requires` cannot read a refusal by a `consteval` call or a `static_assert` in a body: it reads valid, or fails hard where a return type is deduced
- Refusals `requires` cannot read, and diagnostics a caller reads, are rejections: one refused statement beside an accepted one differing in its refused part
- Rejections expect their diagnostic at the refused statement, a failure of another kind is no pass
- Templates run per class of type argument their contract separates (trivially copyable, move-only, padded, floating)
- Use `references/compile-time.md` for both phases, compiler facts, and probes

## [04]-[CASES]

One case states one behavior of one public entity, named as the contract sentence it checks:

| [INDEX] | [INPUT]  | [CLASSES]                                                                       |
| :-----: | :------- | :------------------------------------------------------------------------------ |
|  [01]   | Text     | Empty, missing or repeated separator, sign, trailing bytes, digits past a range |
|  [02]   | Integer  | Zero, one, each bound, one past each bound                                      |
|  [03]   | Float    | Zero of each sign, subnormal, infinity, NaN, neighbors of a threshold           |
|  [04]   | Sequence | Empty, one element, duplicates, sorted, reversed, longer than a fixed buffer    |
|  [05]   | State    | Each state, each refused transition                                             |

- Test units cover one module or one partition of it
- Cases differing in a value are rows of one table, and independent behaviors are separate cases
- Fatal assertions guard values later lines read, non-fatal assertions state independent facts
- Assertions compare values their framework prints: an outcome is unwrapped behind a fatal assertion, and enumerations print by name
- Refusal cases assert the documented reason and that their subject kept its value
- Collections compare whole through a matcher
- Generated and tabled inputs are captured, so a failure prints the row that failed
- Runs count when their report lists the case as executed
- Use `references/cases.md` for test units, tables, and matchers

## [05]-[PROPERTIES]

Properties state an equation every input of a domain satisfies:

| [INDEX] | [PROPERTY]  | [EQUATION]                                                 |
| :-----: | :---------- | :--------------------------------------------------------- |
|  [01]   | Round trip  | Decode of an encoded value equals that value               |
|  [02]   | Invariant   | Stated relation holds after every operation                |
|  [03]   | Metamorphic | Reordered, rescaled, or split input gives a related output |
|  [04]   | Rejection   | Input outside a grammar is refused with its reason         |

- Domains are visited at each bound and hostile value by name, then by draws from an engine seeded by a literal
- Draws read an engine's own output, a standard distribution maps it differently on each standard library
- Fuzz targets enter at a function taking untrusted bytes, view their input buffer in place, and end their process when an oracle fails
- Fuzz oracles restate a contract without calling its implementation
- Inputs that fail a fuzz target become a row of a case and a corpus entry, failing in both before their fix
- Use `references/properties.md` for round trips, reference paths, and fuzz targets

## [06]-[ORDER]

New behavior:
1. Write one case from its contract
2. Run it, its failure names the missing behavior
3. Change code until it passes
4. Rebuild what that change left (a constant standing for a rule, a branch per case) with every case passing
5. Run every case of its package

Reported defect:
1. Write a case from the reported input and symptom
2. Run it against unfixed code, its failure matches the reported symptom
3. Fix the cause
4. Run it again, then search other entry points for that pattern and add a case for each

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

- Contracts about order are cases on one thread, a second thread enters where a contract states blocking
- Waiting threads count down a latch before their wait, and the awaited event follows that latch
- Concurrent cases end on a join and assert state their own thread owns
- Cases share no state and pass in any order
- Use `references/seams.md` for counted storage, order on one thread, and waits

## [08]-[COST]

Counts a contract states (storage through a passed resource, passes, rebuilds) are asserted in cases, and durations are reported by benchmarks:
- Benchmarks assert their result before timing it, on input shaped as production input is
- Optimized paths are compared with their reference at small, boundary, and large sizes
