using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using MathNet.Numerics.LinearAlgebra;
using Rasm.Imaging.Pixels;
using TinyEXR;
using Wacton.Unicolour;

namespace Rasm.Imaging.Color;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record Gamut {
    private static readonly Func<Gamut, (double R, double G, double B)> Luminances = memoUnsafe(static (Gamut gamut) =>
        (Relative(gamut, (1d, 0d, 0d)), Relative(gamut, (0d, 1d, 0d)), Relative(gamut, (0d, 0d, 1d))));

    public Gamut(RgbConfiguration rgb, Option<string> interopId = default) =>
        (Rgb, Configuration, InteropId) = (rgb, new Configuration(rgbConfig: rgb), interopId);

    public static Gamut StandardRgb { get; } = new(RgbConfiguration.StandardRgb, Some("lin_rec709_scene"));
    public static Gamut DisplayP3 { get; } = new(RgbConfiguration.DisplayP3, Some("lin_p3d65_scene"));
    public static Gamut Rec2020 { get; } = new(RgbConfiguration.Rec2020, Some("lin_rec2020_scene"));
    public static Gamut Acescg { get; } = new(RgbConfiguration.Acescg, Some("lin_ap1_scene"));
    public static Gamut Aces20651 { get; } = new(RgbConfiguration.Aces20651, Some("lin_ap0_scene"));
    public static Gamut ProPhoto { get; } = new(RgbConfiguration.ProPhoto);
    public static Gamut EGamut { get; } = Primaries(new(0.8, 0.3177), new(0.18, 0.9), new(0.065, -0.0805), "E-Gamut", Some("blender:lin_egamut_scene"));
    public static Gamut ArriWideGamut3 { get; } = Primaries(new(0.684, 0.313), new(0.221, 0.848), new(0.0861, -0.102), "ARRI Wide Gamut 3");
    public static Gamut ArriWideGamut4 { get; } = Primaries(new(0.7347, 0.2653), new(0.1424, 0.8576), new(0.0991, -0.0308), "ARRI Wide Gamut 4");
    public static Gamut SGamut3 { get; } = Primaries(new(0.73, 0.28), new(0.14, 0.855), new(0.1, -0.05), "S-Gamut3");
    public static Gamut SGamut3Cine { get; } = Primaries(new(0.766, 0.275), new(0.225, 0.8), new(0.089, -0.087), "S-Gamut3.Cine");
    public static Gamut VeniceSGamut3 { get; } = Primaries(
        new(0.740464264304292, 0.27936437475066), new(0.089241145423286, 0.893809528608105), new(0.110488236673827, -0.052579333080476), "Venice S-Gamut3");
    public static Gamut VeniceSGamut3Cine { get; } = Primaries(
        new(0.775901871567345, 0.274502392854799), new(0.188682902773355, 0.828684937020288), new(0.101337382499301, -0.089187517306263), "Venice S-Gamut3.Cine");
    public static Gamut CinemaGamut { get; } = Primaries(new(0.74, 0.27), new(0.17, 1.14), new(0.08, -0.1), "Cinema Gamut");
    public static Gamut RedWideGamutRgb { get; } = Primaries(new(0.780308, 0.304253), new(0.121595, 1.493994), new(0.095612, -0.084589), "REDWideGamutRGB");
    public static Gamut VGamut { get; } = Primaries(new(0.73, 0.28), new(0.165, 0.84), new(0.1, -0.03), "V-Gamut");
    public static Gamut DGamut { get; } = Primaries(new(0.71, 0.31), new(0.21, 0.88), new(0.09, -0.08), "D-Gamut");
    public static Gamut ProtuneNative { get; } = Primaries(
        new(0.698480461493841, 0.193026445370121), new(0.329555378387345, 1.024596624134644), new(0.108442631407675, -0.034678569754016), "Protune Native");
    public static Gamut BlackmagicWideGamut { get; } = Primaries(new(0.7177215, 0.3171181), new(0.228041, 0.861569), new(0.1005841, -0.0820452), "Blackmagic Wide Gamut");
    public static Gamut DaVinciWideGamut { get; } = Primaries(new(0.8, 0.313), new(0.1682, 0.9877), new(0.079, -0.1155), "DaVinci Wide Gamut");
    public static Gamut AppleWideGamut { get; } = Primaries(new(0.725, 0.301), new(0.221, 0.814), new(0.068, -0.076), "Apple Wide Gamut");

    public RgbConfiguration Rgb { get; }
    public Configuration Configuration { get; }
    public Option<string> InteropId { get; }

    public Chromaticities Chromaticities => new(
        (float)Rgb.ChromaticityR.X, (float)Rgb.ChromaticityR.Y,
        (float)Rgb.ChromaticityG.X, (float)Rgb.ChromaticityG.Y,
        (float)Rgb.ChromaticityB.X, (float)Rgb.ChromaticityB.Y,
        (float)Rgb.WhitePoint.Chromaticity.X, (float)Rgb.WhitePoint.Chromaticity.Y);

    public Matrix<double> ToXyz => Columns(basis => new Unicolour(Configuration, ColourSpace.RgbLinear, basis).Xyz.Tuple);

    public Matrix<double> FromXyz => Columns(basis => new Unicolour(Configuration, ColourSpace.Xyz, basis).RgbLinear.Tuple);

    public (double R, double G, double B) Luminance => Luminances(this);

    public YbrConfiguration Ybr => new(Luminance.R, Luminance.B, Configuration.Ybr.RangeY, Configuration.Ybr.RangeC, Rgb.Name);

    public bool Equals([NotNullWhen(true)] Gamut? other) => other is not null && ReferenceEquals(Rgb, other.Rgb);

    public override int GetHashCode() => RuntimeHelpers.GetHashCode(Rgb);

    public Matrix<double> MatrixTo(Gamut target) => Between(Configuration, target.Configuration);

    public Matrix<double> Adapting(WhitePoint source, WhitePoint destination) =>
        (new Configuration(xyzConfig: new XyzConfiguration(source)), new Configuration(xyzConfig: new XyzConfiguration(destination))) switch {
            var (from, to) => Columns(basis => new Unicolour(
                Configuration, ColourSpace.Xyz,
                new Unicolour(from, ColourSpace.Xyz, new Unicolour(Configuration, ColourSpace.RgbLinear, basis).Xyz.Tuple).ConvertToConfiguration(to).Xyz.Tuple).RgbLinear.Tuple),
        };

    public static Matrix<double> Between(Configuration from, Configuration to) =>
        Columns(basis => new Unicolour(from, ColourSpace.RgbLinear, basis).ConvertToConfiguration(to).RgbLinear.Tuple);

    public static Matrix<double> ToYpbpr(YbrConfiguration ybr) =>
        new Configuration(ybrConfig: ybr) switch {
            var configuration => Columns(basis => new Unicolour(configuration, ColourSpace.Rgb, basis).Ypbpr.Tuple),
        };

    public static Matrix<double> FromYpbpr(YbrConfiguration ybr) =>
        new Configuration(ybrConfig: ybr) switch {
            var configuration => Columns(basis => new Unicolour(configuration, ColourSpace.Ypbpr, basis).Rgb.Tuple),
        };

    public static Matrix<double> Columns(Func<(double, double, double), (double, double, double)> column) =>
        (column((1d, 0d, 0d)), column((0d, 1d, 0d)), column((0d, 0d, 1d))) switch {
            var ((r1, g1, b1), (r2, g2, b2), (r3, g3, b3)) =>
                Matrix<double>.Build.DenseOfColumnMajor(3, 3, [r1, g1, b1, r2, g2, b2, r3, g3, b3]),
        };

    private static double Relative(Gamut gamut, (double, double, double) basis) => new Unicolour(gamut.Configuration, ColourSpace.RgbLinear, basis).RelativeLuminance;

    private static Gamut Primaries(Chromaticity red, Chromaticity green, Chromaticity blue, string name, Option<string> interopId = default) =>
        new(new RgbConfiguration(red, green, blue, Illuminant.D65.GetWhitePoint(Observer.Degree2), static linear => linear, static linear => linear, name), interopId);
}

[ComplexValueObject]
[ValidationError<ImagingFault>]
public sealed partial class GamutCompression {
    public static GamutCompression Reference { get; } = Create(new Channels<float>(0.815f, 0.803f, 0.880f), new Channels<float>(1.147f, 1.264f, 1.312f), 1.2f);

    public Channels<float> Threshold { get; }
    public Channels<float> Limit { get; }
    public float Power { get; }

    public Action<Span<Vector4>> Within(Gamut working, Gamut target) {
        (ColorMatrix3x3 inward, ColorMatrix3x3 outward, double power) = (LutTables.Baked(working.MatrixTo(target)), LutTables.Baked(target.MatrixTo(working)), Power);
        ((double Threshold, double Scale) red, (double Threshold, double Scale) green, (double Threshold, double Scale) blue) =
            (Curve(Threshold.Red, Limit.Red, power), Curve(Threshold.Green, Limit.Green, power), Curve(Threshold.Blue, Limit.Blue, power));
        return row => {
            Span<float> samples = MemoryMarshal.Cast<Vector4, float>(row);
            ImageProcessing.ApplyColorMatrix(samples, samples, 4, inward);
            foreach (ref Vector4 pixel in row) {
                double ach = float.Max(pixel.X, float.Max(pixel.Y, pixel.Z));
                pixel = new Vector4((float)Compress(pixel.X, ach, red, power), (float)Compress(pixel.Y, ach, green, power), (float)Compress(pixel.Z, ach, blue, power), pixel.W);
            }
            ImageProcessing.ApplyColorMatrix(samples, samples, 4, outward);
        };
    }

    private static (double Threshold, double Scale) Curve(double threshold, double limit, double power) =>
        (threshold, (1d - threshold) / double.Pow(1d - double.Pow((1d - threshold) / (limit - threshold), power), 1d / power));

    private static double Compress(double component, double ach, (double Threshold, double Scale) curve, double power) =>
        double.Abs(ach) switch {
            0d => 0d,
            var reach => (((ach - component) / reach) - curve.Threshold) switch {
                <= 0d => component,
                var over => (double.Min(over, curve.Scale), double.Max(over, curve.Scale)) switch {
                    var (lower, upper) => ach - (reach * (curve.Threshold + (lower / double.Pow(1d + double.Pow(lower / upper, power), 1d / power)))),
                },
            },
        };

    public static Fin<GamutCompression> Validated(Channels<float> threshold, Channels<float> limit, float power) =>
        TryCreate(threshold, limit, power, out GamutCompression? compression, out ImagingFault? fault) ? compression : fault;

    static partial void ValidateFactoryArguments(ref ImagingFault? validationError, ref Channels<float> threshold, ref Channels<float> limit, ref float power) =>
        validationError = Seq((threshold.Red, limit.Red), (threshold.Green, limit.Green), (threshold.Blue, limit.Blue))
            .ForAll(static pair => pair.Item1 is >= 0f and < 1f && pair.Item2 > 1f && float.IsFinite(pair.Item2)) && float.IsFinite(power) && power >= 1f ? null
            : new ImagingFault(string.Create(CultureInfo.InvariantCulture, $"gamut compression threshold {threshold}, limit {limit}, and power {power}"), ImagingReason.Range);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Lanes {
    public static (Vector256<double> Red, Vector256<double> Green, Vector256<double> Blue) Of(Matrix<double> matrix) =>
        (Vector256.Create([.. matrix.Column(0), 0d]), Vector256.Create([.. matrix.Column(1), 0d]), Vector256.Create([.. matrix.Column(2), 0d]));
}
