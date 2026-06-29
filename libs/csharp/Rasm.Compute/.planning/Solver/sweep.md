# [COMPUTE_SWEEP]

Rasm.Compute solver sweep: one `SweepGrid` design-of-experiments orchestration emitting a queryable `ParetoFront` artifact with a binned global-sensitivity `SensitivityTornado`, and one `FrameBudget` early-stop governor returning a coarse iterative field within a frame deadline and forking the refinement onto a background lane. The grid is the product of two orthogonal axes: a `SweepAxis` `[Union]` per-dimension factor and a `DoeDesign` `[SmartEnum<string>]` whole-grid strategy that owns the design-matrix construction — factorial rows Cartesian-product the per-axis levels while space-filling and response-surface rows draw a JOINT design across all dimensions, so a Latin-hypercube or Sobol sweep is one space-filling net rather than the per-axis-1-D-then-Cartesian mis-model that defeats the variance-reduction it exists to provide. The page owns the `SweepAxis` factor cases, the `DoeDesign` design rows, the `SensitivityMethod` `[SmartEnum<string>]` global-sensitivity rows, the `DoePolicy`/`SweepGrid`/`FrameBudget`/`IterativeField`/`SensitivityTornado`/`SweepResult` carriers, and the `SweepLane` fan-out fold with its rolled-up per-point `ProgressCell` aggregate; the per-point evaluation composes the `Solver/optimizer#OPTIMIZER_LANE` `ParetoFront`/`DesignPoint`/`ObjectiveSense` artifact and the `Solver/contract#SOLVE_CONTRACT` iterative `Solve` residual receipt behind the same `DesignPoint`→objective-vector contract the `Optimizer` and `Uncertainty` lanes drive, `IO`-lifted on this lane (`IO<Fin<Seq<double>>>`) because the live `ProgressCell` observation and the `FrameBudget` refinement fork compose in `IO` where those two synchronous lanes take the bare `Fin<Seq<double>>`, the space-filling `DoeDesign` rows draw the `Tensor/sampling#OWNED_BUILDS` `LowDiscrepancy` joint d-dimensional sampler under the `Scramble` policy, the sensitivity reductions ride the `TensorPrimitives` SIMD folds, and the `ComputeReceipt` rail, `WorkLane`, `CorrelationId`, `ClockPolicy`, and the `ComparerAccessors.StringOrdinal` accessor from `Solver/discretization#DISCRETIZATION_MESH` arrive settled. The `ParetoFront` is the same artifact the optimizer owns and crosses to Persistence content-keyed.

## [01]-[INDEX]

- [01]-[SWEEP_AND_BUDGET]: N-dim DOE design-matrix strategy; binned global sensitivity; frame-budgeted early-stop; rolled-up per-point progress aggregate.

## [02]-[SWEEP_AND_BUDGET]

- Owner: `SweepAxis` `[Union]` per-dimension factor cases carrying the factorial level set and the unit-cube lowering; `DoeDesign` `[SmartEnum<string>]` whole-grid design-matrix strategy rows carrying the space-filling/response-surface discriminants; `SensitivityMethod` `[SmartEnum<string>]` global-sensitivity rows; `DoePolicy` the sample-count/bin/center/axial/fraction/scramble/sensitivity-objective policy (`SensitivityObjective` selecting which objective column of a multi-objective grid the tornado ranks); `SweepGrid` the axes+objectives+sensitivity+strategy record materializing the design matrix; `FrameBudget` the early-stop governor reading an iterative solve's per-iteration residual; `SensitivityTornado` the quantile-binned post-processing fold projecting the swept results onto a per-axis effect ranking; `SweepResult` the front+tornado+counts carrier; `SweepLane` the static fan-out fold reducing the per-point objective vectors to a `ParetoFront` plus tornado and folding one coarse per-point `ProgressCell` through `ProgressCell.Aggregate` into one rolled parent cell.
- Cases: `SweepAxis` cases `Linear(string Name, double Lower, double Upper, int Steps)` · `Logarithmic(string Name, double Lower, double Upper, int Steps)` · `Enumerated(string Name, Seq<double> Values)`; `DoeDesign` rows full-factorial · fractional-factorial · plackett-burman · latin-hypercube · sobol · halton · central-composite · box-behnken (the three latter `responseSurface` rows building coded ±1/±α/0 grids, the four `latin-hypercube`/`sobol`/`halton` rows `spaceFilling` JOINT designs); `SensitivityMethod` rows one-at-a-time · morris-elementary · sobol-variance.
- Entry: `public static (ProgressCell Progress, IO<Fin<SweepResult>> Result) Run(SweepGrid grid, Func<DesignPoint, IO<Fin<Seq<double>>>> evaluate, CorrelationId correlation, CancelScope scope, ClockPolicy clocks)` — the eager `Progress` parent cell is observed live through the `Runtime/progress#OBSERVATION_SEAMS` `Observe`/`Stream` seams while the `Result` `IO` carries the per-point evaluation effect; the evaluation entry stays contract-uniform with `Optimizer.Optimize`/`Uncertainty.Propagate` over the same `DesignPoint`→objective-vector contract (the sweep slot is the `IO`-lift `IO<Fin<Seq<double>>>`, the synchronous lanes the bare `Fin<Seq<double>>`; progress is composed around the oracle, not threaded through it, so the coupling point — a point's coordinates to its objective vector — is untouched), an empty axis set is the only `Fin.Fail`, and an individual point's `Fin.Fail` is tallied as an incomplete point rather than aborting the run; `public static Func<DesignPoint, IO<Fin<Seq<double>>>> Governed(FrameBudget budget, Func<DesignPoint, int, IO<Fin<IterativeField>>> step, Func<DesignPoint, WorkLane, IO<Unit>> refine, ClockPolicy clocks)` wraps an iterative `step` so a frame-deadline expiry returns the coarse best-so-far field and FORKS the refinement onto `budget.Refinement`, plugging into the `evaluate` slot of `Run`.
- Auto: `SweepGrid.Design` dispatches the design matrix on the `DoeDesign` row — `full-factorial` folds the Cartesian product of each axis's `Levels`, `fractional-factorial` aliases the added factors to distinct high-resolution interaction generators over the `2^(k-p)` basis, `plackett-burman` reads the Sylvester-Hadamard screening columns, `latin-hypercube` rank-stratifies a joint `LowDiscrepancy` draw into one point per stratum per dimension, `sobol`/`halton` map a joint d-dimensional low-discrepancy net through each axis's unit-cube lowering, and `central-composite`/`box-behnken` build the response-surface coded grids; `Run` fans the design out applicatively over the independent `evaluate` oracle (each point a `DesignPoint`), folds the succeeded objective vectors into the `ParetoFront`, tallies the failed points, and projects the swept points onto the `SensitivityTornado`; `Governed` iterates `step` until convergence, fault, or `FrameBudget.Expired`, returning the best-so-far field within the frame and forking the refinement continuation so the interactive caller renders the coarse field and the refined field arrives async on `budget.Refinement`; `Run` mints one coarse `ProgressCell` per design point, advances it to `running` before its `evaluate` and to `completed` when the point's effect settles — a tallied `Fin.Fail` point is progress-complete, never an aggregate fault, so a degenerate point never trips the whole sweep terminal — and folds the set through `ProgressCell.Aggregate(correlation, scope, clocks, cells, SubscriptionPolicy.Wire)` so the rolled parent mark's completed ratio rises with the evaluated fraction.
- Receipt: the `Sweep` `ComputeReceipt` case carries the grid-point cardinality, the completed count, and the on-front-versus-dominated split (`Sweep(long GridPoints, int Completed, int OnFront, int Dominated)` from `Runtime/receipts#RECEIPT_UNION`); `SweepLane.Receipt` projects a `SweepResult` onto it under the correlation, the front size is `OnFront`, the evaluated-but-dominated count is `Completed − OnFront`, and the failed points are `GridPoints − Completed`; the frame-budget early-stop's per-iteration residual at stop rides the iterative solve's own `Solve` receipt (`Solver/contract#SOLVE_CONTRACT`), not a fabricated sweep flag.
- Packages: System.Numerics.Tensors, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, Rasm.AppHost (project), Rasm.Persistence (project), BCL inbox
- Growth: a new experimental design is one `DoeDesign` row plus its `Materialize`/`Cardinality` arm; a new factor kind is one `SweepAxis` case carrying its `Levels` and `Map` lowering; a new sensitivity analysis is one `SensitivityMethod` row carrying its `Rank` fold; a frame-budget policy change is one field on `FrameBudget` or `DoePolicy`; zero new surface — a `FactorialSweep`/`LatinHypercubeSweep`/`SobolSweep`/`ResponseSurface` sibling family is collapsed onto the one `DoeDesign` strategy axis, and a per-axis space-filling case (the deleted `SweepAxis.LatinHypercube`/`SweepAxis.Sobol`) is the rejected form because a space-filling design is joint across dimensions, never a per-axis 1-D sequence Cartesian-producted.
- Boundary: the sweep is contract-uniform with the optimizer and the uncertainty lanes — the `evaluate` oracle is the single coupling point (the sweep lane's `IO`-lift of the same `DesignPoint`→objective-vector contract the synchronous optimizer/uncertainty lanes take bare), so the fan-out never knows whether a point ran a full FEA solve or a `Surrogate.Predict`, and a parallel DOE-search path is the rejected form; the design-matrix construction is the `DoeDesign` discriminant, never a parallel sweep type — the factorial rows Cartesian-product per-axis levels while the space-filling rows draw the `Tensor/sampling#OWNED_BUILDS` `LowDiscrepancy.Sobol`/`Halton` JOINT d-dimensional net (a fresh per-axis 1-D Sobol sequence Cartesian-producted is the deleted form because it inflates the point count combinatorially while destroying the joint low-discrepancy that QMC convergence depends on), and the response-surface rows build the coded ±1 factorial corners, ±α star, and center replicates a quadratic RSM fit demands; DOE-level fan-out backpressure is the `Runtime/scheduling#JOB_GRAPH` `JobGraph` composition — an app that needs bounded admission submits the design as one `JobNode` per `DesignPoint` on the `WorkLane.Bulk` row so the N-dim DOE rides the same lane backpressure and drain, never a parallel job queue and never a fabricated per-point `AdmittedIntent` minted inside this fold; a single point's `Fin.Fail` (a singular operator, a non-converged solve) is tallied as an incomplete point and the sweep continues, so a ten-thousand-point DOE survives a degenerate point rather than aborting the whole campaign; the sensitivity tornado is the `SensitivityMethod` axis over the swept results binned into quantile strata per axis — the one-at-a-time row folds the swept objective range, the Morris row computes the μ* mean-absolute elementary effect with its σ interaction band through the owned `TensorPrimitives.Average`/`StdDev` reductions, and the Sobol row computes the first-order main-effect index as the between-bin variance over the global variance (`V[E(Y|Xᵢ)]/V`) — and the quantile binning is load-bearing because a space-filling design has a unique coordinate per point, so grouping by the raw coordinate yields one singleton bin per point and a degenerate unit Sobol index for every axis, the deleted form; the `SensitivityTornado` variance fold is the same one the `Solver/uncertainty#UNCERTAINTY_LANE` total-effect ranking composes through `SensitivityTornado.Of`, so the sweep tornado and the UQ total-effect stay one decomposition; the `Sobol`/`Halton` and `latin-hypercube` designs draw the owned `LowDiscrepancy` sampler under the `Scramble` policy rather than a fresh `System.Random`, so a sweep is replayable from its `DoePolicy.Seed`; the frame-budgeted progressive solve reads the `Solver/contract#SOLVE_CONTRACT` iterative receipts the solve lane already emits and forks the refinement onto `budget.Refinement` (the `IO.Fork` non-blocking continuation), so the UI thread gets a within-frame coarse field and the refinement is a scheduled fiber, never an inline `await` that blocks the frame; the live sweep progress composes at the `Run` frontier — one coarse `ProgressCell` per `DesignPoint` rolled through `ProgressCell.Aggregate` into one parent cell whose `PhaseSubscription` wiring disposes on the bracketed completion — so a dashboard observes one composite progress bar for the whole DOE through the identical `Runtime/progress#OBSERVATION_SEAMS` `Observe`/`Stream` seams a single intent rides, the per-point fail stays a `SweepResult.Failed` tally rather than an aggregate fault (a degenerate point never reports the campaign faulted), and a second sweep-progress shape or a `Sweep`-receipt-polled progress estimate are the rejected forms because the rolled mark is a `ProgressMark`.

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

    // The factorial level set this axis contributes to a full/fractional/Plackett-Burman design — the discrete
    // grid the Cartesian product folds; a space-filling/RSM design ignores Levels and lowers through Map.
    public Seq<double> Levels =>
        Switch(
            linear: static a => toSeq(Enumerable.Range(0, Math.Max(1, a.Steps))).Map(i => a.Lower + (a.Upper - a.Lower) * i / Math.Max(1, a.Steps - 1)),
            logarithmic: static a => toSeq(Enumerable.Range(0, Math.Max(1, a.Steps))).Map(i => a.Lower * Math.Pow(a.Upper / a.Lower, (double)i / Math.Max(1, a.Steps - 1))),
            enumerated: static a => a.Values);

    // Lower a unit-cube coordinate u∈[0,1] onto this axis's physical range — the one mapping every space-filling
    // and response-surface draw rides; Logarithmic stays geometric so a log sweep is equispaced in log-space.
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

    // One total Switch builds the design matrix: factorial rows Cartesian-product per-axis levels, space-filling
    // rows draw a JOINT d-dimensional net, response-surface rows build the coded ±1/±α/0 grid.
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
        axes.Fold(Seq1(ImmutableArray<double>.Empty), static (acc, axis) => acc.Bind(prefix => axis.Levels.Map(prefix.Add)));

    // 2^(k-p): the first (k-p) factors are a full two-level basis, each added factor aliased to a DISTINCT
    // high-resolution interaction generator (highest-popcount columns first) so the fraction stays high-res.
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

    // Sylvester-Hadamard saturated two-level screening: column 0 (all +1) is the intercept and is skipped, so a
    // resolution-III main-effects design screens up to runs-1 factors at runs = next power of two ≥ k+1.
    static Seq<ImmutableArray<double>> PlackettBurmanMatrix(Seq<SweepAxis> axes) {
        int k = axes.Count, runs = (int)HadamardOrder(k + 1);
        int[][] h = Hadamard(runs);
        return toSeq(Enumerable.Range(0, runs)).Map(r => (ImmutableArray<double>)[.. axes.Map((axis, f) => axis.Map((h[r][f + 1] + 1.0) * 0.5))]);
    }

    // JOINT Latin hypercube: the rank-stratified d-dimensional QMC design the Tensor/sampling#OWNED_BUILDS
    // LowDiscrepancy.LatinHypercube owner builds — one point per stratum per dimension, N points regardless of
    // d, the QMC draw the in-stratum offset — composed, never re-deriving the stratification here.
    static Seq<ImmutableArray<double>> LatinHypercubeMatrix(Seq<SweepAxis> axes, DoePolicy policy) {
        int n = Math.Max(2, policy.Samples), d = axes.Count;
        return LowDiscrepancy.LatinHypercube(d, n, policy.Seed, policy.Scramble)
            .Map(unit => toSeq(Enumerable.Range(0, n)).Map(s => (ImmutableArray<double>)[.. axes.Map((axis, f) => axis.Map(unit[s][f]))]))
            .IfFail(Seq<ImmutableArray<double>>());
    }

    // JOINT Sobol/Halton net: one d-dimensional low-discrepancy generator drawn N times, each unit coordinate
    // lowered through its axis — the quasi-Monte-Carlo convergence holds only on the joint net, never per-axis.
    static Seq<ImmutableArray<double>> LowDiscrepancyMatrix(Seq<SweepAxis> axes, DoePolicy policy, bool quasiSobol) {
        int n = Math.Max(2, policy.Samples), d = axes.Count;
        Fin<LowDiscrepancy> generator = quasiSobol ? LowDiscrepancy.Sobol(d, policy.Seed, policy.Scramble) : LowDiscrepancy.Halton(d, policy.Seed, policy.Scramble);
        return generator
            .Map(g => Unit(g, n).Map(point => (ImmutableArray<double>)[.. axes.Map((axis, f) => axis.Map(point[Math.Min(f, point.Length - 1)]))]))
            .IfFail(Seq<ImmutableArray<double>>());
    }

    // Central composite: factorial corners + 2k axial (star) points at ±α + center replicates. The coded grid
    // scales by α so the axial star lands ON the factor-box bounds while the factorial corners sit INSIDE at
    // ±1/α — a genuine ROTATABLE CCD (α = (2^k)^(1/4) by default), so AxialAlpha is a live knob that moves the
    // corners; it degenerates to a face-centered CCD only when α ≤ 1.
    static Seq<ImmutableArray<double>> CentralCompositeMatrix(Seq<SweepAxis> axes, DoePolicy policy) {
        int k = axes.Count;
        double alpha = double.IsNaN(policy.AxialAlpha) ? Math.Pow(1 << k, 0.25) : policy.AxialAlpha;
        double scale = Math.Max(1.0, alpha);
        Seq<ImmutableArray<double>> corners = TwoLevel(k).Map(corner => Coded(axes, corner, scale));
        Seq<ImmutableArray<double>> axial = toSeq(Enumerable.Range(0, k)).Bind(f => Seq(Coded(axes, AxisVector(k, f, -alpha), scale), Coded(axes, AxisVector(k, f, alpha), scale)));
        Seq<ImmutableArray<double>> center = toSeq(Enumerable.Range(0, Math.Max(1, policy.CenterPoints))).Map(_ => Coded(axes, new double[k], scale));
        return corners + axial + center;
    }

    // Box-Behnken: every factor pair at ±1 with the rest at center + center replicates, a three-level rotatable
    // RSM that avoids the factorial corners (degenerates to center-only below three factors).
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

    // Coded ±1/±α → physical: a coded value divides by `scale` (the CCD axial extent α; 1.0 for the ±1-only
    // fractional and Box-Behnken grids) before the [-1,1] clamp and the unit-cube lowering, so a rotatable CCD's
    // axial star reaches the box bounds while its factorial corners fall inside at ±1/α.
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

    // Rank a per-axis conditional-expectation vector E[Y|Xᵢ∈binₖ] into a tornado bar (low, high, effect):
    // OAT = output range across the binned levels, Sobol = first-order index Vᵢ/V = between-bin variance over
    // the global variance, Morris = a POST-HOC screen (μ*/σ of the consecutive-bin gradient of the conditional-
    // mean curve) usable on already-swept data — the controlled-Δ trajectory Morris elementary effect is the
    // Solver/uncertainty#UNCERTAINTY_LANE morris row (MorrisTrajectories/MorrisScreening), never re-spelled here.
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

    // Equal-count quantile binning of axis `index` so a space-filling design (every coordinate unique) yields a
    // real E[Y|Xᵢ∈binₖ] conditional-expectation vector, never one singleton group per point.
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

    // The fan-out reduction: a succeeded point enters the ParetoFront and the swept-points set, a failed point
    // increments the failed tally rather than aborting — the DOE is robust to a degenerate point.
    static SweepResult Reduce(SweepGrid grid, Seq<(ImmutableArray<double> Coords, Fin<Seq<double>> Result)> rows, ClockPolicy clocks) {
        var folded = rows.Fold(
            (Front: new ParetoFront(Seq<DesignPoint>(), grid.Senses), Points: Seq<DesignPoint>(), Failed: 0),
            static (acc, row) => row.Result.Match(
                Succ: objectives => { var point = new DesignPoint(row.Coords, [.. objectives], []); return (acc.Front.Insert(point), acc.Points.Add(point), acc.Failed); },
                Fail: static _ => (acc.Front, acc.Points, acc.Failed + 1)));
        return new SweepResult(grid, folded.Front, SensitivityTornado.Of(grid, folded.Points, grid.Policy.SensitivityObjective), folded.Points.Count, folded.Failed, clocks.Now);
    }

    // Frame-budgeted early-stop governor wrapping an iterative `step`: a frame-deadline expiry returns the
    // best-so-far coarse field WITHIN the frame and FORKS the refinement onto budget.Refinement (the IO.Fork
    // non-blocking continuation), never an inline await; the per-iteration residual and converged flag ride the
    // iterative solve's own Solve receipt, not a fabricated sweep flag.
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
