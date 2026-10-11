using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using BitMiracle.LibTiff.Classic;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;
using Mono.Unix;
using Mono.Unix.Native;
using Rasm.Imaging.ColorManagement;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Quantization;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Tga;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rasm.Imaging.Output;

// --- [MODELS] --------------------------------------------------------------------------
internal readonly record struct ImageSample(OutputDepth Depth, Func<PixelFrame, bool, FileMetadata, Option<ColorEncoding>, IImageEncoder, FileStream, IO<Unit>> Write) {
    internal static ImageSample Bytes { get; } = Of<Rgba32, Rgb24>(OutputDepth.UInt8);

    internal static ImageSample Of<TAlpha, TOpaque>(OutputDepth depth)
        where TAlpha : unmanaged, IPixel<TAlpha>
        where TOpaque : unmanaged, IPixel<TOpaque> =>
        new(depth, (frame, alpha, metadata, encoding, encoder, stream) => alpha
            ? StillWriter.Encoded<TAlpha>(frame, depth, alpha, metadata, encoding, encoder, stream)
            : StillWriter.Encoded<TOpaque>(frame, depth, alpha, metadata, encoding, encoder, stream));
}

internal readonly record struct TiffSample(OutputDepth Depth, SampleFormat Format, Option<Predictor> Predictor);

[ValueObject<int>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidOutput>]
public readonly partial struct Quality : IMinMaxValue<Quality> {
    public static Quality MinValue { get; } = new(0);
    public static Quality MaxValue { get; } = new(100);
    public static Quality Default { get; } = new(90);

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
                uInt16: ImageSample.Of<Rgba64, Rgb48>(depth),
                float16: None,
                float32: None);
    }

    public sealed record JpegRaster : StillFormat {
        private JpegRaster(PixelFrame frame, Option<ColorEncoding> encoding, JpegEncoder encoder) => (Frame, Encoding, Encoder) = (frame, encoding, encoder);

        public PixelFrame Frame { get; }
        public Option<ColorEncoding> Encoding { get; }
        internal JpegEncoder Encoder { get; }

        public static Seq<string> Extensions => Seq(".jpg", ".jpeg");

        public static Fin<StillFormat> From(PixelFrame frame, Option<ColorEncoding> encoding, Quality quality) =>
            Try.lift(() => new JpegEncoder { Quality = quality }).Run()
                .MapFail(static error => error.HasException<ArgumentException>() ? new InvalidOutput() : error)
                .Map(encoder => (StillFormat)new JpegRaster(frame, encoding, encoder));
    }

    public sealed record WebpRaster(PixelFrame Frame, ColorEncoding Encoding, bool Alpha, WebpFileFormatType Format, Quality Quality) : StillFormat {
        public static Seq<string> Extensions => Seq(".webp");
    }

    public sealed record BmpRaster(PixelFrame Frame, bool Alpha) : StillFormat {
        public static Seq<string> Extensions => Seq(".bmp");
    }

    public sealed record TgaRaster(PixelFrame Frame, bool Alpha) : StillFormat {
        public static Seq<string> Extensions => Seq(".tga");
    }

    public sealed record TiffRaster : StillFormat {
        private TiffRaster(PixelFrame frame, TiffSample sample, Option<ColorEncoding> encoding, bool alpha) => (Frame, Sample, Encoding, Alpha) = (frame, sample, encoding, alpha);

        public PixelFrame Frame { get; }
        public Option<ColorEncoding> Encoding { get; }
        public bool Alpha { get; }
        internal TiffSample Sample { get; }

        public static Seq<string> Extensions => Seq(".tif", ".tiff");
        public static Seq<OutputDepth> Depths => toSeq(OutputDepth.Items).Filter(static depth => Table(depth).IsSome);

        public static Fin<StillFormat> From(PixelFrame frame, OutputDepth depth, Option<ColorEncoding> encoding, bool alpha) =>
            Table(depth).ToFin(new DepthRefused(depth, Depths)).Bind(sample =>
                sample.Depth.RowBytes(frame.Size.Width, alpha) switch {
                    var bytes when bytes <= System.Array.MaxLength => Fin.Succ<StillFormat>(new TiffRaster(frame, sample, encoding, alpha)),
                    var bytes => Fin.Fail<StillFormat>(new BufferBytesRefused(bytes)),
                });

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
        private Dng(PixelFrame frame, Seq<float> matrix, Option<float> exposure, Option<float> focalMillimeters, Option<float> aperture) =>
            (Frame, Matrix, Exposure, FocalMillimeters, Aperture) = (frame, matrix, exposure, focalMillimeters, aperture);

        public PixelFrame Frame { get; }
        internal Seq<float> Matrix { get; }
        public Option<float> Exposure { get; }
        public Option<float> FocalMillimeters { get; }
        public Option<float> Aperture { get; }

        public static Seq<string> Extensions => Seq(".dng");

        public static Validation<Error, StillFormat> From(PixelFrame frame, Gamut gamut, Option<double> exposure, Option<FocalLength> focalLength, Option<FNumber> aperture) {
            static Validation<Error, float> Rational(TiffTag tag, float value, int minimum) =>
                value >= minimum && value < (double)int.MaxValue
                    ? Validation.Success<Error, float>(value)
                    : Validation.Fail<Error, float>(new TiffRefused(new TiffCall.Field(tag)));
            return (
                    DngWriter.Bounded(frame.Size),
                    toSeq(gamut.FromXyz.ToRowMajorArray()).Traverse(static value => Rational(TiffTag.COLORMATRIX1, (float)value, int.MinValue)).As(),
                    exposure.Traverse(static gain => Rational(TiffTag.BASELINEEXPOSURE, (float)gain, int.MinValue)).As(),
                    focalLength.Traverse(static focal => Rational(TiffTag.EXIF_FOCALLENGTH, (float)Length.FromMeters((float)focal).Millimeters, 0)).As(),
                    aperture.Traverse(static stop => Rational(TiffTag.EXIF_FNUMBER, stop, 0)).As())
                .Apply((_, matrix, gain, focal, stop) => (StillFormat)new Dng(frame, matrix, gain, focal, stop))
                .As();
        }
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
    private static readonly short[] Unassociated = [(short)ExtraSample.UNASSALPHA];

    private sealed class BorrowedTiffStream : TiffStream {
        public override void Close(object clientData) { }
    }

    public static IO<Unit> Write(StillFormat still, FileMetadata metadata, OutputPath path) =>
        FilePublication.Write(path, overwrite: true, stream => still.Switch(
            (Metadata: metadata, Stream: stream),
            pngRaster: static (save, png) => png.Sample.Write(png.Frame, png.Alpha, save.Metadata, png.Encoding, new PngEncoder {
                BitDepth = (PngBitDepth)png.Sample.Depth.Bits, ColorType = png.Alpha ? PngColorType.RgbWithAlpha : PngColorType.Rgb,
            }, save.Stream),
            jpegRaster: static (save, jpeg) => ImageSample.Bytes.Write(jpeg.Frame, false, save.Metadata, jpeg.Encoding, jpeg.Encoder, save.Stream),
            webpRaster: static (save, webp) => ImageSample.Bytes.Write(webp.Frame, webp.Alpha, save.Metadata, Some(webp.Encoding), new WebpEncoder { FileFormat = webp.Format, Quality = webp.Quality }, save.Stream),
            bmpRaster: static (save, bmp) => ImageSample.Bytes.Write(bmp.Frame, true, save.Metadata, None, new BmpEncoder {
                BitsPerPixel = bmp.Alpha ? BmpBitsPerPixel.Bit32 : BmpBitsPerPixel.Bit24, SupportTransparency = bmp.Alpha,
            }, save.Stream),
            tgaRaster: static (save, tga) => ImageSample.Bytes.Write(tga.Frame, true, save.Metadata, None, new TgaEncoder {
                BitsPerPixel = tga.Alpha ? TgaBitsPerPixel.Bit32 : TgaBitsPerPixel.Bit24,
            }, save.Stream),
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

    internal static byte[][] Planar(OutputDepth depth, PixelExtent size, int lanes, Action<int, Span<Vector4>> source) {
        int bytes = depth.SampleBytes;
        int chunk = int.Min(size.Width, System.Array.MaxLength / Unsafe.SizeOf<Vector4>());
        byte[][] planes = new byte[lanes][];
        for (int lane = 0; lane < lanes; lane++)
            planes[lane] = new byte[(int)((long)size.Width * size.Height * bytes)];
        using SpanOwner<Vector4> scratch = SpanOwner<Vector4>.Allocate(size.Width);
        using SpanOwner<byte> encoded = SpanOwner<byte>.Allocate((int)depth.RowBytes(chunk, alpha: true));
        for (int y = 0; y < size.Height; y++) {
            source(y, scratch.Span);
            for (int x = 0; x < size.Width; x += chunk) {
                int count = int.Min(chunk, size.Width - x);
                Span<byte> row = encoded.Span[..(int)depth.RowBytes(count, alpha: true)];
                depth.Encode(scratch.Span.Slice(x, count), alpha: true, row);
                for (int lane = 0; lane < lanes; lane++)
                    row.AsSpan2D(count, 4 * bytes).Slice(0, lane * bytes, count, bytes)
                        .CopyTo(planes[lane].AsSpan(((y * size.Width) + x) * bytes, count * bytes));
            }
        }
        return planes;
    }

    internal static Func<Tiff, Seq<(TiffTag Tag, object[] Values)>> Tagged(PixelExtent size, TiffSample sample, bool alpha, Option<ColorEncoding> encoding, FileMetadata metadata, Seq<(TiffTag Tag, object[] Values)> extra) =>
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
            .. encoding.Bind(held => ColorTags.Icc(held, metadata.Created)).Map(static bytes => (TiffTag.ICCPROFILE, new object[] { bytes.Length, bytes })).ToSeq(),
            .. metadata.Tiff(),
            .. extra,
            (TiffTag.ROWSPERSTRIP, [open.DefaultStripSize(0)]),
        ];

    internal static IO<Unit> Scanlines(FileStream stream, string mode, Seq<TiffFieldInfo> custom, Func<Tiff, Seq<(TiffTag Tag, object[] Values)>> fields, int height, Action<int, Span<byte>> row) {
        Fin<Unit> Encode(Tiff tiff, CancellationToken token) {
            tiff.MergeFieldInfo([.. custom], custom.Count);
            foreach ((TiffTag tag, object[] values) in fields(tiff))
                if (!tiff.SetField(tag, values))
                    return Fin.Fail<Unit>(new TiffRefused(new TiffCall.Field(tag)));
            using MemoryOwner<byte> line = MemoryOwner<byte>.Allocate(tiff.ScanlineSize());
            ArraySegment<byte> segment = line.DangerousGetArray();
            for (int y = 0; y < height; y++) {
                token.ThrowIfCancellationRequested();
                row(y, line.Span);
                if (!tiff.WriteScanline(segment.Array, segment.Offset, y, 0))
                    return Fin.Fail<Unit>(new TiffRefused(new TiffCall.Scanline(y)));
            }
            return Fin.Succ(unit);
        }
        return IO.lift(environment => Optional(Tiff.ClientOpen(stream.Name, mode, stream, new BorrowedTiffStream())).ToFin(new TiffRefused(new TiffCall.Open())).Bind(tiff => {
            Fin<Unit> encoded = Try.lift(() => Encode(tiff, environment.Token)).Run();
            Fin<Unit> flushed = Try.lift(Fin<Unit> () => tiff.Flush() ? unit : new TiffRefused(new TiffCall.Flush())).Run();
            Fin<Unit> closed = Try.lift(() => { tiff.Dispose(); return unit; }).Run();
            return (encoded.ToValidation(), flushed.ToValidation(), closed.ToValidation()).Apply(static (_, _, _) => unit).As().ToFin();
        }));
    }

    internal static IO<Unit> Encoded<TPixel>(PixelFrame frame, OutputDepth depth, bool alpha, FileMetadata metadata, Option<ColorEncoding> encoding, IImageEncoder encoder, FileStream stream)
        where TPixel : unmanaged, IPixel<TPixel> =>
        IO.liftAsync(async environment => {
            void Fill(int y, Span<TPixel> destination) {
                environment.Token.ThrowIfCancellationRequested();
                ReadOnlySpan<Vector4> source = frame.View.Span.GetRowSpan(y);
                for (int x = 0; x < destination.Length;) {
                    int count = int.Min(destination.Length - x, System.Array.MaxLength / int.Max(Unsafe.SizeOf<TPixel>(), Unsafe.SizeOf<Vector4>()));
                    Span<byte> into = MemoryMarshal.AsBytes(destination.Slice(x, count));
                    if (alpha)
                        depth.Encode(source.Slice(x, count), alpha: true, into);
                    else {
                        using SpanOwner<Vector4> scratch = SpanOwner<Vector4>.Allocate(count);
                        Associate(source.Slice(x, count), scratch.Span);
                        depth.Encode(scratch.Span, alpha: false, into);
                    }
                    x += count;
                }
            }
            using Image<TPixel> image = new(frame.Size.Width, frame.Size.Height);
            metadata.Png(image.Metadata, encoding);
            image.ProcessPixelRows(rows => {
                for (int y = 0; y < rows.Height; y++)
                    Fill(y, rows.GetRowSpan(y));
            });
            await image.SaveAsync(stream, encoder, environment.Token).ConfigureAwait(false);
            return unit;
        });
}

public static class FilePublication {
    public static IO<Unit> Write(OutputPath path, bool overwrite, Func<FileStream, IO<Unit>> write) {
        const int FileExists = unchecked((int)0x80070050);
        const int AlreadyExists = unchecked((int)0x800700B7);
        Error Refused(Error error) => error switch {
            ManyErrors many => Error.Many(many.Errors.Map(Refused)),
            _ => error.HasException<IOException>() || error.HasException<UnauthorizedAccessException>() ? new WriteRefused(path, error) : error,
        };
        return
            from environment in IO.lift(static environment => environment)
            from written in (
                from stream in IO.lift(() => {
                    string folder = Path.GetDirectoryName((string)path)!;
                    _ = Directory.CreateDirectory(folder);
                    return new FileStream(Path.Combine(folder, $"{Path.GetRandomFileName()}.tmp"), new FileStreamOptions {
                        Mode = FileMode.CreateNew, Access = FileAccess.ReadWrite, Share = FileShare.None, Options = FileOptions.Asynchronous,
                    });
                }).MapFail(Refused)
                from encoded in Seq<K<IO, Unit>>(
                    IO.liftVAsync(() => write(stream).Bracket().RunAsync(environment)).MapFail(Refused),
                    IO.liftAsync(async () => {
                        await stream.DisposeAsync().ConfigureAwait(false);
                        return unit;
                    }).MapFail(Refused)).Fails().As()
                from published in Seq<K<IO, Unit>>(
                    encoded.IsEmpty
                        ? IO.lift(() => {
                            environment.Token.ThrowIfCancellationRequested();
                            if (overwrite || OperatingSystem.IsWindows())
                                File.Move(stream.Name, path, overwrite);
                            else
                                UnixMarshal.ThrowExceptionForLastErrorIf(Syscall.link(stream.Name, path));
                            return unit;
                        }).MapFail(error => !overwrite && error.Exception.Case is UnixIOException { ErrorCode: Errno.EEXIST } or IOException { HResult: FileExists or AlreadyExists }
                            ? new DestinationTaken(path)
                            : Refused(error))
                        : IO.fail<Unit>(Error.Many(encoded)),
                    IO.lift(() => File.Delete(stream.Name)).MapFail(Refused)).Fails().As()
                from result in IO.lift(guard<Error>(published.IsEmpty, () => Error.Many(published)).ToFin())
                select result).Uninterruptible()
            select written;
    }

    public static IO<Unit> Write(OutputPath path, string text, bool overwrite = true) =>
        Write(path, overwrite, stream => IO.liftAsync(async environment => {
            await stream.WriteAsync(Encoding.UTF8.GetBytes(text), environment.Token).ConfigureAwait(false);
            return unit;
        }));
}
