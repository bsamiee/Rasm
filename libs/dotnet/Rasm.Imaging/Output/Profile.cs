using System.Buffers.Binary;
using System.Text;
using Rasm.Imaging.ColorManagement;
using SixLabors.ImageSharp.Metadata.Profiles.Cicp;
using SixLabors.ImageSharp.Metadata.Profiles.Icc;
using TinyEXR;
using Wacton.Unicolour;

namespace Rasm.Imaging.Output;

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class ColorTags {
    internal static readonly Seq<(Transfer Transfer, CicpTransferCharacteristics Code, Nits White)> Transfers = [
        (TransferCurve.Linear, CicpTransferCharacteristics.Linear, Nits.ReferenceWhite),
        (TransferCurve.Srgb, CicpTransferCharacteristics.Iec61966_2_1, Nits.ReferenceWhite),
        (GammaExponent.Gamma22, CicpTransferCharacteristics.Gamma2_2, Nits.ReferenceWhite),
        (TransferCurve.Pq, CicpTransferCharacteristics.SmpteSt2084, Nits.FileWhite),
        (TransferCurve.Hlg, CicpTransferCharacteristics.AribStdB67, Nits.FileWhite),
    ];

    private static readonly Configuration Pcs = new(xyzConfig: new XyzConfiguration(new WhitePoint(0.9642, 1.0, 0.8249)));

    internal static Option<CicpProfile> Cicp(ColorEncoding encoding) =>
        (encoding.Gamut.Cicp, Transfers.Find(row => row.Transfer == encoding.Transfer).Map(static row => row.Code))
            .Apply(static (primaries, transfer) => new CicpProfile {
                ColorPrimaries = primaries, TransferCharacteristics = transfer, MatrixCoefficients = CicpMatrixCoefficients.Identity, FullRange = true,
            })
            .As();

    internal static Option<byte[]> Icc(ColorEncoding encoding, DateTimeOffset created) =>
        encoding.Transfer.Switch(
            standard: static transfer => transfer.Curve.Map(
                linear: Some((Function: 0, Parameters: Seq(1d))),
                srgb: Some((Function: 3, Parameters: (1d / 1.055) switch { var gain => Seq(2.4, gain, 1d - gain, 1d / 12.92, 0.04045) })),
                pq: None,
                hlg: None,
                acesCct: None,
                acesCc: None),
            gamma: static transfer => Some((Function: 0, Parameters: Seq<double>(transfer.Exponent))),
            camera: static _ => Option<(int Function, Seq<double> Parameters)>.None)
        .Map(curve => Profile(encoding, curve, created));

    internal static ushort ExifColorSpace(ColorEncoding encoding) =>
        encoding.Gamut == Gamut.StandardRgb && encoding.Transfer == TransferCurve.Srgb ? (ushort)1 : ushort.MaxValue;

    private static byte[] Profile(ColorEncoding encoding, (int Function, Seq<double> Parameters) curve, DateTimeOffset created) {
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
        static ColorMatrix3x3 ToPcs(Configuration source, ColourSpace space) => Gamut.Columns(basis => new Unicolour(source, space, basis).ConvertToConfiguration(Pcs).Xyz.Tuple);
        static Seq<uint> Column(ColorMatrix3x3 matrix, int column) => [Fixed(matrix[0, column]), Fixed(matrix[1, column]), Fixed(matrix[2, column])];

        ColorMatrix3x3 colorants = ToPcs(encoding.Gamut.Configuration, ColourSpace.RgbLinear);
        float[] adaptation = new float[9];
        ToPcs(new Configuration(xyzConfig: new XyzConfiguration(encoding.Gamut.Configuration.Rgb.WhitePoint)), ColourSpace.Xyz).CopyTo(adaptation);
        Seq<uint> illuminant = [Fixed(Pcs.Xyz.WhitePoint.X), Fixed(Pcs.Xyz.WhitePoint.Y), Fixed(Pcs.Xyz.WhitePoint.Z)];
        Seq<(Seq<IccProfileTag> Tags, byte[] Data)> blocks = [
            ([IccProfileTag.ProfileDescription], Text(string.Join(' ', encoding.Gamut.Key, encoding.Transfer.ToValue()))),
            ([IccProfileTag.Copyright], Text("No copyright")),
            ([IccProfileTag.MediaWhitePoint], Block(IccTypeSignature.Xyz, illuminant)),
            ([IccProfileTag.ChromaticAdaptation], Block(IccTypeSignature.S15Fixed16Array, toSeq(adaptation).Map(static entry => Fixed(entry)))),
            ([IccProfileTag.RedMatrixColumn], Block(IccTypeSignature.Xyz, Column(colorants, 0))),
            ([IccProfileTag.GreenMatrixColumn], Block(IccTypeSignature.Xyz, Column(colorants, 1))),
            ([IccProfileTag.BlueMatrixColumn], Block(IccTypeSignature.Xyz, Column(colorants, 2))),
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
