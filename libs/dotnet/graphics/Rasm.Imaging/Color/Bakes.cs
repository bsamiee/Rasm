using System.Numerics;
using System.Runtime.InteropServices;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;
using CommunityToolkit.HighPerformance.Helpers;
using TinyEXR;

namespace Rasm.Imaging.Color;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record LatticeInput {
    public abstract Gamut Gamut { get; }
    public abstract Option<(LutTable Head, LutTable Decode)> Shaper { get; }
    public virtual ColorEncoding Encoding => new(Gamut, new Transfer.Linear());

    public sealed record Encoded(ColorEncoding Signal) : LatticeInput {
        public override Gamut Gamut => Signal.Gamut;
        public override Option<(LutTable Head, LutTable Decode)> Shaper => None;
        public override ColorEncoding Encoding => Signal;
    }

    public sealed record Allocated(Gamut Space, float Low, float Span) : LatticeInput {
        public LogCurve Curve => LogCurve.Allocation(Low, Low + Span, float.Exp2(Low));
        public override Gamut Gamut => Space;
        public override Option<(LutTable Head, LutTable Decode)> Shaper => (new LutTable.Log(Curve), new LutTable.Antilog(Curve));
    }

    public sealed record Rational(Gamut Space) : LatticeInput {
        private static readonly LogCurve Lifted = new(2f, Vector3.One, Vector3.Zero, Vector3.One, Vector3.One);
        private static readonly LogCurve Folded = new(2f, -Vector3.One, Vector3.Zero, -Vector3.One, Vector3.One);
        private static readonly (LutTable Head, LutTable Decode) Spacing = (
            new LutTable.Sequence([new LutTable.Log(Lifted), new LutTable.Antilog(Folded)]),
            new LutTable.Sequence([
                new LutTable.Log(Folded),
                new LutTable.Antilog(Lifted),
                new LutTable.Remap(new RemapBounds.Ceiling(new RemapBound((float)Half.MaxValue, (float)Half.MaxValue))),
            ]));

        public override Gamut Gamut => Space;
        public override Option<(LutTable Head, LutTable Decode)> Shaper => Spacing;
    }
}

public sealed record LutBake(int Size, LatticeInput Input, ColorEncoding Output) {
    public static LutBake Default { get; } = new(
        Size: 33,
        new LatticeInput.Allocated(Gamut.StandardRgb, Low: -10f, Span: 20f),
        new ColorEncoding(Gamut.StandardRgb, new Transfer.Linear()));
}

file readonly struct LatticeRows(int edge, Seq<Action<Span<Vector4>>> steps, Memory<float> samples) : IAction2D {
    public void Invoke(int i, int j) {
        using SpanOwner<Vector4> nodes = SpanOwner<Vector4>.Allocate(edge);
        Span<Vector4> row = nodes.Span;
        for (int red = 0; red < edge; red++) row[red] = new Vector4(red, j, i, edge - 1) / (edge - 1);
        foreach (Action<Span<Vector4>> step in steps) step(row);
        MemoryMarshal.Cast<Vector4, float>(row).AsSpan2D(edge, 4).Slice(0, 0, edge, 3)
            .CopyTo(samples.Span.AsSpan2D(edge * edge, 3 * edge).GetRowSpan((edge * i) + j));
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Bakes {
    public static Fin<LutTable> Bake(LutBake bake, Seq<Action<Span<Vector4>>> chain, Gamut working) {
        (ColorEncoding linear, Option<(LutTable Head, LutTable Decode)> shaper) = (new(working, new Transfer.Linear()), bake.Input.Shaper);
        return Lattice(bake.Size, [
            .. shaper.ToSeq().Map<Action<Span<Vector4>>>(static held => held.Decode.Apply),
            .. bake.Input.Encoding.To(linear),
            .. chain,
            .. linear.To(bake.Output),
        ]).Map(static lattice => new LutTable.Cube(lattice, LutInterpolation.Tetrahedral))
            .Map(cube => shaper.Match(Some: held => new LutTable.Sequence([held.Head, cube]), None: () => (LutTable)cube));
    }

    public static Option<LutTable> Exact(LutBake bake, Seq<LutTable> chain, Gamut working) =>
        from linear in bake.Input.Encoding.Transfer is Transfer.Linear && bake.Output.Transfer is Transfer.Linear ? Some(new ColorEncoding(working, new Transfer.Linear())) : None
        select (LutTable)new LutTable.Sequence(bake.Input.Encoding.LinearTo(linear) + chain + linear.LinearTo(bake.Output));

    internal static Fin<Lut3D> Lattice(int size, Seq<Action<Span<Vector4>>> steps) {
        using MemoryOwner<float> samples = MemoryOwner<float>.Allocate(3 * size * size * size);
        ParallelHelper.For2D(..size, ..size, new LatticeRows(size, steps, samples.Memory));
        return Lut3D.From(size, samples.Memory, Vector3.Zero, Vector3.One);
    }
}
