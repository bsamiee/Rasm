using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using MathNet.Numerics;
using PeterO.Numbers;
using Rasm.Imaging.Color;
using Rasm.Imaging.Files;
using Rasm.Imaging.Filters.Generators;
using Rasm.Imaging.Grade;
using Rasm.Imaging.Pixels;
using TorchSharp;
using UnitsNet;
using UnitsNet.Units;

namespace Rasm.Imaging.Filters.Optics;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidOptics>]
public readonly partial struct HighlightThreshold : IMinMaxValue<HighlightThreshold> {
    public static HighlightThreshold MinValue { get; } = new(0f);
    public static HighlightThreshold MaxValue { get; } = new(HistogramAxis.LowerBound(HistogramAxis.Bins - 1));
    public static HighlightThreshold White { get; } = new(1f);
    public static HighlightThreshold OneStopOver { get; } = new(2f);

    static partial void ValidateFactoryArguments(ref InvalidOptics? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidOptics();
}

[ValueObject<int>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidOptics>]
public readonly partial struct GhostIterations : IMinMaxValue<GhostIterations> {
    public static GhostIterations MinValue { get; } = new(2);
    public static GhostIterations MaxValue { get; } = new(5);
    public static GhostIterations Three { get; } = new(3);

    static partial void ValidateFactoryArguments(ref InvalidOptics? validationError, ref int value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidOptics();
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidOptics>]
public readonly partial struct DirtGain : IMinMaxValue<DirtGain> {
    public static DirtGain MinValue { get; } = new(0f);
    public static DirtGain MaxValue { get; } = new(5f);
    public static DirtGain Unity { get; } = new(1f);

    static partial void ValidateFactoryArguments(ref InvalidOptics? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidOptics();
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidOptics>]
public readonly partial struct DirtCoverage : IMinMaxValue<DirtCoverage> {
    public static DirtCoverage MinValue { get; } = new(0.01f);
    public static DirtCoverage MaxValue { get; } = new(1f);
    public static DirtCoverage Fifth { get; } = new(0.2f);

    static partial void ValidateFactoryArguments(ref InvalidOptics? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidOptics();
}

[ValueObject<int>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidOptics>]
public readonly partial struct ApertureBlades : IMinMaxValue<ApertureBlades> {
    public static ApertureBlades MinValue { get; } = new(3);
    public static ApertureBlades MaxValue { get; } = new(24);
    public static ApertureBlades Six { get; } = new(6);

    static partial void ValidateFactoryArguments(ref InvalidOptics? validationError, ref int value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidOptics();
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidOptics>]
public readonly partial struct AnamorphicSqueeze : IMinMaxValue<AnamorphicSqueeze> {
    public static AnamorphicSqueeze MinValue => Spherical;
    public static AnamorphicSqueeze MaxValue { get; } = new(2f);
    public static AnamorphicSqueeze Spherical { get; } = new(1f);

    static partial void ValidateFactoryArguments(ref InvalidOptics? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidOptics();
}

[ValueObject<float>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Neutral", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidOptics>]
public readonly partial struct ColorFringe : IMinMaxValue<ColorFringe> {
    public static ColorFringe MinValue { get; } = new(-1f);
    public static ColorFringe MaxValue { get; } = new(1f);

    static partial void ValidateFactoryArguments(ref InvalidOptics? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidOptics();
}

public sealed record Iris(ApertureBlades Blades, SignedAngle Rotation, AxisFraction Roundness, AxisFraction Obstruction, AnamorphicSqueeze Squeeze, ColorFringe Fringe) {
    public static Iris Hexagon { get; } =
        new(ApertureBlades.Six, SignedAngle.Create(float.DegreesToRadians(60f)), AxisFraction.MinValue, AxisFraction.MinValue, AnamorphicSqueeze.Spherical, ColorFringe.Neutral);

    public float Circumradius =>
        double.Asin(Roundness * double.Sin(double.Pi / Blades)) switch {
            var beta => (float)double.Sqrt(double.Pi / (Blades / 2d * (double.Sin(double.Tau / Blades) + (Roundness == AxisFraction.MinValue ? 0d : ((2d * beta) - double.Sin(2d * beta)) / ((double)Roundness * Roundness))))),
        };

    public (double Reach, double Edge) Outline((double X, double Y) q) {
        (double x, double y) = (Squeeze * q.X, q.Y);
        double edge = Boundary(double.Atan2(-y, x));
        return (double.Hypot(x, y) / edge, edge);
    }

    private double Boundary(double angle) {
        (int n, double kappa) = (Blades, Roundness);
        (double s1, double c1) = double.SinCos(double.Pi / n);
        (double sa, double ca) = double.SinCos(double.Ieee754Remainder(angle - Rotation - (double.Pi / n), double.Tau / n));
        double root = double.Sqrt(1d - (kappa * kappa * s1 * s1));
        double delta = root - (kappa * c1);
        return Circumradius * ((2d * c1 * root) - (kappa * (1d - (2d * s1 * s1)))) / ((delta * ca) + double.Sqrt(1d - (delta * delta * sa * sa)));
    }

    internal Func<(double X, double Y), double> Transmission(double radius) {
        (double obstruction, double squeeze, double circumradius) = (Obstruction, Squeeze, Circumradius);
        double margin = double.Hypot(squeeze, 1d) / (2d * circumradius * double.Cos(double.Pi / Blades));
        double[] blades = [.. Enumerable.Range(0, Blades).Select(i => double.Tau * i / Blades)];
        return point => {
            (double x, double y) = (squeeze * point.X, point.Y);
            double reach = double.Hypot(x, y) / Boundary(double.Atan2(-y, x));
            if (reach - margin >= radius || reach + margin <= obstruction * radius)
                return 0d;
            if (reach + margin <= radius && reach - margin >= obstruction * radius)
                return 1d / (1d - obstruction);
            (double left, double right, double top, double bottom) = (x - (squeeze / 2d), x + (squeeze / 2d), y - 0.5d, y + 0.5d);
            double[] angles = [.. new[] { 0d, double.Pi / 2d, double.Pi, 3d * double.Pi / 2d, double.Tau }
                .Concat(blades.Select(angle => (((-Rotation - angle) % double.Tau) + double.Tau) % double.Tau))
                .Concat(new[] { (left, top), (left, bottom), (right, top), (right, bottom) }.Select(static corner => (double.Atan2(corner.Item2, corner.Item1) + double.Tau) % double.Tau))
                .Order()];
            double area = 0d;
            for (int i = 1; i < angles.Length; i++)
                area += Integrate.GaussKronrod(Section, angles[i - 1], angles[i]);
            return area / squeeze;

            double Section(double angle) {
                (double sin, double cos) = double.SinCos(angle);
                (double nearX, double farX) = cos == 0d ? left <= 0d && right >= 0d ? (0d, double.PositiveInfinity) : (double.PositiveInfinity, double.NegativeInfinity)
                    : (double.Min(left / cos, right / cos), double.Max(left / cos, right / cos));
                (double nearY, double farY) = sin == 0d ? top <= 0d && bottom >= 0d ? (0d, double.PositiveInfinity) : (double.PositiveInfinity, double.NegativeInfinity)
                    : (double.Min(top / sin, bottom / sin), double.Max(top / sin, bottom / sin));
                double edge = radius * Boundary(-angle);
                (double near, double far) = (double.Max(0d, double.Max(double.Max(nearX, nearY), obstruction * edge)), double.Min(edge, double.Min(farX, farY)));
                return far > near ? (far - near) * (far + near) / (2d * (1d - obstruction)) : 0d;
            }
        };
    }

    public Vector256<double> Weights(double reach, double edge, double radius, double ramp) {
        Vector256<double> r = Radii(radius);
        Vector256<double> distance = Vector256.Create(reach);
        Vector256<double> one = Vector256.Create(1d);
        Vector256<double> obstruction = Vector256.Create((double)Obstruction) * r;
        Vector256<double> w = Obstruction == AxisFraction.MaxValue ? Vector256<double>.Zero : ramp == 0d
            ? Vector256.ConditionalSelect(Vector256.LessThanOrEqual(distance, r) & Vector256.GreaterThanOrEqual(distance, obstruction), one, Vector256<double>.Zero)
            : Vector256.Create(edge / ramp) switch {
                var slope => Vector256.Min(Vector256.Clamp((slope * (r - distance)) + Vector256.Create(0.5d), Vector256<double>.Zero, one), Vector256.Clamp(one + (slope * (distance - obstruction)), Vector256<double>.Zero, one)),
            };
        w = Vector256.ConditionalSelect(Vector256.GreaterThan(r, Vector256<double>.Zero), w, Vector256<double>.Zero);
        return Vector256.Create(w[0], w[1], w[2], (w[0] + w[1] + w[2]) / 3d);
    }

    public Vector256<double> Integral(double radius, double ramp) {
        Vector256<double> r = Radii(radius);
        double[] areas = [.. Enumerable.Range(0, 3).Select(component => r[component] == 0d || Obstruction == AxisFraction.MaxValue ? 0d
            : Blades / (double)Squeeze * Integrate.GaussKronrod(angle => {
                double boundary = r[component] * Boundary(angle);
                double inner = Obstruction * boundary;
                if (ramp == 0d)
                    return (boundary - inner) * (boundary + inner) / 2d;
                (double high, double low) = (boundary + (ramp / 2d), inner - ramp);
                double upper = double.Min(1d, double.Min(high / ramp, (high - low) / (2d * ramp)));
                double width = ramp * upper;
                return low >= 0d ? upper * (high + low) * (high - low - width) / 2d
                    : ((upper * ((high * high) - (high * width) + (width * width / 3d))) - (double.Pow(double.Max(0d, low + width), 3d) / (3d * ramp))) / 2d;
            }, Rotation, Rotation + (double.Tau / Blades)))];
        return Vector256.Create(areas[0], areas[1], areas[2], (areas[0] + areas[1] + areas[2]) / 3d);
    }

    private Vector256<double> Radii(double radius) =>
        Vector256.Create(radius * (1d - double.Max(Fringe, 0d)), radius * (1d - (double.Abs(Fringe) / 2d)), radius * (1d + double.Min(Fringe, 0d)), 0d);

    internal static (StateParameter<TRecord> Blades, StateParameter<TRecord> Rotation, StateParameter<TRecord> Roundness, StateParameter<TRecord> Obstruction, StateParameter<TRecord> Squeeze)
        Kinds<TRecord>(Lens<TRecord, Iris> iris) =>
        (StateParameter.Bounded<TRecord, ApertureBlades, int, InvalidOptics>(lens(iris, Lens<Iris, ApertureBlades>.New(static i => i.Blades, static blades => i => i with { Blades = blades })), new()),
         StateParameter.Bounded<TRecord, SignedAngle, float, InvalidPixelValue>(lens(iris, Lens<Iris, SignedAngle>.New(static i => i.Rotation, static rotation => i => i with { Rotation = rotation })), SignedAngle.Presentation),
         StateParameter.Bounded<TRecord, AxisFraction, float, InvalidGrade>(lens(iris, Lens<Iris, AxisFraction>.New(static i => i.Roundness, static roundness => i => i with { Roundness = roundness })), new()),
         StateParameter.Bounded<TRecord, AxisFraction, float, InvalidGrade>(lens(iris, Lens<Iris, AxisFraction>.New(static i => i.Obstruction, static obstruction => i => i with { Obstruction = obstruction })), new()),
         StateParameter.Bounded<TRecord, AnamorphicSqueeze, float, InvalidOptics>(lens(iris, Lens<Iris, AnamorphicSqueeze>.New(static i => i.Squeeze, static squeeze => i => i with { Squeeze = squeeze })), new()));

    internal static StateParameter<TRecord> FringeKind<TRecord>(Lens<TRecord, Iris> iris) =>
        StateParameter.Bounded<TRecord, ColorFringe, float, InvalidOptics>(lens(iris, Lens<Iris, ColorFringe>.New(static i => i.Fringe, static fringe => i => i with { Fringe = fringe })), new() { Origin = (float)ColorFringe.Neutral });
}

public sealed record HighlightKey(HighlightThreshold Threshold, AxisFraction Softness) {
    public Vector4 Extract(Vector4 color) {
        Vector3 light = color.AsVector3() * color.W;
        float v = float.Max(light.X, float.Max(light.Y, light.Z));
        float k = float.Min(Softness, Threshold);
        float h = float.Max(k - float.Abs(v - Threshold), 0f);
        return v > 0f ? new Vector4(light * ((float.Max(v, Threshold) - Threshold + (k > 0f ? h * h / (4f * k) : 0f)) / v), 0f) : Vector4.Zero;
    }

    internal static (StateParameter<TRecord> Threshold, StateParameter<TRecord> Softness) Kinds<TRecord>(Lens<TRecord, HighlightKey> key) =>
        (StateParameter.Bounded<TRecord, HighlightThreshold, float, InvalidOptics>(lens(key, Lens<HighlightKey, HighlightThreshold>.New(static k => k.Threshold, static threshold => k => k with { Threshold = threshold })), new()),
         StateParameter.Bounded<TRecord, AxisFraction, float, InvalidGrade>(lens(key, Lens<HighlightKey, AxisFraction>.New(static k => k.Softness, static softness => k => k with { Softness = softness })), new()));
}

public sealed unsafe record DirtSpecks(DirtCoverage Coverage, ShortSideExtent Scale, Seed Seed) : IStateRecord<DirtSpecks, DirtSpecksParameter, InvalidOptics> {
    public static DirtSpecks Default { get; } = new(DirtCoverage.Fifth, ShortSideExtent.Create(0.05f * ReferenceFrame.GreaterSide), Seed.MinValue);

    internal static PixelFrame Field((DirtSpecks Specks, PixelExtent Extent) key) {
        const int frequencyPair = 2;
        static double Frequency(int index, int length) => (index <= length / 2 ? index : index - length) / (double)length;
        Size size = new(CvInvoke.GetOptimalDFTSize(int.Max(frequencyPair, key.Extent.Width)), CvInvoke.GetOptimalDFTSize(int.Max(frequencyPair, key.Extent.Height)));
        using Mat draw = new(size.Height, size.Width, DepthType.Cv64F, 1);
        using Mat pink = new(size.Height, size.Width, DepthType.Cv64F, 2);
        using Mat spectrum = new();
        uint field = CoordinateHash.Field(NoiseStream.LensDirt, key.Specks.Seed, 0u);
        double knee = 1d / key.Specks.Scale.Pixels(key.Extent);
        double reference = double.Hypot(1d / int.Max(size.Width, size.Height), knee);
        Span<double> cells = new((void*)draw.DataPointer, size.Width * size.Height);
        Span<Complex> gains = new((void*)pink.DataPointer, cells.Length);
        for (int i = 0; i < cells.Length; i++) {
            (int y, int x) = int.DivRem(i, size.Width);
            double gain = x == 0 && y == 0 ? 0d : reference / double.Hypot(double.Hypot(Frequency(x, size.Width), Frequency(y, size.Height)), knee);
            (cells[i], gains[i]) = (CoordinateHash.Normals(x, y, field).X, new Complex(gain, gain));
        }
        CvInvoke.Dft(draw, spectrum, DxtType.ComplexOutput, 0);
        CvInvoke.Multiply(spectrum, pink, spectrum);
        CvInvoke.Dft(spectrum, draw, DxtType.Inverse | DxtType.RealOutput | DxtType.Scale, 0);
        double[] sorted = cells.ToArray();
        sorted.AsSpan().Sort();
        double rank = (1d - key.Specks.Coverage) * (sorted.Length - 1);
        (int lower, int upper) = ((int)rank, (int)double.Ceiling(rank));
        double floor = double.Lerp(sorted[lower], sorted[upper], rank - lower);
        draw.ConvertTo(draw, DepthType.Cv64F, 1d, -floor);
        _ = CvInvoke.Threshold(draw, draw, 0d, 0d, ThresholdType.ToZero);
        CvInvoke.Normalize(draw, draw, 0d, 1d, NormType.MinMax);
        return new PixelFrame(Point.Empty, key.Extent, key.Extent, block => {
            ReadOnlySpan2D<double> values = new((void*)draw.DataPointer, size.Height, size.Width, 0);
            Span<Vector4> plane = MemoryMarshal.Cast<float, Vector4>(block.AsSpan());
            for (int i = 0; i < plane.Length; i++)
                plane[i] = int.DivRem(i, key.Extent.Width) switch { var (y, x) => new Vector4((float)values[y, x]) };
        });
    }
}

[SmartEnum<string>]
[ValidationError<InvalidOptics>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class DirtSpecksParameter : IStateParameter<DirtSpecks> {
    public static readonly DirtSpecksParameter Coverage = new("coverage", StateParameter.Bounded<DirtSpecks, DirtCoverage, float, InvalidOptics>(
        Lens<DirtSpecks, DirtCoverage>.New(static specks => specks.Coverage, static coverage => specks => specks with { Coverage = coverage }), new()));
    public static readonly DirtSpecksParameter Scale = new("scale", StateParameter.Bounded<DirtSpecks, ShortSideExtent, float, InvalidPixelValue>(
        Lens<DirtSpecks, ShortSideExtent>.New(static specks => specks.Scale, static scale => specks => specks with { Scale = scale }),
        new() { Unit = Quantity.GetUnitInfo(RatioUnit.DecimalFraction), Soft = (0.001f * ReferenceFrame.GreaterSide, 0.5f * ReferenceFrame.GreaterSide) }));
    public static readonly DirtSpecksParameter Seed = new("seed", StateParameter.Bounded<DirtSpecks, Seed, int, InvalidGenerator>(
        Lens<DirtSpecks, Seed>.New(static specks => specks.Seed, static seed => specks => specks with { Seed = seed }), Generators.Seed.Presentation));

    public StateParameter<DirtSpecks> Kind { get; }
}

public sealed unsafe record Bloom(AxisFraction Size, Mix Strength, Option<ImageFile> Dirt, Gated<DirtSpecks> Specks, DirtGain DirtGain) : IStateRecord<Bloom, BloomParameter, InvalidOptics>, IPixelStage<Bloom> {
    public static Bloom Default { get; } = new(AxisFraction.Half, Mix.Create(0.7f * float.Pow(0.15f, 1.5f)), None, new(Enabled: false, DirtSpecks.Default), DirtGain.Unity);

    public static Option<PixelPass> Pass(Bloom state, PassContext context) =>
        state.Strength == Mix.MinValue ? None : Some<PixelPass>(new PixelPass.Frame((frame, progress) => frame.Spread(Planes.Light, light => {
            using Mat tent = new(1, 3, DepthType.Cv32F, 1);
            tent.SetTo([0.25f, 0.5f, 0.25f]);
            int chain = (int)float.Log2(float.Max(1f, frame.Size.ShortSide * state.Size));
            return Chained(light, chain - 1, static tap => 1d / (1d + double.Max(0d, double.Max(tap.X, double.Max(tap.Y, tap.Z)))), tent).Map(_ => {
                _ = (state.Specks.Active.Map(specks => context.Derivations.Derived((specks, context.Extent), DirtSpecks.Field)) | state.Dirt.Map(static file => file.Frame)).Iter(dirt => {
                    using Mat lens = dirt.Header();
                    double cover = double.Max((double)frame.Extent.Width / lens.Cols, (double)frame.Extent.Height / lens.Rows);
                    using Mat fitted = new();
                    CvInvoke.Resize(lens, fitted, default, cover, cover, cover < 1d ? Inter.Area : Inter.Linear);
                    ReadOnlySpan2D<Vector4> soil = new((void*)fitted.DataPointer, fitted.Rows, fitted.Cols, 0);
                    Span<Vector4> cells = new((void*)light.DataPointer, frame.Block.Length / 4);
                    for (int i = 0; i < cells.Length; i++) {
                        (int y, int x) = int.DivRem(i, light.Cols);
                        cells[i] *= Vector4.One + (state.DirtGain * soil.Sample(
                            (((fitted.Cols - (double)frame.Extent.Width) / 2d) + frame.Origin.X + x + 0.5d, ((fitted.Rows - (double)frame.Extent.Height) / 2d) + frame.Origin.Y + y + 0.5d),
                            (WrapMode.Black, WrapMode.Black), Vector4.Zero, static (sum, pixel, weight) => sum + ((float)weight * pixel)));
                    }
                });
                return unit;
            });
        }, BlendingMode.Mix, state.Strength, progress)));

    private static Fin<Unit> Chained(Mat level, int halvings, Func<Vector4, double> weight, Mat tent) {
        if (halvings <= 0)
            return unit;
        using Mat next = new(level.Rows / 2, level.Cols / 2, DepthType.Cv32F, 4);
        using Mat up = new();
        ReadOnlySpan2D<Vector4> taps = new((void*)level.DataPointer, level.Rows, level.Cols, 0);
        Span<Vector4> cells = new((void*)next.DataPointer, next.Rows * next.Cols);
        for (int n = 0; n < cells.Length; n++) {
            (int y, int x) = int.DivRem(n, next.Cols);
            (double u, double v) = ((x + 0.5d) * level.Cols / next.Cols, (y + 0.5d) * level.Rows / next.Rows);
            Vector256<double> sum = (0.5d * Group(taps, u, v, weight)) + (0.125d * (Group(taps, u - 1d, v - 1d, weight) + Group(taps, u + 1d, v - 1d, weight)
                + Group(taps, u - 1d, v + 1d, weight) + Group(taps, u + 1d, v + 1d, weight)));
            cells[n] = Vector256.Narrow(sum, Vector256<double>.Zero).GetLower().AsVector4();
        }
        return Chained(next, halvings - 1, static _ => 1d, tent).Bind(_ => {
            CvInvoke.Resize(next, up, level.Size, 0d, 0d, Inter.Linear);
            return up.Separable(up, tent, tent, DepthType.Cv32F, PaddingModes.Replicate).Map(_ => {
                CvInvoke.AddWeighted(level, 1d / (halvings + 1), up, (double)halvings / (halvings + 1), 0d, level);
                return unit;
            });
        });
    }

    private static Vector256<double> Group(ReadOnlySpan2D<Vector4> taps, double x, double y, Func<Vector4, double> weight) {
        (Vector256<double> sum, double total) = (Vector256<double>.Zero, 0d);
        for (int corner = 0; corner < 4; corner++) {
            Vector256<double> sample = taps.Sample((x + (2 * (corner & 1)) - 1d, y + (corner & 2) - 1d),
                (WrapMode.Clamp, WrapMode.Clamp), Vector256<double>.Zero, static (sum, pixel, factor) => sum + (factor * Vector256.Create(pixel.X, pixel.Y, pixel.Z, pixel.W)));
            Vector4 tap = Vector256.Narrow(sample, Vector256<double>.Zero).GetLower().AsVector4();
            (sum, total) = weight(tap) switch { var w => (sum + (w * sample), total + w) };
        }
        return sum / total;
    }
}

[SmartEnum<string>]
[ValidationError<InvalidOptics>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class BloomParameter : IStateParameter<Bloom> {
    public static readonly BloomParameter Size = new("size", StateParameter.Bounded<Bloom, AxisFraction, float, InvalidGrade>(
        Lens<Bloom, AxisFraction>.New(static bloom => bloom.Size, static size => bloom => bloom with { Size = size }), new()));
    public static readonly BloomParameter Strength = new("strength", StateParameter.Bounded<Bloom, Mix, float, InvalidGrade>(
        Lens<Bloom, Mix>.New(static bloom => bloom.Strength, static strength => bloom => bloom with { Strength = strength }), new()));
    public static readonly BloomParameter Dirt = new("dirt", new StateParameter<Bloom>.Loaded<ImageFile, ImagePath, string, InvalidFileValue>(
        Lens<Bloom, Option<ImageFile>>.New(static bloom => bloom.Dirt, static dirt => bloom => bloom with { Dirt = dirt }), ImageFile.Load, static file => file.Path));
    public static readonly BloomParameter Specks = new("specks", new StateParameter<Bloom>.OptionalRecord<DirtSpecks>(
        Lens<Bloom, Gated<DirtSpecks>>.New(static bloom => bloom.Specks, static specks => bloom => bloom with { Specks = specks })));
    public static readonly BloomParameter DirtGain = new("dirt-gain", StateParameter.Bounded<Bloom, DirtGain, float, InvalidOptics>(
        Lens<Bloom, DirtGain>.New(static bloom => bloom.DirtGain, static gain => bloom => bloom with { DirtGain = gain }), new() { Soft = (Optics.DirtGain.MinValue, Optics.DirtGain.Unity) }));

    public StateParameter<Bloom> Kind { get; }
}

public sealed unsafe record VeilingGlare(AxisFraction Size) : IStateRecord<VeilingGlare, VeilingGlareParameter, InvalidOptics>, IPixelStage<VeilingGlare> {
    public static VeilingGlare Default { get; } = new(AxisFraction.Half);

    public static Option<PixelPass> Pass(VeilingGlare state, PassContext context) =>
        Some<PixelPass>(new PixelPass.Frame((frame, progress) => frame.Spread(Planes.Light,
            light => context.Derivations.Derived((state, context.Extent, frame.Size), static _ => new PointSpread([0, 0, 0]), Spread, spread => spread.Convolve(light)),
            BlendingMode.Mix, 1f, progress)));

    private static void Spread((VeilingGlare State, PixelExtent Extent, PixelExtent Size) key, PointSpread spread) {
        using Mat kernel = spread[0];
        kernel.Create((2 * key.Size.Height) - 1, (2 * key.Size.Width) - 1, DepthType.Cv32F, 1);
        Span<float> cells = new((void*)kernel.DataPointer, kernel.Rows * kernel.Cols);
        double perPixel = double.Lerp(180d, 10d, double.Cbrt(key.State.Size)) / int.Max(key.Extent.Width, key.Extent.Height);
        for (int i = 0; i < cells.Length; i++)
            cells[i] = (float)Spencer(perPixel * int.DivRem(i, kernel.Cols) switch { var (y, x) => double.Hypot(x - (key.Size.Width - 1), y - (key.Size.Height - 1)) });
        kernel.ConvertTo(kernel, DepthType.Cv32F, 1d / CvInvoke.Sum(kernel).V0);
        static double Spencer(double degrees) =>
            (0.384d * 2.61e6d * double.Exp(-double.Pow(degrees / 0.02d, 2d))) + (0.478d * 20.91d / double.Pow(degrees + 0.02d, 3d)) + (0.138d * 72.37d / double.Pow(degrees + 0.02d, 2d));
    }
}

[SmartEnum<string>]
[ValidationError<InvalidOptics>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class VeilingGlareParameter : IStateParameter<VeilingGlare> {
    public static readonly VeilingGlareParameter Size = new("size", StateParameter.Bounded<VeilingGlare, AxisFraction, float, InvalidGrade>(
        Lens<VeilingGlare, AxisFraction>.New(static glare => glare.Size, static size => glare => glare with { Size = size }), new()));

    public StateParameter<VeilingGlare> Kind { get; }
}

public sealed unsafe record StarGlare(Iris Iris, ShortSideLength Radius) : IStateRecord<StarGlare, StarGlareParameter, InvalidOptics>, IPixelStage<StarGlare> {
    public static StarGlare Default { get; } = new(Iris.Hexagon, ShortSideLength.Create(2f / ReferenceFrame.Height));

    public static Option<PixelPass> Pass(StarGlare state, PassContext context) =>
        state.Radius == ShortSideLength.Neutral || state.Iris.Obstruction == AxisFraction.MaxValue ? None
            : Some<PixelPass>(new PixelPass.Frame((frame, progress) => frame.Spread(Planes.Light,
                light => context.Derivations.Derived((state, context.Extent, frame.Size, context.Working), static _ => new PointSpread([0, 1, 2]), Spread, spread => spread.Convolve(light)),
                BlendingMode.Mix, 1f, progress)));

    private static void Spread((StarGlare State, PixelExtent Extent, PixelExtent Size, Gamut Working) key, PointSpread spread) {
        const decimal reference = 550m;
        const decimal airy = 1.22m;
        Arr<(float Wavelength, Vector3 Matching)> rows = StandardObserver.Matching(key.Working);
        Vector3 total = rows.Fold(Vector3.Zero, static (sum, row) => sum + row.Matching);
        Iris pupil = key.State.Iris with { Fringe = ColorFringe.Neutral };
        int grid = 2 * CvInvoke.GetOptimalDFTSize(int.Max(key.Extent.Width, key.Extent.Height));
        (int middle, double half) = (grid / 2, grid / (2d * pupil.Circumradius));
        Size size = new((2 * key.Size.Width) - 1, (2 * key.Size.Height) - 1);
        Point centre = new(key.Size.Width - 1, key.Size.Height - 1);
        using Mat energy = new(grid, grid, DepthType.Cv64F, 1);
        using Mat spectrum = new();
        using Mat power = new();
        using Mat dilated = new(size.Height, size.Width, DepthType.Cv64F, 1);
        using Mat normalized = new();
        Func<(double X, double Y), double> transmission = pupil.Transmission(half);
        for (int y = 0; y < grid; y++) {
            Span<double> cells = new((void*)energy.GetDataPointer(y), grid);
            for (int x = 0; x < cells.Length; x++)
                cells[x] = (1 - (2 * ((x + y) & 1))) * transmission((x - middle + 0.5d, y - middle + 0.5d));
        }
        CvInvoke.Dft(energy, spectrum, DxtType.ComplexOutput, 0);
        CvInvoke.MulSpectrums(spectrum, spectrum, power, MulSpectrumsType.Default, conjB: true);
        CvInvoke.ExtractChannel(power, energy, 0);
        for (int component = 0; component < spread.Size; component++) {
            using Mat plane = spread[component];
            plane.Create(size.Height, size.Width, DepthType.Cv32F, 1);
            plane.SetTo(default(MCvScalar));
        }
        ReadOnlySpan2D<double> samples = new((void*)energy.DataPointer, grid, grid, 0);
        Span<double> light = new((void*)dilated.DataPointer, size.Width * size.Height);
        foreach ((float wavelength, Vector3 matching) in rows) {
            ERational step = ERational.FromDecimal(reference * airy).Multiply(ERational.FromSingle(pupil.Circumradius))
                .Divide(ERational.FromSingle(key.State.Radius)).Divide(key.Extent.ShortSide).Divide(ERational.FromSingle(wavelength)).Divide(grid);
            (double Phase, double Footprint)[] across = [.. Enumerable.Range(0, size.Width).Select(x => Cell(step, x - centre.X))];
            (double Phase, double Footprint)[] down = [.. Enumerable.Range(0, size.Height).Select(y => Cell(step, y - centre.Y))];
            for (int i = 0; i < light.Length; i++) {
                (int y, int x) = int.DivRem(i, size.Width);
                double cell = across[x].Footprint * down[y].Footprint;
                light[i] = cell * cell * samples.Sample(
                    (middle + (grid * across[x].Phase) + 0.5d, middle + (grid * down[y].Phase) + 0.5d),
                    (WrapMode.Periodic, WrapMode.Periodic), 0d, static (sum, pixel, weight) => sum + (pixel * weight));
            }
            dilated.ConvertTo(normalized, DepthType.Cv32F, 1d / CvInvoke.Sum(dilated).V0);
            for (int component = 0; component < 3; component++) {
                using Mat plane = spread[component];
                CvInvoke.ScaleAdd(normalized, (double)matching[component] / total[component], plane, plane);
            }
        }
        static (double Phase, double Footprint) Cell(ERational step, int offset) {
            if (offset == 0)
                return (0d, 1d);
            ERational frequency = step.Multiply(int.Abs(offset));
            ERational centre = ERational.Create(1, 2);
            double phase = frequency.Add(centre).Remainder(ERational.One).Subtract(centre).ToDouble();
            return (offset < 0 ? -phase : phase, double.SinPi(phase) / (double.Pi * frequency.ToDouble()));
        }
    }
}

[SmartEnum<string>]
[ValidationError<InvalidOptics>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class StarGlareParameter : IStateParameter<StarGlare> {
    private static readonly (StateParameter<StarGlare> Blades, StateParameter<StarGlare> Rotation, StateParameter<StarGlare> Roundness, StateParameter<StarGlare> Obstruction, StateParameter<StarGlare> Squeeze)
        Outline = Iris.Kinds(Lens<StarGlare, Iris>.New(static glare => glare.Iris, static iris => glare => glare with { Iris = iris }));

    public static readonly StarGlareParameter Blades = new("blades", Outline.Blades);
    public static readonly StarGlareParameter Rotation = new("rotation", Outline.Rotation);
    public static readonly StarGlareParameter Roundness = new("roundness", Outline.Roundness);
    public static readonly StarGlareParameter Obstruction = new("obstruction", Outline.Obstruction);
    public static readonly StarGlareParameter Squeeze = new("squeeze", Outline.Squeeze);
    public static readonly StarGlareParameter AiryRadius = new("airy-radius", StateParameter.Bounded<StarGlare, ShortSideLength, float, InvalidPixelValue>(
        Lens<StarGlare, ShortSideLength>.New(static glare => glare.Radius, static radius => glare => glare with { Radius = radius }), ShortSideLength.Presentation with { Soft = (1.25f / ReferenceFrame.Height, 64f / ReferenceFrame.Height) }));

    public StateParameter<StarGlare> Kind { get; }
}

public sealed unsafe record Ghosts(HighlightKey Key, GhostIterations Iterations, Mix Modulation) : IStateRecord<Ghosts, GhostsParameter, InvalidOptics>, IPixelStage<Ghosts> {
    private static readonly Arr<Vector3> Modulated = [Vector3.Zero, new(0f, 1f, 1f), new(1f, 1f, 0f), new(1f, 0f, 1f)];

    public static Ghosts Default { get; } = new(new HighlightKey(HighlightThreshold.White, AxisFraction.Create(0.1f)), GhostIterations.Three, Mix.Create(0.25f));

    public static Option<PixelPass> Pass(Ghosts state, PassContext context) =>
        Some<PixelPass>(new PixelPass.Frame((frame, progress) => frame.Spread(state.Key.Extract, key => {
            float radius = 16f / ReferenceFrame.Height * frame.Extent.ShortSide;
            using Mat big = new();
            using Mat input = Mat.Zeros(key.Rows, key.Cols, DepthType.Cv32F, 4);
            return key.Blur(big, 2f * radius).Bind(_ => key.Blur(key, radius)).Map(_ => {
                Accumulate(key, input, frame, [(2.13f, Vector4.One)]);
                Accumulate(big, input, frame, [(-0.97f, Vector4.One)]);
                key.SetTo(default(MCvScalar));
                for (int i = 1; i < state.Iterations; i++) {
                    Accumulate(input, key, frame, Copies(i, state.Iterations, state.Modulation));
                    key.CopyTo(input);
                }
                return unit;
            });
        }, BlendingMode.Add, 1f, progress)));

    internal static (float Scale, Vector4 Gain)[] Copies(int iteration, int count, float modulation) =>
        [.. Enumerable.Range(0, Modulated.Count).Select(k => (
            (2.1f * (1f - (((4 * iteration) + k + (count % 2 == 1 ? 0.5f : 0f)) / (4f * count)))) switch { var s => k % 2 == 1 ? -0.99f / s : s },
            new Vector4((Vector3.One - (Modulated[k] * modulation)) / Modulated.Count, 0f)))];

    private static void Accumulate(Mat source, Mat target, PixelFrame frame, ReadOnlySpan<(float Scale, Vector4 Gain)> copies) {
        ReadOnlySpan2D<Vector4> from = new((void*)source.DataPointer, source.Rows, source.Cols, 0);
        Span<Vector4> to = new((void*)target.DataPointer, frame.Block.Length / 4);
        (double x, double y) = (frame.Extent.Width / 2d, frame.Extent.Height / 2d);
        for (int n = 0; n < to.Length; n++) {
            (double u, double v) = int.DivRem(n, target.Cols) switch { var (row, column) => ((double)frame.Origin.X + column + 0.5d - x, (double)frame.Origin.Y + row + 0.5d - y) };
            double reach = double.Hypot(u / x, v / y);
            foreach ((float scale, Vector4 gain) in copies)
                to[n] += from.Sample((x + (u * scale), y + (v * scale)), (WrapMode.Black, WrapMode.Black), Vector4.Zero,
                    static (sum, pixel, weight) => sum + ((float)weight * pixel), Some((frame.Origin, frame.Extent))) * gain * (float)double.Max(0d, 1d - (reach * float.Abs(scale)));
        }
    }
}

[SmartEnum<string>]
[ValidationError<InvalidOptics>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class GhostsParameter : IStateParameter<Ghosts> {
    private static readonly (StateParameter<Ghosts> Threshold, StateParameter<Ghosts> Softness) Highlight =
        HighlightKey.Kinds(Lens<Ghosts, HighlightKey>.New(static ghosts => ghosts.Key, static key => ghosts => ghosts with { Key = key }));

    public static readonly GhostsParameter Threshold = new("threshold", Highlight.Threshold);
    public static readonly GhostsParameter Softness = new("softness", Highlight.Softness);
    public static readonly GhostsParameter Iterations = new("iterations", StateParameter.Bounded<Ghosts, GhostIterations, int, InvalidOptics>(
        Lens<Ghosts, GhostIterations>.New(static ghosts => ghosts.Iterations, static iterations => ghosts => ghosts with { Iterations = iterations }), new()));
    public static readonly GhostsParameter ColorModulation = new("color-modulation", StateParameter.Bounded<Ghosts, Mix, float, InvalidGrade>(
        Lens<Ghosts, Mix>.New(static ghosts => ghosts.Modulation, static modulation => ghosts => ghosts with { Modulation = modulation }), new()));

    public StateParameter<Ghosts> Kind { get; }
}

public sealed record Halation(HighlightKey Key, ShortSideLength Radius, Swatch Color) : IStateRecord<Halation, HalationParameter, InvalidOptics>, IPixelStage<Halation> {
    public static Halation Default { get; } = new(new HighlightKey(HighlightThreshold.OneStopOver, AxisFraction.MaxValue), ShortSideLength.Create(64f * 0.2f / ReferenceFrame.Height), Swatch.Create(0xFF5900));

    public static Option<PixelPass> Pass(Halation state, PassContext context) =>
        Some<PixelPass>(new PixelPass.Frame((frame, progress) => new Vector4(state.Color.SceneLight(context.Working), 0f) switch {
            var color => frame.Spread(pixel => state.Key.Extract(pixel) * color, key => key.Blur(key, state.Radius.Pixels(frame.Extent)), BlendingMode.Add, 1f, progress),
        }));
}

[SmartEnum<string>]
[ValidationError<InvalidOptics>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class HalationParameter : IStateParameter<Halation> {
    private static readonly (StateParameter<Halation> Threshold, StateParameter<Halation> Softness) Highlight =
        HighlightKey.Kinds(Lens<Halation, HighlightKey>.New(static halation => halation.Key, static key => halation => halation with { Key = key }));

    public static readonly HalationParameter Threshold = new("threshold", Highlight.Threshold);
    public static readonly HalationParameter Softness = new("softness", Highlight.Softness);
    public static readonly HalationParameter Radius = new("radius", StateParameter.Bounded<Halation, ShortSideLength, float, InvalidPixelValue>(
        Lens<Halation, ShortSideLength>.New(static halation => halation.Radius, static radius => halation => halation with { Radius = radius }), ShortSideLength.Presentation with { Soft = (ShortSideLength.MinValue, 64f / ReferenceFrame.Height) }));
    public static readonly HalationParameter Color = new("color", new StateParameter<Halation>.Color(
        Lens<Halation, Swatch>.New(static halation => halation.Color, static color => halation => halation with { Color = color })));

    public StateParameter<Halation> Kind { get; }
}

// --- [SERVICES] ------------------------------------------------------------------------
internal sealed class PointSpread(Arr<int> planes) : VectorOfMat(planes.Fold(0, int.Max) + 1) {
    public Fin<Unit> Convolve(Mat light) => toSeq(Range(0, planes.Count)).Traverse(component => {
        using Mat lane = new();
        using Mat kernel = this[planes[component]];
        CvInvoke.ExtractChannel(light, lane, component);
        return lane.Correlate(lane, kernel, DepthType.Cv32F, PaddingModes.Constant).Map(_ => {
            CvInvoke.InsertChannel(lane, light, component);
            return unit;
        });
    }).As().Map(static _ => unit);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
file static unsafe class Planes {
    private static readonly Progress<int> Unreported = new();

    public static Vector4 Light(Vector4 pixel) => new(pixel.AsVector3() * pixel.W, 0f);

    extension(Mat plane) {
        public Fin<Unit> Blur(Mat target, double radius) => PixelSampling.KernelTaps<long>(radius).Bind(taps =>
            plane.Gaussian(target, (taps, taps), radius / 3d, radius / 3d, PaddingModes.Replicate));
    }

    extension(PixelFrame frame) {
        public Fin<Unit> Spread(Func<Vector4, Vector4> fill, Func<Mat, Fin<Unit>> spread, BlendingMode mode, float weight, IProgress<int> progress) => Try.lift(() => {
            using Mat light = new(frame.Size.Height, frame.Size.Width, DepthType.Cv32F, 4);
            return new PixelPass.Pointwise((row, _, line) => {
                Span<Vector4> plane = Lane(line);
                for (int x = 0; x < row.Length; x++)
                    plane[x] = fill(row[x]);
            }).Run(frame, Unreported).Bind(_ => spread(light)).Bind(_ => new PixelPass.Pointwise((row, _, line) => {
                Span<Vector4> layer = Lane(line);
                for (int x = 0; x < layer.Length; x++)
                    layer[x] = new(row[x].W > 0f ? layer[x].AsVector3() / row[x].W : Vector3.Zero, 0f);
                using SpanOwner<float> weights = SpanOwner<float>.Allocate(row.Length);
                weights.Span.Fill(weight);
                mode.Mixed(row, layer, weights.Span);
                layer.CopyTo(row);
            }).Run(frame, progress));

            Span<Vector4> Lane(long line) => new((void*)light.GetDataPointer(frame.RowIndex(line)), frame.Size.Width);
        }).Run();
    }
}
