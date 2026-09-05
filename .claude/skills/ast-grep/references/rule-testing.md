# [RULE_TESTING]

A rule test proves what the rule reports and what it keeps out, and its snapshot records the node, the labels, and the fix of each `invalid:` case.

## [01]-[OUTCOMES]

`ast-grep test` parses each case with the rule's language, runs the rule alone on it, and classifies the result, and a green run proves the classification of the cases it saw and nothing beyond them:

| [INDEX] | [STATUS]  | [MEANING]                                                   | [MARK] |
| :-----: | :-------- | :---------------------------------------------------------- | :----: |
|  [01]   | Validated | Valid case, no match                                        |  `.`   |
|  [02]   | Reported  | Invalid case, a match, the snapshot equal                   |  `.`   |
|  [03]   | Missing   | Invalid case, no match                                      |  `M`   |
|  [04]   | Noisy     | Valid case, a match                                         |  `N`   |
|  [05]   | Wrong     | Invalid case, a match, the snapshot absent or different     |  `W`   |
|  [06]   | Updated   | `-U` or `-i` accepted the generated snapshot                |  `U`   |
|  [07]   | Error     | Fix failed to apply                                         |  `E`   |

- Exit 0 when every case passes, `Updated` included, and exit 4 for a `Missing`, `Noisy`, `Wrong`, or `Error` case
- Exit 8 for a test or rule file the parser rejects, the whole run, exit 6 for a missing `testDir`, exit 2 for `--skip-snapshot-tests` beside `-U`
- The closing `Help:` line separates a snapshot-only failure (`Run with --update-all`) from a rule failure (the playground line), both exit 4
- `Running N tests` counts test documents, a `---` file counts each, and past 40 cases the marks become counts (`Pass × 41, Wrong × 1`)
- `--filter <regex>` selects test ids by Rust regex (`^no-`, `eval`), `--include-off` runs `severity: off` rules, `-t` bypasses `testConfigs`
- `scan --error=<rule-id>` enables one `severity: off` rule per run
- `--snapshot-dir` acts beside `-t` alone, `--interactive` wins over `-U` when both are passed, and `-U` accepts every diff without a prompt

A green run proves the wrong thing in each listed case, and the check beside it is the proof that closes the gap:

| [INDEX] | [GREEN_CASE]                                                             | [CHECK]                                                  |
| :-----: | :----------------------------------------------------------------------- | :------------------------------------------------------- |
|  [01]   | Rule with no test document, or a symlinked test without `--follow`       | Ids paired both ways, `Running 0 tests` under the filter |
|  [02]   | Test with an id that names no rule, `Configuration not found! <id>`, exit 0 | Same pairing                                           |
|  [03]   | Test document with `id` alone, `SKIP`, `-U` writes `snapshots: {}`       | One case on each side at least                           |
|  [04]   | Key the runner lacks (`vaild:`, `todoValid:`, `language:`), never run    | Every document key is `id`, `valid`, or `invalid`        |
|  [05]   | Folded `>` case that swallows the next `- >` markers into one string     | Case count as written, one hit per case                  |
|  [06]   | Empty case, or one the grammar rejects (`const = ` is `ERROR`), Validated | `run -l <lang> -p '<case>' --debug-query=ast`, no `ERROR` |
|  [07]   | `ast-grep new test` scaffold, `"valid code"`, Validated                  | Every placeholder case replaced                          |
|  [08]   | `severity: off` rule, `Configuration not found!`                         | `--include-off`                                          |
|  [09]   | `files:`, `ignores:`, or a suppression comment in a case, each ignored   | Scoping and waivers proven by `scan` over a path         |
|  [10]   | `--skip-snapshot-tests`, or a snapshot never written, a changed fix passes | Snapshot run                                            |

## [02]-[CASES]

Each case holds one shape under a comment naming it, and the set is the sibling and near-miss enumeration written down, one case per arm:

| [INDEX] | [SET]     | [CASE]                        | [CRITERION]                                                                      |
| :-----: | :-------- | :---------------------------- | :------------------------------------------------------------------------------- |
|  [01]   | `invalid` | Instance                      | Code the fix came from, as written                                               |
|  [02]   | `invalid` | One per sibling               | Each `any:` branch, carrier, container, spelling, data-first and data-last form  |
|  [03]   | `invalid` | Case a weaker rule misses     | Alias, nested form, point-free constructor, stringized annotation                 |
|  [04]   | `invalid` | One per `constraints` arm     | Name inside each alternative of the grammar                                      |
|  [05]   | `valid`   | Corrected form                | Shape the `note` states, re-parsed                                               |
|  [06]   | `valid`   | One per `not:` arm            | Variant the guard refuses, the one with a replacement that adds a wrapper        |
|  [07]   | `valid`   | One per `constraints` boundary | Name outside the grammar that shares the kind                                   |
|  [08]   | `valid`   | Kind without the role         | Thunk, initializer, or pair value where the rule counts callbacks                |
|  [09]   | `valid`   | `stopBy` bound                | Same shape one node past the stopper, a nested closure, a sibling case           |
|  [10]   | `valid`   | Legal arm                     | Bare re-raise, dispatch with two operations, value selection                     |
|  [11]   | `valid`   | Sibling rule's shape          | Shape a split rule owns, its comment naming that rule's id                       |

- One violation per case: the snapshot records the first match's labels and fix alone (`eval(1); eval(2)` fixes `eval(1)`), the second is invisible
- The arm test: delete the arm and run the filtered test, a green run means no case exercises the arm, and the arm leaves or gains its case
- A mutation yields a rule that parses or the arm is unchecked, a sole `constraints` key goes with its map, a lone `not` with its `all` element
- A once-reporting arm changes the count and not the first match, and its proof is the count over a file of the cases
- Code holding `key: value` goes in a `- |` block scalar, the plain form fails with `invalid type: map, expected a string` and exit 8
- `|` keeps the trailing newline in the snapshot key and `|-` drops it, a switch between them orphans the entry
- No schema validates a test file, the runner reads `id`, `valid`, and `invalid` and ignores every other key, `yq 'keys'` is the shape check
- A valid case passes for the wrong reason when its text parses as another kind (a callback under a pair, not an argument list), the tree decides
- Names are placeholders (`Item`, `load`, `<key-a>`), the comment on the line before the case names the shape, and no case restates the `message`
- A regression case's comment names the gap it closed (the `stopBy: end` that reached the earlier owner), the case is the record of the defect

## [03]-[SNAPSHOTS]

The snapshot is the committed record of what the rule reported for each `invalid:` case, keyed by the exact case text, and the run compares it byte for byte:

- `test -U` writes `<testDir>/__snapshots__/<id>-snapshot.yml` with sorted keys, one entry per case: `labels`, and `fixed` when the rule has a fix
- Default labels are one primary on the match and one secondary per relational clause's node, the record of what each clause bound
- With `labels:` the entry holds the configured captures alone, a changed relational clause then leaves no trace and takes a case of its own
- `fixed` is the case with the first fix template substituted over the match range, and it must re-parse: `run -k ERROR -l <lang> --stdin` exits 1
- `fixed` skips `expandStart` and `expandEnd` (`foo(first, second)` yields `foo(, second)`), `replacementOffsets` alone show the consumed comma
- `-U` merges and never deletes, a renamed or re-styled case leaves its old key as an orphan the run never checks, every key is a case
- In a `[Wrong]` diff a moved secondary is a changed relational clause, a changed `fixed` a changed template or guard, a moved primary a new target
- `-i` prompts per snapshot (`y`, `n`, `a`, `q`), and an accepted diff with labels no case explains is a rule change the review missed

## [04]-[ADVERSARIAL]

Assume the rule is wrong, write the case that shows it, and correct the rule when the case is real:

| [INDEX] | [DEVICE]                    | [DISPROVING_CASE]                                                                              |
| :-----: | :-------------------------- | :--------------------------------------------------------------------------------------------- |
|  [01]   | Pattern with a literal callee | Sibling function under `invalid:`, `Missing` proves the gap                                  |
|  [02]   | `any:` branches             | Form the branches omit, the point-free constructor or the method spelling                      |
|  [03]   | `not:` guard                | Refused variant under `valid:`, `Noisy` proves a missing guard, the kept one under `invalid:`     |
|  [04]   | `constraints` grammar       | Name one character outside it under `valid:`, one inside under `invalid:`                      |
|  [05]   | `stopBy: end`               | Shape one node past the owner under `valid:`, the neighbor default two levels down under `invalid:`     |
|  [06]   | Util the rule hides behind  | Each arm of the util as a case, a global util proven through every rule that calls it          |
|  [07]   | `fix`                       | Variant that breaks the template under `invalid:`, its `fixed` re-parsed, the guard it needs     |
|  [08]   | Once-reporting arm          | Nested form counted over a file, one hit                                                       |
|  [09]   | `language` and `files:`     | One known hit counted under `--filter` over a real path                                        |

- A case written before the widening proves it: the sibling fails `Missing`, the widening lands, the case passes, the near miss stays `Validated`
- `-U` runs after the rule is proven and its diff is read, the snapshot is committed with the rule, and `-U` on a red run proves nothing
- A real case corrects the rule, and a real hit corrects the code
- A case no node shape satisfies ends as a residual rule or as no rule
- Hits over the cases as one file, `scan --filter '^<id>$' --json=stream <file> | wc -l`, equal the `invalid:` count, more is a once-reporting gap
- A whole file is proven by `ast-grep-ignore: <id>` marks under `scan --error=unused-suppression`, exit 0 when each mark was needed

Each arm has the case that made it necessary and an arm without one leaves, the case list is the specification the `message` and `note` restate, and a rule with cases of one shape is a one-shape rule for `rule-hardening`.
