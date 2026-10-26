using System.Drawing;
using System.Globalization;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;
using Emgu.CV;
using Emgu.CV.CvEnum;
using SixLabors.ImageSharp.PixelFormats;
using TinyEXR;

namespace Rasm.Imaging.Pixels;

// --- [MODELS] --------------------------------------------------------------------------
[ComplexValueObject]
[ValidationError<ImagingFault>]
public sealed partial class PixelExtent {
    public static PixelExtent Analysis { get; } = new(256, 256);

    public int Width { get; }
    public int Height { get; }
    public int ShortSide => int.Min(Width, Height);
    public PixelExtent Transposed => new(Height, Width);

    public PixelExtent Fitted(PixelExtent bound) =>
        (long)bound.Width * Height <= (long)bound.Height * Width
            ? bound.Width < Width ? new(bound.Width, (int)((((long)Height * bound.Width) + Width - 1) / Width)) : this
            : bound.Height < Height ? new((int)((((long)Width * bound.Height) + Height - 1) / Height), bound.Height) : this;

    public static Fin<PixelExtent> Validated(int width, int height) =>
        TryCreate(width, height, out PixelExtent? extent, out ImagingFault? fault) ? extent : fault;

    static partial void ValidateFactoryArguments(ref ImagingFault? validationError, ref int width, ref int height) =>
        validationError = width >= 1 && height >= 1 && width <= System.Array.MaxLength / 4 / height ? null
            : new ImagingFault(string.Create(CultureInfo.InvariantCulture, $"Extent {width}x{height} must be positive with at most {System.Array.MaxLength / 4} pixels"));
}

[ValueObject<float>(ConversionToKeyMemberType = ConversionOperatorsGeneration.None, UnsafeConversionToKeyMemberType = ConversionOperatorsGeneration.Implicit, SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<ImagingFault>]
public sealed partial class ShortSideLength {
    public static ShortSideLength Zero { get; } = Create(0f);

    public double Pixels(PixelExtent extent) => (double)_value * extent.ShortSide;

    static partial void ValidateFactoryArguments(ref ImagingFault? validationError, ref float value) =>
        validationError = value >= 0f ? null : new ImagingFault(string.Create(CultureInfo.InvariantCulture, $"Short-side length {value} must be non-negative"));
}

[ValueObject<float>(ConversionToKeyMemberType = ConversionOperatorsGeneration.None, UnsafeConversionToKeyMemberType = ConversionOperatorsGeneration.Implicit, SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<ImagingFault>]
public sealed partial class ShortSideExtent {
    public double Pixels(PixelExtent extent) => (double)_value * extent.ShortSide;

    static partial void ValidateFactoryArguments(ref ImagingFault? validationError, ref float value) =>
        validationError = value > 0f ? null : new ImagingFault(string.Create(CultureInfo.InvariantCulture, $"Short-side extent {value} must be positive"));
}

public sealed record FramePosition(float X, float Y) {
    public static FramePosition Center { get; } = new(0.5f, 0.5f);
    public static FramePosition TopLeft { get; } = new(0f, 0f);
    public static FramePosition TopRight { get; } = new(1f, 0f);
    public static FramePosition BottomRight { get; } = new(1f, 1f);
    public static FramePosition BottomLeft { get; } = new(0f, 1f);

    public (double X, double Y) Point(PixelExtent extent) => ((double)X * extent.Width, (double)Y * extent.Height);
}

[SmartEnum<string>]
public sealed partial class WrapMode {
    public static readonly WrapMode Black = new("black", static (point, size, offset) => (point + offset) switch {
        var center => (double.Clamp(center, 0.5d, size - 0.5d), double.Clamp(center + 0.5d, 0d, 1d) * double.Clamp(size + 0.5d - center, 0d, 1d)),
    });
    public static readonly WrapMode Clamp = new("clamp", static (point, size, offset) => (double.Clamp(point + offset, 0.5d, size - 0.5d), 1d));
    public static readonly WrapMode Periodic = new("periodic", static (point, size, offset) => (((point % size) + (offset % size)) % size) switch {
        var turn => (turn < 0d ? turn + size : turn, 1d),
    });
    public static readonly WrapMode Mirror = new("mirror", static (point, size, offset) => (2d * size) switch {
        var period => double.Abs(((point % period) + (offset % period)) % period) switch { var turn => (double.Min(turn, period - turn), 1d) },
    });

    public bool Tiles => Map(black: false, clamp: false, periodic: true, mirror: true);

    [UseDelegateFromConstructor]
    public partial (double Point, double Weight) Fold(double point, int size, double offset);

    internal Option<int> Index(double index, int size) =>
        Fold(index, size, 0.5d) switch {
            (var point, > 0d) => Some((int)point),
            _ => None,
        };
}

public sealed class PixelFrame {
    private readonly float[] block;

    public PixelFrame(Point origin, PixelExtent size, PixelExtent extent) =>
        (Origin, Size, Extent, block) = (origin, size, extent, GC.AllocateUninitializedArray<float>(4 * size.Width * size.Height, pinned: true));

    public PixelFrame(Point origin, PixelExtent size, PixelExtent extent, Action<float[]> fill) : this(origin, size, extent) => fill(block);

    public Point Origin { get; }
    public PixelExtent Size { get; }
    public PixelExtent Extent { get; }
    public ReadOnlySpan<float> Block => block;
    public nint Address => Marshal.UnsafeAddrOfPinnedArrayElement(block, 0);
    public (long Left, long Top, long Right, long Bottom) Window => (Origin.X, Origin.Y, (long)Origin.X + Size.Width, (long)Origin.Y + Size.Height);
    public bool Whole => Origin == Point.Empty && Size == Extent;
    public Memory2D<Vector4> View => block.AsMemory().Cast<float, Vector4>().AsMemory2D(Size.Height, Size.Width);

    public long Line(int index) => (long)Extent.Height - 1 - Origin.Y - index;

    public int RowIndex(long line) => int.CreateChecked((long)Extent.Height - 1 - Origin.Y - line);

    public Span<Vector4> Row(long line) => View.Span.GetRowSpan(RowIndex(line));

    public PixelFrame Copy() => new(Origin, Size, Extent, destination => Block.CopyTo(destination));

    public Mat Header() => new(Size.Height, Size.Width, DepthType.Cv32F, 4, Address, step: 0);

    public Fin<Unit> Filter(Func<Mat, Fin<Unit>> apply) {
        using Mat header = Header();
        return Try.lift(() => apply(header)).Run();
    }

    public Fin<Unit> Mutate(Action<SixLabors.ImageSharp.Image<RgbaVector>> edit) {
        Span2D<float> samples = block.AsSpan().AsSpan2D(Size.Width * Size.Height, 4);
        using SpanOwner<float> rental = SpanOwner<float>.Allocate(samples.Height);
        Span2D<float> alpha = rental.Span.AsSpan2D(samples.Height, 1);
        samples.GetColumn(3).CopyTo(alpha.GetColumn(0));
        samples.GetColumn(3).Fill(1f);
        using SixLabors.ImageSharp.Image<RgbaVector> image = SixLabors.ImageSharp.Image.WrapMemory(block.AsMemory().Cast<float, RgbaVector>(), Size.Width, Size.Height);
        Fin<Unit> edited = Try.lift(() => edit(image)).Run();
        alpha.GetColumn(0).CopyTo(samples.GetColumn(3));
        return edited;
    }

    public T Sample<T>((double X, double Y) point, (WrapMode Across, WrapMode Down) wrap, T sum, Func<T, Vector4, double, T> add) =>
        ((ReadOnlySpan2D<Vector4>)View.Span).Sample(point, wrap, sum, add, Some((Origin, Extent)));

    public PixelFrame Downscaled(PixelExtent bound) =>
        Size.Fitted(bound) switch {
            var fitted => new(Point.Empty, fitted, fitted, block =>
                ImageProcessing.Resize(Block, Size.Width, Size.Height, block, fitted.Width, fitted.Height, 4, ResizeFilter.Triangle, alphaChannel: 3)),
        };
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class PixelSampling {
    public static Fin<TCount> GaussianTaps<TCount>(double sigma) where TCount : IBinaryInteger<TCount>, IMinMaxValue<TCount> => KernelTaps<TCount>(3d * sigma);

    public static Fin<TCount> KernelTaps<TCount>(double radius) where TCount : IBinaryInteger<TCount>, IMinMaxValue<TCount> =>
        double.IsFinite(radius) && radius >= 0d && ((2d * double.Ceiling(radius)) + 1d) is var taps && taps <= double.CreateTruncating(TCount.MaxValue)
            ? TCount.CreateTruncating(taps)
            : new ImagingFault(string.Create(CultureInfo.InvariantCulture, $"Kernel radius {radius} has no tap count up to {TCount.MaxValue}"));

    extension(Mat plane) {
        internal unsafe Span2D<T> View<T>() where T : unmanaged =>
            (plane.Cols * plane.ElementSize / sizeof(T)) switch {
                var width => new((void*)plane.DataPointer, plane.Rows, width, (plane.Step / sizeof(T)) - width),
            };
    }

    extension(ReadOnlySpan2D<Vector4> plane) {
        public Vector4 Sample((double X, double Y) point, (WrapMode Across, WrapMode Down) wrap) =>
            plane.Sample(point, wrap, Vector256<double>.Zero, static (sum, pixel, weight) =>
                sum + (Vector256.Create(pixel.X, pixel.Y, pixel.Z, pixel.W) * weight)) switch {
                    var value => new((float)value[0], (float)value[1], (float)value[2], (float)value[3]),
                };
    }

    extension<TPixel>(ReadOnlySpan2D<TPixel> plane) {
        public T Sample<T>((double X, double Y) point, (WrapMode Across, WrapMode Down) wrap, T sum, Func<T, TPixel, double, T> add, Option<(Point Origin, PixelExtent Extent)> framing = default) {
            if (plane.IsEmpty)
                return sum;
            (int storedWidth, int storedHeight) = (plane.Width, plane.Height);
            (Point origin, int width, int height) = framing.Match(Some: static frame => (frame.Origin, frame.Extent.Width, frame.Extent.Height), None: () => (Point.Empty, storedWidth, storedHeight));
            (int boundsX, int boundsY, int boundsWidth, int boundsHeight) = (
                wrap.Across == WrapMode.Black ? origin.X : 0,
                wrap.Down == WrapMode.Black ? origin.Y : 0,
                wrap.Across == WrapMode.Black ? storedWidth : width,
                wrap.Down == WrapMode.Black ? storedHeight : height);
            ((double horizontalPoint, double horizontalWeight), (double verticalPoint, double verticalWeight)) =
                (wrap.Across.Fold(point.X - boundsX, boundsWidth, 0d), wrap.Down.Fold(point.Y - boundsY, boundsHeight, 0d));
            (double x, double y) = (horizontalPoint - 0.5d, verticalPoint - 0.5d);
            (double left, double top) = (Math.Floor(x), Math.Floor(y));
            (double across, double down) = (x - left, y - top);
            for (int row = 0; row < 2; row++) {
                Option<int> wrappedRow = wrap.Down.Index(top + row, boundsHeight);
                double vertical = row is 0 ? 1d - down : down;
                for (int column = 0; column < 2; column++) {
                    double weight = horizontalWeight * verticalWeight * vertical * (column is 0 ? 1d - across : across);
                    sum = (wrappedRow, wrap.Across.Index(left + column, boundsWidth)) switch {
                        ( { IsSome: true } atRow, { IsSome: true } atColumn)
                            when weight > 0d && (long)(int)atRow + boundsY - origin.Y is var storedRow and >= 0 && storedRow < storedHeight
                                && (long)(int)atColumn + boundsX - origin.X is var storedColumn and >= 0 && storedColumn < storedWidth =>
                            add(sum, plane[(int)storedRow, (int)storedColumn], weight),
                        _ => sum,
                    };
                }
            }
            return sum;
        }
    }
}
