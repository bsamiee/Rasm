using System.Drawing;
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
using SixLabors.ImageSharp.PixelFormats;
using TorchSharp;
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

[SmartEnum<string>]
[ValidationError<InvalidStylize>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class StrokePlacement {
    public static readonly StrokePlacement Center = new("center", static (near, nearWidth, far, farWidth) =>
        near <= far ? ShapeEdge.Strip(near + 0.5f, nearWidth, 1f) : ShapeEdge.Strip(far + 0.5f, farWidth, 1f));
    public static readonly StrokePlacement Inside = new("inside", static (near, nearWidth, far, _) =>
        (float)double.Clamp(nearWidth - near, 0d, 1d) * ShapeEdge.Coverage((near - far) / 2d, 1d));
    public static readonly StrokePlacement Outside = new("outside", static (near, _, far, farWidth) =>
        (float)double.Clamp(farWidth - far, 0d, 1d) * ShapeEdge.Coverage((far - near) / 2d, 1d));

    [UseDelegateFromConstructor]
    public partial float Cover(double near, double nearWidth, double far, double farWidth);
}

[SmartEnum<string>]
[ValidationError<InvalidStylize>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class LineClass {
    public static readonly LineClass Outer = new(
        "outer",
        Lens<GeometryOutline, ShortSideLength>.New(static outline => outline.OuterWidth, static width => outline => outline with { OuterWidth = width }),
        Lens<GeometryOutline, Gated<Length>>.New(static outline => outline.OuterWorld, static world => outline => outline with { OuterWorld = world }),
        Lens<GeometryOutline, Swatch>.New(static outline => outline.OuterColor, static color => outline => outline with { OuterColor = color }),
        static outline => outline.Placement);
    public static readonly LineClass Inner = new(
        "inner",
        Lens<GeometryOutline, ShortSideLength>.New(static outline => outline.InnerWidth, static width => outline => outline with { InnerWidth = width }),
        Lens<GeometryOutline, Gated<Length>>.New(static outline => outline.InnerWorld, static world => outline => outline with { InnerWorld = world }),
        Lens<GeometryOutline, Swatch>.New(static outline => outline.InnerColor, static color => outline => outline with { InnerColor = color }),
        static _ => StrokePlacement.Center);

    public Lens<GeometryOutline, ShortSideLength> Width { get; }
    public Lens<GeometryOutline, Gated<Length>> World { get; }
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
        static (state, context) => context.Camera.Map<Func<Mat, Rectangle, Rectangle, Mat, Fin<Unit>>>(camera => (guide, first, second, edges) => {
            using Mat z = new();
            using Mat curvature = new();
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
            return z.Correlate(curvature, taps, DepthType.Cv64F, PaddingModes.Replicate).Map(_ => {
                CvInvoke.AbsDiff(curvature, zero, step);
                CvInvoke.Multiply(z, ratio, z);
                CvInvoke.Compare(step, z, marks, CmpType.GreaterThan);
                using Mat leading = new(marks, first);
                using Mat trailing = new(marks, second);
                CvInvoke.BitwiseAnd(leading, trailing, edges);
                return unit;
            });
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
            using Mat colour = Lanes.Colour();
            using Mat product = new();
            using Mat dot = new();
            using Mat lengths = new();
            using Mat partner = new();
            using ScalarArray bend = new(double.Cos(state.CreaseAngle));
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
    DepthStep Step, SignedAngle CreaseAngle,
    ShortSideLength OuterWidth, Gated<Length> OuterWorld, Swatch OuterColor, StrokePlacement Placement,
    ShortSideLength InnerWidth, Gated<Length> InnerWorld, Swatch InnerColor,
    Gated<Length> TaperNear, Ramp TaperRamp, Gated<Swatch> Paper, BlendingMode Mode)
    : IStateRecord<GeometryOutline, GeometryOutlineParameter, InvalidStylize>, IPixelStage<GeometryOutline> {
    public static GeometryOutline Default { get; } = Length.FromInches(0.25m) switch {
        var world => new(
        Some(LineClass.Outer), Some(LineClass.Inner), Some(LineClass.Outer), None,
        DepthStep.Create(0.05f), SignedAngle.Create(float.DegreesToRadians(180f - 134.4f)),
        ShortSideLength.Create(3f / ReferenceFrame.Height), new(Enabled: false, world), Swatch.Black, StrokePlacement.Center,
        ShortSideLength.Create(1.5f / ReferenceFrame.Height), new(Enabled: false, world), Swatch.Black,
        new(Enabled: false, Length.FromFeet(10)), Ramp.Grayscale, new(Enabled: false, Swatch.Black), BlendingMode.Mix),
    };

    public static Seq<GuideChannel> Channels(GeometryOutline state) =>
        GuideChannel.Depth.Cons(toSeq(OutlineSource.Items).Filter(source => source.Route.Get(state).IsSome).Map(static source => source.Channel)).Distinct();

    public static Option<PixelPass> Pass(GeometryOutline state, PassContext context) =>
        from depth in context.Guides.Find(GuideChannel.Depth)
        let routes = toSeq(OutlineSource.Items).Map(source => source.Route.Get(state).Map(line => (Source: source, Line: line))).Somes()
        where !routes.IsEmpty || state.Paper.Enabled
        select (PixelPass)new PixelPass.Frame((frame, progress) => Outlined(state, context, depth, routes, frame, progress));

    private static unsafe Fin<Unit> Outlined(
        GeometryOutline state, PassContext context, PixelFrame depth, Seq<(OutlineSource Source, LineClass Line)> routes, PixelFrame frame, IProgress<int> progress) => frame.Filter(_ => {
            Vector3[] colors = [.. toSeq(LineClass.Items).Map(line => line.Color.Get(state).Display.AsVector3())];
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
            return toSeq(LineClass.Items)
                .Map((line, channel) => (Line: line, Channel: channel, Sources: available.Filter(route => route.Line == line).Map(static route => (route.Edges, route.Guide))))
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
            if (marks[pixel] == 0)
                table[label++] = stroke(depths[pixel]);
        }
        return widths;
    }

    private static Fin<Func<float, double>> Width(GeometryOutline state, PassContext context, LineClass line) {
        double frame = line.Width.Get(state).Pixels(context.Extent);
        RampTable ramp = state.TaperRamp.Tabulate(context.Working);
        Fin<Func<float, double>> reach = line.World.Get(state).Active.Match(
            world => world == Length.Zero || frame == 0d ? Fin.Succ<Func<float, double>>(static _ => 0d) : context.Camera.ToFin(new CameraRequired()).Map(camera =>
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
[ValidationError<InvalidStylize>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class GeometryOutlineParameter : IStateParameter<GeometryOutline> {
    private static readonly (float Low, float High) MetresBounds = (0f, float.MaxValue);
    private static readonly Func<float, Fin<Length>> Metres = static value => value >= MetresBounds.Low && value <= MetresBounds.High ? Length.FromMeters(value) : Fin.Fail<Length>(new InvalidStylize());
    private static readonly Func<Length, float> MetresKey = static value => (float)value.Meters;
    private static readonly Presentation<float> Frame = ShortSideLength.Presentation with { Soft = (0f, 0.01f) };
    private static readonly Presentation<float> World = new() {
        Unit = Quantity.GetUnitInfo(LengthUnit.Meter), Soft = (0f, (float)Length.FromInches(1).Meters), Origin = (float)GeometryOutline.Default.OuterWorld.Value.Meters,
    };
    private static readonly Presentation<float> Distance = new() {
        Unit = Quantity.GetUnitInfo(LengthUnit.Meter), Soft = (0f, (float)Length.FromFeet(1000).Meters), Origin = (float)GeometryOutline.Default.TaperNear.Value.Meters,
    };

    public static readonly GeometryOutlineParameter Silhouette = new("silhouette-lines", new StateParameter<GeometryOutline>.OptionalChoice<LineClass, InvalidStylize>(OutlineSource.Silhouette.Route));
    public static readonly GeometryOutlineParameter Crease = new("crease-lines", new StateParameter<GeometryOutline>.OptionalChoice<LineClass, InvalidStylize>(OutlineSource.Crease.Route));
    public static readonly GeometryOutlineParameter Object = new("object-lines", new StateParameter<GeometryOutline>.OptionalChoice<LineClass, InvalidStylize>(OutlineSource.Object.Route));
    public static readonly GeometryOutlineParameter Material = new("material-lines", new StateParameter<GeometryOutline>.OptionalChoice<LineClass, InvalidStylize>(OutlineSource.Material.Route));
    public static readonly GeometryOutlineParameter Step = new("depth-step", StateParameter.Bounded<GeometryOutline, DepthStep, float, InvalidStylize>(Lens<GeometryOutline, DepthStep>.New(static outline => outline.Step, static step => outline => outline with { Step = step }), new() { Soft = (0f, 0.2f) }));
    public static readonly GeometryOutlineParameter CreaseAngle = new("crease-angle", StateParameter.Bounded<GeometryOutline, SignedAngle, float, InvalidPixelValue>(Lens<GeometryOutline, SignedAngle>.New(static outline => outline.CreaseAngle, static angle => outline => outline with { CreaseAngle = angle }),
    SignedAngle.Presentation with { Soft = (0f, SignedAngle.Up) }));
    public static readonly GeometryOutlineParameter OuterWidth = new("outer-width", StateParameter.Bounded<GeometryOutline, ShortSideLength, float, InvalidPixelValue>(LineClass.Outer.Width, Frame));
    public static readonly GeometryOutlineParameter OuterWorld = new("outer-world", new StateParameter<GeometryOutline>.OptionalNumeric<Length, float>(LineClass.Outer.World, Metres, MetresKey, MetresBounds, World));
    public static readonly GeometryOutlineParameter OuterColor = new("outer-color", new StateParameter<GeometryOutline>.Color(LineClass.Outer.Color));
    public static readonly GeometryOutlineParameter Placement = new("placement", new StateParameter<GeometryOutline>.Choice<StrokePlacement, InvalidStylize>(
        Lens<GeometryOutline, StrokePlacement>.New(static outline => outline.Placement, static placement => outline => outline with { Placement = placement })));
    public static readonly GeometryOutlineParameter InnerWidth = new("inner-width", StateParameter.Bounded<GeometryOutline, ShortSideLength, float, InvalidPixelValue>(LineClass.Inner.Width, Frame));
    public static readonly GeometryOutlineParameter InnerWorld = new("inner-world", new StateParameter<GeometryOutline>.OptionalNumeric<Length, float>(LineClass.Inner.World, Metres, MetresKey, MetresBounds, World));
    public static readonly GeometryOutlineParameter InnerColor = new("inner-color", new StateParameter<GeometryOutline>.Color(LineClass.Inner.Color));
    public static readonly GeometryOutlineParameter TaperNear = new("taper-near", new StateParameter<GeometryOutline>.OptionalNumeric<Length, float>(Lens<GeometryOutline, Gated<Length>>.New(static outline => outline.TaperNear, static depth => outline => outline with { TaperNear = depth }), Metres, MetresKey, MetresBounds, Distance));
    public static readonly GeometryOutlineParameter TaperRamp = new("taper-ramp", new StateParameter<GeometryOutline>.Gradient(
        Lens<GeometryOutline, Ramp>.New(static outline => outline.TaperRamp, static ramp => outline => outline with { TaperRamp = ramp })));
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

    public abstract Fin<Unit> Respond(Mat plane, Mat response);

    private protected Mat Across() => Taps(static (row, column) => (3 * (2 - row)) + column);

    private protected Mat Down() => Taps(static (row, column) => (3 * column) + 2 - row);

    private Mat Taps(Func<int, int, int> cell) {
        Mat taps = new(3, 3, DepthType.Cv32F, 1);
        taps.SetTo([.. toSeq(Range(0, 9)).Map(index => Kernel[cell(index / 3, index % 3)])]);
        return taps;
    }

    private sealed class Gradient(string key, Seq<float> kernel) : EdgeOperator(key, kernel) {
        public override float Gain => Steps.Fold(0f, static (gain, step) => float.Max(gain, float.Hypot(step.X, step.Y)));

        public override Fin<Unit> Respond(Mat plane, Mat response) {
            using Mat across = Across();
            using Mat down = Down();
            using Mat vertical = new(plane.Size, plane.Depth, plane.NumberOfChannels);
            return plane.Correlate(response, across, plane.Depth, PaddingModes.Replicate)
                .Bind(_ => plane.Correlate(vertical, down, plane.Depth, PaddingModes.Replicate))
                .Map(_ => {
                    CvInvoke.Multiply(response, response, response);
                    CvInvoke.Multiply(vertical, vertical, vertical);
                    CvInvoke.Add(response, vertical, response);
                    CvInvoke.Sqrt(response, response);
                    return unit;
                });
        }
    }

    private sealed class Relief(string key, Seq<float> kernel) : EdgeOperator(key, kernel) {
        public override float Gain => Steps.Fold(0f, static (gain, step) => float.Max(gain, step.X));

        public override Fin<Unit> Respond(Mat plane, Mat response) {
            using Mat across = Across();
            using ScalarArray floor = new(0d);
            return plane.Correlate(response, across, plane.Depth, PaddingModes.Replicate).Map(_ => {
                CvInvoke.Max(response, floor, response);
                return unit;
            });
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
            (plane[3 * x], plane[(3 * x) + 1], plane[(3 * x) + 2]) = (row[x].X, row[x].Y, row[x].Z);
    });
    public static readonly EdgeSource Luma = new("luma", 1, static (row, plane, weights) => {
        for (int x = 0; x < row.Length; x++)
            plane[x] = (weights.X * row[x].X) + (weights.Y * row[x].Y) + (weights.Z * row[x].Z);
    });
    public static readonly EdgeSource Hue = new("hue", 2, static (row, plane, _) => {
        for (int x = 0; x < row.Length; x++)
            (plane[2 * x], plane[(2 * x) + 1]) = double.SinCos(Hsy.From(row[x]).Hue) switch { var (sine, cosine) => (cosine / double.Tau, sine / double.Tau) };
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
    public partial void Read(ReadOnlySpan<Vector4> row, Span<double> plane, (double X, double Y, double Z) weights);
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

    private static unsafe Fin<Unit> Detected(EdgeDetect state, PassContext context, PixelFrame frame, IProgress<int> progress) => frame.Filter(_ => {
        (int width, int planes, double sigma) = (frame.Size.Width, state.Source.Planes, state.Softening.Pixels(context.Extent));
        return PixelSampling.GaussianTaps<long>(sigma).Bind(taps => {
            using Mat plane = new(frame.Size.Height, width, DepthType.Cv64F, planes);
            using Mat response = new(frame.Size.Height, width, DepthType.Cv64F, planes);
            using Mat squares = new();
            return new PixelPass.Pointwise((row, _, line) =>
                    state.Source.Read(row, new Span<double>(plane.GetDataPointer(frame.RowIndex(line)).ToPointer(), width * planes), context.Working.Luminance))
                .Run(frame, progress)
                .Bind(_ => sigma > 0d ? plane.Gaussian(plane, (taps, taps), sigma, sigma, PaddingModes.Replicate) : Fin.Succ(unit))
                .Bind(_ => state.Operator.Respond(plane, response))
                .Bind(_ => {
                    if (state.Source != EdgeSource.Color) {
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
    });

    private static unsafe Fin<Unit> Magnitudes(PixelFrame frame, Mat edges, IProgress<int> progress) =>
        new PixelPass.Pointwise((row, _, line) => {
            int channels = edges.NumberOfChannels;
            ReadOnlySpan<double> responses = new(edges.GetDataPointer(frame.RowIndex(line)).ToPointer(), channels * row.Length);
            for (int x = 0; x < row.Length; x++)
                row[x] = new Vector4(channels == 1 ? new Vector3((float)responses[x]) : new Vector3((float)responses[3 * x], (float)responses[(3 * x) + 1], (float)responses[(3 * x) + 2]), row[x].W);
        }).Run(frame, progress);

    private static unsafe Fin<Unit> Stroked(EdgeDetect state, PassContext context, PixelFrame frame, Mat edges, Swatch ground, float floor, IProgress<int> progress) {
        (Vector3 line, Vector3 under, double stroke) = (state.Line.Display.AsVector3(), ground.Display.AsVector3(), state.Width.Pixels(context.Extent));
        using Mat value = new();
        using Mat mask = new();
        using Mat reach = new();
        using ScalarArray threshold = new(state.Threshold * state.Operator.Gain);
        if (state.Source == EdgeSource.Color) {
            (double X, double Y, double Z) = context.Working.Luminance;
            using Mat weights = new(1, EdgeSource.Color.Planes, DepthType.Cv64F, 1);
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
[ValidationError<InvalidStylize>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class EdgeDetectParameter : IStateParameter<EdgeDetect> {
    private static readonly Presentation<float> Span = ShortSideLength.Presentation with { Soft = (0f, 0.01f) };

    public static readonly EdgeDetectParameter Operator = new("operator", new StateParameter<EdgeDetect>.Choice<EdgeOperator, InvalidStylize>(
        Lens<EdgeDetect, EdgeOperator>.New(static detect => detect.Operator, static kernel => detect => detect with { Operator = kernel })));
    public static readonly EdgeDetectParameter Source = new("source", new StateParameter<EdgeDetect>.Choice<EdgeSource, InvalidStylize>(
        Lens<EdgeDetect, EdgeSource>.New(static detect => detect.Source, static source => detect => detect with { Source = source })));
    public static readonly EdgeDetectParameter Softening = new("softening", StateParameter.Bounded<EdgeDetect, ShortSideLength, float, InvalidPixelValue>(Lens<EdgeDetect, ShortSideLength>.New(static detect => detect.Softening, static softening => detect => detect with { Softening = softening }), Span));
    public static readonly EdgeDetectParameter Rendering = new("rendering", new StateParameter<EdgeDetect>.Choice<EdgeRendering, InvalidStylize>(
        Lens<EdgeDetect, EdgeRendering>.New(static detect => detect.Rendering, static rendering => detect => detect with { Rendering = rendering })));
    public static readonly EdgeDetectParameter Threshold = new("threshold", StateParameter.Bounded<EdgeDetect, EdgeThreshold, float, InvalidStylize>(Lens<EdgeDetect, EdgeThreshold>.New(static detect => detect.Threshold, static threshold => detect => detect with { Threshold = threshold }), new()));
    public static readonly EdgeDetectParameter Width = new("width", StateParameter.Bounded<EdgeDetect, ShortSideLength, float, InvalidPixelValue>(Lens<EdgeDetect, ShortSideLength>.New(static detect => detect.Width, static width => detect => detect with { Width = width }), Span));
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
    public static readonly KuwaharaKind Classic = new("classic", static pixels => Math.Floor(pixels), Quadrants);
    public static readonly KuwaharaKind Anisotropic = new("anisotropic", static pixels => pixels, Sectors);

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
                    (int left, int right) = (quadrant & 1) == 0 ? ((int)double.Max(0d, x - radius), x) : (x, (int)double.Min(width - 1, x + radius));
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

    private static unsafe Fin<Unit> Sectors(Painterly state, PixelExtent extent, PixelFrame frame, IProgress<int> progress) => frame.Filter(source =>
        PixelSampling.KernelTaps<long>(state.Uniformity.Pixels(extent)).Bind(taps => {
            const int count = 8;
            const double corner = 0.182d;
            (int width, int height, double radius, double uniformity) = (frame.Size.Width, frame.Size.Height, state.Kind.Radius(state.Size.Pixels(extent)), state.Uniformity.Pixels(extent));
            (double sharpness, double overlap) = (16d * state.Sharpness * state.Sharpness, 2d / radius);
            (double sine, double cosine) = double.SinCos(1.5d * double.Pi / count);
            double envelope = (overlap + cosine) / (sine * sine);
            double diagonal = double.Sqrt(2d);
            using Mat copy = new();
            using Mat across = new(3, 3, DepthType.Cv64F, 1);
            using Mat down = new();
            using Mat colour = Lanes.Colour();
            using Mat horizontal = new(source.Size, DepthType.Cv64F, source.NumberOfChannels);
            using Mat vertical = new(source.Size, DepthType.Cv64F, source.NumberOfChannels);
            using Mat product = new();
            using Mat xx = new();
            using Mat xy = new();
            using Mat yy = new();
            source.ConvertTo(copy, DepthType.Cv64F);
            across.SetTo([-corner, 0d, corner, (2d * corner) - 1d, 0d, 1d - (2d * corner), -corner, 0d, corner]);
            CvInvoke.Transpose(across, down);
            return copy.Correlate(horizontal, across, DepthType.Cv64F, PaddingModes.Replicate)
                .Bind(_ => copy.Correlate(vertical, down, DepthType.Cv64F, PaddingModes.Replicate))
                .Bind(_ => Seq((horizontal, horizontal, xx), (horizontal, vertical, xy), (vertical, vertical, yy)).TraverseM(pair => {
                    (Mat first, Mat second, Mat tensor) = pair;
                    CvInvoke.Multiply(first, second, product);
                    CvInvoke.Transform(product, tensor, colour);
                    return uniformity > 0d ? tensor.Gaussian(tensor, (taps, taps), uniformity / 3d, uniformity / 3d, PaddingModes.Replicate) : Fin.Succ(unit);
                }).As()).Bind(_ => {
                    using MemoryOwner<Vector256<double>> sectors = MemoryOwner<Vector256<double>>.Allocate(2 * count);
                    using MemoryOwner<double> deviations = MemoryOwner<double>.Allocate(count);
                    Fin<Unit> Render(int x, int y) {
                        ReadOnlySpan<double> dxx = new(xx.GetDataPointer(y).ToPointer(), width);
                        ReadOnlySpan<double> dxy = new(xy.GetDataPointer(y).ToPointer(), width);
                        ReadOnlySpan<double> dyy = new(yy.GetDataPointer(y).ToPointer(), width);
                        (double half, double root) = ((dxx[x] + dyy[x]) / 2d, double.Hypot(dxx[x] - dyy[x], 2d * dxy[x]) / 2d);
                        (double axisY, double axisX) = double.SinCos(double.Atan2(-2d * dxy[x], dyy[x] - dxx[x]) / 2d);
                        double stretch = 1d + (state.Eccentricity * (half > 0d ? root / half : 0d));
                        (double major, double minor) = (stretch * radius, radius / stretch);
                        (double acrossReach, double downReach) = (double.Ceiling(double.Hypot(major * axisX, minor * axisY)), double.Ceiling(double.Hypot(major * axisY, minor * axisX)));
                        return Try.lift(() => (int.CreateChecked(acrossReach), int.CreateChecked(downReach))).Run()
                            .MapFail(_ => new KuwaharaFootprintUnsupported(acrossReach, downReach))
                            .Bind(support => {
                                (int reachX, int reachY) = support;
                                Span<Vector256<double>> means = sectors.Span[..count];
                                Span<Vector256<double>> spreads = sectors.Span[count..];
                                ReadOnlySpan2D<Vector256<double>> image = new ReadOnlySpan<Vector256<double>>(copy.DataPointer.ToPointer(), width * height).AsSpan2D(height, width);
                                means.Fill(image[y, x].WithElement(3, 1d / count));
                                spreads.Clear();
                                for (long cell = reachX + 1L, span = (2L * reachX) + 1; cell < span * (reachY + 1L); cell++) {
                                    (int i, int j) = ((int)((cell % span) - reachX), (int)(cell / span));
                                    (double diskX, double diskY) = (((axisX * i) + (axisY * j)) / major, ((axisX * j) - (axisY * i)) / minor);
                                    double distance = (diskX * diskX) + (diskY * diskY);
                                    if (distance > 1d)
                                        continue;
                                    Vector256<double> lanes = Vector256.Create(diskX, (diskX - diskY) / diagonal, diskY, (diskX + diskY) / diagonal);
                                    Vector256<double> bend = Vector256.Create(overlap) - (envelope * lanes * lanes);
                                    Vector256<double> swing = Vector256.Create(lanes.GetElement(2), lanes.GetElement(3), -lanes.GetElement(0), -lanes.GetElement(1));
                                    (Vector256<double> low, Vector256<double> high) = (Vector256.Max(bend + swing, Vector256<double>.Zero), Vector256.Max(bend - swing, Vector256<double>.Zero));
                                    double radial = double.Exp(-double.Pi * distance) / Vector256.Sum((low * low) + (high * high));
                                    (low, high) = (low * low * radial, high * high * radial);
                                    Vector256<double> upper = image[(int)long.Clamp((long)y + j, 0, height - 1), (int)long.Clamp((long)x + i, 0, width - 1)];
                                    Vector256<double> lower = image[(int)long.Clamp((long)y - j, 0, height - 1), (int)long.Clamp((long)x - i, 0, width - 1)];
                                    for (int k = 0; k < count; k++) {
                                        (double first, double second) = k < count / 2 ? (low.GetElement(k), high.GetElement(k)) : (high.GetElement(k - (count / 2)), low.GetElement(k - (count / 2)));
                                        (means[k], spreads[k]) = Accumulated(means[k], spreads[k], upper, first);
                                        (means[k], spreads[k]) = Accumulated(means[k], spreads[k], lower, second);
                                    }
                                }
                                for (int k = 0; k < count; k++) {
                                    Vector256<double> deviation = Vector256.Sqrt(spreads[k] / means[k].GetElement(3));
                                    deviations.Span[k] = double.Max(0.02d, Vector256.Sum(deviation));
                                }
                                double minimum = deviations.Span[0];
                                for (int k = 1; k < count; k++)
                                    minimum = double.Min(minimum, deviations.Span[k]);
                                (Vector256<double> total, double weight) = (Vector256<double>.Zero, 0d);
                                for (int k = 0; k < count; k++) {
                                    double share = double.Pow(minimum / deviations.Span[k], sharpness);
                                    (total, weight) = (total + (means[k] * share), weight + share);
                                }
                                Vector256<double> mean = total / weight;
                                Span<Vector4> row = frame.View.Span.GetRowSpan(y);
                                row[x] = new Vector4((float)mean.GetElement(0), (float)mean.GetElement(1), (float)mean.GetElement(2), row[x].W);
                                return Fin.Succ(unit);
                            });
                    }
                    for (int y = 0; y < height; y++) {
                        for (int x = 0; x < width; x++) {
                            Fin<Unit> rendered = Render(x, y);
                            if (rendered.IsFail) return rendered;
                        }
                        progress.Report(y + 1);
                    }
                    return Fin.Succ(unit);
                });
        }));

    private static (Vector256<double> Mean, Vector256<double> Spread) Accumulated(Vector256<double> mean, Vector256<double> spread, Vector256<double> sample, double share) =>
        (mean.GetElement(3), (sample - mean).WithElement(3, 0d)) switch {
            var (weight, delta) => ((mean + (delta * (share / (weight + share)))).WithElement(3, weight + share), spread + (delta * delta * (weight * share / (weight + share)))),
        };
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
    public static readonly PainterlyParameter Size = new("size", StateParameter.Bounded<Painterly, ShortSideLength, float, InvalidPixelValue>(Lens<Painterly, ShortSideLength>.New(static painterly => painterly.Size, static size => painterly => painterly with { Size = size }),
    ShortSideLength.Presentation with { Soft = (0f, 0.02f) }));
    public static readonly PainterlyParameter Uniformity = new("uniformity", StateParameter.Bounded<Painterly, ShortSideLength, float, InvalidPixelValue>(Lens<Painterly, ShortSideLength>.New(static painterly => painterly.Uniformity, static uniformity => painterly => painterly with { Uniformity = uniformity }),
    ShortSideLength.Presentation with { Soft = (0f, 0.01f) }));
    public static readonly PainterlyParameter Sharpness = new("sharpness", StateParameter.Bounded<Painterly, SectorSharpness, float, InvalidStylize>(Lens<Painterly, SectorSharpness>.New(static painterly => painterly.Sharpness, static sharpness => painterly => painterly with { Sharpness = sharpness }), new()));
    public static readonly PainterlyParameter Eccentricity = new("eccentricity", StateParameter.Bounded<Painterly, SectorEccentricity, float, InvalidStylize>(Lens<Painterly, SectorEccentricity>.New(static painterly => painterly.Eccentricity, static eccentricity => painterly => painterly with { Eccentricity = eccentricity }), new()));

    public StateParameter<Painterly> Kind { get; }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
file static class Lanes {
    public static Mat Colour() {
        Mat sums = new(1, 4, DepthType.Cv32F, 1);
        sums.SetTo([1f, 1f, 1f, 0f]);
        return sums;
    }
}
