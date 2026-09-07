# [RULE_TESTING]

Test matching and exclusions, then inspect snapshots for each invalid case's node, labels, and fixed source.

## [01]-[OUTCOMES]

`ast-grep test` parses each case with its rule's language and classifies that rule's result. A passing run establishes only the supplied cases:

| [INDEX] | [STATUS] | [MEANING] | [MARK] |
| :-----: | :----- | :----- | :-----: |
| [01] | Validated | Valid case, no match | `.` |
| [02] | Reported | Invalid case, a match, the snapshot equal | `.` |
| [03] | Missing | Invalid case, no match | `M` |
| [04] | Noisy | Valid case, a match | `N` |
| [05] | Wrong | Invalid case, a match, the snapshot absent or different | `W` |
| [06] | Updated | `-U` or `-i` accepted the generated snapshot | `U` |
| [07] | Error | Fix failed to apply | `E` |

- Exit 0 when every case passes, `Updated` included, and exit 4 for a `Missing`, `Noisy`, `Wrong`, or `Error` case
- Exit 8 for a test or rule file the parser rejects, the whole run, exit 6 for a missing `testDir`, exit 2 for `--skip-snapshot-tests` beside `-U`
- The closing `Help:` line separates a snapshot-only failure (`Run with --update-all`) from a rule failure (the playground line), both exit 4
- `Running N tests` counts test documents, a `---` file counts each, and past 40 cases the marks become counts (`Pass × 41, Wrong × 1`)
- `crates/cli/src/utils/error_context.rs` in the ast-grep source is the one source of exit codes
- `Configuration not found! <id>` names a test with no rule, and a test of a `severity: off` rule under a run without `--include-off`
- `--filter <regex>` selects test ids by Rust regex (`^no-`, `eval`), and `-t` bypasses `testConfigs`
- `test -c <scratch>/sgconfig.yml -t <dir>` runs scratch cases against the real rules from any directory and touches no committed test
- `--include-off` runs the rewrite cases, the `severity: off` rules under `rewrites/`, and the rules target passes it
- `rule-checks.sh pairing` reads the whole tree with no extension, and `gate <ext>` tests the rules of the language that owns the extension alone
- `rule-checks.sh width <ext>`, `arms <ext>`, and `parse <ext>` read that language alone, and a third argument `'^<id>$'` narrows them to one rule
- With `-t`, use `--snapshot-dir <child-directory>` inside the test directory, and `-U` accepts every diff without a prompt

Use the corresponding check when a passing test can hide a defect:

| [INDEX] | [GREEN_CASE] | [CHECK] |
| :-----: | :----- | :----- |
| [01] | Rule with no test document, or a symlinked test without `--follow` | `rule-checks.sh pairing`, `--follow` for the symlink |
| [02] | Test with an id that names no rule, `Configuration not found! <id>`, exit 0 | `rule-checks.sh pairing` |
| [03] | Test document with `id` alone, `SKIP`, `-U` writes `snapshots: {}` | `rule-checks.sh pairing` |
| [04] | Unknown keys, malformed sides, repeated ids, duplicate or contradictory cases | `rule-checks.sh pairing` |
| [05] | Folded `>` case that swallows the next `- >` markers into one string | `rule-checks.sh width <ext>` |
| [06] | Empty case, or one the grammar rejects (`const = ` is `ERROR`), Validated | `rule-checks.sh parse <ext>` |
| [07] | `ast-grep new test` scaffold, `"valid code"`, Validated | Every placeholder case replaced |
| [08] | `severity: off` rule, `Configuration not found!` | `--include-off` on the run, `Configuration not found` line |
| [09] | `files:` or `ignores:` in a case ignored, a suppression comment `Noisy` | Scoping and waivers proven by `scan` over a path |
| [10] | `--skip-snapshot-tests`, or a snapshot never written, a changed fix passes | `rule-checks.sh pairing`, then `-U` |

The scoped parse check uses `bash -n` for Bash and structural `ERROR` searches for other languages. Use the owning parser or compiler for language acceptance. Rule matching and coverage use ast-grep.

## [02]-[CASES]

Derive expected behavior from the correction and package contract before reading the current predicate. Give each distinct sibling and near miss a case with a comment naming its shape:

| [INDEX] | [SET] | [CASE] | [CRITERION] |
| :-----: | :----- | :----- | :----- |
| [01] | `invalid` | Instance | Code the fix came from, as written |
| [02] | `invalid` | One per sibling | Each `any:` branch, exporting module, container, spelling, data-first and data-last form |
| [03] | `invalid` | Case a weaker rule misses | Alias, nested form, point-free constructor, String annotation |
| [04] | `invalid` | One per `constraints` arm | Name inside each alternative of the grammar |
| [05] | `valid` | Corrected form | Shape the `note` states, re-parsed |
| [06] | `valid` | One per `not:` arm | Variant the guard refuses, the one with a replacement that adds a wrapper |
| [07] | `valid` | One per `constraints` boundary | Name outside the grammar that shares the kind |
| [08] | `valid` | Kind without the role | Thunk, initializer, or pair value where the rule counts callbacks |
| [09] | `valid` | `stopBy` bound | Same shape one node past the stopper, a nested closure, a sibling case |
| [10] | `valid` | Legal arm | `raise` alone, dispatch with two operations, value selection |
| [11] | `valid` | Sibling rule's shape | Shape a split rule owns, its comment naming that rule's id |

- Keep one intended diagnostic per invalid native case, and test multiple independent findings through a real-path count or host assertion
- Kill behavior-changing mutations through classification, hit count, or fixed source, without counting label-only snapshot changes
- Separate loader-rejected mutations from runnable survivors, then distinguish missing cases from equivalent predicates
- Inspect each `invalid mutation` loader error outside coverage, preserving valid rules instead of changing them to suit the mutation
- Patterns with fixed arity bind positions through their captures, `constraints: {<VAR>: {matches: <util>}}` replaces `nthChild` arms
- Prove duplicate-match exclusions with `width <ext>` at one hit per case, they change the count rather than the first match
- Presence guards state the kind or a value util, a `\S` over a quoted attribute value matches the quote and fails no case when blanked
- Code holding `key: value` goes in a `- |` block scalar, the plain form fails with `invalid type: map, expected a string` and exit 8
- `|` keeps the trailing newline in the snapshot key and `|-` drops it, a switch between them orphans the entry
- No schema validates a test file, the runner reads `id`, `valid`, and `invalid` and ignores every other key
- Valid cases pass for the wrong reason when their text parses as another kind (a callback under a pair, not an argument list), the tree decides
- A `regex` product over object and property names is proven per pair against the package exports, a pair with another meaning is a near miss
- Names are placeholders (`Item`, `load`, `<key-a>`), the comment on the line before the case names the shape, and no case restates the `message`
- Literals the rule pins (`bsamiee`, `Rasm.*`, a fixed row of `NuGet.config`) are the required case text, the one exception to placeholder names
- Regression comments state the preserved behavior or boundary, without narrating the predicate that happens to implement it

Mutation evidence requires a passing unmodified baseline for every counted caller, including callers outside a selected rule filter. A failed parser, snapshot reader, worker, or preexisting case proves no coverage. Attribute findings to the exact synthesized path rather than inferring identity from its directory depth.

For a surviving mutation, construct a supported input with an intended result that changes. If the grammar or contract proves equivalence, simplify the redundant predicate instead of inventing a case. Record the reason a rejected or equivalent mutation proves no behavioral coverage.

Fixture commands must satisfy the executable's required query, options, and operands. A command that cannot run proves no behavior merely because its text kills a mutation.

Before consolidating tests, map each case to its behavioral distinction. Equal strings are duplicates, while different strings can still test the same boundary. Keep variations that challenge ownership, comments, arity, traversal, or evaluation. Reclassify contradictory cases from the established contract instead of preserving their old valid/invalid labels.

Use shared utilities through their consumers. Cover the utility's shared branches across those consumers and each caller's distinct restrictions locally. Adding identical utility cases to every caller does not strengthen coverage.
- Bash rules prove no hit on a fixture holding the `ERROR` forms, `${| cmd;}` and `${a[$a,$b]}` among them

## [03]-[SNAPSHOTS]

Snapshots record each invalid case by its exact source text and compare results byte for byte:

- By default, `test -U --filter '^<id>$'` writes `<testDir>/__snapshots__/<id>-snapshot.yml`, sorted by case, with `labels` and `fixed`
- The runner reads one snapshot file per id, `snapshotDir` names the directory alone, and a hand-merged or hand-moved snapshot file is never found
- Default labels are one primary on the match and one secondary per relational clause's node, the record of what each clause bound
- Labels list every relational match, so a reordered clause changes the snapshot with the match unchanged, and the `-U` diff reads as label order
- With `labels:` the entry holds the configured captures alone, a changed relational clause then leaves no trace and takes a case of its own
- Label entries hold the `message` text, and a message edit fails `Wrong` until `-U` rewrites the entry
- `fixed` is the case with the first fix template substituted over the match range
- `fixed` skips `expandStart` and `expandEnd` (`foo(first, second)` yields `foo(, second)`), `replacementOffsets` alone show the consumed comma
- `-U` merges and never deletes, a renamed or re-styled case leaves its old key as an orphan the run never checks
- New `invalid:` cases without a snapshot report `[Wrong]` as `No <id> baseline found`, and `-U` writes their entries
- In a `[Wrong]` diff a moved secondary is a changed relational clause, a changed `fixed` a changed template or guard, a moved primary a new target
- `-i` prompts per snapshot (`y`, `n`, `a`, `q`), and an accepted diff with labels no case explains is a rule change the review missed

Check syntax on the source the CLI writes after applying the fix. With `expandStart` or `expandEnd`, snapshot text omits consumed separators and can be invalid even when the applied edit parses. Validate the original cases and the applied source separately.

For YAML member deletion, include a comment between the member and its comma. Adjacent-token expansion can stop at the comment and leave a leading comma after deletion. Preserve the comment and separator together or exclude that shape. For script rewrites, compare the decoded YAML scalar before and after, parse the resulting shell, and confirm a second application changes nothing. Matching command text alone does not establish equivalent process behavior.

## [04]-[ADVERSARIAL]

Assume the rule is wrong, write the case that shows it, and correct the rule when the case is real:

| [INDEX] | [MECHANISM] | [DISPROVING_CASE] |
| :-----: | :----- | :----- |
| [01] | Pattern with a literal callee | Sibling function under `invalid:`, `Missing` proves the gap |
| [02] | `any:` branches | Form the branches omit, the point-free constructor or the method spelling |
| [03] | `not:` guard | Refused variant under `valid:`, `Noisy` proves a missing guard, the kept one under `invalid:` |
| [04] | `constraints` grammar | Name one character outside it under `valid:`, one inside under `invalid:` |
| [05] | `stopBy: end` | Shape one node past the owner under `valid:`, the neighbor default two levels down under `invalid:` |
| [06] | Shared utility | Shared branches across consumers, each caller's distinct restrictions in its own cases |
| [07] | `fix` | Variant that breaks the template under `invalid:`, its `fixed` under the language's fix proof |
| [08] | Duplicate-match exclusion | Nested form counted over a file, one hit |
| [09] | `language` and `files:` | One known hit counted under `--filter` over a real path |
| [10] | Element or callee name `regex` | The same attributes or arguments under another element or callee under `valid:` |
| [11] | Argument rule of a util call | Node the argument rule refuses under `valid:`, one it admits under `invalid:`, per calling rule |
| [12] | `nthChild` object form | Only child under `invalid:`, a sibling before it and one after it under `valid:`, a comment sibling |
| [13] | Closure `stopBy` on `has` | Inner function holding the shape under `valid:`, the owner's own shape under `invalid:` |
| [14] | Nested `follows` count | One sibling short under `valid:`, the counted number under `invalid:`, a skipped kind between them |
| [15] | `transform` chain | Capture only the last stage rewrites, its `fixed` read, an input no stage changes under `valid:` |
| [16] | Partitioning rewriters | Mixed list under `invalid:` with its `fixed` order read, an item no rewriter matches under `valid:` |

- Cases written before the widening prove it: the sibling fails `Missing`, the rule widens, the case passes, the near miss stays `Validated`
- Establish expected matches and fixes before `-U`, inspect generated snapshot changes, and retain snapshots with their rules
- Real cases correct the rule, and real hits correct the code
- Cases that require semantic facts route to language tooling rather than an unsupported syntax diagnostic
- Remove redundant predicates only after proving they distinguish no supported form, keeping guards needed by capture or grammar semantics
- Global util arms are covered once any calling rule's case fails or a count moves, and each caller's test holds a case through the base clause
- For an `invalid:` case, investigate duplicate matches above one hit and rule or file-scope mismatches at zero
- Rerun the scoped gate after widening, because added siblings can change existing cases' match counts
- Typecheck fixed text under the owning project's compiler options and dependencies, keeping syntax acceptance separate from API validity
- Python fixes prove over a scratch copy of each `invalid:` case: `scan -U`, `run -k ERROR`, `uv run ruff format --check`, `uv run ruff check`
- `ruff check` runs `--isolated --select ALL --target-version py315 --preview`, `PLW0108` reports a lambda with a body of one call over its parameter
- Templates the formatter rewrites (the parentheses around a walrus condition) prove as the applied fix under the formatter's check

Cover every behavioral branch with a distinguishing case consistent with `message` and `note`.

YAML deletion cases must include a sole block-mapping entry: removing it can turn the parent into null rather than an empty map. Check decoded values, not only parser acceptance. Pin action-specific rewrite predicates to the version contract that establishes the correction, because an unknown input on an older action makes none of its other inputs redundant. Reused anchors, nested parallel steps, and background prerequisites need separate counterexamples.
- Local utils holding one key are deleted whole by the arm mutation and print `unchecked arm`, the key sits beside a `kind`
