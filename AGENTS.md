# [ROOT_AGENTS]

## [01]-[LOAD_ORDER]

[REQUIRED]:
- List all files in `docs/stacks/<language>/` for each language the task touches, sub-folders included — an awareness pass of the standards inventory, not a reading pass; an untouched language's doctrine is never loaded.
- When working on files (code or code fences in a `.md`) in a language, read `docs/stacks/<language>/README.md` as grounding and the FULL root of `docs/stacks/<language>/` before authoring, and follow every standard there; `docs/stacks/csharp/domain/` is conditional — read only the shards the work touches. Cross-language work grounds each touched language the same way.
- `fmt <target...>` formats any file set or mixed-language tree under repo law (biome.json, pyproject.toml, .editorconfig always win); `fmt --check` is the read-only gate. `loc`/`tree` and the shell verb table are global law, not restated here.

## [02]-[ENGINEERING_CONTRACT]

- The repo is in a long-term planning state, working exclusively on `.md` design/spec sheets within planning folders, never implementing code. Iteratively refine, rebuild, and push existing design docs to the pinnacle of possibility — never by spamming code, but through cycles of adding functionality then rebuilding and collapsing: reduce LOC, surface, and object/shape/type/constant count while maintaining functionality, and ULTRA-stack all external libs/packages per planning folder as sources of new functionality and capability.
- Every planned folder has a `.api/` folder that MUST be FULLY read whenever working in that folder; every language also has one that MUST be fully read when working within that language's planning folders: `libs/csharp/.api`, `libs/python/.api`, `libs/typescript/.api`; folder-level example: `libs/csharp/Rasm.Bim/.api`.
- Design docs are implementation surfaces. A design/spec MUST be file-grouped and decision-complete: each target file names the owner section or card, replacement shape, exact types/signatures/fields/rows/fences, admitted package or API surfaces, deletion/collapse moves, and consumer consequences. Loose prose, placeholder bullets, ceremony tails, and "add docs about" items are not acceptable planning output.
- NEVER couple packages. Every package stands fully on its own and is usable in isolation while staying FULLY aligned; never scope a package's capability to another. All are pushed past the bleeding-edge limit, aligned but independent.
- DEPTH-OVER-SURFACE governs every owner.
- Whenever adding an external package to `Directory.Packages.props`, `pyproject.toml`, or `pnpm-workspace.yaml`, update the relevant planning-folder `README.md`, every planning `README.md` carries a package manifest and a substrate section (shared/universal packages from `libs/<language>/.api/`). For C#/`Directory.Packages.props` additions, also update the relevant planning folder's `.csproj` so it and the README stay aligned.
- Whenever adding an external package, create a stub file in the relevant `.api/` folder with the single line `(placeholder)`. A separate session finalizes the catalog; when the package is relevant to the current scope, still do the deep investigation now and land the findings in the relevant design docs.
- Code fences and prose implementation snippets in design docs MUST be fully realized — never abstract signatures or conceptual sketches. Treat them as real implementations held to the `docs/stacks/<language>/` standards; the stacks docs are a FLOOR, not the ceiling. Non-negotiable.
- Every repo tool routes generated storage, caches, benchmark output, mutation workdirs, coverage files, snapshots, and scratch artifacts through the owning language/tool configuration or the owning repo tool surface. Never rely on ambient CLI defaults or gitignore-only tolerance for root litter; configure the tool in `pyproject.toml`, `Directory.Build.props`, tool manifests, test conftests, or the canonical tool engine so outputs land under `.cache`, `.artifacts`, or another owner-declared path.

## [03]-[MONOREPO_TOPOLOGY]

- Read all of `README.md`
- AEC pressure informs geometry, topology, units, tolerances, host documents, object graphs, exchange, visualization, compute, persistence, and evidence; it never collapses runtime, UI, compute, persistence, Python, TypeScript, Assay, or Bridge into generic app/plugin code.
- "Universal" never means host-free C#: the whole C# branch is RhinoCommon-aware. A host-neutral owner exists only where a non-Rhino runtime (Python, TypeScript) consumes the contract; otherwise the Rhino-native surface is captured as a host-specific feature. The universal-vs-capture rule and the one-owner-per-runtime geometry/mesh/IFC flow are owned in full by `architecture.md`.
- Host lifecycle remains outside libraries. Rhino launch, endpoint discovery, package staging, bridge doctor/check/verify/quit/redeploy, cargo, spool, scenario evidence, and host-bound runtime facts belong to `tools/rhino-bridge` and the Assay bridge/package rails that consume it.
