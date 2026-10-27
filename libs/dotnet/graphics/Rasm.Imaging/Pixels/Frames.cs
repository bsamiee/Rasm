using System.Drawing;
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Helpers;
using Emgu.CV;
using Emgu.CV.CvEnum;
using TinyEXR;

namespace Rasm.Imaging.Pixels;

// --- [MODELS] --------------------------------------------------------------------------
[ComplexValueObject(SkipFactoryMethods = true)]
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
        ImagingFault.Unless(width >= 1 && height >= 1 && width <= System.Array.MaxLength / 4 / height, ImagingReason.Range, (width, height)).Map(_ => new PixelExtent(width, height));
}

[ValueObject<float>(ConversionToKeyMemberType = ConversionOperatorsGeneration.None, UnsafeConversionToKeyMemberType = ConversionOperatorsGeneration.Implicit, SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<ImagingFault>]
public sealed partial class ShortSideLength {
    public static ShortSideLength Zero { get; } = Create(0f);

    public double Pixels(PixelExtent extent) => (double)_value * extent.ShortSide;

    static partial void ValidateFactoryArguments(ref ImagingFault? validationError, ref float value) =>
        validationError = value >= 0f ? null : new ImagingFault(string.Create(CultureInfo.InvariantCulture, $"short-side length {value}"), ImagingReason.Range);
}

[ValueObject<float>(ConversionToKeyMemberType = ConversionOperatorsGeneration.None, UnsafeConversionToKeyMemberType = ConversionOperatorsGeneration.Implicit, SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<ImagingFault>]
public sealed partial class ShortSideExtent {
    public double Pixels(PixelExtent extent) => (double)_value * extent.ShortSide;

    static partial void ValidateFactoryArguments(ref ImagingFault? validationError, ref float value) =>
        validationError = value > 0f ? null : new ImagingFault(string.Create(CultureInfo.InvariantCulture, $"short-side extent {value}"), ImagingReason.Range);
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
    public static readonly WrapMode Black = new("black", windowed: true, tiles: false, border: BorderType.Constant, static (point, size, offset) => (point + offset) switch {
        var center => (double.Clamp(center, 0.5d, size - 0.5d), double.Clamp(center + 0.5d, 0d, 1d) * double.Clamp(size + 0.5d - center, 0d, 1d)),
    });
    public static readonly WrapMode Clamp = new("clamp", windowed: false, tiles: false, border: BorderType.Replicate, static (point, size, offset) => (double.Clamp(point + offset, 0.5d, size - 0.5d), 1d));
    public static readonly WrapMode Periodic = new("periodic", windowed: false, tiles: true, border: BorderType.Wrap, static (point, size, offset) => (((point % size) + (offset % size)) % size) switch {
        var turn => (turn < 0d ? turn + size : turn, 1d),
    });
    public static readonly WrapMode Mirror = new("mirror", windowed: false, tiles: true, border: BorderType.Reflect, static (point, size, offset) => (2d * size) switch {
        var period => double.Abs(((point % period) + (offset % period)) % period) switch { var turn => (double.Min(turn, period - turn), 1d) },
    });

    private readonly bool windowed;

    public bool Tiles { get; }
    internal BorderType Border { get; }

    [UseDelegateFromConstructor]
    public partial (double Point, double Weight) Fold(double point, int size, double offset);

    internal (int Start, int Size) Bounds(int origin, int stored, int extent) => windowed ? (origin, stored) : (0, extent);

    internal Option<int> Index(double index, int size) => Fold(index, size, 0.5d) switch { (var point, > 0d) => Some((int)point), _ => None };
}

public sealed record FrameWindow(long Left, long Top, long Right, long Bottom);

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
    public FrameWindow Window => new(Origin.X, Origin.Y, (long)Origin.X + Size.Width, (long)Origin.Y + Size.Height);
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

    public T Sample<T>((double X, double Y) point, (WrapMode Across, WrapMode Down) wrap, T sum, Func<T, Vector4, double, T> add) =>
        ((ReadOnlySpan2D<Vector4>)View.Span).Sample(point, wrap, sum, add, Some((Origin, Extent)));

    public PixelFrame Downscaled(PixelExtent bound) =>
        Size.Fitted(bound) switch { var fitted => new(Point.Empty, fitted, fitted, block => ImageProcessing.Resize(Block, Size.Width, Size.Height, block, fitted.Width, fitted.Height, 4, ResizeFilter.Triangle, alphaChannel: 3)) };
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PixelPass {
    public abstract Fin<Unit> Run(PixelFrame frame, IProgress<int> progress);

    public sealed record Uniform(Action<Span<Vector4>> Row) : PixelPass {
        public override Fin<Unit> Run(PixelFrame frame, IProgress<int> progress) => RowAction.For(frame, (row, _, _) => Row(row), progress);
    }

    public sealed record Pointwise(Action<Span<Vector4>, long, long> Row) : PixelPass {
        public override Fin<Unit> Run(PixelFrame frame, IProgress<int> progress) => RowAction.For(frame, Row, progress);
    }

    public sealed record Frame(Func<PixelFrame, IProgress<int>, Fin<Unit>> Kernel) : PixelPass {
        public override Fin<Unit> Run(PixelFrame frame, IProgress<int> progress) => Kernel(frame, progress);
    }
}

public sealed record Channels<T>(T Red, T Green, T Blue) where T : notnull {
    public Channels(T all) : this(all, all, all) { }
}

file readonly struct RowAction(PixelFrame frame, Memory2D<Vector4> rows, Action<Span<Vector4>, long, long> kernel, IProgress<int> progress, StrongBox<int> finished) : IAction {
    internal static Unit For(PixelFrame frame, Action<Span<Vector4>, long, long> kernel, IProgress<int> progress) {
        ParallelHelper.For(0, frame.Size.Height, new RowAction(frame, frame.View, kernel, progress, new StrongBox<int>()));
        return unit;
    }

    public void Invoke(int i) {
        kernel(rows.Span.GetRowSpan(i), frame.Origin.X, frame.Line(i));
        progress.Report(Interlocked.Increment(ref finished.Value));
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class PixelSampling {
    public static Fin<TCount> GaussianTaps<TCount>(double sigma) where TCount : IBinaryInteger<TCount>, IMinMaxValue<TCount> => KernelTaps<TCount>(3d * sigma);

    public static Fin<TCount> KernelTaps<TCount>(double radius) where TCount : IBinaryInteger<TCount>, IMinMaxValue<TCount> =>
        double.IsFinite(radius) && radius >= 0d && ((2d * double.Ceiling(radius)) + 1d) is var taps && taps <= double.CreateTruncating(TCount.MaxValue)
            ? TCount.CreateTruncating(taps)
            : new ImagingFault(string.Create(CultureInfo.InvariantCulture, $"kernel radius {radius} up to {TCount.MaxValue} taps"), ImagingReason.Range);

    extension(Mat plane) {
        internal unsafe Span2D<T> View<T>() where T : unmanaged =>
            (plane.Cols * plane.ElementSize / sizeof(T)) switch {
                var width => new((void*)plane.DataPointer, plane.Rows, width, (plane.Step / sizeof(T)) - width),
            };
    }

    extension(Vector4 pixel) {
        public Vector4 Unassociated => pixel.W > 0f ? new(pixel.AsVector3() / pixel.W, pixel.W) : new(Vector3.Zero, pixel.W);
        public Vector256<double> Wide => Vector256.WidenLower(pixel.AsVector128().ToVector256());
    }

    extension(Vector256<double> lanes) {
        public Vector128<float> Narrowed => Vector256.Narrow(lanes, Vector256<double>.Zero).GetLower();
    }

    extension<T>(Channels<T> channels) where T : INumberBase<T> {
        public Vector256<double> Lanes => Vector256.Create(double.CreateTruncating(channels.Red), double.CreateTruncating(channels.Green), double.CreateTruncating(channels.Blue), 0d);
    }

    extension(ReadOnlySpan2D<Vector4> plane) {
        public Vector4 Sample((double X, double Y) point, (WrapMode Across, WrapMode Down) wrap) =>
            plane.Sample(point, wrap, Vector256<double>.Zero, static (sum, pixel, weight) => sum + (pixel.Wide * weight)).Narrowed.AsVector4();
    }

    extension<TPixel>(ReadOnlySpan2D<TPixel> plane) {
        public T Sample<T>((double X, double Y) point, (WrapMode Across, WrapMode Down) wrap, T sum, Func<T, TPixel, double, T> add, Option<(Point Origin, PixelExtent Extent)> framing = default) {
            if (plane.IsEmpty)
                return sum;
            (int storedWidth, int storedHeight) = (plane.Width, plane.Height);
            (Point origin, int width, int height) = framing.Match(Some: static frame => (frame.Origin, frame.Extent.Width, frame.Extent.Height), None: () => (Point.Empty, storedWidth, storedHeight));
            ((int boundsX, int boundsWidth), (int boundsY, int boundsHeight)) = (wrap.Across.Bounds(origin.X, storedWidth, width), wrap.Down.Bounds(origin.Y, storedHeight, height));
            ((double horizontalPoint, double horizontalWeight), (double verticalPoint, double verticalWeight)) =
                (wrap.Across.Fold(point.X - boundsX, boundsWidth, 0d), wrap.Down.Fold(point.Y - boundsY, boundsHeight, 0d));
            (double x, double y) = (horizontalPoint - 0.5d, verticalPoint - 0.5d);
            (double left, double top) = (double.Floor(x), double.Floor(y));
            (double across, double down) = (x - left, y - top);
            (Option<int> upper, Option<int> lower, Option<int> near, Option<int> far) = (wrap.Down.Index(top, boundsHeight), wrap.Down.Index(top + 1, boundsHeight), wrap.Across.Index(left, boundsWidth), wrap.Across.Index(left + 1, boundsWidth));
            for (int tap = 0; tap < 4; tap++) {
                (int row, int column) = (tap >> 1, tap & 1);
                double weight = horizontalWeight * verticalWeight * (row is 0 ? 1d - down : down) * (column is 0 ? 1d - across : across);
                sum = (row is 0 ? upper : lower, column is 0 ? near : far) switch {
                    ( { IsSome: true } atRow, { IsSome: true } atColumn)
                        when weight > 0d && (long)(int)atRow + boundsY - origin.Y is var storedRow and >= 0 && storedRow < storedHeight
                            && (long)(int)atColumn + boundsX - origin.X is var storedColumn and >= 0 && storedColumn < storedWidth =>
                        add(sum, plane[(int)storedRow, (int)storedColumn], weight),
                    _ => sum,
                };
            }
            return sum;
        }
    }
}
