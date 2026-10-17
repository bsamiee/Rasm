using System.Runtime.InteropServices;
using Rasm.Imaging.Filters.Generators;
using Rasm.Rhino.Render.Effects;
using Rhino.Render.PostEffects;

namespace Darkroom.Effects;

// --- [COMPOSITION] ---------------------------------------------------------------------
[Guid("5bfb53d0-6c96-4fc2-bf47-fd7662e4182b")]
[CustomPostEffect(PostEffectType.Early, "Starfield")]
public sealed class NightSky : StageEffect<NightSky, Starfield, StarfieldParameter>;

[Guid("a65107ea-d4f1-4b61-ac09-b74d67ba8935")]
[CustomPostEffect(PostEffectType.Early, "Atmosphere")]
public sealed class Atmosphere : StageEffect<Atmosphere, Fog, FogParameter>, IEffectDefinition<Fog, FogParameter> {
    public static EffectRow<Fog, FogParameter> Row { get; } = new() { Looks = [("Haze", Fog.Haze)] };
}

[Guid("77743df8-86ad-48bc-b60a-a08a5dc51818")]
[CustomPostEffect(PostEffectType.Early, "Noise Texture")]
public sealed class NoiseLayer : StageEffect<NoiseLayer, NoiseTexture, NoiseTextureParameter>, IEffectDefinition<NoiseTexture, NoiseTextureParameter> {
    public static EffectRow<NoiseTexture, NoiseTextureParameter> Row { get; } = new() {
        Looks = [
            ("Turbulent Noise", NoiseTexture.TurbulentNoise), ("Block Noise", NoiseTexture.BlockNoise), ("Diamonds", NoiseTexture.Diamonds),
            ("Voronoi Noise", NoiseTexture.VoronoiNoise), ("Sparkle", NoiseTexture.Sparkle), ("Voronoi Cells", NoiseTexture.VoronoiCells),
            ("Waves", NoiseTexture.Waves), ("Light Leak", NoiseTexture.LightLeak), ("Flow", NoiseTexture.Flow),
            ("Vortex", NoiseTexture.Vortex), ("Smoke", NoiseTexture.Smoke), ("Mist", NoiseTexture.Mist),
        ],
    };
}

[Guid("36ac0f92-c557-4d37-a728-cb2249510efe")]
[CustomPostEffect(PostEffectType.Early, "Fire")]
public sealed class Flames : StageEffect<Flames, NoiseTexture, NoiseTextureParameter>, IEffectDefinition<NoiseTexture, NoiseTextureParameter> {
    public static EffectRow<NoiseTexture, NoiseTextureParameter> Row { get; } = new() { Default = NoiseTexture.Fire };
}

[Guid("31555b4a-ff97-4387-946f-e7e9b0a131e8")]
[CustomPostEffect(PostEffectType.Early, "Particles")]
public sealed class Particles : StageEffect<Particles, ParticleField, ParticleFieldParameter>, IEffectDefinition<ParticleField, ParticleFieldParameter> {
    public static EffectRow<ParticleField, ParticleFieldParameter> Row { get; } = new() {
        Looks = [
            ("Rain", ParticleField.Rain), ("Snow", ParticleField.Snow), ("Sparks", ParticleField.Sparks),
            ("Cell Rain", ParticleField.CellRain), ("Starfield", ParticleField.Starfield),
        ],
    };
}

[Guid("60ad6377-8592-43dd-acd2-cc3ef03f7b5f")]
[CustomPostEffect(PostEffectType.Early, "Lightning")]
public sealed class LightningBolt : StageEffect<LightningBolt, Lightning, LightningParameter>;

[Guid("ff29c5af-33af-4817-90a1-910fdbb21909")]
[CustomPostEffect(PostEffectType.Early, "Light Rays")]
public sealed class SunBeams : StageEffect<SunBeams, LightRays, LightRaysParameter>;

[Guid("05985670-b6cb-40da-9e8e-0f6ac2d30a70")]
[CustomPostEffect(PostEffectType.Early, "Lens Flare")]
public sealed class SourceFlare : StageEffect<SourceFlare, LensFlare, LensFlareParameter>, IEffectDefinition<LensFlare, LensFlareParameter> {
    public static EffectRow<LensFlare, LensFlareParameter> Row { get; } = new() {
        Looks = [("Nacho", LensFlare.NachoLens), ("Ring", LensFlare.RingLens), ("Hex", LensFlare.HexLens), ("Round", LensFlare.RoundLens)],
    };
}

[Guid("99007c9c-22fe-4569-a242-ae2392a3a08d")]
[CustomPostEffect(PostEffectType.Late, "Gradient")]
public sealed class GradientLayer : StageEffect<GradientLayer, Gradient, GradientParameter>;

[Guid("cd4ad9d9-44cd-49d1-ad8f-a332aadd6d8d")]
[CustomPostEffect(PostEffectType.Late, "Four Corner Gradient")]
public sealed class FourCornerGradient : StageEffect<FourCornerGradient, CornerGradient, CornerGradientParameter>;

[Guid("45ae93a8-0400-486a-af24-b70e9f05598d")]
[CustomPostEffect(PostEffectType.Late, "Stripes")]
public sealed class Bands : StageEffect<Bands, Stripes, StripesParameter>;

[Guid("fc4fd80c-73d9-4184-bad1-a759b4182f60")]
[CustomPostEffect(PostEffectType.Late, "Grid")]
public sealed class GridLines : StageEffect<GridLines, Grid, GridParameter>;

[Guid("e7c2ca0d-f654-4980-b540-2314d6c58de2")]
[CustomPostEffect(PostEffectType.Late, "Shape")]
public sealed class ShapeLayer : StageEffect<ShapeLayer, Shapes, ShapesParameter>;

[Guid("1f4355d5-07b5-417c-8ff7-4373e3efc668")]
[CustomPostEffect(PostEffectType.Late, "Letterbox")]
public sealed class AspectBars : StageEffect<AspectBars, Letterbox, LetterboxParameter>, IEffectDefinition<Letterbox, LetterboxParameter> {
    public static EffectRow<Letterbox, LetterboxParameter> Row { get; } = new() { Looks = [("Film Gate", Letterbox.FilmGate)] };
}
