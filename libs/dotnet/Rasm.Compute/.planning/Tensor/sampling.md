# [COMPUTE_SAMPLING]

Rasm.Compute owned-build numeric lane for quasi-Monte-Carlo sampling and scattered reconstruction: kernels with no library surface, built and gated in-house, every estimate leaving as a replicate family carrying its spread. `MathNet` exposes no Sobol/Halton (only `SystemRandomSource`) and no scattered radial-basis solver, so the seed-explicit state-serializable `LowDiscrepancy` carrier, the `JoeKuo` direction-number recurrence, the `HaltonBases` demand-sieved prime table, the rank-stratified Latin-hypercube design, and the `RadialFit` radial-basis-plus-polynomial reconstruction are composed from the rails rather than imported; the kernel's landed natural-neighbour interpolant is a SIBLING scattered scheme this lane routes to rather than absorbs, on the host-type, response-arity, and payload-timing discriminants `[03]-[SCATTER_RECONSTRUCTION]` states. Every DRAW underneath them is the kernel's: `Deterministic` owns the mixer, the lane fold, the bit reversal, and the base-parameterized radical inverse, and this lane owns only the sequence constructions and randomization policies stacked on them. ONE-DIMENSIONAL interpolation is likewise the kernel's whole — `Rasm/Numerics/transform#INTERPOLATE` owns the capability-typed `Interpolant<TCap>` this lane's own form was seated as, so no interpolant capsule survives here.

`LowDiscrepancy` folds `SequenceFamily` as a type axis because variance law, error bars, and convergence rate fork on the family and the state shapes do not unify: each case CARRIES its own generation table — `Sobol(uint[,] Directions)` the gray-code XOR source, `Halton(int[] Bases)` the per-dimension radical-inverse radix, `Independent(ulong Stream)` the counter-stream pseudo-random leg — so an incoherent family/table pairing is unmintable and no empty sibling table rides beside a leg that never reads it. `RadialFit` reconstructs a matrix-valued field through one held rank-revealing SVD into the `Tensor/blas#DENSE_ALGEBRA` route. Host-local, crossing no wire; the direction-number recurrence, gray-code Sobol draw, scrambled digit walk, Warnock discrepancy kernels, monomial enumeration, rank stratification, and the odometer walk are its sanctioned statement-form numeric kernels.

## [01]-[INDEX]

- [02]-[OWNED_BUILDS]: owned Sobol (Joe-Kuo direction numbers) / Halton (prime radical inverse) / Latin-hypercube / pseudo-random sampler over one `LowDiscrepancy` carrier; the public `Net` bulk draw; RQMC replicate family with Warnock-discrepancy net quality.
- [03]-[SCATTER_RECONSTRUCTION]: radial-basis-plus-polynomial design over the kernel `KernelKind` profiles into the held rank-revealing SVD; the `RadialFit` field.

## [02]-[OWNED_BUILDS]

- Owner: `LowDiscrepancy` the seed-explicit state-serializable carrier folding `SequenceFamily` over the per-construction table each CASE holds, a per-draw counter, and a per-dimension `ShiftSeed` key vector; `Scramble` the `[SmartEnum<string>]` randomization policy carrying its generated binary and digit columns; `SequenceFamily` the `[Union]` family discriminant carrying each leg's own table; `ReplicatePolicy` the replicate-count, confidence, net-quality, and discrepancy-sample gate; `ReplicateFamily` the RQMC estimate carrier; `JoeKuo` the direction-number recurrence over the embedded HDF5 primitive-polynomial resource; `HaltonBases` the demand-sieved prime owner.
- Cases: `SequenceFamily` cases `Sobol(uint[,] Directions)`, `Halton(int[] Bases)`, `Independent(ulong Stream)` (3 — each case IS its generation law and holds the table that law reads); `Scramble` rows none, digital-shift, owen (3).
- Entry: `Draw()` folds the `SequenceFamily` case through the generated total `Switch`; `public (LowDiscrepancy Next, NetPlane Points) Net(int count)` is the bulk draw every consumer takes — a rectangular `[count, dimensions]` plane over one granted rent, published because six consumer sites re-derived the same `(generator, point) = generator.Draw()` accumulator by hand and each re-derivation was a place the counter could drift; `LatinHypercube` draws one joint Sobol net and rank-stratifies each dimension into one point per stratum; `Replicates(LowDiscrepancy, ReplicatePolicy, Func<ReadOnlyMemory<double>, double>, Option<CorpusSink>)` runs the campaign, the sink case selecting whether the response corpus lands through `Runtime/archive#HDF_ARCHIVE`.
- Auto: `Replicates` draws exactly `2^BlockExponent` points per replicate, rejects non-finite estimator output, folds the per-replicate means through the kernel `Rasm/Domain/stats#MOMENTS` `Stat<Scalar>` receipt, and admits the Student bound and the Warnock figures through `ReplicatePolicy`; the corpus-bearing case lands one `[Replicates, 2^BlockExponent]` chunked dataset, one chunk per replicate written through the session's OWN cursor (replicate ordinal is chunk ordinal, so write-once is structural rather than an ordering argument the fold has to hold), the regenerating state (family, dimensions, seed, scramble, block exponent, replicate count) riding as scalar attributes so the corpus re-derives from its attributes alone and serializes no generator state.
- Receipt: `ReplicateFamily(Mean, CrossReplicateVariance, StudentBound, StarDiscrepancy, WorstProjection)` because a single equidistributed estimate carries no recoverable spread, and the net-quality fields make a gate reject on discrepancy rather than slow convergence; `WorstProjection` is `Option<double>` because a one-dimensional net has NO two-dimensional projection and a zero there reads as perfect uniformity on an axis pair that does not exist.
- Packages: MathNet.Numerics (`StudentT.InvCDF`), System.Numerics.Tensors, CommunityToolkit.HighPerformance, PureHDF (through the archive capsule), Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm (project, `Deterministic` the ONE draw owner), BCL inbox
- Growth: a new family is one `SequenceFamily` case carrying its own table with one `Fill*` kernel; a new scramble is one `Scramble` row; a new net-quality figure is one `ReplicateFamily` field with one kernel; zero new surface — a `SobolGenerator`/`HaltonGenerator`/`LatinHypercubeSampler` sibling family collapses onto the one `LowDiscrepancy` carrier.
- Boundary — the Sobol leg owns the Joe-Kuo recurrence over the embedded primitive-polynomial set: an all-zero direction table collapses every point to the origin, and the unscaled-`m` recurrence omitting the per-term bit-scaling yields wrong direction numbers and a plausible-looking broken net; both are rejected. The dimension ceiling is READ off the resource's own `/degree` dataspace rather than asserted as a constant beside it, so a resource regenerated at a different width refuses a request past its real extent instead of passing an assertion and faulting inside the hyperslab.
- Boundary — Halton reads its base per dimension from the `HaltonBases` prime table THIS generator holds (dimension 0 → 2) because a single shared base collapses every coordinate onto one radical-inverse sequence; that owner is separate from `JoeKuo` and its sieve is sized to the requested dimension count, so a Halton draw never forces the Sobol type initializer to load an embedded polynomial resource and sieve the Sobol dimension cap for a leg that touches no direction number. A prime sieve has NO dimension ceiling — it grows to whatever is asked — so the mirrored cap this owner once copied from the Sobol resource is deleted rather than kept in step with a constant it shares no cause with. The family discriminant is the case itself, and the deleted `Equidistributed(int Base)` numeric marker — which admitted arbitrary bases and silently routed every non-2 value to Halton — is the named incoherent-admission form, closed by the private constructor under factory-only minting.
- Boundary — `Scramble` applies uniformly across both legs so `Scramble.None` genuinely disables the binary XOR and the base-`b` digit shift, never the hardcoded `(digit + shift) % radix` that shifts even under `None`; the identity arms are ROWS, so the fast-path test a caller once wrote against `Scramble.None` is the row's own `Identity` column and not a reference comparison at a hot call; `owen` and its base-`b` linear-digit analog are the higher-quality randomization the `Growth` axis names, added as `Scramble` rows, never parallel samplers.
- Boundary — every DRAW on this lane routes through the kernel `Domain/identity#CONTENT_KEY` `Deterministic` owner: the `Independent` stream reads `Unit(lanes: [stream, drawn, dimension], seed: shift)`, the per-dimension shift key reads `Stream(lanes: [d], seed)`, the Owen ladder brackets `ReverseBits`, the digit permutation keys on `Stream(lanes: [key, position])`, and an unscrambled Halton coordinate reads `RadicalInverse(index, radix)` — so no `SplitMix64` finalizer, bit-reversal, or shifted-XOR lane pack survives here. Shifted-XOR packs are worse than a duplicate mixer: `((ulong)key << 32) ^ position` collides `(key: 0, position: 5)` with `(key: 5, position: 0)`'s neighbours, the exact defect the lane fold exists to remove, so two positions of one Owen scramble share a permutation.
- Boundary — the block exponent is accepted at the draw entrypoint because equidistribution holds only at power-of-base counts and non-power prefixes degrade discrepancy with no diagnostic; the generator is seed-explicit and state-serializable for checkpoint-resume, since thread-entropy and parallel block fill are non-deterministic regardless of seeding — the MathNet `IContinuousDistribution.Samples()` stateful stream and the `torch.manual_seed`/`torch.randn` device RNG are both named rejected draw sources on this lane for the same reason: neither serializes its state, so neither can resume a checkpointed campaign mid-stream.
- Boundary — net-quality figures are the Warnock L2 star-discrepancy and worst-2D-projection discrepancy; full-dimensional uniformity does not exclude a degenerate 2-D projection. The pair kernel is SYMMETRIC in `(i, j)`, so the sweep walks the upper triangle and doubles the off-diagonal contribution rather than paying the full square for an answer half of it already gave.
- Boundary — `Replicates` rejects `Scramble.None` over the Sobol/Halton legs: `Reseed` reaches those draws only through the scramble key, so an unscrambled equidistributed generator repeats one block per replicate and the cross-replicate variance certifies a false zero spread; the `Independent` leg replicates honestly under `None` because its counter key folds `ShiftSeed` regardless of scramble.
- Boundary — Latin-hypercube rank-stratifies a JOINT low-discrepancy draw into one point per stratum per dimension; a per-axis 1-D sequence Cartesian-producted inflates the point count and destroys the joint low-discrepancy the variance reduction depends on, and is rejected; the embedded Joe-Kuo table is an HDF5 resource read per Sobol construction through `Runtime/archive#HDF_ARCHIVE` as a `Payload` source — `/degree` and `/coefficients` rank-1 runs and a `/seeds` rank-2 hyperslab covering exactly the requested dimensions — so a three-dimension Sobol decodes three rows where the retired ASCII form parsed the whole table behind a `Lazy`, a missing or corrupt resource is a typed construction refusal rather than a cached type-initialization throw, and both legs now share the demand-sized law the Halton prime sieve already held. The archive's own `Dataset` resolve returns on the rail, so every dataset this decode reads BINDS rather than dereferences — eight composing pages call it outside any trap and a throwing resolve would publish an unrecoverable boundary to all of them.
- Boundary — nets are RECTANGULAR planes over one granted rent, never jagged arrays: `[count, dimensions]` is what the data is, so `Span2D<double>` row addressing serves every draw, stratification, discrepancy sweep, and projection, and the per-pair `double[][]` copy the projection scan once made at `d=20` — 190 full copies of the whole sample — is a column pair the plane already addresses. Every rent carries a `Tensor/memory#ALLOCATION_AXIS` `Grant`, because a campaign plane is sized by a caller's replicate policy and not by an admitted kernel operand.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class Scramble {
    public static readonly Scramble None = new("none", identity: true,
        bits: static (value, _) => value,
        digit: static (digit, _, _, _) => digit);
    public static readonly Scramble DigitalShift = new("digital-shift", identity: false,
        bits: static (value, key) => value ^ key,
        digit: static (digit, key, radix, _) => (uint)(((ulong)digit + key) % (ulong)radix));
    public static readonly Scramble Owen = new("owen", identity: false,
        bits: static (value, key) => OwenNestedUniform(value, key),
        digit: static (digit, key, radix, position) => RandomLinearDigit(digit, key, radix, position));

    public bool Identity { get; }

    [UseDelegateFromConstructor] public partial uint Bits(uint value, uint key);
    [UseDelegateFromConstructor] public partial uint Digit(uint value, uint key, int radix, int position);

    static uint OwenNestedUniform(uint value, uint key) {
        unchecked {
            uint x = Deterministic.ReverseBits(value);
            x ^= x * 0x3D20ADEAu;
            x += key;
            x *= (key >> 16) | 1u;
            x ^= x * 0x05526C56u;
            x ^= x * 0x53A22864u;
            return Deterministic.ReverseBits(x);
        }
    }

    static uint RandomLinearDigit(uint digit, uint key, int radix, int position) {
        ulong h = Deterministic.Stream(lanes: [key, position]);
        uint a = (uint)(1UL + h % (ulong)(radix - 1));
        uint c = (uint)((h >> 32) % (ulong)radix);
        return (uint)(((ulong)a * digit + c) % (ulong)radix);
    }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SequenceFamily {
    private SequenceFamily() { }

    public sealed record Sobol(uint[,] Directions) : SequenceFamily;
    public sealed record Halton(int[] Bases) : SequenceFamily;
    public sealed record Independent(ulong Stream) : SequenceFamily;

    public string Key => Switch(
        sobol: static _ => "sobol",
        halton: static _ => "halton",
        independent: static _ => "independent");

    public bool NeedsScramble => Switch(
        sobol: static _ => true, halton: static _ => true, independent: static _ => false);
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record NetPlane(MemoryOwner<double> Backing, int Count, int Dimensions, AllocationEvidence Evidence) : IDisposable {
    public Span2D<double> Points => Backing.Span.AsSpan2D(Count, Dimensions);

    public ReadOnlyMemory<double> Row(int index) => Backing.Memory.Slice(index * Dimensions, Dimensions);

    public static Fin<NetPlane> Rent(int count, int dimensions, AllocationRequest staging) =>
        count < 1 || dimensions < 1
            ? TensorReason.EmptyOperand.Fail<NetPlane>("net-extent", $"{count}x{dimensions}")
            : AllocationClass.PooledMemory
                .Rent<double>(staging with { RequestedBytes = (long)count * dimensions * sizeof(double) }, count * dimensions)
                .Map(rent => new NetPlane(rent.Buffer, count, dimensions, rent.Evidence));

    public void Dispose() => Backing.Dispose();
}

public sealed record ReplicateFamily(double Mean, double CrossReplicateVariance, double StudentBound, double StarDiscrepancy, Option<double> WorstProjection) {
    public ComputeReceipt.Sampling Receipt(LowDiscrepancy generator, ReplicatePolicy policy, WorkLane lane, CorrelationId correlation, Duration elapsed) =>
        new(Family: generator.Family.Key, Dimensions: generator.Dimensions,
            Points: (long)policy.Replicates << policy.BlockExponent, Replicates: policy.Replicates,
            StarDiscrepancy: StarDiscrepancy, WorstProjection: WorstProjection.ToNullable()) {
            Scope = new ReceiptScope.Execution(correlation, lane, Substrate.CpuTensor, AllocationClass.PooledMemory, elapsed),
        };
}

public sealed record CorpusSink(Stream Sink, HdfArchivePolicy Archive);

public sealed record ReplicatePolicy(int BlockExponent, int Replicates, double Confidence, double MaxStarDiscrepancy, double MaxProjection, int DiscrepancySample) {
    public static readonly ReplicatePolicy Default = new(
        BlockExponent: 12, Replicates: 16, Confidence: 0.95, MaxStarDiscrepancy: 0.05, MaxProjection: 0.1, DiscrepancySample: 512);

    internal Validation<Error, Unit> Admits =>
        (Gate(BlockExponent is >= 1 and <= 24, "replicate-block-exponent", BlockExponent),
         Gate(Replicates >= 2, "replicate-count", Replicates),
         Gate(double.IsFinite(Confidence) && Confidence is > 0.0 and < 1.0, "replicate-confidence", Confidence),
         Gate(double.IsFinite(MaxStarDiscrepancy) && MaxStarDiscrepancy >= 0.0, "replicate-star-cap", MaxStarDiscrepancy),
         Gate(double.IsFinite(MaxProjection) && MaxProjection >= 0.0, "replicate-projection-cap", MaxProjection),
         Gate(DiscrepancySample >= 2, "replicate-discrepancy-sample", DiscrepancySample))
            .Apply(static (_, _, _, _, _, _) => unit).As();

    private static Validation<Error, Unit> Gate<T>(bool held, string site, T value) where T : notnull =>
        held ? unit : TensorReason.PolicyInvalid.Fault(site, Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty);
}

public abstract partial record ComputeReceipt {
    public sealed record Sampling(string Family, int Dimensions, long Points, int? Replicates, double? StarDiscrepancy, double? WorstProjection) : ComputeReceipt;
}

[Equatable]
public sealed partial record LowDiscrepancy {
    public SequenceFamily Family { get; private init; }
    public Scramble Scramble { get; private init; }
    public int Dimensions { get; private init; }
    public int Seed { get; private init; }
    [OrderedEquality]
    public uint[] ShiftSeed { get; private init; }
    public long Drawn { get; private init; }

    private LowDiscrepancy(SequenceFamily family, Scramble scramble, int dimensions, int seed, uint[] shift, long drawn) =>
        (Family, Scramble, Dimensions, Seed, ShiftSeed, Drawn) = (family, scramble, dimensions, seed, shift, drawn);

    public static Fin<LowDiscrepancy> Sobol(int dimensions, int seed, Scramble scramble) =>
        JoeKuo.Directions(dimensions).Map(directions =>
            new LowDiscrepancy(new SequenceFamily.Sobol(directions), scramble, dimensions, seed, ShiftFor(dimensions, seed), 0L));

    public static Fin<LowDiscrepancy> Halton(int dimensions, int seed, Scramble scramble) =>
        dimensions >= 1
            ? Fin.Succ(new LowDiscrepancy(new SequenceFamily.Halton(HaltonBases.Primes(dimensions)), scramble, dimensions, seed, ShiftFor(dimensions, seed), 0L))
            : TensorReason.EmptyOperand.Fail<LowDiscrepancy>("halton-dimensions", dimensions.ToString(CultureInfo.InvariantCulture));

    public static Fin<LowDiscrepancy> Pseudo(int dimensions, int seed, ulong stream, Scramble scramble) =>
        dimensions >= 1
            ? Fin.Succ(new LowDiscrepancy(new SequenceFamily.Independent(stream), scramble, dimensions, seed, ShiftFor(dimensions, seed), 0L))
            : TensorReason.EmptyOperand.Fail<LowDiscrepancy>("pseudo-dimensions", dimensions.ToString(CultureInfo.InvariantCulture));

    public (LowDiscrepancy Next, double[] Point) Draw() {
        double[] point = Family.Switch(
            state: this,
            sobol: static (self, s) => self.SobolPoint(s.Directions),
            halton: static (self, h) => self.HaltonPoint(h.Bases),
            independent: static (self, i) => self.FillIndependent(i.Stream));
        return (this with { Drawn = Drawn + 1 }, point);
    }

    public Fin<(LowDiscrepancy Next, NetPlane Points)> Net(int count, AllocationRequest staging) =>
        NetPlane.Rent(count, Dimensions, staging).Map(plane => {
            LowDiscrepancy generator = this;
            Span2D<double> rows = plane.Points;
            for (int i = 0; i < count; i++) {
                (generator, double[] point) = generator.Draw();
                point.AsSpan().CopyTo(rows.GetRowSpan(i));
            }

            return (generator, plane);
        });

    public static Fin<NetPlane> LatinHypercube(int dimensions, int count, int seed, Scramble scramble, AllocationRequest staging) =>
        Sobol(dimensions, seed, scramble)
            .Bind(generator => generator.Net(count, staging))
            .Map(drawn => { Stratify(drawn.Points); return drawn.Points; });

    public static Fin<ReplicateFamily> Replicates(
        LowDiscrepancy generator, ReplicatePolicy policy, Func<ReadOnlyMemory<double>, double> estimator,
        Option<CorpusSink> corpus, AllocationRequest staging) =>
        Admit(generator, policy).Bind(_ => corpus.Match(
            None: () => Campaign(generator, policy, estimator, None, staging),
            Some: sink => Corpus(generator, policy, sink)
                .Bind(slot => ArchiveSession.Write(
                    sink.Sink, sink.Archive, Seq<IArchiveSlot>(slot.Slot), slot.Attributes,
                    session => IO.pure(session.Cursor(slot.Slot)
                        .Bind(cursor => Campaign(generator, policy, estimator, Some(cursor), staging))))
                    .Run())));

    static Fin<(ArchiveSlot<double> Slot, Seq<(string Key, ArchiveAttribute Value)> Attributes)> Corpus(
        LowDiscrepancy generator, ReplicatePolicy policy, CorpusSink sink) {
        int count = 1 << policy.BlockExponent;
        return ChunkGrid.Seat(fileDims: [(ulong)policy.Replicates, (ulong)count], chunks: [1u, (uint)count])
            .Map(grid => (new ArchiveSlot<double>("responses", grid), Seq(
                ("family", (ArchiveAttribute)new ArchiveAttribute.Text(generator.Family.Key)),
                ("dimensions", (ArchiveAttribute)new ArchiveAttribute.Whole(generator.Dimensions)),
                ("seed", (ArchiveAttribute)new ArchiveAttribute.Whole(generator.Seed)),
                ("scramble", (ArchiveAttribute)new ArchiveAttribute.Text(generator.Scramble.Key)),
                ("block-exponent", (ArchiveAttribute)new ArchiveAttribute.Whole(policy.BlockExponent)),
                ("replicates", (ArchiveAttribute)new ArchiveAttribute.Whole(policy.Replicates)))));
    }

    static Fin<Unit> Admit(LowDiscrepancy generator, ReplicatePolicy policy) =>
        policy.Admits.ToFin().Bind(_ =>
            generator.Scramble.Identity && generator.Family.NeedsScramble
                ? TensorReason.PolicyInvalid.Fail<Unit>("replicate-unscrambled-equidistributed", generator.Family.Key)
                : Fin.Succ(unit));

    static Fin<ReplicateFamily> Campaign(
        LowDiscrepancy generator, ReplicatePolicy policy, Func<ReadOnlyMemory<double>, double> estimator,
        Option<ChunkCursor<double>> corpus, AllocationRequest staging) =>
        toSeq(Enumerable.Range(0, policy.Replicates))
            .Traverse(r => Block(generator.Reseed(r), 1 << policy.BlockExponent, policy.DiscrepancySample, estimator, staging)
                .Bind(block => corpus.Match(
                    None: () => Fin.Succ(block),
                    Some: cursor => cursor.Write(block.Values).Map(_ => block))))
            .As()
            .Bind(blocks => Settle(blocks, policy, key));

    static Fin<ReplicateFamily> Settle(Seq<ReplicateBlock> blocks, ReplicatePolicy policy, Op key) {
        Fin<Stat<Scalar>> folded = Stat<Scalar>.Of(blocks.Map(static block => (Scalar)TensorPrimitives.Average<double>(block.Values)), key);
        if (folded.Case is not Stat<Scalar> stat) { return folded.Map(static _ => default(ReplicateFamily)!); }
        double variance = stat.Variance(MomentNormalizer.Sample);
        double bound = StudentT.InvCDF(0.0, 1.0, policy.Replicates - 1, 0.5 + (policy.Confidence / 2.0)) * Math.Sqrt(variance / policy.Replicates);
        double star = blocks.Map(static block => block.Star).Fold(double.NegativeInfinity, Math.Max);
        Option<double> projection = blocks.Map(static block => block.Projection).Somes() is var reported && reported.IsEmpty
            ? None
            : Some(reported.Fold(double.NegativeInfinity, Math.Max));
        return double.IsFinite(stat.Mean) && double.IsFinite(variance) && double.IsFinite(bound) && double.IsFinite(star)
            && star <= policy.MaxStarDiscrepancy && projection.ForAll(p => double.IsFinite(p) && p <= policy.MaxProjection)
                ? Fin.Succ(new ReplicateFamily(stat.Mean, variance, bound, star, projection))
                : TensorReason.WitnessFail.Fail<ReplicateFamily>("replicate-evidence",
                    $"variance={variance:e3}", $"bound={bound:e3}", $"star={star:e3}");
    }

    readonly record struct ReplicateBlock(double[] Values, double Star, Option<double> Projection);

    LowDiscrepancy Reseed(int replicate) =>
        this with { ShiftSeed = ShiftFor(Dimensions, unchecked(Seed + replicate)), Drawn = 0L };

    static Fin<ReplicateBlock> Block(LowDiscrepancy generator, int count, int sample, Func<ReadOnlyMemory<double>, double> estimator, AllocationRequest staging) =>
        generator.Net(count, staging).Bind(drawn => {
            using NetPlane net = drawn.Points;
            double[] values = new double[count];
            for (int i = 0; i < count; i++) { values[i] = estimator(net.Row(i)); }
            return !TensorPrimitives.IsFiniteAll<double>(values)
                ? TensorReason.NonFinite.Fail<ReplicateBlock>("replicate-estimator")
                : Gauged(net, sample, generator.Seed, staging).Map(gauge =>
                    new ReplicateBlock(values, StarDiscrepancyL2(gauge.Points), WorstProjection(gauge.Points, staging)));
        });

    static Fin<NetView> Gauged(NetPlane net, int sample, int seed, AllocationRequest staging) {
        if (net.Count <= sample) { return Fin.Succ(new NetView(net, None)); }
        int[] order = [.. Enumerable.Range(0, net.Count)];
        for (int slot = 0; slot < sample; slot++) {
            int pick = slot + (int)(Deterministic.Stream(lanes: [slot, net.Count], seed: seed) % (ulong)(net.Count - slot));
            (order[slot], order[pick]) = (order[pick], order[slot]);
        }

        return NetPlane.Rent(sample, net.Dimensions, staging).Map(picked => {
            Span2D<double> source = net.Points, sink = picked.Points;
            for (int slot = 0; slot < sample; slot++) { source.GetRowSpan(order[slot]).CopyTo(sink.GetRowSpan(slot)); }
            return new NetView(picked, Some(picked));
        });
    }

    readonly record struct NetView(NetPlane Points, Option<NetPlane> Owned) : IDisposable {
        public void Dispose() => Owned.Iter(static plane => plane.Dispose());
    }

    double[] SobolPoint(uint[,] directions) {
        double[] point = new double[Dimensions];
        uint gray = unchecked((uint)(Drawn ^ (Drawn >> 1)));
        for (int d = 0; d < Dimensions; d++) {
            uint state = 0u;
            for (int bit = 0; bit < JoeKuo.Bits && (gray >> bit) != 0u; bit++) {
                if (((gray >> bit) & 1u) != 0u) { state ^= directions[d, bit]; }
            }

            point[d] = Scramble.Bits(state, ShiftSeed[d]) * Math.ScaleB(1.0, -32);
        }

        return point;
    }

    double[] HaltonPoint(int[] bases) {
        double[] point = new double[Dimensions];
        ulong index = unchecked((ulong)Drawn) + 1UL;
        for (int d = 0; d < Dimensions; d++) {
            point[d] = RadicalInverse(index, bases[d], ShiftSeed[d], Scramble);
        }

        return point;
    }

    double[] FillIndependent(ulong stream) {
        double[] point = new double[Dimensions];
        for (int d = 0; d < Dimensions; d++) {
            point[d] = Deterministic.Unit(lanes: [(long)stream, Drawn, d], seed: ShiftSeed[d]);
        }

        return point;
    }

    static double RadicalInverse(ulong index, int radix, uint key, Scramble scramble) {
        if (scramble.Identity) { return Deterministic.RadicalInverse((uint)index, radix); }
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

    static void Stratify(Span2D<double> net) {
        int count = net.Height, dims = net.Width;
        int[] order = new int[count];
        double[] column = new double[count];
        for (int d = 0; d < dims; d++) {
            for (int i = 0; i < count; i++) { (order[i], column[i]) = (i, net[i, d]); }
            column.AsSpan().Sort(order.AsSpan());
            for (int rank = 0; rank < count; rank++) { net[order[rank], d] = (rank + column[rank]) / count; }
        }
    }

    static double StarDiscrepancyL2(NetPlane net) {
        Span2D<double> points = net.Points;
        int n = net.Count, d = net.Dimensions;
        double single = 0.0;
        for (int i = 0; i < n; i++) {
            double prod = 1.0;
            ReadOnlySpan<double> row = points.GetRowSpan(i);
            for (int k = 0; k < d; k++) { prod *= 1.0 - (row[k] * row[k]); }
            single += prod;
        }

        double pair = 0.0, diagonal = 0.0;
        for (int i = 0; i < n; i++) {
            ReadOnlySpan<double> left = points.GetRowSpan(i);
            for (int j = i; j < n; j++) {
                ReadOnlySpan<double> right = points.GetRowSpan(j);
                double prod = 1.0;
                for (int k = 0; k < d; k++) { prod *= 1.0 - Math.Max(left[k], right[k]); }
                if (i == j) { diagonal += prod; } else { pair += prod; }
            }
        }

        double term = Math.Pow(1.0 / 3.0, d) - (Math.ScaleB(1.0, 1 - d) / n * single) + (((2.0 * pair) + diagonal) / ((double)n * n));
        return Math.Sqrt(Math.Max(0.0, term));
    }

    static Option<double> WorstProjection(NetPlane net, AllocationRequest staging) {
        int d = net.Dimensions;
        if (d < 2) { return None; }
        return NetPlane.Rent(net.Count, 2, staging).Match(
            Fail: static _ => Option<double>.None,
            Succ: pair => {
                using (pair) {
                    Span2D<double> source = net.Points, plane = pair.Points;
                    double worst = 0.0;
                    for (int a = 0; a < d; a++) {
                        for (int b = a + 1; b < d; b++) {
                            for (int i = 0; i < net.Count; i++) {
                                Span<double> row = plane.GetRowSpan(i);
                                (row[0], row[1]) = (source[i, a], source[i, b]);
                            }

                            worst = Math.Max(worst, StarDiscrepancyL2(pair));
                        }
                    }

                    return Some(worst);
                }
            });
    }

    static uint[] ShiftFor(int dimensions, int seed) =>
        toSeq(Enumerable.Range(0, dimensions))
            .Map(d => unchecked((uint)(Deterministic.Stream(lanes: [d], seed: seed) >> 32)))
            .ToArray();
}

public static class HaltonBases {
    static readonly Atom<int[]> Cached = Atom(Array.Empty<int>());

    public static int[] Primes(int dimensions) =>
        Cached.Swap(held => held.Length >= dimensions ? held : Sieve(dimensions));

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

public static class JoeKuo {
    public const int Bits = 32;

    private const string Resource = "Rasm.Compute.new-joe-kuo-6.21201.h5";

    public static Fin<uint[,]> Directions(int dimensions) =>
        dimensions < 1
            ? TensorReason.EmptyOperand.Fail<uint[,]>("sobol-dimensions", dimensions.ToString(CultureInfo.InvariantCulture))
            : Payload().Bind(bytes => HdfArchive.Session(new HdfSource.Payload(bytes), HdfArchivePolicy.Interchange, handle =>
                IO.pure(Decode(handle, dimensions))).Run());

    static Fin<uint[,]> Decode(HdfHandle handle, int dimensions) =>
        from degrees in handle.Dataset("degree")
        from ceiling in Ceiling(degrees, dimensions)
        from coefficients in handle.Dataset("coefficients")
        from seeds in handle.Dataset("seeds")
        from table in Op.Of(name: "joe-kuo-decode").Catch(() => {
            int rows = dimensions - 1;
            int[] degree = new int[rows];
            uint[] polynomials = new uint[rows];
            uint[] payload = new uint[rows * Bits];
            if (rows > 0) {
                degrees.Read<int>(handle.Access, degree.AsSpan(), new HyperslabSelection(0, (ulong)rows));
                coefficients.Read<uint>(handle.Access, polynomials.AsSpan(), new HyperslabSelection(0, (ulong)rows));
                seeds.Read<uint>(handle.Access, payload.AsSpan(), new HyperslabSelection(2, [0UL, 0UL], [(ulong)rows, Bits]));
            }

            return Fin.Succ(Recur(dimensions, degree, polynomials, payload));
        })
        select table;

    static Fin<Unit> Ceiling(NativeDataset degrees, int dimensions) =>
        (long)dimensions <= (long)degrees.Space.Dimensions[0] + 1
            ? Fin.Succ(unit)
            : TensorReason.StagingOverBound.Fail<Unit>("sobol-dimension-bound",
                $"{dimensions}>{degrees.Space.Dimensions[0] + 1}");

    static uint[,] Recur(int dimensions, int[] degree, uint[] coefficients, uint[] seeds) {
        uint[,] v = new uint[dimensions, Bits];
        for (int k = 0; k < Bits; k++) { v[0, k] = 1u << (Bits - 1 - k); }
        for (int d = 1; d < dimensions; d++) {
            int order = degree[d - 1];
            uint polynomial = coefficients[d - 1];
            for (int i = 0; i < order && i < Bits; i++) { v[d, i] = seeds[((d - 1) * Bits) + i] << (Bits - 1 - i); }
            for (int j = order; j < Bits; j++) {
                uint value = v[d, j - order] ^ (v[d, j - order] >> order);
                for (int k = 1; k < order; k++) {
                    if (((polynomial >> (order - 1 - k)) & 1u) != 0u) { value ^= v[d, j - k]; }
                }

                v[d, j] = value;
            }
        }

        return v;
    }

    static Fin<ReadOnlyMemory<byte>> Payload() =>
        Op.Of(name: "joe-kuo-resource").Catch(() =>
            Optional(typeof(JoeKuo).Assembly.GetManifestResourceStream(Resource))
                .ToFin(TensorReason.RowMissing.Fault("joe-kuo-resource", Resource))
                .Map(static stream => {
                    using (stream) {
                        using MemoryStream staged = new();
                        stream.CopyTo(staged);
                        return (ReadOnlyMemory<byte>)staged.ToArray();
                    }
                }));
}
```

## [03]-[SCATTER_RECONSTRUCTION]

- Owner: the owned-build reconstruction lane over the host-free numeric arena — `RbfDesign`/`RbfFit` the design and fitted-field carriers; `RadialFit` the augmented-design constructor, the held rank-revealing reconstruction, and the field fit. `Numerics/calculus#WEIGHT_PROFILES` `KernelKind` IS the radial vocabulary, composed whole — its rows carry value, first and second derivatives, a `DerivativeSupremum` slope bound, and the `PolynomialOrder` reproduction tier this lane reads.
- Cases: the radial rows are the kernel's — gaussian · inverse-multiquadric · wendland (strictly positive-definite, `PolynomialOrder` 0) · multiquadric (constant tier 1) · polyharmonic-cubic · thin-plate-spline (linear tier 2, conditionally positive-definite) beside the compact-support weight rows the reconstruction never selects.
- Entry: `public static Fin<RbfDesign> Design(Matrix<double> centres, Matrix<double> samples, KernelKind kernel, double radius)` builds the augmented radial-basis-plus-polynomial design; `public static Fin<Matrix<double>> Reconstruct(Matrix<double> design, Matrix<double> response, TolerancePolicy tol)` solves a matrix-valued response through one held SVD into the `RankRevealing` route; `public static Fin<RbfFit> Fit(Matrix<double> centres, Matrix<double> samples, Matrix<double> response, KernelKind kernel, double radius, TolerancePolicy tol)` composes design and solve into a fitted field, `RbfFit.Evaluate` projecting a query batch.
- Auto: `Design` builds the `Φ` block `Φ_ij = kernel.Weight(‖xᵢ − cⱼ‖, radius)` and, for a conditionally-positive-definite kernel (`PolynomialOrder ≥ 1`), augments to the saddle system `[Φ P; Pᵀ 0]` over the monomial reproduction basis up to total degree `PolynomialOrder − 1`; every `RadialFit` MathNet boundary is captured and finite-gated; `Reconstruct` decomposes the design once through `Tensor/blas#DENSE_ALGEBRA` `DenseOps.Decompose(design, FactorizationKind.Svd)`, solves every response column through the held `ISolver<double>.Solve(Matrix<double>)`, and witnesses the Frobenius residual against the original design; `Fit` pads the response with polynomial side-constraint zero rows and splits the one solution into RBF weights and polynomial coefficients.
- Receipt: the numeric decomposition rides the `Tensor/blas#DENSE_ALGEBRA` `Factorization` `ComputeReceipt` evidence the held SVD stamps, while the FIT itself projects `RbfFit.Receipt` onto the `ComputeReceipt.Sampling` case `[02]-[OWNED_BUILDS]` declares — the centres are its point set, the radial row its stated family, and every RQMC column reports absence; the `RbfFit` carries the centres, kernel, shape, weights, and polynomial coefficients, and its content key is the `Rasm/Domain/identity#CONTENT_KEY` `ContentHash.Of` digest over that carrier's own canonical bytes, so the "content-keyable" claim has a producer rather than a promise.
- Packages: MathNet.Numerics, System.Numerics.Tensors, Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm (project, `KernelKind` the one radial-profile vocabulary, `ContentHash` the one digest owner), BCL inbox
- Growth: a new radial kernel is one `KernelKind` row at the kernel owner, reaching this lane with zero edit here; zero new surface — a per-kernel design function family collapses onto the one `RadialFit.Design`.
- Boundary — naming: this owner is `RadialFit`, not `Scatter`. Three unrelated "scatter" senses met inside one folder namespace — the tensor-lane `TensorOpFamily.Scatter` structural write, the model-lane `TileScatter` tile fan-out, and this radial reconstruction — and none of the three was the others' caller, so the one whose noun was least earned takes the name its domain already has.
- Boundary — interpolation: ONE-dimensional interpolation is the kernel's whole. `Rasm/Numerics/transform#INTERPOLATE` owns `Interpolant<TCap>` — the capability-typed capsule whose type parameter lifts the package's two runtime support flags into a compile-time capability, with `Interpolant.CubicSpline`/`CubicSplineRobust`/`CubicSplineMonotone`/`Hermite`/`Linear`/`Step` minting `Smooth`, `Polynomial`/`LogLinear` minting `Differentiable`, and `Common`/`RationalWithoutPoles`/`RationalWithPoles`/`PolynomialEquidistant` minting `Sampled`. That owner adopted this lane's compile-time capability form whole and carries twelve schemes where this page minted eight over a strict subset, so the local capsule, its three marker interfaces, its three phantom tier structs, its eight sibling factories, its shared `Build` rail, and its absence-carrying `Read` shell are all DELETED rather than kept beside it. NAMED LOSS: the local `At`/`Slope`/`Curvature`/`Area` reads answered `Option<double>`, where the kernel answers `Fin<double>` under an `Op` key — absence becomes a keyed refusal, which is strictly more evidence and one more thing a caller must thread. WITNESS: `Interpolant.CubicSpline(nodes, values).Bind(curve => curve.Area(to))` replaces `Interpolant.CubicSpline(nodes, values).Map(curve => curve.Area(a, b))`, and the tier that made `Area` compile is the same tier under the same name.
- Boundary — reconstruction: scattered reconstruction is the owned radial-basis-plus-polynomial design, and the polynomial tail is genuine (`KernelKind.PolynomialOrder` drives the `[Φ P; Pᵀ 0]` saddle augmentation the conditionally-positive-definite kernels require for a unique interpolant) — a bare `Φ` block claiming the polynomial reproduction the prose advertises is the deleted form. The OWNERSHIP holds on three discriminants against the kernel sibling, never on library absence alone, because the kernel `Meshing/reconstruct#RECONSTRUCTION` `ReconstructionPolicy.Sibson` natural-neighbour interpolant IS a landed scattered interpolant — it is `Point3d`-typed host geometry the `ARCHITECTURE.md` `[06]` boundary bars from an interior `Tensor` signature, it is scalar-valued over three coordinates where this design is matrix-valued over `centres.ColumnCount`, and it is EVALUATED per query off two Voronoi duals where this design is FITTED into content-keyable coefficients its one consumer (`Solver/optimizer#OPTIMIZER_LANE` `RbfModel`) predicts through in a search inner loop; the routing is therefore total and needs no row here. The radial VOCABULARY is the kernel's alone, since a package-local `RbfKernel` spelling `Wendland` beside the kernel's own row is a same-named twin between strata peers and forks the profile the moment either end tunes a shape parameter — the kernel row carries first and second derivatives beside a slope bound this lane's Hessian-aware consumers reach without a second family. The reconstruction decomposes the design ONCE into a held SVD and solves the matrix-valued response through the one handle per the `Tensor/blas#DENSE_ALGEBRA` held-handle law — a fresh `DenseRoute.Solve` per response column paying a cubic SVD each time is the deleted form; a `Func<double, double>` riding beside the design is the rejected form because both the profile and its reproduction tier are row data the kernel vocabulary owns; the reconstruction witnesses the Frobenius residual against the original design through the `TolerancePolicy.Admits` gate because the SVD pseudo-inverse certifies only the least-squares minimum, not a usable interpolant, and a rank-deficient design under a loose shape parameter passes the solve while failing the field; the lane is host-local and the radial design composes `MathNet` `Matrix<double>` directly — a package-local matrix wrapper is the deleted form mirroring the blas-lane no-`RasmMatrix` law; the operand gate is the blas lane's own `OperandGate.Admit`, so a design and a response cross the same finite/symmetry gate every dense solve on the branch crosses.

```csharp
// --- [MODELS] --------------------------------------------------------------------------
public sealed record RbfDesign(Matrix<double> Matrix, int PolynomialTerms, int Centres);

public sealed record RbfFit(Matrix<double> Centres, KernelKind Kernel, double Radius, Matrix<double> Weights, Matrix<double> PolynomialCoefficients) {
    public int PolynomialOrder => Kernel.PolynomialOrder;

    public ContentHash Key =>
        ContentHash.Of(MemoryMarshal.AsBytes<double>([
            .. Centres.ToColumnMajorArray(), Radius,
            .. Weights.ToColumnMajorArray(), .. PolynomialCoefficients.ToColumnMajorArray()]));

    public ComputeReceipt.Sampling Receipt(WorkLane lane, CorrelationId correlation, Duration elapsed) =>
        new(Family: Kernel.Key, Dimensions: Centres.ColumnCount, Points: Centres.RowCount,
            Replicates: null, StarDiscrepancy: null, WorstProjection: null) {
            Scope = new ReceiptScope.Execution(correlation, lane, Substrate.CpuTensor, AllocationClass.PooledMemory, elapsed),
        };

    public Fin<Matrix<double>> Evaluate(Matrix<double> queries) =>
        queries.RowCount == 0 || queries.ColumnCount != Centres.ColumnCount
            ? TensorReason.ShapeMismatch.Fail<Matrix<double>>("rbf-query-shape", $"{queries.RowCount}x{queries.ColumnCount}")
            : OperandGate.Admit(queries).Bind(admitted => Op.Of(name: "rbf-evaluate").Catch(() => {
                Matrix<double> phi = Matrix<double>.Build.Dense(admitted.RowCount, Centres.RowCount,
                    (i, j) => Kernel.Weight(TensorPrimitives.Distance<double>(admitted.Row(i).AsArray(), Centres.Row(j).AsArray()), Radius));
                Matrix<double> field = phi.Multiply(Weights);
                if (PolynomialOrder <= 0) { return Fin.Succ(field); }
                Seq<int[]> terms = RadialFit.Monomials(admitted.ColumnCount, PolynomialOrder - 1);
                Matrix<double> poly = Matrix<double>.Build.Dense(admitted.RowCount, terms.Count, (i, t) => RadialFit.Evaluate(admitted.Row(i), terms[t]));
                return Fin.Succ(field + poly.Multiply(PolynomialCoefficients));
            }))
            .Bind(static field => TensorPrimitives.IsFiniteAll<double>(field.AsColumnMajorArray())
                ? Fin.Succ(field)
                : TensorReason.NonFinite.Fail<Matrix<double>>("rbf-evaluate"));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class RadialFit {
    public static Fin<RbfDesign> Design(Matrix<double> centres, Matrix<double> samples, KernelKind kernel, double radius) =>
        (Congruent(centres, samples), Positive(radius))
            .Apply(static (_, _) => unit).As().ToFin()
            .Bind(_ => OperandGate.Admit(centres))
            .Bind(_ => OperandGate.Admit(samples))
            .Bind(_ => Op.Of(name: "rbf-design").Catch(() => {
                Matrix<double> phi = Matrix<double>.Build.Dense(samples.RowCount, centres.RowCount,
                    (i, j) => kernel.Weight(TensorPrimitives.Distance<double>(samples.Row(i).AsArray(), centres.Row(j).AsArray()), radius));
                if (kernel.PolynomialOrder <= 0) { return Fin.Succ(new RbfDesign(phi, 0, centres.RowCount)); }

                Seq<int[]> terms = Monomials(samples.ColumnCount, kernel.PolynomialOrder - 1);
                Matrix<double> pSamples = Matrix<double>.Build.Dense(samples.RowCount, terms.Count, (i, t) => Evaluate(samples.Row(i), terms[t]));
                Matrix<double> pCentres = Matrix<double>.Build.Dense(centres.RowCount, terms.Count, (j, t) => Evaluate(centres.Row(j), terms[t]));
                Matrix<double> top = phi.Append(pSamples);
                Matrix<double> bottom = pCentres.Transpose().Append(Matrix<double>.Build.Dense(terms.Count, terms.Count));
                return Fin.Succ(new RbfDesign(top.Stack(bottom), terms.Count, centres.RowCount));
            }))
            .Bind(static design => TensorPrimitives.IsFiniteAll<double>(design.Matrix.AsColumnMajorArray())
                ? Fin.Succ(design)
                : TensorReason.NonFinite.Fail<RbfDesign>("rbf-design"));

    public static Fin<Matrix<double>> Reconstruct(Matrix<double> design, Matrix<double> response, TolerancePolicy tol) =>
        design.RowCount != response.RowCount || response.ColumnCount == 0
            ? TensorReason.ShapeMismatch.Fail<Matrix<double>>("scatter-response-shape", $"{design.RowCount}!={response.RowCount}")
            : OperandGate.Admit(design)
                .Bind(_ => OperandGate.Admit(response))
                .Bind(_ => DenseOps.Decompose(design, FactorizationKind.Svd))
                .Bind(factor => Op.Of(name: "scatter-solve").Catch(() => Fin.Succ(factor.Solve(response))))
                .Bind(solution => (design.Multiply(solution) - response).FrobeniusNorm() / Math.Max(1.0, response.FrobeniusNorm()) is var residual
                    && tol.Admits(residual)
                        ? Fin.Succ(solution)
                        : TensorReason.WitnessFail.Fail<Matrix<double>>("scatter-witness", $"r={residual:e3}"));

    public static Fin<RbfFit> Fit(Matrix<double> centres, Matrix<double> samples, Matrix<double> response, KernelKind kernel, double radius, TolerancePolicy tol) =>
        samples.RowCount != response.RowCount
            ? TensorReason.ShapeMismatch.Fail<RbfFit>("rbf-response-shape", $"{samples.RowCount}!={response.RowCount}")
            : Design(centres, samples, kernel, radius).Bind(design =>
                Reconstruct(design.Matrix, Pad(response, design.PolynomialTerms), tol).Map(solution =>
                    new RbfFit(centres, kernel, radius,
                        solution.SubMatrix(0, design.Centres, 0, solution.ColumnCount),
                        design.PolynomialTerms == 0
                            ? Matrix<double>.Build.Dense(0, solution.ColumnCount)
                            : solution.SubMatrix(design.Centres, design.PolynomialTerms, 0, solution.ColumnCount))));

    private static Validation<Error, Unit> Congruent(Matrix<double> centres, Matrix<double> samples) =>
        centres.RowCount > 0 && samples.RowCount > 0 && centres.ColumnCount > 0 && centres.ColumnCount == samples.ColumnCount
            ? unit
            : TensorReason.ShapeMismatch.Fault("rbf-design-shape", $"{centres.RowCount}x{centres.ColumnCount}", $"{samples.RowCount}x{samples.ColumnCount}");

    private static Validation<Error, Unit> Positive(double radius) =>
        double.IsFinite(radius) && radius > 0.0
            ? unit
            : TensorReason.PolicyInvalid.Fault("rbf-radius", radius.ToString("e3", CultureInfo.InvariantCulture));

    static Matrix<double> Pad(Matrix<double> response, int polynomialTerms) =>
        polynomialTerms == 0 ? response : response.Stack(Matrix<double>.Build.Dense(polynomialTerms, response.ColumnCount));

    public static Seq<int[]> Monomials(int dimension, int order) => toSeq(Compositions(dimension, order));

    public static double Evaluate(Vector<double> point, int[] exponents) {
        double product = 1.0;
        for (int k = 0; k < exponents.Length; k++) {
            double value = point[k];
            product *= exponents[k] switch { 0 => 1.0, 1 => value, 2 => value * value, var n => Math.Pow(value, n) };
        }

        return product;
    }

    static IEnumerable<int[]> Compositions(int slots, int maxTotal) {
        if (slots <= 0) { yield return []; yield break; }
        int[] cursor = new int[slots];
        int total = 0;
        while (true) {
            yield return [.. cursor];
            int axis = slots - 1;
            while (axis >= 0 && total >= maxTotal) {
                total -= cursor[axis];
                cursor[axis] = 0;
                axis--;
            }

            if (axis < 0) { yield break; }
            cursor[axis]++;
            total++;
        }
    }
}
```

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
