using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Numerics;
using Darkroom.Chain;
using MathNet.Numerics.LinearAlgebra;
using Rasm.Imaging.Color;
using Rasm.Imaging.Grade;
using Rasm.Imaging.Grade.Curves;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Tone;
using Wacton.Unicolour;

namespace Darkroom.Catalog;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class Primaries {
    public static readonly Primaries StandardRgb = new("standard-rgb", Gamut.StandardRgb);
    public static readonly Primaries DisplayP3 = new("display-p3", Gamut.DisplayP3);
    public static readonly Primaries Rec2020 = new("rec2020", Gamut.Rec2020);
    public static readonly Primaries Acescg = new("acescg", Gamut.Acescg);
    public static readonly Primaries Aces20651 = new("aces20651", Gamut.Aces20651);
    public static readonly Primaries ProPhoto = new("pro-photo", Gamut.ProPhoto);
    public static readonly Primaries EGamut = new("e-gamut", Gamut.EGamut);
    public static readonly Primaries ArriWideGamut3 = new("arri-wide-gamut-3", Gamut.ArriWideGamut3);
    public static readonly Primaries ArriWideGamut4 = new("arri-wide-gamut-4", Gamut.ArriWideGamut4);
    public static readonly Primaries SGamut3 = new("s-gamut3", Gamut.SGamut3);
    public static readonly Primaries SGamut3Cine = new("s-gamut3-cine", Gamut.SGamut3Cine);
    public static readonly Primaries VeniceSGamut3 = new("venice-s-gamut3", Gamut.VeniceSGamut3);
    public static readonly Primaries VeniceSGamut3Cine = new("venice-s-gamut3-cine", Gamut.VeniceSGamut3Cine);
    public static readonly Primaries CinemaGamut = new("cinema-gamut", Gamut.CinemaGamut);
    public static readonly Primaries RedWideGamutRgb = new("red-wide-gamut-rgb", Gamut.RedWideGamutRgb);
    public static readonly Primaries VGamut = new("v-gamut", Gamut.VGamut);
    public static readonly Primaries DGamut = new("d-gamut", Gamut.DGamut);
    public static readonly Primaries ProtuneNative = new("protune-native", Gamut.ProtuneNative);
    public static readonly Primaries BlackmagicWideGamut = new("blackmagic-wide-gamut", Gamut.BlackmagicWideGamut);
    public static readonly Primaries DaVinciWideGamut = new("davinci-wide-gamut", Gamut.DaVinciWideGamut);
    public static readonly Primaries AppleWideGamut = new("apple-wide-gamut", Gamut.AppleWideGamut);

    public Gamut Gamut { get; }
}

public sealed record ColorDecisionList(
    [property: Display(Name = "Slope", Description = "Multiplier before offset")][property: Range(0d, 2d)] Channels<float> Slope,
    [property: Display(Name = "Offset", Description = "Value added after slope")][property: Range(-1d, 1d)] Channels<float> Offset,
    [property: Display(Name = "Power", Description = "Exponent after offset")][property: Range(0.1d, 2d)] Channels<float> Power,
    [property: Display(Name = "Saturation", Description = "Distance from Rec.709 luma after power")][property: Range(0d, 2d)] float Saturation) : IPixelStage<ColorDecisionList> {
    public static ColorDecisionList Default { get; } = new(new(1f), new(0f), new(1f), 1f);

    public LutTable.Cdl Correction =>
        new(CdlStyle.FwdNoClamp, new(Slope.Red, Slope.Green, Slope.Blue), new(Offset.Red, Offset.Green, Offset.Blue), new(Power.Red, Power.Green, Power.Blue), Saturation);

    public static ColorDecisionList Of(LutTable.Cdl correction) =>
        new(new(correction.Slope.X, correction.Slope.Y, correction.Slope.Z), new(correction.Offset.X, correction.Offset.Y, correction.Offset.Z),
            new(correction.Power.X, correction.Power.Y, correction.Power.Z), correction.Saturation);

    public static Option<PixelPass> Pass(ColorDecisionList state, PassContext context) =>
        state == Default ? None
            : Some<PixelPass>(new PixelPass.Color(LutTables.Bracketed(context.Working, FormationTables.AgXLog.Gamut, FormationTables.AgXLog.Into, state.Correction, FormationTables.AgXLog.Back)));
}

public sealed record ContrastGrade(
    [property: Display(Name = "Contrast", Description = "Exponent about pivot gray, image unchanged at one")][property: Range(0.01d, 2d)] float Amount,
    [property: Display(Name = "Pivot", Description = "Stops from middle gray")][property: Range(-7d, 7d)] float Pivot,
    [property: Display(Name = "Vibrance", Description = "Saturation gain falling as saturation rises")][property: Range(-1d, 1d)] float Vibrance,
    [property: Display(Name = "Saturation", Description = "Distance from working luminance, gray at zero")][property: Range(0d, 2d)] float Saturation) : IPixelStage<ContrastGrade> {
    public static ContrastGrade Default { get; } = new(1f, 0f, 0f, 1f);

    public static Option<PixelPass> Pass(ContrastGrade state, PassContext context) =>
        state with { Pivot = Default.Pivot } == Default ? None
            : Some<PixelPass>(new PixelPass.Color(row => Primary.Contrast(row, state.Amount, Exposure.MiddleGray * float.Exp2(state.Pivot), state.Vibrance, state.Saturation, context.Working.Luminance)));
}

public sealed record Levels(
    [property: Display(Name = "Black", Description = "Scene value mapped to zero, lower values pass unclamped")][property: Range(-0.1d, 0.1d)] float Black,
    [property: Display(Name = "White", Description = "Span above black mapped to one, in stops")][property: Range(-10d, 10d)] float White) : IPixelStage<Levels> {
    public static Levels Default { get; } = new(0f, 0f);

    public static Fin<Levels> Auto(PixelFrame frame, Gamut working) =>
        LuminanceSamples.Gather(frame, working.Luminance).Map(static samples => new Levels((float)samples.Minimum, (float)double.Log2(samples.FrameWhite - samples.Minimum)));

    public static Option<PixelPass> Pass(Levels state, PassContext context) =>
        state == Default ? None : Some<PixelPass>(new PixelPass.Color(Primary.Levels(state.Black, state.White)));
}

public sealed record ChannelMixer(
    [property: Display(Name = "Weights", Description = "Weight of each input channel in each output channel")][property: Range(-2d, 2d)] Channels<Channels<float>> Weights) : IPixelStage<ChannelMixer> {
    public static ChannelMixer Default { get; } = new(new Channels<Channels<float>>(new(1f, 0f, 0f), new(0f, 1f, 0f), new(0f, 0f, 1f)));

    public LutTable.Affine Table =>
        new(Matrix<double>.Build.DenseOfRowArrays(Seq(Weights.Red, Weights.Green, Weights.Blue).Map(static row => new double[] { row.Red, row.Green, row.Blue })));

    public static Option<PixelPass> Pass(ChannelMixer state, PassContext context) =>
        state == Default ? None : Some<PixelPass>(new PixelPass.Color(state.Table));
}

public sealed record WhiteBalance(
    [property: Display(Name = "Temperature", Description = "Kelvin of light rendered neutral")][property: Range(2000d, 11_000d)] double Cct,
    [property: Display(Name = "Tint", Description = "Neutral light offset from blackbody locus, positive toward green")][property: Range(-0.05d, 0.05d)] double Duv) : IPixelStage<WhiteBalance> {
    public static WhiteBalance Default { get; } =
        new Unicolour(Configuration.Default, Configuration.Default.Xyz.WhitePoint.Chromaticity).Temperature switch { var white => new(white.Cct, white.Duv) };

    public static Seq<(string Name, Func<WhiteBalance, WhiteBalance> Look)> Looks { get; } =
        (Seq(Illuminant.A, Illuminant.C, Illuminant.D50, Illuminant.D55, Illuminant.D65, Illuminant.D75, Illuminant.E, Illuminant.F2, Illuminant.F7, Illuminant.F11)
            .Map(static illuminant => (illuminant.Name, White: new Unicolour(Configuration.Default, illuminant.GetWhitePoint(Observer.Degree2).Chromaticity).Temperature))
        + toSeq(StandardIlluminant.Items).Map(static illuminant => (Name: $"Illuminant {illuminant.Key}", White: illuminant.Temperature)))
            .Map(static lit => (lit.Name, (Func<WhiteBalance, WhiteBalance>)(_ => new WhiteBalance(lit.White.Cct, lit.White.Duv))));

    public static Fin<WhiteBalance> Picked(PixelFrame frame, Option<Rectangle> region, Gamut working) =>
        Primary.PickedWhite(frame, region, working).Map(static white => new WhiteBalance(white.Cct, white.Duv));

    public static Option<PixelPass> Pass(WhiteBalance state, PassContext context) =>
        state == Default ? None : Some<PixelPass>(new PixelPass.Color(Primary.WhiteBalance(context.Working, new(state.Cct, state.Duv))));
}

public sealed record ColorWheels(
    [property: Display(Name = "Global", Description = "Tint over every tone")] WheelOffset Global,
    [property: Display(Name = "Shadows", Description = "Tint over dark tones")] WheelOffset Shadows,
    [property: Display(Name = "Midtones", Description = "Tint over middle tones")] WheelOffset Midtones,
    [property: Display(Name = "Highlights", Description = "Tint over bright tones")] WheelOffset Highlights,
    [property: Display(Name = "Shadows end", Description = "Working luminance ending shadows weight")][property: Range(0d, 1d)] float ShadowsEnd,
    [property: Display(Name = "Highlights start", Description = "Working luminance starting highlights weight")][property: Range(0d, 1d)] float HighlightsStart,
    [property: Display(Name = "Highlights width", Description = "Luminance span over which highlights weight reaches full")][property: Range(0d, 1d)] float HighlightsWidth)
    : IPixelStage<ColorWheels> {
    public static ColorWheels Default { get; } = new(default, default, default, default, 0.333f, 0.55f, 0.45f);

    public static Option<PixelPass> Pass(ColorWheels state, PassContext context) =>
        Seq(state.Global, state.Shadows, state.Midtones, state.Highlights).ForAll(static wheel => wheel == default) ? None
            : Some<PixelPass>(new PixelPass.Color(WheelOffset.Graded(
                context.Working, state.Global, state.Shadows, state.Midtones, state.Highlights, state.ShadowsEnd, state.HighlightsStart, state.HighlightsWidth)));
}

public sealed record SelectiveColor(
    [property: Display(Name = "Range", Description = "Band of input graded")] HueRange Range,
    [property: Display(Name = "Hue shift", Description = "Hue turn on qualified pixels in radians")][property: Range(-double.Pi, double.Pi)] float Shift,
    [property: Display(Name = "Saturation", Description = "Saturation gain on qualified pixels")][property: Range(0d, 2d)] float Saturation,
    [property: Display(Name = "Exposure", Description = "Stops added to qualified pixels")][property: Range(-10d, 10d)] float Exposure) : IPixelStage<SelectiveColor> {
    public static SelectiveColor Default { get; } = new(HueRange.Open, 0f, 1f, 0f);

    public static Option<PixelPass> Pass(SelectiveColor state, PassContext context) =>
        state with { Range = Default.Range } == Default ? None : Some<PixelPass>(new PixelPass.Color(row => state.Range.Graded(row, state.Shift, state.Saturation, state.Exposure)));
}

public sealed record HueIsolation([property: Display(Name = "Range", Description = "Band of input kept in color")] HueRange Range) : IPixelStage<HueIsolation> {
    public static HueIsolation Default { get; } = new(HueRange.Open);

    public static Option<PixelPass> Pass(HueIsolation state, PassContext context) =>
        state == Default ? None : Some<PixelPass>(new PixelPass.Color(row => state.Range.Isolated(row, context.Working.Luminance)));
}

public sealed record ToneZones([property: Display(Name = "Zones", Description = "Blacks, shadows, midtones, highlights and whites adjustments with S-curve contrast")] GradingTone Zones)
    : IPixelStage<ToneZones> {
    public static ToneZones Default { get; } = new(GradingStyle.Linear.Neutral);

    public static Option<PixelPass> Pass(ToneZones state, PassContext context) =>
        state.Zones.Graded(GradingStyle.Linear).Map(static graded => (PixelPass)new PixelPass.Color(graded));
}

public sealed record PointCurveGrade(
    [property: Display(Name = "Curves", Description = "Red, green, blue and master curves over stops about middle gray")] PointCurves Curves,
    [property: Display(Name = "Preserve hue", Description = "Share of pixel hue kept through master curve")][property: Range(0d, 1d)] float PreserveHue) : IPixelStage<PointCurveGrade> {
    public static PointCurveGrade Default { get; } = new(PointCurves.Identity, 0f);

    public static Option<PixelPass> Pass(PointCurveGrade state, PassContext context) =>
        state.Curves == PointCurves.Identity ? None : Some<PixelPass>(new PixelPass.Color(state.Curves.Graded(state.PreserveHue)));
}

public sealed record HueCurveGrade([property: Display(Name = "Curves", Description = "Hue, saturation and luma curves by hue, saturation and luma")] HueCurves Curves)
    : IPixelStage<HueCurveGrade> {
    public static HueCurveGrade Default { get; } = new(HueCurves.Identity);

    public static Option<PixelPass> Pass(HueCurveGrade state, PassContext context) =>
        state.Curves == HueCurves.Identity ? None : Some<PixelPass>(new PixelPass.Color(state.Curves.Graded()));
}

public sealed record GradientMapping([property: Display(Name = "Gradient", Description = "Colors mapped over working luminance")] ImmutableArray<RampStop> Ramp)
    : IPixelStage<GradientMapping> {
    private const int Texels = 256;

    public static GradientMapping Default { get; } = new([new RampStop(new Unicolour(ColourSpace.RgbLinear, 0d, 0d, 0d), 0f), new RampStop(new Unicolour(ColourSpace.RgbLinear, 1d, 1d, 1d), 1f)]);

    public static Option<PixelPass> Pass(GradientMapping state, PassContext context) =>
        toSeq(Range(0, Texels + 1)).Map(texel => RampStop.At(state.Ramp, texel / (float)Texels).ConvertToConfiguration(context.Working.Configuration) switch {
            var color => new Vector4((float)color.RgbLinear.R, (float)color.RgbLinear.G, (float)color.RgbLinear.B, (float)color.Alpha.A),
        }).ToArray() switch {
            var lights => Some<PixelPass>(new PixelPass.Color(row => GradientMap.Mapped(row, lights, context.Working.Luminance))),
        };
}

public sealed record ReferenceGamutCompression(
    [property: Display(Name = "Gamut", Description = "Primaries the compression measures distances in, working gamut when unset")] Primaries? Target,
    [property: Display(Name = "Threshold", Description = "Distance from achromatic axis where compression begins, cyan, magenta and yellow")][property: Range(0d, 0.999d)] Channels<float> Threshold,
    [property: Display(Name = "Limit", Description = "Distance compressed onto the gamut boundary, cyan, magenta and yellow")][property: Range(1.001d, 2d)] Channels<float> Limit,
    [property: Display(Name = "Power", Description = "Compression curve exponent")][property: Range(1d, 2d)] float Power) : IPixelStage<ReferenceGamutCompression> {
    public static ReferenceGamutCompression Default { get; } = Of(GamutCompression.Reference);

    public static ReferenceGamutCompression Of(GamutCompression compression) =>
        new(Target: null, new(compression.Threshold.X, compression.Threshold.Y, compression.Threshold.Z), new(compression.Limit.X, compression.Limit.Y, compression.Limit.Z), compression.Power);

    public static Option<PixelPass> Pass(ReferenceGamutCompression state, PassContext context) =>
        GamutCompression.Validate(new(state.Threshold.Red, state.Threshold.Green, state.Threshold.Blue), new(state.Limit.Red, state.Limit.Green, state.Limit.Blue), state.Power, out GamutCompression compression) switch {
            null => Some<PixelPass>(new PixelPass.Color(compression.Within(context.Working, Optional(state.Target).Map(static target => target.Gamut).IfNone(context.Working)))),
            var fault => Some<PixelPass>(new PixelPass.Frame((_, _) => fault)),
        };
}
