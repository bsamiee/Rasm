using System.Drawing;
using System.Globalization;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using CommunityToolkit.HighPerformance;
using Emgu.CV;
using Emgu.CV.CvEnum;
using PeterO.Numbers;
using SixLabors.ImageSharp.PixelFormats;
using TinyEXR;

namespace Rasm.Imaging.Pixels;

// --- [MODELS] --------------------------------------------------------------------------
[ComplexValueObject]
public readonly partial struct PixelExtent {
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
        ValidationResults.ToFin(Validate(width, height, out PixelExtent extent), extent);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int width, ref int height) =>
        validationError = width >= 1 && height >= 1 && width <= System.Array.MaxLength / 4 / height ? null
            : new ValidationError(string.Create(CultureInfo.InvariantCulture, $"Extent {width}x{height} must be positive with at most {System.Array.MaxLength / 4} pixels"));
}

[ValueObject<float>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Neutral", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct ShortSideOffset : IMinMaxValue<ShortSideOffset> {
    public static ShortSideOffset MinValue { get; } = new(-float.MaxValue);
    public static ShortSideOffset MaxValue { get; } = new(float.MaxValue);

    public double Pixels(PixelExtent extent) => (double)_value * extent.ShortSide;

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<float>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Neutral", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct ShortSideLength : IMinMaxValue<ShortSideLength> {
    public static ShortSideLength MinValue => Neutral;
    public static ShortSideLength MaxValue { get; } = new(float.MaxValue);

    public double Pixels(PixelExtent extent) => (double)_value * extent.ShortSide;

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct ShortSideExtent : IMinMaxValue<ShortSideExtent> {
    public static ShortSideExtent MinValue { get; } = new(float.BitIncrement(0f));
    public static ShortSideExtent MaxValue { get; } = new(float.MaxValue);

    public double Pixels(PixelExtent extent) => (double)_value * extent.ShortSide;

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct FrameAxis : IMinMaxValue<FrameAxis> {
    public static FrameAxis MinValue { get; } = new(-0.5f);
    public static FrameAxis MaxValue { get; } = new(1.5f);
    public static FrameAxis Start { get; } = new(0f);
    public static FrameAxis Middle { get; } = new(0.5f);
    public static FrameAxis End { get; } = new(1f);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

public sealed record FramePosition(FrameAxis X, FrameAxis Y) {
    public static FramePosition Center { get; } = new(FrameAxis.Middle, FrameAxis.Middle);
    public static FramePosition TopLeft { get; } = new(FrameAxis.Start, FrameAxis.Start);
    public static FramePosition TopRight { get; } = new(FrameAxis.End, FrameAxis.Start);
    public static FramePosition BottomRight { get; } = new(FrameAxis.End, FrameAxis.End);
    public static FramePosition BottomLeft { get; } = new(FrameAxis.Start, FrameAxis.End);

    public (double X, double Y) Point(PixelExtent extent) => ((double)X * extent.Width, (double)Y * extent.Height);
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

    public Span<Vector4> Row(long line) => MemoryMarshal.Cast<float, Vector4>(block.AsSpan()).AsSpan2D(Size.Height, Size.Width).GetRowSpan(RowIndex(line));

    public PixelFrame Copy() => new(Origin, Size, Extent, destination => Block.CopyTo(destination));

    public Mat Header() => new(Size.Height, Size.Width, DepthType.Cv32F, 4, Address, step: 0);

    public Fin<Unit> Filter(Func<Mat, Fin<Unit>> apply) => Try.lift(() => {
        using Mat header = Header();
        return apply(header);
    }).Run();

    public Fin<Unit> Mutate(Action<SixLabors.ImageSharp.Image<RgbaVector>> edit) => Try.lift(() => {
        using SixLabors.ImageSharp.Image<RgbaVector> image = new(Size.Width, Size.Height);
        Memory2D<Vector4> view = View;
        EachRow(static (pixels, stored) => { for (int x = 0; x < pixels.Length; x++) pixels[x] = stored[x] with { W = 1f }; });
        edit(image);
        EachRow(static (pixels, stored) => { for (int x = 0; x < pixels.Length; x++) stored[x] = pixels[x] with { W = stored[x].W }; });
        void EachRow(Action<Span<Vector4>, Span<Vector4>> copy) =>
            image.ProcessPixelRows(rows => { for (int y = 0; y < rows.Height; y++) copy(MemoryMarshal.Cast<RgbaVector, Vector4>(rows.GetRowSpan(y)), view.Span.GetRowSpan(y)); });
    }).Run();

    public T Sample<T>((double X, double Y) point, (WrapMode Across, WrapMode Down) wrap, T sum, Func<T, Vector4, double, T> add) =>
        ((ReadOnlySpan2D<Vector4>)View.Span).Sample(point, wrap, sum, add, Some((Origin, Extent)));

    public PixelFrame Downscaled(PixelExtent bound) =>
        Size.Fitted(bound) switch {
            var fitted => new(Point.Empty, fitted, fitted, block =>
                ImageProcessing.Resize(Block, Size.Width, Size.Height, block, fitted.Width, fitted.Height, 4, ResizeFilter.Triangle, alphaChannel: 3)),
        };

    public static IO<(PixelFrame Frame, Option<Chromaticities> Primaries)> Read(FileInfo file, string layer) =>
        from result in IO.liftVAsync(env => ExrFile.LoadFromFileAsync(file.FullName, cancellationToken: env.Token))
        from image in IO.lift(Optional(result.Value).ToFin(new Expected($"EXR codec refused reading {file.FullName} with {result.Status}", 0, Optional(result.Error).Map(Error.New))))
        from read in IO.lift(() => Layer(toSeq(image.Parts), layer))
        select read;

    private static Fin<(PixelFrame Frame, Option<Chromaticities> Primaries)> Layer(Seq<Part> parts, string layer) =>
        from part in (parts.Find(candidate => string.Equals(candidate.Header.Name, layer, StringComparison.Ordinal)) | parts.Find(static candidate => candidate.Header.Name.Length is 0))
            .ToFin(Error.New($"EXR file holds no part for layer '{layer}'"))
        from header in (
                from samples in part.Header.IsDeep
                     ? Fin.Fail<InterleavedFloatImage>(Error.New($"EXR part '{part.Header.Name}' holds deep samples"))
                     : layer.Length is 0 && PartConversion.IsLuminanceChroma(part) ? PartConversion.LuminanceChromaToRgbaFloat(part) : PartConversion.ToInterleavedFloat(part)
                from layout in Components(samples.ChannelNames, layer)
                select (Samples: samples, Layout: layout),
                PixelExtent.Validated(int.CreateSaturating(part.Width), int.CreateSaturating(part.Height)),
                PixelExtent.Validated(int.CreateSaturating(part.Header.DisplayWindow.Width), int.CreateSaturating(part.Header.DisplayWindow.Height)))
            .Apply(static (decoded, size, extent) => (decoded.Samples, decoded.Layout, Size: size, Extent: extent))
            .As()
        select (new PixelFrame(
            new Point(part.Header.DataWindow.MinX - part.Header.DisplayWindow.MinX, part.Header.DataWindow.MinY - part.Header.DisplayWindow.MinY),
            header.Size,
            header.Extent,
            block => {
                Span<Vector4> pixels = MemoryMarshal.Cast<float, Vector4>(block.AsSpan());
                ReadOnlySpan2D<float> source = header.Samples.Data.AsSpan2D(pixels.Length, header.Samples.Channels);
                for (int index = 0; index < pixels.Length; index++) {
                    ReadOnlySpan<float> channels = source.GetRowSpan(index);
                    Vector4 pixel = Vector4.UnitW;
                    foreach ((int component, int channel) in header.Layout.Rows)
                        pixel[component] = channels[channel];
                    pixels[index] = (header.Layout.Associated, pixel.W > 0f) switch {
                        (false, _) => pixel,
                        (true, true) => new(pixel.AsVector3() / pixel.W, pixel.W),
                        (true, false) => new(Vector3.Zero, pixel.W),
                    };
                }
            }), Optional(part.Header.Chromaticities));

    private static Fin<((int Component, int Channel)[] Rows, bool Associated)> Components(IReadOnlyList<string> names, string layer) =>
        toSeq(
            (from family in Seq("RGBA", "XYZW", "UV")
             from entry in toSeq(names).Map(static (name, channel) => (Name: name, Channel: channel, Dot: name.LastIndexOf('.')))
             where entry.Name.Length == entry.Dot + 2 && entry.Name.AsSpan(0, int.Max(entry.Dot, 0)).SequenceEqual(layer)
             let component = family.IndexOf(entry.Name[^1], StringComparison.OrdinalIgnoreCase)
             where component >= 0
             select (Component: component, entry.Channel, Letter: entry.Name[^1])).DistinctBy(static row => row.Component)) switch {
                 [] => Error.New($"EXR part holds no channel of layer '{layer}'"),
                 var filled => (
                     (filled.Filter(static row => row.Component < 3) is [var lone]
                         ? filled + Seq(0, 1, 2).Filter(component => component != lone.Component).Map(component => (Component: component, lone.Channel, lone.Letter))
                         : filled).Map(static row => (row.Component, row.Channel)).ToArray(),
                     filled.Exists(static row => row is { Component: 3, Letter: 'A' or 'a' })),
             };
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class PixelSampling {
    public static Fin<TCount> GaussianTaps<TCount>(double sigma) where TCount : IBinaryInteger<TCount>, IMinMaxValue<TCount> => KernelTaps<TCount>(ERational.FromDouble(sigma).Multiply(3));

    public static Fin<TCount> KernelTaps<TCount>(ERational radius) where TCount : IBinaryInteger<TCount>, IMinMaxValue<TCount> =>
        radius.IsFinite && radius.CompareTo(0) >= 0 && radius.Numerator.DivRem(radius.Denominator) is [var whole, var rest]
        && new BigInteger(whole.Add(rest.IsZero ? 0 : 1).Multiply(2).Add(1).ToBytes(littleEndian: true)) is var taps && taps <= BigInteger.CreateTruncating(TCount.MaxValue)
            ? TCount.CreateTruncating(taps)
            : Error.New(string.Create(CultureInfo.InvariantCulture, $"Kernel radius {radius} has no tap count up to {TCount.MaxValue}"));

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
