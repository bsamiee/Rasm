# [RASM_IDENTITY]

`Rasm.Domain` owns the kernel's two reproducibility surfaces with no sibling between them: the content-key capsule — `CanonicalWriter`, the ONE framed-preimage codec, and `ContentHash`, the seed-zero federation entry over it — and `Deterministic`, the one splitmix64 owner supplying order keys, unit-interval draws, and signed-unit streams to every reproducible algorithm. Neither is cryptographic.

Identity and derivation never cross: a content key built from a `Deterministic` order key, or a sampler seeded from a `ContentHash`, is rejected by design. Every federation partner reproduces the zero-fixed seed byte-for-byte, so one content space addresses across packages and runtimes. Framing is this page's law rather than each caller's obligation — `docs/laws/patterns.md` `[PREIMAGE_FRAMING]` and `[DIGEST_OVER_UNORDERED_CONTAINER]` are enforced by the writer's member set, so an unframed field, an uncounted collection, and a machine-endian UTF-16 text preimage are unspellable at the entry.

## [01]-[INDEX]

- [02]-[CONTENT_KEY]: `CanonicalWriter` frames the preimage, `ContentHash` mints and renders the seed-zero `XxHash128` federation key.
- [03]-[DETERMINISTIC_DERIVATION]: `Deterministic` owns order keys, unit draws, lane-bound draws, and signed-unit streams off one splitmix64 finalizer.

## [02]-[CONTENT_KEY]

- Owner: one capsule, two owners with one seam — `CanonicalWriter` owns FRAMING (how fields become bytes), `ContentHash` owns the DIGEST (how bytes become the `UInt128` currency) and its two text projections. `CanonicalWriter` is the only public way to emit a multi-field preimage, so the caller's obligation collapses from "produce canonical bytes" to "name the fields in order".
- Entry: `ContentHash.Of` is one name over three ingress shapes — `Of(ReadOnlySpan<byte>)` for a payload already canonical in one span, `Of(Stream)` for a source no span holds, `Of<TState>(TState, Action<TState, CanonicalWriter>)` for a field stream the caller emits through the writer. `CanonicalWriter.Streaming(tolerance, accumulator)` binds a writer to an accumulator the caller already owns (the dual-digest pass that folds `XxHash128` beside a `Crc32` frame check in one traversal); `Retaining(tolerance)` is the mint for callers that store or wire the canonical bytes themselves.
- Cases: members by field shape — fixed-width (`Bool`, `Ordinal`, `I64`, `U128`, `Single`, `Double`, and the bit-exact `Bits`) concatenate injectively and carry NO frame; variable-width (`String`) is ALWAYS int32-LE length-framed UTF-8; collections (`Rows<T>`, `Doubles`, and the order-publishing `Sorted<T,TKey>`) are count-framed; absence (`Optional<T>`) is presence-prefixed; `Raw` is the one named exemption. `Double` and `Bits` are two identities: the quantized leg keys a tolerance-banded geometry, the exact leg a replay or chaos chain that must re-derive bit-exact.
- Law: `String` has exactly one spelling and it frames — the int32-LE UTF-8 byte count precedes the bytes, so `("ab","c")` and `("a","bc")` cannot key alike, and because no member writes text any other way the `MemoryMarshal.AsBytes(string.AsSpan())` shape (machine-endian UTF-16, which keys differently on a big-endian partner) is unspellable on this surface rather than merely discouraged.
- Law: `Rows<T>` writes its count before its rows, so two adjacent collections whose concatenations agree still key apart; it takes `Seq<T>` because ORDER is part of the preimage and only an ordered carrier can carry it.
- Law: `Optional<T>` writes a presence byte before the value, so an absent column can never alias a written default — the chain-hash defect where `None` and a zero-prefixed present value are one digest.
- Law: `Double`/`Single` canonicalize before their bits leave — every NaN payload collapses to one quiet pattern and `-0.0` to `+0.0`, so two values that compare equal key equal.
- Law: `tolerance` is the double-quantization quantum and is PART OF THE KEY — a coordinate snaps to the grid before its bits are written, so two tolerances address two identity spaces rather than near-misses of one. Model-space callers pass `Context.Absolute.Value`; a grid-free caller (a schema key, an environment fingerprint, a plan shape) passes `EpsilonPolicy.ZeroTolerance`, which `ContentHash.Of<TState>` supplies. A LITERAL `0.0` tolerance is the exact-grid lane and quantizes NOTHING — `Quantize` reads zero as identity (signed zero still folds), so an exact-grid consumer never rides the division whose zero denominator keyed every finite double as NaN.
- Law: seed zero is the federation contract — `Of` mints its own accumulator at `seed: 0L` and there is NO seeded overload, because a computed seed IS a preimage (`[PREIMAGE_FRAMING]` line 31) and belongs in the field stream; a seeded reproducible LANE is `Deterministic.Stream`, a different concern on a different owner. `Streaming` takes the accumulator only so one traversal can feed two algorithms, and a caller minting a non-zero-seeded accumulator forks the seed every partner reproduces.
- Law: the digest RENDERS here. `Hex` is 32 lowercase hex (`:x32`) and `Admit` refuses uppercase, so one key has one text and a round trip is stable; the deleted form is a consumer admitting either case and rendering one, which reads correct in isolation and forks the moment a second reader compares texts.
- Packages: `System.IO.Hashing` (`XxHash128.HashToUInt128` one-shot, `XxHash128(long)` seeded construction, `Append(ReadOnlySpan<byte>)`, `Append(Stream)`, `GetCurrentHashAsUInt128`; MIT, managed, no native asset), `System.Buffers` (`ArrayBufferWriter<byte>`, `ArrayPool<byte>`), `System.Buffers.Binary` (`BinaryPrimitives` little-endian writes), Thinktecture.Runtime.Extensions (`[SmartEnum<int>]`).
- Growth: a new FIELD shape is one member on the writer; a new INGRESS shape is one overload on `Of`. Any second hashing owner beside either forks the federation seed.
- Boundary: `UInt128` is the identity currency, `Half` its one lane split, `Hex`/`Admit` its one text correspondence. `Raw` admits bytes the caller already framed — a fixed-width block or a whole-payload leaf — and a caller placing two variable-width `Raw` writes side by side owes the count itself; every other member frames for it. `Rasm.Element` owns the dimensioned leg: `MeasureValue` is the branch's dimensioned carrier, so its `Measure` member stays an `extension(CanonicalWriter)` block at Element composing `String`/`Double`/`Ordinal`/`Optional`.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using System.Buffers;
using System.Buffers.Binary;
using System.Globalization;
using System.IO.Hashing;
using System.Text;
using Rasm.Numerics;
using Thinktecture;

namespace Rasm.Domain;

// --- [TYPES] --------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class Lane {
    public static readonly Lane Low = new(key: 0);
    public static readonly Lane High = new(key: 1);
    // Each key IS the half index, so the shift derives rather than riding a second column a row could contradict.
    internal int Shift => Key * 64;
}

// --- [MODELS] -------------------------------------------------------------------------------
public sealed class CanonicalWriter {
    private readonly XxHash128 accumulator;
    private readonly Option<ArrayBufferWriter<byte>> retained;

    private CanonicalWriter(double tolerance, XxHash128 accumulator, Option<ArrayBufferWriter<byte>> retained) {
        Tolerance = tolerance;
        this.accumulator = accumulator;
        this.retained = retained;
    }

    // Streaming is the DEFAULT because a content KEY never needs the bytes: a lake schema key, a merge digest,
    // and a per-element fold stop materializing a whole preimage they only ever hash. The accumulator arrives
    // from the caller so ONE traversal can feed two algorithms (content identity beside a frame checksum).
    public static CanonicalWriter Streaming(double tolerance, XxHash128 accumulator) =>
        new(tolerance: tolerance, accumulator: accumulator, retained: None);
    public static CanonicalWriter Retaining(double tolerance) =>
        new(tolerance: tolerance, accumulator: new XxHash128(seed: 0L), retained: Some(new ArrayBufferWriter<byte>()));

    public double Tolerance { get; }

    // --- [FIXED_WIDTH]
    // These concatenate INJECTIVELY and carry no frame: each writes a constant byte count, so a field boundary is
    // recoverable from the schema alone. Little-endian on every one, so the preimage is byte-identical across runtimes.
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

    // `UInt128` splits through the owner's own lane projection, never a local shift — the branch ruling
    // that keeps one digest from acquiring two lane conventions applies to the writer as much as to a consumer.
    public CanonicalWriter U128(UInt128 value) =>
        I64(value: unchecked((long)ContentHash.Half(digest: value, lane: Lane.Low)))
            .I64(value: unchecked((long)ContentHash.Half(digest: value, lane: Lane.High)));

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
    // EXACT bits, never quantized: `Double` keys a tolerance-banded geometry identity, `Bits` keys a replay or
    // chaos chain that must re-derive bit-exact — two identities, two members, never one with a mode knob. Two
    // canonical hazards still normalize (no grid snap): `-0.0` writes as `+0.0` and every NaN writes one quiet
    // payload, because a value equal under `==` splitting into two keys forks one geometry into two identities — the
    // exact defect `Quantize` closes for `Double`. No other bit pattern changes.
    public CanonicalWriter Bits(double value) {
        double canonical = value == 0.0 ? 0.0 : double.IsNaN(value) ? double.NaN : value;
        Span<byte> word = stackalloc byte[sizeof(long)];
        BinaryPrimitives.WriteInt64LittleEndian(destination: word, value: BitConverter.DoubleToInt64Bits(value: canonical));
        return Emit(bytes: word);
    }
    // Count-framed quantized span — the plane leg of `Double`.
    public CanonicalWriter Doubles(ReadOnlySpan<double> values) {
        Ordinal(value: values.Length);
        foreach (double value in values) { Double(value: value); }
        return this;
    }

    // --- [VARIABLE_WIDTH]
    // ONE text member, and it frames. The int32-LE UTF-8 BYTE count precedes the bytes, which is why ("ab","c")
    // and ("a","bc") cannot collide; the pooled buffer keeps the encode allocation-free at every payload size.
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

    // `Raw` names the exemption: bytes the CALLER already canonicalized and delimited — a fixed-width block, a canonical
    // JSON document, a whole-payload leaf. Two variable-width Raw writes side by side owe their own count.
    public CanonicalWriter Raw(ReadOnlySpan<byte> bytes) => Emit(bytes: bytes);

    // --- [COMPOSITE]
    public CanonicalWriter Rows<T>(Seq<T> rows, Action<T, CanonicalWriter> field) {
        Ordinal(value: rows.Count);
        foreach (T row in rows) {
            field(row, this);
        }
        return this;
    }

    // This owner publishes the canonical ORDER for a hash-keyed container: the key selector and comparer ARE the
    // published order, so no caller sorts beside the writer (`DIGEST_OVER_UNORDERED_CONTAINER`).
    public CanonicalWriter Sorted<T, TKey>(Seq<T> rows, Func<T, TKey> key, IComparer<TKey> order, Action<T, CanonicalWriter> field) =>
        Rows(rows: toSeq(rows.OrderBy(keySelector: key, comparer: order)), field: field);
    public CanonicalWriter Optional<T>(Option<T> value, Action<T, CanonicalWriter> field) {
        CanonicalWriter framed = Bool(value: value.IsSome);
        value.Iter(present => field(present, framed));
        return framed;
    }

    // --- [CLOSE]
    public UInt128 Digest() => accumulator.GetCurrentHashAsUInt128();

    // ToBytes is the RETAINING close alone, and its refusal is TYPED. A streaming writer holds no buffer, and
    // returning an empty memory there would read as a legitimately empty preimage at every call site — an
    // absence the bare return cannot state. The mint decides which close is legal, so the rail carries that
    // decision rather than a raise: a caller that reached the wrong close reads a fault it can attribute.
    [BoundaryAdapter]
    public Fin<ReadOnlyMemory<byte>> ToBytes(Op? key = null) =>
        retained.Map(static buffer => buffer.WrittenMemory)
            .ToFin(Fail: key.OrDefault().InvalidContext());

    private CanonicalWriter Emit(ReadOnlySpan<byte> bytes) {
        accumulator.Append(source: bytes);
        if (retained is { IsSome: true, Case: ArrayBufferWriter<byte> buffer }) {
            buffer.Write(value: bytes);
        }
        return this;
    }

    // Snap to the grid FIRST, then collapse the two canonical hazards: every NaN payload to one quiet pattern and
    // -0.0 to +0.0. Non-finite values pass the grid untouched because rounding them is meaningless, not because
    // they are rare — Infinity divided by the quantum is Infinity and NaN stays NaN either way.
    // LAW: a ZERO tolerance is "no snap" — the value passes IDENTITY (signed zero still folds), never through the
    // division, because x/0.0 is Infinity and Infinity*0.0 is NaN, which silently keyed every finite non-zero
    // double at 21 consumer sites spelling an exact grid as a literal 0.0. An exact-bits caller may spell 0.0 or
    // `Bits`; the grid-free CONTENT lane stays `EpsilonPolicy.ZeroTolerance` (2⁻³², a real quantum).
    private double Quantize(double value) => value switch {
        _ when double.IsNaN(d: value) => double.NaN,
        _ when !double.IsFinite(d: value) => value,
        _ when Tolerance == 0.0 => value switch { 0.0 => 0.0, var exact => exact },
        _ => (Math.Round(value: value / Tolerance, mode: MidpointRounding.ToEven) * Tolerance) switch { 0.0 => 0.0, var snapped => snapped },
    };
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class ContentHash {
    [BoundaryAdapter] public static UInt128 Of(ReadOnlySpan<byte> canonicalBytes) => XxHash128.HashToUInt128(source: canonicalBytes);

    // Payloads whose byte length exceeds `int` range reach no `ReadOnlySpan<byte>` at all, so the stream leg is the
    // only spelling for a large plane, tile arena, or segmented artifact; the accumulator drains through a
    // write-only bridge and stages no bytes at any size.
    [BoundaryAdapter]
    public static UInt128 Of(Stream canonical) {
        XxHash128 accumulator = new(seed: 0L);
        accumulator.Append(stream: canonical);
        return accumulator.GetCurrentHashAsUInt128();
    }

    // This framed leg hands the caller a WRITER, never a bare accumulator: the raw-accumulator surface is what
    // let a separator join, an unframed collection, and a machine-endian text span each become a content key.
    [BoundaryAdapter]
    public static UInt128 Of<TState>(TState state, Action<TState, CanonicalWriter> chunks) {
        CanonicalWriter writer = CanonicalWriter.Streaming(tolerance: EpsilonPolicy.ZeroTolerance, accumulator: new XxHash128(seed: 0L));
        chunks(state, writer);
        return writer.Digest();
    }

    // Each lane IS the projection, so no consumer spells the shift and no call can name a third half. Two consumers
    // each writing their own `(ulong)digest` / `(ulong)(digest >> 64)` pair is how a frozen fixture and a seeded
    // generator drift apart on one key while both read correct in isolation.
    public static ulong Half(UInt128 digest, Lane lane) => unchecked((ulong)(digest >> lane.Shift));

    public static string Hex(UInt128 digest) => digest.ToString(format: "x32", provider: CultureInfo.InvariantCulture);

    // Admission REFUSES uppercase rather than normalizing it: a permissive reader beside a lowercase renderer makes the
    // round trip lossy in one direction only, so a text that admits here and a text this key renders are the
    // same 32 characters or the input was never this key's.
    [BoundaryAdapter]
    public static Fin<UInt128> Admit(ReadOnlySpan<char> hex, Op key) =>
        hex.Length == 32
        && !hex.ContainsAnyInRange(lowInclusive: 'A', highInclusive: 'F')
        && UInt128.TryParse(s: hex, style: NumberStyles.AllowHexSpecifier, provider: CultureInfo.InvariantCulture, result: out UInt128 digest)
            ? Fin.Succ(value: digest)
            : Fin.Fail<UInt128>(error: key.InvalidInput());
}
```

## [03]-[DETERMINISTIC_DERIVATION]

- Owner: `Deterministic` static class — the one splitmix64 owner: `Mix` (finalizer), `Advance` (golden-gamma stream), `Fold` (the lane fold), and `Project` (the top-53-bit unit rule) are the private mechanism; the public family is the unit draws, order keys, clamped intervals, lane-keyed draws, the bound `Draw`, the injectable `Supplier`, the bounded integer draw, the equidistributed family, and the one `System.Random` adapter. `Mix` stays unreachable outside the owner.
- Entry: three modalities by input shape — stream sampling advances a `ref ulong state` seeded by the consuming algorithm's named policy seed (`NextSignedUnit` for real bases, `NextSignedComplexUnit` for Hermitian, `NextBelow` for an unbiased bounded index at either integer width); coordinate keying is stateless (`OrderKey(coordinates, seed)`, the `Point3d` overload routing into the span fold, `UnitInterval(point, salt, seed)` for per-point draws); lane keying is stateless over integer lanes (`Stream(lanes, seed)` mints a threadable state, `Unit(lanes, seed)` projects one clamped draw). `Draw` binds a seed and a lane PREFIX so a per-generation or per-element draw supplies only the varying suffix; `Supplier(seed, purpose)` hands a sampler seam a `Func<double>` it can inject, and `Source(seed, lanes)` is THE one adapter for a package API whose SIGNATURE demands `System.Random`.
- Cases: consumers by member — the matrix eigensolver's LOBPCG starting bases (`NextSignedUnit`/`NextSignedComplexUnit` under its named basis-seed policy), the sampler's candidate ordering, active-set rotation, annulus, and weighted-rejection draws (`OrderKey`/`UnitInterval`), the fit consensus sampler's minimal-set draws (`NextBelow` over a `Stream`-minted state), per-(stream, ordinal, dimension) texel and jitter draws (`Unit`), threaded generation and uncertainty chains that carry a prefix through a loop (`Draw`), trace and probe samplers that take their draw as a dependency (`Supplier`), Halton and Sobol coordinates (`RadicalInverse`/`ReverseBits`/`Hammersley`), MathNet distribution constructors (`Source`), and any reproducible tie-break in the processing suite (`OrderKey`).
- Law: a lane ordinal is DECLARED, never derived from a name at runtime — `IDrawLane<TSelf>` exists so a roster publishes `Lane` as data (a `[SmartEnum]` roster satisfies `Items` with its generated member), and scar `SEEDED_FROM_STRING_HASH` is what a `GetHashCode()`-seeded lane costs: perfect stability inside one process and no replay across two.
- Law: coordinate keys normalize the signed zero — `-0.0` projects to `+0.0` before bit extraction so the two zeros key identically, and the seed widens unsigned (`(uint)seed`) so a negative seed never sign-extends into the state.
- Law: unit projections take the top 53 bits (`>> 11`, scaled `2^-53`) for an exact double; the CLAMPED form maps into `[EpsilonPolicy.SqrtEpsilon, 1 - EpsilonPolicy.SqrtEpsilon]` — the one named epsilon owner — so log-weighted rejection draws (`-log(u) / weight`) stay finite at both ends. `NextUnit` alone admits exact `0.0`, which is why the two forms are two private members and not one knob.
- Law: `Source` overrides EVERY `System.Random` virtual, because the base binds a derived instance to a compat implementation that seeds its own prng — `Sample()`, `Next()`, `NextBytes(byte[])`, and the large-range arm of `Next(int, int)` each draw from a stream this seed never touched unless the override exists, and MathNet's int32, full-range, and decimal generators reach two of those four; a missing override is a silent replay hole, never a compile gap. `Source` also answers the BCL's degenerate zero-width contracts, which the owner's own bounded draw refuses as caller errors.
- Exemption: the span folds, the `ref`-threaded state members, and the Lemire rejection loop are the named kernel exemption. That loop carries no attempt ceiling because its termination is provable — the rejected tail is shorter than the ceiling, so the expected iteration count is under two and a typed exhaustion fault names a budget that never runs out. No member reads time, thread identity, or process state.
- Boundary: a frozen WIRE constant does not declare here. Content-defined-chunking gear tables and any literal whose VALUES define a stored format declare at the page that OWNS that wire, because changing it re-cuts stored payloads. [NOT] a `Deterministic.Frozen` seat — a wire constant parked beside the mixer reads as a third splitmix and invites the mixer's gamma to be copied into it, which is the exact defect the private `Gamma` exists to prevent.
- Growth: a new reproducible draw shape is one member composing `Advance`/`Fold`; a new lane vocabulary is one roster implementing `IDrawLane<TSelf>` at its own owner.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using System.Collections.Immutable;
using System.Numerics;
using LanguageExt;
using Rasm.Numerics;
using Rhino.Geometry;
using static LanguageExt.Prelude;

namespace Rasm.Domain;

// --- [TYPES] --------------------------------------------------------------------------------
// Lane identity as a DECLARED ordinal, which is exactly what scar `SEEDED_FROM_STRING_HASH` demands. A roster
// implements this once and every draw addresses through it, so no folder invents a positional constant of its own.
public interface IDrawLane<TSelf> where TSelf : IDrawLane<TSelf> {
    static abstract IReadOnlyList<TSelf> Items { get; }
    long Lane { get; }
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class Deterministic {
    private const ulong Gamma = 0x9E3779B97F4A7C15UL;
    private const int SaltPrime = 16_777_619;
    private const double UnitScale = 1.0 / 9_007_199_254_740_992.0;
    private const float SingleScale = 1.0f / 16_777_216.0f;
    // 2⁻³², the `uint` bit-width reciprocal a reversed word scales by. Byte-identical to `EpsilonPolicy.ZeroTolerance`
    // and unrelated to it, which is exactly why it is named here rather than spelled as a literal at its one reader.
    private const double RadicalScale = 1.0 / 4_294_967_296.0;

    private static ulong Mix(ulong state) {
        ulong z = state;
        z = unchecked((z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL);
        z = unchecked((z ^ (z >> 27)) * 0x94D049BB133111EBUL);
        return z ^ (z >> 31);
    }
    private static ulong Advance(ref ulong state) => Mix(state: state = unchecked(state + Gamma));
    // ONE fold and ONE unit projection serve every keyed member below. The projector is an indirect call per
    // element, which the coordinate and lane folds both pay so that the seed widening, the gamma offset, and the
    // XOR-then-mix order have a single authority — two transcriptions of six lines is what drifts.
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
        Fold(lanes: coordinates, bits: Bits, seed: unchecked((uint)seed));
    // Salt is a lane ordinal, so it arrives `long` from `IDrawLane.Lane`; the order key folds a 32-bit lane word, and
    // the narrowing below is bit-identical for every int-salt caller.
    public static double UnitInterval(Point3d point, long salt, int seed = 0) =>
        Open(word: OrderKey(point: point, seed: unchecked((int)(((long)seed * SaltPrime) + salt))));
    // Stream mints a ref-threadable stream STATE from integer lanes and a 64-bit policy seed, so (pixel: 5,
    // ordinal: 0) and (pixel: 0, ordinal: 5) mint distinct streams where a hand XOR-pack of shifted lanes collides
    // them, a full 64-bit seed rides one argument where a two-int split truncates it, and no consumer
    // re-transcribes the private Gamma to mint a state of its own.
    public static ulong Stream(ReadOnlySpan<long> lanes, long seed = 0L) =>
        Fold(lanes: lanes, bits: static lane => unchecked((ulong)lane), seed: seed);
    // Lane-keyed STATELESS unit draw: a per-(stream, ordinal, dimension) draw keys directly instead of advancing a
    // state a partition could reorder, and it takes the open-interval clamp because it feeds the same
    // log-weighted rejection arithmetic `UnitInterval` does.
    public static double Unit(ReadOnlySpan<long> lanes, long seed = 0L) => Open(word: Stream(lanes: lanes, seed: seed));

    // --- [BOUND_DRAW]
    // `Draw` earns its seat only where a prefix is genuinely threaded through a loop; a two-lane draw at one site stays flat.
    public readonly record struct Draw(long Seed, ImmutableArray<long> Prefix) {
        public Draw At(params ReadOnlySpan<long> lanes) => new(Seed: Seed, Prefix: [.. Prefix, .. lanes]);
        public ulong State => Stream(lanes: Prefix.AsSpan(), seed: Seed);
        public double Unit => Deterministic.Unit(lanes: Prefix.AsSpan(), seed: Seed);
        public Random Source => Deterministic.Source(seed: Seed, lanes: Prefix.AsSpan());
    }

    public static Draw Of<TLane>(long seed, TLane lane) where TLane : IDrawLane<TLane> => new(Seed: seed, Prefix: [lane.Lane]);

    // `Supplier` is the INJECTABLE draw a sampler seam takes instead of a delegate over process state; `purpose` keys the gate,
    // so two samplers sharing one seed never interleave — the property a process-wide generator cannot offer at
    // any seed, and the reason a `Random.Shared.NextDouble` handed to a sampler is unreplayable however the
    // sampler itself is written. The advance is a CAS over a cell rather than a captured mutable local, because two
    // concurrent draws off one closure read the same word and break both the stream and its replay — the property
    // the member sells. The step is TOTAL, every contender taking its own word, so the plain swap carries no verdict.

    public static Func<double> Supplier(long seed, long purpose) {
        Atom<ulong> state = Atom(Stream(lanes: [purpose], seed: seed));
        return () => Project(word: Mix(state: state.Swap(static held => unchecked(held + Gamma))));
    }


    // --- [BOUNDED_DRAW]
    // Unbiased bounded draw (Lemire): the 64x64 widening multiply's high half is the scaled index and the low half
    // rejects only the short tail, so a modulo's low-value bias never enters an ordering or a sample index. ONE body
    // spans both widths — the `int` arity narrows this draw — so a full-width signed span, an int64 ceiling, and a
    // bounded index all reach the same rejection loop instead of a second transcribed copy that drifts from it.
    public static ulong NextBelow(ref ulong state, ulong exclusiveCeiling) {
        // Argument CONTRACT, not a domain state: a zero ceiling divides by zero in the threshold — a caller
        // programming error the BCL throw-helper names at the boundary, never a Fin the hot draw loops would
        // thread for an unreachable arm. The System.Random adapter below owns the BCL's own degenerate-span
        // contract, so admitting a zero here to serve it would widen this draw for a caller that has no such arm.
        ArgumentOutOfRangeException.ThrowIfZero(value: exclusiveCeiling);
        ulong threshold = (0UL - exclusiveCeiling) % exclusiveCeiling;
        ulong draw = Advance(state: ref state);
        while (unchecked(draw * exclusiveCeiling) < threshold) draw = Advance(state: ref state);
        return Math.BigMul(a: draw, b: exclusiveCeiling, low: out _);
    }
    public static int NextBelow(ref ulong state, int exclusiveCeiling) {
        // Negative ceilings cast to a near-2⁶⁴ bound, so they refuse here; zero refuses one hop down.
        ArgumentOutOfRangeException.ThrowIfNegative(value: exclusiveCeiling);
        return (int)NextBelow(state: ref state, exclusiveCeiling: (ulong)exclusiveCeiling);
    }

    // --- [EQUIDISTRIBUTED]
    // splitmix64 clustering leaves visible noise at a bounded tap budget, so equidistribution is its own member
    // family here rather than a consumer-page kernel. `ReverseBits` is the hoisted swap ladder — the BCL exposes no
    // `uint` bit-reverse intrinsic — so a consumer scrambling a Sobol coordinate reads the owner's reversal.
    public static uint ReverseBits(uint bits) {
        bits = (bits << 16) | (bits >> 16);
        bits = ((bits & 0x55555555u) << 1) | ((bits & 0xAAAAAAAAu) >> 1);
        bits = ((bits & 0x33333333u) << 2) | ((bits & 0xCCCCCCCCu) >> 2);
        bits = ((bits & 0x0F0F0F0Fu) << 4) | ((bits & 0xF0F0F0F0u) >> 4);
        return ((bits & 0x00FF00FFu) << 8) | ((bits & 0xFF00FF00u) >> 8);
    }
    public static double RadicalInverse(uint bits) => ReverseBits(bits: bits) * RadicalScale;
    // Base-parameterized radical inverse — the Halton leg's per-dimension prime, closing the declared
    // equidistribution law for every dimension rather than base 2 alone; arity discriminates on the second argument.
    public static double RadicalInverse(uint index, int radix) {
        radix = Math.Max(val1: radix, val2: 2); // base-1 division never terminates and no Halton axis draws below binary
        double inverse = 0.0, fraction = 1.0 / radix;
        while (index > 0u) {
            (inverse, index, fraction) = (inverse + ((index % (uint)radix) * fraction), index / (uint)radix, fraction / radix);
        }
        return inverse;
    }
    public static (double U0, double U1) Hammersley(int index, int count) {
        // Parity with every sibling contract on this owner: a zero count divides to Infinity and a negative index
        // casts to a near-2³² uint, both of which return a well-formed pair that is silently not a Hammersley point.
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value: count);
        ArgumentOutOfRangeException.ThrowIfNegative(value: index);
        return ((index + 0.5) / count, RadicalInverse(bits: (uint)index));
    }

    // --- [HOST_ADAPTER]
    public static Random Source(long seed, params ReadOnlySpan<long> lanes) => new SplitMixRandom(seed: Stream(lanes: lanes, seed: seed));

    // EVERY virtual is overridden, because the base type binds a derived instance to a compat implementation whose
    // parameterless construction seeds an INDEPENDENT legacy prng, and four members read that prng rather than the
    // override: `Sample()`, `Next()`, `NextBytes(byte[])`, and the large-range arm of `Next(int, int)`. MathNet's
    // int32, full-range, and decimal generators reach `Next()` and `NextBytes(byte[])` directly, so a single missing
    // override is a silent determinism hole no compiler reports. Bounded arms honour the BCL's own degenerate
    // contracts — a zero `maxValue` and an equal min and max return the floor — which the owner's draw refuses as
    // caller errors, so the adapter answers them here.
    private sealed class SplitMixRandom(ulong seed) : Random {
        private ulong state = seed;
        protected override double Sample() => NextUnit(state: ref state);
        // `System.Random` EXCLUDES int.MaxValue by contract, so the bounded draw states the ceiling rather than a 31-bit mask,
        // which admits int.MaxValue itself and shifts the distribution of every consumer folding on the endpoint.
        public override int Next() => NextBelow(state: ref state, exclusiveCeiling: int.MaxValue);
        public override int Next(int maxValue) => maxValue == 0 ? 0 : NextBelow(state: ref state, exclusiveCeiling: maxValue);
        // Extent computes in 64 bits: `maxValue - minValue` overflows `int` across the full signed span, and
        // `CheckForOverflowUnderflow` turns that overflow into a raise exactly where the BCL returns a value.
        public override int Next(int minValue, int maxValue) => (int)(minValue + (long)Draw(state: ref state, extent: Extent(floor: minValue, ceiling: maxValue)));
        public override double NextDouble() => NextUnit(state: ref state);
        // Top 24 bits scaled by 2⁻²⁴ is the exact float grid — the top-53-bit rule one width down.
        public override float NextSingle() => (float)(Advance(state: ref state) >> 40) * SingleScale;
        public override long NextInt64() => (long)Draw(state: ref state, extent: (ulong)long.MaxValue);
        public override long NextInt64(long maxValue) => (long)Draw(state: ref state, extent: Extent(floor: 0L, ceiling: maxValue));
        public override long NextInt64(long minValue, long maxValue) => unchecked(minValue + (long)Draw(state: ref state, extent: Extent(floor: minValue, ceiling: maxValue)));
        public override void NextBytes(byte[] buffer) {
            ArgumentNullException.ThrowIfNull(argument: buffer);
            NextBytes(buffer: buffer.AsSpan());
        }
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
        // Widths compute unchecked because the full signed span is 2⁶⁴ − 1, which no signed subtraction holds.
        private static ulong Extent(long floor, long ceiling) {
            ArgumentOutOfRangeException.ThrowIfLessThan(value: ceiling, other: floor);
            return unchecked((ulong)ceiling - (ulong)floor);
        }
        private static ulong Draw(ref ulong state, ulong extent) => extent == 0UL ? 0UL : NextBelow(state: ref state, exclusiveCeiling: extent);
    }

    private static ulong Bits(double value) => BitConverter.DoubleToUInt64Bits(value: value == 0.0 ? 0.0 : value);
}
```

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
