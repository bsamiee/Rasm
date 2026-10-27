using System.Numerics;
using Rasm.UI.Styling;
using SkiaSharp;

namespace Rasm.UI.Rendering;

// --- [MODELS] --------------------------------------------------------------------------
internal sealed record OutlineKey(Shape Shape, Corners Corners, float Smoothing, SKRect Frame, float Offset);

// --- [SERVICES] ------------------------------------------------------------------------
internal abstract class Outline : IDisposable {
    private static readonly Seq<Vector2> Directions = [Vector2.UnitX, Vector2.UnitY, -Vector2.UnitX, -Vector2.UnitY];

    private Outline() { }

    // --- [CONSTRUCTION]
    internal static Fin<Outline> Of(Slot<OutlineKey, Outline> outlines, LayoutNode layout, SKRect frame, float offset) =>
        new OutlineKey(layout.Style.Shape, layout.Corners, layout.Smoothing, frame, offset) switch {
            var key => outlines.GetOrAdd(key, () => key.Shape is Shape.Rounded && key.Smoothing <= 0f
                ? new Rounded(Radii(key.Corners, key.Frame, key.Offset))
                : Lowered(key.Shape, key.Corners, key.Smoothing, key.Frame, key.Offset).Map(static path => (Outline)new Traced(path))),
        };

    internal static Traced Of(Action<SKPathBuilder> trace) => new(Built(trace));

    // --- [DRAWING]
    internal abstract void Draw(SKCanvas canvas, SKPaint paint);

    internal abstract void Clip(SKCanvas canvas, SKClipOperation operation);

    internal abstract void Inset(SKCanvas canvas, SKRect reach, SKPaint paint);

    internal abstract bool Contains(SKPoint at);

    public abstract void Dispose();

    // --- [CASES]
    private sealed class Rounded(SKRoundRect shape) : Outline {
        private static readonly Seq<Vector2> Signs = [-Vector2.One, new(1f, -1f), Vector2.One, new(-1f, 1f)];

        internal override void Draw(SKCanvas canvas, SKPaint paint) => canvas.DrawRoundRect(shape, paint);

        internal override void Clip(SKCanvas canvas, SKClipOperation operation) => canvas.ClipRoundRect(shape, operation, antialias: true);

        internal override void Inset(SKCanvas canvas, SKRect reach, SKPaint paint) {
            using SKRoundRect far = new(reach);
            canvas.DrawRoundRectDifference(far, shape, paint);
        }

        internal override bool Contains(SKPoint at) =>
            shape.Rect switch {
                var bounds => bounds.Contains(at) && toSeq(shape.Radii).Zip(Signs).ForAll(corner =>
                    ((((Vector2)at - new Vector2(bounds.MidX, bounds.MidY)) * corner.Second) - (new Vector2(bounds.Width, bounds.Height) / 2f) + (Vector2)corner.First) switch {
                        var beyond => beyond.X <= 0f || beyond.Y <= 0f || (beyond / (Vector2)corner.First).LengthSquared() <= 1f,
                    }),
            };

        public override void Dispose() => shape.Dispose();
    }

    internal sealed class Traced : Outline {
        private readonly Lazy<SKPath> inverted;

        internal Traced(SKPath path) {
            Path = path;
            inverted = new(() => Built(builder => {
                builder.FillType = SKPathFillType.InverseWinding;
                builder.AddPath(path);
            }));
        }

        internal SKPath Path { get; }

        internal override void Draw(SKCanvas canvas, SKPaint paint) => canvas.DrawPath(Path, paint);

        internal override void Clip(SKCanvas canvas, SKClipOperation operation) => canvas.ClipPath(Path, operation, antialias: true);

        internal override void Inset(SKCanvas canvas, SKRect reach, SKPaint paint) => canvas.DrawPath(inverted.Value, paint);

        internal override bool Contains(SKPoint at) => Path.Contains(at.X, at.Y);

        public override void Dispose() {
            if (inverted.IsValueCreated) inverted.Value.Dispose();
            Path.Dispose();
        }
    }

    // --- [LOWERING]
    private static Fin<SKPath> Lowered(Shape shape, Corners corners, float smoothing, SKRect frame, float offset) =>
        shape.Switch(
            (Corners: corners, Smoothing: smoothing, Frame: frame, Offset: offset),
            rounded: static (held, _) => held.Smoothing <= 0f
                ? Built(path => path.AddRoundRect(Radii(held.Corners, held.Frame, held.Offset), SKPathDirection.Clockwise))
                : Built(path => Continuous(path, held.Corners.Grown(held.Offset), SKRect.Inflate(held.Frame, held.Offset, held.Offset), held.Smoothing)),
            outlined: static (held, outlined) => {
                using SKPath parsed = SKPath.ParseSvgPathData(outlined.PathData);
                SKMatrix placed = ImageFit.Mapping(parsed.Bounds, held.Frame);
                return Offset(Built(path => path.AddPath(parsed, in placed)), held.Offset);
            },
            placed: static (held, placed) =>
                Lowered(placed.Inner, held.Corners, held.Smoothing, SKRect.Create(held.Frame.Left + (placed.Unit.Left * held.Frame.Width), held.Frame.Top + (placed.Unit.Top * held.Frame.Height),
                    placed.Unit.Width * held.Frame.Width, placed.Unit.Height * held.Frame.Height), held.Offset),
            combined: static (held, combined) =>
                from operands in (Lowered(combined.First, held.Corners, held.Smoothing, held.Frame, 0f), Lowered(combined.Second, held.Corners, held.Smoothing, held.Frame, 0f))
                    .Apply(static (first, second) => (First: first, Second: second)).As()
                from joined in Joined(operands.First, operands.Second, combined.Op)
                from offset in Offset(joined, held.Offset)
                select offset);

    private static Fin<SKPath> Offset(SKPath path, float by) {
        if (by is 0f) return path;
        using SKPaint rim = new() { Style = SKPaintStyle.Stroke, StrokeWidth = 2f * MathF.Abs(by), StrokeJoin = SKStrokeJoin.Round };
        return Joined(path, rim.GetFillPath(path), by > 0f ? SKPathOp.Union : SKPathOp.Difference);
    }

    private static Fin<SKPath> Joined(SKPath first, SKPath second, SKPathOp op) {
        using SKPath one = first;
        using SKPath other = second;
        return Optional(one.Op(other, op)).ToFin(new UIFault($"{op}: {one.Bounds} {other.Bounds}", UIReason.Refused));
    }

    private static SKPath Built(Action<SKPathBuilder> trace) {
        using SKPathBuilder builder = new();
        trace(builder);
        return builder.Detach();
    }

    private static SKRoundRect Radii(Corners corners, SKRect frame, float offset) {
        SKRoundRect radii = new();
        radii.SetRectRadii(frame, [new(corners.TopLeft, corners.TopLeft), new(corners.TopRight, corners.TopRight), new(corners.BottomRight, corners.BottomRight), new(corners.BottomLeft, corners.BottomLeft)]);
        radii.Inflate(offset, offset);
        return radii;
    }

    // --- [CONTINUOUS]
    private static void Continuous(SKPathBuilder path, Corners corners, SKRect frame, float smoothing) {
        Corners fitted = corners.Fitted(frame.Size);
        Seq<float> radii = [fitted.TopRight, fitted.BottomRight, fitted.BottomLeft, fitted.TopLeft];
        Seq<SKPoint> vertices = [new(frame.Right, frame.Top), new(frame.Right, frame.Bottom), new(frame.Left, frame.Bottom), new(frame.Left, frame.Top)];
        Seq<(float P, float A, float B, float C, float D, float Arc)> shapes = radii.Map((radius, corner) => Smoothed(radius,
            float.Min(Share(radii, corner, (corner + 1) % 4, Side(frame, corner)), Share(radii, corner, (corner + 3) % 4, Side(frame, (corner + 3) % 4))), float.Min(smoothing, 1f)));
        path.MoveTo(vertices[0] - (SKPoint)(Directions[0] * shapes[0].P));
        _ = shapes.Iter((corner, shape) => {
            float radius = radii[corner];
            if (radius > 0f) {
                SKPoint origin = vertices[corner] - (SKPoint)(Directions[corner] * shape.P);
                (Vector2 along, Vector2 turned) = (Directions[corner], Directions[(corner + 1) % 4]);
                SKPoint At(float u, float v) => origin + (SKPoint)((along * u) + (turned * v));
                path.CubicTo(At(shape.A, 0f), At(shape.A + shape.B, 0f), At(shape.A + shape.B + shape.C, shape.D));
                path.ArcTo(new SKPoint(radius, radius), 0f, SKPathArcSize.Small, SKPathDirection.Clockwise, At(shape.A + shape.B + shape.C + shape.Arc, shape.D + shape.Arc));
                path.CubicTo(At(shape.A + shape.B + shape.C + shape.Arc + shape.D, shape.D + shape.Arc + shape.C), At(shape.A + shape.B + shape.C + shape.Arc + shape.D, shape.D + shape.Arc + shape.B + shape.C), At(shape.P, shape.P));
            }
            if (corner < 3) path.LineTo(vertices[corner + 1] - (SKPoint)(Directions[corner + 1] * shapes[corner + 1].P));
        });
        path.Close();
    }

    private static (float P, float A, float B, float C, float D, float Arc) Smoothed(float radius, float budget, float smoothing) {
        if (radius <= 0f) return (0f, 0f, 0f, 0f, 0f, 0f);
        float kept = float.Min(smoothing, (budget / radius) - 1f);
        float extent = float.Min((1f + kept) * radius, budget);
        float sweep = MathF.PI / 2f * (1f - kept);
        float arc = MathF.Sin(sweep / 2f) * radius * MathF.Sqrt(2f);
        float bend = MathF.PI / 4f * kept;
        float c = radius * MathF.Tan(((MathF.PI / 2f) - sweep) / 4f) * MathF.Cos(bend);
        float d = c * MathF.Tan(bend);
        float b = (extent - arc - c - d) / 3f;
        return (extent, 2f * b, b, c, d, arc);
    }

    private static float Side(SKRect frame, int corner) => corner % 2 is 0 ? frame.Height : frame.Width;

    private static float Share(Seq<float> radii, int corner, int adjacent, float side) => radii[corner] <= 0f ? 0f : side * radii[corner] / (radii[corner] + radii[adjacent]);
}
