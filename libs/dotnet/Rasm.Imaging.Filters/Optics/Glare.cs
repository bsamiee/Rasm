using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Rasm.Imaging.ColorManagement;
using Rasm.Imaging.Filters.Generators;
using Rasm.Imaging.Grade;
using Rasm.Imaging.Pixels;
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

    public (float Reach, float Edge) Outline(Vector2 q) {
        (int n, double kappa, double x, double y) = (Blades, Roundness, Squeeze * q.X, q.Y);
        (double s1, double c1) = double.SinCos(double.Pi / n);
        (double sa, double ca) = double.SinCos(double.Ieee754Remainder(double.Atan2(-y, x) - Rotation - (double.Pi / n), double.Tau / n));
        double root = double.Sqrt(1d - (kappa * kappa * s1 * s1));
        double delta = root - (kappa * c1);
        double edge = Circumradius * ((2d * c1 * root) - (kappa * (1d - (2d * s1 * s1)))) / ((delta * ca) + double.Sqrt(1d - (delta * delta * sa * sa)));
        return ((float)(double.Hypot(x, y) / edge), (float)edge);
    }

    public Vector4 Weights(float reach, float edge, float radius, float ramp) {
        (Vector3 r, float slope) = (radius * new Vector3(1f - float.Max(Fringe, 0f), 1f - (float.Abs(Fringe) / 2f), 1f + float.Min(Fringe, 0f)), edge / ramp);
        Vector3 w = Vector3.Min(Vector3.Clamp((slope * (r - new Vector3(reach))) + new Vector3(0.5f), Vector3.Zero, Vector3.One), Vector3.Clamp(Vector3.One + (slope * (new Vector3(reach) - (Obstruction * r))), Vector3.Zero, Vector3.One));
        return new Vector4(w, (w.X + w.Y + w.Z) / 3f);
    }

    internal static (StateParameter<TRecord> Blades, StateParameter<TRecord> Rotation, StateParameter<TRecord> Roundness, StateParameter<TRecord> Obstruction, StateParameter<TRecord> Squeeze)
        Kinds<TRecord>(Lens<TRecord, Iris> iris) =>
        (new StateParameter<TRecord>.Bounded<ApertureBlades, int, InvalidOptics>(lens(iris, Lens<Iris, ApertureBlades>.New(static i => i.Blades, static blades => i => i with { Blades = blades })), new()),
         new StateParameter<TRecord>.Bounded<SignedAngle, float, InvalidPixelValue>(lens(iris, Lens<Iris, SignedAngle>.New(static i => i.Rotation, static rotation => i => i with { Rotation = rotation })), SignedAngle.Presentation),
         new StateParameter<TRecord>.Bounded<AxisFraction, float, InvalidGrade>(lens(iris, Lens<Iris, AxisFraction>.New(static i => i.Roundness, static roundness => i => i with { Roundness = roundness })), new()),
         new StateParameter<TRecord>.Bounded<AxisFraction, float, InvalidGrade>(lens(iris, Lens<Iris, AxisFraction>.New(static i => i.Obstruction, static obstruction => i => i with { Obstruction = obstruction })), new()),
         new StateParameter<TRecord>.Bounded<AnamorphicSqueeze, float, InvalidOptics>(lens(iris, Lens<Iris, AnamorphicSqueeze>.New(static i => i.Squeeze, static squeeze => i => i with { Squeeze = squeeze })), new()));

    internal static StateParameter<TRecord> FringeKind<TRecord>(Lens<TRecord, Iris> iris) =>
        new StateParameter<TRecord>.Bounded<ColorFringe, float, InvalidOptics>(lens(iris, Lens<Iris, ColorFringe>.New(static i => i.Fringe, static fringe => i => i with { Fringe = fringe })), new() { Origin = (float)ColorFringe.Neutral });
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
        (new StateParameter<TRecord>.Bounded<HighlightThreshold, float, InvalidOptics>(lens(key, Lens<HighlightKey, HighlightThreshold>.New(static k => k.Threshold, static threshold => k => k with { Threshold = threshold })), new()),
         new StateParameter<TRecord>.Bounded<AxisFraction, float, InvalidGrade>(lens(key, Lens<HighlightKey, AxisFraction>.New(static k => k.Softness, static softness => k => k with { Softness = softness })), new()));
}

internal sealed class PointSpread(Size size, Arr<int> planes) {
    private readonly float[] block = GC.AllocateArray<float>(size.Width * size.Height * (planes.Fold(0, int.Max) + 1), pinned: true);

    public Mat Plane(int index) =>
        new(size.Height, size.Width, DepthType.Cv32F, 1, Marshal.UnsafeAddrOfPinnedArrayElement(block, index * size.Width * size.Height), size.Width * sizeof(float));

    public void Convolve(Mat light) {
        using Mat lane = new();
        for (int component = 0; component < planes.Count; component++) {
            using Mat kernel = Plane(planes[component]);
            CvInvoke.ExtractChannel(light, lane, component);
            CvInvoke.Filter2D(lane, lane, kernel, new Point(-1, -1), 0d, BorderType.Constant);
            CvInvoke.InsertChannel(lane, light, component);
        }
    }
}

public sealed record DirtSpecks(DirtCoverage Coverage, ShortSideExtent Scale, Seed Seed) : IStateRecord<DirtSpecks, DirtSpecksParameter, InvalidOptics> {
    private const int Grid = 1024;
    private static readonly PixelExtent Square = Valid.Value(PixelExtent.Validate(Grid, Grid, out PixelExtent square), square);

    public static DirtSpecks Default { get; } = new(DirtCoverage.Fifth, ShortSideExtent.Create(0.05f * ReferenceFrame.GreaterSide), Seed.MinValue);

    internal static PixelFrame Field((DirtSpecks Specks, PixelExtent Extent) key) {
        static float Frequency(int index) => (index < Grid / 2 ? index : index - Grid) / (float)Grid;
        using Mat draw = new(Grid, Grid, DepthType.Cv32F, 1);
        using Mat pink = new(Grid, Grid, DepthType.Cv32F, 2);
        using Mat spectrum = new();
        uint field = CoordinateHash.Field(NoiseStream.LensDirt, key.Specks.Seed, 0u);
        float knee = int.Max(key.Extent.Width, key.Extent.Height) / (key.Specks.Scale.Pixels(key.Extent) * Grid);
        Span<float> cells = draw.GetSpan<float>();
        Span<Vector2> gains = pink.GetSpan<Vector2>();
        for (int i = 0; i < cells.Length; i++)
            (cells[i], gains[i]) = int.DivRem(i, Grid) switch { var (y, x) => (CoordinateHash.Normals(x, y, field).X, new Vector2(1f / new Vector3(Frequency(x), Frequency(y), knee).Length())) };
        CvInvoke.Dft(draw, spectrum, DxtType.ComplexOutput, 0);
        CvInvoke.Multiply(spectrum, pink, spectrum);
        CvInvoke.Dft(spectrum, draw, DxtType.Inverse | DxtType.RealOutput | DxtType.Scale, 0);
        draw.MinMax(out _, out double[] peaks, out _, out _);
        float[] sorted = draw.GetSpan<float>().ToArray();
        sorted.AsSpan().Sort();
        float rank = (1f - key.Specks.Coverage) * (sorted.Length - 1);
        (float floor, float peak) = (float.Lerp(sorted[(int)rank], sorted[(int)rank + 1], rank - (int)rank), (float)peaks[0]);
        return new PixelFrame(Point.Empty, Square, Square, block => {
            ReadOnlySpan<float> values = draw.GetSpan<float>();
            Span<Vector4> plane = MemoryMarshal.Cast<float, Vector4>(block.AsSpan());
            for (int i = 0; i < plane.Length; i++)
                plane[i] = new Vector4(float.Clamp((values[i] - floor) / (peak - floor), 0f, 1f));
        });
    }
}

[SmartEnum<string>]
[ValidationError<InvalidOptics>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class DirtSpecksParameter : IStateParameter<DirtSpecks> {
    public static readonly DirtSpecksParameter Coverage = new("coverage", new StateParameter<DirtSpecks>.Bounded<DirtCoverage, float, InvalidOptics>(
        Lens<DirtSpecks, DirtCoverage>.New(static specks => specks.Coverage, static coverage => specks => specks with { Coverage = coverage }), new()));
    public static readonly DirtSpecksParameter Scale = new("scale", new StateParameter<DirtSpecks>.Bounded<ShortSideExtent, float, InvalidPixelValue>(
        Lens<DirtSpecks, ShortSideExtent>.New(static specks => specks.Scale, static scale => specks => specks with { Scale = scale }),
        new() { Unit = Quantity.GetUnitInfo(RatioUnit.DecimalFraction), Soft = (0.001f * ReferenceFrame.GreaterSide, 0.5f * ReferenceFrame.GreaterSide) }));
    public static readonly DirtSpecksParameter Seed = new("seed", new StateParameter<DirtSpecks>.Bounded<Seed, int, InvalidGenerator>(
        Lens<DirtSpecks, Seed>.New(static specks => specks.Seed, static seed => specks => specks with { Seed = seed }), Generators.Seed.Presentation));

    public StateParameter<DirtSpecks> Kind { get; }
}

public sealed record Bloom(AxisFraction Size, Mix Strength, Option<ImageFile> Dirt, Gated<DirtSpecks> Specks, DirtGain DirtGain) : IStateRecord<Bloom, BloomParameter, InvalidOptics>, IPixelStage<Bloom> {
    public static Bloom Default { get; } = new(AxisFraction.Half, Mix.Create(0.7f * float.Pow(0.15f, 1.5f)), None, new(Enabled: false, DirtSpecks.Default), DirtGain.Unity);

    public static Option<PixelPass> Pass(Bloom state, PassContext context) =>
        state.Strength == Mix.MinValue ? None : Some<PixelPass>(new PixelPass.Frame((frame, progress) => frame.Spread(Planes.Light, light => {
            using Mat tent = new(1, 3, DepthType.Cv32F, 1);
            tent.SetTo([0.25f, 0.5f, 0.25f]);
            int chain = (int)float.Log2(float.Max(1f, frame.Size.ShortSide * state.Size));
            Chained(light, chain - 1, static tap => 1f / (1f + float.Max(tap.X, float.Max(tap.Y, tap.Z))), tent);
            light.ConvertTo(light, DepthType.Cv32F, 1d / int.Max(1, chain));
            _ = (state.Specks.Active.Map(specks => context.Derivations.Derived((specks, context.Extent), DirtSpecks.Field)) | state.Dirt.Map(static file => file.Frame)).Iter(dirt => {
                using Mat lens = dirt.Header();
                double cover = double.Max((double)frame.Extent.Width / lens.Cols, (double)frame.Extent.Height / lens.Rows);
                using Mat fitted = new();
                CvInvoke.Resize(lens, fitted, default, cover, cover, cover < 1d ? Inter.Area : Inter.Linear);
                using Mat soil = new(fitted, new Rectangle(((fitted.Cols - frame.Extent.Width) / 2) + frame.Origin.X, ((fitted.Rows - frame.Extent.Height) / 2) + frame.Origin.Y, light.Cols, light.Rows));
                soil.ConvertTo(soil, DepthType.Cv32F, state.DirtGain, 1d);
                CvInvoke.Multiply(light, soil, light);
            });
        }, BlendingMode.Mix, state.Strength, progress)));

    private static void Chained(Mat level, int halvings, Func<Vector4, float> weight, Mat tent) {
        if (halvings <= 0)
            return;
        using Mat box = new();
        using Mat next = new(level.Rows / 2, level.Cols / 2, DepthType.Cv32F, 4);
        using Mat up = new();
        CvInvoke.BoxFilter(level, box, DepthType.Default, new Size(2, 2), Point.Empty, normalize: true, BorderType.Replicate);
        ReadOnlySpan2D<Vector4> taps = box.GetSpan<Vector4>().AsSpan2D(box.Rows, box.Cols);
        Span<Vector4> cells = next.GetSpan<Vector4>();
        for (int n = 0; n < cells.Length; n++) {
            (int y, int x) = int.DivRem(n, next.Cols);
            cells[n] = ((4f * Group(taps, 2 * x, 2 * y, weight)) + Group(taps, (2 * x) - 1, (2 * y) - 1, weight) + Group(taps, (2 * x) + 1, (2 * y) - 1, weight)
                + Group(taps, (2 * x) - 1, (2 * y) + 1, weight) + Group(taps, (2 * x) + 1, (2 * y) + 1, weight)) / 8f;
        }
        Chained(next, halvings - 1, static _ => 1f, tent);
        CvInvoke.Resize(next, up, level.Size, 0d, 0d, Inter.Linear);
        CvInvoke.SepFilter2D(up, up, DepthType.Cv32F, tent, tent, new Point(-1, -1), 0d, BorderType.Replicate);
        CvInvoke.Add(level, up, level);
    }

    private static Vector4 Group(ReadOnlySpan2D<Vector4> taps, int x, int y, Func<Vector4, float> weight) {
        (Vector4 sum, float total) = (Vector4.Zero, 0f);
        for (int corner = 0; corner < 4; corner++) {
            Vector4 tap = taps[int.Clamp(y + (corner & 2) - 1, 0, taps.Height - 1), int.Clamp(x + (2 * (corner & 1)) - 1, 0, taps.Width - 1)];
            (sum, total) = weight(tap) switch { var w => (sum + (w * tap), total + w) };
        }
        return sum / total;
    }
}

[SmartEnum<string>]
[ValidationError<InvalidOptics>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class BloomParameter : IStateParameter<Bloom> {
    public static readonly BloomParameter Size = new("size", new StateParameter<Bloom>.Bounded<AxisFraction, float, InvalidGrade>(
        Lens<Bloom, AxisFraction>.New(static bloom => bloom.Size, static size => bloom => bloom with { Size = size }), new()));
    public static readonly BloomParameter Strength = new("strength", new StateParameter<Bloom>.Bounded<Mix, float, InvalidGrade>(
        Lens<Bloom, Mix>.New(static bloom => bloom.Strength, static strength => bloom => bloom with { Strength = strength }), new()));
    public static readonly BloomParameter Dirt = new("dirt", new StateParameter<Bloom>.Loaded<ImageFile, ImagePath, string, InvalidPixelValue>(
        Lens<Bloom, Option<ImageFile>>.New(static bloom => bloom.Dirt, static dirt => bloom => bloom with { Dirt = dirt }), ImageFile.Load, static file => file.Path));
    public static readonly BloomParameter Specks = new("specks", new StateParameter<Bloom>.OptionalRecord<DirtSpecks>(
        Lens<Bloom, Gated<DirtSpecks>>.New(static bloom => bloom.Specks, static specks => bloom => bloom with { Specks = specks })));
    public static readonly BloomParameter DirtGain = new("dirt-gain", new StateParameter<Bloom>.Bounded<DirtGain, float, InvalidOptics>(
        Lens<Bloom, DirtGain>.New(static bloom => bloom.DirtGain, static gain => bloom => bloom with { DirtGain = gain }), new() { Soft = (Optics.DirtGain.MinValue, Optics.DirtGain.Unity) }));

    public StateParameter<Bloom> Kind { get; }
}

public sealed record VeilingGlare(AxisFraction Size) : IStateRecord<VeilingGlare, VeilingGlareParameter, InvalidOptics>, IPixelStage<VeilingGlare> {
    public static VeilingGlare Default { get; } = new(AxisFraction.Half);

    public static Option<PixelPass> Pass(VeilingGlare state, PassContext context) =>
        Some<PixelPass>(new PixelPass.Frame((frame, progress) => frame.Spread(Planes.Light, context.Derivations.Derived((state, context.Extent), Spread).Convolve, BlendingMode.Mix, 1f, progress)));

    private static PointSpread Spread((VeilingGlare State, PixelExtent Extent) key) {
        PointSpread spread = new(new Size((2 * key.Extent.Width) - 1, (2 * key.Extent.Height) - 1), [0, 0, 0]);
        using Mat kernel = spread.Plane(0);
        Span<float> cells = kernel.GetSpan<float>();
        double perPixel = double.Lerp(180d, 10d, double.Cbrt(key.State.Size)) / int.Max(key.Extent.Width, key.Extent.Height);
        for (int i = 0; i < cells.Length; i++)
            cells[i] = (float)Spencer(perPixel * int.DivRem(i, kernel.Cols) switch { var (y, x) => double.Hypot(x - (key.Extent.Width - 1), y - (key.Extent.Height - 1)) });
        kernel.ConvertTo(kernel, DepthType.Cv32F, 1d / CvInvoke.Sum(kernel).V0);
        return spread;

        static double Spencer(double degrees) =>
            (0.384d * 2.61e6d * double.Exp(-double.Pow(degrees / 0.02d, 2d))) + (0.478d * 20.91d / double.Pow(degrees + 0.02d, 3d)) + (0.138d * 72.37d / double.Pow(degrees + 0.02d, 2d));
    }
}

[SmartEnum<string>]
[ValidationError<InvalidOptics>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class VeilingGlareParameter : IStateParameter<VeilingGlare> {
    public static readonly VeilingGlareParameter Size = new("size", new StateParameter<VeilingGlare>.Bounded<AxisFraction, float, InvalidGrade>(
        Lens<VeilingGlare, AxisFraction>.New(static glare => glare.Size, static size => glare => glare with { Size = size }), new()));

    public StateParameter<VeilingGlare> Kind { get; }
}

public sealed record StarGlare(Iris Iris, ShortSideLength Radius) : IStateRecord<StarGlare, StarGlareParameter, InvalidOptics>, IPixelStage<StarGlare> {
    private const int PupilGrid = 4096;
    private const float Crop = 1e-9f;
    private const float Reference = 550f;

    public static StarGlare Default { get; } = new(Iris.Hexagon, ShortSideLength.Create(2f / ReferenceFrame.Height));

    public static Option<PixelPass> Pass(StarGlare state, PassContext context) =>
        Some<PixelPass>(new PixelPass.Frame((frame, progress) => frame.Spread(Planes.Light, context.Derivations.Derived((state, context.Extent, context.Working), Spread).Convolve, BlendingMode.Mix, 1f, progress)));

    private static PointSpread Spread((StarGlare State, PixelExtent Extent, Gamut Working) key) {
        Arr<(float Wavelength, Vector3 Matching)> rows = StandardObserver.Matching(key.Working);
        Vector3 total = rows.Fold(Vector3.Zero, static (sum, row) => sum + row.Matching);
        int grid = int.Min((int)BitOperations.RoundUpToPowerOf2((uint)(2 * int.Max(key.Extent.Width, key.Extent.Height))), PupilGrid);
        (int middle, float half) = (grid / 2, 1.22f * grid / (2f * key.State.Radius.Pixels(key.Extent)));
        using Mat energy = new(grid, grid, DepthType.Cv32F, 1);
        using Mat spectrum = new();
        using Mat power = new();
        using Mat held = new();
        using Mat dilated = new();
        Span<float> cells = energy.GetSpan<float>();
        for (int i = 0; i < cells.Length; i++) {
            (int y, int x) = int.DivRem(i, grid);
            cells[i] = key.State.Iris.Outline(new Vector2(x - middle, y - middle)) switch { var (outline, edge) => (1 - (2 * ((x + y) & 1))) * key.State.Iris.Weights(outline, edge, half, 1f).W };
        }
        CvInvoke.Dft(energy, spectrum, DxtType.ComplexOutput, 0);
        CvInvoke.MulSpectrums(spectrum, spectrum, power, MulSpectrumsType.Default, conjB: true);
        CvInvoke.ExtractChannel(power, energy, 0);
        using ScalarArray floor = new(Crop * energy.GetSpan<float>()[(middle * grid) + middle]);
        CvInvoke.Compare(energy, floor, held, CmpType.GreaterEqual);
        Rectangle bound = CvInvoke.BoundingRectangle(held);
        Vector2 frame = new(key.Extent.Width - 1, key.Extent.Height - 1);
        Vector2 crop = Vector2.Min(new(bound.Right - 1 - middle, bound.Bottom - 1 - middle), Vector2.Round(frame * Reference / rows.Min(static row => row.Wavelength), MidpointRounding.ToPositiveInfinity));
        Vector2 reach = Vector2.Min(Vector2.Round(crop * rows.Max(static row => row.Wavelength) / Reference, MidpointRounding.ToPositiveInfinity), frame);
        PointSpread spread = new(Box(reach, reach).Size, [0, 1, 2]);
        using Mat region = new(energy, Box(new Vector2(middle), crop));
        foreach ((float wavelength, Vector3 matching) in rows) {
            Vector2 source = Vector2.Min(crop, Vector2.Round(reach * Reference / wavelength, MidpointRounding.ToPositiveInfinity));
            Vector2 target = Vector2.Min(Vector2.Round(source * wavelength / Reference, MidpointRounding.AwayFromZero), reach);
            using Mat taps = new(region, Box(crop, source));
            CvInvoke.Resize(taps, dilated, Box(target, target).Size, 0d, 0d, wavelength < Reference ? Inter.Area : Inter.Linear);
            Vector3 weights = matching / (total * (float)CvInvoke.Sum(dilated).V0);
            for (int component = 0; component < 3; component++) {
                using Mat plane = spread.Plane(component);
                using Mat window = new(plane, Box(reach, target));
                CvInvoke.ScaleAdd(dilated, weights[component], window, window);
            }
        }
        return spread;

        static Rectangle Box(Vector2 centre, Vector2 half) => new((int)(centre.X - half.X), (int)(centre.Y - half.Y), (2 * (int)half.X) + 1, (2 * (int)half.Y) + 1);
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
    public static readonly StarGlareParameter AiryRadius = new("airy-radius", new StateParameter<StarGlare>.Bounded<ShortSideLength, float, InvalidPixelValue>(
        Lens<StarGlare, ShortSideLength>.New(static glare => glare.Radius, static radius => glare => glare with { Radius = radius }), ShortSideLength.Presentation with { Soft = (1.25f / ReferenceFrame.Height, 64f / ReferenceFrame.Height) }));

    public StateParameter<StarGlare> Kind { get; }
}

public sealed record Ghosts(HighlightKey Key, GhostIterations Iterations, Mix Modulation) : IStateRecord<Ghosts, GhostsParameter, InvalidOptics>, IPixelStage<Ghosts> {
    private static readonly Arr<Vector3> Modulated = [Vector3.Zero, new(0f, 1f, 1f), new(1f, 1f, 0f), new(1f, 0f, 1f)];

    public static Ghosts Default { get; } = new(new HighlightKey(HighlightThreshold.White, AxisFraction.Create(0.1f)), GhostIterations.Three, Mix.Create(0.25f));

    public static Option<PixelPass> Pass(Ghosts state, PassContext context) =>
        Some<PixelPass>(new PixelPass.Frame((frame, progress) => frame.Spread(state.Key.Extract, key => {
            float radius = 16f / ReferenceFrame.Height * frame.Extent.ShortSide;
            using Mat big = new();
            using Mat input = Mat.Zeros(key.Rows, key.Cols, DepthType.Cv32F, 4);
            key.Blur(big, 2f * radius);
            key.Blur(key, radius);
            Accumulate(key, input, [(2.13f, Vector4.One)]);
            Accumulate(big, input, [(-0.97f, Vector4.One)]);
            key.SetTo(default(MCvScalar));
            for (int i = 1; i < state.Iterations; i++) {
                Accumulate(input, key, Copies(i, state.Iterations, state.Modulation));
                key.CopyTo(input);
            }
        }, BlendingMode.Add, 1f, progress)));

    internal static (float Scale, Vector4 Gain)[] Copies(int iteration, int count, float modulation) =>
        [.. Enumerable.Range(0, Modulated.Count).Select(k => (
            (2.1f * (1f - (((4 * iteration) + k + (count % 2 == 1 ? 0.5f : 0f)) / (4f * count)))) switch { var s => k % 2 == 1 ? -0.99f / s : s },
            new Vector4((Vector3.One - (Modulated[k] * modulation)) / Modulated.Count, 0f)))];

    private static void Accumulate(Mat source, Mat target, ReadOnlySpan<(float Scale, Vector4 Gain)> copies) {
        ReadOnlySpan2D<Vector4> from = source.GetSpan<Vector4>().AsSpan2D(source.Rows, source.Cols);
        Span<Vector4> to = target.GetSpan<Vector4>();
        Vector2 center = new Vector2(target.Cols, target.Rows) / 2f;
        for (int n = 0; n < to.Length; n++) {
            Vector2 offset = int.DivRem(n, target.Cols) switch { var (y, x) => new Vector2(x + 0.5f, y + 0.5f) - center };
            float reach = (offset / center).Length();
            foreach ((float scale, Vector4 gain) in copies)
                to[n] += from.Sample(center + (offset * scale), (WrapMode.Black, WrapMode.Black)) * gain * float.Max(0f, 1f - (reach * float.Abs(scale)));
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
    public static readonly GhostsParameter Iterations = new("iterations", new StateParameter<Ghosts>.Bounded<GhostIterations, int, InvalidOptics>(
        Lens<Ghosts, GhostIterations>.New(static ghosts => ghosts.Iterations, static iterations => ghosts => ghosts with { Iterations = iterations }), new()));
    public static readonly GhostsParameter ColorModulation = new("color-modulation", new StateParameter<Ghosts>.Bounded<Mix, float, InvalidGrade>(
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
    public static readonly HalationParameter Radius = new("radius", new StateParameter<Halation>.Bounded<ShortSideLength, float, InvalidPixelValue>(
        Lens<Halation, ShortSideLength>.New(static halation => halation.Radius, static radius => halation => halation with { Radius = radius }), ShortSideLength.Presentation with { Soft = (ShortSideLength.MinValue, 64f / ReferenceFrame.Height) }));
    public static readonly HalationParameter Color = new("color", new StateParameter<Halation>.Color(
        Lens<Halation, Swatch>.New(static halation => halation.Color, static color => halation => halation with { Color = color })));

    public StateParameter<Halation> Kind { get; }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
file static class Planes {
    private static readonly Progress<int> Unreported = new();

    public static Vector4 Light(Vector4 pixel) => new(pixel.AsVector3() * pixel.W, 0f);

    extension(Mat plane) {
        public void Blur(Mat target, float radius) {
            int taps = (2 * (int)float.Ceiling(radius)) + 1;
            CvInvoke.GaussianBlur(plane, target, new Size(taps, taps), radius / 3f, radius / 3f, BorderType.Replicate);
        }
    }

    extension(PixelFrame frame) {
        public Fin<Unit> Spread(Func<Vector4, Vector4> fill, Action<Mat> spread, BlendingMode mode, float weight, IProgress<int> progress) {
            using Mat light = new(frame.Size.Height, frame.Size.Width, DepthType.Cv32F, 4);
            return new PixelPass.Pointwise((row, _, line) => {
                Span<Vector4> plane = Lane(line);
                for (int x = 0; x < row.Length; x++)
                    plane[x] = fill(row[x]);
            }).Run(frame, Unreported).Bind(_ => {
                spread(light);
                return new PixelPass.Pointwise((row, _, line) => {
                    Span<Vector4> layer = Lane(line);
                    using SpanOwner<float> weights = SpanOwner<float>.Allocate(row.Length);
                    weights.Span.Fill(weight);
                    mode.Mixed(row, layer, weights.Span);
                    layer.CopyTo(row);
                }).Run(frame, progress);
            });

            Span<Vector4> Lane(int line) => light.GetSpan<Vector4>().AsSpan2D(light.Rows, light.Cols).GetRowSpan(frame.Line(line));
        }
    }
}
