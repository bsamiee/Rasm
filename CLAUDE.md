# [CLAUDE_MANIFEST]

MUST READ: `libs/.planning/RULINGS.md` + `libs/.planning/ARCHITECTURE.md`

Rasm is in a long-term planning phase, working strictly within design/spec-sheets, not code files. List all files in `libs/.planning` for all guidance:
- All spec sheet docs in `libs/` are meant to be aggressively rebuilt ground-up, every pass is an opportunity to make improvements, changes in direction, and adjustments prior to code implementation. NEVER hesitate to do considerable rebuilding of content within `libs/` while all content is still being planned.
- Each `lib/<language>/` has an `.api/` folder, each sub-folder within a language lib has one as well; all work MUST fully stack as much external lib capabilities from BOTH altitudes where possible. External libs reduce handrolling, they ALSO enable new functionality, capabilities, as well as extending existing code capabilities, use them as guidance for ideation on new code features.

## [01]-[REQUIRED]

- Work in `libs/<language>/` requires a FULL reading of `libs/<kanguage>/.planning/RULINGS.md` + `libs/<kanguage>/.planning/ARCHITECTURE.md`
- Work in `libs/<language>/` requires a FULL reading of ALL files in the root of `docs/stacks/<language>/`, for csharp, the `domain/` folder is case by case.
- Durable infra docs — README/ARCHITECTURE registries, IDEAS/TASKLOG card pools, RULINGS decision registries, `.api` catalogues, system pages — follow the maintenance law in `libs/.planning/campaign-method.md` (catalog-alignment, card-marker, research-section, and rulings disciplines live there).
- `docs/laws/` is the repo-wide maintenance-law corpus; a durable lesson lands ONLY at the end of a large-work session and only where refute-first proves no reviewer surface owns the fact.

[STANDARDS_ROUTING]: Use the route-owned standard for the file being edited:

| [INDEX] | [FILE_TYPE]                | [ROUTE]                        | [LOCATION_TO_USE]                              | [NAMING_SCHEMA]                |
| :-----: | :------------------------- | :----------------------------- | :--------------------------------------------- | :----------------------------- |
|  [01]   | C# production (`.cs`)      | Docs: `docs/stacks/csharp`     | `libs/csharp`                                  | `PascalCase`                   |
|  [02]   | Python (`.py`)             | Docs: `docs/stacks/python`     | `libs/python`                                  | `snake_case`                   |
|  [03]   | TypeScript (`.ts`, `.tsx`) | Docs: `docs/stacks/typescript` | `libs/typescript`                              | `camelCase`                    |
|  [04]   | Bash/sh (`.sh`, `.bash`)   | Skill: `coding-bash`           | [ANY]                                          | `kebab-case`                   |
|  [05]   | SQL (`.sql`)               | Skill: `coding-pg`             | [ANY]                                          | `snake_case`                   |
|  [06]   | Durable markdown (`.md`)   | Skill: `docgen`                | [ANY]                                          | `kebab-case`                   |
|  [07]   | Mermaid fences             | Skill: `mermaid-diagramming`   | Inside `.md` and `.html` pages                 | [N/A]                          |
|  [08]   | HTML artifacts (`.html`)   | Skill: `html-studio`           | Durable: `docs/atlas/` Temp: `.claude/scratch` | `<kind>.<scope>[.<slug>].html` |

[TOOL_ROUTING]:
- [ALWAYS] use `exa` MCP to start open-web search with neural discovery, the right page, repo, paper, or entity; REPLACES `WebFetch` entirely.
- [ALWAYS] use `tavily-search` skill on known targets — extract or crawl a site, or run a cited multi-source report.
- [ALWAYS] use `context7-search` skill when working on code/fences with external libraries, never guess on SDK/framework/API capabilities or implementations.
- [ALWAYS] use `nuget` MCP to validate the existence of a package and newest version available.
- [ALWAYS] use `claudeCodeDocs` MCP when working on Claude Code configs or harness questions; capabilities, memory, skills, hooks, plugins, settings.
- [ALWAYS] read `tests/README.md` before touching any testing surface.

## [02]-[IMPLEMENTATION_STANDARDS]

[CRITICAL]:
- [NEVER] add comments that carry task, session, subagent, review-label, proof, history, or process narration.
- [NEVER] use weak, unbounded, or erased types where the language can express the domain precisely.
- [NEVER] use exception-style control flow in domain logic; use typed error rails and the required route's recovery patterns.
- [NEVER] use imperative branching when a bounded vocabulary, dispatch table, generated switch, match, fold, or monadic rail can own the variation.
- [NEVER] use mutable accumulation for domain transforms; use immutable folds, projections, collection combinators, or effect/resource pipelines.
- [NEVER] extract code to new files to reduce LOC. Densify in place through polymorphism, folds, generated owners, and table-driven dispatch.
- [NEVER] delete functionality to satisfy a density or LOC signal. Preserve capability through denser owners.

[IMPORTANT]:
- [ALWAYS] ASSUME 10X THE COMPLEXITY/DEMANDS ON CODE, NEVER SETTLING FOR SIMPLE/NAIVE SOLUTIONS, NEVER TOLERATE SURFACE LEVEL FUNCTIONALITY. A class carrying 4 fields for a concept that admits 12+ is extended to the full concept in anticipation of all the needs NOW, not later; object proliferation is never the answer.
- [ALWAYS] aggressively rebuild code and planning docs GROUND/ROOT-UP, tear apart any existing patterns to achieve the optimized code surface density without losing functionality; new functionality is always made as if it was there from the start, never as tacked-on/flat-code spam.
- [ALWAYS] co-locate domain logic with its owner instead of scattering it into generic support files.
- [ALWAYS] drive logic with data, bounded vocabularies, discriminants, table rows, and reusable projections.
- [ALWAYS] collapse related variants into one polymorphic surface before adding entrypoints.
- [ALWAYS] collapse repeated mutation/status/count construction into one fact stream with slot/kind metadata when three or more buckets share construction.
- [ALWAYS] keep typed algorithm receipts when fields carry route, status, sampling, solver, spectral, mesh, extraction, benchmark, or host evidence.
- [ALWAYS] treat analyzer diagnostics as architecture pressure: fix true positives, refine false positives, and never use suppressions.
- [ALWAYS] treat planned future consumers as real design pressure. Zero current consumers never reduces the capability bar.
- [ALWAYS] create code as polymorphic, agnostic, and universal by default, ALWAYS PARAMETERIZE INPUTS/OUTPUTS + INGRESS/EGRESS.
- [ALWAYS] maintain semantic consistency in naming patterns of files, code functionality, types, classes, and functions.
- [ALWAYS] use one canonical semantic name per bounded concept; arity, filters, provider, and modality live in request shape, case, or policy row.
- [ALWAYS] extend the canonical owner before adding rails, public surfaces, wrappers, commands, flags, provider selectors, schemas, models, helpers, or files.
- [ALWAYS] keep boundary mapping at the edge; internal code uses canonical names and shapes.
- [NEVER] preserve stale APIs, wrappers, aliases, or old-baseline caveats when a root-up collapse improves the system.
- [NEVER] create operation families like `Get`, `GetMany`, `GetBy<Key>`, `List`, or `Search` for one concept when one polymorphic operation can discriminate by input value.

## [03]-[DEPENDENCY_POLICY]

[IMPORTANT] - External libraries, manifests, and host APIs are implementation surfaces:
- [ALWAYS] keep the package touch-point set bi-directionally aligned; central manager row, owning project manifest, branch README registry, folder README registry, owning `.api` tier; an orphaned member of the set is a defect to fix at its owner, never removed/dropped.
- [ALWAYS] treat dependencies declared in `pyproject.toml`, `pnpm-workspace.yaml`, `Directory.Packages.props`, project files, lockfiles, and equivalent manifests as first-class material.
- [ALWAYS] keep central package/version/tool ownership centralized in the one owning manifest or tool configuration — no per-package `pyproject.toml`, `package.json`, or `*.props`; assume the newest stable release and pin a package only when it is not yet compatible, removing the pin when compatibility lands.
- [ALWAYS] keep Python dependencies in root `pyproject.toml` as lean unpinned package names by default; add bounds or `python_version` markers only when resolver evidence requires them, prefer the newest viable release, remove constraints as compatibility lands, and keep wheel/floor/gate rationale out of Python docs, design docs, `.api` files, and comments.
- [ALWAYS] mine admitted packages to their full useful capability before writing local kernels.
- [ALWAYS] prefer ecosystem libraries that already own the domain concern over lower-level reinvention.
- [ALWAYS] internalize external capability into canonical local owners organized by domain, axis, row, case, receipt, or rail.
- [ALWAYS] keep C# MSBuild, NuGet, and `.csproj` manifests label-grouped by owner, sorted within coherent clusters, and limited to one-line maintenance comments.
- [ALWAYS] put shared C# substrate API catalogues under `libs/csharp/.api/`; package `.api/` folders carry domain catalogues and folder-specific overlays.
- [NEVER] mint a folder-tier `.api` file that duplicates or redirects to a substrate catalogue; a folder composing a substrate-carried package REGISTERS it.
- [NEVER] hand-roll functionality provided by admitted dependencies.
- [NEVER] create thin wrappers that rename, forward, or partially expose external APIs without adding domain value.
- [NEVER] encode package versions, provider caveats, or command catalogs outside the owning manifest, package charter, README, or tool owner.

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
- [ALWAYS] apply ordering as `section` -> `owner block` -> `runtime/declaration dependency` -> `semantic rank` -> `kind` -> `smaller-to-larger` -> `alphabetical`.
- [ALWAYS] prefer concept discovery order from stable declarations to composition: vocabulary, constants, models, failures, services, operations, wiring, exports.
- [ALWAYS] treat one generated type, smart enum, value object, schema/model family, wire model family, kernel, registry, catalog, table, dispatcher, query family, or composition root as an owner block; sort inside the owner instead of flattening its members into unrelated top-level sections.
- [ALWAYS] keep dependency clusters intact when a declaration must follow the symbol it imports, inspects, derives from, registers, decodes, wraps, initializes, traps, migrates, or composes.
- [ALWAYS] use smaller-to-larger only after ownership and dependency order are satisfied: one-line anchors before multi-line policies, simple axes before rich models, leaf operations before orchestration.
- [ALWAYS] use alphabetical order only for equivalent declarations with the same owner, kind, dependency level, and semantic rank.
- [ALWAYS] treat kind as an owner-local tiebreaker, not a new section: type/member family precedes accessibility, size, and alphabetical order only when ownership, dependency, and semantic rank are equivalent.
- [ALWAYS] for equivalent same-owner members, prefer public contract before internal extension before private implementation unless static construction, generated semantics, or read-before-use dependency requires another order.
- [ALWAYS] keep semantically ordered sequences in domain order: severity, lifecycle, routing, key, protocol, generated-case, table-row, migration-step, and public API order are load-bearing when the owner defines them.
- [ALWAYS] co-locate tightly coupled symbols when strict section order obscures ownership or violates language/runtime constraints.
- [ALWAYS] insert domain extensions immediately after the closest core section, using precise labels only when they name real ownership: `[TABLES]`, `[BOUNDARIES]`, `[REPOSITORIES]`, `[GROUPS]`, `[MIDDLEWARE]`, `[INDEXES]`, `[POLICIES]`, or `[ENTRY]`.
- [ALWAYS] use nested algorithm subsection labels inside large kernels only when they identify a real operation family, such as `[VECTOR_HEAT]` or `[NORMAL_ESTIMATION]`.
- [ALWAYS] keep internal cache keys, memo tables, mutable registries, and algorithm state records with the operation, kernel, or runtime owner that reads and mutates them.
- [ALWAYS] treat logger handles, provider handles, and dependency-backed runtime capabilities as `[SERVICES]`, not immutable anchors.
- [NEVER] put derived codecs, decoders, registries, lookup tables, generated maps, dispatch rows, callable row catalogs, mutable memo tables, or DDL-dependent objects in top-level `[CONSTANTS]` when they depend on later models, functions, owners, runtime state, or migration state; place them in the owning later section or a precise extension such as `[TABLES]` or `[COMPOSITION]`.
- [NEVER] split source-generated owners, delegate-backed enum behavior, validation partials, private operation-local state, resource/disposal boundaries, dispatch tables, SQL invariants, or migration units to satisfy mechanical section order.
- [NEVER] rename recurring categories per file; use canonical labels unless a domain extension is materially clearer.
- [NEVER] use alias or drift labels that merely rename core categories or hide complexity: `SCHEMA`, `FUNCTIONS`, `LAYERS`, `IMPORTS`, `INTERFACES`, `ENUMS`, `DTO`, `QUERIES`, `HELPERS`, `UTILS`, `COMMON`, `MISC`.

Language overlays refine the canonical order by runtime semantics.

- [CSHARP]: `[Union]`, `[SmartEnum]`, `[ValueObject]`, generated case families, static entries, delegate partials, validation partials, factories, and projections stay inside the declaring owner block. Preserve generated-case and smart-enum semantic order, with one generated case or static entry per physical declaration line unless a generator or runtime contract requires grouping. Static construction order inside a type is semantic when later fields derive from earlier fields. Static kernels, projectors, acceptors, and extension folds are `[OPERATIONS]` unless they own an actual dependency or service boundary. Inside a section, prefer attributes/delegates/marker types, enums/smart enums, readonly structs/records/value objects, records/classes/services, then owner-local private types when all earlier ordering constraints are equal. Inside a C# owner block, prefer generated/static dependency entries, fields/state, constructors/factories, properties, public operations, explicit boundary adapters, internal operations, then private kernels/implementation details.
- [PYTHON]: imports, `TYPE_CHECKING`, and import-time gates precede ordinary sections. Runtime decoders, encoders, registries, and tables follow the models/functions they inspect because module-level assignments execute immediately and runtime annotation consumers such as `msgspec` and `beartype` resolve real objects. `Annotated` validator functions may use `[BOUNDARIES]` between immutable constants and dependent aliases when the aliases must reference the real validator object.
- [TYPESCRIPT]: side-effect/value imports preserve runtime order, and `import type`/`export type` stay explicit. Runtime schemas/classes are `[MODELS]`, `Effect.Service` owners are `[SERVICES]`, `Layer`/runtime wiring is `[COMPOSITION]`, and catalog or registry rows that reference functions/classes stay after their referenced owners.
- [BASH]: shebang, ShellCheck directives, `set`/`shopt`, and environment/path gates are `[RUNTIME_PRELUDE]`; `readonly` values are `[CONSTANTS]`; `declare -Ar` maps are `[TABLES]`; traps, dispatch, source guards, and `_main` are late `[COMPOSITION]` or `[ENTRY]`.
- [PO_SQL]: extensions, schemas, and search-path guards are `[RUNTIME_PRELUDE]`; domains and types are `[TYPES]`; tables, constraints, generated columns, and partitions are `[MODELS]`; functions split by service boundary or query operation; indexes, triggers, row-level security, and policies are `[COMPOSITION]`; grants and comments are late `[EXPORTS]`.
- [YAML]: manifests and configuration files are data surfaces, not sectioned source; do not add code-section dividers. Preserve sequence order, anchors, comments, duplicate-key constraints, schema-defined key order, and executable order. Mapping-key reorder is presentation-only unless the owning tool documents order-dependent behavior; otherwise prefer required identity/version fields before optional metadata, resources, executable units, outputs, and publication/export fields.
