# [DOTNET_TESTING_API_XUNIT_V3]

xunit.v3 packages carry the whole .NET proof tree: `xunit.v3.assert` is the assertion surface the kit gates throw through, `xunit.v3.extensibility.core` owns the fact/theory/collection attribute model, `xunit.v3.common` is their shared substrate, and `xunit.v3.mtp-v2` is the metapackage that turns each test project into a self-hosting Microsoft.Testing.Platform executable. Every suite csproj declares `xunit.v3.mtp-v2` itself and `Rasm.TestKit.csproj` declares `xunit.v3.assert` and `xunit.v3.extensibility.core`; the root `Directory.Build.targets` derives the global `Using Include="Xunit"` from that declared reference.

## [01]-[PUBLIC_TYPES]

| [INDEX] | [SYMBOL]                                         | [KIND]             | [CAPABILITY]                                                   |
| :-----: | :----------------------------------------------- | :----------------- | :------------------------------------------------------------- |
|  [01]   | `Assert`                                         | static partial     | single surface: equality, type, collection, throws, control    |
|  [02]   | `FactAttribute` / `TheoryAttribute`              | attribute          | test discovery; skip/explicit/timeout policy per case          |
|  [03]   | `InlineDataAttribute` / `MemberDataAttribute`    | attribute          | inline and member theory data                                  |
|  [04]   | `ClassDataAttribute`                             | attribute          | class-typed theory data source                                 |
|  [05]   | `TheoryData<...>` / `TheoryDataRow<...>`         | data carrier       | typed theory rows, 16 arities each                             |
|  [06]   | `CollectionAttribute` / `CollectionAttribute<T>` | attribute          | collection grouping and parallelism opt-out                    |
|  [07]   | `CollectionDefinitionAttribute`                  | attribute          | collection definition with fixture binding                     |
|  [08]   | `ParallelizationAttribute`                       | assembly attribute | assembly parallelism policy: `Mode`, `MaxThreads`, `Algorithm` |
|  [09]   | `IClassFixture<T>` / `ICollectionFixture<T>`     | fixture            | class and collection fixture tiers                             |
|  [10]   | `AssemblyFixtureAttribute`                       | fixture            | assembly-wide fixture tier                                     |
|  [11]   | `TraitAttribute` / `TestCaseOrdererAttribute`    | attribute          | trait tagging and case ordering                                |
|  [12]   | `ITestOutputHelper` / `TestContext`              | service            | per-test output sink and ambient test state                    |
|  [13]   | `Xunit.Sdk.XunitException` family                | exception          | typed failures: `AllException`/`CollectionException`/...       |

## [02]-[ENTRYPOINTS]

| [INDEX] | [SURFACE]                                                   | [KIND]    | [CAPABILITY]                                                   |
| :-----: | :---------------------------------------------------------- | :-------- | :------------------------------------------------------------- |
|  [01]   | `Assert.Equal<T>(T?, T?)`                                   | assertion | equality with comparer/func/tolerance/span/unmanaged overloads |
|  [02]   | `Assert.Equal(double, double, double tolerance)`            | assertion | float equality within a tolerance                              |
|  [03]   | `Assert.True/False/Null/NotNull/Same/NotSame`               | assertion | boolean, null, and reference gates                             |
|  [04]   | `Assert.Single/Contains/DoesNotContain/Empty`               | assertion | presence and emptiness gates                                   |
|  [05]   | `Assert.All/Collection/Distinct/Equivalent`                 | assertion | per-item, shape, distinctness, and equivalence gates           |
|  [06]   | `Assert.IsType<T>/IsNotType<T>/IsAssignableFrom<T>`         | assertion | type gates returning the cast value                            |
|  [07]   | `Assert.Throws<T>/ThrowsAny<T>/ThrowsAsync<T>`              | assertion | typed exception capture returning the exception                |
|  [08]   | `Assert.InRange<T>(T, T, T)`                                | assertion | comparable range gate                                          |
|  [09]   | `Assert.Fail(string?)` / `Assert.Multiple(params Action[])` | control   | explicit failure and aggregated multi-check                    |
|  [10]   | `[Fact(Explicit = true)]`                                   | discovery | explicit-only cases; run via `--explicit only`                 |

```csharp
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
public sealed class ParallelizationAttribute : Attribute {
    public ParallelAlgorithm Algorithm { get; set; }
    public int MaxThreads { get; set; }
    public ParallelizationMode Mode { get; set; }
}
public interface ITestOutputHelper {
    string Output { get; }
    void Write(string message);
    void WriteLine(string message);
}
```

## [03]-[IMPLEMENTATION_LAW]

[RUNNER_CONFIG]: no `xunit.runner.json` exists in the repo; the runner's defaults (`parallelAlgorithm: conservative`, theory pre-enumeration, diagnostic caps) stand, and any per-suite deviation lands as a checked-in `xunit.runner.json` `Content` item in that suite alone. Result writers (`--report-xunit-trx`, `--report-xunit-html`, `--report-xunit-xml`, `--report-xunit-junit`, `--report-xunit-nunit`, `--report-xunit-ctrf`) ship inside `xunit.v3.core.mtp-v2`, so no separate TRX extension is admitted. `testconfig.json` is the MTP-mode configuration surface; its relative paths resolve against the working directory, never the test app, so no checked-in copy routes results.

[MTP_BRIDGE]: `xunit.v3.mtp-v2` is a pure metapackage; its transitive `xunit.v3.core.mtp-v2` carries the `buildTransitive` props/targets that generate the entry point and set `IsTestingPlatformApplication`. `OutputType=Exe` is the one hard requirement; `UseMicrosoftTestingPlatformRunner=true` makes MTP the default host of the generated `Main`, and without it the bare executable routes to xunit's console runner and MTP answers only `--server`/`--internal-msbuild-node`. `IsTestProject` is a VSTest property xunit removed and no suite declares it.

[FIXTURES]: fixtures route through `IClassFixture<T>`/`ICollectionFixture<T>`, `AssemblyFixtureAttribute`, and the `CollectionAttribute<T>`/`CollectionDefinitionAttribute` pairing; all three tiers ship in `xunit.v3.core.dll`.

[STACKING]:
- `CsCheck` (`cscheck.md`): no xunit dependency edge; property failures throw inside `[Fact]` bodies and surface as failed tests through the kit `Spec` gates.
- `Avalonia.Headless.XUnit` (`libs/dotnet/Rasm.AppUi/.api/api-headless.md`): depends on `xunit.v3.extensibility.core`; `[AvaloniaTest]` derives from the v3 fact model.
- `coverlet.MTP` (`coverlet-mtp.md`): no xunit edge; attaches at the MTP extension layer beside the mtp-v2 bridge.
- `Microsoft.Testing.Platform` stack (`testing-platform.md`): the execution host the mtp-v2 entry point registers into.

[LOCAL_ADMISSION]:
- Each suite declares the family in its own csproj; a second owner injecting it is the named defect.
- Assertion access outside kit gates is unconstrained; kit `Spec`/`Approx` owners wrap the float and result regimes so specs never hand-roll tolerance logic.
