# [APPS]

Each `apps/<app-name>/` holds one app's projects in every language it needs. App directories share nothing by position, an app consumes `libs/` like any external package, and the directory holds product intent, host binding, and output alone.

## [01]-[LAYOUT]

```text
apps/
└── <app-name>/
    ├── <Project>/          # C# project, joined to Workspace.slnx
    ├── <project>/          # Python project, resolved by the root `pyproject.toml`
    └── <project>/          # TypeScript project, included by the pnpm apps/*/* glob
```

- App directories stay unrelated: apps couple only through a published package, never through a shared parent
- Language mix is an app decision, an app spans C#, Python, and TypeScript with a directory per project
- Path segments hold app identity alone, host and deployment come from project configuration
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
- Root `pyproject.toml` owns resolution, dependency groups, and `uv.lock`, and an app project's manifest holds bare-name dependencies

[TYPESCRIPT]:
- `pnpm-workspace.yaml` lists app packages through the `apps/*/*` glob and its catalog holds every version
- Project `tsconfig.json` extends `tsconfig.base.json` and holds its `outDir` under `.cache/typescript/out/<project path>` and its `types`
- `references` lists the projects a project depends on, `typecheck` builds it from its own `tsconfig.json`, and `^typecheck` runs them first

## [04]-[PROJECT_CREATION]

Projects are written by hand as the minimal file set, the set an init command produces.

- C# projects are a `.csproj` listed in `Workspace.slnx` and checked by the project policy targets
- TypeScript projects are a `package.json` beside a `tsconfig.json` that extends the root configuration
