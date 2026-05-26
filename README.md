# RhinoWIP Plugin Workspace

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
| `scripts/rhino.sh`             | Build, package, publish, and run RhinoWIP bridge analyzer flows.                         |

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

```bash
bash scripts/check-cs.sh check
```

Build Rhino artifacts:

```bash
scripts/rhino.sh build
```

Create the Mac Yak package:

```bash
scripts/rhino.sh package radyab 0.1.0-wip
```

Deploy a package into RhinoWIP:

```bash
scripts/rhino.sh deploy radyab 0.1.0-wip
```

Push to a Yak feed:

```bash
scripts/rhino.sh push radyab 0.1.0-wip
```

Build and deploy the runtime analyzer bridge:

```bash
VERSION=0.1.0-wip
scripts/rhino.sh bridge build
scripts/rhino.sh package rasm-bridge "$VERSION"
scripts/rhino.sh deploy rasm-bridge "$VERSION"
```

Collect live RhinoWIP runtime evidence:

```bash
scripts/rhino.sh bridge doctor
scripts/rhino.sh bridge check path/to/project.csproj
scripts/rhino.sh bridge check path/to/source.cs path/to/scenario.verify.csx
scripts/rhino.sh bridge check path/to/script.csx
scripts/rhino.sh bridge clean path/to/project.csproj
scripts/rhino.sh verify path/to/scenario.verify.csx
```

## Artifact Flow

`scripts/rhino.sh package <package> <version>` performs one path:

1. Restore `Workspace.slnx`.
2. Clear configured plugin output directories for the selected package.
3. Build `Workspace.slnx` in `Release`.
4. Stage the configured plugin `.rhp` files and owned output assemblies for the selected package.
5. Copy `tools/yak/<package>/manifest.yml` and `tools/yak/<package>/icon.png`.
6. Run RhinoWIP Yak with `--platform mac` and the supplied version.

`scripts/rhino.sh deploy <package> <version>` builds the same package, installs the local `.yak`, and refreshes RhinoWIP through the bridge lifecycle path. `scripts/rhino.sh push <package> <version>` builds the same package and pushes it with `YAK_SOURCE` when that environment variable is set.

Host assemblies stay outside package output: `RhinoCommon`, `Rhino.UI`, `Rhino.Runtime.Code`, `Grasshopper2`, `GrasshopperIO`, `Eto`, `Microsoft.macOS`, and `System.Drawing.Common`.

The generated Yak package root is `.artifacts/rhino/<package>/package`, with `manifest.yml` and plugin files at the top level required by Yak.

Package membership is evaluated from MSBuild. Projects under `apps/` or package-capable `tools/` roots opt in with `YakPackageSlug`; `Directory.Build.targets` derives manifest and stage paths.

## Runtime Notes

Grasshopper uses GH2 component APIs directly: `Grasshopper2.Components.Component`, `InputAdder`, `OutputAdder`, `IDataAccess`, and `GrasshopperIO.IoIdAttribute`.

GH2 can run component work in parallel. Component code must keep execution state local to `Process`; reusable geometry logic belongs in `Rasm.Analysis` and `Rasm.Domain`.

Automated Rhino/GH2 unit-test frameworks stay out of this foundation until Rhino.Testing exposes a current `net10.0` path. Live RhinoWIP runtime analyzer evidence flows through the installed bridge plugin, which registers RhinoCode C# scripting, runs transient scripts in-process, and returns factual build, diagnostic, stdout, stderr, exception, Rhino, document, tolerance, and bridge identity data. `rhinocode list --json`, `bridge doctor`, and endpoint metadata are discovery evidence; RhinoCode publishing remains out of scope. `bridge check <source.cs>` resolves and builds the owning project, then returns `unsupported` unless an executable scenario is supplied as the second positional argument.

Scenarios are source-only diagnostics. Library scenarios live under `tests/csharp/libs/<Project>/<MirrorPath>/scenarios/`. Do not add `#r`, `#load`, or absolute build-output paths; `bridge check` owns host-filtered reference projection, fresh artifact refs, and `SCENARIO_NAME` / `CAPTURE_PATH` injection.

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
