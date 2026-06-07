# [BUILD_AND_PACKAGES]

This page owns C# build control planes, central package graph state, host references, global usings, tool packages, and package adoption gates. Package versions live in `Directory.Packages.props`; this page records ownership, reference mode, and coding route.

## [1][CONTROL_PLANES]

| [INDEX] | [OWNER]                     | [OWNS]                                  |
| :-----: | :-------------------------- | :-------------------------------------- |
|   [1]   | `Directory.Build.props`     | framework, language, globals, host refs |
|   [2]   | `Directory.Packages.props`  | central package versions                |
|   [3]   | `Directory.Build.targets`   | late build cleanup                      |
|   [4]   | `.editorconfig`             | analyzer severity and formatting        |
|   [5]   | `Workspace.slnx`            | solution graph                          |
|   [6]   | `global.json`               | MTP runner selector                     |
|   [7]   | `.config/dotnet-tools.json` | local .NET tools                        |

Do not duplicate package versions in concept pages. Concept pages state implementation policy; this page states graph and build admission.

## [2][ACTIVE_SURFACE]

Target framework: `net10.0`
Language version: `14.0`
Nullable: `enable`
Implicit usings: `enable`
Test runner selector: `global.json` selects `Microsoft.Testing.Platform`.

Analyzer failures are architecture pressure unless the analyzer is proven wrong.

## [3][PACKAGE_STATE]

| [INDEX] | [STATE]            | [MEANING]                                    |
| :-----: | :----------------- | :------------------------------------------- |
|   [1]   | `global`           | injected by `Directory.Build.props`          |
|   [2]   | `direct`           | referenced by a project                      |
|   [3]   | `conditioned`      | project-classifier injection                 |
|   [4]   | `central-reserved` | centrally admitted without project reference |
|   [5]   | `tool`             | local tool surface                           |
|   [6]   | `host-bundled`     | app-bundle assembly                          |
|   [7]   | `candidate`        | accepted adoption gate                       |

`candidate` rows are planning and admission records. Promote them to implementation guidance when the owning source, manifest, tool, host bundle, or project-local roadmap proves the capability route.

## [4][FUNCTIONAL_CORE]

LanguageExt:
    Package: `LanguageExt.Core`.
    State: `global`.
    Injection owner: `Directory.Build.props`.
    Policy owner: [rails and effects](../rails-and-effects.md).

Thinktecture:
    Package: `Thinktecture.Runtime.Extensions`.
    State: `global`.
    Injection owner: `Directory.Build.props`.
    Policy owner: [domain shapes](../domain-shapes.md).

Workspace globals:
    Condition: `UseWorkspaceLibraries=true` and not the local analyzer project.
    Includes: `LanguageExt`, `LanguageExt.Common`, `LanguageExt.Traits`, `LanguageExt.Effects`, `LanguageExt.Pretty`, `LanguageExt.Traits.Domain`, static `LanguageExt.Prelude`, and `Thinktecture`.

## [5][NUMERICS_GRAPH]

MathNet:
    Direct package: `MathNet.Numerics`.
    Graph closure: `MathNet.Numerics.FSharp`, `FSharp.Core`, and `FParsec`.
    State: `direct` for `MathNet.Numerics`; `central-reserved` for closure packages.
    Consumer: `libs/csharp/Rasm/Rasm.csproj`.
    Policy owner: [numeric algorithms](../numeric-algorithms.md).

CSparse:
    Package: `CSparse`.
    State: `direct`.
    Consumer: `libs/csharp/Rasm/Rasm.csproj`.
    Policy owner: [sparse factorization](../sparse-factorization.md).

Symbolics:
    Package: `MathNet.Symbolics`.
    State: `direct`.
    Consumer: `libs/csharp/Rasm/Rasm.csproj`.
    Gate: concept guidance starts only when production source owns formula parsing, transformation, evaluation, or compilation.

## [6][APP_UI_GRAPH]

App UI packages are direct references in `libs/csharp/Rasm.AppUi/Rasm.AppUi.csproj`. Local App UI architecture owns UI adoption details; this page owns package graph presence.

Rendering and UI framework:
    Packages: `Avalonia`, `Avalonia.Desktop`, `Avalonia.Controls.ColorPicker`, `Avalonia.Controls.DataGrid`, `Avalonia.Fonts.Inter`, `Xaml.Behaviors.Avalonia`, `DialogHost.Avalonia`, `bodong.Avalonia.PropertyGrid`, `Projektanker.Icons.Avalonia`, and `Projektanker.Icons.Avalonia.MaterialDesign`.

Reactive and data flow:
    Packages: `ReactiveUI`, `ReactiveUI.Avalonia`, `ReactiveUI.Validation`, `System.Reactive`, and `DynamicData`.

Drawing and charts:
    Packages: `SkiaSharp`, `SkiaSharp.HarfBuzz`, `SkiaSharp.NativeAssets.macOS`, `HarfBuzzSharp.NativeAssets.macOS`, `Svg.Controls.Skia.Avalonia`, and `LiveChartsCore.SkiaSharpView.Avalonia`.

## [7][TEST_GRAPH]

Runnable test projects:
    Packages: `xunit.v3.mtp-v2` and `coverlet.MTP`.
    Injection owner: `Directory.Build.props`.
    Policy owner: [testing](../testing/README.md).

Testkit projects:
    Packages: `xunit.v3.assert`, `xunit.v3.common`, `xunit.v3.extensibility.core`, and `CsCheck`.
    Injection owner: `Directory.Build.props`.
    Policy owner: [managed laws](../testing/managed-laws.md).

MTP platform packages:
    Packages: `Microsoft.Testing.Platform`, `Microsoft.Testing.Platform.MSBuild`, `Microsoft.Testing.Extensions.Telemetry`, and `Microsoft.Testing.Extensions.TrxReport.Abstractions`.
    State: central graph support for MTP test execution.

Specialized proof rails:
    Packages: `Verify.XunitV3`, `TngTech.ArchUnitNET.xUnitV3`, `BenchmarkDotNet`, and `SharpFuzz`.
    State: direct in their owning test surfaces.
    Policy owner: [testing](../testing/README.md).

Test usings:
    Condition: test or testkit projects.
    Includes: `Xunit` and `CsCheck`.

## [8][HOST_REFERENCES]

RhinoWIP references:
    Owner: `Directory.Build.props`.
    Includes: `RhinoCommon.dll`, GH2 assemblies, Eto assemblies, Microsoft.macOS bindings, and Rhino UI drawing assemblies from the app bundle.
    State: `host-bundled`.

System drawing:
    Owner: RhinoWIP app-bundle reference first.
    Package: `System.Drawing.Common`.
    State: `conditioned`.
    Gate: compile metadata only when forwarded types require it; runtime drawing claims need host proof.

Bridge scenarios:
    Owner: `tools/rhino-bridge`.
    Rule: compile-time global usings do not define staged `.verify.csx` runtime usings.

## [9][ANALYZERS_AND_TOOLS]

External analyzers:
    Packages: `Microsoft.VisualStudio.Threading.Analyzers` and `Meziantou.Analyzer`.
    Injection owner: `Directory.Build.props`.

Roslyn analyzer authoring:
    Packages: `Microsoft.CodeAnalysis.CSharp` and `Microsoft.CodeAnalysis.Analyzers`.
    Consumers: `tools/cs-analyzer` and analyzer tests.

Local analyzer:
    Owner: `tools/cs-analyzer`.
    Injection: non-exempt projects receive it as an analyzer reference.
    Exemption: local analyzer, analyzer tests, plugins, GH-aware projects, Rhino UI-aware projects, tests, testkit, and bridge surfaces opt out where the classifier requires it.

Local dotnet tools:
    Owner: `.config/dotnet-tools.json`.
    Tools: `dotnet-outdated-tool`, `dotnet-stryker`, and `ilspycmd`.

## [10][ADOPTION_GATES]

System tensors:
    Candidate: `System.Numerics.Tensors`.
    Adoption rule: add for measured tensor or `TensorPrimitives` code that beats the current MathNet or span owner.
    Proof route: benchmark rail plus platform package update.

Logging abstractions:
    Candidate: `Microsoft.Extensions.Logging.Abstractions`.
    Adoption rule: add when a runtime or host boundary adopts `[LoggerMessage]` or `ILogger`.
    Proof route: owning runtime or host surface plus central package update.

Rate limiting:
    Candidate: package or shared-framework adoption must be proven before a C# stack page names rate limiters as active package graph truth.
    Route: concurrency policy remains in [system APIs](system-apis.md) until a concrete package or framework owner exists.
