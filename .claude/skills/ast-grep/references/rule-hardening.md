# [RULE_HARDENING]

Refine each rule to report every form its correction covers. Test siblings and guards, attach behavior-preserving fixes, and name the rule for its correction family.

## [01]-[WEAKNESS]

Compare the forms a rule reports with the correction its `note` requires:

| [INDEX] | [SMELL] | [GENERAL_FORM] |
| :-----: | :----- | :----- |
| [01] | One literal callee where the module's siblings share a contract | `field: property` with a `regex` over the family, module pinned |
| [02] | One overload of a `dual` export (data-first and data-last) | `any:` branch per overload, the shape in a util |
| [03] | One container where the correction reads the same over another | Util per container, one `any:` over them |
| [04] | One position where the shape is produced before it is consumed | Match where the shape is produced, no position guard |
| [05] | Name where the rule means every node of a kind | `kind` with `field` and `nthChild`, `regex` on the name |
| [06] | Test with the instance alone under `invalid:` | One `invalid:` case per sibling, one `valid:` per guard |
| [07] | `note` naming the instance's fix | Correction as the shape to produce |
| [08] | One hit where a file of the sibling shapes reports many | Every sibling the package contract admits, counted over that file |
| [09] | Two rules with messages that differ by a callee name | One rule, or two over one global util |
| [10] | Default `stopBy` where the related node sits levels away | `stopBy: end` or a same-kind stopper |
| [11] | `constraints` on a `$$$` capture or under `not` | Structural guard in the rule, `$ITEM` inside the list |
| [12] | Fix with no guard where a variant breaks the template | Guard per unfixable variant |
| [13] | Kind chain standing for a role in a depth or count rule | `inside` with the parent kind, `has` on its field, near misses valid |
| [14] | Function exposed through more modules than the rule covers | Exporting-module `regex` on `field: object`, a case per module |
| [15] | Name guard no row of a fixed schema or closed package can violate | The row as one tag `regex`, the guard leaves |
| [16] | Expensive clauses before a cheap independent `has` | Explicit `all:` ordered by capture dependency, then cost |

Before editing, identify each rule and the supported form it misses. In ordered `all:` clauses, bind captures before their consumers and place cheap independent checks before expensive relations.

## [02]-[PATTERN]

Establish that siblings take the same correction for the same reason before widening the rule:
1. State the correction as a category in one line, the shape before, the shape after, and the reason, with no instance name in it
2. Enumerate package siblings with the same contract: functions, spellings, exporting modules, containers, arrow and point-free forms
3. Apply the correction to each sibling and compare its action and reason, keeping equivalent replacement spellings together
4. Write one file holding every sibling and near miss, and count the widened rule over it, because a tree without the shapes counts zero either way
5. Widen the registered rule and count with `--filter` under the root configuration that loads its utilities
6. Investigate unresolved mechanisms in primary sources or maintained rules, then prove any adopted guard against the correction
7. Keep the widened rule when the count rose by the siblings alone, and a hit that is code the correction breaks returns to the sameness judgment
8. Prove the widening with the new sibling case, failed before the widening and passed after it

Share import ownership predicates across corrections through parameterized utilities. Callers bind the required captures through arguments and keep their own required action. Resolve exported member identity, namespace or default access, aliases, and local shadowing, because source spelling alone establishes no package binding. Exclude escaped module strings unless their decoded values can be established.

## [03]-[COLLAPSE]

Combine rules with the same correction and reason, and separate distinct required actions behind a shared predicate:
1. Name the survivor for the pattern, `no-<pattern>`, and keep the id that states the pattern when one exists, a member's name is the wrong id
2. Move the shape distinct callers share into a global util `utils/<language>/<package>-<shape>.yml`, and keep a fixed one-caller shape local
3. Retain each distinct behavioral case in the survivor, reclassify cases invalidated by the corrected contract, and remove duplicate cases
4. Delete the superseded rule, test, and snapshot files, and remove each old id from suppression comments (`rg 'ast-grep-ignore.*<id>'`) and filters
5. Accept snapshots with `ast-grep test --include-off -U --filter '^<survivor>$'`, inspect every original invalid case, and run the scoped gate
6. Compare the survivor against the union of the original findings by file and matched range, retaining intended coverage and removing duplicates

Widened rules keep their id while the id still names the pattern the `message` states, and take the pattern's name through the collapse steps once the id names one member of it (`no-copy-task-into-output` over `Copy` and `Move` keeps its id, a rule over `Copy` alone named for `DestinationFolder` renames). Different replacement spellings can share a rule when they implement the same correction and reason. Split rules when scope, severity, or the required action differs, sharing only their common predicate.

## [04]-[MECHANISMS]

Choose mechanisms that resolve an observed matching or replacement problem.

Use ESQuery selectors for kind-only relationships when they preserve the matched subject. Keep relations that establish named fields,
captures, or traversal stops. Parameterize a utility when callers vary a structural slot, retaining fixed local predicates otherwise.
Shorter YAML alone does not establish equivalent ownership, capture exports, or evaluation order.

| [INDEX] | [SYMPTOM] | [MECHANISM] |
| :-----: | :----- | :----- |
| [01] | Sub-rule copied into a second `any:` arm of one rule | Local `utils:` entry |
| [02] | Sub-rule copied into a second rule | Global util `<package>-<shape>` with `id` and `language` |
| [03] | Shared shape with one slot each caller fills | Parameterized global util, `arguments:` beside a `kind` guard |
| [04] | Nested form of the shape (a parenthesized number) | Util recursion through `matches` under `has` or `inside` |
| [05] | Two shapes each nested inside the other | Mutual recursion across two global utils, leaf sets local to each |
| [06] | Repeated shape the `message` names | Shared util named for that shape |
| [07] | Capture the fix re-matches inside the shared shape | Local util binding it, or an argument rule binding it at the call |
| [08] | Family of one base shape and its narrower forms | Base util, a refinement per form with `matches: <base>` at its root |
| [09] | Single capture that must fit a name grammar, outside `not` | `constraints: {<VAR>: {regex, kind}}` |
| [10] | Derived text in the fix, a function of one capture | `transform` with `convert`, `replace`, or `substring` |
| [11] | Renames, an inserted attribute, or normalized quotes | `transform` chain, each stage's `source` the previous output |
| [12] | Fix over each member of a list, joined by one separator | `rewriters` with `rewrite($$$LIST, joinBy=<sep>)` |
| [13] | List split into two groups by member shape | Two filtering rewriters over one `$$$`, one `rewrite` per group |
| [14] | Only child, or the first child of a shape | `nthChild` object form, forward and reverse under `all` |
| [15] | Count of earlier siblings | Nested `follows` under `stopBy: end`, one level per count |
| [16] | Descent that an inner function must not satisfy | Closure stopper `stopBy: {kind: <function-kind>}` on `has` |
| [17] | Fix that selects the member by the arm's shape | Rewriter per shape emitting the name, one transform joins them |
| [18] | Walk to an owner from every node of a kind, a slow rule | Capture-free prefilter under `all:` ahead of the relational keys |

Test each mechanism on the sibling it must match and the near miss it must exclude. Remove mechanisms that affect neither result. Keep separate real-path scans for scope, coverage comparisons, and applied edits.

Keep the shared predicate in a util and the caller-specific position and correction in the rule. Extract at repeated use, preserving capture visibility and load ordering. Keep engine-required duplication when parameter forwarding loses captures or document-local rewriters cannot be shared.

Each failure class has the check that finds it:

| [INDEX] | [FAILURE] | [CHECK] |
| :-----: | :----- | :----- |
| [01] | Caller repeats its util's established shape | Keep caller-specific guards and omit kind guards already inferred by the util |
| [02] | Zero-argument global util one rule references | `rule-checks.sh arms <ext>`, local at one caller and deleted at none |
| [03] | Util cannot determine candidate kinds | Read the engine load error and supply `kind`, `pattern`, or a util with an inferable kind |
| [04] | Cycle through `matches` | Exit 8 at load naming the cyclic dependency, the recursion moved under `has` |
| [05] | Global util naming an undefined util | A known-hit case detects unresolved references that loading alone can accept |
| [06] | Argument bound to nothing | Every declared argument appears as `matches: <slot>` in the util body |
| [07] | Draft naming a global util under `--inline-rules` | Exit 8, `scan -c <scratch>/sgconfig.yml`, its `utilDirs` the real directory |
| [08] | Parameterized call under `has` or `inside` in a util | Call at the rule root or under `all`, `any`, or `not`, the selected subject kept |
| [09] | Transform named after a capture | `KEY` exits 8 as cyclic, `$KEY` loads and the capture shadows it in `fix` |

Review utils through their callers: `--inspect entity` emits only rule and file entities, and a calling rule's `entity|rule` line proves registration after a successful load. To count a util's matches alone, use a scratch rule with `matches: <id>` and `utilDirs` pointing to the real utilities. Place util cases in a calling rule's test, because tests naming util ids report `Configuration not found!`. Repeatedly load the real tree through the caller's `--filter` to detect intermittent load-order failures.
- Utils under `not:` or `inside:` supply no kind to their caller, the caller's positive clause does
- Callers left with no kind abort at load with `Rule must specify a set of AST kinds to match`

## [05]-[FIXES]

Attach a fix in sequence:
1. Identify the variants the template breaks, and exclude each through a guard that preserves comments and required evaluation
2. Widen the rule first and attach the fix second, because a template proven on the instance breaks on the sibling the widening admitted
3. Widen the template to a variant the correction covers, because a rule reports no form its fix leaves uncorrected
4. Read the snapshot's `fixed:` text and the applied edit, distinguishing an intended nested rewrite pass from a fix that repeats unchanged work

Use distinct rules for different required actions. When a condition merely selects the equivalent replacement spelling, keep it in the existing rule.

## [06]-[ARCHITECTURE]

Move rules when their generalized correction belongs to another directory:
- Rules widened past one package join the directory of the syntax both packages read, and the id keeps the pattern it states
- Rules a widening splits by message leave both halves in the directory of the shared global util, and the pair is read together
- Prefixes in the id are what `--filter '^<prefix>'` selects, and a directory groups files without selecting a rule
- Grammars the binary lacks join as a `customLanguages` entry with a `libraryPath` per target triple, an `expandoChar`, and `outlineRules`
- Language tooling settles type and binding facts, and the bindings compute replacements and coordinated edits across files

## [07]-[MAINTAINED_SETS]

Compare maintained rule patterns against their matching requirements:

| [INDEX] | [FORM] | [CRITERION] |
| :-----: | :----- | :----- |
| [01] | Callee bound to `$IDENT` with `has: {pattern: $IDENT = <api>}` | Alias of the API is the same violation as the direct call |
| [02] | Util that recurses through `has` over a binary chain | Chain of allowed members is allowed whole, one arm per side |
| [03] | `not: {has: {stopBy: end, kind, all: [...]}}` over an argument | Absence of a required argument reads the whole argument subtree |
| [04] | `pattern: {context, selector}` with `inside: {not: {has: <m>}}` | Member is a violation in an unmarked owner |
| [05] | `constraints` entry carrying `kind`, `any`, and `not` | Capture itself is the subject of a composite rule |
| [06] | Fixable rule beside an unfixable sibling stating the reason | Alias used across the file has no one-node fix |
| [07] | `files:` over the package, `ignores:` over the replacement | Module that implements the correction is exempt from it |
| [08] | `rules/<language>/<category>/` with flat `tests/__snapshots__/` | Snapshots key by id, and the tree deepens without moving them |
| [09] | One rule per pattern with `languageGlobs` on the superset | No `-ts` and `-tsx` suffix variants |
| [10] | CI step pairing each rule with a test by id, counting cases | Detects rules that match no fixture |

Inspect maintained rules for defects before adapting them:
- Rule that is `regex` alone, a `files:` glob opening with `./`, a test key spelled outside the runner's three, or a rule with no scan hit
- Long flat rules, one idea written once per language file, where a local `utils:` entry folds the copies
- Unused `utilDirs` entries or repeated predicates that a local utility can share
- Rule length used to judge coverage instead of distinguishing tests and snapshots

Use fixture applications to check intended findings and exclusions. Rust's `_` wildcard matches literally in patterns. Test each wildcard position before combining those rules.
