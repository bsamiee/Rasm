using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using SixLabors.ImageSharp.Metadata.Profiles.Cicp;
using Wacton.Unicolour;

namespace Rasm.Imaging.Color;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record Gamut {
    private static readonly WhitePoint D65 = Illuminant.D65.GetWhitePoint(Observer.Degree2);

    private readonly (Vector2 Red, Vector2 Green, Vector2 Blue, Vector2 White) primaries;

    private Gamut(RgbConfiguration rgb) {
        (Rgb, Configuration) = (rgb, new Configuration(rgbConfig: rgb));
        ToXyz = Columns(Configuration, ColourSpace.RgbLinear, Configuration, ColourSpace.Xyz);
        primaries = (Point(rgb.ChromaticityR), Point(rgb.ChromaticityG), Point(rgb.ChromaticityB), Point(rgb.WhitePoint.Chromaticity));
    }

    public static Gamut StandardRgb { get; } = new(RgbConfiguration.StandardRgb) { InteropId = "lin_rec709_scene", Cicp = CicpColorPrimaries.ItuRBt709_6 };
    public static Gamut DisplayP3 { get; } = new(RgbConfiguration.DisplayP3) { InteropId = "lin_p3d65_scene", Cicp = CicpColorPrimaries.SmpteEg432_1 };
    public static Gamut Rec2020 { get; } = new(RgbConfiguration.Rec2020) { InteropId = "lin_rec2020_scene", Cicp = CicpColorPrimaries.ItuRBt2020_2 };
    public static Gamut Rec601Line625 { get; } = new(RgbConfiguration.Rec601Line625) { Cicp = CicpColorPrimaries.ItuRBt601_7_625 };
    public static Gamut Rec601Line525 { get; } = new(RgbConfiguration.Rec601Line525) { Cicp = CicpColorPrimaries.ItuRBt601_7_525 };
    public static Gamut A98 { get; } = new(RgbConfiguration.A98) { InteropId = "lin_adobergb_scene" };
    public static Gamut Acescg { get; } = new(RgbConfiguration.Acescg) { InteropId = "lin_ap1_scene" };
    public static Gamut Aces20651 { get; } = new(RgbConfiguration.Aces20651) { InteropId = "lin_ap0_scene" };
    public static Gamut ProPhoto { get; } = new(RgbConfiguration.ProPhoto);
    public static Gamut EGamut { get; } = Primaries(new(0.8, 0.3177), new(0.18, 0.9), new(0.065, -0.0805), "E-Gamut") with { InteropId = "blender:lin_egamut_scene" };
    public static Gamut ArriWideGamut3 { get; } = Primaries(new(0.684, 0.313), new(0.221, 0.848), new(0.0861, -0.102), "ARRI Wide Gamut 3");
    public static Gamut ArriWideGamut4 { get; } = Primaries(new(0.7347, 0.2653), new(0.1424, 0.8576), new(0.0991, -0.0308), "ARRI Wide Gamut 4");
    public static Gamut SGamut3 { get; } = Primaries(new(0.73, 0.28), new(0.14, 0.855), new(0.1, -0.05), "S-Gamut3");
    public static Gamut SGamut3Cine { get; } = Primaries(new(0.766, 0.275), new(0.225, 0.8), new(0.089, -0.087), "S-Gamut3.Cine");
    public static Gamut VeniceSGamut3 { get; } = Primaries(new(0.740464264304292, 0.27936437475066), new(0.089241145423286, 0.893809528608105), new(0.110488236673827, -0.052579333080476), "Venice S-Gamut3");
    public static Gamut VeniceSGamut3Cine { get; } = Primaries(new(0.775901871567345, 0.274502392854799), new(0.188682902773355, 0.828684937020288), new(0.101337382499301, -0.089187517306263), "Venice S-Gamut3.Cine");
    public static Gamut CinemaGamut { get; } = Primaries(new(0.74, 0.27), new(0.17, 1.14), new(0.08, -0.1), "Cinema Gamut");
    public static Gamut RedWideGamutRgb { get; } = Primaries(new(0.780308, 0.304253), new(0.121595, 1.493994), new(0.095612, -0.084589), "REDWideGamutRGB");
    public static Gamut VGamut { get; } = Primaries(new(0.73, 0.28), new(0.165, 0.84), new(0.1, -0.03), "V-Gamut");
    public static Gamut DGamut { get; } = Primaries(new(0.71, 0.31), new(0.21, 0.88), new(0.09, -0.08), "D-Gamut");
    public static Gamut ProtuneNative { get; } = Primaries(new(0.698480461493841, 0.193026445370121), new(0.329555378387345, 1.024596624134644), new(0.108442631407675, -0.034678569754016), "Protune Native");
    public static Gamut BlackmagicWideGamut { get; } = Primaries(new(0.7177215, 0.3171181), new(0.228041, 0.861569), new(0.1005841, -0.0820452), "Blackmagic Wide Gamut");
    public static Gamut DaVinciWideGamut { get; } = Primaries(new(0.8, 0.313), new(0.1682, 0.9877), new(0.079, -0.1155), "DaVinci Wide Gamut");
    public static Gamut AppleWideGamut { get; } = Primaries(new(0.725, 0.301), new(0.221, 0.814), new(0.068, -0.076), "Apple Wide Gamut");

    public static Seq<Gamut> Known { get; } = [
        StandardRgb, DisplayP3, Rec2020, Rec601Line625, Rec601Line525, A98, Acescg, Aces20651, ProPhoto, EGamut, ArriWideGamut3, ArriWideGamut4, SGamut3, SGamut3Cine,
        VeniceSGamut3, VeniceSGamut3Cine, CinemaGamut, RedWideGamutRgb, VGamut, DGamut, ProtuneNative, BlackmagicWideGamut, DaVinciWideGamut, AppleWideGamut,
    ];

    public RgbConfiguration Rgb { get; }
    public Configuration Configuration { get; }
    public Option<string> InteropId { get; private init; }
    public Option<CicpColorPrimaries> Cicp { get; private init; }
    public Matrix4x4 ToXyz { get; }

    public Vector3 Luminance => new(ToXyz.M12, ToXyz.M22, ToXyz.M32);

    public YbrConfiguration Ybr => new(Luminance.X, Luminance.Z, Configuration.Ybr.RangeY, Configuration.Ybr.RangeC, Rgb.Name);

    public bool Equals([NotNullWhen(true)] Gamut? other) => other is not null && primaries == other.primaries;

    public override int GetHashCode() => primaries.GetHashCode();

    public Matrix4x4 MatrixTo(Gamut target) => Columns(Configuration, ColourSpace.RgbLinear, target.Configuration, ColourSpace.RgbLinear);

    public Matrix4x4 Adapting(WhitePoint source, Option<WhitePoint> destination = default, Option<ChromaticAdaptation> transform = default) =>
        (destination.IfNone(Configuration.Xyz.WhitePoint), transform.IfNone(ChromaticAdaptation.Bradford)) switch {
            var (white, adaptation) => ToXyz
                * Columns(new(xyzConfig: new(source, Observer.Degree2, adaptation)), ColourSpace.Xyz, new(xyzConfig: new(white, Observer.Degree2, adaptation)), ColourSpace.Xyz)
                * Columns(Configuration, ColourSpace.Xyz, Configuration, ColourSpace.RgbLinear),
        };

    public Fin<Wacton.Unicolour.Temperature> Temperature(Vector3 rgb) =>
        new Unicolour(Configuration, ColourSpace.RgbLinear, rgb.X, rgb.Y, rgb.Z).Temperature switch {
            var correlated => ImagingFault.Unless(Vector3.Dot(Luminance, rgb) > 0f && correlated.IsValid, ImagingReason.Range, (rgb, correlated)).Map(_ => correlated),
        };

    public Gamut Inset(Vector3 inset, Vector3 rotation) {
        Chromaticity white = Rgb.WhitePoint.Chromaticity;
        Seq<(Chromaticity From, Chromaticity To)> sides = [(Rgb.ChromaticityR, Rgb.ChromaticityG), (Rgb.ChromaticityG, Rgb.ChromaticityB), (Rgb.ChromaticityB, Rgb.ChromaticityR)];
        return new(new RgbConfiguration(
            Moved(Rgb.ChromaticityR, inset.X, rotation.X), Moved(Rgb.ChromaticityG, inset.Y, rotation.Y), Moved(Rgb.ChromaticityB, inset.Z, rotation.Z), Rgb.WhitePoint, identity, identity));

        Chromaticity Moved(Chromaticity primary, double share, double turn) {
            (double sin, double cos) = double.SinCos(double.Atan2(primary.Y - white.Y, primary.X - white.X) + turn);
            double edge = sides
                .Map(side => (((white.X - side.From.X) * (side.From.Y - side.To.Y)) - ((side.From.X - side.To.X) * (white.Y - side.From.Y)))
                    / ((sin * (side.From.X - side.To.X)) - (cos * (side.From.Y - side.To.Y))))
                .Filter(static reach => reach >= 0d)
                .Fold(double.PositiveInfinity, double.Min);
            return new(white.X + ((1d - share) * edge * cos), white.Y + ((1d - share) * edge * sin));
        }
    }

    public static Fin<Gamut> Of(Chromaticity red, Chromaticity green, Chromaticity blue, WhitePoint white) =>
        new Gamut(new RgbConfiguration(red, green, blue, white, identity, identity)) switch {
            var stated => ImagingFault.Unless(float.IsNormal(stated.ToXyz.GetDeterminant()), ImagingReason.Degenerate, (red, green, blue, white))
                .Map(_ => Known.Find(stated.Equals).IfNone(stated)),
        };

    public static Matrix4x4 Columns(Func<Vector3, Vector3> image) =>
        (image(Vector3.UnitX), image(Vector3.UnitY), image(Vector3.UnitZ)) switch {
            var (r, g, b) => new(r.X, r.Y, r.Z, 0f, g.X, g.Y, g.Z, 0f, b.X, b.Y, b.Z, 0f, 0f, 0f, 0f, 1f),
        };

    public static Matrix4x4 Columns(Configuration source, ColourSpace from, Configuration target, ColourSpace to) =>
        Columns(basis => new Unicolour(source, from, basis.X, basis.Y, basis.Z).ConvertToConfiguration(target).GetRepresentation(to).Tuple switch {
            var (first, second, third) => new Vector3((float)first, (float)second, (float)third),
        });

    private static Gamut Primaries(Chromaticity red, Chromaticity green, Chromaticity blue, string name) => new(new RgbConfiguration(red, green, blue, D65, identity, identity, name));

    private static Vector2 Point(Chromaticity chromaticity) => new((float)chromaticity.X, (float)chromaticity.Y);
}

[ComplexValueObject(SkipFactoryMethods = true)]
public sealed partial class GamutCompression : IFixedFunction {
    public static GamutCompression Reference { get; } = new(new Vector3(0.815f, 0.803f, 0.880f), new Vector3(1.147f, 1.264f, 1.312f), 1.2f);

    public Vector3 Threshold { get; }
    public Vector3 Limit { get; }
    public float Power { get; }

    public LutTable Within(Gamut working, Gamut target) => LutTable.Of([new LutTable.Affine(working.MatrixTo(target)), new LutTable.FixedFunction(this), new LutTable.Affine(target.MatrixTo(working))]);

    public void Apply(Span<Vector4> row, bool inverted) {
        Vector3 scale = (Vector3.One - Threshold) / Raised(Vector3.One - Raised((Vector3.One - Threshold) / (Limit - Threshold), Power), 1f / Power);
        Vector3 ceiling = inverted ? Vector3.One : new Vector3(float.PositiveInfinity);
        foreach (ref Vector4 pixel in row) {
            float achromatic = float.Max(pixel.X, float.Max(pixel.Y, pixel.Z));
            float reach = float.Abs(achromatic);
            Vector3 distance = reach is 0f ? Vector3.Zero : (new Vector3(achromatic) - pixel.AsVector3()) / reach;
            Vector3 over = Vector3.Max((distance - Threshold) / scale, Vector3.Zero);
            Vector3 moved = Threshold + (scale * (inverted
                ? Raised(Raised(over, Power) / (Vector3.One - Raised(over, Power)), 1f / Power)
                : over / Raised(Vector3.One + Raised(over, Power), 1f / Power)));
            Vector3 kept = Vector3.ConditionalSelect(Vector3.LessThan(distance, Threshold) | Vector3.GreaterThanOrEqual(over, ceiling), distance, moved);
            pixel = new Vector4(new Vector3(achromatic) - (kept * reach), pixel.W);
        }
    }

    public static Fin<GamutCompression> Validated(Vector3 threshold, Vector3 limit, float power) =>
        ImagingFault.Unless(
            Vector3.AllWhereAllBitsSet(Vector3.GreaterThanOrEqual(threshold, Vector3.Zero) & Vector3.LessThan(threshold, Vector3.One) & Vector3.GreaterThan(limit, Vector3.One) & Vector3.IsFinite(limit))
                && float.IsFinite(power) && power > 0f,
            ImagingReason.Range, (threshold, limit, power))
            .Map(_ => new GamutCompression(threshold, limit, power));

    private static Vector3 Raised(Vector3 value, float power) => new(float.Pow(value.X, power), float.Pow(value.Y, power), float.Pow(value.Z, power));
}
