using System.Drawing;
using System.Numerics;
using System.Runtime.Intrinsics;
using CommunityToolkit.HighPerformance;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using MathNet.Numerics.Integration;
using PeterO.Numbers;
using Rasm.Imaging.Pixels;

namespace Rasm.Imaging.Procedural;

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct JitteredCells(float Randomness) {
    public Vector2 Offset(EInteger column, EInteger line, uint field) => (CoordinateHash.Uniforms(column, line, field) * Randomness).AsVector2();

    public IEnumerable<(EInteger X, EInteger Y)> Covering(ERational left, ERational top, ERational right, ERational bottom, ERational reach) {
        ERational jitter = ERational.FromSingle(Randomness);
        (EInteger firstX, EInteger firstY) = (LatticePoint.Split(left.Subtract(jitter).Subtract(reach)).Cell, LatticePoint.Split(top.Subtract(jitter).Subtract(reach)).Cell);
        (EInteger columns, EInteger rows) = (LatticePoint.Split(right.Add(reach)).Cell.Subtract(firstX).Add(1), LatticePoint.Split(bottom.Add(reach)).Cell.Subtract(firstY).Add(1));
        EInteger count = columns.Multiply(rows);
        for (EInteger index = EInteger.Zero; index.CompareTo(count) < 0; index = index.Add(1))
            yield return (firstX.Add(index.Remainder(columns)), firstY.Add(index.Divide(columns)));
    }
}

public readonly record struct SoftDisc(double Radius, double Softness) {
    private static readonly GaussLegendreRule Quadrature = new(0d, 1d, 32);

    public ERational Mass(ERational left, ERational top, ERational right, ERational bottom) {
        ERational radius = ERational.FromDouble(Radius);
        (ERational lowX, ERational lowY, ERational highX, ERational highY) =
            (ERational.Max(radius.Negate(), left), ERational.Max(radius.Negate(), top), ERational.Min(radius, right), ERational.Min(radius, bottom));
        if (lowX.CompareTo(highX) >= 0 || lowY.CompareTo(highY) >= 0)
            return ERational.Zero;
        ERational nearestY = ERational.Max(lowY, ERational.Min(ERational.Zero, highY));
        ERational nearestX = ERational.Max(lowX, ERational.Min(ERational.Zero, highX));
        ERational clearance = radius.Multiply(radius).Subtract(nearestX.Multiply(nearestX)).Subtract(nearestY.Multiply(nearestY));
        return clearance.Sign <= 0 ? ERational.Zero
            : lowX.CompareTo(radius.Negate()) is 0 && lowY.CompareTo(radius.Negate()) is 0 && highX.CompareTo(radius) is 0 && highY.CompareTo(radius) is 0
                ? ERational.FromDouble(double.Pi * Radius * Radius * (1d - Softness + (0.3d * Softness * Softness)))
                : Partial(lowX, lowY, highX, highY, radius, nearestY, right.Subtract(left).Denominator.GetUnsignedBitLengthAsEInteger().Add(clearance.Denominator.GetUnsignedBitLengthAsEInteger()));
    }

    private ERational Partial(ERational lowX, ERational lowY, ERational highX, ERational highY, ERational radius, ERational nearestY, EInteger bits) {
        EContext math = EContext.Binary64.WithUnlimitedExponents().WithPrecision(bits.Add(EContext.Binary64.Precision).ToInt32Checked());
        EFloat r = radius.ToEFloat(math);
        EFloat squared = r * r;
        EFloat horizontal = radius.Multiply(radius).Subtract(nearestY.Multiply(nearestY)).ToEFloat(math).Sqrt(math);
        (EFloat first, EFloat last, EFloat band) = (EFloat.Max(lowX.ToEFloat(math), -horizontal), EFloat.Min(highX.ToEFloat(math), horizontal), EFloat.FromDouble(Softness) * r);
        EFloat extent = last - first;
        EFloat sum = EFloat.Zero;
        for (int across = 0; across < Quadrature.Order; across++) {
            EFloat x = first + (extent * Quadrature.GetAbscissa(across));
            EFloat vertical = (squared - (x * x)).Sqrt(math);
            (EFloat bottom, EFloat top) = (EFloat.Max(lowY.ToEFloat(math), -vertical), EFloat.Min(highY.ToEFloat(math), vertical));
            EFloat height = top - bottom;
            for (int down = 0; down < Quadrature.Order; down++) {
                EFloat y = bottom + (height * Quadrature.GetAbscissa(down));
                EFloat fraction = band.IsZero ? EFloat.One : EFloat.Min(EFloat.One, (r - ((x * x) + (y * y)).Sqrt(math)).Divide(band, math));
                sum += fraction * fraction * (3 - (2 * fraction)) * extent * height * Quadrature.GetWeight(across) * Quadrature.GetWeight(down);
            }
        }
        return ERational.FromEFloat(sum);
    }
}

public sealed record CellGrid(EInteger Columns, EInteger Rows) {
    public static CellGrid Fit(PixelExtent extent, ERational width, ERational height) {
        return new(Count(extent.Width, width), Count(extent.Height, height));

        static EInteger Count(int pixels, ERational size) => ERational.FromInt32(pixels).Divide(size) switch {
            var count => EInteger.Max(EInteger.One, EFloat.FromEInteger(count.Numerator).DivideToExponent(EFloat.FromEInteger(count.Denominator), 0L, ERounding.HalfEven).ToEInteger()),
        };
    }

    public (EInteger Cell, int Remainder) Column(long x, int width) => Coordinate(x, width, Columns);

    public (EInteger Cell, int Remainder) Row(long y, int height) => Coordinate(y, height, Rows);

    public unsafe Func<(EInteger Column, EInteger Row), Vector3> Means(PixelFrame frame, Mat plane) {
        foreach (ref Vector4 pixel in new Span<Vector4>(plane.DataPointer.ToPointer(), plane.Rows * plane.Cols))
            pixel = new Vector4(pixel.AsVector3() * pixel.W, pixel.W);
        bool native = frame.Whole && Columns.Multiply(Rows).CompareTo(int.MaxValue) <= 0 && Columns.Multiply(plane.NumberOfChannels).Add(Rows).CompareTo(int.MaxValue) <= 0;
        if (native) {
            CvInvoke.Resize(plane, plane, new Size(Columns.ToInt32Checked(), Rows.ToInt32Checked()), 0d, 0d, Inter.Area);
            foreach (ref Vector4 pixel in new Span<Vector4>(plane.DataPointer.ToPointer(), plane.Rows * plane.Cols))
                pixel = pixel.W > 0f ? new Vector4(pixel.AsVector3() / pixel.W, pixel.W) : Vector4.Zero;
        }
        CellGrid grid = this;
        return native
            ? cell => new Span<Vector4>(plane.DataPointer.ToPointer(), plane.Rows * plane.Cols)[(cell.Row.ToInt32Checked() * grid.Columns.ToInt32Checked()) + cell.Column.ToInt32Checked()].AsVector3()
            : cell => grid.Mean(plane, frame, cell);
    }

    private static (EInteger Cell, int Remainder) Coordinate(long pixel, int extent, EInteger cells) =>
        LatticePoint.Split(ERational.FromInt64(pixel).Multiply(ERational.FromEInteger(cells)).Divide(extent)) switch {
            var (cell, local) => (cell, local.Multiply(extent).ToInt32Checked()),
        };

    private unsafe Vector3 Mean(Mat source, PixelFrame frame, (EInteger Column, EInteger Row) cell) {
        if (Columns.CompareTo(frame.Extent.Width) > 0 || Rows.CompareTo(frame.Extent.Height) > 0) {
            ReadOnlySpan2D<Vector4> plane = new Span<Vector4>(source.DataPointer.ToPointer(), source.Rows * source.Cols).AsSpan2D(source.Rows, source.Cols);
            Vector4 pixel = plane.Sample(
                (Interpolated(cell.Column, frame.Extent.Width, Columns), Interpolated(cell.Row, frame.Extent.Height, Rows)),
                (WrapMode.Black, WrapMode.Black), Vector4.Zero, static (sum, pixel, weight) => sum + (pixel * (float)weight), Some((frame.Origin, frame.Extent)));
            return pixel.W > 0f ? pixel.AsVector3() / pixel.W : Vector3.Zero;
        }
        (double left, double right) = Bounds(cell.Column, frame.Extent.Width, frame.Origin.X, frame.Size.Width, Columns);
        (double top, double bottom) = Bounds(cell.Row, frame.Extent.Height, frame.Origin.Y, frame.Size.Height, Rows);
        ReadOnlySpan<Vector4> pixels = new(source.DataPointer.ToPointer(), source.Rows * source.Cols);
        Vector256<double> sum = Vector256<double>.Zero;
        for (int y = (int)double.Floor(top); y < (int)double.Ceiling(bottom); y++) {
            double down = double.Min(y + 1d, bottom) - double.Max(y, top);
            for (int x = (int)double.Floor(left); x < (int)double.Ceiling(right); x++) {
                Vector4 pixel = pixels[(y * source.Cols) + x];
                sum += Vector256.Create(pixel.X, pixel.Y, pixel.Z, pixel.W) * (down * (double.Min(x + 1d, right) - double.Max(x, left)));
            }
        }
        return sum.GetElement(3) > 0d
            ? new Vector3((float)(sum.GetElement(0) / sum.GetElement(3)), (float)(sum.GetElement(1) / sum.GetElement(3)), (float)(sum.GetElement(2) / sum.GetElement(3)))
            : Vector3.Zero;

        static double Interpolated(EInteger index, int extent, EInteger cells) =>
            LatticePoint.Split(ERational.FromEInteger(index).Multiply(extent).Divide(ERational.FromEInteger(cells))) switch {
                var (pixel, local) => (Pixel: pixel, Remainder: local.Multiply(ERational.FromEInteger(cells)).ToEIntegerIfExact()) switch {
                    var at => ERational.FromEInteger(at.Pixel).ToDouble() + 0.5d + (index.Sign >= 0 && index.CompareTo(cells) < 0 && at.Pixel.CompareTo(extent - 1) is 0 ? 0d
                        : double.Max(0d, ERational.FromEInteger(EInteger.FromInt32(extent).Subtract(cells).Add(at.Remainder)).Divide(extent).ToDouble())),
                },
            };

        static (double Left, double Right) Bounds(EInteger index, int extent, int origin, int size, EInteger cells) =>
            index.Multiply(extent).Subtract(cells.Multiply(origin)) switch {
                var start => (double.Max(0d, ERational.FromEInteger(start).Divide(ERational.FromEInteger(cells)).ToDouble()),
                    double.Min(size, ERational.FromEInteger(start.Add(extent)).Divide(ERational.FromEInteger(cells)).ToDouble())),
            };
    }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
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

    public float Threshold((ERational U, ERational V) cell) =>
        Spot(LatticePoint.Split(cell.U).Local.Multiply(2).Subtract(1).ToSingle(), LatticePoint.Split(cell.V).Local.Multiply(2).Subtract(1).ToSingle());

    private static double Quarter(double rho, double width, double height) =>
        double.Min(rho, width) switch {
            var reach => double.Min(double.Sqrt(double.Max((rho * rho) - (height * height), 0d)), reach) switch { var knee => (height * knee) + Arc(rho, reach) - Arc(rho, knee) },
        };

    private static double Arc(double rho, double x) => double.Sqrt((rho * rho) - (x * x)) switch { var y => ((x * y) + (rho * rho * double.Atan2(x, y))) / 2d };
}

public readonly record struct HalftoneScreen(Matrix3x2 Frame, ERational Pitch, ScreenShape Shape, float Onset) {
    public float Coverage((double X, double Y) point, float level) {
        (ERational x, ERational y) = (ERational.FromDouble(point.X), ERational.FromDouble(point.Y));
        (ERational acrossX, ERational acrossY, ERational downX, ERational downY) =
            (ERational.FromSingle(Frame.M11).Divide(Pitch), ERational.FromSingle(Frame.M12).Divide(Pitch), ERational.FromSingle(Frame.M21).Divide(Pitch), ERational.FromSingle(Frame.M22).Divide(Pitch));
        (ERational u, ERational v) = (x.Multiply(acrossX).Add(y.Multiply(downX)).Add(ERational.FromSingle(Frame.M31)), x.Multiply(acrossY).Add(y.Multiply(downY)).Add(ERational.FromSingle(Frame.M32)));
        (float t, float k) = (Shape.Threshold((u, v)), float.Clamp((level - Onset) / (1f - Onset), 0f, 1f));
        float w = float.Abs(Shape.Threshold((u.Add(acrossX), v.Add(acrossY))) - t) + float.Abs(Shape.Threshold((u.Add(downX), v.Add(downY))) - t);
        return w > 0f ? float.Clamp((((k * (1f + w)) - (w / 2f) - t) / w) + 0.5f, 0f, 1f) : t < k ? 1f : 0f;
    }
}

[ValueObject<string>(SkipIParsable = true)]
[ValidationError<ImagingFault>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CharacterSet {
    static partial void ValidateFactoryArguments(ref ImagingFault? validationError, ref string value) =>
        validationError = value.Length > 0 && !value.AsSpan().ContainsAnyExceptInRange(' ', '~') && toHashSet(value).Count == value.Length ? null
            : new ImagingFault($"Character set '{value}' must be distinct printable ASCII characters");
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class GlyphAtlas : IDisposable {
    private readonly VectorOfMat planes = new();
    private readonly Arr<int> ranked;

    public GlyphAtlas(CharacterSet glyphs, HersheyFonts font, ERational cell, PixelExtent extent) {
        string set = glyphs;
        (Size Size, int Baseline)[] measures = [.. set.Select(glyph => {
            int baseline = 0;
            return (CvInvoke.GetTextSize(glyph.ToString(), font, 1d, 1, ref baseline), baseline);
        })];
        (int widest, int tallest) = (measures.Max(static measure => measure.Size.Width), measures.Max(static measure => measure.Size.Height + measure.Baseline));
        Grid = CellGrid.Fit(extent, cell.Multiply(widest).Divide(tallest), cell);
        Tile = new(EInteger.FromInt32(extent.Width).Add(Grid.Columns).Subtract(1).Divide(Grid.Columns).ToInt32Checked(),
            EInteger.FromInt32(extent.Height).Add(Grid.Rows).Subtract(1).Divide(Grid.Rows).ToInt32Checked());
        double scale = double.Min((double)Tile.Height / tallest, (double)Tile.Width / widest);
        for (int i = 0; i < set.Length; i++) {
            using Mat plane = Mat.Zeros(Tile.Height, Tile.Width, DepthType.Cv8U, 1);
            CvInvoke.PutText(
                plane, set[i].ToString(), new Point((int)double.Round((Tile.Width - (measures[i].Size.Width * scale)) / 2d, MidpointRounding.ToEven), Tile.Height - (int)double.Ceiling(measures[i].Baseline * scale)),
                font, scale, new MCvScalar(byte.MaxValue), int.Max(1, (int)double.Round(scale, MidpointRounding.ToEven)), LineType.AntiAlias);
            planes.Push(plane);
        }
        ranked = [.. Enumerable.Range(0, set.Length).OrderBy(i => {
            using Mat glyph = planes[i];
            return CvInvoke.Mean(glyph).V0;
        })];
    }

    public CellGrid Grid { get; }
    public Size Tile { get; }

    public int Glyph(float level) => ranked[int.Min(ranked.Count - 1, (int)(level * ranked.Count))];

    public unsafe float Coverage(int glyph, int row, int column) {
        using Mat plane = planes[glyph];
        return new ReadOnlySpan<byte>(plane.GetDataPointer(row).ToPointer(), Tile.Width)[column] / (float)byte.MaxValue;
    }

    public void Dispose() => ((IDisposable)planes).Dispose();
}
