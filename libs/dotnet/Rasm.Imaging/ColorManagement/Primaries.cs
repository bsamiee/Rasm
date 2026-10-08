using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using Rasm.Imaging.Pixels;
using SixLabors.ImageSharp.Metadata.Profiles.Cicp;
using TinyEXR;
using Wacton.Unicolour;

namespace Rasm.Imaging.ColorManagement;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum<string>]
[ValidationError<InvalidColor>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class Gamut {
    public static readonly Gamut StandardRgb = new("standard-rgb", Configuration.Default, Some(CicpColorPrimaries.ItuRBt709_6), Some("lin_rec709_scene"));
    public static readonly Gamut DisplayP3 = new("display-p3", new Configuration(rgbConfig: RgbConfiguration.DisplayP3), Some(CicpColorPrimaries.SmpteEg432_1), Some("lin_p3d65_scene"));
    public static readonly Gamut Rec2020 = new("rec2020", new Configuration(rgbConfig: RgbConfiguration.Rec2020), Some(CicpColorPrimaries.ItuRBt2020_2), Some("lin_rec2020_scene"));
    public static readonly Gamut Acescg = new("acescg", new Configuration(rgbConfig: RgbConfiguration.Acescg), None, Some("lin_ap1_scene"));
    public static readonly Gamut Aces20651 = new("aces20651", new Configuration(rgbConfig: RgbConfiguration.Aces20651), None, Some("lin_ap0_scene"));
    public static readonly Gamut ProPhoto = new("pro-photo", new Configuration(rgbConfig: RgbConfiguration.ProPhoto), None, None);
    public static readonly Gamut EGamut = new("e-gamut", Linear(new(0.8, 0.3177), new(0.18, 0.9), new(0.065, -0.0805)), None, Some("blender:lin_egamut_scene"));
    public static readonly Gamut ArriWideGamut3 = new("arri-wide-gamut-3", Linear(new(0.684, 0.313), new(0.221, 0.848), new(0.0861, -0.102)), None, None);
    public static readonly Gamut ArriWideGamut4 = new("arri-wide-gamut-4", Linear(new(0.7347, 0.2653), new(0.1424, 0.8576), new(0.0991, -0.0308)), None, None);
    public static readonly Gamut SGamut3 = new("s-gamut3", Linear(new(0.73, 0.28), new(0.14, 0.855), new(0.1, -0.05)), None, None);
    public static readonly Gamut SGamut3Cine = new("s-gamut3-cine", Linear(new(0.766, 0.275), new(0.225, 0.8), new(0.089, -0.087)), None, None);
    public static readonly Gamut VeniceSGamut3 = new(
        "venice-s-gamut3",
        Linear(new(0.740464264304292, 0.27936437475066), new(0.089241145423286, 0.893809528608105), new(0.110488236673827, -0.052579333080476)),
        None,
        None);
    public static readonly Gamut VeniceSGamut3Cine = new(
        "venice-s-gamut3-cine",
        Linear(new(0.775901871567345, 0.274502392854799), new(0.188682902773355, 0.828684937020288), new(0.101337382499301, -0.089187517306263)),
        None,
        None);
    public static readonly Gamut CinemaGamut = new("cinema-gamut", Linear(new(0.74, 0.27), new(0.17, 1.14), new(0.08, -0.1)), None, None);
    public static readonly Gamut RedWideGamutRgb = new("red-wide-gamut-rgb", Linear(new(0.780308, 0.304253), new(0.121595, 1.493994), new(0.095612, -0.084589)), None, None);
    public static readonly Gamut VGamut = new("v-gamut", Linear(new(0.73, 0.28), new(0.165, 0.84), new(0.1, -0.03)), None, None);
    public static readonly Gamut DGamut = new("d-gamut", Linear(new(0.71, 0.31), new(0.21, 0.88), new(0.09, -0.08)), None, None);
    public static readonly Gamut ProtuneNative = new(
        "protune-native",
        Linear(new(0.698480461493841, 0.193026445370121), new(0.329555378387345, 1.024596624134644), new(0.108442631407675, -0.034678569754016)),
        None,
        None);
    public static readonly Gamut BlackmagicWideGamut = new("blackmagic-wide-gamut", Linear(new(0.7177215, 0.3171181), new(0.228041, 0.861569), new(0.1005841, -0.0820452)), None, None);
    public static readonly Gamut DaVinciWideGamut = new("davinci-wide-gamut", Linear(new(0.8, 0.313), new(0.1682, 0.9877), new(0.079, -0.1155)), None, None);
    public static readonly Gamut AppleWideGamut = new("apple-wide-gamut", Linear(new(0.725, 0.301), new(0.221, 0.814), new(0.068, -0.076)), None, None);

    public static readonly Memo<HashMap<CicpColorPrimaries, Gamut>> ByCicp = memo(static () => toHashMap(toSeq(Items).Choose(static gamut => gamut.Cicp.Map(code => (code, gamut)))));

    public Configuration Configuration { get; }
    public Option<CicpColorPrimaries> Cicp { get; }
    public Option<string> InteropId { get; }

    public Chromaticities Chromaticities => new(
        (float)Configuration.Rgb.ChromaticityR.X, (float)Configuration.Rgb.ChromaticityR.Y,
        (float)Configuration.Rgb.ChromaticityG.X, (float)Configuration.Rgb.ChromaticityG.Y,
        (float)Configuration.Rgb.ChromaticityB.X, (float)Configuration.Rgb.ChromaticityB.Y,
        (float)Configuration.Rgb.WhitePoint.Chromaticity.X, (float)Configuration.Rgb.WhitePoint.Chromaticity.Y);

    public ColorMatrix3x3 ToXyz => Columns(basis => new Unicolour(Configuration, ColourSpace.RgbLinear, basis).Xyz.Tuple);

    public ColorMatrix3x3 FromXyz => Columns(basis => new Unicolour(Configuration, ColourSpace.Xyz, basis).RgbLinear.Tuple);

    public Vector3 Luminance => ToXyz switch {
        var matrix => new(matrix.M21, matrix.M22, matrix.M23),
    };

    public (Vector3 Red, Vector3 Green, Vector3 Blue) XyzColumns => ToXyz switch {
        var matrix => (new(matrix.M11, matrix.M21, matrix.M31), new(matrix.M12, matrix.M22, matrix.M32), new(matrix.M13, matrix.M23, matrix.M33)),
    };

    public ColorMatrix3x3 MatrixTo(Gamut target) => Between(Configuration, target.Configuration);

    public ColorMatrix3x3 Adapting(WhitePoint source, WhitePoint destination) =>
        (new Configuration(xyzConfig: new XyzConfiguration(source)), new Configuration(xyzConfig: new XyzConfiguration(destination))) switch {
            var (from, to) => Columns(basis => new Unicolour(
                Configuration, ColourSpace.Xyz,
                new Unicolour(from, ColourSpace.Xyz, new Unicolour(Configuration, ColourSpace.RgbLinear, basis).Xyz.Tuple).ConvertToConfiguration(to).Xyz.Tuple).RgbLinear.Tuple),
        };

    public static Option<Gamut> Of(Chromaticities stated) {
        static Vector256<float> Coordinates(Chromaticities c) => Vector256.Create(c.RedX, c.RedY, c.GreenX, c.GreenY, c.BlueX, c.BlueY, c.WhiteX, c.WhiteY);
        return toSeq(Items).Find(gamut => Vector256.LessThanOrEqualAll(Vector256.Abs(Coordinates(gamut.Chromaticities) - Coordinates(stated)), Vector256.Create(1e-4f)));
    }

    public static ColorMatrix3x3 Between(Configuration from, Configuration to) =>
        Columns(basis => new Unicolour(from, ColourSpace.RgbLinear, basis).ConvertToConfiguration(to).RgbLinear.Tuple);

    internal static ColorMatrix3x3 Columns(Func<(double, double, double), (double, double, double)> column) =>
        (column((1d, 0d, 0d)), column((0d, 1d, 0d)), column((0d, 0d, 1d))) switch {
            var ((r1, g1, b1), (r2, g2, b2), (r3, g3, b3)) =>
                new((float)r1, (float)r2, (float)r3, (float)g1, (float)g2, (float)g3, (float)b1, (float)b2, (float)b3),
        };

    private static Configuration Linear(Chromaticity red, Chromaticity green, Chromaticity blue) =>
        new(rgbConfig: new RgbConfiguration(red, green, blue, Illuminant.D65.GetWhitePoint(Observer.Degree2), static linear => linear, static linear => linear));
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidColor>]
public readonly partial struct Nits : IMinMaxValue<Nits> {
    public static Nits MinValue { get; } = new(1e-6f);
    public static Nits MaxValue { get; } = new(10_000f);
    public static Nits ReferenceWhite { get; } = new(100f);
    public static Nits FileWhite { get; } = new(203f);
    public static Nits HlgPeak { get; } = new(1_000f);

    static partial void ValidateFactoryArguments(ref InvalidColor? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidColor();
}

[ValueObject<float>(AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidColor>]
public readonly partial struct GammaExponent : IMinMaxValue<GammaExponent> {
    public static GammaExponent MinValue { get; } = new(float.Epsilon);
    public static GammaExponent MaxValue { get; } = new(float.MaxValue);
    public static GammaExponent Gamma22 { get; } = new(2.2f);
    public static GammaExponent Rec1886 { get; } = new(2.4f);

    static partial void ValidateFactoryArguments(ref InvalidColor? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidColor();
}

[SmartEnum<string>]
[ValidationError<InvalidColor>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class TransferCurve {
    public static readonly TransferCurve Linear = new("linear", static (_, _) => { }, static (_, _) => { });
    public static readonly TransferCurve Srgb = new(
        "srgb",
        static (row, _) => {
            foreach (ref Vector4 pixel in row)
                ImageProcessing.EncodeTransfer(Lanes(ref pixel), Lanes(ref pixel), TransferFunction.Srgb);
        },
        static (row, _) => {
            foreach (ref Vector4 pixel in row)
                ImageProcessing.DecodeTransfer(Lanes(ref pixel), Lanes(ref pixel), TransferFunction.Srgb);
        });
    public static readonly TransferCurve Pq = new(
        "pq",
        static (row, white) => {
            float scale = (float)white / (float)Nits.MaxValue;
            foreach (ref Vector4 pixel in row) {
                pixel = new Vector4(pixel.AsVector3() * scale, pixel.W);
                ImageProcessing.EncodeTransfer(Lanes(ref pixel), Lanes(ref pixel), TransferFunction.Pq);
            }
        },
        static (row, white) => {
            float scale = (float)Nits.MaxValue / (float)white;
            foreach (ref Vector4 pixel in row) {
                ImageProcessing.DecodeTransfer(Lanes(ref pixel), Lanes(ref pixel), TransferFunction.Pq);
                pixel = new Vector4(pixel.AsVector3() * scale, pixel.W);
            }
        });
    public static readonly TransferCurve Hlg = new(
        "hlg",
        static (row, white) => {
            float scale = (float)white / (float)Nits.HlgPeak;
            foreach (ref Vector4 pixel in row) {
                Vector3 display = pixel.AsVector3() * scale;
                pixel = new Vector4(display * float.Pow(float.Max(Vector3.Dot(display, HlgSystem.Weights), HlgSystem.DisplayFloor), (1f - HlgSystem.Gamma) / HlgSystem.Gamma), pixel.W);
                ImageProcessing.EncodeTransfer(Lanes(ref pixel), Lanes(ref pixel), TransferFunction.Hlg);
            }
        },
        static (row, white) => {
            float scale = (float)Nits.HlgPeak / (float)white;
            foreach (ref Vector4 pixel in row) {
                ImageProcessing.DecodeTransfer(Lanes(ref pixel), Lanes(ref pixel), TransferFunction.Hlg);
                Vector3 scene = pixel.AsVector3();
                pixel = new Vector4(scene * float.Pow(float.Max(Vector3.Dot(scene, HlgSystem.Weights), HlgSystem.SceneFloor), HlgSystem.Gamma - 1f) * scale, pixel.W);
            }
        });
    public static readonly TransferCurve AcesCct = new("acescct", static (row, _) => PerLane(row, RgbModels.Acescct.FromLinear), static (row, _) => PerLane(row, RgbModels.Acescct.ToLinear));
    public static readonly TransferCurve AcesCc = new("acescc", static (row, _) => PerLane(row, RgbModels.Acescc.FromLinear), static (row, _) => PerLane(row, RgbModels.Acescc.ToLinear));

    private static readonly (Vector3 Weights, float Gamma, float SceneFloor, float DisplayFloor) HlgSystem =
        (1.2f + (0.42f * float.Log10((float)Nits.HlgPeak / 1_000f))) switch {
            var gamma => (Gamut.Rec2020.Luminance, gamma, float.Pow(1e-4f, 1f / gamma) / 3f, 1e-4f / float.Pow(3f, gamma)),
        };

    [UseDelegateFromConstructor]
    public partial void Encode(Span<Vector4> row, Nits white);

    [UseDelegateFromConstructor]
    public partial void Decode(Span<Vector4> row, Nits white);

    [SuppressMessage("Performance", "CA1517:Prefer ReadOnlySpan over Span", Justification = "Each pixel is rewritten through the foreach ref local, a write the analyzer does not read")]
    internal static void PerLane(Span<Vector4> row, Func<double, double> curve) {
        foreach (ref Vector4 pixel in row)
            pixel = new Vector4((float)curve(pixel.X), (float)curve(pixel.Y), (float)curve(pixel.Z), pixel.W);
    }

    private static Span<float> Lanes(ref Vector4 pixel) => MemoryMarshal.CreateSpan(ref pixel.X, 3);
}

[Union]
[ValidationError<InvalidColor>]
[ObjectFactory<string>]
public abstract partial record Transfer : IConvertible<string> {
    private const string GammaForm = "gamma";

    public static Seq<Transfer> Named { get; } = [.. TransferCurve.Items, GammaExponent.Gamma22, GammaExponent.Rec1886, .. CameraLog.Items];

    public abstract void Encode(Span<Vector4> row, Nits white);
    public abstract void Decode(Span<Vector4> row, Nits white);

    public static InvalidColor? Validate(string? value, IFormatProvider? provider, out Transfer? item) {
        item = value?.Split(':', 2) switch {
            null => null,
            [GammaForm, var exponent] => GammaExponent.TryParse(exponent, CultureInfo.InvariantCulture, out GammaExponent parsed) ? new Gamma(parsed) : null,
            _ => TransferCurve.TryGet(value, out TransferCurve? curve) ? new Standard(curve) : CameraLog.TryGet(value, out CameraLog? log) ? new Camera(log) : null,
        };
        return value is not null && item is null ? new InvalidColor() : null;
    }

    public string ToValue() => Switch(
        standard: static transfer => transfer.Curve.Key,
        gamma: static transfer => string.Create(CultureInfo.InvariantCulture, $"{GammaForm}:{transfer.Exponent}"),
        camera: static transfer => transfer.Log.Key);

    public PixelFrame Light(PixelFrame frame) =>
        new(frame.Origin, frame.Size, frame.Extent, block => {
            frame.Block.CopyTo(block, 0);
            Decode(MemoryMarshal.Cast<float, Vector4>(block.AsSpan()), Nits.ReferenceWhite);
        });

    public int[] Displayed(PixelFrame frame) {
        Span<Vector4> light = MemoryMarshal.Cast<float, Vector4>(Light(frame).Block.AsSpan());
        TransferCurve.Srgb.Encode(light, Nits.ReferenceWhite);
        foreach (ref Vector4 pixel in light)
            pixel = new Vector4(pixel.Z, pixel.Y, pixel.X, pixel.W);
        int[] words = GC.AllocateUninitializedArray<int>(light.Length);
        PixelConversion.FloatToByte(MemoryMarshal.Cast<Vector4, float>(light), MemoryMarshal.AsBytes(words.AsSpan()), PixelConversionMode.Normalized);
        return words;
    }

    public sealed record Standard(TransferCurve Curve) : Transfer {
        public override void Encode(Span<Vector4> row, Nits white) => Curve.Encode(row, white);
        public override void Decode(Span<Vector4> row, Nits white) => Curve.Decode(row, white);
    }

    public sealed record Gamma(GammaExponent Exponent) : Transfer {
        public override void Encode(Span<Vector4> row, Nits white) => TransferCurve.PerLane(row, linear => double.Pow(double.Max(linear, 0d), 1d / Exponent));
        public override void Decode(Span<Vector4> row, Nits white) => TransferCurve.PerLane(row, signal => double.Pow(double.Max(signal, 0d), Exponent));
    }

    public sealed record Camera(CameraLog Log) : Transfer {
        public override void Encode(Span<Vector4> row, Nits white) => TransferCurve.PerLane(row, Log.Encode);
        public override void Decode(Span<Vector4> row, Nits white) => TransferCurve.PerLane(row, Log.Decode);
    }
}

public sealed record ColorEncoding(Gamut Gamut, Transfer Transfer, Nits White) {
    public void Encode(Span<Vector4> row) => Transfer.Encode(row, White);
    public void Decode(Span<Vector4> row) => Transfer.Decode(row, White);

    public Seq<Action<Span<Vector4>>> To(ColorEncoding target) =>
        this == target
            ? Seq<Action<Span<Vector4>>>()
            : (Transfer == TransferCurve.Linear ? Seq<Action<Span<Vector4>>>() : Seq(Decode))
              + (((float)White / (float)target.White) switch {
                  1f when Gamut == target.Gamut => Seq<Action<Span<Vector4>>>(),
                  var white => Gamut.MatrixTo(target.Gamut) switch {
                      var matrix => Seq(new LutTable.Affine(new ColorMatrix3x3(
                          white * matrix.M11, white * matrix.M12, white * matrix.M13,
                          white * matrix.M21, white * matrix.M22, white * matrix.M23,
                          white * matrix.M31, white * matrix.M32, white * matrix.M33), Vector3.Zero).Apply),
                  },
              })
              + (target.Transfer == TransferCurve.Linear ? Seq<Action<Span<Vector4>>>() : Seq(target.Encode));
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidColor>]
public readonly partial struct CompressionThreshold : IMinMaxValue<CompressionThreshold> {
    public static CompressionThreshold MinValue { get; } = new(0f);
    public static CompressionThreshold MaxValue { get; } = new(0.9995f);
    public static CompressionThreshold ReferenceCyan { get; } = new(0.815f);
    public static CompressionThreshold ReferenceMagenta { get; } = new(0.803f);
    public static CompressionThreshold ReferenceYellow { get; } = new(0.880f);

    static partial void ValidateFactoryArguments(ref InvalidColor? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidColor();
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidColor>]
public readonly partial struct CompressionLimit : IMinMaxValue<CompressionLimit> {
    public static CompressionLimit MinValue { get; } = new(1.001f);
    public static CompressionLimit MaxValue { get; } = new(65504f);
    public static CompressionLimit ReferenceCyan { get; } = new(1.147f);
    public static CompressionLimit ReferenceMagenta { get; } = new(1.264f);
    public static CompressionLimit ReferenceYellow { get; } = new(1.312f);

    static partial void ValidateFactoryArguments(ref InvalidColor? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidColor();
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidColor>]
public readonly partial struct CompressionPower : IMinMaxValue<CompressionPower> {
    public static CompressionPower MinValue { get; } = new(1f);
    public static CompressionPower MaxValue { get; } = new(65504f);
    public static CompressionPower Reference { get; } = new(1.2f);

    static partial void ValidateFactoryArguments(ref InvalidColor? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidColor();
}

public sealed record GamutCompression(
    Option<Gamut> Target,
    CompressionThreshold CyanThreshold, CompressionThreshold MagentaThreshold, CompressionThreshold YellowThreshold,
    CompressionLimit CyanLimit, CompressionLimit MagentaLimit, CompressionLimit YellowLimit,
    CompressionPower Power) : IStateRecord<GamutCompression, GamutCompressionParameter, InvalidColor>, IPixelStage<GamutCompression> {
    public static GamutCompression Default { get; } = new(
        None,
        CompressionThreshold.ReferenceCyan, CompressionThreshold.ReferenceMagenta, CompressionThreshold.ReferenceYellow,
        CompressionLimit.ReferenceCyan, CompressionLimit.ReferenceMagenta, CompressionLimit.ReferenceYellow,
        CompressionPower.Reference);

    public static Option<PixelPass> Pass(GamutCompression state, PassContext context) {
        static float Scale(float threshold, float limit, float power) =>
            (limit - threshold) / float.Pow(float.Pow((1f - threshold) / (limit - threshold), -power) - 1f, 1f / power);

        static float Compress(float component, float ach, (float Threshold, float Scale) curve, float power) =>
            ach == 0f ? 0f : ((ach - component) / float.Abs(ach)) switch {
                var distance when distance < curve.Threshold => component,
                var distance => ach - (float.Abs(ach) * (curve.Threshold + ((distance - curve.Threshold) / float.Pow(1f + float.Pow((distance - curve.Threshold) / curve.Scale, power), 1f / power)))),
            };

        Gamut target = state.Target.IfNone(context.Working);
        ColorMatrix3x3 inward = context.Working.MatrixTo(target);
        ColorMatrix3x3 outward = target.MatrixTo(context.Working);
        float power = state.Power;
        (float Threshold, float Scale) cyan = (state.CyanThreshold, Scale(state.CyanThreshold, state.CyanLimit, power));
        (float Threshold, float Scale) magenta = (state.MagentaThreshold, Scale(state.MagentaThreshold, state.MagentaLimit, power));
        (float Threshold, float Scale) yellow = (state.YellowThreshold, Scale(state.YellowThreshold, state.YellowLimit, power));
        return Some<PixelPass>(new PixelPass.Color(row => {
            Span<float> lanes = MemoryMarshal.Cast<Vector4, float>(row);
            ImageProcessing.ApplyColorMatrix(lanes, lanes, 4, inward);
            foreach (ref Vector4 pixel in row) {
                float ach = float.Max(pixel.X, float.Max(pixel.Y, pixel.Z));
                pixel = new Vector4(Compress(pixel.X, ach, cyan, power), Compress(pixel.Y, ach, magenta, power), Compress(pixel.Z, ach, yellow, power), pixel.W);
            }
            ImageProcessing.ApplyColorMatrix(lanes, lanes, 4, outward);
        }));
    }
}

[SmartEnum<string>]
[ValidationError<InvalidColor>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class GamutCompressionParameter : IStateParameter<GamutCompression> {
    private static readonly Presentation<CompressionThreshold, float> ThresholdTrack = new() { Step = 0.001f, Decimals = 3 };
    private static readonly Presentation<CompressionLimit, float> LimitTrack = new() { Soft = (CompressionLimit.MinValue, 2f), Step = 0.001f, Decimals = 3 };

    public static readonly GamutCompressionParameter Target = new("target", new StateParameter<GamutCompression>.OptionalChoice<Gamut, InvalidColor>(
        Lens<GamutCompression, Option<Gamut>>.New(static state => state.Target, static value => state => state with { Target = value })));
    public static readonly GamutCompressionParameter CyanThreshold = new("cyan-threshold", new StateParameter<GamutCompression>.Bounded<CompressionThreshold, float, InvalidColor>(
        Lens<GamutCompression, CompressionThreshold>.New(static state => state.CyanThreshold, static value => state => state with { CyanThreshold = value }), ThresholdTrack));
    public static readonly GamutCompressionParameter MagentaThreshold = new("magenta-threshold", new StateParameter<GamutCompression>.Bounded<CompressionThreshold, float, InvalidColor>(
        Lens<GamutCompression, CompressionThreshold>.New(static state => state.MagentaThreshold, static value => state => state with { MagentaThreshold = value }), ThresholdTrack));
    public static readonly GamutCompressionParameter YellowThreshold = new("yellow-threshold", new StateParameter<GamutCompression>.Bounded<CompressionThreshold, float, InvalidColor>(
        Lens<GamutCompression, CompressionThreshold>.New(static state => state.YellowThreshold, static value => state => state with { YellowThreshold = value }), ThresholdTrack));
    public static readonly GamutCompressionParameter CyanLimit = new("cyan-limit", new StateParameter<GamutCompression>.Bounded<CompressionLimit, float, InvalidColor>(
        Lens<GamutCompression, CompressionLimit>.New(static state => state.CyanLimit, static value => state => state with { CyanLimit = value }), LimitTrack));
    public static readonly GamutCompressionParameter MagentaLimit = new("magenta-limit", new StateParameter<GamutCompression>.Bounded<CompressionLimit, float, InvalidColor>(
        Lens<GamutCompression, CompressionLimit>.New(static state => state.MagentaLimit, static value => state => state with { MagentaLimit = value }), LimitTrack));
    public static readonly GamutCompressionParameter YellowLimit = new("yellow-limit", new StateParameter<GamutCompression>.Bounded<CompressionLimit, float, InvalidColor>(
        Lens<GamutCompression, CompressionLimit>.New(static state => state.YellowLimit, static value => state => state with { YellowLimit = value }), LimitTrack));
    public static readonly GamutCompressionParameter Power = new("power", new StateParameter<GamutCompression>.Bounded<CompressionPower, float, InvalidColor>(
        Lens<GamutCompression, CompressionPower>.New(static state => state.Power, static value => state => state with { Power = value }),
        new() { Soft = (CompressionPower.MinValue, 2f) }));

    public StateParameter<GamutCompression> Kind { get; }
}

[SmartEnum]
public sealed partial class GamutCompressionPreset {
    public static readonly GamutCompressionPreset Working = new(GamutCompression.Default);
    public static readonly GamutCompressionPreset Aces13 = new(GamutCompression.Default with { Target = Some(Gamut.Acescg) });

    public GamutCompression State { get; }
}
