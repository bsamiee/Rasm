# [CLAUDE_MANIFEST]

MUST READ: `libs/.planning/RULINGS.md` + `libs/.planning/ARCHITECTURE.md`

Rasm is in a long-term planning phase, working strictly within spec-sheets, not code files. All `libs/` spec docs are the rebuild surface: rebuilt ground-up each pass, freely and aggressively; always address cross file/folder ripples on landing.

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

[STANDARDS_ROUTING]: Use the route-owned standard for the file being edited:

| [INDEX] | [FILE_TYPE]                | [ROUTE]                        | [LOCATION_TO_USE]                | [NAMING_SCHEMA] |
| :-----: | :------------------------- | :----------------------------- | :------------------------------- | :-------------- |
|  [01]   | C# (`.cs`)                 | Docs: `docs/stacks/csharp`     | `libs/csharp` + `.cs`            | `PascalCase`    |
|  [02]   | Python (`.py`)             | Docs: `docs/stacks/python`     | `libs/python` + `.py`            | `snake_case`    |
|  [03]   | TypeScript (`.ts`, `.tsx`) | Docs: `docs/stacks/typescript` | `libs/typescript` + `.ts`/`.tsx` | `camelCase`     |
|  [04]   | Bash/sh (`.sh`, `.bash`)   | Skill: `coding-bash`           | [ANY]                            | `kebab-case`    |
|  [05]   | SQL (`.sql`)               | Skill: `coding-pg`             | [ANY]                            | `snake_case`    |
|  [06]   | Markdown (`.md`)           | Skill: `docgen`                | [ANY]                            | `kebab-case`    |
|  [07]   | Mermaid                    | Skill: `mermaid-diagramming`   | Inside `.md` and `.html` pages   | [N/A]           |

[TOOL_ROUTING]:
- ALWAYS use `ast-grep` skill on every code surface — outline before reading source, structural search over grep, rewrites, and durable rules.
- ALWAYS use `exa` MCP to start open-web search with neural discovery, the right page, repo, paper, or entity; REPLACES `WebFetch` entirely.
- ALWAYS use `search-tavily` skill on known targets — extract or crawl a site, or run a cited multi-source report.
- ALWAYS use `search-context7` skill when working on code/fences with external libraries, never guess on SDK/framework/API capabilities or implementations.
- ALWAYS use `nuget` MCP to validate the existence of a package and newest version available.
- ALWAYS use `claudeCodeDocs` MCP when working on Claude Code configs or harness questions; capabilities, memory, skills, hooks, plugins, settings.
- ALWAYS use `openaiDeveloperDocs` MCP when working on Codex configs or harness questions; capabilities, memory, skills, hooks, plugins, settings.

[CLI_ESTATE]: Reach for each tool by its own contract, never the upstream one it shadows:

| [INDEX] | [TOOL]     | [GUIDANCE]                                                                                                                  |
| :-----: | :--------- | :-------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `tree`     | `eza --tree` shim: pass `-a` or `<.planning`/`.api>` vanish; `-D` is dirs-only (`-d` errors); `-L n` overrides depth 4.     |
|  [02]   | `loc`      | `scc` wrapper sorting inside folder groups — `\| head` is never a global top-N; rank `--json` on `folder`+`file`.           |
|  [03]   | `fd`       | `--hidden` is baked in `-H` is noise; pattern is regex — `*.md` errors, take `-e md` or `-g`; `-I` admits ignored.          |
|  [04]   | `rg`       | `--smart-case --hidden`; `-s` pins case; types `docs agent config data lock`; `-U` spans `\n`; `-r`=replace, `-E`=encoding. |
|  [05]   | `ast-grep` | Never `sg`. Wrong pattern and clean tree both read zero — control-probe it; `--kind` inventories; `--json=compact` glued.   |
|  [06]   | `assay`    | Run at repo root under `uv run --no-sync`; bare `static` plans zero — pass `--folder\|--project\|--all`.                    |
|  [07]   | `fmt`      | Defaults to `--write` — pass `--check` first; markdown and C# hold no lane and skip silently.                               |
|  [08]   | `gha`      | `check` folds actionlint+zizmor+ratchet, `run` passes to `act`; `act -l -W <dir>` exits 0 on zero workflows.                |
|  [09]   | `jq`/`yq`  | `yq` is mikefarah v4 — `yq '.expr' f`, never `yq r`; `jq` needs `-r` for shell values and `[]?` on optional arrays.         |
|  [10]   | `gh`       | Non-TTY prints nothing when empty — never read the table; count through `--json <fields> \| jq length`.                     |

[LANE_GATES]: Run every gate from the repo root; a wrong cwd fabricates results instead of failing:

| [INDEX] | [LANE]       | [CHECK_INVOCATION]                                                                                                       |
| :-----: | :----------- | :----------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `csharp`     | `assay static --project <csproj>` · `dotnet build Workspace.slnx --no-restore` — serial; assay's parallel fan self-locks |
|  [02]   | `python`     | `assay static --folder <dir>` — ruff, ty, mypy, lint-imports; ty binds, mypy advises, and both lie from a foreign cwd    |
|  [03]   | `typescript` | `assay static --folder <dir>` — `biome ci` and `tsc --noEmit`; `biome lint` skips the formatter and greens falsely       |
|  [04]   | `markdown`   | `assay docs check <paths>` · `.claude/skills/docgen/scripts/prose_gate.py fix --write` — the sole markdown formatter     |
|  [05]   | `sql`        | `uv run sqlfluff lint <paths>` · `uv run squawk` — a bare `.` lints vendored fixtures inside `.venv`, honoring no ignore |
|  [06]   | `shell`      | `shellcheck <files>` · `shfmt -d <files>` — pathless `shfmt` blocks on stdin until the deadline kills it                 |
|  [07]   | `config`     | `taplo fmt --check <files>` · `yamllint <files>` — both read `~/.config`, so neither verdict reproduces off this box     |
|  [08]   | `ci`         | `gha check [PATH...]` · `gha pin` · `gha events` — actionlint exit 3 names an empty discovery, never a lint failure      |
|  [09]   | `oci`        | `hadolint <file>` · `dive --ci <image>` — each hangs or backtraces when handed no explicit argument                      |
|  [10]   | `security`   | `trivy fs --scanners vuln,secret,misconfig --skip-dirs node_modules --skip-dirs .venv .` · `gitleaks detect`             |
|  [11]   | `iac`        | `pulumi preview -C <dir>` · `pulumi about` — `pulumi whoami` mints a real ephemeral cloud account, never read-only       |
|  [12]   | `provision`  | `assay provision <verb>` — Forge service, Postgres-extension, and DuckDB/SQLite surface evidence                         |

## [02]-[IMPLEMENTATION_STANDARDS]

Universal code law: binds every language, present or future; `docs/stacks/<language>/` deepens it per stack and never weakens it.

[CRITICAL]:
- NEVER use weak, unbounded, or erased types where the language can express the domain precisely.
- NEVER use exception-style control flow in domain logic; use typed error rails and the required route's recovery patterns.
- NEVER spell absence as null, sentinel, or magic default past the boundary; absence rides an option-shaped carrier consumers unwrap.

[DENSITY] - dense: every surviving line load-bearing because one declaration carries the family; rich: the owner models its full domain:
- ALWAYS ASSUME 10X THE COMPLEXITY AND DEMANDS ON EVERY SURFACE — a naive, simple, or surface-level solution is rejected and rebuilt on sight.
- ALWAYS treat doctrine as the floor, never the ceiling — a conformant-but-weak form is a defect wherever a stronger form exists.
- ALWAYS rebuild functionality GROUND-UP with zero loss — density is the consequence of collapse, never the goal; file-size budgets do not exist.
- ALWAYS replace flat code — hand-rolled loops, branch ladders, parallel models, per-instance bodies — with folds, tables, generators, owners.
- ALWAYS model the full domain on every owner — a missing axis is a defect, not thrift, and zero current consumers never lowers the bar.
- ALWAYS land new functionality as if designed in from the start, never as tacked-on flat-code spam; extend the owner before minting a sibling.
- ALWAYS state what every collapse loses — a plural form carrying a guarantee is lawful; erasing the guarantee is a downgrade wearing density.
- ALWAYS consume every declared capability — a policy row, column, or receipt nothing reads is decorative density; add the arm or delete it.

[POLYMORPHISM] - fewer, stronger owners over many loose shapes; variants are cases inside one closed family, never sibling types:
- ALWAYS fold one polymorphic entrypoint per concern, discriminating on input shape; forward and inverse of one correspondence share one owner.
- ALWAYS collapse siblings sharing an identity regime, admission path, payload timing, or consumer; survival needs a discriminant named on site.
- ALWAYS fold repeated mutation/status/count construction into one fact stream with slot/kind metadata; the trigger is shared shape, never count.
- ALWAYS widen an owner in place to the full concept it admits NOW — the next case lands as one declaration, consumers untouched or loudly broken.
- ALWAYS close dispatch by default — a catch-all over an owned family turns a compile break into a silent pass; openness needs foreign extension.
- NEVER mint entrypoint siblings — name-suffix families, arity twins, boolean mode knobs; the discriminant must be recoverable from the value.
- NEVER guard an invalid state at each use; make it unrepresentable at construction and canonicalize at intake so consumers read one regime.

[PARAMETERIZATION] - variation lives in data or a type parameter, never in a name, a flag, or a body:
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
- ALWAYS keep typed algorithm receipts when fields carry route, status, sampling, solver, spectral, mesh, extraction, benchmark, or host evidence.

[COMPOSITION]:
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

## [03]-[DEPENDENCY_POLICY]

[IMPORTANT] - External libraries, manifests, and host APIs are implementation surfaces:
- ALWAYS keep C# MSBuild/NuGet manifests label-grouped by owner, cluster-sorted, with one-line maintenance comments at most.
- ALWAYS align the package touch-point set both ways: central manager row, project manifest, branch/folder README registries, owning `.api` tier.
- ALWAYS repair an orphaned touch-point member at its owner, never by removal.
- ALWAYS assume the newest stable release; pin only while incompatible and drop the pin when compatibility lands (verify with tools, pnpm, nuget, etc).
- ALWAYS keep root `pyproject.toml` dependencies as lean unpinned names; remove bounds/`python_version` markers if proven stale/unnecessary.
- NEVER mint a folder-tier `.api` file duplicating or redirecting to a substrate catalogue; a folder composing a substrate package REGISTERS it.
- NEVER create thin wrappers that rename, forward, or partially expose external APIs without adding domain value.

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
- ALWAYS order as `section` -> `owner block` -> `runtime/declaration dependency` -> `semantic rank` -> `kind` -> `smaller-to-larger` -> `alphabetical`.
- ALWAYS use nested algorithm subsection labels inside large kernels to identify a real operation family, such as `[VECTOR_HEAT]` or `[NORMAL_ESTIMATION]`.
- ALWAYS keep internal cache keys, memo tables, mutable registries, and algorithm state records with the operation/kernel/runtime owner that mutates them.
- ALWAYS treat logger handles, provider handles, and dependency-backed runtime capabilities as `[SERVICES]`, not immutable anchors.
- ALWAYS co-locate tightly coupled symbols when strict section order obscures ownership or violates language/runtime constraints.
- ALWAYS treat one registry, catalog, table, or composition root the same; sort inside the owner, never flattened into top-level sections.
- ALWAYS apply smaller-to-larger only after ownership and dependency: anchors before policies, axes before models, leaf ops before orchestration.
- ALWAYS treat kind as an owner-local tiebreaker, not a new section — it ranks only among peers equal in ownership, dependency, and semantic rank.
- ALWAYS order same-owner peers public, then internal, then private — unless static construction, generated semantics, or read-before-use wins.
- ALWAYS hold owner-defined domain order: severity, lifecycle, routing, key, protocol, generated-case, table-row, migration-step, public API.
- ALWAYS insert a domain extension right after its closest core section; a precise label is earned by real ownership.
- ALWAYS use proper extension vocabulary: `[TABLES]`, `[BOUNDARIES]`, `[REPOSITORIES]`, `[GROUPS]`, `[MIDDLEWARE]`, `[INDEXES]`, `[POLICIES]`, `[ENTRY]`.
- NEVER split source-generated owners, delegate-backed enum behavior, validation partials, or operation-local state for mechanical section order.
- NEVER split resource/disposal boundaries, dispatch tables, SQL invariants, or migration units to satisfy section order.
- NEVER seat callable row catalogs, memo tables, or DDL-dependent objects in `[CONSTANTS]`; home each in its owning later section or extension.
