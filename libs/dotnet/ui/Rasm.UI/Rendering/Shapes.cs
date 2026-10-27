using System.Numerics;
using Rasm.UI.Styling;
using SkiaSharp;

namespace Rasm.UI.Rendering;

// --- [SERVICES] ------------------------------------------------------------------------
internal sealed class Outline : IDisposable {
    private static readonly Seq<Vector2> Directions = [Vector2.UnitX, Vector2.UnitY, -Vector2.UnitX, -Vector2.UnitY];

    private readonly Option<SKRoundRect> round;
    private readonly Lazy<SKRegion> mask;
    private readonly Lazy<SKPath> inverted;

    private Outline(Option<SKRoundRect> round, SKPath path) {
        (this.round, Path) = (round, path);
        mask = new(() => new SKRegion(path));
        inverted = new(() => {
            using SKPathBuilder builder = new() { FillType = SKPathFillType.InverseWinding };
            builder.AddPath(path);
            return builder.Detach();
        });
    }

    internal SKPath Path { get; }

    // --- [CONSTRUCTION]
    internal static Fin<Outline> Of(Shape shape, Corners corners, float smoothing, SKRect frame, float offset) =>
        shape is Shape.Rounded && smoothing <= 0f ? Rounded(corners, frame, offset) : Lowered(shape, corners, smoothing, frame, offset).Map(static path => new Outline(None, path));

    internal static Outline Of(Action<SKPathBuilder> trace) => new(None, Traced(trace));

    // --- [DRAWING]
    internal void Draw(SKCanvas canvas, SKPaint paint) {
        if (round.Case is SKRoundRect held) canvas.DrawRoundRect(held, paint);
        else canvas.DrawPath(Path, paint);
    }

    internal void Clip(SKCanvas canvas, SKClipOperation operation) {
        if (round.Case is SKRoundRect held) canvas.ClipRoundRect(held, operation, antialias: true);
        else canvas.ClipPath(Path, operation, antialias: true);
    }

    internal void Inset(SKCanvas canvas, SKRect reach, SKPaint paint) {
        if (round.Case is not SKRoundRect held) {
            canvas.DrawPath(inverted.Value, paint);
            return;
        }
        using SKRoundRect far = new(reach);
        canvas.DrawRoundRectDifference(far, held, paint);
    }

    internal bool Contains(SKPoint at) =>
        mask.Value.Contains((int)MathF.Floor(at.X), (int)MathF.Floor(at.Y));

    public void Dispose() {
        _ = round.Iter(static held => held.Dispose());
        if (mask.IsValueCreated) mask.Value.Dispose();
        if (inverted.IsValueCreated) inverted.Value.Dispose();
        Path.Dispose();
    }

    // --- [LOWERING]
    private static Fin<SKPath> Lowered(Shape shape, Corners corners, float smoothing, SKRect frame, float offset) =>
        shape.Switch(
            (Corners: corners, Smoothing: smoothing, Frame: frame, Offset: offset),
            rounded: static (held, _) => held.Smoothing <= 0f
                ? Traced(path => path.AddRoundRect(Radii(held.Corners, held.Frame, held.Offset), SKPathDirection.Clockwise))
                : Traced(path => Continuous(path, held.Corners.Grown(held.Offset), SKRect.Inflate(held.Frame, held.Offset, held.Offset), held.Smoothing)),
            outlined: static (held, outlined) => {
                using SKPath parsed = SKPath.ParseSvgPathData(outlined.PathData);
                return Mapped(parsed, parsed.Bounds, SKRect.Inflate(held.Frame, held.Offset, held.Offset));
            },
            placed: static (held, placed) => {
                SKRect area = SKRect.Create(held.Frame.Left + (placed.Unit.Left * held.Frame.Width), held.Frame.Top + (placed.Unit.Top * held.Frame.Height),
                    placed.Unit.Width * held.Frame.Width, placed.Unit.Height * held.Frame.Height);
                return Lowered(placed.Inner, held.Corners.Fitted(area.Size), held.Smoothing, area, held.Offset);
            },
            combined: static (held, combined) =>
                from operands in (Lowered(combined.First, held.Corners, held.Smoothing, held.Frame, 0f), Lowered(combined.Second, held.Corners, held.Smoothing, held.Frame, 0f))
                    .Apply(static (first, second) => (First: first, Second: second)).As()
                from joined in Joined(operands.First, operands.Second, combined.Op)
                select Inflated(joined, held.Frame, held.Offset));

    private static Fin<SKPath> Joined(SKPath first, SKPath second, SKPathOp op) {
        using SKPath one = first;
        using SKPath other = second;
        return Optional(one.Op(other, op)).ToFin(new UIFault($"{op}: {one.Bounds} {other.Bounds}", UIReason.Refused));
    }

    private static SKPath Inflated(SKPath joined, SKRect frame, float offset) {
        using SKPath owned = joined;
        return Mapped(owned, frame, SKRect.Inflate(frame, offset, offset));
    }

    private static SKPath Mapped(SKPath path, SKRect from, SKRect to) {
        (float x, float y) = (to.Width / from.Width, to.Height / from.Height);
        SKMatrix placed = SKMatrix.CreateScaleTranslation(x, y, to.Left - (from.Left * x), to.Top - (from.Top * y));
        return Traced(builder => builder.AddPath(path, in placed));
    }

    private static SKPath Traced(Action<SKPathBuilder> trace) {
        using SKPathBuilder builder = new();
        trace(builder);
        return builder.Detach();
    }

    private static Outline Rounded(Corners corners, SKRect frame, float offset) {
        SKRoundRect rounded = Radii(corners, frame, offset);
        return new(rounded, Traced(path => path.AddRoundRect(rounded, SKPathDirection.Clockwise)));
    }

    private static SKRoundRect Radii(Corners corners, SKRect frame, float offset) {
        SKRoundRect radii = new();
        radii.SetRectRadii(frame, [new(corners.TopLeft, corners.TopLeft), new(corners.TopRight, corners.TopRight), new(corners.BottomRight, corners.BottomRight), new(corners.BottomLeft, corners.BottomLeft)]);
        radii.Inflate(offset, offset);
        return radii;
    }

    // --- [CONTINUOUS]
    private sealed record CornerCurve(float P, float A, float B, float C, float D, float Arc) {
        internal static CornerCurve Smoothed(float radius, float budget, float smoothing) {
            if (radius <= 0f) return new(0f, 0f, 0f, 0f, 0f, 0f);
            float kept = float.Min(smoothing, (budget / radius) - 1f);
            float extent = float.Min((1f + kept) * radius, budget);
            float sweep = MathF.PI / 2f * (1f - kept);
            float arc = MathF.Sin(sweep / 2f) * radius * MathF.Sqrt(2f);
            float bend = MathF.PI / 4f * kept;
            float c = radius * MathF.Tan(((MathF.PI / 2f) - sweep) / 4f) * MathF.Cos(bend);
            float d = c * MathF.Tan(bend);
            float b = (extent - arc - c - d) / 3f;
            return new(extent, 2f * b, b, c, d, arc);
        }
    }

    private sealed record CornerFit(HashMap<int, float> Budgets, Seq<float> Radii);

    private static void Continuous(SKPathBuilder path, Corners corners, SKRect frame, float smoothing) {
        Seq<float> stated = [corners.TopRight, corners.BottomRight, corners.BottomLeft, corners.TopLeft];
        Seq<SKPoint> vertices = [new(frame.Right, frame.Top), new(frame.Right, frame.Bottom), new(frame.Left, frame.Bottom), new(frame.Left, frame.Top)];
        CornerFit fit = toSeq(Enumerable.Range(0, 4).OrderByDescending(corner => stated[corner])).Fold(new CornerFit([], stated), (held, corner) => {
            float budget = float.Min(Share(held, corner, (corner + 1) % 4, Side(frame, corner)), Share(held, corner, (corner + 3) % 4, Side(frame, (corner + 3) % 4)));
            return new CornerFit(held.Budgets.Add(corner, budget), held.Radii.Map((radius, index) => index == corner ? float.Min(radius, budget) : radius));
        });
        Seq<CornerCurve> shapes = fit.Radii.Map((radius, corner) => CornerCurve.Smoothed(radius, fit.Budgets[corner], float.Min(smoothing, 1f)));
        path.MoveTo(vertices[0] - (SKPoint)(Directions[0] * shapes[0].P));
        _ = shapes.Iter((corner, shape) => {
            float radius = fit.Radii[corner];
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

    private static float Side(SKRect frame, int corner) => corner % 2 is 0 ? frame.Height : frame.Width;

    private static float Share(CornerFit fit, int corner, int adjacent, float side) =>
        fit.Radii[corner] <= 0f && fit.Radii[adjacent] <= 0f ? 0f
        : fit.Budgets.Find(adjacent).Match(Some: taken => side - taken, None: () => fit.Radii[corner] / (fit.Radii[corner] + fit.Radii[adjacent]) * side);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class Refraction {
    private static readonly SKRuntimeShaderBuilder Matte = SKRuntimeEffect.BuildShader("""
        uniform float2 size;
        uniform float4 corners;
        uniform float smoothing;
        half4 main(float2 p) {
            float2 c = p - (size * 0.5);
            float r = c.x > 0.0 ? (c.y > 0.0 ? corners.z : corners.y) : (c.y > 0.0 ? corners.w : corners.x);
            float2 q = abs(c) - (size * 0.5) + r;
            float2 m = max(q, 0.0);
            float k = 2.0 + (2.0 * smoothing);
            float d = pow(pow(m.x, k) + pow(m.y, k), 1.0 / k) + min(max(q.x, q.y), 0.0) - r;
            float2 g = (q.x > 0.0 && q.y > 0.0) ? pow(m, float2(k - 1.0)) : (q.x > q.y ? float2(1.0, 0.0) : float2(0.0, 1.0));
            float rim = max(max(max(corners.x, corners.y), max(corners.z, corners.w)), 1.0);
            float2 shift = normalize(g) * sign(c) * (1.0 - smoothstep(0.0, rim, -d));
            return half4(half2(0.5 - (0.5 * shift)), 0.5, 1.0);
        }
        """);

    internal static SKShader Shader(SKSize size, Corners corners, float smoothing) {
        using SKRuntimeEffectUniforms uniforms = new(Matte.Effect) {
            ["size"] = size,
            ["corners"] = new float[] { corners.TopLeft, corners.TopRight, corners.BottomRight, corners.BottomLeft },
            ["smoothing"] = smoothing,
        };
        using SKRuntimeEffectChildren children = new(Matte.Effect);
        return Matte.Effect.ToShader(uniforms, children);
    }
}
