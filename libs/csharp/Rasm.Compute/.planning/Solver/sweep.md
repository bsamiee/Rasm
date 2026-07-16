# [COMPUTE_SWEEP]

Rasm.Compute solver sweep: one `SweepGrid` design-of-experiments orchestration emitting a queryable `ParetoFront` with a binned global-sensitivity `SensitivityTornado`, beside one `FrameBudget` early-stop governor returning a coarse iterative field within a frame deadline and forking the refinement onto a background lane. Grid construction is the product of two orthogonal axes — a `SweepAxis` `[Union]` per-dimension factor and a `DoeDesign` `[SmartEnum<string>]` whole-grid strategy owning the design matrix — where factorial rows Cartesian-product per-axis levels while space-filling and response-surface rows draw a JOINT design across all dimensions, so a Latin-hypercube or Sobol sweep is one space-filling net, never the per-axis-1-D-then-Cartesian mis-model that defeats the variance reduction it exists to provide.

Per-point evaluation stays contract-uniform with the `Solver/optimizer#OPTIMIZER_LANE` and `Solver/uncertainty#UNCERTAINTY_LANE` lanes over one `DesignPoint`→objective-vector oracle, `IO`-lifted here alone (`IO<Fin<Seq<double>>>`) because the live `ProgressCell` observation and the `FrameBudget` refinement fork compose in `IO` where those synchronous lanes take the bare `Fin<Seq<double>>`. Space-filling rows draw the `Tensor/sampling#OWNED_BUILDS` `LowDiscrepancy` joint d-dimensional sampler under the `Scramble` policy, the sensitivity reductions ride `TensorPrimitives` SIMD folds, and the `ParetoFront` is the optimizer's own artifact crossing to Persistence content-keyed. `ComputeReceipt`, `WorkLane`, `CorrelationId`, `ClockPolicy`, and the `ComparerAccessors.StringOrdinal` accessor from `Solver/discretization#DISCRETIZATION_MESH` arrive settled.

## [01]-[INDEX]

- [01]-[SWEEP_AND_BUDGET]: N-dim DOE design-matrix strategy; binned global sensitivity; frame-budgeted early-stop; rolled-up per-point progress aggregate.

## [02]-[SWEEP_AND_BUDGET]

- Owner: `SweepAxis` `[Union]` per-dimension factor cases (factorial level set + unit-cube lowering); `DoeDesign` `[SmartEnum<string>]` whole-grid strategy rows (space-filling/response-surface discriminants); `SensitivityMethod` `[SmartEnum<string>]` global-sensitivity rows; `DoePolicy` the sample/bin/center/axial/fraction/scramble policy, `SensitivityObjective` selecting which objective column of a multi-objective grid the tornado ranks; `SweepGrid` the axes+objectives+sensitivity+strategy record; `FrameBudget` the early-stop governor over an iterative solve's per-iteration residual; `SensitivityTornado` the quantile-binned per-axis effect ranking; `SweepResult` the front+tornado+counts carrier; `SweepLane` the fan-out fold reducing per-point objective vectors to a `ParetoFront`+tornado and rolling one coarse `ProgressCell` per point through `ProgressCell.Aggregate`.
- Cases: `SweepAxis` `Linear` · `Logarithmic` · `Enumerated`; `DoeDesign` full-factorial · fractional-factorial · plackett-burman · latin-hypercube · sobol · halton · central-composite · box-behnken (central-composite/box-behnken the two `responseSurface` rows on coded ±1/±α/0 grids, latin-hypercube/sobol/halton the three `spaceFilling` JOINT designs); `SensitivityMethod` one-at-a-time · morris-elementary · sobol-variance.
- Entry: `public static (ProgressCell Progress, IO<Fin<SweepResult>> Result) Run(SweepGrid grid, Func<DesignPoint, IO<Fin<Seq<double>>>> evaluate, CorrelationId correlation, CancelScope scope, ClockPolicy clocks)` — the eager `Progress` parent cell observes live through the `Runtime/progress#OBSERVATION_SEAMS` `Observe`/`Stream` seams while `Result` carries the per-point effect; an empty axis set is the only `Fin.Fail`, and an individual point's `Fin.Fail` tallies as an incomplete point rather than aborting. `public static Func<DesignPoint, IO<Fin<Seq<double>>>> Governed(FrameBudget budget, Func<DesignPoint, int, IO<Fin<IterativeField>>> step, Func<DesignPoint, WorkLane, IO<Unit>> refine, ClockPolicy clocks)` wraps an iterative `step` so a frame-deadline expiry returns the coarse best-so-far field and FORKS the refinement onto `budget.Refinement`, plugging into the `evaluate` slot of `Run`.
- Auto: `SweepGrid.Design` dispatches the design matrix on the `DoeDesign` row (factorial rows Cartesian-product per-axis `Levels`, space-filling rows draw one JOINT `LowDiscrepancy` net, response-surface rows build the coded corners+star+center grid); `Run` fans the design applicatively over the independent `evaluate` oracle, folds succeeded objective vectors into the `ParetoFront`, tallies failed points, and projects the swept points onto the `SensitivityTornado`; `Governed` iterates `step` until convergence, fault, or `FrameBudget.Expired`, returning the best-so-far field within the frame and forking the refinement continuation onto `budget.Refinement`; `Run` mints one coarse `ProgressCell` per point (running before its `evaluate`, completed when the effect settles — a tallied `Fin.Fail` is progress-complete) and rolls the set through `ProgressCell.Aggregate(..., SubscriptionPolicy.Wire)`.
- Receipt: `Sweep(long GridPoints, int Completed, int OnFront, int Dominated)` from `Runtime/receipts#RECEIPT_UNION`; `SweepLane.Receipt` projects a `SweepResult` under the correlation — `OnFront` the front size, dominated `Completed − OnFront`, failed `GridPoints − Completed`; the frame-budget early-stop's per-iteration residual rides the iterative solve's own `Solve` receipt (`Solver/contract#SOLVE_CONTRACT`), never a fabricated sweep flag.
- Packages: System.Numerics.Tensors, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, Rasm.AppHost (project), Rasm.Persistence (project), BCL inbox
- Growth: a new design-of-experiments strategy is one `DoeDesign` row plus its `Materialize`/`Cardinality` arm; a new factor kind is one `SweepAxis` case carrying its `Levels`+`Map` lowering; a new sensitivity analysis is one `SensitivityMethod` row carrying its `Rank` fold; a frame-budget change is one field on `FrameBudget`/`DoePolicy`; zero new surface — a `FactorialSweep`/`LatinHypercubeSweep`/`SobolSweep`/`ResponseSurface` sibling collapses onto the one `DoeDesign` axis, and a per-axis `SweepAxis.LatinHypercube`/`SweepAxis.Sobol` case is rejected because a space-filling design is joint across dimensions, never a per-axis 1-D sequence Cartesian-producted.
- Boundary: the `evaluate` oracle is the single coupling point (this lane's `IO`-lift of the same contract the synchronous optimizer/uncertainty lanes take bare), so a parallel DOE-search path is rejected; space-filling rows draw the `Tensor/sampling#OWNED_BUILDS` `LowDiscrepancy` JOINT d-dimensional net rather than a per-axis 1-D sequence Cartesian-producted, because the per-axis form inflates the point count while destroying the joint low-discrepancy QMC convergence depends on, and the owned generator under the `Scramble` policy keeps a sweep replayable from `DoePolicy.Seed`; DOE-level fan-out backpressure is the `Runtime/scheduling#JOB_GRAPH` composition — one `JobNode` per `DesignPoint` on the `WorkLane.Bulk` row rides the shared lane drain, never a parallel job queue nor a fabricated per-point `AdmittedIntent`; a single point's `Fin.Fail` is tallied incomplete and the sweep continues, so a ten-thousand-point DOE survives a degenerate point; the `SensitivityTornado` is the `SensitivityMethod` axis over quantile-binned strata — the binning is load-bearing because a space-filling design has a unique coordinate per point, so raw-coordinate grouping yields one singleton bin per point and a degenerate unit index — and its variance fold is the one `Solver/uncertainty#UNCERTAINTY_LANE` composes through `SensitivityTornado.Of`, one decomposition; the frame-budgeted progressive solve reads the `Solver/contract#SOLVE_CONTRACT` iterative receipts and forks the refinement onto `budget.Refinement` (the `IO.Fork` non-blocking continuation), never an inline `await` that blocks the frame; live progress rolls one coarse `ProgressCell` per `DesignPoint` through `ProgressCell.Aggregate` into one parent mark observed through the `Runtime/progress#OBSERVATION_SEAMS` seams, so a second sweep-progress shape or a `Sweep`-receipt-polled estimate is rejected.

```csharp signature
// --- [TYPES] ----------------------------------------------------------------------------

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SweepAxis {
    private SweepAxis() { }

    public sealed record Linear(string Name, double Lower, double Upper, int Steps) : SweepAxis;
    public sealed record Logarithmic(string Name, double Lower, double Upper, int Steps) : SweepAxis;
    public sealed record Enumerated(string Name, Seq<double> Values) : SweepAxis;

    public string AxisName => Switch(linear: static a => a.Name, logarithmic: static a => a.Name, enumerated: static a => a.Name);

    public int LevelCount => Switch(linear: static a => Math.Max(1, a.Steps), logarithmic: static a => Math.Max(1, a.Steps), enumerated: static a => a.Values.Count);

    // The factorial level set for a full/fractional/Plackett-Burman design; a space-filling/RSM design ignores Levels and lowers through Map.
    public Seq<double> Levels =>
        Switch(
            linear: static a => toSeq(Enumerable.Range(0, Math.Max(1, a.Steps))).Map(i => a.Lower + (a.Upper - a.Lower) * i / Math.Max(1, a.Steps - 1)),
            logarithmic: static a => toSeq(Enumerable.Range(0, Math.Max(1, a.Steps))).Map(i => a.Lower * Math.Pow(a.Upper / a.Lower, (double)i / Math.Max(1, a.Steps - 1))),
            enumerated: static a => a.Values);

    // Lower a unit-cube coordinate u∈[0,1] onto the physical range — the mapping every space-filling and RSM draw rides; Logarithmic stays geometric.
    public double Map(double unit) =>
        Switch(
            state: Math.Clamp(unit, 0.0, 1.0),
            linear: static (u, a) => a.Lower + (a.Upper - a.Lower) * u,
            logarithmic: static (u, a) => a.Lower * Math.Pow(a.Upper / a.Lower, u),
            enumerated: static (u, a) => a.Values.IsEmpty ? u : a.Values[Math.Min(a.Values.Count - 1, (int)Math.Round(u * (a.Values.Count - 1)))]);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class DoeDesign {
    public static readonly DoeDesign FullFactorial = new("full-factorial", spaceFilling: false, responseSurface: false);
    public static readonly DoeDesign FractionalFactorial = new("fractional-factorial", spaceFilling: false, responseSurface: false);
    public static readonly DoeDesign PlackettBurman = new("plackett-burman", spaceFilling: false, responseSurface: false);
    public static readonly DoeDesign LatinHypercube = new("latin-hypercube", spaceFilling: true, responseSurface: false);
    public static readonly DoeDesign Sobol = new("sobol", spaceFilling: true, responseSurface: false);
    public static readonly DoeDesign Halton = new("halton", spaceFilling: true, responseSurface: false);
    public static readonly DoeDesign CentralComposite = new("central-composite", spaceFilling: false, responseSurface: true);
    public static readonly DoeDesign BoxBehnken = new("box-behnken", spaceFilling: false, responseSurface: true);

    public bool SpaceFilling { get; }
    public bool ResponseSurface { get; }

    // One total Switch: factorial rows Cartesian-product per-axis levels, space-filling rows draw a JOINT net, RSM rows build the coded ±1/±α/0 grid.
    public Seq<ImmutableArray<double>> Materialize(Seq<SweepAxis> axes, DoePolicy policy) =>
        Switch(
            state: (Axes: axes, Policy: policy),
            fullFactorial: static s => Factorial(s.Axes),
            fractionalFactorial: static s => Fractional(s.Axes, s.Policy.FractionExponent),
            plackettBurman: static s => PlackettBurmanMatrix(s.Axes),
            latinHypercube: static s => LatinHypercubeMatrix(s.Axes, s.Policy),
            sobol: static s => LowDiscrepancyMatrix(s.Axes, s.Policy, quasiSobol: true),
            halton: static s => LowDiscrepancyMatrix(s.Axes, s.Policy, quasiSobol: false),
            centralComposite: static s => CentralCompositeMatrix(s.Axes, s.Policy),
            boxBehnken: static s => BoxBehnkenMatrix(s.Axes, s.Policy));

    public long Cardinality(Seq<SweepAxis> axes, DoePolicy policy) =>
        Switch(
            state: (Axes: axes, Policy: policy),
            fullFactorial: static s => s.Axes.Fold(1L, static (acc, a) => acc * Math.Max(1, a.LevelCount)),
            fractionalFactorial: static s => 1L << Math.Max(0, s.Axes.Count - Math.Clamp(s.Policy.FractionExponent, 0, Math.Max(0, s.Axes.Count - 1))),
            plackettBurman: static s => HadamardOrder(s.Axes.Count + 1),
            latinHypercube: static s => Math.Max(2, s.Policy.Samples),
            sobol: static s => Math.Max(2, s.Policy.Samples),
            halton: static s => Math.Max(2, s.Policy.Samples),
            centralComposite: static s => (1L << s.Axes.Count) + 2L * s.Axes.Count + Math.Max(1, s.Policy.CenterPoints),
            boxBehnken: static s => 2L * s.Axes.Count * Math.Max(0, s.Axes.Count - 1) + Math.Max(1, s.Policy.CenterPoints));

    static Seq<ImmutableArray<double>> Factorial(Seq<SweepAxis> axes) =>
        axes.Fold(Seq(ImmutableArray<double>.Empty), static (acc, axis) => acc.Bind(prefix => axis.Levels.Map(prefix.Add)));

    // 2^(k-p): the first (k-p) factors a full two-level basis, each added factor aliased to a DISTINCT high-resolution generator (highest-popcount first).
    static Seq<ImmutableArray<double>> Fractional(Seq<SweepAxis> axes, int exponent) {
        int k = axes.Count, p = Math.Clamp(exponent, 0, Math.Max(0, k - 1)), basis = k - p;
        int[] generators = [.. Enumerable.Range(0, 1 << Math.Max(0, basis)).Reverse().Where(static m => BitOperations.PopCount((uint)m) >= 2)];
        return TwoLevel(basis).Map(corner => {
            double[] coded = new double[k];
            for (int f = 0; f < basis; f++) { coded[f] = corner[f]; }
            for (int i = 0; i < p; i++) {
                int mask = i < generators.Length ? generators[i] : (1 << basis) - 1;
                double product = 1.0;
                for (int b = 0; b < basis; b++) { if (((mask >> b) & 1) != 0) { product *= corner[b]; } }
                coded[basis + i] = product;
            }
            return Coded(axes, coded);
        });
    }

    // Sylvester-Hadamard saturated two-level screening: column 0 (all +1) is the skipped intercept, so a resolution-III design screens up to runs-1 factors at runs = next power of two ≥ k+1.
    static Seq<ImmutableArray<double>> PlackettBurmanMatrix(Seq<SweepAxis> axes) {
        int k = axes.Count, runs = (int)HadamardOrder(k + 1);
        int[][] h = Hadamard(runs);
        return toSeq(Enumerable.Range(0, runs)).Map(r => (ImmutableArray<double>)[.. axes.Map((axis, f) => axis.Map((h[r][f + 1] + 1.0) * 0.5))]);
    }

    // JOINT Latin hypercube composed from the Tensor/sampling#OWNED_BUILDS LowDiscrepancy.LatinHypercube owner (one point per stratum per dimension, N points regardless of d), never re-deriving the stratification here.
    static Seq<ImmutableArray<double>> LatinHypercubeMatrix(Seq<SweepAxis> axes, DoePolicy policy) {
        int n = Math.Max(2, policy.Samples), d = axes.Count;
        return LowDiscrepancy.LatinHypercube(d, n, policy.Seed, policy.Scramble)
            .Map(unit => toSeq(Enumerable.Range(0, n)).Map(s => (ImmutableArray<double>)[.. axes.Map((axis, f) => axis.Map(unit[s][f]))]))
            .IfFail(Seq<ImmutableArray<double>>());
    }

    // JOINT Sobol/Halton net: one d-dimensional generator drawn N times, each coordinate lowered through its axis — QMC convergence holds only on the joint net, never per-axis.
    static Seq<ImmutableArray<double>> LowDiscrepancyMatrix(Seq<SweepAxis> axes, DoePolicy policy, bool quasiSobol) {
        int n = Math.Max(2, policy.Samples), d = axes.Count;
        Fin<LowDiscrepancy> generator = quasiSobol ? LowDiscrepancy.Sobol(d, policy.Seed, policy.Scramble) : LowDiscrepancy.Halton(d, policy.Seed, policy.Scramble);
        return generator
            .Map(g => Unit(g, n).Map(point => (ImmutableArray<double>)[.. axes.Map((axis, f) => axis.Map(point[Math.Min(f, point.Length - 1)]))]))
            .IfFail(Seq<ImmutableArray<double>>());
    }

    // Central composite: factorial corners + 2k axial star points at ±α + center replicates, scaled by α so the star lands ON the box bounds while corners sit INSIDE at
    // ±1/α — a ROTATABLE CCD (α = (2^k)^(1/4) default), AxialAlpha a live knob; degenerates to face-centered only when α ≤ 1.
    static Seq<ImmutableArray<double>> CentralCompositeMatrix(Seq<SweepAxis> axes, DoePolicy policy) {
        int k = axes.Count;
        double alpha = double.IsNaN(policy.AxialAlpha) ? Math.Pow(1 << k, 0.25) : policy.AxialAlpha;
        double scale = Math.Max(1.0, alpha);
        Seq<ImmutableArray<double>> corners = TwoLevel(k).Map(corner => Coded(axes, corner, scale));
        Seq<ImmutableArray<double>> axial = toSeq(Enumerable.Range(0, k)).Bind(f => Seq(Coded(axes, AxisVector(k, f, -alpha), scale), Coded(axes, AxisVector(k, f, alpha), scale)));
        Seq<ImmutableArray<double>> center = toSeq(Enumerable.Range(0, Math.Max(1, policy.CenterPoints))).Map(_ => Coded(axes, new double[k], scale));
        return corners + axial + center;
    }

    // Box-Behnken: every factor pair at ±1 with the rest at center + center replicates, a three-level rotatable RSM avoiding the corners (center-only below three factors).
    static Seq<ImmutableArray<double>> BoxBehnkenMatrix(Seq<SweepAxis> axes, DoePolicy policy) {
        int k = axes.Count;
        Seq<ImmutableArray<double>> blocks = toSeq(Enumerable.Range(0, k)).Bind(i =>
            toSeq(Enumerable.Range(i + 1, Math.Max(0, k - i - 1))).Bind(j =>
                Seq((-1.0, -1.0), (-1.0, 1.0), (1.0, -1.0), (1.0, 1.0)).Map(pair => {
                    double[] coded = new double[k];
                    coded[i] = pair.Item1; coded[j] = pair.Item2;
                    return Coded(axes, coded);
                })));
        return blocks + toSeq(Enumerable.Range(0, Math.Max(1, policy.CenterPoints))).Map(_ => Coded(axes, new double[k]));
    }

    // Coded ±1/±α → physical: divide by `scale` (the CCD axial extent α; 1.0 for the ±1-only fractional/Box-Behnken grids) before the [-1,1] clamp and unit-cube lowering.
    static ImmutableArray<double> Coded(Seq<SweepAxis> axes, double[] coded, double scale = 1.0) =>
        [.. axes.Map((axis, f) => axis.Map(0.5 * (Math.Clamp((f < coded.Length ? coded[f] : 0.0) / scale, -1.0, 1.0) + 1.0)))];

    static Seq<double[]> TwoLevel(int n) =>
        toSeq(Enumerable.Range(0, 1 << Math.Max(0, n))).Map(mask => {
            double[] corner = new double[n];
            for (int b = 0; b < n; b++) { corner[b] = ((mask >> b) & 1) == 0 ? -1.0 : 1.0; }
            return corner;
        });

    static double[] AxisVector(int k, int axis, double value) { double[] v = new double[k]; if (axis < k) { v[axis] = value; } return v; }

    static Seq<double[]> Unit(LowDiscrepancy generator, int count) =>
        toSeq(Enumerable.Range(0, count)).Fold((Gen: generator, Points: Seq<double[]>()), static (acc, _) => {
            var (next, point) = acc.Gen.Draw();
            return (next, acc.Points.Add(point));
        }).Points;

    static long HadamardOrder(int minimum) { long n = 1L; while (n < minimum) { n <<= 1; } return n; }

    static int[][] Hadamard(int n) {
        int[][] h = [[1]];
        for (int size = 1; size < n; size <<= 1) {
            int[][] next = new int[size << 1][];
            for (int i = 0; i < (size << 1); i++) { next[i] = new int[size << 1]; }
            for (int i = 0; i < size; i++) {
                for (int j = 0; j < size; j++) {
                    int v = h[i][j];
                    next[i][j] = v; next[i][j + size] = v; next[i + size][j] = v; next[i + size][j + size] = -v;
                }
            }
            h = next;
        }
        return h;
    }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SensitivityMethod {
    public static readonly SensitivityMethod OneAtATime = new("one-at-a-time");
    public static readonly SensitivityMethod MorrisElementary = new("morris-elementary");
    public static readonly SensitivityMethod SobolVariance = new("sobol-variance");

    // Rank a per-axis conditional-expectation vector E[Y|Xᵢ∈binₖ] into a tornado bar (low, high, effect): OAT = binned-level output range,
    // Sobol = first-order Vᵢ/V between-bin over global variance, Morris = POST-HOC μ*/σ screen of the conditional-mean gradient on already-swept
    // data — the controlled-Δ trajectory Morris row is Solver/uncertainty#UNCERTAINTY_LANE (MorrisTrajectories/MorrisScreening), never re-spelled here.
    public (double Low, double High, double Effect) Rank(Seq<double> perBin, double globalVariance) =>
        Switch(
            state: (Bins: perBin, Variance: globalVariance),
            oneAtATime: static s => s.Bins.IsEmpty ? (0.0, 0.0, 0.0) : (s.Bins.Min(), s.Bins.Max(), Math.Abs(s.Bins.Max() - s.Bins.Min())),
            morrisElementary: static s => Elementary(s.Bins),
            sobolVariance: static s => (s.Bins.IsEmpty ? 0.0 : s.Bins.Min(), s.Bins.IsEmpty ? 0.0 : s.Bins.Max(), s.Variance > 1e-12 ? BinVariance(s.Bins) / s.Variance : 0.0));

    static (double Low, double High, double Effect) Elementary(Seq<double> bins) {
        if (bins.Count < 2) { return (0.0, 0.0, 0.0); }
        double[] effects = [.. toSeq(Enumerable.Range(1, bins.Count - 1)).Map(i => Math.Abs(bins[i] - bins[i - 1]))];
        double muStar = TensorPrimitives.Average<double>(effects), sigma = TensorPrimitives.StdDev<double>(effects);
        return (muStar - sigma, muStar + sigma, muStar);
    }

    static double BinVariance(Seq<double> bins) => bins.IsEmpty ? 0.0 : TensorPrimitives.StdDev<double>([.. bins]) is var sd ? sd * sd : 0.0;
}

// --- [MODELS] ---------------------------------------------------------------------------

public sealed record DoePolicy(int Samples, int SensitivityBins, int CenterPoints, double AxialAlpha, int FractionExponent, Scramble Scramble, int Seed, int SensitivityObjective) {
    public static readonly DoePolicy Default = new(Samples: 256, SensitivityBins: 8, CenterPoints: 1, AxialAlpha: double.NaN, FractionExponent: 1, Scramble: Scramble.DigitalShift, Seed: 0x5DEECE66, SensitivityObjective: 0);
    public static readonly DoePolicy SpaceFillingLarge = Default with { Samples = 4096, SensitivityBins = 16 };
}

public readonly record struct IterativeField(Seq<double> Field, double Residual, bool Done);

public sealed record FrameBudget(Duration Deadline, int MinIterations, int MaxIterations, WorkLane Refinement) {
    public static readonly FrameBudget Interactive = new(Duration.FromMilliseconds(16), MinIterations: 8, MaxIterations: 4096, WorkLane.Background);

    public bool Expired(Instant start, Instant now, int iteration) =>
        iteration >= MaxIterations || (iteration >= MinIterations && now - start >= Deadline);
}

public sealed record SweepGrid(Seq<SweepAxis> Axes, Seq<ObjectiveSense> Objectives, SensitivityMethod Sensitivity) {
    public DoeDesign Strategy { get; init; } = DoeDesign.FullFactorial;
    public DoePolicy Policy { get; init; } = DoePolicy.Default;

    public Seq<ImmutableArray<double>> Design => Strategy.Materialize(Axes, Policy);
    public long Cardinality => Strategy.Cardinality(Axes, Policy);
    public ImmutableArray<double> Senses => [.. Objectives.Map(static o => o.Sign)];
}

public sealed record SensitivityTornado(Seq<(string Axis, double Low, double High, double Effect)> Bars) {
    public static SensitivityTornado Of(SweepGrid grid, Seq<DesignPoint> results, int objective) {
        if (results.IsEmpty) { return new(grid.Axes.Map(static a => (a.AxisName, 0.0, 0.0, 0.0))); }
        double[] response = [.. results.Map(p => objective < p.Objectives.Length ? p.Objectives[objective] : 0.0)];
        double globalVariance = TensorPrimitives.StdDev<double>(response) is var sd ? sd * sd : 0.0;
        int bins = Math.Max(2, grid.Policy.SensitivityBins);
        return new(grid.Axes.Map((axis, index) => {
            var (low, high, effect) = grid.Sensitivity.Rank(ConditionalMeans(results, index, objective, bins), globalVariance);
            return (axis.AxisName, low, high, effect);
        }).OrderByDescending(static bar => bar.Item4).ToSeq());
    }

    // Equal-count quantile binning of axis `index` so a space-filling design (every coordinate unique) yields a real E[Y|Xᵢ∈binₖ] vector, never singletons.
    static Seq<double> ConditionalMeans(Seq<DesignPoint> results, int index, int objective, int bins) {
        (double X, double Y)[] ordered = [.. results
            .Map(p => (X: index < p.Coordinates.Length ? p.Coordinates[index] : 0.0, Y: objective < p.Objectives.Length ? p.Objectives[objective] : 0.0))
            .OrderBy(static row => row.X)];
        int n = ordered.Length, width = Math.Max(1, n / bins);
        return toSeq(Enumerable.Range(0, Math.Min(bins, n))).Map(b => {
            int lo = b * width, hi = b == bins - 1 ? n : Math.Min(n, lo + width);
            double sum = 0.0; int count = 0;
            for (int i = lo; i < hi; i++) { sum += ordered[i].Y; count++; }
            return count > 0 ? sum / count : 0.0;
        });
    }
}

public sealed record SweepResult(SweepGrid Grid, ParetoFront Front, SensitivityTornado Tornado, int Completed, int Failed, Instant At);

// --- [OPERATIONS] -----------------------------------------------------------------------

public static class SweepLane {
    public static (ProgressCell Progress, IO<Fin<SweepResult>> Result) Run(SweepGrid grid, Func<DesignPoint, IO<Fin<Seq<double>>>> evaluate, CorrelationId correlation, CancelScope scope, ClockPolicy clocks) {
        if (grid.Axes.IsEmpty) {
            var empty = new ProgressCell(correlation, scope, clocks);
            ignore(empty.Advance(ProgressPhase.Faulted));
            return (empty, IO.pure(Fin.Fail<SweepResult>(ComputeFault.Create("<sweep-empty-axis-set>"))));
        }
        Seq<ImmutableArray<double>> design = grid.Design;
        Seq<ProgressCell> cells = design.Map(_ => new ProgressCell(correlation, scope, clocks));
        var (parent, wiring) = ProgressCell.Aggregate(correlation, scope, clocks, cells, SubscriptionPolicy.Wire);
        return (parent, IO.pure(wiring).Bracket(
            Use: _ => design.Zip(cells, static (coords, cell) => (Coords: coords, Cell: cell))
                .Traverse(pair =>
                    from _started in IO.lift(() => ignore(pair.Cell.Advance(ProgressPhase.Running)))
                    from result in evaluate(new DesignPoint(pair.Coords, [], []))
                    from _settled in IO.lift(() => ignore(pair.Cell.Advance(ProgressPhase.Completed)))
                    select (Coords: pair.Coords, Result: result))
                .Map(rows => Fin.Succ(Reduce(grid, rows, clocks)))
                .As(),
            Fin: static w => IO.lift(fun(w.Dispose))));
    }

    public static ComputeReceipt.Sweep Receipt(SweepResult result, CorrelationId correlation, Duration elapsed) =>
        new(result.Grid.Cardinality, result.Completed, result.Front.Points.Count, Math.Max(0, result.Completed - result.Front.Points.Count)) {
            Correlation = correlation, Lane = WorkLane.Background, Substrate = Substrate.CpuTensor, AllocationClass = AllocationClass.PooledMemory, Elapsed = elapsed,
        };

    // Fan-out reduction: a succeeded point enters the ParetoFront and swept set, a failed point increments the tally rather than aborting.
    static SweepResult Reduce(SweepGrid grid, Seq<(ImmutableArray<double> Coords, Fin<Seq<double>> Result)> rows, ClockPolicy clocks) {
        var folded = rows.Fold(
            (Front: new ParetoFront(Seq<DesignPoint>(), grid.Senses), Points: Seq<DesignPoint>(), Failed: 0),
            static (acc, row) => row.Result.Match(
                Succ: objectives => { var point = new DesignPoint(row.Coords, [.. objectives], []); return (acc.Front.Insert(point), acc.Points.Add(point), acc.Failed); },
                Fail: static _ => (acc.Front, acc.Points, acc.Failed + 1)));
        return new SweepResult(grid, folded.Front, SensitivityTornado.Of(grid, folded.Points, grid.Policy.SensitivityObjective), folded.Points.Count, folded.Failed, clocks.Now);
    }

    // Frame-budgeted early-stop over an iterative `step`: a deadline expiry returns the best-so-far coarse field WITHIN the frame and FORKS the
    // refinement onto budget.Refinement (IO.Fork non-blocking), never an inline await.
    public static Func<DesignPoint, IO<Fin<Seq<double>>>> Governed(
        FrameBudget budget,
        Func<DesignPoint, int, IO<Fin<IterativeField>>> step,
        Func<DesignPoint, WorkLane, IO<Unit>> refine,
        ClockPolicy clocks) =>
        point =>
            from outcome in Iterate(budget, step, point, clocks)
            from _ in outcome.Early ? refine(point, budget.Refinement).Fork().As().Map(static _ => unit) : IO.pure(unit)
            select outcome.Best.Map(static r => r.Field);

    static IO<(Fin<IterativeField> Best, bool Early)> Iterate(FrameBudget budget, Func<DesignPoint, int, IO<Fin<IterativeField>>> step, DesignPoint point, ClockPolicy clocks) =>
        IO.liftAsync(async env => {
            Instant start = clocks.Now;
            Fin<IterativeField> best = Fin.Fail<IterativeField>(ComputeFault.Create("<frame-budget-no-iteration>"));
            for (int iteration = 0; ; iteration++) {
                best = await step(point, iteration).RunAsync(env);
                if (best.Match(Succ: static r => r.Done, Fail: static _ => true)) { return (best, false); }
                if (budget.Expired(start, clocks.Now, iteration)) { return (best, true); }
            }
        });
}
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
