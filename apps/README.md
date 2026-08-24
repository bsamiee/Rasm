# [APPS]

`apps/` is app-keyed: each `apps/<app-name>/` is one island holding that app's own projects across whatever languages it needs. Islands share nothing by position, and an app composes `libs/` exactly as it takes an external package, so an island carries product intent, host binding, and output alone.

## [01]-[LAYOUT]

```text
apps/
└── <app-name>/
    ├── <Project>/          # C# project, joined to Workspace.slnx
    ├── <project>/          # Python project, seated as a uv workspace member
    └── <project>/          # TypeScript project, seated by the pnpm apps/*/* glob
```

- Islands stay unrelated: two apps couple only where one takes the other's published package, never through a shared parent.
- Language mix is an app decision, and one island spans C#, Python, and TypeScript with one directory per project.
- Path segments carry app identity alone; host reach, tier, and deployment shape ride project configuration.
- Strata law, the consumption axis roster, and the admission ladder live at `libs/.planning/ARCHITECTURE.md`.

## [02]-[HOSTS]

Host roster stays open. Rhino 9 WIP and Grasshopper 2 are the bound hosts today, and Blender or any later host admits by landing its boundary package under `libs/.planning/ARCHITECTURE.md` `[12]-[ADMISSION]` — no tree, glob, or classification here changes for it.

[CRITICAL]: Rhino targets the Rhino 9 WIP lane on macOS; GH1 `.gha`, Rhino 8, and Windows are refused targets.

- Hosts earn no folder level; an app naming its host is a naming choice, never structure.
- Host assemblies resolve through shared build properties, and `Directory.Build.props` owns classification and the reference roster.

## [03]-[SUBSTRATE]

Root config estates reach every island already, so an app invents no configuration surface of its own.

[MSBUILD]:
- Root `Directory.Build.props` and `Directory.Build.targets` classify and configure every project by tree position.
- Nested estate files under an island open with the upward chaining import, whose canonical line rides a comment in `Directory.Build.props`.
- Omitting that import erases the whole root estate while the build stays green, and `tests/csharp/_architecture` refuses the file.

[PYTHON]:
- Root `pyproject.toml` owns resolution, dependency groups, and the one `uv.lock`; member manifests carry membership and bare-name edges.
- `tool.uv.workspace.members` admits `libs/python/*` by glob and each app project by explicit row, since a glob over polyglot trees hard-fails.

[TYPESCRIPT]:
- `pnpm-workspace.yaml` seats app packages through its `apps/*/*` glob, and its catalog holds every version.
- Project `tsconfig.json` extends `tsconfig.base.json` (`tsconfig.node.json` for node runtimes) and declares only `references`.
- Root `tsconfig.json` is the solution shell `tsc --build` drives, so a project absent from its `references` never typechecks in the estate sweep.

## [04]-[MINTING]

Blessed per-project file sets are minimal, and what the init rail emits is the whole set.

- `uv run assay init python-app apps/<app-name>/<project>` writes the manifest, the package seat, and its workspace member row.
- `uv run assay init check` closes uv's silent-orphan hole both ways, refusing an undeclared manifest on disk and a member row resolving to nothing.
- C# projects mint by hand as an identity-only `.csproj` joined through `dotnet sln add`, under the parity and shape guards.
- TypeScript projects mint by hand as `package.json` beside `tsconfig.json`, composed by the root presets.

## [05]-[VERIFICATION]

Every claim runs from the repo root on the `assay` rail and emits one JSON `Envelope`; `tools/assay/README.md` owns the claim and verb rosters.

- `uv run assay static --project <csproj>` grades a C# project, and `--folder <dir>` grades a Python or TypeScript project.
- `uv run assay docs check <paths>` grades markdown under an island.
- `uv run assay package plan` and `publish` drive Yak output, whose per-slug metadata lives at `tools/yak/<slug>/manifest.yml`.
