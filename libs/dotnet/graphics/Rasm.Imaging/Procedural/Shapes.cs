using System.Numerics;
using PeterO.Numbers;
using Rasm.Imaging.Pixels;

namespace Rasm.Imaging.Procedural;

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct FieldFrame(PixelExtent Extent, (double X, double Y) Center, (double Sin, double Cos) Turn) {
    public static FieldFrame Centered(PixelExtent extent) => new(extent, FramePosition.Center.Point(extent), (0d, 1d));

    public (double X, double Y) Local((double X, double Y) point) =>
        ((point.X - Center.X) / Extent.ShortSide, (point.Y - Center.Y) / Extent.ShortSide) switch {
            var (x, y) => ((x * Turn.Cos) - (y * Turn.Sin), (x * Turn.Sin) + (y * Turn.Cos)),
        };

    public (double X, double Y) Upward(long column, long line) => Local((column + 0.5d, Extent.Height - line - 0.5d)) switch { var (x, y) => (x, -y) };

    public Action<Span<Vector4>, long, long> Fill(Func<(double X, double Y), Vector4> shade) {
        FieldFrame frame = this;
        return (row, column, line) => {
            for (int i = 0; i < row.Length; i++)
                row[i] = shade(frame.Upward(column + i, line));
        };
    }
}

public readonly record struct FieldSample(EFloat Distance, EFloat Width) {
    public static FieldSample Empty { get; } = new(EFloat.PositiveInfinity, EFloat.Zero);

    public FieldSample Inverted => new(-Distance, EFloat.PositiveInfinity);

    public static FieldSample Tapered((EFloat X, EFloat Y) point, (EFloat X, EFloat Y) start, (EFloat X, EFloat Y) end, EFloat from, EFloat to, EContext math) =>
        DistanceField.Offset(point, start, end, math) switch {
            var (offset, along) => (from + ((to - from) * along)) switch { var radius => new(DistanceField.Length(offset, math) - radius, 2 * radius) },
        };

    public FieldSample Union(FieldSample other, EFloat smoothing, EContext math) => new(SmoothMinimum(Distance, other.Distance, smoothing, math), EFloat.Max(Width, other.Width));

    public FieldSample Difference(FieldSample other, EFloat smoothing, EContext math) => new(-SmoothMinimum(-Distance, other.Distance, smoothing, math), Width);

    public FieldSample Intersection(FieldSample other, EFloat smoothing, EContext math) => new(-SmoothMinimum(-Distance, -other.Distance, smoothing, math), EFloat.Min(Width, other.Width));

    public FieldSample Morph(FieldSample other, float progress) =>
        progress switch {
            0f => this,
            1f => other,
            _ => new(Width.IsZero || other.Width.IsZero ? EFloat.PositiveInfinity : Distance + ((other.Distance - Distance) * progress), Width + ((other.Width - Width) * progress)),
        };

    public EFloat Coverage(EFloat growth, EFloat band, EContext math) =>
        EFloat.Min(EFloat.Max(0, EFloat.Min(1, 0.5d - (Distance - growth).Divide(band, math))), (Width + (2 * growth)).Divide(band, math));

    private static EFloat SmoothMinimum(EFloat first, EFloat second, EFloat smoothing, EContext math) =>
        (EFloat.Min(first, second), smoothing - (first - second).Abs()) switch {
            (var low, var overlap) when overlap.Sign > 0 => low - (overlap * overlap).Divide(4 * smoothing, math),
            (var low, _) => low,
        };
}

public sealed record DistanceField(Func<(EFloat X, EFloat Y), EFloat> Distance, EFloat Width) {
    public static DistanceField Empty { get; } = new(static _ => EFloat.PositiveInfinity, EFloat.Zero);

    public FieldSample Sample((EFloat X, EFloat Y) point) => new(Distance(point), Width);

    public static DistanceField Ellipse((EFloat X, EFloat Y) half, float exponent, EContext math) =>
        new(point => Superellipse(point, half, exponent, math), 2 * EFloat.Min(half.X, half.Y));

    public static DistanceField Box((EFloat X, EFloat Y) half, float roundness, EContext math) =>
        EFloat.Min(half.X, half.Y) switch { var small => new(point => Rounded(point, half, roundness * small, math), 2 * small) };

    public static DistanceField Polygon((EFloat X, EFloat Y) half, int sides, float inset, float roundness, EContext math) {
        double wedge = double.Pi / sides;
        EFloat radius = roundness * half.X;
        (double sine, double cosine) = double.SinCos(wedge);
        (EFloat depth, EFloat reach) = ((EFloat)cosine * (1 - EFloat.FromSingle(inset)), half.X - radius);
        ((EFloat X, EFloat Y) outer, (EFloat X, EFloat Y) inner) = ((0, reach), (reach * depth * sine, reach * depth * cosine));
        return new(point => double.SinCos(double.Abs(double.Ieee754Remainder(double.Atan2(point.X.ToDouble(), point.Y.ToDouble()), 2d * wedge))) switch {
            var (sin, cos) => Length(point, math) switch { var length => (X: length * sin, Y: length * cos) } switch {
                var folded => Offset(folded, outer, inner, math).Offset switch { var offset => Length(offset, math).CopySign(Side(folded, outer, inner)) - radius },
            },
        }, half.X * (1 + depth));
    }

    public static DistanceField Ring((EFloat X, EFloat Y) half, float exponent, float inset, EContext math) =>
        (inset * EFloat.Min(half.X, half.Y)) switch {
            var band => new(point => (Superellipse(point, half, exponent, math) + (band * 0.5d)).Abs() - (band * 0.5d), band),
        };

    public static DistanceField Cross((EFloat X, EFloat Y) half, float inset, float roundness, EContext math) =>
        ((1 - EFloat.FromSingle(inset)) * EFloat.Min(half.X, half.Y)) switch {
            var arm => (roundness * arm) switch { var radius => new(point => EFloat.Min(Rounded(point, (half.X, arm), radius, math), Rounded(point, (arm, half.Y), radius, math)), 2 * arm) },
        };

    public static DistanceField Heart((EFloat X, EFloat Y) half, EContext math) =>
        (Center: (X: (EFloat)0.25d, Y: (EFloat)0.75d), Radius: EFloat.FromInt32(2).Sqrt(math) * 0.25d) switch {
            var lobe => (Lobe: lobe, Scale: half.X.Divide(lobe.Center.X + lobe.Radius, math), Middle: (lobe.Center.Y + lobe.Radius) * 0.5d),
        } switch {
            var (lobe, scale, middle) => new(point => (X: point.X.Abs().Divide(scale, math), Y: point.Y.Divide(scale, math) + middle) switch {
                var q => (Q: q, Diagonal: 0.5d * EFloat.Max(q.X + q.Y, 0)),
            } switch {
                var (q, _) when (q.X + q.Y).CompareTo(EFloat.One) > 0 => scale * (Length((q.X - lobe.Center.X, q.Y - lobe.Center.Y), math) - lobe.Radius),
                var (q, diagonal) => scale * EFloat.Min(Length((q.X, q.Y - 1), math), Length((q.X - diagonal, q.Y - diagonal), math)) * (q.X - q.Y).Sign,
            }, 2 * lobe.Radius * scale),
        };

    public static DistanceField Triangle((EFloat X, EFloat Y) half, float apex, float roundness, EContext math) {
        ((EFloat X, EFloat Y) a, (EFloat X, EFloat Y) b, (EFloat X, EFloat Y) c) = ((-half.X, -half.Y), (half.X, -half.Y), (apex * half.X, half.Y));
        (EFloat opposite, EFloat across, EFloat under) = (Length((b.X - c.X, b.Y - c.Y), math), Length((c.X - a.X, c.Y - a.Y), math), Length((a.X - b.X, a.Y - b.Y), math));
        (EFloat perimeter, EFloat twiceArea) = (opposite + across + under, 4 * half.X * half.Y);
        (EFloat X, EFloat Y) = (((opposite * a.X) + (across * b.X) + (under * c.X)).Divide(perimeter, math), ((opposite * a.Y) + (across * b.Y) + (under * c.Y)).Divide(perimeter, math));
        Func<(EFloat X, EFloat Y), (EFloat X, EFloat Y)> inward = vertex => (X + ((vertex.X - X) * (1 - EFloat.FromSingle(roundness))), Y + ((vertex.Y - Y) * (1 - EFloat.FromSingle(roundness))));
        ((EFloat X, EFloat Y) p, (EFloat X, EFloat Y) q, (EFloat X, EFloat Y) r, EFloat radius) = (inward(a), inward(b), inward(c), (roundness * twiceArea).Divide(perimeter, math));
        return new(point => (Edge(point, p, q), Edge(point, q, r), Edge(point, r, p)) switch {
            var (first, second, third) => (EFloat.Min(first.Distance, EFloat.Min(second.Distance, third.Distance)) * (EFloat.Min(first.Side, EFloat.Min(second.Side, third.Side)).Sign > 0 ? -1 : 1)) - radius,
        }, twiceArea.Divide(EFloat.Max(under, EFloat.Max(opposite, across)), math));

        (EFloat Distance, EFloat Side) Edge((EFloat X, EFloat Y) point, (EFloat X, EFloat Y) start, (EFloat X, EFloat Y) end) =>
            Offset(point, start, end, math).Offset switch { var offset => (Length(offset, math), Side(point, start, end)) };
    }

    public static EContext Precision(PixelExtent extent) => EContext.Binary64.WithUnlimitedExponents().WithPrecision(
        double.ILogB((double)float.MaxValue * int.Max(extent.Width, extent.Height)) - double.ILogB((double)float.Epsilon / extent.ShortSide) + EContext.Binary64.Precision.ToInt32Checked());

    public static EFloat Length((EFloat X, EFloat Y) point, EContext math) => ((point.X * point.X) + (point.Y * point.Y)).Sqrt(math);

    public static ((EFloat X, EFloat Y) Offset, EFloat Along) Offset((EFloat X, EFloat Y) point, (EFloat X, EFloat Y) start, (EFloat X, EFloat Y) end, EContext math) =>
        (Edge: (X: end.X - start.X, Y: end.Y - start.Y), Reach: (X: point.X - start.X, Y: point.Y - start.Y)) switch {
            var (edge, reach) => ((edge.X * edge.X) + (edge.Y * edge.Y)) switch {
                var squared when squared.IsZero => EFloat.Zero,
                var squared => EFloat.Max(0, EFloat.Min(1, ((reach.X * edge.X) + (reach.Y * edge.Y)).Divide(squared, math))),
            } switch { var along => ((reach.X - (edge.X * along), reach.Y - (edge.Y * along)), along), },
        };

    public static EFloat Side((EFloat X, EFloat Y) point, (EFloat X, EFloat Y) start, (EFloat X, EFloat Y) end) =>
        ((end.X - start.X) * (point.Y - start.Y)) - ((end.Y - start.Y) * (point.X - start.X));

    public static EFloat Rounded((EFloat X, EFloat Y) point, (EFloat X, EFloat Y) extent, EFloat radius, EContext math) =>
        (X: point.X.Abs() - extent.X + radius, Y: point.Y.Abs() - extent.Y + radius) switch {
            var corner => Length((EFloat.Max(corner.X, 0), EFloat.Max(corner.Y, 0)), math) + EFloat.Min(EFloat.Max(corner.X, corner.Y), 0) - radius,
        };

    private static EFloat Superellipse((EFloat X, EFloat Y) point, (EFloat X, EFloat Y) extent, EFloat exponent, EContext math) {
        EFloat length = Length(point, math);
        (EFloat x, EFloat y) = length.Sign > 0 ? (point.X.Divide(length, math).Abs().Divide(extent.X, math), point.Y.Divide(length, math).Abs().Divide(extent.Y, math)) : (0, EFloat.One.Divide(extent.Y, math));
        EFloat largest = EFloat.Max(x, y);
        EFloat level = largest * (x.Divide(largest, math).Pow(exponent, math) + y.Divide(largest, math).Pow(exponent, math)).Pow(EFloat.One.Divide(exponent, math), math);
        EFloat slope = exponent.CompareTo(EFloat.One) is 0
            ? Length((EFloat.One.Divide(extent.X, math), EFloat.One.Divide(extent.Y, math)), math)
            : Length((x.Divide(level, math).Pow(exponent - 1, math).Divide(extent.X, math), y.Divide(level, math).Pow(exponent - 1, math).Divide(extent.Y, math)), math);
        return ((length * level) - 1).Divide(slope, math);
    }
}

public sealed record GridLattice(ERational Height, ERational Span, ERational Shift, double Line, double Band) {
    public float Lines((double X, double Y) point) => ShapeEdge.Strip(Locate(point).Side, Line, Band);

    public Vector3 Cells((double X, double Y) point, Func<EInteger, EInteger, Vector3> color, Vector3 ink) =>
        Locate(point) switch {
            var (i, j, side, acrossI, acrossJ) => Vector3.Lerp(Vector3.Lerp(color(i, j), color(acrossI, acrossJ), ShapeEdge.Coverage(side, Band)), ink, ShapeEdge.Strip(side, Line, Band)),
        };

    private (EInteger I, EInteger J, double Side, EInteger AcrossI, EInteger AcrossJ) Locate((double X, double Y) point) {
        ERational across = ERational.FromDouble(point.X);
        (EInteger j, ERational vertical) = LatticePoint.Split(ERational.FromDouble(point.Y).Divide(Height));
        (EInteger i, ERational horizontal) = LatticePoint.Split(across.Divide(Span).Add(Offset(j)));
        (double x, double y, double right, double bottom) = (horizontal.Multiply(Span).ToDouble(), vertical.Multiply(Height).ToDouble(),
            ERational.One.Subtract(horizontal).Multiply(Span).ToDouble(), ERational.One.Subtract(vertical).Multiply(Height).ToDouble());
        double side = double.Min(double.Min(x, right), double.Min(y, bottom));
        return side == x ? (i, j, side, i.Subtract(1), j)
            : side == right ? (i, j, side, i.Add(1), j)
            : side == y ? (i, j, side, Column(across, j.Subtract(1)), j.Subtract(1))
            : (i, j, side, Column(across, j.Add(1)), j.Add(1));
    }

    private EInteger Column(ERational x, EInteger j) => LatticePoint.Split(x.Divide(Span).Add(Offset(j))).Cell;

    private ERational Offset(EInteger j) => j.IsEven ? Shift : ERational.Zero;
}

public readonly record struct Extinction(float Start, float Attenuation, Option<float> Cutoff) {
    public Func<long, long, float> Field(PixelFrame depth, Camera camera, Func<long, long, float, float> density) {
        (float start, float attenuation, Vector3 axis) = (Start, Attenuation, Vector3.Transform(-Vector3.UnitZ, camera.Orientation));
        Func<float, bool> beyond = Cutoff.Match(Some: static cutoff => (Func<float, bool>)(distance => distance > cutoff), None: static () => static _ => false);
        return (column, line) => {
            (double X, double Y) point = (column + 0.5d, camera.Extent.Height - line - 0.5d);
            float distance = depth.Sample(point, (WrapMode.Black, WrapMode.Black), float.PositiveInfinity, static (_, sample, _) => sample.X) / Vector3.Dot(camera.Ray(point).Direction, axis);
            float path = float.MaxNumber(distance - start, 0f) / attenuation;
            return beyond(distance) ? 0f : float.IsPositiveInfinity(path) ? 1f : 1f - float.Exp(-path * density(column, line, path));
        };
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class ShapeEdge {
    public static double Band(ShortSideLength feather, PixelExtent extent, double scale = 1d) => double.Max(feather.Pixels(extent) * scale, 1d);

    public static float Coverage(double distance, double band) => float.Clamp((float)(0.5d - (distance / band)), 0f, 1f);

    public static float Strip(double distance, double width, double band) =>
        (double.Min(width, band), double.Max(width, band)) switch {
            var (small, large) => (float)(double.Max(0d, double.Min(small, (small / 2d) - (double.Abs(distance) - (large / 2d)))) / band),
        };

    public static T Smooth<T>(T t) where T : IFloatingPoint<T> => t * t * (T.CreateTruncating(3) - (T.CreateTruncating(2) * t));

    public static T Smoother<T>(T t) where T : IFloatingPoint<T> => t * t * t * ((t * ((T.CreateTruncating(6) * t) - T.CreateTruncating(15))) + T.CreateTruncating(10));
}
