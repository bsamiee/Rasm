using System.Globalization;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using CommunityToolkit.HighPerformance.Buffers;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using Rasm.Imaging.Pixels;
using TinyEXR;
using Wacton.Unicolour;

namespace Rasm.Imaging.Color;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class Gamut : ICaptioned {
    public static readonly Gamut StandardRgb = new("standard-rgb", Configuration.Default, Some("lin_rec709_scene"), "Rec.709");
    public static readonly Gamut DisplayP3 = new("display-p3", new Configuration(rgbConfig: RgbConfiguration.DisplayP3), Some("lin_p3d65_scene"), "Display P3");
    public static readonly Gamut Rec2020 = new("rec2020", new Configuration(rgbConfig: RgbConfiguration.Rec2020), Some("lin_rec2020_scene"), "Rec.2020");
    public static readonly Gamut Acescg = new("acescg", new Configuration(rgbConfig: RgbConfiguration.Acescg), Some("lin_ap1_scene"), "ACEScg");
    public static readonly Gamut Aces20651 = new("aces20651", new Configuration(rgbConfig: RgbConfiguration.Aces20651), Some("lin_ap0_scene"), "ACES2065-1");
    public static readonly Gamut ProPhoto = new("pro-photo", new Configuration(rgbConfig: RgbConfiguration.ProPhoto), None, "ProPhoto RGB");
    public static readonly Gamut EGamut = new("e-gamut", Linear(new(0.8, 0.3177), new(0.18, 0.9), new(0.065, -0.0805)), Some("blender:lin_egamut_scene"), "E-Gamut");
    public static readonly Gamut ArriWideGamut3 = new("arri-wide-gamut-3", Linear(new(0.684, 0.313), new(0.221, 0.848), new(0.0861, -0.102)), None, "ARRI Wide Gamut 3");
    public static readonly Gamut ArriWideGamut4 = new("arri-wide-gamut-4", Linear(new(0.7347, 0.2653), new(0.1424, 0.8576), new(0.0991, -0.0308)), None, "ARRI Wide Gamut 4");
    public static readonly Gamut SGamut3 = new("s-gamut3", Linear(new(0.73, 0.28), new(0.14, 0.855), new(0.1, -0.05)), None, "S-Gamut3");
    public static readonly Gamut SGamut3Cine = new("s-gamut3-cine", Linear(new(0.766, 0.275), new(0.225, 0.8), new(0.089, -0.087)), None, "S-Gamut3.Cine");
    public static readonly Gamut VeniceSGamut3 = new(
        "venice-s-gamut3",
        Linear(new(0.740464264304292, 0.27936437475066), new(0.089241145423286, 0.893809528608105), new(0.110488236673827, -0.052579333080476)),
        None, "Venice S-Gamut3");
    public static readonly Gamut VeniceSGamut3Cine = new(
        "venice-s-gamut3-cine",
        Linear(new(0.775901871567345, 0.274502392854799), new(0.188682902773355, 0.828684937020288), new(0.101337382499301, -0.089187517306263)),
        None, "Venice S-Gamut3.Cine");
    public static readonly Gamut CinemaGamut = new("cinema-gamut", Linear(new(0.74, 0.27), new(0.17, 1.14), new(0.08, -0.1)), None, "Cinema Gamut");
    public static readonly Gamut RedWideGamutRgb = new("red-wide-gamut-rgb", Linear(new(0.780308, 0.304253), new(0.121595, 1.493994), new(0.095612, -0.084589)), None, "REDWideGamutRGB");
    public static readonly Gamut VGamut = new("v-gamut", Linear(new(0.73, 0.28), new(0.165, 0.84), new(0.1, -0.03)), None, "V-Gamut");
    public static readonly Gamut DGamut = new("d-gamut", Linear(new(0.71, 0.31), new(0.21, 0.88), new(0.09, -0.08)), None, "D-Gamut");
    public static readonly Gamut ProtuneNative = new(
        "protune-native",
        Linear(new(0.698480461493841, 0.193026445370121), new(0.329555378387345, 1.024596624134644), new(0.108442631407675, -0.034678569754016)),
        None, "Protune Native");
    public static readonly Gamut BlackmagicWideGamut = new("blackmagic-wide-gamut", Linear(new(0.7177215, 0.3171181), new(0.228041, 0.861569), new(0.1005841, -0.0820452)), None, "Blackmagic Wide Gamut");
    public static readonly Gamut DaVinciWideGamut = new("davinci-wide-gamut", Linear(new(0.8, 0.313), new(0.1682, 0.9877), new(0.079, -0.1155)), None, "DaVinci Wide Gamut");
    public static readonly Gamut AppleWideGamut = new("apple-wide-gamut", Linear(new(0.725, 0.301), new(0.221, 0.814), new(0.068, -0.076)), None, "Apple Wide Gamut");

    private static readonly Func<Gamut, (double X, double Y, double Z)> Luminances = memoUnsafe(static (Gamut gamut) => gamut.ToXyz switch {
        var matrix => (matrix[1, 0], matrix[1, 1], matrix[1, 2]),
    });

    public Configuration Configuration { get; }
    public Option<string> InteropId { get; }
    public string Caption { get; }

    public Chromaticities Chromaticities => new(
        (float)Configuration.Rgb.ChromaticityR.X, (float)Configuration.Rgb.ChromaticityR.Y,
        (float)Configuration.Rgb.ChromaticityG.X, (float)Configuration.Rgb.ChromaticityG.Y,
        (float)Configuration.Rgb.ChromaticityB.X, (float)Configuration.Rgb.ChromaticityB.Y,
        (float)Configuration.Rgb.WhitePoint.Chromaticity.X, (float)Configuration.Rgb.WhitePoint.Chromaticity.Y);

    public Matrix<double> ToXyz => Columns(basis => new Unicolour(Configuration, ColourSpace.RgbLinear, basis).Xyz.Tuple);

    public Matrix<double> FromXyz => Columns(basis => new Unicolour(Configuration, ColourSpace.Xyz, basis).RgbLinear.Tuple);

    public (double X, double Y, double Z) Luminance => Luminances(this);

    public ((double X, double Y, double Z) Red, (double X, double Y, double Z) Green, (double X, double Y, double Z) Blue) XyzColumns => ToXyz switch {
        var matrix => ((matrix[0, 0], matrix[1, 0], matrix[2, 0]), (matrix[0, 1], matrix[1, 1], matrix[2, 1]), (matrix[0, 2], matrix[1, 2], matrix[2, 2])),
    };

    public Matrix<double> MatrixTo(Gamut target) => Between(Configuration, target.Configuration);

    public Matrix<double> Adapting(WhitePoint source, WhitePoint destination) =>
        (new Configuration(xyzConfig: new XyzConfiguration(source)), new Configuration(xyzConfig: new XyzConfiguration(destination))) switch {
            var (from, to) => Columns(basis => new Unicolour(
                Configuration, ColourSpace.Xyz,
                new Unicolour(from, ColourSpace.Xyz, new Unicolour(Configuration, ColourSpace.RgbLinear, basis).Xyz.Tuple).ConvertToConfiguration(to).Xyz.Tuple).RgbLinear.Tuple),
        };

    public static Option<Gamut> Of(Chromaticities stated) {
        static Vector256<float> Coordinates(Chromaticities c) => Vector256.Create(c.RedX, c.RedY, c.GreenX, c.GreenY, c.BlueX, c.BlueY, c.WhiteX, c.WhiteY);
        return toSeq(Items).Find(gamut => Vector256.LessThanOrEqualAll(Vector256.Abs(Coordinates(gamut.Chromaticities) - Coordinates(stated)), Vector256.Create(1e-4f)));
    }

    public static Matrix<double> Between(Configuration from, Configuration to) =>
        Columns(basis => new Unicolour(from, ColourSpace.RgbLinear, basis).ConvertToConfiguration(to).RgbLinear.Tuple);

    public static Matrix<double> Columns(Func<(double, double, double), (double, double, double)> column) =>
        (column((1d, 0d, 0d)), column((0d, 1d, 0d)), column((0d, 0d, 1d))) switch {
            var ((r1, g1, b1), (r2, g2, b2), (r3, g3, b3)) =>
                Matrix<double>.Build.DenseOfColumnMajor(3, 3, [r1, g1, b1, r2, g2, b2, r3, g3, b3]),
        };

    private static Configuration Linear(Chromaticity red, Chromaticity green, Chromaticity blue) =>
        new(rgbConfig: new RgbConfiguration(red, green, blue, Illuminant.D65.GetWhitePoint(Observer.Degree2), static linear => linear, static linear => linear));
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct Nits : IMinMaxValue<Nits> {
    public static Nits MinValue { get; } = new(1e-6f);
    public static Nits MaxValue { get; } = new(10_000f);
    public static Nits ReferenceWhite { get; } = new(100f);
    public static Nits FileWhite { get; } = new(203f);
    public static Nits HlgPeak { get; } = new(1_000f);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class TransferCurve : ICaptioned {
    public static readonly TransferCurve Linear = new("linear", "Linear", static (_, _) => { }, static (_, _) => { });
    public static readonly TransferCurve Srgb = new(
        "srgb", "sRGB",
        static (row, _) => {
            foreach (ref Vector4 pixel in row)
                ImageProcessing.EncodeTransfer(Components(ref pixel), Components(ref pixel), TransferFunction.Srgb);
        },
        static (row, _) => {
            foreach (ref Vector4 pixel in row)
                ImageProcessing.DecodeTransfer(Components(ref pixel), Components(ref pixel), TransferFunction.Srgb);
        });
    public static readonly TransferCurve Pq = new(
        "pq", "PQ",
        static (row, white) => {
            float scale = (float)white / (float)Nits.MaxValue;
            foreach (ref Vector4 pixel in row) {
                pixel = new Vector4(pixel.AsVector3() * scale, pixel.W);
                ImageProcessing.EncodeTransfer(Components(ref pixel), Components(ref pixel), TransferFunction.Pq);
            }
        },
        static (row, white) => {
            float scale = (float)Nits.MaxValue / (float)white;
            foreach (ref Vector4 pixel in row) {
                ImageProcessing.DecodeTransfer(Components(ref pixel), Components(ref pixel), TransferFunction.Pq);
                pixel = new Vector4(pixel.AsVector3() * scale, pixel.W);
            }
        });
    public static readonly TransferCurve Hlg = new(
        "hlg", "HLG",
        static (row, white) => {
            double scale = (double)white / Nits.HlgPeak;
            foreach (ref Vector4 pixel in row) {
                Vector256<double> display = Vector256.Create(pixel.X, pixel.Y, pixel.Z, 0d) * scale;
                double luminance = Vector256.Dot(display, HlgSystem.Weights);
                Vector256<double> scene = display * double.Pow(double.Max(luminance, HlgSystem.DisplayFloor), (1d - HlgSystem.Gamma) / HlgSystem.Gamma);
                pixel = new Vector4((float)scene[0], (float)scene[1], (float)scene[2], pixel.W);
                ImageProcessing.EncodeTransfer(Components(ref pixel), Components(ref pixel), TransferFunction.Hlg);
            }
        },
        static (row, white) => {
            double scale = (double)Nits.HlgPeak / white;
            foreach (ref Vector4 pixel in row) {
                ImageProcessing.DecodeTransfer(Components(ref pixel), Components(ref pixel), TransferFunction.Hlg);
                Vector256<double> scene = Vector256.Create(pixel.X, pixel.Y, pixel.Z, 0d);
                double luminance = Vector256.Dot(scene, HlgSystem.Weights);
                Vector256<double> display = scene * (double.Pow(double.Max(luminance, HlgSystem.SceneFloor), HlgSystem.Gamma - 1d) * scale);
                pixel = new Vector4((float)display[0], (float)display[1], (float)display[2], pixel.W);
            }
        });
    public static readonly TransferCurve AcesCct = new("acescct", "ACEScct", static (row, _) => PerChannel(row, RgbModels.Acescct.FromLinear), static (row, _) => PerChannel(row, RgbModels.Acescct.ToLinear));
    public static readonly TransferCurve AcesCc = new("acescc", "ACEScc", static (row, _) => PerChannel(row, RgbModels.Acescc.FromLinear), static (row, _) => PerChannel(row, RgbModels.Acescc.ToLinear));

    private static readonly (Vector256<double> Weights, double Gamma, double SceneFloor, double DisplayFloor) HlgSystem =
        (Gamut.Rec2020.Luminance, 1.2d + (0.42d * double.Log10(Nits.HlgPeak / 1_000d))) switch {
            var ((x, y, z), gamma) => (Vector256.Create(x, y, z, 0d), gamma, double.Pow(1e-4d, 1d / gamma) / 3d, 1e-4d / double.Pow(3d, gamma)),
        };

    public string Caption { get; }

    [UseDelegateFromConstructor]
    public partial void Encode(Span<Vector4> row, Nits white);

    [UseDelegateFromConstructor]
    public partial void Decode(Span<Vector4> row, Nits white);

    internal static void PerChannel(Span<Vector4> row, Func<double, double> curve) {
        for (int index = 0; index < row.Length; index++)
            row[index] = new Vector4((float)curve(row[index].X), (float)curve(row[index].Y), (float)curve(row[index].Z), row[index].W);
    }

    private static Span<float> Components(ref Vector4 pixel) => MemoryMarshal.CreateSpan(ref pixel.X, 3);
}

[Union]
[ObjectFactory<string>]
public abstract partial record Transfer : IConvertible<string> {
    private const string GammaForm = "gamma";

    public static Seq<Transfer> Named { get; } = [.. TransferCurve.Items, Gamma.Gamma22, Gamma.Rec1886, .. CameraLog.Items];

    public abstract void Encode(Span<Vector4> row, Nits white);
    public abstract void Decode(Span<Vector4> row, Nits white);

    public static ValidationError? Validate(string? value, IFormatProvider? provider, out Transfer? item) {
        item = value?.Split(':', 2) switch {
            null => null,
            [GammaForm, var exponent] => PositiveMagnitude.TryParse(exponent, CultureInfo.InvariantCulture, out PositiveMagnitude parsed) ? new Gamma(parsed) : null,
            _ => TransferCurve.TryGet(value, out TransferCurve? curve) ? new Standard(curve) : CameraLog.TryGet(value, out CameraLog? log) ? new Camera(log) : null,
        };
        return value is not null && item is null ? new ValidationError($"transfer '{value}' must name a transfer curve, a camera log, or {GammaForm}:<exponent>") : null;
    }

    public string ToValue() => Switch(
        standard: static transfer => transfer.Curve.Key,
        gamma: static transfer => string.Create(CultureInfo.InvariantCulture, $"{GammaForm}:{transfer.Exponent}"),
        camera: static transfer => transfer.Log.Key);

    public PixelFrame Light(PixelFrame frame) =>
        new(frame.Origin, frame.Size, frame.Extent, block => {
            frame.Block.CopyTo(block);
            Decode(MemoryMarshal.Cast<float, Vector4>(block.AsSpan()), Nits.ReferenceWhite);
        });

    public int[] Displayed(PixelFrame frame) {
        using SpanOwner<float> buffer = SpanOwner<float>.Allocate(frame.Block.Length);
        frame.Block.CopyTo(buffer.Span);
        Span<Vector4> light = MemoryMarshal.Cast<float, Vector4>(buffer.Span);
        Decode(light, Nits.ReferenceWhite);
        TransferCurve.Srgb.Encode(light, Nits.ReferenceWhite);
        foreach (ref Vector4 pixel in light)
            pixel = new Vector4(pixel.Z, pixel.Y, pixel.X, pixel.W);
        int[] words = GC.AllocateUninitializedArray<int>(light.Length);
        PixelConversion.FloatToByte(MemoryMarshal.Cast<Vector4, float>(light), MemoryMarshal.AsBytes(words.AsSpan()), PixelConversionMode.Normalized);
        return words;
    }

    public sealed record Standard(TransferCurve Curve) : Transfer, IStateRecord<Standard, StandardTransferParameter> {
        public static Standard Default { get; } = new(TransferCurve.Srgb);

        public override void Encode(Span<Vector4> row, Nits white) => Curve.Encode(row, white);
        public override void Decode(Span<Vector4> row, Nits white) => Curve.Decode(row, white);
    }

    public sealed record Gamma(PositiveMagnitude Exponent) : Transfer, IStateRecord<Gamma, GammaTransferParameter> {
        public static Gamma Gamma22 { get; } = new(PositiveMagnitude.Create(2.2f));
        public static Gamma Rec1886 { get; } = new(PositiveMagnitude.Create(2.4f));
        public static Gamma Default => Gamma22;

        public override void Encode(Span<Vector4> row, Nits white) => TransferCurve.PerChannel(row, linear => double.Pow(double.Max(linear, 0d), 1d / Exponent));
        public override void Decode(Span<Vector4> row, Nits white) => TransferCurve.PerChannel(row, signal => double.Pow(double.Max(signal, 0d), Exponent));
    }

    public sealed record Camera(CameraLog Log) : Transfer, IStateRecord<Camera, CameraTransferParameter> {
        public static Camera Default { get; } = new(CameraLog.ArriLogC3);
        public static string Caption => "Camera log";

        public override void Encode(Span<Vector4> row, Nits white) => TransferCurve.PerChannel(row, Log.Encode);
        public override void Decode(Span<Vector4> row, Nits white) => TransferCurve.PerChannel(row, Log.Decode);
    }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class StandardTransferParameter : IStateParameter<Transfer.Standard> {
    public static readonly StandardTransferParameter Curve = new("curve", new StateParameter<Transfer.Standard>.Choice<TransferCurve>(Lens<Transfer.Standard, TransferCurve>.New(static transfer => transfer.Curve, static curve => transfer => transfer with { Curve = curve })),
        new("Curve", "Transfer function of the table's input and output"));

    public StateParameter<Transfer.Standard> Kind { get; }
    public ParameterText Text { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class GammaTransferParameter : IStateParameter<Transfer.Gamma> {
    public static readonly GammaTransferParameter Exponent = new("exponent", StateParameter.Bounded(Lens<Transfer.Gamma, PositiveMagnitude>.New(static transfer => transfer.Exponent, static exponent => transfer => transfer with { Exponent = exponent }), new Presentation<float> { Form = NumberForm.Field }),
        new("Exponent", "Power decoding the table's signal to linear light"));

    public StateParameter<Transfer.Gamma> Kind { get; }
    public ParameterText Text { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CameraTransferParameter : IStateParameter<Transfer.Camera> {
    public static readonly CameraTransferParameter Log = new("log", new StateParameter<Transfer.Camera>.Choice<CameraLog>(Lens<Transfer.Camera, CameraLog>.New(static transfer => transfer.Log, static log => transfer => transfer with { Log = log })),
        new("Camera log", "Camera encoding of the table's input and output"));

    public StateParameter<Transfer.Camera> Kind { get; }
    public ParameterText Text { get; }
}

public sealed record ColorEncoding(Gamut Gamut, Transfer Transfer, Nits White) {
    public void Encode(Span<Vector4> row) => Transfer.Encode(row, White);
    public void Decode(Span<Vector4> row) => Transfer.Decode(row, White);

    public Seq<Action<Span<Vector4>>> To(ColorEncoding target) =>
        this == target
            ? Seq<Action<Span<Vector4>>>()
            : (Transfer == TransferCurve.Linear ? Seq<Action<Span<Vector4>>>() : Seq(Decode))
              + LinearTo(target).Map<Action<Span<Vector4>>>(static table => table.Apply)
              + (target.Transfer == TransferCurve.Linear ? Seq<Action<Span<Vector4>>>() : Seq(target.Encode));

    internal Seq<LutTable> LinearTo(ColorEncoding target) =>
        ((double)White / target.White) switch {
            1d when Gamut == target.Gamut => [],
            var white => [new LutTable.Affine(Gamut.MatrixTo(target.Gamut).Multiply(white))],
        };
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct CompressionThreshold : IMinMaxValue<CompressionThreshold> {
    public static CompressionThreshold MinValue { get; } = new(0f);
    public static CompressionThreshold MaxValue { get; } = new(0.9995f);
    public static CompressionThreshold ReferenceCyan { get; } = new(0.815f);
    public static CompressionThreshold ReferenceMagenta { get; } = new(0.803f);
    public static CompressionThreshold ReferenceYellow { get; } = new(0.880f);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct CompressionLimit : IMinMaxValue<CompressionLimit> {
    public static CompressionLimit MinValue { get; } = new(1.001f);
    public static CompressionLimit MaxValue { get; } = new(65504f);
    public static CompressionLimit ReferenceCyan { get; } = new(1.147f);
    public static CompressionLimit ReferenceMagenta { get; } = new(1.264f);
    public static CompressionLimit ReferenceYellow { get; } = new(1.312f);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct CompressionPower : IMinMaxValue<CompressionPower> {
    public static CompressionPower MinValue { get; } = new(1f);
    public static CompressionPower MaxValue { get; } = new(65504f);
    public static CompressionPower Reference { get; } = new(1.2f);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

public sealed record GamutCompression(
    Option<Gamut> Target,
    CompressionThreshold CyanThreshold, CompressionThreshold MagentaThreshold, CompressionThreshold YellowThreshold,
    CompressionLimit CyanLimit, CompressionLimit MagentaLimit, CompressionLimit YellowLimit,
    CompressionPower Power) : IStateRecord<GamutCompression, GamutCompressionParameter>, IPixelStage<GamutCompression> {
    public static GamutCompression Default { get; } = new(
        None,
        CompressionThreshold.ReferenceCyan, CompressionThreshold.ReferenceMagenta, CompressionThreshold.ReferenceYellow,
        CompressionLimit.ReferenceCyan, CompressionLimit.ReferenceMagenta, CompressionLimit.ReferenceYellow,
        CompressionPower.Reference);

    public static Option<PixelPass> Pass(GamutCompression state, PassContext context) {
        static double Scale(double threshold, double limit, double power) =>
            (1d - threshold) switch {
                var head => head / double.Pow(1d - double.Pow(head / (limit - threshold), power), 1d / power),
            };

        static double Compress(double component, double ach, (double Threshold, double Scale) curve, double power) =>
            double.Abs(ach) switch {
                0d => 0d,
                var reach => (((ach - component) / reach) - curve.Threshold) switch {
                    <= 0d => component,
                    var over => (double.Min(over, curve.Scale), double.Max(over, curve.Scale)) switch {
                        var (lower, upper) => ach - (reach * (curve.Threshold + (lower / double.Pow(1d + double.Pow(lower / upper, power), 1d / power)))),
                    },
                },
            };

        Gamut target = state.Target.IfNone(context.Working);
        Matrix<double> inward = context.Working.MatrixTo(target);
        Matrix<double> outward = target.MatrixTo(context.Working);
        double power = state.Power;
        (double Threshold, double Scale) cyan = (state.CyanThreshold, Scale(state.CyanThreshold, state.CyanLimit, power));
        (double Threshold, double Scale) magenta = (state.MagentaThreshold, Scale(state.MagentaThreshold, state.MagentaLimit, power));
        (double Threshold, double Scale) yellow = (state.YellowThreshold, Scale(state.YellowThreshold, state.YellowLimit, power));
        return Some<PixelPass>(new PixelPass.Color(row => {
            DenseVector source = new(3);
            DenseVector rgb = new(3);
            foreach (ref Vector4 pixel in row) {
                (source[0], source[1], source[2]) = (pixel.X, pixel.Y, pixel.Z);
                inward.Multiply(source, rgb);
                double ach = double.Max(rgb[0], double.Max(rgb[1], rgb[2]));
                (source[0], source[1], source[2]) = (Compress(rgb[0], ach, cyan, power), Compress(rgb[1], ach, magenta, power), Compress(rgb[2], ach, yellow, power));
                outward.Multiply(source, rgb);
                pixel = new Vector4((float)rgb[0], (float)rgb[1], (float)rgb[2], pixel.W);
            }
        }));
    }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class GamutCompressionParameter : IStateParameter<GamutCompression> {
    private static readonly Presentation<float> ThresholdTrack = new() { Step = 0.001f, Decimals = 3 };
    private static readonly Presentation<float> LimitTrack = new() { Soft = (CompressionLimit.MinValue, 2f), Step = 0.001f, Decimals = 3 };

    public static readonly GamutCompressionParameter Target = new("target", new StateParameter<GamutCompression>.OptionalChoice<Gamut>(Lens<GamutCompression, Option<Gamut>>.New(static state => state.Target, static value => state => state with { Target = value }), "Working"),
        new("Gamut", "Primaries the compression measures distances in"));
    public static readonly GamutCompressionParameter CyanThreshold = new("cyan-threshold", StateParameter.Bounded(Lens<GamutCompression, CompressionThreshold>.New(static state => state.CyanThreshold, static value => state => state with { CyanThreshold = value }), ThresholdTrack),
        new("Cyan threshold", "Red distance from the achromatic axis where compression begins"));
    public static readonly GamutCompressionParameter MagentaThreshold = new("magenta-threshold", StateParameter.Bounded(Lens<GamutCompression, CompressionThreshold>.New(static state => state.MagentaThreshold, static value => state => state with { MagentaThreshold = value }), ThresholdTrack),
        new("Magenta threshold", "Green distance from the achromatic axis where compression begins"));
    public static readonly GamutCompressionParameter YellowThreshold = new("yellow-threshold", StateParameter.Bounded(Lens<GamutCompression, CompressionThreshold>.New(static state => state.YellowThreshold, static value => state => state with { YellowThreshold = value }), ThresholdTrack),
        new("Yellow threshold", "Blue distance from the achromatic axis where compression begins"));
    public static readonly GamutCompressionParameter CyanLimit = new("cyan-limit", StateParameter.Bounded(Lens<GamutCompression, CompressionLimit>.New(static state => state.CyanLimit, static value => state => state with { CyanLimit = value }), LimitTrack),
        new("Cyan limit", "Red distance compressed onto the gamut boundary"));
    public static readonly GamutCompressionParameter MagentaLimit = new("magenta-limit", StateParameter.Bounded(Lens<GamutCompression, CompressionLimit>.New(static state => state.MagentaLimit, static value => state => state with { MagentaLimit = value }), LimitTrack),
        new("Magenta limit", "Green distance compressed onto the gamut boundary"));
    public static readonly GamutCompressionParameter YellowLimit = new("yellow-limit", StateParameter.Bounded(Lens<GamutCompression, CompressionLimit>.New(static state => state.YellowLimit, static value => state => state with { YellowLimit = value }), LimitTrack),
        new("Yellow limit", "Blue distance compressed onto the gamut boundary"));
    public static readonly GamutCompressionParameter Power = new("power", StateParameter.Bounded(Lens<GamutCompression, CompressionPower>.New(static state => state.Power, static value => state => state with { Power = value }), new Presentation<float> { Soft = (CompressionPower.MinValue, 2f) }),
        new("Power", "Exponent of the compression curve"));

    public StateParameter<GamutCompression> Kind { get; }
    public ParameterText Text { get; }
}

[SmartEnum]
public sealed partial class GamutCompressionPreset {
    public static readonly GamutCompressionPreset Working = new(GamutCompression.Default);
    public static readonly GamutCompressionPreset Aces13 = new(GamutCompression.Default with { Target = Some(Gamut.Acescg) });

    public GamutCompression State { get; }
}
