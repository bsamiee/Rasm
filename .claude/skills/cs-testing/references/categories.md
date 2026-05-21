# [H1][CATEGORIES]
>**Dictum:** *The native-managed boundary determines the rail; the rail determines the file.*

<br>

The Rasm monorepo has exactly two test rails. They share no infrastructure and run via different commands. Categorize correctly before authoring; mis-routing wastes both rails' budgets.

---
## [1][RAIL_MATRIX]

| [INDEX] | [RAIL]   | [LOCATION]                                                | [RUNNER]                              | [WHAT_RUNS_HERE]                                                                                                |
| :-----: | -------- | --------------------------------------------------------- | ------------------------------------- | --------------------------------------------------------------------------------------------------------------- |
|   [1]   | Static   | `tests/csharp/<project>/<MirrorPath>/<Source>.spec.cs`    | `bash scripts/test.sh`                | Pure C# logic: smart constructors, DU/SmartEnum cases, Fin/Validation rails, fold algebras, statistical math.   |
|   [2]   | Bridge   | `apps/<host>/<plugin>/Scenarios/<name>.verify.csx`        | `bash scripts/rhino.sh verify <glob>` | Anything reaching RhinoCommon native (Vector3d.IsTiny, Brep, NurbsCurve, viewport, model document, plugins).    |

---
## [2][PROJECT_LAYOUT]

```
tests/
└── csharp/
    ├── _testkit/                 # Rasm.TestKit — Spec + Gens; shared substrate
    │   ├── Rasm.TestKit.csproj
    │   ├── Spec.cs
    │   └── Gens.cs
    ├── cs-analyzer/              # Roslyn analyzer tests (existing)
    │   ├── CsAnalyzer.Tests.csproj
    │   └── ...
    └── Rasm/                     # Tests for libs/csharp/Rasm
        ├── Rasm.Tests.csproj
        ├── Vectors/
        │   └── Atoms.spec.cs
        └── Domain/
            └── Stats.spec.cs
```

[CRITICAL]:
- [ALWAYS] One test csproj per source csproj. `tests/csharp/Rasm/Rasm.Tests.csproj` tests `libs/csharp/Rasm/Rasm.csproj`. Future tests for `Rasm.Rhino` go to `tests/csharp/Rasm.Rhino/Rasm.Rhino.Tests.csproj`.
- [ALWAYS] Spec files mirror source path under their project root. `libs/csharp/Rasm/Domain/Stats.cs` → `tests/csharp/Rasm/Domain/Stats.spec.cs`.
- [ALWAYS] Add new csproj to `Workspace.slnx` under `/tests/csharp/<Project>/`. `Directory.Build.props` auto-applies the test SDK by path.
- [NEVER] Co-locate tests with production code (`libs/csharp/Rasm/Tests/`). Tests live exclusively under `tests/csharp/`.

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
Test:   `Rasm.Tests.Vectors` lives in `tests/csharp/Rasm/Vectors/Atoms.spec.cs`.

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
        <ProjectReference Include="../../../libs/csharp/Rasm/Rasm.csproj" />
        <ProjectReference Include="../_testkit/Rasm.TestKit.csproj" />
    </ItemGroup>
</Project>
```

`IsRhinoCommonAwareProject=true` is required when the test mentions any RhinoCommon type (`Point3d`, `Vector3d`, `RhinoMath`). Tests that don't touch RhinoCommon omit the flag.

`Directory.Build.props` auto-applies:
- `IsTestProject=true` by path (under `tests/csharp/`)
- `xunit.v3.mtp-off`, `Microsoft.NET.Test.Sdk`, `xunit.runner.visualstudio`, `CsCheck`, `coverlet.collector`
- Global usings: `Xunit`, `CsCheck`, LanguageExt prelude, Thinktecture
- Same analyzer + editorconfig as production code (no relaxed gates)
- RhinoCommon.dll copied to `bin/` when `IsRhinoCommonAwareProject=true`
- `xunit.runner.json` from repo root linked into the test output (shared runner config)

[CRITICAL] To test internal `Rasm.*` factories (e.g. `Direction.Of(Vector3d, double tolerance, Op?)`), the friend list in `Directory.Build.props` already includes `Rasm.Tests` and `Rasm.TestKit`. Adding tests for a NEW source project requires adding `<InternalsVisibleTo Include="<NewProject>.Tests" />` in that source project's friend block.

---
## [6][RUNNER_CONFIG_AND_PARALLELIZATION]
>**Dictum:** *One `xunit.runner.json` at the repo root; one `[CollectionDefinition]` per shared-state region.*

<br>

The shared `xunit.runner.json` (linked into every test project's output via `Directory.Build.props`) sets the v3 runtime contract:

```json
{
    "$schema": "https://xunit.net/schema/v3.0/xunit.runner.schema.json",
    "parallelAlgorithm": "conservative",
    "preEnumerateTheories": true,
    "longRunningTestSeconds": 30,
    "diagnosticMessages": false,
    "shadowCopy": false
}
```

- `parallelAlgorithm: "conservative"` — collections-in-parallel, tests-in-collection-sequential. Switch to `"aggressive"` only after profiling shows starvation.
- `preEnumerateTheories: true` — theory cases enumerate at discovery time, so test count is known before run starts.
- `longRunningTestSeconds: 30` — emits a warning for any test longer than 30s (covers CsCheck shrink storms).
- `shadowCopy: false` — required when tests load native libraries (RhinoCommon).

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
