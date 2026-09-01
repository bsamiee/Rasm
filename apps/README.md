# [APPS]

`apps/` is app-keyed: each `apps/<app-name>/` is one self-contained directory holding that app's own projects across whatever languages it needs. App directories share nothing by position, and an app composes `libs/` exactly as it takes an external package, an app directory carries product intent, host binding, and output alone.

## [01]-[LAYOUT]

```text
apps/
└── <app-name>/
    ├── <Project>/          # C# project, joined to Workspace.slnx
    ├── <project>/          # Python project, joined as a uv workspace member
    └── <project>/          # TypeScript project, included by the pnpm apps/*/* glob
```

- App directories stay unrelated: two apps couple only where one takes the other's published package, never through a shared parent
- Language mix is an app decision, and one app spans C#, Python, and TypeScript with one directory per project
- Path segments carry app identity alone; host reach and deployment shape ride project configuration

## [02]-[HOSTS]

The host set stays open. Rhino 9 and Grasshopper 2 are the bound hosts today, and Blender or any later host joins by landing its boundary package under `libs/` — no tree, glob, or classification here changes for it.

[CRITICAL]: Rhino work targets Rhino 9 on macOS; GH1 `.gha`, Rhino 8, and Windows are refused targets.

- Hosts earn no folder level; an app naming its host is a naming choice, never structure
- Host assemblies resolve through shared build properties, and `Directory.Build.props` owns classification and the host assembly references

## [03]-[SHARED_CONFIGURATION]

Root configuration files reach every app directory already, an app invents no configuration surface of its own.

[MSBUILD]:
- Root `Directory.Build.props` and `Directory.Build.targets` classify and configure every project by tree position
- Nested build files under an app directory open with the upward chaining import, whose canonical line rides a comment in `Directory.Build.props`
- Omitting that import erases the whole root configuration while the build stays green

[PYTHON]:
- Root `pyproject.toml` owns resolution, dependency groups, and the one `uv.lock`; member manifests carry membership and bare-name edges
- `tool.uv.workspace.members` includes `libs/python/*` by glob and each app project by explicit row, since a glob over polyglot trees hard-fails

[TYPESCRIPT]:
- `pnpm-workspace.yaml` seats app packages through its `apps/*/*` glob, and its catalog holds every version
- Project `tsconfig.json` extends `tsconfig.base.json` and declares only `references`; a node runtime adds `types: ["node"]`
- Root `tsconfig.json` is the solution shell `tsc --build` drives, a project absent from its `references` never typechecks in the workspace sweep

## [04]-[PROJECT_CREATION]

Per-project file sets are minimal, and what the init command emits is the whole set.

- C# projects are created by hand as an identity-only `.csproj` joined through `dotnet sln add`, under the parity and shape guards
- TypeScript projects are created by hand as `package.json` beside `tsconfig.json`, composed by the root presets
