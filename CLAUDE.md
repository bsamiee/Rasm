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

| [INDEX] | [FILE_TYPE]                | [REQUIRED_SKILL]        |
| :-----: | -------------------------- | ----------------------- |
|   [1]   | TypeScript (`.ts`, `.tsx`) | `coding-ts`             |
|   [2]   | C# (`.cs`)                 | `coding-csharp`         |
|   [3]   | Python (`.py`)             | `coding-python`         |
|   [4]   | Bash/sh (`.sh`, `.bash`)   | `coding-bash`           |
|   [5]   | SQL (`.sql`)               | `coding-pg`             |
|   [6]   | Markdown/docs (`.md`)      | `docgen`, `style-standards` |

[IMPORTANT]:
- [ALWAYS] Language-specific mechanics come from the required `coding-*` skill.
- [ALWAYS] Treat `.claude/skills/*` as the active local skill directory unless project config proves otherwise.
- [ALWAYS] Documentation mechanics come from `docgen`; Markdown structure and voice come from `style-standards`.

---
## [2][BEHAVIOR]

[IMPORTANT]:
- [ALWAYS] Use new sources when conducting research; sources [MUST] be within last 6 months from current date.
- [ALWAYS] Tools over internal knowledge: read files, search codebase, verify assumptions.
- [ALWAYS] Parallelize aggressively: run multiple searches, read several files, call independent tools concurrently.
- [ALWAYS] Reference symbols by name; avoid inline code blocks for context already shown.

[CRITICAL]:
- [NEVER] Use emojis; use `[X]` style markers with concise UPPERCASE formatting.

---
## [3][DEPENDENCY_POLICY]

[IMPORTANT]: **External-Lib-First**: approved dependencies are primary implementation surface.
- [ALWAYS] Treat dependencies declared in `pyproject.toml`, `pnpm-workspace.yaml`, `Directory.Build.props`/`build.props`, and equivalent manifests as first-class libraries.
- [ALWAYS] Integrate approved external libraries directly; use native APIs end-to-end.
- [ALWAYS] Prefer ecosystem libraries that already own the domain concern over local reinvention.
- [NEVER] Hand-roll functionality already provided by approved dependencies.
- [NEVER] Prefer stdlib alternatives when approved external libraries already cover the requirement.
- [NEVER] Create thin wrappers that rename or forward external APIs without adding domain value.

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
- [NEVER] Treat ~350 LOC or any specific byte-count as a refactor trigger. The trigger is concept density: parallel types ≥3, sibling factories ≥3, repeated switch arms ≥3, single-call helpers ≥3.
- [NEVER] Delete functionality to satisfy a "density" or "LOC" signal. Functionality is preserved in capability through denser polymorphic surfaces, not removed.

[IMPORTANT]:
- [ALWAYS] Collapse related variants into one polymorphic surface before adding new entrypoints.
- [ALWAYS] Drive logic algorithmically with data, bounded vocabularies, discriminants, and reusable projections.
- [ALWAYS] Keep boundary translation at the boundary; internal code uses canonical names and shapes.
- [ALWAYS] Co-locate domain logic with the owning module instead of scattering it into generic support files.

---
## [5][OUTPUT]

[IMPORTANT]:
- [ALWAYS] Use `backticks` for file paths, symbols, and CLI commands.
- [ALWAYS] Avoid large code blocks; reference file/symbol names instead.
- [ALWAYS] Use Markdown: headings for structure, bullets for lists, tables for comparisons.
- [ALWAYS] Keep responses actionable; lead with what changed, not what you will do.

### [5.1][DEPENDENCIES]

[IMPORTANT]:
1. [ALWAYS] **Check catalog**: `rg -n "my-dep" pnpm-workspace.yaml`.
2. [ALWAYS] **Add to catalog** (if missing): `my-dep: 1.2.3` (exact version).
3. [ALWAYS] **Reference**: `"dependencies": { "my-dep": "catalog:" }`.
4. [ALWAYS] **Install**: `pnpm install`.
5. [ALWAYS] **Validate**: `pnpm exec nx run-many -t typecheck`.

### [5.2][QUALITY_GATES]

[IMPORTANT]:
1. [ALWAYS] **During iteration** use `bash scripts/check-cs.sh check` — routes changed files to owning projects, fast.
2. [ALWAYS] **After each discrete phase of work** run `bash scripts/check-cs.sh check` to catch analyzer violations while context is fresh.
3. [ALWAYS] **Final completion** use `bash scripts/check-cs.sh test` — runs analyzer + format + tests on affected projects.
4. [NEVER] **Run `full` or `test-full`** unless trigger files (`Directory.Build.props`, `.editorconfig`, `*.csproj`, `Workspace.slnx`) were modified — these rebuild the entire solution.
5. [ALWAYS] **Trust the analyzer**: 50+ CSP rules (`tools/cs-analyzer/Kernel/RuleCatalog.cs`) enforce coding-csharp standards. When CSP#### fires, fix the architecture; do not suppress.

Reference: `scripts/check-cs.sh` MODE_SPEC routes are `check` (changed/check), `full` (full/check), `test` (changed/test), `test-full` (full/test).

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
