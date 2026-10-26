using System.ComponentModel.DataAnnotations;
using System.Numerics;
using System.Text.Json.Serialization;
using Darkroom.Chain;
using Rasm.Imaging.Color;
using Rasm.Imaging.Files;
using Rasm.Imaging.Grade;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Tone;
using TinyEXR;
using UnitsNet;

namespace Darkroom.Catalog;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class AgXLook {
    public static readonly AgXLook Punchy = new("punchy", static _ => Chained([
        .. (GradingStyle.Log.Neutral with { Shadows = new GradingRgbmsw(0.2f, 0.2f, 0.2f, 0.35f, 0.4f, 0.3f) }).Graded(GradingStyle.Log).ToSeq(),
        new LutTable.Cdl(CdlStyle.FwdNoClamp, Vector3.One, Vector3.Zero, new Vector3(1.0912f), 1f).Apply]));
    public static readonly AgXLook Greyscale = new("greyscale", static space => Chained([new LutTable.Antilog(space.Allocation).Apply, AgX.Grey.Apply, new LutTable.Log(space.Allocation).Apply]));
    public static readonly AgXLook VeryHighContrast = new("very-high-contrast", Contrasted(1.57f, 0.9f));
    public static readonly AgXLook HighContrast = new("high-contrast", Contrasted(1.4f, 0.95f));
    public static readonly AgXLook MediumHighContrast = new("medium-high-contrast", Contrasted(1.2f, 1f));
    public static readonly AgXLook BaseContrast = new("base-contrast", Contrasted(1f, 1f));
    public static readonly AgXLook MediumLowContrast = new("medium-low-contrast", Contrasted(0.9f, 1.05f));
    public static readonly AgXLook LowContrast = new("low-contrast", Contrasted(0.8f, 1.1f));
    public static readonly AgXLook VeryLowContrast = new("very-low-contrast", Contrasted(0.7f, 1.15f));

    [UseDelegateFromConstructor]
    public partial Action<Span<Vector4>> Graded(LogSpace space);

    private static Func<LogSpace, Action<Span<Vector4>>> Contrasted(float contrast, float saturation) =>
        _ => new LutTable.Cdl(CdlStyle.FwdNoClamp, new Vector3(contrast), new Vector3(0.4f * (1f - contrast)), Vector3.One, saturation).Apply;

    private static Action<Span<Vector4>> Chained(Seq<Action<Span<Vector4>>> steps) => (Action<Span<Vector4>>)Delegate.Combine([.. steps])!;
}

[SmartEnum<string>]
public sealed partial class FilmicLook {
    public static readonly FilmicLook VeryHighContrast = new("very-high-contrast", Some("filmic_to_1.20_1-00.spi1d"));
    public static readonly FilmicLook HighContrast = new("high-contrast", Some("filmic_to_0.99_1-0075.spi1d"));
    public static readonly FilmicLook MediumHighContrast = new("medium-high-contrast", Some("filmic_to_0-85_1-011.spi1d"));
    public static readonly FilmicLook MediumContrast = new("medium-contrast", Some("filmic_to_0-70_1-03.spi1d"));
    public static readonly FilmicLook MediumLowContrast = new("medium-low-contrast", Some("filmic_to_0-60_1-04.spi1d"));
    public static readonly FilmicLook LowContrast = new("low-contrast", Some("filmic_to_0-48_1-09.spi1d"));
    public static readonly FilmicLook VeryLowContrast = new("very-low-contrast", Some("filmic_to_0-35_1-30.spi1d"));
    public static readonly FilmicLook Log = new("log", None);

    private readonly Option<string> resource;

    public Option<Lut1D> Curve => resource.Map(FormationTables.Curve);
}

[SmartEnum<string>]
public sealed partial class FilmLook {
    public static readonly FilmLook Nostalgia = new("nostalgia", ViewTransform.FilmPrint.Default with {
        PreExposure = 5.563035f,
        Multiplier = ViewTransform.FilmPrint.Default.Multiplier with { Red = 1.1f, Blue = 1.2f },
        SigmoidSpan = 33f,
        NegativeExposure = 5.8f,
        Backlight = new(0.99f, 1.1f, 1.035989f),
        PrintDensity = 40f,
        BlackPoint = -5f,
        MidtoneSaturation = 1.1f,
    });

    public static readonly FilmLook Silver = new("silver", ViewTransform.FilmPrint.Default with {
        PreExposure = 3.9f,
        PreFilter = ViewTransform.FilmPrint.Default.PreFilter with { Red = 0.95f, Green = 0.975f },
        Multiplier = ViewTransform.FilmPrint.Default.Multiplier with { Blue = 1.06f },
        NegativeExposure = 4.7f,
        NegativeDensity = 7f,
        Backlight = ViewTransform.FilmPrint.Default.Backlight with { Red = 0.9992f, Green = 0.99f },
        PrintExposure = 4.7f,
        PrintDensity = 30f,
        BlackPoint = 0.5f,
        PostFilter = ViewTransform.FilmPrint.Default.PostFilter with { Blue = 0.96f },
        MidtoneSaturation = 1f,
    });

    public ViewTransform.FilmPrint Print { get; }
}

[SmartEnum]
public sealed partial class AgXBase {
    public static readonly AgXBase Rec1886 = new("AgX_Base_sRGB.cube", new ColorEncoding(Gamut.StandardRgb, Transfer.Gamma.Rec1886));
    public static readonly AgXBase DisplayP3 = new("AgX_Base_P3.cube", new ColorEncoding(Gamut.DisplayP3, Transfer.Gamma.Rec1886));
    public static readonly AgXBase Rec2020 = new("AgX_Base_Rec2020.cube", new ColorEncoding(Gamut.Rec2020, Transfer.Gamma.Rec1886));
    public static readonly AgXBase Rec2100Hlg = new("AgX_Rec2100-HLG_p3_lim.cube", new ColorEncoding(Gamut.Rec2020, Transfer.Hlg.Standard));

    private readonly string resource;

    public ColorEncoding Signal { get; }

    public LutTable Table => new LutTable.Cube(FormationTables.Lattice(resource), LutInterpolation.Tetrahedral);

    public static AgXBase For(DisplayDevice device) =>
        device.Map(
            srgb: Rec1886, displayP3: DisplayP3, rec1886: Rec1886, rec2020: Rec2020,
            rec2100PqHdr: Rec2100Hlg, rec2100PqSdr: DisplayP3, rec2100HlgHdr: Rec2100Hlg, rec2100HlgSdr: DisplayP3);
}

public sealed record SceneExposure(
    [property: Display(Name = "Exposure", Description = "Scene gain in stops")][property: Range(-10d, 10d)] float Stops,
    [property: Display(Name = "Metering", Description = "Luminance ranks and region the meter reads")] Metering Meter) : IPixelStage<SceneExposure> {
    public static SceneExposure Default { get; } = new(0f, new Metering(0f, 1f, 0f, FramePosition.TopLeft, FramePosition.BottomRight));

    public Fin<SceneExposure> Metered(PixelFrame frame, Gamut working) =>
        Meter.Metered(frame, working).Map(metered => this with { Stops = metered });

    public static Option<PixelPass> Pass(SceneExposure state, PassContext context) =>
        state.Stops is 0f ? None : Some<PixelPass>(new PixelPass.Color(Exposure.Shifted(state.Stops)));

    public static PassContext Continued(SceneExposure state, float mix, PassContext context) =>
        context with { Exposure = context.Exposure + double.Log2(double.Lerp(1d, double.Exp2(state.Stops), mix)) };
}

public sealed record PeakMapping(
    [property: Display(Name = "Target peak", Description = "Brightest display luminance highlights roll into")][property: Range(100d, 10_000d)] float Target,
    [property: Display(Name = "Display black", Description = "Display black as share of peak")][property: Range(0d, 1e-3d)] float Black,
    [property: Display(Name = "Knee offset", Description = "Distance below target peak where rolloff begins, BT.2390 knee at minimum")][property: Range(Peak.Bt2390, 2d)] float Knee) {
    public static PeakMapping Default { get; } = new((float)DisplayDevice.Rec2100PqHdr.Peak.Nits.ToDouble(), 0f, Peak.Bt2390);

    public Option<Action<Span<Vector4>>> Rolled(Transfer.Pq signal, Luminance source) => Peak.Rolled(signal, source, Luminance.FromNits(Target), Black, Knee);
}

public sealed record CurveControls(
    [property: Display(Name = "Preserve hue", Description = "Share of pixel hue kept through curve")][property: Range(0d, 1d)] float PreserveHue,
    [property: Display(Name = "Purity", Description = "Share of primary inset restored after curve")][property: Range(0d, 1d)] float Purity,
    [property: Display(Name = "Base primaries", Description = "Gamut of rendering primaries, working gamut when unset")] Primaries? Basis,
    [property: Display(Name = "Inset", Description = "Rendering primary pull to white")][property: Range(0d, 0.99d)] Channels<float> Inset,
    [property: Display(Name = "Rotation", Description = "Rendering primary turn about white in radians")][property: Range(-0.29d, 0.29d)] Channels<float> Rotation) {
    public static CurveControls Default { get; } = new(1f, 0f, Basis: null, new(0f), new(0f));

    public Action<Span<Vector4>> Applied(PassContext context, DisplayDevice device, Func<float, float> curve) =>
        ToneCurves.Applied(context.Working, device, Optional(Basis).Map(static basis => basis.Gamut).IfNone(context.Working),
            new CurveApplication(PreserveHue, Purity, new Vector3(Inset.Red, Inset.Green, Inset.Blue), new Vector3(Rotation.Red, Rotation.Green, Rotation.Blue)), curve);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
[JsonPolymorphic]
[JsonDerivedType(typeof(AgXView), nameof(AgXView))]
[JsonDerivedType(typeof(AgXFalseColor), nameof(AgXFalseColor))]
[JsonDerivedType(typeof(AcesOutput), nameof(AcesOutput))]
[JsonDerivedType(typeof(FilmicView), nameof(FilmicView))]
[JsonDerivedType(typeof(FilmPrint), nameof(FilmPrint))]
[JsonDerivedType(typeof(TonyMcMapface), nameof(TonyMcMapface))]
[JsonDerivedType(typeof(Sigmoid), nameof(Sigmoid))]
[JsonDerivedType(typeof(HablePiecewise), nameof(HablePiecewise))]
[JsonDerivedType(typeof(Logarithmic), nameof(Logarithmic))]
[JsonDerivedType(typeof(Photoreceptor), nameof(Photoreceptor))]
[JsonDerivedType(typeof(Reinhard), nameof(Reinhard))]
[JsonDerivedType(typeof(ReinhardExtended), nameof(ReinhardExtended))]
[JsonDerivedType(typeof(Hable), nameof(Hable))]
[JsonDerivedType(typeof(PbrNeutral), nameof(PbrNeutral))]
public abstract partial record ViewTransform {
    public abstract PixelPass Formed(PassContext context, DisplayDevice device);

    private static PixelPass Whitened(PassContext context, float? stops, Func<float, Action<Span<Vector4>>> formed) =>
        Optional(stops).Match(
            Some: chosen => (PixelPass)new PixelPass.Color(formed(float.Exp2(chosen))),
            None: () => new PixelPass.Frame((frame, progress) =>
                LuminanceSamples.Gather(frame, context.Working).Bind(samples => new PixelPass.Color(formed((float)samples.FrameWhite)).Run(frame, progress))));

    [Display(Name = "AgX")]
    public sealed record AgXView([property: Display(Name = "Look", Description = "Grade in AgX Log before the AgX base")] AgXLook? Look) : ViewTransform {
        public static AgXView Default { get; } = new(Look: null);

        public override PixelPass Formed(PassContext context, DisplayDevice device) =>
            AgXBase.For(device) switch {
                var basis => new PixelPass.Color(AgX.Formed(
                    context.Working, device, FormationTables.AgXLog,
                    Optional(Look).Map(static look => AgX.Looked(FormationTables.AgXLog, look.Graded(FormationTables.AgXLog))).IfNone([]), basis.Table, basis.Signal)),
            };
    }

    [Display(Name = "AgX false color")]
    public sealed record AgXFalseColor([property: Display(Name = "Look", Description = "Grade in AgX Log before the false color bands")] AgXLook? Look) : ViewTransform {
        public static AgXFalseColor Default { get; } = new(Look: null);

        public override PixelPass Formed(PassContext context, DisplayDevice device) =>
            new PixelPass.Color(FalseColor.Formed(
                context.Working, device, FormationTables.AgXLog,
                Optional(Look).Map(static look => AgX.Looked(FormationTables.AgXLog, look.Graded(FormationTables.AgXLog))).IfNone([]), AgXBase.Rec2020.Table, FormationTables.FalseColorKey));
    }

    [Display(Name = "ACES 2.0")]
    public sealed record AcesOutput : ViewTransform {
        public static AcesOutput Default { get; } = new();

        public override PixelPass Formed(PassContext context, DisplayDevice device) =>
            Aces.Formed(context.Working, device).Match<PixelPass>(Succ: static formed => new PixelPass.Color(formed), Fail: static fault => new PixelPass.Frame((_, _) => fault));
    }

    public sealed record FilmicView([property: Display(Name = "Contrast", Description = "Contrast look, log shows base log encoding")] FilmicLook Look) : ViewTransform {
        public static FilmicView Default { get; } = new(FilmicLook.MediumContrast);

        public override PixelPass Formed(PassContext context, DisplayDevice device) =>
            new PixelPass.Color(Filmic.Formed(context.Working, device, FormationTables.FilmicDesaturation, Look.Curve));
    }

    public sealed record FilmPrint(
        [property: Display(Name = "Pre-exposure", Description = "Stops before film stock")][property: Range(-10d, 10d)] float PreExposure,
        [property: Display(Name = "Pre-filter", Description = "Transmittance of lens filter")][property: Range(0d, 1d)] Channels<float> PreFilter,
        [property: Display(Name = "Scale", Description = "Primary gamut extension")][property: Range(1d, 2d)] Channels<float> Scale,
        [property: Display(Name = "Rotation", Description = "Primary hue turn in radians")][property: Range(-double.Pi / 6d, double.Pi / 6d)] Channels<float> Rotation,
        [property: Display(Name = "Multiplier", Description = "Primary gain")][property: Range(0.5d, 2d)] Channels<float> Multiplier,
        [property: Display(Name = "Sigmoid minimum", Description = "Lowest stop dye response reads")][property: Range(-32d, 32d)] float SigmoidMinimum,
        [property: Display(Name = "Sigmoid span", Description = "Stops dye response reads above minimum")][property: Range(0d, 64d)] float SigmoidSpan,
        [property: Display(Name = "Toe X", Description = "Toe input")][property: Range(0d, 1d)] float ToeX,
        [property: Display(Name = "Toe Y", Description = "Toe output")][property: Range(0d, 1d)] float ToeY,
        [property: Display(Name = "Shoulder reach X", Description = "Share of input above toe where shoulder starts")][property: Range(0d, 1d)] float ShoulderReachX,
        [property: Display(Name = "Shoulder reach Y", Description = "Share of output above toe where shoulder starts")][property: Range(0d, 1d)] float ShoulderReachY,
        [property: Display(Name = "Negative exposure", Description = "Light on negative in stops")][property: Range(-10d, 10d)] float NegativeExposure,
        [property: Display(Name = "Negative density", Description = "Negative maximum dye density")][property: Range(0d, 149d)] float NegativeDensity,
        [property: Display(Name = "Backlight", Description = "Light printing negative")][property: Range(0.5d, 2d)] Channels<float> Backlight,
        [property: Display(Name = "Print exposure", Description = "Backlight on print in stops")][property: Range(-10d, 10d)] float PrintExposure,
        [property: Display(Name = "Print density", Description = "Print maximum dye density")][property: Range(0d, 149d)] float PrintDensity,
        [property: Display(Name = "Black point", Description = "Film black in per mille, stock black when unset")][property: Range(-10d, 10d)] float? BlackPoint,
        [property: Display(Name = "Post-filter", Description = "Transmittance of print filter")][property: Range(0d, 1d)] Channels<float> PostFilter,
        [property: Display(Name = "Midtone saturation", Description = "Saturation factor at middle gray")][property: Range(0d, 2d)] float MidtoneSaturation)
        : ViewTransform {
        public static FilmPrint Default { get; } = Of(FilmStock.Default);

        public FilmStock Stock => new(
            PreExposure, PreFilter, Scale, Rotation, Multiplier, SigmoidMinimum, SigmoidSpan, ToeX, ToeY, ShoulderReachX, ShoulderReachY,
            NegativeExposure, NegativeDensity, Backlight, PrintExposure, PrintDensity, PostFilter, MidtoneSaturation) { BlackPoint = Optional(BlackPoint) };

        public static FilmPrint Of(FilmStock stock) =>
            new(stock.PreExposure, stock.PreFilter, stock.Scale, stock.Rotation, stock.Multiplier,
                stock.SigmoidMinimum, stock.SigmoidSpan, stock.ToeX, stock.ToeY, stock.ShoulderReachX, stock.ShoulderReachY,
                stock.NegativeExposure, stock.NegativeDensity, stock.Backlight, stock.PrintExposure, stock.PrintDensity,
                stock.BlackPoint.ToNullable(), stock.PostFilter, stock.MidtoneSaturation);

        public override PixelPass Formed(PassContext context, DisplayDevice device) =>
            new PixelPass.Frame((frame, progress) => context.Derivations.Derived(Stock, static stock => stock.Developed())
                .Bind(developed => new PixelPass.Color(FilmStock.Formed(context.Working, device, developed)).Run(frame, progress)));
    }

    [Display(Name = "Tony McMapface")]
    public sealed record TonyMcMapface : ViewTransform {
        public static TonyMcMapface Default { get; } = new();

        public override PixelPass Formed(PassContext context, DisplayDevice device) => new PixelPass.Color(Tony.Formed(context.Working, device, FormationTables.TonyMcMapface));
    }

    public sealed record Sigmoid(
        [property: Display(Name = "Contrast", Description = "Middle gray slope over stops")][property: Range(0.7d, 3d)] float Contrast,
        [property: Display(Name = "Skew", Description = "Contrast shift to shadows or highlights")][property: Range(-1d, 1d)] float Skew,
        [property: Display(Name = "Target black", Description = "Display black as share of white")][property: Range(0d, 0.01d)] float Black,
        [property: Display(Name = "Curve controls", Description = "Hue, purity and rendering primaries around the curve")] CurveControls Controls) : ViewTransform {
        public static Sigmoid Default { get; } = new(1.5f, 0f, 0.000152f, CurveControls.Default);

        public override PixelPass Formed(PassContext context, DisplayDevice device) =>
            new PixelPass.Color(Controls.Applied(context, device, ToneCurves.Sigmoid(Contrast, Skew, Black, device.Peak)));
    }

    public sealed record HablePiecewise(
        [property: Display(Name = "Toe strength", Description = "Toe depth below straight section")][property: Range(0d, 1d)] float ToeStrength,
        [property: Display(Name = "Toe length", Description = "Toe span along input")][property: Range(0d, 1d)] float ToeLength,
        [property: Display(Name = "Shoulder angle", Description = "Shoulder roll past white")][property: Range(0d, 1d)] float ShoulderAngle,
        [property: Display(Name = "Gamma", Description = "Straight section power")][property: Range(0.7d, 3d)] float Gamma,
        [property: Display(Name = "Shoulder strength", Description = "Highlight stops in shoulder past straight section")][property: Range(0d, 16d)] float ShoulderStrength,
        [property: Display(Name = "Shoulder length", Description = "Shoulder span along output")][property: Range(1e-5d, 1d)] float ShoulderLength,
        [property: Display(Name = "Curve controls", Description = "Hue, purity and rendering primaries around the curve")] CurveControls Controls) : ViewTransform {
        public static HablePiecewise Default { get; } = new(0.5f, 0.5f, 1f, 1f, 2f, 0.5f, CurveControls.Default);

        public override PixelPass Formed(PassContext context, DisplayDevice device) =>
            new PixelPass.Color(Controls.Applied(context, device, ToneCurves.Piecewise(ToeStrength, ToeLength, ShoulderAngle, Gamma, ShoulderStrength, ShoulderLength)));
    }

    public sealed record Logarithmic(
        [property: Display(Name = "Bias", Description = "Drago bias, lower values lift shadows")][property: Range(0.5d, 1d)] float Bias,
        [property: Display(Name = "White point", Description = "Scene stops at display white, frame white when unset")][property: Range(-10d, 10d)] float? White,
        [property: Display(Name = "Curve controls", Description = "Hue, purity and rendering primaries around the curve")] CurveControls Controls) : ViewTransform {
        public static Logarithmic Default { get; } = new(0.85f, White: null, CurveControls.Default);

        public override PixelPass Formed(PassContext context, DisplayDevice device) =>
            Whitened(context, White, white => Controls.Applied(context, device, ToneCurves.Logarithmic(Bias, white, device.Peak)));
    }

    public sealed record Photoreceptor(
        [property: Display(Name = "Intensity", Description = "Response brightness")][property: Range(-8d, 8d)] float Intensity,
        [property: Display(Name = "Contrast", Description = "Response slope, read from frame when unset")][property: Range(0.3d, 1d)] float? Contrast,
        [property: Display(Name = "Adaptation", Description = "Pixel level share in adaptation level")][property: Range(0d, 1d)] float LightAdaptation,
        [property: Display(Name = "Color correction", Description = "Channel level share in adaptation level")][property: Range(0d, 1d)] float ChromaticAdaptation) : ViewTransform {
        public static Photoreceptor Default { get; } = new(0f, Contrast: null, 0f, 0f);

        public override PixelPass Formed(PassContext context, DisplayDevice device) =>
            new PixelPass.Frame((frame, progress) =>
                ToneCurves.Photoreceptor(context.Working, device, frame, Intensity, Optional(Contrast), LightAdaptation, ChromaticAdaptation)
                    .Bind(adapted => new PixelPass.Color(adapted).Run(frame, progress)));
    }

    public sealed record Reinhard : ViewTransform {
        public static Reinhard Default { get; } = new();

        public override PixelPass Formed(PassContext context, DisplayDevice device) =>
            new PixelPass.Color(ToneCurves.Mapped(context.Working, device, ToneMapOperator.Reinhard, new ToneMapParameters()));
    }

    public sealed record ReinhardExtended(
        [property: Display(Name = "White point", Description = "Scene stops at display white, frame white when unset")][property: Range(-10d, 10d)] float? White) : ViewTransform {
        public static ReinhardExtended Default { get; } = new(White: null);

        public override PixelPass Formed(PassContext context, DisplayDevice device) =>
            Whitened(context, White, white => ToneCurves.Mapped(context.Working, device, ToneMapOperator.ReinhardExtended, new ToneMapParameters(whitePoint: white)));
    }

    public sealed record Hable(
        [property: Display(Name = "Shoulder strength", Description = "Hable's A")][property: Range(0.001d, 0.5d)] float A,
        [property: Display(Name = "Linear strength", Description = "Hable's B")][property: Range(0d, 0.6d)] float B,
        [property: Display(Name = "Linear angle", Description = "Hable's C")][property: Range(0.1d, 0.3d)] float C,
        [property: Display(Name = "Toe strength", Description = "Hable's D")][property: Range(0.001d, 0.9d)] float D,
        [property: Display(Name = "Toe numerator", Description = "Hable's E, below C times F")][property: Range(0.0001d, 0.0199d)] float E,
        [property: Display(Name = "Toe denominator", Description = "Hable's F")][property: Range(0.2d, 0.6d)] float F,
        [property: Display(Name = "Linear white point", Description = "Scene stops normalized to white, frame white when unset")][property: Range(-10d, 10d)] float? White) : ViewTransform {
        public static Hable Default { get; } = new(0.15f, 0.5f, 0.1f, 0.2f, 0.02f, 0.3f, float.Log2(11.2f));

        public override PixelPass Formed(PassContext context, DisplayDevice device) =>
            Whitened(context, White, white => ToneCurves.Mapped(context.Working, device, ToneMapOperator.Hable, new ToneMapParameters(a: A, b: B, c: C, d: D, e: E, f: F, w: white)));
    }

    [Display(Name = "Khronos PBR Neutral")]
    public sealed record PbrNeutral : ViewTransform {
        public static PbrNeutral Default { get; } = new();

        public override PixelPass Formed(PassContext context, DisplayDevice device) => new PixelPass.Color(ToneCurves.PbrNeutral(context.Working, device));
    }
}

public sealed record ImageFormation(
    [property: Display(Name = "View transform", Description = "Scene light to display code formation")] ViewTransform View,
    [property: Display(Name = "Display", Description = "Display encoding and peak the view transform forms for")] DisplayDevice Device) : IPixelStage<ImageFormation> {
    public static ImageFormation Default { get; } = new(ViewTransform.AgXView.Default, DisplayDevice.Srgb);

    public static Seq<(string Name, Func<ImageFormation, ImageFormation> Look)> Looks { get; } =
        toSeq(AgXLook.Items).Map(static look => Viewed($"{nameof(ViewTransform.AgXView)} {look}", new ViewTransform.AgXView(look)))
        + toSeq(FilmicLook.Items).Map(static look => Viewed($"{nameof(ViewTransform.FilmicView)} {look}", new ViewTransform.FilmicView(look)))
        + toSeq(FilmLook.Items).Map(static look => Viewed($"{nameof(ViewTransform.FilmPrint)} {look}", look.Print));

    public static Option<PixelPass> Pass(ImageFormation state, PassContext context) => Some(state.View.Formed(context, state.Device));

    public static PassContext Continued(ImageFormation state, float mix, PassContext context) =>
        context with { Display = state.Device, Working = state.Device.Encoding.Gamut, Signal = state.Device.Encoding.Transfer };

    private static (string Name, Func<ImageFormation, ImageFormation> Look) Viewed(string name, ViewTransform view) => (name, formation => formation with { View = view });
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class FormationTables {
    public static Func<string, Lut3D> Lattice { get; } = memo(static (string resource) =>
        Parsed(resource, LutFormat.Cube).Bind(Fin<Lut3D> (table) => table is LutTable.Cube { Lattice: var lattice } ? lattice : new DarkroomFault($"embedded table {resource}", DarkroomReason.Lattice)).ThrowIfFail());

    public static Func<string, Lut1D> Curve { get; } = memo(static (string resource) =>
        Parsed(resource, LutFormat.Spi1d).Bind(Fin<Lut1D> (table) => table is LutTable.ChannelCurve { Table: var curve } ? curve : new DarkroomFault($"embedded table {resource}", DarkroomReason.Curve)).ThrowIfFail());

    private static readonly Lazy<LogSpace> AgXSpace = new(static () => LogSpace.AgX(Lattice("luminance_compensation_bt2020.cube")));
    private static readonly Lazy<Lut3D> TonyTable = new(static () => {
        using Stream stream = Opened("tony_mc_mapface.dds");
        using MemoryStream texture = new();
        stream.CopyTo(texture);
        return Tony.Table(texture.ToArray()).ThrowIfFail();
    });

    public static LogSpace AgXLog => AgXSpace.Value;
    public static Lut3D FilmicDesaturation => Lattice("filmic_desat_33.cube");
    public static Lut1D FalseColorKey => Curve("AgX_False_Color.spi1d");
    public static Lut3D TonyMcMapface => TonyTable.Value;

    private static Stream Opened(string resource) => typeof(FormationTables).Assembly.GetManifestResourceStream(resource)!;

    private static Fin<LutTable> Parsed(string resource, LutFormat format) {
        using Stream stream = Opened(resource);
        using MemoryStream bytes = new();
        stream.CopyTo(bytes);
        return format.Decode(bytes.ToArray());
    }
}
