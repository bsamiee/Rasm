# [CSHARP_TESTING]

Authoring law for every C# spec, kit member, scenario, and benchmark under `tests/csharp`. The cross-language proof law — oracle grades, the witness mandate, the banned-shape core — is [tests/README.md](../README.md). Anything in this tree — kit files, suite shells, scenario SDK surfaces, tooling configuration — is rebuilt ground-up the moment a denser shape exists; no compatibility wrappers, no aliased old surfaces, no band-aids, and breaking existing specs is never a reason to preserve chaff. Verified API catalogs for the test-stack packages live in [.api/](.api/) — one catalog per package family, decompile-verified members only.

## [01]-[PROJECTS]

Classifiers route every project into its lane: `Directory.Build.props` derives the kit, scenario-kit, benchmark, and test classifiers from project path, `Directory.Build.targets` seals the classifier vocabulary, and the assay routing closure consumes the shell and host-bound lanes. A csproj states its classifier and adds only its suite-owned harness packages; the shared test stack never re-wires per project:

| [INDEX] | [CLASSIFIER]           | [MEANING]                                                                                                                    |
| :-----: | :--------------------- | :--------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `IsTestProject`        | unit/property spec project; receives the MTP runner, test stack, and one `Rasm.TestKit` reference (analyzer suites excepted) |
|  [02]   | `IsTestKitProject`     | the host-free kit itself (`Rasm.TestKit`)                                                                                    |
|  [03]   | `IsScenarioKitProject` | the host-aware scenario SDK (`Rasm.ScenarioKit`)                                                                             |
|  [04]   | `IsBenchmarkProject`   | the BenchmarkDotNet console session (`Rasm.Benchmarks`)                                                                      |
|  [05]   | `AssayTestShell`       | scenario/shell content assay routes out of unit-test runs                                                                    |
|  [06]   | `AssayHostBound`       | project binding the live host; never executed as a managed unit suite                                                        |

Per-package suites live in `tests/csharp/libs/<Package>/` and mirror `libs/csharp` paths with `<Source>.spec.cs` files. The `Rasm.AppUi` suite is an `AssayTestShell` shell carrying `Avalonia.Headless.XUnit` as its headless UI session runner. `tests/csharp/_architecture` carries the assembly boundary laws (`AssemblyBoundaries.spec.cs`, `CatalogueBoundaries.spec.cs`) and the infra-primitive falsification suites that prove the kits, the benchmark gate, and the snapshot-hygiene rail (`TestInfrastructurePrimitives.spec.cs`, `ScenarioKitPrimitives.spec.cs`, `BenchmarkGatePrimitives.spec.cs`, `SnapshotHygiene.spec.cs`) — a kit capability without a falsification law in `_architecture` is unproven and gets deleted or proven, never trusted.

Manual MTP runs route TRX with `--report-trx --results-directory .artifacts/csharp/trx/<project>`; assay-run suites route results into the assay artifact scope automatically. Mutation rides assay's staged Stryker.NET invocation; the root `stryker-config.json` auto-discovery bounds any bare `dotnet stryker` run with the concurrency cap and `.artifacts/` output routing. The snapshot-hygiene walk (`VerifyChecks`) is a whole-tree audit marked `Explicit`: default runs skip it, and the hygiene lane invokes it with `dotnet test tests/csharp/_architecture/Rasm.Architecture.Tests.csproj -- --explicit only`.

## [02]-[KIT_SURFACE]

`Rasm.TestKit` is the one host-free law substrate; extend the owning file, never add a helper file:

| [INDEX] | [FILE]         | [OWNS]                                                                                                                                                                                                                                     |
| :-----: | :------------- | :----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `Spec.cs`      | `Law<T>` rows, `Spec.ForAll`/`Hold`/`Refutes`/`Replay`, rail gates, `Metamorphic`, `ModelBased`, `DualPath`, `Parallel`, `Classified`, `Distributed`, `RoundtripBytes`, `Catalog`, `Matrix`, `CountsConserve`, `Family`, `ContentKey`, `Causal`, `Semilattice`, `FaultBands`                    |
|  [02]   | `Approx.cs`    | `Tolerance` (absolute/relative/hybrid/ulps regimes), `Metric` rows (`Absolute`, `SignAmbiguous`, `Periodic(period)`), `Approx.Equal`, the throwing `Spec.Equal` gates, the `Spec.Golden` golden-value table                                                                      |
|  [03]   | `Gens.cs`      | magnitude-stratified scalars (`Finite`, `AnyDouble`, `Cancellation`, `Angle`, `IntEdges`, …), geometry bands (`Direction`, `Ring`, `NearCollinear`, `Orthogonal`, `Conditioned`), the wire/stamp/quantity bands, and the typed `Fault`/`Exceptional` rail lanes           |
|  [04]   | `Numeric.cs`   | the `Norm` smart-enum vocabulary (`MaxAbs`, `L1`, `LInf`, `Frobenius`) and independent numeric/geometry oracles (`OrientSign`, `ShoelaceArea`, `SignedVolume`, residuals, spectral references) that return values, never assert mid-flight |
|  [05]   | `Laws.cs`      | `[Law(subject, name)]` coverage attribution, `Laws.ScanAssembly`/`Laws.Sut`/`Laws.AssertCoverage` — the law-coverage census gate                                                                                                           |
|  [06]   | `Seams.cs`     | the `Shape<TValue>` substitution union (`Canned`/`FanOut`/`Factory`), `SeamProbe`, `VariantWriter`, `TmpRoot`, `NdjsonOracle`, and the `Timeline` deterministic clock over `FakeTimeProvider`                                                                                                              |
|  [07]   | `Manifests.cs` | `ProjectFacts` csproj projection and workspace-rooted path resolution — manifest facts only, never package rosters; `DiskProjects()`/`VersionedPackageRows()` with zero roots walk the whole workspace (dot-dirs, `node_modules`, `bin`, `obj` pruned), so a csproj at a new root joins the parity laws automatically; `Corpus` golden-fixture discovery keyed on the seed-zero XxHash128 spine                                                                                                                         |

`Rasm.ScenarioKit` is the sibling host-aware SDK: `[RhinoScenario(theme)]` with optional `Requires`/`BudgetMs`, `ScenarioContext` as the one evidence channel (`Require`/`Expect` for asserted facts, `Note` plus the role-dispatched `Manifest` writer — one verb whose lane admission derives from the Contract's manifest fact-prefix family, so a new Contract manifest lane needs no SDK edit and a foreign role is an input guard — for observations, `Certify` for reference evidence — one generic verb over typed values and raw `JsonElement` actuals, emitting the `{name, actual, tolerance}` payload the supervisor folds while admission stays supervisor-decided by evidence mode — `Case` as the sub-case bracket that converts a throwing body to typed failure and always lands its status fact, `Scratch` for scratch-rooted paths that refuse escape, `Stamp` for stem-derived run stamps, `Artifact` for capture registration), `DocumentScope` for document lifecycle (`Open`/`Done` convert a faulting host surface to typed failure), and `Capture.Snapshot` for viewport captures. Prefix-lane wire keys (`reference.`, `manifest.*`, `artifact.`) render off the Contract's `EvidenceRole.FactPrefix`; the SDK-local `FactKey` grammar owns only composite and constant keys. Unbound SDK calls fail typed — a scenario surface never throws or writes stray files outside a bridge run.

## [03]-[LAW_ROWS]

Every `Law<T>` row is witness-mandatory: registration carries a `RefutingWitness` the property must fail on, and `Spec.Hold` runs `Spec.Refutes` before sampling. Each row closes over its equality policy, so a sloppy `eq` is unregistrable.

The row vocabulary is `Law.Of`, `Law.Identity`, `Law.Idempotent`, `Law.Inverse`, `Law.Roundtrip`, `Law.Commutative`, `Law.Associative`, `Law.Distributive`, `Law.Monotone`, and `Law.Permutation`; a new algebraic family is one row constructor beside these, never a parallel assertion helper. Tables of rows hold together:

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

## [04]-[ORACLES]

Rail, numeric, matrix, and wire proofs ride the kit oracles:
- Rail outcomes prove through the kit gates: `Spec.Succ`, `Spec.Fail`, `Spec.FailCategory`, `Spec.Valid`, `Spec.Invalid`, `Spec.Some`, `Spec.None`, `Spec.AllErrors`, `Spec.FailMany` — carrier-flag inspection (`.IsSucc` and kin) is never primary proof.
- Numeric equality proves through `Spec.Equal` under an explicit `Tolerance` regime and `Metric` row; sign ambiguity is the `SignAmbiguous` row, angular wrap is a `Periodic(period)` row, and bit-adjacency rides the `Tolerance` ulps budget — never a hand-rolled either-or assertion. NaN admits nothing.
- Matrix and vector facts prove against `Numeric` oracles under a named `Norm`; the oracle returns a value and the `Spec` gate decides — no oracle asserts mid-flight.
- Orientation facts on near-degenerate configurations prove against `Numeric.OrientSign`, the exact scaled-integer sign no rounding can flip; area and volume conservation prove against `ShoelaceArea`/`SignedVolume` closed forms.
- Wire shapes prove through `Spec.RoundtripBytes` against the generated `JsonTypeInfo` contract — the corpus arm folds every `Manifests.Corpus` fixture through the same byte-identity law and refuses byte-identical twins; process-boundary output decodes through `NdjsonOracle` (`One` first-row, `All` every gated row), never string-contains scraping.
- Content-keyed identity proves through `Spec.ContentKey`: one mint over one axis table carries stability, representation independence, and separation, and the gauge refuses a table without a separating axis — only a separating row refutes a constant mint.
- Receipt obligations prove through the receipt-algebra gates: `Spec.Causal` holds the HLC advance law against a refuting witness pair, `Spec.Semilattice` proves a verdict fold exhaustively over its closed vocabulary, and `Spec.FaultBands` folds band containment, code uniqueness, and band disjointness into one verdict naming every violation.
- Differential facts prove through the pure `Spec.DualPath` arms — subject against independent reference under generic equality or a `Tolerance`/`Metric` regime — and fixed numeric anchors through the `Spec.Golden` table, which names every diverging row in one verdict.
- Time-dependent behavior proves against `Timeline`: the SUT takes the injected `Clock`, `Advance` is the only motion, and the typed mark log carries schedule-derived instants — a spec that sleeps or reads the wall clock is the named defect.

## [05]-[GENERATORS]

Start from the kit's magnitude-stratified scalars (`Gens.Finite`, `Gens.AnyDouble`, `Gens.Cancellation`, `Gens.Angle`, `Gens.IntEdges`) so every float hazard samples every run instead of arriving as a rare accident; a spec-local scalar generator that resamples only the tame band is a coverage illusion. Geometry laws draw from the construction-invariant bands — `Gens.Direction` unit vectors, `Gens.NearCollinear` ulp-perturbed degeneracy, `Gens.Orthogonal` Householder products covering both `O(n)` components, `Gens.Conditioned` matrices whose condition number is known by construction so tolerances scale as `κ·base`, never a guessed constant. Failure lanes inject the typed `Fault` union so assertions dispatch on case identity, never on message substrings; recovery rails additionally sample `Gens.Exceptional` to prove the `Expected`/`Exceptional` split survives. Wire and identity laws draw from `Gens.WireString` (UTF-8-total hazard strings), `Gens.Payload` (magnitude-stratified byte blocks), and `Gens.Mutant` (one-byte-flip separation pairs); stamp laws draw `Gens.Hlc`, and quantity laws draw the seven-exponent `Gens.SiExponents`/`Gens.Measure` bands.

## [06]-[SNAPSHOTS]

Verify owns stable artifact snapshots only — generated source, emitted contracts, durable wire goldens — registered once per assembly through a `[ModuleInitializer]` calling `VerifyDiffPlex.Initialize()`. Snapshot only what an independent producer emits, and treat a `.verified.txt` diff as evidence about the producer, never as a file to re-accept reflexively. The hygiene gate pair lives in `_architecture`: `VerifyChecks.Run()` audits solution-wide snapshot conventions — orphaned `*.received.*` litter, csproj-imported snapshot nestings, the `.gitignore`/`.gitattributes`/`.editorconfig` rows for every verified extension — and `DanglingSnapshots.Run()` fails a build-server run on verified files no executed test tracked. C# is the sole producer for `tests/contracts/`: corpus assets are emitted by the owning wire surface and round-trip-proven through Verify under the corpus law in [tests/contracts/README.md](../contracts/README.md).

## [07]-[BENCHMARKS]

`Rasm.Benchmarks` is the one measurement session: BenchmarkDotNet rides the `_benchmarks` switcher under `IsBenchmarkProject`, never inside unit runs. Registry and discovery are parity-locked: `Regression.RegistryParity` fails the `_architecture` suite on a `[Benchmark]` without a `BenchCase` row and on a phantom row without a benchmark. A gated benchmark is one `BenchCase` registry row — the exact BDN `FullName`, an absolute budget over a `GateStat` row (`Min`/`Median`/`Mean`), and a `MaxRelIqr` dispersion ceiling; a new gated case is a row, never a parallel harness. The `gate` verb consumes BDN `*-report-full.json` reports newest-last, folds a `Pass`/`TooNoisy`/`Breach` verdict per case, and runs the sustained-regression segmenter across the report series. Breach and noise exit distinctly — `TooNoisy` never folds into pass — and an ungateable case (absent benchmark, missing statistics, dispersion over ceiling) is a visible verdict, never silence; an empty registry still gates visibly through the session receipt.

## [08]-[DENSITY_AND_BANS]

Shared-setup facts collapse into the kit's row families before a second `[Fact]` exists: `Spec.Hold` law tables, `Spec.Matrix` probe rows, `Spec.Catalog` key-membership folds, and `Spec.Family` value-object batteries make each case a separately killable mutation target where a lone generated sample hides per-case logic. Lines beyond the collapsed rows exist only for a new oracle, boundary, runtime classification, or product-bug guard.

[BANNED_SHAPES]:
- Assertion-free scenarios: a `[RhinoScenario]` that records only `Note` observations with no asserted `Require`/`Expect`/`Certify` fact proves nothing the supervisor can fail.
- Kit bypass: a spec-local assertion helper, tolerance constant, or generator that shadows an existing kit owner — extend the owning kit file instead.
