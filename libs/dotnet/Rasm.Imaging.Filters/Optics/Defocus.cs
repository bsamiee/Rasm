using System.Drawing;
using System.Numerics;
using CommunityToolkit.HighPerformance;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Rasm.Imaging.Filters.Generators;
using Rasm.Imaging.Grade;
using Rasm.Imaging.Pixels;
using SixLabors.ImageSharp.PixelFormats;
using UnitsNet;
using UnitsNet.Units;

namespace Rasm.Imaging.Filters.Optics;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<float>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Off", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidOptics>]
public readonly partial struct DefocusRadius : IMinMaxValue<DefocusRadius> {
    public static DefocusRadius MinValue => Off;
    public static DefocusRadius MaxValue { get; } = new(0.1f * ReferenceFrame.GreaterSide);

    static partial void ValidateFactoryArguments(ref InvalidOptics? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidOptics();
}

[ValueObject<float>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Neutral", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidOptics>]
public readonly partial struct FocusAngle : IMinMaxValue<FocusAngle> {
    public static FocusAngle MinValue { get; } = new(-float.Pi / 2f);
    public static FocusAngle MaxValue { get; } = new(float.Pi / 2f);

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

public sealed record Defocus(FocusField Field, DefocusRadius Radius, FNumber Aperture, FramePosition Center, FocusAngle Angle, ShortSideLength Extent, AxisFraction Aspect, ShortSideExtent Feather, Iris Iris)
    : IStateRecord<Defocus, DefocusParameter, InvalidOptics>, IPixelStage<Defocus> {
    public static Defocus Default { get; } = new(
        FocusField.Depth, DefocusRadius.Create(0.02f * ReferenceFrame.GreaterSide), FNumber.Create(2.8f),
        FramePosition.Center, FocusAngle.Neutral, ShortSideLength.Create(0.3f / 2f), AxisFraction.MaxValue, ShortSideExtent.Create(0.5f / 2f),
        Iris.Hexagon with { Roundness = AxisFraction.MaxValue });

    public static Seq<GuideChannel> Channels(Defocus state) =>
        state.Field.Map<Seq<GuideChannel>>(uniform: [], depth: [GuideChannel.Depth], curvature: [], tilt: []);

    public static Option<PixelPass> Pass(Defocus state, PassContext context) =>
        state.Radius == DefocusRadius.Off
            ? None
            : state.Field.Switch(
                (State: state, Context: context, Cap: state.Radius * context.Extent.ShortSide),
                uniform: static s => Some<PixelPass>(Blur(s.State.Iris, s.Cap, None)),
                depth: static s =>
                    from depth in s.Context.Guides.Find(GuideChannel.Depth)
                    from camera in s.Context.Camera
                    from lens in camera.Lens
                    from window in camera.Frustum.Switch(perspective: static frustum => Some(frustum.Window), parallel: static _ => Option<ViewWindow>.None)
                    select (PixelPass)Blur(s.State.Iris, s.Cap, Some(Depth(depth, lens, s.State.Aperture, window, s.Context.Extent, s.Cap))),
                curvature: static s => Some<PixelPass>(Blur(s.State.Iris, s.Cap, Some(Framed(s.State, s.State.Aspect, 2, s.Context.Extent, s.Cap)))),
                tilt: static s => Some<PixelPass>(Blur(s.State.Iris, s.Cap, Some(Framed(s.State, AxisFraction.MinValue, 1, s.Context.Extent, s.Cap)))));

    internal static void Correlate(Mat plane, Iris iris, float radius, int component) {
        int reach = (int)float.Ceiling(radius * iris.Circumradius);
        using Mat kernel = new((2 * reach) + 1, (2 * reach) + 1, DepthType.Cv32F, 1);
        int count = kernel.Rows * kernel.Cols;
        kernel.SetTo(Range(0, count).Select(i => i == count / 2 ? 1f : iris.Outline(Mirrored(i, reach)) switch { var (outline, edge) => iris.Weights(outline, edge, radius)[component] }).ToArray());
        CvInvoke.Normalize(kernel, kernel, 1d, 0d, NormType.L1);
        CvInvoke.Filter2D(plane, plane, kernel, new Point(-1, -1), 0d, BorderType.Replicate);
    }

    private static PixelPass.Frame Blur(Iris iris, float cap, Option<Action<Mat, Point>> field) =>
        new((frame, progress) => {
            using Mat source = new(frame.Size.Height, frame.Size.Width, DepthType.Cv32F, 4);
            PixelOperations<RgbaVector>.Instance.ToVector4(SixLabors.ImageSharp.Configuration.Default, frame.Block.Cast<float, RgbaVector>(), source.GetSpan<Vector4>(), PixelConversionModifiers.Premultiply);
            return field.Match(write => Gathered(source, iris, cap, write, frame, progress), () => Uniform(source, iris, cap, frame, progress));
        });

    private static Fin<Unit> Uniform(Mat source, Iris iris, float cap, PixelFrame frame, IProgress<int> progress) {
        using Mat plane = new();
        for (int component = 0; component < source.NumberOfChannels; component++) {
            CvInvoke.ExtractChannel(source, plane, component);
            Correlate(plane, iris, cap, component);
            CvInvoke.InsertChannel(plane, source, component);
        }
        return new PixelPass.Pointwise((row, _, line) => {
            ReadOnlySpan<Vector4> light = source.GetSpan<Vector4>().AsSpan2D(source.Rows, source.Cols).GetRowSpan(frame.Line(line));
            for (int x = 0; x < row.Length; x++) row[x] = Straight(light[x]);
        }).Run(frame, progress);
    }

    private static Fin<Unit> Gathered(Mat source, Iris iris, float cap, Action<Mat, Point> write, PixelFrame frame, IProgress<int> progress) {
        float circumradius = iris.Circumradius;
        int reach = (int)float.Ceiling(cap * circumradius);
        int side = (2 * reach) + 1;
        using Mat radii = new(frame.Size.Height, frame.Size.Width, DepthType.Cv32F, 1);
        (float Reach, float Edge)[] outlines = [.. Range(0, side * side).Select(i => iris.Outline(Mirrored(i, reach)))];
        write(radii, frame.Origin);
        radii.MinMax(out double[] least, out _, out _, out _);
        float near = float.Max(0f, -(float)least[0]);
        return new PixelPass.Pointwise((row, _, line) => {
            ReadOnlySpan2D<Vector4> light = source.GetSpan<Vector4>().AsSpan2D(source.Rows, source.Cols);
            ReadOnlySpan2D<float> radius = radii.GetSpan<float>().AsSpan2D(radii.Rows, radii.Cols);
            ReadOnlySpan2D<(float Reach, float Edge)> window = outlines.AsSpan().AsSpan2D(side, side);
            int y = frame.Line(line);
            for (int x = 0; x < row.Length; x++) {
                float own = float.Abs(radius[y, x]);
                int span = (2 * (int)float.Ceiling(float.Max(own, near) * circumradius)) + 1;
                (Vector4 sum, Vector4 total) = (light[y, x], Vector4.One);
                for (int k = 0; k < span * span; k++) {
                    (int dx, int dy) = ((k % span) - (span / 2), (k / span) - (span / 2));
                    (int u, int v) = (int.Clamp(x + dx, 0, radius.Width - 1), int.Clamp(y + dy, 0, radius.Height - 1));
                    float other = radius[v, u];
                    (float outline, float edge) = window[dy + reach, dx + reach];
                    Vector4 weight = (dx | dy) == 0 ? Vector4.Zero : iris.Weights(outline, edge, float.Abs(float.Min(other, own)));
                    (sum, total) = (sum + (weight * light[v, u]), total + weight);
                }
                row[x] = Straight(sum / total);
            }
        }).Run(frame, progress);
    }

    private static Action<Mat, Point> Framed(Defocus state, AxisFraction aspect, int power, PixelExtent size, float cap) {
        Matrix3x2 field = Matrix3x2.CreateTranslation(-state.Center.Point(size)) * Matrix3x2.CreateRotation(state.Angle)
            * Matrix3x2.CreateScale(new Vector2(aspect, 1f) / size.ShortSide);
        return (plane, origin) => {
            Span<float> radii = plane.GetSpan<float>();
            for (int i = 0; i < radii.Length; i++) {
                float distance = Vector2.Transform(new Vector2(origin.X + (i % plane.Cols) + 0.5f, origin.Y + (i / plane.Cols) + 0.5f), field).Length();
                radii[i] = cap * float.Pow(float.Clamp((distance - state.Extent) / state.Feather, 0f, 1f), power);
            }
        };
    }

    private static Action<Mat, Point> Depth(PixelFrame depth, LensFocus lens, FNumber aperture, ViewWindow window, PixelExtent size, float cap) {
        (double focal, double focus) = (lens.FocalLength, lens.FocusDistance);
        double k = focal * size.ShortSide / (2d * aperture * (focus - focal) * double.Min((double)window.Right - window.Left, (double)window.Top - window.Bottom));
        int taps = (2 * (int)float.Ceiling(cap)) + 1;
        return (plane, _) => {
            using Mat header = depth.Header();
            using Mat magnitude = new();
            using Mat blur = new();
            using ScalarArray zero = new(0d);
            CvInvoke.ExtractChannel(header, plane, 0);
            Span<float> radius = plane.GetSpan<float>();
            foreach (ref float value in radius)
                value = (float)double.Clamp(k * (1d - (focus / value)), -cap, cap);
            CvInvoke.AbsDiff(plane, zero, magnitude);
            CvInvoke.GaussianBlur(magnitude, blur, new Size(taps, taps), cap / 3f, cap / 3f, BorderType.Replicate);
            CvInvoke.Min(blur, magnitude, blur);
            ReadOnlySpan<float> eroded = blur.GetSpan<float>();
            for (int i = 0; i < radius.Length; i++)
                radius[i] = float.CopySign(eroded[i], radius[i]);
        };
    }

    private static Vector2 Mirrored(int index, int reach) => new(reach - (index % ((2 * reach) + 1)), reach - (index / ((2 * reach) + 1)));

    private static Vector4 Straight(Vector4 light) => light.W > 0f ? new Vector4(light.AsVector3() / light.W, light.W) : Vector4.Zero;
}

[SmartEnum<string>]
[ValidationError<InvalidOptics>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class DefocusParameter : IStateParameter<Defocus> {
    private static readonly (StateParameter<Defocus> X, StateParameter<Defocus> Y) Position =
        FramePosition.Kinds(Lens<Defocus, FramePosition>.New(static defocus => defocus.Center, static center => defocus => defocus with { Center = center }));
    private static readonly Lens<Defocus, Iris> Diaphragm = Lens<Defocus, Iris>.New(static defocus => defocus.Iris, static iris => defocus => defocus with { Iris = iris });
    private static readonly (StateParameter<Defocus> Blades, StateParameter<Defocus> Rotation, StateParameter<Defocus> Roundness, StateParameter<Defocus> Obstruction, StateParameter<Defocus> Squeeze)
        Outline = Iris.Kinds(Diaphragm);

    public static readonly DefocusParameter Field = new("field", new StateParameter<Defocus>.Choice<FocusField, InvalidOptics>(Lens<Defocus, FocusField>.New(static defocus => defocus.Field, static field => defocus => defocus with { Field = field })));
    public static readonly DefocusParameter Radius = new("radius", new StateParameter<Defocus>.Bounded<DefocusRadius, float, InvalidOptics>(
        Lens<Defocus, DefocusRadius>.New(static defocus => defocus.Radius, static radius => defocus => defocus with { Radius = radius }), new() { Unit = Quantity.GetUnitInfo(RatioUnit.DecimalFraction) }));
    public static readonly DefocusParameter Aperture = new("f-stop", new StateParameter<Defocus>.Bounded<FNumber, float, InvalidPixelValue>(
        Lens<Defocus, FNumber>.New(static defocus => defocus.Aperture, static aperture => defocus => defocus with { Aperture = aperture }), new() { Soft = (0.1f, 128f), Decimals = 1 }));
    public static readonly DefocusParameter CenterX = new("center-x", Position.X);
    public static readonly DefocusParameter CenterY = new("center-y", Position.Y);
    public static readonly DefocusParameter Angle = new("angle", new StateParameter<Defocus>.Bounded<FocusAngle, float, InvalidOptics>(
        Lens<Defocus, FocusAngle>.New(static defocus => defocus.Angle, static angle => defocus => defocus with { Angle = angle }), new() { Unit = Quantity.GetUnitInfo(AngleUnit.Radian), Origin = (float)FocusAngle.Neutral }));
    public static readonly DefocusParameter Extent = new("extent", new StateParameter<Defocus>.Bounded<ShortSideLength, float, InvalidPixelValue>(
        Lens<Defocus, ShortSideLength>.New(static defocus => defocus.Extent, static extent => defocus => defocus with { Extent = extent }), ShortSideLength.Presentation));
    public static readonly DefocusParameter Aspect = new("aspect", new StateParameter<Defocus>.Bounded<AxisFraction, float, InvalidGrade>(
        Lens<Defocus, AxisFraction>.New(static defocus => defocus.Aspect, static aspect => defocus => defocus with { Aspect = aspect }), new()));
    public static readonly DefocusParameter Feather = new("feather", new StateParameter<Defocus>.Bounded<ShortSideExtent, float, InvalidPixelValue>(
        Lens<Defocus, ShortSideExtent>.New(static defocus => defocus.Feather, static feather => defocus => defocus with { Feather = feather }), new() { Unit = Quantity.GetUnitInfo(RatioUnit.DecimalFraction), Soft = (ShortSideExtent.MinValue, 1f) }));
    public static readonly DefocusParameter Blades = new("blades", Outline.Blades);
    public static readonly DefocusParameter Rotation = new("rotation", Outline.Rotation);
    public static readonly DefocusParameter Roundness = new("roundness", Outline.Roundness);
    public static readonly DefocusParameter Obstruction = new("obstruction", Outline.Obstruction);
    public static readonly DefocusParameter Squeeze = new("squeeze", Outline.Squeeze);
    public static readonly DefocusParameter Fringe = new("fringe", Iris.FringeKind(Diaphragm));

    public StateParameter<Defocus> Kind { get; }
}
