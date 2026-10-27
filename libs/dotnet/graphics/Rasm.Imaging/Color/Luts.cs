using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using System.Numerics.Tensors;
using System.Runtime.InteropServices;
using CommunityToolkit.HighPerformance;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using TinyEXR;
using Wacton.Unicolour;

namespace Rasm.Imaging.Color;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<int>(SkipFactoryMethods = true, ConversionToKeyMemberType = ConversionOperatorsGeneration.None, UnsafeConversionToKeyMemberType = ConversionOperatorsGeneration.Implicit, SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public sealed partial class CurveLength {
    public static CurveLength UInt16Codes { get; } = new(ushort.MaxValue + 1);

    public static Fin<CurveLength> Validated(int value) => ImagingFault.Unless(value is >= 2 and <= 300_000, ImagingReason.Range, value).Map(_ => new CurveLength(value));
}

public sealed class Lut1D {
    private readonly ImmutableArray<float> planes;

    private Lut1D(CurveLength length, ReadOnlySpan<float> rows, Vector3 domainMinimum, Vector3 domainMaximum) {
        float[] planar = GC.AllocateUninitializedArray<float>(rows.Length);
        ReadOnlySpan2D<float> entries = rows.AsSpan2D(length, 3);
        Span2D<float> channels = planar.AsSpan().AsSpan2D(3, length);
        for (int channel = 0; channel < 3; channel++)
            entries.GetColumn(channel).CopyTo(channels.GetRowSpan(channel));
        (Length, DomainMinimum, DomainMaximum, planes) = (length, domainMinimum, domainMaximum, ImmutableCollectionsMarshal.AsImmutableArray(planar));
    }

    public CurveLength Length { get; }
    public Vector3 DomainMinimum { get; }
    public Vector3 DomainMaximum { get; }

    public static Fin<Lut1D> From(CurveLength length, ReadOnlyMemory<float> rows, Vector3 domainMinimum, Vector3 domainMaximum) =>
        LutTables.Accepts(length, rows.Span, domainMinimum, domainMaximum).Map(_ => new Lut1D(length, rows.Span, domainMinimum, domainMaximum));

    public ReadOnlySpan<float> Channel(int channel) => planes.AsSpan().AsSpan2D(3, Length).GetRowSpan(channel);

    public void Apply(Span<Vector4> row) {
        (Vector3 span, Vector3 top) = (DomainMaximum - DomainMinimum, new Vector3(Length - 1));
        ReadOnlySpan<float> red = Channel(0);
        ReadOnlySpan<float> green = Channel(1);
        ReadOnlySpan<float> blue = Channel(2);
        foreach (ref Vector4 pixel in row) {
            Vector3 position = Vector3.Min(Vector3.MaxNumber((pixel.AsVector3() - DomainMinimum) / span * top, Vector3.Zero), top);
            pixel = new Vector4(Sample(red, position.X), Sample(green, position.Y), Sample(blue, position.Z), pixel.W);
        }
    }

    private static float Sample(ReadOnlySpan<float> table, float position) {
        int high = (int)float.Ceiling(position);
        return table[high] + ((table[(int)position] - table[high]) * (high - position));
    }
}

public sealed class InverseLut1D {
    private readonly Seq<MonotonicRange> ranges;

    private InverseLut1D(Lut1D forward, Seq<MonotonicRange> ranges) => (Forward, this.ranges) = (forward, ranges);

    public Lut1D Forward { get; }

    public static Fin<InverseLut1D> From(Lut1D forward) =>
        toSeq(Range(0, 3)).Traverse(channel => Monotonic(forward.Channel(channel)).ToFin(new ImagingFault("non-monotonic LUT channel", ImagingReason.NoInverse, channel))).As()
            .Map(ranges => new InverseLut1D(forward, ranges));

    public void Apply(Span<Vector4> row) {
        for (int channel = 0; channel < 3; channel++) {
            ReadOnlySpan<float> table = Forward.Channel(channel)[ranges[channel].Start..(ranges[channel].End + 1)];
            foreach (ref Vector4 pixel in row) pixel[channel] = Invert(channel, table, pixel[channel]);
        }
    }

    private float Invert(int channel, ReadOnlySpan<float> table, float value) {
        MonotonicRange range = ranges[channel];
        float bounded = float.IsNaN(value) ? table[0] : float.Clamp(value, float.Min(table[0], table[^1]), float.Max(table[0], table[^1]));
        int above = ~table.BinarySearch(bounded, range.Order);
        float position = range.Start + (above is 0 ? 0f : above - 1 + ((bounded - table[above - 1]) / (table[above] - table[above - 1])));
        return Forward.DomainMinimum[channel] + (position / (Forward.Length - 1) * (Forward.DomainMaximum[channel] - Forward.DomainMinimum[channel]));
    }

    private static Option<MonotonicRange> Monotonic(ReadOnlySpan<float> table) {
        float sign = float.Sign(table[^1] - table[0]);
        for (int entry = 1; entry < table.Length && sign is not 0f; entry++)
            sign = sign * (table[entry] - table[entry - 1]) < 0f ? 0f : sign;
        return sign is 0f ? None : new MonotonicRange(Comparer<float>.Create((value, entry) => sign * value <= sign * entry ? -1 : 1), table.IndexOfAnyExcept(table[0]) - 1, table.LastIndexOfAnyExcept(table[^1]) + 1);
    }

    private sealed record MonotonicRange(Comparer<float> Order, int Start, int End);
}

public sealed record RemapBound(float In, float Out);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record RemapBounds {
    public abstract Vector3 Apply(Vector3 value);
    public abstract Option<RemapBounds> Inverse();

    public sealed record Between(RemapBound Minimum, RemapBound Maximum, bool Clamps) : RemapBounds {
        public override Vector3 Apply(Vector3 value) =>
            (((value - new Vector3(Minimum.In)) * (Maximum.Out - Minimum.Out) / (Maximum.In - Minimum.In)) + new Vector3(Minimum.Out)) switch {
                var mapped when Clamps => Vector3.Clamp(mapped, new(float.Min(Minimum.Out, Maximum.Out)), new(float.Max(Minimum.Out, Maximum.Out))),
                var mapped => mapped,
            };
        public override Option<RemapBounds> Inverse() =>
            Minimum.Out == Maximum.Out ? None : new Between(new RemapBound(Minimum.Out, Minimum.In), new RemapBound(Maximum.Out, Maximum.In), Clamps);
    }

    public sealed record Floor(RemapBound Minimum) : RemapBounds {
        public override Vector3 Apply(Vector3 value) => Vector3.Max(value, new(Minimum.In)) + new Vector3(Minimum.Out - Minimum.In);
        public override Option<RemapBounds> Inverse() => new Floor(new RemapBound(Minimum.Out, Minimum.In));
    }

    public sealed record Ceiling(RemapBound Maximum) : RemapBounds {
        public override Vector3 Apply(Vector3 value) => Vector3.Min(value, new(Maximum.In)) + new Vector3(Maximum.Out - Maximum.In);
        public override Option<RemapBounds> Inverse() => new Ceiling(new RemapBound(Maximum.Out, Maximum.In));
    }
}

public sealed record CameraSegment(Vector3 Break, Vector3 Slope);

public sealed record LogCurve(float Base, Vector3 LogSlope, Vector3 LogOffset, Vector3 LinSlope, Vector3 LinOffset) {
    private static readonly Vector3 Floor = new(float.ScaleB(1f, -126));

    public Option<CameraSegment> Camera { get; init; }

    public static LogCurve Allocation(float minimum, float maximum, float offset) =>
        new(2f, new(1f / (maximum - minimum)), new(-minimum / (maximum - minimum)), Vector3.One, new(offset));

    public Vector3 Forward(Vector3 linear) =>
        Logarithmic(linear) switch {
            var log => Camera.ToSpan() is [var camera]
                ? Vector3.ConditionalSelect(Vector3.LessThanOrEqual(linear, camera.Break), (camera.Slope * (linear - camera.Break)) + Logarithmic(camera.Break), log)
                : log,
        };

    public Vector3 Inverse(Vector3 encoded) =>
        Exponential(encoded) switch {
            var exponential => Camera.ToSpan() is [var camera]
                ? Logarithmic(camera.Break) switch {
                    var joint => Vector3.ConditionalSelect(Vector3.LessThanOrEqual(encoded, joint), ((encoded - joint) / camera.Slope) + camera.Break, exponential),
                }
                : exponential,
        };

    private Vector3 Logarithmic(Vector3 linear) => (LogSlope * (Vector3.Log(Vector3.Max((LinSlope * linear) + LinOffset, Floor)) / float.Log(Base))) + LogOffset;

    private Vector3 Exponential(Vector3 encoded) => (Vector3.Exp((encoded - LogOffset) / LogSlope * float.Log(Base)) - LinOffset) / LinSlope;
}

[SmartEnum<string>]
public sealed partial class ExponentStyle {
    public static readonly ExponentStyle BasicFwd = new("basicFwd", None, static (x, power, _) => float.Pow(float.Max(x, 0f), power));
    public static readonly ExponentStyle BasicRev = new("basicRev", None, static (x, power, _) => float.Pow(float.Max(x, 0f), 1f / power));
    public static readonly ExponentStyle BasicMirrorFwd = new("basicMirrorFwd", None, static (x, power, _) => float.CopySign(float.Pow(float.Abs(x), power), x));
    public static readonly ExponentStyle BasicMirrorRev = new("basicMirrorRev", None, static (x, power, _) => float.CopySign(float.Pow(float.Abs(x), 1f / power), x));
    public static readonly ExponentStyle BasicPassThruFwd = new("basicPassThruFwd", None, static (x, power, _) => x >= 0f ? float.Pow(x, power) : x);
    public static readonly ExponentStyle BasicPassThruRev = new("basicPassThruRev", None, static (x, power, _) => x >= 0f ? float.Pow(x, 1f / power) : x);
    public static readonly ExponentStyle MonCurveFwd = new("monCurveFwd", BasicFwd, MonCurveForward);
    public static readonly ExponentStyle MonCurveRev = new("monCurveRev", BasicRev, MonCurveReverse);
    public static readonly ExponentStyle MonCurveMirrorFwd = new("monCurveMirrorFwd", BasicMirrorFwd, static (x, power, offset) => float.CopySign(MonCurveForward(float.Abs(x), power, offset), x));
    public static readonly ExponentStyle MonCurveMirrorRev = new("monCurveMirrorRev", BasicMirrorRev, static (y, power, offset) => float.CopySign(MonCurveReverse(float.Abs(y), power, offset), y));

    public Option<ExponentStyle> Basic { get; }

    public ExponentStyle Inverse => Map(
        basicFwd: BasicRev, basicRev: BasicFwd, basicMirrorFwd: BasicMirrorRev, basicMirrorRev: BasicMirrorFwd,
        basicPassThruFwd: BasicPassThruRev, basicPassThruRev: BasicPassThruFwd, monCurveFwd: MonCurveRev, monCurveRev: MonCurveFwd,
        monCurveMirrorFwd: MonCurveMirrorRev, monCurveMirrorRev: MonCurveMirrorFwd);

    [UseDelegateFromConstructor]
    public partial float Apply(float x, float power, float offset);

    private static float MonCurveForward(float x, float power, float offset) =>
        Joint(power, offset) switch {
            var (knee, slope) => x >= knee ? float.Pow((x + offset) / (1f + offset), power) : x * slope,
        };

    private static float MonCurveReverse(float y, float power, float offset) =>
        Joint(power, offset) switch {
            var (knee, slope) => y >= knee * slope ? ((1f + offset) * float.Pow(y, 1f / power)) - offset : y / slope,
        };

    private static (float Knee, float Slope) Joint(float power, float offset) =>
        (offset / (power - 1f), (power - 1f) / offset * float.Pow(offset * power / ((power - 1f) * (1f + offset)), power));
}

[SmartEnum<string>]
public sealed partial class CdlStyle {
    public static readonly CdlStyle Fwd = new("Fwd", "v1.2_Fwd", static (color, slope, offset, power, saturation) =>
        Clamped(Saturated(Raised(Clamped((color * slope) + offset), power), new Vector3(saturation))));
    public static readonly CdlStyle FwdNoClamp = new("FwdNoClamp", "noClampFwd", static (color, slope, offset, power, saturation) =>
        Saturated(Raised((color * slope) + offset, power), new Vector3(saturation)));
    public static readonly CdlStyle Rev = new("Rev", "v1.2_Rev", static (color, slope, offset, power, saturation) =>
        Clamped((Raised(Clamped(Saturated(Clamped(color), Reciprocal(new Vector3(saturation)))), Reciprocal(power)) - offset) * Reciprocal(slope)));
    public static readonly CdlStyle RevNoClamp = new("RevNoClamp", "noClampRev", static (color, slope, offset, power, saturation) =>
        (Raised(Saturated(color, Reciprocal(new Vector3(saturation))), Reciprocal(power)) - offset) * Reciprocal(slope));

    private static readonly Vector3 Luma = new((float)YbrConfiguration.Rec709.Kr, (float)YbrConfiguration.Rec709.Kg, (float)YbrConfiguration.Rec709.Kb);

    public string Legacy { get; }

    public CdlStyle Inverse => Map(fwd: Rev, fwdNoClamp: RevNoClamp, rev: Fwd, revNoClamp: FwdNoClamp);

    [UseDelegateFromConstructor]
    public partial Vector3 Apply(Vector3 color, Vector3 slope, Vector3 offset, Vector3 power, float saturation);

    private static Vector3 Clamped(Vector3 color) => Vector3.Min(Vector3.MaxNumber(color, Vector3.Zero), Vector3.One);

    private static Vector3 Saturated(Vector3 color, Vector3 saturation) =>
        new Vector3(Vector3.Dot(color, Luma)) switch {
            var luma => luma + (saturation * (color - luma)),
        };

    private static Vector3 Raised(Vector3 color, Vector3 power) => new(Raised(color.X, power.X), Raised(color.Y, power.Y), Raised(color.Z, power.Z));

    private static float Raised(float value, float power) => float.IsNaN(value) ? 0f : value < 0f ? value : float.Pow(value, power);

    private static Vector3 Reciprocal(Vector3 value) => Vector3.One / Vector3.Max(value, new Vector3(0.01f));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record LutTable {
    public abstract void Apply(Span<Vector4> row);
    public abstract Fin<LutTable> Inverse();

    public sealed record Cube(Lut3D Lattice, LutInterpolation Interpolation) : LutTable {
        public override void Apply(Span<Vector4> row) {
            Span<float> components = MemoryMarshal.Cast<Vector4, float>(row);
            Lattice.Apply(components, components, 4, Interpolation);
        }
        public override Fin<LutTable> Inverse() => new ImagingFault("3D lattice step", ImagingReason.NoInverse);
    }

    public sealed record ChannelCurve(Lut1D Table) : LutTable {
        public override void Apply(Span<Vector4> row) => Table.Apply(row);
        public override Fin<LutTable> Inverse() => InverseLut1D.From(Table).Map(static inverse => (LutTable)new InverseChannelCurve(inverse));
    }

    public sealed record InverseChannelCurve(InverseLut1D Table) : LutTable {
        public override void Apply(Span<Vector4> row) => Table.Apply(row);
        public override Fin<LutTable> Inverse() => new ChannelCurve(Table.Forward);
    }

    public sealed record Affine : LutTable {
        private readonly ColorMatrix3x3 baked;
        private readonly Vector4 shift;

        public Affine(Matrix<double> matrix, (double X, double Y, double Z) offset = default) =>
            (Matrix, Offset, baked, shift) = (matrix, offset, LutTables.Baked(matrix), new Vector4((float)offset.X, (float)offset.Y, (float)offset.Z, 0f));

        public Matrix<double> Matrix { get; }
        public (double X, double Y, double Z) Offset { get; }

        public override void Apply(Span<Vector4> row) {
            Span<float> samples = MemoryMarshal.Cast<Vector4, float>(row);
            ImageProcessing.ApplyColorMatrix(samples, samples, 4, baked);
            if (shift != Vector4.Zero)
                foreach (ref Vector4 pixel in row) pixel += shift;
        }
        public override Fin<LutTable> Inverse() =>
            Matrix.LU() switch {
                var factorization when factorization.U.Diagonal().Contains(0d) => new ImagingFault("singular affine step", ImagingReason.NoInverse),
                var factorization => factorization.Inverse() switch {
                    var inverse => inverse.Multiply(new DenseVector([Offset.X, Offset.Y, Offset.Z])).Negate() switch { var offset => new Affine(inverse, (offset[0], offset[1], offset[2])) },
                },
            };
    }

    public sealed record Remap(RemapBounds Bounds) : LutTable {
        public override void Apply(Span<Vector4> row) {
            foreach (ref Vector4 pixel in row) pixel = new Vector4(Bounds.Apply(pixel.AsVector3()), pixel.W);
        }
        public override Fin<LutTable> Inverse() => Bounds.Inverse().Map(static bounds => (LutTable)new Remap(bounds)).ToFin(new ImagingFault("constant remap step", ImagingReason.NoInverse));
    }

    public sealed record Log(LogCurve Curve) : LutTable {
        public override void Apply(Span<Vector4> row) {
            foreach (ref Vector4 pixel in row) pixel = new Vector4(Curve.Forward(pixel.AsVector3()), pixel.W);
        }
        public override Fin<LutTable> Inverse() => new Antilog(Curve);
    }

    public sealed record Antilog(LogCurve Curve) : LutTable {
        public override void Apply(Span<Vector4> row) {
            foreach (ref Vector4 pixel in row) pixel = new Vector4(Curve.Inverse(pixel.AsVector3()), pixel.W);
        }
        public override Fin<LutTable> Inverse() => new Log(Curve);
    }

    public sealed record Exponent(ExponentStyle Style, Vector3 Power, Vector3 Offset) : LutTable {
        public override void Apply(Span<Vector4> row) {
            foreach (ref Vector4 pixel in row)
                pixel = new Vector4(Style.Apply(pixel.X, Power.X, Offset.X), Style.Apply(pixel.Y, Power.Y, Offset.Y), Style.Apply(pixel.Z, Power.Z, Offset.Z), pixel.W);
        }
        public override Fin<LutTable> Inverse() => new Exponent(Style.Inverse, Power, Offset);
    }

    public sealed record Cdl(CdlStyle Style, Vector3 Slope, Vector3 Offset, Vector3 Power, float Saturation) : LutTable {
        public override void Apply(Span<Vector4> row) {
            foreach (ref Vector4 pixel in row) pixel = new Vector4(Style.Apply(pixel.AsVector3(), Slope, Offset, Power, Saturation), pixel.W);
        }
        public override Fin<LutTable> Inverse() => new Cdl(Style.Inverse, Slope, Offset, Power, Saturation);
    }

    public sealed record Sequence(Seq<LutTable> Steps) : LutTable {
        public override void Apply(Span<Vector4> row) {
            foreach (LutTable step in Steps) step.Apply(row);
        }
        public override Fin<LutTable> Inverse() =>
            Steps.Rev().Traverse(static step => step.Inverse()).As().Map(static steps => (LutTable)new Sequence(steps));
    }
}

public sealed record LogSpace(Gamut Gamut, LogCurve Allocation, Seq<LutTable> Into, Seq<LutTable> Back) {
    private static readonly LogCurve AgXAllocation = LogCurve.Allocation(-12.47393f, 12.5260688117f, 0f);
    private static readonly LutTable Inset = new LutTable.Affine(Matrix<double>.Build.DenseOfRowMajor(3, 3, [
        0.856627153315983d, 0.0951212405381588d, 0.0482516061458583d,
        0.137318972929847d, 0.761241990602591d, 0.101439036467562d,
        0.11189821299995d, 0.0767994186031903d, 0.811302368396859d,
    ]));
    private static readonly LutTable Outset = Inset.Inverse().ThrowIfFail();

    public static LogSpace Filmic { get; } = LogCurve.Allocation(-12.473931188f, 12.526068812f, 0f) switch {
        var allocation => new(Gamut.StandardRgb, allocation, [new LutTable.Log(allocation)], [new LutTable.Antilog(allocation)]),
    };

    public static LogSpace AgX(Lut3D compensation) =>
        new(Gamut.EGamut, AgXAllocation,
            [
                new LutTable.Log(AgXAllocation), new LutTable.Cube(compensation, LutInterpolation.Tetrahedral), new LutTable.Antilog(AgXAllocation),
                .. LutTables.Between(Gamut.EGamut, Gamut.Rec2020), Inset, new LutTable.Log(AgXAllocation),
            ],
            [new LutTable.Antilog(AgXAllocation), Outset, .. LutTables.Between(Gamut.Rec2020, Gamut.EGamut)]);
}

[Union]
public abstract partial record LutSpace {
    public abstract (ColorEncoding Encoding, Seq<LutTable> Into, Seq<LutTable> Back) Bracket { get; }

    public sealed record Encoded(ColorEncoding Signal) : LutSpace {
        public override (ColorEncoding Encoding, Seq<LutTable> Into, Seq<LutTable> Back) Bracket => (Signal, [], []);
    }

    public sealed record Log(LogSpace Space) : LutSpace {
        public override (ColorEncoding Encoding, Seq<LutTable> Into, Seq<LutTable> Back) Bracket => (new(Space.Gamut, new Transfer.Linear()), Space.Into, Space.Back);
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class LutTables {
    extension(Lut3D) {
        public static Fin<Lut3D> From(int size, ReadOnlyMemory<float> samples, Vector3 domainMinimum, Vector3 domainMaximum) =>
            Sampled(samples.Span, domainMinimum, domainMaximum).Bind(_ => Try.lift(() => new Lut3D(size, samples.Span, domainMinimum, domainMaximum)).Run()
                .MapFail(cause => new ImagingFault(string.Create(CultureInfo.InvariantCulture, $"LUT lattice of size {size}"), ImagingReason.Refused, Inner: cause)));
    }

    internal static ColorMatrix3x3 Baked(Matrix<double> matrix) =>
        new((float)matrix[0, 0], (float)matrix[0, 1], (float)matrix[0, 2], (float)matrix[1, 0], (float)matrix[1, 1], (float)matrix[1, 2], (float)matrix[2, 0], (float)matrix[2, 1], (float)matrix[2, 2]);

    internal static Fin<Unit> Accepts(CurveLength length, ReadOnlySpan<float> rows, Vector3 domainMinimum, Vector3 domainMaximum) =>
        (rows.Length == 3 * length
                ? Fin.Succ(unit)
                : new ImagingFault(string.Create(CultureInfo.InvariantCulture, $"LUT curve of length {length} with {rows.Length} samples"), ImagingReason.Miscounted),
         Sampled(rows, domainMinimum, domainMaximum),
         Vector3.AllWhereAllBitsSet(Vector3.IsFinite(domainMinimum) & Vector3.IsFinite(domainMaximum))
                ? Fin.Succ(unit)
                : new ImagingFault(string.Create(CultureInfo.InvariantCulture, $"LUT domain [{domainMinimum}, {domainMaximum}]"), ImagingReason.Range))
            .Apply(static (_, _, _) => unit).As();

    private static Fin<Unit> Sampled(ReadOnlySpan<float> samples, Vector3 domainMinimum, Vector3 domainMaximum) =>
        (TensorPrimitives.IsFiniteAll(samples) ? Fin.Succ(unit) : new ImagingFault("LUT sample", ImagingReason.Range),
         Vector3.AllWhereAllBitsSet(Vector3.LessThan(domainMinimum, domainMaximum))
                ? Fin.Succ(unit)
                : new ImagingFault(string.Create(CultureInfo.InvariantCulture, $"LUT domain order [{domainMinimum}, {domainMaximum}]"), ImagingReason.Degenerate))
            .Apply(static (_, _) => unit).As();

    public static Seq<LutTable> Between(Gamut source, Gamut target) =>
        source == target ? [] : [new LutTable.Affine(source.MatrixTo(target))];

    public static LutTable.Sequence Bracketed(Gamut working, Gamut space, Seq<LutTable> into, LutTable step, Seq<LutTable> back) =>
        new([.. Between(working, space), .. into, step, .. back, .. Between(space, working)]);
}
