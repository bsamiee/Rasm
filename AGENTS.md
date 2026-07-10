# [ROOT_AGENTS]

[COMMENT_DISCIPLINE]: Owned by `CLAUDE.md` — the comment law, the section organizational style, and the `docs/standards/style-guide.md` prose bar apply identically here, including all `.md` prose within `libs/`.

## [01]-[LOAD_ORDER]

[REQUIRED]:
- Read and follow `CLAUDE.md` before this file.
- List all files in `docs/stacks/csharp` + `docs/stacks/python` + `docs/stacks/typescript`, including sub-folders — an awareness pass of the standards inventory, not a reading pass.
- Read ALL of `docs/stacks/csharp/README.md` + `docs/stacks/python/README.md` + `docs/stacks/typescript/README.md` as foundational grounding. When working on files (code or code fences in a `.md`) in a language, read the FULL root of `docs/stacks/<language>/` and follow every standard there; `docs/stacks/csharp/domain/` is conditional — read only when relevant.
- Use `loc <path>` for true LOC and complexity score — the heavy-file signal that scopes attention and delegation.
- Use `tree -a <path>` for the full folder/file topology of a directory.
- Use `fmt <target...>` to format any file set or mixed-language tree — one router dispatches each file type to its owning formatter under repo law (biome.json, pyproject.toml, .editorconfig always win); `fmt --check` is the read-only gate.

## [02]-[ENGINEERING_CONTRACT]

- The repo is in a long-term planning state, working exclusively on `.md` design/spec sheets within planning folders, NOT implementing code. Aggressively and iteratively refine, rebuild, and push existing design docs to the pinnacle of possibility — never by spamming code, but through cycles of adding functionality then aggressively rebuilding/collapsing: reduce LOC, surface, and object/shape/type/constant count while maintaining functionality, and ULTRA-stack all external libs/packages per planning folder as sources of new functionality and capability.
- Every planned folder has a `.api/` folder that MUST be FULLY read whenever working in that folder; every language also has one that MUST be fully read when working within that language's planning folders: `libs/csharp/.api`, `libs/python/.api`, `libs/typescript/.api`; folder-level example: `libs/csharp/Rasm.Bim/.api`.
- Design docs are implementation surfaces. A design/spec MUST be file-grouped and decision-complete: each target file names the owner section or card, replacement shape, exact types/signatures/fields/rows/fences, admitted package or API surfaces, deletion/collapse moves, and consumer consequences. Loose prose, placeholder bullets, ceremony tails, and "add docs about" items are not acceptable planning output.
- ALWAYS follow `docs/standards/formatting.md`, `docs/standards/information-structure.md`, and `docs/standards/style-guide.md`; maintain consistency with sibling files.
- ALWAYS reconcile/align planning folders as changes land in one — a change ripples to others, sometimes beyond the language.
- NEVER couple packages. Every package stands fully on its own and is usable in isolation while staying FULLY aligned; never scope a package's capability to another. All are pushed past the bleeding-edge limit, aligned but independent.
- DEPTH-OVER-SURFACE governs every owner: model the full domain interior (all attributes, sub-kinds, states, relationships, and operations; every admitted package at modern operator depth) while exposing FEW unified polymorphic entry points that absorb single|batch|stream by input detection, carry forward and inverse directions on one surface, and internalize policy, routing, and lifecycle. Variation is input shape, policy values, and table rows — never parallel exports, knobs, or modality-named siblings; the surface narrows by absorption, never omission.
- Whenever a planning folder changes, update its `ARCHITECTURE.md` + `README.md` so they do not drift.
- Whenever adding an external package to `Directory.Packages.props`, `pyproject.toml`, or `pnpm-workspace.yaml`, update the relevant planning-folder `README.md` — every planning `README.md` carries a package manifest and a substrate section (shared/universal packages from `libs/<language>/.api/`). For C#/`Directory.Packages.props` additions, also update the relevant planning folder's `.csproj` so it and the README stay aligned.
- Whenever adding an external package, create a stub file in the relevant `.api/` folder with the single line `(placeholder)`. A separate session finalizes the catalog; when the package is relevant to the current scope, still do the deep investigation now and land the findings in the relevant design docs.
- Code fences and prose implementation snippets in design docs MUST be fully realized — never abstract signatures or conceptual sketches. Treat them as real implementations held to the `docs/stacks/<language>/` standards; the stacks docs are a FLOOR, not the ceiling. Non-negotiable.
- Every repo tool routes generated storage, caches, benchmark output, mutation workdirs, coverage files, snapshots, and scratch artifacts through the owning language/tool configuration or the owning repo tool surface. Never rely on ambient CLI defaults or gitignore-only tolerance for root litter; configure the tool in `pyproject.toml`, `Directory.Build.props`, tool manifests, test conftests, or the canonical tool engine so outputs land under `.cache`, `.artifacts`, or another owner-declared path. The repo-root residency allowlist is owned by `CLAUDE.md`.

## [03]-[MONOREPO_TOPOLOGY]

- Rasm is a RhinoWIP, GH2, and product-neutral AEC/design-geometry workspace. Interpret library, tool, app, service, web, Python, TypeScript, and C# work through that domain frame before choosing shape: reusable capability lands in the deepest owner that can absorb it, while product shells bind intent, host edges, and output.
- AEC pressure informs geometry, topology, units, tolerances, host documents, object graphs, exchange, visualization, compute, persistence, and evidence; it never collapses runtime, UI, compute, persistence, Python, TypeScript, Assay, or Bridge into generic app/plugin code.
- "Universal" never means host-free C#: the whole C# branch is RhinoCommon-aware. A host-neutral owner exists only where a non-Rhino runtime (Python, TypeScript) consumes the contract; otherwise the Rhino-native surface is captured as a host-specific feature. The universal-vs-capture rule and the one-owner-per-runtime geometry/mesh/IFC flow are owned in full by `architecture.md`.
- Host lifecycle remains outside libraries. Rhino launch, endpoint discovery, package staging, bridge doctor/check/verify/quit/redeploy, cargo, spool, scenario evidence, and host-bound runtime facts belong to `tools/rhino-bridge` and the Assay bridge/package rails that consume it.

## [04]-[TOOL_OWNERS]

- ANY PROVISIONING/SUBSTRATE PROVIDED BY `Parametric_Forge` IS ONLY FOR LOCAL DEVELOPMENT, NOT PART OF FINAL PACKAGING/DELIVERABLES; THE PROJECT MUST STAND ON ITS OWN.
- AGGRESSIVELY LIVE-PROBE ON THE LOCAL MACHINE, NEVER HESITATE: RHINO WIP + GH2 EXIST LOCALLY, and the tooling exists to create provisioned containers, CLI tooling, and substrate packages. `Parametric_Forge` may be added to or updated when needed — agnostic alignment, never coupling to `Rasm`.
- Assay command truth lives in `tools/assay/composition/registry.py` and `tools/assay/README.md`.
- Normal Assay invocations emit one stdout `Envelope`; automation emits NDJSON. Parse `report.detail`, `report.results`, `report.artifacts`, `error`, and `error_context` as the result channel. Stderr is transport noise unless the envelope says otherwise.
- Monorepo routing comes from manifests, project graph closure, trigger files, explicit `AssayHostBound`, package slugs, route maps, and catalog rows. Avoid path-name heuristics and single-project assumptions.
- Host-bound work is local by contract. `ASSAY_EXEC_TARGET` moves admitted subprocess execution only; routing, leases, package staging, bridge/API discovery, and live Rhino verification still require the owning local or shared state.
- Artifact paths emitted by Assay envelopes and bridge `reportDir` are authoritative. Inspect them before assuming layouts; root scratch output is a defect unless routed through the owning store, scope, or state file.
- Bridge verification proof is `EvidenceCertificate` plus reviewed `ReferenceEvidence`. MCP exploration may discover invariants, but proof starts only after promotion into typed `[RhinoScenario]` sources and certificate-backed `bridge verify`.
- Machine-level scientific and provisioning executables live in `Parametric_Forge`. Rasm campaign work enters through zero-arity Assay `provision` verbs and reads sanitized `ProvisionRun` evidence from `report.detail`; direct `forge-provision`, `forge-scientific-env`, direct database shells, cleanup, diagnostic JSON, Docker/Compose, Apple Container runtime selection, port, and credential work are Forge-level debugging. Rasm owns the manifests, lockfiles, `.api` catalogues, and assay evidence that consume those tools.
- NuGet feed, version, vulnerability, and supply-chain intelligence routes through the `nuget` MCP; the apply path (`Directory.Packages.props` hand-edit, `assay api` member-truth precedence, `survey` modernization) is owned by `CLAUDE.md` and not restated here.
- The `ifcopenshell` CLI runs through the cp312 `forge-companion-env` lane for batch convert and validate; the `ifc` MCP live-inspection surface and the `Rasm.Bim` sole-authority split are owned by `CLAUDE.md`. The `jupyter-notebooks` skill owns notebook execution: headless `papermill`/`nbclient` via `uv run`, or the always-on `jupyter` MCP for a live kernel.
- The proof estate law — lane vocabulary, proof grades, the witness mandate, artifact routing, gate ownership — is `tests/README.md`; read it before touching any testing surface, and never restate its thresholds or routes elsewhere.

## [05]-[DOCUMENTATION]

- All prose in any `.md` file follows `docs/standards/style-guide.md`, `docs/standards/formatting.md`, and `docs/standards/information-structure.md`: declarative, non-hedging, no meta commentary, no unusual coupling.
- Durable docs, prompts, standards, skills, examples, and reusable templates are agent-facing declarative law, not reports, walkthroughs, origin logs, or checklist tails. ALL documentation and prose, comments included, exist purely for agents, never humans; every line earns its keep by being useful to agents working the file — otherwise remove it.
- Keep generated documentation, prompts, skills, standards, examples, templates, and reusable guidance project-agnostic by default. Do not mention Rasm, repository-specific paths, local commands, local package names, project functions, concrete source files, or project-only docs unless the target file explicitly exists to describe this repository's own usage, routing, or implementation. Generic examples use the neutral names, placeholder alphabet, and code-safe shapes the form standards define; concrete repository names, paths, functions, commands, versions, dates, IDs, or package facts appear only when the document's job is to describe that exact source-backed repository surface.
- Future-facing standards, plans, and target designs do not inherit current drift; remove stale paths, stale commands, compatibility prose, old-baseline caveats, partial-adoption apologies, and invented routes instead of preserving them.
