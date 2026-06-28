[CENTRAL]:
- Directory: `libs/.planning`
- Doctrine: `libs/.planning/campaign-method.md`, `libs/.planning/planning-targets.md`
- Root docs: `libs/.planning/README.md`, `libs/.planning/architecture.md`, `libs/.planning/IDEAS.md`, `libs/.planning/TASKLOG.md`

[CSHARP]:
- Core dir: `libs/csharp/.planning`
- Branch API catalogues: `libs/csharp/.api`
- Routing: `libs/csharp/.planning/README.md`
- Language-wide docs: `libs/csharp/.planning/ARCHITECTURE.md`, `libs/csharp/.planning/IDEAS.md`, `libs/csharp/.planning/TASKLOG.md`
- Rasm (Sub-folder): `libs/csharp/Rasm/geometry/.planning`, `libs/csharp/Rasm/.api`
- Rasm root docs: `libs/csharp/Rasm/README.md`, `libs/csharp/Rasm/ARCHITECTURE.md`, `libs/csharp/Rasm/IDEAS.md`, `libs/csharp/Rasm/TASKLOG.md`
- Planning Folders: `libs/csharp/Rasm.AppHost`, `libs/csharp/Rasm.AppUi`, `libs/csharp/Rasm.Bim`, `libs/csharp/Rasm.Compute`, `libs/csharp/Rasm.Element`, `libs/csharp/Rasm.Fabrication`, `libs/csharp/Rasm.Materials`, `libs/csharp/Rasm.Persistence`

[TYPESCRIPT]:
- Core dir: `libs/typescript/.planning`
- Routing: `libs/typescript/.planning/README.md`
- Language-wide docs: `libs/typescript/.planning/ARCHITECTURE.md`, `libs/typescript/.planning/IDEAS.md`, `libs/typescript/.planning/TASKLOG.md`
- Planning Folders: `libs/typescript/interchange`, `libs/typescript/platform`, `libs/typescript/projection`, `libs/typescript/services`, `libs/typescript/ui`

[PYTHON]:
- Core dir: `libs/python/.planning`
- Routing: `libs/python/.planning/README.md`
- Language-wide docs: `libs/python/.planning/ARCHITECTURE.md`, `libs/python/.planning/IDEAS.md`, `libs/python/.planning/TASKLOG.md`
- Planning Folders: `libs/python/artifacts`, `libs/python/compute`, `libs/python/data`, `libs/python/geometry`, `libs/python/runtime`

[CROSS_CUTTING_SURFACES]:
- Central manifests: `Directory.Packages.props`, `Directory.Build.props`, `Directory.Build.targets`, `global.json`, `NuGet.config`, `pyproject.toml`, `pnpm-workspace.yaml`, `.config`
- Per-folder catalogues: every `<pkg>/.api/` catalogue set
- Toolchain evidence: Assay provision reports and per-folder `.api` catalogues verify Forge-provided server services, native capabilities, and extension availability.
- Pass-prompts: `.claude/prompts/1-ideate-pass.md`, `.claude/prompts/2-refine-pass.md`, `.claude/prompts/3-implement-pass.md`
- Code doctrine: `docs/stacks/<lang>/`, with `docs/stacks/csharp/` the universal floor
- Form standards: `docs/standards/information-structure.md`, `docs/standards/formatting.md`, `docs/standards/style-guide.md`
