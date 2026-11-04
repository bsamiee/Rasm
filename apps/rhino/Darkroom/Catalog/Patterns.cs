using System.Numerics;
using System.Runtime.Intrinsics;
using Darkroom.Chain;
using Rasm.Imaging.Grade;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Procedural;
using UnitsNet;
using Wacton.Unicolour;

namespace Darkroom.Catalog;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class GradientKind {
    public static readonly GradientKind Linear = new("linear", static extent => point => (0.5d + (point.X / (2d * extent.X)), 1d / (2d * extent.X)), static integral => integral.Mean);
    public static readonly GradientKind Radial = new("radial", static extent => point => (X: (double)point.X / extent.X, Y: (double)point.Y / extent.Y) switch {
        var (x, y) => double.Hypot(x, y) switch {
            var level => (level, level > 0d ? double.Hypot(x / (extent.X * level), y / (extent.Y * level)) : 1d / extent.Y),
        },
    }, static integral => integral.Mean);
    public static readonly GradientKind Angular = new("angular", static _ => static point => ((double.Atan2(point.Y, point.X) / double.Tau) + 0.5d, 1d / (double.Tau * double.Hypot(point.X, point.Y))), static integral => integral.PeriodicMean);
    public static readonly GradientKind Box = new("box", static extent => point => (X: double.Abs(point.X) / extent.X, Y: double.Abs(point.Y) / extent.Y) switch {
        var reach => reach.X >= reach.Y ? (reach.X, 1d / extent.X) : (reach.Y, 1d / extent.Y),
    }, static integral => integral.Mean);

    [UseDelegateFromConstructor]
    public partial Func<Vector2, (double Position, double Slope)> Parameter(Vector2 extent);

    [UseDelegateFromConstructor]
    public partial Func<double, double, Vector4> Sample(RampIntegral integral);
}

[SmartEnum<string>]
public sealed partial class GridPattern {
    public static readonly GridPattern Lines = new("lines", static (lattice, line, _, _, _) => point => new Vector4(line, lattice.Mortar(point)));
    public static readonly GridPattern Checker = new("checker", static (lattice, line, cell, alternate, _) => point =>
        new Vector4(lattice.Bricks(point, (i, j) => long.IsEvenInteger(i + j) ? cell : alternate, line), 1f));
    public static readonly GridPattern Brick = new("brick", static (lattice, line, cell, alternate, field) => point =>
        new Vector4(lattice.Bricks(point, (i, j) => Vector3.Lerp(cell, alternate, CoordinateHash.Uniforms(Vector128.Create((int)i, (int)j, 0, 0), field).X), line), 1f));

    [UseDelegateFromConstructor]
    public partial Func<Vector2, Vector4> Shade(BrickPattern lattice, Vector3 line, Vector3 cell, Vector3 alternate, uint field);
}

public sealed record Gradient(GradientKind Kind, FieldPlacement Placement, ShortSideLength Width, ShortSideLength Height, Seq<RampStop> Colors, GeneratedLayer Layer) : IPixelStage<Gradient> {
    public static Gradient Default { get; } = new(GradientKind.Linear, FieldPlacement.Default, ShortSideLength.Create(1f / 2f), ShortSideLength.Create(1f / 2f), Presets.Grayscale, GeneratedLayer.Default);

    public static Fin<Option<PixelPass>> Pass(Gradient state, PassContext context) =>
        (state.Kind.Parameter(new Vector2(state.Width, state.Height)), state.Kind.Sample(RampLight.Integral(state.Colors, None, context)), (double)context.Extent.ShortSide) switch {
            var (parameter, mean, side) => state.Layer.Pass(DistanceField.Fill(state.Placement.Frame(context.Extent), point =>
                parameter(point) switch { var (position, slope) => mean(position, slope / side) })),
        };
}

public sealed record CornerGradient(
    Unicolour TopLeft, Unicolour TopRight,
    Unicolour BottomRight, Unicolour BottomLeft, GeneratedLayer Layer)
    : IPixelStage<CornerGradient> {
    public static CornerGradient Default { get; } = new(
        new Unicolour(ColourSpace.Rgb, 1d, 1d, 1d), new Unicolour(ColourSpace.Rgb, 0d, 0d, 0d), new Unicolour(ColourSpace.Rgb, 1d, 1d, 1d), new Unicolour(ColourSpace.Rgb, 0d, 0d, 0d), GeneratedLayer.Default);

    public static Fin<Option<PixelPass>> Pass(CornerGradient state, PassContext context) {
        (Vector3 topLeft, Vector3 topRight, Vector3 bottomRight, Vector3 bottomLeft) =
            (RampLight.Light(state.TopLeft, context).AsVector3(), RampLight.Light(state.TopRight, context).AsVector3(),
                RampLight.Light(state.BottomRight, context).AsVector3(), RampLight.Light(state.BottomLeft, context).AsVector3());
        PixelExtent extent = context.Extent;
        return state.Layer.Pass(new Action<Span<Vector4>, int, int>((row, x, y) => {
            float down = DistanceField.SmootherStep(0f, extent.Height, y + 0.5f);
            for (int i = 0; i < row.Length; i++) {
                float across = DistanceField.SmootherStep(0f, extent.Width, x + i + 0.5f);
                row[i] = new Vector4(Vector3.Lerp(Vector3.Lerp(topLeft, topRight, across), Vector3.Lerp(bottomLeft, bottomRight, across), down), 1f);
            }
        }));
    }
}

public sealed record Grid(
    GridPattern Pattern, FieldPlacement Placement, ShortSideLength Cell, float Aspect, float Offset, Unicolour LineColor,
    Unicolour CellColor, Unicolour AlternateColor, GeneratedLayer Layer, int Seed)
    : IPixelStage<Grid> {
    public Option<ShortSideLength> LineWidth { get; init; }
    public Option<ShortSideLength> Feather { get; init; }

    public static Grid Default { get; } = new(
        GridPattern.Lines, FieldPlacement.Default, ShortSideLength.Create(1f / 3f), 1f, 0f, new Unicolour(ColourSpace.Rgb, 1d, 1d, 1d), new Unicolour(ColourSpace.Rgb, 0d, 0d, 0d),
        new Unicolour(ColourSpace.Rgb, 1d, 1d, 1d), GeneratedLayer.Default, 0) {
        LineWidth = ShortSideLength.Create(2f * (0.02f + (0.18f * 0.2f)) * (0.1f + (0.4f * 0.1f)) / 3f),
    };

    public static Fin<Option<PixelPass>> Pass(Grid state, PassContext context) =>
        state.Layer.Pass(DistanceField.Fill(state.Placement.Frame(context.Extent), state.Pattern.Shade(
            new BrickPattern(state.Cell, state.Cell * state.Aspect, state.Offset, 2, 1f, 0, state.LineWidth.Share,
                state.Feather.Map(feather => DistanceField.Band(feather, context.Extent)).IfNone(1f) / context.Extent.ShortSide),
            RampLight.Light(state.LineColor, context).AsVector3(), RampLight.Light(state.CellColor, context).AsVector3(), RampLight.Light(state.AlternateColor, context).AsVector3(), CoordinateHash.Branch(state.Seed, 0u, 18u))));
}

public sealed record Stripes(
    WaveProfile Profile, FieldPlacement Placement, ShortSideLength Wavelength, Angle Bend, float Phase, float Speed, Seq<RampStop> Colors, GeneratedLayer Layer, Timing Timing)
    : IPixelStage<Stripes> {
    public static Stripes Default { get; } = new(WaveProfile.Sine, FieldPlacement.Default, ShortSideLength.Create(float.Tau / 20f), Angle.Zero, 0f, 0f, Presets.Grayscale, GeneratedLayer.Default, Timing.Default);

    public static Fin<Option<PixelPass>> Pass(Stripes state, PassContext context) {
        ((float sin, float cos), double start, float wavelength) = (float.SinCos((float)state.Bend.Radians), state.Phase + ((double)state.Speed * (float)state.Timing.At(context).TotalSeconds), state.Wavelength);
        (double width, RampIntegral footprint) = (1d / state.Wavelength.Pixels(context.Extent), RampLight.Integral(state.Colors, Some(state.Profile), context));
        return state.Layer.Pass(DistanceField.Fill(state.Placement.Frame(context.Extent), point =>
            footprint.PeriodicMean((((point.Y * cos) + (float.Abs(point.X) * sin)) / wavelength) + start, width)));
    }
}
