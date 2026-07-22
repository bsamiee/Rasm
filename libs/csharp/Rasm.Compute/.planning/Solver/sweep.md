# [COMPUTE_SWEEP]

Rasm.Compute solver sweep: one `SweepGrid` design-of-experiments orchestration emitting a queryable `ParetoFront` with a binned global-sensitivity `SensitivityTornado`, beside one `FrameBudget` early-stop governor returning a coarse iterative field within a frame deadline and forking the refinement onto a background lane. Grid construction is the product of two orthogonal axes — a `SweepAxis` `[Union]` per-dimension factor and a `DoeDesign` `[SmartEnum<string>]` whole-grid strategy owning the design matrix — where factorial rows Cartesian-product per-axis levels while space-filling and response-surface rows draw a JOINT design across all dimensions, so a Latin-hypercube or Sobol sweep is one space-filling net, never the per-axis-1-D-then-Cartesian mis-model that defeats the variance reduction it exists to provide.

Per-point evaluation stays contract-uniform with the `Solver/optimizer#OPTIMIZER_LANE` and `Solver/uncertainty#UNCERTAINTY_LANE` lanes over one `DesignPoint`→objective-vector oracle, `IO`-lifted here alone (`IO<Fin<Seq<double>>>`) because the live `ProgressCell` observation and the `FrameBudget` refinement fork compose in `IO` where those synchronous lanes take the bare `Fin<Seq<double>>`. Space-filling rows draw the `Tensor/sampling#OWNED_BUILDS` `LowDiscrepancy` joint d-dimensional sampler under the `Scramble` policy, the sensitivity reductions ride `TensorPrimitives` SIMD folds, and the `ParetoFront` is the optimizer's own artifact crossing to Persistence content-keyed. `SweepLane.Dataset` projects a landed `SweepResult` onto the `DoeDataset` wire shape — the `[GRADUATION]` loop's training leg: C# sweep → `DoeDataset` → Python fit → graduated ONNX over `GraduationEvidence` → `Solver/optimizer` neural-field surrogate — so labeled sweep data feeds the surrogate refresh instead of dying in receipts, over the existing Runtime wire plane with no new transport. `ComputeReceipt`, `WorkLane`, `CorrelationId`, NodaTime `IClock` (the App-owned `ClockPolicy` stays at composition), and the `ComparerAccessors.StringOrdinal` accessor from `Solver/discretization#DISCRETIZATION_MESH` arrive settled.

## [01]-[INDEX]

- [01]-[SWEEP_AND_BUDGET]: N-dim DOE design-matrix strategy; binned global sensitivity; frame-budgeted early-stop; rolled-up per-point progress aggregate.

## [02]-[SWEEP_AND_BUDGET]

- Owner: `SweepAxis` `[Union]` per-dimension factor cases; `DoeDesign` `[SmartEnum<string>]` whole-grid strategy rows; `SensitivityMethod` `[SmartEnum<string>]` global-sensitivity rows; `DoePolicy` the sample/bin/center/axial/fraction/scramble/cardinality policy and sensitivity-objective index; `SweepGrid` the validated axes+objectives+sensitivity+strategy record; `FrameBudget` the early-stop governor; `SensitivityTornado` the quantile-binned per-axis effect ranking; `SweepResult` the front+tornado+points+counts carrier; `DoeDataset` the content-keyed training-corpus egress the Python companion trains on; `SweepLane` the fan-out fold, admitted progress consumer, and `Dataset` egress projection.
- Cases: `SweepAxis` `Linear` · `Logarithmic` · `Enumerated`; `DoeDesign` full-factorial · fractional-factorial · plackett-burman · latin-hypercube · sobol · halton · central-composite · box-behnken (central-composite/box-behnken the two `responseSurface` rows on coded ±1/±α/0 grids, latin-hypercube/sobol/halton the three `spaceFilling` JOINT designs); `SensitivityMethod` one-at-a-time · morris-elementary · sobol-variance.
- Entry: `public static (Option<ProgressCell> Progress, IO<Fin<SweepResult>> Result) Run(SweepGrid grid, Func<DesignPoint, IO<Fin<Seq<double>>>> evaluate, Func<Seq<ImmutableArray<double>>, Option<(ProgressCell Parent, PhaseSubscription Wiring, Seq<ProgressCell> Points)>> progress, IClock clock)` — the scheduler-supplied factory owns admitted progress minting; invalid grids fault before materialization, and an individual point fault tallies incomplete rather than aborting. `Governed` wraps an iterative step and forks refinement onto `budget.Refinement` after a cooperative frame-budget expiry.
- Auto: `SweepGrid.Design` dispatches the design matrix on the `DoeDesign` row; `Run` fans the design applicatively over the independent `evaluate` oracle, validates each objective vector, folds successes into `ParetoFront`, tallies faults, and projects `SensitivityTornado`; the injected progress bundle advances admitted point cells and disposes its `PhaseSubscription` through `Bracket`.
- Receipt: `Sweep(long GridPoints, int Completed, int OnFront, int Dominated)` from `Runtime/receipts#RECEIPT_UNION`; `SweepLane.Receipt` projects a `SweepResult` under the correlation — `OnFront` the front size, dominated `Completed − OnFront`, failed `GridPoints − Completed`; the frame-budget early-stop's per-iteration residual rides the iterative solve's own `Solve` receipt (`Solver/contract#SOLVE_CONTRACT`), never a fabricated sweep flag.
- Packages: System.Numerics.Tensors, System.IO.Hashing (`XxHash128.HashToUInt128` the `DoeDataset` content key), Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, Rasm.AppHost (project), Rasm.Persistence (project), BCL inbox (`BinaryPrimitives` little-endian value framing)
- Growth: a new design-of-experiments strategy is one `DoeDesign` row and its `Materialize`/`Cardinality` arm; a new factor kind is one `SweepAxis` case carrying its `Levels`+`Map` lowering; a new sensitivity analysis is one `SensitivityMethod` row carrying its `Rank` fold; a frame-budget change is one field on `FrameBudget`/`DoePolicy`; zero new surface — a `FactorialSweep`/`LatinHypercubeSweep`/`SobolSweep`/`ResponseSurface` sibling collapses onto the one `DoeDesign` axis, and a per-axis `SweepAxis.LatinHypercube`/`SweepAxis.Sobol` case is rejected because a space-filling design is joint across dimensions, never a per-axis 1-D sequence Cartesian-producted.
- Boundary: `evaluate` is the single `IO`-lifted solver coupling; space-filling rows draw one joint `LowDiscrepancy` net; `SweepGrid.Validate` rejects invalid axes, aliased fractional generators, unbounded in-memory grids, absent objectives, and invalid sensitivity columns before materialization. Point faults accumulate without aborting independent rows. `SensitivityTornado` bins equal-count coordinate strata before conditional-mean effects. Scheduler composition supplies the admitted `ProgressCell` leaves, parent, and `PhaseSubscription`; sweep advances and disposes them but never mints an `AdmittedIntent`. `Governed` requires cooperative `step` settlement, returns the best field after the budget predicate, and forks refinement through `IO.Fork`.

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

    public Seq<double> Levels =>
        Switch(
            linear: static a => toSeq(Enumerable.Range(0, Math.Max(1, a.Steps))).Map(i => a.Lower + (a.Upper - a.Lower) * i / Math.Max(1, a.Steps - 1)),
            logarithmic: static a => toSeq(Enumerable.Range(0, Math.Max(1, a.Steps))).Map(i => a.Lower * Math.Pow(a.Upper / a.Lower, (double)i / Math.Max(1, a.Steps - 1))),
            enumerated: static a => a.Values);

    public double Map(double unit) =>
        Switch(
            state: Math.Clamp(unit, 0.0, 1.0),
            linear: static (u, a) => a.Lower + (a.Upper - a.Lower) * u,
            logarithmic: static (u, a) => a.Lower * Math.Pow(a.Upper / a.Lower, u),
            enumerated: static (u, a) => a.Values.IsEmpty ? u : a.Values[Math.Min(a.Values.Count - 1, (int)Math.Round(u * (a.Values.Count - 1)))]);

    public bool Invalid =>
        Switch(
            linear: static axis => string.IsNullOrWhiteSpace(axis.Name) || !double.IsFinite(axis.Lower) || !double.IsFinite(axis.Upper) || axis.Lower >= axis.Upper || axis.Steps < 2,
            logarithmic: static axis => string.IsNullOrWhiteSpace(axis.Name) || !double.IsFinite(axis.Lower) || !double.IsFinite(axis.Upper) || axis.Lower <= 0.0 || axis.Lower >= axis.Upper || axis.Steps < 2,
            enumerated: static axis => string.IsNullOrWhiteSpace(axis.Name) || axis.Values.IsEmpty || !axis.Values.ForAll(double.IsFinite));
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

    public Fin<Seq<ImmutableArray<double>>> Materialize(Seq<SweepAxis> axes, DoePolicy policy) =>
        Switch(
            state: (Axes: axes, Policy: policy),
            fullFactorial: static s => Fin.Succ(Factorial(s.Axes)),
            fractionalFactorial: static s => Fin.Succ(Fractional(s.Axes, s.Policy.FractionExponent)),
            plackettBurman: static s => Fin.Succ(PlackettBurmanMatrix(s.Axes)),
            latinHypercube: static s => LatinHypercubeMatrix(s.Axes, s.Policy),
            sobol: static s => LowDiscrepancyMatrix(s.Axes, s.Policy, quasiSobol: true),
            halton: static s => LowDiscrepancyMatrix(s.Axes, s.Policy, quasiSobol: false),
            centralComposite: static s => Fin.Succ(CentralCompositeMatrix(s.Axes, s.Policy)),
            boxBehnken: static s => Fin.Succ(BoxBehnkenMatrix(s.Axes, s.Policy)));

    public long Cardinality(Seq<SweepAxis> axes, DoePolicy policy) =>
        Switch(
            state: (Axes: axes, Policy: policy),
            fullFactorial: static s => FactorialCardinality(s.Axes, s.Policy.MaxPoints),
            fractionalFactorial: static s => 1L << Math.Max(0, s.Axes.Count - Math.Clamp(s.Policy.FractionExponent, 0, Math.Max(0, s.Axes.Count - 1))),
            plackettBurman: static s => ScreeningOrder(s.Axes.Count + 1),
            latinHypercube: static s => Math.Max(2, s.Policy.Samples),
            sobol: static s => Math.Max(2, s.Policy.Samples),
            halton: static s => Math.Max(2, s.Policy.Samples),
            centralComposite: static s => (1L << s.Axes.Count) + 2L * s.Axes.Count + Math.Max(1, s.Policy.CenterPoints),
            boxBehnken: static s => 2L * s.Axes.Count * Math.Max(0, s.Axes.Count - 1) + Math.Max(1, s.Policy.CenterPoints));

    static long FactorialCardinality(Seq<SweepAxis> axes, long limit) =>
        axes.Fold(1L, (product, axis) => {
            long levels = Math.Max(1, axis.LevelCount);
            long overflow = limit == long.MaxValue ? long.MaxValue : limit + 1L;
            return product > limit / levels ? overflow : product * levels;
        });

    static Seq<ImmutableArray<double>> Factorial(Seq<SweepAxis> axes) =>
        axes.Fold(Seq(ImmutableArray<double>.Empty), static (acc, axis) => acc.Bind(prefix => axis.Levels.Map(prefix.Add)));

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

    static Seq<ImmutableArray<double>> PlackettBurmanMatrix(Seq<SweepAxis> axes) {
        int k = axes.Count, runs = (int)ScreeningOrder(k + 1);
        int[][] h = BitOperations.IsPow2(runs) ? Sylvester(runs) : Paley(runs - 1);
        return toSeq(Enumerable.Range(0, runs)).Map(r => (ImmutableArray<double>)[.. axes.Map((axis, f) => axis.Map((h[r][f + 1] + 1.0) * 0.5))]);
    }

    static Fin<Seq<ImmutableArray<double>>> LatinHypercubeMatrix(Seq<SweepAxis> axes, DoePolicy policy) {
        int n = Math.Max(2, policy.Samples), d = axes.Count;
        return LowDiscrepancy.LatinHypercube(d, n, policy.Seed, policy.Scramble)
            .Map(unit => toSeq(Enumerable.Range(0, n)).Map(s => (ImmutableArray<double>)[.. axes.Map((axis, f) => axis.Map(unit[s][f]))]));
    }

    static Fin<Seq<ImmutableArray<double>>> LowDiscrepancyMatrix(Seq<SweepAxis> axes, DoePolicy policy, bool quasiSobol) {
        int n = Math.Max(2, policy.Samples), d = axes.Count;
        Fin<LowDiscrepancy> generator = quasiSobol ? LowDiscrepancy.Sobol(d, policy.Seed, policy.Scramble) : LowDiscrepancy.Halton(d, policy.Seed, policy.Scramble);
        return generator
            .Map(g => Unit(g, n).Map(point => (ImmutableArray<double>)[.. axes.Map((axis, f) => axis.Map(point[Math.Min(f, point.Length - 1)]))]));
    }

    static Seq<ImmutableArray<double>> CentralCompositeMatrix(Seq<SweepAxis> axes, DoePolicy policy) {
        int k = axes.Count;
        double alpha = double.IsNaN(policy.AxialAlpha) ? Math.Pow(1 << k, 0.25) : policy.AxialAlpha;
        double scale = Math.Max(1.0, alpha);
        Seq<ImmutableArray<double>> corners = TwoLevel(k).Map(corner => Coded(axes, corner, scale));
        Seq<ImmutableArray<double>> axial = toSeq(Enumerable.Range(0, k)).Bind(f => Seq(Coded(axes, AxisVector(k, f, -alpha), scale), Coded(axes, AxisVector(k, f, alpha), scale)));
        Seq<ImmutableArray<double>> center = toSeq(Enumerable.Range(0, Math.Max(1, policy.CenterPoints))).Map(_ => Coded(axes, new double[k], scale));
        return corners + axial + center;
    }

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
            (LowDiscrepancy next, double[] point) = acc.Gen.Draw();
            return (next, acc.Points.Add(point));
        }).Points;

    static long ScreeningOrder(int minimum) {
        int sylvester = 1;
        while (sylvester < minimum) { sylvester <<= 1; }
        int paley = Enumerable.Range(Math.Max(3, minimum - 1), Math.Max(0, sylvester - minimum + 1))
            .Where(static q => q % 4 == 3 && Prime(q))
            .Select(static q => q + 1)
            .DefaultIfEmpty(sylvester)
            .Min();
        return Math.Min(sylvester, paley);
    }

    static int[][] Sylvester(int n) {
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

    static int[][] Paley(int q) {
        int[][] matrix = [.. Enumerable.Range(0, q + 1).Select(_ => new int[q + 1])];
        for (int axis = 0; axis <= q; axis++) { matrix[0][axis] = 1; matrix[axis][0] = 1; }
        for (int row = 0; row < q; row++) {
            for (int column = 0; column < q; column++) {
                int residue = (row - column + q) % q;
                matrix[row + 1][column + 1] = residue == 0 ? -1 : Legendre(residue, q);
            }
        }
        return matrix;
    }

    static int Legendre(int value, int prime) {
        long result = 1L, factor = value;
        for (int exponent = (prime - 1) / 2; exponent > 0; exponent >>= 1) {
            if ((exponent & 1) != 0) { result = result * factor % prime; }
            factor = factor * factor % prime;
        }
        return result == 1L ? 1 : -1;
    }

    static bool Prime(int value) =>
        value >= 2 && !Enumerable.Range(2, Math.Max(0, (int)Math.Sqrt(value) - 1)).Any(divisor => value % divisor == 0);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SensitivityMethod {
    public static readonly SensitivityMethod OneAtATime = new("one-at-a-time");
    public static readonly SensitivityMethod MorrisElementary = new("morris-elementary");
    public static readonly SensitivityMethod SobolVariance = new("sobol-variance");

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

    static double BinVariance(Seq<double> bins) =>
        bins.IsEmpty ? 0.0 : Math.Pow(TensorPrimitives.StdDev<double>([.. bins]), 2.0);
}

// --- [MODELS] ---------------------------------------------------------------------------

public sealed record DoePolicy(int Samples, int SensitivityBins, int CenterPoints, double AxialAlpha, int FractionExponent, Scramble Scramble, int Seed, int SensitivityObjective, long MaxPoints) {
    public static readonly DoePolicy Default = new(Samples: 256, SensitivityBins: 8, CenterPoints: 1, AxialAlpha: double.NaN, FractionExponent: 1, Scramble: Scramble.DigitalShift, Seed: 0x5DEECE66, SensitivityObjective: 0, MaxPoints: 1_000_000L);
    public static readonly DoePolicy SpaceFillingLarge = Default with { Samples = 4096, SensitivityBins = 16 };
}

public readonly record struct IterativeField(Seq<double> Field, double Residual, bool Done);

public sealed record FrameBudget(Duration Deadline, int MinIterations, int MaxIterations, WorkLane Refinement) {
    public static readonly FrameBudget Interactive = new(Duration.FromMilliseconds(16), MinIterations: 8, MaxIterations: 4096, WorkLane.Background);

    public bool Expired(Instant start, Instant now, int iteration) =>
        iteration >= MaxIterations || (iteration >= MinIterations && now - start >= Deadline);

    public bool Invalid => Deadline <= Duration.Zero || MinIterations < 1 || MaxIterations < MinIterations;
}

public sealed record SweepGrid(Seq<SweepAxis> Axes, Seq<ObjectiveSense> Objectives, SensitivityMethod Sensitivity) {
    public DoeDesign Strategy { get; init; } = DoeDesign.FullFactorial;
    public DoePolicy Policy { get; init; } = DoePolicy.Default;

    public Fin<Seq<ImmutableArray<double>>> Design => Strategy.Materialize(Axes, Policy);
    public long Cardinality => Strategy.Cardinality(Axes, Policy);
    public ImmutableArray<double> Senses => [.. Objectives.Map(static o => o.Sign)];

    public Fin<Unit> Validate() {
        int basis = Axes.Count - Policy.FractionExponent;
        long generators = basis is > 0 and < 31 ? (1L << basis) - basis - 1L : 0L;
        bool fractional = Strategy == DoeDesign.FractionalFactorial
            && (Policy.FractionExponent < 0 || Policy.FractionExponent >= Axes.Count || generators < Policy.FractionExponent);
        bool policy = Policy.Samples < 2 || Policy.SensitivityBins < 2 || Policy.CenterPoints < 1 || Policy.MaxPoints < 1
            || (!double.IsNaN(Policy.AxialAlpha) && (!double.IsFinite(Policy.AxialAlpha) || Policy.AxialAlpha <= 0.0));
        bool shape = Axes.IsEmpty || Axes.Count >= 31 || Axes.Exists(static axis => axis.Invalid)
            || Axes.Map(static axis => axis.AxisName).ToHashSet(StringComparer.Ordinal).Count != Axes.Count
            || Objectives.IsEmpty || Policy.SensitivityObjective < 0 || Policy.SensitivityObjective >= Objectives.Count
            || (Strategy == DoeDesign.BoxBehnken && Axes.Count < 3);
        return shape || policy || fractional || Cardinality > Policy.MaxPoints
            ? Fin.Fail<Unit>(ComputeFault.Create("<sweep-invalid-grid>"))
            : Fin.Succ(unit);
    }
}

public sealed record SensitivityTornado(Seq<(string Axis, double Low, double High, double Effect)> Bars) {
    public static SensitivityTornado Of(SweepGrid grid, Seq<DesignPoint> results, int objective) {
        if (results.IsEmpty) { return new(grid.Axes.Map(static a => (a.AxisName, 0.0, 0.0, 0.0))); }
        double[] response = [.. results.Map(p => objective < p.Objectives.Length ? p.Objectives[objective] : 0.0)];
        double globalVariance = Math.Pow(TensorPrimitives.StdDev<double>(response), 2.0);
        int bins = Math.Max(2, grid.Policy.SensitivityBins);
        return new(grid.Axes.Map((axis, index) => {
            (double low, double high, double effect) = grid.Sensitivity.Rank(ConditionalMeans(results, index, objective, bins), globalVariance);
            return (axis.AxisName, low, high, effect);
        }).OrderByDescending(static bar => bar.Item4).ToSeq());
    }

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

// Points retains every COMPLETED evaluation — dominated rows are training corpus the front alone would lose.
public sealed record SweepResult(SweepGrid Grid, ParetoFront Front, SensitivityTornado Tornado, Seq<DesignPoint> Points, int Completed, int Failed, Instant At);

// Surrogate training-data egress: the e13 `DoeDataset` wire shape the Python companion trains on — columnar
// coordinates, responses, and front membership in row-major blocks, axis names, design provenance, and a
// little-endian content key, so every screening campaign is training corpus and the neural-field refresh loop
// closes without a manual hand-off; the `Runtime/codecs` Arrow record-batch arm projects this same carrier
// lake-queryable with the content key preserved as batch metadata.
public sealed record DoeDataset(
    UInt128 ContentKey, Seq<string> Axes, Seq<string> Objectives, DoeDesign Strategy,
    int Points, ReadOnlyMemory<double> Coordinates, ReadOnlyMemory<double> Responses, ReadOnlyMemory<bool> OnFront, Instant At);

// --- [OPERATIONS] -----------------------------------------------------------------------

public static class SweepLane {
    public static (Option<ProgressCell> Progress, IO<Fin<SweepResult>> Result) Run(
        SweepGrid grid,
        Func<DesignPoint, IO<Fin<Seq<double>>>> evaluate,
        Func<Seq<ImmutableArray<double>>, Option<(ProgressCell Parent, PhaseSubscription Wiring, Seq<ProgressCell> Points)>> progress,
        IClock clock) {
        return grid.Validate().Bind(_ => grid.Design).Match(
            Succ: design => {
                Option<(ProgressCell Parent, PhaseSubscription Wiring, Seq<ProgressCell> Points)> observation = progress(design);
                Option<ProgressCell> parent = observation.Map(static state => state.Parent);
                if (observation.Exists(state => state.Points.Count != design.Count)) {
                    IO<Fin<SweepResult>> fault = observation.Match(
                        Some: state => IO.pure(state.Wiring).Bracket(
                            Use: static _ => IO.pure(Fin.Fail<SweepResult>(ComputeFault.Create("<sweep-progress-shape>"))),
                            Fin: static wiring => IO.lift(fun(wiring.Dispose))),
                        None: static () => IO.pure(Fin.Fail<SweepResult>(ComputeFault.Create("<sweep-progress-shape>"))));
                    return (parent, fault);
                }
                // Fork-before-await fan-out (the Runtime/scheduling#JOB_GRAPH precedent): the launch traversal
                // schedules EVERY point effect through IO.Fork under the runtime budget before any await, so
                // independent design points genuinely overlap and each point's fault and progress cell settles
                // on its own — a bare Traverse over the evaluations sequences the effects and is the deleted form.
                IO<Fin<SweepResult>> use = design.Map((coords, index) => (Coords: coords, Cell: observation.Bind(state => index < state.Points.Count ? Some(state.Points[index]) : None)))
                    .Traverse(pair =>
                        (from _started in Advance(pair.Cell, ProgressPhase.Running)
                         from raw in evaluate(new DesignPoint(pair.Coords, [], []))
                         let outcome = ValidateObjectives(grid, raw)
                         from _settled in Advance(pair.Cell, outcome.IsSucc ? ProgressPhase.Completed : ProgressPhase.Faulted)
                         select (Coords: pair.Coords, Result: outcome)).Fork())
                    .As()
                    .Bind(handles => handles.Traverse(static handle => handle.Await).As())
                    .Map(rows => Fin.Succ(Reduce(grid, rows, clock)))
                    .As();
                IO<Fin<SweepResult>> result = observation.Match(
                    Some: state => IO.pure(state.Wiring).Bracket(Use: _ => use, Fin: static wiring => IO.lift(fun(wiring.Dispose))),
                    None: () => use);
                return (parent, result);
            },
            Fail: static error => (None, IO.pure(Fin.Fail<SweepResult>(error))));
    }

    static IO<Unit> Advance(Option<ProgressCell> cell, ProgressPhase phase) =>
        IO.lift(() => cell.Iter(progress => ignore(progress.Advance(phase))));

    static Fin<Seq<double>> ValidateObjectives(SweepGrid grid, Fin<Seq<double>> result) =>
        result.Bind(values => values.Count == grid.Objectives.Count && values.ForAll(double.IsFinite)
            ? Fin.Succ(values)
            : Fin.Fail<Seq<double>>(ComputeFault.Create("<sweep-oracle-shape>")));

    // Completed points only — a faulted evaluation never enters the training corpus; the content key frames axis
    // names, strategy, both little-endian value blocks, and the front-membership block, so an identical campaign
    // re-export reuses its key.
    public static Fin<DoeDataset> Dataset(SweepResult result, IClock clock) {
        Seq<DesignPoint> points = result.Points;
        if (points.IsEmpty || points.Exists(p => p.Coordinates.Length != result.Grid.Axes.Count || p.Objectives.Length != result.Grid.Objectives.Count)) {
            return Fin.Fail<DoeDataset>(ComputeFault.Create("<doe-dataset-shape>"));
        }
        int d = result.Grid.Axes.Count, m = result.Grid.Objectives.Count;
        HashSet<DesignPoint> front = [.. result.Front.Points];
        double[] coordinates = new double[points.Count * d];
        double[] responses = new double[points.Count * m];
        bool[] onFront = new bool[points.Count];
        for (int row = 0; row < points.Count; row++) {                          // row-major block fill — the columnar wire layout the tabular ingest reads
            for (int axis = 0; axis < d; axis++) { coordinates[row * d + axis] = points[row].Coordinates[axis]; }
            for (int objective = 0; objective < m; objective++) { responses[row * m + objective] = points[row].Objectives[objective]; }
            onFront[row] = front.Contains(points[row]);
        }
        ArrayBufferWriter<byte> frame = new();
        foreach (string label in result.Grid.Axes.Map(static a => a.AxisName) + Seq(result.Grid.Strategy.Key)) {
            byte[] encoded = Encoding.UTF8.GetBytes(label);
            Span<byte> slot = frame.GetSpan(4 + encoded.Length);
            BinaryPrimitives.WriteInt32LittleEndian(slot, encoded.Length);
            encoded.CopyTo(slot[4..]);
            frame.Advance(4 + encoded.Length);
        }
        Span<byte> values = frame.GetSpan(8 * (coordinates.Length + responses.Length) + onFront.Length);
        for (int i = 0; i < coordinates.Length; i++) { BinaryPrimitives.WriteDoubleLittleEndian(values[(8 * i)..], coordinates[i]); }
        for (int i = 0; i < responses.Length; i++) { BinaryPrimitives.WriteDoubleLittleEndian(values[(8 * (coordinates.Length + i))..], responses[i]); }
        for (int i = 0; i < onFront.Length; i++) { values[8 * (coordinates.Length + responses.Length) + i] = onFront[i] ? (byte)1 : (byte)0; }
        frame.Advance(8 * (coordinates.Length + responses.Length) + onFront.Length);
        return Fin.Succ(new DoeDataset(
            XxHash128.HashToUInt128(frame.WrittenSpan),
            result.Grid.Axes.Map(static a => a.AxisName),
            toSeq(Enumerable.Range(0, m)).Map(static i => $"objective-{i}"),
            result.Grid.Strategy, points.Count, coordinates, responses, onFront, clock.GetCurrentInstant()));
    }

    public static ComputeReceipt.Sweep Receipt(SweepResult result, CorrelationId correlation, Duration elapsed) =>
        new(result.Grid.Cardinality, result.Completed, result.Front.Points.Count, Math.Max(0, result.Completed - result.Front.Points.Count)) {
            Scope = new ReceiptScope.Execution(correlation, WorkLane.Background, Substrate.CpuTensor, AllocationClass.PooledMemory, elapsed),
        };

    static SweepResult Reduce(SweepGrid grid, Seq<(ImmutableArray<double> Coords, Fin<Seq<double>> Result)> rows, IClock clock) {
        (ParetoFront Front, Seq<DesignPoint> Points, int Failed) folded = rows.Fold(
            (Front: new ParetoFront(Seq<DesignPoint>(), grid.Senses), Points: Seq<DesignPoint>(), Failed: 0),
            static (acc, row) => row.Result.Match(
                Succ: objectives => { DesignPoint point = new(row.Coords, [.. objectives], []); return (acc.Front.Insert(point), acc.Points.Add(point), acc.Failed); },
                Fail: static _ => (acc.Front, acc.Points, acc.Failed + 1)));
        return new SweepResult(grid, folded.Front, SensitivityTornado.Of(grid, folded.Points, grid.Policy.SensitivityObjective), folded.Points, folded.Points.Count, folded.Failed, clock.GetCurrentInstant());
    }

    public static Func<DesignPoint, IO<Fin<Seq<double>>>> Governed(
        FrameBudget budget,
        Func<DesignPoint, int, IO<Fin<IterativeField>>> step,
        Func<DesignPoint, WorkLane, IO<Unit>> refine,
        IClock clock) =>
        point => {
            if (budget.Invalid) { return IO.pure(Fin.Fail<Seq<double>>(ComputeFault.Create("<frame-budget-invalid>"))); }
            return
                from outcome in Iterate(budget, step, point, clock)
                    .Timeout(budget.Deadline.ToTimeSpan())
                    .Catch(static error => error.Is(Errors.TimedOut), static _ => IO.pure((Fin.Fail<IterativeField>(ComputeFault.Create("<frame-budget-timeout>")), true)))
                from _ in outcome.Early ? refine(point, budget.Refinement).Fork().As().Map(static _ => unit) : IO.pure(unit)
                select outcome.Best.Map(static r => r.Field);
        };

    static IO<(Fin<IterativeField> Best, bool Early)> Iterate(FrameBudget budget, Func<DesignPoint, int, IO<Fin<IterativeField>>> step, DesignPoint point, IClock clock) =>
        IO.liftAsync(async env => {
            Instant start = clock.GetCurrentInstant();
            Fin<IterativeField> best = Fin.Fail<IterativeField>(ComputeFault.Create("<frame-budget-no-iteration>"));
            for (int iteration = 0; ; iteration++) {
                best = await step(point, iteration).RunAsync(env);
                if (best.Match(Succ: static r => r.Done, Fail: static _ => true)) { return (best, false); }
                if (budget.Expired(start, clock.GetCurrentInstant(), iteration)) { return (best, true); }
            }
        });
}
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
