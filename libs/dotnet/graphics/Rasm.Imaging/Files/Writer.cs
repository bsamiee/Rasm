using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;
using BitMiracle.LibTiff.Classic;
using CommunityToolkit.HighPerformance.Buffers;
using Rasm.Imaging.Color;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Quantization;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;
using Wacton.Unicolour;

namespace Rasm.Imaging.Files;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ImageFile {
    public abstract IO<Unit> Write(FileMetadata metadata, Stream stream);

    public sealed record Raster(PixelFrame Frame, Option<ColorEncoding> Encoding, bool Alpha, IImageEncoder Encoder) : ImageFile {
        public override IO<Unit> Write(FileMetadata metadata, Stream stream) =>
            IO.liftAsync(async environment => {
                using SixLabors.ImageSharp.Image<RgbaVector> image = new(Frame.Extent.Width, Frame.Extent.Height);
                metadata.Into(image.Metadata, Encoding);
                image.ProcessPixelRows(rows => {
                    for (int y = 0; y < rows.Height; y++)
                        Placed(Frame, Alpha, y, MemoryMarshal.Cast<RgbaVector, Vector4>(rows.GetRowSpan(y)));
                });
                await image.SaveAsync(stream, Encoder, environment.Token).ConfigureAwait(false);
                return unit;
            });
    }

    public sealed record Tiff(PixelFrame Frame, OutputDepth Depth, Option<ColorEncoding> Encoding, bool Alpha) : ImageFile {
        public override IO<Unit> Write(FileMetadata metadata, Stream stream) => Scanlines(stream, Frame, Depth, Alpha, metadata, Rgb(Depth, Encoding, metadata), []);
    }

    public sealed record Dng(PixelFrame Frame, Gamut Gamut, Option<float> Exposure, Option<Length> FocalLength, Option<float> Aperture) : ImageFile {
        private const Photometric LinearRaw = (Photometric)34892;
        private const short D65 = 21;
        private static readonly Seq<TiffFieldInfo> Optics = [
            new(TiffTag.EXIF_FNUMBER, 1, 1, TiffType.RATIONAL, FieldBit.Custom, okToChange: true, passCount: false, "FNumber"),
            new(TiffTag.EXIF_FOCALLENGTH, 1, 1, TiffType.RATIONAL, FieldBit.Custom, okToChange: true, passCount: false, "FocalLength"),
        ];

        public static Seq<string> FileExtensions { get; } = ["dng"];

        public override IO<Unit> Write(FileMetadata metadata, Stream stream) =>
            from accepted in IO.lift(ImagingFault.Unless(
                Exposure.ForAll(static stops => float.Abs(stops) < int.MaxValue) && FocalLength.ForAll(static focal => (double)focal.Millimeters is >= 0d and < int.MaxValue) && Aperture.ForAll(static stop => stop is >= 0f and < int.MaxValue),
                ImagingReason.Range, (Exposure, FocalLength, Aperture)))
            let illuminant = Gamut.StandardRgb.Configuration.Xyz.WhitePoint
            let image = Gamut.Columns(Gamut.StandardRgb.Configuration, ColourSpace.Xyz, Gamut.Configuration, ColourSpace.RgbLinear)
            let neutral = Vector3.Transform(new Vector3((float)illuminant.X, (float)illuminant.Y, (float)illuminant.Z), image)
            from written in Scanlines(stream, Frame, OutputDepth.Float32, alpha: false, metadata, [
                (TiffTag.SUBFILETYPE, [0]),
                (TiffTag.PHOTOMETRIC, [LinearRaw]),
                (TiffTag.COMPRESSION, [Compression.NONE]),
                (TiffTag.MAKE, [metadata.Software]),
                (TiffTag.MODEL, [metadata.Software]),
                (TiffTag.UNIQUECAMERAMODEL, [metadata.Software]),
                (TiffTag.DNGVERSION, [new byte[] { 1, 4, 0, 0 }]),
                (TiffTag.DNGBACKWARDVERSION, [new byte[] { 1, 4, 0, 0 }]),
                (TiffTag.COLORMATRIX1, [9, new[] { image.M11, image.M21, image.M31, image.M12, image.M22, image.M32, image.M13, image.M23, image.M33 }]),
                (TiffTag.CALIBRATIONILLUMINANT1, [D65]),
                (TiffTag.ASSHOTNEUTRAL, [3, new[] { neutral.X, neutral.Y, neutral.Z }]),
                .. Exposure.Map(static stops => (TiffTag.BASELINEEXPOSURE, new object[] { stops })).ToSeq(),
                .. FocalLength.Map(static focal => (TiffTag.EXIF_FOCALLENGTH, new object[] { (float)focal.Millimeters })).ToSeq(),
                .. Aperture.Map(static stop => (TiffTag.EXIF_FNUMBER, new object[] { stop })).ToSeq(),
            ], Optics)
            select written;
    }

    private static Fin<PixelFrame> Canvas(Seq<PixelFrame> frames, bool windowed) =>
        from canvas in frames.Head.ToFin(ImagingFault.Of(ImagingReason.Empty, frames.Count))
        from matched in frames.Map(static (frame, index) => (Frame: frame, Index: index))
            .Traverse(entry => ImagingFault.Unless(entry.Frame.Extent == canvas.Extent && (!windowed || entry.Frame.Window == canvas.Window), ImagingReason.Mismatch, entry.Frame.Window, entry.Index))
            .As()
        select canvas;

    private static void Placed(PixelFrame frame, bool alpha, int y, Span<Vector4> line) {
        Rectangle visible = Rectangle.Intersect(frame.Window, new Rectangle(0, 0, frame.Extent.Width, frame.Extent.Height));
        line.Clear();
        if (y < visible.Top || y >= visible.Bottom)
            return;
        ReadOnlySpan<Vector4> source = frame.View.Span.GetRowSpan(y - frame.Origin.Y).Slice(visible.Left - frame.Origin.X, visible.Width);
        if (alpha)
            source.CopyTo(line.Slice(visible.Left, visible.Width));
        else
            PixelFrame.Associated(source, line.Slice(visible.Left, visible.Width));
    }

    private static Seq<(TiffTag Tag, object[] Values)> Rgb(OutputDepth depth, Option<ColorEncoding> encoding, FileMetadata metadata) => [
        (TiffTag.PHOTOMETRIC, [Photometric.RGB]),
        (TiffTag.COMPRESSION, [Compression.ADOBE_DEFLATE]),
        (TiffTag.PREDICTOR, [depth.Levels.IsNone ? Predictor.FLOATINGPOINT : depth.Bits == 8 * depth.SampleBytes ? Predictor.HORIZONTAL : Predictor.NONE]),
        .. encoding.Bind(held => held.Icc(metadata.Created)).Map(static profile => profile.ToByteArray() switch { var bytes => (TiffTag.ICCPROFILE, new object[] { bytes.Length, bytes }) }).ToSeq(),
    ];

    private static IO<Unit> Scanlines(Stream stream, PixelFrame frame, OutputDepth depth, bool alpha, FileMetadata metadata, Seq<(TiffTag Tag, object[] Values)> fields, Seq<TiffFieldInfo> custom) =>
        from buffered in IO.lift(depth.Buffered(frame.Extent.Width, alpha))
        from written in use(IO.lift(() => Optional(BitMiracle.LibTiff.Classic.Tiff.ClientOpen(nameof(Tiff), "w", stream, new BorrowedTiffStream())).ToFin(ImagingFault.Of(ImagingReason.Refused, stream))))
            .Bind(tiff => IO.lift(environment => Scanned(tiff, frame, depth, alpha, [
             (TiffTag.IMAGEWIDTH, [frame.Extent.Width]),
             (TiffTag.IMAGELENGTH, [frame.Extent.Height]),
             (TiffTag.BITSPERSAMPLE, [depth.Bits]),
             (TiffTag.SAMPLESPERPIXEL, [alpha ? 4 : 3]),
             (TiffTag.SAMPLEFORMAT, [depth.Levels.IsSome ? SampleFormat.UINT : SampleFormat.IEEEFP]),
             (TiffTag.PLANARCONFIG, [PlanarConfig.CONTIG]),
             (TiffTag.ORIENTATION, [Orientation.TOPLEFT]),
             .. alpha ? Seq((TiffTag.EXTRASAMPLES, new object[] { 1, new[] { (short)ExtraSample.UNASSALPHA } })) : [],
             .. metadata.Tiff(),
             .. fields,
             ], custom, environment.Token)))
            .Bracket()
        select written;

    private static Fin<Unit> Scanned(BitMiracle.LibTiff.Classic.Tiff tiff, PixelFrame frame, OutputDepth depth, bool alpha, Seq<(TiffTag Tag, object[] Values)> fields, Seq<TiffFieldInfo> custom, CancellationToken token) {
        Fin<Unit> Set((TiffTag Tag, object[] Values) field) => ImagingFault.Unless(tiff.SetField(field.Tag, field.Values), ImagingReason.Refused, field.Tag);
        tiff.MergeFieldInfo([.. custom], custom.Count);
        return fields.TraverseM(Set).As()
            .Bind(_ => Set((TiffTag.ROWSPERSTRIP, [tiff.DefaultStripSize(0)])))
            .Bind(_ => {
                using MemoryOwner<byte> line = MemoryOwner<byte>.Allocate(tiff.ScanlineSize());
                using SpanOwner<Vector4> pixels = SpanOwner<Vector4>.Allocate(frame.Extent.Width);
                ArraySegment<byte> segment = line.DangerousGetArray();
                for (int y = 0; y < frame.Extent.Height; y++) {
                    if (token.IsCancellationRequested)
                        return Errors.Cancelled;
                    Placed(frame, alpha, y, pixels.Span);
                    depth.Encode(pixels.Span, alpha, line.Span);
                    if (!tiff.WriteScanline(segment.Array, segment.Offset, y, 0))
                        return ImagingFault.Of(ImagingReason.Refused, y);
                }
                return ImagingFault.Unless(tiff.Flush(), ImagingReason.Refused, frame.Extent);
            });
    }
}

// --- [SERVICES] ------------------------------------------------------------------------
file sealed class BorrowedTiffStream : TiffStream {
    public override void Close(object clientData) { }
}
