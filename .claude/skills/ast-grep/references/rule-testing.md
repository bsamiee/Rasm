# [RULE_TESTING]

Test matching and exclusions, then inspect snapshots for each invalid case's node, labels, and fixed source.

## [01]-[OUTCOMES]

`ast-grep test` parses each case with its rule's language and classifies the result, and a passing run proves the supplied cases alone:

| [INDEX] | [STATUS]  | [MEANING]                                               | [MARK] |
| :-----: | :-------- | :------------------------------------------------------ | :----: |
|  [01]   | Validated | Valid case, no match                                    |  `.`   |
|  [02]   | Reported  | Invalid case, a match, the snapshot equal               |  `.`   |
|  [03]   | Missing   | Invalid case, no match                                  |  `M`   |
|  [04]   | Noisy     | Valid case, a match                                     |  `N`   |
|  [05]   | Wrong     | Invalid case, a match, the snapshot absent or different |  `W`   |
|  [06]   | Updated   | `-U` or `-i` accepted the generated snapshot            |  `U`   |
|  [07]   | Error     | Fix failed to apply                                     |  `E`   |

- Exit 0 when every case passes, `Updated` included, and exit 4 for a `Missing`, `Noisy`, `Wrong`, or `Error` case
- Exit 8 for a test or rule file the parser rejects, exit 6 for a missing `testDir`, exit 2 for `--skip-snapshot-tests` beside `-U`
- `--filter <regex>` selects test ids by Rust regex (`^no-`, `eval`), and `-t` bypasses `testConfigs`
- `--include-off` runs the rewrite cases, the `severity: off` rules under `rewrites/`
- Cases scan at a leaf under the config root, `<id>/@N@` per case for a `**` glob or an implied `**/` prefix and `<id>.<ext>` with no extension
- Arms are the keys a case can fail, a list element with siblings or a map with a rule key left, because `field` with `stopBy` alone fails the load
- Deleting `has` or `inside` drops captures a fix needs, `field` mutants are equivalent, and a `regex` arm mutates by blanking

`rule-checks.sh` reads `sgconfig.yml` from the working directory, agents run it, and no project target names it:
- `pairing` reads the whole tree with no extension
- `width <ext>`, `arms <ext>`, and `parse <ext>` read one language, and a third argument `'^<id>$'` narrows them to one rule
- `FAIL` callers stay out of a shared util's arms, and a test or snapshot naming no rule prints under `pairing` alone

Use the matching check when a passing test can hide a defect:

| [INDEX] | [GREEN_CASE]                                                                  | [CHECK]                                              |
| :-----: | :---------------------------------------------------------------------------- | :--------------------------------------------------- |
|  [01]   | Rule with no test document                                                    | `rule-checks.sh pairing`                             |
|  [02]   | Test with an id that names no rule, `Configuration not found! <id>`, exit 0   | `rule-checks.sh pairing`                             |
|  [03]   | Test document with `id` alone, `SKIP`, `-U` writes `snapshots: {}`            | `rule-checks.sh pairing`                             |
|  [04]   | Unknown keys, malformed sides, repeated ids, duplicate or contradictory cases | `rule-checks.sh pairing`                             |
|  [05]   | Folded `>` case that swallows the next `- >` markers into one string          | `rule-checks.sh width <ext>`                         |
|  [06]   | Empty case, or one the grammar rejects (`const = ` is `ERROR`), Validated     | `rule-checks.sh parse <ext>`                         |
|  [07]   | `ast-grep new test` scaffold, `"valid code"`, Validated                       | Every placeholder case replaced                      |
|  [08]   | `severity: off` rule, `Configuration not found!`                              | `--include-off`, then `Configuration not found` line |
|  [09]   | `files:` or `ignores:` in a case ignored, a suppression comment `Noisy`       | Scoping and waivers proven by `scan` over a path     |
|  [10]   | `--skip-snapshot-tests`, or a snapshot never written, a changed fix passes    | `rule-checks.sh pairing`, then `-U`                  |

## [02]-[CASES]

Derive expected behavior from the correction and package contract before reading the predicate, and give each sibling and near miss a case:

| [INDEX] | [SET]     | [CASE]                         | [CRITERION]                                                                              |
| :-----: | :-------- | :----------------------------- | :--------------------------------------------------------------------------------------- |
|  [01]   | `invalid` | Instance                       | Code the fix came from, as written                                                       |
|  [02]   | `invalid` | One per sibling                | Each `any:` branch, exporting module, container, spelling, data-first and data-last form |
|  [03]   | `invalid` | Case a weaker rule misses      | Alias, nested form, point-free constructor, String annotation                            |
|  [04]   | `invalid` | One per `constraints` arm      | Name inside each alternative of the grammar                                              |
|  [05]   | `valid`   | Corrected form                 | Shape the `note` states, re-parsed                                                       |
|  [06]   | `valid`   | One per `not:` arm             | Variant the guard refuses, the one with a replacement that adds a wrapper                |
|  [07]   | `valid`   | One per `constraints` boundary | Name outside the grammar that shares the kind                                            |
|  [08]   | `valid`   | Kind without the role          | Thunk, initializer, or pair value where the rule counts callbacks                        |
|  [09]   | `valid`   | `stopBy` bound                 | Same shape one node past the stopper, a nested closure, a sibling case                   |
|  [10]   | `valid`   | Legal arm                      | `raise` alone, dispatch with two operations, value selection                             |
|  [11]   | `valid`   | Sibling rule's shape           | Shape a split rule owns                                                                  |

- Keep one intended diagnostic per invalid native case, and test independent findings through a real-path count or host assertion
- Kill behavior-changing mutations through classification, hit count, or fixed source, and count no label-only snapshot change
- Separate loader-rejected mutations from runnable survivors, then missing cases from equivalent predicates
- Arm deletions that leave a transform or `constraints` capture undefined print `invalid mutation` per arm outside coverage and exit 0
- Patterns with fixed arity bind positions through their captures, and `constraints: {<VAR>: {matches: <util>}}` replaces `nthChild` arms
- Prove duplicate-match exclusions with `width <ext>` at one hit per case, because they change the count and not the first match
- Presence guards state the kind or a value util, because `\S` over a quoted attribute value matches the quote and fails no case when blanked
- Code holding `key: value` goes in a `- |` block scalar, and the plain form fails with `invalid type: map, expected a string` and exit 8
- `|` keeps the trailing newline in the snapshot key and `|-` drops it, and a switch between them orphans the entry
- Runner reads `id`, `valid`, and `invalid` and ignores every other key, and no schema validates a test file
- Valid cases pass for the wrong reason when their text parses as another kind (a callback under a pair, not an argument list), the tree decides
- `regex` products over object and property names prove per pair against the package exports, and a pair with another meaning is a near miss
- Names are placeholders (`Item`, `load`, `<key-a>`), and literals the rule pins (`bsamiee`, `Rasm.*`, a `NuGet.config` row) are required text
- `constraints` regexes over a positional C# capture read the argument's name label too, and the named form takes its own case and branch
- Regex arms that widen an exemption die to a same-kind sibling outside the exemption placed before the reported one
- Near misses placed before the intended match kill a guard `arms` reports uncovered, because `has` binds the first child and never backtracks
- Bash rules prove no hit on a fixture holding the `ERROR` forms
- Mutation evidence requires a passing unmodified baseline for every counted caller, callers outside a selected rule filter included
- Surviving mutations take a supported input with a changed result, or the redundant predicate simplifies once the contract proves equivalence
- Fixture commands run under the executable's real query, options, and operands, because text that kills a mutation without running proves no behavior
- `builtin -p`, `exec eval`, `env eval`, a text assignment to `RANDOM`, `SECONDS`, `LINENO`, `OPTIND`, or `UID`, and `((m **= 2))` run nothing in Bash
- Equal case strings are duplicates, different strings can test one boundary, and variations over ownership, comments, arity, and traversal stay
- Contradictory cases reclassify from the proven contract, not their old labels

## [03]-[SNAPSHOTS]

Snapshots record each invalid case by its exact source text and compare results byte for byte:
- `test -U --filter '^<id>$'` writes `<testDir>/__snapshots__/<id>-snapshot.yml`, sorted by case, with `labels` and `fixed`
- Runner reads one snapshot file per id, `snapshotDir` names the directory alone, and a hand-merged or hand-moved snapshot file is never found
- Default labels are one primary on the match and a secondary per relational clause node, descendant-bound metavariable, and transform output range
- Runner ignores label order, `-U` can reorder a secondary label of an untouched entry, and a reordered clause reads as a label diff
- `labels` naming the captures of exclusive `any:` arms load, and the snapshot writes the bound arm's label alone
- With `labels:` the entry holds the configured captures alone, and a changed relational clause then leaves no trace and takes a case of its own
- Label entries hold `source`, `style`, `start`, and `end` and no message text, and a `message` edit changes no snapshot and fails no case
- `fixed` is the case with the first fix template substituted over the match range
- Fixed text that emits siblings parses as one document inside its owner alone, and such a case wraps in the owning element
- `fixed` skips `expandStart` and `expandEnd` (`foo(first, second)` yields `foo(, second)`), and `replacementOffsets` show the consumed comma
- Applied source is checked for syntax apart from the case, because snapshot text without consumed separators can be invalid while the edit parses
- `-U` merges and never deletes, and a renamed or re-styled case leaves its old key as an orphan the run never checks
- Orphan entries leave by deleting the snapshot file before `-U --filter '^<id>$'`, because `yq -i` rewrites the whole file in its own format
- `-U --filter '.'` over many documents can panic after some writes, and `-U` per id writes each
- New `invalid:` cases without a snapshot report `[Wrong]` as `No <id> baseline found`, and `-U` writes their entries
- In a `[Wrong]` diff a moved secondary is a changed relational clause, a changed `fixed` a changed template or guard, a moved primary a new target
- Accepted diffs with labels no case explains are a rule change the review missed

## [04]-[ADVERSARIAL]

Assume the rule is wrong, write the case that shows it, and correct the rule when the case is real:

| [INDEX] | [MECHANISM]                    | [DISPROVING_CASE]                                                                                   |
| :-----: | :----------------------------- | :-------------------------------------------------------------------------------------------------- |
|  [01]   | Pattern with a literal callee  | Sibling function under `invalid:`, `Missing` proves the gap                                         |
|  [02]   | `any:` branches                | Form the branches omit, the point-free constructor or the method spelling                           |
|  [03]   | `not:` guard                   | Refused variant under `valid:`, `Noisy` proves a missing guard, the kept one under `invalid:`       |
|  [04]   | `constraints` grammar          | Name one character outside it under `valid:`, one inside under `invalid:`                           |
|  [05]   | `stopBy: end`                  | Shape one node past the owner under `valid:`, the neighbor default two levels down under `invalid:` |
|  [06]   | Shared utility                 | Shared branches across consumers, each caller's distinct restrictions in its own cases              |
|  [07]   | `fix`                          | Variant that breaks the template under `invalid:`, its `fixed` under the language's fix proof       |
|  [08]   | Duplicate-match exclusion      | Nested form counted over a file, one hit                                                            |
|  [09]   | `language` and `files:`        | One known hit counted under `--filter` over a real path                                             |
|  [10]   | Element or callee name `regex` | Same attributes or arguments under another element or callee under `valid:`                         |
|  [11]   | Argument rule of a util call   | Node the argument rule refuses under `valid:`, one it admits under `invalid:`, per calling rule     |
|  [12]   | `nthChild` object form         | Only child under `invalid:`, a sibling before it and one after it under `valid:`, a comment sibling |
|  [13]   | Closure `stopBy` on `has`      | Inner function with the shape under `valid:`, the shape under a nested `try` block under `invalid:` |
|  [14]   | Nested `follows` count         | One sibling short under `valid:`, the counted number under `invalid:`, a skipped kind between them  |
|  [15]   | `transform` chain              | Capture only the last stage rewrites, its `fixed` read, an input no stage changes under `valid:`    |
|  [16]   | Partitioning rewriters         | Mixed list under `invalid:` with its `fixed` order read, an item no rewriter matches under `valid:` |
|  [17]   | `has: {pattern: $_}` guard     | Comment as the only child (`f(/* none */)`) under `valid:`, the guard with `not: {kind: comment}`   |

- Global util arms count as covered once any caller's case fails or a count moves, and each caller's test holds a case through the base clause
- For an `invalid:` case, investigate duplicate matches above one hit and rule or file-scope mismatches at zero
- Typecheck fixed text under the owning project's compiler options and dependencies, syntax acceptance apart from API validity
- Python fixes prove per `invalid:` case: `scan -U`, `run -k ERROR`, `uv run ruff format --check`, `uv run ruff check`
- Bash fixed texts prove under `bash -n` and `shellcheck -o all -f gcc`, and `SC2154` over an array `mapfile` fills is the checker's blind spot
- `ruff check` runs `--isolated --select ALL --target-version py315 --preview`, `PLW0108` reports a lambda with a body of one call on its parameter
- `F401` on a fixed text that dropped a member is the residue `ruff check --fix` removes, and the proof allows it alone
- Templates the formatter rewrites (the parentheses around a walrus condition) prove as the applied fix under the formatter's check
- Local utils holding one key are deleted whole by the arm mutation and print `unchecked arm`, and the key sits beside a `kind`
- YAML deletion cases include a sole block-mapping entry, because removing it can turn the parent into null
- Action-specific rewrite predicates pin the version contract of the correction, an unknown input on an older action makes no other input redundant
