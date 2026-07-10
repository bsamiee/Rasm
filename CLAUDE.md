# [CLAUDE_MANIFEST]

[REQUIRED]: ALWAYS READ `.editorconfig` + `Directory.Build.props` UNDERSTAND FORMAT STYLING, ERRORS, ANALYZERS, NEVER IGNORE, NEVER USE VAR, NEVER USE INCORRECT NAMESPACING FOR CSHARP; ADHERE TO ALL EDITORCONFIG RULINGS, DO NOT MAKE NONSENSE IN DESIGN DOCS, CODE FENCES, OR REAL CODE THAT WILL INSTANTLY FAIL AND GENERATE ERRORS.

BEFORE ANY ACTION IN REPO, LIST ALL ROOT-LEVEL FILES; BE AWARE OF ALL CONFIGS, TOOLING, ETC FOR ANY LANGUAGE OR SOURCE, NEVER IGNORE WHEN RELEVANT.

[COMMENT_DISCIPLINE]: Never tolerate comment spam. Maintain the section organizational style per `[09]-[FILE_ORGANIZATION]` — proper sub-section styling (no trailing dashes) and canonical section labels. Whenever any file is created or edited, aggressively refactor/prune/refine its prose and comments in the same pass (including inside code fences). Comments are RARELY larger than 1-2 lines, framed agent-first, and justified only as critical signal for maintaining/understanding code — never boilerplate or self-explanation. Width, stack, shred, runt, inlining, header-zone, and repair law for comments is global agent law; this repo adds only the `[09]-[FILE_ORGANIZATION]` section grammar and the `libs/` scope below. ALL prose, comments included, follows `docs/standards/style-guide.md`: concise, declarative, active voice, assertive, never hedging or qualifying, always concrete decisions — never ambiguity. Remove noise comments on sight; a comment earns its keep only when it serves agents working the file, never humans. THE SAME STANDARD APPLIES TO ALL `.md` PROSE WITHIN `libs/`.

Read: `README.md` + `tools/assay/README.md`

[CRITICAL]:
- The project is in a long-term planning phase, working strictly within design/spec-sheets, not code files. List all files in `libs/.planning`, and read them fully: `libs/.planning/planning-targets.md`, `libs/.planning/campaign-method.md`, `libs/.planning/README.md`, `libs/.planning/architecture.md`.
- [ALWAYS]: Load the `docgen` skill before authoring, editing, reviewing, or rewriting ANY durable markdown in this planning phase — index docs, specs, `.api` catalogs, standards, briefs, tool docs. It owns the register, the defect catalog, the file-kind templates, and the prose gate; work on durable prose without it loaded is a process defect, and every touched doc passes its gate before the turn ends.
- All main agent + sub-agents MUST approach any/all content within `libs/` with EXTREME hostility and be adversarial, always attacking code quality, naivety, and structure; files are aggressively and constantly rebuilt ground/root-up to achieve world-class, award winning folders, functionality, and code.
- Work in `libs/python/` REQUIRES a full read and UNDERSTANDING of every file within `docs/stacks/python/`, followed to the letter. The same holds for `libs/csharp/` with all files in `docs/stacks/csharp/` including the extended `docs/stacks/csharp/domain` folder files, and for `libs/typescript/` with all files in `docs/stacks/typescript/`.
- WORKING IN `.md` FILES GRANTS MAXIMUM FREEDOM TO REBUILD ALL DESIGN DOCS/FEATURES/CAPABILITIES ROOT/GROUND-UP WITH NO HESITATION, SECURING THE WORLD-CLASS/BLEEDING-EDGE CAPABILITY BAR, ALL FEATURES ADDED VIA REBUILDING, NEVER TACKING-ON FLAT CODE. Constantly find weak implementations and further opportunities to push code sophistication, density, complexity, and richness whilst collapsing total surface as much as possible, reducing LOC as well, and most importantly ULTRA-stacking all content from the `.api/` folders — the language-specific ones plus the planning-folder-specific `libs/csharp/.api/`, `libs/python/.api/`, `libs/typescript/.api/`, `libs/csharp/<folder>/.api/`, `libs/python/<folder>/.api/`, `libs/typescript/<folder>/.api/`.
- External libs/packages admitted to the central package managers and each planning folder are AGGRESSIVELY reviewed and judged: remove and replace weak selections with more advanced/bleeding-edge and powerful ones, and ADD more whenever scope/capability/functionality demands. Always centralize to `Directory.Packages.props`, `pnpm-workspace.yaml`, `pyproject.toml`. Update the respective `README.md` of each planning folder when external sources change, and the `.csproj` when the changes are in a csharp planning folder, keeping documentation aligned with new package additions and ensuring the creation of the `.api/` files of any new additions.
- The universal standard for all languages is: FULL parameterization, no hardcoded values, no fragile logic, no coupling in any way, to other folders, unusual pathing, or fragile logic. UNIVERSAL/MAXIMIALIST polymorphism, and the LEAST POSSIBLE shapes/objects/types/constants, pushing to increase density/richness/complexity over adding flat/spam code. FULL ADT, and stacking the smallest amount with the most possible, AOP (python), interface/graphing/mapping (csharp), FP+ROP+Expression code that is world-class, bleeding-edge, and award winning.

## [01]-[WORKSPACE_LAW]

Rankings, higher = better. Cost reflects actual operator spend (OpenAI is near-free under the operator's deal), not list price. Intelligence is how hard a problem the model takes unsupervised. Taste covers UI/UX, code quality, API design, and copy.

| [INDEX] | [MODEL]  | [COST] | [INTELLIGENCE] | [TASTE] |
| :-----: | :------- | :----: | :------------: | :-----: |
|  [01]   | gpt-5.5  |   9    |       8        |    5    |
|  [02]   | sonnet-5 |   5    |       4        |    6    |
|  [03]   | opus-4.8 |   4    |       7        |    7    |
|  [04]   | fable-5  |   2    |       9        |    9    |

How to apply:
- These are defaults, not limits, under standing permission to override: when a cheaper model's output misses the bar, rerun or redo the work with a smarter model without asking. Judge the output, not the price tag. Escalating costs less than shipping mediocre work.
- Never let cost block the right model for the job. Instead, exploit cheaper options to gather more information and trial approaches before moving the work to a more expensive option.
- Bulk/mechanical work (clear-spec implementation, data analysis, migrations): gpt-5.5 - it's effectively free.
- Heavy exploration, investigation, and research legs: dispatch to gpt-5.5 (`codex exec -s read-only`) before spawning Claude subagents - the transcript stays out of context and the usage is free. Work that must author or edit files dispatches the same way at `-s workspace-write`; the sandbox flag IS the modality (read/response vs write/edit) and is always set explicitly.
- Codex is a first-class worker, never a bent fallback: hand it ONE self-contained prompt (it inherits none of this conversation), let it drive its own tools to completion, and take its final message as the result - relay a read leg's report, apply a write leg's edits as delivered. Verify load-bearing claims against source before acting; never silently rewrite, re-judge, or wrap its output in extra ceremony.
- Anything user-facing (UI, copy, API design) needs taste ≥ 7.
- Reviews of plans/implementations: fable-5 or opus-4.8, optionally gpt-5.5 as an extra independent perspective. A fable agent never delegates to another fable: inline work first, and unavoidable delegation dispatches a single bounded opus (or below) sub-task, never a chain.
- Mechanics: gpt-5.5 is only reachable through the Codex CLI - `codex exec` / `codex review` (`~/.codex/config.toml` defaults to gpt-5.5 at medium reasoning).
- Load the codex skill `.claude/skills/codex/SKILL.md` whenever dispatching work to codex - delegation triggers, invocation mechanics, sandboxing, effort tiers, sessions, and review modes live there.
- Reasoning effort defaults to medium; escalate a single run with `codex exec -c model_reasoning_effort="high"` (or `--profile xhigh`) for the hardest research, review, and design legs - multi-minute latency, reserve for depth over throughput.
- Claude models (sonnet-5, opus-4.8, fable-5) run via the Agent/Workflow model parameter.
- [NEVER]: use Haiku.

Using gpt-5.5 inside workflows and subagents (the model parameter only takes Claude models, so use a wrapper):
- Spawn a thin Claude wrapper agent with `model: 'sonnet', effort: 'low'` whose prompt instructs it to write a self-contained codex prompt, run `codex exec` via Bash, and return the report (use `schema` on the wrapper to get structured output back).
- Always label these agents with a `gpt-5.5:` prefix, e.g. `{label: 'gpt-5.5:review-auth'}` - the workflow UI shows the wrapper's Claude model, so the label is the only indication the real worker is gpt-5.5.
- A short leg runs synchronously: `codex exec` prints its final message to stdout (banner and reasoning go to stderr), so the wrapper captures stdout under a tier-matched Bash timeout. A long leg exceeds one Bash call's 10-minute cap and the wrapper's own stall window, so it launches detached (a bare `&`, never `nohup`, stdout to `/dev/null`, stderr to a per-run log whose tail is the crash reason) against a `-o` report and polls by liveness across bounded calls - report present, or the codex process gone - never relaunching a live run. Inside workflows wrappers are LAUNCH-ONLY - a subagent has no legal wait (foreground sleep is blocked, background tasks never notify it, idle no-ops trip no-progress enforcement and file a false failure while codex runs on): the wrapper returns a launch receipt in seconds, the orchestrator owns time (`await new Promise(r => setTimeout(r, ms))` between harvest rounds), and a short-lived harvester agent per round promotes finished reports mechanically from disk - never relaunch a live run. Liveness is not health: alive past ~20 min with no report is WEDGED - kill it and relaunch once, a second wedge is the failure. Fan-out and file-only legs pass `--ignore-user-config` and restate the effort tier (`-c model_reasoning_effort="medium"` - config.toml is skipped wholesale, so effort resets to none while auth, the gpt-5.5 default, and skills survive) - every codex process spawns the full config MCP fleet otherwise, a `required = true` server that misses its startup handshake kills the lane at session creation (exit 1, no turn, zero JSONL, stderr holds the reason), surgical removal is `-c 'mcp_servers.<name>.enabled=false'`, and `-c mcp_servers={}` is a merge no-op that clears nothing. `--output-schema` schemas are STRICT: every property in `required` (conditional fields required-but-empty) or the API 400s `invalid_json_schema`; task/schema files are Write-tool-written at absolute paths and `test -s`-verified before launch.
- `codex exec -o <file>` writes the final message to a file (the report artifact a detached run polls); `--output-schema <schema.json>` constrains that final message to a JSON Schema when the wrapper must return typed results.
- Workflow token budgets only count Claude tokens; codex work is free and invisible to `budget.spent()`.

[WORKFLOW_ENGINE]:
- Workflows launch by `scriptPath` (the absolute path to `.claude/workflows/<name>.js`), never by registry `name`: name-resolution serves a session-start snapshot and silently runs a stale contract after any in-session workflow edit; `scriptPath` reads the current disk file. After editing a workflow, verify the launch summary echoes the edited contract before trusting the run shape.
- `ls .claude/workflows/` enumerates the standing engine roster and each script's `meta` block states its own contract — the roster is never restated in prose. `rebuild.js` is the standing hostile rebuild engine over any mix of `libs/` planning targets; `/prime` grounds a planning session before campaign entry.
- A campaign needing a shape the roster lacks gets a one-off workflow authored via `.claude/skills/workflow-creator` and deleted after landing.
- Every workflow agent WRITES and is ultra-adversarial; the discovery/critique/red-team/verify role law, the two naivety axes, and the collapse-floor freedom are sealed in `libs/.planning/campaign-method.md`.
- Workflow runs resume only in the launching session: capture a run ledger (run ID, scriptPath, args, resume command) at every launch; never edit a launched script while its run is resumable; a campaign brief travels as a PATH, so editing the brief means a fresh run, never a resume.

[IMPORTANT]:
- [ALWAYS]: Hold both forces of the DEPTH-OVER-SURFACE law at once, in every folder, file, and fence. INTERIOR MAXIMALISM: every domain owner models its full domain — every attribute, sub-kind, state, relationship, invariant, and operation the concept carries, every admitted package mined to modern operator depth — no gaps, no underutilized capability, no thin slices. EXTERIOR FOCUS: capability reaches consumers through FEW dense unified entry points — one polymorphic entry per rail discriminating on input shape (single|batch|stream absorbed by input detection, forward and inverse directions on one surface wherever the domain admits an inverse), with policy resolution, routing, retries, telemetry, and lifecycle internalized so a consumer composes outcomes and never orchestrates internals, imports dozens of symbols, or learns provider nuance. Variation lives in input shape, policy values, and table rows — never in parallel exports, knob/ceremony spam, or modality-named siblings. The surface narrows by ABSORPTION, never by omission: flexibility and capability are never reduced to make the surface small.
- [ALWAYS]: Aggressively rebuild code and planning docs GROUND/ROOT-UP, tear apart any existing patterns to achieve the optimized/advanced code surface density without losing functionality; new functionality is always made as if it was there from the start, never as tacked-on/flat-code spam.
- [ALWAYS]: Create monorepo code as polymorphic, agnostic, and universal by default, ALWAYS PARAMETERIZE INPUTS/OUTPUTS + INGRESS/EGRESS.
- [ALWAYS]: Place every C# package on the canonical strata (KERNEL -> AEC-DOMAIN -> APP-PLATFORM -> HOST-BOUNDARY -> APP), depending strictly upward; build a host-neutral owner only where a non-Rhino runtime consumes the contract ("universal" is never host-free C#), and keep geometry/mesh/IFC to one owner per runtime meeting at the wire. The package roster and full hierarchy law are `libs/.planning/architecture.md`.
- [ALWAYS]: Identify canonical object shapes, field names, semantics, and receipts that scale across packages, tools, apps, plugins, sidecars, services, and web consumers.
- [ALWAYS]: Maintain semantic consistency in naming patterns of files, code functionality, types, classes, and functions — one consistent language/approach regardless of language, with all code behavior following a consistent naming approach.
- [ALWAYS]: Use one canonical semantic name per bounded concept; arity, filters, provider, and modality live in request shape, case, policy row, or boundary adapter, not parallel names.
- [ALWAYS]: Extend the canonical owner before adding rails, public surfaces, wrappers, commands, flags, provider selectors, schemas, models, helpers, or files.
- [ALWAYS]: Treat planned future consumers as real design pressure. Zero current consumers never reduces the capability bar. ALWAYS ASSUME 5X THE COMPLEXITY/DEMANDS ON CODE, NEVER SETTLING FOR SIMPLE/NAIVE SOLUTIONS, NEVER TOLERATE SURFACE LEVEL FUNCTIONALITY. A class carrying 4 fields for a concept that admits 12+ is extended to the full concept in anticipation of all the needs NOW, not later; object proliferation is never the answer.
- [ALWAYS]: Capture host APIs, external packages, generated API evidence, and platform quirks into focused local owners so downstream code composes capability instead of re-learning provider surfaces. Reference/read files within `.api/` folders, use `Context7`, `nuget mcp`, `exa`, `tavily`, and other MCP's for current/fresh information; NEVER RELY ON TRAINING DATA, NEVER TRUST TRAINING DATA, ALWAYS USE NEWEST/CURRENT INFO FROM REAL SOURCES.
- [ALWAYS]: Keep boundary mapping at the edge; internal code uses canonical names and shapes.
- [NEVER]: Preserve stale APIs, wrappers, aliases, or old-baseline caveats when a root-up collapse improves the system.
- [NEVER]: Split one concern across parallel objects, services, error rails, command families, or compatibility shims.
- [NEVER]: Create operation families such as `Get`, `GetMany`, `GetBy<Key>`, `List`, or `Search` for one concept when one polymorphic operation can discriminate by input value.
- [ALWAYS]: Repo-root residency is a closed allowlist owned by `tests/python/_testkit/test_policy.py`: a deliberate new root file adds its allowlist row in the same change; a red root gate resolves by routing tool output under `.cache/`/`.artifacts/` or by reviewed allowlist amendment — never by deleting a legitimate root file to silence the gate.

## [02]-[REQUIRED_STANDARDS]

Use the route-owned standard for the file being edited:

| [INDEX] | [FILE_TYPE]                | [ROUTE]                  |
| :-----: | :------------------------- | :----------------------- |
|  [01]   | TypeScript (`.ts`, `.tsx`) | `docs/stacks/typescript` |
|  [02]   | C# production (`.cs`)      | `docs/stacks/csharp`     |
|  [03]   | Python (`.py`)             | `docs/stacks/python`     |
|  [04]   | Bash/sh (`.sh`, `.bash`)   | `coding-bash`            |
|  [05]   | SQL (`.sql`)               | `coding-pg`              |
|  [06]   | Durable markdown (`.md`)   | `docgen`                 |
|  [07]   | Mermaid fences             | `mermaid-diagramming`    |
|  [08]   | HTML artifacts (`.html`)   | `html-studio`            |

- Each `docs/stacks/<language>` directory is the route-owned production standard for its language: source composes every root page of the directory (`ls docs/stacks/<language>` is the page roster). Specialized C# domains route through `docs/stacks/csharp/domain/README.md`; numerical and scientific Python routes through `docs/stacks/python/algorithms.md` plus the root Python doctrine index.

[SKILL_CONCERT]: The visualization skills compose; each owns one medium and hands off at the seam.
- Interviewing owns elicitation and its schema instances; a comparative, scored, or spatial ruling renders through the html-studio type rows, and an interactive carrier (quiz, wargame board, direction picker) runs served through the html-studio return channel so user verdicts come back as submission receipts.
- html-studio owns single-file interactive HTML pages including their inline-SVG diagrams; mermaid-diagramming owns mermaid fences inside markdown; the dataviz skill owns chart-mark and chart-palette decisions in any medium.
- Durable artifact pages home at `docs/atlas/` as `<kind>.<scope>[.<slug>].html`; session-scoped pages stay in scratch space and never commit.

## [03]-[NAMING_SCHEMA]

Folders, namespaces, and source files follow each branch language's standard casing; `.planning` design pages are `lowercase.md`.

| [INDEX] | [BRANCH]          | [FOLDER_AND_SOURCE] |
| :-----: | :---------------- | :------------------ |
|  [01]   | `libs/csharp`     | `PascalCase`        |
|  [02]   | `libs/python`     | `snake_case`        |
|  [03]   | `libs/typescript` | `camelCase`         |

## [04]-[DEPENDENCY_POLICY]

[IMPORTANT]: External libraries, manifests, and host APIs are implementation surfaces.
- [ALWAYS]: Treat dependencies declared in `pyproject.toml`, `pnpm-workspace.yaml`, `Directory.Packages.props`, project files, lockfiles, and equivalent manifests as first-class material.
- [ALWAYS]: Mine admitted packages to their full useful capability before writing local kernels.
- [ALWAYS]: Prefer ecosystem libraries that already own the domain concern over lower-level reinvention.
- [ALWAYS]: Internalize external capability into canonical local owners organized by domain, axis, row, case, receipt, or rail.
- [ALWAYS]: Keep central package/version/tool ownership centralized in the one owning manifest or tool configuration — no per-package `pyproject.toml`, `package.json`, or `*.props`; assume the newest stable release and pin a package only when it is not yet compatible, removing the pin when compatibility lands.
- [ALWAYS]: Keep Python dependencies in root `pyproject.toml` as lean unpinned package names by default; add bounds or `python_version` markers only when resolver evidence requires them, prefer the newest viable release, remove constraints as compatibility lands, and keep wheel/floor/gate rationale out of Python docs, design docs, `.api` files, and comments.
- [ALWAYS]: Keep C# MSBuild, NuGet, and `.csproj` manifests label-grouped by owner, sorted within coherent clusters, and limited to one-line maintenance comments.
- [ALWAYS]: Put shared C# substrate API catalogues under `libs/csharp/.api/`; package `.api/` folders carry domain catalogues and folder-specific overlays.
- [NEVER]: Hand-roll functionality provided by admitted dependencies.
- [NEVER]: Create thin wrappers that rename, forward, or partially expose external APIs without adding domain value.
- [NEVER]: Encode package versions, provider caveats, or command catalogs outside the owning manifest, package charter, README, or tool owner.

## [05]-[IMPLEMENTATION_CONSTRAINTS]

[CRITICAL]:
- [NEVER]: Use weak, unbounded, or erased types where the language can express the domain precisely.
- [NEVER]: Use exception-style control flow in domain logic; use typed error rails and the required route's recovery patterns.
- [NEVER]: Use imperative branching when a bounded vocabulary, dispatch table, generated switch, match, fold, or monadic rail can own the variation.
- [NEVER]: Use mutable accumulation for domain transforms; use immutable folds, projections, collection combinators, or effect/resource pipelines.
- [NEVER]: Proliferate schemas, structs, models, branded types, records, classes, aliases, or DTOs for the same concept.
- [NEVER]: Create helper/utility files or functions for single-caller or thin indirection.
- [NEVER]: Extract code to new files to reduce LOC. Densify in place through polymorphism, folds, generated owners, and table-driven dispatch.
- [NEVER]: Delete functionality to satisfy a density or LOC signal. Preserve capability through denser owners.
- [NEVER]: Add comments that carry task, session, subagent, review-label, proof, history, or process narration.

[IMPORTANT]:
- [ALWAYS]: Frame all comments and prose within code files, code-fences and docs in general to be AGENT FIRST/ONLY/FOCUSED, the only useful comments/prose are those that IMPLICITLY guide agentic coding/management/maintenance.
- [ALWAYS]: Collapse related variants into one polymorphic surface before adding entrypoints.
- [ALWAYS]: Drive logic with data, bounded vocabularies, discriminants, table rows, and reusable projections.
- [ALWAYS]: Co-locate domain logic with its owner instead of scattering it into generic support files.
- [ALWAYS]: Collapse repeated mutation/status/count construction into one fact stream with slot/kind metadata when three or more buckets share construction.
- [ALWAYS]: Keep typed algorithm receipts when fields carry route, status, sampling, solver, spectral, mesh, extraction, benchmark, or host evidence.
- [ALWAYS]: Treat analyzer diagnostics as architecture pressure: fix true positives, refine false positives, and avoid suppressions that add ceremony without improving correctness.

## [06]-[BEHAVIOR]

[IMPORTANT]:
- [ALWAYS]: Tools over internal knowledge: read files, search code, verify assumptions through source, manifests, docs, and tool output.
- [ALWAYS]: Parallelize independent searches, reads, and checks.
- [ALWAYS]: Use bounded subagents for independent exploration, research, verification, and disjoint implementation.

## [07]-[OWNER_ROUTING]

[IMPORTANT]:
- [ALWAYS]: Resolve external library, framework, SDK, or host-API usage through `Context7` before internalizing into a canonical owner: `Context7` also indexes this repo's own packages, so resolve internal API shape through it before opening source, while `uv run python -m tools.assay api` answers which members verifiably exist locally; verified-local wins on conflict. The web/docs research selection law is the user-global doctrine, not restated here.
- [ALWAYS]: Dependency graph facts live in manifests, package-manager configuration, lockfiles, project files, and the tool owner that consumes them.
- [ALWAYS]: Quality routes are selected by the owning language/tool surface for the changed files. Root policy owns intent, not command catalogs.
- [ALWAYS]: Keep static analysis, tests, runtime scenarios, metadata lookup, formatting, restore, and generated-contract checks orthogonal.
- [ALWAYS]: For docs-only, catalog-only, read-only, declaration-order, move-only, source-comment-only, docstring-only, XML-doc-only, and TSDoc-only work, use text, path, table, link, owner, and preservation checks unless the user requests an executable quality rail.
- [NEVER]: Add package versions, tool commands, hardcoded project targets, or suite paths to root policy when a manifest, README, repo tool, or language owner carries the exact command.
- [ALWAYS]: LSP owns live navigation and post-edit diagnostics over local source; the language-server plugins install from the Forge-owned `forge-lsp` marketplace (`Parametric_Forge/.claude/lsp-marketplace`).
- [ALWAYS]: Invoke the repo operator as `uv run python -m tools.assay ...`; bare `assay ...` is only valid when `command -v assay` proves a local wrapper exists.
- [ALWAYS]: `uv run python -m tools.assay api` owns external-artifact decompile/reflection over host DLLs, NuGet packages, installed Python distributions, and `node_modules` declarations.
- [ALWAYS]: Route live NuGet feed intelligence through the `nuget` MCP; `assay api` answers which members verifiably exist in the restored assembly, and verified-local wins on conflict. Apply a version change by hand-editing the grouped `Directory.Packages.props` (never `dotnet add`), confirm with `dotnet restore`/`dotnet nuget why`, and drive folder-wide modernization through the `survey` workflow; the standalone `nuget.commandline` CLI is unused.
- [ALWAYS]: Treat `Rasm.Bim` as the sole IFC semantic authority: C# owns the `BimModel`/`BimWire`/`ElementSet`/`IfcSemanticModel` graph (GeometryGym, in-process). The `ifc-bim` skill and `ifc` MCP own only live read-only inspection, GLB tessellation (`IfcConvert` keyed by the `XxHash128` content key, cache-checked against the `Rasm.Persistence` artifact index), and the IDS oracle (`ifctester` → `IdsVerdict` rows feeding `IdsAudit.Reconcile`); they never re-author the semantic model, re-implement geometry the kernel owns, or write Psets/Qtos as the system of record.
- [ALWAYS]: Run inherited local CodeRabbit -> Greptile review only when the user requests it or repo policy explicitly requires it for the current work, never when the user disables review. When review is active, run it only after the owner-scoped `uv run python -m tools.assay` gate passes for the changed files; PR-level review and automation route through the repo `gh`/`mcp__github__*` owners. Reviewer depth is repo-owned configuration — `.coderabbit.yaml` and `.greptile/` carry the tone, scope maps, exclusions, and doctrine-derived guidance; tune review behavior there, never in docs or global settings, and never duplicate their content into prose.
- [ALWAYS]: `uv run python -m tools.assay code` owns structural/pattern search over ast-grep metavariables, tree-sitter queries, and CI artifacts; prefer LSP for plain single-symbol navigation.
- [ALWAYS]: `uv run python -m tools.assay static/test/bridge/package` own gating quality rails and mutation routes. LSP is read-only.
- [ALWAYS]: Read `tests/README.md` before touching any testing surface — lane vocabulary, proof grades, the witness mandate, artifact routing, and gate ownership live there and are never restated elsewhere.
- [ALWAYS]: `uv run python -m tools.assay provision` owns Rasm campaign provisioning through sanitized `ProvisionRun` evidence; direct `forge-provision`, `forge-scientific-env`, Docker/Compose, direct database shells, cleanup, and diagnostic JSON calls are Forge-level debugging, not Rasm campaign surfaces.
- [ALWAYS]: Treat bridge proof as `EvidenceCertificate` plus reviewed `ReferenceEvidence`; MCP exploration can promote invariants into scenarios, but never substitutes for certificate-backed verify.

## [08]-[DOCUMENTATION_AND_OUTPUT]

[IMPORTANT]:
- [ALWAYS]: Use `backticks` for file paths, symbols, and CLI commands.
- [ALWAYS]: Keep responses actionable and lead with what changed.
- [ALWAYS]: Treat durable docs, prompts, standards, skills, examples, and templates as agent-facing declarative law.
- [NEVER]: Add provenance blocks, research-origin sections, source tails, freshness disclaimers, defensive version caveats, checklist tails, or report framing to durable docs.
- [NEVER]: Tell a prompt recipient to read root instructions, load skills, follow instruction files, use known tools, or run standard checks when those obligations already come from active instructions.
- [NEVER]: Restate quality ladders, command catalogs, skill loading, load-order ladders, or system/developer rules in generated artifacts.

[CRITICAL]: Plans are decision-complete blueprints. Include context, critical files, implementation approach, acceptance signals, and assumptions only when they change execution. Do not include workflow narration, alternatives considered, command catalogs, or boilerplate closure.

## [09]-[FILE_ORGANIZATION]

[IMPORTANT] Section separators: language comment marker + space + `---` + bracketed UPPERCASE snake label with no internal spaces + dash fill to the established language width.

```typescript
// --- [TYPES] ---------------------------------------------------------------------------

// --- [SUBSECTION]
```

```python
# --- [CONSTANTS] ------------------------------------------------------------------------

# --- [SUBSECTION]
```

```csharp
// --- [SERVICES] ------------------------------------------------------------------------

// --- [SUBSECTION]
```

Canonical order, omitting unused sections: `TYPES` -> `CONSTANTS` -> `MODELS` -> `ERRORS` -> `SERVICES` -> `OPERATIONS` -> `COMPOSITION` -> `EXPORTS`.

`[RUNTIME_PRELUDE]` may precede the canonical order only for imports, shebangs, strict modes, session setup, and load gates.

- `[TYPES]`: type aliases, inferred types, protocols/interfaces, enums, discriminated unions, generated algebraic owners, value-object declarations.
- `[CONSTANTS]`: dependency-free immutable anchors, caps, suffixes, primitive policies, schedules, and static literals.
- `[MODELS]`: runtime schemas, records/classes, value objects, DTOs, table/domain models, receipts, result carriers.
- `[ERRORS]`: typed error rails, tagged failures, domain failure policies.
- `[SERVICES]`: service contracts, dependency surfaces, application/service classes.
- `[OPERATIONS]`: pure transforms, effect/result pipelines, algorithms, repository operations.
- `[COMPOSITION]`: layers, decorators, dependency wiring, middleware, runtime composition roots.
- `[EXPORTS]`: named exports, `__all__`, or language-equivalent public surface declarations.

[IMPORTANT]:
- [ALWAYS]: Apply ordering as `section` -> `owner block` -> `runtime/declaration dependency` -> `semantic rank` -> `kind` -> `smaller-to-larger` -> `alphabetical`.
- [ALWAYS]: Prefer concept discovery order from stable declarations to composition: vocabulary, constants, models, failures, services, operations, wiring, exports.
- [ALWAYS]: Treat one generated type, smart enum, value object, schema/model family, wire model family, kernel, registry, catalog, table, dispatcher, query family, or composition root as an owner block; sort inside the owner instead of flattening its members into unrelated top-level sections.
- [ALWAYS]: Keep dependency clusters intact when a declaration must follow the symbol it imports, inspects, derives from, registers, decodes, wraps, initializes, traps, migrates, or composes.
- [ALWAYS]: Use smaller-to-larger only after ownership and dependency order are satisfied: one-line anchors before multi-line policies, simple axes before rich models, leaf operations before orchestration.
- [ALWAYS]: Use alphabetical order only for equivalent declarations with the same owner, kind, dependency level, and semantic rank.
- [ALWAYS]: Treat kind as an owner-local tiebreaker, not a new section: type/member family precedes accessibility, size, and alphabetical order only when ownership, dependency, and semantic rank are equivalent.
- [ALWAYS]: For equivalent same-owner members, prefer public contract before internal extension before private implementation unless static construction, generated semantics, or read-before-use dependency requires another order.
- [ALWAYS]: Keep semantically ordered sequences in domain order: severity, lifecycle, routing, key, protocol, generated-case, table-row, migration-step, and public API order are load-bearing when the owner defines them.
- [ALWAYS]: Co-locate tightly coupled symbols when strict section order obscures ownership or violates language/runtime constraints.
- [ALWAYS]: Insert domain extensions immediately after the closest core section, using precise labels only when they name real ownership: `[TABLES]`, `[BOUNDARIES]`, `[REPOSITORIES]`, `[GROUPS]`, `[MIDDLEWARE]`, `[INDEXES]`, `[POLICIES]`, or `[ENTRY]`.
- [ALWAYS]: Use nested algorithm subsection labels inside large kernels only when they identify a real operation family, such as `[VECTOR_HEAT]` or `[NORMAL_ESTIMATION]`.
- [ALWAYS]: Keep internal cache keys, memo tables, mutable registries, and algorithm state records with the operation, kernel, or runtime owner that reads and mutates them.
- [ALWAYS]: Treat logger handles, provider handles, and dependency-backed runtime capabilities as `[SERVICES]`, not immutable anchors.
- [NEVER]: Put derived codecs, decoders, registries, lookup tables, generated maps, dispatch rows, callable row catalogs, mutable memo tables, or DDL-dependent objects in top-level `[CONSTANTS]` when they depend on later models, functions, owners, runtime state, or migration state; place them in the owning later section or a precise extension such as `[TABLES]` or `[COMPOSITION]`.
- [NEVER]: Split source-generated owners, delegate-backed enum behavior, validation partials, private operation-local state, resource/disposal boundaries, dispatch tables, SQL invariants, or migration units to satisfy mechanical section order.
- [NEVER]: Rename recurring categories per file; use canonical labels unless a domain extension is materially clearer.
- [NEVER]: Use alias or drift labels that merely rename core categories or hide complexity: `SCHEMA`, `FUNCTIONS`, `LAYERS`, `IMPORTS`, `INTERFACES`, `ENUMS`, `DTO`, `QUERIES`, `HELPERS`, `UTILS`, `COMMON`, `MISC`.

Language overlays refine the canonical order by runtime semantics:
- C#: `[Union]`, `[SmartEnum]`, `[ValueObject]`, generated case families, static entries, delegate partials, validation partials, factories, and projections stay inside the declaring owner block. Preserve generated-case and smart-enum semantic order, with one generated case or static entry per physical declaration line unless a generator or runtime contract requires grouping. Static construction order inside a type is semantic when later fields derive from earlier fields. Static kernels, projectors, acceptors, and extension folds are `[OPERATIONS]` unless they own an actual dependency or service boundary. Inside a section, prefer attributes/delegates/marker types, enums/smart enums, readonly structs/records/value objects, records/classes/services, then owner-local private types when all earlier ordering constraints are equal. Inside a C# owner block, prefer generated/static dependency entries, fields/state, constructors/factories, properties, public operations, explicit boundary adapters, internal operations, then private kernels/implementation details.
- Python: imports, `TYPE_CHECKING`, and import-time gates precede ordinary sections. Runtime decoders, encoders, registries, and tables follow the models/functions they inspect because module-level assignments execute immediately and runtime annotation consumers such as `msgspec` and `beartype` resolve real objects. `Annotated` validator functions may use `[BOUNDARIES]` between immutable constants and dependent aliases when the aliases must reference the real validator object.
- TypeScript: side-effect/value imports preserve runtime order, and `import type`/`export type` stay explicit. Runtime schemas/classes are `[MODELS]`, `Effect.Service` owners are `[SERVICES]`, `Layer`/runtime wiring is `[COMPOSITION]`, and catalog or registry rows that reference functions/classes stay after their referenced owners.
- Bash: shebang, ShellCheck directives, `set`/`shopt`, and environment/path gates are `[RUNTIME_PRELUDE]`; `readonly` values are `[CONSTANTS]`; `declare -Ar` maps are `[TABLES]`; traps, dispatch, source guards, and `_main` are late `[COMPOSITION]` or `[ENTRY]`.
- PostgreSQL: extensions, schemas, and search-path guards are `[RUNTIME_PRELUDE]`; domains and types are `[TYPES]`; tables, constraints, generated columns, and partitions are `[MODELS]`; functions split by service boundary or query operation; indexes, triggers, row-level security, and policies are `[COMPOSITION]`; grants and comments are late `[EXPORTS]`.
- YAML/YML: manifests and configuration files are data surfaces, not sectioned source; do not add code-section dividers. Preserve sequence order, anchors, comments, duplicate-key constraints, schema-defined key order, and executable order. Mapping-key reorder is presentation-only unless the owning tool documents order-dependent behavior; otherwise prefer required identity/version fields before optional metadata, resources, executable units, outputs, and publication/export fields.
