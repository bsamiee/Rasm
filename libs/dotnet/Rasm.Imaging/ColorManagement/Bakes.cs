using System.Numerics;
using System.Runtime.InteropServices;
using CommunityToolkit.HighPerformance.Buffers;
using CommunityToolkit.HighPerformance.Helpers;
using Rasm.Imaging.ColorManagement.Calibration;
using Rasm.Imaging.Pixels;
using TinyEXR;

namespace Rasm.Imaging.ColorManagement;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidColor>]
public readonly partial struct AllocationStop : IMinMaxValue<AllocationStop> {
    public static AllocationStop MinValue { get; } = new(-24f);
    public static AllocationStop MaxValue { get; } = new(16f);
    public static AllocationStop Default { get; } = new(-10f);

    static partial void ValidateFactoryArguments(ref InvalidColor? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidColor();
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidColor>]
public readonly partial struct AllocationSpan : IMinMaxValue<AllocationSpan> {
    public static AllocationSpan MinValue { get; } = new(float.BitIncrement(0f));
    public static AllocationSpan MaxValue { get; } = new(40f);
    public static AllocationSpan Default { get; } = new(20f);

    static partial void ValidateFactoryArguments(ref InvalidColor? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidColor();
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record LatticeInput {
    public abstract Gamut Gamut { get; }
    public abstract Option<(LutTable Head, LutTable Decode)> Shaper { get; }
    public virtual ColorEncoding Encoding => new(Gamut, TransferCurve.Linear, Nits.ReferenceWhite);

    public sealed record Encoded(ColorEncoding Signal) : LatticeInput {
        public override Gamut Gamut => Signal.Gamut;
        public override Option<(LutTable Head, LutTable Decode)> Shaper => None;
        public override ColorEncoding Encoding => Signal;
    }

    public sealed record Allocated(Gamut Space, AllocationStop Low, AllocationSpan Span) : LatticeInput {
        public LogCurve Curve => LogCurve.Allocation(Low, (float)Low + (float)Span, float.Exp2(Low));
        public override Gamut Gamut => Space;
        public override Option<(LutTable Head, LutTable Decode)> Shaper => (new LutTable.Log(Curve), new LutTable.Antilog(Curve));
    }

    public sealed record Rational(Gamut Space) : LatticeInput {
        private static readonly LogCurve Lifted = new(2f, Vector3.One, Vector3.Zero, Vector3.One, Vector3.One, None);
        private static readonly LogCurve Folded = new(2f, -Vector3.One, Vector3.Zero, -Vector3.One, Vector3.One, None);
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

public sealed record LutBake(LatticeSize Size, LatticeInput Input, ColorEncoding Output) {
    public static LutBake Default { get; } = new(
        LatticeSize.Default,
        new LatticeInput.Allocated(Gamut.StandardRgb, AllocationStop.Default, AllocationSpan.Default),
        new ColorEncoding(Gamut.StandardRgb, TransferCurve.Linear, Nits.ReferenceWhite));
}

file readonly struct LatticeRows(int edge, Seq<Action<Span<Vector4>>> steps, Memory<float> samples) : IAction {
    public void Invoke(int i) {
        using SpanOwner<Vector4> nodes = SpanOwner<Vector4>.Allocate(edge);
        Span<Vector4> row = nodes.Span;
        for (int red = 0; red < edge; red++) row[red] = new Vector4(red, i % edge, i / edge, edge - 1) / (edge - 1);
        foreach (Action<Span<Vector4>> step in steps) step(row);
        Span<Vector3> written = MemoryMarshal.Cast<float, Vector3>(samples.Span).Slice(edge * i, edge);
        for (int red = 0; red < edge; red++) written[red] = row[red].AsVector3();
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Bakes {
    public static Fin<LutTable> Bake(LutBake bake, Seq<PixelPass.Color> chain, Gamut working) =>
        new ColorEncoding(working, TransferCurve.Linear, Nits.ReferenceWhite) switch {
            var linear => Lattice(bake.Size, [
                .. bake.Input.Shaper.ToSeq().Map<Action<Span<Vector4>>>(static shaper => shaper.Decode.Apply),
                .. bake.Input.Encoding.To(linear),
                .. chain.Map(static pass => pass.Row),
                .. linear.To(bake.Output),
            ]).Map(lattice => new LutTable.Cube(lattice, LutInterpolation.Tetrahedral) switch {
                var cube => bake.Input.Shaper.Match(Some: shaper => (LutTable)new LutTable.Sequence([shaper.Head, cube]), None: () => cube),
            }),
        };

    public static Option<LutTable> Exact(LutBake bake, Seq<PixelPass.Color> chain, Gamut working) =>
        from steps in chain.Traverse(static pass => pass.Table).As()
        where bake.Input.Encoding.Transfer == TransferCurve.Linear && bake.Output.Transfer == TransferCurve.Linear
        select (LutTable)new LutTable.Sequence(LutTables.Between(bake.Input.Gamut, working) + steps + LutTables.Between(working, bake.Output.Gamut));

    public static LutTable Export(ChartFit fit, FitDirection direction) =>
        fit.Transform(direction).Matrix switch {
            var matrix => new LutTable.Affine(
                new ColorMatrix3x3(
                    (float)matrix[0, 0], (float)matrix[0, 1], (float)matrix[0, 2],
                    (float)matrix[1, 0], (float)matrix[1, 1], (float)matrix[1, 2],
                    (float)matrix[2, 0], (float)matrix[2, 1], (float)matrix[2, 2]),
                Vector3.Zero),
        };

    internal static Fin<Lut3D> Lattice(LatticeSize size, Seq<Action<Span<Vector4>>> steps) {
        using MemoryOwner<float> samples = MemoryOwner<float>.Allocate(3 * size * size * size);
        ParallelHelper.For(0, size * size, new LatticeRows(size, steps, samples.Memory));
        return Lut3D.From(size, samples.Memory, Vector3.Zero, Vector3.One);
    }
}
