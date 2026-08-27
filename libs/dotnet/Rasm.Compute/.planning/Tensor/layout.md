# [COMPUTE_LAYOUT]

Rasm.Compute cpu-tensor layout algebra: one `LayoutForm` named-shape vocabulary whose axis-label correspondence derives every layout permutation, one `Contiguity` memory-order classification over the `Tensor<T>` stride facts, one `AxisPermutation` admission proving a bijection the host kernel never checks, and one generic `ReshapeOp<T>` request union owning the singular shape-edit verb family under one total compile-exhaustive `Switch`. `TensorLayout.Reform`, `TensorLayout.Apply`, and `TensorLayout.Split` are the entrypoints, partitioned by result arity rather than by a `Seq` whose length a prose sentence guarantees.

Layout members ride `System.Numerics.Tensors`; `TensorDtype`/`TensorReason` arrive settled from `Tensor/vocabulary#TENSOR_VOCABULARY`. `LayoutForm` rows are the geometry-encoding wire-shape targets `Tensor/residency#GEOMETRY_ENCODING` reads and the nchw↔nhwc permutation the CoreML image-model route consumes.

## [01]-[INDEX]

- [02]-[LAYOUT_ALGEBRA]: named-shape `LayoutForm` vocabulary, contiguity classification, and the `ReshapeOp<T>` shape-edit verb union under one total `Switch`.

## [02]-[LAYOUT_ALGEBRA]

- Owner: `LayoutForm` (named wire-shape vocabulary, axis-label permutation source) + `Contiguity` (memory-order classification) + `StorageClass` (the aliasing verdict every verb answers) + `AxisPermutation` (admitted bijection) + `ReshapeOp<T>` (the generic `[Union]` singular shape-edit request family) + `DestinationOp<T>` (the two caller-destination writes) under the `TensorLayout.Reform`/`Apply`/`Split` entrypoints.
- Cases: `LayoutForm` rows dense | nxc | vertex-face | nchw | nhwc (rank derives from the axis-label count); `Contiguity` rows dense | strided | broadcast (each carrying its `Reshapeable` column); `StorageClass` rows shared | materialized | source-dependent; the singular `ReshapeOp<T>` verbs `Permute` | `Transpose` | `Squeeze` | `Unsqueeze` | `Reshape` | `Flatten` | `Densify` | `Concatenate` | `Stack` | `Reverse` | `Slice` | `Pad` | `Roll` | `Write`; the `DestinationOp<T>` writes `Broadcast` | `Resize`, reached bare through `Apply`'s destination form and wrapped in `ReshapeOp.Write` through its allocating form.
- Entry: `public static Fin<Tensor<T>> Reform<T>(Tensor<T> source, LayoutForm origin, LayoutForm target)` for a named layout transition whose permutation derives from the axis-label correspondence; `public static Fin<Tensor<T>> Apply<T>(Tensor<T> source, ReshapeOp<T> op)` for the general singular shape edit, every verb an arm of the one total `Switch` and every arm answering exactly one tensor; `public static Fin<Unit> Apply<T>(Tensor<T> source, DestinationOp<T> op, in TensorSpan<T> destination)` threads the ref-struct destination as generated `Switch` state over the two verbs the host can serve; `public static Fin<Seq<Tensor<T>>> Split<T>(Tensor<T> source, int count, nint dimension)` is the one plural verb.
- Packages: System.Numerics.Tensors, Thinktecture.Runtime.Extensions, LanguageExt.Core, CommunityToolkit.HighPerformance, BCL inbox
- Growth: a new layout is one `LayoutForm` row carrying its `axisLabels` — every permutation to and from a same-label-set sibling DERIVES through `AxisMap`, zero permute-table edits; a new singular shape-edit verb is one `ReshapeOp<T>` case breaking the total `Switch` and the `Storage` projection at compile time; a new caller-destination write is one `DestinationOp<T>` case, which the destination entrypoint and the `Write` arm both break on; a second PLURAL verb is one case on a `Split` request value, never a `Seq` return widened across the singular family; a new memory order is one `Contiguity` row carrying its `Reshapeable` column; zero new surface.
- Boundary: `ReshapeOp<T>` is generic, so `Concatenate`/`Stack` carry same-typed `ImmutableArray<Tensor<T>>` operands feeding the variadic `ConcatenateOnDimension`/`StackAlongDimension` `params ReadOnlySpan<Tensor<T>>` directly; a float-locked `Tensor<float>[]` payload with runtime `Cast<Tensor<T>>` is rejected. Every extent, axis, and range payload is `ImmutableArray` under `[Equatable]`/`[OrderedEquality]`, so an admitted request is a VALUE: a caller-owned `nint[]` left the proof `PadLengths`/`Broadcastable`/`AxisPermutation.Admit` had just paid for void the instant the caller wrote to it, and eight reference-compared cases missed every plan-cache probe. `AxisPermutation` publishes the same `ImmutableArray<int>` its bijection proof narrowed. Destination capability is a TYPE, not a refusal: the destination entrypoint takes `DestinationOp<T>`, so a permute or a pad against a caller destination is unrepresentable rather than fourteen arms re-spelling their own case names as string literals. Aliasing is a column: `ReshapeOp<T>.Storage` answers `StorageClass` from the same total `Switch` the apply reads, so a caller asks whether a result aliases its buffer instead of reading a Boundary sentence. Every arm admits its structural invariant before the host call so `Fin<T>` carries `ComputeFault.TensorRejected`: `AxisPermutation.Admit` proves a bijection over `0..rank-1`; `Reshaped` accumulates its four independent wildcard facts through `Validation` and compares an overflow-free `BigInteger` product with `FlattenedLength`; `Broadcastable` checks right-aligned compatibility span-wise; `SplitEven` checks axis divisibility; `Axis` bounds dimensions; `Transpose` requires rank ≥ 2; `SqueezeDimension` requires a unit axis; `PositiveLengths` is the ONE positive-extent oracle both `Resize` forms take a symbol argument to. `Reform` also rejects duplicate axis labels before deriving nchw→nhwc `[0,2,3,1]` or nhwc→nchw `[0,3,1,2]`. `Pad` captures checked extent addition, materializes `CreateFromShape`, and copies the source into the interior slice. `Roll` normalizes the shift modulo the axis extent and concatenates complementary slices. `Reshape` admits zero extents and rejects only an extent below its one `-1` wildcard, `Resize` requires positive extents, and `Slice` captures `NRange.GetOffsetAndLength` and hands back the RANGES IT OWNS. `Split`'s dimension is `nint` because `Tensor.Split(ReadOnlyTensorSpan<T>, int, nint)` declares it so and an `int` plumbing widened implicitly. Join compatibility carries its exempt axis as `Option<int>` and reads the `ReshapeOp` case that already discriminates concatenate from stack, never a `bool stack` knob beside a `-1` sentinel spelling one fact twice. Shape oracles walk spans directly — `TensorPrimitives.Product`, `MemoryExtensions.SequenceEqual`, and index loops — because five `.ToArray()` lifts per shape check bought LINQ five heap allocations on the admission path. Gather, scatter, and take-along-axis read or write by index value and remain `Tensor/dispatch#KERNEL_DISPATCH` structural operations. `Permute`/`Transpose`/`Squeeze`/`Unsqueeze`/`Slice`/`Reverse` are `StorageClass.Shared`; `Concatenate`/`Stack`/`Pad`/`Write` and the plural `Split` are `StorageClass.Materialized`; `Reshape`/`Flatten`/`Densify`/`Roll` are `SourceDependent` — the first three because a strided or broadcast source materializes through `ToDenseTensor` while a dense one re-windows in place (`Contiguity.Reshapeable` holds on the dense row alone), and `Roll` because a normalized shift of zero returns the source untouched. `Flatten` folds a dimension range through `Reshape`, distinct from `Tensor<T>.FlattenTo(Span<T>)`. `Contiguity.Classify` reads `Tensor<T>.IsDense` and zero strides. Region writes remain dispatch-owned.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class LayoutForm {
    public static readonly LayoutForm Dense = new("dense", axisLabels: ["L"]);
    public static readonly LayoutForm NxC = new("nxc", axisLabels: ["N", "C"]);
    public static readonly LayoutForm VertexFace = new("vertex-face", axisLabels: ["V", "F"]);
    public static readonly LayoutForm Nchw = new("nchw", axisLabels: ["N", "C", "H", "W"]);
    public static readonly LayoutForm Nhwc = new("nhwc", axisLabels: ["N", "H", "W", "C"]);

    public ImmutableArray<string> AxisLabels { get; }
    public int Rank => AxisLabels.Length;

    public Fin<ImmutableArray<int>> AxisMap(LayoutForm target) =>
        Rank == target.Rank
        && AxisLabels.Distinct(StringComparer.Ordinal).Count() == Rank
        && target.AxisLabels.Distinct(StringComparer.Ordinal).Count() == target.Rank
        && AxisLabels.ToHashSet(StringComparer.Ordinal).SetEquals(target.AxisLabels)
            ? Fin.Succ<ImmutableArray<int>>([.. target.AxisLabels.Select(label => AxisLabels.IndexOf(label, 0, Rank, StringComparer.Ordinal))])
            : TensorReason.RowMissing.Fail<ImmutableArray<int>>("incompatible-forms", $"{Key}->{target.Key}");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class StorageClass {
    public static readonly StorageClass Shared = new("shared");
    public static readonly StorageClass Materialized = new("materialized");
    public static readonly StorageClass SourceDependent = new("source-dependent");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class Contiguity {
    public static readonly Contiguity Dense = new("dense", reshapeable: true);
    public static readonly Contiguity Strided = new("strided", reshapeable: false);
    public static readonly Contiguity Broadcast = new("broadcast", reshapeable: false);

    public bool Reshapeable { get; }

    public static Contiguity Classify<T>(Tensor<T> tensor) where T : unmanaged =>
        tensor.IsDense ? Dense : tensor.Strides.Contains((nint)0) ? Broadcast : Strided;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record DestinationOp<T> {
    private DestinationOp() { }

    [Equatable]
    public sealed partial record Broadcast([property: OrderedEquality] ImmutableArray<nint> Lengths) : DestinationOp<T>;
    [Equatable]
    public sealed partial record Resize([property: OrderedEquality] ImmutableArray<nint> Lengths) : DestinationOp<T>;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ReshapeOp<T> {
    private ReshapeOp() { }

    [Equatable]
    public sealed partial record Permute([property: OrderedEquality] ImmutableArray<int> Axes) : ReshapeOp<T>;
    public sealed record Transpose : ReshapeOp<T>;
    public sealed record Squeeze(Option<int> Dimension) : ReshapeOp<T>;
    public sealed record Unsqueeze(int Dimension) : ReshapeOp<T>;
    [Equatable]
    public sealed partial record Reshape([property: OrderedEquality] ImmutableArray<nint> Lengths) : ReshapeOp<T>;
    public sealed record Flatten(int Start, int Count) : ReshapeOp<T>;
    public sealed record Densify : ReshapeOp<T>;
    [Equatable]
    public sealed partial record Concatenate([property: OrderedEquality] ImmutableArray<Tensor<T>> Others, int Dimension) : ReshapeOp<T>;
    [Equatable]
    public sealed partial record Stack([property: OrderedEquality] ImmutableArray<Tensor<T>> Others, int Dimension) : ReshapeOp<T>;
    public sealed record Reverse(Option<int> Dimension) : ReshapeOp<T>;
    [Equatable]
    public sealed partial record Slice([property: OrderedEquality] ImmutableArray<NRange> Ranges) : ReshapeOp<T>;
    [Equatable]
    public sealed partial record Pad([property: OrderedEquality] ImmutableArray<nint> Before, [property: OrderedEquality] ImmutableArray<nint> After) : ReshapeOp<T>;
    public sealed record Roll(int Shift, int Dimension) : ReshapeOp<T>;
    public sealed record Write(DestinationOp<T> Form) : ReshapeOp<T>;

    public StorageClass Storage => Switch(
        permute: static _ => StorageClass.Shared, transpose: static _ => StorageClass.Shared,
        squeeze: static _ => StorageClass.Shared, unsqueeze: static _ => StorageClass.Shared,
        reverse: static _ => StorageClass.Shared, slice: static _ => StorageClass.Shared,
        reshape: static _ => StorageClass.SourceDependent, flatten: static _ => StorageClass.SourceDependent,
        densify: static _ => StorageClass.SourceDependent, roll: static _ => StorageClass.SourceDependent,
        concatenate: static _ => StorageClass.Materialized, stack: static _ => StorageClass.Materialized,
        pad: static _ => StorageClass.Materialized, write: static _ => StorageClass.Materialized);
}

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct AxisPermutation {
    private AxisPermutation(ImmutableArray<int> axes) => Axes = axes;

    public ImmutableArray<int> Axes { get; }

    public static Fin<AxisPermutation> Admit(ReadOnlySpan<int> axes, int rank) {
        ImmutableArray<int> order = [.. axes];
        return order.Length == rank && Enumerable.Range(0, rank).All(i => order.Contains(i))
            ? Fin.Succ(new AxisPermutation(order))
            : TensorReason.PermutationInvalid.Fail<AxisPermutation>("not-a-permutation", $"len={order.Length}:rank={rank}");
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class TensorLayout {
    public static Fin<Tensor<T>> Reform<T>(Tensor<T> source, LayoutForm origin, LayoutForm target) where T : unmanaged =>
        source.Rank != origin.Rank
            ? TensorReason.ShapeMismatch.Fail<Tensor<T>>("reform-rank", $"{origin.Key}:{source.Rank}")
            : origin.AxisMap(target).Bind(axes => AxisPermutation.Admit(axes.AsSpan(), origin.Rank)).Map(p => Tensor.PermuteDimensions(source, p.Axes.AsSpan()));

    public static Fin<Tensor<T>> Apply<T>(Tensor<T> source, ReshapeOp<T> op) where T : unmanaged =>
        op.Switch<Fin<Tensor<T>>>(
            permute: p => AxisPermutation.Admit(p.Axes.AsSpan(), source.Rank).Map(perm => Tensor.PermuteDimensions(source, perm.Axes.AsSpan())),
            transpose: _ => source.Rank >= 2 ? Fin.Succ(Tensor.Transpose(source)) : TensorReason.ShapeMismatch.Fail<Tensor<T>>("transpose-rank", source.Rank.ToString(CultureInfo.InvariantCulture)),
            squeeze: s => s.Dimension.Match(
                Some: d => Axis(d, source.Rank).Bind(ax => source.Lengths[ax] == 1
                    ? Fin.Succ(Tensor.SqueezeDimension(source, ax))
                    : TensorReason.ShapeMismatch.Fail<Tensor<T>>("squeeze-nonunit", $"{ax}:{source.Lengths[ax]}")),
                None: () => Fin.Succ(Tensor.Squeeze(source))),
            unsqueeze: u => Axis(u.Dimension, source.Rank + 1).Map(d => Tensor.Unsqueeze(source, d)),
            reshape: r => Reshaped(source, r.Lengths.AsSpan()),
            flatten: f => FlatLengths(source, f.Start, f.Count).Bind(lengths => Reshaped(source, lengths.AsSpan())),
            densify: _ => Fin.Succ(source.ToDenseTensor()),
            concatenate: c => JoinCompatible(source, c.Others, c.Dimension, Some(c.Dimension)).Map(_ => Tensor.ConcatenateOnDimension(c.Dimension, [source, .. c.Others])),
            stack: k => JoinCompatible(source, k.Others, k.Dimension, None).Map(_ => Tensor.StackAlongDimension(k.Dimension, [source, .. k.Others])),
            reverse: v => v.Dimension.Match(
                Some: d => Axis(d, source.Rank).Map(ax => Tensor.ReverseDimension(source, ax)),
                None: () => Fin.Succ(Tensor.Reverse(source))),
            slice: l => SliceRanges(source, l.Ranges).Map(ranges => source.Slice(ranges.AsSpan())),
            pad: p => PadLengths(source, p.Before, p.After).Map(padded => Padded(source, p.Before, padded)),
            roll: r => Axis(r.Dimension, source.Rank).Map(axis => Rolled(source, r.Shift, axis)),
            write: w => Written(source, w.Form));

    public static Fin<Unit> Apply<T>(Tensor<T> source, DestinationOp<T> op, in TensorSpan<T> destination) where T : unmanaged =>
        op.Switch(
            state: destination,
            broadcast: (target, request) => BroadcastTo(source, request.Lengths.AsSpan(), target),
            resize: (target, request) => ResizeTo(source, request.Lengths.AsSpan(), target));

    public static Fin<Seq<Tensor<T>>> Split<T>(Tensor<T> source, int count, nint dimension) where T : unmanaged =>
        SplitEven(source, count, dimension).Map(_ => toSeq(Tensor.Split(source, count, dimension)));

    private static Fin<Tensor<T>> Written<T>(Tensor<T> source, DestinationOp<T> form) where T : unmanaged =>
        form.Switch<Fin<Tensor<T>>>(
            broadcast: b => Broadcastable(source, b.Lengths.AsSpan()).Map(_ => Tensor.Broadcast(source, b.Lengths.AsSpan())),
            resize: z => PositiveLengths(z.Lengths.AsSpan(), "resize-extent").Map(_ => Tensor.Resize(source, z.Lengths.AsSpan())));

    private static Fin<Unit> BroadcastTo<T>(Tensor<T> source, ReadOnlySpan<nint> lengths, in TensorSpan<T> target) where T : unmanaged {
        if (!target.Lengths.SequenceEqual(lengths)) { return TensorReason.ShapeMismatch.Fail<Unit>("destination-shape", "broadcast"); }
        if (Broadcastable(source, lengths) is { IsFail: true } refused) { return refused; }
        return Tensor.TryBroadcastTo(source, target)
            ? Fin.Succ(unit)
            : TensorReason.ShapeMismatch.Fail<Unit>("broadcast-destination", $"rank={source.Rank}");
    }

    private static Fin<Unit> ResizeTo<T>(Tensor<T> source, ReadOnlySpan<nint> lengths, in TensorSpan<T> target) where T : unmanaged {
        if (!target.Lengths.SequenceEqual(lengths)) { return TensorReason.ShapeMismatch.Fail<Unit>("destination-shape", "resize"); }
        if (PositiveLengths(lengths, "resize-extent") is { IsFail: true } refused) { return refused; }
        Tensor.ResizeTo(source, target);
        return Fin.Succ(unit);
    }

    private static Fin<Unit> PositiveLengths(ReadOnlySpan<nint> lengths, string symbol) {
        foreach (nint extent in lengths) {
            if (extent <= 0) { return symbol.Fail<Unit>(extent.ToString(CultureInfo.InvariantCulture)); }
        }
        return Fin.Succ(unit);
    }

    private static Fin<ImmutableArray<NRange>> SliceRanges<T>(Tensor<T> source, ImmutableArray<NRange> ranges) where T : unmanaged =>
        ranges.Length != source.Rank
            ? TensorReason.ShapeMismatch.Fail<ImmutableArray<NRange>>("slice-rank", $"{ranges.Length}!={source.Rank}")
            : Try.lift(() => {
                  for (int axis = 0; axis < ranges.Length; axis++) { _ = ranges[axis].GetOffsetAndLength(source.Lengths[axis]); }
                  return Fin.Succ(ranges);
              }).Run().Bind(static inner => inner);

    private static Fin<ImmutableArray<nint>> PadLengths<T>(Tensor<T> source, ImmutableArray<nint> before, ImmutableArray<nint> after) where T : unmanaged =>
        before.Length != source.Rank || after.Length != source.Rank ? TensorReason.ShapeMismatch.Fail<ImmutableArray<nint>>("pad-rank", $"{before.Length}/{after.Length}/{source.Rank}")
        : before.Any(static d => d < 0) || after.Any(static d => d < 0) ? TensorReason.ShapeMismatch.Fail<ImmutableArray<nint>>("pad-negative", "extent")
        : Try.lift(() => {
              ImmutableArray<nint>.Builder padded = ImmutableArray.CreateBuilder<nint>(source.Rank);
              for (int axis = 0; axis < source.Rank; axis++) { padded.Add(checked(source.Lengths[axis] + before[axis] + after[axis])); }
              return Fin.Succ(padded.MoveToImmutable());
          }).Run().Bind(static inner => inner);

    private static Tensor<T> Padded<T>(Tensor<T> source, ImmutableArray<nint> before, ImmutableArray<nint> lengths) where T : unmanaged {
        Tensor<T> padded = Tensor.CreateFromShape<T>(lengths.AsSpan());
        NRange[] interior = Full(source.Rank);
        for (int axis = 0; axis < interior.Length; axis++) {
            interior[axis] = new NRange(NIndex.FromStart(before[axis]), NIndex.FromStart(before[axis] + source.Lengths[axis]));
        }
        source.AsReadOnlyTensorSpan().CopyTo(padded.Slice(interior).AsTensorSpan());
        return padded;
    }

    private static Tensor<T> Rolled<T>(Tensor<T> source, int shift, int axis) where T : unmanaged {
        nint extent = source.Lengths[axis];
        nint offset = extent == 0 ? 0 : ((shift % extent) + extent) % extent;
        if (offset == 0) { return source; }
        NRange[] head = Full(source.Rank);
        NRange[] tail = Full(source.Rank);
        head[axis] = new NRange(NIndex.FromStart(0), NIndex.FromStart(extent - offset));
        tail[axis] = new NRange(NIndex.FromStart(extent - offset), NIndex.FromStart(extent));
        return Tensor.ConcatenateOnDimension(axis, [source.Slice(tail), source.Slice(head)]);
    }

    private static NRange[] Full(int rank) {
        NRange[] full = new NRange[rank];
        full.AsSpan().Fill(NRange.All);
        return full;
    }

    private static Fin<int> Axis(int dimension, int upperExclusive) =>
        dimension >= 0 && dimension < upperExclusive ? Fin.Succ(dimension) : TensorReason.AxisOutOfRange.Fail<int>("axis-range", $"{dimension}/{upperExclusive}");

    private static Fin<Tensor<T>> Reshaped<T>(Tensor<T> source, ReadOnlySpan<nint> lengths) where T : unmanaged {
        ImmutableArray<nint> declared = [.. lengths];
        int wildcards = declared.Count(static d => d == -1);
        Validation<Error, Unit> floor = declared.Any(static d => d < -1) ? TensorReason.ShapeMismatch.Fault("reshape-extent", $"rank={declared.Length}") : unit;
        Validation<Error, Unit> single = wildcards > 1 ? TensorReason.ShapeMismatch.Fault("reshape-wildcard", wildcards.ToString(CultureInfo.InvariantCulture)) : unit;
        BigInteger known = declared.Where(static d => d != -1).Aggregate(BigInteger.One, static (acc, d) => acc * d);
        BigInteger flat = source.FlattenedLength;
        return (floor, single).Apply(static (_, _) => unit).As().ToFin().Bind(_ =>
            wildcards == 0 && known != flat ? TensorReason.ShapeMismatch.Fail<Tensor<T>>("reshape-product", $"{known}!={flat}")
            : wildcards == 1 && (known == 0 || flat % known != 0) ? TensorReason.ShapeMismatch.Fail<Tensor<T>>("reshape-indivisible", $"{flat}%{known}")
            : Fin.Succ(Tensor.Reshape(Contiguity.Classify(source).Reshapeable ? source : source.ToDenseTensor(), declared.AsSpan())));
    }

    private static Fin<ImmutableArray<nint>> FlatLengths<T>(Tensor<T> source, int start, int count) where T : unmanaged =>
        start >= 0 && count >= 1 && start + count <= source.Rank
            ? Fin.Succ<ImmutableArray<nint>>([.. source.Lengths[..start], TensorPrimitives.Product<nint>(source.Lengths[start..(start + count)]), .. source.Lengths[(start + count)..]])
            : TensorReason.AxisOutOfRange.Fail<ImmutableArray<nint>>("flatten-range", $"{start}+{count}/{source.Rank}");

    private static Fin<Unit> Broadcastable<T>(Tensor<T> source, ReadOnlySpan<nint> lengths) where T : unmanaged {
        ReadOnlySpan<nint> from = source.Lengths;
        if (lengths.Length < from.Length) { return TensorReason.ShapeMismatch.Fail<Unit>("broadcast-rank", $"{lengths.Length}<{from.Length}"); }
        foreach (nint extent in lengths) {
            if (extent <= 0) { return TensorReason.ShapeMismatch.Fail<Unit>("broadcast-extent", $"rank={lengths.Length}"); }
        }
        for (int k = 1; k <= from.Length; k++) {
            if (from[^k] != 1 && from[^k] != lengths[^k]) { return TensorReason.ShapeMismatch.Fail<Unit>("broadcast-incompatible", $"{from.Length}->{lengths.Length}"); }
        }
        return Fin.Succ(unit);
    }

    private static Fin<Unit> JoinCompatible<T>(Tensor<T> source, ImmutableArray<Tensor<T>> others, int dimension, Option<int> exceptAxis) where T : unmanaged {
        int rank = source.Rank;
        int upper = exceptAxis.Match(Some: static _ => rank, None: () => rank + 1);
        Validation<Error, Unit> populated = others.IsDefaultOrEmpty ? TensorReason.EmptyOperand.Fault("join-empty", "others") : unit;
        Validation<Error, Unit> axis = dimension >= 0 && dimension < upper ? unit : TensorReason.AxisOutOfRange.Fault("join-axis", $"{dimension}/{rank}");
        Validation<Error, Unit> shapes = others.All(o => o.Rank == rank && CompatibleShape(source.Lengths, o.Lengths, exceptAxis))
            ? unit : TensorReason.ShapeMismatch.Fault("join-shape", $"rank={rank}");
        return (populated, axis, shapes).Apply(static (_, _, _) => unit).As().ToFin();
    }

    private static bool CompatibleShape(ReadOnlySpan<nint> left, ReadOnlySpan<nint> right, Option<int> exceptAxis) {
        if (left.Length != right.Length) { return false; }
        for (int axis = 0; axis < left.Length; axis++) {
            if (left[axis] != right[axis] && !exceptAxis.Map(free => free == axis).IfNone(false)) { return false; }
        }
        return true;
    }

    private static Fin<Unit> SplitEven<T>(Tensor<T> source, int count, nint dimension) where T : unmanaged =>
        dimension < 0 || dimension >= source.Rank ? TensorReason.AxisOutOfRange.Fail<Unit>("split-axis", $"{dimension}/{source.Rank}")
        : count <= 0 ? TensorReason.ShapeMismatch.Fail<Unit>("split-count", count.ToString(CultureInfo.InvariantCulture))
        : source.Lengths[(int)dimension] % count != 0 ? TensorReason.ShapeMismatch.Fail<Unit>("split-uneven", $"{source.Lengths[(int)dimension]}%{count}")
        : Fin.Succ(unit);
}
```
