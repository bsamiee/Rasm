using BitMiracle.LibTiff.Classic;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Quantization;

namespace Rasm.Imaging.Output;

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class DngWriter {
    private const Photometric LinearRaw = (Photometric)34892;
    private const short D65 = 21;
    private const int Narrowest = 22;
    private const int Widest = 64000;
    private static readonly byte[] Version = [1, 4, 0, 0];
    private static readonly float[] Neutral = [1f, 1f, 1f];
    private static readonly Seq<TiffFieldInfo> Optics = [
        new(TiffTag.EXIF_FNUMBER, 1, 1, TiffType.RATIONAL, FieldBit.Custom, okToChange: true, passCount: false, "FNumber"),
        new(TiffTag.EXIF_FOCALLENGTH, 1, 1, TiffType.RATIONAL, FieldBit.Custom, okToChange: true, passCount: false, "FocalLength"),
    ];

    internal static Validation<Error, Unit> Bounded(PixelExtent size) =>
        ((long)OutputDepth.Float32.RowBytes(1, alpha: false) * size.Width * size.Height) switch {
            var bytes => (
                    size.Width is >= Narrowest and <= Widest && size.Height is >= Narrowest and <= Widest
                        ? Validation.Success<Error, Unit>(unit)
                        : Validation.Fail<Error, Unit>(new DngExtentRefused(size)),
                    bytes <= StillWriter.ClassicBytes ? Validation.Success<Error, Unit>(unit) : Validation.Fail<Error, Unit>(new TiffBytesRefused(bytes)))
                .Apply(static (_, _) => unit)
                .As(),
        };

    internal static IO<Unit> Write(StillFormat.Dng dng, FileMetadata metadata, FileStream stream) =>
        StillWriter.Scanlines(
            stream,
            "w4",
            Optics,
            _ => {
                float[] matrix = new float[9];
                dng.Gamut.FromXyz.CopyTo(matrix);
                return [
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
                    (TiffTag.MAKE, [(string)metadata.Software]),
                    (TiffTag.MODEL, [(string)metadata.Software]),
                    (TiffTag.UNIQUECAMERAMODEL, [(string)metadata.Software]),
                    (TiffTag.DNGVERSION, [Version]),
                    (TiffTag.DNGBACKWARDVERSION, [Version]),
                    (TiffTag.COLORMATRIX1, [matrix.Length, matrix]),
                    (TiffTag.CALIBRATIONILLUMINANT1, [D65]),
                    (TiffTag.ASSHOTNEUTRAL, [Neutral.Length, Neutral]),
                    (TiffTag.BASELINEEXPOSURE, [(float)dng.Exposure]),
                    .. dng.FocalLength.Map(static focal => (TiffTag.EXIF_FOCALLENGTH, new object[] { (float)Length.FromMeters((float)focal).Millimeters })).ToSeq(),
                    .. dng.Aperture.Map(static stop => (TiffTag.EXIF_FNUMBER, new object[] { (float)stop })).ToSeq(),
                    .. metadata.Tiff(),
                    (TiffTag.ROWSPERSTRIP, [dng.Frame.Size.Height]),
                ];
            },
            dng.Frame.Size.Height,
            StillWriter.Rows(dng.Frame, OutputDepth.Float32, alpha: false));
}
