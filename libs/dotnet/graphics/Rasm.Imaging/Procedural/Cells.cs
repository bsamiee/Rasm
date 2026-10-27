using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
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

    public Func<(long Column, long Row), Vector3> Means(PixelFrame frame, Mat plane) {
        if (frame.Whole && 4 * (Int128)Columns * Rows <= System.Array.MaxLength) {
            (int columns, int rows) = ((int)Columns, (int)Rows);
            float[] means = GC.AllocateUninitializedArray<float>(4 * columns * rows);
            ImageProcessing.Resize(plane.GetSpan<float>(), plane.Cols, plane.Rows, means, columns, rows, 4, ResizeFilter.Box, alphaChannel: 3);
            Memory2D<Vector4> cells = means.AsMemory().Cast<float, Vector4>().AsMemory2D(rows, columns);
            return cell => cells.Span[(int)cell.Row, (int)cell.Column].AsVector3();
        }
        return cell => Mean(plane, frame, cell);
    }

    private static (long Cell, int Remainder) Coordinate(long pixel, int extent, long cells) => ((long, int))Floored((Int128)pixel * cells, extent);

    internal static (Int128 Quotient, Int128 Remainder) Floored(Int128 numerator, Int128 denominator) =>
        Int128.DivRem(numerator, denominator) switch {
            var (quotient, remainder) when remainder < 0 => (quotient - 1, remainder + denominator),
            var split => split,
        };

    private Vector3 Mean(Mat source, PixelFrame frame, (long Column, long Row) cell) {
        ReadOnlySpan2D<Vector4> plane = source.View<Vector4>();
        if (Columns > frame.Extent.Width || Rows > frame.Extent.Height) {
            Vector4 pixel = plane.Sample(
                (Interpolated(cell.Column, frame.Extent.Width, Columns), Interpolated(cell.Row, frame.Extent.Height, Rows)),
                (WrapMode.Black, WrapMode.Black), Vector4.Zero, static (sum, pixel, weight) => sum + (new Vector4(pixel.AsVector3() * pixel.W, pixel.W) * (float)weight), Some((frame.Origin, frame.Extent)));
            return pixel.Unassociated.AsVector3();
        }
        (double left, double right) = Bounds(cell.Column, frame.Extent.Width, frame.Origin.X, frame.Size.Width, Columns);
        (double top, double bottom) = Bounds(cell.Row, frame.Extent.Height, frame.Origin.Y, frame.Size.Height, Rows);
        Vector256<double> sum = Vector256<double>.Zero;
        for (int y = (int)double.Floor(top); y < (int)double.Ceiling(bottom); y++) {
            double down = double.Min(y + 1d, bottom) - double.Max(y, top);
            for (int x = (int)double.Floor(left); x < (int)double.Ceiling(right); x++) {
                Vector4 pixel = plane[y, x];
                sum += Vector256.Create(pixel.X * pixel.W, pixel.Y * pixel.W, pixel.Z * pixel.W, pixel.W) * (down * (double.Min(x + 1d, right) - double.Max(x, left)));
            }
        }
        return sum.GetElement(3) > 0d
            ? new Vector3((float)(sum.GetElement(0) / sum.GetElement(3)), (float)(sum.GetElement(1) / sum.GetElement(3)), (float)(sum.GetElement(2) / sum.GetElement(3)))
            : Vector3.Zero;

        static double Interpolated(long index, int extent, long cells) =>
            Floored((Int128)index * extent, cells) switch {
                var (pixel, remainder) => (double)pixel + 0.5d
                    + (index >= 0L && index < cells && pixel == extent - 1 ? 0d : double.Max(0d, (double)(extent - cells + remainder) / extent)),
            };

        static (double Left, double Right) Bounds(long index, int extent, int origin, int size, long cells) =>
            (((Int128)index * extent) - ((Int128)cells * origin)) switch {
                var start => (double.Max(0d, (double)start / cells), double.Min(size, (double)(start + extent) / cells)),
            };
    }
}

[SmartEnum<string>]
public sealed partial class ScreenShape {
    private const double Elongation = double.Pi / 1.6d;

    public static readonly ScreenShape Round = new("round", static (u, v) => (float)Quarter(double.Hypot(u, v), 1d, 1d));
    public static readonly ScreenShape Ellipse = new("ellipse", static (u, v) => (float)(Elongation * Quarter(double.Hypot(u, v / Elongation), 1d, 1d / Elongation)));
    public static readonly ScreenShape Diamond = new("diamond", static (u, v) => (float.Abs(u) + float.Abs(v)) switch { <= 1f and var d => d * d / 2f, var d => 1f - ((2f - d) * (2f - d) / 2f) });
    public static readonly ScreenShape Square = new("square", static (u, v) => float.Max(float.Abs(u), float.Abs(v)) switch { var m => m * m });
    public static readonly ScreenShape Line = new("line", static (_, v) => float.Abs(v));
    public static readonly ScreenShape WavyLine = new("wavy-line", static (u, v) => (v - (0.5f * float.SinPi(u))) switch { var w => float.Abs(w - (2f * float.Floor((w + 1f) / 2f))) });
    public static readonly ScreenShape Cross = new("cross", static (u, v) => float.Min(float.Abs(u), float.Abs(v)) switch { var m => 1f - ((1f - m) * (1f - m)) });

    [UseDelegateFromConstructor]
    public partial float Spot(float u, float v);

    public float Threshold(double u, double v) => Spot((float)((2d * (u - double.Floor(u))) - 1d), (float)((2d * (v - double.Floor(v))) - 1d));

    private static double Quarter(double rho, double width, double height) =>
        double.Min(rho, width) switch {
            var reach => double.Min(double.Sqrt(double.Max((rho * rho) - (height * height), 0d)), reach) switch { var knee => (height * knee) + Arc(rho, reach) - Arc(rho, knee) },
        };

    private static double Arc(double rho, double x) => double.Sqrt((rho * rho) - (x * x)) switch { var y => ((x * y) + (rho * rho * double.Atan2(x, y))) / 2d };
}

public sealed record HalftoneScreen(Matrix3x2 Frame, double Pitch, ScreenShape Shape, float Onset) {
    public float Coverage((double X, double Y) point, float level) {
        (double acrossX, double acrossY, double downX, double downY) = (Frame.M11 / Pitch, Frame.M12 / Pitch, Frame.M21 / Pitch, Frame.M22 / Pitch);
        (double u, double v) = ((point.X * acrossX) + (point.Y * downX) + Frame.M31, (point.X * acrossY) + (point.Y * downY) + Frame.M32);
        (float t, float k) = (Shape.Threshold(u, v), float.Clamp((level - Onset) / (1f - Onset), 0f, 1f));
        float w = float.Abs(Shape.Threshold(u + acrossX, v + acrossY) - t) + float.Abs(Shape.Threshold(u + downX, v + downY) - t);
        return w > 0f ? float.Clamp((((k * (1f + w)) - (w / 2f) - t) / w) + 0.5f, 0f, 1f) : t < k ? 1f : 0f;
    }
}

[ValueObject<string>(SkipIParsable = true, SkipIComparable = true)]
[ValidationError<ImagingFault>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CharacterSet {
    public static Fin<CharacterSet> Validated(string value) =>
        TryCreate(value, out CharacterSet? characters, out ImagingFault? fault) ? characters : fault;

    static partial void ValidateFactoryArguments(ref ImagingFault? validationError, ref string value) =>
        validationError = value.Length > 0 && !value.AsSpan().ContainsAnyExceptInRange(' ', '~') && toHashSet(value).Count == value.Length ? null
            : new ImagingFault($"character set '{value}'", ImagingReason.Range);
}

public sealed class GlyphAtlas {
    private readonly Arr<ReadOnlyMemory<byte>> planes;
    private readonly Arr<int> ranked;

    public GlyphAtlas(CharacterSet glyphs, HersheyFonts font, double cell, PixelExtent extent) {
        string set = glyphs;
        (Size Size, int Baseline)[] measures = [.. set.Select(glyph => {
            int baseline = 0;
            return (CvInvoke.GetTextSize(glyph.ToString(), font, 1d, 1, ref baseline), baseline);
        })];
        (int widest, int tallest) = (measures.Max(static measure => measure.Size.Width), measures.Max(static measure => measure.Size.Height + measure.Baseline));
        Grid = CellGrid.Fit(extent, cell * widest / tallest, cell);
        Tile = new((int)((extent.Width + Grid.Columns - 1L) / Grid.Columns), (int)((extent.Height + Grid.Rows - 1L) / Grid.Rows));
        double scale = double.Min((double)Tile.Height / tallest, (double)Tile.Width / widest);
        (ReadOnlyMemory<byte> Texels, double Mean)[] drawn = [.. Enumerable.Range(0, set.Length).Select(i => {
            using Mat plane = Mat.Zeros(Tile.Height, Tile.Width, DepthType.Cv8U, 1);
            CvInvoke.PutText(
                plane, set[i].ToString(), new Point((int)double.Round((Tile.Width - (measures[i].Size.Width * scale)) / 2d, MidpointRounding.ToEven), Tile.Height - (int)double.Ceiling(measures[i].Baseline * scale)),
                font, scale, new MCvScalar(byte.MaxValue), int.Max(1, (int)double.Round(scale, MidpointRounding.ToEven)), LineType.AntiAlias);
            return (new ReadOnlyMemory<byte>(plane.GetSpan<byte>().ToArray()), CvInvoke.Mean(plane).V0);
        })];
        planes = [.. drawn.Select(static glyph => glyph.Texels)];
        ranked = [.. Enumerable.Range(0, drawn.Length).OrderBy(i => drawn[i].Mean)];
    }

    public CellGrid Grid { get; }
    public Size Tile { get; }

    public int Glyph(float level) => ranked[int.Min(ranked.Count - 1, (int)(level * ranked.Count))];

    public float Coverage(int glyph, int row, int column) => planes[glyph].Span.AsSpan2D(Tile.Height, Tile.Width)[row, column] / (float)byte.MaxValue;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class JitteredCells {
    public static Vector2 Offset(long column, long line, float randomness, uint field) => (CoordinateHash.Uniforms(column, line, field) * randomness).AsVector2();

    public static IEnumerable<(long X, long Y)> Covering((double Left, double Top, double Right, double Bottom) box, double reach, float randomness) {
        (long left, long top) = ((long)double.Floor(box.Left - randomness - reach), (long)double.Floor(box.Top - randomness - reach));
        (long right, long bottom) = ((long)double.Floor(box.Right + reach), (long)double.Floor(box.Bottom + reach));
        for (long y = top; y <= bottom; y++) {
            for (long x = left; x <= right; x++)
                yield return (x, y);
        }
    }
}
