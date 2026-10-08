using System.Numerics;
using System.Runtime.InteropServices;
using CommunityToolkit.HighPerformance;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Util;
using Rasm.Imaging.ColorManagement;
using Rasm.Imaging.Filters.Generators;
using Rasm.Imaging.Filters.Optics;
using Rasm.Imaging.Filters.Warp;
using Rasm.Imaging.Grade;
using Rasm.Imaging.Pixels;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using UnitsNet.Units;

namespace Rasm.Imaging.Filters.Stylize;

// --- [CONSTANTS] -----------------------------------------------------------------------
file static class Units {
    public static UnitsNet.UnitInfo Fraction { get; } = UnitsNet.Quantity.GetUnitInfo(RatioUnit.DecimalFraction);
}

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<float>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Off", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidStylize>]
public readonly partial struct Spread : IMinMaxValue<Spread> {
    public static Spread MinValue => Off;
    public static Spread MaxValue { get; } = new(0.05f);

    public float Pixels(PixelExtent extent) => _value * extent.ShortSide;

    static partial void ValidateFactoryArguments(ref InvalidStylize? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidStylize();
}

[ValueObject<float>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Off", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidStylize>]
public readonly partial struct EdgeGain : IMinMaxValue<EdgeGain> {
    public static EdgeGain MinValue => Off;
    public static EdgeGain MaxValue { get; } = new(4f);

    static partial void ValidateFactoryArguments(ref InvalidStylize? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidStylize();
}

[ValueObject<float>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Off", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidStylize>]
public readonly partial struct NoiseLevel : IMinMaxValue<NoiseLevel> {
    public static NoiseLevel MinValue => Off;
    public static NoiseLevel MaxValue { get; } = new(1f);

    static partial void ValidateFactoryArguments(ref InvalidStylize? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidStylize();
}

[ValueObject<int>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidStylize>]
public readonly partial struct LineCount : IMinMaxValue<LineCount> {
    public static LineCount MinValue { get; } = new(16);
    public static LineCount MaxValue { get; } = new(4320);
    public static LineCount Ntsc { get; } = new(480);

    public int Line(int row, PixelExtent extent) => (int)((long)row * _value / extent.Height);

    static partial void ValidateFactoryArguments(ref InvalidStylize? validationError, ref int value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidStylize();
}

[ValueObject<int>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidStylize>]
public readonly partial struct JpegQuality : IMinMaxValue<JpegQuality> {
    public static JpegQuality MinValue { get; } = new(1);
    public static JpegQuality MaxValue { get; } = new(100);
    public static JpegQuality Standard { get; } = new(75);

    static partial void ValidateFactoryArguments(ref InvalidStylize? validationError, ref int value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidStylize();
}

[ValueObject<int>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidStylize>]
public readonly partial struct Generations : IMinMaxValue<Generations> {
    public static Generations MinValue => Single;
    public static Generations MaxValue { get; } = new(8);
    public static Generations Single { get; } = new(1);

    static partial void ValidateFactoryArguments(ref InvalidStylize? validationError, ref int value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidStylize();
}

[SmartEnum<string>]
[ValidationError<InvalidStylize>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class YuvMatrix {
    public static readonly YuvMatrix Rec601 = new("rec601", static () => new Vector3(0.299f, 0.587f, 0.114f));
    public static readonly YuvMatrix Rec709 = new("rec709", static () => Gamut.StandardRgb.Luminance);

    [UseDelegateFromConstructor]
    public partial Vector3 Weights();
}

[SmartEnum<string>]
[ValidationError<InvalidStylize>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PhosphorMask {
    public static readonly PhosphorMask ApertureGrille = new("aperture-grille", gap: 0f, columnStagger: 0f, rowStagger: 0f);
    public static readonly PhosphorMask SlotMask = new("slot-mask", gap: 1f / 3f, columnStagger: 0.5f, rowStagger: 0f);
    public static readonly PhosphorMask ShadowMask = new("shadow-mask", gap: 1f / 3f, columnStagger: 0f, rowStagger: 0.5f);

    private readonly float _gap;
    private readonly float _columnStagger;
    private readonly float _rowStagger;

    public Vector3 Coverage(int column, int row, float pitch) {
        float span = 1f / pitch;
        float u = (column * span) + (((int)MathF.Floor((row + 0.5f) * span) & 1) * _rowStagger);
        float v = (row * span) + (((int)MathF.Floor((column + 0.5f) * span) & 1) * _columnStagger);
        return 3f * (Stripes(u + span) - Stripes(u)) * (Open(v + span) - Open(v)) / (span * span * (1f - _gap));
    }

    private static Vector3 Stripes(float t) =>
        MathF.Floor(t) switch {
            var cell => new Vector3(cell / 3f) + Vector3.Clamp(new Vector3(t - cell) - new Vector3(0f, 1f / 3f, 2f / 3f), Vector3.Zero, new Vector3(1f / 3f)),
        };

    private float Open(float t) =>
        MathF.Floor(t) switch {
            var cell => (cell * (1f - _gap)) + float.Min(t - cell, 1f - _gap),
        };
}

public sealed record CrtDisplay(
    PhosphorMask Mask, ShortSideExtent Pitch, Mix MaskStrength, LineCount Lines, bool Interlaced, Mix ScanlineDepth,
    Mix Collapse, Spread Glow, Distortion Curvature, LensFraming Framing, Timing Timing)
    : IStateRecord<CrtDisplay, CrtDisplayParameter, InvalidStylize>, IPixelStage<CrtDisplay> {
    private const double NearShare = 0.890909d;

    public static CrtDisplay Default { get; } = new(
        PhosphorMask.SlotMask, ShortSideExtent.Create(8f / 1080f), Mix.Create(0.8f), LineCount.Ntsc, Interlaced: false, Mix.Full,
        Mix.MinValue, Spread.Off, Distortion.Neutral, LensFraming.Edges, Timing.Standard);

    public static CrtDisplay Monitor { get; } = Default with {
        Mask = PhosphorMask.ApertureGrille,
        Pitch = ShortSideExtent.Create(16f / 9f / 640f),
        MaskStrength = Mix.Create(0.705394f),
        Lines = LineCount.Create(240),
        ScanlineDepth = Mix.Create(0.5f),
        Glow = Spread.Create(0.01f * 16f / 9f / 3f),
        Curvature = Distortion.Create(0.0497677f),
    };

    public static CrtDisplay Scanlines { get; } = Default with { MaskStrength = Mix.MinValue, ScanlineDepth = Mix.Create(0.4f) };

    private bool Lit => MaskStrength != Mix.MinValue || ScanlineDepth != Mix.MinValue || Collapse != Mix.MinValue;

    private (float X, float Y) Scales =>
        ((float)Collapse, 1f / Lines) switch {
            var (k, line) => (float.Min(1f, float.Max(2f - (2f * k), line)), float.Max(1f - (2f * k), line)),
        };

    public static Option<PixelPass> Pass(CrtDisplay state, PassContext context) =>
        (state.Lit, state.Collapse == Mix.MinValue && state.Glow == Spread.Off && state.Curvature == Distortion.Neutral) switch {
            (false, true) => None,
            (true, true) => Some<PixelPass>(new PixelPass.Pointwise(Raster(state, context))),
            _ => Some<PixelPass>(new PixelPass.Frame((frame, progress) => Kernel(state, context, frame, progress))),
        };

    private static Fin<Unit> Kernel(CrtDisplay state, PassContext context, PixelFrame frame, IProgress<int> progress) =>
        (state.Collapse == Mix.MinValue ? unit : new CoordinateMap.Analytic(Deflection(state.Scales, context.Extent), Sampling.Linear, WrapMode.Black).Apply(frame, progress))
            .Bind(_ => {
                if (state.Glow != Spread.Off)
                    Bloom(frame, state.Glow.Pixels(context.Extent));
                return state.Lit ? new PixelPass.Pointwise(Raster(state, context)).Run(frame, progress) : unit;
            })
            .Bind(_ => state.Curvature == Distortion.Neutral ? unit
                : new LensDistortion(state.Curvature, Dispersion.Neutral, state.Framing).Placed(context.Extent).Apply(frame, progress));

    private static Action<Span<Vector2>, Span<float>, PixelExtent> Deflection((float X, float Y) scale, PixelExtent extent) {
        Vector2 centre = new Vector2(extent.Width, extent.Height) / 2f;
        Vector2 spread = Vector2.One / new Vector2(scale.X, scale.Y);
        return (points, _, _) => {
            foreach (ref Vector2 point in points)
                point = centre + ((point - centre) * spread);
        };
    }

    private static void Bloom(PixelFrame frame, float sigma) {
        using Mat header = frame.Header();
        using Mat alpha = new();
        using Mat near = new();
        using Mat far = new();
        CvInvoke.ExtractChannel(header, alpha, 3);
        int taps = Kernels.Taps(sigma);
        CvInvoke.GaussianBlur(header, near, new System.Drawing.Size(taps, taps), sigma, sigma, BorderType.Replicate);
        CvInvoke.GaussianBlur(near, far, new System.Drawing.Size(Kernels.Taps(4f * sigma), taps), 4f * sigma, sigma, BorderType.Replicate);
        CvInvoke.AddWeighted(near, NearShare, far, 1d - NearShare, 0d, header);
        CvInvoke.InsertChannel(alpha, header, 3);
    }

    private static Action<Span<Vector4>, int, int> Raster(CrtDisplay state, PassContext context) {
        PixelExtent extent = context.Extent;
        (float width, float height) = state.Scales;
        (double middle, double field) = (extent.Height / 2d, state.Interlaced ? 2d * extent.Height : extent.Height);
        double parity = state.Interlaced ? SystemM.Frame(state.Timing.At(context)) % 2 / 2d : 0d;
        (float pitch, float mask, float depth) = (state.Pitch.Pixels(extent), state.MaskStrength, state.ScanlineDepth);
        float power = 1f / (width * height);
        return (row, column, line) => {
            int y = extent.Height - 1 - line;
            (double top, double bottom) = (Beam(y), Beam(y + 1));
            float lit = power * (1f - (depth * (float)(0.5d + ((double.SinPi(2d * bottom) - double.SinPi(2d * top)) / (4d * double.Pi * (bottom - top)))))) / (1f - (depth / 2f));
            for (int x = 0; x < row.Length; x++)
                row[x] = new Vector4(row[x].AsVector3() * (new Vector3(1f - mask) + (mask * state.Mask.Coverage(column + x, y, pitch))) * lit, row[x].W);
        };

        double Beam(int y) => ((middle + ((y - middle) / height)) * state.Lines / field) + parity;
    }
}

[SmartEnum<string>]
[ValidationError<InvalidStylize>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CrtDisplayParameter : IStateParameter<CrtDisplay> {
    private static readonly (StateParameter<CrtDisplay> Clock, StateParameter<CrtDisplay> Pace) Time =
        Timing.Kinds(Lens<CrtDisplay, Timing>.New(static state => state.Timing, static timing => state => state with { Timing = timing }));
    public static readonly CrtDisplayParameter Mask = new(
        "mask", new StateParameter<CrtDisplay>.Choice<PhosphorMask, InvalidStylize>(Lens<CrtDisplay, PhosphorMask>.New(static crt => crt.Mask, static mask => crt => crt with { Mask = mask })));
    public static readonly CrtDisplayParameter Pitch = new(
        "pitch",
        new StateParameter<CrtDisplay>.Bounded<ShortSideExtent, float, InvalidPixelValue>(
            Lens<CrtDisplay, ShortSideExtent>.New(static crt => crt.Pitch, static pitch => crt => crt with { Pitch = pitch }),
            new() { Unit = Units.Fraction, Soft = (0.002f, 0.02f), Scale = TrackScale.Log }));
    public static readonly CrtDisplayParameter MaskStrength = new(
        "mask-strength",
        new StateParameter<CrtDisplay>.Bounded<Mix, float, InvalidGrade>(Lens<CrtDisplay, Mix>.New(static crt => crt.MaskStrength, static strength => crt => crt with { MaskStrength = strength }), new()));
    public static readonly CrtDisplayParameter Lines = new(
        "lines",
        new StateParameter<CrtDisplay>.Bounded<LineCount, int, InvalidStylize>(
            Lens<CrtDisplay, LineCount>.New(static crt => crt.Lines, static lines => crt => crt with { Lines = lines }), new() { Soft = (120, 1080) }));
    public static readonly CrtDisplayParameter Interlaced = new(
        "interlaced", new StateParameter<CrtDisplay>.Toggle(Lens<CrtDisplay, bool>.New(static crt => crt.Interlaced, static interlaced => crt => crt with { Interlaced = interlaced })));
    public static readonly CrtDisplayParameter ScanlineDepth = new(
        "scanline-depth",
        new StateParameter<CrtDisplay>.Bounded<Mix, float, InvalidGrade>(Lens<CrtDisplay, Mix>.New(static crt => crt.ScanlineDepth, static depth => crt => crt with { ScanlineDepth = depth }), new()));
    public static readonly CrtDisplayParameter Collapse = new(
        "collapse",
        new StateParameter<CrtDisplay>.Bounded<Mix, float, InvalidGrade>(Lens<CrtDisplay, Mix>.New(static crt => crt.Collapse, static collapse => crt => crt with { Collapse = collapse }), new()));
    public static readonly CrtDisplayParameter Glow = new(
        "glow",
        new StateParameter<CrtDisplay>.Bounded<Spread, float, InvalidStylize>(
            Lens<CrtDisplay, Spread>.New(static crt => crt.Glow, static glow => crt => crt with { Glow = glow }), new() { Unit = Units.Fraction, Soft = (0f, 0.005f) }));
    public static readonly CrtDisplayParameter Curvature = new(
        "curvature",
        new StateParameter<CrtDisplay>.Bounded<Distortion, float, InvalidOptics>(
            Lens<CrtDisplay, Distortion>.New(static crt => crt.Curvature, static curvature => crt => crt with { Curvature = curvature }), new() { Origin = (float)Distortion.Neutral }));
    public static readonly CrtDisplayParameter Framing = new(
        "framing", new StateParameter<CrtDisplay>.Choice<LensFraming, InvalidOptics>(Lens<CrtDisplay, LensFraming>.New(static crt => crt.Framing, static framing => crt => crt with { Framing = framing })));
    public static readonly CrtDisplayParameter Clock = new("clock", Time.Clock);
    public static readonly CrtDisplayParameter Pace = new("pace", Time.Pace);

    public StateParameter<CrtDisplay> Kind { get; }
}

public sealed record AnalogVideo(
    YuvMatrix Matrix, LineCount Lines,
    Spread LumaBandwidth, Spread ChromaBandwidth, Spread ChromaVertical, ShortSideLength ChromaPitch, ShortSideOffset CbDelay, ShortSideOffset CrDelay,
    Spread Smear, Mix SmearMix, EdgeGain Ringing, ShortSideOffset RingingDelay, EdgeGain Emboss, Mix Ghost, ShortSideOffset GhostDelay, Mix CrossColor,
    NoiseLevel LumaNoise, NoiseLevel ChromaNoise, ShortSideExtent NoiseLength, Mix Snow, Likelihood SnowDensity,
    ShortSideOffset Interlace, ShortSideLength Jitter, ShortSideExtent JitterPeriod, ShortSideOffset Wrinkle, ShortSideExtent WrinkleHeight, Frequency WrinkleRate,
    ShortSideOffset HeadSwitch, ShortSideExtent HeadSwitchHeight, ShortSideLength Bounce, Frequency RollRate, Mix Hum, Frequency HumRate,
    Seed Seed, Timing Timing)
    : IStateRecord<AnalogVideo, AnalogVideoParameter, InvalidStylize>, IPixelStage<AnalogVideo> {
    private const float SwitchCrest = 0.9f;

    public static AnalogVideo Default { get; } = new(
        YuvMatrix.Rec601, LineCount.Ntsc,
        Spread.Off, Spread.Off, Spread.Off, ShortSideLength.Neutral, ShortSideOffset.Neutral, ShortSideOffset.Neutral,
        Spread.Off, Mix.Create(0.3f * 0.25f), EdgeGain.Off, ShortSideOffset.Neutral, EdgeGain.Off, Mix.MinValue, ShortSideOffset.Neutral, Mix.MinValue,
        NoiseLevel.Off, NoiseLevel.Off, ShortSideExtent.Create(1920f / 1080f / 40f), Mix.MinValue, Likelihood.Off,
        ShortSideOffset.Neutral, ShortSideLength.Neutral, ShortSideExtent.Create(1f / 65f), ShortSideOffset.Neutral, ShortSideExtent.Create(0.15f), Frequency.Neutral,
        ShortSideOffset.Neutral, ShortSideExtent.Create(0.31f / (32f - (28.8f * 0.5f))), ShortSideLength.Neutral, Frequency.Neutral, Mix.MinValue, Frequency.Neutral,
        Seed.MinValue, Timing.Standard);

    public static AnalogVideo ChromaBlur { get; } = Default with {
        ChromaBandwidth = Spread.Create(128f / 1080f / 3f),
        ChromaVertical = Spread.Create(128f * 0.1f / 1080f / 3f),
    };

    private bool Moves =>
        Interlace != ShortSideOffset.Neutral || Jitter != ShortSideLength.Neutral || Wrinkle != ShortSideOffset.Neutral
        || HeadSwitch != ShortSideOffset.Neutral || Bounce != ShortSideLength.Neutral || RollRate != Frequency.Neutral;

    private bool Filters =>
        LumaBandwidth != Spread.Off || ChromaBandwidth != Spread.Off || ChromaVertical != Spread.Off || ChromaPitch != ShortSideLength.Neutral
        || CbDelay != ShortSideOffset.Neutral || CrDelay != ShortSideOffset.Neutral || Smear != Spread.Off || Ghost != Mix.MinValue || CrossColor != Mix.MinValue;

    private bool Lined => LumaNoise != NoiseLevel.Off || ChromaNoise != NoiseLevel.Off;

    private bool Speckled => Snow != Mix.MinValue || Hum != Mix.MinValue;

    public static Option<PixelPass> Pass(AnalogVideo state, PassContext context) =>
        state.Moves || state.Filters || state.Lined || state.Speckled
            ? Some<PixelPass>(new PixelPass.Frame((frame, progress) => Kernel(state, context, frame, progress)))
            : None;

    private static Fin<Unit> Kernel(AnalogVideo state, PassContext context, PixelFrame frame, IProgress<int> progress) {
        (PixelExtent extent, Clock now) = (context.Extent, state.Timing.At(context));
        uint field = CoordinateHash.Field(NoiseStream.AnalogVideo, state.Seed, now.Term);
        return (state.Moves ? Sync(state, extent, now, field).Apply(frame, progress) : unit)
            .Bind(_ => {
                if (state.Filters)
                    Signal(state, extent, now, frame);
                return state.Lined ? LineNoise(state, extent, field, frame, progress) : unit;
            })
            .Bind(_ => state.Speckled ? Snowfall(state, extent, now, field, frame, progress) : unit);
    }

    private static CoordinateMap Sync(AnalogVideo state, PixelExtent extent, Clock now, uint field) {
        float sigma = state.JitterPeriod.Pixels(extent) * state.Lines / extent.Height;
        int reach = (int)MathF.Ceiling(3f * sigma);
        uint jitterDraws = CoordinateHash.Branch(field, 2u);
        using Mat plane = new(state.Lines + (2 * reach), 1, DepthType.Cv32F, 1);
        Span<float> draws = plane.GetSpan<float>();
        for (int index = 0; index < draws.Length; index++)
            draws[index] = CoordinateHash.Normals(0, index - reach, jitterDraws).X;
        using (Mat taps = Kernels.Unit(sigma))
            Kernels.Across(plane, taps);
        float[] jitters = plane.GetSpan<float>().ToArray();
        float height = extent.Height;
        float rise = Kernels.Fraction(state.RollRate * now) * height;
        float jump = state.Bounce.Pixels(extent) * ((2f * NoiseFunctions.White(Vector4.Zero, CoordinateHash.Branch(field, 3u)).X) - 1f);
        float crest = height * (1f - Kernels.Fraction(state.WrinkleRate * now));
        (float interlace, float jitter, float wrinkle, float wrinkleBand) =
            (state.Interlace.Pixels(extent), state.Jitter.Pixels(extent), state.Wrinkle.Pixels(extent), state.WrinkleHeight.Pixels(extent));
        (float switching, float switchBand) = (state.HeadSwitch.Pixels(extent), state.HeadSwitchHeight.Pixels(extent));
        return new CoordinateMap.Analytic(
                (points, _, _) => {
                    foreach (ref Vector2 point in points)
                        point.Y += rise;
                }, Sampling.Linear, WrapMode.Periodic)
            .Then(new CoordinateMap.Analytic(
                (points, _, _) => {
                    foreach (ref Vector2 point in points)
                        point.Y -= jump;
                }, Sampling.Linear, WrapMode.Black))
            .Then(new CoordinateMap.Analytic(
                (points, _, _) => {
                    foreach (ref Vector2 point in points) {
                        int line = state.Lines.Line((int)point.Y, extent);
                        float distance = point.Y - crest - (height * MathF.Round((point.Y - crest) / height, MidpointRounding.ToEven));
                        float band = (height - point.Y) / switchBand;
                        point.X -= ((line & 1) * interlace) + (jitter * jitters[line + reach]) + (wrinkle * float.Max(0f, 1f - (MathF.Abs(distance) / wrinkleBand)))
                            + (band is >= 0f and < 1f ? switching * (band < SwitchCrest ? band / SwitchCrest : (1f - band) / (1f - SwitchCrest)) : 0f);
                    }
                }, Sampling.Linear, WrapMode.Clamp));
    }

    private static void Signal(AnalogVideo state, PixelExtent extent, Clock now, PixelFrame frame) {
        ChromaAxes axes = new(state.Matrix.Weights());
        using Mat header = frame.Header();
        using Mat forward = Kernels.Transform(axes.Forward);
        using Mat inverse = Kernels.Transform(axes.Inverse);
        using Mat ycc = new();
        using VectorOfMat planes = new();
        CvInvoke.Transform(header, ycc, forward);
        CvInvoke.Split(ycc, planes);
        using Mat luma = planes[0];
        using Mat blue = planes[1];
        using Mat red = planes[2];
        if (state.CrossColor != Mix.MinValue)
            Rainbow(state, extent, now, frame, luma, blue, red);
        Luma(state, extent, luma);
        Chroma(state, extent, blue, state.CbDelay);
        Chroma(state, extent, red, state.CrDelay);
        CvInvoke.Merge(planes, ycc);
        CvInvoke.Transform(ycc, header, inverse);
    }

    private static void Rainbow(AnalogVideo state, PixelExtent extent, Clock now, PixelFrame frame, Mat luma, Mat blue, Mat red) {
        using Mat detail = new();
        using (Mat taps = Kernels.Gauss(extent.Width / (6f * SystemM.Subcarrier)))
        using (Mat impulse = Kernels.Impulse())
            CvInvoke.SepFilter2D(luma, detail, DepthType.Cv32F, taps, impulse, Kernels.Anchor, 0d, BorderType.Replicate);
        CvInvoke.Subtract(luma, detail, detail);
        (float Sin, float Cos)[] carrier = [.. Enumerable.Range(frame.Origin.X, frame.Size.Width).Select(x => float.SinCosPi(2f * SystemM.Subcarrier * x / extent.Width))];
        long count = SystemM.Frame(now);
        Span2D<float> luminance = detail.GetSpan<float>().AsSpan2D(frame.Size.Height, frame.Size.Width);
        Span2D<float> cb = blue.GetSpan<float>().AsSpan2D(frame.Size.Height, frame.Size.Width);
        Span2D<float> cr = red.GetSpan<float>().AsSpan2D(frame.Size.Height, frame.Size.Width);
        for (int y = 0; y < luminance.Height; y++) {
            float amplitude = ((state.Lines.Line(frame.Origin.Y + y, extent) + count) & 1) == 0 ? state.CrossColor : -state.CrossColor;
            for (int x = 0; x < luminance.Width; x++)
                (cb[y, x], cr[y, x]) = (cb[y, x] + (amplitude * luminance[y, x] * carrier[x].Cos), cr[y, x] + (amplitude * luminance[y, x] * carrier[x].Sin));
        }
    }

    private static void Luma(AnalogVideo state, PixelExtent extent, Mat luma) {
        float sigma = state.LumaBandwidth.Pixels(extent);
        (bool rings, bool embosses) = (sigma > 0f && state.Ringing != EdgeGain.Off, sigma > 0f && state.Emboss != EdgeGain.Off);
        using Mat ring = new();
        using Mat relief = new();
        if (sigma > 0f)
            using (Mat taps = Kernels.Gauss(sigma))
                Kernels.Along(luma, taps);
        if (rings) {
            CvInvoke.Sobel(luma, ring, DepthType.Cv32F, 2, 0, 1, 1d, 0d, BorderType.Replicate);
            using Mat taps = Kernels.Echo(0f, state.RingingDelay.Pixels(extent), 1f);
            Kernels.Along(ring, taps);
        }
        if (embosses)
            CvInvoke.Sobel(luma, relief, DepthType.Cv32F, 1, 0, 1, 1d, 0d, BorderType.Replicate);
        float smear = state.Smear.Pixels(extent);
        if (smear > 0f)
            using (Mat taps = Kernels.Echo(smear, smear, state.SmearMix))
                Kernels.Along(luma, taps);
        if (rings)
            CvInvoke.ScaleAdd(ring, -state.Ringing * sigma * sigma, luma, luma);
        if (embosses)
            CvInvoke.ScaleAdd(relief, -state.Emboss * sigma / 2f, luma, luma);
        if (state.Ghost != Mix.MinValue)
            using (Mat taps = Kernels.Echo(0f, state.GhostDelay.Pixels(extent), state.Ghost))
                Kernels.Along(luma, taps);
    }

    private static void Chroma(AnalogVideo state, PixelExtent extent, Mat plane, ShortSideOffset delay) {
        float pitch = state.ChromaPitch.Pixels(extent);
        if (pitch > 1f) {
            using Mat coarse = new();
            CvInvoke.Resize(plane, coarse, new System.Drawing.Size(int.Max(1, (int)MathF.Round(plane.Cols / pitch, MidpointRounding.ToEven)), plane.Rows), 0d, 0d, Inter.Area);
            CvInvoke.Resize(coarse, plane, plane.Size, 0d, 0d, Inter.Linear);
        }
        if (state.ChromaBandwidth != Spread.Off)
            using (Mat taps = Kernels.Gauss(state.ChromaBandwidth.Pixels(extent)))
                Kernels.Along(plane, taps);
        if (state.ChromaVertical != Spread.Off)
            using (Mat taps = Kernels.Gauss(state.ChromaVertical.Pixels(extent)))
                Kernels.Across(plane, taps);
        if (delay != ShortSideOffset.Neutral)
            using (Mat taps = Kernels.Echo(0f, delay.Pixels(extent), 1f))
                Kernels.Along(plane, taps);
        if (state.Ghost != Mix.MinValue)
            using (Mat taps = Kernels.Echo(0f, state.GhostDelay.Pixels(extent), state.Ghost))
                Kernels.Along(plane, taps);
    }

    private static Fin<Unit> LineNoise(AnalogVideo state, PixelExtent extent, uint field, PixelFrame frame, IProgress<int> progress) {
        float length = state.NoiseLength.Pixels(extent);
        int reach = (int)MathF.Ceiling(3f * length);
        int width = frame.Size.Width + (2 * reach);
        uint draws = CoordinateHash.Branch(field, 0u);
        using Mat plane = new(state.Lines, width, DepthType.Cv32F, 3);
        Span2D<Vector3> cells = plane.GetSpan<Vector3>().AsSpan2D(state.Lines, width);
        for (int line = 0; line < cells.Height; line++) {
            for (int column = 0; column < cells.Width; column++)
                cells[line, column] = CoordinateHash.Normals(frame.Origin.X - reach + column, line, draws).AsVector3();
        }
        using (Mat taps = Kernels.Unit(length))
            Kernels.Along(plane, taps);
        Matrix4x4 inverse = new ChromaAxes(state.Matrix.Weights()).Inverse;
        Vector3 luma = new(state.LumaNoise);
        (Vector3 blue, Vector3 red) = (state.ChromaNoise * new Vector3(inverse.M21, inverse.M22, inverse.M23), state.ChromaNoise * new Vector3(inverse.M31, inverse.M32, inverse.M33));
        return new PixelPass.Pointwise((row, _, line) => {
            ReadOnlySpan<Vector3> noise = plane.GetSpan<Vector3>().AsSpan2D(state.Lines, width).GetRowSpan(state.Lines.Line(extent.Height - 1 - line, extent)).Slice(reach, row.Length);
            for (int x = 0; x < row.Length; x++)
                row[x] += new Vector4((noise[x].X * luma) + (noise[x].Y * blue) + (noise[x].Z * red), 0f);
        }).Run(frame, progress);
    }

    private static Fin<Unit> Snowfall(AnalogVideo state, PixelExtent extent, Clock now, uint field, PixelFrame frame, IProgress<int> progress) {
        (float length, float phase) = (state.NoiseLength.Pixels(extent), Kernels.Fraction(state.HumRate * now));
        uint flakes = CoordinateHash.Branch(field, 1u);
        return new PixelPass.Pointwise((row, column, line) => {
            int y = extent.Height - 1 - line;
            int scan = state.Lines.Line(y, extent);
            float dim = 1f - (state.Hum * (1f + float.CosPi(2f * (((y + 0.5f) / extent.Height) + phase))) / 2f);
            for (int x = 0; x < row.Length; x++) {
                float cell = (column + x + 0.5f) / length;
                float lit = NoiseFunctions.White(new Vector4(MathF.Floor(cell), scan, 0f, 0f), flakes).X < state.SnowDensity
                    ? state.Snow * (1f - MathF.Abs((2f * Kernels.Fraction(cell)) - 1f))
                    : 0f;
                row[x] = new Vector4(Vector3.Lerp(row[x].AsVector3(), Vector3.One, lit) * dim, row[x].W);
            }
        }).Run(frame, progress);
    }
}

[SmartEnum<string>]
[ValidationError<InvalidStylize>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class AnalogVideoParameter : IStateParameter<AnalogVideo> {
    private static readonly (StateParameter<AnalogVideo> Clock, StateParameter<AnalogVideo> Pace) Time =
        Timing.Kinds(Lens<AnalogVideo, Timing>.New(static state => state.Timing, static timing => state => state with { Timing = timing }));
    public static readonly AnalogVideoParameter Matrix = new(
        "matrix", new StateParameter<AnalogVideo>.Choice<YuvMatrix, InvalidStylize>(Lens<AnalogVideo, YuvMatrix>.New(static video => video.Matrix, static matrix => video => video with { Matrix = matrix })));
    public static readonly AnalogVideoParameter Lines = new(
        "lines",
        new StateParameter<AnalogVideo>.Bounded<LineCount, int, InvalidStylize>(
            Lens<AnalogVideo, LineCount>.New(static video => video.Lines, static lines => video => video with { Lines = lines }), new() { Soft = (120, 1080) }));
    public static readonly AnalogVideoParameter LumaBandwidth = new(
        "luma-bandwidth",
        new StateParameter<AnalogVideo>.Bounded<Spread, float, InvalidStylize>(
            Lens<AnalogVideo, Spread>.New(static video => video.LumaBandwidth, static spread => video => video with { LumaBandwidth = spread }), new() { Unit = Units.Fraction, Soft = (0f, 0.005f) }));
    public static readonly AnalogVideoParameter ChromaBandwidth = new(
        "chroma-bandwidth",
        new StateParameter<AnalogVideo>.Bounded<Spread, float, InvalidStylize>(
            Lens<AnalogVideo, Spread>.New(static video => video.ChromaBandwidth, static spread => video => video with { ChromaBandwidth = spread }), new() { Unit = Units.Fraction, Soft = (0f, 0.005f) }));
    public static readonly AnalogVideoParameter ChromaVertical = new(
        "chroma-vertical",
        new StateParameter<AnalogVideo>.Bounded<Spread, float, InvalidStylize>(
            Lens<AnalogVideo, Spread>.New(static video => video.ChromaVertical, static spread => video => video with { ChromaVertical = spread }), new() { Unit = Units.Fraction, Soft = (0f, 0.005f) }));
    public static readonly AnalogVideoParameter ChromaPitch = new(
        "chroma-pitch",
        new StateParameter<AnalogVideo>.Bounded<ShortSideLength, float, InvalidPixelValue>(
            Lens<AnalogVideo, ShortSideLength>.New(static video => video.ChromaPitch, static pitch => video => video with { ChromaPitch = pitch }), new() { Unit = Units.Fraction, Soft = (0f, 0.01f) }));
    public static readonly AnalogVideoParameter CbDelay = new(
        "cb-delay",
        new StateParameter<AnalogVideo>.Bounded<ShortSideOffset, float, InvalidPixelValue>(
            Lens<AnalogVideo, ShortSideOffset>.New(static video => video.CbDelay, static delay => video => video with { CbDelay = delay }),
            new() { Unit = Units.Fraction, Soft = (-0.02f, 0.02f), Origin = (float)ShortSideOffset.Neutral }));
    public static readonly AnalogVideoParameter CrDelay = new(
        "cr-delay",
        new StateParameter<AnalogVideo>.Bounded<ShortSideOffset, float, InvalidPixelValue>(
            Lens<AnalogVideo, ShortSideOffset>.New(static video => video.CrDelay, static delay => video => video with { CrDelay = delay }),
            new() { Unit = Units.Fraction, Soft = (-0.02f, 0.02f), Origin = (float)ShortSideOffset.Neutral }));
    public static readonly AnalogVideoParameter Smear = new(
        "smear",
        new StateParameter<AnalogVideo>.Bounded<Spread, float, InvalidStylize>(
            Lens<AnalogVideo, Spread>.New(static video => video.Smear, static smear => video => video with { Smear = smear }), new() { Unit = Units.Fraction, Soft = (0f, 0.005f) }));
    public static readonly AnalogVideoParameter SmearMix = new(
        "smear-mix",
        new StateParameter<AnalogVideo>.Bounded<Mix, float, InvalidGrade>(Lens<AnalogVideo, Mix>.New(static video => video.SmearMix, static mix => video => video with { SmearMix = mix }), new()));
    public static readonly AnalogVideoParameter Ringing = new(
        "ringing",
        new StateParameter<AnalogVideo>.Bounded<EdgeGain, float, InvalidStylize>(
            Lens<AnalogVideo, EdgeGain>.New(static video => video.Ringing, static gain => video => video with { Ringing = gain }), new() { Soft = (0f, 2f) }));
    public static readonly AnalogVideoParameter RingingDelay = new(
        "ringing-delay",
        new StateParameter<AnalogVideo>.Bounded<ShortSideOffset, float, InvalidPixelValue>(
            Lens<AnalogVideo, ShortSideOffset>.New(static video => video.RingingDelay, static delay => video => video with { RingingDelay = delay }),
            new() { Unit = Units.Fraction, Soft = (-0.02f, 0.02f), Origin = (float)ShortSideOffset.Neutral }));
    public static readonly AnalogVideoParameter Emboss = new(
        "emboss",
        new StateParameter<AnalogVideo>.Bounded<EdgeGain, float, InvalidStylize>(
            Lens<AnalogVideo, EdgeGain>.New(static video => video.Emboss, static gain => video => video with { Emboss = gain }), new() { Soft = (0f, 2f) }));
    public static readonly AnalogVideoParameter Ghost = new(
        "ghost",
        new StateParameter<AnalogVideo>.Bounded<Mix, float, InvalidGrade>(Lens<AnalogVideo, Mix>.New(static video => video.Ghost, static share => video => video with { Ghost = share }), new()));
    public static readonly AnalogVideoParameter GhostDelay = new(
        "ghost-delay",
        new StateParameter<AnalogVideo>.Bounded<ShortSideOffset, float, InvalidPixelValue>(
            Lens<AnalogVideo, ShortSideOffset>.New(static video => video.GhostDelay, static delay => video => video with { GhostDelay = delay }),
            new() { Unit = Units.Fraction, Soft = (-0.05f, 0.05f), Origin = (float)ShortSideOffset.Neutral }));
    public static readonly AnalogVideoParameter CrossColor = new(
        "cross-color",
        new StateParameter<AnalogVideo>.Bounded<Mix, float, InvalidGrade>(Lens<AnalogVideo, Mix>.New(static video => video.CrossColor, static mix => video => video with { CrossColor = mix }), new()));
    public static readonly AnalogVideoParameter LumaNoise = new(
        "luma-noise",
        new StateParameter<AnalogVideo>.Bounded<NoiseLevel, float, InvalidStylize>(
            Lens<AnalogVideo, NoiseLevel>.New(static video => video.LumaNoise, static level => video => video with { LumaNoise = level }), new() { Soft = (0f, 0.2f) }));
    public static readonly AnalogVideoParameter ChromaNoise = new(
        "chroma-noise",
        new StateParameter<AnalogVideo>.Bounded<NoiseLevel, float, InvalidStylize>(
            Lens<AnalogVideo, NoiseLevel>.New(static video => video.ChromaNoise, static level => video => video with { ChromaNoise = level }), new() { Soft = (0f, 0.2f) }));
    public static readonly AnalogVideoParameter NoiseLength = new(
        "noise-length",
        new StateParameter<AnalogVideo>.Bounded<ShortSideExtent, float, InvalidPixelValue>(
            Lens<AnalogVideo, ShortSideExtent>.New(static video => video.NoiseLength, static length => video => video with { NoiseLength = length }),
            new() { Unit = Units.Fraction, Soft = (0.001f, 0.5f), Scale = TrackScale.Log }));
    public static readonly AnalogVideoParameter Snow = new(
        "snow", new StateParameter<AnalogVideo>.Bounded<Mix, float, InvalidGrade>(Lens<AnalogVideo, Mix>.New(static video => video.Snow, static snow => video => video with { Snow = snow }), new()));
    public static readonly AnalogVideoParameter SnowDensity = new(
        "snow-density",
        new StateParameter<AnalogVideo>.Bounded<Likelihood, float, InvalidStylize>(
            Lens<AnalogVideo, Likelihood>.New(static video => video.SnowDensity, static density => video => video with { SnowDensity = density }), new()));
    public static readonly AnalogVideoParameter Interlace = new(
        "interlace",
        new StateParameter<AnalogVideo>.Bounded<ShortSideOffset, float, InvalidPixelValue>(
            Lens<AnalogVideo, ShortSideOffset>.New(static video => video.Interlace, static offset => video => video with { Interlace = offset }),
            new() { Unit = Units.Fraction, Soft = (-0.01f, 0.01f), Origin = (float)ShortSideOffset.Neutral }));
    public static readonly AnalogVideoParameter Jitter = new(
        "jitter",
        new StateParameter<AnalogVideo>.Bounded<ShortSideLength, float, InvalidPixelValue>(
            Lens<AnalogVideo, ShortSideLength>.New(static video => video.Jitter, static jitter => video => video with { Jitter = jitter }), new() { Unit = Units.Fraction, Soft = (0f, 0.01f) }));
    public static readonly AnalogVideoParameter JitterPeriod = new(
        "jitter-period",
        new StateParameter<AnalogVideo>.Bounded<ShortSideExtent, float, InvalidPixelValue>(
            Lens<AnalogVideo, ShortSideExtent>.New(static video => video.JitterPeriod, static period => video => video with { JitterPeriod = period }),
            new() { Unit = Units.Fraction, Soft = (0.001f, 0.5f), Scale = TrackScale.Log }));
    public static readonly AnalogVideoParameter Wrinkle = new(
        "wrinkle",
        new StateParameter<AnalogVideo>.Bounded<ShortSideOffset, float, InvalidPixelValue>(
            Lens<AnalogVideo, ShortSideOffset>.New(static video => video.Wrinkle, static offset => video => video with { Wrinkle = offset }),
            new() { Unit = Units.Fraction, Soft = (-0.05f, 0.05f), Origin = (float)ShortSideOffset.Neutral }));
    public static readonly AnalogVideoParameter WrinkleHeight = new(
        "wrinkle-height",
        new StateParameter<AnalogVideo>.Bounded<ShortSideExtent, float, InvalidPixelValue>(
            Lens<AnalogVideo, ShortSideExtent>.New(static video => video.WrinkleHeight, static height => video => video with { WrinkleHeight = height }),
            new() { Unit = Units.Fraction, Soft = (0.001f, 0.5f), Scale = TrackScale.Log }));
    public static readonly AnalogVideoParameter WrinkleRate = new(
        "wrinkle-rate",
        new StateParameter<AnalogVideo>.Bounded<Frequency, float, InvalidGenerator>(
            Lens<AnalogVideo, Frequency>.New(static video => video.WrinkleRate, static rate => video => video with { WrinkleRate = rate }),
            Frequency.Presentation with { Soft = (-1f, 1f) }));
    public static readonly AnalogVideoParameter HeadSwitch = new(
        "head-switch",
        new StateParameter<AnalogVideo>.Bounded<ShortSideOffset, float, InvalidPixelValue>(
            Lens<AnalogVideo, ShortSideOffset>.New(static video => video.HeadSwitch, static offset => video => video with { HeadSwitch = offset }),
            new() { Unit = Units.Fraction, Soft = (-0.1f, 0.1f), Origin = (float)ShortSideOffset.Neutral }));
    public static readonly AnalogVideoParameter HeadSwitchHeight = new(
        "head-switch-height",
        new StateParameter<AnalogVideo>.Bounded<ShortSideExtent, float, InvalidPixelValue>(
            Lens<AnalogVideo, ShortSideExtent>.New(static video => video.HeadSwitchHeight, static height => video => video with { HeadSwitchHeight = height }),
            new() { Unit = Units.Fraction, Soft = (0.001f, 0.1f), Scale = TrackScale.Log }));
    public static readonly AnalogVideoParameter Bounce = new(
        "bounce",
        new StateParameter<AnalogVideo>.Bounded<ShortSideLength, float, InvalidPixelValue>(
            Lens<AnalogVideo, ShortSideLength>.New(static video => video.Bounce, static bounce => video => video with { Bounce = bounce }), new() { Unit = Units.Fraction, Soft = (0f, 0.02f) }));
    public static readonly AnalogVideoParameter RollRate = new(
        "roll-rate",
        new StateParameter<AnalogVideo>.Bounded<Frequency, float, InvalidGenerator>(
            Lens<AnalogVideo, Frequency>.New(static video => video.RollRate, static rate => video => video with { RollRate = rate }),
            Frequency.Presentation with { Soft = (-1f, 1f) }));
    public static readonly AnalogVideoParameter Hum = new(
        "hum", new StateParameter<AnalogVideo>.Bounded<Mix, float, InvalidGrade>(Lens<AnalogVideo, Mix>.New(static video => video.Hum, static hum => video => video with { Hum = hum }), new()));
    public static readonly AnalogVideoParameter HumRate = new(
        "hum-rate",
        new StateParameter<AnalogVideo>.Bounded<Frequency, float, InvalidGenerator>(
            Lens<AnalogVideo, Frequency>.New(static video => video.HumRate, static rate => video => video with { HumRate = rate }),
            Frequency.Presentation with { Soft = (-1f, 1f) }));
    public static readonly AnalogVideoParameter Seed = new(
        "seed",
        new StateParameter<AnalogVideo>.Bounded<Seed, int, InvalidGenerator>(
            Lens<AnalogVideo, Seed>.New(static video => video.Seed, static seed => video => video with { Seed = seed }), Generators.Seed.Presentation));
    public static readonly AnalogVideoParameter Clock = new("clock", Time.Clock);
    public static readonly AnalogVideoParameter Pace = new("pace", Time.Pace);

    public StateParameter<AnalogVideo> Kind { get; }
}

public sealed record CodecDamage(JpegQuality Quality, JpegColorType ColorType, Generations Generations, Option<LineCount> Coded)
    : IStateRecord<CodecDamage, CodecDamageParameter, InvalidStylize>, IPixelStage<CodecDamage> {
    private const int Tile = 65504;
    private static readonly DecoderOptions Decoding = new() { SkipMetadata = true };

    public static CodecDamage Default { get; } = new(JpegQuality.Standard, JpegColorType.YCbCrRatio420, Generations.Single, None);

    public static CodecDamage Jpeg { get; } = Default with { Quality = JpegQuality.Create(60), Coded = Some(LineCount.Create(360)) };

    public static Option<PixelPass> Pass(CodecDamage state, PassContext context) => Some<PixelPass>(new PixelPass.Frame((frame, progress) => Kernel(state, frame, progress)));

    private static Fin<Unit> Kernel(CodecDamage state, PixelFrame frame, IProgress<int> progress) {
        using Image<RgbaVector> source = frame.Image();
        _ = state.Coded.Iter(lines => source.Mutate(x => x.Resize(0, lines, KnownResamplers.Box)));
        JpegEncoder encoder = new() { Quality = state.Quality, ColorType = state.ColorType };
        foreach (Rectangle tile in
                 from top in Enumerable.Range(0, (source.Height + Tile - 1) / Tile)
                 from left in Enumerable.Range(0, (source.Width + Tile - 1) / Tile)
                 select new Rectangle(left * Tile, top * Tile, int.Min(Tile, source.Width - (left * Tile)), int.Min(Tile, source.Height - (top * Tile)))) {
            using Image<RgbaVector> piece = source.Clone(x => x.Crop(tile));
            using Image<Rgb24> coded = toSeq(Enumerable.Range(2, state.Generations - 1)).Fold(Generation(piece, encoder), (image, _) => {
                using (image)
                    return Generation(image, encoder);
            });
            source.Mutate(x => x.DrawImage(coded, tile.Location, 1f));
        }
        _ = state.Coded.Iter(_ => source.Mutate(x => x.Resize(frame.Size.Width, frame.Size.Height, KnownResamplers.Triangle)));
        frame.Write(source);
        progress.Report(frame.Size.Height);
        return unit;
    }

    private static Image<Rgb24> Generation(Image image, JpegEncoder encoder) {
        using MemoryStream stream = new();
        image.Save(stream, encoder);
        stream.Position = 0;
        return JpegDecoder.Instance.Decode<Rgb24>(Decoding, stream);
    }
}

[SmartEnum<string>]
[ValidationError<InvalidStylize>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CodecDamageParameter : IStateParameter<CodecDamage> {
    public static readonly CodecDamageParameter Quality = new(
        "quality",
        new StateParameter<CodecDamage>.Bounded<JpegQuality, int, InvalidStylize>(
            Lens<CodecDamage, JpegQuality>.New(static codec => codec.Quality, static quality => codec => codec with { Quality = quality }), new() { Step = 1 }));
    public static readonly CodecDamageParameter ColorType = new(
        "color-type",
        new StateParameter<CodecDamage>.Enumerated<JpegColorType>(Lens<CodecDamage, JpegColorType>.New(static codec => codec.ColorType, static type => codec => codec with { ColorType = type })));
    public static readonly CodecDamageParameter Generations = new(
        "generations",
        new StateParameter<CodecDamage>.Bounded<Generations, int, InvalidStylize>(
            Lens<CodecDamage, Generations>.New(static codec => codec.Generations, static generations => codec => codec with { Generations = generations }), new() { Step = 1 }));
    public static readonly CodecDamageParameter Coded = new(
        "coded-lines",
        new StateParameter<CodecDamage>.OptionalBounded<LineCount, int, InvalidStylize>(
            Lens<CodecDamage, Option<LineCount>>.New(static codec => codec.Coded, static lines => codec => codec with { Coded = lines }), new() { Soft = (120, 1080), Step = 1 }));

    public StateParameter<CodecDamage> Kind { get; }
}

public sealed record Glitch(
    ShortSideExtent BlockWidth, ShortSideExtent BlockHeight, Likelihood BlockDensity, ShortSideLength BlockShift, ShortSideLength BlockLift,
    ShortSideLength Tear, ShortSideExtent TearPeriod, ShortSideOffset Split, SignedAngle SplitDirection, MappingJitter SplitJitter,
    WarpShare Slide, YuvMatrix Matrix, Mix LumaCorruption, Mix ChromaCorruption, WrapMode Wrap,
    Seed Seed, Timing Timing, Hold Hold)
    : IStateRecord<Glitch, GlitchParameter, InvalidStylize>, IPixelStage<Glitch> {
    public static Glitch Default { get; } = new(
        ShortSideExtent.Create(1f / (5f * 0.1f)), ShortSideExtent.Create(1f / 5f), Likelihood.Off, ShortSideLength.Neutral, ShortSideLength.Neutral,
        ShortSideLength.Neutral, ShortSideExtent.Create(MathF.Tau / 20f), ShortSideOffset.Neutral, SignedAngle.Neutral, MappingJitter.Neutral,
        WarpShare.Neutral, YuvMatrix.Rec601, Mix.MinValue, Mix.MinValue, WrapMode.Periodic,
        Seed.MinValue, Timing.Standard, Hold.MinValue);

    public static Glitch Corruption { get; } = Default with {
        BlockWidth = ShortSideExtent.Create(0.03f * 16f / 9f),
        BlockHeight = ShortSideExtent.Create(0.03f * 16f / 9f),
        BlockDensity = Likelihood.Create(0.2f),
        BlockShift = ShortSideLength.Create(0.4f * 0.01f * 16f / 9f),
        ChromaCorruption = Mix.Create(0.6f),
        Hold = Hold.Create(12f / 24f),
    };

    public static Glitch Displaced { get; } = Default with {
        BlockDensity = Likelihood.Create(0.2f),
        BlockShift = ShortSideLength.Create(0.05f * 16f / 9f),
        Split = ShortSideOffset.Create(0.6f * 0.05f * 16f / 9f),
        Hold = Hold.Create(2f / 24f),
    };

    private bool Blocks => BlockDensity != Likelihood.Off && (BlockShift != ShortSideLength.Neutral || BlockLift != ShortSideLength.Neutral);

    private bool Moves => Blocks || Tear != ShortSideLength.Neutral;

    private bool Corrupts => BlockDensity != Likelihood.Off && (LumaCorruption != Mix.MinValue || ChromaCorruption != Mix.MinValue);

    public static Option<PixelPass> Pass(Glitch state, PassContext context) =>
        state.Moves || state.Split != ShortSideOffset.Neutral || state.Corrupts
            ? Some<PixelPass>(new PixelPass.Frame((frame, progress) => Kernel(state, context, frame, progress)))
            : None;

    private static Fin<Unit> Kernel(Glitch state, PassContext context, PixelFrame frame, IProgress<int> progress) {
        PixelExtent extent = context.Extent;
        uint field = CoordinateHash.Field(NoiseStream.Glitch, state.Seed, state.Hold.Period(state.Timing.At(context)));
        Vector4 draw = NoiseFunctions.White(Vector4.Zero, CoordinateHash.Branch(field, 3u));
        CoordinateMap blocks = new CoordinateMap.Analytic(Displacement(state, extent, field, draw.X), Sampling.Nearest, state.Wrap);
        Fin<Unit> moved = (state.Split != ShortSideOffset.Neutral, state.Moves) switch {
            (true, _) => Splits(state, extent, draw.Y, blocks)(frame, progress),
            (false, true) => blocks.Apply(frame, progress),
            (false, false) => unit,
        };
        return moved.Bind(_ => state.Corrupts ? new PixelPass.Pointwise(Corrupted(state, extent, field)).Run(frame, progress) : unit);
    }

    private static Action<Span<Vector2>, Span<float>, PixelExtent> Displacement(Glitch state, PixelExtent extent, uint field, float phase) {
        (Vector2 centre, float side) = (new Vector2(extent.Width, extent.Height) / 2f, extent.ShortSide);
        (uint cells, uint draws) = (CoordinateHash.Branch(field, 0u), CoordinateHash.Branch(field, 1u));
        (float tear, float period) = (state.Tear, state.TearPeriod);
        return (points, _, _) => {
            foreach (ref Vector2 point in points) {
                Vector2 q = (point - centre) / side;
                Vector2 offset = state.Blocks && Cell(state, q, cells, draws) is (true, _, var cell)
                    ? new Vector2(state.BlockShift * ((2f * cell.Y) - 1f), state.BlockLift * ((2f * cell.Z) - 1f))
                    : Vector2.Zero;
                point = centre + (side * (q - offset - new Vector2(tear * (Kernels.Fraction((q.Y / period) + phase) - 0.5f), 0f)));
            }
        };
    }

    private static Func<PixelFrame, IProgress<int>, Fin<Unit>> Splits(Glitch state, PixelExtent extent, float draw, CoordinateMap blocks) {
        float reach = state.Split.Pixels(extent) * (1f - state.SplitJitter + (state.SplitJitter * ((2f * draw) - 1f)));
        Vector2 direction = float.SinCos(state.SplitDirection) switch { var (sin, cos) => new Vector2(cos, -sin) };
        return CoordinateMap.Integrated(tau => {
            int channel = int.Min((int)(3f * tau), 2);
            float shift = reach * float.Lerp(1f - state.Slide, 1f, (3f * tau) - channel);
            (Vector2 offset, Vector4 lanes) = channel switch {
                0 => (shift * direction, Vector4.UnitX),
                1 => (-shift * direction, Vector4.UnitZ),
                _ => (Vector2.Zero, new Vector4(0f, 1f, 0f, 1f)),
            };
            CoordinateMap moved = new CoordinateMap.Analytic(
                (points, _, _) => {
                    foreach (ref Vector2 point in points)
                        point -= offset;
                }, Sampling.Linear, state.Wrap);
            return (state.Moves ? blocks.Then(moved) : moved, lanes);
        });
    }

    private static Action<Span<Vector4>, int, int> Corrupted(Glitch state, PixelExtent extent, uint field) {
        ChromaAxes axes = new(state.Matrix.Weights());
        (Matrix4x4 forward, Matrix4x4 inverse) = (axes.Forward, axes.Inverse);
        (Vector2 centre, float side) = (new Vector2(extent.Width, extent.Height) / 2f, extent.ShortSide);
        (uint cells, uint draws, uint powers) = (CoordinateHash.Branch(field, 0u), CoordinateHash.Branch(field, 1u), CoordinateHash.Branch(field, 2u));
        (float luma, float chroma) = (state.LumaCorruption, state.ChromaCorruption);
        return (row, column, line) => {
            float y = extent.Height - line - 0.5f;
            for (int x = 0; x < row.Length; x++) {
                Vector2 power = Cell(state, (new Vector2(column + x + 0.5f, y) - centre) / side, cells, draws) is (true, var site, _)
                    ? NoiseFunctions.White(site, powers) switch { var e => new Vector2(float.Exp2((4f * e.X) - 2f), float.Exp2((4f * e.Y) - 2f)) }
                    : Vector2.One;
                Vector4 ycc = Vector4.Transform(row[x], forward);
                row[x] = Vector4.Transform(
                    new Vector4(
                        float.Lerp(ycc.X, float.CopySign(float.Pow(float.Abs(ycc.X), power.X), ycc.X), luma),
                        float.Lerp(ycc.Y, float.CopySign(0.5f * float.Pow(float.Abs(2f * ycc.Y), power.Y), ycc.Y), chroma),
                        float.Lerp(ycc.Z, float.CopySign(0.5f * float.Pow(float.Abs(2f * ycc.Z), power.Y), ycc.Z), chroma),
                        ycc.W),
                    inverse);
            }
        };
    }

    private static (bool Active, Vector4 Site, Vector4 Draw) Cell(Glitch state, Vector2 q, uint cells, uint draws) {
        Vector4 site = NoiseDimensions.Two.F1(new Vector4(q.X / state.BlockWidth, q.Y / state.BlockHeight, 0f, 0f), Octaves.Plain, Cellular.Default, cells).Site;
        Vector4 draw = NoiseFunctions.White(site, draws);
        return (draw.X < state.BlockDensity, site, draw);
    }
}

[SmartEnum<string>]
[ValidationError<InvalidStylize>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class GlitchParameter : IStateParameter<Glitch> {
    private static readonly (StateParameter<Glitch> Clock, StateParameter<Glitch> Pace) Time =
        Timing.Kinds(Lens<Glitch, Timing>.New(static state => state.Timing, static timing => state => state with { Timing = timing }));
    public static readonly GlitchParameter BlockWidth = new(
        "block-width",
        new StateParameter<Glitch>.Bounded<ShortSideExtent, float, InvalidPixelValue>(
            Lens<Glitch, ShortSideExtent>.New(static glitch => glitch.BlockWidth, static width => glitch => glitch with { BlockWidth = width }),
            new() { Unit = Units.Fraction, Soft = (0.01f, 4f), Scale = TrackScale.Log }));
    public static readonly GlitchParameter BlockHeight = new(
        "block-height",
        new StateParameter<Glitch>.Bounded<ShortSideExtent, float, InvalidPixelValue>(
            Lens<Glitch, ShortSideExtent>.New(static glitch => glitch.BlockHeight, static height => glitch => glitch with { BlockHeight = height }),
            new() { Unit = Units.Fraction, Soft = (0.01f, 4f), Scale = TrackScale.Log }));
    public static readonly GlitchParameter BlockDensity = new(
        "block-density",
        new StateParameter<Glitch>.Bounded<Likelihood, float, InvalidStylize>(
            Lens<Glitch, Likelihood>.New(static glitch => glitch.BlockDensity, static density => glitch => glitch with { BlockDensity = density }), new()));
    public static readonly GlitchParameter BlockShift = new(
        "block-shift",
        new StateParameter<Glitch>.Bounded<ShortSideLength, float, InvalidPixelValue>(
            Lens<Glitch, ShortSideLength>.New(static glitch => glitch.BlockShift, static shift => glitch => glitch with { BlockShift = shift }), new() { Unit = Units.Fraction, Soft = (0f, 0.5f) }));
    public static readonly GlitchParameter BlockLift = new(
        "block-lift",
        new StateParameter<Glitch>.Bounded<ShortSideLength, float, InvalidPixelValue>(
            Lens<Glitch, ShortSideLength>.New(static glitch => glitch.BlockLift, static lift => glitch => glitch with { BlockLift = lift }), new() { Unit = Units.Fraction, Soft = (0f, 0.1f) }));
    public static readonly GlitchParameter Tear = new(
        "tear",
        new StateParameter<Glitch>.Bounded<ShortSideLength, float, InvalidPixelValue>(
            Lens<Glitch, ShortSideLength>.New(static glitch => glitch.Tear, static tear => glitch => glitch with { Tear = tear }), new() { Unit = Units.Fraction, Soft = (0f, 0.1f) }));
    public static readonly GlitchParameter TearPeriod = new(
        "tear-period",
        new StateParameter<Glitch>.Bounded<ShortSideExtent, float, InvalidPixelValue>(
            Lens<Glitch, ShortSideExtent>.New(static glitch => glitch.TearPeriod, static period => glitch => glitch with { TearPeriod = period }),
            new() { Unit = Units.Fraction, Soft = (0.01f, 1f), Scale = TrackScale.Log }));
    public static readonly GlitchParameter Split = new(
        "split",
        new StateParameter<Glitch>.Bounded<ShortSideOffset, float, InvalidPixelValue>(
            Lens<Glitch, ShortSideOffset>.New(static glitch => glitch.Split, static split => glitch => glitch with { Split = split }),
            new() { Unit = Units.Fraction, Soft = (-0.05f, 0.05f), Origin = (float)ShortSideOffset.Neutral }));
    public static readonly GlitchParameter SplitDirection = new(
        "split-direction",
        new StateParameter<Glitch>.Bounded<SignedAngle, float, InvalidPixelValue>(
            Lens<Glitch, SignedAngle>.New(static glitch => glitch.SplitDirection, static direction => glitch => glitch with { SplitDirection = direction }),
            SignedAngle.Presentation));
    public static readonly GlitchParameter SplitJitter = new(
        "split-jitter",
        new StateParameter<Glitch>.Bounded<MappingJitter, float, InvalidStylize>(
            Lens<Glitch, MappingJitter>.New(static glitch => glitch.SplitJitter, static jitter => glitch => glitch with { SplitJitter = jitter }), new()));
    public static readonly GlitchParameter Slide = new(
        "slide",
        new StateParameter<Glitch>.Bounded<WarpShare, float, InvalidWarp>(Lens<Glitch, WarpShare>.New(static glitch => glitch.Slide, static slide => glitch => glitch with { Slide = slide }), new()));
    public static readonly GlitchParameter Matrix = new(
        "matrix", new StateParameter<Glitch>.Choice<YuvMatrix, InvalidStylize>(Lens<Glitch, YuvMatrix>.New(static glitch => glitch.Matrix, static matrix => glitch => glitch with { Matrix = matrix })));
    public static readonly GlitchParameter LumaCorruption = new(
        "luma-corruption",
        new StateParameter<Glitch>.Bounded<Mix, float, InvalidGrade>(
            Lens<Glitch, Mix>.New(static glitch => glitch.LumaCorruption, static corruption => glitch => glitch with { LumaCorruption = corruption }), new()));
    public static readonly GlitchParameter ChromaCorruption = new(
        "chroma-corruption",
        new StateParameter<Glitch>.Bounded<Mix, float, InvalidGrade>(
            Lens<Glitch, Mix>.New(static glitch => glitch.ChromaCorruption, static corruption => glitch => glitch with { ChromaCorruption = corruption }), new()));
    public static readonly GlitchParameter Wrap = new(
        "wrap", new StateParameter<Glitch>.Choice<WrapMode, InvalidPixelValue>(Lens<Glitch, WrapMode>.New(static glitch => glitch.Wrap, static wrap => glitch => glitch with { Wrap = wrap })));
    public static readonly GlitchParameter Seed = new(
        "seed",
        new StateParameter<Glitch>.Bounded<Seed, int, InvalidGenerator>(
            Lens<Glitch, Seed>.New(static glitch => glitch.Seed, static seed => glitch => glitch with { Seed = seed }), Generators.Seed.Presentation));
    public static readonly GlitchParameter Clock = new("clock", Time.Clock);
    public static readonly GlitchParameter Pace = new("pace", Time.Pace);
    public static readonly GlitchParameter Hold = new(
        "hold",
        new StateParameter<Glitch>.Bounded<Hold, float, InvalidGenerator>(
            Lens<Glitch, Hold>.New(static glitch => glitch.Hold, static hold => glitch => glitch with { Hold = hold }), Generators.Hold.Presentation));

    public StateParameter<Glitch> Kind { get; }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
file static class SystemM {
    public const float Subcarrier = 188.3f;

    public static long Frame(Clock now) => (long)Math.Floor(now * 30000d / 1001d);
}

file static class Kernels {
    public static System.Drawing.Point Anchor { get; } = new(-1, -1);

    public static float Fraction(float value) => value - MathF.Floor(value);

    public static int Taps(float sigma) => (2 * (int)MathF.Ceiling(3f * sigma)) + 1;

    public static Mat Impulse() => Mat.Ones(1, 1, DepthType.Cv32F, 1);

    public static Mat Gauss(float sigma) => CvInvoke.GetGaussianKernel(Taps(sigma), sigma, DepthType.Cv32F);

    public static Mat Unit(float sigma) {
        Mat taps = Gauss(sigma);
        taps.ConvertTo(taps, DepthType.Cv32F, 1d / Math.Sqrt(taps.Dot(taps)));
        return taps;
    }

    public static Mat Echo(float sigma, float delay, float share) {
        int reach = (int)MathF.Ceiling(MathF.Abs(delay) + (3f * sigma));
        float[] shape = [.. Enumerable.Range(-reach, (2 * reach) + 1).Select(k => sigma > 0f ? MathF.Exp(-(k + delay) * (k + delay) / (2f * sigma * sigma)) : float.Max(0f, 1f - MathF.Abs(k + delay)))];
        float total = shape.Sum();
        Mat taps = new(1, shape.Length, DepthType.Cv32F, 1);
        taps.SetTo([.. shape.Select((weight, index) => (share * weight / total) + (index == reach ? 1f - share : 0f))]);
        return taps;
    }

    public static Mat Transform(Matrix4x4 rows) {
        Matrix4x4 columns = Matrix4x4.Transpose(rows);
        Mat matrix = new(4, 4, DepthType.Cv32F, 1);
        matrix.SetTo(MemoryMarshal.CreateReadOnlySpan(ref columns.M11, 16).ToArray());
        return matrix;
    }

    public static void Along(Mat plane, Mat taps) {
        using Mat impulse = Impulse();
        CvInvoke.SepFilter2D(plane, plane, DepthType.Cv32F, taps, impulse, Anchor, 0d, BorderType.Replicate);
    }

    public static void Across(Mat plane, Mat taps) {
        using Mat impulse = Impulse();
        CvInvoke.SepFilter2D(plane, plane, DepthType.Cv32F, impulse, taps, Anchor, 0d, BorderType.Replicate);
    }
}
