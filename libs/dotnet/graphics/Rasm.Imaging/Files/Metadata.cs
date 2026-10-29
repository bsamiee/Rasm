using System.Buffers.Binary;
using System.Globalization;
using System.Text;
using BitMiracle.LibTiff.Classic;
using Rasm.Imaging.Color;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Png.Chunks;
using SixLabors.ImageSharp.Metadata;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;
using TinyEXR;

namespace Rasm.Imaging.Files;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record FileMetadata(string Software, DateTimeOffset Created, double Density, Map<string, string> Fields) {
    internal const string InteropId = "colorInteropID";

    internal Seq<(TiffTag Tag, object[] Values)> Tiff() => [
        (TiffTag.SOFTWARE, [Software]),
        (TiffTag.DATETIME, [Stamp]),
        (TiffTag.XRESOLUTION, [(float)Density]),
        (TiffTag.YRESOLUTION, [(float)Density]),
        (TiffTag.RESOLUTIONUNIT, [ResUnit.INCH]),
    ];

    private string Stamp => Created.ToString("yyyy:MM:dd HH:mm:ss", CultureInfo.InvariantCulture);

    internal void Into(ImageMetadata metadata, Option<ColorEncoding> encoding) {
        ExifProfile exif = new();
        exif.SetValue(ExifTag.Software, Software);
        exif.SetValue(ExifTag.DateTimeOriginal, Stamp);
        exif.SetValue(ExifTag.OffsetTimeOriginal, Created.ToString("zzz", CultureInfo.InvariantCulture));
        exif.SetValue(ExifTag.ColorSpace, encoding.Exists(static held => held.Gamut == Gamut.StandardRgb && held.Transfer == Transfer.Gamma.Srgb) ? (ushort)1 : ushort.MaxValue);
        metadata.ResolutionUnits = PixelResolutionUnit.PixelsPerInch;
        metadata.HorizontalResolution = Density;
        metadata.VerticalResolution = Density;
        metadata.ExifProfile = exif;
        metadata.IccProfile = encoding.Bind(held => held.Icc(Created)).ValueUnsafe();
        metadata.CicpProfile = encoding.Bind(static held => held.Cicp).ValueUnsafe();
        metadata.GetFormatMetadata(PngFormat.Instance).TextData = [.. Fields.ToSeq().Map(static field => new PngTextData(field.Key, field.Value, "", ""))];
    }

    internal Seq<HeaderAttribute> Exr(Option<Gamut> gamut) {
        static HeaderAttribute Text(string name, string text) => new(name, "string", Encoding.UTF8.GetBytes(text));
        static HeaderAttribute Single(string name, double value) {
            Span<byte> bytes = stackalloc byte[sizeof(float)];
            BinaryPrimitives.WriteSingleLittleEndian(bytes, (float)value);
            return new HeaderAttribute(name, "float", bytes);
        }
        return [
            Text("capDate", Stamp),
            Text("Software", Software),
            Single("utcOffset", -Created.Offset.TotalSeconds),
            Single("xDensity", Density),
            .. gamut.Bind(static held => held.InteropId).Map(static id => Text(InteropId, id)).ToSeq(),
            .. Fields.ToSeq().Map(static field => Text(field.Key, field.Value)),
        ];
    }
}
