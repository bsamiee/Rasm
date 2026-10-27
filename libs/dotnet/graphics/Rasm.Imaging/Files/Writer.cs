using System.Globalization;
using System.Numerics;
using System.Runtime.InteropServices;
using BitMiracle.LibTiff.Classic;
using CommunityToolkit.HighPerformance.Buffers;
using Rasm.Imaging.Color;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Quantization;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rasm.Imaging.Files;

// --- [MODELS] --------------------------------------------------------------------------
internal sealed record ImageSample(OutputDepth Depth, Func<PixelFrame, bool, FileMetadata, Option<ColorEncoding>, IImageEncoder, Stream, IO<Unit>> Write) {
    internal static ImageSample Bytes { get; } = Of<Rgba32, Rgb24>(OutputDepth.UInt8);

    internal static ImageSample Of<TAlpha, TOpaque>(OutputDepth depth)
        where TAlpha : unmanaged, IPixel<TAlpha>
        where TOpaque : unmanaged, IPixel<TOpaque> =>
        new(depth, static (frame, alpha, metadata, encoding, encoder, stream) => alpha
            ? StillWriter.Encoded<TAlpha>(frame, alpha, metadata, encoding, encoder, stream)
            : StillWriter.Encoded<TOpaque>(frame, alpha, metadata, encoding, encoder, stream));
}

internal sealed record TiffSample(OutputDepth Depth, SampleFormat Format) {
    internal Option<Predictor> Predictor { get; init; }
}

[ComplexValueObject(SkipFactoryMethods = true)]
internal sealed partial class DngExtent {
    public int Width { get; }
    public int Height { get; }

    public static Fin<DngExtent> Validated(int width, int height) =>
        ImagingFault.Unless(width is >= 22 and <= 64000 && height is >= 22 and <= 64000, ImagingReason.Range, (width, height)).Map(_ => new DngExtent(width, height));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record StillFormat {
    public sealed record PngRaster : StillFormat {
        private PngRaster(PixelFrame frame, ImageSample sample, Option<ColorEncoding> encoding, bool alpha) => (Frame, Sample, Encoding, Alpha) = (frame, sample, encoding, alpha);

        public PixelFrame Frame { get; }
        public Option<ColorEncoding> Encoding { get; }
        public bool Alpha { get; }
        internal ImageSample Sample { get; }

        public static IterableNE<string> Extensions => IterableNE.create(".png");
        public static Seq<OutputDepth> Depths => OutputDepth.Where(PngDepth);

        public static Fin<StillFormat> From(PixelFrame frame, OutputDepth depth, Option<ColorEncoding> encoding, bool alpha) =>
            OutputDepth.Sampled(depth, PngDepth).Map(sample => (StillFormat)new PngRaster(frame, sample, encoding, alpha));

        internal static Option<ImageSample> PngDepth(OutputDepth depth) =>
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

    public sealed record TiffRaster : StillFormat {
        private TiffRaster(PixelFrame frame, TiffSample sample, Option<ColorEncoding> encoding, bool alpha) => (Frame, Sample, Encoding, Alpha) = (frame, sample, encoding, alpha);

        public PixelFrame Frame { get; }
        public Option<ColorEncoding> Encoding { get; }
        public bool Alpha { get; }
        internal TiffSample Sample { get; }

        public static IterableNE<string> Extensions => IterableNE.create(".tif", ".tiff");
        public static Seq<OutputDepth> Depths => OutputDepth.Where(TiffDepth);

        public static Fin<StillFormat> From(PixelFrame frame, OutputDepth depth, Option<ColorEncoding> encoding, bool alpha) =>
            from sample in OutputDepth.Sampled(depth, TiffDepth)
            from _ in sample.Depth.Buffered(frame.Size.Width, alpha)
            select (StillFormat)new TiffRaster(frame, sample, encoding, alpha);

        internal static Option<TiffSample> TiffDepth(OutputDepth depth) =>
            depth.Map<Option<TiffSample>>(
                uInt8: new TiffSample(depth, SampleFormat.UINT) { Predictor = Predictor.HORIZONTAL },
                uInt10: new TiffSample(depth, SampleFormat.UINT),
                uInt12: new TiffSample(depth, SampleFormat.UINT),
                uInt14: new TiffSample(depth, SampleFormat.UINT),
                uInt16: new TiffSample(depth, SampleFormat.UINT) { Predictor = Predictor.HORIZONTAL },
                float16: new TiffSample(depth, SampleFormat.IEEEFP) { Predictor = Predictor.FLOATINGPOINT },
                float32: new TiffSample(depth, SampleFormat.IEEEFP) { Predictor = Predictor.FLOATINGPOINT });
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
                    : new ImagingFault(string.Create(CultureInfo.InvariantCulture, $"DNG field {tag} value {value} in [{minimum}, {int.MaxValue})"), ImagingReason.Range);
            return (
                    DngExtent.Validated(frame.Size.Width, frame.Size.Height),
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
            tiffRaster: static (save, tiff) => Scanlines(save.Stream, "w", [], Tagged(tiff.Frame.Size, tiff.Sample, tiff.Alpha, tiff.Encoding, save.Metadata, []), tiff.Frame.Size.Height, Rows(tiff.Frame, tiff.Sample.Depth, tiff.Alpha)),
            exrRaster: static (save, exr) => ExrParts.Write(exr, save.Metadata, save.Stream),
            dng: static (save, dng) => DngWriter.Write(dng, save.Metadata, save.Stream),
            document: static (save, document) => document.Content.Write(save.Metadata, save.Stream));

    internal static void Associate(ReadOnlySpan<Vector4> row, Span<Vector4> into) =>
        PixelOperations<RgbaVector>.Instance.ToVector4(Configuration.Default, MemoryMarshal.Cast<Vector4, RgbaVector>(row), into, PixelConversionModifiers.Premultiply);

    internal static Action<int, Span<byte>> Rows(PixelFrame frame, OutputDepth depth, bool alpha) =>
        alpha
            ? (y, destination) => depth.Encode(frame.View.Span.GetRowSpan(y), alpha: true, destination)
            : (y, destination) => {
                using SpanOwner<Vector4> scratch = SpanOwner<Vector4>.Allocate(frame.Size.Width);
                Associate(frame.View.Span.GetRowSpan(y), scratch.Span);
                depth.Encode(scratch.Span, alpha: false, destination);
            };

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

    internal static IO<Unit> Scanlines(Stream stream, string mode, Seq<TiffFieldInfo> custom, Func<Tiff, Seq<(TiffTag Tag, object[] Values)>> fields, int height, Action<int, Span<byte>> row) =>
        (from tiff in use(IO.lift(() => Optional(Tiff.ClientOpen(nameof(Tiff), mode, stream, new BorrowedTiffStream())).ToFin(new ImagingFault("TIFF stream open", ImagingReason.Refused))))
         from encoded in IO.lift(environment => Encode(tiff, custom, fields, height, row, environment.Token))
         from flushed in IO.lift(() => tiff.Flush() ? Fin.Succ(unit) : new ImagingFault("TIFF stream flush", ImagingReason.Refused))
         select unit).Bracket();

    internal static IO<Unit> Encoded<TPixel>(PixelFrame frame, bool alpha, FileMetadata metadata, Option<ColorEncoding> encoding, IImageEncoder encoder, Stream stream)
        where TPixel : unmanaged, IPixel<TPixel> =>
        IO.liftAsync(async environment => {
            using Image<TPixel> image = new(frame.Size.Width, frame.Size.Height);
            metadata.Into(image.Metadata, encoding);
            image.ProcessPixelRows(rows => {
                using SpanOwner<Vector4> scratch = SpanOwner<Vector4>.Allocate(rows.Width);
                for (int y = 0; y < rows.Height; y++) {
                    environment.Token.ThrowIfCancellationRequested();
                    ReadOnlySpan<Vector4> source = frame.View.Span.GetRowSpan(y);
                    if (alpha)
                        source.CopyTo(scratch.Span);
                    else
                        Associate(source, scratch.Span);
                    PixelOperations<TPixel>.Instance.FromVector4Destructive(Configuration.Default, scratch.Span, rows.GetRowSpan(y), PixelConversionModifiers.Scale);
                }
            });
            await image.SaveAsync(stream, encoder, environment.Token).ConfigureAwait(false);
            return unit;
        });

    private static Fin<Unit> Encode(Tiff tiff, Seq<TiffFieldInfo> custom, Func<Tiff, Seq<(TiffTag Tag, object[] Values)>> fields, int height, Action<int, Span<byte>> row, CancellationToken token) {
        tiff.MergeFieldInfo([.. custom], custom.Count);
        return fields(tiff).TraverseM(field => tiff.SetField(field.Tag, field.Values) ? Fin.Succ(unit) : new ImagingFault($"TIFF field {field.Tag}", ImagingReason.Refused)).As().Bind(_ => {
            using MemoryOwner<byte> line = MemoryOwner<byte>.Allocate(tiff.ScanlineSize());
            ArraySegment<byte> segment = line.DangerousGetArray();
            for (int y = 0; y < height && !token.IsCancellationRequested; y++) {
                row(y, line.Span);
                if (!tiff.WriteScanline(segment.Array, segment.Offset, y, 0))
                    return new ImagingFault(string.Create(CultureInfo.InvariantCulture, $"TIFF scanline {y}"), ImagingReason.Refused);
            }
            return token.IsCancellationRequested ? Errors.Cancelled : Fin.Succ(unit);
        });
    }
}

internal static class DngWriter {
    private const Photometric LinearRaw = (Photometric)34892;
    private const short D65 = 21;
    private static readonly byte[] Version = [1, 4, 0, 0];
    private static readonly float[] Neutral = [1f, 1f, 1f];
    private static readonly Seq<TiffFieldInfo> Optics = [
        new(TiffTag.EXIF_FNUMBER, 1, 1, TiffType.RATIONAL, FieldBit.Custom, okToChange: true, passCount: false, "FNumber"),
        new(TiffTag.EXIF_FOCALLENGTH, 1, 1, TiffType.RATIONAL, FieldBit.Custom, okToChange: true, passCount: false, "FocalLength"),
    ];

    internal static IO<Unit> Write(StillFormat.Dng dng, FileMetadata metadata, Stream stream) =>
        StillWriter.Scanlines(
            stream,
            "w4",
            Optics,
            _ => [
                (TiffTag.SUBFILETYPE, [0]),
                (TiffTag.IMAGEWIDTH, [dng.Frame.Size.Width]),
                (TiffTag.IMAGELENGTH, [dng.Frame.Size.Height]),
                (TiffTag.BITSPERSAMPLE, [OutputDepth.Float32.Bits]),
                (TiffTag.SAMPLESPERPIXEL, [3]),
                (TiffTag.SAMPLEFORMAT, [SampleFormat.IEEEFP]),
                (TiffTag.PHOTOMETRIC, [LinearRaw]),
                (TiffTag.PLANARCONFIG, [PlanarConfig.CONTIG]),
                (TiffTag.COMPRESSION, [Compression.NONE]),
                (TiffTag.ORIENTATION, [Orientation.TOPLEFT]),
                (TiffTag.MAKE, [metadata.Software]),
                (TiffTag.MODEL, [metadata.Software]),
                (TiffTag.UNIQUECAMERAMODEL, [metadata.Software]),
                (TiffTag.DNGVERSION, [Version]),
                (TiffTag.DNGBACKWARDVERSION, [Version]),
                (TiffTag.COLORMATRIX1, [dng.Matrix.Count, dng.Matrix.ToArray()]),
                (TiffTag.CALIBRATIONILLUMINANT1, [D65]),
                (TiffTag.ASSHOTNEUTRAL, [Neutral.Length, Neutral]),
                .. dng.Exposure.Map(static exposure => (TiffTag.BASELINEEXPOSURE, new object[] { exposure })).ToSeq(),
                .. dng.FocalMillimeters.Map(static focal => (TiffTag.EXIF_FOCALLENGTH, new object[] { focal })).ToSeq(),
                .. dng.Aperture.Map(static stop => (TiffTag.EXIF_FNUMBER, new object[] { stop })).ToSeq(),
                .. metadata.Tiff(),
                (TiffTag.ROWSPERSTRIP, [dng.Frame.Size.Height]),
            ],
            dng.Frame.Size.Height,
            StillWriter.Rows(dng.Frame, OutputDepth.Float32, alpha: false));
}
