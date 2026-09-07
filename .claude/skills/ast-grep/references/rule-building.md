# [RULE_BUILDING]

Derive rules from a refactor diff, existing code, or a project principle. Identify the correction family, integrate it with the existing rules and utilities, and handle the findings it produces.

## [01]-[SOURCES]

Start from the supplied evidence, then read the dependencies that determine the correction:
1. The supplied git diff and its before/after context, the named code, or the stated principle with one conforming and one violating form
2. The language skills, manifests, resolved versions, and installed sources of the packages the correction uses
3. The exported functions and types of the internal packages the scope depends on
4. The scoped checker rules (ruff, biome, shellcheck, analyzers), retaining mechanical rewrites without duplicating diagnostics
5. The surrounding logic and the callers, enough to establish the correction's behavior and scope
6. The brief's source findings, read through their primary evidence before becoming a rule

Use `dotnet-coding` for the C# criteria and `dotnet-msbuild-antipatterns` for the build-file criteria. Every other language expresses the same dependency, purity, and totality requirements through its own idioms and adopted packages.

Keep semantic control and callback nesting at three or fewer levels in every language by composing the result where it is produced. Prefer a direct conditional, pattern match, comprehension, or package operation that removes intermediate work. Moving branches into helpers or callback tables can retain the same complexity behind more declarations. Count semantic control and callback levels rather than indentation, treating an alternative dispatch chain as one selection and resetting control depth at a function boundary. Prefer a clear pattern dispatch over a long conditional chain.

Replace hand-written operations with the documented defaults, combinators, and options that express them. Use internal package exports instead of local copies. Compose package operations directly (an environment default, a memoized shared read, or a default keyed by a tag), and inspect the rest of the scope for uses of each discovered capability. Find repeated computation, values derivable from existing inputs, and branches the type already decides.

## [02]-[SMELLS]

Use structural search to locate candidates, then judge the correction from the language and package contracts. A matching shape alone does not prove redundant types, repeated effects, unused declarations, or interchangeable library calls.

| [INDEX] | [CATEGORY] | [CRITERION] |
| :-----: | :----- | :----- |
| [01] | Nesting | Match callbacks by argument position and parameters, then identify the operation that removes nested control flow |
| [02] | Repeated representation | Find declarations and table columns that restate a value, then inspect their consumers and domain meaning |
| [03] | Repeated computation | Match calls over the same input, then establish purity and evaluation timing before combining them |
| [04] | Forwarding | Match bodies that pass parameters to one call, then preserve domain conversion, composed policy, and required host contracts |
| [05] | Aliases | Match root bindings or type aliases to one name, and distinguish required name disambiguation from renaming alone |
| [06] | Suffix variants | Match operation names by suffix grammar, then compare their input and result contracts |
| [07] | Double assertions | Match nested assertions through `unknown` or `never`, and use the boundary's decoding operation |
| [08] | Service location | Find containers forwarding dependencies already supplied by the function or runtime |
| [09] | Threaded values | Inspect unchanged arguments across calls, preserving explicit dependencies and useful partial application |
| [10] | Constant columns | Bind the first row's value and find repeated columns, retaining fields required by the consuming contract |
| [11] | Restated configuration | Find literals duplicating a manifest, tool default, or environment flag and retain its owning declaration |
| [12] | Ordering | Find use before declaration or general rewrites before specific ones, then inspect the actual dependency |
| [13] | Missed library operation | Find hand-written loops, conversions, or native calls that an adopted package directly expresses |
| [14] | Ambient reads | Match the language's clock, randomness, and environment operations in domain functions |
| [15] | Untyped failure | Match literal failures, unknown error channels, and throwing paths against the boundary's result type |
| [16] | Separate refinement | Find a validator beside the schema that owns its value and combine them when they enforce one invariant |
| [17] | Absence by branch | Find an empty or failure arm beside a value arm, then use the owning type's absence operation |
| [18] | Rebuilt result | Find matches that reconstruct their input result type and use the documented conversion or combinator |
| [19] | Known discriminator | Find dispatch on a parameter and inspect every caller through the language tooling |
| [20] | Deprecated member | Search for the member the package deprecates and use its documented replacement |
| [21] | Fixed delay | Find literal sleeps where a blocking operation or status signal already supplies completion |
| [22] | Excessive check scope | Find commands discarding a known path or rule id and use the configured target for that scope |

For computations used only by a lazy alternative (`Option.orElse`), move the computation into its existing thunk when reference analysis proves exclusive use and deferral preserves required effects and failures. Check captured values and reads at the later evaluation point. Compare selected, skipped, and repeatedly invoked alternatives before deriving a rewrite.

Trace repeated representations from construction to their consumers. A classifier used only to select a callback table can become a direct conditional or pattern match at the consumer. Merge cases only when every consumer treats them identically under the producer's actual contract. Process exit status, diagnostic severity, and the presence of output are different facts, even when their values coincide in a fixture.

Find classifier-selected callback tables with a structural query:

```bash
ast-grep run -l tsx -p '$TABLE[$CLASSIFIER($$$INPUT)]($$$ARGS)' <path> --json=compact
```

Resolve the captured declarations and every reference through the language tooling. Retain classifications that cross an API boundary, drive different consumers, or represent distinct domain states. When the names only mediate a local choice, replace the dispatch and delete the unused classifier, table, and type together. Preserve argument evaluation count and order, callback laziness, lexical bindings, and exceptions. Named functions and methods can depend on `this`, `arguments`, or their own binding.

An alias can snapshot a mutable binding or evaluate a getter once. Resolve writes and reads before replacing it with repeated access. Repeated string spelling or documentation alone establishes no shared domain fact. Keep protocol keys, serialized values, and diagnostics at their owning boundary. Replace string-selected internal behavior with typed cases or direct operations when their consumers establish the meaning, without introducing constants that merely rename literals.

Apply the same analysis to aliases, records, classes, schemas, and constant objects: identify the fact each declaration adds, then retain validation, serialization, identity, public contracts, and reused operations. Syntax can locate construction followed by projection or classification followed by dispatch, while symbol and type information determine whether the intermediate representation has other consumers. Select the language's conditional, pattern match, tuple, or adopted package operation that states the result directly.

Use the configured modern toolchain as the starting point: Bash 5.3 operations before older 5.2/5.1 forms, Python 3.15, and TypeScript 7 where adopted. Check the actual compiler, runtime, package exports, and release-specific primary sources before choosing a feature. A newer spelling is an improvement when it reduces work while preserving the required contract, and parser lag forces no older language form.

For library replacements, compare the member contract with the code and derive families from supported operations and overloads. Bind positional arguments in their documented order and keyword arguments by name, including option terminators and overload-specific roles. Module names and capitalization do not establish equivalence. Resolve package-family compatibility before applying prerelease API guidance, and validate the correction against manifest-resolved exports and overloads. Derive each module branch from its installed export and overload, including yielded-value conversion, laziness, and success type. A constructor in one branch does not prove another branch returns that type, so every return is established before a fold becomes map or bind. Effect, Option, Either, and Exit share some combinator names but differ in supported operations and evaluation behavior.


For Python, select expression forms and adopted package operations from the configured language version. Preserve iterator consumption, binding scope, annotation evaluation, reference release, and constructor behavior. New deferred annotations do not preserve future-import string storage, and union syntax can invoke a custom metaclass operator. Use Pydantic's owning validation or serialization operation to replace manual boundary parsing, while retaining coercion, aliases, defaults, validators, error accumulation, and output contracts. A model or schema can disappear only when its consumers need none of those facts. Verify imported bindings before assigning package behavior to a familiar name. Import the documented submodule for its operation instead of assuming the root package loads it. Express field bounds through their Pydantic constraints rather than identity validators, and distinguish settings reads from environment mutation and warning emission from dependency-warning handling.

When package bindings or runtime behavior require review, a rewrite runs when its execution path restricts edits to the semantically verified selections. A severity setting or prose prerequisite does not make whole-path edits safe.
Remove an invalid rule and its fixtures instead of weakening its severity or teaching consumers to avoid supported operations.

At I/O boundaries, preserve encoding and framing. Output callbacks can receive partial lines or escape sequences, so a line logger does not replace a byte-preserving writer. Convert filesystem paths through the owning URL API, distinguishing raw paths from encoded URL components before deriving a rewrite.

For command wrappers, establish executable resolution, argument interpretation, environment, working directory, and failure behavior before removing a layer. A preceding setup in the same execution sequence can establish PATH, and a setup in another job cannot. Package runners can also set environment values, inject loaders, and check dependencies. Read the installed version's behavior before claiming equivalence. Preserve options that select packages, recursion, shells, or directories. Match a parsed command or a bounded whole-command scalar, never a substring that can occur in an argument or comment. Interpret options under the executable's parser, including terminators and the first positional script operand. When that equivalence requires semantic review, the rewrite restricts its edits to the verified selections, and otherwise a review finding stands without a whole-path replacement.

For Bash, choose the supported operation by its execution contract:
- Keep control nesting at three or fewer `if`, `for`, arithmetic `for`, `while`, and `case` levels per function
- Reduce control flow in place through direct dispatch and data operations, preserving independent failures without extracting helpers to hide depth
- Use `${ command; }` for current-shell output capture and `${| command; }` for `REPLY` capture when shared state is intended
- Preserve `$(command)` isolation, subshell directory scope, and streaming pipelines when their environment or resource lifetime matters
- Keep saved exit statuses until their consumers finish, and preserve arithmetic command status, conditional errexit behavior, and option scope
- Batch independent operands alone, with equal options, framing, output order, and failure handling, and per-item redirections kept
- Use `fd` for path discovery, `rg` for text streams, and `yq`/`jq` for structured records, preserving framing and no-match/error distinctions
- Wait on owned child PIDs or the service's completion operation, retaining deliberate delays required by a scheduling or rate policy

Both `$(command)` and `${ command; }` trim trailing newlines, while `${| command; }` preserves the value assigned to `REPLY`.
Replacing a pipeline with a here-string adds a newline and changes SIGPIPE, `pipefail`, and consumer variable scope.
Process substitution preserves streaming, but its producer status is separate from the consuming command and needs explicit handling when required.
Streaming consumers that publish no shell state need no relocation from a pipeline. A preceding `shopt` does not prove a function's invocation options.
For a final pipeline stage that fills an array, `lastpipe` requires job control to be disabled, and `pipefail` preserves producer failures.
`mapfile` and `readarray`, including `builtin` calls, succeed at EOF. Read once or use bounded batches with an array-length EOF condition.
A loop exit must run in the loop's shell context. Nested-loop breaks, subshells, asynchronous commands, and nonfinal pipeline stages cannot prove that exit.
Preserve producer failures and record boundaries, including NUL-delimited paths. Text processing after `jq` remains valid for rendered text.
Parameter case conversion follows locale rules, which can differ from explicit ASCII ranges in `tr`.
Path expansions must preserve `dirname`/`basename` behavior for roots, trailing slashes, and suffixes before replacing those commands.
Treat process reductions that depend on runtime values or execution context as reviewed optimizations, not syntax violations.
Shell options changed in a function persist after return unless `local -` saves them, and conditional invocation can suppress `errexit` inside it.
Scripts spell `$( )` and a quoted comma key, because tree-sitter-bash parses `${ cmd; }` and `${a[$a,$b]}` as one `ERROR` node that hides the whole tree from every rule, and the scripts move to those forms when a tree-sitter-bash release parses them.

Read the scope's linter and analyzer rules before searching a category. Keep their diagnostics with the checker and add only a missing project condition or mechanical correction. Check the before text under the checker's widest selection:

```bash
uv run ruff check --select ALL --isolated --target-version py315 --preview <scratch>
uv run ruff rule <code>
pnpm exec biome lint --only=<group>/<rule> <scratch>
shellcheck -o all -f gcc <scratch>
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

The categories a package member decides (missed capability, native primitive, ambient read, presence by branch, re-lifted carrier, known discriminator, deprecated member) need the package source open beside the code, and the search flags the call sites once the member is named. Categories the scope's own gates already report yield a finding and no rule. Read the rule list of each linter and analyzer the scope runs before searching a category, and leave the pattern with the gate that reports it. The probe is the before text under the checker's widest selection, `uv run ruff check --select ALL --isolated --target-version py315 --preview <scratch>` then `uv run ruff rule <code>`, and `pnpm exec biome lint --only=<group>/<rule> <scratch>` (a form Biome reports stays with Biome), and `shellcheck -o all -f gcc <scratch>` then `shellcheck --list-optional` for the optional checks, and a diagnostic leaves the pattern with the checker.

## [03]-[FIX]

Preserve evaluation count, order, laziness, error accumulation, cancellation, and resource lifetime when they affect the correction. Moving an expression from call time to definition time, repeating a condition, or eagerly consuming an iterator changes behavior even when the output parses and typechecks. Collection allocation can call length hints, and callback conversion can change capture, return, or finally scope. Narrow the rewrite to equivalent forms and keep the remaining syntax as a finding only when it still violates the stated rule. Compare enumeration and property reads: collecting keys before reading values differs from interleaving those operations when getters or proxies change the object. Array holes and own `undefined` entries differ even when their printed values match.

When a rule reports source that needs correction, apply the owning package's direct form within the authorized scope:
- Reduce representations and nesting by removing repeated facts and operations, and retain types and functions that add domain meaning
- Count matches before and after with the same rule and source paths, passing each path as a separate argument
- Record the source revision or diff with the baseline, and repeat affected measurements after concurrent source changes
- Count control and nested callable scopes under the language's depth rule, and parameters or argument position alone define no execution scope
- Correct the owning expression in place, and extract no helper
- Call the owning library directly, with the documented argument order, overload, defaults, and result composition
- Preserve privately owned mutation, and a copying method removes an explicit clone when receiver, iteration, ordering, and allocation agree
- Preserve handled errors and the result type chosen at the boundary
- Check emitted artifacts against intended behavior, and compare cases that must remain unchanged with their baseline
- The element count and the nesting count are measured before and after under the same commands, each against the baseline the scope already held
- Measure the complete affected scope, including extracted functions and their callers

Judge the reduction by what the remaining declarations and calls mean. Counts locate unnecessary structure, and do not justify deleting a domain invariant or hiding complexity in another file.

Resolve warnings from the scoped checkers before a rule is derived, and keep the language checks and the behavior comparisons of changed source apart from the rule checks.

For an optional object field, use the conditional expression directly when both branches are plain values:

```ts
const entry = (hidden: boolean, label: string): Record<string, string> => ({
  name: label,
  ...(hidden ? {} : { label }),
});
```

An Option pipeline adds value when the input already represents absence or participates in Option composition.
Introducing Option solely to reconstruct the same optional object field adds a representation and calls without removing a decision.
- A finding without the documenting source line is a guess and goes back with the line to read
- Pattern hits are `git ls-files <scope> | xargs ast-grep scan --inline-rules "$(cat <draft>)" --json=stream | wc -l` before and after the fix
- Up to 25 added lines are acceptable for a capability the scope lacked, and the element count still falls

## [04]-[DERIVATION]

Group findings by correction and reason. Instances with both in common become siblings of one rule. Split shared shapes when their corrections differ, or when different reasons change which near misses are valid. The diff supplies instances, and the language and package contracts determine the rest of the family.

Enumerate siblings per module function with the same meaning, per module that exports the function (`Option`, `Either`, `Effect`), per overload of a `dual` export (data-first and data-last), per container kind (object, array, argument list), per spelling of the same operation, and per position the shape occupies (a spread, an argument, a local, a return). Siblings are real when the correction produces the same after form from them, proven by writing the after form once per sibling. Near misses identify where the correction is inapplicable or changes required behavior, and become `valid` test cases. A correction family needs a criterion that distinguishes violations from conforming code. Use the diff's before/after forms, package overloads, and near misses to establish that criterion without manufacturing extra source edits. Compare script history with ShellCheck and the language contract. Keep residual forms as diagnostics only when the violation is established and a safe fix is unavailable.

Keep fixed predicates with one caller in the rule, and share repeated predicates through a utility refined at each caller. Give the predicate a `kind` when its node shape requires one. The near misses bound the rule to what its fix states:
- Default-parameter corrections preserve evaluation timing and argument-binding errors, including repeated positional, keyword, and unpacked values
- Guards over a position leave when the shape fixes the position, a return in both arms makes every later statement dead
- Notes state every operation a fix selects by shape, the map and the bind of one match

Derive absence rules from an operation that already returns Option and manually reconstructs its result. Keep direct boolean conditionals over plain values. Boolean.match and Match add value when their callbacks or pattern dispatch remove existing structure, not when they wrap a conditional in more functions.
- Patterns with one form and no sibling are instances, their rule waits for the second instance, and two rows derive a rule
