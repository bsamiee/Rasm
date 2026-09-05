# [RULE_TESTING]

A rule test proves what the rule reports and what it keeps out, and its snapshot records the node, the labels, and the fix of each `invalid:` case.

## [01]-[OUTCOMES]

`ast-grep test` parses each case with the rule's language, runs the rule alone on it, and classifies the result, and a green run proves the classification of the cases it saw and nothing beyond them:

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
- Exit 8 for a test or rule file the parser rejects, the whole run, exit 6 for a missing `testDir`, exit 2 for `--skip-snapshot-tests` beside `-U`
- The closing `Help:` line separates a snapshot-only failure (`Run with --update-all`) from a rule failure (the playground line), both exit 4
- `Running N tests` counts test documents, a `---` file counts each, and past 40 cases the marks become counts (`Pass × 41, Wrong × 1`)
- `crates/cli/src/utils/error_context.rs` in the ast-grep source is the one source of exit codes, `rule-checks.sh` stops on a code other than 0 and 4
- `Configuration not found! <id>` names a test with no rule and a test of a `severity: off` rule alike, `rule-checks.sh` prints the line either way
- `--filter <regex>` selects test ids by Rust regex (`^no-`, `eval`), `--include-off` runs `severity: off` rules, `-t` bypasses `testConfigs`
- `scan --error=<rule-id>` enables one `severity: off` rule per run
- `--snapshot-dir` acts beside `-t` alone, `--interactive` wins over `-U` when both are passed, and `-U` accepts every diff without a prompt

A green run proves the wrong thing in each listed case, and the check beside it, a line of `rule-checks.sh <ext>` where it prints one, closes the gap:

| [INDEX] | [GREEN_CASE]                                                                | [CHECK]                                                    |
| :-----: | :-------------------------------------------------------------------------- | :--------------------------------------------------------- |
|  [01]   | Rule with no test document, or a symlinked test without `--follow`          | `no test: <id>`                                            |
|  [02]   | Test with an id that names no rule, `Configuration not found! <id>`, exit 0 | `no rule: <id>`                                            |
|  [03]   | Test document with `id` alone, `SKIP`, `-U` writes `snapshots: {}`          | `one side empty: <id>`                                     |
|  [04]   | Key the runner lacks (`vaild:`, `todoValid:`, `language:`), never run       | `unknown key in <id>: <key>`                               |
|  [05]   | Folded `>` case that swallows the next `- >` markers into one string        | `width <id> case <n>: 2 hits`                              |
|  [06]   | Empty case, or one the grammar rejects (`const = ` is `ERROR`), Validated   | `ERROR node in <invalid\|valid\|fixed> <id> case <n>`      |
|  [07]   | `ast-grep new test` scaffold, `"valid code"`, Validated                     | Every placeholder case replaced                            |
|  [08]   | `severity: off` rule, `Configuration not found!`                            | `Configuration not found` line, `--include-off`            |
|  [09]   | `files:`, `ignores:`, or a suppression comment in a case, each ignored      | Scoping and waivers proven by `scan` over a path           |
|  [10]   | `--skip-snapshot-tests`, or a snapshot never written, a changed fix passes  | `no snapshot`, `orphan or missing snapshot key`, then `-U` |

## [02]-[CASES]

Each case holds one shape under a comment naming it, and the set is the sibling and near-miss enumeration written down, one case per arm:

| [INDEX] | [SET]     | [CASE]                         | [CRITERION]                                                                     |
| :-----: | :-------- | :----------------------------- | :------------------------------------------------------------------------------ |
|  [01]   | `invalid` | Instance                       | Code the fix came from, as written                                              |
|  [02]   | `invalid` | One per sibling                | Each `any:` branch, carrier, container, spelling, data-first and data-last form |
|  [03]   | `invalid` | Case a weaker rule misses      | Alias, nested form, point-free constructor, stringized annotation               |
|  [04]   | `invalid` | One per `constraints` arm      | Name inside each alternative of the grammar                                     |
|  [05]   | `valid`   | Corrected form                 | Shape the `note` states, re-parsed                                              |
|  [06]   | `valid`   | One per `not:` arm             | Variant the guard refuses, the one with a replacement that adds a wrapper       |
|  [07]   | `valid`   | One per `constraints` boundary | Name outside the grammar that shares the kind                                   |
|  [08]   | `valid`   | Kind without the role          | Thunk, initializer, or pair value where the rule counts callbacks               |
|  [09]   | `valid`   | `stopBy` bound                 | Same shape one node past the stopper, a nested closure, a sibling case          |
|  [10]   | `valid`   | Legal arm                      | Bare re-raise, dispatch with two operations, value selection                    |
|  [11]   | `valid`   | Sibling rule's shape           | Shape a split rule owns, its comment naming that rule's id                      |

- One violation per case: the snapshot records the first match's labels and fix alone (`eval(1); eval(2)` fixes `eval(1)`), the second is invisible
- The arm test: an arm deleted with no case failing and no count moving is an `uncovered arm: <id> <op> <path>` line, it leaves or gains its case
- `rule-checks.sh <ext>` deletes each `not`, `stopBy`, `nthChild`, `any` branch, and `constraints` entry in turn and blanks each `regex`
- The mutations cover `rule:`, `utils:`, and `constraints:` of each rule of the language and every global util of the language
- `has`, `inside`, and `field` stay, their mutants fail to load or match the same nodes
- A key with no sibling is deleted at the nearest list element or map that keeps one, no empty map stays behind
- A mutant the binary rejects is an `unchecked arm: <id> <op> <path> exit <code>` line, a finding until the script loads it
- An arity-pinned `pattern` binds positions through its captures, `constraints: {<VAR>: {matches: <util>}}` replaces `nthChild` arms
- A once-reporting arm changes the count and not the first match, its proof is the `width` line, one hit per `invalid:` case
- A presence guard states the kind or a value util, a `\S` over a quoted attribute value matches the quote and fails no case when blanked
- Code holding `key: value` goes in a `- |` block scalar, the plain form fails with `invalid type: map, expected a string` and exit 8
- `|` keeps the trailing newline in the snapshot key and `|-` drops it, a switch between them orphans the entry
- No schema validates a test file, the runner reads `id`, `valid`, and `invalid` and ignores every other key, `unknown key` names the rest
- A valid case passes for the wrong reason when its text parses as another kind (a callback under a pair, not an argument list), the tree decides
- A `regex` product over object and property names is proven per pair against the package exports, a pair with another meaning is a near miss
- Names are placeholders (`Item`, `load`, `<key-a>`), the comment on the line before the case names the shape, and no case restates the `message`
- A literal the rule pins (`bsamiee`, `Rasm.*`, a fixed row of `NuGet.config`) is the required case text, the one exception to placeholder names
- A regression case's comment names the gap it closed (the `stopBy: end` that reached the earlier owner), the case is the record of the defect

## [03]-[SNAPSHOTS]

The snapshot is the committed record of what the rule reported for each `invalid:` case, keyed by the exact case text, and the run compares it byte for byte:

- `test -U` writes `<testDir>/__snapshots__/<id>-snapshot.yml` with sorted keys, one entry per case: `labels`, and `fixed` when the rule has a fix
- Default labels are one primary on the match and one secondary per relational clause's node, the record of what each clause bound
- With `labels:` the entry holds the configured captures alone, a changed relational clause then leaves no trace and takes a case of its own
- `fixed` is the case with the first fix template substituted over the match range, it must re-parse, `ERROR node in fixed <id> case <n>` otherwise
- `fixed` skips `expandStart` and `expandEnd` (`foo(first, second)` yields `foo(, second)`), `replacementOffsets` alone show the consumed comma
- `-U` merges and never deletes, a renamed or re-styled case leaves its old key as an orphan the run never checks, `orphan or missing snapshot key`
- A new `invalid:` case fails `[Wrong]` as `No <id> baseline found` and needs no diff read, `-U` writes its entry
- In a `[Wrong]` diff a moved secondary is a changed relational clause, a changed `fixed` a changed template or guard, a moved primary a new target
- `-i` prompts per snapshot (`y`, `n`, `a`, `q`), and an accepted diff with labels no case explains is a rule change the review missed

## [04]-[ADVERSARIAL]

Assume the rule is wrong, write the case that shows it, and correct the rule when the case is real:

| [INDEX] | [DEVICE]                       | [DISPROVING_CASE]                                                                                   |
| :-----: | :----------------------------- | :-------------------------------------------------------------------------------------------------- |
|  [01]   | Pattern with a literal callee  | Sibling function under `invalid:`, `Missing` proves the gap                                         |
|  [02]   | `any:` branches                | Form the branches omit, the point-free constructor or the method spelling                           |
|  [03]   | `not:` guard                   | Refused variant under `valid:`, `Noisy` proves a missing guard, the kept one under `invalid:`       |
|  [04]   | `constraints` grammar          | Name one character outside it under `valid:`, one inside under `invalid:`                           |
|  [05]   | `stopBy: end`                  | Shape one node past the owner under `valid:`, the neighbor default two levels down under `invalid:` |
|  [06]   | Util the rule hides behind     | Each arm of the util as a case, a global util proven through every rule that calls it               |
|  [07]   | `fix`                          | Variant that breaks the template under `invalid:`, its `fixed` re-parsed and type-preserving, the guard it needs        |
|  [08]   | Once-reporting arm             | Nested form counted over a file, one hit                                                            |
|  [09]   | `language` and `files:`        | One known hit counted under `--filter` over a real path                                             |
|  [10]   | Element or callee name `regex` | The same attributes or arguments under another element or callee under `valid:`                     |

- A case written before the widening proves it: the sibling fails `Missing`, the widening lands, the case passes, the near miss stays `Validated`
- `-U` runs after the rule is proven and its diff is read, the snapshot is committed with the rule, and `-U` on a red run proves nothing
- A real case corrects the rule, and a real hit corrects the code
- A case no node shape satisfies ends as a residual rule or as no rule
- An arm no node shape fails (a `regex` over a type with one member) becomes the `pattern` that pins it, a spelling found on the way is the case
- A global util arm is covered once any calling rule's case fails or a count moves, and each caller's test holds a case through the base clause
- `width <id> case <n>: <hits> hits` counts each `invalid:` case alone, past one a once-reporting gap, zero a `files:` glob the case path misses
- A whole file is proven by `ast-grep-ignore: <id>` marks under `scan --error=unused-suppression`, exit 0 when each mark was needed

Each arm has the case that made it necessary and an arm without one leaves, the case list is the specification the `message` and `note` restate, and a rule with cases of one shape is a one-shape rule for `rule-hardening`.
