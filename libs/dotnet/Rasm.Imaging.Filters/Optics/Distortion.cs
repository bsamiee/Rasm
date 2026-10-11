using System.Numerics;
using System.Runtime.Intrinsics;
using CommunityToolkit.HighPerformance;
using Emgu.CV;
using Emgu.CV.CvEnum;
using MathNet.Numerics;
using Rasm.Imaging.Filters.Generators;
using Rasm.Imaging.Filters.Warp;
using Rasm.Imaging.Grade;
using Rasm.Imaging.Pixels;
using SixLabors.ImageSharp.PixelFormats;

namespace Rasm.Imaging.Filters.Optics;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<float>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Neutral", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidOptics>]
public readonly partial struct Distortion : IMinMaxValue<Distortion> {
    public static Distortion MinValue { get; } = new(-0.999f);
    public static Distortion MaxValue { get; } = new(1f);

    static partial void ValidateFactoryArguments(ref InvalidOptics? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidOptics();
}

[SmartEnum<string>]
[ValidationError<InvalidOptics>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class LensFraming {
    public static readonly LensFraming Edges = new("edges", static _ => 1d);
    public static readonly LensFraming Corners = new("corners", static extent => (double.Pow(extent.Width, 2d) + double.Pow(extent.Height, 2d)) / double.Pow(int.Max(extent.Width, extent.Height), 2d));

    public double Scale(PixelExtent extent, double distortion) => 1d / (1d + ((distortion > 0d ? Reach(extent) : 1d) * distortion));

    [UseDelegateFromConstructor]
    private partial double Reach(PixelExtent extent);
}

[SmartEnum<string>]
[ValidationError<InvalidOptics>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class LensDirection {
    public static readonly LensDirection Distort = new("distort", static (distortion, scale, radius) =>
        (4d * distortion * scale * scale * radius) switch { var bound => (2d * scale / (1d + double.Sqrt(double.Max(0d, 1d - bound))), bound > 1d ? 0f : 1f) });
    public static readonly LensDirection Undistort = new("undistort", static (distortion, scale, radius) =>
        (distortion * radius) switch { var bound => bound > -1d ? (1d / ((1d + bound) * scale), bound > 1d ? 0f : 1f) : (0d, 0f) });

    [UseDelegateFromConstructor]
    internal partial (double Factor, float Weight) Radial(double distortion, double scale, double radius);
}

public sealed record LensDistortion(Distortion Distortion, AxisFraction Dispersion, LensFraming Framing, LensDirection Direction, WrapMode Wrap)
    : IStateRecord<LensDistortion, LensDistortionParameter, InvalidOptics>, IPixelStage<LensDistortion> {
    public static LensDistortion Default { get; } = new(Distortion.Create(0.05f), AxisFraction.Create(0.05f), LensFraming.Edges, LensDirection.Distort, WrapMode.Clamp);

    public CoordinateMap Placed => Step(Distortion);

    public static Option<PixelPass> Pass(LensDistortion state, PassContext context) =>
        state.Distortion == Distortion.Neutral && state.Dispersion == AxisFraction.MinValue ? None
            : Some<PixelPass>(new PixelPass.Frame(
                state.Dispersion == AxisFraction.MinValue ? state.Placed.Apply
                    : Vector3.Clamp(new(state.Distortion + (state.Dispersion / 4f), state.Distortion, state.Distortion - (state.Dispersion / 4f)), new(Distortion.MinValue), new(Distortion.MaxValue)) switch {
                        var d => CoordinateMap.IntegratedLinear(static integral => ContourIntegrate.GaussKronrod(integral, 0d, 1d), state.Wrap,
                            (static t => new Vector4((float)(1d - t), (float)t, 0f, 1f), (point, t, extent) => state.Mapped(double.Lerp(d.X, d.Y, t), point, extent)),
                            (static t => new Vector4(0f, (float)(1d - t), (float)t, 1f), (point, t, extent) => state.Mapped(double.Lerp(d.Y, d.Z, t), point, extent))),
                    }));

    private CoordinateMap.Analytic Step(float distortion) =>
        new((points, coverage, extent) => {
            for (int i = 0; i < points.Length; i++)
                (points[i], coverage[i]) = Mapped(distortion, points[i], extent) switch { var (point, weight) => (point, weight * coverage[i]) };
        }, Sampling.Linear, (Wrap, Wrap));

    private ((double X, double Y) Point, float Coverage) Mapped(double distortion, (double X, double Y) point, PixelExtent extent) {
        (double x, double y) = (extent.Width / 2d, extent.Height / 2d);
        (float red, float blue) = (float.Min(Distortion + (Dispersion / 4f), Distortion.MaxValue), float.Max(Distortion - (Dispersion / 4f), Distortion.MinValue));
        double half = double.Max(x, y);
        (double scale, double radius) = (Framing.Scale(extent, red), double.Pow((point.X - x) / half, 2d) + double.Pow((point.Y - y) / half, 2d));
        double factor = Direction.Radial(distortion, scale, radius).Factor;
        return ((x + (factor * (point.X - x)), y + (factor * (point.Y - y))), Direction.Radial(red, scale, radius).Weight * Direction.Radial(blue, scale, radius).Weight);
    }
}

[SmartEnum<string>]
[ValidationError<InvalidOptics>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class LensDistortionParameter : IStateParameter<LensDistortion> {
    public static readonly LensDistortionParameter Distortion = new("distortion", StateParameter.Bounded<LensDistortion, Distortion, float, InvalidOptics>(Lens<LensDistortion, Distortion>.New(static lens => lens.Distortion, static distortion => lens => lens with { Distortion = distortion }), new() { Origin = (float)Optics.Distortion.Neutral }));
    public static readonly LensDistortionParameter Dispersion = new("dispersion", StateParameter.Bounded<LensDistortion, AxisFraction, float, InvalidGrade>(Lens<LensDistortion, AxisFraction>.New(static lens => lens.Dispersion, static dispersion => lens => lens with { Dispersion = dispersion }), new()));
    public static readonly LensDistortionParameter Framing = new("framing", new StateParameter<LensDistortion>.Choice<LensFraming, InvalidOptics>(Lens<LensDistortion, LensFraming>.New(static lens => lens.Framing, static framing => lens => lens with { Framing = framing })));
    public static readonly LensDistortionParameter Direction = new("direction", new StateParameter<LensDistortion>.Choice<LensDirection, InvalidOptics>(Lens<LensDistortion, LensDirection>.New(static lens => lens.Direction, static direction => lens => lens with { Direction = direction })));
    public static readonly LensDistortionParameter Wrap = new("wrap", new StateParameter<LensDistortion>.Choice<WrapMode, InvalidPixelValue>(Lens<LensDistortion, WrapMode>.New(static lens => lens.Wrap, static wrap => lens => lens with { Wrap = wrap })));

    public StateParameter<LensDistortion> Kind { get; }
}

public sealed unsafe record AxialAberration(ShortSideLength Radius, Iris Iris)
    : IStateRecord<AxialAberration, AxialAberrationParameter, InvalidOptics>, IPixelStage<AxialAberration> {
    public static AxialAberration Default { get; } = new(ShortSideLength.Create(20f / ReferenceFrame.Height), Defocus.Default.Iris);

    public static Option<PixelPass> Pass(AxialAberration state, PassContext context) =>
        state.Radius == ShortSideLength.Neutral ? None : Some<PixelPass>(new PixelPass.Frame((frame, progress) => Try.lift(() => {
            using Mat light = new(frame.Size.Height, frame.Size.Width, DepthType.Cv32F, 4);
            PixelOperations<RgbaVector>.Instance.ToVector4(SixLabors.ImageSharp.Configuration.Default, frame.Block.Cast<float, RgbaVector>(), new Span<Vector4>((void*)light.DataPointer, frame.Block.Length / 4), PixelConversionModifiers.Premultiply);
            double radius = state.Radius.Pixels(context.Extent);
            return Defocus.Correlate(light, state.Iris with { Fringe = ColorFringe.Neutral }, Vector256.Create(radius / 2d, 0d, radius, 0d)).Bind(_ => new PixelPass.Pointwise((row, _, line) => {
                ReadOnlySpan<Vector4> blurred = new((void*)light.GetDataPointer(frame.RowIndex(line)), frame.Size.Width);
                for (int x = 0; x < row.Length; x++)
                    row[x] = new(row[x].W > 0f ? blurred[x].AsVector3() / row[x].W : Vector3.Zero, row[x].W);
            }).Run(frame, progress));
        }).Run()));
}

[SmartEnum<string>]
[ValidationError<InvalidOptics>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class AxialAberrationParameter : IStateParameter<AxialAberration> {
    private static readonly (StateParameter<AxialAberration> Blades, StateParameter<AxialAberration> Rotation, StateParameter<AxialAberration> Roundness, StateParameter<AxialAberration> Obstruction, StateParameter<AxialAberration> Squeeze)
        Outline = Iris.Kinds(Lens<AxialAberration, Iris>.New(static aberration => aberration.Iris, static iris => aberration => aberration with { Iris = iris }));

    public static readonly AxialAberrationParameter Radius = new("radius", StateParameter.Bounded<AxialAberration, ShortSideLength, float, InvalidPixelValue>(Lens<AxialAberration, ShortSideLength>.New(static aberration => aberration.Radius, static radius => aberration => aberration with { Radius = radius }), ShortSideLength.Presentation with { Soft = (ShortSideLength.MinValue, 80f / ReferenceFrame.Height) }));
    public static readonly AxialAberrationParameter Blades = new("blades", Outline.Blades);
    public static readonly AxialAberrationParameter Rotation = new("rotation", Outline.Rotation);
    public static readonly AxialAberrationParameter Roundness = new("roundness", Outline.Roundness);
    public static readonly AxialAberrationParameter Obstruction = new("obstruction", Outline.Obstruction);
    public static readonly AxialAberrationParameter Squeeze = new("squeeze", Outline.Squeeze);

    public StateParameter<AxialAberration> Kind { get; }
}
