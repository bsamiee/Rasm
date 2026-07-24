# [ROOT_AGENTS]

- Read and follow `CLAUDE.md` fully.
- Read the root `README.md` in full.

## [01]-[LOAD_ORDER]

[REQUIRED]:
- List `docs/stacks/<language>/` and its sub-folders for each touched language — a listing only, no reading; never load an untouched language.
- Before authoring code or fences in a language, read the FULL root of `docs/stacks/<language>/` and follow every standard there.
- `docs/stacks/csharp/domain/` shards load only when the work touches them; cross-language work grounds each touched language the same way.
- `fmt <target...>` formats any file set or tree under repo law (`biome.json`, `pyproject.toml`, `.editorconfig` win); `fmt --check` is read-only.
- Run `fmt` once over the touched set at the end of a pass — never per edit, never as a repeated verification loop.

## [02]-[ENGINEERING_CONTRACT]

- Work lands ONLY on `.md` design/spec sheets inside planning folders; a source file is never created or edited.
- ULTRA-stack each planning folder's external packages as the source of new functionality and capability.
- Working in a planning folder requires a FULL listing of its `.api/` AND its language tier for external lib integraiton candidates.
- Design docs are implementation surfaces: file-grouped and decision-complete.
- NEVER couple packages: each stands alone and usable in isolation — aligned with siblings, never scoped to another, pushed past the bleeding edge.
- DEPTH-OVER-SURFACE governs every owner.
- A package added to `Directory.Packages.props`, `pyproject.toml`, or `pnpm-workspace.yaml` also lands in the owning planning-folder `README.md`.
- Every planning `README.md` carries a package manifest and a substrate section (shared packages from `libs/<language>/.api/`).
- A C# package addition also updates the owning planning folder's `.csproj`; `.csproj` and README stay aligned.
- Code fences in design docs are fully realized — never abstract signatures or sketches — with `docs/stacks/<language>/` as the FLOOR.
- Planning-corpus verification is READ-ONLY: never compile, build, run code analyzers, or execute tests against `.planning/` pages or any markdown.
- Judge a fence by reading it against `docs/stacks/<language>/`; verify named members with `tools.assay api` and the `.api` catalogs.
- Prose edits gate with `uv run .claude/skills/docgen/scripts/prose_gate.py`; fence bodies verify with `tools.assay api`; never cross-run.
- Run each applicable check ONCE, after the work lands; a clean result is final — never re-run for reassurance.
- Every repo tool routes its generated output (caches, benchmarks, coverage, snapshots, scratch) through the owning tool configuration.
- Configure `pyproject.toml`, `Directory.Build.props`, or the tool manifest so output lands in `.cache/` or `.artifacts/` — never ambient defaults.

## [03]-[MONOREPO_TOPOLOGY]

- AEC pressure informs geometry, topology, units, tolerances, host documents, object graphs, exchange, visualization, compute, persistence, evidence.
- AEC pressure never collapses runtime, UI, compute, persistence, Python, TypeScript, Assay, or Bridge into generic app/plugin code.
- "Universal" never means host-free C#: the whole C# branch is RhinoCommon-aware.
- A host-neutral owner exists only where a non-Rhino runtime (Python, TypeScript) consumes the contract; otherwise capture the Rhino-native surface.
- `ARCHITECTURE.md` owns the universal-vs-capture rule and the one-owner-per-runtime geometry/mesh/IFC flow in full.
