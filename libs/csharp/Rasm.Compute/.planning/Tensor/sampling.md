# [COMPUTE_SAMPLING]

Rasm.Compute owned-build numeric lane for quasi-Monte-Carlo sampling and scattered reconstruction — the kernels with no library surface, built and gated in-house, every estimate leaving as a replicate family carrying its spread. `MathNet` exposes no Sobol/Halton (only `SystemRandomSource`) and no scattered radial-basis solver, so the seed-explicit state-serializable `LowDiscrepancy` carrier, the `JoeKuo` direction-number recurrence, the prime-base Halton radical inverse, the rank-stratified Latin-hypercube design, and the `Scatter` radial-basis-plus-polynomial reconstruction are composed from the rails rather than imported.

The `LowDiscrepancy` carrier folds the `SequenceFamily` discriminant as a type axis because variance law, error bars, and convergence rate fork on the family and the state shapes do not unify: `Equidistributed(Base)` carries the two equidistributed nets on its radix (`Base == 2` selects the Sobol direction-number construction over `JoeKuo.Directions`, `Base > 2` selects Halton whose per-dimension base is `JoeKuo.Primes[d]`), and `Independent(Stream)` carries the counter-stream pseudo-random leg. The `Scramble` randomization policy applies uniformly across both the binary Sobol leg and the base-`b` Halton digit walk so a `None` policy genuinely disables both, `DigitalShift` is the progressive Cranley-Patterson randomization, and `Owen` is the gold-standard nested-uniform scramble. `Replicates` draws independently-randomized realizations of the deterministic net and reports the cross-replicate variance, the Student bound, and the genuine Warnock L2 star-discrepancy plus worst-2D-projection figures so a gate rejects on net quality, not on slow convergence. `Scatter` builds the augmented radial-basis-plus-polynomial design and reconstructs a matrix-valued field through one held rank-revealing SVD into the `Tensor/blas#DENSE_ALGEBRA` route, the interpolant capability lifted to a compile-time type parameter so an unsupported differentiate/integrate call is unrepresentable.

The lane is host-local and crosses no wire. Statement exemption: the `JoeKuo` direction-number recurrence, the gray-code Sobol draw, the base-`b` radical-inverse digit walk, the `SplitMix64` counter, the `Owen` bit-reversal, the Warnock discrepancy kernels, the monomial-basis enumeration, the Latin-hypercube rank stratification, and the `OnlineStat` Welford increment are this page's named in-place numeric kernels.

## [01]-[INDEX]

- [02]-[OWNED_BUILDS]: owned Sobol (Joe-Kuo direction numbers) / Halton (prime radical inverse) / Latin-hypercube / pseudo-random sampler over one `LowDiscrepancy` carrier; RQMC replicate family with Warnock-discrepancy net quality.
- [03]-[SCATTER_RECONSTRUCTION]: capability-typed interpolant; radial-basis-plus-polynomial design into the held rank-revealing SVD; the `RbfKernel` vocabulary and the `RbfFit` field.

## [02]-[OWNED_BUILDS]

- Owner: the owned-build sampler lane with no library surface — `LowDiscrepancy` the seed-explicit state-serializable carrier folding the `SequenceFamily` discriminant over a per-construction `JoeKuo.Directions` table, a per-draw counter, and a per-dimension `ShiftSeed` key vector; `Scramble` the `[SmartEnum<string>]` randomization policy carrying a binary scramble and a base-`b` digit scramble as paired row delegates; `SequenceFamily` the `[Union]` family discriminant; `ReplicateFamily` the RQMC estimate carrier; `JoeKuo` the canonical scaled direction-number recurrence, the embedded `new-joe-kuo-6.21201` primitive-polynomial owner, and the sieved Halton prime-base table.
- Cases: `SequenceFamily` `[Union]` cases `Equidistributed(int Base)` (`Base == 2` Sobol over `JoeKuo.Directions`, `Base > 2` Halton over `RadicalInverse` against the per-dimension prime) · `Independent(ulong Stream)` (the `SplitMix64` counter stream keyed per dimension) (2); `Scramble` `[SmartEnum<string>]` rows none · digital-shift · owen (3).
- Entry: `public static Fin<LowDiscrepancy> Sobol(int dimensions, int seed, Scramble scramble)` and `public static Fin<LowDiscrepancy> Halton(int dimensions, int seed, Scramble scramble)` are the two qmc factories over the one `LowDiscrepancy` carrier, each gated on its own dimension bound (`JoeKuo.MaxDimensions` for Sobol, `JoeKuo.Primes.Length` for Halton); `public (LowDiscrepancy Next, double[] Point) Draw()` is the one polymorphic draw folding the `SequenceFamily` case through the generated total `Switch`; `public double[][] Net(int count)` realizes a full net for a quality probe or a stratification base; `public static Fin<double[][]> LatinHypercube(int dimensions, int count, int seed, Scramble scramble)` is the rank-stratified joint design both the `Solver/sweep#SWEEP_AND_BUDGET` `latin-hypercube` row and the `Solver/uncertainty#UNCERTAINTY_LANE` Latin-hypercube-MC row compose; `public static Fin<ReplicateFamily> Replicates(LowDiscrepancy generator, int blockExponent, int replicates, Func<ReadOnlyMemory<double>, double> estimator)` draws the power-of-base replicate family carrying the Student bound and the Warnock net-quality figures.
- Auto: `Draw` folds the `SequenceFamily` case through the Thinktecture-generated `Switch` (no runtime `_` arm) — the `Equidistributed` arm dispatches `SobolPoint` at `Base == 2` (the gray-code direction-number XOR over the real `JoeKuo.Directions` table under the `Scramble` binary leg) and `HaltonPoint` at `Base > 2` (the base-`b` radical inverse against `JoeKuo.Primes[d]` per dimension under the `Scramble` digit leg), and the `Independent` arm runs `FillIndependent` (the `SplitMix64` counter finalizer keyed per `(Stream, Drawn, dimension, ShiftSeed[d])`); `JoeKuo.Directions` runs the canonical recurrence once per construction (dimension 0 the identity column `v[k] = 2^(31−k)`, every higher dimension scaling its seed `m`-values then folding `v[j] = v[j−s] ^ (v[j−s] >> s) ^ ⊕ a_k·v[j−k]` over the scaled 32-bit direction integers); `LatinHypercube` draws one joint Sobol net and rank-stratifies each dimension into one point per stratum; `Replicates` draws exactly `2^blockExponent` points per replicate, folds the estimator through `OnlineStat`, and returns the cross-replicate variance, the Student bound, and the Warnock L2 star-discrepancy plus worst-2D-projection of one realized net.
- Receipt: the sampler returns a `ReplicateFamily(Mean, CrossReplicateVariance, StudentBound, StarDiscrepancy, WorstProjection)` because a single equidistributed estimate carries no recoverable spread, and the net-quality fields make a gate reject on discrepancy rather than on slow convergence.
- Packages: MathNet.Numerics, Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox
- Growth: a new sampling family is one `SequenceFamily` case plus one `Fill*` kernel; a new scramble is one `Scramble` row binding its binary and digit delegates; a new net-quality figure is one field on `ReplicateFamily` plus one kernel; zero new surface — a `SobolGenerator`/`HaltonGenerator`/`LatinHypercubeSampler` sibling family is the rejected form collapsed onto the one `LowDiscrepancy` carrier, and a per-axis 1-D space-filling sequence is the rejected form (a space-filling design is joint across dimensions).
- Boundary: the low-discrepancy family is built in the owned lane because no library surface exists (`MathNet` exposes no Sobol/Halton, only `SystemRandomSource`); the Sobol leg owns the genuine Joe-Kuo recurrence over the embedded `new-joe-kuo-6.21201` primitive-polynomial set, NEVER an all-zero direction table that collapses every point to the origin and never the unscaled-`m` recurrence that omits the per-term `<< k` bit-scaling on the middle `a_k·m[j−k]` coefficients (which yields wrong direction numbers and a broken net that still looks plausible — the canonical form folds the scaled 32-bit direction integers `v` directly so `v[j] = v[j−s] ^ (v[j−s] >> s) ^ ⊕ a_k·v[j−k]`); the Halton leg reads its base per dimension from `JoeKuo.Primes[d]` (dimension 0 → 2) because a single shared base across all dimensions collapses every Halton coordinate onto one radical-inverse sequence — the `Equidistributed.Base` field is the family discriminant (`2` Sobol, any prime `> 2` Halton), never the per-coordinate radix; the `Scramble` policy is applied uniformly across both legs so `Scramble.None` genuinely disables both the binary XOR and the base-`b` digit shift, never the hardcoded radical-inverse `(digit + shift) % radix` that ignores the policy and shifts even under `None`; the `Independent` stream is a `SplitMix64` counter finalizer keyed per `(Stream, Drawn, dimension)` because a per-dimension constant XORed over one shared counter value leaves every coordinate near-identical across dimensions; the independent-versus-equidistributed discriminant is the `SequenceFamily` `[Union]` case axis never a bool because variance law/error bars/convergence rate fork on it and the state shapes do not unify (Sobol is bitwise over direction integers, Halton is base-`b` digit reversal, Independent is a counter hash), and the Sobol-versus-Halton split rides the `Equidistributed.Base` radix rather than a second union case because both are equidistributed nets keyed by their radix; the block exponent is accepted at the draw entrypoint never a free count because non-power prefixes and dropped origins degrade discrepancy with no diagnostic and equidistribution holds only at power-of-base counts; stochastic samples draw seed-explicit over a state-serializable generator (the `JoeKuo` direction-number table plus the `Drawn` counter and the `ShiftSeed` key vector, all `record`-carried) for checkpoint-resume because the default thread-entropy source and a parallel block fill are non-deterministic regardless of seeding; the `digital-shift` scramble survives progressive extension across exponents and the `owen` nested-uniform scramble is the higher-quality randomization the `Growth` axis names, added as one more `Scramble` row binding its bit-reversal hash rather than a parallel sampler; the net-quality figures are the genuine Warnock L2 star-discrepancy and the worst-2D-coordinate-pair projection discrepancy of one realized net because a `blockExponent / dimensions` ratio is a decorative figure that gates nothing, and the worst-projection figure is load-bearing because a Sobol net can be uniform in full dimension yet degenerate on a 2-D projection; the Latin-hypercube design rank-stratifies a JOINT low-discrepancy draw into one point per stratum per dimension and a fresh per-axis 1-D sequence Cartesian-producted is the deleted form because it inflates the point count combinatorially while destroying the joint low-discrepancy QMC convergence depends on; the embedded primitive-polynomial resource and the sieved prime-base table are loaded once at `JoeKuo` type initialization (a missing resource is a fatal construction fault, the load boundary), never per draw.

```csharp signature
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class Scramble {
    public static readonly Scramble None = new("none",
        bits: static (value, _) => value,
        digit: static (digit, _, _, _) => digit);
    public static readonly Scramble DigitalShift = new("digital-shift",
        bits: static (value, key) => value ^ key,
        digit: static (digit, key, radix, _) => (uint)(((ulong)digit + key) % (ulong)radix));
    public static readonly Scramble Owen = new("owen",
        bits: static (value, key) => OwenNestedUniform(value, key),
        digit: static (digit, key, radix, position) => RandomLinearDigit(digit, key, radix, position));

    private readonly Func<uint, uint, uint> bits;
    private readonly Func<uint, uint, int, int, uint> digit;

    // The binary leg scrambles the whole 32-bit fixed-point Sobol coordinate; the base-b leg scrambles each
    // radix-b Halton digit in place at its position. None genuinely disables BOTH legs — the prior hardcoded
    // radical-inverse (digit + shift) % radix that shifted even under None is the deleted form.
    public uint Bits(uint value, uint key) => bits(value, key);
    public uint Digit(uint value, uint key, int radix, int position) => digit(value, key, radix, position);

    // Hash-based Owen nested-uniform scramble: reverse, hash-perturb from the most-significant bit, reverse
    // back — the gold-standard randomization whose RMSE decays one half-order faster than the digital shift on
    // a smooth integrand.
    static uint OwenNestedUniform(uint value, uint key) {
        unchecked {
            uint x = ReverseBits32(value);
            x ^= x * 0x3D20ADEAu;
            x += key;
            x *= (key >> 16) | 1u;
            x ^= x * 0x05526C56u;
            x ^= x * 0x53A22864u;
            return ReverseBits32(x);
        }
    }

    // Base-b per-position affine digit permutation d -> (a·d + c) mod radix with a coprime to the prime radix,
    // keyed by a SplitMix hash of (key, position) so each digit position draws an independent permutation — the
    // base-b analog of nested scrambling, stronger than a single shared digit shift.
    static uint RandomLinearDigit(uint digit, uint key, int radix, int position) {
        ulong h = SplitMix64(((ulong)key << 32) ^ (uint)position);
        uint a = (uint)(1UL + h % (ulong)(radix - 1));
        uint c = (uint)((h >> 32) % (ulong)radix);
        return (uint)(((ulong)a * digit + c) % (ulong)radix);
    }

    static uint ReverseBits32(uint x) {
        x = (x >> 16) | (x << 16);
        x = ((x & 0xFF00FF00u) >> 8) | ((x & 0x00FF00FFu) << 8);
        x = ((x & 0xF0F0F0F0u) >> 4) | ((x & 0x0F0F0F0Fu) << 4);
        x = ((x & 0xCCCCCCCCu) >> 2) | ((x & 0x33333333u) << 2);
        return ((x & 0xAAAAAAAAu) >> 1) | ((x & 0x55555555u) << 1);
    }

    // The codebase compiles with CheckForOverflowUnderflow, so the wrapping hash arithmetic is explicitly
    // unchecked — the multiplicative diffusion overflows ulong by design and a checked context would fault it.
    internal static ulong SplitMix64(ulong z) {
        unchecked {
            z += 0x9E3779B97F4A7C15UL;
            z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
            z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
            return z ^ (z >> 31);
        }
    }
}

[Union]
public abstract partial record SequenceFamily {
    private SequenceFamily() { }

    public sealed record Equidistributed(int Base) : SequenceFamily;
    public sealed record Independent(ulong Stream) : SequenceFamily;
}

public sealed record ReplicateFamily(double Mean, double CrossReplicateVariance, double StudentBound, double StarDiscrepancy, double WorstProjection);

public sealed record LowDiscrepancy(SequenceFamily Family, Scramble Scramble, int Dimensions, uint[,] DirectionNumbers, uint[] ShiftSeed, long Drawn) {
    public static Fin<LowDiscrepancy> Sobol(int dimensions, int seed, Scramble scramble) =>
        dimensions >= 1 && dimensions <= JoeKuo.MaxDimensions
            ? Fin.Succ(new LowDiscrepancy(new SequenceFamily.Equidistributed(2), scramble, dimensions, JoeKuo.Directions(dimensions), ShiftFor(dimensions, seed), 0L))
            : Fin.Fail<LowDiscrepancy>(new ComputeFault.ModelRejected($"<sobol-dimension-bound:{dimensions}>"));

    // The Base field marks the family only — 3 is a prime > 2 selecting Halton; the per-dimension radix is
    // always JoeKuo.Primes[d] (dimension 0 -> 2), so the family discriminant never collides with Sobol's
    // Base == 2 and never doubles as the shared per-coordinate base the broken form used.
    public static Fin<LowDiscrepancy> Halton(int dimensions, int seed, Scramble scramble) =>
        dimensions >= 1 && dimensions <= JoeKuo.Primes.Length
            ? Fin.Succ(new LowDiscrepancy(new SequenceFamily.Equidistributed(3), scramble, dimensions, EmptyDirections, ShiftFor(dimensions, seed), 0L))
            : Fin.Fail<LowDiscrepancy>(new ComputeFault.ModelRejected($"<halton-dimension-bound:{dimensions}>"));

    // One Draw fold over the SequenceFamily case axis through the generated total Switch — no runtime _ arm.
    // The Base radix splits Sobol (bitwise direction-number XOR) from Halton (base-b digit reversal) inside
    // the one Equidistributed case because both are equidistributed nets keyed by their radix; Independent
    // rides the counter hash. The discriminant is the union case, never a bool — the per-coordinate
    // construction does not unify across the families.
    public (LowDiscrepancy Next, double[] Point) Draw() {
        double[] point = Family.Switch(
            state: this,
            equidistributed: static (self, e) => e.Base == 2 ? self.SobolPoint() : self.HaltonPoint(),
            independent: static (self, i) => self.FillIndependent(i.Stream));
        return (this with { Drawn = Drawn + 1 }, point);
    }

    public double[][] Net(int count) {
        var rows = new double[count][];
        var generator = this;
        for (int i = 0; i < count; i++) {
            (generator, rows[i]) = generator.Draw();
        }

        return rows;
    }

    // Latin hypercube as the rank-stratified joint QMC design: draw one joint Sobol net, then per dimension
    // map each point's rank into its own stratum [rank, rank+1)/count with the QMC value as the in-stratum
    // offset — one point per stratum per dimension while inheriting the joint low-discrepancy of the net. A
    // per-axis 1-D sequence Cartesian-producted is the deleted form (it inflates the point count and destroys
    // the joint property the variance reduction depends on).
    public static Fin<double[][]> LatinHypercube(int dimensions, int count, int seed, Scramble scramble) =>
        dimensions >= 1 && count >= 1
            ? Sobol(dimensions, seed, scramble).Map(generator => Stratify(generator.Net(count)))
            : Fin.Fail<double[][]>(new ComputeFault.ModelRejected($"<lhs-bound:dims={dimensions}:count={count}>"));

    public static Fin<ReplicateFamily> Replicates(LowDiscrepancy generator, int blockExponent, int replicates, Func<ReadOnlyMemory<double>, double> estimator) {
        if (blockExponent < 1 || replicates < 2) {
            return Fin.Fail<ReplicateFamily>(new ComputeFault.ModelRejected($"<replicate-bound:exp={blockExponent}:reps={replicates}>"));
        }

        int count = 1 << blockExponent;
        var stat = toSeq(Enumerable.Range(0, replicates))
            .Map(r => BlockMean(generator.Reseed(r), count, estimator))
            .Fold(OnlineStat.Empty, static (acc, m) => acc.Push(m));
        double variance = stat.Variance(MomentNormalizer.Sample);
        double bound = StudentT.InvCDF(0.0, 1.0, replicates - 1, 0.975) * Math.Sqrt(variance / replicates);
        double[][] net = generator.Reseed(0).Net(count);
        return Fin.Succ(new ReplicateFamily(stat.Mean, variance, bound, StarDiscrepancyL2(net), WorstProjection(net)));
    }

    double[] SobolPoint() {
        var point = new double[Dimensions];
        uint gray = unchecked((uint)(Drawn ^ (Drawn >> 1)));
        for (int d = 0; d < Dimensions; d++) {
            uint state = 0u;
            for (int bit = 0; bit < JoeKuo.Bits && (gray >> bit) != 0u; bit++) {
                if (((gray >> bit) & 1u) != 0u) { state ^= DirectionNumbers[d, bit]; }
            }

            point[d] = Scramble.Bits(state, ShiftSeed[d]) * Math.ScaleB(1.0, -32);
        }

        return point;
    }

    double[] HaltonPoint() {
        var point = new double[Dimensions];
        ulong index = unchecked((ulong)Drawn) + 1UL;
        for (int d = 0; d < Dimensions; d++) {
            point[d] = RadicalInverse(index, JoeKuo.Primes[d], ShiftSeed[d], Scramble);
        }

        return point;
    }

    double[] FillIndependent(ulong stream) {
        var point = new double[Dimensions];
        for (int d = 0; d < Dimensions; d++) {
            ulong key = unchecked(stream ^ ((ulong)Drawn * 0xD1B54A32D192ED03UL) ^ ((ulong)(uint)d * 0x9E3779B97F4A7C15UL) ^ ShiftSeed[d]);
            point[d] = (Scramble.SplitMix64(key) >> 11) * Math.ScaleB(1.0, -53);
        }

        return point;
    }

    LowDiscrepancy Reseed(int replicate) =>
        this with { ShiftSeed = ShiftFor(Dimensions, unchecked(0x5DEECE66 + replicate)), Drawn = 0L };

    static double BlockMean(LowDiscrepancy generator, int count, Func<ReadOnlyMemory<double>, double> estimator) =>
        toSeq(Enumerable.Range(0, count)).Fold(
            (Gen: generator, Stat: OnlineStat.Empty),
            static (acc, _) => {
                var (next, point) = acc.Gen.Draw();
                return (next, acc.Stat.Push(estimator(point)));
            }).Stat.Mean;

    // Base-b scrambled radical inverse for one Halton coordinate: reverse the base-b digits of the index about
    // the radix point under the per-position digit scramble, summing b^-k contributions. The inversion
    // terminates at the index's digit count, never an unbounded loop. A NaN-free closed form — the step
    // interpolant pitfall the [03] Boundary names is for the reconstruction kernel, not this draw.
    static double RadicalInverse(ulong index, int radix, uint key, Scramble scramble) {
        double inverse = 0.0;
        double fraction = 1.0 / radix;
        ulong cursor = index;
        int position = 0;
        while (cursor > 0UL) {
            uint digit = (uint)(cursor % (ulong)radix);
            inverse += scramble.Digit(digit, key, radix, position) * fraction;
            cursor /= (ulong)radix;
            fraction /= radix;
            position++;
        }

        return inverse;
    }

    static double[][] Stratify(double[][] net) {
        int count = net.Length;
        int dims = net[0].Length;
        var stratified = net.Select(static row => new double[row.Length]).ToArray();
        for (int d = 0; d < dims; d++) {
            int[] order = Enumerable.Range(0, count).OrderBy(i => net[i][d]).ToArray();
            for (int rank = 0; rank < count; rank++) {
                int i = order[rank];
                stratified[i][d] = (rank + net[i][d]) / count;
            }
        }

        return stratified;
    }

    // Warnock L2 star-discrepancy of the realized net — the genuine net-quality signal a gate reads (lower is
    // better), O(N^2·d): (1/3)^d − 2^(1−d)/N·Σ_i Π_k(1−x_ik^2) + 1/N^2·Σ_i Σ_j Π_k(1−max(x_ik,x_jk)).
    static double StarDiscrepancyL2(double[][] net) {
        int n = net.Length, d = net[0].Length;
        double single = 0.0;
        for (int i = 0; i < n; i++) {
            double prod = 1.0;
            for (int k = 0; k < d; k++) { prod *= 1.0 - net[i][k] * net[i][k]; }
            single += prod;
        }

        double pair = 0.0;
        for (int i = 0; i < n; i++) {
            for (int j = 0; j < n; j++) {
                double prod = 1.0;
                for (int k = 0; k < d; k++) { prod *= 1.0 - Math.Max(net[i][k], net[j][k]); }
                pair += prod;
            }
        }

        double term = Math.Pow(1.0 / 3.0, d) - Math.ScaleB(1.0, 1 - d) / n * single + pair / ((double)n * n);
        return Math.Sqrt(Math.Max(0.0, term));
    }

    // Worst 2-D coordinate-pair projection discrepancy — the per-coordinate-pair projection figure: a net can
    // be uniform in full dimension yet degenerate on a 2-D projection, so the gate reads the worst projection.
    static double WorstProjection(double[][] net) {
        int d = net[0].Length;
        if (d < 2) { return 0.0; }
        double worst = 0.0;
        for (int a = 0; a < d; a++) {
            for (int b = a + 1; b < d; b++) {
                double[][] projection = net.Select(row => new[] { row[a], row[b] }).ToArray();
                worst = Math.Max(worst, StarDiscrepancyL2(projection));
            }
        }

        return worst;
    }

    static uint[] ShiftFor(int dimensions, int seed) =>
        toSeq(Enumerable.Range(0, dimensions))
            .Map(d => unchecked((uint)(Scramble.SplitMix64(((ulong)(uint)seed << 32) ^ (ulong)(uint)d) >> 32)))
            .ToArray();

    static readonly uint[,] EmptyDirections = new uint[0, 0];
}

// The Joe-Kuo direction-number recurrence, the embedded primitive-polynomial set, and the sieved Halton prime
// bases — the owned construction the no-MathNet-surface Boundary justifies. The primitive-polynomial degrees,
// coefficient encodings, and seed direction integers ship as an embedded new-joe-kuo-6.21201 resource (the
// published 21,201-dimension Joe-Kuo set, MIT-equivalent BSD). The recurrence runs once per generator
// construction, never per draw.
public static class JoeKuo {
    public const int MaxDimensions = 21_201;
    public const int Bits = 32;

    public static readonly int[] Primes = Sieve(MaxDimensions);
    static readonly (int Degree, uint Coefficients, uint[] Seeds)[] Polynomials = LoadPolynomials();

    // Canonical recurrence on the SCALED 32-bit direction integers: dimension 0 is the identity column
    // v[k] = 2^(31−k); every higher dimension scales its seed m-values then folds
    // v[j] = v[j−s] ^ (v[j−s] >> s) ^ (XOR over a_k·v[j−k]). Operating on the scaled v directly is the
    // canonical form; the unscaled-m recurrence that drops the per-term «k scaling yields wrong direction
    // numbers and a broken net that still looks plausible — the deleted form.
    public static uint[,] Directions(int dimensions) {
        var v = new uint[dimensions, Bits];
        for (int k = 0; k < Bits; k++) { v[0, k] = 1u << (Bits - 1 - k); }
        for (int d = 1; d < dimensions; d++) {
            var (degree, coefficients, seeds) = Polynomials[d - 1];
            for (int i = 0; i < degree && i < Bits; i++) { v[d, i] = seeds[i] << (Bits - 1 - i); }
            for (int j = degree; j < Bits; j++) {
                uint value = v[d, j - degree] ^ (v[d, j - degree] >> degree);
                for (int k = 1; k < degree; k++) {
                    if (((coefficients >> (degree - 1 - k)) & 1u) != 0u) { value ^= v[d, j - k]; }
                }

                v[d, j] = value;
            }
        }

        return v;
    }

    // One record per dimension `d s a m_1 … m_s` — degree, the middle coefficient bits, and the seed direction
    // integers; a missing resource is a fatal type-initialization fault (the deployment boundary).
    static (int, uint, uint[])[] LoadPolynomials() {
        using var stream = typeof(JoeKuo).Assembly.GetManifestResourceStream("Rasm.Compute.new-joe-kuo-6.21201.col")
            ?? throw new InvalidOperationException("<joe-kuo-resource-missing>");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd()
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Skip(1)
            .Select(static line => {
                string[] f = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                return (int.Parse(f[1]), uint.Parse(f[2]), f.Skip(3).Select(uint.Parse).ToArray());
            })
            .ToArray();
    }

    // The first MaxDimensions primes by a bounded sieve — the Halton per-dimension bases (dimension d -> the
    // d-th prime, dimension 0 -> 2). p_n ≈ n(ln n + ln ln n) bounds the sieve ceiling.
    static int[] Sieve(int wanted) {
        int ceiling = wanted < 6 ? 15 : (int)(wanted * (Math.Log(wanted) + Math.Log(Math.Log(wanted)))) + 16;
        var composite = new bool[ceiling + 1];
        var primes = new List<int>(wanted);
        for (int n = 2; n <= ceiling && primes.Count < wanted; n++) {
            if (composite[n]) { continue; }
            primes.Add(n);
            for (long multiple = (long)n * n; multiple <= ceiling; multiple += n) { composite[multiple] = true; }
        }

        return primes.ToArray();
    }
}
```

## [03]-[SCATTER_RECONSTRUCTION]

- Owner: the owned-build reconstruction lane with no library surface — `Interpolant<TCap>` the 1-D interpolant capsule lifting the `IInterpolation` runtime support flags to a compile-time capability type parameter; the `IInterpolantCapability` marker family and the `Interpolant` static factory plus capability-gated extension blocks; `RbfKernel` the `[SmartEnum<string>]` radial-basis vocabulary carrying each kernel's radial form and its required polynomial-tail order; `RbfDesign`/`RbfFit` the design and fitted-field carriers; `Scatter` the augmented-design constructor, the held rank-revealing reconstruction, and the field fit.
- Cases: `IInterpolantCapability` markers `Smooth` (differentiable + integrable: cubic/Akima/monotone/linear/step) · `Sampled` (neither: barycentric/Floater-Hormann) (2); `RbfKernel` `[SmartEnum<string>]` rows gaussian · inverse-multiquadric · wendland-c2 (strictly positive-definite, no tail) · multiquadric (constant tail) · polyharmonic-cubic · thin-plate-spline (linear tail, conditionally positive-definite) (6).
- Entry: `public static Interpolant<Smooth> CubicSpline(double[] nodes, double[] values)` and its `Akima`/`Monotone`/`Linear`/`Step` siblings mint the differentiable-integrable tier, `Rational`/`Polynomial` the sample-only tier; `public Option<double> At(double x)` is the absence-carried evaluation present on every tier, `Slope`/`Curvature` are reachable only where `TCap : IDifferentiable` and `Area` only where `TCap : IIntegrable`; `public static Fin<RbfDesign> Design(Matrix<double> centres, Matrix<double> samples, RbfKernel kernel, double shape)` builds the augmented radial-basis-plus-polynomial design; `public static Fin<Matrix<double>> Reconstruct(Matrix<double> design, Matrix<double> response, TolerancePolicy tol)` solves a matrix-valued response through one held SVD into the `RankRevealing` route; `public static Fin<RbfFit> Fit(Matrix<double> centres, Matrix<double> samples, Matrix<double> response, RbfKernel kernel, double shape, TolerancePolicy tol)` composes design and solve into a fitted field, `RbfFit.Evaluate` projecting a query batch.
- Auto: `Interpolant.At`/`Slope`/`Curvature`/`Area` wrap `IInterpolation.Interpolate`/`Differentiate`/`Differentiate2`/`Integrate` in an `Option` absence carrier so a `NaN` at a sample point reads as `None`, and the capability tier (`Smooth` for the cubic/linear/step schemes whose `SupportsDifferentiation`/`SupportsIntegration` are both true, `Sampled` for the barycentric/rational schemes where both are false) is the constraint the extension block reads so an unsupported derivative call does not compile; `Design` builds the `Φ` block `Φ_ij = kernel.Phi(‖xᵢ − cⱼ‖, shape)` and, for a conditionally-positive-definite kernel (`PolynomialOrder ≥ 0`), augments to the saddle system `[Φ P; Pᵀ 0]` over the monomial reproduction basis up to `PolynomialOrder`; `Reconstruct` decomposes the design once through `Tensor/blas#DENSE_ALGEBRA` `DenseOps.Decompose(design, FactorizationKind.Svd)`, solves every response column through the one held `ISolver<double>.Solve(Matrix<double>)`, and witnesses the Frobenius residual against the original design; `Fit` pads the response with the polynomial side-constraint zero rows, solves once, and splits the solution into RBF weights and polynomial coefficients so a vector-valued response reconstructs gradient and flux fields in one solve.
- Receipt: reconstruction rides the `Tensor/blas#DENSE_ALGEBRA` `Factorization` `ComputeReceipt` evidence the held SVD stamps; the `RbfFit` carries the centres, kernel, shape, weights, and polynomial coefficients so the fitted field is reproducible and content-keyable.
- Packages: MathNet.Numerics, Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox
- Growth: a new interpolation scheme is one `Interpolant` factory returning its capability tier; a new capability is one marker interface plus one extension block; a new radial kernel is one `RbfKernel` row carrying its radial form and polynomial-tail order; zero new surface — a per-scheme interpolant class family is collapsed onto the one `Interpolant<TCap>` capsule and a per-kernel design function family onto the one `Scatter.Design`.
- Boundary: interpolant capability is lifted to the `TCap` type parameter so an unsupported differentiate/integrate call is unrepresentable, never a runtime `SupportsDifferentiation` bool that throws on an unsupported call — the phantom that gates nothing (a `TCap` declared but read by no constraint) is the deleted form; interpolant evaluation wraps in an absence carrier because the step interpolant returns `NaN` at sample points and the rational interpolant returns `NaN` below ULP, poisoning a gradient accumulator silently; scattered reconstruction is the owned radial-basis-plus-polynomial design because no library surface exists, and the polynomial tail is genuine (`PolynomialOrder` per kernel row drives the `[Φ P; Pᵀ 0]` saddle augmentation the conditionally-positive-definite kernels require for a unique interpolant) — a bare `Φ` block claiming the polynomial reproduction the prose advertises is the deleted form; the reconstruction decomposes the design ONCE into a held SVD and solves the matrix-valued response through the one handle per the `Tensor/blas#DENSE_ALGEBRA` held-handle law — a fresh `DenseRoute.Solve` per response column paying a cubic SVD each time is the deleted form; the kernel vocabulary is the closed `RbfKernel` `[SmartEnum<string>]` and a `Func<double, double>` riding beside the design is the rejected form because the polynomial-tail order is row data the kernel owns; the reconstruction witnesses the Frobenius residual against the original design through the `TolerancePolicy.Admits` gate because the SVD pseudo-inverse certifies only the least-squares minimum, not a usable interpolant, and a rank-deficient design under a loose shape parameter passes the solve while failing the field; the lane is host-local and the radial design composes `MathNet` `Matrix<double>` directly — a package-local matrix wrapper is the deleted form mirroring the blas-lane no-`RasmMatrix` law.

```csharp signature
// Capability markers — generic bounds ONLY, never read by `is` or reflection. They lift the MathNet
// IInterpolation.SupportsDifferentiation/SupportsIntegration runtime bools to compile-time type constraints so
// an unsupported Slope/Curvature/Area call does not compile and never throws at a sample point.
public interface IInterpolantCapability { }
public interface IDifferentiable : IInterpolantCapability { }
public interface IIntegrable : IInterpolantCapability { }

public readonly struct Smooth : IDifferentiable, IIntegrable { }
public readonly struct Sampled : IInterpolantCapability { }

public readonly record struct Interpolant<TCap>(IInterpolation Inner) where TCap : IInterpolantCapability {
    public Option<double> At(double x) =>
        Inner.Interpolate(x) is var y && double.IsFinite(y) ? Some(y) : None;
}

public static class Interpolant {
    // Smooth tier: the cubic-spline variants, linear, and step schemes whose SupportsDifferentiation and
    // SupportsIntegration are both true. Sampled tier: the barycentric polynomial and Floater-Hormann rational
    // schemes where both are false — Slope/Area on a Sampled interpolant is a compile error, not a throw.
    public static Interpolant<Smooth> CubicSpline(double[] nodes, double[] values) => new(Interpolate.CubicSpline(nodes, values));
    public static Interpolant<Smooth> Akima(double[] nodes, double[] values) => new(Interpolate.CubicSplineRobust(nodes, values));
    public static Interpolant<Smooth> Monotone(double[] nodes, double[] values) => new(Interpolate.CubicSplineMonotone(nodes, values));
    public static Interpolant<Smooth> Linear(double[] nodes, double[] values) => new(Interpolate.Linear(nodes, values));
    public static Interpolant<Smooth> Step(double[] nodes, double[] values) => new(Interpolate.Step(nodes, values));
    public static Interpolant<Sampled> Rational(double[] nodes, double[] values) => new(Interpolate.Common(nodes, values));
    public static Interpolant<Sampled> Polynomial(double[] nodes, double[] values) => new(Interpolate.Polynomial(nodes, values));

    extension<TCap>(Interpolant<TCap> self) where TCap : IDifferentiable {
        public Option<double> Slope(double x) => self.Inner.Differentiate(x) is var d && double.IsFinite(d) ? Some(d) : None;
        public Option<double> Curvature(double x) => self.Inner.Differentiate2(x) is var c && double.IsFinite(c) ? Some(c) : None;
    }

    extension<TCap>(Interpolant<TCap> self) where TCap : IIntegrable {
        public Option<double> Area(double a, double b) => self.Inner.Integrate(a, b) is var area && double.IsFinite(area) ? Some(area) : None;
    }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class RbfKernel {
    // phi(delta, shape): the radial basis as a function of distance δ and shape parameter ε. PolynomialOrder is
    // the minimum polynomial-tail degree a well-posed interpolant needs — −1 for the strictly-positive-definite
    // kernels (no tail), 0/1 for the conditionally-positive-definite kernels whose saddle design adds the tail.
    public static readonly RbfKernel Gaussian = new("gaussian", static (delta, eps) => Math.Exp(-(eps * delta) * (eps * delta)), polynomialOrder: -1);
    public static readonly RbfKernel InverseMultiquadric = new("inverse-multiquadric", static (delta, eps) => 1.0 / Math.Sqrt(1.0 + (eps * delta) * (eps * delta)), polynomialOrder: -1);
    public static readonly RbfKernel Wendland = new("wendland-c2", static (delta, eps) => eps * delta < 1.0 ? Math.Pow(1.0 - eps * delta, 4) * (4.0 * eps * delta + 1.0) : 0.0, polynomialOrder: -1);
    public static readonly RbfKernel Multiquadric = new("multiquadric", static (delta, eps) => Math.Sqrt(1.0 + (eps * delta) * (eps * delta)), polynomialOrder: 0);
    public static readonly RbfKernel PolyharmonicCubic = new("polyharmonic-cubic", static (delta, _) => delta * delta * delta, polynomialOrder: 1);
    public static readonly RbfKernel ThinPlateSpline = new("thin-plate-spline", static (delta, _) => delta > 0.0 ? delta * delta * Math.Log(delta) : 0.0, polynomialOrder: 1);

    private readonly Func<double, double, double> phi;

    public int PolynomialOrder { get; }

    public double Phi(double delta, double shape) => phi(delta, shape);
}

public sealed record RbfDesign(Matrix<double> Matrix, int PolynomialTerms, int Centres);

public sealed record RbfFit(Matrix<double> Centres, RbfKernel Kernel, double Shape, int PolynomialOrder, Matrix<double> Weights, Matrix<double> PolynomialCoefficients) {
    // Evaluate the fitted field at query rows: Σ_j w_j·φ(‖q − c_j‖) + Σ_t p_t·monomial_t(q). The response
    // columns reconstruct jointly so a vector-valued field (gradient + flux) evaluates in one pass.
    public Matrix<double> Evaluate(Matrix<double> queries) {
        var phi = Matrix<double>.Build.Dense(queries.RowCount, Centres.RowCount,
            (i, j) => Kernel.Phi((queries.Row(i) - Centres.Row(j)).L2Norm(), Shape));
        Matrix<double> field = phi.Multiply(Weights);
        if (PolynomialOrder < 0) { return field; }
        Seq<int[]> terms = Scatter.Monomials(queries.ColumnCount, PolynomialOrder);
        var poly = Matrix<double>.Build.Dense(queries.RowCount, terms.Count, (i, t) => Scatter.Evaluate(queries.Row(i), terms[t]));
        return field + poly.Multiply(PolynomialCoefficients);
    }
}

public static class Scatter {
    // The augmented RBF design: [Φ P; Pᵀ 0] when the kernel needs a polynomial reproduction tail, else the
    // bare Φ block. Φ_ij = φ(‖sampleᵢ − centreⱼ‖); the top trend block evaluates the monomials at the samples
    // and the lower constraint block at the centres so the saddle enforces Pᵀ_centres·w = 0 — the orthogonality
    // the conditionally-positive-definite kernels require for a unique interpolant. Sample and centre blocks
    // coincide in the square interpolation case and diverge in the rectangular regression case; the prose's
    // radial-basis-PLUS-polynomial is this tail, not a bare Φ.
    public static Fin<RbfDesign> Design(Matrix<double> centres, Matrix<double> samples, RbfKernel kernel, double shape) {
        var phi = Matrix<double>.Build.Dense(samples.RowCount, centres.RowCount,
            (i, j) => kernel.Phi((samples.Row(i) - centres.Row(j)).L2Norm(), shape));
        if (kernel.PolynomialOrder < 0) {
            return Fin.Succ(new RbfDesign(phi, 0, centres.RowCount));
        }

        Seq<int[]> terms = Monomials(samples.ColumnCount, kernel.PolynomialOrder);
        var pSamples = Matrix<double>.Build.Dense(samples.RowCount, terms.Count, (i, t) => Evaluate(samples.Row(i), terms[t]));
        var pCentres = Matrix<double>.Build.Dense(centres.RowCount, terms.Count, (j, t) => Evaluate(centres.Row(j), terms[t]));
        Matrix<double> top = phi.Append(pSamples);
        Matrix<double> bottom = pCentres.Transpose().Append(Matrix<double>.Build.Dense(terms.Count, terms.Count));
        return Fin.Succ(new RbfDesign(top.Stack(bottom), terms.Count, centres.RowCount));
    }

    // ONE held SVD over the design, the matrix-valued response solved in a single least-squares pass, then a
    // Frobenius witness against the ORIGINAL design — never a per-column re-decomposition paying a fresh cubic
    // SVD for every response column the held-handle law forbids.
    public static Fin<Matrix<double>> Reconstruct(Matrix<double> design, Matrix<double> response, TolerancePolicy tol) =>
        Admission.Admit(design).Bind(_ =>
            DenseOps.Decompose(design, FactorizationKind.Svd).Bind(factor =>
                factor.Solver.Solve(response) is var solution
                && (design.Multiply(solution) - response).FrobeniusNorm() / Math.Max(1.0, response.FrobeniusNorm()) is var residual
                && tol.Admits(residual)
                    ? Fin.Succ(solution)
                    : Fin.Fail<Matrix<double>>(new ComputeFault.ModelRejected($"<scatter-witness-fail:r={residual:e3}>"))));

    // The high-level scattered-field fit: build the augmented design, pad the response with the polynomial
    // side-constraint zero rows, solve once through Reconstruct, and split the solution into RBF weights and
    // polynomial coefficients.
    public static Fin<RbfFit> Fit(Matrix<double> centres, Matrix<double> samples, Matrix<double> response, RbfKernel kernel, double shape, TolerancePolicy tol) =>
        Design(centres, samples, kernel, shape).Bind(design =>
            Reconstruct(design.Matrix, Pad(response, design.PolynomialTerms), tol).Map(solution =>
                new RbfFit(centres, kernel, shape, kernel.PolynomialOrder,
                    solution.SubMatrix(0, design.Centres, 0, solution.ColumnCount),
                    design.PolynomialTerms == 0
                        ? Matrix<double>.Build.Dense(0, solution.ColumnCount)
                        : solution.SubMatrix(design.Centres, design.PolynomialTerms, 0, solution.ColumnCount))));

    static Matrix<double> Pad(Matrix<double> response, int polynomialTerms) =>
        polynomialTerms == 0 ? response : response.Stack(Matrix<double>.Build.Dense(polynomialTerms, response.ColumnCount));

    // The monomial exponent multi-indices of total degree ≤ order over `dimension` variables — the polynomial
    // reproduction basis the conditionally-PD tail spans (order 0 → {constant}, order 1 → {1, x₁ … x_d}).
    public static Seq<int[]> Monomials(int dimension, int order) => toSeq(Compositions(dimension, order));

    public static double Evaluate(Vector<double> point, int[] exponents) =>
        toSeq(Enumerable.Range(0, exponents.Length)).Fold(1.0, (acc, k) => acc * Math.Pow(point[k], exponents[k]));

    static IEnumerable<int[]> Compositions(int slots, int maxTotal) =>
        slots == 0
            ? new[] { Array.Empty<int>() }
            : from head in Enumerable.Range(0, maxTotal + 1)
              from tail in Compositions(slots - 1, maxTotal - head)
              select tail.Prepend(head).ToArray();
}
```
