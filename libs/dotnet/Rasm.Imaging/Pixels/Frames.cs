using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;
using CommunityToolkit.HighPerformance;
using Rasm.Imaging.ColorManagement;
using Rasm.Imaging.Output;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Metadata.Profiles.Cicp;
using SixLabors.ImageSharp.Metadata.Profiles.Icc;
using SixLabors.ImageSharp.PixelFormats;
using TinyEXR;

namespace Rasm.Imaging.Pixels;

// --- [MODELS] --------------------------------------------------------------------------
[ComplexValueObject]
[ValidationError<InvalidPixelValue>]
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

    static partial void ValidateFactoryArguments(ref InvalidPixelValue? validationError, ref int width, ref int height) =>
        validationError = width >= 1 && height >= 1 && 4L * width * height <= System.Array.MaxLength ? null : new InvalidPixelValue();
}

public sealed class PixelFrame {
    private readonly float[] block;

    public PixelFrame(Point origin, PixelExtent size, PixelExtent extent, Action<float[]> fill) {
        (Origin, Size, Extent, block) = (origin, size, extent, GC.AllocateUninitializedArray<float>(4 * size.Width * size.Height, pinned: true));
        fill(block);
    }

    public Point Origin { get; }
    public PixelExtent Size { get; }
    public PixelExtent Extent { get; }
    public ReadOnlySpan<float> Block => block;
    public nint Address => Marshal.UnsafeAddrOfPinnedArrayElement(block, 0);
    public Rectangle Window => new(Origin.X, Origin.Y, Size.Width, Size.Height);
    public bool Whole => Origin == Point.Empty && Size == Extent;
    public Memory2D<Vector4> View => block.AsMemory().Cast<float, Vector4>().AsMemory2D(Size.Height, Size.Width);

    public int Line(int index) => Extent.Height - 1 - Origin.Y - index;

    public Span<Vector4> Row(int line) => MemoryMarshal.Cast<float, Vector4>(block.AsSpan()).AsSpan2D(Size.Height, Size.Width).GetRowSpan(Line(line));

    public PixelFrame Downscaled(PixelExtent bound) =>
        Size.Fitted(bound) switch {
            var fitted => new(Point.Empty, fitted, fitted, block =>
                ImageProcessing.Resize(Block, Size.Width, Size.Height, block, fitted.Width, fitted.Height, 4, ResizeFilter.Triangle, alphaChannel: 3)),
        };

    public static IO<(PixelFrame Frame, Option<Gamut> Gamut)> Read(FileInfo file, string layer) =>
        from result in IO.liftVAsync(env => ExrFile.LoadFromFileAsync(file.FullName, cancellationToken: env.Token))
        from image in IO.lift(CodecRefused.Unless(result))
        from read in IO.lift(() => Layer(toSeq(image.Parts), layer))
        select read;

    public static IO<(PixelFrame Frame, ColorEncoding Encoding)> ReadPng(FileInfo file) =>
        (from stream in use(() => new FileStream(file.FullName, new FileStreamOptions { Mode = FileMode.Open, Access = FileAccess.Read, Options = FileOptions.Asynchronous }))
         from info in IO.liftAsync(env => PngDecoder.Instance.IdentifyAsync(Decoding(ColorProfileHandling.Preserve), stream, env.Token))
         let cicp = Optional(info.Metadata.CicpProfile)
         from encoding in IO.lift(Encoding(cicp))
         from _ in IO.lift(() => stream.Seek(0, SeekOrigin.Begin))
         from frame in (
                 from image in use(IO.liftAsync(env => PngDecoder.Instance.DecodeAsync<RgbaVector>(
                     Decoding(cicp.Match(Some: static _ => ColorProfileHandling.Preserve, None: static () => ColorProfileHandling.Convert)), stream, env.Token)))
                 from size in IO.lift(ExtentOf(image.Width, image.Height))
                 select new PixelFrame(Point.Empty, size, size, block => {
                     image.CopyPixelDataTo(MemoryMarshal.Cast<float, RgbaVector>(block.AsSpan()));
                     encoding.Decode(MemoryMarshal.Cast<float, Vector4>(block.AsSpan()));
                 }))
             .Bracket()
         select (frame, encoding))
            .Bracket()
            .Catch(
                static error => error.HasException<SixLabors.ImageSharp.ImageFormatException>() || error.HasException<InvalidIccProfileException>()
                    || error.HasException<NotSupportedException>() || error.HasException<IOException>() || error.HasException<UnauthorizedAccessException>(),
                static error => IO.fail<(PixelFrame Frame, ColorEncoding Encoding)>(new PngRefused(error)));

    private static Fin<PixelExtent> ExtentOf(int width, int height) =>
        PixelExtent.Validate(width, height, out PixelExtent extent) is { } error ? error : extent;

    private static Fin<(PixelFrame Frame, Option<Gamut> Gamut)> Layer(Seq<Part> parts, string layer) =>
        from part in (parts.Find(candidate => string.Equals(candidate.Header.Name, layer, StringComparison.Ordinal)) | parts.Find(static candidate => candidate.Header.Name.Length == 0)).ToFin(new LayerAbsent(layer))
        from _ in guard<Error>(!part.Header.IsDeep, new CodecRefused(ExrResult.Unsupported, None))
        let samples = layer.Length == 0 && PartConversion.IsLuminanceChroma(part) ? PartConversion.LuminanceChromaToRgbaFloat(part) : PartConversion.ToInterleavedFloat(part)
        from header in (
                Optional(part.Header.Chromaticities).Traverse(static stated => Gamut.Of(stated).ToFin(new GamutUnmatched(stated))).As().ToValidation(),
                ExtentOf(int.CreateSaturating(part.Header.DataWindow.Width), int.CreateSaturating(part.Header.DataWindow.Height)).ToValidation(),
                ExtentOf(int.CreateSaturating(part.Header.DisplayWindow.Width), int.CreateSaturating(part.Header.DisplayWindow.Height)).ToValidation(),
                Components(samples.ChannelNames, layer).ToValidation())
            .Apply(static (gamut, size, extent, layout) => (Gamut: gamut, Size: size, Extent: extent, Layout: layout))
            .As()
            .ToFin()
        select (new PixelFrame(
            new Point(part.Header.DataWindow.MinX - part.Header.DisplayWindow.MinX, part.Header.DataWindow.MinY - part.Header.DisplayWindow.MinY),
            header.Size,
            header.Extent,
            block => {
                Span<Vector4> pixels = MemoryMarshal.Cast<float, Vector4>(block.AsSpan());
                ReadOnlySpan2D<float> source = samples.Data.AsSpan2D(pixels.Length, samples.Channels);
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
            }), header.Gamut);

    private static Fin<((int Component, int Channel)[] Rows, bool Associated)> Components(IReadOnlyList<string> names, string layer) =>
        toSeq(
            (from family in Seq("RGBA", "XYZW", "UV")
             from entry in toSeq(names).Map(static (name, channel) => (Name: name, Channel: channel, Dot: name.LastIndexOf('.')))
             where entry.Name.Length == entry.Dot + 2 && entry.Name.AsSpan(0, int.Max(entry.Dot, 0)).SequenceEqual(layer)
             let component = family.IndexOf(entry.Name[^1], StringComparison.OrdinalIgnoreCase)
             where component >= 0
             select (Component: component, entry.Channel, Letter: entry.Name[^1])).DistinctBy(static row => row.Component)) switch {
                 [] => new LayerAbsent(layer),
                 var filled => (
                     (filled.Filter(static row => row.Component < 3) is [var lone]
                         ? filled + Seq(0, 1, 2).Filter(component => component != lone.Component).Map(component => (Component: component, lone.Channel, lone.Letter))
                         : filled).Map(static row => (row.Component, row.Channel)).ToArray(),
                     filled.Exists(static row => row is { Component: 3, Letter: 'A' or 'a' })),
             };

    private static Fin<ColorEncoding> Encoding(Option<CicpProfile> cicp) =>
        cicp.Match(
            Some: static chunk =>
                (Gamut.ByCicp.Value.Find(chunk.ColorPrimaries), ColorTags.Transfers.Find(row => row.Code == chunk.TransferCharacteristics))
                    .Apply(static (gamut, row) => new ColorEncoding(gamut, row.Transfer, row.White))
                    .As()
                    .Filter(_ => chunk.MatrixCoefficients == CicpMatrixCoefficients.Identity && chunk.FullRange)
                    .ToFin(new CicpUnsupported(chunk.ColorPrimaries, chunk.TransferCharacteristics, chunk.MatrixCoefficients, chunk.FullRange)),
            None: static () => new ColorEncoding(Gamut.StandardRgb, TransferCurve.Srgb, Nits.ReferenceWhite));

    private static DecoderOptions Decoding(ColorProfileHandling handling) =>
        new() { MaxFrames = 1, SegmentIntegrityHandling = SegmentIntegrityHandling.Strict, ColorProfileHandling = handling };
}

[ValueObject<string>]
[ValidationError<InvalidPixelValue>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ImagePath {
    static partial void ValidateFactoryArguments(ref InvalidPixelValue? validationError, ref string value) =>
        validationError = Path.IsPathFullyQualified(value) && ImageFile.Reads.ContainsKey(Path.GetExtension(value)) ? null : new InvalidPixelValue();
}

public sealed record ImageFile {
    private ImageFile(ImagePath path, PixelFrame frame) => (Path, Frame) = (path, frame);

    internal static FrozenDictionary<string, Func<FileInfo, IO<PixelFrame>>> Reads { get; } =
        new KeyValuePair<string, Func<FileInfo, IO<PixelFrame>>>[] {
            new(".exr", static file => PixelFrame.Read(file, "").Map(static read => read.Frame)),
            new(".png", static file => PixelFrame.ReadPng(file).Map(static read => read.Frame)),
        }.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

    public ImagePath Path { get; }
    public PixelFrame Frame { get; }

    public static IO<ImageFile> Load(ImagePath path) =>
        Reads[System.IO.Path.GetExtension(path)](new FileInfo(path)).Map(frame => new ImageFile(path, frame));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class PixelSampling {
    extension(ReadOnlySpan2D<Vector4> plane) {
        public Vector4 Sample(Vector2 point, (WrapMode Across, WrapMode Down) wrap) =>
            plane.Sample(point, wrap, Vector4.Zero, static (sum, pixel, weight) => sum + (pixel * (float)weight));

        public T Sample<T>(Vector2 point, (WrapMode Across, WrapMode Down) wrap, T sum, Func<T, Vector4, double, T> add) {
            (double x, double y) = (point.X - 0.5d, point.Y - 0.5d);
            (double left, double top) = (Math.Floor(x), Math.Floor(y));
            (double across, double down) = (x - left, y - top);
            for (int row = 0; row < 2; row++) {
                Option<int> wrappedRow = wrap.Down.Index(top + row, plane.Height);
                double vertical = row == 0 ? 1d - down : down;
                for (int column = 0; column < 2; column++) {
                    double weight = vertical * (column == 0 ? 1d - across : across);
                    sum = (wrappedRow, wrap.Across.Index(left + column, plane.Width)) switch {
                        ( { IsSome: true } atRow, { IsSome: true } atColumn) => add(sum, plane[(int)atRow, (int)atColumn], weight),
                        _ => sum,
                    };
                }
            }
            return sum;
        }
    }
}
