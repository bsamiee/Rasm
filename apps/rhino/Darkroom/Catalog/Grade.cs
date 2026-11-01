using System.Drawing;
using System.Numerics;
using Darkroom.Chain;
using Rasm.Imaging.Color;
using Rasm.Imaging.Grade;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Procedural;
using Rasm.Imaging.Tone;
using UnitsNet;
using Wacton.Unicolour;

namespace Darkroom.Catalog;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record ColorDecisionList(LutTable.Cdl Correction) : IPixelStage<ColorDecisionList> {
    public static ColorDecisionList Default { get; } = new(new LutTable.Cdl(CdlStyle.FwdNoClamp, Vector3.One, Vector3.Zero, Vector3.One, 1f));

    public static Fin<Option<PixelPass>> Pass(ColorDecisionList state, PassContext context) =>
        state == Default ? None : Some<PixelPass>(new PixelPass.Point(FormationTables.AgXLog.Around(new ColorEncoding(context.Working, context.Signal), state.Correction).Apply));
}

public sealed record ContrastGrade(float Amount, float Pivot, float Vibrance, float Saturation) : IPixelStage<ContrastGrade> {
    public static ContrastGrade Default { get; } = new(1f, 0f, 0f, 1f);

    public static Fin<Option<PixelPass>> Pass(ContrastGrade state, PassContext context) =>
        (GradingPrimary.Neutral(GradingStyle.Linear) with {
            Contrast = new GradingRgbm(1f, 1f, 1f, state.Amount),
            Pivot = state.Pivot,
            Vibrance = state.Vibrance,
            Saturation = state.Saturation,
        }).Graded(GradingStyle.Linear, context.Working).Map(static graded => (PixelPass)new PixelPass.Point(graded));
}

public sealed record Levels(float Black, float White) : IPixelStage<Levels> {
    public static Levels Default { get; } = new(0f, 0f);

    public static Fin<Levels> Auto(PixelFrame frame, Gamut working) =>
        LuminanceSamples.Gather(frame, working).Map(static samples => new Levels((float)samples.At(0f), (float)double.Log2(samples.FrameWhite - samples.At(0f))));

    public static Fin<Option<PixelPass>> Pass(Levels state, PassContext context) =>
        state == Default ? None
            : Some<PixelPass>(new PixelPass.Point(new LutTable.Range(Some((state.Black, 0f)), Some((state.Black + float.Exp2(state.White), 1f)), Clamps: false).Apply));
}

public sealed record ChannelMixer(Vector3 Red, Vector3 Green, Vector3 Blue) : IPixelStage<ChannelMixer> {
    public static ChannelMixer Default { get; } = new(Vector3.UnitX, Vector3.UnitY, Vector3.UnitZ);

    public LutTable Table => new LutTable.Affine(Gamut.Columns(rgb => new Vector3(Vector3.Dot(Red, rgb), Vector3.Dot(Green, rgb), Vector3.Dot(Blue, rgb))));

    public static Fin<Option<PixelPass>> Pass(ChannelMixer state, PassContext context) =>
        state == Default ? None : Some<PixelPass>(new PixelPass.Point(state.Table.Apply));
}

public sealed record WhiteBalance(Wacton.Unicolour.Temperature White) : IPixelStage<WhiteBalance> {
    public static WhiteBalance Default { get; } = new(new Unicolour(Configuration.Default, Configuration.Default.Xyz.WhitePoint.Chromaticity).Temperature);

    public static Seq<(string Name, Func<WhiteBalance, WhiteBalance> Look)> Looks { get; } =
        toSeq(StandardIlluminant.Items).Map(static illuminant =>
            new WhiteBalance(new Unicolour(Configuration.Default, illuminant.Illuminant.GetWhitePoint(Observer.Degree2).Chromaticity).Temperature) switch {
                var lit => ($"Illuminant {illuminant.Key}", (Func<WhiteBalance, WhiteBalance>)(_ => lit)),
            });

    public static Fin<WhiteBalance> Picked(PixelFrame frame, Option<Rectangle> region, Gamut working) =>
        region.IfNone(frame.Window) switch {
            var window => frame.Mean(window).ToFin(new DarkroomFault($"white balance region {window}", DarkroomReason.Empty))
                .Bind(mean => working.Temperature(mean.AsVector3())).Map(static white => new WhiteBalance(white)),
        };

    public static Fin<Option<PixelPass>> Pass(WhiteBalance state, PassContext context) =>
        state == Default ? None : Some<PixelPass>(new PixelPass.Point(new LutTable.Affine(context.Working.Adapting(state.White.WhitePoint)).Apply));
}

public sealed record ColorWheels(
    GradingRgbm Global, GradingRgbm Shadows, GradingRgbm Midtones, GradingRgbm Highlights, float ShadowsEnd, float HighlightsStart, float HighlightsWidth)
    : IPixelStage<ColorWheels> {
    private static readonly GradingRgbm Neutral = new(1f, 1f, 1f, 1f);

    public static ColorWheels Default { get; } = new(Neutral, Neutral, Neutral, Neutral, 0.333f, 0.55f, 0.45f);

    public static Fin<Option<PixelPass>> Pass(ColorWheels state, PassContext context) =>
        Seq(state.Global, state.Shadows, state.Midtones, state.Highlights).ForAll(static wheel => wheel == Neutral) ? None
            : (context.Working.Luminance, state.Global.Product, state.Shadows.Product, state.Midtones.Product, state.Highlights.Product) switch {
                var (luminance, all, low, middle, high) => Some<PixelPass>(new PixelPass.Point(ToneCurve.Graded(color =>
                    Vector3.Dot(luminance, color) switch {
                        var luma => (DistanceField.SmoothStep(0f, state.ShadowsEnd, luma), DistanceField.SmoothStep(state.HighlightsStart, state.HighlightsStart + state.HighlightsWidth, luma)) switch {
                            var (lifted, raised) => color * all * ((low * (1f - lifted)) + (middle * lifted * (1f - raised)) + (high * lifted * raised)),
                        },
                    }))),
            };
}

public sealed record SelectiveColor(HueRange Range, Angle Shift, float Saturation, float Exposure) : IPixelStage<SelectiveColor> {
    public static SelectiveColor Default { get; } = new(HueRange.Open, Angle.Zero, 1f, 0f);

    public static Fin<Option<PixelPass>> Pass(SelectiveColor state, PassContext context) =>
        state with { Range = Default.Range } == Default ? None
            : Some<PixelPass>(new PixelPass.Point(state.Range.Graded(GradingStyle.Linear, context.Working, (float)state.Shift.Revolutions, state.Saturation, state.Exposure)));
}

public sealed record HueIsolation(HueRange Range) : IPixelStage<HueIsolation> {
    public static HueIsolation Default { get; } = new(HueRange.Open);

    public static Fin<Option<PixelPass>> Pass(HueIsolation state, PassContext context) =>
        state == Default ? None : Some<PixelPass>(new PixelPass.Point(state.Range.Isolated(GradingStyle.Linear, context.Working)));
}

public sealed record ToneZones(GradingTone Zones) : IPixelStage<ToneZones> {
    public static ToneZones Default { get; } = new(GradingTone.Neutral(GradingStyle.Linear));

    public static Fin<Option<PixelPass>> Pass(ToneZones state, PassContext context) =>
        state.Zones.Graded(GradingStyle.Linear).Map(static graded => (PixelPass)new PixelPass.Point(graded));
}

public sealed record PointCurveGrade(GradingRgbCurve Curves, float PreserveHue) : IPixelStage<PointCurveGrade> {
    public static PointCurveGrade Default { get; } = new(GradingRgbCurve.Neutral(GradingStyle.Linear), 0f);

    public static Fin<Option<PixelPass>> Pass(PointCurveGrade state, PassContext context) =>
        state.Curves.Graded(GradingStyle.Linear, state.PreserveHue).Map(static graded => (PixelPass)new PixelPass.Point(graded));
}

public sealed record HueCurveGrade(GradingHueCurve Curves) : IPixelStage<HueCurveGrade> {
    public static HueCurveGrade Default { get; } = new(GradingHueCurve.Neutral(GradingStyle.Linear));

    public static Fin<Option<PixelPass>> Pass(HueCurveGrade state, PassContext context) =>
        state.Curves.Graded(GradingStyle.Linear, context.Working).Map(static graded => (PixelPass)new PixelPass.Point(graded));
}

public sealed record GradientMapping(Seq<RampStop> Ramp) : IPixelStage<GradientMapping> {
    public static GradientMapping Default { get; } = new([new RampStop(new Unicolour(ColourSpace.RgbLinear, 0d, 0d, 0d), 0f), new RampStop(new Unicolour(ColourSpace.RgbLinear, 1d, 1d, 1d), 1f)]);

    public static Fin<Option<PixelPass>> Pass(GradientMapping state, PassContext context) =>
        Some<PixelPass>(new PixelPass.Point(new GradientMap(toSeq(RampLight.Of(state.Ramp, context).Texels)).Graded(context.Working)));
}

public sealed record ReferenceGamutCompression(Option<Gamut> Target, GamutCompression Compression) : IPixelStage<ReferenceGamutCompression> {
    public static ReferenceGamutCompression Default { get; } = new(None, GamutCompression.Reference);

    public static Fin<Option<PixelPass>> Pass(ReferenceGamutCompression state, PassContext context) =>
        Some<PixelPass>(new PixelPass.Point(state.Compression.Within(context.Working, state.Target.IfNone(context.Working)).Apply));
}
