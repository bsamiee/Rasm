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
| `libs/csharp/core` | Shared RhinoCommon-aware geometry context, validation, and result rails. |
| `libs/csharp/analysis` | RhinoCommon-native analysis query algebra for future components. |
| `tests/csharp` | Managed C# contract tests for shared libraries. |
| `tests/rhino` | Opt-in RhinoWIP runtime specs for shared Rhino behavior. |
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
| `RhinoYakPath` | `/Applications/RhinoWIP.app/Contents/Resources/bin/yak` |
| `IsGrasshopperPluginProject` | Enables `.rhp` output plus local Grasshopper2 and GrasshopperIO references. |
| `IsGrasshopperAwareProject` | Enables Grasshopper2 and GrasshopperIO references without plugin output. |
| `IsRhinoCommonAwareProject` | Enables local RhinoCommon references. |
| `IsSharedCSharpLibrary` | Marks shared C# libraries for the normal foundation package set. |

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

GH2 can run component work in parallel. Component code must keep execution state local to `Process`; reusable geometry logic belongs in `Core` and `Analysis`.

Python and RhinoCode publishing stay out of this foundation until the local runtime path is proven compatible.

## Adding a New Supported Geometry Type

Components inheriting `AnalysisComponent<TInput>` (with `where TInput : RhinoGeometry`) resolve CLR types to concrete `Grasshopper2.Parameters.Standard` implementations through the [`GeometryParameterKind`](libs/csharp/grasshopper/GeometryParameterKind.cs) SmartEnum. Each case closes over the typed `InputAdder.Add{X}` / `OutputAdder.Add{X}` method groups — adding a case is the single point of extension; an unmapped CLR type returns `Option<GeometryParameterKind>.None` from `From(Type)` rather than silently routing to a generic parameter.

Currently mapped: `Point` (`Point3d`), `Vector` (`Vector3d`), `Curve`, `Surface`, `Brep`, `Mesh`, `Box`, `Plane`, `Line`, `Circle`, `Arc`, `Sphere`, `SubD`, `Polyline`.

To add a new type (e.g. `Point2d`), append one `static readonly` field:

```csharp
public static readonly GeometryParameterKind Point2 = new(
    key: nameof(Point2),
    clrType: typeof(Point2d),
    addInput: static (InputAdder adder, string name, string code, string info, Access access, Requirement requirement) =>
        adder.AddPoint2(name: name, code: code, info: info, access: access, requirement: requirement),
    addOutput: static (OutputAdder adder, string name, string code, string info, Access access) =>
        adder.AddPoint2(name: name, code: code, info: info, access: access));
```

The lambdas must be `static` (no captured state). Thinktecture's `[SmartEnum<string>]` source generator exposes the new case via `GeometryParameterKind.Items` — `From<T>()` resolves it without any runtime registration.
