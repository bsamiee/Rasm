# [ROOT_AGENTS]

## [01]-[LOAD_ORDER]

[REQUIRED]: Read and follow `CLAUDE.md` before this file.

## [02]-[NAVIGATION]

Structural search routes through `uv run python -m tools.assay code ...` when patterns use ast-grep metavariables, tree-sitter queries, CI artifacts, or repo structural rules; direct `ast-grep` is fallback only when the Assay code rail lacks the needed surface.

Read every target file end-to-end before editing. For planning, catalogue, doctrine, and standards work, inventory hidden owner trees with `fd -H`; read the owner README first, then the target `.planning/`, `.api`, or `docs/stacks/<language>/` pages in routing order. Search hits, memory, screenshots, workflow summaries, partial excerpts, and report dashboards are not substitutes for current file bytes.

After editing a planning, catalogue, doctrine, standards, or instruction file, reread the full changed file before final response. Catch malformed headings, broken tables, unbalanced fences, duplicated owner regions, stale route links, and skipped target sections from actual bytes.
For declaration-order passes, preserve generated semantic/key bands; split grouped entries only when grouping obscures ownership, and keep compact generated rows when they are the clearer owner-local table.

## [03]-[QUALITY_VALIDATION]

Quality cadence is gated at the planned milestone, not after every edit: batch implementation, then run at most one narrow owner-scoped `uv run python -m tools.assay` proof for the changed files. Memory, skills, rollout summaries, and old command notes cannot override this gate.

## [04]-[ENGINEERING_CONTRACT]

New library or package folders begin with a planning campaign before production source when the concern is broad, foundational, or consumer-facing. The campaign makes infra truth honest, captures manifests/lockfiles/tool pins, extracts API catalogs through repo-owned evidence rails, runs research/gap/adversarial/deep-read passes before authoring, enumerates isolated and in-concert capability across modalities, then collapses surviving capability into owner ledgers, row/case/policy axes, and decision-complete planning pages. Zero consumers never lowers ambition; it requires full-capability design. Package-local proof ladders, admissions tables, build orders, and file processes stay in that folder's charter, not root policy.

Planning and design documents are implementation surfaces. A plan/spec must be file-grouped and decision-complete: each target file names the owner section or card, replacement shape, exact types/signatures/fields/rows/fences, admitted package or API surfaces, deletion/collapse moves, and consumer consequences. Loose prose, placeholder bullets, ceremony tails, and "add docs about" items are not acceptable planning output.

Code fences and prose implementation snippets in planning or stack-doctrine files are real design contracts. Fences must obey the route-owned stack doctrine, use language-valid neutral identifiers, spell current package/member/operator names exactly, and carry enough surrounding shape to show the future owner surface. Prose that names a callable, type, field, operator, generated surface, command, or package must use a code span and meet the same source-backed truth bar.

All tooling, tests, docs, and libraries discover owners through manifests, configured roots, package graphs, route maps, and tool catalog rows. Current paths are inputs, never reusable doctrine.

Default repair mode is owner rebuild, not accretion. When a target page, card, table, snippet, or section is stale, thin, duplicated, underspecified, or below stack doctrine, rewrite the owning surface in place, merge near-peer material, delete obsolete route/provenance/compatibility prose, and route displaced facts to their real owner. Do not append a sibling section, caveat, shim, or summary copy to avoid reshaping the existing owner.

For planning, doctrine, and design-page hardening, use the phase shape `author -> critique -> redteam -> reconcile`. Critique and redteam passes reopen current disk and attach every defect to exact file/section evidence. Only cross-file defects may enter `residual_high`, and each residual is `{ files, claim }`. Reconcile dedupes by sorted files plus claim, clusters by shared files, fixes real defects in owner order, and classifies each claim from current disk as `fixed`, `invalid`, or `open`.

Every repo tool must route generated storage, caches, benchmark output, mutation workdirs, coverage files, snapshots, and scratch artifacts through the owning language/tool configuration or the owning repo tool surface. Do not rely on ambient CLI defaults or gitignore-only tolerance for root litter; configure the tool in `pyproject.toml`, `Directory.Build.props`, tool manifests, test conftests, or the canonical tool engine so outputs land under `.cache`, `.artifacts`, or another owner-declared path.

## [05]-[MONOREPO_TOPOLOGY]

Rasm is a RhinoWIP, GH2, and product-neutral AEC/design-geometry workspace. Interpret library, tool, app, service, web, Python, TypeScript, and C# work through that domain frame before choosing shape: reusable capability lands in the deepest owner that can absorb it, while product shells bind intent, host edges, and output.

AEC pressure informs geometry, topology, units, tolerances, host documents, object graphs, exchange, visualization, compute, persistence, and evidence; it never collapses runtime, UI, compute, persistence, Python, TypeScript, Assay, or Bridge into generic app/plugin code.

The C# package set follows the canonical strata — KERNEL, AEC-DOMAIN, APP-PLATFORM, HOST-BOUNDARY, then future APP shells — owned in full by `libs/.planning/architecture.md`: the package roster, each package's charter, and the dependency direction live there and are never restated here.

"Universal" never means host-free C#: the whole C# branch is RhinoCommon-aware. A host-neutral owner exists only where a non-Rhino runtime (Python, TypeScript) consumes the contract; otherwise the Rhino-native surface is captured as a host-specific feature. The universal-vs-capture rule and the one-owner-per-runtime geometry/mesh/IFC flow are owned in full by `architecture.md`.

Host lifecycle remains outside libraries. Rhino launch, endpoint discovery, package staging, bridge doctor/check/verify/quit/redeploy, cargo, spool, scenario evidence, and host-bound runtime facts belong to `tools/rhino-bridge` and the Assay bridge/package rails that consume it.

## [06]-[TOOL_OWNERS]

Assay command truth lives in `tools/assay/composition/registry.py`, result envelopes in `tools/assay/core/model.py`, and status projection in `tools/assay/core/status.py`. Do not infer commands from old aliases, stale plans, stderr habits, or direct tool muscle memory.

Normal Assay invocations emit one stdout `Envelope`; automation emits NDJSON. Parse `report.detail`, `report.results`, `report.artifacts`, `error`, and `error_context` as the result channel. Stderr is transport noise unless the envelope says otherwise.

Monorepo routing comes from manifests, project graph closure, trigger files, explicit `AssayHostBound`, package slugs, route maps, and catalog rows. Avoid path-name heuristics and single-project assumptions.

Host-bound work is local by contract. `ASSAY_EXEC_TARGET` moves admitted subprocess execution only; routing, leases, package staging, bridge/API discovery, and live Rhino verification still require the owning local or shared state.

Artifact paths emitted by Assay envelopes and bridge `reportDir` are authoritative. Inspect them before assuming layouts; root scratch output is a defect unless routed through the owning store, scope, or state file.

Bridge verification proof is `EvidenceCertificate` plus reviewed `ReferenceEvidence`. MCP exploration may discover invariants, but proof starts only after promotion into typed `[RhinoScenario]` sources and certificate-backed `bridge verify`.

Machine-level scientific and provisioning executables live in `Parametric_Forge`. Rasm campaign work enters through zero-arity Assay `provision` verbs and reads sanitized `ProvisionRun` evidence from `report.detail`; direct `forge-provision`, `forge-scientific-env`, direct database shells, cleanup, diagnostic JSON, Docker/Compose, port, and credential work are Forge-level debugging. Rasm owns the manifests, lockfiles, `.api` catalogues, and assay evidence that consume those tools.

NuGet feed, version, vulnerability, and supply-chain intelligence routes through the `nuget` MCP; the apply path (`Directory.Packages.props` hand-edit, `assay api` member-truth precedence, `survey-packages`/`survey-gaps` modernization) is owned by `CLAUDE.md` and not restated here.

The `ifcopenshell` CLI runs through the cp312 `forge-companion-env` lane for batch convert and validate; the `ifc` MCP live-inspection surface and the `Rasm.Bim` sole-authority split are owned by `CLAUDE.md`. The `jupyter` skill owns notebook execution: headless `papermill`/`nbclient` via `uv run`, or the always-on `jupyter` MCP for a live kernel.

## [07]-[DOCUMENTATION]

Route README, ADR, architecture, roadmap, test strategy, API, reference, code documentation, support matrix, how-to, runbook, contributing, tutorial, onboarding, and instruction-file work through `docs/standards/README.md`.

Implementation-bearing documentation also composes the route-owned language doctrine. C# planning/source snippets route through `docs/stacks/csharp/README.md` plus the relevant finalized concept/domain pages; Python planning/source snippets route through `docs/stacks/python/README.md` plus the relevant finalized concept pages. C# planning pages read both the shared `libs/csharp/.api` catalogue tier and package-local `.api` catalogues; Python planning pages read root and package-local `.api` catalogues where present.

Keep generated documentation, prompts, skills, standards, examples, templates, and reusable guidance project-agnostic by default. Do not mention Rasm, repository-specific paths, local commands, local package names, project functions, concrete source files, or project-only docs unless the target file explicitly exists to describe this repository's own usage, routing, or implementation. Generic examples use the neutral names, placeholder alphabet, and code-safe shapes the form standards define. Use concrete repository names, paths, functions, commands, versions, dates, IDs, or package facts only when the document's job is to describe that exact source-backed repository surface.

Future-facing standards, plans, and target designs do not inherit current drift; remove stale paths, stale commands, compatibility prose, old-baseline caveats, partial-adoption apologies, and invented routes instead of preserving them.

For planning-heavy design docs, preserve future-facing ambition while making implementation truth explicit. Separate real production source, planning/catalogue truth, and target design inside the owning document; do not let catalogue polish or prose confidence imply that production code exists. When a target tree is planning-only, say so in the owner surface and continue designing the full-capability future shape there.

Durable docs, prompts, standards, skills, examples, and reusable templates are agent-facing declarative law, not reports, walkthroughs, origin logs, or checklist tails.
