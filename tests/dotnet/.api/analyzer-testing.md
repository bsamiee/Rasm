# [DOTNET_TESTING_API_ANALYZER_TESTING]

`Microsoft.CodeAnalysis.CSharp.Analyzer.Testing` runs a `DiagnosticAnalyzer` against markup-annotated or explicitly-located sources inside a synthetic solution and verifies the exact diagnostic set. `DefaultVerifier` carries no xunit dependency, so the harness is verifier-neutral and composes cleanly with xunit.v3, threading `TestContext.Current.CancellationToken` through `RunAsync`. Generator specs deliberately bypass it: the estate drives `CSharpGeneratorDriver` directly with incremental-step tracking and asserts the emitted syntax structurally.

## [01]-[PUBLIC_TYPES]

| [INDEX] | [SYMBOL]                                       | [KIND]      | [CAPABILITY]                                                          |
| :-----: | :--------------------------------------------- | :---------- | :-------------------------------------------------------------------- |
|  [01]   | `CSharpAnalyzerTest<TAnalyzer, TVerifier>`     | harness     | the C# analyzer run: parse/compilation options, single-analyzer bind  |
|  [02]   | `CSharpAnalyzerVerifier<TAnalyzer, TVerifier>` | facade      | static `Diagnostic()` builders and `VerifyAnalyzerAsync`              |
|  [03]   | `AnalyzerTest<TVerifier>`                      | base        | abstract base; the configurable run surface and its expectations      |
|  [04]   | `SolutionState` / `ProjectState`               | state       | the synthetic-solution source and reference input model               |
|  [05]   | `DiagnosticResult`                             | expectation | fluent `WithSpan/WithLocation/WithArguments/WithSeverity` rows        |
|  [06]   | `ReferenceAssemblies` / `PackageIdentity`      | references  | TFM presets (`Net100` -> `Microsoft.NETCore.App.Ref`) + NuGet restore |
|  [07]   | `DefaultVerifier`                              | verifier    | the shipped `IVerifier`; xunit-free failure surfacing                 |
|  [08]   | `TestFileMarkupParser`                         | markup      | position, span, and named-span markup grammar over `TestCode`         |

- [03]-[BASE]: `TestCode`, `TestState`, `ExpectedDiagnostics`, `ReferenceAssemblies`, `SolutionTransforms`, `RunAsync`.
- [04]-[STATE]: `Sources`, `GeneratedSources`, `AdditionalFiles`, `AnalyzerConfigFiles`, `AdditionalReferences`.

## [02]-[ENTRYPOINTS]

| [INDEX] | [SURFACE]                                                         | [KIND]         | [CAPABILITY]                                      |
| :-----: | :---------------------------------------------------------------- | :------------- | :------------------------------------------------ |
|  [01]   | `new CSharpAnalyzerTest<…>{ … }.RunAsync(ct)`                     | harness        | the whole analyzer verification run               |
|  [02]   | `test.TestState.Sources.Add((path, content))`                     | state          | multi-file solution sources                       |
|  [03]   | `test.TestState.AnalyzerConfigFiles.Add(...)`                     | state          | editorconfig-driven rule configuration            |
|  [04]   | `test.ExpectedDiagnostics.Add(DiagnosticResult.CompilerError(…))` | expectation    | diagnostic set; `CompilerDiagnostics` widens it   |
|  [05]   | `new ReferenceAssemblies(tfm, id, path).AddPackages(…)`           | references     | compiled-against surface, NuGet-restored per TFM  |
|  [06]   | `TestBehaviors.SkipGeneratedCodeCheck`                            | policy         | skip generated-code diagnostics                   |
|  [07]   | `MarkupOptions.TreatPositionIndicatorsAsCode`                     | policy         | treat position indicators as code                 |
|  [08]   | `CSharpGeneratorDriver.Create(generator, driverOptions)`          | generator lane | cache-reason assertions over `TrackedOutputSteps` |

```csharp
public class CSharpAnalyzerTest<TAnalyzer, TVerifier> : AnalyzerTest<TVerifier>
    where TAnalyzer : DiagnosticAnalyzer, new()
    where TVerifier : IVerifier, new();
public readonly struct DiagnosticResult {
    public DiagnosticResult WithSpan(int startLine, int startColumn, int endLine, int endColumn);
    public DiagnosticResult WithArguments(params object[] arguments);
    public DiagnosticResult WithSeverity(DiagnosticSeverity severity);
}
```

## [03]-[IMPLEMENTATION_LAW]

[SCOPE]: this pin ships the ANALYZER harness only — the code-fix and source-generator harness families live in separate unadmitted packages, and `FixedState` is not a member here. Generator verification is Roslyn-direct by design: `CSharpGeneratorDriver` created with `GeneratorDriverOptions(IncrementalGeneratorOutputKind.None, trackIncrementalGeneratorSteps: true)` proves incremental cache reasons (`IncrementalStepRunReason.Cached`/`Unchanged`), and the emitted source asserts structurally through a Roslyn syntax walk.

[REFERENCES]: `ReferenceAssemblies` accumulates `PackageIdentity` rows and restores them through NuGet at `ResolveAsync`, cached in the global-packages folder; the reference-assembly package pins the framework `ref/<tfm>` surface independently of the Roslyn version doing the analysis. `LightupHelpers` reflection tolerates newer Roslyn than the declared floor, which is how the harness rides the estate's Roslyn pins.

[STACKING]:
- `xunit.v3` (`xunit-v3.md`): verifier-neutral composition; specs are plain `[Fact]`/`[Theory]` bodies awaiting `RunAsync(TestContext.Current.CancellationToken)`.
- `Microsoft.CodeAnalysis.CSharp` / `.CSharp.Workspaces`: the direct Roslyn pins supply the compiler and workspace contracts the harness binds.

[LOCAL_ADMISSION]:
- Analyzer rule specs ride the harness with `DefaultVerifier`; a hand-rolled compilation + diagnostic diff re-derives the harness.
- Analyzer test projects skip the workspace analyzer injection (`SkipLocalCSharpAnalyzerReference`), so the analyzer under test is the only analyzer in the run.
