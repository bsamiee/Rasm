# [PLANNING_TARGETS]

[CENTRAL]:
- Directory: `libs/.planning`
- Doctrine: `libs/.planning/campaign-method.md`, `libs/.planning/planning-targets.md`
- Root docs: `libs/.planning/README.md`, `libs/.planning/ARCHITECTURE.md`, `libs/.planning/RULINGS.md`, `libs/.planning/IDEAS.md`, `libs/.planning/TASKLOG.md`

[CSHARP]:
- Core dir: `libs/csharp/.planning`
- Branch API catalogues: `libs/csharp/.api`
- Routing: `libs/csharp/.planning/README.md`
- Language-wide docs: `ARCHITECTURE.md`, `RULINGS.md`, `IDEAS.md`, `TASKLOG.md` under the core dir
- Planning Folders: `libs/csharp/Rasm`, `libs/csharp/Rasm.AppHost`, `libs/csharp/Rasm.AppUi`, `libs/csharp/Rasm.Bim`, `libs/csharp/Rasm.Compute`, `libs/csharp/Rasm.Element`, `libs/csharp/Rasm.Fabrication`, `libs/csharp/Rasm.Grasshopper`, `libs/csharp/Rasm.Materials`, `libs/csharp/Rasm.Persistence`, `libs/csharp/Rasm.Rhino`
- Target Packages: `Rasm.Generation` (APP-PLATFORM layout/generation/assembly orchestration)

[TYPESCRIPT]:
- Core dir: `libs/typescript/.planning`
- Branch API catalogues: `libs/typescript/.api`
- Routing: `libs/typescript/.planning/README.md`
- Language-wide docs: `ARCHITECTURE.md`, `RULINGS.md`, `IDEAS.md`, `TASKLOG.md` under the core dir
- Planning Folders: `libs/typescript/core`, `libs/typescript/security`, `libs/typescript/data`, `libs/typescript/runtime`, `libs/typescript/ui`, `libs/typescript/iac`

[PYTHON]:
- Core dir: `libs/python/.planning`
- Branch API catalogues: `libs/python/.api`
- Routing: `libs/python/.planning/README.md`
- Language-wide docs: `ARCHITECTURE.md`, `RULINGS.md`, `IDEAS.md`, `TASKLOG.md` under the core dir
- Planning Folders: `libs/python/artifacts`, `libs/python/compute`, `libs/python/data`, `libs/python/geometry`, `libs/python/runtime`

[CROSS_CUTTING_SURFACES]:
- Central manifests: `Directory.Packages.props`, `Directory.Build.props`, `Directory.Build.targets`, `global.json`, `NuGet.config`, `pyproject.toml`, `pnpm-workspace.yaml`, `.config/`
- Cross-language contracts: `tests/contracts/` defines every seam schema and proves it; a branch page minting or producing a contract binds its `MANIFEST.md` entry.
- Event fabric: `libs/.planning/ARCHITECTURE.md` `[14]-[EVENT_FABRIC]` legislates the message envelope, and its three branch seats are `libs/csharp/Rasm/.planning/Domain/event.md`, `libs/python/runtime/.planning/transport/`, and `libs/typescript/core/.planning/interchange/`.
- Per-folder catalogues: every `<pkg>/.api/` catalogue set
- Toolchain evidence: Assay provision reports and per-folder `.api` catalogues verify Forge-provided server services, native capabilities, and extension availability.
- Pass execution: `.claude/workflows/` (campaign workflows) and `.claude/commands/` (session skills)
- Code doctrine: `docs/stacks/<lang>/`
- Form standards: `docs/standards/information-structure.md`, `docs/standards/formatting.md`, `docs/standards/style-guide.md`
