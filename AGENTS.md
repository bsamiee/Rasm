# [ROOT_AGENTS]

[NEVER]: hand-roll a script, sed/awk/regex pass, or ad-hoc programmatic transform to "fix" markdown, prose, comments, tables, or any documentation — doc repair routes ONLY through the owning skill's fix command or a project tool (e.g. the docgen prose gate `fix`); where none exists, the repair is proper manual read-and-rewrite, one edit at a time, never a bespoke script and never laziness.

## [01]-[LOAD_ORDER]

[REQUIRED]:
- List all files in `docs/stacks/<language>/` for each language the task touches, sub-folders included — an awareness pass of the standards inventory, not a reading pass; an untouched language's doctrine is never loaded.
- When working on files (code or code fences in a `.md`) in a language, read `docs/stacks/<language>/README.md` as grounding and the FULL root of `docs/stacks/<language>/` before authoring, and follow every standard there; `docs/stacks/csharp/domain/` is conditional — read only the shards the work touches. Cross-language work grounds each touched language the same way.
- `fmt <target...>` formats any file set or mixed-language tree under repo law (biome.json, pyproject.toml, .editorconfig always win); `fmt --check` is the read-only gate. `loc`/`tree` and the shell verb table are global law, not restated here.

## [02]-[ENGINEERING_CONTRACT]

- The repo is in a long-term planning state, working exclusively on `.md` design/spec sheets within planning folders, never implementing code. Iteratively refine, rebuild, and push existing design docs to the pinnacle of possibility — never by spamming code, but through cycles of adding functionality then rebuilding and collapsing: reduce LOC, surface, and object/shape/type/constant count while maintaining functionality, and ULTRA-stack all external libs/packages per planning folder as sources of new functionality and capability.
- Every planned folder has a `.api/` folder that MUST be FULLY read whenever working in that folder; every language also has one that MUST be fully read when working within that language's planning folders: `libs/csharp/.api`, `libs/python/.api`, `libs/typescript/.api`; folder-level example: `libs/csharp/Rasm.Bim/.api`.
- Design docs are implementation surfaces. A design/spec MUST be file-grouped and decision-complete: each target file names the owner section or card, replacement shape, exact types/signatures/fields/rows/fences, admitted package or API surfaces, deletion/collapse moves, and consumer consequences. Loose prose, placeholder bullets, ceremony tails, and "add docs about" items are not acceptable planning output.
- ALWAYS reconcile/align planning folders as changes land in one — a change ripples to others, sometimes beyond the language.
- NEVER couple packages. Every package stands fully on its own and is usable in isolation while staying FULLY aligned; never scope a package's capability to another. All are pushed past the bleeding-edge limit, aligned but independent.
- DEPTH-OVER-SURFACE governs every owner.
- Whenever a planning folder changes, update its `ARCHITECTURE.md` + `README.md` so they do not drift.
- Whenever adding an external package to `Directory.Packages.props`, `pyproject.toml`, or `pnpm-workspace.yaml`, update the relevant planning-folder `README.md` — every planning `README.md` carries a package manifest and a substrate section (shared/universal packages from `libs/<language>/.api/`). For C#/`Directory.Packages.props` additions, also update the relevant planning folder's `.csproj` so it and the README stay aligned.
- Whenever adding an external package, create a stub file in the relevant `.api/` folder with the single line `(placeholder)`. A separate session finalizes the catalog; when the package is relevant to the current scope, still do the deep investigation now and land the findings in the relevant design docs.
- Code fences and prose implementation snippets in design docs MUST be fully realized — never abstract signatures or conceptual sketches. Treat them as real implementations held to the `docs/stacks/<language>/` standards; the stacks docs are a FLOOR, not the ceiling. Non-negotiable.
- Every repo tool routes generated storage, caches, benchmark output, mutation workdirs, coverage files, snapshots, and scratch artifacts through the owning language/tool configuration or the owning repo tool surface. Never rely on ambient CLI defaults or gitignore-only tolerance for root litter; configure the tool in `pyproject.toml`, `Directory.Build.props`, tool manifests, test conftests, or the canonical tool engine so outputs land under `.cache`, `.artifacts`, or another owner-declared path.

## [03]-[MONOREPO_TOPOLOGY]

- Rasm is a RhinoWIP, GH2, and product-neutral AEC/design-geometry workspace. Interpret library, tool, app, service, web, Python, TypeScript, and C# work through that domain frame before choosing shape: reusable capability lands in the deepest owner that can absorb it, while product shells bind intent, host edges, and output.
- AEC pressure informs geometry, topology, units, tolerances, host documents, object graphs, exchange, visualization, compute, persistence, and evidence; it never collapses runtime, UI, compute, persistence, Python, TypeScript, Assay, or Bridge into generic app/plugin code.
- "Universal" never means host-free C#: the whole C# branch is RhinoCommon-aware. A host-neutral owner exists only where a non-Rhino runtime (Python, TypeScript) consumes the contract; otherwise the Rhino-native surface is captured as a host-specific feature. The universal-vs-capture rule and the one-owner-per-runtime geometry/mesh/IFC flow are owned in full by `architecture.md`.
- Host lifecycle remains outside libraries. Rhino launch, endpoint discovery, package staging, bridge doctor/check/verify/quit/redeploy, cargo, spool, scenario evidence, and host-bound runtime facts belong to `tools/rhino-bridge` and the Assay bridge/package rails that consume it.

## [04]-[TOOL_OWNERS]

- Provisioning and substrate provided by `Parametric_Forge` serve local development only — never final packaging or deliverables; the project stands on its own.
- Live-probe on the local machine without hesitation where the sandbox admits it: Rhino WIP + GH2 exist locally, and the tooling exists to create provisioned containers, CLI tooling, and substrate packages. `Parametric_Forge` may be added to or updated when needed — agnostic alignment, never coupling to `Rasm`.
- Assay command truth lives in `tools/assay/composition/registry.py` and `tools/assay/README.md`.
- Normal Assay invocations emit one stdout `Envelope`; automation emits NDJSON. Parse `report.detail`, `report.results`, `report.artifacts`, `error`, and `error_context` as the result channel. Stderr is transport noise unless the envelope says otherwise.
- Host-bound work is local by contract. `ASSAY_EXEC_TARGET` moves admitted subprocess execution only; routing, leases, package staging, bridge/API discovery, and live Rhino verification still require the owning local or shared state.
- Artifact paths emitted by Assay envelopes and bridge `reportDir` are authoritative. Inspect them before assuming layouts; root scratch output is a defect unless routed through the owning store, scope, or state file.
- Bridge verification proof is `EvidenceCertificate` plus reviewed `ReferenceEvidence`. MCP exploration may discover invariants, but proof starts only after promotion into typed `[RhinoScenario]` sources and certificate-backed `bridge verify`.
- Machine-level scientific and provisioning executables live in `Parametric_Forge`. Rasm campaign work enters through zero-arity Assay `provision` verbs and reads sanitized `ProvisionRun` evidence from `report.detail`; direct `forge-provision`, `forge-scientific-env`, direct database shells, cleanup, diagnostic JSON, Docker/Compose, Apple Container runtime selection, port, and credential work are Forge-level debugging. Rasm owns the manifests, lockfiles, `.api` catalogues, and assay evidence that consume those tools.
- NuGet apply-path law (`Directory.Packages.props` hand-edit, `assay api` member-truth precedence, `survey` modernization) and the `ifc` MCP / `Rasm.Bim` sole-authority split are owned by `CLAUDE.md` `[07]` — bind them there; the `nuget` MCP, notebook, and Gemini (`agy`) routing are global law.
- The proof estate law — lane vocabulary, proof grades, the witness mandate, artifact routing, gate ownership — is `tests/README.md`; read it before touching any testing surface, and never restate its thresholds or routes elsewhere.

## [05]-[DOCUMENTATION]

- All `.md` prose follows the `docs/standards/` owners (style-guide, formatting, information-structure): declarative, non-hedging, no meta commentary, no unusual coupling; the durable-artifact register and project-agnostic placeholder law are global.
- `docs/laws/` is the repo-wide maintenance-law corpus — read it at source in substantive passes (it stays small by law), and land a diff touching a `topology.md` `[SURFACE]` together with its obligated counterparts; `docs/laws/README.md` owns the admission law, and the twin routing note lives in `CLAUDE.md` `[02]`.
- Future-facing standards, plans, and target designs do not inherit current drift; remove stale paths, stale commands, compatibility prose, old-baseline caveats, partial-adoption apologies, and invented routes instead of preserving them.

## [06]-[REVIEW_GUIDELINES]

- Report only demonstrated P0 or P1 correctness, security, data-loss, concurrency, or contract defects introduced by the diff; omit summaries, praise, style-only comments, speculative risks, and findings already present on the pull request.
- Anchor each finding to the smallest changed range and state the triggering execution path, violated invariant, and concrete consequence.
