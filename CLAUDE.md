# [CLAUDE_MANIFEST]

MUST READ: `libs/.planning/RULINGS.md` + `libs/.planning/ARCHITECTURE.md`

Rasm is in a long-term planning phase, working strictly within design/spec-sheets, not code files. List all files in `libs/.planning` for all guidance:
- All `libs/` spec docs are the rebuild surface: rebuilt ground-up each pass, freely and aggressively.
- Each `lib/<language>/` and `libs/<language>/<sub-folder>` carry a `.api/` folder; all work stacks external-lib capability from BOTH altitudes — REQUIRED.

## [01]-[REQUIRED]

- Work in `libs/<language>/` requires a FULL reading of `libs/<language>/.planning/RULINGS.md` + `libs/<language>/.planning/ARCHITECTURE.md`
- Work in `libs/<language>/` requires a FULL reading of ALL files in the root of `docs/stacks/<language>/` at start of turn (not repeated).
- Durable infra docs — README/ARCHITECTURE/RULINGS/IDEAS/TASKLOG, `.api` files, and spec sheets all follow approach in `libs/.planning/campaign-method.md`.
- `docs/laws/` — repo-wide maintenance-law corpus; durable lessons land ONLY at the end of session, only where refute-first proves no surface owns the fact.

[STANDARDS_ROUTING]: Use the route-owned standard for the file being edited; an HTML artifact routes durable to `docs/atlas/`, temp to `.claude/scratch`:

| [INDEX] | [FILE_TYPE]                | [ROUTE]                        | [LOCATION_TO_USE]              | [NAMING_SCHEMA]                |
| :-----: | :------------------------- | :----------------------------- | :----------------------------- | :----------------------------- |
|  [01]   | C# production (`.cs`)      | Docs: `docs/stacks/csharp`     | `libs/csharp`                  | `PascalCase`                   |
|  [02]   | Python (`.py`)             | Docs: `docs/stacks/python`     | `libs/python`                  | `snake_case`                   |
|  [03]   | TypeScript (`.ts`, `.tsx`) | Docs: `docs/stacks/typescript` | `libs/typescript`              | `camelCase`                    |
|  [04]   | Bash/sh (`.sh`, `.bash`)   | Skill: `coding-bash`           | [ANY]                          | `kebab-case`                   |
|  [05]   | SQL (`.sql`)               | Skill: `coding-pg`             | [ANY]                          | `snake_case`                   |
|  [06]   | Markdown (`.md`)           | Skill: `docgen`                | [ANY]                          | `kebab-case`                   |
|  [07]   | Mermaid                    | Skill: `mermaid-diagramming`   | Inside `.md` and `.html` pages | [N/A]                          |
|  [08]   | HTML (`.html`)             | Skill: `html-studio`           | `docs/atlas/`                  | `<kind>.<scope>[.<slug>].html` |

[TOOL_ROUTING]:
- [ALWAYS]: use `exa` MCP to start open-web search with neural discovery, the right page, repo, paper, or entity; REPLACES `WebFetch` entirely.
- [ALWAYS]: use `tavily-search` skill on known targets — extract or crawl a site, or run a cited multi-source report.
- [ALWAYS]: use `context7-search` skill when working on code/fences with external libraries, never guess on SDK/framework/API capabilities or implementations.
- [ALWAYS]: use `nuget` MCP to validate the existence of a package and newest version available.
- [ALWAYS]: use `claudeCodeDocs` MCP when working on Claude Code configs or harness questions; capabilities, memory, skills, hooks, plugins, settings.
- [ALWAYS]: use `uv run python -m tools.assay static` for static quality `.py`, `.ts/.tsx`, and `.cs` files (ruff/ty/mypy, tsc/biome, dotnet format/build).
- [ALWAYS]: use `tools.assay provision` for Forge service, Postgres-extension, and DuckDB/SQLite surface evidence before an availability claim lands.
- [ALWAYS]: read `tests/README.md` + `tests/RULINGS.md` before touching any testing surface; a language tree adds its own README + RULINGS pair.

## [02]-[IMPLEMENTATION_STANDARDS]

[CRITICAL]:
- [NEVER]: add comments that carry task, session, subagent, review-label, proof, history, or process narration.
- [NEVER]: use weak, unbounded, or erased types where the language can express the domain precisely.
- [NEVER]: use exception-style control flow in domain logic; use typed error rails and the required route's recovery patterns.
- [NEVER]: use imperative branching when a bounded vocabulary, dispatch table, generated switch, match, fold, or monadic rail can own the variation.
- [NEVER]: use mutable accumulation for domain transforms; use immutable folds, projections, collection combinators, or effect/resource pipelines.
- [NEVER]: extract code to new files to reduce LOC. Densify in place through polymorphism, folds, generated owners, and table-driven dispatch.
- [NEVER]: delete functionality to satisfy a density or LOC signal. Preserve capability through denser owners.

[IMPORTANT]:
- [ALWAYS]: ASSUME 10X THE COMPLEXITY AND DEMANDS ON EVERY SURFACE — a naive, simple, or surface-level solution is rejected on sight.
- [ALWAYS]: extend a class to the full concept it admits NOW — a 4-field shape for a 12+ concept widens in place, never proliferates objects.
- [ALWAYS]: rebuild code and planning docs GROUND-UP — tear existing patterns apart for surface density with zero functionality lost.
- [ALWAYS]: land new functionality as if designed in from the start, never as tacked-on flat-code spam.
- [ALWAYS]: co-locate domain logic with its owner instead of scattering it into generic support files.
- [ALWAYS]: drive logic with data, bounded vocabularies, discriminants, table rows, and reusable projections.
- [ALWAYS]: collapse related variants into one polymorphic surface before adding entrypoints.
- [ALWAYS]: collapse repeated mutation/status/count construction into one fact stream with slot/kind metadata when three or more buckets share construction.
- [ALWAYS]: keep typed algorithm receipts when fields carry route, status, sampling, solver, spectral, mesh, extraction, benchmark, or host evidence.
- [ALWAYS]: treat analyzer diagnostics as architecture pressure: fix true positives, refine false positives, and never use suppressions.
- [ALWAYS]: treat planned future consumers as real design pressure. Zero current consumers never reduces the capability bar.
- [ALWAYS]: create code as polymorphic, agnostic, and universal by default, ALWAYS PARAMETERIZE INPUTS/OUTPUTS + INGRESS/EGRESS.
- [ALWAYS]: maintain semantic consistency in naming patterns of files, code functionality, types, classes, and functions.
- [ALWAYS]: use one canonical semantic name per bounded concept; arity, filters, provider, and modality live in request shape, case, or policy row.
- [ALWAYS]: extend the canonical owner before adding rails, public surfaces, wrappers, commands, flags, provider selectors, schemas, models, helpers, or files.
- [ALWAYS]: keep boundary mapping at the edge; internal code uses canonical names and shapes.
- [NEVER]: preserve stale APIs, wrappers, aliases, or old-baseline caveats when a root-up collapse improves the system.
- [NEVER]: create `Get`/`GetMany`/`GetBy<Key>`/`List`/`Search` operation families; one polymorphic operation discriminates on input shape.

## [03]-[DEPENDENCY_POLICY]

[IMPORTANT] - External libraries, manifests, and host APIs are implementation surfaces:
- [ALWAYS]: home substrate API catalogues at `libs/<language>/.api/`; package `.api/` folders carry domain catalogues and overlays.
- [ALWAYS]: treat admitted packages and ecosystem libraries as first class; mine their full capability before any local kernel or hand-roll.
- [ALWAYS]: internalize external capability into canonical local owners organized by domain, axis, row, case, receipt, or rail.
- [ALWAYS]: keep C# MSBuild/NuGet manifests label-grouped by owner, cluster-sorted, with one-line maintenance comments at most.
- [ALWAYS]: align the package touch-point set both ways: central manager row, project manifest, branch/folder README registries, owning `.api` tier.
- [ALWAYS]: repair an orphaned touch-point member at its owner, never by removal.
- [ALWAYS]: centralize package, version, and tool ownership in one owning manifest — no per-package `pyproject.toml`, `package.json`, or `*.props`.
- [ALWAYS]: assume the newest stable release; pin only while incompatible and drop the pin when compatibility lands.
- [ALWAYS]: keep root `pyproject.toml` dependencies as lean unpinned names; bounds or `python_version` markers land only on resolver evidence.
- [NEVER]: mint a folder-tier `.api` file duplicating or redirecting to a substrate catalogue; a folder composing a substrate package REGISTERS it.
- [NEVER]: create thin wrappers that rename, forward, or partially expose external APIs without adding domain value.
- [NEVER]: encode versions, pin/floor rationale, provider caveats, or command catalogs outside the owning manifest, charter, README, or tool owner.

## [04]-[FILE_ORGANIZATION]

Section separators: language comment marker + space + `---` + bracketed UPPERCASE snake label with no internal spaces + dash fill.

```typescript conceptual
// --- [TYPES] ---------------------------------------------------------------------------

// --- [SUBSECTION]
```

```python conceptual
# --- [CONSTANTS] ------------------------------------------------------------------------

# --- [SUBSECTION]
```

```csharp conceptual
// --- [SERVICES] ------------------------------------------------------------------------

// --- [SUBSECTION]
```

Canonical order, omitting unused sections: `TYPES` -> `CONSTANTS` -> `MODELS` -> `ERRORS` -> `SERVICES` -> `OPERATIONS` -> `COMPOSITION` -> `EXPORTS`.

- `[TYPES]`: type aliases, inferred types, protocols/interfaces, enums, discriminated unions, generated algebraic owners, value-object declarations.
- `[CONSTANTS]`: dependency-free immutable anchors, caps, suffixes, primitive policies, schedules, and static literals.
- `[MODELS]`: runtime schemas, records/classes, value objects, DTOs, table/domain models, receipts, result carriers.
- `[ERRORS]`: typed error rails, tagged failures, domain failure policies.
- `[SERVICES]`: service contracts, dependency surfaces, application/service classes.
- `[OPERATIONS]`: pure transforms, effect/result pipelines, algorithms, repository operations.
- `[COMPOSITION]`: layers, decorators, dependency wiring, middleware, runtime composition roots.
- `[EXPORTS]`: named exports, `__all__`, or language-equivalent public surface declarations.

[IMPORTANT]:
- [ALWAYS]: order as `section` -> `owner block` -> `runtime/declaration dependency` -> `semantic rank` -> `kind` -> `smaller-to-larger` -> `alphabetical`.
- [ALWAYS]: order concept discovery order from stable declarations to composition: vocabulary, constants, models, errors, services, operations, wiring, exports.
- [ALWAYS]: use nested algorithm subsection labels inside large kernels to identify a real operation family, such as `[VECTOR_HEAT]` or `[NORMAL_ESTIMATION]`.
- [ALWAYS]: keep internal cache keys, memo tables, mutable registries, and algorithm state records with the operation/kernel/runtime owner that mutates them.
- [ALWAYS]: treat logger handles, provider handles, and dependency-backed runtime capabilities as `[SERVICES]`, not immutable anchors.
- [ALWAYS]: co-locate tightly coupled symbols when strict section order obscures ownership or violates language/runtime constraints.
- [ALWAYS]: use alphabetical order only for equivalent declarations with the same owner, kind, dependency level, and semantic rank.
- [ALWAYS]: treat one generated type, smart enum, value object, schema/model/wire family, kernel, dispatcher, or query family as an owner block.
- [ALWAYS]: treat one registry, catalog, table, or composition root the same; sort inside the owner, never flattened into top-level sections.
- [ALWAYS]: a declaration follows what it imports, inspects, derives from, registers, decodes, wraps, initializes, traps, migrates, or composes.
- [ALWAYS]: apply smaller-to-larger only after ownership and dependency: anchors before policies, axes before models, leaf ops before orchestration.
- [ALWAYS]: treat kind as an owner-local tiebreaker, not a new section — it ranks only among peers equal in ownership, dependency, and semantic rank.
- [ALWAYS]: order same-owner peers public, then internal, then private — unless static construction, generated semantics, or read-before-use wins.
- [ALWAYS]: hold owner-defined domain order: severity, lifecycle, routing, key, protocol, generated-case, table-row, migration-step, public API.
- [ALWAYS]: insert a domain extension right after its closest core section; a precise label is earned by real ownership.
- [ALWAYS]: extension vocabulary: `[TABLES]`, `[BOUNDARIES]`, `[REPOSITORIES]`, `[GROUPS]`, `[MIDDLEWARE]`, `[INDEXES]`, `[POLICIES]`, `[ENTRY]`.
- [NEVER]: rename recurring categories per file; use canonical labels unless a domain extension is materially clearer.
- [NEVER]: use category-drift labels: `SCHEMA` `FUNCTIONS` `LAYERS` `IMPORTS` `INTERFACES` `ENUMS` `DTO` `QUERIES` `HELPERS` `UTILS` `COMMON` `MISC`.
- [NEVER]: split source-generated owners, delegate-backed enum behavior, validation partials, or operation-local state for mechanical section order.
- [NEVER]: split resource/disposal boundaries, dispatch tables, SQL invariants, or migration units to satisfy section order.
- [NEVER]: seat codecs, decoders, registries, lookup tables, generated maps, or dispatch rows in `[CONSTANTS]` ahead of what they depend on.
- [NEVER]: seat callable row catalogs, memo tables, or DDL-dependent objects in `[CONSTANTS]`; home each in its owning later section or extension.

Language overlays refine the canonical order by runtime semantics:
- [CSHARP]: `[Union]`, `[SmartEnum]`, `[ValueObject]`, generated case families, static entries, delegate/validation partials, factories, and projections stay inside the declaring owner block. Generated-case and smart-enum order is semantic — one case or static entry per line unless a generator or runtime contract groups them — and static construction order is semantic when later fields derive from earlier ones. Static kernels, projectors, acceptors, and extension folds are `[OPERATIONS]` unless they own a real dependency or service boundary. In-section kind order: attributes/delegates/marker types, enums/smart enums, readonly structs/records/value objects, records/classes/services, owner-local private types. In-owner order: generated/static dependency entries, fields/state, constructors/factories, properties, public operations, boundary adapters, internal operations, private kernels.
- [PYTHON]: imports, `TYPE_CHECKING`, and import-time gates precede ordinary sections. Runtime decoders, encoders, registries, and tables follow the models/functions they inspect — module-level assignments execute immediately and runtime annotation consumers (`msgspec`, `beartype`) resolve real objects. `Annotated` validators may sit in `[BOUNDARIES]` between immutable constants and the dependent aliases that must reference the real validator object.
- [TYPESCRIPT]: side-effect/value imports keep runtime order; `import type`/`export type` stay explicit. Runtime schemas/classes are `[MODELS]`, `Effect.Service` owners `[SERVICES]`, `Layer`/runtime wiring `[COMPOSITION]`, and catalog or registry rows referencing functions/classes stay after their owners.
- [BASH]: shebang, ShellCheck directives, `set`/`shopt`, and environment/path gates are `[RUNTIME_PRELUDE]`; `readonly` values `[CONSTANTS]`; `declare -Ar` maps `[TABLES]`; traps, dispatch, source guards, and `_main` late `[COMPOSITION]` or `[ENTRY]`.
- [PO_SQL]: extensions, schemas, and search-path guards are `[RUNTIME_PRELUDE]`; domains and types `[TYPES]`; tables, constraints, generated columns, and partitions `[MODELS]`; functions split by service boundary or query operation; indexes, triggers, row-level security, and policies `[COMPOSITION]`; grants and comments late `[EXPORTS]`.
