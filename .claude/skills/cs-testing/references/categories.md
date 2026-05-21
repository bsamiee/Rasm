# [H1][CATEGORIES]
>**Dictum:** *The native-managed boundary determines the rail; the rail determines the file.*

<br>

The Rasm monorepo has exactly two test rails. They share no infrastructure and run via different commands. Categorize correctly before authoring; mis-routing wastes both rails' budgets.

---
## [1][RAIL_MATRIX]

| [INDEX] | [RAIL]   | [LOCATION]                                                       | [RUNNER]                              | [WHAT_RUNS_HERE]                                                                                                |
| :-----: | -------- | ---------------------------------------------------------------- | ------------------------------------- | --------------------------------------------------------------------------------------------------------------- |
|   [1]   | Static   | `tests/csharp/libs/<Library>/<MirrorPath>/<Source>.spec.cs`      | `bash scripts/test.sh`                | Pure C# logic: smart constructors, DU/SmartEnum cases, Fin/Validation rails, fold algebras, statistical math.   |
|   [2]   | Bridge   | `apps/<host>/<plugin>/Scenarios/<name>.verify.csx`               | `bash scripts/rhino.sh verify <glob>` | Anything reaching RhinoCommon native (Vector3d.IsTiny, Brep, NurbsCurve, viewport, model document, plugins).    |

---
## [2][PROJECT_LAYOUT]

```
tests/
├── csharp/
│   ├── _testkit/                       # Rasm.TestKit — Spec + Gens; shared substrate (IsTestKitProject)
│   │   ├── Rasm.TestKit.csproj
│   │   ├── Spec.cs
│   │   └── Gens.cs
│   └── libs/                           # One test csproj per source library
│       ├── Rasm/                       # Tests for libs/csharp/Rasm
│       │   ├── Rasm.Tests.csproj
│       │   ├── Vectors/
│       │   │   └── Atoms.spec.cs
│       │   └── Domain/
│       │       └── Stats.spec.cs
│       ├── Rasm.Grasshopper/           # placeholder — add csproj when first spec lands
│       └── Rasm.Rhino/                 # placeholder — add csproj when first spec lands
└── tools/                              # Tool tests — NOT in default test.sh run
    ├── ast-grep/                       # ast-grep fixtures (Python check:py uses these)
    ├── py_analyzer/                    # Python analyzer pytest
    └── cs-analyzer/                    # Roslyn analyzer C# tests — run via TEST_TARGET=tests/tools/cs-analyzer/CsAnalyzer.Tests.csproj bash scripts/test.sh
        └── CsAnalyzer.Tests.csproj
```

[CRITICAL]:
- [ALWAYS] One test csproj per source library under `tests/csharp/libs/<Library>/<Library>.Tests.csproj` tests `libs/csharp/<Library>/<Library>.csproj`. Per-lib keeps namespaces folder-matched (IDE0130 enforces `dotnet_style_namespace_match_folder=true`).
- [ALWAYS] Spec files mirror source path under their project root. `libs/csharp/Rasm/Domain/Stats.cs` → `tests/csharp/libs/Rasm/Domain/Stats.spec.cs`.
- [ALWAYS] Add new csproj to `Workspace.slnx` under `/tests/csharp/libs/<Library>/`. `Directory.Build.props` auto-applies the test SDK by path (TestsRoot covers `tests/csharp/`, ToolsTestRoot covers `tests/tools/`).
- [NEVER] Co-locate tests with production code (`libs/csharp/Rasm/Tests/`). Tests live exclusively under `tests/csharp/libs/` (library tests) or `tests/tools/` (tooling tests).
- [NEVER] Consolidate into a single `tests/csharp/Rasm.Tests.csproj` at the `tests/csharp/` root — the deep `libs/<X>/` folder hierarchy makes `dotnet_style_namespace_match_folder` fail unless you disable IDE0130 broadly. Per-lib avoids the trade-off.

---
## [3][RAIL_DECISION_PROCEDURE]
>**Dictum:** *If the test would touch RhinoCommon native, route to bridge.*

<br>

```
Source touches…                                | Static? | Bridge? | Notes
-----------------------------------------------|---------|---------|---------------------------------
Smart constructor (.TryCreate / .Validate)     |  ✓      |         | Pure validation
SmartEnum<T> / [Union] case access             |  ✓      |         | Constant readonly fields
Fin<T>.Bind / Match / Map composition          |  ✓      |         | LanguageExt is pure managed
Stat / Distribution computation                |  ✓      |         | Pure double math
Vector3d construction, .X/.Y/.Z, +/-/*         |  ✓      |         | Pure managed math
Vector3d.IsTiny / IsValid / Unitize            |         |  ✓      | UnsafeNativeMethods.*
Vector3d.VectorAngle                           |         |  ✓      | Native angle computation
RhinoMath.UnitScale                            |         |  ✓      | Native ON_GetUnitScale
Brep / NurbsCurve / Surface operations         |         |  ✓      | Geometry kernel native
GH2 component execution / IDataAccess          |         |  ✓      | Grasshopper2 runtime
ViewCapture / Display                          |         |  ✓      | Viewport rendering
```

When in doubt, write the static test first. If it fails at `System.IO.FileNotFoundException : Could not load file or assembly 'RhinoCommon'` or `UnsafeNativeMethods.*`, move it to the bridge rail without further fight.

---
## [4][NAMESPACE_DISCIPLINE]
>**Dictum:** *Test namespace mirrors test folder; collisions with source namespace require full qualification.*

<br>

Source: `Rasm.Vectors.Direction` lives in `libs/csharp/Rasm/Vectors/Atoms.cs`.
Test:   `Rasm.Tests.Vectors` lives in `tests/csharp/libs/Rasm/Vectors/Atoms.spec.cs`.

The folder structure satisfies IDE0130 namespace-folder match. The unqualified token `Direction` inside test code resolves ambiguously when both `Rasm.Vectors` and `Rasm.Tests.Vectors` are visible. **Fully qualify the source type** (`Rasm.Vectors.Direction.Of(...)`) when test code touches it, or introduce a using alias (`using SourceDirection = Rasm.Vectors.Direction;`). Do not rename the test namespace.

---
## [5][PROJECT_REFERENCES]
>**Dictum:** *Two ProjectReferences per test project: source under test, plus TestKit.*

<br>

```xml
<Project Sdk="Microsoft.NET.Sdk">
    <PropertyGroup>
        <AssemblyName>Rasm.Tests</AssemblyName>
        <RootNamespace>Rasm.Tests</RootNamespace>
        <IsRhinoCommonAwareProject>true</IsRhinoCommonAwareProject>
    </PropertyGroup>
    <ItemGroup>
        <ProjectReference Include="../../../../libs/csharp/Rasm/Rasm.csproj" />
        <ProjectReference Include="../../_testkit/Rasm.TestKit.csproj" />
    </ItemGroup>
</Project>
```

`IsRhinoCommonAwareProject=true` is required when the test mentions any RhinoCommon type (`Point3d`, `Vector3d`, `RhinoMath`). Tests that don't touch RhinoCommon omit the flag.

`Directory.Build.props` auto-applies:
- `IsTestProject=true` by path (under `tests/csharp/` or `tests/tools/`, excluding `tests/csharp/_testkit/` which is `IsTestKitProject=true`)
- Runnable tests get `Microsoft.NET.Test.Sdk`, `xunit.v3.mtp-off`, `xunit.runner.visualstudio`, `coverlet.msbuild`, plus `CsCheck`
- `Rasm.TestKit` (test-support library, not runnable) gets `xunit.v3.assert` + `CsCheck` — does NOT run via `dotnet test`
- Global usings: `Xunit`, `CsCheck`, LanguageExt prelude, Thinktecture (applied to test and testkit projects)
- Same analyzer + editorconfig as production code (no relaxed gates)
- RhinoCommon.dll copied to `bin/` when `IsRhinoCommonAwareProject=true`
- `xunit.runner.json` generated into `obj/` per project via a `GenerateXunitRunnerJson` target and linked into output as `xunit.runner.json` (no loose file at repo root; content sourced from `_XunitRunnerJsonContent` property in `Directory.Build.props`)
- Coverage settings via `coverlet.msbuild` MSBuild properties in `Directory.Build.props` "Coverage" PropertyGroup (`Include`/`Exclude`/`CoverletOutputFormat`/`CoverletOutput`/`SkipAutoProps`/`DeterministicReport`); opt in via `dotnet test /p:CollectCoverage=true`

[CRITICAL] To test internal `Rasm.*` factories (e.g. `Direction.Of(Vector3d, double tolerance, Op?)`), the friend list in `Directory.Build.props` already includes `Rasm.Tests` and `Rasm.TestKit`. Adding tests for a NEW source project requires adding `<InternalsVisibleTo Include="<NewProject>.Tests" />` in that source project's friend block.

---
## [6][RUNNER_CONFIG_AND_PARALLELIZATION]
>**Dictum:** *One `xunit.runner.json` at the repo root; one `[CollectionDefinition]` per shared-state region.*

<br>

Runtime config is generated at build time into `obj/xunit.runner.json` per test project and linked to the output directory. Source of truth: the `_XunitRunnerJsonContent` property in `Directory.Build.props`. Current content:

```json
{"$schema":"https://xunit.net/schema/v3.0/xunit.runner.schema.json","parallelAlgorithm":"conservative","preEnumerateTheories":true,"longRunningTestSeconds":30,"shadowCopy":false}
```

- `parallelAlgorithm: "conservative"` — collections-in-parallel, tests-in-collection-sequential. Switch to `"aggressive"` only after profiling shows starvation.
- `preEnumerateTheories: true` — theory cases enumerate at discovery time, so test count is known before run starts.
- `longRunningTestSeconds: 30` — emits a warning for any test longer than 30s (covers CsCheck shrink storms).
- `shadowCopy: false` — required when tests load native libraries (RhinoCommon).

To change runtime config: edit `_XunitRunnerJsonContent` in `Directory.Build.props`; do NOT reintroduce a loose `xunit.runner.json` at repo root.

**Cross-assembly parallelism** is `dotnet test`'s default — each csproj runs in its own process.

**Within-assembly parallelism** for shared-state regions (Rhino bridge handle, Roslyn workspace, native handles) is suppressed via:

```csharp
[CollectionDefinition("Bridge", DisableParallelization = true)]
public sealed class BridgeCollection;

[Collection("Bridge")]
public sealed class BridgeFacingLaws { … }
```

Or for an entire assembly: `[assembly: CollectionBehavior(DisableTestParallelization = true)]`.

**Process-wide one-time setup** uses v3-only `IAssemblyFixture<T>` plus `[assembly: AssemblyFixture(typeof(T))]`. Use for resources that must survive across test classes (bridge handle, expensive snapshot loads). Constructor parameter injection delivers the fixture; `IAsyncLifetime` / `IAsyncDisposable` handle setup/teardown.

[REFERENCE] Live config: [`xunit.runner.json`](../../../xunit.runner.json) at repo root; injection point: `Directory.Build.props` "Test Runtime Config" ItemGroup.
