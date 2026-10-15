using System.Numerics;
using System.Runtime.Intrinsics;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Util;
using MathNet.Numerics.Integration;
using MathNet.Numerics.LinearAlgebra;
using PeterO.Numbers;
using Rasm.Imaging.Color;
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
using TorchSharp;

namespace Rasm.Imaging.Filters.Stylize;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<float>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Off", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidStylize>]
public readonly partial struct EdgeGain : IMinMaxValue<EdgeGain> {
    public static EdgeGain MinValue => Off;
    public static EdgeGain MaxValue { get; } = new(float.MaxValue);

    static partial void ValidateFactoryArguments(ref InvalidStylize? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidStylize();
}

[ValueObject<float>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Off", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidStylize>]
public readonly partial struct NoiseLevel : IMinMaxValue<NoiseLevel> {
    public static NoiseLevel MinValue => Off;
    public static NoiseLevel MaxValue { get; } = new(float.MaxValue);

    static partial void ValidateFactoryArguments(ref InvalidStylize? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidStylize();
}

[ValueObject<int>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidStylize>]
public readonly partial struct LineCount : IMinMaxValue<LineCount> {
    public static LineCount MinValue { get; } = new(1);
    public static LineCount MaxValue { get; } = new(int.MaxValue);
    public static LineCount Ntsc { get; } = new(480);
    public static LineCount Coarse { get; } = new(120);

    public EInteger Line(long row, PixelExtent extent) =>
        NoiseFunctions.Split(ERational.FromInt64(row).Multiply(_value).Divide(extent.Height)).Cell;

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
    public static Generations MaxValue { get; } = new(int.MaxValue);
    public static Generations Single { get; } = new(1);

    static partial void ValidateFactoryArguments(ref InvalidStylize? validationError, ref int value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidStylize();
}

[SmartEnum<string>]
[ValidationError<InvalidStylize>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class YuvMatrix {
    public static readonly YuvMatrix Rec601 = new("rec601", static () => (0.299d, 0.587d, 0.114d));
    public static readonly YuvMatrix Rec709 = new("rec709", static () => Gamut.StandardRgb.Luminance);

    [UseDelegateFromConstructor]
    public partial (double X, double Y, double Z) Weights();
}

[SmartEnum<string>]
[ValidationError<InvalidStylize>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PhosphorMask {
    public static readonly PhosphorMask ApertureGrille = new("aperture-grille", gap: 0d, columnStagger: 0d, rowStagger: 0d);
    public static readonly PhosphorMask SlotMask = new("slot-mask", gap: 1d / 3d, columnStagger: 0.5d, rowStagger: 0d);
    public static readonly PhosphorMask ShadowMask = new("shadow-mask", gap: 1d / 3d, columnStagger: 0d, rowStagger: 0.5d);

    private readonly double _gap;
    private readonly double _columnStagger;
    private readonly double _rowStagger;

    public (double R, double G, double B) Coverage(long column, long row, ShortSideExtent pitch, PixelExtent extent) {
        ERational span = ERational.One.Divide(ERational.FromSingle(pitch).Multiply(extent.ShortSide));
        (ERational u, ERational v, ERational width) =
            (ERational.FromInt64(column).Multiply(span), ERational.FromInt64(row).Multiply(span), ERational.One.Subtract(ERational.FromDouble(_gap)));
        ERational third = ERational.One.Divide(3);
        return (Channel(0), Channel(1), Channel(2));

        double Channel(int channel) => (_rowStagger is 0d
            ? Area(u, 2, channel, 0, third).Multiply(Area(v, 1, 0, 0, width))
                .Add(Area(u, 2, channel, 1, third).Multiply(Area(v.Add(ERational.FromDouble(_columnStagger)), 1, 0, 0, width)))
            : Area(u, 1, channel, 0, third).Multiply(Area(v, 2, 0, 0, width))
                .Add(Area(u.Add(ERational.FromDouble(_rowStagger)), 1, channel, 0, third).Multiply(Area(v, 2, 0, 1, width))))
            .Multiply(3).Divide(span.Multiply(span).Multiply(width)).ToDouble();
        ERational Area(ERational t, int period, int channel, int parity, ERational open) {
            return Primitive(t.Add(span)).Subtract(Primitive(t));

            ERational Primitive(ERational point) => NoiseFunctions.Split(point.Divide(period)) switch {
                var (cell, local) => ERational.FromEInteger(cell).Multiply(open).Add(
                    ERational.Min(ERational.Max(local.Multiply(period).Subtract(parity).Subtract(third.Multiply(channel)), ERational.Zero), open)),
            };
        }
    }
}

public sealed record CrtDisplay(
    PhosphorMask Mask, ShortSideExtent Pitch, Mix MaskStrength, LineCount Lines, bool Interlaced, Mix ScanlineDepth,
    Mix Collapse, ShortSideLength Glow, Distortion Curvature, LensFraming Framing, Timing Timing)
    : IStateRecord<CrtDisplay, CrtDisplayParameter, InvalidStylize>, IPixelStage<CrtDisplay> {
    private const double NearShare = 0.890909d;

    public static CrtDisplay Default { get; } = new(
        PhosphorMask.SlotMask, ShortSideExtent.Create(8f / ReferenceFrame.Height), Mix.Create(0.8f), LineCount.Ntsc, Interlaced: false, Mix.Full,
        Mix.MinValue, ShortSideLength.Neutral, Distortion.Neutral, LensFraming.Edges, Timing.Default);

    public static CrtDisplay Monitor { get; } = Default with {
        Mask = PhosphorMask.ApertureGrille,
        Pitch = ShortSideExtent.Create(ReferenceFrame.GreaterSide / 640f),
        MaskStrength = Mix.Create(0.705394f),
        Lines = LineCount.Create(240),
        ScanlineDepth = Mix.Create(0.5f),
        Glow = ShortSideLength.Create(0.01f * ReferenceFrame.GreaterSide / 3f),
        Curvature = Distortion.Create(0.0497677f),
    };

    public static CrtDisplay Scanlines { get; } = Default with { MaskStrength = Mix.MinValue, ScanlineDepth = Mix.Create(0.4f) };

    private bool Lit => MaskStrength != Mix.MinValue || ScanlineDepth != Mix.MinValue || Collapse != Mix.MinValue;

    private (ERational X, ERational Y) Scales =>
        (ERational.FromSingle(Collapse).Multiply(2), ERational.One.Divide((int)Lines)) switch {
            var (collapse, line) => (ERational.Min(ERational.One, ERational.Max(ERational.FromInt32(2).Subtract(collapse), line)),
                ERational.Max(ERational.One.Subtract(collapse), line)),
        };

    public static Option<PixelPass> Pass(CrtDisplay state, PassContext context) =>
        (state.Lit, state.Collapse == Mix.MinValue && state.Glow == ShortSideLength.Neutral && state.Curvature == Distortion.Neutral) switch {
            (false, true) => None,
            (true, true) => Some<PixelPass>(new PixelPass.Pointwise(Raster(state, context))),
            _ => Some<PixelPass>(new PixelPass.Frame((frame, progress) => Kernel(state, context, frame, progress))),
        };

    private static Fin<Unit> Kernel(CrtDisplay state, PassContext context, PixelFrame frame, IProgress<int> progress) =>
        (state.Collapse == Mix.MinValue ? unit : new CoordinateMap.Analytic(Deflection(state.Scales, context.Extent), Sampling.Linear, (WrapMode.Black, WrapMode.Black)).Apply(frame, progress))
            .Bind(_ => state.Glow == ShortSideLength.Neutral ? unit : Bloom(frame, state.Glow.Pixels(context.Extent)))
            .Bind(_ => state.Lit ? new PixelPass.Pointwise(Raster(state, context)).Run(frame, progress) : unit)
            .Bind(_ => state.Curvature == Distortion.Neutral ? unit
                : new LensDistortion(state.Curvature, AxisFraction.MinValue, state.Framing, LensDirection.Distort, WrapMode.Black).Placed.Apply(frame, progress));

    private static Action<Span<(double X, double Y)>, Span<float>, PixelExtent> Deflection((ERational X, ERational Y) scale, PixelExtent extent) {
        (ERational X, ERational Y) = (ERational.FromInt32(extent.Width).Divide(2), ERational.FromInt32(extent.Height).Divide(2));
        return (points, _, _) => {
            foreach (ref (double X, double Y) point in points)
                point = (Sampling.Linear.Point(X.Add(ERational.FromDouble(point.X).Subtract(X).Divide(scale.X)), WrapMode.Black, extent.Width),
                    Sampling.Linear.Point(Y.Add(ERational.FromDouble(point.Y).Subtract(Y).Divide(scale.Y)), WrapMode.Black, extent.Height));
        };
    }

    private static Fin<Unit> Bloom(PixelFrame frame, double sigma) =>
        frame.Filter(header => (
        from taps in PixelSampling.GaussianTaps<long>(sigma)
        from wide in PixelSampling.GaussianTaps<long>(4d * sigma)
        select (taps, wide)).Bind(support => {
            using Mat alpha = new();
            using Mat light = new();
            using Mat near = new();
            using Mat far = new();
            CvInvoke.ExtractChannel(header, alpha, 3);
            header.ConvertTo(light, DepthType.Cv64F);
            return light.Gaussian(near, (support.taps, support.taps), sigma, sigma, PaddingModes.Replicate)
                .Bind(_ => near.Gaussian(far, (support.wide, support.taps), 4d * sigma, sigma, PaddingModes.Replicate))
                .Map(_ => {
                    CvInvoke.AddWeighted(near, NearShare, far, 1d - NearShare, 0d, light);
                    light.ConvertTo(header, DepthType.Cv32F);
                    CvInvoke.InsertChannel(alpha, header, 3);
                    return unit;
                });
        }));

    private static Action<Span<Vector4>, long, long> Raster(CrtDisplay state, PassContext context) {
        PixelExtent extent = context.Extent;
        (ERational width, ERational height) = state.Scales;
        ERational middle = ERational.FromInt32(extent.Height).Divide(2);
        ERational field = ERational.FromInt32(extent.Height).Multiply(state.Interlaced ? 2 : 1);
        ERational parity = ERational.FromInt64(state.Interlaced ? SystemM.Frame(state.Timing.At(context)) & 1L : 0L).Divide(2);
        (double mask, double depth) = (state.MaskStrength, state.ScanlineDepth);
        double power = ERational.One.Divide(width.Multiply(height)).ToDouble();
        ERational span = ERational.FromInt32(state.Lines).Divide(field.Multiply(height));
        ERational phase = middle.Multiply((int)state.Lines).Divide(field).Add(parity);
        (EInteger whole, ERational fraction) = NoiseFunctions.Split(span);
        double sinc = (whole.IsEven ? 1d : -1d) * double.SinPi(ERational.Min(fraction, ERational.One.Subtract(fraction)).ToDouble()) / (2d * double.Pi * span.ToDouble());
        return (row, column, line) => {
            long y = (long)extent.Height - 1 - line;
            ERational centre = ERational.FromInt64((2 * y) + 1).Divide(2).Subtract(middle);
            double beam = NoiseFunctions.Split(centre.Multiply(span).Add(phase)).Local.ToDouble();
            double average = 0.5d + (double.CosPi(2d * beam) * sinc);
            double lit = power * (1d - (depth * average)) / (1d - (depth / 2d));
            for (int x = 0; x < row.Length; x++) {
                (double R, double G, double B) = mask is 0d ? (1d, 1d, 1d) : state.Mask.Coverage(column + x, y, state.Pitch, extent);
                row[x] = new Vector4(
                    (float)(row[x].X * (1d - mask + (mask * R)) * lit),
                    (float)(row[x].Y * (1d - mask + (mask * G)) * lit),
                    (float)(row[x].Z * (1d - mask + (mask * B)) * lit), row[x].W);
            }
        };
    }
}

[SmartEnum<string>]
[ValidationError<InvalidStylize>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CrtDisplayParameter : IStateParameter<CrtDisplay> {
    public static readonly CrtDisplayParameter Mask = new(
        "mask", new StateParameter<CrtDisplay>.Choice<PhosphorMask, InvalidStylize>(Lens<CrtDisplay, PhosphorMask>.New(static crt => crt.Mask, static mask => crt => crt with { Mask = mask })));
    public static readonly CrtDisplayParameter Pitch = new(
        "pitch",
        StateParameter.Bounded<CrtDisplay, ShortSideExtent, float, InvalidPixelValue>(Lens<CrtDisplay, ShortSideExtent>.New(static crt => crt.Pitch, static pitch => crt => crt with { Pitch = pitch }),
        ShortSideExtent.Presentation with { Soft = (0.002f, 0.02f) }));
    public static readonly CrtDisplayParameter MaskStrength = new(
        "mask-strength",
        StateParameter.Bounded<CrtDisplay, Mix, float, InvalidGrade>(Lens<CrtDisplay, Mix>.New(static crt => crt.MaskStrength, static strength => crt => crt with { MaskStrength = strength }), new()));
    public static readonly CrtDisplayParameter Lines = new(
        "lines",
        StateParameter.Bounded<CrtDisplay, LineCount, int, InvalidStylize>(Lens<CrtDisplay, LineCount>.New(static crt => crt.Lines, static lines => crt => crt with { Lines = lines }), new() { Soft = (LineCount.Coarse, 1080) }));
    public static readonly CrtDisplayParameter Interlaced = new(
        "interlaced", new StateParameter<CrtDisplay>.Toggle(Lens<CrtDisplay, bool>.New(static crt => crt.Interlaced, static interlaced => crt => crt with { Interlaced = interlaced })));
    public static readonly CrtDisplayParameter ScanlineDepth = new(
        "scanline-depth",
        StateParameter.Bounded<CrtDisplay, Mix, float, InvalidGrade>(Lens<CrtDisplay, Mix>.New(static crt => crt.ScanlineDepth, static depth => crt => crt with { ScanlineDepth = depth }), new()));
    public static readonly CrtDisplayParameter Collapse = new(
        "collapse",
        StateParameter.Bounded<CrtDisplay, Mix, float, InvalidGrade>(Lens<CrtDisplay, Mix>.New(static crt => crt.Collapse, static collapse => crt => crt with { Collapse = collapse }), new()));
    public static readonly CrtDisplayParameter Glow = new(
        "glow",
        StateParameter.Bounded<CrtDisplay, ShortSideLength, float, InvalidPixelValue>(Lens<CrtDisplay, ShortSideLength>.New(static crt => crt.Glow, static glow => crt => crt with { Glow = glow }), ShortSideLength.Presentation with { Soft = (0f, 0.005f) }));
    public static readonly CrtDisplayParameter Curvature = new(
        "curvature",
        StateParameter.Bounded<CrtDisplay, Distortion, float, InvalidOptics>(Lens<CrtDisplay, Distortion>.New(static crt => crt.Curvature, static curvature => crt => crt with { Curvature = curvature }), new() { Origin = (float)Distortion.Neutral }));
    public static readonly CrtDisplayParameter Framing = new(
        "framing", new StateParameter<CrtDisplay>.Choice<LensFraming, InvalidOptics>(Lens<CrtDisplay, LensFraming>.New(static crt => crt.Framing, static framing => crt => crt with { Framing = framing })));
    public static readonly CrtDisplayParameter Timing = new("timing", new StateParameter<CrtDisplay>.Record<Timing>(
        Lens<CrtDisplay, Timing>.New(static state => state.Timing, static timing => state => state with { Timing = timing })));

    public StateParameter<CrtDisplay> Kind { get; }
}

public sealed record AnalogVideo(
    YuvMatrix Matrix, LineCount Lines,
    ShortSideLength LumaBandwidth, ShortSideLength ChromaBandwidth, ShortSideLength ChromaVertical, ShortSideLength ChromaPitch, ShortSideOffset ChromaDelay,
    ShortSideLength Smear, Mix SmearMix, EdgeGain Ringing, ShortSideOffset RingingDelay, EdgeGain Emboss, Mix Ghost, ShortSideOffset GhostDelay, Mix CrossColor,
    NoiseLevel LumaNoise, NoiseLevel ChromaNoise, ShortSideExtent NoiseLength, Mix Snow, Likelihood SnowDensity, ShortSideExtent SnowLength,
    ShortSideOffset Interlace, ShortSideLength Jitter, ShortSideExtent JitterPeriod, ShortSideOffset Wrinkle, ShortSideExtent WrinkleHeight, Frequency WrinkleRate,
    ShortSideOffset HeadSwitch, ShortSideExtent HeadSwitchHeight, ShortSideLength Bounce, Frequency RollRate, Mix Hum, Frequency HumRate,
    Seed Seed, Timing Timing)
    : IStateRecord<AnalogVideo, AnalogVideoParameter, InvalidStylize>, IPixelStage<AnalogVideo> {
    private const double SwitchCrest = 0.9d;

    public static AnalogVideo Default { get; } = new(
        YuvMatrix.Rec601, LineCount.Ntsc,
        ShortSideLength.Neutral, ShortSideLength.Neutral, ShortSideLength.Neutral, ShortSideLength.Neutral, ShortSideOffset.Neutral,
        ShortSideLength.Neutral, Mix.Create(0.3f * 0.25f), EdgeGain.Off, ShortSideOffset.Neutral, EdgeGain.Off, Mix.MinValue, ShortSideOffset.Neutral, Mix.MinValue,
        NoiseLevel.Off, NoiseLevel.Off, ShortSideExtent.Create(ReferenceFrame.GreaterSide / 40f), Mix.MinValue, Likelihood.Off, ShortSideExtent.Create(0.04f * ReferenceFrame.GreaterSide),
        ShortSideOffset.Neutral, ShortSideLength.Neutral, ShortSideExtent.Create(1f / 65f), ShortSideOffset.Neutral, ShortSideExtent.Create(0.15f), Frequency.Neutral,
        ShortSideOffset.Neutral, ShortSideExtent.Create(0.31f / (32f - (28.8f * 0.5f))), ShortSideLength.Neutral, Frequency.Neutral, Mix.MinValue, Frequency.Create(60f / 1001f),
        Seed.MinValue, Timing.Default);

    public static AnalogVideo ChromaBlur { get; } = Default with {
        ChromaBandwidth = ShortSideLength.Create(128f / ReferenceFrame.Height / 3f),
        ChromaVertical = ShortSideLength.Create(128f * 0.1f / ReferenceFrame.Height / 3f),
    };

    private bool Moves =>
        Interlace != ShortSideOffset.Neutral || Jitter != ShortSideLength.Neutral || Wrinkle != ShortSideOffset.Neutral
        || HeadSwitch != ShortSideOffset.Neutral || Bounce != ShortSideLength.Neutral || RollRate != Frequency.Neutral;

    private bool Filters(PixelExtent extent) =>
        LumaBandwidth != ShortSideLength.Neutral || ChromaBandwidth != ShortSideLength.Neutral || ChromaVertical != ShortSideLength.Neutral || ChromaPitch.Pixels(extent) > 1d
        || ChromaDelay != ShortSideOffset.Neutral || (Smear != ShortSideLength.Neutral && SmearMix != Mix.MinValue) || Ghost != Mix.MinValue || CrossColor != Mix.MinValue
        || LumaNoise != NoiseLevel.Off || ChromaNoise != NoiseLevel.Off;

    private bool Speckled => (Snow != Mix.MinValue && SnowDensity != Likelihood.Off) || Hum != Mix.MinValue;

    public static Option<PixelPass> Pass(AnalogVideo state, PassContext context) =>
        state.Moves || state.Filters(context.Extent) || state.Speckled
            ? Some<PixelPass>(new PixelPass.Frame((frame, progress) => Kernel(state, context, frame, progress)))
            : None;

    private static Fin<Unit> Kernel(AnalogVideo state, PassContext context, PixelFrame frame, IProgress<int> progress) {
        (PixelExtent extent, Clock now) = (context.Extent, state.Timing.At(context));
        uint field = CoordinateHash.Field(NoiseStream.AnalogVideo, state.Seed, now.Term);
        Fin<Unit> received = state.Moves || state.Filters(extent)
            ? frame.Filter(header => (state.Moves ? Sync(state, extent, now, field, frame, progress) : unit)
                .Bind(_ => state.Filters(extent) ? Signal(state, extent, now, field, frame, header, progress) : unit))
            : unit;
        return received.Bind(_ => state.Speckled ? Snowfall(state, extent, now, field, frame, progress) : unit);
    }

    private static unsafe Fin<Unit> Sync(AnalogVideo state, PixelExtent extent, Clock now, uint field, PixelFrame frame, IProgress<int> progress) {
        double sigma = ERational.FromSingle(state.JitterPeriod).Multiply(extent.ShortSide).Multiply((int)state.Lines).Divide(extent.Height).ToDouble();
        uint jitterDraws = CoordinateHash.Branch(field, 2u);
        ERational clock = ERational.FromSingle(now);
        ERational rise = NoiseFunctions.Split(ERational.FromSingle(state.RollRate).Multiply(clock)).Local.Multiply(extent.Height);
        ERational jump = ERational.FromSingle(state.Bounce).Multiply(extent.ShortSide)
            .Multiply(ERational.FromSingle(NoiseFunctions.White(Vector4.Zero, CoordinateHash.Branch(field, 3u)).X).Multiply(2).Subtract(1));
        ERational crest = ERational.One.Subtract(NoiseFunctions.Split(ERational.FromSingle(state.WrinkleRate).Multiply(clock)).Local).Multiply(extent.Height);
        (ERational interlace, ERational jitter, ERational wrinkle, ERational wrinkleBand) =
            (ERational.FromSingle(state.Interlace).Multiply(extent.ShortSide), ERational.FromSingle(state.Jitter).Multiply(extent.ShortSide),
                ERational.FromSingle(state.Wrinkle).Multiply(extent.ShortSide), ERational.FromSingle(state.WrinkleHeight).Multiply(extent.ShortSide));
        (ERational switching, ERational switchBand) =
            (ERational.FromSingle(state.HeadSwitch).Multiply(extent.ShortSide), ERational.FromSingle(state.HeadSwitchHeight).Multiply(extent.ShortSide));
        return (state.Jitter == ShortSideLength.Neutral ? Kernels.GaussianKernel(0d) : VideoKernels.Unit(sigma)).Bind(taps => {
            using (taps)
            using (torch.Storage<double> storage = taps.storage<double>())
                return new CoordinateMap.Composed(
            new CoordinateMap.Composed(
                new CoordinateMap.Analytic(
                    (points, _, _) => {
                        foreach (ref (double X, double Y) point in points)
                            point.Y = Sampling.Linear.Point(ERational.FromDouble(point.Y).Add(rise), WrapMode.Periodic, extent.Height);
                    }, Sampling.Linear, (WrapMode.Periodic, WrapMode.Periodic)),
                new CoordinateMap.Analytic(
                    (points, _, _) => {
                        foreach (ref (double X, double Y) point in points)
                            point.Y = Sampling.Linear.Point(ERational.FromDouble(point.Y).Subtract(jump), WrapMode.Black, extent.Height);
                    }, Sampling.Linear, (WrapMode.Black, WrapMode.Black))),
            new CoordinateMap.Analytic(
                (points, _, _) => {
                    EInteger line = state.Lines.Line((long)Math.Floor(points[0].Y), extent);
                    double draw = 0d;
                    double* weights = (double*)storage.data_ptr().ToPointer();
                    long count = taps.size(0);
                    for (long tap = 0; tap < count; tap++)
                        draw += weights[tap] * CoordinateHash.Normals(EInteger.Zero, line.Add(tap - (count / 2)), jitterDraws).X;
                    foreach (ref (double X, double Y) point in points) {
                        ERational row = ERational.FromDouble(point.Y);
                        ERational distance = row.Subtract(crest);
                        distance = distance.Subtract(ERational.FromEInteger(NoiseFunctions.Split(distance.Divide(extent.Height).Add(ERational.One.Divide(2))).Cell).Multiply(extent.Height));
                        ERational band = ERational.FromInt32(extent.Height).Subtract(row).Divide(switchBand);
                        ERational crestShare = ERational.FromDouble(SwitchCrest);
                        ERational head = band.Sign >= 0 && band.CompareTo(ERational.One) < 0
                            ? switching.Multiply(band.CompareTo(crestShare) < 0 ? band.Divide(crestShare) : ERational.One.Subtract(band).Divide(ERational.One.Subtract(crestShare)))
                            : ERational.Zero;
                        ERational offset = interlace.Multiply(line.IsEven ? 0 : 1).Add(jitter.Multiply(ERational.FromDouble(draw)))
                            .Add(wrinkle.Multiply(ERational.Max(ERational.Zero, ERational.One.Subtract(distance.Abs().Divide(wrinkleBand))))).Add(head);
                        point.X = Sampling.Linear.Point(ERational.FromDouble(point.X).Subtract(offset), WrapMode.Clamp, extent.Width);
                    }
                }, Sampling.Linear, (WrapMode.Clamp, WrapMode.Clamp))).Apply(frame, progress);
        });
    }

    private static Fin<Unit> Signal(AnalogVideo state, PixelExtent extent, Clock now, uint field, PixelFrame frame, Mat header, IProgress<int> progress) {
        ChromaAxes axes = new(state.Matrix.Weights());
        using Mat alpha = new();
        using Mat light = new();
        using Mat forward = VideoKernels.Transform(axes.Forward);
        using Mat inverse = VideoKernels.Transform(axes.Inverse);
        using Mat ycc = new();
        using VectorOfMat planes = new();
        CvInvoke.ExtractChannel(header, alpha, 3);
        header.ConvertTo(light, DepthType.Cv64F);
        CvInvoke.Transform(light, ycc, forward);
        CvInvoke.Split(ycc, planes);
        using Mat luma = planes[0];
        using Mat blue = planes[1];
        using Mat red = planes[2];
        return (state.CrossColor == Mix.MinValue ? unit : Rainbow(state, extent, now, frame, luma, blue, red))
            .Bind(_ => state.LumaNoise == NoiseLevel.Off && state.ChromaNoise == NoiseLevel.Off ? unit : LineNoise(state, extent, field, frame, luma, blue, red))
            .Bind(_ => Luma(state, extent, luma))
            .Bind(_ => Chroma(state, extent, blue))
            .Bind(_ => Chroma(state, extent, red))
            .Map(_ => {
                CvInvoke.Merge(planes, ycc);
                CvInvoke.Transform(ycc, light, inverse);
                light.ConvertTo(header, DepthType.Cv32F);
                CvInvoke.InsertChannel(alpha, header, 3);
                progress.Report(frame.Size.Height);
                return unit;
            });
    }

    private static unsafe Fin<Unit> Rainbow(AnalogVideo state, PixelExtent extent, Clock now, PixelFrame frame, Mat luma, Mat blue, Mat red) =>
        Kernels.GaussianKernel(extent.Width / (6d * SystemM.Subcarrier)).Bind(taps => {
            using torch.Tensor kernel = taps;
            using torch.Tensor impulse = torch.ones(1, dtype: torch.ScalarType.Float64, device: torch.CPU);
            using Mat detail = new();
            return luma.Separable(detail, taps, impulse, DepthType.Cv64F, PaddingModes.Replicate).Map(_ => {
                CvInvoke.Subtract(luma, detail, detail);
                ERational cycles = ERational.FromDouble(SystemM.Subcarrier).Divide(extent.Width);
                (double Sin, double Cos)[] carrier = [.. Enumerable.Range(0, frame.Size.Width).Select(x =>
            double.SinCosPi(2d * NoiseFunctions.Split(cycles.Multiply((long)frame.Origin.X + x)).Local.ToDouble()))];
                long count = SystemM.Frame(now);
                for (int y = 0; y < frame.Size.Height; y++) {
                    ReadOnlySpan<double> luminance = new(detail.GetDataPointer(y).ToPointer(), frame.Size.Width);
                    Span<double> cb = new(blue.GetDataPointer(y).ToPointer(), frame.Size.Width);
                    Span<double> cr = new(red.GetDataPointer(y).ToPointer(), frame.Size.Width);
                    float amplitude = state.Lines.Line((long)frame.Origin.Y + y, extent).Add(count).IsEven ? state.CrossColor : -state.CrossColor;
                    for (int x = 0; x < luminance.Length; x++)
                        (cb[x], cr[x]) = (cb[x] + (amplitude * luminance[x] * carrier[x].Cos), cr[x] + (amplitude * luminance[x] * carrier[x].Sin));
                }
                return unit;
            });
        });

    private static Fin<Unit> Luma(AnalogVideo state, PixelExtent extent, Mat luma) {
        double sigma = state.LumaBandwidth.Pixels(extent);
        (bool rings, bool embosses) = (sigma > 0f && state.Ringing != EdgeGain.Off, sigma > 0f && state.Emboss != EdgeGain.Off);
        using Mat ring = new();
        using Mat relief = new();
        double smear = state.Smear.Pixels(extent);
        return (sigma > 0d ? Kernels.GaussianKernel(sigma).Bind(taps => { using (taps) return VideoKernels.Filter(luma, taps); }) : unit)
            .Bind(_ => rings ? VideoKernels.Echo(0d, state.RingingDelay.Pixels(extent), 1f).Bind(taps => {
                using (taps)
                    return luma.Derivative(ring, DepthType.Cv64F, 2, 0, 1, 1d, 0d, PaddingModes.Replicate)
                        .Bind(_ => VideoKernels.Filter(ring, taps));
            }) : unit)
            .Bind(_ => embosses ? luma.Derivative(relief, DepthType.Cv64F, 1, 0, 1, 1d, 0d, PaddingModes.Replicate) : unit)
            .Bind(_ => smear > 0d && state.SmearMix != Mix.MinValue
                ? VideoKernels.Echo(smear, smear, state.SmearMix).Bind(taps => { using (taps) return VideoKernels.Filter(luma, taps); }) : unit)
            .Bind(_ => {
                if (rings)
                    CvInvoke.ScaleAdd(ring, -state.Ringing * sigma * sigma, luma, luma);
                if (embosses)
                    CvInvoke.ScaleAdd(relief, -state.Emboss * sigma / 2d, luma, luma);
                return state.Ghost == Mix.MinValue ? unit
                    : VideoKernels.Echo(0d, state.GhostDelay.Pixels(extent), state.Ghost).Bind(taps => { using (taps) return VideoKernels.Filter(luma, taps); });
            });
    }

    private static Fin<Unit> Chroma(AnalogVideo state, PixelExtent extent, Mat plane) {
        double pitch = state.ChromaPitch.Pixels(extent);
        if (pitch > 1f) {
            using Mat coarse = new();
            CvInvoke.Resize(plane, coarse, new System.Drawing.Size(int.Max(1, (int)Math.Round(plane.Cols / pitch, MidpointRounding.ToEven)), plane.Rows), 0d, 0d, Inter.Area);
            CvInvoke.Resize(coarse, plane, plane.Size, 0d, 0d, Inter.Linear);
        }
        return (state.ChromaBandwidth == ShortSideLength.Neutral ? unit
                : Kernels.GaussianKernel(state.ChromaBandwidth.Pixels(extent)).Bind(taps => { using (taps) return VideoKernels.Filter(plane, taps); }))
            .Bind(_ => state.ChromaVertical == ShortSideLength.Neutral ? unit
                : Kernels.GaussianKernel(state.ChromaVertical.Pixels(extent)).Bind(taps => { using (taps) return VideoKernels.Filter(plane, taps, vertical: true); }))
            .Bind(_ => state.ChromaDelay == ShortSideOffset.Neutral ? unit
                : VideoKernels.Echo(0d, state.ChromaDelay.Pixels(extent), 1f).Bind(taps => { using (taps) return VideoKernels.Filter(plane, taps); }))
            .Bind(_ => state.Ghost == Mix.MinValue ? unit
                : VideoKernels.Echo(0d, state.GhostDelay.Pixels(extent), state.Ghost).Bind(taps => { using (taps) return VideoKernels.Filter(plane, taps); }));
    }

    private static unsafe Fin<Unit> LineNoise(AnalogVideo state, PixelExtent extent, uint field, PixelFrame frame, Mat luma, Mat blue, Mat red) =>
        (from taps in VideoKernels.Unit(state.NoiseLength.Pixels(extent))
         let count = taps.size(0)
         from width in Try.lift(() => checked(count + (frame.Size.Width - 1L))).Run()
             .MapFail(_ => new NativeDimensionsUnsupported((double)frame.Size.Width + count - 1d, frame.Size.Height))
         select (taps, count, width)).Map(shape => {
             (torch.Tensor taps, long count, long width) = shape;
             using torch.Tensor kernel = taps;
             long reach = count / 2;
             uint draws = CoordinateHash.Branch(field, 0u);
             using torch.Tensor plane = torch.empty((long)frame.Size.Height * 3, 1, width, dtype: torch.ScalarType.Float64, device: torch.CPU);
             using torch.Storage<double> input = plane.storage<double>();
             double* cells = (double*)input.data_ptr().ToPointer();
             for (int line = 0; line < frame.Size.Height; line++) {
                 EInteger scan = state.Lines.Line((long)frame.Origin.Y + line, extent);
                 long first = (long)line * 3 * width;
                 for (long column = 0; column < width; column++) {
                     Vector4 draw = CoordinateHash.Normals(EInteger.FromInt64(frame.Origin.X - reach + column), scan, draws);
                     (cells[first + column], cells[first + width + column], cells[first + (2 * width) + column]) = (draw.X, draw.Y, draw.Z);
                 }
             }
             using torch.Tensor filtered = torch.nn.functional.conv1d(plane, taps.reshape(1, 1, count)).contiguous();
             using torch.Storage<double> output = filtered.storage<double>();
             double* noise = (double*)output.data_ptr().ToPointer();
             Vector3 levels = new(state.LumaNoise, state.ChromaNoise, state.ChromaNoise);
             for (int y = 0; y < frame.Size.Height; y++) {
                 long first = (long)y * 3 * frame.Size.Width;
                 Span<double> luminance = new(luma.GetDataPointer(y).ToPointer(), frame.Size.Width);
                 Span<double> cb = new(blue.GetDataPointer(y).ToPointer(), frame.Size.Width);
                 Span<double> cr = new(red.GetDataPointer(y).ToPointer(), frame.Size.Width);
                 for (int x = 0; x < luminance.Length; x++) {
                     (luminance[x], cb[x], cr[x]) =
                         (luminance[x] + (noise[first + x] * levels.X), cb[x] + (noise[first + frame.Size.Width + x] * levels.Y), cr[x] + (noise[first + (2 * frame.Size.Width) + x] * levels.Z));
                 }
             }
             return unit;
         });

    private static Fin<Unit> Snowfall(AnalogVideo state, PixelExtent extent, Clock now, uint field, PixelFrame frame, IProgress<int> progress) {
        ERational length = ERational.FromSingle(state.SnowLength).Multiply(extent.ShortSide);
        ERational phase = NoiseFunctions.Split(ERational.FromSingle(state.HumRate).Multiply(ERational.FromSingle(now))).Local;
        (double X, double Y, double Z) = state.Matrix.Weights();
        uint flakes = CoordinateHash.Branch(field, 1u);
        return new PixelPass.Pointwise((row, column, line) => {
            long y = (long)extent.Height - 1 - line;
            EInteger scan = state.Lines.Line(y, extent);
            double cycle = NoiseFunctions.Split(ERational.FromInt64((2 * y) + 1).Divide(2).Divide(extent.Height).Add(phase)).Local.ToDouble();
            double dim = 1d - (state.Hum * (1d + double.CosPi(2d * cycle)) / 2d);
            for (int x = 0; x < row.Length; x++) {
                double lit = 0d;
                if (state.Snow != Mix.MinValue && state.SnowDensity != Likelihood.Off) {
                    (EInteger cell, ERational local) = NoiseFunctions.Split(ERational.FromInt64((2 * (column + x)) + 1).Divide(2).Divide(length));
                    lit = CoordinateHash.Uniforms(cell, scan, flakes).X < state.SnowDensity
                        ? state.Snow * ERational.One.Subtract(local).ToDouble()
                        : 0d;
                }
                if (lit is 0d && state.Hum == Mix.MinValue)
                    continue;
                (double r, double g, double b) = lit is 0d ? (row[x].X, row[x].Y, row[x].Z)
                    : (double.Lerp(row[x].X, 1d, lit), double.Lerp(row[x].Y, 1d, lit), double.Lerp(row[x].Z, 1d, lit));
                if (state.Hum != Mix.MinValue) {
                    double hum = (dim - 1d) * ((r * X) + (g * Y) + (b * Z));
                    (r, g, b) = (r + hum, g + hum, b + hum);
                }
                row[x] = new Vector4((float)r, (float)g, (float)b, row[x].W);
            }
        }).Run(frame, progress);
    }
}

[SmartEnum<string>]
[ValidationError<InvalidStylize>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class AnalogVideoParameter : IStateParameter<AnalogVideo> {
    public static readonly AnalogVideoParameter Matrix = new(
        "matrix", new StateParameter<AnalogVideo>.Choice<YuvMatrix, InvalidStylize>(Lens<AnalogVideo, YuvMatrix>.New(static video => video.Matrix, static matrix => video => video with { Matrix = matrix })));
    public static readonly AnalogVideoParameter Lines = new(
        "lines",
        StateParameter.Bounded<AnalogVideo, LineCount, int, InvalidStylize>(Lens<AnalogVideo, LineCount>.New(static video => video.Lines, static lines => video => video with { Lines = lines }), new() { Soft = (LineCount.Coarse, 1080) }));
    public static readonly AnalogVideoParameter LumaBandwidth = new(
        "luma-bandwidth",
        StateParameter.Bounded<AnalogVideo, ShortSideLength, float, InvalidPixelValue>(Lens<AnalogVideo, ShortSideLength>.New(static video => video.LumaBandwidth, static spread => video => video with { LumaBandwidth = spread }), ShortSideLength.Presentation with { Soft = (0f, 0.005f) }));
    public static readonly AnalogVideoParameter ChromaBandwidth = new(
        "chroma-bandwidth",
        StateParameter.Bounded<AnalogVideo, ShortSideLength, float, InvalidPixelValue>(Lens<AnalogVideo, ShortSideLength>.New(static video => video.ChromaBandwidth, static spread => video => video with { ChromaBandwidth = spread }), ShortSideLength.Presentation with { Soft = (0f, 0.005f) }));
    public static readonly AnalogVideoParameter ChromaVertical = new(
        "chroma-vertical",
        StateParameter.Bounded<AnalogVideo, ShortSideLength, float, InvalidPixelValue>(Lens<AnalogVideo, ShortSideLength>.New(static video => video.ChromaVertical, static spread => video => video with { ChromaVertical = spread }), ShortSideLength.Presentation with { Soft = (0f, 0.005f) }));
    public static readonly AnalogVideoParameter ChromaPitch = new(
        "chroma-pitch",
        StateParameter.Bounded<AnalogVideo, ShortSideLength, float, InvalidPixelValue>(Lens<AnalogVideo, ShortSideLength>.New(static video => video.ChromaPitch, static pitch => video => video with { ChromaPitch = pitch }), ShortSideLength.Presentation with { Soft = (0f, 0.01f) }));
    public static readonly AnalogVideoParameter ChromaDelay = new(
        "chroma-delay",
        StateParameter.Bounded<AnalogVideo, ShortSideOffset, float, InvalidPixelValue>(Lens<AnalogVideo, ShortSideOffset>.New(static video => video.ChromaDelay, static delay => video => video with { ChromaDelay = delay }),
        ShortSideOffset.Presentation with { Soft = (-0.02f, 0.02f) }));
    public static readonly AnalogVideoParameter Smear = new(
        "smear",
        StateParameter.Bounded<AnalogVideo, ShortSideLength, float, InvalidPixelValue>(Lens<AnalogVideo, ShortSideLength>.New(static video => video.Smear, static smear => video => video with { Smear = smear }), ShortSideLength.Presentation with { Soft = (0f, 0.005f) }));
    public static readonly AnalogVideoParameter SmearMix = new(
        "smear-mix",
        StateParameter.Bounded<AnalogVideo, Mix, float, InvalidGrade>(Lens<AnalogVideo, Mix>.New(static video => video.SmearMix, static mix => video => video with { SmearMix = mix }), new()));
    public static readonly AnalogVideoParameter Ringing = new(
        "ringing",
        StateParameter.Bounded<AnalogVideo, EdgeGain, float, InvalidStylize>(Lens<AnalogVideo, EdgeGain>.New(static video => video.Ringing, static gain => video => video with { Ringing = gain }), new() { Soft = (0f, 2f) }));
    public static readonly AnalogVideoParameter RingingDelay = new(
        "ringing-delay",
        StateParameter.Bounded<AnalogVideo, ShortSideOffset, float, InvalidPixelValue>(Lens<AnalogVideo, ShortSideOffset>.New(static video => video.RingingDelay, static delay => video => video with { RingingDelay = delay }),
        ShortSideOffset.Presentation with { Soft = (-0.02f, 0.02f) }));
    public static readonly AnalogVideoParameter Emboss = new(
        "emboss",
        StateParameter.Bounded<AnalogVideo, EdgeGain, float, InvalidStylize>(Lens<AnalogVideo, EdgeGain>.New(static video => video.Emboss, static gain => video => video with { Emboss = gain }), new() { Soft = (0f, 2f) }));
    public static readonly AnalogVideoParameter Ghost = new(
        "ghost",
        StateParameter.Bounded<AnalogVideo, Mix, float, InvalidGrade>(Lens<AnalogVideo, Mix>.New(static video => video.Ghost, static share => video => video with { Ghost = share }), new()));
    public static readonly AnalogVideoParameter GhostDelay = new(
        "ghost-delay",
        StateParameter.Bounded<AnalogVideo, ShortSideOffset, float, InvalidPixelValue>(Lens<AnalogVideo, ShortSideOffset>.New(static video => video.GhostDelay, static delay => video => video with { GhostDelay = delay }),
        ShortSideOffset.Presentation with { Soft = (-0.05f, 0.05f) }));
    public static readonly AnalogVideoParameter CrossColor = new(
        "cross-color",
        StateParameter.Bounded<AnalogVideo, Mix, float, InvalidGrade>(Lens<AnalogVideo, Mix>.New(static video => video.CrossColor, static mix => video => video with { CrossColor = mix }), new()));
    public static readonly AnalogVideoParameter LumaNoise = new(
        "luma-noise",
        StateParameter.Bounded<AnalogVideo, NoiseLevel, float, InvalidStylize>(Lens<AnalogVideo, NoiseLevel>.New(static video => video.LumaNoise, static level => video => video with { LumaNoise = level }), new() { Soft = (0f, 0.2f) }));
    public static readonly AnalogVideoParameter ChromaNoise = new(
        "chroma-noise",
        StateParameter.Bounded<AnalogVideo, NoiseLevel, float, InvalidStylize>(Lens<AnalogVideo, NoiseLevel>.New(static video => video.ChromaNoise, static level => video => video with { ChromaNoise = level }), new() { Soft = (0f, 0.2f) }));
    public static readonly AnalogVideoParameter NoiseLength = new(
        "noise-length",
        StateParameter.Bounded<AnalogVideo, ShortSideExtent, float, InvalidPixelValue>(Lens<AnalogVideo, ShortSideExtent>.New(static video => video.NoiseLength, static length => video => video with { NoiseLength = length }),
        ShortSideExtent.Presentation with { Soft = (0.001f, 0.5f) }));
    public static readonly AnalogVideoParameter Snow = new(
        "snow", StateParameter.Bounded<AnalogVideo, Mix, float, InvalidGrade>(Lens<AnalogVideo, Mix>.New(static video => video.Snow, static snow => video => video with { Snow = snow }), new()));
    public static readonly AnalogVideoParameter SnowDensity = new(
        "snow-density",
        StateParameter.Bounded<AnalogVideo, Likelihood, float, InvalidStylize>(Lens<AnalogVideo, Likelihood>.New(static video => video.SnowDensity, static density => video => video with { SnowDensity = density }), new()));
    public static readonly AnalogVideoParameter SnowLength = new(
        "snow-length",
        StateParameter.Bounded<AnalogVideo, ShortSideExtent, float, InvalidPixelValue>(Lens<AnalogVideo, ShortSideExtent>.New(static video => video.SnowLength, static length => video => video with { SnowLength = length }),
        ShortSideExtent.Presentation with { Soft = (0.001f, 0.5f) }));
    public static readonly AnalogVideoParameter Interlace = new(
        "interlace",
        StateParameter.Bounded<AnalogVideo, ShortSideOffset, float, InvalidPixelValue>(Lens<AnalogVideo, ShortSideOffset>.New(static video => video.Interlace, static offset => video => video with { Interlace = offset }),
        ShortSideOffset.Presentation with { Soft = (-0.01f, 0.01f) }));
    public static readonly AnalogVideoParameter Jitter = new(
        "jitter",
        StateParameter.Bounded<AnalogVideo, ShortSideLength, float, InvalidPixelValue>(Lens<AnalogVideo, ShortSideLength>.New(static video => video.Jitter, static jitter => video => video with { Jitter = jitter }), ShortSideLength.Presentation with { Soft = (0f, 0.01f) }));
    public static readonly AnalogVideoParameter JitterPeriod = new(
        "jitter-period",
        StateParameter.Bounded<AnalogVideo, ShortSideExtent, float, InvalidPixelValue>(Lens<AnalogVideo, ShortSideExtent>.New(static video => video.JitterPeriod, static period => video => video with { JitterPeriod = period }),
        ShortSideExtent.Presentation with { Soft = (0.001f, 0.5f) }));
    public static readonly AnalogVideoParameter Wrinkle = new(
        "wrinkle",
        StateParameter.Bounded<AnalogVideo, ShortSideOffset, float, InvalidPixelValue>(Lens<AnalogVideo, ShortSideOffset>.New(static video => video.Wrinkle, static offset => video => video with { Wrinkle = offset }),
        ShortSideOffset.Presentation with { Soft = (-0.05f, 0.05f) }));
    public static readonly AnalogVideoParameter WrinkleHeight = new(
        "wrinkle-height",
        StateParameter.Bounded<AnalogVideo, ShortSideExtent, float, InvalidPixelValue>(Lens<AnalogVideo, ShortSideExtent>.New(static video => video.WrinkleHeight, static height => video => video with { WrinkleHeight = height }),
        ShortSideExtent.Presentation with { Soft = (0.001f, 0.5f) }));
    public static readonly AnalogVideoParameter WrinkleRate = new(
        "wrinkle-rate",
        StateParameter.Bounded<AnalogVideo, Frequency, float, InvalidGenerator>(Lens<AnalogVideo, Frequency>.New(static video => video.WrinkleRate, static rate => video => video with { WrinkleRate = rate }),
        Frequency.Presentation with { Soft = (-1f, 1f) }));
    public static readonly AnalogVideoParameter HeadSwitch = new(
        "head-switch",
        StateParameter.Bounded<AnalogVideo, ShortSideOffset, float, InvalidPixelValue>(Lens<AnalogVideo, ShortSideOffset>.New(static video => video.HeadSwitch, static offset => video => video with { HeadSwitch = offset }),
        ShortSideOffset.Presentation with { Soft = (-0.1f, 0.1f) }));
    public static readonly AnalogVideoParameter HeadSwitchHeight = new(
        "head-switch-height",
        StateParameter.Bounded<AnalogVideo, ShortSideExtent, float, InvalidPixelValue>(Lens<AnalogVideo, ShortSideExtent>.New(static video => video.HeadSwitchHeight, static height => video => video with { HeadSwitchHeight = height }),
        ShortSideExtent.Presentation with { Soft = (0.001f, 0.1f) }));
    public static readonly AnalogVideoParameter Bounce = new(
        "bounce",
        StateParameter.Bounded<AnalogVideo, ShortSideLength, float, InvalidPixelValue>(Lens<AnalogVideo, ShortSideLength>.New(static video => video.Bounce, static bounce => video => video with { Bounce = bounce }), ShortSideLength.Presentation with { Soft = (0f, 0.02f) }));
    public static readonly AnalogVideoParameter RollRate = new(
        "roll-rate",
        StateParameter.Bounded<AnalogVideo, Frequency, float, InvalidGenerator>(Lens<AnalogVideo, Frequency>.New(static video => video.RollRate, static rate => video => video with { RollRate = rate }),
        Frequency.Presentation with { Soft = (-1f, 1f) }));
    public static readonly AnalogVideoParameter Hum = new(
        "hum", StateParameter.Bounded<AnalogVideo, Mix, float, InvalidGrade>(Lens<AnalogVideo, Mix>.New(static video => video.Hum, static hum => video => video with { Hum = hum }), new()));
    public static readonly AnalogVideoParameter HumRate = new(
        "hum-rate",
        StateParameter.Bounded<AnalogVideo, Frequency, float, InvalidGenerator>(Lens<AnalogVideo, Frequency>.New(static video => video.HumRate, static rate => video => video with { HumRate = rate }),
        Frequency.Presentation with { Soft = (-1f, 1f) }));
    public static readonly AnalogVideoParameter Seed = new(
        "seed",
        StateParameter.Bounded<AnalogVideo, Seed, int, InvalidGenerator>(Lens<AnalogVideo, Seed>.New(static video => video.Seed, static seed => video => video with { Seed = seed }), Generators.Seed.Presentation));
    public static readonly AnalogVideoParameter Timing = new("timing", new StateParameter<AnalogVideo>.Record<Timing>(
        Lens<AnalogVideo, Timing>.New(static state => state.Timing, static timing => state => state with { Timing = timing })));

    public StateParameter<AnalogVideo> Kind { get; }
}

public sealed record CodecDamage(JpegQuality Quality, JpegColorType ColorType, Generations Generations, Gated<LineCount> Coded)
    : IStateRecord<CodecDamage, CodecDamageParameter, InvalidStylize>, IPixelStage<CodecDamage> {
    private const int Tile = 65504;
    private static readonly DecoderOptions Decoding = new() { SkipMetadata = true };

    public static CodecDamage Default { get; } = new(JpegQuality.Standard, JpegColorType.YCbCrRatio420, Generations.Single, new(Enabled: false, LineCount.Coarse));

    public static CodecDamage Jpeg { get; } = Default with { Quality = JpegQuality.Create(60), Coded = new(Enabled: true, LineCount.Create(360)) };

    public static Option<PixelPass> Pass(CodecDamage state, PassContext context) => Some<PixelPass>(new PixelPass.Frame((frame, progress) => Kernel(state, frame, progress)));

    private static Fin<Unit> Kernel(CodecDamage state, PixelFrame frame, IProgress<int> progress) {
        int height = state.Coded.Active.Match(static lines => (int)lines, () => frame.Size.Height);
        double width = double.Max(1d, Math.Round((double)frame.Size.Width * height / frame.Size.Height, MidpointRounding.ToEven));
        return width > int.MaxValue
            ? (Fin<Unit>)new NativeDimensionsUnsupported(width, height)
            : frame.Mutate(source => {
                _ = state.Coded.Active.Iter(_ => source.Mutate(x => x.Resize((int)width, height, KnownResamplers.Box)));
                JpegEncoder encoder = new() { Quality = state.Quality, ColorType = state.ColorType };
                foreach (Rectangle tile in
                         from top in Enumerable.Range(0, ((source.Height - 1) / Tile) + 1)
                         from left in Enumerable.Range(0, ((source.Width - 1) / Tile) + 1)
                         select new Rectangle(left * Tile, top * Tile, int.Min(Tile, source.Width - (left * Tile)), int.Min(Tile, source.Height - (top * Tile)))) {
                    using Image<RgbaVector> piece = source.Clone(x => x.Crop(tile));
                    using Image<Rgb24> coded = toSeq(Enumerable.Range(2, state.Generations - 1)).Fold(Generation(piece, encoder), (image, _) => {
                        using (image)
                            return Generation(image, encoder);
                    });
                    source.Mutate(x => x.DrawImage(coded, tile.Location, 1f));
                }
                _ = state.Coded.Active.Iter(_ => source.Mutate(x => x.Resize(frame.Size.Width, frame.Size.Height, KnownResamplers.Triangle)));
            }).Map(_ => {
                progress.Report(frame.Size.Height);
                return unit;
            });
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
        StateParameter.Bounded<CodecDamage, JpegQuality, int, InvalidStylize>(Lens<CodecDamage, JpegQuality>.New(static codec => codec.Quality, static quality => codec => codec with { Quality = quality }), new() { Step = 1 }));
    public static readonly CodecDamageParameter ColorType = new(
        "color-type",
        new StateParameter<CodecDamage>.Enumerated<JpegColorType>(Lens<CodecDamage, JpegColorType>.New(static codec => codec.ColorType, static type => codec => codec with { ColorType = type })));
    public static readonly CodecDamageParameter Generations = new(
        "generations",
        StateParameter.Bounded<CodecDamage, Generations, int, InvalidStylize>(Lens<CodecDamage, Generations>.New(static codec => codec.Generations, static generations => codec => codec with { Generations = generations }), new() { Soft = (1, 8), Step = 1 }));
    public static readonly CodecDamageParameter Coded = new(
        "coded-lines",
        StateParameter.OptionalBounded<CodecDamage, LineCount, int, InvalidStylize>(Lens<CodecDamage, Gated<LineCount>>.New(static codec => codec.Coded, static lines => codec => codec with { Coded = lines }), new() { Soft = (LineCount.Coarse, 1080), Step = 1 }));

    public StateParameter<CodecDamage> Kind { get; }
}

public sealed record Glitch(
    ShortSideExtent BlockWidth, ShortSideExtent BlockHeight, Likelihood BlockDensity, ShortSideLength BlockShift, ShortSideLength BlockLift,
    ShortSideLength Tear, ShortSideExtent TearPeriod, ShortSideOffset Split, SignedAngle SplitDirection, AxisFraction SplitJitter,
    AxisFraction Slide, YuvMatrix Matrix, Mix LumaCorruption, Mix ChromaCorruption, WrapMode Wrap,
    Seed Seed, Timing Timing, Hold Hold)
    : IStateRecord<Glitch, GlitchParameter, InvalidStylize>, IPixelStage<Glitch> {
    public static Glitch Default { get; } = new(
        ShortSideExtent.Create(1f / (5f * 0.1f)), ShortSideExtent.Create(1f / 5f), Likelihood.Off, ShortSideLength.Neutral, ShortSideLength.Neutral,
        ShortSideLength.Neutral, ShortSideExtent.Create(MathF.Tau / 20f), ShortSideOffset.Neutral, SignedAngle.Neutral, AxisFraction.MinValue,
        AxisFraction.MinValue, YuvMatrix.Rec601, Mix.MinValue, Mix.MinValue, WrapMode.Periodic,
        Seed.MinValue, Timing.Default, Hold.MinValue);

    public static Glitch Corruption { get; } = Default with {
        BlockWidth = ShortSideExtent.Create(0.03f * ReferenceFrame.GreaterSide),
        BlockHeight = ShortSideExtent.Create(0.03f * ReferenceFrame.GreaterSide),
        BlockDensity = Likelihood.Create(0.2f),
        BlockShift = ShortSideLength.Create(0.4f * 0.01f * ReferenceFrame.GreaterSide),
        ChromaCorruption = Mix.Create(0.6f),
        Hold = Hold.Create(12f / 24f),
    };

    public static Glitch Displaced { get; } = Default with {
        BlockDensity = Likelihood.Create(0.2f),
        BlockShift = ShortSideLength.Create(0.05f * ReferenceFrame.GreaterSide),
        Split = ShortSideOffset.Create(0.6f * 0.05f * ReferenceFrame.GreaterSide),
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
        CoordinateMap blocks = new CoordinateMap.Analytic(Displacement(state, extent, field, draw.X), Sampling.Nearest, (state.Wrap, state.Wrap));
        Fin<Unit> moved = (state.Split != ShortSideOffset.Neutral, state.Moves) switch {
            (true, _) => Splits(state, extent, draw.Y, blocks)(frame, progress),
            (false, true) => blocks.Apply(frame, progress),
            (false, false) => unit,
        };
        return moved.Bind(_ => state.Corrupts ? new PixelPass.Pointwise(Corrupted(state, extent, field)).Run(frame, progress) : unit);
    }

    private static Action<Span<(double X, double Y)>, Span<float>, PixelExtent> Displacement(Glitch state, PixelExtent extent, uint field, float phase) {
        (ERational X, ERational Y) = (ERational.FromInt32(extent.Width).Divide(2), ERational.FromInt32(extent.Height).Divide(2));
        uint draws = CoordinateHash.Branch(field, 1u);
        (ERational shift, ERational lift, ERational tear, ERational period) =
            (ERational.FromSingle(state.BlockShift).Multiply(extent.ShortSide), ERational.FromSingle(state.BlockLift).Multiply(extent.ShortSide),
                ERational.FromSingle(state.Tear).Multiply(extent.ShortSide), ERational.FromSingle(state.TearPeriod).Multiply(extent.ShortSide));
        return (points, _, _) => {
            foreach (ref (double X, double Y) point in points) {
                (ERational X, ERational Y) p = (ERational.FromDouble(point.X), ERational.FromDouble(point.Y));
                (ERational X, ERational Y) q = (p.X.Subtract(X), p.Y.Subtract(Y));
                (ERational X, ERational Y) offset = state.Blocks && Cell(state, extent, q, draws) is (true, _, var cell)
                    ? (shift.Multiply(ERational.FromSingle(cell.Y).Multiply(2).Subtract(1)), lift.Multiply(ERational.FromSingle(cell.Z).Multiply(2).Subtract(1)))
                    : (ERational.Zero, ERational.Zero);
                ERational band = NoiseFunctions.Split(q.Y.Divide(period).Add(ERational.FromSingle(phase))).Local.Subtract(ERational.One.Divide(2));
                (ERational X, ERational Y) source = (p.X.Subtract(offset.X).Subtract(tear.Multiply(band)), p.Y.Subtract(offset.Y));
                point = (Sampling.Nearest.Point(source.X, state.Wrap, extent.Width), Sampling.Nearest.Point(source.Y, state.Wrap, extent.Height));
            }
        };
    }

    private static Func<PixelFrame, IProgress<int>, Fin<Unit>> Splits(Glitch state, PixelExtent extent, float draw, CoordinateMap blocks) {
        ERational jitter = ERational.FromSingle(state.SplitJitter);
        ERational reach = ERational.FromSingle(state.Split).Multiply(extent.ShortSide)
            .Multiply(ERational.One.Subtract(jitter).Add(jitter.Multiply(ERational.FromSingle(draw).Multiply(2).Subtract(1))));
        (double X, double Y) = double.SinCos(state.SplitDirection) switch { var (sin, cos) => (cos, -sin) };
        ERational slide = ERational.FromSingle(state.Slide);
        return reach.IsZero ? state.Moves ? blocks.Apply : static (_, _) => unit : CoordinateMap.Integrated(
            new GaussLegendreRule(0d, 1d, 1 << 6),
            tau => Subframe(reach.Multiply(ERational.One.Subtract(slide).Add(slide.Multiply(ERational.FromDouble(tau)))), Vector4.UnitX),
            tau => Subframe(reach.Negate().Multiply(ERational.One.Subtract(slide).Add(slide.Multiply(ERational.FromDouble(tau)))), Vector4.UnitZ),
            _ => Subframe(ERational.Zero, new Vector4(0f, 1f, 0f, 1f)));

        (CoordinateMap Map, Vector4 Lanes) Subframe(ERational shift, Vector4 lanes) {
            (ERational X, ERational Y) offset = (shift.Multiply(ERational.FromDouble(X)), shift.Multiply(ERational.FromDouble(Y)));
            CoordinateMap moved = new CoordinateMap.Analytic(
                (points, _, _) => {
                    foreach (ref (double X, double Y) point in points) {
                        (ERational X, ERational Y) source = (ERational.FromDouble(point.X).Subtract(offset.X), ERational.FromDouble(point.Y).Subtract(offset.Y));
                        point = (Sampling.Linear.Point(source.X, state.Wrap, extent.Width), Sampling.Linear.Point(source.Y, state.Wrap, extent.Height));
                    }
                }, Sampling.Linear, (state.Wrap, state.Wrap));
            return (state.Moves ? new CoordinateMap.Composed(blocks, moved) : moved, lanes);
        }
    }

    private static Action<Span<Vector4>, long, long> Corrupted(Glitch state, PixelExtent extent, uint field) {
        ChromaAxes axes = new(state.Matrix.Weights());
        (Matrix<double> forward, Matrix<double> inverse) = (axes.Forward, axes.Inverse);
        Vector256<double> yAxis = Vector256.Create(forward[0, 0], forward[1, 0], forward[2, 0], 0d);
        Vector256<double> cbAxis = Vector256.Create(forward[0, 1], forward[1, 1], forward[2, 1], 0d);
        Vector256<double> crAxis = Vector256.Create(forward[0, 2], forward[1, 2], forward[2, 2], 0d);
        Vector256<double> rAxis = Vector256.Create(inverse[0, 0], inverse[1, 0], inverse[2, 0], 0d);
        Vector256<double> gAxis = Vector256.Create(inverse[0, 1], inverse[1, 1], inverse[2, 1], 0d);
        Vector256<double> bAxis = Vector256.Create(inverse[0, 2], inverse[1, 2], inverse[2, 2], 0d);
        (uint draws, uint powers) = (CoordinateHash.Branch(field, 1u), CoordinateHash.Branch(field, 2u));
        (float luma, float chroma) = (state.LumaCorruption, state.ChromaCorruption);
        return (row, column, line) => {
            ERational y = ERational.FromInt64(extent.Height - (2 * line) - 1).Divide(2);
            for (int x = 0; x < row.Length; x++) {
                ERational across = ERational.FromInt64((2 * (column + x)) + 1 - extent.Width).Divide(2);
                if (Cell(state, extent, (across, y), draws) is not (true, var site, _))
                    continue;
                Vector4 exponents = CoordinateHash.Uniforms(site.X, site.Y, powers);
                (double yPower, double cPower) = (double.Exp2((4d * exponents.X) - 2d), double.Exp2((4d * exponents.Y) - 2d));
                Vector256<double> color = Vector256.Create(row[x].X, row[x].Y, row[x].Z, 0d);
                (double yValue, double cb, double cr) = (Vector256.Dot(color, yAxis), Vector256.Dot(color, cbAxis), Vector256.Dot(color, crAxis));
                Vector256<double> delta = Vector256.Create(
                    luma is 0f ? 0d : luma * (double.CopySign(double.Pow(double.Abs(yValue), yPower), yValue) - yValue),
                    chroma is 0f ? 0d : chroma * (double.CopySign(0.5d * double.Pow(double.Abs(2d * cb), cPower), cb) - cb),
                    chroma is 0f ? 0d : chroma * (double.CopySign(0.5d * double.Pow(double.Abs(2d * cr), cPower), cr) - cr), 0d);
                row[x] = new Vector4(
                    (float)(row[x].X + Vector256.Dot(delta, rAxis)),
                    (float)(row[x].Y + Vector256.Dot(delta, gAxis)),
                    (float)(row[x].Z + Vector256.Dot(delta, bAxis)), row[x].W);
            }
        };
    }

    private static (bool Active, (EInteger X, EInteger Y) Site, Vector4 Draw) Cell(Glitch state, PixelExtent extent, (ERational X, ERational Y) q, uint draws) {
        (EInteger X, EInteger Y) site = (NoiseFunctions.Split(q.X.Divide(ERational.FromSingle(state.BlockWidth).Multiply(extent.ShortSide))).Cell,
            NoiseFunctions.Split(q.Y.Divide(ERational.FromSingle(state.BlockHeight).Multiply(extent.ShortSide))).Cell);
        Vector4 draw = CoordinateHash.Uniforms(site.X, site.Y, draws);
        return (draw.X < state.BlockDensity, site, draw);
    }
}

[SmartEnum<string>]
[ValidationError<InvalidStylize>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class GlitchParameter : IStateParameter<Glitch> {
    public static readonly GlitchParameter BlockWidth = new(
        "block-width",
        StateParameter.Bounded<Glitch, ShortSideExtent, float, InvalidPixelValue>(Lens<Glitch, ShortSideExtent>.New(static glitch => glitch.BlockWidth, static width => glitch => glitch with { BlockWidth = width }),
        ShortSideExtent.Presentation with { Soft = (0.01f, 4f) }));
    public static readonly GlitchParameter BlockHeight = new(
        "block-height",
        StateParameter.Bounded<Glitch, ShortSideExtent, float, InvalidPixelValue>(Lens<Glitch, ShortSideExtent>.New(static glitch => glitch.BlockHeight, static height => glitch => glitch with { BlockHeight = height }),
        ShortSideExtent.Presentation with { Soft = (0.01f, 4f) }));
    public static readonly GlitchParameter BlockDensity = new(
        "block-density",
        StateParameter.Bounded<Glitch, Likelihood, float, InvalidStylize>(Lens<Glitch, Likelihood>.New(static glitch => glitch.BlockDensity, static density => glitch => glitch with { BlockDensity = density }), new()));
    public static readonly GlitchParameter BlockShift = new(
        "block-shift",
        StateParameter.Bounded<Glitch, ShortSideLength, float, InvalidPixelValue>(Lens<Glitch, ShortSideLength>.New(static glitch => glitch.BlockShift, static shift => glitch => glitch with { BlockShift = shift }), ShortSideLength.Presentation with { Soft = (0f, 0.5f) }));
    public static readonly GlitchParameter BlockLift = new(
        "block-lift",
        StateParameter.Bounded<Glitch, ShortSideLength, float, InvalidPixelValue>(Lens<Glitch, ShortSideLength>.New(static glitch => glitch.BlockLift, static lift => glitch => glitch with { BlockLift = lift }), ShortSideLength.Presentation with { Soft = (0f, 0.1f) }));
    public static readonly GlitchParameter Tear = new(
        "tear",
        StateParameter.Bounded<Glitch, ShortSideLength, float, InvalidPixelValue>(Lens<Glitch, ShortSideLength>.New(static glitch => glitch.Tear, static tear => glitch => glitch with { Tear = tear }), ShortSideLength.Presentation with { Soft = (0f, 0.1f) }));
    public static readonly GlitchParameter TearPeriod = new(
        "tear-period",
        StateParameter.Bounded<Glitch, ShortSideExtent, float, InvalidPixelValue>(Lens<Glitch, ShortSideExtent>.New(static glitch => glitch.TearPeriod, static period => glitch => glitch with { TearPeriod = period }),
        ShortSideExtent.Presentation with { Soft = (0.01f, 1f) }));
    public static readonly GlitchParameter Split = new(
        "split",
        StateParameter.Bounded<Glitch, ShortSideOffset, float, InvalidPixelValue>(Lens<Glitch, ShortSideOffset>.New(static glitch => glitch.Split, static split => glitch => glitch with { Split = split }),
        ShortSideOffset.Presentation with { Soft = (-0.05f, 0.05f) }));
    public static readonly GlitchParameter SplitDirection = new(
        "split-direction",
        StateParameter.Bounded<Glitch, SignedAngle, float, InvalidPixelValue>(Lens<Glitch, SignedAngle>.New(static glitch => glitch.SplitDirection, static direction => glitch => glitch with { SplitDirection = direction }),
        SignedAngle.Presentation));
    public static readonly GlitchParameter SplitJitter = new(
        "split-jitter",
        StateParameter.Bounded<Glitch, AxisFraction, float, InvalidGrade>(Lens<Glitch, AxisFraction>.New(static glitch => glitch.SplitJitter, static jitter => glitch => glitch with { SplitJitter = jitter }), new()));
    public static readonly GlitchParameter Slide = new(
        "slide",
        StateParameter.Bounded<Glitch, AxisFraction, float, InvalidGrade>(Lens<Glitch, AxisFraction>.New(static glitch => glitch.Slide, static slide => glitch => glitch with { Slide = slide }), new()));
    public static readonly GlitchParameter Matrix = new(
        "matrix", new StateParameter<Glitch>.Choice<YuvMatrix, InvalidStylize>(Lens<Glitch, YuvMatrix>.New(static glitch => glitch.Matrix, static matrix => glitch => glitch with { Matrix = matrix })));
    public static readonly GlitchParameter LumaCorruption = new(
        "luma-corruption",
        StateParameter.Bounded<Glitch, Mix, float, InvalidGrade>(Lens<Glitch, Mix>.New(static glitch => glitch.LumaCorruption, static corruption => glitch => glitch with { LumaCorruption = corruption }), new()));
    public static readonly GlitchParameter ChromaCorruption = new(
        "chroma-corruption",
        StateParameter.Bounded<Glitch, Mix, float, InvalidGrade>(Lens<Glitch, Mix>.New(static glitch => glitch.ChromaCorruption, static corruption => glitch => glitch with { ChromaCorruption = corruption }), new()));
    public static readonly GlitchParameter Wrap = new(
        "wrap", new StateParameter<Glitch>.Choice<WrapMode, InvalidPixelValue>(Lens<Glitch, WrapMode>.New(static glitch => glitch.Wrap, static wrap => glitch => glitch with { Wrap = wrap })));
    public static readonly GlitchParameter Seed = new(
        "seed",
        StateParameter.Bounded<Glitch, Seed, int, InvalidGenerator>(Lens<Glitch, Seed>.New(static glitch => glitch.Seed, static seed => glitch => glitch with { Seed = seed }), Generators.Seed.Presentation));
    public static readonly GlitchParameter Timing = new("timing", new StateParameter<Glitch>.Record<Timing>(
        Lens<Glitch, Timing>.New(static state => state.Timing, static timing => state => state with { Timing = timing })));
    public static readonly GlitchParameter Hold = new(
        "hold",
        StateParameter.Bounded<Glitch, Hold, float, InvalidGenerator>(Lens<Glitch, Hold>.New(static glitch => glitch.Hold, static hold => glitch => glitch with { Hold = hold }), Generators.Hold.Presentation));

    public StateParameter<Glitch> Kind { get; }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
file static class SystemM {
    public const double Subcarrier = 188.3d;

    public static long Frame(Clock now) => NoiseFunctions.Split(ERational.FromSingle(now).Multiply(30000).Divide(1001)).Cell.ToInt64Checked();
}

file static class VideoKernels {
    public static Fin<torch.Tensor> Unit(double sigma) => Kernels.GaussianKernel(sigma).Map(static taps => taps / taps.norm());

    public static Fin<torch.Tensor> Echo(double sigma, double delay, float share) => PixelSampling.KernelTaps<long>(ERational.FromDouble(delay).Abs().Add(ERational.FromDouble(sigma).Multiply(3))).Map(count => {
        long integral = long.CreateChecked(Math.Round(delay, MidpointRounding.ToEven));
        double fraction = delay - integral;
        double nearest = Math.Abs(fraction);
        torch.Tensor offsets = torch.ones(count, dtype: torch.ScalarType.Int64, device: torch.CPU).cumsum(0) - ((count / 2) + 1);
        torch.Tensor distance = ((offsets + integral).@double() + fraction).abs();
        torch.Tensor shape = sigma > 0d
            ? ((distance - nearest) * (distance + nearest) * -0.5d / sigma / sigma).exp()
            : (1d - distance).clamp_min(0d);
        return (share * shape / shape.sum()) + (offsets.eq(0L).@double() * (1d - share));
    });

    public static Mat Transform(Matrix<double> rows) {
        double[] columns = rows.Resize(4, 4).ToColumnMajorArray();
        columns[^1] = 1d;
        Mat matrix = new(4, 4, DepthType.Cv64F, 1);
        matrix.SetTo(columns);
        return matrix;
    }

    public static Fin<Unit> Filter(Mat plane, torch.Tensor taps, bool vertical = false) {
        using torch.Tensor impulse = torch.ones(1, dtype: torch.ScalarType.Float64, device: torch.CPU);
        return plane.Separable(plane, vertical ? impulse : taps, vertical ? taps : impulse, DepthType.Cv64F, PaddingModes.Replicate);
    }
}
