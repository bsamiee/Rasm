using System.Numerics;
using Darkroom.Chain;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Procedural;
using UnitsNet;
using Wacton.Unicolour;

namespace Darkroom.Catalog;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class ShapeFit {
    public static readonly ShapeFit ShortSide = new("short-side", static _ => Vector2.One);
    public static readonly ShapeFit LongSide = new("long-side", static extent => new Vector2((float)int.Max(extent.Width, extent.Height) / extent.ShortSide));

    [UseDelegateFromConstructor]
    public partial Vector2 Scale(PixelExtent extent);
}

[SmartEnum<string>]
public sealed partial class ShapeKind {
    public static readonly ShapeKind Ellipse = new("ellipse", static (shape, half) => DistanceField.Ellipse(half, shape.Exponent));
    public static readonly ShapeKind Box = new("box", static (shape, half) => DistanceField.Box(half, shape.Roundness));
    public static readonly ShapeKind Polygon = new("polygon", static (shape, half) => DistanceField.Polygon(half, shape.Sides, shape.Inset, shape.Roundness));
    public static readonly ShapeKind Ring = new("ring", static (shape, half) => DistanceField.Ring(half, shape.Exponent, shape.Inset));
    public static readonly ShapeKind Cross = new("cross", static (shape, half) => DistanceField.Cross(half, shape.Inset, shape.Roundness));
    public static readonly ShapeKind Heart = new("heart", static (_, half) => DistanceField.Heart(half));
    public static readonly ShapeKind Triangle = new("triangle", static (shape, half) => DistanceField.Triangle(half, shape.Apex, shape.Roundness));

    [UseDelegateFromConstructor]
    public partial DistanceField Field(ShapePrimitive shape, Vector2 half);
}

[SmartEnum<string>]
public sealed partial class ShapeOperation {
    public static readonly ShapeOperation Union = new("union", static (first, second, smoothing, _) => first.Union(second, smoothing));
    public static readonly ShapeOperation Difference = new("difference", static (first, second, smoothing, _) => first.Difference(second, smoothing));
    public static readonly ShapeOperation Intersection = new("intersection", static (first, second, smoothing, _) => first.Intersection(second, smoothing));
    public static readonly ShapeOperation Morph = new("morph", static (first, second, _, progress) => first.Morph(second, progress));

    [UseDelegateFromConstructor]
    public partial DistanceField Join(DistanceField first, DistanceField second, float smoothing, float progress);
}

public sealed record ShapePrimitive(ShapeKind Kind, FieldPlacement Placement, ShortSideLength Width, ShortSideLength Height, float Roundness, float Exponent, int Sides, float Inset, float Apex, ShapeFit Fit) {
    public static ShapePrimitive Default { get; } = new(ShapeKind.Ellipse, FieldPlacement.Default, ShortSideLength.Create(0.5f), ShortSideLength.Create(0.5f), 0f, 2f, 5, 0f, 0f, ShapeFit.ShortSide);

    public DistanceField Field(PixelExtent extent) => Placement.Placed(Kind.Field(this, new Vector2(Width, Height) * Fit.Scale(extent)), extent);
}

public sealed record ShapeStyle(Unicolour Outline, bool Inverted) {
    private static readonly NoiseBasis Gate = NoiseBasis.Perlin.Default with {
        Dimensions = NoiseDimensions.Four,
        Octaves = new(15f, 0.9f, 2f),
        Distortion = 1.9f,
        Fractal = Fractal.RidgedMultifractal,
        Gain = 74.5f,
    };

    public Option<Unicolour> Fill { get; init; }
    public Option<ShortSideLength> OutlineWidth { get; init; }
    public Option<ShortSideLength> OutlineOffset { get; init; }
    public Option<ShortSideLength> Feather { get; init; }
    public Option<ShortSideLength> Roughness { get; init; }

    public static ShapeStyle Default { get; } = new(new Unicolour(ColourSpace.Rgb, 0d, 0d, 0d), Inverted: false) { Fill = new Unicolour(ColourSpace.Rgb, 1d, 1d, 1d) };

    public Fin<Func<Vector2, Vector4>> Shading(DistanceField field, PassContext context, uint seed) {
        DistanceField shape = Inverted ? field.Inverted() : field;
        (Vector4 fill, Vector3 outline) = (Fill.Map(color => new Vector4(RampLight.Light(color, context).AsVector3(), 1f)).IfNone(Vector4.Zero), RampLight.Light(Outline, context).AsVector3());
        float band = Feather.Map(feather => DistanceField.Band(feather, context.Extent)).IfNone(1f) / context.Extent.ShortSide;
        (float offset, float width, float reach) = (OutlineOffset.Share, OutlineWidth.Share, Roughness.Share);
        (float near, float far) = (-reach - (band / 2f), offset + width + reach + (band / 2f));
        return Gate.Sampler.Map(gate => new Func<Vector2, Vector4>(point => {
            float growth = reach is 0f || (shape.Distance(point) is var distance && (distance < near || distance > far)) ? 0f
                : -reach * ((2f * float.Clamp(gate(new Vector4(2.5f * point, 0f, 0f), seed).Value, 0f, 1f)) - 1f);
            float covered = fill.W is 0f ? 0f : fill.W * shape.Coverage(point, growth, band);
            float ring = width is 0f ? 0f : shape.Coverage(point, offset + width + growth, band) - shape.Coverage(point, offset + growth, band);
            return covered + ring > 0f ? new Vector4(((covered * fill.AsVector3()) + (ring * outline)) / (covered + ring), covered + ring) : Vector4.Zero;
        }));
    }
}

public sealed record Shapes(ShapePrimitive First, ShapePrimitive Second, float Progress, ShapeStyle Style, GeneratedLayer Layer, Seed Seed) : IPixelStage<Shapes> {
    public const uint Stream = 16u;

    public Option<ShapeOperation> Operation { get; init; }
    public Option<ShortSideLength> Smoothing { get; init; }

    public static Shapes Default { get; } = new(
        ShapePrimitive.Default, ShapePrimitive.Default with { Kind = ShapeKind.Box, Roundness = 0.3f }, 0f, ShapeStyle.Default, GeneratedLayer.Default, Seed.MinValue);

    public static Fin<Option<PixelPass>> Pass(Shapes state, PassContext context) {
        DistanceField first = state.First.Field(context.Extent);
        DistanceField field = state.Operation.Map(operation => operation.Join(first, state.Second.Field(context.Extent), state.Smoothing.Share, state.Progress)).IfNone(first);
        return state.Layer.Pass(state.Style.Shading(field, context, state.Seed.Field(Stream, 0u)).Map(shade => DistanceField.Fill(FieldPlacement.Default.Frame(context.Extent), shade)));
    }
}

public sealed record Lightning(
    FieldPlacement Placement, ShortSideLength Length, ShortSideLength Thickness, int Depth, float Displacement, int Branches, float Reach, Angle Spread, float Taper, float Speed,
    ShapeStyle Style, GeneratedLayer Layer, Seed Seed, Timing Timing, Hold Hold) : IPixelStage<Lightning> {
    private const int Levels = 5;
    private const int Forks = 3;

    public static Lightning Default { get; } = new(
        FieldPlacement.Default, ShortSideLength.Create(1f / 2f), ShortSideLength.Create(0.01f / (5f - (4.5f * 0.5f)) * ReferenceFrame.GreaterSide), Levels, 0.5f, 3, 0.4f, Angle.FromRadians(0.5d), 0.5f,
        1f / 0.05f, ShapeStyle.Default, GeneratedLayer.Added with { Exposure = 3f }, Seed.MinValue, Timing.Default, Hold.Create(Duration.FromMilliseconds(50)));

    public static Fin<Option<PixelPass>> Pass(Lightning state, PassContext context) {
        (uint field, float clock, float chord, float thickness, float spread) =
            (state.Seed.Field(17u, 0u), 2f * state.Speed * state.Hold.Held(state.Timing.At(context)), 2f * state.Length, state.Thickness, (float)state.Spread.Radians);
        Func<int, int, int, float> noise = (x, y, z) => 0.5f + (0.5f * NoiseDimensions.Four.Gradient(new Vector4((2f * x) + 0.5f, (2f * y) + 0.5f, (2f * z) + 0.5f, clock), field));
        Vector2[] channel = Path(new Vector2(0f, state.Length), -Vector2.UnitY, chord, Levels, state.Depth, state.Displacement, k => noise(k, 0, 0));
        DistanceField bolt = Enumerable.Range(1, state.Branches).Aggregate(Strokes(channel, _ => thickness / 2f), (joined, i) => (BitOperations.Log2((uint)i), (i % 2 is 0 ? 1f : -1f) * spread * (0.5f + noise(i, 0, 1))) switch {
            var (level, turn) => joined.Union(Strokes(
                Path(channel[((2 * (i - (1 << level))) + 1) * (1 << Levels) / (2 << level)], float.SinCos(turn) switch { var (sin, cos) => new Vector2(sin, -cos) },
                    state.Reach * chord * (0.5f + noise(i, 0, 2)), Forks, state.Depth, state.Displacement, j => noise(j, i, 0)),
                along => state.Taper * thickness * (1f - along) / 2f), 0f),
        });
        return state.Layer.Pass(state.Style.Shading(bolt, context, CoordinateHash.Branch(field, 0u)).Map(shade => DistanceField.Fill(state.Placement.Frame(context.Extent), shade)));
    }

    private static Vector2[] Path(Vector2 start, Vector2 direction, float length, int levels, int depth, float displacement, Func<int, float> draw) {
        int segments = 1 << levels;
        float[] offsets = new float[segments + 1];
        for (int level = 1; level <= levels; level++) {
            for (int step = segments >> level, k = step; k < segments; k += 2 * step)
                offsets[k] = ((offsets[k - step] + offsets[k + step]) / 2f) + (level <= depth ? displacement * (2f * step * length / segments) * ((2f * draw(k)) - 1f) : 0f);
        }
        return [.. offsets.Select((offset, k) => start + (k * length / segments * direction) + (offset * new Vector2(-direction.Y, direction.X)))];
    }

    private static DistanceField Strokes(Vector2[] vertices, Func<float, float> radius) =>
        Enumerable.Range(0, vertices.Length - 1).Aggregate(DistanceField.Empty, (joined, k) => joined.Union(
            DistanceField.Capsule(vertices[k], vertices[k + 1], radius(k / (vertices.Length - 1f)), radius((k + 1f) / (vertices.Length - 1f))), 0f));
}

public sealed record Letterbox(ShapeStyle Style, GeneratedLayer Layer, Seed Seed) : IPixelStage<Letterbox> {
    public Option<float> Aspect { get; init; }
    public Option<ShortSideLength> Inset { get; init; }
    public Option<ShortSideLength> Radius { get; init; }

    public static Letterbox Default { get; } = new(ShapeStyle.Default with { Fill = new Unicolour(ColourSpace.Rgb, 0d, 0d, 0d), Inverted = true }, GeneratedLayer.Default, Seed.MinValue) { Aspect = 2.39f };

    public static Letterbox FilmGate { get; } = Default with {
        Aspect = None,
        Inset = ShortSideLength.Create(40f / ReferenceFrame.Height),
        Radius = ShortSideLength.Create(40f / ReferenceFrame.Height),
        Style = Default.Style with { Feather = ShortSideLength.Create(10f / 3f * float.Sqrt(float.Tau) / ReferenceFrame.Height), Roughness = ShortSideLength.Create(6f / ReferenceFrame.Height) },
    };

    public static Seq<(string Name, Func<Letterbox, Letterbox> Look)> Looks { get; } = [(nameof(FilmGate), static _ => FilmGate)];

    public static Fin<Option<PixelPass>> Pass(Letterbox state, PassContext context) {
        Vector2 frame = new Vector2(context.Extent.Width, context.Extent.Height) / context.Extent.ShortSide;
        Vector2 half = (state.Aspect.Map(aspect => aspect >= frame.X / frame.Y ? new Vector2(frame.X, frame.X / aspect) : new Vector2(frame.Y * aspect, frame.Y)).IfNone(frame) / 2f)
            - new Vector2(state.Inset.Share);
        float small = float.Min(half.X, half.Y);
        DistanceField window = small <= 0f ? DistanceField.Empty : DistanceField.Box(half, float.Min(state.Radius.Share, small) / small);
        return state.Layer.Pass(state.Style.Shading(window, context, state.Seed.Field(Shapes.Stream, 0u)).Map(shade => DistanceField.Fill(FieldPlacement.Default.Frame(context.Extent), shade)));
    }
}
