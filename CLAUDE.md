# [CLAUDE_MANIFEST]

MUST READ: `libs/.planning/RULINGS.md` + `libs/.planning/ARCHITECTURE.md`

Rasm is in a long-term planning phase, working strictly within spec-sheets, not code files:
- All `libs/` spec docs are the rebuild surface: rebuilt ground-up each pass, freely and aggressively; always address cross file/folder ripples on landing.
- `/prime <target>` executes the grounding ladder; never hand-derive the read set.

## [01]-[REQUIRED]

All mistakes/problems/oversights that are structural are abstracted/defined and added in `docs/laws/scars.md` (Ex: identifying a rebuild of a system of capability, finding various mistakes in fundamental approach, code logic structure, etc). Nuanced mistakes or problems are also recorded to ensure no repeat of the same approach, likewise, code rebuilding/refactoring due to code quality gaps, or strata integration oversights are recorded as well. ALWAYS read the `docs/laws/scars.md` at the start of a session, to ensure mistakes are not repeated.

- Design work in `libs/<language>/` requires FULL reading and adherence across ALL files in `libs/<language>/.planning/` AND `docs/stacks/<language>/`.
- Each `libs/<language>/` and `libs/<language>/<sub-folder>/` carry a `.api/` folder; all work stacks external-lib capability from BOTH sources — REQUIRED.
- Durable planning docs — index docs, spec-sheets, `.api` catalogs — follow `libs/.planning/README.md`; the campaign loop follows `campaign-method.md`.
- `RULINGS.md` is settled law per tier: read before re-deciding; narrowest tier owns; a violation routes as a card at the owning tier, never inline.
- Every homeless settled decision lands its `RULINGS.md` row at the narrowest owning tier in the same pass — never deferred to session end.
- Durable lessons land at the end of session via `docs/laws/README.md` admission ladder; refute-first proves no owner already holds the fact.
- `docs/laws/topology.md` binds counterpart obligations — consult it before any multi-surface edit.

[DOC_TOPOLOGY]: Every durable question has one owning surface — consult the owner, never re-derive or guess:

| [INDEX] | [SURFACE]                                   | [OWNS]                                                                              |
| :-----: | :------------------------------------------ | :---------------------------------------------------------------------------------- |
|  [01]   | `libs/.planning/campaign-method.md`         | Approach standards, quality bar, agent-role law                                     |
|  [02]   | `libs/.planning/README.md`                  | Doc-set per tier, card schema + lifecycle markers, spec-sheet grammar               |
|  [03]   | `libs/.planning/ARCHITECTURE.md`            | Stratification law, cross-branch direction, wire seams, `.planning/` lifecycle      |
|  [04]   | `libs/.planning/RULINGS.md`                 | Cross-libs settled decisions                                                        |
|  [05]   | `libs/.planning/planning-targets.md`        | Target index across the corpus                                                      |
|  [06]   | `libs/<language>/.planning/`                | Language-wide doc-set for cross-folder decisions                                    |
|  [07]   | `libs/<language>/<folder>/`                 | Folder doc set at root — core three README/ARCHITECTURE/RULINGS + IDEAS/TASKLOG     |
|  [08]   | `docs/README.md`                            | Doctrine router: `standards/`, `stacks/<language>/`, `laws/`, `atlas/`, `glossary/` |
|  [09]   | `docs/laws/`                                | Repo maintenance law: edit couplings, cross-branch patterns, regression scars       |
|  [10]   | `docs/glossary/`                            | Binding sense per reused term, and the divergence its `[NOT]` line names            |
|  [11]   | `tests/README.md` + `tests/RULINGS.md`      | Proof-estate law — read before any test work                                        |
|  [12]   | root `README.md` + `tools/<tool>/README.md` | Tool owners, output routing, operator roles                                         |

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
- [ALWAYS]: use `ast-grep` skill on every code surface — outline before reading source, structural search over grep, rewrites, and durable rules.
- [ALWAYS]: use `exa` MCP to start open-web search with neural discovery, the right page, repo, paper, or entity; REPLACES `WebFetch` entirely.
- [ALWAYS]: use `search-tavily` skill on known targets — extract or crawl a site, or run a cited multi-source report.
- [ALWAYS]: use `search-context7` skill when working on code/fences with external libraries, never guess on SDK/framework/API capabilities or implementations.
- [ALWAYS]: use `nuget` MCP to validate the existence of a package and newest version available.
- [ALWAYS]: use `claudeCodeDocs` MCP when working on Claude Code configs or harness questions; capabilities, memory, skills, hooks, plugins, settings.
- [ALWAYS]: use `openaiDeveloperDocs` MCP when working on Codex configs or harness questions; capabilities, memory, skills, hooks, plugins, settings.
- [ALWAYS]: use `uv run python -m tools.assay static` for static quality `.py`, `.ts/.tsx`, and `.cs` files (ruff/ty/mypy, tsc/biome, dotnet format/build).
- [ALWAYS]: use `tools.assay provision` for Forge service, Postgres-extension, and DuckDB/SQLite surface evidence before an availability claim lands.

## [02]-[IMPLEMENTATION_STANDARDS]

[CRITICAL]:
- [NEVER]: preserve code; no wrappers, aliases, obsolete markers, shims, or migrations or old-baseline caveats - full removal and refactor all ripples.
- [NEVER]: use weak, unbounded, or erased types where the language can express the domain precisely.
- [NEVER]: use exception-style control flow in domain logic; use typed error rails and the required route's recovery patterns.
- [NEVER]: use imperative branching when a bounded vocabulary, dispatch table, generated switch, match, fold, or monadic rail can own the variation.
- [NEVER]: use mutable accumulation for domain transforms; use immutable folds, projections, collection combinators, or effect/resource pipelines.
- [NEVER]: extract code to new files to reduce LOC. Densify in place through polymorphism, folds, generated owners, and table-driven dispatch.
- [NEVER]: delete functionality to satisfy a density or LOC signal. Preserve capability through denser owners.

[IMPORTANT]:
- [ALWAYS]: ASSUME 10X THE COMPLEXITY AND DEMANDS ON EVERY SURFACE — a naive, simple, or surface-level solution is rejected, removed, and rebuilt on sight.
- [ALWAYS]: rebuild functionality/code/logic GROUND-UP — tear existing patterns apart for surface density with zero functionality lost.
- [ALWAYS]: land new functionality as if designed in from the start, never as tacked-on flat-code spam.
- [ALWAYS]: extend a class to the full concept it admits NOW — a 4-field shape for a 12+ concept widens in place, never proliferates objects.
- [ALWAYS]: treat planned future consumers as real design pressure. Zero current consumers never reduces the capability bar.
- [ALWAYS]: extend the canonical owner before adding rails, public surfaces, wrappers, commands, flags, provider selectors, schemas, models, helpers, or files.
- [ALWAYS]: co-locate domain logic with its owner instead of scattering it into generic support files.
- [ALWAYS]: create code as polymorphic, agnostic, and universal by default, ALWAYS PARAMETERIZE INPUTS/OUTPUTS + INGRESS/EGRESS.
- [ALWAYS]: collapse related variants into one polymorphic surface before adding entrypoints.
- [ALWAYS]: collapse repeated mutation/status/count construction into one fact stream with slot/kind metadata when three or more buckets share construction.
- [ALWAYS]: drive logic with data, bounded vocabularies, discriminants, table rows, and reusable projections.
- [ALWAYS]: keep typed algorithm receipts when fields carry route, status, sampling, solver, spectral, mesh, extraction, benchmark, or host evidence.
- [ALWAYS]: keep boundary mapping at the edge; internal code uses canonical names and shapes.
- [ALWAYS]: treat analyzer diagnostics as architecture pressure: fix true positives, refine false positives, and never use suppressions.
- [ALWAYS]: maintain semantic consistency in naming patterns of files, code functionality, types, classes, and functions, USE 1-2 word values; avoid 3+
- [ALWAYS]: use one canonical semantic name per bounded concept; arity, filters, provider, and modality live in request shape, case, or policy row.

## [03]-[DEPENDENCY_POLICY]

[IMPORTANT] - External libraries, manifests, and host APIs are implementation surfaces:
- [ALWAYS]: treat admitted packages and ecosystem libraries as first class; mine their full capability before any local kernel or hand-roll.
- [ALWAYS]: keep C# MSBuild/NuGet manifests label-grouped by owner, cluster-sorted, with one-line maintenance comments at most.
- [ALWAYS]: align the package touch-point set both ways: central manager row, project manifest, branch/folder README registries, owning `.api` tier.
- [ALWAYS]: repair an orphaned touch-point member at its owner, never by removal.
- [ALWAYS]: centralize package, version, and tool ownership in one owning manifest — no per-package `pyproject.toml`, `package.json`, or `*.props`.
- [ALWAYS]: assume the newest stable release; pin only while incompatible and drop the pin when compatibility lands (verify with tools, pnpm, nuget, etc).
- [ALWAYS]: keep root `pyproject.toml` dependencies as lean unpinned names; remove bounds/`python_version` markers if proven stale/unnecessary.
- [NEVER]: mint a folder-tier `.api` file duplicating or redirecting to a substrate catalogue; a folder composing a substrate package REGISTERS it.
- [NEVER]: create thin wrappers that rename, forward, or partially expose external APIs without adding domain value.

## [04]-[FILE_ORGANIZATION]

Section separators: language comment marker + space + `---` + bracketed UPPERCASE snake label with no internal spaces + dash fill.

```md template
// Typescript/Csharp Styling
// --- [TYPES] ---------------------------------------------------------------------------

// --- [SUBSECTION]

# Python Styling
# --- [CONSTANTS] ------------------------------------------------------------------------

# --- [SUBSECTION]
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
- [ALWAYS]: use nested algorithm subsection labels inside large kernels to identify a real operation family, such as `[VECTOR_HEAT]` or `[NORMAL_ESTIMATION]`.
- [ALWAYS]: keep internal cache keys, memo tables, mutable registries, and algorithm state records with the operation/kernel/runtime owner that mutates them.
- [ALWAYS]: treat logger handles, provider handles, and dependency-backed runtime capabilities as `[SERVICES]`, not immutable anchors.
- [ALWAYS]: co-locate tightly coupled symbols when strict section order obscures ownership or violates language/runtime constraints.
- [ALWAYS]: treat one registry, catalog, table, or composition root the same; sort inside the owner, never flattened into top-level sections.
- [ALWAYS]: apply smaller-to-larger only after ownership and dependency: anchors before policies, axes before models, leaf ops before orchestration.
- [ALWAYS]: treat kind as an owner-local tiebreaker, not a new section — it ranks only among peers equal in ownership, dependency, and semantic rank.
- [ALWAYS]: order same-owner peers public, then internal, then private — unless static construction, generated semantics, or read-before-use wins.
- [ALWAYS]: hold owner-defined domain order: severity, lifecycle, routing, key, protocol, generated-case, table-row, migration-step, public API.
- [ALWAYS]: insert a domain extension right after its closest core section; a precise label is earned by real ownership.
- [ALWAYS]: extension vocabulary: `[TABLES]`, `[BOUNDARIES]`, `[REPOSITORIES]`, `[GROUPS]`, `[MIDDLEWARE]`, `[INDEXES]`, `[POLICIES]`, `[ENTRY]`.
- [NEVER]: split source-generated owners, delegate-backed enum behavior, validation partials, or operation-local state for mechanical section order.
- [NEVER]: split resource/disposal boundaries, dispatch tables, SQL invariants, or migration units to satisfy section order.
- [NEVER]: seat callable row catalogs, memo tables, or DDL-dependent objects in `[CONSTANTS]`; home each in its owning later section or extension.
