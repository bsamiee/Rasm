---
name: dotnet-coding-memory
description: "Use when writing or reviewing C# over spans, memory, or pooled buffers, covering ownership, views, kernels, lanes, parallel passes, and binary formats"
---

# [DOTNET_CODING_MEMORY]

Covers C# passes over spans and pooled buffers. `CommunityToolkit.HighPerformance` supplies 2D views, pooled owners, bit fields, and `ParallelHelper`, `System.Numerics.Tensors` supplies `TensorPrimitives`, and `System.IO.Hashing` supplies `XxHash128`.

## [01]-[OWNERSHIP]

Lease length selects storage, and ownership transfers explicitly:

| [INDEX] | [LEASE]                                     | [TYPE]                                                      |
| :-----: | :------------------------------------------ | :---------------------------------------------------------- |
|  [01]   | One synchronous call                        | `ReadOnlySpan<T>` input, `Span<T>` destination              |
|  [02]   | Field, closure, `await`, or parallel action | `ReadOnlyMemory<T>` or `Memory2D<T>`, `.Span` read per step |
|  [03]   | Scratch of one scope                        | `using SpanOwner<T>`                                        |
|  [04]   | Scratch a parallel action or builder reads  | `using MemoryOwner<T>`, `.Memory` passed on                 |
|  [05]   | Buffer its receiver releases                | `IMemoryOwner<T>` in the signature                          |
|  [06]   | Contiguous published result                 | `ImmutableArray<T>`                                         |

- Owners outlive every view and asynchronous consumer, including canceled operations until completion
- Read-only views restrict writes through the view, another alias can still mutate storage
- `MemoryOwner<T>.Slice` moves its array into a returned owner and disposes its parent
- Buffers one native call reads pin for the call (`using MemoryHandle` from `Pin()`, `fixed`)
- Retained native buffers stay rooted through final use, with `MemoryHandle` or `GC.AllocateUninitializedArray<T>(length, pinned: true)`

Effects hold an owner under `use`, release it at `Bracket`, and slice its span by the count each read returns:

```csharp
internal static class Digests {
    public static IO<UInt128> Read(Stream stream, int length) =>
        (from owner in use(() => MemoryOwner<byte>.Allocate(length))
         from count in IO.liftVAsync(env => stream.ReadAtLeastAsync(owner.Memory, length, throwOnEndOfStream: false, env.Token))
         select XxHash128.HashToUInt128(owner.Span[..count])).Bracket();
}
```

## [02]-[SCRATCH]

Scratch holds values a later step needs or workspace a kernel requires. Bulk kernels write directly into their destination under each member's alias contract:

```csharp
internal sealed record Levels(float Max) {
    public void Coded(ReadOnlySpan<float> values, Span<float> codes) {
        TensorPrimitives.Multiply(values, Max, codes);
        TensorPrimitives.Add(codes, 0.5f, codes);
        TensorPrimitives.Clamp(codes, 0f, Max, codes);
        TensorPrimitives.Truncate(codes, codes);
    }
}
```

Inputs a stage overwrites and a later step reads take one rented copy:

```csharp
internal static class Stages {
    public static void Mixed(Span<float> row, Action<Span<float>> stage, float amount) {
        using SpanOwner<float> input = SpanOwner<float>.Allocate(row.Length);
        row.CopyTo(input.Span);
        stage(row);
        TensorPrimitives.Lerp(input.Span, row, amount, row);
    }
}
```

- Small bounded scratch uses one `stackalloc` outside loops, larger or data-sized scratch takes `SpanOwner<T>`
- Uninitialized and rented storage must be written before reading, `AllocationMode.Clear` supplies zeroed pooled scratch
- Output of unknown length fills an `ArrayPoolBufferWriter<T>` (`Utf8JsonWriter`, `TryFormat` into `GetSpan`) its scope reads as `WrittenSpan`
- Stencil workspace derives row count and padding from dependency radius, a ring retains only rows later taps need

## [03]-[VIEWS]

Views reinterpret and reshape a buffer in place of copying it. `MemoryMarshal.Cast` changes element type, `AsSpan2D` shapes rows and columns, a 2D range selects channels, and one copy moves a channel subset between interleaved layouts. `Pixmap` is a host type with a `Data` address, `Rows`, `Columns`, and a byte `Step`:

```csharp
internal static class Planes {
    public static void Channels(ReadOnlySpan<Vector4> pixels, Span<float> samples, int channels) =>
        MemoryMarshal.Cast<Vector4, float>(pixels).AsSpan2D(pixels.Length, 4)[.., ..channels].CopyTo(samples.AsSpan2D(pixels.Length, channels));

    extension(Pixmap image) {
        public unsafe Span2D<float> Samples => new((void*)image.Data, image.Rows, image.Columns, (image.Step / sizeof(float)) - image.Columns);
    }
}
```

- Pitch is the element gap after each row (`stride - width`), and a view with a nonzero pitch reads row by row through `GetRowSpan`
- Nonempty array-backed `Memory<T>.Cast` allocates a manager, retain stored casts and use `MemoryMarshal.Cast` for synchronous views
- Column views (`GetColumn(c)`) take `Fill`, `CopyTo`, and `span.CopyTo(column)`, a channel plane needs no hand index
- Planes of one pinned array start at `Marshal.UnsafeAddrOfPinnedArrayElement(array, offset)`, and `new Span2D<T>(array, offset, height, width, 0)` views each
- `MemoryMarshal.Cast` reinterprets native layout and truncates incomplete target elements, external formats establish size and byte order first

## [04]-[KERNELS]

Elementwise and reduction work uses native bulk kernels per span or row, preserving rounding, NaN, and alias contracts. `MultiplyAdd` combines passes with separate rounding, `FusedMultiplyAdd` rounds once, and `MultiplyAddEstimate` varies with hardware. Loops stay for operations no bulk member supplies, and a varying operation is a small `struct` type argument the JIT specializes. Kernel sets use one private generic subclass over stateless structs:

```csharp
internal interface IBlend {
    public static abstract Vector4 Blended(Vector4 under, Vector4 over);
}

[SmartEnum<string>]
internal abstract partial class BlendMode {
    public static readonly BlendMode Multiply = new Kernel<Multiplied>("multiply");
    public static readonly BlendMode Screen = new Kernel<Screened>("screen");
    public static readonly BlendMode Darken = new Kernel<Darkened>("darken");

    public abstract void Composite(Span<Vector4> under, ReadOnlySpan<Vector4> over, ReadOnlySpan<float> weights);

    private sealed class Kernel<T>(string key) : BlendMode(key) where T : struct, IBlend {
        public override void Composite(Span<Vector4> under, ReadOnlySpan<Vector4> over, ReadOnlySpan<float> weights) {
            for (int x = 0; x < under.Length; x++)
                under[x] = Vector4.Lerp(under[x], T.Blended(under[x], over[x]), weights[x]);
        }
    }

    private readonly struct Multiplied : IBlend { public static Vector4 Blended(Vector4 under, Vector4 over) => under * over; }
    private readonly struct Screened : IBlend { public static Vector4 Blended(Vector4 under, Vector4 over) => Vector4.One - ((Vector4.One - under) * (Vector4.One - over)); }
    private readonly struct Darkened : IBlend { public static Vector4 Blended(Vector4 under, Vector4 over) => Vector4.Min(under, over); }
}
```

- Kernel structs sit `private` or `file` beside their set, and a `class` type argument shares one generic body that dispatches every call
- Kernels holding inputs (weights, a window, a border) are instance structs passed by value into a generic method that owns loop and output

Items over an element type derive key, size, and code range from the type argument, and one `TensorPrimitives.ConvertSaturating`, `ConvertChecked`, or `ConvertTruncating` call changes element type per span:

```csharp
[SmartEnum<string>]
internal abstract partial class SampleDepth {
    public static readonly SampleDepth Byte = new Integer<byte>();
    public static readonly SampleDepth Word = new Integer<ushort>();
    public static readonly SampleDepth Half = new Floating<Half>();
    public static readonly SampleDepth Single = new Floating<float>();

    public int Bytes { get; }

    public abstract void Stored(Span<float> samples, Span<byte> destination);

    private sealed class Integer<T>() : SampleDepth(string.Create(CultureInfo.InvariantCulture, $"uint{8 * Unsafe.SizeOf<T>()}"), Unsafe.SizeOf<T>())
        where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>, IMinMaxValue<T> {
        public override void Stored(Span<float> samples, Span<byte> destination) {
            TensorPrimitives.Multiply(samples, float.CreateTruncating(T.MaxValue), samples);
            TensorPrimitives.Round(samples, samples);
            TensorPrimitives.ConvertSaturating(samples, MemoryMarshal.Cast<byte, T>(destination));
        }
    }

    private sealed class Floating<T>() : SampleDepth(string.Create(CultureInfo.InvariantCulture, $"float{8 * Unsafe.SizeOf<T>()}"), Unsafe.SizeOf<T>())
        where T : unmanaged, IFloatingPointIeee754<T> {
        public override void Stored(Span<float> samples, Span<byte> destination) => TensorPrimitives.ConvertSaturating(samples, MemoryMarshal.Cast<byte, T>(destination));
    }
}
```

## [05]-[LANES]

Native kernels select vector widths and handle remainders. Custom SIMD uses runtime support and measured workload costs, `IsHardwareAccelerated` describes support rather than relative speed. Per-pixel work uses `Vector4` or supported intrinsic vectors, and `ConditionalSelect` applies lane masks:

```csharp
internal static class Alpha {
    public static void Unassociated(Span<Vector4> pixels) {
        for (int x = 0; x < pixels.Length; x++) {
            Vector128<float> lanes = pixels[x].AsVector128();
            Vector128<float> alpha = Vector128.Shuffle(lanes, Vector128.Create(3));
            Vector128<float> color = Vector128.ConditionalSelect(Vector128.GreaterThan(alpha, Vector128<float>.Zero), lanes / alpha, Vector128<float>.Zero);
            pixels[x] = Vector128.ConditionalSelect(Vector128.Create(-1, -1, -1, 0).AsSingle(), color, lanes).AsVector4();
        }
    }
}
```

- Double-precision pixels split into `Vector128<double>` halves by `WidenLower` and `WidenUpper`, and one `Narrow(lower, upper)` stores them
- Custom kernels load through `Vector128.Create(span)` and store through `CopyTo`, measured lower-level access uses element offsets
- Overlapping tail vectors require a full vector and repeated writes that preserve results, in-place transforms need an untouched input

## [06]-[PARALLEL_PASSES]

`ParallelHelper` batches ranges through one shared action, use a `readonly struct` and disjoint output slices. Accumulating passes give each band one row of a `bands × bins` buffer and merge rows with `TensorPrimitives.Add`. Caller-supplied band count bounds workspace and concurrency:

```csharp
internal interface IRowFold {
    public int Bins { get; }
    public void Fold(ReadOnlySpan<Vector4> row, Span<int> bins);
}

internal readonly struct Bands<TFold>(ReadOnlyMemory2D<Vector4> rows, Memory2D<int> counts, TFold fold) : IAction where TFold : struct, IRowFold {
    public static ImmutableArray<int> Counted(ReadOnlyMemory2D<Vector4> rows, TFold fold, int bands) {
        Memory2D<int> counts = new(new int[bands * fold.Bins], bands, fold.Bins);
        ParallelHelper.For(0, bands, new Bands<TFold>(rows, counts, fold));
        Span2D<int> merged = counts.Span;
        for (int band = 1; band < bands; band++)
            TensorPrimitives.Add(merged.GetRowSpan(0), merged.GetRowSpan(band), merged.GetRowSpan(0));
        return [.. merged.GetRowSpan(0)];
    }

    public void Invoke(int i) {
        ReadOnlySpan2D<Vector4> view = rows.Span;
        Span<int> bins = counts.Span.GetRowSpan(i);
        for (int line = Start(i); line < Start(i + 1); line++)
            fold.Fold(view.GetRowSpan(line), bins);
    }

    private int Start(int band) => (int)((long)rows.Height * band / counts.Height);
}
```

- Parallelize one level with rows or bands, nested native kernels and outer passes share one concurrency budget
- Measure sequential work against partitioning costs, `minimumActionsPerThread` bounds batch count but supplies no measured threshold
- `Parallel.For` supports cancellation and partition-local state, retain its overload when those contracts select the operation
- `ParallelHelper` supplies neither token nor worker state, row scratch belongs to a band and a retained token enters through action data
- `For2D(..outer, ..inner, action)` batches its outer range and calls `IAction2D.Invoke(i, j)` per cell, one destination row per cell
- `ForEach` over `Memory2D<T>` writes elements in place through `IRefAction<T>`, and a read-only memory takes `IInAction<T>`
- Overloads without an action argument run `default(TAction)`, an action holding data passes its instance and every type argument infers

## [07]-[PUBLISHED_RESULTS]

Filled arrays transfer to `ImmutableArray<T>` through `ImmutableCollectionsMarshal.AsImmutableArray` only when every mutable alias ends. Unknown result counts use `ImmutableArray<T>.Builder.DrainToImmutable`, which transfers full storage or copies a partial buffer. Records and value objects compare array members by reference, content equality requires an accessor matching element equality and hashing:

```csharp
public sealed class SequenceEquality<T> : IEqualityComparerAccessor<ImmutableArray<T>> where T : notnull {
    public static IEqualityComparer<ImmutableArray<T>> EqualityComparer { get; } =
        EqualityComparer<ImmutableArray<T>>.Create(static (left, right) => left.AsSpan().SequenceEqual(right.AsSpan()), static items => items.AsSpan().GetDjb2HashCode());
}
```

- `CollectionsMarshal.AsSpan` borrows list storage, additions and removals wait until every span use ends
- `CollectionsMarshal.GetValueRefOrAddDefault` merges with one lookup, additions and removals wait until every returned ref use ends
- Byte hashes match byte equality, numeric equality folds representations element equality treats alike

## [08]-[SPAN_TEXT]

Parsers enumerate source ranges through `Split` or `SplitAny` without token storage, empty character separators select Unicode whitespace. Result builders collect parsed values and drain once:

```csharp
internal static class Numbers {
    public static Option<ImmutableArray<T>> Parsed<T>(ReadOnlySpan<char> text) where T : struct, INumberBase<T> {
        ImmutableArray<T>.Builder values = ImmutableArray.CreateBuilder<T>();
        foreach (System.Range range in text.SplitAny(ReadOnlySpan<char>.Empty)) {
            ReadOnlySpan<char> token = text[range];
            if (token.IsEmpty)
                continue;
            if (!T.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture, out T value))
                return None;
            values.Add(value);
        }
        return values.DrainToImmutable();
    }
}
```

- Line formats walk `EnumerateLines()` into one token span sliced to `width + 1` slots, and a `SplitAny` count past `width` rejects a long row
- Text writers format fields into their destination through `Utf8.TryWrite(span, CultureInfo.InvariantCulture, $"…", out int written)`
- `ContainsAnyExceptInRange(low, high)` checks every parsed code against its bounds in one call
- Character sets a pass tests are one `static readonly SearchValues<char>`, and a literal set one site tests stays a pattern (`c is '/' or '\\'`)
- Frozen and hash tables read span keys through `GetAlternateLookup<ReadOnlySpan<char>>()` under a comparer with an alternate, ordinal ones included
- `StringPool.Shared.GetOrAdd(span)` deduplicates repeated tokens and evicts at capacity, and an identity map is a dictionary

## [09]-[BINARY_FORMATS]

Formats read each field through `BinaryPrimitives` at its offset in its stated byte order, and packed words decode through `BitHelper.ExtractRange` in one pass into a reinterpreted destination:

```csharp
internal static class SharedExponent {
    public static float[] Decoded(ReadOnlySpan<byte> texels) {
        float[] samples = GC.AllocateUninitializedArray<float>(3 * (texels.Length / sizeof(uint)));
        Span<Vector3> colors = MemoryMarshal.Cast<float, Vector3>(samples.AsSpan());
        for (int x = 0; x < colors.Length; x++) {
            uint texel = BinaryPrimitives.ReadUInt32LittleEndian(texels[(sizeof(uint) * x)..]);
            colors[x] = new Vector3(BitHelper.ExtractRange(texel, 0, 9), BitHelper.ExtractRange(texel, 9, 9), BitHelper.ExtractRange(texel, 18, 9))
                * float.ScaleB(1f, (int)BitHelper.ExtractRange(texel, 27, 5) - 24);
        }
        return samples;
    }
}
```

- Blocks in machine byte order reinterpret through `MemoryMarshal.Cast<byte, T>`, a foreign order swaps in place through span `ReverseEndianness`
- Headers read into a `stackalloc` span through `ReadAtLeast(span, span.Length, throwOnEndOfStream: false)`, a short count refuses the file
- Headers write each field through `BinaryPrimitives.Write*` into one `stackalloc` span and leave in one `Stream.Write(span)`
- Offset reads use `RandomAccess.Read` when consumers handle short counts, fixed-size records retain `ReadExactly` or `ReadAtLeast`
- `SequenceReader<byte>` reads segmented input (`TryReadLittleEndian`, `TryReadTo`), and `IsSingleSegment` with `FirstSpan` takes one span

Persisted noncryptographic keys hash canonical bytes through `XxHash128`, `HashCode` and `string.GetHashCode` seed per process. Serialization defines field boundaries and byte order, floating-point normalization follows key equality. Numeric keys fold equal NaNs and signed zero, bit-identity keys preserve representations:

```csharp
internal static class Canonical {
    extension(XxHash128 digest) {
        public XxHash128 Integer<T>(T value) where T : unmanaged, IBinaryInteger<T> {
            Span<byte> field = stackalloc byte[value.GetByteCount()];
            _ = value.WriteLittleEndian(field);
            digest.Append(field);
            return digest;
        }

        public XxHash128 Bytes(ReadOnlySpan<byte> value) {
            digest.Integer(value.Length).Append(value);
            return digest;
        }

        public XxHash128 Text(ReadOnlySpan<char> value) {
            using SpanOwner<byte> bytes = SpanOwner<byte>.Allocate(Encoding.UTF8.GetByteCount(value));
            _ = Encoding.UTF8.GetBytes(value, bytes.Span);
            return digest.Bytes(bytes.Span);
        }

        public XxHash128 Number(double value) => digest.Integer(BitConverter.DoubleToInt64Bits(double.IsNaN(value) ? double.NaN : value is 0d ? 0d : value));
    }
}
```

## [10]-[ANTI_PATTERNS]

| [INDEX] | [WRONG_FORM]                                                     | [CORRECT_FORM]                                        |
| :-----: | :--------------------------------------------------------------- | :---------------------------------------------------- |
|  [01]   | `ArrayPool<T>.Shared.Rent` with `try/finally` and a manual slice | `using SpanOwner<T>`, or `MemoryOwner<T>` under `use` |
|  [02]   | `new T[]` scratch per call that never leaves its pass            | Destination written in place, or `SpanOwner<T>`       |
|  [03]   | `for` loop over a float span per element                         | One `TensorPrimitives` call                           |
|  [04]   | Per-element cast loop between numeric spans                      | `TensorPrimitives.Convert*` call                      |
|  [05]   | `TensorPrimitives.Max` over a span that can be empty             | `IsEmpty` test returning `Option<T>`                  |
|  [06]   | Delegate or interface call per element                           | `struct` type argument the JIT specializes            |
|  [07]   | Item or member per kernel, each with its own loop                | Private generic subclass over a kernel struct         |
|  [08]   | Literal key or byte size per item over an element type           | Facts derived from the type argument                  |
|  [09]   | Fixed SIMD width selected from processor name                    | Native kernel or runtime-supported vector width       |
|  [10]   | Branch per pixel on a lane value                                 | `ConditionalSelect` over a comparison mask            |
|  [11]   | Hand `[(y * width) + x]` index or `DivRem` row recovery          | `Span2D<T>` rows and columns                          |
|  [12]   | `array.AsSpan2D(height, width)` assigned to `Span2D<T>`          | `new Span2D<T>(array, height, width)`                 |
|  [13]   | Per-element scheduling or shared action mutation                 | Batched native parallel API with disjoint outputs     |
|  [14]   | `Interlocked` on one shared accumulator per element              | Band rows merged after the pass                       |
|  [15]   | `ToArray()` over a filled array to publish it                    | `ImmutableCollectionsMarshal.AsImmutableArray`        |
|  [16]   | Hand `Equals` and `GetHashCode` over a buffer member             | Sequence-equality accessor                            |
|  [17]   | `dictionary[span.ToString()]` per lookup                         | `GetAlternateLookup<ReadOnlySpan<char>>()`            |
|  [18]   | Single `RandomAccess.Read` assumed to fill its destination       | Returned count or exact-read stream contract          |
|  [19]   | Digest fields concatenated with no byte count                    | Length-prefixed field through `Bytes`                 |
