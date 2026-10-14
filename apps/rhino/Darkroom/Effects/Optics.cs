using System.Runtime.InteropServices;
using Rasm.Imaging.Filters.Generators;
using Rasm.Imaging.Filters.Optics;
using Rasm.Imaging.Pixels;
using Rasm.Rhino.Render.Effects;
using Rasm.Rhino.UI.Rows;
using Rhino.Render.PostEffects;

namespace Darkroom.Effects;

// --- [CONSTANTS] -----------------------------------------------------------------------
internal static class LensText {
    public static readonly ParameterText Blades = ParameterText.Of("Flaps", "Aperture blades shaping each out-of-focus disc and diffraction spike");
    public static readonly ParameterText Rotation = ParameterText.Of("Angle", "Turn of the aperture blades, counterclockwise");
    public static readonly ParameterText Roundness = ParameterText.Of("Roundness", "Blade curvature from straight edges at 0 to a circle at 1");
    public static readonly ParameterText Obstruction = ParameterText.Of("Catadioptric Size", "Central obstruction of a mirror lens, a ring in place of a disc");
    public static readonly ParameterText Squeeze = ParameterText.Of("Anamorphic", "Horizontal squeeze of the aperture, 2 for a 2x anamorphic lens");
    public static readonly ParameterText Fringe = ParameterText.Of("Color Shift", "Red and blue disc sizes apart from green");
    public static readonly ParameterText Framing = ParameterText.Choice<LensFraming, InvalidOptics>(
        "Fit", "Scales the warped frame so its edges or its corners stay filled", static framing => framing.Map(edges: "Edges", corners: "Corners"));
}

// --- [COMPOSITION] ---------------------------------------------------------------------
[Guid("7400690e-f17b-4539-a88a-38b5bfa154fd")]
[CustomPostEffect(PostEffectType.Early, Caption)]
public sealed class LensBlur : StageEffect<LensBlur, Defocus, DefocusParameter, InvalidOptics>, IEffectDefinition<Defocus, DefocusParameter> {
    public const string Caption = "Lens Blur";

    public static EffectRow<Defocus, DefocusParameter> Row { get; } = new(static parameter => parameter.Map(
        field: ParameterText.Choice<FocusField, InvalidOptics>(
            "Focus", "Field the blur radius follows across the frame",
            static field => field.Map(uniform: "Whole Frame", depth: "Depth of Field", curvature: "Field Curvature", tilt: "Tilt-Shift")),
        radius: ParameterText.Of("Max Blur", "Largest blur radius as a share of the frame's short side"),
        aperture: ParameterText.Of("F-Stop", "F-Stop ratio of the lens the depth of field draws, lower numbers giving more defocus"),
        centerX: ParameterText.Of("Center X", "Sharp region's center, rightward from the frame's left edge"),
        centerY: ParameterText.Of("Center Y", "Sharp region's center, downward from the frame's top edge"),
        angle: ParameterText.Of("Orientation", "Turn of the sharp band or ellipse, counterclockwise"),
        extent: ParameterText.Of("Sharp Width", "Half-width of the sharp region across its band"),
        aspect: ParameterText.Of("Aspect", "Sharp ellipse from a band at 0 to a circle at 1"),
        feather: ParameterText.Of("Feather", "Distance past the sharp region where the blur reaches its maximum"),
        blades: LensText.Blades,
        rotation: LensText.Rotation,
        roundness: LensText.Roundness,
        obstruction: LensText.Obstruction,
        squeeze: LensText.Squeeze,
        fringe: LensText.Fringe));
}

[Guid("944522ca-dde2-4e3e-81b0-82565728951b")]
[CustomPostEffect(PostEffectType.Early, Caption)]
public sealed class RadialDistortion : StageEffect<RadialDistortion, LensDistortion, LensDistortionParameter, InvalidOptics>, IEffectDefinition<LensDistortion, LensDistortionParameter> {
    public const string Caption = "Lens Distortion";

    public static EffectRow<LensDistortion, LensDistortionParameter> Row { get; } = new(static parameter => parameter.Map(
        distortion: ParameterText.Of("Distortion", "Bends straight lines away from or toward the frame center"),
        dispersion: ParameterText.Of("Dispersion", "Spreads red and blue apart across each wavelength, growing toward the edges"),
        framing: LensText.Framing,
        direction: ParameterText.Choice<LensDirection, InvalidOptics>(
            "Direction", "Applies the lens warp or removes it", static direction => direction.Map(distort: "Distort", undistort: "Undistort")),
        wrap: ParameterText.Choice<WrapMode, InvalidPixelValue>(
            "Wrap", "Sampling beyond the frame boundary", ToneText.WrapText)));
}

[Guid("106bf35b-4759-4e08-a0a3-38f02c7be061")]
[CustomPostEffect(PostEffectType.Early, Caption)]
public sealed class AxialFringing : StageEffect<AxialFringing, AxialAberration, AxialAberrationParameter, InvalidOptics>, IEffectDefinition<AxialAberration, AxialAberrationParameter> {
    public const string Caption = "Axial Fringing";

    public static EffectRow<AxialAberration, AxialAberrationParameter> Row { get; } = new(static parameter => parameter.Map(
        radius: ParameterText.Of("Radius", "Blue defocus radius, red at half of it, the focus each wavelength misses against green"),
        blades: LensText.Blades,
        rotation: LensText.Rotation,
        roundness: LensText.Roundness,
        obstruction: LensText.Obstruction,
        squeeze: LensText.Squeeze));
}

[Guid("b8bf3b1d-0d30-4019-b5d5-417dba7936db")]
[CustomPostEffect(PostEffectType.Early, Caption)]
public sealed class LensBloom : StageEffect<LensBloom, Bloom, BloomParameter, InvalidOptics>, IEffectDefinition<Bloom, BloomParameter> {
    public const string Caption = "Lens Bloom";

    public static EffectRow<Bloom, BloomParameter> Row { get; } = new(static parameter => parameter.Map(
        size: ParameterText.Of("Size", "Reach of the spread across the frame"),
        strength: ParameterText.Of("Strength", "Share of the light the lens scatters, total light kept"),
        dirt: ParameterText.Of("Lens Dirt", "Scene-linear image of dust and smudges the scattered light shows"),
        specks: ParameterText.Of("Specks", "Procedural lens dirt in place of the dirt image") with {
            Parameters = ParameterText.Fields<DirtSpecks, DirtSpecksParameter, InvalidOptics>(static specks => specks.Map(
                coverage: ParameterText.Of("Coverage", "Share of the lens covered by dirt"),
                scale: ParameterText.Of("Scale", "Size of the dirt features as a share of the frame's short side"),
                seed: ParameterText.Of("Seed", "Number selecting the dirt field"))),
        },
        dirtGain: ParameterText.Of("Dirt Intensity", "Gain of the dirt over the scattered light")));
}

[Guid("134e641a-47aa-4f42-9084-8040f3568e94")]
[CustomPostEffect(PostEffectType.Early, Caption)]
public sealed class VeilingFlare : StageEffect<VeilingFlare, VeilingGlare, VeilingGlareParameter, InvalidOptics>, IEffectDefinition<VeilingGlare, VeilingGlareParameter> {
    public const string Caption = "Veiling Flare";

    public static EffectRow<VeilingGlare, VeilingGlareParameter> Row { get; } = new(static parameter => parameter.Map(
        size: ParameterText.Of("Size", "Field of view the glare kernel spans, narrower at higher values")));
}

[Guid("166dcfdb-9388-4cde-ac2b-2c5fe76e4604")]
[CustomPostEffect(PostEffectType.Early, Caption)]
public sealed class ApertureDiffraction : StageEffect<ApertureDiffraction, StarGlare, StarGlareParameter, InvalidOptics>, IEffectDefinition<StarGlare, StarGlareParameter> {
    public const string Caption = "Aperture Diffraction";

    public static EffectRow<StarGlare, StarGlareParameter> Row { get; } = new(static parameter => parameter.Map(
        blades: LensText.Blades,
        rotation: LensText.Rotation,
        roundness: LensText.Roundness,
        obstruction: LensText.Obstruction,
        squeeze: LensText.Squeeze,
        airyRadius: ParameterText.Of("Core", "Radius of the first dark ring around a point light, as a share of the frame's short side")));
}

[Guid("d3df8c60-5cc3-4c70-bbf7-2996bd2d7fd0")]
[CustomPostEffect(PostEffectType.Early, Caption)]
public sealed class LensReflections : StageEffect<LensReflections, Ghosts, GhostsParameter, InvalidOptics>, IEffectDefinition<Ghosts, GhostsParameter> {
    public const string Caption = "Lens Reflections";

    public static EffectRow<Ghosts, GhostsParameter> Row { get; } = new(static parameter => parameter.Map(
        threshold: ParameterText.Of("Threshold", "Scene-linear value above which highlights reflect"),
        softness: ParameterText.Of("Smoothness", "Knee width below the threshold"),
        iterations: ParameterText.Of("Iterations", "Reflection passes, each adding four ghosts"),
        colorModulation: ParameterText.Of("Color Modulation", "Color shift between successive ghosts")));
}

[Guid("f4afa240-2e97-469d-8f23-ce02aab2f499")]
[CustomPostEffect(PostEffectType.Early, Caption)]
public sealed class NaturalVignetting : StageEffect<NaturalVignetting, Vignette, VignetteParameter, InvalidOptics>, IEffectDefinition<Vignette, VignetteParameter> {
    public const string Caption = "Natural Vignetting";

    public static EffectRow<Vignette, VignetteParameter> Row { get; } = new(static parameter => parameter.Map(
        falloff: ParameterText.Variant<Vignette>("Falloff", "Camera illumination or a feathered outline", static mode => mode.Map(
            natural: ("Natural", HashMap<string, ParameterText>()),
            outline: ("Outline", ParameterText.Fields<Vignette.Outline, VignetteOutlineParameter, InvalidOptics>(static outline => outline.Map(
                shape: ParameterText.Of("Boundary", "Figure bounding the illuminated region") with {
                    Parameters = ParameterText.Fields<ShapePrimitive, ShapePrimitiveParameter, InvalidGenerator>(ShapeText.Text),
                },
                feather: ParameterText.Of("Feather", "Edge softness as a share of the frame's short side"),
                pull: ParameterText.Of("Pull", "Scene-linear color beyond the outline"))))))));
}

[Guid("6a47cbbf-f6a2-4b53-9b6d-6ccd6c5ede48")]
[CustomPostEffect(PostEffectType.Early, Caption)]
public sealed class FilmHalation : StageEffect<FilmHalation, Halation, HalationParameter, InvalidOptics>, IEffectDefinition<Halation, HalationParameter> {
    public const string Caption = "Halation";

    public static EffectRow<Halation, HalationParameter> Row { get; } = new(static parameter => parameter.Map(
        threshold: ParameterText.Of("Threshold", "Scene-linear value above which highlights re-expose the film"),
        softness: ParameterText.Of("Smoothness", "Knee width below the threshold"),
        radius: ParameterText.Of("Radius", "Halo radius as a share of the frame's short side"),
        color: ParameterText.Of("Color", "Scene-linear color weighting the halo")));
}
