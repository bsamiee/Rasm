# RhinoWIP Plugin Workspace

RhinoWIP macOS workspace for first-party Rhino and Grasshopper products. Each app under `apps/grasshopper/<PluginName>` or `apps/rhino/<PluginName>` is a concrete plugin boundary; shared geometry capability lives in `libs/csharp` and stays product-neutral.

## Target

- RhinoWIP on macOS.
- `net10.0` for hosted plugins and shared C# projects.
- Grasshopper product surfaces through `Grasshopper2`.
- Yak package output for Mac only.
- No GH1 `.gha`, Windows target, Rhino 8 target, RhinoCode publishing path, or speculative Rhino command shell.

## Layout

| Path | Purpose |
| ---- | ------- |
| `apps/grasshopper/Radyab` | Radyab GH2 `.rhp` plugin boundary. |
| `libs/csharp/Rasm` | RhinoCommon-aware geometry domain + analysis algebra (merged). |
| `libs/csharp/Rasm.Grasshopper` | GH2 component infrastructure: typed parameter bindings, bridge plumbing, component base. |
| `tests/csharp` | Managed C# contract tests for shared libraries. |
| `tools/cs-analyzer` | Local Roslyn analyzer project used by C# builds. |
| `tools/yak/<package>` | Tracked Yak metadata for one package. |
| `scripts/rhino.sh` | Build, stage, package, and push Rhino artifacts. |

## Build Policy

`Directory.Build.props` owns shared C# and RhinoWIP configuration.

| Property | Value |
| -------- | ----- |
| `TargetFramework` | `net10.0` |
| `RhinoWipAppPath` | `/Applications/RhinoWIP.app` |
| `RhinoCommonReferencePath` | Installed RhinoWIP `RhinoCommon.dll` |
| `Grasshopper2ReferencePath` | Installed RhinoWIP `Grasshopper2.dll` |
| `GrasshopperIoReferencePath` | Installed RhinoWIP `GrasshopperIO.dll` |
| `IsGrasshopperPluginProject` | Enables `.rhp` output plus local Grasshopper2 and GrasshopperIO references. |
| `IsGrasshopperAwareProject` | Enables Grasshopper2 and GrasshopperIO references without plugin output. |
| `IsRhinoCommonAwareProject` | Enables local RhinoCommon references. |
| `UseWorkspaceLibraries` | Enables the shared LanguageExt and Thinktecture package surface. |

Plugin projects set plugin classification explicitly in their `.csproj`; build behavior does not depend on product names.

RhinoWIP currently hosts .NET 10 while installed McNeel assemblies can target older `.NETCoreApp` versions. Keep that compatibility detail in build properties and package validation, not product code.

## Commands

Run C# quality gates:

```bash
pnpm check:cs
```

Build Rhino artifacts:

```bash
scripts/rhino.sh build
```

Create the Mac Yak package:

```bash
scripts/rhino.sh package radyab 0.1.0-wip
```

Push to the test Yak feed:

```bash
scripts/rhino.sh push-test radyab 0.1.0-wip
```

Push to the public Yak feed:

```bash
scripts/rhino.sh push radyab 0.1.0-wip
```

## Artifact Flow

`scripts/rhino.sh package <package> <version>` performs one path:

1. Restore `Workspace.slnx`.
2. Clear configured plugin output directories for the selected package.
3. Build `Workspace.slnx` in `Release`.
4. Stage the configured plugin `.rhp` files and owned output assemblies for the selected package.
5. Copy `tools/yak/<package>/manifest.yml` and `tools/yak/<package>/icon.png`.
6. Run RhinoWIP Yak with `--platform mac` and the supplied version.

Host assemblies stay outside package output: `RhinoCommon`, `Grasshopper2`, and `GrasshopperIO`.

The generated Yak package root is `.artifacts/rhino/<package>/package`, with `manifest.yml` and plugin files at the top level required by Yak.

Package-to-plugin membership is explicit in `scripts/rhino.sh`. The current package map stages `radyab` from `apps/grasshopper/Radyab`; future Grasshopper or Rhino plugins should add sibling app roots and one deliberate package mapping.

## Runtime Notes

Grasshopper uses GH2 component APIs directly: `Grasshopper2.Components.Component`, `InputAdder`, `OutputAdder`, `IDataAccess`, and `GrasshopperIO.IoIdAttribute`.

GH2 can run component work in parallel. Component code must keep execution state local to `Process`; reusable geometry logic belongs in `Rasm.Analysis` and `Rasm.Domain`.

Automated Rhino/GH2 unit-test frameworks stay out of this foundation until Rhino.Testing exposes a current `net10.0` path. Live RhinoWIP runtime evidence flows through `scripts/rhino.sh bridge check <project.csproj>` and explicit `bridge script <script.csx|.cs>` executions; RhinoCode publishing remains out of scope.

## GH2 Foundation

`libs/csharp/Rasm.Grasshopper` provides the reusable component boundary for GH2 plugins:

- `PortKind` maps CLR types to native `Grasshopper2.Parameters.Standard` parameters and owns input/output adder delegates.
- `Port<T>` describes item, twig, and tree access plus requirement and parameter policy.
- `PortPolicy` applies native GH2 behavior for vectors and angles; `PortKind.Index` owns native index parameters.
- `ComponentSpec.Of` and `Component` keep plugin components as thin port and output declarations.
- `Bridge.Read<T>` uses `IDataAccess.GetPears<T>` and `GetTree<T>` to preserve metadata, null state, and topology.
- `Bridge.Write<T>` uses `SetPear`, `SetTwig<T>`, and `SetTree` with `Garden.TwigFromPears`, `Garden.TreeFromLeaves`, and `Garden.TreeFromPears`.
- `Output` keeps final GH2 side effects at the component boundary.

The current API ledger is [`docs/rhino-gh2-api-ledger.md`](docs/rhino-gh2-api-ledger.md). It tracks RhinoCommon/GH2 APIs as used, underused, or intentionally unused against the installed RhinoWIP XML docs.

To add a new parameter type, extend `PortKind` with a static case that returns the native `InputAdder.Add{X}` and `OutputAdder.Add{X}` parameter instances. Port factories automatically fall back to `PortKind.Generic` for unmapped CLR types, so new typed mappings should be added only when GH2 has a real native parameter.

To add a component, create static `Port<T>` and `OutputGroup` declarations, pass them to `ComponentSpec.Of`, and inherit `Component`. Prefer adding `PortPolicy` at the port declaration over local validation or conversion code.
