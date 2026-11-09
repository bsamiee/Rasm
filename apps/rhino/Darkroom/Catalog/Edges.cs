using System.Numerics;
using Darkroom.Chain;
using Rasm.Imaging.Color;
using Rasm.Imaging.Filtering;
using Rasm.Imaging.Pixels;
using UnitsNet;
using Wacton.Unicolour;

namespace Darkroom.Catalog;

// --- [MODELS] --------------------------------------------------------------------------

[SmartEnum<string>]
internal sealed partial class EdgeChannel {
    public static readonly EdgeChannel Luma = new("luma", static gamut => EdgeSource.Luma(gamut.Luminance));
    public static readonly EdgeChannel Color = new("color", static _ => EdgeSource.Color);
    public static readonly EdgeChannel Hue = new("hue", static _ => EdgeSource.Hue);
    public static readonly EdgeChannel Saturation = new("saturation", static _ => EdgeSource.Saturation);
    public static readonly EdgeChannel Value = new("value", static _ => EdgeSource.Value);
    public static readonly EdgeChannel Alpha = new("alpha", static _ => EdgeSource.Alpha);

    [UseDelegateFromConstructor]
    public partial EdgeSource Plane(Gamut gamut);
}

[SmartEnum<string>]
internal sealed partial class EdgeRendering {
    public static readonly EdgeRendering Magnitude = new("magnitude", static (_, _, frame, edges, _) => Contours.Magnitudes(frame, edges));
    public static readonly EdgeRendering Lines = new("lines", static (state, context, frame, edges, progress) => state.Stroked(context, frame, edges, state.Line, 0f, progress));
    public static readonly EdgeRendering LinesOnFill = new("lines-on-fill", static (state, context, frame, edges, progress) => state.Stroked(context, frame, edges, state.Fill, 1f, progress));

    [UseDelegateFromConstructor]
    public partial Fin<Unit> Render(EdgeDetect state, PassContext context, PixelFrame frame, PixelPlane edges, Option<IProgress<int>> progress);
}

[SmartEnum<string>]
internal sealed partial class LineClass {
    public static readonly LineClass Outer = new("outer", static outline => outline.Outer, static outline => outline.Alignment);
    public static readonly LineClass Inner = new("inner", static outline => outline.Inner, static _ => 0f);

    [UseDelegateFromConstructor]
    public partial ContourStroke Stroke(GeometryOutline state);

    [UseDelegateFromConstructor]
    public partial float Alignment(GeometryOutline state);
}

[SmartEnum<string>]
internal sealed partial class OutlineSource {
    public static readonly OutlineSource Silhouette = new("silhouette", GuideChannel.Depth, static outline => outline.Silhouette,
        static (state, context) => context.Camera.Map(camera => ContourMarks.Silhouette(camera.Lens, state.Step)));
    public static readonly OutlineSource Crease = new("crease", GuideChannel.Normal, static outline => outline.Crease,
        static (state, _) => Some(ContourMarks.Crease(state.CreaseAngle.Radians.ToDouble())));
    public static readonly OutlineSource Object = new("object", GuideChannel.ObjectId, static outline => outline.Object, static (_, _) => Some(ContourMarks.Identity));
    public static readonly OutlineSource Material = new("material", GuideChannel.MaterialId, static outline => outline.Material, static (_, _) => Some(ContourMarks.Identity));

    public GuideChannel Channel { get; }

    [UseDelegateFromConstructor]
    public partial Option<LineClass> Route(GeometryOutline state);

    [UseDelegateFromConstructor]
    public partial Option<ContourMarks> Marks(GeometryOutline state, PassContext context);
}

internal sealed record EdgeDetect(
    EdgeOperator Operator, EdgeChannel Source, EdgeRendering Rendering, float Threshold, Unicolour Line,
    Unicolour Fill, BlendingMode Mode) : IPixelStage<EdgeDetect> {
    public static EdgeDetect Default { get; } = new(
        EdgeOperator.Kirsch, EdgeChannel.Luma, EdgeRendering.LinesOnFill, 0.1f, new Unicolour(ColourSpace.Rgb, 1d, 1d, 1d), new Unicolour(ColourSpace.Rgb, 0d, 0d, 0d), BlendingMode.Normal) {
        Width = ShortSideLength.Create(1f / ReferenceFrame.Height),
    };

    public Option<ShortSideLength> Softening { get; init; }
    public Option<ShortSideLength> Width { get; init; }

    public static Fin<Option<PixelPass>> Pass(EdgeDetect state, PassContext context) =>
        state.Rendering == EdgeRendering.Lines && state.Width.IsNone ? None
            : Some<PixelPass>(new PixelPass.Frame((frame, progress) => state.Operator.Detected(frame, state.Source.Plane(context.Working),
                state.Softening.Pixels(context.Extent), edges => state.Rendering.Render(state, context, frame, edges, progress))));

    public Fin<Unit> Stroked(PassContext context, PixelFrame frame, PixelPlane edges, Unicolour ground, float floor, Option<IProgress<int>> progress) =>
        Contours.Lines(frame, edges, Threshold, (float)Width.Pixels(context.Extent), RampLight.Light(Line, context).AsVector3(), RampLight.Light(ground, context).AsVector3(), floor, Mode, progress);
}

internal sealed record ContourStroke(ShortSideLength Width, Unicolour Color) {
    public static ContourStroke Default { get; } = new(ShortSideLength.Create(3f / ReferenceFrame.Height), new Unicolour(ColourSpace.Rgb, 0d, 0d, 0d));

    public Option<Length> World { get; init; }
}

internal sealed record GeometryOutline(float Step, Angle CreaseAngle, ContourStroke Outer, float Alignment, ContourStroke Inner, Seq<RampStop> TaperRamp, BlendingMode Mode)
    : IPixelStage<GeometryOutline> {
    public static GeometryOutline Default { get; } = new(
        0.05f, Angle.FromDegrees(180d - 134.4d), ContourStroke.Default, 0f, ContourStroke.Default with { Width = ShortSideLength.Create(1.5f / ReferenceFrame.Height) }, Presets.Grayscale,
        BlendingMode.Normal) {
        Silhouette = LineClass.Outer,
        Crease = LineClass.Inner,
        Object = LineClass.Outer,
    };

    public Option<LineClass> Silhouette { get; init; }
    public Option<LineClass> Crease { get; init; }
    public Option<LineClass> Object { get; init; }
    public Option<LineClass> Material { get; init; }
    public Option<Length> TaperNear { get; init; }

    public Option<Unicolour> Paper { get; init; }

    public static Seq<GuideChannel> Channels(GeometryOutline state) =>
        GuideChannel.Depth.Cons(toSeq(OutlineSource.Items).Filter(source => source.Route(state).IsSome).Map(static source => source.Channel)).Distinct();

    public static Fin<Option<PixelPass>> Pass(GeometryOutline state, PassContext context) =>
        from depth in context.Guides.Find(GuideChannel.Depth)
        let routes = toSeq(OutlineSource.Items).Choose(source => source.Route(state).Map(line => (Source: source, Line: line)))
        where !routes.IsEmpty || state.Paper.IsSome
        let taper = RampLight.Of(state.TaperRamp, context)
        let lined = routes.ToLookup(static route => route.Line)
        select (PixelPass)new PixelPass.Frame((frame, progress) =>
            from lines in toSeq(LineClass.Items).Traverse(line => line.Stroke(state) switch {
                var stroke => Contours.Width(stroke.Width.Pixels(context.Extent), stroke.World.Map(static world => world.Meters.ToDouble()), context.Camera,
                        state.TaperNear.Map(near => (near.Meters.ToDouble(), (Func<float, float>)(at => taper.Sample(at) switch { var scale => float.Max(scale.X, float.Max(scale.Y, scale.Z)) }))))
                    .Map(width => new ContourLine(
                        toSeq(lined[line]).Choose(route =>
                            from guide in context.Guides.Find(route.Source.Channel)
                            from marks in route.Source.Marks(state, context)
                            select (marks, guide)),
                        line.Alignment(state), width, RampLight.Light(stroke.Color, context).AsVector3())),
            }).As()
            from drawn in Contours.Outlined(frame, depth, lines, state.Paper.Map(paper => RampLight.Light(paper, context)), state.Mode, progress)
            select drawn);
}
