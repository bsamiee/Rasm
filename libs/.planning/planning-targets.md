# [PLANNING_TARGETS]

[CENTRAL]:
- Directory: `libs/.planning`
- Doctrine: `libs/.planning/campaign-method.md`, `libs/.planning/planning-targets.md`
- Root docs: the branch doc-set at `libs/.planning/` (`README`, `ARCHITECTURE`, `RULINGS`, `IDEAS`, `TASKLOG`)

[CSHARP]:
- Core dir: `libs/csharp/.planning`
- Branch API catalogues: `libs/csharp/.api`
- Routing: `libs/csharp/.planning/README.md`
- Language-wide docs: `ARCHITECTURE.md`, `RULINGS.md`, `IDEAS.md`, `TASKLOG.md` under the core dir
- Planning Folders: every package folder under `libs/csharp/` — the kernel `Rasm` and its `Rasm.*` siblings, per the branch `[02]-[STRATA]` roster
- Target Packages: `Rasm.Generation` (APP-PLATFORM layout/generation/assembly orchestration)

[TYPESCRIPT]:
- Core dir: `libs/typescript/.planning`
- Branch API catalogues: `libs/typescript/.api`
- Routing: `libs/typescript/.planning/README.md`
- Language-wide docs: `ARCHITECTURE.md`, `RULINGS.md`, `IDEAS.md`, `TASKLOG.md` under the core dir
- Planning Folders: every package folder under `libs/typescript/`, per the branch `[02]-[STRATA]` roster

[PYTHON]:
- Core dir: `libs/python/.planning`
- Branch API catalogues: `libs/python/.api`
- Routing: `libs/python/.planning/README.md`
- Language-wide docs: `ARCHITECTURE.md`, `RULINGS.md`, `IDEAS.md`, `TASKLOG.md` under the core dir
- Planning Folders: every package folder under `libs/python/`, per the branch `[02]-[STRATA]` roster — the plane-distinct `contracts` and `cad` seats included

[CROSS_CUTTING_SURFACES]:
- Central manifests: the root `Directory.*` build files, `global.json`, `NuGet.config`, `pyproject.toml`, `pnpm-workspace.yaml`, `.config/`
- Cross-language contracts: `tests/contracts/` defines and proves each atomic case; every executor and consumer binds its `manifest.json` case.
- Event fabric: `libs/.planning/ARCHITECTURE.md` `[14]-[EVENT_FABRIC]` legislates the message envelope and seats its branch owners.
- Per-folder catalogues: every `<pkg>/.api/` catalogue set
- Toolchain evidence: assay provision reports and `.api` catalogues verify Forge server services, native capabilities, and extension availability.
- Pass execution: `.claude/workflows/` (campaign workflows) and `.claude/commands/` (session skills)
- Code doctrine: `docs/stacks/<lang>/`
- Form standards: `docs/standards/information-structure.md`, `docs/standards/formatting.md`, `docs/standards/style-guide.md`
