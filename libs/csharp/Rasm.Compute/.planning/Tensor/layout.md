# [COMPUTE_LAYOUT]

The cpu-tensor layout algebra: one `LayoutForm` named-shape vocabulary whose axis-label correspondence DERIVES every layout permutation, one `Contiguity` memory-order classification over the `Tensor<T>` stride facts, one `AxisPermutation` admission proving a bijection the host kernel never checks, and one generic `ReshapeOp<T>` request union owning the whole shape-edit verb family under one total compile-exhaustive `Switch` — the singular verbs yielding a one-element `Seq`, the plural `Split` the only multi-segment arm. The page owns the `LayoutForm` row vocabulary and its `AxisMap` permutation derivation, the `Contiguity` classification, the `AxisPermutation` admission, the `ReshapeOp<T>` request union over the fourteen shape-edit verbs, and the `TensorLayout.Reform`/`TensorLayout.Apply` entrypoints; the layout members ride `System.Numerics.Tensors`, and the `TensorDtype`/`TensorFault`/`ComparerAccessors.StringOrdinal` arrive settled from `Tensor/vocabulary#TENSOR_VOCABULARY`. The `LayoutForm` rows are the geometry-encoding wire-shape targets `Tensor/residency#GEOMETRY_ENCODING` reads and the nchw↔nhwc permutation the CoreML image-model route consumes.

## [01]-[INDEX]

- [01]-[LAYOUT_ALGEBRA]: `LayoutForm` rows + `AxisMap` permutation derivation; `Contiguity` classification; `AxisPermutation` admission; `ReshapeOp<T>` request union over the layout family under one total `Switch`.

## [02]-[LAYOUT_ALGEBRA]

- Owner: `LayoutForm` (named wire-shape vocabulary, axis-label permutation source) + `Contiguity` (memory-order classification) + `AxisPermutation` (admitted bijection) + `ReshapeOp<T>` (the generic `[Union]` shape-edit request family) under the `TensorLayout.Reform`/`TensorLayout.Apply` entrypoints.
- Cases: `LayoutForm` rows dense | nxc | vertex-face | nchw | nhwc (rank derives from the axis-label count); `Contiguity` rows dense | strided | broadcast (each carrying its `Reshapeable` column); the `ReshapeOp<T>` verbs `Permute` | `Transpose` | `Squeeze` | `Unsqueeze` | `Reshape` | `Flatten` | `Densify` | `Broadcast` | `Concatenate` | `Stack` | `Split` | `Reverse` | `Resize` | `Slice`.
- Entry: `public static Fin<Tensor<T>> Reform<T>(Tensor<T> source, LayoutForm origin, LayoutForm target)` for a named layout transition whose permutation derives from the axis-label correspondence; `public static Fin<Seq<Tensor<T>>> Apply<T>(Tensor<T> source, ReshapeOp<T> op)` for the general shape-edit request — every verb is a `Seq` arm of the one entrypoint, the singular verbs yielding a one-element `Seq` and the plural `Split` the multi-segment arm, so `Fin<T>` aborts on a rank/permutation/broadcast-incompatible request with a typed `TensorFault` and no verb leaves the dispatch as a poison case.
- Packages: System.Numerics.Tensors, Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox
- Growth: a new layout is one `LayoutForm` row carrying its `axisLabels` — every permutation to and from a same-label-set sibling DERIVES through `AxisMap`, zero permute-table edits; a new shape-edit verb is one `ReshapeOp<T>` case breaking the total `Switch` at every dispatch site at compile time; a new memory order is one `Contiguity` row carrying its `Reshapeable` column; zero new surface.
- Boundary: `ReshapeOp<T>` is GENERIC so `Concatenate`/`Stack` carry same-typed `Tensor<T>[]` operands and feed the variadic `ConcatenateOnDimension`/`StackAlongDimension` `params ReadOnlySpan<Tensor<T>>` directly — the float-locked `Tensor<float>[]` payload and its runtime `Cast<Tensor<T>>` are the deleted form. `Apply` dispatches through the generated `op.Switch<TResult>(...)` whose named per-case parameters make a new verb break every dispatch site at compile time — the raw `switch` expression with a runtime-silent `_ =>` poison arm is the deleted form. Every arm ADMITS its structural invariant before the host call so `Fin<T>` carries a typed `TensorFault` and never leaks an `ArgumentException`: `AxisPermutation.Admit` proves a bijection over `0..rank-1` and is STRICTLY STRONGER than `PermuteDimensions`, which validates only axis range and length and would otherwise mis-stride a duplicate-axis request into a silently malformed tensor; `Reshaped` resolves the single `-1` wildcard, checks the element product against `FlattenedLength`, and densifies a `Contiguity.Strided` backing through `ToDenseTensor` first because `Reshape` throws on a non-dense non-broadcast view; `Broadcastable` checks the right-aligned per-axis compatibility (each source axis is `1` or equal); `SplitEven` checks the axis divides evenly; `Axis` bounds the squeeze/unsqueeze/reverse/slice dimension (unsqueeze admits the inclusive `rank` insertion slot); `Transpose` requires rank ≥ 2; `SqueezeDimension` admits only a unit axis. `Reform` derives the permutation from the `AxisMap` label correspondence (nchw→nhwc = `[0,2,3,1]`, nhwc→nchw = `[0,3,1,2]` DERIVE, the hand-keyed permute table is the deleted form) — the mandatory CoreML image-model pre/post route; a same-rank pair with disjoint label sets faults `incompatible-forms`. `Permute`/`Transpose`/`Squeeze`/`Unsqueeze`/`Slice`/`Reverse` and a dense `Reshape`/`Flatten` SHARE storage (strided views over the same backing, never a copy); `Broadcast`/`Resize`/`Split` materialize at the target shape; `Densify` (`ToDenseTensor`) shares when the source is already `Contiguity.Dense` and materializes a dense backing otherwise, so an INDEPENDENT detached buffer is `Resize` to the same shape (always allocates and copies), never a defensive clone. `Flatten` folds a dimension range through `Reshape` (densifying a strided backing first), distinct from `Tensor<T>.FlattenTo(Span<T>)`, the strided→linear egress that writes a tensor's logical element order into a caller span. `Contiguity.Classify` reads `Tensor<T>.IsDense` and a zero `Strides` entry (the broadcast/expanded view) as the public pre-reshape probe, with `HasAnyDenseDimensions` the partial-density signal and `ToDenseTensor` the densify-on-egress when a strided view cannot reshape in place. Region writes — `SetSlice`, `FilteredUpdate`, the `MaskedWrite` row — are NOT layout verbs: they are the `Tensor/dispatch#KERNEL_DISPATCH` structural write rows, while the layout family is the rank/shape/order transforms that yield a new tensor view or backing.

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

    // result axis j holds the source axis whose label == target.AxisLabels[j]; PermuteDimensions reads this map.
    public Fin<int[]> AxisMap(LayoutForm target) =>
        Rank == target.Rank && AxisLabels.ToHashSet().SetEquals(target.AxisLabels)
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
            resize: z => One(Tensor.Resize(source, z.Lengths)),
            slice: l => l.Ranges.Length == source.Rank
                ? One(source.Slice(l.Ranges))
                : TensorFault.Fail<Seq<Tensor<T>>>("slice-rank", $"{l.Ranges.Length}!={source.Rank}"));

    private static Fin<Seq<Tensor<T>>> One<T>(Tensor<T> tensor) where T : unmanaged => Fin.Succ(Seq(tensor));

    private static Fin<int> Axis(int dimension, int upperExclusive) =>
        dimension >= 0 && dimension < upperExclusive ? Fin.Succ(dimension) : TensorFault.Fail<int>("axis-range", $"{dimension}/{upperExclusive}");

    private static Fin<Tensor<T>> Reshaped<T>(Tensor<T> source, ReadOnlySpan<nint> lengths) where T : unmanaged {
        nint[] declared = lengths.ToArray();
        int wildcards = declared.Count(static d => d == -1);
        nint known = declared.Where(static d => d != -1).Aggregate((nint)1, static (acc, d) => acc * d);
        nint flat = source.FlattenedLength;
        return wildcards > 1 ? TensorFault.Fail<Tensor<T>>("reshape-wildcard", wildcards.ToString())
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
