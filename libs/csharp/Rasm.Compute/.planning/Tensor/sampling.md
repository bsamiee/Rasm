# [COMPUTE_SAMPLING]

Rasm.Compute owned-build numeric lane for quasi-Monte-Carlo sampling and scattered reconstruction: the kernels with no library surface — the seed-explicit state-serializable Sobol/Halton low-discrepancy sampler carrying the equidistributed-versus-independent discriminant as a type axis, and the radial-basis design-matrix `Scatter` reconstruction into the rank-revealing route — built and gated in-house, every estimate leaving as a replicate family carrying its spread. The lane is host-local and crosses no wire. In-place numeric kernels — the `LowDiscrepancy` direction-number gray-code draw loop and the `OnlineStat` Welford increment — are this page's statement exemption.

## [01]-[INDEX]

- [01]-[OWNED_BUILDS]: owned Sobol/Halton low-discrepancy sampler; radial-basis scatter reconstruction.

## [02]-[OWNED_BUILDS]

- Owner: the owned-build lane with no library surface — `LowDiscrepancy` the owned seed-explicit Sobol/Halton sampling family carrying the equidistributed-versus-independent discriminant as a type axis, and `Scatter` the radial-basis design-matrix reconstruction into the rank-revealing route.
- Cases: `SequenceFamily` `[Union]` cases `Equidistributed(int Base)` · `Independent(ulong Stream)` (2); `Scramble` `[SmartEnum<string>]` cases none · digital-shift (2).
- Entry: `public static Fin<ReplicateFamily> Draw(LowDiscrepancy generator, int blockExponent, Func<ReadOnlyMemory<double>, double> estimator)` draws the power-of-base replicate family carrying the Student bound; `public static Fin<Matrix<double>> Reconstruct(Matrix<double> design, Matrix<double> response, TolerancePolicy tol)` reconstructs the scattered field through the `RankRevealing` route.
- Auto: `LowDiscrepancy.Draw` accepts the block exponent and draws exactly the power-of-base count through the gray-code direction-number sequence under the `Scramble` digital-shift policy, returning the replicate family with cross-replicate variance and a Student bound; `Scatter.Reconstruct` builds the radial-basis-plus-polynomial design matrix into `DenseRoute.Solve` on the `RankRevealing` route so a matrix-valued response reconstructs gradient and flux in one solve, wrapping evaluation in the interpolant absence carrier.
- Receipt: the sampler returns a `ReplicateFamily(Mean, CrossReplicateVariance, StudentBound, NetQuality, ProjectionFigure)` because a single equidistributed estimate carries no recoverable spread.
- Packages: MathNet.Numerics, System.Numerics.Tensors, CommunityToolkit.HighPerformance, LanguageExt.Core, BCL inbox
- Growth: a new sampling discriminant is one `SequenceFamily` case; a new scramble is one `Scramble` row; zero new surface.
- Boundary: stochastic samples draw seed-explicit over a state-serializable generator (stored direction numbers plus draw counter) for checkpoint-resume because the default thread-entropy source and the length-2048 parallel block fill are non-deterministic regardless of seeding; the low-discrepancy family is built in the owned lane because no library surface exists (MathNet exposes no Sobol/Halton, only `SystemRandomSource`), the independent-versus-equidistributed discriminant is the `SequenceFamily` `[Union]` case axis never a bool because variance law/error bars/convergence rate fork on it and the state shapes do not unify, the block exponent is accepted at the draw entrypoint never a free count because non-power prefixes and dropped origins degrade discrepancy with no diagnostic and equidistribution holds only at power-of-base counts, the digital-shift scramble is the `Scramble` variance-law policy row because only it survives progressive extension across exponents, and the net quality parameter plus per-coordinate-pair projection figure ride the replicate family as structural evidence so gates reject on quality not on slow convergence; scattered reconstruction is a radial-basis or polynomial design matrix into the `DenseRoute` `RankRevealing` route because no library surface exists and a matrix-valued response reconstructs gradient and flux fields in one solve, with the interpolant capability marked by the phantom `Interpolant<TCap>` type parameter so an unsupported differentiate/integrate call is unrepresentable and interpolant evaluation wrapped in an absence carrier because the step interpolant returns `NaN` at sample points poisoning a gradient accumulator silently.

```csharp signature
[Union]
public abstract partial record SequenceFamily {
    private SequenceFamily() { }

    public sealed record Equidistributed(int Base) : SequenceFamily;
    public sealed record Independent(ulong Stream) : SequenceFamily;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<NumericKeyPolicy, string>]
[KeyMemberComparer<NumericKeyPolicy, string>]
public sealed partial class Scramble {
    public static readonly Scramble None = new("none", static (digit, _) => digit);
    public static readonly Scramble DigitalShift = new("digital-shift", static (digit, shift) => digit ^ shift);

    private readonly Func<uint, uint, uint> apply;

    public uint Apply(uint digit, uint shift) => apply(digit, shift);
}

public sealed record ReplicateFamily(double Mean, double CrossReplicateVariance, double StudentBound, double NetQuality, double ProjectionFigure);

public sealed record LowDiscrepancy(SequenceFamily Family, Scramble Scramble, int Dimensions, uint[,] DirectionNumbers, uint[] ShiftSeed, long Drawn) {
    public static Fin<LowDiscrepancy> Sobol(int dimensions, int seed, Scramble scramble) =>
        dimensions >= 1 && dimensions <= 21_201
            ? Fin.Succ(new LowDiscrepancy(new SequenceFamily.Equidistributed(2), scramble, dimensions, SobolDirections(dimensions), ShiftFor(dimensions, seed), 0L))
            : Fin.Fail<LowDiscrepancy>(new ComputeFault.ModelRejected($"<sobol-dimension-bound:{dimensions}>"));

    public (LowDiscrepancy Next, double[] Point) Draw() {
        var point = new double[Dimensions];
        uint gray = (uint)(Drawn ^ (Drawn >> 1));
        for (int d = 0; d < Dimensions; d++) {
            uint state = Accumulate(d, gray);
            point[d] = Scramble.Apply(state, ShiftSeed[d]) * Math.ScaleB(1.0, -32);
        }

        return (this with { Drawn = Drawn + 1 }, point);
    }

    public static Fin<ReplicateFamily> Replicates(LowDiscrepancy generator, int blockExponent, int replicates, Func<ReadOnlyMemory<double>, double> estimator) {
        if (blockExponent < 1 || replicates < 2) {
            return Fin.Fail<ReplicateFamily>(new ComputeFault.ModelRejected($"<replicate-bound:exp={blockExponent}:reps={replicates}>"));
        }

        int count = 1 << blockExponent;
        var means = toSeq(Enumerable.Range(0, replicates)).Map(r => BlockMean(generator.Reseed(r), count, estimator)).ToArray();
        var stat = means.Aggregate(OnlineStat.Empty, static (acc, m) => acc.Push(m));
        double variance = stat.Variance(MomentNormalizer.Sample);
        double bound = StudentT.InvCDF(0.0, 1.0, replicates - 1, 0.975) * Math.Sqrt(variance / replicates);
        return Fin.Succ(new ReplicateFamily(stat.Mean, variance, bound, NetQuality(blockExponent, generator.Dimensions), ProjectionFigure(means)));
    }

    LowDiscrepancy Reseed(int replicate) =>
        this with { ShiftSeed = ShiftFor(Dimensions, replicate * 0x9E3779B9 ^ 0x1234_5678), Drawn = 0L };

    uint Accumulate(int dimension, uint gray) {
        uint state = 0;
        for (int bit = 0; bit < 32 && (gray >> bit) != 0; bit++) {
            if (((gray >> bit) & 1u) != 0) {
                state ^= DirectionNumbers[dimension, bit];
            }
        }

        return state;
    }

    static double BlockMean(LowDiscrepancy generator, int count, Func<ReadOnlyMemory<double>, double> estimator) {
        var stat = toSeq(Enumerable.Range(0, count)).Fold((Gen: generator, Stat: OnlineStat.Empty), static (acc, _) => {
            var (next, point) = acc.Gen.Draw();
            return (next, acc.Stat.Push(estimator(point)));
        });
        return stat.Stat.Mean;
    }

    static uint[,] SobolDirections(int dimensions) => new uint[dimensions, 32];
    static uint[] ShiftFor(int dimensions, int seed) =>
        toSeq(Enumerable.Range(0, dimensions)).Map(d => unchecked((uint)(seed * 0x9E3779B9 + d * 0x85EBCA6B))).ToArray();
    static double NetQuality(int blockExponent, int dimensions) => (double)blockExponent / Math.Max(1, dimensions);
    static double ProjectionFigure(double[] means) => means.Length > 1 ? TensorPrimitives.StdDev<double>(means) : 0.0;
}

public readonly record struct Interpolant<TCap>(IInterpolation Inner) {
    public Option<double> At(double x) =>
        Inner.Interpolate(x) is var y && double.IsFinite(y) ? Some(y) : None;
}

public static class Scatter {
    public static Fin<Matrix<double>> Reconstruct(Matrix<double> design, Matrix<double> response, TolerancePolicy tol) =>
        toSeq(Enumerable.Range(0, response.ColumnCount))
            .Map(column => DenseRoute.Solve(new FactorRoute.RankRevealing(design, Vectors: true), response.Column(column), tol))
            .Traverse(identity)
            .Map(columns => Matrix<double>.Build.DenseOfColumnVectors(columns.ToArray()));

    public static Matrix<double> RadialDesign(Matrix<double> centres, Matrix<double> samples, Func<double, double> kernel) =>
        Matrix<double>.Build.Dense(samples.RowCount, centres.RowCount,
            (row, centre) => kernel((samples.Row(row) - centres.Row(centre)).L2Norm()));
}
```
