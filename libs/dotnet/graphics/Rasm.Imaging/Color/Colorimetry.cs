using System.Numerics;
using System.Runtime.CompilerServices;
using Wacton.Unicolour;

namespace Rasm.Imaging.Color;

// --- [OPERATIONS] ----------------------------------------------------------------------

public static class StandardObserver {
    public static Seq<Illuminant> Illuminants { get; } = Named();

    public static Arr<(float Wavelength, Vector3 Matching)> Matching(Gamut working) =>
        Gamut.Columns(working.Configuration, ColourSpace.Xyz, working.Configuration, ColourSpace.RgbLinear) switch {
            var matrix => working.Configuration.Xyz.Observer.Tabulated.Map(row => (row.Wavelength, Vector3.Transform(row.Matching, matrix))),
        };

    extension(Observer observer) {
        public Arr<(float Wavelength, Vector3 Matching)> Tabulated =>
            [.. Functions(observer).OrderBy(static row => row.Key).Select(static row => ((float)row.Key, new Vector3((float)row.Value.x, (float)row.Value.y, (float)row.Value.z)))];

        public Arr<Vector2> Locus =>
            observer.Tabulated.Map(static row => row.Matching).Filter(static matching => Vector3.Dot(matching, Vector3.One) > 0f).Map(static matching => new Vector2(matching.X, matching.Y) / Vector3.Dot(matching, Vector3.One));
    }

    extension(Wacton.Unicolour.Temperature temperature) {
        public WhitePoint WhitePoint => new Unicolour(Configuration.Default, temperature).Chromaticity switch { var (x, y) => new WhitePoint(x, y) };

        public Unicolour Emission(Gamut gamut) =>
            new Unicolour(gamut.Configuration, temperature).RgbLinear switch {
                var rgb => double.Max(double.Abs(rgb.R), double.Max(double.Abs(rgb.G), double.Abs(rgb.B))) switch {
                    var peak => new(gamut.Configuration, new Unicolour(gamut.Configuration, ColourSpace.RgbLinear, rgb.R / peak, rgb.G / peak, rgb.B / peak).MapToRgbGamut().Chromaticity),
                },
            };
    }

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "cmf")]
    private static extern ref readonly Cmf Functions(Observer observer);

    private static Seq<Illuminant> Named() {
        static Illuminant At(string name, Chromaticity xy) => new(new WhitePoint(xy.X, xy.Y), $"Illuminant {name}");
        return [
            Illuminant.A, At("B", new(0.34842d, 0.35161d)), Illuminant.C, Illuminant.D50, Illuminant.D55, Illuminant.D65, Illuminant.D75,
            At("D93", new Unicolour(9300d, Locus.Daylight).Chromaticity), Illuminant.E, At("F1", new(0.3131d, 0.3371d)), Illuminant.F2,
            At("F3", new(0.4091d, 0.3941d)), At("F4", new(0.4402d, 0.4031d)), At("F5", new(0.3138d, 0.3452d)), At("F6", new(0.3779d, 0.3882d)), Illuminant.F7,
            At("F8", new(0.3458d, 0.3586d)), At("F9", new(0.3741d, 0.3727d)), At("F10", new(0.3458d, 0.3588d)), Illuminant.F11, At("F12", new(0.437d, 0.4042d)),
            At("LED-B1", new(0.456d, 0.4078d)), At("LED-B2", new(0.4357d, 0.4012d)), At("LED-B3", new(0.3756d, 0.3723d)), At("LED-B4", new(0.3422d, 0.3502d)),
            At("LED-B5", new(0.3118d, 0.3236d)), At("LED-BH1", new(0.4474d, 0.4066d)), At("LED-RGB1", new(0.4557d, 0.4211d)), At("LED-V1", new(0.4548d, 0.4044d)), At("LED-V2", new(0.3781d, 0.3775d)),
        ];
    }
}
