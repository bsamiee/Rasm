# `identity.md` Surgical Refinement Audit

Target: `libs/dotnet/Rasm/.planning/Domain/identity.md`

This audit accepts five ordered moves. Applied together, the target fences project **10 fewer LOC** and **8 fewer declared type/member symbols**. The public surface loses the false half-lane vocabulary, `CanonicalWriter.Streaming`, and public `Digest`; the remaining changes remove one-use deterministic mechanics while preserving framing, hashing, draw, and admission semantics. Consumer-fence deletions are additional and are not counted in the target-only LOC total.

## 1. Return the digest decomposition and delete `Lane`

### Location

- Target first content fence — anchor `using Thinktecture;`
- Target `[TYPES]` — anchor `[SmartEnum<int>] public sealed partial class Lane`
- Target `CanonicalWriter.U128` — anchor `ContentHash.Half(digest: value, lane: Lane.Low)`
- Target `ContentHash` — anchor `public static ulong Half(UInt128 digest, Lane lane)`
- Target content-boundary prose — anchor `` `Half` its one lane split ``
- Target content-package prose — anchor `Thinktecture.Runtime.Extensions (`

### From

```csharp
using Thinktecture;

[SmartEnum<int>]
public sealed partial class Lane {
    public static readonly Lane Low = new(key: 0);
    public static readonly Lane High = new(key: 1);
    internal int Shift => Key * 64;
}

public CanonicalWriter U128(UInt128 value) =>
    U64(value: ContentHash.Half(digest: value, lane: Lane.Low))
        .U64(value: ContentHash.Half(digest: value, lane: Lane.High));

public static ulong Half(UInt128 digest, Lane lane) => unchecked((ulong)(digest >> lane.Shift));
```

### To

```csharp
public CanonicalWriter U128(UInt128 value) {
    (ulong low, ulong high) = ContentHash.Halves(digest: value);
    return U64(value: low).U64(value: high);
}

public static (ulong Low, ulong High) Halves(UInt128 digest) =>
    (unchecked((ulong)digest), unchecked((ulong)(digest >> 64)));
```

Update the owning prose to name `` `Halves` its one `(Low, High)` decomposition `` and remove the now-unused Thinktecture package claim from this fence.

### Effect

- Fenced LOC: `11 -> 6` (`-5`).
- Symbols: delete `Lane`, `Lane.Low`, `Lane.High`, and `Lane.Shift` (`-1` module type, `-3` module members). `Half` becomes `Halves`, so the extraction operation count stays one.
- Generated surface: deleting `[SmartEnum<int>] Lane` also deletes its unused generated key, roster, lookup, conversion, comparison, parsing, and formatting members.
- API: the only representable result is the complete named pair; call sites select `.Low` or `.High` rather than passing an ordinal or a boolean mode knob.

### API / consumer proof

The checked-in Thinktecture catalogue proves that `[SmartEnum<int>]` generates a full keyed-owner surface. Here the owner has exactly two payload-free rows, no behavior column, no admission consumer, and no dispatch consumer; it exists only to turn `Low`/`High` into `0`/`64`. That is the repository's prohibited two-row vocabulary. Returning the decomposition keeps one extraction owner while making both legal results structural. It is stronger than replacing `Lane` with `bool high`: the tuple removes the false type without introducing boolean blindness, and it also repairs the existing numeric `0`/`1` call sites that do not match the current `Lane` signature.

The BCL `UInt128 -> ulong` conversion yields the low 64 bits and the one `>> 64` yields the high 64 bits. `CanonicalWriter.U128` keeps its existing little-endian low-then-high preimage order. `ContentHash.Wire`/`Admit(bytes)` remain the independent big-endian boundary correspondence.

### Ripples

These are the complete live `ContentHash.Half` code consumers:

| Stable location | Exact replacement |
| --- | --- |
| `Rasm.AppHost/.planning/Runtime/determinism.md` — `DeterminismContext.Address` | deconstruct `ContentHash.Halves(digest)` once and place `low`, then `high`, into `Prefix` |
| `Rasm.AppHost/.planning/Runtime/features.md` — `Bucketing.BucketOf` | `ContentHash.Halves(ContentHash.Of(...)).Low % 100UL` |
| `Rasm.AppHost/.planning/Runtime/time.md` — `ScheduleEntry.Seed` | `ContentHash.Halves(ContentHash.Of(...)).Low` |
| `Rasm.AppUi/.planning/Collab/presence.md` — `PeerTint.Unit` | `ContentHash.Halves(digest).High` |
| `Rasm.Compute/.planning/Model/identity.md` — `RosterFingerprint.Of` | `ContentHash.Halves(ContentHash.Of(...)).Low` |
| `Rasm.Compute/.planning/Model/sessions.md` — `Fingerprint` | `ContentHash.Halves(ContentHash.Of(...)).Low` |
| `Rasm.Compute/.planning/Symbolic/lowering.md` — the two frame writes | deconstruct once; write `low`, then `high` |
| `Rasm.Compute/.planning/Tensor/blas.md` — the two problem-digest frame writes | deconstruct once; write `low`, then `high` |
| `Rasm.Fabrication/.planning/Posting/optimization.md` — private `Lane` projection | deconstruct once and return `low ^ high` |
| `Rasm.Persistence/.planning/Store/placement.md` — nonce high word | `ContentHash.Halves(key.ToValue()).High` |

Update the exact owner spellings in `libs/dotnet/.planning/RULINGS.md` (`ContentHash.Half` -> `ContentHash.Halves`), target boundary/package prose, the `Runtime/determinism.md` and `Runtime/features.md` descriptions, the `Collab/presence.md` law/diagram token, the `Store/placement.md` half-lane prose, and `Rasm/.planning/Interaction/paint.md`'s explicit refusal token. No `ArtifactContent`, text, or byte-wire consumer changes.

## 2. Absorb digest-only writer construction into `ContentHash.Of`

### Location

- Target `CanonicalWriter` constructor — anchor `private CanonicalWriter(double tolerance, ...)`
- Target construction surface — anchor `public static CanonicalWriter Streaming`
- Target close surface — anchor `public UInt128 Digest()`
- Target `ContentHash.Of<TState>` — anchor `CanonicalWriter.Streaming(tolerance: EpsilonPolicy.ZeroTolerance, ...)`
- Target content-entry prose — anchor `` `CanonicalWriter.Streaming(tolerance, accumulator)` ``
- Target seed law — anchor `` `Streaming` takes the accumulator ``

### From

```csharp
private CanonicalWriter(double tolerance, XxHash128 accumulator, Option<ArrayBufferWriter<byte>> retained) {
    Tolerance = tolerance;
    this.accumulator = accumulator;
    this.retained = retained;
}

public static CanonicalWriter Streaming(double tolerance, XxHash128 accumulator) =>
    new(tolerance: tolerance, accumulator: accumulator, retained: None);

public UInt128 Digest() => accumulator.GetCurrentHashAsUInt128();

public static UInt128 Of<TState>(TState state, Action<TState, CanonicalWriter> chunks) {
    CanonicalWriter writer = CanonicalWriter.Streaming(tolerance: EpsilonPolicy.ZeroTolerance, accumulator: new XxHash128(seed: 0L));
    chunks(state, writer);
    return writer.Digest();
}
```

### To

```csharp
internal CanonicalWriter(double tolerance, XxHash128 accumulator, Option<ArrayBufferWriter<byte>> retained) {
    Tolerance = tolerance;
    this.accumulator = accumulator;
    this.retained = retained;
}

internal UInt128 Digest() => accumulator.GetCurrentHashAsUInt128();

public static UInt128 Of<TState>(TState state, Action<TState, CanonicalWriter> chunks, double tolerance = EpsilonPolicy.ZeroTolerance) {
    CanonicalWriter writer = new(tolerance, new XxHash128(seed: 0L), None);
    chunks(state, writer);
    return writer.Digest();
}
```

Rewrite the entry and seed-law prose so `Of<TState>` owns both exact-grid and caller-grid digest streams and always owns the zero-seeded accumulator; keep `Retaining(tolerance)` as the public byte-retaining mint.

### Effect

- Fenced LOC: `13 -> 11` (`-2`).
- Symbols: delete public `CanonicalWriter.Streaming` (`-1` declared module member). `Digest` remains one implementation symbol but becomes assembly-internal.
- Public surface: digest-only callers lose accumulator custody and the separate close step. The only legitimate variation—quantization tolerance—moves onto the content-hash entry that owns the seed and digest.

### API / consumer proof

The checked-in `System.IO.Hashing` catalogue establishes the exact lifecycle: `XxHash128(long)`, ordered `Append`, then `GetCurrentHashAsUInt128`. Every live `Streaming` consumer constructs `new XxHash128(seed: 0L)`, frames once, immediately calls `Digest`, and neither clones nor reuses the accumulator. The current public parameter therefore exposes no consumed capability except the non-zero seed that this page explicitly forbids for federation identity. The prose claim that `Streaming` enables a dual `XxHash128`/`Crc32` pass is not supported by the signature: the writer holds only `XxHash128`.

`EpsilonPolicy.ZeroTolerance` is a `const double`, so it is a legal optional default. Tolerance remains part of writer behavior and no caller can select algorithm, width, or seed. The constructor widens only to assembly-internal because adjacent `ContentHash` cannot call a private member; no package consumer receives that escape.

### Ripples

The live code scan finds four consumers, all replaced by one `ContentHash.Of` call:

| Stable location | From | To |
| --- | --- | --- |
| `Rasm.Element/.planning/Projection/address.md` — `ContentAddress.Of<TState>` | construct `Streaming`, invoke `fold`, wrap `writer.Digest()` | `Create(ContentHash.Of(state, fold, tolerance))` |
| `Rasm.Fabrication/.planning/Process/owner.md` — `FabricationCanon.Ordered` | `frame(CanonicalWriter.Streaming(grid, new XxHash128(0))).Digest()` | `ContentHash.Of(frame, static (emit, writer) => emit(writer), tolerance: grid)` |
| `Rasm.AppUi/.planning/Render/capture.md` — `PixelIdentity` mint | construct `Streaming`, write version/extent/raw pixels, read `Digest()` | one `ContentHash.Of((Info, Pixels), static (state, writer) => ..., tolerance: EpsilonPolicy.ZeroTolerance)` |
| `Rasm.Persistence/.planning/Version/commits.md` — `MerkleRange.Of` | local accumulator + `Streaming(...).Rows(...).Digest()` | `ContentHash.Of(sortedKeys, static (keys, writer) => writer.Rows(keys, static (key, framed) => framed.U128(key)))` |

Delete the now-dead `System.IO.Hashing` imports/package claims at those owners. Update the `CanonicalWriter.Streaming` prose in `Projection/address.md`, `Version/commits.md`, and `Rasm.Persistence/RULINGS.md` to `ContentHash.Of`. Correct `libs/dotnet/.api/api-hashing.md` so its multi-field identity entry names `Action<TState, CanonicalWriter>` plus the caller-grid tolerance rather than the stale raw-`XxHash128` callback. No `Retaining`/`ToBytes` consumer changes.

## 3. Inline the single-consumer coordinate-bit projection

### Location

- Target `Deterministic.OrderKey(ReadOnlySpan<double>, int)` — anchor `bits: Bits`
- Target end of `Deterministic` — anchor `private static ulong Bits(double value)`

### From

```csharp
public static ulong OrderKey(ReadOnlySpan<double> coordinates, int seed = 0) =>
    Fold(lanes: coordinates, bits: Bits, seed: unchecked((uint)seed));

private static ulong Bits(double value) => BitConverter.DoubleToUInt64Bits(value: value == 0.0 ? 0.0 : value);
```

### To

```csharp
public static ulong OrderKey(ReadOnlySpan<double> coordinates, int seed = 0) =>
    Fold(lanes: coordinates, bits: static value => BitConverter.DoubleToUInt64Bits(value == 0.0 ? 0.0 : value), seed: unchecked((uint)seed));
```

### Effect

- Fenced LOC: `3 -> 2` (`-1`).
- Symbols: delete private `Deterministic.Bits` (`-1` module member).

### API / consumer proof

`Bits` has exactly one call site and no independent lifecycle, policy, or reusable meaning outside that higher-order `Fold` argument. The capture-free lambda preserves its complete behavior: numeric equality maps both signed zeros to positive zero before `BitConverter.DoubleToUInt64Bits`; every other coordinate, including each NaN payload, keeps the current bit projection. `Fold` remains the only state owner and the lambda allocates no closure.

### Ripples

- None; `OrderKey` is unchanged.

## 4. Replace the one-use radical scale with the binary primitive

### Location

- Target deterministic constants — anchor `private const double RadicalScale`
- Target equidistributed surface — anchor `public static double RadicalInverse(uint bits)`

### From

```csharp
private const double RadicalScale = 1.0 / 4_294_967_296.0;

public static double RadicalInverse(uint bits) => ReverseBits(bits: bits) * RadicalScale;
```

### To

```csharp
public static double RadicalInverse(uint bits) => Math.ScaleB(ReverseBits(bits), -32);
```

### Effect

- Fenced LOC: `2 -> 1` (`-1`).
- Symbols: delete `RadicalScale` (`-1` private constant).

### API / consumer proof

The repository targets `net10.0`; its BCL reference surface exposes `Math.ScaleB(double, int)`, and the C# compute standard already uses it as the exact power-of-two scaling primitive. Every `uint` converts exactly to `double`, so `ScaleB(reversed, -32)` is bit-equivalent to multiplication by the exactly representable `2^-32`. `RadicalScale` has one consumer and carries no policy separate from that operation. Keep `UnitScale`: it is shared by the closed and open 53-bit projections and remains one authority for their common exponent.

### Ripples

- None; both `RadicalInverse` overloads and all consumers retain their signatures.

## 5. Thin `NextSingle` and pin `NextBytes` byte order

### Location

- Target deterministic constants — anchor `private const float SingleScale`
- Target `SplitMixRandom.NextSingle` — anchor `* SingleScale`
- Target `SplitMixRandom.NextBytes(Span<byte>)` — both anchors `BitConverter.TryWriteBytes`

### From

```csharp
private const float SingleScale = 1.0f / 16_777_216.0f;

public override float NextSingle() => (float)(Advance(state: ref state) >> 40) * SingleScale;
BitConverter.TryWriteBytes(destination: buffer, value: Advance(state: ref state));
BitConverter.TryWriteBytes(destination: tail, value: Advance(state: ref state));
```

### To

```csharp
public override float NextSingle() => MathF.ScaleB((float)(Advance(state: ref state) >> 40), -24);
System.Buffers.Binary.BinaryPrimitives.WriteUInt64LittleEndian(buffer, Advance(state: ref state));
System.Buffers.Binary.BinaryPrimitives.WriteUInt64LittleEndian(tail, Advance(state: ref state));
```

### Effect

- Fenced LOC: `4 -> 3` (`-1`).
- Symbols: delete `SingleScale` (`-1` private constant).
- Replay: `NextBytes` emits the same low-byte-first stream on every architecture rather than inheriting machine endianness.

### API / consumer proof

The BCL reference surface exposes `MathF.ScaleB(float, int)`. The shifted word carries at most 24 bits, exactly representable as `float`; scaling it by `2^-24` is therefore bit-equivalent to the current multiply by the exact reciprocal. `SingleScale` has one consumer.

`BitConverter.TryWriteBytes` is machine-endian. The target already uses `BinaryPrimitives` as its fixed-endian byte owner, and `WriteUInt64LittleEndian` preserves the current little-endian stream on the deployed machines while closing the cross-runtime replay hole. Both full-word and tail paths still advance exactly once per word; the tail still copies only the requested prefix. Fully qualifying the two calls avoids adding an import, so the correction does not consume the LOC reduction.

### Ripples

- None; `System.Random` overrides and draw sequences keep their signatures and advancement counts.

## Deliberate non-moves

- Keep `ArtifactContent` as the private-constructor record. Its raw payload and `(sha256, extent)` admissions preserve distinct `KernelFault` cases plus the caller's `Op`. A Thinktecture `[ComplexValueObject]` would add generated factories over the already-projected string, while the raw SHA-256 boundary and semantic `Fin` mapping would still remain; that is more surface, not a collapse.
- Keep `IDrawLane<TSelf>.Items` and the self constraint. The static member is a compile-time roster-shape obligation even though `Deterministic.Of` currently reads only `Lane`; the stack's deep-surface and anticipatory-collapse laws explicitly reject deleting a modeled axis because no current generic consumer reads it. Concrete generated `Items` satisfying the contract is composition, not a mirrored roster.
- Keep `CanonicalWriter.Retaining`, `ToBytes`, `Rows`, `Sorted`, `Doubles`, and `Optional`. Each has multiple live consumers and owns a distinct framing or close shape. Inlining any one moves count, order, presence, or byte-retention obligations back to callers.
- Keep `Quantize` as written. Flattening its nested exact-grid arm deletes no LOC or symbol, and the present branch order makes NaN, infinity, exact-grid signed zero, and snapped signed zero visibly disjoint.
- Keep `Supplier` on `Atom<ulong>`. Replacing it with `Source(...).NextDouble` would reduce lines but silently exchange a CAS-linearized concurrent supplier for mutable `Random` state; the logging sampler consumer can invoke the delegate concurrently.
- Keep `UnitScale`, `SaltPrime`, `Gamma`, `Mix`, `Advance`, `Project`, `Open`, `Extent`, and the private bounded `Draw`. Each has multiple consumers or names a load-bearing replay constant. Inlining them duplicates or obscures the splitmix, projection, salt, or range law.
- Do not replace the span/ref `foreach` and rejection loops with carrier combinators. Their stack-only state cannot cross delegate boundaries without allocation or loss of `ref` semantics.
