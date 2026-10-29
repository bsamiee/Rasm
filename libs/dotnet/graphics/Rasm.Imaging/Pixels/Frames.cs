using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;
using CommunityToolkit.HighPerformance.Helpers;
using Emgu.CV;
using Emgu.CV.CvEnum;
using SixLabors.ImageSharp.PixelFormats;
using TinyEXR;

namespace Rasm.Imaging.Pixels;

// --- [MODELS] --------------------------------------------------------------------------
[ComplexValueObject(SkipFactoryMethods = true)]
public sealed partial class PixelExtent {
    public int Width { get; }
    public int Height { get; }
    public int ShortSide => int.Min(Width, Height);
    public PixelExtent Transposed => new(Height, Width);

    public Vector2 Point(Vector2 position) => position * new Vector2(Width, Height);

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
    public double Pixels(PixelExtent extent) => (double)_value * extent.ShortSide;

    static partial void ValidateFactoryArguments(ref ImagingFault? validationError, ref float value) =>
        validationError = value > 0f && float.IsFinite(value) ? null : ImagingFault.Of(ImagingReason.Range, value);
}

[SmartEnum<string>]
public sealed partial class WrapMode {
    public static readonly WrapMode Black = new("black", BorderType.Constant, static (point, size, offset) => (point + offset) switch {
        var center => (double.Clamp(center, 0.5d, size - 0.5d), double.Clamp(center + 0.5d, 0d, 1d) * double.Clamp(size + 0.5d - center, 0d, 1d)),
    });
    public static readonly WrapMode Clamp = new("clamp", BorderType.Replicate, static (point, size, offset) => (double.Clamp(point + offset, 0.5d, size - 0.5d), 1d));
    public static readonly WrapMode Periodic = new("periodic", BorderType.Wrap, static (point, size, offset) => (((point % size) + (offset % size)) % size) switch {
        var turn => (turn < 0d ? turn + size : turn, 1d),
    });
    public static readonly WrapMode Mirror = new("mirror", BorderType.Reflect, static (point, size, offset) => (2d * size) switch {
        var period => double.Abs(((point % period) + (offset % period)) % period) switch { var turn => (double.Min(turn, period - turn), 1d) },
    });

    internal BorderType Border { get; }
    internal Func<double, int, double, (double Point, double Weight)> Fold { get; }

    internal bool Tiles => Border is BorderType.Wrap or BorderType.Reflect;

    internal Option<int> Stored(double index, int origin, int stored, int extent) =>
        Window(origin, stored, extent) switch {
            var (start, size) => Fold(index - start, size, 0.5d) switch {
                (var point, > 0d) when (int)point + start - origin is var at && at >= 0 && at < stored => Some(at),
                _ => None,
            },
        };

    internal (double Weight, double Share, Option<int> Near, Option<int> Far) Taps(double point, int origin, int stored, int extent) =>
        Window(origin, stored, extent) switch {
            var (start, size) => Fold(point - start, size, 0d) switch {
                var (folded, weight) => double.Floor(folded - 0.5d) switch {
                    var left => (weight, folded - 0.5d - left, Stored(left + start, origin, stored, extent), Stored(left + start + 1d, origin, stored, extent)),
                },
            },
        };

    private (int Start, int Size) Window(int origin, int stored, int extent) => Border is BorderType.Constant ? (origin, stored) : (0, extent);
}

public sealed class PixelFrame {
    private readonly Vector4[] pixels;

    public PixelFrame(Point origin, PixelExtent size, PixelExtent extent) =>
        (Origin, Size, Extent, pixels) = (origin, size, extent, GC.AllocateUninitializedArray<Vector4>(size.Width * size.Height, pinned: true));

    public PixelFrame(Point origin, PixelExtent size, PixelExtent extent, Action<Span<float>> fill) : this(origin, size, extent) => fill(MemoryMarshal.Cast<Vector4, float>(pixels.AsSpan()));

    public Point Origin { get; }
    public PixelExtent Size { get; }
    public PixelExtent Extent { get; }
    public ReadOnlySpan<float> Block => MemoryMarshal.Cast<Vector4, float>(pixels.AsSpan());
    public nint Address => Marshal.UnsafeAddrOfPinnedArrayElement(pixels, 0);
    public Memory2D<Vector4> View => new(pixels, Size.Height, Size.Width);
    public Rectangle Window => new(Origin, new Size(Size.Width, Size.Height));
    public bool Whole => Origin == Point.Empty && Size == Extent;

    public PixelFrame Copy() => new(Origin, Size, Extent, destination => Block.CopyTo(destination));

    public Mat Header() => new(Size.Height, Size.Width, DepthType.Cv32F, 4, Address, step: 0);

    public Mat Premultiplied() {
        Mat associated = new(Size.Height, Size.Width, DepthType.Cv32F, 4);
        Associated(pixels, associated.GetSpan<Vector4>());
        return associated;
    }

    public Fin<Unit> Filter(Func<Mat, Fin<Unit>> apply) {
        using Mat header = Header();
        return Try.lift(() => apply(header)).Run();
    }

    public Fin<Unit> Mutate(Action<SixLabors.ImageSharp.Image<RgbaVector>> edit) {
        Span2D<float> samples = MemoryMarshal.Cast<Vector4, float>(pixels.AsSpan()).AsSpan2D(pixels.Length, 4);
        using SpanOwner<float> alpha = SpanOwner<float>.Allocate(pixels.Length);
        samples.GetColumn(3).CopyTo(alpha.Span);
        samples.GetColumn(3).Fill(1f);
        Fin<Unit> edited = Try.lift(() => {
            using SixLabors.ImageSharp.Image<RgbaVector> image = SixLabors.ImageSharp.Image.WrapMemory(SixLabors.ImageSharp.Configuration.Default, pixels.AsMemory().Cast<Vector4, RgbaVector>(), Size.Width, Size.Height);
            edit(image);
        }).Run();
        alpha.Span.CopyTo(samples.GetColumn(3));
        return edited;
    }

    public void Rows(Action<Span<Vector4>, int, int> row, Option<IProgress<int>> progress = default) =>
        ParallelHelper.For(0, Size.Height, new RowAction(this, row, progress, new StrongBox<int>()));

    public Vector4 Sample(Vector2 point, (WrapMode Across, WrapMode Down) wrap) => ((ReadOnlySpan2D<Vector4>)View.Span).Sample(point, wrap, Some((Origin, Extent)));

    public Option<Vector4> Texel(Point at, (WrapMode Across, WrapMode Down) wrap) =>
        (wrap.Down.Stored(at.Y, Origin.Y, Size.Height, Extent.Height), wrap.Across.Stored(at.X, Origin.X, Size.Width, Extent.Width)) switch {
            ( { IsSome: true } row, { IsSome: true } column) => View.Span[(int)row, (int)column],
            _ => None,
        };

    public Option<Vector4> Mean(Rectangle region) {
        Rectangle window = Rectangle.Intersect(region, Window);
        (Vector128<double> lower, Vector128<double> upper, long count) = (Vector128<double>.Zero, Vector128<double>.Zero, 0L);
        if (!window.IsEmpty)
            foreach (Vector4 sample in View.Span.Slice(window.Top - Origin.Y, window.Left - Origin.X, window.Height, window.Width))
                (lower, upper, count) = sample.W > 0f && Vector4.AllWhereAllBitsSet(Vector4.IsFinite(sample))
                    ? (lower + Vector128.WidenLower(sample.AsVector128()), upper + Vector128.WidenUpper(sample.AsVector128()), count + 1)
                    : (lower, upper, count);
        return count > 0 ? Some(Vector128.Narrow(lower / count, upper / count).AsVector4()) : None;
    }

    public PixelFrame Downscaled(PixelExtent bound) =>
        Size.Fitted(bound) switch { var fitted => new(Point.Empty, fitted, fitted, block => ImageProcessing.Resize(Block, Size.Width, Size.Height, block, fitted.Width, fitted.Height, 4, ResizeFilter.Triangle, alphaChannel: 3)) };

    public static void Associated(ReadOnlySpan<Vector4> row, Span<Vector4> into) =>
        PixelOperations<RgbaVector>.Instance.ToVector4(SixLabors.ImageSharp.Configuration.Default, MemoryMarshal.Cast<Vector4, RgbaVector>(row), into, PixelConversionModifiers.Premultiply);

    public static void Unassociated(Span<Vector4> row) {
        for (int x = 0; x < row.Length; x++) {
            Vector128<float> lanes = row[x].AsVector128();
            Vector128<float> alpha = Vector128.Shuffle(lanes, Vector128.Create(3));
            Vector128<float> color = Vector128.ConditionalSelect(Vector128.GreaterThan(alpha, Vector128<float>.Zero), lanes / alpha, Vector128<float>.Zero);
            row[x] = Vector128.ConditionalSelect(Vector128.Create(-1, -1, -1, 0).AsSingle(), color, lanes).AsVector4();
        }
    }

    private readonly struct RowAction(PixelFrame frame, Action<Span<Vector4>, int, int> row, Option<IProgress<int>> progress, StrongBox<int> finished) : IAction {
        public void Invoke(int i) {
            row(frame.pixels.AsSpan(i * frame.Size.Width, frame.Size.Width), frame.Origin.X, frame.Origin.Y + i);
            if (progress.Case is IProgress<int> report)
                report.Report(Interlocked.Increment(ref finished.Value));
        }
    }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial class PixelPass {
    public abstract Fin<Unit> Run(PixelFrame frame, Option<IProgress<int>> progress = default);

    public sealed class Point(Action<Span<Vector4>> row) : PixelPass {
        public Action<Span<Vector4>> Row => row;

        public override Fin<Unit> Run(PixelFrame frame, Option<IProgress<int>> progress = default) {
            frame.Rows((pixels, _, _) => row(pixels), progress);
            return unit;
        }
    }

    public sealed class Positional(Action<Span<Vector4>, int, int> row) : PixelPass {
        public Action<Span<Vector4>, int, int> Row => row;

        public override Fin<Unit> Run(PixelFrame frame, Option<IProgress<int>> progress = default) {
            frame.Rows(row, progress);
            return unit;
        }
    }

    public sealed class Frame(Func<PixelFrame, Option<IProgress<int>>, Fin<Unit>> kernel) : PixelPass {
        public Func<PixelFrame, Option<IProgress<int>>, Fin<Unit>> Kernel => kernel;

        public override Fin<Unit> Run(PixelFrame frame, Option<IProgress<int>> progress = default) => kernel(frame, progress);
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class PixelSampling {
    extension(Mat plane) {
        public unsafe Span2D<T> View<T>() where T : unmanaged =>
            (plane.Cols * plane.ElementSize / sizeof(T)) switch {
                var width => new((void*)plane.DataPointer, plane.Rows, width, (plane.Step / sizeof(T)) - width),
            };
    }

    extension(ReadOnlySpan2D<Vector4> plane) {
        public Vector4 Sample(Vector2 point, (WrapMode Across, WrapMode Down) wrap, Option<(Point Origin, PixelExtent Extent)> framing = default) {
            Vector4 sum = Vector4.Zero;
            if (plane.IsEmpty)
                return sum;
            (int width, int height) = (plane.Width, plane.Height);
            (Point origin, int columns, int rows) = framing.Map(static frame => (frame.Origin, frame.Extent.Width, frame.Extent.Height)).IfNone((Point.Empty, width, height));
            (double acrossWeight, double acrossShare, Option<int> left, Option<int> right) = wrap.Across.Taps(point.X, origin.X, width, columns);
            (double downWeight, double downShare, Option<int> top, Option<int> bottom) = wrap.Down.Taps(point.Y, origin.Y, height, rows);
            for (int tap = 0; tap < 4; tap++) {
                (int row, int column) = (tap >> 1, tap & 1);
                double weight = acrossWeight * downWeight * (row is 0 ? 1d - downShare : downShare) * (column is 0 ? 1d - acrossShare : acrossShare);
                sum += (row is 0 ? top : bottom, column is 0 ? left : right) switch {
                    ( { IsSome: true } stored, { IsSome: true } at) when weight > 0d => plane[(int)stored, (int)at] * (float)weight,
                    _ => Vector4.Zero,
                };
            }
            return sum;
        }
    }
}
