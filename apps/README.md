# [APPS]

Each `apps/<app-name>/` holds one app's projects in every language it needs. App directories share nothing by position, an app consumes `libs/` like any external package, and the directory holds product intent, host binding, and output alone.

## [01]-[LAYOUT]

```text
apps/
└── <app-name>/
    ├── <Project>/              # C# project, joined to Workspace.slnx
    ├── <project>/              # Python project, resolved by the root pyproject.toml
    ├── <project>/              # TypeScript project, included by the pnpm apps/*/* glob
    ├── <App>.xcodeproj/        # Swift app project and shared scheme, icon document and asset catalog beside it
    └── <Area>/<Feature>.swift  # Swift files by responsibility, synchronized into the app target
```

- App directories stay unrelated: apps couple only through a published package, never through a shared parent
- Language mix is an app decision, an app spans C#, Python, TypeScript, and Swift with a manifest per project
- Path segments hold app identity alone, host and deployment come from project configuration
- Pulumi programs an app owns sit under the app directory with one stack per environment, and `infra/` holds the repository's own resources

## [02]-[HOSTS]

Rhino 9, Grasshopper 2, and macOS are the current hosts. Host APIs belong at app boundaries or in independently consumable packages under `libs/`.

[CRITICAL]: Rhino work targets Rhino 9 on macOS, never GH1 `.gha`, Rhino 8, or Windows.

- Hosts get no folder level, an app naming its host is a naming choice, not structure
- `Directory.Build.props` owns project classification and the host assembly references
- Xcode owns a macOS app's toolchain and SDK

## [03]-[SHARED_CONFIGURATION]

Root files own shared policy, project manifests own app configuration.

[MSBUILD]:
- Root `Directory.Build.props` and `Directory.Build.targets` classify and configure every project by tree position
- Nested `Directory.Build.*` files under an app directory import the parent file first, because MSBuild stops at the nearest one

[PYTHON]:
- Root `pyproject.toml` owns resolution, dependency groups, and `uv.lock`, and an app project's manifest holds bare-name dependencies

[TYPESCRIPT]:
- `pnpm-workspace.yaml` lists app packages through the `apps/*/*` glob and its catalog holds every version
- Project `tsconfig.json` extends `tsconfig.base.json` and holds its `outDir` under `.cache/` and its `types`
- `references` lists the projects a project depends on, `typecheck` builds it from its own `tsconfig.json`, and `^typecheck` runs them first

[SWIFT]:
- `.xcodeproj` files own their build settings: Swift version, warnings as errors, concurrency, deployment target, identity, plist keys, and signing
- Synchronized root folders include every source and resource under the project root, membership exceptions list the files the bundle leaves out
- `swift-format` runs at its defaults, with no configuration file

## [04]-[PROJECT_CREATION]

Projects are written by hand as the minimal file set, the set an init command produces.

- C# projects are a `.csproj` listed in `Workspace.slnx` and checked by the project policy targets
- TypeScript projects are a `package.json` beside a `tsconfig.json` that extends the root configuration
- Swift apps are an `.xcodeproj` with a shared scheme, the Info.plist generated from its settings
