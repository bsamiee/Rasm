# [RULE_HARDENING]

Widen each rule to every form its correction covers, attach behavior-preserving fixes, and name the rule for its correction family.

## [01]-[WEAKNESS]

Before editing, compare the forms a rule reports with the correction its `note` requires and name the form each rule misses:

| [INDEX] | [WEAKNESS]                                                      | [GENERAL_FORM]                                                       |
| :-----: | :-------------------------------------------------------------- | :------------------------------------------------------------------- |
|  [01]   | One literal callee where the module's siblings share a contract | `field: property` with a `regex` over the family, module pinned      |
|  [02]   | One overload of a data-first and data-last pair                 | `any:` arm per overload, the shape in a util                         |
|  [03]   | One container where the correction reads the same over another  | Util per container, one `any:` over them                             |
|  [04]   | One position where the shape is produced before its consumption | Match where the shape is produced, no position guard                 |
|  [05]   | Name where the rule means every node of a kind                  | `kind` with `field` and `nthChild`, `regex` on the name              |
|  [06]   | `note` naming the instance's fix                                | Correction as the shape to produce                                   |
|  [07]   | One match where a file of the sibling shapes reports many       | Every sibling the package contract admits, counted over that file    |
|  [08]   | Two rules with messages that differ by a callee name            | One rule, or two over one global util                                |
|  [09]   | Default `stopBy` where the related node sits levels away        | `stopBy: end` or a same-kind `stopBy`                                |
|  [10]   | `constraints` on a `$$$` capture or under `not`                 | Structural guard in the rule, `$ITEM` inside the list                |
|  [11]   | Fix with no guard where a variant breaks the template           | Guard per unfixable variant, comments and required evaluation kept   |
|  [12]   | Kind chain standing for a role in a depth or count rule         | `inside` with the parent kind, `has` on its field, near misses valid |
|  [13]   | Function exposed through more modules than the rule covers      | Exporting-module `regex` on `field: object`, a case per module       |
|  [14]   | Expensive clauses before a cheap independent `has`              | Explicit `all:` ordered by capture dependency, then cost             |

## [02]-[PATTERN]

Before widening, prove siblings take the same correction for the same reason:
1. State the correction as a category in one line, shape before, shape after, and reason, with no instance name
2. Enumerate package siblings with the same contract
3. Apply the correction to each sibling and compare its action and reason, keeping equivalent replacement spellings together
4. Prove the widened rule on every sibling and near miss, over the tree or a snippet
5. Widen the registered rule and count with `--filter` under the root configuration that loads its utilities
6. Read the package source and the grammar for unresolved mechanisms and prove each adopted guard against the correction
7. Keep the widened rule when the count rose by the siblings alone, a match the correction breaks returns to the sameness judgment

Before the rule widens, import ownership resolves member identity, namespace or default access, aliases, and local shadowing. Escaped module strings stay excluded until their decoded value is known.

## [03]-[COLLAPSE]

Combine rules with the same correction and reason. Split rules when scope, severity, or required action differs, sharing the common predicate alone:
1. Name the survivor `no-<pattern>` for the pattern the `message` states, an existing id stating it stays
2. Move the shape distinct callers share into a global util, a fixed one-caller shape stays local
3. Delete the superseded rule files and each old id in suppression comments and filters
4. Compare the survivor's findings with the union of the originals by file and range, intended coverage stays

- Replacement spellings share one rule when they implement the same correction and reason, the selecting condition stays in that rule

## [04]-[MECHANISMS]

Relations stay where they establish fields, captures, or traversal stops. Duplication stays when parameter forwarding loses captures or document-local rewriters cannot be shared.

| [INDEX] | [SYMPTOM]                                                  | [MECHANISM]                                                         |
| :-----: | :--------------------------------------------------------- | :------------------------------------------------------------------ |
|  [01]   | Sub-rule copied into another `any:` arm of one rule        | Local `utils:` entry                                                |
|  [02]   | Sub-rule copied into another rule                          | Global util                                                         |
|  [03]   | Shared shape with one slot each caller fills               | Parameterized global util, `arguments:` beside a `kind` guard       |
|  [04]   | Nested form of the shape (parenthesized number)            | Util recursion through `matches` under `has` or `inside`            |
|  [05]   | Two shapes each nested inside the other                    | Mutual recursion across two global utils, leaf sets local to each   |
|  [06]   | Repeated shape the `message` names                         | Shared util named for that shape                                    |
|  [07]   | Capture the fix re-matches inside the shared shape         | Local util binding it, or an argument rule binding it at the call   |
|  [08]   | Family of one base shape and its narrower forms            | Base util, a refinement per form with `matches: <base>` at its root |
|  [09]   | Single capture that must fit a name grammar, outside `not` | `constraints: {<VAR>: {regex, kind}}`                               |
|  [10]   | List split into groups by member shape                     | One filtering rewriter over the `$$$` and one `rewrite` per group   |
|  [11]   | Fix that selects the member by the arm's shape             | Rewriter per shape emitting the name, one transform joins them      |
|  [12]   | Walk to an owner from every node of a kind, a slow rule    | Capture-free prefilter under `all:` ahead of the relational keys    |
|  [13]   | Fix text that differs by the matched node's style          | Capture and transform per exclusive `any:` arm, unbound ones empty  |

Mechanisms changing no result go.

| [INDEX] | [FAILURE]                                            | [CHECK]                                                                          |
| :-----: | :--------------------------------------------------- | :------------------------------------------------------------------------------- |
|  [01]   | Caller repeats its util's established shape          | Keep caller-specific guards and omit kind guards the util infers                 |
|  [02]   | Zero-argument global util one rule references        | Local at its one caller, deleted when no caller remains                          |
|  [03]   | Util with no inferable candidate kinds               | Read the load error, supply `kind`, `pattern`, or a util with an inferable kind  |
|  [04]   | Cycle through `matches`                              | Exit 8 at load naming the cyclic dependency, the recursion moved under `has`     |
|  [05]   | Argument bound to nothing                            | Every declared argument appears as `matches: <slot>` in the util body            |
|  [06]   | Parameterized call under `has` or `inside` in a util | Call at the rule root or under `all`, `any`, or `not`, the selected subject kept |
|  [07]   | Transform named after a capture                      | `KEY` exits 8 as cyclic, `$KEY` loads and the capture shadows it in `fix`        |

## [05]-[FIXES]

1. Widen the rule before attaching the fix, a template proven on the instance breaks on the accepted sibling
2. Widen the template to every variant the correction covers, a rule reports no form its fix leaves uncorrected

## [06]-[ARCHITECTURE]

Rules move when their generalized correction belongs to another directory:
- Rules widened past one package join the directory of the syntax both packages read, the id keeps the pattern it states
- Rules a widening splits by message keep both halves in the shared global util's directory
- Id prefixes are what `--filter '^<prefix>'` selects, a directory groups files and selects no rule
