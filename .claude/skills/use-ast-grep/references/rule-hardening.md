# [RULE_HARDENING]

Existing rules reach every form one correction covers and keep their coverage, capture bindings, and governing contracts.

## [01]-[WEAKNESS]

Before editing, compare the forms a rule reports with the correction its `note` requires and name the form each rule misses:

| [INDEX] | [WEAKNESS]                                                      | [GENERAL_FORM]                                                         |
| :-----: | :-------------------------------------------------------------- | :--------------------------------------------------------------------- |
|  [01]   | One overload of a data-first and data-last pair                 | `any:` arm per overload, the shape in a util                           |
|  [02]   | One position where the shape is produced before its consumption | Match producer when every use takes that correction                    |
|  [03]   | Name where the rule means every node of a kind                  | `kind` with `field` and `nthChild`, `regex` on the name                |
|  [04]   | `note` naming the instance's fix                                | Correction as the shape to produce                                     |
|  [05]   | Rules with messages that differ by a callee name                | One rule, or each over one shared global util                          |
|  [06]   | Default `stopBy` where the related node sits levels away        | `stopBy` at the owner kinds, fixed depth through `has` with `field`    |
|  [07]   | Constraint on list capture or capture bound only in negation    | Guard list members structurally, bind captures before negation         |
|  [08]   | Fix changes behavior for an accepted variant                    | Omit fix unless every variant implements the stated correction         |
|  [09]   | Kind chain standing for a role in a depth or count rule         | `inside` with the parent kind, `has` on its field, near misses valid   |
|  [10]   | Form that silences the rule and keeps its cause                 | Arm per spelling, relaying declaration, restated type, rename, wrapper |
|  [11]   | Name regex requiring its module qualifier (`^Swift\.Result`)    | One regex with the qualifier optional, `^(<module>(\.\|::))?<name>`    |
|  [12]   | Name or kind the pinned package or configured grammar lacks     | Package source at its pinned release, `node-types.json` per rebuild    |

## [02]-[WIDENING]

Rules widen to siblings that take the same correction for the same reason, one arm or `regex` member per package member, overload, container, and exporting module:
1. State violating shape, required shape, and governing contract
2. Enumerate package siblings with the same contract from the package's declarations
3. Apply the correction to each sibling and compare behavior, traversals, and allocations, keeping equivalent replacement spellings together
4. Report contract-equivalent violations, keeping diagnostic coverage independent of automatic-fix coverage

Package references resolve through imports, aliases, and bindings the language's scoping makes visible:
- Nested modules see no parent import, block-local functions and constants shadow one, module-level declarations cannot clash with a value import
- Names inside a path (`f` in `a::f::g`) are no imported name
- Semantic identity takes language tooling, an escaped module name its decoded value
- Local names bind at the call in an ordered `all:` before the import compares against them, an `inside` evaluated first binds the first import and stops there

BAD, one spelling and no owner:

```yaml
rule: {pattern: readFileSync($$$ARGS)}
```

GOOD, every alias of every member, a call owned by the named package alone:

```yaml
id: no-sync-read
language: typescript
severity: error
message: $NAME reads synchronously
note: Read through the promise API
utils:
    from-fs: {has: {field: source, regex: '^.node:fs.$'}}
    member: {regex: '^(readFileSync|readSync)$'}
    named-import:
        kind: import_statement
        all:
            - matches: from-fs
            - has:
                  kind: import_specifier
                  stopBy: end
                  has: {field: name, matches: member}
                  any:
                      - has: {field: alias, pattern: $NAME}
                      - {not: {has: {field: alias, kind: identifier}}, has: {field: name, pattern: $NAME}}
    namespace-import:
        kind: import_statement
        all:
            - matches: from-fs
            - has: {kind: namespace_import, stopBy: end, has: {kind: identifier, pattern: $NAME}}
rule:
    kind: call_expression
    any:
        - all:
              - has: {field: function, kind: identifier, pattern: $NAME}
              - inside: {kind: program, stopBy: end, has: {matches: named-import}}
        - all:
              - has:
                    field: function
                    kind: member_expression
                    all:
                        - has: {field: object, pattern: $NAME}
                        - has: {field: property, matches: member}
              - inside: {kind: program, stopBy: end, has: {matches: namespace-import}}
```

Over `import {readFileSync as read, statSync} from 'node:fs'`, `import {readSync} from 'node:fs'`, `import * as fs from 'node:fs'`, and `import {readFileSync as other} from 'other'`:

```text
read('a');            read reads synchronously
readSync(1);          readSync reads synchronously
fs.readFileSync('b'); fs reads synchronously
statSync('c');
other('d');
fs.statSync('e');
```

## [03]-[COLLAPSE]

Rules with one correction and reason combine, a difference in scope, severity, or required action splits them over one shared predicate:
1. Name the survivor's file for the pattern the `message` states, an existing file name stating it stays
2. Share repeated predicates where capture scopes permit, keeping fixed predicates used once inline
3. Delete the superseded rule files and their ids in suppression comments and filters

- Survivors report every site the originals' correction covers, matching and exported captures preserved independently
- Replacement spellings share one rule when they implement the same correction and reason, the selecting condition stays in that rule

## [04]-[MECHANISMS]

Relations stay where they establish fields, captures, or traversal stops. Duplication stays when parameter forwarding loses captures, a recursive local util would pass itself as an argument (`cyclic dependency`), a parameterized util calls itself (`is not defined`), or document-local rewriters cannot be shared.

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

Mechanisms changing no result go:

| [INDEX] | [FAILURE]                                            | [REMEDY]                                                                  |
| :-----: | :--------------------------------------------------- | :------------------------------------------------------------------------ |
|  [01]   | Caller repeats its util's established shape          | Keep caller-specific guards and omit kind guards the util infers          |
|  [02]   | Fixed util with one call                             | Inline unless private capture scope or distinct arguments change behavior |
|  [03]   | Argument bound to nothing                            | Every declared argument appears as `matches: <slot>` in the util body     |
|  [04]   | Parameterized call under `has` or `inside` in a util | Rule's own `matches` makes the call, the util takes the subject directly  |

## [05]-[RECURSION]

Every recursive utility cycle moves strictly along one finite tree order, a clean load proves no cycle terminates:
- Descendant recursion consumes child syntax, ancestor recursion consumes enclosing syntax
- Sibling recursion keeps one direction and moves past its previous declaration
- Mixed `has` and `inside`, or `follows` and `precedes`, can revisit the same node
- Alias lookup can return its own initializer, structural wrapper recursion alone does not make alias recursion terminate
- Resolve a declaration once, then match its initializer with a separately terminating structural predicate
- Depth caps hide a nonterminating relation or reject accepted syntax, fix traversal direction or ownership

## [06]-[ARCHITECTURE]

Rules move when their generalized correction belongs to another directory:
- Rules widened past one package join the directory of the syntax both packages read, the id keeps the pattern it states
- Rules a widening splits by message keep both halves in the shared global util's directory
- Id prefixes are what `--filter '^<prefix>'` selects, a directory groups files and selects no rule
