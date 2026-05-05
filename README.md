# Rasm

RhinoWIP macOS foundation for Rasm plugins. `Rasm.Rhino` loads as a Rhino `.rhp`; `Rasm.Grasshopper` loads as a Grasshopper plugin backed by McNeel GH2 APIs; `Core` holds shared C# logic without Rhino or Grasshopper references.

## Target

- RhinoWIP on macOS.
- `net10.0` for RhinoWIP-hosted and RhinoWIP-consumed C# projects.
- Grasshopper product surface, implemented with `Grasshopper2`.
- Yak package output for Mac only.
- No GH1 `.gha`, Windows target, Rhino 8 target, or RhinoCode publishing path.

## Layout

| Path                       | Purpose                                                        |
| -------------------------- | -------------------------------------------------------------- |
| `libs/csharp/core`         | Shared C# logic consumed by Rhino and Grasshopper boundaries.  |
| `libs/python/<package>`    | Future shared Python packages when Python runtime code exists. |
| `apps/rhino`               | RhinoWIP `.rhp` plugin boundary.                               |
| `apps/grasshopper`         | GH2-backed Grasshopper `.rhp` plugin boundary.                 |
| `tools/cs-analyzer`        | Local Roslyn analyzer project used by C# builds.               |
| `tools/py_analyzer`        | Local Python semantic analyzer invoked by `pnpm check:py`.     |
| `tools/yak/manifest.yml`   | Tracked Yak package metadata for the aggregate `rasm` package. |
| `scripts/rhino.sh`         | Build, stage, package, and push Rhino artifacts.               |
| `.artifacts/rhino/package` | Generated Yak package root used by `yak build`.                |

## Central Values

`Directory.Build.props` owns shared build policy and RhinoWIP variables:

| Property                         | Value                                                                              |
| -------------------------------- | ---------------------------------------------------------------------------------- |
| `TargetFramework`                | `net10.0`                                                                          |
| `RhinoWipAppPath`                | `/Applications/RhinoWIP.app`                                                       |
| `RhinoCommonReferencePath`       | Installed RhinoWIP `RhinoCommon.dll`                                               |
| `Grasshopper2ReferencePath`      | Installed RhinoWIP `Grasshopper2.dll`                                              |
| `GrasshopperIoReferencePath`     | Installed RhinoWIP `GrasshopperIO.dll`                                             |
| `RhinoYakPath`                   | `/Applications/RhinoWIP.app/Contents/Resources/bin/yak`                            |
| `LocalCSharpAnalyzerRoot`        | `tools/cs-analyzer`                                                                |
| `LocalCSharpAnalyzerProject`     | `tools/cs-analyzer/CsAnalyzer.csproj`                                              |
| `BoundaryExemptionContractsPath` | Local analyzer boundary exemption contract source.                                 |
| `IsRhinoPluginProject`           | Enables Rhino `.rhp` settings and local `RhinoCommon`.                             |
| `IsGrasshopperPluginProject`     | Enables Grasshopper `.rhp` settings plus local `Grasshopper2` and `GrasshopperIO`. |

RhinoWIP currently runs on .NET 10. McNeel still builds `RhinoCommon`, `Grasshopper2`, and `GrasshopperIO` as `.NETCoreApp,Version=v8.0` assemblies; those assemblies are referenced from the installed app and loaded by the newer host runtime.

## Commands

Run C# quality gates:

```bash
pnpm check:cs
```

Run Python quality gates:

```bash
pnpm check:py
```

Build Rhino artifacts:

```bash
scripts/rhino.sh build
```

Create Mac Yak package:

```bash
scripts/rhino.sh package 0.1.0-wip
```

Push package to test Yak feed:

```bash
scripts/rhino.sh push-test 0.1.0-wip
```

Push package to public Yak feed:

```bash
scripts/rhino.sh push 0.1.0-wip
```

## Artifact Flow

`scripts/rhino.sh package <version>` performs one path:

1. Restore `Workspace.slnx`.
2. Build `Workspace.slnx` in `Release`.
3. Stage `Rasm.Rhino.rhp`, `Rasm.Grasshopper.rhp`, and owned output assemblies.
4. Copy `tools/yak/manifest.yml` and `tools/yak/icon.png`.
5. Run RhinoWIP Yak with `--platform mac` and supplied version.

Host assemblies stay outside package output: `RhinoCommon`, `Grasshopper2`, and `GrasshopperIO`.

`tools/yak` is only the tracked source of package metadata. The actual Yak package root is generated at `.artifacts/rhino/package` so `.rhp` files and `manifest.yml` sit at the top level required by Yak.

Yak inspects the first `.rhp` and warns that content name `Rasm.Rhino` differs from package name `rasm`. Keep `rasm` as the aggregate package name unless the package is split.

## Runtime Notes

Grasshopper uses GH2 component APIs: `Grasshopper2.Components.Component`, `InputAdder`, `OutputAdder`, `IDataAccess`, and `GrasshopperIO.IoIdAttribute`.

GH2 can run component work in parallel. Component code keeps execution state local to `Process` and moves reusable logic into `Core`.

Python and RhinoCode publishing stay out of this foundation until RhinoWIP `rhinocode` resolves its local runtime mismatch.
