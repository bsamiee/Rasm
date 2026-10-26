using System.Buffers.Binary;
using System.Globalization;
using System.Text;
using BitMiracle.LibTiff.Classic;
using MathNet.Numerics.LinearAlgebra;
using Rasm.Imaging.Color;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Png.Chunks;
using SixLabors.ImageSharp.Metadata;
using SixLabors.ImageSharp.Metadata.Profiles.Cicp;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;
using SixLabors.ImageSharp.Metadata.Profiles.Icc;
using TinyEXR;
using Wacton.Unicolour;

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
        metadata.GetFormatMetadata(PngFormat.Instance).TextData = [.. Fields.ToSeq().Map(static field => new PngTextData(field.Key, field.Value, "", ""))];
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

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class ColorTags {
    internal static readonly Seq<(Transfer Transfer, CicpTransferCharacteristics Code)> Transfers = [
        (new Transfer.Linear(), CicpTransferCharacteristics.Linear),
        (new Transfer.Srgb(), CicpTransferCharacteristics.Iec61966_2_1),
        (Transfer.Gamma.Gamma22, CicpTransferCharacteristics.Gamma2_2),
        (Transfer.Pq.High, CicpTransferCharacteristics.SmpteSt2084),
        (Transfer.Hlg.High, CicpTransferCharacteristics.AribStdB67),
    ];

    internal static readonly Seq<(Gamut Gamut, CicpColorPrimaries Code)> Primaries = [
        (Gamut.StandardRgb, CicpColorPrimaries.ItuRBt709_6),
        (Gamut.DisplayP3, CicpColorPrimaries.SmpteEg432_1),
        (Gamut.Rec2020, CicpColorPrimaries.ItuRBt2020_2),
    ];

    private static readonly Configuration Pcs = new(xyzConfig: new XyzConfiguration(new WhitePoint(0.9642, 1.0, 0.8249)));

    extension(ColorEncoding encoding) {
        public Option<CicpProfile> Cicp =>
            (Primaries.Find(row => row.Gamut == encoding.Gamut).Map(static row => row.Code), Transfers.Find(row => row.Transfer == encoding.Transfer).Map(static row => row.Code))
                .Apply(static (primaries, transfer) => new CicpProfile {
                    ColorPrimaries = primaries, TransferCharacteristics = transfer, MatrixCoefficients = CicpMatrixCoefficients.Identity, FullRange = true,
                })
                .As();

        public Option<byte[]> Profile(DateTimeOffset created) =>
            encoding.Transfer.Switch(
                linear: static _ => Some((Function: 0, Parameters: Seq(1d))),
                srgb: static _ => Some((Function: 3, Parameters: (1d / 1.055) switch { var gain => Seq(2.4, gain, 1d - gain, 1d / 12.92, 0.04045) })),
                pq: static _ => Option<(int Function, Seq<double> Parameters)>.None,
                hlg: static _ => Option<(int Function, Seq<double> Parameters)>.None,
                acesCct: static _ => Option<(int Function, Seq<double> Parameters)>.None,
                acesCc: static _ => Option<(int Function, Seq<double> Parameters)>.None,
                gamma: static transfer => Some((Function: 0, Parameters: Seq<double>(transfer.Exponent))),
                camera: static _ => Option<(int Function, Seq<double> Parameters)>.None)
            .Map(curve => Written(encoding, curve, created));
    }

    internal static ushort ExifColorSpace(ColorEncoding encoding) =>
        encoding.Gamut == Gamut.StandardRgb && encoding.Transfer is Transfer.Srgb ? (ushort)1 : ushort.MaxValue;

    private static byte[] Written(ColorEncoding encoding, (int Function, Seq<double> Parameters) curve, DateTimeOffset created) {
        const int HeaderBytes = 128;
        static uint Fixed(double value) => unchecked((uint)(int)double.Round(value * 65536d, MidpointRounding.ToEven));
        static byte[] Words(Seq<uint> words) {
            byte[] bytes = new byte[sizeof(uint) * words.Count];
            for (int i = 0; i < words.Count; i++)
                BinaryPrimitives.WriteUInt32BigEndian(bytes.AsSpan(sizeof(uint) * i), words[i]);
            return bytes;
        }
        static byte[] Block(IccTypeSignature type, Seq<uint> words) => Words([(uint)type, 0, .. words]);
        static byte[] Text(string text) => Encoding.BigEndianUnicode.GetBytes(text) switch {
            var units => [.. Block(IccTypeSignature.MultiLocalizedUnicode, [1, 12, BinaryPrimitives.ReadUInt32BigEndian("enUS"u8), (uint)units.Length, 28]), .. units],
        };
        static Matrix<double> ToPcs(Configuration source, ColourSpace space) => Gamut.Columns(basis => new Unicolour(source, space, basis).ConvertToConfiguration(Pcs).Xyz.Tuple);

        Matrix<double> colorants = ToPcs(encoding.Gamut.Configuration, ColourSpace.RgbLinear);
        double[] adaptation = ToPcs(new Configuration(xyzConfig: new XyzConfiguration(encoding.Gamut.Rgb.WhitePoint)), ColourSpace.Xyz).ToRowMajorArray();
        Seq<uint> illuminant = [Fixed(Pcs.Xyz.WhitePoint.X), Fixed(Pcs.Xyz.WhitePoint.Y), Fixed(Pcs.Xyz.WhitePoint.Z)];
        Seq<(Seq<IccProfileTag> Tags, byte[] Data)> blocks = [
            ([IccProfileTag.ProfileDescription], Text(string.Join(' ', encoding.Gamut.Rgb.Name, encoding.Transfer.ToValue()))),
            ([IccProfileTag.Copyright], Text("No copyright")),
            ([IccProfileTag.MediaWhitePoint], Block(IccTypeSignature.Xyz, illuminant)),
            ([IccProfileTag.ChromaticAdaptation], Block(IccTypeSignature.S15Fixed16Array, toSeq(adaptation).Map(static entry => Fixed(entry)))),
            .. Seq(IccProfileTag.RedMatrixColumn, IccProfileTag.GreenMatrixColumn, IccProfileTag.BlueMatrixColumn)
                .Map((tag, column) => (Tags: Seq(tag), Data: Block(IccTypeSignature.Xyz, toSeq(colorants.Column(column)).Map(static entry => Fixed(entry))))),
            ([IccProfileTag.RedTrc, IccProfileTag.GreenTrc, IccProfileTag.BlueTrc], Block(IccTypeSignature.ParametricCurve, [(uint)curve.Function << 16, .. curve.Parameters.Map(Fixed)])),
        ];
        int count = blocks.Bind(static block => block.Tags).Count;
        int start = HeaderBytes + (sizeof(uint) * (1 + (3 * count)));
        (Seq<uint> table, byte[] body) = blocks.Fold(
            (Table: Seq<uint>(), Body: (byte[])[]),
            (layout, block) => (
                layout.Table.Concat(block.Tags.Bind(tag => Seq((uint)tag, (uint)(start + layout.Body.Length), (uint)block.Data.Length))),
                [.. layout.Body, .. block.Data, .. new byte[-block.Data.Length & 3]]));
        DateTime utc = created.UtcDateTime;
        byte[] profile = [
            .. Words([
                (uint)(start + body.Length), 0, 0x04400000, (uint)IccProfileClass.DisplayDevice, (uint)IccColorSpaceType.Rgb, (uint)IccColorSpaceType.CieXyz,
                (uint)((utc.Year << 16) | utc.Month), (uint)((utc.Day << 16) | utc.Hour), (uint)((utc.Minute << 16) | utc.Second), BinaryPrimitives.ReadUInt32BigEndian("acsp"u8),
                .. new uint[6], (uint)IccRenderingIntent.Perceptual, .. illuminant, .. new uint[12], (uint)count, .. table,
            ]),
            .. body,
        ];
        IccProfileId id = IccProfile.CalculateHash(profile);
        Words([id.Part1, id.Part2, id.Part3, id.Part4]).CopyTo(profile, 84);
        return profile;
    }
}
