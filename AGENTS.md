# [ROOT_AGENTS]

## [1]-[LOAD_ORDER]

[REQUIRED]: Read and follow `CLAUDE.md` before this file.

## [2]-[NAVIGATION]

Use repository-native discovery before broad scans:
- File discovery: `fd`.
- Exact text search: `rg`.
- Structural search: `uv run python -m tools.assay code ...` when patterns use ast-grep metavariables, tree-sitter queries, CI artifacts, or repo structural rules.
- Direct `ast-grep` is fallback only when the Assay code rail lacks the needed surface.

Read full target files before editing. Read minimal surrounding files needed to prove ownership, existing patterns, and route conflicts.
For declaration-order passes, preserve generated semantic/key bands; split grouped entries only when grouping obscures ownership, and keep compact generated rows when they are the clearer owner-local table.

## [3]-[TRUST_AND_PRESERVATION]

Skills are execution aids and mining input, not durable documentation authority. Promote portable rules through `docs/`, source, manifests, generated contracts, tool owners, or trusted instruction overlays after current behavior is verified.

Hidden and generated reservoirs have explicit roles:
- `.planning/` drives planned library transcription when a package charter makes it the owner.
- `.api/` is generated API evidence and catalog truth, not handwritten source.
- `docs/stacks/csharp/.reports/` is mining material and region evidence for stack-page maintenance only; visible stack pages carry durable doctrine.

Before finalizing non-trivial repository work, classify observed agent mistakes by owner: machine default, repo root policy, subtree overlay, source or documentation owner, tool README, or confidence gap. Refine an existing rule first; add a new rule only for a repeated mistake or a single high-risk miss such as wrong owner routing, destructive command risk, fake confirmation, unsupported claims, or code-quality regression. Do not copy session narration, report frames, memory notes, or research summaries into active instructions.

Quality cadence is phase-gated, not edit-gated. Do not run `dotnet build`, repo quality commands, formatters, analyzers, or tests after ordinary edits, markdown changes, or one-off compiler fixes. Batch implementation first, then run at most one narrow owner-scoped proof at the planned gate unless the user explicitly asks for more. If proof fails, patch related diagnostics as one batch and rerun once; ask before entering any longer loop. Memory, skills, rollout summaries, and old command notes cannot override this rule.

## [4]-[ENGINEERING_CONTRACT]

Extend the canonical owner before adding a rail, object, helper, wrapper, command, confidence path, document body, or public surface; when the owner is local, use the nearest overlay, source file, standard, or tool README that owns the concern.

Plans, documentation, and implementation target the newest objectively stronger language, platform, library, feature, tool, and architectural standard. Current source, manifests, pinned versions, older patterns, partial adoption, and compatibility surfaces are inputs and replacement targets, not baseline ceilings.

Minimize shape count, not capability. Preserve behavior by deepening the canonical owner with cases, rows, folds, projections, typed receipts, scenarios, or boundary adapters; do not delete capability, split helper files, or add shallow sibling surfaces to make the system look simpler.

Library owners internalize the full admitted capability of their platform, host APIs, generated evidence, and route-owned packages behind focused surfaces. Limited entry count never means limited capability. Future app, plugin, sidecar, service, and web consumers compose from these owners instead of re-learning raw provider APIs, host quirks, lifecycle rules, wire shapes, and failure handling.

New library or package folders begin with a planning campaign before production source when the concern is broad, foundational, or consumer-facing. The campaign makes infra truth honest, captures manifests/lockfiles/tool pins, extracts API catalogs through repo-owned evidence rails, runs research/gap/adversarial/deep-read passes before authoring, enumerates isolated and in-concert capability across modalities, then collapses surviving capability into owner ledgers, row/case/policy axes, and decision-complete planning pages. Zero consumers never lowers ambition; it requires full-capability design. Package-local proof ladders, admissions tables, build orders, and file processes stay in that folder's charter, not root policy.

All tooling, tests, docs, and libraries discover owners through manifests, configured roots, package graphs, route maps, and tool catalog rows. Current paths are inputs, never reusable doctrine.

Every repo tool must route generated storage, caches, benchmark output, mutation workdirs, coverage files, snapshots, and scratch artifacts through the owning language/tool configuration or the owning repo tool surface. Do not rely on ambient CLI defaults or gitignore-only tolerance for root litter; configure the tool in `pyproject.toml`, `Directory.Build.props`, tool manifests, test conftests, or the canonical tool engine so outputs land under `.cache`, `.artifacts`, or another owner-declared path.

## [5]-[MONOREPO_TOPOLOGY]

Rasm is a RhinoWIP, GH2, and product-neutral AEC/design-geometry workspace. Interpret library, tool, app, service, web, Python, TypeScript, and C# work through that domain frame before choosing shape: reusable capability lands in the deepest owner that can absorb it, while product shells bind intent, host edges, and output.

AEC pressure informs geometry, topology, units, tolerances, host documents, object graphs, exchange, visualization, compute, persistence, and evidence; it never collapses runtime, UI, compute, persistence, Python, TypeScript, Assay, or Bridge into generic app/plugin code.

The C# library split is owner law, not folder trivia:
- `libs/csharp/Rasm` is the RhinoCommon-aware geometry and numeric kernel.
- `libs/csharp/Rasm.Rhino` owns in-process RhinoCommon command, document, UI, camera, file, block, and event boundaries.
- `libs/csharp/Rasm.Grasshopper` owns the GH2 boundary library and GH2 UI intent adapter.
- `libs/csharp/Rasm.AppHost` owns the runtime spine, host profiles, lifecycle, time, configuration, composition, resources, telemetry, health, support, outbound resilience, and suite ports.
- `libs/csharp/Rasm.AppUi` owns product UI rails over host surfaces, commands, live data, tables, inspectors, charts, visuals, theme, typography, assets, dialogs, input, motion, accessibility, localization, and evidence.
- `libs/csharp/Rasm.Compute` owns measured execution, intent/substrate selection, tensors, model lanes, remote wire vocabulary, staging, scheduling, progress, units, receipts, and benchmark claims.
- `libs/csharp/Rasm.Persistence` owns durable state, store profiles, data lanes, schema/query rails, native SQLite truth, snapshots, caches, sync/collaboration, redaction, and retention.

Host lifecycle remains outside libraries. Rhino launch, endpoint discovery, package staging, bridge doctor/check/verify/quit/redeploy, cargo, spool, scenario evidence, and host-bound runtime facts belong to `tools/rhino-bridge` and the Assay bridge/package rails that consume it.

## [6]-[TOOL_OWNERS]

Assay command truth lives in `tools/assay/composition/registry.py`, result envelopes in `tools/assay/core/model.py`, and status projection in `tools/assay/core/status.py`. Do not infer commands from old aliases, stale plans, stderr habits, or direct tool muscle memory.

Normal Assay invocations emit one stdout `Envelope`; automation emits NDJSON. Parse `report.detail`, `report.results`, `report.artifacts`, `error`, and `error_context` as the result channel. Stderr is transport noise unless the envelope says otherwise.

New Assay capability normally lands as a catalog descriptor row plus one rail/body under an existing claim. A new verb requires a named consumer and a distinct evidence contract; otherwise collapse into the nearest existing verb.

Monorepo routing comes from manifests, project graph closure, trigger files, explicit `AssayHostBound`, package slugs, route maps, and catalog rows. Avoid path-name heuristics and single-project assumptions.

Host-bound work is local by contract. `ASSAY_EXEC_TARGET` moves admitted subprocess execution only; routing, leases, package staging, bridge/API discovery, and live Rhino verification still require the owning local or shared state.

Artifact paths emitted by Assay envelopes and bridge `reportDir` are authoritative. Inspect them before assuming layouts; root scratch output is a defect unless routed through the owning store, scope, or state file.

Machine-level scientific and provisioning tools live in `Parametric_Forge`. Use `forge-scientific-env` for native Python source-build probes and `rasm-spike-stack` for disposable PG18/Timescale/ParadeDB spike services under `.artifacts/spikes/provisioning`. Rasm owns the manifest, lockfile, `.api` catalogues, and assay evidence that consume those tools.

## [7]-[DOCUMENTATION]

Route README, ADR, architecture, roadmap, test strategy, API, reference, code documentation, support matrix, how-to, runbook, contributing, tutorial, onboarding, and instruction-file work through `docs/standards/README.md`.

Keep generated documentation, prompts, skills, standards, examples, templates, and reusable guidance project-agnostic by default. Do not mention Rasm, repository-specific paths, local commands, local package names, project functions, concrete source files, or project-only docs unless the target file explicitly exists to describe this repository's own usage, routing, or implementation. Generic examples use neutral names and abstract shapes that transfer to any project.

For docs under `docs/`, use placeholder values in reusable, generic, standard, template, and example content: `<folder>`, `<file>`, `<surface>`, `<command>`, `<contract>`, `YYYY-MM-DD`, `HH:MM`, `NNNN`, `M<N>`, `ADR-NNNN`, and equivalent neutral forms. In reusable code examples, use language-valid neutral identifiers for code structure and placeholder literals only where the language accepts literal values; accepted families include `Shape`/`RefinedShape`, `Variant`/`PRIMARY`, `Field`/`KEY`, `Row`/`ROW_A`, and `"<value-a>"`/`"<result-a>"`.

Use concrete repository names, paths, functions, commands, versions, dates, IDs, or package facts only when the document's job is to describe that exact source-backed repository surface.

Future-facing standards, plans, and target designs do not inherit current drift; remove stale paths, stale commands, compatibility prose, old-baseline caveats, partial-adoption apologies, and invented routes instead of preserving them.

Durable docs, prompts, standards, skills, examples, and reusable templates are agent-facing declarative law, not reports, walkthroughs, origin logs, or checklist tails.

Tables are load-bearing only when headers make each cell meaningful in its row and column. Long cells, repeated columns, and tautological values move to prose, cards, or records.
