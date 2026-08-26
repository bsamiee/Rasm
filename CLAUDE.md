# [CLAUDE_MANIFEST]

MUST READ: `libs/.planning/ARCHITECTURE.md`

Rasm is in a long-term planning phase, working strictly within spec-sheets, not code files. All `libs/` spec docs are the rebuild surface: rebuilt ground-up each pass, freely and aggressively; always address cross file/folder ripples on landing.

## [01]-[REQUIRED]

- Design work in `libs/<language>/` requires FULL reading of `libs/<language>/.planning/` AND `docs/stacks/<language>/`, adhering to both.
- Each `libs/<language>/` and its sub-folders carry `.api/`; all work stacks external-lib capability from BOTH tiers — REQUIRED.
- Index docs, spec-sheets, and `.api` catalogs follow `libs/.planning/README.md`.
- `RULINGS.md` is settled law: read before re-deciding; narrowest tier owns; violations route as cards there, never inline; rows land same-pass.

[DOC_TOPOLOGY]: Every durable question has one owning surface — consult the owner, never re-derive or guess:

| [INDEX] | [SURFACE]                        | [OWNS]                                                                         |
| :-----: | :------------------------------- | :----------------------------------------------------------------------------- |
|  [01]   | `libs/.planning/README.md`       | Doc-set per tier, card schema + lifecycle markers, spec-sheet grammar          |
|  [02]   | `libs/.planning/ARCHITECTURE.md` | Stratification law, cross-branch direction, wire seams, `.planning/` lifecycle |
|  [03]   | `libs/.planning/RULINGS.md`      | Cross-libs settled decisions                                                   |
|  [04]   | `libs/<language>/.planning/`     | Language-wide doc-set for cross-folder decisions                               |
|  [05]   | `libs/<language>/<folder>/`      | Folder doc set at root — README/ARCHITECTURE/RULINGS                           |

[STANDARDS_ROUTING]: Use the route-owned standard for the file being edited:

| [INDEX] | [FILE_TYPE]                | [ROUTE]                  | [USE_WHEN]                       | [NAMING_SCHEMA] |
| :-----: | :------------------------- | :----------------------- | :------------------------------- | :-------------- |
|  [01]   | C# (`.cs`)                 | `docs/stacks/csharp`     | `libs/dotnet` + `.cs`            | `PascalCase`    |
|  [02]   | Python (`.py`)             | `docs/stacks/python`     | `libs/python` + `.py`            | `snake_case`    |
|  [03]   | TypeScript (`.ts`, `.tsx`) | `docs/stacks/typescript` | `libs/typescript` + `.ts`/`.tsx` | `camelCase`     |
|  [04]   | Bash/sh (`.sh`, `.bash`)   | `coding-bash`            | [ANY]                            | `kebab-case`    |
|  [05]   | SQL (`.sql`)               | `coding-pg`              | [ANY]                            | `snake_case`    |

[TOOL_ROUTING]:
- ALWAYS use `ast-grep` skill on every code surface — outline before reading source, structural search over grep, rewrites, and durable rules.
- ALWAYS use `exa` MCP to start open-web search with neural discovery, the right page, repo, paper, or entity; REPLACES `WebFetch` entirely.
- ALWAYS use `search-tavily` skill on known targets — extract or crawl a site, or run a cited multi-source report.
- ALWAYS use `search-context7` skill on code/fences with external libraries; never guess SDK/framework/API capabilities or implementations.
- ALWAYS use `nuget` MCP to validate the existence of a package and newest version available.
- ALWAYS use `claudeCodeDocs`/`openaiDeveloperDocs` MCP for Claude Code or Codex config and harness questions; memory, skills, hooks, settings.

[CLI_ESTATE]: Navigation and scratch iteration only — these rows are operator-box tool contracts (several ride machine config), and no verdict, gate, or CI lane may depend on one:

| [INDEX] | [TOOL]    | [GUIDANCE]                                                                                                                  |
| :-----: | :-------- | :-------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `tree`    | `tree -a <dir>`, `-L n` sets depth (default 4); `-D` lists dirs only.                                                       |
|  [02]   | `loc`     | `loc --json <dir>`, rows sort inside folder groups, ranks on `folder`+`file`                                                |
|  [03]   | `fmt`     | `fmt --check <dir>` verifies, `fmt <dir>` writes, routes each file to its formatter by extension; `.cs` has no lane.        |
|  [04]   | `fd`      | `--hidden` is baked in `-H` is noise; pattern is regex — `*.md` errors, take `-e md` or `-g`; `-I` admits ignored.          |
|  [05]   | `rg`      | `--smart-case --hidden`; `-s` pins case; types `docs agent config data lock`; `-U` spans `\n`; `-r`=replace, `-E`=encoding. |
|  [06]   | `jq`/`yq` | `yq` is mikefarah v4 — `yq '.expr' f`, never `yq r`; `jq` needs `-r` for shell values and `[]?` on optional arrays.         |
|  [07]   | `gh`      | Non-TTY prints nothing when empty — never read the table; count through `--json <fields> \| jq length`.                     |

## [02]-[IMPLEMENTATION_STANDARDS]

Universal code law: binds every language, present or future; `docs/stacks/<language>/` deepens it per stack and never weakens it.

[CRITICAL]:
- ALWAYS ASSUME 10X THE COMPLEXITY AND DEMANDS ON EVERY SURFACE — a naive, simple, or surface-level solution is rejected and rebuilt on sight.
- ALWAYS land new functionality as if designed in from the start, never as tacked-on flat-code spam; extend the owner before minting a sibling.
- ALWAYS model the full domain on every owner — a missing axis is a defect, not thrift, and zero current consumers never lowers the bar.
- NEVER use weak, unbounded, or erased types where the language can express the domain precisely.
- NEVER use exception-style control flow in domain logic; use typed error rails and the required route's recovery patterns.
- NEVER spell absence as null, sentinel, or magic default past the boundary; absence rides an option-shaped carrier consumers unwrap.

[POLYMORPHISM] - Fewer, stronger owners over many loose shapes; variants are cases inside one closed family, never sibling types:
- ALWAYS fold one polymorphic entrypoint per concern, discriminating on input shape; forward and inverse of one correspondence share one owner.
- ALWAYS collapse siblings sharing an identity regime, admission path, payload timing, or consumer; survival needs a discriminant named on site.
- ALWAYS fold repeated mutation/status/count construction into one fact stream with slot/kind metadata; the trigger is shared shape, never count.
- ALWAYS widen an owner in place to the full concept it admits NOW — the next case lands as one declaration, consumers untouched or loudly broken.
- ALWAYS close dispatch by default — a catch-all over an owned family turns a compile break into a silent pass; openness needs foreign extension.
- NEVER mint entrypoint siblings — name-suffix families, arity twins, boolean mode knobs; the discriminant must be recoverable from the value.
- NEVER guard an invalid state at each use; make it unrepresentable at construction and canonicalize at intake so consumers read one regime.
- NEVER mint a two-case closed family whose cases carry no payload, nor a roster mirroring another owner's cases — a bool column or option.

[PARAMETERIZATION] - Variation lives in data or a typed parameter, never in a name, a flag, or a body:
- ALWAYS hunt both directions — a literal encoding a decision becomes a policy row; a knob set whose combinations the body re-derives collapses.
- ALWAYS test parameters by deletion — one the input value or policy already reconstructs was a knob; collapse it into the owner.
- ALWAYS treat a hardcoded instance roster as seed data for the algebraic owner; a closed member set is lawful only where the owner decides it.
- ALWAYS declare one primary correspondence and derive every secondary map, type, and name from it — the derivation is the executable spec.
- ALWAYS keep one authority per derived value — a hand-kept mirror derives from its roster, or the invariant states at both owners, moving as one.
- ALWAYS return typed exhaustion faults when a bounded budget runs out — a success-shaped fall-through certifies unconverged as converged.
- ALWAYS declare one recovery posture per fault reason at the family owner; cross-cutting policy composes as values, never per call site.

[ADMISSION]:
- ALWAYS admit foreign material once at the boundary into evidence-carrying owners; the interior never re-validates and never sees raw shapes.
- ALWAYS choose the outcome carrier once at admission, thread it unchanged, and collapse it only at the host, UI, or wire edge.
- ALWAYS shape domain logic as expressions on the rail — dependence sequences, independence accumulates, and the carrier, never a flag, selects.
- ALWAYS keep boundary mapping at the edge; internal code uses canonical names and shapes.

[COMPOSITION]:
- ALWAYS keep one live shape per estate surface — schemas, contracts, and storage keys spell no version segment; change replaces the shape whole.
- ALWAYS rebuild stateful stores from declared truth on shape change — desired-state apply and whole replacement; migration logic never exists.
- ALWAYS compose existing logic before minting parallel forms; wire new logic into consumers same-pass — a file no owner reaches is a dead end.
- ALWAYS treat admitted packages as the standard library — use the deepest operator they reach; unmined capability is a hand-rolling defect.
- ALWAYS route a wanted capability DOWN-STRATA to the lowest folder that nearly holds it; surgical substrate work powers every consumer above.
- ALWAYS land a consumer need at its owning libs/ folder as the GENERAL capability — the higher-order axis it instantiates, never its literal.
- ALWAYS co-locate domain logic with its owner instead of scattering it into generic support files.
- ALWAYS judge every surface from its consumers — internalize lifecycle, routing, retry, and policy; ceremony pushed onto callers is a defect.
- ALWAYS resolve names in one hop — no forwarding shells, util shells, or rename wrappers; a single-caller helper with no meaning alone inlines.
- ALWAYS break APIs aggressively with every call site updated same-change — no shims, compat aliases, deprecation layers, or migration surfaces.
- ALWAYS treat analyzer diagnostics as architecture pressure: fix true positives, refine false positives, and never use suppressions.
- ALWAYS maintain semantic consistency in naming of files, code functionality, types, classes, and functions, USE 1-2 word values; avoid 3+
- ALWAYS use one canonical semantic name per bounded concept; arity, filters, provider, and modality live in request shape, case, or policy row.
- ALWAYS spell explicit types and name arguments only where they disambiguate; `var`, `Foo(value: value)`, and `Fin.Succ(x)` in a typed slot are noise.

## [03]-[DEPENDENCY_POLICY]

[IMPORTANT] - External libraries, manifests, and host APIs are implementation surfaces:
- ALWAYS keep C# MSBuild/NuGet manifests label-grouped by owner, cluster-sorted, with one-line maintenance comments at most.
- ALWAYS align the package touch-point set both ways: central manager row, project manifest, branch/folder README registries, owning `.api` tier.
- ALWAYS repair an orphaned touch-point member at its owner, never by removal.
- ALWAYS assume the newest stable release; pin only while incompatible, dropping it when compatibility lands (verify via `nuget`/pnpm/uv).
- ALWAYS spell Python dependency rows as bare unpinned names — workspace-root groups and member manifests alike; `uv.lock` alone fixes versions.
- ALWAYS keep a member `pyproject.toml` to identity and bare-name edges; bounds and `python_version` markers seat at the root and drop once stale.
- NEVER mint a folder-tier `.api` file duplicating or redirecting to a substrate catalogue; a folder composing a substrate package REGISTERS it.
- NEVER create thin wrappers that rename, forward, or partially expose external APIs without adding domain value.

## [04]-[FILE_ORGANIZATION]

Section separators: language comment marker + space + `---` + bracketed UPPERCASE snake label with no internal spaces + dash fill.

```md
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
- `[MODELS]`: runtime schemas, records/classes, value objects, DTOs, table/domain models, result carriers.
- `[ERRORS]`: typed error rails, tagged failures, domain failure policies.
- `[SERVICES]`: service contracts, dependency surfaces, application/service classes.
- `[OPERATIONS]`: pure transforms, effect/result pipelines, algorithms, repository operations.
- `[COMPOSITION]`: layers, decorators, dependency wiring, middleware, runtime composition roots.
- `[EXPORTS]`: named exports, `__all__`, or language-equivalent public surface declarations.

[IMPORTANT]:
- ALWAYS order: `section` -> `owner block` -> `runtime/declaration dependency` -> `semantic rank` -> `kind` -> `smaller-to-larger` -> `alphabetical`.
- ALWAYS label real operation families inside large kernels with nested subsection labels like `[VECTOR_HEAT]` or `[NORMAL_ESTIMATION]`.
- ALWAYS keep cache keys, memo tables, mutable registries, and algorithm state with the operation/kernel/runtime owner that mutates them.
- ALWAYS treat logger handles, provider handles, and dependency-backed runtime capabilities as `[SERVICES]`, not immutable anchors.
- ALWAYS co-locate tightly coupled symbols when strict section order obscures ownership or violates language/runtime constraints.
- ALWAYS treat one registry, catalog, table, or composition root the same; sort inside the owner, never flattened into top-level sections.
- ALWAYS apply smaller-to-larger only after ownership and dependency: anchors before policies, axes before models, leaf ops before orchestration.
- ALWAYS treat kind as an owner-local tiebreaker, not a new section — it ranks only among peers equal in ownership, dependency, and semantic rank.
- ALWAYS order same-owner peers public, then internal, then private — unless static construction, generated semantics, or read-before-use wins.
- ALWAYS hold owner-defined domain order: severity, lifecycle, routing, key, protocol, generated-case, table-row, provisioning-step, public API.
- ALWAYS insert a domain extension right after its closest core section; a precise label is earned by real ownership.
- ALWAYS use extension vocabulary: `[TABLES]`, `[BOUNDARIES]`, `[REPOSITORIES]`, `[GROUPS]`, `[MIDDLEWARE]`, `[INDEXES]`, `[POLICIES]`, `[ENTRY]`.
- NEVER split source-generated owners, delegate-backed enum behavior, validation partials, or operation-local state for mechanical section order.
- NEVER split resource/disposal boundaries, dispatch tables, SQL invariants, or DDL provisioning units to satisfy section order.
- NEVER seat callable row catalogs, memo tables, or DDL-dependent objects in `[CONSTANTS]`; home each in its owning later section or extension.
