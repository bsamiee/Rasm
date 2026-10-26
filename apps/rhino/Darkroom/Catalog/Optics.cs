using System.Numerics;
using Darkroom.Chain;
using Emgu.CV;
using PeterO.Numbers;
using Rasm.Imaging.Files;
using Rasm.Imaging.Filtering;
using Rasm.Imaging.Grade;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Procedural;
using Rasm.Imaging.Warping;
using Rasm.UI;
using SkiaSharp;

namespace Darkroom.Catalog;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<int>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct GhostIterations : IPresented<GhostIterations, int> {
    public static GhostIterations MinValue { get; } = new(2);
    public static GhostIterations MaxValue { get; } = new(5);
    public static Presentation<int> Presentation { get; } = new();

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct DirtGain : IPresented<DirtGain, float> {
    public static DirtGain MinValue { get; } = new(0f);
    public static DirtGain MaxValue { get; } = new(5f);
    public static Presentation<float> Presentation { get; } = new() { Soft = (0f, 1f) };

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct DirtCoverage : IPresented<DirtCoverage, float> {
    public static DirtCoverage MinValue { get; } = new(0.01f);
    public static DirtCoverage MaxValue { get; } = new(1f);
    public static Presentation<float> Presentation => ProperFraction.Presentation;

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<float>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Off", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct DefocusRadius : IPresented<DefocusRadius, float> {
    public static DefocusRadius MinValue => Off;
    public static DefocusRadius MaxValue { get; } = new(0.1f * ReferenceFrame.GreaterSide);
    public static Presentation<float> Presentation => ProperFraction.Presentation;

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<float>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Neutral", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct Distortion : IPresented<Distortion, float> {
    public static Distortion MinValue { get; } = new((float)RadialDistortion.Bounds.Low);
    public static Distortion MaxValue { get; } = new((float)RadialDistortion.Bounds.High);
    public static Presentation<float> Presentation { get; } = new() { Origin = 0f };

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

public sealed record DirtSpecks(DirtCoverage Coverage, ShortSideExtent Scale, Seed Seed) : IStateRecord<DirtSpecks, DirtSpecksParameter> {
    private const uint Stream = 21u;

    public static DirtSpecks Default { get; } = new(DirtCoverage.Create(0.2f), ShortSideExtent.Create(0.05f * ReferenceFrame.GreaterSide), Seed.MinValue);

    public static PixelFrame Field((DirtSpecks Specks, PixelExtent Extent) key) =>
        PinkSpecks.Field(key.Extent, key.Specks.Scale.Pixels(key.Extent), key.Specks.Coverage, key.Specks.Seed.Field(Stream, 0u));
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class DirtSpecksParameter : IStateParameter<DirtSpecks> {
    public static readonly DirtSpecksParameter Coverage = new("coverage", StateParameter.Bounded(Lens<DirtSpecks, DirtCoverage>.New(static specks => specks.Coverage, static coverage => specks => specks with { Coverage = coverage })),
        new("Coverage", "Share of the lens covered by dirt"));
    public static readonly DirtSpecksParameter Scale = new("scale", StateParameter.Bounded(Lens<DirtSpecks, ShortSideExtent>.New(static specks => specks.Scale, static scale => specks => specks with { Scale = scale }),
        Presentations.Extent with { Soft = (0.001f, 0.5f), Track = Some<Func<AxisRange, Axis>>(static range => new Axis.Linear(range)) }),
        new("Scale", "Dirt feature size in frame short sides"));
    public static readonly DirtSpecksParameter Seed = new("seed", StateParameter.Bounded(Lens<DirtSpecks, Seed>.New(static specks => specks.Seed, static seed => specks => specks with { Seed = seed })),
        new("Seed", "Number selecting the dirt field"));

    public StateParameter<DirtSpecks> Kind { get; }
    public ParameterText Text { get; }
}

public sealed record Bloom(Fraction Size, Fraction Strength, Option<ImageFile> Dirt, Gated<DirtSpecks> Specks, DirtGain DirtGain) : IStateRecord<Bloom, BloomParameter>, IPixelStage<Bloom> {
    public static Bloom Default { get; } = new(Fraction.Create(0.5f), Fraction.Create(0.7f * float.Pow(0.15f, 1.5f)), None, new(Enabled: false, DirtSpecks.Default), DirtGain.Create(1f));

    public static Option<PixelPass> Pass(Bloom state, PassContext context) =>
        state.Strength == Fraction.MinValue ? None : Some<PixelPass>(new PixelPass.Frame((frame, progress) => frame.Spread(LightSpread.Light, light => {
            _ = TentPyramid.Spread(light, (int)float.Log2(float.Max(1f, frame.Size.ShortSide * state.Size)) - 1);
            return (state.Specks.Active.Map(specks => context.Derivations.Derived((specks, context.Extent), DirtSpecks.Field)) | state.Dirt.Map(static file => file.Frame))
                .Map(dirt => TentPyramid.Soiled(light, frame, dirt, state.DirtGain)).IfNone(unit);
        }, BlendingMode.Mix, state.Strength, progress)));
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class BloomParameter : IStateParameter<Bloom> {
    public static readonly BloomParameter Size = new("size", StateParameter.Bounded(Lens<Bloom, Fraction>.New(static bloom => bloom.Size, static size => bloom => bloom with { Size = size })),
        new("Size", "Spread reach across the frame"));
    public static readonly BloomParameter Strength = new("strength", StateParameter.Bounded(Lens<Bloom, Fraction>.New(static bloom => bloom.Strength, static strength => bloom => bloom with { Strength = strength })),
        new("Strength", "Share of light the lens scatters with total light kept"));
    public static readonly BloomParameter Dirt = new("dirt", new StateParameter<Bloom>.Loaded<ImageFile, ImagePath, string>(Lens<Bloom, Option<ImageFile>>.New(static bloom => bloom.Dirt, static dirt => bloom => bloom with { Dirt = dirt }), ImageFile.Load, static file => file.Path),
        new("Lens dirt", "Scene-linear dust and smudge image scattered light shows"));
    public static readonly BloomParameter Specks = new("specks", new StateParameter<Bloom>.OptionalRecord<DirtSpecks>(Lens<Bloom, Gated<DirtSpecks>>.New(static bloom => bloom.Specks, static specks => bloom => bloom with { Specks = specks })),
        new("Specks", "Procedural lens dirt in place of the dirt image"));
    public static readonly BloomParameter DirtGain = new("dirt-gain", StateParameter.Bounded(Lens<Bloom, DirtGain>.New(static bloom => bloom.DirtGain, static gain => bloom => bloom with { DirtGain = gain })),
        new("Dirt intensity", "Dirt gain over scattered light"));

    public StateParameter<Bloom> Kind { get; }
    public ParameterText Text { get; }
}

public sealed record VeilingGlare(Fraction Size) : IStateRecord<VeilingGlare, VeilingGlareParameter>, IPixelStage<VeilingGlare> {
    public static VeilingGlare Default { get; } = new(Fraction.Create(0.5f));

    public static Option<PixelPass> Pass(VeilingGlare state, PassContext context) =>
        Some<PixelPass>(new PixelPass.Frame((frame, progress) => frame.Spread(LightSpread.Light,
            light => context.Derivations.Held((state.Size, context.Extent, Window: frame.Size),
                static key => Fin.Succ(PointSpread.Veiling(key.Window, double.Lerp(180d, 10d, double.Cbrt(key.Size)) / int.Max(key.Extent.Width, key.Extent.Height))),
                spread => spread.Convolve(light)),
            BlendingMode.Mix, 1f, progress)));
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class VeilingGlareParameter : IStateParameter<VeilingGlare> {
    public static readonly VeilingGlareParameter Size = new("size", StateParameter.Bounded(Lens<VeilingGlare, Fraction>.New(static glare => glare.Size, static size => glare => glare with { Size = size })),
        new("Size", "Field of view the glare kernel spans, narrower at higher values"));

    public StateParameter<VeilingGlare> Kind { get; }
    public ParameterText Text { get; }
}

public sealed record StarGlare(Aperture Aperture, ShortSideLength Radius) : IStateRecord<StarGlare, StarGlareParameter>, IPixelStage<StarGlare> {
    public static StarGlare Default { get; } = new(Aperture.Default, ShortSideLength.Create(2f / ReferenceFrame.Height));

    public static Option<PixelPass> Pass(StarGlare state, PassContext context) =>
        state.Radius == ShortSideLength.Neutral || state.Aperture.Obstruction == Fraction.MaxValue ? None
            : Some<PixelPass>(new PixelPass.Frame((frame, progress) => frame.Spread(LightSpread.Light,
                light => context.Derivations.Held((State: state, context.Extent, Window: frame.Size, context.Working),
                    static key => key.State.Aperture.Pupil.Map(pupil => PointSpread.Diffraction(pupil, key.State.Radius.Pixels(key.Extent), key.Window, key.Extent, key.Working)),
                    spread => spread.Convolve(light)),
                BlendingMode.Mix, 1f, progress)));
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class StarGlareParameter : IStateParameter<StarGlare> {
    public static readonly StarGlareParameter Aperture = new("iris", new StateParameter<StarGlare>.Record<Aperture>(Lens<StarGlare, Aperture>.New(static glare => glare.Aperture, static aperture => glare => glare with { Aperture = aperture })),
        new("Aperture", "Lens opening shaping bokeh and diffraction spikes"));
    public static readonly StarGlareParameter Radius = new("airy-radius", StateParameter.Bounded(Lens<StarGlare, ShortSideLength>.New(static glare => glare.Radius, static radius => glare => glare with { Radius = radius }),
        Presentations.Length with { Soft = (1.25f / ReferenceFrame.Height, 64f / ReferenceFrame.Height) }),
        new("Core", "First dark ring radius around point lights in frame short sides"));

    public StateParameter<StarGlare> Kind { get; }
    public ParameterText Text { get; }
}

public sealed record Ghosts(Highlights Highlight, GhostIterations Iterations, Fraction Modulation) : IStateRecord<Ghosts, GhostsParameter>, IPixelStage<Ghosts> {
    public static Ghosts Default { get; } = new(Highlights.Default, GhostIterations.Create(3), Fraction.Create(0.25f));

    public static Option<PixelPass> Pass(Ghosts state, PassContext context) =>
        Some<PixelPass>(new PixelPass.Frame((frame, progress) => state.Highlight.Key.Bind(key => frame.Spread(key.Extract,
            light => GhostReflections.Reflected(light, frame, 16f / ReferenceFrame.Height * frame.Extent.ShortSide, state.Iterations, state.Modulation), BlendingMode.Add, 1f, progress))));
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class GhostsParameter : IStateParameter<Ghosts> {
    public static readonly GhostsParameter Highlight = new("highlight", new StateParameter<Ghosts>.Record<Highlights>(Lens<Ghosts, Highlights>.New(static ghosts => ghosts.Highlight, static highlight => ghosts => ghosts with { Highlight = highlight })),
        new("Highlights", "Highlights the lens reflects"));
    public static readonly GhostsParameter Iterations = new("iterations", StateParameter.Bounded(Lens<Ghosts, GhostIterations>.New(static ghosts => ghosts.Iterations, static iterations => ghosts => ghosts with { Iterations = iterations })),
        new("Iterations", "Reflection passes, each adding four ghosts"));
    public static readonly GhostsParameter Modulation = new("color-modulation", StateParameter.Bounded(Lens<Ghosts, Fraction>.New(static ghosts => ghosts.Modulation, static modulation => ghosts => ghosts with { Modulation = modulation })),
        new("Color modulation", "Color shift between successive ghosts"));

    public StateParameter<Ghosts> Kind { get; }
    public ParameterText Text { get; }
}

public sealed record Halation(Highlights Highlight, ShortSideLength Radius, SKColorF Color) : IStateRecord<Halation, HalationParameter>, IPixelStage<Halation> {
    public static Halation Default { get; } = new(new Highlights(HighlightLevel.OneStopOver, Fraction.MaxValue), ShortSideLength.Create(64f * 0.2f / ReferenceFrame.Height), new SKColorF(1f, 0x59 / 255f, 0f));

    public static Option<PixelPass> Pass(Halation state, PassContext context) =>
        Some<PixelPass>(new PixelPass.Frame((frame, progress) => (RampLight.Light(state.Color, context.Working) with { W = 0f }) switch {
            var color => state.Highlight.Key.Bind(key => frame.Spread(pixel => key.Extract(pixel) * color, light => light.Blur(light, state.Radius.Pixels(frame.Extent)), BlendingMode.Add, 1f, progress)),
        }));
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class HalationParameter : IStateParameter<Halation> {
    public static readonly HalationParameter Highlight = new("highlight", new StateParameter<Halation>.Record<Highlights>(Lens<Halation, Highlights>.New(static halation => halation.Highlight, static highlight => halation => halation with { Highlight = highlight })),
        new("Highlights", "Highlights that re-expose the film"));
    public static readonly HalationParameter Radius = new("radius", StateParameter.Bounded(Lens<Halation, ShortSideLength>.New(static halation => halation.Radius, static radius => halation => halation with { Radius = radius }),
        Presentations.Length with { Soft = (0f, 64f / ReferenceFrame.Height) }),
        new("Radius", "Halo radius in frame short sides"));
    public static readonly HalationParameter Color = new("color", new StateParameter<Halation>.Color(Lens<Halation, SKColorF>.New(static halation => halation.Color, static color => halation => halation with { Color = color })),
        new("Color", "Color weighting the halo"));

    public StateParameter<Halation> Kind { get; }
    public ParameterText Text { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FocusField : ICaptioned {
    public static readonly FocusField Uniform = new("uniform", [], static (state, pupil, _, cap) => Some(Bokeh.Uniform(pupil, state.Fringe, cap)));
    public static readonly FocusField Depth = new("depth", [GuideChannel.Depth], static (state, _, context, cap) =>
        from depth in context.Guides.Find(GuideChannel.Depth)
        from camera in context.Camera
        from field in Bokeh.Depth(depth, camera, state.FStop, cap, context.Extent)
        select field);
    public static readonly FocusField Curvature = new("curvature", [], static (state, _, context, cap) => Some(state.Focal(context, state.Aspect, 2, cap)));
    public static readonly FocusField Tilt = new("tilt", [], static (state, _, context, cap) => Some(state.Focal(context, 0d, 1, cap)));

    public Seq<GuideChannel> Guides { get; }

    [UseDelegateFromConstructor]
    public partial Option<Func<Mat, Mat, PixelFrame, Fin<Unit>>> Field(Defocus state, Iris pupil, PassContext context, double cap);
}

public sealed record Defocus(
    FocusField Field, DefocusRadius Radius, Factor FStop, Placement Placement, ShortSideLength Extent, Fraction Aspect, ShortSideExtent Feather, Aperture Aperture, SignedFraction Fringe)
    : IStateRecord<Defocus, DefocusParameter>, IPixelStage<Defocus> {
    public static Defocus Default { get; } = new(FocusField.Depth, DefocusRadius.Create(0.02f * ReferenceFrame.GreaterSide), Factor.Create(2.8f), Placement.Default, ShortSideLength.Create(0.15f), Fraction.MaxValue, ShortSideExtent.Create(0.25f), Aperture.Default with { Roundness = Fraction.MaxValue }, SignedFraction.Neutral);

    public static Seq<GuideChannel> Channels(Defocus state) => state.Field.Guides;

    public static Option<PixelPass> Pass(Defocus state, PassContext context) =>
        state.Radius == DefocusRadius.Off ? None
            : state.Aperture.Pupil.Match(
                Succ: pupil => state.Field.Field(state, pupil, context, state.Radius * context.Extent.ShortSide).Map<PixelPass>(field => Bokeh.Gathered(pupil, state.Fringe, field)),
                Fail: static error => Some<PixelPass>(new PixelPass.Frame((_, _) => error)));

    public Func<Mat, Mat, PixelFrame, Fin<Unit>> Focal(PassContext context, double aspect, int power, double cap) =>
        Bokeh.Focal(Placement.Frame(context.Extent), Extent, Feather, aspect, power, cap);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class DefocusParameter : IStateParameter<Defocus> {
    public static readonly DefocusParameter Field = new("field", new StateParameter<Defocus>.Choice<FocusField>(Lens<Defocus, FocusField>.New(static defocus => defocus.Field, static field => defocus => defocus with { Field = field })),
        new("Focus", "Field setting blur radius across frame"));
    public static readonly DefocusParameter Radius = new("radius", StateParameter.Bounded(Lens<Defocus, DefocusRadius>.New(static defocus => defocus.Radius, static radius => defocus => defocus with { Radius = radius })),
        new("Max blur", "Largest blur radius in frame short sides"));
    public static readonly DefocusParameter FStop = new("f-stop", StateParameter.Bounded(Lens<Defocus, Factor>.New(static defocus => defocus.FStop, static aperture => defocus => defocus with { FStop = aperture }),
        Factor.Presentation with { Soft = (0.5f, 128f), Origin = None, Decimals = 1 }),
        new("F-stop", "Lens aperture ratio, lower values defocus more"));
    public static readonly DefocusParameter Placement = new("placement", new StateParameter<Defocus>.Record<Placement>(Lens<Defocus, Placement>.New(static defocus => defocus.Placement, static placement => defocus => defocus with { Placement = placement })),
        new("Sharp region", "Center and counterclockwise turn of the sharp band or ellipse"));
    public static readonly DefocusParameter Extent = new("extent", StateParameter.Bounded(Lens<Defocus, ShortSideLength>.New(static defocus => defocus.Extent, static extent => defocus => defocus with { Extent = extent }), Presentations.Length),
        new("Sharp width", "Sharp band half-width"));
    public static readonly DefocusParameter Aspect = new("aspect", StateParameter.Bounded(Lens<Defocus, Fraction>.New(static defocus => defocus.Aspect, static aspect => defocus => defocus with { Aspect = aspect })),
        new("Aspect", "Sharp ellipse from band at 0% to circle at 100%"));
    public static readonly DefocusParameter Feather = new("feather", StateParameter.Bounded(Lens<Defocus, ShortSideExtent>.New(static defocus => defocus.Feather, static feather => defocus => defocus with { Feather = feather }),
        Presentations.Extent with { Soft = (0.001f, 1f), Track = Some<Func<AxisRange, Axis>>(static range => new Axis.Linear(range)) }),
        new("Feather", "Distance past the sharp region to full blur"));
    public static readonly DefocusParameter Aperture = new("iris", new StateParameter<Defocus>.Record<Aperture>(Lens<Defocus, Aperture>.New(static defocus => defocus.Aperture, static aperture => defocus => defocus with { Aperture = aperture })),
        new("Aperture", "Lens opening shaping bokeh and diffraction spikes"));
    public static readonly DefocusParameter Fringe = new("fringe", StateParameter.Bounded(Lens<Defocus, SignedFraction>.New(static defocus => defocus.Fringe, static fringe => defocus => defocus with { Fringe = fringe })),
        new("Color shift", "Red and blue bokeh sizes apart from green"));

    public StateParameter<Defocus> Kind { get; }
    public ParameterText Text { get; }
}

public sealed record LensDistortion(Distortion Coefficient, Fraction Dispersion, LensFraming Framing, LensDirection Direction, WrapMode Wrap)
    : IStateRecord<LensDistortion, LensDistortionParameter>, IPixelStage<LensDistortion> {
    public static LensDistortion Default { get; } = new(Distortion.Create(0.05f), Fraction.Create(0.05f), LensFraming.Edges, LensDirection.Distort, WrapMode.Clamp);

    public static Option<PixelPass> Pass(LensDistortion state, PassContext context) =>
        state.Coefficient == Distortion.Neutral && state.Dispersion == Fraction.MinValue ? None
            : Some<PixelPass>(new PixelPass.Frame((frame, progress) =>
                ValidationResults.ToFin(RadialDistortion.Validate(state.Coefficient, state.Dispersion, state.Framing, state.Direction, out RadialDistortion? lens), lens)
                    .Bind(admitted => admitted.Pass(state.Wrap).Run(frame, progress))));
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class LensDistortionParameter : IStateParameter<LensDistortion> {
    public static readonly LensDistortionParameter Coefficient = new("distortion", StateParameter.Bounded(Lens<LensDistortion, Distortion>.New(static lens => lens.Coefficient, static coefficient => lens => lens with { Coefficient = coefficient })),
        new("Distortion", "Bends straight lines away from or toward frame center"));
    public static readonly LensDistortionParameter Dispersion = new("dispersion", StateParameter.Bounded(Lens<LensDistortion, Fraction>.New(static lens => lens.Dispersion, static dispersion => lens => lens with { Dispersion = dispersion })),
        new("Dispersion", "Spreads red and blue over continuous spectrum, more toward edges"));
    public static readonly LensDistortionParameter Framing = new("framing", new StateParameter<LensDistortion>.Choice<LensFraming>(Lens<LensDistortion, LensFraming>.New(static lens => lens.Framing, static framing => lens => lens with { Framing = framing })),
        new("Fit", "Scales warped frame to fill edges or corners"));
    public static readonly LensDistortionParameter Direction = new("direction", new StateParameter<LensDistortion>.Choice<LensDirection>(Lens<LensDistortion, LensDirection>.New(static lens => lens.Direction, static direction => lens => lens with { Direction = direction })),
        new("Direction", "Applies or removes lens warp"));
    public static readonly LensDistortionParameter Wrap = new("wrap", new StateParameter<LensDistortion>.Choice<WrapMode>(Lens<LensDistortion, WrapMode>.New(static lens => lens.Wrap, static wrap => lens => lens with { Wrap = wrap })),
        new("Wrap", "Source sampling past frame edges"));

    public StateParameter<LensDistortion> Kind { get; }
    public ParameterText Text { get; }
}

public sealed record AxialAberration(ShortSideLength Radius, Aperture Aperture) : IStateRecord<AxialAberration, AxialAberrationParameter>, IPixelStage<AxialAberration> {
    public static AxialAberration Default { get; } = new(ShortSideLength.Create(20f / ReferenceFrame.Height), Defocus.Default.Aperture);

    public static Option<PixelPass> Pass(AxialAberration state, PassContext context) =>
        state.Radius == ShortSideLength.Neutral ? None : Some<PixelPass>(new PixelPass.Frame((frame, progress) => state.Aperture.Pupil.Bind(pupil => new AxialDefocus(pupil, state.Radius).Pass.Run(frame, progress))));
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class AxialAberrationParameter : IStateParameter<AxialAberration> {
    public static readonly AxialAberrationParameter Radius = new("radius", StateParameter.Bounded(Lens<AxialAberration, ShortSideLength>.New(static aberration => aberration.Radius, static radius => aberration => aberration with { Radius = radius }),
        Presentations.Length with { Soft = (0f, 80f / ReferenceFrame.Height) }),
        new("Radius", "Blue defocus radius against green, red at half"));
    public static readonly AxialAberrationParameter Aperture = new("iris", new StateParameter<AxialAberration>.Record<Aperture>(Lens<AxialAberration, Aperture>.New(static aberration => aberration.Aperture, static aperture => aberration => aberration with { Aperture = aperture })),
        new("Aperture", "Lens opening shaping bokeh and diffraction spikes"));

    public StateParameter<AxialAberration> Kind { get; }
    public ParameterText Text { get; }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Vignette : IStateRecord<Vignette, VignetteParameter>, IPixelStage<Vignette> {
    public static Vignette Default { get; } = Natural.Default;

    public static Option<PixelPass> Pass(Vignette state, PassContext context) =>
        state.Switch(context,
            natural: static (run, _) => run.Camera.Filter(static camera => camera.Frustum is Frustum.Perspective)
                .Map<PixelPass>(camera => Pulled(Vector3.Zero, (column, line) => camera.Illumination((column + 0.5d, (double)run.Extent.Height - line - 0.5d)).IfNone(1f))),
            outline: static (run, outline) => Some<PixelPass>(outline.Falloff(run)));

    public sealed record Natural : Vignette, IStateRecord<Natural> {
        public static new Natural Default { get; } = new();
    }

    public sealed record Outline(Fraction Width, Fraction Height, Fraction Roundness, ShortSideLength Feather, SKColorF Pull) : Vignette, IStateRecord<Outline, VignetteOutlineParameter> {
        public static new Outline Default { get; } = new(Fraction.Create(0.8f), Fraction.Create(0.8f), Fraction.MaxValue, ShortSideLength.Create(0.2f * float.Sqrt(float.Tau) / 3f), new SKColorF(0f, 0f, 0f));

        public PixelPass.Pointwise Falloff(PassContext context) {
            (PixelExtent extent, EContext math) = (context.Extent, DistanceField.Precision(context.Extent));
            DistanceField field = DistanceField.Box((EFloat.FromDouble(Width * extent.Width / 2d), EFloat.FromDouble(Height * extent.Height / 2d)), Roundness, math);
            double band = ShapeEdge.Band(Feather, extent);
            return Pulled(RampLight.Light(Pull, context.Working).AsVector3(), (column, line) => ShapeEdge.Coverage(field.Sample((
                EFloat.FromDouble(column + 0.5d - (extent.Width / 2d)), EFloat.FromDouble((extent.Height / 2d) - line - 0.5d))).Distance.ToDouble(), band));
        }
    }

    private static PixelPass.Pointwise Pulled(Vector3 pull, Func<long, long, float> weight) =>
        new((row, column, line) => {
            for (int x = 0; x < row.Length; x++)
                row[x] = new(Vector3.Lerp(pull, row[x].AsVector3(), weight(column + x, line)), row[x].W);
        });
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class VignetteParameter : IStateParameter<Vignette> {
    public static readonly VignetteParameter Falloff = new("falloff", new StateParameter<Vignette>.Variant<Vignette>(Lens.identity<Vignette>()),
        new("Falloff", "Camera illumination or feathered outline"));

    public StateParameter<Vignette> Kind { get; }
    public ParameterText Text { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class VignetteOutlineParameter : IStateParameter<Vignette.Outline> {
    public static readonly VignetteOutlineParameter Width = new("width", StateParameter.Bounded(Lens<Vignette.Outline, Fraction>.New(static outline => outline.Width, static width => outline => outline with { Width = width })),
        new("Width", "Illuminated region width as share of frame width"));
    public static readonly VignetteOutlineParameter Height = new("height", StateParameter.Bounded(Lens<Vignette.Outline, Fraction>.New(static outline => outline.Height, static height => outline => outline with { Height = height })),
        new("Height", "Illuminated region height as share of frame height"));
    public static readonly VignetteOutlineParameter Roundness = new("roundness", StateParameter.Bounded(Lens<Vignette.Outline, Fraction>.New(static outline => outline.Roundness, static roundness => outline => outline with { Roundness = roundness })),
        new("Roundness", "Corner rounding from rectangle at 0% to rounded ends at 100%"));
    public static readonly VignetteOutlineParameter Feather = new("feather", StateParameter.Bounded(Lens<Vignette.Outline, ShortSideLength>.New(static outline => outline.Feather, static feather => outline => outline with { Feather = feather }), Presentations.Length),
        new("Feather", "Edge softness in frame short sides"));
    public static readonly VignetteOutlineParameter Pull = new("pull", new StateParameter<Vignette.Outline>.Color(Lens<Vignette.Outline, SKColorF>.New(static outline => outline.Pull, static pull => outline => outline with { Pull = pull })),
        new("Pull", "Color beyond the outline"));

    public StateParameter<Vignette.Outline> Kind { get; }
    public ParameterText Text { get; }
}
