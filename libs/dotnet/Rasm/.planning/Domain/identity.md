# [RASM_IDENTITY]

`Rasm.Domain` owns the kernel identity surfaces: `CanonicalWriter` and `ContentHash` for semantic XXH128 keys, `ArtifactContent` for stored SHA-256 payload identity plus extent, and `Deterministic` for reproducible derivation.

Identity and derivation never cross: a content key built from a `Deterministic` order key, or a sampler seeded from a `ContentHash`, is rejected by design. Every federation partner reproduces the zero-fixed seed byte-for-byte, so one content space addresses across packages and runtimes. Framing is this page's law rather than each caller's obligation, enforced by the writer's member set, so an unframed field, an uncounted collection, and a machine-endian UTF-16 text preimage are unspellable at the entry.

## [01]-[INDEX]

- [02]-[CONTENT_KEY]: `CanonicalWriter` frames semantic preimages, `ContentHash` mints XXH128 keys, and `ArtifactContent` admits the disjoint SHA-256-plus-extent coordinate for stored bytes.
- [03]-[DETERMINISTIC_DERIVATION]: `Deterministic` owns order keys, unit draws, lane-bound draws, and signed-unit streams off one splitmix64 finalizer.

## [02]-[CONTENT_KEY]

- Owner: one capsule, two owners with one boundary — `CanonicalWriter` owns FRAMING (how fields become bytes), `ContentHash` owns the DIGEST (how bytes become the `UInt128` currency) and its two text projections. `CanonicalWriter` is the only public way to emit a multi-field preimage, so the caller's obligation collapses from "produce canonical bytes" to "name the fields in order".
- Entry: `ContentHash.Of` is one name over three ingress shapes — `Of(ReadOnlySpan<byte>)` for a payload already canonical in one span, `Of(Stream)` for a source no span holds, `Of<TState>(TState, Action<TState, CanonicalWriter>, tolerance)` for a field stream the caller emits through the writer — exact-grid by default and caller-grid when a quantum is passed, the one entry owning the zero-seeded accumulator and the close for every digest-only stream. `Retaining(tolerance)` is the one public writer mint, for callers that store or wire the canonical bytes themselves.
- Cases: members by field shape — fixed-width (`Bool`, `Ordinal`, `I64`, `U128`, `Single`, `Double`, and the bit-exact `Bits`) concatenate injectively and carry NO frame; variable-width (`String`) is ALWAYS int32-LE length-framed UTF-8; collections (`Rows<T>`, `Doubles`, and the order-publishing `Sorted<T,TKey>`) are count-framed; absence (`Optional<T>`) is presence-prefixed; `Raw` is the one named exemption. `Double` and `Bits` are two identities: the quantized leg keys a tolerance-banded geometry, the exact leg a replay or chaos chain that must re-derive bit-exact.
- Law: `String` has exactly one spelling and it frames — the int32-LE UTF-8 byte count precedes the bytes, so `("ab","c")` and `("a","bc")` cannot key alike, and because no member writes text any other way the `MemoryMarshal.AsBytes(string.AsSpan())` shape (machine-endian UTF-16, which keys differently on a big-endian partner) is unspellable on this surface rather than merely discouraged.
- Law: `Rows<T>` writes its count before its rows, so two adjacent collections whose concatenations agree still key apart; it takes `Seq<T>` because ORDER is part of the preimage and only an ordered carrier can carry it.
- Law: `Optional<T>` writes a presence byte before the value, so an absent column can never alias a written default — the chain-hash defect where `None` and a zero-prefixed present value are one digest.
- Law: `Double`/`Single` canonicalize before their bits leave — every NaN payload collapses to one quiet pattern and `-0.0` to `+0.0`, so two values that compare equal key equal.
- Law: `tolerance` is the double-quantization quantum and is PART OF THE KEY — a coordinate snaps to the grid before its bits are written, so two tolerances address two identity spaces rather than near-misses of one. Model-space callers pass `Context.Absolute.Value`; a grid-free caller (a schema key, an environment fingerprint, a plan shape) passes `EpsilonPolicy.ZeroTolerance`, which `ContentHash.Of<TState>` supplies. A LITERAL `0.0` tolerance is the exact-grid lane and quantizes NOTHING — `Quantize` reads zero as identity (signed zero still folds), so an exact-grid consumer never rides the division whose zero denominator keyed every finite double as NaN.
- Law: seed zero is the federation contract — `Of` mints its own accumulator at `seed: 0L` and there is NO seeded overload, because a computed seed IS a preimage (`[PREIMAGE_FRAMING]` line 31) and belongs in the field stream; a seeded reproducible LANE is `Deterministic.Stream`, a different concern on a different owner. `Of<TState>` owns the accumulator outright — no entry takes one — so a non-zero-seeded accumulator forking the seed every partner reproduces is unspellable.
- Law: the digest RENDERS here. `Hex` is 32 lowercase hex (`:x32`) and `Admit` refuses uppercase, so one key has one text and a round trip is stable; the deleted form is a consumer admitting either case and rendering one, which reads correct in isolation and forks the moment a second reader compares texts.
- Law: the digest CROSSES here too. `Wire` is the 16 big-endian bytes and `Admit(ReadOnlySpan<byte>)` its one inverse, refusing any width but sixteen — so every `bytes` key column on every generated message composes this pair, the text form composes `Hex`, and no consumer spells `WriteUInt128BigEndian`, a hex format string, or a lane shift beside the owner. The little-endian `I64` halves `U128` writes stay HASH INPUT and never leave the process.
- Packages: `System.IO.Hashing` (`XxHash128.HashToUInt128` one-shot, `XxHash128(long)` seeded construction, `Append(ReadOnlySpan<byte>)`, `Append(Stream)`, `GetCurrentHashAsUInt128`; MIT, managed, no native asset), `System.Buffers` (`ArrayBufferWriter<byte>`, `ArrayPool<byte>`), `System.Buffers.Binary` (`BinaryPrimitives` little-endian preimage writes, the one big-endian pair under `Wire`/`Admit`), Google.Protobuf (`ByteString.CopyFrom(ReadOnlySpan<byte>)` — the carrier every generated `bytes` column holds, so the wire projection lands once here and `Rasm.csproj` carries the direct row).
- Growth: a new FIELD shape is one member on the writer; a new INGRESS shape is one overload on `Of`. Any second hashing owner beside either forks the federation seed.
- Boundary: `UInt128` is the identity currency, `Halves` its one `(Low, High)` decomposition, `Hex`/`Admit(text)` its one text correspondence, `Wire`/`Admit(bytes)` its one byte correspondence. `Raw` admits bytes the caller already framed — a fixed-width block or a whole-payload leaf — and a caller placing two variable-width `Raw` writes side by side owes the count itself; every other member frames for it. `Rasm.Element` owns the dimensioned leg: `MeasureValue` is the branch's dimensioned carrier, so its `Measure` member stays an `extension(CanonicalWriter)` block at Element composing `String`/`Double`/`Ordinal`/`Optional`.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Buffers;
using System.Buffers.Binary;
using System.Globalization;
using System.IO.Hashing;
using System.Security.Cryptography;
using System.Text;
using Google.Protobuf;
using LanguageExt;
using LanguageExt.Common;
using Rasm.Numerics;
using static LanguageExt.Prelude;

namespace Rasm.Domain;

// --- [TYPES] ---------------------------------------------------------------------------
public sealed record ArtifactContent {
    public const ulong MaxBytes = 1_073_741_824UL;

    private ArtifactContent(string sha256, ulong bytes) => (Sha256, Bytes) = (sha256, bytes);

    public string Sha256 { get; }
    public ulong Bytes { get; }

    public static Fin<ArtifactContent> Of(ReadOnlyMemory<byte> payload) =>
        Of(SHA256.HashData(payload.Span), checked((ulong)payload.Length));

    public static Fin<ArtifactContent> Of(ReadOnlySpan<byte> sha256, ulong bytes) =>
        sha256.Length != SHA256.HashSizeInBytes
            ? new KernelFault.InvalidValue("artifact-content.sha256", "carry 32 bytes")
            : bytes is not (> 0UL and <= MaxBytes)
                ? new KernelFault.OutOfRange("artifact-content.bytes", bytes, 1UL, MaxBytes)
                : Fin.Succ(new ArtifactContent(Convert.ToHexStringLower(sha256), bytes));
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed class CanonicalWriter {
    private readonly XxHash128 accumulator;
    private readonly Option<ArrayBufferWriter<byte>> retained;

    internal CanonicalWriter(double tolerance, XxHash128 accumulator, Option<ArrayBufferWriter<byte>> retained) {
        Tolerance = tolerance;
        this.accumulator = accumulator;
        this.retained = retained;
    }

    public static CanonicalWriter Retaining(double tolerance) =>
        new(tolerance: tolerance, accumulator: new XxHash128(seed: 0L), retained: Some(new ArrayBufferWriter<byte>()));

    public double Tolerance { get; }

    // --- [FIXED_WIDTH]
    public CanonicalWriter Bool(bool value) => Emit(bytes: [value ? (byte)1 : (byte)0]);

    public CanonicalWriter Ordinal(int value) {
        Span<byte> word = stackalloc byte[sizeof(int)];
        BinaryPrimitives.WriteInt32LittleEndian(destination: word, value: value);
        return Emit(bytes: word);
    }

    public CanonicalWriter I64(long value) {
        Span<byte> word = stackalloc byte[sizeof(long)];
        BinaryPrimitives.WriteInt64LittleEndian(destination: word, value: value);
        return Emit(bytes: word);
    }

    public CanonicalWriter U64(ulong value) {
        Span<byte> word = stackalloc byte[sizeof(ulong)];
        BinaryPrimitives.WriteUInt64LittleEndian(destination: word, value: value);
        return Emit(bytes: word);
    }

    public CanonicalWriter U128(UInt128 value) {
        (ulong low, ulong high) = ContentHash.Halves(digest: value);
        return U64(value: low).U64(value: high);
    }

    public CanonicalWriter Single(float value) {
        Span<byte> word = stackalloc byte[sizeof(float)];
        BinaryPrimitives.WriteSingleLittleEndian(destination: word, value: (float)Quantize(value: value));
        return Emit(bytes: word);
    }

    public CanonicalWriter Double(double value) {
        Span<byte> word = stackalloc byte[sizeof(double)];
        BinaryPrimitives.WriteDoubleLittleEndian(destination: word, value: Quantize(value: value));
        return Emit(bytes: word);
    }
    public CanonicalWriter Bits(double value) {
        double canonical = value == 0.0 ? 0.0 : double.IsNaN(value) ? double.NaN : value;
        Span<byte> word = stackalloc byte[sizeof(long)];
        BinaryPrimitives.WriteInt64LittleEndian(destination: word, value: BitConverter.DoubleToInt64Bits(value: canonical));
        return Emit(bytes: word);
    }
    public CanonicalWriter Doubles(ReadOnlySpan<double> values) {
        Ordinal(value: values.Length);
        foreach (double value in values) { Double(value: value); }
        return this;
    }

    // --- [VARIABLE_WIDTH]
    public CanonicalWriter String(ReadOnlySpan<char> value) {
        byte[] rented = ArrayPool<byte>.Shared.Rent(minimumLength: Encoding.UTF8.GetByteCount(chars: value));
        try {
            int written = Encoding.UTF8.GetBytes(chars: value, bytes: rented);
            Ordinal(value: written).Emit(bytes: rented.AsSpan(start: 0, length: written));
            return this;
        }
        finally {
            ArrayPool<byte>.Shared.Return(array: rented);
        }
    }

    public CanonicalWriter Bytes(ReadOnlySpan<byte> value) =>
        Ordinal(value: value.Length).Raw(bytes: value);

    public CanonicalWriter Raw(ReadOnlySpan<byte> bytes) => Emit(bytes: bytes);

    // --- [COMPOSITE]
    public CanonicalWriter Rows<T>(Seq<T> rows, Action<T, CanonicalWriter> field) {
        Ordinal(value: rows.Count);
        foreach (T row in rows) {
            field(row, this);
        }
        return this;
    }

    public CanonicalWriter Sorted<T, TKey>(Seq<T> rows, Func<T, TKey> key, IComparer<TKey> order, Action<T, CanonicalWriter> field) =>
        Rows(rows: toSeq(rows.OrderBy(keySelector: key, comparer: order)), field: field);
    public CanonicalWriter Optional<T>(Option<T> value, Action<T, CanonicalWriter> field) {
        CanonicalWriter framed = Bool(value: value.IsSome);
        value.Iter(present => field(present, framed));
        return framed;
    }

    // --- [CLOSE]
    internal UInt128 Digest() => accumulator.GetCurrentHashAsUInt128();

    public Fin<ReadOnlyMemory<byte>> ToBytes() =>
        retained.Map(static buffer => buffer.WrittenMemory)
            .ToFin(Fail: key.OrDefault().InvalidContext());

    private CanonicalWriter Emit(ReadOnlySpan<byte> bytes) {
        accumulator.Append(source: bytes);
        if (retained is { IsSome: true, Case: ArrayBufferWriter<byte> buffer }) {
            buffer.Write(value: bytes);
        }
        return this;
    }

    private double Quantize(double value) => value switch {
        _ when double.IsNaN(d: value) => double.NaN,
        _ when !double.IsFinite(d: value) => value,
        _ when Tolerance == 0.0 => value switch { 0.0 => 0.0, var exact => exact },
        _ => (Math.Round(value: value / Tolerance, mode: MidpointRounding.ToEven) * Tolerance) switch { 0.0 => 0.0, var snapped => snapped },
    };
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class ContentHash {
    public static UInt128 Of(ReadOnlySpan<byte> canonicalBytes) => XxHash128.HashToUInt128(source: canonicalBytes);

    public static UInt128 Of(Stream canonical) {
        XxHash128 accumulator = new(seed: 0L);
        accumulator.Append(stream: canonical);
        return accumulator.GetCurrentHashAsUInt128();
    }

    public static UInt128 Of<TState>(TState state, Action<TState, CanonicalWriter> chunks, double tolerance = EpsilonPolicy.ZeroTolerance) {
        CanonicalWriter writer = new(tolerance, new XxHash128(seed: 0L), None);
        chunks(state, writer);
        return writer.Digest();
    }

    public static (ulong Low, ulong High) Halves(UInt128 digest) =>
        (unchecked((ulong)digest), unchecked((ulong)(digest >> 64)));

    public static string Hex(UInt128 digest) => digest.ToString(format: "x32", provider: CultureInfo.InvariantCulture);

    public static ByteString Wire(UInt128 digest) {
        Span<byte> wire = stackalloc byte[16];
        BinaryPrimitives.WriteUInt128BigEndian(destination: wire, value: digest);
        return ByteString.CopyFrom(bytes: wire);
    }

    public static Fin<UInt128> Admit(ReadOnlySpan<char> hex) =>
        hex.Length == 32
        && !hex.ContainsAnyInRange(lowInclusive: 'A', highInclusive: 'F')
        && UInt128.TryParse(s: hex, style: NumberStyles.AllowHexSpecifier, provider: CultureInfo.InvariantCulture, result: out UInt128 digest)
            ? Fin.Succ(value: digest)
            : Fin.Fail<UInt128>(error: new KernelFault.InvalidInput());

    public static Fin<UInt128> Admit(ReadOnlySpan<byte> wire) =>
        wire.Length == 16
            ? Fin.Succ(value: BinaryPrimitives.ReadUInt128BigEndian(source: wire))
            : Fin.Fail<UInt128>(error: new KernelFault.InvalidInput(Axis: Some("content-key-wire-width")));
}
```

## [03]-[DETERMINISTIC_DERIVATION]

- Owner: `Deterministic` static class — the one splitmix64 owner: `Mix` (finalizer), `Advance` (golden-gamma stream), `Fold` (the lane fold), and `Project` (the top-53-bit unit rule) are the private mechanism; the public family is the unit draws, order keys, clamped intervals, lane-keyed draws, the bound `Draw`, the injectable `Supplier`, the bounded integer draw, the equidistributed family, and the one `System.Random` adapter. `Mix` stays unreachable outside the owner.
- Entry: three modalities by input shape — stream sampling advances a `ref ulong state` seeded by the consuming algorithm's named policy seed (`NextSignedUnit` for real bases, `NextSignedComplexUnit` for Hermitian, `NextBelow` for an unbiased bounded index at either integer width); coordinate keying is stateless (`OrderKey(coordinates, seed)`, the `Point3d` overload routing into the span fold, `UnitInterval(point, salt, seed)` for per-point draws); lane keying is stateless over integer lanes (`Stream(lanes, seed)` mints a threadable state, `Unit(lanes, seed)` projects one clamped draw). `Draw` binds a seed and a lane PREFIX so a per-generation or per-element draw supplies only the varying suffix; `Supplier(seed, purpose)` hands a sampler site a `Func<double>` it can inject, and `Source(seed, lanes)` is THE one adapter for a package API whose SIGNATURE demands `System.Random`.
- Cases: consumers by member — the matrix eigensolver's LOBPCG starting bases (`NextSignedUnit`/`NextSignedComplexUnit` under its named basis-seed policy), the sampler's candidate ordering, active-set rotation, annulus, and weighted-rejection draws (`OrderKey`/`UnitInterval`), the fit consensus sampler's minimal-set draws (`NextBelow` over a `Stream`-minted state), per-(stream, ordinal, dimension) texel and jitter draws (`Unit`), threaded generation and uncertainty chains that carry a prefix through a loop (`Draw`), trace and probe samplers that take their draw as a dependency (`Supplier`), Halton and Sobol coordinates (`RadicalInverse`/`ReverseBits`/`Hammersley`), MathNet distribution constructors (`Source`), and any reproducible tie-break in the processing suite (`OrderKey`).
- Law: a lane ordinal is DECLARED, never derived from a name at runtime — `IDrawLane<TSelf>` exists so a roster publishes `Lane` as data (a `[SmartEnum]` roster satisfies `Items` with its generated member), and scar `SEEDED_FROM_STRING_HASH` is what a `GetHashCode()`-seeded lane costs: perfect stability inside one process and no replay across two.
- Law: coordinate keys normalize the signed zero — `-0.0` projects to `+0.0` before bit extraction so the two zeros key identically, and the seed widens unsigned (`(uint)seed`) so a negative seed never sign-extends into the state.
- Law: unit projections take the top 53 bits (`>> 11`, scaled `2^-53`) for an exact double; the CLAMPED form maps into `[EpsilonPolicy.SqrtEpsilon, 1 - EpsilonPolicy.SqrtEpsilon]` — the one named epsilon owner — so log-weighted rejection draws (`-log(u) / weight`) stay finite at both ends. `NextUnit` alone admits exact `0.0`, which is why the two forms are two private members and not one knob.
- Law: `Source` overrides EVERY `System.Random` virtual, because the base binds a derived instance to a compat implementation that seeds its own prng — `Sample()`, `Next()`, `NextBytes(byte[])`, and the large-range arm of `Next(int, int)` each draw from a stream this seed never touched unless the override exists, and MathNet's int32, full-range, and decimal generators reach two of those four; a missing override is a silent replay hole, never a compile gap. `Source` also answers the BCL's degenerate zero-width contracts, which the owner's own bounded draw refuses as caller errors.
- Exemption: the span folds, the `ref`-threaded state members, and the Lemire rejection loop are the named kernel exemption. That loop carries no attempt ceiling because its termination is provable — the rejected tail is shorter than the ceiling, so the expected iteration count is under two and a typed exhaustion fault names a budget that never runs out. No member reads time, thread identity, or process state.
- Boundary: a frozen WIRE constant does not declare here. Content-defined-chunking gear tables and any literal whose VALUES define a stored format declare at the page that OWNS that wire, because changing it re-cuts stored payloads. [NOT] a `Deterministic.Frozen` seat — a wire constant parked beside the mixer reads as a third splitmix and invites the mixer's gamma to be copied into it, which is the exact defect the private `Gamma` exists to prevent.
- Growth: a new reproducible draw shape is one member composing `Advance`/`Fold`; a new lane vocabulary is one roster implementing `IDrawLane<TSelf>` at its own owner.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Collections.Immutable;
using System.Numerics;
using LanguageExt;
using Rasm.Numerics;
using Rhino.Geometry;
using static LanguageExt.Prelude;

namespace Rasm.Domain;

// --- [TYPES] ---------------------------------------------------------------------------
public interface IDrawLane<TSelf> where TSelf : IDrawLane<TSelf> {
    static abstract IReadOnlyList<TSelf> Items { get; }
    long Lane { get; }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Deterministic {
    private const ulong Gamma = 0x9E3779B97F4A7C15UL;
    private const int SaltPrime = 16_777_619;
    private const double UnitScale = 1.0 / 9_007_199_254_740_992.0;

    private static ulong Mix(ulong state) {
        ulong z = state;
        z = unchecked((z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL);
        z = unchecked((z ^ (z >> 27)) * 0x94D049BB133111EBUL);
        return z ^ (z >> 31);
    }
    private static ulong Advance(ref ulong state) => Mix(state: state = unchecked(state + Gamma));
    private static ulong Fold<T>(ReadOnlySpan<T> lanes, Func<T, ulong> bits, long seed) {
        ulong state = unchecked((ulong)seed + Gamma);
        foreach (T lane in lanes) {
            state = Mix(state: state ^ bits(lane));
        }
        return state;
    }
    private static double Project(ulong word) => (word >> 11) * UnitScale;
    private static double Open(ulong word) =>
        Math.Clamp(value: ((word >> 11) + 1.0) * UnitScale, min: EpsilonPolicy.SqrtEpsilon, max: 1.0 - EpsilonPolicy.SqrtEpsilon);

    public static double NextUnit(ref ulong state) => Project(word: Advance(state: ref state));
    public static double NextSignedUnit(ref ulong state) => (NextUnit(state: ref state) * 2.0) - 1.0;
    public static Complex NextSignedComplexUnit(ref ulong state) => new(real: NextSignedUnit(state: ref state), imaginary: NextSignedUnit(state: ref state));
    public static ulong OrderKey(Point3d point, int seed = 0) => OrderKey(coordinates: [point.X, point.Y, point.Z], seed: seed);
    public static ulong OrderKey(ReadOnlySpan<double> coordinates, int seed = 0) =>
        Fold(lanes: coordinates, bits: static value => BitConverter.DoubleToUInt64Bits(value == 0.0 ? 0.0 : value), seed: unchecked((uint)seed));
    public static double UnitInterval(Point3d point, long salt, int seed = 0) =>
        Open(word: OrderKey(point: point, seed: unchecked((int)(((long)seed * SaltPrime) + salt))));
    public static ulong Stream(ReadOnlySpan<long> lanes, long seed = 0L) =>
        Fold(lanes: lanes, bits: static lane => unchecked((ulong)lane), seed: seed);
    public static double Unit(ReadOnlySpan<long> lanes, long seed = 0L) => Open(word: Stream(lanes: lanes, seed: seed));

    // --- [BOUND_DRAW]
    public readonly record struct Draw(long Seed, ImmutableArray<long> Prefix) {
        public Draw At(params ReadOnlySpan<long> lanes) => new(Seed: Seed, Prefix: [.. Prefix, .. lanes]);
        public ulong State => Stream(lanes: Prefix.AsSpan(), seed: Seed);
        public double Unit => Deterministic.Unit(lanes: Prefix.AsSpan(), seed: Seed);
        public Random Source => Deterministic.Source(seed: Seed, lanes: Prefix.AsSpan());
    }

    public static Draw Of<TLane>(long seed, TLane lane) where TLane : IDrawLane<TLane> => new(Seed: seed, Prefix: [lane.Lane]);


    public static Func<double> Supplier(long seed, long purpose) {
        Atom<ulong> state = Atom(Stream(lanes: [purpose], seed: seed));
        return () => Project(word: Mix(state: state.Swap(static held => unchecked(held + Gamma))));
    }


    // --- [BOUNDED_DRAW]
    public static ulong NextBelow(ref ulong state, ulong exclusiveCeiling) {
        ArgumentOutOfRangeException.ThrowIfZero(value: exclusiveCeiling);
        ulong threshold = (0UL - exclusiveCeiling) % exclusiveCeiling;
        ulong draw = Advance(state: ref state);
        while (unchecked(draw * exclusiveCeiling) < threshold) draw = Advance(state: ref state);
        return Math.BigMul(a: draw, b: exclusiveCeiling, low: out _);
    }
    public static int NextBelow(ref ulong state, int exclusiveCeiling) {
        ArgumentOutOfRangeException.ThrowIfNegative(value: exclusiveCeiling);
        return (int)NextBelow(state: ref state, exclusiveCeiling: (ulong)exclusiveCeiling);
    }

    // --- [EQUIDISTRIBUTED]
    public static uint ReverseBits(uint bits) {
        bits = (bits << 16) | (bits >> 16);
        bits = ((bits & 0x55555555u) << 1) | ((bits & 0xAAAAAAAAu) >> 1);
        bits = ((bits & 0x33333333u) << 2) | ((bits & 0xCCCCCCCCu) >> 2);
        bits = ((bits & 0x0F0F0F0Fu) << 4) | ((bits & 0xF0F0F0F0u) >> 4);
        return ((bits & 0x00FF00FFu) << 8) | ((bits & 0xFF00FF00u) >> 8);
    }
    public static double RadicalInverse(uint bits) => Math.ScaleB(ReverseBits(bits), -32);
    public static double RadicalInverse(uint index, int radix) {
        radix = Math.Max(val1: radix, val2: 2);
        double inverse = 0.0, fraction = 1.0 / radix;
        while (index > 0u) {
            (inverse, index, fraction) = (inverse + ((index % (uint)radix) * fraction), index / (uint)radix, fraction / radix);
        }
        return inverse;
    }
    public static (double U0, double U1) Hammersley(int index, int count) {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value: count);
        ArgumentOutOfRangeException.ThrowIfNegative(value: index);
        return ((index + 0.5) / count, RadicalInverse(bits: (uint)index));
    }

    // --- [HOST_ADAPTER]
    public static Random Source(long seed, params ReadOnlySpan<long> lanes) => new SplitMixRandom(seed: Stream(lanes: lanes, seed: seed));

    private sealed class SplitMixRandom(ulong seed) : Random {
        private ulong state = seed;
        protected override double Sample() => NextUnit(state: ref state);
        public override int Next() => NextBelow(state: ref state, exclusiveCeiling: int.MaxValue);
        public override int Next(int maxValue) => maxValue == 0 ? 0 : NextBelow(state: ref state, exclusiveCeiling: maxValue);
        public override int Next(int minValue, int maxValue) => (int)(minValue + (long)Draw(state: ref state, extent: Extent(floor: minValue, ceiling: maxValue)));
        public override double NextDouble() => NextUnit(state: ref state);
        public override float NextSingle() => MathF.ScaleB((float)(Advance(state: ref state) >> 40), -24);
        public override long NextInt64() => (long)Draw(state: ref state, extent: (ulong)long.MaxValue);
        public override long NextInt64(long maxValue) => (long)Draw(state: ref state, extent: Extent(floor: 0L, ceiling: maxValue));
        public override long NextInt64(long minValue, long maxValue) => unchecked(minValue + (long)Draw(state: ref state, extent: Extent(floor: minValue, ceiling: maxValue)));
        public override void NextBytes(byte[] buffer) {
            ArgumentNullException.ThrowIfNull(argument: buffer);
            NextBytes(buffer: buffer.AsSpan());
        }
        public override void NextBytes(Span<byte> buffer) {
            while (buffer.Length >= sizeof(ulong)) {
                System.Buffers.Binary.BinaryPrimitives.WriteUInt64LittleEndian(buffer, Advance(state: ref state));
                buffer = buffer[sizeof(ulong)..];
            }
            if (!buffer.IsEmpty) {
                Span<byte> tail = stackalloc byte[sizeof(ulong)];
                System.Buffers.Binary.BinaryPrimitives.WriteUInt64LittleEndian(tail, Advance(state: ref state));
                tail[..buffer.Length].CopyTo(destination: buffer);
            }
        }
        private static ulong Extent(long floor, long ceiling) {
            ArgumentOutOfRangeException.ThrowIfLessThan(value: ceiling, other: floor);
            return unchecked((ulong)ceiling - (ulong)floor);
        }
        private static ulong Draw(ref ulong state, ulong extent) => extent == 0UL ? 0UL : NextBelow(state: ref state, exclusiveCeiling: extent);
    }
}
```

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
