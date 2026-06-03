# [CLAUDE_MANIFEST]

Operate as a senior developer in a bleeding-edge monorepo; use the newest viable current versions of languages, libraries, plugins, extensions, and add-ons after verifying tooling behavior from docs or local output.

[IMPORTANT]:
- [ALWAYS] Treat monorepo code as polymorphic, agnostic, and universal by default.
- [ALWAYS] Identify canonical object shapes, field names, and semantics that scale across packages and apps.
- [ALWAYS] Reuse established naming patterns; prefer universal names (`countBy`) over narrow variants (`countByIp`, `countByX`).
- [ALWAYS] If an external contract requires a different name, isolate mapping at boundary adapters and keep canonical names internally.
- [NEVER] Rename a canonical concept across schemas/models/classes, parameters, and return keys within the same bounded context.

---
## [1][REQUIRED_STANDARDS]

If reviewing, refining, editing, creating, or modifying X file type, use skill Y (required):

| [INDEX] | [FILE_TYPE]                         | [REQUIRED_SKILL]            |
| :-----: | ----------------------------------- | --------------------------- |
|   [1]   | TypeScript (`.ts`, `.tsx`)          | `coding-ts`                 |
|   [2]   | C# production (`.cs`)               | `coding-csharp`             |
|   [3]   | C# tests (`.spec.cs`)               | `cs-testing`                |
|   [4]   | RhinoCode scenarios (`.verify.csx`) | `cs-testing`                |
|   [5]   | Python (`.py`)                      | `coding-python`             |
|   [6]   | Bash/sh (`.sh`, `.bash`)            | `coding-bash`               |
|   [7]   | SQL (`.sql`)                        | `coding-pg`                 |
|   [8]   | Markdown/docs (`.md`)               | `docgen`, `style-standards` |

[IMPORTANT]:
- [ALWAYS] Language-specific mechanics come from the required `coding-*` skill.
- [ALWAYS] Treat `.claude/skills/*` as project skill context and `/Users/bardiasamiee/.codex/skills/*` as Codex runtime skill context; keep overlapping standards mirrored when they govern this repo.
- [ALWAYS] Documentation mechanics come from `docgen`; Markdown structure and voice come from `style-standards`.

---
## [2][BEHAVIOR]

[IMPORTANT]:
- [ALWAYS] Use new sources when conducting research; sources [MUST] be within last 9 months from current date unless stable official docs are the only primary source for a settled platform rule.
- [ALWAYS] Tools over internal knowledge: read files, search codebase, verify assumptions.
- [ALWAYS] Parallelize aggressively: run multiple searches, read several files, call independent tools concurrently.
- [ALWAYS] Reference symbols by name; avoid inline code blocks for context already shown.

[CRITICAL]:
- [NEVER] Use emojis; use `[X]` style markers with concise UPPERCASE formatting.

---
## [3][DEPENDENCY_POLICY]

[IMPORTANT]: **External-Lib-First**: approved dependencies are primary implementation surface.
- [ALWAYS] Treat dependencies declared in `pyproject.toml`, `pnpm-workspace.yaml`, `Directory.Packages.props`, and equivalent manifests as first-class libraries.
- [ALWAYS] Integrate approved external libraries directly; use native APIs end-to-end.
- [ALWAYS] Prefer ecosystem libraries that already own the domain concern over local reinvention.
- [NEVER] Hand-roll functionality already provided by approved dependencies.
- [NEVER] Prefer stdlib alternatives when approved external libraries already cover the requirement.
- [NEVER] Create thin wrappers that rename or forward external APIs without adding domain value.

[IMPORTANT]: **.NET-Central-Package-Management**: C# package versions live in `Directory.Packages.props`; project files may declare usage but never versions.
- [ALWAYS] Check `docs/system-api-map` before adding a `System.*` package, global using, or BCL replacement.
- [ALWAYS] Keep RhinoWIP/GH2/Eto/System.Drawing host assemblies resolved through `Directory.Build.props` app-bundle references; if SDK compilation needs a NuGet reference surface, add it only as a conditioned central compile package.
- [NEVER] Add unused `PackageVersion` entries as future intent.

---
## [4][UNIVERSAL_CONSTRAINTS]

[CRITICAL]:
- [NEVER] Use weak, unbounded, or erased types where the language can express the domain precisely.
- [NEVER] Use imperative branching in domain logic; use the language skill's expression, match, dispatch-table, or monadic ROP patterns.
- [NEVER] Use mutable accumulation for domain transforms; use immutable folds, projections, collection combinators, or effect/resource pipelines.
- [NEVER] Use exception-style control flow in domain logic; use typed error rails and the required skill's recovery patterns.
- [NEVER] Proliferate schemas, structs, models, branded types, records, classes, or aliases for the same concept.
- [NEVER] Create helper/utility files or functions (`helpers.*`, `*Helper`, `*Util`, `common.*`) for single-caller or thin indirection.
- [NEVER] Create wrappers, unnecessary intermediate bindings, single-use aliases, or constant spam.
- [NEVER] Split one concern across parallel names, objects, services, or error rails.
- [NEVER] Add comments describing "what"; reserve comments for "why", boundary exceptions, and non-obvious invariants.
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
- [ALWAYS] Use indexed `Map(..., index)` plus `TraverseM(identity)` when an indexed effectful traversal is needed and no native indexed `TraverseM` exists; do not replace it with an index-threaded fold unless the fold carries additional algorithm state.

---
## [5][OUTPUT]

[IMPORTANT]:
- [ALWAYS] Use `backticks` for file paths, symbols, and CLI commands.
- [ALWAYS] Avoid large code blocks; reference file/symbol names instead.
- [ALWAYS] Use Markdown: headings for structure, bullets for lists, tables for comparisons.
- [ALWAYS] Keep responses actionable; lead with what changed, not what you will do.

### [5.1][DEPENDENCIES]

[IMPORTANT] TypeScript dependency workflow:

[IMPORTANT]:
1. [ALWAYS] **Check catalog**: `rg -n "my-dep" pnpm-workspace.yaml`.
2. [ALWAYS] **Add to catalog** (if missing): `my-dep: 1.2.3` (exact version).
3. [ALWAYS] **Reference**: `"dependencies": { "my-dep": "catalog:" }`.
4. [ALWAYS] **Install**: `pnpm install`.
5. [ALWAYS] **Validate**: `pnpm exec nx run-many -t typecheck`.

[IMPORTANT] C# dependency workflow:
1. [ALWAYS] **Check package truth**: `rg -n "<PackageId>" Directory.Packages.props Directory.Build.props **/*.csproj`.
2. [ALWAYS] **Add version centrally** only when a concrete consumer is added.
3. [ALWAYS] **Keep project references versionless** under central package management.
4. [ALWAYS] **Validate graph**: use `uv run python -m tools.quality static plan <changed-manifest>` when routing is uncertain, then `uv run python -m tools.quality static full` for central package, solution, global runner, `.editorconfig`, or analyzer trigger changes.

### [5.2][QUALITY_GATES]

Three orthogonal rails: static analysis, unit tests, runtime verification. Each tool verb owns one rail; never conflate.

[CRITICAL]:
1. [NEVER] Run static, test, or bridge rails for comment-only, divider-only, declaration-order, or move-only organization work unless the user explicitly requests a quality rail or preservation proof fails.
2. [ALWAYS] Prove move-only organization with `git diff --check` plus line, declaration, or hash preservation at the movement boundary. Report no compile, analyzer, test, or runtime claim.

[IMPORTANT]:
1. [ALWAYS] **Static fix** — `uv run python -m tools.quality static fix [paths...]`. Run after executable C# source changes, analyzer remediation, or user-requested cleanup when safe autofix is desired. Routes changed files or explicit paths to owning projects. Applies scoped `dotnet format whitespace`, `style`, and `analyzers` fixes. No build, no tests.
2. [ALWAYS] **Static build** — `uv run python -m tools.quality static build [paths...]`. Run after semantic or compilable source changes. Routes changed files or explicit paths to owning project closure. Runs restore + build + MSBuild analyzers for compile proof. No formatting, no tests.
3. [ALWAYS] **Full static** — `uv run python -m tools.quality static full`. Runs `Workspace.slnx` parity plus full-solution restore/build/analyzers. Required only when trigger files change (`Directory.Build.props`, `Directory.Build.targets`, `Directory.Packages.props`, `Workspace.slnx`, `.editorconfig`, `global.json`, `tools/cs-analyzer/**`).
4. [ALWAYS] **Unit tests** — `uv run python -m tools.quality test run [<filter>]`. Runs .NET 10 MTP against the library tests target (`tests/csharp/libs/Rasm/Rasm.Tests.csproj` by default; override via `--target <csproj>` or use `--all`). Mutation is explicit via `--mutation changed|full`; default test runs are unit-only.
5. [ALWAYS] **Rhino runtime verification** — `uv run python -m tools.quality bridge verify <path-or-glob>`. Routes scenarios through the in-process bridge against running `RhinoWIP.app`. Outputs JSON evidence and PNG captures under `.artifacts/rhino/verify/`. See the `cs-testing` skill.
6. [ALWAYS] **Trust the analyzer**: 50+ CSP rules (`tools/cs-analyzer/Kernel/RuleCatalog.cs`) enforce coding-csharp standards. When CSP#### fires, fix the architecture; do not suppress.
7. [NEVER] Re-introduce a `test` mode into the static rail. Tests are a separate gate.
8. [ALWAYS] **Parallel agents** — `quality static fix`, `quality static report`, `quality static build`, and unit test runs isolate scratch under `.artifacts/quality/<rail>/<run-id>/` and may run concurrently. Stryker mutation is opt-in and fail-fast on `.artifacts/locks/mutation.lock`. Live Rhino bridge commands, bridge verify, bridge package live steps, and package stage commit use fail-fast leases.

### [5.3][PLAN_DISCIPLINE]

[IMPORTANT]:
- [ALWAYS] Plans are documents, not narratives. Maximum 1-2 screen pages.
- [ALWAYS] Structure: Context (1 sentence on why), Critical files (paths + line numbers), Approach (3-5 bullets), Verification (1-2 commands). Nothing else.
- [NEVER] Include "Phase 1...Phase N" workflow narration, alternatives considered, or implementation prose. The plan is the recommendation, not a journal.
- [ALWAYS] If a plan exceeds the page limit, that is signal to collapse the problem, not expand the prose.

### [5.4][SURFACE_PREFERENCE]

[IMPORTANT]:
- [ALWAYS] Prefer FEWER deep, complex surfaces over MANY loose, simple ones. A single 400-LOC type that owns a full polymorphic concern is better than four 100-LOC types that fragment it.
- [ALWAYS] The unit of design is the polymorphic dispatch surface, not the file.

---
## [6][FILE_ORGANIZATION]

[IMPORTANT] **Section separators**: language comment marker + space + `---` + bracketed UPPERCASE label with spaces replaced by `_` + dash fill to 90 columns.

```typescript
// --- [TYPES] ---------------------------------------------------------------------------
```

```python
# --- [CONSTANTS] ------------------------------------------------------------------------
```

```csharp
// --- [SERVICES] ------------------------------------------------------------------------
```

**Canonical order** (omit unused): `TYPES` -> `MODELS` -> `CONSTANTS` -> `ERRORS` -> `SERVICES` -> `OPERATIONS` -> `COMPOSITION` -> `EXPORTS`.

**Core Sections**:
- `[TYPES]`: type aliases, inferred types, protocols/interfaces, discriminated unions.
- `[MODELS]`: runtime schemas, records/classes, value objects, DTOs, table/domain models.
- `[CONSTANTS]`: immutable vocabularies, policies, schedules, config anchors, lookup tables.
- `[ERRORS]`: typed error rails, tagged failures, domain failure policies.
- `[SERVICES]`: service contracts, dependency surfaces, application/service classes.
- `[OPERATIONS]`: pure transforms, effect/result pipelines, algorithms, repository operations.
- `[COMPOSITION]`: layers, decorators, dependency wiring, middleware, runtime composition roots.
- `[EXPORTS]`: named exports, `__all__`, or language-equivalent public surface declarations.

[IMPORTANT]:
- [ALWAYS] Prefer concept discovery order from stable declarations to composition.
- [ALWAYS] Co-locate tightly coupled symbols when strict section order would obscure ownership or violate language/runtime constraints.
- [ALWAYS] Insert domain extensions immediately after the closest core section, using stable labels such as `[TABLES]`, `[REPOSITORIES]`, `[GROUPS]`, or `[MIDDLEWARE]`.
- [NEVER] Rename recurring categories per file; use canonical labels unless a domain extension is materially clearer.
- [NEVER] Use semantic drift labels: `HELPERS`, `UTILS`, `HANDLERS`, `MISC`, `COMMON`, `CONFIG`, `DISPATCH_TABLES`.
