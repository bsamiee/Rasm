# [RULE_TESTING]

Test matching and exclusions, then inspect snapshots for each invalid case's node, labels, and fixed source.

## [01]-[OUTCOMES]

`ast-grep test` parses each case with the rule's language and classifies the result, a passing run proves the supplied cases alone:

| [INDEX] | [STATUS]  | [MEANING]                                               | [MARK] |
| :-----: | :-------- | :------------------------------------------------------ | :----: |
|  [01]   | Validated | Valid case, no match                                    |  `.`   |
|  [02]   | Reported  | Invalid case, a match, the snapshot equal               |  `.`   |
|  [03]   | Missing   | Invalid case, no match                                  |  `M`   |
|  [04]   | Noisy     | Valid case, a match                                     |  `N`   |
|  [05]   | Wrong     | Invalid case, a match, the snapshot absent or different |  `W`   |
|  [06]   | Updated   | `-U` or `-i` accepted the generated snapshot            |  `U`   |
|  [07]   | Error     | Fix failed to apply                                     |  `E`   |

- Exit 0 when every case passes, `Updated` included, exit 4 for a `Missing`, `Noisy`, `Wrong`, or `Error` case
- Exit 8 for a test or rule file the parser rejects, exit 6 for a missing `testDir`, exit 2 for `--skip-snapshot-tests` beside `-U`
- `--filter <regex>` selects test ids by Rust regex (`^no-`, `eval`), `test` takes no positional id
- `-U` with no `--filter` rewrites every changed snapshot in the tree
- `-t` bypasses `testConfigs`
- `--include-off` runs the rewrite cases, the `severity: off` rules under `rewrites/`

Use the matching check when a passing test can hide a defect:

| [INDEX] | [PASSING_CASE]                                                                | [CHECK]                                              |
| :-----: | :---------------------------------------------------------------------------- | :--------------------------------------------------- |
|  [01]   | Test with an id that names no rule, `Configuration not found! <id>`, exit 0   | The `Configuration not found` line of the run        |
|  [02]   | Test document with `id` alone, `SKIP`, `-U` writes `snapshots: {}`            | The `SKIP` line of the run                           |
|  [03]   | Empty case, or one the grammar rejects (`const = ` is `ERROR`), Validated     | `ast-grep run -k ERROR -l <lang>` over the case text |
|  [04]   | `ast-grep new test` scaffold, `"valid code"`, Validated                       | Every placeholder case replaced                      |
|  [05]   | `severity: off` rule, `Configuration not found!`                              | `--include-off`, then `Configuration not found` line |
|  [06]   | `files:` or `ignores:` in a case ignored, a suppression comment `Noisy`       | Scoping and suppression proven by `scan` over a path |
|  [07]   | `--skip-snapshot-tests`, or a snapshot never written, a changed fix passes    | `-U --filter '^<id>$'`, then the run without `-U`    |

## [02]-[CASES]

Before reading the predicate, derive expected behavior from the correction and package contract, and give each sibling and near miss a case:

| [INDEX] | [SET]     | [CASE]                         | [CRITERION]                                                                              |
| :-----: | :-------- | :----------------------------- | :--------------------------------------------------------------------------------------- |
|  [01]   | `invalid` | Instance                       | Code the fix came from, as written                                                       |
|  [02]   | `invalid` | One per sibling                | Each `any:` arm, exporting module, container, spelling, data-first and data-last form    |
|  [03]   | `invalid` | Case a weaker rule misses      | Alias, nested form, point-free constructor, String annotation                            |
|  [04]   | `invalid` | One per `constraints` arm      | Name inside each alternative of the grammar                                              |
|  [05]   | `valid`   | Corrected form                 | Shape the `note` states, re-parsed                                                       |
|  [06]   | `valid`   | One per `not:` arm             | Variant the guard refuses, the one with a replacement adding a wrapper                   |
|  [07]   | `valid`   | One per `constraints` boundary | Name outside the grammar that shares the kind                                            |
|  [08]   | `valid`   | Kind without the role          | Thunk, initializer, or pair value where the rule counts callbacks                        |
|  [09]   | `valid`   | `stopBy` bound                 | Same shape one node past the `stopBy` node, a nested closure, a sibling case             |
|  [10]   | `valid`   | Legal arm                      | `raise` alone, dispatch with two operations, value selection                             |
|  [11]   | `valid`   | Sibling rule's shape           | Shape a split rule owns                                                                  |

- Keep one intended diagnostic per invalid native case, test independent findings through a real-path count or host assertion
- Patterns with fixed arity bind positions through their captures, `constraints: {<VAR>: {matches: <util>}}` replaces `nthChild` arms
- Presence guards state the kind or a value util, `\S` over a quoted attribute value matches the quote and fails no case when blanked
- Code holding `key: value` goes in a `- |` block scalar, the plain form fails with `invalid type: map, expected a string` and exit 8
- `|` keeps the trailing newline in the snapshot key and `|-` drops it, a switch between them orphans the entry
- Runner reads `id`, `valid`, and `invalid`, ignores every other key, no schema validates a test file
- Valid cases pass for the wrong reason when their text parses as another kind (a callback under a pair, not an argument list), the tree decides
- `regex` products over object and property names prove per pair against the package exports, a pair with another meaning is a near miss
- Names are placeholders (`Item`, `load`, `<key-a>`), literals a rule pins (a package prefix, a `NuGet.config` row) are required text
- `constraints` regexes over a positional C# capture include the argument's name label, the named form takes its own case and arm
- Regex arms widening an exemption die to a same-kind sibling outside the exemption placed before the reported one
- Bash rules prove no match on a fixture holding the `ERROR` forms
- `builtin -p`, `exec eval`, `env eval`, text assigned to `RANDOM`, `SECONDS`, `LINENO`, `OPTIND`, or `UID`, and `((m **= 2))` run nothing in Bash
- Equal case strings are duplicates, different strings can test one boundary, variations over ownership, comments, arity, and traversal stay
- Contradictory cases reclassify from the proven contract

## [03]-[SNAPSHOTS]

Snapshots record each invalid case by its exact source text and compare results byte for byte:
- `test -U --filter '^<id>$'` writes `<testDir>/__snapshots__/<id>-snapshot.yml`, sorted by case, with `labels` and `fixed`
- Runner reads one snapshot file per id, `snapshotDir` names the directory alone, a hand-merged or hand-moved snapshot file is never found
- Default labels are one primary on the match and a secondary per relational clause node, descendant-bound metavariable, and transform output range
- Runner ignores label order, `-U` can reorder a secondary label of an untouched entry, a reordered clause reads as a label diff
- `labels` naming the captures of exclusive `any:` arms load, the snapshot writes the bound arm's label alone
- With `labels:` the entry holds the configured captures alone, a changed relational clause leaves no trace and takes its own case
- Label entries hold `source`, `style`, `start`, and `end` with no message text, a `message` edit changes no snapshot and fails no case
- `fixed` is the case with the first fix template substituted over the match range
- Fixed text emitting siblings parses as one document inside its owner alone, the case wraps in the owning element
- `fixed` skips `expandStart` and `expandEnd` (`foo(first, second)` yields `foo(, second)`), `replacementOffsets` show the consumed comma
- Applied source is checked for syntax apart from the case
- `-U` merges and never deletes, a renamed or re-styled case leaves its old key as an orphan the run never checks
- Orphan entries go by deleting the snapshot file before `-U --filter '^<id>$'`, the file regenerates byte for byte
- `yq -i` rewrites the whole file in its own format
- `-U --filter '.'` over many documents can panic after some writes, `-U` per id writes each
- New `invalid:` cases without a snapshot report `[Wrong]` as `No <id> baseline found`, `-U` writes their entries
- In a `[Wrong]` diff a moved secondary is a changed relational clause, a changed `fixed` a changed template or guard, a moved primary a new target
- Accepted diffs with labels no case explains are a rule change the review missed

## [04]-[ADVERSARIAL]

Assume the rule is wrong, write the case showing it, and correct the rule when the case is real:

| [INDEX] | [MECHANISM]                    | [DISPROVING_CASE]                                                                                   |
| :-----: | :----------------------------- | :-------------------------------------------------------------------------------------------------- |
|  [01]   | Pattern with a literal callee  | Sibling function under `invalid:`, `Missing` proves the gap                                         |
|  [02]   | `any:` arms                    | Form the arms omit, the point-free constructor or the method spelling                               |
|  [03]   | `not:` guard                   | Refused variant under `valid:`, `Noisy` proves a missing guard, the kept one under `invalid:`       |
|  [04]   | `constraints` grammar          | Name one character outside it under `valid:`, one inside under `invalid:`                           |
|  [05]   | `stopBy: end`                  | Shape one node past the owner under `valid:`, the neighbor default two levels down under `invalid:` |
|  [06]   | Shared utility                 | Shared arms across consumers, each caller's distinct restrictions in its own cases                  |
|  [07]   | `fix`                          | Variant breaking the template under `invalid:`, its `fixed` under the language's fix proof          |
|  [08]   | Duplicate-match exclusion      | Nested form counted over a file, one match                                                          |
|  [09]   | `language` and `files:`        | One known match counted under `--filter` over a real path                                           |
|  [10]   | Element or callee name `regex` | Same attributes or arguments under another element or callee under `valid:`                         |
|  [11]   | Argument rule of a util call   | Node the argument rule refuses under `valid:`, one it admits under `invalid:`, per calling rule     |
|  [12]   | `nthChild` object form         | Only child under `invalid:`, a sibling before it and one after it under `valid:`, a comment sibling |
|  [13]   | Closure `stopBy` on `has`      | Inner function with the shape under `valid:`, the shape under a nested `try` block under `invalid:` |
|  [14]   | Nested `follows` count         | One sibling short under `valid:`, the counted number under `invalid:`, a skipped kind between them  |
|  [15]   | `transform` chain              | Capture only the last stage rewrites, its `fixed` read, an input no stage changes under `valid:`    |
|  [16]   | Partitioning rewriters         | Mixed list under `invalid:` with its `fixed` order read, an item no rewriter matches under `valid:` |
|  [17]   | `has: {pattern: $_}` guard     | Comment as the only child (`f(/* none */)`) under `valid:`                                          |

- Global util arms count as covered once any caller's case fails or a count moves, each caller's test holds a case through the base clause
- For an `invalid:` case, investigate duplicate matches above one and rule or file-scope mismatches at zero
- Typecheck fixed text under the owning project's compiler options and dependencies, syntax acceptance apart from API validity
- Python fixes prove per `invalid:` case: `scan -U`, `run -k ERROR`, `uv run ruff format --check`, `uv run ruff check`
- Bash fixed texts prove under `bash -n` and `shellcheck -o all -f gcc`, `SC2154` over an array `mapfile` fills is a false positive
- `PLW0108` reports a lambda with a body of one call on its parameter
- `F401` on a fixed text with a dropped member is the unused import `ruff check --fix` removes, the proof allows it alone
- Templates the formatter rewrites (the parentheses around a walrus condition) prove as the applied fix under the formatter's check
- YAML deletion cases include a sole block-mapping entry, removing it can turn the parent into null
- Action-specific rewrite predicates pin the correction's version contract, an unknown input on an older action makes no other input redundant
