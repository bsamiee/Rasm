# [CLAUDE_MANIFEST]

Operate as a senior developer in a bleeding-edge monorepo; use the newest viable current versions of languages, libraries, plugins, extensions, and add-ons after verifying tooling behavior from docs or local output.

[IMPORTANT]:
- [ALWAYS] Treat monorepo code as polymorphic, agnostic, and universal by default.
- [ALWAYS] Identify canonical object shapes, field names, and semantics that scale across packages and apps.
- [ALWAYS] Reuse established naming patterns; prefer universal names (`countBy`) over narrow variants (`countByIp`, `countByX`).
- [ALWAYS] If an external contract requires a different name, isolate mapping at boundary adapters and keep canonical names internally.
- [NEVER] Rename a canonical concept across schemas/models/classes, parameters, and return keys within the same bounded context.

## [1]-[REQUIRED_STANDARDS]

If reviewing, refining, editing, creating, or modifying X file type, use skill Y (required):

| [INDEX] | [FILE_TYPE]                         | [REQUIRED_SKILL]            |
| :-----: | ----------------------------------- | --------------------------- |
|   [1]   | TypeScript (`.ts`, `.tsx`)          | `coding-ts`                 |
|   [2]   | C# production (`.cs`)               | `coding-csharp`             |
|   [3]   | C# tests (`.spec.cs`)               | `testing-cs`                |
|   [4]   | RhinoCode scenarios (`.verify.csx`) | `testing-cs`                |
|   [5]   | Python (`.py`)                      | `coding-python`             |
|   [6]   | Bash/sh (`.sh`, `.bash`)            | `coding-bash`               |
|   [7]   | SQL (`.sql`)                        | `coding-pg`                 |

## [2]-[BEHAVIOR]

[IMPORTANT]:
- [ALWAYS] Use new sources when conducting research; freshness-sensitive sources [MUST] be within the last 3-4 months from current date unless stable official docs are the only primary source for a settled platform rule.
- [ALWAYS] Tools over internal knowledge: read files, search codebase, verify assumptions.
- [ALWAYS] Parallelize aggressively: run multiple searches, read several files, call independent tools concurrently.
- [ALWAYS] Use bounded sub-agents for independent exploration, research, verification, and disjoint implementation when the user asks for sub-agents or parallel agent work; merge findings through current source proof and keep fixed agent counts, transcript order, and critique labels out of durable policy.
- [ALWAYS] Reference symbols by name; avoid inline code blocks for context already shown.

[CRITICAL]:
- [NEVER] Use emojis; use `[X]` style markers with concise UPPERCASE formatting.

## [3]-[DEPENDENCY_POLICY]

[IMPORTANT]: **External-Lib-First**: approved dependencies are primary implementation surface.
- [ALWAYS] Treat dependencies declared in `pyproject.toml`, `pnpm-workspace.yaml`, `Directory.Packages.props`, and equivalent manifests as first-class libraries.
- [ALWAYS] Integrate approved external libraries directly; use native APIs end-to-end.
- [ALWAYS] Prefer ecosystem libraries that already own the domain concern over local reinvention.
- [ALWAYS] Internalize manifest-admitted package capability into the canonical local owner before exposing commands, wrappers, facades, flags, provider selectors, or provider-branded public surfaces.
- [NEVER] Hand-roll functionality already provided by approved dependencies.
- [NEVER] Prefer stdlib alternatives when approved external libraries already cover the requirement.
- [NEVER] Create thin wrappers that rename or forward external APIs without adding domain value.

[IMPORTANT]: **.NET-Central-Package-Management**: C# package versions live in `Directory.Packages.props`; project files may declare usage but never versions.
- [ALWAYS] Check `docs/stacks/csharp/platform` before adding a `System.*` package, global using, or BCL replacement.
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
- [NEVER] Replace algorithm-specific proof receipts with generic `IReceipt`, ledger, or reported-value abstractions.

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

### [5.1]-[DEPENDENCIES]

[IMPORTANT] TypeScript dependency workflow:

[IMPORTANT]:
1. [ALWAYS] **Check catalog**: `rg -n "my-dep" pnpm-workspace.yaml`.
2. [ALWAYS] **Add to catalog** (if missing): `my-dep: 1.2.3` (exact version).
3. [ALWAYS] **Reference**: `"dependencies": { "my-dep": "catalog:" }`.
4. [ALWAYS] **Install**: `pnpm install`.
5. [ALWAYS] **Validate**: `pnpm exec nx run-many -t typecheck`.

[IMPORTANT] C# dependency workflow:
1. [ALWAYS] **Check package truth**: `rg -n "<PackageId>" Directory.Packages.props Directory.Build.props **/*.csproj`.
2. [ALWAYS] **Add version centrally** only when a project, tool, host route, or accepted owner route admits the package.
3. [ALWAYS] **Keep project references versionless** under central package management.
4. [ALWAYS] **Validate graph**: use `uv run python -m tools.quality static plan <changed-manifest>` when routing is uncertain, then `uv run python -m tools.quality static full` for central package, solution, global runner, `.editorconfig`, or analyzer trigger changes.

### [5.2]-[QUALITY_GATES]

Three orthogonal rails: static analysis, unit tests, runtime verification. Each tool verb owns one rail; never conflate.

[CRITICAL]:
1. [NEVER] Run static, test, or bridge rails for source-comment-only, docstring-only, XML-doc-only, TSDoc-only, divider-only, declaration-order, or move-only organization work unless the user explicitly requests a quality rail or preservation proof fails.

[IMPORTANT]:
1. [ALWAYS] **Static fix** — `uv run python -m tools.quality static fix [paths...]`. Run after executable C# source changes, analyzer remediation, or user-requested cleanup when safe autofix is desired. Routes changed files or explicit paths to owning projects. Applies scoped `dotnet format whitespace`, `style`, and `analyzers` fixes. No build, no tests.
2. [ALWAYS] **Static build** — `uv run python -m tools.quality static build [paths...]`. Run after semantic or compilable source changes. Routes changed files or explicit paths to owning project closure. Runs restore + build + MSBuild analyzers for compile proof. No formatting, no tests.
3. [ALWAYS] **Static report** — `uv run python -m tools.quality static report [paths...]`. Runs the scoped `dotnet format` ladder as diagnostics only. Use before build when mutation is disallowed or autofix is not allowed.
4. [ALWAYS] **Full static** — `uv run python -m tools.quality static full`. Runs `Workspace.slnx` parity plus full-solution restore/build/analyzers. Required only when trigger files change (`.config/dotnet-tools.json`, `Directory.Build.props`, `Directory.Build.targets`, `Directory.Packages.props`, `Workspace.slnx`, `.editorconfig`, `global.json`, `tools/cs-analyzer/**`).
5. [ALWAYS] **Unit tests** — `uv run python -m tools.quality test run [<filter>]`. Runs .NET 10 MTP against the library tests target (`tests/csharp/libs/Rasm/Rasm.Tests.csproj` by default; override via `--target <csproj>` or use `--all`). Mutation is explicit via `--mutation changed|full`; default test runs are unit-only.
6. [ALWAYS] **Metadata/API lookup** — `uv run python -m tools.quality api doctor|resolve|query|show`. Use before relying on RhinoWIP, GH2, Eto, or central package APIs.
7. [ALWAYS] **Rhino runtime verification** — `uv run python -m tools.quality bridge verify <path-or-glob>`. Routes scenarios through the in-process bridge against running `RhinoWIP.app`. Outputs JSON evidence and PNG captures under `.artifacts/rhino/verify/`. See the `testing-cs` skill.
8. [ALWAYS] **Trust the analyzer**: 80+ CSP descriptors (`tools/cs-analyzer/Kernel/RuleCatalog.cs`) enforce coding-csharp standards. When CSP#### fires, fix the architecture; do not suppress.
9. [NEVER] Re-introduce a `test` mode into the static rail. Tests are a separate gate.

### [5.3]-[PLAN_DISCIPLINE]

[IMPORTANT]:
- [ALWAYS] Plans are documents, not narratives. Code change must be explicit, real, and validated to pass all quality checks per language tooling.
- [ALWAYS] Structure: Context (1 sentence on why), Critical files (paths + line numbers), Approach (3-5 bullets), Code Change (true to implementaiton code block).
- [NEVER] Include "Phase 1...Phase N" workflow narration, alternatives considered, or implementation prose. The plan is the blueprint, not a journal.

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
