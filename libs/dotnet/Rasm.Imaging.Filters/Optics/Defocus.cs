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
    public static Defocus Default { get; } = new(FocusField.Depth, DefocusRadius.Create(0.02f * ReferenceFrame.GreaterSide), FNumber.Create(2.8f), FramePosition.Center,
        FocusAngle.Neutral, ShortSideLength.Create(0.3f / 2f), AxisFraction.MaxValue, ShortSideExtent.Create(0.5f / 2f), Iris.Hexagon with { Roundness = AxisFraction.MaxValue });

    public static Seq<GuideChannel> Channels(Defocus state) => state.Field.Map<Seq<GuideChannel>>(uniform: [], depth: [GuideChannel.Depth], curvature: [], tilt: []);

    public static Option<PixelPass> Pass(Defocus state, PassContext context) =>
        state.Radius == DefocusRadius.Off ? None : state.Field.Switch(
            (State: state, Context: context, Cap: state.Radius * context.Extent.ShortSide),
            uniform: static s => Some<Action<Mat, Span<float>, PixelFrame>>((light, _, _) => Correlate(light, s.State.Iris, new Vector4(s.Cap))),
            depth: Depth,
            curvature: static s => Some(Framed(s, s.State.Aspect, 2)),
            tilt: static s => Some(Framed(s, AxisFraction.MinValue, 1))).Map<PixelPass>(field => Gathered(state.Iris, field));

    internal static void Correlate(Mat image, Iris iris, Vector4 radii) {
        using Mat plane = new();
        for (int lane = 0; lane < image.NumberOfChannels; lane++) {
            int side = Window(radii[lane] * iris.Circumradius);
            (float Reach, float Edge)[] outlines = Outlines(iris, side);
            using Mat kernel = new(side, side, DepthType.Cv32F, 1);
            kernel.SetTo(outlines.Select((_, index) => Tap(iris, outlines, index, radii[lane])[lane]).ToArray());
            CvInvoke.Normalize(kernel, kernel, 1d, 0d, NormType.L1);
            CvInvoke.ExtractChannel(image, plane, lane);
            CvInvoke.Filter2D(plane, plane, kernel, new Point(-1, -1), 0d, BorderType.Replicate);
            CvInvoke.InsertChannel(plane, image, lane);
        }
    }

    private static PixelPass.Frame Gathered(Iris iris, Action<Mat, Span<float>, PixelFrame> field) =>
        new((frame, progress) => {
            using Mat source = new(frame.Size.Height, frame.Size.Width, DepthType.Cv32F, 4);
            using Mat radii = Mat.Zeros(frame.Size.Height, frame.Size.Width, DepthType.Cv32F, 1);
            PixelOperations<RgbaVector>.Instance.ToVector4(SixLabors.ImageSharp.Configuration.Default, frame.Block.Cast<float, RgbaVector>(), source.GetSpan<Vector4>(), PixelConversionModifiers.Premultiply);
            field(source, radii.GetSpan<float>(), frame);
            radii.MinMax(out double[] least, out double[] most, out _, out _);
            (float circumradius, float near) = (iris.Circumradius, float.Max(0f, -(float)least[0]));
            int side = Window(float.Max(near, (float)most[0]) * circumradius);
            (float Reach, float Edge)[] outlines = Outlines(iris, side);
            return new PixelPass.Pointwise((row, _, line) => {
                ReadOnlySpan2D<Vector4> light = source.GetSpan<Vector4>().AsSpan2D(source.Rows, source.Cols);
                ReadOnlySpan2D<float> radius = radii.GetSpan<float>().AsSpan2D(radii.Rows, radii.Cols);
                int y = frame.Line(line);
                for (int x = 0; x < row.Length; x++) {
                    float own = float.Abs(radius[y, x]);
                    (Vector4 sum, Vector4 total) = (Vector4.Zero, Vector4.Zero);
                    for (int span = Window(float.Max(own, near) * circumradius), k = 0; k < span * span; k++) {
                        (int dy, int dx) = ((k / span) - (span / 2), (k % span) - (span / 2));
                        (int u, int v) = (int.Clamp(x + dx, 0, radius.Width - 1), int.Clamp(y + dy, 0, radius.Height - 1));
                        (sum, total) = Tap(iris, outlines, ((dy + (side / 2)) * side) + dx + (side / 2), float.Abs(float.Min(radius[v, u], own))) switch { var weight => (sum + (weight * light[v, u]), total + weight) };
                    }
                    row[x] = (sum / total) switch { { W: > 0f } mean => new(mean.AsVector3() / mean.W, mean.W), _ => Vector4.Zero };
                }
            }).Run(frame, progress);
        });

    private static Action<Mat, Span<float>, PixelFrame> Framed((Defocus State, PassContext Context, float Cap) s, AxisFraction aspect, int power) =>
        (_, radius, frame) => {
            Matrix3x2 field = Matrix3x2.CreateTranslation(-s.State.Center.Point(s.Context.Extent)) * Matrix3x2.CreateRotation(s.State.Angle) * Matrix3x2.CreateScale(new Vector2(aspect, 1f) / s.Context.Extent.ShortSide);
            for (int i = 0; i < radius.Length; i++)
                radius[i] = s.Cap * float.Pow(float.Clamp((Vector2.Transform(new Vector2(frame.Origin.X + (i % frame.Size.Width) + 0.5f, frame.Origin.Y + (i / frame.Size.Width) + 0.5f), field).Length() - s.State.Extent) / s.State.Feather, 0f, 1f), power);
        };

    private static Option<Action<Mat, Span<float>, PixelFrame>> Depth((Defocus State, PassContext Context, float Cap) s) =>
        from depth in s.Context.Guides.Find(GuideChannel.Depth)
        from camera in s.Context.Camera
        from lens in camera.Lens
        from window in camera.Frustum.Switch(perspective: static frustum => Some(frustum.Window), parallel: static _ => Option<ViewWindow>.None)
        let focus = (double)lens.FocusDistance
        let taps = Window(s.Cap)
        let k = lens.FocalLength * (double)s.Context.Extent.ShortSide / (2d * s.State.Aperture * (focus - lens.FocalLength) * double.Min((double)window.Right - window.Left, (double)window.Top - window.Bottom))
        select new Action<Mat, Span<float>, PixelFrame>((_, radius, frame) => {
            using Mat magnitude = new(frame.Size.Height, frame.Size.Width, DepthType.Cv32F, 1);
            ReadOnlySpan<Vector4> distance = depth.Block.Cast<float, Vector4>();
            Span<float> blurred = magnitude.GetSpan<float>();
            for (int i = 0; i < radius.Length; i++)
                (radius[i], blurred[i]) = (float)double.Clamp(k * (1d - (focus / distance[i].X)), -s.Cap, s.Cap) switch { var r => (r, float.Abs(r)) };
            CvInvoke.GaussianBlur(magnitude, magnitude, new Size(taps, taps), s.Cap / 3f, s.Cap / 3f, BorderType.Replicate);
            for (int i = 0; i < radius.Length; i++)
                radius[i] = float.CopySign(float.Min(blurred[i], float.Abs(radius[i])), radius[i]);
        });

    private static (float Reach, float Edge)[] Outlines(Iris iris, int side) =>
        [.. Range(0, side * side).Select(i => iris.Outline(new Vector2((side / 2) - (i % side), (side / 2) - (i / side))))];

    private static Vector4 Tap(Iris iris, (float Reach, float Edge)[] outlines, int index, float radius) =>
        index == outlines.Length / 2 ? Vector4.One : iris.Weights(outlines[index].Reach, outlines[index].Edge, radius, 1f);

    private static int Window(float reach) => (2 * (int)float.Ceiling(reach)) + 1;
}

[SmartEnum<string>]
[ValidationError<InvalidOptics>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class DefocusParameter : IStateParameter<Defocus> {
    private static readonly (StateParameter<Defocus> X, StateParameter<Defocus> Y) Position = FramePosition.Kinds(Lens<Defocus, FramePosition>.New(static defocus => defocus.Center, static center => defocus => defocus with { Center = center }));
    private static readonly Lens<Defocus, Iris> Diaphragm = Lens<Defocus, Iris>.New(static defocus => defocus.Iris, static iris => defocus => defocus with { Iris = iris });
    private static readonly (StateParameter<Defocus> Blades, StateParameter<Defocus> Rotation, StateParameter<Defocus> Roundness, StateParameter<Defocus> Obstruction, StateParameter<Defocus> Squeeze) Outline = Iris.Kinds(Diaphragm);

    public static readonly DefocusParameter Field = new("field", new StateParameter<Defocus>.Choice<FocusField, InvalidOptics>(Lens<Defocus, FocusField>.New(static defocus => defocus.Field, static field => defocus => defocus with { Field = field })));
    public static readonly DefocusParameter Radius = new("radius", new StateParameter<Defocus>.Bounded<DefocusRadius, float, InvalidOptics>(Lens<Defocus, DefocusRadius>.New(static defocus => defocus.Radius, static radius => defocus => defocus with { Radius = radius }), new() { Unit = Quantity.GetUnitInfo(RatioUnit.DecimalFraction) }));
    public static readonly DefocusParameter Aperture = new("f-stop", new StateParameter<Defocus>.Bounded<FNumber, float, InvalidPixelValue>(Lens<Defocus, FNumber>.New(static defocus => defocus.Aperture, static aperture => defocus => defocus with { Aperture = aperture }), new() { Soft = (0.1f, 128f), Decimals = 1 }));
    public static readonly DefocusParameter CenterX = new("center-x", Position.X);
    public static readonly DefocusParameter CenterY = new("center-y", Position.Y);
    public static readonly DefocusParameter Angle = new("angle", new StateParameter<Defocus>.Bounded<FocusAngle, float, InvalidOptics>(Lens<Defocus, FocusAngle>.New(static defocus => defocus.Angle, static angle => defocus => defocus with { Angle = angle }), new() { Unit = Quantity.GetUnitInfo(AngleUnit.Radian), Origin = (float)FocusAngle.Neutral }));
    public static readonly DefocusParameter Extent = new("extent", new StateParameter<Defocus>.Bounded<ShortSideLength, float, InvalidPixelValue>(Lens<Defocus, ShortSideLength>.New(static defocus => defocus.Extent, static extent => defocus => defocus with { Extent = extent }), ShortSideLength.Presentation));
    public static readonly DefocusParameter Aspect = new("aspect", new StateParameter<Defocus>.Bounded<AxisFraction, float, InvalidGrade>(Lens<Defocus, AxisFraction>.New(static defocus => defocus.Aspect, static aspect => defocus => defocus with { Aspect = aspect }), new()));
    public static readonly DefocusParameter Feather = new("feather", new StateParameter<Defocus>.Bounded<ShortSideExtent, float, InvalidPixelValue>(Lens<Defocus, ShortSideExtent>.New(static defocus => defocus.Feather, static feather => defocus => defocus with { Feather = feather }), new() { Unit = Quantity.GetUnitInfo(RatioUnit.DecimalFraction), Soft = (ShortSideExtent.MinValue, 1f) }));
    public static readonly DefocusParameter Blades = new("blades", Outline.Blades);
    public static readonly DefocusParameter Rotation = new("rotation", Outline.Rotation);
    public static readonly DefocusParameter Roundness = new("roundness", Outline.Roundness);
    public static readonly DefocusParameter Obstruction = new("obstruction", Outline.Obstruction);
    public static readonly DefocusParameter Squeeze = new("squeeze", Outline.Squeeze);
    public static readonly DefocusParameter Fringe = new("fringe", Iris.FringeKind(Diaphragm));

    public StateParameter<Defocus> Kind { get; }
}
