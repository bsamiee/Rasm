# [CLAUDE_MANIFEST]

Operate as a senior developer in a bleeding-edge monorepo; use the newest viable current versions of languages, libraries, plugins, extensions, and add-ons after verifying tooling behavior from current docs or local output.

[IMPORTANT]:
- [ALWAYS] Treat monorepo code as polymorphic, agnostic, and universal by default.
- [ALWAYS] Identify canonical object shapes, field names, and semantics that scale across packages and apps.
- [ALWAYS] Reuse established naming patterns; prefer universal names (`countBy`) over narrow variants (`countByIp`, `countByX`).
- [ALWAYS] If an external contract requires a different name, isolate mapping at boundary adapters and keep canonical names internally.
- [NEVER] Rename a canonical concept across schemas/models/classes, parameters, and return keys within the same bounded context.

## [1]-[REQUIRED_STANDARDS]

If reviewing, refining, editing, creating, or modifying X file type, use skill Y (required):

| [INDEX] | [FILE_TYPE]                         | [REQUIRED_SKILL]     |
| :-----: | ----------------------------------- | -------------------- |
|   [1]   | TypeScript (`.ts`, `.tsx`)          | `coding-ts`          |
|   [2]   | C# production (`.cs`)               | `docs/stacks/csharp` |
|   [3]   | C# tests (`.spec.cs`)               | `testing-cs`         |
|   [4]   | Runtime scenarios (`Scenarios/*.cs`) | `testing-cs`         |
|   [5]   | Python (`.py`)                      | `coding-python`      |
|   [6]   | Bash/sh (`.sh`, `.bash`)            | `coding-bash`        |
|   [7]   | SQL (`.sql`)                        | `coding-pg`          |

- Treat `docs/stacks/csharp` as the only source of truth for csharp coding standards, and as a skill. All code MUST adhere to the doctrine stated in `docs/stacks/csharp/README.md`. All code is built on the standards of: `docs/stacks/csharp/language.md` + `docs/stacks/csharp/shapes.md` + `docs/stacks/csharp/surfaces-and-dispatch.md` + `docs/stacks/csharp/rails-and-effects.md` + `docs/stacks/csharp/boundaries.md` + `docs/stacks/csharp/algorithms.md` + `docs/stacks/csharp/system-apis.md`
- Use `docs/stacks/csharp/domain` for specialized `.cs` file functionality and concers, file routing: `docs/stacks/csharp/domain/README.md`

## [2]-[BEHAVIOR]

[IMPORTANT]:
- [ALWAYS] Use current technical material when conducting research; changing material [MUST] be within the last 3-4 months from current date unless stable official docs are the only primary route for a settled platform rule.
- [ALWAYS] Tools over internal knowledge: read files, search codebase, verify assumptions.
- [ALWAYS] Parallelize aggressively: run multiple searches, read several files, call independent tools concurrently.
- [ALWAYS] Use bounded sub-agents for independent exploration, research, verification, and disjoint implementation when the user asks for sub-agents or parallel agent work; merge findings through current code and tool output, and keep fixed agent counts, transcript order, and critique labels out of durable policy.
- [ALWAYS] Reference symbols by name; avoid inline code blocks for context already shown.

[CRITICAL]:
- [NEVER] Use emojis; use `[X]` style markers with concise UPPERCASE formatting.

### [2.1]-[SHELL_AND_WORKFLOW_EXECUTION]

[IMPORTANT]:
- [ALWAYS] Invoke real executables on `PATH`; use `zsh -ic` only when intentionally testing interactive zsh configuration.
- [ALWAYS] Run Bash-only snippets using `mapfile`, `readarray`, `shopt`, `BASH_*`, arrays, or Bash 5.3 features through `bash -lc`, a Bash heredoc, or an executable with a Bash shebang.
- [ALWAYS] Treat Claude workflow globals such as `args` as Claude workflow-runtime state, separate from Nix, zsh, aliases, and shell `PATH`; verify `args` before using it for phase selection in saved or scriptPath-launched workflows.

## [3]-[DEPENDENCY_POLICY]

[IMPORTANT]: **External-Lib-First**: approved dependencies are primary implementation surfaces.
- [ALWAYS] Treat dependencies declared in `pyproject.toml`, `pnpm-workspace.yaml`, `Directory.Packages.props`, and equivalent manifests as first-class libraries.
- [ALWAYS] Integrate approved external libraries directly; use native APIs end-to-end.
- [ALWAYS] Prefer ecosystem libraries that already own the domain concern over local reinvention.
- [ALWAYS] Internalize the full admitted package capability into the canonical local owner before exposing commands, wrappers, facades, flags, provider selectors, or provider-branded public surfaces.
- [NEVER] Hand-roll functionality already provided by approved dependencies.
- [NEVER] Prefer stdlib alternatives when approved external libraries already cover the requirement.
- [NEVER] Create thin wrappers that rename or forward external APIs without adding domain value.

[IMPORTANT]: **.NET-Central-Package-Management**: C# package versions live in `Directory.Packages.props`; project files may declare usage but never versions.
- [ALWAYS] Check `docs/stacks/csharp/system-apis.md` before adding a `System.*` package, global using, or BCL replacement.
- [ALWAYS] Keep RhinoWIP/GH2/Eto/System.Drawing host assemblies resolved through `Directory.Build.props` app-bundle references; if SDK compilation needs a NuGet reference surface, add it only as a conditioned central compile package.

## [4]-[UNIVERSAL_CONSTRAINTS]

[CRITICAL]:
- [NEVER] Use weak, unbounded, or erased types where the language can express the domain precisely.
- [NEVER] Use imperative branching in domain logic; use the language skill's expression, match, dispatch-table, or monadic ROP patterns.
- [NEVER] Use mutable accumulation for domain transforms; use immutable folds, projections, collection combinators, or effect/resource pipelines.
- [NEVER] Use exception-style control flow in domain logic; use typed error rails and the required skill's recovery patterns.
- [NEVER] Proliferate schemas, structs, models, branded types, records, classes, or aliases for the same concept.
- [NEVER] Create helper/utility files or functions (`helpers.*`, `*Helper`, `*Util`, `common.*`) for single-caller or thin indirection.
- [NEVER] Create wrappers, unnecessary intermediate bindings, single-use aliases, or constant spam.
- [NEVER] Split one concern across parallel names, objects, services, or error rails.
- [NEVER] Add comments describing "what"; reserve comments for "why", boundary exceptions, and non-obvious invariants. Public XML docs, TSDoc, Google docstrings, Bash contract comments, and SQL `COMMENT ON` state caller-visible semantics only; comments and docstrings never carry task, session, sub-agent, review-label, or process narration.
- [NEVER] Add new code before searching for existing canonical shapes, vocabularies, services, and policies to extend.
- [NEVER] Extract code to new files to reduce LOC. Densify in place through polymorphism, fold algebras, table-driven dispatch.
- [NEVER] Add shims, adapters, legacy aliases, `[Obsolete]` wrappers, or backwards-compat surfaces. Break APIs freely when collapse improves the system.
- [NEVER] Couple custom analyzer rules to project namespaces, paths, or one-off symbols. Rules describe semantic shapes and include positive and negative tests for valid compact code.
- [NEVER] Treat ~350 LOC or any specific byte-count as a refactor trigger. The trigger is concept density: parallel types ≥3, sibling factories ≥3, repeated switch arms ≥3, single-call helpers ≥3.
- [NEVER] Delete functionality to satisfy a "density" or "LOC" signal. Functionality is preserved in capability through denser polymorphic surfaces, not removed.
- [NEVER] Replace algorithm-specific typed receipts with generic `IReceipt`, ledger, or reported-value abstractions.

[IMPORTANT]:
- [ALWAYS] Collapse related variants into one polymorphic surface before adding new entrypoints.
- [ALWAYS] Drive logic algorithmically with data, bounded vocabularies, discriminants, and reusable projections.
- [ALWAYS] Keep boundary translation at the boundary; internal code uses canonical names and shapes.
- [ALWAYS] Co-locate domain logic with the owning module instead of scattering it into generic support files.
- [ALWAYS] Collapse operational mutation receipts into one fact stream with slot/kind metadata and fold-derived projections when 3+ mutation buckets or repeated slot families share construction, count, or status semantics.
- [ALWAYS] Keep typed algorithm receipts when fields carry route, status, sampling, solver, spectral, mesh, or extraction evidence.
- [ALWAYS] Treat CSP analyzer diagnostics as hypotheses: fix production code for true positives; refine the analyzer for false positives or fixes that add ceremony without improving correctness, capability, or maintainability.
- [ALWAYS] Consider `tools/cs-analyzer` when a repeated C# optimization pattern is proven by diffs. Add rules only after the best fix reduces LOC or surface while preserving semantics.

## [5]-[OUTPUT]

[IMPORTANT]:
- [ALWAYS] Use `backticks` for file paths, symbols, and CLI commands.
- [ALWAYS] Use Markdown: headings for structure, bullets for lists, tables for comparisons.
- [ALWAYS] Keep responses actionable; lead with what changed, not what you will do.

### [5.1]-[OWNER_ROUTING]

[IMPORTANT]:
- [ALWAYS] Dependency graph facts live in manifests, package-manager configuration, lockfiles, project files, and the tool owner that consumes them.
- [ALWAYS] Quality routes are selected by the owning language/tool surface for the changed files. Root policy owns intent, not command catalogs.
- [ALWAYS] Static analysis, tests, runtime scenarios, metadata lookup, formatting, restore, and generated-contract checks stay orthogonal. Do not conflate one rail with another or hardcode one suite as universal.
- [ALWAYS] For docs-only, catalogue-only, read-only, declaration-order, move-only, source-comment-only, docstring-only, XML-doc-only, and TSDoc-only work, use text, path, table, link, owner, and preservation checks unless the user requests an executable quality rail.
- [NEVER] Add package versions, tool commands, hardcoded project targets, or suite paths to root policy when a manifest, README, repo tool, or language owner carries the exact command.

### [5.3]-[PLAN_DISCIPLINE]

[IMPORTANT]:
- [ALWAYS] Plans are decision-complete blueprints, not narratives. Code change must be explicit, real, and routed to the owning quality policy.
- [ALWAYS] Structure: Context, critical files, implementation approach, acceptance signals, and explicit assumptions only when they change execution.
- [NEVER] Include "Phase 1...Phase N" workflow narration, alternatives considered, checklist tails, command catalogs, or boilerplate closure. The plan is the blueprint, not a journal.

### [5.4]-[SURFACE_PREFERENCE]

[IMPORTANT]:
- [ALWAYS] Prefer FEWER deep, complex surfaces over MANY loose, simple ones. A single 400-LOC type that owns a full polymorphic concern is better than four 100-LOC types that fragment it.
- [ALWAYS] The unit of design is the polymorphic dispatch surface, not the file.

## [6]-[FILE_ORGANIZATION]

[IMPORTANT] **Section separators**: language comment marker + space + `---` + bracketed UPPERCASE snake label with no internal spaces + dash fill to the established language width.

```typescript
// --- [TYPES] ---------------------------------------------------------------------------
```

```python
# --- [CONSTANTS] ------------------------------------------------------------------------
```

```csharp
// --- [SERVICES] ------------------------------------------------------------------------
```

**Canonical order** (omit unused): `TYPES` -> `CONSTANTS` -> `MODELS` -> `ERRORS` -> `SERVICES` -> `OPERATIONS` -> `COMPOSITION` -> `EXPORTS` 

`[RUNTIME_PRELUDE]` may precede the canonical order only for imports, shebangs, strict modes, session setup, and load gates.

**Core Sections**:
- `[TYPES]`: type aliases, inferred types, protocols/interfaces, enums, discriminated unions, generated algebraic owners, value-object declarations.
- `[CONSTANTS]`: dependency-free immutable anchors, caps, suffixes, primitive policies, schedules, and static literals.
- `[MODELS]`: runtime schemas, records/classes, value objects, DTOs, table/domain models, receipts, result carriers.
- `[ERRORS]`: typed error rails, tagged failures, domain failure policies.
- `[SERVICES]`: service contracts, dependency surfaces, application/service classes.
- `[OPERATIONS]`: pure transforms, effect/result pipelines, algorithms, repository operations.
- `[COMPOSITION]`: layers, decorators, dependency wiring, middleware, runtime composition roots.
- `[EXPORTS]`: named exports, `__all__`, or language-equivalent public surface declarations.

[IMPORTANT]:
- [ALWAYS] Apply ordering as `section` -> `owner block` -> `runtime/declaration dependency` -> `semantic rank` -> `kind` -> `smaller-to-larger` -> `alphabetical`.
- [ALWAYS] Prefer concept discovery order from stable declarations to composition: vocabulary, constants, models, failures, services, operations, wiring, exports.
- [ALWAYS] Treat one generated type, smart enum, value object, schema/model family, wire model family, kernel, registry, catalog, table, dispatcher, query family, or composition root as an owner block; sort inside the owner instead of flattening its members into unrelated top-level sections.
- [ALWAYS] Keep dependency clusters intact when a declaration must follow the symbol it imports, inspects, derives from, registers, decodes, wraps, statically initializes, traps, migrates, or composes.
- [ALWAYS] Use smaller-to-larger only after ownership and dependency order are satisfied: one-line anchors before multi-line policies, simple axes before rich models, leaf operations before orchestration.
- [ALWAYS] Use alphabetical order only for equivalent declarations with the same owner, kind, dependency level, and semantic rank.
- [ALWAYS] Treat kind as an owner-local tiebreaker, not a new section: type/member family precedes accessibility, size, and alphabetical order only when ownership, dependency, and semantic rank are equivalent.
- [ALWAYS] For equivalent same-owner members, prefer public contract before internal extension before private implementation unless static construction, generated semantics, or read-before-use dependency requires another order.
- [ALWAYS] Keep semantically ordered sequences in domain order, not alphabetical order: severity, lifecycle, routing, key, protocol, generated-case, table-row, migration-step, and public API order are load-bearing when the owner defines them.
- [ALWAYS] Co-locate tightly coupled symbols when strict section order would obscure ownership or violate language/runtime constraints.
- [ALWAYS] Insert domain extensions immediately after the closest core section, using precise labels only when they name real ownership: `[TABLES]`, `[BOUNDARIES]`, `[REPOSITORIES]`, `[GROUPS]`, `[MIDDLEWARE]`, `[INDEXES]`, `[POLICIES]`, or `[ENTRY]`.
- [ALWAYS] Use nested algorithm subsection labels inside large kernels only when they identify a real operation family, such as `[VECTOR_HEAT]` or `[NORMAL_ESTIMATION]`.
- [ALWAYS] Keep internal cache keys, memo tables, mutable registries, and algorithm state records with the operation, kernel, or runtime owner that reads and mutates them.
- [ALWAYS] Treat logger handles, provider handles, and dependency-backed runtime capabilities as `[SERVICES]`, not immutable anchors.
- [NEVER] Put derived codecs, decoders, registries, lookup tables, generated maps, dispatch rows, callable row catalogs, mutable memo tables, or DDL-dependent objects in top-level `[CONSTANTS]` when they depend on later models, functions, owners, runtime state, or migration state; place them in the owning later section or a precise extension such as `[TABLES]` or `[COMPOSITION]`.
- [NEVER] Split source-generated owners, delegate-backed enum behavior, validation partials, private operation-local state, resource/disposal boundaries, dispatch tables, SQL invariants, or migration units to satisfy mechanical section order.
- [NEVER] Rename recurring categories per file; use canonical labels unless a domain extension is materially clearer.
- [NEVER] Use alias or drift labels that merely rename core categories or hide complexity: `SCHEMA`, `FUNCTIONS`, `LAYERS`, `IMPORTS`, `INTERFACES`, `ENUMS`, `DTO`, `QUERIES`, `HELPERS`, `UTILS`, `COMMON`, `MISC`.

**Language Overlays**:
- C#: `[Union]`, `[SmartEnum]`, `[ValueObject]`, generated case families, static entries, delegate partials, validation partials, factories, and projections stay inside the declaring owner block. Preserve generated-case and smart-enum semantic order, with one generated case or static entry per physical declaration line unless a generator or runtime contract requires grouping. Static construction order inside a type is semantic when later fields derive from earlier fields. Static kernels, projectors, acceptors, and extension folds are `[OPERATIONS]` unless they own an actual dependency or service boundary. Inside a section, prefer attributes/delegates/marker types, enums/smart enums, readonly structs/records/value objects, records/classes/services, then owner-local private types when all earlier ordering constraints are equal. Inside a C# owner block, prefer generated/static dependency entries, fields/state, constructors/factories, properties, public operations, explicit boundary adapters, internal operations, then private kernels/implementation details.
- Python: imports, `TYPE_CHECKING`, and import-time gates precede ordinary sections. Runtime decoders, encoders, registries, and tables follow the models/functions they inspect because module-level assignments execute immediately and runtime annotation consumers such as `msgspec` and `beartype` resolve real objects. `Annotated` validator functions may use `[BOUNDARIES]` between immutable constants and dependent aliases when the aliases must reference the real validator object.
- TypeScript: side-effect/value imports preserve runtime order, and `import type`/`export type` stay explicit. Runtime schemas/classes are `[MODELS]`, `Effect.Service` owners are `[SERVICES]`, `Layer`/runtime wiring is `[COMPOSITION]`, and catalog or registry rows that reference functions/classes stay after their referenced owners.
- Bash: shebang, ShellCheck directives, `set`/`shopt`, and environment/path gates are `[RUNTIME_PRELUDE]`; `readonly` values are `[CONSTANTS]`; `declare -Ar` maps are `[TABLES]`; traps, dispatch, source guards, and `_main` are late `[COMPOSITION]` or `[ENTRY]`.
- PostgreSQL: extensions, schemas, and search-path guards are `[RUNTIME_PRELUDE]`; domains and types are `[TYPES]`; tables, constraints, generated columns, and partitions are `[MODELS]`; functions split by service boundary or query operation; indexes, triggers, row-level security, and policies are `[COMPOSITION]`; grants and comments are late `[EXPORTS]`.
- YAML/YML: manifests and configuration files are data surfaces, not sectioned source; do not add code-section dividers. Preserve sequence order, anchors, comments, duplicate-key constraints, schema-defined key order, and executable order. Mapping-key reorder is presentation-only unless the owning tool documents order-dependent behavior; otherwise prefer required identity/version fields before optional metadata, resources, executable units, outputs, and publication/export fields.
