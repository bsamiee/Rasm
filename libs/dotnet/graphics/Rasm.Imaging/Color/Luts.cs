using System.Globalization;
using System.Numerics;
using System.Numerics.Tensors;
using System.Runtime.InteropServices;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using TinyEXR;
using Wacton.Unicolour;

namespace Rasm.Imaging.Color;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<int>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct LatticeSize : IMinMaxValue<LatticeSize> {
    public static LatticeSize MinValue { get; } = new(2);
    public static LatticeSize MaxValue { get; } = new(256);
    public static LatticeSize Default { get; } = new(33);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<int>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct CurveLength : IMinMaxValue<CurveLength> {
    public static CurveLength MinValue { get; } = new(2);
    public static CurveLength MaxValue { get; } = new(300_000);
    public static CurveLength ResolveShaper { get; } = new(65_536);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

public sealed class Lut1D {
    private readonly ImmutableArray<float> planes;

    private Lut1D(CurveLength length, ReadOnlySpan<float> rows, Vector3 domainMinimum, Vector3 domainMaximum) {
        float[] planar = GC.AllocateUninitializedArray<float>(rows.Length);
        ReadOnlySpan<Vector3> entries = MemoryMarshal.Cast<float, Vector3>(rows);
        for (int entry = 0; entry < entries.Length; entry++)
            (planar[entry], planar[length + entry], planar[(2 * length) + entry]) = (entries[entry].X, entries[entry].Y, entries[entry].Z);
        (Length, DomainMinimum, DomainMaximum, planes) = (length, domainMinimum, domainMaximum, ImmutableCollectionsMarshal.AsImmutableArray(planar));
    }

    public CurveLength Length { get; }
    public Vector3 DomainMinimum { get; }
    public Vector3 DomainMaximum { get; }

    public static Fin<Lut1D> From(CurveLength length, ReadOnlyMemory<float> rows, Vector3 domainMinimum, Vector3 domainMaximum) =>
        LutTables.Accepts(length, 3 * length, rows.Span, domainMinimum, domainMaximum).Map(_ => new Lut1D(length, rows.Span, domainMinimum, domainMaximum));

    public ReadOnlySpan<float> Channel(int channel) => planes.AsSpan(channel * Length, Length);

    public void Apply(Span<Vector4> row) {
        (Vector3 span, Vector3 top) = (DomainMaximum - DomainMinimum, new Vector3(Length - 1));
        foreach (ref Vector4 pixel in row) {
            Vector3 position = Vector3.Min(Vector3.MaxNumber((pixel.AsVector3() - DomainMinimum) / span * top, Vector3.Zero), top);
            pixel = new Vector4(Sample(Channel(0), position.X), Sample(Channel(1), position.Y), Sample(Channel(2), position.Z), pixel.W);
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
        toSeq(Range(0, 3)).Traverse(channel => Monotonic(forward.Channel(channel)).ToFin(Error.New(string.Create(CultureInfo.InvariantCulture, $"non-monotonic LUT channel {channel} has no exact inverse")))).As()
            .Map(ranges => new InverseLut1D(forward, ranges));

    public void Apply(Span<Vector4> row) {
        foreach (ref Vector4 pixel in row)
            pixel = new Vector4(Invert(0, pixel.X), Invert(1, pixel.Y), Invert(2, pixel.Z), pixel.W);
    }

    private float Invert(int channel, float value) {
        MonotonicRange range = ranges[channel];
        ReadOnlySpan<float> table = Forward.Channel(channel)[range.Start..(range.End + 1)];
        float bounded = float.IsNaN(value) ? table[0] : float.Clamp(value, float.Min(table[0], table[^1]), float.Max(table[0], table[^1]));
        int above = ~table.BinarySearch(new LowerBound(bounded, range.Sign));
        float position = range.Start + (above is 0 ? 0f : above - 1 + ((bounded - table[above - 1]) / (table[above] - table[above - 1])));
        return Forward.DomainMinimum[channel] + (position / (Forward.Length - 1) * (Forward.DomainMaximum[channel] - Forward.DomainMinimum[channel]));
    }

    private static Option<MonotonicRange> Monotonic(ReadOnlySpan<float> table) {
        float sign = float.Sign(table[^1] - table[0]);
        for (int entry = 1; entry < table.Length && sign is not 0f; entry++)
            sign = sign * (table[entry] - table[entry - 1]) < 0f ? 0f : sign;
        return sign is 0f ? None : new MonotonicRange(sign, table.IndexOfAnyExcept(table[0]) - 1, table.LastIndexOfAnyExcept(table[^1]) + 1);
    }

    private readonly record struct MonotonicRange(float Sign, int Start, int End);

    private readonly struct LowerBound(float value, float sign) : IComparable<float> {
        public int CompareTo(float other) => sign * value <= sign * other ? -1 : 1;
    }
}

public readonly record struct RemapBound(float In, float Out);

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

public readonly record struct CameraSegment(Vector3 Break, Vector3 Slope);

public readonly record struct LogCurve(float Base, Vector3 LogSlope, Vector3 LogOffset, Vector3 LinSlope, Vector3 LinOffset, Option<CameraSegment> Camera) {
    private static readonly Vector3 Floor = new(float.ScaleB(1f, -126));

    public static LogCurve Allocation(float minimum, float maximum, float offset) =>
        new(2f, new(1f / (maximum - minimum)), new(-minimum / (maximum - minimum)), Vector3.One, new(offset), None);

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
        public override Fin<LutTable> Inverse() => Error.New("3D lattice step has no exact inverse");
    }

    public sealed record ChannelCurve(Lut1D Table) : LutTable {
        public override void Apply(Span<Vector4> row) => Table.Apply(row);
        public override Fin<LutTable> Inverse() => InverseLut1D.From(Table).Map(static inverse => (LutTable)new InverseChannelCurve(inverse));
    }

    public sealed record InverseChannelCurve(InverseLut1D Table) : LutTable {
        public override void Apply(Span<Vector4> row) => Table.Apply(row);
        public override Fin<LutTable> Inverse() => new ChannelCurve(Table.Forward);
    }

    public sealed record Affine(Matrix<double> Matrix, (double X, double Y, double Z) Offset = default) : LutTable {
        public override void Apply(Span<Vector4> row) {
            DenseVector source = new(3);
            DenseVector mapped = new(3);
            foreach (ref Vector4 pixel in row) {
                (source[0], source[1], source[2]) = (pixel.X, pixel.Y, pixel.Z);
                Matrix.Multiply(source, mapped);
                pixel = new Vector4((float)(mapped[0] + Offset.X), (float)(mapped[1] + Offset.Y), (float)(mapped[2] + Offset.Z), pixel.W);
            }
        }
        public override Fin<LutTable> Inverse() =>
            Matrix.LU() switch {
                var factorization when factorization.U.Diagonal().Contains(0d) => Error.New("singular affine step has no exact inverse"),
                var factorization => factorization.Inverse() switch {
                    var inverse => inverse.Multiply(new DenseVector([Offset.X, Offset.Y, Offset.Z])) switch {
                        var offset => new Affine(inverse, (-offset[0], -offset[1], -offset[2])),
                    },
                },
            };
    }

    public sealed record Remap(RemapBounds Bounds) : LutTable {
        public override void Apply(Span<Vector4> row) {
            foreach (ref Vector4 pixel in row) pixel = new Vector4(Bounds.Apply(pixel.AsVector3()), pixel.W);
        }
        public override Fin<LutTable> Inverse() => Bounds.Inverse().Map(static bounds => (LutTable)new Remap(bounds)).ToFin(Error.New("constant remap step has no exact inverse"));
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
        public static Fin<Lut3D> From(LatticeSize size, ReadOnlyMemory<float> samples, Vector3 domainMinimum, Vector3 domainMaximum) =>
            Accepts(size, 3 * size * size * size, samples.Span, domainMinimum, domainMaximum).Map(_ => new Lut3D(size, samples.Span, domainMinimum, domainMaximum));
    }

    internal static Fin<Unit> Accepts(int size, int count, ReadOnlySpan<float> samples, Vector3 domainMinimum, Vector3 domainMaximum) =>
        (samples.Length == count
                ? Fin.Succ(unit)
                : Error.New(string.Create(CultureInfo.InvariantCulture, $"LUT of size {size} needs {count} samples, not {samples.Length}")),
         TensorPrimitives.IsFiniteAll(samples) ? Fin.Succ(unit) : Error.New("LUT holds non-finite sample"),
         Vector3.AllWhereAllBitsSet(Vector3.LessThan(domainMinimum, domainMaximum) & Vector3.IsFinite(domainMinimum) & Vector3.IsFinite(domainMaximum))
                ? Fin.Succ(unit)
                : Error.New(string.Create(CultureInfo.InvariantCulture, $"LUT domain [{domainMinimum}, {domainMaximum}] must be finite and ascending")))
            .Apply(static (_, _, _) => unit).As();

    public static Seq<LutTable> Between(Gamut source, Gamut target) =>
        source == target ? [] : [new LutTable.Affine(source.MatrixTo(target))];

    public static LutTable.Sequence Bracketed(Gamut working, Gamut space, Seq<LutTable> into, LutTable step, Seq<LutTable> back) =>
        new([.. Between(working, space), .. into, step, .. back, .. Between(space, working)]);
}
