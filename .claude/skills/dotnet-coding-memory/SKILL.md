---
name: dotnet-coding-memory
description: "Use when writing or reviewing C# over spans, memory, or pooled buffers, covering ownership, views, kernels, lanes, parallel passes, and binary formats"
---

# [DOTNET_CODING_MEMORY]

Covers C# passes over spans and pooled buffers. `CommunityToolkit.HighPerformance` supplies 2D views, pooled owners, bit fields, and `ParallelHelper`, `System.Numerics.Tensors` supplies `TensorPrimitives`, and `System.IO.Hashing` supplies `XxHash128`.

## [01]-[OWNERSHIP]

Pooled and pinned buffers stay inside the pass that rents them, and a domain signature takes spans for one call and returns a published value. Lease length selects the type a member takes or holds:

| [INDEX] | [LEASE]                                     | [TYPE]                                                      |
| :-----: | :------------------------------------------ | :---------------------------------------------------------- |
|  [01]   | One synchronous call                        | `ReadOnlySpan<T>` input, `Span<T>` destination              |
|  [02]   | Field, closure, `await`, or parallel action | `ReadOnlyMemory<T>` or `Memory2D<T>`, `.Span` read per step |
|  [03]   | Scratch of one scope                        | `using SpanOwner<T>`                                        |
|  [04]   | Scratch a parallel action or builder reads  | `using MemoryOwner<T>`, `.Memory` passed on                 |
|  [05]   | Buffer its receiver releases                | `MemoryOwner<T>` in the signature                           |
|  [06]   | Published result                            | `ImmutableArray<T>`                                         |

- `Memory<T>` taken from an owner views, after `Dispose`, an array its next renter writes, and an owner outlives every view of it
- `MemoryOwner<T>.Slice` moves its array into a returned owner and disposes its parent
- Buffers one native call reads pin for the call (`using MemoryHandle` from `Pin()`, `fixed`)
- Buffers native objects read across calls are arrays from `GC.AllocateUninitializedArray<T>(length, pinned: true)` that outlive the objects

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

Scratch exists for a value an in-place write destroys and for nothing else. `TensorPrimitives` destinations alias an input exactly or not at all, and a chain of calls rewrites one destination with no scratch:

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

- Small scratch is a constant-size `stackalloc` before any loop, and lengths over the constant take `SpanOwner<T>` through one conditional
- Rented arrays hold their previous renter's data, `AllocationMode.Clear` serves a buffer a pass reads before writing every element
- Output of unknown length fills an `ArrayPoolBufferWriter<T>` (`Utf8JsonWriter`, `TryFormat` into `GetSpan`) its scope reads as `WrittenSpan`
- Neighbor passes hold state in a padded two-row `Span2D<T>` ring, `line & 1` selects its current row, boundary taps read padding with no bounds test

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
- Storable views (`Memory2D<T>`, a `Memory<T>.Cast` result) are built once and held, each `Memory<T>.Cast` call allocates a manager
- Column views (`GetColumn(c)`) take `Fill`, `CopyTo`, and `span.CopyTo(column)`, a channel plane needs no hand index
- Planes of one pinned array start at `Marshal.UnsafeAddrOfPinnedArrayElement(array, offset)`, and `new Span2D<T>(array, offset, height, width, 0)` views each

## [04]-[KERNELS]

Elementwise and reduction work over a scalar plane is one `TensorPrimitives` call per span or row. Loops stay where each element is a pixel vector or reads neighbors, and an operation that varies is a `struct` type argument the JIT specializes. Kernel sets are a smart enum, one private generic subclass holds the loop, and each item constructs it over a stateless kernel struct:

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

Filtering loops write every sample and advance the count by `Keeps`, with no branch around the write:

```csharp
internal interface ISample<out T> {
    public T Sample(Vector4 pixel);
    public bool Keeps(Vector4 pixel);
}

internal readonly struct Luma(Vector3 weights) : ISample<float> {
    public float Sample(Vector4 pixel) => Vector3.Dot(pixel.AsVector3(), weights);
    public bool Keeps(Vector4 pixel) => pixel.W > 0f;
}

internal static class Samples {
    public static ImmutableArray<T> Kept<T, TKernel>(ReadOnlySpan<Vector4> pixels, TKernel kernel) where TKernel : struct, ISample<T> {
        T[] kept = GC.AllocateUninitializedArray<T>(pixels.Length);
        int count = 0;
        for (int x = 0; x < pixels.Length; x++) {
            kept[count] = kernel.Sample(pixels[x]);
            count += kernel.Keeps(pixels[x]) ? 1 : 0;
        }
        return ImmutableCollectionsMarshal.AsImmutableArray(kept[..count]);
    }
}
```

## [05]-[LANES]

`Vector4` and `Vector128<T>` lanes accelerate on x64 and Arm64 and run per-pixel work no `TensorPrimitives` call states. `Vector256<T>` and `Vector512<T>` accelerate on x64 alone, and on Arm64 (`IsHardwareAccelerated` false) a `Vector256` loop runs slower than its scalar form. Lane decisions are a comparison mask into `ConditionalSelect`, and pixels of mixed alpha take no branch:

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
- Span loops with no `TensorPrimitives` member step `Vector128<T>.Count` elements through `Vector128.LoadUnsafe` and `StoreUnsafe`
- Destinations apart from their source finish on one vector at `length - Count` in place of a scalar remainder

## [06]-[PARALLEL_PASSES]

`ParallelHelper` splits an index range into batches, and each batch runs a sequential loop over one shared copy of a `readonly struct` action. Results leave through reference members (a `Memory2D<T>` target, an array written at `[i]`, a `StrongBox<int>` under `Interlocked`), and a mutable field races across workers. Accumulating passes give each band one row of a `bands × bins` buffer and merge rows with `TensorPrimitives.Add`, and one generic action over a fold struct serves every histogram:

```csharp
internal interface IRowFold {
    public int Bins { get; }
    public void Fold(ReadOnlySpan<Vector4> row, Span<int> bins);
}

internal readonly struct Bands<TFold>(ReadOnlyMemory2D<Vector4> rows, Memory2D<int> counts, TFold fold) : IAction where TFold : struct, IRowFold {
    public static ImmutableArray<int> Counted(ReadOnlyMemory2D<Vector4> rows, TFold fold) {
        int height = int.Clamp(rows.Height, 1, Environment.ProcessorCount);
        Memory2D<int> counts = new(new int[height * fold.Bins], height, fold.Bins);
        ParallelHelper.For(0, height, new Bands<TFold>(rows, counts, fold));
        Span2D<int> merged = counts.Span;
        for (int band = 1; band < height; band++)
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

- Batches cover whole rows or bands, and a body of nanoseconds per element runs a row per `Invoke` or raises `minimumActionsPerThread`
- Actions receive no token and no per-worker state, `Invoke` reads a token its action holds and rents its own row scratch
- `For2D(..outer, ..inner, action)` batches its outer range and calls `IAction2D.Invoke(i, j)` per cell, one destination row per cell
- `ForEach` over `Memory2D<T>` writes elements in place through `IRefAction<T>`, and a read-only memory takes `IInAction<T>`
- Overloads without an action argument run `default(TAction)`, an action holding data passes its instance and every type argument infers

## [07]-[PUBLISHED_RESULTS]

Results publish a filled array as `ImmutableArray<T>` through `ImmutableCollectionsMarshal.AsImmutableArray`, and a later array write changes the published value. Records and value objects compare an `ImmutableArray<T>` member by reference: a member a span pass reads takes one sequence-equality accessor, and a member no pass spans is a `Seq<A>`:

```csharp
public sealed class SequenceEquality<T> : IEqualityComparerAccessor<ImmutableArray<T>> {
    public static IEqualityComparer<ImmutableArray<T>> EqualityComparer { get; } =
        EqualityComparer<ImmutableArray<T>>.Create(static (left, right) => left.SequenceEqual(right), static items => toSeq(items).GetHashCode());
}

[ComplexValueObject(SkipFactoryMethods = true, SkipToString = true)]
public sealed partial class Response {
    [MemberEqualityComparer<SequenceEquality<float>, ImmutableArray<float>>]
    public ImmutableArray<float> Samples { get; }

    public static Response Scaled(ReadOnlySpan<float> source, float gain) {
        float[] samples = GC.AllocateUninitializedArray<float>(source.Length);
        TensorPrimitives.Multiply(source, gain, samples);
        return new(ImmutableCollectionsMarshal.AsImmutableArray(samples));
    }
}
```

- `CollectionsMarshal.AsSpan` views a list's backing array until its next growth
- `CollectionsMarshal.GetValueRefOrAddDefault` counts or merges with one lookup inside a builder that publishes its dictionary

## [08]-[SPAN_TEXT]

Parsers read ranges of their source span and allocate their result alone. `SplitAny` fills a pooled `System.Range` buffer, and generic parsing writes each token into its slot of the result:

```csharp
internal static class Numbers {
    public static Option<T[]> Parsed<T>(ReadOnlySpan<char> text) where T : struct, INumberBase<T> {
        using SpanOwner<System.Range> tokens = SpanOwner<System.Range>.Allocate(text.Length + 1);
        T[] values = new T[text.SplitAny(tokens.Span, " \t\r\n", StringSplitOptions.RemoveEmptyEntries)];
        int at = 0;
        while (at < values.Length && T.TryParse(text[tokens.Span[at]], NumberStyles.Float, CultureInfo.InvariantCulture, out values[at]))
            at++;
        return at == values.Length ? values : None;
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
- Reads at an offset call `RandomAccess.Read(handle, span, offset)` on a `File.OpenHandle` handle
- `SequenceReader<byte>` reads segmented input (`TryReadLittleEndian`, `TryReadTo`), and `IsSingleSegment` with `FirstSpan` takes one span

Persisted and cross-process keys digest canonical bytes through `XxHash128`, and `HashCode` and `string.GetHashCode` seed per process. One extension family appends each field kind, integers little-endian, text as UTF-8 after its byte count, and floats with NaN and `-0` folded, and callers chain fields from `new XxHash128()` to `GetCurrentHashAsUInt128()`:

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
|  [09]   | `Vector256<T>` lanes on Arm64                                    | `Vector128<T>` lanes or `Vector4`                     |
|  [10]   | Branch per pixel on a lane value                                 | `ConditionalSelect` over a comparison mask            |
|  [11]   | Hand `[(y * width) + x]` index or `DivRem` row recovery          | `Span2D<T>` rows and columns                          |
|  [12]   | `array.AsSpan2D(height, width)` assigned to `Span2D<T>`          | `new Span2D<T>(array, height, width)`                 |
|  [13]   | `Parallel.For` closure over an array                             | `ParallelHelper` action over `Memory2D<T>`            |
|  [14]   | `Interlocked` on one shared accumulator per element              | Band rows merged after the pass                       |
|  [15]   | `ToArray()` over a filled array to publish it                    | `ImmutableCollectionsMarshal.AsImmutableArray`        |
|  [16]   | Hand `Equals` and `GetHashCode` over a buffer member             | Sequence-equality accessor                            |
|  [17]   | `dictionary[span.ToString()]` per lookup                         | `GetAlternateLookup<ReadOnlySpan<char>>()`            |
|  [18]   | `Position` set before each `ReadExactly`                         | `RandomAccess.Read` at the offset                     |
|  [19]   | Digest fields concatenated with no byte count                    | Length-prefixed field through `Bytes`                 |
