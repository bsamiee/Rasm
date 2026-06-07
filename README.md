# [RHINOWIP_PLUGIN_WORKSPACE]

RhinoWIP macOS workspace for first-party Rhino and Grasshopper products. Each app under `apps/grasshopper/<PluginName>` or `apps/rhino/<PluginName>` is a concrete plugin boundary; shared geometry capability lives in `libs/csharp` and stays product-neutral.

## Target

- RhinoWIP on macOS.
- `net10.0` for hosted plugins and shared C# projects.
- Grasshopper product surfaces through `Grasshopper2`.
- Yak package output for Mac only.
- No GH1 `.gha`, Windows target, Rhino 8 target, RhinoCode publishing path, or speculative Rhino command shell.

## Layout

| Path                           | Purpose                                                                                  |
| ------------------------------ | ---------------------------------------------------------------------------------------- |
| `apps/grasshopper/Radyab`      | Radyab GH2 `.rhp` plugin boundary.                                                       |
| `libs/csharp/Rasm`             | RhinoCommon-aware geometry domain + analysis algebra (merged).                           |
| `libs/csharp/Rasm.Grasshopper` | GH2 component infrastructure: typed parameter bindings, bridge plumbing, component base. |
| `tests/csharp`                 | Managed C# contract tests for shared libraries.                                          |
| `tools/cs-analyzer`            | Local Roslyn analyzer project used by C# builds.                                         |
| `tools/yak/<package>`          | Tracked Yak metadata for one package.                                                    |
| `tools/assay`                  | Typed quality operator: static, test, bridge, package, code, docs, and API rails.        |
| `tools/rhino-bridge`           | In-Rhino bridge plugin, client, and protocol.                                            |

## Build Policy

`Directory.Build.props` owns shared C# and RhinoWIP configuration.

| Property                           | Value                                                                       |
| ---------------------------------- | --------------------------------------------------------------------------- |
| `TargetFramework`                  | `net10.0`                                                                   |
| `RhinoWipAppPath`                  | `/Applications/RhinoWIP.app`                                                |
| `RhinoCommonReferencePath`         | Installed RhinoWIP `RhinoCommon.dll`                                        |
| `RhinoUiReferencePath`             | Installed RhinoWIP `Rhino.UI.dll`                                           |
| `EtoReferencePath`                 | Installed RhinoWIP Eto assembly.                                            |
| `SystemDrawingCommonReferencePath` | RhinoWIP-hosted `System.Drawing.Common.dll` for UI/raster boundaries.       |
| `Grasshopper2ReferencePath`        | Installed RhinoWIP `Grasshopper2.dll`                                       |
| `GrasshopperIoReferencePath`       | Installed RhinoWIP `GrasshopperIO.dll`                                      |
| `IsGrasshopperPluginProject`       | Enables `.rhp` output plus local Grasshopper2 and GrasshopperIO references. |
| `IsGrasshopperAwareProject`        | Enables Grasshopper2 and GrasshopperIO references without plugin output.    |
| `IsRhinoCommonAwareProject`        | Enables local RhinoCommon references.                                       |
| `IsRhinoUiAwareProject`            | Enables Rhino UI, Eto, and RhinoWIP-hosted drawing references.              |
| `UseWorkspaceLibraries`            | Enables the shared LanguageExt and Thinktecture package surface.            |

Plugin projects set plugin classification explicitly in their `.csproj`; build behavior does not depend on product names.

RhinoWIP currently hosts .NET 10 while installed McNeel assemblies can target older `.NETCoreApp` versions. Keep that compatibility detail in build properties and package validation, not product code.

## Commands

Run C# quality gates:

```bash copy-safe
uv run python -m tools.assay static fix
uv run python -m tools.assay static build
```

Build Rhino artifacts:

```bash copy-safe
uv run python -m tools.assay bridge build
```

Create the Mac Yak package:

```bash copy-safe
uv run python -m tools.assay package stage --slug radyab --version 0.1.0-wip
```

Deploy a package into RhinoWIP:

```bash copy-safe
uv run python -m tools.assay package deploy --slug radyab --version 0.1.0-wip
```

Build, install locally, then push to a Yak feed (one shot):

```bash copy-safe
uv run python -m tools.assay package publish --slug radyab --version 0.1.0-wip
```

Build and deploy the runtime analyzer bridge:

```bash copy-safe
VERSION=0.1.0-wip
uv run python -m tools.assay bridge build
uv run python -m tools.assay package stage --slug rasm-bridge --version "$VERSION"
uv run python -m tools.assay package deploy --slug rasm-bridge --version "$VERSION"
```

Collect live RhinoWIP runtime evidence:

```bash copy-safe
uv run python -m tools.assay bridge doctor
uv run python -m tools.assay bridge check
uv run python -m tools.assay bridge clean
uv run python -m tools.assay bridge verify --pattern path/to/scenario.verify.csx
```

## Artifact Flow

`uv run python -m tools.assay package stage --slug <package> --version <version>` performs one path:

1. Resolve one package project by `YakPackageSlug`.
2. Clear configured plugin output directories for the selected package.
3. Build the selected package project in `Release`.
4. Stage the configured plugin `.rhp` files and owned output assemblies for the selected package.
5. Copy `tools/yak/<package>/manifest.yml` and `tools/yak/<package>/icon.png`.
6. Run RhinoWIP Yak with `--platform mac` and the supplied version.

`uv run python -m tools.assay package deploy --slug <package> --version <version>` builds the same package, installs the local `.yak`, and refreshes RhinoWIP through the idempotent bridge launch path where that package requires it. `uv run python -m tools.assay package publish --slug <package> --version <version>` builds the same package, installs it locally, and pushes it with `YAK_SOURCE` when that environment variable is set.

Host assemblies stay outside package output: `RhinoCommon`, `Rhino.UI`, `Rhino.Runtime.Code`, `Grasshopper2`, `GrasshopperIO`, `Eto`, `Microsoft.macOS`, and `System.Drawing.Common`.

The generated Yak package root is `.artifacts/rhino/<package>/package`, with `manifest.yml` and plugin files at the top level required by Yak.

Package membership is evaluated from MSBuild. Projects under `apps/` or package-capable `tools/` roots opt in with `YakPackageSlug`; `Directory.Build.targets` derives manifest and stage paths.

## Runtime Notes

Grasshopper uses GH2 component APIs directly: `Grasshopper2.Components.Component`, `InputAdder`, `OutputAdder`, `IDataAccess`, and `GrasshopperIO.IoIdAttribute`.

GH2 can run component work in parallel. Component code must keep execution state local to `Process`; reusable geometry logic belongs in `Rasm.Analysis` and `Rasm.Domain`.

Automated Rhino/GH2 unit-test frameworks stay out of this foundation until Rhino.Testing exposes a current `net10.0` path. Live RhinoWIP runtime analyzer evidence flows through the installed bridge plugin, which registers RhinoCode C# scripting, runs transient scripts in-process, and returns factual build, diagnostic, stdout, stderr, exception, Rhino, document, tolerance, and bridge identity data. `rhinocode list --json`, `bridge doctor`, and endpoint metadata are discovery evidence; RhinoCode publishing remains out of scope. `bridge check <source.cs>` resolves and builds the owning project, then returns `unsupported` unless an executable scenario is supplied as the second positional argument.

Scenarios are source-only diagnostics. Library scenarios live under `tests/csharp/libs/<Project>/<MirrorPath>/scenarios/`. Do not add `#r`, `#load`, or absolute build-output paths; `bridge verify --pattern <scenario>` owns host-filtered reference projection, fresh artifact refs, and `SCENARIO_NAME` / `CAPTURE_PATH` injection.

## GH2 Foundation

`libs/csharp/Rasm.Grasshopper` provides the reusable component boundary for GH2 plugins:

- `PortKind` maps CLR types to native `Grasshopper2.Parameters.Standard` parameters and owns input/output adder delegates.
- `Port<T>` describes item, twig, and tree access plus requirement and parameter policy.
- `PortPolicy` applies native GH2 behavior for vectors, curves, surfaces, angles, optional ports, categories, and index semantics; `PortKind.Index` owns native index parameters.
- `ComponentSpec.Of` and `Component` keep plugin components as thin port and output declarations.
- `Bridge.Read<T>` uses `IDataAccess.GetPears<T>` and `GetTree<T>` to preserve metadata, null state, and topology.
- `Bridge.Write<T>` uses `SetPear`, `SetTwig<T>`, and `SetTree` with `Garden.TwigFromPears`, `Garden.TreeFromLeaves`, and `Garden.TreeFromPears`.
- `Output` keeps final GH2 side effects at the component boundary.

Add a new parameter type by extending `PortKind` with a static case that returns the native `InputAdder.Add{X}` and `OutputAdder.Add{X}` parameter instances. Port factories fall back to `PortKind.Generic` for unmapped CLR types; add typed mappings only when GH2 has a real native parameter.

To add a component, create static `Port<T>` and `OutputBinding` declarations, pass them to `ComponentSpec.Of`, and inherit `Component`. Prefer adding `PortPolicy` at the port declaration over local validation or conversion code.
