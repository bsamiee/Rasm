using System.Numerics;
using System.Text;
using BitMiracle.LibTiff.Classic;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;
using Rasm.Imaging.ColorManagement;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Quantization;
using Rasm.Imaging.Tone;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Metadata;
using SixLabors.ImageSharp.PixelFormats;

namespace Rasm.Imaging.Output;

// --- [MODELS] --------------------------------------------------------------------------
internal readonly record struct ImageSample(OutputDepth Depth, Func<Memory<byte>, PixelExtent, bool, ImageMetadata, Image> Wrap) {
    internal static ImageSample Bytes { get; } = new(OutputDepth.UInt8, static (block, size, alpha, metadata) =>
        alpha
            ? Image.WrapMemory<Rgba32>(Configuration.Default, block, size.Width, size.Height, metadata)
            : Image.WrapMemory<Rgb24>(Configuration.Default, block, size.Width, size.Height, metadata));
}

internal readonly record struct TiffSample(OutputDepth Depth, SampleFormat Format, Option<Predictor> Predictor);

[ValueObject<int>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidOutput>]
public readonly partial struct Quality : IMinMaxValue<Quality> {
    public static Quality MinValue { get; } = new(1);
    public static Quality MaxValue { get; } = new(100);

    static partial void ValidateFactoryArguments(ref InvalidOutput? validationError, ref int value) =>
        validationError = value >= MinValue._value && value <= MaxValue._value ? null : new InvalidOutput();
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record StillFormat {
    public sealed record PngRaster : StillFormat {
        private PngRaster(PixelFrame frame, ImageSample sample, Option<ColorEncoding> encoding, bool alpha) => (Frame, Sample, Encoding, Alpha) = (frame, sample, encoding, alpha);

        public PixelFrame Frame { get; }
        public Option<ColorEncoding> Encoding { get; }
        public bool Alpha { get; }
        internal ImageSample Sample { get; }

        public static Seq<string> Extensions => Seq(".png");
        public static Seq<OutputDepth> Depths => toSeq(OutputDepth.Items).Filter(static depth => Table(depth).IsSome);

        public static Fin<StillFormat> From(PixelFrame frame, OutputDepth depth, Option<ColorEncoding> encoding, bool alpha) =>
            Table(depth).Map(sample => (StillFormat)new PngRaster(frame, sample, encoding, alpha)).ToFin(new DepthRefused(depth, Depths));

        internal static Option<ImageSample> Table(OutputDepth depth) =>
            depth.Map<Option<ImageSample>>(
                uInt8: ImageSample.Bytes,
                uInt10: None,
                uInt12: None,
                uInt14: None,
                uInt16: new ImageSample(depth, static (block, size, alpha, metadata) =>
                    alpha
                        ? Image.WrapMemory<Rgba64>(Configuration.Default, block, size.Width, size.Height, metadata)
                        : Image.WrapMemory<Rgb48>(Configuration.Default, block, size.Width, size.Height, metadata)),
                float16: None,
                float32: None);
    }

    public sealed record JpegRaster(PixelFrame Frame, ColorEncoding Encoding, Quality Quality) : StillFormat {
        public static Seq<string> Extensions => Seq(".jpg", ".jpeg");
    }

    public sealed record WebpRaster(PixelFrame Frame, ColorEncoding Encoding, bool Alpha, WebpFileFormatType Format, Quality Quality) : StillFormat {
        public static Seq<string> Extensions => Seq(".webp");
    }

    public sealed record TiffRaster : StillFormat {
        private TiffRaster(PixelFrame frame, TiffSample sample, ColorEncoding encoding, bool alpha) => (Frame, Sample, Encoding, Alpha) = (frame, sample, encoding, alpha);

        public PixelFrame Frame { get; }
        public ColorEncoding Encoding { get; }
        public bool Alpha { get; }
        internal TiffSample Sample { get; }

        public static Seq<string> Extensions => Seq(".tif", ".tiff");
        public static Seq<OutputDepth> Depths => toSeq(OutputDepth.Items).Filter(static depth => Table(depth).IsSome);

        public static Fin<StillFormat> From(PixelFrame frame, OutputDepth depth, ColorEncoding encoding, bool alpha) =>
            Table(depth).Map(sample => (StillFormat)new TiffRaster(frame, sample, encoding, alpha)).ToFin(new DepthRefused(depth, Depths));

        internal static Option<TiffSample> Table(OutputDepth depth) =>
            depth.Map<Option<TiffSample>>(
                uInt8: new TiffSample(depth, SampleFormat.UINT, Predictor.HORIZONTAL),
                uInt10: new TiffSample(depth, SampleFormat.UINT, None),
                uInt12: new TiffSample(depth, SampleFormat.UINT, None),
                uInt14: new TiffSample(depth, SampleFormat.UINT, None),
                uInt16: new TiffSample(depth, SampleFormat.UINT, Predictor.HORIZONTAL),
                float16: new TiffSample(depth, SampleFormat.IEEEFP, Predictor.FLOATINGPOINT),
                float32: new TiffSample(depth, SampleFormat.IEEEFP, Predictor.FLOATINGPOINT));
    }

    public sealed record ExrRaster : StillFormat {
        private ExrRaster(PixelFrame beauty, ExrLayout layout, bool alpha, Option<PassSet> passes) => (Beauty, Layout, Alpha, Passes) = (beauty, layout, alpha, passes);

        public PixelFrame Beauty { get; }
        public ExrLayout Layout { get; }
        public bool Alpha { get; }
        public Option<PassSet> Passes { get; }

        public static Seq<string> Extensions => Seq(".exr");

        public static Validation<Error, StillFormat> From(PixelFrame beauty, ExrLayout layout, bool alpha, Option<PassSet> passes) =>
            passes.Traverse(set => set.Fits(beauty)).As().Map(_ => (StillFormat)new ExrRaster(beauty, layout, alpha, passes));
    }

    public sealed record Dng : StillFormat {
        private Dng(PixelFrame frame, Gamut gamut, Exposure exposure, Option<FocalLength> focalLength, Option<FNumber> aperture) =>
            (Frame, Gamut, Exposure, FocalLength, Aperture) = (frame, gamut, exposure, focalLength, aperture);

        public PixelFrame Frame { get; }
        public Gamut Gamut { get; }
        public Exposure Exposure { get; }
        public Option<FocalLength> FocalLength { get; }
        public Option<FNumber> Aperture { get; }

        public static Seq<string> Extensions => Seq(".dng");

        public static Validation<Error, StillFormat> From(PixelFrame frame, Gamut gamut, Exposure exposure, Option<FocalLength> focalLength, Option<FNumber> aperture) =>
            DngWriter.Bounded(frame.Size).Map(_ => (StillFormat)new Dng(frame, gamut, exposure, focalLength, aperture));
    }

    public sealed record Document(LayeredDocument Content) : StillFormat;
}

[ValueObject<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
[ValidationError<InvalidOutput>]
public sealed partial class OutputPath {
    static partial void ValidateFactoryArguments(ref InvalidOutput? validationError, ref string value) =>
        validationError = Path.IsPathFullyQualified(value) && Path.GetFileName(value).Length > 0 ? null : new InvalidOutput();
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class StillWriter {
    internal const long ClassicBytes = (1L << 32) - (1L << 25);

    private static readonly short[] Unassociated = [(short)ExtraSample.UNASSALPHA];

    public static IO<Unit> Write(StillFormat still, FileMetadata metadata, OutputPath path) =>
        Commit(path, stream => still.Switch(
            (Metadata: metadata, Stream: stream),
            pngRaster: static (save, png) => Encoded(png.Frame, png.Sample, png.Alpha, save.Metadata.Png(png.Encoding), new PngEncoder {
                BitDepth = (PngBitDepth)png.Sample.Depth.Bits, ColorType = png.Alpha ? PngColorType.RgbWithAlpha : PngColorType.Rgb,
            }, save.Stream),
            jpegRaster: static (save, jpeg) => Encoded(jpeg.Frame, ImageSample.Bytes, alpha: false, save.Metadata.Png(Some(jpeg.Encoding)), new JpegEncoder { Quality = jpeg.Quality }, save.Stream),
            webpRaster: static (save, webp) => Encoded(webp.Frame, ImageSample.Bytes, webp.Alpha, save.Metadata.Png(Some(webp.Encoding)), new WebpEncoder { FileFormat = webp.Format, Quality = webp.Quality }, save.Stream),
            tiffRaster: static (save, tiff) => Scanlines(save.Stream, "w", [], Tagged(tiff.Frame.Size, tiff.Sample, tiff.Alpha, tiff.Encoding, save.Metadata, []), tiff.Frame.Size.Height, Rows(tiff.Frame, tiff.Sample.Depth, tiff.Alpha)),
            exrRaster: static (save, exr) => ExrParts.Write(exr, save.Metadata, save.Stream),
            dng: static (save, dng) => DngWriter.Write(dng, save.Metadata, save.Stream),
            document: static (save, document) => document.Content.Write(save.Metadata, save.Stream)));

    internal static void Associate(ReadOnlySpan<Vector4> row, Span<Vector4> into) {
        for (int x = 0; x < row.Length; x++)
            into[x] = new Vector4(row[x].AsVector3() * row[x].W, row[x].W);
    }

    internal static Action<int, Span<byte>> Rows(PixelFrame frame, OutputDepth depth, bool alpha) =>
        alpha
            ? (y, destination) => depth.Encode(frame.View.Span.GetRowSpan(y), alpha: true, destination)
            : (y, destination) => {
                using SpanOwner<Vector4> scratch = SpanOwner<Vector4>.Allocate(frame.Size.Width);
                Associate(frame.View.Span.GetRowSpan(y), scratch.Span);
                depth.Encode(scratch.Span, alpha: false, destination);
            };

    internal static void Planar(OutputDepth depth, PixelExtent size, int lanes, Action<int, Span<Vector4>> source, Span<byte> planes) {
        int bytes = depth.SampleBytes;
        using SpanOwner<Vector4> scratch = SpanOwner<Vector4>.Allocate(size.Width);
        using SpanOwner<byte> encoded = SpanOwner<byte>.Allocate(depth.RowBytes(size.Width, alpha: true));
        Span2D<byte> planar = planes.AsSpan2D(lanes * size.Height, size.Width * bytes);
        for (int y = 0; y < size.Height; y++) {
            source(y, scratch.Span);
            depth.Encode(scratch.Span, alpha: true, encoded.Span);
            for (int lane = 0; lane < lanes; lane++)
                encoded.Span.AsSpan2D(size.Width, 4 * bytes).Slice(0, lane * bytes, size.Width, bytes).CopyTo(planar.GetRowSpan((lane * size.Height) + y));
        }
    }

    internal static Func<Tiff, Seq<(TiffTag Tag, object[] Values)>> Tagged(PixelExtent size, TiffSample sample, bool alpha, ColorEncoding encoding, FileMetadata metadata, Seq<(TiffTag Tag, object[] Values)> extra) =>
        open => [
            (TiffTag.IMAGEWIDTH, [size.Width]),
            (TiffTag.IMAGELENGTH, [size.Height]),
            (TiffTag.BITSPERSAMPLE, [sample.Depth.Bits]),
            (TiffTag.SAMPLESPERPIXEL, [alpha ? 4 : 3]),
            (TiffTag.SAMPLEFORMAT, [sample.Format]),
            (TiffTag.PHOTOMETRIC, [Photometric.RGB]),
            (TiffTag.PLANARCONFIG, [PlanarConfig.CONTIG]),
            (TiffTag.COMPRESSION, [Compression.ADOBE_DEFLATE]),
            .. sample.Predictor.Map(static predictor => (TiffTag.PREDICTOR, new object[] { predictor })).ToSeq(),
            .. (alpha ? Some((TiffTag.EXTRASAMPLES, new object[] { 1, Unassociated })) : None).ToSeq(),
            (TiffTag.ORIENTATION, [Orientation.TOPLEFT]),
            .. ColorTags.Icc(encoding, metadata.Created).Map(static bytes => (TiffTag.ICCPROFILE, new object[] { bytes.Length, bytes })).ToSeq(),
            .. metadata.Tiff(),
            .. extra,
            (TiffTag.ROWSPERSTRIP, [open.DefaultStripSize(0)]),
        ];

    internal static IO<Unit> Scanlines(FileStream stream, string mode, Seq<TiffFieldInfo> custom, Func<Tiff, Seq<(TiffTag Tag, object[] Values)>> fields, int height, Action<int, Span<byte>> row) =>
        (from opened in IO.lift(() => Optional(Tiff.ClientOpen(stream.Name, mode, stream, new TiffStream())).ToFin(new TiffRefused(new TiffCall.Open())))
         from tiff in use(() => opened)
         from merged in IO.lift(() => tiff.MergeFieldInfo([.. custom], custom.Count))
         from fielded in fields(tiff).TraverseM(field => Answered(() => tiff.SetField(field.Tag, field.Values), new TiffCall.Field(field.Tag))).As()
         from line in use(() => MemoryOwner<byte>.Allocate(tiff.ScanlineSize()))
         from written in toSeq(Range(0, height)).TraverseM(y => Answered(() => {
             row(y, line.Span);
             ArraySegment<byte> segment = line.DangerousGetArray();
             return tiff.WriteScanline(segment.Array, segment.Offset, y, 0);
         }, new TiffCall.Scanline(y))).As()
         from flushed in Answered(tiff.Flush, new TiffCall.Flush())
         select unit).Bracket();

    internal static IO<Unit> Commit(OutputPath path, Func<FileStream, IO<Unit>> write) =>
        IO.pure((string)path + ".partial")
            .Bracket(
                Use: temporary =>
                    from folder in IO.lift(() => Directory.CreateDirectory(Path.GetDirectoryName((string)path)!))
                    from encoded in use(() => new FileStream(temporary, new FileStreamOptions { Mode = FileMode.Create, Access = FileAccess.ReadWrite, Share = FileShare.None, Options = FileOptions.Asynchronous }))
                        .Bind(write)
                        .Bracket()
                    from moved in IO.lift(() => File.Move(temporary, path, overwrite: true))
                    select unit,
                Fin: static temporary => IO.lift(() => File.Delete(temporary)))
            .Catch(static error => error.HasException<IOException>() || error.HasException<UnauthorizedAccessException>(), error => IO.fail<Unit>(new WriteRefused(path, error)));

    internal static IO<Unit> Commit(OutputPath path, string text) =>
        Commit(path, stream =>
            from token in cancelToken
            from written in IO.liftAsync(async () => {
                await stream.WriteAsync(Encoding.UTF8.GetBytes(text), token).ConfigureAwait(false);
                return unit;
            })
            select written);

    private static IO<Unit> Answered(Func<bool> call, TiffCall refused) => IO.lift(Fin<Unit> () => call() ? unit : new TiffRefused(refused));

    private static IO<Unit> Encoded(PixelFrame frame, ImageSample sample, bool alpha, ImageMetadata metadata, IImageEncoder encoder, FileStream stream) =>
        sample.Depth.RowBytes(frame.Size.Width, alpha) switch {
            var stride =>
                from block in use(() => MemoryOwner<byte>.Allocate(frame.Size.Height * stride))
                from filled in IO.lift(() => {
                    Action<int, Span<byte>> fill = Rows(frame, sample.Depth, alpha);
                    Span2D<byte> rows = block.Span.AsSpan2D(frame.Size.Height, stride);
                    for (int y = 0; y < rows.Height; y++)
                        fill(y, rows.GetRowSpan(y));
                })
                from image in use(() => sample.Wrap(block.Memory, frame.Size, alpha, metadata))
                from token in cancelToken
                from saved in IO.liftAsync(async () => {
                    await image.SaveAsync(stream, encoder, token).ConfigureAwait(false);
                    return unit;
                })
                select unit,
        };
}
