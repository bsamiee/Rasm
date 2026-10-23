using System.Drawing;
using System.Globalization;
using System.Numerics;
using System.Runtime.Intrinsics;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Rasm.Imaging.Filters.Generators;
using Rasm.Imaging.Grade;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Tone;
using SixLabors.ImageSharp.PixelFormats;
using UnitsNet;

namespace Rasm.Imaging.Filters.Stylize;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct SectorEccentricity : IMinMaxValue<SectorEccentricity> {
    public static SectorEccentricity MinValue { get; } = new(0f);
    public static SectorEccentricity MaxValue { get; } = new(2f);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class StrokePlacement : ICaptioned {
    public static readonly StrokePlacement Center = new("center", "Center", static (near, nearWidth, far, farWidth) =>
        near <= far ? ShapeEdge.Strip(near + 0.5f, nearWidth, 1f) : ShapeEdge.Strip(far + 0.5f, farWidth, 1f));
    public static readonly StrokePlacement Inside = new("inside", "Inside", static (near, nearWidth, far, _) =>
        (float)double.Clamp(nearWidth - near, 0d, 1d) * ShapeEdge.Coverage((near - far) / 2d, 1d));
    public static readonly StrokePlacement Outside = new("outside", "Outside", static (near, _, far, farWidth) =>
        (float)double.Clamp(farWidth - far, 0d, 1d) * ShapeEdge.Coverage((far - near) / 2d, 1d));

    public string Caption { get; }

    [UseDelegateFromConstructor]
    public partial float Cover(double near, double nearWidth, double far, double farWidth);
}

public sealed record Stroke(ShortSideLength Width, Gated<Length> World, Swatch Color) : IStateRecord<Stroke, StrokeParameter> {
    public static Stroke Default { get; } = new(ShortSideLength.Create(3f / ReferenceFrame.Height), new(Enabled: false, Length.FromInches(0.25m)), Swatch.Black);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class StrokeParameter : IStateParameter<Stroke> {
    public static readonly StrokeParameter Width = new("width", StateParameter.Bounded(Lens<Stroke, ShortSideLength>.New(static stroke => stroke.Width, static width => stroke => stroke with { Width = width }), ShortSideLength.Presentation with { Soft = (0f, 0.01f) }),
        new("Width", "Line width in frame short sides, capping world width"));
    public static readonly StrokeParameter World = new("world", new StateParameter<Stroke>.OptionalNumeric<Length, float>(
        Lens<Stroke, Gated<Length>>.New(static stroke => stroke.World, static world => stroke => stroke with { World = world }),
        NumericDomain.Meters((0f, float.MaxValue), new() { Soft = (0f, (float)Length.FromInches(1).Meters), Origin = (float)Stroke.Default.World.Value.Meters })),
        new("World width", "Surface line width, narrowing with distance"));
    public static readonly StrokeParameter Color = new("color", new StateParameter<Stroke>.Color(Lens<Stroke, Swatch>.New(static stroke => stroke.Color, static color => stroke => stroke with { Color = color })),
        new("Color", "Line color"));

    public StateParameter<Stroke> Kind { get; }
    public ParameterText Text { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class LineClass : ICaptioned {
    public static readonly LineClass Outer = new(
        "outer", Lens<GeometryOutline, Stroke>.New(static outline => outline.Outer, static stroke => outline => outline with { Outer = stroke }), "Outer", static outline => outline.Placement);
    public static readonly LineClass Inner = new(
        "inner", Lens<GeometryOutline, Stroke>.New(static outline => outline.Inner, static stroke => outline => outline with { Inner = stroke }), "Inner", static _ => StrokePlacement.Center);

    public Lens<GeometryOutline, Stroke> Stroke { get; }
    public string Caption { get; }

    [UseDelegateFromConstructor]
    public partial StrokePlacement Placement(GeometryOutline state);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class OutlineSource {
    public static readonly OutlineSource Silhouette = new(
        "silhouette",
        Lens<GeometryOutline, Option<LineClass>>.New(static outline => outline.Silhouette, static line => outline => outline with { Silhouette = line }),
        GuideChannel.Depth,
        static (state, context) => context.Camera.Map<Func<Mat, Rectangle, Rectangle, Mat, Fin<Unit>>>(camera => (guide, first, second, edges) => {
            using Mat z = new();
            using Mat curvature = new(guide.Size, DepthType.Cv64F, 1);
            using Mat step = new();
            using Mat marks = new();
            using Mat taps = new(1 + (2 * (second.Y - first.Y)), 1 + (2 * (second.X - first.X)), DepthType.Cv32F, 1);
            using ScalarArray zero = new(0d);
            using ScalarArray ratio = new(state.Step);
            taps.SetTo([1f, -2f, 1f]);
            CvInvoke.ExtractChannel(guide, z, 0);
            z.ConvertTo(z, DepthType.Cv64F);
            camera.Frustum.Switch(z,
                perspective: static (depth, _) => {
                    using ScalarArray one = new(1d);
                    CvInvoke.Divide(one, depth, depth);
                },
                parallel: static (depth, _) => {
                    using ScalarArray infinity = new(float.PositiveInfinity);
                    using Mat background = new();
                    CvInvoke.Compare(depth, infinity, background, CmpType.Equal);
                    depth.SetTo(new MCvScalar(0d), background);
                });
            CvInvoke.Filter2D(z, curvature, taps, new Point(-1, -1), 0d, BorderType.Replicate);
            CvInvoke.AbsDiff(curvature, zero, step);
            CvInvoke.Multiply(z, ratio, z);
            CvInvoke.Compare(step, z, marks, CmpType.GreaterThan);
            using Mat leading = new(marks, first);
            using Mat trailing = new(marks, second);
            CvInvoke.BitwiseAnd(leading, trailing, edges);
            return unit;
        }));
    public static readonly OutlineSource Crease = new(
        "crease",
        Lens<GeometryOutline, Option<LineClass>>.New(static outline => outline.Crease, static line => outline => outline with { Crease = line }),
        GuideChannel.Normal,
        static (state, _) => Some<Func<Mat, Rectangle, Rectangle, Mat, Fin<Unit>>>((guide, firstBox, secondBox, edges) => {
            using Mat first = new(guide, firstBox);
            using Mat second = new(guide, secondBox);
            first.ConvertTo(first, DepthType.Cv64F);
            second.ConvertTo(second, DepthType.Cv64F);
            using Mat rgb = ChannelMask.Rgb();
            using Mat product = new();
            using Mat dot = new();
            using Mat lengths = new();
            using Mat partner = new();
            using ScalarArray bend = new(double.Cos(state.CreaseAngle));
            CvInvoke.Multiply(first, second, product);
            CvInvoke.Transform(product, dot, rgb);
            CvInvoke.Multiply(first, first, product);
            CvInvoke.Transform(product, lengths, rgb);
            CvInvoke.Multiply(second, second, product);
            CvInvoke.Transform(product, partner, rgb);
            CvInvoke.Multiply(lengths, partner, lengths);
            CvInvoke.Sqrt(lengths, lengths);
            CvInvoke.Multiply(lengths, bend, lengths);
            CvInvoke.Compare(dot, lengths, edges, CmpType.LessThan);
            return unit;
        }));
    public static readonly OutlineSource Object = new(
        "object",
        Lens<GeometryOutline, Option<LineClass>>.New(static outline => outline.Object, static line => outline => outline with { Object = line }),
        GuideChannel.ObjectId,
        static (_, _) => Some(Identities));
    public static readonly OutlineSource Material = new(
        "material",
        Lens<GeometryOutline, Option<LineClass>>.New(static outline => outline.Material, static line => outline => outline with { Material = line }),
        GuideChannel.MaterialId,
        static (_, _) => Some(Identities));

    public Lens<GeometryOutline, Option<LineClass>> Route { get; }
    public GuideChannel Channel { get; }

    [UseDelegateFromConstructor]
    public partial Option<Func<Mat, Rectangle, Rectangle, Mat, Fin<Unit>>> Edges(GeometryOutline state, PassContext context);

    private static Fin<Unit> Identities(Mat guide, Rectangle firstBox, Rectangle secondBox, Mat edges) {
        using Mat first = new(guide, firstBox);
        using Mat second = new(guide, secondBox);
        using Mat leading = new();
        using Mat trailing = new();
        CvInvoke.ExtractChannel(first, leading, 0);
        CvInvoke.ExtractChannel(second, trailing, 0);
        CvInvoke.Compare(leading, trailing, edges, CmpType.NotEqual);
        return unit;
    }
}

public sealed record GeometryOutline(
    Option<LineClass> Silhouette, Option<LineClass> Crease, Option<LineClass> Object, Option<LineClass> Material,
    AxisFraction Step, SignedAngle CreaseAngle,
    Stroke Outer, StrokePlacement Placement, Stroke Inner, Gated<Length> TaperNear, Ramp TaperRamp, Gated<Swatch> Paper, BlendingMode Mode)
    : IStateRecord<GeometryOutline, GeometryOutlineParameter>, IPixelStage<GeometryOutline> {
    public static GeometryOutline Default { get; } = new(
        Some(LineClass.Outer), Some(LineClass.Inner), Some(LineClass.Outer), None,
        AxisFraction.Create(0.05f), SignedAngle.Create(float.DegreesToRadians(180f - 134.4f)),
        Stroke.Default, StrokePlacement.Center, Stroke.Default with { Width = ShortSideLength.Create(1.5f / ReferenceFrame.Height) },
        new(Enabled: false, Length.FromFeet(10)), Ramp.Grayscale, new(Enabled: false, Swatch.Black), BlendingMode.Mix);

    public static Seq<GuideChannel> Channels(GeometryOutline state) =>
        GuideChannel.Depth.Cons(toSeq(OutlineSource.Items).Filter(source => source.Route.Get(state).IsSome).Map(static source => source.Channel)).Distinct();

    public static Option<PixelPass> Pass(GeometryOutline state, PassContext context) =>
        from depth in context.Guides.Find(GuideChannel.Depth)
        let routes = toSeq(OutlineSource.Items).Map(source => source.Route.Get(state).Map(line => (Source: source, Line: line))).Somes()
        where !routes.IsEmpty || state.Paper.Enabled
        select (PixelPass)new PixelPass.Frame((frame, progress) => Outlined(state, context, depth, routes, frame, progress));

    private static unsafe Fin<Unit> Outlined(
        GeometryOutline state, PassContext context, PixelFrame depth, Seq<(OutlineSource Source, LineClass Line)> routes, PixelFrame frame, IProgress<int> progress) => frame.Filter(_ => {
            Vector3[] colors = [.. toSeq(LineClass.Items).Map(line => line.Stroke.Get(state).Color.Display.AsVector3())];
            using Mat range = depth.Header();
            using Mat z = new();
            using Mat cover = Mat.Zeros(frame.Size.Height, frame.Size.Width, DepthType.Cv32F, colors.Length);
            using Mat unclaimed = new(frame.Size.Height, frame.Size.Width, DepthType.Cv8U, 1);
            unclaimed.SetTo(new MCvScalar(byte.MaxValue));
            CvInvoke.ExtractChannel(range, z, 0);
            Seq<(LineClass Line, Func<Mat, Rectangle, Rectangle, Mat, Fin<Unit>> Edges, PixelFrame Guide)> available = routes.Map(route =>
                from guide in context.Guides.Find(route.Source.Channel)
                from edges in route.Source.Edges(state, context)
                select (route.Line, edges, guide)).Somes();
            return toSeq(LineClass.Items.GroupJoin(available, static line => line, static route => route.Line,
                    static (line, routes) => (Line: line, Sources: toSeq(routes).Map(static route => (route.Edges, route.Guide)))))
                .Map(static (stroke, channel) => (stroke.Line, Channel: channel, stroke.Sources))
                .Filter(static stroke => !stroke.Sources.IsEmpty)
                .TraverseM(stroke => Width(state, context, stroke.Line).Bind(width => Covered(state, z, stroke.Sources, stroke.Line, width, stroke.Channel, cover, unclaimed, frame, progress)))
                .As()
                .Bind(_ => new PixelPass.Pointwise((row, _, line) => {
                    using SpanOwner<Vector4> layer = SpanOwner<Vector4>.Allocate(row.Length);
                    using SpanOwner<float> weights = SpanOwner<float>.Allocate(row.Length);
                    weights.Span.Fill(1f);
                    ReadOnlySpan<float> covers = new(cover.GetDataPointer(frame.RowIndex(line)).ToPointer(), row.Length * colors.Length);
                    if (state.Paper.Enabled)
                        row.Fill(state.Paper.Value.Display);
                    for (int x = 0; x < row.Length; x++) {
                        (Vector3 color, float weight) = (Vector3.Zero, 0f);
                        for (int k = 0; k < colors.Length; k++)
                            (color, weight) = (covers[(x * colors.Length) + k] * (1f - weight)) switch { var c => (color + (c * colors[k]), weight + c) };
                        layer.Span[x] = new Vector4(color, weight);
                    }
                    state.Mode.Composite(row, layer.Span, weights.Span, PixelAlphaRepresentation.Unassociated, PixelAlphaRepresentation.Associated);
                }).Run(frame, progress));
        });

    private static unsafe Fin<Unit> Covered(
        GeometryOutline state, Mat z, Seq<(Func<Mat, Rectangle, Rectangle, Mat, Fin<Unit>> Edges, PixelFrame Guide)> sources, LineClass line, Func<float, double> stroke, int channel, Mat cover, Mat unclaimed, PixelFrame frame, IProgress<int> progress) {
        (int width, int height, int classes) = (z.Cols, z.Rows, cover.NumberOfChannels);
        StrokePlacement placement = line.Placement(state);
        using Mat near = Mat.Zeros(height, width, DepthType.Cv8U, 1);
        using Mat far = Mat.Zeros(height, width, DepthType.Cv8U, 1);
        using Mat contour = new(height, width, DepthType.Cv32F, 1);
        using Mat nearReach = new();
        using Mat farReach = new();
        using Mat nearLabels = new();
        using Mat farLabels = new();
        contour.SetTo(new MCvScalar(float.PositiveInfinity));
        return sources.TraverseM(source => Marked(source.Edges, source.Guide, z, near, far, contour)).As().Bind(_ => {
            CvInvoke.BitwiseAnd(near, unclaimed, near);
            CvInvoke.BitwiseAnd(far, unclaimed, far);
            using Mat claimed = new();
            CvInvoke.BitwiseOr(near, far, claimed);
            CvInvoke.BitwiseNot(claimed, unclaimed);
            using Mat nearWidths = Reached(near, contour, stroke, nearReach, nearLabels);
            using Mat farWidths = Reached(far, contour, stroke, farReach, farLabels);
            return new PixelPass.Pointwise((_, _, at) => {
                int row = frame.RowIndex(at);
                ReadOnlySpan<float> nearDistances = new(nearReach.GetDataPointer(row).ToPointer(), width);
                ReadOnlySpan<float> farDistances = new(farReach.GetDataPointer(row).ToPointer(), width);
                ReadOnlySpan<int> nearLines = new(nearLabels.GetDataPointer(row).ToPointer(), width);
                ReadOnlySpan<int> farLines = new(farLabels.GetDataPointer(row).ToPointer(), width);
                ReadOnlySpan<double> nearStrokes = new(nearWidths.DataPointer.ToPointer(), nearWidths.Cols);
                ReadOnlySpan<double> farStrokes = new(farWidths.DataPointer.ToPointer(), farWidths.Cols);
                Span<float> covers = new(cover.GetDataPointer(row).ToPointer(), width * classes);
                for (int x = 0; x < width; x++)
                    covers[(x * classes) + channel] = placement.Cover(nearDistances[x], nearStrokes[nearLines[x]], farDistances[x], farStrokes[farLines[x]]);
            }).Run(frame, progress);
        });
    }

    private static Fin<Unit> Marked(Func<Mat, Rectangle, Rectangle, Mat, Fin<Unit>> mark, PixelFrame guide, Mat z, Mat near, Mat far, Mat contour) {
        using Mat header = guide.Header();
        using Mat edges = new();
        using Mat side = new();
        using Mat nearer = new();
        using Mat lowered = new();
        return Seq(
                (new Rectangle(0, 0, z.Cols - 1, z.Rows), new Rectangle(1, 0, z.Cols - 1, z.Rows)),
                (new Rectangle(0, 0, z.Cols, z.Rows - 1), new Rectangle(0, 1, z.Cols, z.Rows - 1))).Filter(static pair => pair.Item1.Width > 0 && pair.Item1.Height > 0).TraverseM(pair => {
                    (Rectangle first, Rectangle second) = pair;
                    using Mat firstDepth = new(z, first);
                    using Mat secondDepth = new(z, second);
                    return mark(header, first, second, edges).Map(_ => {
                        CvInvoke.Min(firstDepth, secondDepth, nearer);
                        foreach ((Rectangle box, Mat own, Mat partner) in Seq((first, firstDepth, secondDepth), (second, secondDepth, firstDepth))) {
                            foreach ((CmpType test, Mat mask) in Seq((CmpType.LessThan, near), (CmpType.GreaterEqual, far))) {
                                using Mat marks = new(mask, box);
                                CvInvoke.Compare(own, partner, side, test);
                                CvInvoke.BitwiseAnd(edges, side, side);
                                CvInvoke.BitwiseOr(marks, side, marks);
                            }
                            using Mat depth = new(contour, box);
                            CvInvoke.Min(depth, nearer, lowered);
                            lowered.CopyTo(depth, edges);
                        }
                        return unit;
                    });
                }).As().Map(static _ => unit);
    }

    private static unsafe Mat Reached(Mat mask, Mat contour, Func<float, double> stroke, Mat reach, Mat labels) {
        Mat widths = Mat.Zeros(1, CvInvoke.CountNonZero(mask) + 1, DepthType.Cv64F, 1);
        CvInvoke.BitwiseNot(mask, mask);
        CvInvoke.DistanceTransform(mask, reach, labels, DistType.L2, 5, DistLabelType.Pixel);
        Span<double> table = new(widths.DataPointer.ToPointer(), widths.Cols);
        ReadOnlySpan<byte> marks = new(mask.DataPointer.ToPointer(), mask.Rows * mask.Cols);
        ReadOnlySpan<float> depths = new(contour.DataPointer.ToPointer(), contour.Rows * contour.Cols);
        for (int pixel = 0, label = 1; pixel < marks.Length; pixel++) {
            if (marks[pixel] is 0)
                table[label++] = stroke(depths[pixel]);
        }
        return widths;
    }

    private static Fin<Func<float, double>> Width(GeometryOutline state, PassContext context, LineClass line) {
        Stroke stroke = line.Stroke.Get(state);
        double frame = stroke.Width.Pixels(context.Extent);
        RampTable ramp = state.TaperRamp.Tabulate(context.Working);
        Fin<Func<float, double>> reach = stroke.World.Active.Match(
            world => world == Length.Zero || frame is 0d ? Fin.Succ<Func<float, double>>(static _ => 0d) : context.Camera.ToFin(Error.New("world width needs frame camera")).Map(camera =>
                camera.Frustum.Switch(
                    (Frame: frame, Span: (double)world.Meters * context.Extent.Width / camera.Frustum.Window.Width),
                    perspective: static (scale, _) => (Func<float, double>)(depth => double.Min(scale.Span / depth, scale.Frame)),
                    parallel: static (scale, _) => _ => double.Min(scale.Span, scale.Frame))),
            () => Fin.Succ<Func<float, double>>(_ => frame));
        return reach.Map(width => state.TaperNear.Active.Match(
            near => depth => ramp.Sample((float)double.Clamp((double)near.Meters / depth, 0d, 1d)) switch { var color => width(depth) * float.Max(color.X, float.Max(color.Y, color.Z)) },
            () => width));
    }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class GeometryOutlineParameter : IStateParameter<GeometryOutline> {
    private static readonly NumericDomain<Length, float> Distance = NumericDomain.Meters((0f, float.MaxValue), new() {
        Soft = (0f, (float)Length.FromFeet(1000).Meters), Origin = (float)GeometryOutline.Default.TaperNear.Value.Meters,
    });

    public static readonly GeometryOutlineParameter Silhouette = new("silhouette-lines", new StateParameter<GeometryOutline>.OptionalChoice<LineClass>(OutlineSource.Silhouette.Route, "Off"),
        new("Silhouettes", "Lines at depth steps where one surface occludes another"));
    public static readonly GeometryOutlineParameter Crease = new("crease-lines", new StateParameter<GeometryOutline>.OptionalChoice<LineClass>(OutlineSource.Crease.Route, "Off"),
        new("Creases", "Lines where neighboring normals turn past the crease angle"));
    public static readonly GeometryOutlineParameter Object = new("object-lines", new StateParameter<GeometryOutline>.OptionalChoice<LineClass>(OutlineSource.Object.Route, "Off"),
        new("Object edges", "Lines where object IDs change"));
    public static readonly GeometryOutlineParameter Material = new("material-lines", new StateParameter<GeometryOutline>.OptionalChoice<LineClass>(OutlineSource.Material.Route, "Off"),
        new("Material edges", "Lines where material IDs change"));
    public static readonly GeometryOutlineParameter Step = new("depth-step", StateParameter.Bounded(Lens<GeometryOutline, AxisFraction>.New(static outline => outline.Step, static step => outline => outline with { Step = step }), AxisFraction.Presentation with { Soft = (0f, 0.2f) }),
        new("Depth threshold", "Silhouette depth curvature, 1 minus near over far depth across flat regions"));
    public static readonly GeometryOutlineParameter CreaseAngle = new("crease-angle", StateParameter.Bounded(Lens<GeometryOutline, SignedAngle>.New(static outline => outline.CreaseAngle, static angle => outline => outline with { CreaseAngle = angle }), SignedAngle.Presentation with { Soft = (0f, SignedAngle.Up) }),
        new("Crease angle", "Turn between neighboring normals marking a crease"));
    public static readonly GeometryOutlineParameter Outer = new("outer", new StateParameter<GeometryOutline>.Record<Stroke>(LineClass.Outer.Stroke),
        new("Outer lines", "Width and color of outer lines"));
    public static readonly GeometryOutlineParameter Placement = new("placement", new StateParameter<GeometryOutline>.Choice<StrokePlacement>(Lens<GeometryOutline, StrokePlacement>.New(static outline => outline.Placement, static placement => outline => outline with { Placement = placement })),
        new("Placement", "Contour side outer lines draw on"));
    public static readonly GeometryOutlineParameter Inner = new("inner", new StateParameter<GeometryOutline>.Record<Stroke>(LineClass.Inner.Stroke),
        new("Inner lines", "Width and color of inner lines"));
    public static readonly GeometryOutlineParameter TaperNear = new("taper-near", new StateParameter<GeometryOutline>.OptionalNumeric<Length, float>(Lens<GeometryOutline, Gated<Length>>.New(static outline => outline.TaperNear, static depth => outline => outline with { TaperNear = depth }), Distance),
        new("Taper near", "Near depth divided by contour depth places width on ramp, off keeping full width"));
    public static readonly GeometryOutlineParameter TaperRamp = new("taper-ramp", new StateParameter<GeometryOutline>.Gradient(Lens<GeometryOutline, Ramp>.New(static outline => outline.TaperRamp, static ramp => outline => outline with { TaperRamp = ramp })),
        new("Taper ramp", "Line width scale from distant contours at left to near contours at right"));
    public static readonly GeometryOutlineParameter Paper = new("paper", new StateParameter<GeometryOutline>.OptionalColor(Lens<GeometryOutline, Gated<Swatch>>.New(static outline => outline.Paper, static paper => outline => outline with { Paper = paper })),
        new("Paper", "Color under lines, off drawing over image"));
    public static readonly GeometryOutlineParameter Mode = new("mode", new StateParameter<GeometryOutline>.Choice<BlendingMode>(Lens<GeometryOutline, BlendingMode>.New(static outline => outline.Mode, static mode => outline => outline with { Mode = mode })),
        new("Blend mode", "Drawn layer blend over frame"));

    public StateParameter<GeometryOutline> Kind { get; }
    public ParameterText Text { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public abstract partial class EdgeOperator : ICaptioned {
    public static readonly EdgeOperator Laplace = new Gradient("laplace", Seq(-1f / 8f, -1f / 8f, -1f / 8f, -1f / 8f, 1f, -1f / 8f, -1f / 8f, -1f / 8f, -1f / 8f), "Laplace");
    public static readonly EdgeOperator Sobel = new Gradient("sobel", Seq(1f, 0f, -1f, 2f, 0f, -2f, 1f, 0f, -1f), "Sobel");
    public static readonly EdgeOperator Prewitt = new Gradient("prewitt", Seq(1f, 0f, -1f, 1f, 0f, -1f, 1f, 0f, -1f), "Prewitt");
    public static readonly EdgeOperator Kirsch = new Gradient("kirsch", Seq(5f, -3f, -2f, 5f, -3f, -2f, 5f, -3f, -2f), "Kirsch");
    public static readonly EdgeOperator Emboss = new Relief("emboss", Seq(1f, 2f, 1f, 0f, 1f, 0f, -1f, -2f, -1f), "Emboss");

    public Seq<float> Kernel { get; }
    public string Caption { get; }

    public abstract float Gain { get; }

    private protected Seq<(float X, float Y)> Steps =>
        from axis in Seq<Func<int, int>>(static cell => cell / 3, static cell => cell % 3)
        from split in Seq(1, 2)
        from side in Seq<Func<int, int, bool>>(static (at, edge) => at < edge, static (at, edge) => at >= edge)
        let lit = toSeq(Range(0, 9)).Filter(cell => side(axis(cell), split))
        select (lit.Fold(0f, (sum, cell) => sum + Kernel[cell]), lit.Fold(0f, (sum, cell) => sum + Kernel[(3 * (cell % 3)) + (cell / 3)]));

    public abstract Unit Respond(Mat plane, Mat response);

    private protected Mat Across() => Taps(static (row, column) => (3 * (2 - row)) + column);

    private protected Mat Down() => Taps(static (row, column) => (3 * column) + 2 - row);

    private Mat Taps(Func<int, int, int> cell) {
        Mat taps = new(3, 3, DepthType.Cv32F, 1);
        taps.SetTo([.. toSeq(Range(0, 9)).Map(index => Kernel[cell(index / 3, index % 3)])]);
        return taps;
    }

    private sealed class Gradient(string key, Seq<float> kernel, string caption) : EdgeOperator(key, kernel, caption) {
        public override float Gain => Steps.Fold(0f, static (gain, step) => float.Max(gain, float.Hypot(step.X, step.Y)));

        public override Unit Respond(Mat plane, Mat response) {
            using Mat across = Across();
            using Mat down = Down();
            using Mat vertical = new(plane.Size, plane.Depth, plane.NumberOfChannels);
            CvInvoke.Filter2D(plane, response, across, new Point(-1, -1), 0d, BorderType.Replicate);
            CvInvoke.Filter2D(plane, vertical, down, new Point(-1, -1), 0d, BorderType.Replicate);
            CvInvoke.Multiply(response, response, response);
            CvInvoke.Multiply(vertical, vertical, vertical);
            CvInvoke.Add(response, vertical, response);
            CvInvoke.Sqrt(response, response);
            return unit;
        }
    }

    private sealed class Relief(string key, Seq<float> kernel, string caption) : EdgeOperator(key, kernel, caption) {
        public override float Gain => Steps.Fold(0f, static (gain, step) => float.Max(gain, step.X));

        public override Unit Respond(Mat plane, Mat response) {
            using Mat across = Across();
            using ScalarArray floor = new(0d);
            CvInvoke.Filter2D(plane, response, across, new Point(-1, -1), 0d, BorderType.Replicate);
            CvInvoke.Max(response, floor, response);
            return unit;
        }
    }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class EdgeSource : ICaptioned {
    public static readonly EdgeSource Color = new("color", 3, separate: true, "Color", static (row, plane, _) => {
        for (int x = 0; x < row.Length; x++)
            (plane[3 * x], plane[(3 * x) + 1], plane[(3 * x) + 2]) = (row[x].X, row[x].Y, row[x].Z);
    });
    public static readonly EdgeSource Luma = new("luma", 1, separate: false, "Luma", static (row, plane, weights) => {
        for (int x = 0; x < row.Length; x++)
            plane[x] = LuminanceSamples.Luminance(row[x].X, row[x].Y, row[x].Z, weights);
    });
    public static readonly EdgeSource Hue = new("hue", 2, separate: false, "Hue", static (row, plane, _) => {
        for (int x = 0; x < row.Length; x++)
            (plane[2 * x], plane[(2 * x) + 1]) = double.SinCos(Hsy.From(row[x]).Hue) switch { var (sine, cosine) => (cosine / double.Tau, sine / double.Tau) };
    });
    public static readonly EdgeSource Saturation = new("saturation", 1, separate: false, "Saturation", static (row, plane, _) => {
        for (int x = 0; x < row.Length; x++)
            plane[x] = RangeAxis.Saturation.Coordinate(Hsy.From(row[x]));
    });
    public static readonly EdgeSource Value = new("value", 1, separate: false, "Value", static (row, plane, _) => {
        for (int x = 0; x < row.Length; x++)
            plane[x] = float.Max(float.Max(row[x].X, row[x].Y), row[x].Z);
    });
    public static readonly EdgeSource Alpha = new("alpha", 1, separate: false, "Alpha", static (row, plane, _) => {
        for (int x = 0; x < row.Length; x++)
            plane[x] = row[x].W;
    });

    public int Planes { get; }
    public bool Separate { get; }
    public string Caption { get; }

    [UseDelegateFromConstructor]
    public partial void Read(ReadOnlySpan<Vector4> row, Span<double> plane, (double X, double Y, double Z) weights);
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

public sealed record EdgeDetect(
    EdgeOperator Operator, EdgeSource Source, ShortSideLength Softening, EdgeRendering Rendering,
    AxisFraction Threshold, ShortSideLength Width, Swatch Line, Swatch Fill, BlendingMode Mode)
    : IStateRecord<EdgeDetect, EdgeDetectParameter>, IPixelStage<EdgeDetect> {
    public static EdgeDetect Default { get; } = new(
        EdgeOperator.Kirsch, EdgeSource.Luma, ShortSideLength.Neutral, EdgeRendering.LinesOnFill,
        AxisFraction.Create(0.1f), ShortSideLength.Create(1f / ReferenceFrame.Height), Swatch.White, Swatch.Black, BlendingMode.Mix);

    public static Option<PixelPass> Pass(EdgeDetect state, PassContext context) =>
        state.Rendering.Map(magnitude: false, lines: state.Width == ShortSideLength.Neutral, linesOnFill: false)
            ? None
            : Some<PixelPass>(new PixelPass.Frame((frame, progress) => Detected(state, context, frame, progress)));

    private static unsafe Fin<Unit> Detected(EdgeDetect state, PassContext context, PixelFrame frame, IProgress<int> progress) => frame.Filter(_ => {
        (int width, int planes) = (frame.Size.Width, state.Source.Planes);
        using Mat plane = new(frame.Size.Height, width, DepthType.Cv64F, planes);
        using Mat response = new(frame.Size.Height, width, DepthType.Cv64F, planes);
        using Mat squares = new();
        return new PixelPass.Pointwise((row, _, line) =>
                state.Source.Read(row, new Span<double>(plane.GetDataPointer(frame.RowIndex(line)).ToPointer(), width * planes), context.Working.Luminance))
            .Run(frame, progress)
            .Bind(_ => plane.Blur(plane, 3d * state.Softening.Pixels(context.Extent)))
            .Map(_ => state.Operator.Respond(plane, response))
            .Bind(_ => {
                if (!state.Source.Separate) {
                    using Mat sums = Mat.Ones(1, planes, DepthType.Cv32F, 1);
                    CvInvoke.Multiply(response, response, response);
                    CvInvoke.Transform(response, squares, sums);
                    CvInvoke.Sqrt(squares, response);
                }
                return state.Rendering.Switch(
                    (State: state, Context: context, Frame: frame, Edges: response, Progress: progress),
                    magnitude: static run => Magnitudes(run.Frame, run.Edges, run.Progress),
                    lines: static run => Stroked(run.State, run.Context, run.Frame, run.Edges, run.State.Line, 0f, run.Progress),
                    linesOnFill: static run => Stroked(run.State, run.Context, run.Frame, run.Edges, run.State.Fill, 1f, run.Progress));
            });
    });

    private static unsafe Fin<Unit> Magnitudes(PixelFrame frame, Mat edges, IProgress<int> progress) =>
        new PixelPass.Pointwise((row, _, line) => {
            int channels = edges.NumberOfChannels;
            ReadOnlySpan<double> responses = new(edges.GetDataPointer(frame.RowIndex(line)).ToPointer(), channels * row.Length);
            for (int x = 0; x < row.Length; x++)
                row[x] = new Vector4(channels is 1 ? new Vector3((float)responses[x]) : new Vector3((float)responses[3 * x], (float)responses[(3 * x) + 1], (float)responses[(3 * x) + 2]), row[x].W);
        }).Run(frame, progress);

    private static unsafe Fin<Unit> Stroked(EdgeDetect state, PassContext context, PixelFrame frame, Mat edges, Swatch ground, float floor, IProgress<int> progress) {
        (Vector3 line, Vector3 under, double stroke) = (state.Line.Display.AsVector3(), ground.Display.AsVector3(), state.Width.Pixels(context.Extent));
        using Mat value = new();
        using Mat mask = new();
        using Mat reach = new();
        using ScalarArray threshold = new(state.Threshold * state.Operator.Gain);
        if (state.Source.Separate) {
            (double X, double Y, double Z) = context.Working.Luminance;
            using Mat weights = new(1, state.Source.Planes, DepthType.Cv64F, 1);
            weights.SetTo([X, Y, Z]);
            CvInvoke.Transform(edges, value, weights);
        } else
            CvInvoke.ExtractChannel(edges, value, 0);
        CvInvoke.Compare(value, threshold, mask, CmpType.GreaterThan);
        CvInvoke.BitwiseNot(mask, mask);
        CvInvoke.DistanceTransform(mask, reach, labels: null, DistType.L2, 5, DistLabelType.Pixel);
        return new PixelPass.Pointwise((row, _, at) => {
            using SpanOwner<Vector4> layer = SpanOwner<Vector4>.Allocate(row.Length);
            using SpanOwner<float> cover = SpanOwner<float>.Allocate(row.Length);
            ReadOnlySpan<float> distances = new(reach.GetDataPointer(frame.RowIndex(at)).ToPointer(), row.Length);
            for (int x = 0; x < row.Length; x++) {
                float coverage = ShapeEdge.Strip(distances[x] + 0.5f, stroke, 1f);
                layer.Span[x] = new Vector4(Vector3.Lerp(under, line, coverage), 1f);
                cover.Span[x] = float.Max(coverage, floor);
            }
            state.Mode.Composite(row, layer.Span, cover.Span, PixelAlphaRepresentation.Unassociated);
        }).Run(frame, progress);
    }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class EdgeDetectParameter : IStateParameter<EdgeDetect> {
    private static readonly Presentation<float> Span = ShortSideLength.Presentation with { Soft = (0f, 0.01f) };

    public static readonly EdgeDetectParameter Operator = new("operator", new StateParameter<EdgeDetect>.Choice<EdgeOperator>(Lens<EdgeDetect, EdgeOperator>.New(static detect => detect.Operator, static kernel => detect => detect with { Operator = kernel })),
        new("Type", "Kernel measuring local change"));
    public static readonly EdgeDetectParameter Source = new("source", new StateParameter<EdgeDetect>.Choice<EdgeSource>(Lens<EdgeDetect, EdgeSource>.New(static detect => detect.Source, static source => detect => detect with { Source = source })),
        new("Channel", "Color or one channel the kernel reads"));
    public static readonly EdgeDetectParameter Softening = new("softening", StateParameter.Bounded(Lens<EdgeDetect, ShortSideLength>.New(static detect => detect.Softening, static softening => detect => detect with { Softening = softening }), Span),
        new("Softening", "Blur before kernel in frame short sides"));
    public static readonly EdgeDetectParameter Rendering = new("rendering", new StateParameter<EdgeDetect>.Choice<EdgeRendering>(Lens<EdgeDetect, EdgeRendering>.New(static detect => detect.Rendering, static rendering => detect => detect with { Rendering = rendering })),
        new("Output", "Response, lines, or lines on fill"));
    public static readonly EdgeDetectParameter Threshold = new("threshold", StateParameter.Bounded(Lens<EdgeDetect, AxisFraction>.New(static detect => detect.Threshold, static threshold => detect => detect with { Threshold = threshold })),
        new("Threshold", "Kernel step response share a line pixel exceeds"));
    public static readonly EdgeDetectParameter Width = new("width", StateParameter.Bounded(Lens<EdgeDetect, ShortSideLength>.New(static detect => detect.Width, static width => detect => detect with { Width = width }), Span),
        new("Line width", "Stroke width in frame short sides"));
    public static readonly EdgeDetectParameter Line = new("line-color", new StateParameter<EdgeDetect>.Color(Lens<EdgeDetect, Swatch>.New(static detect => detect.Line, static line => detect => detect with { Line = line })),
        new("Line color", "Drawn line color"));
    public static readonly EdgeDetectParameter Fill = new("fill-color", new StateParameter<EdgeDetect>.Color(Lens<EdgeDetect, Swatch>.New(static detect => detect.Fill, static fill => detect => detect with { Fill = fill })),
        new("Fill color", "Color under lines on fill"));
    public static readonly EdgeDetectParameter Mode = new("mode", new StateParameter<EdgeDetect>.Choice<BlendingMode>(Lens<EdgeDetect, BlendingMode>.New(static detect => detect.Mode, static mode => detect => detect with { Mode = mode })),
        new("Blend mode", "Drawn layer blend over frame"));

    public StateParameter<EdgeDetect> Kind { get; }
    public ParameterText Text { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class KuwaharaKind : ICaptioned {
    public static readonly KuwaharaKind Classic = new("classic", "Classic", static pixels => Math.Floor(pixels), Quadrants);
    public static readonly KuwaharaKind Anisotropic = new("anisotropic", "Anisotropic", static pixels => pixels, Sectors);

    public string Caption { get; }

    [UseDelegateFromConstructor]
    public partial double Radius(double pixels);

    [UseDelegateFromConstructor]
    public partial Fin<Unit> Run(Painterly state, PixelExtent extent, PixelFrame frame, IProgress<int> progress);

    private static unsafe Fin<Unit> Quadrants(Painterly state, PixelExtent extent, PixelFrame frame, IProgress<int> progress) => frame.Filter(header => {
        (double radius, int width, int height) = (state.Kind.Radius(state.Size.Pixels(extent)), frame.Size.Width, frame.Size.Height);
        using Mat source = new();
        header.ConvertTo(source, DepthType.Cv64F);
        return new PixelPass.Pointwise((row, _, line) => {
            int y = frame.RowIndex(line);
            ReadOnlySpan2D<Vector256<double>> image = new ReadOnlySpan<Vector256<double>>(source.DataPointer.ToPointer(), width * height).AsSpan2D(height, width);
            using SpanOwner<Vector256<double>> sectors = SpanOwner<Vector256<double>>.Allocate(8);
            Span<Vector256<double>> means = sectors.Span[..4];
            Span<Vector256<double>> spreads = sectors.Span[4..];
            for (int x = 0; x < width; x++) {
                sectors.Span.Clear();
                for (int quadrant = 0; quadrant < means.Length; quadrant++) {
                    (int left, int right) = (quadrant & 1) is 0 ? ((int)double.Max(0d, x - radius), x) : (x, (int)double.Min(width - 1, x + radius));
                    (int top, int bottom) = quadrant < 2 ? (y, (int)double.Min(height - 1, y + radius)) : ((int)double.Max(0d, y - radius), y);
                    int columns = right - left + 1;
                    for (int cell = 0; cell < columns * (bottom - top + 1); cell++)
                        (means[quadrant], spreads[quadrant]) = Accumulated(means[quadrant], spreads[quadrant], image[top + (cell / columns), left + (cell % columns)], 1d);
                }
                int selected = 0;
                double variance = Vector256.Sum(spreads[selected]) / means[selected].GetElement(3);
                for (int quadrant = 1; quadrant < means.Length; quadrant++) {
                    double next = Vector256.Sum(spreads[quadrant]) / means[quadrant].GetElement(3);
                    if (next < variance)
                        (selected, variance) = (quadrant, next);
                }
                Vector256<double> mean = means[selected];
                row[x] = new Vector4((float)mean.GetElement(0), (float)mean.GetElement(1), (float)mean.GetElement(2), row[x].W);
            }
        }).Run(frame, progress);
    });

    private static unsafe Fin<Unit> Sectors(Painterly state, PixelExtent extent, PixelFrame frame, IProgress<int> progress) {
        double radius = state.Kind.Radius(state.Size.Pixels(extent));
        double reach = double.Ceiling((1d + state.Eccentricity) * radius) + 1d;
        return
            from bounded in guard(reach <= int.MaxValue, Error.New(string.Create(CultureInfo.InvariantCulture, $"Kuwahara reach {reach} pixels exceeds {int.MaxValue}")))
            from filtered in frame.Filter(source => {
                const int count = 8;
                const double corner = 0.182d;
                (int width, int height, double uniformity) = (frame.Size.Width, frame.Size.Height, state.Uniformity.Pixels(extent));
                (double sharpness, double overlap) = (16d * state.Sharpness * state.Sharpness, 2d / radius);
                (double sine, double cosine) = double.SinCos(1.5d * double.Pi / count);
                double envelope = (overlap + cosine) / (sine * sine);
                double diagonal = double.Sqrt(2d);
                using Mat copy = new();
                using Mat across = new(3, 3, DepthType.Cv64F, 1);
                using Mat down = new();
                using Mat rgb = ChannelMask.Rgb();
                using Mat horizontal = new(source.Size, DepthType.Cv64F, source.NumberOfChannels);
                using Mat vertical = new(source.Size, DepthType.Cv64F, source.NumberOfChannels);
                using Mat product = new();
                using Mat xx = new();
                using Mat xy = new();
                using Mat yy = new();
                source.ConvertTo(copy, DepthType.Cv64F);
                across.SetTo([-corner, 0d, corner, (2d * corner) - 1d, 0d, 1d - (2d * corner), -corner, 0d, corner]);
                CvInvoke.Transpose(across, down);
                CvInvoke.Filter2D(copy, horizontal, across, new Point(-1, -1), 0d, BorderType.Replicate);
                CvInvoke.Filter2D(copy, vertical, down, new Point(-1, -1), 0d, BorderType.Replicate);
                return Seq((horizontal, horizontal, xx), (horizontal, vertical, xy), (vertical, vertical, yy)).TraverseM(pair => {
                    (Mat first, Mat second, Mat tensor) = pair;
                    CvInvoke.Multiply(first, second, product);
                    CvInvoke.Transform(product, tensor, rgb);
                    return tensor.Blur(tensor, uniformity);
                }).As().Bind(_ => new PixelPass.Pointwise((row, _, line) => {
                    int y = frame.RowIndex(line);
                    ReadOnlySpan2D<Vector256<double>> image = new ReadOnlySpan<Vector256<double>>(copy.DataPointer.ToPointer(), width * height).AsSpan2D(height, width);
                    ReadOnlySpan<double> dxx = new(xx.GetDataPointer(y).ToPointer(), width);
                    ReadOnlySpan<double> dxy = new(xy.GetDataPointer(y).ToPointer(), width);
                    ReadOnlySpan<double> dyy = new(yy.GetDataPointer(y).ToPointer(), width);
                    using SpanOwner<Vector256<double>> sectors = SpanOwner<Vector256<double>>.Allocate(2 * count);
                    using SpanOwner<double> deviations = SpanOwner<double>.Allocate(count);
                    Span<Vector256<double>> means = sectors.Span[..count];
                    Span<Vector256<double>> spreads = sectors.Span[count..];
                    for (int x = 0; x < width; x++) {
                        (double half, double root) = ((dxx[x] + dyy[x]) / 2d, double.Hypot(dxx[x] - dyy[x], 2d * dxy[x]) / 2d);
                        (double axisY, double axisX) = double.SinCos(double.Atan2(-2d * dxy[x], dyy[x] - dxx[x]) / 2d);
                        double stretch = 1d + (state.Eccentricity * (half > 0d ? root / half : 0d));
                        (double major, double minor) = (stretch * radius, radius / stretch);
                        (int reachX, int reachY) = ((int)double.Ceiling(double.Hypot(major * axisX, minor * axisY)), (int)double.Ceiling(double.Hypot(major * axisY, minor * axisX)));
                        means.Fill(image[y, x].WithElement(3, 1d / count));
                        spreads.Clear();
                        for (long cell = reachX + 1L, span = (2L * reachX) + 1; cell < span * (reachY + 1L); cell++) {
                            (int i, int j) = ((int)((cell % span) - reachX), (int)(cell / span));
                            (double diskX, double diskY) = (((axisX * i) + (axisY * j)) / major, ((axisX * j) - (axisY * i)) / minor);
                            double distance = (diskX * diskX) + (diskY * diskY);
                            if (distance > 1d)
                                continue;
                            Vector256<double> projections = Vector256.Create(diskX, (diskX - diskY) / diagonal, diskY, (diskX + diskY) / diagonal);
                            Vector256<double> bend = Vector256.Create(overlap) - (envelope * projections * projections);
                            Vector256<double> swing = Vector256.Create(projections.GetElement(2), projections.GetElement(3), -projections.GetElement(0), -projections.GetElement(1));
                            (Vector256<double> low, Vector256<double> high) = (Vector256.Max(bend + swing, Vector256<double>.Zero), Vector256.Max(bend - swing, Vector256<double>.Zero));
                            double radial = double.Exp(-double.Pi * distance) / Vector256.Sum((low * low) + (high * high));
                            (low, high) = (low * low * radial, high * high * radial);
                            Vector256<double> upper = image[(int)long.Clamp((long)y + j, 0, height - 1), (int)long.Clamp((long)x + i, 0, width - 1)];
                            Vector256<double> lower = image[(int)long.Clamp((long)y - j, 0, height - 1), (int)long.Clamp((long)x - i, 0, width - 1)];
                            for (int k = 0, m = count / 2; k < count / 2; k++, m++) {
                                (means[k], spreads[k]) = Accumulated(means[k], spreads[k], upper, low.GetElement(k));
                                (means[k], spreads[k]) = Accumulated(means[k], spreads[k], lower, high.GetElement(k));
                                (means[m], spreads[m]) = Accumulated(means[m], spreads[m], upper, high.GetElement(k));
                                (means[m], spreads[m]) = Accumulated(means[m], spreads[m], lower, low.GetElement(k));
                            }
                        }
                        double minimum = double.PositiveInfinity;
                        for (int k = 0; k < count; k++) {
                            deviations.Span[k] = double.Max(0.02d, Vector256.Sum(Vector256.Sqrt(spreads[k] / means[k].GetElement(3))));
                            minimum = double.Min(minimum, deviations.Span[k]);
                        }
                        (Vector256<double> total, double weight) = (Vector256<double>.Zero, 0d);
                        for (int k = 0; k < count; k++) {
                            double share = double.Pow(minimum / deviations.Span[k], sharpness);
                            (total, weight) = (total + (means[k] * share), weight + share);
                        }
                        Vector256<double> mean = total / weight;
                        row[x] = new Vector4((float)mean.GetElement(0), (float)mean.GetElement(1), (float)mean.GetElement(2), row[x].W);
                    }
                }).Run(frame, progress));
            })
            select filtered;
    }

    private static (Vector256<double> Mean, Vector256<double> Spread) Accumulated(Vector256<double> mean, Vector256<double> spread, Vector256<double> sample, double share) =>
        (mean.GetElement(3), (sample - mean).WithElement(3, 0d)) switch {
            var (weight, delta) => ((mean + (delta * (share / (weight + share)))).WithElement(3, weight + share), spread + (delta * delta * (weight * share / (weight + share)))),
        };
}

public sealed record Painterly(KuwaharaKind Kind, ShortSideLength Size, ShortSideLength Uniformity, AxisFraction Sharpness, SectorEccentricity Eccentricity)
    : IStateRecord<Painterly, PainterlyParameter>, IPixelStage<Painterly> {
    public static Painterly Default { get; } = new(
        KuwaharaKind.Anisotropic, ShortSideLength.Create(6f / ReferenceFrame.Height), ShortSideLength.Create(4f / ReferenceFrame.Height), AxisFraction.MaxValue, SectorEccentricity.Create(1f));

    public static Option<PixelPass> Pass(Painterly state, PassContext context) =>
        state.Kind.Radius(state.Size.Pixels(context.Extent)) == 0f
            ? None
            : Some<PixelPass>(new PixelPass.Frame((frame, progress) => state.Kind.Run(state, context.Extent, frame, progress)));
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PainterlyParameter : IStateParameter<Painterly> {
    public static readonly PainterlyParameter Kuwahara = new("kind", new StateParameter<Painterly>.Choice<KuwaharaKind>(Lens<Painterly, KuwaharaKind>.New(static painterly => painterly.Kind, static kind => painterly => painterly with { Kind = kind })),
        new("Type", "Square quadrants or stroke-following sectors"));
    public static readonly PainterlyParameter Size = new("size", StateParameter.Bounded(Lens<Painterly, ShortSideLength>.New(static painterly => painterly.Size, static size => painterly => painterly with { Size = size }), ShortSideLength.Presentation with { Soft = (0f, 0.02f) }),
        new("Size", "Filter radius in frame short sides"));
    public static readonly PainterlyParameter Uniformity = new("uniformity", StateParameter.Bounded(Lens<Painterly, ShortSideLength>.New(static painterly => painterly.Uniformity, static uniformity => painterly => painterly with { Uniformity = uniformity }), ShortSideLength.Presentation with { Soft = (0f, 0.01f) }),
        new("Uniformity", "Stroke direction smoothing radius in frame short sides"));
    public static readonly PainterlyParameter Sharpness = new("sharpness", StateParameter.Bounded(Lens<Painterly, AxisFraction>.New(static painterly => painterly.Sharpness, static sharpness => painterly => painterly with { Sharpness = sharpness })),
        new("Sharpness", "Weight toward the least varied sector"));
    public static readonly PainterlyParameter Eccentricity = new("eccentricity", StateParameter.Bounded(Lens<Painterly, SectorEccentricity>.New(static painterly => painterly.Eccentricity, static eccentricity => painterly => painterly with { Eccentricity = eccentricity }), new Presentation<float>()),
        new("Eccentricity", "Sector elongation along strokes"));

    public StateParameter<Painterly> Kind { get; }
    public ParameterText Text { get; }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
file static class ChannelMask {
    public static Mat Rgb() {
        Mat sums = new(1, 4, DepthType.Cv32F, 1);
        sums.SetTo([1f, 1f, 1f, 0f]);
        return sums;
    }
}
