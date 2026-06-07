# [CODE_DOCUMENTATION_STANDARDS]

Code documentation exists only when a public caller needs semantics the declaration cannot express. Signatures, annotations, schemas, shell declarations, SQL objects, and catalogs own machine shape; source comments own omitted caller-visible obligations, outcomes, failure channels, side effects, resource contracts, security exposure, lifecycle signals, and resolvable routes.

## [1]-[USE_WHEN]

Apply this standard when writing or reviewing source-level documentation for:
- public visible types, members, functions, methods, modules, packages, properties, scripts, command functions, SQL functions, routines, policies, and catalog objects.
- public error, fault, result, validation, effect, exit-status, or SQLSTATE variants and their observable channels.
- generic type parameters, type forms, shell namerefs, SQL argument modes, catalog identity, or receiver contracts where the declaration omits caller meaning.
- lifecycle, resource, concurrency, interop, security, data-exposure, runtime-context, transaction, lock, trap, cleanup, or terminal-runner obligations a caller must honor.
- inline rationale for a non-obvious implementation, migration, shell, query, planner, security, or resource choice.

Omit comments that restate source-owned facts: signatures, obvious accessors, private implementation detail, field names, column names, shell syntax, SQL syntax, command names, type facts, branches, or implementation steps. Route generated mirrors, catalogs, architecture, support, tasks, operations, and prose mechanics through the decision router below.

[AUTHORING_CONTRACT]:
- Agent use: decide whether a caller-visible surface needs source documentation, which semantic fields the declaration omits, and which adjacent routes change.
- Produced structure: opening lead, `Use when`, decision router, produced shape, generated handoff, surface model, language capsules, lifecycle references, anti-patterns, boundaries, and validation.
- Cardinality: one public-surface decision per comment, one generated-reference handoff only when adjacent output changes, and one language capsule only where the source language applies.
- Adjacent checks: API for generated reference, README for public entrypoints, reference for lookup facts, architecture for invariants, support matrix for lifecycle, runbook for operational failure response, and how-to or tutorial for usage paths.
- Maintenance triggers: public surface added, removed, renamed, retyped, made visible, given a new failure carrier, changed side effect, changed generated-reference anchor, changed schema catalog comment, changed script command, or changed caller-visible invariant.
- Stale prevention: source comments document semantics that source shape cannot express; generated catalogs are regenerated or linked, not rebuilt by hand.

## [2]-[DECISION_ROUTER]

Review a surface before writing a comment. Public visibility creates a documentation question, not an automatic comment requirement.

[REPAIR_FIRST]:
- Prefer stronger source shape before prose: rename, retype, add annotation, schema metadata, SQL catalog comment, ShellCheck directive, or generated route when that owner can carry the fact.
- Treat a comment needed to explain a confusing public shape as a design signal; keep it only when the caller-visible obligation cannot move into source shape or canonical documentation without weakening correctness.
- Keep examples as semantic probes for likely misuse, not syntax proof, happy-path tutorials, command transcripts, or generated output dumps.

[DOCUMENT_WHEN]:
- The declaration omits caller obligation, unit, ownership, trusted boundary, context requirement, transaction state, lock state, shell environment, SQL role, tenant scope, receiver invariant, or catalog identity.
- The result, failure carrier, exit status, SQLSTATE, stream item, validation accumulation, effect channel, or generated value needs caller-visible meaning beyond the type.
- The surface exposes side effects, cancellation, retry, resource ownership, terminal execution, native-boundary conversion, security behavior, data exposure, lifecycle, or routed reference obligations.

[OMIT_WHEN]:
- The declaration, annotation, type parameter, schema metadata, shell declaration, SQL object, or catalog comment already carries the complete caller-visible fact.
- The proposed text echoes a symbol name, parameter type, return carrier, nullability annotation, shell syntax, SQL syntax, field name, column name, command name, branch, or implementation step.
- The surface is private implementation detail and no inline rationale is needed for a non-obvious boundary, migration, query, planner, shell, security, or resource choice.

[ROUTE_AWAY]:
- Generated or contract-backed API mirrors belong in [api.md](api.md) or the generated API reference.
- Curated lookup facts, command dictionaries, source-map facts, and package/tool facts belong in [reference.md](reference.md) or a route-specific reference leaf.
- Public entry maps belong in [readme.md](readme.md); folder ownership, invariants, and codemaps belong in [architecture.md](../explanation/architecture.md).
- Support, lifecycle, compatibility, migration status, task procedure, learning path, and operational response belong in [support-matrix.md](support-matrix.md), task routes, learning routes, or [runbook.md](../task/runbook.md).
- Prose mechanics inside comments belong in [style-guide.md](../style-guide.md).
- Comments and generator-consumed metadata must not contain secrets, personal data, tenant identifiers, credential routes, private host paths, exploit steps, or privileged assumptions.

## [3]-[PRODUCED_SHAPE]

Produced source comments use the smallest syntax the language toolchain parses. Do not publish a manual profile label in a source comment unless the toolchain defines that exact tag.

A source-comment decision uses this review record:

```text template
Surface: `<public symbol, command function, script surface, SQL object, policy, or catalog object>`
Profile: `<pure, rail, throwing, script, or catalog>`
Machine-shape source: `<signature, annotation, schema, shell declaration, SQL object, catalog, or generator>`
Semantic comment fields: `<purpose, caller obligation, result, failure, side effect, resource, security, lifecycle, or route fields the declaration omits>`
Failure carrier: `<typed error, exception, SQLSTATE, exit status, terminal runner rejection, or omit when absent>`
Comment syntax: `<XML docs, TSDoc, Google docstring, Bash contract comment, COMMENT ON, or omit when not applicable>`
Generated consumer: `<compiler XML, DocFX, API Extractor, TypeDoc, Griffe, mkdocstrings, catalog output, UID, or anchor; omit when absent>`
Adjacent route: `<README, API reference, reference leaf, architecture, support matrix, runbook, how-to, tutorial, or omit when unchanged>`
Omit when: `<signature, annotation, schema, catalog, shell declaration, or SQL object already carries the fact>`
```

[SECTION_CARDINALITY]:
- Opening lead, `Use when`, `Decision router`, `Produced shape`, `Surface model`, `Language capsules`, `Lifecycle references`, `Anti-patterns`, `Boundaries`, and `Validation`: required, single.
- Language capsule: required for C# 14, TypeScript 7, Python 3.15, Bash 5.3+, and PostgreSQL 18.4 when the source language applies.
- Generated-reference handoff: conditional; include only when a source-comment fact changes a generated mirror or adjacent maintained route.
- Lifecycle-tag record: conditional; include only when an external support contract consumes a lifecycle marker.
- Example: conditional; include only beside non-obvious misuse where prose alone is likely to fail.

## [4]-[GENERATED_HANDOFF]

Use a generated-reference handoff only when a public comment changes contract behavior, generated output, or an adjacent reader action. When an API page already carries `Generation command` or proof fields, this handoff links the generated anchor and avoids duplicating those fields; when a clarity fix preserves behavior, no adjacent document changes are required.

[GENERATED_HANDOFF]:
- `Changed fact`: public symbol behavior, source-comment contract, generated anchor, failure carrier, catalog comment, script command, or public surface.
- `Consumed by`: README, generated API reference, reference leaf, architecture codemap, support matrix, runbook, how-to, tutorial, or generated catalog.
- `Use in this document`: caller behavior, generated reference text, route, invariant, failure handling, security boundary, or safe edit.
- `Update when`: public symbol, source comment, generated output, catalog comment, failure carrier, script command, or adjacent route changes.
- `Close when`: generated reference and consuming route are updated, or route-away explicitly declines the fact.
- `Route-away`: README, api, reference, architecture, roadmap, runbook, how-to, tutorial, or support body that stays in its route.
- Optional local fields: `Public surface`, `Source paths`, `Generated reference`, `Generation command`, `Failure carriers`, `Evidence`, `Generated from`, `Controlling source`, and `Review trigger`; proof and freshness label meanings stay in [proof.md](../proof.md).

## [5]-[SURFACE_MODEL]

Choose one public-surface profile during authoring and review. A comment is complete when a caller can use the surface correctly and handle every outcome from the declaration plus comment without reading the body.

| [INDEX] | [PROFILE] | [SURFACE]                   | [COMMENT_OWNS]                      | [FAILURE]          |
| :-----: | :-------- | :-------------------------- | :---------------------------------- | :----------------- |
|   [1]   | Pure      | total values/facts          | purpose, obligations                | absent             |
|   [2]   | Rail      | result/effect/status        | success, faults, runtime, resources | typed data/status  |
|   [3]   | Throwing  | native exception boundary   | exception contract, cause           | thrown types       |
|   [4]   | Script    | Bash command surfaces       | stdout, stderr, state, traps        | exit status        |
|   [5]   | Catalog   | PostgreSQL catalog surfaces | meaning, security, planner, route   | exposed `SQLSTATE` |

Use only fields whose semantics the source does not already carry. Omit absent fields instead of filling placeholders.

[SEMANTIC_FIELDS]:
- Summary: required, single; states controlling purpose in the surface-kind shape.
- Caller obligation: parameter, environment value, receiver, SQL argument, shell nameref, runtime service, transaction, lock, or context meaning.
- Result semantics: returned value, effect, validation result, generator item, stream, catalog row, status envelope, exit payload, or failure carrier meaning.
- Failure channel: typed errors, accumulated validation, actual thrown exceptions, SQLSTATE exposure, Bash exit codes, process status, or terminal-runner rejection.
- Side effect: state mutation, artifact writes, time or IO, resource allocation or disposal, work start, external-state change, row locks, catalog changes, or stdout/stderr.
- Cancellation or resource contract: token, deadline, async iterator, scope, runtime context, disposable, retry policy, trap, file descriptor, cursor, transaction, or lock obligation.
- Security or data exposure: auth, tenant scope, row-level security, security definer behavior, leakproof behavior, access class, secrets, personal data, public catalog visibility, or trace/log output.
- Auxiliary routes: lifecycle, cross-reference route, value or attribute entry, remarks or elaboration, actual exception or throw entry, and inline rationale when they change caller action.

Functional, result-oriented, and expression-style APIs document observable channels instead of imperative control flow. Every rail surface states the success value and observable side effect when present, every failure variant, fault, accumulated-validation meaning, status branch, SQLSTATE exposure, or exit-status meaning a caller can receive, and the cancellation, retry, resource, clock, IO, runtime-context, trap, transaction, lock, scope, or terminal-runner requirement the call imposes.

State boundary details only when the public surface crosses them: native exception conversion, process exit, SQL error conversion, host fault conversion, deferred execution, transaction commit, resource release, or generated-reference mirroring. A comment that says only `returns Fin<T>`, `Effect<A, E, R>`, `Result[T, E]`, `Promise<T>`, SQL return type, or another typed carrier without naming failure variants is incomplete.

## [6]-[LANGUAGE_CAPSULES]

Each language capsule uses the syntax its toolchain parses and keeps semantic content out of generated catalogs. The capsule shape is `Toolchain`, `Generated profile`, `Comment owns`, `Rail/resource rules`, `Special shapes`, `Reject`, and `Syntax cue`; examples are syntax cues, not duplicate policy bodies.

Language-version claims are target standards and route freshness through [proof.md](../proof.md). This file does not claim current repository execution on Python 3.15; current repo Python runtime claims route to `pyproject.toml`.

[CAPSULE_INDEX]:

| [INDEX] | [LANGUAGE]      | [SYNTAX]                   | [GENERATOR]                          |
| :-----: | :-------------- | :------------------------- | :----------------------------------- |
|   [1]   | C# 14           | XML documentation comments | compiler XML and DocFX               |
|   [2]   | TypeScript 7    | TSDoc                      | API Extractor and TypeDoc            |
|   [3]   | Python 3.15     | Google docstrings          | Griffe and mkdocstrings              |
|   [4]   | Bash 5.3+       | contract comments          | none by default                      |
|   [5]   | PostgreSQL 18.4 | `COMMENT ON`               | catalog comments and describe output |

### [6.1]-[C_SHARP]

Toolchain: XML documentation comments for C# 14 public API contracts; XML comments in `.cs` are the semantic owner, compiler XML is the generated mirror, and DocFX is the generated-reference profile.
Generated profile: compiler XML and DocFX; `cref` routes compiler-verifiable internal references.

[COMMENT_OWNS]:
- `<summary>` purpose.
- `<typeparam>` and `<typeparamref>` semantic type role, runtime capability, algebraic obligation, or variant family.
- `<param>` and `<paramref>` caller obligation, ownership, normalization state, trusted boundary, lifetime, or cancellation propagation.
- `<returns>` success semantics, typed failure rail, validation accumulation, effect runtime, terminal boundary, or resource ownership.
- `<value>` property value meaning.
- `<remarks>`, `<para>`, and `<list>` invariants, generated-code behavior, resource scope, retry schedule, security/data exposure, interop, or lifecycle details.

[RAIL_RESOURCE_RULES]:
- `Fin<T>`: success value and domain `Error` meanings.
- `Validation<Error,T>`: independent obligations and accumulated failure semantics.
- `Eff<RT,T>`: runtime capabilities, deferred execution, cancellation or interruption behavior, retry or repeat `Schedule`, resource scope, and terminal `Run` owner.
- `IO<T>`: boundary action, resource ownership, and execution point.
- `Bracket`: acquire/use/release ownership and whether release runs on success, failure, and cancellation.
- `K<F,T>` or trait-polymorphic APIs: algebraic obligation on `F` only when the constraint does not explain caller meaning.

[SPECIAL_SHAPES]:
- `<exception>` names actual thrown type and cause on a throwing surface only.
- `<see cref>` and `<seealso cref>` route compiler-verifiable references; `<see href>` routes external links.
- `<see langword>`, `<typeparamref>`, and `<paramref>` avoid prose echo; `<inheritdoc>` applies only when inherited semantics and copied fields are exact.
- C# 14 extension blocks document receiver invariants, static or instance extension method, property, or operator semantics, receiver type-parameter meaning, allocation/resource behavior, side effects, and failure rail.
- Records and primary constructors document normalization, equality, copy, required-member initialization ordering, external binder obligations, and invariant semantics.
- Thinktecture value objects, complex value objects, smart enums, and unions document invariant, normalization, invalid-state prevention, factory failure, equality consequence, closed vocabulary, case semantics, and exhaustive dispatch route.
- Nullable annotations and attributes own null-state; comments state domain absence, sentinel meaning, default-value pitfall, boundary conversion, or null-propagation behavior only when observable.
- Cancellation and resource comments name token propagation, observable `OperationCanceledException`, cleanup, borrowed or owned lifetime, disposal or async cleanup, lock release, and DI lifetime only when caller-visible.
[REJECT]:
- Reject: missing-comment churn where declarations already carry caller truth, unresolved `cref`, fake `<exception>` tags for typed rails, Markdown-heavy XML unless the generated renderer is the only consumer, generated-member catalogs, `<include>` files without maintained source, and lifecycle wrappers for internal surfaces that should be deleted or replaced.

[SYNTAX_CUE]:
- Syntax cue: `/// <summary>Builds one cancellable geometry import effect.</summary>`.

### [6.2]-[TYPESCRIPT]

Toolchain: TSDoc for exported TypeScript 7 `.ts` APIs that form a package, module, service, schema, model, runner, or testkit contract. TypeScript syntax, exported schemas, models, and `Effect<A, E, R>` carry machine shape; API Extractor is the strict package-API canon, and TypeDoc is the browsing renderer.
Generated profile: API Extractor for strict package API and TypeDoc for browsing.

[COMMENT_OWNS]:
- summary before the first block tag.
- `@remarks`: invariants, lifecycle, resource, retry, cancellation, security, terminal runner, or observability semantics.
- `@typeParam`: semantic generic relationship, not type-expression echo.
- `@param`: caller obligation, unit, ownership, trust boundary, or resource meaning.
- `@returns`: success, typed failure, environment, deferred execution, terminal behavior, or stream semantics.
- `@throws`: escaped exception or terminal-runner Promise rejection only; typed `E` failures belong in `@returns`, `@remarks`, or the rail contract.
- `{@link ...}` and linked `@see` blocks: resolvable public references through the TypeScript generator profile.
- `{@inheritDoc ...}`: exact inherited summary, `@remarks`, `@param`, `@typeParam`, and `@returns`; non-copied lifecycle, default, example, or deprecation tags stay explicit.
- `@packageDocumentation`: package entrypoint contract only; README or reference owns package maps and curated lookup facts.

[RAIL_RESOURCE_RULES]:
- `A`: success meaning and observable side effect.
- `E`: expected typed failure variants, recovery boundary, retryable failure classes, and terminal failure after retry exhaustion; defects route through `Cause`, `Exit`, terminal diagnostics, or Promise rejection only when exposed.
- `R`: required services, layers, runtime context, scope, and caller-owned configuration.
- Interruption: whether finalizers run, whether external APIs receive cancellation, and whether callers inspect `Exit` or `Cause`.
- Resource scope: who opens and closes `Scope`, what finalizers release, and whether release observes success, failure, and interruption.
- Retry and terminal runner: stop condition, backoff or jitter shape, retryable failure tags, schedule requirements added to `R`, runtime ownership, process exit mapping, signal handling, logging, spans/metrics, and Promise rejection at the boundary.

[SPECIAL_SHAPES]:
- `Option`: absence semantics only when absence carries domain meaning.
- `Either`: pure success and failure branches when exported as a public rail.
- `Exit` and `Cause`: all-outcome inspection, defects, and interruption only when callers observe them.
- `Stream`: item semantics, ordering, backpressure, end condition, failure channel, interruption, and resource finalizers.
- `Layer` and services: construction side effects, dependency provision, reference sharing, fresh allocation, memoization, scope, teardown, and configuration ownership.
- `satisfies`, `const` type parameters, and exact optional fields: declarations own conformance, literal inference, and omitted-versus-`undefined` shape; comments state only observable semantic consequences.
- Decorators and schema annotations: runtime metadata, registration side effects, generated consumers, cross-field invariants, parse failures, and generation routes only when public callers depend on them.
- `@public`, `@beta`, `@alpha`, and `@internal`: API Extractor release-stage contract; use at most one per exported API item when the package API rail consumes release status.
- `@deprecated`: lifecycle warning, not a release tag; include replacement path, behavior delta, migration or removal condition, generated-reference route, and review trigger.
[REJECT]:
- Reject: JSDoc type-expression syntax in `.ts`, closure-style type comments, duplicate type info in `@param`, broad `@throws` for typed `E`, bare `@inheritDoc`, `@see SomeSymbol` without a link route, TSDoc on local implementation details, generated package catalogs, release tags on internal greenfield surfaces, `@internal` as a security boundary, copied TypeDoc output, and Promise-return comments that hide the terminal `Effect` boundary.

[SYNTAX_CUE]:
- Syntax cue: `/** Imports one artifact; returns an Effect with committed receipt, ImportFailure, and required services. @public */`.

### [6.3]-[PYTHON]

Toolchain: Google docstrings for Python 3.15 public modules, classes, functions, methods, properties, protocols, and package entrypoints; PEP 257 supplies docstring placement and layout, while signatures, annotations, strict type checkers, and annotation introspection own type shape.
Generated profile: Griffe and mkdocstrings.

[COMMENT_OWNS]:
- Summary: one line that does not echo the function name.
- Extended summary: invariants, lifecycle, resource, cancellation, concurrency, security, data-exposure, schema, or interop semantics.
- `Args:`: obligation, unit, ownership, accepted semantic range, trusted boundary, or context requirement.
- `Returns:`: success payload, typed error rail, effect boundary, resource ownership, terminal behavior, converted native exceptions, and typed-error semantics.
- `Yields:`: emitted value, ordering, completion, and resource finalization for public generator or async generator contracts.
- `Receives:`: `send()` input only when the public generator accepts it and Griffe or mkdocstrings parses the section.
- `Raises:`, `Attributes:`, `Warns:`, and `Examples:`: intentionally exposed native exceptions, public attribute meaning, emitted warning categories, and non-obvious call shape, lifecycle, or failure handling.

[RAIL_RESOURCE_RULES]:
- `Result[T, E]`: `Ok` payload and each meaningful error variant in `Returns:`.
- `Option[T]`: absence semantics only when absence is not obvious from function name and type.
- Expression-style effect builders: success and failure rails, resource lifetime, terminal boundary, and whether native exceptions are converted before returning.
- anyio cancellation: caller-visible cancellation, cleanup shielding, and re-raise behavior only when cancellation changes observable semantics.
- `ExceptionGroup`: grouped boundary exceptions only when part of the supported interface.
- Native exception boundary: converted exceptions belong in `Returns:` as typed errors; intentionally exposed native exceptions belong in `Raises:`.

[SPECIAL_SHAPES]:
- Annotation owners: `annotationlib`, PEP 649, and PEP 749 govern deferred annotation inspection for generated references; PEP 695 and PEP 696 place type parameters and defaults in signatures.
- Docstring boundary: docstrings add only semantic relationships, variance obligations, runtime constraints, or caller meaning.
- Narrowing and type forms: PEP 742 `TypeIs`, PEP 747 `TypeForm`, and PEP 800 `@disjoint_base` comments document narrowing, type-expression handling, or nominal disjointness only when public callers depend on it.
- Payload shape: PEP 728, PEP 705, PEP 655, and PEP 692 let `TypedDict`, `ReadOnly`, `Required`, `NotRequired`, and `Unpack` carry payload shape.
- Runtime and schema metadata: PEP 810 lazy imports, PEP 661 `sentinel`, and PEP 814 `frozendict` document deferred import-error timing, public absence, identity, snapshot, or hashability semantics only when caller-visible; PEP 702-style deprecation belongs to external support contracts only.
- Schema owners: Pydantic and msgspec metadata own schema-facing field descriptions, aliases, strictness, immutability, generated JSON Schema, and declarative validation; beartype `Annotated` validators own runtime validation at decorated boundaries.
- Model docstrings: docstrings own model purpose, cross-field invariants, security exposure, resource obligations, and caller-visible failure semantics that schema metadata cannot carry.
- Sensitive metadata: generated schema annotations are publishable documentation and must exclude secrets, personal data, tenant IDs, credential routes, private hostnames, nonpublic paths, and real sensitive examples from `description`, `examples`, `json_schema_extra`, dataclass `doc`, and generator-consumed metadata.

[REJECT]:
- Type or signature echo: type echo in `Args:` or `Returns:`, signature-echo summaries, blanket parameter documentation, comments that compensate for missing annotations, and old string-forward-reference lore.
- Rail mismatch: `Raises:` for typed rails, validation data, warnings, and precondition violations.
- Tooling mismatch: docstring-only deprecation where tooling needs `warnings.deprecated`, public `object()` sentinels where `sentinel` fits, and mixed section dialects inside Google docstrings.
- Schema drift: schema fields documented only in prose when metadata owns them and field examples with sensitive data.

[SYNTAX_CUE]:
- Syntax cue: `"""Import one validated plan artifact."""`.

### [6.4]-[BASH]

Toolchain: Bash 5.3+ has no docstrings; use contract comments only where callers, analyzers, or maintainers need stdout, stderr, exit-status, state, trap, cleanup, environment, nameref, stream, durable-write, current-shell substitution, or ShellCheck rationale. POSIX.1-2024 appears only when a script explicitly claims portable shell semantics.
Generated profile: no generated-reference profile by default; help metadata owns command catalogs and ShellCheck directives own analyzer-control comments.

[COMMENT_OWNS]:
- Caller action, analyzer behavior, or maintenance safety only.
- Script contract, command function contract, environment contract, dispatch route, trap/cleanup ownership, nameref return, stream boundary, durable write, current-shell substitution, redaction boundary, or ShellCheck directive rationale.
- Bash-only target through shebang or ShellCheck directive; no POSIX, dash, ksh, zsh, macOS `/bin/bash`, or BusyBox compatibility mention unless tested or explicitly constrained.

[RAIL_RESOURCE_RULES]:
- Script headers: Bash baseline, command surface, write scope, environment contract, stdout shape, stderr role, exit-status vocabulary, traps, cleanup, and destructive or resource boundaries.
- Command functions: purpose, admitted arguments, globals read or written, stdout/stderr contract, exit status, side effect, and dispatch-table route.
- Nameref outputs: caller-allocated variable names, mutation ownership, structured return shape, collision rule, and whether stdout is intentionally unused.
- Dispatch and environment: route key grammar, table ownership, handler signature, metadata source, unsupported-command exit, required variables, accepted shape, absence/default behavior, export behavior, redaction rule, log sink, and missing or invalid failure status.
- Channel split: stdout is machine data only, stderr is diagnostics and logs only, and exit status is the failure channel.
- Structured output: namerefs carry structured outputs where command substitution would hide mutation or split data.

[SPECIAL_SHAPES]:
- Mutable state: comments name parsing, retry counter, stream loop, signal flag, cleanup stack, PID map, or process supervision.
- `errexit` and `ERR`: comments state ignored contexts, `pipefail` dependency, `errtrace` inheritance, and `inherit_errexit` command-substitution behavior only when the public failure rail depends on them.
- Traps and cleanup: comments state signal, `BASH_TRAPSIG`, forward target, wait owner, reentrancy guard, cleanup order, child forwarding, exit-status mapping, acquisition/release order, partial acquisition, LIFO ownership, idempotence, temporary path, same-filesystem rename assumption, sensitive `umask`, durability choice, and rollback behavior.
- Streaming loops and retry: comments state stream boundary, delimiter, subshell or current-shell mutation, backpressure, ordering, finalization, failure propagation, retryable status class, maximum attempts, capped delay, jitter source, idempotence, and terminal failure rail.
- Atomic replacement and durable write: comments name temp path, same-filesystem or same-directory assumption, destination replacement policy, `EXDEV` or `--no-copy` behavior, rollback, file flush, directory-entry flush, crash model, filesystem caveat, and proof route.
- Bash 5.3 substitution forms: `${ command; }` captures output in the current shell and preserves side effects when persisted mutation, `return`, `exit`, or positional-parameter sharing is caller-visible; `${| command; }` expands from local `REPLY` while stdout remains on the caller stream when that separation avoids a subshell.
- Timing and glob forms: `BASH_MONOSECONDS` is for elapsed monotonic duration contracts; `EPOCHREALTIME` is for wall-clock timestamps; `GLOBSORT` appears only when glob ordering is semantic; `shfmt` owns layout and parse shape only.
- ShellCheck directives: use a short rationale before the directive, name the diagnostic code and local invariant, keep the directive line machine-scannable, and scope it to the smallest complete command; `source`, `source-path`, and `shell` directives name analysis routing or dialect truth, not runtime behavior; `external-sources=true` belongs in `.shellcheckrc`.

[REJECT]:
- False documentation surface: pseudo-docstring blocks, generated Bash catalogs, comments for every function, and mechanical parameter headers.
- Source echo: comments that restate `local`, `readonly`, associative-array shape, positional numbering, or function names.
- Dialect drift: portable-shell hedging in Bash-only scripts, bare ShellCheck disables, and trailing directive rationales.
- Channel or stream drift: mixed stdout payload and logs, collection loops documented as streams, and `set -e handles errors` comments.
- Durability drift: durable-write claims backed only by temp write plus `mv`.

[SYNTAX_CUE]:
- Syntax cue: `# shellcheck shell=bash`.

### [6.5]-[POSTGRESQL]

Toolchain: PostgreSQL 18.4 `COMMENT ON` is durable schema and catalog documentation visible through catalog access; SQL source comments are local rationale only because PostgreSQL treats them as whitespace before syntax analysis. Catalog comments apply to every object kind supported by PostgreSQL `COMMENT ON`; do not turn the standard into an object grammar catalog.
Generated profile: durable generated references extract object comments from `pg_description` or `pg_shdescription`; `psql` describe output is a human smoke route.
[COMMENT_OWNS]:
- Catalog comments own durable schema meaning.
- SQL source comments own local migration, function-body, and RLS rationale only.

[RAIL_RESOURCE_RULES]:
- Objects: schemas, tables, domains, and types own modeled concept, ownership boundary, temporal meaning, tenant scope, or invariant; columns own semantic value, unit, lifecycle, generated meaning, non-obvious nullable meaning, tenant scope, or public data-exposure constraint.
- Constraints and indexes: invariant, planner purpose, operator class, selectivity assumption, uniqueness/exclusion law, or access path.
- Functions: contract, volatility, strictness, null behavior, set-returning cardinality, parallel safety, cost/rows when caller-visible, security mode, `search_path` expectation, leakproof promise, and SQLSTATE exposure.
- Procedures: contract, security mode, transaction-control limitation where caller-visible, procedure-local `SET` behavior, `search_path` expectation, privilege boundary, and SQLSTATE exposure from the procedure body.
- Routine identity: routines own catalog identity umbrella only when a generated route or `COMMENT ON ROUTINE` target spans functions and procedures.
- Policies: access invariant, command scope, role scope, `USING` and `WITH CHECK` split, permissive or restrictive combination, tenant predicate, bypass assumption, owner behavior, race, and covert-channel reasoning.
- Views: projection scope, owner privilege mode, `security_invoker`, `security_barrier`, `CHECK OPTION`, RLS policy user, function execution mode, and data-exposure rule.
- Materialized views: stored projection, freshness, `relispopulated` state, refresh owner or event, `CONCURRENTLY` eligibility, unique-index proof, stale-data tolerance, and data-exposure rule.
- Replication and extension: extensions, publications, and subscriptions own installed purpose, version gate, replication scope, provider assumption, and operational boundary.

[SPECIAL_SHAPES]:
- Migrations: comments state lock level, rewrite behavior, backfill shape, validation phase, privilege window, rollout gate, irreversibility, rollback boundary, extension gate, and smallest proof-query class without output transcripts.
- Routine bodies: comments state dynamic SQL, exception conversion, search-path hardening, lock ordering, cursor ownership, transaction behavior, and planner assumptions.
- RLS security: comments state table lookup, current-setting behavior, bypass role, owner behavior, race, leakproof boundary, and covert-channel reasoning that the policy expression alone cannot carry.
- Catalog proof functions: use `pg_description` plus `obj_description(oid, catalog)` for database-local objects, `col_description(table_oid, column_number)` for columns, and `pg_shdescription` plus `shobj_description(oid, catalog)` for shared objects.
- Generated dictionaries: derive from catalog address, `pg_identify_object`, owning catalog facts, and description functions; include catalog facts beside comments instead of copying migration prose.
- Identity: function, procedure, routine, and aggregate comment identity is determined by input-relevant argument types, not names, and `OUT`-only arguments are omitted when identity does not require them.
- Security proof: security comments on policies, RLS tables, views, materialized views, and functions need catalog proof through `pg_policies` or `pg_policy`, `pg_class.relrowsecurity`, `relforcerowsecurity`, `reloptions`, `relispopulated`, `pg_roles.rolbypassrls`, `pg_proc.proleakproof`, or `prosecdef`.
- Exposure and linting: generated dictionaries must not expose subscription `subconninfo` or credentials; `sqlfluff --dialect postgres` proves formatting and linting only, not catalog comments, object identity, privileges, data exposure, replication semantics, or semantic documentation.

[REJECT]:
- Exposure risks: secrets, credentials, privileged assumptions, exploit details, credential routes, tenant IDs, sensitive operational data, security-critical internals, private backup routes, and proof-query transcripts in `COMMENT ON`.
- Operational drift: migration status, rollout windows, and rollback plans in `COMMENT ON`.
- Catalog visibility miss: any connected database user can see database-object comments, and connected cluster users can see shared-object comments.
- Catalog drift: hand-maintained data dictionaries that duplicate catalog comments, source comments for durable schema meaning, and blanket comments like `user id`.
- Weak description calls: `obj_description(oid)` without catalog identity and one-argument `obj_description(oid)`.
- Security/freshness gaps: RLS prose without policy comments plus verification route and materialized-view comments that omit freshness and refresh contract.

[SYNTAX_CUE]:
- Syntax cue: `COMMENT ON TABLE app.account_event IS 'Append-only account event ledger.';`.

## [7]-[LIFECYCLE_REFERENCES]

Lifecycle tags preserve only external support contracts. Use `@deprecated`, `[Obsolete]`, OpenAPI `deprecated`, PEP 702-style deprecation, TypeScript release tags, and equivalent generator-visible markers only when an external caller, package, generated reference, support matrix, or compatibility policy needs a warning plus migration route.

| [INDEX] | [STATE]      | [MEANING]                                                                                                           |
| :-----: | :----------- | :------------------------------------------------------------------------------------------------------------------ |
|   [1]   | `supported`  | current public contract; no lifecycle warning                                                                       |
|   [2]   | `preview`    | external support contract is intentionally provisional and consumed by generated reference or support policy        |
|   [3]   | `deprecated` | supported with warning, replacement path, behavior delta, and migration condition                                   |
|   [4]   | `removed`    | public contract no longer exists; historical fact routes to support or migration documentation, not source comments |
|   [5]   | `internal`   | excluded from public generated reference; not a preservation device for greenfield stale surfaces                   |

Every lifecycle tag names marker, consumer, replacement path, behavioral delta, removal or migration condition, generated-reference or support route that consumes the signal, and review trigger for removal or route update. Marker text belongs in source comments; support status, dates, migration records, and procedures route to support matrix, how-to, or runbook. In greenfield internal code, delete or replace stale surfaces instead of preserving them with lifecycle tags; a deprecated tag without replacement guidance is stale-source preservation, not documentation.

Resolve every code reference through the toolchain that carries it, or omit the reference: C# uses compiler-checkable `cref`; TypeScript uses TSDoc `{@link ...}` and may pair with `@see`; `@see SomeSymbol` alone is not a link; Python uses Griffe or mkdocstrings resolvable syntax; Bash uses literal command, variable, function, and ShellCheck code spans; PostgreSQL schema-qualifies schema-contained objects in `COMMENT ON` and uses generated catalog routes through `psql` describe output or description functions.

Reserve inline comments for the reason a non-obvious choice exists. Accepted: `// Resolve eagerly to surface a missing host symbol at registration rather than first call.` Rejected: document every parameter, return type, command, column, or branch because it is public. Reason: public visibility creates a review question, not an automatic comment requirement.

## [8]-[ANTI_PATTERNS]

Reject these cross-language shapes:
- Type-restating parameter: a `<param>`, `@param`, `Args:`, Bash function header, or SQL comment entry that echoes the declared type. Replace it with unit, range, origin, obligation, catalog meaning, or delete it.
- Throw tag for typed rail: an `<exception>`, `@throws`, or `Raises:` entry for a failure the surface returns as data. Move the semantics to the result, remarks, or rail section.
- Carrier echo: a `<returns>`, `@returns`, or `Returns:` entry that restates `Fin<T>`, `Effect<A, E, R>`, `Result[T, E]`, `Promise<T>`, SQL return type, or another carrier. Replace it with success, effect, or failure semantics, or remove it.
- Hidden side effect or cancellation: a comment that describes only the returned value while the surface mutates state, writes artifacts, starts work, observes time or IO, allocates or disposes resources, locks rows, changes catalog state, or requires cancellation handling. Add the observable obligation or route the detail to the controlling API, runbook, how-to, or schema reference.
- Name-echo summary: a summary that paraphrases the symbol, command, column, or policy name. Rewrite it to the surface-kind lead shape.
- Profile or line-narration leakage: a manual-only profile label emitted into a source comment without a toolchain-local tag, or an inline comment that restates the next statement. Remove the label or delete the narration.
- Generated or lifecycle preservation: a source file or docs leaf that hand-maintains generated public surface, command lists, or SQL dictionaries, or a lifecycle marker on greenfield internal surfaces that should be deleted or replaced.

## [9]-[BOUNDARIES]

[REFERENCE_ROUTES]:
- [API reference](api.md): generated and contract-backed API reference, including generated mirrors of source comments.
- [reference](reference.md): curated lookup facts that live outside source.
- [README](readme.md): scope-local entry maps that point readers to public surfaces without cataloging every symbol.
- [architecture](../explanation/architecture.md): current route blocks, invariants, and codemaps; code comments do not carry folder architecture.
- [support matrix](support-matrix.md), [runbook](../task/runbook.md), [how-to](../task/how-to.md), and [tutorial](../learning/tutorial.md): lifecycle status, operational response, task procedures, and learning paths.

[SHARED_ROUTES]:
- [style guide](../style-guide.md): prose mechanics inside comments.
- [proof](../proof.md): source-comment, generated-reference, and catalog-output proof.
- source-map pages: generated reference or public-symbol behavior links, never source-comment standards or folder-level mini API catalogs.
- [standards README](../README.md): document-type routing, placement, lifecycle, and stale-document questions.

Source comments carry no proof details unless a language-specific generator consumes them.

## [10]-[VALIDATION]

Use this verification checklist by group:

[STRUCTURE_ROUTE]:
- [ ] The standard keeps the comment decision, produced shape, generated handoff, adjacent checks, maintenance triggers, and stale-prevention rules before language mechanics.
- [ ] The decision router repairs declaration, annotation, schema, catalog, ShellCheck, or generated-route ownership before adding prose.
- [ ] Route-away rules are visible before generated handoffs and language capsules.
- [ ] Generated-reference handoffs omit absent optional fields, keep relation fields before proof fields, and route proof/freshness semantics to [proof.md](../proof.md).
- [ ] Lifecycle-tag states come from the closed vocabulary and serve external support contracts only.

[SYMBOL_CONTRACT]:
- [ ] One public-surface profile is chosen during review, without leaking review labels into source comments.
- [ ] The comment carries the required semantic fields for that profile and does not restate declared type, return type, nullability, arity, column name, shell command syntax, SQL syntax, or schema shape.
- [ ] The lead sentence matches the surface kind and carries contract, not name echo.
- [ ] FP/ROP surfaces name success, every failure variant, runtime-context requirements, resource contracts, and terminal boundaries where caller-visible.
- [ ] Throwing and Bash surfaces name actual thrown types with causes, or stdout, stderr, exit status, traps, resources, state, and ShellCheck rationale where those affect callers.
- [ ] PostgreSQL catalog surfaces use `COMMENT ON` for durable schema meaning and SQL comments only for local rationale.

[ROUTES_GENERATION]:
- [ ] Public symbol behavior, failure carrier, source comment, generated anchor, README entrypoint, architecture invariant, script command, and catalog comment changes trigger the adjacent checks named in the authoring contract.
- [ ] C#, TypeScript, Python, Bash, and PostgreSQL comments use the syntax and tags their toolchains parse.
- [ ] Comment syntax, generated consumer, and generated route are not conflated.
- [ ] C# references use compiler-checked XML routes where the toolchain can validate them, and cross-references resolve through the controlling toolchain or maintained catalog route.

[COMMENTS_BOUNDARY]:
- [ ] Inline comments state a reason, not narration.
- [ ] Folder-level public-surface catalogs route to README, generated API reference, architecture, generated catalog output, or reference leaves instead of leaking into source comments.
- [ ] Machine-consumed ledgers and generated mirrors declare their consumer and keep parser-required shape without hand-prettifying records into ordinary prose.
- [ ] Python schema fields use generator-consumed metadata for field descriptions, examples, aliases, constraints, and generated schema facts; docstrings carry only model purpose, cross-field invariants, security exposure, and caller-visible failure or resource semantics omitted by schema metadata.
- [ ] PostgreSQL routine comments distinguish function-only planner/nullability attributes from procedure-local security and `SET` attributes.
- [ ] No anti-pattern remains.

[DOCS_ONLY_PROOF]:
- [ ] `git diff --check -- docs/standards` passes.
- [ ] Local path and anchor validation runs when headings, links, or anchors change, or a proof gap is stated.
- [ ] A stale-term scan finds no old doctrine in this file.
- [ ] C#, TypeScript, Python, Bash, SQL, static, test, bridge, and generated-reference rails stay unrun unless executable source, configs, generated artifacts, or tooling change.
