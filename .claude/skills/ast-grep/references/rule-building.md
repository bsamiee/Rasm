# [RULE_BUILDING]

Derive rules from a refactor diff, existing code, or a project principle, and integrate each with the existing rules and utilities.

## [01]-[SOURCES]

Read the supplied evidence, then the dependencies that decide the correction:
1. Git diff with its before and after context, the named code, or the stated principle with one conforming and one violating form
2. Language skills, manifests, resolved versions, and installed sources of the packages the correction uses
3. Exported functions and types of the internal packages the scope depends on
4. Scoped checker rules (ruff, biome, shellcheck, analyzers), their diagnostics stay with the checker
5. Surrounding logic and callers, enough to establish the correction's behavior and scope

Nesting stays at three or fewer semantic control and callback levels, counted by scope, depth reset at a function boundary. `no-fourth-nesting-level` locates execution and control depth:
- Execution scopes are callbacks and function bodies, resolved to the package that invokes or stores each callback
- Control scopes are conditionals, loops, and exception regions, with alternative dispatch and resource cleanup counted as one operation
- Data-first calls are an operation consuming another operation, resolved to available overloads and evaluation order
- Data and schema constructors describe a value's required shape, and their braces establish no execution depth

## [02]-[SMELLS]

Structural search locates candidates, and language and package contracts judge the correction. Matching shapes alone prove no redundant type, repeated effect, unused declaration, or interchangeable library call.

| [INDEX] | [CATEGORY]               | [CRITERION]                                                                                                |
| :-----: | :----------------------- | :--------------------------------------------------------------------------------------------------------- |
|  [01]   | Nesting                  | Match callbacks by argument position and parameters, then find the operation removing nested control flow  |
|  [02]   | Repeated representation  | Find declarations and table columns that restate a value, then inspect their consumers and domain meaning  |
|  [03]   | Repeated computation     | Match calls over the same input, then establish purity and evaluation timing before combining them         |
|  [04]   | Forwarding               | Match bodies of one forwarding call, keep domain conversion, composed policy, and required host contracts  |
|  [05]   | Aliases                  | Match root bindings or type aliases to one name, and tell required name disambiguation from renaming alone |
|  [06]   | Suffix variants          | Match operation names by suffix grammar, then compare their input and result contracts                     |
|  [07]   | Double assertions        | Match nested assertions through `unknown` or `never`, and use the boundary's decoding operation            |
|  [08]   | Service location         | Find containers forwarding dependencies the function or runtime supplies                                   |
|  [09]   | Threaded values          | Inspect unchanged arguments across calls, preserving explicit dependencies and useful partial application  |
|  [10]   | Constant columns         | Bind the first row's value and find repeated columns, retaining fields required by the consuming contract  |
|  [11]   | Restated configuration   | Find literals duplicating a manifest, tool default, or environment flag and retain its owning declaration  |
|  [12]   | Ordering                 | Find use before declaration or general rewrites before specific ones, then inspect the dependency          |
|  [13]   | Missed library operation | Find hand-written loops, conversions, or native calls that an adopted package expresses                    |
|  [14]   | Ambient reads            | Match the language's clock, randomness, and environment operations in domain functions                     |
|  [15]   | Untyped failure          | Match literal failures, unknown error channels, and throwing paths against the boundary's result type      |
|  [16]   | Separate refinement      | Find a validator beside the schema that owns its value and combine them when they enforce one invariant    |
|  [17]   | Absence by branch        | Find an empty or failure arm beside a value arm, then use the owning type's absence operation              |
|  [18]   | Rebuilt result           | Find matches that reconstruct their input result type and use the documented conversion or combinator      |
|  [19]   | Known discriminator      | Find dispatch on a parameter and inspect every caller through the language tooling                         |
|  [20]   | Deprecated member        | Search for the member the package deprecates and use its documented replacement                            |
|  [21]   | Fixed delay              | Find literal sleeps where a blocking operation or status signal supplies completion                        |
|  [22]   | Excessive check scope    | Find commands discarding a known path or rule id and use the configured target for that scope              |

Classifier-selected callback tables have a structural query:

```bash
ast-grep run -l tsx -p '$TABLE[$CLASSIFIER($$$INPUT)]($$$ARGS)' <path> --json=compact
```

- Classifications that cross an API boundary, drive different consumers, or represent distinct domain states stay
- Classifiers that mediate a local choice alone become a direct conditional or pattern match at the consumer, and the table and type leave with them
- Repeated string spelling proves no shared domain fact
- Library families derive from installed exports and overloads, positional arguments in documented order and keyword arguments by name
- Every branch's return is established before a fold becomes map or bind, and a constructor in one branch proves no return type of another
- Effect, Option, Either, and Exit share combinator names and differ in supported operations and evaluation behavior
- Pydantic's owning validation or serialization operation replaces manual boundary parsing, with coercion, aliases, defaults, and validators kept
- Field bounds are Pydantic constraints in place of identity validators, and a package operation imports from its documented submodule
- `${ command; }` captures output in the current shell, `${| command; }` captures `REPLY`, `$(command)` isolates, and shared state decides the form
- Replacing a pipeline with a here-string adds a newline and changes SIGPIPE, `pipefail`, and consumer variable scope
- Final pipeline stages that fill an array need `lastpipe` with job control disabled, and `pipefail` keeps producer failures

Read the scope's linter and analyzer rules before searching a category, a category the checker already reports yields a finding and no rule, and a missing project condition or mechanical correction alone takes one. Probe the before text under the checker's widest selection:

```bash
uv run ruff check --select ALL --isolated --target-version py315 --preview <path>
uv run ruff rule <code>
pnpm exec biome lint --only=<group>/<rule> <path>
shellcheck -o all -f gcc <path>
shellcheck --list-optional
```

Flag each category with `find_code_by_rule` over a `kind` and a relational rule:

| [INDEX] | [CATEGORY]                 | [FLAG]                                                                                                     |
| :-----: | :------------------------- | :--------------------------------------------------------------------------------------------------------- |
|  [01]   | Deep nesting               | Callback kind `inside` three callback kinds, `stopBy: end` on each                                         |
|  [02]   | Elements for one fact      | Declaration with one use site, or a table with values that collapse to one rule                            |
|  [03]   | Repeated logic             | Two calls on one callee with the same argument in one scope                                                |
|  [04]   | Wrapper                    | Function body that is one call forwarding every parameter                                                  |
|  [05]   | Alias binding              | Declarator or type alias with an identifier as its `value` field at the program root                       |
|  [06]   | Suffix variant             | Name-position `regex` over the suffix grammar, a Many, All, or ByIds tail and a verb before a By clause    |
|  [07]   | Double assertion           | `as_expression` with an `as_expression` to `unknown` or `never` as its value, parentheses included         |
|  [08]   | Forwarder, service locator | Tag or container holding other services beside a scalar                                                    |
|  [09]   | Threaded value             | Parameter every callee passes unchanged to the next call                                                   |
|  [10]   | Constant column            | Pair bound on the first row and repeated on every row                                                      |
|  [11]   | Restated tool fact         | Literal that names a tool's default or a tool's configuration key                                          |
|  [12]   | Element order              | Use that precedes its declaration, a general pattern before its specific form                              |
|  [13]   | Missed capability          | Loop, probe, or conversion beside an import of the package that owns the operation                         |
|  [14]   | Native primitive           | Callee `regex` over the global names, `inside: {kind: program, has: <import of the package>}`              |
|  [15]   | Ambient read               | Global callee `regex` in one util per language, `syntax-ambient-read`, that every stability rule calls     |
|  [16]   | Untyped failure            | Fail constructor over a literal or `new Error`, `unknown` in `type_arguments`, try over an inline function |
|  [17]   | Standalone refinement      | `variable_declarator` with a value that holds the refining call, not inside the class body                 |
|  [18]   | Presence by branch         | Two-arm match with an empty arm, or with a fail arm beside a pass arm                                      |
|  [19]   | Re-lifted carrier          | Match on a carrier with arms that rebuild the carrier's own cases                                          |
|  [20]   | Known discriminator        | Parameter matched inside the callee and known at every caller                                              |
|  [21]   | Deprecated member          | Member's name, read from the deprecation in the package source                                             |
|  [22]   | Fixed delay                | Sleep callee with a number literal, an identifier with no comment naming a source line, or no signal init  |
|  [23]   | Whole-tree run             | Command row `argv` discarding its path parameter, spelling scan or test with no --filter, --files, or path |
|  [24]   | Restated environment flag  | String, template, or argv array holding the tool word and the flag, in either spelling                     |

Categories a package member decides (missed capability, native primitive, ambient read, presence by branch, re-lifted carrier, known discriminator, deprecated member) need the package source beside the code, and the search flags call sites once the member is named.

## [03]-[FIX]

Rewrites edit the semantically verified selections alone, an unverified form stays a finding when it violates the stated rule, and an invalid rule leaves with its fixtures instead of weakening:
- Count pattern hits and elements before and after under the same rule and paths, each path a separate argument, the whole affected scope included
- Up to 25 added lines pass for a capability the scope lacked, when the element count falls
- Counts locate unnecessary structure and justify no deletion of a domain invariant or hiding of complexity in another file
- Resolve warnings from scoped checkers before deriving a rule

```bash
git ls-files <scope> | xargs ast-grep scan --inline-rules "$(cat <draft>)" --json=stream | wc -l
```

## [04]-[DERIVATION]

Findings group by correction and reason, and instances with both in common become siblings of one rule:
- Shared shapes split when corrections differ, or when different reasons change which near misses are valid
- Diff supplies instances, and language and package contracts decide the rest of the family
- Patterns with one form and no sibling are instances, and the second instance derives the rule
- Siblings enumerate per module function, exporting module, `dual` overload, container kind, spelling, and position
- Siblings are real when the after form, written once per sibling, is the same
- Near misses are where the correction is inapplicable or changes required behavior, and become `valid` test cases
- Criterion derives from the diff's forms, package overloads, and near misses
- Fixed predicates with one caller stay in the rule, and repeated predicates share a utility refined at each caller
- Default-parameter corrections preserve evaluation timing and argument-binding errors, repeated positional, keyword, and unpacked values included
- Guards over a position leave when the shape fixes the position, and a return in both arms makes every later statement dead
- Notes state every operation a fix selects by shape, the map and the bind of one match
- Absence rules derive from an operation that returns Option and reconstructs its result by hand
- Direct boolean conditionals over plain values stay, `Boolean.match` and `Match` add value when they remove existing structure
