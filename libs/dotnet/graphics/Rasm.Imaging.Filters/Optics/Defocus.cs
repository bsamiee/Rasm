using System.Drawing;
using System.Numerics;
using System.Runtime.Intrinsics;
using CommunityToolkit.HighPerformance;
using Emgu.CV;
using Emgu.CV.CvEnum;
using PeterO.Numbers;
using Rasm.Imaging.Filters.Generators;
using Rasm.Imaging.Pixels;
using SixLabors.ImageSharp.PixelFormats;

namespace Rasm.Imaging.Filters.Optics;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<float>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Off", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct DefocusRadius : IPresented<DefocusRadius, float> {
    public static DefocusRadius MinValue => Off;
    public static DefocusRadius MaxValue { get; } = new(0.1f * ReferenceFrame.GreaterSide);
    public static Presentation<float> Presentation => AxisFraction.Presentation;

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FocusField : ICaptioned {
    public static readonly FocusField Uniform = new("uniform", "Whole frame");
    public static readonly FocusField Depth = new("depth", "Depth of field");
    public static readonly FocusField Curvature = new("curvature", "Field curvature");
    public static readonly FocusField Tilt = new("tilt", "Tilt-shift");

    public string Caption { get; }
}

public sealed unsafe record Defocus(FocusField Field, DefocusRadius Radius, PositiveMagnitude Aperture, FramePosition Center, Elevation Angle, ShortSideLength Extent, AxisFraction Aspect, ShortSideExtent Feather, Iris Iris, SignedFraction Fringe)
    : IStateRecord<Defocus, DefocusParameter>, IPixelStage<Defocus> {
    public static Defocus Default { get; } = new(FocusField.Depth, DefocusRadius.Create(0.02f * ReferenceFrame.GreaterSide), PositiveMagnitude.Create(2.8f), FramePosition.Center,
        Elevation.Neutral, ShortSideLength.Create(0.3f / 2f), AxisFraction.MaxValue, ShortSideExtent.Create(0.5f / 2f), Iris.Default with { Roundness = AxisFraction.MaxValue }, SignedFraction.Neutral);

    public static Seq<GuideChannel> Channels(Defocus state) => state.Field.Map<Seq<GuideChannel>>(uniform: [], depth: [GuideChannel.Depth], curvature: [], tilt: []);

    public static Option<PixelPass> Pass(Defocus state, PassContext context) =>
        state.Radius == DefocusRadius.Off ? None : state.Field.Switch(
            (State: state, Context: context, Cap: state.Radius * context.Extent.ShortSide),
            uniform: static s => Some<Func<Mat, Mat, PixelFrame, Fin<Unit>>>((light, _, _) => Correlate(light, s.State.Iris, s.State.Fringe, Vector256.Create((double)s.Cap))),
            depth: Depth,
            curvature: static s => Some(Framed(s, s.State.Aspect, 2)),
            tilt: static s => Some(Framed(s, AxisFraction.MinValue, 1))).Map<PixelPass>(field => Gathered(state.Iris, state.Fringe, field));

    internal static Fin<Unit> Correlate(Mat image, Iris iris, SignedFraction fringe, Vector256<double> radii) =>
        toSeq(Range(0, image.NumberOfChannels)).Traverse(channel => PixelSampling.KernelTaps<int>(ERational.FromDouble(radii[channel]).Multiply(ERational.FromSingle(iris.Circumradius))).Map(side => (Channel: channel, Side: side))).As().Bind(kernels => Try.lift(() => {
            foreach ((int channel, int side) in kernels) {
                using Mat plane = new();
                using Mat kernel = new(side, side, DepthType.Cv32F, 1);
                for (int y = 0; y < side; y++) {
                    Span<float> cells = new((void*)kernel.GetDataPointer(y), side);
                    for (int x = 0; x < cells.Length; x++)
                        cells[x] = (float)Tap(iris, fringe, iris.Outline(((side / 2) - x, (side / 2) - y)), radii[channel])[channel];
                }
                CvInvoke.Normalize(kernel, kernel, 1d, 0d, NormType.L1);
                CvInvoke.ExtractChannel(image, plane, channel);
                CvInvoke.Filter2D(plane, plane, kernel, new Point(-1, -1), 0d, BorderType.Replicate);
                CvInvoke.InsertChannel(plane, image, channel);
            }
            return unit;
        }).Run());

    internal static PixelPass.Frame Gathered(Iris iris, SignedFraction fringe, Func<Mat, Mat, PixelFrame, Fin<Unit>> field) =>
        new((frame, progress) => Try.lift(() => {
            using Mat source = new(frame.Size.Height, frame.Size.Width, DepthType.Cv32F, 4);
            using Mat radii = Mat.Zeros(frame.Size.Height, frame.Size.Width, DepthType.Cv32F, 1);
            PixelOperations<RgbaVector>.Instance.ToVector4(SixLabors.ImageSharp.Configuration.Default, frame.Block.Cast<float, RgbaVector>(), new Span<Vector4>((void*)source.DataPointer, frame.Block.Length / 4), PixelConversionModifiers.Premultiply);
            return field(source, radii, frame).Bind(delegate {
                radii.MinMax(out double[] least, out double[] most, out _, out _);
                (float circumradius, float near) = (iris.Circumradius, float.Max(0f, -(float)least[0]));
                int side = Window(float.Max(near, (float)most[0]) * circumradius);
                (double Reach, double Edge)[] outlines = Outlines(iris, side);
                return new PixelPass.Pointwise((row, _, line) => {
                    ReadOnlySpan2D<Vector4> light = new((void*)source.DataPointer, source.Rows, source.Cols, 0);
                    ReadOnlySpan2D<float> radius = new((void*)radii.DataPointer, radii.Rows, radii.Cols, 0);
                    int y = frame.RowIndex(line);
                    for (int x = 0; x < row.Length; x++) {
                        float own = float.Abs(radius[y, x]);
                        (Vector256<double> sum, Vector256<double> total) = (Vector256<double>.Zero, Vector256<double>.Zero);
                        for (int span = Window(float.Max(own, near) * circumradius), k = 0; k < span * span; k++) {
                            (int dy, int dx) = ((k / span) - (span / 2), (k % span) - (span / 2));
                            (int u, int v) = (int.Clamp(x + dx, 0, radius.Width - 1), int.Clamp(y + dy, 0, radius.Height - 1));
                            Vector256<double> weights = Tap(iris, fringe, outlines[((dy + (side / 2)) * side) + dx + (side / 2)], float.Abs(float.Min(radius[v, u], own)));
                            Vector4 pixel = light[v, u];
                            (sum, total) = (sum + (weights * Vector256.Create(pixel.X, pixel.Y, pixel.Z, pixel.W)), total + weights);
                        }
                        row[x] = Vector256.Narrow(sum / total, Vector256<double>.Zero).GetLower().AsVector4() switch { { W: > 0f } mean => new(mean.AsVector3() / mean.W, mean.W), _ => Vector4.Zero };
                    }
                }).Run(frame, progress);
            });
        }).Run());

    private static Func<Mat, Mat, PixelFrame, Fin<Unit>> Framed((Defocus State, PassContext Context, float Cap) s, AxisFraction aspect, int power) =>
        (_, radii, frame) => {
            Span<float> radius = new((void*)radii.DataPointer, frame.Block.Length / 4);
            (double x, double y) = s.State.Center.Point(s.Context.Extent);
            (double sin, double cos) = double.SinCos(s.State.Angle);
            for (int i = 0; i < radius.Length; i++) {
                (double u, double v) = (((double)frame.Origin.X + (i % frame.Size.Width) + 0.5d - x) / s.Context.Extent.ShortSide, ((double)frame.Origin.Y + (i / frame.Size.Width) + 0.5d - y) / s.Context.Extent.ShortSide);
                radius[i] = (float)(s.Cap * double.Pow(double.Clamp((double.Hypot(aspect * ((cos * u) - (sin * v)), (sin * u) + (cos * v)) - s.State.Extent) / s.State.Feather, 0d, 1d), power));
            }
            return unit;
        };

    private static Option<Func<Mat, Mat, PixelFrame, Fin<Unit>>> Depth((Defocus State, PassContext Context, float Cap) s) =>
        from depth in s.Context.Guides.Find(GuideChannel.Depth)
        from camera in s.Context.Camera
        from lens in camera.Lens
        from window in camera.Frustum.Switch(perspective: static frustum => Some(frustum.Window), parallel: static _ => Option<ViewWindow>.None)
        let focus = (double)lens.FocusDistance
        let k = lens.FocalLength * (double)s.Context.Extent.ShortSide / (2d * s.State.Aperture * (focus - lens.FocalLength))
        let span = double.Min(window.Width, window.Height)
        select new Func<Mat, Mat, PixelFrame, Fin<Unit>>((_, radii, frame) => {
            Span<float> radius = new((void*)radii.DataPointer, frame.Block.Length / 4);
            using Mat magnitude = new(frame.Size.Height, frame.Size.Width, DepthType.Cv32F, 1);
            ReadOnlySpan2D<Vector4> distance = depth.View.Span;
            Span<float> blurred = new((void*)magnitude.DataPointer, radius.Length);
            for (int i = 0; i < radius.Length; i++) {
                (int y, int x) = int.DivRem(i, frame.Size.Width);
                (int u, int v) = ((int)long.Clamp((long)frame.Origin.X + x - depth.Origin.X, 0, distance.Width - 1), (int)long.Clamp((long)frame.Origin.Y + y - depth.Origin.Y, 0, distance.Height - 1));
                (radius[i], blurred[i]) = (float)double.Clamp(k * (1d - (focus / distance[v, u].X)) / span, -s.Cap, s.Cap) switch { var r => (r, float.Abs(r)) };
            }
            return magnitude.Blur(magnitude, s.Cap).Map(_ => {
                Span<float> values = new((void*)radii.DataPointer, frame.Block.Length / 4);
                ReadOnlySpan<float> smooth = new((void*)magnitude.DataPointer, values.Length);
                for (int i = 0; i < values.Length; i++)
                    values[i] = float.CopySign(float.Min(smooth[i], float.Abs(values[i])), values[i]);
                return unit;
            });
        });

    private static (double Reach, double Edge)[] Outlines(Iris iris, int side) =>
        [.. Range(0, side * side).Select(i => iris.Outline(((side / 2) - (i % side), (side / 2) - (i / side))))];

    private static Vector256<double> Tap(Iris iris, SignedFraction fringe, (double Reach, double Edge) outline, double radius) =>
        outline.Reach is 0d ? Vector256.Create(1d) : iris.Weights(outline.Reach, outline.Edge, radius, 1d, fringe);

    private static int Window(float reach) => (2 * (int)float.Ceiling(reach)) + 1;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class DefocusParameter : IStateParameter<Defocus> {
    public static readonly DefocusParameter Field = new("field", new StateParameter<Defocus>.Choice<FocusField>(Lens<Defocus, FocusField>.New(static defocus => defocus.Field, static field => defocus => defocus with { Field = field })),
        new("Focus", "Field the blur radius follows across the frame"));
    public static readonly DefocusParameter Radius = new("radius", StateParameter.Bounded(Lens<Defocus, DefocusRadius>.New(static defocus => defocus.Radius, static radius => defocus => defocus with { Radius = radius })),
        new("Max blur", "Largest blur radius as a share of the frame's short side"));
    public static readonly DefocusParameter Aperture = new("f-stop", StateParameter.Bounded(Lens<Defocus, PositiveMagnitude>.New(static defocus => defocus.Aperture, static aperture => defocus => defocus with { Aperture = aperture }), new Presentation<float> { Soft = (0.1f, 128f), Decimals = 1 }),
        new("F-stop", "F-stop ratio of the lens the depth of field draws, lower numbers giving more defocus"));
    public static readonly DefocusParameter Center = new("center", new StateParameter<Defocus>.Record<FramePosition>(Lens<Defocus, FramePosition>.New(static defocus => defocus.Center, static center => defocus => defocus with { Center = center })),
        new("Center", "Sharp region's center"));
    public static readonly DefocusParameter Angle = new("angle", StateParameter.Bounded(Lens<Defocus, Elevation>.New(static defocus => defocus.Angle, static angle => defocus => defocus with { Angle = angle })),
        new("Orientation", "Turn of the sharp band or ellipse, counterclockwise"));
    public static readonly DefocusParameter Extent = new("extent", StateParameter.Bounded(Lens<Defocus, ShortSideLength>.New(static defocus => defocus.Extent, static extent => defocus => defocus with { Extent = extent })),
        new("Sharp width", "Half-width of the sharp region across its band"));
    public static readonly DefocusParameter Aspect = new("aspect", StateParameter.Bounded(Lens<Defocus, AxisFraction>.New(static defocus => defocus.Aspect, static aspect => defocus => defocus with { Aspect = aspect })),
        new("Aspect", "Sharp ellipse from a band at 0% to a circle at 100%"));
    public static readonly DefocusParameter Feather = new("feather", StateParameter.Bounded(Lens<Defocus, ShortSideExtent>.New(static defocus => defocus.Feather, static feather => defocus => defocus with { Feather = feather }), ShortSideExtent.Presentation with { Soft = (0.001f, 1f), Scale = TrackScale.Linear }),
        new("Feather", "Distance past the sharp region where the blur reaches its maximum"));
    public static readonly DefocusParameter Iris = new("iris", new StateParameter<Defocus>.Record<Iris>(Lens<Defocus, Iris>.New(static defocus => defocus.Iris, static iris => defocus => defocus with { Iris = iris })),
        new("Aperture", "Outline of the lens opening each out-of-focus disc and diffraction spike takes"));
    public static readonly DefocusParameter Fringe = new("fringe", StateParameter.Bounded(Lens<Defocus, SignedFraction>.New(static defocus => defocus.Fringe, static fringe => defocus => defocus with { Fringe = fringe })),
        new("Color shift", "Red and blue disc sizes apart from green"));

    public StateParameter<Defocus> Kind { get; }
    public ParameterText Text { get; }
}
