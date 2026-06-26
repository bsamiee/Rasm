# [COMPUTE_SAMPLING]

Rasm.Compute owned-build numeric lane for quasi-Monte-Carlo sampling and scattered reconstruction: the kernels with no library surface — the seed-explicit state-serializable Sobol/Halton low-discrepancy sampler carrying the equidistributed-versus-independent discriminant as a type axis, and the radial-basis design-matrix `Scatter` reconstruction into the rank-revealing route — built and gated in-house, every estimate leaving as a replicate family carrying its spread. The Sobol leg drives the real `JoeKuo` direction-number recurrence over the embedded `new-joe-kuo-6.21201` primitive-polynomial set, the Halton leg drives the scrambled base-`b` radical inverse over the d-th prime, so the `SequenceFamily.Equidistributed(int Base)` case carries both qmc families on its `Base` discriminant (`Base == 2` Sobol, `Base > 2` Halton) and the all-zero direction stub is the deleted form. The lane is host-local and crosses no wire. In-place numeric kernels — the `LowDiscrepancy` direction-number gray-code draw loop, the `JoeKuo.Directions` recurrence, the `RadicalInverse` digit walk, and the `OnlineStat` Welford increment — are this page's statement exemption.

## [01]-[INDEX]

- [01]-[OWNED_BUILDS]: owned Sobol (Joe-Kuo direction numbers) / Halton (prime radical inverse) low-discrepancy sampler; radial-basis scatter reconstruction.

## [02]-[OWNED_BUILDS]

- Owner: the owned-build lane with no library surface — `LowDiscrepancy` the owned seed-explicit Sobol/Halton sampling family carrying the equidistributed-versus-independent discriminant as a type axis, `JoeKuo` the direction-number recurrence and prime-base table owner over the embedded `new-joe-kuo-6.21201` resource, and `Scatter` the radial-basis design-matrix reconstruction into the rank-revealing route.
- Cases: `SequenceFamily` `[Union]` cases `Equidistributed(int Base)` (`Base == 2` Sobol over `JoeKuo.Directions`, `Base > 2` Halton over `RadicalInverse` against the d-th prime) · `Independent(ulong Stream)` (the digital-shift-scrambled counter stream) (2); `Scramble` `[SmartEnum<string>]` cases none · digital-shift (2).
- Entry: `public static Fin<LowDiscrepancy> Sobol(int dimensions, int seed, Scramble scramble)` and `public static Fin<LowDiscrepancy> Halton(int dimensions, int seed, Scramble scramble)` are the two qmc factories over the one `LowDiscrepancy` carrier (each gated on its own dimension bound — `JoeKuo.MaxDimensions` for Sobol, `JoeKuo.Primes.Length` for Halton); `public (LowDiscrepancy Next, double[] Point) Draw()` is the one polymorphic draw folding the `SequenceFamily` case; `public static Fin<ReplicateFamily> Replicates(LowDiscrepancy generator, int blockExponent, int replicates, Func<ReadOnlyMemory<double>, double> estimator)` draws the power-of-base replicate family carrying the Student bound; `public static Fin<Matrix<double>> Reconstruct(Matrix<double> design, Matrix<double> response, TolerancePolicy tol)` reconstructs the scattered field through the `RankRevealing` route.
- Auto: `LowDiscrepancy.Draw` folds the `SequenceFamily` case — the `Equidistributed(2)` Sobol case unrolls the gray-code direction-number XOR over the real `JoeKuo.Directions` table under the `Scramble` digital-shift policy, every `Equidistributed(base>2)` Halton case folds the `RadicalInverse` base-`b` digit reversal over the prime base, and `Independent` rides the scrambled stream counter; `JoeKuo.Directions` runs the primitive-polynomial recurrence once per construction (dimension 0 the identity column, each higher dimension folding its seed m-values then the `m[j] = m[j-s] ^ (m[j-s] << s) ^ ⊕ coefficient·m[j-bit]` recurrence to 32-bit direction integers), never the all-zero stub; `Replicates` draws exactly the power-of-base count and returns the replicate family with cross-replicate variance and a Student bound; `Scatter.Reconstruct` builds the radial-basis-plus-polynomial design matrix into `DenseRoute.Solve` on the `RankRevealing` route so a matrix-valued response reconstructs gradient and flux in one solve, wrapping evaluation in the interpolant absence carrier.
- Receipt: the sampler returns a `ReplicateFamily(Mean, CrossReplicateVariance, StudentBound, NetQuality, ProjectionFigure)` because a single equidistributed estimate carries no recoverable spread.
- Packages: MathNet.Numerics, System.Numerics.Tensors, CommunityToolkit.HighPerformance, LanguageExt.Core, BCL inbox
- Growth: a new sampling discriminant is one `SequenceFamily` case; a new scramble is one `Scramble` row; a higher-quality scramble (Owen nested-uniform scrambling) is one more `Scramble` row binding its bit-permutation delegate, never a parallel sampler; zero new surface.
- Boundary: stochastic samples draw seed-explicit over a state-serializable generator (the `JoeKuo` direction-number table plus the draw counter, both `record`-carried) for checkpoint-resume because the default thread-entropy source and the length-2048 parallel block fill are non-deterministic regardless of seeding; the low-discrepancy family is built in the owned lane because no library surface exists (MathNet exposes no Sobol/Halton, only `SystemRandomSource`), so the Sobol leg owns the genuine Joe-Kuo recurrence (`JoeKuo.Directions` over the embedded `new-joe-kuo-6.21201` primitive-polynomial set, never an all-zero direction table that collapses every point to the origin) and the Halton leg owns the scrambled base-`b` radical inverse over the d-th prime; the independent-versus-equidistributed discriminant is the `SequenceFamily` `[Union]` case axis never a bool because variance law/error bars/convergence rate fork on it and the state shapes do not unify (Sobol is bitwise over direction integers, Halton is base-`b` digit reversal), and the Sobol-versus-Halton split rides the `Equidistributed.Base` field rather than a second union case because both are equidistributed nets keyed by their radix; the block exponent is accepted at the draw entrypoint never a free count because non-power prefixes and dropped origins degrade discrepancy with no diagnostic and equidistribution holds only at power-of-base counts; the digital-shift scramble is the `Scramble` variance-law policy row because only it survives progressive extension across exponents, and the net quality parameter plus per-coordinate-pair projection figure ride the replicate family as structural evidence so gates reject on quality not on slow convergence; scattered reconstruction is a radial-basis or polynomial design matrix into the `DenseRoute` `RankRevealing` route because no library surface exists and a matrix-valued response reconstructs gradient and flux fields in one solve, with the interpolant capability marked by the phantom `Interpolant<TCap>` type parameter so an unsupported differentiate/integrate call is unrepresentable and interpolant evaluation wrapped in an absence carrier because the step interpolant returns `NaN` at sample points poisoning a gradient accumulator silently.

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
        dimensions >= 1 && dimensions <= JoeKuo.MaxDimensions
            ? Fin.Succ(new LowDiscrepancy(new SequenceFamily.Equidistributed(2), scramble, dimensions, JoeKuo.Directions(dimensions), ShiftFor(dimensions, seed), 0L))
            : Fin.Fail<LowDiscrepancy>(new ComputeFault.ModelRejected($"<sobol-dimension-bound:{dimensions}>"));

    public static Fin<LowDiscrepancy> Halton(int dimensions, int seed, Scramble scramble) =>
        dimensions >= 1 && dimensions <= JoeKuo.Primes.Length
            ? Fin.Succ(new LowDiscrepancy(new SequenceFamily.Equidistributed(JoeKuo.Primes[dimensions - 1]), scramble, dimensions, EmptyDirections, ShiftFor(dimensions, seed), 0L))
            : Fin.Fail<LowDiscrepancy>(new ComputeFault.ModelRejected($"<halton-dimension-bound:{dimensions}>"));

    // One Draw fold over the SequenceFamily case axis: the Equidistributed(2) Sobol case unrolls the
    // gray-code direction-number XOR, every Equidistributed(base>2) Halton case folds the scrambled
    // radical inverse over the d-th prime, and Independent rides the same digital-shift over a counter
    // re-keyed by the stream. The discriminant is the union case, never a bool — the per-coordinate
    // construction does not unify across the families (Sobol is bitwise, Halton is base-b digit-reversal).
    public (LowDiscrepancy Next, double[] Point) Draw() {
        var point = new double[Dimensions];
        uint gray = (uint)(Drawn ^ (Drawn >> 1));
        for (int d = 0; d < Dimensions; d++) {
            point[d] = Family switch {
                SequenceFamily.Equidistributed { Base: 2 } =>
                    Scramble.Apply(Accumulate(d, gray), ShiftSeed[d]) * Math.ScaleB(1.0, -32),
                SequenceFamily.Equidistributed equidistributed =>
                    RadicalInverse(checked((ulong)Drawn) + 1UL, equidistributed.Base, ShiftSeed[d]),
                SequenceFamily.Independent independent =>
                    Scramble.Apply(unchecked((uint)(independent.Stream * 0x2545F4914F6CDD1DUL + (ulong)Drawn)), ShiftSeed[d]) * Math.ScaleB(1.0, -32),
                _ => double.NaN,
            };
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

    static readonly uint[,] EmptyDirections = new uint[0, 0];

    // Base-b scrambled radical inverse for the Halton coordinate: reverse the base-b digits of the index
    // about the radix point under the per-coordinate digital shift, summing b^-k contributions. Bounded by
    // the 1/u32-resolution shift, so the inversion terminates at the index's digit count, never an
    // unbounded loop. A NaN-free closed form — the step interpolant pitfall the Boundary names is for the
    // reconstruction kernel, not this draw.
    static double RadicalInverse(ulong index, int radix, uint shift) {
        double inverse = 0.0;
        double fraction = 1.0 / radix;
        ulong cursor = index;
        while (cursor > 0UL) {
            ulong digit = cursor % (ulong)radix;
            ulong scrambled = (digit + shift) % (ulong)radix;
            inverse += scrambled * fraction;
            cursor /= (ulong)radix;
            fraction /= radix;
        }

        return inverse;
    }

    static uint[] ShiftFor(int dimensions, int seed) =>
        toSeq(Enumerable.Range(0, dimensions)).Map(d => unchecked((uint)(seed * 0x9E3779B9 + d * 0x85EBCA6B))).ToArray();
    static double NetQuality(int blockExponent, int dimensions) => (double)blockExponent / Math.Max(1, dimensions);
    static double ProjectionFigure(double[] means) => means.Length > 1 ? TensorPrimitives.StdDev<double>(means) : 0.0;
}

// The Joe-Kuo direction-number recurrence and the Halton prime base table — the owned construction the
// no-MathNet-surface Boundary justifies. The primitive-polynomial degrees `s`, the polynomial coefficient
// encodings `a`, and the seed direction integers `m` ship as an embedded `new-joe-kuo-6.21201` resource
// (the published 21,201-dimension Joe-Kuo set, MIT-equivalent BSD). Dimension 0 is the identity column
// (v[k] = 1 << (31-k)); every higher dimension folds its initial m-values then the
// m[j] = m[j-s] ^ (a-bits XOR over m[j-1..j-s+1]) << shifts recurrence, scaling each m to a 32-bit
// direction integer v[k] = m[k] << (31-k). The recurrence runs once per generator construction, never
// per draw.
public static class JoeKuo {
    public const int MaxDimensions = 21_201;
    const int Bits = 32;

    // The first MaxDimensions primes — the Halton coordinate bases (prime 2 is dimension 1). Shipped as the
    // same embedded resource, read once into a frozen array so a Halton coordinate reads its base by index.
    public static readonly int[] Primes = LoadPrimes();

    static readonly (int Degree, uint Coefficients, uint[] Seeds)[] Polynomials = LoadPolynomials();

    public static uint[,] Directions(int dimensions) {
        var v = new uint[dimensions, Bits];
        for (int k = 0; k < Bits; k++) {
            v[0, k] = 1u << (Bits - 1 - k);
        }

        for (int d = 1; d < dimensions; d++) {
            var (degree, coefficients, seeds) = Polynomials[d - 1];
            var m = new uint[Bits];
            for (int i = 0; i < degree && i < Bits; i++) {
                m[i] = seeds[i];
                v[d, i] = m[i] << (Bits - 1 - i);
            }

            for (int j = degree; j < Bits; j++) {
                uint next = m[j - degree] ^ (m[j - degree] << degree);
                for (int bit = 1; bit < degree; bit++) {
                    if (((coefficients >> (degree - 1 - bit)) & 1u) != 0u) {
                        next ^= m[j - bit];
                    }
                }

                m[j] = next;
                v[d, j] = m[j] << (Bits - 1 - j);
            }
        }

        return v;
    }

    static (int, uint, uint[])[] LoadPolynomials() => EmbeddedResource.JoeKuoPolynomials();
    static int[] LoadPrimes() => EmbeddedResource.FirstPrimes(MaxDimensions);
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
