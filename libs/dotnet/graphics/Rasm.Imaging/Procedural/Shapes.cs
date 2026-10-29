using System.Drawing;
using System.Numerics;
using Rasm.Imaging.Pixels;
using UnitsNet;

namespace Rasm.Imaging.Procedural;

// --- [MODELS] --------------------------------------------------------------------------
public sealed class DistanceField {
    private readonly Func<Vector2, (float Distance, float Width)> sample;

    private DistanceField(Func<Vector2, (float Distance, float Width)> sample) => this.sample = sample;

    // --- [PRIMITIVES]
    public static DistanceField Empty { get; } = new(static _ => (float.PositiveInfinity, 0f));

    public static DistanceField Ellipse(Vector2 half, float exponent) =>
        (2f * float.Min(half.X, half.Y)) switch { var width => new(point => (Superellipse(point, half, exponent), width)) };

    public static DistanceField Box(Vector2 half, float roundness) =>
        float.Min(half.X, half.Y) switch { var small => new(point => (Rounded(point, half, roundness * small), 2f * small)) };

    public static DistanceField Polygon(Vector2 half, int sides, float inset, float roundness) {
        float wedge = float.Pi / sides;
        float radius = roundness * half.X;
        (float sine, float cosine) = float.SinCos(wedge);
        (float depth, float reach) = (cosine * (1f - inset), half.X - radius);
        (Vector2 outer, Vector2 inner, float width) = (new(0f, reach), new(reach * depth * sine, reach * depth * cosine), half.X * (1f + depth));
        return new(point => float.SinCos(float.Abs(float.Ieee754Remainder(float.Atan2(point.X, point.Y), 2f * wedge))) switch {
            var (sin, cos) => (point.Length() * new Vector2(sin, cos)) switch { var folded => (float.CopySign(Segment(folded, outer, inner).Length(), Side(folded, outer, inner)) - radius, width) },
        });
    }

    public static DistanceField Ring(Vector2 half, float exponent, float inset) =>
        (inset * float.Min(half.X, half.Y)) switch { var band => new(point => (float.Abs(Superellipse(point, half, exponent) + (band * 0.5f)) - (band * 0.5f), band)) };

    public static DistanceField Cross(Vector2 half, float inset, float roundness) =>
        ((1f - inset) * float.Min(half.X, half.Y)) switch {
            var arm => new(point => (float.Min(Rounded(point, half with { Y = arm }, roundness * arm), Rounded(point, half with { X = arm }, roundness * arm)), 2f * arm)),
        };

    public static DistanceField Heart(Vector2 half) {
        (Vector2 center, float radius) = (new(0.25f, 0.75f), float.Sqrt(2f) * 0.25f);
        (float scale, float middle) = (half.X / (center.X + radius), (center.Y + radius) * 0.5f);
        return new(point => (new Vector2(float.Abs(point.X), point.Y + (middle * scale)) / scale) switch {
            var q when q.X + q.Y > 1f => (scale * (Vector2.Distance(q, center) - radius), 2f * radius * scale),
            var q => (scale * float.Min(Vector2.Distance(q, Vector2.UnitY), Vector2.Distance(q, new Vector2(0.5f * float.Max(q.X + q.Y, 0f)))) * float.Sign(q.X - q.Y), 2f * radius * scale),
        });
    }

    public static DistanceField Triangle(Vector2 half, float apex, float roundness) {
        (Vector2 a, Vector2 b, Vector2 c) = (-half, half with { Y = -half.Y }, half with { X = apex * half.X });
        (float opposite, float across, float under) = (Vector2.Distance(b, c), Vector2.Distance(c, a), Vector2.Distance(a, b));
        (float perimeter, float twiceArea) = (opposite + across + under, 4f * half.X * half.Y);
        Vector2 incenter = ((opposite * a) + (across * b) + (under * c)) / perimeter;
        (Vector2 p, Vector2 q, Vector2 r) = (Vector2.Lerp(incenter, a, 1f - roundness), Vector2.Lerp(incenter, b, 1f - roundness), Vector2.Lerp(incenter, c, 1f - roundness));
        (float radius, float width) = (roundness * twiceArea / perimeter, twiceArea / float.Max(under, float.Max(opposite, across)));
        return new(point => ((float.Min(Segment(point, p, q).Length(), float.Min(Segment(point, q, r).Length(), Segment(point, r, p).Length()))
            * (float.Min(Side(point, p, q), float.Min(Side(point, q, r), Side(point, r, p))) > 0f ? -1f : 1f)) - radius, width));
    }

    public static DistanceField Capsule(Vector2 start, Vector2 end, float from, float to) {
        (Vector2 axis, float lean) = (end - start, from - to);
        float squared = axis.LengthSquared();
        Vector2 tangent = new(float.Sqrt(float.Max(squared - (lean * lean), 0f)), lean);
        return tangent.X is 0f
            ? (from >= to ? (start, from) : (end, to)) switch { var (center, radius) => new(point => (Vector2.Distance(point, center) - radius, 2f * radius)) }
            : new(point => (new Vector2(float.Abs(Vector2.Cross(point - start, axis)), Vector2.Dot(point - start, axis)) / squared) switch {
                var q => Vector2.Cross(tangent, q) switch {
                    < 0f => (float.Sqrt(squared * q.LengthSquared()) - from, 2f * from),
                    var k when k > tangent.X => (float.Sqrt(squared * (q.LengthSquared() + 1f - (2f * q.Y))) - to, 2f * to),
                    var k => (Vector2.Dot(tangent, q) - from, 2f * float.Lerp(from, to, k / tangent.X)),
                },
            });
    }

    // --- [COMPOSITION]
    public DistanceField Inverted() => new(point => (-sample(point).Distance, float.PositiveInfinity));

    public DistanceField Union(DistanceField other, float smoothing) =>
        new(point => (sample(point), other.sample(point)) switch { var (a, b) => (SmoothMinimum(a.Distance, b.Distance, smoothing), float.Max(a.Width, b.Width)) });

    public DistanceField Difference(DistanceField other, float smoothing) =>
        new(point => (sample(point), other.sample(point)) switch { var (a, b) => (-SmoothMinimum(-a.Distance, b.Distance, smoothing), a.Width) });

    public DistanceField Intersection(DistanceField other, float smoothing) =>
        new(point => (sample(point), other.sample(point)) switch { var (a, b) => (-SmoothMinimum(-a.Distance, -b.Distance, smoothing), float.Min(a.Width, b.Width)) });

    public DistanceField Morph(DistanceField other, float progress) =>
        progress switch {
            0f => this,
            1f => other,
            _ => new(point => (sample(point), other.sample(point)) switch {
                var (a, b) => (a.Width is 0f || b.Width is 0f ? float.PositiveInfinity : float.Lerp(a.Distance, b.Distance, progress), float.Lerp(a.Width, b.Width, progress)),
            }),
        };

    // --- [EVALUATION]
    public float Distance(Vector2 point) => sample(point).Distance;

    public float Coverage(Vector2 point, float growth, float band) =>
        sample(point) switch { var (distance, width) => float.Min(Coverage(distance - growth, band), (width + (2f * growth)) / band) };

    public static float Coverage(float distance, float band) => float.Clamp(0.5f - (distance / band), 0f, 1f);

    public static float Stroke(float distance, float width, float band) =>
        (float.Min(width, band), float.Max(width, band)) switch { var (small, large) => float.Max(0f, float.Min(small, (small / 2f) - (float.Abs(distance) - (large / 2f)))) / band };

    public static float Band(ShortSideLength feather, PixelExtent extent, float scale = 1f) => float.Max((float)feather.Pixels(extent) * scale, 1f);

    public static T SmoothStep<T>(T edge0, T edge1, T x) where T : IFloatingPoint<T> =>
        T.Clamp((x - edge0) / (edge1 - edge0), T.Zero, T.One) switch { var t => t * t * (T.CreateTruncating(3) - (T.CreateTruncating(2) * t)) };

    public static T SmootherStep<T>(T edge0, T edge1, T x) where T : IFloatingPoint<T> =>
        T.Clamp((x - edge0) / (edge1 - edge0), T.Zero, T.One) switch { var t => t * t * t * ((t * ((T.CreateTruncating(6) * t) - T.CreateTruncating(15))) + T.CreateTruncating(10)) };

    // --- [FRAMES]
    public static Matrix3x2 Frame(PixelExtent extent, Vector2 center, Angle rotation) =>
        Matrix3x2.CreateTranslation(new Vector2(0.5f) - center) * Matrix3x2.CreateScale(1f / extent.ShortSide) * Matrix3x2.CreateRotation((float)rotation.Radians) * Matrix3x2.CreateScale(1f, -1f);

    public static Action<Span<Vector4>, int, int> Fill(Matrix3x2 frame, Func<Vector2, Vector4> shade) =>
        (row, x, y) => {
            for (int i = 0; i < row.Length; i++)
                row[i] = shade(Vector2.Transform(new Vector2(x + i, y), frame));
        };

    // --- [GEOMETRY]
    private static Vector2 Segment(Vector2 point, Vector2 start, Vector2 end) =>
        (end - start, point - start) switch {
            var (edge, reach) => reach - (edge * (edge == Vector2.Zero ? 0f : float.Clamp(Vector2.Dot(reach, edge) / edge.LengthSquared(), 0f, 1f))),
        };

    private static float Side(Vector2 point, Vector2 start, Vector2 end) => Vector2.Cross(end - start, point - start);

    private static float Rounded(Vector2 point, Vector2 extent, float radius) =>
        (Vector2.Abs(point) - extent + new Vector2(radius)) switch { var corner => Vector2.Max(corner, Vector2.Zero).Length() + float.Min(float.Max(corner.X, corner.Y), 0f) - radius };

    private static float SmoothMinimum(float first, float second, float smoothing) =>
        (float.Min(first, second), smoothing - float.Abs(first - second)) switch {
            (var low, > 0f and var overlap) => low - (overlap * overlap / (4f * smoothing)),
            (var low, _) => low,
        };

    private static float Superellipse(Vector2 point, Vector2 extent, float exponent) {
        float length = point.Length();
        Vector2 direction = length > 0f ? Vector2.Abs(point / length) / extent : new Vector2(0f, 1f / extent.Y);
        float largest = float.Max(direction.X, direction.Y);
        float level = largest * float.Pow(float.Pow(direction.X / largest, exponent) + float.Pow(direction.Y / largest, exponent), 1f / exponent);
        float slope = exponent is 1f ? (Vector2.One / extent).Length()
            : new Vector2(float.Pow(direction.X / level, exponent - 1f) / extent.X, float.Pow(direction.Y / level, exponent - 1f) / extent.Y).Length();
        return ((length * level) - 1f) / slope;
    }
}

public sealed record BrickPattern(float RowHeight, float BrickWidth, float Offset, int OffsetFrequency, float Squash, int SquashFrequency, float MortarSize, float Band) {
    public float Mortar(Vector2 point) => DistanceField.Stroke(Locate(point).Side, MortarSize, Band);

    public Vector3 Bricks(Vector2 point, Func<long, long, Vector3> color, Vector3 stroke) =>
        Locate(point) switch {
            var (i, j, side, acrossI, acrossJ) => Vector3.Lerp(Vector3.Lerp(color(i, j), color(acrossI, acrossJ), DistanceField.Coverage(side, Band)), stroke, DistanceField.Stroke(side, MortarSize, Band)),
        };

    private (long I, long J, float Side, long AcrossI, long AcrossJ) Locate(Vector2 point) {
        float down = point.Y / RowHeight;
        long j = (long)float.Floor(down);
        float width = Width(j);
        float along = (point.X / width) + Shift(j);
        long i = (long)float.Floor(along);
        (float horizontal, float vertical) = (along - i, down - j);
        (float left, float top, float right, float bottom) = (horizontal * width, vertical * RowHeight, (1f - horizontal) * width, (1f - vertical) * RowHeight);
        float side = float.Min(float.Min(left, right), float.Min(top, bottom));
        return side == left ? (i, j, side, i - 1L, j)
            : side == right ? (i, j, side, i + 1L, j)
            : side == top ? (i, j, side, Column(point.X, j - 1L), j - 1L)
            : (i, j, side, Column(point.X, j + 1L), j + 1L);
    }

    private long Column(float x, long j) => (long)float.Floor((x / Width(j)) + Shift(j));

    private float Width(long j) => SquashFrequency > 0 && j % SquashFrequency is 0L ? BrickWidth * Squash : BrickWidth;

    private float Shift(long j) => OffsetFrequency > 0 && j % OffsetFrequency is 0L ? Offset : 0f;
}

public sealed record Extinction(float Start, float AttenuationLength, Option<float> Cutoff = default) {
    public Func<int, int, float> Field(PixelFrame depth, Camera camera, Func<int, int, float, float> density) {
        Vector3 axis = Vector3.Transform(-Vector3.UnitZ, camera.Orientation);
        return (x, y) => camera.Ray(new Vector2(x + 0.5f, y + 0.5f)).Match(
            Some: ray => (depth.Texel(new Point(x, y), (WrapMode.Black, WrapMode.Black)).Match(Some: static texel => texel.X, None: static () => float.PositiveInfinity) / Vector3.Dot(ray.Direction, axis)) switch {
                var distance when Cutoff.Exists(cutoff => distance > cutoff) => 0f,
                var distance => (float.MaxNumber(distance - Start, 0f) / AttenuationLength) switch { var path => float.IsPositiveInfinity(path) ? 1f : 1f - float.Exp(-path * density(x, y, path)) },
            },
            None: static () => 0f);
    }
}
