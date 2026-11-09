using System.Numerics;
using Rasm.UI.Layout;
using Rasm.UI.Media;
using Rasm.UI.Styling;
using SkiaSharp;

namespace Rasm.UI.Rendering;

// --- [MODELS] --------------------------------------------------------------------------

internal sealed record OutlineKey(Shape Shape, Corners Corners, float Smoothing, SKRect Frame, float Offset, float Inset, float Turn, SKFontStyleWeight Weight);

// --- [SERVICES] ------------------------------------------------------------------------

internal abstract class Outline : IDisposable {

    // --- [CONSTRUCTION]

    internal static Fin<Outline> Of(Slot<OutlineKey, Outline> outlines, LayoutNode layout, SKRect frame, float offset) {
        OutlineKey key = new(layout.Style.Shape, layout.Corners, layout.Smoothing, frame, offset, layout.Style.Metrics[Metric.Inset], layout.Style.Metrics[Metric.Turn],
            layout.Cascade.Theme.Face.Combine(layout.Style.Face).Weight.IfNone(SKFontStyleWeight.Normal));
        return outlines.GetOrAdd(key, () => key.Shape is Shape.Rounded or Shape.Capsule && key.Smoothing <= 0f && key.Turn is 0f
            ? new Rounded(Radii(key.Corners, key.Frame, key.Offset))
            : Lowered(key).Map(path => (Outline)new Traced(Turned(path, key))));
    }

    internal static Traced Of(Action<SKPathBuilder> trace) => new(Built(trace));

    // --- [DRAWING]

    internal abstract void Draw(SKCanvas canvas, SKPaint paint);

    internal abstract void Clip(SKCanvas canvas, SKClipOperation operation);

    internal abstract void Inset(SKCanvas canvas, SKRect reach, SKPaint paint);

    internal abstract bool Contains(SKPoint at);

    public abstract void Dispose();

    // --- [CASES]

    private Outline() { }

    private sealed class Rounded(SKRoundRect shape) : Outline {
        private readonly Lazy<SKPath> path = new(() => Built(builder => builder.AddRoundRect(shape)));

        internal override void Draw(SKCanvas canvas, SKPaint paint) => canvas.DrawRoundRect(shape, paint);

        internal override void Clip(SKCanvas canvas, SKClipOperation operation) => canvas.ClipRoundRect(shape, operation, antialias: true);

        internal override void Inset(SKCanvas canvas, SKRect reach, SKPaint paint) {
            using SKRoundRect far = new(reach);
            canvas.DrawRoundRectDifference(far, shape, paint);
        }

        internal override bool Contains(SKPoint at) => path.Value.Contains(at.X, at.Y);

        public override void Dispose() {
            if (path.IsValueCreated) path.Value.Dispose();
            shape.Dispose();
        }
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

    private static Fin<SKPath> Lowered(OutlineKey key) =>
        key.Shape.Switch(
            key,
            rounded: static (held, _) => held.Smoothing <= 0f
                ? Built(path => {
                    using SKRoundRect radii = Radii(held.Corners, held.Frame, held.Offset);
                    path.AddRoundRect(radii, SKPathDirection.Clockwise);
                })
                : Built(path => Continuous(path, held.Corners.Grown(held.Offset), SKRect.Inflate(held.Frame, held.Offset, held.Offset), held.Smoothing)),
            capsule: static (held, _) => Lowered(held with { Shape = new Shape.Rounded() }),
            outlined: static (held, outlined) => {
                SKPath parsed = outlined.Path;
                SKMatrix placed = ImageFit.Mapping(parsed.Bounds, held.Frame);
                return Offset(Built(path => path.AddPath(parsed, in placed)), held.Offset);
            },
            icon: static (held, icon) => {
                SKPath grid = icon.Outline.Path;
                SKMatrix placed = ImageFit.Mapping(icon.Set.Box, ImageFit.Contain.Fitted(SKRect.Inflate(held.Frame, -held.Inset, -held.Inset), icon.Set.Box.Size));
                float weighted = icon.Set.Stem * placed.ScaleX * (((float)held.Weight / (float)SKFontStyleWeight.Normal) - 1f) / 2f;
                return Offset(Built(path => path.AddPath(grid, in placed)), held.Offset + weighted);
            },
            placed: static (held, placed) =>
                Lowered(held with {
                    Shape = placed.Inner,
                    Frame = SKRect.Create(held.Frame.Left + (placed.Unit.Left * held.Frame.Width), held.Frame.Top + (placed.Unit.Top * held.Frame.Height), placed.Unit.Width * held.Frame.Width, placed.Unit.Height * held.Frame.Height),
                }),
            combined: static (held, combined) => {
                Fin<SKPath> first = Lowered(held with { Shape = combined.First, Offset = 0f });
                Fin<SKPath> second = Lowered(held with { Shape = combined.Second, Offset = 0f });
                using SKPath? one = first.ToOption().ValueUnsafe();
                using SKPath? other = second.ToOption().ValueUnsafe();
                return (first, second).Apply((left, right) => Joined(left, right, combined.Op)).As().Bind(identity).Bind(joined => Offset(joined, held.Offset));
            });

    private static SKPath Turned(SKPath path, OutlineKey key) {
        if (key.Turn is 0f) return path;
        using SKPath held = path;
        SKMatrix turn = SKMatrix.CreateRotationDegrees(key.Turn, key.Frame.MidX, key.Frame.MidY);
        return Built(builder => builder.AddPath(held, in turn));
    }

    private static Fin<SKPath> Offset(SKPath path, float by) {
        if (by is 0f) return path;
        using SKPath owned = path;
        using SKPaint rim = new() { Style = SKPaintStyle.Stroke, StrokeWidth = 2f * MathF.Abs(by), StrokeJoin = SKStrokeJoin.Round };
        using SKPath edge = rim.GetFillPath(owned);
        return Joined(owned, edge, by > 0f ? SKPathOp.Union : SKPathOp.Difference);
    }

    private static Fin<SKPath> Joined(SKPath first, SKPath second, SKPathOp op) =>
        Optional(first.Op(second, op)).ToFin(new UIFault((op, first.Bounds, second.Bounds), UIReason.Refused));

    private static SKPath Built(Action<SKPathBuilder> trace) {
        using SKPathBuilder builder = new();
        trace(builder);
        return builder.Detach();
    }

    private static SKRoundRect Radii(Corners corners, SKRect frame, float offset) {
        SKRoundRect radii = new();
        radii.SetRectRadii(frame, [.. corners.Clockwise.Select(static radius => new SKPoint(radius, radius))]);
        radii.Inflate(offset, offset);
        return radii;
    }

    // --- [CONTINUOUS]

    private static readonly Seq<Vector2> Directions = [-Vector2.UnitY, Vector2.UnitX, Vector2.UnitY, -Vector2.UnitX];

    private static void Continuous(SKPathBuilder path, Corners corners, SKRect frame, float smoothing) {
        Seq<float> radii = corners.Fitted(frame.Size).Clockwise;
        Seq<SKPoint> vertices = [new(frame.Left, frame.Top), new(frame.Right, frame.Top), new(frame.Right, frame.Bottom), new(frame.Left, frame.Bottom)];
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

    private static float Side(SKRect frame, int corner) => corner % 2 is 0 ? frame.Width : frame.Height;

    private static float Share(Seq<float> radii, int corner, int adjacent, float side) => radii[corner] <= 0f ? 0f : side * radii[corner] / (radii[corner] + radii[adjacent]);
}
