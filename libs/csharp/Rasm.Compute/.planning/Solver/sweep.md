# [COMPUTE_SWEEP]

Rasm.Compute solver sweep: one `SweepGrid` design-of-experiments orchestration emitting a queryable `ParetoFront` with a `SensitivityTornado` whose per-axis bars come from binned sampling or, where the oracle is hyper-dual-authorable, from exact forward-mode derivatives, beside one `FrameBudget` early-stop governor returning a coarse iterative field within a frame deadline and forking the refinement onto a background lane. Grid construction is the product of two orthogonal axes — a `SweepAxis` `[Union]` per-dimension factor and a `DoeDesign` `[SmartEnum<string>]` whole-grid strategy owning the design matrix — where factorial rows Cartesian-product per-axis levels while space-filling and response-surface rows draw a JOINT design across all dimensions, so a Latin-hypercube or Sobol sweep is one space-filling net, never the per-axis-1-D-then-Cartesian mis-model that defeats the variance reduction it exists to provide.

Per-point evaluation stays contract-uniform with the `Solver/optimizer#OPTIMIZER_LANE` and `Solver/uncertainty#UNCERTAINTY_LANE` lanes over one `DesignPoint`→objective-vector oracle, `IO`-lifted here alone (`IO<Fin<Seq<double>>>`) because the live `ProgressCell` observation, the CHUNKED fan-out, and the `FrameBudget` refinement fork compose in `IO` where those synchronous lanes take the bare `Fin<Seq<double>>`. Space-filling rows draw the `Tensor/sampling#OWNED_BUILDS` `LowDiscrepancy` joint d-dimensional sampler under the `Scramble` policy, the sensitivity reductions ride `TensorPrimitives` SIMD folds, and the `ParetoFront` is the optimizer's own artifact crossing to Persistence content-keyed. `SweepLane.Dataset` projects a landed `SweepResult` onto the `DoeDataset` wire shape — the `[GRADUATION]` loop's training leg: this lane EXPORTS the labeled corpus, and the fitted ONNX asset arrives back by content key through the `Model/identity#MODEL_IDENTITY` `ModelSource.Acquire` admission under its `GraduationEnvelope` evidence key, where the `Solver/optimizer` neural-field surrogate reads it. The producer of that asset is an external training environment this branch neither names nor constrains; the branch states its own domain and the graduation contract is the whole of what crosses. `ComputeReceipt`, `WorkLane`, `CpuBudget`, `CorrelationId`, NodaTime `IClock` (the App-owned `ClockPolicy` stays at composition), and the Thinktecture `ComparerAccessors.StringOrdinal` accessor arrive settled.

## [01]-[INDEX]

- [02]-[SWEEP_AND_BUDGET]: N-dim DOE design-matrix strategy; binned global sensitivity; frame-budgeted early-stop; rolled-up per-point progress aggregate.

## [02]-[SWEEP_AND_BUDGET]

- Owner: `SweepAxis` `[Union]` per-dimension factor cases; `DoeDesign` `[SmartEnum<string>]` whole-grid strategy rows; `SensitivityMethod` `[SmartEnum<string>]` global-sensitivity rows carrying the exact-versus-sampled column and the `Rank` fold; `SensitivityEvidence`/`SensitivityBar` the per-axis measure carrier and its ranked bar; `DoePolicy` the sample/bin/center/axial/fraction/scramble/cardinality policy and sensitivity-objective index; `SweepGrid` the validated axes+objectives+sensitivity+strategy record with its optional hyper-dual objective; `FrameBudget` the early-stop governor; `SensitivityTornado` the per-axis effect ranking beside the axes its method leaves unranked; `SweepResult` the front+tornado+points+counts carrier; `DoeDataset` the content-keyed training-corpus egress the Python companion trains on; `SweepLane` the fan-out fold, admitted progress consumer, and `Dataset` egress projection.
- Cases: `SweepAxis` `Linear` · `Logarithmic` · `Enumerated`; `DoeDesign` full-factorial · fractional-factorial · plackett-burman · latin-hypercube · sobol · halton · central-composite · box-behnken (central-composite/box-behnken the two `responseSurface` rows on coded ±1/±α/0 grids, latin-hypercube/sobol/halton the three `spaceFilling` JOINT designs); `SensitivityMethod` one-at-a-time · morris-elementary · sobol-variance (the three sampled rows) · dual-forward (the exact row reading the hyper-dual gradient).
- Entry: `public static (Option<ProgressCell> Progress, IO<Fin<SweepResult>> Result) Run(SweepGrid grid, CpuBudget budget, Func<DesignPoint, IO<Fin<Seq<double>>>> evaluate, Func<Seq<ImmutableArray<double>>, Option<(ProgressCell Parent, PhaseSubscription Wiring, Seq<ProgressCell> Points)>> progress, IClock clock)` — the scheduler-supplied factory owns admitted progress minting; invalid grids fault before materialization, and an individual point fault tallies incomplete rather than aborting. `Governed` wraps an iterative step and forks refinement onto `budget.Refinement` after a cooperative frame-budget expiry.
- Auto: `SweepGrid.Design` dispatches the design matrix on the `DoeDesign` row; `Run` partitions the design into `CpuBudget.Workers` chunks, forks ONE effect per chunk, evaluates each chunk's points sequentially inside it, validates each objective vector, folds successes into `ParetoFront`, tallies faults, and projects `SensitivityTornado`; the injected progress bundle advances admitted point cells and disposes its `PhaseSubscription` through `Bracket`.
- Receipt: `Sweep(long GridPoints, int Completed, int OnFront, int Dominated, int Unranked, int Failed)` from `Runtime/receipts#RECEIPT_UNION`; `SweepLane.Receipt` projects a `SweepResult` under the correlation — `GridPoints` the MATERIALIZED design count the run actually walked, `OnFront` the front size, dominated `Completed − OnFront`, `Unranked` the tornado's withheld-bar roster size, failed `GridPoints − Completed`; the frame-budget early-stop's per-iteration residual rides the iterative solve's own `Solve` receipt (`Solver/contract#SOLVE_CONTRACT`), never a fabricated sweep flag.
- Packages: System.Numerics.Tensors, System.IO.Hashing (`XxHash128.HashToUInt128` the `DoeDataset` content key), HyperJet (`DDScalar` the hyper-dual objective the exact sensitivity row differentiates, reached through `Tensor/dispatch#EQUIVALENCE_INTEROP` `SensitivityLaw.Gradient` and never bound here), Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, Rasm.AppHost (project), Rasm.Persistence (project), BCL inbox (`BinaryPrimitives` little-endian value framing, `Enumerable.Chunk` the worker partition)
- Growth: a new design-of-experiments strategy is one `DoeDesign` row and its `Materialize`/`Cardinality` arm; a new factor kind is one `SweepAxis` case carrying its `Levels`+`Map` lowering, its `Continuous` column, and its `Span`/`Chain` transform pair; a new sensitivity analysis is one `SensitivityMethod` row carrying its `Exact` column and its `Rank` fold, a new per-axis measure one slot on `SensitivityEvidence` the rows that take it read; a frame-budget change is one field on `FrameBudget`/`DoePolicy`; zero new surface — a `FactorialSweep`/`LatinHypercubeSweep`/`SobolSweep`/`ResponseSurface` sibling collapses onto the one `DoeDesign` axis, and a per-axis `SweepAxis.LatinHypercube`/`SweepAxis.Sobol` case is rejected because a space-filling design is joint across dimensions, never a per-axis 1-D sequence Cartesian-producted.
- Boundary: `evaluate` is the single `IO`-lifted solver coupling and the fan-out is CHUNKED to `CpuBudget.Workers` — each `ForkIO` spins a DEDICATED long-running thread, so one fork per design point turns a 4096-point sweep into 4096 threads and the machine spends its time scheduling rather than solving. One fork per chunk keeps the overlap the fan-out exists for at the governed thread count, and a bare `Traverse` over the evaluations — which sequences them outright — is the other deleted form.
- Boundary: `SweepGrid.Validate` rejects invalid axes, aliased fractional generators, unbounded in-memory grids, absent objectives, invalid sensitivity columns, and an enumerated axis under a space-filling or response-surface row — a categorical factor has no interior to fill and no coded ±α level to reach, so the joint net and the axial star both quantize back onto the same handful of values and the design silently degenerates. Enumerated axes admit the factorial and screening rows alone. Point faults accumulate without aborting independent rows.
- Boundary: `SensitivityTornado` stratifies coordinate bins LEVEL-aware — a factor with no more distinct values than bins gets one bin per level, so a three-level factor reports three conditional means rather than eight equal-count strata that split one level across bins and read its within-level noise as an effect. Every measure a method never takes withholds its bar onto the `Unranked` column — an empty campaign, a degenerate response variance, an enumerated axis under the exact row, and a refused gradient each report absence, never a zero-effect bar a reader takes for measured insensitivity.
- Boundary: exact-row admissibility IS the FD/AD partition this package already rules for the modal oracle at `Solver/clash#CLASH_AND_TWIN`: a hyper-dual-authorable objective earns exact derivatives, a full-FE, subprocess, or ONNX oracle stays black-box on the sampled rows, and `Validate` refuses the mismatch before materialization rather than degrading silently. `Tensor/dispatch#EQUIVALENCE_INTEROP` `SensitivityLaw.Gradient` supplies that exact source, read once at the design centre for every axis — a per-axis re-evaluation, a lane-local dual scalar, and a finite-difference stand-in behind the exact row's name are the three deleted forms. The linearized bar takes each axis's OWN transform: a logarithmic factor spanning two decades has a geometric extent, so its slope chains through the centre value and its span is the log-space extent — the arithmetic difference of the endpoints reports a bar dominated by the upper decade alone.
- Boundary: scheduler composition supplies the admitted `ProgressCell` leaves, parent, and `PhaseSubscription`; sweep advances and disposes them but never mints an `AdmittedIntent`. `Governed` requires cooperative `step` settlement and forks refinement through `IO.Fork`; a frame-budget expiry returns the BEST SETTLED refinement rather than discarding the frame's work — only an expiry before the first refinement settles has nothing to return and faults.

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

    // Continuity is the axis's own column, not a caller's claim: a derivative along an enumerated factor names no
    // direction, so the exact sensitivity row ranks the continuous axes and reports the rest unranked.
    public bool Continuous =>
        Switch(linear: static _ => true, logarithmic: static _ => true, enumerated: static _ => false);

    // The axis's extent in ITS OWN transform coordinate: arithmetic for a linear factor, LOGARITHMIC for a decade
    // sweep. A stiffness spanning 1e3 to 1e5 has a span of two decades, not 99000 — and the linearized bar built on
    // the arithmetic difference reports the upper decade's slope as the whole axis effect.
    public double Span =>
        Switch(
            linear: static a => a.Upper - a.Lower,
            logarithmic: static a => Math.Log(a.Upper / a.Lower),
            enumerated: static _ => 0.0);

    // The chain factor carrying a PHYSICAL derivative into that transform coordinate: `d/d(ln x) = x · d/dx`, so the
    // exact gradient read at the design centre multiplies by the centre value on a log axis and by one elsewhere.
    // Span and Chain are ONE decision — the pair is what makes `Slope × Span` the response change over the axis.
    public double Chain(double physical) =>
        Switch(
            state: physical,
            linear: static (_, _) => 1.0,
            logarithmic: static (x, _) => x,
            enumerated: static (_, _) => 0.0);

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
            plackettBurman: static s => PlackettBurmanMatrix(s.Axes),
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

    // A screening design is only orthogonal if its matrix is genuinely Hadamard. Sylvester's construction is Hadamard
    // by induction; the Paley construction is Hadamard only for a prime `q ≡ 3 (mod 4)`, and a composite or wrongly
    // classed `q` yields a matrix whose columns are correlated — a design that reports main effects aliased with each
    // other as if they were separated. The `HHᵀ = nI` check proves it POST-construction rather than trusting the
    // primality screen upstream, and a failure is a typed refusal, never a silently non-orthogonal screen.
    static Fin<Seq<ImmutableArray<double>>> PlackettBurmanMatrix(Seq<SweepAxis> axes) {
        int k = axes.Count, runs = (int)ScreeningOrder(k + 1);
        int[][] h = BitOperations.IsPow2(runs) ? Sylvester(runs) : Paley(runs - 1);
        return Hadamard(h, runs)
            ? Fin.Succ(toSeq(Enumerable.Range(0, runs)).Map(r => (ImmutableArray<double>)[.. axes.Map((axis, f) => axis.Map((h[r][f + 1] + 1.0) * 0.5))]))
            : Fin.Fail<Seq<ImmutableArray<double>>>(new ComputeFault.ModelRejected($"<sweep-screening-not-hadamard:{runs}>"));
    }

    static bool Hadamard(int[][] h, int order) {
        if (h.Length != order || h.Any(row => row.Length != order)) { return false; }
        for (int i = 0; i < order; i++) {
            for (int j = i; j < order; j++) {
                int dot = 0;
                for (int c = 0; c < order; c++) { dot += h[i][c] * h[j][c]; }
                if (dot != (i == j ? order : 0)) { return false; }
            }
        }
        return true;
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

// One evidence carrier per axis seats each measure on the slot its own producing fold fills: sampling rows read
// binned conditional means beside the response variance, exact rows read the derivative beside the axis span, and
// any measure the fold never took stays ABSENT — empty bins name an unbinned axis, an absent slope an underivable
// one. Nothing defaults, so `Rank` withholds the bar rather than publishing a zero effect a reader cannot tell
// from a measured insensitivity.
public readonly record struct SensitivityEvidence(Seq<double> Bins, double GlobalVariance, Option<double> Slope, double Span);

public readonly record struct SensitivityBar(string Axis, double Low, double High, double Effect);

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SensitivityMethod {
    public static readonly SensitivityMethod OneAtATime = new("one-at-a-time", exact: false);
    public static readonly SensitivityMethod MorrisElementary = new("morris-elementary", exact: false);
    public static readonly SensitivityMethod SobolVariance = new("sobol-variance", exact: false);

    // Exact forward-mode row: the effect is the linearized response change over the axis span read off the ONE
    // hyper-dual gradient the whole design centre yields, never a sampled estimate. Admissibility is the FD/AD
    // partition `Solver/clash#CLASH_AND_TWIN` already rules — an oracle authorable over `DDScalar` earns this row,
    // a full-FE or subprocess oracle stays with the sampling rows, and `SweepGrid.Validate` refuses the mismatch
    // rather than silently degrading to a zero field.
    public static readonly SensitivityMethod DualForward = new("dual-forward", exact: true);

    // `Exact` marks a row reading derivatives instead of samples: `SensitivityTornado.Of` fills the `Slope` slot
    // only for it and `Validate` gates the differentiable objective on it, so selection is one column, not a branch.
    public bool Exact { get; }

    public Option<SensitivityBar> Rank(string axis, SensitivityEvidence evidence) =>
        Switch(
            state: (Axis: axis, Evidence: evidence),
            oneAtATime: static s => s.Evidence.Bins is { Count: >= 2 } bins
                && bins.Min(double.PositiveInfinity) is var low && bins.Max(double.NegativeInfinity) is var high
                ? Some(new SensitivityBar(s.Axis, low, high, Math.Abs(high - low)))
                : None,
            morrisElementary: static s => Elementary(s.Axis, s.Evidence.Bins),
            // Spreadless responses support no variance share, and this refusal is STRUCTURAL: a ratio against zero
            // global variance reads non-finite and reports unranked, where a tuned absolute floor stays unreplayable
            // across responses and renders a degenerate campaign as a flat tornado.
            sobolVariance: static s => s.Evidence.Bins is { Count: >= 2 } bins
                && BinVariance(bins) / s.Evidence.GlobalVariance is var share && double.IsFinite(share)
                ? Some(new SensitivityBar(s.Axis, bins.Min(double.PositiveInfinity), bins.Max(double.NegativeInfinity), share))
                : None,
            dualForward: static s => s.Evidence.Slope.Bind(slope => Banded(s.Axis, slope * s.Evidence.Span)));

    static Option<SensitivityBar> Banded(string axis, double change) =>
        double.IsFinite(change)
            ? Some(new SensitivityBar(axis, Math.Min(0.0, change), Math.Max(0.0, change), Math.Abs(change)))
            : None;

    static Option<SensitivityBar> Elementary(string axis, Seq<double> bins) {
        if (bins.Count < 2) { return None; }
        double[] effects = [.. toSeq(Enumerable.Range(1, bins.Count - 1)).Map(i => Math.Abs(bins[i] - bins[i - 1]))];
        double muStar = TensorPrimitives.Average<double>(effects), sigma = TensorPrimitives.StdDev<double>(effects);
        return Some(new SensitivityBar(axis, muStar - sigma, muStar + sigma, muStar));
    }

    static double BinVariance(Seq<double> bins) => Math.Pow(TensorPrimitives.StdDev<double>([.. bins]), 2.0);
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

    // `Differentiable` carries the hyper-dual form of the `Policy.SensitivityObjective` column's OWN objective —
    // one scalar authored over `DDScalar`, absent wherever the oracle crosses a full FE solve, a subprocess, or an
    // ONNX session. It rides the grid rather than `Run` because `Validate` gates the exact row before materialization.
    public Option<Func<DDScalar[], DDScalar>> Differentiable { get; init; } = None;

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
        // FD/AD admissibility: the exact row needs a hyper-dual-authorable objective and at least one continuous
        // axis, and the mismatch REFUSES here rather than materializing a design whose every bar comes back unranked.
        bool sensitivity = Sensitivity.Exact && (Differentiable.IsNone || !Axes.Exists(static axis => axis.Continuous));
        // An enumerated factor has no interior for a joint net to fill and no coded ±α level for an axial star to
        // reach: both families quantize straight back onto the same handful of values, so the design degenerates to a
        // repeated factorial while still reporting itself as space-filling. Enumerated axes admit factorial and
        // screening rows alone, and the mismatch refuses before materialization.
        bool discrete = (Strategy.SpaceFilling || Strategy.ResponseSurface) && Axes.Exists(static axis => !axis.Continuous);
        return shape || policy || fractional || sensitivity || discrete || Cardinality > Policy.MaxPoints
            ? Fin.Fail<Unit>(ComputeFault.Create("<sweep-invalid-grid>"))
            : Fin.Succ(unit);
    }
}

// Bars carry the RANKED axes alone and `Unranked` names every axis whose method could take no measure — an
// under-sampled bin set, a degenerate response variance, an enumerated axis under the exact row, a refused
// gradient. A zero-effect bar beside them would read as a measured insensitivity and send a design decision the
// campaign never supported, so absence is a named column rather than a filled row.
public sealed record SensitivityTornado(Seq<SensitivityBar> Bars, Seq<string> Unranked) {
    public static SensitivityTornado Of(SweepGrid grid, Seq<DesignPoint> results, int objective) {
        double[] response = [.. results.Map(p => objective < p.Objectives.Length ? p.Objectives[objective] : 0.0)];
        double variance = results.Count >= 2 ? Math.Pow(TensorPrimitives.StdDev<double>(response), 2.0) : 0.0;
        int bins = Math.Max(2, grid.Policy.SensitivityBins);
        Seq<Option<double>> slopes = Slopes(grid);
        Seq<(string Axis, Option<SensitivityBar> Bar)> ranked = grid.Axes.Map((axis, index) => (
            Axis: axis.AxisName,
            Bar: grid.Sensitivity.Rank(axis.AxisName, new SensitivityEvidence(
                Bins: ConditionalMeans(results, index, objective, bins),
                GlobalVariance: variance,
                Slope: slopes[index],
                Span: axis.Span))));
        return new(
            toSeq(ranked.Choose(static row => row.Bar).OrderByDescending(static bar => bar.Effect)),
            ranked.Filter(static row => row.Bar.IsNone).Map(static row => row.Axis));
    }

    // ONE hyper-dual evaluation at the design centre yields the whole gradient — `SensitivityLaw.Gradient` is the
    // package's own forward-mode leg on the `Tensor/dispatch#EQUIVALENCE_INTEROP` `Sensitivity` family, so this
    // lane reads a gradient SOURCE that already exists and mints no fourth mechanism (the kernel `Dual<T>` floor
    // differentiates an `ILmModel` residual and never a design vector, so it is not this lane's source). A refused
    // evaluation or a discrete axis leaves the slot absent and the axis reports unranked.
    // The gradient arrives in PHYSICAL coordinates at the design centre, so each axis chains it into its own
    // transform before pairing with that axis's span — a log axis multiplies by the centre value, a linear axis by
    // one, and `Slope × Span` is then the response change across the whole axis in either case.
    static Seq<Option<double>> Slopes(SweepGrid grid) =>
        grid.Sensitivity.Exact
            ? grid.Differentiable
                .Bind(objective => SensitivityLaw.Gradient(objective, [.. grid.Axes.Map(static axis => axis.Map(0.5))]).ToOption())
                .Match(
                    Some: exact => grid.Axes.Map((axis, index) =>
                        axis.Continuous && index < exact.Gradient.Count
                            ? Some(exact.Gradient[index] * axis.Chain(axis.Map(0.5)))
                            : Option<double>.None),
                    None: () => grid.Axes.Map(static _ => Option<double>.None))
            : grid.Axes.Map(static _ => Option<double>.None);

    // Stratification is LEVEL-aware: a factor carrying no more distinct coordinates than the bin count gets one bin
    // PER LEVEL, because equal-count strata over a three-level factor split one level across two bins and read its
    // within-level response noise as a conditional-mean effect the campaign never varied.
    static Seq<double> ConditionalMeans(Seq<DesignPoint> results, int index, int objective, int bins) {
        (double X, double Y)[] ordered = [.. results
            .Map(p => (X: index < p.Coordinates.Length ? p.Coordinates[index] : 0.0, Y: objective < p.Objectives.Length ? p.Objectives[objective] : 0.0))
            .OrderBy(static row => row.X)];
        int n = ordered.Length;
        double[] levels = [.. ordered.Select(static row => row.X).Distinct()];
        if (levels.Length <= bins) {
            return toSeq(levels).Map(level => {
                double sum = 0.0; int count = 0;
                for (int i = 0; i < n; i++) { if (ordered[i].X == level) { sum += ordered[i].Y; count++; } }
                return count > 0 ? sum / count : 0.0;
            });
        }
        int width = Math.Max(1, n / bins);
        return toSeq(Enumerable.Range(0, Math.Min(bins, n))).Map(b => {
            int lo = b * width, hi = b == bins - 1 ? n : Math.Min(n, lo + width);
            double sum = 0.0; int count = 0;
            for (int i = lo; i < hi; i++) { sum += ordered[i].Y; count++; }
            return count > 0 ? sum / count : 0.0;
        });
    }
}

// Points retains every COMPLETED evaluation — dominated rows are training corpus the front alone would lose.
// `GridPoints` is the MATERIALIZED design size the run actually walked; `SweepGrid.Cardinality` is a pre-estimate
// the admission gate compares against `MaxPoints` and a screening or fractional row can only approximate, so a
// receipt reading the estimate would report a grid the campaign never evaluated. `Failed` derives from the pair.
public sealed record SweepResult(SweepGrid Grid, ParetoFront Front, SensitivityTornado Tornado, Seq<DesignPoint> Points, long GridPoints, int Completed, Instant At) {
    public int Failed => (int)Math.Max(0L, GridPoints - Completed);
}

// Surrogate training-data egress: the e13 `DoeDataset` wire shape the Python companion trains on — columnar
// coordinates, responses, and front membership in row-major blocks, axis names, design provenance, and a
// little-endian content key, so every screening campaign is training corpus and the neural-field refresh loop
// closes without a manual hand-off; the `Runtime/codecs` Arrow record-batch arm projects this same carrier
// lake-queryable with the content key preserved as batch metadata.
public sealed record DoeDataset(
    UInt128 ContentKey, Seq<string> Axes, Seq<string> Objectives, DoeDesign Strategy,
    int Points, ReadOnlyMemory<double> Coordinates, ReadOnlyMemory<double> Responses, ReadOnlyMemory<bool> OnFront, Instant At);

// --- [OPERATIONS] -----------------------------------------------------------------------

// Coordinate VALUE identity for the front-membership join. Bit-exact by design: both sides are the same doubles the
// same fold produced, so a tolerance here would merge two genuinely distinct design points that a screening grid
// deliberately placed close together.
public sealed class CoordinateComparer : IEqualityComparer<ImmutableArray<double>> {
    public static readonly CoordinateComparer Ordinal = new();

    private CoordinateComparer() { }

    public bool Equals(ImmutableArray<double> left, ImmutableArray<double> right) => left.AsSpan().SequenceEqual(right.AsSpan());

    public int GetHashCode(ImmutableArray<double> value) {
        HashCode hash = new();
        foreach (double component in value) { hash.Add(component); }
        return hash.ToHashCode();
    }
}

public static class SweepLane {
    public static (Option<ProgressCell> Progress, IO<Fin<SweepResult>> Result) Run(
        SweepGrid grid,
        CpuBudget budget,
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
                // CHUNKED fork-before-await fan-out (the Runtime/scheduling#JOB_GRAPH precedent): each ForkIO spins
                // its OWN long-running thread, so one fork per design point turns a 4096-point sweep into 4096
                // threads competing for the cores the budget already bounds. The launch partitions the design into
                // `budget.Workers` chunks, forks ONE effect per chunk before any await so the chunks genuinely
                // overlap, and walks each chunk's points sequentially inside it — every point still settles its own
                // fault and its own progress cell. A bare Traverse over the evaluations sequences them outright and
                // is the other deleted form.
                Seq<(ImmutableArray<double> Coords, Option<ProgressCell> Cell)> points =
                    design.Map((coords, index) => (Coords: coords, Cell: observation.Bind(state => index < state.Points.Count ? Some(state.Points[index]) : None)));
                int workers = Math.Max(1, Math.Min(budget.Workers, points.Count));
                IO<Fin<SweepResult>> use = toSeq(points.Chunk((points.Count + workers - 1) / workers))
                    .Traverse(chunk => toSeq(chunk).Traverse(pair =>
                            from _started in Advance(pair.Cell, ProgressPhase.Running)
                            from raw in evaluate(new DesignPoint(pair.Coords, [], []))
                            let outcome = ValidateObjectives(grid, raw)
                            from _settled in Advance(pair.Cell, outcome.IsSucc ? ProgressPhase.Completed : ProgressPhase.Faulted)
                            select (Coords: pair.Coords, Result: outcome))
                        .As()
                        .Fork())
                    .As()
                    .Bind(handles => handles.Traverse(static handle => handle.Await).As())
                    .Map(chunks => Fin.Succ(Reduce(grid, chunks.Bind(identity), design.Count, clock)))
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
        // Front membership is a COORDINATE-VALUE test: `DesignPoint` wraps `ImmutableArray<double>`, whose default
        // equality is the array REFERENCE, so a set of front points answers false for the very rows it holds once a
        // fold has re-materialized any of them — and the exported corpus then labels every point as dominated.
        HashSet<ImmutableArray<double>> front = new(result.Front.Points.Map(static p => p.Coordinates), CoordinateComparer.Ordinal);
        double[] coordinates = new double[points.Count * d];
        double[] responses = new double[points.Count * m];
        bool[] onFront = new bool[points.Count];
        for (int row = 0; row < points.Count; row++) {                          // row-major block fill — the columnar wire layout the tabular ingest reads
            for (int axis = 0; axis < d; axis++) { coordinates[row * d + axis] = points[row].Coordinates[axis]; }
            for (int objective = 0; objective < m; objective++) { responses[row * m + objective] = points[row].Objectives[objective]; }
            onFront[row] = front.Contains(points[row].Coordinates);
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

    // Every count reads the MATERIALIZED run: `GridPoints` the design the fold walked, `Unranked` the axes the
    // tornado withheld a bar for. The pre-estimate `Cardinality` bounds the admission and never reaches a receipt.
    public static ComputeReceipt.Sweep Receipt(SweepResult result, CorrelationId correlation, Duration elapsed) =>
        new(result.GridPoints, result.Completed, result.Front.Points.Count,
            Math.Max(0, result.Completed - result.Front.Points.Count),
            result.Tornado.Unranked.Count, result.Failed) {
            Scope = new ReceiptScope.Execution(correlation, WorkLane.Background, Substrate.CpuTensor, AllocationClass.PooledMemory, elapsed),
        };

    static SweepResult Reduce(SweepGrid grid, Seq<(ImmutableArray<double> Coords, Fin<Seq<double>> Result)> rows, int gridPoints, IClock clock) {
        (ParetoFront Front, Seq<DesignPoint> Points) folded = rows.Fold(
            (Front: new ParetoFront(Seq<DesignPoint>(), grid.Senses), Points: Seq<DesignPoint>()),
            static (acc, row) => row.Result.Match(
                Succ: objectives => { DesignPoint point = new(row.Coords, [.. objectives], []); return (acc.Front.Insert(point), acc.Points.Add(point)); },
                Fail: static _ => acc));
        return new SweepResult(grid, folded.Front, SensitivityTornado.Of(grid, folded.Points, grid.Policy.SensitivityObjective),
            folded.Points, gridPoints, folded.Points.Count, clock.GetCurrentInstant());
    }

    public static Func<DesignPoint, IO<Fin<Seq<double>>>> Governed(
        FrameBudget budget,
        Func<DesignPoint, int, IO<Fin<IterativeField>>> step,
        Func<DesignPoint, WorkLane, IO<Unit>> refine,
        IClock clock) =>
        point => {
            if (budget.Invalid) { return IO.pure(Fin.Fail<Seq<double>>(ComputeFault.Create("<frame-budget-invalid>"))); }
            // The published cell survives the timeout the iteration itself cannot observe: `Timeout` abandons the
            // in-flight effect, so a governor that reads only its return value discards every refinement the frame
            // DID settle. Each accepted refinement commits here, and an expiry answers the best-so-far — the frame's
            // whole point being a coarse field NOW with the refinement continuing on the background lane. Only an
            // expiry before the FIRST refinement settles has nothing to return.
            Atom<Option<IterativeField>> settled = Atom(Option<IterativeField>.None);
            return
                from outcome in Iterate(budget, step, point, settled, clock)
                    .Timeout(budget.Deadline.ToTimeSpan())
                    .Catch(static error => error.Is(Errors.TimedOut), _ => IO.pure((BestSoFar(settled), true)))
                from _ in outcome.Early ? refine(point, budget.Refinement).Fork().As().Map(static _ => unit) : IO.pure(unit)
                select outcome.Best.Map(static r => r.Field);
        };

    // A budget-expired answer carries `Done: false`, so a consumer reads the coarse field as unfinished rather than
    // as a converged solve — the expiry mark rides the field the iteration already carries, never a second flag.
    static Fin<IterativeField> BestSoFar(Atom<Option<IterativeField>> settled) =>
        settled.Value.Match(
            Some: static field => Fin.Succ(field with { Done = false }),
            None: static () => Fin.Fail<IterativeField>(ComputeFault.Create("<frame-budget-no-iteration>")));

    static IO<(Fin<IterativeField> Best, bool Early)> Iterate(
        FrameBudget budget, Func<DesignPoint, int, IO<Fin<IterativeField>>> step, DesignPoint point, Atom<Option<IterativeField>> settled, IClock clock) =>
        IO.liftAsync(async env => {
            Instant start = clock.GetCurrentInstant();
            Fin<IterativeField> best = Fin.Fail<IterativeField>(ComputeFault.Create("<frame-budget-no-iteration>"));
            for (int iteration = 0; ; iteration++) {
                best = await step(point, iteration).RunAsync(env);
                best.Iter(field => ignore(settled.Swap(_ => Some(field))));       // publish before the next await, so an abandoned frame still has it
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
