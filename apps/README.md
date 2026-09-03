# [APPS]

Each `apps/<app-name>/` holds one app's projects in every language it needs. App directories share nothing by position, an app consumes `libs/` like any external package, and the directory carries product intent, host binding, and output alone.

## [01]-[LAYOUT]

```text
apps/
└── <app-name>/
    ├── <Project>/          # C# project, joined to Workspace.slnx
    ├── <project>/          # Python project, joined as a uv workspace member
    └── <project>/          # TypeScript project, included by the pnpm apps/*/* glob
```

- App directories stay unrelated: apps couple only through a published package, never through a shared parent
- Language mix is an app decision, an app spans C#, Python, and TypeScript with a directory per project
- Path segments carry app identity alone, host and deployment come from project configuration
- Pulumi programs an app owns sit under the app directory with one stack per environment, and `infra/` holds the repository's own resources

## [02]-[HOSTS]

Rhino 9 and Grasshopper 2 are the current hosts. Blender or any later host joins by adding its boundary package under `libs/`, with no change to tree, glob, or classification here.

[CRITICAL]: Rhino work targets Rhino 9 on macOS, never GH1 `.gha`, Rhino 8, or Windows.

- Hosts get no folder level, an app naming its host is a naming choice, not structure
- `Directory.Build.props` owns project classification and the host assembly references

## [03]-[SHARED_CONFIGURATION]

Root configuration files reach every app directory, an app adds no configuration of its own.

[MSBUILD]:
- Root `Directory.Build.props` and `Directory.Build.targets` classify and configure every project by tree position
- Nested `Directory.Build.*` files under an app directory first import the parent file through `GetPathOfFileAbove`
- Without that import the root configuration is lost and the build still passes

[PYTHON]:
- Root `pyproject.toml` owns resolution, dependency groups, and `uv.lock`, and an app project's manifest carries bare-name dependencies

[TYPESCRIPT]:
- `pnpm-workspace.yaml` lists app packages through the `apps/*/*` glob and its catalog holds every version
- Project `tsconfig.json` extends `tsconfig.base.json` and declares only `references`, a node runtime adds `types: ["node"]`
- `typecheck` builds each project from its own `tsconfig.json`, and `^typecheck` runs the referenced projects first

## [04]-[PROJECT_CREATION]

Per-project file sets are minimal, the init command output is the whole set.

- C# projects are a minimal `.csproj` written by hand and listed in `Workspace.slnx`, checked by the project policy targets
- TypeScript projects are created by hand as `package.json` beside `tsconfig.json`, extending the root configuration
