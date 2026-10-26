using System.Globalization;
using System.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using Wacton.Unicolour;

namespace Rasm.Imaging.Color;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class StandardIlluminant {
    public static readonly StandardIlluminant B = new("B", new Wacton.Unicolour.Temperature(4873d, -0.001267d));
    public static readonly StandardIlluminant D93 = new("D93", new Wacton.Unicolour.Temperature(9298d, 0.003167d));
    public static readonly StandardIlluminant F1 = new("F1", new Wacton.Unicolour.Temperature(6424d, 0.007267d));
    public static readonly StandardIlluminant F3 = new("F3", new Wacton.Unicolour.Temperature(3447d, 0.000833d));
    public static readonly StandardIlluminant F4 = new("F4", new Wacton.Unicolour.Temperature(2940d, -0.000667d));
    public static readonly StandardIlluminant F5 = new("F5", new Wacton.Unicolour.Temperature(6342d, 0.0109d));
    public static readonly StandardIlluminant F6 = new("F6", new Wacton.Unicolour.Temperature(4148d, 0.0062d));
    public static readonly StandardIlluminant F8 = new("F8", new Wacton.Unicolour.Temperature(4995d, 0.003233d));
    public static readonly StandardIlluminant F9 = new("F9", new Wacton.Unicolour.Temperature(4147d, 0.0001d));
    public static readonly StandardIlluminant F10 = new("F10", new Wacton.Unicolour.Temperature(4991d, 0.0037d));
    public static readonly StandardIlluminant F12 = new("F12", new Wacton.Unicolour.Temperature(3002d, 0.0002d));
    public static readonly StandardIlluminant LedB1 = new("LED-B1", new Wacton.Unicolour.Temperature(2733d, -0.000667d));
    public static readonly StandardIlluminant LedB2 = new("LED-B2", new Wacton.Unicolour.Temperature(2997d, -0.000933d));
    public static readonly StandardIlluminant LedB3 = new("LED-B3", new Wacton.Unicolour.Temperature(4103d, -0.0006d));
    public static readonly StandardIlluminant LedB4 = new("LED-B4", new Wacton.Unicolour.Temperature(5108d, 0.000533d));
    public static readonly StandardIlluminant LedB5 = new("LED-B5", new Wacton.Unicolour.Temperature(6598d, 0.0009d));
    public static readonly StandardIlluminant LedBh1 = new("LED-BH1", new Wacton.Unicolour.Temperature(2852d, -0.0003d));
    public static readonly StandardIlluminant LedRgb1 = new("LED-RGB1", new Wacton.Unicolour.Temperature(2840d, 0.004267d));
    public static readonly StandardIlluminant LedV1 = new("LED-V1", new Wacton.Unicolour.Temperature(3079d, 0.0163d));
    public static readonly StandardIlluminant LedV2 = new("LED-V2", new Wacton.Unicolour.Temperature(4070d, 0.0011d));

    public Wacton.Unicolour.Temperature Temperature { get; }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Adaptation {
    public static WhitePoint White(Wacton.Unicolour.Temperature temperature) =>
        new Unicolour(Configuration.Default, temperature).Chromaticity switch {
            var (x, y) => new WhitePoint(x, y),
        };

    public static Unicolour Peaked(Unicolour color) =>
        color.RgbLinear switch {
            var rgb => double.Max(double.Abs(rgb.R), double.Max(double.Abs(rgb.G), double.Abs(rgb.B))) switch {
                var peak => new Unicolour(color.Configuration, ColourSpace.RgbLinear, rgb.R / peak, rgb.G / peak, rgb.B / peak),
            },
        };

    public static Unicolour Blackbody(Gamut gamut, Wacton.Unicolour.Temperature temperature) =>
        new(gamut.Configuration, Peaked(new Unicolour(gamut.Configuration, temperature)).MapToRgbGamut().Chromaticity);

    public static Fin<Wacton.Unicolour.Temperature> Correlate(Gamut gamut, Vector3 color) =>
        new Unicolour(gamut.Configuration, ColourSpace.RgbLinear, color.X, color.Y, color.Z) switch {
            { Xyz.Y: <= 0d and var luminance } => new ImagingFault(string.Create(CultureInfo.InvariantCulture, $"picked white luminance {luminance} must be positive")),
            { Temperature: { IsValid: true } correlated } => correlated,
            var unresolved => new ImagingFault(string.Create(CultureInfo.InvariantCulture, $"picked white at ({unresolved.Chromaticity.X}, {unresolved.Chromaticity.Y}) has no temperature and tint")),
        };
}

public static class StandardObserver {
    private static readonly Memo<Arr<(float Wavelength, Vector3 Xyz)>> Rows = memo(static () => {
        using StreamReader reader = new(typeof(StandardObserver).Assembly.GetManifestResourceStream(typeof(StandardObserver), "Tables.CIE_xyz_1931_2deg.csv")!);
        return (Arr<(float Wavelength, Vector3 Xyz)>)[.. reader.ReadToEnd().Split('\n', StringSplitOptions.RemoveEmptyEntries).Select(static line =>
            line.Split(',').Select(static cell => float.Parse(cell, CultureInfo.InvariantCulture)).ToArray() switch {
                var cells => (cells[0], new Vector3(cells[1], cells[2], cells[3])),
            })];
    });

    public static Arr<Vector2> Locus { get; } = [.. Cmf.RequiredWavelengths.Select(static wavelength =>
        new Unicolour(new Spd(wavelength, 1, 1d)).Chromaticity switch { var point => new Vector2((float)point.X, (float)point.Y) })];

    public static Arr<(float Wavelength, Vector3 Matching)> Matching(Gamut working) {
        Matrix<double> matrix = working.FromXyz;
        return Rows.Value.Map(row => matrix.Multiply(new DenseVector([row.Xyz.X, row.Xyz.Y, row.Xyz.Z])) switch {
            var rgb => (row.Wavelength, new Vector3((float)rgb[0], (float)rgb[1], (float)rgb[2])),
        });
    }
}
