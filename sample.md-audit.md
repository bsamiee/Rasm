# 1. Use the admitted sample kind as the algorithm identity

## From

`libs/dotnet/Rasm/.planning/Processing/sample.md:86`

```csharp
[SmartEnum<int>]
public sealed partial class SampleAlgorithmKind {
    public static readonly SampleAlgorithmKind Explicit = new(key: 0, candidateScale: 0.0, densityDriven: false);
    public static readonly SampleAlgorithmKind BridsonActiveListPoisson = new(key: 1, candidateScale: 0.0, densityDriven: false);
    public static readonly SampleAlgorithmKind FarthestCandidate = new(key: 2, candidateScale: 8.0, densityDriven: false);
    public static readonly SampleAlgorithmKind FarthestOptimize = new(key: 3, candidateScale: 8.0, densityDriven: false);
    public static readonly SampleAlgorithmKind LloydCandidateRelaxation = new(key: 4, candidateScale: 8.0, densityDriven: false);
    public static readonly SampleAlgorithmKind CapacityLimitedLloydCandidate = new(key: 5, candidateScale: 1.0, densityDriven: false);
    public static readonly SampleAlgorithmKind WeightedMassPropagation = new(key: 6, candidateScale: 0.0, densityDriven: false);
    public static readonly SampleAlgorithmKind VariableDensityPoisson = new(key: 7, candidateScale: 8.0, densityDriven: true);
    public static readonly SampleAlgorithmKind YukselWeightedSampleElimination = new(key: 8, candidateScale: 1.0, densityDriven: false);
    public static readonly SampleAlgorithmKind DworkVariableDensity = new(key: 9, candidateScale: 12.0, densityDriven: true);
    public static readonly SampleAlgorithmKind ContinuousPowerCcvt = new(key: 10, candidateScale: 8.0, densityDriven: false);
    public static readonly SampleAlgorithmKind AdaptiveVariableDensityPoisson = new(key: 11, candidateScale: 12.0, densityDriven: true);

    public double CandidateScale { get; }
    public bool DensityDriven { get; }
}
```

`libs/dotnet/Rasm/.planning/Processing/sample.md:251`

```csharp
internal (Option<int> Count, Option<int> Iterations, double CandidateScale, SampleAlgorithmKind Algorithm) Facts => Switch(
```

`libs/dotnet/Rasm/.planning/Processing/sample.md:355`

```csharp
public readonly record struct SampleAlgorithm(
    SampleAlgorithmKind Kind, CapabilitySet<SampleAssurance> Assurances,
    Option<int> Seed = default, Option<int> TargetCount = default, Option<int> OversampleCount = default, Option<int> OversampleFactor = default,
    Option<double> Alpha = default, Option<double> Beta = default, Option<double> Gamma = default, Option<double> Radius = default, Option<double> WeightLimitRadius = default,
    Option<int> Eliminated = default, Option<int> NeighborUpdates = default,
    Option<int> Attempts = default, Option<int> ActivePops = default, Option<int> RejectedTooClose = default, Option<int> RejectedDomain = default,
```

## To

```csharp
// SampleAlgorithmKind DELETED
```

```csharp
internal (Option<int> Count, Option<int> Iterations, double CandidateScale) Facts => Switch(
    explicitCase: static _ => (Option<int>.None, Option<int>.None, 0.0),
    poissonDiskCase: static _ => (Option<int>.None, Option<int>.None, 0.0),
    farthestCase: static c => (Some(c.Count.Value), Option<int>.None, 8.0),
    optimizeCase: static c => (Some(c.Count.Value), Some(c.Iterations.Value), 8.0),
    lloydCase: static c => (Some(c.Count.Value), Some(c.Iterations.Value), 8.0),
    capacityCase: static c => (Some(c.Count.Value), Some(c.Iterations.Value), c.Limit.Value),
    weightedCase: static _ => (Option<int>.None, Option<int>.None, 0.0),
    scalarDensityCase: static c => (Some(c.Count.Value), Option<int>.None, 8.0),
    adaptiveCase: static c => (Some(c.Count.Value), Option<int>.None, 12.0),
    sampleEliminationCase: static c => (Some(c.Count.Value), Option<int>.None, c.OversampleFactor.Value),
    dworkVariableDensityCase: static c => (Some(c.Count.Value), Option<int>.None, 12.0),
    powerCcvtCase: static c => (Some(c.Count.Value), Some(c.Policy.Iterations.Value), 8.0));
```

```csharp
public readonly record struct SampleAlgorithm(
    SampleKind Kind, CapabilitySet<SampleAssurance> Assurances,
    Option<int> OversampleCount = default,
    Option<double> EliminationRadius = default, Option<double> WeightLimitRadius = default,
    Option<int> Eliminated = default, Option<int> NeighborUpdates = default,
    Option<int> ActivePops = default, Option<int> RejectedTooClose = default, Option<int> RejectedDomain = default,
```

## Why

`SampleAlgorithmKind` is a one-for-one shadow of `SampleKind`, violating the generated-owner rule against a roster that mirrors a union. The result then repeats request-owned seed, target, oversample-factor, tuning, attempt, and Poisson-radius values even though the admitted `SampleKind` already carries them. The same `Radius` column also means the derived Yuksel elimination radius, giving one field two semantics.

## Change

Store the admitted `SampleKind` on `SampleAlgorithm`, return only count, iteration, and candidate-scale data from `Facts`, and pass the original case into every selection/run. Delete the seven result members `Seed`, `TargetCount`, `OversampleFactor`, `Alpha`, `Beta`, `Gamma`, and `Attempts`; stop copying the Poisson request radius and rename the surviving Yuksel output to `EliminationRadius`. Remove the unused `radius` parameter from the generic `SelectionOf` path. Replace the capacity test with `algorithm.Kind is SampleKind.CapacityCase` and every algorithm-row construction with its originating union case.

## Delta

Code-fence LOC: `-33`. Module-level types: `-1`. Members: `-21` (`12` roster fields, `2` roster columns, and `7` duplicated result columns). Helper parameters: `-1`.

## Ripples

- `libs/dotnet/Rasm/.planning/Processing/sample.md:14-23`: remove the mirrored algorithm-roster claims and state that `SampleAlgorithm.Kind` carries the admitted `SampleKind`.

# 2. Remove result axes derivable from the execution facts

## From

`libs/dotnet/Rasm/.planning/Processing/sample.md:119`

```csharp
[SmartEnum<int>]
public sealed partial class SampleDomainStatus {
    public static readonly SampleDomainStatus Projected = new(key: 0);
    public static readonly SampleDomainStatus CandidateAccepted = new(key: 1);
    public static readonly SampleDomainStatus CandidateRejected = new(key: 2);
}

[SmartEnum<int>]
public sealed partial class SampleStopKind {
    public static readonly SampleStopKind Completed = new(key: 0);
    public static readonly SampleStopKind CapacityLimited = new(key: 1);
    public static readonly SampleStopKind AllRejected = new(key: 2);
    public static readonly SampleStopKind CandidateExhausted = new(key: 3);
}
```

`libs/dotnet/Rasm/.planning/Processing/sample.md:264`

```csharp
internal Option<double> DensityError(int emitted) =>
    Facts is { Algorithm.DensityDriven: true, Count: Option<int> count }
        ? count.Map(value => Math.Abs(value: emitted - value) / Math.Max(1.0, value))
        : Option<double>.None;
```

`libs/dotnet/Rasm/.planning/Processing/sample.md:385`

```csharp
public readonly record struct SampleTally(
    int Attempted, int Emitted, int Rejected, Option<int> CandidateCount, Option<Distribution<Scalar>> Spacing,
    Option<double> DensityError, Option<int> DensityAccepted, Option<int> DensityRejected, Option<int> Iterations,
    SampleStopKind Stop, SampleDomainStatus DomainStatus, Option<SampleAlgorithm> Algorithm) : IValidityEvidence {
```

## To

```csharp
// SampleDomainStatus DELETED
// SampleStopKind DELETED
```

```csharp
// SampleKind.DensityError DELETED
```

```csharp
public readonly record struct SampleTally(
    int Attempted, int Emitted, int Rejected, Option<int> CandidateCount, Option<Distribution<Scalar>> Spacing,
    Option<int> DensityAccepted, Option<int> DensityRejected, Option<SampleAlgorithm> Algorithm) : IValidityEvidence {
```

## Why

The two smart enums materialize facts already fixed by the result: projection versus selection is carried by `Algorithm.Kind`, rejection by `Rejected`, all-rejected by `Emitted == 0`, target exhaustion by the count on `Algorithm.Kind`, and capacity refusal by the capacity case without `CapacityResidual` assurance. `DensityError` is arithmetic over that same target and `Emitted`. `Iterations` copies request budgets for most cases while CCVT already publishes actual iteration counts in `PowerCcvtSolution`.

## Change

Delete both rosters, `DensityError`, and the four derived `SampleTally` columns. Remove the corresponding `TallyOf` parameters and all call-site branches; retain attempted, emitted, rejected, candidate-count, density census, spacing, and the algorithm execution fact as the non-derivable output.

## Delta

Code-fence LOC: `-24`. Module-level types: `-2`. Members: `-12` (`7` roster fields, `4` tally columns, and `DensityError`).

## Ripples

- `libs/dotnet/Rasm/.planning/Processing/sample.md:14-23`: remove `SampleStopKind` and `SampleDomainStatus` vocabulary claims; describe terminal classification as a projection of the one tally.

# 3. Make algorithm evidence mandatory and keep spectrum at one level

## From

`libs/dotnet/Rasm/.planning/Processing/sample.md:333`

```csharp
PowerCellFragmentFacts Fragments, Option<LinearSolution> DualSolve = default, Option<SamplingSpectrum> Spectrum = default) : IValidityEvidence {
```

`libs/dotnet/Rasm/.planning/Processing/sample.md:385`

```csharp
public readonly record struct SampleTally(
    int Attempted, int Emitted, int Rejected, Option<int> CandidateCount, Option<Distribution<Scalar>> Spacing,
    Option<double> DensityError, Option<int> DensityAccepted, Option<int> DensityRejected, Option<int> Iterations,
    SampleStopKind Stop, SampleDomainStatus DomainStatus, Option<SampleAlgorithm> Algorithm) : IValidityEvidence {
```

`libs/dotnet/Rasm/.planning/Processing/sample.md:436`

```csharp
internal readonly record struct SampleSelection(Point3d[] Points, Option<Arr<double>> Mass, Option<int> DensityAccepted, Option<int> DensityRejected, Option<SampleAlgorithm> Algorithm);
```

`libs/dotnet/Rasm/.planning/Processing/sample.md:630`

```csharp
private static SampleResult SpectrumOntoCcvt(SampleResult result) =>
    result.Tally.Algorithm.Bind(static algorithm => algorithm.PowerCcvt.Map(ccvt => (Algorithm: algorithm, Ccvt: ccvt))).Match(
        Some: pair => result with { Tally = result.Tally with { Algorithm = Some(pair.Algorithm with {
            Spectrum = Option<SamplingSpectrum>.None,
            PowerCcvt = Some(pair.Ccvt with { Spectrum = pair.Algorithm.Spectrum }) }) } },
        None: () => result);
```

## To

```csharp
PowerCellFragmentFacts Fragments, Option<LinearSolution> DualSolve = default) : IValidityEvidence {
```

```csharp
public readonly record struct SampleTally(
    int Attempted, int Emitted, int Rejected, Option<int> CandidateCount, Option<Distribution<Scalar>> Spacing,
    Option<double> DensityError, Option<int> DensityAccepted, Option<int> DensityRejected, Option<int> Iterations,
    SampleStopKind Stop, SampleDomainStatus DomainStatus, SampleAlgorithm Algorithm) : IValidityEvidence {
```

```csharp
internal readonly record struct SampleSelection(Point3d[] Points, Option<Arr<double>> Mass, Option<int> DensityAccepted, Option<int> DensityRejected, SampleAlgorithm Algorithm);
```

```csharp
// SpectrumOntoCcvt DELETED
```

## Why

Every successful selection constructs an algorithm fact, so `Option<SampleAlgorithm>` encodes an impossible success state and forces defensive folds in the kernel and `segment.md`. Sampling spectrum belongs on the common `SampleAlgorithm`; moving it into the CCVT child creates two authorities and a post-processing transfer solely to clear one copy.

## Change

Make `SampleSelection.Algorithm`, `SampleTally.Algorithm`, and the corresponding helper parameters bare `SampleAlgorithm`. Remove `Some`, `Bind`, `Map`, and `IsNone` ceremony at every producer and consumer. Delete `PowerCcvtSolution.Spectrum` and `SpectrumOntoCcvt`; `SegmentKernel.ValidateSamplingSpectrum` updates the common algorithm record directly for every sampling case.

## Delta

Code-fence LOC: `-18`. Members: `-2` (`PowerCcvtSolution.Spectrum` and `SpectrumOntoCcvt`). Optional carriers: `-2`. Types: unchanged.

## Ripples

- `libs/dotnet/Rasm/.planning/Processing/segment.md:137`: remove `result.Tally.Algorithm.IsNone` and replace the optional `Map` update with `Algorithm = result.Tally.Algorithm with { Assurances = ..., Spectrum = Some(spectrum) }`.
- `libs/dotnet/Rasm/.planning/Processing/sample.md:20-31`: state that sampling spectrum has one seat on `SampleAlgorithm`; remove the CCVT child-spectrum transfer claim.

# 4. Collapse the Dwork domain flag into the census

## From

`libs/dotnet/Rasm/.planning/Processing/sample.md:80`

```csharp
[SmartEnum<int>]
public sealed partial class DworkSamplingDomain {
    public static readonly DworkSamplingDomain ContinuousMesh = new(key: 0);
    public static readonly DworkSamplingDomain CandidateSet = new(key: 1);
}
```

`libs/dotnet/Rasm/.planning/Processing/sample.md:299`

```csharp
public readonly record struct DworkCensus(
    DworkSamplingDomain Domain, double RMin, Option<double> BackgroundCellSize, Option<int> BackgroundGridCells,
    int AttemptsPerActive, int GeneratedCandidates, int ActivePops, int RejectedTooClose, int RejectedDomain,
    double LocalRadiusMin, double LocalRadiusMax) : IValidityEvidence {
    public bool CandidateOnly => Domain.Equals(DworkSamplingDomain.CandidateSet);
```

## To

```csharp
// DworkSamplingDomain DELETED
```

```csharp
public readonly record struct DworkCensus(
    bool CandidateOnly, double RMin, double BackgroundCellSize, int BackgroundGridCells,
    int AttemptsPerActive, int GeneratedCandidates, int ActivePops, int RejectedTooClose, int RejectedDomain,
    double LocalRadiusMin, double LocalRadiusMax) : IValidityEvidence {
```

## Why

The two payload-free rows encode one binary census fact and `CandidateOnly` immediately re-derives it. Both producers always supply background cell size and grid-cell count, so their `Option` wrappers also encode impossible absence.

## Change

Store `CandidateOnly` directly, make both always-present background measurements bare, delete the generated roster and forwarding property, and mint the values at the two execution sites.

## Delta

Code-fence LOC: `-8`. Module-level types: `-1`. Members: `-3` (`ContinuousMesh`, `CandidateSet`, and derived `CandidateOnly`; `Domain` is replaced). Optional carriers: `-2`.

# 5. Replace the mirrored CCVT gauge roster with pin data

## From

`libs/dotnet/Rasm/.planning/Processing/sample.md:134`

```csharp
[SmartEnum<int>]
public sealed partial class PowerCcvtGauge {
    public static readonly PowerCcvtGauge ZeroMean = new(key: 0);
    public static readonly PowerCcvtGauge PinIndexZero = new(key: 1);
    internal GaugePolicy Policy(Arr<double> fragmentMasses) => Switch(
        state: fragmentMasses,
        zeroMean: static mass => GaugePolicy.MeanZeroConstant(dimension: mass.Count, mass: Some(mass), shift: GaugeShift.MeanZero),
        pinIndexZero: static mass => GaugePolicy.Pinned(indices: [0], mass: Some(mass), shift: GaugeShift.MeanZero));
}
```

`libs/dotnet/Rasm/.planning/Processing/sample.md:402`

```csharp
CapacityPolicy Capacity, MotionPolicy Motion, ArmijoPolicy Search, RegularityPolicy Regularity,
PowerCcvtGauge Gauge, int Seed) {
```

`libs/dotnet/Rasm/.planning/Processing/sample.md:332`

```csharp
double NormalizedPoissonRadius, double PlanarityDeviation, PowerCcvtGauge Gauge, PowerCcvtStopKind Stop,
```

## To

```csharp
// PowerCcvtGauge DELETED
```

```csharp
CapacityPolicy Capacity, MotionPolicy Motion, ArmijoPolicy Search, RegularityPolicy Regularity,
Option<int> PinnedSite, int Seed) {
```

```csharp
double NormalizedPoissonRadius, double PlanarityDeviation, Option<int> PinnedSite, PowerCcvtStopKind Stop,
```

## Why

`GaugePolicy` already owns the gauge algebra. `PowerCcvtGauge` mirrors two of its cases, hard-codes the only pin to zero, and applies a mean-zero post-shift after pinning, which destroys the pinned value.

## Change

Represent the caller choice as optional pin data: absence selects `GaugePolicy.MeanZeroConstant`, presence selects `GaugePolicy.Pinned(indices: [site], ..., shift: GaugeShift.None)`. Admit nonnegative pins once on `PowerCcvtPolicy`, carry the pin on the solution, and delete the second gauge vocabulary.

## Delta

Code-fence LOC: `-5`. Module-level types: `-1`. Members: `-3` (`ZeroMean`, `PinIndexZero`, and `Policy`; `Gauge` is replaced).

## Ripples

- `libs/dotnet/Rasm/.planning/Processing/sample.md:27-32`: replace `PowerCcvtGauge` growth language with direct `GaugePolicy` composition and the optional pin coordinate.

# 6. Store CCVT convergence as the binary result fact

## From

`libs/dotnet/Rasm/.planning/Processing/sample.md:144`

```csharp
[SmartEnum<int>]
public sealed partial class PowerCcvtStopKind {
    public static readonly PowerCcvtStopKind Converged = new(key: 0);
    public static readonly PowerCcvtStopKind StoppedWithoutConvergence = new(key: 1);
}
```

`libs/dotnet/Rasm/.planning/Processing/sample.md:334`

```csharp
public bool MeanZeroGaugeApplied =>
    Gauge.Equals(PowerCcvtGauge.ZeroMean)
    && DualSolve.Bind(static solve => solve.Gauge).Exists(static gauge => gauge.PostShiftApplied.Equals(GaugeShift.MeanZero));
```

`libs/dotnet/Rasm/.planning/Processing/sample.md:332`

```csharp
double NormalizedPoissonRadius, double PlanarityDeviation, PowerCcvtGauge Gauge, PowerCcvtStopKind Stop,
```

## To

```csharp
// PowerCcvtStopKind DELETED
```

```csharp
// PowerCcvtSolution.MeanZeroGaugeApplied DELETED
```

```csharp
double NormalizedPoissonRadius, double PlanarityDeviation, Option<int> PinnedSite, bool Converged,
```

```csharp
PinnedSite.IsSome || DualSolve.Bind(static solve => solve.Gauge)
    .Exists(static gauge => gauge.PostShiftApplied.Equals(GaugeShift.MeanZero)),
```

## Why

The stop roster is a payload-free two-case family over a boolean the outer run already holds. `MeanZeroGaugeApplied` is an unconsumed forwarding property and is absent from `IsValid`, despite the result claiming the gauge witness.

## Change

Replace `Stop` with `bool Converged`, delete the stop type, and move the mean-zero solve witness into `IsValid`; pinned solves bypass that post-shift requirement while retaining their solve evidence.

## Delta

Code-fence LOC: `-8`. Module-level types: `-1`. Members: `-3` (`2` rows and `MeanZeroGaugeApplied`; `Stop` is replaced).

# 7. Fold one-consumer power-cell facts into the CCVT result

## From

`libs/dotnet/Rasm/.planning/Processing/sample.md:313`

```csharp
[StructLayout(LayoutKind.Auto)]
public readonly record struct PowerCellFragmentFacts(
    int SiteCount, int FragmentCount, int FacetCount, int EmptyCellCount,
    Stat<Scalar> Mass, double IntegrationResidual) : IValidityEvidence {
    public double TotalMass => Mass.Mean * Mass.Count;
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.CountAtLeast(SiteCount, 1), ValidityClaim.CountAtLeast(FragmentCount, 0),
        ValidityClaim.CountAtLeast(FacetCount, 0), ValidityClaim.CountAtLeast(EmptyCellCount, 0),
        EmptyCellCount <= SiteCount, ValidityClaim.Evidence(Mass), ValidityClaim.Nonnegative(IntegrationResidual));
}
```

`libs/dotnet/Rasm/.planning/Processing/sample.md:332`

```csharp
double NormalizedPoissonRadius, double PlanarityDeviation, PowerCcvtGauge Gauge, PowerCcvtStopKind Stop,
PowerCellFragmentFacts Fragments, Option<LinearSolution> DualSolve = default, Option<SamplingSpectrum> Spectrum = default) : IValidityEvidence {
```

## To

```csharp
// PowerCellFragmentFacts DELETED
```

```csharp
double NormalizedPoissonRadius, double PlanarityDeviation, Option<int> PinnedSite, bool Converged,
int FragmentCount, int FacetCount, Stat<Scalar> CellMass, double IntegrationResidual,
Option<LinearSolution> DualSolve = default) : IValidityEvidence {
```

```csharp
ValidityClaim.CountAtLeast(FragmentCount, 0), ValidityClaim.CountAtLeast(FacetCount, 0),
ValidityClaim.CountAtLeast(EmptyCellCount, 0), EmptyCellCount <= SiteCount,
ValidityClaim.Evidence(CellMass), ValidityClaim.Nonnegative(IntegrationResidual),
```

## Why

`PowerCellFragmentFacts` has one constructor and one consumer, repeats `SiteCount` and `EmptyCellCount` already held by `PowerCcvtSolution`, and adds a public type solely to group five values. `TotalMass` is unconsumed and directly derivable.

## Change

Move fragment count, facet count, cell-mass statistics, and integration residual onto `PowerCcvtSolution`; keep the existing solution-level site and empty-cell counts as the single authorities. Inline the child validity clauses and delete the wrapper type.

## Delta

Code-fence LOC: `-10`. Module-level types: `-1`. Members: `-3` net (`TotalMass`, duplicate `SiteCount`, and the `Fragments` member removed; four genuine facts move).

## Ripples

- `libs/dotnet/Rasm/.planning/Processing/sample.md:31`: describe cell facts as columns on `PowerCcvtSolution`, not a nested wrapper.

# 8. Flatten one-consumer CCVT policy shells

## From

`libs/dotnet/Rasm/.planning/Processing/sample.md:396`

```csharp
public sealed record CapacityPolicy(Dimension MaxNewton);
public sealed record MotionPolicy(Dimension LloydSweeps, Dimension GradientSteps, PositiveMagnitude LloydPosTol, PositiveMagnitude GradPosTol);
public sealed record ArmijoPolicy(PositiveMagnitude Backtrack, PositiveMagnitude InitialStep, Dimension MaxHalvings);
public sealed record RegularityPolicy(PositiveMagnitude AliasScale, PositiveMagnitude JitterVariance, PositiveMagnitude MagnitudeScale, PositiveMagnitude RelocateFraction);

public sealed record PowerCcvtPolicy(
    Dimension Iterations, Option<ScalarField> Density,
    CapacityPolicy Capacity, MotionPolicy Motion, ArmijoPolicy Search, RegularityPolicy Regularity,
    PowerCcvtGauge Gauge, int Seed) {
```

`libs/dotnet/Rasm/.planning/Processing/sample.md:416`

```csharp
from aliasScale in key.AcceptValidated<PositiveMagnitude>(candidate: 0.65)
from jitterVariance in key.AcceptValidated<PositiveMagnitude>(candidate: 0.05)
from magnitudeScale in key.AcceptValidated<PositiveMagnitude>(candidate: 0.5)
from relocateFraction in key.AcceptValidated<PositiveMagnitude>(candidate: 0.05)
```

## To

```csharp
// CapacityPolicy DELETED
// MotionPolicy DELETED
// ArmijoPolicy DELETED
// RegularityPolicy DELETED
```

```csharp
public sealed record PowerCcvtPolicy(
    Dimension Iterations, Option<ScalarField> Density, Dimension MaxNewton,
    Dimension LloydSweeps, Dimension GradientSteps,
    PositiveMagnitude LloydTolerance, PositiveMagnitude GradientTolerance,
    PositiveMagnitude Backtrack, PositiveMagnitude InitialStep, Dimension MaxHalvings,
    PositiveMagnitude AliasScale, PositiveMagnitude JitterScale, PositiveMagnitude RelocateFraction,
    Option<int> PinnedSite, int Seed) {
```

```csharp
from aliasScale in key.AcceptValidated<PositiveMagnitude>(candidate: 0.65)
from jitterScale in key.AcceptValidated<PositiveMagnitude>(candidate: 0.025)
from relocateFraction in key.AcceptValidated<PositiveMagnitude>(candidate: 0.05)
```

## Why

Each nested record is constructed once, consumed only through `PowerCcvtPolicy`, and owns no independent invariant or external consumer. `JitterVariance` is not a variance, and it and `MagnitudeScale` are never observed independently; only their product affects the run.

## Change

Move the fields onto the run policy, rename both positional thresholds by the quantity they bound, replace the non-identifiable jitter pair with one `JitterScale` preset to the existing product `0.025`, and rewrite every nested member read to the direct column.

## Delta

Code-fence LOC: `-9`. Module-level types: `-4`. Members: `-1` net because two jitter knobs become one; all other positional members move.

## Ripples

- `libs/dotnet/Rasm/.planning/Processing/sample.md:27-32`: describe one flat CCVT policy and one jitter scale; remove the four deleted policy type names.

# 9. Seat sampling internals under the kernel owner

## From

`libs/dotnet/Rasm/.planning/Processing/sample.md:433`

```csharp
internal readonly record struct SampleCandidate(Point3d Point, Option<double> Mass);
internal readonly record struct SampleResult(Seq<Point3d> Points, Option<Arr<double>> Mass, SampleTally Tally);
internal readonly record struct SampleSelection(Point3d[] Points, Option<Arr<double>> Mass, Option<int> DensityAccepted, Option<int> DensityRejected, Option<SampleAlgorithm> Algorithm);

internal static class Spacing {
```

`libs/dotnet/Rasm/.planning/Processing/sample.md:467`

```csharp
}

internal static class SampleKernel {
```

## To

```csharp
internal static class SampleKernel {
    internal readonly record struct Result(Seq<Point3d> Points, Option<Arr<double>> Mass, SampleTally Tally);
    private readonly record struct Candidate(Point3d Point, Option<double> Mass);
    private readonly record struct Selection(Point3d[] Points, Option<Arr<double>> Mass, Option<int> DensityAccepted, Option<int> DensityRejected, SampleAlgorithm Algorithm);

    private static double HexagonalSpacing(double measure, int count) =>
```

## Why

The three transport structs and `Spacing` have no consumer outside `SampleKernel`. Their module-level placement exports implementation details, and the wrapper class adds a type only to prefix four kernel-private calculations.

## Change

Nest the carriers under `SampleKernel`, shorten owner-redundant names, move the spacing members directly onto the kernel, and qualify only the external target references as `SampleKernel.Result`. Rewrite internal uses to `Candidate`, `Result`, and `Selection`.

## Delta

Code-fence LOC: `-2`. Module-level types: `-4`; total types: `-1` because the spacing shell disappears. Members: unchanged and narrowed to the kernel owner.

## Ripples

- `libs/dotnet/Rasm/.planning/Processing/segment.md:137`: change the internal signature to `Fin<SampleKernel.Result>`.
- `libs/dotnet/Rasm/.planning/Processing/sample.md:249-286`: qualify `Evaluate`, `Project`, and projection-owner references with `SampleKernel.Result`.
- `libs/dotnet/Rasm/.planning/Processing/sample.md:23`: seat nearest-spacing calculation under `SampleKernel`.

# 10. Traverse fallible weight admission without staging mutation

## From

`libs/dotnet/Rasm/.planning/Processing/sample.md:941`

```csharp
double[] weights = new double[candidates.Count];
return toSeq(Enumerable.Range(start: 0, count: candidates.Count)).Fold(
    initialState: Fin.Succ((Accepted: 0, Rejected: 0, Band: Option<(double Min, double Max)>.None)),
    f: (state, i) => state.Bind(current => density.SampleScalar(sample: candidates[index: i].Point, context: context, key: key)
        .Bind(value => value > 0.0 && double.IsFinite(value)
            ? key.AcceptValue(value: value * candidates[index: i].Mass.IfNone(1.0)).Map(valid => { weights[i] = valid; return (current.Accepted + 1, current.Rejected, Some(Widen(current.Band, valid))); })
            : Fin.Succ((current.Accepted, current.Rejected + 1, current.Band)))))
```

`libs/dotnet/Rasm/.planning/Processing/sample.md:981`

```csharp
[StructLayout(LayoutKind.Auto)] private readonly record struct DworkCandidate(int Index, double Radius);
private static Fin<SampleSelection> DworkCandidateSelection(Seq<SampleCandidate> candidates, ScalarField radius, int count, double minRadius, Dimension attempts, int seed, Context context, Op key) {
    DworkCandidate[] admitted = new DworkCandidate[candidates.Count];
    return toSeq(Enumerable.Range(start: 0, count: candidates.Count)).Fold(
        initialState: Fin.Succ((Accepted: 0, Rejected: 0, Band: Option<(double Min, double Max)>.None)),
```

## To

```csharp
return toSeq(Enumerable.Range(start: 0, count: candidates.Count))
    .TraverseM(i => density.SampleScalar(sample: candidates[index: i].Point, context: context, key: key)
        .Bind(value => value > 0.0 && double.IsFinite(value)
            ? key.AcceptValue(value * candidates[index: i].Mass.IfNone(1.0)).Map(valid => Some((Index: i, Value: valid)))
            : Fin.Succ(Option<(int Index, double Value)>.None)))
    .As()
```

```csharp
// DworkCandidate DELETED
```

```csharp
return toSeq(Enumerable.Range(start: 0, count: candidates.Count))
    .TraverseM(i => radius.SampleScalar(sample: candidates[index: i].Point, context: context, key: key)
        .Bind(value => value > 0.0 && double.IsFinite(value)
            ? key.AcceptValue(Math.Max(minRadius, value)).Map(local => Some((Index: i, Radius: local)))
            : Fin.Succ(Option<(int Index, double Radius)>.None)))
    .As()
```

## Why

Both routines manually thread `Fin` through a fold while mutating an out-of-band array. Their accepted count, rejected count, and min/max band are derivable from the admitted rows. LanguageExt `TraverseM` already owns fallible mapping and first-failure sequencing, and `Somes` owns optional-row compaction.

## Change

Traverse each candidate into an optional admitted tuple, call `Somes`, derive counts and extrema once, and continue with the existing selection body. Use named tuples throughout the Dwork candidate-local algorithm and delete its private record.

## Delta

Code-fence LOC: `-14`. Nested types: `-1`. Mutable staging arrays: `-2`. Members: unchanged.

# 11. Localize transient iteration state and farthest optimization

## From

`libs/dotnet/Rasm/.planning/Processing/sample.md:876`

```csharp
[StructLayout(LayoutKind.Auto)] private readonly record struct CapacityResidual(double Inf, double L1, double L2, double Normalized);
[StructLayout(LayoutKind.Auto)] private readonly record struct SiteMotion(Seq<Point3d> Sites, int LloydIterations, int GradientIterations, int GradientHalvings, double Displacement, double PositionGradientNorm);
[StructLayout(LayoutKind.Auto)] private readonly record struct Regularity(Seq<Point3d> Sites, int AliasedCount, int RelocatedCount);
```

`libs/dotnet/Rasm/.planning/Processing/sample.md:1264`

```csharp
[StructLayout(LayoutKind.Auto)] private readonly record struct CapacityAssignment(int[] Hits, int Assigned, int Unassigned, double Residual);
```

`libs/dotnet/Rasm/.planning/Processing/sample.md:1382`

```csharp
[StructLayout(LayoutKind.Auto)] private readonly record struct FpoState(int[] Chosen, double BestScore, bool Settled);
private static int[] FpoSample(Seq<SampleCandidate> candidates, int count, int iterations, Op key) {
```

`libs/dotnet/Rasm/.planning/Processing/sample.md:1395`

```csharp
private static FpoState SwapRound(Seq<SampleCandidate> candidates, FpoState state) =>
```

## To

```csharp
// CapacityResidual DELETED
// SiteMotion DELETED
// Regularity DELETED
// CapacityAssignment DELETED
// FpoState DELETED
```

```csharp
private static int[] OptimizeFarthest(Seq<Candidate> candidates, int count, int iterations, Op key) {
    Option<(int Index, double Distance)> Worst(int[] chosen) =>
```

```csharp
(int[] Chosen, double BestScore, bool Settled) Swap((int[] Chosen, double BestScore, bool Settled) state) =>
```

## Why

The deleted records are behavior-free transition payloads. Named tuples preserve their labels without minting five types. `Fpo` is an opaque abbreviation, and `SwapRound` plus `WorstCoverage` serve only that optimization entry while occupying sibling kernel-member slots.

## Change

Use named tuples for the residual, motion, alias, assignment, and farthest-optimization states. Rename `FpoSample` to `OptimizeFarthest`, move its score and swap functions into the method, and replace the two record `with` expressions with explicit tuple returns. Keep `NewtonState` and `OuterState` because their repeated state transitions justify records.

## Delta

Code-fence LOC: `-7`. Nested types: `-5`. Kernel members: `-2` (`SwapRound` and `WorstCoverage`).

# 12. Delete forwarding and derivable helpers

## From

`libs/dotnet/Rasm/.planning/Processing/sample.md:503`

```csharp
private static Seq<SampleCandidate> LatticeCandidates(CellLattice grid) =>
    toSeq(Enumerable.Range(start: 0, count: (int)Math.Min(val1: grid.CellCount, val2: int.MaxValue))
        .Select(linear => grid.Coordinate(linear: linear) switch {
            (int column, int row, int layer) => new SampleCandidate(Point: grid.Center(column: column, row: row, layer: layer), Mass: Option<double>.None),
        }));
```

`libs/dotnet/Rasm/.planning/Processing/sample.md:785`

```csharp
private Arr<double> FragmentMasses(RestrictedPowerDiagram diagram) =>
    new([.. Enumerable.Range(start: 0, count: siteCount).Select(i => Math.Max(val1: diagram.Cells[index: i].Mass, val2: 0.0))]);
```

`libs/dotnet/Rasm/.planning/Processing/sample.md:886`

```csharp
internal static OuterState Of(Seq<Point3d> sites, NewtonState capacity) => new(
    Sites: sites, Capacity: capacity, OuterIterations: 0, LloydIterations: 0, GradientIterations: 0,
    StepHalvings: capacity.StepHalvings, PositionGradientNorm: 0.0,
    TransportEnergyDelta: 0.0, Converged: false, Fault: Option<Error>.None);
```

`libs/dotnet/Rasm/.planning/Processing/sample.md:939`

```csharp
private static Fin<Arr<double>> NormalizeMass(Seq<double> mass, Op key) =>
    CloudKernel.MassOf(mass: new Arr<double>([.. mass.AsIterable()]), count: mass.Count, key: key);
```

## To

```csharp
// LatticeCandidates DELETED
```

```csharp
// FragmentMasses DELETED
```

```csharp
// OuterState.Of DELETED
```

```csharp
// NormalizeMass DELETED
```

## Why

The lattice projection, fragment-mass projection, and outer-state seed each have one call site and no meaning outside that expression. `NormalizeMass` is a thin rename of `CloudKernel.MassOf` and adds no sampling policy despite having three callers.

## Change

Inline each one-call projection or seed at its use, and call `CloudKernel.MassOf` directly at the three normalization sites. Do not introduce replacement helper names.

## Delta

Code-fence LOC: `-14`. Members: `-4`. Types: unchanged.

# 13. Make CCVT setup one entry on the run owner

## From

`libs/dotnet/Rasm/.planning/Processing/sample.md:605`

```csharp
private static Fin<SampleResult> PowerCcvtMeshSolve(MeshSpace domain, SampleKind.PowerCcvtCase kind, Context context, Op key) {
```

`libs/dotnet/Rasm/.planning/Processing/sample.md:616`

```csharp
private static Fin<(Plane Plane, double Deviation)> CanonicalPlaneOf(Seq<Point3d> points, Op key) =>
```

`libs/dotnet/Rasm/.planning/Processing/sample.md:621`

```csharp
private static Seq<Point3d> DensityImportanceSites(Seq<Point3d> candidates, int count, Option<ScalarField> density, Context context, int seed, Op key) =>
```

`libs/dotnet/Rasm/.planning/Processing/sample.md:637`

```csharp
private sealed class PowerCcvtRun(MeshSpace domain, Dimension count, PowerCcvtPolicy policy, Seq<Point3d> sites, double totalMass, double planarityDeviation, Context context, Op key) {
```

## To

```csharp
// PowerCcvtMeshSolve DELETED
// CanonicalPlaneOf DELETED
// DensityImportanceSites DELETED
```

```csharp
private sealed class PowerCcvtRun(MeshSpace domain, SampleKind.PowerCcvtCase kind, Seq<Point3d> sites, double totalMass, double planarityDeviation, Context context, Op key) {
    internal static Fin<Result> Execute(MeshSpace domain, SampleKind.PowerCcvtCase kind, Context context, Op key) {
```

```csharp
Fin<(Plane Plane, double Deviation)> Fit(Seq<Point3d> points) =>
```

```csharp
Fin<Seq<Point3d>> Sites(Seq<Point3d> candidates, int count) => kind.Policy.Density.Match(
    Some: field => candidates.TraverseM(point =>
        field.SampleScalar(sample: point, context: context, key: key).Map(weight => (Point: point, Weight: weight))).As()
        .Map(rows => toSeq(rows.AsIterable()
            .Where(static row => double.IsFinite(row.Weight) && row.Weight > 0.0)
            .OrderBy(row => -Math.Log(Deterministic.UnitInterval(row.Point, SampleLane.Priority.Lane, kind.Policy.Seed)) / row.Weight)
            .Take(count).Select(static row => row.Point))),
    None: () => Fin.Succ(toSeq(FarthestIndices(candidates.Map(static point => new Candidate(point, None)), count)
        .Select(i => candidates[i]))));
```

## Why

Three `SampleKernel` siblings exist only to construct one `PowerCcvtRun`. The run also stores `Count` and `Policy` separately although the admitted `PowerCcvtCase` already carries both. `DensityImportanceSites` additionally converts a failed scalar-field evaluation into weight zero, turning a typed failure into a successful omission.

## Change

Expose `PowerCcvtRun.Execute` as the single setup entry, retain plane fitting and site choice as local functions, change the mesh dispatch to that entry, and store the admitted CCVT case instead of duplicating its fields. Traverse density evaluations on `Fin`, bind the admitted sites before constructing the run, and propagate a field failure unchanged. Keep `SurfaceCandidatePoints` on `SampleKernel` because the non-CCVT mesh path also consumes it.

## Delta

Code-fence LOC: `-3`. Kernel members: `-2` net (`3` siblings replaced by one run entry). Run fields: `-1`. Types: unchanged.

# 14. Share one Armijo search and refuse exhausted steps

## From

`libs/dotnet/Rasm/.planning/Processing/sample.md:708`

```csharp
private (Seq<Point3d> Sites, Option<RestrictedPowerDiagram> Diagram, int Halvings) AscentLineSearch(Seq<Point3d> currentSites, Vector3d[] direction, double slope, double baseEnergy, Arr<double> weights, double alpha, int halvings) {
    Seq<Point3d> trial = toSeq(Enumerable.Range(start: 0, count: siteCount).Select(i => currentSites[index: i] + (alpha * direction[i])));
    return RebuildPowerCells(currentSites: trial, weights: weights).Match(
        Succ: diagram => -TransportEnergyOf(diagram: diagram) >= baseEnergy + (SufficientDecrease * alpha * slope)
            ? (trial, Some(diagram), halvings)
            : Backtrack(),
        Fail: _ => Backtrack());
```

`libs/dotnet/Rasm/.planning/Processing/sample.md:760`

```csharp
private Fin<NewtonState> AscentSearch(Seq<Point3d> currentSites, NewtonState state, Arr<double> direction, double slope, double baseObjective, double alpha, int halvings) {
    Arr<double> advanced = new([.. Enumerable.Range(start: 0, count: siteCount).Select(i => state.Weights[index: i] + (alpha * direction[index: i]))]);
    return RebuildDiagram(currentSites: currentSites, weights: advanced).Bind(rebuilt =>
        rebuilt.DualObjective >= baseObjective + (SufficientDecrease * alpha * slope) || halvings >= policy.Search.MaxHalvings.Value
            ? Fin.Succ(rebuilt with { StepHalvings = state.StepHalvings + halvings, NewtonIterations = state.NewtonIterations })
            : AscentSearch(currentSites: currentSites, state: state, direction: direction, slope: slope, baseObjective: baseObjective, alpha: alpha * policy.Search.Backtrack.Value, halvings: halvings + 1));
}
```

## To

```csharp
// AscentLineSearch DELETED
// AscentSearch DELETED
```

```csharp
private Fin<(T State, int Halvings)> Armijo<T>(double baseline, double slope, Func<double, Fin<(T State, double Objective)>> trial) =>
    toSeq(Enumerable.Range(start: 0, count: kind.Policy.MaxHalvings.Value + 1)).Fold(
        initialState: Fin.Fail<(T State, int Halvings)>(key.InvalidResult()),
        f: (accepted, halvings) => accepted.Match(
            Succ: static found => Fin.Succ(found),
            Fail: _ => {
                double step = kind.Policy.InitialStep.Value * Math.Pow(kind.Policy.Backtrack.Value, halvings);
                return trial(step).Bind(candidate => candidate.Objective >= baseline + (SufficientDecrease * step * slope)
                    ? Fin.Succ((candidate.State, halvings))
                    : Fin.Fail<(T State, int Halvings)>(key.InvalidResult()));
            }));
```

## Why

The two recursive methods implement the same Armijo backtracking policy with different carriers. The weight path currently accepts the last candidate solely because the budget was exhausted, certifying a non-improving step as success and violating the typed-exhaustion law.

## Change

Use one bounded generic search that evaluates decreasing step sizes, retains the first sufficient improvement, and returns typed failure when no step passes. Supply site-motion and weight-update trial lambdas at the two call sites; the callers retain their distinct state projections while the acceptance rule and budget have one owner.

## Delta

Code-fence LOC: `-12`. Members: `-1` net (`2` searches replaced by `1`). Duplicate acceptance bodies: `-1`.

# 15. Localize single-consumer algorithm helpers

## From

`libs/dotnet/Rasm/.planning/Processing/sample.md:869`

```csharp
private static Vector3d JitterOffset(Deterministic.Draw draw, double magnitude) {
```

`libs/dotnet/Rasm/.planning/Processing/sample.md:861`

```csharp
private bool[] AliasMask(Seq<Point3d> currentSites, double radius) =>
    (from index in NeighborIndex.Of(source: new NeighborSource.PointsCase(Values: currentSites), key: key)
     from reach in key.AcceptValidated<PositiveMagnitude>(candidate: radius)
     from graph in NeighborKernel.GraphOf(index: index, needles: [.. currentSites.AsIterable()], count: Option<Dimension>.None, radius: Some(reach), key: key)
     select graph.Ids)
    .Match(
        Succ: ids => [.. Enumerable.Range(start: 0, count: currentSites.Count).Select(i => ids.Length > i && ids[i].Any(id => id >= 0 && id < i))],
        Fail: _ => new bool[currentSites.Count]);
```

`libs/dotnet/Rasm/.planning/Processing/sample.md:1314`

```csharp
private static UndirectedGraph<int, TaggedEdge<int, double>> ConflictGraph(SampleCandidate[] input, int[][] ids, double dMax, double dMin, double alpha) {
```

`libs/dotnet/Rasm/.planning/Processing/sample.md:1325`

```csharp
private static (int[] Indices, int Eliminated, int NeighborUpdates) Eliminate(UndirectedGraph<int, TaggedEdge<int, double>> graph, SampleCandidate[] input, int count, int seed) {
```

`libs/dotnet/Rasm/.planning/Processing/sample.md:1426`

```csharp
private static Fin<int[]> RelaxSites(int[] sites, Seq<SampleCandidate> candidates, NeighborIndex candidateIndex, int total, Option<int> capacity, Op key) {
```

## To

```csharp
// JitterOffset DELETED
```

```csharp
private Fin<bool[]> AliasMask(Seq<Point3d> currentSites, double radius) =>
    from index in NeighborIndex.Of(source: new NeighborSource.PointsCase(Values: currentSites), key: key)
    from reach in key.AcceptValidated<PositiveMagnitude>(candidate: radius)
    from graph in NeighborKernel.GraphOf(index: index, needles: [.. currentSites.AsIterable()], count: Option<Dimension>.None, radius: Some(reach), key: key)
    select Enumerable.Range(0, currentSites.Count)
        .Select(i => graph.Ids.Length > i && graph.Ids[i].Any(id => id >= 0 && id < i)).ToArray();
```

```csharp
// ConflictGraph DELETED
// Eliminate DELETED
```

```csharp
// RelaxSites DELETED
```

## Why

Each helper has exactly one owning caller and no independent policy: jitter construction belongs to alias resolution, graph construction and elimination belong to Yuksel elimination, and site relaxation belongs to the Lloyd iteration. Their sibling placement expands the kernel member surface without reuse. `AliasMask` also converts neighbourhood-construction failure into an all-clear mask, certifying failed evidence as absence of aliases.

## Change

Move `JitterOffset` into alias resolution, `ConflictGraph` and `Eliminate` into `SampleElimination`, and `RelaxSites` into `RelaxationSample` as local functions. Return `AliasMask` on `Fin`, bind it in regularity breaking, and propagate neighbour failures instead of substituting `false`. Preserve QuikGraph `UndirectedGraph`, `TaggedEdge`, `AdjacentEdges`, and `BinaryQueue.Update` directly; do not replace package algorithms with new wrappers.

## Delta

Code-fence LOC: `-2`. Members: `-4`. Types: unchanged.

# 16. Derive spatial-rank formulas without a generated roster

## From

`libs/dotnet/Rasm/.planning/Processing/sample.md:105`

```csharp
[SmartEnum<int>]
public sealed partial class SpatialRank {
    public static readonly SpatialRank Planar = new(key: 2, packingDensity: 2.0 * Math.Sqrt(d: 3.0), exponent: 0.5);
    public static readonly SpatialRank Volumetric = new(key: 3, packingDensity: 4.0 * Math.Sqrt(d: 2.0), exponent: 1.0 / 3.0);

    public double PackingDensity { get; }
    public double Exponent { get; }
    internal double MaxRadius(double measure, int count) =>
        2.0 * Math.Pow(x: measure / Math.Max(val1: 1, val2: count) / PackingDensity, y: Exponent);
    internal double MeanSpacing(double measure, int count) =>
        Math.Pow(x: measure / Math.Max(val1: 1, val2: count), y: Exponent);
    internal static SpatialRank Of(int rank) => rank >= 3 ? Volumetric : Planar;
}
```

`libs/dotnet/Rasm/.planning/Processing/sample.md:1351`

```csharp
private static (SpatialRank Rank, double Measure) BoundingMeasure(Seq<SampleCandidate> candidates) {
```

## To

```csharp
// SpatialRank DELETED
```

```csharp
private static double MeanSpacing(int rank, double measure, int count) =>
    Math.Pow(measure / Math.Max(1, count), 1.0 / rank);
private static double MaxRadius(int rank, double measure, int count) =>
    2.0 * Math.Pow(measure / Math.Max(1, count)
        / (rank >= 3 ? 4.0 * Math.Sqrt(2.0) : 2.0 * Math.Sqrt(3.0)), 1.0 / rank);
```

```csharp
private static (int Rank, double Measure) BoundingMeasure(Seq<Candidate> candidates) {
```

## Why

`SpatialRank` has no external consumer and no independently extensible vocabulary. Its two rows only cache `1 / rank` and one packing constant, while callers already possess the lattice rank or derive planar versus volumetric measure from bounds.

## Change

Carry the ambient rank as `int` in the private measure tuple, normalize lattice ranks to `2` or `3` at the one intake, and derive spacing and elimination radius directly in two kernel-private formulas. Delete the smart enum, its two instances, two stored columns, and `Of` conversion.

## Delta

Code-fence LOC: `-7`. Module-level types: `-1`. Members: `-5` net (`7` generated-owner members replaced by `2` private formulas).

# 17. Dispatch the generated sample union through generated folds

## From

`libs/dotnet/Rasm/.planning/Processing/sample.md:483`

```csharp
internal static Fin<SampleResult> Sample(SampleKind kind, ExtractionDomain domain, Context context, Op key) =>
    kind switch {
        SampleKind.ExplicitCase explicitCase => SampleAdmitted(
```

`libs/dotnet/Rasm/.planning/Processing/sample.md:553`

```csharp
private static Fin<SampleResult> SampleOnMesh(SampleKind kind, MeshSpace domain, Context context, Op key) {
    if (kind is SampleKind.PowerCcvtCase power) return PowerCcvtMeshSolve(domain: domain, kind: power, context: context, key: key);
    if (kind is SampleKind.DworkVariableDensityCase dwork)
```

`libs/dotnet/Rasm/.planning/Processing/sample.md:491`

```csharp
cloudCase: static (state, d) => d.Value is VectorCloud.ClusterCase cluster
```

`libs/dotnet/Rasm/.planning/Processing/sample.md:895`

```csharp
private static Fin<SampleResult> SampleOnCandidates(SampleKind kind, Seq<SampleCandidate> candidates, bool admitsPoisson, Option<(SpatialRank Rank, double Measure)> domainMeasure, Context context, Op key) =>
    from selection in kind switch {
```

## To

```csharp
internal static Fin<Result> Sample(SampleKind kind, ExtractionDomain domain, Context context, Op key) =>
    kind.SwitchPartially(
        state: (Domain: domain, Context: context, Key: key),
        @default: static (state, value) => state.Domain.Switch(
            state: (Kind: value, state.Context, state.Key),
            supportCase: static (s, d) => SampleGeneratedSupport(s.Kind, d.Value, s.Context, s.Key),
            meshCase: static (s, d) => SampleOnMesh(s.Kind, d.Value, s.Context, s.Key),
            cloudCase: static (s, d) => d.Value.SwitchPartially(
                state: (s.Kind, s.Context, s.Key),
                @default: static (held, value) => Fin.Fail<Result>(held.Key.Unsupported(inputType: value.GetType(), outputType: typeof(Result))),
                clusterCase: static (held, cluster) => CloudKernel.MassOf(cluster: cluster, key: held.Key).Bind(mass => SampleOnCandidates(held.Kind,
                    cluster.Vertices.Map((point, index) => new Candidate(point, Some(mass[index]))), false, None, held.Context, held.Key))),
            latticeCase: static (s, d) => SampleOnCandidates(s.Kind,
                toSeq(Enumerable.Range(0, (int)Math.Min(d.Value.CellCount, int.MaxValue)).Select(linear => d.Value.Coordinate(linear) switch {
                    (int column, int row, int layer) => new Candidate(d.Value.Center(column, row, layer), None),
                })), true, Some((Rank: d.Value.Rank >= 3 ? 3 : 2, Measure: d.Value.CellCount * d.Value.CellMeasure)), s.Context, s.Key)),
        explicitCase: static (state, value) => SampleAdmitted(value.Points.Map(static point => new Candidate(point, None)), state.Domain, value, state.Context, state.Key),
        weightedCase: static (state, value) => SampleAdmitted(value.Points.Map(static item => new Candidate(item.Point, Some(item.Mass))), state.Domain, value, state.Context, state.Key));
```

```csharp
private static Fin<Result> SampleOnMesh(SampleKind kind, MeshSpace domain, Context context, Op key) =>
    kind.SwitchPartially(
        state: (Domain: domain, Context: context, Key: key),
        @default: static (state, value) => Optional(AreaMassProperties.Compute(mesh: state.Domain.Native, area: true, firstMoments: false, secondMoments: false, productMoments: false)).ToFin(state.Key.InvalidResult()).Bind(props =>
            from density in value.MeshCandidateDensity(props.Area, state.Key)
            from candidates in SurfaceCandidatePoints(state.Domain, density, state.Key)
            from sampled in SampleOnCandidates(value, candidates.Map(static point => new Candidate(point, None)), true,
                Some((Rank: 2, Measure: props.Area)), state.Context, state.Key)
            from validated in SegmentKernel.ValidateSamplingSpectrum(state.Domain, sampled, state.Key)
            select validated),
        powerCcvtCase: static (state, value) => PowerCcvtRun.Execute(state.Domain, value, state.Context, state.Key),
        dworkVariableDensityCase: static (state, value) =>
            from selection in DworkMeshRun.Execute(state.Domain, value.Radius, value.Count.Value, value.MinRadius.Value, value.Attempts, value.Seed, state.Context, state.Key)
            let points = toSeq(selection.Points)
            let census = selection.Algorithm.Dwork.ToFin(state.Key.InvalidResult())
            from dwork in census
            let result = new Result(points, selection.Mass, TallyOf(dwork.GeneratedCandidates, points,
                dwork.RejectedTooClose + dwork.RejectedDomain, None, selection.Algorithm, state.Key))
            from validated in SegmentKernel.ValidateSamplingSpectrum(state.Domain, result, state.Key)
            select validated);
```

```csharp
private static Fin<Selection> Select(
    SampleKind kind,
    (Seq<Candidate> Candidates, bool AdmitsPoisson, Option<(int Rank, double Measure)> DomainMeasure, Context Context, Op Key) input) =>
    kind.SwitchPartially(
        state: input,
        @default: static (state, value) => Fin.Fail<Selection>(state.Key.Unsupported(inputType: value.GetType(), outputType: typeof(Result))),
        poissonDiskCase: static (state, value) => state.AdmitsPoisson
            ? PoissonDiskSelection(state.Candidates, value.Radius, value.Attempts, value.Seed, state.Key)
            : Fin.Fail<Selection>(state.Key.Unsupported(inputType: value.GetType(), outputType: typeof(Result))),
        farthestCase: static (state, value) => SelectionOf(value, state.Candidates, FarthestIndices(state.Candidates, value.Count.Value), state.Key),
        optimizeCase: static (state, value) => SelectionOf(value, state.Candidates, OptimizeFarthest(state.Candidates, value.Count.Value, value.Iterations.Value, state.Key), state.Key),
        lloydCase: static (state, value) => RelaxationSample(state.Candidates, value.Count.Value, value.Iterations.Value, None, state.Key)
            .Bind(relaxed => SelectionOf(value, state.Candidates, relaxed.Indices, state.Key)),
        capacityCase: static (state, value) => CapacityCvtSelection(state.Candidates, value.Count.Value, value.Limit.Value, value.Iterations.Value,
            state.Context.For(ToleranceLane.Convergence).Value, state.Key),
        scalarDensityCase: static (state, value) => BoundingMeasure(state.Candidates) switch {
            var bounds => DensitySelection(state.Candidates, value.Density, value.Count.Value,
                0.5 * MeanSpacing(bounds.Rank, bounds.Measure, value.Count.Value), state.Context, value.Seed, state.Key),
        },
        adaptiveCase: static (state, value) => DensitySelection(state.Candidates, value.Density, value.Count.Value, value.MinSpacing.Value, state.Context, value.Seed, state.Key),
        sampleEliminationCase: static (state, value) => SampleElimination(state.Candidates, value.Count.Value, value.Alpha.Value, value.Beta.Value, value.Gamma.Value, value.Seed, state.DomainMeasure, state.Key)
            .Bind(run => SelectionOf(state.Candidates, run.Indices, run.Algorithm, state.Key)),
        dworkVariableDensityCase: static (state, value) => DworkCandidateSelection(state.Candidates, value.Radius, value.Count.Value, value.MinRadius.Value, value.Attempts, value.Seed, state.Context, state.Key));
```

## Why

`SampleKind` and `VectorCloud` are Thinktecture `[Union]` owners, but the operational dispatch sites bypass their generated folds with raw type switches, `if` tests, and catch-all arms. That forfeits generated case binding and leaves the package capability unused at the exact owners it exists to serve.

## Change

Use generated `SwitchPartially` for each intentional subset: admitted-point cases at the top entry, the cluster-only cloud arm, mesh-only algorithms at the mesh entry, and candidate-suite cases at selection. Keep one named default per subset that routes to the next domain stage or emits `Unsupported`; remove raw `is`, raw `switch`, and duplicated unsupported arms. Thread candidate dispatch state as a named tuple, not a new type.

## Delta

Code-fence LOC: `-2`. Members: unchanged. Types: unchanged.

# 18. Construct the trusted CCVT preset without boundary re-admission

## From

`libs/dotnet/Rasm/.planning/Processing/sample.md:406`

```csharp
internal static Fin<PowerCcvtPolicy> Preset(Op key) =>
    from iterations in key.AcceptValidated<Dimension>(candidate: 16)
    from maxNewton in key.AcceptValidated<Dimension>(candidate: 32)
    from lloydSweeps in key.AcceptValidated<Dimension>(candidate: 1)
    from gradientSteps in key.AcceptValidated<Dimension>(candidate: 8)
    from lloydPosTol in key.AcceptValidated<PositiveMagnitude>(candidate: 0.01)
    from gradPosTol in key.AcceptValidated<PositiveMagnitude>(candidate: 0.1)
    from backtrack in key.AcceptValidated<PositiveMagnitude>(candidate: 0.5)
    from initialStep in key.AcceptValidated<PositiveMagnitude>(candidate: 1.0)
    from maxHalvings in key.AcceptValidated<Dimension>(candidate: 32)
    from aliasScale in key.AcceptValidated<PositiveMagnitude>(candidate: 0.65)
    from jitterVariance in key.AcceptValidated<PositiveMagnitude>(candidate: 0.05)
    from magnitudeScale in key.AcceptValidated<PositiveMagnitude>(candidate: 0.5)
    from relocateFraction in key.AcceptValidated<PositiveMagnitude>(candidate: 0.05)
```

## To

```csharp
internal static Fin<PowerCcvtPolicy> Preset(Op key) => new PowerCcvtPolicy(
    Iterations: Dimension.Create(value: 16), Density: None, MaxNewton: Dimension.Create(value: 32),
    LloydSweeps: Dimension.Create(value: 1), GradientSteps: Dimension.Create(value: 8),
    LloydTolerance: PositiveMagnitude.Create(value: 0.01), GradientTolerance: PositiveMagnitude.Create(value: 0.1),
    Backtrack: PositiveMagnitude.Create(value: 0.5), InitialStep: PositiveMagnitude.Create(value: 1.0), MaxHalvings: Dimension.Create(value: 32),
    AliasScale: PositiveMagnitude.Create(value: 0.65), JitterScale: PositiveMagnitude.Create(value: 0.025),
    RelocateFraction: PositiveMagnitude.Create(value: 0.05), PinnedSite: None, Seed: 0).Admit(key);
```

## Why

The preset literals are trusted declaration data, not foreign input. Sending thirteen fixed constants through `Op.AcceptValidated` rebuilds the same `Fin` rail on every call even though Thinktecture already owns their construction and the policy's only cross-field clauses remain on `Admit`.

## Change

Construct the fixed `Dimension` and `PositiveMagnitude` values with their generated Thinktecture `Create` factories, construct the flattened policy once per call, and retain the single policy-level `Admit` for `Backtrack < 1` and `RelocateFraction <= 1`. Keep caller overrides on the existing fallible `policy.Admit` path.

## Delta

Code-fence LOC: `-12`. Validation steps: `-13`. Module-level symbols and members: unchanged.
