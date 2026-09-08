# [RULE_HARDENING]

Widen each rule to every form its correction covers, test siblings and guards, attach behavior-preserving fixes, and name the rule for its correction family.

## [01]-[WEAKNESS]

Compare the forms a rule reports with the correction its `note` requires, and name the form each rule misses before editing:

| [INDEX] | [SMELL]                                                           | [GENERAL_FORM]                                                       |
| :-----: | :---------------------------------------------------------------- | :------------------------------------------------------------------- |
|  [01]   | One literal callee where the module's siblings share a contract   | `field: property` with a `regex` over the family, module pinned      |
|  [02]   | One overload of a `dual` export (data-first and data-last)        | `any:` branch per overload, the shape in a util                      |
|  [03]   | One container where the correction reads the same over another    | Util per container, one `any:` over them                             |
|  [04]   | One position where the shape is produced before it is consumed    | Match where the shape is produced, no position guard                 |
|  [05]   | Name where the rule means every node of a kind                    | `kind` with `field` and `nthChild`, `regex` on the name              |
|  [06]   | Test with the instance alone under `invalid:`                     | One `invalid:` case per sibling, one `valid:` per guard              |
|  [07]   | `note` naming the instance's fix                                  | Correction as the shape to produce                                   |
|  [08]   | One hit where a file of the sibling shapes reports many           | Every sibling the package contract admits, counted over that file    |
|  [09]   | Two rules with messages that differ by a callee name              | One rule, or two over one global util                                |
|  [10]   | Default `stopBy` where the related node sits levels away          | `stopBy: end` or a same-kind stopper                                 |
|  [11]   | `constraints` on a `$$$` capture or under `not`                   | Structural guard in the rule, `$ITEM` inside the list                |
|  [12]   | Fix with no guard where a variant breaks the template             | Guard per unfixable variant, comments and required evaluation kept   |
|  [13]   | Kind chain standing for a role in a depth or count rule           | `inside` with the parent kind, `has` on its field, near misses valid |
|  [14]   | Function exposed through more modules than the rule covers        | Exporting-module `regex` on `field: object`, a case per module       |
|  [15]   | Name guard no row of a fixed schema or closed package can violate | Row as one tag `regex`, the guard leaves                             |
|  [16]   | Expensive clauses before a cheap independent `has`                | Explicit `all:` ordered by capture dependency, then cost             |

## [02]-[PATTERN]

Prove siblings take the same correction for the same reason before widening:
1. State the correction as a category in one line, shape before, shape after, and reason, with no instance name
2. Enumerate package siblings with the same contract: functions, spellings, exporting modules, containers, arrow and point-free forms
3. Apply the correction to each sibling and compare its action and reason, keeping equivalent replacement spellings together
4. Write one file holding every sibling and near miss and count the widened rule over it, because a tree without the shapes counts zero either way
5. Widen the registered rule and count with `--filter` under the root configuration that loads its utilities
6. Read primary sources or maintained rules for unresolved mechanisms, then prove each adopted guard against the correction
7. Keep the widened rule when the count rose by the siblings alone, and return a hit the correction breaks to the sameness judgment
8. Prove the widening with the new sibling case, failing before and passing after
9. Rerun `ast-grep test --filter '^<id>$'` and `width <ext> '^<id>$'`, because added siblings change existing cases' match counts

Import ownership resolves member identity, namespace or default access, aliases, and local shadowing before the rule widens, because source spelling proves no package binding, and an escaped module string stays excluded until its decoded value is known.

## [03]-[COLLAPSE]

Combine rules with the same correction and reason, and split them when scope, severity, or required action differs, sharing the common predicate alone:
1. Name the survivor `no-<pattern>` for the pattern the `message` states, and keep an existing id that states it
2. Move the shape distinct callers share into a global util `utils/<language>/<package>-<shape>.yml`, and keep a fixed one-caller shape local
3. Keep each distinct case in the survivor, reclassify cases the corrected contract invalidates, and remove duplicates
4. Delete the superseded rule, test, and snapshot files, and remove each old id from suppression comments (`rg 'ast-grep-ignore.*<id>'`) and filters
5. Accept snapshots with `ast-grep test --include-off -U --filter '^<survivor>$'`, inspect every original invalid case, and rerun `arms <ext> '^<survivor>$'`
6. Compare the survivor's findings with the union of the originals by file and range, and keep intended coverage
7. Move the deleted test's cases that covered a shared util's arms to a surviving caller, because `arms <ext>` reports them uncovered

- Ids naming one member of the pattern take the pattern's name, `no-copy-task-into-output` over `Copy` and `Move` keeps its id
- Replacement spellings share one rule when they implement the same correction and reason, and the selecting condition stays in that rule

## [04]-[MECHANISMS]

Relations stay where they establish fields, captures, or traversal stops. Shorter YAML proves no equivalent ownership, capture exports, or evaluation order, and duplication stays when parameter forwarding loses captures or document-local rewriters cannot be shared.

| [INDEX] | [SYMPTOM]                                                  | [MECHANISM]                                                              |
| :-----: | :--------------------------------------------------------- | :----------------------------------------------------------------------- |
|  [01]   | Sub-rule copied into a second `any:` arm of one rule       | Local `utils:` entry                                                     |
|  [02]   | Sub-rule copied into a second rule                         | Global util `<package>-<shape>` with `id` and `language`                 |
|  [03]   | Shared shape with one slot each caller fills               | Parameterized global util, `arguments:` beside a `kind` guard            |
|  [04]   | Nested form of the shape (a parenthesized number)          | Util recursion through `matches` under `has` or `inside`                 |
|  [05]   | Two shapes each nested inside the other                    | Mutual recursion across two global utils, leaf sets local to each        |
|  [06]   | Repeated shape the `message` names                         | Shared util named for that shape                                         |
|  [07]   | Capture the fix re-matches inside the shared shape         | Local util binding it, or an argument rule binding it at the call        |
|  [08]   | Family of one base shape and its narrower forms            | Base util, a refinement per form with `matches: <base>` at its root      |
|  [09]   | Single capture that must fit a name grammar, outside `not` | `constraints: {<VAR>: {regex, kind}}`                                    |
|  [10]   | List split into two groups by member shape                 | Two filtering rewriters over one `$$$`, one `rewrite` per group          |
|  [11]   | Fix that selects the member by the arm's shape             | Rewriter per shape emitting the name, one transform joins them           |
|  [12]   | Walk to an owner from every node of a kind, a slow rule    | Capture-free prefilter under `all:` ahead of the relational keys         |
|  [13]   | Fix text that differs by the matched node's style          | Capture and transform per exclusive `any:` arm, unbound ones empty       |
|  [14]   | Runner option words told apart by runnable input           | One walk, the runner name as the caller's slot, no per-runner branch     |
|  [15]   | Line inserted at the top of a file                         | `kind: stream`, `pattern: $FILE`, first comment guarded by `nthChild: 1` |

`replace` transforms delete a misplaced, quoted, or spaced-apart copy of the inserted line before the fix prepends it. Test each mechanism on the sibling it must match and the near miss it must exclude, and remove mechanisms that change neither result.

| [INDEX] | [FAILURE]                                            | [CHECK]                                                                          |
| :-----: | :--------------------------------------------------- | :------------------------------------------------------------------------------- |
|  [01]   | Caller repeats its util's established shape          | Keep caller-specific guards and omit kind guards the util infers                 |
|  [02]   | Zero-argument global util one rule references        | `rule-checks.sh arms <ext>`, local at one caller and deleted at none             |
|  [03]   | Util with no inferable candidate kinds               | Read the load error, supply `kind`, `pattern`, or a util with an inferable kind  |
|  [04]   | Cycle through `matches`                              | Exit 8 at load naming the cyclic dependency, the recursion moved under `has`     |
|  [05]   | Global util naming an undefined util                 | Known-hit case detects unresolved references that loading alone can accept       |
|  [06]   | Argument bound to nothing                            | Every declared argument appears as `matches: <slot>` in the util body            |
|  [07]   | Parameterized call under `has` or `inside` in a util | Call at the rule root or under `all`, `any`, or `not`, the selected subject kept |
|  [08]   | Transform named after a capture                      | `KEY` exits 8 as cyclic, `$KEY` loads and the capture shadows it in `fix`        |

## [05]-[FIXES]

Attach a fix in sequence:
1. Widen the rule before attaching the fix, because a template proven on the instance breaks on the admitted sibling
2. Widen the template to every variant the correction covers, because a rule reports no form its fix leaves uncorrected
3. Read the snapshot's `fixed:` text and the applied edit, and tell an intended nested rewrite pass from a fix that repeats unchanged work

## [06]-[ARCHITECTURE]

Rules move when their generalized correction belongs to another directory:
- Rules widened past one package join the directory of the syntax both packages read, and the id keeps the pattern it states
- Rules a widening splits by message leave both halves in the directory of the shared global util, and the pair is read together
- Prefixes in the id are what `--filter '^<prefix>'` selects, and a directory groups files without selecting a rule

## [07]-[MAINTAINED_SETS]

Forms from maintained rule sets and the criterion each meets:

| [INDEX] | [FORM]                                                          | [CRITERION]                                                     |
| :-----: | :-------------------------------------------------------------- | :-------------------------------------------------------------- |
|  [01]   | Util that recurses through `has` over a binary chain            | Chain of allowed members is allowed whole, one arm per side     |
|  [02]   | `not: {has: {stopBy: end, kind, all: [...]}}` over an argument  | Absence of a required argument reads the whole argument subtree |
|  [03]   | `pattern: {context, selector}` with `inside: {not: {has: <m>}}` | Member in an unmarked owner, default `stopBy` reads the parent  |
|  [04]   | `constraints` entry carrying `kind`, `any`, and `not`           | Capture itself is the subject of a composite rule               |
|  [05]   | Fixable rule beside an unfixable sibling stating the reason     | Alias used across the file has no one-node fix                  |
|  [06]   | `files:` over the package, `ignores:` over the replacement      | Module that implements the correction is exempt from it         |

`_` in a pattern matches literally under Rust, and each wildcard position gets a test before rules combine.
