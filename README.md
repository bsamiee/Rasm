# Rasm

RhinoWIP macOS foundation for Rasm plugins. `Rasm.Rhino` loads as a Rhino `.rhp`; `Rasm.Grasshopper` loads as a GH2-backed Grasshopper `.rhp`; `Core` is the shared C# assembly for Rasm logic without RhinoCommon or Grasshopper references.

## Target

- RhinoWIP on macOS.
- `net10.0` for RhinoWIP-hosted plugins and shared C# projects.
- Grasshopper product surface, implemented with `Grasshopper2`.
- Yak package output for Mac only.
- No GH1 `.gha`, Windows target, Rhino 8 target, or RhinoCode publishing path.

## Layout

| Path                       | Purpose                                                        |
| -------------------------- | -------------------------------------------------------------- |
| `libs/csharp/core`         | Shared C# foundation used by Rhino and Grasshopper boundaries. |
| `libs/python/<package>`    | Future shared Python packages when Python runtime code exists. |
| `apps/rhino`               | RhinoWIP `.rhp` plugin boundary and RhinoCommon metadata.      |
| `apps/grasshopper`         | GH2-backed Grasshopper `.rhp` product boundary.                |
| `tools/cs-analyzer`        | Local Roslyn analyzer project used by C# builds.               |
| `tools/py_analyzer`        | Local Python semantic analyzer invoked by `pnpm check:py`.     |
| `tools/yak/manifest.yml`   | Tracked Yak package metadata aligned with the Rhino plug-in.   |
| `scripts/rhino.sh`         | Build, stage, package, and push Rhino artifacts.               |
| `.artifacts/rhino/package` | Generated Yak package root used by `yak build`.                |

`apps/rhino` and `apps/grasshopper` are settled app roots for the current outputs. `Rasm.Rhino.csproj` and `Rasm.Grasshopper.csproj` keep the `Rasm.` prefix because those names carry plugin/runtime identity. `Core.csproj` stays under `libs/csharp/core` as the first shared assembly; add more shared C# projects only for a real dependency, target-framework, package, or plugin-loading boundary.

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
| `IsSharedCSharpLibrary`          | Marks `Core` as a shared C# library with the normal foundation package set.         |

RhinoWIP currently runs on .NET 10. McNeel still builds `RhinoCommon`, `Grasshopper2`, and `GrasshopperIO` as `.NETCoreApp,Version=v8.0` assemblies; those assemblies are referenced from the installed app and loaded by the newer host runtime.

Plugin projects stay lean boundary assemblies and reference RhinoWIP host assemblies from `/Applications/RhinoWIP.app`. `Core` receives the workspace C# foundation libraries from `Directory.Build.props` so shared logic can use LanguageExt, Thinktecture, NodaTime, Polly, observability, and persistence packages without reworking project structure.

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

Run the Rhino status command after the Rhino plug-in is loaded:

```text
RasmStatus
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

Yak package metadata uses `Rasm.Rhino` so RhinoWIP Yak's inspected content name matches the manifest while still packaging both app boundaries.

## Runtime Notes

Grasshopper uses GH2 component APIs directly: `Grasshopper2.Components.Component`, `InputAdder`, `OutputAdder`, `IDataAccess`, and `GrasshopperIO.IoIdAttribute`.

GH2 can run component work in parallel. Component code keeps execution state local to `Process` and moves reusable logic into `Core`.

Both plug-in boundaries use RhinoCommon persistence and diagnostics directly. `PlugIn.Settings` stores the current runtime target and surface when the status entry points run, `SettingsDirectory` anchors user-profile files, and `RhinoApp.WriteLine` reports plug-in load messages plus Rhino command status to the command window. The plug-ins do not change Rhino load timing.

Python and RhinoCode publishing stay out of this foundation until RhinoWIP `rhinocode` resolves its local runtime mismatch.
