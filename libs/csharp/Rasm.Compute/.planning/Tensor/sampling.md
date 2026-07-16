# [COMPUTE_SAMPLING]

Rasm.Compute owned-build numeric lane for quasi-Monte-Carlo sampling and scattered reconstruction: kernels with no library surface, built and gated in-house, every estimate leaving as a replicate family carrying its spread. `MathNet` exposes no Sobol/Halton (only `SystemRandomSource`) and no scattered radial-basis solver, so the seed-explicit state-serializable `LowDiscrepancy` carrier, the `JoeKuo` direction-number recurrence, the Halton radical inverse, the rank-stratified Latin-hypercube design, and the `Scatter` radial-basis-plus-polynomial reconstruction are composed from the rails rather than imported.

`LowDiscrepancy` folds `SequenceFamily` as a type axis because variance law, error bars, and convergence rate fork on the family and the state shapes do not unify: the stateless `Sobol` and `Halton` markers each ARE their generation law (`Sobol` the gray-code XOR over `JoeKuo.Directions`, `Halton` the radical inverse over the per-dimension `JoeKuo.Primes[d]`) and `Independent(Stream)` the counter-stream pseudo-random leg; construction is factory-only, so an incoherent family/state pairing is unmintable. `Scatter` reconstructs a matrix-valued field through one held rank-revealing SVD into the `Tensor/blas#DENSE_ALGEBRA` route, interpolant capability lifted to a compile-time type parameter so an unsupported differentiate/integrate call is unrepresentable. Host-local, crossing no wire; the direction-number recurrence, gray-code Sobol draw, radical-inverse digit walk, `SplitMix64` counter, Warnock discrepancy kernels, monomial enumeration, rank stratification, and `OnlineStat` Welford increment are its sanctioned statement-form numeric kernels.

## [01]-[INDEX]

- [02]-[OWNED_BUILDS]: owned Sobol (Joe-Kuo direction numbers) / Halton (prime radical inverse) / Latin-hypercube / pseudo-random sampler over one `LowDiscrepancy` carrier; RQMC replicate family with Warnock-discrepancy net quality.
- [03]-[SCATTER_RECONSTRUCTION]: capability-typed interpolant; radial-basis-plus-polynomial design into the held rank-revealing SVD; the `RbfKernel` vocabulary and the `RbfFit` field.

## [02]-[OWNED_BUILDS]

- Owner: `LowDiscrepancy` the seed-explicit state-serializable carrier folding `SequenceFamily` over a per-construction `JoeKuo.Directions` table, a per-draw counter, and a per-dimension `ShiftSeed` key vector; `Scramble` the `[SmartEnum<string>]` randomization policy carrying paired binary and digit delegates; `SequenceFamily` the `[Union]` family discriminant; `ReplicatePolicy` the replicate-count, confidence, and net-quality gate; `ReplicateFamily` the RQMC estimate carrier; `JoeKuo` the direction-number recurrence, embedded primitive-polynomial owner, and sieved Halton prime table.
- Cases: `SequenceFamily` cases `Sobol`, `Halton` (stateless markers — each case is its generation law, no radix field exists to hold an incoherent value) and `Independent(ulong Stream)`; `Scramble` rows none, digital-shift, owen.
- Auto: `Draw` folds the `SequenceFamily` case through the generated total `Switch`; `LatinHypercube` draws one joint Sobol net and rank-stratifies each dimension into one point per stratum; `Replicates` draws exactly `2^BlockExponent` points per replicate, rejects non-finite estimator output, folds the estimator through `OnlineStat`, and admits the Student bound plus Warnock figures through `ReplicatePolicy`.
- Receipt: `ReplicateFamily(Mean, CrossReplicateVariance, StudentBound, StarDiscrepancy, WorstProjection)` because a single equidistributed estimate carries no recoverable spread, and the net-quality fields make a gate reject on discrepancy rather than slow convergence.
- Packages: MathNet.Numerics, Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox
- Growth: a new family is one `SequenceFamily` case plus one `Fill*` kernel; a new scramble is one `Scramble` row; a new net-quality figure is one `ReplicateFamily` field plus one kernel; zero new surface — a `SobolGenerator`/`HaltonGenerator`/`LatinHypercubeSampler` sibling family collapses onto the one `LowDiscrepancy` carrier.
- Boundary — the Sobol leg owns the Joe-Kuo recurrence over the embedded primitive-polynomial set: an all-zero direction table collapses every point to the origin, and the unscaled-`m` recurrence omitting the per-term bit-scaling yields wrong direction numbers and a plausible-looking broken net; both are rejected.
- Boundary — Halton reads its base per dimension from `JoeKuo.Primes[d]` (dimension 0 → 2) because a single shared base collapses every coordinate onto one radical-inverse sequence; the family discriminant is the stateless case itself, and the deleted `Equidistributed(int Base)` numeric marker — which admitted arbitrary bases and silently routed every non-2 value to Halton — is the named incoherent-admission form, closed by the private constructor plus `Sobol`/`Halton`/`Pseudo` factory-only minting.
- Boundary — `Scramble` applies uniformly across both legs so `Scramble.None` genuinely disables the binary XOR and the base-`b` digit shift, never the hardcoded `(digit + shift) % radix` that shifts even under `None`; `owen` and its base-`b` linear-digit analog are the higher-quality randomization the `Growth` axis names, added as `Scramble` rows, never parallel samplers.
- Boundary — the `Independent` stream is a `SplitMix64` counter keyed per `(Stream, Drawn, dimension)` because a per-dimension constant XORed over one shared counter leaves every coordinate near-identical; the independent-versus-equidistributed split is the `SequenceFamily` case axis, never a bool, because the per-coordinate constructions do not unify.
- Boundary — the block exponent is accepted at the draw entrypoint because equidistribution holds only at power-of-base counts and non-power prefixes degrade discrepancy with no diagnostic; the generator is seed-explicit and state-serializable for checkpoint-resume, since thread-entropy and parallel block fill are non-deterministic regardless of seeding — the MathNet `IContinuousDistribution.Samples()` stateful stream and the `torch.manual_seed`/`torch.randn` device RNG are both named rejected draw sources on this lane for the same reason: neither serializes its state, so neither can resume a checkpointed campaign mid-stream.
- Boundary — net-quality figures are the Warnock L2 star-discrepancy and worst-2D-projection discrepancy; full-dimensional uniformity does not exclude a degenerate 2-D projection.
- Boundary — `Replicates` rejects `Scramble.None` over the Sobol/Halton legs: `Reseed` reaches those draws only through the scramble key, so an unscrambled equidistributed generator repeats one block per replicate and the cross-replicate variance certifies a false zero spread; the `Independent` leg replicates honestly under `None` because its counter key folds `ShiftSeed` regardless of scramble.
- Boundary — Latin-hypercube rank-stratifies a JOINT low-discrepancy draw into one point per stratum per dimension; a per-axis 1-D sequence Cartesian-producted inflates the point count and destroys the joint low-discrepancy the variance reduction depends on, and is rejected; the embedded polynomial resource and prime table load once at `JoeKuo` type initialization, a missing resource a fatal construction fault.

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

    // Binary scrambling transforms a Sobol coordinate; radix scrambling transforms each Halton digit.
    public uint Bits(uint value, uint key) => bits(value, key);
    public uint Digit(uint value, uint key, int radix, int position) => digit(value, key, radix, position);

    // Owen nested-uniform scramble: reverse, hash-perturb from the most-significant bit, reverse back — RMSE
    // decays one half-order faster than the digital shift on a smooth integrand.
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

    // Base-b per-position affine digit permutation d -> (a·d + c) mod radix, a coprime to the prime radix,
    // keyed by a SplitMix hash of (key, position) so each position draws an independent permutation.
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

    // Checked-overflow builds require an explicit unchecked region for multiplicative diffusion.
    internal static ulong SplitMix64(ulong z) {
        unchecked {
            z += 0x9E3779B97F4A7C15UL;
            z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
            z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
            return z ^ (z >> 31);
        }
    }
}

// Sobol and Halton are stateless markers whose radices derive from their laws; no integer base can admit an
// incoherent family, unlike the deleted `Equidistributed(int Base)` form.
[Union]
public abstract partial record SequenceFamily {
    private SequenceFamily() { }

    public sealed record Sobol : SequenceFamily;
    public sealed record Halton : SequenceFamily;
    public sealed record Independent(ulong Stream) : SequenceFamily;
}

public sealed record ReplicateFamily(double Mean, double CrossReplicateVariance, double StudentBound, double StarDiscrepancy, double WorstProjection);

public sealed record ReplicatePolicy(int BlockExponent, int Replicates, double Confidence, double MaxStarDiscrepancy, double MaxProjection) {
    public static readonly ReplicatePolicy Default = new(BlockExponent: 12, Replicates: 16, Confidence: 0.95, MaxStarDiscrepancy: 0.05, MaxProjection: 0.1);
}

// Factory-only construction makes family/direction-table mismatch unmintable: Sobol owns a Joe-Kuo table,
// while Halton and Independent own none.
public sealed record LowDiscrepancy {
    public SequenceFamily Family { get; private init; }
    public Scramble Scramble { get; private init; }
    public int Dimensions { get; private init; }
    public int Seed { get; private init; }
    public uint[,] DirectionNumbers { get; private init; }
    public uint[] ShiftSeed { get; private init; }
    public long Drawn { get; private init; }

    private LowDiscrepancy(SequenceFamily family, Scramble scramble, int dimensions, int seed, uint[,] directions, uint[] shift, long drawn) =>
        (Family, Scramble, Dimensions, Seed, DirectionNumbers, ShiftSeed, Drawn) = (family, scramble, dimensions, seed, directions, shift, drawn);

    public static Fin<LowDiscrepancy> Sobol(int dimensions, int seed, Scramble scramble) =>
        scramble is not null && dimensions >= 1 && dimensions <= JoeKuo.MaxDimensions
            ? Fin.Succ(new LowDiscrepancy(new SequenceFamily.Sobol(), scramble, dimensions, seed, JoeKuo.Directions(dimensions), ShiftFor(dimensions, seed), 0L))
            : Fin.Fail<LowDiscrepancy>(new ComputeFault.ModelRejected($"<sobol-dimension-bound:{dimensions}>"));

    // Per-dimension radix is always JoeKuo.Primes[d] (dimension 0 -> 2); the case is a marker, never a
    // shared per-coordinate base.
    public static Fin<LowDiscrepancy> Halton(int dimensions, int seed, Scramble scramble) =>
        scramble is not null && dimensions >= 1 && dimensions <= JoeKuo.Primes.Length
            ? Fin.Succ(new LowDiscrepancy(new SequenceFamily.Halton(), scramble, dimensions, seed, EmptyDirections, ShiftFor(dimensions, seed), 0L))
            : Fin.Fail<LowDiscrepancy>(new ComputeFault.ModelRejected($"<halton-dimension-bound:{dimensions}>"));

    public static Fin<LowDiscrepancy> Pseudo(int dimensions, int seed, ulong stream, Scramble scramble) =>
        scramble is not null && dimensions >= 1
            ? Fin.Succ(new LowDiscrepancy(new SequenceFamily.Independent(stream), scramble, dimensions, seed, EmptyDirections, ShiftFor(dimensions, seed), 0L))
            : Fin.Fail<LowDiscrepancy>(new ComputeFault.ModelRejected($"<pseudo-dimension-bound:{dimensions}>"));

    // Generated total `Switch` folds the family axis without a radix test: Sobol is gray-code XOR, Halton is
    // radix reversal, and Independent is a counter hash.
    public (LowDiscrepancy Next, double[] Point) Draw() {
        double[] point = Family.Switch(
            state: this,
            sobol: static (self, _) => self.SobolPoint(),
            halton: static (self, _) => self.HaltonPoint(),
            independent: static (self, i) => self.FillIndependent(i.Stream));
        return (this with { Drawn = Drawn + 1 }, point);
    }

    double[][] Net(int count) {
        double[][] rows = new double[count][];
        LowDiscrepancy generator = this;
        for (int i = 0; i < count; i++) {
            (generator, rows[i]) = generator.Draw();
        }

        return rows;
    }

    // Latin hypercube rank-stratifies one joint Sobol net; Cartesian products destroy its joint discrepancy.
    public static Fin<double[][]> LatinHypercube(int dimensions, int count, int seed, Scramble scramble) =>
        dimensions >= 1 && count >= 1
            ? Sobol(dimensions, seed, scramble).Map(generator => Stratify(generator.Net(count)))
            : Fin.Fail<double[][]>(new ComputeFault.ModelRejected($"<lhs-bound:dims={dimensions}:count={count}>"));

    public static Fin<ReplicateFamily> Replicates(LowDiscrepancy generator, ReplicatePolicy policy, Func<ReadOnlyMemory<double>, double> estimator) {
        bool valid = generator is not null && policy is not null && estimator is not null
            && policy.BlockExponent is >= 1 and <= 24 && policy.Replicates >= 2
            && double.IsFinite(policy.Confidence) && policy.Confidence is > 0.0 and < 1.0
            && double.IsFinite(policy.MaxStarDiscrepancy) && policy.MaxStarDiscrepancy >= 0.0
            && double.IsFinite(policy.MaxProjection) && policy.MaxProjection >= 0.0;
        if (!valid) {
            return Fin.Fail<ReplicateFamily>(new ComputeFault.ModelRejected("<replicate-policy-invalid>"));
        }

        // Reseed randomizes Sobol/Halton only through the scramble key; `Scramble.None` draws byte-identical
        // replicate blocks, so cross-replicate variance and the Student bound collapse to a false zero.
        if (generator.Scramble == Scramble.None && generator.Family is not SequenceFamily.Independent) {
            return Fin.Fail<ReplicateFamily>(new ComputeFault.ModelRejected("<replicate-unscrambled-equidistributed>"));
        }

        return Try.lift<Fin<ReplicateFamily>>(() => {
                int count = 1 << policy.BlockExponent;
                return toSeq(Enumerable.Range(0, policy.Replicates))
                    .Map(r => Block(generator.Reseed(r), count, estimator))
                    .Traverse(identity)
                    .Bind(blocks => {
                        OnlineStat stat = blocks.Fold(OnlineStat.Empty, static (acc, block) => acc.Push(block.Mean));
                        double variance = stat.Variance(MomentNormalizer.Sample);
                        double bound = StudentT.InvCDF(0.0, 1.0, policy.Replicates - 1, 0.5 + policy.Confidence / 2.0) * Math.Sqrt(variance / policy.Replicates);
                        double star = blocks.Map(static block => block.Star).Max();
                        double projection = blocks.Map(static block => block.Projection).Max();
                        return double.IsFinite(stat.Mean) && double.IsFinite(variance) && double.IsFinite(bound)
                            && double.IsFinite(star) && double.IsFinite(projection)
                            && star <= policy.MaxStarDiscrepancy && projection <= policy.MaxProjection
                                ? Fin.Succ(new ReplicateFamily(stat.Mean, variance, bound, star, projection))
                                : Fin.Fail<ReplicateFamily>(new ComputeFault.ModelRejected($"<replicate-evidence:variance={variance:e3}:bound={bound:e3}:star={star:e3}:projection={projection:e3}>"));
                    });
            })
            .Run()
            .MapFail(static error => (Error)new ComputeFault.ModelRejected($"<replicate-kernel:{error.Message}>"))
            .Bind(identity);
    }

    double[] SobolPoint() {
        double[] point = new double[Dimensions];
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
        double[] point = new double[Dimensions];
        ulong index = unchecked((ulong)Drawn) + 1UL;
        for (int d = 0; d < Dimensions; d++) {
            point[d] = RadicalInverse(index, JoeKuo.Primes[d], ShiftSeed[d], Scramble);
        }

        return point;
    }

    double[] FillIndependent(ulong stream) {
        double[] point = new double[Dimensions];
        for (int d = 0; d < Dimensions; d++) {
            ulong key = unchecked(stream ^ ((ulong)Drawn * 0xD1B54A32D192ED03UL) ^ ((ulong)(uint)d * 0x9E3779B97F4A7C15UL) ^ ShiftSeed[d]);
            point[d] = (Scramble.SplitMix64(key) >> 11) * Math.ScaleB(1.0, -53);
        }

        return point;
    }

    LowDiscrepancy Reseed(int replicate) =>
        this with { ShiftSeed = ShiftFor(Dimensions, unchecked(Seed + replicate)), Drawn = 0L };

    static Fin<(double Mean, double Star, double Projection)> Block(LowDiscrepancy generator, int count, Func<ReadOnlyMemory<double>, double> estimator) {
        double[][] net = generator.Net(count);
        Seq<double> values = toSeq(net).Map(point => estimator(point));
        return values.Exists(static value => !double.IsFinite(value))
            ? Fin.Fail<(double Mean, double Star, double Projection)>(new ComputeFault.ModelRejected("<replicate-estimator-nonfinite>"))
            : Fin.Succ((
                Mean: values.Fold(OnlineStat.Empty, static (acc, value) => acc.Push(value)).Mean,
                Star: StarDiscrepancyL2(net),
                Projection: WorstProjection(net)));
    }

    // Radix inversion sums finite b^-k contributions under the per-position digit scramble.
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
        double[][] stratified = net.Select(static row => new double[row.Length]).ToArray();
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

// Owned Joe-Kuo recurrence reads embedded primitive-polynomial degrees, coefficients, and seeds; Halton bases
// come from a bounded prime sieve, and both constructions run once per generator.
public static class JoeKuo {
    public const int MaxDimensions = 21_201;
    public const int Bits = 32;

    public static readonly int[] Primes = Sieve(MaxDimensions);
    static readonly (int Degree, uint Coefficients, uint[] Seeds)[] Polynomials = LoadPolynomials();

    // Canonical recurrence operates on scaled 32-bit directions: `v[k]=2^(31−k)` for dimension zero and the
    // primitive-polynomial XOR recurrence for higher dimensions; unscaled seeds yield a plausible broken net.
    public static uint[,] Directions(int dimensions) {
        uint[,] v = new uint[dimensions, Bits];
        for (int k = 0; k < Bits; k++) { v[0, k] = 1u << (Bits - 1 - k); }
        for (int d = 1; d < dimensions; d++) {
            (int degree, uint coefficients, uint[] seeds) = Polynomials[d - 1];
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
        using Stream stream = typeof(JoeKuo).Assembly.GetManifestResourceStream("Rasm.Compute.new-joe-kuo-6.21201.col")
            ?? throw new InvalidOperationException("<joe-kuo-resource-missing>");
        using StreamReader reader = new(stream);
        return reader.ReadToEnd()
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Skip(1)
            .Select(static line => {
                string[] f = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                return (int.Parse(f[1]), uint.Parse(f[2]), f.Skip(3).Select(uint.Parse).ToArray());
            })
            .ToArray();
    }

    // First MaxDimensions primes come from a bounded sieve — Halton dimension d uses the
    // d-th prime, dimension 0 -> 2). p_n ≈ n(ln n + ln ln n) bounds the sieve ceiling.
    static int[] Sieve(int wanted) {
        int ceiling = wanted < 6 ? 15 : (int)(wanted * (Math.Log(wanted) + Math.Log(Math.Log(wanted)))) + 16;
        bool[] composite = new bool[ceiling + 1];
        List<int> primes = new(wanted);
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
- Entry: `public static Fin<Interpolant<Smooth>> CubicSpline(double[] nodes, double[] values)` and its `Hermite` (derivative-constrained, `Interpolate.CubicSplineWithDerivatives`)/`Akima`/`Monotone`/`Linear`/`Step` siblings admit finite, aligned, strictly ordered samples before minting the differentiable-integrable tier; `Rational`/`Polynomial` mint the sample-only tier through the same rail; `public Option<double> At(double x)` is the absence-carried evaluation present on every tier, `Slope`/`Curvature` are reachable only where `TCap : IDifferentiable` and `Area` only where `TCap : IIntegrable`; `public static Fin<RbfDesign> Design(Matrix<double> centres, Matrix<double> samples, RbfKernel kernel, double shape)` builds the augmented radial-basis-plus-polynomial design; `public static Fin<Matrix<double>> Reconstruct(Matrix<double> design, Matrix<double> response, TolerancePolicy tol)` solves a matrix-valued response through one held SVD into the `RankRevealing` route; `public static Fin<RbfFit> Fit(Matrix<double> centres, Matrix<double> samples, Matrix<double> response, RbfKernel kernel, double shape, TolerancePolicy tol)` composes design and solve into a fitted field, `RbfFit.Evaluate` projecting a query batch.
- Auto: `Interpolant.Build` performs the common sample admission and captures every throwing `Interpolate.*` factory through `Try.lift`; `Interpolant.Read` captures evaluation, derivative, and integral calls in an `Option` absence carrier so a throw or non-finite result reads as `None`, and the capability tier (`Smooth` for the cubic/linear/step schemes whose `SupportsDifferentiation`/`SupportsIntegration` are both true, `Sampled` for the barycentric/rational schemes where both are false) makes an unsupported derivative call fail compilation; `Design` builds the `Φ` block `Φ_ij = kernel.Phi(‖xᵢ − cⱼ‖, shape)` and, for a conditionally-positive-definite kernel (`PolynomialOrder ≥ 0`), augments to the saddle system `[Φ P; Pᵀ 0]` over the monomial reproduction basis up to `PolynomialOrder`; every `Scatter` MathNet boundary is captured and finite-gated; `Reconstruct` decomposes the design once through `Tensor/blas#DENSE_ALGEBRA` `DenseOps.Decompose(design, FactorizationKind.Svd)`, solves every response column through the held `ISolver<double>.Solve(Matrix<double>)`, and witnesses the Frobenius residual against the original design; `Fit` pads the response with polynomial side-constraint zero rows and splits the one solution into RBF weights and polynomial coefficients.
- Receipt: reconstruction rides the `Tensor/blas#DENSE_ALGEBRA` `Factorization` `ComputeReceipt` evidence the held SVD stamps; the `RbfFit` carries the centres, kernel, shape, weights, and polynomial coefficients so the fitted field is reproducible and content-keyable.
- Packages: MathNet.Numerics, Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox
- Growth: a new interpolation scheme is one `Interpolant` factory returning its capability tier; a new capability is one marker interface plus one extension block; a new radial kernel is one `RbfKernel` row carrying its radial form and polynomial-tail order; zero new surface — a per-scheme interpolant class family is collapsed onto the one `Interpolant<TCap>` capsule and a per-kernel design function family onto the one `Scatter.Design`.
- Boundary: interpolant capability is lifted to the `TCap` type parameter so an unsupported differentiate/integrate call is unrepresentable, never a runtime `SupportsDifferentiation` bool that throws on an unsupported call — the phantom that gates nothing (a `TCap` declared but read by no constraint) is the deleted form; one `Build` rail rejects null, misaligned, non-finite, or unordered samples and captures the native factory exception, so a public constructor-shaped factory never throws before returning its promised `Fin`; interpolant evaluation wraps in an absence carrier because the step interpolant returns `NaN` at sample points and the rational interpolant returns `NaN` below ULP, poisoning a gradient accumulator silently; scattered reconstruction is the owned radial-basis-plus-polynomial design because no library surface exists, and the polynomial tail is genuine (`PolynomialOrder` per kernel row drives the `[Φ P; Pᵀ 0]` saddle augmentation the conditionally-positive-definite kernels require for a unique interpolant) — a bare `Φ` block claiming the polynomial reproduction the prose advertises is the deleted form; the reconstruction decomposes the design ONCE into a held SVD and solves the matrix-valued response through the one handle per the `Tensor/blas#DENSE_ALGEBRA` held-handle law — a fresh `DenseRoute.Solve` per response column paying a cubic SVD each time is the deleted form; the kernel vocabulary is the closed `RbfKernel` `[SmartEnum<string>]` and a `Func<double, double>` riding beside the design is the rejected form because the polynomial-tail order is row data the kernel owns; the reconstruction witnesses the Frobenius residual against the original design through the `TolerancePolicy.Admits` gate because the SVD pseudo-inverse certifies only the least-squares minimum, not a usable interpolant, and a rank-deficient design under a loose shape parameter passes the solve while failing the field; the lane is host-local and the radial design composes `MathNet` `Matrix<double>` directly — a package-local matrix wrapper is the deleted form mirroring the blas-lane no-`RasmMatrix` law.

```csharp signature
// Generic bounds lift MathNet interpolation capability flags into compile-time constraints; unsupported
// `Slope`, `Curvature`, or `Area` calls do not compile.
public interface IInterpolantCapability { }
public interface IDifferentiable : IInterpolantCapability { }
public interface IIntegrable : IInterpolantCapability { }

public readonly struct Smooth : IDifferentiable, IIntegrable { }
public readonly struct Sampled : IInterpolantCapability { }

public sealed class Interpolant<TCap> where TCap : IInterpolantCapability {
    internal Interpolant(IInterpolation inner) => Inner = inner;

    internal IInterpolation Inner { get; }

    public Option<double> At(double x) =>
        double.IsFinite(x) ? Interpolant.Read(() => Inner.Interpolate(x)) : None;
}

public static class Interpolant {
    // `Smooth` factories admit schemes supporting both derivative and integral operations; `Sampled` factories
    // admit schemes supporting neither, so `Slope`/`Area` on `Sampled` fails at compile time.
    public static Fin<Interpolant<Smooth>> CubicSpline(double[] nodes, double[] values) =>
        Build<Smooth>(nodes, values, null, () => Interpolate.CubicSpline(nodes, values));

    public static Fin<Interpolant<Smooth>> Hermite(double[] nodes, double[] values, double[] slopes) =>
        Build<Smooth>(nodes, values, slopes, () => Interpolate.CubicSplineWithDerivatives(nodes, values, slopes));

    public static Fin<Interpolant<Smooth>> Akima(double[] nodes, double[] values) =>
        Build<Smooth>(nodes, values, null, () => Interpolate.CubicSplineRobust(nodes, values));

    public static Fin<Interpolant<Smooth>> Monotone(double[] nodes, double[] values) =>
        Build<Smooth>(nodes, values, null, () => Interpolate.CubicSplineMonotone(nodes, values));

    public static Fin<Interpolant<Smooth>> Linear(double[] nodes, double[] values) =>
        Build<Smooth>(nodes, values, null, () => Interpolate.Linear(nodes, values));

    public static Fin<Interpolant<Smooth>> Step(double[] nodes, double[] values) =>
        Build<Smooth>(nodes, values, null, () => Interpolate.Step(nodes, values));

    public static Fin<Interpolant<Sampled>> Rational(double[] nodes, double[] values) =>
        Build<Sampled>(nodes, values, null, () => Interpolate.Common(nodes, values));

    public static Fin<Interpolant<Sampled>> Polynomial(double[] nodes, double[] values) =>
        Build<Sampled>(nodes, values, null, () => Interpolate.Polynomial(nodes, values));

    static Fin<Interpolant<TCap>> Build<TCap>(double[] nodes, double[] values, double[]? slopes, Func<IInterpolation> build)
        where TCap : IInterpolantCapability {
        bool admitted = nodes is { Length: >= 2 }
            && values is not null
            && nodes.Length == values.Length
            && nodes.All(double.IsFinite)
            && values.All(double.IsFinite)
            && Enumerable.Range(1, nodes.Length - 1).All(index => nodes[index - 1] < nodes[index])
            && (slopes is null || slopes.Length == nodes.Length && slopes.All(double.IsFinite));

        return admitted
            ? Try.lift(() => new Interpolant<TCap>(build())).Run()
                .MapFail(error => new ComputeFault.ModelRejected($"<interpolant-build:{error.Message}>"))
            : Fin.Fail<Interpolant<TCap>>(new ComputeFault.ModelRejected("<interpolant-samples-invalid>"));
    }

    internal static Option<double> Read(Func<double> read) =>
        Try.lift(read).Run().Match(
            Succ: static value => double.IsFinite(value) ? Some(value) : None,
            Fail: static _ => None);

    extension<TCap>(Interpolant<TCap> self) where TCap : IDifferentiable {
        public Option<double> Slope(double x) => double.IsFinite(x) ? Read(() => self.Inner.Differentiate(x)) : None;
        public Option<double> Curvature(double x) => double.IsFinite(x) ? Read(() => self.Inner.Differentiate2(x)) : None;
    }

    extension<TCap>(Interpolant<TCap> self) where TCap : IIntegrable {
        public Option<double> Area(double a, double b) =>
            double.IsFinite(a) && double.IsFinite(b) ? Read(() => self.Inner.Integrate(a, b)) : None;
    }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class RbfKernel {
    // `PolynomialOrder` carries the minimum reproduction tail: −1 omits it, while 0/1 extend the saddle design.
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
    public Fin<Matrix<double>> Evaluate(Matrix<double> queries) {
        if (queries is null || Centres is null || Kernel is null || Weights is null || PolynomialCoefficients is null
            || queries.RowCount == 0 || queries.ColumnCount != Centres.ColumnCount || !TensorPrimitives.IsFiniteAll(queries.AsColumnMajorArray())) {
            return Fin.Fail<Matrix<double>>(new ComputeFault.ModelRejected("<rbf-query-invalid>"));
        }

        return Try.lift(() => {
                Matrix<double> phi = Matrix<double>.Build.Dense(queries.RowCount, Centres.RowCount,
                    (i, j) => Kernel.Phi((queries.Row(i) - Centres.Row(j)).L2Norm(), Shape));
                Matrix<double> field = phi.Multiply(Weights);
                if (PolynomialOrder < 0) { return field; }
                Seq<int[]> terms = Scatter.Monomials(queries.ColumnCount, PolynomialOrder);
                Matrix<double> poly = Matrix<double>.Build.Dense(queries.RowCount, terms.Count, (i, t) => Scatter.Evaluate(queries.Row(i), terms[t]));
                return field + poly.Multiply(PolynomialCoefficients);
            })
            .Run()
            .MapFail(static error => (Error)new ComputeFault.ModelRejected($"<rbf-evaluate:{error.Message}>"))
            .Bind(static field => TensorPrimitives.IsFiniteAll(field.AsColumnMajorArray())
                ? Fin.Succ(field)
                : Fin.Fail<Matrix<double>>(new ComputeFault.ModelRejected("<rbf-evaluate-nonfinite>")));
    }
}

public static class Scatter {
    // Augmented design uses `[Φ P; Pᵀ 0]`; sample and centre trend blocks support rectangular regression.
    public static Fin<RbfDesign> Design(Matrix<double> centres, Matrix<double> samples, RbfKernel kernel, double shape) {
        bool admitted = centres is not null && samples is not null && kernel is not null
            && centres.RowCount > 0 && samples.RowCount > 0 && centres.ColumnCount > 0
            && centres.ColumnCount == samples.ColumnCount && double.IsFinite(shape) && shape > 0.0
            && TensorPrimitives.IsFiniteAll(centres.AsColumnMajorArray()) && TensorPrimitives.IsFiniteAll(samples.AsColumnMajorArray());
        if (!admitted) {
            return Fin.Fail<RbfDesign>(new ComputeFault.ModelRejected("<rbf-design-invalid>"));
        }

        return Try.lift(() => {
                Matrix<double> phi = Matrix<double>.Build.Dense(samples.RowCount, centres.RowCount,
                    (i, j) => kernel.Phi((samples.Row(i) - centres.Row(j)).L2Norm(), shape));
                if (kernel.PolynomialOrder < 0) { return new RbfDesign(phi, 0, centres.RowCount); }

                Seq<int[]> terms = Monomials(samples.ColumnCount, kernel.PolynomialOrder);
                Matrix<double> pSamples = Matrix<double>.Build.Dense(samples.RowCount, terms.Count, (i, t) => Evaluate(samples.Row(i), terms[t]));
                Matrix<double> pCentres = Matrix<double>.Build.Dense(centres.RowCount, terms.Count, (j, t) => Evaluate(centres.Row(j), terms[t]));
                Matrix<double> top = phi.Append(pSamples);
                Matrix<double> bottom = pCentres.Transpose().Append(Matrix<double>.Build.Dense(terms.Count, terms.Count));
                return new RbfDesign(top.Stack(bottom), terms.Count, centres.RowCount);
            })
            .Run()
            .MapFail(static error => (Error)new ComputeFault.ModelRejected($"<rbf-design:{error.Message}>"))
            .Bind(static design => TensorPrimitives.IsFiniteAll(design.Matrix.AsColumnMajorArray())
                ? Fin.Succ(design)
                : Fin.Fail<RbfDesign>(new ComputeFault.ModelRejected("<rbf-design-nonfinite>")));
    }

    // One held SVD solves the matrix-valued response in one least-squares pass, then witnesses Frobenius
    // residual against the original design.
    public static Fin<Matrix<double>> Reconstruct(Matrix<double> design, Matrix<double> response, TolerancePolicy tol) =>
        design is null || response is null || tol is null || design.RowCount != response.RowCount || response.ColumnCount == 0
            ? Fin.Fail<Matrix<double>>(new ComputeFault.ModelRejected("<scatter-response-shape>"))
            : Try.lift<Fin<Matrix<double>>>(() => Admission.Admit(design).Bind(_ =>
                    Admission.Admit(response).Bind(_ =>
                        DenseOps.Decompose(design, FactorizationKind.Svd).Bind(factor =>
                            factor.Solve(response) is var solution
                            && (design.Multiply(solution) - response).FrobeniusNorm() / Math.Max(1.0, response.FrobeniusNorm()) is var residual
                            && tol.Admits(residual)
                                ? Fin.Succ(solution)
                                : Fin.Fail<Matrix<double>>(new ComputeFault.ModelRejected($"<scatter-witness-fail:r={residual:e3}>"))))))
                .Run()
                .MapFail(static error => (Error)new ComputeFault.ModelRejected($"<scatter-solve:{error.Message}>"))
                .Bind(identity);

    // Scattered-field fit pads the response with polynomial side-constraint rows, solves once, and splits the
    // result into RBF weights and polynomial coefficients.
    public static Fin<RbfFit> Fit(Matrix<double> centres, Matrix<double> samples, Matrix<double> response, RbfKernel kernel, double shape, TolerancePolicy tol) =>
        centres is null || samples is null || response is null || kernel is null || tol is null || samples.RowCount != response.RowCount
            ? Fin.Fail<RbfFit>(new ComputeFault.ModelRejected("<rbf-response-shape>"))
            : Try.lift<Fin<RbfFit>>(() => Design(centres, samples, kernel, shape).Bind(design =>
                    Reconstruct(design.Matrix, Pad(response, design.PolynomialTerms), tol).Map(solution =>
                        new RbfFit(centres, kernel, shape, kernel.PolynomialOrder,
                            solution.SubMatrix(0, design.Centres, 0, solution.ColumnCount),
                            design.PolynomialTerms == 0
                                ? Matrix<double>.Build.Dense(0, solution.ColumnCount)
                                : solution.SubMatrix(design.Centres, design.PolynomialTerms, 0, solution.ColumnCount)))))
                .Run()
                .MapFail(static error => (Error)new ComputeFault.ModelRejected($"<rbf-fit:{error.Message}>"))
                .Bind(identity);

    static Matrix<double> Pad(Matrix<double> response, int polynomialTerms) =>
        polynomialTerms == 0 ? response : response.Stack(Matrix<double>.Build.Dense(polynomialTerms, response.ColumnCount));

    // Monomial exponent multi-indices carry total degree ≤ order over `dimension` variables — the polynomial
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
