using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;
using Rasm.Imaging.Color;
using Rasm.Imaging.Pixels;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Metadata.Profiles.Cicp;
using SixLabors.ImageSharp.Metadata.Profiles.Icc;
using SixLabors.ImageSharp.PixelFormats;

namespace Rasm.Imaging.Files;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ImagePath {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) =>
        validationError = Path.IsPathFullyQualified(value) && ImageFile.Reads.ContainsKey(Path.GetExtension(value)) ? null
            : new ValidationError($"image path '{value}' must be fully qualified with one of {string.Join(", ", ImageFile.Reads.Keys)}");
}

public sealed record ImageFile {
    private ImageFile(ImagePath path, PixelFrame frame) => (Path, Frame) = (path, frame);

    internal static FrozenDictionary<string, Func<FileInfo, IO<PixelFrame>>> Reads { get; } =
        new KeyValuePair<string, Func<FileInfo, IO<PixelFrame>>>[] {
            new(".exr", static file => PixelFrame.Read(file, "").Map(static read => read.Frame)),
            new(".png", Png),
        }.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

    public ImagePath Path { get; }
    public PixelFrame Frame { get; }

    public static IO<ImageFile> Load(ImagePath path) =>
        Reads[System.IO.Path.GetExtension(path)](new FileInfo(path)).Map(frame => new ImageFile(path, frame));

    private static IO<PixelFrame> Png(FileInfo file) =>
        (from stream in use(() => new FileStream(file.FullName, new FileStreamOptions { Mode = FileMode.Open, Access = FileAccess.Read, Options = FileOptions.Asynchronous }))
         from info in IO.liftAsync(env => PngDecoder.Instance.IdentifyAsync(Decoding(ColorProfileHandling.Preserve), stream, env.Token))
         let cicp = Optional(info.Metadata.CicpProfile)
         from encoding in IO.lift(Encoding(cicp))
         from _ in IO.lift(() => stream.Seek(0, SeekOrigin.Begin))
         from frame in (
                 from image in use(IO.liftAsync(env => PngDecoder.Instance.DecodeAsync<RgbaVector>(
                     Decoding(cicp.Match(Some: static _ => ColorProfileHandling.Preserve, None: static () => ColorProfileHandling.Convert)), stream, env.Token)))
                 from size in IO.lift(PixelExtent.Validated(image.Width, image.Height))
                 select new PixelFrame(Point.Empty, size, size, block => {
                     image.CopyPixelDataTo(MemoryMarshal.Cast<float, RgbaVector>(block.AsSpan()));
                     encoding.Decode(MemoryMarshal.Cast<float, Vector4>(block.AsSpan()));
                 }))
             .Bracket()
         select frame)
            .Bracket()
            .Catch(
                static error => error.HasException<SixLabors.ImageSharp.ImageFormatException>() || error.HasException<InvalidIccProfileException>()
                    || error.HasException<NotSupportedException>() || error.HasException<IOException>() || error.HasException<UnauthorizedAccessException>(),
                error => IO.fail<PixelFrame>(Error.New($"PNG decoder refused {file.FullName}", error)));

    private static Fin<ColorEncoding> Encoding(Option<CicpProfile> cicp) =>
        cicp.Match(
            Some: static chunk =>
                (ColorTags.Primaries.Find(row => row.Code == chunk.ColorPrimaries).Map(static row => row.Gamut), ColorTags.Transfers.Find(row => row.Code == chunk.TransferCharacteristics))
                    .Apply(static (gamut, row) => new ColorEncoding(gamut, row.Transfer, row.White))
                    .As()
                    .Filter(_ => chunk.MatrixCoefficients is CicpMatrixCoefficients.Identity && chunk.FullRange)
                    .ToFin(Error.New($"PNG cICP {chunk.ColorPrimaries}/{chunk.TransferCharacteristics}/{chunk.MatrixCoefficients} full range {chunk.FullRange} names no held gamut and transfer")),
            None: static () => new ColorEncoding(Gamut.StandardRgb, TransferCurve.Srgb, Nits.ReferenceWhite));

    private static DecoderOptions Decoding(ColorProfileHandling handling) =>
        new() { MaxFrames = 1, SegmentIntegrityHandling = SegmentIntegrityHandling.Strict, ColorProfileHandling = handling };
}
