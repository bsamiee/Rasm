using System.Drawing;
using System.Numerics;
using CommunityToolkit.HighPerformance;
using Darkroom.Chain;
using Emgu.CV;
using Emgu.CV.CvEnum;
using MathNet.Numerics.Integration;
using PeterO.Numbers;
using Rasm.Imaging.Color;
using Rasm.Imaging.Files;
using Rasm.Imaging.Filtering;
using Rasm.Imaging.Grade;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Procedural;
using Rasm.Imaging.Tone;
using Rasm.Imaging.Warping;
using Rasm.UI;
using Rasm.UI.Styling;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Dithering;
using SixLabors.ImageSharp.Processing.Processors.Quantization;
using SkiaSharp;
using Wacton.Unicolour;

namespace Darkroom.Catalog;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct SectorEccentricity : IPresented<SectorEccentricity, float> {
    public static SectorEccentricity MinValue { get; } = new(0f);
    public static SectorEccentricity MaxValue { get; } = new(2f);
    public static Presentation<float> Presentation { get; } = new();

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<int>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct LevelCount : IPresented<LevelCount, int> {
    public static LevelCount MinValue { get; } = new(2);
    public static LevelCount MaxValue { get; } = new(int.MaxValue);
    public static LevelCount Eighths { get; } = new(8 + 1);
    public static Presentation<int> Presentation { get; } = new() { Soft = (2, 256) };

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<int>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct ColorCount : IPresented<ColorCount, int> {
    public static ColorCount MinValue { get; } = new(QuantizerConstants.MinColors);
    public static ColorCount MaxValue { get; } = new(QuantizerConstants.MaxColors);
    public static Presentation<int> Presentation { get; } = new() { Track = Some(Presentations.Decades) };

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<int>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct LineCount : IPresented<LineCount, int> {
    public static LineCount MinValue { get; } = new(1);
    public static LineCount MaxValue { get; } = new(int.MaxValue);
    public static LineCount Ntsc { get; } = new(480);
    public static LineCount Coarse { get; } = new(120);
    public static Presentation<int> Presentation { get; } = new() { Soft = (120, 1080), Step = 1 };

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ComplexValueObject]
[ObjectFactory<Seq<SKColorF>>]
public sealed partial class Palette : IConvertible<Seq<SKColorF>> {
    public Seq<SKColorF> Colors { get; }

    public static Palette CubeCorners { get; } = Create(Seq(
        new SKColorF(0f, 0f, 0f), new SKColorF(1f, 0f, 0f), new SKColorF(0f, 1f, 0f), new SKColorF(0f, 0f, 1f),
        new SKColorF(1f, 1f, 0f), new SKColorF(1f, 0f, 1f), new SKColorF(0f, 1f, 1f), new SKColorF(1f, 1f, 1f)));

    public Seq<SKColorF> ToValue() => Colors;

    public static ValidationError? Validate(Seq<SKColorF> value, IFormatProvider? provider, out Palette? item) {
        ValidationError? error = null;
        ValidateFactoryArguments(ref error, ref value);
        item = error is null ? new Palette(value) : null;
        return error;
    }

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref Seq<SKColorF> colors) =>
        validationError = ColorCount.Validate(colors.Count, provider: null, out _);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PaletteDither : ICaptioned {
    public static readonly PaletteDither Bayer2 = new("bayer-2", KnownDitherings.Bayer2x2, "Bayer 2 × 2");
    public static readonly PaletteDither Ordered3 = new("ordered-3", KnownDitherings.Ordered3x3, "Ordered 3 × 3");
    public static readonly PaletteDither Bayer4 = new("bayer-4", KnownDitherings.Bayer4x4, "Bayer 4 × 4");
    public static readonly PaletteDither Bayer8 = new("bayer-8", KnownDitherings.Bayer8x8, "Bayer 8 × 8");
    public static readonly PaletteDither Bayer16 = new("bayer-16", KnownDitherings.Bayer16x16, "Bayer 16 × 16");
    public static readonly PaletteDither FloydSteinberg = new("floyd-steinberg", KnownDitherings.FloydSteinberg, "Floyd-Steinberg");
    public static readonly PaletteDither Atkinson = new("atkinson", KnownDitherings.Atkinson, "Atkinson");
    public static readonly PaletteDither Burkes = new("burkes", KnownDitherings.Burks, "Burkes");
    public static readonly PaletteDither JarvisJudiceNinke = new("jarvis-judice-ninke", KnownDitherings.JarvisJudiceNinke, "Jarvis-Judice-Ninke");
    public static readonly PaletteDither Stucki = new("stucki", KnownDitherings.Stucki, "Stucki");
    public static readonly PaletteDither Sierra2 = new("sierra-2", KnownDitherings.Sierra2, "Sierra 2");
    public static readonly PaletteDither Sierra3 = new("sierra-3", KnownDitherings.Sierra3, "Sierra 3");
    public static readonly PaletteDither SierraLite = new("sierra-lite", KnownDitherings.SierraLite, "Sierra Lite");
    public static readonly PaletteDither StevensonArce = new("stevenson-arce", KnownDitherings.StevensonArce, "Stevenson-Arce");

    public IDither Kernel { get; }
    public string Caption { get; }

    public QuantizerOptions Options(int colors, Fraction strength) =>
        new() { MaxColors = colors, Dither = strength == Fraction.MinValue ? null : Kernel, DitherScale = strength, ColorMatchingMode = ColorMatchingMode.Exact };
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class VideoStandard : ICaptioned {
    public static readonly VideoStandard Rec601 = new("rec601", "Rec.601", new Gamut(RgbConfiguration.Ntsc));
    public static readonly VideoStandard Rec709 = new("rec709", "Rec.709", Gamut.StandardRgb);

    public string Caption { get; }
    public Gamut Signal { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class EdgeRendering : ICaptioned {
    public static readonly EdgeRendering Magnitude = new("magnitude", "Response");
    public static readonly EdgeRendering Lines = new("lines", "Lines");
    public static readonly EdgeRendering LinesOnFill = new("lines-on-fill", "Lines on fill");

    public string Caption { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class LineClass {
    public static readonly LineClass Outer = new("outer", Lens<GeometryOutline, ContourStroke>.New(static outline => outline.Outer, static stroke => outline => outline with { Outer = stroke }), static outline => outline.Placement);
    public static readonly LineClass Inner = new("inner", Lens<GeometryOutline, ContourStroke>.New(static outline => outline.Inner, static stroke => outline => outline with { Inner = stroke }), static _ => StrokePlacement.Center);

    public Lens<GeometryOutline, ContourStroke> Stroke { get; }

    [UseDelegateFromConstructor]
    public partial StrokePlacement Placement(GeometryOutline state);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class OutlineSource {
    public static readonly OutlineSource Silhouette = new("silhouette",
        Lens<GeometryOutline, Option<LineClass>>.New(static outline => outline.Silhouette, static line => outline => outline with { Silhouette = line }), GuideChannel.Depth,
        static (state, context) => context.Camera.Map(camera => GuideEdges.Silhouette(camera.Frustum, state.Step)));
    public static readonly OutlineSource Crease = new("crease",
        Lens<GeometryOutline, Option<LineClass>>.New(static outline => outline.Crease, static line => outline => outline with { Crease = line }), GuideChannel.Normal,
        static (state, _) => Some(GuideEdges.Crease(state.CreaseAngle)));
    public static readonly OutlineSource Object = new("object",
        Lens<GeometryOutline, Option<LineClass>>.New(static outline => outline.Object, static line => outline => outline with { Object = line }), GuideChannel.ObjectId,
        static (_, _) => Some<ContourMarks>(GuideEdges.Identity));
    public static readonly OutlineSource Material = new("material",
        Lens<GeometryOutline, Option<LineClass>>.New(static outline => outline.Material, static line => outline => outline with { Material = line }), GuideChannel.MaterialId,
        static (_, _) => Some<ContourMarks>(GuideEdges.Identity));

    public Lens<GeometryOutline, Option<LineClass>> Route { get; }
    public GuideChannel Channel { get; }

    [UseDelegateFromConstructor]
    public partial Option<ContourMarks> Marks(GeometryOutline state, PassContext context);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class KuwaharaKind {
    public static readonly KuwaharaKind Classic = new("classic", static pixels => Math.Floor(pixels), static (_, _, radius, frame, progress) => Kuwahara.Quadrants(frame, radius, progress));
    public static readonly KuwaharaKind Anisotropic = new("anisotropic", static pixels => pixels, static (state, extent, radius, frame, progress) =>
        Kuwahara.Sectors(frame, radius, state.Uniformity.Pixels(extent), state.Sharpness, state.Eccentricity, progress));

    [UseDelegateFromConstructor]
    public partial double Radius(double pixels);

    [UseDelegateFromConstructor]
    public partial Fin<Unit> Run(Painterly state, PixelExtent extent, double radius, PixelFrame frame, IProgress<int> progress);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class Separation : ICaptioned {
    public static readonly Separation Mono = new("mono", "Single ink", static (rgb, weights) => new Vector4(Vector3.Zero, 1f - (float)double.Clamp(LuminanceSamples.Luminance(rgb.X, rgb.Y, rgb.Z, weights), 0d, 1d)));
    public static readonly Separation Process = new("process", "CMYK", static (rgb, _) => Vector3.Clamp(rgb, Vector3.Zero, Vector3.One) switch {
        var saturated => float.Max(saturated.X, float.Max(saturated.Y, saturated.Z)) switch {
            > 0f and var most => new Vector4(Vector3.One - (saturated / most), 1f - most),
            _ => Vector4.UnitW,
        },
    });

    public string Caption { get; }

    [UseDelegateFromConstructor]
    public partial Vector4 Levels(Vector3 rgb, (double R, double G, double B) weights);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class MosaicCell {
    public static readonly MosaicCell Square = new("square");
    public static readonly MosaicCell Cellular = new("cellular");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class GlyphColoring : ICaptioned {
    public static readonly GlyphColoring Input = new("input", "Image");
    public static readonly GlyphColoring Text = new("text", "Text color");
    public static readonly GlyphColoring PerCell = new("per-cell", "Random per cell");

    public string Caption { get; }
}

public sealed record EdgeDetect(
    EdgeOperator Operator, EdgeSource Source, ShortSideLength Softening, EdgeRendering Rendering, Fraction Threshold, ShortSideLength Width, SKColorF Line, SKColorF Fill, BlendingMode Mode)
    : IStateRecord<EdgeDetect, EdgeDetectParameter>, IPixelStage<EdgeDetect> {
    public static EdgeDetect Default { get; } = new(
        EdgeOperator.Kirsch, EdgeSource.Luma, ShortSideLength.Neutral, EdgeRendering.LinesOnFill, Fraction.Create(0.1f), ShortSideLength.Create(1f / ReferenceFrame.Height),
        new SKColorF(1f, 1f, 1f), new SKColorF(0f, 0f, 0f), BlendingMode.Mix);

    public static Option<PixelPass> Pass(EdgeDetect state, PassContext context) =>
        state.Rendering == EdgeRendering.Lines && state.Width == ShortSideLength.Neutral ? None
            : Some<PixelPass>(new PixelPass.Frame((frame, progress) => EdgeResponse.Detected(frame, state.Operator, state.Source, 3d * state.Softening.Pixels(context.Extent), context.Working.Luminance,
                edges => state.Rendering.Switch(
                    (State: state, Context: context, Frame: frame, Edges: edges, Progress: progress),
                    magnitude: static run => EdgeResponse.Magnitudes(run.Frame, run.Edges, run.Progress),
                    lines: static run => run.State.Stroked(run.Context, run.Frame, run.Edges, run.State.Line, 0f, run.Progress),
                    linesOnFill: static run => run.State.Stroked(run.Context, run.Frame, run.Edges, run.State.Fill, 1f, run.Progress)))));

    private Fin<Unit> Stroked(PassContext context, PixelFrame frame, Mat edges, SKColorF ground, float floor, IProgress<int> progress) =>
        EdgeResponse.Lines(frame, edges, (float)Threshold * Operator.Gain, Width.Pixels(context.Extent), new Vector3(Line.Red, Line.Green, Line.Blue), new Vector3(ground.Red, ground.Green, ground.Blue),
            floor, Mode, context.Working.Luminance, progress);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class EdgeDetectParameter : IStateParameter<EdgeDetect> {
    private static readonly Presentation<float> Span = Presentations.Length with { Soft = (0f, 0.01f) };

    public static readonly EdgeDetectParameter Operator = new("operator", new StateParameter<EdgeDetect>.Choice<EdgeOperator>(Lens<EdgeDetect, EdgeOperator>.New(static detect => detect.Operator, static kernel => detect => detect with { Operator = kernel })),
        new("Type", "Kernel measuring local change"));
    public static readonly EdgeDetectParameter Source = new("source", new StateParameter<EdgeDetect>.Choice<EdgeSource>(Lens<EdgeDetect, EdgeSource>.New(static detect => detect.Source, static source => detect => detect with { Source = source })),
        new("Channel", "Color or one channel the kernel reads"));
    public static readonly EdgeDetectParameter Softening = new("softening", StateParameter.Bounded(Lens<EdgeDetect, ShortSideLength>.New(static detect => detect.Softening, static softening => detect => detect with { Softening = softening }), Span),
        new("Softening", "Blur before kernel in frame short sides"));
    public static readonly EdgeDetectParameter Rendering = new("rendering", new StateParameter<EdgeDetect>.Choice<EdgeRendering>(Lens<EdgeDetect, EdgeRendering>.New(static detect => detect.Rendering, static rendering => detect => detect with { Rendering = rendering })),
        new("Output", "Response, lines, or lines on fill"));
    public static readonly EdgeDetectParameter Threshold = new("threshold", StateParameter.Bounded(Lens<EdgeDetect, Fraction>.New(static detect => detect.Threshold, static threshold => detect => detect with { Threshold = threshold })),
        new("Threshold", "Kernel step response share a line pixel exceeds"));
    public static readonly EdgeDetectParameter Width = new("width", StateParameter.Bounded(Lens<EdgeDetect, ShortSideLength>.New(static detect => detect.Width, static width => detect => detect with { Width = width }), Span),
        new("Line width", "Stroke width in frame short sides"));
    public static readonly EdgeDetectParameter Line = new("line-color", new StateParameter<EdgeDetect>.Color(Lens<EdgeDetect, SKColorF>.New(static detect => detect.Line, static line => detect => detect with { Line = line })),
        new("Line color", "Drawn line color"));
    public static readonly EdgeDetectParameter Fill = new("fill-color", new StateParameter<EdgeDetect>.Color(Lens<EdgeDetect, SKColorF>.New(static detect => detect.Fill, static fill => detect => detect with { Fill = fill })),
        new("Fill color", "Color under lines on fill"));
    public static readonly EdgeDetectParameter Mode = new("mode", new StateParameter<EdgeDetect>.Choice<BlendingMode>(Lens<EdgeDetect, BlendingMode>.New(static detect => detect.Mode, static mode => detect => detect with { Mode = mode })),
        new("Blend mode", "Drawn layer blend over frame"));

    public StateParameter<EdgeDetect> Kind { get; }
    public ParameterText Text { get; }
}

public sealed record ContourStroke(ShortSideLength Width, Gated<Length> World, SKColorF Color) : IStateRecord<ContourStroke, ContourStrokeParameter> {
    public static ContourStroke Default { get; } = new(ShortSideLength.Create(3f / ReferenceFrame.Height), new(Enabled: false, Length.FromInches(0.25d)), new SKColorF(0f, 0f, 0f));
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ContourStrokeParameter : IStateParameter<ContourStroke> {
    public static readonly ContourStrokeParameter Width = new("width", StateParameter.Bounded(Lens<ContourStroke, ShortSideLength>.New(static stroke => stroke.Width, static width => stroke => stroke with { Width = width }), Presentations.Length with { Soft = (0f, 0.01f) }),
        new("Width", "Line width in frame short sides, capping world width"));
    public static readonly ContourStrokeParameter World = new("world", new StateParameter<ContourStroke>.OptionalNumeric<Length, float>(
        Lens<ContourStroke, Gated<Length>>.New(static stroke => stroke.World, static world => stroke => stroke with { World = world }),
        NumericDomain.Meters((0f, float.MaxValue), new Presentation<float> { Soft = (0f, (float)Length.FromInches(1d).Meters), Origin = (float)Length.FromInches(0.25d).Meters })),
        new("World width", "Surface line width, narrowing with distance"));
    public static readonly ContourStrokeParameter Color = new("color", new StateParameter<ContourStroke>.Color(Lens<ContourStroke, SKColorF>.New(static stroke => stroke.Color, static color => stroke => stroke with { Color = color })),
        new("Color", "Line color"));

    public StateParameter<ContourStroke> Kind { get; }
    public ParameterText Text { get; }
}

public sealed record GeometryOutline(
    Option<LineClass> Silhouette, Option<LineClass> Crease, Option<LineClass> Object, Option<LineClass> Material, Fraction Step, SignedAngle CreaseAngle,
    ContourStroke Outer, StrokePlacement Placement, ContourStroke Inner, Gated<Length> TaperNear, ColorRamp TaperRamp, Gated<SKColorF> Paper, BlendingMode Mode)
    : IStateRecord<GeometryOutline, GeometryOutlineParameter>, IPixelStage<GeometryOutline> {
    public static GeometryOutline Default { get; } = new(
        Some(LineClass.Outer), Some(LineClass.Inner), Some(LineClass.Outer), None, Fraction.Create(0.05f), SignedAngle.Create(float.DegreesToRadians(180f - 134.4f)),
        ContourStroke.Default, StrokePlacement.Center, ContourStroke.Default with { Width = ShortSideLength.Create(1.5f / ReferenceFrame.Height) },
        new(Enabled: false, Length.FromFeet(10d)), Presets.Grayscale, new(Enabled: false, new SKColorF(0f, 0f, 0f)), BlendingMode.Mix);

    public static Seq<GuideChannel> Channels(GeometryOutline state) =>
        GuideChannel.Depth.Cons(toSeq(OutlineSource.Items).Filter(source => source.Route.Get(state).IsSome).Map(static source => source.Channel)).Distinct();

    public static Option<PixelPass> Pass(GeometryOutline state, PassContext context) =>
        from depth in context.Guides.Find(GuideChannel.Depth)
        let routes = toSeq(OutlineSource.Items).Map(source => source.Route.Get(state).Map(line => (Source: source, Line: line))).Somes()
        where !routes.IsEmpty || state.Paper.Enabled
        select (PixelPass)new PixelPass.Frame((frame, progress) =>
            from taper in RampLight.Of(state.TaperRamp, context.Working)
            from lines in toSeq(LineClass.Items).Traverse(line => line.Stroke.Get(state) switch {
                var stroke => Contours.Width(stroke.Width.Pixels(context.Extent), stroke.World.Active.Map(static world => (double)world.Meters), context.Camera, context.Extent,
                    state.TaperNear.Active.Map(near => ((double)near.Meters, (Func<float, float>)(at => taper.Sample(at) switch { var scale => float.Max(scale.X, float.Max(scale.Y, scale.Z)) })))).Map(width =>
                    new ContourLine(routes.Filter(route => route.Line == line).Map(route =>
                            from guide in context.Guides.Find(route.Source.Channel)
                            from marks in route.Source.Marks(state, context)
                            select (marks, guide)).Somes(),
                        line.Placement(state), width, new Vector3(stroke.Color.Red, stroke.Color.Green, stroke.Color.Blue))),
            }).As()
            from drawn in Contours.Outlined(frame, depth, lines, state.Paper.Active.Map(static paper => new Vector4(paper.Red, paper.Green, paper.Blue, paper.Alpha)), state.Mode, progress)
            select drawn);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class GeometryOutlineParameter : IStateParameter<GeometryOutline> {
    private static readonly NumericDomain<Length, float> Distance =
        NumericDomain.Meters((0f, float.MaxValue), new Presentation<float> { Soft = (0f, (float)Length.FromFeet(1000d).Meters), Origin = (float)Length.FromFeet(10d).Meters });

    public static readonly GeometryOutlineParameter Silhouette = new("silhouette-lines", new StateParameter<GeometryOutline>.OptionalChoice<LineClass>(OutlineSource.Silhouette.Route, "Off"),
        new("Silhouettes", "Lines at depth steps where one surface occludes another"));
    public static readonly GeometryOutlineParameter Crease = new("crease-lines", new StateParameter<GeometryOutline>.OptionalChoice<LineClass>(OutlineSource.Crease.Route, "Off"),
        new("Creases", "Lines where neighboring normals turn past the crease angle"));
    public static readonly GeometryOutlineParameter Object = new("object-lines", new StateParameter<GeometryOutline>.OptionalChoice<LineClass>(OutlineSource.Object.Route, "Off"),
        new("Object edges", "Lines where object IDs change"));
    public static readonly GeometryOutlineParameter Material = new("material-lines", new StateParameter<GeometryOutline>.OptionalChoice<LineClass>(OutlineSource.Material.Route, "Off"),
        new("Material edges", "Lines where material IDs change"));
    public static readonly GeometryOutlineParameter Step = new("depth-step", StateParameter.Bounded(Lens<GeometryOutline, Fraction>.New(static outline => outline.Step, static step => outline => outline with { Step = step }),
        Fraction.Presentation with { Soft = (0f, 0.2f) }),
        new("Depth threshold", "Silhouette depth curvature, 1 minus near over far depth across flat regions"));
    public static readonly GeometryOutlineParameter CreaseAngle = new("crease-angle", StateParameter.Bounded(Lens<GeometryOutline, SignedAngle>.New(static outline => outline.CreaseAngle, static angle => outline => outline with { CreaseAngle = angle }),
        Presentations.Angle with { Soft = (0f, float.Pi / 2f) }),
        new("Crease angle", "Turn between neighboring normals marking a crease"));
    public static readonly GeometryOutlineParameter Outer = new("outer", new StateParameter<GeometryOutline>.Record<ContourStroke>(LineClass.Outer.Stroke),
        new("Outer lines", "Width and color of outer lines"));
    public static readonly GeometryOutlineParameter Placement = new("placement", new StateParameter<GeometryOutline>.Choice<StrokePlacement>(Lens<GeometryOutline, StrokePlacement>.New(static outline => outline.Placement, static placement => outline => outline with { Placement = placement })),
        new("Placement", "Contour side outer lines draw on"));
    public static readonly GeometryOutlineParameter Inner = new("inner", new StateParameter<GeometryOutline>.Record<ContourStroke>(LineClass.Inner.Stroke),
        new("Inner lines", "Width and color of inner lines"));
    public static readonly GeometryOutlineParameter TaperNear = new("taper-near", new StateParameter<GeometryOutline>.OptionalNumeric<Length, float>(Lens<GeometryOutline, Gated<Length>>.New(static outline => outline.TaperNear, static depth => outline => outline with { TaperNear = depth }), Distance),
        new("Taper near", "Near depth divided by contour depth places width on ramp, off keeping full width"));
    public static readonly GeometryOutlineParameter TaperRamp = new("taper-ramp", new StateParameter<GeometryOutline>.Gradient(Lens<GeometryOutline, ColorRamp>.New(static outline => outline.TaperRamp, static ramp => outline => outline with { TaperRamp = ramp })),
        new("Taper ramp", "Line width scale from distant contours at left to near contours at right"));
    public static readonly GeometryOutlineParameter Paper = new("paper", new StateParameter<GeometryOutline>.OptionalColor(Lens<GeometryOutline, Gated<SKColorF>>.New(static outline => outline.Paper, static paper => outline => outline with { Paper = paper })),
        new("Paper", "Color under lines, off drawing over image"));
    public static readonly GeometryOutlineParameter Mode = new("mode", new StateParameter<GeometryOutline>.Choice<BlendingMode>(Lens<GeometryOutline, BlendingMode>.New(static outline => outline.Mode, static mode => outline => outline with { Mode = mode })),
        new("Blend mode", "Drawn layer blend over frame"));

    public StateParameter<GeometryOutline> Kind { get; }
    public ParameterText Text { get; }
}

public sealed record Painterly(KuwaharaKind Kind, ShortSideLength Size, ShortSideLength Uniformity, Fraction Sharpness, SectorEccentricity Eccentricity)
    : IStateRecord<Painterly, PainterlyParameter>, IPixelStage<Painterly> {
    public static Painterly Default { get; } = new(
        KuwaharaKind.Anisotropic, ShortSideLength.Create(6f / ReferenceFrame.Height), ShortSideLength.Create(4f / ReferenceFrame.Height), Fraction.MaxValue, SectorEccentricity.Create(1f));

    public static Option<PixelPass> Pass(Painterly state, PassContext context) =>
        state.Kind.Radius(state.Size.Pixels(context.Extent)) switch {
            0d => None,
            var radius => Some<PixelPass>(new PixelPass.Frame((frame, progress) => state.Kind.Run(state, context.Extent, radius, frame, progress))),
        };
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PainterlyParameter : IStateParameter<Painterly> {
    public static readonly PainterlyParameter Kuwahara = new("kind", new StateParameter<Painterly>.Choice<KuwaharaKind>(Lens<Painterly, KuwaharaKind>.New(static painterly => painterly.Kind, static kind => painterly => painterly with { Kind = kind })),
        new("Type", "Square quadrants or stroke-following sectors"));
    public static readonly PainterlyParameter Size = new("size", StateParameter.Bounded(Lens<Painterly, ShortSideLength>.New(static painterly => painterly.Size, static size => painterly => painterly with { Size = size }), Presentations.Length with { Soft = (0f, 0.02f) }),
        new("Size", "Filter radius in frame short sides"));
    public static readonly PainterlyParameter Uniformity = new("uniformity", StateParameter.Bounded(Lens<Painterly, ShortSideLength>.New(static painterly => painterly.Uniformity, static uniformity => painterly => painterly with { Uniformity = uniformity }), Presentations.Length with { Soft = (0f, 0.01f) }),
        new("Uniformity", "Stroke direction smoothing radius in frame short sides"));
    public static readonly PainterlyParameter Sharpness = new("sharpness", StateParameter.Bounded(Lens<Painterly, Fraction>.New(static painterly => painterly.Sharpness, static sharpness => painterly => painterly with { Sharpness = sharpness })),
        new("Sharpness", "Weight toward the least varied sector"));
    public static readonly PainterlyParameter Eccentricity = new("eccentricity", StateParameter.Bounded(Lens<Painterly, SectorEccentricity>.New(static painterly => painterly.Eccentricity, static eccentricity => painterly => painterly with { Eccentricity = eccentricity })),
        new("Eccentricity", "Sector elongation along strokes"));

    public StateParameter<Painterly> Kind { get; }
    public ParameterText Text { get; }
}

public sealed record Posterize(LevelCount Levels, Fraction RetainHue, Fraction DitherStrength, Gated<ShortSideExtent> DitherCell) : IStateRecord<Posterize, PosterizeParameter>, IPixelStage<Posterize> {
    public static Posterize Default { get; } = new(LevelCount.Eighths, Fraction.MinValue, Fraction.MinValue, new(Enabled: false, ShortSideExtent.MinValue));

    public static Option<PixelPass> Pass(Posterize state, PassContext context) =>
        Some(Reduction.Posterized(state.Levels, state.DitherStrength, state.RetainHue,
                state.DitherCell.Active.Map(size => ERational.FromSingle(size).Multiply(context.Extent.ShortSide)).IfNone(ERational.One), context.Extent.Height).Match<PixelPass>(
            Succ: static rows => new PixelPass.Pointwise(rows),
            Fail: static error => new PixelPass.Frame((_, _) => error)));
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PosterizeParameter : IStateParameter<Posterize> {
    public static readonly PosterizeParameter Levels = new("levels", StateParameter.Bounded(Lens<Posterize, LevelCount>.New(static posterize => posterize.Levels, static levels => posterize => posterize with { Levels = levels })),
        new("Levels", "Even output levels per channel from black to white"));
    public static readonly PosterizeParameter RetainHue = new("retain-hue", StateParameter.Bounded(Lens<Posterize, Fraction>.New(static posterize => posterize.RetainHue, static retain => posterize => posterize with { RetainHue = retain })),
        new("Retain hue", "Share of input hue kept at rounded saturation and luma"));
    public static readonly PosterizeParameter DitherStrength = new("dither-strength", StateParameter.Bounded(Lens<Posterize, Fraction>.New(static posterize => posterize.DitherStrength, static strength => posterize => posterize with { DitherStrength = strength })),
        new("Dither amount", "Dither offset, 0% rounding to nearest level or color"));
    public static readonly PosterizeParameter DitherCell = new("dither-cell", StateParameter.OptionalBounded(Lens<Posterize, Gated<ShortSideExtent>>.New(static posterize => posterize.DitherCell, static cell => posterize => posterize with { DitherCell = cell }),
        Presentations.Extent with { Soft = (0.001f, 0.25f) }),
        new("Dither cell", "Ordered dither cell in frame short sides, off drawing one cell per pixel"));

    public StateParameter<Posterize> Kind { get; }
    public ParameterText Text { get; }
}

public sealed record PaletteReduction(ColorCount Colors, PaletteDither Dithering, Fraction DitherStrength) : IStateRecord<PaletteReduction, PaletteReductionParameter>, IPixelStage<PaletteReduction> {
    public static PaletteReduction Default { get; } = new(ColorCount.MaxValue, PaletteDither.FloydSteinberg, Fraction.MaxValue);

    public static Option<PixelPass> Pass(PaletteReduction state, PassContext context) =>
        Some<PixelPass>(new PixelPass.Frame((frame, progress) => Reduction.Quantized(frame, new WuQuantizer(state.Dithering.Options(state.Colors, state.DitherStrength)), progress)));
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PaletteReductionParameter : IStateParameter<PaletteReduction> {
    public static readonly PaletteReductionParameter Colors = new("colors", StateParameter.Bounded(Lens<PaletteReduction, ColorCount>.New(static reduction => reduction.Colors, static colors => reduction => reduction with { Colors = colors })),
        new("Colors", "Palette size the frame's colors split into by variance"));
    public static readonly PaletteReductionParameter Dithering = new("dithering", new StateParameter<PaletteReduction>.Choice<PaletteDither>(Lens<PaletteReduction, PaletteDither>.New(static reduction => reduction.Dithering, static dithering => reduction => reduction with { Dithering = dithering })),
        new("Dither", "Ordered matrix or error diffusion kernel spreading rounding error"));
    public static readonly PaletteReductionParameter DitherStrength = new("dither-strength", StateParameter.Bounded(Lens<PaletteReduction, Fraction>.New(static reduction => reduction.DitherStrength, static strength => reduction => reduction with { DitherStrength = strength })),
        new("Dither amount", "Dither offset, 0% rounding to nearest level or color"));

    public StateParameter<PaletteReduction> Kind { get; }
    public ParameterText Text { get; }
}

public sealed record PaletteMapping(Palette Palette, PaletteDither Dithering, Fraction DitherStrength) : IStateRecord<PaletteMapping, PaletteMappingParameter>, IPixelStage<PaletteMapping> {
    public static PaletteMapping Default { get; } = new(Palette.CubeCorners, PaletteDither.FloydSteinberg, Fraction.MaxValue);

    public static Option<PixelPass> Pass(PaletteMapping state, PassContext context) =>
        Some<PixelPass>(new PixelPass.Frame((frame, progress) => Reduction.Quantized(frame, new PaletteQuantizer(
            state.Palette.Colors.Map(static color => SixLabors.ImageSharp.Color.FromScaledVector(new Vector4(color.Red, color.Green, color.Blue, color.Alpha))).ToArray(),
            state.Dithering.Options(ColorCount.MaxValue, state.DitherStrength)), progress)));
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PaletteMappingParameter : IStateParameter<PaletteMapping> {
    public static readonly PaletteMappingParameter Palette = new("palette", new StateParameter<PaletteMapping>.Colors<Palette>(Lens<PaletteMapping, Palette>.New(static mapping => mapping.Palette, static palette => mapping => mapping with { Palette = palette })),
        new("Palette", "Colors each pixel maps to by nearest match"));
    public static readonly PaletteMappingParameter Dithering = new("dithering", new StateParameter<PaletteMapping>.Choice<PaletteDither>(Lens<PaletteMapping, PaletteDither>.New(static mapping => mapping.Dithering, static dithering => mapping => mapping with { Dithering = dithering })),
        new("Dither", "Ordered matrix or error diffusion kernel spreading rounding error"));
    public static readonly PaletteMappingParameter DitherStrength = new("dither-strength", StateParameter.Bounded(Lens<PaletteMapping, Fraction>.New(static mapping => mapping.DitherStrength, static strength => mapping => mapping with { DitherStrength = strength })),
        new("Dither amount", "Dither offset, 0% rounding to nearest level or color"));

    public StateParameter<PaletteMapping> Kind { get; }
    public ParameterText Text { get; }
}

public sealed record InkScreen(SignedAngle Angle, ProperFraction U, ProperFraction V) : IStateRecord<InkScreen, InkScreenParameter> {
    private const float ChannelScale = 4f;

    public static InkScreen Default { get; } = new(SignedAngle.Diagonal, ProperFraction.Zero, ProperFraction.Zero);
    public static InkScreen Cyan { get; } = Default with { U = ProperFraction.Create(0.193f * ChannelScale), V = ProperFraction.Create(0.052f * ChannelScale) };
    public static InkScreen Magenta { get; } = Default with { U = ProperFraction.Create(0.052f * ChannelScale), V = ProperFraction.Create(0.193f * ChannelScale) };
    public static InkScreen Yellow { get; } = Default with { U = ProperFraction.Zero, V = ProperFraction.Create(0.2f * ChannelScale) };
    public static InkScreen Key { get; } = Default with { U = ProperFraction.Create(0.1414f * ChannelScale), V = ProperFraction.Create(0.1414f * ChannelScale) };

    public HalftoneScreen Screen(ERational pitch, ScreenShape shape, float turn, float onset) =>
        new(Matrix3x2.CreateRotation(Angle + turn) * Matrix3x2.CreateTranslation(U, V), pitch, shape, onset);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class InkScreenParameter : IStateParameter<InkScreen> {
    public static readonly InkScreenParameter Angle = new("angle", StateParameter.Bounded(Lens<InkScreen, SignedAngle>.New(static ink => ink.Angle, static angle => ink => ink with { Angle = angle }), Presentations.Angle),
        new("Angle", "Counterclockwise turn of ink screen"));
    public static readonly InkScreenParameter U = new("u", StateParameter.Bounded(Lens<InkScreen, ProperFraction>.New(static ink => ink.U, static u => ink => ink with { U = u })),
        new("Phase U", "Ink screen shift along its first axis in cells"));
    public static readonly InkScreenParameter V = new("v", StateParameter.Bounded(Lens<InkScreen, ProperFraction>.New(static ink => ink.V, static v => ink => ink with { V = v })),
        new("Phase V", "Ink screen shift along its second axis in cells"));

    public StateParameter<InkScreen> Kind { get; }
    public ParameterText Text { get; }
}

public sealed record Halftone(
    ScreenShape Shape, ShortSideExtent Size, Separation Separation, InkScreen Cyan, InkScreen Magenta, InkScreen Yellow, InkScreen Key, Fraction CrossOnset, SKColorF Ink, Gated<SKColorF> Paper, BlendingMode Mode)
    : IStateRecord<Halftone, HalftoneParameter>, IPixelStage<Halftone> {
    public static Halftone Default { get; } = new(
        ScreenShape.Round, ShortSideExtent.Create(1f / (2f * 2f * 4f) * ReferenceFrame.GreaterSide), Separation.Mono, InkScreen.Cyan, InkScreen.Magenta, InkScreen.Yellow, InkScreen.Key,
        Fraction.MaxValue, new SKColorF(0f, 0f, 0f), new(Enabled: true, new SKColorF(1f, 1f, 1f)), BlendingMode.Mix);

    public static Option<PixelPass> Pass(Halftone state, PassContext context) => Blend.Mixed(new PixelPass.Pointwise(Print(state, context)), state.Mode, 1f, []);

    private static Action<Span<Vector4>, long, long> Print(Halftone state, PassContext context) {
        (ERational pitch, PixelExtent extent, (double R, double G, double B) weights) = (ERational.FromSingle(state.Size).Multiply(context.Extent.ShortSide), context.Extent, context.Working.Luminance);
        Arr<(Arr<HalftoneScreen> Screens, Vector3 Absorbance, Vector4 Channel)> plates = [
            Plate(state.Cyan, Vector3.UnitX, Vector4.UnitX), Plate(state.Magenta, Vector3.UnitY, Vector4.UnitY), Plate(state.Yellow, Vector3.UnitZ, Vector4.UnitZ),
            Plate(state.Key, Vector3.One - new Vector3(state.Ink.Red, state.Ink.Green, state.Ink.Blue), Vector4.UnitW)];
        Option<Vector3> paper = state.Paper.Active.Map(static color => new Vector3(color.Red, color.Green, color.Blue));
        return (row, column, line) => {
            for (int x = 0; x < row.Length; x++) {
                ((double X, double Y) point, Vector4 levels) = ((column + x + 0.5d - (extent.Width / 2d), extent.Height - line - 0.5d - (extent.Height / 2d)), state.Separation.Levels(row[x].AsVector3(), weights));
                Vector3 transmitted = Vector3.One;
                foreach ((Arr<HalftoneScreen> screens, Vector3 absorbance, Vector4 channel) in plates.AsSpan()) {
                    float level = Vector4.Dot(levels, channel);
                    float clear = level > 0f ? screens.Fold(1f, (held, screen) => held * (1f - screen.Coverage(point, level))) : 1f;
                    transmitted *= Vector3.One - ((1f - clear) * absorbance);
                }
                row[x] = new Vector4(paper.IfNone(row[x].AsVector3()) * transmitted, row[x].W);
            }
        };

        (Arr<HalftoneScreen> Screens, Vector3 Absorbance, Vector4 Channel) Plate(InkScreen ink, Vector3 absorbance, Vector4 channel) =>
            (state.CrossOnset < Fraction.MaxValue ? [ink.Screen(pitch, state.Shape, 0f, 0f), ink.Screen(pitch, state.Shape, float.Pi / 2f, state.CrossOnset)] : [ink.Screen(pitch, state.Shape, 0f, 0f)],
                absorbance, channel);
    }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class HalftoneParameter : IStateParameter<Halftone> {
    public static readonly HalftoneParameter Shape = new("shape", new StateParameter<Halftone>.Choice<ScreenShape>(Lens<Halftone, ScreenShape>.New(static halftone => halftone.Shape, static shape => halftone => halftone with { Shape = shape })),
        new("Shape", "Screen cell spot, inked area matching cell tone"));
    public static readonly HalftoneParameter Size = new("size", StateParameter.Bounded(Lens<Halftone, ShortSideExtent>.New(static halftone => halftone.Size, static size => halftone => halftone with { Size = size }),
        Presentations.Extent with { Soft = (1f / (2f * 64f) * ReferenceFrame.GreaterSide, 0.25f) }),
        new("Cell size", "Screen cell height in frame short sides"));
    public static readonly HalftoneParameter Separation = new("separation", new StateParameter<Halftone>.Choice<Separation>(Lens<Halftone, Separation>.New(static halftone => halftone.Separation, static separation => halftone => halftone with { Separation = separation })),
        new("Separation", "One ink screened on luma, or cyan, magenta, yellow, and key screens"));
    public static readonly HalftoneParameter Cyan = new("cyan", new StateParameter<Halftone>.Record<InkScreen>(Lens<Halftone, InkScreen>.New(static halftone => halftone.Cyan, static ink => halftone => halftone with { Cyan = ink })),
        new("Cyan", "Ink screen angle and phase"));
    public static readonly HalftoneParameter Magenta = new("magenta", new StateParameter<Halftone>.Record<InkScreen>(Lens<Halftone, InkScreen>.New(static halftone => halftone.Magenta, static ink => halftone => halftone with { Magenta = ink })),
        new("Magenta", "Ink screen angle and phase"));
    public static readonly HalftoneParameter Yellow = new("yellow", new StateParameter<Halftone>.Record<InkScreen>(Lens<Halftone, InkScreen>.New(static halftone => halftone.Yellow, static ink => halftone => halftone with { Yellow = ink })),
        new("Yellow", "Ink screen angle and phase"));
    public static readonly HalftoneParameter KeyScreen = new("key", new StateParameter<Halftone>.Record<InkScreen>(Lens<Halftone, InkScreen>.New(static halftone => halftone.Key, static ink => halftone => halftone with { Key = ink })),
        new("Key", "Ink screen angle and phase"));
    public static readonly HalftoneParameter CrossOnset = new("cross-onset", StateParameter.Bounded(Lens<Halftone, Fraction>.New(static halftone => halftone.CrossOnset, static onset => halftone => halftone with { CrossOnset = onset })),
        new("Crosshatch", "Ink level where a perpendicular screen joins, 100% drawing none"));
    public static readonly HalftoneParameter Ink = new("ink", new StateParameter<Halftone>.Color(Lens<Halftone, SKColorF>.New(static halftone => halftone.Ink, static ink => halftone => halftone with { Ink = ink })),
        new("Ink", "Color the key screen prints"));
    public static readonly HalftoneParameter Paper = new("paper", new StateParameter<Halftone>.OptionalColor(Lens<Halftone, Gated<SKColorF>>.New(static halftone => halftone.Paper, static paper => halftone => halftone with { Paper = paper })),
        new("Paper", "Color under inks, off printing over image"));
    public static readonly HalftoneParameter Mode = new("mode", new StateParameter<Halftone>.Choice<BlendingMode>(Lens<Halftone, BlendingMode>.New(static halftone => halftone.Mode, static mode => halftone => halftone with { Mode = mode })),
        new("Blend mode", "Drawn layer blend over frame"));

    public StateParameter<Halftone> Kind { get; }
    public ParameterText Text { get; }
}

public sealed record Mosaic(MosaicCell Cell, ShortSideExtent Size, Cellular Cells, Seed Seed, Timing Timing, Hold Hold) : IStateRecord<Mosaic, MosaicParameter>, IPixelStage<Mosaic> {
    public static Mosaic Default { get; } = new(MosaicCell.Square, ShortSideExtent.Create(0.1f * 0.05f * ReferenceFrame.GreaterSide), Cellular.Default, Seed.MinValue, Timing.Default with { Pace = Magnitude.Zero }, Hold.MinValue);

    public static Option<PixelPass> Pass(Mosaic state, PassContext context) =>
        Some<PixelPass>(new PixelPass.Frame((frame, progress) => state.Cell.Switch((State: state, Context: context, Frame: frame, Progress: progress),
            square: static run => Blocks(run.State, run.Context, run.Frame, run.Progress),
            cellular: static run => Sites(run.State, run.Context, run.Frame, run.Progress))));

    private static Fin<Unit> Blocks(Mosaic state, PassContext context, PixelFrame frame, IProgress<int> progress) => frame.Filter(header => {
        ERational cell = ERational.FromSingle(state.Size).Multiply(context.Extent.ShortSide);
        CellGrid grid = CellGrid.Fit(context.Extent, cell, cell);
        using Mat source = header.Clone();
        Func<(EInteger Column, EInteger Row), Vector3> colors = memoUnsafe(grid.Means(frame, source));
        return new PixelPass.Pointwise((row, column, line) => {
            EInteger down = grid.Row(context.Extent.Height - 1L - line, context.Extent.Height).Cell;
            for (int x = 0; x < row.Length; x++)
                row[x] = new Vector4(colors((grid.Column(column + x, context.Extent.Width).Cell, down)), row[x].W);
        }).Run(frame, progress);
    });

    private static Fin<Unit> Sites(Mosaic state, PassContext context, PixelFrame frame, IProgress<int> progress) {
        PixelFrame copy = frame.Copy();
        (ERational centerX, ERational centerY, ERational pitch) =
            (ERational.FromInt32(context.Extent.Width).Divide(2), ERational.FromInt32(context.Extent.Height).Divide(2), ERational.FromSingle(state.Size).Multiply(context.Extent.ShortSide));
        (uint field, VoronoiCells cells) = (state.Seed.Field(4u, state.Hold.Period(state.Timing.At(context))), state.Cells.Cells);
        return new PixelPass.Pointwise((row, column, line) => {
            ReadOnlySpan2D<Vector4> source = copy.View.Span;
            for (int x = 0; x < row.Length; x++) {
                (ERational X, ERational Y) = NoiseDimensions.PlanarF1(
                    ERational.FromInt64((2L * (column + x)) + 1L).Divide(2).Subtract(centerX).Divide(pitch),
                    ERational.FromInt64((2L * (context.Extent.Height - line)) - 1L).Divide(2).Subtract(centerY).Divide(pitch), cells, field).Site;
                Vector4 color = source.Sample(
                    (ERational.FromEInteger(LatticePoint.Split(X.Multiply(pitch).Add(centerX)).Cell).ToDouble() + 0.5d, ERational.FromEInteger(LatticePoint.Split(Y.Multiply(pitch).Add(centerY)).Cell).ToDouble() + 0.5d),
                    (WrapMode.Clamp, WrapMode.Clamp), Vector4.Zero, static (sum, pixel, weight) => sum + (pixel * (float)weight), Some((frame.Origin, frame.Extent)));
                row[x] = new Vector4(color.AsVector3(), row[x].W);
            }
        }).Run(frame, progress);
    }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class MosaicParameter : IStateParameter<Mosaic> {
    public static readonly MosaicParameter Cell = new("cell", new StateParameter<Mosaic>.Choice<MosaicCell>(Lens<Mosaic, MosaicCell>.New(static mosaic => mosaic.Cell, static cell => mosaic => mosaic with { Cell = cell })),
        new("Cells", "Square blocks of mean color, or irregular cells of site color"));
    public static readonly MosaicParameter Size = new("size", StateParameter.Bounded(Lens<Mosaic, ShortSideExtent>.New(static mosaic => mosaic.Size, static size => mosaic => mosaic with { Size = size }),
        Presentations.Extent with { Soft = (0.001f, 0.1f * ReferenceFrame.GreaterSide) }),
        new("Cell size", "Cell height in frame short sides"));
    public static readonly MosaicParameter Cells = new("cells", new StateParameter<Mosaic>.Record<Cellular>(Lens<Mosaic, Cellular>.New(static mosaic => mosaic.Cells, static cells => mosaic => mosaic with { Cells = cells })),
        new("Cells", "Cell distance and site placement"));
    public static readonly MosaicParameter Seed = new("seed", StateParameter.Bounded(Lens<Mosaic, Seed>.New(static mosaic => mosaic.Seed, static seed => mosaic => mosaic with { Seed = seed })),
        new("Seed", "Number selecting pattern"));
    public static readonly MosaicParameter Timing = new("timing", new StateParameter<Mosaic>.Record<Timing>(Lens<Mosaic, Timing>.New(static mosaic => mosaic.Timing, static timing => mosaic => mosaic with { Timing = timing })),
        new("Timing", "Start time and pace over recorded frames"));
    public static readonly MosaicParameter Hold = new("hold", StateParameter.Bounded(Lens<Mosaic, Hold>.New(static mosaic => mosaic.Hold, static hold => mosaic => mosaic with { Hold = hold })),
        new("Hold", "Seconds between pattern redraws"));

    public StateParameter<Mosaic> Kind { get; }
    public ParameterText Text { get; }
}

public sealed record GlyphCells(
    CharacterSet Characters, HersheyFonts Font, ShortSideExtent Size, GlyphColoring Coloring, SKColorF Text, SKColorF Background, Fraction Saturation, Fraction Jitter,
    Seed Seed, Timing Timing, Hold Hold, BlendingMode Mode)
    : IStateRecord<GlyphCells, GlyphCellsParameter>, IPixelStage<GlyphCells> {
    public static GlyphCells Default { get; } = new(
        CharacterSet.Create(" .:-=+*#%@"), HersheyFonts.Simplex, ShortSideExtent.Create(1f / 24f), GlyphColoring.Input, new SKColorF(1f, 1f, 1f), new SKColorF(0f, 0f, 0f), Fraction.MaxValue,
        Fraction.Create(0.1f), Seed.MinValue, Timing.Default, Hold.Create(6f / 24f), BlendingMode.Mix);

    public static GlyphCells Binary { get; } = Default with { Characters = CharacterSet.Create(" 01") };
    public static GlyphCells Numbers { get; } = Default with { Characters = CharacterSet.Create(" 0123456789") };
    public static GlyphCells Arithmetic { get; } = Default with { Characters = CharacterSet.Create(" +-*/=<>^%") };
    public static GlyphCells Latin { get; } = Default with { Characters = CharacterSet.Create(" ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz") };
    public static GlyphCells Mixed { get; } = Default with { Characters = CharacterSet.Create(" .:-=+*#%@01xX") };

    public static Seq<(string Name, EffectLook<GlyphCells> Look)> Looks { get; } = [
        Presets.Look(nameof(Binary), Binary), Presets.Look(nameof(Numbers), Numbers), Presets.Look(nameof(Arithmetic), Arithmetic),
        Presets.Look(nameof(Latin), Latin), Presets.Look(nameof(Mixed), Mixed)];

    public static Option<PixelPass> Pass(GlyphCells state, PassContext context) => Blend.Mixed(new PixelPass.Frame((frame, progress) => Kernel(state, context, frame, progress)), state.Mode, 1f, []);

    private static Fin<Unit> Kernel(GlyphCells state, PassContext context, PixelFrame frame, IProgress<int> progress) => frame.Filter(header => {
        using GlyphAtlas atlas = new(state.Characters, state.Font, ERational.FromSingle(state.Size).Multiply(context.Extent.ShortSide), context.Extent);
        using Mat source = header.Clone();
        Func<(EInteger Column, EInteger Row), Vector3> mean = atlas.Grid.Means(frame, source);
        uint field = state.Seed.Field(5u, state.Hold.Period(state.Timing.At(context)));
        (uint gray, uint tint, (double R, double G, double B) weights) = (CoordinateHash.Branch(field, 0u), CoordinateHash.Branch(field, 1u), context.Working.Luminance);
        Func<(EInteger Column, EInteger Row), (int Glyph, Vector3 Color)> cells = memoUnsafe<(EInteger Column, EInteger Row), (int Glyph, Vector3 Color)>(cell => {
            Vector3 color = mean(cell);
            (Vector4 u, Vector4 c) = (CoordinateHash.Uniforms(cell.Column, cell.Row, gray), CoordinateHash.Uniforms(cell.Column, cell.Row, tint));
            float level = (float)double.Clamp(double.Clamp(LuminanceSamples.Luminance(color.X, color.Y, color.Z, weights), 0d, 1d) + ((float)state.Jitter * (u.X - 0.5d)), 0d, 1d);
            return (atlas.Glyph(level), state.Coloring.Switch(
                (Mean: color, Gray: u.Y, Tint: c.AsVector3(), State: state),
                input: static s => s.Mean,
                text: static s => new Vector3(s.State.Text.Red, s.State.Text.Green, s.State.Text.Blue),
                perCell: static s => Vector3.Lerp(new Vector3(s.Gray), s.Tint, s.State.Saturation)));
        });
        (int width, int height, Size tile, Vector3 background) = (context.Extent.Width, context.Extent.Height, atlas.Tile, new Vector3(state.Background.Red, state.Background.Green, state.Background.Blue));
        return new PixelPass.Pointwise((row, column, line) => {
            (EInteger down, int across) = atlas.Grid.Row(height - 1L - line, height);
            int maskRow = (int)((long)across * tile.Height / height);
            for (int x = 0; x < row.Length; x++) {
                (EInteger right, int inside) = atlas.Grid.Column(column + x, width);
                (int glyph, Vector3 color) = cells((right, down));
                row[x] = new Vector4(Vector3.Lerp(background, color, atlas.Coverage(glyph, maskRow, (int)((long)inside * tile.Width / width))), row[x].W);
            }
        }).Run(frame, progress);
    });
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class GlyphCellsParameter : IStateParameter<GlyphCells> {
    public static readonly GlyphCellsParameter Characters = new("glyphs", new StateParameter<GlyphCells>.Keyed<CharacterSet, string>(Lens<GlyphCells, CharacterSet>.New(static cells => cells.Characters, static characters => cells => cells with { Characters = characters })),
        new("Characters", "Cell characters ranked by ink coverage"));
    public static readonly GlyphCellsParameter Font = new("font", new StateParameter<GlyphCells>.Enumerated<HersheyFonts>(Lens<GlyphCells, HersheyFonts>.New(static cells => cells.Font, static font => cells => cells with { Font = font })),
        new("Font", "Character stroke font"));
    public static readonly GlyphCellsParameter Size = new("size", StateParameter.Bounded(Lens<GlyphCells, ShortSideExtent>.New(static cells => cells.Size, static size => cells => cells with { Size = size }), Presentations.Extent with { Soft = (1f / 128f, 0.25f) }),
        new("Cell size", "Character cell height in frame short sides"));
    public static readonly GlyphCellsParameter Coloring = new("coloring", new StateParameter<GlyphCells>.Choice<GlyphColoring>(Lens<GlyphCells, GlyphColoring>.New(static cells => cells.Coloring, static coloring => cells => cells with { Coloring = coloring })),
        new("Coloring", "Color each character draws in"));
    public static readonly GlyphCellsParameter TextColor = new("text", new StateParameter<GlyphCells>.Color(Lens<GlyphCells, SKColorF>.New(static cells => cells.Text, static text => cells => cells with { Text = text })),
        new("Text color", "Character color when coloring is Text color"));
    public static readonly GlyphCellsParameter Background = new("background", new StateParameter<GlyphCells>.Color(Lens<GlyphCells, SKColorF>.New(static cells => cells.Background, static background => cells => cells with { Background = background })),
        new("Background", "Color behind every character"));
    public static readonly GlyphCellsParameter Saturation = new("saturation", StateParameter.Bounded(Lens<GlyphCells, Fraction>.New(static cells => cells.Saturation, static saturation => cells => cells with { Saturation = saturation })),
        new("Saturation", "Saturation of random cell colors"));
    public static readonly GlyphCellsParameter Jitter = new("jitter", StateParameter.Bounded(Lens<GlyphCells, Fraction>.New(static cells => cells.Jitter, static jitter => cells => cells with { Jitter = jitter })),
        new("Jitter", "Random offset range on cell tones before character choice"));
    public static readonly GlyphCellsParameter Seed = new("seed", StateParameter.Bounded(Lens<GlyphCells, Seed>.New(static cells => cells.Seed, static seed => cells => cells with { Seed = seed })),
        new("Seed", "Number selecting pattern"));
    public static readonly GlyphCellsParameter Timing = new("timing", new StateParameter<GlyphCells>.Record<Timing>(Lens<GlyphCells, Timing>.New(static cells => cells.Timing, static timing => cells => cells with { Timing = timing })),
        new("Timing", "Start time and pace over recorded frames"));
    public static readonly GlyphCellsParameter Hold = new("hold", StateParameter.Bounded(Lens<GlyphCells, Hold>.New(static cells => cells.Hold, static hold => cells => cells with { Hold = hold }),
        Presentations.Seconds with { Track = Some(Presentations.Decades), Soft = (0.001f, 120f / 24f) }),
        new("Hold", "Seconds between pattern redraws"));
    public static readonly GlyphCellsParameter Mode = new("mode", new StateParameter<GlyphCells>.Choice<BlendingMode>(Lens<GlyphCells, BlendingMode>.New(static cells => cells.Mode, static mode => cells => cells with { Mode = mode })),
        new("Blend mode", "Drawn layer blend over frame"));

    public StateParameter<GlyphCells> Kind { get; }
    public ParameterText Text { get; }
}

public sealed record PixelSort(SortKey Key, Fraction Low, Fraction High, SortDirection Direction) : IStateRecord<PixelSort, PixelSortParameter>, IPixelStage<PixelSort> {
    public static PixelSort Default { get; } = new(SortKey.Luma, Fraction.MinValue, Fraction.Create(0.3f), SortDirection.Right);

    public static Option<PixelPass> Pass(PixelSort state, PassContext context) => Some(state.Direction.Sorted(state.Key, state.Low, state.High, context.Working.Luminance));
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PixelSortParameter : IStateParameter<PixelSort> {
    public static readonly PixelSortParameter SortKey = new("key", new StateParameter<PixelSort>.Choice<SortKey>(Lens<PixelSort, SortKey>.New(static sort => sort.Key, static key => sort => sort with { Key = key })),
        new("Sort by", "Value each span sorts on"));
    public static readonly PixelSortParameter Low = new("low", StateParameter.Bounded(Lens<PixelSort, Fraction>.New(static sort => sort.Low, static low => sort => sort with { Low = low })),
        new("Lower bound", "Key value where sorted spans start, above upper bound wrapping the band"));
    public static readonly PixelSortParameter High = new("high", StateParameter.Bounded(Lens<PixelSort, Fraction>.New(static sort => sort.High, static high => sort => sort with { High = high })),
        new("Upper bound", "Key value where sorted spans end"));
    public static readonly PixelSortParameter Direction = new("direction", new StateParameter<PixelSort>.Choice<SortDirection>(Lens<PixelSort, SortDirection>.New(static sort => sort.Direction, static direction => sort => sort with { Direction = direction })),
        new("Direction", "Direction sorted values rise along"));

    public StateParameter<PixelSort> Kind { get; }
    public ParameterText Text { get; }
}

public sealed record CrtDisplay(
    PhosphorMask Mask, ShortSideExtent Pitch, Fraction MaskStrength, LineCount Lines, bool Interlaced, Fraction ScanlineDepth, Fraction Collapse, ShortSideLength Glow,
    Distortion Curvature, LensFraming Framing, Timing Timing)
    : IStateRecord<CrtDisplay, CrtDisplayParameter>, IPixelStage<CrtDisplay> {
    public static CrtDisplay Default { get; } = new(
        PhosphorMask.SlotMask, ShortSideExtent.Create(8f / ReferenceFrame.Height), Fraction.Create(0.8f), LineCount.Ntsc, Interlaced: false, Fraction.MaxValue,
        Fraction.MinValue, ShortSideLength.Neutral, Distortion.Create(0f), LensFraming.Edges, Timing.Default);

    public static CrtDisplay CathodeRayTube { get; } = Default with {
        Mask = PhosphorMask.ApertureGrille, Pitch = ShortSideExtent.Create(ReferenceFrame.GreaterSide / 640f), MaskStrength = Fraction.Create(0.705394f), Lines = LineCount.Create(240),
        ScanlineDepth = Fraction.Create(0.5f), Glow = ShortSideLength.Create(0.01f * ReferenceFrame.GreaterSide / 3f), Curvature = Distortion.Create(0.0497677f),
    };

    public static CrtDisplay Scanlines { get; } = Default with { MaskStrength = Fraction.MinValue, ScanlineDepth = Fraction.Create(0.4f) };

    public static Seq<(string Name, EffectLook<CrtDisplay> Look)> Looks { get; } = [Presets.Look(nameof(CathodeRayTube), CathodeRayTube), Presets.Look(nameof(Scanlines), Scanlines)];

    private bool Lit => MaskStrength != Fraction.MinValue || ScanlineDepth != Fraction.MinValue || Collapse != Fraction.MinValue;

    private bool Warped => (float)Curvature is not 0f;

    private (ERational Width, ERational Height) Scales =>
        (ERational.FromSingle(Collapse).Multiply(2), ERational.One.Divide((int)Lines)) switch {
            var (collapse, line) => (ERational.Min(ERational.One, ERational.Max(ERational.FromInt32(2).Subtract(collapse), line)), ERational.Max(ERational.One.Subtract(collapse), line)),
        };

    public static Option<PixelPass> Pass(CrtDisplay state, PassContext context) =>
        (state.Lit, state.Collapse == Fraction.MinValue && state.Glow == ShortSideLength.Neutral && !state.Warped) switch {
            (false, true) => None,
            (true, true) => Some<PixelPass>(new PixelPass.Pointwise(Raster(state, context))),
            _ => Some<PixelPass>(new PixelPass.Frame((frame, progress) =>
                from collapsed in state.Collapse == Fraction.MinValue ? unit : new CoordinateMap.Analytic(Deflection(state.Scales, context.Extent), Sampling.Linear, (WrapMode.Black, WrapMode.Black)).Apply(frame, progress)
                from glowed in state.Glow == ShortSideLength.Neutral ? unit : VideoKernels.Glow(frame, state.Glow.Pixels(context.Extent))
                from rastered in state.Lit ? new PixelPass.Pointwise(Raster(state, context)).Run(frame, progress) : unit
                from warped in state.Warped
                    ? ValidationResults.ToFin(RadialDistortion.Validate(state.Curvature, 0d, state.Framing, LensDirection.Distort, out RadialDistortion? lens), lens).Bind(held => held.Map(WrapMode.Black).Apply(frame, progress))
                    : unit
                select warped)),
        };

    private static Action<Span<(double X, double Y)>, Span<float>, PixelExtent> Deflection((ERational Width, ERational Height) scale, PixelExtent extent) {
        (ERational X, ERational Y) = (ERational.FromInt32(extent.Width).Divide(2), ERational.FromInt32(extent.Height).Divide(2));
        return (points, _, _) => {
            foreach (ref (double X, double Y) point in points)
                point = (Sampling.Linear.Point(X.Add(ERational.FromDouble(point.X).Subtract(X).Divide(scale.Width)), WrapMode.Black, extent.Width),
                    Sampling.Linear.Point(Y.Add(ERational.FromDouble(point.Y).Subtract(Y).Divide(scale.Height)), WrapMode.Black, extent.Height));
        };
    }

    private static Action<Span<Vector4>, long, long> Raster(CrtDisplay state, PassContext context) =>
        VideoKernels.Raster(context.Extent, state.Lines, state.Interlaced, SystemM.Frame(state.Timing.At(context)), state.Scales, state.Mask,
            ERational.FromSingle(state.Pitch).Multiply(context.Extent.ShortSide), state.MaskStrength, state.ScanlineDepth);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CrtDisplayParameter : IStateParameter<CrtDisplay> {
    public static readonly CrtDisplayParameter Mask = new("mask", new StateParameter<CrtDisplay>.Choice<PhosphorMask>(Lens<CrtDisplay, PhosphorMask>.New(static crt => crt.Mask, static mask => crt => crt with { Mask = mask })),
        new("Mask", "Phosphor layout behind the glass"));
    public static readonly CrtDisplayParameter Pitch = new("pitch", StateParameter.Bounded(Lens<CrtDisplay, ShortSideExtent>.New(static crt => crt.Pitch, static pitch => crt => crt with { Pitch = pitch }), Presentations.Extent with { Soft = (0.002f, 0.02f) }),
        new("Triad pitch", "Phosphor triad width in frame short sides"));
    public static readonly CrtDisplayParameter MaskStrength = new("mask-strength", StateParameter.Bounded(Lens<CrtDisplay, Fraction>.New(static crt => crt.MaskStrength, static strength => crt => crt with { MaskStrength = strength })),
        new("Mask strength", "Light the mask moves onto each phosphor"));
    public static readonly CrtDisplayParameter Lines = new("lines", StateParameter.Bounded(Lens<CrtDisplay, LineCount>.New(static crt => crt.Lines, static lines => crt => crt with { Lines = lines })),
        new("Lines", "Scanlines across the frame height"));
    public static readonly CrtDisplayParameter Interlaced = new("interlaced", new StateParameter<CrtDisplay>.Toggle(Lens<CrtDisplay, bool>.New(static crt => crt.Interlaced, static interlaced => crt => crt with { Interlaced = interlaced })),
        new("Interlaced", "Lights alternate lines on alternate frames"));
    public static readonly CrtDisplayParameter ScanlineDepth = new("scanline-depth", StateParameter.Bounded(Lens<CrtDisplay, Fraction>.New(static crt => crt.ScanlineDepth, static depth => crt => crt with { ScanlineDepth = depth })),
        new("Scanline depth", "Darkening between scanlines"));
    public static readonly CrtDisplayParameter Collapse = new("collapse", StateParameter.Bounded(Lens<CrtDisplay, Fraction>.New(static crt => crt.Collapse, static collapse => crt => crt with { Collapse = collapse })),
        new("Collapse", "Raster shrink to line then dot on power loss"));
    public static readonly CrtDisplayParameter Glow = new("glow", StateParameter.Bounded(Lens<CrtDisplay, ShortSideLength>.New(static crt => crt.Glow, static glow => crt => crt with { Glow = glow }), Presentations.Length with { Soft = (0f, 0.005f) }),
        new("Glow", "Beam light spread in frame short sides"));
    public static readonly CrtDisplayParameter Curvature = new("curvature", StateParameter.Bounded(Lens<CrtDisplay, Distortion>.New(static crt => crt.Curvature, static curvature => crt => crt with { Curvature = curvature })),
        new("Curvature", "Tube glass bulge"));
    public static readonly CrtDisplayParameter Framing = new("framing", new StateParameter<CrtDisplay>.Choice<LensFraming>(Lens<CrtDisplay, LensFraming>.New(static crt => crt.Framing, static framing => crt => crt with { Framing = framing })),
        new("Fit", "Warped frame scale filling edges or corners"));
    public static readonly CrtDisplayParameter Timing = new("timing", new StateParameter<CrtDisplay>.Record<Timing>(Lens<CrtDisplay, Timing>.New(static crt => crt.Timing, static timing => crt => crt with { Timing = timing })),
        new("Timing", "Start time and pace over recorded frames"));

    public StateParameter<CrtDisplay> Kind { get; }
    public ParameterText Text { get; }
}

public sealed record AnalogVideo(
    VideoStandard Matrix, LineCount Lines,
    ShortSideLength LumaBandwidth, ShortSideLength ChromaBandwidth, ShortSideLength ChromaVertical, ShortSideLength ChromaPitch, ShortSideOffset ChromaDelay,
    ShortSideLength Smear, Fraction SmearMix, Magnitude Ringing, ShortSideOffset RingingDelay, Magnitude Emboss, Fraction Ghost, ShortSideOffset GhostDelay, Fraction CrossColor,
    Magnitude LumaNoise, Magnitude ChromaNoise, ShortSideExtent NoiseLength, Fraction Snow, Fraction SnowDensity, ShortSideExtent SnowLength,
    ShortSideOffset Interlace, ShortSideLength Jitter, ShortSideExtent JitterPeriod, ShortSideOffset Wrinkle, ShortSideExtent WrinkleHeight, Hertz WrinkleRate,
    ShortSideOffset HeadSwitch, ShortSideExtent HeadSwitchHeight, ShortSideLength Bounce, Hertz RollRate, Fraction Hum, Hertz HumRate, Seed Seed, Timing Timing)
    : IStateRecord<AnalogVideo, AnalogVideoParameter>, IPixelStage<AnalogVideo> {
    public static AnalogVideo Default { get; } = new(
        VideoStandard.Rec601, LineCount.Ntsc,
        ShortSideLength.Neutral, ShortSideLength.Neutral, ShortSideLength.Neutral, ShortSideLength.Neutral, ShortSideOffset.Neutral,
        ShortSideLength.Neutral, Fraction.Create(0.3f * 0.25f), Magnitude.Zero, ShortSideOffset.Neutral, Magnitude.Zero, Fraction.MinValue, ShortSideOffset.Neutral, Fraction.MinValue,
        Magnitude.Zero, Magnitude.Zero, ShortSideExtent.Create(ReferenceFrame.GreaterSide / 40f), Fraction.MinValue, Fraction.MinValue, ShortSideExtent.Create(0.04f * ReferenceFrame.GreaterSide),
        ShortSideOffset.Neutral, ShortSideLength.Neutral, ShortSideExtent.Create(1f / 65f), ShortSideOffset.Neutral, ShortSideExtent.Create(0.15f), Hertz.Neutral,
        ShortSideOffset.Neutral, ShortSideExtent.Create(0.31f / (32f - (28.8f * 0.5f))), ShortSideLength.Neutral, Hertz.Neutral, Fraction.MinValue, Hertz.Create(60f / 1001f),
        Seed.MinValue, Timing.Default);

    public static AnalogVideo ChromaBlur { get; } =
        Default with { ChromaBandwidth = ShortSideLength.Create(128f / ReferenceFrame.Height / 3f), ChromaVertical = ShortSideLength.Create(12.8f / ReferenceFrame.Height / 3f) };

    public static Seq<(string Name, EffectLook<AnalogVideo> Look)> Looks { get; } = [Presets.Look(nameof(ChromaBlur), ChromaBlur)];

    private bool Moves =>
        Interlace != ShortSideOffset.Neutral || Jitter != ShortSideLength.Neutral || Wrinkle != ShortSideOffset.Neutral
        || HeadSwitch != ShortSideOffset.Neutral || Bounce != ShortSideLength.Neutral || RollRate != Hertz.Neutral;

    private bool Speckled => (Snow != Fraction.MinValue && SnowDensity != Fraction.MinValue) || Hum != Fraction.MinValue;

    private bool Filters(PixelExtent extent) =>
        LumaBandwidth != ShortSideLength.Neutral || ChromaBandwidth != ShortSideLength.Neutral || ChromaVertical != ShortSideLength.Neutral || ChromaPitch.Pixels(extent) > 1d
        || ChromaDelay != ShortSideOffset.Neutral || (Smear != ShortSideLength.Neutral && SmearMix != Fraction.MinValue) || Ghost != Fraction.MinValue || CrossColor != Fraction.MinValue
        || LumaNoise != Magnitude.Zero || ChromaNoise != Magnitude.Zero;

    public static Option<PixelPass> Pass(AnalogVideo state, PassContext context) =>
        state.Moves || state.Filters(context.Extent) || state.Speckled ? Some<PixelPass>(new PixelPass.Frame((frame, progress) => Kernel(state, context, frame, progress))) : None;

    private static Fin<Unit> Kernel(AnalogVideo state, PassContext context, PixelFrame frame, IProgress<int> progress) {
        (PixelExtent extent, Clock now) = (context.Extent, state.Timing.At(context));
        uint field = state.Seed.Field(2u, now.Term);
        return (state.Moves || state.Filters(extent)
                ? frame.Filter(header =>
                    from synced in state.Moves ? Sync(state, extent, now, field, frame, progress) : unit
                    from signaled in state.Filters(extent) ? Signal(state, extent, now, field, frame, header, progress) : unit
                    select signaled)
                : unit)
            .Bind(_ => state.Speckled
                ? new PixelPass.Pointwise(VideoKernels.Snow(extent, state.Lines, ERational.FromSingle(state.SnowLength).Multiply(extent.ShortSide), state.Snow, state.SnowDensity, state.Hum,
                    LatticePoint.Split(ERational.FromSingle(state.HumRate).Multiply(ERational.FromSingle(now))).Local, state.Matrix.Signal.Luminance, CoordinateHash.Branch(field, 1u))).Run(frame, progress)
                : unit);
    }

    private static Fin<Unit> Sync(AnalogVideo state, PixelExtent extent, Clock now, uint field, PixelFrame frame, IProgress<int> progress) {
        double sigma = ERational.FromSingle(state.JitterPeriod).Multiply(extent.ShortSide).Multiply((int)state.Lines).Divide(extent.Height).ToDouble();
        (uint jitterDraws, ERational clock) = (CoordinateHash.Branch(field, 2u), ERational.FromSingle(now));
        ERational rise = LatticePoint.Split(ERational.FromSingle(state.RollRate).Multiply(clock)).Local.Multiply(extent.Height);
        ERational jump = ERational.FromSingle(state.Bounce).Multiply(extent.ShortSide).Multiply(ERational.FromSingle(CoordinateHash.White(Vector4.Zero, CoordinateHash.Branch(field, 3u)).X).Multiply(2).Subtract(1));
        ERational crest = ERational.One.Subtract(LatticePoint.Split(ERational.FromSingle(state.WrinkleRate).Multiply(clock)).Local).Multiply(extent.Height);
        (ERational interlace, ERational jitter, ERational wrinkle, ERational wrinkleBand) =
            (ERational.FromSingle(state.Interlace).Multiply(extent.ShortSide), ERational.FromSingle(state.Jitter).Multiply(extent.ShortSide),
                ERational.FromSingle(state.Wrinkle).Multiply(extent.ShortSide), ERational.FromSingle(state.WrinkleHeight).Multiply(extent.ShortSide));
        (ERational switching, ERational switchBand) = (ERational.FromSingle(state.HeadSwitch).Multiply(extent.ShortSide), ERational.FromSingle(state.HeadSwitchHeight).Multiply(extent.ShortSide));
        return (state.Jitter == ShortSideLength.Neutral ? Gaussian.Kernel(0d) : VideoKernels.Unit(sigma)).Map(static taps => {
            using (taps) {
                double[] weights = new double[taps.Rows];
                taps.CopyTo(weights);
                return weights;
            }
        }).Bind(weights => new CoordinateMap.Composed(
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
                    EInteger line = SystemM.Line((long)Math.Floor(points[0].Y), state.Lines, extent.Height);
                    double draw = 0d;
                    for (int tap = 0; tap < weights.Length; tap++)
                        draw += weights[tap] * CoordinateHash.Normals(EInteger.Zero, line.Add(tap - (weights.Length / 2)), jitterDraws).X;
                    foreach (ref (double X, double Y) point in points) {
                        ERational row = ERational.FromDouble(point.Y);
                        ERational distance = row.Subtract(crest);
                        distance = distance.Subtract(ERational.FromEInteger(LatticePoint.Split(distance.Divide(extent.Height).Add(ERational.One.Divide(2))).Cell).Multiply(extent.Height));
                        ERational band = ERational.FromInt32(extent.Height).Subtract(row).Divide(switchBand);
                        ERational crestShare = ERational.FromDouble(0.9d);
                        ERational head = band.Sign >= 0 && band.CompareTo(ERational.One) < 0
                            ? switching.Multiply(band.CompareTo(crestShare) < 0 ? band.Divide(crestShare) : ERational.One.Subtract(band).Divide(ERational.One.Subtract(crestShare)))
                            : ERational.Zero;
                        ERational offset = interlace.Multiply(line.IsEven ? 0 : 1).Add(jitter.Multiply(ERational.FromDouble(draw)))
                            .Add(wrinkle.Multiply(ERational.Max(ERational.Zero, ERational.One.Subtract(distance.Abs().Divide(wrinkleBand))))).Add(head);
                        point.X = Sampling.Linear.Point(ERational.FromDouble(point.X).Subtract(offset), WrapMode.Clamp, extent.Width);
                    }
                }, Sampling.Linear, (WrapMode.Clamp, WrapMode.Clamp))).Apply(frame, progress));
    }

    private static Fin<Unit> Signal(AnalogVideo state, PixelExtent extent, Clock now, uint field, PixelFrame frame, Mat header, IProgress<int> progress) =>
        VideoKernels.Decomposed(header, state.Matrix.Signal, (luma, blue, red) =>
            (state.CrossColor == Fraction.MinValue ? unit : VideoKernels.CrossColor(luma, blue, red, frame, state.Lines, SystemM.Frame(now), state.CrossColor))
                .Bind(_ => state.LumaNoise == Magnitude.Zero && state.ChromaNoise == Magnitude.Zero ? unit
                    : VideoKernels.LineNoise(luma, blue, red, frame, state.Lines, state.NoiseLength.Pixels(extent), state.LumaNoise, state.ChromaNoise, CoordinateHash.Branch(field, 0u)))
                .Bind(_ => Luma(state, extent, luma))
                .Bind(_ => Chroma(state, extent, blue))
                .Bind(_ => Chroma(state, extent, red)))
            .Map(_ => {
                progress.Report(frame.Size.Height);
                return unit;
            });

    private static Fin<Unit> Luma(AnalogVideo state, PixelExtent extent, Mat luma) {
        (double sigma, double smear) = (state.LumaBandwidth.Pixels(extent), state.Smear.Pixels(extent));
        (bool rings, bool embosses) = (sigma > 0d && state.Ringing != Magnitude.Zero, sigma > 0d && state.Emboss != Magnitude.Zero);
        using Mat ring = new();
        using Mat relief = new();
        return (sigma > 0d ? VideoKernels.Filter(luma, Gaussian.Kernel(sigma)) : unit)
            .Bind(_ => {
                if (rings)
                    CvInvoke.Sobel(luma, ring, DepthType.Cv64F, 2, 0, 1, 1d, 0d, BorderType.Replicate);
                if (embosses)
                    CvInvoke.Sobel(luma, relief, DepthType.Cv64F, 1, 0, 1, 1d, 0d, BorderType.Replicate);
                return rings ? VideoKernels.Filter(ring, VideoKernels.Echo(0d, state.RingingDelay.Pixels(extent), 1f)) : unit;
            })
            .Bind(_ => smear > 0d && state.SmearMix != Fraction.MinValue ? VideoKernels.Filter(luma, VideoKernels.Echo(smear, smear, state.SmearMix)) : unit)
            .Bind(_ => {
                if (rings)
                    CvInvoke.ScaleAdd(ring, -(float)state.Ringing * sigma * sigma, luma, luma);
                if (embosses)
                    CvInvoke.ScaleAdd(relief, -(float)state.Emboss * sigma / 2d, luma, luma);
                return state.Ghost == Fraction.MinValue ? unit : VideoKernels.Filter(luma, VideoKernels.Echo(0d, state.GhostDelay.Pixels(extent), state.Ghost));
            });
    }

    private static Fin<Unit> Chroma(AnalogVideo state, PixelExtent extent, Mat plane) {
        double pitch = state.ChromaPitch.Pixels(extent);
        if (pitch > 1d) {
            using Mat coarse = new();
            CvInvoke.Resize(plane, coarse, new Size(int.Max(1, (int)Math.Round(plane.Cols / pitch, MidpointRounding.ToEven)), plane.Rows), 0d, 0d, Inter.Area);
            CvInvoke.Resize(coarse, plane, plane.Size, 0d, 0d, Inter.Linear);
        }
        return (state.ChromaBandwidth == ShortSideLength.Neutral ? unit : VideoKernels.Filter(plane, Gaussian.Kernel(state.ChromaBandwidth.Pixels(extent))))
            .Bind(_ => state.ChromaVertical == ShortSideLength.Neutral ? unit : VideoKernels.Filter(plane, Gaussian.Kernel(state.ChromaVertical.Pixels(extent)), vertical: true))
            .Bind(_ => state.ChromaDelay == ShortSideOffset.Neutral ? unit : VideoKernels.Filter(plane, VideoKernels.Echo(0d, state.ChromaDelay.Pixels(extent), 1f)))
            .Bind(_ => state.Ghost == Fraction.MinValue ? unit : VideoKernels.Filter(plane, VideoKernels.Echo(0d, state.GhostDelay.Pixels(extent), state.Ghost)));
    }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class AnalogVideoParameter : IStateParameter<AnalogVideo> {
    private static readonly Presentation<float> Band = Presentations.Length with { Soft = (0f, 0.005f) };
    private static readonly Presentation<float> Delay = Presentations.Offset with { Soft = (-0.02f, 0.02f) };
    private static readonly Presentation<float> Span = Presentations.Extent with { Soft = (0.001f, 0.5f) };
    private static readonly Presentation<float> Gain = Magnitude.Presentation with { Soft = (0f, 2f) };
    private static readonly Presentation<float> Noise = Magnitude.Presentation with { Soft = (0f, 0.2f) };
    private static readonly Presentation<float> Passes = Hertz.Presentation with { Soft = (-1f, 1f) };

    public static readonly AnalogVideoParameter Matrix = new("matrix", new StateParameter<AnalogVideo>.Choice<VideoStandard>(Lens<AnalogVideo, VideoStandard>.New(static video => video.Matrix, static matrix => video => video with { Matrix = matrix })),
        new("Matrix", "Video standard for Y, Cb, and Cr weights"));
    public static readonly AnalogVideoParameter Lines = new("lines", StateParameter.Bounded(Lens<AnalogVideo, LineCount>.New(static video => video.Lines, static lines => video => video with { Lines = lines })),
        new("Lines", "Raster lines across the frame height"));
    public static readonly AnalogVideoParameter LumaBandwidth = new("luma-bandwidth", StateParameter.Bounded(Lens<AnalogVideo, ShortSideLength>.New(static video => video.LumaBandwidth, static spread => video => video with { LumaBandwidth = spread }), Band),
        new("Luma blur", "Luma band limit along each line in frame short sides"));
    public static readonly AnalogVideoParameter ChromaBandwidth = new("chroma-bandwidth", StateParameter.Bounded(Lens<AnalogVideo, ShortSideLength>.New(static video => video.ChromaBandwidth, static spread => video => video with { ChromaBandwidth = spread }), Band),
        new("Chroma blur", "Chroma band limit along each line in frame short sides"));
    public static readonly AnalogVideoParameter ChromaVertical = new("chroma-vertical", StateParameter.Bounded(Lens<AnalogVideo, ShortSideLength>.New(static video => video.ChromaVertical, static spread => video => video with { ChromaVertical = spread }), Band),
        new("Chroma vertical blur", "Chroma blur across lines in frame short sides"));
    public static readonly AnalogVideoParameter ChromaPitch = new("chroma-pitch", StateParameter.Bounded(Lens<AnalogVideo, ShortSideLength>.New(static video => video.ChromaPitch, static pitch => video => video with { ChromaPitch = pitch }), Presentations.Length with { Soft = (0f, 0.01f) }),
        new("Chroma sampling", "Chroma sample spacing along each line in frame short sides"));
    public static readonly AnalogVideoParameter ChromaDelay = new("chroma-delay", StateParameter.Bounded(Lens<AnalogVideo, ShortSideOffset>.New(static video => video.ChromaDelay, static delay => video => video with { ChromaDelay = delay }), Delay),
        new("Chroma delay", "Shift of both color difference signals against luma"));
    public static readonly AnalogVideoParameter Smear = new("smear", StateParameter.Bounded(Lens<AnalogVideo, ShortSideLength>.New(static video => video.Smear, static smear => video => video with { Smear = smear }), Band),
        new("Smear", "Luma trail length right of each edge"));
    public static readonly AnalogVideoParameter SmearMix = new("smear-mix", StateParameter.Bounded(Lens<AnalogVideo, Fraction>.New(static video => video.SmearMix, static mix => video => video with { SmearMix = mix })),
        new("Smear amount", "Trail share mixed into luma"));
    public static readonly AnalogVideoParameter Ringing = new("ringing", StateParameter.Bounded(Lens<AnalogVideo, Magnitude>.New(static video => video.Ringing, static gain => video => video with { Ringing = gain }), Gain),
        new("Ringing", "Overshoot beside edges at luma band scale"));
    public static readonly AnalogVideoParameter RingingDelay = new("ringing-delay", StateParameter.Bounded(Lens<AnalogVideo, ShortSideOffset>.New(static video => video.RingingDelay, static delay => video => video with { RingingDelay = delay }), Delay),
        new("Ringing delay", "Signed shift of ringing from image edges"));
    public static readonly AnalogVideoParameter Emboss = new("emboss", StateParameter.Bounded(Lens<AnalogVideo, Magnitude>.New(static video => video.Emboss, static gain => video => video with { Emboss = gain }), Gain),
        new("Emboss", "Relief from band-limited luma slope"));
    public static readonly AnalogVideoParameter Ghost = new("ghost", StateParameter.Bounded(Lens<AnalogVideo, Fraction>.New(static video => video.Ghost, static share => video => video with { Ghost = share })),
        new("Ghost", "Strength of delayed signal copy"));
    public static readonly AnalogVideoParameter GhostDelay = new("ghost-delay", StateParameter.Bounded(Lens<AnalogVideo, ShortSideOffset>.New(static video => video.GhostDelay, static delay => video => video with { GhostDelay = delay }), Presentations.Offset with { Soft = (-0.05f, 0.05f) }),
        new("Ghost delay", "Delayed copy shift"));
    public static readonly AnalogVideoParameter CrossColor = new("cross-color", StateParameter.Bounded(Lens<AnalogVideo, Fraction>.New(static video => video.CrossColor, static mix => video => video with { CrossColor = mix })),
        new("Cross color", "Rainbow fringes fine luma detail leaks into chroma"));
    public static readonly AnalogVideoParameter LumaNoise = new("luma-noise", StateParameter.Bounded(Lens<AnalogVideo, Magnitude>.New(static video => video.LumaNoise, static level => video => video with { LumaNoise = level }), Noise),
        new("Luma noise", "Gaussian noise deviation in luma before band limit"));
    public static readonly AnalogVideoParameter ChromaNoise = new("chroma-noise", StateParameter.Bounded(Lens<AnalogVideo, Magnitude>.New(static video => video.ChromaNoise, static level => video => video with { ChromaNoise = level }), Noise),
        new("Chroma noise", "Independent Gaussian noise deviation in each chroma signal before band limit"));
    public static readonly AnalogVideoParameter NoiseLength = new("noise-length", StateParameter.Bounded(Lens<AnalogVideo, ShortSideExtent>.New(static video => video.NoiseLength, static length => video => video with { NoiseLength = length }), Span),
        new("Noise length", "Gaussian noise correlation along each line in frame short sides"));
    public static readonly AnalogVideoParameter Snow = new("snow", StateParameter.Bounded(Lens<AnalogVideo, Fraction>.New(static video => video.Snow, static snow => video => video with { Snow = snow })),
        new("Snow", "Brightness of snow streaks"));
    public static readonly AnalogVideoParameter SnowDensity = new("snow-density", StateParameter.Bounded(Lens<AnalogVideo, Fraction>.New(static video => video.SnowDensity, static density => video => video with { SnowDensity = density })),
        new("Snow density", "Share of line cells lit as snow"));
    public static readonly AnalogVideoParameter SnowLength = new("snow-length", StateParameter.Bounded(Lens<AnalogVideo, ShortSideExtent>.New(static video => video.SnowLength, static length => video => video with { SnowLength = length }), Span),
        new("Snow length", "Snow cell fading head length in frame short sides"));
    public static readonly AnalogVideoParameter Interlace = new("interlace", StateParameter.Bounded(Lens<AnalogVideo, ShortSideOffset>.New(static video => video.Interlace, static offset => video => video with { Interlace = offset }), Presentations.Offset with { Soft = (-0.01f, 0.01f) }),
        new("Interlace", "Shift of odd lines against even lines"));
    public static readonly AnalogVideoParameter Jitter = new("jitter", StateParameter.Bounded(Lens<AnalogVideo, ShortSideLength>.New(static video => video.Jitter, static jitter => video => video with { Jitter = jitter }), Presentations.Length with { Soft = (0f, 0.01f) }),
        new("Line jitter", "Standard deviation of Gaussian sideways line shifts"));
    public static readonly AnalogVideoParameter JitterPeriod = new("jitter-period", StateParameter.Bounded(Lens<AnalogVideo, ShortSideExtent>.New(static video => video.JitterPeriod, static period => video => video with { JitterPeriod = period }), Span),
        new("Jitter period", "Height over which line shifts change"));
    public static readonly AnalogVideoParameter Wrinkle = new("wrinkle", StateParameter.Bounded(Lens<AnalogVideo, ShortSideOffset>.New(static video => video.Wrinkle, static offset => video => video with { Wrinkle = offset }), Presentations.Offset with { Soft = (-0.05f, 0.05f) }),
        new("Wrinkle", "Sideways shift inside rolling wrinkle band"));
    public static readonly AnalogVideoParameter WrinkleHeight = new("wrinkle-height", StateParameter.Bounded(Lens<AnalogVideo, ShortSideExtent>.New(static video => video.WrinkleHeight, static height => video => video with { WrinkleHeight = height }), Span),
        new("Wrinkle height", "Wrinkle band half height"));
    public static readonly AnalogVideoParameter WrinkleRate = new("wrinkle-rate", StateParameter.Bounded(Lens<AnalogVideo, Hertz>.New(static video => video.WrinkleRate, static rate => video => video with { WrinkleRate = rate }), Passes),
        new("Wrinkle rate", "Upward wrinkle band passes per second"));
    public static readonly AnalogVideoParameter HeadSwitch = new("head-switch", StateParameter.Bounded(Lens<AnalogVideo, ShortSideOffset>.New(static video => video.HeadSwitch, static offset => video => video with { HeadSwitch = offset }), Presentations.Offset with { Soft = (-0.1f, 0.1f) }),
        new("Head switching", "Bottom band skew where tape heads switch"));
    public static readonly AnalogVideoParameter HeadSwitchHeight = new("head-switch-height", StateParameter.Bounded(Lens<AnalogVideo, ShortSideExtent>.New(static video => video.HeadSwitchHeight, static height => video => video with { HeadSwitchHeight = height }),
        Presentations.Extent with { Soft = (0.001f, 0.1f) }),
        new("Head switching height", "Skewed band height"));
    public static readonly AnalogVideoParameter Bounce = new("bounce", StateParameter.Bounded(Lens<AnalogVideo, ShortSideLength>.New(static video => video.Bounce, static bounce => video => video with { Bounce = bounce }), Presentations.Length with { Soft = (0f, 0.02f) }),
        new("Bounce", "Largest vertical picture jump"));
    public static readonly AnalogVideoParameter RollRate = new("roll-rate", StateParameter.Bounded(Lens<AnalogVideo, Hertz>.New(static video => video.RollRate, static rate => video => video with { RollRate = rate }), Passes),
        new("Roll rate", "Frame heights the picture rolls up per second"));
    public static readonly AnalogVideoParameter Hum = new("hum", StateParameter.Bounded(Lens<AnalogVideo, Fraction>.New(static video => video.Hum, static hum => video => video with { Hum = hum })),
        new("Hum", "Hum bar luma darkening, chroma kept"));
    public static readonly AnalogVideoParameter HumRate = new("hum-rate", StateParameter.Bounded(Lens<AnalogVideo, Hertz>.New(static video => video.HumRate, static rate => video => video with { HumRate = rate }), Passes),
        new("Hum rate", "Upward hum bar passes per second"));
    public static readonly AnalogVideoParameter Seed = new("seed", StateParameter.Bounded(Lens<AnalogVideo, Seed>.New(static video => video.Seed, static seed => video => video with { Seed = seed })),
        new("Seed", "Number selecting pattern"));
    public static readonly AnalogVideoParameter Timing = new("timing", new StateParameter<AnalogVideo>.Record<Timing>(Lens<AnalogVideo, Timing>.New(static video => video.Timing, static timing => video => video with { Timing = timing })),
        new("Timing", "Start time and pace over recorded frames"));

    public StateParameter<AnalogVideo> Kind { get; }
    public ParameterText Text { get; }
}

public sealed record CodecDamage(JpegQuality Quality, JpegColorType ColorType, PositiveCount Generations, Gated<LineCount> Coded) : IStateRecord<CodecDamage, CodecDamageParameter>, IPixelStage<CodecDamage> {
    public static CodecDamage Default { get; } = new(JpegQuality.Create(75), JpegColorType.YCbCrRatio420, PositiveCount.MinValue, new(Enabled: false, LineCount.Coarse));

    public static CodecDamage Jpeg { get; } = Default with { Quality = JpegQuality.Create(60), Coded = new(Enabled: true, LineCount.Create(360)) };

    public static Seq<(string Name, EffectLook<CodecDamage> Look)> Looks { get; } = [Presets.Look(nameof(Jpeg), Jpeg)];

    public static Option<PixelPass> Pass(CodecDamage state, PassContext context) =>
        Some<PixelPass>(new PixelPass.Frame((frame, progress) =>
            VideoKernels.Recompressed(frame, new JpegEncoder { Quality = state.Quality, ColorType = state.ColorType }, state.Generations, state.Coded.Active.Map(static lines => (int)lines), progress)));
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CodecDamageParameter : IStateParameter<CodecDamage> {
    public static readonly CodecDamageParameter Quality = new("quality", StateParameter.Bounded(Lens<CodecDamage, JpegQuality>.New(static codec => codec.Quality, static quality => codec => codec with { Quality = quality }), new Presentation<int> { Step = 1 }),
        new("Quality", "JPEG quality each generation encodes at"));
    public static readonly CodecDamageParameter ColorType = new("color-type", new StateParameter<CodecDamage>.Enumerated<JpegColorType>(Lens<CodecDamage, JpegColorType>.New(static codec => codec.ColorType, static type => codec => codec with { ColorType = type })),
        new("Subsampling", "Color model and chroma subsampling the encoder codes"));
    public static readonly CodecDamageParameter Generations = new("generations", StateParameter.Bounded(Lens<CodecDamage, PositiveCount>.New(static codec => codec.Generations, static generations => codec => codec with { Generations = generations }),
        PositiveCount.Presentation with { Soft = (1, 8), Step = 1 }),
        new("Generations", "Encode and decode cycles, each recompressing the last"));
    public static readonly CodecDamageParameter Coded = new("coded-lines", StateParameter.OptionalBounded(Lens<CodecDamage, Gated<LineCount>>.New(static codec => codec.Coded, static lines => codec => codec with { Coded = lines })),
        new("Coded lines", "Coded picture height, off using frame height"));

    public StateParameter<CodecDamage> Kind { get; }
    public ParameterText Text { get; }
}

public sealed record Glitch(
    ShortSideExtent BlockWidth, ShortSideExtent BlockHeight, Fraction BlockDensity, ShortSideLength BlockShift, ShortSideLength BlockLift,
    ShortSideLength Tear, ShortSideExtent TearPeriod, ShortSideOffset Split, SignedAngle SplitDirection, Fraction SplitJitter,
    Fraction Slide, VideoStandard Matrix, Fraction LumaCorruption, Fraction ChromaCorruption, WrapMode Wrap, Seed Seed, Timing Timing, Hold Hold)
    : IStateRecord<Glitch, GlitchParameter>, IPixelStage<Glitch> {
    private static readonly GaussLegendreRule Smear = new(0d, 1d, 1 << 6);

    public static Glitch Default { get; } = new(
        ShortSideExtent.Create(1f / (5f * 0.1f)), ShortSideExtent.Create(1f / 5f), Fraction.MinValue, ShortSideLength.Neutral, ShortSideLength.Neutral,
        ShortSideLength.Neutral, ShortSideExtent.Create(MathF.Tau / 20f), ShortSideOffset.Neutral, SignedAngle.Neutral, Fraction.MinValue,
        Fraction.MinValue, VideoStandard.Rec601, Fraction.MinValue, Fraction.MinValue, WrapMode.Periodic, Seed.MinValue, Timing.Default, Hold.MinValue);

    public static Glitch Corruption { get; } = Default with {
        BlockWidth = ShortSideExtent.Create(0.03f * ReferenceFrame.GreaterSide), BlockHeight = ShortSideExtent.Create(0.03f * ReferenceFrame.GreaterSide), BlockDensity = Fraction.Create(0.2f),
        BlockShift = ShortSideLength.Create(0.004f * ReferenceFrame.GreaterSide), ChromaCorruption = Fraction.Create(0.6f), Hold = Hold.Create(12f / 24f),
    };

    public static Glitch BlockGlitch { get; } = Default with {
        BlockDensity = Fraction.Create(0.2f), BlockShift = ShortSideLength.Create(0.05f * ReferenceFrame.GreaterSide), Split = ShortSideOffset.Create(0.03f * ReferenceFrame.GreaterSide), Hold = Hold.Create(2f / 24f),
    };

    public static Seq<(string Name, EffectLook<Glitch> Look)> Looks { get; } = [Presets.Look(nameof(Corruption), Corruption), Presets.Look(nameof(BlockGlitch), BlockGlitch)];

    private bool Blocks => BlockDensity != Fraction.MinValue && (BlockShift != ShortSideLength.Neutral || BlockLift != ShortSideLength.Neutral);

    private bool Moves => Blocks || Tear != ShortSideLength.Neutral;

    private bool Corrupts => BlockDensity != Fraction.MinValue && (LumaCorruption != Fraction.MinValue || ChromaCorruption != Fraction.MinValue);

    public static Option<PixelPass> Pass(Glitch state, PassContext context) =>
        state.Moves || state.Split != ShortSideOffset.Neutral || state.Corrupts ? Some<PixelPass>(new PixelPass.Frame((frame, progress) => Kernel(state, context, frame, progress))) : None;

    private static Fin<Unit> Kernel(Glitch state, PassContext context, PixelFrame frame, IProgress<int> progress) {
        PixelExtent extent = context.Extent;
        uint field = state.Seed.Field(3u, state.Hold.Period(state.Timing.At(context)));
        Vector4 draw = CoordinateHash.White(Vector4.Zero, CoordinateHash.Branch(field, 3u));
        CoordinateMap blocks = new CoordinateMap.Analytic(Displacement(state, extent, field, draw.X), Sampling.Nearest, (state.Wrap, state.Wrap));
        return (state.Split != ShortSideOffset.Neutral, state.Moves) switch {
            (true, _) => Splits(state, extent, draw.Y, blocks, frame, progress),
            (false, true) => blocks.Apply(frame, progress),
            (false, false) => unit,
        } switch {
            var moved => moved.Bind(_ => state.Corrupts
                ? new PixelPass.Pointwise(VideoKernels.Corrupted(state.Matrix.Signal, state.LumaCorruption, state.ChromaCorruption, extent,
                    point => state.Cell(extent, point, CoordinateHash.Branch(field, 1u)) switch { (true, var site, _) => Some(site), _ => None }, CoordinateHash.Branch(field, 2u))).Run(frame, progress)
                : unit),
        };
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
                (ERational X, ERational Y) offset = state.Blocks && state.Cell(extent, q, draws) is (true, _, var cell)
                    ? (shift.Multiply(ERational.FromSingle(cell.Y).Multiply(2).Subtract(1)), lift.Multiply(ERational.FromSingle(cell.Z).Multiply(2).Subtract(1)))
                    : (ERational.Zero, ERational.Zero);
                ERational band = LatticePoint.Split(q.Y.Divide(period).Add(ERational.FromSingle(phase))).Local.Subtract(ERational.One.Divide(2));
                point = (Sampling.Nearest.Point(p.X.Subtract(offset.X).Subtract(tear.Multiply(band)), state.Wrap, extent.Width), Sampling.Nearest.Point(p.Y.Subtract(offset.Y), state.Wrap, extent.Height));
            }
        };
    }

    private static Fin<Unit> Splits(Glitch state, PixelExtent extent, float draw, CoordinateMap blocks, PixelFrame frame, IProgress<int> progress) {
        (ERational jitter, ERational slide) = (ERational.FromSingle(state.SplitJitter), ERational.FromSingle(state.Slide));
        ERational reach = ERational.FromSingle(state.Split).Multiply(extent.ShortSide).Multiply(ERational.One.Subtract(jitter).Add(jitter.Multiply(ERational.FromSingle(draw).Multiply(2).Subtract(1))));
        (double X, double Y) = double.SinCos(state.SplitDirection) switch { var (sin, cos) => (cos, -sin) };
        return reach.IsZero ? state.Moves ? blocks.Apply(frame, progress) : unit : CoordinateMap.Integrated(
            Smear,
            tau => Subframe(reach.Multiply(ERational.One.Subtract(slide).Add(slide.Multiply(ERational.FromDouble(tau)))), Vector4.UnitX),
            tau => Subframe(reach.Negate().Multiply(ERational.One.Subtract(slide).Add(slide.Multiply(ERational.FromDouble(tau)))), Vector4.UnitZ),
            _ => Subframe(ERational.Zero, new Vector4(0f, 1f, 0f, 1f)))(frame, progress);

        (CoordinateMap Map, Vector4 Gains) Subframe(ERational shift, Vector4 gains) {
            (ERational X, ERational Y) offset = (shift.Multiply(ERational.FromDouble(X)), shift.Multiply(ERational.FromDouble(Y)));
            CoordinateMap moved = new CoordinateMap.Analytic(
                (points, _, _) => {
                    foreach (ref (double X, double Y) point in points)
                        point = (Sampling.Linear.Point(ERational.FromDouble(point.X).Subtract(offset.X), state.Wrap, extent.Width), Sampling.Linear.Point(ERational.FromDouble(point.Y).Subtract(offset.Y), state.Wrap, extent.Height));
                }, Sampling.Linear, (state.Wrap, state.Wrap));
            return (state.Moves ? new CoordinateMap.Composed(blocks, moved) : moved, gains);
        }
    }

    private (bool Active, (EInteger X, EInteger Y) Site, Vector4 Draw) Cell(PixelExtent extent, (ERational X, ERational Y) q, uint draws) {
        (EInteger X, EInteger Y) site = (LatticePoint.Split(q.X.Divide(ERational.FromSingle(BlockWidth).Multiply(extent.ShortSide))).Cell,
            LatticePoint.Split(q.Y.Divide(ERational.FromSingle(BlockHeight).Multiply(extent.ShortSide))).Cell);
        Vector4 draw = CoordinateHash.Uniforms(site.X, site.Y, draws);
        return (draw.X < BlockDensity, site, draw);
    }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class GlitchParameter : IStateParameter<Glitch> {
    private static readonly Presentation<float> Block = Presentations.Extent with { Soft = (0.01f, 4f) };

    public static readonly GlitchParameter BlockWidth = new("block-width", StateParameter.Bounded(Lens<Glitch, ShortSideExtent>.New(static glitch => glitch.BlockWidth, static width => glitch => glitch with { BlockWidth = width }), Block),
        new("Block width", "Block width in frame short sides"));
    public static readonly GlitchParameter BlockHeight = new("block-height", StateParameter.Bounded(Lens<Glitch, ShortSideExtent>.New(static glitch => glitch.BlockHeight, static height => glitch => glitch with { BlockHeight = height }), Block),
        new("Block height", "Block height in frame short sides"));
    public static readonly GlitchParameter BlockDensity = new("block-density", StateParameter.Bounded(Lens<Glitch, Fraction>.New(static glitch => glitch.BlockDensity, static density => glitch => glitch with { BlockDensity = density })),
        new("Block density", "Share of blocks selected for displacement or corruption"));
    public static readonly GlitchParameter BlockShift = new("block-shift", StateParameter.Bounded(Lens<Glitch, ShortSideLength>.New(static glitch => glitch.BlockShift, static shift => glitch => glitch with { BlockShift = shift }), Presentations.Length with { Soft = (0f, 0.5f) }),
        new("Block shift", "Largest sideways block displacement in frame short sides"));
    public static readonly GlitchParameter BlockLift = new("block-lift", StateParameter.Bounded(Lens<Glitch, ShortSideLength>.New(static glitch => glitch.BlockLift, static lift => glitch => glitch with { BlockLift = lift }), Presentations.Length with { Soft = (0f, 0.1f) }),
        new("Block lift", "Largest vertical block displacement in frame short sides"));
    public static readonly GlitchParameter Tear = new("tear", StateParameter.Bounded(Lens<Glitch, ShortSideLength>.New(static glitch => glitch.Tear, static tear => glitch => glitch with { Tear = tear }), Presentations.Length with { Soft = (0f, 0.1f) }),
        new("Tear", "Torn band sideways shift range in frame short sides"));
    public static readonly GlitchParameter TearPeriod = new("tear-period", StateParameter.Bounded(Lens<Glitch, ShortSideExtent>.New(static glitch => glitch.TearPeriod, static period => glitch => glitch with { TearPeriod = period }), Presentations.Extent with { Soft = (0.01f, 1f) }),
        new("Tear period", "Height of each torn band in frame short sides"));
    public static readonly GlitchParameter Split = new("split", StateParameter.Bounded(Lens<Glitch, ShortSideOffset>.New(static glitch => glitch.Split, static split => glitch => glitch with { Split = split }), Presentations.Offset with { Soft = (-0.05f, 0.05f) }),
        new("RGB split", "Opposed red and blue offset in frame short sides"));
    public static readonly GlitchParameter SplitDirection = new("split-direction", StateParameter.Bounded(Lens<Glitch, SignedAngle>.New(static glitch => glitch.SplitDirection, static direction => glitch => glitch with { SplitDirection = direction }), Presentations.Angle),
        new("Split angle", "Counterclockwise from right"));
    public static readonly GlitchParameter SplitJitter = new("split-jitter", StateParameter.Bounded(Lens<Glitch, Fraction>.New(static glitch => glitch.SplitJitter, static jitter => glitch => glitch with { SplitJitter = jitter })),
        new("Split jitter", "Mix of fixed and random signed split per held period"));
    public static readonly GlitchParameter Slide = new("slide", StateParameter.Bounded(Lens<Glitch, Fraction>.New(static glitch => glitch.Slide, static slide => glitch => glitch with { Slide = slide })),
        new("Slide", "Share of the split each moved channel smears across"));
    public static readonly GlitchParameter Matrix = new("matrix", new StateParameter<Glitch>.Choice<VideoStandard>(Lens<Glitch, VideoStandard>.New(static glitch => glitch.Matrix, static matrix => glitch => glitch with { Matrix = matrix })),
        new("Matrix", "Video standard for Y, Cb, and Cr weights"));
    public static readonly GlitchParameter LumaCorruption = new("luma-corruption", StateParameter.Bounded(Lens<Glitch, Fraction>.New(static glitch => glitch.LumaCorruption, static corruption => glitch => glitch with { LumaCorruption = corruption })),
        new("Luma corruption", "Mix toward luma raised to a random power in selected blocks"));
    public static readonly GlitchParameter ChromaCorruption = new("chroma-corruption", StateParameter.Bounded(Lens<Glitch, Fraction>.New(static glitch => glitch.ChromaCorruption, static corruption => glitch => glitch with { ChromaCorruption = corruption })),
        new("Chroma corruption", "Mix toward chroma raised to a random power in selected blocks"));
    public static readonly GlitchParameter Wrap = new("wrap", new StateParameter<Glitch>.Choice<WrapMode>(Lens<Glitch, WrapMode>.New(static glitch => glitch.Wrap, static wrap => glitch => glitch with { Wrap = wrap })),
        new("Edges", "Fill of pixels a displacement uncovers"));
    public static readonly GlitchParameter Seed = new("seed", StateParameter.Bounded(Lens<Glitch, Seed>.New(static glitch => glitch.Seed, static seed => glitch => glitch with { Seed = seed })),
        new("Seed", "Number selecting pattern"));
    public static readonly GlitchParameter Timing = new("timing", new StateParameter<Glitch>.Record<Timing>(Lens<Glitch, Timing>.New(static glitch => glitch.Timing, static timing => glitch => glitch with { Timing = timing })),
        new("Timing", "Start time and pace over recorded frames"));
    public static readonly GlitchParameter Hold = new("hold", StateParameter.Bounded(Lens<Glitch, Hold>.New(static glitch => glitch.Hold, static hold => glitch => glitch with { Hold = hold })),
        new("Hold", "Seconds between pattern redraws"));

    public StateParameter<Glitch> Kind { get; }
    public ParameterText Text { get; }
}

public sealed record DamageMark(Fraction Likelihood, Hold Hold, Fraction White) : IStateRecord<DamageMark, DamageMarkParameter> {
    public static DamageMark Default { get; } = new(Fraction.MinValue, Hold.Create(6f / 24f), Fraction.MinValue);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class DamageMarkParameter : IStateParameter<DamageMark> {
    public static readonly DamageMarkParameter Likelihood = new("likelihood", StateParameter.Bounded(Lens<DamageMark, Fraction>.New(static mark => mark.Likelihood, static likelihood => mark => mark with { Likelihood = likelihood })),
        new("Likelihood", "Held periods showing marks"));
    public static readonly DamageMarkParameter Hold = new("hold", StateParameter.Bounded(Lens<DamageMark, Hold>.New(static mark => mark.Hold, static hold => mark => mark with { Hold = hold })),
        new("Hold", "Seconds between pattern redraws"));
    public static readonly DamageMarkParameter White = new("white", StateParameter.Bounded(Lens<DamageMark, Fraction>.New(static mark => mark.White, static white => mark => mark with { White = white })),
        new("White", "Share of marks printing white, rest black"));

    public StateParameter<DamageMark> Kind { get; }
    public ParameterText Text { get; }
}

public sealed record FilmSpots(DamageMark Mark, PointCells Cells) : IStateRecord<FilmSpots, FilmSpotsParameter> {
    public static FilmSpots Default { get; } = new(DamageMark.Default,
        new(ShortSideExtent.Create(0.02f * ReferenceFrame.GreaterSide), Fraction.Create(0.02f), Fraction.Create(2f * 0.2f), Fraction.MaxValue, Magnitude.Zero, Fraction.Create(0.3f)));
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FilmSpotsParameter : IStateParameter<FilmSpots> {
    public static readonly FilmSpotsParameter Mark = new("mark", new StateParameter<FilmSpots>.Record<DamageMark>(Lens<FilmSpots, DamageMark>.New(static spots => spots.Mark, static mark => spots => spots with { Mark = mark })),
        new("Timing and polarity", "Mark timing and print tone"));
    public static readonly FilmSpotsParameter Cells = new("cells", new StateParameter<FilmSpots>.Record<PointCells>(Lens<FilmSpots, PointCells>.New(static spots => spots.Cells, static cells => spots => spots with { Cells = cells })),
        new("Cells", "Mark spacing, density, size, variation, spread, and softness"));

    public StateParameter<FilmSpots> Kind { get; }
    public ParameterText Text { get; }
}

public sealed record FilmDamage(
    FilmSpots Specks, FilmSpots Dirt, FilmSpots Blotches, DamageMark Scratches, PrimaryScale ScratchCount, ShortSideLength ScratchWidth, Hertz ScratchRate,
    DamageMark Hairs, Magnitude HairCount, ShortSideLength HairWidth, ShortSideExtent HairLength, NoiseDistortion HairCurl, Hertz HairRate,
    ShortSideLength WeaveX, ShortSideLength WeaveY, Hertz WeaveRate, Seed Seed, Timing Timing)
    : IStateRecord<FilmDamage, FilmDamageParameter>, IPixelStage<FilmDamage> {
    private const uint Stream = 6u;
    private static readonly Progress<int> Unreported = new();

    public static FilmDamage Default { get; } = new(
        FilmSpots.Default with { Mark = DamageMark.Default with { White = Fraction.Create(0.5f) } }, FilmSpots.Default, FilmSpots.Default,
        DamageMark.Default with { White = Fraction.Create(0.7f) }, PrimaryScale.Create(3f), ShortSideLength.Create(0.0008f * ReferenceFrame.GreaterSide), Hertz.Create(0.05f * 24f),
        DamageMark.Default, Magnitude.Unit, ShortSideLength.Create(0.0012f * ReferenceFrame.GreaterSide), ShortSideExtent.Create(0.08f * ReferenceFrame.GreaterSide), NoiseDistortion.Create(1f),
        Hertz.Create(0.1f * 24f), ShortSideLength.Neutral, ShortSideLength.Neutral, Hertz.Create(5f * 0.05f * 0.5f * 24f), Seed.MinValue, Timing.Default);

    public static Option<PixelPass> Pass(FilmDamage state, PassContext context) {
        (Clock clock, PixelExtent extent) = (state.Timing.At(context), context.Extent);
        long denominator = 2L * extent.ShortSide;
        Seq<(DamageMark Mark, bool Held, Func<uint, Func<long, long, (float Coverage, float Value)>> Field)> marks = [
            .. Seq(state.Specks, state.Dirt, state.Blotches).Map(spots => (spots.Mark, true, new Func<uint, Func<long, long, (float Coverage, float Value)>>(field => (column, line) =>
                spots.Cells.Disc((ERational.Create((2L * column) + 1 - extent.Width, denominator), ERational.Create((2L * line) + 1 - extent.Height, denominator)), extent.ShortSide, field,
                    tone => tone < spots.Mark.White ? 1f : 0f)))),
            (state.Scratches, false, field => Scratch(state, extent, clock, field)),
            (state.Hairs, false, field => Hair(state, extent, clock, field))];
        Func<long, long, (float Coverage, float Value)>[] shown = [.. marks
            .Map((mark, index) => (Mark: mark, Period: mark.Mark.Hold.Period(clock), Index: (uint)index))
            .Filter(drawn => CoordinateHash.White(Vector4.Zero, Shot(drawn.Period, drawn.Index)).X < drawn.Mark.Mark.Likelihood)
            .Map(drawn => drawn.Mark.Field(Shot(drawn.Mark.Held ? drawn.Period : 0u, drawn.Index)))];
        Option<PixelPass> composite = shown is [] ? None : Some<PixelPass>(Composite(shown, extent));
        return (state.WeaveX, state.WeaveY) == (ShortSideLength.Neutral, ShortSideLength.Neutral) ? composite : Weave(state, extent, clock, (uint)marks.Count) switch {
            var map => Some<PixelPass>(new PixelPass.Frame((frame, progress) => composite.Map(pass => pass.Run(frame, Unreported)).IfNone(unit).Bind(_ => map.Apply(frame, progress)))),
        };

        uint Shot(uint term, uint index) => CoordinateHash.Branch(state.Seed.Field(Stream, term), index);
    }

    private static PixelPass.Pointwise Composite(Func<long, long, (float Coverage, float Value)>[] marks, PixelExtent extent) => new((row, column, line) => {
        foreach (Func<long, long, (float Coverage, float Value)> field in marks) {
            for (int x = 0; x < row.Length; x++) {
                (float coverage, float value) = field(column + x, (long)extent.Height - 1 - line);
                row[x] = coverage switch {
                    0f => row[x],
                    1f => new Vector4(new Vector3(value), row[x].W),
                    _ => new Vector4((row[x].AsVector3() * (1f - coverage)) + new Vector3(value), row[x].W),
                };
            }
        }
    });

    private static CoordinateMap.Analytic Weave(FilmDamage state, PixelExtent extent, Clock clock, uint axes) {
        uint field = state.Seed.Field(Stream, 0u);
        LatticePoint term = new(ERational.FromSingle(clock).Multiply(ERational.FromSingle(state.WeaveRate)), ERational.Zero, ERational.Zero, ERational.Zero);
        (double X, double Y) = (
            state.WeaveX.Pixels(extent) * NoiseDimensions.One.Fbm(term, Fractal.Default.Octaves, CoordinateHash.Branch(field, axes)),
            state.WeaveY.Pixels(extent) * NoiseDimensions.One.Fbm(term, Fractal.Default.Octaves, CoordinateHash.Branch(field, axes + 1u)));
        return new((points, _, _) => { foreach (ref (double X, double Y) point in points) point = (point.X - X, point.Y - Y); }, Sampling.Linear, (WrapMode.Clamp, WrapMode.Clamp));
    }

    private static Func<long, long, (float Coverage, float Value)> Scratch(FilmDamage state, PixelExtent extent, Clock clock, uint field) {
        (ERational count, ERational drift) = (ERational.FromSingle(state.ScratchCount), ERational.FromSingle(clock).Multiply(ERational.FromSingle(state.ScratchRate)));
        (double width, uint sites, uint tones) = (state.ScratchWidth.Pixels(extent), CoordinateHash.Branch(field, 0u), CoordinateHash.Branch(field, 1u));
        return (column, _) => NoiseDimensions.PlanarF1(ERational.Create((2L * column) + 1 - extent.Width, 2L * extent.Width).Multiply(count), drift, Cellular.Default.Cells, sites) switch {
            var feature => ShapeEdge.Strip(feature.Delta.X.Abs().Divide(count).Multiply(extent.Width).ToDouble(), width, 1d) switch {
                var coverage => (coverage, CoordinateHash.Uniforms(feature.Cell.X, feature.Cell.Y, tones).X < state.Scratches.White ? coverage : 0f),
            },
        };
    }

    private static Func<long, long, (float Coverage, float Value)> Hair(FilmDamage state, PixelExtent extent, Clock clock, uint field) {
        (double pixels, double width) = (state.HairLength.Pixels(extent), state.HairWidth.Pixels(extent));
        (ERational length, ERational curl, ERational drift) =
            (ERational.FromSingle(state.HairLength).Multiply(extent.ShortSide), ERational.FromSingle(state.HairCurl), ERational.FromSingle(clock).Multiply(ERational.FromSingle(state.HairRate)));
        (double share, Octaves octaves) = ((float)state.HairCount * pixels * pixels / ((double)extent.Width * extent.Height), Fractal.Default.Octaves);
        (uint across, uint down, uint sites, uint tones, uint strands) =
            (CoordinateHash.Branch(field, 0u), CoordinateHash.Branch(field, 1u), CoordinateHash.Branch(field, 2u), CoordinateHash.Branch(field, 3u), CoordinateHash.Branch(field, 4u));
        return (column, line) => {
            LatticePoint strand = new(ERational.Create((2L * column) + 1 - extent.Width, 2).Divide(length), ERational.Create((2L * line) + 1 - extent.Height, 2).Divide(length), ERational.Zero, ERational.Zero);
            LatticePoint bent = new(strand.X.Add(curl.Multiply(ERational.FromSingle(NoiseDimensions.Two.Fbm(strand, octaves, across)))),
                strand.Y.Add(curl.Multiply(ERational.FromSingle(NoiseDimensions.Two.Fbm(strand, octaves, down)))), drift, ERational.Zero);
            (float _, float edgeDistance, LatticeCell cell) = NoiseDimensions.Three.Voronoi(bent, Cellular.Default.Cells, sites);
            Vector4 draw = cell.White(tones);
            float edge = ShapeEdge.Strip(edgeDistance * pixels, width, 1d);
            float open = ShapeEdge.Smooth(float.Clamp((0.5f + (0.5f * NoiseDimensions.Two.Fbm(strand, octaves, strands)) - 0.45f) / 0.1f, 0f, 1f));
            float coverage = draw.X < share ? edge * open : 0f;
            return (coverage, draw.Y < state.Hairs.White ? coverage : 0f);
        };
    }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FilmDamageParameter : IStateParameter<FilmDamage> {
    private static readonly Presentation<float> Width = Presentations.Length with { Soft = (0f, 0.005f * ReferenceFrame.GreaterSide), Decimals = 4 };
    private static readonly Presentation<float> Rate = Hertz.Presentation with { Soft = (0f, 24f) };
    private static readonly Presentation<float> Sway = Presentations.Length with { Soft = (0f, 0.01f) };

    public static readonly FilmDamageParameter Specks = new("specks", new StateParameter<FilmDamage>.Record<FilmSpots>(Lens<FilmDamage, FilmSpots>.New(static damage => damage.Specks, static specks => damage => damage with { Specks = specks })),
        new("Specks", "Own timing, polarity, and point field"));
    public static readonly FilmDamageParameter Dirt = new("dirt", new StateParameter<FilmDamage>.Record<FilmSpots>(Lens<FilmDamage, FilmSpots>.New(static damage => damage.Dirt, static dirt => damage => damage with { Dirt = dirt })),
        new("Dirt", "Own timing, polarity, and point field"));
    public static readonly FilmDamageParameter Blotches = new("blotches", new StateParameter<FilmDamage>.Record<FilmSpots>(Lens<FilmDamage, FilmSpots>.New(static damage => damage.Blotches, static blotches => damage => damage with { Blotches = blotches })),
        new("Blotches", "Own timing, polarity, and point field"));
    public static readonly FilmDamageParameter Scratches = new("scratches", new StateParameter<FilmDamage>.Record<DamageMark>(Lens<FilmDamage, DamageMark>.New(static damage => damage.Scratches, static scratches => damage => damage with { Scratches = scratches })),
        new("Scratches", "Mark timing and print tone"));
    public static readonly FilmDamageParameter ScratchCount = new("scratches-count", StateParameter.Bounded(Lens<FilmDamage, PrimaryScale>.New(static damage => damage.ScratchCount, static count => damage => damage with { ScratchCount = count }), new Presentation<float> { Soft = (1f, 20f) }),
        new("Scratch count", "Scratch lines across the frame width"));
    public static readonly FilmDamageParameter ScratchWidth = new("scratches-width", StateParameter.Bounded(Lens<FilmDamage, ShortSideLength>.New(static damage => damage.ScratchWidth, static width => damage => damage with { ScratchWidth = width }), Width),
        new("Scratch width", "Scratch stroke width in frame short sides"));
    public static readonly FilmDamageParameter ScratchRate = new("scratches-rate", StateParameter.Bounded(Lens<FilmDamage, Hertz>.New(static damage => damage.ScratchRate, static rate => damage => damage with { ScratchRate = rate }), Rate),
        new("Scratch rate", "Scratch field evolution per second"));
    public static readonly FilmDamageParameter Hairs = new("hairs", new StateParameter<FilmDamage>.Record<DamageMark>(Lens<FilmDamage, DamageMark>.New(static damage => damage.Hairs, static hairs => damage => damage with { Hairs = hairs })),
        new("Hairs", "Mark timing and print tone"));
    public static readonly FilmDamageParameter HairCount = new("hairs-count", StateParameter.Bounded(Lens<FilmDamage, Magnitude>.New(static damage => damage.HairCount, static count => damage => damage with { HairCount = count }), Magnitude.Presentation with { Soft = (0f, 10f) }),
        new("Hair count", "Mean hair strands per frame, fractions allowed"));
    public static readonly FilmDamageParameter HairWidth = new("hairs-width", StateParameter.Bounded(Lens<FilmDamage, ShortSideLength>.New(static damage => damage.HairWidth, static width => damage => damage with { HairWidth = width }), Width),
        new("Hair width", "Hair stroke width in frame short sides"));
    public static readonly FilmDamageParameter HairLength = new("hairs-length", StateParameter.Bounded(Lens<FilmDamage, ShortSideExtent>.New(static damage => damage.HairLength, static length => damage => damage with { HairLength = length }),
        Presentations.Extent with { Soft = (0.01f * ReferenceFrame.GreaterSide, 0.3f * ReferenceFrame.GreaterSide), Track = Presentations.Length.Track }),
        new("Hair length", "Hair cell size in frame short sides"));
    public static readonly FilmDamageParameter HairCurl = new("hairs-curl", StateParameter.Bounded(Lens<FilmDamage, NoiseDistortion>.New(static damage => damage.HairCurl, static curl => damage => damage with { HairCurl = curl }), NoiseDistortion.Presentation with { Soft = (0f, 4f) }),
        new("Hair curl", "Hair bend in hair cell sizes"));
    public static readonly FilmDamageParameter HairRate = new("hairs-rate", StateParameter.Bounded(Lens<FilmDamage, Hertz>.New(static damage => damage.HairRate, static rate => damage => damage with { HairRate = rate }), Rate),
        new("Hair rate", "Hair field evolution per second"));
    public static readonly FilmDamageParameter WeaveX = new("weave-x", StateParameter.Bounded(Lens<FilmDamage, ShortSideLength>.New(static damage => damage.WeaveX, static weave => damage => damage with { WeaveX = weave }), Sway),
        new("Weave X", "Horizontal gate weave in frame short sides"));
    public static readonly FilmDamageParameter WeaveY = new("weave-y", StateParameter.Bounded(Lens<FilmDamage, ShortSideLength>.New(static damage => damage.WeaveY, static weave => damage => damage with { WeaveY = weave }), Sway),
        new("Weave Y", "Vertical gate weave in frame short sides"));
    public static readonly FilmDamageParameter WeaveRate = new("weave-rate", StateParameter.Bounded(Lens<FilmDamage, Hertz>.New(static damage => damage.WeaveRate, static rate => damage => damage with { WeaveRate = rate }), Hertz.Presentation with { Soft = (0.1f, 20f) }),
        new("Weave rate", "Gate weave noise cells per second"));
    public static readonly FilmDamageParameter Seed = new("seed", StateParameter.Bounded(Lens<FilmDamage, Seed>.New(static damage => damage.Seed, static seed => damage => damage with { Seed = seed })),
        new("Seed", "Number selecting pattern"));
    public static readonly FilmDamageParameter Timing = new("timing", new StateParameter<FilmDamage>.Record<Timing>(Lens<FilmDamage, Timing>.New(static damage => damage.Timing, static timing => damage => damage with { Timing = timing })),
        new("Timing", "Start time and pace over recorded frames"));

    public StateParameter<FilmDamage> Kind { get; }
    public ParameterText Text { get; }
}

public sealed record FilmFlicker(Stops Depth, Hertz Rate, Seed Seed, Timing Timing) : IStateRecord<FilmFlicker, FilmFlickerParameter>, IPixelStage<FilmFlicker> {
    public static FilmFlicker Default { get; } = new(Stops.Zero, Hertz.Create(5f * 0.5f * 24f), Seed.MinValue, Timing.Default);

    public static Option<PixelPass> Pass(FilmFlicker state, PassContext context) =>
        state.Depth == Stops.Zero ? None
            : new Vector3(float.Exp2((float)state.Depth * NoiseDimensions.One.Fbm(
                new LatticePoint(ERational.FromSingle(state.Timing.At(context)).Multiply(ERational.FromSingle(state.Rate)), ERational.Zero, ERational.Zero, ERational.Zero),
                Fractal.Default.Octaves, state.Seed.Field(7u, 0u)))) switch {
                var gain => Some<PixelPass>(new PixelPass.Color(row => {
                    foreach (ref Vector4 pixel in row)
                        pixel = new Vector4(pixel.AsVector3() * gain, pixel.W);
                })),
            };
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FilmFlickerParameter : IStateParameter<FilmFlicker> {
    public static readonly FilmFlickerParameter Depth = new("depth", StateParameter.Bounded(Lens<FilmFlicker, Stops>.New(static flicker => flicker.Depth, static depth => flicker => flicker with { Depth = depth }), Stops.Presentation with { Soft = (0f, 1f) }),
        new("Depth", "Peak flicker exposure swing in stops"));
    public static readonly FilmFlickerParameter Rate = new("rate", StateParameter.Bounded(Lens<FilmFlicker, Hertz>.New(static flicker => flicker.Rate, static rate => flicker => flicker with { Rate = rate }), Hertz.Presentation with { Soft = (1f, 60f) }),
        new("Rate", "Flicker noise cells per second"));
    public static readonly FilmFlickerParameter Seed = new("seed", StateParameter.Bounded(Lens<FilmFlicker, Seed>.New(static flicker => flicker.Seed, static seed => flicker => flicker with { Seed = seed })),
        new("Seed", "Number selecting pattern"));
    public static readonly FilmFlickerParameter Timing = new("timing", new StateParameter<FilmFlicker>.Record<Timing>(Lens<FilmFlicker, Timing>.New(static flicker => flicker.Timing, static timing => flicker => flicker with { Timing = timing })),
        new("Timing", "Start time and pace over recorded frames"));

    public StateParameter<FilmFlicker> Kind { get; }
    public ParameterText Text { get; }
}
