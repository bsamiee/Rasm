using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using BitMiracle.LibTiff.Classic;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;
using Rasm.Imaging.Color;
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

namespace Rasm.Imaging.Files;

// --- [MODELS] --------------------------------------------------------------------------
internal readonly record struct ImageSample(OutputDepth Depth, Func<PixelFrame, bool, FileMetadata, Option<ColorEncoding>, IImageEncoder, Stream, IO<Unit>> Write) {
    internal static ImageSample Bytes { get; } = Of<Rgba32, Rgb24>(OutputDepth.UInt8);

    internal static ImageSample Of<TAlpha, TOpaque>(OutputDepth depth)
        where TAlpha : unmanaged, IPixel<TAlpha>
        where TOpaque : unmanaged, IPixel<TOpaque> =>
        new(depth, (frame, alpha, metadata, encoding, encoder, stream) => alpha
            ? StillWriter.Encoded<TAlpha>(frame, depth, alpha, metadata, encoding, encoder, stream)
            : StillWriter.Encoded<TOpaque>(frame, depth, alpha, metadata, encoding, encoder, stream));
}

internal readonly record struct TiffSample(OutputDepth Depth, SampleFormat Format, Option<Predictor> Predictor);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record StillFormat {
    public sealed record PngRaster : StillFormat {
        private PngRaster(PixelFrame frame, ImageSample sample, Option<ColorEncoding> encoding, bool alpha) => (Frame, Sample, Encoding, Alpha) = (frame, sample, encoding, alpha);

        public PixelFrame Frame { get; }
        public Option<ColorEncoding> Encoding { get; }
        public bool Alpha { get; }
        internal ImageSample Sample { get; }

        public static IterableNE<string> Extensions => IterableNE.create(".png");
        public static Seq<OutputDepth> Depths => OutputDepth.Where(Table);

        public static Fin<StillFormat> From(PixelFrame frame, OutputDepth depth, Option<ColorEncoding> encoding, bool alpha) =>
            OutputDepth.Sampled("PNG", depth, Table).Map(sample => (StillFormat)new PngRaster(frame, sample, encoding, alpha));

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

        public static IterableNE<string> Extensions => IterableNE.create(".jpg", ".jpeg");

        public static StillFormat From(PixelFrame frame, Option<ColorEncoding> encoding, int quality) =>
            new JpegRaster(frame, encoding, new JpegEncoder { Quality = quality });
    }

    public sealed record WebpRaster(PixelFrame Frame, ColorEncoding Encoding, bool Alpha, WebpFileFormatType Format, int Quality) : StillFormat {
        public static IterableNE<string> Extensions => IterableNE.create(".webp");
    }

    public sealed record BmpRaster(PixelFrame Frame, bool Alpha) : StillFormat {
        public static IterableNE<string> Extensions => IterableNE.create(".bmp");
    }

    public sealed record TgaRaster(PixelFrame Frame, bool Alpha) : StillFormat {
        public static IterableNE<string> Extensions => IterableNE.create(".tga");
    }

    public sealed record TiffRaster : StillFormat {
        private TiffRaster(PixelFrame frame, TiffSample sample, Option<ColorEncoding> encoding, bool alpha) => (Frame, Sample, Encoding, Alpha) = (frame, sample, encoding, alpha);

        public PixelFrame Frame { get; }
        public Option<ColorEncoding> Encoding { get; }
        public bool Alpha { get; }
        internal TiffSample Sample { get; }

        public static IterableNE<string> Extensions => IterableNE.create(".tif", ".tiff");
        public static Seq<OutputDepth> Depths => OutputDepth.Where(Table);

        public static Fin<StillFormat> From(PixelFrame frame, OutputDepth depth, Option<ColorEncoding> encoding, bool alpha) =>
            from sample in OutputDepth.Sampled("TIFF", depth, Table)
            from _ in sample.Depth.Buffered("TIFF", frame.Size.Width, alpha)
            select (StillFormat)new TiffRaster(frame, sample, encoding, alpha);

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

        public static IterableNE<string> Extensions => IterableNE.create(".exr");

        public static Fin<StillFormat> From(PixelFrame beauty, ExrLayout layout, bool alpha, Option<PassSet> passes) =>
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

        public static IterableNE<string> Extensions => IterableNE.create(".dng");

        public static Fin<StillFormat> From(PixelFrame frame, Gamut gamut, Option<double> exposure, Option<Length> focalLength, Option<float> aperture) {
            static Fin<float> Rational(TiffTag tag, float value, int minimum) =>
                value >= minimum && value < (double)int.MaxValue
                    ? value
                    : Error.New(string.Create(CultureInfo.InvariantCulture, $"DNG field {tag} value {value} lies outside [{minimum}, {int.MaxValue})"));
            return (
                    DngWriter.Bounded(frame.Size),
                    toSeq(gamut.FromXyz.ToRowMajorArray()).Traverse(static value => Rational(TiffTag.COLORMATRIX1, (float)value, int.MinValue)).As(),
                    exposure.Traverse(static gain => Rational(TiffTag.BASELINEEXPOSURE, (float)gain, int.MinValue)).As(),
                    focalLength.Traverse(static focal => Rational(TiffTag.EXIF_FOCALLENGTH, (float)focal.Millimeters, 0)).As(),
                    aperture.Traverse(static stop => Rational(TiffTag.EXIF_FNUMBER, stop, 0)).As())
                .Apply((_, matrix, gain, focal, stop) => (StillFormat)new Dng(frame, matrix, gain, focal, stop))
                .As();
        }
    }

    public sealed record Document(LayeredDocument Content) : StillFormat;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class StillWriter {
    private static readonly short[] Unassociated = [(short)ExtraSample.UNASSALPHA];

    private sealed class BorrowedTiffStream : TiffStream {
        public override void Close(object clientData) { }
    }

    public static IO<Unit> Encode(StillFormat still, FileMetadata metadata, Stream stream) =>
        still.Switch(
            (Metadata: metadata, Stream: stream),
            pngRaster: static (save, png) => png.Sample.Write(png.Frame, png.Alpha, save.Metadata, png.Encoding, new PngEncoder {
                BitDepth = (PngBitDepth)png.Sample.Depth.Bits, ColorType = png.Alpha ? PngColorType.RgbWithAlpha : PngColorType.Rgb,
            }, save.Stream),
            jpegRaster: static (save, jpeg) => ImageSample.Bytes.Write(jpeg.Frame, false, save.Metadata, jpeg.Encoding, jpeg.Encoder, save.Stream),
            webpRaster: static (save, webp) => ImageSample.Bytes.Write(webp.Frame, webp.Alpha, save.Metadata, Some(webp.Encoding), new WebpEncoder { FileFormat = webp.Format, Quality = webp.Quality }, save.Stream),
            bmpRaster: static (save, bmp) => ImageSample.Bytes.Write(bmp.Frame, bmp.Alpha, save.Metadata, None, new BmpEncoder {
                BitsPerPixel = bmp.Alpha ? BmpBitsPerPixel.Bit32 : BmpBitsPerPixel.Bit24, SupportTransparency = bmp.Alpha,
            }, save.Stream),
            tgaRaster: static (save, tga) => ImageSample.Bytes.Write(tga.Frame, tga.Alpha, save.Metadata, None, new TgaEncoder {
                BitsPerPixel = tga.Alpha ? TgaBitsPerPixel.Bit32 : TgaBitsPerPixel.Bit24,
            }, save.Stream),
            tiffRaster: static (save, tiff) => Scanlines(save.Stream, "w", [], Tagged(tiff.Frame.Size, tiff.Sample, tiff.Alpha, tiff.Encoding, save.Metadata, []), tiff.Frame.Size.Height, Rows(tiff.Frame, tiff.Sample.Depth, tiff.Alpha)),
            exrRaster: static (save, exr) => ExrParts.Write(exr, save.Metadata, save.Stream),
            dng: static (save, dng) => DngWriter.Write(dng, save.Metadata, save.Stream),
            document: static (save, document) => document.Content.Write(save.Metadata, save.Stream));

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

    internal static byte[][] Planar(OutputDepth depth, PixelExtent size, int channels, Action<int, Span<Vector4>> source) {
        int bytes = depth.SampleBytes;
        int chunk = int.Min(size.Width, System.Array.MaxLength / Unsafe.SizeOf<Vector4>());
        byte[][] planes = new byte[channels][];
        for (int channel = 0; channel < channels; channel++)
            planes[channel] = new byte[(int)((long)size.Width * size.Height * bytes)];
        using SpanOwner<Vector4> scratch = SpanOwner<Vector4>.Allocate(size.Width);
        using SpanOwner<byte> encoded = SpanOwner<byte>.Allocate((int)depth.RowBytes(chunk, alpha: true));
        for (int y = 0; y < size.Height; y++) {
            source(y, scratch.Span);
            for (int x = 0; x < size.Width; x += chunk) {
                int count = int.Min(chunk, size.Width - x);
                Span<byte> row = encoded.Span[..(int)depth.RowBytes(count, alpha: true)];
                depth.Encode(scratch.Span.Slice(x, count), alpha: true, row);
                for (int channel = 0; channel < channels; channel++)
                    row.AsSpan2D(count, 4 * bytes).Slice(0, channel * bytes, count, bytes)
                        .CopyTo(planes[channel].AsSpan(((y * size.Width) + x) * bytes, count * bytes));
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
            .. encoding.Bind(held => held.Profile(metadata.Created)).Map(static bytes => (TiffTag.ICCPROFILE, new object[] { bytes.Length, bytes })).ToSeq(),
            .. metadata.Tiff(),
            .. extra,
            (TiffTag.ROWSPERSTRIP, [open.DefaultStripSize(0)]),
        ];

    internal static IO<Unit> Scanlines(Stream stream, string mode, Seq<TiffFieldInfo> custom, Func<Tiff, Seq<(TiffTag Tag, object[] Values)>> fields, int height, Action<int, Span<byte>> row) {
        Fin<Unit> Encode(Tiff tiff, CancellationToken token) {
            tiff.MergeFieldInfo([.. custom], custom.Count);
            foreach ((TiffTag tag, object[] values) in fields(tiff))
                if (!tiff.SetField(tag, values))
                    return Error.New($"TIFF writer refused field {tag}");
            using MemoryOwner<byte> line = MemoryOwner<byte>.Allocate(tiff.ScanlineSize());
            ArraySegment<byte> segment = line.DangerousGetArray();
            for (int y = 0; y < height; y++) {
                token.ThrowIfCancellationRequested();
                row(y, line.Span);
                if (!tiff.WriteScanline(segment.Array, segment.Offset, y, 0))
                    return Error.New(string.Create(CultureInfo.InvariantCulture, $"TIFF writer refused scanline {y}"));
            }
            return unit;
        }
        return IO.lift(environment => Optional(Tiff.ClientOpen(nameof(Tiff), mode, stream, new BorrowedTiffStream())).ToFin(Error.New("TIFF writer refused to open its stream")).Bind(tiff => {
            Fin<Unit> encoded = Try.lift(() => Encode(tiff, environment.Token)).Run();
            Fin<Unit> flushed = Try.lift(Fin<Unit> () => tiff.Flush() ? unit : Error.New("TIFF writer refused to flush its stream")).Run();
            Fin<Unit> closed = Try.lift(() => { tiff.Dispose(); return unit; }).Run();
            return (encoded, flushed, closed).Apply(static (_, _, _) => unit).As();
        }));
    }

    internal static IO<Unit> Encoded<TPixel>(PixelFrame frame, OutputDepth depth, bool alpha, FileMetadata metadata, Option<ColorEncoding> encoding, IImageEncoder encoder, Stream stream)
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
            metadata.Into(image.Metadata, encoding);
            image.ProcessPixelRows(rows => {
                for (int y = 0; y < rows.Height; y++)
                    Fill(y, rows.GetRowSpan(y));
            });
            await image.SaveAsync(stream, encoder, environment.Token).ConfigureAwait(false);
            return unit;
        });
}
