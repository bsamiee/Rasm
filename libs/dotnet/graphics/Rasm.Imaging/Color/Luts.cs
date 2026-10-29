using System.Collections.Generic;
using System.Numerics;
using System.Numerics.Tensors;
using System.Runtime.InteropServices;
using System.Text;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;
using CommunityToolkit.HighPerformance.Helpers;
using TinyEXR;
using Wacton.Unicolour;

namespace Rasm.Imaging.Color;

// --- [TYPES] ---------------------------------------------------------------------------
public interface IFixedFunction {
    public void Apply(Span<Vector4> row, bool inverted);
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed class Lut1D {
    private const int HalfCodes = ushort.MaxValue + 1;

    private readonly ImmutableArray<float> planes;

    private Lut1D(ReadOnlySpan<float> rows, Vector3 domainMinimum, Vector3 domainMaximum, bool halfDomain) {
        float[] planar = GC.AllocateUninitializedArray<float>(rows.Length);
        (Length, DomainMinimum, DomainMaximum, HalfDomain) = (rows.Length / 3, domainMinimum, domainMaximum, halfDomain);
        for (int channel = 0; channel < 3; channel++)
            rows.AsSpan2D(Length, 3).GetColumn(channel).CopyTo(planar.AsSpan().AsSpan2D(3, Length).GetRowSpan(channel));
        planes = ImmutableCollectionsMarshal.AsImmutableArray(planar);
    }

    public int Length { get; }
    public Vector3 DomainMinimum { get; }
    public Vector3 DomainMaximum { get; }
    public bool HalfDomain { get; }

    public static Fin<Lut1D> From(ReadOnlyMemory<float> rows, Vector3 domainMinimum, Vector3 domainMaximum, bool halfDomain = false) =>
        (ImagingFault.Unless(rows.Length % 3 is 0 && (halfDomain ? rows.Length == 3 * HalfCodes : rows.Length >= 6), ImagingReason.Miscounted, rows.Length),
         halfDomain ? unit : LutTables.Checked(rows.Span, domainMinimum, domainMaximum))
            .Apply((_, _) => new Lut1D(rows.Span, domainMinimum, domainMaximum, halfDomain)).As();

    public ReadOnlySpan<float> Channel(int channel) => planes.AsSpan().AsSpan2D(3, Length).GetRowSpan(channel);

    public Vector3 Sample(Vector3 value) =>
        HalfDomain
            ? new(Coded(Channel(0), value.X), Coded(Channel(1), value.Y), Coded(Channel(2), value.Z))
            : Vector3.Min(Vector3.MaxNumber((value - DomainMinimum) / (DomainMaximum - DomainMinimum) * (Length - 1), Vector3.Zero), new Vector3(Length - 1)) switch {
                var position => new(Linear(Channel(0), position.X), Linear(Channel(1), position.Y), Linear(Channel(2), position.Z)),
            };

    private static float Linear(ReadOnlySpan<float> table, float position) =>
        float.Lerp(table[(int)position], table[(int)float.Ceiling(position)], position - float.Floor(position));

    private static float Coded(ReadOnlySpan<float> table, float value) {
        Half code = (Half)value;
        Half next = value > (float)code ? Half.BitIncrement(code) : Half.BitDecrement(code);
        return Half.IsFinite(code) && Half.IsFinite(next) && (float)code != value
            ? float.Lerp(table[BitConverter.HalfToUInt16Bits(code)], table[BitConverter.HalfToUInt16Bits(next)], (value - (float)code) / ((float)next - (float)code))
            : table[BitConverter.HalfToUInt16Bits(code)];
    }
}

public sealed record LogCurve(float Base, Vector3 LogSideSlope, Vector3 LogSideOffset, Vector3 LinSideSlope, Vector3 LinSideOffset, Option<(Vector3 LinSideBreak, Vector3 LinearSlope)> Camera = default) {
    private static readonly Vector3 Floor = new(float.ScaleB(1f, -126));

    public static LogCurve Allocation(float minimum, float maximum, float offset) =>
        new(2f, new(1f / (maximum - minimum)), new(-minimum / (maximum - minimum)), Vector3.One, new(offset));

    public static LogCurve Of(float logBase, Vector3 logSideSlope, Vector3 logSideOffset, Vector3 linSideSlope, Vector3 linSideOffset, Vector3 linSideBreak, Option<Vector3> linearSlope = default) =>
        new(logBase, logSideSlope, logSideOffset, linSideSlope, linSideOffset,
            Some((linSideBreak, linearSlope.IfNone(() => logSideSlope * linSideSlope / (((linSideSlope * linSideBreak) + linSideOffset) * float.Log(logBase))))));

    public Vector3 Forward(Vector3 linear) =>
        Camera.Match(
            Some: camera => Vector3.ConditionalSelect(Vector3.LessThanOrEqual(linear, camera.LinSideBreak), (camera.LinearSlope * (linear - camera.LinSideBreak)) + Logarithmic(camera.LinSideBreak), Logarithmic(linear)),
            None: () => Logarithmic(linear));

    public Vector3 Inverse(Vector3 encoded) =>
        Camera.Match(
            Some: camera => Logarithmic(camera.LinSideBreak) switch {
                var joint => Vector3.ConditionalSelect(Vector3.LessThanOrEqual(encoded, joint), ((encoded - joint) / camera.LinearSlope) + camera.LinSideBreak, Exponential(encoded)),
            },
            None: () => Exponential(encoded));

    private Vector3 Logarithmic(Vector3 linear) => (LogSideSlope * (Vector3.Log(Vector3.Max((LinSideSlope * linear) + LinSideOffset, Floor)) / float.Log(Base))) + LogSideOffset;

    private Vector3 Exponential(Vector3 encoded) => (Vector3.Exp((encoded - LogSideOffset) / LogSideSlope * float.Log(Base)) - LinSideOffset) / LinSideSlope;
}

[SmartEnum<string>]
public sealed partial class ExponentStyle {
    public static readonly ExponentStyle BasicFwd = new("basicFwd", monCurve: false, mirror: false, passThru: false, forward: true);
    public static readonly ExponentStyle BasicRev = new("basicRev", monCurve: false, mirror: false, passThru: false, forward: false);
    public static readonly ExponentStyle BasicMirrorFwd = new("basicMirrorFwd", monCurve: false, mirror: true, passThru: false, forward: true);
    public static readonly ExponentStyle BasicMirrorRev = new("basicMirrorRev", monCurve: false, mirror: true, passThru: false, forward: false);
    public static readonly ExponentStyle BasicPassThruFwd = new("basicPassThruFwd", monCurve: false, mirror: false, passThru: true, forward: true);
    public static readonly ExponentStyle BasicPassThruRev = new("basicPassThruRev", monCurve: false, mirror: false, passThru: true, forward: false);
    public static readonly ExponentStyle MonCurveFwd = new("monCurveFwd", monCurve: true, mirror: false, passThru: false, forward: true);
    public static readonly ExponentStyle MonCurveRev = new("monCurveRev", monCurve: true, mirror: false, passThru: false, forward: false);
    public static readonly ExponentStyle MonCurveMirrorFwd = new("monCurveMirrorFwd", monCurve: true, mirror: true, passThru: false, forward: true);
    public static readonly ExponentStyle MonCurveMirrorRev = new("monCurveMirrorRev", monCurve: true, mirror: true, passThru: false, forward: false);

    private readonly bool monCurve;
    private readonly bool mirror;
    private readonly bool passThru;
    private readonly bool forward;

    public ExponentStyle Inverse => toSeq(Items).Find(item => (item.monCurve, item.mirror, item.passThru, item.forward) == (monCurve, mirror, passThru, !forward)).IfNone(this);

    public void Apply(Span<Vector4> row, Vector3 power, Vector3 offset) {
        (Vector3 knee, Vector3 slope) = Joint(power, offset);
        (Vector3 exponent, Vector3 curved) = (forward ? power : Vector3.One / power, monCurve ? Vector3.GreaterThan(offset, Vector3.Zero) : Vector3.Zero);
        foreach (ref Vector4 pixel in row) {
            Vector3 x = pixel.AsVector3();
            Vector3 magnitude = mirror || passThru ? Vector3.Abs(x) : monCurve ? x : Vector3.Max(x, Vector3.Zero);
            Vector3 joined = forward
                ? Vector3.ConditionalSelect(Vector3.GreaterThanOrEqual(magnitude, knee), Raised((magnitude + offset) / (Vector3.One + offset), power), magnitude * slope)
                : Vector3.ConditionalSelect(Vector3.GreaterThanOrEqual(magnitude, knee * slope), ((Vector3.One + offset) * Raised(magnitude, exponent)) - offset, magnitude / slope);
            Vector3 y = Vector3.ConditionalSelect(curved, joined, Raised(Vector3.Max(magnitude, Vector3.Zero), exponent));
            pixel = new Vector4(mirror ? Vector3.CopySign(y, x) : passThru ? Vector3.ConditionalSelect(Vector3.LessThan(x, Vector3.Zero), x, y) : y, pixel.W);
        }
    }

    internal static (Vector3 Knee, Vector3 Slope) Joint(Vector3 power, Vector3 offset) =>
        (offset / (power - Vector3.One), (power - Vector3.One) / offset * Raised(offset * power / ((power - Vector3.One) * (Vector3.One + offset)), power));

    private static Vector3 Raised(Vector3 value, Vector3 power) => Vector3.Exp(power * Vector3.Log(value));
}

[SmartEnum<string>]
public sealed partial class CdlStyle {
    public static readonly CdlStyle Fwd = new("Fwd", "v1.2_Fwd", forward: true, clamps: true);
    public static readonly CdlStyle FwdNoClamp = new("FwdNoClamp", "noClampFwd", forward: true, clamps: false);
    public static readonly CdlStyle Rev = new("Rev", "v1.2_Rev", forward: false, clamps: true);
    public static readonly CdlStyle RevNoClamp = new("RevNoClamp", "noClampRev", forward: false, clamps: false);

    private static readonly Vector3 Luma = new((float)YbrConfiguration.Rec709.Kr, (float)YbrConfiguration.Rec709.Kg, (float)YbrConfiguration.Rec709.Kb);

    public string Legacy { get; }

    private readonly bool forward;
    private readonly bool clamps;

    public CdlStyle Inverse => toSeq(Items).Find(item => (item.forward, item.clamps) == (!forward, clamps)).IfNone(this);

    public Vector3 Apply(Vector3 color, Vector3 slope, Vector3 offset, Vector3 power, float saturation) =>
        forward
            ? Bounded(Saturated(Raised(Bounded((color * slope) + offset), power), new Vector3(saturation)))
            : Bounded((Raised(Bounded(Saturated(Bounded(color), Reciprocal(new Vector3(saturation)))), Reciprocal(power)) - offset) * Reciprocal(slope));

    private Vector3 Bounded(Vector3 color) => clamps ? Vector3.Min(Vector3.MaxNumber(color, Vector3.Zero), Vector3.One) : color;

    private static Vector3 Saturated(Vector3 color, Vector3 saturation) => new Vector3(Vector3.Dot(color, Luma)) switch { var luma => luma + (saturation * (color - luma)) };

    private static Vector3 Raised(Vector3 color, Vector3 power) => new(Raised(color.X, power.X), Raised(color.Y, power.Y), Raised(color.Z, power.Z));

    private static float Raised(float value, float power) => float.IsNaN(value) ? 0f : value < 0f ? value : float.Pow(value, power);

    private static Vector3 Reciprocal(Vector3 value) => Vector3.One / Vector3.Max(value, new Vector3(0.01f));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record LutTable {
    public virtual Seq<LutTable> Steps => [this];

    protected virtual bool PrintMembers(StringBuilder builder) => false;

    public abstract void Apply(Span<Vector4> row);
    public abstract Fin<LutTable> Inverse();

    public static LutTable Of(Seq<LutTable> steps) => steps.Bind(static step => step.Steps) switch {
        [var step] => step,
        var flat => new Sequence(flat),
    };

    public sealed record Cube(Lut3D Lattice, LutInterpolation Interpolation) : LutTable {
        public override void Apply(Span<Vector4> row) => Lattice.Apply(MemoryMarshal.Cast<Vector4, float>(row), MemoryMarshal.Cast<Vector4, float>(row), 4, Interpolation);
        public override Fin<LutTable> Inverse() => ImagingFault.Of(ImagingReason.NoInverse, Lattice.Size);
    }

    public sealed record ChannelCurve : LutTable {
        private static readonly Comparer<float> Rising = Comparer<float>.Create(static (value, entry) => value <= entry ? -1 : 1);
        private static readonly Comparer<float> Falling = Comparer<float>.Create(static (value, entry) => value >= entry ? -1 : 1);

        private readonly Seq<(Comparer<float> Order, int Start, int End)> ranges;

        public ChannelCurve(Lut1D table, bool hueAdjust = false) : this(table, hueAdjust, []) { }

        private ChannelCurve(Lut1D table, bool hueAdjust, Seq<(Comparer<float> Order, int Start, int End)> ranges) => (Table, HueAdjust, this.ranges) = (table, hueAdjust, ranges);

        public Lut1D Table { get; }
        public bool HueAdjust { get; }
        public bool Inverted => !ranges.IsEmpty;

        public override void Apply(Span<Vector4> row) {
            foreach (ref Vector4 pixel in row) {
                Vector3 color = pixel.AsVector3();
                Vector3 mapped = Inverted ? new(Invert(0, color.X), Invert(1, color.Y), Invert(2, color.Z)) : Table.Sample(color);
                pixel = new Vector4(HueAdjust ? Restored(color, mapped) : mapped, pixel.W);
            }
        }

        public override Fin<LutTable> Inverse() =>
            Inverted
                ? new ChannelCurve(Table, HueAdjust)
                : ImagingFault.Unless(!Table.HalfDomain, ImagingReason.Unsupported, Table.HalfDomain)
                    .Bind(_ => toSeq(Enumerable.Range(0, 3)).Traverse(channel => Monotonic(Table.Channel(channel)).ToFin(ImagingFault.Of(ImagingReason.NoInverse, channel, channel))).As())
                    .Map(found => (LutTable)new ChannelCurve(Table, HueAdjust, found));

        private float Invert(int channel, float value) {
            ReadOnlySpan<float> table = Table.Channel(channel)[ranges[channel].Start..(ranges[channel].End + 1)];
            float bounded = float.IsNaN(value) ? table[0] : float.Clamp(value, float.Min(table[0], table[^1]), float.Max(table[0], table[^1]));
            int above = ~table.BinarySearch(bounded, ranges[channel].Order);
            float position = ranges[channel].Start + (above is 0 ? 0f : above - 1 + ((bounded - table[above - 1]) / (table[above] - table[above - 1])));
            return Table.DomainMinimum[channel] + (position / (Table.Length - 1) * (Table.DomainMaximum[channel] - Table.DomainMinimum[channel]));
        }

        private static Option<(Comparer<float> Order, int Start, int End)> Monotonic(ReadOnlySpan<float> table) {
            float sign = float.Sign(table[^1] - table[0]);
            for (int entry = 1; entry < table.Length && sign is not 0f; entry++)
                sign = sign * (table[entry] - table[entry - 1]) < 0f ? 0f : sign;
            return sign is 0f ? None : (sign > 0f ? Rising : Falling, table.IndexOfAnyExcept(table[0]) - 1, table.LastIndexOfAnyExcept(table[^1]) + 1);
        }

        private static Vector3 Restored(Vector3 original, Vector3 mapped) {
            int high = original.X >= original.Y ? (original.X >= original.Z ? 0 : 2) : (original.Y >= original.Z ? 1 : 2);
            int low = original.X < original.Y ? (original.X < original.Z ? 0 : 2) : (original.Y < original.Z ? 1 : 2);
            float chroma = original[high] - original[low];
            mapped[3 - high - low] = mapped[low] + ((chroma is 0f ? 0f : (original[3 - high - low] - original[low]) / chroma) * (mapped[high] - mapped[low]));
            return mapped;
        }
    }

    public sealed record Affine(Matrix4x4 Matrix) : LutTable {
        public override void Apply(Span<Vector4> row) {
            foreach (ref Vector4 pixel in row) pixel = new Vector4(Vector3.Transform(pixel.AsVector3(), Matrix), pixel.W);
        }
        public override Fin<LutTable> Inverse() => Matrix4x4.Invert(Matrix, out Matrix4x4 inverse) ? new Affine(inverse) : ImagingFault.Of(ImagingReason.NoInverse, Matrix);
    }

    public sealed record Range(Option<(float In, float Out)> Minimum, Option<(float In, float Out)> Maximum, bool Clamps = true) : LutTable {
        public override void Apply(Span<Vector4> row) {
            (float scale, float shift, float low, float high) = Minimum.Match(
                Some: bottom => Maximum.Match(
                    Some: top => ((top.Out - bottom.Out) / (top.In - bottom.In)) switch {
                        var gain => (gain, bottom.Out - (bottom.In * gain), Clamps ? float.Min(bottom.Out, top.Out) : float.NegativeInfinity, Clamps ? float.Max(bottom.Out, top.Out) : float.PositiveInfinity),
                    },
                    None: () => (1f, bottom.Out - bottom.In, bottom.Out, float.PositiveInfinity)),
                None: () => Maximum.Match(Some: static top => (1f, top.Out - top.In, float.NegativeInfinity, top.Out), None: static () => (1f, 0f, float.NegativeInfinity, float.PositiveInfinity)));
            foreach (ref Vector4 pixel in row) pixel = new Vector4(Vector3.Clamp((pixel.AsVector3() * scale) + new Vector3(shift), new Vector3(low), new Vector3(high)), pixel.W);
        }
        public override Fin<LutTable> Inverse() =>
            ImagingFault.Unless(!(from bottom in Minimum from top in Maximum select bottom.Out == top.Out).IfNone(noneValue: false), ImagingReason.NoInverse, (Minimum, Maximum))
                .Map(_ => (LutTable)new Range(Minimum.Map(static bound => (bound.Out, bound.In)), Maximum.Map(static bound => (bound.Out, bound.In)), Clamps));
    }

    public sealed record Log(LogCurve Curve, bool Inverted = false) : LutTable {
        public override void Apply(Span<Vector4> row) {
            foreach (ref Vector4 pixel in row) pixel = new Vector4(Inverted ? Curve.Inverse(pixel.AsVector3()) : Curve.Forward(pixel.AsVector3()), pixel.W);
        }
        public override Fin<LutTable> Inverse() => this with { Inverted = !Inverted };
    }

    public sealed record Exponent(ExponentStyle Style, Vector3 Power, Vector3 Offset) : LutTable {
        public override void Apply(Span<Vector4> row) => Style.Apply(row, Power, Offset);
        public override Fin<LutTable> Inverse() => this with { Style = Style.Inverse };
    }

    public sealed record Cdl(CdlStyle Style, Vector3 Slope, Vector3 Offset, Vector3 Power, float Saturation) : LutTable {
        public override void Apply(Span<Vector4> row) {
            foreach (ref Vector4 pixel in row) pixel = new Vector4(Style.Apply(pixel.AsVector3(), Slope, Offset, Power, Saturation), pixel.W);
        }
        public override Fin<LutTable> Inverse() => this with { Style = Style.Inverse };
    }

    public sealed record FixedFunction(IFixedFunction Function, bool Inverted = false) : LutTable {
        public override void Apply(Span<Vector4> row) => Function.Apply(row, Inverted);
        public override Fin<LutTable> Inverse() => this with { Inverted = !Inverted };
    }

    public sealed record Sequence : LutTable {
        internal Sequence(Seq<LutTable> steps) => Steps = steps;

        public override Seq<LutTable> Steps { get; }

        public override void Apply(Span<Vector4> row) {
            foreach (LutTable step in Steps) step.Apply(row);
        }
        public override Fin<LutTable> Inverse() => Steps.Rev().Traverse(static step => step.Inverse()).As().Map(Of);
    }
}

public sealed record LutSpace(ColorEncoding Encoding, LutTable Into, LutTable Back) {
    private static readonly LogCurve Base = LogCurve.Allocation(-12.473931188f, 12.526068812f, 0f);
    private static readonly LutTable Inset = new LutTable.Affine(Matrix4x4.Transpose(new(
        0.856627153315983f, 0.0951212405381588f, 0.0482516061458583f, 0f,
        0.137318972929847f, 0.761241990602591f, 0.101439036467562f, 0f,
        0.11189821299995f, 0.0767994186031903f, 0.811302368396859f, 0f,
        0f, 0f, 0f, 1f)));

    public static LutSpace Filmic { get; } = Of(Gamut.StandardRgb, Base);

    public static LutSpace Of(ColorEncoding signal) => new(signal, LutTable.Of([]), LutTable.Of([]));

    public static LutSpace Of(Gamut gamut, LogCurve allocation) => new(new(gamut, new Transfer.Linear()), new LutTable.Log(allocation), new LutTable.Log(allocation, Inverted: true));

    public static LutSpace Rational(Gamut gamut) =>
        (new LogCurve(2f, Vector3.One, Vector3.Zero, Vector3.One, Vector3.One), new LogCurve(2f, -Vector3.One, Vector3.Zero, -Vector3.One, Vector3.One)) switch {
            var (lifted, folded) => new(new(gamut, new Transfer.Linear()),
                LutTable.Of([new LutTable.Log(lifted), new LutTable.Log(folded, Inverted: true)]),
                LutTable.Of([new LutTable.Log(folded), new LutTable.Log(lifted, Inverted: true), new LutTable.Range(None, Some(((float)Half.MaxValue, (float)Half.MaxValue)))])),
        };

    public static LutSpace AgX(Lut3D compensation) =>
        (new ColorEncoding(Gamut.EGamut, new Transfer.Linear()), new ColorEncoding(Gamut.Rec2020, new Transfer.Linear())) switch {
            var (native, wide) => new(native,
                LutTable.Of([new LutTable.Log(Base), new LutTable.Cube(compensation, LutInterpolation.Tetrahedral), new LutTable.Log(Base, Inverted: true), native.To(wide), Inset, new LutTable.Log(Base)]),
                LutTable.Of([new LutTable.Log(Base, Inverted: true), Inset.Inverse().ThrowIfFail(), wide.To(native)])),
        };

    public LutTable Around(ColorEncoding working, LutTable step) => LutTable.Of([working.To(Encoding), Into, step, Back, Encoding.To(working)]);

    public LutTable Exact(LutTable chain, Gamut working, ColorEncoding target) =>
        new ColorEncoding(working, new Transfer.Linear()) switch {
            var linear => LutTable.Of([Encoding.To(linear), chain, linear.To(target)]),
        };

    public Fin<LutTable> Bake(int cubeSize, Seq<Action<Span<Vector4>>> chain, Gamut working, ColorEncoding target) {
        ColorEncoding linear = new(working, new Transfer.Linear());
        using MemoryOwner<float> samples = MemoryOwner<float>.Allocate(3 * cubeSize * cubeSize * cubeSize);
        ParallelHelper.For2D(..cubeSize, ..cubeSize, new LatticeRows(cubeSize, [Back.Apply, Encoding.To(linear).Apply, .. chain, linear.To(target).Apply], samples.Memory));
        return Lut3D.From(cubeSize, samples.Memory, Vector3.Zero, Vector3.One).Map(lattice => LutTable.Of([Into, new LutTable.Cube(lattice, LutInterpolation.Tetrahedral)]));
    }
}

file readonly struct LatticeRows(int edge, Seq<Action<Span<Vector4>>> steps, Memory<float> samples) : IAction2D {
    public void Invoke(int i, int j) {
        using SpanOwner<Vector4> nodes = SpanOwner<Vector4>.Allocate(edge);
        Span<Vector4> row = nodes.Span;
        for (int red = 0; red < edge; red++) row[red] = new Vector4(red, j, i, edge - 1) / (edge - 1);
        foreach (Action<Span<Vector4>> step in steps) step(row);
        MemoryMarshal.Cast<Vector4, float>(row).AsSpan2D(edge, 4)[.., ..3].CopyTo(samples.Span.AsSpan2D(edge * edge, 3 * edge).GetRowSpan((edge * i) + j));
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class LutTables {
    extension(Lut3D) {
        public static Fin<Lut3D> From(int size, ReadOnlyMemory<float> samples, Vector3 domainMinimum, Vector3 domainMaximum) =>
            Checked(samples.Span, domainMinimum, domainMaximum).Bind(_ => Try.lift(() => new Lut3D(size, samples.Span, domainMinimum, domainMaximum)).Run()
                .MapFail(cause => ImagingFault.Of(ImagingReason.Refused, size, inner: cause)));
    }

    internal static Fin<Unit> Checked(ReadOnlySpan<float> samples, Vector3 domainMinimum, Vector3 domainMaximum) =>
        (ImagingFault.Unless(TensorPrimitives.IsFiniteAll(samples), ImagingReason.Range, samples.Length),
         ImagingFault.Unless(Vector3.AllWhereAllBitsSet(Vector3.IsFinite(domainMinimum) & Vector3.IsFinite(domainMaximum) & Vector3.LessThan(domainMinimum, domainMaximum)), ImagingReason.Degenerate, (domainMinimum, domainMaximum)))
            .Apply(static (_, _) => unit).As();
}
