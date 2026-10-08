using System.Drawing;
using System.Numerics;
using CommunityToolkit.HighPerformance;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Rasm.Imaging.Grade;
using Rasm.Imaging.Pixels;
using UnitsNet;
using UnitsNet.Units;

namespace Rasm.Imaging.Filters.Optics;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<float>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Off", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidOptics>]
public readonly partial struct DefocusRadius : IMinMaxValue<DefocusRadius> {
    public static DefocusRadius MinValue => Off;
    public static DefocusRadius MaxValue { get; } = new(10f * 0.01f * 1920f / 1080f);

    static partial void ValidateFactoryArguments(ref InvalidOptics? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidOptics();
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidOptics>]
public readonly partial struct FocusAspect : IMinMaxValue<FocusAspect> {
    public static FocusAspect MinValue => Band;
    public static FocusAspect MaxValue => Round;
    public static FocusAspect Band { get; } = new(0f);
    public static FocusAspect Round { get; } = new(1f);

    static partial void ValidateFactoryArguments(ref InvalidOptics? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidOptics();
}

[SmartEnum<string>]
[ValidationError<InvalidOptics>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FocusField {
    public static readonly FocusField Uniform = new("uniform");
    public static readonly FocusField Depth = new("depth");
    public static readonly FocusField Curvature = new("curvature");
    public static readonly FocusField Tilt = new("tilt");
}

public sealed record FocusRegion(FramePosition Center, SignedAngle Angle, ShortSideLength Extent, FocusAspect Aspect, ShortSideExtent Feather);

public sealed record Defocus(FocusField Field, DefocusRadius Radius, FNumber Aperture, FocusRegion Region, Iris Iris)
    : IStateRecord<Defocus, DefocusParameter, InvalidOptics>, IPixelStage<Defocus> {
    public static Defocus Default { get; } = new(
        FocusField.Depth,
        DefocusRadius.Create(0.02f * 1920f / 1080f),
        FNumber.Create(2.8f),
        new FocusRegion(FramePosition.Center, SignedAngle.Neutral, ShortSideLength.Create(0.3f / 2f), FocusAspect.Round, ShortSideExtent.Create(0.5f / 2f)),
        Iris.Hexagon with { Roundness = AxisFraction.MaxValue });

    public static Seq<GuideChannel> Channels(Defocus state) =>
        state.Field.Map<Seq<GuideChannel>>(uniform: [], depth: [GuideChannel.Depth], curvature: [], tilt: []);

    public static Option<PixelPass> Pass(Defocus state, PassContext context) =>
        state.Radius == DefocusRadius.Off
            ? None
            : state.Field.Switch(
                (State: state, Context: context, Cap: state.Radius * int.Min(context.Extent.Width, context.Extent.Height)),
                uniform: static s => Some<PixelPass>(Uniform(s.State.Iris, s.Cap)),
                depth: static s =>
                    from depth in s.Context.Guides.Find(GuideChannel.Depth)
                    from camera in s.Context.Camera
                    from lens in camera.Lens
                    from window in camera.Frustum.Switch(perspective: static frustum => Some(frustum.Window), parallel: static _ => Option<ViewWindow>.None)
                    select (PixelPass)Gathered(s.State.Iris, s.Cap, Depth(depth, lens, s.State.Aperture, window, s.Context.Extent, s.Cap)),
                curvature: static s => Some<PixelPass>(Gathered(s.State.Iris, s.Cap, Framed(s.State.Region, s.State.Region.Aspect, 2, s.Context.Extent, s.Cap))),
                tilt: static s => Some<PixelPass>(Gathered(s.State.Iris, s.Cap, Framed(s.State.Region, FocusAspect.Band, 1, s.Context.Extent, s.Cap))));

    internal static void Correlate(Mat plane, Iris iris, float radius, int component) {
        int reach = (int)float.Ceiling(radius * iris.Circumradius);
        using Mat kernel = new((2 * reach) + 1, (2 * reach) + 1, DepthType.Cv32F, 1);
        Span<float> taps = kernel.GetSpan<float>();
        for (int i = 0; i < taps.Length; i++) {
            (float outline, float edge) = iris.Outline(Mirrored(i, reach));
            taps[i] = i == taps.Length / 2 ? 1f : iris.Weights(outline, edge, radius)[component];
        }
        CvInvoke.Normalize(kernel, kernel, 1d, 0d, NormType.L1);
        CvInvoke.Filter2D(plane, plane, kernel, new Point(-1, -1), 0d, BorderType.Replicate);
    }

    private static PixelPass.Frame Uniform(Iris iris, float cap) =>
        new((frame, progress) => {
            using Mat source = Premultiplied(frame);
            using Mat plane = new();
            for (int component = 0; component < 4; component++) {
                CvInvoke.ExtractChannel(source, plane, component);
                Correlate(plane, iris, cap, component);
                CvInvoke.InsertChannel(plane, source, component);
            }
            return new PixelPass.Pointwise((row, _, line) => {
                ReadOnlySpan<Vector4> light = source.GetSpan<Vector4>().AsSpan2D(source.Rows, source.Cols).GetRowSpan(frame.Line(line));
                for (int x = 0; x < row.Length; x++)
                    row[x] = Straight(light[x]);
            }).Run(frame, progress);
        });

    private static PixelPass.Frame Gathered(Iris iris, float cap, Action<Mat, Point> field) =>
        new((frame, progress) => {
            float circumradius = iris.Circumradius;
            int reach = (int)float.Ceiling(cap * circumradius);
            int side = (2 * reach) + 1;
            using Mat source = Premultiplied(frame);
            using Mat table = new(side, side, DepthType.Cv32F, 2);
            using Mat radii = new(frame.Size.Height, frame.Size.Width, DepthType.Cv32F, 1);
            Span<Vector2> outlines = table.GetSpan<Vector2>();
            for (int i = 0; i < outlines.Length; i++)
                outlines[i] = iris.Outline(Mirrored(i, reach)) switch { var (outline, edge) => new Vector2(outline, edge) };
            field(radii, frame.Origin);
            radii.MinMax(out double[] least, out _, out _, out _);
            float near = float.Max(0f, -(float)least[0]);
            return new PixelPass.Pointwise((row, _, line) => {
                ReadOnlySpan2D<Vector4> light = source.GetSpan<Vector4>().AsSpan2D(source.Rows, source.Cols);
                ReadOnlySpan2D<float> radius = radii.GetSpan<float>().AsSpan2D(radii.Rows, radii.Cols);
                ReadOnlySpan<Vector2> window = table.GetSpan<Vector2>();
                int y = frame.Line(line);
                for (int x = 0; x < row.Length; x++) {
                    float own = radius[y, x];
                    int span = (2 * (int)float.Ceiling(float.Max(float.Abs(own), near) * circumradius)) + 1;
                    (Vector4 sum, Vector4 total) = (light[y, x], Vector4.One);
                    for (int k = 0; k < span * span; k++) {
                        (int dx, int dy) = ((k % span) - (span / 2), (k / span) - (span / 2));
                        (int u, int v) = (int.Clamp(x + dx, 0, radius.Width - 1), int.Clamp(y + dy, 0, radius.Height - 1));
                        float other = radius[v, u];
                        float r = float.Abs(other < own ? other : float.MinMagnitude(own, other));
                        Vector2 outline = window[((dy + reach) * side) + dx + reach];
                        Vector4 weight = (dx | dy) != 0 && outline.X < r ? iris.Weights(outline.X, outline.Y, r) : Vector4.Zero;
                        (sum, total) = (sum + (weight * light[v, u]), total + weight);
                    }
                    row[x] = Straight(sum / total);
                }
            }).Run(frame, progress);
        });

    private static Action<Mat, Point> Framed(FocusRegion region, FocusAspect aspect, int power, PixelExtent size, float cap) {
        Vector2 middle = new Vector2(size.Width, size.Height) / 2f;
        float shortSide = int.Min(size.Width, size.Height);
        Vector2 center = (region.Center.Point(size) - middle) / shortSide;
        (float sin, float cos) = float.SinCos(region.Angle);
        return (plane, origin) => {
            Span2D<float> radius = plane.GetSpan<float>().AsSpan2D(plane.Rows, plane.Cols);
            for (int y = 0; y < radius.Height; y++) {
                for (int x = 0; x < radius.Width; x++) {
                    Vector2 d = ((new Vector2(origin.X + x + 0.5f, origin.Y + y + 0.5f) - middle) / shortSide) - center;
                    float distance = float.Hypot((d.X * sin) + (d.Y * cos), aspect * ((d.X * cos) - (d.Y * sin)));
                    radius[y, x] = cap * float.Pow(float.Min(float.Max(distance - region.Extent, 0f) / region.Feather, 1f), power);
                }
            }
        };
    }

    private static Action<Mat, Point> Depth(PixelFrame depth, LensFocus lens, FNumber aperture, ViewWindow window, PixelExtent size, float cap) {
        (float focal, float focus) = (lens.FocalLength, lens.FocusDistance);
        float k = focal * int.Min(size.Width, size.Height) / (2f * aperture * (focus - focal) * float.Min(window.Right - window.Left, window.Top - window.Bottom));
        int taps = (2 * (int)float.Ceiling(cap)) + 1;
        return (plane, _) => {
            using Mat header = depth.Header();
            using Mat magnitude = new();
            using Mat blur = new();
            using ScalarArray zero = new(0d);
            CvInvoke.ExtractChannel(header, plane, 0);
            Span<float> radius = plane.GetSpan<float>();
            foreach (ref float value in radius)
                value = float.Clamp(k * (1f - (focus / value)), -cap, cap);
            CvInvoke.AbsDiff(plane, zero, magnitude);
            CvInvoke.GaussianBlur(magnitude, blur, new Size(taps, taps), cap / 3f, cap / 3f, BorderType.Replicate);
            CvInvoke.Min(blur, magnitude, blur);
            ReadOnlySpan<float> eroded = blur.GetSpan<float>();
            for (int i = 0; i < radius.Length; i++)
                radius[i] = float.CopySign(eroded[i], radius[i]);
        };
    }

    private static Mat Premultiplied(PixelFrame frame) {
        using Mat header = frame.Header();
        using Mat alpha = new();
        using Mat ones = new(frame.Size.Height, frame.Size.Width, DepthType.Cv32F, 1);
        CvInvoke.ExtractChannel(header, alpha, 3);
        ones.SetTo(new MCvScalar(1d));
        using VectorOfMat planes = new(alpha, alpha, alpha, ones);
        Mat source = new();
        CvInvoke.Merge(planes, source);
        CvInvoke.Multiply(header, source, source);
        return source;
    }

    private static Vector2 Mirrored(int index, int reach) => new(reach - (index % ((2 * reach) + 1)), (index / ((2 * reach) + 1)) - reach);

    private static Vector4 Straight(Vector4 light) => light.W > 0f ? new Vector4(light.AsVector3() / light.W, light.W) : Vector4.Zero;
}

[SmartEnum<string>]
[ValidationError<InvalidOptics>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class DefocusParameter : IStateParameter<Defocus> {
    private static readonly Lens<Defocus, FocusRegion> Region =
        Lens<Defocus, FocusRegion>.New(static defocus => defocus.Region, static region => defocus => defocus with { Region = region });
    private static readonly (StateParameter<Defocus> X, StateParameter<Defocus> Y) Position =
        FramePosition.Kinds(lens(Region, Lens<FocusRegion, FramePosition>.New(static region => region.Center, static center => region => region with { Center = center })));
    private static readonly Lens<Defocus, Iris> Diaphragm = Lens<Defocus, Iris>.New(static defocus => defocus.Iris, static iris => defocus => defocus with { Iris = iris });
    private static readonly (StateParameter<Defocus> Blades, StateParameter<Defocus> Rotation, StateParameter<Defocus> Roundness, StateParameter<Defocus> Obstruction, StateParameter<Defocus> Squeeze)
        Outline = Iris.Kinds(Diaphragm);

    public static readonly DefocusParameter Field = new(
        "field",
        new StateParameter<Defocus>.Choice<FocusField, InvalidOptics>(Lens<Defocus, FocusField>.New(static defocus => defocus.Field, static field => defocus => defocus with { Field = field })));
    public static readonly DefocusParameter Radius = new(
        "radius",
        new StateParameter<Defocus>.Bounded<DefocusRadius, float, InvalidOptics>(
            Lens<Defocus, DefocusRadius>.New(static defocus => defocus.Radius, static radius => defocus => defocus with { Radius = radius }),
            new() { Unit = Quantity.GetUnitInfo(RatioUnit.DecimalFraction) }));
    public static readonly DefocusParameter Aperture = new(
        "f-stop",
        new StateParameter<Defocus>.Bounded<FNumber, float, InvalidPixelValue>(
            Lens<Defocus, FNumber>.New(static defocus => defocus.Aperture, static aperture => defocus => defocus with { Aperture = aperture }),
            new() { Soft = (0.1f, 128f), Decimals = 1 }));
    public static readonly DefocusParameter CenterX = new("center-x", Position.X);
    public static readonly DefocusParameter CenterY = new("center-y", Position.Y);
    public static readonly DefocusParameter Angle = new(
        "angle",
        new StateParameter<Defocus>.Bounded<SignedAngle, float, InvalidPixelValue>(
            lens(Region, Lens<FocusRegion, SignedAngle>.New(static region => region.Angle, static angle => region => region with { Angle = angle })),
            new() { Unit = Quantity.GetUnitInfo(AngleUnit.Radian), Origin = (float)SignedAngle.Neutral }));
    public static readonly DefocusParameter Extent = new(
        "extent",
        new StateParameter<Defocus>.Bounded<ShortSideLength, float, InvalidPixelValue>(
            lens(Region, Lens<FocusRegion, ShortSideLength>.New(static region => region.Extent, static extent => region => region with { Extent = extent })),
            new() { Unit = Quantity.GetUnitInfo(RatioUnit.DecimalFraction), Soft = (0f, 1f) }));
    public static readonly DefocusParameter Aspect = new(
        "aspect",
        new StateParameter<Defocus>.Bounded<FocusAspect, float, InvalidOptics>(
            lens(Region, Lens<FocusRegion, FocusAspect>.New(static region => region.Aspect, static aspect => region => region with { Aspect = aspect })), new()));
    public static readonly DefocusParameter Feather = new(
        "feather",
        new StateParameter<Defocus>.Bounded<ShortSideExtent, float, InvalidPixelValue>(
            lens(Region, Lens<FocusRegion, ShortSideExtent>.New(static region => region.Feather, static feather => region => region with { Feather = feather })),
            new() { Unit = Quantity.GetUnitInfo(RatioUnit.DecimalFraction), Soft = (ShortSideExtent.MinValue, 1f) }));
    public static readonly DefocusParameter Blades = new("blades", Outline.Blades);
    public static readonly DefocusParameter Rotation = new("rotation", Outline.Rotation);
    public static readonly DefocusParameter Roundness = new("roundness", Outline.Roundness);
    public static readonly DefocusParameter Obstruction = new("obstruction", Outline.Obstruction);
    public static readonly DefocusParameter Squeeze = new("squeeze", Outline.Squeeze);
    public static readonly DefocusParameter Fringe = new("fringe", Iris.FringeKind(Diaphragm));

    public StateParameter<Defocus> Kind { get; }
}
