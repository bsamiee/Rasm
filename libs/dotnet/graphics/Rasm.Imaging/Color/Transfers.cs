using System.Buffers.Binary;
using System.Globalization;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;
using SixLabors.ImageSharp.Metadata.Profiles.Cicp;
using SixLabors.ImageSharp.Metadata.Profiles.Icc;
using TinyEXR;
using UnitsNet;
using Wacton.Unicolour;

namespace Rasm.Imaging.Color;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
[ObjectFactory<string>(UseForSerialization = SerializationFrameworks.SystemTextJson)]
[ValidationError<ImagingFault>]
public abstract partial record Transfer : IConvertible<string>, IFixedFunction {
    private const string LinearKey = "linear";
    private const string AcesCcKey = "acescc";
    private const string GammaForm = "gamma";
    private const string PqForm = "pq";
    private const string HlgForm = "hlg";

    public static Seq<Transfer> Known { get; } = [
        new Linear(), Gamma.Srgb, Gamma.Rec709, Gamma.Gamma22, Gamma.Gamma28, Gamma.Rec1886, Pq.High, Pq.Standard, Hlg.High, Hlg.Standard, new AcesCc(),
        Log.ArriLogC3, Log.ArriLogC4, Log.SonySLog2, Log.SonySLog3, Log.CanonLog2, Log.CanonLog3, Log.RedLog3G10, Log.PanasonicVLog, Log.FujifilmFLog, Log.FujifilmFLog2, Log.DjiDLog,
        Log.GoProProtune, Log.NikonNLog, Log.AppleLog, Log.BlackmagicFilmGen5, Log.DaVinciIntermediate, Log.FilmLightTLog, Log.AcesCct,
    ];

    private static readonly FrozenDictionary<string, Transfer> Named = Known.ToFrozenDictionary(static transfer => transfer.ToValue(), StringComparer.Ordinal);

    public virtual DynamicRange Range => DynamicRange.Standard;
    public Luminance White => Luminance.FromNits(Range.WhiteLuminance);
    public virtual Option<CicpTransferCharacteristics> Cicp => None;
    public virtual LutTable Encode => new LutTable.FixedFunction(this);
    public virtual LutTable Decode => new LutTable.FixedFunction(this, Inverted: true);

    public virtual void Apply(Span<Vector4> row, bool inverted) => (inverted ? Decode : Encode).Apply(row);

    public string ToValue() => Switch(
        linear: static _ => LinearKey,
        gamma: static gamma => gamma.Name.IfNone(() => gamma.Offset is 0f
            ? string.Create(CultureInfo.InvariantCulture, $"{GammaForm}:{gamma.Exponent}")
            : string.Create(CultureInfo.InvariantCulture, $"{GammaForm}:{gamma.Exponent}:{gamma.Offset}")),
        pq: static pq => string.Create(CultureInfo.InvariantCulture, $"{PqForm}:{pq.Range.WhiteLuminance}:{pq.Range.MaxLuminance}"),
        hlg: static hlg => string.Create(CultureInfo.InvariantCulture, $"{HlgForm}:{hlg.Range.WhiteLuminance}:{hlg.Range.MaxLuminance}"),
        acesCc: static _ => AcesCcKey,
        log: static log => log.Key);

    public static Fin<Transfer> Read(string value) => Named.TryGetValue(value, out Transfer? named) ? named : Parametric(value, toSeq(value.Split(':')));

    public static ImagingFault? Validate(string? value, IFormatProvider? provider, out Transfer? item) {
        (item, ImagingFault? fault) = Optional(value).Match(
            Some: static text => Read(text).Match(Succ: static transfer => ((Transfer?)transfer, (ImagingFault?)null), Fail: static error => (null, error.Filter<ImagingFault>().Head as ImagingFault)),
            None: static () => (null, null));
        return fault;
    }

    private static Fin<float> Number(string text) =>
        float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out float number) ? number : ImagingFault.Of(ImagingReason.Malformed, text);

    private static Fin<Transfer> Parametric(string value, Seq<string> parts) =>
        parts.Tail.Traverse(Number).As().Bind(levels => (parts[0], levels.Count) switch {
            (GammaForm, 1 or 2) => Gamma.Of(levels[0], levels.Tail.Head.IfNone(0f)).Map(static gamma => (Transfer)gamma),
            (PqForm, 1 or 2) => Pq.Of(Display(levels)).Map(static pq => (Transfer)pq),
            (HlgForm, 1 or 2) => Hlg.Of(Display(levels)).Map(static hlg => (Transfer)hlg),
            _ => ImagingFault.Of(ImagingReason.UnknownFormat, value),
        });

    private static DynamicRange Display(Seq<float> levels) => new(levels[0], levels.Tail.Head.Map(static nits => (double)nits).IfNone(DynamicRange.High.MaxLuminance), DynamicRange.High.MinLuminance);

    private static Fin<DynamicRange> Lit(DynamicRange range) =>
        ImagingFault.Unless(double.IsNormal(range.WhiteLuminance) && double.IsNormal(range.MaxLuminance) && range.WhiteLuminance > 0d && range.MaxLuminance > 0d, ImagingReason.Range, (range.WhiteLuminance, range.MaxLuminance)).Map(_ => range);

    private static void Coded(Span<Vector4> row, TransferFunction curve, bool encode) {
        Span<float> samples = MemoryMarshal.Cast<Vector4, float>(row);
        using SpanOwner<float> alpha = SpanOwner<float>.Allocate(row.Length);
        samples.AsSpan2D(row.Length, 4).GetColumn(3).CopyTo(alpha.Span);
        if (encode) ImageProcessing.EncodeTransfer(samples, samples, curve);
        else ImageProcessing.DecodeTransfer(samples, samples, curve);
        alpha.Span.CopyTo(samples.AsSpan2D(row.Length, 4).GetColumn(3));
    }

    public sealed record Linear : Transfer {
        public override Option<CicpTransferCharacteristics> Cicp => CicpTransferCharacteristics.Linear;
        public override LutTable Encode => LutTable.Of([]);
        public override LutTable Decode => LutTable.Of([]);
    }

    public sealed record Gamma : Transfer {
        private Gamma(float exponent, float offset, Option<CicpTransferCharacteristics> cicp = default, Option<string> name = default) => (Exponent, Offset, Cicp, Name) = (exponent, offset, cicp, name);

        public static Gamma Srgb { get; } = new(2.4f, 0.055f, CicpTransferCharacteristics.Iec61966_2_1, "srgb");
        public static Gamma Rec709 { get; } = new(1f / 0.45f, 0.099f, CicpTransferCharacteristics.ItuRBt709_6, "rec709");
        public static Gamma Gamma22 { get; } = new(2.2f, 0f, CicpTransferCharacteristics.Gamma2_2);
        public static Gamma Gamma28 { get; } = new(2.8f, 0f, CicpTransferCharacteristics.Gamma2_8);
        public static Gamma Rec1886 { get; } = new(2.4f, 0f);

        public float Exponent { get; }
        public float Offset { get; }
        public Option<string> Name { get; }
        public override Option<CicpTransferCharacteristics> Cicp { get; }
        public override LutTable Encode => new LutTable.Exponent(Offset > 0f ? ExponentStyle.MonCurveRev : ExponentStyle.BasicRev, new(Exponent), new(Offset));
        public override LutTable Decode => new LutTable.Exponent(Offset > 0f ? ExponentStyle.MonCurveFwd : ExponentStyle.BasicFwd, new(Exponent), new(Offset));

        public static Fin<Gamma> Of(float exponent, float offset = 0f) =>
            ImagingFault.Unless(float.IsFinite(exponent) && float.IsFinite(offset) && exponent > 0f && offset >= 0f && (offset is 0f || exponent > 1f), ImagingReason.Range, (exponent, offset))
                .Map(_ => Seq(Srgb, Rec709, Gamma22, Gamma28, Rec1886).Find(known => (known.Exponent, known.Offset) == (exponent, offset)).IfNone(() => new Gamma(exponent, offset)));
    }

    public sealed record Pq : Transfer {
        private const double Peak = 10_000d;

        private Pq(DynamicRange range) => Range = range;

        public static Pq Standard { get; } = new(DynamicRange.Standard);
        public static Pq High { get; } = new(DynamicRange.High);

        public override DynamicRange Range { get; }
        public override Option<CicpTransferCharacteristics> Cicp => CicpTransferCharacteristics.SmpteSt2084;

        public static Fin<Pq> Of(DynamicRange range) => Lit(range).Map(static lit => new Pq(lit));

        public override void Apply(Span<Vector4> row, bool inverted) {
            float scale = (float)(inverted ? Peak / Range.WhiteLuminance : Range.WhiteLuminance / Peak);
            if (inverted) Coded(row, TransferFunction.Pq, encode: false);
            foreach (ref Vector4 pixel in row) pixel = new Vector4(pixel.AsVector3() * scale, pixel.W);
            if (!inverted) Coded(row, TransferFunction.Pq, encode: true);
        }
    }

    public sealed record Hlg : Transfer {
        private const float Black = 1e-4f;

        private Hlg(DynamicRange range) => Range = range;

        public static Hlg Standard { get; } = new(DynamicRange.Standard);
        public static Hlg High { get; } = new(DynamicRange.High);

        public override DynamicRange Range { get; }
        public override Option<CicpTransferCharacteristics> Cicp => CicpTransferCharacteristics.AribStdB67;

        public static Fin<Hlg> Of(DynamicRange range) => Lit(range).Map(static lit => new Hlg(lit));

        public override void Apply(Span<Vector4> row, bool inverted) {
            (Vector3 weights, float gamma, float scale) = (Gamut.Rec2020.Luminance, 1.2f + (0.42f * float.Log10((float)Range.MaxLuminance / 1_000f)), (float)(Range.WhiteLuminance / Range.MaxLuminance));
            if (inverted) Coded(row, TransferFunction.Hlg, encode: false);
            foreach (ref Vector4 pixel in row) {
                float luminance = Vector3.Dot(pixel.AsVector3(), weights);
                pixel = inverted
                    ? new Vector4(pixel.AsVector3() * (float.Pow(float.Max(luminance, float.Pow(Black, 1f / gamma) / 3f), gamma - 1f) / scale), pixel.W)
                    : new Vector4(pixel.AsVector3() * scale * float.Pow(float.Max(luminance * scale, Black / float.Pow(3f, gamma)), (1f - gamma) / gamma), pixel.W);
            }
            if (!inverted) Coded(row, TransferFunction.Hlg, encode: true);
        }
    }

    public sealed record AcesCc : Transfer {
        public override void Apply(Span<Vector4> row, bool inverted) {
            foreach (ref Vector4 pixel in row) pixel = new Vector4(Coded(pixel.X), Coded(pixel.Y), Coded(pixel.Z), pixel.W);

            float Coded(float value) => (float)(inverted ? RgbModels.Acescc.ToLinear(value) : RgbModels.Acescc.FromLinear(value));
        }
    }

    public sealed record Log : Transfer {
        private readonly LogCurve curve;
        private readonly Option<(float Joint, Either<LogCurve, (float Scale, float Shift, float Exponent)> Below)> lower;

        private Log(string key, LogCurve curve, Option<(float Joint, Either<LogCurve, (float Scale, float Shift, float Exponent)> Below)> lower = default) => (Key, this.curve, this.lower) = (key, curve, lower);

        public static Log ArriLogC3 { get; } = new("arri-logc3", LogCurve.Of(10f, new(0.247189638318671f), new(0.385536998692443f), new(5.55555555555556f), new(0.0522722750251688f), new(0.0105909904954696f)));
        public static Log ArriLogC4 { get; } = new("arri-logc4", LogCurve.Of(2f, new(0.0647954196341293f), new(-0.295908392682586f), new(2231.82630906769f), new(64f), new(-0.0180569961199113f)));
        public static Log SonySLog2 { get; } = new("sony-slog2", LogCurve.Of(10f, new(0.432699f * 876f / 1023f), new((0.646596f * 876f / 1023f) + (64f / 1023f)), new(155f / (219f * 0.9f)), new(0.037584f), Vector3.Zero));
        public static Log SonySLog3 { get; } = new("sony-slog3", LogCurve.Of(10f, new(261.5f / 1023f), new(420f / 1023f), new(1f / 0.19f), new(0.01f / 0.19f), new(0.01125f), new Vector3((171.2102946929f - 95f) / 0.01125f / 1023f)));
        public static Log CanonLog2 { get; } = new("canon-log2", new(10f, new(0.24136077f), new(0.092864125f), new(87.099375f / 0.9f), Vector3.One), (0f, new LogCurve(10f, new(-0.24136077f), new(0.092864125f), new(-87.099375f / 0.9f), Vector3.One)));
        public static Log CanonLog3 { get; } = new(
            "canon-log3", LogCurve.Of(10f, new(0.36726845f), new(0.12240537f), new(14.98325f / 0.9f), Vector3.One, new(0.014f * 0.9f), new Vector3(1.9754798f / 0.9f)),
            (-0.014f * 0.9f, new LogCurve(10f, new(-0.36726845f), new(0.12783901f), new(-14.98325f / 0.9f), Vector3.One)));
        public static Log RedLog3G10 { get; } = new("red-log3g10", LogCurve.Of(10f, new(0.224282f), Vector3.Zero, new(155.975327f), new((0.01f * 155.975327f) + 1f), new(-0.01f)));
        public static Log PanasonicVLog { get; } = new("panasonic-vlog", LogCurve.Of(10f, new(0.241514f), new(0.598206f), Vector3.One, new(0.00873f), new(0.01f)));
        public static Log FujifilmFLog { get; } = new("fujifilm-flog", LogCurve.Of(10f, new(0.344676f), new(0.790453f), new(0.555556f), new(0.009468f), new(0.00089f), new Vector3(8.735631f)));
        public static Log FujifilmFLog2 { get; } = new("fujifilm-flog2", LogCurve.Of(10f, new(0.245281f), new(0.384316f), new(5.555556f), new(0.064829f), new(0.000889f), new Vector3(8.799461f)));
        public static Log DjiDLog { get; } = new("dji-dlog", LogCurve.Of(10f, new(0.256662970719888f), new(0.58455504907396f), new(0.9892f), new(0.0108f), new(0.00758078675f)));
        public static Log GoProProtune { get; } = new("gopro-protune", new(113f, Vector3.One, Vector3.Zero, new(112f), Vector3.One));
        public static Log NikonNLog { get; } = new("nikon-nlog", new(float.E, new(150f / 1023f), new(619f / 1023f), Vector3.One, Vector3.Zero), (0.328f, (650f / 1023f, 0.0075f, 1f / 3f)));
        public static Log AppleLog { get; } = new("apple-log", new(2f, new(0.08550479f), new(0.69336945f), Vector3.One, new(0.00964052f)), (0.01f, (47.28711236f, 0.05641088f, 2f)));
        public static Log BlackmagicFilmGen5 { get; } = new("blackmagic-film-gen5", LogCurve.Of(float.E, new(0.0869287606549122f), new(0.530013339229194f), Vector3.One, new(0.00549407243225781f), new(0.005f)));
        public static Log DaVinciIntermediate { get; } = new("davinci-intermediate", LogCurve.Of(2f, new(0.07329248f), new(0.07329248f * 7f), Vector3.One, new(0.0075f), new(0.00262409f), new Vector3(10.44426855f)));
        public static Log FilmLightTLog { get; } = ((1f - 0.075f) / (16f * (0.7107f + (1.2359f * float.Log(128f * 16f))))) switch {
            var offset => ((1f - 0.075f) / float.Log(1f + (128f / offset))) switch {
                var slope => new Log("filmlight-tlog", LogCurve.Of(float.E, new(slope), new(1f - (slope * float.Log(128f + offset))), Vector3.One, new(offset), Vector3.Zero)),
            },
        };
        public static Log AcesCct { get; } = new("acescct", LogCurve.Of(2f, new(1f / 17.52f), new(9.72f / 17.52f), Vector3.One, Vector3.Zero, new(1f / 128f)));

        public string Key { get; }
        public override LutTable Encode => lower.IsNone ? new LutTable.Log(curve) : base.Encode;
        public override LutTable Decode => lower.IsNone ? new LutTable.Log(curve, Inverted: true) : base.Decode;

        public override void Apply(Span<Vector4> row, bool inverted) {
            foreach (ref Vector4 pixel in row) pixel = new Vector4(inverted ? Decoded(pixel.AsVector3()) : Encoded(pixel.AsVector3()), pixel.W);
        }

        private Vector3 Encoded(Vector3 linear) =>
            lower.Match(
                Some: piece => Vector3.ConditionalSelect(
                    Vector3.LessThan(linear, new Vector3(piece.Joint)),
                    piece.Below.Match(Left: below => below.Forward(linear), Right: power => power.Scale * Vector3.Exp(power.Exponent * Vector3.Log(Vector3.Max(linear + new Vector3(power.Shift), Vector3.Zero)))),
                    curve.Forward(linear)),
                None: () => curve.Forward(linear));

        private Vector3 Decoded(Vector3 code) =>
            lower.Match(
                Some: piece => Vector3.ConditionalSelect(
                    Vector3.LessThan(code, curve.Forward(new Vector3(piece.Joint))),
                    piece.Below.Match(Left: below => below.Inverse(code), Right: power => Vector3.Exp(Vector3.Log(Vector3.Max(code / power.Scale, Vector3.Zero)) / power.Exponent) - new Vector3(power.Shift)),
                    curve.Inverse(code)),
                None: () => curve.Inverse(code));
    }
}

public sealed record ColorEncoding(Gamut Gamut, Transfer Transfer) {
    private static readonly Configuration Pcs = new(xyzConfig: new XyzConfiguration(new WhitePoint(0.9642, 1.0, 0.8249)));

    public Option<CicpProfile> Cicp =>
        (Gamut.Cicp, Transfer.Cicp).Apply(static (primaries, transfer) => new CicpProfile {
            ColorPrimaries = primaries, TransferCharacteristics = transfer, MatrixCoefficients = CicpMatrixCoefficients.Identity, FullRange = true,
        }).As();

    public static Fin<ColorEncoding> Of(CicpProfile cicp) =>
        (ImagingFault.Unless(cicp is { MatrixCoefficients: CicpMatrixCoefficients.Identity, FullRange: true }, ImagingReason.Unsupported, (cicp.MatrixCoefficients, cicp.FullRange)),
         Gamut.Known.Find(gamut => gamut.Cicp == Some(cicp.ColorPrimaries)).ToFin(ImagingFault.Of(ImagingReason.UnknownFormat, cicp.ColorPrimaries)),
         Transfer.Known.Find(transfer => transfer.Cicp == Some(cicp.TransferCharacteristics is CicpTransferCharacteristics.ItuRBt601_7 or CicpTransferCharacteristics.ItuRBt2020_2_10bit or CicpTransferCharacteristics.ItuRBt2020_2_12bit
                 ? CicpTransferCharacteristics.ItuRBt709_6
                 : cicp.TransferCharacteristics)).ToFin(ImagingFault.Of(ImagingReason.UnknownFormat, cicp.TransferCharacteristics)))
            .Apply(static (_, gamut, transfer) => new ColorEncoding(gamut, transfer)).As();

    public LutTable To(ColorEncoding target) =>
        this == target
            ? LutTable.Of([])
            : LutTable.Of([
                Transfer.Decode,
                .. Gamut == target.Gamut && Transfer.White == target.Transfer.White
                    ? Seq<LutTable>()
                    : Seq<LutTable>(new LutTable.Affine(Gamut.MatrixTo(target.Gamut) * Matrix4x4.CreateScale((float)(Transfer.White / target.Transfer.White).ToDouble()))),
                target.Transfer.Encode,
            ]);

    public Option<IccProfile> Icc(DateTimeOffset created) =>
        Transfer.Switch(
            linear: static _ => Some((Function: 0u, Parameters: Seq(1f))),
            gamma: static gamma => gamma.Offset is 0f
                ? Some((Function: 0u, Parameters: Seq(gamma.Exponent)))
                : ExponentStyle.Joint(new Vector3(gamma.Exponent), new Vector3(gamma.Offset)) switch {
                    var (knee, slope) => Some((Function: 3u, Parameters: Seq(gamma.Exponent, 1f / (1f + gamma.Offset), gamma.Offset / (1f + gamma.Offset), slope.X, knee.X))),
                },
            pq: static _ => None,
            hlg: static _ => None,
            acesCc: static _ => None,
            log: static _ => None)
        .Map(curve => new IccProfile(Profile(curve.Function, curve.Parameters, created)));

    private byte[] Profile(uint function, Seq<float> parameters, DateTimeOffset created) {
        const int HeaderBytes = 128;
        static uint Fixed(float value) => unchecked((uint)(int)double.Round(value * 65536d, MidpointRounding.ToEven));
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
        static Seq<uint> Row(Vector3 row) => [Fixed(row.X), Fixed(row.Y), Fixed(row.Z)];

        Matrix4x4 colorants = Gamut.Columns(Gamut.Configuration, ColourSpace.RgbLinear, Pcs, ColourSpace.Xyz);
        Matrix4x4 adaptation = Matrix4x4.Transpose(Gamut.Columns(new Configuration(xyzConfig: new XyzConfiguration(Gamut.Rgb.WhitePoint)), ColourSpace.Xyz, Pcs, ColourSpace.Xyz));
        Seq<uint> illuminant = Row(new Vector3((float)Pcs.Xyz.WhitePoint.X, (float)Pcs.Xyz.WhitePoint.Y, (float)Pcs.Xyz.WhitePoint.Z));
        Seq<(Seq<IccProfileTag> Tags, byte[] Data)> blocks = [
            ([IccProfileTag.ProfileDescription], Text(string.Join(' ', Gamut.Rgb.Name, Transfer.ToValue()))),
            ([IccProfileTag.Copyright], Text("No copyright")),
            ([IccProfileTag.MediaWhitePoint], Block(IccTypeSignature.Xyz, illuminant)),
            ([IccProfileTag.ChromaticAdaptation], Block(IccTypeSignature.S15Fixed16Array, [.. Row(adaptation.X.AsVector3()), .. Row(adaptation.Y.AsVector3()), .. Row(adaptation.Z.AsVector3())])),
            ([IccProfileTag.RedMatrixColumn], Block(IccTypeSignature.Xyz, Row(colorants.X.AsVector3()))),
            ([IccProfileTag.GreenMatrixColumn], Block(IccTypeSignature.Xyz, Row(colorants.Y.AsVector3()))),
            ([IccProfileTag.BlueMatrixColumn], Block(IccTypeSignature.Xyz, Row(colorants.Z.AsVector3()))),
            ([IccProfileTag.RedTrc, IccProfileTag.GreenTrc, IccProfileTag.BlueTrc], Block(IccTypeSignature.ParametricCurve, [function << 16, .. parameters.Map(Fixed)])),
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
