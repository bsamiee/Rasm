# [CODE_DOCUMENTATION_STANDARDS]

Code documentation exists only when a public caller needs semantics the declaration cannot express. Signatures, annotations, schemas, shell declarations, and catalog objects own machine shape; source comments own purpose, caller obligations, outcome semantics, failure channel, side effects, resource or cancellation contract, security or data exposure, lifecycle, and resolvable routes. Generated mirrors, lookup catalogs, folder architecture, task procedures, and support policy route out of source comments.

## [1][USE_WHEN]

Apply this standard when writing or reviewing source-level documentation for:
- public visible types, members, functions, methods, modules, packages, properties, scripts, command functions, SQL functions, routines, policies, and catalog objects.
- public error, fault, result, validation, effect, exit-status, or SQLSTATE variants and their observable channels.
- generic type parameters, type forms, shell namerefs, SQL argument modes, catalog identity, or receiver contracts where the declaration omits caller meaning.
- lifecycle, resource, concurrency, interop, security, data-exposure, runtime-context, transaction, lock, trap, cleanup, or terminal-runner obligations a caller must honor.
- inline rationale for a non-obvious implementation, migration, shell, query, planner, security, or resource choice.

Omit comments that restate a signature, obvious accessor, private implementation detail, field name, column name, shell syntax, SQL syntax, command name, or type fact already carried by source. Route generated reference pages through [api.md](api.md), curated lookup facts through [reference.md](reference.md), scope-local entry maps through [readme.md](readme.md), folder invariants through [architecture.md](../explanation/architecture.md), support lifecycle through [support-matrix.md](support-matrix.md), task paths through [how-to.md](../task/how-to.md) or [tutorial.md](../learning/tutorial.md), operational response through [runbook.md](../task/runbook.md), and prose mechanics inside comments through [style-guide.md](../style-guide.md).

[AUTHORING_CONTRACT]:
- Agent use: decide whether a caller-visible surface needs source documentation, which semantic fields the declaration omits, and whether the changed contract affects generated reference, README, architecture, support, runbook, how-to, or tutorial routes.
- Required produced structure: opening lead, `Use when`, comment decision, required structure, source truth, surface profiles, comment fields, rail surfaces, generated handoffs, language capsules, lifecycle tags, cross-references, anti-patterns, boundaries, and validation.
- Section cardinality: one public-surface decision per comment, one generated-reference handoff only when adjacent output changes, and language-specific rules only where the source language applies.
- Adjacent checks: API docs for generated reference, README for public entrypoints, reference for lookup facts, architecture for invariants, support matrix for lifecycle, runbook for operational failure response, and how-to or tutorial for usage paths.
- Maintenance triggers: public surface added, removed, renamed, retyped, made visible, given a new failure carrier, changed side effect, changed generated-reference anchor, changed schema catalog comment, changed script command, or changed caller-visible invariant.
- Stale prevention: source comments document semantics that the signature, annotation, schema, shell declaration, SQL object, or catalog cannot express; generated catalogs are regenerated or linked, not rebuilt by hand.

## [2][COMMENT_DECISION]

Review a surface before writing a comment. Public visibility creates a documentation question, not an automatic comment requirement.

Document the surface when:

[CALLER_SEMANTICS]:
- The declaration omits caller obligation, unit, ownership, trusted boundary, context requirement, transaction state, lock state, shell environment, SQL role, tenant scope, receiver invariant, or catalog identity.
- The result, failure carrier, exit status, SQLSTATE, stream item, validation accumulation, effect channel, or generated value needs caller-visible meaning beyond the type.
- The surface exposes side effects, cancellation, retry, resource ownership, terminal execution, native-boundary conversion, security behavior, data exposure, lifecycle, or routed reference obligations.

Omit the comment when:

[SOURCE_SHAPE]:
- The declaration, annotation, type parameter, schema metadata, shell declaration, SQL object, or catalog comment already carries the complete caller-visible fact.
- The proposed text echoes a symbol name, parameter type, return carrier, nullability annotation, shell syntax, SQL syntax, field name, column name, command name, branch, or implementation step.
- The surface is private implementation detail and no inline rationale is needed for a non-obvious boundary, migration, query, planner, shell, security, or resource choice.

Route the fact away when:

[ADJACENT_ROUTES]:
- Generated or contract-backed API mirrors belong in [api.md](api.md) or the generated API reference.
- Curated lookup facts, command dictionaries, source-map facts, and package/tool facts belong in [reference.md](reference.md) or a route-specific reference leaf.
- Public entry maps belong in [readme.md](readme.md); folder ownership, invariants, and codemaps belong in [architecture.md](../explanation/architecture.md).
- Support, lifecycle, compatibility, migration status, task procedure, learning path, and operational response belong in their support, task, learning, or runbook routes.

## [3][REQUIRED_STRUCTURE]

Produced source comments use the smallest syntax the language toolchain parses. Do not publish a profile label in a source comment unless the toolchain defines that exact tag.

A source-comment decision has this review shape:

```text template
Surface: `<public symbol, command function, script surface, SQL object, policy, or catalog object>`
Profile: `<pure, rail, throwing, script, or catalog>`
Machine-shape source: `<signature, annotation, schema, shell declaration, SQL object, catalog, or generator>`
Semantic comment fields: `<purpose, caller obligation, result, failure, side effect, resource, security, lifecycle, or route fields the declaration omits>`
Failure carrier: `<typed error, exception, SQLSTATE, exit status, terminal runner rejection, or omit when absent>`
Generated reference: `<compiler XML, DocFX, TSDoc, API Extractor, TypeDoc, Griffe, mkdocstrings, catalog output, UID, or anchor; omit when absent>`
Adjacent route: `<README, API reference, reference leaf, architecture, support matrix, runbook, how-to, tutorial, or omit when unchanged>`
Omit when: `<signature, annotation, schema, catalog, shell declaration, or SQL object already carries the fact>`
```

Section cardinality uses these groups:

[REQUIRED_CORE]:
- Opening lead: required, single; states source comments carry caller-visible semantics only.
- `Use when`, `Comment decision`, `Required structure`, `Source truth`, `Surface profiles`, `Comment fields`, `Rail surfaces`, `Generated handoffs`, `Lifecycle tags`, `Cross-references`, `Anti-patterns`, `Boundaries`, and `Validation`: required, single.
- Language capsule: required for C# 14, TypeScript 7, Python 3.15, Bash 5.3+, and PostgreSQL 18.4.

[CONDITIONAL_RECORDS]:
- Generated-reference handoff: conditional; include only when a source-comment fact changes a generated mirror or adjacent maintained route.
- Lifecycle-tag record: conditional; include only when an external support contract consumes a lifecycle marker.
- Example: conditional; include only beside non-obvious misuse where prose alone is likely to fail.

## [4][SOURCE_TRUTH]

Use the closest maintained source for each claim. Source declarations carry shape; comments carry semantic contract; generated references mirror source truth and expose anchors; README, reference, architecture, support, runbook, how-to, and tutorial documents consume only routed facts.

[TRUTH_TIERS]:
- Machine shape: types, arity, nullability, generic bounds, TypeScript signatures, Python annotations, C# declarations, Bash parameter expansion, ShellCheck directives, SQL object identity, and PostgreSQL catalog objects.
- Semantic contract: caller obligation, result meaning, effect and failure channels, side effects, cancellation, resources, lifecycle, security boundaries, data exposure, and non-obvious rationale.
- Generated reference: compiler XML, DocFX, TSDoc, API Extractor, TypeDoc, Griffe, mkdocstrings, PostgreSQL catalog comments, description functions, and `psql` describe output.
- Routed documentation: README entrypoints, generated API pages, curated reference leaves, architecture codemaps, support matrices, runbooks, how-to guides, and tutorials.

Repository semantic overlays come from `CLAUDE.md`, language skills, `Directory.Build.props`, `pyproject.toml`, `pnpm-workspace.yaml`, this standard, and generated contracts. A source-comment standard may name a generator profile, but it must not claim a configured gate or generated artifact unless repository truth or current command output proves it.

## [5][SURFACE_PROFILES]

Choose one public-surface profile during authoring and review. Each profile answers one question: how does the caller handle every observable outcome?

Pure surface
    Applies: total functions, values, and facts.
    Comment owns: purpose and caller obligations the declaration omits.
    Failure channel: absent.

Rail surface
    Applies: result, effect, validation, status, stream, or terminal-runner surfaces.
    Comment owns: success meaning, typed fault meaning, runtime requirement, resource boundary, and recovery obligation.
    Failure channel: typed data or observable status only.

Throwing surface
    Applies: actual throws or boundary interop that intentionally exposes native exceptions.
    Comment owns: native exception contract and cause.
    Failure channel: actual thrown types only.

Script surface
    Applies: Bash scripts, command functions, dispatch tables, traps, nameref outputs, and streaming loops.
    Comment owns: stdout, stderr, state, environment, trap, cleanup, and exit-status contract.
    Failure channel: explicit exit status.

Catalog surface
    Applies: SQL schemas, tables, columns, domains, types, constraints, indexes, functions, routines, policies, views, publications, subscriptions, and extensions.
    Comment owns: durable object meaning, security boundary, planner or migration rationale, and catalog extraction route.
    Failure channel: SQLSTATE where exposed.

A comment is complete when a caller can use the surface correctly and handle every outcome from the declaration plus comment without reading the body.

## [6][COMMENT_FIELDS]

Use only fields whose semantics the source does not already carry. Omit absent fields instead of filling placeholders.

[ALWAYS]:
- Summary: required, single; states controlling purpose in the surface-kind shape.

[OUTCOME_CHANNELS]:
- Caller obligation: conditional; required when a parameter, environment value, receiver, SQL argument, shell nameref, runtime service, transaction, lock, or context value imposes meaning the declaration omits.
- Result semantics: conditional; required when the surface returns a value, effect, validation result, generator item, stream, catalog row, status envelope, exit payload, or failure carrier whose meaning is not obvious from the type.
- Failure channel: conditional; required for typed errors, accumulated validation, actual thrown exceptions, SQLSTATE exposure, Bash exit codes, process status, or terminal-runner rejection.

[BOUNDARY_CONTRACTS]:
- Side effect: conditional; required when the surface mutates state, writes artifacts, observes time or IO, allocates or disposes resources, starts work, changes external state, locks rows, changes catalog state, or touches stdout/stderr.
- Cancellation or resource contract: conditional; required when a token, deadline, async iterator, scope, runtime context, disposable, retry policy, trap, file descriptor, cursor, transaction, or lock changes caller obligations.
- Security or data exposure: conditional; required when auth, tenant scope, row-level security, security definer behavior, leakproof behavior, access class, secrets, personal data, public catalog visibility, or trace/log output affects safe use.

[ROUTES_LIFECYCLE]:
- Lifecycle: conditional; required only where an external support contract consumes a deprecation, removal, preview, compatibility, or migration signal.
- Cross-reference route: optional and repeatable; include only references the controlling toolchain or maintained documentation can resolve.

[AUXILIARY_FIELDS]:
- Value or attribute entry: conditional; required for a public property, Python attribute, SQL column, Bash environment variable, config value, or schema field whose value carries a caller constraint.
- Remarks or elaboration: optional; include only invariants, lifecycle, resource, concurrency, examples, query-planner assumptions, shell reality, generated-code behavior, or interop boundaries that do not fit the summary.
- Exception or throw entry: repeatable only for actual thrown types on a throwing surface.
- Inline comment: optional; states why a non-obvious implementation, migration, shell, query, or security choice exists, never what the next line does.

A parameter entry that says only `A string`, a return entry that restates `Fin<T>` or another carrier name, a `COMMENT ON COLUMN` value that echoes the column name, or a summary that echoes the symbol name fails this model.

## [7][RAIL_SURFACES]

Functional, result-oriented, and expression-style APIs document observable channels instead of imperative control flow. The carrier or schema owns machine shape; the comment names the caller-visible rail, terminal boundary, and recovery obligation.

Every rail surface states:
1. The success value and observable side effect, when present.
2. Every failure variant, fault, accumulated-validation meaning, status branch, SQLSTATE exposure, or exit-status meaning a caller can receive.
3. The cancellation, retry, resource, clock, IO, runtime-context, trap, transaction, lock, scope, or terminal-runner requirement the call imposes.

State boundary details only when the public surface crosses them: native exception conversion, process exit, SQL error conversion, host fault conversion, deferred execution, transaction commit, resource release, or generated-reference mirroring.

An effect-surface comment that says only `returns Fin<T>`, `Effect<A, E, R>`, `Result[T, E]`, or another typed carrier without naming failure variants is incomplete. A comment omits terminal-runner, interop, transaction, or trap details only when the surface does not cross that boundary.

## [8][GENERATED_HANDOFFS]

Folder-level code catalogs, mini API pages, shell command catalogs, and hand-written SQL dictionaries are not source comments. They aggregate current public surface for agents and maintenance routes, so they belong in a scope README, generated API reference, catalog generated from PostgreSQL comments, or reference leaf.

Use a generated-reference handoff only when a public comment changes contract behavior, generated output, or an adjacent reader action. Omit optional fields whose absence does not change reader behavior.

```text template
Changed fact: `<public symbol behavior, source-comment contract, generated anchor, failure carrier, catalog comment, script command, or public surface>`
Consumed by: `<README, generated API reference, reference leaf, architecture codemap, support matrix, runbook, how-to, tutorial, or generated catalog>`
Use in this document: `<caller behavior, generated reference text, route, invariant, failure handling, security boundary, or safe edit>`
Update when: `<public symbol, source comment, generated output, catalog comment, failure carrier, script command, or adjacent route changes>`
Close when: `<generated reference and consuming route are updated, or route-away explicitly declines the fact>`
Route-away: `<README, api, reference, architecture, roadmap, runbook, how-to, tutorial, or support body that stays in its route>`
Public surface: `<entrypoint, type family, command family, SQL object, or generated reference anchor>`
Source paths: `<current paths that implement the surface>`
Generated reference: `<api.md-local output path, catalog output path, UID, or anchor; omit when absent>`
Generation command: `<exact command or API-page generation record; omit when the API page carries it>`
Failure carriers: `<typed result, fault, receipt, status, SQLSTATE, or exit status; omit when absent>`
Evidence: `<source path, command output, generated artifact, maintained source, or proof gap; omit when not needed>`
Generated from: `<command, schema, generator, compiler XML, catalog extraction, or omit when manual>`
Source of truth: `<signature, source comment, schema, shell declaration, SQL object, catalog comment, generated artifact, or maintained source>`
Last verified: YYYY-MM-DD
Review trigger: `<source, generated output, field, status, side effect, support row, route, or upstream source changes>`
```

When an API page already carries `Generation command` or proof fields, this handoff links the generated anchor and avoids duplicating those fields. When a clarity fix preserves behavior, no adjacent document changes are required.

## [9][C_SHARP_14]

Use XML documentation comments for C# 14 public API contracts. XML comments in `.cs` are semantic source truth; compiler XML is the generated mirror; DocFX is the generated-reference profile when generated .NET API documentation is produced.

[XML_TAGS]:
- `<summary>`: controlling purpose in surface-kind form.
- `<typeparam>` and `<typeparamref>`: semantic type role, runtime capability, algebraic obligation, or variant family; no `where`-clause echo.
- `<param>` and `<paramref>`: caller obligation, ownership, normalization state, trusted boundary, lifetime, or cancellation propagation; no type echo.
- `<returns>`: success semantics, typed failure rail, validation accumulation, effect runtime, terminal boundary, or resource ownership for non-void surfaces.
- `<value>`: property value meaning where the property itself is the value contract.
- `<remarks>`, `<para>`, and `<list>`: invariants, generated-code behavior, resource scope, retry schedule, security/data exposure, interop, or lifecycle details.
- `<exception>`: actual thrown type and cause on a throwing surface only.
- `<see cref>`, `<seealso cref>`, `<see href>`, `<see langword>`, `<typeparamref>`, and `<paramref>`: compiler-checked or stable route; avoid free-text symbol names where a checked route exists.
- `<inheritdoc>`: inherited generated-reference text only when inherited semantics are exact.

[LANGUAGE_RAILS]:
- `Fin<T>` comments name the success value and domain `Error` meanings.
- `Validation<Error,T>` comments state independent obligations and accumulated failure semantics.
- `Eff<RT,T>` comments name required runtime capabilities, deferred execution, cancellation or interruption behavior, retry or repeat `Schedule`, resource scope, and terminal `Run` owner when caller-visible.
- `IO<T>` comments state the boundary action, resource ownership, and execution point.
- `Bracket` comments state acquire, use, release ownership and whether release runs on success, failure, and cancellation.
- `K<F,T>` or trait-polymorphic APIs document the algebraic obligation on `F` only when the constraint does not explain caller meaning.

[DECLARATION_SHAPES]:
- C# 14 extension blocks document receiver invariants, static or instance extension semantics, receiver type-parameter meaning, allocation/resource behavior, side effects, and failure rail; generated extension grouping or marker metadata is not reader-facing doctrine.
- Records and primary constructors document normalization, equality, copy, and invariant semantics that the declaration omits.
- Thinktecture value objects, complex value objects, smart enums, and unions document domain invariant, normalization, invalid-state prevention, factory failure meaning, equality consequence, closed vocabulary, case semantics, and exhaustive dispatch route; generated factories and boilerplate stay in signatures and code generation.
- Nullable annotations and nullable attributes own null-state; comments state domain absence, sentinel meaning, default-value pitfall, boundary conversion, or null-propagation behavior only when observable.

```csharp conceptual
/// <summary>Builds one cancellable geometry import effect.</summary>
/// <typeparam name="RT">Runtime capabilities that provide the file system, clock, and geometry catalog.</typeparam>
/// <param name="source">Caller-owned source stream; disposal remains with the caller after cancellation.</param>
/// <returns>
/// An effect that succeeds with the committed import receipt, fails with an import error for decode, validation, or catalog faults, and releases staged geometry on success, failure, or cancellation.
/// </returns>
/// <remarks>
/// Retry follows the import schedule for transient catalog faults only; the application boundary owns the terminal run.
/// </remarks>
public static Eff<RT, ImportReceipt> BuildImport<RT>(ImportSource source)
    where RT : struct, HasFileSystem<RT>, HasClock<RT>;
```

[REJECT]:
- Missing-comment churn from `CS1591` where the declaration already carries complete caller truth.
- Unresolved `cref`, fake `<exception>` tags for typed rails, XML comments that justify analyzer violations, and Markdown-heavy XML unless the generated renderer is the only consumer.
- Generated-member catalogs, `<include>` files without a maintained source, and lifecycle wrappers for internal surfaces that should be deleted or replaced.

## [10][TYPESCRIPT_7]

Use TSDoc only for exported TypeScript 7 `.ts` APIs that form a package, module, service, schema, model, runner, or testkit contract. TypeScript syntax, exported schemas, models, and `Effect<A, E, R>` carry machine-checkable shape; TSDoc carries semantics a caller cannot infer safely. API Extractor is the strict comment canon, and TypeDoc is the browsing renderer.

[TSDOC_TAGS]:
- Summary: controlling purpose before the first block tag.
- `@remarks`: invariants, lifecycle, resource, retry, cancellation, security, terminal runner, or observability semantics.
- `@typeParam`: semantic generic relationship; no type-expression echo.
- `@param`: caller obligation, unit, ownership, trust boundary, or resource meaning; no type-expression echo.
- `@returns`: success, typed failure, environment, deferred execution, terminal behavior, or stream semantics.
- `@throws`: actual thrown exception or terminal-runner edge only.
- `{@link ...}` and `@see`: resolvable public references; prose-only symbol names do not count as routed references.
- `{@inheritDoc ...}`: inherited contract only when semantics are exact.
- `@packageDocumentation`: package entrypoint contract only.
- `@public`, `@beta`, `@alpha`, `@internal`, and `@deprecated`: external support or release contract; use at most one release tag per exported API.

[EFFECT_CHANNELS]:
- `A`: success meaning and observable side effect, not only the type name.
- `E`: expected typed failure variants, recovery boundary, retryable failure classes, and terminal failure after retry exhaustion.
- `R`: required services, layers, runtime context, scope, and caller-owned configuration.
- Interruption: when cancellation changes caller behavior, state whether finalizers run, whether external APIs receive cancellation, and whether callers inspect `Exit` or `Cause`.
- Resource scope: state who opens and closes `Scope`, what finalizers release, and whether release observes success, failure, and interruption.
- Retry and schedule: state stop condition, backoff or jitter shape, retryable failure tags, and whether schedule requirements add to `R`.
- Terminal runner: document only boundary APIs that cross into `Promise`, process exit, browser events, test runners, or CLI execution.

[ROP_SURFACES]:
- `Option` comments name absence semantics only when absence carries domain meaning.
- `Either` comments name pure success and failure branches when exported as a public rail.
- `Exit` and `Cause` comments name all-outcome inspection, defects, and interruption only when callers observe them.
- `Stream` comments state item semantics, ordering, backpressure, end condition, failure channel, interruption, and resource finalizers.
- `Layer` and service comments state construction side effects, dependency provision, memoization, scope, teardown, and configuration ownership.
- Terminal-runner helpers document runtime ownership, process exit mapping, signal handling, logging, spans/metrics, and Promise rejection only at the boundary.

```typescript conceptual
/**
 * Imports one artifact into the geometry catalog.
 *
 * @remarks Opens a scoped staging area, retries transient catalog faults with the import schedule,
 * and releases staged geometry on success, failure, or interruption. The CLI boundary owns the
 * terminal runner and maps terminal failures to process exit codes.
 *
 * @param source - Caller-owned readable stream positioned at the artifact boundary.
 * @returns Effect that succeeds with the committed receipt, fails with `ImportFailure` for decode,
 * persistence, or retry exhaustion, and requires `ImportStore`, `Clock`, and `Scope`.
 *
 * @public
 */
export declare const importArtifact: (
  source: ArtifactSource,
) => Effect.Effect<ImportReceipt, ImportFailure, ImportStore | Clock | Scope.Scope>;
```

[REJECT]:
- JSDoc type-expression syntax in `.ts`, closure-style type comments, duplicate type info in `@param`, broad `@throws` for typed `E`, and `@see SomeSymbol` without a link route.
- TSDoc on local implementation details, generated package catalogs, release tags on internal greenfield surfaces, copied TypeDoc output, and Promise-return comments that hide the terminal `Effect` boundary.

## [11][PYTHON_3_15]

Use Google docstrings for Python 3.15 public modules, classes, functions, methods, properties, protocols, and package entrypoints. PEP 257 supplies the docstring substrate; annotations, strict type checkers, and generated-reference tooling own type shape.

[DOCSTRING_SHAPE]:
- One-line summary: imperative or descriptive purpose that does not echo the function name.
- Extended summary: optional; use for invariants, lifecycle, resource, cancellation, concurrency, security, data-exposure, schema, or interop semantics.
- `Args:`: conditional; document obligation, unit, ownership, accepted semantic range, trusted boundary, or context requirement, not the type.
- `Returns:`: conditional for non-`None`; describe success payload, typed error rail, effect boundary, resource ownership, or terminal behavior.
- `Yields:`: conditional for generators; document emitted value and ordering.
- `Receives:`: conditional when `send()` input is part of the public generator contract.
- `Raises:`: repeatable only for exception types intentionally exposed as part of the supported interface.
- `Attributes:`: conditional for public attribute meaning not carried by annotations, schema metadata, or property docstrings.
- `Warns:`: conditional only when warnings are part of the public contract.
- `Examples:`: optional; include only where call shape, lifecycle, or failure handling is non-obvious.

[TYPE_TRUTH]:
- `annotationlib` and deferred annotation semantics govern generated-reference annotation inspection; docstrings do not duplicate annotation expressions.
- PEP 695 and PEP 696 place type parameters and defaults in signatures; docstrings state only semantic relationships, variance obligations, runtime constraints, or caller meaning.
- PEP 742 `TypeIs` predicates document the iff narrowing contract when public and non-obvious.
- PEP 728 `TypedDict` features, `ReadOnly`, `Required`, `NotRequired`, and `Unpack` carry payload shape; docstrings add semantics only when exposed shape omits meaning.
- PEP 747 `TypeForm` surfaces state whether type expressions are inspected, evaluated, normalized, preserved, or compared.
- PEP 800 `@disjoint_base` surfaces document nominal disjointness only when public narrowing or inheritance safety depends on it.
- PEP 810 lazy imports document deferred import-error timing only when it changes caller behavior.
- PEP 661 `sentinel` and PEP 814 `frozendict` carry sentinel and immutable mapping truth when exposed; comments explain public absence, identity, snapshot, or hashability semantics only when callers depend on them.
- PEP 702-style deprecation belongs to external support contracts only.

[LANGUAGE_RAILS]:
- `Result[T, E]` docstrings describe `Ok` payload and each meaningful error variant in `Returns:`.
- `Option[T]` docstrings describe absence semantics only when absence is not obvious from the function name and type.
- Expression-style effect builders document success and failure rails, resource lifetime, terminal boundary, and whether native exceptions are converted before returning.
- anyio cancellation comments state caller-visible cancellation, cleanup shielding, and re-raise behavior only when cancellation changes observable semantics.
- `ExceptionGroup` appears only when grouped boundary exceptions are part of the supported interface.
- Pydantic, msgspec, and beartype metadata own schema-facing field descriptions, validation, aliases, strictness, immutability, generated JSON Schema, and runtime validation where consumers read those surfaces; docstrings own model purpose and cross-field invariants.
- Griffe and mkdocstrings are the preferred generated-reference profile when generated Python API documentation is adopted.

```python conceptual
async def import_plan(source: PlanSource) -> Result[ImportReceipt, ImportFault]:
    """Import one validated plan artifact.

    Args:
        source: Caller-owned stream positioned at the plan payload boundary.

    Returns:
        `Ok(receipt)` after the payload decodes, validates, and commits. `Error(ImportFault)` for malformed payloads, unsupported plan versions, unavailable storage, or retry exhaustion; cancellation closes the stream adapter and re-raises after cleanup.
    """
```

[REJECT]:
- Type echo in `Args:` or `Returns:`, blanket parameter documentation, broad `Raises:` for contract violations, and comments that compensate for missing annotations.
- Old string-forward-reference lore, public `object()` sentinels where `sentinel` fits, schema fields documented only in prose when metadata owns them, and mixed section dialects inside Google docstrings.

## [12][BASH_5_3]

Bash 5.3+ has no docstrings. Use contract comments for script headers, command functions, trap handlers, streaming loops, dispatch tables, environment contracts, nameref returns, current-shell substitution, durable writes, and ShellCheck directives. POSIX.1-2024 appears only when a script explicitly claims portable shell semantics.

[SURFACE_CONTRACT]:
- Script header: Bash baseline, command surface, write scope, environment contract, stdout shape, stderr role, exit-status vocabulary, traps, cleanup, and destructive or resource boundaries.
- Command function: purpose, admitted arguments, globals read or written, stdout/stderr contract, exit status, side effect, and dispatch-table route.
- Nameref output: caller-allocated variable names, mutation ownership, structured return shape, and whether stdout is intentionally unused.
- Dispatch table: route key grammar, table ownership, handler signature, metadata source, and unsupported-command exit.
- Environment contract: required variables, validation shape, redaction rule, export behavior, and failure status.

[LANGUAGE_RAILS]:
- stdout is machine data only.
- stderr is diagnostics and logs only.
- exit status is the failure channel.
- namerefs carry structured outputs where command substitution would hide mutation or split data.
- mutable state comments name the boundary reason: parsing, retry counter, stream loop, signal flag, cleanup stack, PID map, or process supervision.

[RESOURCE_TRAPS]:
- Trap comments state signal, `BASH_TRAPSIG` use inside trap actions, reentrancy guard, cleanup order, child forwarding, and exit-status mapping.
- Cleanup comments state acquisition/release order, LIFO ownership, idempotence, temporary path, same-filesystem rename assumption, sensitive `umask`, durability choice, and rollback behavior.
- Streaming-loop comments state stream boundary, delimiter, backpressure, ordering, finalization, and failure propagation.
- Retry comments state retryable status class, maximum attempts, capped delay, jitter source, idempotence, and terminal failure rail.

[BASH_5_3_FEATURES]:
- `${ command; }` captures output in the current shell and preserves side effects; document it when persisted mutation, `return`, `exit`, or positional-parameter sharing is caller-visible.
- `${| command; }` expands from the command's local `REPLY` while stdout remains on the caller stream; document it when that separation is the reason to avoid a subshell.
- `BASH_MONOSECONDS` is for elapsed monotonic duration contracts; `EPOCHREALTIME` is for wall-clock timestamps.
- `GLOBSORT` appears only when glob ordering is part of the semantic result.
- `shfmt` owns layout and parse shape only; it does not prove comment semantics.
- ShellCheck directives are documentation: every local disable, source, source-path, or shell directive names the diagnostic class and the local invariant that makes the shape intentional.

```bash conceptual
# shellcheck shell=bash
# Runs the release command surface; stdout is the artifact path, stderr is diagnostics,
# and exits 2 for usage errors or EX_BUILD for failed build commands.
set -Eeuo pipefail
shopt -s inherit_errexit

# Resolves a public command through the dispatch table.
# Contract: $1 is the command key; $2 is a nameref output slot; stdout is unused;
# returns 2 when the key is not public.
_resolve_handler() {
  local -r command_name="${1:?command_name required}"
  local -n out="${2:?out nameref required}"
  ...
}
```

[REJECT]:
- Pseudo-docstring blocks, generated Bash catalogs, comments for every function, portable-shell hedging in Bash-only scripts, and comments that restate `local`, `readonly`, or `declare -A`.
- Bare ShellCheck disables, trailing directive rationales, mechanical parameter headers, mixed stdout data/logs, and collection loops documented as streams.

## [13][POSTGRESQL_18_4]

Use PostgreSQL 18.4 `COMMENT ON` for durable schema and catalog documentation. SQL source comments are local rationale only; PostgreSQL treats them as whitespace before syntax analysis.

[CATALOG_COMMENTS]:
- Schemas, tables, domains, and types: modeled concept, ownership boundary, temporal meaning, tenant scope, or invariant that names and constraints omit.
- Columns: semantic value, unit, lifecycle, generated meaning, non-obvious nullable meaning, tenant scope, or public data-exposure constraint; omit name/type echo.
- Constraints and indexes: invariant, planner purpose, operator class, selectivity assumption, uniqueness/exclusion law, or access path.
- Functions, procedures, and routines: contract, volatility, strictness, null behavior, set-returning cardinality, parallel safety, cost/rows when caller-visible, security mode, search-path expectation, leakproof promise, and SQLSTATE exposure.
- Policies: access invariant, command scope, role scope, `USING` and `WITH CHECK` split, permissive or restrictive combination, tenant predicate, bypass assumption, owner behavior, race, and covert-channel reasoning.
- Views and materialized views: security mode, freshness, refresh contract, barrier or invoker behavior, projection scope, and data-exposure rule.
- Extensions, publications, and subscriptions: installed purpose, version gate, replication scope, provider assumption, and operational boundary.

[SOURCE_COMMENTS]:
- Migration comments: lock level, rewrite behavior, backfill shape, validation phase, privilege window, rollout gate, irreversibility, rollback boundary, extension gate, and proof query.
- Function-body comments: dynamic SQL, exception conversion, search-path hardening, lock ordering, cursor ownership, transaction behavior, and planner assumptions.
- RLS comments: table lookup, current-setting behavior, bypass role, owner behavior, race, leakproof boundary, and covert-channel reasoning that the policy expression alone cannot carry.

[CATALOG_EXTRACTION]:
- Durable generated references extract object comments from `pg_description` or `pg_shdescription`.
- Use `col_description`, `obj_description(oid, catalog)`, and `shobj_description` for catalog routes.
- `psql` describe output is a human smoke route; generated dictionaries must include catalog facts beside comments instead of copying migration prose.
- `sqlfluff --dialect postgres` is formatting and linting proof only; it does not prove semantic documentation.

```sql conceptual
-- MIGRATION: lock=SHARE UPDATE EXCLUSIVE; rewrite=no; backfill=bounded by shard key;
-- validate=constraint proof query; rollback=forward-only after validation starts.
COMMENT ON TABLE app.account_event IS
'Append-only account event ledger; tenant isolation is enforced by account_event_tenant_rls.';

COMMENT ON POLICY account_event_tenant_rls ON app.account_event IS
'Restricts visible and mutable rows to the current tenant setting; fail-closed when unset.';
```

[REJECT]:
- Secrets, credentials, privileged assumptions, exploit details, credential routes, tenant IDs, or sensitive operational data in `COMMENT ON`; PostgreSQL comments are broadly visible through describe commands and description functions.
- Hand-maintained data dictionaries that duplicate catalog comments, source comments for durable schema meaning, blanket comments like `user id`, `obj_description(oid)` without catalog identity, and RLS prose without policy comments plus verification route.

## [14][LIFECYCLE_TAGS]

Lifecycle tags preserve only external support contracts. Use `@deprecated`, `[Obsolete]`, OpenAPI `deprecated`, PEP 702-style deprecation, TSDoc release tags, and equivalent generator-visible markers only when an external caller, package, generated reference, support matrix, or compatibility policy needs a warning plus migration route.

Lifecycle tag state uses this closed vocabulary:

[STATE_CLASSES]:
- `supported`: current public contract; no lifecycle warning.
- `preview`: external support contract is intentionally provisional and consumed by generated reference or support policy.
- `deprecated`: supported with warning, replacement path, behavior delta, and migration condition.
- `removed`: public contract no longer exists; route any historical fact to support or migration documentation, not source comments.
- `internal`: excluded from public generated reference; not a preservation device for greenfield stale surfaces.

Every lifecycle tag names:
- replacement path.
- behavioral delta.
- removal or migration condition.
- generated-reference or support route that consumes the signal.
- review trigger for removal or route update.

In greenfield internal code, delete or replace stale surfaces instead of preserving them with lifecycle tags. A deprecated tag without replacement guidance is stale-source preservation, not documentation.

## [15][CROSS_REFERENCES]

Resolve every code reference through the toolchain that carries it, or omit the reference.

- C# references use `cref` where compiler documentation validation can check them.
- TypeScript references use TSDoc `{@link ...}` and may pair with `@see`; `@see SomeSymbol` alone is not a link.
- Python references use the configured documentation generator's resolvable syntax when generated Python API docs exist.
- Bash references use literal command, variable, function, and ShellCheck code spans; there is no generated-reference profile by default.
- PostgreSQL references use schema-qualified object names in `COMMENT ON` and generated catalog routes through `psql` describe output or description functions.

Reserve inline comments for the reason a non-obvious choice exists:

```csharp conceptual
// Host catalog lists members lazily, so resolve eagerly here to surface a missing symbol at registration rather than at first call.
RegisterEager(symbol);
```

Accepted: document caller-visible semantics that the declaration cannot express.
Rejected: document every parameter, return type, command, column, or branch because it is public.
Reason: public visibility creates a review question, not an automatic comment requirement.

## [16][ANTI_PATTERNS]

Reject these cross-language shapes:
- Type-restating parameter: a `<param>`, `@param`, `Args:`, Bash function header, or SQL comment entry that echoes the declared type. Replace it with unit, range, origin, obligation, catalog meaning, or delete it.
- Throw tag for typed rail: an `<exception>`, `@throws`, or `Raises:` entry for a failure the surface returns as data. Move the semantics to the result, remarks, or rail section.
- Carrier echo: a `<returns>`, `@returns`, or `Returns:` entry that restates `Fin<T>`, `Effect<A, E, R>`, `Result[T, E]`, `Promise<T>`, SQL return type, or another carrier. Replace it with success, effect, or failure semantics, or remove it.
- Hidden side effect or cancellation: a comment that describes only the returned value while the surface mutates state, writes artifacts, starts work, observes time or IO, allocates or disposes resources, locks rows, changes catalog state, or requires cancellation handling. Add the observable obligation or route the detail to the controlling API, runbook, how-to, or schema reference.
- Name-echo summary: a summary that paraphrases the symbol, command, column, or policy name. Rewrite it to the surface-kind lead shape.
- Profile label leakage: a manual-only profile label emitted into a source comment without a toolchain-local tag. Remove the label and keep the semantics.
- Line-narrating inline comment: an inline comment that restates the next statement. Delete it.
- Generated mini catalog: a source file or docs leaf that hand-maintains generated public surface, command lists, or SQL dictionaries. Generate or route it.
- Internal lifecycle preservation: a lifecycle marker on greenfield internal surfaces that should be deleted or replaced.

## [17][BOUNDARIES]

[REFERENCE_ROUTES]:
- [api.md](api.md) carries generated and contract-backed API reference, including generated mirrors of source comments.
- [reference.md](reference.md) carries curated lookup facts that live outside source.
- [readme.md](readme.md) carries scope-local entry maps that point readers to public surfaces without cataloging every symbol.
- [architecture.md](../explanation/architecture.md) carries current route blocks, invariants, and codemaps; code comments do not carry folder architecture.
- [support-matrix.md](support-matrix.md), [runbook.md](../task/runbook.md), [how-to.md](../task/how-to.md), and [tutorial.md](../learning/tutorial.md) carry lifecycle status, operational response, task procedures, and learning paths.

[SHARED_ROUTES]:
- [style-guide.md](../style-guide.md) carries prose mechanics inside comments.
- [proof.md](../proof.md) carries proof that source comments match source behavior, generated reference output, and catalog output; source comments carry no proof details unless a language-specific generator consumes them.
- Source-map pages such as external-library, system API, and testing-tool references may link generated reference or public-symbol behavior, but they must not become source-comment standards or folder-level mini API catalogs.
- [README.md](../README.md) routes document-type, placement, lifecycle, and stale-document questions.

## [18][VALIDATION]

Use this verification checklist by group:

[STRUCTURE_ROUTE]:
- [ ] The standard keeps the comment decision, required structure, section cardinality, adjacent checks, maintenance triggers, and stale-prevention rules before language mechanics.
- [ ] Route-away rules are visible before generated handoffs and language capsules.
- [ ] Generated-reference handoffs omit absent optional fields and keep relation fields before local proof fields.
- [ ] Lifecycle-tag states come from the closed vocabulary and serve external support contracts only.

[SYMBOL_CONTRACT]:
- [ ] One public-surface profile is chosen during review, without leaking review labels into source comments.
- [ ] The comment carries the required semantic fields for that profile.
- [ ] No comment restates declared type, return type, nullability, arity, column name, shell command syntax, SQL syntax, or schema shape.
- [ ] The lead sentence matches the surface kind and carries contract, not name echo.
- [ ] FP/ROP surfaces name success, every failure variant, runtime-context requirements, resource contracts, and terminal boundaries where caller-visible.
- [ ] Throwing surfaces list only actual thrown types with their causes.
- [ ] Bash script surfaces name stdout, stderr, exit status, traps, resources, state, and ShellCheck rationale where those affect callers.
- [ ] PostgreSQL catalog surfaces use `COMMENT ON` for durable schema meaning and SQL comments only for local rationale.

[ROUTES_GENERATION]:
- [ ] Public symbol behavior, failure carrier, source comment, generated anchor, README entrypoint, architecture invariant, script command, and catalog comment changes trigger the adjacent checks named in the authoring contract.
- [ ] C#, TypeScript, Python, Bash, and PostgreSQL comments use the syntax and tags their toolchains parse.
- [ ] C# reference-resolution language reflects Rasm's warning-as-error and documentation-generation settings.
- [ ] Cross-references resolve through the controlling toolchain or maintained catalog route.

[COMMENTS_BOUNDARY]:
- [ ] Inline comments state a reason, not narration.
- [ ] Folder-level public-surface catalogs route to README, generated API reference, architecture, generated catalog output, or reference leaves instead of leaking into source comments.
- [ ] No anti-pattern remains.

[DOCS_ONLY_PROOF]:
- [ ] `git diff --check -- docs/standards` passes.
- [ ] Local path and anchor validation runs when headings, links, or anchors change, or a proof gap is stated.
- [ ] A stale-term scan finds no old doctrine in this file.
- [ ] C#, TypeScript, Python, Bash, SQL, static, test, bridge, and generated-reference rails stay unrun unless executable source, configs, generated artifacts, or tooling change.
