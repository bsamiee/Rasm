# [analyzer-testing] — the Roslyn analyzer harness behind the Csp rule specs

`Microsoft.CodeAnalysis.CSharp.Analyzer.Testing` runs a `DiagnosticAnalyzer` against markup-annotated or explicitly-located sources inside a synthetic solution and verifies the exact diagnostic set. The package is verifier-neutral — `DefaultVerifier` carries no xunit dependency — so the harness composes cleanly with xunit.v3 and threads `TestContext.Current.CancellationToken` through `RunAsync`. Generator specs deliberately bypass it: the estate drives `CSharpGeneratorDriver` directly with incremental-step tracking and snapshots emissions through the Verify lane.

## [01]-[PACKAGE_SURFACE]

- package: `Microsoft.CodeAnalysis.CSharp.Analyzer.Testing` `1.1.4` (carries `Microsoft.CodeAnalysis.Analyzer.Testing` at the exact same version)
- license: `MIT`
- namespace: `Microsoft.CodeAnalysis.CSharp.Testing` (the two C# types), `Microsoft.CodeAnalysis.Testing` (everything shared)
- asset: `lib/net8.0`; Roslyn contract floor rides `Microsoft.CodeAnalysis.CSharp.Workspaces` and lifts to the estate's direct Roslyn pins
- rail: evidence — analyzer diagnostic verification over synthetic solutions with NuGet-resolved reference assemblies

## [02]-[PUBLIC_TYPES]

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

## [03]-[ENTRYPOINTS]

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

```csharp signature
public class CSharpAnalyzerTest<TAnalyzer, TVerifier> : AnalyzerTest<TVerifier>
    where TAnalyzer : DiagnosticAnalyzer, new()
    where TVerifier : IVerifier, new();
public readonly struct DiagnosticResult {
    public DiagnosticResult WithSpan(int startLine, int startColumn, int endLine, int endColumn);
    public DiagnosticResult WithArguments(params object[] arguments);
    public DiagnosticResult WithSeverity(DiagnosticSeverity severity);
}
```

## [04]-[IMPLEMENTATION_LAW]

[SCOPE]: this pin ships the ANALYZER harness only — the code-fix and source-generator harness families live in separate unadmitted packages, and `FixedState` is not a member here. Generator verification is Roslyn-direct by design: `CSharpGeneratorDriver` created with `GeneratorDriverOptions(IncrementalGeneratorOutputKind.None, trackIncrementalGeneratorSteps: true)` proves incremental cache reasons (`IncrementalStepRunReason.Cached`/`Unchanged`), and the emitted source snapshots through the Verify lane.

[REFERENCES]: `ReferenceAssemblies` accumulates `PackageIdentity` rows and restores them through NuGet at `ResolveAsync`, cached in the global-packages folder; the reference-assembly package pins the framework `ref/<tfm>` surface independently of the Roslyn version doing the analysis. `LightupHelpers` reflection tolerates newer Roslyn than the declared floor, which is how the harness rides the estate's Roslyn pins.

[STACKING]:
- `xunit.v3` (`xunit-v3.md`): verifier-neutral composition; specs are plain `[Fact]`/`[Theory]` bodies awaiting `RunAsync(TestContext.Current.CancellationToken)`.
- `Verify` (`verify.md`): the generator lane's emission snapshots; DiffPlex rendering armed per assembly.
- `Microsoft.CodeAnalysis.CSharp` / `.CSharp.Workspaces`: the direct Roslyn pins supply the compiler and workspace contracts the harness binds.

[LOCAL_ADMISSION]:
- Analyzer rule specs ride the harness with `DefaultVerifier`; a hand-rolled compilation + diagnostic diff re-derives the harness.
- The analyzer test project skips the workspace analyzer injection (`SkipLocalCSharpAnalyzerReference`), so the analyzer under test is the only analyzer in the run.

[RAIL_LAW]:
- Package: `Microsoft.CodeAnalysis.CSharp.Analyzer.Testing`
- Owns: analyzer diagnostic verification — sources, expected diagnostics, reference surfaces, markup spans.
- Accept: multi-file `TestState` solutions, editorconfig rows via `AnalyzerConfigFiles`, exact `DiagnosticResult` sets.
- Reject: the code-fix/generator harness siblings while unadmitted, xunit-coupled verifiers, or diagnostic assertions by string-matching compiler output.
