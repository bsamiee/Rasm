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
    public static readonly AgXLook Punchy = new("punchy", [
        .. (GradingTone.Neutral(GradingStyle.Log) with { Shadows = new GradingRgbmsw(0.2f, 0.2f, 0.2f, 0.35f, 0.4f, 0.3f) }).Graded(GradingStyle.Log).ToSeq(),
        new LutTable.Cdl(CdlStyle.FwdNoClamp, Vector3.One, Vector3.Zero, new Vector3(1.0912f), 1f).Apply]);
    public static readonly AgXLook Greyscale = new("greyscale", [LutSpace.Filmic.Back.Apply, ViewTransform.AgXGreyscale.Apply, LutSpace.Filmic.Into.Apply]);
    public static readonly AgXLook VeryHighContrast = new("very-high-contrast", Contrasted(1.57f, 0.9f));
    public static readonly AgXLook HighContrast = new("high-contrast", Contrasted(1.4f, 0.95f));
    public static readonly AgXLook MediumHighContrast = new("medium-high-contrast", Contrasted(1.2f, 1f));
    public static readonly AgXLook BaseContrast = new("base-contrast", Contrasted(1f, 1f));
    public static readonly AgXLook MediumLowContrast = new("medium-low-contrast", Contrasted(0.9f, 1.05f));
    public static readonly AgXLook LowContrast = new("low-contrast", Contrasted(0.8f, 1.1f));
    public static readonly AgXLook VeryLowContrast = new("very-low-contrast", Contrasted(0.7f, 1.15f));

    public Seq<Action<Span<Vector4>>> Steps { get; }

    private static Seq<Action<Span<Vector4>>> Contrasted(float contrast, float saturation) =>
        [new LutTable.Cdl(CdlStyle.FwdNoClamp, new Vector3(contrast), new Vector3(0.4f * (1f - contrast)), Vector3.One, saturation).Apply];
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

[SmartEnum]
public sealed partial class AgXBase {
    public static readonly AgXBase Rec1886 = new("AgX_Base_sRGB.cube", new ColorEncoding(Gamut.StandardRgb, Transfer.Gamma.Rec1886));
    public static readonly AgXBase DisplayP3 = new("AgX_Base_P3.cube", new ColorEncoding(Gamut.DisplayP3, Transfer.Gamma.Rec1886));
    public static readonly AgXBase Rec2020 = new("AgX_Base_Rec2020.cube", new ColorEncoding(Gamut.Rec2020, Transfer.Gamma.Rec1886));
    public static readonly AgXBase Rec2100Hlg = new("AgX_Rec2100-HLG_p3_lim.cube", new ColorEncoding(Gamut.Rec2020, Transfer.Hlg.Standard));

    private readonly string resource;

    public ColorEncoding Signal { get; }

    public LutTable Table => new LutTable.Cube(FormationTables.Lattice(resource), LutInterpolation.Tetrahedral);
}

[SmartEnum<string>]
public sealed partial class Display {
    public static readonly Display Srgb = new("srgb", DisplayDevice.Srgb, AgXBase.Rec1886);
    public static readonly Display DisplayP3 = new("display-p3", DisplayDevice.DisplayP3, AgXBase.DisplayP3);
    public static readonly Display Rec1886 = new("rec1886", DisplayDevice.Rec1886, AgXBase.Rec1886);
    public static readonly Display Rec2020 = new("rec2020", DisplayDevice.Rec2020, AgXBase.Rec2020);
    public static readonly Display Rec2100PqHdr = new("rec2100-pq-hdr", DisplayDevice.Rec2100PqHdr, AgXBase.Rec2100Hlg);
    public static readonly Display Rec2100PqSdr = new("rec2100-pq-sdr", DisplayDevice.Rec2100PqSdr, AgXBase.DisplayP3);
    public static readonly Display Rec2100HlgHdr = new("rec2100-hlg-hdr", DisplayDevice.Rec2100HlgHdr, AgXBase.Rec2100Hlg);
    public static readonly Display Rec2100HlgSdr = new("rec2100-hlg-sdr", DisplayDevice.Rec2100HlgSdr, AgXBase.DisplayP3);

    public DisplayDevice Device { get; }
    public AgXBase AgX { get; }
}

public sealed record SceneExposure(float Stops, Metering Meter) : IPixelStage<SceneExposure> {
    public static SceneExposure Default { get; } = new(0f, Metering.Default);

    public Fin<SceneExposure> Metered(PixelFrame frame, Gamut working) =>
        Meter.Metered(frame, working).Map(metered => this with { Stops = metered });

    public static Fin<Option<PixelPass>> Pass(SceneExposure state, PassContext context) =>
        state.Stops is 0f ? None : Some<PixelPass>(new PixelPass.Point(new LutTable.Affine(Matrix4x4.CreateScale(float.Exp2(state.Stops))).Apply));

    public static PassContext Continued(SceneExposure state, float mix, PassContext context) =>
        context with { Exposure = context.Exposure + double.Log2(double.Lerp(1d, double.Exp2(state.Stops), mix)) };
}

public sealed record PeakMapping(Luminance Target, float Black, float Knee) {
    public static PeakMapping Default { get; } = new(DisplayDevice.Rec2100PqHdr.Peak, 0f, DisplayDevice.Bt2390KneeOffset);

    public Option<Action<Span<Vector4>>> Rolled(DisplayDevice device, Luminance source) => (device with { Peak = Target }).Eetf(source, Black, Knee);
}

public sealed record CurveControls(float PreserveHue, float Purity, Option<Gamut> Basis, Vector3 Inset, Vector3 Rotation) {
    public static CurveControls Default { get; } = new(1f, 0f, None, Vector3.Zero, Vector3.Zero);

    public ViewTransform Applied(PassContext context, Func<float, float> curve) =>
        ViewTransform.Curve(Basis.IfNone(context.Working), curve, PreserveHue, Purity, Inset, Rotation);
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
public abstract partial record ToneMapping {
    public abstract Fin<PixelPass> Formed(PassContext context, Display display);

    private static Fin<PixelPass> Pointed(PassContext context, Display display, Fin<ViewTransform> view) =>
        view.Map(formed => (PixelPass)new PixelPass.Point(formed.Formed(new ColorEncoding(context.Working, context.Signal), display.Device.Encoding)));

    private static Fin<PixelPass> Framed(PassContext context, Display display, Func<PixelFrame, Fin<ViewTransform>> view) =>
        new PixelPass.Frame((frame, progress) => Pointed(context, display, view(frame)).Bind(pass => pass.Run(frame, progress)));

    private static Fin<PixelPass> Whitened(PassContext context, Display display, Option<float> stops, Func<float, ViewTransform> formed) =>
        stops.Match(
            Some: chosen => Pointed(context, display, formed(float.Exp2(chosen))),
            None: () => Framed(context, display, frame => LuminanceSamples.Gather(frame, context.Working).Map(samples => formed((float)samples.FrameWhite))));

    public sealed record AgXView(Option<AgXLook> Look) : ToneMapping {
        public static AgXView Default { get; } = new(None);

        public override Fin<PixelPass> Formed(PassContext context, Display display) =>
            Pointed(context, display, ViewTransform.AgX(FormationTables.AgXLog, Look.ToSeq().Bind(static look => look.Steps), display.AgX.Table, display.AgX.Signal));
    }

    public sealed record AgXFalseColor(Option<AgXLook> Look) : ToneMapping {
        public static AgXFalseColor Default { get; } = new(None);

        public override Fin<PixelPass> Formed(PassContext context, Display display) =>
            Pointed(context, display, ViewTransform.FalseColor(FormationTables.AgXLog, Look.ToSeq().Bind(static look => look.Steps), AgXBase.Rec2020.Table, FormationTables.FalseColorKey));
    }

    public sealed record AcesOutput : ToneMapping {
        public static AcesOutput Default { get; } = new();

        public override Fin<PixelPass> Formed(PassContext context, Display display) => Pointed(context, display, ViewTransform.Aces(display.Device));
    }

    public sealed record FilmicView(FilmicLook Look) : ToneMapping {
        public static FilmicView Default { get; } = new(FilmicLook.MediumContrast);

        public override Fin<PixelPass> Formed(PassContext context, Display display) =>
            Pointed(context, display, ViewTransform.Filmic(FormationTables.FilmicDesaturation, Look.Curve));
    }

    public sealed record FilmPrint(FilmStock Stock) : ToneMapping {
        public static FilmPrint Default { get; } = new(FilmStock.Default);

        public override Fin<PixelPass> Formed(PassContext context, Display display) =>
            Pointed(context, display, context.Derivations.Derived(Stock, static stock => stock.Developed()));
    }

    public sealed record TonyMcMapface : ToneMapping {
        public static TonyMcMapface Default { get; } = new();

        public override Fin<PixelPass> Formed(PassContext context, Display display) =>
            Pointed(context, display, ViewTransform.Lattice(LutSpace.Rational(Gamut.StandardRgb), FormationTables.TonyMcMapface, new ColorEncoding(Gamut.StandardRgb, new Transfer.Linear())));
    }

    public sealed record Sigmoid(float Contrast, float Skew, float Black, CurveControls Controls) : ToneMapping {
        public static Sigmoid Default { get; } = new(1.5f, 0f, 0.000152f, CurveControls.Default);

        public override Fin<PixelPass> Formed(PassContext context, Display display) =>
            Pointed(context, display, Controls.Applied(context, ToneCurve.Sigmoid(Contrast, Skew, Black, display.Device.Peak)));
    }

    public sealed record HablePiecewise(float ToeStrength, float ToeLength, float ShoulderAngle, float Gamma, float ShoulderStrength, float ShoulderLength, CurveControls Controls) : ToneMapping {
        public static HablePiecewise Default { get; } = new(0.5f, 0.5f, 1f, 1f, 2f, 0.5f, CurveControls.Default);

        public override Fin<PixelPass> Formed(PassContext context, Display display) =>
            Pointed(context, display, Controls.Applied(context, ToneCurve.Piecewise(ToeStrength, ToeLength, ShoulderAngle, Gamma, ShoulderStrength, ShoulderLength)));
    }

    public sealed record Logarithmic(float Bias, Option<float> White, CurveControls Controls) : ToneMapping {
        public static Logarithmic Default { get; } = new(0.85f, None, CurveControls.Default);

        public override Fin<PixelPass> Formed(PassContext context, Display display) =>
            Whitened(context, display, White, white => Controls.Applied(context, ToneCurve.Drago(Bias, white, display.Device.Peak)));
    }

    public sealed record Photoreceptor(float Intensity, Option<float> Contrast, float LightAdaptation, float ChromaticAdaptation) : ToneMapping {
        public static Photoreceptor Default { get; } = new(0f, None, 0f, 0f);

        public override Fin<PixelPass> Formed(PassContext context, Display display) =>
            Framed(context, display, frame => ViewTransform.Photoreceptor(context.Working, frame, Intensity, Contrast, LightAdaptation, ChromaticAdaptation));
    }

    public sealed record Reinhard : ToneMapping {
        public static Reinhard Default { get; } = new();

        public override Fin<PixelPass> Formed(PassContext context, Display display) =>
            Pointed(context, display, ViewTransform.Curve(context.Working, ToneCurve.Operator(ToneMapOperator.Reinhard, new ToneMapParameters())));
    }

    public sealed record ReinhardExtended(Option<float> White) : ToneMapping {
        public static ReinhardExtended Default { get; } = new(None);

        public override Fin<PixelPass> Formed(PassContext context, Display display) =>
            Whitened(context, display, White, white => ViewTransform.Curve(context.Working, ToneCurve.Operator(ToneMapOperator.ReinhardExtended, new ToneMapParameters(whitePoint: white))));
    }

    public sealed record Hable(float A, float B, float C, float D, float E, float F, Option<float> White) : ToneMapping {
        public static Hable Default { get; } = new(0.15f, 0.5f, 0.1f, 0.2f, 0.02f, 0.3f, float.Log2(11.2f));

        public override Fin<PixelPass> Formed(PassContext context, Display display) =>
            Whitened(context, display, White, white => ViewTransform.Curve(context.Working, ToneCurve.Operator(ToneMapOperator.Hable, new ToneMapParameters(a: A, b: B, c: C, d: D, e: E, f: F, w: white))));
    }

    public sealed record PbrNeutral : ToneMapping {
        public static PbrNeutral Default { get; } = new();

        public override Fin<PixelPass> Formed(PassContext context, Display display) => Pointed(context, display, ViewTransform.PbrNeutral);
    }
}

public sealed record ImageFormation(ToneMapping View, Display Device) : IPixelStage<ImageFormation> {
    public static ImageFormation Default { get; } = new(ToneMapping.AgXView.Default, Display.Srgb);

    public static Seq<(string Name, Func<ImageFormation, ImageFormation> Look)> Looks { get; } =
        toSeq(AgXLook.Items).Map(static look => Viewed($"{nameof(ToneMapping.AgXView)} {look}", new ToneMapping.AgXView(look)))
        + toSeq(FilmicLook.Items).Map(static look => Viewed($"{nameof(ToneMapping.FilmicView)} {look}", new ToneMapping.FilmicView(look)))
        + Seq(Viewed($"{nameof(ToneMapping.FilmPrint)} {nameof(FilmStock.Nostalgia)}", new ToneMapping.FilmPrint(FilmStock.Nostalgia)),
            Viewed($"{nameof(ToneMapping.FilmPrint)} {nameof(FilmStock.Silver)}", new ToneMapping.FilmPrint(FilmStock.Silver)));

    public static Fin<Option<PixelPass>> Pass(ImageFormation state, PassContext context) => state.View.Formed(context, state.Device).Map(static pass => Some(pass));

    public static PassContext Continued(ImageFormation state, float mix, PassContext context) =>
        context with { Display = state.Device.Device, Working = state.Device.Device.Encoding.Gamut, Signal = state.Device.Device.Encoding.Transfer };

    private static (string Name, Func<ImageFormation, ImageFormation> Look) Viewed(string name, ToneMapping view) => (name, formation => formation with { View = view });
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class FormationTables {
    public static Func<(string Resource, LutFormat Format), LutTable> Table { get; } = memoUnsafe(static ((string Resource, LutFormat Format) source) => {
        using Stream stream = typeof(FormationTables).Assembly.GetManifestResourceStream(source.Resource)!;
        using MemoryStream bytes = new();
        stream.CopyTo(bytes);
        return source.Format.Decode(bytes.ToArray()).ThrowIfFail();
    });

    public static Func<string, Lut3D> Lattice { get; } = memoUnsafe(static (string resource) => {
        using StreamReader reader = new(typeof(FormationTables).Assembly.GetManifestResourceStream(resource)!);
        return Lut3D.ParseCube(reader.ReadToEnd());
    });

    public static LutSpace AgXLog { get; } = LutSpace.AgX(Lattice("luminance_compensation_bt2020.cube"));
    public static Lut3D FilmicDesaturation => Lattice("filmic_desat_33.cube");
    public static Lut1D FalseColorKey => Curve("AgX_False_Color.spi1d");
    public static LutTable TonyMcMapface => Table(("tony_mc_mapface.dds", LutFormat.DirectDrawSurface));

    public static Func<string, Lut1D> Curve { get; } = memoUnsafe(static (string resource) => {
        using StreamReader reader = new(typeof(FormationTables).Assembly.GetManifestResourceStream(resource)!);
        return LutFormat.ParseSpi1d(reader.ReadToEnd()).ThrowIfFail();
    });
}
