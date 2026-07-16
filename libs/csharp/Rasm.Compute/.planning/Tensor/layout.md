# [COMPUTE_LAYOUT]

Rasm.Compute cpu-tensor layout algebra: one `LayoutForm` named-shape vocabulary whose axis-label correspondence derives every layout permutation, one `Contiguity` memory-order classification over the `Tensor<T>` stride facts, one `AxisPermutation` admission proving a bijection the host kernel never checks, and one generic `ReshapeOp<T>` request union owning the whole shape-edit verb family under one total compile-exhaustive `Switch` — singular verbs yield a one-element `Seq`, the plural `Split` alone spans multiple segments. `TensorLayout.Reform` and `TensorLayout.Apply` are the entrypoints.

Layout members ride `System.Numerics.Tensors`; `TensorDtype`/`TensorFault` arrive settled from `Tensor/vocabulary#TENSOR_VOCABULARY`. `LayoutForm` rows are the geometry-encoding wire-shape targets `Tensor/residency#GEOMETRY_ENCODING` reads and the nchw↔nhwc permutation the CoreML image-model route consumes.

## [01]-[INDEX]

- [01]-[LAYOUT_ALGEBRA]: named-shape `LayoutForm` vocabulary, contiguity classification, and the `ReshapeOp<T>` shape-edit verb union under one total `Switch`.

## [02]-[LAYOUT_ALGEBRA]

- Owner: `LayoutForm` (named wire-shape vocabulary, axis-label permutation source) + `Contiguity` (memory-order classification) + `AxisPermutation` (admitted bijection) + `ReshapeOp<T>` (the generic `[Union]` shape-edit request family) under the `TensorLayout.Reform`/`TensorLayout.Apply` entrypoints.
- Cases: `LayoutForm` rows dense | nxc | vertex-face | nchw | nhwc (rank derives from the axis-label count); `Contiguity` rows dense | strided | broadcast (each carrying its `Reshapeable` column); the `ReshapeOp<T>` verbs `Permute` | `Transpose` | `Squeeze` | `Unsqueeze` | `Reshape` | `Flatten` | `Densify` | `Broadcast` | `Concatenate` | `Stack` | `Split` | `Reverse` | `Resize` | `Slice` | `Pad` | `Roll`.
- Entry: `public static Fin<Tensor<T>> Reform<T>(Tensor<T> source, LayoutForm origin, LayoutForm target)` for a named layout transition whose permutation derives from the axis-label correspondence; `public static Fin<Seq<Tensor<T>>> Apply<T>(Tensor<T> source, ReshapeOp<T> op)` for the general shape-edit request, every verb a `Seq` arm of the one entrypoint; the destination-polarity overload `public static Fin<Unit> Apply<T>(Tensor<T> source, ReshapeOp<T> op, in TensorSpan<T> destination)` threads the ref-struct destination as generated `Switch` state, requires its shape to equal the request, and writes `Broadcast` through `Tensor.TryBroadcastTo` or `Resize` through `Tensor.ResizeTo` with zero allocation while every view verb faults `destination-form`.
- Packages: System.Numerics.Tensors, Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox
- Growth: a new layout is one `LayoutForm` row carrying its `axisLabels` — every permutation to and from a same-label-set sibling DERIVES through `AxisMap`, zero permute-table edits; a new shape-edit verb is one `ReshapeOp<T>` case breaking the total `Switch` at every dispatch site at compile time; a new memory order is one `Contiguity` row carrying its `Reshapeable` column; zero new surface.
- Boundary: `ReshapeOp<T>` is generic, so `Concatenate`/`Stack` carry same-typed `Tensor<T>[]` operands feeding the variadic `ConcatenateOnDimension`/`StackAlongDimension` `params ReadOnlySpan<Tensor<T>>` directly; a float-locked `Tensor<float>[]` payload with runtime `Cast<Tensor<T>>` is rejected. Both `Apply` forms dispatch through generated total `Switch` overloads whose named arms break at compile time when a verb lands; the destination form uses the state-threaded overload because `TensorSpan<T>` cannot cross a closure. Every arm admits its structural invariant before the host call so `Fin<T>` carries `TensorFault`: `AxisPermutation.Admit` proves a bijection over `0..rank-1`; `Reshaped` resolves one `-1` wildcard and compares an overflow-free `BigInteger` product with `FlattenedLength`; `Broadcastable` checks right-aligned compatibility; `SplitEven` checks axis divisibility; `Axis` bounds dimensions; `Transpose` requires rank ≥ 2; `SqueezeDimension` requires a unit axis. `Reform` also rejects duplicate axis labels before deriving nchw→nhwc `[0,2,3,1]` or nhwc→nchw `[0,3,1,2]`. `Pad` captures checked extent addition, materializes `CreateFromShape`, and copies the source into the interior slice. `Roll` normalizes the shift modulo the axis extent and concatenates complementary slices. `Reshape` and `Resize` reject non-positive extents, while `Slice` captures `NRange.GetOffsetAndLength`. Gather, scatter, and take-along-axis read or write by index value and remain `Tensor/dispatch#KERNEL_DISPATCH` structural operations. `Permute`/`Transpose`/`Squeeze`/`Unsqueeze`/`Slice`/`Reverse` and dense `Reshape`/`Flatten` share storage; `Broadcast`/`Resize`/`Split` materialize; `Densify` shares dense sources and materializes others. `Flatten` folds a dimension range through `Reshape`, distinct from `Tensor<T>.FlattenTo(Span<T>)`. `Contiguity.Classify` reads `Tensor<T>.IsDense` and zero strides. Region writes remain dispatch-owned.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class LayoutForm {
    public static readonly LayoutForm Dense = new("dense", axisLabels: ["L"]);
    public static readonly LayoutForm NxC = new("nxc", axisLabels: ["N", "C"]);
    public static readonly LayoutForm VertexFace = new("vertex-face", axisLabels: ["V", "F"]);
    public static readonly LayoutForm Nchw = new("nchw", axisLabels: ["N", "C", "H", "W"]);
    public static readonly LayoutForm Nhwc = new("nhwc", axisLabels: ["N", "H", "W", "C"]);

    public string[] AxisLabels { get; }
    public int Rank => AxisLabels.Length;

    // Result axis j holds the source axis matching target.AxisLabels[j].
    public Fin<int[]> AxisMap(LayoutForm target) =>
        Rank == target.Rank
        && AxisLabels.Distinct(StringComparer.Ordinal).Count() == Rank
        && target.AxisLabels.Distinct(StringComparer.Ordinal).Count() == target.Rank
        && AxisLabels.ToHashSet(StringComparer.Ordinal).SetEquals(target.AxisLabels)
            ? Fin.Succ<int[]>([.. target.AxisLabels.Select(label => Array.IndexOf(AxisLabels, label))])
            : TensorFault.Fail<int[]>("incompatible-forms", $"{Key}->{target.Key}");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class Contiguity {
    public static readonly Contiguity Dense = new("dense", reshapeable: true);
    public static readonly Contiguity Strided = new("strided", reshapeable: false);
    public static readonly Contiguity Broadcast = new("broadcast", reshapeable: true);

    public bool Reshapeable { get; }

    public static Contiguity Classify<T>(Tensor<T> tensor) where T : unmanaged =>
        tensor.IsDense ? Dense : tensor.Strides.Contains((nint)0) ? Broadcast : Strided;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ReshapeOp<T> {
    private ReshapeOp() { }

    public sealed record Permute(int[] Axes) : ReshapeOp<T>;
    public sealed record Transpose : ReshapeOp<T>;
    public sealed record Squeeze(Option<int> Dimension) : ReshapeOp<T>;
    public sealed record Unsqueeze(int Dimension) : ReshapeOp<T>;
    public sealed record Reshape(nint[] Lengths) : ReshapeOp<T>;
    public sealed record Flatten(int Start, int Count) : ReshapeOp<T>;
    public sealed record Densify : ReshapeOp<T>;
    public sealed record Broadcast(nint[] Lengths) : ReshapeOp<T>;
    public sealed record Concatenate(Tensor<T>[] Others, int Dimension) : ReshapeOp<T>;
    public sealed record Stack(Tensor<T>[] Others, int Dimension) : ReshapeOp<T>;
    public sealed record Split(int Count, int Dimension) : ReshapeOp<T>;
    public sealed record Reverse(Option<int> Dimension) : ReshapeOp<T>;
    public sealed record Resize(nint[] Lengths) : ReshapeOp<T>;
    public sealed record Slice(NRange[] Ranges) : ReshapeOp<T>;
    public sealed record Pad(nint[] Before, nint[] After) : ReshapeOp<T>;
    public sealed record Roll(int Shift, int Dimension) : ReshapeOp<T>;
}

// --- [MODELS] ------------------------------------------------------------------------------
public readonly record struct AxisPermutation {
    private AxisPermutation(int[] axes) => Axes = axes;

    public int[] Axes { get; }

    // length == rank AND every value 0..rank-1 present => a proven bijection (the host checks neither).
    public static Fin<AxisPermutation> Admit(ReadOnlySpan<int> axes, int rank) {
        int[] order = axes.ToArray();
        return order.Length == rank && Enumerable.Range(0, rank).All(i => Array.IndexOf(order, i) >= 0)
            ? Fin.Succ(new AxisPermutation(order))
            : TensorFault.Fail<AxisPermutation>("not-a-permutation", $"len={order.Length}:rank={rank}");
    }
}

// --- [OPERATIONS] --------------------------------------------------------------------------
public static class TensorLayout {
    public static Fin<Tensor<T>> Reform<T>(Tensor<T> source, LayoutForm origin, LayoutForm target) where T : unmanaged =>
        source.Rank != origin.Rank
            ? TensorFault.Fail<Tensor<T>>("reform-rank", $"{origin.Key}:{source.Rank}")
            : origin.AxisMap(target).Bind(axes => AxisPermutation.Admit(axes, origin.Rank)).Map(p => Tensor.PermuteDimensions(source, p.Axes));

    public static Fin<Seq<Tensor<T>>> Apply<T>(Tensor<T> source, ReshapeOp<T> op) where T : unmanaged =>
        op.Switch<Fin<Seq<Tensor<T>>>>(
            permute: p => AxisPermutation.Admit(p.Axes, source.Rank).Bind(perm => One(Tensor.PermuteDimensions(source, perm.Axes))),
            transpose: _ => source.Rank >= 2 ? One(Tensor.Transpose(source)) : TensorFault.Fail<Seq<Tensor<T>>>("transpose-rank", source.Rank.ToString()),
            squeeze: s => s.Dimension.Match(
                Some: d => Axis(d, source.Rank).Bind(ax => source.Lengths[ax] == 1
                    ? One(Tensor.SqueezeDimension(source, ax))
                    : TensorFault.Fail<Seq<Tensor<T>>>("squeeze-nonunit", $"{ax}:{source.Lengths[ax]}")),
                None: () => One(Tensor.Squeeze(source))),
            unsqueeze: u => Axis(u.Dimension, source.Rank + 1).Bind(d => One(Tensor.Unsqueeze(source, d))),
            reshape: r => Reshaped(source, r.Lengths).Map(Seq),
            flatten: f => FlatLengths(source, f.Start, f.Count).Bind(lengths => Reshaped(source, lengths)).Map(Seq),
            densify: _ => One(source.ToDenseTensor()),
            broadcast: b => Broadcastable(source, b.Lengths).Bind(_ => One(Tensor.Broadcast(source, b.Lengths))),
            concatenate: c => JoinCompatible(source, c.Others, c.Dimension, stack: false).Bind(_ => One(Tensor.ConcatenateOnDimension(c.Dimension, [source, .. c.Others]))),
            stack: k => JoinCompatible(source, k.Others, k.Dimension, stack: true).Bind(_ => One(Tensor.StackAlongDimension(k.Dimension, [source, .. k.Others]))),
            split: l => SplitEven(source, l.Count, l.Dimension).Map(_ => toSeq(Tensor.Split(source, l.Count, l.Dimension))),
            reverse: v => v.Dimension.Match(
                Some: d => Axis(d, source.Rank).Bind(ax => One(Tensor.ReverseDimension(source, ax))),
                None: () => One(Tensor.Reverse(source))),
            resize: z => PositiveLengths(z.Lengths, "resize-extent").Bind(_ => One(Tensor.Resize(source, z.Lengths))),
            slice: l => SliceRanges(source, l.Ranges).Bind(ranges => One(source.Slice(ranges))),
            pad: p => PadLengths(source, p.Before, p.After).Bind(padded => One(Padded(source, p.Before, padded))),
            roll: r => Axis(r.Dimension, source.Rank).Map(axis => Seq(Rolled(source, r.Shift, axis))));

    public static Fin<Unit> Apply<T>(Tensor<T> source, ReshapeOp<T> op, in TensorSpan<T> destination) where T : unmanaged =>
        op.Switch(
            state: destination,
            permute: static (_, _) => DestinationForm("permute"),
            transpose: static (_, _) => DestinationForm("transpose"),
            squeeze: static (_, _) => DestinationForm("squeeze"),
            unsqueeze: static (_, _) => DestinationForm("unsqueeze"),
            reshape: static (_, _) => DestinationForm("reshape"),
            flatten: static (_, _) => DestinationForm("flatten"),
            densify: static (_, _) => DestinationForm("densify"),
            broadcast: (target, request) => BroadcastTo(source, request.Lengths, target),
            concatenate: static (_, _) => DestinationForm("concatenate"),
            stack: static (_, _) => DestinationForm("stack"),
            split: static (_, _) => DestinationForm("split"),
            reverse: static (_, _) => DestinationForm("reverse"),
            resize: (target, request) => ResizeTo(source, request.Lengths, target),
            slice: static (_, _) => DestinationForm("slice"),
            pad: static (_, _) => DestinationForm("pad"),
            roll: static (_, _) => DestinationForm("roll"));

    private static Fin<Seq<Tensor<T>>> One<T>(Tensor<T> tensor) where T : unmanaged => Fin.Succ(Seq(tensor));

    private static Fin<Unit> DestinationForm(string key) => TensorFault.Fail<Unit>("destination-form", key);

    // Broadcastable is the one compatibility oracle; this destination form adds only the shape-equality gate
    // and the host write, so the right-alignment law never forks into a second spelling.
    private static Fin<Unit> BroadcastTo<T>(Tensor<T> source, ReadOnlySpan<nint> lengths, in TensorSpan<T> target) where T : unmanaged {
        if (!target.Lengths.SequenceEqual(lengths)) { return TensorFault.Fail<Unit>("destination-shape", "broadcast"); }
        Fin<Unit> compatible = Broadcastable(source, lengths);
        return compatible.Case is not Unit ? compatible
            : Tensor.TryBroadcastTo(source, target) ? Fin.Succ(unit)
            : TensorFault.Fail<Unit>("broadcast-destination", $"rank={source.Rank}");
    }

    private static Fin<Unit> ResizeTo<T>(Tensor<T> source, ReadOnlySpan<nint> lengths, in TensorSpan<T> target) where T : unmanaged {
        if (!target.Lengths.SequenceEqual(lengths)) { return TensorFault.Fail<Unit>("destination-shape", "resize"); }
        foreach (nint extent in lengths) {
            if (extent <= 0) { return TensorFault.Fail<Unit>("resize-extent", extent.ToString(CultureInfo.InvariantCulture)); }
        }
        Tensor.ResizeTo(source, target);
        return Fin.Succ(unit);
    }

    private static Fin<Unit> PositiveLengths(ReadOnlySpan<nint> lengths, string symbol) {
        foreach (nint extent in lengths) {
            if (extent <= 0) { return TensorFault.Fail<Unit>(symbol, extent.ToString(CultureInfo.InvariantCulture)); }
        }
        return Fin.Succ(unit);
    }

    // GetOffsetAndLength is the captured bounds oracle; no raw range reaches the host slice.
    private static Fin<NRange[]> SliceRanges<T>(Tensor<T> source, NRange[] ranges) where T : unmanaged =>
        ranges.Length != source.Rank
            ? TensorFault.Fail<NRange[]>("slice-rank", $"{ranges.Length}!={source.Rank}")
            : Try.lift(() => {
                  for (int axis = 0; axis < ranges.Length; axis++) { ranges[axis].GetOffsetAndLength(source.Lengths[axis]); }
                  return ranges;
              }).Run().MapFail(error => TensorFault.Symbol("slice-range", error.Message));

    private static Fin<nint[]> PadLengths<T>(Tensor<T> source, nint[] before, nint[] after) where T : unmanaged =>
        before.Length != source.Rank || after.Length != source.Rank ? TensorFault.Fail<nint[]>("pad-rank", $"{before.Length}/{after.Length}/{source.Rank}")
        : before.Any(static d => d < 0) || after.Any(static d => d < 0) ? TensorFault.Fail<nint[]>("pad-negative", "extent")
        : Try.lift(() => source.Lengths.ToArray().Select((extent, axis) => checked(extent + before[axis] + after[axis])).ToArray())
            .Run().MapFail(error => TensorFault.Symbol("pad-overflow", error.Message));

    private static Tensor<T> Padded<T>(Tensor<T> source, nint[] before, nint[] lengths) where T : unmanaged {
        Tensor<T> padded = Tensor.CreateFromShape<T>(lengths);
        NRange[] interior = [.. before.Select((offset, axis) => new NRange(NIndex.FromStart(offset), NIndex.FromStart(offset + source.Lengths[axis])))];
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

    private static NRange[] Full(int rank) => [.. Enumerable.Repeat(NRange.All, rank)];

    private static Fin<int> Axis(int dimension, int upperExclusive) =>
        dimension >= 0 && dimension < upperExclusive ? Fin.Succ(dimension) : TensorFault.Fail<int>("axis-range", $"{dimension}/{upperExclusive}");

    private static Fin<Tensor<T>> Reshaped<T>(Tensor<T> source, ReadOnlySpan<nint> lengths) where T : unmanaged {
        nint[] declared = lengths.ToArray();
        int wildcards = declared.Count(static d => d == -1);
        BigInteger known = declared.Where(static d => d != -1).Aggregate(BigInteger.One, static (acc, d) => acc * d);
        BigInteger flat = source.FlattenedLength;
        return declared.Any(static d => d == 0 || d < -1) ? TensorFault.Fail<Tensor<T>>("reshape-extent", $"rank={declared.Length}")
            : wildcards > 1 ? TensorFault.Fail<Tensor<T>>("reshape-wildcard", wildcards.ToString())
            : wildcards == 0 && known != flat ? TensorFault.Fail<Tensor<T>>("reshape-product", $"{known}!={flat}")
            : wildcards == 1 && (known == 0 || flat % known != 0) ? TensorFault.Fail<Tensor<T>>("reshape-indivisible", $"{flat}%{known}")
            : Fin.Succ(Tensor.Reshape(Contiguity.Classify(source).Reshapeable ? source : source.ToDenseTensor(), declared));
    }

    private static Fin<nint[]> FlatLengths<T>(Tensor<T> source, int start, int count) where T : unmanaged =>
        start >= 0 && count >= 1 && start + count <= source.Rank
            ? Fin.Succ<nint[]>([.. source.Lengths[..start], TensorPrimitives.Product<nint>(source.Lengths[start..(start + count)]), .. source.Lengths[(start + count)..]])
            : TensorFault.Fail<nint[]>("flatten-range", $"{start}+{count}/{source.Rank}");

    private static Fin<Unit> Broadcastable<T>(Tensor<T> source, ReadOnlySpan<nint> lengths) where T : unmanaged {
        nint[] from = source.Lengths.ToArray();
        nint[] to = lengths.ToArray();
        return to.Length < from.Length ? TensorFault.Fail<Unit>("broadcast-rank", $"{to.Length}<{from.Length}")
            : to.Any(static d => d <= 0) ? TensorFault.Fail<Unit>("broadcast-extent", $"rank={to.Length}")
            : Enumerable.Range(1, from.Length).All(k => from[^k] == 1 || from[^k] == to[^k]) ? Fin.Succ(unit)
            : TensorFault.Fail<Unit>("broadcast-incompatible", $"{from.Length}->{to.Length}");
    }

    private static Fin<Unit> JoinCompatible<T>(Tensor<T> source, Tensor<T>[] others, int dimension, bool stack) where T : unmanaged {
        int rank = source.Rank;
        return others.Length == 0 ? TensorFault.Fail<Unit>("join-empty", "others")
            : dimension < 0 || dimension >= (stack ? rank + 1 : rank) ? TensorFault.Fail<Unit>("join-axis", $"{dimension}/{rank}")
            : others.All(o => o.Rank == rank && CompatibleShape(source.Lengths, o.Lengths, stack ? -1 : dimension)) ? Fin.Succ(unit)
            : TensorFault.Fail<Unit>("join-shape", $"rank={rank}");
    }

    // exceptAxis == -1 (Stack) requires identical shapes; an axis index (Concatenate) lets that one axis differ.
    private static bool CompatibleShape(ReadOnlySpan<nint> left, ReadOnlySpan<nint> right, int exceptAxis) {
        nint[] a = left.ToArray();
        nint[] b = right.ToArray();
        return a.Length == b.Length && Enumerable.Range(0, a.Length).All(i => i == exceptAxis || a[i] == b[i]);
    }

    private static Fin<Unit> SplitEven<T>(Tensor<T> source, int count, int dimension) where T : unmanaged =>
        dimension < 0 || dimension >= source.Rank ? TensorFault.Fail<Unit>("split-axis", $"{dimension}/{source.Rank}")
        : count <= 0 ? TensorFault.Fail<Unit>("split-count", count.ToString())
        : source.Lengths[dimension] % count != 0 ? TensorFault.Fail<Unit>("split-uneven", $"{source.Lengths[dimension]}%{count}")
        : Fin.Succ(unit);
}
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
