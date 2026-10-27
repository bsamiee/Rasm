---
name: dotnet-coding-memory
description: "Use when writing or reviewing C# over spans, memory, or pooled buffers, covering ownership, views, parallel passes, and kernels"
---

# [DOTNET_CODING_MEMORY]

Covers C# code over buffers (`Span<T>`, `Memory<T>`, pooled owners, 2D views), from ownership to parallel passes, kernels, and span search. `CommunityToolkit.HighPerformance` supplies 2D views, pooled owners, and `ParallelHelper`, `System.Numerics.Tensors` supplies `TensorPrimitives`, and `System.IO.Hashing` supplies stable digests, each namespace imported per file with no global using.

## [01]-[OWNERSHIP]

Buffers have one owner that releases them and consumers that use them for a lease, the period a call or an object holds them. Lifetime selects type:

| [INDEX] | [TYPE]                           | [HOLDS]                      | [LIFETIME]                                          |
| :-----: | :------------------------------- | :--------------------------- | :-------------------------------------------------- |
|  [01]   | `ReadOnlySpan<T>`, `Span<T>`     | Stack-only view              | One synchronous call                                |
|  [02]   | `ReadOnlyMemory<T>`, `Memory<T>` | Storable view                | Field, closure, or `await`, within its owner's life |
|  [03]   | `SpanOwner<T>`                   | Pooled array, `ref struct`   | One `using` scope                                   |
|  [04]   | `MemoryOwner<T>`                 | Pooled array, `IMemoryOwner` | Until `Dispose` or a `Slice` transfer               |
|  [05]   | `ArrayPoolBufferWriter<T>`       | Growing pooled output        | Until `Dispose`                                     |
|  [06]   | `T[]`, `ImmutableArray<T>`       | Garbage-collected array      | As long as a result references it                   |

- Pure synchronous transforms take `ReadOnlySpan<T>` and write a `Span<T>` destination
- Effects, fields, and closures take `ReadOnlyMemory<T>` or `Memory<T>` and read `.Span` inside each synchronous step
- Span locals held across an `await` fail `CS4007`, lambdas capturing one fail `CS8175`
- Methods taking `Memory<T>` hold it for their call, one returning a `Task` or `IO` holds it until the effect completes
- Constructors taking `Memory<T>` make every instance member a consumer, and each instance lives no longer than its owner
- `IMemoryOwner<T>` in a parameter or return type transfers ownership, the receiver disposes it or transfers it once
- `Memory<T>` read from an owner before `Dispose` stays readable afterward and reads an array the next renter writes

Effects hold the owner under `use` and slice its span by the count each read returns:

```csharp
internal static class Digests {
    public static IO<ulong> Read(Stream stream, int length) =>
        from owner in use(() => MemoryOwner<byte>.Allocate(length))
        from count in IO.liftVAsync(env => stream.ReadAtLeastAsync(owner.Memory, length, throwOnEndOfStream: false, env.Token))
        select XxHash3.HashToUInt64(owner.Span[..count]);
}
```

## [02]-[POOLED_BUFFERS]

Pooled owners rent from `ArrayPool<T>.Shared` and return the array on `Dispose`:
- `Length` is requested size and `Span` slices to it, rented arrays are longer (a request for 10 rents 16)
- `DangerousGetArray` returns an `ArraySegment<T>` for an API taking `T[]`, its `Offset` and `Count` bound the rented range
- Rented contents hold the previous renter's data, `AllocationMode.Clear` zeroes a buffer read before every element is written
- `SpanOwner<T>.Dispose` returns its array on every call, a second call puts one array in the pool twice
- `MemoryOwner<T>` throws `ObjectDisposedException` from `Span` and `Memory` after `Dispose`, a second `Dispose` does nothing
- `MemoryOwner<T>` has no finalizer, an owner that is never disposed never returns its array
- `MemoryOwner<T>.Slice(start, length)` moves its array into a new owner and leaves its parent disposed
- Stack buffers up to a constant limit are `stackalloc`, one conditional picks `SpanOwner<T>` above it, and `stackalloc` sits outside loops (`CA2014`)
- `[SkipLocalsInit]` requires `AllowUnsafeBlocks` (`CS0227`) and fits a method that writes its whole `stackalloc` buffer before reading it
- `ArrayPoolBufferWriter<T>` collects output of unknown length through `GetSpan(sizeHint)` and `Advance(count)`, `WrittenSpan` reads it in scope

```csharp
internal static class Spreads {
    private const int StackLimit = 256;

    public static float Of(ReadOnlySpan<float> samples, float mean) {
        using SpanOwner<float> rented = samples.Length > StackLimit ? SpanOwner<float>.Allocate(samples.Length) : SpanOwner<float>.Empty;
        Span<float> centered = (samples.Length > StackLimit ? rented.Span : stackalloc float[StackLimit])[..samples.Length];
        TensorPrimitives.Subtract(samples, mean, centered);
        return TensorPrimitives.SumOfSquares(centered);
    }
}
```

APIs writing to an `IBufferWriter<T>` (`Utf8JsonWriter`, `TryFormat` into `GetSpan`) fill a pooled writer that one scope reads and releases inside a pure function:

```csharp
internal static class Fingerprints {
    public static ulong Of<T>(T value, JsonTypeInfo<T> type) {
        using ArrayPoolBufferWriter<byte> buffer = new();
        using (Utf8JsonWriter writer = new(buffer)) {
            JsonSerializer.Serialize(writer, value, type);
        }
        return XxHash3.HashToUInt64(buffer.WrittenSpan);
    }
}
```

## [03]-[VIEWS]

Views reinterpret or reshape a buffer in place:
- `MemoryMarshal.Cast<TFrom, TTo>` and toolkit `Cast` drop a trailing partial element (10 `float` values give 2 `Vector4`)
- Span casts allocate nothing, toolkit `Memory<T>.Cast` allocates a `MemoryManager<TTo>` per call, a storable cast view is built once and held
- `MemoryMarshal.AsBytes` gives the byte view a digest or a file write reads
- `MemoryMarshal.CreateSpan(ref value.X, 3)` views consecutive fields of one struct for one call
- First-class span conversions bind `array.AsSpan2D(height, width)` to a `ReadOnlySpan<T>` extension returning `ReadOnlySpan2D<T>`
- `new Span2D<T>(array, height, width)` and `array.AsSpan().AsSpan2D(height, width)` give a writable view
- `Memory2D<T>` is the storable form for a field, an action member, or an `await`, and `.Span` yields `Span2D<T>` in each synchronous step
- Range indexers slice both axes (`view[1.., 1..]`), and `Slice(row, column, height, width)` takes counts
- `Span2D<T>` pitch is the element gap between one row's end and the next row's start, a host row stride in elements enters once as `stride - width`
- `TryGetSpan` succeeds on a contiguous view alone, a view with a nonzero pitch is read row by row
- `GetRow` and `GetColumn` return `RefEnumerable<T>` with `CopyTo`, `Fill`, `Clear`, and `TryCopyFrom`, which returns `false` for a short target
- `span.CopyTo(column)` writes a span into a column and throws for a short column

Channel planes of a `pixels × 4` sample view copy out and back through columns:

```csharp
internal static class Alphas {
    public static void Opaque(Span2D<float> samples, Span<float> alpha, Action<Span2D<float>> edit) {
        samples.GetColumn(3).CopyTo(alpha);
        samples.GetColumn(3).Fill(1f);
        edit(samples);
        alpha.CopyTo(samples.GetColumn(3));
    }
}
```

## [04]-[PINNING]

Native code reads a managed buffer at an address the garbage collector cannot move:
- Calls that read a buffer during one call pin it for that call through `using MemoryHandle pinned = memory.Pin()` or `fixed`
- Buffers a native object reads across calls are `GC.AllocateUninitializedArray<T>(length, pinned: true)`, an array that never moves
- `Marshal.UnsafeAddrOfPinnedArrayElement(array, 0)` reads a pinned array's address, and the array outlives every native object holding it
- `new Span2D<T>(pointer, height, width, pitch)` views native memory within the lock or handle that keeps it valid

## [05]-[PUBLISHED_RESULTS]

Filled arrays publish without a copy, and records over buffers state content equality:
- `GC.AllocateUninitializedArray<T>(length)` allocates a result the pass writes in full
- `ImmutableCollectionsMarshal.AsImmutableArray` publishes a filled array with no copy, and a later write changes the published value
- `CollectionsMarshal.AsSpan` views a `List<T>` backing array, and an `Add` that grows it leaves views on its old array
- `CollectionsMarshal.SetCount` exposes backing slots with their old values
- Records holding `ReadOnlyMemory<T>` or `ImmutableArray<T>` compare the buffer by reference, a `Seq<A>` member compares by content
- Content equality is an `Equals` over `SequenceEqual` beside a `GetHashCode` that agrees with it
- `HashCode<T>.Combine(span)` hashes element bytes and agrees with `SequenceEqual` for integer and `char` elements
- Float elements hash through `HashCode.Add` per element, byte hashes split `0` from `-0` that `SequenceEqual` holds equal
- `HashCode` and `string.GetHashCode` seed per process, persisted or cross-process keys are `XxHash3` or `XxHash128` digests
- Persisted digests read canonical bytes: integers through `WriteLittleEndian`, text as UTF-8 behind its byte count, floats with NaN and `-0` folded
- `XxHash128` takes fields in order through `Append`, and `GetCurrentHashAsUInt128` reads the key

```csharp
internal static class Gains {
    public static ImmutableArray<float> Scaled(ReadOnlySpan<float> source, float gain) {
        float[] target = GC.AllocateUninitializedArray<float>(source.Length);
        TensorPrimitives.Multiply(source, gain, target);
        return ImmutableCollectionsMarshal.AsImmutableArray(target);
    }
}

internal sealed record Samples(ImmutableArray<float> Values) {
    public bool Equals(Samples? other) => other is not null && Values.AsSpan().SequenceEqual(other.Values.AsSpan());

    public override int GetHashCode() {
        HashCode hash = new();
        foreach (float value in Values) {
            hash.Add(value);
        }
        return hash.ToHashCode();
    }
}
```

## [06]-[PARALLEL_PASSES]

`ParallelHelper` splits an index range into batches and runs each batch as a sequential loop:
- Batch count is `min(ceil(count / minimumActionsPerThread), Environment.ProcessorCount)`, a `minimumActionsPerThread` above 1 suits a cheap body
- `For2D` and `ForEach` over `Memory2D<T>` cap batch count at the row count, and every batch runs whole rows
- One batch runs inline on the calling thread, more run under `Parallel.For` with batch count as degree of parallelism
- Parallel batches share one action copy, an inline batch runs on the caller's `in` reference
- Mutable action fields race across workers or write the caller's local, results leave through reference members
- Reference members are a `T[]` written at `[i]`, a `Memory2D<T>` target, or a `StrongBox<int>` under `Interlocked`
- `ref struct` actions fail `CS9244`, `Invoke` reads `.Span` from a memory member
- One batch rethrows the action's exception, parallel batches throw `AggregateException`
- Passes take no `CancellationToken` and no per-worker state, `Invoke` reads a token it holds and rents its own row scratch
- `ForEach` writes each element in place through `IRefAction<T>` over `Memory<T>` or `Memory2D<T>`, `IInAction<T>` reads read-only forms
- Overloads without an action argument run `default(TAction)` with every field unset
- Action instances infer every type argument, and a static member on the action starts its pass

Per-worker accumulators are one row per band of a `bands × bins` buffer, `For` runs over bands, and one `TensorPrimitives.Add` per band merges rows:

```csharp
internal readonly struct BandCount(ReadOnlyMemory<int> source, Memory2D<int> bands) : IAction {
    public static ImmutableArray<int> Of(ReadOnlyMemory<int> source, int bins) {
        int height = int.Clamp(source.Length, 1, Environment.ProcessorCount);
        Memory2D<int> bands = new(new int[height * bins], height, bins);
        ParallelHelper.For(0, height, new BandCount(source, bands));
        Span2D<int> rows = bands.Span;
        for (int band = 1; band < height; band++) {
            TensorPrimitives.Add(rows.GetRowSpan(0), rows.GetRowSpan(band), rows.GetRowSpan(0));
        }
        return [.. rows.GetRowSpan(0)];
    }

    public void Invoke(int i) {
        Span<int> counts = bands.Span.GetRowSpan(i);
        (long length, int height) = (source.Length, bands.Height);
        foreach (int value in source.Span[(int)(length * i / height)..(int)(length * (i + 1) / height)]) {
            counts[value]++;
        }
    }
}
```

## [07]-[KERNELS]

Elementwise and reduction loops over a float span are one `TensorPrimitives` call, a loop stays where each element is a `Vector4` pixel or reads neighbors. `TensorPrimitives` validates spans before computing and throws `ArgumentException` on a violation:
- Destinations alias an input exactly or not at all, a partial overlap throws
- Destinations shorter than their input throw, longer ones keep elements past input length unchanged
- Binary inputs of unequal length throw
- `Max`, `Min`, and `Average` throw on an empty span, `Sum` and `SumOfSquares` return 0, and `IndexOfMax` returns -1
- Reductions over a span that can be empty return `Option<T>`
- Element type changes are one `ConvertChecked`, `ConvertSaturating`, `ConvertTruncating`, or `ConvertToHalf` call per span

```csharp
internal static class Peaks {
    public static Option<float> Of(ReadOnlySpan<float> samples) => samples.IsEmpty ? None : TensorPrimitives.Max(samples);
}
```

## [08]-[SEARCH]

Span search reads precomputed sets and lookups that a `static readonly` field holds:
- `SearchValues<T>` serves `IndexOfAny`, `IndexOfAnyExcept`, `ContainsAny`, and `ContainsAnyExcept`
- `SearchValues<string>` accepts `Ordinal` or `OrdinalIgnoreCase` alone, every other comparison throws `ArgumentException`
- Frozen and hash collections read span keys through `GetAlternateLookup<ReadOnlySpan<char>>()` with no string allocation
- Comparers without `IAlternateEqualityComparer<ReadOnlySpan<char>, string>` fail `TryGetAlternateLookup`, and `GetAlternateLookup` throws
- `MemoryExtensions.Split(separator)` enumerates `System.Range` values with empty fields included
- Unqualified `Range` is ambiguous between `System.Range` and `LanguageExt.Range` (`CS0104`)
- `StringPool.Shared.GetOrAdd(span)` returns one instance per equal span and discards least recently used entries at capacity

```csharp
internal static class Codes {
    private static readonly SearchValues<char> Slug = SearchValues.Create("-0123456789abcdefghijklmnopqrstuvwxyz");
    private static readonly FrozenDictionary<string, int>.AlternateLookup<ReadOnlySpan<char>> Table =
        new Dictionary<string, int>(StringComparer.Ordinal) { ["alpha"] = 1, ["beta"] = 2 }.ToFrozenDictionary(StringComparer.Ordinal).GetAlternateLookup<ReadOnlySpan<char>>();

    public static bool IsSlug(ReadOnlySpan<char> text) => !text.IsEmpty && !text.ContainsAnyExcept(Slug);

    public static Option<int> Find(ReadOnlySpan<char> key) => Table.TryGetValue(key, out int code) ? code : None;
}
```

## [09]-[ANTI_PATTERNS]

| [INDEX] | [WRONG_FORM]                                                      | [CORRECT_FORM]                                                      |
| :-----: | :---------------------------------------------------------------- | :------------------------------------------------------------------ |
|  [01]   | `ArrayPool<T>.Shared.Rent` with `try/finally` and a manual slice  | `using SpanOwner<T>`, or `MemoryOwner<T>` under `use`               |
|  [02]   | `Dispose` called by hand on a `SpanOwner<T>`                      | `using` declaration                                                 |
|  [03]   | `Memory<T>` from an owner kept past `Dispose`                     | Owner held for the lease, or a published copy                       |
|  [04]   | Parent `MemoryOwner<T>` read after `Slice`                        | Slice as the one owner                                              |
|  [05]   | `stackalloc` sized by input or inside a loop                      | Constant-size `stackalloc` before the loop, `SpanOwner<T>` above it |
|  [06]   | Pooled buffer read before a full write under the default mode     | `AllocationMode.Clear`                                              |
|  [07]   | `new T[]` scratch per call that never leaves its pass             | `SpanOwner<T>`                                                      |
|  [08]   | `IO` lifted around pure serialization into a pooled writer        | `using` scope inside a pure function                                |
|  [09]   | `array.AsSpan2D(height, width)` assigned to `Span2D<T>`           | `new Span2D<T>(array, height, width)`                               |
|  [10]   | `Memory<T>.Cast` in a property read per access                    | Cast view built once and held                                       |
|  [11]   | One-column view built to copy a span into a column                | `span.CopyTo(column)`                                               |
|  [12]   | Hand `[(y * width) + x]` index or `DivRem` row recovery           | `Span2D<T>` rows and `GetColumn` channel planes                     |
|  [13]   | `GCHandle.Alloc` pinning with a manual `Free`                     | `using MemoryHandle` from `Pin()`, or a pinned array                |
|  [14]   | `Parallel.For` closure over a buffer                              | `ParallelHelper` action over `Memory<T>` or `Memory2D<T>`           |
|  [15]   | Mutable field on a parallel action                                | Reference member (`T[]`, `Memory2D<T>`, `StrongBox<T>`) per result  |
|  [16]   | `Interlocked` on one shared accumulator per element               | Band rows merged after the pass                                     |
|  [17]   | `ParallelHelper.For<TAction>(start, end)` on an action with data  | Action instance passed to the overload                              |
|  [18]   | `TensorPrimitives` destination at an offset into its input        | Exact alias or a separate destination                               |
|  [19]   | `TensorPrimitives.Max` over a span that can be empty              | `IsEmpty` check returning `Option<T>`                               |
|  [20]   | Per-element cast loop between numeric spans                       | `TensorPrimitives.Convert*` call                                    |
|  [21]   | Record equality over a `ReadOnlyMemory<T>` or `ImmutableArray<T>` | `Equals` over `SequenceEqual` with an agreeing `GetHashCode`        |
|  [22]   | `HashCode<float>.Combine` beside `SequenceEqual` equality         | `HashCode.Add` per element                                          |
|  [23]   | `HashCode<T>` or `GetHashCode` value persisted                    | `XxHash3` or `XxHash128` digest over canonical bytes                |
|  [24]   | `ToArray()` on a filled array to publish it                       | `ImmutableCollectionsMarshal.AsImmutableArray`                      |
|  [25]   | `CollectionsMarshal.AsSpan` view held across `Add`                | View taken after the last growth                                    |
|  [26]   | `dictionary[span.ToString()]` per lookup                          | `GetAlternateLookup<ReadOnlySpan<char>>()`                          |
|  [27]   | `char[]` set or `IndexOfAny(char[])` per call                     | `static readonly SearchValues<char>`                                |
|  [28]   | Toolkit `Tokenize`                                                | `MemoryExtensions.Split` over `System.Range`                        |
|  [29]   | `Array.Empty<T>()` under static `LanguageExt.Prelude` (`CS0119`)  | `[]` or `ReadOnlyMemory<T>.Empty`                                   |
