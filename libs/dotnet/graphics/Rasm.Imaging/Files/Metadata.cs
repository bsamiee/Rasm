using System.Buffers.Binary;
using System.Globalization;
using System.Text;
using BitMiracle.LibTiff.Classic;
using Rasm.Imaging.Color;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png.Chunks;
using SixLabors.ImageSharp.Metadata;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;
using SixLabors.ImageSharp.Metadata.Profiles.Icc;
using TinyEXR;

namespace Rasm.Imaging.Files;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record FileMetadata(string Software, DateTimeOffset Created, double Density, Map<string, string> Fields) {
    private const string CapDateAttribute = "capDate";
    private const string SoftwareAttribute = "Software";
    private const string UtcOffsetAttribute = "utcOffset";
    private const string XDensityAttribute = "xDensity";
    private const string InteropIdAttribute = "colorInteropID";

    private string Stamp => Created.ToString("yyyy:MM:dd HH:mm:ss", CultureInfo.InvariantCulture);

    internal void Into(ImageMetadata metadata, Option<ColorEncoding> encoding) {
        ExifProfile exif = new();
        exif.SetValue(ExifTag.Software, Software);
        exif.SetValue(ExifTag.DateTimeOriginal, Stamp);
        exif.SetValue(ExifTag.OffsetTimeOriginal, Created.ToString("zzz", CultureInfo.InvariantCulture));
        _ = encoding.Iter(encoded => exif.SetValue(ExifTag.ColorSpace, ColorTags.ExifColorSpace(encoded)));
        metadata.ResolutionUnits = PixelResolutionUnit.PixelsPerInch;
        metadata.HorizontalResolution = Density;
        metadata.VerticalResolution = Density;
        metadata.ExifProfile = exif;
        _ = encoding.Bind(encoded => encoded.Profile(Created)).Iter(bytes => metadata.IccProfile = new IccProfile(bytes));
        _ = encoding.Bind(static encoded => encoded.Cicp).Iter(cicp => metadata.CicpProfile = cicp);
        metadata.GetPngMetadata().TextData = [.. Fields.ToSeq().Map(static field => new PngTextData(field.Key, field.Value, "", ""))];
    }

    internal Seq<(TiffTag Tag, object[] Values)> Tiff() => [
        (TiffTag.SOFTWARE, [Software]),
        (TiffTag.DATETIME, [Stamp]),
        (TiffTag.XRESOLUTION, [(float)Density]),
        (TiffTag.YRESOLUTION, [(float)Density]),
        (TiffTag.RESOLUTIONUNIT, [ResUnit.INCH]),
    ];

    internal Seq<HeaderAttribute> Exr(Option<Gamut> gamut) {
        static HeaderAttribute Text(string name, string text) => new(name, "string", Encoding.UTF8.GetBytes(text));
        static HeaderAttribute Single(string name, float value) {
            Span<byte> bytes = stackalloc byte[sizeof(float)];
            BinaryPrimitives.WriteSingleLittleEndian(bytes, value);
            return new HeaderAttribute(name, "float", bytes);
        }
        return [
            Text(CapDateAttribute, Stamp),
            Text(SoftwareAttribute, Software),
            Single(UtcOffsetAttribute, (float)-Created.Offset.TotalSeconds),
            Single(XDensityAttribute, (float)Density),
            .. gamut.Bind(static held => held.InteropId).Map(static id => Text(InteropIdAttribute, id)).ToSeq(),
            .. Fields.ToSeq().Map(static field => Text(field.Key, field.Value)),
        ];
    }
}
