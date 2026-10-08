using System.Drawing;
using System.Numerics;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Rasm.Imaging.Filters.Generators;
using Rasm.Imaging.Grade;
using Rasm.Imaging.Pixels;
using UnitsNet;
using UnitsNet.Units;

namespace Rasm.Imaging.Filters.Stylize;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidStylize>]
public readonly partial struct DepthStep : IMinMaxValue<DepthStep> {
    public static DepthStep MinValue { get; } = new(0f);
    public static DepthStep MaxValue { get; } = new(1f);

    static partial void ValidateFactoryArguments(ref InvalidStylize? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidStylize();
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidStylize>]
public readonly partial struct EdgeThreshold : IMinMaxValue<EdgeThreshold> {
    public static EdgeThreshold MinValue { get; } = new(0f);
    public static EdgeThreshold MaxValue { get; } = new(1f);

    static partial void ValidateFactoryArguments(ref InvalidStylize? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidStylize();
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidStylize>]
public readonly partial struct ViewDepth : IMinMaxValue<ViewDepth> {
    public static ViewDepth MinValue { get; } = new(0f);
    public static ViewDepth MaxValue { get; } = new(100000f);

    static partial void ValidateFactoryArguments(ref InvalidStylize? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidStylize();
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidStylize>]
public readonly partial struct TaperScale : IMinMaxValue<TaperScale> {
    public static TaperScale MinValue { get; } = new(0f);
    public static TaperScale MaxValue { get; } = new(1f);

    static partial void ValidateFactoryArguments(ref InvalidStylize? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidStylize();
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidStylize>]
public readonly partial struct SectorSharpness : IMinMaxValue<SectorSharpness> {
    public static SectorSharpness MinValue { get; } = new(0f);
    public static SectorSharpness MaxValue { get; } = new(1f);

    static partial void ValidateFactoryArguments(ref InvalidStylize? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidStylize();
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidStylize>]
public readonly partial struct SectorEccentricity : IMinMaxValue<SectorEccentricity> {
    public static SectorEccentricity MinValue { get; } = new(0f);
    public static SectorEccentricity MaxValue { get; } = new(2f);

    static partial void ValidateFactoryArguments(ref InvalidStylize? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidStylize();
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidStylize>]
public readonly partial struct StrokeLength : IMinMaxValue<StrokeLength> {
    public static StrokeLength MinValue { get; } = new(0f);
    public static StrokeLength MaxValue { get; } = new(1f);
    public static StrokeLength Standard { get; } = new((float)(double)Length.FromInches(0.25m).Meters);

    static partial void ValidateFactoryArguments(ref InvalidStylize? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidStylize();
}

[SmartEnum<string>]
[ValidationError<InvalidStylize>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class StrokePlacement {
    public static readonly StrokePlacement Center = new("center", static (near, nearWidth, far, farWidth) =>
        near <= far ? ShapeEdge.Strip(near + 0.5f, nearWidth, 1f) : ShapeEdge.Strip(far + 0.5f, farWidth, 1f));
    public static readonly StrokePlacement Inside = new("inside", static (near, nearWidth, far, _) =>
        near <= far ? ShapeEdge.Coverage(near + 0.5f - nearWidth, 1f) : 0f);
    public static readonly StrokePlacement Outside = new("outside", static (near, _, far, farWidth) =>
        far < near ? ShapeEdge.Coverage(far + 0.5f - farWidth, 1f) : 0f);

    [UseDelegateFromConstructor]
    public partial float Cover(float near, float nearWidth, float far, float farWidth);
}

[SmartEnum<string>]
[ValidationError<InvalidStylize>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class LineClass {
    public static readonly LineClass Inner = new(
        "inner",
        Lens<GeometryOutline, ShortSideLength>.New(static outline => outline.InnerWidth, static width => outline => outline with { InnerWidth = width }),
        Lens<GeometryOutline, Gated<StrokeLength>>.New(static outline => outline.InnerWorld, static world => outline => outline with { InnerWorld = world }),
        Lens<GeometryOutline, Swatch>.New(static outline => outline.InnerColor, static color => outline => outline with { InnerColor = color }),
        static _ => StrokePlacement.Center);
    public static readonly LineClass Outer = new(
        "outer",
        Lens<GeometryOutline, ShortSideLength>.New(static outline => outline.OuterWidth, static width => outline => outline with { OuterWidth = width }),
        Lens<GeometryOutline, Gated<StrokeLength>>.New(static outline => outline.OuterWorld, static world => outline => outline with { OuterWorld = world }),
        Lens<GeometryOutline, Swatch>.New(static outline => outline.OuterColor, static color => outline => outline with { OuterColor = color }),
        static outline => outline.Placement);

    public Lens<GeometryOutline, ShortSideLength> Width { get; }
    public Lens<GeometryOutline, Gated<StrokeLength>> World { get; }
    public Lens<GeometryOutline, Swatch> Color { get; }

    [UseDelegateFromConstructor]
    public partial StrokePlacement Placement(GeometryOutline state);
}

[SmartEnum<string>]
[ValidationError<InvalidStylize>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class OutlineSource {
    public static readonly OutlineSource Silhouette = new(
        "silhouette",
        Lens<GeometryOutline, Option<LineClass>>.New(static outline => outline.Silhouette, static line => outline => outline with { Silhouette = line }),
        GuideChannel.Depth,
        static (first, second, edges, state) => {
            using Mat near = new();
            using Mat far = new();
            using Mat step = new();
            using ScalarArray ratio = new(state.Step);
            CvInvoke.ExtractChannel(first, near, 0);
            CvInvoke.ExtractChannel(second, far, 0);
            CvInvoke.AbsDiff(near, far, step);
            CvInvoke.Min(near, far, near);
            CvInvoke.Multiply(near, ratio, near);
            CvInvoke.Compare(step, near, edges, CmpType.GreaterThan);
        });
    public static readonly OutlineSource Crease = new(
        "crease",
        Lens<GeometryOutline, Option<LineClass>>.New(static outline => outline.Crease, static line => outline => outline with { Crease = line }),
        GuideChannel.Normal,
        static (first, second, edges, state) => {
            using Mat colour = Lanes.Colour();
            using Mat product = new();
            using Mat dot = new();
            using Mat lengths = new();
            using Mat partner = new();
            using ScalarArray bend = new(float.Cos(state.CreaseAngle));
            CvInvoke.Multiply(first, second, product);
            CvInvoke.Transform(product, dot, colour);
            CvInvoke.Multiply(first, first, product);
            CvInvoke.Transform(product, lengths, colour);
            CvInvoke.Multiply(second, second, product);
            CvInvoke.Transform(product, partner, colour);
            CvInvoke.Multiply(lengths, partner, lengths);
            CvInvoke.Sqrt(lengths, lengths);
            CvInvoke.Multiply(lengths, bend, lengths);
            CvInvoke.Compare(dot, lengths, edges, CmpType.LessThan);
        });
    public static readonly OutlineSource Object = new(
        "object",
        Lens<GeometryOutline, Option<LineClass>>.New(static outline => outline.Object, static line => outline => outline with { Object = line }),
        GuideChannel.ObjectId,
        Identities);
    public static readonly OutlineSource Material = new(
        "material",
        Lens<GeometryOutline, Option<LineClass>>.New(static outline => outline.Material, static line => outline => outline with { Material = line }),
        GuideChannel.MaterialId,
        Identities);

    public Lens<GeometryOutline, Option<LineClass>> Route { get; }
    public GuideChannel Channel { get; }

    [UseDelegateFromConstructor]
    public partial void Edges(Mat first, Mat second, Mat edges, GeometryOutline state);

    private static void Identities(Mat first, Mat second, Mat edges, GeometryOutline state) {
        using Mat leading = new();
        using Mat trailing = new();
        CvInvoke.ExtractChannel(first, leading, 0);
        CvInvoke.ExtractChannel(second, trailing, 0);
        CvInvoke.Compare(leading, trailing, edges, CmpType.NotEqual);
    }
}

public sealed record GeometryOutline(
    Option<LineClass> Silhouette, Option<LineClass> Crease, Option<LineClass> Object, Option<LineClass> Material,
    DepthStep Step, SignedAngle CreaseAngle,
    ShortSideLength OuterWidth, Gated<StrokeLength> OuterWorld, Swatch OuterColor, StrokePlacement Placement,
    ShortSideLength InnerWidth, Gated<StrokeLength> InnerWorld, Swatch InnerColor,
    ViewDepth TaperStart, ViewDepth TaperEnd, TaperScale Taper, Gated<Swatch> Paper, BlendingMode Mode)
    : IStateRecord<GeometryOutline, GeometryOutlineParameter, InvalidStylize>, IPixelStage<GeometryOutline> {
    public static GeometryOutline Default { get; } = new(
        Some(LineClass.Outer), Some(LineClass.Inner), Some(LineClass.Outer), None,
        DepthStep.Create(0.02f), SignedAngle.Diagonal,
        ShortSideLength.Create(3f / ReferenceFrame.Height), new(Enabled: false, StrokeLength.Standard), Swatch.Black, StrokePlacement.Center,
        ShortSideLength.Create(1f / ReferenceFrame.Height), new(Enabled: false, StrokeLength.Standard), Swatch.Black,
        ViewDepth.Create((float)(double)Length.FromFeet(10).Meters), ViewDepth.Create((float)(double)Length.FromFeet(100).Meters), TaperScale.MaxValue, new(Enabled: false, Swatch.Black), BlendingMode.Mix);

    public static Seq<GuideChannel> Channels(GeometryOutline state) =>
        GuideChannel.Depth.Cons(toSeq(OutlineSource.Items).Filter(source => source.Route.Get(state).IsSome).Map(static source => source.Channel)).Distinct();

    public static Option<PixelPass> Pass(GeometryOutline state, PassContext context) =>
        from depth in context.Guides.Find(GuideChannel.Depth)
        let routes = toSeq(OutlineSource.Items).Map(source => source.Route.Get(state).Map(line => (Source: source, Line: line))).Somes()
        where !routes.IsEmpty || state.Paper.Enabled
        select (PixelPass)new PixelPass.Frame((frame, progress) => Outlined(state, context, depth, routes, frame, progress));

    private static Fin<Unit> Outlined(
        GeometryOutline state, PassContext context, PixelFrame depth, Seq<(OutlineSource Source, LineClass Line)> routes, PixelFrame frame, IProgress<int> progress) {
        Vector3[] colors = [.. toSeq(LineClass.Items).Map(line => line.Color.Get(state).Display.AsVector3())];
        using Mat range = depth.Header();
        using Mat z = new();
        using Mat cover = Mat.Zeros(frame.Size.Height, frame.Size.Width, DepthType.Cv32F, colors.Length);
        CvInvoke.ExtractChannel(range, z, 0);
        return toSeq(LineClass.Items)
            .Map((line, channel) => (Line: line, Channel: channel, Sources: routes.Filter(route => route.Line == line).Map(static route => route.Source)))
            .Filter(static stroke => !stroke.Sources.IsEmpty)
            .TraverseM(stroke => Covered(state, context, z, stroke.Sources, stroke.Line, stroke.Channel, cover, frame, progress))
            .As()
            .Bind(_ => new PixelPass.Pointwise((row, _, line) => {
                using SpanOwner<Vector4> layer = SpanOwner<Vector4>.Allocate(row.Length);
                using SpanOwner<float> weights = SpanOwner<float>.Allocate(row.Length);
                ReadOnlySpan<float> covers = cover.GetSpan<float>().Slice(frame.Line(line) * row.Length * colors.Length, row.Length * colors.Length);
                if (state.Paper.Enabled)
                    row.Fill(state.Paper.Value.Display);
                for (int x = 0; x < row.Length; x++) {
                    (Vector3 color, float weight) = (Vector3.Zero, 0f);
                    for (int k = 0; k < colors.Length; k++)
                        (color, weight) = covers[(x * colors.Length) + k] switch { var c => ((c * colors[k]) + ((1f - c) * color), c + ((1f - c) * weight)) };
                    layer.Span[x] = new Vector4(weight > 0f ? color / weight : row[x].AsVector3(), 1f);
                    weights.Span[x] = weight;
                }
                state.Mode.Composite(row, layer.Span, weights.Span);
            }).Run(frame, progress));
    }

    private static Fin<Unit> Covered(
        GeometryOutline state, PassContext context, Mat z, Seq<OutlineSource> sources, LineClass line, int channel, Mat cover, PixelFrame frame, IProgress<int> progress) {
        (int width, int height, int classes) = (z.Cols, z.Rows, cover.NumberOfChannels);
        Func<float, float> stroke = Width(state, context, line);
        StrokePlacement placement = line.Placement(state);
        using Mat near = Mat.Zeros(height, width, DepthType.Cv8U, 1);
        using Mat far = Mat.Zeros(height, width, DepthType.Cv8U, 1);
        using Mat contour = new(height, width, DepthType.Cv32F, 1);
        using Mat nearReach = new();
        using Mat farReach = new();
        using Mat nearLabels = new();
        using Mat farLabels = new();
        contour.SetTo(new MCvScalar(float.PositiveInfinity));
        foreach ((OutlineSource source, PixelFrame guide) in sources.Map(source => context.Guides.Find(source.Channel).Map(guide => (source, guide))).Somes())
            Marked(source, state, guide, z, near, far, contour);
        using Mat nearWidths = Reached(near, contour, stroke, nearReach, nearLabels);
        using Mat farWidths = Reached(far, contour, stroke, farReach, farLabels);
        return new PixelPass.Pointwise((_, _, at) => {
            int start = frame.Line(at) * width;
            ReadOnlySpan<float> nearDistances = nearReach.GetSpan<float>().Slice(start, width);
            ReadOnlySpan<float> farDistances = farReach.GetSpan<float>().Slice(start, width);
            ReadOnlySpan<int> nearLines = nearLabels.GetSpan<int>().Slice(start, width);
            ReadOnlySpan<int> farLines = farLabels.GetSpan<int>().Slice(start, width);
            ReadOnlySpan<float> nearStrokes = nearWidths.GetSpan<float>();
            ReadOnlySpan<float> farStrokes = farWidths.GetSpan<float>();
            Span<float> covers = cover.GetSpan<float>().Slice(start * classes, width * classes);
            for (int x = 0; x < width; x++)
                covers[(x * classes) + channel] = placement.Cover(nearDistances[x], nearStrokes[nearLines[x]], farDistances[x], farStrokes[farLines[x]]);
        }).Run(frame, progress);
    }

    private static void Marked(OutlineSource source, GeometryOutline state, PixelFrame guide, Mat z, Mat near, Mat far, Mat contour) {
        using Mat header = guide.Header();
        using Mat edges = new();
        using Mat side = new();
        using Mat nearer = new();
        using Mat lowered = new();
        foreach ((Rectangle first, Rectangle second) in Seq(
                     (new Rectangle(0, 0, z.Cols - 1, z.Rows), new Rectangle(1, 0, z.Cols - 1, z.Rows)),
                     (new Rectangle(0, 0, z.Cols, z.Rows - 1), new Rectangle(0, 1, z.Cols, z.Rows - 1)))) {
            using Mat leading = new(header, first);
            using Mat trailing = new(header, second);
            using Mat firstDepth = new(z, first);
            using Mat secondDepth = new(z, second);
            source.Edges(leading, trailing, edges, state);
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
        }
    }

    private static Mat Reached(Mat mask, Mat contour, Func<float, float> stroke, Mat reach, Mat labels) {
        Mat widths = Mat.Zeros(1, CvInvoke.CountNonZero(mask) + 1, DepthType.Cv32F, 1);
        CvInvoke.BitwiseNot(mask, mask);
        CvInvoke.DistanceTransform(mask, reach, labels, DistType.L2, 5, DistLabelType.Pixel);
        Span<float> table = widths.GetSpan<float>();
        ReadOnlySpan<byte> marks = mask.GetSpan<byte>();
        ReadOnlySpan<float> depths = contour.GetSpan<float>();
        for (int pixel = 0, label = 1; pixel < marks.Length; pixel++) {
            if (marks[pixel] == 0)
                table[label++] = stroke(depths[pixel]);
        }
        return widths;
    }

    private static Func<float, float> Width(GeometryOutline state, PassContext context, LineClass line) {
        float frame = line.Width.Get(state).Pixels(context.Extent);
        (float start, float end, float taper) = (state.TaperStart, state.TaperEnd, state.Taper);
        Func<float, float> reach = (
                from world in line.World.Get(state).Active
                from camera in context.Camera
                select camera.Frustum.Switch(
                    (Frame: frame, Span: world * context.Extent.Width / (camera.Frustum.Window.Right - camera.Frustum.Window.Left)),
                    perspective: static (scale, _) => (Func<float, float>)(depth => float.Min(scale.Span / depth, scale.Frame)),
                    parallel: static (scale, _) => _ => float.Min(scale.Span, scale.Frame)))
            .IfNone(() => _ => frame);
        return depth => reach(depth) * float.Lerp(1f, taper, start == end ? depth >= start ? 1f : 0f : float.Clamp((depth - start) / (end - start), 0f, 1f));
    }
}

[SmartEnum<string>]
[ValidationError<InvalidStylize>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class GeometryOutlineParameter : IStateParameter<GeometryOutline> {
    private static readonly Presentation<ShortSideLength, float> Frame = ShortSideLength.Presentation with { Soft = (0f, 0.01f) };
    private static readonly Presentation<StrokeLength, float> World = new() {
        Unit = Quantity.GetUnitInfo(LengthUnit.Meter), Soft = (0f, (float)(double)Length.FromInches(1).Meters), Origin = (float)StrokeLength.Standard,
    };
    private static readonly Presentation<ViewDepth, float> Distance = new() { Unit = Quantity.GetUnitInfo(LengthUnit.Meter), Soft = (0f, (float)(double)Length.FromFeet(1000).Meters) };

    public static readonly GeometryOutlineParameter Silhouette = new("silhouette-lines", new StateParameter<GeometryOutline>.OptionalChoice<LineClass, InvalidStylize>(OutlineSource.Silhouette.Route));
    public static readonly GeometryOutlineParameter Crease = new("crease-lines", new StateParameter<GeometryOutline>.OptionalChoice<LineClass, InvalidStylize>(OutlineSource.Crease.Route));
    public static readonly GeometryOutlineParameter Object = new("object-lines", new StateParameter<GeometryOutline>.OptionalChoice<LineClass, InvalidStylize>(OutlineSource.Object.Route));
    public static readonly GeometryOutlineParameter Material = new("material-lines", new StateParameter<GeometryOutline>.OptionalChoice<LineClass, InvalidStylize>(OutlineSource.Material.Route));
    public static readonly GeometryOutlineParameter Step = new("depth-step", new StateParameter<GeometryOutline>.Bounded<DepthStep, float, InvalidStylize>(
        Lens<GeometryOutline, DepthStep>.New(static outline => outline.Step, static step => outline => outline with { Step = step }), new() { Soft = (0f, 0.2f) }));
    public static readonly GeometryOutlineParameter CreaseAngle = new("crease-angle", new StateParameter<GeometryOutline>.Bounded<SignedAngle, float, InvalidPixelValue>(
        Lens<GeometryOutline, SignedAngle>.New(static outline => outline.CreaseAngle, static angle => outline => outline with { CreaseAngle = angle }),
        SignedAngle.Presentation with { Soft = (0f, SignedAngle.Up) }));
    public static readonly GeometryOutlineParameter OuterWidth = new("outer-width", new StateParameter<GeometryOutline>.Bounded<ShortSideLength, float, InvalidPixelValue>(LineClass.Outer.Width, Frame));
    public static readonly GeometryOutlineParameter OuterWorld = new("outer-world", new StateParameter<GeometryOutline>.OptionalBounded<StrokeLength, float, InvalidStylize>(LineClass.Outer.World, World));
    public static readonly GeometryOutlineParameter OuterColor = new("outer-color", new StateParameter<GeometryOutline>.Color(LineClass.Outer.Color));
    public static readonly GeometryOutlineParameter Placement = new("placement", new StateParameter<GeometryOutline>.Choice<StrokePlacement, InvalidStylize>(
        Lens<GeometryOutline, StrokePlacement>.New(static outline => outline.Placement, static placement => outline => outline with { Placement = placement })));
    public static readonly GeometryOutlineParameter InnerWidth = new("inner-width", new StateParameter<GeometryOutline>.Bounded<ShortSideLength, float, InvalidPixelValue>(LineClass.Inner.Width, Frame));
    public static readonly GeometryOutlineParameter InnerWorld = new("inner-world", new StateParameter<GeometryOutline>.OptionalBounded<StrokeLength, float, InvalidStylize>(LineClass.Inner.World, World));
    public static readonly GeometryOutlineParameter InnerColor = new("inner-color", new StateParameter<GeometryOutline>.Color(LineClass.Inner.Color));
    public static readonly GeometryOutlineParameter TaperStart = new("taper-start", new StateParameter<GeometryOutline>.Bounded<ViewDepth, float, InvalidStylize>(
        Lens<GeometryOutline, ViewDepth>.New(static outline => outline.TaperStart, static depth => outline => outline with { TaperStart = depth }), Distance));
    public static readonly GeometryOutlineParameter TaperEnd = new("taper-end", new StateParameter<GeometryOutline>.Bounded<ViewDepth, float, InvalidStylize>(
        Lens<GeometryOutline, ViewDepth>.New(static outline => outline.TaperEnd, static depth => outline => outline with { TaperEnd = depth }), Distance));
    public static readonly GeometryOutlineParameter Taper = new("taper-scale", new StateParameter<GeometryOutline>.Bounded<TaperScale, float, InvalidStylize>(
        Lens<GeometryOutline, TaperScale>.New(static outline => outline.Taper, static taper => outline => outline with { Taper = taper }), new()));
    public static readonly GeometryOutlineParameter Paper = new("paper", new StateParameter<GeometryOutline>.OptionalColor(
        Lens<GeometryOutline, Gated<Swatch>>.New(static outline => outline.Paper, static paper => outline => outline with { Paper = paper })));
    public static readonly GeometryOutlineParameter Mode = new("mode", new StateParameter<GeometryOutline>.Choice<BlendingMode, InvalidGrade>(
        Lens<GeometryOutline, BlendingMode>.New(static outline => outline.Mode, static mode => outline => outline with { Mode = mode })));

    public StateParameter<GeometryOutline> Kind { get; }
}

[SmartEnum<string>]
[ValidationError<InvalidStylize>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public abstract partial class EdgeOperator {
    public static readonly EdgeOperator Laplace = new Gradient("laplace", Seq(-1f / 8f, -1f / 8f, -1f / 8f, -1f / 8f, 1f, -1f / 8f, -1f / 8f, -1f / 8f, -1f / 8f));
    public static readonly EdgeOperator Sobel = new Gradient("sobel", Seq(1f, 0f, -1f, 2f, 0f, -2f, 1f, 0f, -1f));
    public static readonly EdgeOperator Prewitt = new Gradient("prewitt", Seq(1f, 0f, -1f, 1f, 0f, -1f, 1f, 0f, -1f));
    public static readonly EdgeOperator Kirsch = new Gradient("kirsch", Seq(5f, -3f, -2f, 5f, -3f, -2f, 5f, -3f, -2f));
    public static readonly EdgeOperator Emboss = new Relief("emboss", Seq(1f, 2f, 1f, 0f, 1f, 0f, -1f, -2f, -1f));

    public Seq<float> Kernel { get; }

    public abstract float Gain { get; }

    private protected Seq<(float X, float Y)> Steps =>
        from axis in Seq<Func<int, int>>(static cell => cell / 3, static cell => cell % 3)
        from split in Seq(1, 2)
        from side in Seq<Func<int, int, bool>>(static (at, edge) => at < edge, static (at, edge) => at >= edge)
        let lit = toSeq(Range(0, 9)).Filter(cell => side(axis(cell), split))
        select (lit.Fold(0f, (sum, cell) => sum + Kernel[cell]), lit.Fold(0f, (sum, cell) => sum + Kernel[(3 * (cell % 3)) + (cell / 3)]));

    public abstract void Respond(Mat plane, Mat response);

    private protected Mat Across() => Taps(static (row, column) => (3 * (2 - row)) + column);

    private protected Mat Down() => Taps(static (row, column) => (3 * column) + 2 - row);

    private Mat Taps(Func<int, int, int> cell) {
        Mat taps = new(3, 3, DepthType.Cv32F, 1);
        taps.SetTo([.. toSeq(Range(0, 9)).Map(index => Kernel[cell(index / 3, index % 3)])]);
        return taps;
    }

    private sealed class Gradient(string key, Seq<float> kernel) : EdgeOperator(key, kernel) {
        public override float Gain => Steps.Fold(0f, static (gain, step) => float.Max(gain, float.Hypot(step.X, step.Y)));

        public override void Respond(Mat plane, Mat response) {
            using Mat across = Across();
            using Mat down = Down();
            using Mat vertical = new(plane.Size, DepthType.Cv32F, plane.NumberOfChannels);
            CvInvoke.Filter2D(plane, response, across, Lanes.Centered, 0d, BorderType.Replicate);
            CvInvoke.Filter2D(plane, vertical, down, Lanes.Centered, 0d, BorderType.Replicate);
            CvInvoke.Multiply(response, response, response);
            CvInvoke.Multiply(vertical, vertical, vertical);
            CvInvoke.Add(response, vertical, response);
            CvInvoke.Sqrt(response, response);
        }
    }

    private sealed class Relief(string key, Seq<float> kernel) : EdgeOperator(key, kernel) {
        public override float Gain => Steps.Fold(0f, static (gain, step) => float.Max(gain, step.X));

        public override void Respond(Mat plane, Mat response) {
            using Mat across = Across();
            using ScalarArray floor = new(0d);
            CvInvoke.Filter2D(plane, response, across, Lanes.Centered, 0d, BorderType.Replicate);
            CvInvoke.Max(response, floor, response);
        }
    }
}

[SmartEnum<string>]
[ValidationError<InvalidStylize>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class EdgeSource {
    public static readonly EdgeSource Color = new("color", 3, static (row, plane, _) => {
        for (int x = 0; x < row.Length; x++)
            row[x].AsVector3().CopyTo(plane[(3 * x)..]);
    });
    public static readonly EdgeSource Luma = new("luma", 1, static (row, plane, weights) => {
        for (int x = 0; x < row.Length; x++)
            plane[x] = Vector3.Dot(weights, row[x].AsVector3());
    });
    public static readonly EdgeSource Hue = new("hue", 2, static (row, plane, _) => {
        for (int x = 0; x < row.Length; x++)
            (float.SinCos(Hsy.From(row[x]).Hue) switch { var (sine, cosine) => new Vector2(cosine, sine) / float.Tau }).CopyTo(plane[(2 * x)..]);
    });
    public static readonly EdgeSource Saturation = new("saturation", 1, static (row, plane, _) => {
        for (int x = 0; x < row.Length; x++)
            plane[x] = RangeAxis.Saturation.Coordinate(Hsy.From(row[x]));
    });
    public static readonly EdgeSource Value = new("value", 1, static (row, plane, _) => {
        for (int x = 0; x < row.Length; x++)
            plane[x] = float.Max(float.Max(row[x].X, row[x].Y), row[x].Z);
    });
    public static readonly EdgeSource Alpha = new("alpha", 1, static (row, plane, _) => {
        for (int x = 0; x < row.Length; x++)
            plane[x] = row[x].W;
    });

    public int Planes { get; }

    [UseDelegateFromConstructor]
    public partial void Read(ReadOnlySpan<Vector4> row, Span<float> plane, Vector3 weights);
}

[SmartEnum<string>]
[ValidationError<InvalidStylize>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class EdgeRendering {
    public static readonly EdgeRendering Magnitude = new("magnitude");
    public static readonly EdgeRendering Lines = new("lines");
    public static readonly EdgeRendering LinesOnFill = new("lines-on-fill");
}

public sealed record EdgeDetect(
    EdgeOperator Operator, EdgeSource Source, ShortSideLength Softening, EdgeRendering Rendering,
    EdgeThreshold Threshold, ShortSideLength Width, Swatch Line, Swatch Fill, BlendingMode Mode)
    : IStateRecord<EdgeDetect, EdgeDetectParameter, InvalidStylize>, IPixelStage<EdgeDetect> {
    public static EdgeDetect Default { get; } = new(
        EdgeOperator.Kirsch, EdgeSource.Luma, ShortSideLength.Neutral, EdgeRendering.LinesOnFill,
        EdgeThreshold.Create(0.1f), ShortSideLength.Create(1f / ReferenceFrame.Height), Swatch.White, Swatch.Black, BlendingMode.Mix);

    public static Option<PixelPass> Pass(EdgeDetect state, PassContext context) =>
        state.Rendering.Map(magnitude: false, lines: state.Width == ShortSideLength.Neutral, linesOnFill: false)
            ? None
            : Some<PixelPass>(new PixelPass.Frame((frame, progress) => Detected(state, context, frame, progress)));

    private static Fin<Unit> Detected(EdgeDetect state, PassContext context, PixelFrame frame, IProgress<int> progress) {
        (int width, int planes, float sigma) = (frame.Size.Width, state.Source.Planes, state.Softening.Pixels(context.Extent));
        int taps = PixelSampling.GaussianTaps(sigma);
        using Mat plane = new(frame.Size.Height, width, DepthType.Cv32F, planes);
        using Mat response = new(frame.Size.Height, width, DepthType.Cv32F, planes);
        using Mat squares = new();
        return new PixelPass.Pointwise((row, _, line) =>
                state.Source.Read(row, plane.GetSpan<float>().Slice(frame.Line(line) * width * planes, width * planes), context.Working.Luminance))
            .Run(frame, progress)
            .Bind(_ => {
                if (sigma > 0f)
                    CvInvoke.GaussianBlur(plane, plane, new Size(taps, taps), sigma, sigma, BorderType.Replicate);
                state.Operator.Respond(plane, response);
                if (state.Source != EdgeSource.Color) {
                    using Mat sums = Mat.Ones(EdgeSource.Color.Planes, planes, DepthType.Cv32F, 1);
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
    }

    private static Fin<Unit> Magnitudes(PixelFrame frame, Mat edges, IProgress<int> progress) =>
        new PixelPass.Pointwise((row, _, line) => {
            ReadOnlySpan<Vector3> responses = edges.GetSpan<Vector3>().Slice(frame.Line(line) * row.Length, row.Length);
            for (int x = 0; x < row.Length; x++)
                row[x] = new Vector4(responses[x], row[x].W);
        }).Run(frame, progress);

    private static Fin<Unit> Stroked(EdgeDetect state, PassContext context, PixelFrame frame, Mat edges, Swatch ground, float floor, IProgress<int> progress) {
        (Vector3 luminance, Vector3 line, Vector3 under, float stroke) = (context.Working.Luminance, state.Line.Display.AsVector3(), ground.Display.AsVector3(), state.Width.Pixels(context.Extent));
        using Mat weights = new(1, EdgeSource.Color.Planes, DepthType.Cv32F, 1);
        using Mat value = new();
        using Mat mask = new();
        using Mat reach = new();
        using ScalarArray threshold = new(state.Threshold * state.Operator.Gain);
        weights.SetTo([luminance.X, luminance.Y, luminance.Z]);
        CvInvoke.Transform(edges, value, weights);
        CvInvoke.Compare(value, threshold, mask, CmpType.GreaterThan);
        CvInvoke.BitwiseNot(mask, mask);
        CvInvoke.DistanceTransform(mask, reach, labels: null, DistType.L2, 5, DistLabelType.Pixel);
        return new PixelPass.Pointwise((row, _, at) => {
            using SpanOwner<Vector4> layer = SpanOwner<Vector4>.Allocate(row.Length);
            using SpanOwner<float> cover = SpanOwner<float>.Allocate(row.Length);
            ReadOnlySpan<float> distances = reach.GetSpan<float>().Slice(frame.Line(at) * row.Length, row.Length);
            for (int x = 0; x < row.Length; x++) {
                float coverage = ShapeEdge.Strip(distances[x] + 0.5f, stroke, 1f);
                layer.Span[x] = new Vector4(Vector3.Lerp(under, line, coverage), 1f);
                cover.Span[x] = float.Max(coverage, floor);
            }
            state.Mode.Composite(row, layer.Span, cover.Span);
        }).Run(frame, progress);
    }
}

[SmartEnum<string>]
[ValidationError<InvalidStylize>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class EdgeDetectParameter : IStateParameter<EdgeDetect> {
    private static readonly Presentation<ShortSideLength, float> Span = ShortSideLength.Presentation with { Soft = (0f, 0.01f) };

    public static readonly EdgeDetectParameter Operator = new("operator", new StateParameter<EdgeDetect>.Choice<EdgeOperator, InvalidStylize>(
        Lens<EdgeDetect, EdgeOperator>.New(static detect => detect.Operator, static kernel => detect => detect with { Operator = kernel })));
    public static readonly EdgeDetectParameter Source = new("source", new StateParameter<EdgeDetect>.Choice<EdgeSource, InvalidStylize>(
        Lens<EdgeDetect, EdgeSource>.New(static detect => detect.Source, static source => detect => detect with { Source = source })));
    public static readonly EdgeDetectParameter Softening = new("softening", new StateParameter<EdgeDetect>.Bounded<ShortSideLength, float, InvalidPixelValue>(
        Lens<EdgeDetect, ShortSideLength>.New(static detect => detect.Softening, static softening => detect => detect with { Softening = softening }), Span));
    public static readonly EdgeDetectParameter Rendering = new("rendering", new StateParameter<EdgeDetect>.Choice<EdgeRendering, InvalidStylize>(
        Lens<EdgeDetect, EdgeRendering>.New(static detect => detect.Rendering, static rendering => detect => detect with { Rendering = rendering })));
    public static readonly EdgeDetectParameter Threshold = new("threshold", new StateParameter<EdgeDetect>.Bounded<EdgeThreshold, float, InvalidStylize>(
        Lens<EdgeDetect, EdgeThreshold>.New(static detect => detect.Threshold, static threshold => detect => detect with { Threshold = threshold }), new()));
    public static readonly EdgeDetectParameter Width = new("width", new StateParameter<EdgeDetect>.Bounded<ShortSideLength, float, InvalidPixelValue>(
        Lens<EdgeDetect, ShortSideLength>.New(static detect => detect.Width, static width => detect => detect with { Width = width }), Span));
    public static readonly EdgeDetectParameter Line = new("line-color", new StateParameter<EdgeDetect>.Color(
        Lens<EdgeDetect, Swatch>.New(static detect => detect.Line, static line => detect => detect with { Line = line })));
    public static readonly EdgeDetectParameter Fill = new("fill-color", new StateParameter<EdgeDetect>.Color(
        Lens<EdgeDetect, Swatch>.New(static detect => detect.Fill, static fill => detect => detect with { Fill = fill })));
    public static readonly EdgeDetectParameter Mode = new("mode", new StateParameter<EdgeDetect>.Choice<BlendingMode, InvalidGrade>(
        Lens<EdgeDetect, BlendingMode>.New(static detect => detect.Mode, static mode => detect => detect with { Mode = mode })));

    public StateParameter<EdgeDetect> Kind { get; }
}

[SmartEnum<string>]
[ValidationError<InvalidStylize>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class KuwaharaKind {
    public static readonly KuwaharaKind Classic = new("classic", static pixels => float.Floor(pixels), Quadrants);
    public static readonly KuwaharaKind Anisotropic = new("anisotropic", static pixels => pixels, Sectors);

    [UseDelegateFromConstructor]
    public partial float Radius(float pixels);

    [UseDelegateFromConstructor]
    public partial Fin<Unit> Run(Painterly state, PixelExtent extent, PixelFrame frame, IProgress<int> progress);

    private static Fin<Unit> Quadrants(Painterly state, PixelExtent extent, PixelFrame frame, IProgress<int> progress) {
        (int radius, int width, int height) = ((int)state.Kind.Radius(state.Size.Pixels(extent)), frame.Size.Width, frame.Size.Height);
        Size window = new(radius + 1, radius + 1);
        using Mat source = frame.Header();
        using Mat sums = new();
        using Mat squares = new();
        using Mat means = new(height, width, DepthType.Cv32F, 3);
        using Mat spreads = new(height, width, DepthType.Cv32F, 1);
        spreads.SetTo(new MCvScalar(float.MaxValue));
        return Seq((Right: false, Up: false), (Right: true, Up: false), (Right: false, Up: true), (Right: true, Up: true))
            .TraverseM(quadrant => {
                Point anchor = new(quadrant.Right ? 0 : radius, quadrant.Up ? radius : 0);
                CvInvoke.BoxFilter(source, sums, DepthType.Cv32F, window, anchor, normalize: false, BorderType.Constant);
                CvInvoke.SqrBoxFilter(source, squares, DepthType.Cv32F, window, anchor, normalize: false, BorderType.Constant);
                return new PixelPass.Pointwise((_, _, line) => {
                    int y = frame.Line(line);
                    int rows = int.Min(radius, quadrant.Up ? y : height - 1 - y) + 1;
                    ReadOnlySpan<Vector4> sum = sums.GetSpan<Vector4>().Slice(y * width, width);
                    ReadOnlySpan<Vector4> square = squares.GetSpan<Vector4>().Slice(y * width, width);
                    Span<Vector3> kept = means.GetSpan<Vector3>().Slice(y * width, width);
                    Span<float> spread = spreads.GetSpan<float>().Slice(y * width, width);
                    for (int x = 0; x < width; x++) {
                        float count = rows * (int.Min(radius, quadrant.Right ? width - 1 - x : x) + 1);
                        Vector3 mean = sum[x].AsVector3() / count;
                        float variance = Vector3.Dot((square[x].AsVector3() / count) - (mean * mean), Vector3.One);
                        if (variance < spread[x])
                            (spread[x], kept[x]) = (variance, mean);
                    }
                }).Run(frame, progress);
            })
            .As()
            .Bind(_ => new PixelPass.Pointwise((row, _, line) => {
                ReadOnlySpan<Vector3> kept = means.GetSpan<Vector3>().Slice(frame.Line(line) * width, width);
                for (int x = 0; x < width; x++)
                    row[x] = new Vector4(kept[x], row[x].W);
            }).Run(frame, progress));
    }

    private static Fin<Unit> Sectors(Painterly state, PixelExtent extent, PixelFrame frame, IProgress<int> progress) {
        const int count = 8;
        const float corner = 0.182f;
        (int width, int height, float radius, float uniformity) = (frame.Size.Width, frame.Size.Height, state.Kind.Radius(state.Size.Pixels(extent)), state.Uniformity.Pixels(extent));
        (float eccentricity, float sharpness, float overlap) = (1f / float.Max(0.01f, state.Eccentricity), 16f * state.Sharpness * state.Sharpness, 2f / radius);
        (float sine, float cosine) = float.SinCos(1.5f * float.Pi / count);
        (float envelope, int taps) = ((overlap + cosine) / (sine * sine), (2 * (int)float.Ceiling(uniformity)) + 1);
        using Mat source = frame.Header();
        using Mat copy = new();
        using Mat across = new(3, 3, DepthType.Cv32F, 1);
        using Mat down = new();
        using Mat colour = Lanes.Colour();
        using Mat horizontal = new(source.Size, DepthType.Cv32F, source.NumberOfChannels);
        using Mat vertical = new(source.Size, DepthType.Cv32F, source.NumberOfChannels);
        using Mat product = new();
        using Mat xx = new();
        using Mat xy = new();
        using Mat yy = new();
        across.SetTo([-corner, 0f, corner, (2f * corner) - 1f, 0f, 1f - (2f * corner), -corner, 0f, corner]);
        CvInvoke.Transpose(across, down);
        CvInvoke.Filter2D(source, horizontal, across, Lanes.Centered, 0d, BorderType.Replicate);
        CvInvoke.Filter2D(source, vertical, down, Lanes.Centered, 0d, BorderType.Replicate);
        foreach ((Mat first, Mat second, Mat tensor) in Seq((horizontal, horizontal, xx), (horizontal, vertical, xy), (vertical, vertical, yy))) {
            CvInvoke.Multiply(first, second, product);
            CvInvoke.Transform(product, tensor, colour);
            if (uniformity > 0f)
                CvInvoke.GaussianBlur(tensor, tensor, new Size(taps, taps), uniformity / 3f, uniformity / 3f, BorderType.Replicate);
        }
        source.CopyTo(copy);
        return new PixelPass.Pointwise((row, _, line) => {
            int y = frame.Line(line);
            using SpanOwner<Vector4> sectors = SpanOwner<Vector4>.Allocate(2 * count);
            Span<Vector4> means = sectors.Span[..count];
            Span<Vector4> spreads = sectors.Span[count..];
            ReadOnlySpan2D<Vector4> image = copy.GetSpan<Vector4>().AsSpan2D(height, width);
            ReadOnlySpan<float> dxx = xx.GetSpan<float>().Slice(y * width, width);
            ReadOnlySpan<float> dxy = xy.GetSpan<float>().Slice(y * width, width);
            ReadOnlySpan<float> dyy = yy.GetSpan<float>().Slice(y * width, width);
            for (int x = 0; x < width; x++) {
                (float half, float root) = ((dxx[x] + dyy[x]) / 2f, float.Hypot(dxx[x] - dyy[x], 2f * dxy[x]) / 2f);
                Vector2 vector = new(half + root - dxx[x], -dxy[x]);
                Vector2 axis = vector.Length() != 0f ? Vector2.Normalize(vector) : Vector2.One;
                float stretch = (eccentricity + (half > 0f ? root / half : 0f)) / eccentricity;
                (float major, float minor) = (stretch * radius, radius / stretch);
                (int reachX, int reachY) = ((int)float.Ceiling(float.Hypot(major * axis.X, minor * axis.Y)), (int)float.Ceiling(float.Hypot(major * axis.Y, minor * axis.X)));
                Vector3 center = image[y, x].AsVector3();
                means.Fill(new Vector4(center / count, 1f / count));
                spreads.Fill(new Vector4(center * center / count, 0f));
                for (int cell = reachX + 1, span = (2 * reachX) + 1; cell < span * (reachY + 1); cell++) {
                    (int i, int j) = ((cell % span) - reachX, cell / span);
                    Vector2 disk = new(((axis.X * i) + (axis.Y * j)) / major, ((axis.X * j) - (axis.Y * i)) / minor);
                    float distance = Vector2.Dot(disk, disk);
                    if (distance > 1f)
                        continue;
                    Vector2 turned = new Vector2(disk.X - disk.Y, disk.X + disk.Y) / float.Sqrt(2f);
                    Vector4 lanes = new(disk.X, turned.X, disk.Y, turned.Y);
                    Vector4 bend = new Vector4(overlap) - (envelope * lanes * lanes);
                    Vector4 swing = new(lanes.Z, lanes.W, -lanes.X, -lanes.Y);
                    (Vector4 low, Vector4 high) = (Vector4.Max(bend + swing, Vector4.Zero), Vector4.Max(bend - swing, Vector4.Zero));
                    float radial = float.Exp(-float.Pi * distance) / Vector4.Dot((low * low) + (high * high), Vector4.One);
                    (low, high) = (low * low * radial, high * high * radial);
                    Vector3 upper = image[int.Clamp(y + j, 0, height - 1), int.Clamp(x + i, 0, width - 1)].AsVector3();
                    Vector3 lower = image[int.Clamp(y - j, 0, height - 1), int.Clamp(x - i, 0, width - 1)].AsVector3();
                    for (int k = 0; k < count / 2; k++) {
                        means[k] += new Vector4((upper * low[k]) + (lower * high[k]), low[k] + high[k]);
                        means[k + (count / 2)] += new Vector4((upper * high[k]) + (lower * low[k]), low[k] + high[k]);
                        spreads[k] += new Vector4((upper * upper * low[k]) + (lower * lower * high[k]), 0f);
                        spreads[k + (count / 2)] += new Vector4((upper * upper * high[k]) + (lower * lower * low[k]), 0f);
                    }
                }
                (Vector3 total, float weight) = (Vector3.Zero, 0f);
                for (int k = 0; k < count; k++) {
                    Vector3 mean = means[k].AsVector3() / means[k].W;
                    float deviation = Vector3.Dot(Vector3.SquareRoot(Vector3.Abs((spreads[k].AsVector3() / means[k].W) - (mean * mean))), Vector3.One);
                    float share = 1f / float.Pow(float.Max(0.02f, deviation), sharpness);
                    (total, weight) = (total + (mean * share), weight + share);
                }
                row[x] = new Vector4(weight == 0f ? center : total / weight, row[x].W);
            }
        }).Run(frame, progress);
    }
}

public sealed record Painterly(KuwaharaKind Kind, ShortSideLength Size, ShortSideLength Uniformity, SectorSharpness Sharpness, SectorEccentricity Eccentricity)
    : IStateRecord<Painterly, PainterlyParameter, InvalidStylize>, IPixelStage<Painterly> {
    public static Painterly Default { get; } = new(
        KuwaharaKind.Anisotropic, ShortSideLength.Create(6f / ReferenceFrame.Height), ShortSideLength.Create(4f / ReferenceFrame.Height), SectorSharpness.MaxValue, SectorEccentricity.Create(1f));

    public static Option<PixelPass> Pass(Painterly state, PassContext context) =>
        state.Kind.Radius(state.Size.Pixels(context.Extent)) == 0f
            ? None
            : Some<PixelPass>(new PixelPass.Frame((frame, progress) => state.Kind.Run(state, context.Extent, frame, progress)));
}

[SmartEnum<string>]
[ValidationError<InvalidStylize>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PainterlyParameter : IStateParameter<Painterly> {
    public static readonly PainterlyParameter Kuwahara = new("kind", new StateParameter<Painterly>.Choice<KuwaharaKind, InvalidStylize>(
        Lens<Painterly, KuwaharaKind>.New(static painterly => painterly.Kind, static kind => painterly => painterly with { Kind = kind })));
    public static readonly PainterlyParameter Size = new("size", new StateParameter<Painterly>.Bounded<ShortSideLength, float, InvalidPixelValue>(
        Lens<Painterly, ShortSideLength>.New(static painterly => painterly.Size, static size => painterly => painterly with { Size = size }),
        ShortSideLength.Presentation with { Soft = (0f, 0.02f) }));
    public static readonly PainterlyParameter Uniformity = new("uniformity", new StateParameter<Painterly>.Bounded<ShortSideLength, float, InvalidPixelValue>(
        Lens<Painterly, ShortSideLength>.New(static painterly => painterly.Uniformity, static uniformity => painterly => painterly with { Uniformity = uniformity }),
        ShortSideLength.Presentation with { Soft = (0f, 0.01f) }));
    public static readonly PainterlyParameter Sharpness = new("sharpness", new StateParameter<Painterly>.Bounded<SectorSharpness, float, InvalidStylize>(
        Lens<Painterly, SectorSharpness>.New(static painterly => painterly.Sharpness, static sharpness => painterly => painterly with { Sharpness = sharpness }), new()));
    public static readonly PainterlyParameter Eccentricity = new("eccentricity", new StateParameter<Painterly>.Bounded<SectorEccentricity, float, InvalidStylize>(
        Lens<Painterly, SectorEccentricity>.New(static painterly => painterly.Eccentricity, static eccentricity => painterly => painterly with { Eccentricity = eccentricity }), new()));

    public StateParameter<Painterly> Kind { get; }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
file static class Lanes {
    public static Point Centered { get; } = new(-1, -1);

    public static Mat Colour() {
        Mat sums = new(1, 4, DepthType.Cv32F, 1);
        sums.SetTo([1f, 1f, 1f, 0f]);
        return sums;
    }
}
