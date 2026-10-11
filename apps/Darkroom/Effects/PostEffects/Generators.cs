using Rasm.Imaging.Filters.Generators;
using Rasm.Rhino.Render.Effects;

using System.Runtime.InteropServices;
using Rasm.Imaging.Grade;
using Rasm.Imaging.Pixels;
using Rhino.Render.PostEffects;

namespace Darkroom.Effects;

// --- [OPERATIONS] ----------------------------------------------------------------------
file static class GeneratorText {
    public static ParameterText Text(NoiseTextureParameter parameter) =>
        parameter.Map(
            noise: ParameterText.Of("Noise", "Pattern, feature size, and motion of the field") with {
                Parameters = ParameterText.Fields<NoiseSource, NoiseSourceParameter, InvalidGenerator>(Text),
            },
            placement: Placement,
            vortex: ParameterText.Of("Vortex", "Turns the field around its center and deepens its sampled axis") with {
                Parameters = ParameterText.Fields<VortexField, VortexFieldParameter, InvalidGenerator>(static vortex => vortex.Map(
                    arms: ParameterText.Of("Arms", "Repeats the field around each turn, 0 drawing concentric rings"),
                    twist: ParameterText.Of("Twist", "Turn at the center falling toward the vortex radius"),
                    radius: ParameterText.Of("Vortex Radius", "Distance where the tunnel stops deepening, as a share of the frame's short side"),
                    tunnel: ParameterText.Of("Tunnel", "Depth sampled along the vortex axis per unit of radius"),
                    spin: ParameterText.Of("Spin", "Turns per second the field rotates about its center"))),
            },
            warp: ParameterText.Of("Warp", "Displacement by the warp noise as a share of the frame's short side"),
            warpBasis: ParameterText.Of("Warp Noise", "Pattern displacing the field") with {
                Parameters = ParameterText.Fields<NoiseBasis, NoiseBasisParameter, InvalidGenerator>(static basis => NoiseText.Basis(basis, "Warp ")),
            },
            colors: ParameterText.Choice<RampInterpolation, InvalidPixelValue>(
                "Colors", "Light and coverage over the noise value, each stop a color and its coverage", ToneText.InterpolationText),
            envelope: ParameterText.Of("Envelope", "Elliptical falloff shaping the generated field") with {
                Parameters = ParameterText.Fields<FieldEnvelope, FieldEnvelopeParameter, InvalidGenerator>(static envelope => envelope.Map(
                    width: ParameterText.Of("Width", "Half width of the envelope as a share of the frame's short side"),
                    height: ParameterText.Of("Height", "Half height of the envelope as a share of the frame's short side"))),
            },
            incandescence: Emission,
            extinction: ParameterText.Of("Extinction", "Optical depth at full coverage, the field absorbing light as smoke"),
            layer: Layer,
            seed: NoiseText.Seed,
            timing: NoiseText.Timing);

    public static ParameterText Layer => ParameterText.Of("Layer", "Composition of the generated layer over the frame") with {
        Parameters = ParameterText.Fields<GeneratedLayer, GeneratedLayerParameter, InvalidGenerator>(static layer => layer.Map(
            mode: ParameterText.Choice<BlendingMode, InvalidGrade>(
                "Blend", "Way the generated layer joins the frame, Mix covering it and Add adding light", ToneText.BlendText),
            exposure: ParameterText.Of("Exposure", "Stops scaling the generated layer"))),
    };

    public static ParameterText Placement => ParameterText.Of("Placement", "Position and turn of the field") with {
        Parameters = ParameterText.Fields<Placement, PlacementParameter, InvalidGenerator>(PrimitiveText.Text),
    };

    public static ParameterText Source => ParameterText.Of("Source", "Frame position or scene light the generated light follows") with {
        Parameters = ParameterText.Fields<FlareSource, FlareSourceParameter, InvalidGenerator>(static source => source.Map(
            anchor: ParameterText.Choice<FlareAnchor, InvalidGenerator>(
                "Source", "What the generated light converges on", static anchor => anchor.Map(frame: "Frame Position", sun: "Sun", light: "Light")),
            positionX: ParameterText.Of("Source X", "Source under Frame Position, rightward from the frame's left edge"),
            positionY: ParameterText.Of("Source Y", "Source under Frame Position, downward from the frame's top edge"),
            light: ParameterText.Of("Light", "Document light the source follows under Light"),
            occlusion: ParameterText.Of("Occlusion", "Surfaces nearer the camera than the source hide its generated light"))),
    };

    public static ParameterText Feather => ParameterText.Of("Feather", "Edge softness as a share of the frame's short side, 0 one pixel wide");

    public static ParameterText Style => ParameterText.Of("Style", "Fill and boundary of the generated figure") with {
        Parameters = ParameterText.Fields<ShapeStyle, ShapeStyleParameter, InvalidGenerator>(static style => style.Map(
            fill: ParameterText.Of("Fill", "Color inside the boundary, off drawing the outline alone"),
            outline: ParameterText.Of("Outline", "Color of the band around the boundary"),
            outlineWidth: ParameterText.Of("Outline Width", "Width of the outline band as a share of the frame's short side"),
            outlineOffset: ParameterText.Of("Outline Offset", "Distance of the outline from the boundary as a share of the frame's short side"),
            feather: Feather,
            roughness: ParameterText.Of("Rough Edge", "Noise displacement of the edge as a share of the frame's short side"),
            inverted: ParameterText.Of("Invert", "Fills outside the boundary"))),
    };

    public static ParameterText Emission => ParameterText.Of("Blackbody", "Blackbody light between the cool and hot temperatures") with {
        Parameters = ParameterText.Fields<Emission, EmissionParameter, InvalidGenerator>(static emission => emission.Map(
            cool: ParameterText.Of("Cool", "Blackbody temperature at the low end of the field"),
            hot: ParameterText.Of("Hot", "Blackbody temperature at the high end of the field"))),
    };

    public static ParameterText Text(NoiseSourceParameter parameter) =>
        parameter.Map(
            basis: ParameterText.Of("Basis", "Noise the field draws") with {
                Parameters = ParameterText.Fields<NoiseBasis, NoiseBasisParameter, InvalidGenerator>(static basis => NoiseText.Basis(basis)),
            },
            scaleX: ParameterText.Of("Scale X", "Feature width as a share of the frame's short side"),
            scaleY: ParameterText.Of("Scale Y", "Feature height as a share of the frame's short side"),
            driftX: ParameterText.Of("Drift X", "Short sides per second the field moves rightward"),
            driftY: ParameterText.Of("Drift Y", "Short sides per second the field moves upward"),
            evolution: ParameterText.Of("Evolution", "Noise units per second the field changes through"));
}

// --- [COMPOSITION] ---------------------------------------------------------------------
[Guid("5bfb53d0-6c96-4fc2-bf47-fd7662e4182b")]
[CustomPostEffect(PostEffectType.Early, Caption)]
public sealed class NightSky : StageEffect<NightSky, Starfield, StarfieldParameter, InvalidGenerator>, IEffectDefinition<Starfield, StarfieldParameter> {
    public const string Caption = "Starfield";

    public static EffectRow<Starfield, StarfieldParameter> Row { get; } = new(static parameter =>
        parameter.Map(
            placement: GeneratorText.Placement,
            stars: ParameterText.Of("Stars", "Cellular field of the bright stars") with {
                Parameters = ParameterText.Fields<NoiseBasis, NoiseBasisParameter, InvalidGenerator>(static basis => NoiseText.Basis(basis, "Stars ")),
            },
            starScale: ParameterText.Of("Star Scale", "Cell side of the bright stars as a share of the frame's short side"),
            starRadius: ParameterText.Of("Star Radius", "Reach of each bright star as a share of the frame's short side"),
            starColors: ParameterText.Choice<RampInterpolation, InvalidPixelValue>("Star Colors", "Colors the bright stars draw from", ToneText.InterpolationText),
            starExposure: ParameterText.Of("Star Exposure", "Stops of the bright stars"),
            twinkle: ParameterText.Of("Twinkle", "Share of each star's light the twinkle dims, 0 holding every star steady"),
            twinkleRate: ParameterText.Of("Twinkle Rate", "Twinkle periods per second"),
            nebula: ParameterText.Of("Nebula", "Pattern, feature size, and motion of the nebula") with {
                Parameters = ParameterText.Fields<NoiseSource, NoiseSourceParameter, InvalidGenerator>(GeneratorText.Text),
            },
            nebulaDensity: ParameterText.Of("Nebula Density", "Share of the noise axis the nebula covers"),
            nebulaColors: ParameterText.Choice<RampInterpolation, InvalidPixelValue>("Nebula Colors", "Colors the nebula draws by its density", ToneText.InterpolationText),
            nebulaExposure: ParameterText.Of("Nebula Exposure", "Stops of the nebula"),
            layer: GeneratorText.Layer,
            seed: NoiseText.Seed,
            timing: NoiseText.Timing));
}

[Guid("a65107ea-d4f1-4b61-ac09-b74d67ba8935")]
[CustomPostEffect(PostEffectType.Early, Caption)]
public sealed class Atmosphere : StageEffect<Atmosphere, Fog, FogParameter, InvalidGenerator>, IEffectDefinition<Fog, FogParameter> {
    public const string Caption = "Atmosphere";

    public static EffectRow<Fog, FogParameter> Row { get; } = new(static parameter =>
        parameter.Map(
            noise: ParameterText.Of("Wisps", "Pattern, feature size, and motion of the medium") with {
                Parameters = ParameterText.Fields<NoiseSource, NoiseSourceParameter, InvalidGenerator>(GeneratorText.Text),
            },
            variation: ParameterText.Of("Variation", "Density the wisps vary by, 0 a uniform medium"),
            start: ParameterText.Of("Start", "Distance from the camera where the medium begins"),
            attenuation: ParameterText.Of("Attenuation", "Distance through the medium that passes 1/e of the light behind it"),
            cutoff: ParameterText.Of("Cutoff", "Distance past which a surface keeps its own light, the background clear"),
            diffusion: ParameterText.Of("Diffusion", "Blur radius of the frame's own light the medium scatters, off for an evenly lit medium"),
            amount: ParameterText.Of("Density", "Share of the medium's coverage before the layer joins the frame"),
            colors: ParameterText.Choice<RampInterpolation, InvalidPixelValue>(
                "Colors", "Light the medium scatters toward the camera, near to far by its optical depth", ToneText.InterpolationText),
            layer: GeneratorText.Layer,
            seed: NoiseText.Seed,
            timing: NoiseText.Timing)) {
        Looks = [("Haze", new EffectLook<Fog, FogParameter>.Whole(Fog.Haze))],
    };
}

[Guid("77743df8-86ad-48bc-b60a-a08a5dc51818")]
[CustomPostEffect(PostEffectType.Early, Caption)]
public sealed class NoiseLayer : StageEffect<NoiseLayer, NoiseTexture, NoiseTextureParameter, InvalidGenerator>, IEffectDefinition<NoiseTexture, NoiseTextureParameter> {
    public const string Caption = "Noise Texture";

    public static EffectRow<NoiseTexture, NoiseTextureParameter> Row { get; } = new(GeneratorText.Text) {
        Looks = [
            ("Turbulent Noise", new EffectLook<NoiseTexture, NoiseTextureParameter>.Whole(NoiseTexture.TurbulentNoise)),
            ("Block Noise", new EffectLook<NoiseTexture, NoiseTextureParameter>.Whole(NoiseTexture.BlockNoise)),
            ("Diamonds", new EffectLook<NoiseTexture, NoiseTextureParameter>.Whole(NoiseTexture.Diamonds)),
            ("Voronoi Noise", new EffectLook<NoiseTexture, NoiseTextureParameter>.Whole(NoiseTexture.VoronoiNoise)),
            ("Sparkle", new EffectLook<NoiseTexture, NoiseTextureParameter>.Whole(NoiseTexture.Sparkle)),
            ("Voronoi Cells", new EffectLook<NoiseTexture, NoiseTextureParameter>.Whole(NoiseTexture.VoronoiCells)),
            ("Waves", new EffectLook<NoiseTexture, NoiseTextureParameter>.Whole(NoiseTexture.Waves)),
            ("Light Leak", new EffectLook<NoiseTexture, NoiseTextureParameter>.Whole(NoiseTexture.LightLeak)),
            ("Flow", new EffectLook<NoiseTexture, NoiseTextureParameter>.Whole(NoiseTexture.Flow)),
            ("Vortex", new EffectLook<NoiseTexture, NoiseTextureParameter>.Whole(NoiseTexture.Vortex)),
            ("Smoke", new EffectLook<NoiseTexture, NoiseTextureParameter>.Whole(NoiseTexture.Smoke)),
            ("Mist", new EffectLook<NoiseTexture, NoiseTextureParameter>.Whole(NoiseTexture.Mist)),
        ],
    };
}

[Guid("36ac0f92-c557-4d37-a728-cb2249510efe")]
[CustomPostEffect(PostEffectType.Early, Caption)]
public sealed class Flames : StageEffect<Flames, NoiseTexture, NoiseTextureParameter, InvalidGenerator>, IEffectDefinition<NoiseTexture, NoiseTextureParameter> {
    public const string Caption = "Fire";

    public static EffectRow<NoiseTexture, NoiseTextureParameter> Row { get; } = NoiseLayer.Row with { Default = NoiseTexture.Fire, Looks = [] };
}

[Guid("31555b4a-ff97-4387-946f-e7e9b0a131e8")]
[CustomPostEffect(PostEffectType.Early, Caption)]
public sealed class Particles : StageEffect<Particles, ParticleField, ParticleFieldParameter, InvalidGenerator>, IEffectDefinition<ParticleField, ParticleFieldParameter> {
    public const string Caption = "Particles";

    public static EffectRow<ParticleField, ParticleFieldParameter> Row { get; } = new(static parameter =>
        parameter.Map(
            cells: ParameterText.Of("Population", "Spacing, presence, size, and light of the particles") with {
                Parameters = ParameterText.Fields<PointCells, PointCellsParameter, InvalidGenerator>(static cells => NoiseText.PointCells(cells)),
            },
            layers: ParameterText.Of("Layers", "Depth layers of the particle field"),
            heading: ParameterText.Of("Heading", "Direction of travel, counterclockwise from the frame's right"),
            speed: ParameterText.Of("Speed", "Short sides per second the nearest layer travels"),
            basis: ParameterText.Of("Wind", "Noise displacing each particle in the wind field") with {
                Parameters = ParameterText.Fields<NoiseBasis, NoiseBasisParameter, InvalidGenerator>(static basis => NoiseText.Basis(basis)),
            },
            turbulence: ParameterText.Of("Turbulence", "Sway of each particle in the wind field as a share of the frame's short side"),
            eddy: ParameterText.Of("Eddy Size", "Feature width of the wind field as a share of the frame's short side"),
            evolution: ParameterText.Of("Evolution", "Noise units per second the wind field changes through"),
            depth: ParameterText.Of("Depth", "Distance of the farthest layer over the nearest"),
            depthVariation: ParameterText.Of("Depth Variation", "Variation of particle depth within each layer"),
            focus: ParameterText.Of("Focus", "Depth in focus, 0 the nearest layer and 1 the farthest"),
            defocus: ParameterText.Of("Defocus", "Largest out-of-focus disc radius as a share of the frame's short side"),
            blades: LensText.Blades, rotation: LensText.Rotation, roundness: LensText.Roundness,
            obstruction: LensText.Obstruction, squeeze: LensText.Squeeze, fringe: LensText.Fringe,
            shutter: ParameterText.Of("Shutter", "Seconds the shutter stays open, each particle streaking along its travel"),
            fade: ParameterText.Of("Fade", "Dimming of each streak toward its tail"),
            colors: ParameterText.Choice<RampInterpolation, InvalidPixelValue>("Colors", "Colors each particle draws from", ToneText.InterpolationText),
            covering: ParameterText.Of("Covering", "Particles cover the light behind them"),
            coating: ParameterText.Choice<RampInterpolation, InvalidPixelValue>("Coating", "Covering colors and opacity each particle draws from", ToneText.InterpolationText),
            radiance: ParameterText.Of("Radiance", "Stops of emitted particle light, independent of covering"),
            emission: GeneratorText.Emission,
            incandescence: ParameterText.Of("Incandescence", "Share of each particle's color taken from its blackbody light"),
            twinkle: ParameterText.Of("Twinkle", "Share of each particle's light varied over time"),
            twinkleRate: ParameterText.Of("Twinkle Rate", "Twinkle periods per second"),
            layer: GeneratorText.Layer,
            seed: NoiseText.Seed,
            timing: NoiseText.Timing)) {
        Looks = [
            ("Rain", new EffectLook<ParticleField, ParticleFieldParameter>.Whole(ParticleField.Rain)),
            ("Snow", new EffectLook<ParticleField, ParticleFieldParameter>.Whole(ParticleField.Snow)),
            ("Sparks", new EffectLook<ParticleField, ParticleFieldParameter>.Whole(ParticleField.Sparks)),
            ("Cell Rain", new EffectLook<ParticleField, ParticleFieldParameter>.Whole(ParticleField.CellRain)),
            ("Starfield", new EffectLook<ParticleField, ParticleFieldParameter>.Whole(ParticleField.Starfield)),
        ],
    };
}

[Guid("60ad6377-8592-43dd-acd2-cc3ef03f7b5f")]
[CustomPostEffect(PostEffectType.Early, Caption)]
public sealed class LightningBolt : StageEffect<LightningBolt, Lightning, LightningParameter, InvalidGenerator>, IEffectDefinition<Lightning, LightningParameter> {
    public const string Caption = "Lightning";

    public static EffectRow<Lightning, LightningParameter> Row { get; } = new(static parameter =>
        parameter.Map(
            placement: GeneratorText.Placement,
            length: ParameterText.Of("Length", "Half the stroke length as a share of the frame's short side"),
            thickness: ParameterText.Of("Thickness", "Width of the stroke as a share of the frame's short side"),
            depth: ParameterText.Of("Detail", "Subdivisions of each displaced segment"),
            displacement: ParameterText.Of("Displacement", "Share of each segment's length the stroke wanders by"),
            branches: ParameterText.Of("Branches", "Forks from the stroke's midpoints"),
            reach: ParameterText.Of("Reach", "Share of the stroke length the branches reach"),
            spread: ParameterText.Of("Spread", "Angle by which the branches turn from the stroke"),
            taper: ParameterText.Of("Taper", "Width of each branch against the stroke before narrowing to its tip"),
            speed: ParameterText.Of("Speed", "Noise units per second the bolt changes through"),
            style: GeneratorText.Style,
            layer: GeneratorText.Layer,
            seed: NoiseText.Seed,
            timing: NoiseText.Timing,
            hold: NoiseText.Hold));
}

[Guid("ff29c5af-33af-4817-90a1-910fdbb21909")]
[CustomPostEffect(PostEffectType.Early, Caption)]
public sealed class SunBeams : StageEffect<SunBeams, LightRays, LightRaysParameter, InvalidGenerator>, IEffectDefinition<LightRays, LightRaysParameter> {
    public const string Caption = "Light Rays";

    public static EffectRow<LightRays, LightRaysParameter> Row { get; } = new(static parameter =>
        parameter.Map(
            threshold: ParameterText.Of("Threshold", "Scene-linear value above which highlights cast rays"),
            softness: ParameterText.Of("Smoothness", "Knee width below the threshold"),
            source: GeneratorText.Source,
            size: ParameterText.Of("Size", "Longest ray march as a share of the frame's short side"),
            jitter: ParameterText.Of("Jitter", "Share of the march steps traded for hashed sample shifts"),
            tint: ParameterText.Of("Tint", "Scene-linear color of the rays"),
            layer: GeneratorText.Layer,
            seed: NoiseText.Seed,
            timing: NoiseText.Timing));
}

[Guid("05985670-b6cb-40da-9e8e-0f6ac2d30a70")]
[CustomPostEffect(PostEffectType.Early, Caption)]
public sealed class SourceFlare : StageEffect<SourceFlare, LensFlare, LensFlareParameter, InvalidGenerator>, IEffectDefinition<LensFlare, LensFlareParameter> {
    public const string Caption = "Lens Flare";

    public static EffectRow<LensFlare, LensFlareParameter> Row { get; } = new(static parameter =>
        parameter.Map(
            source: GeneratorText.Source,
            hotspot: ParameterText.Of("Hotspot", "Stops of the glow at the source, off drawing none"),
            hotspotColors: ParameterText.Choice<RampInterpolation, InvalidPixelValue>("Hotspot Colors", "Light and coverage from the center to the hotspot edge", ToneText.InterpolationText),
            hotspotSize: ParameterText.Of("Hotspot Size", "Radius of the hotspot as a share of the frame's short side"),
            hotspotSoftness: ParameterText.Of("Hotspot Softness", "Softness of the hotspot edge as a share of the frame's short side"),
            starburst: ParameterText.Of("Starburst", "Stops of the rays about the source, off drawing none"),
            starburstColors: ParameterText.Choice<RampInterpolation, InvalidPixelValue>("Starburst Colors", "Light and coverage from the source along each ray", ToneText.InterpolationText),
            starburstLength: ParameterText.Of("Starburst Length", "Reach of the rays as a share of the frame's short side"),
            basis: ParameterText.Of("Starburst Noise", "Pattern of the rays around the source") with {
                Parameters = ParameterText.Fields<NoiseBasis, NoiseBasisParameter, InvalidGenerator>(static basis => NoiseText.Basis(basis)),
            },
            starburstDensity: ParameterText.Of("Starburst Density", "Noise periods per turn about the source"),
            starburstSharpness: ParameterText.Of("Starburst Sharpness", "Power narrowing each ray around its brightest axis"),
            shimmer: ParameterText.Of("Shimmer", "Noise units per second the rays change through"),
            hold: NoiseText.Hold,
            bar: ParameterText.Of("Anamorphic Bar", "Stops of the bar through the source, off drawing none"),
            barColors: ParameterText.Choice<RampInterpolation, InvalidPixelValue>("Bar Colors", "Light and coverage from the source along the bar", ToneText.InterpolationText),
            barLength: ParameterText.Of("Bar Length", "Half length of the bar as a share of the frame's short side"),
            barThickness: ParameterText.Of("Bar Thickness", "Width of the bar as a share of the frame's short side"),
            barAngle: ParameterText.Of("Bar Angle", "Turn of the bar from horizontal, counterclockwise"),
            ring: ParameterText.Of("Halo", "Stops of the ring, off drawing none"),
            ringColors: ParameterText.Choice<RampInterpolation, InvalidPixelValue>("Halo Colors", "Light and coverage across the halo band", ToneText.InterpolationText),
            ringRadius: ParameterText.Of("Halo Radius", "Radius of the ring as a share of the frame's short side"),
            ringWidth: ParameterText.Of("Halo Width", "Width of the ring band as a share of the frame's short side"),
            ringSoftness: ParameterText.Of("Halo Softness", "Softness of the ring edges as a share of the frame's short side"),
            ringMagnification: ParameterText.Of("Halo Position", "Halo center along the source axis, 0 at frame center and 1 at source"),
            ghosts: ParameterText.Of("Ghosts", "Stops of the iris images along the source axis, off drawing none"),
            ghostColors: ParameterText.Choice<RampInterpolation, InvalidPixelValue>("Ghost Colors", "Light and coverage along the ghost chain", ToneText.InterpolationText),
            ghostSize: ParameterText.Of("Ghost Size", "Radius of the largest ghost as a share of the frame's short side"),
            ghostCount: ParameterText.Of("Ghost Count", "Iris images along the source axis"),
            ghostFirst: ParameterText.Of("First Ghost", "Start of the ghost chain, 0 at frame center and 1 at source"),
            ghostLast: ParameterText.Of("Last Ghost", "End of the ghost chain, 0 at frame center and 1 at source"),
            ghostVariation: ParameterText.Of("Ghost Variation", "Variation in each ghost's position and radius"),
            ghostSoftness: ParameterText.Of("Ghost Softness", "Share of each ghost's radius feathered at its edge"),
            blades: LensText.Blades, rotation: LensText.Rotation, roundness: LensText.Roundness,
            obstruction: LensText.Obstruction, squeeze: LensText.Squeeze, fringe: LensText.Fringe,
            layer: GeneratorText.Layer,
            seed: NoiseText.Seed,
            timing: NoiseText.Timing)) {
        Looks = [
            ("Nacho", new EffectLook<LensFlare, LensFlareParameter>.Whole(LensFlare.NachoLens)),
            ("Ring", new EffectLook<LensFlare, LensFlareParameter>.Whole(LensFlare.RingLens)),
            ("Hex", new EffectLook<LensFlare, LensFlareParameter>.Whole(LensFlare.HexLens)),
            ("Round", new EffectLook<LensFlare, LensFlareParameter>.Whole(LensFlare.RoundLens)),
        ],
    };
}

[Guid("99007c9c-22fe-4569-a242-ae2392a3a08d")]
[CustomPostEffect(PostEffectType.Late, Caption)]
public sealed class GradientLayer : StageEffect<GradientLayer, Gradient, GradientParameter, InvalidGenerator>, IEffectDefinition<Gradient, GradientParameter> {
    public const string Caption = "Gradient";

    public static EffectRow<Gradient, GradientParameter> Row { get; } = new(static parameter =>
        parameter.Map(
            kind: ParameterText.Choice<GradientKind, InvalidGenerator>(
                "Type", "Field the ramp runs over", static kind => kind.Map(linear: "Linear", radial: "Radial", angular: "Angular", box: "Box")),
            placement: GeneratorText.Placement,
            width: ParameterText.Of("Width", "Half width the ramp spans as a share of the frame's short side"),
            height: ParameterText.Of("Height", "Half height the ramp spans as a share of the frame's short side"),
            colors: ParameterText.Choice<RampInterpolation, InvalidPixelValue>("Colors", "Display colors and coverage along the ramp", ToneText.InterpolationText),
            layer: GeneratorText.Layer));
}

[Guid("cd4ad9d9-44cd-49d1-ad8f-a332aadd6d8d")]
[CustomPostEffect(PostEffectType.Late, Caption)]
public sealed class FourCornerGradient : StageEffect<FourCornerGradient, CornerGradient, CornerGradientParameter, InvalidGenerator>, IEffectDefinition<CornerGradient, CornerGradientParameter> {
    public const string Caption = "Four Corner Gradient";

    public static EffectRow<CornerGradient, CornerGradientParameter> Row { get; } = new(static parameter =>
        parameter.Map(
            topLeft: ParameterText.Of("Top Left", "Display color at the top left corner"),
            topRight: ParameterText.Of("Top Right", "Display color at the top right corner"),
            bottomRight: ParameterText.Of("Bottom Right", "Display color at the bottom right corner"),
            bottomLeft: ParameterText.Of("Bottom Left", "Display color at the bottom left corner"),
            layer: GeneratorText.Layer));
}

[Guid("45ae93a8-0400-486a-af24-b70e9f05598d")]
[CustomPostEffect(PostEffectType.Late, Caption)]
public sealed class Bands : StageEffect<Bands, Stripes, StripesParameter, InvalidGenerator>, IEffectDefinition<Stripes, StripesParameter> {
    public const string Caption = "Stripes";

    public static EffectRow<Stripes, StripesParameter> Row { get; } = new(static parameter =>
        parameter.Map(
            profile: ParameterText.Choice<WaveProfile, InvalidGenerator>("Profile", "Level across each stripe period", NoiseText.Profiles),
            placement: GeneratorText.Placement,
            wavelength: ParameterText.Of("Wavelength", "Period of the stripes as a share of the frame's short side"),
            bend: ParameterText.Of("Bend", "Fold of the stripes about the center line, 0 straight bands"),
            phase: ParameterText.Of("Phase", "Share of a period the stripes shift by"),
            speed: ParameterText.Of("Speed", "Periods per second the stripes scroll"),
            colors: ParameterText.Choice<RampInterpolation, InvalidPixelValue>("Colors", "Display colors and coverage across one period", ToneText.InterpolationText),
            layer: GeneratorText.Layer,
            timing: NoiseText.Timing));
}

[Guid("fc4fd80c-73d9-4184-bad1-a759b4182f60")]
[CustomPostEffect(PostEffectType.Late, Caption)]
public sealed class GridLines : StageEffect<GridLines, Grid, GridParameter, InvalidGenerator>, IEffectDefinition<Grid, GridParameter> {
    public const string Caption = "Grid";

    public static EffectRow<Grid, GridParameter> Row { get; } = new(static parameter =>
        parameter.Map(
            pattern: ParameterText.Choice<GridPattern, InvalidGenerator>(
                "Pattern", "Lines alone or filled cells", static pattern => pattern.Map(lines: "Lines", checker: "Checker", brick: "Brick")),
            placement: GeneratorText.Placement,
            cell: ParameterText.Of("Cell", "Cell height as a share of the frame's short side"),
            aspect: ParameterText.Of("Aspect", "Cell width over cell height"),
            offset: ParameterText.Of("Brick Offset", "Share of a cell every second brick row shifts by"),
            lineWidth: ParameterText.Of("Line Width", "Line width as a share of the frame's short side"),
            feather: GeneratorText.Feather,
            lineColor: ParameterText.Of("Line Color", "Display color of the lines"),
            cellColor: ParameterText.Of("Cell Color", "Display color of the cells"),
            alternateColor: ParameterText.Of("Alternate Color", "Display color of every other checker cell and the far end of the brick tints"),
            layer: GeneratorText.Layer,
            seed: NoiseText.Seed));
}

[Guid("e7c2ca0d-f654-4980-b540-2314d6c58de2")]
[CustomPostEffect(PostEffectType.Late, Caption)]
public sealed class ShapeLayer : StageEffect<ShapeLayer, Shapes, ShapesParameter, InvalidGenerator>, IEffectDefinition<Shapes, ShapesParameter> {
    public const string Caption = "Shape";

    public static EffectRow<Shapes, ShapesParameter> Row { get; } = new(static parameter =>
        parameter.Map(
            first: ParameterText.Of("First", "First figure of the combination") with {
                Parameters = ParameterText.Fields<ShapePrimitive, ShapePrimitiveParameter, InvalidGenerator>(PrimitiveText.Text),
            },
            second: ParameterText.Of("Second", "Second figure of the combination") with {
                Parameters = ParameterText.Fields<ShapePrimitive, ShapePrimitiveParameter, InvalidGenerator>(PrimitiveText.Text),
            },
            operation: ParameterText.Choice<ShapeOperation, InvalidGenerator>(
                "Operation", "Combination of the first shape with the second", "None",
                static operation => operation.Map(union: "Union", difference: "Difference", intersection: "Intersection", morph: "Morph")),
            smoothing: ParameterText.Of("Smoothing", "Blend width of the combination as a share of the frame's short side"),
            progress: ParameterText.Of("Progress", "Morph from the first shape at 0 to the second at 1"),
            style: GeneratorText.Style,
            layer: GeneratorText.Layer,
            seed: NoiseText.Seed));
}

[Guid("1f4355d5-07b5-417c-8ff7-4373e3efc668")]
[CustomPostEffect(PostEffectType.Late, Caption)]
public sealed class AspectBars : StageEffect<AspectBars, Letterbox, LetterboxParameter, InvalidGenerator>, IEffectDefinition<Letterbox, LetterboxParameter> {
    public const string Caption = "Letterbox";

    public static EffectRow<Letterbox, LetterboxParameter> Row { get; } = new(static parameter =>
        parameter.Map(
            aspect: ParameterText.Of("Aspect", "Width over height of the window the bars leave, off following the full frame"),
            inset: ParameterText.Of("Inset", "Margin inside the window as a share of the frame's short side"),
            radius: ParameterText.Of("Roundness", "Radius of the window's corners as a share of the frame's short side"),
            style: GeneratorText.Style,
            layer: GeneratorText.Layer,
            seed: NoiseText.Seed)) {
        Looks = [("Film Gate", new EffectLook<Letterbox, LetterboxParameter>.Whole(Letterbox.FilmGate))],
    };
}
