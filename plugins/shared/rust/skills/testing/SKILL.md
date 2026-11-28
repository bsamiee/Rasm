---
name: testing
description: "Use when writing, editing, fixing, strengthening, pruning, or grading Rust tests, covering flaky tests, fixtures, rstest, proptest, insta, nextest, doubles, real services, and cargo-mutants"
---

# [RUST_TESTING]

Covers Rust tests that `cargo nextest` runs, `cargo insta` checks, and `cargo mutants` grades. Use `code:test` for test design. Use `rust:coding` for the code rules test code keeps.

Tests use each crate below before a hand-written equivalent:

| [INDEX] | [FIELD]                | [CRATES]                                                |
| :-----: | :--------------------- | :------------------------------------------------------ |
|  [01]   | Runner                 | `cargo-nextest`                                         |
|  [02]   | Table-driven tests     | `rstest`                                                |
|  [03]   | Generated inputs       | `proptest`, `proptest-state-machine`, `proptest-derive` |
|  [04]   | Snapshots              | `insta`, `cargo-insta`, `insta-cmd`                     |
|  [05]   | Mutation               | `cargo-mutants`                                         |
|  [06]   | HTTP peers             | `wiremock`                                              |
|  [07]   | Services in containers | `testcontainers`, `testcontainers-modules`              |
|  [08]   | Controlled time        | `tokio` with `test-util`                                |
|  [09]   | Scripted byte peers    | `tokio-test`                                            |
|  [10]   | Folders                | `tempfile`                                              |
|  [11]   | Captured events        | `tracing-test`                                          |
|  [12]   | Diffs and tolerances   | `similar-asserts`, `approx`                             |
|  [13]   | Benchmarks             | `criterion`                                             |
|  [14]   | Custom harnesses       | `libtest-mimic`                                         |

[REFERENCES]:
- [01]-[TABLES](references/tables.md): Table-driven tests
- [02]-[GENERATED](references/generated.md): Properties and state-machine models
- [03]-[SNAPSHOTS](references/snapshots.md): Snapshot tests
- [04]-[BOUNDARIES](references/boundaries.md): Code at a boundary
- [05]-[FIXTURES](references/fixtures.md): Shared test infrastructure
- [06]-[SUITES](references/suites.md): Test packages and runner configuration
- [07]-[MUTATION](references/mutation.md): Mutation runs and their outcomes

Placeholders mean one thing each across these files: `<crate>` the crate under test, `<package>` the package holding its tests, `<target>` that package's test target, `<module>` a module of cases over one subject, `<function>` a test function, `<row>` an `rstest` row's name, `<test>` one test's whole name, `<source>` a production file, `<seconds>` a per-mutant timeout.

## [01]-[CYCLE]

Each test is seen red for its named reason before code makes it green, once `code:test` steps 1 to 6 fixed its source, partitions, and oracle:
1. Signature: declare the item with a body returning a value the test names as wrong (`Ok(0)`, `Vec::new()`), as a compile error or an early panic is no red
2. Red: run `cargo nextest run -p <package> -E 'test(/^<module>::<function>(::|$)/)'`, and read the assertion's left and right
3. Green: write the least code that passes, then rerun step 2's command, green on that test
4. Next: add the next row and read its first run, where red returns to step 3 and green keeps the row only for a partition no other row holds
5. Proof: run `git add --intent-to-add` on each new file and `git diff --default-prefix <base> > <diff>` (`<base>` the commit the change started from), then the mutation command with `--in-diff <diff>`
6. Survivors: each missed mutant takes a case through Strengthening, and the built binary runs once through its own entry point

`no tests to run` (exit status 4) reads as a filter matching no name, a test file no `mod` line declares, or a package the profile's `default-filter` drops.

Tests written for code that already exists take their red from a break:
- `cargo mutants --re '<mutant>'` with the test's filter applies one break per mutant, `<mutant>` a line `cargo mutants --list` prints
- Breaks no mutant makes (a changed literal, a deleted call, swapped arguments) take a hand edit of `<source>`, restored from a copy after its red run

Mutation runs follow a green `cargo nextest run` over every package `test_package` names, `.cargo/mutants.toml` holding the choices from the mutation reference:

```bash
cargo mutants --baseline=skip --timeout <seconds> --file <source> -- -E 'test(/^<module>::<function>(::|$)/)'
```

`<seconds>` takes cargo-mutants' own rule (five times the suite's run, at least 20), and a run without `-- -E ..` tests the whole suite.

## [02]-[FAILURES]

Red tests change code or test once the behavior's source decides which is wrong:
1. Reproduce: run `cargo nextest run -p <package> -E 'test(=<test>)' --no-capture`, and read the panic's location or the printed error
2. Decide: compare the failing value with the behavior's source, where a wrong value sends the fix to code and a changed contract to the test
3. Rerun: step 1's command green after the fix

Flaky tests take a stress run as their red:
1. Alone: run `cargo nextest run -p <package> -E 'test(=<test>)' --stress-count <count>`, where one red run shows nondeterminism inside the test
2. Beside siblings: run the same with `-E 'binary_id(<package>::<target>)'`, where red only here shows an outside resource siblings share
3. Fix: move the uncontrolled input (a clock, a port, an iteration order, a shared object) to a parameter or a controlled double, as a retry hides it
4. Rerun: the stress run that went red, now green across every repetition

## [03]-[ASSERTIONS]

Bodies return `Result<(), E>`, `E` the one error their `?` steps meet, and a test-local `Failure` where they meet more than one:

```rust
#[derive(Debug, thiserror::Error)]
enum Failure {
    #[error(transparent)]
    Io(#[from] io::Error),
    #[error(transparent)]
    Size(#[from] SizeError),
    #[error("{file}: {source}")]
    Count { file: PathBuf, source: ParseIntError },
}
```

- Failing bodies print `Error: <Debug>` with no line, and each variant holds the subject it refused, so a foreign error naming no input (`ParseIntError`) takes a variant holding it
- `thiserror::Error` reads each field, where a `Debug` derive alone leaves them unread under `dead_code`
- `Option` values a later step needs bind through `.ok_or(<variant>)?`, and an expected error a step reads through `let Err(error) = &got else { .. }`
- `claims` stays out as a second form of `?` and `assert_matches!`, and a variant's subject names the input where its panic named a line
- `assert_matches!` is stable at `std::assert_matches` and takes `use std::assert_matches;`

Each fact takes the assertion that prints it whole, its message interpolating the case's inputs:

| [INDEX] | [FACT]           | [FORM]                                                                                           |
| :-----: | :--------------- | :----------------------------------------------------------------------------------------------- |
|  [01]   | Value            | `assert_eq!(got, want, "<inputs>")` over the whole record or `Result`                            |
|  [02]   | Facts of one act | `assert_eq!((status, stdout), (Some(0), want), "<inputs>")`                                      |
|  [03]   | Variant in part  | `assert_matches!(got, Err(Error::Http(error)) if error.status() == ..)`                          |
|  [04]   | Unordered result | `BTreeSet` equality, `Itertools::counts` for a multiset                                          |
|  [05]   | Large value      | `similar_asserts::assert_eq!`, printing a line diff                                              |
|  [06]   | Float            | `assert_abs_diff_eq!` for a stated epsilon or a value near zero, `assert_relative_eq!` otherwise |
|  [07]   | Generated case   | `prop_assert_eq!(got, want, "<inputs>")`                                                         |

Domain assertions take `#[track_caller]`, and a failure points at the calling test's line:

```rust
#[track_caller]
fn assert_refusals(texts: &[&str], want: &[SizeError]) {
    assert_eq!(refused(texts.iter().copied()).into_iter().counts(), want.iter().cloned().counts(), "refusals of {texts:?}");
}
```

`refused` returns the refusal of each text that fails to parse, and `Itertools::counts` compares refusals as a multiset.

Each form on the left hides what broke:

| [INDEX] | [FORBIDDEN]                                         | [CORRECT]                                                                   |
| :-----: | :-------------------------------------------------- | :-------------------------------------------------------------------------- |
|  [01]   | `assert!(matches!(..))`, `assert!(a == b)`          | `assert_matches!`, `assert_eq!`, printing the value                         |
|  [02]   | `.contains("<word>")` on rendered error text        | Variant and fields of the typed error                                       |
|  [03]   | `to_string()` of a value compared with text         | Comparison of the value, or the whole derived `Display` users read          |
|  [04]   | Independent cases sharing one `for` loop and check  | `#[case::<row>]` rows                                                       |
|  [05]   | `len() == len() && all(contains)`                   | Set or `counts` equality, as the hand form passes `[A, A]` against `[A, B]` |
|  [06]   | `if let Ok(..)` or `if let Some(..)` around a check | `?`, or `assert_matches!` on the absent case                                |

## [04]-[SMELLS]

Test smells name a test that decides nothing or reports a wrong verdict. Each kind keeps its `code:test` meaning, the Rust sign below finds it, and its action decides what Editing does with it:

| [INDEX] | [KIND]          | [RUST_SIGN]                                                                                            | [ACTION]     |
| :-----: | :-------------- | :----------------------------------------------------------------------------------------------------- | :----------- |
|  [01]   | Tautological    | Expected value whose one source is production text (`steps(&plan)`, a pasted script)                   | Delete       |
|  [02]   | Vacuous         | Dropped result of a poll or `timeout`, a spawned handle never joined, a file no `mod` line declares    | Delete       |
|  [03]   | Weak            | `is_some()`, `!is_empty()`, `len() == n`, or a substring as the one check of the claim a name states   | Delete       |
|  [04]   | Echo            | `assert_eq!(stub_answer, got)` with no call of crate code between                                      | Delete       |
|  [05]   | Blind negative  | `Err(_)`, `is_err()`, `#[should_panic]` with no `expected`, a wildcard over fields telling paths apart | Delete       |
|  [06]   | Unreached       | Arrangement a break at its claimed branch leaves green, the input landing in another partition         | Delete       |
|  [07]   | Unlabelled pin  | Literal in `assert_eq!` recorded from a run outside snapshot review, over code with a source           | Delete       |
|  [08]   | Change detector | Assertion on a constant's value, an unqualified `Debug` text, or a variant's name as text              | Delete       |
|  [09]   | Dependency      | Vendor or crate fact no decision of the crate depends on                                               | Delete       |
|  [10]   | Type-proven     | Fact the compiler proves: a return type, a trait bound, a variant's existence                          | Delete       |
|  [11]   | Ignored         | `#[ignore]` no profile or task runs with `--run-ignored`                                               | Delete       |
|  [12]   | Coupled         | Order no contract states, an item made `pub` or `#[doc(hidden)]` for a test package                    | Rebuild      |
|  [13]   | Costly          | Real-clock wait where an in-memory peer fits, rounds repeating a partition a sibling holds             | Rebuild      |
|  [14]   | Flaky           | `sleep` deciding order, wall-clock bounds across processes, a port probed free, `HashMap` order        | Fix at cause |

Kept tests catch a mutant no other test catches, or hold a partition their source states. Rewriting a smelly test in place keeps its author's case list and reading of the code, the cause of its defect, and a behavior worth keeping gets a new test written red first. Pins over code with no source stay as characterization, named for it, until that behavior is decided.

## [05]-[STRENGTHENING]

Rows from the source's partitions and limits come first, and each missed mutant then names the case kind that catches it through the mutation reference's genre table:

| [INDEX] | [CASE_KIND] | [FORM]                                                                                  |
| :-----: | :---------- | :-------------------------------------------------------------------------------------- |
|  [01]   | Partitions  | One `#[case::<row>]` per partition its source states                                    |
|  [02]   | Limits      | Rows on each side of every limit: `16_384` and `16_385`, a nanosecond around a deadline |
|  [03]   | Refusals    | Every rule broken at once beside each rule kept nearest its limit                       |
|  [04]   | Reference   | Second implementation as oracle: `git`, `cargo metadata`, `/bin/sh`, a textbook pass    |
|  [05]   | Property    | `#[property_test]` beside the rows, draws shaped onto each limit                        |
|  [06]   | Relation    | Output equal or unequal to a related input's, in place of a pinned value                |
|  [07]   | Model       | `prop_state_machine!` against a standard-library value                                  |
|  [08]   | Real engine | Suite against the engine whose answer is the behavior                                   |

- Limits, calls, and argument orders the source states take a hand break each, as no mutant genre makes those breaks
- Snapshots join a suite for rendered output that is the product, never as the answer to a missed mutant, as a pin names no change it catches

## [06]-[EDITING]

Editing, improving, or grading a suite runs in order, and a grade stops after step 4:
1. Run: `cargo nextest run -p <package>`, then the flaky stress run over its binary, where any red sends the suite through Failures first
2. Tally: one whole-suite mutation run with `--file <source>` for each production file of `<crate>`, its `caught.txt`, `missed.txt`, and per-test tally kept
3. Read: write one line per test holding its oracle source, partition, caught mutants, run time, and a Smells kind or none
4. Report: the lines of step 3, then the caught share per file, the missed list, the partitions no test holds, and each judgement no run settled
5. Delete: each test of a Delete kind with every mention (its fixture files, snapshot, nextest override, and each helper it alone read)
6. Rebuild: each Coupled or Costly test at an interface callers use, and each Flaky test's input through Failures, one test per step
7. Raise: loops of independent cases to `#[case]` rows, rows following one rule gaining a `#[property_test]` beside them, `proptest!` blocks to `#[property_test]`, shared checks to `#[track_caller]` assertions
8. Compare: step 2's mutation runs again, where each mutant step 2 caught and no test now catches takes a new test red first
9. Replace: each partition a deleted test claimed and no kept test holds takes a new test from its source, red first
10. Strengthen: each missed mutant through Strengthening, or its verdict from the mutation reference
