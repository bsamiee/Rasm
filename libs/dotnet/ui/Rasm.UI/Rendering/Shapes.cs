using System.Numerics;
using SkiaSharp;

namespace Rasm.UI.Rendering;

// --- [MODELS] --------------------------------------------------------------------------
internal sealed record Outline(Option<SKRoundRect> Round, SKPath Path) : IDisposable {
    private static readonly Seq<Vector2> Headings = [Vector2.UnitX, Vector2.UnitY, -Vector2.UnitX, -Vector2.UnitY];

    internal static Outline Of(Corners corners, float smoothing, SKRect frame, float offset) =>
        smoothing <= 0f
            ? Rounded(corners, frame, offset)
            : new(None, Traced(path => Continuous(path, corners.Grown(offset), SKRect.Inflate(frame, offset, offset), smoothing)));

    internal static SKPath Traced(Action<SKPathBuilder> add) {
        using SKPathBuilder builder = new();
        add(builder);
        return builder.Detach();
    }

    internal void Draw(SKCanvas canvas, SKPaint paint) => _ = Round.Match(round => canvas.DrawRoundRect(round, paint), () => canvas.DrawPath(Path, paint));

    internal void Clip(SKCanvas canvas, SKClipOperation operation) =>
        _ = Round.Match(round => canvas.ClipRoundRect(round, operation, antialias: true), () => canvas.ClipPath(Path, operation, antialias: true));

    internal bool Holds(SKPoint at) => Path.Contains(at.X, at.Y);

    public void Dispose() {
        _ = Round.Iter(static round => round.Dispose());
        Path.Dispose();
    }

    private static Outline Rounded(Corners corners, SKRect frame, float offset) {
        SKRoundRect round = new();
        round.SetRectRadii(frame, [new(corners.TopLeft, corners.TopLeft), new(corners.TopRight, corners.TopRight), new(corners.BottomRight, corners.BottomRight), new(corners.BottomLeft, corners.BottomLeft)]);
        round.Inflate(offset, offset);
        return new(round, Traced(path => path.AddRoundRect(round, SKPathDirection.Clockwise)));
    }

    private static void Continuous(SKPathBuilder path, Corners corners, SKRect frame, float smoothing) {
        float continuous = float.Min(smoothing, 1f);
        float[] radii = [corners.TopRight, corners.BottomRight, corners.BottomLeft, corners.TopLeft];
        float[] budgets = [-1f, -1f, -1f, -1f];
        SKPoint[] vertices = [new(frame.Right, frame.Top), new(frame.Right, frame.Bottom), new(frame.Left, frame.Bottom), new(frame.Left, frame.Top)];
        foreach (int corner in Enumerable.Range(0, 4).OrderByDescending(corner => radii[corner])) {
            budgets[corner] = float.Min(Share(corner, (corner + 1) % 4, Side(corner)), Share(corner, (corner + 3) % 4, Side((corner + 3) % 4)));
            radii[corner] = float.Min(radii[corner], budgets[corner]);
        }
        Seq<Bend> shapes = toSeq(Enumerable.Range(0, 4)).Map(corner => Smoothed(radii[corner], budgets[corner], continuous));
        path.MoveTo(vertices[0] - (SKPoint)(Headings[0] * shapes[0].P));
        _ = toSeq(Enumerable.Range(0, 4)).Iter(corner => {
            (float p, float a, float b, float c, float d, float arc) = shapes[corner];
            if (radii[corner] > 0f) {
                SKPoint origin = vertices[corner] - (SKPoint)(Headings[corner] * p);
                (Vector2 along, Vector2 turned) = (Headings[corner], Headings[(corner + 1) % 4]);
                SKPoint At(float u, float v) => origin + (SKPoint)((along * u) + (turned * v));
                path.CubicTo(At(a, 0f), At(a + b, 0f), At(a + b + c, d));
                path.ArcTo(new SKPoint(radii[corner], radii[corner]), 0f, SKPathArcSize.Small, SKPathDirection.Clockwise, At(a + b + c + arc, d + arc));
                path.CubicTo(At(a + b + c + arc + d, d + arc + c), At(a + b + c + arc + d, d + arc + b + c), At(p, p));
            }
            if (corner < 3) path.LineTo(vertices[corner + 1] - (SKPoint)(Headings[corner + 1] * shapes[corner + 1].P));
        });
        path.Close();

        float Side(int corner) => corner % 2 is 0 ? frame.Height : frame.Width;

        float Share(int corner, int adjacent, float side) =>
            radii[corner] <= 0f && radii[adjacent] <= 0f ? 0f
            : budgets[adjacent] >= 0f ? side - budgets[adjacent]
            : radii[corner] / (radii[corner] + radii[adjacent]) * side;
    }

    private sealed record Bend(float P, float A, float B, float C, float D, float Arc);

    private static Bend Smoothed(float radius, float budget, float smoothing) {
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
