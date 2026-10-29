using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using CommunityToolkit.HighPerformance;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Rasm.Imaging.Pixels;
using TinyEXR;

namespace Rasm.Imaging.Procedural;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record CellGrid(long Columns, long Rows) {
    public static CellGrid Fit(PixelExtent extent, double width, double height) {
        return new(Count(extent.Width, width), Count(extent.Height, height));

        static long Count(int pixels, double size) => long.Max(1L, (long)double.Round(pixels / size, MidpointRounding.ToEven));
    }

    public (long Cell, int Remainder) Column(long x, int width) => Coordinate(x, width, Columns);

    public (long Cell, int Remainder) Row(long y, int height) => Coordinate(y, height, Rows);

    public Func<(long Column, long Row), Vector3> Means(PixelFrame frame) {
        if (frame.Whole && 4 * (Int128)Columns * Rows <= System.Array.MaxLength) {
            (int columns, int rows) = ((int)Columns, (int)Rows);
            float[] means = GC.AllocateUninitializedArray<float>(4 * columns * rows);
            ImageProcessing.Resize(frame.Block, frame.Size.Width, frame.Size.Height, means, columns, rows, 4, ResizeFilter.Box, alphaChannel: 3);
            Memory2D<Vector4> cells = means.AsMemory().Cast<float, Vector4>().AsMemory2D(rows, columns);
            return cell => cells.Span[(int)cell.Row, (int)cell.Column].AsVector3();
        }
        PixelFrame snapshot = frame.Copy();
        return memoUnsafe(((long Column, long Row) cell) => Mean(snapshot, cell));
    }

    internal static (Int128 Quotient, Int128 Remainder) Floored(Int128 numerator, Int128 denominator) =>
        Int128.DivRem(numerator, denominator) switch {
            var (quotient, remainder) when remainder < 0 => (quotient - 1, remainder + denominator),
            var split => split,
        };

    private static (long Cell, int Remainder) Coordinate(long pixel, int extent, long cells) => ((long, int))Floored((Int128)pixel * cells, extent);

    private Vector3 Mean(PixelFrame frame, (long Column, long Row) cell) {
        ReadOnlySpan2D<Vector4> plane = frame.View.Span;
        ((double left, double right), (double top, double bottom)) =
            (Bounds(cell.Column, frame.Extent.Width, frame.Origin.X, frame.Size.Width, Columns), Bounds(cell.Row, frame.Extent.Height, frame.Origin.Y, frame.Size.Height, Rows));
        Vector4 sum = Vector4.Zero;
        for (int y = (int)double.Floor(top); y < (int)double.Ceiling(bottom); y++) {
            for (int x = (int)double.Floor(left); x < (int)double.Ceiling(right); x++) {
                Vector4 pixel = plane[y, x];
                sum += new Vector4(pixel.AsVector3() * pixel.W, pixel.W) * (float)((double.Min(y + 1d, bottom) - double.Max(y, top)) * (double.Min(x + 1d, right) - double.Max(x, left)));
            }
        }
        return sum.W > 0f ? sum.AsVector3() / sum.W : Vector3.Zero;

        static (double Low, double High) Bounds(long index, int extent, int origin, int size, long cells) =>
            (((Int128)index * extent) - ((Int128)cells * origin)) switch { var start => (double.Max(0d, (double)start / cells), double.Min(size, (double)(start + extent) / cells)) };
    }
}

[SmartEnum<string>]
public sealed partial class SpotFunction {
    private const float Elongation = float.Pi / 1.6f;

    public static readonly SpotFunction Round = new("round", static spot => Quarter(spot.Length(), 1f, 1f));
    public static readonly SpotFunction Ellipse = new("ellipse", static spot => Elongation * Quarter(float.Hypot(spot.X, spot.Y / Elongation), 1f, 1f / Elongation));
    public static readonly SpotFunction Diamond = new("diamond", static spot => (float.Abs(spot.X) + float.Abs(spot.Y)) switch { <= 1f and var d => d * d / 2f, var d => 1f - ((2f - d) * (2f - d) / 2f) });
    public static readonly SpotFunction Square = new("square", static spot => float.Max(float.Abs(spot.X), float.Abs(spot.Y)) switch { var m => m * m });
    public static readonly SpotFunction Line = new("line", static spot => float.Abs(spot.Y));
    public static readonly SpotFunction WavyLine = new("wavy-line", static spot => (spot.Y - (0.5f * float.SinPi(spot.X))) switch { var w => float.Abs(w - (2f * float.Floor((w + 1f) / 2f))) });
    public static readonly SpotFunction Cross = new("cross", static spot => (1f - float.Min(float.Abs(spot.X), float.Abs(spot.Y))) switch { var m => 1f - (m * m) });

    [UseDelegateFromConstructor]
    private partial float Spot(Vector2 spot);

    public float Coverage(Vector2 point, float level, Matrix3x2 screen, float onset) {
        Vector2 at = Vector2.Transform(point, screen);
        (Vector2 across, Vector2 down) = (new(screen.M11, screen.M12), new(screen.M21, screen.M22));
        (float t, float k) = (Threshold(at), float.Clamp((level - onset) / (1f - onset), 0f, 1f));
        float w = float.Abs(Threshold(at + across) - t) + float.Abs(Threshold(at + down) - t);
        return w > 0f ? float.Clamp((((k * (1f + w)) - (w / 2f) - t) / w) + 0.5f, 0f, 1f) : t < k ? 1f : 0f;
    }

    private float Threshold(Vector2 at) => Spot((2f * (at - Vector128.Floor(at.AsVector128()).AsVector2())) - Vector2.One);

    private static float Quarter(float rho, float width, float height) =>
        float.Min(rho, width) switch {
            var reach => float.Min(float.Sqrt(float.Max((rho * rho) - (height * height), 0f)), reach) switch { var knee => (height * knee) + Arc(rho, reach) - Arc(rho, knee) },
        };

    private static float Arc(float rho, float x) => float.Sqrt((rho * rho) - (x * x)) switch { var y => ((x * y) + (rho * rho * float.Atan2(x, y))) / 2f };
}

public sealed class GlyphAtlas {
    private readonly PixelExtent extent;
    private readonly ReadOnlyMemory2D<byte> planes;
    private readonly ImmutableArray<int> ranked;

    private GlyphAtlas(CellGrid grid, PixelExtent extent, ReadOnlyMemory2D<byte> planes, ImmutableArray<int> ranked) =>
        (Grid, this.extent, this.planes, this.ranked) = (grid, extent, planes, ranked);

    public CellGrid Grid { get; }

    public static Fin<GlyphAtlas> Rendered(string glyphs, HersheyFonts font, double cell, PixelExtent extent) =>
        ImagingFault.Unless(glyphs.Length > 0 && !glyphs.AsSpan().ContainsAnyExceptInRange(' ', '~') && toHashSet(glyphs).Count == glyphs.Length, ImagingReason.Range, glyphs)
            .Map(_ => Drawn(glyphs, font, cell, extent));

    public int Glyph(float level) => ranked[int.Clamp((int)(level * ranked.Length), 0, ranked.Length - 1)];

    public float Coverage(int glyph, long x, long y) =>
        (planes.Height / ranked.Length) switch {
            var height => planes.Span[(glyph * height) + (int)((long)Grid.Row(y, extent.Height).Remainder * height / extent.Height),
                (int)((long)Grid.Column(x, extent.Width).Remainder * planes.Width / extent.Width)] / (float)byte.MaxValue,
        };

    private static GlyphAtlas Drawn(string glyphs, HersheyFonts font, double cell, PixelExtent extent) {
        (Size Size, int Baseline)[] measures = [.. glyphs.Select(glyph => {
            int baseline = 0;
            return (CvInvoke.GetTextSize(glyph.ToString(), font, 1d, 1, ref baseline), baseline);
        })];
        (int widest, int tallest) = (measures.Max(static measure => measure.Size.Width), measures.Max(static measure => measure.Size.Height + measure.Baseline));
        CellGrid grid = CellGrid.Fit(extent, cell * widest / tallest, cell);
        Size tile = new((int)((extent.Width + grid.Columns - 1L) / grid.Columns), (int)((extent.Height + grid.Rows - 1L) / grid.Rows));
        double scale = double.Min((double)tile.Height / tallest, (double)tile.Width / widest);
        byte[] texels = GC.AllocateArray<byte>(glyphs.Length * tile.Width * tile.Height, pinned: true);
        double[] means = [.. Enumerable.Range(0, glyphs.Length).Select(i => {
            using Mat plane = new(tile.Height, tile.Width, DepthType.Cv8U, 1, Marshal.UnsafeAddrOfPinnedArrayElement(texels, i * tile.Width * tile.Height), tile.Width);
            CvInvoke.PutText(
                plane, glyphs[i].ToString(), new Point((int)double.Round((tile.Width - (measures[i].Size.Width * scale)) / 2d, MidpointRounding.ToEven), tile.Height - (int)double.Ceiling(measures[i].Baseline * scale)),
                font, scale, new MCvScalar(byte.MaxValue), int.Max(1, (int)double.Round(scale, MidpointRounding.ToEven)), LineType.AntiAlias);
            return CvInvoke.Mean(plane).V0;
        })];
        return new(grid, extent, new ReadOnlyMemory2D<byte>(texels, glyphs.Length * tile.Height, tile.Width), [.. Enumerable.Range(0, glyphs.Length).OrderBy(i => means[i])]);
    }
}
