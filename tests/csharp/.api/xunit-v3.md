# [xunit-v3] — the discovery, assertion, and MTP execution spine under every C# spec

The xunit.v3 family carries the whole C# proof estate: `xunit.v3.assert` is the assertion surface the kit gates throw through, `xunit.v3.extensibility.core` owns the fact/theory/collection attribute model, `xunit.v3.common` is their shared substrate, and `xunit.v3.mtp-v2` is the metapackage that turns each test project into a self-hosting Microsoft.Testing.Platform executable. `Directory.Build.props` injects the three sub-packages into `IsTestKitProject` and `mtp-v2` into `IsTestProject`, all `PrivateAssets="all"`, with a global `Using Include="Xunit"`; a csproj never re-wires them.

## [01]-[PACKAGE_SURFACE]

- package: `xunit.v3.assert` `3.2.2` / `xunit.v3.common` `3.2.2` / `xunit.v3.extensibility.core` `3.2.2` / `xunit.v3.mtp-v2` `3.2.2`
- license: `Apache-2.0`
- namespace: `Xunit` (assertions + attributes), `Xunit.Sdk` (exception and formatting internals), `Xunit.v3` (output plumbing)
- asset: `xunit.v3.assert.dll` (`netstandard2.0` + `net8.0`), `xunit.v3.common.dll`, `xunit.v3.core.dll` (both `netstandard2.0`); `mtp-v2` ships no assembly — it aggregates `xunit.v3.core.mtp-v2` (MSBuild entry-point generation + in-proc console runner), `xunit.v3.assert`, and `xunit.analyzers`
- rail: evidence — every spec, kit gate, architecture law, and snapshot verb executes on this spine

## [02]-[PUBLIC_TYPES]

| [INDEX] | [SYMBOL]                                                                           | [KIND]             | [CAPABILITY]                                                              |
| :-----: | :--------------------------------------------------------------------------------- | :----------------- | :------------------------------------------------------------------------ |
|  [01]   | `Assert`                                                                           | static partial     | the single assertion surface; equality, type, collection, throws, control |
|  [02]   | `FactAttribute` / `TheoryAttribute`                                                | attribute          | test discovery; skip/explicit/timeout policy per case                     |
|  [03]   | `InlineDataAttribute` / `MemberDataAttribute` / `ClassDataAttribute`               | attribute          | theory data sources                                                       |
|  [04]   | `TheoryData<...>` / `TheoryDataRow<...>`                                           | data carrier       | typed theory rows, 16 arities each                                        |
|  [05]   | `CollectionAttribute` / `CollectionAttribute<T>` / `CollectionDefinitionAttribute` | attribute          | collection grouping and per-collection parallelism opt-out                |
|  [06]   | `CollectionBehaviorAttribute`                                                      | assembly attribute | assembly parallelism policy: algorithm, max threads, disable              |
|  [07]   | `IClassFixture<T>` / `ICollectionFixture<T>` / `AssemblyFixtureAttribute`          | fixture            | class, collection, and assembly fixture tiers                             |
|  [08]   | `TraitAttribute` / `TestCaseOrdererAttribute`                                      | attribute          | trait tagging and case ordering                                           |
|  [09]   | `ITestOutputHelper` / `TestContext`                                                | service            | per-test output sink and ambient test state                               |
|  [10]   | `Xunit.Sdk.XunitException` family                                                  | exception          | typed assertion failures (`AllException`, `CollectionException`, ...)     |

## [03]-[ENTRYPOINTS]

| [INDEX] | [SURFACE]                                                                        | [KIND]    | [CAPABILITY]                                                                  |
| :-----: | :------------------------------------------------------------------------------- | :-------- | :---------------------------------------------------------------------------- |
|  [01]   | `Assert.Equal<T>(T?, T?)` + comparer/func/tolerance/span/unmanaged overloads     | assertion | equality across regimes; `Equal(double, double, double tolerance)` for floats |
|  [02]   | `Assert.True/False/Null/NotNull/Same/NotSame`                                    | assertion | boolean, null, and reference gates                                            |
|  [03]   | `Assert.Single/Contains/DoesNotContain/Empty/All/Collection/Distinct/Equivalent` | assertion | collection shape and membership gates                                         |
|  [04]   | `Assert.IsType<T>/IsNotType<T>/IsAssignableFrom<T>`                              | assertion | type gates returning the cast value                                           |
|  [05]   | `Assert.Throws<T>/ThrowsAny<T>/ThrowsAsync<T>`                                   | assertion | typed exception capture returning the exception                               |
|  [06]   | `Assert.InRange<T>(T, T, T)`                                                     | assertion | comparable range gate                                                         |
|  [07]   | `Assert.Fail(string?)` / `Assert.Multiple(params Action[])`                      | control   | explicit failure and aggregated multi-check                                   |
|  [08]   | `[Fact(Explicit = true)]`                                                        | discovery | explicit-only cases; run via `-- --explicit only`                             |

```csharp contract
public class FactAttribute : Attribute, IFactAttribute {
    public string? DisplayName { get; set; }
    public bool Explicit { get; set; }
    public string? Skip { get; set; }
    public Type[]? SkipExceptions { get; set; }
    public Type? SkipType { get; set; }
    public string? SkipUnless { get; set; }
    public string? SkipWhen { get; set; }
    public string? SourceFilePath { get; }
    public int? SourceLineNumber { get; }
    public int Timeout { get; set; }
}
public class TheoryAttribute : FactAttribute {
    public bool DisableDiscoveryEnumeration { get; set; }
    public bool SkipTestWithoutData { get; set; }
}
public sealed class CollectionBehaviorAttribute : Attribute {
    public bool DisableTestParallelization { get; set; }
    public int MaxParallelThreads { get; set; }
    public ParallelAlgorithm ParallelAlgorithm { get; set; }
}
public interface ITestOutputHelper {
    string Output { get; }
    void Write(string message);
    void WriteLine(string message);
}
```

## [04]-[IMPLEMENTATION_LAW]

[RUNNER_CONFIG]: the runner json is MSBuild-emitted, never a checked-in file — `Directory.Build.props` holds the `_XunitRunnerJsonContent` literal and `Directory.Build.targets` writes it to `$(IntermediateOutputPath)/xunit.runner.json` per test project.
- `parallelAlgorithm: "conservative"` — caps concurrently scheduled threads instead of oversubscribing when tests block.
- `preEnumerateTheories: true` — each theory row discovers as its own case.
- `longRunningTestSeconds: 30` — diagnostic notification past the wall-clock threshold.
- `printMaxEnumerableLength: 64` / `printMaxObjectDepth: 4` — failure-message formatting caps.

[MTP_BRIDGE]: `xunit.v3.mtp-v2` is a pure metapackage; its transitive `xunit.v3.core.mtp-v2` carries the `buildTransitive` props/targets that generate the MTP entry point, so `UseMicrosoftTestingPlatformRunner=true` + `OutputType=Exe` compile every suite into a self-hosting MTP executable. The MTP dependency floor it declares floats up to the centrally pinned Testing.Platform stack. `GenerateTestingPlatformEntryPoint`-family properties are scrubbed from transitive project references by the estate's reference-isolation `ItemDefinitionGroup`.

[FIXTURES]: fixtures route through `IClassFixture<T>`/`ICollectionFixture<T>`, `AssemblyFixtureAttribute`, and the `CollectionAttribute<T>`/`CollectionDefinitionAttribute` pairing; all three tiers ship in `xunit.v3.core.dll`.

[STACKING]:
- `CsCheck` (`cscheck.md`): no xunit dependency edge; property failures throw inside `[Fact]` bodies and surface as failed tests through the kit `Spec` gates.
- `Verify.XunitV3` (`verify.md`): depends on `xunit.v3.extensibility.core`; binds the snapshot verb to the v3 discovery model.
- `TngTech.ArchUnitNET.xUnitV3` (`archunitnet.md`): depends on `xunit.v3.assert`; architecture rules sink failures through `Assert`.
- `Avalonia.Headless.XUnit` (`libs/csharp/Rasm.AppUi/.api/api-headless.md`): depends on `xunit.v3.extensibility.core`; `[AvaloniaTest]` derives from the v3 fact model.
- `coverlet.MTP` (`coverlet-mtp.md`): no xunit edge; attaches at the MTP extension layer beside the mtp-v2 bridge.
- `Microsoft.Testing.Platform` stack (`testing-platform.md`): the execution host the mtp-v2 entry point registers into.

[LOCAL_ADMISSION]:
- Test and kit projects receive the family through the `Directory.Build.props` classifier rows; a csproj adding its own xunit reference is the named defect.
- Assertion access outside kit gates is unconstrained; kit `Spec`/`Approx` owners wrap the float and rail regimes so specs never hand-roll tolerance logic.

[RAIL_LAW]:
- Package: `xunit.v3.*`
- Owns: discovery, assertion vocabulary, parallelism policy, and the MTP entry-point bridge for every C# spec.
- Accept: `[Fact]`/`[Theory]` specs composing kit gates; `Explicit = true` for hygiene walks; assembly-level parallelism policy via `CollectionBehaviorAttribute`.
- Reject: a second assertion library, checked-in `xunit.runner.json` files, or per-csproj runner wiring.
