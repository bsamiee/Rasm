# [MUTATION]

`cargo mutants` copies the workspace, applies one mutant at a time to the source, builds it, and runs the tests: a failing run catches the mutant, and a passing run misses it.

## [01]-[RUNS]

`.cargo/mutants.toml` holds the choices every run shares:

```toml
test_tool                  = "nextest"
test_package               = ["<package>", "<support>"]
exclude_globs              = ["<tests>/**"]
cap_lints                  = true
additional_cargo_test_args = ["--no-tests=warn"]
```

`<support>` is the workspace's support crate, `<tests>` a folder holding test packages and no support crate, and `<diff>` a file `git diff` wrote:
- `test_package` names each package whose tests run per mutant, and a run without it tests the mutated package's own tests alone
- `exclude_globs` keeps the `[lib]` and `[[bin]]` targets of test packages out, as every library and binary target of a selected package takes mutants
- `cap_lints = true` keeps a denied lint from marking a mutant unviable
- `--no-tests=warn` makes a mutant whose filtered run selects no test read as missed, where nextest's exit status 4 otherwise counts it as caught
- Baselines test the mutated package alone whatever `test_package` names, and a green `cargo nextest run -p <package> -p <support>` stands in for one through `--baseline=skip`
- Runs copy the tree and leave the source untouched, where `--in-place` edits the checkout another session reads

Scope flags select what a run mutates:

| [INDEX] | [SCOPE]              | [FORM]                                                                     |
| :-----: | :------------------- | :------------------------------------------------------------------------- |
|  [01]   | Files                | `--file <glob>`, `--exclude <glob>`, merged with config                    |
|  [02]   | Changed lines        | `--in-diff <diff>`, `<diff>` written by `git diff --default-prefix <base>` |
|  [03]   | Mutant names         | `--re <regex>`, `--exclude-re <regex>` over names `--list` prints          |
|  [04]   | Packages mutated     | `-p <package>`, `--workspace`                                              |
|  [05]   | Parallel copies      | `-j <jobs>`, `--shard <k>/<total>` with `<k>` from 0                       |
|  [06]   | Last run's survivors | `--iterate`, skipping caught and unviable mutants                          |

- `-E` before `--` is `--exclude-re` and drops mutants, and `-E` after `--` is nextest's filterset selecting the tests each mutant runs
- `cargo mutants --list` with a run's flags prints the set it tests, and `--list --json` adds each mutant's genre, function, and span
- Struct field deletions skip `--re`, `--exclude-re`, and `exclude_re`, joining every run of their file, missed in a run filtered to tests claiming none
- Integration test targets, `#[cfg(test)]` items, `#[test]` functions, `unsafe fn`, and every method named `new` take no mutants
- `--in-diff` exits 5 for a diff the tree no longer matches and 6 for an unreadable diff file
- `--in-diff` exits 0 with nothing tested for text that is no diff, a diff under `diff.noprefix`, or a diff touching no mutant

## [02]-[OPERATORS]

Each genre of mutant names the case that catches it:

| [INDEX] | [MUTANT]                                    | [CAUGHT_BY]                                          |
| :-----: | :------------------------------------------ | :--------------------------------------------------- |
|  [01]   | Body replaced by a value of its return type | Any assertion on a non-default output                |
|  [02]   | `<` to `<=`, `>=` to `<`                    | Rows at the limit and one past it                    |
|  [03]   | `&&` to `\|\|`, `\|\|` to `&&`              | One row per sub-condition deciding the outcome alone |
|  [04]   | `+` to `-` or `*`, `/` to `%` or `*`        | Hand-worked values from asymmetric inputs            |
|  [05]   | `==` to `!=`, `!` or unary `-` deleted      | Rows on each side of the test                        |
|  [06]   | Match arm deleted beside a `_` arm          | Row routed into each arm                             |
|  [07]   | Match guard replaced with `true` or `false` | Row the guard accepts beside a row it refuses        |
|  [08]   | Field deleted from a literal with `..base`  | Assertion on the whole record                        |

Genres never change a literal, delete a statement, or swap arguments, and those breaks take a hand edit of a copy-backed source, a red run, and the copy moved back.

## [03]-[OUTCOMES]

`mutants.out/` holds `caught.txt`, `missed.txt`, `timeout.txt`, `unviable.txt`, `outcomes.json`, a diff per mutant, and a nextest log per mutant under `log/`:
- Exit status 4 means a failing baseline, 3 a timeout, 2 a missed mutant, 1 a usage error, and 0 none, read in that order of precedence
- Timeouts mark a mutant hanging a test (a loop bound flipped), and a test's own deadline turns it into a catch
- Unviable mutants fail to build and test nothing

Each test's caught mutants come from the logs of one whole-suite run under `fail-fast = false`, where nextest prints a failing test twice, once as it fails and once in its summary. Counting each test once per log gives each test's caught count, a log of a mutant in `timeout.txt` naming tests still running at the kill:

```bash
for log in mutants.out/log/*.log; do rg -o '(?:FAIL|TIMEOUT) +\[.*\) \S+ (\S+)$' -r '$1' "$log" | sort -u; done | sort | uniq -c | sort -n
```

Logs naming one test alone give the mutants no other test catches, counted per test:

```bash
for log in mutants.out/log/*.log; do rg -o '(?:FAIL|TIMEOUT) +\[.*\) \S+ (\S+)$' -r '$1' "$log" | sort -u | paste -sd ' ' -; done | rg -v '^$| ' | sort | uniq -c
```

- Tests absent from the first count catch nothing, and a test in the second holds a catch no other test makes
- Missed mutants take one verdict each: a case red against it, deletion of code with no effect, or a rebuild of a form both versions compute alike
- Mutants over cost or observability code (a capacity hint, a cache, a log event) take a benchmark or a `tracing-test` case where cost or event is the contract

Whole-suite runs over `line`, a reader of one line under a deadline and a byte limit, miss one mutant there:

```text
MISSED   size/src/lib.rs:103:18: replace match guard count as u64 == limit with true in line
```

Its guard separates a line cut at its byte limit from a peer closing before its newline, and no row sent a short line then a close, so the next row writes that partition and sees it red under the mutant.

## [04]-[PROOF]

One test's caught mutants are those its filtered run fails on, every other test out:
- `-- -E 'test(/^<module>::<function>(::|$)/)'` filters a run to one function's rows, and `(::|$)` closes the name, as an open prefix also selects every function whose name extends it
- `-- -E 'test(=<test>)'` filters a run to one row
- Suites catch the same set before and after a deletion when the deleted tests held nothing the rest miss
