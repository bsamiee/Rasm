# [RULE_HARDENING]

Harden rules by correction family, preserving coverage, capture bindings, and governing contracts across supported forms.

## [01]-[WEAKNESS]

Before editing, compare the forms a rule reports with the correction its `note` requires and name the form each rule misses:

| [INDEX] | [WEAKNESS]                                                      | [GENERAL_FORM]                                                         |
| :-----: | :-------------------------------------------------------------- | :--------------------------------------------------------------------- |
|  [01]   | One literal member of a contract-equivalent family              | Shared import ownership with a member predicate                        |
|  [02]   | One overload of a data-first and data-last pair                 | `any:` arm per overload, the shape in a util                           |
|  [03]   | One container where the correction reads the same over another  | Container arms over shared predicates                                  |
|  [04]   | One position where the shape is produced before its consumption | Match producer when every use takes that correction                    |
|  [05]   | Name where the rule means every node of a kind                  | `kind` with `field` and `nthChild`, `regex` on the name                |
|  [06]   | `note` naming the instance's fix                                | Correction as the shape to produce                                     |
|  [07]   | One match where a file of the sibling shapes reports many       | Every sibling the package contract accepts                             |
|  [08]   | Rules with messages that differ by a callee name                | One rule, or each over one shared global util                          |
|  [09]   | Default `stopBy` where the related node sits levels away        | `stopBy` at the owner kinds, fixed depth through `has` with `field`    |
|  [10]   | Constraint on list capture or capture bound only in negation    | Guard list members structurally, bind captures before negation         |
|  [11]   | Fix changes behavior for an accepted variant                    | Omit fix unless every variant implements the stated correction         |
|  [12]   | Kind chain standing for a role in a depth or count rule         | `inside` with the parent kind, `has` on its field, near misses valid   |
|  [13]   | Function exposed through more modules than the rule covers      | Ownership predicate per exporting module and import form               |
|  [14]   | Form that silences the rule and keeps its cause                 | Arm per spelling, relaying declaration, restated type, rename, wrapper |

## [02]-[PATTERN]

Rules widen to siblings that take the same correction for the same reason:
1. State violating shape, required shape, and governing contract
2. Enumerate package siblings with the same contract
3. Apply the correction to each sibling and compare behavior, traversals, and allocations, keeping equivalent replacement spellings together
4. Report contract-equivalent violations, keeping diagnostic coverage independent of automatic-fix coverage
5. Read the package source and the grammar for unresolved mechanisms

Resolve package references through supported imports, aliases, and visible bindings. Semantic identity needs language tooling. Escaped module names require decoded values.

## [03]-[COLLAPSE]

Combine rules with the same correction and reason. Split rules when scope, severity, or required action differs, sharing the common predicate alone:
1. Name the survivor `no-<pattern>` for the pattern the `message` states, an existing id stating it stays
2. Share repeated predicates where capture scopes permit, keeping fixed predicates used once inline
3. Delete the superseded rule files and each old id in suppression comments and filters

- Survivors report every site the originals' correction covers
- Replacement spellings share one rule when they implement the same correction and reason, the selecting condition stays in that rule

## [04]-[MECHANISMS]

Relations stay where they establish fields, captures, or traversal stops. Duplication stays when parameter forwarding loses captures or document-local rewriters cannot be shared.

| [INDEX] | [SYMPTOM]                                                  | [MECHANISM]                                                            |
| :-----: | :--------------------------------------------------------- | :--------------------------------------------------------------------- |
|  [01]   | Sub-rule copied into another `any:` arm of one rule        | Local `utils:` entry                                                   |
|  [02]   | Sub-rule copied into another rule                          | Global util                                                            |
|  [03]   | Shared shape with one slot each caller fills               | Parameterized global with kinds fixed at definition or caller          |
|  [04]   | Nested form of the shape (parenthesized number)            | Recursive util consuming strict descendants                            |
|  [05]   | Shapes each nested inside the other                        | Mutual recursion consuming strict descendants, leaf sets local         |
|  [06]   | Repeated shape the `message` names                         | Shared util named for that shape                                       |
|  [07]   | Capture the fix re-matches inside the shared shape         | Local captures for caller bindings, arguments for independent captures |
|  [08]   | Family of one base shape and its narrower forms            | Base util, a refinement per form with `matches: <base>` at its root    |
|  [09]   | Single capture that must fit a name grammar, outside `not` | `constraints: {<VAR>: {regex, kind}}`                                  |
|  [10]   | List split into groups by member shape                     | Filtering rewriter per group with explicit `joinBy`                    |
|  [11]   | Fix that selects the member by the arm's shape             | Rewriter per shape emitting the name, one transform joins them         |
|  [12]   | Fix text that differs by the matched node's style          | Exclusive arms with branch transforms and complete replacements        |

Mechanisms changing no result go.

| [INDEX] | [FAILURE]                                            | [REMEDY]                                                                        |
| :-----: | :--------------------------------------------------- | :------------------------------------------------------------------------------ |
|  [01]   | Caller repeats its util's established shape          | Keep caller-specific guards and omit kind guards the util infers                |
|  [02]   | Fixed util with one call                             | Inline unless private capture scope changes behavior                            |
|  [03]   | Util with no inferable candidate kinds               | Read the load error, supply `kind`, `pattern`, or a util with an inferable kind |
|  [04]   | Cycle through `matches`                              | Strict progress along descendants, ancestors, or one sibling direction          |
|  [05]   | Argument bound to nothing                            | Every declared argument appears as `matches: <slot>` in the util body           |
|  [06]   | Parameterized call under `has` or `inside` in a util | Shared operation owns subject selection, direct calls expose dependencies       |
|  [07]   | Capture and transform share a name                   | Give transformed text a distinct output name                                    |

## [05]-[FIXES]

1. Enumerate matched tokens, comments, wrappers, and call shapes before writing templates
2. Omit unsafe fixes when types, evaluation, binding, precedence, or comments decide the required replacement

## [06]-[ARCHITECTURE]

Rules move when their generalized correction belongs to another directory:
- Rules widened past one package join the directory of the syntax both packages read, the id keeps the pattern it states
- Rules a widening splits by message keep both halves in the shared global util's directory
- Id prefixes are what `--filter '^<prefix>'` selects, a directory groups files and selects no rule
