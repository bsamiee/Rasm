using System.Numerics;
using BitMiracle.LibTiff.Classic;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Quantization;

namespace Rasm.Imaging.Files;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<int>]
internal readonly partial struct DngSide : IMinMaxValue<DngSide> {
    public static DngSide MinValue { get; } = new(22);
    public static DngSide MaxValue { get; } = new(64000);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class DngWriter {
    private const Photometric LinearRaw = (Photometric)34892;
    private const short D65 = 21;
    private static readonly byte[] Version = [1, 4, 0, 0];
    private static readonly float[] Neutral = [1f, 1f, 1f];
    private static readonly Seq<TiffFieldInfo> Optics = [
        new(TiffTag.EXIF_FNUMBER, 1, 1, TiffType.RATIONAL, FieldBit.Custom, okToChange: true, passCount: false, "FNumber"),
        new(TiffTag.EXIF_FOCALLENGTH, 1, 1, TiffType.RATIONAL, FieldBit.Custom, okToChange: true, passCount: false, "FocalLength"),
    ];

    internal static Fin<Unit> Bounded(PixelExtent size) =>
        (DngSide.Validated(size.Width), DngSide.Validated(size.Height)).Apply(static (_, _) => unit).As()
            .MapFail(error => Error.New($"frame {size} lies outside the DNG extent LibRaw reads", error));

    internal static IO<Unit> Write(StillFormat.Dng dng, FileMetadata metadata, FileStream stream) =>
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
                (TiffTag.MAKE, [(string)metadata.Software]),
                (TiffTag.MODEL, [(string)metadata.Software]),
                (TiffTag.UNIQUECAMERAMODEL, [(string)metadata.Software]),
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
