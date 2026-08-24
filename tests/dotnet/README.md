# [DOTNET_TESTING]

Authoring law for every C# spec, kit member, scenario, and benchmark under `tests/dotnet`. Every suite composes the two kits — `Rasm.TestKit` host-free, `Rasm.ScenarioKit` host-aware — and never re-derives a surface they own.

## [01]-[ROUTER]

- [01]-[RULINGS](RULINGS.md): Settled .NET-tree testing decisions — package admissions, oracle discriminants, structure retirements.
- [02]-[API](.api/): Dev-tool API catalogs, one per test-stack package family; kit members and specs transcribe at catalog-verified spellings.
- [03]-[CONTRACTS](../../libs/contracts/README.md): Estate conformance law — C# emits or round-trips vectors by manifest role.
- [04]-[BRIDGE](../../tools/rhino-bridge/README.md): Scenario lifecycle, reference admission, and tolerance law for the live-host rail.

## [02]-[TOPOLOGY]

Classifiers route every project into its lane, and `Directory.Build.props` derives the kit, scenario-kit, benchmark, and test-lane classifiers from project path, `Directory.Build.targets` settles the `IsTestProject` verdict a body-set `AssayTestShell` overturns and seals the classifier vocabulary, and the assay routing closure consumes the shell and host-bound lanes. Each csproj states its classifier and adds only its suite-owned harness packages; the shared test stack never re-wires per project:

| [INDEX] | [CLASSIFIER]           | [MEANING]                                                                                                  |
| :-----: | :--------------------- | :--------------------------------------------------------------------------------------------------------- |
|  [01]   | `IsTestProject`        | unit/property spec; receives the MTP runner, test stack, one `Rasm.TestKit` ref (analyzer suites excepted) |
|  [02]   | `IsTestKitProject`     | the host-free kit itself (`Rasm.TestKit`)                                                                  |
|  [03]   | `IsScenarioKitProject` | the host-aware scenario SDK (`Rasm.ScenarioKit`)                                                           |
|  [04]   | `IsBenchmarkProject`   | the BenchmarkDotNet console session (`Rasm.Benchmarks`)                                                    |
|  [05]   | `AssayTestShell`       | scenario/shell content assay routes out of unit-test runs                                                  |
|  [06]   | `AssayHostBound`       | project binding the live host; never executed as a managed unit suite                                      |

- Per-package suites live in `tests/dotnet/libs/<Package>/` and mirror `libs/dotnet` paths with `<Source>.spec.cs` files.
- `Rasm.AppUi`'s suite is an `AssayTestShell` shell carrying `Avalonia.Headless.XUnit` as its headless UI session runner.
- `tests/dotnet/_architecture` carries the assembly boundary laws beside the infra-primitive suites proving the kits, the gate, and snapshot hygiene.
- Kit capability without a falsification law in `_architecture` stays unproven and gets deleted or proven, never trusted.
- `HostFreeModel.NonVacuous` runs before any boundary rule and every kit fold refuses an empty table — an ArchUnit rule passes vacuously over none.

## [03]-[KIT]

`Rasm.TestKit` is the one host-free law substrate:
- [01]-`Spec.cs`: `Law<T>` rows, the `ForAll`/`Hold`/`Refutes`/`Replay` sampler spine, rail gates, and the matrix/catalog/family fold surface.
- [02]-`Approx.cs`: `Tolerance` regimes, `Metric` rows, `Approx.Equal`, the throwing `Spec.Equal` gates, and the `Spec.Golden` table.
- [03]-`Gens.cs`: Magnitude-stratified scalar, geometry, wire, stamp, and quantity bands with the typed `Fault`/`Exceptional` rail lanes.
- [04]-`Numeric.cs`: `Norm` smart-enum vocabulary and independent numeric/geometry oracles that return values, never assert mid-flight.
- [05]-`Laws.cs`: `[Law(subject, name)]` coverage attribution and the `ScanAssembly`/`Sut`/`AssertCoverage` census gate.
- [06]-`Seams.cs`: `Shape<TValue>` substitution union, `SeamProbe`, `VariantWriter`, `TmpRoot`, `NdjsonOracle`, and the `Timeline` clock.
- [07]-`Manifests.cs`: `ProjectFacts` csproj projection, zero-root workspace walks joining any new csproj to the parity laws, `Corpus` discovery.

`Rasm.ScenarioKit` is the sibling host-aware SDK; `ScenarioContext` is the one evidence channel, and an unbound SDK call fails typed:
- `[RhinoScenario(theme)]` declares a scenario with optional `Requires`/`BudgetMs`.
- `Require`/`Expect` assert facts, `Note` records observations; the `Manifest` writer derives lane admission from the Contract fact-prefix family.
- New Contract manifest lanes need no SDK edit; a foreign manifest role is an input guard.
- `Certify` emits reference evidence over typed or raw `JsonElement` actuals as `{name, actual, tolerance}`; admission stays supervisor-decided.
- `Case` brackets a sub-case, converting a throwing body to typed failure while always landing its status fact.
- `Scratch` refuses path escape, `Stamp` derives run stamps from the stem, and `Artifact` registers captures.
- `DocumentScope` owns lifecycle — `Open`/`Done` convert a faulting host surface to typed failure; `Capture.Snapshot` owns viewport captures.
- Prefix-lane wire keys render off the Contract's `EvidenceRole.FactPrefix`; the SDK-local `FactKey` grammar owns only composite and constant keys.

## [04]-[LAWS]

Every `Law<T>` row is witness-mandatory: registration carries a `RefutingWitness` the property must fail on, and `Spec.Hold` runs `Spec.Refutes` before sampling. Each row closes over its equality policy, so a sloppy `eq` is unregistrable, and a shrunk counterexample pins as a seed-keyed `Spec.Replay` regression.

Row vocabulary is `Law.Of`, `Law.Identity`, `Law.Idempotent`, `Law.Inverse`, `Law.Roundtrip`, `Law.Commutative`, `Law.Associative`, `Law.Distributive`, `Law.Monotone`, and `Law.Permutation`; a new algebraic family is one row constructor beside these, never a parallel assertion helper. Tables of rows hold together:

```csharp conceptual
[Law(typeof(Shape), "algebra")]
public sealed class ShapeSpec {
    [Fact]
    public void Algebra() => Spec.Hold(
        Law.Roundtrip(name: "encode/decode", gen: ShapeGen, forward: Shape.Encode, back: Shape.Decode, witness: Shape.Degenerate),
        Law.Idempotent(name: "normalize", gen: ShapeGen, f: Shape.Normalize, witness: Shape.Degenerate));
}
```

Coverage attribution rides `[Law(typeof(Subject), "name")]` on the spec class or method, optionally narrowed with `Member`; `Laws.AssertCoverage` folds the scanned `LawRecord` manifest against the `SutTarget` public surface, with exemptions derived from production `[CspExempt]`/`[CspScope]` sites — never a parallel exemption catalog.

## [05]-[ORACLES]

Rail, numeric, matrix, and wire proofs ride the kit oracles:
- Rail outcomes prove through the kit's `Spec` gates, failure identity being the closed-family case name, never a carrier flag or message text.
- `Spec.Equal` proves numeric equality under an explicit `Tolerance` and `Metric` row — `SignAmbiguous`, `Periodic`, ulps — and NaN admits nothing.
- Matrix and vector facts prove against `Numeric` oracles under a named `Norm`: the oracle returns a value and the `Spec` gate decides.
- Near-degenerate orientation proves on `Numeric.OrientSign`, the scaled-integer sign no rounding flips; conservation proves on closed-form oracles.
- `Spec.RoundtripBytes` proves wire shape against generated `JsonTypeInfo` and refuses byte-identical twins; `NdjsonOracle` decodes boundary output.
- `Spec.ContentKey` proves one mint over one axis table for stability, representation independence, and separation, refusing a separator-less table.
- `Spec.Causal` holds the HLC advance law, `Spec.Semilattice` folds a verdict over its closed vocabulary, `Spec.FaultBands` names band violations.
- `Spec.DualPath` proves subject against independent reference, and `Spec.Golden` anchors fixed numerics, naming every diverging row in one verdict.
- `Timeline` proves time-dependent behavior: the SUT takes the injected `Clock`, `Advance` is the one motion, and the mark log carries its instants.

## [06]-[GENERATORS]

- Kit magnitude-stratified scalars sample every float hazard each run; a spec-local generator resampling the tame band alone is a coverage illusion.
- Geometry laws draw construction-invariant bands, `Gens.Conditioned` fixing a known condition number so tolerances scale as `κ·base`, never a guess.
- Failure lanes inject the typed `Fault` union so assertions dispatch on case identity, and `Gens.Exceptional` proves the recovery split survives.
- Wire and identity laws draw `Gens.WireString`, `Gens.Payload`, and `Gens.Mutant`; stamp laws draw `Gens.Hlc`, quantity laws the SI-exponent bands.

## [07]-[LANES]

- Manual MTP runs route TRX with `--report-trx --results-directory .artifacts/dotnet/trx/<project>`; assay-run suites route into its artifact scope.
- Mutation rides assay's staged Stryker.NET invocation, and root `stryker-config.json` auto-discovery bounds any bare `dotnet stryker` run.
- `VerifyChecks` walks snapshot hygiene whole-tree under `Explicit`, so default runs skip it and the hygiene lane invokes `-- --explicit only`.

## [08]-[BENCHMARKS]

`Rasm.Benchmarks` is the one measurement session: BenchmarkDotNet rides the `_benchmarks` switcher under `IsBenchmarkProject`, never inside unit runs. Registry and discovery stay parity-locked: `Regression.RegistryParity` fails the `_architecture` suite on a `[Benchmark]` without a `BenchCase` row and on a phantom row without a benchmark. Gated benchmarks are one `BenchCase` registry row each — the exact BDN `FullName`, an absolute budget over a `GateStat` row (`Min`/`Median`/`Mean`), and a `MaxRelIqr` dispersion ceiling; a new gated case is a row, never a parallel harness.

One `gate` verb consumes BDN `*-report-full.json` reports newest-last, verifies the corpus manifest, folds a `Pass`/`TooNoisy`/`Breach` verdict per case, and runs the sustained-regression segmenter across the report series. Breach and noise exit distinctly — `TooNoisy` never folds into pass — and an ungateable case (absent benchmark, missing statistics, dispersion over ceiling) is a visible verdict, never silence; an empty registry still gates visibly through the session receipt.

`BenchCorpus` is the committed corpus manifest: folder `BenchClaim` rosters declare corpus slugs and stay the authority, `BenchCorpus.Declared` transcribes each slug verbatim as the fixture's `CorpusEntry.RelativePath` under `tests/dotnet/_benchmarks/corpus`, and the kit's `Manifests.Corpus` discovery measures `CorpusEntry.Key` from fixture bytes at run. Every fixture is one extensionless file named for its slug, so `.gitattributes` binds pointer custody by the corpus root rather than by extension. `BenchCorpus.Admit` is bijective — a declared slug with no committed fixture and a fixture no roster declares are both typed refusals the gate breaches on — so a corpus-bound claim gates on a real fixture, never on a declaration floating free.

## [09]-[SNAPSHOTS]

Verify owns stable artifact snapshots only — generated source, emitted contracts, durable wire goldens — registered once per assembly through a `[ModuleInitializer]` calling `VerifyDiffPlex.Initialize()`. Snapshot only what an independent producer emits, and treat a `.verified.txt` diff as evidence about the producer, never as a file to re-accept reflexively.

Its hygiene gate pair lives in `_architecture`: `VerifyChecks.Run()` audits orphaned received files, imported snapshot roots, and repository snapshot policy; `DanglingSnapshots.Run()` rejects verified files no executed test tracks. Verify proves every C# binding an admitted manifest entry names, and a golden whose producer and every decoder are C# homes here under the `tests/RULINGS.md` `[04]-[STRUCTURE]` corpus-seat ruling.

[BRANCH_GOLDENS] — wires this tree owns end to end, producer and every decoder inside `libs/dotnet`:
- Clash node-link: `dotnet:Rasm/Spatial/index#SPATIAL_INDEX` emits `NodeLinkProjection`, its `[WIRE]` region freezing the golden a scenario asserts.

## [10]-[DENSITY_AND_BANS]

Shared-setup facts collapse into the kit's row families before a second `[Fact]` exists: `Spec.Hold` law tables, `Spec.Matrix` probe rows, `Spec.Catalog` key-membership folds, and `Spec.Family` value-object batteries make each case a separately killable mutation target where a lone generated sample hides per-case logic. Lines beyond the collapsed rows exist only for a new oracle, boundary, runtime classification, or product-bug guard.

[BANNED_SHAPES]:
- Assertion-free scenarios: a `[RhinoScenario]` recording `Note` alone with no `Require`/`Expect`/`Certify` fact proves nothing a supervisor can fail.
- Kit bypass: a spec-local assertion helper, tolerance constant, or generator that shadows an existing kit owner — extend the owning kit file instead.
