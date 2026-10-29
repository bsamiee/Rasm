using System.Globalization;
using System.Numerics;
using Wacton.Unicolour;

namespace Rasm.Imaging.Color;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class StandardIlluminant {
    public static readonly StandardIlluminant A = new("A", Illuminant.A);
    public static readonly StandardIlluminant B = new("B", new Chromaticity(0.34842d, 0.35161d));
    public static readonly StandardIlluminant C = new("C", Illuminant.C);
    public static readonly StandardIlluminant D50 = new("D50", Illuminant.D50);
    public static readonly StandardIlluminant D55 = new("D55", Illuminant.D55);
    public static readonly StandardIlluminant D65 = new("D65", Illuminant.D65);
    public static readonly StandardIlluminant D75 = new("D75", Illuminant.D75);
    public static readonly StandardIlluminant D93 = new("D93", new Unicolour(9300d, Locus.Daylight).Chromaticity);
    public static readonly StandardIlluminant E = new("E", Illuminant.E);
    public static readonly StandardIlluminant F1 = new("F1", new Chromaticity(0.3131d, 0.3371d));
    public static readonly StandardIlluminant F2 = new("F2", Illuminant.F2);
    public static readonly StandardIlluminant F3 = new("F3", new Chromaticity(0.4091d, 0.3941d));
    public static readonly StandardIlluminant F4 = new("F4", new Chromaticity(0.4402d, 0.4031d));
    public static readonly StandardIlluminant F5 = new("F5", new Chromaticity(0.3138d, 0.3452d));
    public static readonly StandardIlluminant F6 = new("F6", new Chromaticity(0.3779d, 0.3882d));
    public static readonly StandardIlluminant F7 = new("F7", Illuminant.F7);
    public static readonly StandardIlluminant F8 = new("F8", new Chromaticity(0.3458d, 0.3586d));
    public static readonly StandardIlluminant F9 = new("F9", new Chromaticity(0.3741d, 0.3727d));
    public static readonly StandardIlluminant F10 = new("F10", new Chromaticity(0.3458d, 0.3588d));
    public static readonly StandardIlluminant F11 = new("F11", Illuminant.F11);
    public static readonly StandardIlluminant F12 = new("F12", new Chromaticity(0.437d, 0.4042d));
    public static readonly StandardIlluminant LedB1 = new("LED-B1", new Chromaticity(0.456d, 0.4078d));
    public static readonly StandardIlluminant LedB2 = new("LED-B2", new Chromaticity(0.4357d, 0.4012d));
    public static readonly StandardIlluminant LedB3 = new("LED-B3", new Chromaticity(0.3756d, 0.3723d));
    public static readonly StandardIlluminant LedB4 = new("LED-B4", new Chromaticity(0.3422d, 0.3502d));
    public static readonly StandardIlluminant LedB5 = new("LED-B5", new Chromaticity(0.3118d, 0.3236d));
    public static readonly StandardIlluminant LedBh1 = new("LED-BH1", new Chromaticity(0.4474d, 0.4066d));
    public static readonly StandardIlluminant LedRgb1 = new("LED-RGB1", new Chromaticity(0.4557d, 0.4211d));
    public static readonly StandardIlluminant LedV1 = new("LED-V1", new Chromaticity(0.4548d, 0.4044d));
    public static readonly StandardIlluminant LedV2 = new("LED-V2", new Chromaticity(0.3781d, 0.3775d));

    private StandardIlluminant(string key, Chromaticity chromaticity) : this(key, new Illuminant(new WhitePoint(chromaticity.X, chromaticity.Y), key)) { }

    public Illuminant Illuminant { get; }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class StandardObserver {
    private static readonly Arr<(float Wavelength, Vector3 Matching)> Rows = Tabulated();

    public static Arr<Vector2> Locus { get; } = Rows.Map(static row => new Vector2(row.Matching.X, row.Matching.Y) / (row.Matching.X + row.Matching.Y + row.Matching.Z));

    public static Arr<(float Wavelength, Vector3 Matching)> Matching(Gamut working) =>
        Gamut.Columns(working.Configuration, ColourSpace.Xyz, working.Configuration, ColourSpace.RgbLinear) switch {
            var matrix => Rows.Map(row => (row.Wavelength, Vector3.Transform(row.Matching, matrix))),
        };

    extension(Wacton.Unicolour.Temperature temperature) {
        public WhitePoint WhitePoint => new Unicolour(Configuration.Default, temperature).Chromaticity switch { var (x, y) => new WhitePoint(x, y) };

        public Unicolour Emission(Gamut gamut) =>
            new Unicolour(gamut.Configuration, temperature).RgbLinear switch {
                var rgb => double.Max(double.Abs(rgb.R), double.Max(double.Abs(rgb.G), double.Abs(rgb.B))) switch {
                    var peak => new(gamut.Configuration, new Unicolour(gamut.Configuration, ColourSpace.RgbLinear, rgb.R / peak, rgb.G / peak, rgb.B / peak).MapToRgbGamut().Chromaticity),
                },
            };
    }

    private static Arr<(float Wavelength, Vector3 Matching)> Tabulated() {
        using StreamReader reader = new(typeof(StandardObserver).Assembly.GetManifestResourceStream(typeof(StandardObserver), "Tables.CIE_xyz_1931_2deg.csv")!);
        return [.. reader.ReadToEnd().Split('\n', StringSplitOptions.RemoveEmptyEntries).Select(static line =>
            line.Split(',') switch {
                var cells => (Parsed(cells[0]), new Vector3(Parsed(cells[1]), Parsed(cells[2]), Parsed(cells[3]))),
            })];

        static float Parsed(string cell) => float.Parse(cell, CultureInfo.InvariantCulture);
    }
}
