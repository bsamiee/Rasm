# [RASM_IDENTITY]

`Rasm.Domain` owns the kernel's two reproducibility surfaces with no sibling between them: `ContentHash`, the federation content key over caller-canonical bytes, and `Deterministic`, the one splitmix64 owner supplying order keys, unit-interval draws, and signed-unit streams to every reproducible algorithm. Neither is cryptographic.

Identity and derivation never cross: a content key built from a `Deterministic` order key, or a sampler seeded from a `ContentHash`, is rejected by design. Every federation partner reproduces the zero-fixed seed byte-for-byte, so one content space addresses across packages and runtimes.

## [01]-[INDEX]

- [02]-[CONTENT_KEY]: `ContentHash` mints the seed-zero `XxHash128` federation content key.
- [03]-[DETERMINISTIC_DERIVATION]: `Deterministic` owns order keys, unit draws, and signed-unit streams off one splitmix64 finalizer.

## [02]-[CONTENT_KEY]

- Owner: `ContentHash` static class — one algorithm, seed zero; THE federation content key every partner composes. Caller owns the canonical byte projection, this owns the digest, so identity is byte-stable across packages and runtimes.
- Entry: one name over two input shapes — `Of(ReadOnlySpan<byte> canonicalBytes)` folds a payload one span spans, `Of<TState>(TState state, Action<TState, XxHash128> chunks)` folds an ordered chunk stream through one seeded accumulator; both close on `UInt128` at seed zero, so the streaming leg is the same identity space and never a second key.
- Law: canonicalization is the caller's proof — this entry hashes the bytes it is handed, so byte-stable member order, numeric normalization, and encoding are the projecting owner's obligation, and two semantically equal values with divergent canonical projections are two identities.
- Law: chunk ORDER is the streaming leg's canonical projection — the accumulator is order-sensitive, so a producer emits its chunks under one declared traversal and a reordered emission is a different identity, exactly as a reordered span is.
- Law: a payload whose byte length exceeds `int` range reaches no `ReadOnlySpan<byte>` at all, so the streaming leg is the only spelling for a large plane, tile arena, or segmented artifact; picking the one-shot there is unrepresentable rather than slow.
- Packages: `System.IO.Hashing` (`XxHash128.HashToUInt128` static one-shot, `XxHash128(long)` seeded construction, `Append`, `GetCurrentHashAsUInt128`; MIT, managed, no native asset).
- Growth: a new ingress shape lands as one overload on this entry name; a second hashing owner beside it forks the federation seed every partner reproduces.
- Boundary: `UInt128` is the identity currency; wire and storage encodings (hex, two-lane `ulong`, byte order) are boundary projections at the consuming seam.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using System.IO.Hashing;
using Rasm.Csp;

namespace Rasm.Domain;

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class ContentHash {
    [BoundaryAdapter] public static UInt128 Of(ReadOnlySpan<byte> canonicalBytes) => XxHash128.HashToUInt128(canonicalBytes);

    // Chunk legs spell seed zero explicitly because the accumulator's construction takes it, where the one-shot
    // defaults it — one seed, two spellings, and a divergence here forks every partner's reproduction of the key.
    [BoundaryAdapter]
    public static UInt128 Of<TState>(TState state, Action<TState, XxHash128> chunks) {
        XxHash128 accumulator = new(seed: 0L);
        chunks(state, accumulator);
        return accumulator.GetCurrentHashAsUInt128();
    }
}
```

## [03]-[DETERMINISTIC_DERIVATION]

- Owner: `Deterministic` static class — the one splitmix64 owner: `Mix` (finalizer) and `Advance` (golden-gamma stream) are the private mechanism, the public family is the unit draws, order keys, clamped intervals, lane-keyed draws, the bounded integer draw, the equidistributed family, and the one `System.Random` adapter; the mixer is unreachable outside the owner.
- Entry: three modalities by input shape — stream sampling advances a `ref ulong state` seeded by the consuming algorithm's named policy seed (`NextSignedUnit` for real bases, `NextSignedComplexUnit` for Hermitian, `NextBelow` for an unbiased bounded index); coordinate keying is stateless (`OrderKey(coordinates, seed)`, the `Point3d` overload routing into the span floor, `UnitInterval(point, salt, seed)` for per-point draws); lane keying is stateless over integer lanes (`Stream(lanes, seed)` mints a threadable state, `Unit(lanes, seed)` projects one clamped draw). `Source(seed, lanes)` is THE one adapter for a package API whose SIGNATURE demands `System.Random` — it ADDS the replay guarantee the BCL type cannot carry, and it is the only sanctioned crossing.
- Law: coordinate keys normalize the signed zero — `-0.0` projects to `+0.0` before bit extraction so the two zeros key identically, and the seed widens unsigned (`(uint)seed`) so a negative seed never sign-extends into the state.
- Law: unit projections take the top 53 bits (`>> 11`, scaled `2^-53`) for an exact double; `UnitInterval` clamps to `[EpsilonPolicy.SqrtEpsilon, 1 - EpsilonPolicy.SqrtEpsilon]` — the one named epsilon owner — so log-weighted rejection draws (`-log(u) / weight`) stay finite at both ends.
- Cases: consumers by member — the matrix eigensolver's LOBPCG starting bases (`NextSignedUnit`/`NextSignedComplexUnit` under its named basis-seed policy), the sampler's candidate ordering, active-set rotation, annulus, and weighted-rejection draws (`OrderKey`/`UnitInterval`), the fit consensus sampler's minimal-set draws (`NextBelow` over a `Stream`-minted state), per-(stream, ordinal, dimension) texel and jitter draws (`Unit`), Halton and Sobol coordinates (`RadicalInverse`/`ReverseBits`/`Hammersley`), MathNet distribution constructors (`Source`), and any reproducible tie-break in the processing suite (`OrderKey`).
- Growth: a new reproducible draw shape is one member on this owner composing `Advance`/`OrderKey`.
- Boundary: the span fold in `OrderKey` and the state-advancing `ref` members are the named kernel exemption; no member reads time, thread identity, or process state.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using System.Numerics;
using Rasm.Numerics;
using Rhino.Geometry;

namespace Rasm.Domain;

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class Deterministic {
    private const ulong Gamma = 0x9E3779B97F4A7C15UL;
    private static ulong Mix(ulong state) {
        ulong z = state;
        z = unchecked((z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL);
        z = unchecked((z ^ (z >> 27)) * 0x94D049BB133111EBUL);
        return z ^ (z >> 31);
    }
    private static ulong Advance(ref ulong state) => Mix(state: state = unchecked(state + Gamma));
    public static double NextUnit(ref ulong state) => (Advance(state: ref state) >> 11) * (1.0 / 9_007_199_254_740_992.0);
    public static double NextSignedUnit(ref ulong state) => (NextUnit(state: ref state) * 2.0) - 1.0;
    public static Complex NextSignedComplexUnit(ref ulong state) => new(real: NextSignedUnit(state: ref state), imaginary: NextSignedUnit(state: ref state));
    public static ulong OrderKey(Point3d point, int seed = 0) => OrderKey(coordinates: [point.X, point.Y, point.Z], seed: seed);
    public static ulong OrderKey(ReadOnlySpan<double> coordinates, int seed = 0) {
        ulong state = unchecked((uint)seed + Gamma);
        foreach (double coordinate in coordinates) {
            state = Mix(state: state ^ Bits(value: coordinate));
        }
        return state;
    }
    public static double UnitInterval(Point3d point, int salt, int seed = 0) {
        int mixed = unchecked((seed * 16_777_619) + salt);
        double unit = ((OrderKey(point: point, seed: mixed) >> 11) + 1.0) * (1.0 / 9_007_199_254_740_992.0);
        return Math.Clamp(value: unit, min: EpsilonPolicy.SqrtEpsilon, max: 1.0 - EpsilonPolicy.SqrtEpsilon);
    }
    // Stream mints a ref-threadable stream STATE from integer lanes and a 64-bit policy seed — the OrderKey fold
    // over exact integer lanes, so (pixel: 5, ordinal: 0) and (pixel: 0, ordinal: 5) mint distinct streams where
    // a hand XOR-pack of shifted lanes collides them, a full 64-bit seed rides one argument where a two-int split
    // truncates it, and no consumer re-transcribes the private Gamma to mint a state of its own.
    public static ulong Stream(ReadOnlySpan<long> lanes, long seed = 0L) {
        ulong state = unchecked((ulong)seed + Gamma);
        foreach (long lane in lanes) {
            state = Mix(state: state ^ unchecked((ulong)lane));
        }
        return state;
    }
    // Lane-keyed STATELESS unit draw — the Stream fold projected through the same top-53-bit rule as NextUnit, so a
    // per-(stream, ordinal, dimension) draw keys directly instead of advancing a state a partition could reorder.
    // The clamp is UnitInterval's, not NextUnit's: a lane-keyed draw feeds log-weighted rejection the same way.
    public static double Unit(ReadOnlySpan<long> lanes, long seed = 0L) =>
        Math.Clamp(value: ((Stream(lanes: lanes, seed: seed) >> 11) + 1.0) * (1.0 / 9_007_199_254_740_992.0),
            min: EpsilonPolicy.SqrtEpsilon, max: 1.0 - EpsilonPolicy.SqrtEpsilon);
    // Unbiased bounded integer draw (Lemire): the 64x64 widening multiply's high half is the scaled index and the
    // low half rejects only the short tail, so a modulo's low-value bias never enters an ordering or a sample index.
    public static int NextBelow(ref ulong state, int exclusiveCeiling) {
        // Argument CONTRACT, not a domain state: a zero ceiling divides by zero in the threshold and a negative
        // one casts to a near-2⁶⁴ bound — both caller programming errors the BCL throw-helper names at the
        // boundary, never a Fin the hot draw loops would thread for an unreachable arm.
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value: exclusiveCeiling);
        ulong bound = (ulong)exclusiveCeiling, threshold = (0UL - bound) % bound;
        ulong draw = Advance(state: ref state);
        while (unchecked(draw * bound) < threshold) draw = Advance(state: ref state);
        return (int)Math.BigMul(left: draw, right: bound, low: out _);
    }
    // The EQUIDISTRIBUTED family beside the pseudo-random stream: RadicalInverse is the digit reversal onto [0, 1)
    // and Hammersley the (i/n, radicalInverse(i)) pair a bounded-tap spherical integral reads — splitmix64
    // clustering leaves visible noise at a bounded tap budget, so equidistribution is its own member family on
    // the ONE deterministic-draw owner, never a consumer-page kernel. ReverseBits is the hoisted swap ladder, so a
    // consumer scrambling a Sobol coordinate reads the owner's reversal instead of transcribing five swap steps.
    public static uint ReverseBits(uint bits) {
        bits = (bits << 16) | (bits >> 16);
        bits = ((bits & 0x55555555u) << 1) | ((bits & 0xAAAAAAAAu) >> 1);
        bits = ((bits & 0x33333333u) << 2) | ((bits & 0xCCCCCCCCu) >> 2);
        bits = ((bits & 0x0F0F0F0Fu) << 4) | ((bits & 0xF0F0F0F0u) >> 4);
        return ((bits & 0x00FF00FFu) << 8) | ((bits & 0xFF00FF00u) >> 8);
    }
    public static double RadicalInverse(uint bits) => ReverseBits(bits: bits) * 2.3283064365386963e-10;
    // Base-parameterized radical inverse — the Halton leg's per-dimension prime, closing the declared
    // equidistribution law for every dimension rather than base 2 alone; arity discriminates on the second argument.
    public static double RadicalInverse(uint index, int radix) {
        double inverse = 0.0, fraction = 1.0 / radix;
        while (index > 0u) {
            (inverse, index, fraction) = (inverse + ((index % (uint)radix) * fraction), index / (uint)radix, fraction / radix);
        }
        return inverse;
    }
    public static (double U0, double U1) Hammersley(int index, int count) => ((index + 0.5) / count, RadicalInverse(bits: (uint)index));
    // THE one adapter for a package API whose SIGNATURE demands System.Random (MathNet distribution constructors).
    // Not a shim: it ADDS the replay guarantee the BCL type cannot carry, and it is the only sanctioned crossing.
    public static Random Source(long seed, params ReadOnlySpan<long> lanes) => new SplitMixRandom(seed: Stream(lanes: lanes, seed: seed));
    private sealed class SplitMixRandom(ulong seed) : Random {
        private ulong state = seed;
        protected override double Sample() => NextUnit(state: ref state);
        public override int Next(int maxValue) => NextBelow(state: ref state, exclusiveCeiling: maxValue);
        public override int Next(int minValue, int maxValue) => minValue + NextBelow(state: ref state, exclusiveCeiling: maxValue - minValue);
        public override void NextBytes(Span<byte> buffer) {
            while (buffer.Length >= sizeof(ulong)) {
                BitConverter.TryWriteBytes(destination: buffer, value: Advance(state: ref state));
                buffer = buffer[sizeof(ulong)..];
            }
            if (!buffer.IsEmpty) {
                Span<byte> tail = stackalloc byte[sizeof(ulong)];
                BitConverter.TryWriteBytes(destination: tail, value: Advance(state: ref state));
                tail[..buffer.Length].CopyTo(destination: buffer);
            }
        }
    }
    // Signed zeros key identically: -0.0 normalizes before bit projection.
    private static ulong Bits(double value) => BitConverter.DoubleToUInt64Bits(value: value == 0.0 ? 0.0 : value);
}
```

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

- [DERIVED_RANDOM_VIRTUALS]-[OPEN]: which `System.Random` virtuals does .NET 10 route through a derived override versus its internal fast path, so `SplitMixRandom` covers every entry a MathNet distribution reaches (`Sample`, `Next()`, `Next(int)`, `Next(int,int)`, `NextDouble`, `NextBytes`, `NextInt64`, `NextSingle`); verify against the decompiled `System.Random` implementation on the assay api rail.
