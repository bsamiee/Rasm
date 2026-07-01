# [ROOT_AGENTS]

## [01]-[LOAD_ORDER]

[REQUIRED]:
- Read and follow `CLAUDE.md` before this file
- Read and follow `README.md`
- Read and follow `docs/standards/style-guide.md` for all prose in any capacity, including comments, and prompts

## [02]-[NAVIGATION]

- Read every target file end-to-end before editing. For planning, catalogue, doctrine, and standards work, inventory owner trees with `fd`; read the owner README first, then the target `.planning/`, `.api`, or `docs/stacks/<language>/` pages in routing order. Use `tree` and `loc` to display the folder/file structure of an `<ARG>` and the line count and complexity of each file, and summaries of each folder, `<ARG>` takes a path as value.
- When extracting/investigating the API of a source, use `tools/assay/` read the `tools/assay/README.md`, if a c#/net package, use the nuget MCP server, and always use Context7 mcp server when needed for any implementation details for external packages.

## [03]-[ENGINEERING_CONTRACT]

- All libraries and package folders begin with a planning campaign, we never make code files from files within `.planning/` folders unless explicitly stated, always assume refinement/rebuilding/refactoring of the `.md` within them, with a focus on code fences. All planning guidance is found in `libs/.planning`, read all of `libs/.planning/planning-targets.md` for existing targets, and `libs/.planning/architecture.md` for the strata/topology.
- All planned folders have a `.api/` folder that MUST be FULLY read whenever working in such a folder, all languages also have a `.api/` that MUST be fully read when working within that languages library planning folders: `libs/csharp/.api`, `libs/python/.api`, `libs/typescript/.api`, example of a planning folder api relative location: `libs/csharp/Rasm.Bim/.api`.
- Design docs are implementation surfaces. A design/spec MUST be file-grouped and decision-complete: each target file names the owner section or card, replacement shape, exact types/signatures/fields/rows/fences, admitted package or API surfaces, deletion/collapse moves, and consumer consequences. Loose prose, placeholder bullets, ceremony tails, and "add docs about" items are not acceptable planning output.
- Whenever adding a new external package/lib to `Directory.Packages.props`, `pyproject.toml`, or `pnpm-workspace.yaml`, MUST update the relevant `README.md` of the planning folder to account for the new package, all `README.md` within planning directories contain a package manifest, and substrate, substrate is shared/universal packages all folders use from the `libs/<language>/.api/` location. IF the external package added is for c#/`Directory.Packages.props`, we MUST also update the `.csproj` of the relevant planning folder so that it and the readme are aligned.
- Whenever adding a new external package, regardless of langauge, make a SIMPLE stub file in the relevant `.api/` folder with a single line value of: `(placeholder)`. A seperate session will finalize/flesh out the file, however, if the package is relevant to current scope of work, you must still do the deep investigation to extract all relevant information to implement in the relevant design docs.
- Code fences and prose implementation snippets in design docs MUST be fully realized, not abstract signatures or conceptual implementations, treat them as real code implementation, that MUST adhere to the `docs/stacks/<language>/` file standards, treat the stacks docs as a FLOOR not the ceiling, this is non-negotiable.
- Every repo tool must route generated storage, caches, benchmark output, mutation workdirs, coverage files, snapshots, and scratch artifacts through the owning language/tool configuration or the owning repo tool surface. Do not rely on ambient CLI defaults or gitignore-only tolerance for root litter; configure the tool in `pyproject.toml`, `Directory.Build.props`, tool manifests, test conftests, or the canonical tool engine so outputs land under `.cache`, `.artifacts`, or another owner-declared path.

## [04]-[MONOREPO_TOPOLOGY]

- Rasm is a RhinoWIP, GH2, and product-neutral AEC/design-geometry workspace. Interpret library, tool, app, service, web, Python, TypeScript, and C# work through that domain frame before choosing shape: reusable capability lands in the deepest owner that can absorb it, while product shells bind intent, host edges, and output.
- AEC pressure informs geometry, topology, units, tolerances, host documents, object graphs, exchange, visualization, compute, persistence, and evidence; it never collapses runtime, UI, compute, persistence, Python, TypeScript, Assay, or Bridge into generic app/plugin code.
- "Universal" never means host-free C#: the whole C# branch is RhinoCommon-aware. A host-neutral owner exists only where a non-Rhino runtime (Python, TypeScript) consumes the contract; otherwise the Rhino-native surface is captured as a host-specific feature. The universal-vs-capture rule and the one-owner-per-runtime geometry/mesh/IFC flow are owned in full by `architecture.md`.
- Host lifecycle remains outside libraries. Rhino launch, endpoint discovery, package staging, bridge doctor/check/verify/quit/redeploy, cargo, spool, scenario evidence, and host-bound runtime facts belong to `tools/rhino-bridge` and the Assay bridge/package rails that consume it.

## [05]-[TOOL_OWNERS]

- Assay command truth lives in `tools/assay/composition/registry.py` and `tools/assay/README.md`.
- Normal Assay invocations emit one stdout `Envelope`; automation emits NDJSON. Parse `report.detail`, `report.results`, `report.artifacts`, `error`, and `error_context` as the result channel. Stderr is transport noise unless the envelope says otherwise.
- Monorepo routing comes from manifests, project graph closure, trigger files, explicit `AssayHostBound`, package slugs, route maps, and catalog rows. Avoid path-name heuristics and single-project assumptions.
- Host-bound work is local by contract. `ASSAY_EXEC_TARGET` moves admitted subprocess execution only; routing, leases, package staging, bridge/API discovery, and live Rhino verification still require the owning local or shared state.
- Artifact paths emitted by Assay envelopes and bridge `reportDir` are authoritative. Inspect them before assuming layouts; root scratch output is a defect unless routed through the owning store, scope, or state file.
- Bridge verification proof is `EvidenceCertificate` plus reviewed `ReferenceEvidence`. MCP exploration may discover invariants, but proof starts only after promotion into typed `[RhinoScenario]` sources and certificate-backed `bridge verify`.
- Machine-level scientific and provisioning executables live in `Parametric_Forge`. Rasm campaign work enters through zero-arity Assay `provision` verbs and reads sanitized `ProvisionRun` evidence from `report.detail`; direct `forge-provision`, `forge-scientific-env`, direct database shells, cleanup, diagnostic JSON, Docker/Compose, port, and credential work are Forge-level debugging. Rasm owns the manifests, lockfiles, `.api` catalogues, and assay evidence that consume those tools.
- NuGet feed, version, vulnerability, and supply-chain intelligence routes through the `nuget` MCP; the apply path (`Directory.Packages.props` hand-edit, `assay api` member-truth precedence, `survey-packages`/`survey-gaps` modernization) is owned by `CLAUDE.md` and not restated here.
- The `ifcopenshell` CLI runs through the cp312 `forge-companion-env` lane for batch convert and validate; the `ifc` MCP live-inspection surface and the `Rasm.Bim` sole-authority split are owned by `CLAUDE.md`. The `jupyter` skill owns notebook execution: headless `papermill`/`nbclient` via `uv run`, or the always-on `jupyter` MCP for a live kernel.

## [06]-[DOCUMENTATION]

- Whenever working on any PROSE in any `.md` file, MUST follow and adhere to `docs/standards/style-guide.md`, `docs/standards/formatting.md`, and `docs/standards/information-structure.md`, NEVER use meta commentary, unusual coupling or agentic instructions/guidance, all prose are declarative and NON-HEDGING.
- Durable docs, prompts, standards, skills, examples, and reusable templates are agent-facing declarative law, not reports, walkthroughs, origin logs, or checklist tails. ALL DOCUMENTATION AND PROSE, INCLUDING COMMENTS ARE PURELY FOR AGENTS, NOT HUMANS, NEVER CREATE UNNECESSARY EXPLANATORY PROSE/COMMENTS, AND ENSURE ALL PROSE IN ANY CAPACITY EARN THEIR KEEP, THEY MUST BE RELEVANT/USEFUL TO AGENTS WORKING WITHIN A FILE; OTHERWISE REMOVE THE PROSE.
- Keep generated documentation, prompts, skills, standards, examples, templates, and reusable guidance project-agnostic by default. Do not mention Rasm, repository-specific paths, local commands, local package names, project functions, concrete source files, or project-only docs unless the target file explicitly exists to describe this repository's own usage, routing, or implementation. Generic examples use the neutral names, placeholder alphabet, and code-safe shapes the form standards define. Use concrete repository names, paths, functions, commands, versions, dates, IDs, or package facts only when the document's job is to describe that exact source-backed repository surface.
- Future-facing standards, plans, and target designs do not inherit current drift; remove stale paths, stale commands, compatibility prose, old-baseline caveats, partial-adoption apologies, and invented routes instead of preserving them.
