# [ROOT_AGENTS]

- Read and follow `CLAUDE.md` fully, IGNORE THE DISPATCH/CUSTODIAN + PRIME INSTRUCTIONS, NOT APPLICABLE.
- Read the root `README.md` in full.

## [01]-[LOAD_ORDER]

[REQUIRED]:
- Before authoring code or fences in a language, read the FULL root of `docs/stacks/<language>/` and follow every standard there ONCE not repeated.
- `docs/stacks/csharp/domain/` shards load only when the work touches them; cross-language work grounds each touched language the same way.
- `fmt <target...>` formats any file set or tree under repo law (`biome.json`, `pyproject.toml`, `.editorconfig` win); `fmt --check` is read-only.

## [02]-[ENGINEERING_CONTRACT]

- Planning-corpus verification is READ-ONLY: never compile, build, run code analyzers, or execute tests against `.planning/` pages or any markdown.
- Work lands ONLY on `.md` design/spec sheets inside planning folders; a source file is never created from.
- Working in a planning folder requires a FULL listing of its `libs/<language>/<folder>/.api/` AND `libs/<language>/.api/` for external lib integraiton.
- ULTRA-stack each planning folder's external packages as the source of new functionality and capability.
- Design docs are implementation surfaces: file-grouped and decision-complete.
- NEVER couple packages: each stands alone and usable in isolation — aligned with siblings, respect `libs/.planning/ARCHITECTURE.md`.
- DEPTH-OVER-SURFACE governs every owner.
- A package added to `Directory.Packages.props`, `pyproject.toml`, or `pnpm-workspace.yaml` also lands in the owning planning-folder `README.md`.
- Every planning `README.md` carries a package manifest and a substrate section (shared packages from `libs/<language>/.api/`).
- A C# package addition also updates the owning planning folder's `.csproj`; `.csproj` and `README.md` stay aligned.
- Code fences in design docs are fully realized — never abstract signatures or sketches — with `docs/stacks/<language>/` as the FLOOR.
- Run each applicable check ONCE, after the work lands; a clean result is final — never re-run until all prior findings have been resolved.
- Every repo tool routes its generated output (caches, benchmarks, coverage, snapshots, scratch) through the owning tool configuration.
- Configure `pyproject.toml`, `Directory.Build.props`, or the tool manifest so output lands in `.cache/` or `.artifacts/` — never ambient defaults.
