# [CSHARP_TESTING]

Authoring law for every C# spec, kit member, scenario, and benchmark under `tests/csharp`. Every suite composes the two kits — `Rasm.TestKit` host-free, `Rasm.ScenarioKit` host-aware — and never re-derives a surface they own.

## [01]-[ROUTER]

- [01]-[RULINGS](RULINGS.md): Settled C#-tree testing decisions — package admissions, oracle discriminants, structure retirements.
- [02]-[API](.api/): Dev-tool API catalogs, one per test-stack package family; kit members and specs transcribe at catalog-verified spellings.
- [03]-[CONTRACTS](../contracts/README.md): Corpus producer law — C# emits every asset the sibling trees round-trip.
- [04]-[BRIDGE](../../tools/rhino-bridge/README.md): Scenario lifecycle, reference admission, and tolerance law for the live-host rail.

## [02]-[TOPOLOGY]

Classifiers route every project into its lane: `Directory.Build.props` derives the kit, scenario-kit, benchmark, and test classifiers from project path, `Directory.Build.targets` seals the classifier vocabulary, and the assay routing closure consumes the shell and host-bound lanes. A csproj states its classifier and adds only its suite-owned harness packages; the shared test stack never re-wires per project:

| [INDEX] | [CLASSIFIER]           | [MEANING]                                                                                                  |
| :-----: | :--------------------- | :--------------------------------------------------------------------------------------------------------- |
|  [01]   | `IsTestProject`        | unit/property spec; receives the MTP runner, test stack, one `Rasm.TestKit` ref (analyzer suites excepted) |
|  [02]   | `IsTestKitProject`     | the host-free kit itself (`Rasm.TestKit`)                                                                  |
|  [03]   | `IsScenarioKitProject` | the host-aware scenario SDK (`Rasm.ScenarioKit`)                                                           |
|  [04]   | `IsBenchmarkProject`   | the BenchmarkDotNet console session (`Rasm.Benchmarks`)                                                    |
|  [05]   | `AssayTestShell`       | scenario/shell content assay routes out of unit-test runs                                                  |
|  [06]   | `AssayHostBound`       | project binding the live host; never executed as a managed unit suite                                      |

- Per-package suites live in `tests/csharp/libs/<Package>/` and mirror `libs/csharp` paths with `<Source>.spec.cs` files.
- `Rasm.AppUi`'s suite is an `AssayTestShell` shell carrying `Avalonia.Headless.XUnit` as its headless UI session runner.
- `tests/csharp/_architecture` carries the assembly boundary laws (`AssemblyBoundaries.spec.cs`, `CatalogueBoundaries.spec.cs`) and the infra-primitive falsification suites proving the kits, the benchmark gate, and the snapshot-hygiene rail (`TestInfrastructurePrimitives.spec.cs`, `ScenarioKitPrimitives.spec.cs`, `BenchmarkGatePrimitives.spec.cs`, `SnapshotHygiene.spec.cs`).
- A kit capability without a falsification law in `_architecture` is unproven and gets deleted or proven, never trusted.
- Every ArchUnit rule is vacuously true over an empty type set: `HostFreeModel.NonVacuous` runs before any boundary rule, and every kit fold refuses an empty table as proving nothing.

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
- A new Contract manifest lane needs no SDK edit; a foreign manifest role is an input guard.
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
- Rail outcomes prove through the kit gates: `Spec.Succ`, `Spec.Fail`, `Spec.FailCategory`, `Spec.Valid`, `Spec.Invalid`, `Spec.Some`, `Spec.None`, `Spec.AllErrors`, `Spec.FailMany` — carrier-flag inspection (`.IsSucc` and kin) is never primary proof, and failure identity is the closed-family case name, never message text.
- Numeric equality proves through `Spec.Equal` under an explicit `Tolerance` regime and `Metric` row; sign ambiguity is the `SignAmbiguous` row, angular wrap is a `Periodic(period)` row, and bit-adjacency rides the `Tolerance` ulps budget — never a hand-rolled either-or assertion. NaN admits nothing.
- Matrix and vector facts prove against `Numeric` oracles under a named `Norm`; the oracle returns a value and the `Spec` gate decides — no oracle asserts mid-flight.
- Orientation facts on near-degenerate configurations prove against `Numeric.OrientSign`, the exact scaled-integer sign no rounding can flip; area and volume conservation prove against `ShoelaceArea`/`SignedVolume` closed forms.
- Wire shapes prove through `Spec.RoundtripBytes` against the generated `JsonTypeInfo` contract — the corpus arm folds every `Manifests.Corpus` fixture through the same byte-identity law and refuses byte-identical twins; process-boundary output decodes through `NdjsonOracle` (`One` first-row, `All` every gated row), never string-contains scraping.
- Content-keyed identity proves through `Spec.ContentKey`: one mint over one axis table carries stability, representation independence, and separation, and the gauge refuses a table without a separating axis — only a separating row refutes a constant mint.
- Receipt obligations prove through the receipt-algebra gates: `Spec.Causal` holds the HLC advance law against a refuting witness pair, `Spec.Semilattice` proves a verdict fold exhaustively over its closed vocabulary, and `Spec.FaultBands` folds band containment, code uniqueness, and band disjointness into one verdict naming every violation.
- Differential facts prove through the pure `Spec.DualPath` arms — subject against independent reference under generic equality or a `Tolerance`/`Metric` regime — and fixed numeric anchors through the `Spec.Golden` table, which names every diverging row in one verdict.
- Time-dependent behavior proves against `Timeline`: the SUT takes the injected `Clock`, `Advance` is the only motion, and the typed mark log carries schedule-derived instants — a spec that sleeps or reads the wall clock is the named defect.

## [06]-[GENERATORS]

- Start from the kit's magnitude-stratified scalars (`Gens.Finite`, `Gens.AnyDouble`, `Gens.Cancellation`, `Gens.Angle`, `Gens.IntEdges`) so every float hazard samples every run instead of arriving as a rare accident; a spec-local scalar generator that resamples only the tame band is a coverage illusion.
- Geometry laws draw from the construction-invariant bands — `Gens.Direction` unit vectors, `Gens.NearCollinear` ulp-perturbed degeneracy, `Gens.Orthogonal` Householder products covering both `O(n)` components, `Gens.Conditioned` matrices whose condition number is known by construction so tolerances scale as `κ·base`, never a guessed constant.
- Failure lanes inject the typed `Fault` union so assertions dispatch on case identity, never on message substrings; recovery rails sample `Gens.Exceptional` to prove the `Expected`/`Exceptional` split survives.
- Wire and identity laws draw from `Gens.WireString` (UTF-8-total hazard strings), `Gens.Payload` (magnitude-stratified byte blocks), and `Gens.Mutant` (one-byte-flip separation pairs); stamp laws draw `Gens.Hlc`, and quantity laws draw the seven-exponent `Gens.SiExponents`/`Gens.Measure` bands.

## [07]-[LANES]

- Manual MTP runs route TRX with `--report-trx --results-directory .artifacts/csharp/trx/<project>`; assay-run suites route results into the assay artifact scope automatically.
- Mutation rides assay's staged Stryker.NET invocation; the root `stryker-config.json` auto-discovery bounds any bare `dotnet stryker` run with the concurrency cap and `.artifacts/` output routing.
- `VerifyChecks`, the snapshot-hygiene walk, is a whole-tree audit marked `Explicit`: default runs skip it, and the hygiene lane invokes it with `dotnet test tests/csharp/_architecture/Rasm.Architecture.Tests.csproj -- --explicit only`.

## [08]-[BENCHMARKS]

`Rasm.Benchmarks` is the one measurement session: BenchmarkDotNet rides the `_benchmarks` switcher under `IsBenchmarkProject`, never inside unit runs. Registry and discovery are parity-locked: `Regression.RegistryParity` fails the `_architecture` suite on a `[Benchmark]` without a `BenchCase` row and on a phantom row without a benchmark. A gated benchmark is one `BenchCase` registry row — the exact BDN `FullName`, an absolute budget over a `GateStat` row (`Min`/`Median`/`Mean`), and a `MaxRelIqr` dispersion ceiling; a new gated case is a row, never a parallel harness.

One `gate` verb consumes BDN `*-report-full.json` reports newest-last, folds a `Pass`/`TooNoisy`/`Breach` verdict per case, and runs the sustained-regression segmenter across the report series. Breach and noise exit distinctly — `TooNoisy` never folds into pass — and an ungateable case (absent benchmark, missing statistics, dispersion over ceiling) is a visible verdict, never silence; an empty registry still gates visibly through the session receipt.

## [09]-[SNAPSHOTS]

Verify owns stable artifact snapshots only — generated source, emitted contracts, durable wire goldens — registered once per assembly through a `[ModuleInitializer]` calling `VerifyDiffPlex.Initialize()`. Snapshot only what an independent producer emits, and treat a `.verified.txt` diff as evidence about the producer, never as a file to re-accept reflexively.

Its hygiene gate pair lives in `_architecture`: `VerifyChecks.Run()` audits solution-wide snapshot conventions — orphaned `*.received.*` litter, csproj-imported snapshot nestings, the `.gitignore`/`.gitattributes`/`.editorconfig` rows for every verified extension — and `DanglingSnapshots.Run()` fails a build-server run on verified files no executed test tracked. C# is the sole producer for `tests/contracts/`: corpus assets are emitted by the owning wire surface and round-trip-proven through Verify under the corpus law.

## [10]-[DENSITY_AND_BANS]

Shared-setup facts collapse into the kit's row families before a second `[Fact]` exists: `Spec.Hold` law tables, `Spec.Matrix` probe rows, `Spec.Catalog` key-membership folds, and `Spec.Family` value-object batteries make each case a separately killable mutation target where a lone generated sample hides per-case logic. Lines beyond the collapsed rows exist only for a new oracle, boundary, runtime classification, or product-bug guard.

[BANNED_SHAPES]:
- Assertion-free scenarios: a `[RhinoScenario]` that records only `Note` observations with no asserted `Require`/`Expect`/`Certify` fact proves nothing the supervisor can fail.
- Kit bypass: a spec-local assertion helper, tolerance constant, or generator that shadows an existing kit owner — extend the owning kit file instead.
