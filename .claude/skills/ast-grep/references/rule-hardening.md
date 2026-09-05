# [RULE_HARDENING]

Hardening rebuilds a rule until it reports the higher-order pattern its correction applies to, in every surface form, with a test per sibling and per guard, a fix where the replacement re-parses, and a place in the rules tree a later rule joins without a rename.

## [01]-[WEAKNESS]

A rule is weak when the correction its `note` states applies to a shape the rule misses, and each row names one gap with the form that closes it:

| [INDEX] | [SMELL]                                                           | [HIGHER_ORDER_FORM]                                                  |
| :-----: | :---------------------------------------------------------------- | :------------------------------------------------------------------- |
|  [01]   | One literal callee where the module's siblings share a contract   | `field: property` with a `regex` over the family, module pinned      |
|  [02]   | One spelling where the package documents data-first and method    | `any:` branch per form, the shape in a util                          |
|  [03]   | One container where the correction reads the same over another    | Util per container, one `any:` over them                             |
|  [04]   | One position where the shape is produced before it is consumed    | Match where the shape is produced, no position guard                 |
|  [05]   | Name where the rule means every node of a kind                    | `kind` with `field` and `nthChild`, `regex` on the name              |
|  [06]   | Test with the instance alone under `invalid:`                     | One `invalid:` case per sibling, one `valid:` per guard              |
|  [07]   | `note` naming the instance's fix                                  | Correction as the shape to produce                                   |
|  [08]   | One hit where a file of the sibling shapes reports many           | Every sibling the package contract admits, counted over that file    |
|  [09]   | Two rules with messages that differ by a callee name              | One rule, or two over one global util                                |
|  [10]   | Default `stopBy` where the related node sits levels away          | `stopBy: end` or a same-kind stopper                                 |
|  [11]   | `constraints` on a `$$$` capture or under `not`                   | Structural guard in the rule, `$ITEM` inside the list                |
|  [12]   | Fix with no `not:` arm where a variant breaks the template        | `not:` arm per variant before the template                           |
|  [13]   | Kind chain standing for a role in a depth or count rule           | `inside` with the parent kind, `has` on its field, near misses valid |
|  [14]   | One carrier module where the package exports the function on more | `regex` over the carriers on `field: object`, a case per carrier     |
|  [15]   | Name guard no row of a fixed-schema file can violate              | The row as one tag `regex`, the guard leaves                         |

Read every rule in scope against the table before any edit, and record each hit as `rule | row | sibling missed`, because the widening starts from the missed siblings. Use `rule-building` for the category of weak code a rule under-covers.

## [02]-[PATTERN]

The higher-order pattern is real when every sibling takes the same correction for the same reason, and the sequence proves it before the rule is widened:
1. State the correction as a category in one line, the shape before, the shape after, and the reason, with no instance name in it
2. Enumerate the siblings from the package: each function with the same contract, each spelling, carrier, and container, the arrow and point-free arm
3. Apply the correction to each sibling on paper, and judge sameness by the finding judgment of `rule-building`, a changed shape is another rule
4. Write one file holding every sibling and near miss, and count the widened rule over it, because a tree without the shapes counts zero either way
5. Widen in the rule file and count with `--filter`, the one route that resolves a global util
6. Confirm the family against a maintained set, `github` `search_code` for the construct in a `.yml` rule, and take a guard the set found
7. Keep the widened rule when the count rose by the siblings alone, and a hit that is code the correction breaks returns to the sameness judgment
8. Prove the widening with the new sibling case, failed before the widening and passed after it

The pair is a one-shape rule and its higher-order form, the two-arm branch over a carrier, where the first version named one module and one arm spelling:

```yaml
# BEFORE, one callee, one arm spelling, the note names the instance
rule:
  pattern: Boolean.match($FLAG, { onTrue: () => Effect.fail($ERROR), onFalse: () => Effect.void })
note: Use Effect.filterOrFail on the flag
```

```yaml
# AFTER, the family in a global util, the arms in local utils, every carrier and both arm spellings
utils:
  fail-arm:
    any:
      - {kind: arrow_function, has: {field: body, any: [{pattern: Effect.fail($_)}, {pattern: Effect.failCause($_)}]}}
      - pattern: Effect.fail
  pass-arm:
    any:
      - {kind: arrow_function, has: {field: body, any: [{pattern: Effect.void}, {pattern: Effect.succeed($_)}]}}
      - pattern: Effect.succeed
rule:
  matches: effect-two-arm-branch # Global util: Effect.if, or Boolean, Option, Either, Exit .match
  has:
    field: arguments
    has:
      kind: object
      all:
        - has: {kind: pair, has: {field: value, matches: fail-arm}}
        - has: {kind: pair, has: {field: value, matches: pass-arm}}
note: Guard a boolean with Effect.filterOrFail, an Option, Either, or Exit is an Effect already, Effect.mapError renames its failure
```

The AFTER form reports the instance, the mirrored arms, the same branch on each carrier, and the point-free constructors, and its `valid:` cases hold the dispatch (one arm runs an operation) and the value selection (both arms succeed), the near misses the sameness judgment found.

## [03]-[COLLAPSE]

Rules collapse into one when they share the correction and the reason, and they stay split behind one global util when the message or the fix diverges:
1. Name the survivor for the pattern, `no-<pattern>`, and keep the id that states the pattern when one exists, a member's name is the wrong id
2. Move the shape the rules share into a global util `utils/<language>/<package>-<shape>.yml` at two references, and keep a one-reference shape local
3. Merge every `invalid:` case of the superseded rules into the survivor's test, then every `valid:` case, and comment each case with its shape
4. Delete the superseded rule, test, and snapshot files, and remove each old id from suppression comments (`rg 'ast-grep-ignore.*<id>'`) and filters
5. Run `ast-grep test -U` once, then `ast-grep test`, and read the survivor's snapshot for every original invalid source
6. Prove the count, `ast-grep scan --filter '^<survivor>$' --json=stream <root> | wc -l` is at least the sum of the superseded counts

A widened rule keeps its id while the id still names the pattern the `message` states, and takes the pattern's name through the collapse steps once the id names one member of it (`no-copy-task-into-output` over `Copy` and `Move` keeps its id, a rule over `Copy` alone named for `DestinationFolder` renames). A collapse fails when the survivor's `note` lists a fix per branch, because the reader then picks by branch, and two rules over one global util is the form that keeps each `note` one shape.

## [04]-[DEVICES]

Each device answers one symptom the widening raised, and a device with no symptom in the rule is bloat. Use the `ast-grep` skill's rule-craft devices for the relational, capture, and pattern-parse mechanics:

| [INDEX] | [SYMPTOM]                                                  | [DEVICE]                                                          |
| :-----: | :--------------------------------------------------------- | :---------------------------------------------------------------- |
|  [01]   | Sub-shape repeated in two `any:` arms of one rule          | Local `utils:` entry                                              |
|  [02]   | Shape two rules match on                                   | Global util `<package>-<shape>` with `id` and `language`          |
|  [03]   | Shared shape with one slot each caller fills               | Parameterized global util, `arguments:` beside a `kind` guard     |
|  [04]   | Nested form of the shape (a parenthesized number)          | Util recursion through `matches` under `has` or `inside`          |
|  [05]   | Shape the `message` names as one noun                      | Util with that noun as its id, the rule reads as the sentence     |
|  [06]   | Capture the fix re-matches inside the shared shape         | Local util binding it, or an argument rule binding it at the call |
|  [07]   | Family of one base shape and its narrower forms            | Base util, one local util per refinement over the base            |
|  [08]   | Single capture that must fit a name grammar, outside `not` | `constraints: {<VAR>: {regex, kind}}`                             |
|  [09]   | Derived text in the fix, a function of one capture         | `transform` with `convert`, `replace`, or `substring`             |
|  [10]   | Fix over each member of a list, joined by one separator    | `rewriters` with `rewrite($$$LIST, joinBy=<sep>)`                 |

A device proves itself through `test_match_code_rule` on the sibling that needed it and on the near miss it must keep out, and a device that changes neither result leaves.

A util turns the sibling list into variants before the rule widens: the shape every sibling shares is the util's base clause under its `kind`, each sibling that narrows it is a refinement (a local util or an `any:` arm of the util), and the rule references the util in one clause, the family widens in the util and the rule keeps one shape:
- A util carries a `kind` or an `any:` of kinds at its rule root, `matches: <slot>`, `regex`, `not:`, and a relational clause infer none
- A util under `not:` or `inside:` supplies no kind to its caller, the caller's positive clause does
- A caller left with no kind aborts at load with `Rule must specify a set of AST kinds to match`
- An export that collides with a caller binding retries no `any:` arm inside the util, the call fails whole
- An argument no clause of the body `matches` binds nothing, and a caller capture placed in it expands empty in `message` and `fix`
- A global util's own capture named in a caller's `message` expands empty and in its `fix` aborts the load
- A local util inside a global util file reads that file's arguments
- Recursion goes through `has` or `inside`, and `matches` to the util's own id under `all`, `any`, or `not` is a cycle the load rejects
- A parameterized util names non-parameterized global utils alone, another parameterized util or itself fails at load as `Rule <id> is not defined`
- A shape two parameterized utils share is inlined in each of them
- A util with `has: {stopBy: end}` walks the subtree of every node its kind admits, a `field` or a `stopBy` rule bounds the walk
- The kind guard bounds the candidates, and an `any:` of kinds walks every node of each kind

Each failure class has the check that finds it:

| [INDEX] | [FAILURE]                                         | [CHECK]                                                                            |
| :-----: | :------------------------------------------------ | :--------------------------------------------------------------------------------- |
|  [01]   | Util hiding the rule's shape, `matches:` alone    | Rule states its own kind, position, and guards, the util the shared shape          |
|  [02]   | Global util one rule references                   | `one rule calls util` of `rule-checks.sh` goes local, `no rule calls util` deletes |
|  [03]   | Util without a kind at its root                   | `no kind at util root` from `rule-checks.sh`, a kind in each `any:` arm             |
|  [04]   | Cycle through `matches`                           | Exit 8 at load naming the cyclic dependency, the recursion moved under `has`       |
|  [05]   | Global util naming an undefined util              | Exit 0 and zero hits, the load skips the check, a count over a known hit shows it  |
|  [06]   | Argument bound to nothing                         | Every declared argument appears as `matches: <slot>` in the util body              |
|  [07]   | Draft naming a global util under `--inline-rules` | Exit 8, `scan -c <scratch>/sgconfig.yml`, its `utilDirs` the real directory        |

A util is reviewed through its callers because `--inspect entity` prints `rule` and `file` entities and no util: a calling rule's `entity|rule` line after a load that did not exit 8 proves registration, a scratch rule `matches: <id>` under a config with `utilDirs` set to the real utils directory counts the util's shape alone, and each util case sits in the test of a calling rule because a test naming a util id prints `Configuration not found!`.

## [05]-[FIXES]

A fix attaches when its replacement re-parses and every other gate accepts it, and the residual variants stay findings. Use the `ast-grep` skill's rewrite shapes and rule-craft devices for the guard stack and the range expansion:
1. List every variant the template breaks (a guard, an export, a discard, a valueless member, a comment inside) and add one `not:` arm per variant
2. Widen the rule first and attach the fix second, because a template proven on the instance breaks on the sibling the widening admitted
3. Split the residual into a sibling rule with `severity: error`, no `fix`, and a `note` stating the manual correction, and the scan still blocks
4. Read the fix as the snapshot's `fixed:` text, run `scan -U` twice, and a second `Applied` line means a nested match the rule must report once

A fix the `note` describes with a condition ("when the value is a local") is two fixes, and the conditional half is a guard arm or a residual rule.

## [06]-[ARCHITECTURE]

A widening moves a rule when the pattern it now reports belongs to another owner, and the `ast-grep` skill's rules tree holds the scoping keys and the discovery rules:
- A rule widened past one package joins the directory of the syntax both packages read, and the id keeps the pattern it states
- A rule that a widening splits by message leaves both halves in the directory of the shared global util, and the pair is read together
- A prefix in the id is what `--filter '^<prefix>'` selects, and a directory groups files without selecting a rule
- A grammar the binary lacks joins as a `customLanguages` entry with a `libraryPath` per target triple, its `expandoChar`, and its `outlineRules`
- A rule that needs a type, a scope, or a second file leaves the tree for the binding (`@ast-grep/napi`, `ast-grep-py`) and its per-argument checks

## [07]-[MAINTAINED_SETS]

Maintained rule sets show the forms that hold up, and each row is the criterion a hardened rule meets with the form the set uses:

| [INDEX] | [FORM]                                                          | [CRITERION]                                                     |
| :-----: | :-------------------------------------------------------------- | :-------------------------------------------------------------- |
|  [01]   | Callee bound to `$IDENT` with `has: {pattern: $IDENT = <api>}`  | Alias of the API is the same violation as the direct call       |
|  [02]   | Util that recurses through `has` over a binary chain            | Chain of allowed members is allowed whole, one arm per side     |
|  [03]   | `not: {has: {stopBy: end, kind, all: [...]}}` over an argument  | Absence of a required argument reads the whole argument subtree |
|  [04]   | `pattern: {context, selector}` with `inside: {not: {has: <m>}}` | Member is a violation in an unmarked owner                      |
|  [05]   | `constraints` entry carrying `kind`, `any`, and `not`           | Capture itself is the subject of a composite rule               |
|  [06]   | Fixable rule beside an unfixable sibling stating the reason     | Alias used across the file has no one-node fix                  |
|  [07]   | `files:` over the package, `ignores:` over the replacement      | Module that implements the correction is exempt from it         |
|  [08]   | `rules/<language>/<category>/` with flat `tests/__snapshots__/` | Snapshots key by id, and the tree deepens without moving them   |
|  [09]   | One rule per pattern with `languageGlobs` on the superset       | No `-ts` and `-tsx` suffix variants                             |
|  [10]   | CI step pairing each rule with a test by id, counting cases     | Rule that matches nothing is found before it goes dead          |

A rule set fails when a rule is `regex` alone, when a `files:` glob opens with `./`, when the test key is spelled outside the schema, or when a rule with no scan hit stays, and each is a hardening finding.
