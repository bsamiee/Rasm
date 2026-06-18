# [COMPUTE_LAYOUT]

The cpu-tensor layout algebra: one `LayoutForm` algebra over the layout member surface with the `ReshapeOp` request union owning the whole shape-edit family under one total dispatch whose plural-result verb is a `Traverse` arm of the same entrypoint. The page owns the `LayoutForm` row vocabulary, the `ReshapeOp` request union over the fourteen shape-edit verbs, the `TensorLayout.Reform` named-transition surface, and the `TensorLayout.Apply` total-dispatch entrypoint; the layout members ride `System.Numerics.Tensors`, and the `TensorDtype`/`TensorFault`/`TensorKeyPolicy` arrive settled from `vocabulary#TENSOR_VOCABULARY`. The `LayoutForm` rows are the geometry-encoding wire-shape targets `residency#GEOMETRY_ENCODING` reads and the nchw↔nhwc permute rows the CoreML image-model route consumes.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]      | [OWNS]                                                                         |
| :-----: | -------------- | ----------------------------------------------------------------------------- |
|   [1]   | LAYOUT_ALGEBRA | LayoutForm rows; permute table; ReshapeOp request union over the layout family |

## [2]-[LAYOUT_ALGEBRA]

- Owner: `LayoutForm`; `ReshapeOp` the `[Union]` request family owning the whole shape-edit surface under one total dispatch.
- Cases: dense, nxc, vertex-face, nchw, nhwc — and the reshape verbs `Permute` | `Transpose` | `Squeeze` | `Unsqueeze` | `Reshape` | `Flatten` | `Densify` | `Broadcast` | `Concatenate` | `Stack` | `Split` | `Reverse` | `Resize` | `Slice`.
- Entry: `public static Fin<Tensor<T>> Reform<T>(Tensor<T> source, LayoutForm origin, LayoutForm target)` for the named layout transitions; `public static Fin<Seq<Tensor<T>>> Apply<T>(Tensor<T> source, ReshapeOp op)` for the general shape-edit request — every verb is a `Seq` arm of the one entrypoint, the singular verbs yielding a one-element `Seq` and the plural `Split` the `Traverse`-shaped multi-segment arm, so `Fin<T>` aborts on an undeclared permute row or a rank/broadcast-incompatible request and no verb leaves the dispatch as a poison case.
- Packages: System.Numerics.Tensors, CommunityToolkit.HighPerformance, Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox
- Growth: a new layout is one `LayoutForm` row plus one permute-table entry; a new shape-edit verb is one `ReshapeOp` case breaking the total dispatch at compile time; zero new surface.
- Boundary: the layout family — `PermuteDimensions`, `Transpose`, `Squeeze`, `SqueezeDimension`, `Unsqueeze`, `SetSlice`, `FilteredUpdate`, `Split`, `Stack`, `StackAlongDimension`, `Concatenate`, `ConcatenateOnDimension`, `Reverse`, `ReverseDimension`, `Resize`, `Broadcast`, `BroadcastTo`, `Reshape`, `FlattenTo`, `ToDenseTensor`, and `Slice` with `NIndex`/`NRange` — is the only layout surface, and the `ReshapeOp` request union collapses every verb into one case per verb under one total `Switch` (a new verb breaks every dispatch site, never a sibling `Permute`/`Reshape`/`Squeeze` method family); `Transpose` is the two-trailing-axis transpose distinct from the explicit-permutation `Permute`, both routing the layout family's reorder members; the nchw↔nhwc permute rows are the mandatory CoreML image-model pre/post route; `Slice` shares storage with adjusted strides through the instance `Tensor<T>.Slice(params ReadOnlySpan<NRange>)` and never copies while `Broadcast` materializes at the broadcast shape (validates, allocates, copies), so a scalar operand rides the scalar-position kernel overload, never a manufactured constant tensor; `Reverse` is the whole-tensor element-order reversal and `ReverseDimension` the per-dimension form (the `Reverse.Dimension` option routing the two), never a dimension argument smuggled onto whole-tensor `Reverse`; `Span2D`/`ReadOnlySpan2D` planes are views over dense backings (carried onto rented buffers via `AsSpan2D`, contiguity-probed via `TryGetSpan`) and never substitute for rank permutation; `Flatten` is the strided-to-linear `FlattenTo` bridge and `Densify` is `ToDenseTensor` which returns `this` on dense input so an independent buffer is `CreateFromShape` plus `CopyTo`, never a defensive copy; broadcast compatibility and rank/stride invariants ride `Broadcast`/`BroadcastTo` and the dimension spans, stated here once for the lane.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<TensorKeyPolicy, string>]
[KeyMemberComparer<TensorKeyPolicy, string>]
public sealed partial class LayoutForm {
    public static readonly LayoutForm Dense = new("dense", rank: 1);
    public static readonly LayoutForm NxC = new("nxc", rank: 2);
    public static readonly LayoutForm VertexFace = new("vertex-face", rank: 2);
    public static readonly LayoutForm Nchw = new("nchw", rank: 4);
    public static readonly LayoutForm Nhwc = new("nhwc", rank: 4);

    public int Rank { get; }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ReshapeOp {
    private ReshapeOp() { }

    public sealed record Permute(int[] Axes) : ReshapeOp;
    public sealed record Transpose : ReshapeOp;
    public sealed record Squeeze(Option<int> Dimension) : ReshapeOp;
    public sealed record Unsqueeze(int Dimension) : ReshapeOp;
    public sealed record Reshape(nint[] Lengths) : ReshapeOp;
    public sealed record Flatten(int Start, int Count) : ReshapeOp;
    public sealed record Densify : ReshapeOp;
    public sealed record Broadcast(nint[] Lengths) : ReshapeOp;
    public sealed record Concatenate(Tensor<float>[] Others, int Dimension) : ReshapeOp;
    public sealed record Stack(Tensor<float>[] Others, int Dimension) : ReshapeOp;
    public sealed record Split(int Count, int Dimension) : ReshapeOp;
    public sealed record Reverse(Option<int> Dimension) : ReshapeOp;
    public sealed record Resize(nint[] Lengths) : ReshapeOp;
    public sealed record Slice(NRange[] Ranges) : ReshapeOp;
}

// --- [OPERATIONS] --------------------------------------------------------------------------
public static class TensorLayout {
    private static readonly FrozenDictionary<(LayoutForm Origin, LayoutForm Target), int[]> PermuteRows =
        new Dictionary<(LayoutForm Origin, LayoutForm Target), int[]> {
            [(LayoutForm.Nchw, LayoutForm.Nhwc)] = [0, 2, 3, 1],
            [(LayoutForm.Nhwc, LayoutForm.Nchw)] = [0, 3, 1, 2],
        }.ToFrozenDictionary();

    public static Fin<Tensor<T>> Reform<T>(Tensor<T> source, LayoutForm origin, LayoutForm target) =>
        PermuteRows.TryGetValue((origin, target), out int[]? axes)
            ? Fin.Succ(Tensor.PermuteDimensions(source, axes!))
            : TensorFault.Fail<Tensor<T>>("no-permute-row", $"{origin.Key}->{target.Key}");

    public static Fin<Seq<Tensor<T>>> Apply<T>(Tensor<T> source, ReshapeOp op) where T : unmanaged =>
        op switch {
            ReshapeOp.Permute p => One(Tensor.PermuteDimensions(source, p.Axes)),
            ReshapeOp.Transpose => One(Tensor.Transpose(source)),
            ReshapeOp.Squeeze { Dimension.IsSome: true } s => One(Tensor.SqueezeDimension(source, s.Dimension.IfNone(0))),
            ReshapeOp.Squeeze => One(Tensor.Squeeze(source)),
            ReshapeOp.Unsqueeze u => One(Tensor.Unsqueeze(source, u.Dimension)),
            ReshapeOp.Reshape r => One(Tensor.Reshape(source, r.Lengths)),
            ReshapeOp.Flatten f => One(Flattened(source, f.Start, f.Count)),
            ReshapeOp.Densify => One(source.ToDenseTensor()),
            ReshapeOp.Broadcast b => One(Tensor.BroadcastTo(source, b.Lengths)),
            ReshapeOp.Concatenate c => One(Tensor.ConcatenateOnDimension(c.Dimension, [source, .. c.Others.Cast<Tensor<T>>()])),
            ReshapeOp.Stack k => One(Tensor.StackAlongDimension(k.Dimension, [source, .. k.Others.Cast<Tensor<T>>()])),
            ReshapeOp.Split l => Fin.Succ(toSeq(Tensor.Split(source, l.Count, l.Dimension))),
            ReshapeOp.Reverse { Dimension.IsSome: true } v => One(Tensor.ReverseDimension(source, v.Dimension.IfNone(0))),
            ReshapeOp.Reverse => One(Tensor.Reverse(source)),
            ReshapeOp.Resize z => One(Tensor.Resize(source, z.Lengths)),
            ReshapeOp.Slice l => One(source.Slice(l.Ranges)),
            _ => TensorFault.Fail<Seq<Tensor<T>>>("unhandled-reshape-op", op.GetType().Name),
        };

    private static Fin<Seq<Tensor<T>>> One<T>(Tensor<T> tensor) where T : unmanaged => Fin.Succ(Seq1(tensor));

    private static Tensor<T> Flattened<T>(Tensor<T> source, int start, int count) where T : unmanaged {
        using MemoryOwner<T> linear = MemoryOwner<T>.Allocate(checked((int)source.FlattenedLength), AllocationMode.Clear);
        source.AsReadOnlyTensorSpan().FlattenTo(linear.Span);
        nint folded = 1;
        for (int d = start; d < start + count; d++) { folded *= source.Lengths[d]; }
        nint[] lengths = [.. source.Lengths[..start], folded, .. source.Lengths[(start + count)..]];
        return Tensor.Create(linear.Span.ToArray(), lengths);
    }
}
```
