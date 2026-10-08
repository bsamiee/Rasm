using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using CommunityToolkit.HighPerformance;
using Emgu.CV;
using Emgu.CV.CvEnum;
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
        ((int)Blades, (double)Roundness) switch {
            var (n, kappa) => double.Asin(kappa * double.Sin(double.Pi / n)) switch {
                var beta => (float)double.Sqrt(double.Pi / (n / 2d * (double.Sin(double.Tau / n) + (kappa == 0d ? 0d : ((2d * beta) - double.Sin(2d * beta)) / (kappa * kappa))))),
            },
        };

    public (float Reach, float Edge) Outline(Vector2 q) {
        (int n, double kappa, double x, double y) = (Blades, Roundness, Squeeze * q.X, q.Y);
        (double s1, double c1) = double.SinCos(double.Pi / n);
        double root = double.Sqrt(1d - (kappa * kappa * s1 * s1));
        double delta = root - (kappa * c1);
        (double sector, double turn) = (double.Tau / n, double.Atan2(-y, x) - Rotation);
        (double sa, double ca) = double.SinCos(turn - (sector * double.Floor(turn / sector)) - (double.Pi / n));
        double edge = Circumradius * ((2d * c1 * root) - (kappa * double.Cos(sector))) / ((delta * ca) + double.Sqrt(1d - (delta * delta * sa * sa)));
        return ((float)(double.Hypot(x, y) / edge), (float)edge);
    }

    public Vector4 Weights(float reach, float edge, float radius) {
        Vector3 scale = new(1f - float.Max(Fringe, 0f), 1f - (float.Abs(Fringe) / 2f), 1f + float.Min(Fringe, 0f));
        Vector3 w = Vector3.Min(
            Vector3.Clamp((edge * ((radius * scale) - new Vector3(reach))) + new Vector3(0.5f), Vector3.Zero, Vector3.One),
            Vector3.Clamp(Vector3.One + (edge * (new Vector3(reach) - (Obstruction * radius * scale))), Vector3.Zero, Vector3.One));
        return new Vector4(w, (w.X + w.Y + w.Z) / 3f);
    }

    internal static (StateParameter<TRecord> Blades, StateParameter<TRecord> Rotation, StateParameter<TRecord> Roundness, StateParameter<TRecord> Obstruction, StateParameter<TRecord> Squeeze)
        Kinds<TRecord>(Lens<TRecord, Iris> iris) =>
        (new StateParameter<TRecord>.Bounded<ApertureBlades, int, InvalidOptics>(lens(iris, Lens<Iris, ApertureBlades>.New(static i => i.Blades, static blades => i => i with { Blades = blades })), new()),
         new StateParameter<TRecord>.Bounded<SignedAngle, float, InvalidPixelValue>(
             lens(iris, Lens<Iris, SignedAngle>.New(static i => i.Rotation, static rotation => i => i with { Rotation = rotation })),
             new() { Unit = Quantity.GetUnitInfo(AngleUnit.Radian), Origin = (float)SignedAngle.Neutral }),
         new StateParameter<TRecord>.Bounded<AxisFraction, float, InvalidGrade>(lens(iris, Lens<Iris, AxisFraction>.New(static i => i.Roundness, static roundness => i => i with { Roundness = roundness })), new()),
         new StateParameter<TRecord>.Bounded<AxisFraction, float, InvalidGrade>(
             lens(iris, Lens<Iris, AxisFraction>.New(static i => i.Obstruction, static obstruction => i => i with { Obstruction = obstruction })), new()),
         new StateParameter<TRecord>.Bounded<AnamorphicSqueeze, float, InvalidOptics>(lens(iris, Lens<Iris, AnamorphicSqueeze>.New(static i => i.Squeeze, static squeeze => i => i with { Squeeze = squeeze })), new()));

    internal static StateParameter<TRecord> FringeKind<TRecord>(Lens<TRecord, Iris> iris) =>
        new StateParameter<TRecord>.Bounded<ColorFringe, float, InvalidOptics>(
            lens(iris, Lens<Iris, ColorFringe>.New(static i => i.Fringe, static fringe => i => i with { Fringe = fringe })), new() { Origin = (float)ColorFringe.Neutral });
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
        (new StateParameter<TRecord>.Bounded<HighlightThreshold, float, InvalidOptics>(
             lens(key, Lens<HighlightKey, HighlightThreshold>.New(static k => k.Threshold, static threshold => k => k with { Threshold = threshold })), new()),
         new StateParameter<TRecord>.Bounded<AxisFraction, float, InvalidGrade>(
             lens(key, Lens<HighlightKey, AxisFraction>.New(static k => k.Softness, static softness => k => k with { Softness = softness })), new()));
}

internal sealed class PointSpread(Size size, Arr<int> planes) {
    private readonly float[] block = GC.AllocateArray<float>(size.Width * size.Height * (planes.Fold(0, int.Max) + 1), pinned: true);

    public Mat Plane(int index) =>
        new(size.Height, size.Width, DepthType.Cv32F, 1, Marshal.UnsafeAddrOfPinnedArrayElement(block, index * size.Width * size.Height), size.Width * sizeof(float));

    public PointSpread Normalized() {
        foreach (int index in planes.Distinct()) {
            using Mat kernel = Plane(index);
            kernel.ConvertTo(kernel, DepthType.Cv32F, 1d / CvInvoke.Sum(kernel).V0);
        }
        return this;
    }

    public Fin<Unit> Convolved(PixelFrame frame, IProgress<int> progress) {
        using Mat light = new(frame.Size.Height, frame.Size.Width, DepthType.Cv32F, 4);
        using Mat plane = new();
        return light.Lit(frame, Planes.Light).Bind(_ => {
            for (int component = 0; component < planes.Count; component++) {
                using Mat kernel = Plane(planes[component]);
                CvInvoke.ExtractChannel(light, plane, component);
                CvInvoke.Filter2D(plane, plane, kernel, Planes.Centered, 0d, BorderType.Constant);
                CvInvoke.InsertChannel(plane, light, component);
            }
            return light.Mixed(frame, Mix.Full, progress);
        });
    }
}

[Union]
public abstract partial record LensDirt {
    internal abstract Mat Header(PassContext context);

    public sealed record Image(ImageFile File) : LensDirt {
        internal override Mat Header(PassContext context) => File.Frame.Header();
    }

    public sealed record Specks(DirtCoverage Coverage, ShortSideExtent Scale, Seed Seed) : LensDirt {
        private const int Grid = 1024;

        public static Specks Default { get; } = new(DirtCoverage.Fifth, ShortSideExtent.Create(0.05f * ReferenceFrame.GreaterSide), Seed.MinValue);

        internal override Mat Header(PassContext context) =>
            new(Grid, Grid, DepthType.Cv32F, 4, Marshal.UnsafeAddrOfPinnedArrayElement(context.Derivations.Derived((Specks: this, context.Extent), Field), 0), Grid * Unsafe.SizeOf<Vector4>());

        private static Vector4[] Field((Specks Specks, PixelExtent Extent) key) {
            static float Frequency(int index) => (index < Grid / 2 ? index : index - Grid) / (float)Grid;
            using Mat draw = new(Grid, Grid, DepthType.Cv32F, 1);
            using Mat pink = new(Grid, Grid, DepthType.Cv32F, 2);
            using Mat spectrum = new();
            uint field = CoordinateHash.Field(NoiseStream.LensDirt, key.Specks.Seed, 0u);
            float knee = int.Max(key.Extent.Width, key.Extent.Height) / (key.Specks.Scale.Pixels(key.Extent) * Grid);
            Span2D<float> cells = draw.GetSpan<float>().AsSpan2D(Grid, Grid);
            Span2D<Vector2> gains = pink.GetSpan<Vector2>().AsSpan2D(Grid, Grid);
            for (int y = 0; y < Grid; y++) {
                for (int x = 0; x < Grid; x++)
                    (cells[y, x], gains[y, x]) = (CoordinateHash.Normals(x, y, field).X, new Vector2(1f / new Vector3(Frequency(x), Frequency(y), knee).Length()));
            }
            CvInvoke.Dft(draw, spectrum, DxtType.ComplexOutput, 0);
            CvInvoke.Multiply(spectrum, pink, spectrum);
            CvInvoke.Dft(spectrum, draw, DxtType.Inverse | DxtType.RealOutput | DxtType.Scale, 0);
            draw.MinMax(out _, out double[] peaks, out _, out _);
            ReadOnlySpan<float> values = draw.GetSpan<float>();
            float[] sorted = values.ToArray();
            sorted.AsSpan().Sort();
            float rank = (1f - key.Specks.Coverage) * (sorted.Length - 1);
            float floor = float.Lerp(sorted[(int)rank], sorted[(int)rank + 1], rank - (int)rank);
            Vector4[] plane = GC.AllocateUninitializedArray<Vector4>(values.Length, pinned: true);
            for (int i = 0; i < plane.Length; i++)
                plane[i] = new Vector4(float.Clamp((values[i] - floor) / ((float)peaks[0] - floor), 0f, 1f));
            return plane;
        }
    }
}

public sealed record Bloom(AxisFraction Size, Mix Strength, Option<LensDirt> Dirt, DirtGain DirtGain) : IStateRecord<Bloom, BloomParameter, InvalidOptics>, IPixelStage<Bloom> {
    public static Bloom Default { get; } = new(AxisFraction.Half, Mix.Create(0.7f * float.Pow(0.15f, 1.5f)), None, DirtGain.Unity);

    public static Option<PixelPass> Pass(Bloom state, PassContext context) =>
        state.Strength == Mix.MinValue ? None : Some<PixelPass>(new PixelPass.Frame((frame, progress) => {
            using Mat light = new(frame.Size.Height, frame.Size.Width, DepthType.Cv32F, 4);
            using Mat tent = new(1, 3, DepthType.Cv32F, 1);
            tent.SetTo([0.25f, 0.5f, 0.25f]);
            int chain = (int)float.Log2(float.Max(1f, frame.Size.ShortSide * state.Size));
            return light.Lit(frame, Planes.Light).Bind(_ => {
                Chained(light, chain - 1, static tap => 1f / (1f + float.Max(tap.X, float.Max(tap.Y, tap.Z))), tent);
                light.ConvertTo(light, DepthType.Cv32F, 1d / int.Max(1, chain));
                _ = state.Dirt.Iter(dirt => {
                    using Mat lens = dirt.Header(context);
                    double cover = double.Max((double)light.Cols / lens.Cols, (double)light.Rows / lens.Rows);
                    using Mat fitted = new();
                    CvInvoke.Resize(lens, fitted, new Size((int)double.Ceiling(lens.Cols * cover), (int)double.Ceiling(lens.Rows * cover)), 0d, 0d, cover < 1d ? Inter.Area : Inter.Linear);
                    using Mat soil = new(fitted, new Rectangle((fitted.Cols - light.Cols) / 2, (fitted.Rows - light.Rows) / 2, light.Cols, light.Rows));
                    using Mat lit = new();
                    soil.ConvertTo(lit, DepthType.Cv32F, state.DirtGain, 1d);
                    CvInvoke.Multiply(light, lit, light);
                });
                return light.Mixed(frame, state.Strength, progress);
            });
        }));

    private static void Chained(Mat level, int halvings, Func<Vector4, float> weight, Mat tent) {
        if (halvings <= 0)
            return;
        using Mat box = new();
        using Mat next = new(level.Rows / 2, level.Cols / 2, DepthType.Cv32F, 4);
        using Mat up = new();
        CvInvoke.BoxFilter(level, box, DepthType.Default, new Size(2, 2), Point.Empty, normalize: true, BorderType.Replicate);
        ReadOnlySpan2D<Vector4> source = box.GetSpan<Vector4>().AsSpan2D(box.Rows, box.Cols);
        Span2D<Vector4> cells = next.GetSpan<Vector4>().AsSpan2D(next.Rows, next.Cols);
        for (int j = 0; j < cells.Height; j++) {
            for (int i = 0; i < cells.Width; i++)
                cells[j, i] = ((4f * Group(source, 2 * i, 2 * j, weight)) + Group(source, (2 * i) - 1, (2 * j) - 1, weight) + Group(source, (2 * i) + 1, (2 * j) - 1, weight)
                    + Group(source, (2 * i) - 1, (2 * j) + 1, weight) + Group(source, (2 * i) + 1, (2 * j) + 1, weight)) / 8f;
        }
        Chained(next, halvings - 1, static _ => 1f, tent);
        CvInvoke.Resize(next, up, level.Size, 0d, 0d, Inter.Linear);
        CvInvoke.SepFilter2D(up, up, DepthType.Cv32F, tent, tent, Planes.Centered, 0d, BorderType.Replicate);
        CvInvoke.Add(level, up, level);
    }

    private static Vector4 Group(ReadOnlySpan2D<Vector4> source, int x, int y, Func<Vector4, float> weight) {
        (Vector4 sum, float total) = (Vector4.Zero, 0f);
        for (int corner = 0; corner < 4; corner++) {
            Vector4 tap = source[int.Clamp(y + (corner & 2) - 1, 0, source.Height - 1), int.Clamp(x + (2 * (corner & 1)) - 1, 0, source.Width - 1)];
            (sum, total) = (sum + (weight(tap) * tap), total + weight(tap));
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
        Lens<Bloom, Option<ImageFile>>.New(
            static bloom => bloom.Dirt.Bind(static dirt => dirt.Switch(image: static image => Some(image.File), specks: static _ => Option<ImageFile>.None)),
            static file => bloom => bloom with { Dirt = file.Map(static file => (LensDirt)new LensDirt.Image(file)) | bloom.Dirt.Filter(static dirt => dirt.Map(image: false, specks: true)) }),
        ImageFile.Load, static file => file.Path));
    public static readonly BloomParameter DirtCoverage = new("dirt-coverage", new StateParameter<Bloom>.OptionalBounded<DirtCoverage, float, InvalidOptics>(
        Speckled(Lens<LensDirt.Specks, DirtCoverage>.New(static specks => specks.Coverage, static coverage => specks => specks with { Coverage = coverage })), new()));
    public static readonly BloomParameter DirtScale = new("dirt-scale", new StateParameter<Bloom>.OptionalBounded<ShortSideExtent, float, InvalidPixelValue>(
        Speckled(Lens<LensDirt.Specks, ShortSideExtent>.New(static specks => specks.Scale, static scale => specks => specks with { Scale = scale })),
        new() { Unit = Quantity.GetUnitInfo(RatioUnit.DecimalFraction), Soft = (0.001f * ReferenceFrame.GreaterSide, 0.5f * ReferenceFrame.GreaterSide) }));
    public static readonly BloomParameter DirtSeed = new("dirt-seed", new StateParameter<Bloom>.OptionalBounded<Seed, int, InvalidGenerator>(
        Speckled(Lens<LensDirt.Specks, Seed>.New(static specks => specks.Seed, static seed => specks => specks with { Seed = seed })), Seed.Presentation));
    public static readonly BloomParameter DirtGain = new("dirt-gain", new StateParameter<Bloom>.Bounded<DirtGain, float, InvalidOptics>(
        Lens<Bloom, DirtGain>.New(static bloom => bloom.DirtGain, static gain => bloom => bloom with { DirtGain = gain }),
        new() { Soft = (Optics.DirtGain.MinValue, Optics.DirtGain.Unity) }));

    public StateParameter<Bloom> Kind { get; }

    private static Lens<Bloom, Option<T>> Speckled<T>(Lens<LensDirt.Specks, T> member) {
        static Option<LensDirt.Specks> Of(LensDirt dirt) => dirt.Switch(image: static _ => Option<LensDirt.Specks>.None, specks: static specks => Some(specks));
        return Lens<Bloom, Option<T>>.New(
            bloom => bloom.Dirt.Bind(Of).Map(member.Get),
            value => bloom => bloom with {
                Dirt = value.Map(set => (LensDirt)member.Set(set, bloom.Dirt.Bind(Of).IfNone(LensDirt.Specks.Default))) | bloom.Dirt.Filter(static dirt => dirt.Map(image: true, specks: false)),
            });
    }
}

public sealed record VeilingGlare(AxisFraction Size) : IStateRecord<VeilingGlare, VeilingGlareParameter, InvalidOptics>, IPixelStage<VeilingGlare> {
    public static VeilingGlare Default { get; } = new(AxisFraction.Half);

    public static Option<PixelPass> Pass(VeilingGlare state, PassContext context) =>
        Some<PixelPass>(new PixelPass.Frame((frame, progress) => context.Derivations.Derived((state, context.Extent), Spread).Convolved(frame, progress)));

    private static PointSpread Spread((VeilingGlare State, PixelExtent Extent) key) {
        PointSpread spread = new(new Size((2 * key.Extent.Width) - 1, (2 * key.Extent.Height) - 1), [0, 0, 0]);
        using Mat kernel = spread.Plane(0);
        Span2D<float> cells = kernel.GetSpan<float>().AsSpan2D(kernel.Rows, kernel.Cols);
        double perPixel = double.Lerp(180d, 10d, double.Cbrt(key.State.Size)) / int.Max(key.Extent.Width, key.Extent.Height);
        for (int y = 0; y < cells.Height; y++) {
            for (int x = 0; x < cells.Width; x++)
                cells[y, x] = (float)Spencer(perPixel * double.Hypot(x - (key.Extent.Width - 1), y - (key.Extent.Height - 1)));
        }
        return spread.Normalized();
    }

    private static double Spencer(double degrees) =>
        (0.384d * 2.61e6d * double.Exp(-double.Pow(degrees / 0.02d, 2d))) + (0.478d * 20.91d / double.Pow(degrees + 0.02d, 3d)) + (0.138d * 72.37d / double.Pow(degrees + 0.02d, 2d));
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

    public static StarGlare Default { get; } = new(Iris.Hexagon, ShortSideLength.Create(2f / 1080f));

    public static Option<PixelPass> Pass(StarGlare state, PassContext context) =>
        Some<PixelPass>(new PixelPass.Frame((frame, progress) => context.Derivations.Derived((state, context.Extent, context.Working), Spread).Convolved(frame, progress)));

    private static PointSpread Spread((StarGlare State, PixelExtent Extent, Gamut Working) key) {
        (PixelExtent extent, Iris iris) = (key.Extent, key.State.Iris);
        Arr<(float Wavelength, Vector3 Matching)> rows = StandardObserver.Matching(key.Working);
        int grid = int.Min((int)BitOperations.RoundUpToPowerOf2((uint)(2 * int.Max(extent.Width, extent.Height))), PupilGrid);
        (int middle, float half) = (grid / 2, 1.22f * grid / (2f * key.State.Radius.Pixels(extent)));
        using Mat pupil = new(grid, grid, DepthType.Cv32F, 1);
        using Mat spectrum = new();
        using Mat power = new();
        using Mat energy = new();
        Span2D<float> cells = pupil.GetSpan<float>().AsSpan2D(grid, grid);
        for (int y = 0; y < grid; y++) {
            for (int x = 0; x < grid; x++)
                cells[y, x] = iris.Outline(new Vector2(x - middle, middle - y)) switch { var (reach, edge) => (1 - (2 * ((x + y) & 1))) * iris.Weights(reach, edge, half).W };
        }
        CvInvoke.Dft(pupil, spectrum, DxtType.ComplexOutput, 0);
        CvInvoke.MulSpectrums(spectrum, spectrum, power, MulSpectrumsType.Default, conjB: true);
        CvInvoke.ExtractChannel(power, energy, 0);
        using ScalarArray floor = new(Crop * energy.GetSpan<float>().AsSpan2D(grid, grid)[middle, middle]);
        using Mat held = new();
        CvInvoke.Compare(energy, floor, held, CmpType.GreaterEqual);
        Rectangle bound = CvInvoke.BoundingRectangle(held);
        (int across, int down) = (int.Max(middle - bound.Left, bound.Right - 1 - middle), int.Max(middle - bound.Top, bound.Bottom - 1 - middle));
        (float shortest, float longest) = (rows.Min(static row => row.Wavelength), rows.Max(static row => row.Wavelength));
        (across, down) = (int.Min(across, (int)float.Ceiling((extent.Width - 1) * Reference / shortest)), int.Min(down, (int)float.Ceiling((extent.Height - 1) * Reference / shortest)));
        (int reachX, int reachY) = (int.Min((int)float.Ceiling(across * longest / Reference), extent.Width - 1), int.Min((int)float.Ceiling(down * longest / Reference), extent.Height - 1));
        PointSpread spread = new(new Size((2 * reachX) + 1, (2 * reachY) + 1), [0, 1, 2]);
        using Mat region = new(energy, new Rectangle(middle - across, middle - down, (2 * across) + 1, (2 * down) + 1));
        using Mat dilated = new();
        foreach ((float wavelength, Vector3 matching) in rows) {
            float scale = wavelength / Reference;
            (int sourceX, int sourceY) = (int.Min(across, (int)float.Ceiling(reachX / scale)), int.Min(down, (int)float.Ceiling(reachY / scale)));
            (int targetX, int targetY) = (int.Min((int)float.Round(sourceX * scale, MidpointRounding.AwayFromZero), reachX), int.Min((int)float.Round(sourceY * scale, MidpointRounding.AwayFromZero), reachY));
            using Mat source = new(region, new Rectangle(across - sourceX, down - sourceY, (2 * sourceX) + 1, (2 * sourceY) + 1));
            CvInvoke.Resize(source, dilated, new Size((2 * targetX) + 1, (2 * targetY) + 1), 0d, 0d, scale < 1f ? Inter.Area : Inter.Linear);
            for (int component = 0; component < 3; component++) {
                using Mat whole = spread.Plane(component);
                using Mat target = new(whole, new Rectangle(reachX - targetX, reachY - targetY, (2 * targetX) + 1, (2 * targetY) + 1));
                CvInvoke.ScaleAdd(dilated, matching[component] / (scale * scale), target, target);
            }
        }
        return spread.Normalized();
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
        Lens<StarGlare, ShortSideLength>.New(static glare => glare.Radius, static radius => glare => glare with { Radius = radius }),
        new() { Unit = Quantity.GetUnitInfo(RatioUnit.DecimalFraction), Soft = (1.25f / 1080f, 64f / 1080f) }));

    public StateParameter<StarGlare> Kind { get; }
}

public sealed record Ghosts(HighlightKey Key, GhostIterations Iterations, Mix Modulation) : IStateRecord<Ghosts, GhostsParameter, InvalidOptics>, IPixelStage<Ghosts> {
    private static readonly Arr<Vector3> Modulated = [Vector3.Zero, new(0f, 1f, 1f), new(1f, 1f, 0f), new(1f, 0f, 1f)];

    public static Ghosts Default { get; } = new(new HighlightKey(HighlightThreshold.White, AxisFraction.Create(0.1f)), GhostIterations.Three, Mix.Create(0.25f));

    public static Option<PixelPass> Pass(Ghosts state, PassContext context) =>
        Some<PixelPass>(new PixelPass.Frame((frame, progress) => {
            float radius = ShortSideLength.Create(16f / 1080f).Pixels(frame.Extent);
            using Mat key = new(frame.Size.Height, frame.Size.Width, DepthType.Cv32F, 4);
            using Mat big = new();
            using Mat input = Mat.Zeros(frame.Size.Height, frame.Size.Width, DepthType.Cv32F, 4);
            using Mat output = Mat.Zeros(frame.Size.Height, frame.Size.Width, DepthType.Cv32F, 4);
            return key.Lit(frame, state.Key.Extract).Bind(_ => {
                key.Blur(big, 2f * radius);
                key.Blur(key, radius);
                Accumulate(key, input, [(2.13f, Vector4.One)]);
                Accumulate(big, input, [(-0.97f, Vector4.One)]);
                for (int i = 1; i < state.Iterations; i++) {
                    Accumulate(input, output, Copies(i, state.Iterations, state.Modulation));
                    output.CopyTo(input);
                }
                return output.Composited(frame, progress, Vector4.Add);
            });
        }));

    internal static (float Scale, Vector4 Gain)[] Copies(int iteration, int count, float modulation) =>
        [.. Enumerable.Range(0, Modulated.Count).Select(k => (
            (2.1f * (1f - (((4 * iteration) + k + (count % 2 == 1 ? 0.5f : 0f)) / (4f * count)))) switch { var s => k % 2 == 1 ? -0.99f / s : s },
            new Vector4((Vector3.One - (Modulated[k] * modulation)) / Modulated.Count, 0f)))];

    private static void Accumulate(Mat source, Mat target, ReadOnlySpan<(float Scale, Vector4 Gain)> copies) {
        ReadOnlySpan2D<Vector4> from = source.GetSpan<Vector4>().AsSpan2D(source.Rows, source.Cols);
        Span2D<Vector4> to = target.GetSpan<Vector4>().AsSpan2D(target.Rows, target.Cols);
        Vector2 center = new Vector2(to.Width, to.Height) / 2f;
        for (int y = 0; y < to.Height; y++) {
            for (int x = 0; x < to.Width; x++) {
                Vector2 offset = new Vector2(x + 0.5f, y + 0.5f) - center;
                float reach = (offset / center).Length();
                foreach ((float scale, Vector4 gain) in copies)
                    to[y, x] += from.Sample(center + (offset * scale), (WrapMode.Black, WrapMode.Black)) * gain * float.Max(0f, 1f - (reach * float.Abs(scale)));
            }
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
    public static Halation Default { get; } = new(new HighlightKey(HighlightThreshold.OneStopOver, AxisFraction.MaxValue), ShortSideLength.Create(12.8f / 1080f), Swatch.Create(0xFF5900));

    public static Option<PixelPass> Pass(Halation state, PassContext context) =>
        Some<PixelPass>(new PixelPass.Frame((frame, progress) => {
            Vector4 color = new(state.Color.SceneLight(context.Working), 0f);
            using Mat key = new(frame.Size.Height, frame.Size.Width, DepthType.Cv32F, 4);
            return key.Lit(frame, pixel => state.Key.Extract(pixel) * color).Bind(_ => {
                key.Blur(key, state.Radius.Pixels(frame.Extent));
                return key.Composited(frame, progress, Vector4.Add);
            });
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
        Lens<Halation, ShortSideLength>.New(static halation => halation.Radius, static radius => halation => halation with { Radius = radius }),
        new() { Unit = Quantity.GetUnitInfo(RatioUnit.DecimalFraction), Soft = (ShortSideLength.MinValue, 64f / 1080f) }));
    public static readonly HalationParameter Color = new("color", new StateParameter<Halation>.Color(
        Lens<Halation, Swatch>.New(static halation => halation.Color, static color => halation => halation with { Color = color })));

    public StateParameter<Halation> Kind { get; }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
file static class Planes {
    private static readonly Progress<int> Unreported = new();

    public static Point Centered { get; } = new(-1, -1);

    public static Vector4 Light(Vector4 pixel) => new(pixel.AsVector3() * pixel.W, 0f);

    extension(Mat plane) {
        public Span<Vector4> Row(PixelFrame frame, int line) => plane.GetSpan<Vector4>().AsSpan2D(plane.Rows, plane.Cols).GetRowSpan(frame.Line(line));

        public void Blur(Mat target, float radius) {
            int taps = (2 * (int)float.Ceiling(radius)) + 1;
            CvInvoke.GaussianBlur(plane, target, new Size(taps, taps), radius / 3f, radius / 3f, BorderType.Replicate);
        }

        public Fin<Unit> Lit(PixelFrame frame, Func<Vector4, Vector4> light) =>
            new PixelPass.Pointwise((row, _, line) => {
                Span<Vector4> target = plane.Row(frame, line);
                for (int x = 0; x < row.Length; x++)
                    target[x] = light(row[x]);
            }).Run(frame, Unreported);

        public Fin<Unit> Mixed(PixelFrame frame, float amount, IProgress<int> progress) =>
            plane.Composited(frame, progress, (color, layer) => new Vector4(Vector3.Lerp(color.AsVector3(), layer.AsVector3(), amount), color.W));

        public Fin<Unit> Composited(PixelFrame frame, IProgress<int> progress, Func<Vector4, Vector4, Vector4> composite) =>
            new PixelPass.Pointwise((row, _, line) => {
                Span<Vector4> layer = plane.Row(frame, line);
                for (int x = 0; x < row.Length; x++)
                    row[x] = composite(row[x], layer[x]);
            }).Run(frame, progress);
    }
}
