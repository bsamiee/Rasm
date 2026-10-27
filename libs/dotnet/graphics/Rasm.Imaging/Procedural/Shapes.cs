using System.Numerics;
using MathNet.Numerics.Integration;
using Rasm.Imaging.Pixels;

namespace Rasm.Imaging.Procedural;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record FieldFrame(PixelExtent Extent, (double X, double Y) Center, (double Sin, double Cos) Turn) {
    public static FieldFrame Centered(PixelExtent extent) => new(extent, FramePosition.Center.Point(extent), (0d, 1d));

    public (double X, double Y) Local((double X, double Y) point) =>
        ((point.X - Center.X) / Extent.ShortSide, (point.Y - Center.Y) / Extent.ShortSide) switch {
            var (x, y) => ((x * Turn.Cos) - (y * Turn.Sin), (x * Turn.Sin) + (y * Turn.Cos)),
        };

    public (double X, double Y) Upward(long column, long line) => Local((column + 0.5d, Extent.Height - line - 0.5d)) switch { var (x, y) => (x, -y) };

    public Action<Span<Vector4>, long, long> Fill(Func<(double X, double Y), Vector4> shade) =>
        (row, column, line) => {
            for (int i = 0; i < row.Length; i++)
                row[i] = shade(Upward(column + i, line));
        };
}

public sealed record DistanceField(Func<(double X, double Y), double> Distance, double Width) {
    // --- [FIELDS]
    public static DistanceField Empty { get; } = new(static _ => double.PositiveInfinity, 0d);

    public static DistanceField Ellipse((double X, double Y) half, float exponent) =>
        new(point => Superellipse(point, half, exponent), 2d * double.Min(half.X, half.Y));

    public static DistanceField Box((double X, double Y) half, float roundness) =>
        double.Min(half.X, half.Y) switch { var small => new(point => Rounded(point, half, roundness * small), 2d * small) };

    public static DistanceField Polygon((double X, double Y) half, int sides, float inset, float roundness) {
        double wedge = double.Pi / sides;
        double radius = roundness * half.X;
        (double sine, double cosine) = double.SinCos(wedge);
        (double depth, double reach) = (cosine * (1d - inset), half.X - radius);
        ((double X, double Y) outer, (double X, double Y) inner) = ((0d, reach), (reach * depth * sine, reach * depth * cosine));
        return new(point => (double.SinCos(double.Abs(double.Ieee754Remainder(double.Atan2(point.X, point.Y), 2d * wedge))), Length(point)) switch {
            var ((sin, cos), length) => (X: length * sin, Y: length * cos) switch { var folded => double.CopySign(Length(Offset(folded, outer, inner).Offset), Side(folded, outer, inner)) - radius },
        }, half.X * (1d + depth));
    }

    public static DistanceField Ring((double X, double Y) half, float exponent, float inset) =>
        (inset * double.Min(half.X, half.Y)) switch {
            var band => new(point => double.Abs(Superellipse(point, half, exponent) + (band * 0.5d)) - (band * 0.5d), band),
        };

    public static DistanceField Cross((double X, double Y) half, float inset, float roundness) =>
        ((1d - inset) * double.Min(half.X, half.Y)) switch {
            var arm => (roundness * arm) switch { var radius => new(point => double.Min(Rounded(point, (half.X, arm), radius), Rounded(point, (arm, half.Y), radius)), 2d * arm) },
        };

    public static DistanceField Heart((double X, double Y) half) {
        ((double X, double Y) center, double radius) = ((0.25d, 0.75d), double.Sqrt(2d) * 0.25d);
        (double scale, double middle) = (half.X / (center.X + radius), (center.Y + radius) * 0.5d);
        return new(point => (X: double.Abs(point.X) / scale, Y: (point.Y / scale) + middle) switch {
            var q when q.X + q.Y > 1d => scale * (Length((q.X - center.X, q.Y - center.Y)) - radius),
            var q => (0.5d * double.Max(q.X + q.Y, 0d)) switch { var diagonal => scale * double.Min(Length((q.X, q.Y - 1d)), Length((q.X - diagonal, q.Y - diagonal))) * double.Sign(q.X - q.Y) },
        }, 2d * radius * scale);
    }

    public static DistanceField Triangle((double X, double Y) half, float apex, float roundness) {
        ((double X, double Y) a, (double X, double Y) b, (double X, double Y) c) = ((-half.X, -half.Y), (half.X, -half.Y), (apex * half.X, half.Y));
        (double opposite, double across, double under) = (Length((b.X - c.X, b.Y - c.Y)), Length((c.X - a.X, c.Y - a.Y)), Length((a.X - b.X, a.Y - b.Y)));
        (double perimeter, double twiceArea) = (opposite + across + under, 4d * half.X * half.Y);
        (double X, double Y) = (((opposite * a.X) + (across * b.X) + (under * c.X)) / perimeter, ((opposite * a.Y) + (across * b.Y) + (under * c.Y)) / perimeter);
        Func<(double X, double Y), (double X, double Y)> inward = vertex => (X + ((vertex.X - X) * (1d - roundness)), Y + ((vertex.Y - Y) * (1d - roundness)));
        ((double X, double Y) p, (double X, double Y) q, (double X, double Y) r, double radius) = (inward(a), inward(b), inward(c), roundness * twiceArea / perimeter);
        return new(point => (Edge(point, p, q), Edge(point, q, r), Edge(point, r, p)) switch {
            var (first, second, third) => (double.Min(first.Distance, double.Min(second.Distance, third.Distance)) * (double.Min(first.Side, double.Min(second.Side, third.Side)) > 0d ? -1d : 1d)) - radius,
        }, twiceArea / double.Max(under, double.Max(opposite, across)));

        static (double Distance, double Side) Edge((double X, double Y) point, (double X, double Y) start, (double X, double Y) end) =>
            (Length(Offset(point, start, end).Offset), Side(point, start, end));
    }

    // --- [SAMPLES]
    public static (double Distance, double Width) Outside => (double.PositiveInfinity, 0d);

    public (double Distance, double Width) Sample((double X, double Y) point) => (Distance(point), Width);

    public static (double Distance, double Width) Tapered((double X, double Y) point, (double X, double Y) start, (double X, double Y) end, double from, double to) =>
        Offset(point, start, end) switch {
            var (offset, along) => (from + ((to - from) * along)) switch { var radius => (Length(offset) - radius, 2d * radius) },
        };

    public static (double Distance, double Width) Inverted((double Distance, double Width) sample) => (-sample.Distance, double.PositiveInfinity);

    public static (double Distance, double Width) Union((double Distance, double Width) first, (double Distance, double Width) second, double smoothing) =>
        (SmoothMinimum(first.Distance, second.Distance, smoothing), double.Max(first.Width, second.Width));

    public static (double Distance, double Width) Difference((double Distance, double Width) first, (double Distance, double Width) second, double smoothing) =>
        (-SmoothMinimum(-first.Distance, second.Distance, smoothing), first.Width);

    public static (double Distance, double Width) Intersection((double Distance, double Width) first, (double Distance, double Width) second, double smoothing) =>
        (-SmoothMinimum(-first.Distance, -second.Distance, smoothing), double.Min(first.Width, second.Width));

    public static (double Distance, double Width) Morph((double Distance, double Width) first, (double Distance, double Width) second, float progress) =>
        progress switch {
            0f => first,
            1f => second,
            _ => (first.Width is 0d || second.Width is 0d ? double.PositiveInfinity : first.Distance + ((second.Distance - first.Distance) * progress),
                first.Width + ((second.Width - first.Width) * progress)),
        };

    public static double Coverage((double Distance, double Width) sample, double growth, double band) =>
        double.Min(double.Clamp(0.5d - ((sample.Distance - growth) / band), 0d, 1d), (sample.Width + (2d * growth)) / band);

    // --- [GEOMETRY]
    public static double Length((double X, double Y) point) => double.Hypot(point.X, point.Y);

    public static ((double X, double Y) Offset, double Along) Offset((double X, double Y) point, (double X, double Y) start, (double X, double Y) end) {
        ((double X, double Y) edge, (double X, double Y) reach) = ((end.X - start.X, end.Y - start.Y), (point.X - start.X, point.Y - start.Y));
        double squared = (edge.X * edge.X) + (edge.Y * edge.Y);
        double along = squared is 0d ? 0d : double.Clamp(((reach.X * edge.X) + (reach.Y * edge.Y)) / squared, 0d, 1d);
        return ((reach.X - (edge.X * along), reach.Y - (edge.Y * along)), along);
    }

    public static double Side((double X, double Y) point, (double X, double Y) start, (double X, double Y) end) =>
        ((end.X - start.X) * (point.Y - start.Y)) - ((end.Y - start.Y) * (point.X - start.X));

    public static double Rounded((double X, double Y) point, (double X, double Y) extent, double radius) =>
        (X: double.Abs(point.X) - extent.X + radius, Y: double.Abs(point.Y) - extent.Y + radius) switch {
            var corner => Length((double.Max(corner.X, 0d), double.Max(corner.Y, 0d))) + double.Min(double.Max(corner.X, corner.Y), 0d) - radius,
        };

    private static double SmoothMinimum(double first, double second, double smoothing) =>
        (double.Min(first, second), smoothing - double.Abs(first - second)) switch {
            (var low, > 0d and var overlap) => low - (overlap * overlap / (4d * smoothing)),
            (var low, _) => low,
        };

    private static double Superellipse((double X, double Y) point, (double X, double Y) extent, double exponent) {
        double length = Length(point);
        (double x, double y) = length > 0d ? (double.Abs(point.X / length) / extent.X, double.Abs(point.Y / length) / extent.Y) : (0d, 1d / extent.Y);
        double largest = double.Max(x, y);
        double level = largest * double.Pow(double.Pow(x / largest, exponent) + double.Pow(y / largest, exponent), 1d / exponent);
        double slope = exponent is 1d
            ? Length((1d / extent.X, 1d / extent.Y))
            : Length((double.Pow(x / level, exponent - 1d) / extent.X, double.Pow(y / level, exponent - 1d) / extent.Y));
        return ((length * level) - 1d) / slope;
    }
}

public sealed record GridLattice(double Height, double Span, double Shift, double Line, double Band) {
    public float Lines((double X, double Y) point) => ShapeEdge.Strip(Locate(point).Side, Line, Band);

    public Vector3 Cells((double X, double Y) point, Func<long, long, Vector3> color, Vector3 ink) =>
        Locate(point) switch {
            var (i, j, side, acrossI, acrossJ) => Vector3.Lerp(Vector3.Lerp(color(i, j), color(acrossI, acrossJ), ShapeEdge.Coverage(side, Band)), ink, ShapeEdge.Strip(side, Line, Band)),
        };

    private (long I, long J, double Side, long AcrossI, long AcrossJ) Locate((double X, double Y) point) {
        double down = point.Y / Height;
        double j = double.Floor(down);
        double along = (point.X / Span) + Offset((long)j);
        double i = double.Floor(along);
        (double horizontal, double vertical) = (along - i, down - j);
        (double x, double y, double right, double bottom) = (horizontal * Span, vertical * Height, (1d - horizontal) * Span, (1d - vertical) * Height);
        double side = double.Min(double.Min(x, right), double.Min(y, bottom));
        (long column, long line) = ((long)i, (long)j);
        return side == x ? (column, line, side, column - 1L, line)
            : side == right ? (column, line, side, column + 1L, line)
            : side == y ? (column, line, side, Column(point.X, line - 1L), line - 1L)
            : (column, line, side, Column(point.X, line + 1L), line + 1L);
    }

    private long Column(double x, long j) => (long)double.Floor((x / Span) + Offset(j));

    private double Offset(long j) => long.IsEvenInteger(j) ? Shift : 0d;
}

public sealed record Extinction(float Start, float Attenuation) {
    public Option<float> Cutoff { get; init; }

    public Func<long, long, float> Field(PixelFrame depth, Camera camera, Func<long, long, float, float> density) {
        Vector3 axis = Vector3.Transform(-Vector3.UnitZ, camera.Orientation);
        Func<float, bool> beyond = Cutoff.Match(Some: static cutoff => (Func<float, bool>)(distance => distance > cutoff), None: static () => static _ => false);
        return (column, line) => {
            (double X, double Y) point = (column + 0.5d, camera.Extent.Height - line - 0.5d);
            float distance = depth.Sample(point, (WrapMode.Black, WrapMode.Black), float.PositiveInfinity, static (_, sample, _) => sample.X) / Vector3.Dot(camera.Ray(point).Direction, axis);
            float path = float.MaxNumber(distance - Start, 0f) / Attenuation;
            return beyond(distance) ? 0f : float.IsPositiveInfinity(path) ? 1f : 1f - float.Exp(-path * density(column, line, path));
        };
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class ShapeEdge {
    private const int QuadratureOrder = 32;

    public static double Band(ShortSideLength feather, PixelExtent extent, double scale = 1d) => double.Max(feather.Pixels(extent) * scale, 1d);

    public static float Coverage(double distance, double band) => float.Clamp((float)(0.5d - (distance / band)), 0f, 1f);

    public static float Strip(double distance, double width, double band) =>
        (double.Min(width, band), double.Max(width, band)) switch {
            var (small, large) => (float)(double.Max(0d, double.Min(small, (small / 2d) - (double.Abs(distance) - (large / 2d)))) / band),
        };

    public static double DiscMass(double radius, double softness, (double Left, double Top, double Right, double Bottom) box) {
        (double lowX, double lowY, double highX, double highY) = (double.Max(-radius, box.Left), double.Max(-radius, box.Top), double.Min(radius, box.Right), double.Min(radius, box.Bottom));
        (double nearestX, double nearestY) = (double.Max(lowX, double.Min(0d, highX)), double.Max(lowY, double.Min(0d, highY)));
        return lowX >= highX || lowY >= highY || (radius * radius) <= (nearestX * nearestX) + (nearestY * nearestY) ? 0d
            : lowX == -radius && lowY == -radius && highX == radius && highY == radius ? double.Pi * radius * radius * (1d - softness + (0.3d * softness * softness))
            : Partial(radius, softness, (lowX, lowY, highX, highY), double.Sqrt((radius * radius) - (nearestY * nearestY)));

        static double Partial(double radius, double softness, (double LowX, double LowY, double HighX, double HighY) clipped, double horizontal) =>
            (softness * radius) switch {
                var band => GaussLegendreRule.Integrate(x => double.Sqrt((radius * radius) - (x * x)) switch {
                    var vertical => GaussLegendreRule.Integrate(
                        y => Smooth(band is 0d ? 1d : double.Min(1d, (radius - double.Hypot(x, y)) / band)), double.Max(clipped.LowY, -vertical), double.Min(clipped.HighY, vertical), QuadratureOrder),
                }, double.Max(clipped.LowX, -horizontal), double.Min(clipped.HighX, horizontal), QuadratureOrder),
            };
    }

    public static T Smooth<T>(T t) where T : IFloatingPoint<T> => t * t * (T.CreateTruncating(3) - (T.CreateTruncating(2) * t));

    public static T Smoother<T>(T t) where T : IFloatingPoint<T> => t * t * t * ((t * ((T.CreateTruncating(6) * t) - T.CreateTruncating(15))) + T.CreateTruncating(10));
}
